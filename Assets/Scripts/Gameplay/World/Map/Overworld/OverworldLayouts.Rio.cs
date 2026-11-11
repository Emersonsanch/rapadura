using Rapadura.Gameplay.Night;
using Rapadura.Gameplay.World.Biomes;
using Rapadura.Gameplay.World.Cities.Town;
using Rapadura.Gameplay.World.Map.Areas;
using Rapadura.Gameplay.World.Map.Mechanics;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Overworld
{
    /// <summary>
    /// Região 13 — Rio das Doze Horas as ONE continuous map of 2400 × 2400 m (Docs/Mapas100.md §0.10 and §5, maps 71–76), no scene change
    /// between the quay and Apep's sandbank. The Cais de Mesektet (71, the region's capital, level 260) stands at the plaza (0, 0), 600 m
    /// from the south edge where two passes come over the ash from the Arquipélago de Hinokura's north row (in the stitched world the river
    /// of the night grows a new row of the map north of the archipelago, where the ash of the Devouring Fire runs down to the black water);
    /// the river rises towards the north, each map an "hour":
    /// <list type="bullet">
    /// <item>round the quay (nv 260–267, map 71): the <b>Cais de Mesektet</b>, the <b>Margem de Papiro</b>, the <b>Foz das Cinzas</b> and
    /// the <b>Praia dos Remadores</b> (the ways in from Hinokura), the <b>Píeres das Horas</b> and the <b>Canal da Barca</b> along the river;</item>
    /// <item>north of the river, the reed fields (nv 266–276, map 72: a Reed Boat for the canals): the <b>Campos de Juncos</b> with the
    /// Casa de Junco (post), the <b>Canteiros de Juncos</b>, the <b>Trigais de Luz</b> and the <b>Canais de Sekhet</b>;</item>
    /// <item>the sands on both flanks (nv 275–283, map 73: a lit lamp in the dark): the <b>Areal Escondido</b> with the Trenó de Areia
    /// (post), the <b>Portas Fechadas</b>, the <b>Zigue-Zague do Areal</b> and the <b>Caminho das Serpentes</b>;</item>
    /// <item>the coils to the north-west (nv 284–296, map 74: a Runic Lamp): the <b>Anel Externo</b>, the <b>Anéis de Mehen</b> with the
    /// Anel Quieto (post, at the centre of the rings) and the <b>Nó Errado</b>;</item>
    /// <item>the lake to the north-east (nv 292–306, map 75: Eternal Fire Oil): the <b>Beira Fervente</b>, the <b>Praia de Brasa</b> and the
    /// <b>Lago de Fogo</b> with the Ilha das Palmeiras (post, lobes of the lake on three sides);</item>
    /// <item>the north row, the dry bed of the river (nv 304–320, map 76: an Anti-Venom Amulet): the <b>Banco de Areia de Apep</b> with the
    /// Proa Encalhada (post), the <b>Corpo de Apep</b>, the <b>Rio Seco</b>, the <b>Escamas Roxas</b> and the <b>Areia Movediça</b>.</item>
    /// </list>
    /// Water: the dark river north of the quay (carved, two bridges on the north roads), the two irrigation canals of the fields (bridged on
    /// the same roads) and a few ponds. Fire: three lobes of the Lake of Fire and two small fire ponds, drawn glowing like the lava of
    /// Hinokura (<see cref="IsLava"/>). Monsters: the region's own beasts of maps 71–74 and the scarab of 76, existing beasts that still fit
    /// the other sheets (see <see cref="Rio"/>) and the night species, a night table in every clearing; no boss (A Barca Atrasou, the Hipopótamo-de-Junco, the Grande Serpente, O Nó Errado, O Guardião do Lago and
    /// Apep are not placed).
    /// </summary>
    public static partial class OverworldLayouts
    {
        public const string RioRegionId = "overworld_rio_doze_horas";

        public const string CaisMesektetMapId = "map_071_cais_mesektet";
        public const string CamposJuncosMapId = "map_072_campos_juncos";
        public const string ArealEscondidoMapId = "map_073_areal_escondido";
        public const string AneisMehenMapId = "map_074_aneis_mehen";
        public const string LagoDeFogoMapId = "map_075_lago_de_fogo";
        public const string BancoApepMapId = "map_076_banco_apep";

        /// <summary>Apep's arena (76): a stone ring on the flat of the sandbank east of the Proa Encalhada, south of her spine (so no climb from the road).</summary>
        public static readonly Vector2 ApepArena = new Vector2(170f, 1590f);

        public static readonly Vector2 CaisMesektetOffset = Vector2.zero;
        public static readonly Vector2 CasaDeJuncoOffset = new Vector2(0f, 600f);
        public static readonly Vector2 TrenoDeAreiaOffset = new Vector2(-960f, 120f);
        public static readonly Vector2 AnelQuietoOffset = new Vector2(-480f, 1080f);
        public static readonly Vector2 IlhaPalmeirasOffset = new Vector2(800f, 1090f);
        public static readonly Vector2 ProaEncalhadaOffset = new Vector2(0f, 1560f);

        /// <summary>The centre of Mehen's rings (region coordinates; the post of the Anel Quieto stands on it).</summary>
        public static readonly Vector2 MehenCenter = AnelQuietoOffset;

        /// <summary>Half the side (m) of the flat ground reserved for the Cais' recipe (±30 m at y = 0).</summary>
        public const float CaisMesektetGround = 30f;

        /// <summary>The dark river north of the quay (the bank the city's north side drops to).</summary>
        public static readonly Vector2[] RioNoturno =
        {
            new Vector2(-700f, 240f), new Vector2(-350f, 200f), new Vector2(0f, 230f), new Vector2(350f, 200f), new Vector2(700f, 240f),
        };

        /// <summary>A zone of the region: its <see cref="OverworldZone.Origin"/> says where the region sits in the stitched world.</summary>
        public const string RioAnchorZoneId = "cais_de_mesektet";

        /// <summary>The region in its own coordinates (the rectangle of <see cref="RioRegion"/>).</summary>
        public static readonly Rect RioBounds = Rect.MinMaxRect(-1200f, -600f, 1200f, 1800f);

        /// <summary>Distance (m) from the river's centre line to a pier's landing on the south bank (the river is 14 m wide).</summary>
        private const float PierSetback = 14f;

        /// <summary>The twelve piers of the hours along the south bank, west to east, 70 m apart and clear of the two bridges (<see cref="RioHourRules"/>).</summary>
        public static readonly Vector2[] HourPiers =
        {
            RioPier(-385f), RioPier(-315f), RioPier(-245f), RioPier(-175f), RioPier(-105f), RioPier(-35f),
            RioPier(35f), RioPier(105f), RioPier(175f), RioPier(245f), RioPier(315f), RioPier(385f),
        };

        /// <summary>The barque's berths at the posts of 72–76, in the order of <see cref="RioHourRules.DestinationOf"/>: just off each post's ground, and each one also the way back.</summary>
        public static readonly Vector2[] BarqueBerths =
        {
            CasaDeJuncoOffset + new Vector2(10f, -26f), TrenoDeAreiaOffset + new Vector2(10f, -26f), AnelQuietoOffset + new Vector2(10f, -26f),
            IlhaPalmeirasOffset + new Vector2(10f, -26f), ProaEncalhadaOffset + new Vector2(10f, -26f),
        };

        /// <summary>Where the return barque leaves the hero: on the south road, just outside the Cais.</summary>
        public static readonly Vector2 QuayBerth = new Vector2(4f, -46f);

        /// <summary>Radius (m) of the troughs either side of Mehen's second scale wall, joined by <see cref="MehenGates"/>.</summary>
        public const float MehenGateInner = OverworldRelief.MehenRingPeriod;
        public const float MehenGateOuter = OverworldRelief.MehenRingPeriod * 2f;

        /// <summary>The eight gates over the wall of the turning ring (<see cref="RioMehenRules"/>); index = gate, 45° apart, A inside and B outside.</summary>
        public static readonly SkyRoute[] MehenGates = NewMehenGates();

        private static Vector2 RioPier(float x)
        {
            for (int i = 0; i + 1 < RioNoturno.Length; i++)
            {
                if (x <= RioNoturno[i + 1].x)
                {
                    float t = Mathf.InverseLerp(RioNoturno[i].x, RioNoturno[i + 1].x, x);
                    return new Vector2(x, Mathf.Lerp(RioNoturno[i].y, RioNoturno[i + 1].y, t) - PierSetback);
                }
            }

            return new Vector2(x, RioNoturno[RioNoturno.Length - 1].y - PierSetback);
        }

        private static SkyRoute[] NewMehenGates()
        {
            var gates = new SkyRoute[RioMehenRules.Gates];
            for (int i = 0; i < gates.Length; i++)
            {
                float angle = i * RioMehenRules.DegreesPerStep * Mathf.Deg2Rad;
                Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                Vector2 inner = MehenCenter + direction * MehenGateInner;
                Vector2 outer = MehenCenter + direction * MehenGateOuter;
                gates[i] = new SkyRoute { Id = "mehen_gate_" + i, Kind = SkyRouteKind.RopeBasket, A = inner, B = outer, PropA = inner, PropB = outer };
            }

            return gates;
        }

        /// <summary>The south irrigation canal of the fields (z 500).</summary>
        public static readonly Vector2[] CanalSekhetSul =
        {
            new Vector2(-420f, 500f), new Vector2(-150f, 500f), new Vector2(150f, 500f), new Vector2(420f, 500f),
        };

        /// <summary>The north irrigation canal of the fields (z 760).</summary>
        public static readonly Vector2[] CanalSekhetNorte =
        {
            new Vector2(-420f, 760f), new Vector2(0f, 760f), new Vector2(420f, 760f),
        };

        /// <summary>Water ponds (centre segment, width): the reed beds, the light wheat, the banks, the canals of Sekhet.</summary>
        public static readonly (Vector2 A, Vector2 B, float Width)[] RioPools =
        {
            (new Vector2(-640f, 500f), new Vector2(-628f, 506f), 24f),
            (new Vector2(640f, 500f), new Vector2(652f, 505f), 22f),
            (new Vector2(640f, -380f), new Vector2(652f, -374f), 22f),
            (new Vector2(-620f, -440f), new Vector2(-610f, -434f), 22f),
            (new Vector2(-140f, 1000f), new Vector2(-128f, 1006f), 20f),
            (new Vector2(140f, 970f), new Vector2(150f, 976f), 20f),
        };

        /// <summary>Fire (centre segment, width): the three lobes of the Lake of Fire round the Ilha das Palmeiras and two small fire ponds.</summary>
        public static readonly (Vector2 A, Vector2 B, float Width)[] LagoDeFogoPools =
        {
            (new Vector2(800f, 1240f), new Vector2(860f, 1250f), 130f),
            (new Vector2(800f, 935f), new Vector2(860f, 925f), 120f),
            (new Vector2(1000f, 1090f), new Vector2(1060f, 1100f), 140f),
            (new Vector2(1060f, 560f), new Vector2(1066f, 566f), 22f),
            (new Vector2(470f, 970f), new Vector2(480f, 976f), 20f),
        };

        /// <summary>The region's own beasts (Mapas100 71–74, the scarab also in 76) and the existing beasts that still stand in for the other sheets.</summary>
        private static class Rio
        {
            public const string Crocodile = "enemy_forest_dark_river_crocodile";
            public const string MudCrocodile = "enemy_forest_mangrove_crocodile";
            public const string Ibis = "enemy_forest_dimmed_ibis";
            public const string Goose = "enemy_forest_thief_goose";
            public const string Scarab = "enemy_forest_shadow_scarab";
            public const string Reedsnare = "enemy_forest_reed_snare";
            public const string ShadowLocust = "enemy_forest_shadow_locust";
            public const string Locust = NightVariantRules.FireflySwarmEnemyId;
            public const string Hippo = "enemy_forest_mud_hippo";
            public const string ReedSnake = "enemy_forest_reed_snake";
            public const string Salamander = "enemy_desert_sand_snake";
            public const string ThreeHeaded = "enemy_forest_three_headed_serpent";
            public const string Python = "enemy_forest_child_of_apep";
            public const string Scorpion = "enemy_forest_black_scorpion";
            public const string RingScorpion = "enemy_desert_scorpion";
            public const string SandUshabti = "enemy_forest_sand_ushabti";
            public const string DimmedUshabti = "enemy_mountain_rope_ushabti";
            public const string ShadowBiter = "enemy_forest_shadow_biter";
            public const string Creeper = "enemy_forest_creeping_dark";
            public const string Ooze = "enemy_cave_ooze";
            public const string Quicksand = "enemy_forest_quicksand";
            public const string CrackedUshabti = "enemy_forest_terracotta_soldier";
            public const string Vulture = "enemy_desert_ember_vulture";
        }

        private static OverworldLayout RioRegion()
        {
            OverworldLayout w = NewRegion(RioRegionId, MapRegion.RioDasDozeHoras, Rect.MinMaxRect(-1200f, -600f, 1200f, 1800f));
            w.World.Biome = BiomeType.Forest;
            const string cais = CaisMesektetMapId;
            const string campos = CamposJuncosMapId;
            const string areal = ArealEscondidoMapId;
            const string aneis = AneisMehenMapId;
            const string lago = LagoDeFogoMapId;
            const string banco = BancoApepMapId;
            const string coast = MapRegionTypes.Coast;
            const string plain = MapRegionTypes.Plain;
            const string desert = MapRegionTypes.Desert;
            const string ruins = MapRegionTypes.Ruins;
            const string reedBoat = MapRequirementTags.SaddleRequired;
            const string lamp = MapRequirementTags.TorchRequired;
            const string runic = MapRequirementTags.RunicLamp;
            const string fireOil = MapRequirementTags.EternalFireOil;
            const string antivenom = MapRequirementTags.AntiVenomAmulet;

            // ---------------- Zones (a 5 × 5 grid; columns x −1200/−720/−240/240/720/1200, rows z −600/−120/360/840/1320/1800) ----------------
            // South row (towards Hinokura): the closed gates (73), the ash delta and the rowers' beach (71, the ways in), the papyrus bank, the serpents' way (73).
            OverworldZone zPortas = Zone(w, "portas_fechadas", areal, null, -1200f, -600f, -720f, -120f, OverworldTerrain.ClosedGates, 276, 283, 272,
                "Closed Gates", "Portas Fechadas", desert, lamp, 95f);
            OverworldZone zFoz = Zone(w, "foz_das_cinzas", cais, null, -720f, -600f, -240f, -120f, OverworldTerrain.AshDelta, 263, 267, 259,
                "Ash Mouth", "Foz das Cinzas", coast, spacing: 90f);
            OverworldZone zMargem = Zone(w, "margem_de_papiro", cais, null, -240f, -600f, 240f, -120f, OverworldTerrain.PapyrusBank, 261, 265, 257,
                "Papyrus Bank", "Margem de Papiro", coast, spacing: 90f);
            OverworldZone zPraia = Zone(w, "praia_dos_remadores", cais, null, 240f, -600f, 720f, -120f, OverworldTerrain.RowersBeach, 263, 267, 259,
                "Rowers' Beach", "Praia dos Remadores", coast, spacing: 90f);
            OverworldZone zCaminho = Zone(w, "caminho_das_serpentes", areal, null, 720f, -600f, 1200f, -120f, OverworldTerrain.ClosedGates, 276, 286, 272,
                "Serpents' Way", "Caminho das Serpentes", desert, lamp, 95f);
            // Second row: the hidden sands and the Sand Sled (73), the piers of the hours, the Cais, the barque's canal, the zigzag (73).
            OverworldZone zAreal = Zone(w, "areal_escondido", areal, null, -1200f, -120f, -720f, 360f, OverworldTerrain.HiddenSands, 275, 281, 270,
                "Hidden Sands", "Areal Escondido", desert, lamp, 95f);
            OverworldZone zPieres = Zone(w, "pieres_das_horas", cais, null, -720f, -120f, -240f, 360f, OverworldTerrain.HourPiers, 262, 266, 258,
                "Piers of the Hours", "Píeres das Horas", coast, spacing: 90f);
            OverworldZone zCais = Zone(w, "cais_de_mesektet", cais, null, -240f, -120f, 240f, 360f, OverworldTerrain.NightQuay, 260, 264, 256,
                "Mesektet Quay", "Cais de Mesektet", coast, spacing: 85f);
            OverworldZone zCanal = Zone(w, "canal_da_barca", cais, null, 240f, -120f, 720f, 360f, OverworldTerrain.BarqueCanal, 262, 266, 258,
                "Barque's Canal", "Canal da Barca", coast, spacing: 90f);
            OverworldZone zZigue = Zone(w, "zigue_zague_do_areal", areal, null, 720f, -120f, 1200f, 360f, OverworldTerrain.ZigzagSands, 275, 281, 270,
                "Zigzag of the Sands", "Zigue-Zague do Areal", desert, lamp, 95f);
            // Middle row: the outer coil (74), the reed beds, the fields with the Casa de Junco, the light wheat (72), the boiling shore (75).
            OverworldZone zAnelExterno = Zone(w, "anel_externo", aneis, null, -1200f, 360f, -720f, 840f, OverworldTerrain.OuterCoil, 284, 290, 280,
                "Outer Coil", "Anel Externo", ruins, runic, 100f);
            OverworldZone zCanteiros = Zone(w, "canteiros_de_juncos", campos, null, -720f, 360f, -240f, 840f, OverworldTerrain.ReedBeds, 268, 272, 264,
                "Reed Beds", "Canteiros de Juncos", plain, reedBoat, 90f);
            OverworldZone zCampos = Zone(w, "campos_de_juncos", campos, null, -240f, 360f, 240f, 840f, OverworldTerrain.ReedFields, 266, 270, 262,
                "Reed Fields", "Campos de Juncos", plain, reedBoat, 90f);
            OverworldZone zTrigais = Zone(w, "trigais_de_luz", campos, null, 240f, 360f, 720f, 840f, OverworldTerrain.LightWheat, 268, 272, 264,
                "Wheat of Light", "Trigais de Luz", plain, reedBoat, 90f);
            OverworldZone zBeira = Zone(w, "beira_fervente", lago, null, 720f, 360f, 1200f, 840f, OverworldTerrain.FireLakeShore, 292, 298, 288,
                "Boiling Shore", "Beira Fervente", coast, fireOil, 100f);
            // Fourth row: the Wrong Knot, the coils with the Quiet Ring (74), the canals of Sekhet (72), the ember beach and the Lake of Fire (75).
            OverworldZone zNo = Zone(w, "no_errado", aneis, null, -1200f, 840f, -720f, 1320f, OverworldTerrain.CoilKnot, 294, 296, 290,
                "Wrong Knot", "Nó Errado", ruins, runic, 100f);
            OverworldZone zAneis = Zone(w, "aneis_de_mehen", aneis, null, -720f, 840f, -240f, 1320f, OverworldTerrain.MehenCoils, 286, 292, 282,
                "Coils of Mehen", "Anéis de Mehen", ruins, runic, 100f);
            OverworldZone zCanais = Zone(w, "canais_de_sekhet", campos, null, -240f, 840f, 240f, 1320f, OverworldTerrain.SekhetCanals, 270, 276, 266,
                "Canals of Sekhet", "Canais de Sekhet", plain, reedBoat, 90f);
            OverworldZone zBrasa = Zone(w, "praia_de_brasa", lago, null, 240f, 840f, 720f, 1320f, OverworldTerrain.FireLakeShore, 294, 300, 290,
                "Ember Beach", "Praia de Brasa", coast, fireOil, 100f);
            OverworldZone zLago = Zone(w, "lago_de_fogo", lago, null, 720f, 840f, 1200f, 1320f, OverworldTerrain.FireLake, 298, 306, 294,
                "Lake of Fire", "Lago de Fogo", coast, fireOil, 100f);
            // North row, the dry bed of the river (76): Apep's body, the sandbank with the stranded prow, the dry river, the purple scales, the quicksand.
            OverworldZone zEscamas = Zone(w, "escamas_roxas", banco, null, -1200f, 1320f, -720f, 1800f, OverworldTerrain.ApepScales, 310, 320, 306,
                "Purple Scales", "Escamas Roxas", desert, antivenom, 100f);
            OverworldZone zCorpo = Zone(w, "corpo_de_apep", banco, null, -720f, 1320f, -240f, 1800f, OverworldTerrain.ApepSandbank, 306, 314, 302,
                "Apep's Body", "Corpo de Apep", desert, antivenom, 100f);
            OverworldZone zBanco = Zone(w, "banco_de_areia_de_apep", banco, null, -240f, 1320f, 240f, 1800f, OverworldTerrain.ApepSandbank, 304, 312, 300,
                "Apep's Sandbank", "Banco de Areia de Apep", desert, antivenom, 100f);
            OverworldZone zSeco = Zone(w, "rio_seco", banco, null, 240f, 1320f, 720f, 1800f, OverworldTerrain.ApepSandbank, 306, 314, 302,
                "Dry River", "Rio Seco", desert, antivenom, 100f);
            OverworldZone zMovedica = Zone(w, "areia_movedica", banco, null, 720f, 1320f, 1200f, 1800f, OverworldTerrain.ApepScales, 310, 320, 306,
                "Quicksand", "Areia Movediça", desert, antivenom, 100f);

            AddRioFauna(zPortas, zFoz, zMargem, zPraia, zCaminho, zAreal, zPieres, zCais, zCanal, zZigue, zAnelExterno, zCanteiros, zCampos, zTrigais, zBeira, zNo, zAneis,
                zCanais, zBrasa, zLago, zEscamas, zCorpo, zBanco, zSeco, zMovedica);

            // ---------------- Settlements ----------------
            // The Cais: the region's return city and the barca's stop for 71–76 (flat ground at y = 0, ±30 m). Then the posts of 72–76.
            w.Vila = Place(w, cais, CaisMesektetRecipe(), CaisMesektetOffset, Rect.MinMaxRect(-26f, -26f, 26f, 26f), CaisMesektetGround, true);
            Place(w, campos, Post("casa_de_junco", TownSwatch.Reed, TownSwatch.MesektetMat, TownRoofKind.Gable, TownShopDressing.ReedMarket, false),
                CasaDeJuncoOffset, PostSafe, 18f, false);
            Place(w, areal, Post("treno_de_areia", TownSwatch.QuaySandstone, TownSwatch.Reed, TownRoofKind.Flat, TownShopDressing.Herder, false),
                TrenoDeAreiaOffset, PostSafe, 18f, false);
            Place(w, aneis, Post("anel_quieto", TownSwatch.PapyrusGreen, TownSwatch.KemetGold, TownRoofKind.Flat, TownShopDressing.Cistern, false),
                AnelQuietoOffset, PostSafe, 18f, false);
            Place(w, lago, Post("ilha_das_palmeiras", TownSwatch.Laterite, TownSwatch.Thatch, TownRoofKind.Gable, TownShopDressing.Herder, false),
                IlhaPalmeirasOffset, PostSafe, 18f, false);
            Place(w, banco, Post("proa_encalhada", TownSwatch.KemetGold, TownSwatch.MesektetMat, TownRoofKind.Gable, TownShopDressing.BoatTrader, false),
                ProaEncalhadaOffset, PostSafe, 18f, false);

            // ---------------- Water and fire ----------------
            // The dark river (14 m, carved, two bridges on the roads north); the two canals of the fields (8 m, carved, bridged); the ponds;
            // the three lobes of the Lake of Fire and two fire ponds (glowing).
            MapRiver rio = River(w, 14f, RioNoturno);
            Vector2 bridgeWest = On(RioNoturno[1], RioNoturno[2], 130f / 350f);
            Vector2 bridgeEast = On(RioNoturno[2], RioNoturno[3], 220f / 350f);
            Crossing(rio, bridgeWest, 4.5f, true);
            Crossing(rio, bridgeEast, 4.5f, true);
            MapRiver canalSul = River(w, 8f, CanalSekhetSul);
            Crossing(canalSul, new Vector2(-150f, 500f), 4.5f, true);
            Crossing(canalSul, new Vector2(150f, 500f), 4.5f, true);
            MapRiver canalNorte = River(w, 8f, CanalSekhetNorte);
            Crossing(canalNorte, new Vector2(0f, 760f), 4.5f, true);
            foreach ((Vector2 a, Vector2 b, float width) in RioPools)
            {
                River(w, width, a, b);
            }

            foreach ((Vector2 a, Vector2 b, float width) in LagoDeFogoPools)
            {
                LavaRiver(w, width, a, b);
            }

            // ---------------- Gates out of the region ----------------
            // South: two passes down to the Arquipélago de Hinokura's north row (the ash fields, map 70, and the sea of ash, tagged with map 69: one target per gate).
            Gate(w, "exit_campos_de_cinza", MapExitKind.Map, CaldeiraMapId, Rect.MinMaxRect(-128f, -600f, -112f, -597f), new Vector2(-120f, -591f), Vector2.up);
            Gate(w, "exit_mar_de_cinza", MapExitKind.Map, NoveFornalhasMapId, Rect.MinMaxRect(472f, -600f, 488f, -597f), new Vector2(480f, -591f), Vector2.up);
            Gate(w, "exit_island", MapExitKind.WorldMap, null, Rect.MinMaxRect(-82f, 78f, -78f, 82f), new Vector2(-80f, 70f), Vector2.up);
            Landmark(w, MapLandmarkKind.StoneRing, -80f, 80f, 0f, 0.45f);

            // ---------------- Trails ----------------
            float g = CaisMesektetGround;
            // The south road from the Cais to the first pass; from its fork south-east to the second pass and south-west to the ash delta.
            Vector2 southFork = new Vector2(20f, -200f);
            PathVia(w, 4f, new Vector2(0f, -g), new Vector2(10f, -120f), southFork, new Vector2(-40f, -400f), new Vector2(-120f, -591f));
            PathVia(w, 3.5f, southFork, new Vector2(260f, -280f), new Vector2(480f, -450f), new Vector2(480f, -591f));
            PathVia(w, 3.5f, southFork, new Vector2(-260f, -260f), new Vector2(-500f, -290f), new Vector2(-620f, -300f));
            // The west road along the piers to the Sand Sled; the east road along the barque's canal to the zigzag of the sands.
            Vector2 westFork = new Vector2(-260f, 40f);
            PathVia(w, 4f, new Vector2(-g, 0f), new Vector2(-150f, 20f), westFork, new Vector2(-500f, 60f), new Vector2(-700f, 90f), TrenoDeAreiaOffset + new Vector2(18f, 0f));
            Vector2 eastFork = new Vector2(260f, 40f);
            Vector2 eastSands = new Vector2(960f, 120f);
            PathVia(w, 4f, new Vector2(g, 0f), new Vector2(150f, 20f), eastFork, new Vector2(500f, 60f), new Vector2(700f, 90f), eastSands);
            // North from both roads over the river's two bridges and the south canal to the Casa de Junco; branches to the reed beds and the wheat.
            PathVia(w, 4f, westFork, new Vector2(-220f, 140f), bridgeWest, new Vector2(-220f, 280f), new Vector2(-170f, 400f), new Vector2(-150f, 470f), new Vector2(-150f, 500f),
                new Vector2(-150f, 540f), new Vector2(-70f, 590f), CasaDeJuncoOffset + new Vector2(-18f, 0f));
            PathVia(w, 4f, eastFork, new Vector2(220f, 140f), bridgeEast, new Vector2(220f, 280f), new Vector2(170f, 400f), new Vector2(150f, 470f), new Vector2(150f, 500f),
                new Vector2(150f, 540f), new Vector2(70f, 590f), CasaDeJuncoOffset + new Vector2(18f, 0f));
            PathVia(w, 3.5f, new Vector2(-220f, 280f), new Vector2(-460f, 430f), new Vector2(-620f, 600f), new Vector2(-640f, 680f));
            PathVia(w, 3.5f, new Vector2(220f, 280f), new Vector2(460f, 430f), new Vector2(620f, 600f), new Vector2(640f, 680f));
            // On north over the north canal to the fork of the upper river.
            Vector2 upperFork = new Vector2(0f, 1100f);
            PathVia(w, 4f, CasaDeJuncoOffset + new Vector2(0f, 18f), new Vector2(0f, 700f), new Vector2(0f, 760f), new Vector2(0f, 820f), new Vector2(0f, 960f), upperFork);
            // West to the Quiet Ring, the centre of Mehen's coils; east to the Palm Isle by the lake.
            Vector2 lakeFork = new Vector2(640f, 1090f);
            PathVia(w, 4f, upperFork, new Vector2(-200f, 1100f), AnelQuietoOffset + new Vector2(18f, 0f));
            PathVia(w, 4f, upperFork, new Vector2(300f, 1095f), lakeFork, IlhaPalmeirasOffset + new Vector2(-18f, 0f));
            // North to the stranded prow; west and east along the dry river.
            PathVia(w, 4f, upperFork, new Vector2(0f, 1320f), ProaEncalhadaOffset + new Vector2(0f, -18f));
            PathVia(w, 3.5f, ProaEncalhadaOffset + new Vector2(-18f, 0f), new Vector2(-400f, 1500f), new Vector2(-800f, 1560f), new Vector2(-1000f, 1620f));
            PathVia(w, 3.5f, ProaEncalhadaOffset + new Vector2(18f, 0f), new Vector2(400f, 1500f), new Vector2(800f, 1560f), new Vector2(1000f, 1620f));
            // The west flank: the Sand Sled south to the closed gates, north through the outer coil to the Quiet Ring, a branch to the Wrong Knot.
            Vector2 coilFork = new Vector2(-900f, 760f);
            PathVia(w, 3.5f, TrenoDeAreiaOffset + new Vector2(0f, -18f), new Vector2(-960f, -360f));
            PathVia(w, 3.5f, TrenoDeAreiaOffset + new Vector2(0f, 18f), new Vector2(-1000f, 420f), coilFork, new Vector2(-620f, 1000f), AnelQuietoOffset + new Vector2(-18f, 0f));
            PathVia(w, 3.5f, coilFork, new Vector2(-1050f, 1000f), new Vector2(-1080f, 1240f));
            // The east flank: south to the serpents' way, north along the boiling shore to the fork by the lake.
            PathVia(w, 3.5f, eastSands, new Vector2(980f, -360f));
            PathVia(w, 3.5f, eastSands, new Vector2(980f, 500f), new Vector2(760f, 760f), new Vector2(640f, 960f), lakeFork);

            // ---------------- Landmarks ----------------
            Sign(w, southFork, new Vector2(260f, -280f), 4f, 0f);
            Sign(w, westFork, new Vector2(-500f, 60f), 4f, 0f);
            Sign(w, eastFork, new Vector2(500f, 60f), 4f, 0f);
            Sign(w, upperFork, new Vector2(300f, 1095f), 4f, 0f);
            Sign(w, lakeFork, new Vector2(640f, 960f), 4f, 0f);
            Sign(w, coilFork, new Vector2(-620f, 1000f), 3.5f, 0f);
            Sign(w, eastSands, new Vector2(980f, 500f), 3.5f, 0f);
            Landmark(w, MapLandmarkKind.Campfire, -400f, -400f, 0f, 0.9f);
            Landmark(w, MapLandmarkKind.Campfire, 380f, -190f, 0f, 0.9f);
            Landmark(w, MapLandmarkKind.Campfire, -660f, -30f, 0f, 1f);
            Landmark(w, MapLandmarkKind.Campfire, 340f, 700f, 0f, 1f);
            Landmark(w, MapLandmarkKind.Campfire, -1050f, 300f, 0f, 1f);
            Landmark(w, MapLandmarkKind.Campfire, 500f, 1250f, 0f, 1f);
            Landmark(w, MapLandmarkKind.Campfire, -520f, 1650f, 0f, 1f);
            Landmark(w, MapLandmarkKind.StoneRing, -330f, 640f, 0f, 1f);
            Landmark(w, MapLandmarkKind.StoneRing, ApepArena.x, ApepArena.y, 0f, 1.1f);

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
        // The quay (its own recipe, Cities/Town: the PapyrusQuay style, inside a radius of 22.5 m on its reserved flat ground of ±30 m at
        // y = 0; no backdrop: the river is all round it). Its streets leave W, E and S (no north street: the bank drops to the dark river),
        // so the north roads leave from the west and east roads.
        // ------------------------------------------------------------------

        private static CityTownRecipe CaisMesektetRecipe() => NoBackdrop(CityTownRecipes.CaisDeMesektet());

        // ------------------------------------------------------------------
        // Monster groups per area (existing definitions only, no boss; Mapas100 71–76 archetypes)
        // ------------------------------------------------------------------

        private static void AddRioFauna(OverworldZone portas, OverworldZone foz, OverworldZone margem, OverworldZone praia, OverworldZone caminho, OverworldZone areal,
            OverworldZone pieres, OverworldZone cais, OverworldZone canal, OverworldZone zigue, OverworldZone anelExterno, OverworldZone canteiros, OverworldZone campos,
            OverworldZone trigais, OverworldZone beira, OverworldZone no, OverworldZone aneis, OverworldZone canais, OverworldZone brasa, OverworldZone lago,
            OverworldZone escamas, OverworldZone corpo, OverworldZone banco, OverworldZone seco, OverworldZone movedica)
        {
            // It is always night on the river (the night groups are the same beasts, stronger): the moon-mother over the water, the
            // owl over the reeds, the bats in the rings, the sparks on the lake.
            NightSpecies owl = NightVariantRules.RasgaMortalha;
            NightSpecies bats = NightVariantRules.MorcegoVampiro;
            NightSpecies moon = NightVariantRules.MaeDaLua;
            NightSpecies sparks = NightVariantRules.VagaLume;
            NightSpecies werewolf = NightVariantRules.Lobisomem;

            // The margins of the quay (71): the Crocodilo do Rio Escuro [tanque], the Íbis Apagada [rápido], the Escaravelho de Sombra [enxame],
            // the Junco-Laço [controle].
            cais.Fauna.Add(Fauna("ibis_apagadas", "[rápido]", T(N(Rio.Ibis, 3), N(Rio.Crocodile, 1)), T(V(Rio.Ibis, 2), S(moon, 1)), 4));
            cais.Fauna.Add(Fauna("escaravelhos_do_cais", "[enxame]", T(N(Rio.Scarab, 3), N(Rio.Ibis, 1)), T(V(Rio.Scarab, 3), S(sparks, 1)), 5));
            margem.Fauna.Add(Fauna("crocodilos_do_rio_escuro", "[tanque]", T(N(Rio.Crocodile, 3), N(Rio.Ibis, 1)), T(V(Rio.Crocodile, 2), S(moon, 1)), 3, null, 30f));
            margem.Fauna.Add(Fauna("juncos_laco", "[controle]", T(N(Rio.Reedsnare, 3), N(Rio.Scarab, 1)), T(V(Rio.Reedsnare, 2), S(owl, 1)), 4));
            foz.Fauna.Add(Fauna("escaravelhos_da_foz", "[enxame]", T(N(Rio.Scarab, 3), N(Rio.Reedsnare, 1)), T(V(Rio.Scarab, 3), S(sparks, 1)), 5));
            foz.Fauna.Add(Fauna("abutres_da_cinza", "[distância]", T(N(Rio.Vulture, 3), N(Rio.Scarab, 1)), T(V(Rio.Vulture, 2), S(owl, 1)), 3));
            praia.Fauna.Add(Fauna("crocodilos_da_praia", "[tanque]", T(N(Rio.Crocodile, 3), N(Rio.Scarab, 1)), T(V(Rio.Crocodile, 2), S(moon, 1)), 3, null, 30f));
            praia.Fauna.Add(Fauna("ibis_da_praia", "[rápido]", T(N(Rio.Ibis, 3), N(Rio.Reedsnare, 1)), T(V(Rio.Ibis, 3), S(owl, 1)), 4));
            pieres.Fauna.Add(Fauna("juncos_dos_pieres", "[controle]", T(N(Rio.Reedsnare, 3), N(Rio.Crocodile, 1)), T(V(Rio.Reedsnare, 2), S(moon, 1)), 4));
            pieres.Fauna.Add(Fauna("ibis_dos_pieres", "[rápido]", T(N(Rio.Ibis, 3), N(Rio.Scarab, 1)), T(V(Rio.Ibis, 3), S(moon, 1)), 4));
            canal.Fauna.Add(Fauna("crocodilos_do_canal", "[tanque]", T(N(Rio.Crocodile, 3), N(Rio.Reedsnare, 1)), T(V(Rio.Crocodile, 2), S(sparks, 1)), 3, null, 30f));
            canal.Fauna.Add(Fauna("escaravelhos_do_canal", "[enxame]", T(N(Rio.Scarab, 3), N(Rio.Crocodile, 1)), T(V(Rio.Scarab, 3), S(moon, 1)), 5));

            // Campos de Juncos (72): the Gafanhoto de Sombra [enxame], the Hipopótamo de Lodo [tanque], the Cobra do Junco [rápido], the Ganso
            // Ladrão [rápido] (the thief gull).
            campos.Fauna.Add(Fauna("gafanhotos_de_sombra", "[enxame]", T(N(Rio.ShadowLocust, 3), N(Rio.ReedSnake, 1)), T(V(Rio.ShadowLocust, 3), S(sparks, 1)), 5));
            campos.Fauna.Add(Fauna("gansos_ladroes", "[rápido]", T(N(Rio.Goose, 3), N(Rio.ReedSnake, 1)), T(V(Rio.Goose, 3), S(owl, 1)), 4));
            canteiros.Fauna.Add(Fauna("hipopotamos_de_lodo", "[tanque]", T(N(Rio.Hippo, 3), N(Rio.ReedSnake, 1)), T(V(Rio.Hippo, 2), S(moon, 1)), 3, null, 30f));
            canteiros.Fauna.Add(Fauna("cobras_do_junco", "[rápido]", T(N(Rio.ReedSnake, 3), N(Rio.ShadowLocust, 1)), T(V(Rio.ReedSnake, 3), S(owl, 1)), 4));
            trigais.Fauna.Add(Fauna("gafanhotos_dos_trigais", "[enxame]", T(N(Rio.ShadowLocust, 3), N(Rio.Goose, 1)), T(V(Rio.ShadowLocust, 3), S(sparks, 1)), 5));
            trigais.Fauna.Add(Fauna("gansos_dos_trigais", "[rápido]", T(N(Rio.Goose, 3), N(Rio.Hippo, 1)), T(V(Rio.Goose, 3), S(moon, 1)), 4));
            canais.Fauna.Add(Fauna("hipopotamos_dos_canais", "[tanque]", T(N(Rio.Hippo, 3), N(Rio.ShadowLocust, 1)), T(V(Rio.Hippo, 3), S(moon, 1)), 3, null, 30f));
            canais.Fauna.Add(Fauna("cobras_dos_canais", "[rápido]", T(N(Rio.ReedSnake, 3), N(Rio.Hippo, 1)), T(V(Rio.ReedSnake, 3), S(owl, 1)), 4));

            // Areal Escondido (73): the Serpente de Três Cabeças [elite], the Escorpião Negro [rápido], the Escaravelho de Sombra [enxame], the
            // Ushabti de Areia [tanque].
            areal.Fauna.Add(Fauna("serpentes_de_tres_cabecas", "[elite]", T(N(Rio.ThreeHeaded, 3), N(Rio.Scorpion, 1)), T(V(Rio.ThreeHeaded, 2), S(werewolf, 1)), 3, null, 32f));
            areal.Fauna.Add(Fauna("escorpioes_negros", "[rápido]", T(N(Rio.Scorpion, 3), N(Rio.Scarab, 1)), T(V(Rio.Scorpion, 3), S(owl, 1)), 4));
            portas.Fauna.Add(Fauna("ushabtis_de_areia", "[tanque]", T(N(Rio.SandUshabti, 3), N(Rio.Scarab, 1)), T(V(Rio.SandUshabti, 2), S(bats, 1)), 3, null, 30f));
            portas.Fauna.Add(Fauna("escaravelhos_das_portas", "[enxame]", T(N(Rio.Scarab, 3), N(Rio.SandUshabti, 1)), T(V(Rio.Scarab, 3), S(sparks, 1)), 5));
            caminho.Fauna.Add(Fauna("serpentes_do_caminho", "[elite]", T(N(Rio.ThreeHeaded, 3), N(Rio.Scarab, 1)), T(V(Rio.ThreeHeaded, 2), S(owl, 1)), 3, null, 32f));
            caminho.Fauna.Add(Fauna("ushabtis_do_caminho", "[tanque]", T(N(Rio.SandUshabti, 3), N(Rio.Scorpion, 1)), T(V(Rio.SandUshabti, 2), S(moon, 1)), 3, null, 30f));
            zigue.Fauna.Add(Fauna("escorpioes_do_zigue_zague", "[rápido]", T(N(Rio.Scorpion, 3), N(Rio.ThreeHeaded, 1)), T(V(Rio.Scorpion, 3), S(owl, 1)), 4));
            zigue.Fauna.Add(Fauna("escaravelhos_do_zigue_zague", "[enxame]", T(N(Rio.Scarab, 3), N(Rio.Scorpion, 1)), T(V(Rio.Scarab, 3), S(sparks, 1)), 5));

            // Anéis de Mehen (74): the Mordedor de Sombra [rápido], the Treva Rastejante [enxame], the Escorpião [tanque] (the desert scorpion), the
            // Ushabti Apagado [tanque] (the rope ushabti); the bats in the rings.
            aneis.Fauna.Add(Fauna("mordedores_de_sombra", "[rápido]", T(N(Rio.ShadowBiter, 3), N(Rio.Creeper, 1)), T(V(Rio.ShadowBiter, 3), S(bats, 1)), 4));
            aneis.Fauna.Add(Fauna("trevas_rastejantes", "[enxame]", T(N(Rio.Creeper, 3), N(Rio.ShadowBiter, 1)), T(V(Rio.Creeper, 3), S(sparks, 1)), 5));
            anelExterno.Fauna.Add(Fauna("escorpioes_dos_aneis", "[tanque]", T(N(Rio.RingScorpion, 3), N(Rio.Creeper, 1)), T(V(Rio.RingScorpion, 2), S(bats, 1)), 3, null, 30f));
            anelExterno.Fauna.Add(Fauna("mordedores_do_anel", "[rápido]", T(N(Rio.ShadowBiter, 3), N(Rio.RingScorpion, 1)), T(V(Rio.ShadowBiter, 3), S(werewolf, 1)), 4));
            no.Fauna.Add(Fauna("ushabtis_apagados", "[tanque]", T(N(Rio.DimmedUshabti, 3), N(Rio.ShadowBiter, 1)), T(V(Rio.DimmedUshabti, 2), S(bats, 1)), 3, null, 30f));
            no.Fauna.Add(Fauna("trevas_do_no", "[enxame]", T(N(Rio.Creeper, 3), N(Rio.DimmedUshabti, 1)), T(V(Rio.Creeper, 3), S(bats, 1)), 5));

            // Lago de Fogo (75): the Chama Faminta [enxame] (the firefly swarm), the Salamandra de Sombra [rápido] (the sand snake), the Sombra Fervida
            // [controle] (the ooze), the Ushabti Rachado [tanque] (the terracotta soldier).
            lago.Fauna.Add(Fauna("chamas_famintas_do_lago", "[enxame]", T(N(Rio.Locust, 3), N(Rio.Salamander, 1)), T(V(Rio.Locust, 3), S(sparks, 1)), 5));
            lago.Fauna.Add(Fauna("ushabtis_rachados", "[tanque]", T(N(Rio.CrackedUshabti, 3), N(Rio.Locust, 1)), T(V(Rio.CrackedUshabti, 2), S(bats, 1)), 3, null, 30f));
            beira.Fauna.Add(Fauna("salamandras_de_sombra", "[rápido]", T(N(Rio.Salamander, 3), N(Rio.Ooze, 1)), T(V(Rio.Salamander, 3), S(sparks, 1)), 4));
            beira.Fauna.Add(Fauna("sombras_fervidas", "[controle]", T(N(Rio.Ooze, 3), N(Rio.Salamander, 1)), T(V(Rio.Ooze, 2), S(owl, 1)), 4));
            brasa.Fauna.Add(Fauna("sombras_da_brasa", "[controle]", T(N(Rio.Ooze, 3), N(Rio.Locust, 1)), T(V(Rio.Ooze, 2), S(sparks, 1)), 4));
            brasa.Fauna.Add(Fauna("ushabtis_da_brasa", "[tanque]", T(N(Rio.CrackedUshabti, 3), N(Rio.Salamander, 1)), T(V(Rio.CrackedUshabti, 2), S(moon, 1)), 3, null, 30f));

            // Banco de Areia de Apep (76): the Filhos de Apep [rápido] (the dimmed python), the Areia Movediça [controle] (the ooze), the Escaravelho de
            // Sombra [enxame] (the region's own), the Crocodilo de Lodo [tanque] (the mangrove crocodile).
            banco.Fauna.Add(Fauna("filhos_de_apep", "[rápido]", T(N(Rio.Python, 3), N(Rio.Scarab, 1)), T(V(Rio.Python, 3), S(werewolf, 1)), 4));
            banco.Fauna.Add(Fauna("crocodilos_de_lodo", "[tanque]", T(N(Rio.MudCrocodile, 3), N(Rio.Python, 1)), T(V(Rio.MudCrocodile, 2), S(moon, 1)), 3, null, 30f));
            corpo.Fauna.Add(Fauna("filhos_do_corpo", "[rápido]", T(N(Rio.Python, 3), N(Rio.Quicksand, 1)), T(V(Rio.Python, 3), S(bats, 1)), 4));
            corpo.Fauna.Add(Fauna("escaravelhos_do_corpo", "[enxame]", T(N(Rio.Scarab, 3), N(Rio.Python, 1)), T(V(Rio.Scarab, 3), S(sparks, 1)), 5));
            seco.Fauna.Add(Fauna("crocodilos_do_rio_seco", "[tanque]", T(N(Rio.MudCrocodile, 3), N(Rio.Scarab, 1)), T(V(Rio.MudCrocodile, 2), S(moon, 1)), 3, null, 30f));
            seco.Fauna.Add(Fauna("escaravelhos_do_rio_seco", "[enxame]", T(N(Rio.Scarab, 3), N(Rio.MudCrocodile, 1)), T(V(Rio.Scarab, 3), S(sparks, 1)), 5));
            escamas.Fauna.Add(Fauna("filhos_das_escamas", "[rápido]", T(N(Rio.Python, 3), N(Rio.MudCrocodile, 1)), T(V(Rio.Python, 3), S(werewolf, 1)), 4));
            escamas.Fauna.Add(Fauna("areias_das_escamas", "[controle]", T(N(Rio.Quicksand, 3), N(Rio.Scarab, 1)), T(V(Rio.Quicksand, 2), S(bats, 1)), 4));
            movedica.Fauna.Add(Fauna("areias_movedicas", "[controle]", T(N(Rio.Quicksand, 3), N(Rio.MudCrocodile, 1)), T(V(Rio.Quicksand, 2), S(owl, 1)), 4));
            movedica.Fauna.Add(Fauna("filhos_da_areia_movedica", "[rápido]", T(N(Rio.Python, 3), N(Rio.Quicksand, 1)), T(V(Rio.Python, 3), S(moon, 1)), 4));
        }
    }
}
