using System;
using System.Collections.Generic;
using System.Linq;
using Rapadura.Gameplay.Housing;

namespace Rapadura.Gameplay.Seasons
{
    public enum PrendaBuyCheck
    {
        Ok,
        /// <summary>Not an item of the Barraca de Prendas.</summary>
        NotInStall,
        NotEnoughPrendas,
        /// <summary>The stall only opens during the festa.</summary>
        FestaClosed,
    }

    /// <summary>
    /// Numbers and small rules of the Festa Junina (pure, tested; Docs/Seasons.md):
    /// <list type="bullet">
    /// <item>Prendas of a quadrilha: up to <see cref="DancePrendasMax"/> by score, +<see cref="FullComboBonus"/> without a
    /// miss, +<see cref="FirstDanceBonus"/> for the first dance of the day.</item>
    /// <item>Pescaria: <see cref="FishPrenda"/> prenda per fish landed during the festa, up to <see cref="FishPrendasPerDay"/> a day.</item>
    /// <item>The Barraca de Prendas sells the 18 Festa Junina house items of CatalogoCasa.md (<see cref="HouseSeason.Junina"/>,
    /// event source, <c>_eventPrice</c> in prendas). The Lendário (<see cref="LegendaryItemId"/>) is not sold: it comes
    /// from finishing the "tarefas do Bema" (<see cref="TasksDone"/>).</item>
    /// </list>
    /// </summary>
    public static class FestaJuninaRules
    {
        public const int DancePrendasMax = 10;
        public const int FullComboBonus = 2;
        public const int FirstDanceBonus = 5;
        public const int FishPrenda = 1;
        public const int FishPrendasPerDay = 10;

        /// <summary>"Estandarte do primeiro Bema da Vila": the festa's Lendário.</summary>
        public const string LegendaryItemId = "casa_familia_025";

        /// <summary>Tarefas do arraiá: dances to finish.</summary>
        public const int TaskDances = 5;
        /// <summary>Tarefas do arraiá: different stall items to buy.</summary>
        public const int TaskDistinctPrizes = 6;

        public static int PrendasForDance(int score, int maxScore, bool fullCombo, bool firstDanceToday)
        {
            if (maxScore <= 0 || score < 0)
            {
                return 0;
            }

            int prendas = (int)Math.Floor(Math.Min(score, maxScore) * (double)DancePrendasMax / maxScore);
            if (fullCombo)
            {
                prendas += FullComboBonus;
            }

            if (firstDanceToday)
            {
                prendas += FirstDanceBonus;
            }

            return prendas;
        }

        /// <summary>Prendas for one more fish today, given how many fish already paid today.</summary>
        public static int PrendasForFish(bool festaActive, int fishPaidToday)
        {
            return festaActive && fishPaidToday < FishPrendasPerDay ? FishPrenda : 0;
        }

        private static List<HouseItemDefinition> _stall;

        /// <summary>The Barraca de Prendas: Festa Junina items with an event price, cheapest first.</summary>
        public static IReadOnlyList<HouseItemDefinition> StallItems => _stall ??= BuildStall(HouseCatalog.All);

        public static List<HouseItemDefinition> BuildStall(IEnumerable<HouseItemDefinition> catalog)
        {
            if (catalog == null)
            {
                return new List<HouseItemDefinition>();
            }

            return catalog
                .Where(item => item != null && item.Seasonal == HouseSeason.Junina && item.EventPrice > 0 && IsFromFesta(item))
                .OrderBy(item => item.EventPrice)
                .ThenBy(item => item.ItemId, StringComparer.Ordinal)
                .ToList();
        }

        public static bool IsFromFesta(HouseItemDefinition item)
        {
            return item?.Sources != null
                && item.Sources.Any(source => source != null && source.Type == HouseItemSourceType.Event && source.RefId == SeasonCalendar.FestaJuninaEventId);
        }

        public static HouseItemDefinition StallItem(string itemId)
        {
            if (string.IsNullOrEmpty(itemId))
            {
                return null;
            }

            foreach (HouseItemDefinition item in StallItems)
            {
                if (item.ItemId == itemId)
                {
                    return item;
                }
            }

            return null;
        }

        public static PrendaBuyCheck CheckBuy(HouseItemDefinition item, int prendas, bool festaActive)
        {
            if (!festaActive)
            {
                return PrendaBuyCheck.FestaClosed;
            }

            if (item == null || StallItem(item.ItemId) == null)
            {
                return PrendaBuyCheck.NotInStall;
            }

            return prendas < item.EventPrice ? PrendaBuyCheck.NotEnoughPrendas : PrendaBuyCheck.Ok;
        }

        /// <summary>All tarefas do Bema done: <see cref="TaskDances"/> dances, one without a miss, <see cref="TaskDistinctPrizes"/> different prizes.</summary>
        public static bool TasksDone(int dances, bool fullComboDanced, int distinctPrizes)
        {
            return dances >= TaskDances && fullComboDanced && distinctPrizes >= TaskDistinctPrizes;
        }
    }
}
