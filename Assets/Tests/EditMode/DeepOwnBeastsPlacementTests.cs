using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rapadura.Gameplay.Night;
using Rapadura.Gameplay.World.Cities;
using Rapadura.Gameplay.World.Map;
using Rapadura.Gameplay.World.Map.Areas;
using Rapadura.Gameplay.World.Map.Overworld;
using Placement = Rapadura.Tests.MingchengNartesBeastsPlacementTests.Placement;
using Deep = Rapadura.Gameplay.World.Map.Overworld.OverworldLayouts.DeepBeasts;

namespace Rapadura.Tests
{
    /// <summary>
    /// The twenty-second pass of beasts (<see cref="DeepOwnBeastsTests"/>) now stands where its stand-ins stood in the Rio das Doze Horas, the
    /// Margens da Mistura and the Limiar Profundo: each beast is in the day table of the groups of its sheet, its night form in the same
    /// group's night table where it leads, the stand-in left those groups' day tables, and every case is checked on the clearings the group
    /// really got (OverworldLayouts.GenerateClearings, after the trails took theirs), at that area's levels. Same idea as
    /// <see cref="HighlandOwnBeastsPlacementTests"/>. Every new beast leads at least one drawn clearing, no stand-in it replaced still spawns
    /// by day in its region, and the city quests that hunted a stand-in hunt the region's own beast.
    /// </summary>
    public class DeepOwnBeastsPlacementTests
    {
        private const string SandSnake = "enemy_desert_sand_snake";
        private const string DesertScorpion = PeriodSpawnTable.DesertScorpion;
        private const string RopeUshabti = "enemy_mountain_rope_ushabti";
        private const string Ooze = "enemy_cave_ooze";
        private const string Mangrove = "enemy_forest_mangrove_crocodile";
        private const string Terracotta = "enemy_forest_terracotta_soldier";
        private const string LacquerUshabti = "enemy_mountain_lacquer_ushabti";
        private const string SmokeHarpy = "enemy_mountain_smoke_harpy";
        private const string SmokeGoat = "enemy_forest_smoke_goat";
        private const string LanternFish = "enemy_cave_lantern_fish";
        private const string DimmedPython = "enemy_forest_dimmed_python";
        private const string Golem = PeriodSpawnTable.MountainGolem;

        private const string Rio = OverworldLayouts.RioRegionId;
        private const string Mistura = OverworldLayouts.MisturaRegionId;
        private const string Profundo = OverworldLayouts.ProfundoRegionId;

        private const bool Leads = true;
        private const bool Joins = false;

        private static Placement P(string region, string enemy, string area, string group, bool withNightForm, params string[] standIns) =>
            new Placement(region, enemy, area, group, withNightForm, standIns);

        private static readonly Placement[] Placements =
        {
            // Anéis de Mehen (74)
            P(Rio, Deep.CoilScorpion, "anel_externo", "escorpioes_dos_aneis", Leads, DesertScorpion),
            P(Rio, Deep.CoilScorpion, "anel_externo", "mordedores_do_anel", Joins, DesertScorpion),
            P(Rio, Deep.DimmedUshabti, "no_errado", "ushabtis_apagados", Leads, RopeUshabti),
            P(Rio, Deep.DimmedUshabti, "no_errado", "trevas_do_no", Joins, RopeUshabti),

            // Lago de Fogo (75)
            P(Rio, Deep.ShadowSalamander, "beira_fervente", "salamandras_de_sombra", Leads, SandSnake),
            P(Rio, Deep.ShadowSalamander, "beira_fervente", "sombras_fervidas", Joins, SandSnake),
            P(Rio, Deep.ShadowSalamander, "lago_de_fogo", "chamas_famintas_do_lago", Joins, SandSnake),
            P(Rio, Deep.ShadowSalamander, "praia_de_brasa", "ushabtis_da_brasa", Joins, SandSnake),
            P(Rio, Deep.BoiledShadow, "beira_fervente", "sombras_fervidas", Leads, Ooze),
            P(Rio, Deep.BoiledShadow, "praia_de_brasa", "sombras_da_brasa", Leads, Ooze),
            P(Rio, Deep.BoiledShadow, "beira_fervente", "salamandras_de_sombra", Joins, Ooze),
            P(Rio, Deep.CrackedUshabti, "lago_de_fogo", "ushabtis_rachados", Leads, Terracotta),
            P(Rio, Deep.CrackedUshabti, "praia_de_brasa", "ushabtis_da_brasa", Leads, Terracotta),

            // Banco de Areia de Apep (76)
            P(Rio, Deep.MudCrocodile, "banco_de_areia_de_apep", "crocodilos_de_lodo", Leads, Mangrove),
            P(Rio, Deep.MudCrocodile, "rio_seco", "crocodilos_do_rio_seco", Leads, Mangrove),
            P(Rio, Deep.MudCrocodile, "rio_seco", "escaravelhos_do_rio_seco", Joins, Mangrove),
            P(Rio, Deep.MudCrocodile, "escamas_roxas", "filhos_das_escamas", Joins, Mangrove),
            P(Rio, Deep.MudCrocodile, "areia_movedica", "areias_movedicas", Joins, Mangrove),

            // Deserto do Relógio (78) and Poços de Betume (79)
            P(Mistura, Deep.ClockUshabti, "sombras_atrasadas", "ushabtis_de_relogio", Leads, LacquerUshabti),
            P(Mistura, Deep.ClockUshabti, "areia_das_horas", "ushabtis_das_horas", Leads, LacquerUshabti),
            P(Mistura, Deep.ClockUshabti, "sombras_atrasadas", "sombras_atrasadas", Joins, LacquerUshabti),
            P(Mistura, Deep.PitchSmoke, "pocas_de_piche", "fumacas_de_piche", Leads, SmokeHarpy),
            P(Mistura, Deep.PitchSmoke, "juncal_de_piche", "fumacas_do_juncal", Leads, SmokeHarpy),
            P(Mistura, Deep.PitchSmoke, "pocos_de_betume", "bagres_negros", Joins, SmokeHarpy),

            // Cova da Fumaça (84), Cova do Lodo (87), Cova da Treva (88), Portão da Terra da Treva (90)
            P(Profundo, Deep.ChokingSmoke, "ventiladores_artifices", "fumacas_sufocantes", Leads, SmokeGoat),
            P(Profundo, Deep.ChokingSmoke, "ventiladores_artifices", "arqueiros_dos_ventiladores", Joins, SmokeGoat),
            P(Profundo, Deep.ChokingSmoke, "cova_da_fumaca", "sombras_copia", Joins, SmokeGoat),
            P(Profundo, Deep.DarkShoal, "saloes_alagados", "cardumes_escuros", Leads, LanternFish),
            P(Profundo, Deep.DarkShoal, "saloes_alagados", "enguias_grandes", Joins, LanternFish),
            P(Profundo, Deep.DarkShoal, "cova_do_lodo", "peixes_da_treva", Joins, LanternFish),
            P(Profundo, Deep.BlindSerpent, "cova_da_treva", "serpentes_cegas", Leads, DimmedPython),
            P(Profundo, Deep.BlindSerpent, "cova_da_treva", "lagartos_sombra", Joins, DimmedPython),
            P(Profundo, Deep.GiantDimmedUshabti, "praca_dos_pedestais", "ushabtis_gigantes", Leads, Golem),
            P(Profundo, Deep.GiantDimmedUshabti, "portao_da_terra_da_treva", "ushabtis_do_portao", Leads, Golem),
        };

        private static IEnumerable<Placement> All() => Placements;

        /// <summary>The twelve beasts of the pass and their region (every one must lead a drawn clearing there).</summary>
        private static IEnumerable<TestCaseData> Beasts() => new[]
        {
            new TestCaseData(Rio, Deep.CoilScorpion), new TestCaseData(Rio, Deep.DimmedUshabti), new TestCaseData(Rio, Deep.ShadowSalamander),
            new TestCaseData(Rio, Deep.BoiledShadow), new TestCaseData(Rio, Deep.CrackedUshabti), new TestCaseData(Rio, Deep.MudCrocodile),
            new TestCaseData(Mistura, Deep.ClockUshabti), new TestCaseData(Mistura, Deep.PitchSmoke),
            new TestCaseData(Profundo, Deep.ChokingSmoke), new TestCaseData(Profundo, Deep.DarkShoal), new TestCaseData(Profundo, Deep.BlindSerpent),
            new TestCaseData(Profundo, Deep.GiantDimmedUshabti),
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

        [TestCase(Rio, SandSnake)]
        [TestCase(Rio, DesertScorpion)]
        [TestCase(Rio, RopeUshabti)]
        [TestCase(Rio, Ooze)]
        [TestCase(Rio, Mangrove)]
        [TestCase(Rio, Terracotta)]
        [TestCase(Mistura, LacquerUshabti)]
        [TestCase(Mistura, SmokeHarpy)]
        [TestCase(Profundo, SmokeGoat)]
        [TestCase(Profundo, LanternFish)]
        [TestCase(Profundo, DimmedPython)]
        [TestCase(Profundo, Golem)]
        public void TheStandIns_NoLongerSpawnByDay_InTheRegion(string region, string standIn)
        {
            OverworldLayout w = OverworldLayouts.Get(region);
            Assert.IsFalse(w.World.Zones.SelectMany(s => s.Day).Any(e => e.EnemyId == standIn), $"{standIn}: its roles in {region} have their own beasts now");
        }

        // The Ushabti Apagado of the fortress (83) is its own beast now, the rampart ushabti, and the story step of the Muralha em Arco
        // hunts it (DeepOwn2BeastsPlacementTests).

        [TestCase(CityQuestCatalog.MesektetCrocodiles, Rio, "enemy_forest_dark_river_crocodile")]
        [TestCase(CityQuestCatalog.MesektetScorpions, Rio, "enemy_forest_black_scorpion")]
        [TestCase(CityQuestCatalog.MesektetBoilingShore, Rio, Deep.ShadowSalamander)]
        [TestCase(CityQuestCatalog.UltimaLampadaSerpents, Profundo, Deep.BlindSerpent)]
        public void TheCityQuests_HuntTheRegionsOwnBeast_WhichSpawnsThere(string questId, string region, string target)
        {
            CityQuestSpec spec = CityQuestCatalog.FindSpec(questId);
            Assert.IsNotNull(spec, questId);
            Assert.AreEqual(target, spec.Quest.Objectives.Single().TargetId, questId);
            Assert.IsTrue(OverworldLayouts.Get(region).World.Zones.SelectMany(s => s.Day).Any(e => e.EnemyId == target), $"{target} spawns by day in {region}");
        }
    }
}
