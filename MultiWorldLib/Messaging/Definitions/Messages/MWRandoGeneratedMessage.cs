namespace MultiWorldLib.Messaging.Definitions.Messages
{
    [MWMessageType(MWMessageType.RandoGeneratedMessage)]
    public class MWRandoGeneratedMessage : MWMessage
    {
        public Dictionary<string, (string, string)[]> Items { get; set; }
        public int Seed { get; set; }

        public MWRandoGeneratedMessage()
        {
            MessageType = MWMessageType.RandoGeneratedMessage;
        }
    }

    public class MWRandoGeneratedMessageDefinition : MWMessageDefinition<MWRandoGeneratedMessage>
    {
        public MWRandoGeneratedMessageDefinition() : base(MWMessageType.RandoGeneratedMessage)
        {
            Properties.Add(new MWMessageProperty<Dictionary<string, (string, string)[]>, MWRandoGeneratedMessage>(nameof(MWRandoGeneratedMessage.Items)));
            Properties.Add(new MWMessageProperty<int, MWRandoGeneratedMessage>(nameof(MWRandoGeneratedMessage.Seed)));
        }
    }
}
