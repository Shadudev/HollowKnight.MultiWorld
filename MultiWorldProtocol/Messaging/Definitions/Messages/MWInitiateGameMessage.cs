namespace MultiWorldLib.Messaging.Definitions.Messages
{
    [MWMessageType(MWMessageType.InitiateGameMessage)]
    public class MWInitiateGameMessage: MWMessage
    {
        public MWInitiateGameMessage()
        {
            MessageType = MWMessageType.InitiateGameMessage;
        }
    }

    public class MWMWInitiateGameDefinition : MWMessageDefinition<MWInitiateGameMessage>
    {
        public MWMWInitiateGameDefinition() : base(MWMessageType.InitiateGameMessage) { }
    }
}
