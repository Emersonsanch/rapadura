using System;
using System.Collections.Generic;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.Mounts;

namespace Rapadura.Gameplay.Inventory
{
    /// <summary>How heavy the load feels (drives the toasts and the HUD color).</summary>
    public enum LoadBand
    {
        /// <summary>No penalty.</summary>
        Light = 0,
        /// <summary>Mounted only: above <see cref="EncumbranceRules.MountFreeRatio"/>, the ride slows down.</summary>
        Heavy = 1,
        /// <summary>Over capacity: on foot the hero slows; mounted the ride walks, no gallop.</summary>
        Overloaded = 2,
    }

    /// <summary>One evaluation of the carry load (<see cref="EncumbranceRules.Evaluate"/>): what the motor, the HUD and the toasts read.</summary>
    public readonly struct EncumbranceSnapshot
    {
        /// <summary>Backpack + equipped weight.</summary>
        public readonly float Load;
        /// <summary>The hero's own capacity.</summary>
        public readonly float HeroCapacity;
        /// <summary>The mount species' capacity at its level (0 on foot).</summary>
        public readonly float MountCapacity;
        public readonly bool Mounted;

        public EncumbranceSnapshot(float load, float heroCapacity, bool mounted, float mountCapacity)
        {
            Load = Math.Max(0f, load);
            HeroCapacity = Math.Max(0f, heroCapacity);
            Mounted = mounted;
            MountCapacity = mounted ? Math.Max(0f, mountCapacity) : 0f;
        }

        /// <summary>The capacity that counts now: the ride's (mount + half the hero) when mounted, else the hero's.</summary>
        public float Capacity => Mounted ? EncumbranceRules.RideCapacity(MountCapacity, HeroCapacity) : HeroCapacity;

        public float Ratio => EncumbranceRules.LoadRatio(Load, Capacity);

        /// <summary>Speed factor (0..1) for the current stance (mounted or on foot).</summary>
        public float SpeedFactor => Mounted ? EncumbranceRules.MountSpeedFactor(Ratio) : EncumbranceRules.HeroSpeedFactor(Ratio);

        /// <summary>On foot always true (the hero has no gallop); mounted only up to full capacity.</summary>
        public bool CanGallop => !Mounted || EncumbranceRules.CanGallop(Ratio);

        public LoadBand Band => EncumbranceRules.Band(Ratio, Mounted);
    }

    /// <summary>
    /// Carry weight and encumbrance (Docs/Balance/README.md "Peso e carga", Docs/Mounts.md §2.7), plain C# for the
    /// future server, like <c>MountRules</c>:
    /// <list type="bullet">
    /// <item><b>Load</b> = everything the hero carries: the backpack <b>and</b> the equipped pieces (Tibia "cap"), each
    /// stack at <see cref="ItemWeightRules.UnitWeight(ItemDefinition)"/> × quantity. Gold weighs nothing.</item>
    /// <item><b>Hero capacity</b> = 60 + 2 per level above 1 + 1 per Vitality point (<see cref="HeroCapacity"/>).</item>
    /// <item><b>Mount capacity</b> = the species' base (<see cref="MountBaseCapacity"/>, pack animals carry most) ×
    /// (1 + 2% per mount level above 1). Riding, the backpack stays on the cousin's back, so the ride's capacity is
    /// mount + half the hero's (<see cref="RideCapacity"/>).</item>
    /// <item><b>Mounted speed</b> (<see cref="MountSpeedFactor"/>, r = load ÷ ride capacity): ×1 up to r = 0.5, then
    /// linear to ×0.5 at r = 1; over capacity ×0.5 − 0.4·(r − 1), never below ×0.3, and no gallop.</item>
    /// <item><b>On foot</b> (<see cref="HeroSpeedFactor"/>, r = load ÷ hero capacity, mild): ×1 up to r = 1, then
    /// ×(1 − 0.3·(r − 1)), never below ×0.6.</item>
    /// <item>The factor only ever lowers the speed: it multiplies the already-capped buff/mount multiplier and the result
    /// keeps the <c>BuffController</c> floor (<see cref="MountRules.MinMoveMultiplier"/>); rooted (0) stays 0. So the
    /// §9.2 server speed cap is still an upper bound.</item>
    /// </list>
    /// </summary>
    public static class EncumbranceRules
    {
        public const float HeroBaseCapacity = 60f;
        public const float HeroCapacityPerLevel = 2f;
        public const float HeroCapacityPerVitality = 1f;

        /// <summary>Share of the hero's own capacity that still counts while riding (the backpack on the cousin's back).</summary>
        public const float RiderCapacityShare = 0.5f;

        /// <summary>Mount capacity growth per mount level above 1.</summary>
        public const float MountCapacityPerLevel = 0.02f;

        /// <summary>Mounted: no penalty up to this load ratio.</summary>
        public const float MountFreeRatio = 0.5f;
        /// <summary>Mounted: the factor at exactly full capacity (r = 1).</summary>
        public const float MountFactorAtFull = 0.5f;
        /// <summary>Mounted, over capacity: factor lost per extra 100% of load.</summary>
        public const float MountOverloadSlope = 0.4f;
        public const float MountMinFactor = 0.3f;

        /// <summary>On foot, over capacity: factor lost per extra 100% of load.</summary>
        public const float HeroOverloadSlope = 0.3f;
        public const float HeroMinFactor = 0.6f;

        /// <summary>Fallback capacity by size for a species missing from <see cref="MountBaseCapacity"/>.</summary>
        public static float CapacityForSize(MountSize size)
        {
            switch (size)
            {
                case MountSize.Pequeno: return 100f;
                case MountSize.Grande: return 300f;
                default: return 150f;
            }
        }

        /// <summary>
        /// Base carry capacity (mount level 1) of the 30 species of Docs/Mounts.md §3. Pack animals (the Asno de
        /// Caravana above all) carry most; birds, cats and canids little; the big beasts a lot.
        /// </summary>
        public static readonly IReadOnlyDictionary<string, float> MountBaseCapacity = new Dictionary<string, float>(StringComparer.Ordinal)
        {
            // Comuns
            { "mount_pony", 120f },
            { "mount_big_dog", 100f },
            { "mount_capybara", 110f },
            { "mount_mountain_goat", 130f },
            { "mount_pack_donkey", 450f },
            { "mount_rhea", 70f },
            // Incomuns
            { "mount_pinto_horse", 180f },
            { "mount_enchanted_donkey", 380f },
            { "mount_giant_armadillo", 160f },
            { "mount_maned_wolf", 90f },
            { "mount_caipora_peccary", 130f },
            { "mount_river_seahorse", 130f },
            { "mount_giant_anteater", 150f },
            { "mount_golden_scarab", 170f },
            // Raras
            { "mount_snow_tiger", 120f },
            { "mount_woolly_mammoth", 420f },
            { "mount_unicorn", 170f },
            { "mount_hippogriff", 110f },
            { "mount_griffin", 140f },
            { "mount_light_jaguar", 110f },
            { "mount_carbuncle", 100f },
            { "mount_fire_rhea", 80f },
            // Épicas
            { "mount_pegasus", 160f },
            { "mount_sleipnir", 240f },
            { "mount_kirin", 180f },
            { "mount_boitata", 200f },
            { "mount_boi_bumba", 360f },
            // Lendárias
            { "mount_sertao_phoenix", 130f },
            { "mount_dragon_hatchling", 220f },
            { "mount_elder_tortoise", 400f },
        };

        // ------------------------------------------------------------------
        // Capacity
        // ------------------------------------------------------------------

        /// <summary>The hero's own capacity ("capacidade", Tibia style).</summary>
        public static float HeroCapacity(int level, int vitality)
        {
            return HeroBaseCapacity + HeroCapacityPerLevel * Math.Max(0, level - 1) + HeroCapacityPerVitality * Math.Max(0, vitality);
        }

        /// <summary>A species' capacity at <paramref name="mountLevel"/> (unknown ids fall back to <see cref="CapacityForSize"/>).</summary>
        public static float MountCapacity(string mountId, MountSize size, int mountLevel)
        {
            float baseCapacity = mountId != null && MountBaseCapacity.TryGetValue(mountId, out float known) ? known : CapacityForSize(size);
            return baseCapacity * (1f + MountCapacityPerLevel * Math.Max(0, mountLevel - 1));
        }

        /// <summary>What a ride carries: the mount plus <see cref="RiderCapacityShare"/> of the hero's capacity.</summary>
        public static float RideCapacity(float mountCapacity, float heroCapacity)
        {
            return Math.Max(0f, mountCapacity) + RiderCapacityShare * Math.Max(0f, heroCapacity);
        }

        /// <summary>load ÷ capacity (0 with no capacity and no load; very large with no capacity and some load).</summary>
        public static float LoadRatio(float load, float capacity)
        {
            if (load <= 0f)
            {
                return 0f;
            }

            return capacity > 0f ? load / capacity : float.MaxValue;
        }

        // ------------------------------------------------------------------
        // Load
        // ------------------------------------------------------------------

        /// <summary>Weight of every non-empty entry of <paramref name="entries"/> (null-tolerant; unknown ids weigh 0).</summary>
        public static float WeightOf(IEnumerable<InventorySlotData> entries, Func<string, ItemDefinition> resolve)
        {
            float total = 0f;
            if (entries == null || resolve == null)
            {
                return total;
            }

            foreach (InventorySlotData entry in entries)
            {
                if (entry == null || entry.IsEmpty)
                {
                    continue;
                }

                total += ItemWeightRules.StackWeight(resolve(entry.itemId), entry.quantity);
            }

            return total;
        }

        /// <summary>The carried load from a saved inventory (world map: the carried bag).</summary>
        public static float WeightOf(InventorySaveState state, Func<string, ItemDefinition> resolve)
        {
            if (state == null || resolve == null)
            {
                return 0f;
            }

            float total = WeightOf(state.slots, resolve);
            if (state.equipped != null)
            {
                foreach (InventorySaveState.EquippedEntry entry in state.equipped)
                {
                    if (entry != null && !string.IsNullOrEmpty(entry.itemId))
                    {
                        total += ItemWeightRules.UnitWeight(resolve(entry.itemId));
                    }
                }
            }

            return total;
        }

        // ------------------------------------------------------------------
        // Speed
        // ------------------------------------------------------------------

        /// <summary>Mounted speed factor for a load ratio (see the class summary).</summary>
        public static float MountSpeedFactor(float ratio)
        {
            if (ratio <= MountFreeRatio)
            {
                return 1f;
            }

            if (ratio <= 1f)
            {
                float t = (ratio - MountFreeRatio) / (1f - MountFreeRatio);
                return 1f + (MountFactorAtFull - 1f) * t;
            }

            return Math.Max(MountMinFactor, MountFactorAtFull - MountOverloadSlope * (ratio - 1f));
        }

        /// <summary>On-foot speed factor for a load ratio (mild; only over capacity).</summary>
        public static float HeroSpeedFactor(float ratio)
        {
            if (ratio <= 1f)
            {
                return 1f;
            }

            return Math.Max(HeroMinFactor, 1f - HeroOverloadSlope * (ratio - 1f));
        }

        /// <summary>Gallop only while the ride is not over capacity.</summary>
        public static bool CanGallop(float ratio) => ratio <= 1f;

        /// <summary>
        /// Applies a load factor to a motor multiplier (buffs or the ride): 0 (rooted) stays 0, otherwise the product
        /// keeps the <see cref="MountRules.MinMoveMultiplier"/> floor and never goes above <paramref name="multiplier"/>.
        /// </summary>
        public static float ApplyFactor(float multiplier, float factor)
        {
            if (multiplier <= 0f)
            {
                return 0f;
            }

            float clampedFactor = Math.Min(1f, Math.Max(0f, factor));
            return Math.Min(multiplier, Math.Max(MountRules.MinMoveMultiplier, multiplier * clampedFactor));
        }

        /// <summary>Everything at once (see <see cref="EncumbranceSnapshot"/>).</summary>
        public static EncumbranceSnapshot Evaluate(float load, int heroLevel, int vitality, bool mounted, string mountId, MountSize size, int mountLevel)
        {
            return new EncumbranceSnapshot(load, HeroCapacity(heroLevel, vitality), mounted, mounted ? MountCapacity(mountId, size, mountLevel) : 0f);
        }

        /// <summary>Which band a ratio is in.</summary>
        public static LoadBand Band(float ratio, bool mounted)
        {
            if (ratio > 1f)
            {
                return LoadBand.Overloaded;
            }

            return mounted && ratio > MountFreeRatio ? LoadBand.Heavy : LoadBand.Light;
        }
    }
}
