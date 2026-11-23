using System;
using System.Collections.Generic;
using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.World.Cities;

namespace Rapadura.Gameplay.Rebirth
{
    /// <summary>
    /// The MU-style reset of FASE 12.3 / 12.4 ("New Game+ → reset por personagem no servidor"), as pure rules the server runs
    /// with its own clock and records. Told as the <b>"Volta da Luz"</b>: at the Cristal of the Vila da Vó Edna the hero hands
    /// the light gathered on the road back to the Barca da Lua (bible §1.5: the Moon gathers the freed light and grows) and
    /// starts again at level 1, lighter, keeping a little of it as bonus points. After 10 resets the <b>"Subida da Coluna"</b>
    /// (grand reset) at the Cais da Coluna climbs one step of the Coluna da Glória and opens the next 10.
    ///
    /// <para><b>Numbers.</b> Reset n of a cycle (1–10) asks level <c>min(500, 400 + 10·(n−1))</c> (400, 410 … 490) and the
    /// e-th reset overall (1–60) costs <c>min(100 000, 20 000 + 2 000·(e−1))</c> gold (≈ 20–80 h of hunting at the flat
    /// ~1 000 gold/h of Balanco100a500 §5.1). The grand reset asks the 10 resets of the cycle, level 500 and
    /// <c>200 000 + 50 000·g</c> gold (g = grand resets done). At most <see cref="MaxGrandResets"/> grand resets and 10 resets
    /// after the last: 60 effective. At least <see cref="MinSecondsBetweenResets"/> (24 h) between resets and
    /// <see cref="MinSecondsBeforeGrandReset"/> (7 days) from the last reset to a grand reset.</para>
    ///
    /// <para><b>Bonus points</b> (decreasing): resets 1–5 give 20 each, 6–10 give 10, 11–20 give 5, 21–40 give 3, 41–60 give
    /// 2 — 300 in total, against the 998 a level-500 hero earns by levelling (Progressao500 §2). They wake up with the levels
    /// (<see cref="UnlockedBonusPoints"/>: in proportion up to level 400), so a fresh reset hero at level 1 is a level-1 hero,
    /// and at every level the edge stays inside the band checked by <see cref="RebirthBalance"/> (≤ +30% power, ~+25% at the cap).</para>
    ///
    /// <para><b>Eco da Luz:</b> below <see cref="RebirthState.PeakLevel"/> kill XP is ×<see cref="EchoXpMultiplier"/> (4), so a
    /// cycle back to 400 takes ~120 h of hunting instead of ~460 h. Kill XP only (quests are kept finished and not offered
    /// again) and it stops at the old level, so it never speeds anyone past where they were. Party XP still applies its
    /// 20-level band (<c>PartyXpRules</c>), so a reset hero cannot be carried by a level-400 party either.</para>
    ///
    /// <para><b>NG+ and reset.</b> The single-player Nova Jornada+ (<c>NewGamePlusRules</c>) resets the <i>world</i> of a
    /// save (story beats, bosses, region quests) and scales enemies by the save's cycle; in a shared online world neither is
    /// possible per player. Online characters therefore use this per-character reset and no NG+
    /// (<see cref="UsesNewGamePlus"/>); offline saves keep NG+ exactly as it is (it is not touched).</para>
    /// </summary>
    public static class RebirthRules
    {
        public const int ResetsPerCycle = 10;
        public const int MaxGrandResets = 5;
        public const int MaxEffectiveResets = MaxGrandResets * ResetsPerCycle + ResetsPerCycle;

        public const int BaseMinLevel = 400;
        public const int MinLevelStepPerReset = 10;
        public const int GrandResetMinLevel = LevelCurveMath.MaxLevel;

        public const long BaseGoldCost = 20000;
        public const long GoldCostStep = 2000;
        public const long MaxGoldCost = 100000;
        public const long GrandBaseGoldCost = 200000;
        public const long GrandGoldCostStep = 50000;

        public const double MinSecondsBetweenResets = 24d * 3600d;
        public const double MinSecondsBeforeGrandReset = 7d * 24d * 3600d;

        /// <summary>Bonus points are fully awake at this level (the first reset level).</summary>
        public const int BonusFullAtLevel = BaseMinLevel;

        /// <summary>Kill XP multiplier while below the level of the last reset ("Eco da Luz").</summary>
        public const float EchoXpMultiplier = 4f;

        /// <summary>Where the "Volta da Luz" happens: the Cristal at the Vila da Vó Edna (id kept from the old name).</summary>
        public const string ResetCityId = CityCatalog.VilaZefa;

        /// <summary>Where the "Subida da Coluna" happens: the Cais da Coluna (Reinos do Céu).</summary>
        public const string GrandResetCityId = CityCatalog.CaisDaColuna;

        /// <summary>What a reset sends back to zero.</summary>
        public const RebirthPart Cleared = RebirthPart.Level | RebirthPart.Experience | RebirthPart.AttributePoints | RebirthPart.SkillAndTalentPoints;

        /// <summary>What a reset keeps (gold minus the price).</summary>
        public const RebirthPart Kept = RebirthPart.Items | RebirthPart.Gold | RebirthPart.Quests | RebirthPart.Caderno | RebirthPart.Achievements
            | RebirthPart.Mounts | RebirthPart.House | RebirthPart.Guild | RebirthPart.Graduations | RebirthPart.MasterLevel
            | RebirthPart.GrowthFruits | RebirthPart.PvpRecord;

        /// <summary>Online characters reset; only offline saves play Nova Jornada+.</summary>
        public static bool UsesNewGamePlus(bool onlineCharacter) => !onlineCharacter;

        // ------------------------------------------------------------------ numbers

        /// <summary>Level asked by reset <paramref name="resetInCycle"/> (1–10) of a cycle.</summary>
        public static int MinLevelFor(int resetInCycle)
        {
            int n = Math.Max(1, Math.Min(ResetsPerCycle, resetInCycle));
            return Math.Min(LevelCurveMath.MaxLevel, BaseMinLevel + MinLevelStepPerReset * (n - 1));
        }

        /// <summary>Gold asked by the <paramref name="effectiveReset"/>-th reset overall (1–60).</summary>
        public static long GoldCostFor(int effectiveReset)
        {
            int e = Math.Max(1, effectiveReset);
            return Math.Min(MaxGoldCost, BaseGoldCost + GoldCostStep * (e - 1));
        }

        /// <summary>Gold asked by the grand reset that follows <paramref name="grandResetsDone"/> grand resets.</summary>
        public static long GrandGoldCostFor(int grandResetsDone) => GrandBaseGoldCost + GrandGoldCostStep * Math.Max(0, grandResetsDone);

        /// <summary>Bonus attribute points given by the <paramref name="effectiveReset"/>-th reset (1–60); 0 outside.</summary>
        public static int BonusPointsFor(int effectiveReset)
        {
            if (effectiveReset < 1 || effectiveReset > MaxEffectiveResets)
            {
                return 0;
            }

            if (effectiveReset <= 5) return 20;
            if (effectiveReset <= 10) return 10;
            if (effectiveReset <= 20) return 5;
            if (effectiveReset <= 40) return 3;
            return 2;
        }

        /// <summary>Sum of <see cref="BonusPointsFor"/> over the first <paramref name="effectiveResets"/> resets (300 at 60).</summary>
        public static int TotalBonusPoints(int effectiveResets)
        {
            int total = 0;
            int last = Math.Min(MaxEffectiveResets, effectiveResets);
            for (int e = 1; e <= last; e++)
            {
                total += BonusPointsFor(e);
            }

            return total;
        }

        /// <summary>Bonus points usable at <paramref name="level"/>: <c>⌊total × min(level, 400) / 400⌋</c>.</summary>
        public static int UnlockedBonusPoints(int totalBonusPoints, int level)
        {
            if (totalBonusPoints <= 0)
            {
                return 0;
            }

            int l = Math.Max(0, Math.Min(BonusFullAtLevel, level));
            return (int)((long)totalBonusPoints * l / BonusFullAtLevel);
        }

        /// <summary>Points the server grants on a level-up from <paramref name="fromLevel"/> to <paramref name="toLevel"/> (never negative).</summary>
        public static int BonusPointsToGrant(int totalBonusPoints, int fromLevel, int toLevel)
        {
            return Math.Max(0, UnlockedBonusPoints(totalBonusPoints, toLevel) - UnlockedBonusPoints(totalBonusPoints, fromLevel));
        }

        public static bool IsEchoActive(RebirthState state, int level) => state != null && state.HasEverReset && level < state.PeakLevel;

        /// <summary>Kill XP after the "Eco da Luz" (unchanged when it is off; rounded, never below the base).</summary>
        public static long ScaleKillExperience(long baseExperience, RebirthState state, int level)
        {
            if (baseExperience <= 0 || !IsEchoActive(state, level))
            {
                return baseExperience;
            }

            return Math.Max(baseExperience, (long)Math.Round(baseExperience * (double)EchoXpMultiplier, MidpointRounding.AwayFromZero));
        }

        // ------------------------------------------------------------------ milestones

        /// <summary>Titles and cosmetics by count. Normal ones by effective resets, grand ones by grand resets.</summary>
        public static readonly IReadOnlyList<RebirthMilestone> Milestones = new[]
        {
            new RebirthMilestone(1, false, "title_rebirth_returned_spark", "cosmetic_rebirth_spark_thread"),
            new RebirthMilestone(3, false, "title_rebirth_lit_lamp", "cosmetic_rebirth_floating_lamp"),
            new RebirthMilestone(5, false, "title_rebirth_waxing_moon", "cosmetic_rebirth_moon_trail"),
            new RebirthMilestone(10, false, "title_rebirth_full_moon", "cosmetic_rebirth_full_moon_cloak"),
            new RebirthMilestone(25, false, "title_rebirth_sun_boat", "cosmetic_rebirth_sun_boat_wake"),
            new RebirthMilestone(40, false, "title_rebirth_twelve_jars", "cosmetic_rebirth_jar_wheel"),
            new RebirthMilestone(60, false, "title_rebirth_way_home", "cosmetic_rebirth_way_home_glow"),
            new RebirthMilestone(1, true, "title_rebirth_column_step_1", "cosmetic_rebirth_column_step_1"),
            new RebirthMilestone(2, true, "title_rebirth_column_step_2", "cosmetic_rebirth_column_step_2"),
            new RebirthMilestone(3, true, "title_rebirth_column_step_3", "cosmetic_rebirth_column_step_3"),
            new RebirthMilestone(4, true, "title_rebirth_column_step_4", "cosmetic_rebirth_column_step_4"),
            new RebirthMilestone(5, true, "title_rebirth_column_step_5", "cosmetic_rebirth_column_step_5"),
        };

        /// <summary>Milestones crossed going from (<paramref name="effectiveBefore"/>, <paramref name="grandBefore"/>) to the after values.</summary>
        public static List<RebirthMilestone> MilestonesBetween(int effectiveBefore, int grandBefore, int effectiveAfter, int grandAfter)
        {
            var list = new List<RebirthMilestone>();
            foreach (RebirthMilestone m in Milestones)
            {
                int before = m.IsGrand ? grandBefore : effectiveBefore;
                int after = m.IsGrand ? grandAfter : effectiveAfter;
                if (before < m.Count && after >= m.Count)
                {
                    list.Add(m);
                }
            }

            return list;
        }

        /// <summary>Every milestone a character with this record owns (to rebuild the wardrobe from the record alone).</summary>
        public static List<RebirthMilestone> MilestonesOwned(RebirthState state) =>
            state == null ? new List<RebirthMilestone>() : MilestonesBetween(0, 0, state.EffectiveResets, state.GrandResets);

        // ------------------------------------------------------------------ requirement and checks

        /// <summary>What the next step asks: a reset while the cycle is not full, else the grand reset.</summary>
        public static RebirthRequirement NextRequirement(RebirthState state)
        {
            int resets = state?.Resets ?? 0;
            int grand = state?.GrandResets ?? 0;
            double last = state?.LastResetAt ?? double.NegativeInfinity;
            if (resets < ResetsPerCycle)
            {
                int effective = grand * ResetsPerCycle + resets + 1;
                return new RebirthRequirement(false, true, MinLevelFor(resets + 1), GoldCostFor(effective), ResetCityId,
                    last + MinSecondsBetweenResets, BonusPointsFor(effective));
            }

            return new RebirthRequirement(true, grand < MaxGrandResets, GrandResetMinLevel, GrandGoldCostFor(grand), GrandResetCityId,
                last + MinSecondsBeforeGrandReset, 0);
        }

        /// <summary>Every server check of a reset (or grand reset), in a fixed order; <see cref="RebirthRefusal.None"/> = allowed.</summary>
        public static RebirthRefusal Check(RebirthState state, RebirthCandidate who, double now, bool grand)
        {
            if (state == null || string.IsNullOrEmpty(who.PlayerId) || who.PlayerId != state.PlayerId)
            {
                return RebirthRefusal.InvalidPlayer;
            }

            if (who.ExpectedEffectiveResets >= 0 && who.ExpectedEffectiveResets != state.EffectiveResets)
            {
                return RebirthRefusal.StaleRequest;
            }

            if (grand)
            {
                if (state.GrandResets >= MaxGrandResets)
                {
                    return RebirthRefusal.MaxReached;
                }

                if (state.Resets < ResetsPerCycle)
                {
                    return RebirthRefusal.CycleNotFull;
                }
            }
            else if (state.Resets >= ResetsPerCycle)
            {
                return state.GrandResets >= MaxGrandResets ? RebirthRefusal.MaxReached : RebirthRefusal.CycleFull;
            }

            if (!who.Alive)
            {
                return RebirthRefusal.Dead;
            }

            if (who.Busy)
            {
                return RebirthRefusal.Busy;
            }

            RebirthRequirement req = NextRequirement(state);
            if (who.Zone != ZoneKind.City || who.CityId != req.CityId)
            {
                return RebirthRefusal.WrongPlace;
            }

            if (who.PkStains > 0)
            {
                return RebirthRefusal.Stained;
            }

            if (who.Level < req.MinLevel)
            {
                return RebirthRefusal.LevelTooLow;
            }

            if (who.Gold < req.GoldCost)
            {
                return RebirthRefusal.NotEnoughGold;
            }

            if (now < req.ReadyAt)
            {
                return RebirthRefusal.TooSoon;
            }

            return RebirthRefusal.None;
        }

        /// <summary>
        /// "Volta da Luz": on success the record advances (resets +1, peak = current level, time = now). The server then, in one
        /// transaction: takes <see cref="RebirthResult.GoldCost"/>, sets level 1 and XP 0, clears the <see cref="Cleared"/> parts
        /// and grants the milestones. Nothing changes on a refusal.
        /// </summary>
        public static RebirthResult TryReset(RebirthState state, RebirthCandidate who, double now)
        {
            RebirthRefusal refusal = Check(state, who, now, false);
            if (refusal != RebirthRefusal.None)
            {
                return RebirthResult.Refused(refusal);
            }

            RebirthRequirement req = NextRequirement(state);
            int effectiveBefore = state.EffectiveResets;
            state.Resets++;
            Advance(state, who.Level, now);
            return new RebirthResult(RebirthRefusal.None, false, req.GoldCost, who.Level, req.BonusPoints, state.TotalBonusPoints,
                MilestonesBetween(effectiveBefore, state.GrandResets, state.EffectiveResets, state.GrandResets));
        }

        /// <summary>
        /// "Subida da Coluna": the cycle's 10 resets become one grand reset (resets 0, grand +1; effective count and bonus
        /// points unchanged), level back to 1 like a reset, and the next cycle starts again at level 400.
        /// </summary>
        public static RebirthResult TryGrandReset(RebirthState state, RebirthCandidate who, double now)
        {
            RebirthRefusal refusal = Check(state, who, now, true);
            if (refusal != RebirthRefusal.None)
            {
                return RebirthResult.Refused(refusal, true);
            }

            RebirthRequirement req = NextRequirement(state);
            int effectiveBefore = state.EffectiveResets;
            int grandBefore = state.GrandResets;
            state.GrandResets++;
            state.Resets = 0;
            Advance(state, who.Level, now);
            return new RebirthResult(RebirthRefusal.None, true, req.GoldCost, who.Level, 0, state.TotalBonusPoints,
                MilestonesBetween(effectiveBefore, grandBefore, state.EffectiveResets, state.GrandResets));
        }

        private static void Advance(RebirthState state, int level, double now)
        {
            state.PeakLevel = Math.Max(state.PeakLevel, level);
            state.LastResetAt = now;
        }
    }
}
