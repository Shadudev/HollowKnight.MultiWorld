namespace MultiWorldLib.Messaging.Definitions.Messages
{
    [MWMessageType(MWMessageType.ItemReceiveMessage)]
    public class MWItemReceiveMessage : MWConfirmableMessage
    {
        public Item Item { get; set; }
        public string From { get; set; }

        public MWItemReceiveMessage()
        {
            MessageType = MWMessageType.ItemReceiveMessage;
        }
    }

    public class MWItemReceiveDefinition : MWMessageDefinition<MWItemReceiveMessage>
    {
        public MWItemReceiveDefinition() : base(MWMessageType.ItemReceiveMessage)
        {
            Properties.Add(new MWMessageProperty<Item, MWItemReceiveMessage>(nameof(MWItemReceiveMessage.Item)));
            Properties.Add(new MWMessageProperty<string, MWItemReceiveMessage>(nameof(MWItemReceiveMessage.From)));
        }
    }
}
