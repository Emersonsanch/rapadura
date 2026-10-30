using System.Collections.Generic;
using System.Linq;
using Rapadura.Core.Localization;

namespace Rapadura.Gameplay.Night
{
    /// <summary>
    /// Localization keys (en + pt-BR) of the day/night danger feature: the HUD night indicator, the "(noturno)" name
    /// suffix, the reinforcement call-out and the playable night species' names. Same self-contained pattern as
    /// <c>BossTexts</c>: merged into the live <see cref="LocalizationManager"/> on first use (keys it already has win),
    /// pt-BR fallback otherwise. The "Noite mais clara" accessibility label lives in <c>SettingsMenuTexts</c>.
    /// </summary>
    public static class NightTexts
    {
        /// <summary>HUD indicator while it is night.</summary>
        public const string HudNight = "ui.night.hud.danger";

        /// <summary>Name of a night variant; {0} = the enemy's own name.</summary>
        public const string VariantName = "enemy.night.variant_name";

        /// <summary>Floating call-out when a reinforcement wave arrives.</summary>
        public const string WaveIncoming = "ui.night.wave_incoming";

        /// <summary>Same, at night.</summary>
        public const string WaveIncomingNight = "ui.night.wave_incoming_night";

        public static readonly List<LocalizationEntry> Entries = BuildEntries();

        private static Dictionary<string, string> _fallbackByKey;
        private static LocalizationManager _mergedInto;

        private static List<LocalizationEntry> BuildEntries()
        {
            var entries = new List<LocalizationEntry>
            {
                new LocalizationEntry(HudNight, "Night — the critters are fiercer", "Noite — os bichos estão mais bravos"),
                new LocalizationEntry(VariantName, "{0} (nocturnal)", "{0} (noturno)"),
                new LocalizationEntry(WaveIncoming, "More critters coming!", "Vem mais bicho!"),
                new LocalizationEntry(WaveIncomingNight, "More critters coming out of the dark!", "Vem mais bicho do escuro!"),
            };

            foreach (NightSpecies species in NightVariantRules.PlayableSpecies)
            {
                entries.Add(new LocalizationEntry(species.NameKey, species.NameEn, species.NamePtBR));
            }

            return entries;
        }

        /// <summary>Merges <see cref="Entries"/> into <paramref name="manager"/>, skipping keys it already has. Safe to call repeatedly.</summary>
        public static void EnsureRegistered(LocalizationManager manager)
        {
            if (manager == null || (ReferenceEquals(manager, _mergedInto) && manager.HasKey(HudNight)))
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
