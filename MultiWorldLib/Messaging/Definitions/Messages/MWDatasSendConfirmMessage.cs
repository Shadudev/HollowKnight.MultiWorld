namespace MultiWorldLib.Messaging.Definitions.Messages
{
    [MWMessageType(MWMessageType.DatasSendConfirmMessage)]
    public class MWDatasSendConfirmMessage : MWMessage, IConfirmMessage
    {
        public int DatasCount { get; set; }

        public MWDatasSendConfirmMessage()
        {
            MessageType = MWMessageType.DatasSendConfirmMessage;
        }

        public bool Confirms(MWConfirmableMessage message)
        {
            if (message.MessageType != MWMessageType.DatasSendMessage)
                return false;

            return ((MWDatasSendMessage)message).Datas.Count == DatasCount;
        }
    }

    public class MWDatasSendConfirmDefinition : MWMessageDefinition<MWDatasSendConfirmMessage>
    {
        public MWDatasSendConfirmDefinition() : base(MWMessageType.DatasSendConfirmMessage)
        {
            Properties.Add(new MWMessageProperty<int, MWDatasSendConfirmMessage>(nameof(MWDatasSendConfirmMessage.DatasCount)));
        }
    }
}
