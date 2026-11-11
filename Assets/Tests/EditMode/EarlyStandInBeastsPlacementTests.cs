using System.Collections.Generic;
using NUnit.Framework;
using Rapadura.Gameplay.Night;
using Rapadura.Gameplay.World.Map;
using Rapadura.Gameplay.World.Map.Areas;
using Rapadura.Gameplay.World.Map.Overworld;
using Placement = Rapadura.Tests.MingchengNartesBeastsPlacementTests.Placement;
using Early = Rapadura.Gameplay.World.Map.Overworld.OverworldLayouts.EarlyBeasts;

namespace Rapadura.Tests
{
    /// <summary>
    /// The twelfth pass of beasts (<see cref="EarlyStandInBeastsTests"/>) now stands where its stand-ins stood in Regions 1–4: each beast is in the day
    /// table of the groups of its sheet, its night form in the same group's night table where it leads, the stand-in left those groups' day tables,
    /// and every case is checked on the clearings the group really got, at that area's levels. Same idea as <see cref="LateStandInBeastsPlacementTests"/>.
    /// </summary>
    public class EarlyStandInBeastsPlacementTests
    {
        private const string Potoo = NightVariantRules.PotooEnemyId;
        private const string Wolf = PeriodSpawnTable.ForestWolf;
        private const string Spider = PeriodSpawnTable.ForestSpider;
        private const string Bats = PeriodSpawnTable.CaveBats;
        private const string Jackal = "enemy_desert_jackal";

        private const bool Leads = true;
        private const bool Joins = false;

        private static Placement P(string region, string enemy, string area, string group, bool withNightForm, params string[] standIns) =>
            new Placement(region, enemy, area, group, withNightForm, standIns);

        private static readonly Placement[] Placements =
        {
            P(OverworldLayouts.ZarafshanId, Early.Stork, "prado_das_cegonhas", "cegonhas", Leads, Potoo),
            P(OverworldLayouts.ZarafshanId, Early.Stork, "varzea_dourada", "aves_da_varzea", Leads, Potoo),
            P(OverworldLayouts.ZarafshanId, Early.BubbleToad, "brejo", "lodo_escuro", Joins, Bats),
            P(OverworldLayouts.ZarafshanId, Early.SmokeMosquito, "brejo", "fumaca", Leads, Bats),
            P(OverworldLayouts.ZarafshanId, Early.SilkMoth, "amoreiras", "gralhas", Leads, Potoo),
            P(OverworldLayouts.ZarafshanId, Early.LaurelPigeon, "garajonay", "nevoa", Leads, Potoo),
            P(OverworldLayouts.ZarafshanId, Early.LaurelLizard, "garajonay", "louros", Leads, Wolf),
            P(OverworldLayouts.ZarafshanId, Early.SmokePeacock, "jardim_mihrshah", "lobos_de_cinzas", Joins, Potoo),

            P(OverworldLayouts.AreiasId, Early.ThornLizard, "dunas_que_cantam", "lagartos_da_duna", Leads, Jackal),

            P(OverworldLayouts.KaggenId, Early.DimmedLynx, "passos_estreitos", "lince", Leads, Wolf),
            P(OverworldLayouts.KaggenId, Early.Smudge, "abrigos_pintados", "borroes", Leads, Wolf),
            P(OverworldLayouts.KaggenId, Early.StoneJackdaw, "muralhas_pedra_seca", "gralhas_de_pedra", Leads, Potoo),

            P(OverworldLayouts.LimiarId, Early.YoungUbir, "terracos_de_tufo", "degraus_rosados", Leads, Spider),
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
