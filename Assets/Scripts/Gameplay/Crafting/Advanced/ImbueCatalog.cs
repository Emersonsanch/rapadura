using System;
using System.Collections.Generic;
using Rapadura.Gameplay.Skills;

namespace Rapadura.Gameplay.Crafting.Advanced
{
    /// <summary>One temporary weapon enchantment ("benzedura de arma" by Vô Jorge, made from creature parts).</summary>
    public sealed class ImbueSpec
    {
        public readonly string ImbueId;
        public readonly int Tier;
        public readonly string PartItemId;
        public readonly int PartQuantity;
        public readonly int GoldCost;
        public readonly StatType Stat;
        public readonly ModifierApplication Application;
        public readonly float Value;

        public ImbueSpec(string imbueId, int tier, string partItemId, int partQuantity, int goldCost, StatType stat,
            ModifierApplication application, float value)
        {
            ImbueId = imbueId;
            Tier = tier;
            PartItemId = partItemId;
            PartQuantity = partQuantity;
            GoldCost = goldCost;
            Stat = stat;
            Application = application;
            Value = value;
        }

        public string NameKey => "ui.craft.imbue." + ImbueId + ".name";
        public string EffectKey => "ui.craft.imbue." + ImbueId + ".effect";
    }

    /// <summary>
    /// The weapon enchantments (Docs/Balance/README.md "Crafting avançado"): Vô Jorge rubs creature parts into the blade
    /// and the weapon in hand carries one bonus for <see cref="ImbueRules.DurationEncounters"/> fights.
    /// Every bonus is at most the strongest road-event blessing (AttackDamage +8%) and shares its slot: a fight that
    /// already has a road blessing pauses the enchantment (<see cref="ImbueRules.AppliesThisFight"/>), so the power
    /// stack of §14 keeps the same ceiling. <c>tools/balance/crafting_stack.py</c> parses this table.
    /// </summary>
    public static class ImbueCatalog
    {
        public const string WolfFang = "imbue_presa_lobo";
        public const string SpiderSilk = "imbue_seda_aranha";
        public const string Stinger = "imbue_ferrao";
        public const string GolemShard = "imbue_lasca_golem";
        public const string BatWing = "imbue_asa_morcego";

        public static readonly IReadOnlyList<ImbueSpec> All = new[]
        {
            new ImbueSpec(WolfFang, 1, CreatureParts.WolfFang, 4, 10, StatType.AttackDamage, ModifierApplication.PercentAdditive, 0.06f),
            new ImbueSpec(SpiderSilk, 1, CreatureParts.SpiderSilk, 4, 10, StatType.AttackSpeed, ModifierApplication.PercentAdditive, 0.06f),
            new ImbueSpec(Stinger, 2, CreatureParts.ScorpionStinger, 4, 20, StatType.CriticalChance, ModifierApplication.Flat, 0.04f),
            new ImbueSpec(GolemShard, 3, CreatureParts.GolemShard, 4, 30, StatType.Defense, ModifierApplication.PercentAdditive, 0.10f),
            new ImbueSpec(BatWing, 4, CreatureParts.BatWing, 4, 40, StatType.HealthRegen, ModifierApplication.Flat, 2f),
        };

        public static ImbueSpec Get(string imbueId)
        {
            foreach (ImbueSpec spec in All)
            {
                if (spec.ImbueId == imbueId)
                {
                    return spec;
                }
            }

            return null;
        }
    }

    /// <summary>The enchantment on the weapon hand: which one and how many fights are left (JSON-serializable).</summary>
    [Serializable]
    public struct ActiveImbue
    {
        public string imbueId;
        public int encountersRemaining;

        public ActiveImbue(string imbueId, int encountersRemaining)
        {
            this.imbueId = imbueId;
            this.encountersRemaining = encountersRemaining;
        }

        public bool IsActive => !string.IsNullOrEmpty(imbueId) && encountersRemaining > 0 && ImbueCatalog.Get(imbueId) != null;

        public static ActiveImbue None => new ActiveImbue(null, 0);
    }

    /// <summary>Why an enchantment can't be applied.</summary>
    public enum ImbueCheck
    {
        Ok,
        Unknown,
        Locked,
        MissingParts,
        MissingGold,
    }

    /// <summary>Pure rules of the weapon enchantment (server-friendly).</summary>
    public static class ImbueRules
    {
        /// <summary>Fights an enchantment lasts (only fights where it actually applied count).</summary>
        public const int DurationEncounters = 8;
        /// <summary>BuffController source of the enchantment's modifier in the arena.</summary>
        public const string EffectSourceKey = "craft_imbue";
        /// <summary>The road-event blessing's source (<c>WorldEventState.BuffSource</c>): when present the enchantment waits.</summary>
        public const string RoadBlessingSource = "world_event";

        public static ImbueCheck Validate(ImbueSpec spec, int tier, Func<string, int> countInBag, string goldItemId)
        {
            if (spec == null)
            {
                return ImbueCheck.Unknown;
            }

            if (!CraftTierRules.IsUnlocked(spec.Tier, tier))
            {
                return ImbueCheck.Locked;
            }

            if (countInBag == null || countInBag(spec.PartItemId) < spec.PartQuantity)
            {
                return ImbueCheck.MissingParts;
            }

            if (spec.GoldCost > 0 && countInBag(goldItemId) < spec.GoldCost)
            {
                return ImbueCheck.MissingGold;
            }

            return ImbueCheck.Ok;
        }

        /// <summary>A new enchantment replaces whatever was on the weapon (they never stack) with a full duration.</summary>
        public static ActiveImbue Apply(ImbueSpec spec) => new ActiveImbue(spec.ImbueId, DurationEncounters);

        /// <summary>The enchantment works this fight unless a road blessing already does or no weapon is in hand.</summary>
        public static bool AppliesThisFight(ActiveImbue active, bool hasRoadBlessing, bool hasWeaponInHand)
        {
            return active.IsActive && !hasRoadBlessing && hasWeaponInHand;
        }

        /// <summary>One fight ended: counts down only when the enchantment applied in it.</summary>
        public static ActiveImbue CountEncounter(ActiveImbue active, bool appliedThisFight, out bool expired)
        {
            expired = false;
            if (!active.IsActive || !appliedThisFight)
            {
                return active.IsActive ? active : ActiveImbue.None;
            }

            int left = active.encountersRemaining - 1;
            if (left <= 0)
            {
                expired = true;
                return ActiveImbue.None;
            }

            return new ActiveImbue(active.imbueId, left);
        }

        /// <summary>Loaded state: unknown ids or out-of-range counts become "none" / clamped.</summary>
        public static ActiveImbue Sanitize(ActiveImbue saved)
        {
            if (ImbueCatalog.Get(saved.imbueId) == null || saved.encountersRemaining <= 0)
            {
                return ActiveImbue.None;
            }

            return new ActiveImbue(saved.imbueId, Math.Min(DurationEncounters, saved.encountersRemaining));
        }

        /// <summary>The modifier the arena applies (lasts the whole encounter).</summary>
        public static StatModifierDefinition BuildEffect(ImbueSpec spec)
        {
            return new StatModifierDefinition
            {
                displayName = EffectSourceKey,
                affectedStat = spec.Stat,
                application = spec.Application,
                value = spec.Value,
                durationSeconds = 0f,
            };
        }
    }
}
