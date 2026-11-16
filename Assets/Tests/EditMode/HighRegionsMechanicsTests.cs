using NUnit.Framework;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.World.Map.Mechanics;
using Rapadura.Gameplay.World.Map.Overworld;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// The 2026-11-16 mechanics of Regiões 10–12 (Docs/Mapas100.md): the headwind and shelter stones of the Garganta dos Ays (57), the Huma's
    /// shadow in the Rei-Vendaval's fight (58), the season of the Tundra de Omöl (61), the raked gravel and hidden tea house (66), the goat's
    /// hooves on the eruption (67) and the nine furnaces (69).
    /// </summary>
    public class HighRegionsMechanicsTests
    {
        // ---------------- 57 ----------------

        [Test]
        public void Headwind_SlowsOnlyIntoTheWind_NotBehindAStone_HalfAtNight()
        {
            Vector2 wind = GorgeHeadwindRules.Wind(OverworldLayouts.GorgeWindAxis);
            Assert.AreEqual(1f, wind.magnitude, 1e-4f);
            Assert.AreEqual(GorgeHeadwindRules.AgainstFactor, GorgeHeadwindRules.SpeedFactor(-wind * 5f, wind, false, false));
            Assert.AreEqual(GorgeHeadwindRules.NightAgainstFactor, GorgeHeadwindRules.SpeedFactor(-wind * 5f, wind, false, true));
            Assert.AreEqual(1f, GorgeHeadwindRules.SpeedFactor(-wind * 5f, wind, true, false), "behind a pedra-abrigo");
            Assert.AreEqual(1f, GorgeHeadwindRules.SpeedFactor(wind * 5f, wind, false, false), "with the wind");
            Assert.AreEqual(1f, GorgeHeadwindRules.SpeedFactor(new Vector2(-wind.y, wind.x) * 5f, wind, false, false), "across the wind");
            Assert.AreEqual(1f, GorgeHeadwindRules.SpeedFactor(Vector2.zero, wind, false, false), "standing still");
            Vector2 at50 = -(Quaternion.Euler(0f, 0f, 50f) * wind);
            Assert.IsTrue(GorgeHeadwindRules.Against(at50, wind), "50° off straight into the wind still counts");
            Vector2 at70 = -(Quaternion.Euler(0f, 0f, 70f) * wind);
            Assert.IsFalse(GorgeHeadwindRules.Against(at70, wind));
        }

        [Test]
        public void ShelterStones_StandInTheGorge_AndAreFoundWithinTheirRadius()
        {
            Rect gorge = OverworldLayouts.GustAreas[0];
            foreach (Vector2 stone in OverworldLayouts.GorgeShelterStones)
            {
                Assert.IsTrue(gorge.Contains(stone), $"{stone} is outside the Garganta dos Ays");
            }

            Vector2 first = OverworldLayouts.GorgeShelterStones[0];
            Assert.AreEqual(0, GorgeHeadwindRules.ShelterAt(OverworldLayouts.GorgeShelterStones, first + new Vector2(5f, 0f)));
            Assert.AreEqual(-1, GorgeHeadwindRules.ShelterAt(OverworldLayouts.GorgeShelterStones, first + new Vector2(0f, -30f)));
        }

        // ---------------- 58 ----------------

        [Test]
        public void HumaShadow_FirstAt12s_ThenEvery24s_MarkedFor4s()
        {
            Assert.AreEqual(-1, HumaShadowRules.PassIndex(11.9f));
            Assert.IsFalse(HumaShadowRules.Marked(5f));
            Assert.AreEqual(0, HumaShadowRules.PassIndex(12f));
            Assert.IsTrue(HumaShadowRules.Marked(15.9f));
            Assert.IsFalse(HumaShadowRules.Marked(16.1f));
            Assert.AreEqual(1, HumaShadowRules.PassIndex(36f));
            Assert.IsTrue(HumaShadowRules.Marked(37f));
            Assert.AreEqual(7f, HumaShadowRules.SecondsToNext(5f), 1e-4f);
            Assert.AreEqual(0f, HumaShadowRules.SecondsToNext(13f));
            Assert.AreEqual(4f, HumaShadowRules.SecondsToNext(32f), 1e-4f);
        }

        [Test]
        public void HumaShadow_FallsAStepAway_NeverTwiceInTheSameSpot_HealsWithinMax()
        {
            Vector2 hero = new Vector2(880f, 1500f);
            Vector2 a = HumaShadowRules.RingCenter(hero, 0);
            Vector2 b = HumaShadowRules.RingCenter(hero, 1);
            Assert.AreEqual(HumaShadowRules.RingOffset, (a - hero).magnitude, 1e-3f);
            Assert.IsFalse(HumaShadowRules.Inside(a, hero), "the hero must step to it");
            Assert.Greater((a - b).magnitude, HumaShadowRules.RingRadius, "the next pass falls elsewhere");
            Assert.IsTrue(HumaShadowRules.Inside(a, a + new Vector2(3f, 0f)));
            Assert.AreEqual(80f, HumaShadowRules.HealAmount(500f, 1000f), 1e-3f);
            Assert.AreEqual(20f, HumaShadowRules.HealAmount(980f, 1000f), 1e-3f);
            Assert.AreEqual(0f, HumaShadowRules.HealAmount(1000f, 1000f));
        }

        // ---------------- 61 ----------------

        [Test]
        public void TundraSeason_WinterIsTheFrozenCanals_SummerTheNewMoon_AllFourInOneMoon()
        {
            var seen = new System.Collections.Generic.HashSet<TundraSeason>();
            for (int day = 1; day <= MoonCanalRules.CycleDays; day++)
            {
                TundraSeason season = TundraSeasonRules.SeasonOf(day);
                seen.Add(season);
                Assert.AreEqual(MoonCanalRules.IsFrozen(day), season == TundraSeason.Winter, $"day {day}");
            }

            Assert.AreEqual(4, seen.Count);
            Assert.AreEqual(TundraSeason.Summer, TundraSeasonRules.SeasonOf(1), "day 1 is the new moon");
            Assert.AreEqual(TundraSeason.Autumn, TundraSeasonRules.SeasonOf(3));
            Assert.AreEqual(TundraSeason.Spring, TundraSeasonRules.SeasonOf(7));
            Assert.AreEqual(TundraSeasonRules.SeasonOf(2), TundraSeasonRules.SeasonOf(2 + MoonCanalRules.CycleDays));
        }

        [Test]
        public void TundraSeason_BogSlows_SnowSpeeds_SafeGroundIsPlain_CountsToTheNext()
        {
            Assert.AreEqual(TundraSeasonRules.BogFactor, TundraSeasonRules.SpeedFactor(TundraSeason.Summer, false));
            Assert.AreEqual(TundraSeasonRules.SnowFactor, TundraSeasonRules.SpeedFactor(TundraSeason.Winter, false));
            Assert.AreEqual(1f, TundraSeasonRules.SpeedFactor(TundraSeason.Autumn, false));
            Assert.AreEqual(1f, TundraSeasonRules.SpeedFactor(TundraSeason.Summer, true));
            Assert.Less(TundraSeasonRules.BogFactor, 1f);
            Assert.Greater(TundraSeasonRules.SnowFactor, 1f);
            Assert.AreEqual(2, TundraSeasonRules.DaysToNextSeason(1), "summer runs days 8, 1, 2");
            Assert.AreEqual(1, TundraSeasonRules.DaysToNextSeason(3), "autumn is one day");
            Assert.IsTrue(OverworldLayouts.TundraSeasonArea.Contains(OverworldLayouts.PrimeiroDesajeitadoArena), "the season's ground is under the boss's ring too");
            Assert.IsTrue(OverworldLayouts.TundraSeasonArea.Contains(OverworldLayouts.TendaDeYenOffset));
        }

        // ---------------- 66 ----------------

        [Test]
        public void Gravel_EachNeighbourAsksItsPattern_RakeCycles_TheSpiralIsNobodys()
        {
            Assert.AreEqual(GravelPattern.Waves, HinokuraGravelRules.Target(GravelNeighbour.Water));
            Assert.AreEqual(GravelPattern.Rings, HinokuraGravelRules.Target(GravelNeighbour.Stone));
            Assert.AreEqual(GravelPattern.Lines, HinokuraGravelRules.Target(GravelNeighbour.Pine));
            foreach (GravelNeighbour neighbour in HinokuraGravelRules.Neighbours)
            {
                Assert.AreNotEqual(GravelPattern.Spiral, HinokuraGravelRules.Target(neighbour));
            }

            GravelPattern p = GravelPattern.Waves;
            for (int i = 0; i < HinokuraGravelRules.PatternCount; i++)
            {
                p = HinokuraGravelRules.Next(p);
            }

            Assert.AreEqual(GravelPattern.Waves, p, "four rakes go round");
            Assert.AreEqual(OverworldLayouts.GravelGardens.Length, HinokuraGravelRules.Neighbours.Length);
            Assert.IsTrue(HinokuraGravelRules.AllRight(HinokuraGravelRules.Calm()));
        }

        [Test]
        public void Gravel_TheMorningsMess_IsTheSameForEveryone_NeverAlreadyCalm_AndSolvable()
        {
            for (long day = 20000; day < 20400; day++)
            {
                GravelPattern[] start = HinokuraGravelRules.StartFor(day);
                CollectionAssert.AreEqual(start, HinokuraGravelRules.StartFor(day));
                Assert.IsFalse(HinokuraGravelRules.AllRight(start), $"day {day}");

                // Raking each garden until it is right always calms the three in at most 3 rakes per garden.
                for (int g = 0; g < start.Length; g++)
                {
                    int rakes = 0;
                    while (!HinokuraGravelRules.IsRight(start, g))
                    {
                        start[g] = HinokuraGravelRules.Next(start[g]);
                        rakes++;
                    }

                    Assert.LessOrEqual(rakes, 3);
                }

                Assert.IsTrue(HinokuraGravelRules.AllRight(start));
            }
        }

        [Test]
        public void TeaHouse_PaysFirstTimeMore_ThenOncePerDay_OnlyWhenCalm_AndIsClearOfThePost()
        {
            Assert.AreEqual((HinokuraGravelRules.FirstGold, HinokuraGravelRules.FirstStones), HinokuraGravelRules.TeaReward(true, false, false));
            Assert.AreEqual((HinokuraGravelRules.DailyGold, 0), HinokuraGravelRules.TeaReward(true, false, true));
            Assert.AreEqual((0, 0), HinokuraGravelRules.TeaReward(true, true, true), "served today");
            Assert.AreEqual((0, 0), HinokuraGravelRules.TeaReward(false, false, false), "the gardens are not calm");
            Assert.Greater((OverworldLayouts.HiddenTeaHouse - OverworldLayouts.CasaDeChaOffset).magnitude, 30f);
            Assert.AreEqual(1, HinokuraGravelRules.GardenAt(OverworldLayouts.GravelGardens, OverworldLayouts.GravelGardens[1] + new Vector2(3f, 0f)));
            Assert.AreEqual(-1, HinokuraGravelRules.GardenAt(OverworldLayouts.GravelGardens, OverworldLayouts.CasaDeChaOffset));
            foreach (Vector2 garden in OverworldLayouts.GravelGardens)
            {
                Assert.IsTrue(OverworldLayouts.HinokuraBounds.Contains(garden));
            }
        }

        // ---------------- 67 ----------------

        [Test]
        public void GoatHooves_OnlyMountedOnTheVolcanoGoat()
        {
            Assert.IsTrue(HinokuraEruptionRules.HoovesProof(true, "mount_volcano_goat"));
            Assert.IsFalse(HinokuraEruptionRules.HoovesProof(false, "mount_volcano_goat"), "on foot beside it");
            Assert.IsFalse(HinokuraEruptionRules.HoovesProof(true, "mount_woolly_mammoth"));
            Assert.IsFalse(HinokuraEruptionRules.HoovesProof(true, null));
        }

        // ---------------- 69 ----------------

        [Test]
        public void Furnaces_StartUnbalanced_KeepTheTotal_AndStayInRange()
        {
            for (long seed = 0; seed < 300; seed++)
            {
                foreach (bool night in new[] { false, true })
                {
                    int[] heat = NineFurnacesPuzzle.Start(seed, NineFurnacesPuzzle.ScramblesFor(night));
                    Assert.IsFalse(NineFurnacesPuzzle.IsBalanced(heat), $"seed {seed}");
                    int sum = 0;
                    foreach (int h in heat)
                    {
                        Assert.GreaterOrEqual(h, NineFurnacesPuzzle.MinHeat);
                        Assert.LessOrEqual(h, NineFurnacesPuzzle.MaxHeat);
                        sum += h;
                    }

                    Assert.AreEqual(NineFurnacesPuzzle.Count * NineFurnacesPuzzle.Balance, sum);
                    Assert.AreEqual(NineFurnacesPuzzle.ScramblesFor(night), NineFurnacesPuzzle.ScrambledFurnaces(seed, NineFurnacesPuzzle.ScramblesFor(night)).Length);
                }
            }
        }

        [Test]
        public void Furnaces_PumpingTheScrambledOnesInAnyOrder_Balances_NoPumpRefused()
        {
            for (long seed = 0; seed < 200; seed++)
            {
                int scrambles = NineFurnacesPuzzle.ScramblesFor(seed % 2 == 0);
                int[] heat = NineFurnacesPuzzle.Start(seed, scrambles);
                int[] order = NineFurnacesPuzzle.ScrambledFurnaces(seed, scrambles);
                if (seed % 3 == 0)
                {
                    System.Array.Reverse(order);
                }

                foreach (int i in order)
                {
                    Assert.AreEqual(FurnacePumpResult.Pumped, NineFurnacesPuzzle.Pump(heat, i), $"seed {seed} furnace {i}");
                }

                Assert.IsTrue(NineFurnacesPuzzle.IsBalanced(heat), $"seed {seed}");
                Assert.AreEqual(FurnacePumpResult.Balanced, NineFurnacesPuzzle.Pump(heat, 0));
            }
        }

        [Test]
        public void Furnaces_APumpMovesHeat_RefusesPastTheEnds_RewardOncePerDay()
        {
            int[] heat = { 5, 5, 5, 5, 5, 5, 5, 5, 4 };
            heat[0] = 6;
            Assert.AreEqual(FurnacePumpResult.Pumped, NineFurnacesPuzzle.Pump(heat, 4));
            CollectionAssert.AreEqual(new[] { 6, 5, 5, 4, 7, 4, 5, 5, 4 }, heat);

            int[] hot = { 8, 5, 5, 5, 5, 5, 5, 5, 2 };
            Assert.AreEqual(FurnacePumpResult.TooHot, NineFurnacesPuzzle.Pump(hot, 0));
            int[] cold = { 5, 1, 5, 5, 5, 5, 5, 5, 9 };
            Assert.AreEqual(FurnacePumpResult.NeighbourTooCold, NineFurnacesPuzzle.Pump(cold, 0));
            CollectionAssert.AreEqual(new[] { 5, 1, 5, 5, 5, 5, 5, 5, 9 }, cold, "a refused pump changes nothing");

            Assert.AreEqual(8, NineFurnacesPuzzle.Left(0));
            Assert.AreEqual(0, NineFurnacesPuzzle.Right(8));
            Assert.AreEqual((NineFurnacesPuzzle.DailyGold, NineFurnacesPuzzle.DailyStones, NineFurnacesPuzzle.FirstMixtureStones), NineFurnacesPuzzle.Reward(false, true));
            Assert.AreEqual((NineFurnacesPuzzle.DailyGold, NineFurnacesPuzzle.DailyStones, 0), NineFurnacesPuzzle.Reward(false, false));
            Assert.AreEqual((0, 0, 0), NineFurnacesPuzzle.Reward(true, false));
            Assert.AreNotEqual(NineFurnacesPuzzle.SeedFor(100, 0, false), NineFurnacesPuzzle.SeedFor(100, 0, true));
        }

        [Test]
        public void Furnaces_TheRingStandsInTheCourt_ClearOfTheMastersArena()
        {
            Rect court = Rect.MinMaxRect(-1680f, 360f, -1200f, 840f);
            for (int i = 0; i < NineFurnacesPuzzle.Count; i++)
            {
                Vector2 bellows = OverworldLayouts.NineFurnace(i);
                Assert.IsTrue(court.Contains(bellows), $"furnace {i} at {bellows}");
                Assert.Greater((bellows - OverworldLayouts.MestreDasFornalhasArena).magnitude, 30f);
                Assert.AreEqual(i, NineFurnacesPuzzle.FurnaceAt(new[]
                {
                    OverworldLayouts.NineFurnace(0), OverworldLayouts.NineFurnace(1), OverworldLayouts.NineFurnace(2), OverworldLayouts.NineFurnace(3),
                    OverworldLayouts.NineFurnace(4), OverworldLayouts.NineFurnace(5), OverworldLayouts.NineFurnace(6), OverworldLayouts.NineFurnace(7),
                    OverworldLayouts.NineFurnace(8),
                }, bellows + new Vector2(1f, 1f)));
            }
        }

        // ---------------- Texts ----------------

        [Test]
        public void Texts_HaveBothLanguages_AndALinePerCousin()
        {
            foreach (var entries in new[] { HinokuraPuzzleTexts.Entries, TundraSeasonTexts.Entries, FarnkeshHeightsTexts.Entries })
            {
                foreach (var entry in entries)
                {
                    Assert.IsFalse(string.IsNullOrEmpty(entry.En), entry.Key);
                    Assert.IsFalse(string.IsNullOrEmpty(entry.PtBR), entry.Key);
                    StringAssert.DoesNotContain("Dona ", entry.PtBR);
                    StringAssert.DoesNotContain("Seu ", entry.PtBR);
                }
            }

            foreach (CharacterId hero in new[] { CharacterId.Maithe, CharacterId.Icaro, CharacterId.Joaquim, CharacterId.Maria, CharacterId.Lavine })
            {
                Assert.IsNotNull(HinokuraPuzzleTexts.CalmLineFor(hero));
                Assert.IsNotNull(HinokuraPuzzleTexts.FurnaceLineFor(hero));
                Assert.IsNotNull(TundraSeasonTexts.LineFor(hero));
                Assert.IsNotNull(FarnkeshHeightsTexts.GorgeLineFor(hero));
                Assert.IsNotNull(FarnkeshHeightsTexts.HumaLineFor(hero));
                Assert.AreNotEqual(HinokuraPuzzleTexts.CalmLineFor(hero), HinokuraPuzzleTexts.Get(null, HinokuraPuzzleTexts.CalmLineFor(hero)), "the line resolves to text");
            }

            Assert.AreEqual("ondas", HinokuraPuzzleTexts.Get(null, HinokuraPuzzleTexts.PatternNames[0]));
            StringAssert.Contains("3", HinokuraPuzzleTexts.Format(null, HinokuraPuzzleTexts.GravelRaked, "ondas", 3));
        }
    }
}
