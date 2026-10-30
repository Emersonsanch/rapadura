using Rapadura.Core.Logging;
using UnityEditor;
using UnityEngine;

namespace Rapadura.Editor
{
    /// <summary>
    /// Editor switches for the local crash reporter (<see cref="CrashReporter"/>), which stays off in the
    /// Editor by default so tests that log errors on purpose leave no reports or title notices behind:
    /// <list type="bullet">
    /// <item><c>Rapadura/Debug/Relatório de falhas no Editor</c> toggles it (takes effect on the next Play);</item>
    /// <item><c>Rapadura/Debug/Simular relatório de falha</c> (Play Mode, reporter on) writes a report as an
    /// uncaught exception would — stop and press Play again to see the title screen's notice;</item>
    /// <item><c>Rapadura/Debug/Abrir pasta de relatórios de falha</c> reveals persistentDataPath/CrashReports.</item>
    /// </list>
    /// </summary>
    public static class CrashReporterMenu
    {
        private const string ToggleMenu = "Rapadura/Debug/Relatório de falhas no Editor";

        [MenuItem(ToggleMenu)]
        private static void Toggle()
        {
            CrashReporter.EnabledInEditor = !CrashReporter.EnabledInEditor;
            Debug.Log($"[CrashReporter] {(CrashReporter.EnabledInEditor ? "Ligado" : "Desligado")} no Editor (vale a partir do próximo Play).");
        }

        [MenuItem(ToggleMenu, true)]
        private static bool ToggleValidate()
        {
            Menu.SetChecked(ToggleMenu, CrashReporter.EnabledInEditor);
            return true;
        }

        [MenuItem("Rapadura/Debug/Simular relatório de falha")]
        private static void Simulate()
        {
            string path = CrashReporter.WriteReportNow("SimulatedCrashException: relatório de teste gerado pelo menu do Editor.",
                "Rapadura.Editor.CrashReporterMenu.Simulate ()");
            if (path == null)
            {
                Debug.LogWarning("[CrashReporter] Entre no Play Mode com \"" + ToggleMenu + "\" ligado para simular.");
                return;
            }

            Debug.Log("[CrashReporter] Relatório simulado gravado; pare e dê Play de novo para ver o aviso no título.");
        }

        [MenuItem("Rapadura/Debug/Simular relatório de falha", true)]
        private static bool SimulateValidate() => EditorApplication.isPlaying;

        [MenuItem("Rapadura/Debug/Abrir pasta de relatórios de falha")]
        private static void Reveal()
        {
            string folder = new CrashReportStore(Application.persistentDataPath).Folder;
            System.IO.Directory.CreateDirectory(folder);
            EditorUtility.RevealInFinder(folder);
        }
    }
}
