using MultiWorldLib.Messaging;
using MultiWorldLib.Messaging.Definitions;

[MWMessageType(MWMessageType.DisconnectMessage)]
public class MWDisconnectMessage : MWMessage
{
	public MWDisconnectMessage()
	{
        MessageType = MWMessageType.DisconnectMessage;
    }
}

public class MWDisconnectMessageDefinition : MWMessageDefinition<MWDisconnectMessage>
{
    public MWDisconnectMessageDefinition() : base(MWMessageType.DisconnectMessage)
    {
    }
}
