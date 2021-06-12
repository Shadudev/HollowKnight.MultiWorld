using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MultiWorld.Connection
{
    public class ConnectionState
    {
        public ulong Uid;
        public bool Connected = false;
        public bool Joined = false;

        public int SessionId = -1;
        public int PlayerId = -1;

        public DateTime LastPing = DateTime.Now;
    }
}
