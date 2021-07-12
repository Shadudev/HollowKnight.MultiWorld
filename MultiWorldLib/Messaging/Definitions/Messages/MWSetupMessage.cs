namespace MultiWorldLib.Messaging.Definitions.Messages
{
    [MWMessageType(MWMessageType.SetupMessage)]
    public class MWSetupMessage : MWMessage
    {
        public MWSetupMessage()
        {
            MessageType = MWMessageType.SetupMessage;
        }
    }

    public class MWSetupMessageDefinition : MWMessageDefinition<MWConnectMessage>
    {
        public MWSetupMessageDefinition() : base(MWMessageType.SetupMessage)
        {
        }
    }
}
