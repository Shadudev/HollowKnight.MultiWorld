using ItemChanger;

namespace MultiWorldLib.Messaging.Definitions.Messages
{
    [MWMessageType(MWMessageType.VisitStateChangedMessage)]
    public class MWVisitStateChangedMessage : MWConfirmableMessage
    {
        public string Name { get; set; }
        public string[] PreviewTexts { get; set; }
        public bool IsMultiPreviewRecordTag { get; set; }
        public VisitState NewVisitFlags { get; set; }

        public MWVisitStateChangedMessage()
        {
            MessageType = MWMessageType.VisitStateChangedMessage;
        }
    }

    public class MWVisitStateChangedDefinition : MWMessageDefinition<MWVisitStateChangedMessage>
    {
        public MWVisitStateChangedDefinition() : base(MWMessageType.VisitStateChangedMessage)
        {
            Properties.Add(new MWMessageProperty<string, MWVisitStateChangedMessage>(nameof(MWVisitStateChangedMessage.Name)));
            Properties.Add(new MWMessageProperty<string[], MWVisitStateChangedMessage>(nameof(MWVisitStateChangedMessage.PreviewTexts)));
            Properties.Add(new MWMessageProperty<bool, MWVisitStateChangedMessage>(nameof(MWVisitStateChangedMessage.IsMultiPreviewRecordTag)));
            Properties.Add(new MWMessageProperty<VisitState, MWVisitStateChangedMessage>(nameof(MWVisitStateChangedMessage.NewVisitFlags)));
        }
    }
}
