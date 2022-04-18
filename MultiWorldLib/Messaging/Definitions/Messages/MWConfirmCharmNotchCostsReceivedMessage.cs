namespace MultiWorldLib.Messaging.Definitions.Messages
{

    [MWMessageType(MWMessageType.ConfirmCharmNotchCostsReceivedMessage)]
    public class MWConfirmCharmNotchCostsReceivedMessage : MWMessage, IConfirmMessage
    {
        public int PlayerID { get; set; }

        public MWConfirmCharmNotchCostsReceivedMessage()
        {
            MessageType = MWMessageType.ConfirmCharmNotchCostsReceivedMessage;
        }

        public bool Confirms(MWConfirmableMessage message)
        {
            if (message.MessageType != MWMessageType.AnnounceCharmNotchCostsMessage)
                return false;

            return ((MWAnnounceCharmNotchCostsMessage) message).PlayerID == PlayerID;
        }
    }

    public class MWConfirmCharmNotchCostsReceivedDefinition : MWMessageDefinition<MWConfirmCharmNotchCostsReceivedMessage>
    {
        public MWConfirmCharmNotchCostsReceivedDefinition() : base(MWMessageType.ConfirmCharmNotchCostsReceivedMessage)
        {
            Properties.Add(new MWMessageProperty<int, MWConfirmCharmNotchCostsReceivedMessage>(nameof(MWConfirmCharmNotchCostsReceivedMessage.PlayerID)));
        }
    }
}
