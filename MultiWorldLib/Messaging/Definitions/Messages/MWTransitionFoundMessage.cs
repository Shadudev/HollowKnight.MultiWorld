namespace MultiWorldLib.Messaging.Definitions.Messages
{
    [MWMessageType(MWMessageType.TransitionFoundMessage)]
    public class MWTransitionFoundMessage : MWConfirmableMessage
    {
        public string Source { get; set; }
        public string Target { get; set; }
        
        public MWTransitionFoundMessage()
        {
            MessageType = MWMessageType.TransitionFoundMessage;
        }
    }

    public class MWTransitionFoundMessageDefinition : MWMessageDefinition<MWTransitionFoundMessage>
    {
        public MWTransitionFoundMessageDefinition() : base(MWMessageType.TransitionFoundMessage)
        {
            Properties.Add(new MWMessageProperty<string, MWTransitionFoundMessage>(nameof(MWTransitionFoundMessage.Source)));
            Properties.Add(new MWMessageProperty<string, MWTransitionFoundMessage>(nameof(MWTransitionFoundMessage.Target)));
        }
    }
}
