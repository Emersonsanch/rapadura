using System.Collections.Generic;
using System.Linq;
using Rapadura.Core.Localization;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>Localization (en + pt-BR) of the Areias Flamejantes sandstorm. Same self-contained pattern as <c>OverworldTexts</c>.</summary>
    public static class AreiasMechanicsTexts
    {
        public const string StormWarning = "ui.areias.storm_warning";
        public const string StormHit = "ui.areias.storm_hit";
        public const string StormSheltered = "ui.areias.storm_sheltered";
        public const string StormPassed = "ui.areias.storm_passed";

        private static List<LocalizationEntry> _entries;
        private static Dictionary<string, string> _fallback;

        public static IReadOnlyList<LocalizationEntry> Entries => _entries ??= new List<LocalizationEntry>
        {
            new LocalizationEntry(StormWarning, "A sandstorm is coming: run for a campfire, a wreck or a ruin!", "A tempestade de areia vem aí: corra para uma fogueira, uma carroça ou uma ruína!"),
            new LocalizationEntry(StormHit, "The sand is flaying you: get to shelter!", "A areia está te esfolando: procure abrigo!"),
            new LocalizationEntry(StormSheltered, "Sheltered: the sand goes by", "Abrigado: a areia passa"),
            new LocalizationEntry(StormPassed, "The storm has passed", "A tempestade passou"),
        };

        public static string Get(LocalizationManager manager, string key)
        {
            if (manager != null)
            {
                if (!manager.HasKey(StormWarning))
                {
                    manager.LoadEntries(Entries.Where(entry => !manager.HasKey(entry.Key)).ToList());
                }

                if (manager.HasKey(key))
                {
                    return manager.Get(key);
                }
            }

            _fallback ??= Entries.ToDictionary(entry => entry.Key, entry => entry.PtBR);
            return key != null && _fallback.TryGetValue(key, out string text) ? text : key;
        }
    }
}
