namespace MultiWorldLib.Messaging.Definitions.Messages
{
    [MWMessageType(MWMessageType.DataSendMessage)]
    public class MWDataSendMessage : MWConfirmableMessage
    {
        public string Label { get; set; }
        public string Content { get; set; }
        public int To { get; set; }

        public MWDataSendMessage()
        {
            MessageType = MWMessageType.DataSendMessage;
        }
    }

    public class MWDataSendDefinition : MWMessageDefinition<MWDataSendMessage>
    {
        public MWDataSendDefinition() : base(MWMessageType.DataSendMessage)
        {
            Properties.Add(new MWMessageProperty<string, MWDataSendMessage>(nameof(MWDataSendMessage.Label)));
            Properties.Add(new MWMessageProperty<string, MWDataSendMessage>(nameof(MWDataSendMessage.Content)));
            Properties.Add(new MWMessageProperty<int, MWDataSendMessage>(nameof(MWDataSendMessage.To)));
        }
    }
}
