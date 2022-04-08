namespace MultiWorldLib.Messaging.Definitions.Messages
{
    [MWMessageType(MWMessageType.ItemsReceiveConfirmMessage)]
    public class MWItemsReceiveConfirmMessage : MWMessage, IConfirmMessage
    {
        public int Count { get; set; }
        public string From { get; set; }

        public MWItemsReceiveConfirmMessage()
        {
            MessageType = MWMessageType.ItemsReceiveConfirmMessage;
        }

        public bool Confirms(MWConfirmableMessage message)
        {
            if (message.MessageType != MWMessageType.ItemsReceiveMessage)
                return false;

            MWItemsReceiveMessage itemsReceiveMessage = (MWItemsReceiveMessage)message;
            return itemsReceiveMessage.From == From && itemsReceiveMessage.Items.Count == Count;
        }
    }

    public class MWItemsReceiveConfirmDefinition : MWMessageDefinition<MWItemsReceiveConfirmMessage>
    {
        public MWItemsReceiveConfirmDefinition() : base(MWMessageType.ItemsReceiveConfirmMessage)
        {
            Properties.Add(new MWMessageProperty<int, MWItemsReceiveConfirmMessage>(nameof(MWItemsReceiveConfirmMessage.Count)));
            Properties.Add(new MWMessageProperty<string, MWItemsReceiveConfirmMessage>(nameof(MWItemsReceiveConfirmMessage.From)));
        }
    }
}
