using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rapadura.Gameplay.Night;
using Rapadura.Gameplay.World.Map;
using Rapadura.Gameplay.World.Map.Areas;
using Rapadura.Gameplay.World.Map.Overworld;
using Placement = Rapadura.Tests.MingchengNartesBeastsPlacementTests.Placement;

namespace Rapadura.Tests
{
    /// <summary>
    /// The fourth pass of beasts (<see cref="FarnkeshBeastsTests"/>: Picos de Farnkesh) now stand where their stand-ins stood
    /// (Docs/Mapas100.md 53–58), same idea as <see cref="MingchengNartesBeastsPlacementTests"/>: each one is in the day table of
    /// every group its sheet gives it (leading the group or as its companion), its night form in the same group's night table
    /// where the beast leads it (the smoke goats sleep at night: the vine foxes keep their groups), the stand-in left those
    /// groups' day tables, and every case is checked on the clearings the group really got (each group listed here draws at
    /// least one clearing in its area), at that area's levels. The wild griffins of Huma's nest are the young griffin.
    /// </summary>
    public class FarnkeshBeastsPlacementTests
    {
        private const string SmokeGoat = "enemy_forest_smoke_goat";
        private const string Bee = "enemy_forest_golden_bee";
        private const string FluteBird = "enemy_forest_flute_bird";
        private const string VineFox = "enemy_forest_vine_fox";
        private const string Chough = "enemy_mountain_alpine_chough";
        private const string Takin = "enemy_mountain_takin";
        private const string BlueSheep = "enemy_mountain_blue_sheep";
        private const string CandleBat = "enemy_mountain_candle_bat";
        private const string Ushabti = "enemy_mountain_rope_ushabti";
        private const string PallasCat = "enemy_mountain_pallas_cat";
        private const string StormEagle = "enemy_mountain_storm_eagle";
        private const string Harpy = "enemy_mountain_smoke_harpy";
        private const string Griffin = "enemy_mountain_young_griffin";

        private const string Yak = "enemy_mountain_wild_yak";
        private const string FireflySwarm = NightVariantRules.FireflySwarmEnemyId;
        private const string Hawk = "enemy_mountain_crested_hawk";
        private const string Jackal = "enemy_desert_jackal";
        private const string Crow = "enemy_cave_dark_crow";
        private const string VampireBat = NightVariantRules.VampireBatEnemyId;
        private const string Golem = PeriodSpawnTable.MountainGolem;
        private const string Irbis = "enemy_mountain_irbis";
        private const string Vulture = "enemy_desert_ember_vulture";
        private const string CaveBats = PeriodSpawnTable.CaveBats;

        private const string Fark = OverworldLayouts.FarnkeshRegionId;
        private const bool Leads = true;
        private const bool Joins = false;

        private static Placement P(string enemy, string area, string group, bool withNightForm, params string[] standIns) =>
            new Placement(Fark, enemy, area, group, withNightForm, standIns);

        private static readonly Placement[] Placements =
        {
            // The smoke goats lead their groups by day and sleep at night.
            P(SmokeGoat, "clareiras_do_baile", "bodes_de_fumaca", Joins, Yak),
            P(SmokeGoat, "vinhas_do_sol", "bodes_das_vinhas", Joins, Yak),
            P(SmokeGoat, "rio_pactolo", "bodes_do_pactolo", Joins, Yak),

            P(Bee, "vale_dos_satiros", "abelhas_douradas", Leads, FireflySwarm),
            P(Bee, "vinhas_do_sol", "abelhas_do_sol", Leads, FireflySwarm),
            P(Bee, "rio_pactolo", "abelhas_do_pactolo", Leads, FireflySwarm),
            P(Bee, "clareiras_do_baile", "passaros_flauta", Joins, FireflySwarm),
            P(Bee, "vinhas_de_gordias", "macacos_das_vinhas", Joins, FireflySwarm),
            P(Bee, "vinhas_do_sol", "bodes_das_vinhas", Joins, FireflySwarm),

            P(FluteBird, "clareiras_do_baile", "passaros_flauta", Leads, Hawk),
            P(FluteBird, "vau_do_pactolo", "passaros_do_vau", Leads, Hawk),
            P(FluteBird, "vale_dos_satiros", "raposas_da_vinha", Joins, Hawk),
            P(FluteBird, "vau_do_pactolo", "raposas_do_vau", Joins, Hawk),
            P(FluteBird, "rio_pactolo", "abelhas_do_pactolo", Joins, Hawk),

            P(VineFox, "vale_dos_satiros", "raposas_da_vinha", Leads, Jackal),
            P(VineFox, "vinhas_de_gordias", "raposas_de_gordias", Leads, Jackal),
            P(VineFox, "vau_do_pactolo", "raposas_do_vau", Leads, Jackal),
            P(VineFox, "vale_dos_satiros", "abelhas_douradas", Joins, Jackal),
            P(VineFox, "clareiras_do_baile", "bodes_de_fumaca", Leads, Jackal),
            P(VineFox, "vau_do_pactolo", "passaros_do_vau", Joins, Jackal),
            P(VineFox, "rio_pactolo", "bodes_do_pactolo", Leads, Jackal),
            P(VineFox, "mata_das_castanheiras", "ursos_das_castanheiras", Joins, Jackal),

            P(Chough, "prados_do_vento", "gralhas_do_vento", Leads, Crow),
            P(Chough, "subida_de_farnkesh", "gralhas_alpinas", Leads, Crow),
            P(Chough, "farnkesh", "gralhas_de_farnkesh", Leads, Crow),
            P(Chough, "pinaculo_do_sino", "gralhas_das_ermidas", Leads, Crow, CaveBats),
            P(Chough, "prados_do_vento", "marmotas_do_vento", Joins, Crow),
            P(Chough, "prados_dos_sinos", "cabras_azuis", Joins, Crow),
            P(Chough, "escadaria_dos_cestos", "takins_apagados", Joins, Crow),
            P(Chough, "mar_de_nuvens", "neblinas_das_nuvens", Joins, Crow),
            P(Chough, "bosque_de_pinaculos", "gatos_dos_pinaculos", Joins, Crow),

            P(Takin, "prados_dos_sinos", "takins_dos_sinos", Leads, Yak),
            P(Takin, "escadaria_dos_cestos", "takins_apagados", Leads, Yak),
            P(Takin, "escadaria_dos_cestos", "abutres_do_pico", Joins, Yak),
            P(Takin, "farnkesh", "cabras_dos_ninhos", Joins, Yak),

            P(BlueSheep, "prados_dos_sinos", "cabras_azuis", Leads, Jackal),
            P(BlueSheep, "farnkesh", "cabras_dos_ninhos", Leads, Jackal),
            P(BlueSheep, "subida_de_farnkesh", "gralhas_alpinas", Joins, Jackal),
            P(BlueSheep, "subida_de_farnkesh", "marmotas_da_subida", Joins, Jackal),

            P(CandleBat, "pinaculos_do_leste", "morcegos_vela_do_leste", Leads, VampireBat),
            P(CandleBat, "pontes_de_vento", "morcegos_vela", Leads, VampireBat),
            P(CandleBat, "pinaculos_do_leste", "pipas_sombra_do_leste", Joins, VampireBat),
            P(CandleBat, "ermida_baixa", "ushabtis_da_ermida", Joins, VampireBat),

            P(Ushabti, "pontes_de_vento", "ushabtis_de_corda", Leads, Golem),
            P(Ushabti, "ermida_baixa", "ushabtis_da_ermida", Leads, Golem),
            P(Ushabti, "ermida_baixa", "pipas_sombra", Joins, Golem),

            P(PallasCat, "bosque_de_pinaculos", "gatos_dos_pinaculos", Leads, Irbis),
            P(PallasCat, "pinaculo_do_sino", "gatos_de_pallas", Leads, Irbis),
            P(PallasCat, "escarpa_dos_ventos", "gatos_da_escarpa", Leads, Irbis),
            P(PallasCat, "bosque_de_pinaculos", "abutres_dos_pinaculos", Joins, Irbis),
            P(PallasCat, "ermidas_suspensas", "ecos_apagados", Joins, Irbis),
            P(PallasCat, "pinaculo_do_sino", "gralhas_das_ermidas", Joins, Irbis),

            P(StormEagle, "gruta_abrigo", "aguias_da_tempestade", Leads, Hawk),
            P(StormEagle, "garganta_dos_ays", "ays_do_vento", Joins, Hawk),
            P(StormEagle, "escarpa_dos_ventos", "pedras_da_escarpa", Joins, Hawk),

            P(Harpy, "ilhas_de_nuvem", "harpias_de_fumaca", Leads, Crow),
            P(Harpy, "ilhas_de_nuvem", "ventanias", Joins, Crow),

            P(Griffin, "ninho_da_huma", "grifos_selvagens", Leads, Vulture),
        };

        private static IEnumerable<Placement> All() => Placements;

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

        [Test]
        public void TheSmokeGoats_SleepAtNight_TheVineFoxesKeepTheirGroups()
        {
            OverworldLayout w = OverworldLayouts.Get(Fark);
            foreach (string group in new[] { "bodes_de_fumaca", "bodes_das_vinhas", "bodes_do_pactolo" })
            {
                OverworldFauna fauna = w.Zones.SelectMany(z => z.Fauna).First(f => f.Id == group);
                Assert.IsFalse(fauna.Night.Any(e => e.EnemyId == SmokeGoat), $"{group}: the smoke goats sleep");
                Assert.IsTrue(fauna.Night.Any(e => e.EnemyId == VineFox && e.Form != NightForm.Normal), $"{group}: the vine foxes at night");
            }
        }

        [Test]
        public void NoStandIn_SpawnsByDay_AnywhereOnThePeaks()
        {
            OverworldLayout w = OverworldLayouts.Get(Fark);
            List<string> day = w.World.Zones.SelectMany(s => s.Day).Select(e => e.EnemyId).ToList();
            foreach (string standIn in new[] { Yak, FireflySwarm, Hawk, Jackal, Crow, VampireBat, Golem, Irbis, CaveBats })
            {
                Assert.IsFalse(day.Contains(standIn), $"no {standIn} by day on the peaks any more");
            }
        }
    }
}
