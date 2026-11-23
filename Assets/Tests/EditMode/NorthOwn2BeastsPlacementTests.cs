using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rapadura.Gameplay.World.Map;
using Rapadura.Gameplay.World.Map.Areas;
using Rapadura.Gameplay.World.Map.Overworld;
using Placement = Rapadura.Tests.MingchengNartesBeastsPlacementTests.Placement;
using North2 = Rapadura.Gameplay.World.Map.Overworld.OverworldLayouts.NorthOwn2Beasts;

namespace Rapadura.Tests
{
    /// <summary>
    /// The twenty-ninth pass of beasts (<see cref="NorthOwn2BeastsTests"/>) now stands where its stand-ins stood on the Ninho da Huma (58), in
    /// the Canais de Gelo (60) and the Galerias dos Mamutes (62), on the Jardins de Lava Laqueada (66), Echeyde (67) and the Nine Furnaces (69),
    /// and in the Cidadela dos Quatro Gulosos (46): each beast is in the day table of the groups of its sheet, its night form in the same group's
    /// night table where it leads, the stand-in left those groups' day tables, and every case is checked on the clearings the group really got,
    /// at that area's levels. Same idea as <see cref="PeaksOwnBeastsPlacementTests"/>. The ooze and the broken lantern left the Mar Gelado, the
    /// lantern fish and the ill-wind mist left the archipelago; the mist keeps the Pontes de Vento and the Mar de Nuvens of Farnkesh, the stone
    /// lion the gates and the lion road of Mingcheng.
    /// </summary>
    public class NorthOwn2BeastsPlacementTests
    {
        private const string Mist = "enemy_mountain_ill_wind_mist";
        private const string Ooze = "enemy_cave_ooze";
        private const string Lantern = "enemy_desert_broken_lantern";
        private const string LanternFish = "enemy_cave_lantern_fish";
        private const string Lion = "enemy_forest_stone_lion";

        private const string Fark = OverworldLayouts.FarnkeshRegionId;
        private const string Gelo = OverworldLayouts.MarGeladoRegionId;
        private const string Fogo = OverworldLayouts.HinokuraRegionId;
        private const string Ming = OverworldLayouts.MingchengId;

        private const bool Leads = true;
        private const bool Joins = false;

        private static Placement P(string region, string enemy, string area, string group, bool withNightForm, params string[] standIns) =>
            new Placement(region, enemy, area, group, withNightForm, standIns);

        private static readonly Placement[] Placements =
        {
            P(Fark, North2.Gale, "ilhas_de_nuvem", "ventanias", Leads, Mist),
            P(Fark, North2.Gale, "ninho_da_huma", "raios_presos", Joins, Mist),

            P(Gelo, North2.IceCrack, "gelo_rachado", "rachaduras", Leads, Ooze),
            P(Gelo, North2.IceCrack, "gelo_rachado", "ursos_do_gelo_rachado", Joins, Ooze),
            P(Gelo, North2.IceCrack, "canais_do_oeste", "peixes_gelo_do_oeste", Joins, Ooze),

            P(Gelo, North2.LivingCrystal, "galerias_dos_mamutes", "cristais_vivos", Leads, Lantern),
            P(Gelo, North2.LivingCrystal, "fiorde_das_presas", "mamutes_do_fiorde", Joins, Lantern),
            P(Gelo, North2.IceRoot, "terra_congelada", "raizes_gelo", Leads, Ooze),
            P(Gelo, North2.IceRoot, "terra_congelada", "toupeiras_gelo", Joins, Ooze),
            P(Gelo, North2.IceRoot, "galerias_dos_mamutes", "cristais_vivos", Joins, Ooze),

            P(Fogo, North2.EmberCarp, "pontes_vermelhas", "carpas_de_brasa", Leads, LanternFish),
            P(Fogo, North2.EmberCarp, "pontes_vermelhas", "shisas_das_pontes", Joins, LanternFish),
            P(Fogo, North2.EmberCarp, "jardins_de_lava_laqueada", "tanukis_ladroes", Joins, LanternFish),
            P(Fogo, North2.EmberCarp, "estreito_do_degelo", "caranguejos_do_degelo", Joins, LanternFish),
            P(Fogo, North2.SmokeBamboo, "pinhais_de_nuvem", "bambus_de_fumaca", Leads, Mist),
            P(Fogo, North2.SmokeBamboo, "pinhais_de_nuvem", "tanukis_dos_pinhais", Joins, Mist),

            P(Fogo, North2.Fumarole, "malpais_de_echeyde", "fumarolas", Leads, Mist),
            P(Fogo, North2.Fumarole, "fontes_termais", "vapores_das_fontes", Leads, Mist),
            P(Fogo, North2.Fumarole, "monte_echeyde", "bombas_vulcanicas", Joins, Mist),
            P(Fogo, North2.Fumarole, "costa_das_fumarolas", "pedras_pomes", Joins, Mist),

            P(Fogo, North2.LivingBellows, "castelo_das_nove_fornalhas", "foles_vivos", Leads, Mist),

            P(Ming, North2.TaowuGuard, "cidadela_dos_quatro_gulosos", "guarda_de_taowu", Leads, Lion),
            P(Ming, North2.TaowuGuard, "cidadela_dos_quatro_gulosos", "servos_de_taotie", Joins, Lion),
        };

        private static IEnumerable<Placement> All() => Placements;

        /// <summary>The nine beasts of the pass and their region (every one must lead a drawn clearing there).</summary>
        private static IEnumerable<TestCaseData> Beasts() => new[]
        {
            new TestCaseData(Fark, North2.Gale), new TestCaseData(Gelo, North2.IceCrack), new TestCaseData(Gelo, North2.LivingCrystal),
            new TestCaseData(Gelo, North2.IceRoot), new TestCaseData(Fogo, North2.EmberCarp), new TestCaseData(Fogo, North2.SmokeBamboo),
            new TestCaseData(Fogo, North2.Fumarole), new TestCaseData(Fogo, North2.LivingBellows), new TestCaseData(Ming, North2.TaowuGuard),
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

        [TestCase(Gelo, Ooze)]
        [TestCase(Gelo, Lantern)]
        [TestCase(Fogo, Mist)]
        [TestCase(Fogo, LanternFish)]
        public void TheStandIns_NoLongerSpawnByDay_AnywhereInTheRegion(string region, string standIn)
        {
            OverworldLayout w = OverworldLayouts.Get(region);
            Assert.IsFalse(w.World.Zones.SelectMany(s => s.Day).Any(e => e.EnemyId == standIn), $"{standIn}: its roles in {region} have their own beasts now");
        }

        [Test]
        public void TheGale_LeftTheMist_OnTheCloudIsles_AndTheNest()
        {
            OverworldLayout w = OverworldLayouts.Get(Fark);
            foreach (string area in new[] { "ilhas_de_nuvem", "ninho_da_huma" })
            {
                Assert.IsFalse(w.Zone(area).Fauna.SelectMany(f => f.Day).Any(e => e.EnemyId == Mist), $"{area}: the Ventania has its own beast now");
            }
        }

        [TestCase(Fark, "neblinas_do_vento_ruim", Mist)]
        [TestCase(Fark, "neblinas_das_nuvens", Mist)]
        [TestCase(Ming, "leoes_de_pedra", Lion)]
        [TestCase(Ming, "leoes_do_caminho", Lion)]
        public void TheOtherSheetsOfTheRegions_KeepTheirStandIn(string region, string group, string standIn)
        {
            OverworldFauna fauna = OverworldLayouts.Get(region).Zones.SelectMany(z => z.Fauna).FirstOrDefault(f => f.Id == group);
            Assert.IsNotNull(fauna, group);
            Assert.IsTrue(fauna.Day.Any(e => e.EnemyId == standIn), $"{group}: still {standIn} by day");
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
