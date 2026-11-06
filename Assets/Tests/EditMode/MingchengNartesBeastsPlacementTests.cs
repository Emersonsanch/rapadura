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
    /// The third pass of beasts (<see cref="MingchengNartesBeastsTests"/>: Planície de Mingcheng, Estepe dos Nartes) now
    /// stand where their stand-ins stood (Docs/Mapas100.md 41–52), same idea as <see cref="LateRegionBeastsPlacementTests"/>:
    /// each one is in the day table of every group its sheet gives it (leading the group or as its companion), its night
    /// form in the same group's night table where the beast leads it (the mud buffaloes keep the queixada herd at night),
    /// the stand-in left those groups' day tables, and every case is checked on the clearings the group really got (each
    /// group listed here draws at least one clearing in its area), at that area's levels.
    /// </summary>
    public class MingchengNartesBeastsPlacementTests
    {
        public readonly struct Placement
        {
            public readonly string Region;
            public readonly string Enemy;
            public readonly string Area;
            public readonly string Group;
            public readonly bool WithNightForm;
            public readonly string[] StandIns;

            public Placement(string region, string enemy, string area, string group, bool withNightForm, params string[] standIns)
            {
                Region = region;
                Enemy = enemy;
                Area = area;
                Group = group;
                WithNightForm = withNightForm;
                StandIns = standIns;
            }

            public override string ToString() => $"{Enemy} in {Area}/{Group}";
        }

        private const string Lion = "enemy_forest_stone_lion";
        private const string Fox = "enemy_forest_lantern_fox";
        private const string Terracotta = "enemy_forest_terracotta_soldier";
        private const string Turtle = "enemy_forest_stone_turtle";
        private const string Buffalo = "enemy_forest_mud_buffalo";
        private const string Nian = "enemy_forest_nian_cub";
        private const string Monkey = "enemy_forest_thief_monkey";
        private const string Griffin = "enemy_mountain_young_griffin";
        private const string Saiga = "enemy_mountain_saiga";
        private const string BronzeWolf = "enemy_mountain_bronze_wolf";
        private const string Giant = "enemy_mountain_sleepwalker_giant";
        private const string Beetle = "enemy_mountain_bark_beetle";
        private const string Ochokochi = "enemy_mountain_ochokochi";
        private const string Bear = "enemy_mountain_brown_bear";

        private const string Jackal = "enemy_desert_jackal";
        private const string Orc = "enemy_desert_crowned_orc";
        private const string CoconutCrab = "enemy_forest_coconut_crab";
        private const string Baboon = "enemy_mountain_baboon";
        private const string Vulture = "enemy_desert_ember_vulture";
        private const string Golem = PeriodSpawnTable.MountainGolem;
        private const string Peccary = NightVariantRules.PeccaryEnemyId;
        private const string Werewolf = NightVariantRules.WerewolfEnemyId;

        private const string Ming = OverworldLayouts.MingchengId;
        private const string Nart = OverworldLayouts.NartesId;
        private const bool Leads = true;
        private const bool Joins = false;

        private static readonly Placement[] Placements =
        {
            new Placement(Ming, Lion, "vilas_do_anel", "leoes_de_pedra", Leads, Golem),
            new Placement(Ming, Lion, "caminho_dos_leoes", "leoes_do_caminho", Leads, Golem),
            new Placement(Ming, Lion, "torres_brancas", "guardioes_de_cal", Leads, Golem),
            new Placement(Ming, Lion, "colinas_dos_espelhos", "guardioes_das_colinas", Leads, Golem),
            new Placement(Ming, Lion, "caminho_dos_leoes", "soldados_do_caminho", Joins, Golem),
            new Placement(Ming, Lion, "fossos_de_terracota", "soldados_de_terracota", Joins, Golem),
            new Placement(Ming, Lion, "vale_dos_guerreiros", "cavalos_do_vale", Joins, Golem),
            new Placement(Ming, Lion, "torres_brancas", "espelhos_das_torres", Joins, Golem),

            new Placement(Ming, Fox, "aneis_de_mingcheng", "raposas_de_cauda_lanterna", Leads, Jackal),
            new Placement(Ming, Fox, "vilas_do_anel", "raposas_das_vilas", Leads, Jackal),
            new Placement(Ming, Fox, "caminho_dos_leoes", "raposas_do_caminho", Leads, Jackal),
            new Placement(Ming, Fox, "campos_de_painco", "raposas_do_painco", Leads, Jackal),
            new Placement(Ming, Fox, "aneis_de_mingcheng", "grous_de_papel", Joins, Jackal),
            new Placement(Ming, Fox, "vilas_do_anel", "leoes_de_pedra", Leads, Jackal),
            new Placement(Ming, Fox, "colinas_de_cha", "grous_do_cha", Joins, Jackal),

            new Placement(Ming, Terracotta, "caminho_dos_leoes", "soldados_do_caminho", Leads, Orc),
            new Placement(Ming, Terracotta, "fossos_de_terracota", "soldados_de_terracota", Leads, Orc),
            new Placement(Ming, Terracotta, "vale_dos_guerreiros", "guerreiros_de_terracota", Leads, Orc),
            new Placement(Ming, Terracotta, "muralha_das_mil_torres", "soldados_da_muralha", Leads, Orc),
            new Placement(Ming, Terracotta, "muralha_oeste", "soldados_do_oeste", Leads, Orc),
            new Placement(Ming, Terracotta, "fossos_de_terracota", "cavalos_de_terracota", Joins, Orc),
            new Placement(Ming, Terracotta, "vale_dos_guerreiros", "arqueiros_de_papel", Joins, Orc),
            new Placement(Ming, Terracotta, "muralha_das_mil_torres", "arqueiros_de_terracota", Joins, Orc),
            new Placement(Ming, Terracotta, "torre_quinhentos", "cavalos_da_muralha", Joins, Orc),
            new Placement(Ming, Terracotta, "portao_do_nian", "filhotes_do_portao", Joins, Orc),

            new Placement(Ming, Turtle, "eclusa_da_lua", "tartarugas_de_pedra", Leads, CoconutCrab),
            new Placement(Ming, Turtle, "canal_das_lanternas", "tartarugas_do_canal", Leads, CoconutCrab),

            new Placement(Ming, Buffalo, "rio_dos_salgueiros", "bufalos_da_margem", Joins, Peccary),
            new Placement(Ming, Buffalo, "arrozais_espelhados", "bufalos_de_lama", Joins, Peccary),
            new Placement(Ming, Buffalo, "moinho_dagua", "bufalos_do_moinho", Joins, Peccary),
            new Placement(Ming, Buffalo, "moinho_dagua", "espantalhos_apagados", Joins, Peccary),

            new Placement(Ming, Nian, "picos_do_nian", "filhotes_de_nian", Leads, Baboon),
            new Placement(Ming, Nian, "portao_do_nian", "filhotes_do_portao", Leads, Baboon),
            new Placement(Ming, Monkey, "bambuzal_do_oeste", "macacos_do_bambu", Leads, Baboon),
            new Placement(Ming, Monkey, "picos_de_calcario", "macacos_do_calcario", Leads, Baboon),
            new Placement(Ming, Monkey, "vale_da_nevoa", "macacos_da_nevoa", Leads, Baboon),
            new Placement(Ming, Monkey, "cristas_dos_pinheiros", "corvos_dos_pinheiros", Joins, Baboon),

            new Placement(Nart, Griffin, "pistas_de_corrida", "grifos_jovens", Leads, Vulture),
            new Placement(Nart, Griffin, "kurgans_de_ouro", "grifos_guardioes", Leads, Vulture),
            new Placement(Nart, Griffin, "kurgan_aberto", "lobos_de_bronze", Joins, Vulture),

            new Placement(Nart, Saiga, "acampamento_de_soslan", "saigas_assustadas", Leads, Jackal),
            new Placement(Nart, Saiga, "estepe_de_soslan", "saigas_da_estepe", Leads, Jackal),
            new Placement(Nart, Saiga, "estepe_das_saigas", "manadas_de_saigas", Leads, Jackal),
            new Placement(Nart, Saiga, "acampamento_de_soslan", "aguias_douradas", Joins, Jackal),
            new Placement(Nart, Saiga, "pistas_de_corrida", "lobos_da_estepe", Joins, Jackal),
            new Placement(Nart, Saiga, "estepe_das_saigas", "abutres_do_absinto", Leads, Jackal),
            new Placement(Nart, Saiga, "vau_largo", "cisnes_do_vau", Joins, Jackal),
            new Placement(Nart, Saiga, "estepe_dos_ventos", "tempestades_dos_ventos", Joins, Jackal),
            new Placement(Nart, Saiga, "kurgans_do_leste", "lobos_de_bronze_do_leste", Joins, Jackal),
            new Placement(Nart, Saiga, "pastos_dos_gigantes", "javalis_da_colina", Leads, Jackal),
            new Placement(Nart, Saiga, "campos_de_papoula", "abutres_da_papoula", Joins, Jackal),

            new Placement(Nart, BronzeWolf, "kurgan_aberto", "lobos_de_bronze", Leads, Golem),
            new Placement(Nart, BronzeWolf, "kurgans_do_leste", "lobos_de_bronze_do_leste", Leads, Golem),
            new Placement(Nart, BronzeWolf, "kurgans_de_ouro", "grifos_guardioes", Joins, Golem),

            new Placement(Nart, Giant, "fortim_dos_gigantes", "gigantes_sonambulos", Leads, Orc),
            new Placement(Nart, Giant, "vale_das_tabuas_de_sonho", "sonambulos_das_tabuas", Leads, Orc),
            new Placement(Nart, Giant, "fortim_dos_gigantes", "corvos_mensageiros", Joins, Orc),

            new Placement(Nart, Beetle, "taiga_de_laricos", "besouros_casca", Leads, CoconutCrab),
            new Placement(Nart, Beetle, "lago_de_degelo", "besouros_do_degelo", Leads, CoconutCrab),
            new Placement(Nart, Beetle, "taiga_de_laricos", "linces_da_taiga", Joins, CoconutCrab),
            new Placement(Nart, Beetle, "arvoredo_de_haripushpa", "fungos_apagados", Joins, CoconutCrab),

            new Placement(Nart, Ochokochi, "vale_das_torres_svan", "ochokochis_do_vale", Leads, Baboon),
            new Placement(Nart, Ochokochi, "corrente_de_amirani", "ochokochis_da_corrente", Leads, Baboon),
            new Placement(Nart, Ochokochi, "vale_das_torres_svan", "corvos_das_torres", Joins, Baboon),
            new Placement(Nart, Ochokochi, "torre_de_vigia", "aguias_de_ferro", Joins, Baboon),

            new Placement(Nart, Bear, "torre_de_vigia", "ursos_do_caucaso", Leads, Werewolf),
            new Placement(Nart, Bear, "cume_de_ohya", "lobos_de_neve", Joins, Werewolf),
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
        public void EachBeast_IsInItsGroupsTables_WithItsNightForm_AndTheStandInLeft([ValueSource(nameof(All))] Placement placement)
        {
            OverworldZone area = OverworldLayouts.Get(placement.Region).Zone(placement.Area);
            Assert.IsNotNull(area, placement.Area);
            OverworldFauna fauna = area.Fauna.FirstOrDefault(f => f.Id == placement.Group);
            Assert.IsNotNull(fauna, $"{placement}: the area declares the group");
            Assert.IsTrue(fauna.Day.Any(e => e.EnemyId == placement.Enemy && e.Form == NightForm.Normal), $"{placement}: it spawns by day");
            if (placement.WithNightForm)
            {
                Assert.IsTrue(fauna.Night.Any(e => e.EnemyId == placement.Enemy && e.Form != NightForm.Normal), $"{placement}: its night variant");
            }

            foreach (string standIn in placement.StandIns)
            {
                Assert.IsFalse(fauna.Day.Any(e => e.EnemyId == standIn), $"{placement}: no {standIn} by day any more");
            }
        }

        [Test]
        public void EachBeast_SpawnsInItsGroupsClearings_AtTheAreasLevels([ValueSource(nameof(All))] Placement placement)
        {
            OverworldLayout w = OverworldLayouts.Get(placement.Region);
            OverworldZone area = w.Zone(placement.Area);
            List<MapSpawnZone> clearings = w.World.Zones
                .Where(s => w.ZoneOfSpawn[s.Id].Id == placement.Area && s.Id.EndsWith("_" + placement.Group)).ToList();
            Assert.IsNotEmpty(clearings, $"{placement}: the group draws at least one clearing in its area");
            foreach (MapSpawnZone spawn in clearings)
            {
                Assert.IsTrue(spawn.Day.Any(e => e.EnemyId == placement.Enemy), $"{spawn.Id}: spawns {placement.Enemy} by day");
                Assert.GreaterOrEqual(spawn.MinLevel, area.MinLevel, spawn.Id);
                Assert.LessOrEqual(spawn.MaxLevel, area.MaxLevel, spawn.Id);
                foreach (string standIn in placement.StandIns)
                {
                    Assert.IsFalse(spawn.Day.Any(e => e.EnemyId == standIn), $"{spawn.Id}: no {standIn} by day any more");
                }
            }

            if (placement.WithNightForm)
            {
                Assert.IsTrue(clearings.Any(s => s.Night.Any(e => e.EnemyId == placement.Enemy && e.Form != NightForm.Normal)),
                    $"{placement}: its night variant prowls the same clearings");
            }
        }

        [Test]
        public void TheMudBuffaloes_KeepTheQueixadaHerd_AtNight()
        {
            OverworldLayout w = OverworldLayouts.Get(Ming);
            foreach (string group in new[] { "bufalos_da_margem", "bufalos_de_lama", "bufalos_do_moinho" })
            {
                OverworldFauna fauna = w.Zones.SelectMany(z => z.Fauna).First(f => f.Id == group);
                Assert.IsTrue(fauna.Night.Any(e => e.SpeciesId == NightVariantRules.Queixada.Id), group);
            }
        }

        [Test]
        public void TheWerewolf_IsANightSpeciesOnly_OnTheSteppeAndThePlain()
        {
            foreach (string region in new[] { Ming, Nart })
            {
                OverworldLayout w = OverworldLayouts.Get(region);
                Assert.IsFalse(w.World.Zones.SelectMany(s => s.Day).Any(e => e.EnemyId == Werewolf), $"{region}: the werewolf is a night species");
            }
        }
    }
}
