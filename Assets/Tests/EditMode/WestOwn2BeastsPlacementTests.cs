using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rapadura.Gameplay.Night;
using Rapadura.Gameplay.World.Cities;
using Rapadura.Gameplay.World.Map;
using Rapadura.Gameplay.World.Map.Areas;
using Rapadura.Gameplay.World.Map.Overworld;
using Placement = Rapadura.Tests.MingchengNartesBeastsPlacementTests.Placement;
using West2 = Rapadura.Gameplay.World.Map.Overworld.OverworldLayouts.WestOwn2Beasts;

namespace Rapadura.Tests
{
    /// <summary>
    /// The twenty-seventh pass of beasts (<see cref="WestOwn2BeastsTests"/>) now stands where its stand-ins stood in the Costa das Monções
    /// (25–29), the Delta de Jhilpur (33, 35) and the Planície de Mingcheng (41, 46, the Vale da Névoa): each beast is in the day table of the
    /// groups of its sheet, its night form in the same group's night table where it leads, the stand-in left those groups' day tables, and every
    /// case is checked on the clearings the group really got, at that area's levels. Same idea as <see cref="WestOwnBeastsPlacementTests"/>.
    /// The Carneiro de Pedra (27), the Dingonek (29) and the Lanterna de Charada (41) got a group of their own. The desert jackal left the Costa,
    /// the Costa's crocodile the Delta, the ill-wind mist and the broken lantern Mingcheng, all by day; and the Lobo de Gelo took the last wolf
    /// packs of the Teto from the forest wolf.
    /// </summary>
    public class WestOwn2BeastsPlacementTests
    {
        private const string Jackal = "enemy_desert_jackal";
        private const string Crocodile = "enemy_forest_mangrove_crocodile";
        private const string Mist = "enemy_mountain_ill_wind_mist";
        private const string Lantern = "enemy_desert_broken_lantern";
        private const string Wolf = PeriodSpawnTable.ForestWolf;

        private const string Costa = OverworldLayouts.CostaId;
        private const string Delta = OverworldLayouts.DeltaId;
        private const string Ming = OverworldLayouts.MingchengId;
        private const string Teto = OverworldLayouts.TetoId;

        private const bool Leads = true;
        private const bool Joins = false;

        private static Placement C(string enemy, string area, string group, bool withNightForm, params string[] standIns) =>
            new Placement(Costa, enemy, area, group, withNightForm, standIns);

        private static Placement D(string enemy, string area, string group, bool withNightForm, params string[] standIns) =>
            new Placement(Delta, enemy, area, group, withNightForm, standIns);

        private static Placement M(string enemy, string area, string group, bool withNightForm, params string[] standIns) =>
            new Placement(Ming, enemy, area, group, withNightForm, standIns);

        private static Placement T(string enemy, string area, string group, bool withNightForm, params string[] standIns) =>
            new Placement(Teto, enemy, area, group, withNightForm, standIns);

        private static readonly Placement[] Placements =
        {
            C(West2.StripedJackal, "dunas_das_casuarinas", "chacais_das_dunas", Leads, Jackal),
            C(West2.StripedJackal, "praia_dos_dhows", "gaivotas_dos_dhows", Joins, Jackal),
            C(West2.StripedJackal, "savana_dos_auroques", "leoes_do_capim", Joins, Jackal),
            C(West2.StripedJackal, "savana_dos_auroques", "rinocerontes_de_po", Joins, Jackal),
            C(West2.StripedJackal, "savana_das_acacias", "hienas_de_fumaca", Joins, Jackal),

            C(West2.SmokeHyena, "savana_das_acacias", "hienas_de_fumaca", Leads, Jackal, Wolf),

            C(West2.StoneRam, "piramides_altas_de_meroe", "carneiros_de_pedra", Leads),

            C(West2.Dingonek, "kilwa_afundada", "dingoneks", Leads),

            D(West2.MossCrocodile, "pantano_aberto", "crocodilos_de_musgo", Leads, Crocodile),
            D(West2.MossCrocodile, "ilhas_que_andam", "crocodilos_das_ilhas", Leads, Crocodile),

            D(West2.ShadowGharial, "raizes_afogadas", "gaviais_sombra", Leads, Crocodile),
            D(West2.ShadowGharial, "coracao_do_lodo", "gaviais_sombra_do_lodo", Leads, Crocodile),
            D(West2.ShadowGharial, "olho_do_lodo", "gaviais_do_olho", Leads, Crocodile),

            M(West2.ShenMist, "vale_da_nevoa", "neblinas_do_vale", Leads, Mist),

            M(West2.HundunBubble, "cidadela_dos_quatro_gulosos", "bolhas_de_hundun", Leads, Mist),
            M(West2.HundunBubble, "cidadela_dos_quatro_gulosos", "filhotes_de_qiongqi", Joins, Mist),

            M(West2.RiddleLantern, "aneis_de_mingcheng", "lanternas_de_charada", Leads),
            M(West2.RiddleLantern, "vilas_do_anel", "grous_das_vilas", Joins, Lantern),
            M(West2.RiddleLantern, "cidadela_dos_quatro_gulosos", "bolhas_de_hundun", Joins, Lantern),

            T(West2.TetoIceWolf, "encosta_dos_zimbros", "lobos_dos_zimbros", Leads, Wolf),
            T(West2.TetoIceWolf, "pastagens_dos_iaques", "lobos_das_pastagens", Leads, Wolf),
            T(West2.TetoIceWolf, "campos_de_neve_de_sangkent", "lobos_das_neves", Leads, Wolf),
        };

        private static IEnumerable<Placement> All() => Placements;

        /// <summary>The nine beasts of the pass and their region (every one must lead a drawn clearing there).</summary>
        private static IEnumerable<TestCaseData> Beasts() => new[]
        {
            new TestCaseData(Costa, West2.StripedJackal), new TestCaseData(Costa, West2.SmokeHyena), new TestCaseData(Costa, West2.StoneRam),
            new TestCaseData(Costa, West2.Dingonek), new TestCaseData(Delta, West2.MossCrocodile), new TestCaseData(Delta, West2.ShadowGharial),
            new TestCaseData(Ming, West2.ShenMist), new TestCaseData(Ming, West2.HundunBubble), new TestCaseData(Ming, West2.RiddleLantern),
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

        [TestCaseSource(nameof(Beasts))]
        public void EachBeast_LeadsAtLeastOneDrawnClearing_InItsRegion(string region, string enemy)
        {
            OverworldLayout w = OverworldLayouts.Get(region);
            List<MapSpawnZone> clearings = w.World.Zones.Where(s => s.Day.Any(e => e.EnemyId == enemy)).ToList();
            Assert.IsNotEmpty(clearings, $"{enemy}: no clearing of {region} spawns it (the trails or the draw took them all)");
            Assert.IsTrue(clearings.Any(s => s.Day.Where(e => e.EnemyId == enemy).Sum(e => e.Weight) >= 3), $"{enemy}: leads at least one clearing (weight 3)");
        }

        [TestCase(Costa, Jackal)]
        [TestCase(Delta, Crocodile)]
        [TestCase(Ming, Mist)]
        [TestCase(Ming, Lantern)]
        public void TheStandIns_NoLongerSpawnByDay_AnywhereInTheRegion(string region, string standIn)
        {
            OverworldLayout w = OverworldLayouts.Get(region);
            Assert.IsFalse(w.World.Zones.SelectMany(s => s.Day).Any(e => e.EnemyId == standIn), $"{standIn}: its roles in {region} have their own beasts now");
        }

        [Test]
        public void TheDingonek_ProwlsTheRaysStreets_AtNight()
        {
            OverworldFauna rays = OverworldLayouts.Get(Costa).Zone("kilwa_afundada").Fauna.First(f => f.Id == "raias_de_fumaca");
            Assert.IsTrue(rays.Night.Any(e => e.EnemyId == West2.Dingonek && e.Form != NightForm.Normal), "the dingonek walks Kilwa at night (the sheet's Noite)");
        }

        [Test]
        public void TheTetosForestWolf_StaysOnlyWithTheHerds()
        {
            OverworldLayout w = OverworldLayouts.Get(Teto);
            Assert.IsFalse(w.Zones.SelectMany(z => z.Fauna).Any(f => f.Day.Any(e => e.EnemyId == Wolf && e.Weight >= 3)), "no wolf pack of the Teto is the forest wolf any more");
        }

        [Test]
        public void TheQuestsThatAskedForTheStandIns_AskForWhatStandsThereNow()
        {
            CityQuestSpec jackals = CityQuestCatalog.Quests.Single(q => q.QuestId == CityQuestCatalog.ArattaJackals);
            Assert.AreEqual(West2.StripedJackal, jackals.Quest.Objectives.Single().TargetId, "Enmerkar's jackals: the Chacal-Listrado Apagado, the desert jackal left the Costa");
        }
    }
}
