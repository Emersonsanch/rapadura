using Rapadura.Gameplay.Night;
using Rapadura.Gameplay.World.Biomes;
using Rapadura.Gameplay.World.Cities.Town;
using Rapadura.Gameplay.World.Map.Areas;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Overworld
{
    /// <summary>
    /// Região 5 — Costa das Monções as ONE continuous map of 2400 × 2400 m (Docs/Mapas100.md §0.10 and §5, maps 25–30), no
    /// scene change between Taa Pwani and the coast around it. Taa Pwani (25's coral-stone city, the region's return city)
    /// stands at the plaza (0, 0) on the beach, 600 m from the west edge where the old road from the Zarafshan comes in:
    /// <list type="bullet">
    /// <item>round the town (nv 45–51): the <b>Praia de Taa Pwani</b> and the <b>Recife de Taa Pwani</b> (25: the reef flat
    /// past the Canal do Recife, its fords and causeway; Dhow Passage), the <b>Dunas das Casuarinas</b>, the <b>Mata da
    /// Restinga</b> (the Zarafshan pass), the <b>Coqueiral</b>, the <b>Cravais do Vento Leste</b>, the <b>Praia dos Dhows</b>,
    /// the <b>Lagoas de Maré</b> and the <b>Baixios do Sul</b> (islets linked by shallows);</item>
    /// <item>the middle ring (nv 48–60): the <b>Savana dos Auroques</b> (26, with Aratta, the bull-folk's city) and the
    /// <b>Savana das Acácias</b>, the <b>Pirâmides Altas de Meroé</b> (27, Pátio dos Relevos; Artificers' Relic), the <b>Bosque
    /// dos Baobás</b>, the <b>Falésias Vermelhas</b>, the <b>Ruínas de Gedi</b>, <b>Kirta's atoll</b> (28, the sea-folk's city
    /// on the ring south of the lagoon; Breathing Shell, sold in Taa Pwani), the <b>Ilhotas de Coral</b> and the <b>Lagoa
    /// Grande</b>;</item>
    /// <item>the far east (nv 58–72): the <b>Jardins de Alga</b> (28), <b>Kilwa Afundada</b> (29, its drowned streets, the
    /// Farol de Coral; Air Bubble) and the <b>Palácio de Husuni</b>, the <b>Falésias do Norte</b>, the <b>Mangue Raso</b>,
    /// the <b>Canais de Liyongo</b> (30, the Cais das Dhows; Root Boots) and the <b>Mangues de Liyongo</b>.</item>
    /// </list>
    /// Water: the sea itself is beyond the east wall; inside, the Canal do Recife, the lagoons and the channels are carved
    /// water that blocks (dark), crossed at bridges and at fords — the baixios, marked by striped posts — while the reef
    /// flat, the shoals and the atoll ring are walkable shallows (turquoise ground). Monsters: the Gaivota Ladra and the
    /// Caranguejo-dos-Coqueiros with the desert and forest beasts that fit (jackal, sand snake, scorpion, spider, wolf,
    /// peccary, ushabti, ember vulture, hawk, lodo, crow, lantern fish) and the night species; no boss.
    /// </summary>
    public static partial class OverworldLayouts
    {
        public const string CostaId = "overworld_costa";

        public const string TaaPwaniMapId = "map_025_taa_pwani";
        public const string AuroquesId = "map_026_auroques";
        public const string MeroeId = "map_027_meroe";
        public const string KirtaMapId = "map_028_kirta";
        public const string KilwaId = "map_029_kilwa";
        public const string LiyongoId = "map_030_liyongo";

        public static readonly Vector2 TaaPwaniOffset = Vector2.zero;
        public static readonly Vector2 ArattaOffset = new Vector2(-380f, 950f);
        public static readonly Vector2 PatioRelevosOffset = new Vector2(500f, 950f);
        public static readonly Vector2 KirtaOffset = new Vector2(1000f, -60f);
        public static readonly Vector2 FarolDeCoralOffset = new Vector2(1500f, 470f);
        public static readonly Vector2 CaisDasDhowsOffset = new Vector2(1500f, -480f);

        /// <summary>Hedammu's arena (30): the Largo de Hedammu, the stone ring at the end of the mangrove trail past the Hedammu ford.</summary>
        public static readonly Vector2 HedammuArena = new Vector2(1540f, -1150f);

        /// <summary>O Polvo das Monções's arena (25, map boss): the stone ring on the south road through the Praia dos Dhows, where the beached dhows lie.</summary>
        public static readonly Vector2 PolvoDasMoncoesArena = new Vector2(6f, -488f);

        /// <summary>O Touro de Fumaça's arena (26, map boss): the stone ring on the trail through the north of the Savana dos Auroques, past Aratta.</summary>
        public static readonly Vector2 TouroDeFumacaArena = new Vector2(-311f, 1115f);

        /// <summary>O Leão dos Relevos's arena (27, map boss): the stone ring on the Falésias do Norte above Meroë, by the trail, where no Artificer relic is asked.</summary>
        public static readonly Vector2 LeaoDosRelevosArena = new Vector2(998f, 1020f);

        /// <summary>A Moreia do Coral Cinza's arena (28, map boss): the stone ring by the trail down the Lagoa Grande, west of the great lagoon (an area of Kirta that asks no shell).</summary>
        public static readonly Vector2 MoreiaDoCoralCinzaArena = new Vector2(837f, -387f);

        /// <summary>O Caranguejo-Palácio's arena (29, map boss): the stone ring by the trail through the Ruínas de Gedi, Kilwa's area that asks no air bubble.</summary>
        public static readonly Vector2 CaranguejoPalacioArena = new Vector2(542f, -895f);

        /// <summary>Half the side (m) of the flat town ground of the race cities Aratta and Kirta (their solids reach 22 m).</summary>
        public const float RaceCityGround = 28f;

        /// <summary>The Canal do Recife between Taa Pwani's beach and the reef flat, and its east-road ford.</summary>
        public static readonly Vector2[] ReefCanal =
        {
            new Vector2(300f, -700f), new Vector2(285f, -420f), new Vector2(300f, -150f), new Vector2(280f, 120f), new Vector2(300f, 380f), new Vector2(330f, 700f),
        };

        /// <summary>
        /// The thirteenth pass of beasts (bible §3.2.1, <c>MidStandInBeastRecipes</c>): the roles of Regions 5–9, the road of the middle story
        /// (<c>MidStoryCatalog</c>), that the continuous world played with stand-ins (Docs/Mapas100.md 25, 26, 30–32, 36–38, 43, 45, 48), placed in the
        /// groups of their sheets here and in the Delta, Teto, Mingcheng and Nartes layouts.
        /// </summary>
        public static class MidBeasts
        {
            public const string Moray = "enemy_forest_dimmed_moray";
            public const string SmokeUrchin = "enemy_forest_smoke_urchin";
            public const string LightningBird = "enemy_forest_lightning_bird";
            public const string FiddlerCrab = "enemy_forest_fiddler_crab";
            public const string SpearHeron = "enemy_forest_spear_heron";
            public const string MireLeech = "enemy_forest_mire_leech";
            public const string RedBilledChough = "enemy_mountain_red_billed_chough";
            public const string BeardedVulture = "enemy_mountain_bearded_vulture";
            public const string IceWolf = "enemy_mountain_ice_wolf";
            public const string DragonCarp = "enemy_forest_dragon_carp";
            public const string TerracottaHorse = "enemy_forest_terracotta_horse";
            public const string FeltSwan = "enemy_mountain_felt_swan";
        }

        /// <summary>
        /// The fifteenth pass of beasts (bible §3.2.1, <c>WideStandInBeastRecipes</c>): the most-seen roles of Regions 5–16 that the continuous world
        /// still played with stand-ins after the passes by region (Docs/Mapas100.md 29, 31, 33, 34, 41, 42, 44, 47, 51, 91, 94), placed in the
        /// groups of their sheets in the Costa, Delta, Mingcheng, Nartes and Céu layouts.
        /// </summary>
        public static class WideBeasts
        {
            public const string SteppeVulture = Nart.Griffin;
            public const string SteppeWolf = Nart.SteppeWolf;
            public const string GrassStorm = Nart.GrassStorm;
            public const string MarshWisp = Delta.Lantern;
            public const string BullFrog = Delta.BullFrog;
            public const string Mudskipper = Delta.LanternFish;
            public const string BrokenMirror = Ming.Mirror;
            public const string PaperCrane = Ming.PaperCrane;
            public const string SmokeLocust = Ming.Locusts;
            public const string CoralUshabti = Coast.CoralUshabti;
            public const string ConfusedSpark = Ceu.ConfusedSpark;
            public const string DarkDrop = Ceu.DarkDrop;
        }

        /// <summary>
        /// The sixteenth pass of beasts (bible §3.2.1, <c>BorrowedRoleBeastRecipes</c>): roles of Regions 7–16 that the continuous world still played
        /// with another region's beast (Docs/Mapas100.md 38, 62, 65, 68, 78, 79, 81, 82, 91, 92, 99), placed in the groups of their sheets in the Teto,
        /// Mar Gelado, Hinokura, Mistura and Céu layouts. Each keeps its stand-in's biome and numbers.
        /// </summary>
        public static class BorrowedBeasts
        {
            public const string LivingCrevasse = Teto.Crevasse;
            public const string IceMole = Gelo.Mole;
            public const string FlyingPumice = Fogo.Pumice;
            public const string GlassUrchin = Fogo.Urchin;
            public const string InstantLizard = Mistura.InstantLizard;
            public const string BlackCatfish = Mistura.Catfish;
            public const string Tremor = Mistura.Tremor;
            public const string ErasingWater = Mistura.ErasingWater;
            public const string StowawayShadow = Ceu.StowawayShadow;
            public const string SwimmingShadow = Ceu.SwimmingShadow;
            public const string WrongCurrent = Ceu.WrongCurrent;
            public const string SkyCrack = Ceu.SkyCrackBeast;
        }

        /// <summary>
        /// The seventeenth pass of beasts (bible §3.2.1, <c>LentRoleBeastRecipes</c>): the roles of the Estepe dos Nartes (Docs/Mapas100.md 47–52)
        /// that the continuous world still played with another region's beast, placed in the groups of their sheets in the Nartes layout. Each
        /// keeps its stand-in's biome and numbers.
        /// </summary>
        public static class LentBeasts
        {
            public const string DimmedGoldenEagle = Nart.GoldenEagle;
            public const string GoldenStag = Nart.GoldenStag;
            public const string GiantIbex = Nart.Ibex;
            public const string HillBoar = Nart.HillBoar;
            public const string SmokeMessenger = Nart.SmokeCrow;
            public const string SmokeWoodpecker = Nart.Woodpecker;
            public const string DimmedFungus = Nart.DimmedFungus;
            public const string TaigaLynx = Nart.Lynx;
            public const string DimmedStallion = Nart.Stallion;
            public const string IronEagle = Nart.IronEagle;
            public const string CaucasusBear = Nart.CaucasusBear;
            public const string SnowWolf = Nart.SnowWolf;
        }

        private static class Coast
        {
            public const string Gull = "enemy_forest_thief_gull";
            public const string Crab = "enemy_forest_coconut_crab";
            public const string Jackal = "enemy_desert_jackal";
            public const string SandSnake = "enemy_desert_sand_snake";
            public const string Scorpion = PeriodSpawnTable.DesertScorpion;
            public const string Vulture = "enemy_desert_ember_vulture";
            public const string Lantern = "enemy_desert_broken_lantern";
            public const string Spider = PeriodSpawnTable.ForestSpider;
            public const string Wolf = PeriodSpawnTable.ForestWolf;
            public const string Boar = NightVariantRules.PeccaryEnemyId;
            public const string Ushabti = PeriodSpawnTable.MountainGolem;
            public const string CoralUshabti = "enemy_forest_coral_ushabti";
            public const string Potoo = NightVariantRules.PotooEnemyId;
            public const string Bats = PeriodSpawnTable.CaveBats;
            public const string Hawk = "enemy_mountain_crested_hawk";
            public const string Baboon = "enemy_mountain_baboon";
            public const string RollingStone = "enemy_mountain_rolling_stone";
            public const string Ooze = "enemy_cave_ooze";
            public const string Crow = "enemy_cave_dark_crow";
            public const string LanternFish = "enemy_cave_lantern_fish";
            public const string Crocodile = "enemy_forest_mangrove_crocodile";
            public const string Lion = "enemy_forest_grass_lion";
            public const string Rhino = "enemy_forest_dust_rhino";
        }

        private static OverworldLayout Costa()
        {
            OverworldLayout w = NewRegion(CostaId, MapRegion.CostaDasMoncoes, Rect.MinMaxRect(-600f, -1200f, 1800f, 1200f));
            w.World.Biome = BiomeType.Forest;
            const string taa = TaaPwaniMapId;
            const string coast = MapRegionTypes.Coast;
            const string plain = MapRegionTypes.Plain;
            const string ruins = MapRegionTypes.Ruins;
            const string swamp = MapRegionTypes.Swamp;
            const string rock = MapRegionTypes.Mountain;

            // ---------------- Zones (a 5 × 5 grid that tiles the region; columns x −600/−150/250/750/1250/1800, rows z −1200/−700/−250/250/700/1200) ----------------
            // North row: the savanna of 26, Meroë (27), the north cliffs, the palace of Husuni (29).
            OverworldZone zAuroques = Zone(w, "savana_dos_auroques", AuroquesId, null, -600f, 700f, -150f, 1200f, OverworldTerrain.Savanna, 48, 53, 45,
                "Aurochs Savanna", "Savana dos Auroques", plain, spacing: 90f);
            OverworldZone zAcacias = Zone(w, "savana_das_acacias", AuroquesId, null, -150f, 700f, 250f, 1200f, OverworldTerrain.Savanna, 50, 55, 47,
                "Acacia Savanna", "Savana das Acácias", plain, spacing: 90f);
            OverworldZone zMeroe = Zone(w, "piramides_altas_de_meroe", MeroeId, null, 250f, 700f, 750f, 1200f, OverworldTerrain.KushPyramids, 52, 58, 50,
                "High Pyramids of Meroë", "Pirâmides Altas de Meroé", ruins, MapRequirementTags.ArtificerRelic, 90f);
            OverworldZone zFalesiasNorte = Zone(w, "falesias_do_norte", null, MeroeId, 750f, 700f, 1250f, 1200f, OverworldTerrain.SeaCliffs, 58, 62, 55,
                "Northern Cliffs", "Falésias do Norte", rock, spacing: 95f);
            OverworldZone zHusuni = Zone(w, "palacio_de_husuni", KilwaId, null, 1250f, 700f, 1800f, 1200f, OverworldTerrain.CoralRuins, 62, 66, 60,
                "Husuni Palace", "Palácio de Husuni", ruins, MapRequirementTags.AirBubble, 95f);
            // Second row: cloves, coconuts, the tidal lagoons, the coral islets, Kilwa's drowned streets.
            OverworldZone zCravais = Zone(w, "cravais_do_vento_leste", null, taa, -600f, 250f, -150f, 700f, OverworldTerrain.CloveRows, 47, 50, 45,
                "East Wind Clove Rows", "Cravais do Vento Leste", plain, spacing: 85f);
            OverworldZone zCoqueiral = Zone(w, "coqueiral_de_pwani", null, taa, -150f, 250f, 250f, 700f, OverworldTerrain.CoconutGrove, 46, 49, 44,
                "Pwani Coconut Grove", "Coqueiral de Pwani", plain, spacing: 85f);
            OverworldZone zLagoas = Zone(w, "lagoas_de_mare", null, taa, 250f, 250f, 750f, 700f, OverworldTerrain.TideLagoons, 47, 51, 45,
                "Tidal Lagoons", "Lagoas de Maré", coast, spacing: 85f);
            OverworldZone zIlhotas = Zone(w, "ilhotas_de_coral", null, KirtaMapId, 750f, 250f, 1250f, 700f, OverworldTerrain.Shoals, 55, 59, 52,
                "Coral Islets", "Ilhotas de Coral", coast, spacing: 85f);
            OverworldZone zKilwa = Zone(w, "kilwa_afundada", KilwaId, null, 1250f, 250f, 1800f, 700f, OverworldTerrain.SunkenCity, 60, 64, 58,
                "Sunken Kilwa", "Kilwa Afundada", ruins, MapRequirementTags.AirBubble, 85f);
            // Middle row: the dunes, the town's beach, the reef, Kirta's atoll, the kelp flats.
            OverworldZone zDunas = Zone(w, "dunas_das_casuarinas", null, taa, -600f, -250f, -150f, 250f, OverworldTerrain.CoastalDunes, 45, 48, 43,
                "Casuarina Dunes", "Dunas das Casuarinas", coast, spacing: 85f);
            OverworldZone zPraia = Zone(w, "praia_de_taa_pwani", taa, null, -150f, -250f, 250f, 250f, OverworldTerrain.WhiteBeach, 45, 47, 42,
                "Taa Pwani Beach", "Praia de Taa Pwani", coast, spacing: 80f);
            OverworldZone zRecife = Zone(w, "recife_de_taa_pwani", taa, null, 250f, -250f, 750f, 250f, OverworldTerrain.Reef, 46, 50, 44,
                "Taa Pwani Reef", "Recife de Taa Pwani", coast, MapRequirementTags.BoatOrDock, 80f);
            OverworldZone zAtol = Zone(w, "atol_de_kirta", KirtaMapId, null, 750f, -250f, 1250f, 250f, OverworldTerrain.Atoll, 55, 60, 52,
                "Kirta Atoll", "Atol de Kirta", coast, MapRequirementTags.BreathingShell, 85f);
            OverworldZone zAlgas = Zone(w, "jardins_de_alga", KirtaMapId, null, 1250f, -250f, 1800f, 250f, OverworldTerrain.KelpFlats, 58, 62, 55,
                "Kelp Gardens", "Jardins de Alga", coast, MapRequirementTags.BreathingShell, 85f);
            // Fourth row: the restinga (the road from the Zarafshan), the dhows' beach, the south shoals, the great lagoon, Liyongo's channels.
            OverworldZone zRestinga = Zone(w, "mata_da_restinga", null, taa, -600f, -700f, -150f, -250f, OverworldTerrain.CoastScrub, 46, 49, 44,
                "Restinga Thicket", "Mata da Restinga", MapRegionTypes.Forest, spacing: 85f);
            OverworldZone zDhows = Zone(w, "praia_dos_dhows", null, taa, -150f, -700f, 250f, -250f, OverworldTerrain.WhiteBeach, 46, 49, 44,
                "Beach of the Dhows", "Praia dos Dhows", coast, spacing: 85f);
            OverworldZone zBaixios = Zone(w, "baixios_do_sul", null, taa, 250f, -700f, 750f, -250f, OverworldTerrain.Shoals, 47, 51, 45,
                "Southern Shallows", "Baixios do Sul", coast, spacing: 85f);
            OverworldZone zLagoaGrande = Zone(w, "lagoa_grande", null, KirtaMapId, 750f, -700f, 1250f, -250f, OverworldTerrain.TideLagoons, 55, 59, 52,
                "Great Lagoon", "Lagoa Grande", coast, spacing: 85f);
            OverworldZone zCanais = Zone(w, "canais_de_liyongo", LiyongoId, null, 1250f, -700f, 1800f, -250f, OverworldTerrain.Mangrove, 64, 68, 60,
                "Liyongo Channels", "Canais de Liyongo", swamp, MapRequirementTags.RootBoots, 85f);
            // South row: baobabs, the red cliffs, Gedi's ruins, the shallow mangrove, the Mangues de Liyongo.
            OverworldZone zBaobas = Zone(w, "bosque_dos_baobas", null, AuroquesId, -600f, -1200f, -150f, -700f, OverworldTerrain.BaobabWoods, 52, 56, 49,
                "Baobab Wood", "Bosque dos Baobás", plain, spacing: 90f);
            OverworldZone zFalesias = Zone(w, "falesias_vermelhas", null, taa, -150f, -1200f, 250f, -700f, OverworldTerrain.SeaCliffs, 52, 56, 49,
                "Red Cliffs", "Falésias Vermelhas", rock, spacing: 95f);
            OverworldZone zGedi = Zone(w, "ruinas_de_gedi", null, KilwaId, 250f, -1200f, 750f, -700f, OverworldTerrain.CoralRuins, 54, 58, 51,
                "Ruins of Gedi", "Ruínas de Gedi", ruins, spacing: 90f);
            OverworldZone zMangueRaso = Zone(w, "mangue_raso", null, LiyongoId, 750f, -1200f, 1250f, -700f, OverworldTerrain.Mangrove, 60, 64, 58,
                "Shallow Mangrove", "Mangue Raso", swamp, spacing: 85f);
            OverworldZone zMangues = Zone(w, "mangues_de_liyongo", LiyongoId, null, 1250f, -1200f, 1800f, -700f, OverworldTerrain.Mangrove, 66, 72, 64,
                "Liyongo Mangroves", "Mangues de Liyongo", swamp, MapRequirementTags.RootBoots, 85f);

            AddCostaFauna(zAuroques, zAcacias, zMeroe, zFalesiasNorte, zHusuni, zCravais, zCoqueiral, zLagoas, zIlhotas, zKilwa, zDunas, zPraia, zRecife, zAtol,
                zAlgas, zRestinga, zDhows, zBaixios, zLagoaGrande, zCanais, zBaobas, zFalesias, zGedi, zMangueRaso, zMangues);

            // ---------------- Settlements ----------------
            // Taa Pwani first: the region's return city and the barca's stop for 25.
            CityTownRecipe taaPwani = CityTownRecipes.TaaPwani();
            taaPwani.Backdrop = false;
            w.Vila = Place(w, taa, taaPwani, TaaPwaniOffset, Rect.MinMaxRect(-38f, -34f, 40f, 26f), 42f, true);

            // Aratta (26, the bull-folk) and Kirta (28, the sea-folk): the race cities, return cities of 26–27 and 28–29, on flat
            // ground at y = 0 (their solids stay within 22 m of the plaza). Kirta's south edge is the reef over the abyss: its
            // roads leave west, east and north.
            var raceSafe = Rect.MinMaxRect(-24f, -24f, 24f, 24f);
            CityTownRecipe aratta = CityTownRecipes.Aratta();
            aratta.Backdrop = false;
            Place(w, AuroquesId, aratta, ArattaOffset, raceSafe, RaceCityGround, true);
            CityTownRecipe kirta = CityTownRecipes.Kirta();
            kirta.Backdrop = false;
            Place(w, KirtaMapId, kirta, KirtaOffset, raceSafe, RaceCityGround, true);

            // The posts of 27, 29 and 30.
            Place(w, MeroeId, CoastPost("patio_dos_relevos", TownSwatch.Sandstone, TownShopDressing.Storyteller), PatioRelevosOffset, PostSafe, 18f, false);
            Place(w, KilwaId, CoastPost("farol_de_coral", TownSwatch.Sea, TownShopDressing.Wicks), FarolDeCoralOffset, PostSafe, 18f, false);
            Place(w, LiyongoId, CoastPost("cais_das_dhows", TownSwatch.Sail, TownShopDressing.RootWeaver), CaisDasDhowsOffset, PostSafe, 18f, false);

            // ---------------- Water ----------------
            // The Canal do Recife: the deep channel between the beach and the reef flat; a ford on the east road, one in the
            // south shallows and one in the north lagoons, the coral causeway (bridge) south of the town.
            MapRiver canal = River(w, 30f, ReefCanal);
            Crossing(canal, On(ReefCanal[2], ReefCanal[3], 150f / 270f), 10f, false);
            Crossing(canal, On(ReefCanal[0], ReefCanal[1], 0.5f), 10f, false);
            Crossing(canal, On(ReefCanal[4], ReefCanal[5], 0.3f), 10f, false);
            Crossing(canal, On(ReefCanal[1], ReefCanal[2], 0.5f), 4.5f, true);

            // Tide pools on the reef flat; the tidal lagoons.
            River(w, 14f, new Vector2(450f, -120f), new Vector2(480f, -130f));
            River(w, 18f, new Vector2(560f, 90f), new Vector2(600f, 110f));
            River(w, 12f, new Vector2(650f, -60f), new Vector2(690f, -40f));
            River(w, 10f, new Vector2(420f, 160f), new Vector2(445f, 175f));
            River(w, 34f, new Vector2(380f, 560f), new Vector2(520f, 600f));
            River(w, 30f, new Vector2(560f, 360f), new Vector2(700f, 330f));
            River(w, 24f, new Vector2(600f, 630f), new Vector2(700f, 650f));

            // The south shallows: a channel across the islets, two baixios.
            Vector2 s0 = new Vector2(255f, -470f), s1 = new Vector2(500f, -500f), s2 = new Vector2(745f, -470f);
            MapRiver south = River(w, 26f, s0, s1, s2);
            Crossing(south, On(s0, s1, 0.55f), 12f, false);
            Crossing(south, On(s1, s2, 0.5f), 12f, false);

            // Kirta's lagoon north of the race city's ground (rows of water side by side: blocked edge to edge).
            River(w, 38f, new Vector2(880f, 70f), new Vector2(1120f, 70f), new Vector2(1120f, 106f), new Vector2(880f, 106f), new Vector2(880f, 142f),
                new Vector2(1120f, 142f), new Vector2(1120f, 178f), new Vector2(880f, 178f));

            // The coral islets: a channel north–south with a baixio, one east–west.
            Vector2 i0 = new Vector2(1000f, 265f), i1 = new Vector2(980f, 470f), i2 = new Vector2(1010f, 690f);
            MapRiver islets = River(w, 28f, i0, i1, i2);
            Crossing(islets, On(i0, i1, 0.5f), 12f, false);
            Crossing(islets, On(i1, i2, 0.6f), 12f, false);
            River(w, 22f, new Vector2(780f, 600f), new Vector2(900f, 580f));

            // The great lagoon (D4), east of the trail.
            River(w, 36f, new Vector2(880f, -560f), new Vector2(1180f, -560f), new Vector2(1180f, -524f), new Vector2(880f, -524f));

            // The kelp flats: a channel to the east wall, a baixio on the road to Kilwa.
            Vector2 k0 = new Vector2(1265f, 100f), k1 = new Vector2(1500f, 60f), k2 = new Vector2(1770f, 120f);
            MapRiver kelp = River(w, 30f, k0, k1, k2);
            Crossing(kelp, On(k0, k1, 0.6f), 12f, false);
            Crossing(kelp, On(k1, k2, 0.5f), 12f, false);

            // Kilwa's drowned streets: two east–west canals and two north–south ones, fords and bridges.
            MapRiver kilwaSouth = River(w, 14f, new Vector2(1290f, 360f), new Vector2(1760f, 360f));
            Crossing(kilwaSouth, new Vector2(1450f, 360f), 8f, false);
            Crossing(kilwaSouth, new Vector2(1700f, 360f), 4f, true);
            MapRiver kilwaNorth = River(w, 14f, new Vector2(1290f, 560f), new Vector2(1760f, 560f));
            Crossing(kilwaNorth, new Vector2(1560f, 560f), 8f, false);
            Crossing(kilwaNorth, new Vector2(1330f, 560f), 4f, true);
            MapRiver kilwaWest = River(w, 12f, new Vector2(1380f, 280f), new Vector2(1380f, 680f));
            Crossing(kilwaWest, new Vector2(1380f, 470f), 8f, false);
            MapRiver kilwaEast = River(w, 12f, new Vector2(1640f, 280f), new Vector2(1640f, 680f));
            Crossing(kilwaEast, new Vector2(1640f, 470f), 4f, true);

            // The mangroves: the Canal de Liyongo from the south wall to the east, the channels of 30, a creek in the shallow mangrove.
            Vector2 l0 = new Vector2(1000f, -1190f), l1 = new Vector2(1050f, -1000f), l2 = new Vector2(1250f, -930f), l3 = new Vector2(1500f, -900f), l4 = new Vector2(1770f, -960f);
            MapRiver liyongo = River(w, 16f, l0, l1, l2, l3, l4);
            Crossing(liyongo, On(l2, l3, 0.6f), 9f, false);
            Crossing(liyongo, On(l1, l2, 0.5f), 4.5f, true);
            Crossing(liyongo, On(l3, l4, 0.6f), 9f, false);
            Vector2 c0 = new Vector2(1265f, -600f), c1 = new Vector2(1450f, -620f), c2 = new Vector2(1770f, -580f);
            MapRiver channels = River(w, 12f, c0, c1, c2);
            Crossing(channels, On(c0, c1, 0.85f), 8f, false);
            Crossing(channels, On(c1, c2, 0.6f), 4f, true);
            Vector2 m0 = new Vector2(1300f, -1185f), m1 = new Vector2(1400f, -1060f), m2 = new Vector2(1600f, -1120f), m3 = new Vector2(1770f, -1040f);
            MapRiver hedammu = River(w, 12f, m0, m1, m2, m3);
            Crossing(hedammu, On(m1, m2, 0.5f), 8f, false);
            River(w, 10f, new Vector2(820f, -1150f), new Vector2(900f, -1050f), new Vector2(980f, -1020f));

            // The savanna's three pools, a well in Gedi, the coves under the cliffs.
            River(w, 26f, new Vector2(-250f, 1080f), new Vector2(-200f, 1100f));
            River(w, 22f, new Vector2(-480f, 780f), new Vector2(-440f, 770f));
            River(w, 24f, new Vector2(100f, 900f), new Vector2(150f, 920f));
            River(w, 10f, new Vector2(560f, -1000f), new Vector2(580f, -1010f));
            River(w, 34f, new Vector2(60f, -1188f), new Vector2(120f, -1060f));
            River(w, 30f, new Vector2(1000f, 1188f), new Vector2(950f, 1080f));

            // ---------------- Gates out of the region ----------------
            Gate(w, "exit_zarafshan", MapExitKind.Map, MapCatalog.PomarId, Rect.MinMaxRect(-600f, -438f, -597f, -422f), new Vector2(-591f, -430f), Vector2.right);
            Gate(w, "exit_island", MapExitKind.WorldMap, null, Rect.MinMaxRect(-92f, 68f, -88f, 72f), new Vector2(-90f, 60f), Vector2.up);
            Landmark(w, MapLandmarkKind.StoneRing, -90f, 70f, 0f, 0.45f);

            // ---------------- Trails ----------------
            // The road from the Zarafshan through the restinga to Taa Pwani's West street.
            Vector2 westFork = new Vector2(-330f, -300f);
            PathVia(w, 5f, new Vector2(-591f, -430f), new Vector2(-470f, -400f), westFork, new Vector2(-200f, -150f), new Vector2(-120f, -20f), new Vector2(-42f, 0f));
            PathVia(w, 3.5f, westFork, new Vector2(-420f, -650f), new Vector2(-400f, -950f), new Vector2(-300f, -1080f));
            // The south road: the dhows' beach, the red cliffs; the branch over the canal's south baixio to the shallows, Gedi and the mangroves.
            Vector2 southFork = new Vector2(0f, -560f);
            PathVia(w, 4f, new Vector2(0f, -42f), new Vector2(0f, -250f), new Vector2(10f, -420f), southFork, new Vector2(0f, -760f), new Vector2(-20f, -950f),
                new Vector2(-60f, -1100f));
            Vector2 canalSouth = On(ReefCanal[0], ReefCanal[1], 0.5f);
            Vector2 gediFork = new Vector2(620f, -820f);
            PathVia(w, 3.5f, southFork, new Vector2(150f, -560f), canalSouth, new Vector2(450f, -600f), new Vector2(560f, -680f), gediFork, new Vector2(450f, -950f));
            PathVia(w, 3.5f, gediFork, new Vector2(750f, -880f), new Vector2(950f, -860f), new Vector2(1150f, -800f), new Vector2(1350f, -700f));
            // The east road: over the canal's ford, across the reef flat to Kirta's ground.
            Vector2 canalFord = On(ReefCanal[2], ReefCanal[3], 150f / 270f);
            Vector2 reefFork = new Vector2(650f, 0f);
            PathVia(w, 4f, new Vector2(42f, 0f), new Vector2(180f, 0f), canalFord, new Vector2(450f, 10f), reefFork, new Vector2(760f, -30f),
                KirtaOffset + new Vector2(-RaceCityGround, 0f));
            // From the reef north over the coral islets' baixio to Kilwa (ford over its west canal), south to the great lagoon.
            Vector2 isletsFord = On(i0, i1, 0.5f);
            PathVia(w, 3.5f, reefFork, new Vector2(800f, 300f), isletsFord, new Vector2(1150f, 450f), new Vector2(1290f, 470f), new Vector2(1380f, 470f),
                FarolDeCoralOffset + new Vector2(-18f, 0f));
            PathVia(w, 3.5f, new Vector2(760f, -30f), new Vector2(800f, -300f), new Vector2(850f, -480f), new Vector2(780f, -650f), new Vector2(750f, -880f));
            // From Kirta east over the kelp baixio to Kilwa's south canal and the Farol; from the Farol north to Husuni.
            Vector2 kelpFord = On(k0, k1, 0.6f);
            PathVia(w, 3.5f, KirtaOffset + new Vector2(RaceCityGround, 0f), new Vector2(1250f, -20f), kelpFord, new Vector2(1420f, 200f), new Vector2(1450f, 300f),
                new Vector2(1450f, 360f), new Vector2(1450f, 430f), FarolDeCoralOffset + new Vector2(-18f, -18f));
            PathVia(w, 3.5f, FarolDeCoralOffset + new Vector2(18f, 0f), new Vector2(1560f, 500f), new Vector2(1560f, 560f), new Vector2(1560f, 700f), new Vector2(1500f, 950f));
            // Into Liyongo's channels: the ford to the Cais das Dhows, the ford over the Canal de Liyongo into the mangroves.
            Vector2 channelsFord = On(c0, c1, 0.85f);
            PathVia(w, 3.5f, new Vector2(1350f, -700f), channelsFord, CaisDasDhowsOffset + new Vector2(-18f, 0f));
            Vector2 liyongoFord = On(l2, l3, 0.6f);
            Vector2 hedammuFord = On(m1, m2, 0.5f);
            PathVia(w, 3.5f, new Vector2(1350f, -700f), new Vector2(1390f, -820f), liyongoFord, new Vector2(1470f, -1000f), hedammuFord, new Vector2(1540f, -1150f));
            // North: the coconut grove to Aratta; Meroë's court; on along the cliffs to Husuni.
            Vector2 northFork = new Vector2(-40f, 500f);
            PathVia(w, 4f, new Vector2(-50f, 42f), new Vector2(-80f, 120f), new Vector2(-60f, 300f), northFork, new Vector2(-100f, 700f), new Vector2(-250f, 850f),
                ArattaOffset + new Vector2(RaceCityGround, 0f));
            PathVia(w, 3.5f, new Vector2(-80f, 120f), new Vector2(-300f, 300f), new Vector2(-450f, 500f));
            PathVia(w, 3.5f, northFork, new Vector2(150f, 620f), new Vector2(350f, 800f), PatioRelevosOffset + new Vector2(-18f, 0f));
            PathVia(w, 3.5f, PatioRelevosOffset + new Vector2(18f, 0f), new Vector2(750f, 1000f), new Vector2(1000f, 1010f), new Vector2(1250f, 980f), new Vector2(1500f, 950f));

            // ---------------- Landmarks ----------------
            Sign(w, westFork, new Vector2(-420f, -650f), 5f, 0f);
            Sign(w, southFork, new Vector2(150f, -560f), 4f, 0f);
            Sign(w, northFork, new Vector2(150f, 620f), 4f, 0f);
            Sign(w, reefFork, new Vector2(800f, 300f), 4f, 20f);
            Sign(w, gediFork, new Vector2(750f, -880f), 3.5f, 0f);
            Landmark(w, MapLandmarkKind.Campfire, -470f, -370f, 0f, 0.9f);
            Landmark(w, MapLandmarkKind.Campfire, 30f, -760f, 0f, 0.9f);
            Landmark(w, MapLandmarkKind.Campfire, -130f, 680f, 0f, 1f);
            Landmark(w, MapLandmarkKind.Campfire, 780f, 10f, 0f, 0.9f);
            Landmark(w, MapLandmarkKind.Campfire, 1180f, -790f, 0f, 1f);
            Landmark(w, MapLandmarkKind.Campfire, 1270f, 1000f, 0f, 1f);
            Landmark(w, MapLandmarkKind.StoneRing, -200f, 960f, 0f, 1f);
            Landmark(w, MapLandmarkKind.StoneRing, 560f, 1100f, 0f, 0.8f);
            Landmark(w, MapLandmarkKind.StoneRing, HedammuArena.x, HedammuArena.y, 0f, 1.1f);
            Landmark(w, MapLandmarkKind.StoneRing, PolvoDasMoncoesArena.x, PolvoDasMoncoesArena.y, 0f, 1.1f);
            Landmark(w, MapLandmarkKind.StoneRing, TouroDeFumacaArena.x, TouroDeFumacaArena.y, 0f, 1.1f);
            Landmark(w, MapLandmarkKind.StoneRing, LeaoDosRelevosArena.x, LeaoDosRelevosArena.y, 0f, 1.1f);
            Landmark(w, MapLandmarkKind.StoneRing, MoreiaDoCoralCinzaArena.x, MoreiaDoCoralCinzaArena.y, 0f, 1.1f);
            Landmark(w, MapLandmarkKind.StoneRing, CaranguejoPalacioArena.x, CaranguejoPalacioArena.y, 0f, 1.1f);

            // ---------------- Finishing ----------------
            SplitPaths(w.World);
            FinishSettlements(w);
            CleanFixedProps(w);
            foreach (OverworldZone zone in w.Zones)
            {
                GenerateClearings(w, zone);
            }

            TrimSteepClearings(w);
            return w;
        }

        // ------------------------------------------------------------------
        // The posts of 27, 29 and 30 (the coast style of the town kit)
        // ------------------------------------------------------------------

        /// <summary>A coast post: a coral-stone workshop at the back of the plaza, two houses, the Cais da Barca, lamps and nets.</summary>
        private static CityTownRecipe CoastPost(string id, TownSwatch trim, TownShopDressing dressing)
        {
            var posto = new CityTownRecipe
            {
                CityId = id, Region = BiomeType.Forest, Style = TownStyles.Coast, PlazaRadius = 4.5f, StreetWidth = 3.5f, SidewalkWidth = 0.8f,
                WalkHalfExtent = 16f, EntryArm = CityArm.West, AutoYards = false, Backdrop = false, NightLights = 4,
            };
            posto.Arms.Add(new CityArmRecipe(CityArm.West, 12f, exitToMap: true));
            posto.Arms.Add(new CityArmRecipe(CityArm.East, 12f, exitToMap: true));
            posto.Lots.Add(new CityLotRecipe
            {
                Id = id + "_oficina", Kind = CityLotKind.Workshop, Free = true, Center = new Vector2(0f, 9.5f), Front = Vector2.down, Width = 7f, Depth = 5f,
                Porch = 1.4f, Spec = CoralSpec(trim, dressing),
            });
            posto.Lots.Add(new CityLotRecipe { Id = id + "_cais", Kind = CityLotKind.ShipDock, Arm = CityArm.West, Side = CityArm.North, Along = 8.5f, Width = 6f, Depth = 3f, Porch = 1f });
            posto.Lots.Add(new CityLotRecipe
            {
                Id = id + "_casa1", Kind = CityLotKind.House, Arm = CityArm.East, Side = CityArm.South, Along = 8.5f, Width = 5.5f, Depth = 4.6f, Porch = 1.2f,
                Spec = CoralSpec(TownSwatch.Laterite, TownShopDressing.None),
            });
            posto.Lots.Add(new CityLotRecipe
            {
                Id = id + "_casa2", Kind = CityLotKind.House, Arm = CityArm.East, Side = CityArm.North, Along = 9f, Width = 5f, Depth = 4.4f, Porch = 1f,
                Spec = CoralSpec(TownSwatch.Sea, TownShopDressing.None),
            });
            posto.PlazaProps.Add(Prop(CityPropKind.Crates, -3f, -6f, 20f));
            posto.PlazaProps.Add(Prop(CityPropKind.Jugs, 3.2f, -5.6f, 40f));
            posto.PlazaProps.Add(Prop(CityPropKind.Tree, -8.5f, -7.5f, 0f, 1.1f));
            foreach (float angle in new[] { 45f, 135f, 225f, 315f })
            {
                posto.PlazaProps.Add(new CityProp { Kind = CityPropKind.LampPost, Position = CityTownRecipes.Polar(angle, 4.9f), Yaw = CityGeometry.YawFacing(-CityTownRecipes.Polar(angle, 1f)) });
            }

            return posto;
        }

        /// <summary>A whitewashed coral-stone building with teak joinery and brass, like Taa Pwani's.</summary>
        private static TownBuildingSpec CoralSpec(TownSwatch trim, TownShopDressing dressing)
        {
            return new TownBuildingSpec
            {
                WallColor = TownSwatch.CoralLime, TrimColor = trim, RoofColor = TownSwatch.CoralLime, JoineryColor = TownSwatch.Teak, AccentColor = TownSwatch.Gold,
                SignColor = TownSwatch.Sea, Roof = TownRoofKind.Flat, Dressing = dressing, WallHeight = 3f, CoralStone = true,
            };
        }

        // ------------------------------------------------------------------
        // Monster groups per area (existing definitions only, no boss; Mapas100 25–30 archetypes)
        // ------------------------------------------------------------------

        private static void AddCostaFauna(OverworldZone auroques, OverworldZone acacias, OverworldZone meroe, OverworldZone falesiasNorte, OverworldZone husuni,
            OverworldZone cravais, OverworldZone coqueiral, OverworldZone lagoas, OverworldZone ilhotas, OverworldZone kilwa, OverworldZone dunas, OverworldZone praia,
            OverworldZone recife, OverworldZone atol, OverworldZone algas, OverworldZone restinga, OverworldZone dhows, OverworldZone baixios,
            OverworldZone lagoaGrande, OverworldZone canais, OverworldZone baobas, OverworldZone falesias, OverworldZone gedi, OverworldZone mangueRaso,
            OverworldZone mangues)
        {
            // The plankton glows on the reef at night (the firefly swarm), the Mngwa prowls the beaches (the werewolf's body).
            NightSpecies plankton = NightVariantRules.VagaLume;
            NightSpecies mngwa = NightVariantRules.Lobisomem;
            NightSpecies kongamato = NightVariantRules.MorcegoVampiro;

            // Praia de Taa Pwani (25): the Gaivota Ladra [rápido] and the Caranguejo-dos-Coqueiros [tanque].
            praia.Fauna.Add(Fauna("gaivotas_ladras", "[rápido]", T(N(Coast.Gull, 3), N(Coast.Crab, 1)), T(V(Coast.Gull, 2), S(mngwa, 1)), 4, null, 22f));
            praia.Fauna.Add(Fauna("caranguejos_da_praia", "[tanque]", T(N(Coast.Crab, 3), N(Coast.Gull, 1)), T(V(Coast.Crab, 2), S(NightVariantRules.RasgaMortalha, 1)), 3, null, 28f));

            // Recife de Taa Pwani (25): crabs on the coral, the Moreia Apagada [rápido] (the sand snake's body), lantern fish in the pools.
            recife.Fauna.Add(Fauna("caranguejos_do_recife", "[tanque]", T(N(Coast.Crab, 3), N(Coast.LanternFish, 1)), T(V(Coast.Crab, 2), V(Coast.LanternFish, 1)), 3, null, 28f));
            recife.Fauna.Add(Fauna("moreias", "[rápido]", T(N(MidBeasts.Moray, 3), N(Coast.Gull, 1)), T(V(MidBeasts.Moray, 2), S(plankton, 1)), 4));
            recife.Fauna.Add(Fauna("gaivotas_do_recife", "[rápido] [distância]", T(N(Coast.Gull, 3), N(Coast.LanternFish, 1)), T(V(Coast.Gull, 2), S(plankton, 1)), 4, null, 22f));

            // Dunas das Casuarinas: jackals and sand snakes, scorpions under the casuarinas.
            dunas.Fauna.Add(Fauna("chacais_das_dunas", "[rápido]", T(N(Coast.Jackal, 3), N(Coast.Gull, 1)), T(V(Coast.Jackal, 2), S(mngwa, 1)), 4));
            dunas.Fauna.Add(Fauna("cobras_das_dunas", "[controle]", T(N(Coast.SandSnake, 2), N(Coast.Scorpion, 2)), T(V(Coast.SandSnake, 2), V(Coast.Scorpion, 1)), 4));

            // Coqueiral: the coconut crabs climb the palms, gulls steal the copra.
            coqueiral.Fauna.Add(Fauna("caranguejos_dos_coqueiros", "[tanque]", T(N(Coast.Crab, 3), N(Coast.Boar, 1)), T(V(Coast.Crab, 2), S(NightVariantRules.Queixada, 1)), 3, null, 28f));
            coqueiral.Fauna.Add(Fauna("gaivotas_da_copra", "[rápido]", T(N(Coast.Gull, 3), N(Coast.Potoo, 1)), T(V(Coast.Gull, 2), S(NightVariantRules.MaeDaLua, 1)), 4, null, 22f));

            // Praia dos Dhows: gulls on the hulls, crabs under them, spiders in the nets.
            dhows.Fauna.Add(Fauna("gaivotas_dos_dhows", "[rápido]", T(N(Coast.Gull, 3), N(Coast.Jackal, 1)), T(V(Coast.Gull, 2), V(Coast.Jackal, 1)), 4, null, 22f));
            dhows.Fauna.Add(Fauna("caranguejos_dos_cascos", "[tanque] [controle]", T(N(Coast.Crab, 3), N(MidBeasts.SmokeUrchin, 1)), T(V(Coast.Crab, 2), S(NightVariantRules.RasgaMortalha, 1)), 4));

            // Cravais: peccaries rooting under the clove trees, spiders in the rows.
            cravais.Fauna.Add(Fauna("queixadas_do_cravo", "[rápido]", T(N(Coast.Boar, 3), N(Coast.Spider, 1)), T(S(NightVariantRules.Queixada, 2), V(Coast.Spider, 1)), 4));
            cravais.Fauna.Add(Fauna("aranhas_do_cravo", "[controle]", T(N(Coast.Spider, 3), N(Coast.Potoo, 1)), T(V(Coast.Spider, 2), S(NightVariantRules.MaeDaLua, 1)), 4));

            // Mata da Restinga: spiders and the wolf in the thicket, peccaries and crabs.
            restinga.Fauna.Add(Fauna("aranhas_da_restinga", "[controle]", T(N(Coast.Spider, 3), N(Coast.Wolf, 1)), T(V(Coast.Spider, 2), S(mngwa, 1)), 4));
            restinga.Fauna.Add(Fauna("queixadas_da_restinga", "[rápido] [tanque]", T(N(Coast.Boar, 2), N(Coast.Crab, 2)), T(S(NightVariantRules.Queixada, 2), V(Coast.Crab, 1)), 4));

            // Lagoas de Maré: the lodo in the mud [controle], crabs and lantern fish in the pools.
            lagoas.Fauna.Add(Fauna("lodo_da_lagoa", "[controle]", T(N(MidBeasts.SmokeUrchin, 3), N(Coast.Gull, 1)), T(V(MidBeasts.SmokeUrchin, 2), S(plankton, 1)), 4));
            lagoas.Fauna.Add(Fauna("pocas_de_mare", "[tanque] [distância]", T(N(Coast.Crab, 2), N(Coast.LanternFish, 2)), T(V(Coast.Crab, 2), V(Coast.LanternFish, 1)), 4));

            // Baixios do Sul: gulls over the shallows, lantern fish in the channel.
            baixios.Fauna.Add(Fauna("gaivotas_dos_baixios", "[rápido]", T(N(Coast.Gull, 3), N(Coast.Crab, 1)), T(V(Coast.Gull, 2), V(Coast.Crab, 1)), 4, null, 22f));
            baixios.Fauna.Add(Fauna("peixes_lanterna", "[distância]", T(N(Coast.LanternFish, 3), N(Coast.SandSnake, 1)), T(V(Coast.LanternFish, 2), S(plankton, 1)), 4));

            // Savana dos Auroques (26): the Leão do Capim Apagado [rápido] (the wolf's body), the Rinoceronte-de-Pó [tanque]
            // (peccary and ushabti), the Ave-Relâmpago [distância] (the ember vulture); the smoke hyenas at night.
            auroques.Fauna.Add(Fauna("leoes_do_capim", "[rápido]", T(N(Coast.Lion, 3), N(Coast.Jackal, 1)), T(V(Coast.Lion, 2), S(mngwa, 1)), 4));
            auroques.Fauna.Add(Fauna("rinocerontes_de_po", "[tanque]", T(N(Coast.Rhino, 3), N(Coast.Jackal, 1)), T(V(Coast.Rhino, 2), S(NightVariantRules.Queixada, 1)), 3, null, 30f));
            auroques.Fauna.Add(Fauna("aves_relampago", "[distância]", T(N(MidBeasts.LightningBird, 3), N(Coast.Hawk, 1)), T(V(MidBeasts.LightningBird, 2), S(NightVariantRules.RasgaMortalha, 1)), 3));
            acacias.Fauna.Add(Fauna("hienas_de_fumaca", "[enxame] [rápido]", T(N(Coast.Jackal, 3), N(Coast.Wolf, 1)), T(V(Coast.Jackal, 3), S(mngwa, 1)), 5));
            acacias.Fauna.Add(Fauna("aves_das_acacias", "[distância]", T(N(MidBeasts.LightningBird, 2), N(Coast.Hawk, 2)), T(V(MidBeasts.LightningBird, 1), V(Coast.Hawk, 1), S(NightVariantRules.RasgaMortalha, 1)), 4));

            // Bosque dos Baobás: baboons in the baobabs, potoos on the branches.
            baobas.Fauna.Add(Fauna("babuinos_dos_baobas", "[rápido] [enxame]", T(N(Coast.Baboon, 3), N(Coast.Boar, 1)), T(V(Coast.Baboon, 2), S(NightVariantRules.Queixada, 1)), 4));
            baobas.Fauna.Add(Fauna("urutaus", "[distância]", T(N(MidBeasts.LightningBird, 3), N(Coast.Vulture, 1)), T(S(NightVariantRules.MaeDaLua, 2), V(MidBeasts.LightningBird, 1)), 3));

            // Falésias Vermelhas: hawks on the bluffs, the Pedra que Rola down the laterite.
            falesias.Fauna.Add(Fauna("gavioes_das_falesias", "[distância]", T(N(Coast.Hawk, 3), N(Coast.Gull, 1)), T(V(Coast.Hawk, 2), S(NightVariantRules.MaeDaLua, 1)), 3));
            falesias.Fauna.Add(Fauna("pedras_da_falesia", "[controle] [tanque]", T(N(Coast.RollingStone, 2), N(Coast.Ushabti, 1)), T(V(Coast.RollingStone, 2), V(Coast.Ushabti, 1)), 3, null, 30f));

            // Pirâmides Altas de Meroé (27): the Ushabti de Bronze Kushita [tanque], the Escorpião-Rei Jovem [rápido], the
            // Chacal de Areia Negra [enxame], the broken lanterns of the chapels.
            meroe.Fauna.Add(Fauna("ushabtis_kushitas", "[tanque]", T(N(Coast.Ushabti, 3), N(Coast.Lantern, 1)), T(V(Coast.Ushabti, 2), V(Coast.Lantern, 1)), 3, null, 30f));
            meroe.Fauna.Add(Fauna("escorpioes_rei", "[rápido]", T(N(Coast.Scorpion, 3), N(Coast.Jackal, 1)), T(V(Coast.Scorpion, 2), S(plankton, 1)), 4));
            meroe.Fauna.Add(Fauna("chacais_de_areia_negra", "[enxame]", T(N(Coast.Jackal, 3), N(Coast.SandSnake, 1)), T(V(Coast.Jackal, 2), S(mngwa, 1)), 5));

            // Falésias do Norte: hawks and baboons on the headland, the rolling stones.
            falesiasNorte.Fauna.Add(Fauna("gavioes_do_norte", "[distância]", T(N(Coast.Hawk, 3), N(Coast.Gull, 1)), T(V(Coast.Hawk, 2), S(NightVariantRules.MaeDaLua, 1)), 3));
            falesiasNorte.Fauna.Add(Fauna("babuinos_da_falesia", "[rápido] [controle]", T(N(Coast.Baboon, 3), N(Coast.RollingStone, 1)), T(V(Coast.Baboon, 2), V(Coast.RollingStone, 1)), 4));

            // Ruínas de Gedi: coral ushabtis, the crows of the dark on the pillar tombs.
            gedi.Fauna.Add(Fauna("ushabtis_de_coral", "[tanque]", T(N(Coast.CoralUshabti, 3), N(Coast.Crow, 1)), T(V(Coast.CoralUshabti, 2), V(Coast.Crow, 1)), 3, null, 30f));
            gedi.Fauna.Add(Fauna("corvos_de_gedi", "[distância]", T(N(Coast.Crow, 3), N(Coast.Bats, 1)), T(V(Coast.Crow, 2), S(kongamato, 1)), 4));

            // Atol de Kirta (28): the Caranguejo-Aranha [tanque], the Peixe-Leão Apagado [distância] (lantern fish), the
            // Água-Viva de Lodo [controle].
            atol.Fauna.Add(Fauna("caranguejos_aranha", "[tanque]", T(N(Coast.Crab, 3), N(Coast.LanternFish, 1)), T(V(Coast.Crab, 2), V(Coast.LanternFish, 1)), 3, null, 28f));
            atol.Fauna.Add(Fauna("peixes_leao", "[distância]", T(N(Coast.LanternFish, 3), N(Coast.Gull, 1)), T(V(Coast.LanternFish, 2), S(plankton, 1)), 4));
            atol.Fauna.Add(Fauna("aguas_vivas_de_lodo", "[controle]", T(N(Coast.Ooze, 3), N(Coast.Crab, 1)), T(V(Coast.Ooze, 2), V(Coast.Crab, 1)), 4));

            // Jardins de Alga (28): jellyfish of mud in the kelp, the Tubarão-de-Lodo [rápido] (the sand snake's body).
            algas.Fauna.Add(Fauna("aguas_vivas_da_alga", "[controle]", T(N(Coast.Ooze, 3), N(Coast.LanternFish, 1)), T(V(Coast.Ooze, 2), S(plankton, 1)), 4));
            algas.Fauna.Add(Fauna("tubaroes_de_lodo", "[rápido]", T(N(Coast.SandSnake, 3), N(Coast.LanternFish, 1)), T(V(Coast.SandSnake, 2), V(Coast.LanternFish, 1)), 4));

            // Ilhotas de Coral: gulls and crabs on the islets, jellyfish in the channels.
            ilhotas.Fauna.Add(Fauna("gaivotas_das_ilhotas", "[rápido]", T(N(Coast.Gull, 3), N(Coast.Crab, 1)), T(V(Coast.Gull, 2), S(plankton, 1)), 4, null, 22f));
            ilhotas.Fauna.Add(Fauna("caranguejos_das_ilhotas", "[tanque] [controle]", T(N(Coast.Crab, 3), N(Coast.Ooze, 1)), T(V(Coast.Crab, 2), V(Coast.Ooze, 1)), 4));

            // Lagoa Grande: the lodo, crows over the water, crabs and gulls on the banks.
            lagoaGrande.Fauna.Add(Fauna("lodo_da_lagoa_grande", "[controle]", T(N(Coast.Ooze, 3), N(Coast.Crow, 1)), T(V(Coast.Ooze, 2), S(plankton, 1)), 4));
            lagoaGrande.Fauna.Add(Fauna("margens_da_lagoa", "[tanque] [rápido]", T(N(Coast.Crab, 2), N(Coast.Gull, 2)), T(V(Coast.Crab, 1), V(Coast.Gull, 2)), 4));

            // Kilwa Afundada (29): the Ushabti de Coral [tanque] (its own beast now, WideBeasts), the Polvo Ladrão [controle] (the lodo), the Raia de Fumaça [rápido]
            // (lantern fish); the dingonek prowls at night (the werewolf's body).
            kilwa.Fauna.Add(Fauna("ushabtis_de_kilwa", "[tanque]", T(N(Coast.CoralUshabti, 3), N(Coast.Ooze, 1)), T(V(Coast.CoralUshabti, 2), V(Coast.Ooze, 1)), 3, null, 30f));
            kilwa.Fauna.Add(Fauna("polvos_ladroes", "[controle]", T(N(Coast.Ooze, 3), N(Coast.Crab, 1)), T(V(Coast.Ooze, 2), S(plankton, 1)), 4));
            kilwa.Fauna.Add(Fauna("raias_de_fumaca", "[rápido]", T(N(Coast.LanternFish, 3), N(Coast.Gull, 1)), T(V(Coast.LanternFish, 2), S(mngwa, 1)), 4));

            // Palácio de Husuni (29): coral ushabtis, crows on the arches, the hermit crabs (the coconut crab) round the
            // octagonal pool, the broken lanterns.
            husuni.Fauna.Add(Fauna("guardas_de_coral", "[tanque] [elite]", T(N(Coast.CoralUshabti, 3), N(Coast.Lantern, 1)), T(V(Coast.CoralUshabti, 2), V(Coast.Lantern, 1)), 3, null, 32f));
            husuni.Fauna.Add(Fauna("eremitas_do_palacio", "[tanque]", T(N(Coast.Crab, 3), N(Coast.Gull, 1)), T(V(Coast.Crab, 2), S(mngwa, 1)), 3, null, 28f));
            husuni.Fauna.Add(Fauna("corvos_de_husuni", "[distância]", T(N(Coast.Crow, 3), N(Coast.Bats, 1)), T(V(Coast.Crow, 2), S(kongamato, 1)), 4));

            // Mangue Raso: the Crocodilo do Mangue [tanque] (the sand snake's body), the fiddler crabs [enxame].
            mangueRaso.Fauna.Add(Fauna("crocodilos_do_mangue", "[tanque]", T(N(Coast.Crocodile, 3), N(Coast.Crab, 1)), T(V(Coast.Crocodile, 2), S(plankton, 1)), 3, null, 28f));
            mangueRaso.Fauna.Add(Fauna("caranguejos_violinistas", "[enxame]", T(N(MidBeasts.FiddlerCrab, 3), N(Coast.Crab, 1)), T(V(MidBeasts.FiddlerCrab, 2), V(Coast.Crab, 1), S(plankton, 1)), 5));

            // Canais e Mangues de Liyongo (30): the Kongamato [distância] (the crow, the vampire bat at night: it doubles), the
            // Garça Apagada [rápido] (the gull's body), the crocodiles, the fiddler crabs; fireflies in the mangroves at night.
            canais.Fauna.Add(Fauna("kongamatos", "[distância]", T(N(Coast.Crow, 3), N(Coast.Bats, 1)), T(V(Coast.Crow, 2), S(kongamato, 2)), 4));
            canais.Fauna.Add(Fauna("garcas_apagadas", "[rápido]", T(N(Coast.Gull, 3), N(Coast.Crow, 1)), T(V(Coast.Gull, 2), S(plankton, 1)), 4, null, 22f));
            canais.Fauna.Add(Fauna("crocodilos_dos_canais", "[tanque]", T(N(Coast.Crocodile, 3), N(Coast.Ooze, 1)), T(V(Coast.Crocodile, 2), V(Coast.Ooze, 1)), 3, null, 30f));
            mangues.Fauna.Add(Fauna("kongamatos_de_liyongo", "[distância]", T(N(Coast.Crow, 3), N(Coast.Bats, 1)), T(V(Coast.Crow, 2), S(kongamato, 2)), 4));
            mangues.Fauna.Add(Fauna("violinistas_de_liyongo", "[enxame] [tanque]", T(N(MidBeasts.FiddlerCrab, 3), N(Coast.Ooze, 1)), T(V(MidBeasts.FiddlerCrab, 2), S(plankton, 1)), 5));
            mangues.Fauna.Add(Fauna("crocodilos_de_liyongo", "[tanque] [elite]", T(N(Coast.Crocodile, 3), N(Coast.Spider, 1)), T(V(Coast.Crocodile, 2), V(Coast.Spider, 1)), 3, null, 32f));
        }
    }
}
