using System.Collections.Generic;
using System.Linq;
using Rapadura.Core.Localization;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// Localization (en + pt-BR) of the waking stones (21), the low fire of the savanna (26), the calves' glow (34) and the wind vanes (39).
    /// Same self-contained pattern as <c>OverworldTexts</c>.
    /// </summary>
    public static class StonesFireGazeWindTexts
    {
        public const string StonePrompt = "ui.route2.stone_prompt";
        public const string StoneLit = "ui.route2.stone_lit";
        public const string StoneSpent = "ui.route2.stone_spent";
        public const string FireAsh = "ui.route2.fire_ash";
        public const string FireBurning = "ui.route2.fire_burning";
        public const string GazeWarning = "ui.route2.gaze_warning";
        public const string GazeHit = "ui.route2.gaze_hit";
        public const string VanePrompt = "ui.route2.vane_prompt";
        public const string VaneTurned = "ui.route2.vane_turned";
        public const string WindFull = "ui.route2.wind_full";
        public const string WindWith = "ui.route2.wind_with";
        public const string WindAgainst = "ui.route2.wind_against";

        private static List<LocalizationEntry> _entries;
        private static Dictionary<string, string> _fallback;

        public static IReadOnlyList<LocalizationEntry> Entries => _entries ??= new List<LocalizationEntry>
        {
            new LocalizationEntry(StonePrompt, "[E] Touch the dragon stone", "[E] Tocar a pedra-dragão"),
            new LocalizationEntry(StoneLit, "The spring wakes and heals… and so does a young vishap!", "A fonte acorda e cura… e um vishap jovem também!"),
            new LocalizationEntry(StoneSpent, "The stone sleeps again until tomorrow", "A pedra voltou a dormir até amanhã"),
            new LocalizationEntry(FireAsh, "Burnt grass: a fast trail, and the beasts shy from the ash", "Capim queimado: trilha rápida, e os bichos fogem da cinza"),
            new LocalizationEntry(FireBurning, "The low fire renews the grass: it does not burn you", "O fogo baixo renova o capim: ele não te queima"),
            new LocalizationEntry(GazeWarning, "A calf is shining: turn your back!", "Um filhote está brilhando: vire de costas!"),
            new LocalizationEntry(GazeHit, "You looked: the calf's shame slows you", "Você olhou: a vergonha do filhote te deixa lento"),
            new LocalizationEntry(VanePrompt, "[E] Turn the wind vane", "[E] Girar o cata-vento"),
            new LocalizationEntry(VaneTurned, "The vane turns: the wind changes", "O cata-vento gira: o vento muda"),
            new LocalizationEntry(WindFull, "All four vanes agree: a good breeze blows through the valley", "Os quatro cata-ventos concordam: a brisa boa volta ao vale"),
            new LocalizationEntry(WindWith, "Wind at your back", "Vento a favor"),
            new LocalizationEntry(WindAgainst, "Wind in your face", "Vento contra"),
        };

        public static string Get(LocalizationManager manager, string key)
        {
            if (manager != null)
            {
                if (!manager.HasKey(StonePrompt))
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
