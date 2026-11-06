using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rapadura.Gameplay.Night;
using Rapadura.Gameplay.World.Map;
using Rapadura.Gameplay.World.Map.Areas;
using Rapadura.Gameplay.World.Map.Overworld;
using Placement = Rapadura.Tests.MingchengNartesBeastsPlacementTests.Placement;

namespace Rapadura.Tests
{
    /// <summary>
    /// The fifth pass of beasts (<see cref="MarGeladoBeastsTests"/>: Mar Gelado) now stand where their stand-ins stood
    /// (Docs/Mapas100.md 59–64), same idea as <see cref="FarnkeshBeastsPlacementTests"/>: each one is in the day table of
    /// every group its sheet gives it (leading the group or as its companion), its night form in the same group's night table
    /// where the beast leads it, the stand-in left those groups' day tables, and every case is checked on the clearings the
    /// group really got (each group listed here draws at least one clearing in its area), at that area's levels. The leopard
    /// seal is also the narwhals, the ice gull also the storm gulls and the giant pike also the ice fish of the canals.
    /// </summary>
    public class MarGeladoBeastsPlacementTests
    {
        private const string LeopardSeal = "enemy_mountain_leopard_seal";
        private const string Walrus = "enemy_mountain_smoke_walrus";
        private const string IceGull = "enemy_mountain_ice_gull";
        private const string ArcticFox = "enemy_mountain_arctic_fox";
        private const string PolarBear = "enemy_mountain_polar_bear";
        private const string Reindeer = "enemy_mountain_clay_reindeer";
        private const string StoneHare = "enemy_mountain_stone_hare";
        private const string TwigBird = "enemy_mountain_twig_bird";
        private const string Mammoth = "enemy_mountain_burrow_mammoth";
        private const string SeaWolf = "enemy_mountain_sea_wolf";
        private const string Pike = "enemy_mountain_giant_pike";
        private const string ShadowSeal = "enemy_mountain_shadow_seal";

        private const string Otter = "enemy_forest_thief_otter";
        private const string Buffalo = "enemy_forest_mud_buffalo";
        private const string ThiefGull = "enemy_forest_thief_gull";
        private const string LanternFox = "enemy_forest_lantern_fox";
        private const string LanternFish = "enemy_cave_lantern_fish";
        private const string BrownBear = "enemy_mountain_brown_bear";
        private const string Potoo = NightVariantRules.PotooEnemyId;
        private const string RollingStone = "enemy_mountain_rolling_stone";
        private const string Yak = "enemy_mountain_wild_yak";
        private const string Rhino = "enemy_forest_dust_rhino";
        private const string Wolf = "enemy_forest_wolf";
        private const string Hawk = "enemy_mountain_crested_hawk";
        private const string Turtle = "enemy_forest_stone_turtle";

        private const string Gelo = OverworldLayouts.MarGeladoRegionId;
        private const bool Leads = true;
        private const bool Joins = false;

        private static Placement P(string enemy, string area, string group, bool withNightForm, params string[] standIns) =>
            new Placement(Gelo, enemy, area, group, withNightForm, standIns);

        private static readonly Placement[] Placements =
        {
            P(LeopardSeal, "baia_das_focas", "focas_leopardo", Leads, Otter),
            P(LeopardSeal, "baia_gelada", "focas_da_baia", Leads, Otter),
            P(LeopardSeal, "banquisas", "narvais_apagados", Leads, Otter),
            P(LeopardSeal, "ilha_do_farol", "narvais_do_farol", Leads, Otter),
            P(LeopardSeal, "baia_das_focas", "morsas_de_fumaca", Joins, Otter),
            P(LeopardSeal, "banquisas", "lobos_do_mar", Joins, Otter),
            P(LeopardSeal, "mar_das_auroras", "gaivotas_tempestade", Joins, Otter),

            P(Walrus, "baia_das_focas", "morsas_de_fumaca", Leads, Buffalo),
            P(Walrus, "baia_gelada", "morsas_da_baia", Leads, Buffalo),
            P(Walrus, "colinas_de_neve", "morsas_das_colinas", Leads, Buffalo),
            P(Walrus, "colinas_de_neve", "raposas_das_colinas", Joins, Buffalo),

            P(IceGull, "porto_da_lua_branca", "gaivotas_do_gelo", Leads, ThiefGull),
            P(IceGull, "passo_da_muralha", "gaivotas_da_muralha", Leads, ThiefGull),
            P(IceGull, "mar_das_auroras", "gaivotas_tempestade", Leads, Hawk),
            P(IceGull, "ilha_do_farol", "gaivotas_do_farol", Leads, Hawk),
            P(IceGull, "porto_da_lua_branca", "raposas_do_artico", Joins, ThiefGull),
            P(IceGull, "baia_das_focas", "focas_leopardo", Joins, ThiefGull),
            P(IceGull, "baia_gelada", "morsas_da_baia", Joins, ThiefGull),
            P(IceGull, "passo_da_muralha", "raposas_da_muralha", Joins, ThiefGull),
            P(IceGull, "mar_das_auroras", "gelos_das_auroras", Joins, Hawk),
            P(IceGull, "ilha_do_farol", "narvais_do_farol", Joins, Hawk),
            P(IceGull, "fiorde_negro", "irbis_do_fiorde_negro", Joins, Hawk),

            P(ArcticFox, "porto_da_lua_branca", "raposas_do_artico", Leads, LanternFox),
            P(ArcticFox, "passo_da_muralha", "raposas_da_muralha", Leads, LanternFox),
            P(ArcticFox, "colinas_de_neve", "raposas_das_colinas", Leads, LanternFox),
            P(ArcticFox, "porto_da_lua_branca", "gaivotas_do_gelo", Joins, LanternFox),
            P(ArcticFox, "baia_gelada", "focas_da_baia", Joins, LanternFox),
            P(ArcticFox, "passo_da_muralha", "gaivotas_da_muralha", Joins, LanternFox),
            P(ArcticFox, "colinas_de_neve", "morsas_das_colinas", Joins, LanternFox),
            P(ArcticFox, "pastos_de_rena", "lobos_dos_pastos", Joins, LanternFox),
            P(ArcticFox, "pastos_de_rena", "renas_dos_pastos", Joins, LanternFox),

            P(PolarBear, "canais_do_oeste", "ursos_polares", Leads, BrownBear),
            P(PolarBear, "gelo_rachado", "ursos_do_gelo_rachado", Leads, BrownBear),
            P(PolarBear, "fiorde_negro", "ursos_do_fiorde_negro", Leads, BrownBear),
            P(PolarBear, "fiorde_do_kul", "ursos_do_kul", Leads, BrownBear),
            P(PolarBear, "gelo_rachado", "rachaduras", Joins, BrownBear),

            P(Reindeer, "pastos_de_rena", "renas_dos_pastos", Leads, Yak),
            P(Reindeer, "tundra_do_sul", "renas_de_barro", Leads, Yak),
            P(Reindeer, "tundra_de_omol", "lebres_pedra", Joins, Yak),
            P(Reindeer, "lagos_de_omol", "aves_graveto_dos_lagos", Joins, Yak),

            P(StoneHare, "tundra_de_omol", "lebres_pedra", Leads, RollingStone),
            P(StoneHare, "tundra_de_omol", "aves_graveto", Joins, RollingStone),
            P(StoneHare, "tundra_do_sul", "renas_de_barro", Joins, RollingStone),
            P(StoneHare, "lagos_de_omol", "mosquitos_dos_lagos", Joins, RollingStone),

            P(TwigBird, "tundra_de_omol", "aves_graveto", Leads, Potoo),
            P(TwigBird, "lagos_de_omol", "aves_graveto_dos_lagos", Leads, Potoo),
            P(TwigBird, "tundra_do_sul", "mosquitos_da_tundra", Joins, Potoo),

            P(Mammoth, "galerias_dos_mamutes", "mamutes_de_baixo_da_terra", Leads, Rhino),
            P(Mammoth, "fiorde_das_presas", "mamutes_do_fiorde", Leads, Rhino),

            P(SeaWolf, "pastos_de_rena", "lobos_dos_pastos", Leads, Wolf),
            P(SeaWolf, "banquisas", "lobos_do_mar", Leads, Wolf),
            P(SeaWolf, "banquisas_do_oeste", "lobos_das_banquisas", Leads, Wolf),
            P(SeaWolf, "banquisas_do_oeste", "gelos_flutuantes", Joins, Wolf),
            P(SeaWolf, "ilha_do_farol", "gaivotas_do_farol", Joins, Wolf),

            P(Pike, "canais_de_gelo", "peixes_gelo", Leads, LanternFish),
            P(Pike, "fundo_do_velho_kul", "lucios_gigantes", Leads, LanternFish),
            P(Pike, "buracos_de_pesca", "lucios_dos_buracos", Leads, LanternFish),
            P(Pike, "canais_de_gelo", "enguias_lanterna", Joins, LanternFish),
            P(Pike, "canais_do_oeste", "ursos_polares", Joins, LanternFish),
            P(Pike, "canais_do_oeste", "peixes_gelo_do_oeste", Joins, LanternFish),
            P(Pike, "fundo_do_velho_kul", "kuls_pequenos", Joins, LanternFish),
            P(Pike, "buracos_de_pesca", "peixes_bruxa", Joins, LanternFish),

            P(ShadowSeal, "lago_do_kul", "focas_sombra", Leads, Turtle),
            P(ShadowSeal, "gelo_negro", "focas_do_gelo_negro", Leads, Turtle),
            P(ShadowSeal, "lago_do_kul", "kuls_do_lago", Joins, Turtle),
            P(ShadowSeal, "gelo_negro", "peixes_bruxa_do_gelo_negro", Joins, Turtle),
            P(ShadowSeal, "fiorde_do_kul", "irbis_do_kul", Joins, Turtle),
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

        [Test]
        public void TheTwigBirds_KeepTheMoonMother_AtNight()
        {
            OverworldLayout w = OverworldLayouts.Get(Gelo);
            foreach (string group in new[] { "aves_graveto", "aves_graveto_dos_lagos" })
            {
                OverworldFauna fauna = w.Zones.SelectMany(z => z.Fauna).First(f => f.Id == group);
                Assert.IsTrue(fauna.Night.Any(e => e.SpeciesId == NightVariantRules.MaeDaLua.Id), $"{group}: the moon-mother at night");
            }
        }

        [Test]
        public void NoStandIn_SpawnsByDay_AnywhereOnTheFrozenSea()
        {
            OverworldLayout w = OverworldLayouts.Get(Gelo);
            List<string> day = w.World.Zones.SelectMany(s => s.Day).Select(e => e.EnemyId).ToList();
            foreach (string standIn in new[] { Otter, Buffalo, ThiefGull, LanternFox, LanternFish, BrownBear, Potoo, RollingStone, Yak, Rhino, Wolf, Hawk, Turtle })
            {
                Assert.IsFalse(day.Contains(standIn), $"no {standIn} by day on the frozen sea any more");
            }
        }
    }
}
