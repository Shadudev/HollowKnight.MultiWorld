using System.Diagnostics;

namespace ItemSyncMod
{
    public static class LogHelper
    {
        public static event Action<string> OnLog;

        public static void Log(string message = "")
        {
            OnLog?.Invoke(message);
        }

        [Conditional("DEBUG")]
        public static void LogDebug(string message)
        {
            OnLog?.Invoke(message);
        }

        public static void LogError(string message)
        {
            OnLog?.Invoke(message);
        }

        public static void LogWarn(string message)
        {
            OnLog?.Invoke(message);
        }
    }
}
