using System.Collections.Generic;
using System.Linq;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Characters;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// Localization (en + pt-BR) of the Bomba de Água (68, <see cref="WaterBombRules"/>): what the splash hardened, and one line for each cousin the
    /// first time it cools the lava in a visit. Same self-contained pattern as <see cref="HinokuraPuzzleTexts"/>.
    /// </summary>
    public static class WaterBombTexts
    {
        public const string Wake = "ui.water_bomb.wake";
        public const string Eruption = "ui.water_bomb.eruption";
        public const string Heat = "ui.water_bomb.heat";
        public const string NoLava = "ui.water_bomb.no_lava";
        public const string Dried = "ui.water_bomb.dried";

        /// <summary>The Grande Tremor's spill (65): said by the splash when it lands on a burning spill, and by the Tremor when the crust holds the hero.</summary>
        public const string Spill = "ui.water_bomb.spill";

        private const string LineMaithe = "ui.water_bomb.line_maithe";
        private const string LineIcaro = "ui.water_bomb.line_icaro";
        private const string LineJoaquim = "ui.water_bomb.line_joaquim";
        private const string LineMaria = "ui.water_bomb.line_maria";
        private const string LineLavine = "ui.water_bomb.line_lavine";

        private static List<LocalizationEntry> _entries;
        private static Dictionary<string, string> _fallback;

        public static IReadOnlyList<LocalizationEntry> Entries => _entries ??= new List<LocalizationEntry>
        {
            new LocalizationEntry(Wake, "The water hisses on the turtle's wake: the lava hardens into black glass for {0} s", "A água chia no rastro da tartaruga: a lava endurece em vidro preto por {0} s"),
            new LocalizationEntry(Eruption, "The water hisses on the flow: a crust of stone holds under your feet for {0} s", "A água chia no derrame: uma casca de pedra segura debaixo dos pés por {0} s"),
            new LocalizationEntry(Heat, "Steam and cold stone: the heat stops while the water lasts ({0} s)", "Vapor e pedra fria: o calor para enquanto a água dura ({0} s)"),
            new LocalizationEntry(NoLava, "The water soaks into the ground: there is no lava here to harden", "A água se espalha no chão: aqui não há lava para endurecer"),
            new LocalizationEntry(Spill, "The water hisses on the spilled lava: a crust holds you over the overflow while it lasts",
                "A água chia na lava transbordada: a casca segura você sobre o transbordo enquanto dura"),
            new LocalizationEntry(Dried, "The crust cracks: the water has dried", "A casca racha: a água secou"),
            new LocalizationEntry(LineMaithe, "Maithe: Lava plus water equals a floor. I'm writing that one down.", "Maithe: Lava mais água dá chão. Essa eu vou anotar."),
            new LocalizationEntry(LineIcaro, "Ícaro: Living Water over the fire that eats. It doesn't put it out, it gives it a shape.", "Ícaro: Água Viva sobre o fogo que devora. Não apaga, dá forma."),
            new LocalizationEntry(LineJoaquim, "Joaquim: Vó Edna cooled the hot pan just like this. Hsss — and done.", "Joaquim: A Vó Edna esfriava a panela quente assim. Tsss — e pronto."),
            new LocalizationEntry(LineMaria, "Maria: Twenty seconds of glass floor. Run first, admire later!", "Maria: Vinte segundos de chão de vidro. Corre primeiro, admira depois!"),
            new LocalizationEntry(LineLavine, "Lavine: The steam smells like rain. Even here.", "Lavine: O vapor cheira a chuva. Até aqui."),
        };

        /// <summary>The cousin's line when a bomb first cools the lava (null when no cousin is known).</summary>
        public static string LineFor(CharacterId? hero)
        {
            switch (hero)
            {
                case CharacterId.Maithe: return LineMaithe;
                case CharacterId.Icaro: return LineIcaro;
                case CharacterId.Joaquim: return LineJoaquim;
                case CharacterId.Maria: return LineMaria;
                case CharacterId.Lavine: return LineLavine;
                default: return null;
            }
        }

        public static string Format(LocalizationManager manager, string key, params object[] args) => string.Format(Get(manager, key), args);

        public static string Get(LocalizationManager manager, string key)
        {
            if (manager != null)
            {
                if (!manager.HasKey(Wake))
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
