using System;
using System.Collections.Generic;
using Rapadura.Core.DI;
using Rapadura.Core.Events;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Inventory;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.Items.Procedural;
using Rapadura.Gameplay.Story;
using Rapadura.Gameplay.World.Biomes;
using Rapadura.Gameplay.World.Map;

namespace Rapadura.Gameplay.Crafting.Advanced
{
    /// <summary>Why an advanced recipe can't be crafted.</summary>
    public enum AdvancedCraftCheck
    {
        Ok,
        Unknown,
        Locked,
        /// <summary>Runes: only Lavine and Maria draw them.</summary>
        WrongCrafter,
        MissingIngredients,
        MissingGold,
        MissingTrophy,
        NoRoom,
    }

    /// <summary>Raised after an advanced craft (gear with its rolled rarity, or a rune stack).</summary>
    public readonly struct AdvancedItemCraftedEvent : IGameEvent
    {
        public readonly string RecipeId;
        public readonly string ItemId;
        public readonly int Quantity;
        public readonly ItemRarity Rarity;
        public readonly MaterialQuality Quality;

        public AdvancedItemCraftedEvent(string recipeId, string itemId, int quantity, ItemRarity rarity, MaterialQuality quality)
        {
            RecipeId = recipeId;
            ItemId = itemId;
            Quantity = quantity;
            Rarity = rarity;
            Quality = quality;
        }
    }

    /// <summary>Raised after a fusion roll (success or consolation).</summary>
    public readonly struct ItemFusedEvent : IGameEvent
    {
        public readonly string ItemId;
        public readonly FusionOutcome Outcome;

        public ItemFusedEvent(string itemId, FusionOutcome outcome)
        {
            ItemId = itemId;
            Outcome = outcome;
        }
    }

    /// <summary>
    /// Applies the advanced crafting rules (<see cref="CraftTierRules"/>, <see cref="AdvancedRecipeCatalog"/>,
    /// <see cref="FusionRules"/>, <see cref="ImbueRules"/>) to a live <see cref="InventoryManager"/>: checks, spends and
    /// produces, all or nothing (room is checked before anything is spent). The RNG is always passed in, so online
    /// (Fase 12) the same calls run on the server with its seed.
    /// </summary>
    public static class AdvancedCraftingService
    {
        public const string GoldItemId = "item_gold";

        /// <summary>The live tier: deepest region unlocked on the world map (Floresta when none), all four on NG+.</summary>
        public static int ResolveTier()
        {
            int deepest = 0;
            if (ServiceLocator.TryGet(out WorldMapProgress progress) && progress != null)
            {
                foreach (BiomeType region in Enum.GetValues(typeof(BiomeType)))
                {
                    if (progress.IsUnlocked(region))
                    {
                        deepest = Math.Max(deepest, (int)region);
                    }
                }
            }

            return CraftTierRules.TierFor(deepest, NewGamePlusProgress.ResolveCurrentCycle());
        }

        /// <summary>The hero now playing (null outside a session).</summary>
        public static CharacterId? ResolveHero()
        {
            return ServiceLocator.TryGet(out CharacterSelectionState selection) && selection != null ? selection.Selected : null;
        }

        // ------------------------------------------------------------------ recipes

        public static AdvancedCraftCheck Check(AdvancedRecipeSpec recipe, int tier, CharacterId? hero, bool withTrophy, Func<string, int> countInBag)
        {
            if (recipe == null)
            {
                return AdvancedCraftCheck.Unknown;
            }

            if (!CraftTierRules.IsUnlocked(recipe.Tier, tier))
            {
                return AdvancedCraftCheck.Locked;
            }

            if (!recipe.CanBeCraftedBy(hero))
            {
                return AdvancedCraftCheck.WrongCrafter;
            }

            if (countInBag == null)
            {
                return AdvancedCraftCheck.MissingIngredients;
            }

            foreach (AdvancedIngredient ingredient in recipe.Ingredients)
            {
                if (countInBag(ingredient.ItemId) < ingredient.Quantity)
                {
                    return AdvancedCraftCheck.MissingIngredients;
                }
            }

            if (recipe.GoldCost > 0 && countInBag(GoldItemId) < recipe.GoldCost)
            {
                return AdvancedCraftCheck.MissingGold;
            }

            if (withTrophy && recipe.Kind == AdvancedRecipeKind.Gear && countInBag(CreatureParts.BeastTrophy) < 1)
            {
                return AdvancedCraftCheck.MissingTrophy;
            }

            return AdvancedCraftCheck.Ok;
        }

        /// <summary>Material quality of <paramref name="recipe"/> (ingredient rarities from <paramref name="resolveItem"/>).</summary>
        public static MaterialQuality QualityOf(AdvancedRecipeSpec recipe, bool withTrophy, Func<string, ItemDefinition> resolveItem = null)
        {
            resolveItem = resolveItem ?? ItemDatabase.GetById;
            var materials = new List<(ItemRarity, int)>();
            foreach (AdvancedIngredient ingredient in recipe.Ingredients)
            {
                ItemDefinition item = resolveItem(ingredient.ItemId);
                materials.Add((item != null ? item.Rarity : ItemRarity.Common, ingredient.Quantity));
            }

            return CraftTierRules.QualityOf(materials, withTrophy);
        }

        /// <summary>
        /// Crafts <paramref name="recipe"/> from <paramref name="bag"/>. Gear: one piece with a rarity rolled by quality and
        /// capped by <paramref name="tier"/> (affixes by <see cref="ProceduralItemGenerator"/>, seeded from
        /// <paramref name="random"/>); runes: a plain stack.
        /// </summary>
        public static AdvancedCraftCheck TryCraft(AdvancedRecipeSpec recipe, InventoryManager bag, int tier, CharacterId? hero, bool withTrophy,
            Random random, out ItemRarity rarity)
        {
            rarity = ItemRarity.Common;
            if (bag == null || random == null)
            {
                return AdvancedCraftCheck.MissingIngredients;
            }

            bool trophy = withTrophy && recipe != null && recipe.Kind == AdvancedRecipeKind.Gear;
            AdvancedCraftCheck check = Check(recipe, tier, hero, trophy, bag.GetTotalCount);
            if (check != AdvancedCraftCheck.Ok)
            {
                return check;
            }

            ItemDefinition result = ItemDatabase.GetById(recipe.ResultItemId);
            if (result == null)
            {
                return AdvancedCraftCheck.Unknown;
            }

            // Room first (the ingredients' cells are not counted as freed: conservative, never destroys anything).
            if (!bag.CanFit(result, recipe.ResultQuantity))
            {
                return AdvancedCraftCheck.NoRoom;
            }

            MaterialQuality quality = recipe.Kind == AdvancedRecipeKind.Gear ? QualityOf(recipe, trophy) : MaterialQuality.Rustic;
            GeneratedItemInstance piece = null;
            if (recipe.Kind == AdvancedRecipeKind.Gear)
            {
                rarity = CraftTierRules.RollRarity(quality, tier, random, result.Rarity);
                piece = ProceduralItemGenerator.Generate(result, rarity, random.Next());
            }
            else
            {
                rarity = result.Rarity;
            }

            foreach (AdvancedIngredient ingredient in recipe.Ingredients)
            {
                bag.RemoveById(ingredient.ItemId, ingredient.Quantity);
            }

            if (recipe.GoldCost > 0)
            {
                bag.RemoveById(GoldItemId, recipe.GoldCost);
            }

            if (trophy)
            {
                bag.RemoveById(CreatureParts.BeastTrophy, 1);
            }

            if (piece != null)
            {
                bag.AddGeneratedItem(piece);
            }
            else
            {
                bag.AddItem(result, recipe.ResultQuantity);
            }

            EventBus.Publish(new AdvancedItemCraftedEvent(recipe.RecipeId, result.ItemId, recipe.ResultQuantity, rarity, quality));
            return AdvancedCraftCheck.Ok;
        }

        // ------------------------------------------------------------------ fusion

        private static List<InventorySlotData> Pieces(InventoryManager bag, IReadOnlyList<int> slotIndices)
        {
            var pieces = new List<InventorySlotData>();
            IReadOnlyList<InventorySlotData> slots = bag.Slots;
            foreach (int index in slotIndices)
            {
                pieces.Add(index >= 0 && index < slots.Count ? slots[index] : null);
            }

            return pieces;
        }

        public static FusionCheck CheckFusion(InventoryManager bag, IReadOnlyList<int> slotIndices, int tier, bool withTrophy)
        {
            if (bag == null || slotIndices == null)
            {
                return FusionCheck.WrongCount;
            }

            return FusionRules.Validate(Pieces(bag, slotIndices), tier, withTrophy, ItemDatabase.GetById, bag.GetTotalCount, GoldItemId, CreatureParts.BeastTrophy);
        }

        /// <summary>
        /// Fuses the bag entries <paramref name="slotIndices"/> (2–3 copies of one piece, none equipped). Success: the best
        /// piece moves one rarity up with freshly rolled affixes (refine level kept) and the others melt.
        /// Failure: the best piece is untouched, the others melt into iron (<see cref="FusionRules.ConsolationPerPiece"/> each).
        /// </summary>
        public static FusionCheck TryFuse(InventoryManager bag, IReadOnlyList<int> slotIndices, int tier, bool withTrophy, Random random,
            out FusionOutcome outcome)
        {
            outcome = default;
            FusionCheck check = CheckFusion(bag, slotIndices, tier, withTrophy);
            if (check != FusionCheck.Ok || random == null)
            {
                return check == FusionCheck.Ok ? FusionCheck.WrongCount : check;
            }

            List<InventorySlotData> pieces = Pieces(bag, slotIndices);
            outcome = FusionRules.Roll(pieces, withTrophy, ItemDatabase.GetById, random);
            InventorySlotData kept = pieces[outcome.KeptIndex];
            ItemDefinition item = ItemDatabase.GetById(kept.itemId);

            bag.RemoveById(GoldItemId, FusionRules.GoldCost(FusionRules.TargetRarity(outcome.FromRarity)));
            if (withTrophy)
            {
                bag.RemoveById(CreatureParts.BeastTrophy, 1);
            }

            for (int i = 0; i < pieces.Count; i++)
            {
                if (i != outcome.KeptIndex)
                {
                    bag.RemoveFromSlot(slotIndices[i], 1);
                }
            }

            if (outcome.Success)
            {
                GeneratedItemInstance reroll = ProceduralItemGenerator.Generate(item, outcome.ToRarity, random.Next());
                kept.generatedAffixIds = reroll.AffixIds;
                kept.generatedAffixValues = reroll.AffixValues;
                kept.generatedName = reroll.GeneratedName;
                kept.generatedRarity = reroll.Rarity;
                bag.NotifyItemDataChanged();
            }
            else if (outcome.Consolation > 0)
            {
                ItemDefinition iron = ItemDatabase.GetById(FusionRules.ConsolationItemId);
                if (iron != null)
                {
                    bag.AddItemIgnoringWeight(iron, outcome.Consolation);
                }
            }

            EventBus.Publish(new ItemFusedEvent(kept.itemId, outcome));
            return FusionCheck.Ok;
        }

        // ------------------------------------------------------------------ enchantment

        /// <summary>Vô Jorge's enchantment: spends the parts and gold and puts it on <paramref name="crafting"/>'s weapon hand.</summary>
        public static ImbueCheck TryImbue(CraftingManager crafting, InventoryManager bag, string imbueId, int tier)
        {
            ImbueSpec spec = ImbueCatalog.Get(imbueId);
            if (crafting == null || bag == null)
            {
                return ImbueCheck.Unknown;
            }

            ImbueCheck check = ImbueRules.Validate(spec, tier, bag.GetTotalCount, GoldItemId);
            if (check != ImbueCheck.Ok)
            {
                return check;
            }

            bag.RemoveById(spec.PartItemId, spec.PartQuantity);
            if (spec.GoldCost > 0)
            {
                bag.RemoveById(GoldItemId, spec.GoldCost);
            }

            crafting.SetImbue(ImbueRules.Apply(spec));
            return ImbueCheck.Ok;
        }
    }
}
