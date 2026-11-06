using System.Collections.Generic;
using System.Linq;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.World.Biomes;

namespace Rapadura.Gameplay.World.Cities
{
    /// <summary>
    /// A resident of a city the cousins can chat with in the city hub: a name and one line per cousin (the
    /// <c>city.resident.&lt;id&gt;.*</c> keys of <c>CityTexts</c>). Shopkeepers are residents too (<see cref="ShopId"/>).
    /// </summary>
    public sealed class CityResidentSpec
    {
        public readonly string ResidentId;

        /// <summary>The shop this resident keeps (<see cref="CityShopCatalog"/>), or null.</summary>
        public readonly string ShopId;

        private readonly string _nameKey;

        public CityResidentSpec(string residentId, string shopId = null, string nameKey = null)
        {
            ResidentId = residentId;
            ShopId = shopId;
            _nameKey = nameKey;
        }

        /// <summary>Own key, or a shared one (Mariam, Mestre Mihr... are also region quest-givers: same name).</summary>
        public string NameKey => _nameKey ?? KeyBase + ".name";

        public string KeyBase => "city.resident." + ResidentId;

        public string LineKey(CharacterId hero) => KeyBase + ".line." + hero.ToString().ToLowerInvariant();

        /// <summary>Line when no cousin is selected (map played on its own).</summary>
        public string AnyLineKey => KeyBase + ".line.any";
    }

    /// <summary>One region's city: name, flavour, residents, shops and which village-only services it offers.</summary>
    public sealed class CitySpec
    {
        public readonly string CityId;
        public readonly BiomeType Region;
        public readonly IReadOnlyList<CityResidentSpec> Residents;

        /// <summary>Shops with a counter in this city (<see cref="CityShopCatalog"/> ids, or a <c>Resources/Shops</c> asset id).</summary>
        public readonly IReadOnlyList<string> ShopIds;

        /// <summary>
        /// Baú da Vó Edna and forno (tandir): only in her village (they are her chest and her stove; the Barcas de Luz
        /// brings the cousins back cheaply). See Docs/Cities.md.
        /// </summary>
        public readonly bool HasGrandmaServices;

        /// <summary>The personal-story villagers of <c>VillageNpcCatalog</c> (dialogue graphs) live here.</summary>
        public readonly bool HasStoryVillagers;

        public CitySpec(string cityId, BiomeType region, IReadOnlyList<CityResidentSpec> residents, IReadOnlyList<string> shopIds,
            bool hasGrandmaServices = false, bool hasStoryVillagers = false)
        {
            CityId = cityId;
            Region = region;
            Residents = residents;
            ShopIds = shopIds;
            HasGrandmaServices = hasGrandmaServices;
            HasStoryVillagers = hasStoryVillagers;
        }

        public string KeyBase => "city." + CityId;
        public string NameKey => KeyBase + ".name";

        /// <summary>Short label of the HUD button ("Vila", "Qocho"...).</summary>
        public string ButtonKey => KeyBase + ".button";
        public string FlavourKey => KeyBase + ".flavour";
    }

    /// <summary>
    /// Every region has a city (TODO.md 12.3a). The Floresta keeps the Vila da Vó Edna (story villagers, the old
    /// general store, Baú and Fogão); the Deserto, the Montanha and the Cavernas get their own town, residents and
    /// local shop. Content as code, like <c>RegionQuestCatalog</c>/<c>MealCatalog</c>; texts in <c>CityTexts</c>.
    /// </summary>
    public static class CityCatalog
    {
        public const string VilaZefa = "vila_zefa";
        public const string AreiaBranca = "areia_branca";
        public const string SerraAlta = "serra_alta";
        public const string Lamparinas = "lamparinas";

        /// <summary>
        /// Copas de Garajonay, the race city of the elfos-da-mata (map 05, Docs/Mapas100.md §3.3). Same id as
        /// <c>MapCatalog.CopasGarajonay</c> and <c>CityTownRecipes.GarajonayCityId</c> (never change it: the map uses it).
        /// </summary>
        public const string CopasGarajonay = "copas_garajonay";

        /// <summary>
        /// Taa Pwani, the Costa da Lâmpada: the city of return of the Costa das Monções (região 5, maps 25 and 30,
        /// Docs/Mapas100.md). Same id as <c>MapCatalog.TaaPwani</c> and <c>CityTownRecipes.TaaPwaniCityId</c>.
        /// </summary>
        public const string TaaPwani = "taa_pwani";

        /// <summary>
        /// Tambor Rubro, the race city of the orcs (map 11, return city of maps 11–12, Docs/Mapas100.md §3.3). Same id as
        /// <c>MapCatalog.TamborRubro</c> and <c>CityTownRecipes.TamborRubroCityId</c>.
        /// </summary>
        public const string TamborRubro = "tambor_rubro";

        /// <summary>
        /// Salões de Bronze de Napirasu, the race city of the dwarves (map 17, return city of maps 17–18, Docs/Mapas100.md §3.3).
        /// Same id as <c>MapCatalog.SaloesDeBronze</c> and <c>CityTownRecipes.BronzeHallsCityId</c>.
        /// </summary>
        public const string SaloesDeBronze = "saloes_de_bronze";

        /// <summary>
        /// Mercado dos Túneis, the race city of the kobolds (map 22, return city of maps 22–23, Docs/Mapas100.md §3.3). Same id
        /// as <c>MapCatalog.MercadoDosTuneis</c> and <c>CityTownRecipes.TunnelMarketCityId</c>.
        /// </summary>
        public const string MercadoDosTuneis = "mercado_dos_tuneis";

        /// <summary>
        /// Aratta, the Cidade dos Chifres, race city of the povo-touro (map 26, return city of maps 26–27, Docs/Mapas100.md §3.3).
        /// Same id as <c>MapCatalog.Aratta</c> and <c>CityTownRecipes.ArattaCityId</c>.
        /// </summary>
        public const string Aratta = "aratta";

        /// <summary>
        /// Kirta, the Cidade-Recife, race city of the povo-do-mar (map 28, return city of maps 28–29, Docs/Mapas100.md §3.3). Same
        /// id as <c>MapCatalog.Kirta</c> and <c>CityTownRecipes.KirtaCityId</c>.
        /// </summary>
        public const string Kirta = "kirta";

        /// <summary>
        /// Jhilpur, a Cidade nas Raízes, capital of the Delta de Jhilpur (map 31, return city of maps 31, 32 and 35,
        /// Docs/Mapas100.md §2.1 and região 6). Same id as <c>MapCatalog.Jhilpur</c> and <c>CityTownRecipes.JhilpurCityId</c>.
        /// </summary>
        public const string Jhilpur = "jhilpur";

        /// <summary>
        /// Palafitas de Tarchon, the race city of the trols (map 33, return city of maps 33–34, Docs/Mapas100.md §3.3). Same id
        /// as <c>MapCatalog.PalafitasTarchon</c> and <c>CityTownRecipes.TarchonCityId</c>.
        /// </summary>
        public const string PalafitasTarchon = "palafitas_tarchon";

        /// <summary>
        /// Sangkent, a Cidade-Escadaria, capital of the Teto do Mundo (map 36, return city of maps 36–40, Docs/Mapas100.md §2.1
        /// and região 7). Same id as <c>MapCatalog.Sangkent</c> and <c>CityTownRecipes.SangkentCityId</c>.
        /// </summary>
        public const string Sangkent = "sangkent";

        /// <summary>
        /// Mingcheng, a Cidade dos Nove Anéis, capital of the Planície de Mingcheng (map 41, return city of maps 41, 43, 44 and
        /// 46, Docs/Mapas100.md §2.1 and região 8). Same id as <c>MapCatalog.Mingcheng</c> and <c>CityTownRecipes.MingchengCityId</c>.
        /// </summary>
        public const string Mingcheng = "mingcheng";

        /// <summary>
        /// The Torres Brancas de Amanirenas, race city of the high elves (map 42, return city of maps 42 and 45,
        /// Docs/Mapas100.md §3.3 and região 8). Same id as <c>MapCatalog.TorresBrancas</c> and <c>CityTownRecipes.TorresBrancasCityId</c>.
        /// </summary>
        public const string TorresBrancas = "torres_brancas";

        /// <summary>
        /// The Acampamento de Soslan, race city of the centaurs (map 47, return city of maps 47, 48 and 51, Docs/Mapas100.md §3.3
        /// and região 9). Same id as <c>MapCatalog.AcampamentoSoslan</c> and <c>CityTownRecipes.SoslanCityId</c>.
        /// </summary>
        public const string AcampamentoSoslan = "acampamento_soslan";

        /// <summary>
        /// The Fortim dos Gigantes-da-Colina, race city of the hill giants (map 49, return city of maps 49 and 52,
        /// Docs/Mapas100.md §3.3 and região 9). Same id as <c>MapCatalog.Fortim</c> and <c>CityTownRecipes.FortimCityId</c>.
        /// </summary>
        public const string Fortim = "fortim";

        /// <summary>
        /// The Arvoredo de Haripushpa, race city of the trunk folk (map 50, its own return city, Docs/Mapas100.md §3.3 and
        /// região 9). Same id as <c>MapCatalog.Arvoredo</c> and <c>CityTownRecipes.ArvoredoCityId</c>.
        /// </summary>
        public const string Arvoredo = "arvoredo";

        /// <summary>
        /// The Vale dos Sátiros, race city of the satyrs (map 53, its own return city, Docs/Mapas100.md §3.3 and região 10).
        /// Same id as <c>MapCatalog.ValeDosSatiros</c> and <c>CityTownRecipes.ValeCityId</c>.
        /// </summary>
        public const string ValeDosSatiros = "vale_dos_satiros";

        /// <summary>
        /// Farnkesh, os Ninhos de Pedra, the capital of the Picos de Farnkesh (map 54, return city of maps 54–58, Docs/Mapas100.md
        /// §2 and região 10). Same id as <c>MapCatalog.Farnkesh</c> and <c>CityTownRecipes.FarnkeshCityId</c>.
        /// </summary>
        public const string Farnkesh = "farnkesh";

        /// <summary>
        /// Porto da Lua Branca, the capital of the Mar Gelado (map 59, return city of maps 59–64, Docs/Mapas100.md §2 and região
        /// 11; the Ar Sereno, Lavine's element). Same id as <c>MapCatalog.PortoDaLuaBranca</c> and
        /// <c>CityTownRecipes.PortoDaLuaBrancaCityId</c>.
        /// </summary>
        public const string PortoDaLuaBranca = "porto_da_lua_branca";

        public static readonly IReadOnlyList<CitySpec> Cities = new List<CitySpec>
        {
            new CitySpec(VilaZefa, BiomeType.Forest,
                new List<CityResidentSpec>
                {
                    new CityResidentSpec("cida", CityShopCatalog.ArmarinhoCida, "dialogue.forest_rezadeira.name"),
                    new CityResidentSpec("lazaro", CityShopCatalog.MerceariaLazaro),
                    new CityResidentSpec("nenem", CityShopCatalog.FeiraLivre),
                    // Srosh of the Haras (Docs/Mounts.md §3), Vô Jorge's old road friend: keeps the mount whistles.
                    // Not Patig, the Tracker (Hunting): two different people. Was "ze_haras" before 2026-10-30; resident ids
                    // are not saved anywhere (only localization keys), so the rename needs no migration.
                    new CityResidentSpec("tonico_haras", CityShopCatalog.HarasVila),
                    // The lore cast of the Vila without a counter (TODO.md "História e Arcos dos NPCs"): Vó Edna on her
                    // aivan, Ramrat at the lamp, the two musicians of the Bema and Patig the tracker on the plaza.
                    new CityResidentSpec("vo_edna"),
                    new CityResidentSpec("socorro_vila"),
                    new CityResidentSpec("xroshtag"),
                    new CityResidentSpec("padwaxtag"),
                    new CityResidentSpec("patig"),
                    // The new Vila shops (Docs/Cities.md §4): blacksmith, carpenter and seamstress.
                    new CityResidentSpec("severino", CityShopCatalog.FerrariaSeverino),
                    new CityResidentSpec("damiao", CityShopCatalog.MarcenariaDamiao),
                    new CityResidentSpec("fatima", CityShopCatalog.CostureiraFatima),
                },
                new List<string> { CityShopCatalog.GeneralStore, CityShopCatalog.MerceariaLazaro, CityShopCatalog.ArmarinhoCida, CityShopCatalog.FeiraLivre, CityShopCatalog.HarasVila,
                    CityShopCatalog.FerrariaSeverino, CityShopCatalog.MarcenariaDamiao, CityShopCatalog.CostureiraFatima },
                hasGrandmaServices: true, hasStoryVillagers: true),

            new CitySpec(AreiaBranca, BiomeType.Desert,
                new List<CityResidentSpec>
                {
                    new CityResidentSpec("vava", CityShopCatalog.SelariaVava),
                    new CityResidentSpec("socorro"),
                    new CityResidentSpec("tiao"),
                    // Náfsha the potter, in the olaria on the West street (old id "rosa", like shop_olaria_rosa).
                    new CityResidentSpec("rosa"),
                },
                new List<string> { CityShopCatalog.SelariaVava }),

            new CitySpec(SerraAlta, BiomeType.Mountain,
                new List<CityResidentSpec>
                {
                    new CityResidentSpec("tonho", CityShopCatalog.ForjaTonho, "dialogue.mountain_miner.name"),
                    new CityResidentSpec("graca"),
                    new CityResidentSpec("quinzinho"),
                },
                new List<string> { CityShopCatalog.ForjaTonho }),

            new CitySpec(Lamparinas, BiomeType.Caves,
                new List<CityResidentSpec>
                {
                    new CityResidentSpec("benedita", CityShopCatalog.LamparinasBenedita, "dialogue.caves_lampkeeper.name"),
                    new CityResidentSpec("candinho"),
                    new CityResidentSpec("fia"),
                    new CityResidentSpec("zezinho"),
                },
                new List<string> { CityShopCatalog.LamparinasBenedita }),
        };

        /// <summary>
        /// The race cities (Docs/Mapas100.md §3.3) that already have a walkable town: a second city inside a region, so they
        /// stay out of <see cref="Cities"/> (one hub city per region: the world-map hub, the region respawn, the Barcas list).
        /// <see cref="Get"/>, <see cref="AllCities"/> and <see cref="AllResidents"/> see them; a defeat on one of their maps
        /// wakes the hero in their plaza (<c>MapCatalog.RespawnCityFor</c>).
        /// </summary>
        public static readonly IReadOnlyList<CitySpec> RaceCities = new List<CitySpec>
        {
            // Copas de Garajonay (elfos-da-mata): Gara the canopy warden (missions), Jonay the archer, Dácil the herbalist
            // (the city's shop, with the rope of the canopy and the Garden Key), Bencomo the old chief, Tinguaro the tracker.
            new CitySpec(CopasGarajonay, BiomeType.Forest,
                new List<CityResidentSpec>
                {
                    new CityResidentSpec("gara"),
                    new CityResidentSpec("jonay"),
                    new CityResidentSpec("dacil", CityShopCatalog.ErvariaDacil),
                    new CityResidentSpec("bencomo"),
                    new CityResidentSpec("tinguaro"),
                },
                new List<string> { CityShopCatalog.ErvariaDacil }),

            // Tambor Rubro (orcs, names from the Hurrian tales and Hittite/Mitanni history): Gurparanzah the archer chief
            // (missions), Kessi the hunter (second mission), Appu the gentle merchant who adopts the camp's orphans,
            // Ashmunikal the matriarch of the drums (the guardian lamp on the drum tower), Tushratta the smith (the city's
            // shop: the Forja de Tambor, with the theriac of map 12).
            new CitySpec(TamborRubro, BiomeType.Desert,
                new List<CityResidentSpec>
                {
                    new CityResidentSpec("gurparanzah"),
                    new CityResidentSpec("kessi"),
                    new CityResidentSpec("appu"),
                    new CityResidentSpec("ashmunikal"),
                    new CityResidentSpec("tushratta", CityShopCatalog.ForjaTambor),
                },
                new List<string> { CityShopCatalog.ForjaTambor }),

            // Salões de Bronze de Napirasu (dwarves, Elamite names): Napirasu the master founder (her hall at the plaza head),
            // Untash the master builder (missions), Puzur the rune keeper (the city's shop: the Portões de Runa, with the
            // pickaxe and torch of map 17 and the ginger tea of map 18), Shilhak the miner (second mission), Kidin the cook.
            new CitySpec(SaloesDeBronze, BiomeType.Mountain,
                new List<CityResidentSpec>
                {
                    new CityResidentSpec("napirasu"),
                    new CityResidentSpec("untash"),
                    new CityResidentSpec("puzur", CityShopCatalog.PortoesRuna),
                    new CityResidentSpec("shilhak"),
                    new CityResidentSpec("kidin"),
                },
                new List<string> { CityShopCatalog.PortoesRuna }),

            // Mercado dos Túneis (kobolds, names from the Sogdian Ancient Letters): Miwnay who runs the market (the city's shop:
            // the Feira da Miwnay, with the eternal-fire oil of map 23), her daughter Shayn who mends and recycles, Farnkhund the
            // cart driver of the tunnels (second mission), Takhsich the inventor (missions; Ícaro's favourite).
            new CitySpec(MercadoDosTuneis, BiomeType.Caves,
                new List<CityResidentSpec>
                {
                    new CityResidentSpec("miwnay", CityShopCatalog.FeiraMiwnay),
                    new CityResidentSpec("shayn"),
                    new CityResidentSpec("farnkhund"),
                    new CityResidentSpec("takhsich"),
                },
                new List<string> { CityShopCatalog.FeiraMiwnay }),

            // Aratta (povo-touro, Sumerian names from the epics of Enmerkar and Lugalbanda and the Hymn of Shulgi): Enmerkar the
            // chief builder (missions), Lugalbanda the wanderer (the city's shop: the Alforje do Lugalbanda, with the Artífices'
            // relic of map 27), Shulgi the runner (second mission), Ensuhkeshdanna the rival who became a friend. The Costa das
            // Monções has no world-map biome: the region is the borrowed Forest (always unlocked), as for Taa Pwani.
            new CitySpec(Aratta, BiomeType.Forest,
                new List<CityResidentSpec>
                {
                    new CityResidentSpec("enmerkar"),
                    new CityResidentSpec("lugalbanda", CityShopCatalog.AlforjeLugalbanda),
                    new CityResidentSpec("shulgi"),
                    new CityResidentSpec("ensuhkeshdanna"),
                },
                new List<string> { CityShopCatalog.AlforjeLugalbanda }),

            // Kirta (povo-do-mar, Ugaritic names from the epics of Kirta and Aqhat): Kirta the king (missions), Pughat who looks
            // after everyone (second mission, rescues), Danel the just judge (the city's shop: the diving tools, with the breathing
            // shell of map 28 and the air bubble of map 29), Aqhat the archer (trainer). Region: the borrowed Forest.
            new CitySpec(Kirta, BiomeType.Forest,
                new List<CityResidentSpec>
                {
                    new CityResidentSpec("kirta"),
                    new CityResidentSpec("pughat"),
                    new CityResidentSpec("danel", CityShopCatalog.MergulhoDanel),
                    new CityResidentSpec("aqhat"),
                },
                new List<string> { CityShopCatalog.MergulhoDanel }),

            // Palafitas de Tarchon (trols, Etruscan names from Tarquinia and Cerveteri): Tarchon the founder (missions), Ramtha
            // the healer (the guardian lamp), Larthia the basket weaver (the city's shop: the Cestaria da Larthia, with the reed
            // boat of map 33 and the antivenom amulet of map 34), Vel the fisher (second mission), Thanchvil the storyteller.
            // The Delta de Jhilpur has no world-map biome: the region is the borrowed Forest, as for Jhilpur.
            new CitySpec(PalafitasTarchon, BiomeType.Forest,
                new List<CityResidentSpec>
                {
                    new CityResidentSpec("tarchon"),
                    new CityResidentSpec("ramtha"),
                    new CityResidentSpec("larthia", CityShopCatalog.CestariaLarthia),
                    new CityResidentSpec("vel"),
                    new CityResidentSpec("thanchvil"),
                },
                new List<string> { CityShopCatalog.CestariaLarthia }),

            // Torres Brancas de Amanirenas (altos-elfos, names of the candaces and kings of Kush/Meroë): Amanirenas the
            // queen-guardian (missions), Amanitore the builder, Natakamani the master of the mirrors (the city's shop: the
            // Espelhos de Natakamani, refining with reflected light), Shanakdakhete who teaches the light (second mission),
            // Arkamani who reads the stars. The Planície de Mingcheng has no world-map biome: the region is the borrowed Forest.
            new CitySpec(TorresBrancas, BiomeType.Forest,
                new List<CityResidentSpec>
                {
                    new CityResidentSpec("amanirenas"),
                    new CityResidentSpec("amanitore"),
                    new CityResidentSpec("natakamani", CityShopCatalog.EspelhosNatakamani),
                    new CityResidentSpec("shanakdakhete"),
                    new CityResidentSpec("arkamani"),
                },
                new List<string> { CityShopCatalog.EspelhosNatakamani }),

            // Acampamento de Soslan (centaurs, the Estepe dos Nartes, região 9; names of the Nart sagas of Ossetia): Soslan, the
            // steel-skinned hero (missions), Batradz the smith-warrior, Satana the wise woman of the house (second mission:
            // the guardian lamp), Syrdon the trickster (the city's shop: the Selaria Nômade, saddles and blankets with a good
            // prank) and old Uryzmæg, the chief. The Estepe dos Nartes has no world-map biome: the region is the borrowed Forest.
            new CitySpec(AcampamentoSoslan, BiomeType.Forest,
                new List<CityResidentSpec>
                {
                    new CityResidentSpec("soslan"),
                    new CityResidentSpec("batradz"),
                    new CityResidentSpec("satana"),
                    new CityResidentSpec("syrdon", CityShopCatalog.SelariaNomade),
                    new CityResidentSpec("uryzmaeg"),
                },
                new List<string> { CityShopCatalog.SelariaNomade }),

            // Fortim dos Gigantes-da-Colina (the penitent giants of the Manichaean Book of Giants): Baraqel, the chief (missions),
            // Virogdad the dreamer (second mission: the crows of a bad dream; pays with the crampons of map 52) and old
            // Shahmizad, the patriarch, who cooks for everyone at the Panela Grande (the city's shop). Region: the borrowed Forest.
            new CitySpec(Fortim, BiomeType.Forest,
                new List<CityResidentSpec>
                {
                    new CityResidentSpec("baraqel"),
                    new CityResidentSpec("virogdad"),
                    new CityResidentSpec("shahmizad", CityShopCatalog.PanelaGrande),
                },
                new List<string> { CityShopCatalog.PanelaGrande }),

            // Arvoredo de Haripushpa (the trunk folk; Haripuṣpa and Suvarṇapuṣpa, kings of Kucha): Haripushpa, the eldest
            // (missions), Suvarnapushpa the gardener (the city's shop: the Viveiro) and Aryaman Puhr, a Sogdian herbalist
            // passing through (second mission; Mār Aryāmān Puhr, the Manichaean teacher of the Bezeklik letters). Region: the
            // borrowed Forest.
            new CitySpec(Arvoredo, BiomeType.Forest,
                new List<CityResidentSpec>
                {
                    new CityResidentSpec("haripushpa"),
                    new CityResidentSpec("suvarnapushpa", CityShopCatalog.ViveiroSuvarnapushpa),
                    new CityResidentSpec("aryaman"),
                },
                new List<string> { CityShopCatalog.ViveiroSuvarnapushpa }),

            // Vale dos Sátiros (the satyrs, the Picos de Farnkesh, região 10; the Phrygian myths of the ficha): King Midas, touched by
            // Āz, whose hands turn everything to gold (missions: the golden bees of the river), Marsyas, the master of the double
            // flute (the city's shop: the Flauta Dupla) and Gordias, the farmer of the knots (second mission). Region: the borrowed
            // Forest (the Picos de Farnkesh have no world-map biome).
            new CitySpec(ValeDosSatiros, BiomeType.Forest,
                new List<CityResidentSpec>
                {
                    new CityResidentSpec("midas"),
                    new CityResidentSpec("marsyas", CityShopCatalog.FlautaDupla),
                    new CityResidentSpec("gordias"),
                },
                new List<string> { CityShopCatalog.FlautaDupla }),
        };

        /// <summary>
        /// The cities of the regions past the four biomes of the world map (Docs/Mapas100.md regiões 5–8) that already have a
        /// walkable town. Their region has no world-map biome yet, so their <see cref="CitySpec.Region"/> is a borrowed one (the
        /// Forest: always unlocked, so the respawn of <see cref="CityProgress.ApplyDefeatIn"/> always lands in their plaza);
        /// like the race cities they stay out of <see cref="Cities"/> (no world-map hub, no peddler, no Barcas hub list)
        /// and are reached through their maps. Their shops sell the gear of their region's map gates
        /// (Docs/MetodologiaMapasERacas.md §2.1).
        /// </summary>
        public static readonly IReadOnlyList<CitySpec> LateRegionCities = new List<CitySpec>
        {
            // Taa Pwani (Swahili sailors and merchants): Captain Liyongo the nakhoda (missions), Mwana the pearl merchant (the
            // city's shop: dhow passages and the gear of maps 25–30), Nuru the keeper of the Farol de Coral, Zawadi who braids
            // root boots (second mission), old Mbaraka who reads the Moon Ship's stars.
            new CitySpec(TaaPwani, BiomeType.Forest,
                new List<CityResidentSpec>
                {
                    new CityResidentSpec("liyongo"),
                    new CityResidentSpec("mwana", CityShopCatalog.BazarMwana),
                    new CityResidentSpec("nuru"),
                    new CityResidentSpec("zawadi"),
                    new CityResidentSpec("mbaraka"),
                },
                new List<string> { CityShopCatalog.BazarMwana }),

            // Jhilpur (the Delta de Jhilpur, região 6; names of Manichaean missionaries and of the Kellis letters): old Salmaios,
            // the ascetic of the Cologne Codex, who tends the Raiz-Mãe (missions), Nuhzadag, Mani's interpreter, here the
            // boatman who speaks every tongue of the delta (the city's shop: the gear of the gates 32–35; Tarchon's own
            // Cestaria sells 33–34 too), Abiesous, the master of the Codex, who teaches at the boat school (second mission), and Tehat, a woman of
            // the Kellis letters, the herbalist of the herb quarter. Region: the borrowed Forest (as Taa Pwani).
            new CitySpec(Jhilpur, BiomeType.Forest,
                new List<CityResidentSpec>
                {
                    new CityResidentSpec("salmaios"),
                    new CityResidentSpec("nuhzadag", CityShopCatalog.TrocasNuhzadag),
                    new CityResidentSpec("abiesous"),
                    new CityResidentSpec("tehat"),
                },
                new List<string> { CityShopCatalog.TrocasNuhzadag }),

            // Sangkent (the Teto do Mundo, região 7; names of Manichaean missionaries and scribes): Abā, the missionary, here the
            // chief of the chutes (missions), Kustai, the scribe of the Cologne Codex, who keeps the caravan notice board (second
            // mission), Baraies, the master of the Codex, the teacher who counts the maps, and Sisinnios, Mani's successor, here
            // the outfitter of the caravans (the city's shop: the gear of the gates 36, 38 and 40). Region: the borrowed Forest
            // (as Jhilpur).
            new CitySpec(Sangkent, BiomeType.Forest,
                new List<CityResidentSpec>
                {
                    new CityResidentSpec("aba"),
                    new CityResidentSpec("sisinnios", CityShopCatalog.AgasalhosSisinnios),
                    new CityResidentSpec("kustai"),
                    new CityResidentSpec("baraies"),
                },
                new List<string> { CityShopCatalog.AgasalhosSisinnios }),

            // Mingcheng (the Planície de Mingcheng, região 8; names of the Mingjiao, the Manichaeism of China and of the Uyghurs):
            // Bögü, the Uyghur khagan, here the old chief of the ring guard (missions), Mōzak, the title of the Manichaean
            // teachers, here the keeper of the lanterns (the city's shop: the Sun Seal of map 42), Lin Deng of Fujian, the master
            // of the canal (second mission), and Fuduodan, the bishop of 694, who reads the Two Principles. Region: the borrowed
            // Forest (as Jhilpur and Sangkent).
            new CitySpec(Mingcheng, BiomeType.Forest,
                new List<CityResidentSpec>
                {
                    new CityResidentSpec("bogu"),
                    new CityResidentSpec("mozak", CityShopCatalog.LanternasMozak),
                    new CityResidentSpec("lin_deng"),
                    new CityResidentSpec("fuduodan"),
                },
                new List<string> { CityShopCatalog.LanternasMozak }),

            // Farnkesh, os Ninhos de Pedra (the Picos de Farnkesh, região 10; names of the Manichaean mission to the east):
            // Shad-Ohrmizd, who founded an eastern branch of the church, here the guardian of the wind bells (missions), Sisin
            // (Sisinnios, Mani's successor), here the master of the rope bridges who pulls the baskets (the city's shop: the gear
            // of the gates 57 and 58), Tadi, the sister of Nafsha in the mission story of Palmyra, here the folder of wind paper
            // (second mission), and Ardawan, the Parthian prince of Mar Ammo's mission, here the braider of yak rope. Region: the
            // borrowed Forest (as Mingcheng).
            new CitySpec(Farnkesh, BiomeType.Forest,
                new List<CityResidentSpec>
                {
                    new CityResidentSpec("shad_ohrmizd"),
                    new CityResidentSpec("sisin", CityShopCatalog.CordasSisin),
                    new CityResidentSpec("tadi"),
                    new CityResidentSpec("ardawan"),
                },
                new List<string> { CityShopCatalog.CordasSisin }),

            // Porto da Lua Branca (the Mar Gelado, região 11; the Komi tales of the ficha and the Manichaean letters of Kellis):
            // Yirkap, the ski hunter of the Komi tales, here the hunter who keeps the harbour safe (missions), Pamour, of the
            // Manichaean family of the Kellis letters, here the furrier and boat-hirer of the quays (the city's shop: the gear of
            // the gates of maps 59–64), Pera, the good giant of the Komi tales, who breaks the ice for the boats (second mission),
            // and Makarios, the father who writes letters in the Kellis papyri, here the keeper of the moon lanterns of the Cais
            // da Lua. Region: the borrowed Forest (as Farnkesh).
            new CitySpec(PortoDaLuaBranca, BiomeType.Forest,
                new List<CityResidentSpec>
                {
                    new CityResidentSpec("yirkap"),
                    new CityResidentSpec("pamour", CityShopCatalog.PelesPamour),
                    new CityResidentSpec("pera"),
                    new CityResidentSpec("makarios"),
                },
                new List<string> { CityShopCatalog.PelesPamour }),
        };

        /// <summary>Ammo, the peddler: not a resident of one city, he is wherever his mule is today (<see cref="ShopRotation.MascateRegion"/>).</summary>
        public static readonly CityResidentSpec Mascate = new CityResidentSpec("bento", CityShopCatalog.MascateBento, "dialogue.desert_tropeiro.name");

        /// <summary>Any city with a walkable town: the region hubs, then the race cities, then the late-region cities.</summary>
        public static IEnumerable<CitySpec> AllCities => Cities.Concat(RaceCities).Concat(LateRegionCities);

        public static CitySpec Get(string cityId) => AllCities.FirstOrDefault(c => c.CityId == cityId);

        /// <summary>Whether <paramref name="city"/> is a race city (not its region's hub: no world-map hub, no peddler).</summary>
        public static bool IsRaceCity(CitySpec city) => city != null && RaceCities.Contains(city);

        /// <summary>Whether <paramref name="city"/> is a late-region city (Taa Pwani...): reached through its maps, not the world map.</summary>
        public static bool IsLateRegionCity(CitySpec city) => city != null && LateRegionCities.Contains(city);

        /// <summary>Whether <paramref name="city"/> is its region's hub (<see cref="Cities"/>): the only kind the peddler visits.</summary>
        public static bool IsHubCity(CitySpec city) => city != null && Cities.Contains(city);

        public static CitySpec ForRegion(BiomeType? region)
        {
            return region.HasValue ? Cities.FirstOrDefault(c => c.Region == region.Value) : null;
        }

        public static bool HasCity(BiomeType region) => ForRegion(region) != null;

        /// <summary>Every resident, the roaming peddler included.</summary>
        public static IEnumerable<CityResidentSpec> AllResidents()
        {
            foreach (CitySpec city in AllCities)
            {
                foreach (CityResidentSpec resident in city.Residents)
                {
                    yield return resident;
                }
            }

            yield return Mascate;
        }
    }
}
