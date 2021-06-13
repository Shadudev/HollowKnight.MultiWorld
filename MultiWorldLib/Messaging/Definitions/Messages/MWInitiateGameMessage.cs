namespace MultiWorldLib.Messaging.Definitions.Messages
{
    [MWMessageType(MWMessageType.InitiateGameMessage)]
    public class MWInitiateGameMessage: MWMessage
    {
        public int Seed { get; set; }
        public int ReadyID { get; set; }

        public MWInitiateGameMessage()
        {
            MessageType = MWMessageType.InitiateGameMessage;
        }
    }

    public class MWMWInitiateGameDefinition : MWMessageDefinition<MWInitiateGameMessage>
    {
        public MWMWInitiateGameDefinition() : base(MWMessageType.InitiateGameMessage) 
        {
            Properties.Add(new MWMessageProperty<int, MWInitiateGameMessage>(nameof(MWInitiateGameMessage.Seed)));
            Properties.Add(new MWMessageProperty<int, MWInitiateGameMessage>(nameof(MWInitiateGameMessage.ReadyID)));
        }
    }
}
