namespace MultiWorldLib.Messaging.Definitions.Messages
{
    [MWMessageType(MWMessageType.RandoGeneratedMessage)]
    public class MWRandoGeneratedMessage : MWMessage
    {
        public (int, string, string)[] Items { get; set; }

        public MWRandoGeneratedMessage()
        {
            MessageType = MWMessageType.RandoGeneratedMessage;
        }
    }

    public class MWRandoGeneratedMessageDefinition : MWMessageDefinition<MWRandoGeneratedMessage>
    {
        public MWRandoGeneratedMessageDefinition() : base(MWMessageType.RandoGeneratedMessage)
        {
            Properties.Add(new MWMessageProperty<(int, string, string)[], MWRandoGeneratedMessage>(nameof(MWRandoGeneratedMessage.Items)));
        }
    }
}
