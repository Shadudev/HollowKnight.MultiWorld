namespace MultiWorldLib.Messaging.Definitions.Messages
{
    [MWMessageType(MWMessageType.ResultMessage)]
    public class MWResultMessage : MWMessage
    {
        public ResultData ResultData { get; set; }
        public Dictionary<string, (string item, string location)[]> Placements { get; set; }

        public MWResultMessage()
        {
            MessageType = MWMessageType.ResultMessage;
        }
    }

    public class MWResultMessageDefinition : MWMessageDefinition<MWResultMessage>
    {
        public MWResultMessageDefinition() : base(MWMessageType.ResultMessage)
        {
            Properties.Add(new MWMessageProperty<ResultData, MWResultMessage>(nameof(MWResultMessage.ResultData)));
            Properties.Add(new MWMessageProperty<Dictionary<string, (string, string)[]>, MWResultMessage>(nameof(MWResultMessage.Placements)));
        }
    }
}
