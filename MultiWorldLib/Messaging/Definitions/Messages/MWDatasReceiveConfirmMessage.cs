namespace MultiWorldLib.Messaging.Definitions.Messages
{
    [MWMessageType(MWMessageType.DatasReceiveConfirmMessage)]
    public class MWDatasReceiveConfirmMessage : MWMessage, IConfirmMessage
    {
        public int Count { get; set; }
        public string From { get; set; }

        public MWDatasReceiveConfirmMessage()
        {
            MessageType = MWMessageType.DatasReceiveConfirmMessage;
        }

        public bool Confirms(MWConfirmableMessage message)
        {
            if (message.MessageType != MWMessageType.DatasReceiveMessage)
                return false;

            MWDatasReceiveMessage datasReceiveMessage = (MWDatasReceiveMessage)message;
            return datasReceiveMessage.From == From && datasReceiveMessage.Datas.Count == Count;
        }
    }

    public class MWDatasReceiveConfirmDefinition : MWMessageDefinition<MWDatasReceiveConfirmMessage>
    {
        public MWDatasReceiveConfirmDefinition() : base(MWMessageType.DatasReceiveConfirmMessage)
        {
            Properties.Add(new MWMessageProperty<int, MWDatasReceiveConfirmMessage>(nameof(MWDatasReceiveConfirmMessage.Count)));
            Properties.Add(new MWMessageProperty<string, MWDatasReceiveConfirmMessage>(nameof(MWDatasReceiveConfirmMessage.From)));
        }
    }
}
