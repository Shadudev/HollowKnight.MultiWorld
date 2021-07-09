namespace MultiWorldLib.Messaging.Definitions.Messages
{
    [MWMessageType(MWMessageType.RequestRandoMessage)]
    public class MWRequestRandoMessage : MWMessage
    {
        public MWRequestRandoMessage()
        {
            MessageType = MWMessageType.RequestRandoMessage;
        }
    }

    public class MWRequestRandoMessageDefinition : MWMessageDefinition<MWRequestRandoMessage>
    {
        public MWRequestRandoMessageDefinition() : base(MWMessageType.RequestRandoMessage)
        {
        }
    }
}
