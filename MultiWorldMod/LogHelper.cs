// ReSharper disable file UnusedMember.Global

namespace MultiWorldMod
{
    public static class LogHelper
    {
        public static void Log(string message)
        {
            ItemSync.Instance.Log(message);
        }

        public static void Log(object message)
        {
            ItemSync.Instance.Log(message);
        }

        public static void LogDebug(string message)
        {
            ItemSync.Instance.LogDebug(message);
        }

        public static void LogDebug(object message)
        {
            ItemSync.Instance.LogDebug(message);
        }

        public static void LogError(string message)
        {
            ItemSync.Instance.LogError(message);
        }

        public static void LogError(object message)
        {
            ItemSync.Instance.LogError(message);
        }

        public static void LogFine(string message)
        {
            ItemSync.Instance.LogFine(message);
        }

        public static void LogFine(object message)
        {
            ItemSync.Instance.LogFine(message);
        }

        public static void LogWarn(string message)
        {
            ItemSync.Instance.LogWarn(message);
        }

        public static void LogWarn(object message)
        {
            ItemSync.Instance.LogWarn(message);
        }
    }
}