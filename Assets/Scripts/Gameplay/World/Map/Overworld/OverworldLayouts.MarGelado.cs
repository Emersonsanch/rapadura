using Rapadura.Gameplay.Night;
using Rapadura.Gameplay.World.Biomes;
using Rapadura.Gameplay.World.Cities.Town;
using Rapadura.Gameplay.World.Map.Areas;
using Rapadura.Gameplay.World.Map.Mechanics;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Overworld
{
    /// <summary>
    /// Região 11 — Mar Gelado as ONE continuous map of 2400 × 2400 m (Docs/Mapas100.md §0.10 and §5, maps 59–64), no scene change
    /// between the harbour and Old Kul's lake. The Porto da Lua Branca (59, the capital of the Ar Sereno, the first map of the
    /// region, level 180) stands at the plaza (0, 0) on the shore of a frozen bay, 600 m from the south edge where a pass comes up
    /// from the Planície de Mingcheng (in the stitched world the frozen sea fills the walled hole north of Mingcheng, west of the
    /// Picos de Farnkesh: two passes go east to the peaks' foot woods and pinnacle wood); the sea opens northwards:
    /// <list type="bullet">
    /// <item>round the harbour (nv 180–187): the <b>Porto da Lua Branca</b>, the <b>Baía Gelada</b> and the <b>Baía das
    /// Focas</b>, the snowy shore of the <b>Passo da Muralha</b> (the way in from Mingcheng) and the <b>Colinas de Neve</b>, the
    /// <b>Pastos de Rena</b>;</item>
    /// <item>the canals north of the harbour (nv 186–193, map 60): the <b>Canais de Gelo</b> with the Casa de Neve dos Barqueiros
    /// (post), the <b>Canais do Oeste</b> and the <b>Gelo Rachado</b>;</item>
    /// <item>west, Omöl's tundra (nv 194–204, map 61): the <b>Tundra do Sul</b>, the <b>Tundra de Omöl</b> with Yen's tent (post)
    /// and the <b>Lagos de Omöl</b>;</item>
    /// <item>east, under the peaks (nv 202–212, map 62): the <b>Terra Congelada</b> (the way in from the peaks' foot woods), the
    /// <b>Fiorde das Presas</b> and the <b>Galerias dos Mamutes</b> with the Boca do Gelo (post; the way in from the pinnacle
    /// wood);</item>
    /// <item>the Mar das Auroras (nv 212–224, map 63: Blizzard Resistance): the <b>Banquisas</b> and the <b>Banquisas do
    /// Oeste</b>, the icebergs of the <b>Mar das Auroras</b>, the <b>Ilha do Farol</b> with the Farol de Gelo (post) and the
    /// <b>Fiorde Negro</b>;</item>
    /// <item>Old Kul's Deep (nv 226–240, map 64: Fine Thermal Clothes): the <b>Buracos de Pesca</b>, the <b>Fundo do Velho Kul</b>
    /// with the Buraco Grande de Pesca (post), the <b>Lago do Kul</b> and the <b>Gelo Negro</b>, the <b>Fiorde do Kul</b>.</item>
    /// </list>
    /// Water: three black-water canals out of the harbour (two running north to open leads in the pack ice, one across the north
    /// road under a whalebone bridge), seven round lakes on Omöl's tundra, open leads in the pack ice, the seven fishing holes of
    /// Old Kul's lake and its Great Hole. Monsters: the region's own beasts (the leopard seal, also for the narwhals; the smoke
    /// walrus; the ice gull, also for the storm gulls; the arctic fox; the polar bear; Omöl's clay reindeer, stone hare and twig
    /// bird; the mammoth under the earth; the sea wolf; the giant pike, also for the ice fish; the shadow seal), existing beasts
    /// that fit the other sheets (the sand snake for the lantern eels, the ooze for the cracks, the ice roots and the hagfish, the
    /// firefly swarm for the tundra mosquitoes and the little kuls, the giant marmot for the ice moles, the broken lantern for the
    /// living crystals, the ill-wind mist for the floating ice, the irbis in the fjords) and the night species; no boss (the
    /// Velho Kul is the map 64 world boss: not placed).
    /// </summary>
    public static partial class OverworldLayouts
    {
        public const string MarGeladoRegionId = "overworld_mar_gelado";

        public const string PortoLuaBrancaMapId = "map_059_porto_lua_branca";
        public const string CanaisGeloMapId = "map_060_canais_gelo";
        public const string TundraOmolMapId = "map_061_tundra_omol";
        public const string GaleriasMamutesMapId = "map_062_galerias_mamutes";
        public const string MarAurorasMapId = "map_063_mar_auroras";
        public const string FundoVelhoKulMapId = "map_064_fundo_velho_kul";

        public static readonly Vector2 PortoLuaBrancaOffset = Vector2.zero;
        /// <summary>The Velho Kul's arena (64): a stone ring on the ice behind the Great Hole.</summary>
        public static readonly Vector2 VelhoKulArena = new Vector2(0f, 1762f);

        /// <summary>A Morsa-Rainha's arena (60, map boss): the stone ring on the north road past the whalebone bridge.</summary>
        public static readonly Vector2 MorsaRainhaArena = new Vector2(18f, 800f);

        /// <summary>O Mamute Avô's arena (62, map boss): the stone ring on the road from the Boca do Gelo down the galleries.</summary>
        public static readonly Vector2 MamuteAvoArena = new Vector2(984f, 758f);

        /// <summary>O Primeiro Desajeitado's arena (61, map boss): the stone ring on the open Tundra de Omöl, by the trail, among the crooked things Omöl made.</summary>
        public static readonly Vector2 PrimeiroDesajeitadoArena = new Vector2(-956f, 164f);

        /// <summary>A Aurora Presa's arena (63, map boss): the stone ring on a great floe of the Mar das Auroras, by the trail, round the iceberg that holds the light.</summary>
        public static readonly Vector2 AuroraPresaArena = new Vector2(12f, 1120f);
        public static readonly Vector2 CasaDeNeveOffset = new Vector2(0f, 600f);
        public static readonly Vector2 TendaDeYenOffset = new Vector2(-960f, 120f);
        public static readonly Vector2 BocaDoGeloOffset = new Vector2(960f, 600f);
        public static readonly Vector2 FarolDeGeloOffset = new Vector2(480f, 1080f);
        public static readonly Vector2 BuracoGrandeOffset = new Vector2(0f, 1560f);

        /// <summary>Half the side (m) of the flat ground reserved for the Porto da Lua Branca's recipe (±28 m and more, at y = 0).</summary>
        public const float MarGeladoCityGround = 30f;

        /// <summary>The west canal: out of the harbour north-north-west to an open lead in the pack ice.</summary>
        public static readonly Vector2[] CanalOeste =
        {
            new Vector2(-100f, 140f), new Vector2(-130f, 300f), new Vector2(-190f, 470f), new Vector2(-240f, 640f), new Vector2(-270f, 800f), new Vector2(-300f, 900f),
        };

        /// <summary>The east canal: out of the harbour north-north-east to an open lead in the pack ice.</summary>
        public static readonly Vector2[] CanalLeste =
        {
            new Vector2(100f, 140f), new Vector2(140f, 300f), new Vector2(210f, 470f), new Vector2(280f, 640f), new Vector2(330f, 800f), new Vector2(360f, 880f),
        };

        /// <summary>The middle canal: across the north road, under the whalebone bridge north of the boatmen's snow house.</summary>
        public static readonly Vector2[] CanalDoMeio =
        {
            new Vector2(-200f, 700f), new Vector2(-90f, 700f), new Vector2(10f, 715f), new Vector2(120f, 700f), new Vector2(250f, 690f),
        };

        /// <summary>Omöl's round lakes (centre, width) on the tundra.</summary>
        public static readonly (Vector2 A, Vector2 B, float Width)[] OmolLakes =
        {
            (new Vector2(-900f, -300f), new Vector2(-860f, -270f), 40f),
            (new Vector2(-1060f, 0f), new Vector2(-1020f, 40f), 36f),
            (new Vector2(-800f, 230f), new Vector2(-770f, 250f), 34f),
            (new Vector2(-1060f, 460f), new Vector2(-1020f, 490f), 50f),
            (new Vector2(-800f, 700f), new Vector2(-760f, 740f), 44f),
            (new Vector2(-1080f, 780f), new Vector2(-1050f, 810f), 30f),
            (new Vector2(-560f, -400f), new Vector2(-530f, -380f), 30f),
        };

        /// <summary>The seven fishing holes of Old Kul's lake and its Great Hole (the last one).</summary>
        public static readonly (Vector2 A, Vector2 B, float Width)[] KulHoles =
        {
            (new Vector2(-600f, 1420f), new Vector2(-590f, 1425f), 16f),
            (new Vector2(-420f, 1680f), new Vector2(-410f, 1684f), 14f),
            (new Vector2(-200f, 1400f), new Vector2(-190f, 1404f), 16f),
            (new Vector2(240f, 1700f), new Vector2(250f, 1704f), 14f),
            (new Vector2(420f, 1420f), new Vector2(430f, 1424f), 16f),
            (new Vector2(640f, 1680f), new Vector2(650f, 1684f), 14f),
            (new Vector2(-680f, 1700f), new Vector2(-670f, 1704f), 14f),
            (new Vector2(0f, 1690f), new Vector2(15f, 1700f), 36f),
        };

        /// <summary>Open leads in the pack ice of the Mar das Auroras (and the two the north canals end in).</summary>
        public static readonly (Vector2 A, Vector2 B, float Width)[] AuroraLeads =
        {
            (new Vector2(-560f, 1000f), new Vector2(-520f, 1030f), 56f),
            (new Vector2(240f, 1230f), new Vector2(270f, 1250f), 40f),
            (new Vector2(780f, 960f), new Vector2(800f, 990f), 40f),
            (new Vector2(-300f, 900f), new Vector2(-320f, 950f), 50f),
            (new Vector2(360f, 880f), new Vector2(390f, 930f), 50f),
        };

        /// <summary>The region's own rectangle (where the Mar Gelado mechanics apply, in the coordinates of the places below).</summary>
        public static readonly Rect MarGeladoBounds = Rect.MinMaxRect(-1200f, -600f, 1200f, 1800f);

        /// <summary>A zone of the region: its <see cref="OverworldZone.Origin"/> says where the region sits in the stitched world.</summary>
        public const string MarGeladoAnchorZoneId = "porto_da_lua_branca";

        /// <summary>
        /// Where a canal can be crossed while the moon freezes it, or by boat (59: <see cref="MoonCanalRules"/>): a landing on each
        /// bank, 10 m off the canal's axis, and the ice lantern that marks it.
        /// </summary>
        public static readonly SkyRoute[] CanalCrossings =
        {
            CanalFord("travessia_oeste_sul", new Vector2(-169.4f, 381.7f), new Vector2(-150.6f, 388.3f)),
            CanalFord("travessia_leste_sul", new Vector2(165.75f, 388.8f), new Vector2(184.25f, 381.2f)),
            CanalFord("travessia_meio_leste", new Vector2(184.2f, 685f), new Vector2(185.8f, 705f)),
            CanalFord("travessia_oeste_norte", new Vector2(-264.8f, 718.2f), new Vector2(-245.2f, 721.8f)),
            CanalFord("travessia_leste_norte", new Vector2(295.5f, 723f), new Vector2(314.5f, 717f)),
        };

        /// <summary>The thin ice (60: <see cref="ThinIceRules"/>): on the north and the west road, and out on the canals and the cracked ice.</summary>
        public static readonly Rect[] ThinIcePatches =
        {
            Rect.MinMaxRect(-40f, 470f, 30f, 540f),
            Rect.MinMaxRect(-520f, 590f, -440f, 650f),
            Rect.MinMaxRect(-640f, 440f, -540f, 520f),
            Rect.MinMaxRect(380f, 480f, 500f, 600f),
            Rect.MinMaxRect(520f, 650f, 640f, 760f),
        };

        /// <summary>The fishing holes of Old Kul's lake (64: <see cref="IceFishingSession"/>), read off <see cref="KulHoles"/>; the Great Hole is the last.</summary>
        public static readonly IceHole[] IceFishingHoles = BuildIceHoles();

        private static IceHole[] BuildIceHoles()
        {
            var holes = new IceHole[KulHoles.Length];
            for (int i = 0; i < holes.Length; i++)
            {
                (Vector2 a, Vector2 b, float width) = KulHoles[i];
                holes[i] = new IceHole((a + b) * 0.5f, width * 0.5f + Vector2.Distance(a, b) * 0.5f, i == holes.Length - 1);
            }

            return holes;
        }

        private static SkyRoute CanalFord(string id, Vector2 a, Vector2 b)
        {
            Vector2 along = Vector2.Perpendicular((b - a).normalized) * 3f;
            return new SkyRoute { Id = id, Kind = SkyRouteKind.RopeBasket, A = a, B = b, PropA = a + along, PropB = b + along };
        }

        private static class Gelo
        {
            public const string Seal = "enemy_mountain_leopard_seal";
            public const string Walrus = "enemy_mountain_smoke_walrus";
            public const string Gull = "enemy_mountain_ice_gull";
            public const string Fox = "enemy_mountain_arctic_fox";
            public const string IceFish = "enemy_mountain_giant_pike";
            public const string Bear = "enemy_mountain_polar_bear";
            public const string Eel = "enemy_desert_sand_snake";
            public const string Ooze = "enemy_cave_ooze";
            public const string TwigBird = "enemy_mountain_twig_bird";
            public const string Hare = "enemy_mountain_stone_hare";
            public const string Reindeer = "enemy_mountain_clay_reindeer";
            public const string Mosquito = NightVariantRules.FireflySwarmEnemyId;
            public const string Kul = FarBeasts.LittleKul;
            public const string Mammoth = "enemy_mountain_burrow_mammoth";
            public const string Mole = "enemy_mountain_ice_mole";
            public const string Crystal = "enemy_desert_broken_lantern";
            public const string SeaWolf = "enemy_mountain_sea_wolf";
            public const string Floe = FarBeasts.DriftingFloe;
            public const string StormGull = "enemy_mountain_ice_gull";
            public const string ShadowSeal = "enemy_mountain_shadow_seal";
            public const string Irbis = "enemy_mountain_irbis";
        }

        private static OverworldLayout MarGeladoRegion()
        {
            OverworldLayout w = NewRegion(MarGeladoRegionId, MapRegion.MarGelado, Rect.MinMaxRect(-1200f, -600f, 1200f, 1800f));
            w.World.Biome = BiomeType.Mountain;
            const string porto = PortoLuaBrancaMapId;
            const string canais = CanaisGeloMapId;
            const string tundra = TundraOmolMapId;
            const string galerias = GaleriasMamutesMapId;
            const string auroras = MarAurorasMapId;
            const string kul = FundoVelhoKulMapId;
            const string coast = MapRegionTypes.Coast;
            const string plain = MapRegionTypes.Plain;
            const string under = MapRegionTypes.Underground;
            const string blizzard = MapRequirementTags.BlizzardResistance;
            const string thermal = MapRequirementTags.ThermalClothes;

            // ---------------- Zones (a 5 × 5 grid; columns x −1200/−720/−240/240/720/1200, rows z −600/−120/360/840/1320/1800) ----------------
            // South row (towards Mingcheng's wall): the southern tundra, the reindeer pastures, the snowy pass (the way in from
            // Mingcheng), the snow hills, the frozen earth (the way in from the peaks' foot woods).
            OverworldZone zTundraSul = Zone(w, "tundra_do_sul", tundra, null, -1200f, -600f, -720f, -120f, OverworldTerrain.Tundra, 194, 198, 190,
                "Southern Tundra", "Tundra do Sul", plain, spacing: 100f);
            OverworldZone zPastos = Zone(w, "pastos_de_rena", null, tundra, -720f, -600f, -240f, -120f, OverworldTerrain.ReindeerPasture, 182, 186, 178,
                "Reindeer Pastures", "Pastos de Rena", plain, spacing: 95f);
            OverworldZone zPasso = Zone(w, "passo_da_muralha", porto, null, -240f, -600f, 240f, -120f, OverworldTerrain.SnowShore, 180, 184, 176,
                "Pass of the Wall", "Passo da Muralha", coast, spacing: 90f);
            OverworldZone zColinas = Zone(w, "colinas_de_neve", porto, null, 240f, -600f, 720f, -120f, OverworldTerrain.SnowShore, 182, 186, 178,
                "Snow Hills", "Colinas de Neve", coast, spacing: 95f);
            OverworldZone zTerra = Zone(w, "terra_congelada", galerias, null, 720f, -600f, 1200f, -120f, OverworldTerrain.FrozenEarth, 202, 206, 198,
                "Frozen Earth", "Terra Congelada", under, spacing: 100f);
            // Second row: Omöl's tundra and Yen's tent (61), the seals' bay, the Porto da Lua Branca (59), the frozen bay, the fjord of the tusks (62).
            OverworldZone zOmol = Zone(w, "tundra_de_omol", tundra, null, -1200f, -120f, -720f, 360f, OverworldTerrain.Tundra, 194, 200, 190,
                "Omöl's Tundra", "Tundra de Omöl", plain, spacing: 100f);
            OverworldZone zFocas = Zone(w, "baia_das_focas", porto, null, -720f, -120f, -240f, 360f, OverworldTerrain.FrozenBay, 182, 186, 178,
                "Bay of the Seals", "Baía das Focas", coast, spacing: 90f);
            OverworldZone zPorto = Zone(w, "porto_da_lua_branca", porto, null, -240f, -120f, 240f, 360f, OverworldTerrain.MoonHarbor, 180, 183, 176,
                "White Moon Harbor", "Porto da Lua Branca", coast, spacing: 85f);
            OverworldZone zBaia = Zone(w, "baia_gelada", porto, null, 240f, -120f, 720f, 360f, OverworldTerrain.FrozenBay, 182, 187, 178,
                "Frozen Bay", "Baía Gelada", coast, spacing: 90f);
            OverworldZone zPresas = Zone(w, "fiorde_das_presas", galerias, null, 720f, -120f, 1200f, 360f, OverworldTerrain.Fjord, 202, 208, 198,
                "Fjord of the Tusks", "Fiorde das Presas", under, spacing: 100f);
            // Middle row: Omöl's lakes (61), the canals (60) with the boatmen's snow house, the cracked ice, the mammoth galleries (62).
            OverworldZone zLagos = Zone(w, "lagos_de_omol", tundra, null, -1200f, 360f, -720f, 840f, OverworldTerrain.TundraLakes, 197, 204, 193,
                "Omöl's Lakes", "Lagos de Omöl", plain, spacing: 100f);
            OverworldZone zCanaisOeste = Zone(w, "canais_do_oeste", canais, null, -720f, 360f, -240f, 840f, OverworldTerrain.IceCanals, 187, 192, 182,
                "Western Canals", "Canais do Oeste", coast, spacing: 95f);
            OverworldZone zCanais = Zone(w, "canais_de_gelo", canais, null, -240f, 360f, 240f, 840f, OverworldTerrain.IceCanals, 186, 190, 182,
                "Ice Canals", "Canais de Gelo", coast, spacing: 90f);
            OverworldZone zRachado = Zone(w, "gelo_rachado", canais, null, 240f, 360f, 720f, 840f, OverworldTerrain.CrackedIce, 187, 196, 182,
                "Cracked Ice", "Gelo Rachado", coast, spacing: 95f);
            OverworldZone zGalerias = Zone(w, "galerias_dos_mamutes", galerias, null, 720f, 360f, 1200f, 840f, OverworldTerrain.MammothGalleries, 204, 212, 198,
                "Mammoth Galleries", "Galerias dos Mamutes", under, spacing: 100f);
            // Fourth row, the Mar das Auroras (63: Blizzard Resistance): the pack ice, the icebergs, the lighthouse isle, the black fjord.
            OverworldZone zBanquisasOeste = Zone(w, "banquisas_do_oeste", auroras, null, -1200f, 840f, -720f, 1320f, OverworldTerrain.PackIce, 216, 222, 208,
                "Western Pack Ice", "Banquisas do Oeste", coast, blizzard, 100f);
            OverworldZone zBanquisas = Zone(w, "banquisas", auroras, null, -720f, 840f, -240f, 1320f, OverworldTerrain.PackIce, 212, 218, 208,
                "Pack Ice", "Banquisas", coast, blizzard, 100f);
            OverworldZone zAuroras = Zone(w, "mar_das_auroras", auroras, null, -240f, 840f, 240f, 1320f, OverworldTerrain.AuroraSea, 212, 218, 208,
                "Aurora Sea", "Mar das Auroras", coast, blizzard, 100f);
            OverworldZone zFarol = Zone(w, "ilha_do_farol", auroras, null, 240f, 840f, 720f, 1320f, OverworldTerrain.LighthouseIsle, 212, 218, 208,
                "Lighthouse Isle", "Ilha do Farol", coast, blizzard, 100f);
            OverworldZone zFiordeNegro = Zone(w, "fiorde_negro", auroras, null, 720f, 840f, 1200f, 1320f, OverworldTerrain.Fjord, 216, 224, 208,
                "Black Fjord", "Fiorde Negro", coast, blizzard, 100f);
            // North row, Old Kul's Deep (64: Fine Thermal Clothes): the fjord, the fishing holes, the Great Hole, the lake, the black ice.
            OverworldZone zFiordeKul = Zone(w, "fiorde_do_kul", kul, null, -1200f, 1320f, -720f, 1800f, OverworldTerrain.Fjord, 232, 240, 222,
                "Kul's Fjord", "Fiorde do Kul", coast, thermal, 100f);
            OverworldZone zBuracos = Zone(w, "buracos_de_pesca", kul, null, -720f, 1320f, -240f, 1800f, OverworldTerrain.FrozenLake, 226, 234, 222,
                "Fishing Holes", "Buracos de Pesca", coast, thermal, 100f);
            OverworldZone zFundo = Zone(w, "fundo_do_velho_kul", kul, null, -240f, 1320f, 240f, 1800f, OverworldTerrain.FrozenLake, 226, 236, 222,
                "Old Kul's Deep", "Fundo do Velho Kul", coast, thermal, 100f);
            OverworldZone zLagoKul = Zone(w, "lago_do_kul", kul, null, 240f, 1320f, 720f, 1800f, OverworldTerrain.BlackIce, 228, 236, 222,
                "Kul's Lake", "Lago do Kul", coast, thermal, 100f);
            OverworldZone zGeloNegro = Zone(w, "gelo_negro", kul, null, 720f, 1320f, 1200f, 1800f, OverworldTerrain.BlackIce, 232, 240, 222,
                "Black Ice", "Gelo Negro", coast, thermal, 100f);

            AddMarGeladoFauna(zTundraSul, zPastos, zPasso, zColinas, zTerra, zOmol, zFocas, zPorto, zBaia, zPresas, zLagos, zCanaisOeste, zCanais, zRachado,
                zGalerias, zBanquisasOeste, zBanquisas, zAuroras, zFarol, zFiordeNegro, zFiordeKul, zBuracos, zFundo, zLagoKul, zGeloNegro);

            // ---------------- Settlements ----------------
            // The Porto da Lua Branca: the region's return city and the barca's stop for 59–64 (flat ground at y = 0, ±30 m). Then the
            // posts of 60, 61, 62, 63 and 64.
            w.Vila = Place(w, porto, PortoDaLuaBrancaRecipe(), PortoLuaBrancaOffset, Rect.MinMaxRect(-26f, -26f, 26f, 26f), MarGeladoCityGround, true);
            Place(w, canais, Post("casa_de_neve_dos_barqueiros", TownSwatch.LimeWhite, TownSwatch.WoodDark, TownRoofKind.Gable, TownShopDressing.Marcenaria, false),
                CasaDeNeveOffset, PostSafe, 18f, false);
            Place(w, tundra, Post("tenda_de_yen", TownSwatch.WoodWeathered, TownSwatch.Thatch, TownRoofKind.Gable, TownShopDressing.Herder, false),
                TendaDeYenOffset, PostSafe, 18f, false);
            Place(w, galerias, Post("boca_do_gelo", TownSwatch.Stone, TownSwatch.Slate, TownRoofKind.Gable, TownShopDressing.Lamparinas, false),
                BocaDoGeloOffset, PostSafe, 18f, false);
            Place(w, auroras, Post("farol_de_gelo", TownSwatch.LimeWhite, TownSwatch.Slate, TownRoofKind.Gable, TownShopDressing.Storyteller, false),
                FarolDeGeloOffset, PostSafe, 18f, false);
            Place(w, kul, Post("buraco_grande_de_pesca", TownSwatch.WoodDark, TownSwatch.WoodWeathered, TownRoofKind.Gable, TownShopDressing.Mercearia, false),
                BuracoGrandeOffset, PostSafe, 18f, false);

            // ---------------- Water ----------------
            // The canals (10 m, carved: black water): the west one crossed by the snow house's west road on a whalebone bridge, the
            // middle one by the north road. Omöl's lakes, the open leads in the pack ice, Old Kul's fishing holes and the Great Hole.
            MapRiver canalOeste = River(w, 10f, CanalOeste);
            Vector2 westBridge = On(CanalOeste[2], CanalOeste[3], 0.853f);
            Crossing(canalOeste, westBridge, 4.5f, true);
            River(w, 10f, CanalLeste);
            MapRiver canalMeio = River(w, 10f, CanalDoMeio);
            Vector2 northBridge = CanalDoMeio[2];
            Crossing(canalMeio, northBridge, 4.5f, true);
            foreach ((Vector2 a, Vector2 b, float width) in OmolLakes)
            {
                River(w, width, a, b);
            }

            foreach ((Vector2 a, Vector2 b, float width) in AuroraLeads)
            {
                River(w, width, a, b);
            }

            foreach ((Vector2 a, Vector2 b, float width) in KulHoles)
            {
                River(w, width, a, b);
            }

            // ---------------- Gates out of the region ----------------
            // South: the pass down to Mingcheng's north road through the wall (map 42's area); east: two passes to the Picos de
            // Farnkesh (the foot woods, map 53's area, and the pinnacle wood, map 56's).
            Gate(w, "exit_muralha", MapExitKind.Map, TorresBrancasMapId, Rect.MinMaxRect(32f, -600f, 48f, -597f), new Vector2(40f, -591f), Vector2.up);
            Gate(w, "exit_pe_dos_picos", MapExitKind.Map, ValeSatirosMapId, Rect.MinMaxRect(1197f, -428f, 1200f, -412f), new Vector2(1191f, -420f), Vector2.left);
            Gate(w, "exit_pinaculos", MapExitKind.Map, ErmidasMapId, Rect.MinMaxRect(1197f, 552f, 1200f, 568f), new Vector2(1191f, 560f), Vector2.left);
            Gate(w, "exit_island", MapExitKind.WorldMap, null, Rect.MinMaxRect(-82f, 78f, -78f, 82f), new Vector2(-80f, 70f), Vector2.up);
            Landmark(w, MapLandmarkKind.StoneRing, -80f, 80f, 0f, 0.45f);

            // ---------------- Trails ----------------
            float g = MarGeladoCityGround;
            // The south road from the harbour to the pass of the wall (Mingcheng's north road).
            PathVia(w, 4f, new Vector2(0f, -g), new Vector2(20f, -200f), new Vector2(40f, -400f), new Vector2(40f, -591f));
            // The west road along the seals' bay to Yen's tent; a loop south through the pastures and the southern tundra.
            Vector2 westFork = new Vector2(-200f, -20f);
            PathVia(w, 4f, new Vector2(-g, 0f), new Vector2(-110f, -10f), westFork, new Vector2(-480f, 40f), new Vector2(-760f, 100f), TendaDeYenOffset + new Vector2(18f, 0f));
            PathVia(w, 3.5f, westFork, new Vector2(-420f, -260f), new Vector2(-700f, -420f), new Vector2(-960f, -400f));
            // The east road along the frozen bay and the snow hills to the pass of the foot woods; a branch up to the Boca do Gelo
            // and on to the pass of the pinnacle wood.
            Vector2 eastFork = new Vector2(220f, -30f);
            PathVia(w, 4f, new Vector2(g, 0f), new Vector2(110f, -10f), eastFork, new Vector2(480f, -130f), new Vector2(760f, -260f), new Vector2(1000f, -400f),
                new Vector2(1191f, -420f));
            Vector2 bayFork = new Vector2(420f, 150f);
            PathVia(w, 3.5f, eastFork, bayFork, new Vector2(700f, 300f), new Vector2(900f, 430f), BocaDoGeloOffset + new Vector2(0f, -18f));
            PathVia(w, 3.5f, BocaDoGeloOffset + new Vector2(18f, 0f), new Vector2(1100f, 570f), new Vector2(1191f, 560f));
            // The north road (from the east road: the city has no north street) between the canals to the boatmen's snow house, over the whalebone bridge, out on the aurora sea and
            // over the ice to the Great Fishing Hole.
            Vector2 seaFork = new Vector2(40f, 960f);
            PathVia(w, 4f, On(new Vector2(g, 0f), new Vector2(110f, -10f), 0.5f), new Vector2(60f, 100f), new Vector2(0f, 220f), new Vector2(-10f, 420f),
                CasaDeNeveOffset + new Vector2(0f, -18f));
            PathVia(w, 4f, CasaDeNeveOffset + new Vector2(0f, 18f), northBridge, new Vector2(20f, 820f), seaFork, new Vector2(20f, 1200f), new Vector2(10f, 1400f),
                BuracoGrandeOffset + new Vector2(0f, -18f));
            // The snow house's west road over the west canal to Omöl's lakes; up from Yen's tent to the lakes; north along the
            // tundra's coast to the western pack ice and Kul's fjord.
            Vector2 lakesFork = new Vector2(-880f, 540f);
            Vector2 westSea = new Vector2(-900f, 1150f);
            PathVia(w, 3.5f, CasaDeNeveOffset + new Vector2(-18f, 0f), westBridge, new Vector2(-400f, 630f), new Vector2(-600f, 600f), lakesFork);
            PathVia(w, 3.5f, TendaDeYenOffset + new Vector2(0f, 18f), new Vector2(-1000f, 300f), new Vector2(-920f, 460f), lakesFork);
            PathVia(w, 3.5f, lakesFork, new Vector2(-960f, 900f), westSea, new Vector2(-900f, 1500f), new Vector2(-880f, 1700f));
            PathVia(w, 3.5f, seaFork, new Vector2(-300f, 1080f), new Vector2(-600f, 1150f), westSea);
            // The lighthouse road; on east to the black fjord and Kul's black ice; from the Boca do Gelo north to the black fjord.
            Vector2 eastSea = new Vector2(980f, 1150f);
            PathVia(w, 3.5f, seaFork, new Vector2(240f, 1040f), FarolDeGeloOffset + new Vector2(-18f, 0f));
            PathVia(w, 3.5f, FarolDeGeloOffset + new Vector2(18f, 0f), new Vector2(760f, 1100f), eastSea, new Vector2(1000f, 1500f), new Vector2(980f, 1700f));
            PathVia(w, 3.5f, BocaDoGeloOffset + new Vector2(0f, 18f), new Vector2(1000f, 850f), eastSea);
            // Over Old Kul's lake from the Great Hole west and east to the fjord and the black ice.
            PathVia(w, 3.5f, BuracoGrandeOffset + new Vector2(-18f, 0f), new Vector2(-300f, 1560f), new Vector2(-600f, 1520f), new Vector2(-900f, 1500f));
            PathVia(w, 3.5f, BuracoGrandeOffset + new Vector2(18f, 0f), new Vector2(300f, 1560f), new Vector2(600f, 1540f), new Vector2(1000f, 1500f));

            // ---------------- Landmarks ----------------
            Sign(w, westFork, new Vector2(-420f, -260f), 4f, 0f);
            Sign(w, eastFork, bayFork, 4f, 0f);
            Sign(w, bayFork, new Vector2(700f, 300f), 3.5f, 0f);
            Sign(w, seaFork, new Vector2(240f, 1040f), 4f, 0f);
            Sign(w, lakesFork, new Vector2(-960f, 900f), 3.5f, 0f);
            Sign(w, westSea, new Vector2(-900f, 1500f), 3.5f, 0f);
            Sign(w, eastSea, new Vector2(1000f, 1500f), 3.5f, 0f);
            Landmark(w, MapLandmarkKind.Campfire, -480f, 80f, 0f, 0.9f);
            Landmark(w, MapLandmarkKind.Campfire, 600f, -160f, 0f, 0.9f);
            Landmark(w, MapLandmarkKind.Campfire, -560f, 560f, 0f, 1f);
            Landmark(w, MapLandmarkKind.Campfire, 160f, 920f, 0f, 1f);
            Landmark(w, MapLandmarkKind.Campfire, -300f, 1500f, 0f, 1f);
            Landmark(w, MapLandmarkKind.Campfire, 700f, 1180f, 0f, 1f);
            Landmark(w, MapLandmarkKind.StoneRing, -420f, 220f, 0f, 1f);
            Landmark(w, MapLandmarkKind.StoneRing, 860f, 1400f, 0f, 0.9f);
            Landmark(w, MapLandmarkKind.StoneRing, VelhoKulArena.x, VelhoKulArena.y, 0f, 1.1f);
            Landmark(w, MapLandmarkKind.StoneRing, MorsaRainhaArena.x, MorsaRainhaArena.y, 0f, 1.1f);
            Landmark(w, MapLandmarkKind.StoneRing, MamuteAvoArena.x, MamuteAvoArena.y, 0f, 1.1f);
            Landmark(w, MapLandmarkKind.StoneRing, PrimeiroDesajeitadoArena.x, PrimeiroDesajeitadoArena.y, 0f, 1.1f);
            Landmark(w, MapLandmarkKind.StoneRing, AuroraPresaArena.x, AuroraPresaArena.y, 0f, 1.1f);

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
        // The town of the frozen sea (its own recipe, Cities/Town: the IceCanal style, inside a radius of 22.5 m on its reserved
        // flat ground of ±30 m at y = 0; no backdrop: the frozen sea is all round it). Its streets leave W, E and S (no north
        // street: the frozen bay), so the north road leaves from the east road.
        // ------------------------------------------------------------------

        private static CityTownRecipe PortoDaLuaBrancaRecipe() => NoBackdrop(CityTownRecipes.PortoDaLuaBranca());

        // ------------------------------------------------------------------
        // Monster groups per area (existing definitions only, no boss; Mapas100 59–64 archetypes)
        // ------------------------------------------------------------------

        private static void AddMarGeladoFauna(OverworldZone tundraSul, OverworldZone pastos, OverworldZone passo, OverworldZone colinas, OverworldZone terra,
            OverworldZone omol, OverworldZone focas, OverworldZone porto, OverworldZone baia, OverworldZone presas, OverworldZone lagos, OverworldZone canaisOeste,
            OverworldZone canais, OverworldZone rachado, OverworldZone galerias, OverworldZone banquisasOeste, OverworldZone banquisas, OverworldZone auroras,
            OverworldZone farol, OverworldZone fiordeNegro, OverworldZone fiordeKul, OverworldZone buracos, OverworldZone fundo, OverworldZone lagoKul,
            OverworldZone geloNegro)
        {
            // At night: the snowy owl (the shroud owl) over the shore and the tundra, Omöl's twig bird (the moon-mother) on the
            // lakes, the vampire bats in the galleries, the sea wolves run as werewolves on the pack ice, the aurora's sparks (the
            // fireflies) over the sea and the lake; Omöl's creatures sleep standing (their groups thin out at night).
            NightSpecies werewolf = NightVariantRules.Lobisomem;
            NightSpecies owl = NightVariantRules.RasgaMortalha;
            NightSpecies bats = NightVariantRules.MorcegoVampiro;
            NightSpecies moon = NightVariantRules.MaeDaLua;
            NightSpecies sparks = NightVariantRules.VagaLume;

            // Porto da Lua Branca, the bays, the pass and the snow hills (59): the Foca-Leopardo Apagada [rápido], the
            // Morsa de Fumaça [tanque], the Gaivota-do-Gelo [distância], the Raposa-do-Ártico Ladra [rápido].
            porto.Fauna.Add(Fauna("gaivotas_do_gelo", "[distância]", T(N(Gelo.Gull, 3), N(Gelo.Fox, 1)), T(V(Gelo.Gull, 2), S(owl, 1)), 4));
            porto.Fauna.Add(Fauna("raposas_do_artico", "[rápido]", T(N(Gelo.Fox, 3), N(Gelo.Gull, 1)), T(V(Gelo.Fox, 2), S(owl, 1)), 4));
            focas.Fauna.Add(Fauna("focas_leopardo", "[rápido]", T(N(Gelo.Seal, 3), N(Gelo.Gull, 1)), T(V(Gelo.Seal, 2), S(moon, 1)), 4));
            focas.Fauna.Add(Fauna("morsas_de_fumaca", "[tanque]", T(N(Gelo.Walrus, 3), N(Gelo.Seal, 1)), T(V(Gelo.Walrus, 2), S(moon, 1)), 3, null, 30f));
            baia.Fauna.Add(Fauna("morsas_da_baia", "[tanque]", T(N(Gelo.Walrus, 3), N(Gelo.Gull, 1)), T(V(Gelo.Walrus, 2), S(owl, 1)), 3, null, 30f));
            baia.Fauna.Add(Fauna("focas_da_baia", "[rápido]", T(N(Gelo.Seal, 3), N(Gelo.Fox, 1)), T(V(Gelo.Seal, 2), S(moon, 1)), 4));
            passo.Fauna.Add(Fauna("raposas_da_muralha", "[rápido]", T(N(Gelo.Fox, 3), N(Gelo.Gull, 1)), T(V(Gelo.Fox, 2), S(owl, 1)), 4));
            passo.Fauna.Add(Fauna("gaivotas_da_muralha", "[distância]", T(N(Gelo.Gull, 3), N(Gelo.Fox, 1)), T(V(Gelo.Gull, 2), S(owl, 1)), 3));
            colinas.Fauna.Add(Fauna("raposas_das_colinas", "[rápido]", T(N(Gelo.Fox, 3), N(Gelo.Walrus, 1)), T(V(Gelo.Fox, 2), S(owl, 1)), 4));
            colinas.Fauna.Add(Fauna("morsas_das_colinas", "[tanque]", T(N(Gelo.Walrus, 3), N(Gelo.Fox, 1)), T(V(Gelo.Walrus, 2), S(werewolf, 1)), 3, null, 30f));

            // The reindeer pastures: sea wolves after the herd, the clay reindeer; the werewolf at night.
            pastos.Fauna.Add(Fauna("lobos_dos_pastos", "[enxame]", T(N(Gelo.SeaWolf, 3), N(Gelo.Fox, 1)), T(V(Gelo.SeaWolf, 2), S(werewolf, 1)), 5));
            pastos.Fauna.Add(Fauna("renas_dos_pastos", "[tanque]", T(N(Gelo.Reindeer, 3), N(Gelo.Fox, 1)), T(V(Gelo.Reindeer, 2), S(owl, 1)), 3, null, 28f));

            // Canais de Gelo, Canais do Oeste, Gelo Rachado (60): the Peixe-Gelo [enxame] (the giant pike), the Urso-Polar Apagado
            // [tanque], the Enguia-Lanterna [distância] (the sand snake), the Rachadura [controle] (the ooze); the eels
            // light up at night.
            canais.Fauna.Add(Fauna("peixes_gelo", "[enxame]", T(N(Gelo.IceFish, 3), N(Gelo.Eel, 1)), T(V(Gelo.IceFish, 2), S(moon, 1)), 5));
            canais.Fauna.Add(Fauna("enguias_lanterna", "[distância]", T(N(Gelo.Eel, 3), N(Gelo.IceFish, 1)), T(V(Gelo.Eel, 3), S(moon, 1)), 3));
            canaisOeste.Fauna.Add(Fauna("ursos_polares", "[tanque]", T(N(Gelo.Bear, 3), N(Gelo.IceFish, 1)), T(V(Gelo.Bear, 2), S(owl, 1)), 3, null, 30f));
            canaisOeste.Fauna.Add(Fauna("peixes_gelo_do_oeste", "[enxame]", T(N(Gelo.IceFish, 3), N(Gelo.Ooze, 1)), T(V(Gelo.Eel, 2), S(moon, 1)), 5));
            rachado.Fauna.Add(Fauna("rachaduras", "[controle]", T(N(Gelo.Ooze, 3), N(Gelo.Bear, 1)), T(V(Gelo.Ooze, 3), S(owl, 1)), 4));
            rachado.Fauna.Add(Fauna("ursos_do_gelo_rachado", "[tanque]", T(N(Gelo.Bear, 3), N(Gelo.Ooze, 1)), T(V(Gelo.Bear, 2), S(werewolf, 1)), 3, null, 30f));

            // Tundra de Omöl, Tundra do Sul, Lagos de Omöl (61): Omöl's clumsy creatures — the twig bird [distância], the
            // stone hare [rápido], the clay reindeer [tanque] — and the tundra mosquito [enxame]
            // (the firefly swarm); at night they sleep standing: thinner groups, the owl and the moon-mother.
            omol.Fauna.Add(Fauna("aves_graveto", "[distância]", T(N(Gelo.TwigBird, 3), N(Gelo.Hare, 1)), T(V(Gelo.TwigBird, 1), S(moon, 1)), 3));
            omol.Fauna.Add(Fauna("lebres_pedra", "[rápido]", T(N(Gelo.Hare, 3), N(Gelo.Reindeer, 1)), T(V(Gelo.Hare, 1), S(owl, 1)), 4));
            tundraSul.Fauna.Add(Fauna("renas_de_barro", "[tanque]", T(N(Gelo.Reindeer, 3), N(Gelo.Hare, 1)), T(V(Gelo.Reindeer, 1), S(owl, 1)), 3, null, 28f));
            tundraSul.Fauna.Add(Fauna("mosquitos_da_tundra", "[enxame]", T(N(Gelo.Mosquito, 3), N(Gelo.TwigBird, 1)), T(V(Gelo.Mosquito, 1), S(moon, 1)), 5));
            lagos.Fauna.Add(Fauna("mosquitos_dos_lagos", "[enxame]", T(N(Gelo.Mosquito, 3), N(Gelo.Hare, 1)), T(V(Gelo.Mosquito, 2), S(moon, 1)), 5));
            lagos.Fauna.Add(Fauna("aves_graveto_dos_lagos", "[distância]", T(N(Gelo.TwigBird, 3), N(Gelo.Reindeer, 1)), T(V(Gelo.TwigBird, 1), S(moon, 2)), 3));

            // Terra Congelada, Fiorde das Presas, Galerias dos Mamutes (62): the Mamute de Baixo da Terra [elite], the
            // Toupeira-Gelo [enxame] (its own beast now, BorrowedBeasts), the Cristal Vivo [distância] (the broken lantern), the Raiz-Gelo [controle]
            // (the ooze); the irbis on the fjord's ledges; the vampire bats at night.
            galerias.Fauna.Add(Fauna("mamutes_de_baixo_da_terra", "[elite]", T(N(Gelo.Mammoth, 3), N(Gelo.Mole, 1)), T(V(Gelo.Mammoth, 2), S(bats, 1)), 3, null, 32f));
            galerias.Fauna.Add(Fauna("cristais_vivos", "[distância]", T(N(Gelo.Crystal, 3), N(Gelo.Ooze, 1)), T(V(Gelo.Crystal, 2), S(bats, 1)), 3));
            terra.Fauna.Add(Fauna("toupeiras_gelo", "[enxame]", T(N(Gelo.Mole, 3), N(Gelo.Ooze, 1)), T(V(Gelo.Mole, 2), S(bats, 1)), 5));
            terra.Fauna.Add(Fauna("raizes_gelo", "[controle]", T(N(Gelo.Ooze, 3), N(Gelo.Mole, 1)), T(V(Gelo.Ooze, 2), S(werewolf, 1)), 4));
            presas.Fauna.Add(Fauna("irbis_das_presas", "[rápido]", T(N(Gelo.Irbis, 3), N(Gelo.Mole, 1)), T(V(Gelo.Irbis, 2), S(werewolf, 1)), 4));
            presas.Fauna.Add(Fauna("mamutes_do_fiorde", "[elite]", T(N(Gelo.Mammoth, 3), N(Gelo.Crystal, 1)), T(V(Gelo.Mammoth, 2), S(bats, 1)), 3, null, 32f));

            // Banquisas, Mar das Auroras, Ilha do Farol, Fiorde Negro (63): the Narval Apagado [rápido] (the leopard seal), the
            // Lobo-do-Mar [enxame], the Gelo Flutuante [controle] (its own beast now, FarBeasts), the Gaivota-Tempestade [distância]
            // (the ice gull); the aurora's sparks and the sea werewolves at night.
            banquisas.Fauna.Add(Fauna("lobos_do_mar", "[enxame]", T(N(Gelo.SeaWolf, 3), N(Gelo.Seal, 1)), T(V(Gelo.SeaWolf, 2), S(werewolf, 1)), 5));
            banquisas.Fauna.Add(Fauna("narvais_apagados", "[rápido]", T(N(Gelo.Seal, 3), N(Gelo.Floe, 1)), T(V(Gelo.Seal, 2), S(sparks, 1)), 4));
            banquisasOeste.Fauna.Add(Fauna("gelos_flutuantes", "[controle]", T(N(Gelo.Floe, 3), N(Gelo.SeaWolf, 1)), T(V(Gelo.Floe, 2), S(sparks, 1)), 4));
            banquisasOeste.Fauna.Add(Fauna("lobos_das_banquisas", "[enxame]", T(N(Gelo.SeaWolf, 3), N(Gelo.Floe, 1)), T(V(Gelo.SeaWolf, 2), S(werewolf, 1)), 5));
            auroras.Fauna.Add(Fauna("gaivotas_tempestade", "[distância]", T(N(Gelo.StormGull, 3), N(Gelo.Seal, 1)), T(V(Gelo.StormGull, 2), S(sparks, 1)), 3));
            auroras.Fauna.Add(Fauna("gelos_das_auroras", "[controle]", T(N(Gelo.Floe, 3), N(Gelo.StormGull, 1)), T(V(Gelo.Floe, 2), S(sparks, 2)), 4));
            farol.Fauna.Add(Fauna("gaivotas_do_farol", "[distância]", T(N(Gelo.StormGull, 3), N(Gelo.SeaWolf, 1)), T(V(Gelo.StormGull, 2), S(owl, 1)), 3));
            farol.Fauna.Add(Fauna("narvais_do_farol", "[rápido]", T(N(Gelo.Seal, 3), N(Gelo.StormGull, 1)), T(V(Gelo.Seal, 2), S(sparks, 1)), 4));
            fiordeNegro.Fauna.Add(Fauna("irbis_do_fiorde_negro", "[rápido]", T(N(Gelo.Irbis, 3), N(Gelo.StormGull, 1)), T(V(Gelo.Irbis, 2), S(werewolf, 1)), 4));
            fiordeNegro.Fauna.Add(Fauna("ursos_do_fiorde_negro", "[tanque]", T(N(Gelo.Bear, 3), N(Gelo.Floe, 1)), T(V(Gelo.Bear, 2), S(bats, 1)), 3, null, 30f));

            // Fundo do Velho Kul, Buracos de Pesca, Lago do Kul, Gelo Negro, Fiorde do Kul (64): the Lúcio Gigante [rápido], the
            // Kul Pequeno [enxame] (its own beast now, FarBeasts: little water spirits that glow), the Peixe-Bruxa [controle] (the
            // ooze), the Foca-Sombra [tanque]; the holes glow at night (the moon-mother, the sparks).
            fundo.Fauna.Add(Fauna("lucios_gigantes", "[rápido]", T(N(Gelo.IceFish, 3), N(Gelo.Kul, 1)), T(V(Gelo.IceFish, 2), S(moon, 1)), 4));
            fundo.Fauna.Add(Fauna("kuls_pequenos", "[enxame]", T(N(Gelo.Kul, 3), N(Gelo.IceFish, 1)), T(V(Gelo.Kul, 3), S(sparks, 1)), 5));
            buracos.Fauna.Add(Fauna("peixes_bruxa", "[controle]", T(N(Gelo.Ooze, 3), N(Gelo.IceFish, 1)), T(V(Gelo.Ooze, 2), S(moon, 1)), 4));
            buracos.Fauna.Add(Fauna("lucios_dos_buracos", "[rápido]", T(N(Gelo.IceFish, 3), N(Gelo.Ooze, 1)), T(V(Gelo.IceFish, 2), S(sparks, 1)), 4));
            lagoKul.Fauna.Add(Fauna("focas_sombra", "[tanque]", T(N(Gelo.ShadowSeal, 3), N(Gelo.Ooze, 1)), T(V(Gelo.ShadowSeal, 2), S(moon, 1)), 3, null, 30f));
            lagoKul.Fauna.Add(Fauna("kuls_do_lago", "[enxame]", T(N(Gelo.Kul, 3), N(Gelo.ShadowSeal, 1)), T(V(Gelo.Kul, 3), S(sparks, 1)), 5));
            geloNegro.Fauna.Add(Fauna("focas_do_gelo_negro", "[tanque]", T(N(Gelo.ShadowSeal, 3), N(Gelo.Floe, 1)), T(V(Gelo.ShadowSeal, 2), S(bats, 1)), 3, null, 30f));
            geloNegro.Fauna.Add(Fauna("peixes_bruxa_do_gelo_negro", "[controle]", T(N(Gelo.Ooze, 3), N(Gelo.ShadowSeal, 1)), T(V(Gelo.Ooze, 2), S(moon, 1)), 4));
            fiordeKul.Fauna.Add(Fauna("irbis_do_kul", "[rápido]", T(N(Gelo.Irbis, 3), N(Gelo.ShadowSeal, 1)), T(V(Gelo.Irbis, 2), S(werewolf, 1)), 4));
            fiordeKul.Fauna.Add(Fauna("ursos_do_kul", "[tanque]", T(N(Gelo.Bear, 3), N(Gelo.Irbis, 1)), T(V(Gelo.Bear, 2), S(bats, 1)), 3, null, 30f));
        }
    }
}
