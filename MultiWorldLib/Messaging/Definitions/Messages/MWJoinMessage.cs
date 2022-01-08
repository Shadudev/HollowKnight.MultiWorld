namespace MultiWorldLib.Messaging.Definitions.Messages
{
    [MWMessageType(MWMessageType.JoinMessage)]
    public class MWJoinMessage : MWMessage
    {
        public string DisplayName { get; set; }
        public int RandoId { get; set; }
        public int PlayerId { get; set; }
        public Mode Mode { get; set; }

        public MWJoinMessage()
        {
            MessageType = MWMessageType.JoinMessage;
        }
    }

    public class MWJoinMessageDefinition : MWMessageDefinition<MWJoinMessage>
    {
        public MWJoinMessageDefinition() : base(MWMessageType.JoinMessage)
        {
            Properties.Add(new MWMessageProperty<string, MWJoinMessage>(nameof(MWJoinMessage.DisplayName)));
            Properties.Add(new MWMessageProperty<int, MWJoinMessage>(nameof(MWJoinMessage.RandoId)));
            Properties.Add(new MWMessageProperty<int, MWJoinMessage>(nameof(MWJoinMessage.PlayerId)));
            Properties.Add(new MWMessageProperty<Mode, MWJoinMessage>(nameof(MWJoinMessage.Mode)));
        }
    }
}
