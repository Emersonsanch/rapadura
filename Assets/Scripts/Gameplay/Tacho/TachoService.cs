using System.Collections.Generic;
using Rapadura.Core.DI;
using Rapadura.Core.Events;
using Rapadura.Gameplay.Achievements;
using Rapadura.Gameplay.Inventory;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.Refining;

namespace Rapadura.Gameplay.Tacho
{
    /// <summary>What the Tacho window shows for one recipe (and main item) before the player confirms (pure snapshot).</summary>
    public readonly struct TachoPreview
    {
        public readonly TachoRecipe Recipe;
        public readonly RefineTarget? Target;
        public readonly InventorySlotData Data;
        public readonly ItemDefinition MainItem;
        /// <summary>Refine level (wings/protection) or wing level (upgrade) of the main item; 0 without one.</summary>
        public readonly int MainLevel;
        public readonly float Chance;
        public readonly int Gold;
        public readonly int GoldOwned;
        public readonly List<TachoIngredient> Cost;
        public readonly TachoCheck Check;

        public TachoPreview(TachoRecipe recipe, RefineTarget? target, InventorySlotData data, ItemDefinition mainItem, int mainLevel,
            float chance, int gold, int goldOwned, List<TachoIngredient> cost, TachoCheck check)
        {
            Recipe = recipe;
            Target = target;
            Data = data;
            MainItem = mainItem;
            MainLevel = mainLevel;
            Chance = chance;
            Gold = gold;
            GoldOwned = goldOwned;
            Cost = cost;
            Check = check;
        }

        public bool CanTry => Check == TachoCheck.Ok;
    }

    /// <summary>Raised after a Tacho roll was applied (success or failure).</summary>
    public readonly struct TachoCombinedEvent : IGameEvent
    {
        public readonly TachoOutcome Outcome;
        /// <summary>The item made did not fit the Mochila and went to the Baú da Vó Edna.</summary>
        public readonly bool SentToChest;

        public TachoCombinedEvent(TachoOutcome outcome, bool sentToChest)
        {
            Outcome = outcome;
            SentToChest = sentToChest;
        }
    }

    /// <summary>
    /// Applies <see cref="TachoRules"/> to a live <see cref="InventoryManager"/> (the hub's carried bag): checks and spends the
    /// materials/stones/gold (<see cref="InventoryManager.RemoveById"/>), rolls with the given seed and applies the outcome.
    /// <list type="bullet">
    /// <item>Wings made/upgraded: the main item is consumed only on success (bag entry, or the equipped slot through
    /// <see cref="InventoryManager.RemoveEquipped"/>), the new wing goes to the bag (an equipped wing is re-equipped at
    /// its new level) and, if it does not fit the grid, to the Baú da Vó Edna, never lost.</item>
    /// <item>Proteção do Caos: writes the refine level in place, like the forge (<see cref="RefineService"/>).</item>
    /// <item>Fruits: the fruit goes to the bag (or the Baú).</item>
    /// <item>Failure: the spent materials are gone, the main item is untouched (kid-friendly rule).</item>
    /// </list>
    /// Feeds the wing achievements (<see cref="AchievementConditionType.WingsCrafted"/>, <see cref="AchievementConditionType.WingLevelReached"/>).
    /// Online (Fase 12) this whole call runs on the server with its own seed.
    /// </summary>
    public static class TachoService
    {
        /// <summary>The main items a recipe can work on: refinable pieces (wings, protection) or wings of its style and level.</summary>
        public static List<RefineTarget> ListMainItems(InventoryManager bag, TachoRecipe recipe)
        {
            var list = new List<RefineTarget>();
            if (bag == null || recipe == null || !recipe.NeedsMainItem)
            {
                return list;
            }

            if (recipe.Kind != TachoRecipeKind.UpgradeWings)
            {
                return RefineService.ListTargets(bag);
            }

            if (IsUpgradeMain(RefineService.Resolve(bag, RefineTarget.Equipped(EquipmentSlot.Wings)), recipe))
            {
                list.Add(RefineTarget.Equipped(EquipmentSlot.Wings));
            }

            IReadOnlyList<InventorySlotData> slots = bag.Slots;
            for (int i = 0; i < slots.Count; i++)
            {
                if (IsUpgradeMain(slots[i], recipe))
                {
                    list.Add(RefineTarget.Bag(i));
                }
            }

            return list;
        }

        /// <summary>Every wing the hero has below level 3 (equipped first, then the bag): the upgrade rows.</summary>
        public static List<RefineTarget> ListUpgradableWings(InventoryManager bag)
        {
            var list = new List<RefineTarget>();
            if (bag == null)
            {
                return list;
            }

            if (TachoRules.UpgradeFor(RefineService.Resolve(bag, RefineTarget.Equipped(EquipmentSlot.Wings))?.itemId) != null)
            {
                list.Add(RefineTarget.Equipped(EquipmentSlot.Wings));
            }

            IReadOnlyList<InventorySlotData> slots = bag.Slots;
            for (int i = 0; i < slots.Count; i++)
            {
                InventorySlotData data = slots[i];
                if (data != null && !data.IsEmpty && TachoRules.UpgradeFor(data.itemId) != null)
                {
                    list.Add(RefineTarget.Bag(i));
                }
            }

            return list;
        }

        private static bool IsUpgradeMain(InventorySlotData data, TachoRecipe recipe) =>
            data != null && !data.IsEmpty && WingCatalog.TryParse(data.itemId, out WingStyle style, out int level)
            && style == recipe.Style && level == recipe.FromLevel;

        public static TachoPreview Preview(InventoryManager bag, TachoRecipe recipe, RefineTarget? target)
        {
            if (recipe == null)
            {
                return new TachoPreview(null, target, null, null, 0, 0f, 0, 0, new List<TachoIngredient>(), TachoCheck.NoMainItem);
            }

            InventorySlotData data = recipe != null && recipe.NeedsMainItem && target.HasValue ? RefineService.Resolve(bag, target.Value) : null;
            ItemDefinition item = data != null ? ItemDatabase.GetById(data.itemId) : null;
            int level = MainLevelOf(recipe, data);
            int gold = TachoRules.GoldFor(recipe, level);
            int goldOwned = bag != null ? bag.GetTotalCount(TachoRules.Gold) : 0;
            bool hasMain = data != null && (recipe.Kind == TachoRecipeKind.UpgradeWings || RefineRules.IsRefinable(item));
            TachoCheck check = bag == null
                ? TachoCheck.MissingMaterial
                : TachoRules.Check(recipe, hasMain, level, data?.itemId, bag.GetTotalCount);
            return new TachoPreview(recipe, target, data, item, level, TachoRules.ChanceFor(recipe, level), gold, goldOwned,
                TachoRules.CostFor(recipe, level), check);
        }

        private static int MainLevelOf(TachoRecipe recipe, InventorySlotData data)
        {
            if (recipe == null || data == null)
            {
                return 0;
            }

            if (recipe.Kind == TachoRecipeKind.UpgradeWings)
            {
                return WingCatalog.TryParse(data.itemId, out _, out int wingLevel) ? wingLevel : 0;
            }

            return RefineRules.ClampLevel(data.refineLevel);
        }

        /// <summary>
        /// Spends the cost, rolls with <paramref name="seed"/> and applies the outcome. False (nothing spent) when the
        /// preview's check fails. <paramref name="sentToChest"/>: the item made went to the Baú (Mochila full).
        /// </summary>
        public static bool TryCombine(InventoryManager bag, TachoRecipe recipe, RefineTarget? target, int seed,
            out TachoOutcome outcome, out TachoCheck check, out bool sentToChest, VillageChestStorage chest = null)
        {
            outcome = default;
            sentToChest = false;
            TachoPreview preview = Preview(bag, recipe, target);
            check = preview.Check;
            if (!preview.CanTry)
            {
                return false;
            }

            foreach (TachoIngredient ingredient in preview.Cost)
            {
                if (!bag.RemoveById(ingredient.ItemId, ingredient.Quantity))
                {
                    check = TachoCheck.MissingMaterial;
                    return false;
                }
            }

            if (preview.Gold > 0)
            {
                bag.RemoveById(TachoRules.Gold, preview.Gold);
            }

            outcome = TachoRules.Attempt(recipe, preview.MainLevel, seed);
            if (outcome.Success)
            {
                sentToChest = ApplySuccess(bag, preview, outcome, chest);
            }

            FeedAchievements(outcome);
            EventBus.Publish(new TachoCombinedEvent(outcome, sentToChest));
            return true;
        }

        private static bool ApplySuccess(InventoryManager bag, TachoPreview preview, TachoOutcome outcome, VillageChestStorage chest)
        {
            TachoRecipe recipe = preview.Recipe;
            if (recipe.Kind == TachoRecipeKind.ProtectedRefine)
            {
                preview.Data.refineLevel = outcome.ToLevel;
                bag.NotifyItemDataChanged(preview.Target.Value.IsEquipped ? preview.Target.Value.EquipSlot : EquipmentSlot.None);
                return false;
            }

            bool reequip = false;
            if (recipe.NeedsMainItem && preview.Target.HasValue)
            {
                RefineTarget main = preview.Target.Value;
                if (main.IsEquipped)
                {
                    reequip = recipe.Kind == TachoRecipeKind.UpgradeWings;
                    bag.RemoveEquipped(main.EquipSlot);
                }
                else
                {
                    bag.RemoveFromSlot(main.SlotIndex, 1);
                }
            }

            return !Give(bag, outcome.ResultItemId, reequip, chest);
        }

        /// <summary>One <paramref name="itemId"/> into the bag (re-equipped when asked); the Baú when it does not fit. True = in the bag.</summary>
        private static bool Give(InventoryManager bag, string itemId, bool equip, VillageChestStorage chest)
        {
            ItemDefinition item = ItemDatabase.GetById(itemId);
            if (item == null)
            {
                return true;
            }

            if (bag.AddItem(item, 1) == 0)
            {
                if (equip)
                {
                    IReadOnlyList<InventorySlotData> slots = bag.Slots;
                    for (int i = slots.Count - 1; i >= 0; i--)
                    {
                        if (slots[i] != null && !slots[i].IsEmpty && slots[i].itemId == itemId)
                        {
                            bag.EquipFromSlot(i);
                            break;
                        }
                    }
                }

                return true;
            }

            (chest ?? bag.OverflowChest ?? VillageChestStorage.ResolveShared()).EnqueueOverflow(new InventorySlotData
            {
                itemId = itemId,
                quantity = 1,
                currentDurability = item.MaxDurability,
            });
            return false;
        }

        private static void FeedAchievements(TachoOutcome outcome)
        {
            if (!outcome.Success || !ServiceLocator.TryGet(out AchievementManager achievements) || achievements == null)
            {
                return;
            }

            if (outcome.Kind == TachoRecipeKind.CreateWings)
            {
                achievements.RecordProgress(AchievementConditionType.WingsCrafted, 1, outcome.ResultItemId);
            }

            if (outcome.Kind == TachoRecipeKind.CreateWings || outcome.Kind == TachoRecipeKind.UpgradeWings)
            {
                achievements.RecordMax(AchievementConditionType.WingLevelReached, outcome.ToLevel);
            }
        }
    }
}
