using System;
using System.Net.Sockets;
using System.Threading;

namespace MultiWorldLib.Messaging
{
    public struct MWPackedMessage
    {
        public uint Length;
        public byte[] Buffer;

        /// <summary>
        /// Create with length and buffer. Does not perform sanity check on length and buffer
        /// </summary>
        /// <param name="length"></param>
        /// <param name="buffer"></param>
        public MWPackedMessage(uint length, byte[] buffer)
        {
            Length = length;
            Buffer = buffer;
        }

        /// <summary>
        /// Create from buffer. Will parse the length out of the buffer and sanity check
        /// </summary>
        /// <param name="buffer"></param>
        public MWPackedMessage(byte[] buffer)
        {
            Buffer = buffer;
            Length = BitConverter.ToUInt32(buffer, 0);

            if(Buffer.Length != Length)
            {
                throw new InvalidOperationException("Buffer Length and length in data are mismatched");
            }
        }

        /// <summary>
        /// Blocking packet constructor from NetworkStream
        /// Highly stateful and blocking
        /// </summary>
        /// <param name="stream"></param>
        public MWPackedMessage(NetworkStream stream)
        {
            byte[] lengthBuffer = new byte[4];
            int curPointer = 0;
            while(curPointer < 4)
            {
                if (stream.DataAvailable)
                {
                    var lenRead = stream.Read(lengthBuffer, curPointer, 1);
                    curPointer+=lenRead;
                }
                Thread.Sleep(10);
            }
            Length = BitConverter.ToUInt32(lengthBuffer, 0);
            Buffer = new byte[Length];

            for (int i = 0; i < 4; i++)
                Buffer[i] = lengthBuffer[i];
            while(curPointer<Length)
            {
                if(stream.DataAvailable)
                {
                    var lenRead = stream.Read(Buffer, curPointer, (int)(Length - curPointer));
                    curPointer += lenRead;
                }
                Thread.Sleep(10);
            }
        }
    }
}
