using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rapadura.Gameplay.World.Map;
using Rapadura.Gameplay.World.Map.Areas;
using Rapadura.Gameplay.World.Map.Overworld;
using Placement = Rapadura.Tests.MingchengNartesBeastsPlacementTests.Placement;
using Sky = Rapadura.Gameplay.World.Map.Overworld.OverworldLayouts.SkyOwnBeasts;

namespace Rapadura.Tests
{
    /// <summary>
    /// The eighteenth pass of beasts (<see cref="SkyOwnBeastsTests"/>) now stands where its stand-ins stood in the Reinos do Céu: each beast is in
    /// the day table of the groups of its sheet, its night form in the same group's night table where it leads, the stand-in left those groups'
    /// day tables, and every case is checked on the clearings the group really got (OverworldLayouts.GenerateClearings, after the trails took
    /// theirs), at that area's levels. Same idea as <see cref="LentRoleBeastsPlacementTests"/>. Every new beast draws and leads at least one
    /// clearing; the glow mantle, the smoke goat, the garden shadow, the angry cedar, the bronze wolf, the late shadow, the creeping dark and the
    /// smoke harpy no longer spawn anywhere in the Sky, and the Limiar's Arconte Menor is left only as the Arconte Menor of its sheet (98).
    /// </summary>
    public class SkyOwnBeastsPlacementTests
    {
        private const string LesserArchon = "enemy_forest_lesser_archon";
        private const string GlowMantle = "enemy_forest_glow_mantle";
        private const string SmokeGoat = "enemy_forest_smoke_goat";
        private const string GardenShadow = "enemy_forest_garden_shadow";
        private const string AngryCedar = "enemy_forest_angry_cedar";
        private const string BronzeWolf = "enemy_mountain_bronze_wolf";
        private const string LateShadow = "enemy_forest_late_shadow";
        private const string CreepingDark = "enemy_forest_creeping_dark";
        private const string SmokeHarpy = "enemy_mountain_smoke_harpy";

        private const string CeuId = OverworldLayouts.CeuRegionId;

        private const bool Leads = true;
        private const bool Joins = false;

        private static Placement P(string enemy, string area, string group, bool withNightForm, params string[] standIns) =>
            new Placement(CeuId, enemy, area, group, withNightForm, standIns);

        private static readonly Placement[] Placements =
        {
            P(Sky.RunawayArchon, "beira_oeste", "arcontes_da_beira", Leads, LesserArchon),
            P(Sky.RunawayArchon, "pier_do_sol", "arcontes_do_sol", Leads, LesserArchon),
            P(Sky.RunawayArchon, "pier_do_sol", "faiscas_do_sol", Joins, LesserArchon),
            P(Sky.RunawayArchon, "fresta_das_luzinhas", "aves_da_fresta", Joins, LesserArchon),

            P(Sky.StubbornSpark, "mastro_da_lua", "faiscas_teimosas", Leads, GlowMantle),
            P(Sky.StubbornSpark, "conves_da_lua", "faiscas_do_conves", Leads, GlowMantle),
            P(Sky.StubbornSpark, "mastro_da_lua", "clandestinos_do_mastro", Joins, GlowMantle),
            P(Sky.StubbornSpark, "popa_da_lua", "sombras_da_popa", Joins, GlowMantle),

            P(Sky.SphereSmoke, "orbita_oeste", "fumacas_de_esfera", Leads, SmokeGoat),
            P(Sky.SphereSmoke, "orbita_leste", "fumacas_do_leste", Leads, SmokeGoat),
            P(Sky.SphereSmoke, "orbita_oeste", "arcontes_acorrentados", Joins, SmokeGoat),
            P(Sky.SphereSmoke, "orbita_leste", "gigantes_balao", Joins, SmokeGoat),

            P(Sky.FlowerTreader, "jardim_das_flores", "sombras_das_flores", Leads, GardenShadow),
            P(Sky.FlowerTreader, "pergola_do_jardineiro", "sombras_da_pergola", Leads, GardenShadow),
            P(Sky.FlowerTreader, "pomar_de_luz", "ladroes_do_pomar", Joins, GardenShadow),
            P(Sky.FlowerTreader, "pomar_de_luz", "espinhos_do_pomar", Joins, GardenShadow),

            P(Sky.SmokeThorn, "jardim_das_flores", "espinhos_das_flores", Leads, AngryCedar),
            P(Sky.SmokeThorn, "pomar_de_luz", "espinhos_do_pomar", Leads, AngryCedar),
            P(Sky.SmokeThorn, "jardim_das_flores", "sombras_das_flores", Joins, AngryCedar),
            P(Sky.SmokeThorn, "pergola_do_jardineiro", "ladroes_da_pergola", Joins, AngryCedar),

            P(Sky.EmberStowaway, "conves_do_sol", "clandestinos_de_brasa", Leads, BronzeWolf),
            P(Sky.EmberStowaway, "conves_do_sol", "gaivotas_de_luz", Joins, BronzeWolf),

            P(Sky.NoonShadow, "vela_do_sol", "sombras_do_meio_dia", Leads, LateShadow),

            P(Sky.LastShadow, "degraus_da_coluna", "ultimas_sombras", Leads, CreepingDark),
            P(Sky.LastShadow, "pes_da_coluna", "sombras_dos_pes", Leads, CreepingDark),
            P(Sky.LastShadow, "degraus_da_coluna", "ventos_do_alto", Joins, CreepingDark),
            P(Sky.LastShadow, "pes_da_coluna", "arcontes_do_degrau", Joins, CreepingDark),

            P(Sky.HighWind, "degraus_da_coluna", "ventos_do_alto", Leads, SmokeHarpy),
            P(Sky.HighWind, "degraus_da_coluna", "ultimas_sombras", Joins, SmokeHarpy),
            P(Sky.HighWind, "pes_da_coluna", "sombras_dos_pes", Joins, SmokeHarpy),

            P(Sky.FledArchon, "mirante_do_ceu", "arcontes_fugidos", Leads, LesserArchon),
        };

        private static IEnumerable<Placement> All() => Placements;

        /// <summary>The ten beasts of the pass (every one must lead a clearing somewhere in the Sky).</summary>
        private static IEnumerable<string> Beasts() => new[]
        {
            Sky.RunawayArchon, Sky.StubbornSpark, Sky.SphereSmoke, Sky.FlowerTreader, Sky.SmokeThorn,
            Sky.EmberStowaway, Sky.NoonShadow, Sky.LastShadow, Sky.HighWind, Sky.FledArchon,
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
        public void EachBeast_IsInItsGroupsTables_WithItsNightForm_AndTheStandInLeft([ValueSource(nameof(All))] Placement placement) =>
            checks.EachBeast_IsInItsGroupsTables_WithItsNightForm_AndTheStandInLeft(placement);

        [Test]
        public void EachBeast_SpawnsInItsGroupsClearings_AtTheAreasLevels([ValueSource(nameof(All))] Placement placement) =>
            checks.EachBeast_SpawnsInItsGroupsClearings_AtTheAreasLevels(placement);

        [Test]
        public void EachBeast_LeadsAtLeastOneDrawnClearing_InTheSky([ValueSource(nameof(Beasts))] string enemy)
        {
            OverworldLayout w = OverworldLayouts.Get(CeuId);
            List<MapSpawnZone> clearings = w.World.Zones.Where(s => s.Day.Any(e => e.EnemyId == enemy)).ToList();
            Assert.IsNotEmpty(clearings, $"{enemy}: no clearing of the Sky spawns it (the trails or the draw took them all)");
            Assert.IsTrue(clearings.Any(s => s.Day.Where(e => e.EnemyId == enemy).Sum(e => e.Weight) >= 3), $"{enemy}: leads at least one clearing (weight 3)");
        }

        [Test]
        public void TheStandIns_NoLongerSpawn_AnywhereInTheSky()
        {
            OverworldLayout w = OverworldLayouts.Get(CeuId);
            List<string> all = w.World.Zones.SelectMany(s => s.Day.Concat(s.Night)).Select(e => e.EnemyId).ToList();
            foreach (string standIn in new[] { GlowMantle, SmokeGoat, GardenShadow, AngryCedar, BronzeWolf, LateShadow, CreepingDark, SmokeHarpy })
            {
                Assert.IsFalse(all.Contains(standIn), $"{standIn}: its roles in the Sky have their own beasts now");
            }
        }

        [Test]
        public void TheArconteMenor_IsLeftOnlyAtTheFootOfTheColumn()
        {
            OverworldLayout w = OverworldLayouts.Get(CeuId);
            List<MapSpawnZone> clearings = w.World.Zones.Where(s => s.Day.Concat(s.Night).Any(e => e.EnemyId == LesserArchon)).ToList();
            Assert.IsNotEmpty(clearings, "the Arconte Menor (98) still stands on the steps");
            foreach (MapSpawnZone spawn in clearings)
            {
                Assert.AreEqual(OverworldLayouts.PesDaColunaMapId, w.ZoneOfSpawn[spawn.Id].MapIdOrOwner, $"{spawn.Id}: the Arconte Menor only in map 98");
            }
        }

        [TestCase(OverworldLayouts.CaisDaColunaMapId, Sky.FledArchon)]
        [TestCase(OverworldLayouts.MiranteCeuMapId, Sky.RunawayArchon)]
        [TestCase(OverworldLayouts.BarcaDoSolMapId, Sky.StubbornSpark)]
        [TestCase(OverworldLayouts.JardimDeLuzMapId, Sky.NoonShadow)]
        [TestCase(OverworldLayouts.BarcaDoSolMapId, Sky.FlowerTreader)]
        [TestCase(OverworldLayouts.BarcaDaLuaMapId, Sky.EmberStowaway)]
        [TestCase(OverworldLayouts.CaisDaColunaMapId, Sky.HighWind)]
        public void TheBeast_StaysOutOfTheMapsItsSheetDoesNotName(string map, string beast)
        {
            OverworldLayout w = OverworldLayouts.Get(CeuId);
            List<MapSpawnZone> clearings = w.World.Zones.Where(s => w.ZoneOfSpawn[s.Id].MapIdOrOwner == map).ToList();
            Assert.IsNotEmpty(clearings, map);
            Assert.IsFalse(clearings.Any(s => s.Day.Concat(s.Night).Any(e => e.EnemyId == beast)), $"no {beast} in {map}");
        }
    }
}
