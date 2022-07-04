namespace MultiWorldLib.Messaging.Definitions.Messages
{
    [MWMessageType(MWMessageType.ItemSendConfirmMessage)]
    public class MWItemSendConfirmMessage : MWMessage, IConfirmMessage
    {
        public Item Item { get; set; }

        public MWItemSendConfirmMessage()
        {
            MessageType = MWMessageType.ItemSendConfirmMessage;
        }

        public bool Confirms(MWConfirmableMessage message)
        {
            if (message.MessageType != MWMessageType.ItemSendMessage)
            {
                return false;
            }

            MWItemSendMessage itemSendMessage = (MWItemSendMessage)message;
            return itemSendMessage.Item == Item;
        }
    }

    public class MWItemSendConfirmDefinition : MWMessageDefinition<MWItemSendConfirmMessage>
    {
        public MWItemSendConfirmDefinition() : base(MWMessageType.ItemSendConfirmMessage)
        {
            Properties.Add(new MWMessageProperty<Item, MWItemSendConfirmMessage>(nameof(MWItemSendConfirmMessage.Item)));
        }
    }
}
