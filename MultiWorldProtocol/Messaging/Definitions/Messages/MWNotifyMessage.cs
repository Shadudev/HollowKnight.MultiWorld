namespace MultiWorldLib.Messaging.Definitions.Messages
{
    [MWMessageType(MWMessageType.NotifyMessage)]
    public class MWNotifyMessage : MWMessage
    {
        /// <summary>
        /// The message to send
        /// </summary>
        public string Message { get; set; }
        /// <summary>
        /// A player name or All
        /// </summary>
        public string To { get; set; }
        /// <summary>
        /// A player name or Server
        /// </summary>
        public string From { get; set; }

        public MWNotifyMessage()
        {
            MessageType = MWMessageType.NotifyMessage;
        }
    }

    public class MWNotifyMessageDefinition : MWMessageDefinition<MWNotifyMessage>
    {
        public MWNotifyMessageDefinition() : base(MWMessageType.NotifyMessage)
        {
            Properties.Add(new MWMessageProperty<string, MWNotifyMessage>(nameof(MWNotifyMessage.Message)));
            Properties.Add(new MWMessageProperty<string, MWNotifyMessage>(nameof(MWNotifyMessage.To)));
            Properties.Add(new MWMessageProperty<string, MWNotifyMessage>(nameof(MWNotifyMessage.From)));
        }
    }
}