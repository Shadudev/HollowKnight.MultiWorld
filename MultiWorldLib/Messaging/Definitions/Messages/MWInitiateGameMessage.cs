namespace MultiWorldLib.Messaging.Definitions.Messages
{
    [MWMessageType(MWMessageType.InitiateGameMessage)]
    public class MWInitiateGameMessage: MWMessage
    {
        public MultiWorldSettings Settings { get; set; }
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
            Properties.Add(new MWMessageProperty<MultiWorldSettings, MWInitiateGameMessage>(nameof(MWInitiateGameMessage.Settings)));
            Properties.Add(new MWMessageProperty<int, MWInitiateGameMessage>(nameof(MWInitiateGameMessage.ReadyID)));
        }
    }
}
