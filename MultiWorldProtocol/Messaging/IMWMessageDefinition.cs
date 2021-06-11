using System.Collections.Generic;
using MultiWorldLib.Messaging.Definitions;

namespace MultiWorldLib.Messaging
{
    public interface IMWMessageDefinition
    {
        MWMessageType MessageType { get; }
        List<IMWMessageProperty> Properties { get; }
    }
}
