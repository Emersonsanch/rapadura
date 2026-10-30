using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Rapadura.Core.Logging
{
    /// <summary>What made <see cref="CrashReporter"/> write a report.</summary>
    public enum CrashReportKind
    {
        /// <summary>An uncaught exception (or failed Debug.Assert) while the game kept running.</summary>
        Exception = 0,

        /// <summary>The previous session never reached a clean quit (process killed, native crash, power loss).</summary>
        UncleanExit = 1
    }

    /// <summary>Everything one crash report carries. Plain data: <see cref="CrashReportBuilder"/> turns it into text.</summary>
    public sealed class CrashReportInfo
    {
        public CrashReportKind Kind;
        public DateTime TimestampUtc;
        public string BuildVersion;
        public string UnityVersion;
        public string Platform;
        public string Scene;

        /// <summary>Game state lines ("Estado do jogo", "Herói", "Região"...), from <see cref="CrashReporter.RegisterContext"/>.</summary>
        public IReadOnlyList<KeyValuePair<string, string>> GameState;

        /// <summary>Hardware/OS lines. Never the device name or unique id (see <see cref="CrashReporter.CollectDeviceInfo"/>).</summary>
        public IReadOnlyList<KeyValuePair<string, string>> Device;

        public string ErrorMessage;
        public string StackTrace;

        /// <summary>Last log lines, oldest first.</summary>
        public IReadOnlyList<LogEntry> RecentLog;

        /// <summary>Free text under the header (e.g. the previous session's last checkpoint for <see cref="CrashReportKind.UncleanExit"/>).</summary>
        public string Notes;
    }

    /// <summary>
    /// Builds the plain-text crash report <see cref="CrashReporter"/> writes to disk and the title screen
    /// copies to the clipboard (TODO Fase 0 "Error/crash reporting", no external service). Pure and
    /// static so EditMode tests cover the format and the privacy scrub without a running player.
    ///
    /// Privacy: the report is meant to be pasted into a bug report by the player, so it carries no
    /// personal data. <see cref="Sanitize"/> replaces every known private fragment (the persistent data
    /// path, the user's home folder, the OS user name) and anything that still looks like a user folder
    /// or an e-mail address; callers never pass the device name or unique identifier in the first place.
    /// </summary>
    public static class CrashReportBuilder
    {
        public const string Title = "Rapadura - relatório de falha";

        /// <summary>Stack traces and messages are cut to this many characters (a runaway recursion can log megabytes).</summary>
        public const int MaxBlockLength = 8000;

        public const string UserPlaceholder = "<usuario>";
        public const string EmailPlaceholder = "<email>";

        private const string SectionRule = "----------------------------------------";

        private static readonly Regex WindowsUserFolder = new Regex(@"([A-Za-z]:[\\/]+(?:Users|Documents and Settings)[\\/]+)[^\\/\r\n""'<>|:]+", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        private static readonly Regex UnixUserFolder = new Regex(@"(/(?:Users|home)/)[^/\s""'<>]+", RegexOptions.CultureInvariant);
        private static readonly Regex Email = new Regex(@"[A-Za-z0-9._%+\-]+@[A-Za-z0-9.\-]+\.[A-Za-z]{2,}", RegexOptions.CultureInvariant);

        /// <summary>Report text, sections in a fixed order. Every free-form value goes through <see cref="Sanitize"/>.</summary>
        public static string Build(CrashReportInfo info, IReadOnlyList<KeyValuePair<string, string>> privateFragments = null)
        {
            if (info == null)
            {
                throw new ArgumentNullException(nameof(info));
            }

            var builder = new StringBuilder(2048);
            builder.Append(Title).Append('\n');
            builder.Append("Tipo: ").Append(KindLabel(info.Kind)).Append('\n');
            builder.Append("Data (UTC): ").Append(info.TimestampUtc.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)).Append('\n');
            builder.Append("Versão: ").Append(OrDash(info.BuildVersion)).Append('\n');
            builder.Append("Unity: ").Append(OrDash(info.UnityVersion)).Append('\n');
            builder.Append("Plataforma: ").Append(OrDash(info.Platform)).Append('\n');
            builder.Append("Cena: ").Append(OrDash(Sanitize(info.Scene, privateFragments))).Append('\n');

            if (!string.IsNullOrEmpty(info.Notes))
            {
                builder.Append('\n').Append(Truncate(Sanitize(info.Notes, privateFragments))).Append('\n');
            }

            AppendSection(builder, "Estado do jogo", info.GameState, privateFragments);
            AppendSection(builder, "Dispositivo", info.Device, privateFragments);

            if (!string.IsNullOrEmpty(info.ErrorMessage) || !string.IsNullOrEmpty(info.StackTrace))
            {
                builder.Append('\n').Append("Erro").Append('\n').Append(SectionRule).Append('\n');
                builder.Append(Truncate(Sanitize(info.ErrorMessage, privateFragments) ?? string.Empty).TrimEnd()).Append('\n');
                if (!string.IsNullOrEmpty(info.StackTrace))
                {
                    builder.Append(Truncate(Sanitize(info.StackTrace, privateFragments)).TrimEnd()).Append('\n');
                }
            }

            builder.Append('\n').Append("Últimas linhas do log").Append('\n').Append(SectionRule).Append('\n');
            if (info.RecentLog == null || info.RecentLog.Count == 0)
            {
                builder.Append("(vazio)").Append('\n');
            }
            else
            {
                for (int i = 0; i < info.RecentLog.Count; i++)
                {
                    LogEntry entry = info.RecentLog[i];
                    builder.Append(entry.Timestamp.ToString("HH:mm:ss", CultureInfo.InvariantCulture))
                        .Append(' ').Append(LevelLetter(entry.Level))
                        .Append(" [").Append(Sanitize(entry.Category, privateFragments) ?? "?").Append("] ")
                        .Append(FirstLine(Sanitize(entry.Message, privateFragments)))
                        .Append('\n');
                }
            }

            return builder.ToString();
        }

        /// <summary>
        /// Replaces each fragment's key with its value (case-insensitive, '/' and '\' interchangeable,
        /// longest first so a path wins over the user name inside it), then any leftover user folder
        /// (C:\Users\x, /Users/x, /home/x) and e-mail address. Null stays null.
        /// </summary>
        public static string Sanitize(string text, IReadOnlyList<KeyValuePair<string, string>> privateFragments)
        {
            if (string.IsNullOrEmpty(text))
            {
                return text;
            }

            string result = text;
            if (privateFragments != null && privateFragments.Count > 0)
            {
                var ordered = new List<KeyValuePair<string, string>>(privateFragments);
                ordered.Sort((a, b) => (b.Key?.Length ?? 0).CompareTo(a.Key?.Length ?? 0));
                foreach (KeyValuePair<string, string> fragment in ordered)
                {
                    if (string.IsNullOrEmpty(fragment.Key))
                    {
                        continue;
                    }

                    string pattern = Regex.Escape(fragment.Key.Replace('\\', '/')).Replace("/", @"[\\/]");
                    result = Regex.Replace(result, pattern, (fragment.Value ?? string.Empty).Replace("$", "$$"),
                        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
                }
            }

            result = WindowsUserFolder.Replace(result, "$1" + UserPlaceholder);
            result = UnixUserFolder.Replace(result, "$1" + UserPlaceholder);
            result = Email.Replace(result, EmailPlaceholder);
            return result;
        }

        /// <summary>
        /// Fragment list for <see cref="Sanitize"/>: each non-empty path (with its placeholder) plus the
        /// user name. A user name shorter than 3 characters is skipped — replacing "a" or "jo" everywhere
        /// would shred the report; the user-folder patterns still cover the paths that contain it.
        /// </summary>
        public static List<KeyValuePair<string, string>> PrivateFragments(IEnumerable<KeyValuePair<string, string>> paths, string userName)
        {
            var fragments = new List<KeyValuePair<string, string>>();
            if (paths != null)
            {
                foreach (KeyValuePair<string, string> path in paths)
                {
                    string trimmed = path.Key?.Trim().TrimEnd('/', '\\');
                    if (!string.IsNullOrEmpty(trimmed) && trimmed.Length >= 3)
                    {
                        fragments.Add(new KeyValuePair<string, string>(trimmed, path.Value));
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(userName) && userName.Trim().Length >= 3)
            {
                fragments.Add(new KeyValuePair<string, string>(userName.Trim(), UserPlaceholder));
            }

            return fragments;
        }

        public static string KindLabel(CrashReportKind kind)
        {
            return kind == CrashReportKind.UncleanExit ? "encerramento inesperado" : "exceção não tratada";
        }

        private static void AppendSection(StringBuilder builder, string title, IReadOnlyList<KeyValuePair<string, string>> lines,
            IReadOnlyList<KeyValuePair<string, string>> privateFragments)
        {
            if (lines == null || lines.Count == 0)
            {
                return;
            }

            builder.Append('\n').Append(title).Append('\n').Append(SectionRule).Append('\n');
            foreach (KeyValuePair<string, string> line in lines)
            {
                builder.Append(line.Key).Append(": ").Append(OrDash(FirstLine(Sanitize(line.Value, privateFragments)))).Append('\n');
            }
        }

        private static string FirstLine(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return text ?? string.Empty;
            }

            int newline = text.IndexOf('\n');
            string line = newline >= 0 ? text.Substring(0, newline) : text;
            line = line.TrimEnd('\r');
            return line.Length > 300 ? line.Substring(0, 300) + "..." : line;
        }

        private static string Truncate(string text)
        {
            if (text == null || text.Length <= MaxBlockLength)
            {
                return text;
            }

            return text.Substring(0, MaxBlockLength) + "\n... (cortado)";
        }

        private static string OrDash(string value) => string.IsNullOrWhiteSpace(value) ? "-" : value;

        private static char LevelLetter(LogLevel level)
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
