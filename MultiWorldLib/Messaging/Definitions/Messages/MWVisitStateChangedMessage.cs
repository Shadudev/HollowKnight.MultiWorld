using ItemChanger;

namespace MultiWorldLib.Messaging.Definitions.Messages
{
    public enum PreviewRecordTagType
    {
        None = 0,
        Single = 1,
        Multi = 2
    }

    [MWMessageType(MWMessageType.VisitStateChangedMessage)]
    public class MWVisitStateChangedMessage : MWConfirmableMessage
    {
        public string Name { get; set; }
        public string[] PreviewTexts { get; set; }
        public PreviewRecordTagType PreviewRecordTagType { get; set; }
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
            Properties.Add(new MWMessageProperty<PreviewRecordTagType, MWVisitStateChangedMessage>(nameof(MWVisitStateChangedMessage.PreviewRecordTagType)));
            Properties.Add(new MWMessageProperty<VisitState, MWVisitStateChangedMessage>(nameof(MWVisitStateChangedMessage.NewVisitFlags)));
        }
    }
}
