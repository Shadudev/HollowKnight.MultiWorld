namespace MultiWorldLib.Messaging.Definitions.Messages
{
    [MWMessageType(MWMessageType.ResultMessage)]
    public class MWResultMessage : MWMessage
    {
        public RandoResult Result { get; set; }

        public MWResultMessage()
        {
            MessageType = MWMessageType.ResultMessage;
        }
    }

    public class MWResultMessageDefinition : MWMessageDefinition<MWResultMessage>
    {
        public MWResultMessageDefinition() : base(MWMessageType.ResultMessage)
        {
            Properties.Add(new MWMessageProperty<RandoResult, MWResultMessage>(nameof(MWResultMessage.Result)));
        }
    }
}
