using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Rapadura.Core.Logging;
using UnityEngine;

namespace Rapadura.Core.Debug
{
    /// <summary>
    /// Rolling log behind the debug overlay (TODO "log rolante"): the last
    /// <see cref="LogRingBuffer.DefaultCapacity"/> <see cref="GameLogger"/> messages plus every uncaught
    /// exception Unity reports (<see cref="Application.logMessageReceivedThreaded"/>, type Exception only,
    /// so GameLogger lines are not captured twice).
    ///
    /// Installed before the first scene's Awake, ONLY in the Editor and Development builds (the same
    /// gate as <see cref="DebugOverlayController"/>'s bootstrap). In release builds nothing subscribes, so
    /// <see cref="GameLogger.MessageLogged"/> has no listener and this costs nothing. Capturing a message
    /// copies a struct into a preallocated ring (no allocation); text is built only while the overlay
    /// shows the log, and only when <see cref="LogRingBuffer.Version"/> changed.
    /// </summary>
    public static class DebugLogHistory
    {
        public const string ExceptionCategory = "Exception";

        private static readonly LogRingBuffer Buffer = new LogRingBuffer();
        private static bool _installed;

        public static LogRingBuffer Entries => Buffer;

        public static bool IsInstalled => _installed;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void AutoInstall()
        {
            // Domain reload may be off (Enter Play Mode options): drop the previous run's lines/handlers first.
            Uninstall();
            Buffer.Clear();
            Install();
        }
#endif

        /// <summary>Subscribes to GameLogger and Unity's threaded log callback. Idempotent.</summary>
        public static void Install()
        {
            if (_installed)
            {
                return;
            }

            _installed = true;
            GameLogger.MessageLogged += OnGameLog;
            Application.logMessageReceivedThreaded += OnUnityLog;
        }

        public static void Uninstall()
        {
            if (!_installed)
            {
                return;
            }

            _installed = false;
            GameLogger.MessageLogged -= OnGameLog;
            Application.logMessageReceivedThreaded -= OnUnityLog;
        }

        private static void OnGameLog(LogLevel level, string category, string message)
        {
            Buffer.Add(level, category, message);
        }

        private static void OnUnityLog(string condition, string stackTrace, LogType type)
        {
            // GameLogger lines already arrived through MessageLogged and plain Debug.Log noise from
            // packages is not wanted; uncaught exceptions are exactly what a tester needs to see.
            if (type == LogType.Exception)
            {
                Buffer.Add(LogLevel.Error, ExceptionCategory, condition);
            }
        }

        /// <summary>
        /// Appends entries as "HH:mm:ss E [Category] message" lines (newest last, invariant culture), each
        /// message cut to its first line and to <paramref name="maxMessageLength"/> characters.
        /// </summary>
        public static void AppendLines(StringBuilder builder, IReadOnlyList<LogEntry> entries, int maxMessageLength = 90)
        {
            for (int i = 0; i < entries.Count; i++)
            {
                LogEntry entry = entries[i];
                if (i > 0)
                {
                    builder.Append('\n');
                }

                builder.Append(entry.Timestamp.ToString("HH:mm:ss", CultureInfo.InvariantCulture))
                    .Append(' ').Append(LevelLetter(entry.Level))
                    .Append(" [").Append(entry.Category ?? "?").Append("] ");

                string message = entry.Message ?? string.Empty;
                int newline = message.IndexOf('\n');
                int length = newline >= 0 ? newline : message.Length;
                if (length > 0 && message[length - 1] == '\r')
                {
                    length--;
                }

                if (length > maxMessageLength)
                {
                    builder.Append(message, 0, maxMessageLength).Append("...");
                }
                else
                {
                    builder.Append(message, 0, length);
                }
            }
        }

        public static char LevelLetter(LogLevel level)
        {
            switch (level)
            {
                case LogLevel.Debug: return 'D';
                case LogLevel.Info: return 'I';
                case LogLevel.Warning: return 'W';
                default: return 'E';
            }
        }
    }
}
