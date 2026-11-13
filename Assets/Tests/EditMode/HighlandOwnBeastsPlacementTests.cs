using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rapadura.Gameplay.Night;
using Rapadura.Gameplay.World.Map;
using Rapadura.Gameplay.World.Map.Areas;
using Rapadura.Gameplay.World.Map.Overworld;
using Placement = Rapadura.Tests.MingchengNartesBeastsPlacementTests.Placement;
using Highland = Rapadura.Gameplay.World.Map.Overworld.OverworldLayouts.HighlandBeasts;

namespace Rapadura.Tests
{
    /// <summary>
    /// The twentieth pass of beasts (<see cref="HighlandOwnBeastsTests"/>) now stands where its stand-ins stood in the Teto do Mundo and, for two
    /// roles, in the Planície de Mingcheng: each beast is in the day table of the groups of its sheet, its night form in the same group's night
    /// table where it leads, the stand-in left those groups' day tables, and every case is checked on the clearings the group really got
    /// (OverworldLayouts.GenerateClearings, after the trails took theirs), at that area's levels. Same idea as
    /// <see cref="LentRoleBeastsPlacementTests"/>. Every new beast leads at least one drawn clearing, and the rolling stone, the golem, the
    /// ill-wind mist, the broken lantern, the hawk, the bats, the crowned orc and the crow no longer spawn by day anywhere on the Teto.
    /// </summary>
    public class HighlandOwnBeastsPlacementTests
    {
        private const string RollingStone = "enemy_mountain_rolling_stone";
        private const string Golem = PeriodSpawnTable.MountainGolem;
        private const string Mist = "enemy_mountain_ill_wind_mist";
        private const string Lantern = "enemy_desert_broken_lantern";
        private const string Hawk = "enemy_mountain_crested_hawk";
        private const string Bats = PeriodSpawnTable.CaveBats;
        private const string Orc = "enemy_desert_crowned_orc";
        private const string Crow = "enemy_cave_dark_crow";
        private const string Yak = "enemy_mountain_wild_yak";

        private const string Teto = OverworldLayouts.TetoId;
        private const string Ming = OverworldLayouts.MingchengId;

        private const bool Leads = true;
        private const bool Joins = false;

        private static Placement T(string enemy, string area, string group, bool withNightForm, params string[] standIns) =>
            new Placement(Teto, enemy, area, group, withNightForm, standIns);

        private static Placement M(string enemy, string area, string group, bool withNightForm, params string[] standIns) =>
            new Placement(Ming, enemy, area, group, withNightForm, standIns);

        private static readonly Placement[] Placements =
        {
            T(Highland.MoraineRoller, "terracos_de_sangkent", "pedras_rolantes", Leads, RollingStone),
            T(Highland.MoraineRoller, "rio_de_degelo", "pedras_do_degelo", Leads, RollingStone),
            T(Highland.MoraineRoller, "calhas_geladas", "pedras_das_calhas", Leads, RollingStone),
            T(Highland.MoraineRoller, "terracos_altos", "pedras_dos_terracos", Leads, RollingStone),
            T(Highland.MoraineRoller, "morenas_da_geleira", "pedras_das_morenas", Leads, RollingStone),
            T(Highland.MoraineRoller, "calhas_de_pedra", "ushabtis_de_calha", Joins, RollingStone),
            T(Highland.MoraineRoller, "calhas_de_pedra", "marmotas_das_calhas", Joins, RollingStone),
            T(Highland.MoraineRoller, "terracos_altos", "gralhas_dos_terracos_altos", Joins, RollingStone),

            T(Highland.ChuteUshabti, "calhas_de_pedra", "ushabtis_de_calha", Leads, Golem),
            T(Highland.ChuteUshabti, "estacao_da_calha_alta", "ushabtis_da_estacao", Leads, Golem),
            T(Highland.ChuteUshabti, "calhas_geladas", "abutres_das_calhas_geladas", Joins, Golem),

            T(Highland.CrestMist, "estacao_da_calha_alta", "neblinas_do_vento_ruim", Leads, Mist),
            T(Highland.CrestMist, "rio_de_degelo", "neblinas_do_degelo", Leads, Mist),
            T(Highland.CrestMist, "lago_turquesa", "neblinas_do_lago", Leads, Mist),
            T(Highland.CrestMist, "cristas_do_vento_ruim", "neblinas_das_cristas", Leads, Mist),
            T(Highland.CrestMist, "estacao_da_calha_alta", "ushabtis_da_estacao", Joins, Mist),
            T(Highland.CrestMist, "calhas_geladas", "pedras_das_calhas", Joins, Mist),
            T(Highland.CrestMist, "geleira_do_teto_do_mundo", "fendas_vivas", Joins, Mist),
            T(Highland.CrestMist, "torres_de_cata_vento", "morcegos_das_torres", Joins, Mist),
            T(Highland.CrestMist, "picos_gelados", "lobos_dos_picos", Joins, Mist),

            T(Highland.SmokeKite, "forte_dos_ventos", "pipas_de_fumaca", Leads, Lantern),
            T(Highland.SmokeKite, "desfiladeiro_dos_ventos", "pipas_do_desfiladeiro", Leads, Lantern),
            T(Highland.SmokeKite, "forte_dos_ventos", "guardas_de_pedra", Joins, Lantern),
            T(Highland.SmokeKite, "torres_de_cata_vento", "gavioes_dos_cata_ventos", Joins, Lantern),

            T(Highland.DimmedRoyalHawk, "desfiladeiro_dos_ventos", "gavioes_reais", Leads, Hawk),
            T(Highland.DimmedRoyalHawk, "torres_de_cata_vento", "gavioes_dos_cata_ventos", Leads, Hawk),
            T(Highland.DimmedRoyalHawk, "estepe_alta", "gavioes_da_estepe", Leads, Hawk),
            T(Highland.DimmedRoyalHawk, "estepe_alta", "colonias_de_marmotas", Joins, Hawk),
            T(Highland.DimmedRoyalHawk, "vale_dos_damasqueiros", "gralhas_dos_pomares", Joins, Hawk),
            T(Highland.DimmedRoyalHawk, "lago_turquesa", "neblinas_do_lago", Joins, Hawk),
            T(Highland.DimmedRoyalHawk, "forte_dos_ventos", "pipas_de_fumaca", Joins, Hawk),
            T(Highland.DimmedRoyalHawk, "forte_dos_ventos", "morcegos_do_vento", Joins, Hawk),
            T(Highland.DimmedRoyalHawk, "cristas_do_vento_ruim", "neblinas_das_cristas", Joins, Hawk),

            T(Highland.WindBat, "forte_dos_ventos", "morcegos_do_vento", Leads, Bats),
            T(Highland.WindBat, "torres_de_cata_vento", "morcegos_das_torres", Leads, Bats),
            T(Highland.WindBat, "desfiladeiro_dos_ventos", "gavioes_reais", Joins, Bats),
            T(Highland.WindBat, "cristas_do_vento_ruim", "guardas_das_cristas", Joins, Bats),

            T(Highland.StoneGuardUshabti, "forte_dos_ventos", "guardas_de_pedra", Leads, Golem),
            T(Highland.StoneGuardUshabti, "cristas_do_vento_ruim", "guardas_das_cristas", Leads, Golem),
            T(Highland.StoneGuardUshabti, "desfiladeiro_dos_ventos", "pipas_do_desfiladeiro", Joins, Golem),
            T(Highland.StoneGuardUshabti, "ninho_de_aydahar", "guardas_do_ouro_de_az", Joins, Golem),

            T(Highland.AzBandit, "passo_de_wakhan", "bandoleiros_de_az", Leads, Orc),
            T(Highland.AzBandit, "alto_do_wakhan", "bandoleiros_do_alto", Leads, Orc),
            T(Highland.AzBandit, "ninho_de_aydahar", "guardas_do_ouro_de_az", Leads, Orc),
            T(Highland.AzBandit, "ninho_de_aydahar", "abutres_do_ninho", Joins, Orc),

            T(Highland.DimmedArgali, "estepe_alta", "argalis_da_estepe", Leads, Yak),
            T(Highland.DimmedArgali, "vale_do_wakhan", "argalis_do_vale", Leads, Yak),
            T(Highland.DimmedArgali, "passo_de_wakhan", "argalis_apagados", Leads, Yak),

            T(Highland.SnowChough, "passo_de_wakhan", "gralhas_das_neves", Leads, Crow),
            T(Highland.SnowChough, "terracos_de_sangkent", "marmotas_gigantes", Joins, Crow),
            T(Highland.SnowChough, "vale_dos_damasqueiros", "marmotas_dos_pomares", Joins, Crow),
            T(Highland.SnowChough, "estepe_alta", "gavioes_da_estepe", Joins, Crow),
            T(Highland.SnowChough, "rio_de_degelo", "neblinas_do_degelo", Joins, Crow),
            T(Highland.SnowChough, "encosta_dos_zimbros", "ursos_dos_zimbros", Joins, Crow),
            T(Highland.SnowChough, "calhas_de_pedra", "abutres_barbudos", Joins, Crow),
            T(Highland.SnowChough, "campos_de_neve_de_sangkent", "lobos_das_neves", Joins, Crow),
            T(Highland.SnowChough, "morenas_da_geleira", "lobos_das_morenas", Joins, Crow),
            T(Highland.SnowChough, "vale_do_wakhan", "argalis_do_vale", Joins, Crow),
            T(Highland.SnowChough, "vale_do_wakhan", "marmotas_do_vale", Joins, Crow),
            T(Highland.SnowChough, "passo_de_wakhan", "argalis_apagados", Joins, Crow),
            T(Highland.SnowChough, "alto_do_wakhan", "irbis_apagados", Joins, Crow),

            M(Highland.TerracottaArcher, "vale_dos_guerreiros", "arqueiros_de_papel", Leads, Lantern),
            M(Highland.TerracottaArcher, "muralha_das_mil_torres", "arqueiros_de_terracota", Leads, Lantern),
            M(Highland.TerracottaArcher, "portao_do_nian", "arqueiros_do_portao", Leads, Lantern),
            M(Highland.TerracottaArcher, "muralha_das_mil_torres", "soldados_da_muralha", Joins, Lantern),
            M(Highland.TerracottaArcher, "torre_quinhentos", "morcegos_das_torres_de_vigia", Joins, Lantern),

            M(Highland.LightIbis, "jardins_em_terraco", "ibis_de_luz", Leads, Hawk),
            M(Highland.LightIbis, "jardins_em_terraco", "espelhos_quebrados", Joins, Hawk),
            M(Highland.LightIbis, "colinas_dos_espelhos", "guardioes_das_colinas", Joins, Hawk),
        };

        private static IEnumerable<Placement> All() => Placements;

        /// <summary>The twelve beasts of the pass and their region (every one must lead a drawn clearing there).</summary>
        private static IEnumerable<TestCaseData> Beasts() => new[]
        {
            Highland.MoraineRoller, Highland.ChuteUshabti, Highland.CrestMist, Highland.SmokeKite, Highland.DimmedRoyalHawk, Highland.WindBat,
            Highland.StoneGuardUshabti, Highland.AzBandit, Highland.DimmedArgali, Highland.SnowChough,
        }.Select(e => new TestCaseData(Teto, e)).Concat(new[] { Highland.TerracottaArcher, Highland.LightIbis }.Select(e => new TestCaseData(Ming, e)));

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

        [TestCaseSource(nameof(Beasts))]
        public void EachBeast_LeadsAtLeastOneDrawnClearing_InItsRegion(string region, string enemy)
        {
            OverworldLayout w = OverworldLayouts.Get(region);
            List<MapSpawnZone> clearings = w.World.Zones.Where(s => s.Day.Any(e => e.EnemyId == enemy)).ToList();
            Assert.IsNotEmpty(clearings, $"{enemy}: no clearing of {region} spawns it (the trails or the draw took them all)");
            Assert.IsTrue(clearings.Any(s => s.Day.Where(e => e.EnemyId == enemy).Sum(e => e.Weight) >= 3), $"{enemy}: leads at least one clearing (weight 3)");
        }

        [Test]
        public void TheStandIns_NoLongerSpawnByDay_AnywhereOnTheRoofOfTheWorld()
        {
            OverworldLayout w = OverworldLayouts.Get(Teto);
            foreach (string standIn in new[] { RollingStone, Golem, Mist, Lantern, Hawk, Bats, Orc, Crow })
            {
                Assert.IsFalse(w.World.Zones.SelectMany(s => s.Day).Any(e => e.EnemyId == standIn), $"{standIn}: its roles on the Teto have their own beasts now");
            }
        }

        [Test]
        public void TheWildYak_KeepsItsOwnHerds_TheArgaliTookOnlyTheArgaliGroups()
        {
            OverworldLayout w = OverworldLayouts.Get(Teto);
            foreach (string group in new[] { "iaques_selvagens", "iaques_da_geleira" })
            {
                OverworldFauna fauna = w.Zones.SelectMany(z => z.Fauna).First(f => f.Id == group);
                Assert.IsTrue(fauna.Day.Any(e => e.EnemyId == Yak), $"{group}: still the Iaque Selvagem Apagado");
            }

            foreach (string group in new[] { "argalis_da_estepe", "argalis_do_vale", "argalis_apagados" })
            {
                OverworldFauna fauna = w.Zones.SelectMany(z => z.Fauna).First(f => f.Id == group);
                Assert.IsTrue(fauna.Night.Any(e => e.SpeciesId == NightVariantRules.Queixada.Id), $"{group}: the herd still comes down at night");
            }
        }

        [Test]
        public void TheHawk_NoLongerSpawnsByDay_OnThePlainOfMingcheng()
        {
            OverworldLayout w = OverworldLayouts.Get(Ming);
            Assert.IsFalse(w.World.Zones.SelectMany(s => s.Day).Any(e => e.EnemyId == Hawk), "the Íbis de Luz Apagada took the hawk's last roles");
        }

        [TestCase("espantalhos_apagados")]
        [TestCase("grous_das_vilas")]
        public void TheScarecrowsAndTheVillageCranes_KeepTheBrokenLantern_ForALaterPass(string group)
        {
            OverworldFauna fauna = OverworldLayouts.Get(Ming).Zones.SelectMany(z => z.Fauna).First(f => f.Id == group);
            Assert.IsTrue(fauna.Day.Any(e => e.EnemyId == Lantern), $"{group}: still the broken lantern by day");
        }
    }
}
