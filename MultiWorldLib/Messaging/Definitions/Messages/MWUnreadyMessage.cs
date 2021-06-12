namespace MultiWorldLib.Messaging.Definitions.Messages
{
    [MWMessageType(MWMessageType.UnreadyMessage)]
    public class MWUnreadyMessage : MWMessage
    {
        public MWUnreadyMessage()
        {
            MessageType = MWMessageType.UnreadyMessage;
        }
    }

    public class MWUnreadyMessageDefinition : MWMessageDefinition<MWUnreadyMessage>
    {
        public MWUnreadyMessageDefinition() : base(MWMessageType.UnreadyMessage)
        {
        }
    }
}
