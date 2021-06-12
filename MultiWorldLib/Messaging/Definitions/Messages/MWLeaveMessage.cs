namespace MultiWorldLib.Messaging.Definitions.Messages
{
    [MWMessageType(MWMessageType.LeaveMessage)]
    public class MWLeaveMessage : MWMessage
    {
        public MWLeaveMessage()
        {
            MessageType = MWMessageType.LeaveMessage;
        }
    }

    public class MWLeaveMessageDefinition : MWMessageDefinition<MWLeaveMessage>
    {
        public MWLeaveMessageDefinition() : base(MWMessageType.LeaveMessage)
        {
        }
    }
}
