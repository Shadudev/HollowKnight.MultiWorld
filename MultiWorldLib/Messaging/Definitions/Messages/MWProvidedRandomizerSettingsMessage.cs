namespace MultiWorldLib.Messaging.Definitions.Messages
{
    [MWMessageType(MWMessageType.ProvidedRandomizerSettingsMessage)]
    public class MWProvidedRandomizerSettingsMessage : MWMessage
    {
        public string Settings { get; set; }
        
        public MWProvidedRandomizerSettingsMessage()
        {
            MessageType = MWMessageType.ProvidedRandomizerSettingsMessage;
        }
    }

    public class MWProvidedRandomizerSettingsMessageDefinition : MWMessageDefinition<MWProvidedRandomizerSettingsMessage>
    {
        public MWProvidedRandomizerSettingsMessageDefinition() : base(MWMessageType.ProvidedRandomizerSettingsMessage)
        {
            Properties.Add(new MWMessageProperty<string, MWProvidedRandomizerSettingsMessage>(nameof(MWProvidedRandomizerSettingsMessage.Settings)));

        }
    }
}