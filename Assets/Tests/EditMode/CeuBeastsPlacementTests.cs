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
    /// The tenth pass of beasts (<see cref="CeuBeastsTests"/>: Reinos do Céu) now stand where their stand-ins stood (Docs/Mapas100.md 91–100),
    /// same idea as <see cref="ProfundoBeastsPlacementTests"/>: each one is in the day table of every group of its map where it leads or comes
    /// along, its night form in the same group's night table where the beast leads it, the stand-in left those groups' day tables, and every
    /// case is checked on the clearings the group really got, at that area's levels. The three small archons of the sheets (91, 98, 99) are the
    /// Arconte Menor of the Limiar Profundo. The Corda de Fumaça (97) has its own beast (LateStandInBeastsTests), the Sky Stone also walks the Porch (100: "outside the
    /// orchard, the beasts of 99"), the sheets the pass does not cover keep their stand-ins, and each new beast stays out of the maps its sheet
    /// does not name.
    /// </summary>
    public class CeuBeastsPlacementTests
    {
        private const string CloudBird = "enemy_forest_dimmed_cloud_bird";
        private const string LightFish = "enemy_forest_dimmed_light_fish";
        private const string DarkStowaway = "enemy_forest_dark_stowaway";
        private const string HoldShadow = "enemy_forest_hold_shadow";
        private const string LivingRope = "enemy_forest_living_rope";
        private const string ToyArchon = "enemy_forest_toy_archon";
        private const string CrackedJar = "enemy_forest_cracked_jar";
        private const string ChainedArchon = "enemy_forest_chained_archon";
        private const string BalloonGiant = "enemy_forest_balloon_giant";
        private const string AzThief = "enemy_forest_az_thief";
        private const string DimmedGull = "enemy_forest_dimmed_light_gull";
        private const string SkyStone = "enemy_forest_sky_stone";
        private const string LesserArchon = "enemy_forest_lesser_archon";

        private const string Ibis = "enemy_forest_dimmed_ibis";
        private const string LanternFish = "enemy_cave_lantern_fish";
        private const string ShadowBiter = "enemy_forest_shadow_biter";
        private const string ShadowSeal = "enemy_mountain_shadow_seal";
        private const string ReedSnare = "enemy_forest_reed_snare";
        private const string SmokeRope = "enemy_forest_smoke_rope";
        private const string NianCub = "enemy_forest_nian_cub";
        private const string BrokenTablet = "enemy_forest_broken_tablet";
        private const string Golem = "enemy_mountain_stone_golem";
        private const string Sleepwalker = "enemy_mountain_sleepwalker_giant";
        private const string ThiefMonkey = "enemy_forest_thief_monkey";
        private const string EmberGull = "enemy_mountain_ember_gull";
        private const string RollingStone = "enemy_mountain_rolling_stone";
        private const string CrownedOrc = "enemy_desert_crowned_orc";

        private const string FireflySwarm = "enemy_desert_firefly_swarm";
        private const string GardenShadow = "enemy_forest_garden_shadow";
        private const string LateShadow = "enemy_forest_late_shadow";
        private const string Mist = "enemy_mountain_ill_wind_mist";
        private const string BrokenLantern = "enemy_desert_broken_lantern";
        private const string GlowMantle = "enemy_forest_glow_mantle";
        private const string Ooze = "enemy_cave_ooze";
        private const string SmokeGoat = "enemy_forest_smoke_goat";
        private const string StormEagle = "enemy_mountain_storm_eagle";
        private const string AngryCedar = "enemy_forest_angry_cedar";
        private const string BronzeWolf = "enemy_mountain_bronze_wolf";
        private const string CreepingDark = "enemy_forest_creeping_dark";
        private const string SmokeHarpy = "enemy_mountain_smoke_harpy";

        private const string CeuId = OverworldLayouts.CeuRegionId;
        private const bool Leads = true;
        private const bool Joins = false;

        private static Placement P(string enemy, string area, string group, bool withNightForm, params string[] standIns) =>
            new Placement(CeuId, enemy, area, group, withNightForm, standIns);

        private static readonly Placement[] Placements =
        {
            P(CloudBird, "pier_da_lua", "aves_da_lua", Leads, Ibis),
            P(CloudBird, "cais_da_coluna", "aves_do_cais", Leads, Ibis),
            P(CloudBird, "fresta_das_luzinhas", "aves_da_fresta", Leads, Ibis),
            P(CloudBird, "beira_oeste", "arcontes_da_beira", Joins, Ibis),
            P(CloudBird, "pier_da_lua", "sombras_clandestinas", Joins, Ibis),
            P(CloudBird, "cais_da_coluna", "faiscas_do_cais", Joins, Ibis),

            P(LightFish, "foz_oeste", "peixes_apagados", Leads, LanternFish),
            P(LightFish, "ilhotas_de_nuvem", "peixes_das_ilhas", Leads, LanternFish),
            P(LightFish, "rio_de_luz", "peixes_do_rio", Leads, LanternFish),
            P(LightFish, "foz_oeste", "sombras_nadadoras", Joins, LanternFish),
            P(LightFish, "rio_de_luz", "correntes_erradas", Joins, LanternFish),
            P(LightFish, "foz_leste", "lanternas_da_foz", Joins, LanternFish),

            P(DarkStowaway, "proa_da_lua", "clandestinos_da_proa", Leads, ShadowBiter),
            P(DarkStowaway, "mastro_da_lua", "clandestinos_do_mastro", Leads, ShadowBiter),
            P(DarkStowaway, "conves_da_lua", "clandestinos_de_treva", Leads, ShadowBiter),
            P(DarkStowaway, "proa_da_lua", "cordas_da_proa", Joins, ShadowBiter),
            P(DarkStowaway, "mastro_da_lua", "faiscas_teimosas", Joins, ShadowBiter),
            P(DarkStowaway, "popa_da_lua", "cordas_da_popa", Joins, ShadowBiter),

            P(HoldShadow, "porao_da_lua", "sombras_do_porao", Leads, ShadowSeal),
            P(HoldShadow, "popa_da_lua", "sombras_da_popa", Leads, ShadowSeal),
            P(HoldShadow, "conves_da_lua", "clandestinos_de_treva", Joins, ShadowSeal),
            P(HoldShadow, "porao_da_lua", "cordas_vivas", Joins, ShadowSeal),

            P(LivingRope, "proa_da_lua", "cordas_da_proa", Leads, ReedSnare),
            P(LivingRope, "porao_da_lua", "cordas_vivas", Leads, ReedSnare),
            P(LivingRope, "popa_da_lua", "cordas_da_popa", Leads, ReedSnare),
            P(LivingRope, "proa_da_lua", "clandestinos_da_proa", Joins, ReedSnare),
            P(LivingRope, "conves_da_lua", "faiscas_do_conves", Joins, ReedSnare),
            P(LivingRope, "porao_da_lua", "sombras_do_porao", Joins, ReedSnare),

            P(ToyArchon, "jarras_oeste", "doze_oeste", Leads, NianCub),
            P(ToyArchon, "roda_celeste", "doze_da_roda", Leads, NianCub),
            P(ToyArchon, "jarras_leste", "doze_leste", Leads, NianCub),
            P(ToyArchon, "engrenagens_oeste", "gotas_do_oeste", Joins, NianCub),
            P(ToyArchon, "jarras_oeste", "gotas_das_jarras", Joins, NianCub),
            P(ToyArchon, "roda_celeste", "jarras_rachadas", Joins, NianCub),
            P(ToyArchon, "engrenagens_leste", "jarras_rachadas_leste", Joins, NianCub),

            P(CrackedJar, "engrenagens_oeste", "jarras_rachadas_oeste", Leads, BrokenTablet),
            P(CrackedJar, "roda_celeste", "jarras_rachadas", Leads, BrokenTablet),
            P(CrackedJar, "jarras_leste", "jarras_do_leste", Leads, BrokenTablet),
            P(CrackedJar, "engrenagens_leste", "jarras_rachadas_leste", Leads, BrokenTablet),
            P(CrackedJar, "jarras_oeste", "doze_oeste", Joins, BrokenTablet),
            P(CrackedJar, "jarras_leste", "doze_leste", Joins, BrokenTablet),
            P(CrackedJar, "engrenagens_leste", "gotas_do_leste", Joins, BrokenTablet),

            P(ChainedArchon, "orbita_oeste", "arcontes_acorrentados", Leads, Golem),
            P(ChainedArchon, "esferas_dos_arcontes", "arcontes_da_ilhota", Leads, Golem),
            P(ChainedArchon, "orbita_vazia_leste", "arcontes_do_leste", Leads, Golem),
            P(ChainedArchon, "orbita_oeste", "fumacas_de_esfera", Joins, Golem),
            P(ChainedArchon, "esferas_dos_arcontes", "ventos_da_ilhota", Joins, Golem),

            P(BalloonGiant, "orbita_vazia_oeste", "gigantes_balao_oeste", Leads, Sleepwalker),
            P(BalloonGiant, "orbita_leste", "gigantes_balao", Leads, Sleepwalker),
            P(BalloonGiant, "orbita_leste", "fumacas_do_leste", Joins, Sleepwalker),
            P(BalloonGiant, "orbita_vazia_leste", "ventos_de_orbita", Joins, Sleepwalker),

            P(AzThief, "pomar_de_luz", "ladroes_do_pomar", Leads, ThiefMonkey),
            P(AzThief, "pergola_do_jardineiro", "ladroes_da_pergola", Leads, ThiefMonkey),
            P(AzThief, "jardim_das_flores", "espinhos_das_flores", Joins, ThiefMonkey),
            P(AzThief, "pergola_do_jardineiro", "sombras_da_pergola", Joins, ThiefMonkey),

            P(DimmedGull, "conves_do_sol", "gaivotas_de_luz", Leads, EmberGull),
            P(DimmedGull, "conves_do_sol", "clandestinos_de_brasa", Joins, EmberGull),
            P(DimmedGull, "vela_do_sol", "cordas_de_fumaca", Joins, EmberGull),

            P(SkyStone, "rachaduras_do_oeste", "pedras_do_ceu", Leads, RollingStone),
            P(SkyStone, "varanda_do_tempo_de_depois", "pedras_do_pomar", Leads, RollingStone),
            P(SkyStone, "rachaduras_do_oeste", "rachaduras_do_ceu", Joins, RollingStone),
            P(SkyStone, "mirante_do_ceu", "ventos_do_mirante", Joins, RollingStone),
            P(SkyStone, "varanda_do_tempo_de_depois", "ventos_do_pomar", Joins, RollingStone),

            P(LesserArchon, "beira_oeste", "arcontes_da_beira", Leads, CrownedOrc),
            P(LesserArchon, "pier_do_sol", "arcontes_do_sol", Leads, CrownedOrc),
            P(LesserArchon, "pes_da_coluna", "arcontes_do_degrau", Leads, CrownedOrc),
            P(LesserArchon, "mirante_do_ceu", "arcontes_fugidos", Leads, CrownedOrc),
            P(LesserArchon, "pier_do_sol", "faiscas_do_sol", Joins, CrownedOrc),
            P(LesserArchon, "fresta_das_luzinhas", "aves_da_fresta", Joins, CrownedOrc),
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
            OverworldLayouts.Get(CeuId).Zones.SelectMany(z => z.Fauna).First(f => f.Id == id);

        [TestCase("sombras_clandestinas", GardenShadow)]
        [TestCase("sombras_da_fresta", GardenShadow)]
        [TestCase("sombras_das_flores", GardenShadow)]
        [TestCase("sombras_da_pergola", GardenShadow)]
        [TestCase("sombras_nadadoras", LateShadow)]
        [TestCase("sombras_das_margens", LateShadow)]
        [TestCase("sombras_do_meio_dia", LateShadow)]
        [TestCase("correntes_erradas", Mist)]
        [TestCase("correntes_da_foz", Mist)]
        [TestCase("rachaduras_do_ceu", Mist)]
        [TestCase("faiscas_teimosas", GlowMantle)]
        [TestCase("faiscas_do_conves", GlowMantle)]
        [TestCase("fumacas_de_esfera", SmokeGoat)]
        [TestCase("fumacas_do_leste", SmokeGoat)]
        [TestCase("espinhos_das_flores", AngryCedar)]
        [TestCase("espinhos_do_pomar", AngryCedar)]
        [TestCase("clandestinos_de_brasa", BronzeWolf)]
        [TestCase("ultimas_sombras", CreepingDark)]
        [TestCase("sombras_dos_pes", CreepingDark)]
        [TestCase("ventos_do_alto", SmokeHarpy)]
        public void TheOtherSheets_KeepTheirStandIn(string group, string standIn)
        {
            OverworldFauna fauna = Group(group);
            Assert.IsTrue(fauna.Day.Any(e => e.EnemyId == standIn), $"{group}: still {standIn} by day");
            Assert.IsTrue(fauna.Night.Any(e => e.EnemyId == standIn), $"{group}: still {standIn} at night");
        }

        [Test]
        public void TheSmokeRope_IsTheCompanionOfTheNoonShadows()
        {
            OverworldFauna fauna = Group("sombras_do_meio_dia");
            Assert.IsTrue(fauna.Day.Any(e => e.EnemyId == SmokeRope), "the Corda de Fumaça comes along by day");
            Assert.IsFalse(fauna.Day.Any(e => e.EnemyId == ReedSnare), "no reed snare by day any more");
            Assert.IsFalse(fauna.Day.Any(e => e.EnemyId == LivingRope), "not the Corda Viva");
        }

        [TestCase(OverworldLayouts.RioDeLuzMapId, CloudBird)]
        [TestCase(OverworldLayouts.BarcaDaLuaMapId, CloudBird)]
        [TestCase(OverworldLayouts.CaisDaColunaMapId, LightFish)]
        [TestCase(OverworldLayouts.BarcaDaLuaMapId, LightFish)]
        [TestCase(OverworldLayouts.RioDeLuzMapId, DarkStowaway)]
        [TestCase(OverworldLayouts.BarcaDoSolMapId, DarkStowaway)]
        [TestCase(OverworldLayouts.RodaCelesteMapId, HoldShadow)]
        [TestCase(OverworldLayouts.BarcaDoSolMapId, LivingRope)]
        [TestCase(OverworldLayouts.RodaCelesteMapId, LivingRope)]
        [TestCase(OverworldLayouts.BarcaDaLuaMapId, ToyArchon)]
        [TestCase(OverworldLayouts.EsferasArcontesMapId, ToyArchon)]
        [TestCase(OverworldLayouts.EsferasArcontesMapId, CrackedJar)]
        [TestCase(OverworldLayouts.JardimDeLuzMapId, ChainedArchon)]
        [TestCase(OverworldLayouts.RodaCelesteMapId, BalloonGiant)]
        [TestCase(OverworldLayouts.BarcaDoSolMapId, AzThief)]
        [TestCase(OverworldLayouts.JardimDeLuzMapId, DimmedGull)]
        [TestCase(OverworldLayouts.PesDaColunaMapId, SkyStone)]
        [TestCase(OverworldLayouts.BarcaDoSolMapId, SkyStone)]
        [TestCase(OverworldLayouts.RodaCelesteMapId, LesserArchon)]
        public void TheBeast_StaysOutOfTheMapsItsSheetDoesNotName(string map, string beast)
        {
            OverworldLayout w = OverworldLayouts.Get(CeuId);
            List<MapSpawnZone> clearings = w.World.Zones.Where(s => w.ZoneOfSpawn[s.Id].MapIdOrOwner == map).ToList();
            Assert.IsNotEmpty(clearings, map);
            Assert.IsFalse(clearings.Any(s => s.Day.Concat(s.Night).Any(e => e.EnemyId == beast)), $"no {beast} in {map}");
        }

        [Test]
        public void NoStandIn_SpawnsByDay_AnywhereInTheSky()
        {
            OverworldLayout w = OverworldLayouts.Get(CeuId);
            List<string> day = w.World.Zones.SelectMany(s => s.Day).Select(e => e.EnemyId).ToList();
            foreach (string standIn in new[] { Ibis, LanternFish, ShadowBiter, ShadowSeal, NianCub, BrokenTablet, Golem, Sleepwalker, ThiefMonkey, EmberGull, RollingStone, CrownedOrc })
            {
                Assert.IsFalse(day.Contains(standIn), $"no {standIn} by day in the Sky any more");
            }
        }
    }
}
