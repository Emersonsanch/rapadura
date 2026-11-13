using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rapadura.Gameplay.Night;
using Rapadura.Gameplay.World.Map;
using Rapadura.Gameplay.World.Map.Areas;
using Rapadura.Gameplay.World.Map.Overworld;
using Placement = Rapadura.Tests.MingchengNartesBeastsPlacementTests.Placement;
using Wide = Rapadura.Gameplay.World.Map.Overworld.OverworldLayouts.WideBeasts;

namespace Rapadura.Tests
{
    /// <summary>
    /// The fifteenth pass of beasts (<see cref="WideStandInBeastsTests"/>) now stands where its stand-ins stood in Regions 5–16: each beast is in the
    /// day table of the groups of its sheet, its night form in the same group's night table where it leads, the stand-in left those groups' day
    /// tables, and every case is checked on the clearings the group really got, at that area's levels. Same idea as
    /// <see cref="FarStandInBeastsPlacementTests"/>. The roles that shared the region constant keep their stand-in (the Ushabti de Bronze Kushita,
    /// the Espantalho and the Arqueiros de Terracota, the Íbis de Luz's lead, the other Nartes wolves and mists).
    /// </summary>
    public class WideStandInBeastsPlacementTests
    {
        private const string EmberVulture = "enemy_desert_ember_vulture";
        private const string Wolf = PeriodSpawnTable.ForestWolf;
        private const string Mist = "enemy_mountain_ill_wind_mist";
        private const string BrokenLantern = "enemy_desert_broken_lantern";
        private const string Peccary = NightVariantRules.PeccaryEnemyId;
        private const string LanternFish = "enemy_cave_lantern_fish";
        private const string Hawk = "enemy_mountain_crested_hawk";
        private const string CaveBats = PeriodSpawnTable.CaveBats;
        private const string Golem = PeriodSpawnTable.MountainGolem;
        private const string Firefly = NightVariantRules.FireflySwarmEnemyId;
        private const string Ooze = "enemy_cave_ooze";

        private const string Costa = OverworldLayouts.CostaId;
        private const string Delta = OverworldLayouts.DeltaId;
        private const string Ming = OverworldLayouts.MingchengId;
        private const string Nart = OverworldLayouts.NartesId;
        private const string Ceu = OverworldLayouts.CeuRegionId;

        private const bool Leads = true;
        private const bool Joins = false;

        private static Placement P(string region, string enemy, string area, string group, bool withNightForm, params string[] standIns) =>
            new Placement(region, enemy, area, group, withNightForm, standIns);

        private static readonly Placement[] Placements =
        {
            P(Nart, Wide.SteppeVulture, "estepe_das_saigas", "abutres_do_absinto", Leads, EmberVulture),
            P(Nart, Wide.SteppeVulture, "campos_de_papoula", "abutres_da_papoula", Leads, EmberVulture),
            P(Nart, Wide.SteppeVulture, "pradaria_ondulada", "abutres_da_estepe", Leads, EmberVulture),
            P(Nart, Wide.SteppeVulture, "pradaria_dos_mil_cavalos", "abutres_dos_cavalos", Leads, EmberVulture),
            P(Nart, Wide.SteppeWolf, "vau_largo", "lobos_do_vau", Leads, Wolf),
            P(Nart, Wide.SteppeWolf, "pistas_de_corrida", "lobos_da_estepe", Leads, Wolf),
            P(Nart, Wide.SteppeWolf, "estepe_de_soslan", "lobos_de_soslan", Leads, Wolf),
            P(Nart, Wide.SteppeWolf, "bosque_de_betulas", "lobos_das_betulas", Leads, Wolf),
            P(Nart, Wide.SteppeWolf, "estepe_dos_ventos", "aguias_dos_ventos", Joins, Wolf),
            P(Nart, Wide.GrassStorm, "estepe_dos_ventos", "tempestades_dos_ventos", Leads, Mist),
            P(Nart, Wide.GrassStorm, "campos_de_papoula", "tempestades_de_capim", Leads, Mist),
            P(Nart, Wide.GrassStorm, "pradaria_dos_mil_cavalos", "tempestades_da_pradaria", Leads, Mist),
            P(Nart, Wide.GrassStorm, "pradaria_ondulada", "lobos_cinzentos", Joins, Mist),

            P(Delta, Wide.MarshWisp, "ilhas_de_junco", "fogos_fatuos", Leads, BrokenLantern),
            P(Delta, Wide.MarshWisp, "pantano_aberto", "fogos_fatuos_do_pantano", Leads, BrokenLantern),
            P(Delta, Wide.MarshWisp, "palafitas_de_tarchon", "fogos_fatuos_presos", Leads, BrokenLantern),
            P(Delta, Wide.MarshWisp, "ilhas_que_andam", "fogos_das_ilhas", Leads, BrokenLantern),
            P(Delta, Wide.BullFrog, "brejo_das_palmeiras", "sapos_boi", Leads, Peccary),
            P(Delta, Wide.BullFrog, "charco_do_ninki_nanka", "sapos_boi_do_charco", Leads, Peccary),
            P(Delta, Wide.BullFrog, "toca_da_dragoa", "sapos_da_toca", Leads, Peccary),
            P(Delta, Wide.Mudskipper, "lago_de_jhilpur", "peixes_saltadores", Leads, LanternFish),
            P(Delta, Wide.Mudskipper, "bancos_de_jhilpur", "saltadores_dos_bancos", Leads, LanternFish),
            P(Delta, Wide.Mudskipper, "charcos_de_lotus", "peixes_do_charco", Leads, LanternFish),
            P(Delta, Wide.Mudskipper, "lagoas_de_lotus", "sanguessugas_do_lotus", Joins, LanternFish),

            P(Ming, Wide.BrokenMirror, "jardins_em_terraco", "espelhos_quebrados", Leads, BrokenLantern),
            P(Ming, Wide.BrokenMirror, "torres_brancas", "espelhos_das_torres", Leads, BrokenLantern),
            P(Ming, Wide.BrokenMirror, "colinas_dos_espelhos", "espelhos_do_atalho", Leads, BrokenLantern),
            P(Ming, Wide.PaperCrane, "aneis_de_mingcheng", "grous_de_papel", Leads, Hawk),
            P(Ming, Wide.PaperCrane, "vilas_do_anel", "grous_das_vilas", Leads, Hawk),
            P(Ming, Wide.PaperCrane, "colinas_de_cha", "grous_do_cha", Leads, Hawk),
            P(Ming, Wide.SmokeLocust, "campos_de_painco", "gafanhotos_do_painco", Leads, CaveBats),
            P(Ming, Wide.SmokeLocust, "arrozais_espelhados", "gafanhotos_de_fumaca", Leads, CaveBats),
            P(Ming, Wide.SmokeLocust, "colinas_de_cha", "gafanhotos_do_cha", Leads, CaveBats),

            P(Costa, Wide.CoralUshabti, "ruinas_de_gedi", "ushabtis_de_coral", Leads, Golem),
            P(Costa, Wide.CoralUshabti, "kilwa_afundada", "ushabtis_de_kilwa", Leads, Golem),
            P(Costa, Wide.CoralUshabti, "palacio_de_husuni", "guardas_de_coral", Leads, Golem),

            P(Ceu, Wide.ConfusedSpark, "beira_oeste", "faiscas_da_beira", Leads, Firefly),
            P(Ceu, Wide.ConfusedSpark, "cais_da_coluna", "faiscas_do_cais", Leads, Firefly),
            P(Ceu, Wide.ConfusedSpark, "pier_do_sol", "faiscas_do_sol", Leads, Firefly),
            P(Ceu, Wide.DarkDrop, "engrenagens_oeste", "gotas_do_oeste", Leads, Ooze),
            P(Ceu, Wide.DarkDrop, "jarras_oeste", "gotas_das_jarras", Leads, Ooze),
            P(Ceu, Wide.DarkDrop, "engrenagens_leste", "gotas_do_leste", Leads, Ooze),
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

        // The Ushabti de Bronze Kushita of Meroë (27) has its own body now (CoastOwnBeastsPlacementTests).
        [TestCase(Ming, "espantalhos_apagados", BrokenLantern)]
        [TestCase(Nart, "lobos_cinzentos", Wolf)]
        [TestCase(Nart, "neblinas_do_rio", Mist)]
        public void TheSheetsThatSharedTheConstant_KeepTheirStandIn(string region, string group, string standIn)
        {
            OverworldFauna fauna = OverworldLayouts.Get(region).Zones.SelectMany(z => z.Fauna).FirstOrDefault(f => f.Id == group);
            Assert.IsNotNull(fauna, group);
            Assert.IsTrue(fauna.Day.Any(e => e.EnemyId == standIn), $"{group}: still {standIn} by day");
        }

        [TestCase("sapos_boi")]
        [TestCase("sapos_boi_do_charco")]
        [TestCase("sapos_da_toca")]
        public void TheBullfrogs_SingAtNightThemselves_NotThePeccaryHerd(string group)
        {
            OverworldFauna fauna = OverworldLayouts.Get(Delta).Zones.SelectMany(z => z.Fauna).First(f => f.Id == group);
            Assert.IsFalse(fauna.Night.Any(e => e.SpeciesId == NightVariantRules.Queixada.Id), $"{group}: no queixada herd at night any more");
            Assert.IsTrue(fauna.Night.Any(e => e.EnemyId == Wide.BullFrog), group);
        }
    }
}
