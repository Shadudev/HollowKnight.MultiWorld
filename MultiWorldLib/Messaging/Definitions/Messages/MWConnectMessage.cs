namespace MultiWorldLib.Messaging.Definitions.Messages
{
    [MWMessageType(MWMessageType.ConnectMessage)]
    public class MWConnectMessage : MWMessage
    {
        public MWConnectMessage()
        {
            MessageType = MWMessageType.ConnectMessage;
        }
    }

    public class MWConnectMessageDefinition : MWMessageDefinition<MWConnectMessage>
    {
        public MWConnectMessageDefinition() : base(MWMessageType.ConnectMessage)
        {
        }
    }
}