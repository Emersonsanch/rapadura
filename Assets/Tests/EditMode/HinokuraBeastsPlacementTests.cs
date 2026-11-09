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
    /// The sixth pass of beasts (<see cref="HinokuraBeastsTests"/>: Arquipélago de Hinokura) now stand where their stand-ins
    /// stood (Docs/Mapas100.md 65–70), same idea as <see cref="MarGeladoBeastsPlacementTests"/>: each one is in the day table of
    /// every group its sheet gives it (leading the group or as its companion), its night form in the same group's night table
    /// where the beast leads it, the stand-in left those groups' day tables, and every case is checked on the clearings the
    /// group really got (each group listed here draws at least one clearing in its area), at that area's levels. The sand snake
    /// and the ember vulture stay on as the Devouring Salamander and the Volcanic Bomb: the lava lizard stays out of the caldera
    /// (70) and the ember gull off Mount Echeyde (67).
    /// </summary>
    public class HinokuraBeastsPlacementTests
    {
        private const string VolcanoCrab = "enemy_mountain_volcano_crab";
        private const string LavaLizard = "enemy_mountain_lava_lizard";
        private const string EmberGull = "enemy_mountain_ember_gull";
        private const string Shisa = "enemy_mountain_dimmed_shisa";
        private const string Tanuki = "enemy_mountain_tanuki";
        private const string Tibicena = "enemy_mountain_tibicena";
        private const string GiantLizard = "enemy_mountain_giant_lava_lizard";
        private const string BladeFish = "enemy_mountain_blade_fish";
        private const string EmberCoral = "enemy_mountain_ember_coral";
        private const string Ushabti = "enemy_mountain_lacquer_ushabti";
        private const string Ogre = "enemy_mountain_furnace_ogre";
        private const string SlagGiant = "enemy_mountain_slag_giant";

        private const string CoconutCrab = "enemy_forest_coconut_crab";
        private const string SandSnake = "enemy_desert_sand_snake";
        private const string Vulture = "enemy_desert_ember_vulture";
        private const string StoneLion = "enemy_forest_stone_lion";
        private const string Otter = "enemy_forest_thief_otter";
        private const string Jackal = "enemy_desert_jackal";
        private const string Crocodile = "enemy_forest_mangrove_crocodile";
        private const string Pike = "enemy_mountain_giant_pike";
        private const string Lantern = "enemy_desert_broken_lantern";
        private const string Terracotta = "enemy_forest_terracotta_soldier";
        private const string Troll = "enemy_forest_dimmed_troll";
        private const string Golem = "enemy_mountain_stone_golem";

        private const string Fogo = OverworldLayouts.HinokuraRegionId;
        private const bool Leads = true;
        private const bool Joins = false;

        private static Placement P(string enemy, string area, string group, bool withNightForm, params string[] standIns) =>
            new Placement(Fogo, enemy, area, group, withNightForm, standIns);

        private static readonly Placement[] Placements =
        {
            P(VolcanoCrab, "praia_de_areia_negra", "caranguejos_vulcao", Leads, CoconutCrab),
            P(VolcanoCrab, "costa_das_fumarolas", "caranguejos_das_fumarolas", Leads, CoconutCrab),
            P(VolcanoCrab, "estreito_do_degelo", "caranguejos_do_degelo", Leads, CoconutCrab),
            P(VolcanoCrab, "hinokura", "lagartos_de_lava", Joins, CoconutCrab),
            P(VolcanoCrab, "praia_de_areia_negra", "gaivotas_da_praia", Joins, CoconutCrab),
            P(VolcanoCrab, "fontes_termais", "vapores_das_fontes", Joins, CoconutCrab),

            P(LavaLizard, "hinokura", "lagartos_de_lava", Leads, SandSnake),
            P(LavaLizard, "canais_de_lava", "lagartos_dos_canais", Leads, SandSnake),
            P(LavaLizard, "fontes_termais", "lagartos_das_fontes", Leads, SandSnake),
            P(LavaLizard, "hinokura", "gaivotas_brasa", Joins, SandSnake),
            P(LavaLizard, "canais_de_lava", "pedras_pomes_dos_canais", Joins, SandSnake),

            P(EmberGull, "hinokura", "gaivotas_brasa", Leads, Vulture),
            P(EmberGull, "praia_de_areia_negra", "gaivotas_da_praia", Leads, Vulture),
            P(EmberGull, "estreito_do_degelo", "gaivotas_do_estreito", Leads, Vulture),
            P(EmberGull, "praia_de_areia_negra", "caranguejos_vulcao", Joins, Vulture),

            P(Shisa, "jardins_de_lava_laqueada", "shisas_apagadas", Leads, StoneLion),
            P(Shisa, "pontes_vermelhas", "shisas_das_pontes", Leads, StoneLion),

            P(Tanuki, "jardins_de_lava_laqueada", "tanukis_ladroes", Leads, Otter),
            P(Tanuki, "pinhais_de_nuvem", "tanukis_dos_pinhais", Leads, Otter),
            P(Tanuki, "jardins_de_lava_laqueada", "shisas_apagadas", Joins, Otter),
            P(Tanuki, "pinhais_de_nuvem", "bambus_de_fumaca", Joins, Otter),
            P(Tanuki, "pontes_vermelhas", "carpas_de_brasa", Joins, Otter),

            P(Tibicena, "campos_de_retama", "tibicenas_da_retama", Leads, Jackal),
            P(Tibicena, "monte_echeyde", "tibicenas", Leads, Jackal),
            P(Tibicena, "campos_de_retama", "cabras_dos_pastores", Joins, Jackal),
            P(Tibicena, "malpais_de_echeyde", "lagartos_gigantes", Joins, Jackal),

            P(GiantLizard, "malpais_de_echeyde", "lagartos_gigantes", Leads, Crocodile),
            P(GiantLizard, "malpais_de_echeyde", "fumarolas", Joins, Crocodile),

            P(BladeFish, "canais_quentes", "peixes_lamina", Leads, Pike),
            P(BladeFish, "ilhotas_de_vidro", "peixes_das_ilhotas", Leads, Pike),
            P(BladeFish, "canais_quentes", "polvos_dos_canais", Joins, Pike),
            P(BladeFish, "ilhotas_de_vidro", "ouricos_de_vidro", Joins, Pike),

            P(EmberCoral, "recife_de_obsidiana", "corais_de_brasa", Leads, Lantern),
            P(EmberCoral, "recife_de_obsidiana", "polvos_de_obsidiana", Joins, Lantern),
            P(EmberCoral, "ilhotas_de_vidro", "peixes_das_ilhotas", Joins, Lantern),

            P(Ushabti, "muralhas_curvas", "ushabtis_de_laca", Leads, Terracotta),
            P(Ushabti, "escoria_velha", "ushabtis_da_escoria", Leads, Terracotta),
            P(Ushabti, "muralhas_curvas", "corvos_das_muralhas", Joins, Terracotta),
            P(Ushabti, "castelo_das_nove_fornalhas", "ogros_fornalha", Joins, Terracotta),
            P(Ushabti, "escoria_velha", "faiscas_da_escoria", Joins, Terracotta),

            P(Ogre, "castelo_das_nove_fornalhas", "ogros_fornalha", Leads, Troll),
            P(Ogre, "patio_das_fornalhas", "nians_das_fornalhas", Joins, Troll),

            P(SlagGiant, "caldeira_do_fogo_devorador", "gigantes_de_escoria", Leads, Golem),
            P(SlagGiant, "borda_fria", "gigantes_da_borda", Leads, Golem),
            P(SlagGiant, "mar_de_cinza", "gigantes_do_mar_de_cinza", Leads, Golem),
            P(SlagGiant, "encosta_da_caldeira", "salamandras_devoradoras", Joins, Golem),
            P(SlagGiant, "borda_fria", "chamas_da_borda", Joins, Golem),
            P(SlagGiant, "campos_de_cinza", "corvos_da_cinza", Joins, Golem),
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
            OverworldLayouts.Get(Fogo).Zones.SelectMany(z => z.Fauna).First(f => f.Id == id);

        [TestCase("salamandras_devoradoras")]
        [TestCase("salamandras_das_cinzas")]
        [TestCase("salamandras_do_mar")]
        public void TheDevouringSalamanders_KeepTheirStandIn(string group)
        {
            OverworldFauna fauna = Group(group);
            Assert.IsTrue(fauna.Day.Any(e => e.EnemyId == SandSnake), $"{group}: still the sand snake by day");
            Assert.IsTrue(fauna.Night.Any(e => e.EnemyId == SandSnake), $"{group}: still the sand snake at night");
        }

        [Test]
        public void TheVolcanicBombs_KeepTheirStandIn()
        {
            OverworldFauna fauna = Group("bombas_vulcanicas");
            Assert.IsTrue(fauna.Day.Any(e => e.EnemyId == Vulture), "the bombs are still the ember vulture by day");
            Assert.IsTrue(fauna.Night.Any(e => e.EnemyId == Vulture), "the bombs are still the ember vulture at night");
            Assert.IsFalse(fauna.Day.Concat(fauna.Night).Any(e => e.EnemyId == EmberGull), "no ember gull among the bombs");
        }

        [TestCase(OverworldLayouts.CaldeiraMapId, LavaLizard)]
        [TestCase(OverworldLayouts.MonteEcheydeMapId, EmberGull)]
        public void TheBeast_StaysOutOfTheMapsItsStandInStillWorks(string map, string beast)
        {
            OverworldLayout w = OverworldLayouts.Get(Fogo);
            List<MapSpawnZone> clearings = w.World.Zones.Where(s => w.ZoneOfSpawn[s.Id].MapIdOrOwner == map).ToList();
            Assert.IsNotEmpty(clearings, map);
            Assert.IsFalse(clearings.Any(s => s.Day.Concat(s.Night).Any(e => e.EnemyId == beast)), $"no {beast} in {map}");
        }

        [Test]
        public void NoStandIn_SpawnsByDay_AnywhereOnTheArchipelago()
        {
            OverworldLayout w = OverworldLayouts.Get(Fogo);
            List<string> day = w.World.Zones.SelectMany(s => s.Day).Select(e => e.EnemyId).ToList();
            foreach (string standIn in new[] { CoconutCrab, StoneLion, Otter, Jackal, Crocodile, Pike, Lantern, Terracotta, Troll, Golem })
            {
                Assert.IsFalse(day.Contains(standIn), $"no {standIn} by day on the archipelago any more");
            }
        }
    }
}
