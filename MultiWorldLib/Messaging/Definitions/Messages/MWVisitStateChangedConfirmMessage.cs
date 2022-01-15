using ItemChanger;

namespace MultiWorldLib.Messaging.Definitions.Messages
{
    [MWMessageType(MWMessageType.VisitStateChangedConfirmMessage)]
    public class MWVisitStateChangedConfirmMessage : MWMessage, IConfirmMessage
    {
        public string Name { get; set; }
        public bool IsMultiPreviewRecordTag { get; set; }
        public VisitState NewVisitFlags { get; set; }

        public MWVisitStateChangedConfirmMessage()
        {
            MessageType = MWMessageType.VisitStateChangedConfirmMessage;
        }

        public bool Confirms(MWConfirmableMessage message)
        {
            if (message.MessageType != MWMessageType.VisitStateChangedMessage) 
                return false;

            MWVisitStateChangedMessage msg = (MWVisitStateChangedMessage)message;
            return Name == msg.Name && IsMultiPreviewRecordTag == msg.IsMultiPreviewRecordTag &&
                NewVisitFlags == msg.NewVisitFlags;
        }
    }

    public class MWVisitStateChangedConfirmDefinition : MWMessageDefinition<MWVisitStateChangedConfirmMessage>
    {
        public MWVisitStateChangedConfirmDefinition() : base(MWMessageType.VisitStateChangedConfirmMessage)
        {
            Properties.Add(new MWMessageProperty<string, MWVisitStateChangedConfirmMessage>(nameof(MWVisitStateChangedConfirmMessage.Name)));
            Properties.Add(new MWMessageProperty<bool, MWVisitStateChangedConfirmMessage>(nameof(MWVisitStateChangedConfirmMessage.IsMultiPreviewRecordTag)));
            Properties.Add(new MWMessageProperty<VisitState, MWVisitStateChangedConfirmMessage>(nameof(MWVisitStateChangedConfirmMessage.NewVisitFlags)));
        }
    }
}
