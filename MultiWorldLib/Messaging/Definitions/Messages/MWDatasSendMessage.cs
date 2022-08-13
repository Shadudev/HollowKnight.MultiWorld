namespace MultiWorldLib.Messaging.Definitions.Messages
{
    [MWMessageType(MWMessageType.DatasSendMessage)]
    public class MWDatasSendMessage : MWConfirmableMessage
    {
        public List<(string Label, string Data, int To)> Datas { get; set; }

        public MWDatasSendMessage()
        {
            MessageType = MWMessageType.DatasSendMessage;
        }
    }

    public class MWDatasSendDefinition : MWMessageDefinition<MWDatasSendMessage>
    {
        public MWDatasSendDefinition() : base(MWMessageType.DatasSendMessage)
        {
            Properties.Add(new MWMessageProperty<List<(string, string, int)>,
                MWDatasSendMessage>(nameof(MWDatasSendMessage.Datas)));
        }
    }
}
