namespace MultiWorldLib.Messaging.Definitions.Messages
{
    [MWMessageType(MWMessageType.RequestCharmNotchCostsMessage)]
    public class MWRequestCharmNotchCostsMessage : MWMessage
    {
        public MWRequestCharmNotchCostsMessage()
        {
            MessageType = MWMessageType.RequestCharmNotchCostsMessage;
        }
    }

    public class MWRequestCharmNotchCostsDefinition : MWMessageDefinition<MWRequestCharmNotchCostsMessage>
    {
        public MWRequestCharmNotchCostsDefinition() : base(MWMessageType.RequestCharmNotchCostsMessage)
        {
        }
    }
}
