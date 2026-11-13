using Rapadura.Gameplay.Night;
using Rapadura.Gameplay.World.Biomes;
using Rapadura.Gameplay.World.Cities.Town;
using Rapadura.Gameplay.World.Map.Areas;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Overworld
{
    /// <summary>
    /// Região 14 — Margens da Mistura as ONE continuous map of 2400 × 2400 m (Docs/Mapas100.md §0.10 and §5, maps 77–82), no scene change
    /// between the ziggurat and the Field of the Dream Tablets. Dur-Jarras (77, the capital, level 300) stands at the plaza (0, 0), 600 m from
    /// the west edge, where two passes come over from the Rio das Doze Horas (the Beira Fervente): in the stitched world the Margins fill the
    /// cell of the fifth row east of the river and north of the Mar Gelado. The bands rise eastwards, north and south, each map in its own column or
    /// row of the 5 × 5 grid:
    /// <list type="bullet">
    /// <item>round the city (nv 300–307, map 77): <b>Dur-Jarras</b>, the <b>Portão de Tijolo</b> (the way in), the <b>Olarias</b>, the
    /// <b>Colinas de Tell</b>, the <b>Canal das Palmeiras</b> and the <b>Terraços de Cevada</b>;</item>
    /// <item>east, the middle column (nv 306–316, map 78: a Waterskin): the <b>Deserto do Relógio</b> with the Gnômon Caído (post), the 12 gnomons in a
    /// circle, the <b>Sombras Atrasadas</b> and the <b>Areia das Horas</b>;</item>
    /// <item>the south row (nv 314–326, map 79: a Reed Boat): the <b>Poços de Betume</b> with the Balsa de Junco (post), the <b>Poças de Piche</b>,
    /// the <b>Juncal de Piche</b> and the canal of bitumen with three bridges;</item>
    /// <item>the north row (nv 322–336, map 80: a Climbing Rope): the <b>Cedral de Hobabish</b> with the Clareira do Lenhador (post), seven lanterns
    /// of resin, the <b>Beira do Cedral</b> and the <b>Cedros Anciãos</b>;</item>
    /// <item>the fourth column (nv 334–346, map 81: Climbing Crampons): the <b>Ombro de Upelluri</b> with the Ninho no Ombro (post), the
    /// <b>Dobras de Pedra</b>, the <b>Clavícula do Gigante</b>, the <b>Diorito Crescente</b> and the <b>Beiral do Céu</b>;</item>
    /// <item>the east column (nv 344–360, map 82: a lit lamp): the <b>Campo das Tábuas do Sonho</b> with the Tenda dos Intérpretes (post), the
    /// <b>Jardim Cortado</b>, the <b>Água Parada</b>, the <b>Tábuas Quebradas</b> and the <b>Tábuas do Sonho</b>.</item>
    /// </list>
    /// Water: the canal of bitumen across the south row (carved, three bridges), the ponds of pitch and the still water of the dream.
    /// Monsters: the region's own beasts and existing ones that fit the other sheets (see <see cref="Mistura"/>) and the night species, a night table in every clearing; no
    /// boss (O Instante Perdido, O Rei do Piche, Hobabish, Ullikummi and the Sonho da Destruição are not placed).
    /// </summary>
    public static partial class OverworldLayouts
    {
        public const string MisturaRegionId = "overworld_margens_da_mistura";

        public const string DurJarrasMapId = "map_077_dur_jarras";
        public const string DesertoRelogioMapId = "map_078_deserto_relogio";
        public const string PocosBetumeMapId = "map_079_pocos_betume";
        public const string CedralHobabishMapId = "map_080_cedral_hobabish";
        public const string OmbroUpelluriMapId = "map_081_ombro_upelluri";
        public const string TabuasSonhoMapId = "map_082_tabuas_sonho";

        /// <summary>O Sonho da Destruição's arena (82): the stone ring at the north of the Dream Tablets.</summary>
        public static readonly Vector2 SonhoDaDestruicaoArena = new Vector2(1640f, 980f);

        /// <summary>O Rei do Piche's arena (79, map boss): the stone ring among the Poços de Betume, by the trail, off the tar pools.</summary>
        public static readonly Vector2 ReiDoPicheArena = new Vector2(96f, -968f);

        /// <summary>O Instante Perdido's arena (78, map boss): the stone ring inside the dial of the twelve gnomons, west of the Gnômon Caído, by the east road, off the bubbles of time.</summary>
        public static readonly Vector2 InstantePerdidoArena = new Vector2(530f, 30f);

        /// <summary>Hobabish's arena (80, map boss): the stone ring inside the ring of resin lanterns, west of the Clareira do Lenhador, by the trail to the Beira do Cedral.</summary>
        public static readonly Vector2 HobabishArena = new Vector2(60f, 930f);

        /// <summary>Ullikummi's arena (81, map boss): the stone ring in the Dobras de Pedra, by the trail down the shoulder towards the growing diorite.</summary>
        public static readonly Vector2 UllikummiArena = new Vector2(1070f, -450f);

        /// <summary>The region's rectangle (region coordinates): where <see cref="Mechanics.MisturaMechanicsDriver"/> runs.</summary>
        public static readonly Rect MisturaBounds = Rect.MinMaxRect(-600f, -1200f, 1800f, 1200f);

        /// <summary>A zone of the region: its <see cref="OverworldZone.Origin"/> says where the region sits in the stitched world.</summary>
        public const string MisturaAnchorZoneId = "dur_jarras";

        /// <summary>The bubbles of time of the Desert of the Clock (map 78): most on its roads, slow and fast in turn.</summary>
        public static readonly Mechanics.MisturaTimeBubble[] TimeBubbles =
        {
            new Mechanics.MisturaTimeBubble(new Vector2(470f, 5f), 26f, Mechanics.MisturaTimeBubbleRules.SlowFactor),
            new Mechanics.MisturaTimeBubble(new Vector2(600f, 130f), 24f, Mechanics.MisturaTimeBubbleRules.FastFactor),
            new Mechanics.MisturaTimeBubble(new Vector2(600f, 360f), 26f, Mechanics.MisturaTimeBubbleRules.SlowFactor),
            new Mechanics.MisturaTimeBubble(new Vector2(600f, 600f), 24f, Mechanics.MisturaTimeBubbleRules.FastFactor),
            new Mechanics.MisturaTimeBubble(new Vector2(600f, -130f), 24f, Mechanics.MisturaTimeBubbleRules.FastFactor),
            new Mechanics.MisturaTimeBubble(new Vector2(600f, -360f), 26f, Mechanics.MisturaTimeBubbleRules.SlowFactor),
            new Mechanics.MisturaTimeBubble(new Vector2(600f, -600f), 24f, Mechanics.MisturaTimeBubbleRules.FastFactor),
            new Mechanics.MisturaTimeBubble(new Vector2(450f, -480f), 24f, Mechanics.MisturaTimeBubbleRules.FastFactor),
            new Mechanics.MisturaTimeBubble(new Vector2(750f, -60f), 28f, Mechanics.MisturaTimeBubbleRules.SlowFactor),
            new Mechanics.MisturaTimeBubble(new Vector2(450f, 470f), 26f, Mechanics.MisturaTimeBubbleRules.FastFactor),
            new Mechanics.MisturaTimeBubble(new Vector2(760f, 480f), 26f, Mechanics.MisturaTimeBubbleRules.SlowFactor),
        };

        /// <summary>The patches of pitch of the Bitumen Wells (map 79): six across the three south roads, three off them.</summary>
        public static readonly Mechanics.MisturaTarPatch[] TarPatches =
        {
            new Mechanics.MisturaTarPatch(new Vector2(-445f, -870f), 9f),
            new Mechanics.MisturaTarPatch(new Vector2(-425f, -965f), 10f),
            new Mechanics.MisturaTarPatch(new Vector2(84f, -850f), 9f),
            new Mechanics.MisturaTarPatch(new Vector2(95f, -915f), 8f),
            new Mechanics.MisturaTarPatch(new Vector2(605f, -830f), 10f),
            new Mechanics.MisturaTarPatch(new Vector2(610f, -915f), 11f),
            new Mechanics.MisturaTarPatch(new Vector2(-240f, -1000f), 10f),
            new Mechanics.MisturaTarPatch(new Vector2(220f, -1010f), 10f),
            new Mechanics.MisturaTarPatch(new Vector2(510f, -1030f), 10f),
        };

        /// <summary>The column of the giant's shoulder (map 81), where the ground breathes (region coordinates).</summary>
        public static readonly Rect ShoulderArea = Rect.MinMaxRect(840f, -1200f, 1320f, 1200f);

        /// <summary>The anchor stones of the shoulder (map 81): a diorite shard every 120 m beside the road, then a few out on the folds.</summary>
        public static readonly Vector2[] BreathStones =
        {
            new Vector2(992f, -960f), new Vector2(998f, -840f), new Vector2(1049f, -720f), new Vector2(1044f, -600f), new Vector2(1092f, -480f), new Vector2(1068f, -360f),
            new Vector2(1092f, -240f), new Vector2(1068f, -120f), new Vector2(1092f, 120f), new Vector2(1068f, 240f), new Vector2(1092f, 360f), new Vector2(1068f, 480f),
            new Vector2(1070f, 600f), new Vector2(1028f, 720f), new Vector2(1052f, 840f), new Vector2(1028f, 960f),
            new Vector2(900f, -960f), new Vector2(1260f, -960f), new Vector2(900f, -480f), new Vector2(1260f, -480f),
            new Vector2(900f, 480f), new Vector2(1260f, 480f), new Vector2(900f, 960f), new Vector2(1260f, 960f),
        };

        public static readonly Vector2 DurJarrasOffset = Vector2.zero;
        public static readonly Vector2 GnomonCaidoOffset = new Vector2(600f, 0f);
        public static readonly Vector2 BalsaJuncoOffset = new Vector2(120f, -960f);
        public static readonly Vector2 ClareiraLenhadorOffset = new Vector2(120f, 960f);
        public static readonly Vector2 NinhoOmbroOffset = new Vector2(1080f, 0f);
        public static readonly Vector2 TendaInterpretesOffset = new Vector2(1560f, 0f);

        /// <summary>The centre of the circle of twelve gnomons (the post of the Gnômon Caído stands on it, region coordinates).</summary>
        public static readonly Vector2 GnomonDialCenter = GnomonCaidoOffset;

        /// <summary>The centre of Hobabish's rings (the Clareira do Lenhador stands on it, region coordinates).</summary>
        public static readonly Vector2 CedarRingCenter = ClareiraLenhadorOffset;

        /// <summary>The foot of Ullikummi's diorite (region coordinates).</summary>
        public static readonly Vector2 DioriteCenter = new Vector2(1080f, -960f);

        /// <summary>Half the side (m) of the flat ground reserved for Dur-Jarras' recipe (±30 m at y = 0).</summary>
        public const float DurJarrasGround = 30f;

        public const int GnomonCount = 12;
        public const float GnomonRingRadius = 180f;
        public const int ResinLanternCount = 7;
        public const float ResinLanternRingRadius = 150f;

        /// <summary>Where gnomon <paramref name="i"/> of the twelve lies (region coordinates; 15° off the spokes, between the trails).</summary>
        public static Vector2 GnomonPosition(int i)
        {
            float a = (15f + 30f * i) * Mathf.Deg2Rad;
            return GnomonDialCenter + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * GnomonRingRadius;
        }

        /// <summary>Where resin lantern <paramref name="i"/> of the seven stands (region coordinates).</summary>
        public static Vector2 ResinLanternPosition(int i)
        {
            float a = (90f + 360f * i / ResinLanternCount) * Mathf.Deg2Rad;
            return CedarRingCenter + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * ResinLanternRingRadius;
        }

        /// <summary>The canal of bitumen across the south row (map 79).</summary>
        public static readonly Vector2[] CanalDeBetume =
        {
            new Vector2(-540f, -760f), new Vector2(-200f, -800f), new Vector2(150f, -770f), new Vector2(500f, -800f), new Vector2(820f, -760f),
        };

        /// <summary>The ponds of pitch, the palm canal's ponds and the still water of the dream (centre segment, width).</summary>
        public static readonly (Vector2 A, Vector2 B, float Width)[] MisturaPools =
        {
            (new Vector2(-300f, -1010f), new Vector2(-288f, -1004f), 22f),
            (new Vector2(-500f, -1060f), new Vector2(-490f, -1056f), 24f),
            (new Vector2(40f, -1060f), new Vector2(50f, -1054f), 22f),
            (new Vector2(300f, -1000f), new Vector2(310f, -996f), 22f),
            (new Vector2(-60f, -900f), new Vector2(-52f, -896f), 20f),
            (new Vector2(470f, -980f), new Vector2(478f, -976f), 20f),
            (new Vector2(720f, -1040f), new Vector2(730f, -1034f), 22f),
            (new Vector2(-60f, -420f), new Vector2(-50f, -414f), 16f),
            (new Vector2(200f, -520f), new Vector2(210f, -514f), 16f),
            (new Vector2(1430f, 500f), new Vector2(1440f, 504f), 22f),
            (new Vector2(1450f, 640f), new Vector2(1460f, 646f), 22f),
            (new Vector2(1680f, 510f), new Vector2(1690f, 514f), 22f),
        };

        /// <summary>The sheets' monsters (Mapas100 77–82): the region's own twelve beasts, and existing ones standing in for the rest.</summary>
        private static class Mistura
        {
            public const string BrickLion = "enemy_forest_brick_lion";
            public const string Mushussu = "enemy_forest_young_mushussu";
            public const string ScorpionMan = "enemy_forest_scorpion_man";
            public const string Locust = "enemy_forest_bitumen_locust";
            public const string Hourglass = "enemy_forest_living_hourglass";
            public const string InstantLizard = "enemy_mountain_instant_lizard";
            public const string LateShadow = "enemy_forest_late_shadow";
            public const string ClockUshabti = "enemy_mountain_lacquer_ushabti";
            public const string LivingTar = "enemy_forest_living_tar";
            public const string TarHeron = "enemy_forest_tar_heron";
            public const string Catfish = "enemy_mountain_black_catfish";
            public const string TarSmoke = "enemy_mountain_smoke_harpy";
            public const string AngryCedar = "enemy_forest_angry_cedar";
            public const string CedarLynx = "enemy_forest_cedar_lynx";
            public const string ResinBee = "enemy_forest_resin_bee";
            public const string GlowMantle = "enemy_forest_glow_mantle";
            public const string Diorite = FarBeasts.DioriteStone;
            public const string ShoulderBird = FarBeasts.ShoulderBird;
            public const string Tremor = "enemy_mountain_tremor";
            public const string UllikummiSon = "enemy_forest_ullikummi_son";
            public const string BrokenTablet = "enemy_forest_broken_tablet";
            public const string GardenShadow = "enemy_forest_garden_shadow";
            public const string DreamBird = FarBeasts.DreamBird;
            public const string ErasingWater = "enemy_mountain_erasing_water";
        }

        private static OverworldLayout MisturaRegion()
        {
            OverworldLayout w = NewRegion(MisturaRegionId, MapRegion.MargensDaMistura, MisturaBounds);
            w.World.Biome = BiomeType.Forest;
            const string durJarras = DurJarrasMapId;
            const string relogio = DesertoRelogioMapId;
            const string betume = PocosBetumeMapId;
            const string cedral = CedralHobabishMapId;
            const string ombro = OmbroUpelluriMapId;
            const string tabuas = TabuasSonhoMapId;
            const string plain = MapRegionTypes.Plain;
            const string desert = MapRegionTypes.Desert;
            const string swamp = MapRegionTypes.Swamp;
            const string forest = MapRegionTypes.Forest;
            const string mountain = MapRegionTypes.Mountain;
            const string ruins = MapRegionTypes.Ruins;
            const string water = MapRequirementTags.FreshWater;
            const string reedBoat = MapRequirementTags.SaddleRequired;
            const string rope = MapRequirementTags.ElevatedPlatforms;
            const string crampons = MapRequirementTags.ClimbingCrampons;
            const string lamp = MapRequirementTags.TorchRequired;

            // ---------------- Zones (a 5 × 5 grid; columns x −600/−120/360/840/1320/1800, rows z −1200/−720/−240/240/720/1200) ----------------
            // West column: the pitch pools (79), the kilns (77), the way in from the river, the tells (77), the edge of the cedars (80).
            OverworldZone zPocas = Zone(w, "pocas_de_piche", betume, null, -600f, -1200f, -120f, -720f, OverworldTerrain.TarPools, 316, 320, 312,
                "Pitch Pools", "Poças de Piche", swamp, reedBoat, 95f);
            OverworldZone zOlarias = Zone(w, "olarias", durJarras, null, -600f, -720f, -120f, -240f, OverworldTerrain.BrickKilns, 303, 307, 299,
                "Brick Kilns", "Olarias", plain, spacing: 90f);
            OverworldZone zPortao = Zone(w, "portao_de_tijolo", durJarras, null, -600f, -240f, -120f, 240f, OverworldTerrain.BrickGate, 301, 305, 297,
                "Brick Gate", "Portão de Tijolo", plain, spacing: 90f);
            OverworldZone zTell = Zone(w, "colinas_de_tell", durJarras, null, -600f, 240f, -120f, 720f, OverworldTerrain.TellMounds, 303, 307, 299,
                "Tell Mounds", "Colinas de Tell", plain, spacing: 90f);
            OverworldZone zBeira = Zone(w, "beira_do_cedral", cedral, null, -600f, 720f, -120f, 1200f, OverworldTerrain.CedarEdge, 324, 330, 320,
                "Edge of the Cedars", "Beira do Cedral", forest, rope, 100f);
            // Second column: the wells (79), the palm canal (77), Dur-Jarras, the barley (77), the rings of Hobabish (80).
            OverworldZone zPocos = Zone(w, "pocos_de_betume", betume, null, -120f, -1200f, 360f, -720f, OverworldTerrain.BitumenWells, 314, 318, 310,
                "Bitumen Wells", "Poços de Betume", swamp, reedBoat, 95f);
            OverworldZone zCanalPalmeiras = Zone(w, "canal_das_palmeiras", durJarras, null, -120f, -720f, 360f, -240f, OverworldTerrain.PalmCanal, 302, 306, 298,
                "Palm Canal", "Canal das Palmeiras", plain, spacing: 90f);
            OverworldZone zDurJarras = Zone(w, "dur_jarras", durJarras, null, -120f, -240f, 360f, 240f, OverworldTerrain.ZigguratTerrace, 300, 304, 296,
                "Dur-Jarras", "Dur-Jarras", plain, spacing: 85f);
            OverworldZone zCevada = Zone(w, "terracos_de_cevada", durJarras, null, -120f, 240f, 360f, 720f, OverworldTerrain.BarleyTerraces, 302, 306, 298,
                "Barley Terraces", "Terraços de Cevada", plain, spacing: 90f);
            OverworldZone zCedral = Zone(w, "cedral_de_hobabish", cedral, null, -120f, 720f, 360f, 1200f, OverworldTerrain.CedarRings, 322, 328, 318,
                "Hobabish Cedar Forest", "Cedral de Hobabish", forest, rope, 100f);
            // Third column: the reeds of pitch (79), the late shadows, the Desert of the Clock, the hours (78), the elder cedars (80).
            OverworldZone zJuncal = Zone(w, "juncal_de_piche", betume, null, 360f, -1200f, 840f, -720f, OverworldTerrain.TarReeds, 318, 326, 314,
                "Pitch Reeds", "Juncal de Piche", swamp, reedBoat, 95f);
            OverworldZone zSombras = Zone(w, "sombras_atrasadas", relogio, null, 360f, -720f, 840f, -240f, OverworldTerrain.ShadowSands, 308, 313, 304,
                "Late Shadows", "Sombras Atrasadas", desert, water, 95f);
            OverworldZone zRelogio = Zone(w, "deserto_do_relogio", relogio, null, 360f, -240f, 840f, 240f, OverworldTerrain.ClockDesert, 306, 311, 302,
                "Desert of the Clock", "Deserto do Relógio", desert, water, 95f);
            OverworldZone zHoras = Zone(w, "areia_das_horas", relogio, null, 360f, 240f, 840f, 720f, OverworldTerrain.HourSands, 308, 316, 304,
                "Sands of the Hours", "Areia das Horas", desert, water, 95f);
            OverworldZone zAnciaos = Zone(w, "cedros_anciaos", cedral, null, 360f, 720f, 840f, 1200f, OverworldTerrain.ElderCedars, 326, 336, 322,
                "Elder Cedars", "Cedros Anciãos", forest, rope, 100f);
            // Fourth column, the giant's shoulder (81): the growing diorite, the folds, the shoulder with the Ninho, the collarbone, the eaves of the sky.
            OverworldZone zDiorito = Zone(w, "diorito_crescente", ombro, null, 840f, -1200f, 1320f, -720f, OverworldTerrain.DioriteRise, 340, 346, 336,
                "Growing Diorite", "Diorito Crescente", mountain, crampons, 100f);
            OverworldZone zDobras = Zone(w, "dobras_de_pedra", ombro, null, 840f, -720f, 1320f, -240f, OverworldTerrain.SkinFolds, 336, 342, 332,
                "Folds of Stone", "Dobras de Pedra", mountain, crampons, 100f);
            OverworldZone zOmbro = Zone(w, "ombro_de_upelluri", ombro, null, 840f, -240f, 1320f, 240f, OverworldTerrain.ShoulderStone, 334, 340, 330,
                "Upelluri's Shoulder", "Ombro de Upelluri", mountain, crampons, 100f);
            OverworldZone zClavicula = Zone(w, "clavicula_do_gigante", ombro, null, 840f, 240f, 1320f, 720f, OverworldTerrain.SkinFolds, 336, 342, 332,
                "Giant's Collarbone", "Clavícula do Gigante", mountain, crampons, 100f);
            OverworldZone zBeiral = Zone(w, "beiral_do_ceu", ombro, null, 840f, 720f, 1320f, 1200f, OverworldTerrain.ShoulderStone, 340, 346, 336,
                "Eaves of the Sky", "Beiral do Céu", mountain, crampons, 100f);
            // East column, the field of the dream (82): the broken tablets, the cut garden, the field with the Tenda, the still water, the dream tablets.
            OverworldZone zQuebradas = Zone(w, "tabuas_quebradas", tabuas, null, 1320f, -1200f, 1800f, -720f, OverworldTerrain.DreamTablets, 352, 360, 348,
                "Broken Tablets", "Tábuas Quebradas", ruins, lamp, 100f);
            OverworldZone zJardim = Zone(w, "jardim_cortado", tabuas, null, 1320f, -720f, 1800f, -240f, OverworldTerrain.CutGarden, 346, 354, 342,
                "Cut Garden", "Jardim Cortado", ruins, lamp, 100f);
            OverworldZone zCampo = Zone(w, "campo_das_tabuas", tabuas, null, 1320f, -240f, 1800f, 240f, OverworldTerrain.DreamTablets, 344, 352, 340,
                "Field of the Tablets", "Campo das Tábuas", ruins, lamp, 100f);
            OverworldZone zAgua = Zone(w, "agua_parada", tabuas, null, 1320f, 240f, 1800f, 720f, OverworldTerrain.CutGarden, 346, 354, 342,
                "Still Water", "Água Parada", ruins, lamp, 100f);
            OverworldZone zSonho = Zone(w, "tabuas_do_sonho", tabuas, null, 1320f, 720f, 1800f, 1200f, OverworldTerrain.DreamTablets, 352, 360, 348,
                "Dream Tablets", "Tábuas do Sonho", ruins, lamp, 100f);

            AddMisturaFauna(zPocas, zOlarias, zPortao, zTell, zBeira, zPocos, zCanalPalmeiras, zDurJarras, zCevada, zCedral, zJuncal, zSombras, zRelogio, zHoras, zAnciaos,
                zDiorito, zDobras, zOmbro, zClavicula, zBeiral, zQuebradas, zJardim, zCampo, zAgua, zSonho);

            // ---------------- Settlements ----------------
            // Dur-Jarras: the region's return city and the barca's stop for 77–82 (flat ground at y = 0, ±30 m). Then the posts of 78–82.
            w.Vila = Place(w, durJarras, DurJarrasRecipe(), DurJarrasOffset, Rect.MinMaxRect(-26f, -26f, 26f, 26f), DurJarrasGround, true);
            Place(w, relogio, Post("gnomon_caido", TownSwatch.Sandstone, TownSwatch.DustSky, TownRoofKind.Flat, TownShopDressing.Herder, false),
                GnomonCaidoOffset, PostSafe, 18f, false);
            Place(w, betume, Post("balsa_de_junco", TownSwatch.MarshReed, TownSwatch.Bitumen, TownRoofKind.Gable, TownShopDressing.BoatTrader, false),
                BalsaJuncoOffset, PostSafe, 18f, false);
            Place(w, cedral, Post("clareira_do_lenhador", TownSwatch.Trunk, TownSwatch.Pine, TownRoofKind.Gable, TownShopDressing.Herder, false),
                ClareiraLenhadorOffset, PostSafe, 18f, false);
            Place(w, ombro, Post("ninho_no_ombro", TownSwatch.Stone, TownSwatch.Moss, TownRoofKind.Gable, TownShopDressing.HuntingRack, false),
                NinhoOmbroOffset, PostSafe, 18f, false);
            Place(w, tabuas, Post("tenda_dos_interpretes", TownSwatch.LimeWhite, TownSwatch.MoonLilac, TownRoofKind.Flat, TownShopDressing.Storyteller, false),
                TendaInterpretesOffset, PostSafe, 18f, false);

            // ---------------- Water ----------------
            // The canal of bitumen across the south row (10 m, carved, three bridges where the south roads cross it); the ponds.
            Vector2 bridgeWest = On(CanalDeBetume[0], CanalDeBetume[1], 100f / 340f);
            Vector2 bridgeSouth = On(CanalDeBetume[1], CanalDeBetume[2], 266f / 350f);
            Vector2 bridgeEast = On(CanalDeBetume[3], CanalDeBetume[4], 100f / 320f);
            MapRiver canal = River(w, 10f, CanalDeBetume);
            Crossing(canal, bridgeWest, 4.5f, true);
            Crossing(canal, bridgeSouth, 4.5f, true);
            Crossing(canal, bridgeEast, 4.5f, true);
            foreach ((Vector2 a, Vector2 b, float width) in MisturaPools)
            {
                River(w, width, a, b);
            }

            // ---------------- Gates out of the region ----------------
            // West: two passes to the Rio das Doze Horas (the river's east flank, Beira Fervente; tagged with maps 75 and 76: one target per gate).
            Gate(w, "exit_beira_fervente_sul", MapExitKind.Map, LagoDeFogoMapId, Rect.MinMaxRect(-600f, -108f, -597f, -92f), new Vector2(-591f, -100f), Vector2.right);
            Gate(w, "exit_beira_fervente_norte", MapExitKind.Map, BancoApepMapId, Rect.MinMaxRect(-600f, 152f, -597f, 168f), new Vector2(-591f, 160f), Vector2.right);
            Gate(w, "exit_island", MapExitKind.WorldMap, null, Rect.MinMaxRect(-82f, 78f, -78f, 82f), new Vector2(-80f, 70f), Vector2.up);
            Landmark(w, MapLandmarkKind.StoneRing, -80f, 80f, 0f, 0.45f);

            // ---------------- Trails ----------------
            float g = DurJarrasGround;
            // West road from the city through the Brick Gate to the two passes to the river.
            Vector2 westFork = new Vector2(-300f, 30f);
            PathVia(w, 4f, new Vector2(-g, 0f), new Vector2(-150f, 10f), westFork);
            PathVia(w, 4f, westFork, new Vector2(-450f, -40f), new Vector2(-591f, -100f));
            PathVia(w, 4f, westFork, new Vector2(-450f, 100f), new Vector2(-591f, 160f));
            // From the fork, north to the tells and south to the kilns, on to the pitch pools over the west bridge.
            PathVia(w, 3.5f, westFork, new Vector2(-320f, 320f), new Vector2(-360f, 640f), new Vector2(-300f, 900f), new Vector2(-300f, 960f));
            PathVia(w, 3.5f, westFork, new Vector2(-340f, -280f), new Vector2(-380f, -480f), new Vector2(-440f, -700f), bridgeWest, new Vector2(-450f, -860f), new Vector2(-420f, -980f));

            // South road: down the palm canal, over the canal bridge to the Balsa de Junco; a branch east to the late shadows.
            Vector2 southFork = new Vector2(20f, -240f);
            PathVia(w, 4f, new Vector2(0f, -g), new Vector2(10f, -130f), southFork, new Vector2(40f, -500f), new Vector2(66f, -740f), bridgeSouth, new Vector2(80f, -820f), new Vector2(90f, -900f),
                BalsaJuncoOffset + new Vector2(-18f, 0f));
            PathVia(w, 3.5f, southFork, new Vector2(300f, -480f), new Vector2(600f, -480f));

            // East road through the desert: the Gnômon Caído, the shoulder and its Ninho, the Tenda of the dream.
            Vector2 eastFork = new Vector2(300f, 20f);
            PathVia(w, 4f, new Vector2(g, 0f), new Vector2(150f, 10f), eastFork, new Vector2(420f, 8f), GnomonCaidoOffset + new Vector2(-18f, 0f));
            PathVia(w, 4f, GnomonCaidoOffset + new Vector2(18f, 0f), new Vector2(790f, 6f), NinhoOmbroOffset + new Vector2(-18f, 0f));
            PathVia(w, 4f, NinhoOmbroOffset + new Vector2(18f, 0f), new Vector2(1300f, 10f), TendaInterpretesOffset + new Vector2(-18f, 0f));

            // North of the east fork: the barley terraces and the cedars of Hobabish; east along the elder cedars.
            PathVia(w, 4f, eastFork, new Vector2(260f, 300f), new Vector2(220f, 640f), ClareiraLenhadorOffset + new Vector2(0f, -18f));
            PathVia(w, 3.5f, ClareiraLenhadorOffset + new Vector2(18f, 0f), new Vector2(360f, 960f), new Vector2(600f, 960f));
            PathVia(w, 3.5f, ClareiraLenhadorOffset + new Vector2(-18f, 0f), new Vector2(-300f, 900f));

            // North and south of the Gnômon Caído: the hours and the late shadows, on to the elder cedars and the pitch reeds (over the east bridge).
            PathVia(w, 3.5f, GnomonCaidoOffset + new Vector2(0f, 18f), new Vector2(600f, 240f), new Vector2(600f, 480f), new Vector2(620f, 800f), new Vector2(600f, 960f));
            PathVia(w, 3.5f, GnomonCaidoOffset + new Vector2(0f, -18f), new Vector2(600f, -240f), new Vector2(600f, -480f), new Vector2(600f, -700f), bridgeEast, new Vector2(610f, -900f));

            // North and south along the giant's shoulder.
            PathVia(w, 3.5f, NinhoOmbroOffset + new Vector2(0f, 18f), new Vector2(1080f, 240f), new Vector2(1080f, 480f), new Vector2(1040f, 700f), new Vector2(1040f, 940f));
            PathVia(w, 3.5f, NinhoOmbroOffset + new Vector2(0f, -18f), new Vector2(1080f, -240f), new Vector2(1080f, -480f), new Vector2(1040f, -700f), new Vector2(980f, -940f));

            // North and south through the field of the dream.
            PathVia(w, 3.5f, TendaInterpretesOffset + new Vector2(0f, 18f), new Vector2(1560f, 240f), new Vector2(1560f, 480f), new Vector2(1560f, 900f));
            PathVia(w, 3.5f, TendaInterpretesOffset + new Vector2(0f, -18f), new Vector2(1560f, -240f), new Vector2(1560f, -480f), new Vector2(1560f, -900f));

            // ---------------- Landmarks ----------------
            Sign(w, westFork, new Vector2(-320f, 320f), 4f, 0f);
            Sign(w, southFork, new Vector2(300f, -480f), 4f, 0f);
            Sign(w, eastFork, new Vector2(260f, 300f), 4f, 0f);
            Landmark(w, MapLandmarkKind.Campfire, -470f, 280f, 0f, 0.9f);
            Landmark(w, MapLandmarkKind.Campfire, -220f, -560f, 0f, 0.9f);
            Landmark(w, MapLandmarkKind.Campfire, 470f, 420f, 0f, 1f);
            Landmark(w, MapLandmarkKind.Campfire, 480f, -600f, 0f, 1f);
            Landmark(w, MapLandmarkKind.Campfire, -170f, 1050f, 0f, 1f);
            Landmark(w, MapLandmarkKind.Campfire, 930f, 420f, 0f, 1f);
            Landmark(w, MapLandmarkKind.Campfire, 1400f, -440f, 0f, 1f);
            Landmark(w, MapLandmarkKind.StoneRing, SonhoDaDestruicaoArena.x, SonhoDaDestruicaoArena.y, 0f, 1f);
            Landmark(w, MapLandmarkKind.StoneRing, ReiDoPicheArena.x, ReiDoPicheArena.y, 0f, 1.1f);
            Landmark(w, MapLandmarkKind.StoneRing, InstantePerdidoArena.x, InstantePerdidoArena.y, 0f, 1.1f);
            Landmark(w, MapLandmarkKind.StoneRing, HobabishArena.x, HobabishArena.y, 0f, 1.1f);
            Landmark(w, MapLandmarkKind.StoneRing, UllikummiArena.x, UllikummiArena.y, 0f, 1.1f);

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
        // The city (its own recipe, Cities/Town: the Ziggurat style inside a radius of 22.5 m on its reserved flat ground of ±30 m at y = 0;
        // no backdrop: the open land is all round it). Its streets leave W, E and S (no north street: the ziggurat closes the plaza to the
        // north), so the north roads leave from the east road.
        // ------------------------------------------------------------------

        private static CityTownRecipe DurJarrasRecipe() => NoBackdrop(CityTownRecipes.DurJarras());

        // ------------------------------------------------------------------
        // Monster groups per area (existing definitions only, no boss; Mapas100 77–82 archetypes)
        // ------------------------------------------------------------------

        private static void MisturaGroup(OverworldZone zone, string id, string archetype, string lead, string mate, NightSpecies night, int maxAlive = 4, float respawn = 25f)
        {
            zone.Fauna.Add(Fauna(id, archetype, T(N(lead, 3), N(mate, 1)), T(V(lead, 3), S(night, 1)), maxAlive, null, respawn));
        }

        private static void AddMisturaFauna(OverworldZone pocas, OverworldZone olarias, OverworldZone portao, OverworldZone tell, OverworldZone beira, OverworldZone pocos,
            OverworldZone canalPalmeiras, OverworldZone durJarras, OverworldZone cevada, OverworldZone cedral, OverworldZone juncal, OverworldZone sombras, OverworldZone relogio,
            OverworldZone horas, OverworldZone anciaos, OverworldZone diorito, OverworldZone dobras, OverworldZone ombro, OverworldZone clavicula, OverworldZone beiral,
            OverworldZone quebradas, OverworldZone jardim, OverworldZone campo, OverworldZone agua, OverworldZone sonho)
        {
            NightSpecies owl = NightVariantRules.RasgaMortalha;
            NightSpecies bats = NightVariantRules.MorcegoVampiro;
            NightSpecies moon = NightVariantRules.MaeDaLua;
            NightSpecies sparks = NightVariantRules.VagaLume;
            NightSpecies werewolf = NightVariantRules.Lobisomem;

            // Round the city (77): the Leão de Tijolo Apagado [tanque], the Mušhuššu Jovem [elite], the Homem-Escorpião [distância] (the scorpion),
            // the Gafanhoto de Betume [enxame].
            MisturaGroup(durJarras, "leoes_do_terraco", "[tanque]", Mistura.BrickLion, Mistura.Locust, moon, 3, 30f);
            MisturaGroup(durJarras, "gafanhotos_do_terraco", "[enxame]", Mistura.Locust, Mistura.BrickLion, sparks, 5);
            MisturaGroup(portao, "leoes_do_portao", "[tanque]", Mistura.BrickLion, Mistura.ScorpionMan, moon, 3, 30f);
            MisturaGroup(portao, "homens_escorpiao_do_portao", "[distância]", Mistura.ScorpionMan, Mistura.Locust, owl);
            MisturaGroup(olarias, "gafanhotos_das_olarias", "[enxame]", Mistura.Locust, Mistura.ScorpionMan, sparks, 5);
            MisturaGroup(olarias, "leoes_das_olarias", "[tanque]", Mistura.BrickLion, Mistura.Locust, bats, 3, 30f);
            MisturaGroup(tell, "homens_escorpiao_do_tell", "[distância]", Mistura.ScorpionMan, Mistura.BrickLion, owl);
            MisturaGroup(tell, "gafanhotos_do_tell", "[enxame]", Mistura.Locust, Mistura.ScorpionMan, sparks, 5);
            MisturaGroup(canalPalmeiras, "mushussu_do_canal", "[elite]", Mistura.Mushussu, Mistura.Locust, moon, 3, 32f);
            MisturaGroup(canalPalmeiras, "homens_escorpiao_do_canal", "[distância]", Mistura.ScorpionMan, Mistura.Locust, moon);
            MisturaGroup(cevada, "gafanhotos_da_cevada", "[enxame]", Mistura.Locust, Mistura.Mushussu, sparks, 5);
            MisturaGroup(cevada, "mushussu_da_cevada", "[elite]", Mistura.Mushussu, Mistura.BrickLion, werewolf, 3, 32f);

            // The Desert of the Clock (78): the Ampulheta Viva [tanque], the Lagarto do Instante [rápido] (its own beast now, BorrowedBeasts), the
            // Sombra Atrasada [controle], the Ushabti de Relógio [distância] (the lacquer ushabti).
            MisturaGroup(relogio, "ampulhetas_vivas", "[tanque]", Mistura.Hourglass, Mistura.InstantLizard, bats, 3, 30f);
            MisturaGroup(relogio, "lagartos_do_instante", "[rápido]", Mistura.InstantLizard, Mistura.LateShadow, owl);
            MisturaGroup(sombras, "sombras_atrasadas", "[controle]", Mistura.LateShadow, Mistura.ClockUshabti, bats);
            MisturaGroup(sombras, "ushabtis_de_relogio", "[distância]", Mistura.ClockUshabti, Mistura.InstantLizard, moon);
            MisturaGroup(horas, "lagartos_das_horas", "[rápido]", Mistura.InstantLizard, Mistura.Hourglass, sparks);
            MisturaGroup(horas, "ushabtis_das_horas", "[distância]", Mistura.ClockUshabti, Mistura.LateShadow, owl);

            // The Bitumen Wells (79): the Betume Vivo [controle], the Garça de Piche [distância], the Bagre Negro [tanque]
            // (its own beast now, BorrowedBeasts), the Fumaça de Piche [enxame] (the smoke harpy).
            MisturaGroup(pocos, "betumes_vivos", "[controle]", Mistura.LivingTar, Mistura.Catfish, moon);
            MisturaGroup(pocos, "bagres_negros", "[tanque]", Mistura.Catfish, Mistura.TarSmoke, moon, 3, 30f);
            MisturaGroup(pocas, "fumacas_de_piche", "[enxame]", Mistura.TarSmoke, Mistura.LivingTar, sparks, 5);
            MisturaGroup(pocas, "betumes_das_pocas", "[controle]", Mistura.LivingTar, Mistura.TarHeron, bats);
            MisturaGroup(juncal, "garcas_de_piche", "[distância]", Mistura.TarHeron, Mistura.LivingTar, moon);
            MisturaGroup(juncal, "fumacas_do_juncal", "[enxame]", Mistura.TarSmoke, Mistura.TarHeron, sparks, 5);

            // The Cedral de Hobabish (80): the Cedro Raivoso [tanque], the Lince do Cedro [rápido] (the irbis), the Abelha-Resina
            // [enxame] (the golden bee), the Manto de Brilho [controle].
            MisturaGroup(cedral, "cedros_raivosos", "[tanque]", Mistura.AngryCedar, Mistura.ResinBee, werewolf, 3, 30f);
            MisturaGroup(cedral, "mantos_de_brilho", "[controle]", Mistura.GlowMantle, Mistura.CedarLynx, owl);
            MisturaGroup(beira, "linces_do_cedro", "[rápido]", Mistura.CedarLynx, Mistura.ResinBee, owl);
            MisturaGroup(beira, "abelhas_resina", "[enxame]", Mistura.ResinBee, Mistura.CedarLynx, sparks, 5);
            MisturaGroup(anciaos, "cedros_anciaos_raivosos", "[tanque]", Mistura.AngryCedar, Mistura.GlowMantle, werewolf, 3, 30f);
            MisturaGroup(anciaos, "mantos_dos_anciaos", "[controle]", Mistura.GlowMantle, Mistura.ResinBee, bats);

            // The shoulder of Upelluri (81): the Pedra de Diorito [tanque] and the Ave do Ombro [rápido] (their own beasts now, FarBeasts), the Tremor
            // [controle] (its own beast now, BorrowedBeasts), the Filho de Ullikummi [elite].
            MisturaGroup(ombro, "aves_do_ombro", "[rápido]", Mistura.ShoulderBird, Mistura.Diorite, owl);
            MisturaGroup(ombro, "pedras_de_diorito", "[tanque]", Mistura.Diorite, Mistura.ShoulderBird, bats, 3, 30f);
            MisturaGroup(dobras, "tremores_das_dobras", "[controle]", Mistura.Tremor, Mistura.Diorite, bats);
            MisturaGroup(dobras, "aves_das_dobras", "[rápido]", Mistura.ShoulderBird, Mistura.Tremor, moon);
            MisturaGroup(clavicula, "pedras_da_clavicula", "[tanque]", Mistura.Diorite, Mistura.Tremor, bats, 3, 30f);
            MisturaGroup(clavicula, "tremores_da_clavicula", "[controle]", Mistura.Tremor, Mistura.ShoulderBird, owl);
            MisturaGroup(diorito, "filhos_de_ullikummi", "[elite]", Mistura.UllikummiSon, Mistura.Diorite, werewolf, 3, 32f);
            MisturaGroup(diorito, "pedras_do_diorito", "[tanque]", Mistura.Diorite, Mistura.UllikummiSon, bats, 3, 30f);
            MisturaGroup(beiral, "filhos_do_beiral", "[elite]", Mistura.UllikummiSon, Mistura.ShoulderBird, moon, 3, 32f);
            MisturaGroup(beiral, "aves_do_beiral", "[rápido]", Mistura.ShoulderBird, Mistura.UllikummiSon, owl);

            // The Field of the Dream Tablets (82): the Tábua Quebrada [distância], the Jardim-Sombra [tanque], the Pássaro-Sonho [rápido]
            // (its own beast now, FarBeasts), the Água que Apaga [controle] (its own beast now, BorrowedBeasts).
            MisturaGroup(campo, "tabuas_do_campo", "[distância]", Mistura.BrokenTablet, Mistura.DreamBird, moon);
            MisturaGroup(campo, "passaros_sonho", "[rápido]", Mistura.DreamBird, Mistura.BrokenTablet, owl);
            MisturaGroup(jardim, "jardins_sombra", "[tanque]", Mistura.GardenShadow, Mistura.DreamBird, werewolf, 3, 30f);
            MisturaGroup(jardim, "passaros_do_jardim", "[rápido]", Mistura.DreamBird, Mistura.GardenShadow, owl);
            MisturaGroup(agua, "aguas_que_apagam", "[controle]", Mistura.ErasingWater, Mistura.DreamBird, moon);
            MisturaGroup(agua, "jardins_da_agua", "[tanque]", Mistura.GardenShadow, Mistura.ErasingWater, moon, 3, 30f);
            MisturaGroup(quebradas, "tabuas_quebradas", "[distância]", Mistura.BrokenTablet, Mistura.ErasingWater, bats);
            MisturaGroup(quebradas, "aguas_das_tabuas", "[controle]", Mistura.ErasingWater, Mistura.BrokenTablet, moon);
            MisturaGroup(sonho, "passaros_do_sonho", "[rápido]", Mistura.DreamBird, Mistura.BrokenTablet, werewolf);
            MisturaGroup(sonho, "jardins_do_sonho", "[tanque]", Mistura.GardenShadow, Mistura.DreamBird, bats, 3, 30f);
        }
    }
}
