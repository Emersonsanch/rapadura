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
    /// The eighth pass of beasts (<see cref="MisturaBeastsTests"/>: Margens da Mistura) now stand where their stand-ins stood
    /// (Docs/Mapas100.md 77–82), same idea as <see cref="RioBeastsPlacementTests"/>: each one is in the day table of every group of its
    /// map where it leads or comes along, its night form in the same group's night table where the beast leads it, the stand-in left
    /// those groups' day tables, and every case is checked on the clearings the group really got, at that area's levels. The sheets the
    /// pass does not cover keep their stand-ins (the Homem-Escorpião, the Lagarto do Instante, the Ushabti de Relógio, the Bagre Negro, the
    /// Fumaça de Piche, the Lince do Cedro, the Abelha-Resina, the Pedra de Diorito, the Ave do Ombro, the Tremor, the Pássaro-Sonho, the
    /// Água que Apaga), and each new beast stays out of the maps its sheet does not name.
    /// </summary>
    public class MisturaBeastsPlacementTests
    {
        private const string BrickLion = "enemy_forest_brick_lion";
        private const string Mushussu = "enemy_forest_young_mushussu";
        private const string Locust = "enemy_forest_bitumen_locust";
        private const string Hourglass = "enemy_forest_living_hourglass";
        private const string LateShadow = "enemy_forest_late_shadow";
        private const string LivingTar = "enemy_forest_living_tar";
        private const string TarHeron = "enemy_forest_tar_heron";
        private const string AngryCedar = "enemy_forest_angry_cedar";
        private const string GlowMantle = "enemy_forest_glow_mantle";
        private const string UllikummiSon = "enemy_forest_ullikummi_son";
        private const string BrokenTablet = "enemy_forest_broken_tablet";
        private const string GardenShadow = "enemy_forest_garden_shadow";

        private const string StoneLion = "enemy_forest_stone_lion";
        private const string Griffin = "enemy_mountain_young_griffin";
        private const string Beetle = "enemy_mountain_bark_beetle";
        private const string Golem = "enemy_mountain_stone_golem";
        private const string Mist = "enemy_mountain_ill_wind_mist";
        private const string Ooze = "enemy_cave_ooze";
        private const string EmberGull = "enemy_mountain_ember_gull";
        private const string Troll = "enemy_forest_dimmed_troll";
        private const string BrokenLantern = "enemy_desert_broken_lantern";
        private const string SlagGiant = "enemy_mountain_slag_giant";
        private const string Terracotta = "enemy_forest_terracotta_soldier";
        private const string Sleepwalker = "enemy_mountain_sleepwalker_giant";

        private const string Scorpion = "enemy_desert_scorpion";
        private const string LavaLizard = "enemy_mountain_lava_lizard";
        private const string LacquerUshabti = "enemy_mountain_lacquer_ushabti";
        private const string Pike = "enemy_mountain_giant_pike";
        private const string SmokeHarpy = "enemy_mountain_smoke_harpy";
        private const string Irbis = "enemy_mountain_irbis";
        private const string GoldenBee = "enemy_forest_golden_bee";
        private const string RollingStone = "enemy_mountain_rolling_stone";
        private const string StormEagle = "enemy_mountain_storm_eagle";
        private const string ShroudOwl = "enemy_forest_shroud_owl";
        private const string ShadowSeal = "enemy_mountain_shadow_seal";

        private const string MisturaId = OverworldLayouts.MisturaRegionId;
        private const bool Leads = true;
        private const bool Joins = false;

        private static Placement P(string enemy, string area, string group, bool withNightForm, params string[] standIns) =>
            new Placement(MisturaId, enemy, area, group, withNightForm, standIns);

        private static readonly Placement[] Placements =
        {
            P(BrickLion, "dur_jarras", "leoes_do_terraco", Leads, StoneLion),
            P(BrickLion, "portao_de_tijolo", "leoes_do_portao", Leads, StoneLion),
            P(BrickLion, "olarias", "leoes_das_olarias", Leads, StoneLion),
            P(BrickLion, "dur_jarras", "gafanhotos_do_terraco", Joins, StoneLion),
            P(BrickLion, "colinas_de_tell", "homens_escorpiao_do_tell", Joins, StoneLion),
            P(BrickLion, "terracos_de_cevada", "mushussu_da_cevada", Joins, StoneLion),

            P(Mushussu, "canal_das_palmeiras", "mushussu_do_canal", Leads, Griffin),
            P(Mushussu, "terracos_de_cevada", "mushussu_da_cevada", Leads, Griffin),
            P(Mushussu, "terracos_de_cevada", "gafanhotos_da_cevada", Joins, Griffin),

            P(Locust, "dur_jarras", "gafanhotos_do_terraco", Leads, Beetle),
            P(Locust, "olarias", "gafanhotos_das_olarias", Leads, Beetle),
            P(Locust, "colinas_de_tell", "gafanhotos_do_tell", Leads, Beetle),
            P(Locust, "terracos_de_cevada", "gafanhotos_da_cevada", Leads, Beetle),
            P(Locust, "dur_jarras", "leoes_do_terraco", Joins, Beetle),
            P(Locust, "portao_de_tijolo", "homens_escorpiao_do_portao", Joins, Beetle),
            P(Locust, "olarias", "leoes_das_olarias", Joins, Beetle),
            P(Locust, "canal_das_palmeiras", "mushussu_do_canal", Joins, Beetle),
            P(Locust, "canal_das_palmeiras", "homens_escorpiao_do_canal", Joins, Beetle),

            P(Hourglass, "deserto_do_relogio", "ampulhetas_vivas", Leads, Golem),
            P(Hourglass, "areia_das_horas", "lagartos_das_horas", Joins, Golem),

            P(LateShadow, "sombras_atrasadas", "sombras_atrasadas", Leads, Mist),
            P(LateShadow, "deserto_do_relogio", "lagartos_do_instante", Joins, Mist),
            P(LateShadow, "areia_das_horas", "ushabtis_das_horas", Joins, Mist),

            P(LivingTar, "pocos_de_betume", "betumes_vivos", Leads, Ooze),
            P(LivingTar, "pocas_de_piche", "betumes_das_pocas", Leads, Ooze),
            P(LivingTar, "pocas_de_piche", "fumacas_de_piche", Joins, Ooze),
            P(LivingTar, "juncal_de_piche", "garcas_de_piche", Joins, Ooze),

            P(TarHeron, "juncal_de_piche", "garcas_de_piche", Leads, EmberGull),
            P(TarHeron, "pocas_de_piche", "betumes_das_pocas", Joins, EmberGull),
            P(TarHeron, "juncal_de_piche", "fumacas_do_juncal", Joins, EmberGull),

            P(AngryCedar, "cedral_de_hobabish", "cedros_raivosos", Leads, Troll),
            P(AngryCedar, "cedros_anciaos", "cedros_anciaos_raivosos", Leads, Troll),

            P(GlowMantle, "cedral_de_hobabish", "mantos_de_brilho", Leads, BrokenLantern),
            P(GlowMantle, "cedros_anciaos", "mantos_dos_anciaos", Leads, BrokenLantern),
            P(GlowMantle, "cedros_anciaos", "cedros_anciaos_raivosos", Joins, BrokenLantern),

            P(UllikummiSon, "diorito_crescente", "filhos_de_ullikummi", Leads, SlagGiant),
            P(UllikummiSon, "beiral_do_ceu", "filhos_do_beiral", Leads, SlagGiant),
            P(UllikummiSon, "diorito_crescente", "pedras_do_diorito", Joins, SlagGiant),
            P(UllikummiSon, "beiral_do_ceu", "aves_do_beiral", Joins, SlagGiant),

            P(BrokenTablet, "campo_das_tabuas", "tabuas_do_campo", Leads, Terracotta),
            P(BrokenTablet, "tabuas_quebradas", "tabuas_quebradas", Leads, Terracotta),
            P(BrokenTablet, "campo_das_tabuas", "passaros_sonho", Joins, Terracotta),
            P(BrokenTablet, "tabuas_quebradas", "aguas_das_tabuas", Joins, Terracotta),
            P(BrokenTablet, "tabuas_do_sonho", "passaros_do_sonho", Joins, Terracotta),

            P(GardenShadow, "jardim_cortado", "jardins_sombra", Leads, Sleepwalker),
            P(GardenShadow, "agua_parada", "jardins_da_agua", Leads, Sleepwalker),
            P(GardenShadow, "tabuas_do_sonho", "jardins_do_sonho", Leads, Sleepwalker),
            P(GardenShadow, "jardim_cortado", "passaros_do_jardim", Joins, Sleepwalker),
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
            OverworldLayouts.Get(MisturaId).Zones.SelectMany(z => z.Fauna).First(f => f.Id == id);

        [TestCase("ushabtis_de_relogio", LacquerUshabti)]
        [TestCase("ushabtis_das_horas", LacquerUshabti)]
        [TestCase("fumacas_de_piche", SmokeHarpy)]
        [TestCase("fumacas_do_juncal", SmokeHarpy)]
        public void TheOtherSheets_KeepTheirStandIn(string group, string standIn)
        {
            OverworldFauna fauna = Group(group);
            Assert.IsTrue(fauna.Day.Any(e => e.EnemyId == standIn), $"{group}: still {standIn} by day");
            Assert.IsTrue(fauna.Night.Any(e => e.EnemyId == standIn), $"{group}: still {standIn} at night");
        }

        [TestCase(OverworldLayouts.DesertoRelogioMapId, BrickLion)]
        [TestCase(OverworldLayouts.DesertoRelogioMapId, Locust)]
        [TestCase(OverworldLayouts.PocosBetumeMapId, Locust)]
        [TestCase(OverworldLayouts.DurJarrasMapId, Hourglass)]
        [TestCase(OverworldLayouts.DurJarrasMapId, LateShadow)]
        [TestCase(OverworldLayouts.CedralHobabishMapId, LivingTar)]
        [TestCase(OverworldLayouts.CedralHobabishMapId, TarHeron)]
        [TestCase(OverworldLayouts.OmbroUpelluriMapId, AngryCedar)]
        [TestCase(OverworldLayouts.TabuasSonhoMapId, GlowMantle)]
        [TestCase(OverworldLayouts.TabuasSonhoMapId, UllikummiSon)]
        [TestCase(OverworldLayouts.OmbroUpelluriMapId, BrokenTablet)]
        [TestCase(OverworldLayouts.OmbroUpelluriMapId, GardenShadow)]
        public void TheBeast_StaysOutOfTheMapsItsSheetDoesNotName(string map, string beast)
        {
            OverworldLayout w = OverworldLayouts.Get(MisturaId);
            List<MapSpawnZone> clearings = w.World.Zones.Where(s => w.ZoneOfSpawn[s.Id].MapIdOrOwner == map).ToList();
            Assert.IsNotEmpty(clearings, map);
            Assert.IsFalse(clearings.Any(s => s.Day.Concat(s.Night).Any(e => e.EnemyId == beast)), $"no {beast} in {map}");
        }

        [Test]
        public void NoStandIn_SpawnsByDay_AnywhereOnTheMargins()
        {
            OverworldLayout w = OverworldLayouts.Get(MisturaId);
            List<string> day = w.World.Zones.SelectMany(s => s.Day).Select(e => e.EnemyId).ToList();
            foreach (string standIn in new[] { StoneLion, Griffin, Beetle, Mist, Ooze, EmberGull, Troll, BrokenLantern, SlagGiant, Terracotta, Sleepwalker })
            {
                Assert.IsFalse(day.Contains(standIn), $"no {standIn} by day on the Margins any more");
            }
        }

        [Test]
        public void TheGolem_LeftTheShoulder_TheTremorIsItsOwnBeast()
        {
            OverworldLayout w = OverworldLayouts.Get(MisturaId);
            Assert.IsFalse(w.World.Zones.SelectMany(s => s.Day).Any(e => e.EnemyId == Golem), "the Tremor (81) is its own beast now (BorrowedRoleBeastsTests)");
        }
    }
}
