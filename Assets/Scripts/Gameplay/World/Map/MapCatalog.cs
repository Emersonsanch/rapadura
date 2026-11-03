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
    /// Região 9 (☆ 47–52, the Estepe dos Nartes, walked as zones of its continuous region): level
    /// bands, walk/warp levels, special gates, settlements and return cities, as content-as-code like
    /// <c>CityCatalog</c>. Only <see cref="MapDefinition.IsBuilt"/> maps are playable today (01–42, 47–52): a hand-made layout
    /// (<c>MapLayouts.Has</c>: 01–08, 13, 14, 19, 20) or only a zone of a continuous region (<c>OverworldLayouts.ForMap</c>:
    /// 09–12, 15–18, 21–42, 47–52, their posts with a Cais da Barca); <c>MapLayouts.IsPlayable</c> is the runtime check and a test keeps
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

        public const string PomarId = "map_001_pomar";
        public const string TugaiId = "map_002_tugai";

        private const int Fee = MapAccessRules.BaseWarpFee;

        /// <summary>Every return city id a map may name (existing + planned), in region order.</summary>
        public static readonly IReadOnlyList<string> ReturnCityIds = new[]
        {
            CityCatalog.VilaZefa, CopasGarajonay, CityCatalog.AreiaBranca, TamborRubro, CityCatalog.SerraAlta, SaloesDeBronze,
            CityCatalog.Lamparinas, MercadoDosTuneis, TaaPwani, Aratta, Kirta, Jhilpur, PalafitasTarchon, Sangkent, Mingcheng, TorresBrancas,
            AcampamentoSoslan, Fortim, Arvoredo,
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
        };

        public static readonly IReadOnlyList<MapRegion> Regions = new[]
        {
            R.Zarafshan, R.AreiasFlamejantes, R.SerraDeKaggen, R.GrutasDoLimiar, R.CostaDasMoncoes, R.DeltaDeJhilpur, R.TetoDoMundo, R.PlanicieDeMingcheng,
            R.EstepeDosNartes,
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
