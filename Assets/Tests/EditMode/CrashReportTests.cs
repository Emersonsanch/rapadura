using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Rapadura.Core.DI;
using Rapadura.Core.Localization;
using Rapadura.Core.Logging;
using Rapadura.UI.Menus;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Rapadura.Tests
{
    /// <summary>
    /// EditMode tests for the local crash reporting (TODO Fase 0 "Error/crash reporting"): the report text
    /// and its privacy scrub (<see cref="CrashReportBuilder"/>), the folder with rotation, session marker and
    /// pending notice (<see cref="CrashReportStore"/>), the reporter's previous-session detection and
    /// exception capture (<see cref="CrashReporter"/>, installed on a temp folder) and the title banner's
    /// rules (<see cref="CrashNoticeBanner"/>, with injected providers — no clipboard, no panel).
    /// </summary>
    public class CrashReportTests
    {
        private static readonly DateTime Noon = new DateTime(2026, 10, 29, 12, 34, 56, DateTimeKind.Utc);

        private string _root;

        [SetUp]
        public void SetUp()
        {
            CrashReporter.Uninstall();
            _root = Path.Combine(Path.GetTempPath(), "RapaduraCrashTests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_root);
        }

        [TearDown]
        public void TearDown()
        {
            CrashReporter.Uninstall();
            CrashReporter.UnregisterContext("Teste");

            try
            {
                Directory.Delete(_root, recursive: true);
            }
            catch (IOException)
            {
                // Temp folder: the OS cleans it eventually.
            }
        }

        private static CrashReportInfo SampleInfo()
        {
            return new CrashReportInfo
            {
                Kind = CrashReportKind.Exception,
                TimestampUtc = Noon,
                BuildVersion = "v0.1.0 (dev)",
                UnityVersion = "6000.3.22f1",
                Platform = "WindowsPlayer",
                Scene = "Main",
                GameState = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("Estado do jogo", "Encounter"),
                    new KeyValuePair<string, string>("Herói", "Maithe")
                },
                Device = new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>("Sistema", "Windows 11") },
                ErrorMessage = "NullReferenceException: Object reference not set",
                StackTrace = "Rapadura.Gameplay.Foo.Bar () (at C:/Users/fulano/rapadura/Assets/Foo.cs:12)",
                RecentLog = new List<LogEntry>
                {
                    new LogEntry(LogLevel.Info, Noon, "Save", "Slot 0 salvo"),
                    new LogEntry(LogLevel.Error, Noon, "Exception", "NullReferenceException\nsecond line")
                }
            };
        }

        // ------------------------------------------------------------------
        // CrashReportBuilder
        // ------------------------------------------------------------------

        [Test]
        public void Build_HasEverySectionInOrder()
        {
            string report = CrashReportBuilder.Build(SampleInfo());

            StringAssert.StartsWith(CrashReportBuilder.Title, report);
            StringAssert.Contains("Tipo: exceção não tratada", report);
            StringAssert.Contains("Data (UTC): 2026-10-29 12:34:56", report);
            StringAssert.Contains("Versão: v0.1.0 (dev)", report);
            StringAssert.Contains("Cena: Main", report);
            StringAssert.Contains("Herói: Maithe", report);
            StringAssert.Contains("Sistema: Windows 11", report);
            StringAssert.Contains("NullReferenceException: Object reference not set", report);
            StringAssert.Contains("12:34:56 I [Save] Slot 0 salvo", report);
            StringAssert.Contains("12:34:56 E [Exception] NullReferenceException", report);
            StringAssert.DoesNotContain("second line", report, "Log lines are cut to their first line.");

            int state = report.IndexOf("Estado do jogo\n", StringComparison.Ordinal);
            int device = report.IndexOf("Dispositivo\n", StringComparison.Ordinal);
            int error = report.IndexOf("Erro\n", StringComparison.Ordinal);
            int log = report.IndexOf("Últimas linhas do log\n", StringComparison.Ordinal);
            Assert.That(state > 0 && state < device && device < error && error < log, "Sections out of order:\n" + report);
        }

        [Test]
        public void Build_UncleanExit_HasNotesAndNoErrorSection()
        {
            CrashReportInfo info = SampleInfo();
            info.Kind = CrashReportKind.UncleanExit;
            info.ErrorMessage = null;
            info.StackTrace = null;
            info.Notes = CrashReporter.UncleanExitNotes;
            info.RecentLog = null;

            string report = CrashReportBuilder.Build(info);

            StringAssert.Contains("Tipo: encerramento inesperado", report);
            StringAssert.Contains(CrashReporter.UncleanExitNotes, report);
            StringAssert.DoesNotContain("Erro\n", report);
            StringAssert.Contains("(vazio)", report);
        }

        [Test]
        public void Build_TruncatesHugeStackTraces()
        {
            CrashReportInfo info = SampleInfo();
            info.StackTrace = new string('x', CrashReportBuilder.MaxBlockLength * 3);

            string report = CrashReportBuilder.Build(info);

            Assert.Less(report.Length, CrashReportBuilder.MaxBlockLength * 2);
            StringAssert.Contains("(cortado)", report);
        }

        [Test]
        public void Sanitize_ReplacesKnownPathsUserNameUserFoldersAndEmails()
        {
            List<KeyValuePair<string, string>> fragments = CrashReportBuilder.PrivateFragments(new[]
            {
                new KeyValuePair<string, string>(@"C:\Users\fulano\AppData\LocalLow\Engenho Estudio\Rapadura", "<dados>")
            }, "fulano");

            string text = "Falha ao ler C:/Users/fulano/AppData/LocalLow/Engenho Estudio/Rapadura/Saves/slot_0.json; "
                          + "log em c:\\users\\FULANO\\Desktop\\x.txt; outro /home/beltrano/jogo e /Users/ciclano/a; "
                          + "contato fulano.silva@example.com; usuario fulano";

            string clean = CrashReportBuilder.Sanitize(text, fragments);

            StringAssert.Contains("<dados>/Saves/slot_0.json", clean);
            StringAssert.Contains("/home/<usuario>/jogo", clean);
            StringAssert.Contains("/Users/<usuario>/a", clean);
            StringAssert.Contains(CrashReportBuilder.EmailPlaceholder, clean);
            StringAssert.DoesNotContain("fulano", clean.ToLowerInvariant());
            StringAssert.DoesNotContain("beltrano", clean);
            StringAssert.DoesNotContain("ciclano", clean);
        }

        [Test]
        public void Build_ScrubsEveryFreeFormField()
        {
            CrashReportInfo info = SampleInfo();
            info.Scene = "Cena de fulano";
            info.RecentLog = new List<LogEntry> { new LogEntry(LogLevel.Warning, Noon, "Save", "C:\\Users\\fulano\\slot.json") };

            string report = CrashReportBuilder.Build(info, CrashReportBuilder.PrivateFragments(null, "fulano"));

            StringAssert.DoesNotContain("fulano", report, "The user name leaked into the report:\n" + report);
        }

        [Test]
        public void PrivateFragments_SkipsEmptyAndTooShortValues()
        {
            List<KeyValuePair<string, string>> fragments = CrashReportBuilder.PrivateFragments(new[]
            {
                new KeyValuePair<string, string>("", "<x>"),
                new KeyValuePair<string, string>(null, "<x>"),
                new KeyValuePair<string, string>("C:/", "<x>"),
                new KeyValuePair<string, string>("D:/Jogos/Rapadura/", "<jogo>")
            }, "jo");

            Assert.AreEqual(1, fragments.Count, "Only the real path survives; a 2-letter user name would shred the report.");
            Assert.AreEqual("D:/Jogos/Rapadura", fragments[0].Key, "Trailing separators are trimmed.");
        }

        // ------------------------------------------------------------------
        // CrashReportStore
        // ------------------------------------------------------------------

        [Test]
        public void Store_Write_CreatesTimestampedFileInCrashReportsFolder()
        {
            var store = new CrashReportStore(_root);

            string path = store.Write("relatório", Noon);

            Assert.IsNotNull(path);
            Assert.AreEqual(Path.Combine(_root, CrashReportStore.FolderName), Path.GetDirectoryName(path));
            Assert.AreEqual("crash-20261029-123456-000.txt", Path.GetFileName(path));
            Assert.AreEqual("relatório", store.ReadReport(path));
        }

        [Test]
        public void Store_Write_SameTimestampTwice_KeepsBoth()
        {
            var store = new CrashReportStore(_root);

            string first = store.Write("a", Noon);
            string second = store.Write("b", Noon);

            Assert.AreNotEqual(first, second);
            Assert.AreEqual(2, store.ListReports().Count);
        }

        [Test]
        public void Store_Rotation_KeepsOnlyTheNewestReports()
        {
            var store = new CrashReportStore(_root, maxReports: 3);
            for (int i = 0; i < 7; i++)
            {
                store.Write("report " + i, Noon.AddMinutes(i));
            }

            List<string> reports = store.ListReports();

            Assert.AreEqual(3, reports.Count);
            CollectionAssert.AreEqual(new[] { "report 6", "report 5", "report 4" }, reports.Select(store.ReadReport).ToArray(),
                "Rotation must drop the oldest reports and list newest first.");
        }

        [Test]
        public void Store_Rotate_IgnoresForeignFiles()
        {
            var store = new CrashReportStore(_root, maxReports: 1);
            store.Write("a", Noon);
            string foreign = Path.Combine(store.Folder, "leia-me.txt");
            File.WriteAllText(foreign, "não é relatório");
            store.Write("b", Noon.AddSeconds(1));

            Assert.AreEqual(0, store.Rotate());
            Assert.IsTrue(File.Exists(foreign), "Rotation deleted a file it did not write.");
            Assert.AreEqual(1, store.ListReports().Count);
        }

        [Test]
        public void Store_PendingNotice_RoundTripsAndRequiresTheReportFile()
        {
            var store = new CrashReportStore(_root);
            string path = store.Write("x", Noon);

            Assert.IsFalse(store.TryReadPendingNotice(out _, out _));
            Assert.IsTrue(store.WritePendingNotice(path, CrashReportKind.UncleanExit));
            Assert.IsTrue(store.TryReadPendingNotice(out string read, out CrashReportKind kind));
            Assert.AreEqual(path, read);
            Assert.AreEqual(CrashReportKind.UncleanExit, kind);

            File.Delete(path);
            Assert.IsFalse(store.TryReadPendingNotice(out _, out _), "A notice whose report was rotated away offers nothing to copy.");

            store.ClearPendingNotice();
            Assert.IsFalse(File.Exists(store.NoticePath));
        }

        [Test]
        public void Store_PendingNotice_RejectsPathsOutsideTheFolder()
        {
            var store = new CrashReportStore(_root);
            Directory.CreateDirectory(store.Folder);
            File.WriteAllText(store.NoticePath, "../../outro.txt\n0");

            Assert.IsFalse(store.TryReadPendingNotice(out _, out _));
        }

        [Test]
        public void Store_SessionMarker_WriteReadClear()
        {
            var store = new CrashReportStore(_root);

            Assert.IsFalse(store.HasSessionMarker);
            Assert.IsTrue(store.WriteSessionMarker("checkpoint"));
            Assert.IsTrue(store.HasSessionMarker);
            Assert.AreEqual("checkpoint", store.ReadSessionMarker());
            Assert.IsTrue(store.ClearSessionMarker());
            Assert.IsFalse(store.HasSessionMarker);
            Assert.IsNull(store.ReadSessionMarker());
        }

        // ------------------------------------------------------------------
        // CrashReporter
        // ------------------------------------------------------------------

        [Test]
        public void Install_CleanPreviousSession_NoNotice_AndWritesThisSessionsMarker()
        {
            CrashReporter.Install(_root);

            Assert.AreEqual(CrashNoticeKind.None, CrashReporter.PendingNotice);
            Assert.IsNull(CrashReporter.PendingNoticeReportPath);
            Assert.IsTrue(CrashReporter.Store.HasSessionMarker, "A running session must leave a marker for the next launch to find.");
            StringAssert.Contains("encerramento inesperado", CrashReporter.Store.ReadSessionMarker());
        }

        [Test]
        public void Install_LeftoverMarker_TurnsItIntoAnUncleanExitReportAndNotice()
        {
            var store = new CrashReportStore(_root);
            store.WriteSessionMarker("checkpoint da sessão anterior");

            CrashReporter.Install(_root);

            Assert.AreEqual(CrashNoticeKind.UnexpectedClose, CrashReporter.PendingNotice);
            Assert.AreEqual("checkpoint da sessão anterior", CrashReporter.ReadPendingReport());
            Assert.AreEqual(1, store.ListReports().Count);
            Assert.IsFalse(File.Exists(store.NoticePath), "The notice is shown on this launch only.");
        }

        [Test]
        public void Install_LeftoverMarkerAndExceptionReport_OffersTheExceptionReport()
        {
            var store = new CrashReportStore(_root);
            string exceptionReport = store.Write("exceção da sessão anterior", Noon);
            store.WritePendingNotice(exceptionReport, CrashReportKind.Exception);
            store.WriteSessionMarker("checkpoint");

            CrashReporter.Install(_root);

            Assert.AreEqual(CrashNoticeKind.UnexpectedClose, CrashReporter.PendingNotice);
            Assert.AreEqual(exceptionReport, CrashReporter.PendingNoticeReportPath);
            Assert.AreEqual(1, store.ListReports().Count, "No extra checkpoint report when the exception report explains the crash.");
        }

        [Test]
        public void Install_ExceptionReportButCleanQuit_IsErrorLoggedNotice()
        {
            var store = new CrashReportStore(_root);
            store.WritePendingNotice(store.Write("exceção", Noon), CrashReportKind.Exception);

            CrashReporter.Install(_root);

            Assert.AreEqual(CrashNoticeKind.ErrorLogged, CrashReporter.PendingNotice);

            CrashReporter.DismissNotice();
            Assert.AreEqual(CrashNoticeKind.None, CrashReporter.PendingNotice);
        }

        [Test]
        public void UncaughtException_WritesSanitizedReportWithContext_AndFlagsTheNextLaunch()
        {
            CrashReporter.Install(_root);
            CrashReporter.RegisterContext("Teste", () => "valor do contexto");
            Debug.Log("[Save] linha antes da falha");

            LogAssert.Expect(LogType.Exception, new Regex("InvalidOperationException"));
            Debug.LogException(new InvalidOperationException("falhou lendo C:\\Users\\fulano\\save.json"));

            List<string> reports = CrashReporter.Store.ListReports();
            Assert.AreEqual(1, reports.Count, "An uncaught exception must write exactly one report.");
            Assert.AreEqual(1, CrashReporter.ReportsWrittenThisSession);

            string report = CrashReporter.Store.ReadReport(reports[0]);
            StringAssert.Contains("InvalidOperationException", report);
            StringAssert.Contains("Teste: valor do contexto", report);
            StringAssert.Contains("[Save] linha antes da falha", report);
            StringAssert.DoesNotContain("fulano", report);

            Assert.IsTrue(CrashReporter.Store.TryReadPendingNotice(out string flagged, out CrashReportKind kind));
            Assert.AreEqual(reports[0], flagged);
            Assert.AreEqual(CrashReportKind.Exception, kind);
            Assert.AreEqual(CrashNoticeKind.None, CrashReporter.PendingNotice, "This session's report is for the next launch, not this title screen.");
        }

        [Test]
        public void UncaughtException_SameMessageTwice_WritesOnce_AndCapsPerSession()
        {
            CrashReporter.Install(_root);

            for (int i = 0; i < 2; i++)
            {
                LogAssert.Expect(LogType.Exception, new Regex("repetida"));
                Debug.LogException(new Exception("repetida"));
            }

            Assert.AreEqual(1, CrashReporter.Store.ListReports().Count, "A repeating exception (every frame) must not flood the folder.");

            for (int i = 0; i < CrashReporter.MaxReportsPerSession + 2; i++)
            {
                LogAssert.Expect(LogType.Exception, new Regex("diferente " + i));
                Debug.LogException(new Exception("diferente " + i));
            }

            Assert.AreEqual(CrashReporter.MaxReportsPerSession, CrashReporter.ReportsWrittenThisSession);
        }

        [Test]
        public void SplitCategory_ParsesGameLoggerPrefix()
        {
            CrashReporter.SplitCategory("[Save] Slot 2 corrompido", LogType.Error, out string category, out string message);
            Assert.AreEqual("Save", category);
            Assert.AreEqual("Slot 2 corrompido", message);

            CrashReporter.SplitCategory("NullReferenceException: x", LogType.Exception, out category, out message);
            Assert.AreEqual("Exception", category);
            Assert.AreEqual("NullReferenceException: x", message);

            CrashReporter.SplitCategory("[] vazio", LogType.Log, out category, out _);
            Assert.AreEqual("Unity", category);
        }

        [Test]
        public void CollectDeviceInfo_HasNoDeviceNameOrUniqueId()
        {
            string lines = string.Join("\n", CrashReporter.CollectDeviceInfo().Select(l => l.Key + ": " + l.Value));

            if (!string.IsNullOrEmpty(SystemInfo.deviceUniqueIdentifier) && SystemInfo.deviceUniqueIdentifier != SystemInfo.unsupportedIdentifier)
            {
                StringAssert.DoesNotContain(SystemInfo.deviceUniqueIdentifier, lines);
            }

            StringAssert.Contains("Sistema:", lines);
            StringAssert.Contains("GPU:", lines);
        }

        // ------------------------------------------------------------------
        // CrashNoticeBanner
        // ------------------------------------------------------------------

        [Test]
        public void Banner_ShowsOnlyWithANoticeAndAReport()
        {
            Assert.IsFalse(CrashNoticeBanner.ShouldShow(CrashNoticeKind.None, true));
            Assert.IsFalse(CrashNoticeBanner.ShouldShow(CrashNoticeKind.UnexpectedClose, false));
            Assert.IsTrue(CrashNoticeBanner.ShouldShow(CrashNoticeKind.UnexpectedClose, true));
            Assert.AreEqual(CrashNoticeTexts.UnexpectedClose, CrashNoticeBanner.MessageKey(CrashNoticeKind.UnexpectedClose));
            Assert.AreEqual(CrashNoticeTexts.ErrorLogged, CrashNoticeBanner.MessageKey(CrashNoticeKind.ErrorLogged));
            Assert.AreEqual("O jogo fechou de forma inesperada da última vez.", CrashNoticeTexts.GetFallback(CrashNoticeTexts.UnexpectedClose));

            var parent = new VisualElement();
            var banner = new CrashNoticeBanner { NoticeProvider = () => CrashNoticeKind.None, ReportProvider = () => "r" };
            Assert.IsFalse(banner.Attach(parent));
            Assert.AreEqual(0, parent.childCount);
        }

        [Test]
        public void Banner_CopyPutsTheReportOnTheClipboard_DismissRemovesIt()
        {
            string clipboard = null;
            bool dismissed = false;
            var parent = new VisualElement();
            var banner = new CrashNoticeBanner
            {
                NoticeProvider = () => CrashNoticeKind.UnexpectedClose,
                ReportProvider = () => "conteúdo do relatório",
                CopyToClipboard = text => clipboard = text,
                OnDismissed = () => dismissed = true
            };

            // Another test may have left a LocalizationManager registered: expect whatever the banner resolves.
            ServiceLocator.TryGet(out LocalizationManager localization);

            Assert.IsTrue(banner.Attach(parent));
            Assert.IsTrue(banner.IsShown);
            Assert.IsNotNull(parent.Q<Button>(CrashNoticeBanner.CopyButtonName));
            Assert.AreEqual(CrashNoticeTexts.Get(localization, CrashNoticeTexts.UnexpectedClose),
                parent.Q<Label>(className: CrashNoticeBanner.MessageClass).text);

            Assert.IsTrue(banner.CopyReport());
            Assert.AreEqual("conteúdo do relatório", clipboard);
            Assert.AreEqual(CrashNoticeTexts.Get(localization, CrashNoticeTexts.Copied), parent.Q<Button>(CrashNoticeBanner.CopyButtonName).text);

            banner.Dismiss();
            Assert.IsTrue(dismissed);
            Assert.IsFalse(banner.IsShown);
            Assert.AreEqual(0, parent.childCount);
        }
    }
}
