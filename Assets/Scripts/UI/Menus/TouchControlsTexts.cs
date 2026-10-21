using System.Collections.Generic;
using System.Linq;
using Rapadura.Core.Localization;

namespace Rapadura.UI.Menus
{
    /// <summary>
    /// Localization keys (en + pt-BR) for the touch rows of the Settings "Controles" tab
    /// (<see cref="TouchControlsSettingsSection"/>, plus the touch-only status that replaces the
    /// keyboard/gamepad rebind table on phones). Same self-contained pattern as
    /// <see cref="HapticsTexts"/>: merged into the live <see cref="LocalizationManager"/> on first use
    /// (keys it already has win), pt-BR fallback otherwise.
    /// </summary>
    public static class TouchControlsTexts
    {
        public const string SettingsVirtualJoystick = "ui.settings.controls.virtual_joystick";
        public const string SettingsVirtualJoystickTouchOnly = "ui.settings.controls.virtual_joystick_touch_only";
        public const string StatusTouchOnly = "ui.settings.controls.status.touch_only";

        public static readonly List<LocalizationEntry> Entries = new List<LocalizationEntry>
        {
            new LocalizationEntry(SettingsVirtualJoystick, "Virtual joystick", "Joystick virtual"),
            new LocalizationEntry(SettingsVirtualJoystickTouchOnly, "Virtual joystick (touch screens only)", "Joystick virtual (só em tela de toque)"),
            new LocalizationEntry(StatusTouchOnly,
                "On a touch screen, tap or hold the ground to walk and use the on-screen buttons. Connect a keyboard or gamepad to remap keys.",
                "Na tela de toque, toque ou segure no chão para andar e use os botões da tela. Conecte um teclado ou controle para remapear."),
        };

        private static Dictionary<string, string> _fallbackByKey;
        private static LocalizationManager _mergedInto;

        /// <summary>Merges <see cref="Entries"/> into <paramref name="manager"/>, skipping keys it already has. Safe to call repeatedly.</summary>
        public static void EnsureRegistered(LocalizationManager manager)
        {
            if (manager == null || (ReferenceEquals(manager, _mergedInto) && manager.HasKey(SettingsVirtualJoystick)))
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
