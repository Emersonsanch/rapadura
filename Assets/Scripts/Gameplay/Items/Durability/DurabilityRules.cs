using System;
using Rapadura.Gameplay.Inventory;

namespace Rapadura.Gameplay.Items.Durability
{
    /// <summary>
    /// Pure durability rules (FASE 16, Docs/Balance/README.md "Durabilidade e conserto"), unit-tested in
    /// <c>DurabilityTests</c>. No UnityEngine, no state: online (Fase 12) the server runs the very same numbers.
    /// <list type="bullet">
    /// <item>Only items with <see cref="ItemDefinition.HasDurability"/> wear: the weapon a little per attack, each armour
    /// piece a little per hit taken, and every equipped piece <see cref="DeathWearFraction"/> of its maximum per death.</item>
    /// <item>At 0 the piece is <b>broken</b>: it gives no stats at all (base, affixes, refine, options) but is never
    /// destroyed, dropped or unequipped by force — it waits for the ferreiro.</item>
    /// <item>Repair at the ferreiro: gold by the item's value × rarity × refine level (+ options), scaled by how much is
    /// missing; a broken piece costs <see cref="BrokenSurcharge"/> more.</item>
    /// </list>
    /// Save-compatible: the value lives in <see cref="InventorySlotData.currentDurability"/> (and the save's
    /// <c>EquippedEntry.durability</c>), which already existed.
    /// </summary>
    public static class DurabilityRules
    {
        /// <summary>Lost by the equipped weapon per basic attack (iron sword, 120: ~1200 attacks).</summary>
        public const float WeaponWearPerAttack = 0.1f;
        /// <summary>Lost by each equipped armour piece per hit taken.</summary>
        public const float ArmourWearPerHit = 0.15f;
        /// <summary>Share of the maximum every equipped piece loses on a death.</summary>
        public const float DeathWearFraction = 0.10f;
        /// <summary>Below this share the tooltip/forge call the piece "gasto" (worn).</summary>
        public const float WornThreshold = 0.25f;

        /// <summary>A full repair (from 0) costs this share of the item's value, before the multipliers.</summary>
        public const float RepairValueFraction = 0.25f;
        /// <summary>Each refine level adds this share to the repair cost (+10 = ×2).</summary>
        public const float RefineCostPerLevel = 0.10f;
        /// <summary>Excellent / ancient / socketed pieces cost this much more to repair.</summary>
        public const float OptionsCostBonus = 0.5f;
        /// <summary>A broken piece costs this much more (the drain of letting it break).</summary>
        public const float BrokenSurcharge = 1.25f;

        private static readonly float[] RarityMultipliers = { 1f, 1.25f, 1.5f, 2f, 2.5f };

        public static float RarityMultiplier(ItemRarity rarity)
        {
            int index = Math.Max(0, Math.Min(RarityMultipliers.Length - 1, (int)rarity));
            return RarityMultipliers[index];
        }

        public static bool IsTracked(bool hasDurability, float maxDurability) => hasDurability && maxDurability > 0f;

        public static bool IsTracked(ItemDefinition item) => item != null && IsTracked(item.HasDurability, item.MaxDurability);

        public static bool IsBroken(bool hasDurability, float maxDurability, float current) =>
            IsTracked(hasDurability, maxDurability) && current <= 0f;

        /// <summary>True when <paramref name="data"/> of <paramref name="item"/> tracks durability and reached 0.</summary>
        public static bool IsBroken(ItemDefinition item, InventorySlotData data) =>
            item != null && data != null && IsBroken(item.HasDurability, item.MaxDurability, data.currentDurability);

        /// <summary>0..1 share left (1 for untracked items).</summary>
        public static float Fraction(float maxDurability, float current)
        {
            if (maxDurability <= 0f)
            {
                return 1f;
            }

            return Math.Max(0f, Math.Min(1f, current / maxDurability));
        }

        public static bool IsWorn(float maxDurability, float current) => maxDurability > 0f && Fraction(maxDurability, current) < WornThreshold;

        /// <summary>The value after losing <paramref name="amount"/> (never below 0).</summary>
        public static float Wear(float current, float amount) => Math.Max(0f, current - Math.Max(0f, amount));

        /// <summary>What one death takes from a piece with <paramref name="maxDurability"/>.</summary>
        public static float DeathWear(float maxDurability) => Math.Max(0f, maxDurability) * DeathWearFraction;

        /// <summary>The effective rarity of an instance: the higher of the base item's and the rolled one.</summary>
        public static ItemRarity EffectiveRarity(ItemDefinition item, InventorySlotData data)
        {
            ItemRarity baseRarity = item != null ? item.Rarity : ItemRarity.Common;
            ItemRarity rolled = data != null ? data.generatedRarity : ItemRarity.Common;
            return rolled > baseRarity ? rolled : baseRarity;
        }

        /// <summary>Gold of a repair from 0 to full (without the broken surcharge), at least 1.</summary>
        public static int FullRepairCost(int itemValue, ItemRarity rarity, int refineLevel, bool hasOptions)
        {
            double cost = Math.Max(1, itemValue) * RepairValueFraction * RarityMultiplier(rarity)
                * (1.0 + Math.Max(0, refineLevel) * RefineCostPerLevel) * (hasOptions ? 1.0 + OptionsCostBonus : 1.0);
            return Math.Max(1, (int)Math.Ceiling(cost - 1e-4));
        }

        /// <summary>
        /// Gold to bring <paramref name="current"/> back to <paramref name="maxDurability"/>: the full cost × the missing
        /// share (× <see cref="BrokenSurcharge"/> at 0), rounded up, at least 1. 0 when nothing is missing.
        /// </summary>
        public static int RepairCost(int itemValue, ItemRarity rarity, int refineLevel, bool hasOptions, float maxDurability, float current)
        {
            if (maxDurability <= 0f || current >= maxDurability)
            {
                return 0;
            }

            double missing = (maxDurability - Math.Max(0f, current)) / maxDurability;
            double cost = FullRepairCost(itemValue, rarity, refineLevel, hasOptions) * missing * (current <= 0f ? BrokenSurcharge : 1.0);
            return Math.Max(1, (int)Math.Ceiling(cost - 1e-4));
        }

        /// <summary><see cref="RepairCost(int, ItemRarity, int, bool, float, float)"/> of an item instance (0 when untracked).</summary>
        public static int RepairCost(ItemDefinition item, InventorySlotData data)
        {
            if (!IsTracked(item) || data == null)
            {
                return 0;
            }

            return RepairCost(item.Value, EffectiveRarity(item, data), data.refineLevel, data.HasItemOptions, item.MaxDurability, data.currentDurability);
        }
    }
}
