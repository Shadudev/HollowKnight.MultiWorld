namespace MultiWorldLib.Messaging.Definitions.Messages
{
    [MWMessageType(MWMessageType.ItemSendConfirmMessage)]
    public class MWItemSendConfirmMessage : MWMessage, IConfirmMessage
    {
        public string Item { get; set; }
        public int To { get; set; }

        public MWItemSendConfirmMessage()
        {
            MessageType = MWMessageType.ItemSendConfirmMessage;
        }

        public bool Confirms(MWConfirmableMessage message)
        {
            if (message is not MWItemSendMessage)
            {
                return false;
            }

            MWItemSendMessage itemSendMessage = (MWItemSendMessage)message;
            return itemSendMessage.To == To && itemSendMessage.Item == Item;
        }
    }

    public class MWItemSendConfirmDefinition : MWMessageDefinition<MWItemSendConfirmMessage>
    {
        public MWItemSendConfirmDefinition() : base(MWMessageType.ItemSendConfirmMessage)
        {
            Properties.Add(new MWMessageProperty<string, MWItemSendConfirmMessage>(nameof(MWItemSendConfirmMessage.Item)));
            Properties.Add(new MWMessageProperty<int, MWItemSendConfirmMessage>(nameof(MWItemSendConfirmMessage.To)));
        }
    }
}
