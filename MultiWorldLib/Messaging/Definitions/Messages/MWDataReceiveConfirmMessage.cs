namespace MultiWorldLib.Messaging.Definitions.Messages
{
    [MWMessageType(MWMessageType.DataReceiveConfirmMessage)]
    public class MWDataReceiveConfirmMessage : MWMessage, IConfirmMessage
    {
        public string Label { get; set; }
        public string Data { get; set; }
        public string From { get; set; }

        public MWDataReceiveConfirmMessage()
        {
            MessageType = MWMessageType.DataReceiveConfirmMessage;
        }

        public bool Confirms(MWConfirmableMessage message)
        {
            if (message.MessageType != MWMessageType.DataReceiveMessage)
                return false;

            MWDataReceiveMessage dataReceiveMessage = (MWDataReceiveMessage)message;
            return dataReceiveMessage.Label == Label &&
                dataReceiveMessage.Data == Data && dataReceiveMessage.From == From;
        }
    }

    public class MWDataReceiveConfirmDefinition : MWMessageDefinition<MWDataReceiveConfirmMessage>
    {
        public MWDataReceiveConfirmDefinition() : base(MWMessageType.DataReceiveConfirmMessage)
        {
            Properties.Add(new MWMessageProperty<string, MWDataReceiveConfirmMessage>(nameof(MWDataReceiveConfirmMessage.Label)));
            Properties.Add(new MWMessageProperty<string, MWDataReceiveConfirmMessage>(nameof(MWDataReceiveConfirmMessage.Data)));
            Properties.Add(new MWMessageProperty<string, MWDataReceiveConfirmMessage>(nameof(MWDataReceiveConfirmMessage.From)));
        }
    }
}