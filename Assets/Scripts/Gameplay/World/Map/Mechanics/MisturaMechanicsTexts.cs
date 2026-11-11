using System.Collections.Generic;
using System.Linq;
using Rapadura.Core.Localization;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>Localization (en + pt-BR) of the Margens da Mistura mechanics. Same self-contained pattern as <see cref="MarGeladoMechanicsTexts"/>.</summary>
    public static class MisturaMechanicsTexts
    {
        public const string BubbleSlow = "ui.mistura.bubble_slow";
        public const string BubbleFast = "ui.mistura.bubble_fast";
        public const string TarSticky = "ui.mistura.tar_sticky";
        public const string TarLit = "ui.mistura.tar_lit";
        public const string TarFuse = "ui.mistura.tar_fuse";
        public const string TarBurns = "ui.mistura.tar_burns";
        public const string BreathWarning = "ui.mistura.breath_warning";
        public const string BreathTilts = "ui.mistura.breath_tilts";
        public const string BreathHeld = "ui.mistura.breath_held";

        private static List<LocalizationEntry> _entries;
        private static Dictionary<string, string> _fallback;

        public static IReadOnlyList<LocalizationEntry> Entries => _entries ??= new List<LocalizationEntry>
        {
            new LocalizationEntry(BubbleSlow, "Time drags here, for you and the beasts", "O tempo se arrasta aqui, para você e para os bichos"),
            new LocalizationEntry(BubbleFast, "Time rushes here, for you and the beasts", "O tempo corre aqui, para você e para os bichos"),
            new LocalizationEntry(TarSticky, "The pitch clings. Fire would burn it away", "O piche gruda. Fogo queimaria isto"),
            new LocalizationEntry(TarLit, "The pitch caught: the way opens for 30 seconds", "O piche pegou: o caminho abre por 30 segundos"),
            new LocalizationEntry(TarFuse, "The pitch is about to burn: get away!", "O piche vai queimar: saia daí!"),
            new LocalizationEntry(TarBurns, "The burning pitch scalds you!", "O piche em chamas queima você!"),
            new LocalizationEntry(BreathWarning, "The shoulder is about to breathe: hold on to a stone!", "O ombro vai respirar: segure numa pedra!"),
            new LocalizationEntry(BreathTilts, "The ground tilts and you slide!", "O chão inclina e você escorrega!"),
            new LocalizationEntry(BreathHeld, "The stone holds you", "A pedra segura você"),
        };

        public static string Get(LocalizationManager manager, string key)
        {
            if (manager != null)
            {
                if (!manager.HasKey(BubbleSlow))
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
