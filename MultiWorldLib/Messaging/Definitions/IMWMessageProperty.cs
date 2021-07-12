using System;

namespace MultiWorldLib.Messaging.Definitions
{
    public interface IMWMessageProperty
    {
        Type Type { get;}
        void SetValue(object target, object val);
        object GetValue(object target);
    }
}
