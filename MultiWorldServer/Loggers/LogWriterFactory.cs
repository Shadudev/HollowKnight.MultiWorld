using System.IO;

namespace MultiWorldServer.Loggers
{
    internal class LogWriterFactory
    {
        internal static LogWriter CreateLogger(string fileName, uint maxSize = uint.MaxValue, FileMode fileMode = FileMode.Create, uint scatterCount = 4)
        {
            if (!Directory.Exists("Logs"))
                Directory.CreateDirectory("Logs");

            string fileNameFormat = "Logs\\" + GenerateFileNameFormat(fileName);
            string currentFileName = LogWriter.GetCurrentFileName(fileNameFormat, 0);

            StreamWriter streamWriter = LogWriter.OpenFile(fileName, fileMode);
            return new LogWriter(streamWriter, currentFileName, maxSize, scatterCount);
        }

        private static string GenerateFileNameFormat(string fileName)
        {
            int separatorIndex = fileName.LastIndexOf('.');
            if (separatorIndex == -1)
                return fileName + "{0}";
            return fileName.Substring(0, separatorIndex) + "{0}" + fileName.Substring(separatorIndex + 1);
        }
    }
}
