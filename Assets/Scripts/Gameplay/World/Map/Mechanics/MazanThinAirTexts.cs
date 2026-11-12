using System.Collections.Generic;
using System.Linq;
using Rapadura.Core.Localization;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>Localization (en + pt-BR) of the thin air of the Cume dos Mazans (18). Same self-contained pattern as <c>OverworldTexts</c>.</summary>
    public static class MazanThinAirTexts
    {
        public const string ThinAir = "ui.kaggen.thin_air";
        public const string ThinAirMaithe = "ui.kaggen.thin_air_maithe";
        public const string ThinAirEnd = "ui.kaggen.thin_air_end";

        private static List<LocalizationEntry> _entries;
        private static Dictionary<string, string> _fallback;

        public static IReadOnlyList<LocalizationEntry> Entries => _entries ??= new List<LocalizationEntry>
        {
            new LocalizationEntry(ThinAir, "Thin air up here: your breath comes back slower", "Ar fino aqui em cima: o fôlego volta mais devagar"),
            new LocalizationEntry(ThinAirMaithe, "The summit wind is Maithe's own: she breathes easy", "O vento do cume é da Maithe: ela respira tranquila"),
            new LocalizationEntry(ThinAirEnd, "Thicker air: your breath comes back as usual", "Ar mais grosso: o fôlego volta como sempre"),
        };

        public static string Get(LocalizationManager manager, string key)
        {
            if (manager != null)
            {
                if (!manager.HasKey(ThinAir))
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
