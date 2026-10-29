using System.Diagnostics;

namespace Rapadura.Core.Logging
{
    /// <summary>
    /// Central logging entry point. Wraps <see cref="Debug"/> so every log carries a
    /// consistent "[Category] message" prefix and Debug-level logs compile out of
    /// non-development builds entirely (see <see cref="ConditionalAttribute"/> below).
    /// Swap the body of each method for a third-party sink (Sentry, Unity Cloud Diagnostics)
    /// later without touching call sites.
    /// </summary>
    public static class GameLogger
    {
        public static LogLevel MinimumLevel = LogLevel.Debug;

        /// <summary>
        /// Fired after a message passes <see cref="MinimumLevel"/> and is written to the Unity console:
        /// (level, category, message). Used by the dev-only rolling log of the debug overlay
        /// (<c>Rapadura.Core.Debug.DebugLogHistory</c>); with no subscriber (release builds) logging costs
        /// nothing extra. A throwing subscriber never breaks the caller. Raised on whatever thread logged.
        /// </summary>
        public static event System.Action<LogLevel, string, string> MessageLogged;

        [Conditional("DEVELOPMENT_BUILD"), Conditional("UNITY_EDITOR")]
        public static void Debug(string category, string message)
        {
            if (MinimumLevel > LogLevel.Debug)
            {
                return;
            }

            UnityEngine.Debug.Log($"[{category}] {message}");
            Notify(LogLevel.Debug, category, message);
        }

        public static void Info(string category, string message)
        {
            if (MinimumLevel > LogLevel.Info)
            {
                return;
            }

            UnityEngine.Debug.Log($"[{category}] {message}");
            Notify(LogLevel.Info, category, message);
        }

        public static void Warning(string category, string message)
        {
            if (MinimumLevel > LogLevel.Warning)
            {
                return;
            }

            UnityEngine.Debug.LogWarning($"[{category}] {message}");
            Notify(LogLevel.Warning, category, message);
        }

        public static void Error(string category, string message)
        {
            UnityEngine.Debug.LogError($"[{category}] {message}");
            Notify(LogLevel.Error, category, message);
        }

        public static void Exception(string category, System.Exception exception)
        {
            UnityEngine.Debug.LogError($"[{category}] {exception}");

            if (MessageLogged != null)
            {
                Notify(LogLevel.Error, category, exception != null ? exception.GetType().Name + ": " + exception.Message : "null exception");
            }
        }

        private static void Notify(LogLevel level, string category, string message)
        {
            System.Action<LogLevel, string, string> handler = MessageLogged;
            if (handler == null)
            {
                return;
            }

            try
            {
                handler(level, category, message);
            }
            catch (System.Exception)
            {
                // A diagnostics sink must never turn a log call into a crash (and logging the failure
                // here could recurse into the same sink).
            }
        }
    }
}
