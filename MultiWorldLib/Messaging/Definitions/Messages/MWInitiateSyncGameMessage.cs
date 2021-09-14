namespace MultiWorldLib.Messaging.Definitions.Messages
{
    [MWMessageType(MWMessageType.InitiateSyncGameMessage)]
    public class MWInitiateSyncGameMessage : MWMessage
    {
        public MWInitiateSyncGameMessage()
        {
            MessageType = MWMessageType.InitiateSyncGameMessage;
        }
    }

    public class MWMWInitiateSyncGameDefinition : MWMessageDefinition<MWInitiateGameMessage>
    {
        public MWMWInitiateSyncGameDefinition() : base(MWMessageType.InitiateSyncGameMessage)
        {
        }
    }
}
