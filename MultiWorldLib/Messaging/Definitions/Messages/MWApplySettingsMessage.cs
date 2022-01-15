namespace MultiWorldLib.Messaging.Definitions.Messages
{
    [MWMessageType(MWMessageType.ApplySettingsMessage)]
    public class MWApplySettingsMessage : MWMessage
    {
        public string Settings { get; set; }
        
        public MWApplySettingsMessage()
        {
            MessageType = MWMessageType.ApplySettingsMessage;
        }
    }

    public class MWApplySettingsMessageDefinition : MWMessageDefinition<MWApplySettingsMessage>
    {
        public MWApplySettingsMessageDefinition() : base(MWMessageType.ApplySettingsMessage)
        {
            Properties.Add(new MWMessageProperty<string, MWApplySettingsMessage>(nameof(MWApplySettingsMessage.Settings)));

        }
    }
}