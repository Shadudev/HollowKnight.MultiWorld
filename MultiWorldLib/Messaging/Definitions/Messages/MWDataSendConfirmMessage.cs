namespace MultiWorldLib.Messaging.Definitions.Messages
{
    [MWMessageType(MWMessageType.DataSendConfirmMessage)]
    public class MWDataSendConfirmMessage : MWMessage, IConfirmMessage
    {
        public string Label { get; set; }
        public string Content { get; set; }
        public int To { get; set; }

        public MWDataSendConfirmMessage()
        {
            MessageType = MWMessageType.DataSendConfirmMessage;
        }

        public bool Confirms(MWConfirmableMessage message)
        {
            if (message.MessageType != MWMessageType.DataSendMessage)
            {
                return false;
            }

            MWDataSendMessage dataSendMessage = (MWDataSendMessage)message;
            return dataSendMessage.Label == Label &&
                dataSendMessage.To == To && dataSendMessage.Content == Content;
        }
    }

    public class MWDataSendConfirmDefinition : MWMessageDefinition<MWDataSendConfirmMessage>
    {
        public MWDataSendConfirmDefinition() : base(MWMessageType.DataSendConfirmMessage)
        {
            Properties.Add(new MWMessageProperty<string, MWDataSendConfirmMessage>(nameof(MWDataSendConfirmMessage.Label)));
            Properties.Add(new MWMessageProperty<string, MWDataSendConfirmMessage>(nameof(MWDataSendConfirmMessage.Content)));
            Properties.Add(new MWMessageProperty<int, MWDataSendConfirmMessage>(nameof(MWDataSendConfirmMessage.To)));
        }
    }
}
