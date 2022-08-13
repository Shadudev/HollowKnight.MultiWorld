namespace MultiWorldLib.Messaging.Definitions.Messages
{
    [MWMessageType(MWMessageType.DataSendMessage)]
    public class MWDataSendMessage : MWConfirmableMessage
    {
        public string Label { get; set; }
        public string Data { get; set; }
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
            Properties.Add(new MWMessageProperty<string, MWDataSendMessage>(nameof(MWDataSendMessage.Data)));
            Properties.Add(new MWMessageProperty<int, MWDataSendMessage>(nameof(MWDataSendMessage.To)));
        }
    }
}
