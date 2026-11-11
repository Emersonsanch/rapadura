using System.Collections.Generic;
using System.Linq;
using Rapadura.Core.Localization;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>Localization (en + pt-BR) of the mechanics of Regions 5–7. Same self-contained pattern as <see cref="ProfundoMechanicsTexts"/>.</summary>
    public static class CostaDeltaTetoMechanicsTexts
    {
        public const string TideLow = "ui.costa.tide_low";
        public const string TideTurning = "ui.costa.tide_turning";
        public const string TideHigh = "ui.costa.tide_high";
        public const string TideChestPrompt = "ui.costa.tide_chest_prompt";
        public const string TideChestSpent = "ui.costa.tide_chest_spent";
        public const string RewardFound = "ui.costa.reward_found";
        public const string RewardBagFull = "ui.costa.reward_bag_full";
        public const string IslandsDrift = "ui.delta.islands_drift";
        public const string BogEnter = "ui.delta.bog_enter";
        public const string IslandBoard = "ui.delta.island_board";
        public const string SparkPrompt = "ui.delta.spark_prompt";
        public const string AvalancheWarning = "ui.teto.avalanche_warning";
        public const string AvalancheHit = "ui.teto.avalanche_hit";
        public const string AvalanchePassed = "ui.teto.avalanche_passed";

        private static List<LocalizationEntry> _entries;
        private static Dictionary<string, string> _fallback;

        public static IReadOnlyList<LocalizationEntry> Entries => _entries ??= new List<LocalizationEntry>
        {
            new LocalizationEntry(TideLow, "The tide is out: the reef flats are open", "A maré baixou: os baixios do recife estão abertos"),
            new LocalizationEntry(TideTurning, "The tide is turning: leave the flats within 40 seconds!", "A maré vai virar: saia dos baixios em 40 segundos!"),
            new LocalizationEntry(TideHigh, "High tide: the water drags you off the flat", "Maré alta: a água arrasta você do baixio"),
            new LocalizationEntry(TideChestPrompt, "[E] Open the reef chest", "[E] Abrir o baú do recife"),
            new LocalizationEntry(TideChestSpent, "This chest is empty until the next tide", "Este baú está vazio até a próxima maré"),
            new LocalizationEntry(RewardFound, "You found: {0}", "Você achou: {0}"),
            new LocalizationEntry(RewardBagFull, "Your bag is full: no room for {0}", "Sua bolsa está cheia: não coube {0}"),
            new LocalizationEntry(IslandsDrift, "The reed islands are drifting to new places", "As ilhas de junco estão indo para outros lugares"),
            new LocalizationEntry(BogEnter, "The bog sucks at your feet: keep to the reed islands", "O lodo puxa seus pés: fique nas ilhas de junco"),
            new LocalizationEntry(IslandBoard, "Firm reeds: the island carries you along", "Junco firme: a ilha leva você junto"),
            new LocalizationEntry(SparkPrompt, "[E] Free the caged spark", "[E] Libertar a faísca presa"),
            new LocalizationEntry(AvalancheWarning, "The avalanche bell! Get out of the marked chute!", "O sino da avalanche! Saia da calha marcada!"),
            new LocalizationEntry(AvalancheHit, "The avalanche sweeps over you!", "A avalanche passa por cima de você!"),
            new LocalizationEntry(AvalanchePassed, "The snow has gone by", "A neve passou"),
        };

        public static string Get(LocalizationManager manager, string key)
        {
            if (manager != null)
            {
                if (!manager.HasKey(TideLow))
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

        public static string Format(LocalizationManager manager, string key, string argument) => string.Format(Get(manager, key), argument);
    }
}
