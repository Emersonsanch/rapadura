using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rapadura.Gameplay.World.Map;
using Rapadura.Gameplay.World.Map.Areas;
using Rapadura.Gameplay.World.Map.Overworld;
using Placement = Rapadura.Tests.MingchengNartesBeastsPlacementTests.Placement;

namespace Rapadura.Tests
{
    /// <summary>
    /// The ninth pass of beasts (<see cref="ProfundoBeastsTests"/>: Limiar Profundo) now stand where their stand-ins stood (Docs/Mapas100.md
    /// 84–90), same idea as <see cref="MisturaBeastsPlacementTests"/>: each one is in the day table of every group of its map where it leads
    /// or comes along, its night form in the same group's night table where the beast leads it, the stand-in left those groups' day tables,
    /// and every case is checked on the clearings the group really got, at that area's levels. The sheets the pass does not cover keep their
    /// stand-ins (the Sombra Pequena, the Fumaça, the Morcego da Treva, the Ushabti Apagado, the Fumaça Sufocante, the Touro de Cinza, the
    /// Chama Faminta, the Morcego-Lâmina, the Redemoinho, the Enguia Grande, the Cardume Escuro, the Serpente Cega, the Rastejante, the
    /// Ushabti Apagado Gigante), and each new beast stays out of the maps its sheet does not name.
    /// </summary>
    public class ProfundoBeastsPlacementTests
    {
        private const string CopyShadow = "enemy_forest_copy_shadow";
        private const string SmokeGiant = "enemy_forest_smoke_giant";
        private const string SmokeArcher = "enemy_forest_smoke_archer";
        private const string EmberWolf = "enemy_forest_ember_wolf";
        private const string HotLion = "enemy_forest_hot_smoke_lion";
        private const string BlackEagle = "enemy_forest_black_eagle";
        private const string WindHarpy = "enemy_forest_wind_harpy";
        private const string MudOctopus = "enemy_forest_mud_octopus";
        private const string DarkFish = "enemy_forest_dark_fish";
        private const string ShadowLizard = "enemy_forest_shadow_lizard";
        private const string LesserDragon = "enemy_forest_lesser_shadow_dragon";
        private const string LesserArchon = "enemy_forest_lesser_archon";

        private const string Almas = "enemy_mountain_almas";
        private const string Sleepwalker = "enemy_mountain_sleepwalker_giant";
        private const string Terracotta = "enemy_forest_terracotta_soldier";
        private const string BronzeWolf = "enemy_mountain_bronze_wolf";
        private const string StoneLion = "enemy_forest_stone_lion";
        private const string StormEagle = "enemy_mountain_storm_eagle";
        private const string SmokeHarpy = "enemy_mountain_smoke_harpy";
        private const string Ooze = "enemy_cave_ooze";
        private const string BladeFish = "enemy_mountain_blade_fish";
        private const string LavaLizard = "enemy_mountain_giant_lava_lizard";
        private const string ThreeHeaded = "enemy_forest_three_headed_serpent";
        private const string CrownedOrc = "enemy_desert_crowned_orc";

        private const string ShadowBiter = "enemy_forest_shadow_biter";
        private const string Mist = "enemy_mountain_ill_wind_mist";
        private const string VampireBat = "enemy_cave_vampire_bat";
        private const string LacquerUshabti = "enemy_mountain_lacquer_ushabti";
        private const string SmokeGoat = "enemy_forest_smoke_goat";
        private const string WildYak = "enemy_mountain_wild_yak";
        private const string FireflySwarm = "enemy_desert_firefly_swarm";
        private const string CandleBat = "enemy_mountain_candle_bat";
        private const string ReedSnake = "enemy_forest_reed_snake";
        private const string LanternFish = "enemy_cave_lantern_fish";
        private const string DimmedPython = "enemy_forest_dimmed_python";
        private const string CreepingDark = "enemy_forest_creeping_dark";
        private const string Golem = "enemy_mountain_stone_golem";

        private const string ProfundoId = OverworldLayouts.ProfundoRegionId;
        private const bool Leads = true;
        private const bool Joins = false;

        private static Placement P(string enemy, string area, string group, bool withNightForm, params string[] standIns) =>
            new Placement(ProfundoId, enemy, area, group, withNightForm, standIns);

        private static readonly Placement[] Placements =
        {
            P(CopyShadow, "cova_da_fumaca", "sombras_copia", Leads, Almas),
            P(CopyShadow, "cabeceira_da_ponte", "filhos_da_fumaca", Leads, Almas),
            P(CopyShadow, "cova_da_fumaca", "gigantes_de_fumaca", Joins, Almas),
            P(CopyShadow, "corredores_de_cinza", "arqueiros_de_fumaca", Joins, Almas),

            P(SmokeGiant, "cova_da_fumaca", "gigantes_de_fumaca", Leads, Sleepwalker),
            P(SmokeGiant, "corredores_de_cinza", "gigantes_dos_corredores", Leads, Sleepwalker),

            P(SmokeArcher, "ventiladores_artifices", "arqueiros_dos_ventiladores", Leads, Terracotta),
            P(SmokeArcher, "corredores_de_cinza", "arqueiros_de_fumaca", Leads, Terracotta),
            P(SmokeArcher, "lances_do_novelo", "arqueiros_do_novelo", Leads, Terracotta),
            P(SmokeArcher, "ventiladores_artifices", "fumacas_sufocantes", Joins, Terracotta),
            P(SmokeArcher, "corredores_de_cinza", "gigantes_dos_corredores", Joins, Terracotta),
            P(SmokeArcher, "lances_do_novelo", "filhos_da_treva", Joins, Terracotta),

            P(EmberWolf, "cova_do_fogo_devorador", "lobos_de_brasa", Leads, BronzeWolf),
            P(EmberWolf, "saloes_de_cinza_quente", "lobos_da_cinza", Leads, BronzeWolf),
            P(EmberWolf, "cova_do_fogo_devorador", "chamas_famintas", Joins, BronzeWolf),
            P(EmberWolf, "saloes_de_cinza_quente", "leoes_de_fumaca_quente", Joins, BronzeWolf),
            P(EmberWolf, "cabeceira_da_ponte", "filhos_da_fumaca", Joins, BronzeWolf),

            P(HotLion, "saloes_de_cinza_quente", "leoes_de_fumaca_quente", Leads, StoneLion),
            P(HotLion, "cabeceira_da_ponte", "filhos_do_fogo", Leads, StoneLion),

            P(BlackEagle, "cova_do_vento_destruidor", "aguias_negras", Leads, StormEagle),
            P(BlackEagle, "correntes_ascendentes", "aguias_das_correntes", Leads, StormEagle),
            P(BlackEagle, "ponte_do_novelo", "filhos_do_vento", Leads, StormEagle),
            P(BlackEagle, "cova_do_vento_destruidor", "morcegos_lamina", Joins, StormEagle),
            P(BlackEagle, "cabeceira_da_ponte", "filhos_do_fogo", Joins, StormEagle),

            P(WindHarpy, "plataformas_soltas", "harpias_de_vento", Leads, SmokeHarpy),
            P(WindHarpy, "plataformas_soltas", "redemoinhos", Joins, SmokeHarpy),
            P(WindHarpy, "correntes_ascendentes", "aguias_das_correntes", Joins, SmokeHarpy),

            P(MudOctopus, "cova_do_lodo", "polvos_de_lodo", Leads, Ooze),
            P(MudOctopus, "comportas_artifices", "polvos_das_comportas", Leads, Ooze),
            P(MudOctopus, "ponte_do_novelo", "filhos_do_lodo", Leads, Ooze),
            P(MudOctopus, "comportas_artifices", "peixes_das_comportas", Joins, Ooze),

            P(DarkFish, "cova_do_lodo", "peixes_da_treva", Leads, BladeFish),
            P(DarkFish, "comportas_artifices", "peixes_das_comportas", Leads, BladeFish),
            P(DarkFish, "cova_do_lodo", "polvos_de_lodo", Joins, BladeFish),
            P(DarkFish, "ponte_do_novelo", "filhos_do_vento", Joins, BladeFish),

            P(ShadowLizard, "cova_da_treva", "lagartos_sombra", Leads, LavaLizard),
            P(ShadowLizard, "tuneis_cegos", "rastejantes", Joins, LavaLizard),

            P(LesserDragon, "tuneis_cegos", "dragoes_sombra_menores", Leads, ThreeHeaded),
            P(LesserDragon, "portao_da_terra_da_treva", "arcontes_do_portao", Joins, ThreeHeaded),

            P(LesserArchon, "praca_dos_pedestais", "arcontes_menores", Leads, CrownedOrc),
            P(LesserArchon, "portao_da_terra_da_treva", "arcontes_do_portao", Leads, CrownedOrc),
            P(LesserArchon, "praca_dos_pedestais", "ushabtis_gigantes", Joins, CrownedOrc),
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

        private static OverworldFauna Group(string id) =>
            OverworldLayouts.Get(ProfundoId).Zones.SelectMany(z => z.Fauna).First(f => f.Id == id);

        [TestCase("fumacas_dos_nichos", Mist)]
        [TestCase("fumacas_dos_varais", Mist)]
        [TestCase("ushabtis_da_muralha", LacquerUshabti)]
        [TestCase("ushabtis_das_cisternas", LacquerUshabti)]
        [TestCase("ushabtis_dos_varais", LacquerUshabti)]
        [TestCase("fumacas_sufocantes", SmokeGoat)]
        [TestCase("redemoinhos", Mist)]
        [TestCase("redemoinhos_das_correntes", Mist)]
        [TestCase("cardumes_escuros", LanternFish)]
        [TestCase("serpentes_cegas", DimmedPython)]
        [TestCase("rastejantes", CreepingDark)]
        [TestCase("filhos_da_treva", CreepingDark)]
        [TestCase("ushabtis_gigantes", Golem)]
        [TestCase("ushabtis_do_portao", Golem)]
        public void TheOtherSheets_KeepTheirStandIn(string group, string standIn)
        {
            OverworldFauna fauna = Group(group);
            Assert.IsTrue(fauna.Day.Any(e => e.EnemyId == standIn), $"{group}: still {standIn} by day");
            Assert.IsTrue(fauna.Night.Any(e => e.EnemyId == standIn), $"{group}: still {standIn} at night");
        }

        [TestCase(OverworldLayouts.FortalezaUltimaLampadaMapId, CopyShadow)]
        [TestCase(OverworldLayouts.CovaFogoMapId, CopyShadow)]
        [TestCase(OverworldLayouts.CovaFogoMapId, SmokeGiant)]
        [TestCase(OverworldLayouts.PonteNoveloMapId, SmokeGiant)]
        [TestCase(OverworldLayouts.FortalezaUltimaLampadaMapId, SmokeArcher)]
        [TestCase(OverworldLayouts.CovaVentoMapId, SmokeArcher)]
        [TestCase(OverworldLayouts.CovaFumacaMapId, EmberWolf)]
        [TestCase(OverworldLayouts.CovaLodoMapId, EmberWolf)]
        [TestCase(OverworldLayouts.CovaVentoMapId, HotLion)]
        [TestCase(OverworldLayouts.CovaFogoMapId, BlackEagle)]
        [TestCase(OverworldLayouts.CovaTrevaMapId, BlackEagle)]
        [TestCase(OverworldLayouts.CovaFumacaMapId, WindHarpy)]
        [TestCase(OverworldLayouts.PonteNoveloMapId, WindHarpy)]
        [TestCase(OverworldLayouts.CovaTrevaMapId, MudOctopus)]
        [TestCase(OverworldLayouts.CovaVentoMapId, DarkFish)]
        [TestCase(OverworldLayouts.CovaLodoMapId, ShadowLizard)]
        [TestCase(OverworldLayouts.PortaoTrevaMapId, ShadowLizard)]
        [TestCase(OverworldLayouts.PonteNoveloMapId, LesserDragon)]
        [TestCase(OverworldLayouts.CovaTrevaMapId, LesserArchon)]
        public void TheBeast_StaysOutOfTheMapsItsSheetDoesNotName(string map, string beast)
        {
            OverworldLayout w = OverworldLayouts.Get(ProfundoId);
            List<MapSpawnZone> clearings = w.World.Zones.Where(s => w.ZoneOfSpawn[s.Id].MapIdOrOwner == map).ToList();
            Assert.IsNotEmpty(clearings, map);
            Assert.IsFalse(clearings.Any(s => s.Day.Concat(s.Night).Any(e => e.EnemyId == beast)), $"no {beast} in {map}");
        }

        [Test]
        public void NoStandIn_SpawnsByDay_AnywhereOnTheThreshold()
        {
            OverworldLayout w = OverworldLayouts.Get(ProfundoId);
            List<string> day = w.World.Zones.SelectMany(s => s.Day).Select(e => e.EnemyId).ToList();
            foreach (string standIn in new[] { Almas, Sleepwalker, Terracotta, BronzeWolf, StoneLion, StormEagle, SmokeHarpy, Ooze, BladeFish, LavaLizard, ThreeHeaded, CrownedOrc })
            {
                Assert.IsFalse(day.Contains(standIn), $"no {standIn} by day on the Threshold any more");
            }
        }
    }
}
