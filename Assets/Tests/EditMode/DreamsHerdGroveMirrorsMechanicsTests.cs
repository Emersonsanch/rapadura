using NUnit.Framework;
using Rapadura.Gameplay.World.Map.Mechanics;
using Rapadura.Gameplay.World.Map.Overworld;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// Four more mechanics on the main road (Docs/Mapas100.md): the mirror clock of the Torres Brancas (42, <see cref="MirrorClockRules"/>), the
    /// dream chests of the Fortim (49, <see cref="NartesDreamRules"/>), the slow clock of the Arvoredo (50, <see cref="GroveSlowTimeRules"/>) and
    /// the herding of the Pradaria dos Mil Cavalos (51, <see cref="PrairieHerdRules"/>): pure rules, and the places the layouts declare for them.
    /// </summary>
    public class DreamsHerdGroveMirrorsMechanicsTests
    {
        // ------------------------------------------------------------------
        // 42 · Torres Brancas
        // ------------------------------------------------------------------

        [Test]
        public void Mirrors_TheRightWayChangesEverySixHours()
        {
            Assert.AreEqual(0, MirrorClockRules.BandOf(0f));
            Assert.AreEqual(1, MirrorClockRules.BandOf(6f));
            Assert.AreEqual(2, MirrorClockRules.BandOf(12.5f));
            Assert.AreEqual(3, MirrorClockRules.BandOf(23.9f));
            Assert.AreEqual(0, MirrorClockRules.BandOf(24f));

            var right = new int[4];
            for (int i = 0; i < 4; i++)
            {
                right[i] = MirrorClockRules.RightState(i, 2);
            }

            Assert.IsTrue(MirrorClockRules.Solved(right, 13f));
            Assert.IsFalse(MirrorClockRules.Solved(right, 19f), "six hours later the light goes elsewhere");
            for (float hour = 0f; hour < 24f; hour += 1f)
            {
                Assert.IsFalse(MirrorClockRules.Solved(new[] { 0, 0, 0, 0 }, hour), $"the mirrors as they stand are never right ({hour} h)");
            }
        }

        [Test]
        public void Mirrors_TheLightGoesOnInOrder_TheVaultOncePerDay()
        {
            int band = MirrorClockRules.BandOf(9f);
            var states = new[] { MirrorClockRules.RightState(0, band), MirrorClockRules.RightState(1, band), 0, 0 };
            states[2] = MirrorClockRules.Turn(MirrorClockRules.RightState(2, band));
            Assert.AreEqual(2, MirrorClockRules.LitChain(states, 9f));
            states[0] = MirrorClockRules.Turn(states[0]);
            Assert.AreEqual(0, MirrorClockRules.LitChain(states, 9f), "a wrong first mirror stops the light at once");

            Assert.IsTrue(MirrorClockRules.VaultOpens(true, 0, 1));
            Assert.IsFalse(MirrorClockRules.VaultOpens(true, 3, 3), "taken today");
            Assert.IsFalse(MirrorClockRules.VaultOpens(false, 0, 1));
        }

        [Test]
        public void Mirrors_StandRoundTheWhiteCity()
        {
            foreach (Vector2 mirror in OverworldLayouts.MirrorClockMirrors)
            {
                Assert.IsTrue(OverworldLayouts.MirrorClockBounds.Contains(mirror), $"{mirror}");
                Assert.Greater(Vector2.Distance(mirror, OverworldLayouts.TorresBrancasOffset), OverworldLayouts.RaceCityGround + 10f, "outside the town ground");
            }

            Assert.AreEqual(1, MirrorClockRules.MirrorAt(OverworldLayouts.MirrorClockMirrors, OverworldLayouts.MirrorClockMirrors[1] + new Vector2(3f, 0f)));
        }

        // ------------------------------------------------------------------
        // 49 · Fortim dos Gigantes-da-Colina
        // ------------------------------------------------------------------

        [Test]
        public void Dreams_OneHidingPlaceADay_AllOfThemUsed()
        {
            int count = OverworldLayouts.FortimDreamSpots.Length;
            var seen = new bool[count];
            for (long day = 20000; day < 20200; day++)
            {
                int spot = NartesDreamRules.SpotOf(day, count);
                Assert.That(spot, Is.InRange(0, count - 1));
                Assert.AreEqual(spot, NartesDreamRules.SpotOf(day, count), "the same for everyone");
                seen[spot] = true;
            }

            foreach (bool used in seen)
            {
                Assert.IsTrue(used, "every hiding place comes up");
            }

            Assert.AreEqual(-1, NartesDreamRules.SpotOf(5, 0));
        }

        [Test]
        public void Dreams_TheChestOpensOnlyAfterTheDream_OnceADay()
        {
            Assert.IsTrue(NartesDreamRules.Open(10, 9, 10));
            Assert.IsFalse(NartesDreamRules.Open(9, 0, 10), "not heard today");
            Assert.IsFalse(NartesDreamRules.Open(10, 10, 10), "already opened");

            Assert.AreEqual(0, NartesDreamRules.CompassOf(Vector2.zero, new Vector2(0f, 10f)), "north");
            Assert.AreEqual(2, NartesDreamRules.CompassOf(Vector2.zero, new Vector2(10f, 0f)), "east");
            Assert.AreEqual(5, NartesDreamRules.CompassOf(Vector2.zero, new Vector2(-10f, -10f)), "south-west");
            Assert.AreEqual(300, NartesDreamRules.ToldDistance(Vector2.zero, new Vector2(0f, 290f)));
            Assert.AreEqual(50, NartesDreamRules.ToldDistance(Vector2.zero, new Vector2(0f, 5f)));
        }

        [Test]
        public void Dreams_TheHidingPlacesLieInTheMap_AwayFromTheFort()
        {
            foreach (Vector2 spot in OverworldLayouts.FortimDreamSpots)
            {
                bool inMap = false;
                foreach (Rect area in OverworldLayouts.FortimDreamAreas)
                {
                    inMap |= area.Contains(spot);
                }

                Assert.IsTrue(inMap, $"{spot} in the map");
                Assert.Greater(Vector2.Distance(spot, OverworldLayouts.FortimOffset), 100f, "a walk from the fort");
            }

            Assert.IsTrue(OverworldLayouts.FortimDreamAreas[1].Contains(OverworldLayouts.FortimOffset), "the fort is where Virogdad tells it");
        }

        // ------------------------------------------------------------------
        // 50 · Arvoredo de Haripushpa
        // ------------------------------------------------------------------

        [Test]
        public void Grove_HalvesTheClock_PutsItBack()
        {
            Assert.AreEqual(0.5f, GroveSlowTimeRules.SlowedScale(1f), 1e-6f);
            Assert.AreEqual(1f, GroveSlowTimeRules.RestoredScale(0.5f, 0.5f, 1f), 1e-6f);
            Assert.AreEqual(2f, GroveSlowTimeRules.RestoredScale(2f, 0.5f, 1f), 1e-6f, "someone else changed it: keep that");
            Assert.IsTrue(OverworldLayouts.SlowGroveBounds.Contains(OverworldLayouts.ArvoredoOffset), "the tree-folk's town is in the grove");
        }

        // ------------------------------------------------------------------
        // 51 · Pradaria dos Mil Cavalos
        // ------------------------------------------------------------------

        [Test]
        public void Herd_HorsesGrazeInCircles_FollowInALine()
        {
            Vector2 spot = new Vector2(100f, 100f);
            for (double t = 0d; t < 240d; t += 7d)
            {
                Assert.AreEqual(PrairieHerdRules.GrazeRadius, Vector2.Distance(spot, PrairieHerdRules.GrazingAt(spot, 2, t)), 1e-3f);
            }

            Assert.AreNotEqual(PrairieHerdRules.GrazingAt(spot, 0, 0d), PrairieHerdRules.GrazingAt(spot, 1, 0d), "each horse on its own point");
            Assert.AreEqual(new Vector2(0f, -3f), PrairieHerdRules.FollowAt(Vector2.zero, Vector2.up, 0));
            Assert.AreEqual(new Vector2(0f, -9f), PrairieHerdRules.FollowAt(Vector2.zero, Vector2.up, 2));

            Assert.IsTrue(PrairieHerdRules.CanLead(2));
            Assert.IsFalse(PrairieHerdRules.CanLead(3), "three at most");
            Assert.IsTrue(PrairieHerdRules.Free(9, 10));
            Assert.IsFalse(PrairieHerdRules.Free(10, 10), "once per day");
            Assert.AreSame(PrairieHerdRules.DayKey(1), PrairieHerdRules.DayKey(1));
        }

        [Test]
        public void Herd_TheHorsesGrazeOnThePrairie_OutsideTheCorral()
        {
            Assert.AreEqual(PrairieHerdRules.HorseCount, OverworldLayouts.PrairieHorseSpots.Length);
            foreach (Vector2 spot in OverworldLayouts.PrairieHorseSpots)
            {
                Assert.IsTrue(OverworldLayouts.PrairieHerdBounds.Contains(spot), $"{spot}");
                Assert.IsFalse(PrairieHerdRules.InCorral(OverworldLayouts.PocoDaTacaOffset, spot), "a horse must be led");
            }

            Assert.IsTrue(PrairieHerdRules.InCorral(OverworldLayouts.PocoDaTacaOffset, OverworldLayouts.PocoDaTacaOffset + new Vector2(21f, 0f)));
            Assert.IsTrue(OverworldLayouts.PrairieHerdBounds.Contains(OverworldLayouts.PocoDaTacaOffset));
        }
    }
}
