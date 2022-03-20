namespace MultiWorldLib.Messaging.Definitions.Messages
{
    [MWMessageType(MWMessageType.ItemsSendMessage)]
    public class MWItemsSendMessage : MWConfirmableMessage
    {
        public List<(int, string)> Items { get; set; }

        public MWItemsSendMessage()
        {
            MessageType = MWMessageType.ItemsSendMessage;
        }
    }

    public class MWItemsSendDefinition : MWMessageDefinition<MWItemsSendMessage>
    {
        public MWItemsSendDefinition() : base(MWMessageType.ItemsSendMessage)
        {
            Properties.Add(new MWMessageProperty<List<(int, string)>, MWItemsSendMessage>(nameof(MWItemsSendMessage.Items)));
        }
    }
}
