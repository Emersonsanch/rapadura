using NUnit.Framework;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.World.Map.Mechanics;
using Rapadura.Gameplay.World.Map.Overworld;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// The Falta items of maps 37–53 done on 2026-11-16: the freed Runaway Cart (37), the Great Almas guiding (38), the shadow clock (42), the red
    /// lanterns and the drum of Nian (45), the old griffin's presents (48) and the gilding washed off in the Pactolo (53).
    /// </summary>
    public class MapBossAftermathMechanicsTests
    {
        // ---------------- 37 · the freed Runaway Cart ----------------

        [Test]
        public void Cart_Freed_IsTheMapBoss_AndRunsFaster()
        {
            Assert.AreEqual(RegionBossRules.RunawayCartId, TetoCartRules.RunawayCartEnemyId);
            Assert.AreEqual(TetoCartRules.Speed(false), TetoCartRules.Speed(false, false), 1e-4f);
            Assert.AreEqual(TetoCartRules.DaySpeed * TetoCartRules.FreedSpeedFactor, TetoCartRules.Speed(false, true), 1e-4f);
            Assert.AreEqual(TetoCartRules.NightSpeed * TetoCartRules.FreedSpeedFactor, TetoCartRules.Speed(true, true), 1e-4f);
            Assert.Less(TetoCartRules.RunSeconds(813f, false, true), TetoCartRules.RunSeconds(813f, false), "the fastest cart of the chutes");
            Assert.Greater(TetoCartRules.RunSeconds(813f, true, true), 30f, "still a run, not a jump");
        }

        // ---------------- 38 · the Great Almas guiding ----------------

        [Test]
        public void Glacier_FreedAlmas_SlowsTheCold_AndStacksWithTeaAndNight()
        {
            Assert.AreEqual(RegionBossRules.GreatAlmasId, GlacierColdRules.GreatAlmasEnemyId);
            float plain = GlacierColdRules.Step(0f, true, false, false, false, false, 10f);
            float guided = GlacierColdRules.Step(0f, true, false, false, false, true, 10f);
            Assert.AreEqual(10f, plain, 1e-4f);
            Assert.AreEqual(10f * GlacierColdRules.GuideFactor, guided, 1e-4f);
            Assert.AreEqual(10f * GlacierColdRules.NightFactor * GlacierColdRules.TeaFactor * GlacierColdRules.GuideFactor,
                GlacierColdRules.Step(0f, true, false, true, true, true, 10f), 1e-4f);
            Assert.AreEqual(plain, GlacierColdRules.Step(0f, true, false, false, false, 10f), 1e-4f, "the old overload is unguided");
            Assert.AreEqual(0f, GlacierColdRules.Step(80f, true, true, true, false, true, 1f), "a fire still zeroes it");
        }

        // ---------------- 42 · the shadow clock ----------------

        [Test]
        public void ShadowClock_FreedEclipse_ReadsTheHour_AndTheNorthMirror()
        {
            Assert.AreEqual(RegionBossRules.LittleEclipseId, ShadowClockRules.EclipseEnemyId);
            Assert.IsTrue(OverworldLayouts.MirrorClockBounds.Contains(ShadowClockRules.Dial), "in the white city");
            Assert.IsFalse(ShadowClockRules.HasShadow(3f));
            Assert.IsFalse(ShadowClockRules.HasShadow(20f));
            Assert.AreEqual(Vector2.zero, ShadowClockRules.ShadowDirection(22f));

            Assert.Less(ShadowClockRules.ShadowDirection(6f).x, -0.99f, "sun in the east: the shadow points west");
            Assert.Greater(ShadowClockRules.ShadowDirection(12f).y, 0.99f, "sun in the south at noon: the shadow points north");
            Assert.Greater(ShadowClockRules.ShadowDirection(17.9f).x, 0.95f, "sun in the west: the shadow points east");

            Assert.AreEqual(6, ShadowClockRules.HoursToNextBand(0f));
            Assert.AreEqual(1, ShadowClockRules.HoursToNextBand(11.5f));
            for (float h = 0f; h < 24f; h += 1.5f)
            {
                int north = ShadowClockRules.FirstMirrorState(h);
                Assert.AreEqual(MirrorClockRules.RightState(0, MirrorClockRules.BandOf(h)), north);
                var states = new[] { north, 0, 0, 0 };
                Assert.GreaterOrEqual(MirrorClockRules.LitChain(states, h), 1, "facing the north mirror as read lights it");
            }

            Assert.IsTrue(ShadowClockRules.AtDial(ShadowClockRules.Dial, ShadowClockRules.Dial + Vector2.right * (ShadowClockRules.Reach - 0.1f)));
            Assert.IsFalse(ShadowClockRules.AtDial(ShadowClockRules.Dial, ShadowClockRules.Dial + Vector2.right * (ShadowClockRules.Reach + 0.1f)));
        }

        // ---------------- 45 · the red lanterns and the drum of Nian ----------------

        [Test]
        public void Nian_LanternsAndDrum_StandRoundTheRing()
        {
            Vector2 arena = OverworldLayouts.NianArena;
            for (int i = 0; i < NianScareRules.LanternCount; i++)
            {
                Vector2 lantern = NianScareRules.LanternAt(arena, i);
                Assert.AreEqual(NianScareRules.RingRadius, Vector2.Distance(arena, lantern), 1e-3f);
                Assert.AreEqual(i, NianScareRules.LanternNear(arena, lantern + Vector2.one * 0.5f));
                Assert.Greater(Vector2.Distance(lantern, NianScareRules.DrumAt(arena)), NianScareRules.Reach * 2f, "the drum never overlaps a lantern");
            }

            Assert.AreEqual(-1, NianScareRules.LanternNear(arena, arena), "the centre of the ring is far from all of them");
            Assert.IsTrue(NianScareRules.AtDrum(arena, NianScareRules.DrumAt(arena)));
            Assert.IsFalse(NianScareRules.AtDrum(arena, arena));
        }

        [Test]
        public void Nian_DrumInTheWarning_AndLitLanternByTheMark_Frighten()
        {
            Assert.IsTrue(NianScareRules.DrumScares(10.5d, 10d, 12.4d), "beat in the crouch");
            Assert.IsFalse(NianScareRules.DrumScares(9.9d, 10d, 12.4d), "too early");
            Assert.IsFalse(NianScareRules.DrumScares(double.NegativeInfinity, 10d, 12.4d), "never beaten");
            Assert.IsTrue(NianScareRules.CanDrum(0d, NianScareRules.DrumCooldownSeconds));
            Assert.IsFalse(NianScareRules.CanDrum(0d, NianScareRules.DrumCooldownSeconds - 0.1d));

            var lanterns = new[] { Vector2.zero, new Vector2(20f, 0f), new Vector2(40f, 0f) };
            var lit = new[] { 50d, 0d, 50d };
            Assert.AreEqual(0, NianScareRules.ShelteringLantern(lanterns, lit, 10d, new Vector2(0f, NianScareRules.ShelterRadius - 0.1f)));
            Assert.AreEqual(-1, NianScareRules.ShelteringLantern(lanterns, lit, 10d, new Vector2(20f, 1f)), "that one is dark");
            Assert.AreEqual(-1, NianScareRules.ShelteringLantern(lanterns, lit, 60d, Vector2.zero), "burnt out");
            Assert.AreEqual(-1, NianScareRules.ShelteringLantern(lanterns, lit, 10d, new Vector2(0f, NianScareRules.ShelterRadius + 0.1f)));
            Assert.IsTrue(NianScareRules.Frightened(false, false, true));
            Assert.IsFalse(NianScareRules.Frightened(false, false, false));

            Assert.IsTrue(NianScareRules.CanLight(0d, 0d, 1d));
            Assert.IsFalse(NianScareRules.CanLight(50d, 0d, 1d), "already burning");
            Assert.IsFalse(NianScareRules.CanLight(0d, 30d, 20d), "the wick still smokes");
        }

        [Test]
        public void Nian_Board_SpendsTheLanternThatSaved()
        {
            NianScareBoard.Reset();
            try
            {
                Vector2 arena = new Vector2(100f, 100f);
                NianScareBoard.Place(arena);
                Vector2 lantern = NianScareRules.LanternAt(arena, 0);
                Assert.IsFalse(NianScareBoard.Scares(lantern, 0d, 1d), "nothing lit, no drum");
                Assert.IsTrue(NianScareBoard.Light(0, 1d));
                Assert.IsTrue(NianScareBoard.Scares(lantern, 5d, 7.4d));
                Assert.AreEqual(1, NianScareBoard.Frights);
                Assert.IsFalse(NianScareBoard.LastByDrum);
                Assert.IsFalse(NianScareBoard.Scares(lantern, 15d, 17.4d), "the beast swatted it out");
                Assert.IsFalse(NianScareBoard.Light(0, 18d), "cooling");
                Assert.IsTrue(NianScareBoard.Light(0, 7.4d + NianScareRules.CoolSeconds));

                Assert.IsTrue(NianScareBoard.Beat(30d));
                Assert.IsFalse(NianScareBoard.Beat(31d), "the drumskin still shakes");
                Assert.IsTrue(NianScareBoard.Scares(arena, 29.5d, 31.9d));
                Assert.IsTrue(NianScareBoard.LastByDrum);
                Assert.AreEqual(2, NianScareBoard.Frights);
            }
            finally
            {
                NianScareBoard.Reset();
            }
        }

        // ---------------- 48 · the old griffin's presents ----------------

        [Test]
        public void Griffin_EachCousin_GetsTheirOwnPresent_Once()
        {
            Assert.AreEqual(RegionBossRules.OldKurganGriffinId, GriffinGiftRules.GriffinEnemyId);
            Assert.AreEqual(GriffinGiftCheck.Guarded, GriffinGiftRules.Check(false, false));
            Assert.AreEqual(GriffinGiftCheck.Take, GriffinGiftRules.Check(true, false));
            Assert.AreEqual(GriffinGiftCheck.Taken, GriffinGiftRules.Check(true, true));

            var cousins = new[] { CharacterId.Maithe, CharacterId.Icaro, CharacterId.Joaquim, CharacterId.Maria, CharacterId.Lavine };
            var items = new System.Collections.Generic.HashSet<string>();
            var keys = new System.Collections.Generic.HashSet<string>();
            foreach (CharacterId cousin in cousins)
            {
                GriffinGift gift = GriffinGiftRules.GiftFor(cousin);
                Assert.IsNotNull(ItemDatabase.GetById(gift.ItemId), gift.ItemId);
                Assert.Greater(gift.Quantity, 0);
                Assert.IsTrue(items.Add(gift.ItemId), "one present each");
                Assert.IsTrue(keys.Add(GriffinGiftRules.TakenKey(cousin)), "one mark each");
                Assert.AreNotEqual(GriffinGiftTexts.LineFor(cousin), GriffinGiftTexts.Get(null, GriffinGiftTexts.LineFor(cousin)), "a line for " + cousin);
            }

            Vector2 hoard = OverworldLayouts.GrifoVelhoArena;
            Assert.IsTrue(GriffinGiftRules.AtHoard(hoard, hoard + Vector2.up * (GriffinGiftRules.Reach - 0.1f)));
            Assert.IsFalse(GriffinGiftRules.AtHoard(hoard, hoard + Vector2.up * (GriffinGiftRules.Reach + 0.1f)));
        }

        // ---------------- 53 · the gilding, washed off in the Pactolo ----------------

        [Test]
        public void Gild_BeesAndMidas_AddLayers_ThatSlow()
        {
            Assert.AreEqual(RegionBossRules.MidasGoldId, PactoloGildRules.MidasGoldEnemyId);
            Assert.IsNotNull(Resources.Load<EnemyDefinition>("Enemies/" + PactoloGildRules.GoldenBeeEnemyId), "the Golden Bee exists");
            Assert.AreEqual(1, PactoloGildRules.LayersFrom(PactoloGildRules.GoldenBeeEnemyId));
            Assert.AreEqual(0, PactoloGildRules.LayersFrom(PactoloGildRules.MidasGoldEnemyId), "Midas gilds only with the touch, not every shot");
            Assert.AreEqual(2, PactoloGildRules.MidasLayers);
            int before = GoldenTouchSignal.Count;
            GoldenTouchSignal.Landed();
            Assert.AreEqual(before + 1, GoldenTouchSignal.Count);
            Assert.AreEqual(0, PactoloGildRules.LayersFrom("enemy_forest_vine_fox"));
            Assert.AreEqual(0, PactoloGildRules.LayersFrom(null));

            Assert.AreEqual(PactoloGildRules.MaxLayers, PactoloGildRules.Add(4, 2), "never past the cap");
            Assert.AreEqual(1f, PactoloGildRules.SpeedFactor(0), 1e-4f);
            Assert.AreEqual(0.7f, PactoloGildRules.SpeedFactor(PactoloGildRules.MaxLayers), 1e-4f);
            Assert.AreEqual(0.7f, PactoloGildRules.SpeedFactor(99), 1e-4f);
        }

        [Test]
        public void Gild_FlakesSlowly_AndThePactoloWashesItAll()
        {
            float since = 0f;
            Assert.AreEqual(3, PactoloGildRules.Tick(3, false, ref since, PactoloGildRules.FlakeSeconds - 1f));
            Assert.AreEqual(2, PactoloGildRules.Tick(3, false, ref since, 1f), "one layer flakes off");
            Assert.AreEqual(0f, since, 1e-4f);
            Assert.AreEqual(0, PactoloGildRules.Tick(5, true, ref since, 0.01f), "Living Water: all off at once");

            Vector2[] river = OverworldLayouts.PactoloRiver;
            Vector2 mid = (river[4] + river[5]) * 0.5f;
            Assert.IsTrue(PactoloGildRules.InRiver(river, mid));
            Assert.IsFalse(PactoloGildRules.InRiver(river, mid + Vector2.right * 40f));
            Assert.IsFalse(PactoloGildRules.InRiver(null, mid));
        }
    }
}
