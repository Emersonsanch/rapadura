using System;
using System.Collections.Generic;
using System.Linq;

namespace Rapadura.Gameplay.Housing
{
    /// <summary>What a house recipe makes: a house item (to the Depósito) or a wardrobe entry (look/dye, via the owed list).</summary>
    public enum HouseCraftResultKind
    {
        HouseItem,
        Wardrobe,
    }

    /// <summary>One furniture (or wardrobe) recipe made at a station placed in the house (Housing.md §7.6).</summary>
    public sealed class HouseCraftRecipe
    {
        public string Id;
        public string ResultId;
        public HouseCraftResultKind Kind;
        /// <summary>The station function that must be placed in the house (Bench, Forge, Stove).</summary>
        public HouseItemFunction Station;
        /// <summary>A specific station piece when any of that function will not do (the "Bancada de mestre").</summary>
        public string StationItemId;
        /// <summary>A boss that must be defeated first (the doc's "lembrança do chefe" is kept, never spent).</summary>
        public string RequiredBossId;
        public HouseMaterialCost[] Costs = Array.Empty<HouseMaterialCost>();
        /// <summary>True for the doc's own numbers; false for the ones this phase proposes (tunable, Docs/Housing.md §7.6).</summary>
        public bool FromDoc;
    }

    public enum HouseCraftBlock
    {
        None,
        UnknownRecipe,
        NoHouse,
        NoStation,
        BossNeeded,
        MissingMaterials,
        PaymentFailed,
    }

    /// <summary>
    /// H3 crafting of the catalog's "Crafting:" items (12 house items + 3 wardrobe entries). Materials are the real biome
    /// materials (<c>item_wood</c>, <c>item_fiber</c>, <c>item_stone</c>, <c>item_iron</c> = lingote, <c>item_iron_ore</c>),
    /// paid from the bag and the Baú da casa (<see cref="IHouseWallet"/>), all or nothing. Pure.
    /// The Tear recipes of the clothing catalog need boss materials that do not exist as items yet: not here.
    /// </summary>
    public static class HouseCraftRules
    {
        public const string MasterBenchId = "casa_util_012";
        private const string Wood = HouseUpgradeRules.WoodId;
        private const string Fiber = HouseUpgradeRules.FiberId;
        private const string Stone = HouseUpgradeRules.StoneId;
        private const string Iron = HouseUpgradeRules.IronId;
        private const string IronOre = "item_iron_ore";

        private static HouseCraftRecipe R(string result, HouseItemFunction station, bool fromDoc, params (string Id, int Qty)[] costs)
        {
            return new HouseCraftRecipe
            {
                Id = "recipe_house_" + result,
                ResultId = result,
                Kind = result.StartsWith("casa_", StringComparison.Ordinal) ? HouseCraftResultKind.HouseItem : HouseCraftResultKind.Wardrobe,
                Station = station,
                FromDoc = fromDoc,
                Costs = costs.Select(c => new HouseMaterialCost(c.Id, c.Qty)).ToArray(),
            };
        }

        public static readonly IReadOnlyList<HouseCraftRecipe> All = BuildAll();

        private static List<HouseCraftRecipe> BuildAll()
        {
            var list = new List<HouseCraftRecipe>
            {
                // §7.6, numbers from the doc.
                R("casa_sala_042", HouseItemFunction.Bench, true, (Wood, 4)),
                R("casa_quintal_023", HouseItemFunction.Bench, true, (Wood, 6), (Stone, 2)),
                R("casa_quintal_019", HouseItemFunction.Bench, true, (Stone, 20), (Iron, 4)),
                R("casa_banheiro_020", HouseItemFunction.Bench, true, (Iron, 3), (Fiber, 4)),
                R("casa_cozinha_040", HouseItemFunction.Forge, true, (Iron, 6), (Stone, 10)),
                R("casa_tapete_021", HouseItemFunction.Bench, true, (Fiber, 12), (Wood, 6)),
                // Proposed here (the catalog says "Crafting: Bancada" without numbers): Ícaro's workshop set and the blackout curtain.
                R("casa_luz_016", HouseItemFunction.Bench, false, (Iron, 3), (Wood, 2)),
                R("casa_luz_027", HouseItemFunction.Bench, false, (Iron, 5), (Stone, 2), (Wood, 2)),
                R("casa_parede_012", HouseItemFunction.Bench, false, (Iron, 2), (Wood, 3)),
                R("casa_tapete_022", HouseItemFunction.Bench, false, (Fiber, 8), (Iron, 1)),
                R("casa_cortina_018", HouseItemFunction.Bench, false, (Fiber, 6)),
                R("casa_musica_017", HouseItemFunction.Bench, false, (Wood, 4), (Stone, 2), (Fiber, 2)),
                // Wardrobe entries of CatalogoRoupas.md with a "Crafting:" origin (proposed numbers).
                R("tinta_006", HouseItemFunction.Stove, false, (Wood, 2)),
                R("tinta_019", HouseItemFunction.Forge, false, (Iron, 1), (IronOre, 1)),
                R("visual_arma_icaro_002", HouseItemFunction.Bench, false, (Iron, 2), (Fiber, 2)),
            };

            HouseCraftRecipe fireplace = R("casa_sala_041", HouseItemFunction.Bench, true, (Stone, 30), (Wood, 10));
            fireplace.StationItemId = MasterBenchId;
            fireplace.RequiredBossId = "enemy_mountain_warlord";
            list.Insert(4, fireplace);
            return list;
        }

        public static HouseCraftRecipe Get(string recipeId) => All.FirstOrDefault(r => r.Id == recipeId);

        public static HouseCraftRecipe ForResult(string resultId) => All.FirstOrDefault(r => r.ResultId == resultId);

        /// <param name="hasStation">Is a piece with this function (or this exact item, when given) placed in the house?</param>
        public static HouseCraftBlock Check(HouseCraftRecipe recipe, bool hasHouse, Func<HouseItemFunction, string, bool> hasStation,
            Func<string, bool> bossDefeated, Func<string, int> owned)
        {
            if (recipe == null) return HouseCraftBlock.UnknownRecipe;
            if (!hasHouse) return HouseCraftBlock.NoHouse;
            if (hasStation == null || !hasStation(recipe.Station, recipe.StationItemId)) return HouseCraftBlock.NoStation;
            if (!string.IsNullOrEmpty(recipe.RequiredBossId) && (bossDefeated == null || !bossDefeated(recipe.RequiredBossId))) return HouseCraftBlock.BossNeeded;
            if (owned == null || recipe.Costs.Any(c => owned(c.ItemId) < c.Quantity)) return HouseCraftBlock.MissingMaterials;
            return HouseCraftBlock.None;
        }
    }
}
