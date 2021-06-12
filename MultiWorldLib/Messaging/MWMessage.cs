namespace MultiWorldLib.Messaging
{
    public class MWMessage
    {
        ulong senderUid;
        ulong messageId;
        MWMessageType messageType;
        public MWMessage()
        {
        }

        public ulong SenderUid { get => senderUid; set => senderUid = value; }
        public ulong MessageId { get => messageId; set => messageId = value; }
        public MWMessageType MessageType { get => messageType; set => messageType = value; }
    }
}
