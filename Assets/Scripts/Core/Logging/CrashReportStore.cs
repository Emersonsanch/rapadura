using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace Rapadura.Core.Logging
{
    /// <summary>
    /// The crash report folder on disk (<c>&lt;root&gt;/CrashReports</c>, root = Application.persistentDataPath
    /// in the game, a temp folder in tests). Holds:
    /// <list type="bullet">
    /// <item><c>crash-yyyyMMdd-HHmmss-fff.txt</c> reports, newest kept, rotated down to <see cref="MaxReports"/>
    /// on every write (names sort chronologically, so rotation is a plain name sort);</item>
    /// <item><see cref="SessionMarkerFileName"/>: written while a session runs, deleted on a clean quit — found
    /// at the next launch it means the previous run died (<see cref="CrashReportKind.UncleanExit"/>);</item>
    /// <item><see cref="NoticeFileName"/>: the report the next launch's title screen should offer.</item>
    /// </list>
    /// Every method swallows IO errors (read-only disk, full disk, antivirus lock) and reports failure
    /// through its return value: the crash reporter must never become the crash.
    /// </summary>
    public sealed class CrashReportStore
    {
        public const string FolderName = "CrashReports";
        public const string FilePrefix = "crash-";
        public const string FileExtension = ".txt";
        public const string SessionMarkerFileName = "session.marker";
        public const string NoticeFileName = "notice.pending";
        public const int DefaultMaxReports = 5;

        private static readonly UTF8Encoding Utf8NoBom = new UTF8Encoding(false);

        private readonly object _gate = new object();

        public CrashReportStore(string rootFolder, int maxReports = DefaultMaxReports)
        {
            if (string.IsNullOrEmpty(rootFolder))
            {
                throw new ArgumentException("A root folder is required.", nameof(rootFolder));
            }

            Folder = Path.Combine(rootFolder, FolderName);
            MaxReports = Math.Max(1, maxReports);
        }

        public string Folder { get; }

        public int MaxReports { get; }

        public string SessionMarkerPath => Path.Combine(Folder, SessionMarkerFileName);

        public string NoticePath => Path.Combine(Folder, NoticeFileName);

        /// <summary>File name for a report written at <paramref name="utc"/> (before any collision suffix).</summary>
        public static string FileNameFor(DateTime utc)
        {
            return FilePrefix + utc.ToString("yyyyMMdd-HHmmss-fff", CultureInfo.InvariantCulture) + FileExtension;
        }

        public static bool IsReportFileName(string fileName)
        {
            return !string.IsNullOrEmpty(fileName)
                   && fileName.StartsWith(FilePrefix, StringComparison.OrdinalIgnoreCase)
                   && fileName.EndsWith(FileExtension, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>Writes <paramref name="report"/> as a new file, rotates, and returns its full path (null on failure).</summary>
        public string Write(string report, DateTime utcNow)
        {
            lock (_gate)
            {
                try
                {
                    Directory.CreateDirectory(Folder);

                    string baseName = FileNameFor(utcNow);
                    string path = Path.Combine(Folder, baseName);
                    for (int suffix = 1; File.Exists(path); suffix++)
                    {
                        path = Path.Combine(Folder, baseName.Substring(0, baseName.Length - FileExtension.Length) + "-" + suffix + FileExtension);
                    }

                    File.WriteAllText(path, report ?? string.Empty, Utf8NoBom);
                    RotateLocked(keep: path);
                    return path;
                }
                catch (Exception e) when (IsIoFailure(e))
                {
                    return null;
                }
            }
        }

        /// <summary>Deletes the oldest reports beyond <see cref="MaxReports"/>. Returns how many were deleted.</summary>
        public int Rotate()
        {
            lock (_gate)
            {
                return RotateLocked(keep: null);
            }
        }

        /// <summary>Full paths of the stored reports, newest first.</summary>
        public List<string> ListReports()
        {
            lock (_gate)
            {
                return ListLocked();
            }
        }

        public string ReadReport(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }

            lock (_gate)
            {
                try
                {
                    return File.Exists(path) ? File.ReadAllText(path, Utf8NoBom) : null;
                }
                catch (Exception e) when (IsIoFailure(e))
                {
                    return null;
                }
            }
        }

        // ------------------------------------------------------------------
        // Session marker
        // ------------------------------------------------------------------

        public bool HasSessionMarker
        {
            get
            {
                lock (_gate)
                {
                    try
                    {
                        return File.Exists(SessionMarkerPath);
                    }
                    catch (Exception e) when (IsIoFailure(e))
                    {
                        return false;
                    }
                }
            }
        }

        /// <summary>The marker's contents (the running session's last checkpoint report), or null when there is none.</summary>
        public string ReadSessionMarker() => ReadFile(SessionMarkerPath);

        public bool WriteSessionMarker(string contents) => WriteFile(SessionMarkerPath, contents);

        public bool ClearSessionMarker() => DeleteFile(SessionMarkerPath);

        // ------------------------------------------------------------------
        // Pending notice
        // ------------------------------------------------------------------

        /// <summary>Remembers which report the next launch should offer, plus why (one line each).</summary>
        public bool WritePendingNotice(string reportPath, CrashReportKind kind)
        {
            if (string.IsNullOrEmpty(reportPath))
            {
                return false;
            }

            return WriteFile(NoticePath, Path.GetFileName(reportPath) + "\n" + (int)kind);
        }

        /// <summary>The pending notice's report (full path, only if the file still exists) and kind; false when there is none.</summary>
        public bool TryReadPendingNotice(out string reportPath, out CrashReportKind kind)
        {
            reportPath = null;
            kind = CrashReportKind.Exception;

            string contents = ReadFile(NoticePath);
            if (string.IsNullOrEmpty(contents))
            {
                return false;
            }

            string[] lines = contents.Split('\n');
            string fileName = lines[0].Trim();
            if (!IsReportFileName(fileName) || fileName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            {
                return false;
            }

            if (lines.Length > 1 && int.TryParse(lines[1].Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int raw)
                && Enum.IsDefined(typeof(CrashReportKind), raw))
            {
                kind = (CrashReportKind)raw;
            }

            string path = Path.Combine(Folder, fileName);
            lock (_gate)
            {
                try
                {
                    if (!File.Exists(path))
                    {
                        return false;
                    }
                }
                catch (Exception e) when (IsIoFailure(e))
                {
                    return false;
                }
            }

            reportPath = path;
            return true;
        }

        public bool ClearPendingNotice() => DeleteFile(NoticePath);

        // ------------------------------------------------------------------

        private List<string> ListLocked()
        {
            var reports = new List<string>();
            try
            {
                if (!Directory.Exists(Folder))
                {
                    return reports;
                }

                foreach (string path in Directory.GetFiles(Folder, FilePrefix + "*" + FileExtension))
                {
                    if (IsReportFileName(Path.GetFileName(path)))
                    {
                        reports.Add(path);
                    }
                }
            }
            catch (Exception e) when (IsIoFailure(e))
            {
                return reports;
            }

            // Timestamped names sort chronologically (two reports in the same millisecond get a "-n"
            // suffix and may swap places, which rotation can live with).
            reports.Sort((a, b) => string.CompareOrdinal(Path.GetFileName(b), Path.GetFileName(a)));
            return reports;
        }

        private int RotateLocked(string keep)
        {
            List<string> reports = ListLocked();
            int deleted = 0;
            for (int i = MaxReports; i < reports.Count; i++)
            {
                if (string.Equals(reports[i], keep, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                try
                {
                    File.Delete(reports[i]);
                    deleted++;
                }
                catch (Exception e) when (IsIoFailure(e))
                {
                    // Locked by a viewer: try again on the next write.
                }
            }

            return deleted;
        }

        private string ReadFile(string path)
        {
            lock (_gate)
            {
                try
                {
                    return File.Exists(path) ? File.ReadAllText(path, Utf8NoBom) : null;
                }
                catch (Exception e) when (IsIoFailure(e))
                {
                    return null;
                }
            }
        }

        private bool WriteFile(string path, string contents)
        {
            lock (_gate)
            {
                try
                {
                    Directory.CreateDirectory(Folder);
                    File.WriteAllText(path, contents ?? string.Empty, Utf8NoBom);
                    return true;
                }
                catch (Exception e) when (IsIoFailure(e))
                {
                    return false;
                }
            }
        }

        private bool DeleteFile(string path)
        {
            lock (_gate)
            {
                try
                {
                    if (File.Exists(path))
                    {
                        File.Delete(path);
                    }

                    return true;
                }
                catch (Exception e) when (IsIoFailure(e))
                {
                    return false;
                }
            }
        }

        private static bool IsIoFailure(Exception e)
        {
            return e is IOException || e is UnauthorizedAccessException || e is System.Security.SecurityException
                   || e is NotSupportedException || e is ArgumentException;
        }
    }
}
