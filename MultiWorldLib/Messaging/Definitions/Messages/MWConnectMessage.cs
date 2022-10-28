namespace MultiWorldLib.Messaging.Definitions.Messages
{
    [MWMessageType(MWMessageType.ConnectMessage)]
    public class MWConnectMessage : MWMessage
    {
        public string ServerName { get; set; } = string.Empty;

        public MWConnectMessage()
        {
            MessageType = MWMessageType.ConnectMessage;
        }

    }

    public class MWConnectMessageDefinition : MWMessageDefinition<MWConnectMessage>
    {
        public MWConnectMessageDefinition() : base(MWMessageType.ConnectMessage)
        {
            Properties.Add(new MWMessageProperty<string, MWConnectMessage>(nameof(MWConnectMessage.ServerName)));
        }
    }
}