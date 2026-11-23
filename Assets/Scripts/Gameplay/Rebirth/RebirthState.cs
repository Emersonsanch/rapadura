using System;
using System.Collections.Generic;
using Rapadura.Gameplay.World.Cities;

namespace Rapadura.Gameplay.Rebirth
{
    /// <summary>Why a "Volta da Luz" (reset) or a "Subida da Coluna" (grand reset) was refused. <see cref="None"/> = done.</summary>
    public enum RebirthRefusal
    {
        None = 0,
        InvalidPlayer = 1,
        LevelTooLow = 2,
        NotEnoughGold = 3,

        /// <summary>The 10 resets of this cycle are done: the next step is the grand reset.</summary>
        CycleFull = 4,

        /// <summary>Grand reset asked before the 10 resets of the cycle.</summary>
        CycleNotFull = 5,

        /// <summary>All 5 grand resets (and the 10 resets after the last one) are done.</summary>
        MaxReached = 6,

        /// <summary>Less than <see cref="RebirthRules.MinSecondsBetweenResets"/> (or the grand cooldown) since the last one.</summary>
        TooSoon = 7,

        /// <summary>Not in the safe zone of the right city (Vila da Vó Edna for a reset, Cais da Coluna for a grand reset).</summary>
        WrongPlace = 8,

        /// <summary>Outlaw stains: the light only goes back clean (pay the bail or let them fade).</summary>
        Stained = 9,

        /// <summary>In a trade, duel, arena match or arena queue.</summary>
        Busy = 10,

        Dead = 11,

        /// <summary>The client confirmed a summary for a different reset count (double click, replay, two tabs).</summary>
        StaleRequest = 12,
    }

    /// <summary>
    /// What a character keeps and what goes back to zero on a reset (one flag each, so the server, the summary window and
    /// the tests read the same list). <see cref="RebirthRules.Cleared"/> and <see cref="RebirthRules.Kept"/> split them.
    /// </summary>
    [Flags]
    public enum RebirthPart
    {
        None = 0,
        Level = 1 << 0,
        Experience = 1 << 1,

        /// <summary>Allocated attribute points (they come back with the levels, plus the reset bonus).</summary>
        AttributePoints = 1 << 2,

        /// <summary>Skill ranks and talent wheel: a free respec, the points come back with the levels.</summary>
        SkillAndTalentPoints = 1 << 3,

        /// <summary>Backpack, equipment, Baú and refines (equipped pieces stay equipped).</summary>
        Items = 1 << 4,

        /// <summary>Gold, minus the reset price.</summary>
        Gold = 1 << 5,

        /// <summary>Finished quests and the story (not offered again: no quest XP farming by resetting).</summary>
        Quests = 1 << 6,

        /// <summary>Caderno da Vó Edna (bestiary), Lembranças do Vô Jorge, secrets.</summary>
        Caderno = 1 << 7,

        Achievements = 1 << 8,
        Mounts = 1 << 9,
        House = 1 << 10,
        Guild = 1 << 11,

        /// <summary>Class graduations G1–G5 (titles and missions; G4/G5 effects wake up again at 150/350).</summary>
        Graduations = 1 << 12,

        /// <summary>Master level and its tree (active again once the hero is back at 500).</summary>
        MasterLevel = 1 << 13,

        /// <summary>Mandacaru/Melão growth fruits already eaten (they stay as points to spend).</summary>
        GrowthFruits = 1 << 14,

        /// <summary>Arena rating and PK stains (stains must be zero to reset anyway).</summary>
        PvpRecord = 1 << 15,
    }

    /// <summary>
    /// The reset record of one character, kept by the server (never by the client). <see cref="Resets"/> counts inside
    /// the current grand cycle (0–10); <see cref="GrandResets"/> counts grand resets (0–5). <see cref="EffectiveResets"/>
    /// (grand × 10 + resets, up to 60) drives the bonus points and the price.
    /// </summary>
    public sealed class RebirthState
    {
        public string PlayerId { get; }

        public int Resets { get; internal set; }

        public int GrandResets { get; internal set; }

        /// <summary>Level the hero had right before the last reset (0 = never reset). "Eco da Luz" runs below it.</summary>
        public int PeakLevel { get; internal set; }

        /// <summary>Server time of the last reset or grand reset (<see cref="double.NegativeInfinity"/> = never).</summary>
        public double LastResetAt { get; internal set; }

        public RebirthState(string playerId, int resets = 0, int grandResets = 0, int peakLevel = 0,
            double lastResetAt = double.NegativeInfinity)
        {
            PlayerId = playerId;
            GrandResets = Math.Max(0, Math.Min(RebirthRules.MaxGrandResets, grandResets));
            Resets = Math.Max(0, Math.Min(RebirthRules.ResetsPerCycle, resets));
            PeakLevel = Math.Max(0, peakLevel);
            LastResetAt = lastResetAt;
        }

        public int EffectiveResets => GrandResets * RebirthRules.ResetsPerCycle + Resets;

        /// <summary>Bonus attribute points earned by every reset so far (<see cref="RebirthRules.TotalBonusPoints"/>).</summary>
        public int TotalBonusPoints => RebirthRules.TotalBonusPoints(EffectiveResets);

        public bool HasEverReset => EffectiveResets > 0;
    }

    /// <summary>The character as the server sees it at the moment of the request (filled from its own records).</summary>
    public readonly struct RebirthCandidate
    {
        public readonly string PlayerId;
        public readonly int Level;
        public readonly long Gold;

        /// <summary>City whose safe zone the hero is in (<see cref="CityCatalog"/> id), null outside a city.</summary>
        public readonly string CityId;

        public readonly ZoneKind Zone;
        public readonly int PkStains;
        public readonly bool Alive;

        /// <summary>In a trade, duel, arena match or arena queue.</summary>
        public readonly bool Busy;

        /// <summary>The <see cref="RebirthState.EffectiveResets"/> the confirmed summary showed (−1 = skip the check, tests/tools).</summary>
        public readonly int ExpectedEffectiveResets;

        public RebirthCandidate(string playerId, int level, long gold, string cityId, ZoneKind zone = ZoneKind.City,
            int pkStains = 0, bool alive = true, bool busy = false, int expectedEffectiveResets = -1)
        {
            PlayerId = playerId;
            Level = level;
            Gold = gold;
            CityId = cityId;
            Zone = zone;
            PkStains = pkStains;
            Alive = alive;
            Busy = busy;
            ExpectedEffectiveResets = expectedEffectiveResets;
        }
    }

    /// <summary>What the next reset of a character asks for (the NPC window and the server read the same numbers).</summary>
    public readonly struct RebirthRequirement
    {
        public readonly bool IsGrand;

        /// <summary>False when nothing more is possible (<see cref="RebirthRefusal.MaxReached"/>).</summary>
        public readonly bool Available;

        public readonly int MinLevel;
        public readonly long GoldCost;
        public readonly string CityId;

        /// <summary>Server time from which the cooldown allows it.</summary>
        public readonly double ReadyAt;

        /// <summary>Bonus attribute points this step adds (0 on a grand reset).</summary>
        public readonly int BonusPoints;

        public RebirthRequirement(bool isGrand, bool available, int minLevel, long goldCost, string cityId, double readyAt, int bonusPoints)
        {
            IsGrand = isGrand;
            Available = available;
            MinLevel = minLevel;
            GoldCost = goldCost;
            CityId = cityId;
            ReadyAt = readyAt;
            BonusPoints = bonusPoints;
        }
    }

    /// <summary>A title + cosmetic given at a reset count. Titles and cosmetics only: never an attribute, never sold.</summary>
    public readonly struct RebirthMilestone
    {
        /// <summary>Effective resets (normal milestone) or grand resets (<see cref="IsGrand"/>) that unlock it.</summary>
        public readonly int Count;

        public readonly bool IsGrand;
        public readonly string TitleId;
        public readonly string CosmeticId;

        public RebirthMilestone(int count, bool isGrand, string titleId, string cosmeticId)
        {
            Count = count;
            IsGrand = isGrand;
            TitleId = titleId;
            CosmeticId = cosmeticId;
        }
    }

    /// <summary>Result of <see cref="RebirthRules.TryReset"/> / <see cref="RebirthRules.TryGrandReset"/>. Nothing throws.</summary>
    public readonly struct RebirthResult
    {
        public readonly RebirthRefusal Refusal;
        public readonly bool IsGrand;

        /// <summary>Gold the server takes (in the same transaction as the level change).</summary>
        public readonly long GoldCost;

        /// <summary>Level the hero had (becomes <see cref="RebirthState.PeakLevel"/>); the new level is always 1.</summary>
        public readonly int PreviousLevel;

        public readonly int BonusPointsGained;
        public readonly int TotalBonusPoints;

        /// <summary>Titles/cosmetics reached by this step (usually empty).</summary>
        public readonly IReadOnlyList<RebirthMilestone> NewMilestones;

        public RebirthResult(RebirthRefusal refusal, bool isGrand = false, long goldCost = 0, int previousLevel = 0,
            int bonusPointsGained = 0, int totalBonusPoints = 0, IReadOnlyList<RebirthMilestone> newMilestones = null)
        {
            Refusal = refusal;
            IsGrand = isGrand;
            GoldCost = goldCost;
            PreviousLevel = previousLevel;
            BonusPointsGained = bonusPointsGained;
            TotalBonusPoints = totalBonusPoints;
            NewMilestones = newMilestones ?? Array.Empty<RebirthMilestone>();
        }

        public bool Ok => Refusal == RebirthRefusal.None;

        public static RebirthResult Refused(RebirthRefusal reason, bool isGrand = false) => new RebirthResult(reason, isGrand);
    }
}
