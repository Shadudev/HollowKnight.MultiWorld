namespace MultiWorldLib.Messaging.Definitions.Messages
{
    [MWMessageType(MWMessageType.ItemSendMessage)]
    public class MWItemSendMessage : MWConfirmableMessage
    {
        public string Item { get; set; }
        public int To { get; set; }

        public MWItemSendMessage()
        {
            MessageType = MWMessageType.ItemSendMessage;
        }
    }

    public class MWItemSendDefinition : MWMessageDefinition<MWItemSendMessage>
    {
        public MWItemSendDefinition() : base(MWMessageType.ItemSendMessage)
        {
            Properties.Add(new MWMessageProperty<string, MWItemSendMessage>(nameof(MWItemSendMessage.Item)));
            Properties.Add(new MWMessageProperty<int, MWItemSendMessage>(nameof(MWItemSendMessage.To)));
        }
    }
}
