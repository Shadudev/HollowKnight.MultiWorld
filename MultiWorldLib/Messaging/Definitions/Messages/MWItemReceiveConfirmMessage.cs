namespace MultiWorldLib.Messaging.Definitions.Messages
{
    [MWMessageType(MWMessageType.ItemReceiveConfirmMessage)]
    public class MWItemReceiveConfirmMessage : MWMessage, IConfirmMessage
    {
        public Item Item { get; set; }
        public string From { get; set; }

        public MWItemReceiveConfirmMessage()
        {
            MessageType = MWMessageType.ItemReceiveConfirmMessage;
        }

        public bool Confirms(MWConfirmableMessage message)
        {
            if (message.MessageType != MWMessageType.ItemReceiveMessage)
                return false;

            MWItemReceiveMessage msg = (MWItemReceiveMessage)message;
            return msg.Item == Item && msg.From == From;
        }
    }

    public class MWItemReceiveConfirmDefinition : MWMessageDefinition<MWItemReceiveConfirmMessage>
    {
        public MWItemReceiveConfirmDefinition() : base(MWMessageType.ItemReceiveConfirmMessage)
        {
            Properties.Add(new MWMessageProperty<Item, MWItemReceiveConfirmMessage>(nameof(MWItemReceiveConfirmMessage.Item)));
            Properties.Add(new MWMessageProperty<string, MWItemReceiveConfirmMessage>(nameof(MWItemReceiveConfirmMessage.From)));
        }
    }
}