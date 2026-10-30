using System.Collections.Generic;
using System.Linq;
using Rapadura.Core.Localization;

namespace Rapadura.Gameplay.World.Cities.Town
{
    /// <summary>
    /// Localization keys (en + pt-BR) of the walkable cities (Docs/Cities.md §5): the hub's "walk into town" button, the
    /// arrival line and the Cais da Barca call-out. Same self-contained pattern as <c>NightTexts</c>.
    /// </summary>
    public static class CityWalkTexts
    {
        /// <summary>Hub button that loads the walkable town.</summary>
        public const string WalkIn = "ui.city.walk_in";

        /// <summary>Floating line on arrival; {0} = city name.</summary>
        public const string Arrived = "ui.city.walk_arrived";

        /// <summary>Floating line when the hero wakes up at the plaza after a defeat (Docs/Progressao500.md §7); {0} = city name.</summary>
        public const string RespawnedAtPlaza = "ui.city.walk_respawned";

        /// <summary>Floating line at the Cais da Barca, just before going back to the map with the barcas open.</summary>
        public const string DockBoarding = "ui.city.walk_dock";

        /// <summary>Floating line at a gate, on the way back to the map.</summary>
        public const string Leaving = "ui.city.walk_leaving";

        public static readonly List<LocalizationEntry> Entries = new List<LocalizationEntry>
        {
            new LocalizationEntry(WalkIn, "Walk into town", "Andar pela cidade"),
            new LocalizationEntry(Arrived, "{0} · safe zone", "{0} · zona segura"),
            new LocalizationEntry(RespawnedAtPlaza, "Ari found you again. {0} plaza · safe zone", "O Ari te achou de novo. Praça de {0} · zona segura"),
            new LocalizationEntry(DockBoarding, "To the Ship Dock: choose where the barca goes", "Ao Cais da Barca: escolha para onde a barca vai"),
            new LocalizationEntry(Leaving, "Back to the road", "De volta à estrada"),
        };

        private static Dictionary<string, string> _fallbackByKey;
        private static LocalizationManager _mergedInto;

        public static void EnsureRegistered(LocalizationManager manager)
        {
            if (manager == null || (ReferenceEquals(manager, _mergedInto) && manager.HasKey(WalkIn)))
            {
                return;
            }

            manager.LoadEntries(Entries.Where(entry => !manager.HasKey(entry.Key)).ToList());
            _mergedInto = manager;
        }

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
