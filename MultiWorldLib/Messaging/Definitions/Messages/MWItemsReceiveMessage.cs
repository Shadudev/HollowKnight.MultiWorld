namespace MultiWorldLib.Messaging.Definitions.Messages
{
    [MWMessageType(MWMessageType.ItemsReceiveMessage)]
    public class MWItemsReceiveMessage : MWConfirmableMessage
    {
        public List<Item> Items { get; set; }
        public string From { get; set; }

        public MWItemsReceiveMessage()
        {
            MessageType = MWMessageType.ItemsReceiveMessage;
        }
    }

    public class MWItemsReceiveDefinition : MWMessageDefinition<MWItemsReceiveMessage>
    {
        public MWItemsReceiveDefinition() : base(MWMessageType.ItemsReceiveMessage)
        {
            Properties.Add(new MWMessageProperty<List<Item>, MWItemsReceiveMessage>(nameof(MWItemsReceiveMessage.Items)));
            Properties.Add(new MWMessageProperty<string, MWItemsReceiveMessage>(nameof(MWItemsReceiveMessage.From)));
        }
    }
}
