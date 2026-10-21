using System.Collections.Generic;
using System.Linq;
using Rapadura.Core.Localization;

namespace Rapadura.UI.Menus
{
    /// <summary>
    /// Localization keys (en + pt-BR) for the pause menu's "Voltar ao menu?" confirm dialog
    /// (<see cref="PauseMenuController"/>, ReturnToTitleDialog.uxml). Same self-contained pattern as
    /// <see cref="SettingsMenuTexts"/>: merged into the live <see cref="LocalizationManager"/> on first
    /// use (keys it already has win), pt-BR fallback when no manager is registered.
    /// </summary>
    public static class ReturnToTitleTexts
    {
        public const string Title = "ui.pause.return_to_title.title";
        public const string Message = "ui.pause.return_to_title.message";
        public const string SaveAndQuit = "ui.pause.return_to_title.save_and_quit";
        public const string QuitWithoutSaving = "ui.pause.return_to_title.quit_without_saving";
        public const string Cancel = "ui.pause.return_to_title.cancel";

        public static readonly List<LocalizationEntry> Entries = new List<LocalizationEntry>
        {
            new LocalizationEntry(Title, "Return to the main menu?", "Voltar ao menu?"),
            new LocalizationEntry(Message, "Unsaved progress will be lost.", "Progresso não salvo será perdido."),
            new LocalizationEntry(SaveAndQuit, "Save and quit", "Salvar e sair"),
            new LocalizationEntry(QuitWithoutSaving, "Quit without saving", "Sair sem salvar"),
            new LocalizationEntry(Cancel, "Cancel", "Cancelar"),
        };

        private static Dictionary<string, string> _fallbackByKey;
        private static LocalizationManager _mergedInto;

        /// <summary>Merges <see cref="Entries"/> into <paramref name="manager"/>, skipping keys it already has. Safe to call repeatedly.</summary>
        public static void EnsureRegistered(LocalizationManager manager)
        {
            if (manager == null || (ReferenceEquals(manager, _mergedInto) && manager.HasKey(Title)))
            {
                return;
            }

            manager.LoadEntries(Entries.Where(entry => !manager.HasKey(entry.Key)).ToList());
            _mergedInto = manager;
        }

        /// <summary>Localized text via <paramref name="manager"/> when available, else the pt-BR fallback, else the key.</summary>
        public static string Get(LocalizationManager manager, string key)
        {
            if (manager != null)
            {
                EnsureRegistered(manager);
                if (manager.HasKey(key))
                {
                    return manager.Get(key);
                }
            }

            return GetFallback(key) ?? key;
        }

        public static string GetFallback(string key)
        {
            if (_fallbackByKey == null)
            {
                _fallbackByKey = new Dictionary<string, string>();
                foreach (LocalizationEntry entry in Entries)
                {
                    _fallbackByKey[entry.Key] = entry.PtBR;
                }
            }

            return key != null && _fallbackByKey.TryGetValue(key, out string text) ? text : null;
        }
    }
}
