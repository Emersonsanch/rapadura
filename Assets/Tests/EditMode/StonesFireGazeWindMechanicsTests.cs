using NUnit.Framework;
using Rapadura.Gameplay.World.Map.Mechanics;
using Rapadura.Gameplay.World.Map.Overworld;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// Four more mechanics on the main road (Docs/Mapas100.md): the waking stones of the Campo das Pedras-Dragão (21, <see cref="VishapStoneRules"/>),
    /// the low fire of the Savana dos Auroques (26, <see cref="SavannaFireRules"/>), the calves' glow of the Charco do Ninki Nanka (34,
    /// <see cref="NinkiGazeRules"/>) and the wind vanes of the Forte dos Ventos (39, <see cref="WindVaneRules"/>): pure rules, and the places the
    /// layouts declare for them.
    /// </summary>
    public class StonesFireGazeWindMechanicsTests
    {
        // ------------------------------------------------------------------
        // 21 · Campo das Pedras-Dragão
        // ------------------------------------------------------------------

        [Test]
        public void Stones_TwelveOnTheField_AwayFromThePost()
        {
            Vector2[] stones = OverworldLayouts.VishapStones;
            Assert.AreEqual(VishapStoneRules.StoneCount, stones.Length);
            foreach (Vector2 stone in stones)
            {
                Assert.IsTrue(OverworldLayouts.DragonstoneFieldBounds.Contains(stone), $"stone {stone} on the field");
                Assert.Greater(Vector2.Distance(stone, OverworldLayouts.PedrasDragaoPost), 100f, "the ring round the outpost");
            }

            for (int i = 0; i < stones.Length; i++)
            {
                for (int j = i + 1; j < stones.Length; j++)
                {
                    Assert.Greater(Vector2.Distance(stones[i], stones[j]), 2f * VishapStoneRules.SpringRadius, "springs never overlap");
                }
            }

            Assert.AreEqual(3, VishapStoneRules.StoneAt(stones, stones[3] + new Vector2(3.9f, 0f)));
            Assert.AreEqual(-1, VishapStoneRules.StoneAt(stones, stones[3] + new Vector2(4.1f, 0f)));
            Assert.AreSame(VishapStoneRules.TouchKey(5), VishapStoneRules.TouchKey(5), "the save key is built once");
        }

        [Test]
        public void Stones_SpringHealsFor60s_WakesOnceADay_NeverABoss()
        {
            Assert.IsTrue(VishapStoneRules.Asleep(0, 1), "never touched");
            Assert.IsFalse(VishapStoneRules.Asleep(4, 4), "already today");
            Assert.IsTrue(VishapStoneRules.Asleep(4, 5));

            Assert.IsTrue(VishapStoneRules.Flowing(10f, 69.9f));
            Assert.IsFalse(VishapStoneRules.Flowing(10f, 70f));
            Assert.IsFalse(VishapStoneRules.Flowing(float.NegativeInfinity, 0f));

            Assert.AreEqual(2f, VishapStoneRules.HealAmount(8f, 100f, 1f), 1e-5f, "2 % per second");
            Assert.AreEqual(0f, VishapStoneRules.HealAmount(8.1f, 100f, 1f));

            Assert.IsTrue(VishapStoneRules.CanWake(false, 60f));
            Assert.IsFalse(VishapStoneRules.CanWake(false, 60.1f));
            Assert.IsFalse(VishapStoneRules.CanWake(true, 5f), "bosses sleep on");
        }

        // ------------------------------------------------------------------
        // 26 · Savana dos Auroques
        // ------------------------------------------------------------------

        [Test]
        public void Fire_StripsTakeTurns_TwoAlwaysAsh()
        {
            for (double t = 1_700_000_000d; t < 1_700_000_000d + 1500d; t += 100d)
            {
                int ash = 0, burning = 0;
                for (int i = 0; i < OverworldLayouts.SavannaFireStrips.Length; i++)
                {
                    SavannaStripState state = SavannaFireRules.StateOf(i, t);
                    ash += SavannaFireRules.IsAsh(state) ? 1 : 0;
                    burning += state == SavannaStripState.Burning ? 1 : 0;
                }

                Assert.AreEqual(2, ash, $"t={t}");
                Assert.AreEqual(1, burning, $"t={t}");
            }

            const double t0 = 1_700_000_100d; // a multiple of 300 s
            Assert.AreEqual(SavannaStripState.Burning, SavannaFireRules.StateOf(0, t0 - (SavannaFireRules.TurnIndex(t0) % 4) * 300d));
            Assert.AreEqual(SavannaFireRules.StateOf(1, t0), SavannaFireRules.StateOf(0, t0 + 300d), "the next strip is one turn ahead");
        }

        [Test]
        public void Fire_AshIsFast_BeastsShyFromAshAndFire_NotGrass()
        {
            Assert.AreEqual(1.2f, SavannaFireRules.HeroSpeedFactor(SavannaStripState.Ash, false), 1e-6f);
            Assert.AreEqual(1.2f, SavannaFireRules.HeroSpeedFactor(SavannaStripState.FreshAsh, false), 1e-6f);
            Assert.AreEqual(1f, SavannaFireRules.HeroSpeedFactor(SavannaStripState.Burning, false));
            Assert.AreEqual(1f, SavannaFireRules.HeroSpeedFactor(SavannaStripState.Ash, true), "in Aratta");

            Assert.IsTrue(SavannaFireRules.BeastShies(SavannaStripState.Burning, false));
            Assert.IsTrue(SavannaFireRules.BeastShies(SavannaStripState.Ash, false));
            Assert.IsFalse(SavannaFireRules.BeastShies(SavannaStripState.Grass, false));
            Assert.IsFalse(SavannaFireRules.BeastShies(SavannaStripState.Ash, true), "bosses");
        }

        [Test]
        public void Fire_StripsLieInTheSavanna_OffAratta()
        {
            foreach (Rect strip in OverworldLayouts.SavannaFireStrips)
            {
                Assert.IsTrue(OverworldLayouts.SavannaFireBounds.Contains(strip.min) && OverworldLayouts.SavannaFireBounds.Contains(strip.max), $"{strip}");
                Vector2 a = OverworldLayouts.ArattaOffset;
                Vector2 nearest = new Vector2(Mathf.Clamp(a.x, strip.xMin, strip.xMax), Mathf.Clamp(a.y, strip.yMin, strip.yMax));
                Assert.Greater(Vector2.Distance(nearest, a), OverworldLayouts.RaceCityGround + 20f, "Aratta's ground never burns");
            }

            Assert.AreEqual(1, SavannaFireRules.StripAt(OverworldLayouts.SavannaFireStrips, new Vector2(-480f, 900f)));
            Assert.AreEqual(-1, SavannaFireRules.StripAt(OverworldLayouts.SavannaFireStrips, OverworldLayouts.ArattaOffset));
        }

        // ------------------------------------------------------------------
        // 34 · Charco do Ninki Nanka
        // ------------------------------------------------------------------

        [Test]
        public void Gaze_EachCalfShinesOnItsOwnBeat()
        {
            Assert.AreEqual(NinkiGlowPhase.Dim, NinkiGazeRules.PhaseAt(0d, 0f));
            Assert.AreEqual(NinkiGlowPhase.Warning, NinkiGazeRules.PhaseAt(10.3d, 0f));
            Assert.AreEqual(NinkiGlowPhase.Glowing, NinkiGazeRules.PhaseAt(11.5d, 0f));
            Assert.AreEqual(NinkiGlowPhase.Dim, NinkiGazeRules.PhaseAt(12d, 0f));
            Assert.AreEqual(NinkiGlowPhase.Glowing, NinkiGazeRules.PhaseAt(1d, 10.5f), "an offset of 10.5 s");
            Assert.AreEqual(NinkiGazeRules.GlowIndex(11d, 0f) + 1L, NinkiGazeRules.GlowIndex(23d, 0f));

            for (int seed = -50; seed < 50; seed++)
            {
                float offset = NinkiGazeRules.Offset(seed);
                Assert.That(offset, Is.InRange(0f, NinkiGazeRules.CycleSeconds));
            }

            Assert.AreNotEqual(NinkiGazeRules.Offset(1), NinkiGazeRules.Offset(2), "two calves, two beats");
        }

        [Test]
        public void Gaze_FacingWithin60DegreesAnd14m_SlowsByForty()
        {
            Vector2 hero = Vector2.zero;
            Assert.IsTrue(NinkiGazeRules.Facing(hero, Vector2.up, new Vector2(0f, 10f)));
            Assert.IsTrue(NinkiGazeRules.Facing(hero, Vector2.up, new Vector2(8f, 5f)), "within 60°");
            Assert.IsFalse(NinkiGazeRules.Facing(hero, Vector2.up, new Vector2(10f, 3f)), "from the side");
            Assert.IsFalse(NinkiGazeRules.Facing(hero, Vector2.down, new Vector2(0f, 10f)), "back turned");
            Assert.IsFalse(NinkiGazeRules.Facing(hero, Vector2.up, new Vector2(0f, 14.1f)), "too far");
            Assert.IsFalse(NinkiGazeRules.Facing(hero, Vector2.up, new Vector2(0f, 1f)), "pressed against it");
            Assert.AreEqual(-0.4f, NinkiGazeRules.SlowBonus, 1e-6f);
            Assert.IsTrue(OverworldLayouts.NinkiBogBounds.Contains(OverworldLayouts.IlhotaLanternasOffset));
        }

        // ------------------------------------------------------------------
        // 39 · Forte dos Ventos
        // ------------------------------------------------------------------

        [Test]
        public void Wind_TheVanesStartCancelled_AlignedTheyBlowFull()
        {
            var states = new int[WindVaneRules.VaneCount];
            for (int i = 0; i < states.Length; i++)
            {
                states[i] = WindVaneRules.StateFrom(0L, i);
            }

            Assert.AreEqual(0f, WindVaneRules.Wind(states).magnitude, 1e-5f, "one each way: no breeze");

            states[1] = WindVaneRules.Turn(WindVaneRules.Turn(WindVaneRules.Turn(states[1])));
            states[2] = WindVaneRules.Turn(WindVaneRules.Turn(states[2]));
            states[3] = WindVaneRules.Turn(states[3]);
            Assert.AreEqual(Vector2.up, WindVaneRules.Wind(states), "all four north");

            Assert.AreEqual(2, WindVaneRules.StateFrom(WindVaneRules.MarkOf(2), 0), "the save round-trips");
            Assert.AreEqual(0, WindVaneRules.Turn(3));
        }

        [Test]
        public void Wind_WithItFaster_AgainstSlower_AcrossNothing()
        {
            Vector2 wind = Vector2.up;
            Assert.AreEqual(1.2f, WindVaneRules.SpeedFactor(new Vector2(0f, 5f), wind), 1e-5f);
            Assert.AreEqual(0.8f, WindVaneRules.SpeedFactor(new Vector2(0f, -5f), wind), 1e-5f);
            Assert.AreEqual(1f, WindVaneRules.SpeedFactor(new Vector2(5f, 0f), wind), "across");
            Assert.AreEqual(1.1f, WindVaneRules.SpeedFactor(new Vector2(0f, 5f), wind * 0.5f), 1e-5f, "half a wind, half the help");
            Assert.AreEqual(1f, WindVaneRules.SpeedFactor(Vector2.zero, wind), "standing");
        }

        [Test]
        public void Wind_VanesStandRoundTheFort()
        {
            foreach (Vector2 vane in OverworldLayouts.WindVanes)
            {
                Assert.IsTrue(OverworldLayouts.WindFortBounds.Contains(vane), $"vane {vane}");
                Assert.Greater(Vector2.Distance(vane, OverworldLayouts.ForteVentosOffset), 30f, "outside the fort's yard");
            }

            Assert.AreEqual(0, WindVaneRules.VaneAt(OverworldLayouts.WindVanes, OverworldLayouts.WindVanes[0] + new Vector2(0f, 3.9f)));
            Assert.AreEqual(-1, WindVaneRules.VaneAt(OverworldLayouts.WindVanes, OverworldLayouts.ForteVentosOffset));
        }
    }
}
