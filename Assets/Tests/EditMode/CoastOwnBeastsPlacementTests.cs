using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rapadura.Gameplay.Night;
using Rapadura.Gameplay.Quests;
using Rapadura.Gameplay.World.Cities;
using Rapadura.Gameplay.World.Map;
using Rapadura.Gameplay.World.Map.Areas;
using Rapadura.Gameplay.World.Map.Overworld;
using Placement = Rapadura.Tests.MingchengNartesBeastsPlacementTests.Placement;
using Own = Rapadura.Gameplay.World.Map.Overworld.OverworldLayouts.CoastOwnBeasts;

namespace Rapadura.Tests
{
    /// <summary>
    /// The nineteenth pass of beasts (<see cref="CoastOwnBeastsTests"/>) now stands where its stand-ins stood in the Costa das Monções and the
    /// Delta de Jhilpur: each beast is in the day table of the groups of its sheet, its night form in the same group's night table where it leads,
    /// the stand-in left those groups' day tables, and every case is checked on the clearings the group really got (OverworldLayouts.GenerateClearings,
    /// after the trails took theirs), at that area's levels. Same idea as <see cref="LentRoleBeastsPlacementTests"/>. Every new beast leads at least
    /// one drawn clearing of its region, the stand-ins left the areas of the sheets by day, and the spider left the whole Delta.
    /// </summary>
    public class CoastOwnBeastsPlacementTests
    {
        private const string Golem = PeriodSpawnTable.MountainGolem;
        private const string Scorpion = PeriodSpawnTable.DesertScorpion;
        private const string Jackal = "enemy_desert_jackal";
        private const string Ooze = "enemy_cave_ooze";
        private const string LanternFish = "enemy_cave_lantern_fish";
        private const string Crow = "enemy_cave_dark_crow";
        private const string Spider = PeriodSpawnTable.ForestSpider;
        private const string Gull = "enemy_forest_thief_gull";
        private const string Bats = PeriodSpawnTable.CaveBats;
        private const string Mist = "enemy_mountain_ill_wind_mist";

        private const string Costa = OverworldLayouts.CostaId;
        private const string Delta = OverworldLayouts.DeltaId;

        private const bool Leads = true;
        private const bool Joins = false;

        private static Placement C(string enemy, string area, string group, bool withNightForm, params string[] standIns) =>
            new Placement(Costa, enemy, area, group, withNightForm, standIns);

        private static Placement D(string enemy, string area, string group, bool withNightForm, params string[] standIns) =>
            new Placement(Delta, enemy, area, group, withNightForm, standIns);

        private static readonly Placement[] Placements =
        {
            // Pirâmides Altas de Meroé (27).
            C(Own.KushiteUshabti, "piramides_altas_de_meroe", "ushabtis_kushitas", Leads, Golem),
            C(Own.YoungScorpionKing, "piramides_altas_de_meroe", "escorpioes_rei", Leads, Scorpion),
            C(Own.BlackSandJackal, "piramides_altas_de_meroe", "chacais_de_areia_negra", Leads, Jackal),
            C(Own.BlackSandJackal, "piramides_altas_de_meroe", "escorpioes_rei", Joins, Jackal),

            // Atol de Kirta, Jardins de Alga and the coral islets (28).
            C(Own.MudJellyfish, "atol_de_kirta", "aguas_vivas_de_lodo", Leads, Ooze),
            C(Own.MudJellyfish, "jardins_de_alga", "aguas_vivas_da_alga", Leads, Ooze),
            C(Own.MudJellyfish, "ilhotas_de_coral", "caranguejos_das_ilhotas", Joins, Ooze),
            C(Own.DimmedLionfish, "atol_de_kirta", "peixes_leao", Leads, LanternFish),
            C(Own.DimmedLionfish, "atol_de_kirta", "caranguejos_aranha", Joins, LanternFish),
            C(Own.DimmedLionfish, "jardins_de_alga", "aguas_vivas_da_alga", Joins, LanternFish),
            C(Own.DimmedLionfish, "jardins_de_alga", "tubaroes_de_lodo", Joins, LanternFish),

            // Kilwa Afundada (29).
            C(Own.ThiefOctopus, "kilwa_afundada", "polvos_ladroes", Leads, Ooze),
            C(Own.ThiefOctopus, "kilwa_afundada", "ushabtis_de_kilwa", Joins, Ooze),

            // Canais e Mangues de Liyongo (30).
            C(Own.Kongamato, "canais_de_liyongo", "kongamatos", Leads, Crow),
            C(Own.Kongamato, "mangues_de_liyongo", "kongamatos_de_liyongo", Leads, Crow),
            C(Own.Kongamato, "canais_de_liyongo", "garcas_apagadas", Joins, Crow),

            // Figueiral, Pontes de Raiz Viva, Gargantas, Cascatas and Floresta de Chuva (32).
            D(Own.SnareRoot, "figueiral_das_raizes", "raizes_laco", Leads, Spider),
            D(Own.SnareRoot, "pontes_de_raiz_viva", "raizes_laco_das_pontes", Leads, Spider),
            D(Own.SnareRoot, "gargantas_esquecidas", "sanguessugas_da_garganta", Joins, Spider),
            D(Own.SnareRoot, "cascatas_da_chuva", "pitons_das_cascatas", Joins, Spider),
            D(Own.SnareRoot, "floresta_de_chuva", "sanguessugas_da_chuva", Joins, Spider),

            // Ilhas de Junco and Palafitas de Tarchon (33).
            D(Own.GhostHeron, "ilhas_de_junco", "garcas_fantasma", Leads, Gull),
            D(Own.GhostHeron, "palafitas_de_tarchon", "garcas_fantasma_de_tarchon", Leads, Gull),

            // Charco do Ninki Nanka, Ilhota das Lanternas and Toca da Dragoa (34).
            D(Own.SmokeDragonfly, "charco_do_ninki_nanka", "libelulas_de_fumaca", Leads, Bats),
            D(Own.SmokeDragonfly, "ilhota_das_lanternas", "libelulas_das_lanternas", Leads, Bats),
            D(Own.SmokeDragonfly, "toca_da_dragoa", "sapos_da_toca", Joins, Bats),

            // Raízes Afogadas, Coração and Olho do Lodo (35), and the Bancos de Lama where the Filhos begin.
            D(Own.SmokeBubble, "raizes_afogadas", "bolhas_de_fumaca", Leads, Mist),
            D(Own.SmokeBubble, "olho_do_lodo", "bolhas_do_olho", Leads, Mist),
            D(Own.SmokeBubble, "coracao_do_lodo", "filhos_do_lodo", Joins, Mist),
            D(Own.MudChild, "coracao_do_lodo", "filhos_do_lodo", Leads, Ooze),
            D(Own.MudChild, "olho_do_lodo", "filhos_do_olho", Leads, Ooze),
            D(Own.MudChild, "bancos_de_lama", "filhos_do_lodo_dos_bancos", Leads, Ooze),
            D(Own.MudChild, "olho_do_lodo", "bolhas_do_olho", Joins, Ooze),
            D(Own.MudChild, "raizes_afogadas", "bolhas_de_fumaca", Joins, Ooze),
            D(Own.MudChild, "raizes_afogadas", "gaviais_sombra", Joins, Ooze),
        };

        private static IEnumerable<Placement> All() => Placements;

        /// <summary>The twelve beasts of the pass and their region (every one must lead a drawn clearing there).</summary>
        private static IEnumerable<TestCaseData> Beasts() => new[]
        {
            new TestCaseData(Costa, Own.KushiteUshabti), new TestCaseData(Costa, Own.YoungScorpionKing), new TestCaseData(Costa, Own.BlackSandJackal),
            new TestCaseData(Costa, Own.MudJellyfish), new TestCaseData(Costa, Own.DimmedLionfish), new TestCaseData(Costa, Own.Kongamato),
            new TestCaseData(Costa, Own.ThiefOctopus), new TestCaseData(Delta, Own.SnareRoot), new TestCaseData(Delta, Own.GhostHeron),
            new TestCaseData(Delta, Own.SmokeDragonfly), new TestCaseData(Delta, Own.SmokeBubble), new TestCaseData(Delta, Own.MudChild),
        };

        /// <summary>The areas of the sheets the stand-ins left by day (the same beasts still play other, unrelated groups elsewhere).</summary>
        private static IEnumerable<TestCaseData> LeftAreas() => new[]
        {
            new TestCaseData(Costa, "piramides_altas_de_meroe", new[] { Golem, Scorpion, Jackal }),
            new TestCaseData(Costa, "atol_de_kirta", new[] { Ooze, LanternFish }),
            new TestCaseData(Costa, "jardins_de_alga", new[] { Ooze, LanternFish }),
            new TestCaseData(Costa, "kilwa_afundada", new[] { Ooze }),
            new TestCaseData(Costa, "canais_de_liyongo", new[] { Crow }),
            new TestCaseData(Costa, "mangues_de_liyongo", new[] { Crow }),
            new TestCaseData(Delta, "ilhas_de_junco", new[] { Gull }),
            new TestCaseData(Delta, "palafitas_de_tarchon", new[] { Gull }),
            new TestCaseData(Delta, "charco_do_ninki_nanka", new[] { Bats }),
            new TestCaseData(Delta, "ilhota_das_lanternas", new[] { Bats }),
            new TestCaseData(Delta, "toca_da_dragoa", new[] { Bats }),
            new TestCaseData(Delta, "raizes_afogadas", new[] { Ooze, Mist }),
            new TestCaseData(Delta, "coracao_do_lodo", new[] { Ooze, Mist }),
            new TestCaseData(Delta, "olho_do_lodo", new[] { Ooze, Mist }),
            new TestCaseData(Delta, "bancos_de_lama", new[] { Ooze }),
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

        [TestCaseSource(nameof(LeftAreas))]
        public void TheStandIns_NoLongerSpawnByDay_InTheAreasOfTheSheets(string region, string area, string[] standIns)
        {
            OverworldLayout w = OverworldLayouts.Get(region);
            List<MapSpawnZone> clearings = w.World.Zones.Where(s => w.ZoneOfSpawn.TryGetValue(s.Id, out OverworldZone zone) && zone.Id == area).ToList();
            Assert.IsNotEmpty(clearings, $"{area}: draws clearings");
            foreach (string standIn in standIns)
            {
                Assert.IsFalse(clearings.SelectMany(s => s.Day).Any(e => e.EnemyId == standIn), $"{area}: {standIn} has its own beast in these roles now");
            }
        }

        [Test]
        public void TheSpider_NoLongerSpawnsByDay_AnywhereInTheDelta()
        {
            OverworldLayout w = OverworldLayouts.Get(Delta);
            Assert.IsFalse(w.World.Zones.SelectMany(s => s.Day).Any(e => e.EnemyId == Spider), "the Raiz-Laço took every role the spider had in the Delta");
            OverworldFauna pythons = w.Zones.SelectMany(z => z.Fauna).First(f => f.Id == "pitons_apagadas");
            Assert.IsTrue(pythons.Day.Any(e => e.EnemyId == Own.SnareRoot), "pitons_apagadas: the snare root with the pythons too, even where the trail took the clearing");
        }

        [TestCase("garcas_fantasma")]
        [TestCase("garcas_fantasma_de_tarchon")]
        public void TheGhostHerons_KeepTheShroudOwl_AtNight(string group)
        {
            OverworldFauna fauna = OverworldLayouts.Get(Delta).Zones.SelectMany(z => z.Fauna).First(f => f.Id == group);
            Assert.IsTrue(fauna.Night.Any(e => e.SpeciesId == NightVariantRules.RasgaMortalha.Id), $"{group}: the shroud owl still flies with them at night");
        }

        [TestCase("kongamatos")]
        [TestCase("kongamatos_de_liyongo")]
        public void TheKongamatos_KeepTheVampireBat_AtNight(string group)
        {
            OverworldFauna fauna = OverworldLayouts.Get(Costa).Zones.SelectMany(z => z.Fauna).First(f => f.Id == group);
            Assert.IsTrue(fauna.Night.Any(e => e.SpeciesId == NightVariantRules.MorcegoVampiro.Id), $"{group}: the vampire bat still doubles them at night");
        }

        [TestCase(CityQuestCatalog.KirtaOoze, "enemy_cave_mud_jellyfish")]
        [TestCase(CityQuestCatalog.KirtaRescue, "enemy_cave_dimmed_lionfish")]
        [TestCase(CityQuestCatalog.TarchonNets, "enemy_forest_ghost_heron")]
        public void TheCityQuests_OfTheSheets_HuntTheOwnBeast(string questId, string enemy)
        {
            CityQuestSpec spec = CityQuestCatalog.Quests.Single(q => q.QuestId == questId);
            CollectionAssert.Contains(spec.Quest.Objectives.Where(o => o.Type == QuestObjectiveType.KillEnemyType).Select(o => o.TargetId).ToList(), enemy, questId);
        }
    }
}
