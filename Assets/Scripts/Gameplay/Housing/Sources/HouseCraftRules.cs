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
    /// H3 crafting of the catalog's "Crafting:" items (12 house items + 13 wardrobe entries). Materials are the real biome
    /// materials (<c>item_wood</c>, <c>item_fiber</c>, <c>item_stone</c>, <c>item_iron</c> = lingote, <c>item_iron_ore</c>),
    /// paid from the bag and the Baú da casa (<see cref="IHouseWallet"/>), all or nothing. Pure.
    /// The clothing catalog's boss recipes (Tear, Tear de mestre, "Forja (lingotes + material de chefe)") take the world
    /// bosses' own keepsakes (<see cref="WorldBossKeepsakes"/>, each one the <c>LootItem</c> of its boss asset).
    /// </summary>
    public static class HouseCraftRules
    {
        public const string MasterBenchId = "casa_util_012";

        /// <summary>
        /// The own drop of each world boss of Regions 5–16 (<c>RegionBossRules.AllSpawns</c>), by boss asset name. Each one is
        /// that asset's <c>LootItem</c> (<c>Resources/Items</c>) and goes into at least one recipe of <see cref="All"/>.
        /// </summary>
        public static readonly IReadOnlyDictionary<string, string> WorldBossKeepsakes = new Dictionary<string, string>
        {
            { "enemy_mountain_hedammu", "item_chefe_escama_hedammu" },
            { "enemy_mountain_swallowing_mire", "item_chefe_lama_limpa" },
            { "enemy_mountain_aydahar", "item_chefe_escama_aydahar" },
            { "enemy_mountain_four_gluttons", "item_chefe_bronze_antigo" },
            { "enemy_mountain_ohya", "item_chefe_ferro_antigo" },
            { "enemy_mountain_wind_king", "item_chefe_pena_vendaval" },
            { "enemy_mountain_old_kul", "item_chefe_barbatana_kul" },
            { "enemy_mountain_ember_glutton", "item_chefe_brasa_viva" },
            { "enemy_mountain_apep", "item_chefe_escama_apep" },
            { "enemy_mountain_dream_of_destruction", "item_chefe_tabua_sonho" },
            { "enemy_mountain_great_dark", "item_chefe_lasca_selo" },
            { "enemy_mountain_az_captain", "item_chefe_semente_luz" },
        };

        private const string HedammuScale = "item_chefe_escama_hedammu";
        private const string CleanMud = "item_chefe_lama_limpa";
        private const string AydaharScale = "item_chefe_escama_aydahar";
        private const string VesselBronze = "item_chefe_bronze_antigo";
        private const string AncientIron = "item_chefe_ferro_antigo";
        private const string GaleFeather = "item_chefe_pena_vendaval";
        private const string SilverFin = "item_chefe_barbatana_kul";
        private const string LivingEmber = "item_chefe_brasa_viva";
        private const string ApepScale = "item_chefe_escama_apep";
        private const string DreamTablet = "item_chefe_tabua_sonho";
        private const string SealShard = "item_chefe_lasca_selo";
        private const string SeedOfLight = "item_chefe_semente_luz";
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
                // CatalogoRoupas.md's boss pieces (proposed numbers). Épico: "Tear (tecido + material de chefe)" and
                // "Forja (lingotes + material de chefe)", one world boss keepsake each.
                R("roupa_maithe_015", HouseItemFunction.Loom, false, (Fiber, 10), (GaleFeather, 2)),
                R("roupa_maria_015", HouseItemFunction.Loom, false, (Fiber, 10), (HedammuScale, 3)),
                R("roupa_lavine_015", HouseItemFunction.Loom, false, (Fiber, 10), (ApepScale, 2)),
                R("roupa_joaquim_015", HouseItemFunction.Forge, false, (Iron, 6), (Stone, 4), (AncientIron, 2)),
                R("roupa_icaro_015", HouseItemFunction.Forge, false, (Iron, 6), (Fiber, 2), (LivingEmber, 2)),
                // Lendário: "Tear de mestre (4 lembranças de chefe)", four different keepsakes each.
                R("roupa_maithe_019", HouseItemFunction.Loom, false, (Fiber, 12), (GaleFeather, 1), (AydaharScale, 1), (VesselBronze, 1), (SilverFin, 1)),
                R("roupa_maria_019", HouseItemFunction.Loom, false, (Fiber, 12), (SeedOfLight, 1), (CleanMud, 1), (HedammuScale, 1), (SilverFin, 1)),
                R("roupa_joaquim_019", HouseItemFunction.Loom, false, (Fiber, 12), (AncientIron, 1), (CleanMud, 1), (VesselBronze, 1), (DreamTablet, 1)),
                R("roupa_icaro_019", HouseItemFunction.Loom, false, (Fiber, 12), (LivingEmber, 1), (VesselBronze, 1), (AncientIron, 1), (SealShard, 1)),
                R("roupa_lavine_019", HouseItemFunction.Loom, false, (Fiber, 12), (ApepScale, 1), (DreamTablet, 1), (SealShard, 1), (SeedOfLight, 1)),
            };

            HouseCraftRecipe fireplace = R("casa_sala_041", HouseItemFunction.Bench, true, (Stone, 30), (Wood, 10));
            fireplace.StationItemId = MasterBenchId;
            fireplace.RequiredBossId = "enemy_mountain_warlord";
            list.Insert(4, fireplace);
            return list;
        }

        public static HouseCraftRecipe Get(string recipeId) => All.FirstOrDefault(r => r.Id == recipeId);

        public static HouseCraftRecipe ForResult(string resultId) => All.FirstOrDefault(r => r.ResultId == resultId);

        /// <summary>The recipes that spend <paramref name="itemId"/>.</summary>
        public static IEnumerable<HouseCraftRecipe> UsesOf(string itemId) => All.Where(r => r.Costs.Any(c => c.ItemId == itemId));

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
