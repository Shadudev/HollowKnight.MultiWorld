using System;
using MultiWorldLib.Messaging;

namespace MultiWorldServer
{
    class ResendEntry
    {
        public MWConfirmableMessage Message;
        public DateTime LastSent;

        public ResendEntry(MWConfirmableMessage message)
        {
            LastSent = DateTime.MinValue;
            Message = message;
        }
    }
}
