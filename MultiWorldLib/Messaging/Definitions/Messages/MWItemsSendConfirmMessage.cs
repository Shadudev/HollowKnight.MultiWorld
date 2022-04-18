namespace MultiWorldLib.Messaging.Definitions.Messages
{
    [MWMessageType(MWMessageType.ItemsSendConfirmMessage)]
    public class MWItemsSendConfirmMessage : MWMessage, IConfirmMessage
    {
        public int ItemsCount { get; set; }

        public MWItemsSendConfirmMessage()
        {
            MessageType = MWMessageType.ItemsSendConfirmMessage;
        }

        public bool Confirms(MWConfirmableMessage message)
        {
            if (message.MessageType != MWMessageType.ItemsSendMessage)
                return false;

            return ((MWItemsSendMessage)message).Items.Count == ItemsCount;
        }
    }

    public class MWItemsSendConfirmDefinition : MWMessageDefinition<MWItemsSendConfirmMessage>
    {
        public MWItemsSendConfirmDefinition() : base(MWMessageType.ItemsSendConfirmMessage)
        {
            Properties.Add(new MWMessageProperty<int, MWItemsSendConfirmMessage>(nameof(MWItemsSendConfirmMessage.ItemsCount)));
        }
    }
}
