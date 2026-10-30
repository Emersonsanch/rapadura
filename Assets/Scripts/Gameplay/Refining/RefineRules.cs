using System;
using Rapadura.Gameplay.Items;

namespace Rapadura.Gameplay.Refining
{
    /// <summary>The "Pedras do Sertão" (MU Online's jewels, our names and theme).</summary>
    public enum RefineStone
    {
        None = 0,
        /// <summary>Pedra da Bênção: +0 → +6, always succeeds.</summary>
        Blessing = 1,
        /// <summary>Pedra da Alma: +6 → +9 with a chance; +9 → +15 with gold too.</summary>
        Soul = 2,
        /// <summary>Pedra da Vida: one more level of the item's extra option line.</summary>
        Life = 3,
        /// <summary>Pedra do Caos: Tacho Encantado (combinations, later). Defined now, not used by refining.</summary>
        Chaos = 4,
    }

    /// <summary>Why a refine (or an option) can or cannot be tried right now.</summary>
    public enum RefineCheck
    {
        Ok = 0,
        NotRefinable = 1,
        MaxLevel = 2,
        MissingStone = 3,
        NotEnoughGold = 4,
        MissingCharm = 5,
        MaxOption = 6,
    }

    /// <summary>The outcome of one roll (pure data: the server sends it back to the client online).</summary>
    public readonly struct RefineAttempt
    {
        public readonly int FromLevel;
        public readonly int ToLevel;
        public readonly RefineStone Stone;
        public readonly float Chance;
        public readonly int Gold;
        public readonly bool UsedCharm;
        public readonly double Roll;
        public readonly bool Success;

        public RefineAttempt(int fromLevel, int toLevel, RefineStone stone, float chance, int gold, bool usedCharm, double roll, bool success)
        {
            FromLevel = fromLevel;
            ToLevel = toLevel;
            Stone = stone;
            Chance = chance;
            Gold = gold;
            UsedCharm = usedCharm;
            Roll = roll;
            Success = success;
        }

        /// <summary>Failed and went back one level (never below +6, never destroyed).</summary>
        public bool Dropped => !Success && ToLevel < FromLevel;
    }

    /// <summary>
    /// Pure rules of item refining +0 → +15 (FASE 16, Docs/Balance/README.md "Refino"), unit-tested in
    /// <c>RefineRulesTests</c>. No UnityEngine, no state: the client shows the preview with them, and online the
    /// server runs the very same <see cref="Attempt"/> with its own seed (the client never rolls).
    /// <list type="bullet">
    /// <item>+0 → +6: one <see cref="RefineStone.Blessing"/>, always succeeds (the luck charm is not used).</item>
    /// <item>+6 → +9: one <see cref="RefineStone.Soul"/>, 80% / 75% / 70%.</item>
    /// <item>+9 → +15: one <see cref="RefineStone.Soul"/> + 10..50 gold, 60% falling to 35%.</item>
    /// <item>Failure (user decision 2026-10-29): the item goes back ONE level, never below +6 (the always-safe band),
    /// and never disappears.</item>
    /// <item>Amuleto da Sorte: +10 points of chance on a roll that can fail (capped at 95%), spent on that roll.</item>
    /// <item>Each level raises the item's base stat (weapon damage / armour defense) by <see cref="StatBonusFraction"/>.</item>
    /// <item>Pedra da Vida: the extra option line goes up one level (max 4) at 60%; failing changes nothing.</item>
    /// </list>
    /// </summary>
    public static class RefineRules
    {
        public const int MaxLevel = 15;
        /// <summary>Highest level reached with the Pedra da Bênção (always succeeds up to here).</summary>
        public const int BlessingCap = 6;
        /// <summary>Highest level reached without gold (Pedra da Alma alone).</summary>
        public const int SoulCap = 9;
        public const int MaxOptionLevel = 4;

        /// <summary>Amuleto da Sorte: added to the success chance of a roll that can fail.</summary>
        public const float LuckCharmBonus = 0.10f;
        /// <summary>A piece "com Sorte" (ItemOptionRules): added to the chance of its rolls that can fail (stacks with the charm, cap 95%).</summary>
        public const float LuckBonus = 0.05f;
        /// <summary>A roll that can fail never becomes certain, not even with the charm.</summary>
        public const float MaxChance = 0.95f;
        /// <summary>Pedra da Vida's chance of adding one level to the option line.</summary>
        public const float LifeStoneChance = 0.60f;

        /// <summary>
        /// How much of a weapon's refine bonus reaches the hero's AttackDamage (PercentAdditive): a hit is the hero's
        /// base damage × level × attributes, the weapon is only a part of it (Docs/Balance/README.md "Refino de item", power_stack.py).
        /// </summary>
        public const float WeaponAttackShare = 0.10f;
        /// <summary>Weapon option line: AttackDamage PercentAdditive per option level.</summary>
        public const float WeaponOptionAttackPercent = 0.01f;
        /// <summary>Armour option line: MaxHealth PercentAdditive per option level.</summary>
        public const float ArmourOptionHealthPercent = 0.005f;

        // Success chance of going from level i to i + 1 (index = current level).
        private static readonly float[] Chances =
        {
            1f, 1f, 1f, 1f, 1f, 1f,       // +0..+5 → +6: Bênção
            0.80f, 0.75f, 0.70f,          // +6..+8 → +9: Alma
            0.60f, 0.55f, 0.50f, 0.45f, 0.40f, 0.35f, // +9..+14 → +15: Alma + ouro
        };

        // Gold paid with the stone from level i (index = current level).
        private static readonly int[] GoldCosts =
        {
            0, 0, 0, 0, 0, 0,
            0, 0, 0,
            10, 15, 20, 30, 40, 50,
        };

        // Cumulative bonus on the item's base stat at each level 0..15 (+15 doubles it).
        private static readonly float[] StatBonuses =
        {
            0f, 0.04f, 0.08f, 0.12f, 0.16f, 0.20f, 0.24f,
            0.30f, 0.36f, 0.42f,
            0.50f, 0.58f, 0.67f, 0.77f, 0.88f, 1.00f,
        };

        /// <summary>Levels where the glow steps up (tier 1..5).</summary>
        public static readonly int[] GlowLevels = { 7, 9, 11, 13, 15 };

        public static int ClampLevel(int level) => Math.Max(0, Math.Min(MaxLevel, level));

        public static int ClampOption(int option) => Math.Max(0, Math.Min(MaxOptionLevel, option));

        /// <summary>Only equipment with a base stat to raise: weapons with damage, armour with defense (tools are not).</summary>
        public static bool IsRefinable(ItemDefinition item) =>
            item != null && item.IsEquippable && !item.IsStackable && (item.WeaponDamage > 0f || item.ArmorDefense > 0f);

        /// <summary>The stone that refines an item at <paramref name="level"/> (None at +15).</summary>
        public static RefineStone StoneFor(int level)
        {
            level = ClampLevel(level);
            if (level >= MaxLevel)
            {
                return RefineStone.None;
            }

            return level < BlessingCap ? RefineStone.Blessing : RefineStone.Soul;
        }

        /// <summary>Chance of <paramref name="level"/> → +1 without the charm (0 at +15).</summary>
        public static float BaseChance(int level)
        {
            level = ClampLevel(level);
            return level >= MaxLevel ? 0f : Chances[level];
        }

        /// <summary>True when the roll from <paramref name="level"/> can fail (the charm is only worth it there).</summary>
        public static bool CanFail(int level) => BaseChance(level) < 1f && ClampLevel(level) < MaxLevel;

        /// <summary>Chance of <paramref name="level"/> → +1, with the Amuleto da Sorte when <paramref name="withCharm"/>.</summary>
        public static float ChanceFor(int level, bool withCharm, bool luck = false)
        {
            float chance = BaseChance(level);
            if (!CanFail(level))
            {
                return chance;
            }

            if (withCharm)
            {
                chance += LuckCharmBonus;
            }

            if (luck)
            {
                chance += LuckBonus;
            }

            return withCharm || luck ? Math.Min(MaxChance, chance) : chance;
        }

        /// <summary>Gold paid with the stone to refine from <paramref name="level"/>.</summary>
        public static int GoldCostFor(int level)
        {
            level = ClampLevel(level);
            return level >= MaxLevel ? 0 : GoldCosts[level];
        }

        /// <summary>What a failed roll from <paramref name="level"/> leaves: one level less above +6 (decided), else the same.</summary>
        public static int FailureLevel(int level)
        {
            level = ClampLevel(level);
            return level > BlessingCap ? level - 1 : level;
        }

        /// <summary>Cumulative bonus on the base stat at <paramref name="level"/> (0.42 = +42% at +9).</summary>
        public static float StatBonusFraction(int level) => StatBonuses[ClampLevel(level)];

        /// <summary>The base stat at <paramref name="level"/>: <c>base × (1 + bonus)</c>, rounded to 0.1 like the tooltip (+0 = the base, untouched).</summary>
        public static float RefinedStat(float baseValue, int level)
        {
            if (baseValue <= 0f || ClampLevel(level) == 0)
            {
                return baseValue;
            }

            return (float)Math.Round(baseValue * (1f + StatBonusFraction(level)), 1, MidpointRounding.AwayFromZero);
        }

        /// <summary>Glow tier 0..5 (0 below +7; +7, +9, +11, +13, +15 step it up).</summary>
        public static int GlowTier(int level)
        {
            level = ClampLevel(level);
            int tier = 0;
            foreach (int threshold in GlowLevels)
            {
                if (level >= threshold)
                {
                    tier++;
                }
            }

            return tier;
        }

        /// <summary>" +7" appended to the item name (empty at +0).</summary>
        public static string NameSuffix(int level)
        {
            level = ClampLevel(level);
            return level > 0 ? " +" + level : string.Empty;
        }

        // -------------------------------------------------------------- combat bonuses (InventoryManager)

        /// <summary>A weapon's refine + option bonus on the hero's AttackDamage (PercentAdditive).</summary>
        public static float WeaponAttackPercent(int level, int option) =>
            StatBonusFraction(level) * WeaponAttackShare + ClampOption(option) * WeaponOptionAttackPercent;

        /// <summary>An armour piece's refine bonus on the hero's Defense (Flat): the refined part of its armour defense.</summary>
        public static float ArmourDefenseBonus(float armorDefense, int level) =>
            armorDefense > 0f ? RefinedStat(armorDefense, level) - armorDefense : 0f;

        /// <summary>An armour piece's option bonus on the hero's MaxHealth (PercentAdditive).</summary>
        public static float ArmourHealthPercent(int option) => ClampOption(option) * ArmourOptionHealthPercent;

        // -------------------------------------------------------------- checks

        /// <summary>Whether <paramref name="level"/> → +1 can be tried with what the bag holds.</summary>
        public static RefineCheck Check(bool refinable, int level, int stonesOwned, int goldOwned, bool useCharm, int charmsOwned)
        {
            if (!refinable)
            {
                return RefineCheck.NotRefinable;
            }

            if (ClampLevel(level) >= MaxLevel)
            {
                return RefineCheck.MaxLevel;
            }

            if (stonesOwned < 1)
            {
                return RefineCheck.MissingStone;
            }

            if (goldOwned < GoldCostFor(level))
            {
                return RefineCheck.NotEnoughGold;
            }

            if (useCharm && CanFail(level) && charmsOwned < 1)
            {
                return RefineCheck.MissingCharm;
            }

            return RefineCheck.Ok;
        }

        /// <summary>Whether a Pedra da Vida can be tried on an item with <paramref name="option"/>.</summary>
        public static RefineCheck CheckOption(bool refinable, int option, int lifeStonesOwned)
        {
            if (!refinable)
            {
                return RefineCheck.NotRefinable;
            }

            if (ClampOption(option) >= MaxOptionLevel)
            {
                return RefineCheck.MaxOption;
            }

            return lifeStonesOwned < 1 ? RefineCheck.MissingStone : RefineCheck.Ok;
        }

        // -------------------------------------------------------------- rolls (seeded: the server's)

        /// <summary>
        /// A uniform [0, 1) number from <paramref name="seed"/> (SplitMix64 finaliser): the same on every platform and
        /// runtime, so the server and a replay agree. Not <c>System.Random</c>, whose algorithm is not a contract.
        /// </summary>
        public static double Roll(int seed)
        {
            unchecked
            {
                ulong z = (ulong)(uint)seed + 0x9E3779B97F4A7C15UL;
                z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9UL;
                z = (z ^ (z >> 27)) * 0x94D049BB133111EBUL;
                z ^= z >> 31;
                return (z >> 11) * (1.0 / (1UL << 53));
            }
        }

        /// <summary>
        /// Resolves a roll already drawn: success when <c>roll &lt; chance</c>. At the cap there is nothing to attempt
        /// (<see cref="Check"/> answers <see cref="RefineCheck.MaxLevel"/>), so a stray call is a no-op, never a drop.
        /// </summary>
        public static RefineAttempt Resolve(int level, bool withCharm, double roll, bool luck = false)
        {
            level = ClampLevel(level);
            float chance = ChanceFor(level, withCharm, luck);
            bool atCap = level >= MaxLevel;
            bool success = !atCap && roll < chance;
            int to = success ? level + 1 : atCap ? level : FailureLevel(level);
            return new RefineAttempt(level, to, StoneFor(level), chance, GoldCostFor(level), withCharm && CanFail(level), roll, success);
        }

        /// <summary>One refine from <paramref name="level"/> with <paramref name="seed"/> (online: the server's seed).</summary>
        public static RefineAttempt Attempt(int level, bool withCharm, int seed, bool luck = false) => Resolve(level, withCharm, Roll(seed), luck);

        /// <summary>One Pedra da Vida on <paramref name="option"/>: the new option level (unchanged on failure).</summary>
        public static int ResolveOption(int option, double roll)
        {
            option = ClampOption(option);
            return option < MaxOptionLevel && roll < LifeStoneChance ? option + 1 : option;
        }

        /// <summary>
        /// Expected stones (and gold) to take an item from +0 to <paramref name="target"/> with the failure rule above,
        /// for the balance doc: a Markov walk over the levels (a failure costs the level back).
        /// </summary>
        public static (double stones, double gold) ExpectedCostTo(int target, bool withCharm = false)
        {
            target = ClampLevel(target);
            // e[i] = expected attempts to go from level i to i + 1, counting the re-climb after a drop.
            double stones = 0, gold = 0;
            var stepStones = new double[MaxLevel];
            var stepGold = new double[MaxLevel];
            for (int i = 0; i < target; i++)
            {
                double p = ChanceFor(i, withCharm);
                bool drops = FailureLevel(i) < i;
                double reclimbStones = drops ? stepStones[i - 1] : 0;
                double reclimbGold = drops ? stepGold[i - 1] : 0;
                // E = 1 + (1 - p) × (reclimb + E)  →  E = (1 + (1 - p) × reclimb) / p
                stepStones[i] = (1 + (1 - p) * reclimbStones) / p;
                stepGold[i] = (GoldCostFor(i) + (1 - p) * reclimbGold) / p;
                stones += stepStones[i];
                gold += stepGold[i];
            }

            return (stones, gold);
        }
    }
}
