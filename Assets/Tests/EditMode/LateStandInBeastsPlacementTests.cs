using System.Collections.Generic;
using NUnit.Framework;
using Rapadura.Gameplay.World.Map;
using Rapadura.Gameplay.World.Map.Areas;
using Rapadura.Gameplay.World.Map.Overworld;
using Placement = Rapadura.Tests.MingchengNartesBeastsPlacementTests.Placement;

namespace Rapadura.Tests
{
    /// <summary>
    /// The eleventh pass of beasts (<see cref="LateStandInBeastsTests"/>) now stands where its stand-ins stood in Regions 12–16: each beast is in the day
    /// table of the groups of its sheet, its night form in the same group's night table where it leads, the stand-in left those groups' day tables,
    /// and every case is checked on the clearings the group really got, at that area's levels. Same idea as <see cref="CeuBeastsPlacementTests"/>.
    /// </summary>
    public class LateStandInBeastsPlacementTests
    {
        private const string Salamander = "enemy_mountain_devouring_salamander";
        private const string Bomb = "enemy_mountain_volcanic_bomb";
        private const string Goose = "enemy_forest_thief_goose";
        private const string ChildOfApep = "enemy_forest_child_of_apep";
        private const string Quicksand = "enemy_forest_quicksand";
        private const string ScorpionMan = "enemy_forest_scorpion_man";
        private const string CedarLynx = "enemy_forest_cedar_lynx";
        private const string ResinBee = "enemy_forest_resin_bee";
        private const string BladeBat = "enemy_forest_blade_bat";
        private const string GreatEel = "enemy_forest_great_eel";
        private const string AshBull = "enemy_forest_ash_bull";
        private const string SmokeRope = "enemy_forest_smoke_rope";

        private const string SandSnake = "enemy_desert_sand_snake";
        private const string Vulture = "enemy_desert_ember_vulture";
        private const string ThiefGull = "enemy_forest_thief_gull";
        private const string Python = "enemy_forest_dimmed_python";
        private const string Ooze = "enemy_cave_ooze";
        private const string DesertScorpion = "enemy_desert_scorpion";
        private const string Irbis = "enemy_mountain_irbis";
        private const string GoldenBee = "enemy_forest_golden_bee";
        private const string CandleBat = "enemy_mountain_candle_bat";
        private const string ReedSnake = "enemy_forest_reed_snake";
        private const string WildYak = "enemy_mountain_wild_yak";
        private const string ReedSnare = "enemy_forest_reed_snare";

        private const bool Leads = true;
        private const bool Joins = false;

        private static Placement P(string region, string enemy, string area, string group, bool withNightForm, params string[] standIns) =>
            new Placement(region, enemy, area, group, withNightForm, standIns);

        private static readonly Placement[] Placements =
        {
            P(OverworldLayouts.HinokuraRegionId, Salamander, "encosta_da_caldeira", "salamandras_devoradoras", Leads, SandSnake),
            P(OverworldLayouts.HinokuraRegionId, Salamander, "cinzas_vivas", "salamandras_das_cinzas", Leads, SandSnake),
            P(OverworldLayouts.HinokuraRegionId, Salamander, "mar_de_cinza", "salamandras_do_mar", Leads, SandSnake),
            P(OverworldLayouts.HinokuraRegionId, Salamander, "encosta_da_caldeira", "cinzas_vivas_da_encosta", Joins, SandSnake),
            P(OverworldLayouts.HinokuraRegionId, Bomb, "monte_echeyde", "bombas_vulcanicas", Leads, Vulture),
            P(OverworldLayouts.HinokuraRegionId, Bomb, "monte_echeyde", "tibicenas", Joins, Vulture),

            P(OverworldLayouts.RioRegionId, Goose, "campos_de_juncos", "gansos_ladroes", Leads, ThiefGull),
            P(OverworldLayouts.RioRegionId, Goose, "trigais_de_luz", "gansos_dos_trigais", Leads, ThiefGull),
            P(OverworldLayouts.RioRegionId, ChildOfApep, "banco_de_areia_de_apep", "filhos_de_apep", Leads, Python),
            P(OverworldLayouts.RioRegionId, ChildOfApep, "corpo_de_apep", "filhos_do_corpo", Leads, Python),
            P(OverworldLayouts.RioRegionId, ChildOfApep, "escamas_roxas", "filhos_das_escamas", Leads, Python),
            P(OverworldLayouts.RioRegionId, ChildOfApep, "areia_movedica", "filhos_da_areia_movedica", Leads, Python),
            P(OverworldLayouts.RioRegionId, Quicksand, "escamas_roxas", "areias_das_escamas", Leads, Ooze),
            P(OverworldLayouts.RioRegionId, Quicksand, "areia_movedica", "areias_movedicas", Leads, Ooze),
            P(OverworldLayouts.RioRegionId, Quicksand, "corpo_de_apep", "filhos_do_corpo", Joins, Ooze),

            P(OverworldLayouts.MisturaRegionId, ScorpionMan, "portao_de_tijolo", "homens_escorpiao_do_portao", Leads, DesertScorpion),
            P(OverworldLayouts.MisturaRegionId, ScorpionMan, "colinas_de_tell", "homens_escorpiao_do_tell", Leads, DesertScorpion),
            P(OverworldLayouts.MisturaRegionId, ScorpionMan, "canal_das_palmeiras", "homens_escorpiao_do_canal", Leads, DesertScorpion),
            P(OverworldLayouts.MisturaRegionId, CedarLynx, "beira_do_cedral", "linces_do_cedro", Leads, Irbis),
            P(OverworldLayouts.MisturaRegionId, ResinBee, "beira_do_cedral", "abelhas_resina", Leads, GoldenBee),

            P(OverworldLayouts.ProfundoRegionId, AshBull, "chao_que_acende", "touros_de_cinza", Leads, WildYak),
            P(OverworldLayouts.ProfundoRegionId, BladeBat, "cova_do_vento_destruidor", "morcegos_lamina", Leads, CandleBat),
            P(OverworldLayouts.ProfundoRegionId, GreatEel, "saloes_alagados", "enguias_grandes", Leads, ReedSnake),

            P(OverworldLayouts.CeuRegionId, SmokeRope, "vela_do_sol", "cordas_de_fumaca", Leads, ReedSnare),
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
    }
}
