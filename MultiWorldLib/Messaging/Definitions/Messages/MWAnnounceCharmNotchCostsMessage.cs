namespace MultiWorldLib.Messaging.Definitions.Messages
{

    [MWMessageType(MWMessageType.AnnounceCharmNotchCostsMessage)]
    public class MWAnnounceCharmNotchCostsMessage : MWConfirmableMessage, IConfirmMessage
    {
        public int PlayerID { get; set; }
        public int[] Costs { get; set; }

        public MWAnnounceCharmNotchCostsMessage()
        {
            MessageType = MWMessageType.AnnounceCharmNotchCostsMessage;
        }

        public bool Confirms(MWConfirmableMessage message)
        {
            // This is called for a specific player's costs and is only sent once
            return message.MessageType == MWMessageType.RequestCharmNotchCostsMessage;
        }
    }

    public class MWAnnounceCostsDefinition : MWMessageDefinition<MWAnnounceCharmNotchCostsMessage>
    {
        public MWAnnounceCostsDefinition() : base(MWMessageType.AnnounceCharmNotchCostsMessage)
        {
            Properties.Add(new MWMessageProperty<int, MWAnnounceCharmNotchCostsMessage>(nameof(MWAnnounceCharmNotchCostsMessage.PlayerID)));
            Properties.Add(new MWMessageProperty<int[], MWAnnounceCharmNotchCostsMessage>(nameof(MWAnnounceCharmNotchCostsMessage.Costs)));
        }
    }
}
