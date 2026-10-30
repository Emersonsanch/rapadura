using System.Collections.Generic;
using Rapadura.Gameplay.Characters;

namespace Rapadura.Gameplay.Crafting.Advanced
{
    /// <summary>What an advanced recipe makes.</summary>
    public enum AdvancedRecipeKind
    {
        /// <summary>One equipment piece whose rarity is rolled by material quality (<see cref="CraftTierRules"/>).</summary>
        Gear,
        /// <summary>A stack of runes (consumables); only the magic cousins can draw them.</summary>
        Rune,
    }

    /// <summary>One ingredient (item id + units).</summary>
    public readonly struct AdvancedIngredient
    {
        public readonly string ItemId;
        public readonly int Quantity;

        public AdvancedIngredient(string itemId, int quantity)
        {
            ItemId = itemId;
            Quantity = quantity;
        }
    }

    /// <summary>One advanced recipe (code data, like <c>MealCatalog</c>: online the server holds the same table).</summary>
    public sealed class AdvancedRecipeSpec
    {
        public readonly string RecipeId;
        public readonly AdvancedRecipeKind Kind;
        public readonly int Tier;
        public readonly string ResultItemId;
        public readonly int ResultQuantity;
        public readonly AdvancedIngredient[] Ingredients;
        public readonly int GoldCost;
        /// <summary>Heroes allowed to craft it (null = anyone).</summary>
        public readonly CharacterId[] Crafters;

        public AdvancedRecipeSpec(string recipeId, AdvancedRecipeKind kind, int tier, string resultItemId, int resultQuantity,
            AdvancedIngredient[] ingredients, int goldCost = 0, CharacterId[] crafters = null)
        {
            RecipeId = recipeId;
            Kind = kind;
            Tier = tier;
            ResultItemId = resultItemId;
            ResultQuantity = resultQuantity;
            Ingredients = ingredients ?? new AdvancedIngredient[0];
            GoldCost = goldCost;
            Crafters = crafters;
        }

        public bool CanBeCraftedBy(CharacterId? hero)
        {
            if (Crafters == null || Crafters.Length == 0)
            {
                return true;
            }

            if (!hero.HasValue)
            {
                return false;
            }

            foreach (CharacterId id in Crafters)
            {
                if (id == hero.Value)
                {
                    return true;
                }
            }

            return false;
        }
    }

    /// <summary>Rune item ids (<c>Resources/Items</c>, consumables used from the bag like potions).</summary>
    public static class RuneItems
    {
        public const string Healing = "item_runa_cura";
        public const string Clarity = "item_runa_clareza";
        public const string Hide = "item_runa_couro";
        public const string Wind = "item_runa_vento";
        public const string GreatHealing = "item_runa_cura_grande";

        public static readonly string[] All = { Healing, Clarity, Hide, Wind, GreatHealing };
    }

    /// <summary>
    /// The advanced recipes (Docs/Balance/README.md "Crafting avançado"): leather and weapons with quality from creature parts,
    /// unlocked tier by tier, and the runes Lavine and Maria draw on cordel paper. Plain recipes stay in
    /// <c>Resources/Recipes</c> (<see cref="CraftingManager"/>); these are code data because the rolled rarity and the
    /// crafter rule are server rules.
    /// </summary>
    public static class AdvancedRecipeCatalog
    {
        private const string Wood = "item_wood";
        private const string Fiber = "item_fiber";
        private const string Herb = "item_herb";
        private const string Iron = "item_iron";
        private const string Stone = "item_stone";

        /// <summary>The cousins who know the "magia das palavras" (Referencias_MMO: magos Lavine/Maria).</summary>
        public static readonly CharacterId[] RuneCrafters = { CharacterId.Lavine, CharacterId.Maria };

        private static AdvancedIngredient I(string id, int quantity) => new AdvancedIngredient(id, quantity);

        public static readonly IReadOnlyList<AdvancedRecipeSpec> All = new[]
        {
            // -- Gear (rarity rolled by material quality, capped by tier) --
            new AdvancedRecipeSpec("adv_leather_helmet", AdvancedRecipeKind.Gear, 1, "item_leather_helmet", 1,
                new[] { I(Fiber, 4), I(CreatureParts.WolfFang, 2) }, goldCost: 10),
            new AdvancedRecipeSpec("adv_leather_boots", AdvancedRecipeKind.Gear, 1, "item_leather_boots", 1,
                new[] { I(Fiber, 3), I(CreatureParts.SpiderSilk, 2) }, goldCost: 10),
            new AdvancedRecipeSpec("adv_leather_chest", AdvancedRecipeKind.Gear, 2, "item_leather_chest", 1,
                new[] { I(Fiber, 4), I(CreatureParts.WolfFang, 2), I(CreatureParts.ScorpionStinger, 2) }, goldCost: 20),
            new AdvancedRecipeSpec("adv_hunting_bow", AdvancedRecipeKind.Gear, 2, "item_hunting_bow", 1,
                new[] { I(Wood, 3), I(CreatureParts.SpiderSilk, 2), I(CreatureParts.ScorpionStinger, 2) }, goldCost: 20),
            new AdvancedRecipeSpec("adv_iron_sword", AdvancedRecipeKind.Gear, 3, "item_iron_sword", 1,
                new[] { I(Iron, 3), I(Wood, 1), I(CreatureParts.GolemShard, 3) }, goldCost: 30),
            new AdvancedRecipeSpec("adv_iron_sword_caves", AdvancedRecipeKind.Gear, 4, "item_iron_sword", 1,
                new[] { I(Iron, 2), I(CreatureParts.GolemShard, 2), I(CreatureParts.BatWing, 3) }, goldCost: 40),

            // -- Runes (Lavine and Maria only) --
            new AdvancedRecipeSpec("adv_rune_healing", AdvancedRecipeKind.Rune, 1, RuneItems.Healing, 2,
                new[] { I(Herb, 2), I(CreatureParts.SpiderSilk, 1) }, crafters: RuneCrafters),
            new AdvancedRecipeSpec("adv_rune_clarity", AdvancedRecipeKind.Rune, 1, RuneItems.Clarity, 2,
                new[] { I(Herb, 2), I(CreatureParts.WolfFang, 1) }, crafters: RuneCrafters),
            new AdvancedRecipeSpec("adv_rune_hide", AdvancedRecipeKind.Rune, 2, RuneItems.Hide, 2,
                new[] { I(Herb, 1), I(Stone, 2), I(CreatureParts.ScorpionStinger, 1) }, crafters: RuneCrafters),
            new AdvancedRecipeSpec("adv_rune_wind", AdvancedRecipeKind.Rune, 3, RuneItems.Wind, 2,
                new[] { I(Herb, 1), I(Fiber, 2), I(CreatureParts.GolemShard, 1) }, crafters: RuneCrafters),
            new AdvancedRecipeSpec("adv_rune_great_healing", AdvancedRecipeKind.Rune, 4, RuneItems.GreatHealing, 2,
                new[] { I(Herb, 3), I(CreatureParts.BatWing, 1) }, crafters: RuneCrafters),
        };

        public static AdvancedRecipeSpec Get(string recipeId)
        {
            foreach (AdvancedRecipeSpec recipe in All)
            {
                if (recipe.RecipeId == recipeId)
                {
                    return recipe;
                }
            }

            return null;
        }

        /// <summary>Recipes open at <paramref name="tier"/> (any crafter).</summary>
        public static List<AdvancedRecipeSpec> Unlocked(int tier)
        {
            var list = new List<AdvancedRecipeSpec>();
            foreach (AdvancedRecipeSpec recipe in All)
            {
                if (CraftTierRules.IsUnlocked(recipe.Tier, tier))
                {
                    list.Add(recipe);
                }
            }

            return list;
        }
    }
}
