using System.Collections.Generic;
using MultiWorldLib.Messaging.Definitions;

namespace MultiWorldLib.Messaging
{
    public class MWMessageDefinition<T> : IMWMessageDefinition where T : MWMessage
    {
        public List<IMWMessageProperty> Properties { private set; get; }
        public MWMessageType MessageType { get; private set; }

        public MWMessageDefinition() : this(MWMessageType.SharedCore) { }

        public MWMessageDefinition(MWMessageType type)
        {
            Properties = new List<IMWMessageProperty>();
            MessageType = type;
            Properties.Add(new MWMessageProperty<MWMessageType, T>(nameof(MWMessage.MessageType)));
            Properties.Add(new MWMessageProperty<ulong, T>(nameof(MWMessage.SenderUid)));
            Properties.Add(new MWMessageProperty<ulong, T>(nameof(MWMessage.MessageId)));
        }
    }
}
