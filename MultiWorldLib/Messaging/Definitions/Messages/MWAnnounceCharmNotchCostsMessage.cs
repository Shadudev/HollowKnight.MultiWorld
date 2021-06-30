namespace MultiWorldLib.Messaging.Definitions.Messages
{

    [MWMessageType(MWMessageType.AnnounceCharmNotchCostsMessage)]
    public class MWAnnounceCharmNotchCostsMessage : MWMessage
    {
        public int PlayerID { get; set; }
        public int[] Costs { get; set; }

        public MWAnnounceCharmNotchCostsMessage()
        {
            MessageType = MWMessageType.AnnounceCharmNotchCostsMessage;
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
