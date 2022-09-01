using System.Collections.Concurrent;

namespace MultiWorldLib.Messaging
{
    internal class MemoryStreamsPool
    {
        private ConcurrentBag<MemoryStream> freeMemoryStreams;
        private List<MemoryStream> busyMemoryStreams;

        public MemoryStreamsPool(int memoryStreamSize, int backingStreamsCount)
        {
            freeMemoryStreams = new ConcurrentBag<MemoryStream>();
            for (int i = 0; i < backingStreamsCount; i++)
                freeMemoryStreams.Add(new MemoryStream(memoryStreamSize));

            busyMemoryStreams = new List<MemoryStream>();
        }

        public MemoryStream Get()
        {
            lock (freeMemoryStreams)
            {
                if (freeMemoryStreams.IsEmpty)
                    Monitor.Wait(freeMemoryStreams);

                freeMemoryStreams.TryTake(out MemoryStream memoryStream);

                busyMemoryStreams.Add(memoryStream);
                return memoryStream;
            }
        }

        public void Release(MemoryStream memoryStream)
        {
            lock (freeMemoryStreams)
            {
                busyMemoryStreams.Remove(memoryStream);

                freeMemoryStreams.Add(memoryStream);
                Monitor.Pulse(freeMemoryStreams);
            }
        }
    }
}
