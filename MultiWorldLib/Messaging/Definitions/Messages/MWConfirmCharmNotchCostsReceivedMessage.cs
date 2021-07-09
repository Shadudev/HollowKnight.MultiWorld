namespace MultiWorldLib.Messaging.Definitions.Messages
{

    [MWMessageType(MWMessageType.ConfirmCharmNotchCostsReceivedMessage)]
    public class MWConfirmCharmNotchCostsReceivedMessage : MWMessage
    {
        public int PlayerID { get; set; }

        public MWConfirmCharmNotchCostsReceivedMessage()
        {
            MessageType = MWMessageType.ConfirmCharmNotchCostsReceivedMessage;
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
