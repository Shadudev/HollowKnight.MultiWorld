namespace MultiWorldLib.Messaging.Definitions.Messages
{
    [MWMessageType(MWMessageType.RequestSettingsMessage)]
    public class MWRequestSettingsMessage : MWMessage
    {
        public MWRequestSettingsMessage()
        {
            MessageType = MWMessageType.RequestSettingsMessage;
        }
    }

    public class MWRequestSettingsMessageDefinition : MWMessageDefinition<MWRequestSettingsMessage>
    {
        public MWRequestSettingsMessageDefinition() : base(MWMessageType.RequestSettingsMessage) { }
    }
}