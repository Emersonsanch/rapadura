using System.Collections.Generic;
using System.Linq;
using Rapadura.Gameplay.World.Biomes;
using Rapadura.Gameplay.World.Cities;
using R = Rapadura.Gameplay.World.Map.MapRegion;
using T = Rapadura.Gameplay.World.Map.MapRequirementTags;
using K = Rapadura.Gameplay.World.Map.MapSettlementKind;
using S = Rapadura.Gameplay.World.Map.MapSize;
using Y = Rapadura.Gameplay.World.Map.MapRegionTypes;

namespace Rapadura.Gameplay.World.Map
{
    /// <summary>
    /// The 42 maps of the "até 100" scope (★, Docs/Mapas100.md §1 and Docs/MetodologiaMapasERacas.md Part II) and the six of
    /// Região 9 (☆ 47–52, the Estepe dos Nartes), the six of Região 10 (☆ 53–58, the Picos de Farnkesh) and the six of Região 11
    /// (☆ 59–64, the Mar Gelado), the six of Região 12 (☆ 65–70, the Arquipélago de Hinokura), the six of Região 13 (☆ 71–76, the Rio das Doze Horas), the six of Região 14 (☆ 77–82, the Margens da Mistura), the eight of Região 15 (☆ 83–90, the Limiar Profundo) and the ten of Região 16 (☆ 91–100, the Reinos do Céu), walked as zones of their continuous regions: level
    /// bands, walk/warp levels, special gates, settlements and return cities, as content-as-code like
    /// <c>CityCatalog</c>. Only <see cref="MapDefinition.IsBuilt"/> maps are playable today (01–42, 47–100): a hand-made layout
    /// (<c>MapLayouts.Has</c>: 01–08, 13, 14, 19, 20) or only a zone of a continuous region (<c>OverworldLayouts.ForMap</c>:
    /// 09–12, 15–18, 21–42, 47–100, their posts with a Cais da Barca); <c>MapLayouts.IsPlayable</c> is the runtime check and a test keeps
    /// the flag equal to it. The rest are listed in the Barcas de Luz window, closed. The gold fare is never stored: it is always the
    /// formula of <see cref="MapAccessRules.CalculateWarpCost(int,int,bool)"/> (the "Custo Ouro" column of the methodology
    /// table is design history).
    /// </summary>
    public static class MapCatalog
    {
        // Return cities: the four that exist (CityCatalog ids) and the planned ones (Docs/Mapas100.md §2–§3).
        public const string CopasGarajonay = "copas_garajonay";
        public const string TamborRubro = "tambor_rubro";
        public const string SaloesDeBronze = "saloes_de_bronze";
        public const string MercadoDosTuneis = "mercado_dos_tuneis";
        public const string TaaPwani = "taa_pwani";
        public const string Aratta = "aratta";
        public const string Kirta = "kirta";
        public const string Jhilpur = "jhilpur";
        public const string PalafitasTarchon = "palafitas_tarchon";
        public const string Sangkent = "sangkent";
        public const string Mingcheng = "mingcheng";
        public const string TorresBrancas = "torres_brancas";
        public const string AcampamentoSoslan = "acampamento_soslan";
        public const string Fortim = "fortim";
        public const string Arvoredo = "arvoredo";
        public const string ValeDosSatiros = "vale_dos_satiros";
        public const string Farnkesh = "farnkesh";
        public const string PortoDaLuaBranca = "porto_da_lua_branca";
        public const string Hinokura = "hinokura";
        public const string CaisDeMesektet = "cais_de_mesektet";
        public const string DurJarras = "dur_jarras";
        public const string FortalezaUltimaLampada = "fortaleza_ultima_lampada";
        public const string CaisDaColuna = "cais_da_coluna";

        public const string PomarId = "map_001_pomar";
        public const string TugaiId = "map_002_tugai";

        private const int Fee = MapAccessRules.BaseWarpFee;

        /// <summary>Every return city id a map may name (existing + planned), in region order.</summary>
        public static readonly IReadOnlyList<string> ReturnCityIds = new[]
        {
            CityCatalog.VilaZefa, CopasGarajonay, CityCatalog.AreiaBranca, TamborRubro, CityCatalog.SerraAlta, SaloesDeBronze,
            CityCatalog.Lamparinas, MercadoDosTuneis, TaaPwani, Aratta, Kirta, Jhilpur, PalafitasTarchon, Sangkent, Mingcheng, TorresBrancas,
            AcampamentoSoslan, Fortim, Arvoredo, ValeDosSatiros, Farnkesh, PortoDaLuaBranca, Hinokura, CaisDeMesektet, DurJarras, FortalezaUltimaLampada, CaisDaColuna,
        };

        public static readonly IReadOnlyList<MapDefinition> Maps = new List<MapDefinition>
        {
            // Região 1 — Bosque do Zarafshan
            new MapDefinition(1, PomarId, R.Zarafshan, Y.Forest, S.P, 1, 3, 1, 1, 0, T.None, CityCatalog.VilaZefa, K.City, "vila_vo_edna", isBuilt: true),
            new MapDefinition(2, TugaiId, R.Zarafshan, Y.Forest, S.M, 2, 5, 2, 5, Fee, T.None, CityCatalog.VilaZefa, K.Outpost, "moinho_do_pomar", isBuilt: true),
            new MapDefinition(3, "map_003_brejo", R.Zarafshan, Y.Swamp, S.M, 16, 22, 14, 18, Fee, T.LeatherBoots, CityCatalog.VilaZefa, K.Outpost, "ponte_do_aryk", isBuilt: true),
            new MapDefinition(4, "map_004_amoreiras", R.Zarafshan, Y.Forest, S.M, 18, 24, 16, 20, Fee, T.None, CityCatalog.VilaZefa, K.Outpost, "casa_do_tear", isBuilt: true),
            new MapDefinition(5, "map_005_garajonay", R.Zarafshan, Y.Forest, S.G, 20, 26, 18, 22, Fee, T.ElevatedPlatforms, CopasGarajonay, K.RaceCity, "copas_garajonay", isBuilt: true),
            new MapDefinition(6, "map_006_mihrshah", R.Zarafshan, Y.Ruins, S.M, 26, 32, 24, 28, Fee, T.GardenKey, CopasGarajonay, K.Outpost, "portao_do_jardim", isBuilt: true),

            // Região 2 — Areias Flamejantes
            new MapDefinition(7, "map_007_caravanas", R.AreiasFlamejantes, Y.Desert, S.M, 5, 7, 5, 8, Fee, T.None, CityCatalog.AreiaBranca, K.Outpost, "caravancara_do_ammo", isBuilt: true),
            new MapDefinition(8, "map_008_artifices", R.AreiasFlamejantes, Y.Ruins, S.M, 6, 9, 6, 10, Fee, T.None, CityCatalog.AreiaBranca, K.Outpost, "acampamento_das_piramides", isBuilt: true),
            new MapDefinition(9, "map_009_karez", R.AreiasFlamejantes, Y.Underground, S.M, 20, 26, 18, 22, Fee, T.None, CityCatalog.AreiaBranca, K.Outpost, "boca_do_karez", isBuilt: true),
            new MapDefinition(10, "map_010_dunas", R.AreiasFlamejantes, Y.Desert, S.G, 22, 28, 20, 25, Fee, T.FreshWater, CityCatalog.AreiaBranca, K.Outpost, "poco_das_tamareiras", isBuilt: true),
            new MapDefinition(11, "map_011_tambor_rubro", R.AreiasFlamejantes, Y.Desert, S.G, 24, 32, 22, 28, Fee, T.None, TamborRubro, K.RaceCity, "tambor_rubro", isBuilt: true),
            new MapDefinition(12, "map_012_namrael", R.AreiasFlamejantes, Y.Desert, S.G, 30, 38, 28, 32, Fee, T.PoisonProtection, TamborRubro, K.Outpost, "torre_de_taipa", isBuilt: true),

            // Região 3 — Serra de Kaggen
            new MapDefinition(13, "map_013_rondaveis", R.SerraDeKaggen, Y.Mountain, S.M, 9, 11, 8, 12, Fee, T.None, CityCatalog.SerraAlta, K.City, "vila_de_litema", isBuilt: true),
            new MapDefinition(14, "map_014_hahya", R.SerraDeKaggen, Y.Mountain, S.M, 10, 13, 9, 14, Fee, T.WoolCloak, CityCatalog.SerraAlta, K.Outpost, "boca_da_mina", isBuilt: true),
            new MapDefinition(15, "map_015_abrigos", R.SerraDeKaggen, Y.Mountain, S.M, 26, 32, 24, 28, Fee, T.None, CityCatalog.SerraAlta, K.Outpost, "abrigo_do_eland", isBuilt: true),
            new MapDefinition(16, "map_016_pedra_seca", R.SerraDeKaggen, Y.Ruins, S.G, 28, 34, 26, 30, Fee, T.None, CityCatalog.SerraAlta, K.Outpost, "recinto_curvo", isBuilt: true),
            new MapDefinition(17, "map_017_napirasu", R.SerraDeKaggen, Y.Mountain, S.G, 30, 38, 28, 34, Fee, T.PickaxeTorch, SaloesDeBronze, K.RaceCity, "saloes_de_bronze", isBuilt: true),
            new MapDefinition(18, "map_018_mazans", R.SerraDeKaggen, Y.Mountain, S.G, 36, 44, 34, 40, Fee, T.BlizzardResistance, SaloesDeBronze, K.Outpost, "estela_alta", isBuilt: true),

            // Região 4 — Grutas do Limiar
            new MapDefinition(19, "map_019_lago_limiar", R.GrutasDoLimiar, Y.Underground, S.M, 13, 15, 12, 16, Fee, T.TorchRequired, CityCatalog.Lamparinas, K.City, "vila_das_lamparinas", isBuilt: true),
            new MapDefinition(20, "map_020_tages", R.GrutasDoLimiar, Y.Underground, S.M, 14, 18, 13, 18, Fee, T.TorchRequired, CityCatalog.Lamparinas, K.Outpost, "nicho_da_ponte", isBuilt: true),
            new MapDefinition(21, "map_021_pedras_dragao", R.GrutasDoLimiar, Y.Underground, S.G, 32, 38, 30, 35, Fee, T.RunicLamp, CityCatalog.Lamparinas, K.Outpost, "vishapakar_grande", isBuilt: true),
            new MapDefinition(22, "map_022_mercado_tuneis", R.GrutasDoLimiar, Y.Underground, S.G, 34, 42, 32, 38, Fee, T.None, MercadoDosTuneis, K.RaceCity, "mercado_dos_tuneis", isBuilt: true),
            new MapDefinition(23, "map_023_canion_roda", R.GrutasDoLimiar, Y.Ruins, S.G, 38, 46, 36, 42, Fee, T.EternalFireOil, MercadoDosTuneis, K.Outpost, "torre_das_jarras", isBuilt: true),
            new MapDefinition(24, "map_024_mahaway", R.GrutasDoLimiar, Y.Underground, S.G, 42, 50, 40, 46, Fee, T.WingsOrDoubleJump, CityCatalog.Lamparinas, K.Outpost, "ultima_ponte", isBuilt: true),

            // Região 5 — Costa das Monções
            new MapDefinition(25, "map_025_taa_pwani", R.CostaDasMoncoes, Y.Coast, S.G, 45, 50, 42, 48, Fee, T.BoatOrDock, TaaPwani, K.City, "taa_pwani", isBuilt: true),
            new MapDefinition(26, "map_026_auroques", R.CostaDasMoncoes, Y.Plain, S.GG, 48, 55, 45, 52, Fee, T.None, Aratta, K.RaceCity, "aratta", isBuilt: true),
            new MapDefinition(27, "map_027_meroe", R.CostaDasMoncoes, Y.Ruins, S.G, 52, 58, 50, 56, Fee, T.ArtificerRelic, Aratta, K.Outpost, "patio_dos_relevos", isBuilt: true),
            new MapDefinition(28, "map_028_kirta", R.CostaDasMoncoes, Y.Coast, S.G, 55, 62, 52, 60, Fee, T.BreathingShell, Kirta, K.RaceCity, "kirta", isBuilt: true),
            new MapDefinition(29, "map_029_kilwa", R.CostaDasMoncoes, Y.Ruins, S.G, 60, 66, 58, 65, Fee, T.AirBubble, Kirta, K.Outpost, "farol_de_coral", isBuilt: true),
            new MapDefinition(30, "map_030_liyongo", R.CostaDasMoncoes, Y.Swamp, S.G, 64, 72, 60, 70, Fee, T.RootBoots, TaaPwani, K.Outpost, "cais_das_dhows", isBuilt: true),

            // Região 6 — Delta de Jhilpur
            new MapDefinition(31, "map_031_jhilpur", R.DeltaDeJhilpur, Y.Swamp, S.GG, 60, 66, 58, 65, Fee, T.None, Jhilpur, K.Capital, "jhilpur", isBuilt: true),
            new MapDefinition(32, "map_032_raiz_viva", R.DeltaDeJhilpur, Y.Swamp, S.G, 64, 70, 62, 68, Fee, T.RootBridgeBalance, Jhilpur, K.Outpost, "plataforma_das_garcas", isBuilt: true),
            new MapDefinition(33, "map_033_tarchon", R.DeltaDeJhilpur, Y.Swamp, S.G, 70, 78, 66, 74, Fee, T.SaddleRequired, PalafitasTarchon, K.RaceCity, "palafitas_tarchon", isBuilt: true),
            new MapDefinition(34, "map_034_ninki_nanka", R.DeltaDeJhilpur, Y.Swamp, S.G, 76, 82, 72, 80, Fee, T.AntiVenomAmulet, PalafitasTarchon, K.Outpost, "ilhota_das_lanternas", isBuilt: true),
            new MapDefinition(35, "map_035_coracao_lodo", R.DeltaDeJhilpur, Y.Swamp, S.G, 82, 88, 78, 85, Fee, T.PurifyingFruit, Jhilpur, K.Outpost, "raiz_mae", isBuilt: true),

            // Região 7 — Teto do Mundo
            new MapDefinition(36, "map_036_sangkent", R.TetoDoMundo, Y.Mountain, S.GG, 70, 76, 68, 75, Fee, T.ThermalClothes, Sangkent, K.Capital, "sangkent", isBuilt: true),
            new MapDefinition(37, "map_037_calhas", R.TetoDoMundo, Y.Mountain, S.G, 74, 80, 70, 78, Fee, T.None, Sangkent, K.Outpost, "estacao_calha_alta", isBuilt: true),
            new MapDefinition(38, "map_038_geleira", R.TetoDoMundo, Y.Mountain, S.G, 80, 86, 76, 84, Fee, T.ClimbingCrampons, Sangkent, K.Outpost, "cabana_do_iaque", isBuilt: true),
            new MapDefinition(39, "map_039_forte_ventos", R.TetoDoMundo, Y.Mountain, S.G, 84, 92, 80, 88, Fee, T.WingsRequired, Sangkent, K.Outpost, "forte_dos_ventos", isBuilt: true),
            new MapDefinition(40, "map_040_wakhan", R.TetoDoMundo, Y.Mountain, S.GG, 90, 98, 86, 94, Fee, T.StormProtection, Sangkent, K.Outpost, "caravancara_de_pedra", isBuilt: true),

            // Região 8 — Planície de Mingcheng
            new MapDefinition(41, "map_041_mingcheng", R.PlanicieDeMingcheng, Y.Plain, S.GG, 90, 96, 88, 95, Fee, T.None, Mingcheng, K.Capital, "mingcheng", isBuilt: true),
            new MapDefinition(42, "map_042_torres_brancas", R.PlanicieDeMingcheng, Y.Plain, S.G, 95, 102, 92, 100, Fee, T.SunSeal, TorresBrancas, K.RaceCity, "torres_brancas", isBuilt: true),

            // Região 9 — Estepe dos Nartes (☆, Docs/Mapas100.md §1; zones of its continuous region, OverworldLayouts.Nartes)
            new MapDefinition(47, "map_047_soslan", R.EstepeDosNartes, Y.Plain, S.GG, 100, 108, 96, 104, Fee, T.None, AcampamentoSoslan, K.RaceCity, "acampamento_soslan", isBuilt: true),
            new MapDefinition(48, "map_048_kurgans", R.EstepeDosNartes, Y.Plain, S.G, 106, 114, 102, 110, Fee, T.None, AcampamentoSoslan, K.Outpost, "kurgan_aberto", isBuilt: true),
            new MapDefinition(49, "map_049_fortim", R.EstepeDosNartes, Y.Forest, S.G, 112, 120, 108, 116, Fee, T.None, Fortim, K.RaceCity, "fortim", isBuilt: true),
            new MapDefinition(50, "map_050_haripushpa", R.EstepeDosNartes, Y.Forest, S.G, 118, 126, 114, 122, Fee, T.None, Arvoredo, K.RaceCity, "arvoredo", isBuilt: true),
            new MapDefinition(51, "map_051_mil_cavalos", R.EstepeDosNartes, Y.Plain, S.GG, 124, 134, 120, 128, Fee, T.None, AcampamentoSoslan, K.Outpost, "poco_da_taca", isBuilt: true),
            new MapDefinition(52, "map_052_amirani", R.EstepeDosNartes, Y.Mountain, S.GG, 134, 150, 128, 140, Fee, T.ClimbingCrampons, Fortim, K.Outpost, "torre_de_vigia", isBuilt: true),

            // Região 10 — Picos de Farnkesh (☆, Docs/Mapas100.md §1; zones of its continuous region, OverworldLayouts.Farnkesh)
            new MapDefinition(53, "map_053_vale_satiros", R.PicosDeFarnkesh, Y.Forest, S.G, 140, 148, 136, 144, Fee, T.None, ValeDosSatiros, K.RaceCity, "vale_dos_satiros", isBuilt: true),
            new MapDefinition(54, "map_054_farnkesh", R.PicosDeFarnkesh, Y.Mountain, S.GG, 145, 152, 141, 149, Fee, T.None, Farnkesh, K.Capital, "farnkesh", isBuilt: true),
            new MapDefinition(55, "map_055_pontes_vento", R.PicosDeFarnkesh, Y.Mountain, S.G, 150, 160, 146, 154, Fee, T.None, Farnkesh, K.Outpost, "ermida_baixa", isBuilt: true),
            new MapDefinition(56, "map_056_ermidas", R.PicosDeFarnkesh, Y.Mountain, S.G, 158, 168, 154, 162, Fee, T.None, Farnkesh, K.Outpost, "pinaculo_do_sino", isBuilt: true),
            new MapDefinition(57, "map_057_garganta_ays", R.PicosDeFarnkesh, Y.Mountain, S.G, 166, 180, 162, 170, Fee, T.StormProtection, Farnkesh, K.Outpost, "gruta_abrigo", isBuilt: true),
            new MapDefinition(58, "map_058_ninho_huma", R.PicosDeFarnkesh, Y.Mountain, S.GG, 180, 200, 176, 186, Fee, T.WingsOrDoubleJump, Farnkesh, K.Outpost, "ultimo_pinaculo", isBuilt: true),

            // Região 11 — Mar Gelado (☆, Docs/Mapas100.md §1; zones of its continuous region, OverworldLayouts.MarGelado)
            new MapDefinition(59, "map_059_porto_lua_branca", R.MarGelado, Y.Coast, S.GG, 180, 188, 176, 184, Fee, T.None, PortoDaLuaBranca, K.Capital, "porto_da_lua_branca", isBuilt: true),
            new MapDefinition(60, "map_060_canais_gelo", R.MarGelado, Y.Coast, S.G, 186, 196, 182, 190, Fee, T.None, PortoDaLuaBranca, K.Outpost, "casa_de_neve_dos_barqueiros", isBuilt: true),
            new MapDefinition(61, "map_061_tundra_omol", R.MarGelado, Y.Plain, S.GG, 194, 204, 190, 198, Fee, T.None, PortoDaLuaBranca, K.Outpost, "tenda_de_yen", isBuilt: true),
            new MapDefinition(62, "map_062_galerias_mamutes", R.MarGelado, Y.Underground, S.G, 202, 214, 198, 206, Fee, T.None, PortoDaLuaBranca, K.Outpost, "boca_do_gelo", isBuilt: true),
            new MapDefinition(63, "map_063_mar_auroras", R.MarGelado, Y.Coast, S.GG, 212, 226, 208, 216, Fee, T.BlizzardResistance, PortoDaLuaBranca, K.Outpost, "farol_de_gelo", isBuilt: true),
            new MapDefinition(64, "map_064_fundo_velho_kul", R.MarGelado, Y.Coast, S.GG, 226, 240, 222, 230, Fee, T.ThermalClothes, PortoDaLuaBranca, K.Outpost, "buraco_grande_de_pesca", isBuilt: true),

            // Região 12 — Arquipélago de Hinokura (☆, Docs/Mapas100.md §1; zones of its continuous region, OverworldLayouts.Hinokura)
            new MapDefinition(65, "map_065_hinokura", R.ArquipelagoDeHinokura, Y.Coast, S.GG, 220, 228, 216, 224, Fee, T.None, Hinokura, K.Capital, "hinokura", isBuilt: true),
            new MapDefinition(66, "map_066_jardins_lava", R.ArquipelagoDeHinokura, Y.Plain, S.G, 226, 236, 222, 230, Fee, T.None, Hinokura, K.Outpost, "casa_de_cha_de_pedra", isBuilt: true),
            new MapDefinition(67, "map_067_monte_echeyde", R.ArquipelagoDeHinokura, Y.Mountain, S.GG, 234, 246, 230, 238, Fee, T.ClimbingCrampons, Hinokura, K.Outpost, "abrigo_dos_pastores", isBuilt: true),
            new MapDefinition(68, "map_068_recife_obsidiana", R.ArquipelagoDeHinokura, Y.Coast, S.G, 244, 254, 240, 248, Fee, T.BoatOrDock, Hinokura, K.Outpost, "barco_farol", isBuilt: true),
            new MapDefinition(69, "map_069_nove_fornalhas", R.ArquipelagoDeHinokura, Y.Ruins, S.GG, 252, 264, 248, 256, Fee, T.None, Hinokura, K.Outpost, "portao_vermelho", isBuilt: true),
            new MapDefinition(70, "map_070_caldeira_fogo_devorador", R.ArquipelagoDeHinokura, Y.Mountain, S.GG, 264, 280, 260, 268, Fee, T.FreshWater, Hinokura, K.Outpost, "borda_fria", isBuilt: true),

            // Região 13 — Rio das Doze Horas (☆, Docs/Mapas100.md §1; zones of its continuous region, OverworldLayouts.Rio)
            new MapDefinition(71, "map_071_cais_mesektet", R.RioDasDozeHoras, Y.Coast, S.GG, 260, 268, 256, 264, Fee, T.None, CaisDeMesektet, K.Capital, "cais_de_mesektet", isBuilt: true),
            new MapDefinition(72, "map_072_campos_juncos", R.RioDasDozeHoras, Y.Plain, S.G, 266, 276, 262, 270, Fee, T.SaddleRequired, CaisDeMesektet, K.Outpost, "casa_de_junco", isBuilt: true),
            new MapDefinition(73, "map_073_areal_escondido", R.RioDasDozeHoras, Y.Desert, S.G, 274, 286, 270, 278, Fee, T.TorchRequired, CaisDeMesektet, K.Outpost, "treno_de_areia", isBuilt: true),
            new MapDefinition(74, "map_074_aneis_mehen", R.RioDasDozeHoras, Y.Ruins, S.G, 284, 296, 280, 288, Fee, T.RunicLamp, CaisDeMesektet, K.Outpost, "anel_quieto", isBuilt: true),
            new MapDefinition(75, "map_075_lago_de_fogo", R.RioDasDozeHoras, Y.Coast, S.G, 292, 306, 288, 296, Fee, T.EternalFireOil, CaisDeMesektet, K.Outpost, "ilha_das_palmeiras", isBuilt: true),
            new MapDefinition(76, "map_076_banco_apep", R.RioDasDozeHoras, Y.Desert, S.GG, 304, 320, 300, 308, Fee, T.AntiVenomAmulet, CaisDeMesektet, K.Outpost, "proa_encalhada", isBuilt: true),

            // Região 14 — Margens da Mistura (☆, Docs/Mapas100.md §1; zones of its continuous region, OverworldLayouts.Mistura)
            new MapDefinition(77, "map_077_dur_jarras", R.MargensDaMistura, Y.Plain, S.GG, 300, 308, 296, 304, Fee, T.None, DurJarras, K.Capital, "dur_jarras", isBuilt: true),
            new MapDefinition(78, "map_078_deserto_relogio", R.MargensDaMistura, Y.Desert, S.GG, 306, 316, 302, 310, Fee, T.FreshWater, DurJarras, K.Outpost, "gnomon_caido", isBuilt: true),
            new MapDefinition(79, "map_079_pocos_betume", R.MargensDaMistura, Y.Swamp, S.G, 314, 326, 310, 318, Fee, T.SaddleRequired, DurJarras, K.Outpost, "balsa_de_junco", isBuilt: true),
            new MapDefinition(80, "map_080_cedral_hobabish", R.MargensDaMistura, Y.Forest, S.GG, 322, 336, 318, 326, Fee, T.ElevatedPlatforms, DurJarras, K.Outpost, "clareira_do_lenhador", isBuilt: true),
            new MapDefinition(81, "map_081_ombro_upelluri", R.MargensDaMistura, Y.Mountain, S.GG, 334, 346, 330, 338, Fee, T.ClimbingCrampons, DurJarras, K.Outpost, "ninho_no_ombro", isBuilt: true),
            new MapDefinition(82, "map_082_tabuas_sonho", R.MargensDaMistura, Y.Ruins, S.GG, 344, 360, 340, 348, Fee, T.TorchRequired, DurJarras, K.Outpost, "tenda_dos_interpretes", isBuilt: true),

            // Região 15 — Limiar Profundo (☆, Docs/Mapas100.md §1; zones of its continuous region, OverworldLayouts.Profundo)
            new MapDefinition(83, "map_083_fortaleza_ultima_lampada", R.LimiarProfundo, Y.Ruins, S.GG, 340, 350, 336, 344, Fee, T.None, FortalezaUltimaLampada, K.Capital, "fortaleza_ultima_lampada", isBuilt: true),
            new MapDefinition(84, "map_084_cova_fumaca", R.LimiarProfundo, Y.Underground, S.G, 348, 362, 344, 352, Fee, T.PoisonProtection, FortalezaUltimaLampada, K.Outpost, "lampada_1", isBuilt: true),
            new MapDefinition(85, "map_085_cova_fogo_devorador", R.LimiarProfundo, Y.Underground, S.G, 360, 374, 356, 364, Fee, T.EternalFireOil, FortalezaUltimaLampada, K.Outpost, "lampada_2", isBuilt: true),
            new MapDefinition(86, "map_086_cova_vento_destruidor", R.LimiarProfundo, Y.Underground, S.G, 372, 388, 368, 376, Fee, T.WingsOrDoubleJump, FortalezaUltimaLampada, K.Outpost, "lampada_3", isBuilt: true),
            new MapDefinition(87, "map_087_cova_lodo", R.LimiarProfundo, Y.Underground, S.G, 386, 402, 382, 390, Fee, T.AirBubble, FortalezaUltimaLampada, K.Outpost, "lampada_4", isBuilt: true),
            new MapDefinition(88, "map_088_cova_treva", R.LimiarProfundo, Y.Underground, S.G, 400, 416, 396, 404, Fee, T.RunicLamp, FortalezaUltimaLampada, K.Outpost, "lampada_5", isBuilt: true),
            new MapDefinition(89, "map_089_ponte_novelo", R.LimiarProfundo, Y.Ruins, S.GG, 412, 428, 408, 416, Fee, T.RootBridgeBalance, FortalezaUltimaLampada, K.Outpost, "pilar_do_meio", isBuilt: true),
            new MapDefinition(90, "map_090_portao_terra_treva", R.LimiarProfundo, Y.Ruins, S.GG, 424, 440, 420, 428, Fee, T.TorchRequired, FortalezaUltimaLampada, K.Outpost, "selo_dos_artifices", isBuilt: true),

            // Região 16 — Reinos do Céu (☆, Docs/Mapas100.md §1; zones of its continuous region, OverworldLayouts.Ceu)
            new MapDefinition(91, "map_091_cais_da_coluna", R.ReinosDoCeu, Y.Plain, S.GG, 420, 430, 416, 424, Fee, T.None, CaisDaColuna, K.Capital, "cais_da_coluna", isBuilt: true),
            new MapDefinition(92, "map_092_rio_de_luz", R.ReinosDoCeu, Y.Coast, S.GG, 428, 440, 424, 432, Fee, T.None, CaisDaColuna, K.Outpost, "boia_de_luz", isBuilt: true),
            new MapDefinition(93, "map_093_barca_da_lua", R.ReinosDoCeu, Y.Ruins, S.G, 436, 450, 432, 440, Fee, T.None, CaisDaColuna, K.Outpost, "cabine_da_proa", isBuilt: true),
            new MapDefinition(94, "map_094_roda_celeste", R.ReinosDoCeu, Y.Ruins, S.GG, 446, 460, 442, 450, Fee, T.ElevatedPlatforms, CaisDaColuna, K.Outpost, "eixo_da_roda", isBuilt: true),
            new MapDefinition(95, "map_095_esferas_arcontes", R.ReinosDoCeu, Y.Ruins, S.GG, 456, 470, 452, 460, Fee, T.WingsOrDoubleJump, CaisDaColuna, K.Outpost, "ilhota_parada", isBuilt: true),
            new MapDefinition(96, "map_096_jardim_de_luz", R.ReinosDoCeu, Y.Forest, S.G, 466, 478, 462, 470, Fee, T.None, CaisDaColuna, K.Outpost, "pergola_do_jardineiro", isBuilt: true),
            new MapDefinition(97, "map_097_barca_do_sol", R.ReinosDoCeu, Y.Ruins, S.G, 474, 486, 470, 478, Fee, T.None, CaisDaColuna, K.Outpost, "cabine_da_popa", isBuilt: true),
            new MapDefinition(98, "map_098_pes_da_coluna", R.ReinosDoCeu, Y.Mountain, S.GG, 482, 492, 478, 486, Fee, T.None, CaisDaColuna, K.Outpost, "degrau_um", isBuilt: true),
            new MapDefinition(99, "map_099_mirante_do_ceu", R.ReinosDoCeu, Y.Mountain, S.GG, 488, 498, 484, 492, Fee, T.StormProtection, CaisDaColuna, K.Outpost, "mao_aberta", isBuilt: true),
            new MapDefinition(100, "map_100_varanda", R.ReinosDoCeu, Y.Plain, S.M, 495, 500, 491, 499, Fee, T.None, CaisDaColuna, K.Outpost, "a_varanda", isBuilt: true),
        };

        public static readonly IReadOnlyList<MapRegion> Regions = new[]
        {
            R.Zarafshan, R.AreiasFlamejantes, R.SerraDeKaggen, R.GrutasDoLimiar, R.CostaDasMoncoes, R.DeltaDeJhilpur, R.TetoDoMundo, R.PlanicieDeMingcheng,
            R.EstepeDosNartes, R.PicosDeFarnkesh, R.MarGelado, R.ArquipelagoDeHinokura, R.RioDasDozeHoras, R.MargensDaMistura, R.LimiarProfundo, R.ReinosDoCeu,
        };

        private static Dictionary<string, MapDefinition> _byId;

        public static MapDefinition Get(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return null;
            }

            _byId ??= Maps.ToDictionary(m => m.Id);
            return _byId.TryGetValue(id, out MapDefinition map) ? map : null;
        }

        public static MapDefinition Get(int mapId) => Maps.FirstOrDefault(m => m.MapId == mapId);

        public static IEnumerable<MapDefinition> InRegion(MapRegion region) => Maps.Where(m => m.Region == region);

        public static IEnumerable<MapDefinition> Built => Maps.Where(m => m.IsBuilt);

        /// <summary>Map id → its return city (<see cref="Rapadura.Gameplay.World.Cities.SafeZoneRules.RespawnCity"/> lookup).</summary>
        public static string ReturnCityOf(string mapId) => Get(mapId)?.ReturnCityId;

        /// <summary>
        /// The city a defeat on <paramref name="mapId"/> wakes the hero in, among the cities of this build: the map's return
        /// city when it exists in <see cref="CityCatalog"/>, else the region's existing city, else the Vila (the start).
        /// </summary>
        public static CitySpec RespawnCityFor(string mapId)
        {
            string id = SafeZoneRules.RespawnCity(mapId, ReturnCityOf, CityCatalog.VilaZefa);
            CitySpec city = CityCatalog.Get(id);
            if (city != null)
            {
                return city;
            }

            BiomeType? legacy = Get(mapId)?.LegacyBiome;
            return CityCatalog.ForRegion(legacy) ?? CityCatalog.Get(CityCatalog.VilaZefa);
        }

        /// <summary>The biome of today's world map a region maps onto (the first four), else null.</summary>
        public static BiomeType? LegacyBiomeOf(MapRegion region)
        {
            switch (region)
            {
                case R.Zarafshan: return BiomeType.Forest;
                case R.AreiasFlamejantes: return BiomeType.Desert;
                case R.SerraDeKaggen: return BiomeType.Mountain;
                case R.GrutasDoLimiar: return BiomeType.Caves;
                default: return null;
            }
        }

        /// <summary>The map region of a world-map biome (each biome is the first region of the same theme).</summary>
        public static MapRegion RegionOf(BiomeType biome)
        {
            switch (biome)
            {
                case BiomeType.Desert: return R.AreiasFlamejantes;
                case BiomeType.Mountain: return R.SerraDeKaggen;
                case BiomeType.Caves: return R.GrutasDoLimiar;
                default: return R.Zarafshan;
            }
        }
    }
}
