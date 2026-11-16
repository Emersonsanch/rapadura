using System.Collections.Generic;
using System.Linq;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Characters;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// Localization (en + pt-BR) of the season of the Tundra de Omöl (61, <see cref="TundraSeasonRules"/>): what the ground is like on the way in,
    /// with Yen (the good brother of the Komi tale, whose tent is the map's post) and one line per cousin. Same self-contained pattern as
    /// <c>DrumRhythmTexts</c>.
    /// </summary>
    public static class TundraSeasonTexts
    {
        public const string Summer = "ui.tundra_season.summer";
        public const string Autumn = "ui.tundra_season.autumn";
        public const string Winter = "ui.tundra_season.winter";
        public const string Spring = "ui.tundra_season.spring";
        public const string Changed = "ui.tundra_season.changed";
        public const string LineMaithe = "ui.tundra_season.line_maithe";
        public const string LineIcaro = "ui.tundra_season.line_icaro";
        public const string LineJoaquim = "ui.tundra_season.line_joaquim";
        public const string LineMaria = "ui.tundra_season.line_maria";
        public const string LineLavine = "ui.tundra_season.line_lavine";

        private static List<LocalizationEntry> _entries;
        private static Dictionary<string, string> _fallback;

        public static IReadOnlyList<LocalizationEntry> Entries => _entries ??= new List<LocalizationEntry>
        {
            new LocalizationEntry(Summer, "Summer on Omöl's tundra: the moss is a bog (speed −20%) · next season in {0} day(s)", "Verão na tundra de Omöl: o musgo virou brejo (velocidade −20%) · próxima estação em {0} dia(s)"),
            new LocalizationEntry(Autumn, "Autumn on Omöl's tundra: firm ground · winter in {0} day(s)", "Outono na tundra de Omöl: chão firme · inverno em {0} dia(s)"),
            new LocalizationEntry(Winter, "Winter on Omöl's tundra: packed snow, fast going (speed +15%) · next season in {0} day(s)", "Inverno na tundra de Omöl: neve batida, anda-se rápido (velocidade +15%) · próxima estação em {0} dia(s)"),
            new LocalizationEntry(Spring, "Spring on Omöl's tundra: firm ground · summer in {0} day(s)", "Primavera na tundra de Omöl: chão firme · verão em {0} dia(s)"),
            new LocalizationEntry(Changed, "Yen: \"The season turned with the moon. Mind the ground.\"", "Yen: \"A estação virou com a lua. Cuidado com o chão.\""),
            new LocalizationEntry(LineMaithe, "Maithe: In the bog every step goes squelch. Gross. Fun, but gross.", "Maithe: No brejo todo passo faz chuac. Nojento. Divertido, mas nojento."),
            new LocalizationEntry(LineIcaro, "Ícaro: The tundra follows the canals' moon. Frozen canals, frozen tundra. Makes sense.", "Ícaro: A tundra segue a lua dos canais. Canal congelado, tundra congelada. Faz sentido."),
            new LocalizationEntry(LineJoaquim, "Joaquim: Vô Jorge always said: in the wet season, walk where the cows walk.", "Joaquim: O Vô Jorge sempre dizia: no tempo das águas, pise onde o boi pisa."),
            new LocalizationEntry(LineMaria, "Maria: Snow this hard is basically a road. Race you!", "Maria: Neve dura assim é praticamente estrada. Aposto corrida!"),
            new LocalizationEntry(LineLavine, "Lavine: Omöl made the clumsy things, but he made this tundra beautiful in every season.", "Lavine: O Omöl fez as coisas desajeitadas, mas fez esta tundra bonita em toda estação."),
        };

        public static string SeasonKey(TundraSeason season)
        {
            switch (season)
            {
                case TundraSeason.Summer: return Summer;
                case TundraSeason.Autumn: return Autumn;
                case TundraSeason.Winter: return Winter;
                default: return Spring;
            }
        }

        /// <summary>The cousin's line on entering the tundra (null when no cousin is known).</summary>
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
                if (!manager.HasKey(Summer))
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
