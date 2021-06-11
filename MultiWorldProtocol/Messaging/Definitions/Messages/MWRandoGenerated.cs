namespace MultiWorldLib.Messaging.Definitions.Messages
{
    [MWMessageType(MWMessageType.RandoGeneratedMessage)]
    public class MWRandoGenerated : MWMessage
    {
        public MWRandoGenerated()
        {
            MessageType = MWMessageType.RandoGeneratedMessage;
        }
    }

    public class MWStartMessageDefinition : MWMessageDefinition<MWConnectMessage>
    {
        public MWStartMessageDefinition() : base(MWMessageType.RandoGeneratedMessage)
        {
        }
    }
}
