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

        [Conditional("DEVELOPMENT_BUILD"), Conditional("UNITY_EDITOR")]
        public static void Debug(string category, string message)
        {
            if (MinimumLevel > LogLevel.Debug)
            {
                return;
            }

            UnityEngine.Debug.Log($"[{category}] {message}");
        }

        public static void Info(string category, string message)
        {
            if (MinimumLevel > LogLevel.Info)
            {
                return;
            }

            UnityEngine.Debug.Log($"[{category}] {message}");
        }

        public static void Warning(string category, string message)
        {
            if (MinimumLevel > LogLevel.Warning)
            {
                return;
            }

            UnityEngine.Debug.LogWarning($"[{category}] {message}");
        }

        public static void Error(string category, string message)
        {
            UnityEngine.Debug.LogError($"[{category}] {message}");
        }

        public static void Exception(string category, System.Exception exception)
        {
            UnityEngine.Debug.LogError($"[{category}] {exception}");
        }
    }
}
