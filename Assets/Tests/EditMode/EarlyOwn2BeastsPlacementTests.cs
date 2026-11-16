using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rapadura.Gameplay.Night;
using Rapadura.Gameplay.World.Cities;
using Rapadura.Gameplay.World.Map;
using Rapadura.Gameplay.World.Map.Areas;
using Rapadura.Gameplay.World.Map.Overworld;
using Placement = Rapadura.Tests.MingchengNartesBeastsPlacementTests.Placement;
using MapPlacement = Rapadura.Tests.EarlyOwnBeastsPlacementTests.MapPlacement;
using Own = Rapadura.Gameplay.World.Map.Overworld.OverworldLayouts.EarlyOwn2Beasts;

namespace Rapadura.Tests
{
    /// <summary>
    /// The twenty-sixth pass of beasts (<see cref="EarlyOwn2BeastsTests"/>) now stands where its stand-ins stood in Regions 2 and 3: in the day
    /// table of the overworld groups of its sheet (its night form where it leads, the stand-in gone from those day tables) and in the clearings
    /// those groups really drew at the area's levels. Also the Lagarto-Gigante do Louro, which now plays its own role in every clearing of the
    /// hand-made map 05 (the Lobo stood in for it there), and the Gara's quest that asks for it. Same idea as
    /// <see cref="EarlyOwnBeastsPlacementTests"/>.
    /// </summary>
    public class EarlyOwn2BeastsPlacementTests
    {
        private const string Scorpion = PeriodSpawnTable.DesertScorpion;
        private const string Hawk = "enemy_mountain_crested_hawk";
        private const string Bats = PeriodSpawnTable.CaveBats;
        private const string Wolf = PeriodSpawnTable.ForestWolf;
        private const string LaurelLizard = OverworldLayouts.EarlyBeasts.LaurelLizard;

        private const bool Leads = true;
        private const bool Joins = false;

        private static Placement P(string region, string enemy, string area, string group, bool withNightForm, params string[] standIns) =>
            new Placement(region, enemy, area, group, withNightForm, standIns);

        /// <summary>The overworld groups the beast leads (each draws several clearings in its big area).</summary>
        private static readonly Placement[] Placements =
        {
            P(OverworldLayouts.AreiasId, Own.ThirstyRoot, "galerias_do_karez", "morcegos_do_karez", Leads, Scorpion),
            P(OverworldLayouts.AreiasId, Own.CrystalScorpion, "trono_de_namrael", "escorpioes_cristal", Leads, Scorpion),
            P(OverworldLayouts.AreiasId, Own.SaltSerpent, "trono_de_namrael", "espelhos_de_sal", Leads, Scorpion),
            P(OverworldLayouts.KaggenId, Own.BlackEagle, "escarpas_de_arenito", "aguias_pretas", Leads, Hawk),
            P(OverworldLayouts.KaggenId, Own.ColdSalamander, "saloes_de_napirasu", "escoria", Leads, Bats),
            P(OverworldLayouts.KaggenId, Own.StormEagle, "cume_dos_mazans", "aguias_da_tempestade", Leads, Hawk),
        };

        /// <summary>The groups the beast only joins as a companion: only their tables are checked.</summary>
        private static readonly Placement[] CompanionPlacements =
        {
            P(OverworldLayouts.KaggenId, Own.BlackEagle, "abrigos_pintados", "ushabti_de_arenito", Joins, Hawk),
            P(OverworldLayouts.KaggenId, Own.StormEagle, "cume_dos_mazans", "neblina_do_cume", Joins, Hawk),
        };

        private static MapPlacement M(string zone) => new MapPlacement(OverworldLayouts.ZarafshanId, MapLayouts.GarajonayId, zone, LaurelLizard, Wolf);

        /// <summary>The clearings of the hand-made map 05 where the Lobo played the Lagarto-Gigante do Louro.</summary>
        private static readonly MapPlacement[] MapPlacements =
        {
            M("samambaial"), M("musgo"), M("louros_altos"), M("cascata_leste"), M("beira_da_cascata"),
        };

        private static IEnumerable<Placement> All() => Placements;

        private static IEnumerable<Placement> AllTables() => Placements.Concat(CompanionPlacements);

        private static IEnumerable<MapPlacement> AllMaps() => MapPlacements;

        /// <summary>Every beast of the pass and the region it must spawn in by day.</summary>
        private static IEnumerable<TestCaseData> BeastsByRegion() => new[]
        {
            new TestCaseData(OverworldLayouts.AreiasId, Own.ThirstyRoot),
            new TestCaseData(OverworldLayouts.AreiasId, Own.CrystalScorpion),
            new TestCaseData(OverworldLayouts.AreiasId, Own.SaltSerpent),
            new TestCaseData(OverworldLayouts.KaggenId, Own.BlackEagle),
            new TestCaseData(OverworldLayouts.KaggenId, Own.ColdSalamander),
            new TestCaseData(OverworldLayouts.KaggenId, Own.StormEagle),
        };

        private readonly MingchengNartesBeastsPlacementTests checks = new MingchengNartesBeastsPlacementTests();
        private readonly EarlyOwnBeastsPlacementTests mapChecks = new EarlyOwnBeastsPlacementTests();

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

        [TestCaseSource(nameof(BeastsByRegion))]
        public void EachBeast_SpawnsByDay_InItsRegionsContinuousWorld(string region, string enemy)
        {
            OverworldLayout w = OverworldLayouts.Get(region);
            List<MapSpawnZone> spawns = w.World.Zones.Where(s => s.Day.Any(e => e.EnemyId == enemy)).ToList();
            Assert.IsNotEmpty(spawns, $"{enemy}: no clearing of {region} spawns it");
            Assert.IsTrue(spawns.Any(s => !s.AlongTrail), $"{enemy}: at least one real clearing, not only the trails");
        }

        [Test]
        public void TheHandMadeMap05_SpawnsTheLaurelLizard_InThePlaceOfTheWolf([ValueSource(nameof(AllMaps))] MapPlacement placement) =>
            mapChecks.TheHandMadeMaps_SpawnTheBeast_InThePlaceOfTheStandIn(placement);

        [Test]
        public void TheContinuousWorld_CopiesMap05sClearings_WithTheLaurelLizard([ValueSource(nameof(AllMaps))] MapPlacement placement) =>
            mapChecks.TheContinuousWorld_CopiesTheHandMadeClearings_WithTheBeast(placement);

        [Test]
        public void Map05_HasNoForestWolfLeft_DayOrNight()
        {
            MapLayout layout = MapLayouts.For(MapLayouts.GarajonayId);
            Assert.IsFalse(layout.Zones.Any(z => z.AllEntries().Any(e => e.EnemyId == Wolf && e.Form != NightForm.Species)),
                "05: the Lagarto-Gigante do Louro plays its own role now (the Lobisomem at night is a night species)");
        }

        [Test]
        public void GarasQuest_AsksForTheLaurelLizard_ThatLivesInMap05()
        {
            CityQuestSpec gara = CityQuestCatalog.Quests.First(q => q.QuestId == CityQuestCatalog.GarajonayMist);
            Assert.AreEqual(LaurelLizard, gara.Quest.Objectives.Single(o => o.ObjectiveId == "calm_mist_lizards").TargetId);
            MapLayout layout = MapLayouts.For(MapLayouts.GarajonayId);
            Assert.IsTrue(layout.Zones.Any(z => z.Day.Any(e => e.EnemyId == LaurelLizard)), "the target lives in the Copas by day");
        }
    }
}
