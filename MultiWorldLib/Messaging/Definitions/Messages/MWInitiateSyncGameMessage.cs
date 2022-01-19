namespace MultiWorldLib.Messaging.Definitions.Messages
{
    [MWMessageType(MWMessageType.InitiateSyncGameMessage)]
    public class MWInitiateSyncGameMessage : MWMessage
    {
        public string Settings { get; set; }

        public MWInitiateSyncGameMessage()
        {
            MessageType = MWMessageType.InitiateSyncGameMessage;
        }
    }

    public class MWMWInitiateSyncGameDefinition : MWMessageDefinition<MWInitiateSyncGameMessage>
    {
        public MWMWInitiateSyncGameDefinition() : base(MWMessageType.InitiateSyncGameMessage)
        {
            Properties.Add(new MWMessageProperty<string, MWInitiateSyncGameMessage>(nameof(MWInitiateSyncGameMessage.Settings)));
        }
    }
}
