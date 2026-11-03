using System.Collections.Generic;
using System.Linq;
using Rapadura.Core.Localization;

namespace Rapadura.Gameplay.World.Map
{
    /// <summary>
    /// Localization (en + pt-BR) of the 100-maps system: map, settlement, region and return-city names
    /// (Docs/Mapas100.md §1), the special gates (Docs/MetodologiaMapasERacas.md §2), the Barcas de Luz window (M) and the
    /// floating lines of the playable maps. Same self-contained pattern as <c>CityWalkTexts</c>.
    /// </summary>
    public static class MapTexts
    {
        public const string WindowTitle = "ui.map_travel.title";
        public const string WindowSubtitle = "ui.map_travel.subtitle";
        /// <summary>{0} = hero name, {1} = level.</summary>
        public const string HeroLine = "ui.map_travel.hero";
        /// <summary>{0} = level.</summary>
        public const string HeroLineNoName = "ui.map_travel.hero_any";
        /// <summary>{0} = gold.</summary>
        public const string Gold = "ui.map_travel.gold";
        public const string Overloaded = "ui.map_travel.overloaded";
        /// <summary>{0} = min level, {1} = max level, {2} = settlement line.</summary>
        public const string RowMeta = "ui.map_travel.row_meta";
        /// <summary>{0} = warp level, {1} = walk level, {2} = gold.</summary>
        public const string RequirementLine = "ui.map_travel.requirement";
        /// <summary>{0} = warp level, {1} = walk level.</summary>
        public const string RequirementLineFree = "ui.map_travel.requirement_free";
        /// <summary>{0} = gold, {1} = walk level.</summary>
        public const string OpenLine = "ui.map_travel.open";
        /// <summary>{0} = walk level.</summary>
        public const string OpenLineFree = "ui.map_travel.open_free";
        public const string StatusHere = "ui.map_travel.here";
        public const string StatusNotBuilt = "ui.map_travel.not_built";
        public const string StatusBusy = "ui.map_travel.busy";
        /// <summary>{0} = requirement.</summary>
        public const string StatusRequirement = "ui.map_travel.requirement_missing";
        /// <summary>{0} = gold.</summary>
        public const string StatusNoGold = "ui.map_travel.no_gold";
        /// <summary>{0} = warp level.</summary>
        public const string StatusLevel = "ui.map_travel.level";
        /// <summary>{0} = walk level.</summary>
        public const string LevelHint = "ui.map_travel.level_hint";
        /// <summary>{0} = gold.</summary>
        public const string Go = "ui.map_travel.go";
        public const string GoFree = "ui.map_travel.go_free";
        public const string Close = "ui.map_travel.close";
        public const string CompassButton = "ui.map_travel.compass";
        public const string CitiesHeader = "ui.map_travel.cities_header";
        /// <summary>{0} = map name.</summary>
        public const string OnTheWay = "ui.map_travel.on_the_way";
        /// <summary>{0} = map name, {1} = min level, {2} = max level.</summary>
        public const string Arrived = "ui.map_travel.arrived";
        public const string DangerWarning = "ui.map_travel.danger";
        /// <summary>{0} = walk level.</summary>
        public const string WalkBlocked = "ui.map_travel.walk_blocked";
        public const string ExitToWorldMap = "ui.map_travel.exit_world";
        public const string ExitClosed = "ui.map_travel.exit_closed";
        /// <summary>{0} = settlement.</summary>
        public const string OutpostSafe = "ui.map_travel.outpost_safe";
        public const string DockCallout = "ui.map_travel.dock";
        public const string ReasonLevel = "ui.map_travel.reason_level";
        public const string ReasonGold = "ui.map_travel.reason_gold";
        /// <summary>{0} = settlement name.</summary>
        public const string KindCity = "ui.map_travel.kind_city";
        public const string KindRaceCity = "ui.map_travel.kind_race_city";
        public const string KindCapital = "ui.map_travel.kind_capital";
        public const string KindOutpost = "ui.map_travel.kind_outpost";

        public static string RegionKey(MapRegion region) => "map.region." + region.ToString().ToLowerInvariant() + ".name";

        public static string ReturnCityKey(string cityId) => "map.city." + cityId + ".name";

        public static string RequirementKey(string tag) => "map.requirement." + tag;

        /// <summary>Where the gear of a gate comes from (MapRequirementRules), shown when the barca refuses for it.</summary>
        public static string RequirementHintKey(string tag) => "map.requirement." + tag + ".hint";

        public static string KindKey(MapSettlementKind kind)
        {
            switch (kind)
            {
                case MapSettlementKind.RaceCity: return KindRaceCity;
                case MapSettlementKind.Capital: return KindCapital;
                case MapSettlementKind.Outpost: return KindOutpost;
                default: return KindCity;
            }
        }

        // id, en, pt, settlement slug, settlement en, settlement pt
        private static readonly (string Id, string En, string Pt, string SettlementEn, string SettlementPt)[] MapNames =
        {
            ("map_001_pomar", "Grandma Edna's Orchard", "Pomar da Vó Edna", "Grandma Edna's Village", "Vila da Vó Edna"),
            ("map_002_tugai", "Golden River Tugai", "Tugai do Rio Dourado", "Orchard Mill", "Moinho do Pomar"),
            ("map_003_brejo", "Dark Mire Marsh", "Brejo do Lodo Escuro", "Aryk Bridge", "Ponte do Aryk"),
            ("map_004_amoreiras", "Silk Mulberry Grove", "Bosque das Amoreiras de Seda", "Loom House", "Casa do Tear"),
            ("map_005_garajonay", "Garajonay Canopy", "Copas de Garajonay", "Garajonay Canopy", "Copas de Garajonay"),
            ("map_006_mihrshah", "Mihrshah's Forgotten Garden", "Jardim Esquecido de Mihrshah", "Garden Gate", "Portão do Jardim"),
            ("map_007_caravanas", "Caravan Road", "Estrada das Caravanas", "Ammo's Caravanserai", "Caravançará do Ammo"),
            ("map_008_artifices", "Artificers' Ruins", "Ruínas dos Artífices", "Pyramid Camp", "Acampamento das Pirâmides"),
            ("map_009_karez", "Karez Galleries", "Galerias do Karez", "Karez Mouth", "Boca do Karez"),
            ("map_010_dunas", "Singing Dunes", "Dunas que Cantam", "Date Palm Well", "Poço das Tamareiras"),
            ("map_011_tambor_rubro", "Red Drum Badlands", "Ermos do Tambor Rubro", "Red Drum", "Tambor Rubro"),
            ("map_012_namrael", "Namrael's Throne", "Trono de Namrael", "Rammed-Earth Tower", "Torre de Taipa"),
            ("map_013_rondaveis", "Rondavel Trail", "Trilha dos Rondavéis", "Litema Village", "Vila de Litema"),
            ("map_014_hahya", "Hahya's Pass", "Passo do Hahya", "Mihr's Mine Mouth", "Boca da Mina do Mihr"),
            ("map_015_abrigos", "Painted Shelters", "Abrigos Pintados", "Eland Shelter", "Abrigo do Eland"),
            ("map_016_pedra_seca", "Drystone Walls", "Muralhas de Pedra Seca", "Curved Enclosure", "Recinto Curvo"),
            ("map_017_napirasu", "Napirasu's Bronze Halls", "Salões de Bronze de Napirasu", "Bronze Halls", "Salões de Bronze"),
            ("map_018_mazans", "Mazan Summit", "Cume dos Mazans", "High Stela", "Estela Alta"),
            ("map_019_lago_limiar", "Threshold Lake", "Lago do Limiar", "Lampkeepers' Village", "Vila das Lamparinas"),
            ("map_020_tages", "Tages' Hall", "Salão de Tages", "Bridge Niche", "Nicho da Ponte"),
            ("map_021_pedras_dragao", "Dragonstone Field", "Campo das Pedras-Dragão", "Great Vishapakar", "Vishapakar Grande"),
            ("map_022_mercado_tuneis", "Tunnel Market", "Mercado dos Túneis", "Tunnel Market", "Mercado dos Túneis"),
            ("map_023_canion_roda", "Wheel Canyon", "Cânion da Roda", "Tower of the Jars", "Torre das Jarras"),
            ("map_024_mahaway", "Mahaway's Roost", "Ninho de Mahaway", "Last Rope Bridge", "Última Ponte de Corda"),
            ("map_025_taa_pwani", "Taa Pwani and the Reef", "Taa Pwani e o Recife", "Taa Pwani", "Taa Pwani"),
            ("map_026_auroques", "Aurochs Savanna", "Savana dos Auroques", "Aratta", "Aratta"),
            ("map_027_meroe", "High Pyramids of Meroë", "Pirâmides Altas de Meroé", "Courtyard of Reliefs", "Pátio dos Relevos"),
            ("map_028_kirta", "Kirta Reef City", "Cidade-Recife de Kirta", "Kirta", "Kirta"),
            ("map_029_kilwa", "Sunken Kilwa", "Kilwa Afundada", "Coral Lighthouse", "Farol de Coral"),
            ("map_030_liyongo", "Liyongo Mangroves", "Mangues de Liyongo", "Dhow Quay", "Cais das Dhows"),
            ("map_031_jhilpur", "Jhilpur, City in the Roots", "Jhilpur, a Cidade nas Raízes", "Jhilpur", "Jhilpur"),
            ("map_032_raiz_viva", "Living Root Bridges", "Pontes de Raiz Viva", "Heron Platform", "Plataforma das Garças"),
            ("map_033_tarchon", "Tarchon Stilts", "Palafitas de Tarchon", "Tarchon Stilts", "Palafitas de Tarchon"),
            ("map_034_ninki_nanka", "Ninki Nanka Bog", "Charco do Ninki Nanka", "Lantern Islet", "Ilhota das Lanternas"),
            ("map_035_coracao_lodo", "Heart of the Mire", "Coração do Lodo", "Mother Root", "Raiz-Mãe"),
            ("map_036_sangkent", "Sangkent, the Stair City", "Sangkent, a Cidade-Escadaria", "Sangkent", "Sangkent"),
            ("map_037_calhas", "Stone Chutes", "Calhas de Pedra", "High Chute Station", "Estação da Calha Alta"),
            ("map_038_geleira", "Roof-of-the-World Glacier", "Geleira do Teto do Mundo", "Yak Hut", "Cabana do Iaque"),
            ("map_039_forte_ventos", "Fort of the Winds", "Forte dos Ventos", "The Fort Itself", "O Próprio Forte"),
            ("map_040_wakhan", "Wakhan Pass", "Passo de Wakhan", "Stone Caravanserai", "Caravançará de Pedra"),
            ("map_041_mingcheng", "Mingcheng, City of Nine Rings", "Mingcheng, a Cidade dos Nove Anéis", "Mingcheng", "Mingcheng"),
            ("map_042_torres_brancas", "White Spires of Amanirenas", "Torres Brancas de Amanirenas", "White Spires", "Torres Brancas"),
        };

        private static readonly (MapRegion Region, string En, string Pt)[] RegionNames =
        {
            (MapRegion.Zarafshan, "Zarafshan Woods", "Bosque do Zarafshan"),
            (MapRegion.AreiasFlamejantes, "Flaming Sands", "Areias Flamejantes"),
            (MapRegion.SerraDeKaggen, "Kaggen Range", "Serra de Kaggen"),
            (MapRegion.GrutasDoLimiar, "Threshold Caves", "Grutas do Limiar"),
            (MapRegion.CostaDasMoncoes, "Monsoon Coast", "Costa das Monções"),
            (MapRegion.DeltaDeJhilpur, "Jhilpur Delta", "Delta de Jhilpur"),
            (MapRegion.TetoDoMundo, "Roof of the World", "Teto do Mundo"),
            (MapRegion.PlanicieDeMingcheng, "Mingcheng Plain", "Planície de Mingcheng"),
        };

        private static readonly (string Id, string En, string Pt)[] CityNames =
        {
            ("vila_zefa", "Grandma Edna's Village", "Vila da Vó Edna"),
            ("copas_garajonay", "Garajonay Canopy", "Copas de Garajonay"),
            ("areia_branca", "Qocho Oasis", "Oásis de Qocho"),
            ("tambor_rubro", "Red Drum", "Tambor Rubro"),
            ("serra_alta", "Litema Village", "Vila de Litema"),
            ("saloes_de_bronze", "Bronze Halls", "Salões de Bronze"),
            ("lamparinas", "Lampkeepers' Village", "Vila das Lamparinas"),
            ("mercado_dos_tuneis", "Tunnel Market", "Mercado dos Túneis"),
            ("taa_pwani", "Taa Pwani", "Taa Pwani"),
            ("aratta", "Aratta", "Aratta"),
            ("kirta", "Kirta", "Kirta"),
            ("jhilpur", "Jhilpur", "Jhilpur"),
            ("palafitas_tarchon", "Tarchon Stilts", "Palafitas de Tarchon"),
            ("sangkent", "Sangkent", "Sangkent"),
            ("mingcheng", "Mingcheng", "Mingcheng"),
            ("torres_brancas", "White Spires", "Torres Brancas"),
        };

        private static readonly (string Tag, string En, string Pt)[] RequirementHints =
        {
            (MapRequirementTags.LeatherBoots, "Wear any boots: Leather Boots at Mariam's haberdashery (Vila da Vó Edna). Joaquim does not sink in the mud.", "Calce qualquer bota: Botas de Couro no Armarinho da Mariam (Vila da Vó Edna). O Joaquim não afunda no lodo."),
            (MapRequirementTags.ElevatedPlatforms, "Carry a Climbing Rope (Mariam's haberdashery, Vila da Vó Edna) or wear wings.", "Leve uma Corda de Escalada (Armarinho da Mariam, Vila da Vó Edna) ou use asas."),
            (MapRequirementTags.GardenKey, "Mariam gives the Garden Key for clearing the spider webs; after the Forest boss she also sells a copy.", "A Mariam dá a Chave do Jardim por limpar as teias; depois do chefe da Floresta ela também vende uma cópia."),
            (MapRequirementTags.FreshWater, "Carry a Waterskin of Fresh Water from the Qocho Oasis saddlery.", "Leve um Odre de Água Fresca da selaria do Oásis de Qocho."),
            (MapRequirementTags.PoisonProtection, "Carry an Oasis Theriac (Qocho) or an anti-venom amulet. Lavine breathes the poison mists.", "Leve uma Teriaga do Oásis (Qocho) ou um amuleto antiveneno. A Lavine respira as névoas venenosas."),
            (MapRequirementTags.WoolCloak, "Carry a Wool Cloak (Litema forge, or Mestre Mihr's mine-pass quest) or wear the Nart Felt Cloak.", "Leve uma Manta de Lã (forja de Litema ou a missão do passo da mina do Mestre Mihr) ou vista a Capa de Feltro dos Nartes."),
            (MapRequirementTags.PickaxeTorch, "Carry a pickaxe and a lamp: Oil Lamp at Parthenos's lamps (Vila das Lamparinas).", "Leve picareta e lamparina: Lamparina de Óleo nas Lamparinas da Parthenos (Vila das Lamparinas)."),
            (MapRequirementTags.BlizzardResistance, "Carry Tandir Ginger Tea (Litema forge) or wear fine thermal clothes.", "Leve Chá de Gengibre do Tandir (forja de Litema) ou vista roupa térmica refinada."),
            (MapRequirementTags.TorchRequired, "Carry an Oil Lamp: Parthenos sells it in the Vila das Lamparinas and gives one for scattering the bats.", "Leve uma Lamparina de Óleo: a Parthenos vende na Vila das Lamparinas e dá uma por espantar os morcegos."),
            (MapRequirementTags.RunicLamp, "Carry a Runic Lamp: Parthenos sells it once the Caves boss has fallen.", "Leve uma Lamparina Rúnica: a Parthenos vende depois do chefe das Cavernas."),
            (MapRequirementTags.EternalFireOil, "Carry Eternal Fire Oil: Parthenos sells it once the Caves boss has fallen.", "Leve Óleo de Fogo Eterno: a Parthenos vende depois do chefe das Cavernas."),
            (MapRequirementTags.WingsOrDoubleJump, "Wear Takhsich's Canvas Wings (Miwnay's Fair, Tunnel Market) or wings of the Enchanted Cauldron, or own a winged mount.", "Vista as Asas de Lona do Takhsich (Feira da Miwnay, Mercado dos Túneis) ou asas do Tacho Encantado, ou tenha uma montaria alada."),
            (MapRequirementTags.BoatOrDock, "Carry a Dhow Passage (Taa Pwani) or own a water mount.", "Leve uma Passagem de Dhow (Taa Pwani) ou tenha uma montaria aquática."),
            (MapRequirementTags.ArtificerRelic, "Carry an Artificers' Relic (Aratta).", "Leve uma Relíquia dos Artífices (Aratta)."),
            (MapRequirementTags.BreathingShell, "Carry a Breathing Shell (Kirta).", "Leve uma Concha Respirável (Kirta)."),
            (MapRequirementTags.AirBubble, "Carry a Bottled Air Bubble or a Breathing Shell (Kirta).", "Leve uma Bolha de Ar Engarrafada ou uma Concha Respirável (Kirta)."),
            (MapRequirementTags.RootBoots, "Wear Living-Root Boots (Taa Pwani) or own a water mount. Joaquim does not sink in the mud.", "Calce Botas de Raiz Viva (Taa Pwani) ou tenha uma montaria aquática. O Joaquim não afunda no lodo."),
            (MapRequirementTags.RootBridgeBalance, "Carry a Balance Pole (Jhilpur). Maithe walks the root bridges on her own.", "Leve uma Vara de Equilíbrio (Jhilpur). A Maithe atravessa as pontes de raiz sozinha."),
            (MapRequirementTags.SaddleRequired, "Carry a Reed Boat (Tarchon stilt houses) or own a water mount.", "Leve um Barco de Junco (Palafitas de Tarchon) ou tenha uma montaria aquática."),
            (MapRequirementTags.AntiVenomAmulet, "Carry an Anti-Venom Amulet (Tarchon stilt houses).", "Leve um Amuleto Antiveneno (Palafitas de Tarchon)."),
            (MapRequirementTags.PurifyingFruit, "Carry a Purifying Fruit (Jhilpur).", "Leve uma Fruta Purificadora (Jhilpur)."),
            (MapRequirementTags.ThermalClothes, "Wear Fine Thermal Clothes (Sangkent).", "Vista a Roupa Térmica Refinada (Sangkent)."),
            (MapRequirementTags.ClimbingCrampons, "Carry Climbing Crampons (Sangkent).", "Leve Grampos de Escalada (Sangkent)."),
            (MapRequirementTags.WingsRequired, "Wear wings of the Enchanted Cauldron or own a winged mount.", "Vista asas do Tacho Encantado ou tenha uma montaria alada."),
            (MapRequirementTags.StormProtection, "Carry a Storm Cloak (Sangkent).", "Leve uma Capa de Tempestade (Sangkent)."),
            (MapRequirementTags.SunSeal, "Carry the Sun Seal (Mingcheng).", "Leve o Selo do Sol (Mingcheng)."),
        };

        private static readonly (string Tag, string En, string Pt)[] RequirementNames =
        {
            (MapRequirementTags.LeatherBoots, "Leather boots", "Botas de couro"),
            (MapRequirementTags.ElevatedPlatforms, "Climbing the canopy platforms", "Subir às plataformas das copas"),
            (MapRequirementTags.GardenKey, "The Garden Key", "Chave do Jardim"),
            (MapRequirementTags.FreshWater, "Fresh water in the saddlebag", "Água fresca no alforje"),
            (MapRequirementTags.PoisonProtection, "Protection from poison", "Proteção contra veneno"),
            (MapRequirementTags.WoolCloak, "A wool cloak", "Manta de lã"),
            (MapRequirementTags.PickaxeTorch, "Pickaxe and torch", "Picareta e tocha"),
            (MapRequirementTags.BlizzardResistance, "Blizzard resistance", "Resistência à nevasca"),
            (MapRequirementTags.TorchRequired, "A lit lamp", "Lamparina acesa"),
            (MapRequirementTags.RunicLamp, "A runic lamp", "Lamparina rúnica"),
            (MapRequirementTags.EternalFireOil, "Eternal Fire oil", "Óleo de Fogo Eterno"),
            (MapRequirementTags.WingsOrDoubleJump, "Wings or a double jump", "Asas ou salto duplo"),
            (MapRequirementTags.BoatOrDock, "A boat or the quay", "Barco ou cais"),
            (MapRequirementTags.ArtificerRelic, "An Artificers' relic", "Relíquia dos Artífices"),
            (MapRequirementTags.BreathingShell, "A breathing shell", "Concha respirável"),
            (MapRequirementTags.AirBubble, "An air bubble or swimming", "Bolha de ar ou natação"),
            (MapRequirementTags.RootBoots, "Root boots", "Botas de raiz"),
            (MapRequirementTags.RootBridgeBalance, "Balance on root bridges", "Equilíbrio nas pontes de raiz"),
            (MapRequirementTags.SaddleRequired, "A reed boat or a swamp saddle", "Barco de junco ou sela do brejo"),
            (MapRequirementTags.AntiVenomAmulet, "An anti-venom amulet", "Amuleto antiveneno"),
            (MapRequirementTags.PurifyingFruit, "A purifying fruit", "Fruta purificadora"),
            (MapRequirementTags.ThermalClothes, "Fine thermal clothes", "Roupa térmica refinada"),
            (MapRequirementTags.ClimbingCrampons, "Climbing crampons", "Grampos de escalada"),
            (MapRequirementTags.WingsRequired, "Wings or a winged mount", "Asas ou montaria alada"),
            (MapRequirementTags.StormProtection, "Storm protection", "Proteção contra tempestade"),
            (MapRequirementTags.SunSeal, "The Sun Seal", "Selo do Sol"),
        };

        private static List<LocalizationEntry> _entries;

        public static IReadOnlyList<LocalizationEntry> Entries => _entries ??= Build();

        private static List<LocalizationEntry> Build()
        {
            var entries = new List<LocalizationEntry>
            {
                new LocalizationEntry(WindowTitle, "Light Ships · Travel", "Barcas de Luz · Mover"),
                new LocalizationEntry(WindowSubtitle, "The barca sails the river of light to the safe ground of every map: towns, outposts and the maps beyond. Locked maps show what they ask for.",
                    "A barca navega o rio de luz até a zona segura de cada mapa: cidades, postos e os mapas além. Os trancados dizem o que pedem."),
                new LocalizationEntry(HeroLine, "{0} · level {1}", "{0} · nível {1}"),
                new LocalizationEntry(HeroLineNoName, "Level {0}", "Nível {0}"),
                new LocalizationEntry(Gold, "Gold in the bag: {0}", "Ouro na mochila: {0}"),
                new LocalizationEntry(Overloaded, "Backpack overloaded: the fare doubles", "Mochila sobrecarregada: a passagem dobra"),
                new LocalizationEntry(RowMeta, "Level {0}–{1} · {2}", "Nível {0}–{1} · {2}"),
                new LocalizationEntry(RequirementLine, "Requires level {0} (or level {1} on foot) - Ship: {2} gold", "Exige Nível {0} (ou a pé nível {1}) - Barca: {2} ouro"),
                new LocalizationEntry(RequirementLineFree, "Requires level {0} (or level {1} on foot) - Ship: free", "Exige Nível {0} (ou a pé nível {1}) - Barca: grátis"),
                new LocalizationEntry(OpenLine, "Ship: {0} gold · on foot from level {1}", "Barca: {0} ouro · a pé desde o nível {1}"),
                new LocalizationEntry(OpenLineFree, "Ship: free · on foot from level {0}", "Barca: grátis · a pé desde o nível {0}"),
                new LocalizationEntry(StatusHere, "You are here", "Vocês estão aqui"),
                new LocalizationEntry(StatusNotBuilt, "Not open yet in this version", "Ainda não aberto nesta versão"),
                new LocalizationEntry(StatusBusy, "The barca waits: something is going on", "A barca espera: tem algo acontecendo"),
                new LocalizationEntry(StatusRequirement, "Needs: {0}", "Exige: {0}"),
                new LocalizationEntry(StatusNoGold, "Not enough gold (fare: {0})", "Ouro insuficiente (passagem: {0})"),
                new LocalizationEntry(StatusLevel, "Level {0} for the barca", "Nível {0} para a barca"),
                new LocalizationEntry(LevelHint, "You can try walking in from the neighbouring map from level {0}.", "Você pode tentar ir a pé pelo mapa vizinho a partir do nível {0}."),
                new LocalizationEntry(Go, "Travel · {0} gold", "Mover · {0} ouro"),
                new LocalizationEntry(GoFree, "Travel", "Mover"),
                new LocalizationEntry(Close, "Close", "Fechar"),
                new LocalizationEntry(CompassButton, "Map (M)", "Mapa (M)"),
                new LocalizationEntry(CitiesHeader, "Towns", "Cidades"),
                new LocalizationEntry(OnTheWay, "The barca sails to {0}…", "A barca parte para {0}…"),
                new LocalizationEntry(Arrived, "{0} · level {1}–{2}", "{0} · nível {1}–{2}"),
                new LocalizationEntry(DangerWarning, "Careful: the beasts here are far above your level", "Cuidado: os bichos daqui estão muito acima do seu nível"),
                new LocalizationEntry(WalkBlocked, "Not yet: on foot only from level {0}", "Ainda não: a pé só a partir do nível {0}"),
                new LocalizationEntry(ExitToWorldMap, "Back to the island map", "De volta ao mapa da ilha"),
                new LocalizationEntry(ExitClosed, "This road is not open yet", "Esta estrada ainda não foi aberta"),
                new LocalizationEntry(OutpostSafe, "{0} · safe outpost", "{0} · posto seguro"),
                new LocalizationEntry(DockCallout, "Ship Dock: choose where the barca goes", "Cais da Barca: escolha para onde a barca vai"),
                new LocalizationEntry(ReasonLevel, "Level too low", "Nível insuficiente"),
                new LocalizationEntry(ReasonGold, "Not enough gold", "Ouro insuficiente"),
                new LocalizationEntry(KindCity, "Town: {0}", "Cidade: {0}"),
                new LocalizationEntry(KindRaceCity, "Town of the peoples: {0}", "Cidade de raça: {0}"),
                new LocalizationEntry(KindCapital, "Capital: {0}", "Capital: {0}"),
                new LocalizationEntry(KindOutpost, "Outpost: {0}", "Posto: {0}"),
            };

            foreach ((string id, string en, string pt, string settlementEn, string settlementPt) in MapNames)
            {
                MapDefinition map = MapCatalog.Get(id);
                entries.Add(new LocalizationEntry("map." + id + ".name", en, pt));
                if (map != null && entries.All(e => e.Key != map.SettlementNameKey))
                {
                    entries.Add(new LocalizationEntry(map.SettlementNameKey, settlementEn, settlementPt));
                }
            }

            foreach ((MapRegion region, string en, string pt) in RegionNames)
            {
                entries.Add(new LocalizationEntry(RegionKey(region), en, pt));
            }

            foreach ((string id, string en, string pt) in CityNames)
            {
                entries.Add(new LocalizationEntry(ReturnCityKey(id), en, pt));
            }

            foreach ((string tag, string en, string pt) in RequirementNames)
            {
                entries.Add(new LocalizationEntry(RequirementKey(tag), en, pt));
            }

            foreach ((string tag, string en, string pt) in RequirementHints)
            {
                entries.Add(new LocalizationEntry(RequirementHintKey(tag), en, pt));
            }

            return entries;
        }

        private static Dictionary<string, string> _fallbackByKey;
        private static LocalizationManager _mergedInto;

        public static void EnsureRegistered(LocalizationManager manager)
        {
            if (manager == null || (ReferenceEquals(manager, _mergedInto) && manager.HasKey(WindowTitle)))
            {
                return;
            }

            manager.LoadEntries(Entries.Where(entry => !manager.HasKey(entry.Key)).ToList());
            _mergedInto = manager;
        }

        public static string Get(LocalizationManager manager, string key)
        {
            if (manager != null)
            {
                EnsureRegistered(manager);
                if (manager.HasKey(key))
                {
                    return manager.Get(key);
                }
            }

            return GetFallback(key) ?? key;
        }

        public static string Format(LocalizationManager manager, string key, params object[] args)
        {
            string format = Get(manager, key);
            try
            {
                return string.Format(format, args);
            }
            catch (System.FormatException)
            {
                return format;
            }
        }

        public static string GetFallback(string key)
        {
            if (_fallbackByKey == null)
            {
                _fallbackByKey = new Dictionary<string, string>();
                foreach (LocalizationEntry entry in Entries)
                {
                    _fallbackByKey[entry.Key] = entry.PtBR;
                }
            }

            return key != null && _fallbackByKey.TryGetValue(key, out string text) ? text : null;
        }

        public static string MapName(LocalizationManager manager, MapDefinition map) => map == null ? string.Empty : Get(manager, map.MapNameKey);

        /// <summary>"01 · Pomar da Vó Edna".</summary>
        public static string NumberedName(LocalizationManager manager, MapDefinition map) => map == null ? string.Empty : $"{map.MapId:00} · {MapName(manager, map)}";

        public static string SettlementName(LocalizationManager manager, MapDefinition map) => map == null ? string.Empty : Get(manager, map.SettlementNameKey);

        /// <summary>"Cidade: Vila da Vó Edna" / "Posto: Moinho do Pomar".</summary>
        public static string SettlementLine(LocalizationManager manager, MapDefinition map) =>
            map == null ? string.Empty : Format(manager, KindKey(map.Settlement), SettlementName(manager, map));

        public static string RegionName(LocalizationManager manager, MapRegion region) => Get(manager, RegionKey(region));

        public static string RequirementName(LocalizationManager manager, string tag) => string.IsNullOrEmpty(tag) ? string.Empty : Get(manager, RequirementKey(tag));

        public static string RequirementHint(LocalizationManager manager, string tag) => string.IsNullOrEmpty(tag) ? string.Empty : Get(manager, RequirementHintKey(tag));
    }
}
