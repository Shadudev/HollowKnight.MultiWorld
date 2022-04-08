namespace MultiWorldLib.Messaging.Definitions.Messages
{
    [MWMessageType(MWMessageType.TransitionFoundConfirmMessage)]
    public class MWTransitionFoundConfirmMessage : MWMessage, IConfirmMessage
    { 
        public string Source { get; set; }
        public string Target { get; set; }

        public MWTransitionFoundConfirmMessage()
        {
            MessageType = MWMessageType.TransitionFoundConfirmMessage;
        }

        public bool Confirms(MWConfirmableMessage message)
        {
            if (message.MessageType != MWMessageType.TransitionFoundMessage)
                return false;

            MWTransitionFoundMessage msg = (MWTransitionFoundMessage)message;
            return msg.Source == Source && msg.Target == Target;
        }
    }

    public class MWTransitionFoundConfirmMessageDefinition : MWMessageDefinition<MWTransitionFoundConfirmMessage>
    {
        public MWTransitionFoundConfirmMessageDefinition() : base(MWMessageType.TransitionFoundConfirmMessage)
        {
            Properties.Add(new MWMessageProperty<string, MWTransitionFoundConfirmMessage>(nameof(MWTransitionFoundConfirmMessage.Source)));
            Properties.Add(new MWMessageProperty<string, MWTransitionFoundConfirmMessage>(nameof(MWTransitionFoundConfirmMessage.Target)));
        }
    }
}
