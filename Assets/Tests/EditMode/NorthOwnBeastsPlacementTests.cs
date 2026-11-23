using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rapadura.Gameplay.World.Map;
using Rapadura.Gameplay.World.Map.Areas;
using Rapadura.Gameplay.World.Map.Overworld;
using Placement = Rapadura.Tests.MingchengNartesBeastsPlacementTests.Placement;
using North = Rapadura.Gameplay.World.Map.Overworld.OverworldLayouts.NorthBeasts;

namespace Rapadura.Tests
{
    /// <summary>
    /// The twenty-first pass of beasts (<see cref="NorthOwnBeastsTests"/>) now stands where its stand-ins stood in the Picos de Farnkesh (57–58),
    /// the Mar Gelado (60–64) and the Arquipélago de Hinokura (68–70): each beast is in the day table of the groups of its sheet, its night form
    /// in the same group's night table where it leads, the stand-in left those groups' day tables, and every case is checked on the clearings
    /// the group really got (OverworldLayouts.GenerateClearings, after the trails took theirs), at that area's levels. Same idea as
    /// <see cref="HighlandOwnBeastsPlacementTests"/>. Every new beast leads at least one drawn clearing; the rolling stone left Farnkesh and the
    /// ooze and the firefly swarm left Hinokura by day, while the ill-wind mist, the broken lantern, the giant pike, the leopard seal, the ice
    /// gull and the ooze keep the roles of the other maps of their regions.
    /// </summary>
    public class NorthOwnBeastsPlacementTests
    {
        private const string RollingStone = "enemy_mountain_rolling_stone";
        private const string Mist = "enemy_mountain_ill_wind_mist";
        private const string Lantern = "enemy_desert_broken_lantern";
        private const string Pike = "enemy_mountain_giant_pike";
        private const string SandSnake = "enemy_desert_sand_snake";
        private const string Firefly = "enemy_desert_firefly_swarm";
        private const string Seal = "enemy_mountain_leopard_seal";
        private const string IceGull = "enemy_mountain_ice_gull";
        private const string Ooze = "enemy_cave_ooze";

        private const string Fark = OverworldLayouts.FarnkeshRegionId;
        private const string Gelo = OverworldLayouts.MarGeladoRegionId;
        private const string Fogo = OverworldLayouts.HinokuraRegionId;

        private const bool Leads = true;
        private const bool Joins = false;

        private static Placement F(string enemy, string area, string group, bool withNightForm, params string[] standIns) =>
            new Placement(Fark, enemy, area, group, withNightForm, standIns);

        private static Placement G(string enemy, string area, string group, bool withNightForm, params string[] standIns) =>
            new Placement(Gelo, enemy, area, group, withNightForm, standIns);

        private static Placement H(string enemy, string area, string group, bool withNightForm, params string[] standIns) =>
            new Placement(Fogo, enemy, area, group, withNightForm, standIns);

        private static readonly Placement[] Placements =
        {
            F(North.FlyingStone, "garganta_dos_ays", "pedras_voadoras", Leads, RollingStone),
            F(North.FlyingStone, "escarpa_dos_ventos", "pedras_da_escarpa", Leads, RollingStone),
            F(North.FlyingStone, "gruta_abrigo", "aguias_da_tempestade", Joins, RollingStone),
            F(North.FlyingStone, "gruta_abrigo", "redemoinhos_de_po", Joins, RollingStone),
            F(North.FlyingStone, "escarpa_dos_ventos", "gatos_da_escarpa", Joins, RollingStone),

            F(North.GorgeAys, "garganta_dos_ays", "ays_do_vento", Leads, Mist),
            F(North.GorgeAys, "gruta_abrigo", "redemoinhos_de_po", Leads, Mist),
            F(North.GorgeAys, "garganta_dos_ays", "pedras_voadoras", Joins, Mist),

            F(North.TrappedLightning, "ninho_da_huma", "raios_presos", Leads, Lantern),
            F(North.TrappedLightning, "ninho_da_huma", "grifos_selvagens", Joins, Lantern),

            G(North.IceFish, "canais_de_gelo", "peixes_gelo", Leads, Pike),
            // The west canals' shoal leads by day, but its night form there is the lantern eel's.
            G(North.IceFish, "canais_do_oeste", "peixes_gelo_do_oeste", Joins, Pike),
            G(North.IceFish, "canais_de_gelo", "enguias_lanterna", Joins, Pike),
            G(North.IceFish, "canais_do_oeste", "ursos_polares", Joins, Pike),

            G(North.LanternEel, "canais_de_gelo", "enguias_lanterna", Leads, SandSnake),
            G(North.LanternEel, "canais_de_gelo", "peixes_gelo", Joins, SandSnake),

            G(North.TundraMosquito, "tundra_do_sul", "mosquitos_da_tundra", Leads, Firefly),
            G(North.TundraMosquito, "lagos_de_omol", "mosquitos_dos_lagos", Leads, Firefly),

            G(North.DimmedNarwhal, "banquisas", "narvais_apagados", Leads, Seal),
            G(North.DimmedNarwhal, "ilha_do_farol", "narvais_do_farol", Leads, Seal),
            G(North.DimmedNarwhal, "banquisas", "lobos_do_mar", Joins, Seal),
            G(North.DimmedNarwhal, "mar_das_auroras", "gaivotas_tempestade", Joins, Seal),

            G(North.StormGull, "mar_das_auroras", "gaivotas_tempestade", Leads, IceGull),
            G(North.StormGull, "ilha_do_farol", "gaivotas_do_farol", Leads, IceGull),
            G(North.StormGull, "mar_das_auroras", "gelos_das_auroras", Joins, IceGull),
            G(North.StormGull, "ilha_do_farol", "narvais_do_farol", Joins, IceGull),
            G(North.StormGull, "fiorde_negro", "irbis_do_fiorde_negro", Joins, IceGull),

            G(North.WitchFish, "buracos_de_pesca", "peixes_bruxa", Leads, Ooze),
            G(North.WitchFish, "gelo_negro", "peixes_bruxa_do_gelo_negro", Leads, Ooze),
            G(North.WitchFish, "buracos_de_pesca", "lucios_dos_buracos", Joins, Ooze),
            G(North.WitchFish, "lago_do_kul", "focas_sombra", Joins, Ooze),

            H(North.ObsidianOctopus, "canais_quentes", "polvos_dos_canais", Leads, Ooze),
            H(North.ObsidianOctopus, "recife_de_obsidiana", "polvos_de_obsidiana", Leads, Ooze),
            H(North.ObsidianOctopus, "canais_quentes", "peixes_lamina", Joins, Ooze),

            H(North.RagingSpark, "patio_das_fornalhas", "faiscas_raivosas", Leads, Firefly),
            H(North.RagingSpark, "escoria_velha", "faiscas_da_escoria", Leads, Firefly),
            H(North.RagingSpark, "castelo_das_nove_fornalhas", "foles_vivos", Joins, Firefly),
            H(North.RagingSpark, "escoria_velha", "ushabtis_da_escoria", Joins, Firefly),
            H(North.RagingSpark, "cinzas_vivas", "cinzas_vivas", Joins, Firefly),
            H(North.RagingSpark, "mar_de_cinza", "salamandras_do_mar", Joins, Firefly),

            H(North.LivingAsh, "encosta_da_caldeira", "cinzas_vivas_da_encosta", Leads, Ooze),
            H(North.LivingAsh, "cinzas_vivas", "cinzas_vivas", Leads, Ooze),
            H(North.LivingAsh, "borda_fria", "gigantes_da_borda", Joins, Ooze),
            H(North.LivingAsh, "cinzas_vivas", "salamandras_das_cinzas", Joins, Ooze),
        };

        private static IEnumerable<Placement> All() => Placements;

        /// <summary>The twelve beasts of the pass and their region (every one must lead a drawn clearing there).</summary>
        private static IEnumerable<TestCaseData> Beasts() => new[]
        {
            new TestCaseData(Fark, North.FlyingStone), new TestCaseData(Fark, North.GorgeAys), new TestCaseData(Fark, North.TrappedLightning),
            new TestCaseData(Gelo, North.IceFish), new TestCaseData(Gelo, North.LanternEel), new TestCaseData(Gelo, North.TundraMosquito),
            new TestCaseData(Gelo, North.DimmedNarwhal), new TestCaseData(Gelo, North.StormGull), new TestCaseData(Gelo, North.WitchFish),
            new TestCaseData(Fogo, North.ObsidianOctopus), new TestCaseData(Fogo, North.RagingSpark), new TestCaseData(Fogo, North.LivingAsh),
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

        [TestCase(Fark, RollingStone)]
        [TestCase(Fogo, Ooze)]
        [TestCase(Fogo, Firefly)]
        public void TheStandIns_NoLongerSpawnByDay_AnywhereInTheRegion(string region, string standIn)
        {
            OverworldLayout w = OverworldLayouts.Get(region);
            Assert.IsFalse(w.World.Zones.SelectMany(s => s.Day).Any(e => e.EnemyId == standIn), $"{standIn}: its roles in {region} have their own beasts now");
        }

        [TestCase(Fark, "neblinas_do_vento_ruim", Mist)]
        // The Ventania (ventanias) and the Rachadura (rachaduras) are their own beasts since the twenty-ninth pass (NorthOwn2BeastsPlacementTests).
        // The Sino Rachado (sinos_rachados) is its own beast since the twenty-eighth pass (PeaksOwnBeastsPlacementTests).
        [TestCase(Gelo, "lucios_gigantes", Pike)]
        [TestCase(Gelo, "focas_leopardo", Seal)]
        [TestCase(Gelo, "gaivotas_do_gelo", IceGull)]
        public void TheOtherMapsOfTheRegion_KeepTheirStandIn(string region, string group, string standIn)
        {
            OverworldFauna fauna = OverworldLayouts.Get(region).Zones.SelectMany(z => z.Fauna).FirstOrDefault(f => f.Id == group);
            Assert.IsNotNull(fauna, group);
            Assert.IsTrue(fauna.Day.Any(e => e.EnemyId == standIn), $"{group}: still {standIn} by day");
        }
    }
}
