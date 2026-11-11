using System.Collections.Generic;
using NUnit.Framework;
using Rapadura.Gameplay.World.Map;
using Rapadura.Gameplay.World.Map.Areas;
using Rapadura.Gameplay.World.Map.Mechanics;
using Rapadura.Gameplay.World.Map.Overworld;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// The ways of the pits of the Limiar Profundo (Docs/Mapas100.md 86, 87, 89): the rising currents (<see cref="ProfundoUpdraftRules"/>), the
    /// sluices of the Cova do Lodo (<see cref="ProfundoSluiceRules"/>), the flights of the Ponte do Novelo (<see cref="NoveloBridgeRules"/>), the
    /// polyline the rides follow (<see cref="RidePath"/>), the save marks and the points the layout declares. Pure rules and layout data only.
    /// </summary>
    public class ProfundoPathsMechanicsTests
    {
        private static OverworldLayout World => OverworldLayouts.Get(OverworldLayouts.ProfundoRegionId);

        private static string MapOf(Vector2 p)
        {
            OverworldZone zone = OverworldRules.ZoneAt(World, p);
            return zone == null ? null : zone.MapId ?? zone.OwnerMapId;
        }

        // ------------------------------------------------------------------
        // RidePath
        // ------------------------------------------------------------------

        [Test]
        public void RidePath_WalksThePolylineByDistance()
        {
            Vector2[] points = { new Vector2(0f, 0f), new Vector2(10f, 0f), new Vector2(10f, 20f) };
            Assert.AreEqual(30f, RidePath.Length(points), 1e-4f);
            Assert.AreEqual(new Vector2(5f, 0f), RidePath.PointAt(points, 5f));
            Assert.AreEqual(new Vector2(10f, 5f), RidePath.PointAt(points, 15f));
            Assert.AreEqual(new Vector2(10f, 20f), RidePath.PointAt(points, 99f), "clamped to the far end");
            Assert.AreEqual(new Vector2(0f, 0f), RidePath.PointAt(points, -3f), "clamped to the start");
            Assert.AreEqual(Vector2.right, RidePath.DirectionAt(points, 2f));
            Assert.AreEqual(Vector2.up, RidePath.DirectionAt(points, 25f));
            Vector2[] back = RidePath.ToWorld(points, new Vector2(100f, 0f), true);
            Assert.AreEqual(new Vector2(110f, 20f), back[0]);
            Assert.AreEqual(new Vector2(100f, 0f), back[2]);
        }

        // ------------------------------------------------------------------
        // 86 · the rising currents
        // ------------------------------------------------------------------

        [Test]
        public void Updraft_Check_TideFirst_ThenTheZone_ThenTheRest()
        {
            Assert.AreEqual(ProfundoUpdraftCheck.TideShut, ProfundoUpdraftRules.Check(true, true, 99f));
            Assert.AreEqual(ProfundoUpdraftCheck.ZoneBlocked, ProfundoUpdraftRules.Check(false, false, 99f));
            Assert.AreEqual(ProfundoUpdraftCheck.Resting, ProfundoUpdraftRules.Check(false, true, 1f));
            Assert.AreEqual(ProfundoUpdraftCheck.Ok, ProfundoUpdraftRules.Check(false, true, ProfundoUpdraftRules.CooldownSeconds));
        }

        [Test]
        public void Updraft_Pecks_EveryThreeSeconds_TwoPercent_ThreeAtNight_NeverUnderThirty()
        {
            Assert.AreEqual(0, ProfundoUpdraftRules.PecksDue(2.9f));
            Assert.AreEqual(1, ProfundoUpdraftRules.PecksDue(3f));
            Assert.AreEqual(5, ProfundoUpdraftRules.PecksDue(15.5f));
            Assert.AreEqual(2f, ProfundoUpdraftRules.PeckAmount(100f, 100f, false), 1e-4f);
            Assert.AreEqual(3f, ProfundoUpdraftRules.PeckAmount(100f, 100f, true), 1e-4f);
            Assert.AreEqual(1f, ProfundoUpdraftRules.PeckAmount(31f, 100f, false), 1e-4f);
            Assert.AreEqual(0f, ProfundoUpdraftRules.PeckAmount(30f, 100f, true));
            Assert.AreEqual(60f, ProfundoUpdraftRules.GustAmount(1000f, false), 1e-3f);
            Assert.AreEqual(10f, ProfundoUpdraftRules.GustAmount(1000f, true), 1e-3f);
        }

        [Test]
        public void Updraft_Layout_FourVentsByTheLamp_LandingsOnTheOuterRing_InThePitOfWind()
        {
            ProfundoUpdraft[] updrafts = OverworldLayouts.ProfundoUpdrafts;
            Assert.AreEqual(4, updrafts.Length);
            var ids = new HashSet<string>();
            foreach (ProfundoUpdraft u in updrafts)
            {
                Assert.IsTrue(ids.Add(u.Id), u.Id);
                float fromLamp = Vector2.Distance(u.Vent, OverworldLayouts.LampadaVentoOffset);
                Assert.Greater(fromLamp, 18f, u.Id + " stands on the post's flat ground");
                Assert.Less(fromLamp, 40f, u.Id + " is far from the Lâmpada 3");
                Assert.AreEqual(u.Id, updrafts[ProfundoUpdraftRules.VentAt(updrafts, u.Vent)].Id);
                Assert.AreEqual(OverworldLayouts.CovaVentoMapId, MapOf(u.Landing), u.Id + " lands outside the Cova do Vento");
                Assert.IsTrue(ProfundoPathsMechanicsDriver.PartBounds[0].Contains(u.Vent), u.Id);
                Assert.IsTrue(ProfundoPathsMechanicsDriver.PartBounds[0].Contains(u.Landing), u.Id);
                float seconds = ProfundoUpdraftRules.RideSeconds(u);
                Assert.That(seconds, Is.InRange(12f, 17f), u.Id + " ride time");
                Assert.IsFalse(MapLayoutRules.InRiver(World.World, u.Landing, 2f), u.Id + " lands in the water");
            }

            Assert.AreEqual(-1, ProfundoUpdraftRules.VentAt(updrafts, OverworldLayouts.LampadaVentoOffset), "the lamp itself is no vent");
        }

        // ------------------------------------------------------------------
        // 87 · the sluices
        // ------------------------------------------------------------------

        [Test]
        public void Sluice_Bites_AfterTheBubble_EveryOneAndAHalfSeconds()
        {
            Assert.AreEqual(0, ProfundoSluiceRules.BitesDue(3.9f));
            Assert.AreEqual(1, ProfundoSluiceRules.BitesDue(4f));
            Assert.AreEqual(3, ProfundoSluiceRules.BitesDue(7.1f));
            Assert.AreEqual(2f, ProfundoSluiceRules.BiteAmount(100f, 100f, false), 1e-4f);
            Assert.AreEqual(3f, ProfundoSluiceRules.BiteAmount(100f, 100f, true), 1e-4f);
            Assert.AreEqual(0f, ProfundoSluiceRules.BiteAmount(29f, 100f, false));
            Assert.AreEqual(-0.5f, ProfundoSluiceRules.MoveBonus, 1e-4f);
            Assert.AreEqual(60f, ProfundoSluiceRules.RushAmount(1000f, false), 1e-3f);
            Assert.AreEqual(10f, ProfundoSluiceRules.RushAmount(1000f, true), 1e-3f);
        }

        [Test]
        public void Sluice_DrainedForTheDay_SavedAsADailyMark()
        {
            Assert.IsFalse(ProfundoSluiceRules.DrainedToday(0, 1));
            Assert.IsTrue(ProfundoSluiceRules.DrainedToday(4, 4));
            Assert.IsFalse(ProfundoSluiceRules.DrainedToday(4, 5), "the water rises again the next day");

            var progress = new WorldMapProgress();
            Assert.IsFalse(ProfundoPathsMechanicsDriver.DrainedIn(progress, 0, 3));
            progress.SetDailyMark(ProfundoSluiceRules.MarkKey(OverworldLayouts.ProfundoSluices[0].Id), 3);
            Assert.IsTrue(ProfundoPathsMechanicsDriver.DrainedIn(progress, 0, 3));
            Assert.IsFalse(ProfundoPathsMechanicsDriver.DrainedIn(progress, 1, 3));
            Assert.IsFalse(ProfundoPathsMechanicsDriver.DrainedIn(progress, 0, 4));
            Assert.IsFalse(ProfundoPathsMechanicsDriver.DrainedIn(null, 0, 3));
        }

        [Test]
        public void Sluice_Layout_TwoStretchesAcrossTheColumn_LeversOutOfTheWater()
        {
            ProfundoSluice[] sluices = OverworldLayouts.ProfundoSluices;
            Assert.AreEqual(2, sluices.Length);
            Rect column = ProfundoPathsMechanicsDriver.PartBounds[1];
            for (int i = 0; i < sluices.Length; i++)
            {
                ProfundoSluice s = sluices[i];
                Assert.AreEqual(column.xMin, s.Water.xMin, 1e-3f, s.Id + " leaves a dry way round on the west");
                Assert.AreEqual(column.xMax, s.Water.xMax, 1e-3f, s.Id + " leaves a dry way round on the east");
                Assert.AreEqual(OverworldLayouts.CovaLodoMapId, MapOf(s.Water.center), s.Id);
                Assert.AreEqual(OverworldLayouts.CovaLodoMapId, MapOf(s.Lever), s.Id + " lever");
                Assert.AreEqual(-1, ProfundoSluiceRules.WaterAt(sluices, s.Lever), s.Id + " lever stands in the deep water");
                Assert.AreEqual(i, ProfundoSluiceRules.LeverAt(sluices, s.Lever), s.Id);
                Assert.AreEqual(i, ProfundoSluiceRules.WaterAt(sluices, s.Water.center), s.Id);
                Assert.Less(Mathf.Abs(s.Lever.y), Mathf.Abs(s.Water.center.y), s.Id + " lever is not on the Lâmpada 4 side");
            }

            Assert.AreEqual(-1, ProfundoSluiceRules.WaterAt(sluices, OverworldLayouts.LampadaLodoOffset), "the Lâmpada 4 stands dry");
        }

        // ------------------------------------------------------------------
        // 89 · the flights of the Novelo
        // ------------------------------------------------------------------

        [Test]
        public void Novelo_MatchesTheLayoutsCauseway()
        {
            Assert.AreEqual(OverworldLayouts.NoveloFlightCount, NoveloBridgeRules.FlightCount);
            Assert.AreEqual(OverworldLayouts.NoveloFlightStart, NoveloBridgeRules.FlightStart, 1e-3f);
            Assert.AreEqual(OverworldLayouts.NoveloFlightStep, NoveloBridgeRules.FlightStep, 1e-3f);
            Assert.That(OverworldLayouts.NoveloBridgeX, Is.InRange(NoveloBridgeRules.DarkWest, NoveloBridgeRules.DarkEast));
        }

        [Test]
        public void Novelo_LitCount_StopsAtTheFirstDarkFlight()
        {
            Assert.AreEqual(1, NoveloBridgeRules.LitCount(null));
            Assert.AreEqual(1, NoveloBridgeRules.LitCount(f => false));
            Assert.AreEqual(4, NoveloBridgeRules.LitCount(f => f != 4));
            Assert.AreEqual(NoveloBridgeRules.FlightCount, NoveloBridgeRules.LitCount(f => true));
        }

        [Test]
        public void Novelo_Cages_OnlyTheNextOneOpens()
        {
            Assert.AreEqual(NoveloCageCheck.Ok, NoveloBridgeRules.CheckCage(0, 1));
            Assert.AreEqual(NoveloCageCheck.FlightDark, NoveloBridgeRules.CheckCage(1, 1));
            Assert.AreEqual(NoveloCageCheck.AlreadyFree, NoveloBridgeRules.CheckCage(0, 2));
            Assert.AreEqual(NoveloCageCheck.Ok, NoveloBridgeRules.CheckCage(8, 9));
            Assert.AreEqual(NoveloCageCheck.Last, NoveloBridgeRules.CheckCage(9, 10));
            Assert.AreEqual(1, NoveloBridgeRules.BreathsDue(1.5f));
            Assert.AreEqual(3, NoveloBridgeRules.BreathsDue(NoveloBridgeRules.FreeSeconds));
            Assert.AreEqual(3f, NoveloBridgeRules.BreathAmount(100f, 100f, false), 1e-4f);
            Assert.AreEqual(4f, NoveloBridgeRules.BreathAmount(100f, 100f, true), 1e-4f);
            Assert.AreEqual(0f, NoveloBridgeRules.BreathAmount(30f, 100f, true));
            Assert.AreEqual(80f, NoveloBridgeRules.BurstAmount(1000f, false), 1e-3f);
        }

        [Test]
        public void Novelo_TheDark_CoversTheUnlitFlights_ButNotThePillarOrTheWest()
        {
            Vector2 pillar = OverworldLayouts.PilarDoMeioOffset;
            float x = OverworldLayouts.NoveloBridgeX;
            Assert.IsFalse(NoveloBridgeRules.IsDark(new Vector2(x, NoveloBridgeRules.FlightSouth(0) + 10f), 1, pillar), "flight 0 is lit from the start");
            Assert.IsTrue(NoveloBridgeRules.IsDark(new Vector2(x, NoveloBridgeRules.FlightSouth(1) + 10f), 1, pillar));
            Assert.IsFalse(NoveloBridgeRules.IsDark(new Vector2(x, NoveloBridgeRules.FlightSouth(1) + 10f), 2, pillar));
            Assert.IsFalse(NoveloBridgeRules.IsDark(pillar + new Vector2(10f, 0f), 1, pillar), "the Pilar do Meio's post stays lit");
            Assert.IsFalse(NoveloBridgeRules.IsDark(new Vector2(NoveloBridgeRules.DarkWest - 1f, 0f), 1, pillar), "the Cova do Lodo column is never dark");
            Assert.IsFalse(NoveloBridgeRules.IsDark(new Vector2(x, NoveloBridgeRules.FlightEnd + 5f), 1, pillar), "past the bridge");

            Vector2 lost = new Vector2(x, NoveloBridgeRules.FlightSouth(6) + 20f);
            Vector2 back = NoveloBridgeRules.NearestLitPoint(lost, 1, pillar);
            Assert.IsFalse(NoveloBridgeRules.IsDark(back, 1, pillar), "the nearest lit point is lit");
            Vector2 step = NoveloBridgeRules.PushStep(lost, back, 0.5f);
            Assert.AreEqual(NoveloBridgeRules.PushSpeed * 0.5f, step.magnitude, 1e-3f);
            Assert.Greater(NoveloBridgeRules.PushSpeed, 5.5f, "the dark pushes faster than the hero runs");
        }

        [Test]
        public void Novelo_FlightsLit_SavedAsTimeMarks_OnePerFlight()
        {
            var progress = new WorldMapProgress();
            Assert.IsFalse(ProfundoPathsMechanicsDriver.FlightLitIn(progress, 3));
            progress.SetTimeMark(NoveloBridgeRules.MarkKey(3), 1_700_000_000L);
            Assert.IsTrue(ProfundoPathsMechanicsDriver.FlightLitIn(progress, 3));
            Assert.IsFalse(ProfundoPathsMechanicsDriver.FlightLitIn(progress, 4));
            Assert.IsFalse(ProfundoPathsMechanicsDriver.FlightLitIn(null, 3));
            var keys = new HashSet<string>();
            for (int f = 1; f < NoveloBridgeRules.FlightCount; f++)
            {
                Assert.IsTrue(keys.Add(NoveloBridgeRules.MarkKey(f)));
            }
        }

        [Test]
        public void Novelo_Layout_NineCages_OnTheBridge_InTheirFlights()
        {
            for (int k = 0; k < NoveloBridgeRules.FlightCount - 1; k++)
            {
                Vector2 cage = OverworldLayouts.NoveloCage(k);
                Assert.AreEqual(k, NoveloBridgeRules.FlightAt(cage.y), "cage " + k);
                Assert.AreEqual(OverworldLayouts.PonteNoveloMapId, MapOf(cage), "cage " + k);
                Assert.IsTrue(ProfundoPathsMechanicsDriver.PartBounds[2].Contains(cage), "cage " + k);
                Assert.IsFalse(NoveloBridgeRules.IsDark(cage, k + 1, OverworldLayouts.PilarDoMeioOffset), "cage " + k + " stands in the dark when it can be opened");
            }
        }
    }
}
