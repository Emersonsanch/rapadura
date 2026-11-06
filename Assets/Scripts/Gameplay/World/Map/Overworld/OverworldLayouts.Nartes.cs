using Rapadura.Gameplay.Night;
using Rapadura.Gameplay.World.Biomes;
using Rapadura.Gameplay.World.Cities.Town;
using Rapadura.Gameplay.World.Map.Areas;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Overworld
{
    /// <summary>
    /// Região 9 — Estepe dos Nartes as ONE continuous map of 2400 × 2400 m (Docs/Mapas100.md §0.10 and §5, maps 47–52), no
    /// scene change between the centaurs' camp and the steppe round it. The Acampamento de Soslan (47, the region's return
    /// city) stands at the plaza (0, 0) by the Rio dos Nartes, 600 m from the south edge where the roads come up from the Costa
    /// das Monções (in the stitched world the steppe fills the walled corner north of the Costa, the Planície de Mingcheng to
    /// its west):
    /// <list type="bullet">
    /// <item>round the camp (nv 100–105): the <b>Acampamento de Soslan</b>, the <b>Vau Largo</b> and the <b>Pistas de
    /// Corrida</b> (47), the <b>Estepe de Soslan</b>, the <b>Estepe das Saigas</b>, and east of it the <b>Kurgans de Ouro</b>
    /// (48);</item>
    /// <item>the middle ring (nv 104–118): the <b>Rio dos Nartes</b>, the <b>Pastos dos Gigantes</b>, the <b>Campos de
    /// Papoula</b>, the <b>Kurgan Aberto</b> (48, post), the <b>Lago Salgado</b>, the <b>Kurgans do Leste</b>, the
    /// <b>Estepe dos Ventos</b> and the <b>Bosque de Bétulas</b> by Mingcheng, the <b>Vale das Tábuas de Sonho</b> and the
    /// <b>Fortim dos Gigantes</b> (49, the hill giants' town), the <b>Vale das Torres Svan</b>, the <b>Pradaria
    /// Ondulada</b>;</item>
    /// <item>the far north (nv 118–150): the <b>Taiga de Lariços</b>, the <b>Arvoredo de Haripushpa</b> (50, the trunk folk's
    /// town) and the <b>Lago de Degelo</b> (50), the <b>Pradaria dos Mil Cavalos</b> with the Poço da Taça (51, post), and the
    /// Caucasus of Amirani's chain (52: Climbing Crampons): the <b>Torre de Vigia</b> (post), the <b>Corrente de Amirani</b>
    /// and the <b>Cume de Ohya</b>.</item>
    /// </list>
    /// Water: the shallow Rio dos Nartes comes down from the snows past the svan towers and the camp to a lake in the south,
    /// crossed at two wide fords (the Vau Largo) and a stone bridge under the Torre de Vigia; the Lago de Degelo in the taiga,
    /// the salt lake. Monsters: existing beasts that fit the sheets (the wolf for the steppe wolves, the stallions and the
    /// snow wolves, the hawk for the golden eagles, the iron eagles and the smoke woodpeckers, the ember vulture for the
    /// steppe vultures, the jackal for the golden stags and the wild goats, the thief gull for the felt swans, the peccary for
    /// the hill boars, the crow for the smoke messengers, the ooze for the dimmed fungus, the ill-wind mist for the grass
    /// storms, the Pamir's brown bear for the Caucasus bear, the rolling stone on the crags), the region's own beasts (the
    /// young griffin, also as the guardian griffins, the saiga, the bronze wolf, the sleepwalking giant, the bark beetle, the
    /// ochokochi) and the night species; no boss.
    /// </summary>
    public static partial class OverworldLayouts
    {
        public const string NartesId = "overworld_nartes";

        public const string SoslanMapId = "map_047_soslan";
        public const string KurgansMapId = "map_048_kurgans";
        public const string FortimMapId = "map_049_fortim";
        public const string HaripushpaMapId = "map_050_haripushpa";
        public const string MilCavalosMapId = "map_051_mil_cavalos";
        public const string AmiraniMapId = "map_052_amirani";

        public static readonly Vector2 SoslanOffset = Vector2.zero;
        public static readonly Vector2 KurganAbertoOffset = new Vector2(960f, 120f);
        public static readonly Vector2 FortimOffset = new Vector2(-480f, 1080f);
        public static readonly Vector2 ArvoredoOffset = new Vector2(-960f, 1560f);
        public static readonly Vector2 PocoDaTacaOffset = new Vector2(960f, 1080f);
        public static readonly Vector2 TorreDeVigiaOffset = new Vector2(0f, 1560f);

        /// <summary>Half the side (m) of the flat ground of the three towns of the steppe (reserved for their recipes, ±28 m and more).</summary>
        public const float NartesCityGround = 30f;

        /// <summary>The Rio dos Nartes: from the snows under the chain, past the svan towers and the camp, to its lake in the south.</summary>
        public static readonly Vector2[] NartesRiver =
        {
            new Vector2(280f, 1780f), new Vector2(230f, 1500f), new Vector2(150f, 1260f), new Vector2(80f, 1000f), new Vector2(140f, 760f),
            new Vector2(230f, 520f), new Vector2(210f, 300f), new Vector2(150f, 90f), new Vector2(150f, -120f), new Vector2(190f, -330f),
        };

        /// <summary>
        /// Amirani's chain (52) in the region's own coordinates: from its foot in the crags up the Caucasus to the snow crest
        /// under Ohya's summit (the chunk builder lays its links along it).
        /// </summary>
        public static readonly Vector2[] AmiraniChain =
        {
            new Vector2(470f, 1340f), new Vector2(600f, 1460f), new Vector2(760f, 1580f), new Vector2(940f, 1700f), new Vector2(1100f, 1780f),
        };

        private static class Nart
        {
            public const string Wolf = PeriodSpawnTable.ForestWolf;
            public const string Eagle = "enemy_mountain_crested_hawk";
            public const string Griffin = "enemy_desert_ember_vulture";
            public const string YoungGriffin = "enemy_mountain_young_griffin";
            public const string Saiga = "enemy_mountain_saiga";
            public const string Deer = "enemy_desert_jackal";
            public const string Swan = "enemy_forest_thief_gull";
            public const string BronzeWolf = "enemy_mountain_bronze_wolf";
            public const string Giant = "enemy_mountain_sleepwalker_giant";
            public const string Boar = NightVariantRules.PeccaryEnemyId;
            public const string Crow = "enemy_cave_dark_crow";
            public const string Beetle = "enemy_mountain_bark_beetle";
            public const string Fungus = "enemy_cave_ooze";
            public const string Storm = "enemy_mountain_ill_wind_mist";
            public const string Ochokochi = "enemy_mountain_ochokochi";
            public const string Bear = "enemy_mountain_brown_bear";
            public const string Stone = "enemy_mountain_rolling_stone";
            public const string Scorpion = "enemy_desert_scorpion";
            public const string Spider = "enemy_forest_spider";
            public const string Bats = PeriodSpawnTable.CaveBats;
            public const string Kite = "enemy_desert_broken_lantern";
            public const string Potoo = NightVariantRules.PotooEnemyId;
        }

        private static OverworldLayout NartesRegion()
        {
            OverworldLayout w = NewRegion(NartesId, MapRegion.EstepeDosNartes, Rect.MinMaxRect(-1200f, -600f, 1200f, 1800f));
            w.World.Biome = BiomeType.Mountain;
            const string soslan = SoslanMapId;
            const string kurgans = KurgansMapId;
            const string fortim = FortimMapId;
            const string arvoredo = HaripushpaMapId;
            const string cavalos = MilCavalosMapId;
            const string amirani = AmiraniMapId;
            const string plain = MapRegionTypes.Plain;
            const string forest = MapRegionTypes.Forest;
            const string rock = MapRegionTypes.Mountain;
            const string crampons = MapRequirementTags.ClimbingCrampons;

            // ---------------- Zones (a 5 × 5 grid; columns x −1200/−720/−240/240/720/1200, rows z −600/−120/360/840/1320/1800) ----------------
            // South row (towards the Costa): the windy steppe, the saigas' wormwood, the wide ford (47), the race tracks (47), the salt lake.
            OverworldZone zVentos = Zone(w, "estepe_dos_ventos", null, soslan, -1200f, -600f, -720f, -120f, OverworldTerrain.FeatherSteppe, 107, 110, 105,
                "Steppe of the Winds", "Estepe dos Ventos", plain, spacing: 90f);
            OverworldZone zSaigas = Zone(w, "estepe_das_saigas", null, soslan, -720f, -600f, -240f, -120f, OverworldTerrain.WormwoodSteppe, 101, 104, 99,
                "Saiga Steppe", "Estepe das Saigas", plain, spacing: 85f);
            OverworldZone zVau = Zone(w, "vau_largo", soslan, null, -240f, -600f, 240f, -120f, OverworldTerrain.SteppeRiver, 100, 103, 98,
                "Wide Ford", "Vau Largo", plain, spacing: 85f);
            OverworldZone zPistas = Zone(w, "pistas_de_corrida", soslan, null, 240f, -600f, 720f, -120f, OverworldTerrain.FeatherSteppe, 101, 105, 99,
                "Race Tracks", "Pistas de Corrida", plain, spacing: 85f);
            OverworldZone zSalgado = Zone(w, "lago_salgado", null, kurgans, 720f, -600f, 1200f, -120f, OverworldTerrain.WormwoodSteppe, 107, 111, 105,
                "Salt Lake", "Lago Salgado", plain, spacing: 90f);
            // Second row: the birch wood by Mingcheng, the steppe of Soslan, the camp (47), the golden kurgans and the open kurgan (48).
            OverworldZone zBetulas = Zone(w, "bosque_de_betulas", null, soslan, -1200f, -120f, -720f, 360f, OverworldTerrain.BirchSteppe, 107, 110, 105,
                "Birch Wood", "Bosque de Bétulas", forest, spacing: 90f);
            OverworldZone zEstepe = Zone(w, "estepe_de_soslan", null, soslan, -720f, -120f, -240f, 360f, OverworldTerrain.FeatherSteppe, 101, 104, 99,
                "Steppe of Soslan", "Estepe de Soslan", plain, spacing: 85f);
            OverworldZone zAcampamento = Zone(w, "acampamento_de_soslan", soslan, null, -240f, -120f, 240f, 360f, OverworldTerrain.HorseCamp, 100, 103, 98,
                "Soslan's Camp", "Acampamento de Soslan", plain, spacing: 80f);
            OverworldZone zKurgans = Zone(w, "kurgans_de_ouro", kurgans, null, 240f, -120f, 720f, 360f, OverworldTerrain.KurganField, 106, 110, 104,
                "Golden Kurgans", "Kurgans de Ouro", plain, spacing: 90f);
            OverworldZone zAberto = Zone(w, "kurgan_aberto", kurgans, null, 720f, -120f, 1200f, 360f, OverworldTerrain.KurganField, 108, 112, 106,
                "Open Kurgan", "Kurgan Aberto", plain, spacing: 90f);
            // Middle row: the dream tablets (49), the giants' pastures, the river, the poppies, the eastern kurgans.
            OverworldZone zTabuas = Zone(w, "vale_das_tabuas_de_sonho", fortim, null, -1200f, 360f, -720f, 840f, OverworldTerrain.DreamVale, 112, 116, 110,
                "Vale of the Dream Tablets", "Vale das Tábuas de Sonho", forest, spacing: 90f);
            OverworldZone zPastos = Zone(w, "pastos_dos_gigantes", null, fortim, -720f, 360f, -240f, 840f, OverworldTerrain.GiantHills, 105, 109, 103,
                "Giants' Pastures", "Pastos dos Gigantes", forest, spacing: 90f);
            OverworldZone zRio = Zone(w, "rio_dos_nartes", null, soslan, -240f, 360f, 240f, 840f, OverworldTerrain.SteppeRiver, 104, 107, 102,
                "River of the Narts", "Rio dos Nartes", plain, spacing: 85f);
            OverworldZone zPapoula = Zone(w, "campos_de_papoula", null, cavalos, 240f, 360f, 720f, 840f, OverworldTerrain.PoppyPrairie, 105, 109, 103,
                "Poppy Fields", "Campos de Papoula", plain, spacing: 90f);
            OverworldZone zLeste = Zone(w, "kurgans_do_leste", null, kurgans, 720f, 360f, 1200f, 840f, OverworldTerrain.KurganField, 110, 114, 108,
                "Eastern Kurgans", "Kurgans do Leste", plain, spacing: 90f);
            // Fourth row: the larch taiga (50), the hill giants' fort (49), the svan valley, the rolling prairie, the thousand horses (51).
            OverworldZone zTaiga = Zone(w, "taiga_de_laricos", null, arvoredo, -1200f, 840f, -720f, 1320f, OverworldTerrain.Taiga, 118, 122, 116,
                "Larch Taiga", "Taiga de Lariços", forest, spacing: 95f);
            OverworldZone zFortim = Zone(w, "fortim_dos_gigantes", fortim, null, -720f, 840f, -240f, 1320f, OverworldTerrain.GiantHills, 113, 117, 111,
                "Hill-Giant Fort", "Fortim dos Gigantes", forest, spacing: 90f);
            OverworldZone zSvan = Zone(w, "vale_das_torres_svan", null, amirani, -240f, 840f, 240f, 1320f, OverworldTerrain.SvanValley, 112, 116, 110,
                "Valley of the Svan Towers", "Vale das Torres Svan", rock, spacing: 95f);
            OverworldZone zOndulada = Zone(w, "pradaria_ondulada", null, cavalos, 240f, 840f, 720f, 1320f, OverworldTerrain.PoppyPrairie, 114, 118, 112,
                "Rolling Prairie", "Pradaria Ondulada", plain, spacing: 95f);
            OverworldZone zCavalos = Zone(w, "pradaria_dos_mil_cavalos", cavalos, null, 720f, 840f, 1200f, 1320f, OverworldTerrain.HorsePlains, 124, 130, 122,
                "Thousand-Horse Prairie", "Pradaria dos Mil Cavalos", plain, spacing: 95f);
            // North row: the trunk folk's grove and the meltwater lake (50), the Caucasus of Amirani's chain (52: Climbing Crampons).
            OverworldZone zArvoredo = Zone(w, "arvoredo_de_haripushpa", arvoredo, null, -1200f, 1320f, -720f, 1800f, OverworldTerrain.Taiga, 124, 126, 122,
                "Haripushpa Grove", "Arvoredo de Haripushpa", forest, spacing: 95f);
            OverworldZone zDegelo = Zone(w, "lago_de_degelo", arvoredo, null, -720f, 1320f, -240f, 1800f, OverworldTerrain.Taiga, 120, 126, 118,
                "Meltwater Lake", "Lago de Degelo", forest, spacing: 95f);
            OverworldZone zVigia = Zone(w, "torre_de_vigia", amirani, null, -240f, 1320f, 240f, 1800f, OverworldTerrain.SvanValley, 134, 140, 132,
                "Watch Tower", "Torre de Vigia", rock, crampons, 100f);
            OverworldZone zCorrente = Zone(w, "corrente_de_amirani", amirani, null, 240f, 1320f, 720f, 1800f, OverworldTerrain.ChainPeak, 138, 146, 136,
                "Amirani's Chain", "Corrente de Amirani", rock, crampons, 100f);
            OverworldZone zOhya = Zone(w, "cume_de_ohya", amirani, null, 720f, 1320f, 1200f, 1800f, OverworldTerrain.SnowCrest, 142, 150, 140,
                "Ohya's Crest", "Cume de Ohya", rock, crampons, 100f);

            AddNartesFauna(zVentos, zSaigas, zVau, zPistas, zSalgado, zBetulas, zEstepe, zAcampamento, zKurgans, zAberto, zTabuas, zPastos, zRio, zPapoula,
                zLeste, zTaiga, zFortim, zSvan, zOndulada, zCavalos, zArvoredo, zDegelo, zVigia, zCorrente, zOhya);

            // ---------------- Settlements ----------------
            // The Acampamento de Soslan first: the region's return city and the barca's stop for 47 (flat ground at y = 0, ±30 m;
            // roads leave W, E and S). Then the hill giants' Fortim (49) and the trunk folk's Arvoredo (50), return cities of their
            // maps, and the posts of 48, 51 and 52.
            w.Vila = Place(w, soslan, SoslanRecipe(), SoslanOffset, Rect.MinMaxRect(-26f, -26f, 26f, 26f), NartesCityGround, true);
            Place(w, fortim, FortimRecipe(), FortimOffset, Rect.MinMaxRect(-26f, -26f, 26f, 26f), NartesCityGround, true);
            Place(w, arvoredo, ArvoredoRecipe(), ArvoredoOffset, Rect.MinMaxRect(-26f, -26f, 26f, 26f), NartesCityGround, true);
            Place(w, kurgans, Post("kurgan_aberto", TownSwatch.Loam, TownSwatch.Thatch, TownRoofKind.Cone, TownShopDressing.Storyteller, false),
                KurganAbertoOffset, PostSafe, 18f, false);
            Place(w, cavalos, Post("poco_da_taca", TownSwatch.Stone, TownSwatch.Thatch, TownRoofKind.Cone, TownShopDressing.Herder, false),
                PocoDaTacaOffset, PostSafe, 18f, false);
            Place(w, amirani, Post("torre_de_vigia", TownSwatch.Granite, TownSwatch.Slate, TownRoofKind.Gable, TownShopDressing.Mercearia, false),
                TorreDeVigiaOffset, PostSafe, 18f, false);

            // ---------------- Water ----------------
            // The Rio dos Nartes (12 m, carved but shallow): the two wide fords of the camp's roads (the Vau Largo), a stone bridge
            // under the Torre de Vigia.
            MapRiver river = River(w, 12f, NartesRiver);
            Vector2 campFord = On(NartesRiver[7], NartesRiver[8], 0.38f);
            Crossing(river, campFord, 9f, false);
            Vector2 poppyFord = On(NartesRiver[5], NartesRiver[6], 0.59f);
            Crossing(river, poppyFord, 9f, false);
            Vector2 towerBridge = On(NartesRiver[0], NartesRiver[1], 0.786f);
            Crossing(river, towerBridge, 4.5f, true);
            // Its lake south of the camp, the Lago de Degelo in the taiga, the salt lake, a pond of the wild herds.
            River(w, 56f, new Vector2(190f, -330f), new Vector2(270f, -400f));
            River(w, 80f, new Vector2(-600f, 1580f), new Vector2(-460f, 1610f));
            River(w, 60f, new Vector2(960f, -380f), new Vector2(1060f, -400f));
            River(w, 24f, new Vector2(1100f, 960f), new Vector2(1130f, 975f));

            // ---------------- Gates out of the region ----------------
            // South: three roads down to the Costa das Monções (Aratta's savanna, Meroë's court, Husuni's cliffs); west: three
            // passes to the Planície de Mingcheng (the loess track, the Torres Brancas' mirror hills, the nian's peaks).
            Gate(w, "exit_auroques", MapExitKind.Map, AuroquesId, Rect.MinMaxRect(-908f, -600f, -892f, -597f), new Vector2(-900f, -591f), Vector2.up);
            Gate(w, "exit_meroe", MapExitKind.Map, MeroeId, Rect.MinMaxRect(142f, -600f, 158f, -597f), new Vector2(150f, -591f), Vector2.up);
            Gate(w, "exit_husuni", MapExitKind.Map, KilwaId, Rect.MinMaxRect(892f, -600f, 908f, -597f), new Vector2(900f, -591f), Vector2.up);
            Gate(w, "exit_loess", MapExitKind.Map, MingchengMapId, Rect.MinMaxRect(-1200f, -158f, -1197f, -142f), new Vector2(-1191f, -150f), Vector2.right);
            Gate(w, "exit_torres", MapExitKind.Map, TorresBrancasMapId, Rect.MinMaxRect(-1200f, 692f, -1197f, 708f), new Vector2(-1191f, 700f), Vector2.right);
            Gate(w, "exit_nian", MapExitKind.Map, TorresBrancasMapId, Rect.MinMaxRect(-1200f, 1412f, -1197f, 1428f), new Vector2(-1191f, 1420f), Vector2.right);
            Gate(w, "exit_island", MapExitKind.WorldMap, null, Rect.MinMaxRect(-82f, 78f, -78f, 82f), new Vector2(-80f, 70f), Vector2.up);
            Landmark(w, MapLandmarkKind.StoneRing, -80f, 80f, 0f, 0.45f);

            // ---------------- Trails ----------------
            float g = NartesCityGround;
            // The south road from the camp over the steppe to Meroë's court; the south-west road to Aratta's savanna.
            Vector2 southFork = new Vector2(-20f, -200f);
            PathVia(w, 4f, new Vector2(0f, -g), southFork, new Vector2(40f, -420f), new Vector2(150f, -591f));
            PathVia(w, 3.5f, southFork, new Vector2(-300f, -260f), new Vector2(-560f, -380f), new Vector2(-780f, -480f), new Vector2(-900f, -591f));
            // The west road over the steppe of Soslan to the loess pass; the saigas' loop.
            Vector2 westFork = new Vector2(-260f, 20f);
            Vector2 birchFork = new Vector2(-780f, -60f);
            PathVia(w, 4f, new Vector2(-g, 0f), westFork, new Vector2(-520f, 0f), birchFork, new Vector2(-1000f, -130f), new Vector2(-1191f, -150f));
            PathVia(w, 3.5f, westFork, new Vector2(-380f, -200f), new Vector2(-560f, -380f));
            // The east road through the wide ford to the kurgans and the Kurgan Aberto; south-east past the race tracks to Husuni.
            Vector2 eastFork = new Vector2(430f, 70f);
            PathVia(w, 4f, new Vector2(g, 0f), new Vector2(90f, 8f), campFord, new Vector2(300f, 40f), eastFork, new Vector2(620f, 110f),
                KurganAbertoOffset + new Vector2(-18f, 0f));
            PathVia(w, 3.5f, eastFork, new Vector2(560f, -150f), new Vector2(720f, -380f), new Vector2(900f, -591f));
            // The north road (from the east road: the camp's north side is the river's): up the river's west bank to the svan valley
            // and the Torre de Vigia.
            Vector2 northFork = new Vector2(60f, 380f);
            Vector2 svanFork = new Vector2(-20f, 1100f);
            PathVia(w, 4f, On(new Vector2(g, 0f), new Vector2(90f, 8f), 0.5f), new Vector2(50f, 180f), northFork, new Vector2(40f, 600f), new Vector2(0f, 840f), svanFork,
                new Vector2(-10f, 1350f), new Vector2(80f, 1460f), TorreDeVigiaOffset + new Vector2(18f, -10f));
            // The poppy road: over the second ford to the poppy fields, the rolling prairie and the Poço da Taça.
            Vector2 poppyFork = new Vector2(500f, 600f);
            PathVia(w, 3.5f, northFork, poppyFord, new Vector2(340f, 420f), poppyFork, new Vector2(700f, 800f), PocoDaTacaOffset + new Vector2(-18f, 0f));
            PathVia(w, 3.5f, eastFork, new Vector2(480f, 300f), poppyFork);
            // From the Kurgan Aberto north to the Poço da Taça; from the well up to Ohya's crest.
            PathVia(w, 3.5f, KurganAbertoOffset + new Vector2(0f, 18f), new Vector2(990f, 400f), new Vector2(960f, 700f), PocoDaTacaOffset + new Vector2(0f, -18f));
            PathVia(w, 3.5f, PocoDaTacaOffset + new Vector2(18f, 0f), new Vector2(1080f, 1250f), new Vector2(1080f, 1500f), new Vector2(1000f, 1680f));
            // The giants' road: from the west road and the north road up the pastures to the Fortim.
            Vector2 giantsFork = new Vector2(-320f, 560f);
            PathVia(w, 3.5f, westFork, new Vector2(-300f, 300f), giantsFork, new Vector2(-420f, 820f), FortimOffset + new Vector2(0f, -g));
            PathVia(w, 3.5f, northFork, new Vector2(-120f, 460f), giantsFork);
            // From the Fortim west to the dream tablets and the Torres pass; down to the birch wood.
            Vector2 dreamFork = new Vector2(-860f, 780f);
            Vector2 taigaFork = new Vector2(-820f, 1180f);
            Vector2 fortimWest = new Vector2(-700f, 980f);
            PathVia(w, 3.5f, FortimOffset + new Vector2(-g, 0f), fortimWest, dreamFork, new Vector2(-1000f, 720f), new Vector2(-1191f, 700f));
            PathVia(w, 3.5f, dreamFork, new Vector2(-820f, 520f), new Vector2(-760f, 260f), birchFork);
            // From the Fortim's south road east to the svan valley (the fort has no east street); north-west through the taiga to the
            // Arvoredo and the nian pass.
            PathVia(w, 3.5f, On(new Vector2(-420f, 820f), FortimOffset + new Vector2(0f, -g), 0.6f), new Vector2(-300f, 1100f), svanFork);
            PathVia(w, 3.5f, fortimWest, taigaFork, new Vector2(-930f, 1380f), ArvoredoOffset + new Vector2(0f, -g));
            PathVia(w, 3.5f, taigaFork, new Vector2(-1000f, 1300f), new Vector2(-1191f, 1420f));
            // From the Arvoredo east past the meltwater lake to the Torre de Vigia; over the bridge to the chain and Ohya's crest.
            PathVia(w, 3.5f, ArvoredoOffset + new Vector2(g, 0f), new Vector2(-760f, 1460f), new Vector2(-560f, 1420f), new Vector2(-340f, 1430f), new Vector2(-160f, 1500f),
                TorreDeVigiaOffset + new Vector2(-18f, 0f));
            Vector2 chainFork = new Vector2(380f, 1570f);
            PathVia(w, 3.5f, TorreDeVigiaOffset + new Vector2(18f, 0f), new Vector2(120f, 1560f), towerBridge, chainFork, new Vector2(600f, 1620f), new Vector2(820f, 1660f),
                new Vector2(1000f, 1680f));

            // ---------------- Landmarks ----------------
            Sign(w, southFork, new Vector2(-300f, -260f), 4f, 0f);
            Sign(w, westFork, new Vector2(-380f, -200f), 4f, 0f);
            Sign(w, birchFork, new Vector2(-760f, 260f), 4f, 0f);
            Sign(w, eastFork, new Vector2(560f, -150f), 4f, 0f);
            Sign(w, northFork, new Vector2(-120f, 460f), 4f, 0f);
            Sign(w, poppyFork, new Vector2(700f, 800f), 3.5f, 0f);
            Sign(w, giantsFork, new Vector2(-420f, 820f), 3.5f, 0f);
            Sign(w, dreamFork, new Vector2(-1000f, 720f), 3.5f, 0f);
            Sign(w, taigaFork, new Vector2(-1000f, 1300f), 3.5f, 0f);
            Sign(w, svanFork, new Vector2(-280f, 1140f), 4f, 0f);
            Sign(w, chainFork, new Vector2(600f, 1620f), 3.5f, 0f);
            Landmark(w, MapLandmarkKind.Campfire, -560f, -300f, 0f, 0.9f);
            Landmark(w, MapLandmarkKind.Campfire, 560f, -300f, 0f, 0.9f);
            Landmark(w, MapLandmarkKind.Campfire, -400f, 640f, 0f, 1f);
            Landmark(w, MapLandmarkKind.Campfire, 640f, 700f, 0f, 1f);
            Landmark(w, MapLandmarkKind.Campfire, -880f, 1240f, 0f, 1f);
            Landmark(w, MapLandmarkKind.Campfire, 60f, 1220f, 0f, 1f);
            Landmark(w, MapLandmarkKind.StoneRing, 560f, 220f, 0f, 1f);
            Landmark(w, MapLandmarkKind.StoneRing, -960f, 600f, 0f, 0.9f);

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
        // The three towns of the steppe (their own recipes, Cities/Town: the SteppeCamp, HillFort and LivingGrove styles, each
        // inside a radius of 22.5 m on its reserved flat ground of ±30 m at y = 0; no backdrop: the steppe is all round them).
        // The camp's and the grove's streets leave W, E and S; the Fortim's W and S.
        // ------------------------------------------------------------------

        private static CityTownRecipe SoslanRecipe() => NoBackdrop(CityTownRecipes.Soslan());

        private static CityTownRecipe FortimRecipe() => NoBackdrop(CityTownRecipes.Fortim());

        private static CityTownRecipe ArvoredoRecipe() => NoBackdrop(CityTownRecipes.Arvoredo());

        private static CityTownRecipe NoBackdrop(CityTownRecipe recipe)
        {
            recipe.Backdrop = false;
            return recipe;
        }

        // ------------------------------------------------------------------
        // Monster groups per area (existing definitions only, no boss; Mapas100 47–52 archetypes)
        // ------------------------------------------------------------------

        private static void AddNartesFauna(OverworldZone ventos, OverworldZone saigas, OverworldZone vau, OverworldZone pistas, OverworldZone salgado,
            OverworldZone betulas, OverworldZone estepe, OverworldZone acampamento, OverworldZone kurgans, OverworldZone aberto, OverworldZone tabuas,
            OverworldZone pastos, OverworldZone rio, OverworldZone papoula, OverworldZone leste, OverworldZone taiga, OverworldZone fortim, OverworldZone svan,
            OverworldZone ondulada, OverworldZone cavalos, OverworldZone arvoredo, OverworldZone degelo, OverworldZone vigia, OverworldZone corrente,
            OverworldZone ohya)
        {
            // At night: the steppe wolves run in packs as werewolves, the shroud owl over the grass, the vampire bats in the kurgans
            // and the towers, the moon-mother by the water, the boars' night herd (the queixada), fireflies over the poppies.
            NightSpecies werewolf = NightVariantRules.Lobisomem;
            NightSpecies owl = NightVariantRules.RasgaMortalha;
            NightSpecies bats = NightVariantRules.MorcegoVampiro;
            NightSpecies moon = NightVariantRules.MaeDaLua;
            NightSpecies herd = NightVariantRules.Queixada;
            NightSpecies fireflies = NightVariantRules.VagaLume;

            // Acampamento de Soslan, Vau Largo, Pistas de Corrida (47): the Lobo da Estepe Apagado [rápido] (the wolf), the Grifo
            // Jovem [elite], the Águia-Dourada Apagada [distância] (the hawk), the Saiga Assustada [enxame].
            acampamento.Fauna.Add(Fauna("saigas_assustadas", "[enxame]", T(N(Nart.Saiga, 3), N(Nart.Eagle, 1)), T(V(Nart.Saiga, 2), S(owl, 1)), 5));
            acampamento.Fauna.Add(Fauna("aguias_douradas", "[distância]", T(N(Nart.Eagle, 3), N(Nart.Saiga, 1)), T(V(Nart.Eagle, 2), S(moon, 1)), 3));
            vau.Fauna.Add(Fauna("cisnes_do_vau", "[distância]", T(N(Nart.Swan, 3), N(Nart.Saiga, 1)), T(V(Nart.Swan, 2), S(moon, 1)), 4));
            vau.Fauna.Add(Fauna("lobos_do_vau", "[rápido]", T(N(Nart.Wolf, 3), N(Nart.Swan, 1)), T(V(Nart.Wolf, 2), S(werewolf, 1)), 4));
            pistas.Fauna.Add(Fauna("lobos_da_estepe", "[rápido]", T(N(Nart.Wolf, 3), N(Nart.Saiga, 1)), T(V(Nart.Wolf, 3), S(werewolf, 1)), 4));
            pistas.Fauna.Add(Fauna("grifos_jovens", "[elite]", T(N(Nart.YoungGriffin, 3), N(Nart.Eagle, 1)), T(V(Nart.YoungGriffin, 2), S(owl, 1)), 3, null, 32f));

            // Estepe de Soslan and Estepe dos Ventos: wolves, saigas, golden eagles; Estepe das Saigas and the salt lake: the herds
            // in the wormwood, scorpions in the salt.
            estepe.Fauna.Add(Fauna("saigas_da_estepe", "[enxame]", T(N(Nart.Saiga, 3), N(Nart.Wolf, 1)), T(V(Nart.Saiga, 2), S(owl, 1)), 5));
            estepe.Fauna.Add(Fauna("lobos_de_soslan", "[rápido]", T(N(Nart.Wolf, 3), N(Nart.Eagle, 1)), T(V(Nart.Wolf, 2), S(werewolf, 1)), 4));
            ventos.Fauna.Add(Fauna("aguias_dos_ventos", "[distância]", T(N(Nart.Eagle, 3), N(Nart.Wolf, 1)), T(V(Nart.Eagle, 2), S(owl, 1)), 3));
            ventos.Fauna.Add(Fauna("tempestades_dos_ventos", "[controle]", T(N(Nart.Storm, 3), N(Nart.Saiga, 1)), T(V(Nart.Storm, 2), S(moon, 1)), 4));
            saigas.Fauna.Add(Fauna("manadas_de_saigas", "[enxame]", T(N(Nart.Saiga, 3), N(Nart.Griffin, 1)), T(V(Nart.Saiga, 2), S(owl, 1)), 5));
            saigas.Fauna.Add(Fauna("abutres_do_absinto", "[distância]", T(N(Nart.Griffin, 3), N(Nart.Saiga, 1)), T(V(Nart.Griffin, 2), V(Nart.Saiga, 1)), 3));
            salgado.Fauna.Add(Fauna("escorpioes_do_sal", "[controle]", T(N(Nart.Scorpion, 3), N(Nart.Griffin, 1)), T(V(Nart.Scorpion, 2), S(owl, 1)), 4));
            salgado.Fauna.Add(Fauna("cisnes_do_lago_salgado", "[distância]", T(N(Nart.Swan, 3), N(Nart.Scorpion, 1)), T(V(Nart.Swan, 2), S(moon, 1)), 4));

            // Kurgans de Ouro, Kurgan Aberto, Kurgans do Leste (48): the Grifo Guardião [elite] (a young griffin), the Cervo de Ouro
            // Apagado [rápido] (the jackal), the Cisne de Feltro [distância] (the thief gull), the Lobo de Bronze [tanque].
            kurgans.Fauna.Add(Fauna("grifos_guardioes", "[elite]", T(N(Nart.YoungGriffin, 3), N(Nart.BronzeWolf, 1)), T(V(Nart.YoungGriffin, 2), S(bats, 1)), 3, null, 32f));
            kurgans.Fauna.Add(Fauna("cervos_de_ouro", "[rápido]", T(N(Nart.Deer, 3), N(Nart.Swan, 1)), T(V(Nart.Deer, 2), S(moon, 1)), 4));
            aberto.Fauna.Add(Fauna("lobos_de_bronze", "[tanque]", T(N(Nart.BronzeWolf, 3), N(Nart.YoungGriffin, 1)), T(V(Nart.BronzeWolf, 2), S(bats, 1)), 3, null, 30f));
            aberto.Fauna.Add(Fauna("cisnes_de_feltro", "[distância]", T(N(Nart.Swan, 3), N(Nart.Kite, 1)), T(V(Nart.Swan, 2), V(Nart.Kite, 1)), 4));
            leste.Fauna.Add(Fauna("lobos_de_bronze_do_leste", "[tanque]", T(N(Nart.BronzeWolf, 3), N(Nart.Saiga, 1)), T(V(Nart.BronzeWolf, 2), S(bats, 1)), 3, null, 30f));
            leste.Fauna.Add(Fauna("morcegos_dos_kurgans", "[enxame]", T(N(Nart.Bats, 3), N(Nart.Griffin, 1)), T(V(Nart.Bats, 2), S(bats, 2)), 5));

            // Rio dos Nartes: felt swans on the shingle, wolves at the fords, the river's mist.
            rio.Fauna.Add(Fauna("cisnes_do_rio", "[distância]", T(N(Nart.Swan, 3), N(Nart.Wolf, 1)), T(V(Nart.Swan, 2), S(moon, 1)), 4));
            rio.Fauna.Add(Fauna("neblinas_do_rio", "[controle]", T(N(Nart.Storm, 3), N(Nart.Swan, 1)), T(V(Nart.Storm, 2), S(owl, 1)), 4));

            // Bosque de Bétulas: spiders in the birches, wolves, the potoo.
            betulas.Fauna.Add(Fauna("aranhas_das_betulas", "[controle]", T(N(Nart.Spider, 3), N(Nart.Potoo, 1)), T(V(Nart.Spider, 2), S(moon, 1)), 4));
            betulas.Fauna.Add(Fauna("lobos_das_betulas", "[rápido]", T(N(Nart.Wolf, 3), N(Nart.Spider, 1)), T(V(Nart.Wolf, 2), S(werewolf, 1)), 4));

            // Pastos dos Gigantes, Fortim dos Gigantes, Vale das Tábuas (49): the Gigante Sonâmbulo [tanque], the
            // Cabra-Montês Gigante [rápido] (the jackal), the Javali da Colina [tanque] (the peccary), the Corvo-Mensageiro de Fumaça
            // [distância] (the crow); at night the sleepwalkers double.
            pastos.Fauna.Add(Fauna("javalis_da_colina", "[tanque]", T(N(Nart.Boar, 3), N(Nart.Saiga, 1)), T(S(herd, 2), V(Nart.Saiga, 1)), 3, null, 28f));
            pastos.Fauna.Add(Fauna("cabras_gigantes", "[rápido]", T(N(Nart.Deer, 3), N(Nart.Boar, 1)), T(V(Nart.Deer, 2), S(owl, 1)), 4));
            fortim.Fauna.Add(Fauna("gigantes_sonambulos", "[tanque]", T(N(Nart.Giant, 3), N(Nart.Crow, 1)), T(V(Nart.Giant, 3), S(werewolf, 1)), 3, null, 30f));
            fortim.Fauna.Add(Fauna("corvos_mensageiros", "[distância]", T(N(Nart.Crow, 3), N(Nart.Giant, 1)), T(V(Nart.Crow, 2), S(owl, 1)), 4));
            fortim.Fauna.Add(Fauna("javalis_do_fortim", "[tanque]", T(N(Nart.Boar, 3), N(Nart.Crow, 1)), T(S(herd, 2), V(Nart.Crow, 1)), 3, null, 28f));
            tabuas.Fauna.Add(Fauna("sonambulos_das_tabuas", "[tanque]", T(N(Nart.Giant, 3), N(Nart.Storm, 1)), T(V(Nart.Giant, 3), S(owl, 1)), 3, null, 30f));
            tabuas.Fauna.Add(Fauna("pesadelos_de_fumaca", "[controle]", T(N(Nart.Storm, 3), N(Nart.Crow, 1)), T(V(Nart.Storm, 2), S(moon, 1)), 4));

            // Taiga de Lariços, Arvoredo de Haripushpa, Lago de Degelo (50): the Besouro-Casca [tanque], the Pica-Pau
            // de Fumaça [distância] (the hawk), the Fungo Apagado [controle] (the ooze), the Lince da Taiga [rápido] (the wolf).
            taiga.Fauna.Add(Fauna("besouros_casca", "[tanque]", T(N(Nart.Beetle, 3), N(Nart.Fungus, 1)), T(V(Nart.Beetle, 2), S(owl, 1)), 3, null, 30f));
            taiga.Fauna.Add(Fauna("linces_da_taiga", "[rápido]", T(N(Nart.Wolf, 3), N(Nart.Beetle, 1)), T(V(Nart.Wolf, 2), S(werewolf, 1)), 4));
            arvoredo.Fauna.Add(Fauna("fungos_apagados", "[controle]", T(N(Nart.Fungus, 3), N(Nart.Beetle, 1)), T(V(Nart.Fungus, 2), S(moon, 1)), 4));
            arvoredo.Fauna.Add(Fauna("pica_paus_de_fumaca", "[distância]", T(N(Nart.Eagle, 3), N(Nart.Fungus, 1)), T(V(Nart.Eagle, 2), S(owl, 1)), 3));
            degelo.Fauna.Add(Fauna("besouros_do_degelo", "[tanque]", T(N(Nart.Beetle, 3), N(Nart.Swan, 1)), T(V(Nart.Beetle, 2), S(moon, 1)), 3, null, 30f));
            degelo.Fauna.Add(Fauna("linces_do_lago", "[rápido]", T(N(Nart.Wolf, 3), N(Nart.Fungus, 1)), T(V(Nart.Wolf, 2), S(werewolf, 1)), 4));

            // Campos de Papoula, Pradaria Ondulada, Pradaria dos Mil Cavalos (51): the Garanhão Apagado [rápido] (the wolf's body), the
            // Lobo-Cinzento [enxame] (wolves), the Tempestade de Capim [controle] (the ill-wind mist), the Abutre da Estepe [distância]
            // (the ember vulture); fireflies over the poppies at night.
            papoula.Fauna.Add(Fauna("tempestades_de_capim", "[controle]", T(N(Nart.Storm, 3), N(Nart.Wolf, 1)), T(V(Nart.Storm, 2), S(fireflies, 1)), 4));
            papoula.Fauna.Add(Fauna("abutres_da_papoula", "[distância]", T(N(Nart.Griffin, 3), N(Nart.Saiga, 1)), T(V(Nart.Griffin, 2), S(fireflies, 1)), 3));
            ondulada.Fauna.Add(Fauna("lobos_cinzentos", "[enxame]", T(N(Nart.Wolf, 3), N(Nart.Storm, 1)), T(V(Nart.Wolf, 3), S(werewolf, 1)), 5));
            ondulada.Fauna.Add(Fauna("abutres_da_estepe", "[distância]", T(N(Nart.Griffin, 3), N(Nart.Wolf, 1)), T(V(Nart.Griffin, 2), S(owl, 1)), 3));
            cavalos.Fauna.Add(Fauna("garanhoes_apagados", "[rápido]", T(N(Nart.Wolf, 3), N(Nart.Griffin, 1)), T(V(Nart.Wolf, 3), S(werewolf, 1)), 4));
            cavalos.Fauna.Add(Fauna("tempestades_da_pradaria", "[controle]", T(N(Nart.Storm, 3), N(Nart.Wolf, 1)), T(V(Nart.Storm, 2), S(fireflies, 1)), 4));
            cavalos.Fauna.Add(Fauna("abutres_dos_cavalos", "[distância]", T(N(Nart.Griffin, 3), N(Nart.Storm, 1)), T(V(Nart.Griffin, 2), S(owl, 1)), 3));

            // Vale das Torres Svan, Torre de Vigia, Corrente de Amirani, Cume de Ohya (52): the Ochokochi [elite], the Águia
            // de Ferro [distância] (the hawk), the Urso do Cáucaso Apagado [tanque] (the Pamir's brown bear; the
            // werewolf only at night), the Lobo de Neve [rápido] (the wolf); rolling stones off the crags.
            svan.Fauna.Add(Fauna("ochokochis_do_vale", "[elite]", T(N(Nart.Ochokochi, 3), N(Nart.Crow, 1)), T(V(Nart.Ochokochi, 3), S(werewolf, 1)), 3, null, 32f));
            svan.Fauna.Add(Fauna("corvos_das_torres", "[distância]", T(N(Nart.Crow, 3), N(Nart.Ochokochi, 1)), T(V(Nart.Crow, 2), S(bats, 1)), 4));
            vigia.Fauna.Add(Fauna("ursos_do_caucaso", "[tanque]", T(N(Nart.Bear, 3), N(Nart.Crow, 1)), T(V(Nart.Bear, 2), S(werewolf, 1)), 3, null, 30f));
            vigia.Fauna.Add(Fauna("aguias_de_ferro", "[distância]", T(N(Nart.Eagle, 3), N(Nart.Ochokochi, 1)), T(V(Nart.Eagle, 2), S(bats, 1)), 3));
            corrente.Fauna.Add(Fauna("ochokochis_da_corrente", "[elite]", T(N(Nart.Ochokochi, 3), N(Nart.Stone, 1)), T(V(Nart.Ochokochi, 3), S(werewolf, 1)), 3, null, 32f));
            corrente.Fauna.Add(Fauna("pedras_da_corrente", "[controle]", T(N(Nart.Stone, 3), N(Nart.Eagle, 1)), T(V(Nart.Stone, 2), S(bats, 1)), 3, null, 28f));
            ohya.Fauna.Add(Fauna("lobos_de_neve", "[rápido]", T(N(Nart.Wolf, 3), N(Nart.Bear, 1)), T(V(Nart.Wolf, 3), S(werewolf, 1)), 4));
            ohya.Fauna.Add(Fauna("aguias_do_cume", "[distância]", T(N(Nart.Eagle, 3), N(Nart.Stone, 1)), T(V(Nart.Eagle, 2), S(owl, 1)), 3));
        }
    }
}
