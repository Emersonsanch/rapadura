using System;
using System.Collections.Generic;
using Rapadura.Gameplay.Refining;

namespace Rapadura.Gameplay.Tacho
{
    /// <summary>What a Tacho recipe does (append only).</summary>
    public enum TachoRecipeKind
    {
        /// <summary>A refined piece (+4 or more) + Pedra do Caos + biome materials + gold → wings level 1.</summary>
        CreateWings = 0,
        /// <summary>Wings level N + Pedras do Caos/Alma(/Vida) + materials + gold → level N + 1 (1→2, 2→3).</summary>
        UpgradeWings = 1,
        /// <summary>"Proteção do Caos": a refine (+7..+14 → +1) that stays at the same level when it fails.</summary>
        ProtectedRefine = 2,
        /// <summary>Fruta do Mandacaru: +1 attribute point when eaten (capped per hero, <see cref="FruitRules"/>).</summary>
        GrowthFruit = 3,
        /// <summary>Fruta do Umbu: takes 1 point back from an attribute to spend again.</summary>
        ResetFruit = 4,
    }

    /// <summary>Why a Tacho recipe can or cannot be tried now.</summary>
    public enum TachoCheck
    {
        Ok = 0,
        /// <summary>No main item chosen (wings / protected refine need one).</summary>
        NoMainItem = 1,
        /// <summary>The main item is below +4 (wings) or the wing is not the recipe's level.</summary>
        MainItemTooLow = 2,
        MissingMaterial = 3,
        NotEnoughGold = 4,
        /// <summary>Wings at level 3 (level 4 is not craftable) or a piece at +15.</summary>
        MaxLevel = 5,
        /// <summary>Protected refine where a failure would not lose a level anyway (+0..+6).</summary>
        NotNeeded = 6,
    }

    /// <summary>One Tacho recipe (pure data). Materials exclude the main item and the gold.</summary>
    public sealed class TachoRecipe
    {
        public readonly string Id;
        public readonly TachoRecipeKind Kind;
        /// <summary>Wings recipes: the style made / upgraded.</summary>
        public readonly WingStyle Style;
        /// <summary>UpgradeWings: the level of the wing that goes in (1 or 2).</summary>
        public readonly int FromLevel;
        public readonly TachoIngredient[] Materials;
        public readonly int Gold;
        /// <summary>Fixed chance (0 = it depends on the main item: CreateWings, ProtectedRefine).</summary>
        public readonly float BaseChance;
        /// <summary>The item made on success (null for ProtectedRefine, which writes the level in place).</summary>
        public readonly string ResultItemId;

        public TachoRecipe(string id, TachoRecipeKind kind, WingStyle style, int fromLevel, TachoIngredient[] materials, int gold,
            float baseChance, string resultItemId)
        {
            Id = id;
            Kind = kind;
            Style = style;
            FromLevel = fromLevel;
            Materials = materials ?? Array.Empty<TachoIngredient>();
            Gold = gold;
            BaseChance = baseChance;
            ResultItemId = resultItemId;
        }

        /// <summary>Wings and the protected refine work on a main item, which is ALWAYS kept when the roll fails.</summary>
        public bool NeedsMainItem => Kind == TachoRecipeKind.CreateWings || Kind == TachoRecipeKind.UpgradeWings || Kind == TachoRecipeKind.ProtectedRefine;
    }

    /// <summary>The result of one Tacho roll (pure data: online the server sends this back).</summary>
    public readonly struct TachoOutcome
    {
        public readonly string RecipeId;
        public readonly TachoRecipeKind Kind;
        public readonly float Chance;
        public readonly double Roll;
        public readonly bool Success;
        /// <summary>Level of the main item before (wing level or refine level; 0 for fruits).</summary>
        public readonly int FromLevel;
        /// <summary>Level after: the new wing/refine level on success, the same on failure (never lower).</summary>
        public readonly int ToLevel;
        /// <summary>Item made on success (wings, fruit); null for the protected refine and on failure.</summary>
        public readonly string ResultItemId;

        public TachoOutcome(string recipeId, TachoRecipeKind kind, float chance, double roll, bool success, int fromLevel, int toLevel, string resultItemId)
        {
            RecipeId = recipeId;
            Kind = kind;
            Chance = chance;
            Roll = roll;
            Success = success;
            FromLevel = fromLevel;
            ToLevel = toLevel;
            ResultItemId = resultItemId;
        }

        /// <summary>Kid-friendly rule: on a failure the materials are spent and the main item stays exactly as it was.</summary>
        public bool MainItemKept => !Success;
    }

    /// <summary>
    /// Pure rules of the "Tacho Encantado da Vó Edna" (FASE 16, MU Online's Chaos Machine with our names; Docs/Balance/README.md
    /// "Tacho Encantado e asas"), unit-tested in <c>TachoTests</c>. No UnityEngine, no state: the window previews with them
    /// and, online (Fase 12), the server runs <see cref="Attempt"/> with its own seed; the client never rolls.
    /// <list type="bullet">
    /// <item>Asas nível 1: a refined weapon/armour piece at +4 or more (consumed on success) + 1 Pedra do Caos + the style's
    /// biome materials + <see cref="CreateWingsGold"/> gold. Chance 50% at +4, +5 points per level above, up to 75% (+9).</item>
    /// <item>Asas 1→2: 2 Caos + 1 Alma + 4 minério de ferro + 4 pedras + 100 gold, 50%. 2→3: 3 Caos + 2 Alma + 1 Vida +
    /// 6 ferro + 6 pedras + 150 gold, 35%. Level 4 (lendária) is documented only.</item>
    /// <item>Proteção do Caos: the normal refine (<see cref="RefineRules"/>: same stone, gold and chance, no charm) + 1 Pedra do
    /// Caos, only where a failure would lose a level (+7..+14); a failure keeps the level.</item>
    /// <item>Frutas de Luz: Mandacaru (+1 point, 60%) and Umbu (redistribute 1 point, 80%).</item>
    /// <item>Failure, every recipe: the materials, stones and gold are spent; the main item is kept as it was.</item>
    /// </list>
    /// </summary>
    public static class TachoRules
    {
        public const string ChaosStone = RefineItems.ChaosStone;
        public const string Gold = RefineItems.Gold;

        /// <summary>Lowest refine level of the piece that becomes wings.</summary>
        public const int MinRefineForWings = 4;
        public const float CreateWingsChanceAtMin = 0.50f;
        public const float CreateWingsChancePerLevel = 0.05f;
        public const float CreateWingsMaxChance = 0.75f;
        public const int CreateWingsChaosStones = 1;
        public const int CreateWingsGold = 60;

        public const float UpgradeTo2Chance = 0.50f;
        public const float UpgradeTo3Chance = 0.35f;
        public const int UpgradeTo2Gold = 100;
        public const int UpgradeTo3Gold = 150;

        /// <summary>Pedras do Caos added to a protected refine.</summary>
        public const int ProtectedRefineChaosStones = 1;

        public const float GrowthFruitChance = 0.60f;
        public const int GrowthFruitGold = 40;
        public const float ResetFruitChance = 0.80f;
        public const int ResetFruitGold = 20;

        public const string ProtectedRefineId = "tacho_protecao_caos";
        public const string GrowthFruitId = "tacho_fruta_mandacaru";
        public const string ResetFruitId = "tacho_fruta_umbu";

        private static IReadOnlyList<TachoRecipe> _all;

        /// <summary>Every recipe, in the window's order: wings per style, upgrades per style, protection, fruits.</summary>
        public static IReadOnlyList<TachoRecipe> All => _all ?? (_all = Build());

        private static IReadOnlyList<TachoRecipe> Build()
        {
            var list = new List<TachoRecipe>();
            foreach (WingStyleSpec spec in WingCatalog.Styles)
            {
                var materials = new List<TachoIngredient> { new TachoIngredient(ChaosStone, CreateWingsChaosStones) };
                materials.AddRange(spec.Materials);
                list.Add(new TachoRecipe(CreateWingsId(spec.Style), TachoRecipeKind.CreateWings, spec.Style, 0, materials.ToArray(),
                    CreateWingsGold, 0f, WingCatalog.ItemId(spec.Style, 1)));
            }

            foreach (WingStyleSpec spec in WingCatalog.Styles)
            {
                for (int from = 1; from < WingCatalog.MaxCraftableLevel; from++)
                {
                    list.Add(new TachoRecipe(UpgradeWingsId(spec.Style, from), TachoRecipeKind.UpgradeWings, spec.Style, from,
                        UpgradeMaterials(from), from == 1 ? UpgradeTo2Gold : UpgradeTo3Gold, from == 1 ? UpgradeTo2Chance : UpgradeTo3Chance,
                        WingCatalog.ItemId(spec.Style, from + 1)));
                }
            }

            list.Add(new TachoRecipe(ProtectedRefineId, TachoRecipeKind.ProtectedRefine, default, 0,
                new[] { new TachoIngredient(ChaosStone, ProtectedRefineChaosStones) }, 0, 0f, null));
            list.Add(new TachoRecipe(GrowthFruitId, TachoRecipeKind.GrowthFruit, default, 0,
                new[] { new TachoIngredient(ChaosStone, 1), new TachoIngredient(RefineItems.LifeStone, 1), new TachoIngredient("item_herb", 5) },
                GrowthFruitGold, GrowthFruitChance, FruitRules.GrowthFruit));
            list.Add(new TachoRecipe(ResetFruitId, TachoRecipeKind.ResetFruit, default, 0,
                new[] { new TachoIngredient(RefineItems.BlessingStone, 2), new TachoIngredient("item_herb", 3) },
                ResetFruitGold, ResetFruitChance, FruitRules.ResetFruit));
            return list;
        }

        private static TachoIngredient[] UpgradeMaterials(int fromLevel)
        {
            return fromLevel == 1
                ? new[]
                {
                    new TachoIngredient(ChaosStone, 2), new TachoIngredient(RefineItems.SoulStone, 1),
                    new TachoIngredient("item_iron_ore", 4), new TachoIngredient("item_stone", 4),
                }
                : new[]
                {
                    new TachoIngredient(ChaosStone, 3), new TachoIngredient(RefineItems.SoulStone, 2), new TachoIngredient(RefineItems.LifeStone, 1),
                    new TachoIngredient("item_iron", 6), new TachoIngredient("item_stone", 6),
                };
        }

        public static string CreateWingsId(WingStyle style) => "tacho_asas_" + style.ToString().ToLowerInvariant();

        public static string UpgradeWingsId(WingStyle style, int fromLevel) => CreateWingsId(style) + "_" + fromLevel + "_" + (fromLevel + 1);

        public static TachoRecipe Get(string recipeId)
        {
            foreach (TachoRecipe recipe in All)
            {
                if (recipe.Id == recipeId)
                {
                    return recipe;
                }
            }

            return null;
        }

        /// <summary>The upgrade recipe that takes <paramref name="wingItemId"/> one level up (null at level 3 or for a non-wing).</summary>
        public static TachoRecipe UpgradeFor(string wingItemId)
        {
            if (!WingCatalog.TryParse(wingItemId, out WingStyle style, out int level) || level >= WingCatalog.MaxCraftableLevel)
            {
                return null;
            }

            return Get(UpgradeWingsId(style, level));
        }

        // -------------------------------------------------------------- chances

        /// <summary>Chance of making wings from a piece at <paramref name="refineLevel"/> (0 below +4).</summary>
        public static float CreateWingsChance(int refineLevel)
        {
            refineLevel = RefineRules.ClampLevel(refineLevel);
            if (refineLevel < MinRefineForWings)
            {
                return 0f;
            }

            return Math.Min(CreateWingsMaxChance, CreateWingsChanceAtMin + CreateWingsChancePerLevel * (refineLevel - MinRefineForWings));
        }

        /// <summary>True where the Proteção do Caos changes something: the normal refine would lose a level on failure.</summary>
        public static bool ProtectionMatters(int refineLevel)
        {
            refineLevel = RefineRules.ClampLevel(refineLevel);
            return refineLevel < RefineRules.MaxLevel && RefineRules.FailureLevel(refineLevel) < refineLevel;
        }

        /// <summary>
        /// The chance shown for <paramref name="recipe"/> with a main item at <paramref name="mainLevel"/> (refine level for
        /// wings/protection, wing level for upgrades; ignored by fruits).
        /// </summary>
        public static float ChanceFor(TachoRecipe recipe, int mainLevel)
        {
            if (recipe == null)
            {
                return 0f;
            }

            switch (recipe.Kind)
            {
                case TachoRecipeKind.CreateWings: return CreateWingsChance(mainLevel);
                case TachoRecipeKind.ProtectedRefine: return RefineRules.BaseChance(mainLevel);
                default: return recipe.BaseChance;
            }
        }

        /// <summary>Gold of <paramref name="recipe"/>: the protected refine pays what the normal refine pays (+9 and up).</summary>
        public static int GoldFor(TachoRecipe recipe, int mainLevel)
        {
            if (recipe == null)
            {
                return 0;
            }

            return recipe.Kind == TachoRecipeKind.ProtectedRefine ? RefineRules.GoldCostFor(mainLevel) : recipe.Gold;
        }

        /// <summary>
        /// Everything spent by one try (materials + the refine stone of a protected refine; gold apart, see <see cref="GoldFor"/>).
        /// </summary>
        public static List<TachoIngredient> CostFor(TachoRecipe recipe, int mainLevel)
        {
            var list = new List<TachoIngredient>();
            if (recipe == null)
            {
                return list;
            }

            if (recipe.Kind == TachoRecipeKind.ProtectedRefine)
            {
                string stone = RefineItems.IdOf(RefineRules.StoneFor(mainLevel));
                if (stone != null)
                {
                    list.Add(new TachoIngredient(stone, 1));
                }
            }

            list.AddRange(recipe.Materials);
            return list;
        }

        // -------------------------------------------------------------- checks

        /// <summary>
        /// Whether <paramref name="recipe"/> can be tried. <paramref name="hasMain"/>/<paramref name="mainLevel"/>/<paramref name="mainItemId"/>
        /// describe the chosen main item; <paramref name="owned"/> counts an item id in the bag.
        /// </summary>
        public static TachoCheck Check(TachoRecipe recipe, bool hasMain, int mainLevel, string mainItemId, Func<string, int> owned)
        {
            if (recipe == null)
            {
                return TachoCheck.NoMainItem;
            }

            owned = owned ?? (_ => 0);
            if (recipe.NeedsMainItem && !hasMain)
            {
                return TachoCheck.NoMainItem;
            }

            switch (recipe.Kind)
            {
                case TachoRecipeKind.CreateWings:
                    if (RefineRules.ClampLevel(mainLevel) < MinRefineForWings)
                    {
                        return TachoCheck.MainItemTooLow;
                    }

                    break;
                case TachoRecipeKind.UpgradeWings:
                    if (!WingCatalog.TryParse(mainItemId, out WingStyle style, out int wingLevel) || style != recipe.Style)
                    {
                        return TachoCheck.NoMainItem;
                    }

                    if (wingLevel >= WingCatalog.MaxCraftableLevel)
                    {
                        return TachoCheck.MaxLevel;
                    }

                    if (wingLevel != recipe.FromLevel)
                    {
                        return TachoCheck.MainItemTooLow;
                    }

                    break;
                case TachoRecipeKind.ProtectedRefine:
                    if (RefineRules.ClampLevel(mainLevel) >= RefineRules.MaxLevel)
                    {
                        return TachoCheck.MaxLevel;
                    }

                    if (!ProtectionMatters(mainLevel))
                    {
                        return TachoCheck.NotNeeded;
                    }

                    break;
            }

            var needed = new Dictionary<string, int>();
            foreach (TachoIngredient ingredient in CostFor(recipe, mainLevel))
            {
                needed.TryGetValue(ingredient.ItemId, out int sum);
                needed[ingredient.ItemId] = sum + ingredient.Quantity;
            }

            foreach (KeyValuePair<string, int> pair in needed)
            {
                if (owned(pair.Key) < pair.Value)
                {
                    return TachoCheck.MissingMaterial;
                }
            }

            return owned(Gold) < GoldFor(recipe, mainLevel) ? TachoCheck.NotEnoughGold : TachoCheck.Ok;
        }

        /// <summary>The first material line the bag lacks (null when every line is there).</summary>
        public static TachoIngredient? FirstMissing(TachoRecipe recipe, int mainLevel, Func<string, int> owned)
        {
            owned = owned ?? (_ => 0);
            foreach (TachoIngredient ingredient in CostFor(recipe, mainLevel))
            {
                if (owned(ingredient.ItemId) < ingredient.Quantity)
                {
                    return ingredient;
                }
            }

            return null;
        }

        // -------------------------------------------------------------- rolls (seeded: the server's)

        /// <summary>Same SplitMix64 roll as the refine (<see cref="RefineRules.Roll"/>): identical on every platform.</summary>
        public static double Roll(int seed) => RefineRules.Roll(seed);

        /// <summary>Resolves a roll already drawn: success when <c>roll &lt; chance</c>; a failure never lowers anything.</summary>
        public static TachoOutcome Resolve(TachoRecipe recipe, int mainLevel, double roll)
        {
            if (recipe == null)
            {
                return default;
            }

            float chance = ChanceFor(recipe, mainLevel);
            bool success = chance > 0f && roll < chance;
            int from;
            int to;
            switch (recipe.Kind)
            {
                case TachoRecipeKind.CreateWings:
                    from = 0;
                    to = success ? 1 : 0;
                    break;
                case TachoRecipeKind.UpgradeWings:
                    from = recipe.FromLevel;
                    to = success ? from + 1 : from;
                    break;
                case TachoRecipeKind.ProtectedRefine:
                    from = RefineRules.ClampLevel(mainLevel);
                    to = success ? from + 1 : from;
                    break;
                default:
                    from = 0;
                    to = 0;
                    break;
            }

            return new TachoOutcome(recipe.Id, recipe.Kind, chance, roll, success, from, to, success ? recipe.ResultItemId : null);
        }

        /// <summary>One try with <paramref name="seed"/> (online: the server's seed).</summary>
        public static TachoOutcome Attempt(TachoRecipe recipe, int mainLevel, int seed) => Resolve(recipe, mainLevel, Roll(seed));
    }
}
