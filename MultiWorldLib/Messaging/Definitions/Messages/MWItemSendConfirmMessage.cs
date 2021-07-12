namespace MultiWorldLib.Messaging.Definitions.Messages
{
    [MWMessageType(MWMessageType.ItemSendConfirmMessage)]
    public class MWItemSendConfirmMessage : MWMessage
    {
        public string Location { get; set; }
        public string Item { get; set; }
        public int To { get; set; }

        public MWItemSendConfirmMessage()
        {
            MessageType = MWMessageType.ItemSendConfirmMessage;
        }
    }

    public class MWItemSendConfirmDefinition : MWMessageDefinition<MWItemSendConfirmMessage>
    {
        public MWItemSendConfirmDefinition() : base(MWMessageType.ItemSendConfirmMessage)
        {
            Properties.Add(new MWMessageProperty<string, MWItemSendConfirmMessage>(nameof(MWItemSendConfirmMessage.Location)));
            Properties.Add(new MWMessageProperty<string, MWItemSendConfirmMessage>(nameof(MWItemSendConfirmMessage.Item)));
            Properties.Add(new MWMessageProperty<int, MWItemSendConfirmMessage>(nameof(MWItemSendConfirmMessage.To)));
        }
    }
}
