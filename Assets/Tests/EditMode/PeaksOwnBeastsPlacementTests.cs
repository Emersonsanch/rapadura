using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rapadura.Gameplay.Night;
using Rapadura.Gameplay.World.Map;
using Rapadura.Gameplay.World.Map.Areas;
using Rapadura.Gameplay.World.Map.Mechanics;
using Rapadura.Gameplay.World.Map.Overworld;
using Placement = Rapadura.Tests.MingchengNartesBeastsPlacementTests.Placement;
using Peaks = Rapadura.Gameplay.World.Map.Overworld.OverworldLayouts.PeaksOwnBeasts;

namespace Rapadura.Tests
{
    /// <summary>
    /// The twenty-eighth pass of beasts (<see cref="PeaksOwnBeastsTests"/>) now stands where its stand-ins stood in the Planície de Mingcheng (42,
    /// 46) and on the Ermidas Suspensas of Farnkesh (56): each beast is in the day table of the groups of its sheet, its night form in the same
    /// group's night table where it leads, the stand-in left those groups' day tables, and every case is checked on the clearings the group
    /// really got, at that area's levels. Same idea as <see cref="WestOwn2BeastsPlacementTests"/>. The ill-wind mist and the broken lantern left
    /// the hermitages by day (they keep the Pontes de Vento, the Mar de Nuvens and the cloud isles); the stone lion keeps the gates and the
    /// Guarda de Taowu, the terracotta soldier the Muralha, the forest wolf the pines.
    /// </summary>
    public class PeaksOwnBeastsPlacementTests
    {
        private const string Mist = "enemy_mountain_ill_wind_mist";
        private const string Lantern = "enemy_desert_broken_lantern";
        private const string Terracotta = "enemy_forest_terracotta_soldier";
        private const string Lion = "enemy_forest_stone_lion";
        private const string Wolf = PeriodSpawnTable.ForestWolf;

        private const string Ming = OverworldLayouts.MingchengId;
        private const string Fark = OverworldLayouts.FarnkeshRegionId;

        private const bool Leads = true;
        private const bool Joins = false;

        private static Placement M(string enemy, string area, string group, bool withNightForm, params string[] standIns) =>
            new Placement(Ming, enemy, area, group, withNightForm, standIns);

        private static Placement F(string enemy, string area, string group, bool withNightForm, params string[] standIns) =>
            new Placement(Fark, enemy, area, group, withNightForm, standIns);

        private static readonly Placement[] Placements =
        {
            F(Peaks.DimmedEcho, "ermidas_suspensas", "ecos_apagados", Leads, Mist),
            F(Peaks.DimmedEcho, "ermidas_suspensas", "sinos_rachados", Joins, Mist),

            F(Peaks.CrackedBell, "ermidas_suspensas", "sinos_rachados", Leads, Lantern),
            F(Peaks.CrackedBell, "pinaculo_do_sino", "gatos_de_pallas", Joins, Lantern),

            M(Peaks.TaotieServant, "cidadela_dos_quatro_gulosos", "servos_de_taotie", Leads, Terracotta),
            M(Peaks.QiongqiCub, "cidadela_dos_quatro_gulosos", "filhotes_de_qiongqi", Leads, Wolf),

            M(Peaks.LimeGuardian, "torres_brancas", "guardioes_de_cal", Leads, Lion),
            M(Peaks.LimeGuardian, "colinas_dos_espelhos", "guardioes_das_colinas", Leads, Lion),
        };

        private static IEnumerable<Placement> All() => Placements;

        /// <summary>The five beasts of the pass and their region (every one must lead a drawn clearing there).</summary>
        private static IEnumerable<TestCaseData> Beasts() => new[]
        {
            new TestCaseData(Fark, Peaks.DimmedEcho), new TestCaseData(Fark, Peaks.CrackedBell), new TestCaseData(Ming, Peaks.TaotieServant),
            new TestCaseData(Ming, Peaks.QiongqiCub), new TestCaseData(Ming, Peaks.LimeGuardian),
        };

        private readonly MingchengNartesBeastsPlacementTests checks = new MingchengNartesBeastsPlacementTests();

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            MapLayouts.ClearCache();
            OverworldLayouts.ClearCache();
        }

        [OneTimeTearDown]
        public void OneTimeTearDown() => MapLayouts.ClearCache();

        [Test]
        public void EachBeast_IsInItsGroupsTables_WithItsNightForm_AndTheStandInLeft([ValueSource(nameof(All))] Placement placement) =>
            checks.EachBeast_IsInItsGroupsTables_WithItsNightForm_AndTheStandInLeft(placement);

        [Test]
        public void EachBeast_SpawnsInItsGroupsClearings_AtTheAreasLevels([ValueSource(nameof(All))] Placement placement) =>
            checks.EachBeast_SpawnsInItsGroupsClearings_AtTheAreasLevels(placement);

        [TestCaseSource(nameof(Beasts))]
        public void EachBeast_LeadsAtLeastOneDrawnClearing_InItsRegion(string region, string enemy)
        {
            OverworldLayout w = OverworldLayouts.Get(region);
            List<MapSpawnZone> clearings = w.World.Zones.Where(s => s.Day.Any(e => e.EnemyId == enemy)).ToList();
            Assert.IsNotEmpty(clearings, $"{enemy}: no clearing of {region} spawns it (the trails or the draw took them all)");
            Assert.IsTrue(clearings.Any(s => s.Day.Where(e => e.EnemyId == enemy).Sum(e => e.Weight) >= 3), $"{enemy}: leads at least one clearing (weight 3)");
        }

        [TestCase("ermidas_suspensas", Mist)]
        [TestCase("ermidas_suspensas", Lantern)]
        [TestCase("pinaculo_do_sino", Mist)]
        [TestCase("pinaculo_do_sino", Lantern)]
        public void TheStandIns_LeftTheHermitages_ByDay(string area, string standIn)
        {
            OverworldLayout w = OverworldLayouts.Get(Fark);
            Assert.IsFalse(w.Zone(area).Fauna.SelectMany(f => f.Day).Any(e => e.EnemyId == standIn), $"{standIn}: its roles in {area} have their own beasts now");
        }

        [TestCase("neblinas_do_vento_ruim", Mist)]
        [TestCase("ventanias", Mist)]
        [TestCase("neblinas_do_vento_ruim", Lantern)]
        [TestCase("leoes_de_pedra", Lion)]
        [TestCase("guarda_de_taowu", Lion)]
        [TestCase("soldados_da_muralha", Terracotta)]
        [TestCase("lobos_dos_pinheiros", Wolf)]
        public void TheOtherSheetsOfTheRegions_KeepTheirStandIn(string group, string standIn)
        {
            OverworldFauna fauna = new[] { Fark, Ming }.SelectMany(r => OverworldLayouts.Get(r).Zones).SelectMany(z => z.Fauna).FirstOrDefault(f => f.Id == group);
            Assert.IsNotNull(fauna, group);
            Assert.IsTrue(fauna.Day.Any(e => e.EnemyId == standIn), $"{group}: still {standIn} by day");
        }

        [Test]
        public void TheEchoesThatRiseFromTheNoise_AreTheCliffsEcho()
        {
            Assert.AreEqual(Peaks.DimmedEcho, HermitageEchoRules.EchoEnemyId, "the meter raises the same Eco Apagado the ecos_apagados group spawns");
            OverworldLayout w = OverworldLayouts.Get(Fark);
            OverworldZone ermidas = w.Zone("ermidas_suspensas");
            Assert.AreEqual(163, HermitageEchoRules.RisenLevel(ermidas.MinLevel, ermidas.MaxLevel), "the middle of the hermitages' band");
        }

        [Test]
        public void TheQuestsOfTheRegions_StillFindTheirTargets()
        {
            // Bögü's lions (Mingcheng) and Tadi's ill wind (Farnkesh) ask for stand-ins that keep other roles in their regions.
            foreach ((string region, string target) in new[] { (Ming, Lion), (Fark, Mist) })
            {
                OverworldLayout w = OverworldLayouts.Get(region);
                Assert.IsTrue(w.World.Zones.SelectMany(s => s.Day).Any(e => e.EnemyId == target), $"{target}: still spawns in {region}");
            }
        }
    }
}
