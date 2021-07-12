using System;

namespace MultiWorldLib.Messaging.Definitions.Messages
{
    [MWMessageType(MWMessageType.PingMessage)]
    public class MWPingMessage : MWMessage
    {
        public UInt32 RepVal { get; set; }

        public MWPingMessage()
        {
            MessageType = MWMessageType.PingMessage;
        }
    }

    public class MWPingMessageDefinition : MWMessageDefinition<MWPingMessage>
    {
        public MWPingMessageDefinition() : base(MWMessageType.PingMessage)
        {
            Properties.Add(new MWMessageProperty<UInt32, MWPingMessage>(nameof(MWPingMessage.RepVal)));
        }
    }
}