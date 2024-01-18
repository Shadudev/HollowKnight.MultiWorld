using System;
using System.IO;
using System.Text;

namespace MultiWorldServer.Loggers
{
    public class LogWriter
    {
        private readonly string fileNameFormat;
        private readonly uint cappedFileSize, scatterCount;
        private StreamWriter streamWriter;
        private uint accumulatedSize = 0, fileIndex = 0;

        internal LogWriter(StreamWriter streamWriter, string fileNameFormat, uint maxSize, uint scatterCount)
        {
            this.streamWriter = streamWriter;
            this.fileNameFormat = fileNameFormat;
            this.scatterCount = scatterCount;
            cappedFileSize = maxSize / scatterCount;
        }

        internal void LogToAll(string message)
        {
            LogToConsole(message);
#if DEBUG
            Log(message);
#endif
        }

        internal void Log(string message, int? session = null)
        {
            string msg;
            if (session == null)
            {
                msg = $"[{DateTime.Now.ToLongTimeString()}] {message}";
            }
            else
            {
                msg = $"[{DateTime.Now.ToLongTimeString()}] [{session}] {message}";
            }

            lock (this)
            {
                accumulatedSize += (uint)msg.Length;
                streamWriter.WriteLine(msg);

                if (accumulatedSize > cappedFileSize)
                    MoveToNextFile();
            }

#if DEBUG
            Console.WriteLine(msg);
#endif
        }

        private void MoveToNextFile()
        {
            fileIndex = (fileIndex + 1) % scatterCount;
            string fileName = GetCurrentFileName();

            streamWriter = OpenFile(fileName, FileMode.Create);
            accumulatedSize = 0;
        }

        private string GetCurrentFileName() => GetCurrentFileName(fileNameFormat, fileIndex);

        internal static string GetCurrentFileName(string fileNameformat, uint index) => string.Format(fileNameformat, index);

        internal static StreamWriter OpenFile(string fileName, FileMode fileMode)
        {
            FileStream fileStream = new FileStream(fileName, fileMode, FileAccess.Write, FileShare.ReadWrite);
            return new StreamWriter(fileStream, Encoding.UTF8) { AutoFlush = true };
        }

        internal void LogDebug(string message, int? session = null)
        {
#if DEBUG
            Log(message, session);
#endif
        }

        internal void LogToConsole(string message)
        {
            Console.WriteLine(message);
#if !DEBUG
            Log(message);
#endif
        }

        internal void Close()
        {
            streamWriter.Close();
        }

        ~LogWriter()
        {
            Close();
        }
    }
}
