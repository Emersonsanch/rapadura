using System.Collections.Generic;
using System.Linq;
using Rapadura.Core.Localization;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>Localization (en + pt-BR) of the Limiar Profundo mechanics. Same self-contained pattern as <see cref="MisturaMechanicsTexts"/>.</summary>
    public static class ProfundoMechanicsTexts
    {
        public const string LampHint = "ui.profundo.lamp_hint";
        public const string LampLit = "ui.profundo.lamp_lit";
        public const string RingGrew = "ui.profundo.ring_grew";
        public const string TideWarning = "ui.profundo.tide_warning";
        public const string TideRising = "ui.profundo.tide_rising";
        public const string TideBites = "ui.profundo.tide_bites";
        public const string TideHeld = "ui.profundo.tide_held";
        public const string TideFell = "ui.profundo.tide_fell";
        public const string FloorEnter = "ui.profundo.floor_enter";
        public const string FloorWarning = "ui.profundo.floor_warning";
        public const string FloorBurns = "ui.profundo.floor_burns";
        public const string SmokeEnter = "ui.profundo.smoke_enter";
        public const string VentHint = "ui.profundo.vent_hint";
        public const string SmokeCleared = "ui.profundo.smoke_cleared";
        public const string SmokeReturns = "ui.profundo.smoke_returns";

        private static List<LocalizationEntry> _entries;
        private static Dictionary<string, string> _fallback;

        public static IReadOnlyList<LocalizationEntry> Entries => _entries ??= new List<LocalizationEntry>
        {
            new LocalizationEntry(LampHint, "Stay by the lamp to light it", "Fique junto da lâmpada para acendê-la"),
            new LocalizationEntry(LampLit, "The lamp caught", "A lâmpada acendeu"),
            new LocalizationEntry(RingGrew, "A whole ring burns: the safe ground grows", "Um anel inteiro aceso: a zona segura cresce"),
            new LocalizationEntry(TideWarning, "The Tide of Darkness is coming: get under the lamps!", "A Maré de Treva vem aí: venha para baixo das lâmpadas!"),
            new LocalizationEntry(TideRising, "The shadow rose: get to the light!", "A sombra subiu: vá para a luz!"),
            new LocalizationEntry(TideBites, "The shadow gnaws at you outside the light", "A sombra rói você fora da luz"),
            new LocalizationEntry(TideHeld, "The wall held: the Mother Lamp restores you", "A muralha aguentou: a Lâmpada-Mãe restaura você"),
            new LocalizationEntry(TideFell, "The tide ebbs, and the lamps are out: light them again", "A maré recua e as lâmpadas se apagaram: acenda de novo"),
            new LocalizationEntry(FloorEnter, "The floor lights in patterns: keep to the dark stones", "O chão acende em desenhos: fique nas pedras escuras"),
            new LocalizationEntry(FloorWarning, "The stone under you is about to light!", "A pedra embaixo de você vai acender!"),
            new LocalizationEntry(FloorBurns, "The burning stone scalds you!", "A pedra acesa queima você!"),
            new LocalizationEntry(SmokeEnter, "The smoke chokes you. A fan would clear it", "A fumaça sufoca você. Um ventilador limparia isto"),
            new LocalizationEntry(VentHint, "Stay by the fan to start it", "Fique junto do ventilador para ligá-lo"),
            new LocalizationEntry(SmokeCleared, "The wind clears the smoke for 30 seconds", "O vento limpa a fumaça por 30 segundos"),
            new LocalizationEntry(SmokeReturns, "The smoke seeps back", "A fumaça está voltando"),
        };

        public static string Get(LocalizationManager manager, string key)
        {
            if (manager != null)
            {
                if (!manager.HasKey(LampHint))
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
