using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using MultiWorldLib.Messaging.Definitions;

namespace MultiWorldLib.Messaging
{
    public class MWMessagePacker
    {
        static Dictionary<MWMessageType, IMWMessageDefinition> definitionLookup = new Dictionary<MWMessageType, IMWMessageDefinition>();
        static Dictionary<MWMessageType, ConstructorInfo> messageConstructors = new Dictionary<MWMessageType, ConstructorInfo>();
        static object[] _dummyParams = new object[0]; //Empty array of objects to pass to parameterless message constructors

        IMWMessageEncoder encoder;
        //Create a Memory stream in RAM that can be used to pack messages and then copy them into a freshly allocated buffer after the fact
        //Safes us from having to precalculate message size at the cost of a little efficiency due to the memory copy, but with the benefit of less computation
        //And optimal packing
        MemoryStream backingStream;
        BinaryWriter streamWriter;

        MWSharedCore _dummy; //Used to save allocations when unpacking the first part of a message to work out the type

        static MWMessagePacker()
        {
            //Build lookup tables
            var assembly = Assembly.GetAssembly(typeof(MWMessagePacker));
            var types = assembly.GetTypes();
            foreach(Type t in types)
            {
                //If it implements the message definition interface
                if(typeof(IMWMessageDefinition).IsAssignableFrom(t) && !t.IsInterface && !t.ContainsGenericParameters)
                {
                    //Message Definitions need to implement empty Constructors
                    var constructor = t.GetConstructor(new Type[] { });

                    if (constructor == null)
                    {
                        throw new InvalidOperationException($"Type {t.Name} has no public parameterless constructor");
                    }

                    IMWMessageDefinition def = (IMWMessageDefinition) constructor.Invoke(_dummyParams);
                    definitionLookup.Add(def.MessageType, def);
                }
                //If it inherited from MWMessage we want the constructor
                else if(typeof(MWMessage).IsAssignableFrom(t) && !t.IsInterface && !t.ContainsGenericParameters &&
                    !typeof(MWConfirmableMessage).Equals(t))
                {
                    if (t == typeof(MWMessage))
                    {
                        continue;
                    }

                    var constructor = t.GetConstructor(new Type[] { });
                    var attributes = t.GetCustomAttributes(false);
                    MWMessageType type = MWMessageType.InvalidMessage;
                    for(int i=0; i<attributes.Length; i++)
                    {
                        if (attributes[i] is MWMessageTypeAttribute attribute)
                        {
                            type = attribute.Type;
                            break;
                        }
                    }

                    if (type == MWMessageType.InvalidMessage)
                    {
                        throw new InvalidOperationException($"Message type {t.Name} is invalid");
                    }

                    MWMessageType messageType = type;
                    messageConstructors.Add(messageType, constructor);
                }
            }
        }

        public MWMessagePacker(IMWMessageEncoder encoder)
        {
            this.encoder = encoder;
            backingStream = new MemoryStream(0x4000); //Should be large enough to accomodate any message we might send, if not can easily be changed
            streamWriter = new BinaryWriter(backingStream);
            _dummy = new MWSharedCore();
        }

        public MWPackedMessage Pack(MWMessage message)
        {
            //Reset packing stream to 4 skipping the size field, can update that at the end
            backingStream.Seek(4, SeekOrigin.Begin);
            //Get definition and go through all properties writing them out to the stream
            var definition = definitionLookup[message.MessageType];
            for(int i=0; i<definition.Properties.Count; i++)
            {
                var property = definition.Properties[i];
                encoder.Encode(streamWriter, property, message);
            }

            //Update length in the message
            uint length = (uint)backingStream.Position;
            backingStream.Seek(0, SeekOrigin.Begin);
            streamWriter.Write(length);
            backingStream.Seek(0, SeekOrigin.Begin);

            //Create and copy to dedicated buffer
            var buffer = new byte[length];
            Array.Copy(backingStream.GetBuffer(), buffer, buffer.Length);

            return new MWPackedMessage(length, buffer);
        }

        public MWMessage Unpack(MWPackedMessage message)
        {
            using (MemoryStream bufferStream = new MemoryStream(message.Buffer))
            {
                using (BinaryReader reader = new BinaryReader(bufferStream))
                {
                    //Skip over size field, that's not relevant for message content
                    bufferStream.Seek(4, SeekOrigin.Begin);
                    //Parse the beginning of the message generically to work out the messagetype
                    //Parsing into dummy to save ourselves allocating a new dummy object every time
                    var definition = definitionLookup[MWMessageType.SharedCore];
                    for (int i = 0; i < definition.Properties.Count; i++)
                    {
                        var property = definition.Properties[i];
                        encoder.Decode(reader, property, _dummy);
                    }
                    //Seek back to 
                    bufferStream.Seek(4, SeekOrigin.Begin);
                    return Unpack(reader, _dummy.MessageType);
                }
            }
        }

        private MWMessage Unpack(BinaryReader reader, MWMessageType type)
        {
            try
            {
                var definition = definitionLookup[type];
                MWMessage result = (MWMessage)messageConstructors[type].Invoke(_dummyParams);
                for (int i = 0; i < definition.Properties.Count; i++)
                {
                    var property = definition.Properties[i];
                    encoder.Decode(reader, property, result);
                }
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception("Threw for " + type, ex);
            }
        }
    }
}
