namespace MultiWorldLib.Messaging.Definitions.Messages
{
    [MWMessageType(MWMessageType.ReadyConfirmMessage)]
    public class MWReadyConfirmMessage : MWMessage
    {
        public int Ready { get; set; }
        public string Names { get; set; }

        public MWReadyConfirmMessage()
        {
            MessageType = MWMessageType.ReadyConfirmMessage;
        }
    }

    public class MWReadyConfirmMessageDefinition : MWMessageDefinition<MWReadyConfirmMessage>
    {
        public MWReadyConfirmMessageDefinition() : base(MWMessageType.ReadyConfirmMessage)
        {
            Properties.Add(new MWMessageProperty<int, MWReadyConfirmMessage>(nameof(MWReadyConfirmMessage.Ready)));
            Properties.Add(new MWMessageProperty<string, MWReadyConfirmMessage>(nameof(MWReadyConfirmMessage.Names)));
        }
    }
}
