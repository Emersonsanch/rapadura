using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rapadura.Gameplay.Night;
using Rapadura.Gameplay.World.Map;
using Rapadura.Gameplay.World.Map.Areas;
using Rapadura.Gameplay.World.Map.Overworld;
using Placement = Rapadura.Tests.MingchengNartesBeastsPlacementTests.Placement;
using Lent = Rapadura.Gameplay.World.Map.Overworld.OverworldLayouts.LentBeasts;

namespace Rapadura.Tests
{
    /// <summary>
    /// The seventeenth pass of beasts (<see cref="LentRoleBeastsTests"/>) now stands where its stand-ins stood in the Estepe dos Nartes: each beast
    /// is in the day table of the groups of its sheet, its night form in the same group's night table where it leads, the stand-in left those
    /// groups' day tables, and every case is checked on the clearings the group really got (OverworldLayouts.GenerateClearings, after the trails
    /// took theirs), at that area's levels. Same idea as <see cref="BorrowedRoleBeastsPlacementTests"/>. Every new beast draws at least one
    /// clearing, and the hawk, the jackal, the crow, the ooze and the Pamir's bear no longer spawn by day anywhere in the steppe.
    /// </summary>
    public class LentRoleBeastsPlacementTests
    {
        private const string Hawk = "enemy_mountain_crested_hawk";
        private const string Jackal = "enemy_desert_jackal";
        private const string Peccary = NightVariantRules.PeccaryEnemyId;
        private const string Crow = "enemy_cave_dark_crow";
        private const string Ooze = "enemy_cave_ooze";
        private const string Wolf = "enemy_forest_wolf";
        private const string BrownBear = "enemy_mountain_brown_bear";
        private const string Werewolf = NightVariantRules.WerewolfEnemyId;

        private const string Nart = OverworldLayouts.NartesId;

        private const bool Leads = true;
        private const bool Joins = false;

        private static Placement P(string enemy, string area, string group, bool withNightForm, params string[] standIns) =>
            new Placement(Nart, enemy, area, group, withNightForm, standIns);

        private static readonly Placement[] Placements =
        {
            P(Lent.DimmedGoldenEagle, "acampamento_de_soslan", "aguias_douradas", Leads, Hawk),
            P(Lent.DimmedGoldenEagle, "estepe_dos_ventos", "aguias_dos_ventos", Leads, Hawk),
            P(Lent.DimmedGoldenEagle, "acampamento_de_soslan", "saigas_assustadas", Joins, Hawk),
            P(Lent.DimmedGoldenEagle, "estepe_de_soslan", "lobos_de_soslan", Joins, Hawk),
            P(Lent.DimmedGoldenEagle, "pistas_de_corrida", "grifos_jovens", Joins, Hawk),

            P(Lent.GoldenStag, "kurgans_de_ouro", "cervos_de_ouro", Leads, Jackal),

            P(Lent.GiantIbex, "pastos_dos_gigantes", "cabras_gigantes", Leads, Jackal),

            P(Lent.HillBoar, "pastos_dos_gigantes", "javalis_da_colina", Joins, Peccary),
            P(Lent.HillBoar, "fortim_dos_gigantes", "javalis_do_fortim", Joins, Peccary),
            P(Lent.HillBoar, "pastos_dos_gigantes", "cabras_gigantes", Joins, Peccary),

            P(Lent.SmokeMessenger, "fortim_dos_gigantes", "corvos_mensageiros", Leads, Crow),
            P(Lent.SmokeMessenger, "vale_das_torres_svan", "corvos_das_torres", Leads, Crow),
            P(Lent.SmokeMessenger, "fortim_dos_gigantes", "javalis_do_fortim", Leads, Crow),
            P(Lent.SmokeMessenger, "fortim_dos_gigantes", "gigantes_sonambulos", Joins, Crow),
            P(Lent.SmokeMessenger, "vale_das_tabuas_de_sonho", "pesadelos_de_fumaca", Joins, Crow),
            P(Lent.SmokeMessenger, "vale_das_torres_svan", "ochokochis_do_vale", Joins, Crow),
            P(Lent.SmokeMessenger, "torre_de_vigia", "ursos_do_caucaso", Joins, Crow),

            P(Lent.SmokeWoodpecker, "arvoredo_de_haripushpa", "pica_paus_de_fumaca", Leads, Hawk),

            P(Lent.DimmedFungus, "arvoredo_de_haripushpa", "fungos_apagados", Leads, Ooze),
            P(Lent.DimmedFungus, "arvoredo_de_haripushpa", "pica_paus_de_fumaca", Joins, Ooze),
            P(Lent.DimmedFungus, "taiga_de_laricos", "besouros_casca", Joins, Ooze),
            P(Lent.DimmedFungus, "lago_de_degelo", "linces_do_lago", Joins, Ooze),

            P(Lent.TaigaLynx, "taiga_de_laricos", "linces_da_taiga", Leads, Wolf),
            P(Lent.TaigaLynx, "lago_de_degelo", "linces_do_lago", Leads, Wolf),

            P(Lent.DimmedStallion, "pradaria_dos_mil_cavalos", "garanhoes_apagados", Leads, Wolf),
            P(Lent.DimmedStallion, "pradaria_dos_mil_cavalos", "tempestades_da_pradaria", Joins, Wolf),

            P(Lent.IronEagle, "torre_de_vigia", "aguias_de_ferro", Leads, Hawk),
            P(Lent.IronEagle, "cume_de_ohya", "aguias_do_cume", Leads, Hawk),
            P(Lent.IronEagle, "corrente_de_amirani", "pedras_da_corrente", Joins, Hawk),

            P(Lent.CaucasusBear, "torre_de_vigia", "ursos_do_caucaso", Leads, BrownBear, Werewolf),
            P(Lent.CaucasusBear, "cume_de_ohya", "lobos_de_neve", Joins, BrownBear, Werewolf),

            P(Lent.SnowWolf, "cume_de_ohya", "lobos_de_neve", Leads, Wolf),
        };

        private static IEnumerable<Placement> All() => Placements;

        /// <summary>The twelve beasts of the pass (every one must draw a clearing somewhere in the steppe).</summary>
        private static IEnumerable<string> Beasts() => new[]
        {
            Lent.DimmedGoldenEagle, Lent.GoldenStag, Lent.GiantIbex, Lent.HillBoar, Lent.SmokeMessenger, Lent.SmokeWoodpecker,
            Lent.DimmedFungus, Lent.TaigaLynx, Lent.DimmedStallion, Lent.IronEagle, Lent.CaucasusBear, Lent.SnowWolf,
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
        public void EachBeast_LeadsAtLeastOneDrawnClearing_InTheSteppe([ValueSource(nameof(Beasts))] string enemy)
        {
            OverworldLayout w = OverworldLayouts.Get(Nart);
            List<MapSpawnZone> clearings = w.World.Zones.Where(s => s.Day.Any(e => e.EnemyId == enemy)).ToList();
            Assert.IsNotEmpty(clearings, $"{enemy}: no clearing of the steppe spawns it (the trails or the draw took them all)");
            Assert.IsTrue(clearings.Any(s => s.Day.Where(e => e.EnemyId == enemy).Sum(e => e.Weight) >= 3), $"{enemy}: leads at least one clearing (weight 3)");
        }

        [Test]
        public void TheStandIns_NoLongerSpawnByDay_AnywhereInTheSteppe()
        {
            OverworldLayout w = OverworldLayouts.Get(Nart);
            foreach (string standIn in new[] { Hawk, Jackal, Crow, Ooze, BrownBear })
            {
                Assert.IsFalse(w.World.Zones.SelectMany(s => s.Day).Any(e => e.EnemyId == standIn), $"{standIn}: its roles in the steppe have their own beasts now");
            }

            Assert.IsFalse(w.World.Zones.SelectMany(s => s.Day).Any(e => e.EnemyId == Peccary), "the hill boar took the peccary's day roles (the queixada herd stays at night)");
        }

        [Test]
        public void TheGreyWolves_KeepTheWolf_ForALaterPass()
        {
            OverworldFauna fauna = OverworldLayouts.Get(Nart).Zones.SelectMany(z => z.Fauna).FirstOrDefault(f => f.Id == "lobos_cinzentos");
            Assert.IsNotNull(fauna, "lobos_cinzentos");
            Assert.IsTrue(fauna.Day.Any(e => e.EnemyId == Wolf), "the Lobo-Cinzento (51) is still the wolf by day");
        }

        [TestCase("javalis_da_colina")]
        [TestCase("javalis_do_fortim")]
        public void TheHillBoars_KeepTheQueixadaHerd_AtNight(string group)
        {
            OverworldFauna fauna = OverworldLayouts.Get(Nart).Zones.SelectMany(z => z.Fauna).First(f => f.Id == group);
            Assert.IsTrue(fauna.Night.Any(e => e.SpeciesId == NightVariantRules.Queixada.Id), $"{group}: the herd still comes down at night");
        }
    }
}
