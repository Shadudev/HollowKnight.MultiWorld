namespace MultiWorldLib.Messaging.Definitions.Messages
{
    [MWMessageType(MWMessageType.DataReceiveMessage)]
    public class MWDataReceiveMessage : MWConfirmableMessage
    {
        public string Label { get; set; }
        public string Content { get; set; }
        public string From { get; set; }

        public MWDataReceiveMessage()
        {
            MessageType = MWMessageType.DataReceiveMessage;
        }
    }

    public class MWDataReceiveDefinition : MWMessageDefinition<MWDataReceiveMessage>
    {
        public MWDataReceiveDefinition() : base(MWMessageType.DataReceiveMessage)
        {
            Properties.Add(new MWMessageProperty<string, MWDataReceiveMessage>(nameof(MWDataReceiveMessage.Label)));
            Properties.Add(new MWMessageProperty<string, MWDataReceiveMessage>(nameof(MWDataReceiveMessage.Content)));
            Properties.Add(new MWMessageProperty<string, MWDataReceiveMessage>(nameof(MWDataReceiveMessage.From)));
        }
    }
}
