using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.PvP;
using Rapadura.Gameplay.Rebirth;
using Rapadura.Gameplay.World.Cities;

namespace Rapadura.Tests
{
    /// <summary>
    /// The pure MU-style reset of TODO.md 12.3 / 12.4 ("Volta da Luz" and "Subida da Coluna"): requirements (level, gold,
    /// place, cooldown, stains, busy, stale summary), the cycle of 10 and the grand reset, the decreasing bonus points that
    /// wake up with the levels, the "Eco da Luz" catch-up XP, milestones (titles and cosmetics only), the ranking order with
    /// resets, the texts, and the balance band of a hero with R resets at level L against one with none.
    /// </summary>
    public class RebirthRulesTests
    {
        private const double Day = 24d * 3600d;

        private static RebirthCandidate Hero(int level = 400, long gold = 1000000, string city = RebirthRules.ResetCityId,
            ZoneKind zone = ZoneKind.City, int stains = 0, bool alive = true, bool busy = false, int expected = -1, string id = "p1") =>
            new RebirthCandidate(id, level, gold, city, zone, stains, alive, busy, expected);

        private static RebirthCandidate GrandHero(int level = 500, long gold = 10000000) => Hero(level, gold, RebirthRules.GrandResetCityId);

        /// <summary>Resets the record <paramref name="times"/> times (a day apart), at the minimum level each one asks.</summary>
        private static double ResetTimes(RebirthState state, int times, double start = 0d)
        {
            double now = start;
            for (int i = 0; i < times; i++)
            {
                RebirthRequirement req = RebirthRules.NextRequirement(state);
                RebirthResult r = RebirthRules.TryReset(state, Hero(req.MinLevel), now);
                Assert.IsTrue(r.Ok, r.Refusal.ToString());
                now += Day;
            }

            return now;
        }

        // ------------------------------------------------------------------ requirements

        [Test]
        public void Requirements_LevelAndGoldRisePerReset()
        {
            Assert.AreEqual(400, RebirthRules.MinLevelFor(1));
            Assert.AreEqual(410, RebirthRules.MinLevelFor(2));
            Assert.AreEqual(490, RebirthRules.MinLevelFor(10));
            Assert.AreEqual(490, RebirthRules.MinLevelFor(99), "clamped to the cycle");

            Assert.AreEqual(20000, RebirthRules.GoldCostFor(1));
            Assert.AreEqual(38000, RebirthRules.GoldCostFor(10));
            Assert.AreEqual(100000, RebirthRules.GoldCostFor(41));
            Assert.AreEqual(100000, RebirthRules.GoldCostFor(60), "gold has a ceiling");
            for (int e = 2; e <= RebirthRules.MaxEffectiveResets; e++)
            {
                Assert.GreaterOrEqual(RebirthRules.GoldCostFor(e), RebirthRules.GoldCostFor(e - 1));
            }

            Assert.AreEqual(200000, RebirthRules.GrandGoldCostFor(0));
            Assert.AreEqual(400000, RebirthRules.GrandGoldCostFor(4));
            Assert.AreEqual(60, RebirthRules.MaxEffectiveResets);
        }

        [Test]
        public void Reset_RefusesEachMissingRequirement()
        {
            var s = new RebirthState("p1");
            Assert.AreEqual(RebirthRefusal.InvalidPlayer, RebirthRules.TryReset(s, Hero(id: "other"), 0d).Refusal);
            Assert.AreEqual(RebirthRefusal.InvalidPlayer, RebirthRules.TryReset(null, Hero(), 0d).Refusal);
            Assert.AreEqual(RebirthRefusal.LevelTooLow, RebirthRules.TryReset(s, Hero(level: 399), 0d).Refusal);
            Assert.AreEqual(RebirthRefusal.NotEnoughGold, RebirthRules.TryReset(s, Hero(gold: 19999), 0d).Refusal);
            Assert.AreEqual(RebirthRefusal.WrongPlace, RebirthRules.TryReset(s, Hero(city: CityCatalog.Mingcheng), 0d).Refusal);
            Assert.AreEqual(RebirthRefusal.WrongPlace, RebirthRules.TryReset(s, Hero(zone: ZoneKind.Wilds), 0d).Refusal);
            Assert.AreEqual(RebirthRefusal.WrongPlace, RebirthRules.TryReset(s, Hero(city: null, zone: ZoneKind.Wilds), 0d).Refusal);
            Assert.AreEqual(RebirthRefusal.Stained, RebirthRules.TryReset(s, Hero(stains: 1), 0d).Refusal);
            Assert.AreEqual(RebirthRefusal.Busy, RebirthRules.TryReset(s, Hero(busy: true), 0d).Refusal);
            Assert.AreEqual(RebirthRefusal.Dead, RebirthRules.TryReset(s, Hero(alive: false), 0d).Refusal);
            Assert.AreEqual(RebirthRefusal.StaleRequest, RebirthRules.TryReset(s, Hero(expected: 3), 0d).Refusal);
            Assert.AreEqual(0, s.EffectiveResets, "refusals change nothing");

            RebirthResult ok = RebirthRules.TryReset(s, Hero(level: 432, gold: 20000, expected: 0), 0d);
            Assert.IsTrue(ok.Ok);
            Assert.AreEqual(20000, ok.GoldCost);
            Assert.AreEqual(432, ok.PreviousLevel);
            Assert.AreEqual(20, ok.BonusPointsGained);
            Assert.AreEqual(20, ok.TotalBonusPoints);
            Assert.AreEqual(1, s.Resets);
            Assert.AreEqual(432, s.PeakLevel);
            Assert.AreEqual(0d, s.LastResetAt);
        }

        [Test]
        public void Reset_CooldownAndDoubleSubmit()
        {
            var s = new RebirthState("p1");
            Assert.IsTrue(RebirthRules.TryReset(s, Hero(expected: 0), 100d).Ok);
            Assert.AreEqual(RebirthRefusal.StaleRequest, RebirthRules.TryReset(s, Hero(410, expected: 0), 100d + Day).Refusal,
                "the same confirmed summary cannot be spent twice");
            Assert.AreEqual(RebirthRefusal.TooSoon, RebirthRules.TryReset(s, Hero(410, expected: 1), 100d + Day - 1d).Refusal);
            Assert.AreEqual(RebirthRefusal.LevelTooLow, RebirthRules.TryReset(s, Hero(400, expected: 1), 100d + Day).Refusal,
                "reset 2 asks 410");
            Assert.IsTrue(RebirthRules.TryReset(s, Hero(410, expected: 1), 100d + Day).Ok);
            Assert.AreEqual(100d + Day + RebirthRules.MinSecondsBetweenResets, RebirthRules.NextRequirement(s).ReadyAt);
        }

        // ------------------------------------------------------------------ cycle and grand reset

        [Test]
        public void Cycle_TenResetsThenGrandReset()
        {
            var s = new RebirthState("p1");
            double now = ResetTimes(s, 10);
            Assert.AreEqual(10, s.Resets);
            Assert.AreEqual(RebirthRefusal.CycleFull, RebirthRules.TryReset(s, Hero(500), now + 30 * Day).Refusal);

            RebirthRequirement grand = RebirthRules.NextRequirement(s);
            Assert.IsTrue(grand.IsGrand);
            Assert.AreEqual(500, grand.MinLevel);
            Assert.AreEqual(RebirthRules.GrandResetCityId, grand.CityId);

            double after = s.LastResetAt;
            Assert.AreEqual(RebirthRefusal.WrongPlace, RebirthRules.TryGrandReset(s, Hero(500), after + 8 * Day).Refusal);
            Assert.AreEqual(RebirthRefusal.LevelTooLow, RebirthRules.TryGrandReset(s, GrandHero(499), after + 8 * Day).Refusal);
            Assert.AreEqual(RebirthRefusal.NotEnoughGold, RebirthRules.TryGrandReset(s, GrandHero(500, 199999), after + 8 * Day).Refusal);
            Assert.AreEqual(RebirthRefusal.TooSoon, RebirthRules.TryGrandReset(s, GrandHero(), after + 6 * Day).Refusal);

            int pointsBefore = s.TotalBonusPoints;
            RebirthResult r = RebirthRules.TryGrandReset(s, GrandHero(), after + 7 * Day);
            Assert.IsTrue(r.Ok, r.Refusal.ToString());
            Assert.IsTrue(r.IsGrand);
            Assert.AreEqual(200000, r.GoldCost);
            Assert.AreEqual(0, r.BonusPointsGained);
            Assert.AreEqual(pointsBefore, s.TotalBonusPoints, "the grand reset keeps the points, it gives a step");
            Assert.AreEqual(0, s.Resets);
            Assert.AreEqual(1, s.GrandResets);
            Assert.AreEqual(10, s.EffectiveResets);
            Assert.AreEqual(400, RebirthRules.NextRequirement(s).MinLevel, "the next cycle starts at 400 again");
            Assert.AreEqual(40000, RebirthRules.NextRequirement(s).GoldCost, "but the price keeps rising (11th reset)");
            Assert.That(r.NewMilestones.Select(m => m.TitleId), Contains.Item("title_rebirth_column_step_1"));
        }

        [Test]
        public void GrandReset_NeedsAFullCycle_AndEverythingStopsAtTheCap()
        {
            var s = new RebirthState("p1", resets: 3);
            Assert.AreEqual(RebirthRefusal.CycleNotFull, RebirthRules.TryGrandReset(s, GrandHero(), 1e9).Refusal);

            var capped = new RebirthState("p1", resets: 10, grandResets: 5, peakLevel: 500, lastResetAt: 0d);
            Assert.AreEqual(60, capped.EffectiveResets);
            Assert.AreEqual(300, capped.TotalBonusPoints);
            Assert.AreEqual(RebirthRefusal.MaxReached, RebirthRules.TryReset(capped, Hero(500), 1e9).Refusal);
            Assert.AreEqual(RebirthRefusal.MaxReached, RebirthRules.TryGrandReset(capped, GrandHero(), 1e9).Refusal);
            Assert.IsFalse(RebirthRules.NextRequirement(capped).Available);

            var clamped = new RebirthState("p1", resets: 99, grandResets: 99);
            Assert.AreEqual(10, clamped.Resets);
            Assert.AreEqual(5, clamped.GrandResets);
        }

        // ------------------------------------------------------------------ bonus points

        [Test]
        public void BonusPoints_DecreasingCurve_300AtTheCap()
        {
            Assert.AreEqual(20, RebirthRules.BonusPointsFor(1));
            Assert.AreEqual(10, RebirthRules.BonusPointsFor(6));
            Assert.AreEqual(5, RebirthRules.BonusPointsFor(11));
            Assert.AreEqual(3, RebirthRules.BonusPointsFor(21));
            Assert.AreEqual(2, RebirthRules.BonusPointsFor(60));
            Assert.AreEqual(0, RebirthRules.BonusPointsFor(0));
            Assert.AreEqual(0, RebirthRules.BonusPointsFor(61));
            for (int e = 2; e <= RebirthRules.MaxEffectiveResets; e++)
            {
                Assert.LessOrEqual(RebirthRules.BonusPointsFor(e), RebirthRules.BonusPointsFor(e - 1), "never grows");
            }

            Assert.AreEqual(100, RebirthRules.TotalBonusPoints(5));
            Assert.AreEqual(150, RebirthRules.TotalBonusPoints(10));
            Assert.AreEqual(300, RebirthRules.TotalBonusPoints(60));
            Assert.AreEqual(300, RebirthRules.TotalBonusPoints(1000));
        }

        [Test]
        public void BonusPoints_WakeUpWithTheLevels()
        {
            Assert.AreEqual(0, RebirthRules.UnlockedBonusPoints(300, 1));
            Assert.AreEqual(75, RebirthRules.UnlockedBonusPoints(300, 100));
            Assert.AreEqual(300, RebirthRules.UnlockedBonusPoints(300, 400));
            Assert.AreEqual(300, RebirthRules.UnlockedBonusPoints(300, 500));
            Assert.AreEqual(0, RebirthRules.UnlockedBonusPoints(0, 500));

            // Granting level by level adds up to the total, and never gives anything twice.
            int granted = 0;
            for (int level = 2; level <= 500; level++)
            {
                int g = RebirthRules.BonusPointsToGrant(150, level - 1, level);
                Assert.GreaterOrEqual(g, 0);
                granted += g;
            }

            Assert.AreEqual(150, granted);
            Assert.AreEqual(0, RebirthRules.BonusPointsToGrant(150, 300, 200), "going down never takes points");
        }

        // ------------------------------------------------------------------ Eco da Luz

        [Test]
        public void Echo_FourTimesKillXpOnlyBelowTheOldLevel()
        {
            var never = new RebirthState("p1");
            Assert.AreEqual(1000, RebirthRules.ScaleKillExperience(1000, never, 10), "never reset: unchanged");

            var s = new RebirthState("p1");
            Assert.IsTrue(RebirthRules.TryReset(s, Hero(420), 0d).Ok);
            Assert.IsTrue(RebirthRules.IsEchoActive(s, 1));
            Assert.AreEqual(4000, RebirthRules.ScaleKillExperience(1000, s, 1));
            Assert.AreEqual(4000, RebirthRules.ScaleKillExperience(1000, s, 419));
            Assert.AreEqual(1000, RebirthRules.ScaleKillExperience(1000, s, 420), "stops at the old level");
            Assert.AreEqual(0, RebirthRules.ScaleKillExperience(0, s, 1));
            Assert.AreEqual(3_000_000_000L * 4, RebirthRules.ScaleKillExperience(3_000_000_000L, s, 1), "long-safe");
        }

        // ------------------------------------------------------------------ kept and cleared

        [Test]
        public void Parts_KeptAndClearedSplitEverything()
        {
            Assert.AreEqual(RebirthPart.None, RebirthRules.Cleared & RebirthRules.Kept, "nothing is both kept and cleared");
            RebirthPart all = RebirthPart.None;
            foreach (RebirthPart p in System.Enum.GetValues(typeof(RebirthPart)))
            {
                all |= p;
            }

            Assert.AreEqual(all, RebirthRules.Cleared | RebirthRules.Kept, "every part has a decision");
            Assert.IsTrue((RebirthRules.Cleared & RebirthPart.Level) != 0);
            Assert.IsTrue((RebirthRules.Kept & RebirthPart.Items) != 0);
            Assert.IsTrue((RebirthRules.Kept & RebirthPart.Quests) != 0);
            Assert.IsTrue((RebirthRules.Kept & RebirthPart.Caderno) != 0);
            Assert.IsTrue(RebirthRules.UsesNewGamePlus(false), "offline saves keep NG+");
            Assert.IsFalse(RebirthRules.UsesNewGamePlus(true), "online characters reset instead");
        }

        // ------------------------------------------------------------------ milestones

        [Test]
        public void Milestones_CosmeticOnly_GivenOnce()
        {
            var s = new RebirthState("p1");
            RebirthResult first = RebirthRules.TryReset(s, Hero(), 0d);
            Assert.AreEqual(new[] { "title_rebirth_returned_spark" }, first.NewMilestones.Select(m => m.TitleId).ToArray());
            RebirthResult second = RebirthRules.TryReset(s, Hero(410), Day);
            Assert.AreEqual(0, second.NewMilestones.Count);

            Assert.AreEqual(12, RebirthRules.Milestones.Count);
            Assert.AreEqual(RebirthRules.Milestones.Count, RebirthRules.Milestones.Select(m => m.TitleId).Distinct().Count());
            foreach (RebirthMilestone m in RebirthRules.Milestones)
            {
                StringAssert.StartsWith("title_rebirth_", m.TitleId);
                StringAssert.StartsWith("cosmetic_rebirth_", m.CosmeticId);
            }

            var capped = new RebirthState("p1", 10, 5);
            Assert.AreEqual(12, RebirthRules.MilestonesOwned(capped).Count);
            Assert.AreEqual(4, RebirthRules.MilestonesOwned(new RebirthState("p1", 10)).Count, "1, 3, 5 and 10");
        }

        // ------------------------------------------------------------------ ranking

        [Test]
        public void Ranking_ResetsBeforeLevel_LikeMu()
        {
            var entries = new[]
            {
                new RankingEntry("a", "A", 500, 1_800_000_000L),
                new RankingEntry("b", "B", 30, 100, resets: 1),
                new RankingEntry("c", "C", 450, 900, resets: 1),
                new RankingEntry("d", "D", 5, 0, resets: 0, grandResets: 1),
                new RankingEntry("e", "E", 450, 900, reachedAt: 1d, resets: 1),
            };

            string[] order = RankingRules.Rank(entries, RankingBoard.Level).Select(r => r.Entry.PlayerId).ToArray();
            Assert.AreEqual(new[] { "d", "c", "e", "b", "a" }, order);
            List<RankedEntry> ranked = RankingRules.Rank(entries, RankingBoard.Level);
            Assert.AreEqual(ranked[1].Rank, ranked[2].Rank, "same resets, level and XP share the rank");
            Assert.IsFalse(RankingRules.ResetsWithSeason(RankingBoard.Level));
        }

        // ------------------------------------------------------------------ texts

        [Test]
        public void Texts_EveryRefusalPartTitleAndCousinHasPtAndEn()
        {
            var keys = new HashSet<string>(RebirthTexts.Entries.Select(e => e.Key));
            Assert.AreEqual(RebirthTexts.Entries.Count, keys.Count, "keys are unique");
            foreach (RebirthRefusal r in System.Enum.GetValues(typeof(RebirthRefusal)))
            {
                Assert.IsTrue(keys.Contains(RebirthTexts.RefusalKey(r)), r.ToString());
            }

            foreach (RebirthPart p in System.Enum.GetValues(typeof(RebirthPart)))
            {
                if (p != RebirthPart.None)
                {
                    Assert.IsTrue(keys.Contains(RebirthTexts.PartKey(p)), p.ToString());
                }
            }

            foreach (RebirthMilestone m in RebirthRules.Milestones)
            {
                Assert.IsTrue(keys.Contains(RebirthTexts.TitleKey(m.TitleId)), m.TitleId);
                Assert.IsTrue(keys.Contains(RebirthTexts.CosmeticKey(m.CosmeticId)), m.CosmeticId);
            }

            foreach (CharacterId c in System.Enum.GetValues(typeof(CharacterId)))
            {
                Assert.IsTrue(keys.Contains(RebirthTexts.CousinResetLineKey(c)), c.ToString());
                Assert.IsTrue(keys.Contains(RebirthTexts.CousinGrandLineKey(c)), c.ToString());
            }

            foreach (var e in RebirthTexts.Entries)
            {
                Assert.IsFalse(string.IsNullOrWhiteSpace(e.En), e.Key);
                Assert.IsFalse(string.IsNullOrWhiteSpace(e.PtBR), e.Key);
                foreach (string banned in new[] { "salvação", "pecado", "religião", "rezar", "Deus" })
                {
                    StringAssert.DoesNotContain(banned, e.PtBR, e.Key + " follows the bible's tone rules");
                }
            }

            Assert.AreEqual("Volta da Luz", RebirthTexts.Get(null, RebirthTexts.ResetName));
            Assert.AreEqual("Nível 432 → 1 · Ouro: 20000 · Pontos de luz: +20 (total 20)",
                RebirthTexts.Format(null, RebirthTexts.Summary, 432, 20000, 20, 20));
            Assert.AreEqual("stale_request", RebirthTexts.Snake("StaleRequest"));
        }

        // ------------------------------------------------------------------ balance simulation

        [Test]
        public void Balance_PowerWithResetsStaysInsideTheBand()
        {
            for (int level = 1; level <= 500; level++)
            {
                double previous = 1d;
                for (int resets = 0; resets <= RebirthRules.MaxEffectiveResets; resets++)
                {
                    double ratio = RebirthBalance.PowerRatio(level, resets);
                    Assert.GreaterOrEqual(ratio, previous - 1e-9, $"L{level} R{resets}: more resets never weaker");
                    Assert.LessOrEqual(ratio, RebirthBalance.MaxPowerRatio, $"L{level} R{resets}: at most +30%");
                    previous = ratio;
                }

                Assert.LessOrEqual(RebirthBalance.PowerRatio(level, 1), RebirthBalance.OneResetPowerRatio, $"L{level}: 1 reset < +3%");
                Assert.LessOrEqual(RebirthBalance.PowerRatio(level, 10), RebirthBalance.OneCyclePowerRatio, $"L{level}: 1 cycle ≤ +15%");
            }

            Assert.AreEqual(1d, RebirthBalance.PowerRatio(1, 60), 1e-9, "level 1 after a reset is a level-1 hero");
            Assert.Greater(RebirthBalance.PowerRatio(400, 60), 1.20, "the cap is still worth it (> +20% at 400)");
            Assert.Greater(RebirthBalance.PowerRatio(400, 1), 1.01, "even one reset is felt");

            // Even at the cap a reset hero never beats a hero 75 levels above with no reset (the bonus is not a level skip).
            for (int level = 1; level <= 425; level++)
            {
                int bonus = RebirthRules.UnlockedBonusPoints(RebirthRules.TotalBonusPoints(60), level);
                Assert.Less(RebirthBalance.Power(level, bonus), RebirthBalance.Power(level + 75, 0), $"L{level}");
            }
        }
    }
}
