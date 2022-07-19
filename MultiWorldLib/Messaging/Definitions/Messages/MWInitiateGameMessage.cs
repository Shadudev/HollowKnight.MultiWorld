namespace MultiWorldLib.Messaging.Definitions.Messages
{
    [MWMessageType(MWMessageType.InitiateGameMessage)]
    public class MWInitiateGameMessage: MWMessage
    {
        public string Settings { get; set; }

        public MWInitiateGameMessage()
        {
            MessageType = MWMessageType.InitiateGameMessage;
        }
    }

    public class MWInitiateGameDefinition : MWMessageDefinition<MWInitiateGameMessage>
    {
        public MWInitiateGameDefinition() : base(MWMessageType.InitiateGameMessage) 
        {
            Properties.Add(new MWMessageProperty<string, MWInitiateGameMessage>(nameof(MWInitiateGameMessage.Settings)));
        }
    }
}
