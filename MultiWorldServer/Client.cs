using System;
using System.Net.Sockets;
using System.Threading;

namespace MultiWorldServer
{
    public class Client
    {
        public string Nickname;
        public ulong UID;
        public string Room = null;
        public (string, string)[] ReadyMetadata = null;
        public TcpClient TcpClient;
        public object SendLock = new object();
        public DateTime lastPing;
        public Thread ReadWorker;

        public PlayerSession Session;
    }
}
