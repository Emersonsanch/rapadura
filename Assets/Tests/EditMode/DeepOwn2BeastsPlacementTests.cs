using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rapadura.Gameplay.Quests;
using Rapadura.Gameplay.World.Map;
using Rapadura.Gameplay.World.Map.Areas;
using Rapadura.Gameplay.World.Map.Overworld;
using Placement = Rapadura.Tests.MingchengNartesBeastsPlacementTests.Placement;
using Deep2 = Rapadura.Gameplay.World.Map.Overworld.OverworldLayouts.DeepBeasts2;

namespace Rapadura.Tests
{
    /// <summary>
    /// The second half of the twenty-second pass of beasts (<see cref="DeepOwn2BeastsTests"/>) stands where its stand-ins stood in the
    /// Limiar Profundo: the Ushabti Apagado and the Fumaça outside the fortress gates (83), the Redemoinho of the Pit of Ruinous Wind (86)
    /// and the Rastejante of the Pit of Darkness and the Bridge of the Bōlos (88, 89). Each beast is in the day table of the groups of its
    /// sheet, its night form in the night table where it leads, the stand-in left those groups' day tables, and every case is checked on
    /// the clearings the group really got. Same idea as <see cref="DeepOwnBeastsPlacementTests"/>. The story step of the Muralha em Arco
    /// hunts the region's own ushabti now.
    /// </summary>
    public class DeepOwn2BeastsPlacementTests
    {
        private const string Mist = "enemy_mountain_ill_wind_mist";
        private const string LacquerUshabti = "enemy_mountain_lacquer_ushabti";
        private const string CreepingDark = "enemy_forest_creeping_dark";

        private const string Profundo = OverworldLayouts.ProfundoRegionId;

        private const bool Leads = true;
        private const bool Joins = false;

        private static Placement P(string enemy, string area, string group, bool withNightForm, params string[] standIns) =>
            new Placement(Profundo, enemy, area, group, withNightForm, standIns);

        private static readonly Placement[] Placements =
        {
            // Fortaleza da Última Lâmpada (83): the Ushabti Apagado [tanque] and the Fumaça [controle] outside the gates.
            P(Deep2.RampartUshabti, "muralha_em_arco", "ushabtis_da_muralha", Leads, LacquerUshabti),
            P(Deep2.RampartUshabti, "cisternas_de_oleo", "ushabtis_das_cisternas", Leads, LacquerUshabti),
            P(Deep2.RampartUshabti, "varais_de_pavios", "ushabtis_dos_varais", Leads, LacquerUshabti),
            P(Deep2.RampartUshabti, "muralha_em_arco", "sombras_do_arco", Joins, LacquerUshabti),
            P(Deep2.RampartUshabti, "beira_do_abismo", "morcegos_do_abismo", Joins, LacquerUshabti),
            P(Deep2.GateSmoke, "nichos_de_basalto", "fumacas_dos_nichos", Leads, Mist),
            P(Deep2.GateSmoke, "varais_de_pavios", "fumacas_dos_varais", Leads, Mist),
            P(Deep2.GateSmoke, "nichos_de_basalto", "morcegos_dos_nichos", Joins, Mist),
            P(Deep2.GateSmoke, "cisternas_de_oleo", "sombras_das_cisternas", Joins, Mist),

            // Cova do Vento Destruidor (86): the Redemoinho [controle].
            P(Deep2.RuinWhirlwind, "plataformas_soltas", "redemoinhos", Leads, Mist),
            P(Deep2.RuinWhirlwind, "correntes_ascendentes", "redemoinhos_das_correntes", Leads, Mist),
            P(Deep2.RuinWhirlwind, "plataformas_soltas", "harpias_de_vento", Joins, Mist),

            // Cova da Treva (88) and the Ponte do Novelo (89): the Rastejante [enxame].
            P(Deep2.DarkCrawler, "tuneis_cegos", "rastejantes", Leads, CreepingDark),
            P(Deep2.DarkCrawler, "lances_do_novelo", "filhos_da_treva", Leads, CreepingDark),
            P(Deep2.DarkCrawler, "cova_da_treva", "serpentes_cegas", Joins, CreepingDark),
            P(Deep2.DarkCrawler, "tuneis_cegos", "dragoes_sombra_menores", Joins, CreepingDark),
            P(Deep2.DarkCrawler, "ponte_do_novelo", "filhos_do_lodo", Joins, CreepingDark),
            P(Deep2.DarkCrawler, "praca_dos_pedestais", "arcontes_menores", Joins, CreepingDark),
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

        [TestCase(Deep2.RampartUshabti)]
        [TestCase(Deep2.GateSmoke)]
        [TestCase(Deep2.RuinWhirlwind)]
        [TestCase(Deep2.DarkCrawler)]
        public void EachBeast_LeadsAtLeastOneDrawnClearing(string enemy)
        {
            OverworldLayout w = OverworldLayouts.Get(Profundo);
            List<MapSpawnZone> clearings = w.World.Zones.Where(s => s.Day.Any(e => e.EnemyId == enemy)).ToList();
            Assert.IsNotEmpty(clearings, $"{enemy}: no clearing of the Limiar Profundo spawns it (the trails or the draw took them all)");
            Assert.IsTrue(clearings.Any(s => s.Day.Where(e => e.EnemyId == enemy).Sum(e => e.Weight) >= 3), $"{enemy}: leads at least one clearing (weight 3)");
        }

        [TestCase(Mist)]
        [TestCase(LacquerUshabti)]
        [TestCase(CreepingDark)]
        public void TheStandIns_NoLongerSpawn_InTheRegion(string standIn)
        {
            OverworldLayout w = OverworldLayouts.Get(Profundo);
            Assert.IsFalse(w.World.Zones.SelectMany(s => s.Day.Concat(s.Night)).Any(e => e.EnemyId == standIn), $"{standIn}: its roles in the Limiar Profundo have their own beasts now");
        }

        [Test]
        public void TheRampartStep_HuntsTheRampartUshabti_WhichSpawnsOnTheRampart()
        {
            OverworldLayout w = OverworldLayouts.Get(Profundo);
            List<MapSpawnZone> rampart = w.World.Zones.Where(s => w.ZoneOfSpawn[s.Id].Id == "muralha_em_arco").ToList();
            Assert.IsTrue(rampart.Any(s => s.Day.Any(e => e.EnemyId == Deep2.RampartUshabti)), "the Muralha em Arco spawns the rampart ushabti by day");
            Assert.IsTrue(LateStoryCatalog.Steps.Any(st => st.ZoneId == "muralha_em_arco" && st.Quest.Objectives.Any(o => o.TargetId == Deep2.RampartUshabti)), "the story step of the Muralha em Arco hunts it");
            Assert.IsFalse(LateStoryCatalog.Steps.Any(st => st.Quest.Objectives.Any(o => o.TargetId == LacquerUshabti)), "no Late story step hunts the lacquer ushabti any more");
        }
    }
}
