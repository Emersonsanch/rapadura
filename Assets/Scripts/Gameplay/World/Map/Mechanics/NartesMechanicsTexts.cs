using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Rapadura.Core.Localization;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>Localization (en + pt-BR) of the Estepe dos Nartes mechanics. Same self-contained pattern as <see cref="RioMechanicsTexts"/>.</summary>
    public static class NartesMechanicsTexts
    {
        public const string RaceCall = "ui.nartes.race_call";
        public const string RaceCount = "ui.nartes.race_count";
        public const string RaceGo = "ui.nartes.race_go";
        public const string RaceGate = "ui.nartes.race_gate";
        public const string RaceMedal = "ui.nartes.race_medal";
        public const string RaceNoMedal = "ui.nartes.race_no_medal";
        public const string RaceTimeOut = "ui.nartes.race_time_out";
        public const string RaceAbandon = "ui.nartes.race_abandon";
        public const string BagFull = "ui.nartes.bag_full";
        public const string ChainEnter = "ui.nartes.chain_enter";
        public const string ChainMounted = "ui.nartes.chain_mounted";
        public const string ChainWarn = "ui.nartes.chain_warn";
        public const string ChainGust = "ui.nartes.chain_gust";
        public const string ChainFall = "ui.nartes.chain_fall";
        public const string ChainTop = "ui.nartes.chain_top";
        public const string ChainTopDone = "ui.nartes.chain_top_done";
        public const string GoldCall = "ui.nartes.gold_call";
        public const string GoldHot = "ui.nartes.gold_hot";
        public const string GoldCooled = "ui.nartes.gold_cooled";
        public const string GoldTaken = "ui.nartes.gold_taken";

        /// <summary>The medal names, indexed by <see cref="NartesRaceMedal"/> (None is unused).</summary>
        public static readonly string[] Medals = { "ui.nartes.medal.none", "ui.nartes.medal.bronze", "ui.nartes.medal.silver", "ui.nartes.medal.gold" };

        /// <summary>The four Golds of Heaven, in the order of <c>OverworldLayouts.NartesSkyGolds</c>.</summary>
        public static readonly string[] Golds = { "ui.nartes.gold.plough", "ui.nartes.gold.yoke", "ui.nartes.gold.axe", "ui.nartes.gold.cup" };

        private static List<LocalizationEntry> _entries;
        private static Dictionary<string, string> _fallback;

        public static IReadOnlyList<LocalizationEntry> Entries => _entries ??= new List<LocalizationEntry>
        {
            new LocalizationEntry(RaceCall, "[E] Race the centaurs round the tracks", "[E] Correr com os centauros nas pistas"),
            new LocalizationEntry(RaceCount, "{0}...", "{0}..."),
            new LocalizationEntry(RaceGo, "Go! Follow the golden gates", "Vai! Siga as portas douradas"),
            new LocalizationEntry(RaceGate, "Gate {0}/{1} · {2} s", "Porta {0}/{1} · {2} s"),
            new LocalizationEntry(RaceMedal, "{0} medal in {1} s! +{2} gold", "Medalha de {0} em {1} s! +{2} de ouro"),
            new LocalizationEntry(RaceNoMedal, "Finished in {0} s. The centaurs laugh, kindly", "Chegou em {0} s. Os centauros riem, sem maldade"),
            new LocalizationEntry(RaceTimeOut, "Out of time: the centaurs have gone to eat", "O tempo acabou: os centauros foram comer"),
            new LocalizationEntry(RaceAbandon, "Race abandoned", "Corrida abandonada"),
            new LocalizationEntry(BagFull, "Your backpack is full: the prize stays here", "Sua mochila está cheia: o prêmio fica aqui"),
            new LocalizationEntry(ChainEnter, "Amirani's chain: wind crosses the links, hold on at the rings", "Corrente de Amirani: o vento cruza os elos, segure nas argolas"),
            new LocalizationEntry(ChainMounted, "The links are too narrow for a mount", "Os elos são estreitos demais para a montaria"),
            new LocalizationEntry(ChainWarn, "Wind coming! Reach a ring", "Vento vindo! Chegue a uma argola"),
            new LocalizationEntry(ChainGust, "The gust tears at you", "A rajada puxa você"),
            new LocalizationEntry(ChainFall, "Blown off the links, down to the ring below", "O vento jogou você dos elos, até a argola de baixo"),
            new LocalizationEntry(ChainTop, "The top of the chain! +{0} gold", "O alto da corrente! +{0} de ouro"),
            new LocalizationEntry(ChainTopDone, "The top of the chain. Its gold comes back tomorrow", "O alto da corrente. O ouro volta amanhã"),
            new LocalizationEntry(GoldCall, "[E] Lift {0}", "[E] Levantar {0}"),
            new LocalizationEntry(GoldHot, "{0}: too hot to touch. Maybe for someone smaller", "{0}: quente demais. Talvez para alguém menor"),
            new LocalizationEntry(GoldCooled, "{0} is resting. Come back tomorrow", "{0} está descansando. Volte amanhã"),
            new LocalizationEntry(GoldTaken, "\"I'm not small!\" +{0} gold", "\"Eu não sou pequena!\" +{0} de ouro"),
            new LocalizationEntry(Medals[0], "none", "nenhuma"),
            new LocalizationEntry(Medals[1], "bronze", "bronze"),
            new LocalizationEntry(Medals[2], "silver", "prata"),
            new LocalizationEntry(Medals[3], "gold", "ouro"),
            new LocalizationEntry(Golds[0], "the golden plough", "o arado de ouro"),
            new LocalizationEntry(Golds[1], "the golden yoke", "a canga de ouro"),
            new LocalizationEntry(Golds[2], "the golden axe", "o machado de ouro"),
            new LocalizationEntry(Golds[3], "the golden cup", "a taça de ouro"),
        };

        public static string Get(LocalizationManager manager, string key)
        {
            if (manager != null)
            {
                if (!manager.HasKey(RaceCall))
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

        public static string Format(LocalizationManager manager, string key, params object[] arguments) => string.Format(CultureInfo.InvariantCulture, Get(manager, key), arguments);
    }
}
