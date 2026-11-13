using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rapadura.Gameplay.World.Map;
using Rapadura.Gameplay.World.Map.Areas;
using Rapadura.Gameplay.World.Map.Overworld;
using Placement = Rapadura.Tests.MingchengNartesBeastsPlacementTests.Placement;
using Borrowed = Rapadura.Gameplay.World.Map.Overworld.OverworldLayouts.BorrowedBeasts;

namespace Rapadura.Tests
{
    /// <summary>
    /// The sixteenth pass of beasts (<see cref="BorrowedRoleBeastsTests"/>) now stands where its stand-ins stood in Regions 7–16: each beast is in the
    /// day table of the groups of its sheet, its night form in the same group's night table where it leads, the stand-in left those groups' day
    /// tables, and every case is checked on the clearings the group really got, at that area's levels. Same idea as
    /// <see cref="WideStandInBeastsPlacementTests"/>. The roles that shared the stand-in keep it (the Sombra que Pisa as Flores and the Sombra do
    /// Meio-Dia in the Sky, the Rachadura and the Raiz-Gelo of the Mar Gelado, the Polvo de Obsidiana of Hinokura).
    /// </summary>
    public class BorrowedRoleBeastsPlacementTests
    {
        private const string Ooze = "enemy_cave_ooze";
        private const string Marmot = "enemy_mountain_giant_marmot";
        private const string RollingStone = "enemy_mountain_rolling_stone";
        private const string StoneTurtle = "enemy_forest_stone_turtle";
        private const string LavaLizard = "enemy_mountain_lava_lizard";
        private const string Pike = "enemy_mountain_giant_pike";
        private const string Golem = "enemy_mountain_stone_golem";
        private const string ShadowSeal = "enemy_mountain_shadow_seal";
        private const string GardenShadow = "enemy_forest_garden_shadow";
        private const string LateShadow = "enemy_forest_late_shadow";
        private const string Mist = "enemy_mountain_ill_wind_mist";

        private const string Teto = OverworldLayouts.TetoId;
        private const string Gelo = OverworldLayouts.MarGeladoRegionId;
        private const string Fogo = OverworldLayouts.HinokuraRegionId;
        private const string Mistura = OverworldLayouts.MisturaRegionId;
        private const string Ceu = OverworldLayouts.CeuRegionId;

        private const bool Leads = true;
        private const bool Joins = false;

        private static Placement P(string region, string enemy, string area, string group, bool withNightForm, params string[] standIns) =>
            new Placement(region, enemy, area, group, withNightForm, standIns);

        private static readonly Placement[] Placements =
        {
            P(Teto, Borrowed.LivingCrevasse, "geleira_do_teto_do_mundo", "fendas_vivas", Leads, Ooze),
            P(Teto, Borrowed.LivingCrevasse, "campo_de_seracs", "fendas_dos_seracs", Leads, Ooze),
            P(Teto, Borrowed.LivingCrevasse, "campo_de_seracs", "lobos_de_gelo", Joins, Ooze),
            P(Teto, Borrowed.LivingCrevasse, "picos_gelados", "almas_dos_picos", Joins, Ooze),

            P(Gelo, Borrowed.IceMole, "terra_congelada", "toupeiras_gelo", Leads, Marmot),
            P(Gelo, Borrowed.IceMole, "terra_congelada", "raizes_gelo", Joins, Marmot),
            P(Gelo, Borrowed.IceMole, "galerias_dos_mamutes", "mamutes_de_baixo_da_terra", Joins, Marmot),
            P(Gelo, Borrowed.IceMole, "fiorde_das_presas", "irbis_das_presas", Joins, Marmot),

            P(Fogo, Borrowed.FlyingPumice, "costa_das_fumarolas", "pedras_pomes", Leads, RollingStone),
            P(Fogo, Borrowed.FlyingPumice, "canais_de_lava", "pedras_pomes_dos_canais", Leads, RollingStone),
            P(Fogo, Borrowed.FlyingPumice, "canais_de_lava", "lagartos_dos_canais", Joins, RollingStone),
            P(Fogo, Borrowed.GlassUrchin, "ilhotas_de_vidro", "ouricos_de_vidro", Leads, StoneTurtle),
            P(Fogo, Borrowed.GlassUrchin, "recife_de_obsidiana", "corais_de_brasa", Joins, StoneTurtle),

            P(Mistura, Borrowed.InstantLizard, "deserto_do_relogio", "lagartos_do_instante", Leads, LavaLizard),
            P(Mistura, Borrowed.InstantLizard, "areia_das_horas", "lagartos_das_horas", Leads, LavaLizard),
            P(Mistura, Borrowed.InstantLizard, "deserto_do_relogio", "ampulhetas_vivas", Joins, LavaLizard),
            P(Mistura, Borrowed.InstantLizard, "sombras_atrasadas", "ushabtis_de_relogio", Joins, LavaLizard),
            P(Mistura, Borrowed.BlackCatfish, "pocos_de_betume", "bagres_negros", Leads, Pike),
            P(Mistura, Borrowed.BlackCatfish, "pocos_de_betume", "betumes_vivos", Joins, Pike),
            P(Mistura, Borrowed.Tremor, "dobras_de_pedra", "tremores_das_dobras", Leads, Golem),
            P(Mistura, Borrowed.Tremor, "clavicula_do_gigante", "tremores_da_clavicula", Leads, Golem),
            P(Mistura, Borrowed.Tremor, "dobras_de_pedra", "aves_das_dobras", Joins, Golem),
            P(Mistura, Borrowed.Tremor, "clavicula_do_gigante", "pedras_da_clavicula", Joins, Golem),
            P(Mistura, Borrowed.ErasingWater, "agua_parada", "aguas_que_apagam", Leads, ShadowSeal),
            P(Mistura, Borrowed.ErasingWater, "tabuas_quebradas", "aguas_das_tabuas", Leads, ShadowSeal),
            P(Mistura, Borrowed.ErasingWater, "agua_parada", "jardins_da_agua", Joins, ShadowSeal),
            P(Mistura, Borrowed.ErasingWater, "tabuas_quebradas", "tabuas_quebradas", Joins, ShadowSeal),

            P(Ceu, Borrowed.StowawayShadow, "pier_da_lua", "sombras_clandestinas", Leads, GardenShadow),
            P(Ceu, Borrowed.StowawayShadow, "fresta_das_luzinhas", "sombras_da_fresta", Leads, GardenShadow),
            P(Ceu, Borrowed.StowawayShadow, "beira_oeste", "faiscas_da_beira", Joins, GardenShadow),
            P(Ceu, Borrowed.StowawayShadow, "cais_da_coluna", "aves_do_cais", Joins, GardenShadow),
            P(Ceu, Borrowed.SwimmingShadow, "foz_oeste", "sombras_nadadoras", Leads, LateShadow),
            P(Ceu, Borrowed.SwimmingShadow, "margens_da_corrente", "sombras_das_margens", Leads, LateShadow),
            P(Ceu, Borrowed.SwimmingShadow, "foz_oeste", "peixes_apagados", Joins, LateShadow),
            P(Ceu, Borrowed.SwimmingShadow, "ilhotas_de_nuvem", "lanternas_fugitivas", Joins, LateShadow),
            P(Ceu, Borrowed.WrongCurrent, "rio_de_luz", "correntes_erradas", Leads, Mist),
            P(Ceu, Borrowed.WrongCurrent, "foz_leste", "correntes_da_foz", Leads, Mist),
            P(Ceu, Borrowed.WrongCurrent, "rio_de_luz", "peixes_do_rio", Joins, Mist),
            P(Ceu, Borrowed.WrongCurrent, "margens_da_corrente", "lanternas_das_margens", Joins, Mist),
            P(Ceu, Borrowed.SkyCrack, "rachaduras_do_oeste", "rachaduras_do_ceu", Leads, Mist),
            P(Ceu, Borrowed.SkyCrack, "rachaduras_do_oeste", "pedras_do_ceu", Joins, Mist),
            P(Ceu, Borrowed.SkyCrack, "varanda_do_tempo_de_depois", "pedras_do_pomar", Joins, Mist),
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

        [TestCase(Ceu, "sombras_das_flores", GardenShadow)]
        [TestCase(Ceu, "sombras_da_pergola", GardenShadow)]
        [TestCase(Ceu, "sombras_do_meio_dia", LateShadow)]
        [TestCase(Gelo, "rachaduras", Ooze)]
        [TestCase(Gelo, "raizes_gelo", Ooze)]
        [TestCase(Fogo, "polvos_de_obsidiana", Ooze)]
        public void TheSheetsThatSharedTheStandIn_KeepIt(string region, string group, string standIn)
        {
            OverworldFauna fauna = OverworldLayouts.Get(region).Zones.SelectMany(z => z.Fauna).FirstOrDefault(f => f.Id == group);
            Assert.IsNotNull(fauna, group);
            Assert.IsTrue(fauna.Day.Any(e => e.EnemyId == standIn), $"{group}: still {standIn} by day");
        }

        [Test]
        public void TheMist_NoLongerSpawnsByDay_AnywhereInTheSky()
        {
            OverworldLayout w = OverworldLayouts.Get(Ceu);
            Assert.IsFalse(w.World.Zones.SelectMany(s => s.Day).Any(e => e.EnemyId == Mist), "the Corrente Errada and the Rachadura do Céu were the last mists of the Sky");
        }
    }
}
