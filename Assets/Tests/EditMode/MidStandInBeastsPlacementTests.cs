using System.Collections.Generic;
using NUnit.Framework;
using Rapadura.Gameplay.Night;
using Rapadura.Gameplay.World.Map;
using Rapadura.Gameplay.World.Map.Areas;
using Rapadura.Gameplay.World.Map.Overworld;
using Placement = Rapadura.Tests.MingchengNartesBeastsPlacementTests.Placement;
using Mid = Rapadura.Gameplay.World.Map.Overworld.OverworldLayouts.MidBeasts;

namespace Rapadura.Tests
{
    /// <summary>
    /// The thirteenth pass of beasts (<see cref="MidStandInBeastsTests"/>) now stands where its stand-ins stood in Regions 5–9: each beast is in the day
    /// table of the groups of its sheet, its night form in the same group's night table where it leads, the stand-in left those groups' day tables,
    /// and every case is checked on the clearings the group really got, at that area's levels. Same idea as <see cref="EarlyStandInBeastsPlacementTests"/>.
    /// The middle story's own targets in these areas (Gaivota Ladra, Crocodilo do Mangue, Urso-Pardo, Almas, Tartaruga de Pedra, Soldado de
    /// Terracota…) keep their groups: <see cref="MidStoryCatalogTests"/> still finds them in the fauna of the zone the step names.
    /// </summary>
    public class MidStandInBeastsPlacementTests
    {
        private const string Potoo = NightVariantRules.PotooEnemyId;
        private const string Wolf = PeriodSpawnTable.ForestWolf;
        private const string Spider = PeriodSpawnTable.ForestSpider;
        private const string Gull = "enemy_forest_thief_gull";
        private const string CoconutCrab = "enemy_forest_coconut_crab";
        private const string SandSnake = "enemy_desert_sand_snake";
        private const string EmberVulture = "enemy_desert_ember_vulture";
        private const string Ooze = "enemy_cave_ooze";
        private const string Crow = "enemy_cave_dark_crow";

        private const string Costa = OverworldLayouts.CostaId;
        private const string Delta = OverworldLayouts.DeltaId;
        private const string Teto = OverworldLayouts.TetoId;
        private const string Ming = OverworldLayouts.MingchengId;
        private const string Nart = OverworldLayouts.NartesId;

        private const bool Leads = true;
        private const bool Joins = false;

        private static Placement P(string region, string enemy, string area, string group, bool withNightForm, params string[] standIns) =>
            new Placement(region, enemy, area, group, withNightForm, standIns);

        private static readonly Placement[] Placements =
        {
            P(Costa, Mid.Moray, "recife_de_taa_pwani", "moreias", Leads, SandSnake),
            P(Costa, Mid.SmokeUrchin, "lagoas_de_mare", "lodo_da_lagoa", Leads, Ooze),
            P(Costa, Mid.SmokeUrchin, "praia_dos_dhows", "caranguejos_dos_cascos", Joins, Spider),
            P(Costa, Mid.LightningBird, "savana_dos_auroques", "aves_relampago", Leads, EmberVulture),
            P(Costa, Mid.LightningBird, "savana_das_acacias", "aves_das_acacias", Leads, EmberVulture),
            P(Costa, Mid.LightningBird, "bosque_dos_baobas", "urutaus", Leads, Potoo),
            P(Costa, Mid.FiddlerCrab, "mangue_raso", "caranguejos_violinistas", Leads, Spider),
            P(Costa, Mid.FiddlerCrab, "mangues_de_liyongo", "violinistas_de_liyongo", Leads, CoconutCrab),

            P(Delta, Mid.SpearHeron, "lago_de_jhilpur", "garcas_lanca", Leads, Gull),
            P(Delta, Mid.SpearHeron, "diques_do_norte", "garcas_dos_diques", Leads, Gull),
            P(Delta, Mid.SpearHeron, "varzea_dos_bufalos", "garcas_da_varzea", Leads, Gull),
            P(Delta, Mid.SpearHeron, "lagoas_de_lotus", "garcas_do_lotus", Leads, Gull),
            P(Delta, Mid.SpearHeron, "bancos_de_lama", "garcas_da_lama", Leads, Gull),
            P(Delta, Mid.MireLeech, "floresta_de_chuva", "sanguessugas_da_chuva", Leads, Ooze),
            P(Delta, Mid.MireLeech, "lagoas_de_lotus", "sanguessugas_do_lotus", Leads, Ooze),
            P(Delta, Mid.MireLeech, "gargantas_esquecidas", "sanguessugas_da_garganta", Leads, Ooze),

            P(Teto, Mid.RedBilledChough, "terracos_de_sangkent", "gralhas_de_bico_vermelho", Leads, Crow),
            P(Teto, Mid.RedBilledChough, "vale_dos_damasqueiros", "gralhas_dos_pomares", Leads, Crow),
            P(Teto, Mid.RedBilledChough, "choupal_dos_canais", "gralhas_dos_canais", Leads, Crow),
            P(Teto, Mid.RedBilledChough, "terracos_altos", "gralhas_dos_terracos_altos", Leads, Crow),
            P(Teto, Mid.BeardedVulture, "calhas_de_pedra", "abutres_barbudos", Leads, EmberVulture),
            P(Teto, Mid.BeardedVulture, "calhas_geladas", "abutres_das_calhas_geladas", Leads, EmberVulture),
            P(Teto, Mid.BeardedVulture, "lago_turquesa", "abutres_do_lago", Leads, EmberVulture),
            P(Teto, Mid.BeardedVulture, "vale_do_wakhan", "abutres_do_vale", Leads, EmberVulture),
            P(Teto, Mid.BeardedVulture, "ninho_de_aydahar", "abutres_do_ninho", Leads, EmberVulture),
            P(Teto, Mid.IceWolf, "campo_de_seracs", "lobos_de_gelo", Leads, Wolf),
            P(Teto, Mid.IceWolf, "picos_gelados", "lobos_dos_picos", Leads, Wolf),
            P(Teto, Mid.IceWolf, "morenas_da_geleira", "lobos_das_morenas", Leads, Wolf),

            P(Ming, Mid.DragonCarp, "eclusa_da_lua", "carpas_da_eclusa", Leads, SandSnake),
            P(Ming, Mid.DragonCarp, "rio_dos_salgueiros", "carpas_do_rio", Leads, SandSnake),
            P(Ming, Mid.TerracottaHorse, "fossos_de_terracota", "cavalos_de_terracota", Leads, Wolf),
            P(Ming, Mid.TerracottaHorse, "vale_dos_guerreiros", "cavalos_do_vale", Leads, Wolf),
            P(Ming, Mid.TerracottaHorse, "torre_quinhentos", "cavalos_da_muralha", Leads, Wolf),

            P(Nart, Mid.FeltSwan, "vau_largo", "cisnes_do_vau", Leads, Gull),
            P(Nart, Mid.FeltSwan, "lago_salgado", "cisnes_do_lago_salgado", Leads, Gull),
            P(Nart, Mid.FeltSwan, "kurgan_aberto", "cisnes_de_feltro", Leads, Gull),
            P(Nart, Mid.FeltSwan, "rio_dos_nartes", "cisnes_do_rio", Leads, Gull),
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
