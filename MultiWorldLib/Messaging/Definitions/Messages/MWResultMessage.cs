namespace MultiWorldLib.Messaging.Definitions.Messages
{
    [MWMessageType(MWMessageType.ResultMessage)]
    public class MWResultMessage : MWMessage
    {
        public (int, string, string)[] Items { get; set; }
        public int RandoId { get; set; }

        public MWResultMessage()
        {
            MessageType = MWMessageType.ResultMessage;
        }
    }

    public class MWResultMessageDefinition : MWMessageDefinition<MWResultMessage>
    {
        public MWResultMessageDefinition() : base(MWMessageType.ResultMessage)
        {
            Properties.Add(new MWMessageProperty<(int, string, string)[], MWResultMessage>(nameof(MWResultMessage.Items)));
            Properties.Add(new MWMessageProperty<int, MWResultMessage>(nameof(MWResultMessage.RandoId)));
        }
    }
}
