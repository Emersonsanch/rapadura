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
    /// The beasts of <see cref="OverworldBeastsTests"/> that live in the continuous worlds now stand where their stand-ins
    /// stood (Docs/Mapas100.md 07–30): each one spawns by day in at least one clearing of every area its sheet puts it in,
    /// at that area's levels, with its night form in the same clearings' night table; the hand-made maps 07, 08, 13, 14 and
    /// 19 carry them too.
    /// </summary>
    public class OverworldBeastsPlacementTests
    {
        public readonly struct Placement
        {
            public readonly string Region;
            public readonly string Enemy;
            public readonly string Area;

            public Placement(string region, string enemy, string area)
            {
                Region = region;
                Enemy = enemy;
                Area = area;
            }

            public override string ToString() => $"{Enemy} in {Area}";
        }

        private const string Jackal = "enemy_desert_jackal";
        private const string SandSnake = "enemy_desert_sand_snake";
        private const string CrownedOrc = "enemy_desert_crowned_orc";
        private const string RollingStone = "enemy_mountain_rolling_stone";
        private const string Mist = "enemy_mountain_ill_wind_mist";
        private const string Baboon = "enemy_mountain_baboon";
        private const string Ooze = "enemy_cave_ooze";
        private const string Crow = "enemy_cave_dark_crow";
        private const string Gull = "enemy_forest_thief_gull";
        private const string Crab = "enemy_forest_coconut_crab";

        private static readonly Placement[] Placements =
        {
            new Placement(OverworldLayouts.AreiasId, Jackal, "estrada_das_caravanas"),
            new Placement(OverworldLayouts.AreiasId, Jackal, "oasis_de_qocho"),
            new Placement(OverworldLayouts.AreiasId, Jackal, "palmeiral_do_karez"),
            new Placement(OverworldLayouts.AreiasId, Jackal, "dunas_do_vento_quente"),
            new Placement(OverworldLayouts.AreiasId, Jackal, "planicie_dos_karez"),
            new Placement(OverworldLayouts.AreiasId, Jackal, "oasis_das_tamargueiras"),
            new Placement(OverworldLayouts.AreiasId, Jackal, "vale_das_ossadas"),
            new Placement(OverworldLayouts.AreiasId, Jackal, "cemiterio_de_caravanas"),
            new Placement(OverworldLayouts.AreiasId, SandSnake, "ruinas_dos_artifices"),
            new Placement(OverworldLayouts.AreiasId, CrownedOrc, "ermos_do_tambor_rubro"),
            new Placement(OverworldLayouts.AreiasId, RollingStone, "ermos_do_tambor_rubro"),
            new Placement(OverworldLayouts.KaggenId, RollingStone, "trilha_dos_rondaveis"),
            new Placement(OverworldLayouts.KaggenId, Mist, "campos_de_neve"),
            new Placement(OverworldLayouts.KaggenId, Mist, "passos_estreitos"),
            new Placement(OverworldLayouts.KaggenId, Mist, "passo_do_hahya"),
            new Placement(OverworldLayouts.KaggenId, Mist, "cume_dos_mazans"),
            new Placement(OverworldLayouts.KaggenId, Baboon, "trilha_dos_rondaveis"),
            new Placement(OverworldLayouts.KaggenId, Baboon, "abrigos_pintados"),
            new Placement(OverworldLayouts.LimiarId, Ooze, "lago_do_limiar"),
            new Placement(OverworldLayouts.LimiarId, Ooze, "boca_do_hahya"),
            new Placement(OverworldLayouts.LimiarId, Crow, "ninhos_ocidentais"),
            new Placement(OverworldLayouts.LimiarId, Crow, "ninho_de_mahaway"),
            new Placement(OverworldLayouts.CostaId, Gull, "praia_de_taa_pwani"),
            new Placement(OverworldLayouts.CostaId, Gull, "praia_dos_dhows"),
            new Placement(OverworldLayouts.CostaId, Gull, "coqueiral_de_pwani"),
            new Placement(OverworldLayouts.CostaId, Gull, "baixios_do_sul"),
            new Placement(OverworldLayouts.CostaId, Gull, "recife_de_taa_pwani"),
            new Placement(OverworldLayouts.CostaId, Crab, "praia_de_taa_pwani"),
            new Placement(OverworldLayouts.CostaId, Crab, "coqueiral_de_pwani"),
            new Placement(OverworldLayouts.CostaId, Crab, "recife_de_taa_pwani"),
            new Placement(OverworldLayouts.CostaId, Crab, "atol_de_kirta"),
            new Placement(OverworldLayouts.CostaId, Crab, "mangue_raso"),
        };

        private static IEnumerable<Placement> All() => Placements;

        /// <summary>Hand-made map → the beasts its own clearings spawn by day.</summary>
        private static readonly Dictionary<string, string[]> ByHandMadeMap = new Dictionary<string, string[]>
        {
            { MapLayouts.CaravanasId, new[] { Jackal } },
            { MapLayouts.ArtificesId, new[] { SandSnake } },
            { MapLayouts.RondaveisId, new[] { RollingStone, Baboon } },
            { MapLayouts.HahyaId, new[] { Mist } },
            { MapLayouts.LagoLimiarId, new[] { Ooze } },
        };

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            MapLayouts.ClearCache();
            OverworldLayouts.ClearCache();
        }

        [OneTimeTearDown]
        public void OneTimeTearDown() => MapLayouts.ClearCache();

        [Test]
        public void EachBeast_SpawnsInItsAreas_AtTheAreasLevels_WithItsNightForm([ValueSource(nameof(All))] Placement placement)
        {
            OverworldLayout w = OverworldLayouts.Get(placement.Region);
            OverworldZone area = w.Zone(placement.Area);
            Assert.IsNotNull(area, placement.Area);
            List<MapSpawnZone> clearings = w.World.Zones
                .Where(s => w.ZoneOfSpawn[s.Id] == area && s.Day.Any(e => e.EnemyId == placement.Enemy))
                .ToList();
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
        public void TheStandIns_LeftTheSlotsTheirBeastsTookOver()
        {
            OverworldLayout areias = OverworldLayouts.Get(OverworldLayouts.AreiasId);
            Assert.IsFalse(Spawns(areias, null).Any(e => e.EnemyId == PeriodSpawnTable.ForestWolf), "no forest wolf in the sands: the Chacal Apagado");
            Assert.IsFalse(Spawns(areias, "ermos_do_tambor_rubro", "pedras_rolantes").Any(e => e.EnemyId == PeriodSpawnTable.MountainGolem), "the Pedra que Rola rolls, not the ushabti");

            OverworldLayout kaggen = OverworldLayouts.Get(OverworldLayouts.KaggenId);
            Assert.IsFalse(Spawns(kaggen, "trilha_dos_rondaveis", "capim_alto").Any(e => e.EnemyId == PeriodSpawnTable.ForestWolf), "the Babuíno in the tall grass");
            Assert.IsFalse(Spawns(kaggen, "campos_de_neve", "neblina").Any(e => e.EnemyId == NightVariantRules.PotooEnemyId), "the Neblina is no potoo");
            Assert.IsFalse(Spawns(kaggen, "abrigos_pintados", "borroes").Any(e => e.EnemyId == PeriodSpawnTable.CaveBats), "the baboon thieves are no bats");

            OverworldLayout limiar = OverworldLayouts.Get(OverworldLayouts.LimiarId);
            Assert.IsFalse(Spawns(limiar, "ninhos_ocidentais", "corvos_da_treva").Any(e => e.EnemyId == NightVariantRules.PotooEnemyId), "the crows are crows");
            Assert.IsFalse(Spawns(limiar, "ninho_de_mahaway", "morcegoes_de_pedra").Any(e => e.EnemyId == NightVariantRules.PotooEnemyId), "the crows are crows");
        }

        [Test]
        public void TheHandMadeMaps_SpawnTheirOwnBeasts()
        {
            foreach (KeyValuePair<string, string[]> pair in ByHandMadeMap)
            {
                MapLayout layout = MapLayouts.For(pair.Key);
                Assert.IsNotNull(layout, pair.Key);
                foreach (string enemy in pair.Value)
                {
                    Assert.IsTrue(layout.Zones.Any(z => z.Day.Any(e => e.EnemyId == enemy) && z.Night.Any(e => e.EnemyId == enemy)),
                        $"{pair.Key}: a clearing with {enemy} by day and its night form");
                }
            }

            Assert.IsFalse(MapLayouts.For(MapLayouts.CaravanasId).Zones.SelectMany(z => z.Day).Any(e => e.EnemyId == PeriodSpawnTable.ForestWolf),
                "07: the jackal follows the caravans, not the wolf");
            Assert.IsFalse(MapLayouts.For(MapLayouts.LagoLimiarId).Zones.SelectMany(z => z.Day).Any(e => e.EnemyId == PeriodSpawnTable.MountainGolem),
                "19: the Lodo de Gruta, not the ushabti");
        }

        /// <summary>The day entries of the clearings of <paramref name="area"/> (all areas when null) hosting <paramref name="group"/> (the night species borrow other bodies).</summary>
        private static IEnumerable<PeriodSpawnEntry> Spawns(OverworldLayout w, string area, string group = null)
        {
            return w.World.Zones
                .Where(s => (area == null || w.ZoneOfSpawn[s.Id].Id == area) && (group == null || s.Id.EndsWith("_" + group)))
                .SelectMany(s => s.Day);
        }
    }
}
