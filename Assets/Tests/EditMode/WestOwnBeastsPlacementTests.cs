using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rapadura.Gameplay.Night;
using Rapadura.Gameplay.World.Cities;
using Rapadura.Gameplay.World.Map;
using Rapadura.Gameplay.World.Map.Areas;
using Rapadura.Gameplay.World.Map.Overworld;
using Placement = Rapadura.Tests.MingchengNartesBeastsPlacementTests.Placement;
using West = Rapadura.Gameplay.World.Map.Overworld.OverworldLayouts.WestOwnBeasts;

namespace Rapadura.Tests
{
    /// <summary>
    /// The twenty-fourth pass of beasts (<see cref="WestOwnBeastsTests"/>) now stands where its stand-ins stood in the Costa das Monções (28–30),
    /// the Delta de Jhilpur (31–35), the Planície de Mingcheng (42–44) and the Estepe dos Nartes (51): each beast is in the day table of the
    /// groups of its sheet, its night form in the same group's night table where it leads, the stand-in left those groups' day tables, and every
    /// case is checked on the clearings the group really got, at that area's levels. Same idea as <see cref="NorthOwnBeastsPlacementTests"/>.
    /// The lantern fish, the gull and the Tugai wolf left Mingcheng and the steppe by day; the sand snake, the lantern fish, the gull, the
    /// crocodile, the crow, the ill-wind mist and the broken lantern keep the other roles of their regions.
    /// </summary>
    public class WestOwnBeastsPlacementTests
    {
        private const string SandSnake = "enemy_desert_sand_snake";
        private const string LanternFish = "enemy_cave_lantern_fish";
        private const string Gull = "enemy_forest_thief_gull";
        private const string Crocodile = "enemy_forest_mangrove_crocodile";
        private const string Crow = "enemy_cave_dark_crow";
        private const string Mist = "enemy_mountain_ill_wind_mist";
        private const string Lantern = "enemy_desert_broken_lantern";
        private const string Wolf = PeriodSpawnTable.ForestWolf;

        private const string Costa = OverworldLayouts.CostaId;
        private const string Delta = OverworldLayouts.DeltaId;
        private const string Ming = OverworldLayouts.MingchengId;
        private const string Nart = OverworldLayouts.NartesId;

        private const bool Leads = true;
        private const bool Joins = false;

        private static Placement C(string enemy, string area, string group, bool withNightForm, params string[] standIns) =>
            new Placement(Costa, enemy, area, group, withNightForm, standIns);

        private static Placement D(string enemy, string area, string group, bool withNightForm, params string[] standIns) =>
            new Placement(Delta, enemy, area, group, withNightForm, standIns);

        private static Placement M(string enemy, string area, string group, bool withNightForm, params string[] standIns) =>
            new Placement(Ming, enemy, area, group, withNightForm, standIns);

        private static Placement N(string enemy, string area, string group, bool withNightForm, params string[] standIns) =>
            new Placement(Nart, enemy, area, group, withNightForm, standIns);

        private static readonly Placement[] Placements =
        {
            C(West.MudShark, "jardins_de_alga", "tubaroes_de_lodo", Leads, SandSnake),
            C(West.SmokeRay, "kilwa_afundada", "raias_de_fumaca", Leads, LanternFish),
            C(West.DimmedHeron, "canais_de_liyongo", "garcas_apagadas", Leads, Gull),

            D(West.MudGharial, "lago_de_jhilpur", "gaviais_do_lodo", Leads, Crocodile),
            D(West.MudGharial, "charcos_de_lotus", "gaviais_do_charco", Leads, Crocodile),
            D(West.MudGharial, "floresta_de_mare", "gaviais_da_mare", Leads, Crocodile),

            D(West.NinkiHatchling, "charco_do_ninki_nanka", "filhotes_de_ninki_nanka", Leads, Crocodile),
            D(West.NinkiHatchling, "toca_da_dragoa", "filhotes_da_toca", Leads, Crocodile),

            D(West.RottenRoot, "mata_morta", "raizes_podres", Leads, Crow),
            D(West.RottenRoot, "coracao_do_lodo", "raizes_podres_do_lodo", Leads, Crow),

            M(West.FishLantern, "canal_das_lanternas", "lanternas_peixe", Leads, LanternFish),
            M(West.FishLantern, "eclusa_da_lua", "carpas_da_eclusa", Joins, LanternFish),
            M(West.FishLantern, "canal_das_lanternas", "barqueiros_de_fumaca", Joins, LanternFish),

            M(West.SmokeBoatman, "canal_das_lanternas", "barqueiros_de_fumaca", Leads, Mist),
            M(West.SmokeBoatman, "canal_das_lanternas", "tartarugas_do_canal", Joins, Mist),

            M(West.MandarinDuck, "rio_dos_salgueiros", "patos_mandarins", Leads, Gull),
            M(West.MandarinDuck, "arrozais_espelhados", "patos_ladroes", Leads, Gull),
            M(West.MandarinDuck, "vilas_do_anel", "raposas_das_vilas", Joins, Gull),
            M(West.MandarinDuck, "campos_de_painco", "gafanhotos_do_painco", Joins, Gull),
            M(West.MandarinDuck, "arrozais_espelhados", "bufalos_de_lama", Joins, Gull),

            M(West.DimmedScarecrow, "moinho_dagua", "espantalhos_apagados", Leads, Lantern),

            M(West.CrookedRay, "torres_brancas", "raios_tortos", Leads, Mist),
            M(West.CrookedRay, "torres_brancas", "guardioes_de_cal", Joins, Mist),
            M(West.CrookedRay, "colinas_dos_espelhos", "espelhos_do_atalho", Joins, Mist),

            N(West.GreyWolf, "pradaria_ondulada", "lobos_cinzentos", Leads, Wolf),
            N(West.GreyWolf, "estepe_de_soslan", "saigas_da_estepe", Joins, Wolf),
            N(West.GreyWolf, "rio_dos_nartes", "cisnes_do_rio", Joins, Wolf),
            N(West.GreyWolf, "campos_de_papoula", "tempestades_de_capim", Joins, Wolf),
            N(West.GreyWolf, "pradaria_ondulada", "abutres_da_estepe", Joins, Wolf),
        };

        private static IEnumerable<Placement> All() => Placements;

        /// <summary>The twelve beasts of the pass and their region (every one must lead a drawn clearing there).</summary>
        private static IEnumerable<TestCaseData> Beasts() => new[]
        {
            new TestCaseData(Costa, West.MudShark), new TestCaseData(Costa, West.SmokeRay), new TestCaseData(Costa, West.DimmedHeron),
            new TestCaseData(Delta, West.MudGharial), new TestCaseData(Delta, West.NinkiHatchling), new TestCaseData(Delta, West.RottenRoot),
            new TestCaseData(Ming, West.FishLantern), new TestCaseData(Ming, West.SmokeBoatman), new TestCaseData(Ming, West.MandarinDuck),
            new TestCaseData(Ming, West.DimmedScarecrow), new TestCaseData(Ming, West.CrookedRay), new TestCaseData(Nart, West.GreyWolf),
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

        [TestCase(Ming, LanternFish)]
        [TestCase(Ming, Gull)]
        [TestCase(Nart, Wolf)]
        public void TheStandIns_NoLongerSpawnByDay_AnywhereInTheRegion(string region, string standIn)
        {
            OverworldLayout w = OverworldLayouts.Get(region);
            Assert.IsFalse(w.World.Zones.SelectMany(s => s.Day).Any(e => e.EnemyId == standIn), $"{standIn}: its roles in {region} have their own beasts now");
        }

        [TestCase(Costa, "cobras_das_dunas", SandSnake)]
        [TestCase(Costa, "peixes_lanterna", LanternFish)]
        [TestCase(Costa, "gaivotas_ladras", Gull)]
        [TestCase(Delta, "gavioes_da_garganta", Crow)]
        // The Crocodilo de Musgo (33), the Gavial-Sombra (35), the Névoa do Shen, the Bolha de Hundun (46) and the riddle lanterns with the village
        // cranes (41) took the rest of those roles in the twenty-seventh pass (WestOwn2BeastsPlacementTests).
        public void TheOtherMapsOfTheRegion_KeepTheirStandIn(string region, string group, string standIn)
        {
            OverworldFauna fauna = OverworldLayouts.Get(region).Zones.SelectMany(z => z.Fauna).FirstOrDefault(f => f.Id == group);
            Assert.IsNotNull(fauna, group);
            Assert.IsTrue(fauna.Day.Any(e => e.EnemyId == standIn), $"{group}: still {standIn} by day");
        }

        [Test]
        public void TheQuestsThatAskedForTheStandIns_AskForWhatStandsThereNow()
        {
            CityQuestSpec canal = CityQuestCatalog.Quests.Single(q => q.QuestId == CityQuestCatalog.MingchengCanal);
            Assert.AreEqual(West.FishLantern, canal.Quest.Objectives.Single().TargetId, "Lin Deng's canal: the Lanterna-Peixe");
            CityQuestSpec wolves = CityQuestCatalog.Quests.Single(q => q.QuestId == CityQuestCatalog.SoslanWolves);
            Assert.AreEqual("enemy_mountain_steppe_wolf", wolves.Quest.Objectives.Single().TargetId, "Soslan's wolves: the Lobo da Estepe, the Tugai wolf left the steppe");
        }
    }
}
