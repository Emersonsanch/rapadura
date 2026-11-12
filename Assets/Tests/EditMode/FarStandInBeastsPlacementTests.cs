using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rapadura.Gameplay.Night;
using Rapadura.Gameplay.World.Map;
using Rapadura.Gameplay.World.Map.Areas;
using Rapadura.Gameplay.World.Map.Overworld;
using Placement = Rapadura.Tests.MingchengNartesBeastsPlacementTests.Placement;
using Far = Rapadura.Gameplay.World.Map.Overworld.OverworldLayouts.FarBeasts;

namespace Rapadura.Tests
{
    /// <summary>
    /// The fourteenth pass of beasts (<see cref="FarStandInBeastsTests"/>) now stands where its stand-ins stood in Regions 10–16: each beast is in the
    /// day table of the groups of its sheet, its night form in the same group's night table where it leads, the stand-in left those groups' day
    /// tables, and every case is checked on the clearings the group really got, at that area's levels. Same idea as
    /// <see cref="MidStandInBeastsPlacementTests"/>. The roles the region constants shared with other sheets (the broken lantern of the Sino Rachado
    /// and the Raio Preso, the firefly swarm of the Mosquito da Tundra and the Faísca Raivosa) keep their stand-in.
    /// </summary>
    public class FarStandInBeastsPlacementTests
    {
        private const string EmberVulture = "enemy_desert_ember_vulture";
        private const string BrokenLantern = "enemy_desert_broken_lantern";
        private const string Mist = "enemy_mountain_ill_wind_mist";
        private const string Firefly = NightVariantRules.FireflySwarmEnemyId;
        private const string RollingStone = "enemy_mountain_rolling_stone";
        private const string StormEagle = "enemy_mountain_storm_eagle";
        private const string ShroudOwl = "enemy_forest_shroud_owl";
        private const string ShadowBiter = "enemy_forest_shadow_biter";
        private const string VampireBat = "enemy_cave_vampire_bat";

        private const string Fark = OverworldLayouts.FarnkeshRegionId;
        private const string Gelo = OverworldLayouts.MarGeladoRegionId;
        private const string Fogo = OverworldLayouts.HinokuraRegionId;
        private const string Rio = OverworldLayouts.RioRegionId;
        private const string Mist14 = OverworldLayouts.MisturaRegionId;
        private const string Prof = OverworldLayouts.ProfundoRegionId;
        private const string Ceu = OverworldLayouts.CeuRegionId;

        private const bool Leads = true;
        private const bool Joins = false;

        private static Placement P(string region, string enemy, string area, string group, bool withNightForm, params string[] standIns) =>
            new Placement(region, enemy, area, group, withNightForm, standIns);

        private static readonly Placement[] Placements =
        {
            P(Fark, Far.PeakVulture, "escadaria_dos_cestos", "abutres_do_pico", Leads, EmberVulture),
            P(Fark, Far.PeakVulture, "mar_de_nuvens", "abutres_das_nuvens", Leads, EmberVulture),
            P(Fark, Far.PeakVulture, "bosque_de_pinaculos", "abutres_dos_pinaculos", Leads, EmberVulture),
            P(Fark, Far.PeakVulture, "farnkesh", "gralhas_de_farnkesh", Joins, EmberVulture),
            P(Fark, Far.ShadowKite, "pinaculos_do_leste", "pipas_sombra_do_leste", Leads, BrokenLantern),
            P(Fark, Far.ShadowKite, "ermida_baixa", "pipas_sombra", Leads, BrokenLantern),
            P(Fark, Far.ShadowKite, "pontes_de_vento", "morcegos_vela", Joins, BrokenLantern),

            P(Gelo, Far.DriftingFloe, "banquisas_do_oeste", "gelos_flutuantes", Leads, Mist),
            P(Gelo, Far.DriftingFloe, "mar_das_auroras", "gelos_das_auroras", Leads, Mist),
            P(Gelo, Far.DriftingFloe, "banquisas", "narvais_apagados", Joins, Mist),
            P(Gelo, Far.LittleKul, "fundo_do_velho_kul", "kuls_pequenos", Leads, Firefly),
            P(Gelo, Far.LittleKul, "lago_do_kul", "kuls_do_lago", Leads, Firefly),
            P(Gelo, Far.LittleKul, "fundo_do_velho_kul", "lucios_gigantes", Joins, Firefly),

            P(Fogo, Far.HungryFlame, "caldeira_do_fogo_devorador", "chamas_famintas", Leads, Firefly),
            P(Fogo, Far.HungryFlame, "borda_fria", "chamas_da_borda", Leads, Firefly),
            P(Fogo, Far.HungryFlame, "campos_de_cinza", "chamas_dos_campos", Leads, Firefly),
            P(Fogo, Far.HungryFlame, "caldeira_do_fogo_devorador", "gigantes_de_escoria", Joins, Firefly),
            P(Rio, Far.HungryFlame, "lago_de_fogo", "chamas_famintas_do_lago", Leads, Firefly),
            P(Prof, Far.HungryFlame, "cova_do_fogo_devorador", "chamas_famintas", Leads, Firefly),
            P(Prof, Far.HungryFlame, "chao_que_acende", "chamas_do_chao", Leads, Firefly),

            P(Mist14, Far.DioriteStone, "ombro_de_upelluri", "pedras_de_diorito", Leads, RollingStone),
            P(Mist14, Far.DioriteStone, "clavicula_do_gigante", "pedras_da_clavicula", Leads, RollingStone),
            P(Mist14, Far.DioriteStone, "diorito_crescente", "pedras_do_diorito", Leads, RollingStone),
            P(Mist14, Far.ShoulderBird, "ombro_de_upelluri", "aves_do_ombro", Leads, StormEagle),
            P(Mist14, Far.ShoulderBird, "dobras_de_pedra", "aves_das_dobras", Leads, StormEagle),
            P(Mist14, Far.ShoulderBird, "beiral_do_ceu", "aves_do_beiral", Leads, StormEagle),
            P(Mist14, Far.DreamBird, "campo_das_tabuas", "passaros_sonho", Leads, ShroudOwl),
            P(Mist14, Far.DreamBird, "jardim_cortado", "passaros_do_jardim", Leads, ShroudOwl),
            P(Mist14, Far.DreamBird, "tabuas_do_sonho", "passaros_do_sonho", Leads, ShroudOwl),

            P(Prof, Far.LittleShadow, "fortaleza_ultima_lampada", "sombras_da_muralha", Leads, ShadowBiter),
            P(Prof, Far.LittleShadow, "muralha_em_arco", "sombras_do_arco", Leads, ShadowBiter),
            P(Prof, Far.LittleShadow, "cisternas_de_oleo", "sombras_das_cisternas", Leads, ShadowBiter),
            P(Prof, Far.LittleShadow, "beira_do_abismo", "sombras_do_abismo", Leads, ShadowBiter),
            P(Prof, Far.DarkBat, "fortaleza_ultima_lampada", "morcegos_da_muralha", Leads, VampireBat),
            P(Prof, Far.DarkBat, "nichos_de_basalto", "morcegos_dos_nichos", Leads, VampireBat),
            P(Prof, Far.DarkBat, "beira_do_abismo", "morcegos_do_abismo", Leads, VampireBat),

            P(Ceu, Far.OrbitWind, "orbita_vazia_oeste", "ventos_de_orbita_oeste", Leads, StormEagle),
            P(Ceu, Far.OrbitWind, "esferas_dos_arcontes", "ventos_da_ilhota", Leads, StormEagle),
            P(Ceu, Far.OrbitWind, "orbita_vazia_leste", "ventos_de_orbita", Leads, StormEagle),
            P(Ceu, Far.OrbitWind, "mirante_do_ceu", "ventos_do_mirante", Leads, StormEagle),
            P(Ceu, Far.OrbitWind, "varanda_do_tempo_de_depois", "ventos_do_pomar", Leads, StormEagle),
            P(Ceu, Far.RunawayLantern, "ilhotas_de_nuvem", "lanternas_fugitivas", Leads, BrokenLantern),
            P(Ceu, Far.RunawayLantern, "margens_da_corrente", "lanternas_das_margens", Leads, BrokenLantern),
            P(Ceu, Far.RunawayLantern, "foz_leste", "lanternas_da_foz", Leads, BrokenLantern),
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

        [TestCase(Fark, "sinos_rachados", BrokenLantern)]
        [TestCase(Fark, "raios_presos", BrokenLantern)]
        [TestCase(Gelo, "mosquitos_da_tundra", Firefly)]
        [TestCase(Fogo, "faiscas_raivosas", Firefly)]
        public void TheSheetsThatSharedTheConstant_KeepTheirStandIn(string region, string group, string standIn)
        {
            OverworldFauna fauna = OverworldLayouts.Get(region).Zones.SelectMany(z => z.Fauna).FirstOrDefault(f => f.Id == group);
            Assert.IsNotNull(fauna, group);
            Assert.IsTrue(fauna.Day.Any(e => e.EnemyId == standIn), $"{group}: still {standIn} by day");
        }
    }
}
