using System;
using System.Collections.Generic;
using Rapadura.Gameplay.Skills;

namespace Rapadura.Gameplay.Talents
{
    /// <summary>What a cousin trains by using it (Tibia's sword fighting / shielding / magic level).</summary>
    public enum ProficiencyKind
    {
        /// <summary>"Golpe": basic attacks swung in a fight.</summary>
        Weapon = 0,
        /// <summary>"Magia": mana spent on skills.</summary>
        Magic = 1,
        /// <summary>"Defesa": hits taken.</summary>
        Shielding = 2
    }

    /// <summary>One trained proficiency of one cousin: level and the tries collected towards the next one.</summary>
    [Serializable]
    public class ProficiencyEntry
    {
        public string kind;
        public int level;
        public long tries;
    }

    /// <summary>Per-kind constants: tries to the first level, the bonus it gives per level.</summary>
    public sealed class ProficiencySpec
    {
        public readonly ProficiencyKind Kind;
        /// <summary>Tries from level 0 to 1; each next level costs <see cref="ProficiencyRules.TriesGrowth"/> times more.</summary>
        public readonly int BaseTries;
        public readonly StatType Stat;
        public readonly ModifierApplication Application;
        public readonly float ValuePerLevel;

        public ProficiencySpec(ProficiencyKind kind, int baseTries, StatType stat, ModifierApplication application, float valuePerLevel)
        {
            Kind = kind;
            BaseTries = baseTries;
            Stat = stat;
            Application = application;
            ValuePerLevel = valuePerLevel;
        }

        public float MaxValue => ValuePerLevel * ProficiencyRules.MaxLevel;
    }

    /// <summary>
    /// Proficiencies that rise with use (Docs/Balance/README.md §26): every swing in a fight, every mana point spent on a
    /// skill and every hit taken is one "try"; the tries a level needs grow by <see cref="TriesGrowth"/> per level (each
    /// level takes 25% longer than the last: diminishing returns in time) up to <see cref="MaxLevel"/>. The bonus is small
    /// and linear per level. Pure: online (FASE 12) the server counts the tries with the same numbers.
    /// </summary>
    public static class ProficiencyRules
    {
        public const int MaxLevel = 20;
        public const float TriesGrowth = 1.25f;
        /// <summary>A swing only trains when the hero hit or was hit this recently (no training on thin air).</summary>
        public const float CombatWindowSeconds = 4f;

        public const int WeaponBaseTries = 50;
        public const int MagicBaseTries = 400;
        public const int ShieldingBaseTries = 40;
        public const float WeaponAttackSpeedPerLevel = 0.00075f;
        public const float MagicCostReductionPerLevel = 0.0025f;
        public const float ShieldingDefensePerLevel = 0.0025f;

        public static readonly IReadOnlyList<ProficiencySpec> Specs = new[]
        {
            new ProficiencySpec(ProficiencyKind.Weapon, WeaponBaseTries, StatType.AttackSpeed, ModifierApplication.Flat, WeaponAttackSpeedPerLevel),
            new ProficiencySpec(ProficiencyKind.Magic, MagicBaseTries, StatType.ResourceCostReduction, ModifierApplication.Flat, MagicCostReductionPerLevel),
            new ProficiencySpec(ProficiencyKind.Shielding, ShieldingBaseTries, StatType.Defense, ModifierApplication.PercentAdditive, ShieldingDefensePerLevel),
        };

        public static ProficiencySpec GetSpec(ProficiencyKind kind)
        {
            foreach (ProficiencySpec spec in Specs)
            {
                if (spec.Kind == kind)
                {
                    return spec;
                }
            }

            return null;
        }

        /// <summary>Tries from <paramref name="level"/> to the next (0 at the cap).</summary>
        public static long TriesToNext(ProficiencyKind kind, int level)
        {
            ProficiencySpec spec = GetSpec(kind);
            if (spec == null || level >= MaxLevel)
            {
                return 0;
            }

            return Math.Max(1L, (long)Math.Round(spec.BaseTries * Math.Pow(TriesGrowth, Math.Max(0, level))));
        }

        /// <summary>Every try from level 0 to <paramref name="level"/>.</summary>
        public static long TotalTriesTo(ProficiencyKind kind, int level)
        {
            long total = 0;
            for (int i = 0; i < Math.Min(level, MaxLevel); i++)
            {
                total += TriesToNext(kind, i);
            }

            return total;
        }

        /// <summary>Adds <paramref name="amount"/> tries. Returns levels gained; tries at the cap are dropped.</summary>
        public static int AddTries(ProficiencyEntry entry, long amount)
        {
            if (entry == null || amount <= 0 || !TryParse(entry.kind, out ProficiencyKind kind) || entry.level >= MaxLevel)
            {
                return 0;
            }

            int gained = 0;
            entry.tries += amount;
            while (entry.level < MaxLevel && entry.tries >= TriesToNext(kind, entry.level))
            {
                entry.tries -= TriesToNext(kind, entry.level);
                entry.level++;
                gained++;
            }

            if (entry.level >= MaxLevel)
            {
                entry.tries = 0;
            }

            return gained;
        }

        /// <summary>0..1 progress to the next level (1 at the cap).</summary>
        public static float Progress(ProficiencyEntry entry)
        {
            if (entry == null || !TryParse(entry.kind, out ProficiencyKind kind))
            {
                return 0f;
            }

            long need = TriesToNext(kind, entry.level);
            return need <= 0 ? 1f : Math.Max(0f, Math.Min(1f, (float)entry.tries / need));
        }

        public static float Bonus(ProficiencyKind kind, int level)
        {
            ProficiencySpec spec = GetSpec(kind);
            return spec == null ? 0f : spec.ValuePerLevel * Math.Max(0, Math.Min(MaxLevel, level));
        }

        /// <summary>Whether a swing at <paramref name="now"/> trains: the hero dealt or took damage within the window.</summary>
        public static bool IsInCombat(double now, double lastCombatTime) =>
            lastCombatTime > double.NegativeInfinity && now - lastCombatTime <= CombatWindowSeconds && now >= lastCombatTime;

        /// <summary>Tries a skill cast gives to Magia: its mana cost rounded (at least 1 for a skill that costs mana).</summary>
        public static long MagicTries(float manaSpent) => manaSpent <= 0f ? 0 : Math.Max(1L, (long)Math.Round(manaSpent));

        public static bool TryParse(string text, out ProficiencyKind kind) =>
            Enum.TryParse(text, out kind) && Enum.IsDefined(typeof(ProficiencyKind), kind);

        /// <summary>The modifiers the trained levels put on the hero (one per kind with levels), for the BuffController.</summary>
        public static List<StatModifierDefinition> Modifiers(IEnumerable<ProficiencyEntry> entries)
        {
            var list = new List<StatModifierDefinition>();
            if (entries == null)
            {
                return list;
            }

            foreach (ProficiencyEntry entry in entries)
            {
                if (entry == null || entry.level <= 0 || !TryParse(entry.kind, out ProficiencyKind kind))
                {
                    continue;
                }

                ProficiencySpec spec = GetSpec(kind);
                list.Add(new StatModifierDefinition
                {
                    displayName = "proficiency_" + kind.ToString().ToLowerInvariant(),
                    affectedStat = spec.Stat,
                    application = spec.Application,
                    value = Bonus(kind, entry.level),
                    durationSeconds = 0f,
                    isDebuff = false,
                });
            }

            return list;
        }

        /// <summary>Level 0..max, tries in range, one entry per known kind (unknown/duplicates dropped, missing ones added).</summary>
        public static void Sanitize(List<ProficiencyEntry> entries)
        {
            if (entries == null)
            {
                return;
            }

            entries.RemoveAll(e => e == null || !TryParse(e.kind, out _));
            var seen = new HashSet<string>();
            entries.RemoveAll(e => !seen.Add(e.kind));
            foreach (ProficiencyEntry entry in entries)
            {
                TryParse(entry.kind, out ProficiencyKind kind);
                entry.kind = kind.ToString();
                entry.level = Math.Max(0, Math.Min(MaxLevel, entry.level));
                long need = TriesToNext(kind, entry.level);
                entry.tries = need <= 0 ? 0 : Math.Max(0, Math.Min(entry.tries, need - 1));
            }

            foreach (ProficiencySpec spec in Specs)
            {
                if (!seen.Contains(spec.Kind.ToString()))
                {
                    entries.Add(new ProficiencyEntry { kind = spec.Kind.ToString() });
                }
            }

            entries.Sort((a, b) => ((int)Enum.Parse(typeof(ProficiencyKind), a.kind)).CompareTo((int)Enum.Parse(typeof(ProficiencyKind), b.kind)));
        }
    }
}
