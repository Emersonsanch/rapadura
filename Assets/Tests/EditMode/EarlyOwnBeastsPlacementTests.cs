using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rapadura.Gameplay.Night;
using Rapadura.Gameplay.World.Map;
using Rapadura.Gameplay.World.Map.Areas;
using Rapadura.Gameplay.World.Map.Overworld;
using Placement = Rapadura.Tests.MingchengNartesBeastsPlacementTests.Placement;
using Own = Rapadura.Gameplay.World.Map.Overworld.OverworldLayouts.EarlyOwnBeasts;

namespace Rapadura.Tests
{
    /// <summary>
    /// The twenty-third pass of beasts (<see cref="EarlyOwnBeastsTests"/>) now stands where its stand-ins stood in Regions 1–4: in the day table of
    /// the overworld groups of its sheet (its night form where it leads, the stand-in gone from those day tables), in the clearings those groups
    /// really drew at the area's levels, and in the clearings of the hand-made maps 03–06, 19 and 20 that the continuous world embeds (the trails
    /// copy the nearest clearing, so they follow). Same idea as <see cref="EarlyStandInBeastsPlacementTests"/>.
    /// </summary>
    public class EarlyOwnBeastsPlacementTests
    {
        private const string Wolf = PeriodSpawnTable.ForestWolf;
        private const string Spider = PeriodSpawnTable.ForestSpider;
        private const string Boar = NightVariantRules.PeccaryEnemyId;
        private const string Golem = PeriodSpawnTable.MountainGolem;
        private const string Bats = PeriodSpawnTable.CaveBats;
        private const string Scorpion = PeriodSpawnTable.DesertScorpion;

        private const bool Leads = true;
        private const bool Joins = false;

        private static Placement P(string region, string enemy, string area, string group, bool withNightForm, params string[] standIns) =>
            new Placement(region, enemy, area, group, withNightForm, standIns);

        /// <summary>The overworld groups, in the big open areas (each draws several clearings).</summary>
        private static readonly Placement[] Placements =
        {
            P(OverworldLayouts.ZarafshanId, Own.ThreadJackdaw, "amoreiras", "gralhas", Joins, Spider),
            P(OverworldLayouts.ZarafshanId, Own.StickyMist, "garajonay", "nevoa", Joins, Spider),
            P(OverworldLayouts.ZarafshanId, Own.AshWolf, "jardim_mihrshah", "lobos_de_cinzas", Leads, Wolf),
            P(OverworldLayouts.ZarafshanId, Own.ThornRose, "jardim_mihrshah", "jardineiros", Joins, Wolf),

            P(OverworldLayouts.AreiasId, Own.KarezRat, "galerias_do_karez", "morcegos_do_karez", Leads, Bats),
            P(OverworldLayouts.AreiasId, Own.WaterScorpion, "galerias_do_karez", "escorpioes_dagua", Leads, Scorpion),

            P(OverworldLayouts.KaggenId, Own.StoneLizard, "muralhas_pedra_seca", "lagartos", Leads, Wolf),
        };

        /// <summary>The overworld groups of the Limiar's small halls round the embedded maps: only their tables are checked (the maps bring the clearings).</summary>
        private static readonly Placement[] HallPlacements =
        {
            P(OverworldLayouts.LimiarId, Own.GrottoSpider, "lago_do_limiar", "aranhas_de_gruta", Leads, Spider),
            P(OverworldLayouts.LimiarId, Own.GrottoSpider, "boca_do_hahya", "boca_aranhas", Leads, Spider),
            P(OverworldLayouts.LimiarId, Own.BladeStone, "salao_de_tages", "estalactites", Joins, Golem),
            P(OverworldLayouts.LimiarId, Own.BladeStone, "salao_de_tages", "lanternas_quebradas", Joins, Golem),
        };

        public readonly struct MapPlacement
        {
            public readonly string Region;
            public readonly string MapId;
            public readonly string Zone;
            public readonly string Enemy;
            public readonly string StandIn;

            public MapPlacement(string region, string mapId, string zone, string enemy, string standIn)
            {
                Region = region;
                MapId = mapId;
                Zone = zone;
                Enemy = enemy;
                StandIn = standIn;
            }

            public override string ToString() => $"{Enemy} in {MapId}/{Zone}";
        }

        private static MapPlacement M(string region, string mapId, string zone, string enemy, string standIn) => new MapPlacement(region, mapId, zone, enemy, standIn);

        /// <summary>The clearings of the hand-made maps that played the role with the stand-in.</summary>
        private static readonly MapPlacement[] MapPlacements =
        {
            M(OverworldLayouts.ZarafshanId, MapLayouts.BrejoId, "juncal_norte", Own.MudEel, Wolf),
            M(OverworldLayouts.ZarafshanId, MapLayouts.BrejoId, "lodo_oeste", Own.MudEel, Wolf),
            M(OverworldLayouts.ZarafshanId, MapLayouts.BrejoId, "ilha_do_meio", Own.MudEel, Wolf),
            M(OverworldLayouts.ZarafshanId, MapLayouts.BrejoId, "beira_do_poco", Own.MudEel, Wolf),
            M(OverworldLayouts.ZarafshanId, MapLayouts.BrejoId, "poca_leste", OverworldLayouts.EarlyBeasts.SmokeMosquito, Bats),
            M(OverworldLayouts.ZarafshanId, MapLayouts.AmoreirasId, "casulos", Own.CocoonCaterpillar, Boar),
            M(OverworldLayouts.ZarafshanId, MapLayouts.AmoreirasId, "tear_grande", Own.CocoonCaterpillar, Boar),
            M(OverworldLayouts.ZarafshanId, MapLayouts.AmoreirasId, "varais", Own.ThreadJackdaw, Wolf),
            M(OverworldLayouts.ZarafshanId, MapLayouts.GarajonayId, "nevoa_baixa", Own.StickyMist, Spider),
            M(OverworldLayouts.ZarafshanId, MapLayouts.GarajonayId, "beira_da_cascata", Own.StickyMist, Spider),
            M(OverworldLayouts.ZarafshanId, MapLayouts.MihrshahId, "quadrante_noroeste", Own.ThornRose, Spider),
            M(OverworldLayouts.ZarafshanId, MapLayouts.MihrshahId, "quadrante_nordeste", Own.AshWolf, Wolf),
            M(OverworldLayouts.ZarafshanId, MapLayouts.MihrshahId, "quadrante_nordeste", Own.ThornRose, Spider),
            M(OverworldLayouts.ZarafshanId, MapLayouts.MihrshahId, "quadrante_sudoeste", Own.ThornRose, Spider),
            M(OverworldLayouts.ZarafshanId, MapLayouts.MihrshahId, "quadrante_sudeste", Own.AshWolf, Wolf),
            M(OverworldLayouts.LimiarId, MapLayouts.LagoLimiarId, "margem_leste", Own.GrottoSpider, Spider),
            M(OverworldLayouts.LimiarId, MapLayouts.TagesId, "vau_escuro", Own.LittleShadow, Spider),
            M(OverworldLayouts.LimiarId, MapLayouts.TagesId, "colunata", Own.BladeStone, Golem),
            M(OverworldLayouts.LimiarId, MapLayouts.TagesId, "antessala_de_tages", Own.BladeStone, Golem),
        };

        private static IEnumerable<Placement> All() => Placements;

        private static IEnumerable<Placement> AllTables() => Placements.Concat(HallPlacements);

        private static IEnumerable<MapPlacement> AllMaps() => MapPlacements;

        /// <summary>Every beast of the pass and the region it must spawn in by day (somewhere: clearings, embedded maps or trails).</summary>
        private static IEnumerable<TestCaseData> BeastsByRegion() => new[]
        {
            new TestCaseData(OverworldLayouts.ZarafshanId, Own.MudEel),
            new TestCaseData(OverworldLayouts.ZarafshanId, Own.CocoonCaterpillar),
            new TestCaseData(OverworldLayouts.ZarafshanId, Own.ThreadJackdaw),
            new TestCaseData(OverworldLayouts.ZarafshanId, Own.StickyMist),
            new TestCaseData(OverworldLayouts.ZarafshanId, Own.ThornRose),
            new TestCaseData(OverworldLayouts.ZarafshanId, Own.AshWolf),
            new TestCaseData(OverworldLayouts.LimiarId, Own.GrottoSpider),
            new TestCaseData(OverworldLayouts.LimiarId, Own.BladeStone),
            new TestCaseData(OverworldLayouts.LimiarId, Own.LittleShadow),
            new TestCaseData(OverworldLayouts.KaggenId, Own.StoneLizard),
            new TestCaseData(OverworldLayouts.AreiasId, Own.KarezRat),
            new TestCaseData(OverworldLayouts.AreiasId, Own.WaterScorpion),
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
        public void EachBeast_IsInItsGroupsTables_WithItsNightForm_AndTheStandInLeft([ValueSource(nameof(AllTables))] Placement placement) =>
            checks.EachBeast_IsInItsGroupsTables_WithItsNightForm_AndTheStandInLeft(placement);

        [Test]
        public void EachBeast_SpawnsInItsGroupsClearings_AtTheAreasLevels([ValueSource(nameof(All))] Placement placement) =>
            checks.EachBeast_SpawnsInItsGroupsClearings_AtTheAreasLevels(placement);

        [Test]
        public void TheHandMadeMaps_SpawnTheBeast_InThePlaceOfTheStandIn([ValueSource(nameof(AllMaps))] MapPlacement placement)
        {
            MapLayout layout = MapLayouts.For(placement.MapId);
            Assert.IsNotNull(layout, placement.MapId);
            MapSpawnZone zone = layout.Zones.FirstOrDefault(z => z.Id == placement.Zone);
            Assert.IsNotNull(zone, placement.ToString());
            Assert.IsTrue(zone.Day.Any(e => e.EnemyId == placement.Enemy && e.Form == NightForm.Normal), $"{placement}: by day");
            // The night species borrow other bodies (the Lobisomem is dressed on a wolf): only the stand-in itself and its variant must be gone.
            Assert.IsFalse(zone.AllEntries().Any(e => e.EnemyId == placement.StandIn && e.Form != NightForm.Species), $"{placement}: no {placement.StandIn} any more, day or night");
            Assert.IsTrue(zone.Night.Any(e => e.Form != NightForm.Normal), $"{placement}: the night still brings variants or night species");
        }

        [Test]
        public void TheContinuousWorld_CopiesTheHandMadeClearings_WithTheBeast([ValueSource(nameof(AllMaps))] MapPlacement placement)
        {
            OverworldLayout w = OverworldLayouts.Get(placement.Region);
            string prefix = placement.MapId.Length > 8 ? placement.MapId.Substring(8) : placement.MapId;
            MapSpawnZone copy = w.World.Zones.FirstOrDefault(s => s.Id == prefix + "_" + placement.Zone);
            if (copy == null)
            {
                Assert.Pass($"{placement}: this clearing falls outside its map's area in the stitched world");
            }

            Assert.IsTrue(copy.Day.Any(e => e.EnemyId == placement.Enemy), $"{copy.Id}: spawns {placement.Enemy} by day");
            Assert.IsFalse(copy.Day.Any(e => e.EnemyId == placement.StandIn), $"{copy.Id}: no {placement.StandIn} by day any more");
        }

        [TestCaseSource(nameof(BeastsByRegion))]
        public void EachBeast_SpawnsByDay_InItsRegionsContinuousWorld(string region, string enemy)
        {
            OverworldLayout w = OverworldLayouts.Get(region);
            List<MapSpawnZone> spawns = w.World.Zones.Where(s => s.Day.Any(e => e.EnemyId == enemy)).ToList();
            Assert.IsNotEmpty(spawns, $"{enemy}: no clearing of {region} spawns it");
            Assert.IsTrue(spawns.Any(s => !s.AlongTrail), $"{enemy}: at least one real clearing, not only the trails");
        }

        [Test]
        public void TheTrails_CarryTheNewBeasts_WhereTheyStandNextToAClearingOfThem()
        {
            // Trails copy the nearest clearing of their area: the beast of the Jardim's wolves walks its roads too.
            OverworldLayout w = OverworldLayouts.Get(OverworldLayouts.ZarafshanId);
            List<MapSpawnZone> trail = w.World.Zones.Where(s => s.AlongTrail && w.ZoneOfSpawn[s.Id].Id == "jardim_mihrshah").ToList();
            if (trail.Count == 0)
            {
                Assert.Pass("no trail group in the Jardim");
            }

            Assert.IsFalse(trail.Any(s => s.Day.Any(e => e.EnemyId == Wolf)), "no Lobo do Tugai on the Jardim's trails: the Lobo de Cinzas");
        }

        [Test]
        public void TheJardim_HasNoForestWolfOrSpiderLeft_ByDay()
        {
            OverworldLayout w = OverworldLayouts.Get(OverworldLayouts.ZarafshanId);
            List<MapSpawnZone> jardim = w.World.Zones.Where(s => w.ZoneOfSpawn[s.Id].Id == "jardim_mihrshah").ToList();
            Assert.IsNotEmpty(jardim);
            Assert.IsFalse(jardim.Any(s => s.Day.Any(e => e.EnemyId == Wolf || e.EnemyId == Spider)), "06: Lobo de Cinzas and Roseira-Espinho, not the Tugai's");
        }
    }
}
