namespace MultiWorldLib.Messaging.Definitions.Messages
{
    [MWMessageType(MWMessageType.RejoinMessage)]
    public class MWRejoinMessage : MWMessage
    {
        public int ReadyID { get; set; }
        public MWRejoinMessage()
        {
            MessageType = MWMessageType.RejoinMessage;
        }
    }

    public class MWRejoinMessageDefinition : MWMessageDefinition<MWRejoinMessage>
    {
        public MWRejoinMessageDefinition() : base(MWMessageType.RejoinMessage)
        {
            Properties.Add(new MWMessageProperty<int, MWRejoinMessage>(nameof(MWRejoinMessage.ReadyID)));
        }
    }
}
