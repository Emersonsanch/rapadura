using System.Linq;
using NUnit.Framework;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.World.Map.Mechanics;
using Rapadura.Gameplay.World.Map.Overworld;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// The second 2026-11-23 round on the sheets 49, 52, 55, 56 and 70 (Docs/Mapas100.md): falling off the Pontes de Vento and the freed Cortador de
    /// Cordas, the stilled bell's silence, the good dream, Amirani's chain at night, and the caldera's night lava and the Glutão's own heat.
    /// </summary>
    public class PeaksSteppeRound2Tests
    {
        // ---------------- 55 · falling off the wind bridges ----------------

        [Test]
        public void Bridge_Nearest_FindsTheLineAndTheDistance()
        {
            Vector2[] line = OverworldLayouts.WindBridgeLines[1];
            Vector2 mid = Vector2.Lerp(line[0], line[1], 0.5f);
            Vector2 across = new Vector2(-(line[1] - line[0]).y, (line[1] - line[0]).x).normalized;
            int found = WindBridgeFallRules.Nearest(OverworldLayouts.WindBridgeLines, mid + across * 3f, out float along, out float distance);
            Assert.AreEqual(1, found);
            Assert.AreEqual(3f, distance, 0.01f);
            Assert.AreEqual(Vector2.Distance(line[0], line[1]) * 0.5f, along, 0.05f);
            Assert.AreEqual(-1, WindBridgeFallRules.Nearest(null, mid, out _, out _));
        }

        [Test]
        public void Bridge_WalkingWithoutCloak_FallsFromTheMiddle_CloakOnlyFromTheEdge()
        {
            float walking = WindGustRules.PushSpeed * WindGustRules.BlowSeconds;
            float cloak = walking * WindGustRules.StormCloakFactor;
            Assert.IsFalse(WindBridgeFallRules.OnPlanks(walking), "7.5 m off the line: off the planks");
            Assert.IsTrue(WindBridgeFallRules.OnPlanks(cloak), "3.75 m from the middle: still on them");
            Assert.IsFalse(WindBridgeFallRules.OnPlanks(1.5f + cloak), "from 1.5 m off the middle the cloak is not enough");
        }

        [Test]
        public void Bridge_OnlyAPushedHeroFalls()
        {
            Assert.IsTrue(WindBridgeFallRules.IsFall(5f, 10f, 10.5f));
            Assert.IsFalse(WindBridgeFallRules.IsFall(5f, 11f, 10.5f), "walked off with no gust: no fall");
            Assert.IsFalse(WindBridgeFallRules.IsFall(3f, 10f, 10.5f), "still on the planks");
        }

        [Test]
        public void Bridge_Net_ThrowsBack12m_NeverPastTheEntry_GuardKeepsThePlace()
        {
            Assert.AreEqual(88f, WindBridgeFallRules.LandingAlong(20f, 100f, false), 1e-4f);
            Assert.AreEqual(20f, WindBridgeFallRules.LandingAlong(20f, 25f, false), 1e-4f, "never behind where they stepped on");
            Assert.AreEqual(212f, WindBridgeFallRules.LandingAlong(300f, 200f, false), 1e-4f, "walking down the line: back is up it");
            Assert.AreEqual(100f, WindBridgeFallRules.LandingAlong(20f, 100f, true), 1e-4f, "the guard: where the gust hit");
        }

        [Test]
        public void Bridge_FallDamage_NeverUnderTheFloor_NoneWhenCaught()
        {
            Assert.AreEqual(80f, WindBridgeFallRules.FallDamage(1000f, 1000f, false), 1e-3f);
            Assert.AreEqual(10f, WindBridgeFallRules.FallDamage(260f, 1000f, false), 1e-3f);
            Assert.AreEqual(0f, WindBridgeFallRules.FallDamage(200f, 1000f, false), 1e-3f);
            Assert.AreEqual(0f, WindBridgeFallRules.FallDamage(1000f, 1000f, true), 1e-3f);
            Assert.AreEqual(RegionBossRules.RopeCutterId, WindBridgeFallRules.GuardEnemyId);
        }

        // ---------------- 56 · the stilled bell ----------------

        [Test]
        public void Bell_Stilled_QuietsTheCliff_AndJoinsLavineAndTheNight()
        {
            Assert.AreEqual(1f, HermitageStillBellRules.Quiet(false, false, false), 1e-5f);
            Assert.AreEqual(0.7f, HermitageStillBellRules.Quiet(false, false, true), 1e-5f);
            Assert.AreEqual(0.6f * 0.6f * 0.7f, HermitageStillBellRules.Quiet(true, true, true), 1e-5f);
            Assert.AreEqual(FarnkeshSilenceRules.AfterEcho, HermitageStillBellRules.AfterEcho(false), 1e-5f);
            Assert.Less(HermitageStillBellRules.AfterEcho(true), FarnkeshSilenceRules.AfterEcho);
        }

        [Test]
        public void Bell_Stilled_RunningTakesLongerToWakeTheEcho()
        {
            float SecondsToFull(bool stilled)
            {
                float meter = 0f;
                float quiet = HermitageStillBellRules.Quiet(false, false, stilled);
                int steps = 0;
                while (!FarnkeshSilenceRules.Full(meter) && steps < 10000)
                {
                    meter = FarnkeshSilenceRules.Step(meter, FarnkeshPace.Running, 0, quiet, 0.1f);
                    steps++;
                }

                return steps * 0.1f;
            }

            Assert.AreEqual(12.5f, SecondsToFull(false), 0.11f);
            Assert.AreEqual(12.5f / 0.7f, SecondsToFull(true), 0.11f);
        }

        // ---------------- 49 · the good dream ----------------

        [Test]
        public void Dream_Good_IsClearer_AndPaysMore()
        {
            Assert.AreEqual(NartesDreamRules.Purse, GoodDreamRules.Purse(false));
            Assert.AreEqual(70, GoodDreamRules.Purse(true));
            Vector2 fort = Vector2.zero;
            Vector2 chest = new Vector2(0f, 137f);
            Assert.AreEqual(NartesDreamRules.ToldDistance(fort, chest), GoodDreamRules.ToldDistance(fort, chest, false), "blurred: the old rounding");
            Assert.AreEqual(150, GoodDreamRules.ToldDistance(fort, chest, false));
            Assert.AreEqual(125, GoodDreamRules.ToldDistance(fort, chest, true));
            Assert.AreEqual(25, GoodDreamRules.ToldDistance(fort, new Vector2(3f, 0f), true), "never under the rounding");
            Assert.AreEqual(RegionBossRules.TabletNightmareId, GoodDreamRules.NightmareEnemyId);
        }

        // ---------------- 52 · the chain at night ----------------

        [Test]
        public void Chain_ByDay_NothingChanges()
        {
            Assert.AreEqual(1f, ChainNightGlowRules.PushFactor(false, 0, 10));
            Assert.AreEqual(1f, ChainNightGlowRules.PushFactor(false, 12, 10));
            Assert.IsFalse(ChainNightGlowRules.Lit(3, 10, false));
        }

        [Test]
        public void Chain_AtNight_LitStretchHolds_AboveItTheSnowPushes()
        {
            Assert.AreEqual(ChainNightGlowRules.GlowFactor, ChainNightGlowRules.PushFactor(true, 4, 5), "between rings 4 and 5, both lit");
            Assert.AreEqual(ChainNightGlowRules.SnowFactor, ChainNightGlowRules.PushFactor(true, 5, 5), "past the last lit ring");
            Assert.AreEqual(ChainNightGlowRules.SnowFactor, ChainNightGlowRules.PushFactor(true, 0, -1), "never climbed: all snow");
            Assert.IsTrue(ChainNightGlowRules.Lit(5, 5, true));
            Assert.IsFalse(ChainNightGlowRules.Lit(6, 5, true));

            // The strong snow still lets a runner hold against it (run 5.5 m/s).
            Assert.Less(NartesChainRules.PushSpeed * ChainNightGlowRules.SnowFactor, 5.5f);
        }

        [Test]
        public void Chain_BestRing_RoundTripsThroughTheMark_AndOnlyNewBestsWrite()
        {
            Assert.AreEqual(-1, ChainNightGlowRules.BestFromMark(0L));
            Assert.AreEqual(0, ChainNightGlowRules.BestFromMark(ChainNightGlowRules.MarkOf(0)));
            Assert.AreEqual(17, ChainNightGlowRules.BestFromMark(ChainNightGlowRules.MarkOf(17)));
            Assert.IsTrue(ChainNightGlowRules.IsNewBest(-1, 0));
            Assert.IsFalse(ChainNightGlowRules.IsNewBest(4, 4));
            Assert.IsFalse(ChainNightGlowRules.IsNewBest(4, 2));
        }

        // ---------------- 70 · the caldera's fires ----------------

        [Test]
        public void Caldera_WithoutTheGlutton_IsTheOldMeter()
        {
            foreach (float d in new[] { -5f, 0f, 30f, 89f, 90f, 200f })
            {
                foreach (bool tea in new[] { false, true })
                {
                    Assert.AreEqual(HinokuraHeatRules.Step(40f, d, false, tea, 0.5f), CalderaFireRules.Step(40f, d, 0f, false, tea, 0.5f), 1e-4f, $"d {d} tea {tea}");
                }
            }

            Assert.AreEqual(0f, CalderaFireRules.Step(90f, 0f, 5f, true, false, 1f), "a cold stone still zeroes");
        }

        [Test]
        public void Caldera_AtNight_TheLavaCreepsUp()
        {
            Assert.AreEqual(95f, CalderaFireRules.LakeDistance(95f, false));
            Assert.AreEqual(87f, CalderaFireRules.LakeDistance(95f, true));
            Assert.AreEqual(0f, CalderaFireRules.LakeGain(CalderaFireRules.LakeDistance(95f, false)));
            Assert.Greater(CalderaFireRules.LakeGain(CalderaFireRules.LakeDistance(95f, true)), 0f, "95 m out is warm at night");
        }

        [Test]
        public void Caldera_TheGluttonIsFire_ForgesCoolHim()
        {
            Assert.AreEqual(5f, CalderaFireRules.GluttonGain(1f, 0), 1e-4f);
            Assert.AreEqual(0f, CalderaFireRules.GluttonGain(CalderaFireRules.GluttonReach, 0), 1e-4f);
            Assert.AreEqual(1.25f, CalderaFireRules.GluttonGain(1f, 3), 1e-4f);
            Assert.AreEqual(1.25f, CalderaFireRules.GluttonGain(1f, 9), 1e-4f, "never past the three forges");
            Assert.Greater(CalderaFireRules.GluttonGain(10f, 0), CalderaFireRules.GluttonGain(20f, 0));

            // Next to him, far from the lake: the meter climbs instead of falling; 24 m out it falls.
            float hot = CalderaFireRules.Step(0f, 260f, CalderaFireRules.GluttonGain(3f, 0), false, false, 10f);
            Assert.Greater(hot, 40f);
            Assert.AreEqual(hot - HinokuraHeatRules.CoolPerSecond, CalderaFireRules.Step(hot, 260f, CalderaFireRules.GluttonGain(30f, 0), false, false, 1f), 1e-4f);
            Assert.AreEqual(RegionBossRules.EmberGluttonId, CalderaFireRules.GluttonEnemyId);
        }

        [Test]
        public void Caldera_GluttonArena_IsOutOfTheLakesHeat_SoOnlyHeHeatsTheFight()
        {
            float d = RioFireRules.DistanceToFire(OverworldLayouts.CaldeiraLake, OverworldLayouts.GlutaoArena);
            Assert.Greater(CalderaFireRules.LakeDistance(d, true), HinokuraHeatRules.HeatRange + CalderaFireRules.GluttonReach);
        }

        // ---------------- texts ----------------

        [Test]
        public void Texts_EveryKeyHasBothLanguages_AndALinePerCousin()
        {
            foreach (var entry in PeaksSteppeRound2Texts.Entries)
            {
                Assert.IsFalse(string.IsNullOrWhiteSpace(entry.En), entry.Key);
                Assert.IsFalse(string.IsNullOrWhiteSpace(entry.PtBR), entry.Key);
                Assert.IsFalse(entry.PtBR.Contains("Dona ") || entry.PtBR.Contains("Seu "), entry.Key);
            }

            string[] prefixes =
            {
                PeaksSteppeRound2Texts.BridgeLinePrefix, PeaksSteppeRound2Texts.BellLinePrefix, PeaksSteppeRound2Texts.DreamLinePrefix,
                PeaksSteppeRound2Texts.ChainLinePrefix, PeaksSteppeRound2Texts.GluttonLinePrefix,
            };
            var keys = new System.Collections.Generic.HashSet<string>(PeaksSteppeRound2Texts.Entries.Select(entry => entry.Key));
            foreach (string prefix in prefixes)
            {
                foreach (CharacterId hero in new[] { CharacterId.Maithe, CharacterId.Icaro, CharacterId.Joaquim, CharacterId.Maria, CharacterId.Lavine })
                {
                    Assert.IsTrue(keys.Contains(PeaksSteppeRound2Texts.LineFor(prefix, hero)), prefix + hero);
                }
            }

            Assert.AreEqual(keys.Count, PeaksSteppeRound2Texts.Entries.Count, "no duplicate keys");
            Assert.AreEqual("O sino parou: o paredão está mais quieto agora", PeaksSteppeRound2Texts.Get(null, PeaksSteppeRound2Texts.BellStilled));
        }
    }
}
