namespace MultiWorldLib.Messaging.Definitions.Messages
{
    [MWMessageType(MWMessageType.ItemsSendConfirmMessage)]
    public class MWItemsSendConfirmMessage : MWMessage
    {
        public int ItemsCount { get; set; }

        public MWItemsSendConfirmMessage()
        {
            MessageType = MWMessageType.ItemsSendConfirmMessage;
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
