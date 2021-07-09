using System;

namespace MultiWorldLib.Messaging.Definitions
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class MWMessageTypeAttribute : Attribute
    {
        public readonly MWMessageType Type;
        public MWMessageTypeAttribute(MWMessageType type)
        {
            Type = type;
        }
    }
}
