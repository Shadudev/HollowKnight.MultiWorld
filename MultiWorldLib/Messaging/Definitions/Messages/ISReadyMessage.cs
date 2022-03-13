namespace MultiWorldLib.Messaging.Definitions.Messages
{
    [MWMessageType(MWMessageType.ISReadyMessage)]
    public class ISReadyMessage : MWMessage
    {
        public string Room { get; set; }
        public string Nickname { get; set; }
        public int Hash { get; set; }
        public ISReadyMessage()
        {
            MessageType = MWMessageType.ISReadyMessage;
        }
    }

    public class ISReadyMessageDefinition : MWMessageDefinition<ISReadyMessage>
    {
        public ISReadyMessageDefinition() : base(MWMessageType.ISReadyMessage)
        {
            Properties.Add(new MWMessageProperty<string, ISReadyMessage>(nameof(ISReadyMessage.Room)));
            Properties.Add(new MWMessageProperty<string, ISReadyMessage>(nameof(ISReadyMessage.Nickname)));
            Properties.Add(new MWMessageProperty<Mode, ISReadyMessage>(nameof(ISReadyMessage.Hash)));
        }
    }
}
