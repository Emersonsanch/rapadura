using System.Collections.Generic;
using System.Linq;
using Rapadura.Core.Localization;

namespace Rapadura.UI.Menus
{
    /// <summary>
    /// Localization keys (en + pt-BR) for the title screen's "the game closed unexpectedly" banner
    /// (<see cref="CrashNoticeBanner"/>). Same self-contained pattern as <see cref="ReturnToTitleTexts"/>:
    /// merged into the live <see cref="LocalizationManager"/> on first use (keys it already has win),
    /// pt-BR fallback when no manager is registered.
    /// </summary>
    public static class CrashNoticeTexts
    {
        public const string UnexpectedClose = "ui.menu.crash_notice.unexpected_close";
        public const string ErrorLogged = "ui.menu.crash_notice.error_logged";
        public const string Hint = "ui.menu.crash_notice.hint";
        public const string Copy = "ui.menu.crash_notice.copy";
        public const string Copied = "ui.menu.crash_notice.copied";
        public const string Dismiss = "ui.menu.crash_notice.dismiss";

        public static readonly List<LocalizationEntry> Entries = new List<LocalizationEntry>
        {
            new LocalizationEntry(UnexpectedClose, "The game closed unexpectedly last time.", "O jogo fechou de forma inesperada da última vez."),
            new LocalizationEntry(ErrorLogged, "The game ran into an error last time.", "O jogo encontrou um erro da última vez."),
            new LocalizationEntry(Hint, "The report holds no personal data. Paste it into your bug report.", "O relatório não tem dados pessoais. Cole-o no seu relato do problema."),
            new LocalizationEntry(Copy, "Copy report", "Copiar relatório"),
            new LocalizationEntry(Copied, "Copied!", "Copiado!"),
            new LocalizationEntry(Dismiss, "Close", "Fechar"),
        };

        private static Dictionary<string, string> _fallbackByKey;
        private static LocalizationManager _mergedInto;

        /// <summary>Merges <see cref="Entries"/> into <paramref name="manager"/>, skipping keys it already has. Safe to call repeatedly.</summary>
        public static void EnsureRegistered(LocalizationManager manager)
        {
            if (manager == null || (ReferenceEquals(manager, _mergedInto) && manager.HasKey(UnexpectedClose)))
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
