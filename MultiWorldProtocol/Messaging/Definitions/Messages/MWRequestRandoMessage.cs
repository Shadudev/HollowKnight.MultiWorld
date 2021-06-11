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

    public class RequestRandoMessageDefinition : MWMessageDefinition<MWRequestRandoMessage>
    {
        public RequestRandoMessageDefinition() : base(MWMessageType.RequestRandoMessage)
        {
        }
    }
}
