namespace MultiWorldLib.Messaging.Definitions.Messages
{
    [MWMessageType(MWMessageType.ResultMessage)]
    public class MWResultMessage : MWMessage
    {
        public int PlayerId { get; set; }
        public int RandoId { get; set; }
        public string[] Nicknames { get; set; }
        public string ItemsSpoiler { get; set; }

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
            Properties.Add(new MWMessageProperty<int, MWResultMessage>(nameof(MWResultMessage.PlayerId)));
            Properties.Add(new MWMessageProperty<int, MWResultMessage>(nameof(MWResultMessage.RandoId)));
            Properties.Add(new MWMessageProperty<string[], MWResultMessage>(nameof(MWResultMessage.Nicknames)));
            Properties.Add(new MWMessageProperty<string, MWResultMessage>(nameof(MWResultMessage.ItemsSpoiler)));
            Properties.Add(new MWMessageProperty<Dictionary<string, (string, string)[]>, MWResultMessage>(nameof(MWResultMessage.Placements)));
        }
    }
}
