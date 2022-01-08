namespace MultiWorldLib.Messaging.Definitions.Messages
{
    [MWMessageType(MWMessageType.ReadyDenyMessage)]
    public class MWReadyDenyMessage : MWMessage
    {
        public string Description { get; set; }

        public MWReadyDenyMessage()
        {
            MessageType = MWMessageType.ReadyDenyMessage;
        }
    }

    public class MWReadyDenyMessageDefinition : MWMessageDefinition<MWReadyDenyMessage>
    {
        public MWReadyDenyMessageDefinition() : base(MWMessageType.ReadyDenyMessage)
        {
            Properties.Add(new MWMessageProperty<string, MWReadyDenyMessage>(nameof(MWReadyDenyMessage.Description)));
        }
    }
}
