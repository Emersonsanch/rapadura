using System;
using Rapadura.Core.Events;
using Rapadura.Gameplay.Items;

namespace Rapadura.Gameplay.Inventory
{
    /// <summary>
    /// "Morrer perde XP" (Docs/Referencias_MMO.md §6; Docs/Balance/README.md, "Perda de XP, Bênção da Romaria e Patuá").
    /// Pure, EditMode-tested.
    /// <list type="bullet">
    /// <item>A death costs <see cref="LossFraction"/> (5%) of the XP the current level needs (<c>xp_to_next</c>).</item>
    /// <item><b>Never de-levels</b> (kid-friendly): the loss only comes out of the XP already earned inside the current
    /// level, so the bar can drop to empty but the level number never goes down.</item>
    /// <item>No loss while the hero is below <see cref="GraceLevel"/> (the first fights / tutorial) nor in a region boss
    /// challenge of the first campaign (<c>NewGamePlus</c> cycle 0): bosses are the wall kids retry most.</item>
    /// <item>Bênção da Romaria cuts the loss by <see cref="BlessingReduction"/> (60%); the Patuá da Vó Edna cancels it.</item>
    /// </list>
    /// </summary>
    public static class DeathXpLossRules
    {
        /// <summary>Share of the current level's <c>xp_to_next</c> lost per death.</summary>
        public const float LossFraction = 0.05f;

        /// <summary>How much of the loss the Bênção da Romaria takes away.</summary>
        public const float BlessingReduction = 0.6f;

        /// <summary>No XP is lost below this level (first fights and the tutorial).</summary>
        public const int GraceLevel = 3;

        /// <summary>The loss before any protection, never more than the XP inside the current level.</summary>
        public static int BaseLoss(int level, int currentExperience, int experienceToNextLevel)
        {
            if (level < GraceLevel || currentExperience <= 0 || experienceToNextLevel <= 0)
            {
                return 0;
            }

            int raw = (int)Math.Round(experienceToNextLevel * (double)LossFraction, MidpointRounding.AwayFromZero);
            return Math.Min(Math.Max(0, raw), currentExperience);
        }

        /// <summary>
        /// XP lost on this death: <see cref="BaseLoss"/> after the exemptions and the protection. Clamped to
        /// <paramref name="currentExperience"/>, so the level never goes down.
        /// </summary>
        public static int Compute(int level, int currentExperience, int experienceToNextLevel, DeathProtection protection,
            bool bossChallenge = false, int newGamePlusCycle = 0)
        {
            if (protection == DeathProtection.Patua || IsExempt(bossChallenge, newGamePlusCycle))
            {
                return 0;
            }

            int loss = BaseLoss(level, currentExperience, experienceToNextLevel);
            if (protection == DeathProtection.Blessing)
            {
                loss = (int)Math.Round(loss * (1.0 - BlessingReduction), MidpointRounding.AwayFromZero);
            }

            return Math.Min(Math.Max(0, loss), Math.Max(0, currentExperience));
        }

        /// <summary>Boss challenges of the first campaign never cost XP (NG+ ones do).</summary>
        public static bool IsExempt(bool bossChallenge, int newGamePlusCycle) => bossChallenge && newGamePlusCycle <= 0;
    }

    /// <summary>What protected the hero on a death.</summary>
    public enum DeathProtection
    {
        None = 0,
        /// <summary>Bênção da Romaria: −60% XP loss, 0% item drop; ends on this death.</summary>
        Blessing = 1,
        /// <summary>Patuá da Vó Edna: no XP loss, no item drop; one is used up from the bag.</summary>
        Patua = 2,
    }

    /// <summary>
    /// Which protection a death uses. <b>The Patuá goes first</b> (it is the full protection: a child who carries one
    /// loses nothing, simple to explain) and the Bênção then stays for the next death, so neither is wasted.
    /// </summary>
    public static class DeathPenaltyRules
    {
        public static DeathProtection Resolve(bool hasPatua, bool blessed)
        {
            if (hasPatua)
            {
                return DeathProtection.Patua;
            }

            return blessed ? DeathProtection.Blessing : DeathProtection.None;
        }

        /// <summary>Any protection turns the 9% item drop into 0% (a dropped item is lost for good, so this matters).</summary>
        public static bool AllowsItemDrop(DeathProtection protection) => protection == DeathProtection.None;
    }

    /// <summary>The dying hero's numbers (read from <c>PlayerStats</c>) and where the death happened.</summary>
    public readonly struct DeathPenaltyContext
    {
        public readonly int Level;
        public readonly int CurrentExperience;
        public readonly int ExperienceToNextLevel;
        public readonly bool BossChallenge;
        public readonly int NewGamePlusCycle;

        public DeathPenaltyContext(int level, int currentExperience, int experienceToNextLevel, bool bossChallenge = false, int newGamePlusCycle = 0)
        {
            Level = level;
            CurrentExperience = currentExperience;
            ExperienceToNextLevel = experienceToNextLevel;
            BossChallenge = bossChallenge;
            NewGamePlusCycle = newGamePlusCycle;
        }
    }

    /// <summary>What <see cref="DeathPenaltyService.Apply"/> did (the caller removes <see cref="ExperienceLost"/>).</summary>
    public readonly struct DeathPenaltyResult
    {
        public readonly int ExperienceLost;
        public readonly DeathProtection Protection;
        public readonly DeathDropResult Drop;

        public DeathPenaltyResult(int experienceLost, DeathProtection protection, DeathDropResult drop)
        {
            ExperienceLost = experienceLost;
            Protection = protection;
            Drop = drop;
        }
    }

    /// <summary>Raised once per death that went through <see cref="DeathPenaltyService.Apply"/>.</summary>
    public readonly struct DeathPenaltyAppliedEvent : IGameEvent
    {
        public readonly int ExperienceLost;
        public readonly DeathProtection Protection;

        public DeathPenaltyAppliedEvent(int experienceLost, DeathProtection protection)
        {
            ExperienceLost = experienceLost;
            Protection = protection;
        }
    }

    /// <summary>
    /// Applies one death to a live bag and the protection state: picks the protection
    /// (<see cref="DeathPenaltyRules.Resolve"/>), uses it up (one Patuá out of the bag, or the Bênção ends), works out the
    /// XP loss and rolls the 9% drop only when unprotected (<see cref="DeathDropService"/>). The XP itself is removed by
    /// the caller (<c>PlayerStats.RemoveExperience</c>), so this stays testable without a player. Online (FASE 12) the
    /// server runs this same rule.
    /// </summary>
    public static class DeathPenaltyService
    {
        public static DeathPenaltyResult Apply(DeathPenaltyContext context, InventoryManager bag,
            DeathProtectionState protection, Random random, Func<string, ItemDefinition> resolve = null)
        {
            bool hasPatua = bag != null && bag.GetTotalCount(PatuaRules.ItemId) > 0;
            bool blessed = protection != null && protection.IsBlessed;
            DeathProtection used = DeathPenaltyRules.Resolve(hasPatua, blessed);

            if (used == DeathProtection.Patua && !bag.RemoveById(PatuaRules.ItemId, 1))
            {
                used = DeathPenaltyRules.Resolve(false, blessed);
            }

            if (used == DeathProtection.Blessing)
            {
                protection.ConsumeBlessing();
            }

            int lost = DeathXpLossRules.Compute(context.Level, context.CurrentExperience, context.ExperienceToNextLevel, used,
                context.BossChallenge, context.NewGamePlusCycle);

            DeathDropResult drop = DeathPenaltyRules.AllowsItemDrop(used)
                ? DeathDropService.Apply(bag, random, resolve)
                : DeathDropResult.None;

            EventBus.Publish(new DeathPenaltyAppliedEvent(lost, used));
            return new DeathPenaltyResult(lost, used, drop);
        }
    }
}
