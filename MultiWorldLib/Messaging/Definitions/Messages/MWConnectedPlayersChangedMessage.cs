namespace MultiWorldLib.Messaging.Definitions.Messages
{
    [MWMessageType(MWMessageType.ConnectedPlayersChangedMessage)]
    public class MWConnectedPlayersChangedMessage : MWMessage
    {
        public Dictionary<int, string> Players { get; set; }

        public MWConnectedPlayersChangedMessage()
        {
            MessageType = MWMessageType.ConnectedPlayersChangedMessage;
        }
    }

    public class MWConnectedPlayersChangedDefinition : MWMessageDefinition<MWConnectedPlayersChangedMessage>
    {
        public MWConnectedPlayersChangedDefinition() : base(MWMessageType.ConnectedPlayersChangedMessage)
        {
            Properties.Add(new MWMessageProperty<Dictionary<int, string>, MWConnectedPlayersChangedMessage>(
                nameof(MWConnectedPlayersChangedMessage.Players)));
        }
    }
}
