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
    /// The seventh pass of beasts (<see cref="RioBeastsTests"/>: Rio das Doze Horas) now stand where their stand-ins stood
    /// (Docs/Mapas100.md 71–74, the scarab also in 76), same idea as <see cref="HinokuraBeastsPlacementTests"/>: each one is in the day
    /// table of every group its sheet gives it (leading the group or as its companion), its night form in the same group's night
    /// table where the beast leads it, the stand-in left those groups' day tables, and every case is checked on the clearings the
    /// group really got, at that area's levels. The sheets the pass does not cover keep their stand-ins (the Ganso Ladrão, the Chama
    /// Faminta, the Salamandra de Sombra, the Filhos de Apep, the Escorpião of 74, the Ushabti Apagado, the Sombra Fervida, the Areia
    /// Movediça, the Crocodilo de Lodo), and the new beasts stay out of the maps where those stand-ins still work.
    /// </summary>
    public class RioBeastsPlacementTests
    {
        private const string Crocodile = "enemy_forest_dark_river_crocodile";
        private const string Ibis = "enemy_forest_dimmed_ibis";
        private const string Scarab = "enemy_forest_shadow_scarab";
        private const string Reedsnare = "enemy_forest_reed_snare";
        private const string Locust = "enemy_forest_shadow_locust";
        private const string Hippo = "enemy_forest_mud_hippo";
        private const string ReedSnake = "enemy_forest_reed_snake";
        private const string Serpent = "enemy_forest_three_headed_serpent";
        private const string Scorpion = "enemy_forest_black_scorpion";
        private const string SandUshabti = "enemy_forest_sand_ushabti";
        private const string Biter = "enemy_forest_shadow_biter";
        private const string Creeper = "enemy_forest_creeping_dark";

        private const string Mangrove = "enemy_forest_mangrove_crocodile";
        private const string Gull = "enemy_forest_thief_gull";
        private const string Beetle = "enemy_mountain_bark_beetle";
        private const string Mist = "enemy_mountain_ill_wind_mist";
        private const string Firefly = NightVariantRules.FireflySwarmEnemyId;
        private const string Buffalo = "enemy_forest_mud_buffalo";
        private const string SandSnake = "enemy_desert_sand_snake";
        private const string Python = "enemy_forest_dimmed_python";
        private const string DesertScorpion = "enemy_desert_scorpion";
        private const string RopeUshabti = "enemy_mountain_rope_ushabti";
        private const string Jackal = "enemy_desert_jackal";
        private const string Ooze = "enemy_cave_ooze";

        private const string RioId = OverworldLayouts.RioRegionId;
        private const bool Leads = true;
        private const bool Joins = false;

        private static Placement P(string enemy, string area, string group, bool withNightForm, params string[] standIns) =>
            new Placement(RioId, enemy, area, group, withNightForm, standIns);

        private static readonly Placement[] Placements =
        {
            P(Crocodile, "margem_de_papiro", "crocodilos_do_rio_escuro", Leads, Mangrove),
            P(Crocodile, "praia_dos_remadores", "crocodilos_da_praia", Leads, Mangrove),
            P(Crocodile, "canal_da_barca", "crocodilos_do_canal", Leads, Mangrove),
            P(Crocodile, "cais_de_mesektet", "ibis_apagadas", Joins, Mangrove),
            P(Crocodile, "pieres_das_horas", "juncos_dos_pieres", Joins, Mangrove),
            P(Crocodile, "canal_da_barca", "escaravelhos_do_canal", Joins, Mangrove),

            P(Ibis, "cais_de_mesektet", "ibis_apagadas", Leads, Gull),
            P(Ibis, "praia_dos_remadores", "ibis_da_praia", Leads, Gull),
            P(Ibis, "pieres_das_horas", "ibis_dos_pieres", Leads, Gull),
            P(Ibis, "cais_de_mesektet", "escaravelhos_do_cais", Joins, Gull),
            P(Ibis, "margem_de_papiro", "crocodilos_do_rio_escuro", Joins, Gull),

            P(Scarab, "cais_de_mesektet", "escaravelhos_do_cais", Leads, Beetle),
            P(Scarab, "foz_das_cinzas", "escaravelhos_da_foz", Leads, Beetle),
            P(Scarab, "canal_da_barca", "escaravelhos_do_canal", Leads, Beetle),
            P(Scarab, "portas_fechadas", "escaravelhos_das_portas", Leads, Beetle),
            P(Scarab, "zigue_zague_do_areal", "escaravelhos_do_zigue_zague", Leads, Beetle),
            P(Scarab, "corpo_de_apep", "escaravelhos_do_corpo", Leads, Beetle),
            P(Scarab, "rio_seco", "escaravelhos_do_rio_seco", Leads, Beetle),
            P(Scarab, "margem_de_papiro", "juncos_laco", Joins, Beetle),
            P(Scarab, "foz_das_cinzas", "abutres_da_cinza", Joins, Beetle),
            P(Scarab, "praia_dos_remadores", "crocodilos_da_praia", Joins, Beetle),
            P(Scarab, "pieres_das_horas", "ibis_dos_pieres", Joins, Beetle),
            P(Scarab, "areal_escondido", "escorpioes_negros", Joins, Beetle),
            P(Scarab, "portas_fechadas", "ushabtis_de_areia", Joins, Beetle),
            P(Scarab, "caminho_das_serpentes", "serpentes_do_caminho", Joins, Beetle),
            P(Scarab, "banco_de_areia_de_apep", "filhos_de_apep", Joins, Beetle),
            P(Scarab, "rio_seco", "crocodilos_do_rio_seco", Joins, Beetle),
            P(Scarab, "escamas_roxas", "areias_das_escamas", Joins, Beetle),

            P(Reedsnare, "margem_de_papiro", "juncos_laco", Leads, Mist),
            P(Reedsnare, "pieres_das_horas", "juncos_dos_pieres", Leads, Mist),
            P(Reedsnare, "foz_das_cinzas", "escaravelhos_da_foz", Joins, Mist),
            P(Reedsnare, "praia_dos_remadores", "ibis_da_praia", Joins, Mist),
            P(Reedsnare, "canal_da_barca", "crocodilos_do_canal", Joins, Mist),

            P(Locust, "campos_de_juncos", "gafanhotos_de_sombra", Leads, Firefly),
            P(Locust, "trigais_de_luz", "gafanhotos_dos_trigais", Leads, Firefly),
            P(Locust, "canteiros_de_juncos", "cobras_do_junco", Joins, Firefly),
            P(Locust, "canais_de_sekhet", "hipopotamos_dos_canais", Joins, Firefly),

            P(Hippo, "canteiros_de_juncos", "hipopotamos_de_lodo", Leads, Buffalo),
            P(Hippo, "canais_de_sekhet", "hipopotamos_dos_canais", Leads, Buffalo),
            P(Hippo, "trigais_de_luz", "gansos_dos_trigais", Joins, Buffalo),
            P(Hippo, "canais_de_sekhet", "cobras_dos_canais", Joins, Buffalo),

            P(ReedSnake, "canteiros_de_juncos", "cobras_do_junco", Leads, SandSnake),
            P(ReedSnake, "canais_de_sekhet", "cobras_dos_canais", Leads, SandSnake),
            P(ReedSnake, "campos_de_juncos", "gafanhotos_de_sombra", Joins, SandSnake),
            P(ReedSnake, "campos_de_juncos", "gansos_ladroes", Joins, SandSnake),
            P(ReedSnake, "canteiros_de_juncos", "hipopotamos_de_lodo", Joins, SandSnake),

            P(Serpent, "areal_escondido", "serpentes_de_tres_cabecas", Leads, Python),
            P(Serpent, "caminho_das_serpentes", "serpentes_do_caminho", Leads, Python),
            P(Serpent, "zigue_zague_do_areal", "escorpioes_do_zigue_zague", Joins, Python),

            P(Scorpion, "areal_escondido", "escorpioes_negros", Leads, DesertScorpion),
            P(Scorpion, "zigue_zague_do_areal", "escorpioes_do_zigue_zague", Leads, DesertScorpion),
            P(Scorpion, "areal_escondido", "serpentes_de_tres_cabecas", Joins, DesertScorpion),
            P(Scorpion, "caminho_das_serpentes", "ushabtis_do_caminho", Joins, DesertScorpion),
            P(Scorpion, "zigue_zague_do_areal", "escaravelhos_do_zigue_zague", Joins, DesertScorpion),

            P(SandUshabti, "portas_fechadas", "ushabtis_de_areia", Leads, RopeUshabti),
            P(SandUshabti, "caminho_das_serpentes", "ushabtis_do_caminho", Leads, RopeUshabti),
            P(SandUshabti, "portas_fechadas", "escaravelhos_das_portas", Joins, RopeUshabti),

            P(Biter, "aneis_de_mehen", "mordedores_de_sombra", Leads, Jackal),
            P(Biter, "anel_externo", "mordedores_do_anel", Leads, Jackal),
            P(Biter, "aneis_de_mehen", "trevas_rastejantes", Joins, Jackal),
            P(Biter, "no_errado", "ushabtis_apagados", Joins, Jackal),

            P(Creeper, "aneis_de_mehen", "trevas_rastejantes", Leads, Ooze),
            P(Creeper, "no_errado", "trevas_do_no", Leads, Ooze),
            P(Creeper, "aneis_de_mehen", "mordedores_de_sombra", Joins, Ooze),
            P(Creeper, "anel_externo", "escorpioes_dos_aneis", Joins, Ooze),
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
            OverworldLayouts.Get(RioId).Zones.SelectMany(z => z.Fauna).First(f => f.Id == id);

        [TestCase("chamas_famintas_do_lago", Firefly)]
        [TestCase("salamandras_de_sombra", SandSnake)]
        [TestCase("escorpioes_dos_aneis", DesertScorpion)]
        [TestCase("ushabtis_apagados", RopeUshabti)]
        [TestCase("sombras_fervidas", Ooze)]
        [TestCase("sombras_da_brasa", Ooze)]
        [TestCase("crocodilos_de_lodo", Mangrove)]
        [TestCase("crocodilos_do_rio_seco", Mangrove)]
        public void TheOtherSheets_KeepTheirStandIn(string group, string standIn)
        {
            OverworldFauna fauna = Group(group);
            Assert.IsTrue(fauna.Day.Any(e => e.EnemyId == standIn), $"{group}: still {standIn} by day");
            Assert.IsTrue(fauna.Night.Any(e => e.EnemyId == standIn), $"{group}: still {standIn} at night");
        }

        [TestCase(OverworldLayouts.BancoApepMapId, Crocodile)]
        [TestCase(OverworldLayouts.BancoApepMapId, Serpent)]
        [TestCase(OverworldLayouts.BancoApepMapId, Creeper)]
        [TestCase(OverworldLayouts.CamposJuncosMapId, Ibis)]
        [TestCase(OverworldLayouts.AneisMehenMapId, Scorpion)]
        [TestCase(OverworldLayouts.AneisMehenMapId, SandUshabti)]
        [TestCase(OverworldLayouts.LagoDeFogoMapId, Locust)]
        [TestCase(OverworldLayouts.LagoDeFogoMapId, ReedSnake)]
        [TestCase(OverworldLayouts.LagoDeFogoMapId, Creeper)]
        public void TheBeast_StaysOutOfTheMapsItsStandInStillWorks(string map, string beast)
        {
            OverworldLayout w = OverworldLayouts.Get(RioId);
            List<MapSpawnZone> clearings = w.World.Zones.Where(s => w.ZoneOfSpawn[s.Id].MapIdOrOwner == map).ToList();
            Assert.IsNotEmpty(clearings, map);
            Assert.IsFalse(clearings.Any(s => s.Day.Concat(s.Night).Any(e => e.EnemyId == beast)), $"no {beast} in {map}");
        }

        [Test]
        public void NoStandIn_SpawnsByDay_AnywhereOnTheRiver()
        {
            OverworldLayout w = OverworldLayouts.Get(RioId);
            List<string> day = w.World.Zones.SelectMany(s => s.Day).Select(e => e.EnemyId).ToList();
            foreach (string standIn in new[] { Beetle, Mist, Buffalo, Jackal })
            {
                Assert.IsFalse(day.Contains(standIn), $"no {standIn} by day on the river any more");
            }
        }
    }
}
