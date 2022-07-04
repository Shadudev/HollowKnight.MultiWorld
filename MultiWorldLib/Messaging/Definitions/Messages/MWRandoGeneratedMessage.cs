namespace MultiWorldLib.Messaging.Definitions.Messages
{
    [MWMessageType(MWMessageType.RandoGeneratedMessage)]
    public class MWRandoGeneratedMessage : MWMessage
    {
        public Placement[] Placements { get; set; }

        public MWRandoGeneratedMessage()
        {
            MessageType = MWMessageType.RandoGeneratedMessage;
        }
    }

    public class MWRandoGeneratedMessageDefinition : MWMessageDefinition<MWRandoGeneratedMessage>
    {
        public MWRandoGeneratedMessageDefinition() : base(MWMessageType.RandoGeneratedMessage)
        {
            Properties.Add(new MWMessageProperty<Placement[], MWRandoGeneratedMessage>(nameof(MWRandoGeneratedMessage.Placements)));
        }
    }
}
