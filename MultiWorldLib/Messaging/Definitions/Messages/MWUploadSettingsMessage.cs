namespace MultiWorldLib.Messaging.Definitions.Messages
{
    [MWMessageType(MWMessageType.UploadSettingsMessage)]
    public class MWUploadSettingsMessage : MWMessage
    {
        public string Settings { get; set; }

        public MWUploadSettingsMessage()
        {
            MessageType = MWMessageType.UploadSettingsMessage;
        }
    }

    public class MWuploadSettingsMessageDefinition : MWMessageDefinition<MWUploadSettingsMessage>
    {
        public MWuploadSettingsMessageDefinition() : base(MWMessageType.UploadSettingsMessage)
        {
            Properties.Add(new MWMessageProperty<string, MWUploadSettingsMessage>(nameof(MWApplySettingsMessage.Settings)));
        }
    }
}
