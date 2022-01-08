using System;
using System.IO;
using Newtonsoft.Json;
using MultiWorldLib.Messaging;
using MultiWorldLib.Messaging.Definitions;
using MultiWorldLib.Messaging.Definitions.Messages;

namespace MultiWorldLib.Binary
{
    public class BinaryMWMessageEncoder : IMWMessageEncoder
    {
        public void Encode(BinaryWriter dataStream, IMWMessageProperty property, MWMessage message)
        {
            if (property.Type == typeof(MWMessageType))
            {
                dataStream.Write((int)(MWMessageType)property.GetValue(message));
                return;
            } else if (property.Type == typeof(Mode))
            {
                dataStream.Write((byte)(Mode)property.GetValue(message));
                return;
            }

            object val = property.GetValue(message);

            switch (val)
            {
                case ulong ul:
                    dataStream.Write(ul);
                    break;
                case uint ui:
                    dataStream.Write(ui);
                    break;
                case ushort us:
                    dataStream.Write(us);
                    break;
                case byte b:
                    dataStream.Write(b);
                    break;
                case sbyte sb:
                    dataStream.Write(sb);
                    break;
                case long l:
                    dataStream.Write(l);
                    break;
                case int i:
                    dataStream.Write(i);
                    break;
                case short s:
                    dataStream.Write(s);
                    break;
                case string str:
                    dataStream.Write(str);
                    break;
                case object obj when obj.GetType().IsSerializable:
                    dataStream.Write(JsonConvert.SerializeObject(obj));
                    break;
                default:
                    throw new InvalidOperationException($"Unhandled type in {nameof(Encode)}: {val?.GetType().Name}");
            }
        }

        public void Decode(BinaryReader dataStream, IMWMessageProperty property, MWMessage message)
        {
            object val = null;

            if (property.Type == typeof(MWMessageType))
            {
                val = (MWMessageType)dataStream.ReadInt32();
                property.SetValue(message, val);
                return;
            }
            if (property.Type == typeof(Mode))
            {
                val = ((Mode)dataStream.ReadByte());
                property.SetValue(message, val);
                return;
            }

            switch (Type.GetTypeCode(property.Type))
            {
                case TypeCode.UInt64:
                    val = dataStream.ReadUInt64();
                    break;
                case TypeCode.UInt32:
                    val = dataStream.ReadUInt32();
                    break;
                case TypeCode.UInt16:
                    val = dataStream.ReadUInt16();
                    break;
                case TypeCode.Byte:
                    val = dataStream.ReadByte();
                    break;
                case TypeCode.Int64:
                    val = dataStream.ReadInt64();
                    break;
                case TypeCode.Int32:
                    val = dataStream.ReadInt32();
                    break;
                case TypeCode.Int16:
                    val = dataStream.ReadInt16();
                    break;
                case TypeCode.String:
                    val = dataStream.ReadString();
                    break;
                default:
                    if (property.Type.IsSerializable)
                    {
                        val = JsonConvert.DeserializeObject(dataStream.ReadString(), property.Type);
                        break;
                    }
                    throw new InvalidOperationException($"Unhandled type in {nameof(Decode)}: {property.Type.Name}");
            }
            property.SetValue(message, val);
        }
    }
}
