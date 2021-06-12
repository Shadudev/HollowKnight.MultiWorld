namespace MultiWorldLib.Messaging.Definitions.Messages
{
    [MWMessageType(MWMessageType.InitiateGameMessage)]
    public class MWInitiateGameMessage: MWMessage
    {
        public MWInitiateGameMessage()
        {
            MessageType = MWMessageType.InitiateGameMessage;
        }

        public int Seed { get; set; }
    }

    public class MWMWInitiateGameDefinition : MWMessageDefinition<MWInitiateGameMessage>
    {
        public MWMWInitiateGameDefinition() : base(MWMessageType.InitiateGameMessage) 
        {
            Properties.Add(new MWMessageProperty<int, MWInitiateGameMessage>(nameof(MWInitiateGameMessage.Seed)));
        }
    }
}
