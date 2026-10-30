using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using Rapadura.Core.Versioning;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Rapadura.Core.Logging
{
    /// <summary>What the title screen should tell the player about the previous session (<see cref="CrashReporter.PendingNotice"/>).</summary>
    public enum CrashNoticeKind
    {
        None = 0,

        /// <summary>The previous session died without a clean quit ("O jogo fechou de forma inesperada da última vez").</summary>
        UnexpectedClose = 1,

        /// <summary>The previous session quit normally but logged an uncaught exception.</summary>
        ErrorLogged = 2
    }

    /// <summary>
    /// Local crash/error reporting, no external service (TODO Fase 0 "Error/crash reporting"). Installed
    /// before the first scene loads, in every build:
    /// <list type="bullet">
    /// <item>keeps the last <see cref="LogCapacity"/> Unity log lines (GameLogger's "[Categoria]" prefix is
    /// split back out) in a <see cref="LogRingBuffer"/> — a struct copy per line, no allocation beyond the
    /// strings Unity already built;</item>
    /// <item>on an uncaught exception or failed assert writes a report (<see cref="CrashReportBuilder"/>:
    /// recent log, scene, game state from <see cref="RegisterContext"/> providers, build version, device)
    /// to <c>persistentDataPath/CrashReports</c> (<see cref="CrashReportStore"/>, rotated), at most
    /// <see cref="MaxReportsPerSession"/> per session and never twice in a row for the same message;</item>
    /// <item>keeps a session marker refreshed on every scene change — it holds a checkpoint report, and is
    /// deleted on a clean quit or when the app loses focus (mobile OSes kill backgrounded apps without a
    /// quit callback, which must not read as a crash). A marker found at launch becomes an
    /// <see cref="CrashReportKind.UncleanExit"/> report;</item>
    /// <item>exposes the previous session's outcome as <see cref="PendingNotice"/>, which the title screen
    /// shows once as a small non-blocking banner with "Copiar relatório".</item>
    /// </list>
    /// No personal data: no device name, no unique id, and every path/user name/e-mail is scrubbed
    /// (<see cref="CrashReportBuilder.Sanitize"/>).
    ///
    /// In the Editor it stays off unless <see cref="EnabledInEditor"/> (menu Rapadura/Debug/Relatório de
    /// falhas no Editor): tests log errors on purpose and must not leave reports and title notices behind.
    /// The log callback may run on any thread; game state providers are only queried on the main thread
    /// (a worker-thread exception gets the cached scene name and no state lines).
    /// </summary>
    public static class CrashReporter
    {
        public const int LogCapacity = 100;
        public const int ReportLogLines = 60;
        public const int MaxReportsPerSession = 3;
        public const string EditorPrefKey = "Rapadura.CrashReporter.EnabledInEditor";

        public const string UncleanExitNotes =
            "O jogo não chegou a fechar normalmente (processo encerrado, travamento ou queda de energia). "
            + "Os dados abaixo são do último ponto de controle da sessão (troca de cena); o log termina ali.";

        private const string UnityCategory = "Unity";

        private static readonly LogRingBuffer Buffer = new LogRingBuffer(LogCapacity);
        private static readonly List<KeyValuePair<string, Func<string>>> ContextProviders = new List<KeyValuePair<string, Func<string>>>();
        private static readonly List<LogEntry> ScratchLog = new List<LogEntry>(ReportLogLines);
        private static readonly object Gate = new object();

        private static CrashReportStore _store;
        private static List<KeyValuePair<string, string>> _privateFragments;
        private static List<KeyValuePair<string, string>> _device;
        private static string _buildVersion;
        private static string _unityVersion;
        private static string _platform;
        private static string _sceneName;
        private static string _lastReportedCondition;
        private static int _mainThreadId;
        private static bool _installed;
        private static bool _writing;

        public static bool IsInstalled => _installed;

        /// <summary>The report folder in use (null until installed).</summary>
        public static CrashReportStore Store => _store;

        /// <summary>The rolling log the reports quote.</summary>
        public static LogRingBuffer Entries => Buffer;

        public static int ReportsWrittenThisSession { get; private set; }

        /// <summary>What the previous session left for the title screen to say (None once dismissed).</summary>
        public static CrashNoticeKind PendingNotice { get; private set; }

        /// <summary>The report <see cref="PendingNotice"/> refers to (full path), or null.</summary>
        public static string PendingNoticeReportPath { get; private set; }

        /// <summary>Raised after a report file was written (full path). May run on a worker thread.</summary>
        public static event Action<string> ReportWritten;

        /// <summary>Editor-only opt-in (EditorPrefs); always true in a player build.</summary>
        public static bool EnabledInEditor
        {
            get
            {
#if UNITY_EDITOR
                return UnityEditor.EditorPrefs.GetBool(EditorPrefKey, false);
#else
                return true;
#endif
            }
            set
            {
#if UNITY_EDITOR
                UnityEditor.EditorPrefs.SetBool(EditorPrefKey, value);
#endif
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void AutoInstall()
        {
            // Domain reload may be off (Enter Play Mode options): drop the previous run's handlers first.
            Uninstall();

            if (Application.isEditor && !EnabledInEditor)
            {
                return;
            }

            Install(Application.persistentDataPath);
        }

        /// <summary>
        /// Starts capturing into <paramref name="rootFolder"/>/CrashReports, reads what the previous session
        /// left (<see cref="PendingNotice"/>) and writes this session's marker. Main thread. Idempotent.
        /// </summary>
        public static void Install(string rootFolder)
        {
            if (_installed)
            {
                return;
            }

            _store = new CrashReportStore(rootFolder);
            _mainThreadId = Thread.CurrentThread.ManagedThreadId;
            _buildVersion = SafeGet(() => BuildVersion.DisplayLabel);
            _unityVersion = SafeGet(() => Application.unityVersion);
            _platform = SafeGet(() => Application.platform.ToString());
            _sceneName = SafeActiveSceneName();
            _device = CollectDeviceInfo();
            _privateFragments = CollectPrivateFragments(rootFolder);
            _lastReportedCondition = null;
            ReportsWrittenThisSession = 0;
            Buffer.Clear();

            ReadPreviousSession();

            _installed = true;
            Application.logMessageReceivedThreaded += OnUnityLog;
            Application.quitting += OnQuitting;
            Application.focusChanged += OnFocusChanged;
            SceneManager.activeSceneChanged += OnActiveSceneChanged;

            WriteCheckpoint();
        }

        /// <summary>Stops capturing. Leaves the files alone (a clean quit is <see cref="Application.quitting"/>'s job).</summary>
        public static void Uninstall()
        {
            if (!_installed)
            {
                return;
            }

            _installed = false;
            Application.logMessageReceivedThreaded -= OnUnityLog;
            Application.quitting -= OnQuitting;
            Application.focusChanged -= OnFocusChanged;
            SceneManager.activeSceneChanged -= OnActiveSceneChanged;
            PendingNotice = CrashNoticeKind.None;
            PendingNoticeReportPath = null;
        }

        /// <summary>
        /// Adds (or replaces, by <paramref name="label"/>) one "Estado do jogo" line, evaluated on the main
        /// thread when a report is built. A throwing or null-returning provider shows as "-". Works before
        /// <see cref="Install"/>: providers outlive installs.
        /// </summary>
        public static void RegisterContext(string label, Func<string> provider)
        {
            if (string.IsNullOrEmpty(label) || provider == null)
            {
                return;
            }

            lock (Gate)
            {
                int index = ContextProviders.FindIndex(p => p.Key == label);
                var entry = new KeyValuePair<string, Func<string>>(label, provider);
                if (index >= 0)
                {
                    ContextProviders[index] = entry;
                }
                else
                {
                    ContextProviders.Add(entry);
                }
            }
        }

        public static void UnregisterContext(string label)
        {
            lock (Gate)
            {
                ContextProviders.RemoveAll(p => p.Key == label);
            }
        }

        /// <summary>The text of <see cref="PendingNoticeReportPath"/> (what "Copiar relatório" copies), or null.</summary>
        public static string ReadPendingReport()
        {
            return _store != null ? _store.ReadReport(PendingNoticeReportPath) : null;
        }

        /// <summary>The player closed the title banner: it is not offered again this session.</summary>
        public static void DismissNotice()
        {
            PendingNotice = CrashNoticeKind.None;
        }

        /// <summary>
        /// Builds and writes an <see cref="CrashReportKind.Exception"/> report right now (main thread) and
        /// flags it for the next launch's notice, like an uncaught exception would. For the Editor's
        /// "Simular relatório de falha" menu and tests. Returns the file path, or null when not installed.
        /// </summary>
        public static string WriteReportNow(string message, string stackTrace = null)
        {
            if (!_installed)
            {
                return null;
            }

            string report = BuildReport(CrashReportKind.Exception, message, stackTrace, onMainThread: true);
            return StoreExceptionReport(report, message);
        }

        /// <summary>Report text for the current moment. <paramref name="onMainThread"/> false skips everything Unity only allows on the main thread.</summary>
        public static string BuildReport(CrashReportKind kind, string message, string stackTrace, bool onMainThread)
        {
            var info = new CrashReportInfo
            {
                Kind = kind,
                TimestampUtc = DateTime.UtcNow,
                BuildVersion = _buildVersion,
                UnityVersion = _unityVersion,
                Platform = _platform,
                Scene = onMainThread ? SafeActiveSceneName() : _sceneName,
                GameState = onMainThread
                    ? CollectGameState()
                    : new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>("Contexto", "indisponível (erro fora da thread principal)") },
                Device = _device,
                ErrorMessage = message,
                StackTrace = stackTrace,
                Notes = kind == CrashReportKind.UncleanExit ? UncleanExitNotes : null
            };

            lock (ScratchLog)
            {
                Buffer.CopyNewest(ScratchLog, ReportLogLines);
                info.RecentLog = new List<LogEntry>(ScratchLog);
            }

            return CrashReportBuilder.Build(info, _privateFragments);
        }

        /// <summary>
        /// Hardware/OS lines for the report. Deliberately not <see cref="SystemInfo.deviceName"/> (often the
        /// owner's name, "PC do João") nor <see cref="SystemInfo.deviceUniqueIdentifier"/>. Main thread.
        /// </summary>
        public static List<KeyValuePair<string, string>> CollectDeviceInfo()
        {
            var lines = new List<KeyValuePair<string, string>>();
            Add(lines, "Sistema", () => SystemInfo.operatingSystem);
            Add(lines, "Modelo", () => SystemInfo.deviceModel);
            Add(lines, "Tipo", () => SystemInfo.deviceType.ToString());
            Add(lines, "CPU", () => $"{SystemInfo.processorType} ({SystemInfo.processorCount} núcleos)");
            Add(lines, "RAM (MB)", () => SystemInfo.systemMemorySize.ToString(CultureInfo.InvariantCulture));
            Add(lines, "GPU", () => $"{SystemInfo.graphicsDeviceName} ({SystemInfo.graphicsDeviceType}, {SystemInfo.graphicsMemorySize} MB)");
            Add(lines, "Tela", () => $"{Screen.width}x{Screen.height}");
            Add(lines, "Idioma do sistema", () => Application.systemLanguage.ToString());
            return lines;
        }

        // ------------------------------------------------------------------
        // Callbacks
        // ------------------------------------------------------------------

        private static void OnUnityLog(string condition, string stackTrace, LogType type)
        {
            try
            {
                SplitCategory(condition, type, out string category, out string message);
                Buffer.Add(ToLevel(type), category, message);

                if (type == LogType.Exception || type == LogType.Assert)
                {
                    TryWriteExceptionReport(condition, stackTrace);
                }
            }
            catch (Exception)
            {
                // Never throw from a log callback: Unity would log that too and call us again.
            }
        }

        private static void TryWriteExceptionReport(string condition, string stackTrace)
        {
            lock (Gate)
            {
                if (_writing || !_installed || ReportsWrittenThisSession >= MaxReportsPerSession || condition == _lastReportedCondition)
                {
                    return;
                }

                _writing = true;
            }

            try
            {
                bool onMainThread = Thread.CurrentThread.ManagedThreadId == _mainThreadId;
                string report = BuildReport(CrashReportKind.Exception, condition, stackTrace, onMainThread);
                StoreExceptionReport(report, condition);
            }
            finally
            {
                lock (Gate)
                {
                    _writing = false;
                }
            }
        }

        private static string StoreExceptionReport(string report, string condition)
        {
            CrashReportStore store = _store;
            string path = store?.Write(report, DateTime.UtcNow);
            if (path == null)
            {
                return null;
            }

            ReportsWrittenThisSession++;
            _lastReportedCondition = condition;
            store.WritePendingNotice(path, CrashReportKind.Exception);

            try
            {
                ReportWritten?.Invoke(path);
            }
            catch (Exception)
            {
                // A listener must not break reporting.
            }

            return path;
        }

        private static void OnActiveSceneChanged(Scene previous, Scene next)
        {
            _sceneName = next.name;
            WriteCheckpoint();
        }

        private static void OnFocusChanged(bool focused)
        {
            if (focused)
            {
                WriteCheckpoint();
            }
            else
            {
                // Backgrounded: Android/iOS may kill the process without Application.quitting.
                _store?.ClearSessionMarker();
            }
        }

        private static void OnQuitting()
        {
            _store?.ClearSessionMarker();
        }

        // ------------------------------------------------------------------
        // Session
        // ------------------------------------------------------------------

        private static void ReadPreviousSession()
        {
            PendingNotice = CrashNoticeKind.None;
            PendingNoticeReportPath = null;

            bool hasNotice = _store.TryReadPendingNotice(out string noticePath, out CrashReportKind noticeKind);
            string marker = _store.ReadSessionMarker();

            if (marker != null)
            {
                // Died without a clean quit. An exception report from that session (likely the cause) beats
                // the checkpoint; otherwise the checkpoint becomes the report.
                string path = hasNotice ? noticePath : _store.Write(marker, DateTime.UtcNow);
                if (path != null)
                {
                    PendingNotice = CrashNoticeKind.UnexpectedClose;
                    PendingNoticeReportPath = path;
                }
            }
            else if (hasNotice)
            {
                PendingNotice = noticeKind == CrashReportKind.UncleanExit ? CrashNoticeKind.UnexpectedClose : CrashNoticeKind.ErrorLogged;
                PendingNoticeReportPath = noticePath;
            }

            // Shown this launch only; this session's own reports flag the next one.
            _store.ClearPendingNotice();
            _store.ClearSessionMarker();
        }

        private static void WriteCheckpoint()
        {
            if (!_installed || _store == null)
            {
                return;
            }

            try
            {
                _store.WriteSessionMarker(BuildReport(CrashReportKind.UncleanExit, null, null, onMainThread: true));
            }
            catch (Exception)
            {
                // A provider or SystemInfo call failing must not break a scene change.
            }
        }

        // ------------------------------------------------------------------
        // Helpers
        // ------------------------------------------------------------------

        private static List<KeyValuePair<string, string>> CollectGameState()
        {
            var lines = new List<KeyValuePair<string, string>>();
            Add(lines, "Tempo de sessão (s)", () => Time.realtimeSinceStartup.ToString("0", CultureInfo.InvariantCulture));
            Add(lines, "timeScale", () => Time.timeScale.ToString("0.##", CultureInfo.InvariantCulture));

            KeyValuePair<string, Func<string>>[] providers;
            lock (Gate)
            {
                providers = ContextProviders.ToArray();
            }

            foreach (KeyValuePair<string, Func<string>> provider in providers)
            {
                Add(lines, provider.Key, provider.Value);
            }

            return lines;
        }

        private static List<KeyValuePair<string, string>> CollectPrivateFragments(string rootFolder)
        {
            var paths = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>(rootFolder, "<dados>"),
                new KeyValuePair<string, string>(SafeGet(() => Application.persistentDataPath), "<dados>"),
                new KeyValuePair<string, string>(SafeGet(() => Application.temporaryCachePath), "<cache>"),
                new KeyValuePair<string, string>(SafeGet(() => Application.dataPath), "<jogo>"),
                new KeyValuePair<string, string>(SafeGet(() => System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile)), "<home>")
            };

            return CrashReportBuilder.PrivateFragments(paths, SafeGet(() => System.Environment.UserName));
        }

        /// <summary>"[Save] Slot 2 corrupted" → ("Save", "Slot 2 corrupted"); anything else → (Unity/Exception, text).</summary>
        public static void SplitCategory(string condition, LogType type, out string category, out string message)
        {
            category = type == LogType.Exception ? "Exception" : UnityCategory;
            message = condition ?? string.Empty;

            if (message.Length > 2 && message[0] == '[')
            {
                int close = message.IndexOf(']', 1);
                if (close > 1 && close <= 40)
                {
                    category = message.Substring(1, close - 1);
                    message = message.Substring(close + 1).TrimStart();
                }
            }
        }

        private static LogLevel ToLevel(LogType type)
        {
            switch (type)
            {
                case LogType.Log: return LogLevel.Info;
                case LogType.Warning: return LogLevel.Warning;
                default: return LogLevel.Error;
            }
        }

        private static void Add(List<KeyValuePair<string, string>> lines, string label, Func<string> value)
        {
            lines.Add(new KeyValuePair<string, string>(label, SafeGet(value)));
        }

        private static string SafeGet(Func<string> value)
        {
            try
            {
                return value() ?? "-";
            }
            catch (Exception)
            {
                return "-";
            }
        }

        private static string SafeActiveSceneName()
        {
            try
            {
                return SceneManager.GetActiveScene().name;
            }
            catch (Exception)
            {
                return _sceneName;
            }
        }
    }
}
