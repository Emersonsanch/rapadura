using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rapadura.Gameplay.Night;
using Rapadura.Gameplay.World.Map;
using Rapadura.Gameplay.World.Map.Areas;
using Rapadura.Gameplay.World.Map.Overworld;

namespace Rapadura.Tests
{
    /// <summary>
    /// The second pass of beasts (<see cref="LateRegionBeastsTests"/>: Costa das Monções, Delta de Jhilpur, Teto do Mundo)
    /// now stand where their stand-ins stood (Docs/Mapas100.md 25–40): each one spawns by day in at least one clearing of
    /// every group its sheet gives it, at that area's levels, with its night form in the same clearings' night table, and
    /// the stand-in left those groups' day tables.
    /// </summary>
    public class LateRegionBeastsPlacementTests
    {
        public readonly struct Placement
        {
            public readonly string Region;
            public readonly string Enemy;
            public readonly string Area;
            public readonly string Group;
            public readonly string[] StandIns;

            public Placement(string region, string enemy, string area, string group, params string[] standIns)
            {
                Region = region;
                Enemy = enemy;
                Area = area;
                Group = group;
                StandIns = standIns;
            }

            public override string ToString() => $"{Enemy} in {Area}/{Group}";
        }

        private const string Crocodile = "enemy_forest_mangrove_crocodile";
        private const string Lion = "enemy_forest_grass_lion";
        private const string Rhino = "enemy_forest_dust_rhino";
        private const string Monkey = "enemy_forest_thief_monkey";
        private const string Otter = "enemy_forest_thief_otter";
        private const string Python = "enemy_forest_dimmed_python";
        private const string Troll = "enemy_forest_dimmed_troll";
        private const string Bear = "enemy_mountain_brown_bear";
        private const string Yak = "enemy_mountain_wild_yak";
        private const string Irbis = "enemy_mountain_irbis";
        private const string Almas = "enemy_mountain_almas";
        private const string Marmot = "enemy_mountain_giant_marmot";

        private const string SandSnake = "enemy_desert_sand_snake";
        private const string Jackal = "enemy_desert_jackal";
        private const string Baboon = "enemy_mountain_baboon";
        private const string Wolf = PeriodSpawnTable.ForestWolf;
        private const string Golem = PeriodSpawnTable.MountainGolem;
        private const string Peccary = NightVariantRules.PeccaryEnemyId;
        private const string Werewolf = NightVariantRules.WerewolfEnemyId;

        private const string Costa = OverworldLayouts.CostaId;
        private const string Delta = OverworldLayouts.DeltaId;
        private const string Teto = OverworldLayouts.TetoId;

        private static readonly Placement[] Placements =
        {
            new Placement(Costa, Crocodile, "mangue_raso", "crocodilos_do_mangue", SandSnake),
            new Placement(Costa, Crocodile, "canais_de_liyongo", "crocodilos_dos_canais", SandSnake),
            new Placement(Costa, Crocodile, "mangues_de_liyongo", "crocodilos_de_liyongo", SandSnake),
            new Placement(Costa, Lion, "savana_dos_auroques", "leoes_do_capim", Wolf),
            new Placement(Costa, Rhino, "savana_dos_auroques", "rinocerontes_de_po", Peccary, Golem),

            new Placement(Delta, Crocodile, "lago_de_jhilpur", "gaviais_do_lodo", SandSnake),
            new Placement(Delta, Crocodile, "charcos_de_lotus", "gaviais_do_charco", SandSnake),
            new Placement(Delta, Crocodile, "floresta_de_mare", "gaviais_da_mare", SandSnake),
            new Placement(Delta, Crocodile, "pantano_aberto", "crocodilos_de_musgo", SandSnake),
            new Placement(Delta, Crocodile, "ilhas_que_andam", "crocodilos_das_ilhas", SandSnake),
            new Placement(Delta, Crocodile, "raizes_afogadas", "gaviais_sombra", SandSnake),
            new Placement(Delta, Crocodile, "coracao_do_lodo", "gaviais_sombra_do_lodo", SandSnake),
            new Placement(Delta, Crocodile, "olho_do_lodo", "gaviais_do_olho", SandSnake),
            new Placement(Delta, Crocodile, "charco_do_ninki_nanka", "filhotes_de_ninki_nanka", SandSnake),
            new Placement(Delta, Crocodile, "toca_da_dragoa", "filhotes_da_toca", SandSnake),
            new Placement(Delta, Monkey, "figueiral_das_raizes", "macacos_do_figueiral", Baboon),
            new Placement(Delta, Monkey, "floresta_de_mare", "macacos_da_mare", Baboon),
            new Placement(Delta, Monkey, "pontes_de_raiz_viva", "macacos_das_pontes", Baboon),
            new Placement(Delta, Monkey, "cascatas_da_chuva", "macacos_das_cascatas", Baboon),
            new Placement(Delta, Monkey, "floresta_de_chuva", "macacos_da_chuva", Baboon),
            new Placement(Delta, Otter, "bancos_de_jhilpur", "lontras_ladras", Jackal),
            new Placement(Delta, Otter, "diques_do_norte", "lontras_dos_arrozais", Jackal),
            new Placement(Delta, Python, "gargantas_esquecidas", "pitons_da_garganta", SandSnake),
            new Placement(Delta, Python, "cascatas_da_chuva", "pitons_das_cascatas", SandSnake),
            new Placement(Delta, Python, "brejo_das_palmeiras", "cobras_dagua", SandSnake),
            new Placement(Delta, Python, "ilhota_das_lanternas", "cobras_dagua_da_ilhota", SandSnake),
            new Placement(Delta, Troll, "palafitas_de_tarchon", "trols_apagados", Golem),
            new Placement(Delta, Troll, "mata_morta", "trols_esquecidos_da_mata", Golem),

            new Placement(Teto, Bear, "choupal_dos_canais", "ursos_do_choupal", Werewolf),
            new Placement(Teto, Bear, "encosta_dos_zimbros", "ursos_dos_zimbros", Werewolf),
            new Placement(Teto, Bear, "campos_de_neve_de_sangkent", "ursos_das_neves", Werewolf),
            new Placement(Teto, Yak, "pastagens_dos_iaques", "iaques_selvagens", Peccary),
            new Placement(Teto, Yak, "geleira_do_teto_do_mundo", "iaques_da_geleira", Peccary),
            new Placement(Teto, Yak, "estepe_alta", "argalis_da_estepe", Peccary),
            new Placement(Teto, Yak, "vale_do_wakhan", "argalis_do_vale", Peccary),
            new Placement(Teto, Yak, "passo_de_wakhan", "argalis_apagados", Peccary),
            new Placement(Teto, Irbis, "alto_do_wakhan", "irbis_apagados", Wolf),
            new Placement(Teto, Irbis, "ninho_de_aydahar", "irbis_do_ninho", Wolf),
            new Placement(Teto, Almas, "geleira_do_teto_do_mundo", "almas", Baboon),
            new Placement(Teto, Almas, "campo_de_seracs", "almas_dos_seracs", Baboon),
            new Placement(Teto, Almas, "picos_gelados", "almas_dos_picos", Baboon),
            new Placement(Teto, Marmot, "terracos_de_sangkent", "marmotas_gigantes", Jackal),
            new Placement(Teto, Marmot, "vale_dos_damasqueiros", "marmotas_dos_pomares", Jackal),
            new Placement(Teto, Marmot, "estepe_alta", "colonias_de_marmotas", Jackal),
            new Placement(Teto, Marmot, "calhas_de_pedra", "marmotas_das_calhas", Jackal),
            new Placement(Teto, Marmot, "vale_do_wakhan", "marmotas_do_vale", Jackal),
        };

        private static IEnumerable<Placement> All() => Placements;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            MapLayouts.ClearCache();
            OverworldLayouts.ClearCache();
        }

        [OneTimeTearDown]
        public void OneTimeTearDown() => MapLayouts.ClearCache();

        [Test]
        public void EachBeast_SpawnsInItsGroup_AtTheAreasLevels_WithItsNightForm([ValueSource(nameof(All))] Placement placement)
        {
            OverworldLayout w = OverworldLayouts.Get(placement.Region);
            OverworldZone area = w.Zone(placement.Area);
            Assert.IsNotNull(area, placement.Area);
            List<MapSpawnZone> group = Clearings(w, placement.Area, placement.Group).ToList();
            Assert.IsNotEmpty(group, $"{placement}: the group has clearings in its area");
            List<MapSpawnZone> clearings = group.Where(s => s.Day.Any(e => e.EnemyId == placement.Enemy)).ToList();
            Assert.IsNotEmpty(clearings, $"{placement}: at least one clearing spawns it by day");
            foreach (MapSpawnZone spawn in clearings)
            {
                Assert.GreaterOrEqual(spawn.MinLevel, area.MinLevel, spawn.Id);
                Assert.LessOrEqual(spawn.MaxLevel, area.MaxLevel, spawn.Id);
            }

            Assert.IsTrue(clearings.Any(s => s.Night.Any(e => e.EnemyId == placement.Enemy && e.Form != NightForm.Normal)),
                $"{placement}: its night variant prowls the same clearings");
        }

        [Test]
        public void TheStandIns_LeftTheGroupsTheirBeastsTookOver([ValueSource(nameof(All))] Placement placement)
        {
            OverworldLayout w = OverworldLayouts.Get(placement.Region);
            List<PeriodSpawnEntry> day = Clearings(w, placement.Area, placement.Group).SelectMany(s => s.Day).ToList();
            foreach (string standIn in placement.StandIns)
            {
                Assert.IsFalse(day.Any(e => e.EnemyId == standIn), $"{placement}: no {standIn} by day any more");
            }
        }

        [Test]
        public void TheWerewolf_IsNoLongerABearByDay_OnTheRoofOfTheWorld()
        {
            OverworldLayout teto = OverworldLayouts.Get(Teto);
            Assert.IsFalse(teto.World.Zones.SelectMany(s => s.Day).Any(e => e.EnemyId == Werewolf), "the werewolf is a night species");
        }

        /// <summary>The clearings of <paramref name="area"/> hosting <paramref name="group"/>.</summary>
        private static IEnumerable<MapSpawnZone> Clearings(OverworldLayout w, string area, string group)
        {
            return w.World.Zones.Where(s => w.ZoneOfSpawn[s.Id].Id == area && s.Id.EndsWith("_" + group));
        }
    }
}
