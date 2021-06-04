// ReSharper disable file UnusedMember.Global

namespace MultiWorld
{
    public static class LogHelper
    {
        public static void Log(string message)
        {
            MultiWorld.Instance.Log(message);
        }

        public static void Log(object message)
        {
            MultiWorld.Instance.Log(message);
        }

        public static void LogDebug(string message)
        {
            MultiWorld.Instance.LogDebug(message);
        }

        public static void LogDebug(object message)
        {
            MultiWorld.Instance.LogDebug(message);
        }

        public static void LogError(string message)
        {
            MultiWorld.Instance.LogError(message);
        }

        public static void LogError(object message)
        {
            MultiWorld.Instance.LogError(message);
        }

        public static void LogFine(string message)
        {
            MultiWorld.Instance.LogFine(message);
        }

        public static void LogFine(object message)
        {
            MultiWorld.Instance.LogFine(message);
        }

        public static void LogWarn(string message)
        {
            MultiWorld.Instance.LogWarn(message);
        }

        public static void LogWarn(object message)
        {
            MultiWorld.Instance.LogWarn(message);
        }
    }
}