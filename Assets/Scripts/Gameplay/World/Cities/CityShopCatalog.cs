using System.Collections.Generic;
using System.Linq;
using Rapadura.Gameplay.Housing;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.World.Biomes;

namespace Rapadura.Gameplay.World.Cities
{
    public enum ShopTheme
    {
        GeneralStore,
        Armarinho,
        Ferreiro,
        Feira,
        Mercearia,
        /// <summary>A city's regional speciality (selaria do Deserto, lamparinas das Cavernas...).</summary>
        Speciality,
        /// <summary>The roaming peddler.</summary>
        Mascate,
        /// <summary>Wooden house furniture (Marcenaria do Mestre Omóforo).</summary>
        Marcenaria,
        /// <summary>Clothes and cloth for the house (Costureira Mestra Narisah).</summary>
        Costureira,
    }

    public enum ShopRotationPeriod
    {
        Daily = 0,
        Weekly = 1,
    }

    public enum ShopUnlockKind
    {
        None = 0,
        /// <summary>Sold once the region is open on the map.</summary>
        RegionUnlocked = 1,
        /// <summary>Sold once the region's boss was beaten ("Enfrentar o chefe").</summary>
        RegionCleared = 2,
        /// <summary>Sold once the cousins reached <see cref="ShopUnlock.Level"/> in this shop's freguesia (<see cref="ShopReputation"/>).</summary>
        Reputation = 3,
    }

    /// <summary>What a shop line sells: a bag item, a house item (goes to the house depot) or an outfit (wardrobe).</summary>
    public enum CityShopEntryKind
    {
        /// <summary>An <c>ItemDefinition</c> of <c>Resources/Items</c>.</summary>
        Item = 0,
        /// <summary>A <c>HouseCatalog</c> item: bought into the house depot (only once the cousins have the house).</summary>
        HouseItem = 1,
        /// <summary>A <c>CatalogoRoupas.md</c> look: on the counter only once the wardrobe data exists (<c>CityShopService.OutfitResolver</c>).</summary>
        Outfit = 2,
    }

    /// <summary>Which draw a rotating line belongs to (fixed lines ignore it).</summary>
    public enum ShopPool
    {
        /// <summary>The shop's own period (daily or weekly), <see cref="CityShopSpec.RotatingSlots"/>.</summary>
        Main = 0,
        /// <summary>House items, drawn on the shop's period with their own seed, <see cref="CityShopSpec.HouseSlots"/>.</summary>
        House = 1,
        /// <summary>The week's special, always drawn per Monday-to-Sunday week, <see cref="CityShopSpec.WeeklySlots"/>.</summary>
        Weekly = 2,
    }

    /// <summary>Gate of a shop entry: campaign progress (region open / boss beaten) or the shop's freguesia.</summary>
    public readonly struct ShopUnlock
    {
        public readonly ShopUnlockKind Kind;
        public readonly BiomeType Region;

        /// <summary>Reputation tier needed (<see cref="ShopUnlockKind.Reputation"/>).</summary>
        public readonly int Level;

        public ShopUnlock(ShopUnlockKind kind, BiomeType region, int level = 0)
        {
            Kind = kind;
            Region = region;
            Level = level;
        }

        public static ShopUnlock None => default;
        public static ShopUnlock Unlocked(BiomeType region) => new ShopUnlock(ShopUnlockKind.RegionUnlocked, region);
        public static ShopUnlock Cleared(BiomeType region) => new ShopUnlock(ShopUnlockKind.RegionCleared, region);
        public static ShopUnlock Reputation(int tier) => new ShopUnlock(ShopUnlockKind.Reputation, default, tier);

        public bool IsGated => Kind != ShopUnlockKind.None;
    }

    /// <summary>One line of a themed shop: item, price in <c>item_gold</c>, daily stock (-1 = endless), whether it rotates and its gate.</summary>
    public sealed class CityShopEntry
    {
        public readonly string ItemId;
        public readonly int Price;
        public readonly int Stock;

        /// <summary>False = always on the counter (when unlocked); true = drawn into the rotation's slots.</summary>
        public readonly bool Rotating;
        public readonly ShopUnlock Unlock;
        public readonly CityShopEntryKind Kind;

        /// <summary>Draw of a rotating line (house items default to <see cref="ShopPool.House"/>).</summary>
        public readonly ShopPool Pool;

        public CityShopEntry(string itemId, int price, int stock, bool rotating = false, ShopUnlock unlock = default,
            CityShopEntryKind kind = CityShopEntryKind.Item, ShopPool? pool = null)
        {
            ItemId = itemId;
            Price = price;
            Stock = stock;
            Rotating = rotating;
            Unlock = unlock;
            Kind = kind;
            Pool = pool ?? (kind == CityShopEntryKind.HouseItem ? ShopPool.House : ShopPool.Main);
        }

        public bool IsBagItem => Kind == CityShopEntryKind.Item;
    }

    /// <summary>A themed shop with its keeper (<see cref="CityResidentSpec.ShopId"/>), home region (null = roaming) and stock.</summary>
    public sealed class CityShopSpec
    {
        public readonly string ShopId;
        public readonly ShopTheme Theme;
        public readonly BiomeType? HomeRegion;
        public readonly ShopRotationPeriod Period;

        /// <summary>How many rotating <see cref="ShopPool.Main"/> entries are on the counter each period (fewer if fewer are open).</summary>
        public readonly int RotatingSlots;

        /// <summary>How many rotating house items (<see cref="ShopPool.House"/>) are on the counter each period.</summary>
        public readonly int HouseSlots;

        /// <summary>How many week specials (<see cref="ShopPool.Weekly"/>) are on the counter each week.</summary>
        public readonly int WeeklySlots;
        public readonly IReadOnlyList<CityShopEntry> Entries;
        public readonly float SellBackRate;

        public CityShopSpec(string shopId, ShopTheme theme, BiomeType? homeRegion, ShopRotationPeriod period, int rotatingSlots,
            IReadOnlyList<CityShopEntry> entries, float sellBackRate = 0.5f, int houseSlots = 0, int weeklySlots = 0)
        {
            ShopId = shopId;
            Theme = theme;
            HomeRegion = homeRegion;
            Period = period;
            RotatingSlots = rotatingSlots;
            HouseSlots = houseSlots;
            WeeklySlots = weeklySlots;
            Entries = entries;
            SellBackRate = sellBackRate;
        }

        public bool IsRoaming => !HomeRegion.HasValue;
        public string NameKey => "city.shop." + ShopId + ".name";

        /// <summary>Slots of <paramref name="pool"/>.</summary>
        public int SlotsOf(ShopPool pool) => pool == ShopPool.House ? HouseSlots : pool == ShopPool.Weekly ? WeeklySlots : RotatingSlots;
    }

    /// <summary>
    /// The themed shops of the cities (FASE 15 "Lojas"). Only existing items (<c>Resources/Items</c>); prices stay within
    /// 80–125% of each item's <c>ItemDefinition.Value</c> (Docs/Balance/README.md, "Cidades"): the speciality of a city sells
    /// its theme ~10% cheaper, the peddler ~20% dearer. Names follow Docs/Housing/CatalogoCasa.md (Armarinho da Dona
    /// Cida, Mercearia do Zaku, Bazar da Nana, Forja do Mestre Mihr, Lamparinas da Parthenos, Mascate Ammo).
    /// <see cref="GeneralStore"/> is the old <c>Resources/Shops/shop_general_store</c> asset, kept as is.
    /// </summary>
    public static class CityShopCatalog
    {
        public const string GeneralStore = "shop_general_store";
        public const string MerceariaLazaro = "shop_mercearia_lazaro";
        public const string ArmarinhoCida = "shop_armarinho_cida";
        public const string FeiraLivre = "shop_feira_livre";
        public const string SelariaVava = "shop_selaria_vava";
        public const string ForjaTonho = "shop_forja_tonho";
        public const string LamparinasBenedita = "shop_lamparinas_benedita";
        public const string MascateBento = "shop_mascate_bento";

        /// <summary>"Estrebaria do Srosh" (Srosh, Docs/Mounts.md §3): sells the mount whistles; buying one redeems it (<c>MountCollection</c>).</summary>
        public const string HarasVila = "shop_haras_vila";

        /// <summary>Ferraria do Mestre Adamas: the Vila's own blacksmith (tools, iron, the first refining stones).</summary>
        public const string FerrariaSeverino = "shop_ferraria_severino";

        /// <summary>Marcenaria do Mestre Omóforo (Docs/Housing.md §8.1): the wooden furniture of CatalogoCasa.md.</summary>
        public const string MarcenariaDamiao = "shop_marcenaria_damiao";

        /// <summary>Costureira Mestra Narisah (Docs/Housing.md §8.1): cloth for the house and the looks of CatalogoRoupas.md.</summary>
        public const string CostureiraFatima = "shop_costureira_fatima";

        /// <summary>
        /// Ervaria da Dácil (Copas de Garajonay, the elfos-da-mata, Docs/Mapas100.md §3.3): herbs, mist potions and the
        /// canopy gear. Sells the Corda de Escalada (gate of map 05) and, after the Forest boss, a copy of the Chave do
        /// Jardim (gate of map 06), the same as Mariam's Armarinho (Docs/MetodologiaMapasERacas.md §2.1).
        /// </summary>
        public const string ErvariaDacil = "shop_ervaria_dacil";

        /// <summary>
        /// Bazar de Pérolas da Mwana (Taa Pwani, the Costa das Monções, Docs/Mapas100.md região 5): pearls, sea gear and the
        /// gear of the gates whose way starts at Taa Pwani (Docs/MetodologiaMapasERacas.md §2.1): the Passagem de Dhow (25), the
        /// Concha Respirável (28: Kirta stands on map 28, so the shell has to be bought before reaching it; Danel sells it too)
        /// and the Botas de Raiz Viva (30). The Relíquia dos Artífices (27) is Lugalbanda's (Aratta) and the Bolha de Ar (29)
        /// Danel's (Kirta) since those cities have a town.
        /// </summary>
        public const string BazarMwana = "shop_bazar_mwana";

        /// <summary>
        /// Forja de Tambor (Tambor Rubro, the orcs' race city, Docs/Mapas100.md §3.3): Tushratta's copper-and-bronze forge:
        /// iron, tools and blades, the Pedras da Luz Presa, and the Teriaga do Oásis for the gate of map 12 (the Trono de
        /// Namrael, whose return city is the Tambor Rubro). The counter turns weekly.
        /// </summary>
        public const string ForjaTambor = "shop_forja_tambor";

        /// <summary>
        /// Portões de Runa (Salões de Bronze de Napirasu, the dwarves' race city, Docs/Mapas100.md §3.3): Puzur's rune counter:
        /// bronze runes, the Pedras da Luz Presa, and the gear of the region's gates: the pickaxe and the oil lamp (torch) of
        /// map 17 and the ginger tea of map 18 (the Cume dos Mazans, whose return city is the Salões). The counter turns weekly.
        /// </summary>
        public const string PortoesRuna = "shop_portoes_runa";

        /// <summary>
        /// Feira da Miwnay (Mercado dos Túneis, the kobolds' race city, Docs/Mapas100.md §3.3): the market that moves from gallery
        /// to gallery: mended scrap, lamps and oil, potions, and the Óleo do Fogo Eterno for the gate of map 23 (the Cânion da
        /// Roda, whose return city is the Mercado). The counter turns daily.
        /// </summary>
        public const string FeiraMiwnay = "shop_feira_miwnay";

        /// <summary>
        /// Alforje do Lugalbanda (Aratta, the povo-touro's race city, Docs/Mapas100.md §3.3): the wanderer's saddlebag: rope,
        /// waterskins, savanna food, and the Relíquia dos Artífices for the gate of map 27 (the Pirâmides de Meroé, whose
        /// return city is Aratta). The counter turns daily.
        /// </summary>
        public const string AlforjeLugalbanda = "shop_alforje_lugalbanda";

        /// <summary>
        /// Ferramentas de Mergulho do Danel (Kirta, the povo-do-mar's race city, Docs/Mapas100.md §3.3): diving tools: the
        /// Concha Respirável (gate of map 28) and the Bolha de Ar Engarrafada (gate of map 29, Kilwa Afundada, whose return city
        /// is Kirta), sea potions and the reef's catch. The counter turns daily.
        /// </summary>
        public const string MergulhoDanel = "shop_mergulho_danel";

        /// <summary>
        /// Barcos e Trocas do Nuhzadag (Jhilpur, the capital of the Delta de Jhilpur, Docs/Mapas100.md região 6): the boatman
        /// who speaks every tongue of the delta trades the gear of every gate of the region (Docs/MetodologiaMapasERacas.md
        /// §2.1): the Vara de Equilíbrio (32), the Barco de Junco (33), the Amuleto Antiveneno (34) and the Fruta Purificadora
        /// (35). The Palafitas de Tarchon (return city of 33–34) weave the boats and the amulets; his boats bring them down too.
        /// The counter turns daily.
        /// </summary>
        public const string TrocasNuhzadag = "shop_trocas_nuhzadag";

        /// <summary>
        /// Cestaria da Larthia (Palafitas de Tarchon, the trols' city, Docs/Mapas100.md §3.3): the basket weaver sells what the
        /// trols weave and catch: the Barco de Junco (33, their own map: the boats are woven here) and the Amuleto Antiveneno
        /// (34, the Charco do Ninki Nanka, whose return city this is), the Alforje (her "bags", one per save), oil lamps for the
        /// lantern basket and the catfish of the swamp. The counter turns daily.
        /// </summary>
        public const string CestariaLarthia = "shop_cestaria_larthia";

        /// <summary>
        /// Agasalhos do Sisinnios (Sangkent, the capital of the Teto do Mundo, Docs/Mapas100.md região 7): the outfitter of the
        /// caravans sells the gear of every gate of the region that an item answers (Docs/MetodologiaMapasERacas.md §2.1): the
        /// Roupa Térmica Refinada (36, worn), the Grampos de Escalada (38) and the Capa de Tempestade (40). The Forte dos Ventos
        /// (39) asks for real wings or a flying mount, which no counter sells. The counter turns daily.
        /// </summary>
        public const string AgasalhosSisinnios = "shop_agasalhos_sisinnios";

        /// <summary>
        /// Lanternas do Mestre Mōzak (Mingcheng, the capital of the Planície de Mingcheng, Docs/Mapas100.md região 8): the
        /// keeper of the lanterns sells the gear of the only gate of the region that an item answers
        /// (Docs/MetodologiaMapasERacas.md §2.1): the Selo do Sol of the Torres Brancas (42), bound to the hero. Oil and rune
        /// lamps for the night of the outer rings. The counter turns daily.
        /// </summary>
        public const string LanternasMozak = "shop_lanternas_mozak";

        /// <summary>
        /// Espelhos de Natakamani (Torres Brancas de Amanirenas, the high elves' city, Docs/Mapas100.md §3.3): the master of the
        /// mirrors sells the stones of the refining ("luz refletida": blessing, soul, life and chaos stones) and the clarity rune;
        /// no gate item (the Sun Seal of 42 is sold in Mingcheng, before the road in). The counter turns daily.
        /// </summary>
        public const string EspelhosNatakamani = "shop_espelhos_natakamani";

        /// <summary>
        /// Selaria Nômade (Acampamento de Soslan, the centaurs' camp, Docs/Mapas100.md §3.3): Syrdon the trickster sells felt
        /// saddle blankets, feed and travel food for the steppe (maps 47, 48 and 51 have no gate). The counter turns daily.
        /// </summary>
        public const string SelariaNomade = "shop_selaria_nomade";

        /// <summary>
        /// Panela Grande (Fortim dos Gigantes-da-Colina, Docs/Mapas100.md §3.3): old Shahmizad cooks for everyone and sells the
        /// gear of the only gate of the region (Docs/MetodologiaMapasERacas.md §2.1): the Grampos de Escalada of the Corrente de
        /// Amirani (52), whose return city is the Fortim. The counter turns daily.
        /// </summary>
        public const string PanelaGrande = "shop_panela_grande";

        /// <summary>
        /// Viveiro (Arvoredo de Haripushpa, Docs/Mapas100.md §3.3): Suvarnapushpa the gardener sells herbs, fibre and healing
        /// runes grown slowly in the grove (map 50 has no gate). The counter turns daily.
        /// </summary>
        public const string ViveiroSuvarnapushpa = "shop_viveiro_suvarnapushpa";

        /// <summary>
        /// Flauta Dupla (Vale dos Sátiros, Docs/Mapas100.md §3.3): Marsyas sells wind runes and songs for the road, travel food
        /// and the bottled Serene Air of the high paths (map 53 has no gate). The counter turns daily.
        /// </summary>
        public const string FlautaDupla = "shop_flauta_dupla";

        /// <summary>
        /// Cordas do Sisin (Farnkesh, Docs/Mapas100.md map 54): Sisin, the master of the rope bridges, sells the gear of the gates
        /// of the Picos de Farnkesh (Docs/MetodologiaMapasERacas.md §2.1): the Capa de Tempestade of the Garganta dos Ays (57) and
        /// the Asas de Lona that glide between the islands of the Ninho da Huma (58), both of them maps whose return city is
        /// Farnkesh; rope, balance poles and crampons for the bridges and the hanging hermitages. The counter turns daily.
        /// </summary>
        public const string CordasSisin = "shop_cordas_sisin";

        /// <summary>
        /// Peles e Barcos do Pamour (Porto da Lua Branca, Docs/Mapas100.md map 59): Pamour, the furrier and boat-hirer of the quays,
        /// sells the gear of the gates of the Mar Gelado (Docs/MetodologiaMapasERacas.md §2.1), maps 59–64 that all return to the
        /// Porto: thermal furs and ginger tea for the cold, passages and reed boats for the canals and the open sea, oil and runic
        /// lamps for the Galerias dos Mamutes, crampons for the ice, the storm cloak for the polar wind. The counter turns daily.
        /// </summary>
        public const string PelesPamour = "shop_peles_pamour";

        /// <summary>
        /// Ferro Preto do Kurogane (Hinokura, Docs/Mapas100.md map 65): Kurogane, the smith of the Ilha-Forja, sells the gear the
        /// maps of the Arquipélago de Hinokura (65–70, all returning to Hinokura) will ask for (Docs/MetodologiaMapasERacas.md
        /// §2.1): passages and reed boats for the obsidian reef and the hot channels (68), crampons and rope for the climb of
        /// Monte Echeyde (67), theriac and the antivenom amulet for the fumaroles (67, 70), the balance pole for the red bridges
        /// of the gardens (66) and water skins for the heat of the caldera (70) and to cool the lava (68). The counter turns daily.
        /// </summary>
        public const string FerroPretoKurogane = "shop_ferro_preto_kurogane";

        /// <summary>
        /// Trocas do Khunanup (Cais de Mesektet, Docs/Mapas100.md map 71): Khunanup, the eloquent merchant of the quay, sells the
        /// gear the hours of the Rio das Doze Horas (71–76, all returning to the Cais) will ask for (Docs/MetodologiaMapasERacas.md
        /// §2.1): passages and reed boats for the river and the canals of the Campos de Juncos (71, 72), root boots for the mud of
        /// the canals and the quicksand of Apep's sandbank (72, 76), oil and runic lamps and the oil of eternal fire for the night
        /// that never ends and the dark coils of Mehen (73, 74), water skins for the Areal Escondido and the Lago de Fogo (73, 75),
        /// theriac and the antivenom amulet for the black scorpions and the snakes (73, 76) and rope for the sand-sledge (73).
        /// The counter turns daily.
        /// </summary>
        public const string TrocasKhunanup = "shop_trocas_khunanup";

        /// <summary>
        /// Betume do Atrahasis (Dur-Jarras, Docs/Mapas100.md map 77): Atrahasis, the master of the bitumen rafts, sells the gear maps
        /// 77–82 (all returning to Dur-Jarras) will ask for (Docs/MetodologiaMapasERacas.md §2.1): water skins for the Deserto do Relógio
        /// (78), reed boats, passages and root boots for the Poços de Betume (79), the oil of eternal fire to light the pitch (79), rope and
        /// crampons for the Cedral and the Ombro de Upelluri (80, 81), oil lamps and theriac for the fumes of the pitch. The counter turns daily.
        /// </summary>
        public const string BetumeAtrahasis = "shop_betume_atrahasis";

        /// <summary>
        /// Pavios da Tehat (Fortaleza da Última Lâmpada, Docs/Mapas100.md map 83): Tehat, the weaver of wicks, sells the gear maps 83–90 (all returning
        /// to the Fortaleza) will ask for: air bottles and theriac for the smoke (84), canvas wings and storm cloaks for the shaft of the wind (86), breathing
        /// shells and air bubbles for the drowned halls (87), oil lamps, runic lamps and the oil of eternal fire for the dark (88) and the lamp network,
        /// the balance pole for the bridge of the Novelo (89) and rope. The counter turns daily.
        /// </summary>
        public const string PaviosTehat = "shop_pavios_tehat";

        /// <summary>
        /// A Última Caravana do Ammo (Cais da Coluna, Docs/Mapas100.md map 91): Ammo's counter at the end of the road. The fichas of maps 91–100 ask for no gear of their own: he sells
        /// what the road up here has always needed (potions, the runic lamp and its oil, the storm cloak and canvas wings for the high wind, rope and theriac) and what the caravans bring. The counter turns daily.
        /// </summary>
        public const string UltimaCaravanaAmmo = "shop_ultima_caravana_ammo";

        /// <summary>"Alforje de Feltro": buying it adds <see cref="BornalRows"/> rows to the Mochila for good (<c>InventoryManager.AddBagRows</c>).</summary>
        public const string BornalItemId = "item_bornal_couro";

        /// <summary>Rows one Alforje de Feltro adds (the bag's cap, <c>InventoryManager.MaxExtraRows</c>, is 2: one bornal per save).</summary>
        public const int BornalRows = 2;

        /// <summary>Asset path (under Resources) of shops that are not in <see cref="Shops"/>.</summary>
        public const string AssetFolder = "Shops/";

        public static readonly IReadOnlyList<CityShopSpec> Shops = new List<CityShopSpec>
        {
            // ---- Vila da Vó Edna (Floresta) ----
            new CityShopSpec(MerceariaLazaro, ShopTheme.Mercearia, BiomeType.Forest, ShopRotationPeriod.Daily, 2, WithHouse(MerceariaLazaro, new List<CityShopEntry>
            {
                new CityShopEntry("item_cooked_meat", 6, 10),
                new CityShopEntry("item_health_potion", 15, 5),
                new CityShopEntry("item_herb", 3, 15, rotating: true),
                new CityShopEntry("item_wood", 2, 20, rotating: true),
                new CityShopEntry("item_fiber", 1, 20, rotating: true),
                new CityShopEntry("item_mana_potion", 16, 3, rotating: true, unlock: ShopUnlock.Unlocked(BiomeType.Desert)),
            }), houseSlots: 2),
            new CityShopSpec(ArmarinhoCida, ShopTheme.Armarinho, BiomeType.Forest, ShopRotationPeriod.Weekly, 2, WithHouse(ArmarinhoCida, new List<CityShopEntry>
            {
                new CityShopEntry("item_fiber", 1, 30),
                new CityShopEntry("item_leather_boots", 10, 2),
                new CityShopEntry("item_leather_helmet", 12, 2, rotating: true),
                new CityShopEntry("item_leather_chest", 20, 1, rotating: true),
                new CityShopEntry("item_herb", 3, 10, rotating: true),
                new CityShopEntry("item_hunting_bow", 38, 1, rotating: true, unlock: ShopUnlock.Cleared(BiomeType.Forest)),
                // Alforje de Feltro (+2 rows of the Mochila, once): for a regular of Mariam (freguesia tier 1).
                new CityShopEntry(BornalItemId, 150, 1, unlock: ShopUnlock.Reputation(ShopReputation.Regular)),
                // Map gates (MapRequirementRules): the rope of the Copas de Garajonay (05) and a copy of the Garden Key (06).
                new CityShopEntry("item_corda_escalada", 38, 2),
                new CityShopEntry("item_chave_jardim", 66, 1, unlock: ShopUnlock.Cleared(BiomeType.Forest)),
            }), houseSlots: 2),
            new CityShopSpec(FeiraLivre, ShopTheme.Feira, BiomeType.Forest, ShopRotationPeriod.Daily, 3, WithHouse(FeiraLivre, new List<CityShopEntry>
            {
                new CityShopEntry("item_herb", 3, 20),
                new CityShopEntry("item_wood", 2, 30, rotating: true),
                new CityShopEntry("item_fiber", 1, 30, rotating: true),
                new CityShopEntry("item_stone", 1, 30, rotating: true),
                new CityShopEntry("item_cooked_meat", 5, 8, rotating: true),
                new CityShopEntry("item_iron_ore", 6, 5, rotating: true, unlock: ShopUnlock.Unlocked(BiomeType.Mountain)),
            }), houseSlots: 3),

            // ---- Oásis de Qocho (Deserto): couro de vaqueiro; and the Móveis da Cisterna de Qocho (Docs/Housing.md §6.2), the Epic as the week's special ----
            new CityShopSpec(SelariaVava, ShopTheme.Speciality, BiomeType.Desert, ShopRotationPeriod.Weekly, 2, WithHouse(SelariaVava, new List<CityShopEntry>
            {
                new CityShopEntry("item_leather_boots", 9, 3),
                new CityShopEntry("item_leather_helmet", 11, 2),
                new CityShopEntry("item_leather_chest", 18, 2),
                new CityShopEntry("item_health_potion", 16, 3, rotating: true),
                new CityShopEntry("item_cooked_meat", 7, 6, rotating: true),
                new CityShopEntry("item_fiber", 1, 20, rotating: true),
                new CityShopEntry("item_hunting_bow", 33, 1, rotating: true, unlock: ShopUnlock.Cleared(BiomeType.Desert)),
                // Ema Corredora (Docs/Mounts.md §3.1, M3 wave 1): the oasis critter fair lives here until it has a stall of its own.
                new CityShopEntry("item_whistle_mount_rhea", 75, -1),
                // Map gates (MapRequirementRules): water for the Dunas que Cantam (10), theriac for Namrael (12).
                new CityShopEntry("item_odre_agua", 28, 5),
                new CityShopEntry("item_teriaga", 48, 3),
            }), houseSlots: 3, weeklySlots: 1),

            // ---- Vila de Litema (Montanha): ferreiro; and the Móveis do Rondavel (Docs/Housing.md §6.2), the Epic as the week's special ----
            new CityShopSpec(ForjaTonho, ShopTheme.Ferreiro, BiomeType.Mountain, ShopRotationPeriod.Weekly, 2, WithHouse(ForjaTonho, new List<CityShopEntry>
            {
                new CityShopEntry("item_iron", 7, 20),
                new CityShopEntry("item_iron_ore", 5, 20),
                new CityShopEntry("item_pickaxe", 18, 1),
                new CityShopEntry("item_hammer", 15, 1, rotating: true),
                new CityShopEntry("item_axe", 18, 1, rotating: true),
                new CityShopEntry("item_wooden_sword", 10, 1, rotating: true),
                new CityShopEntry("item_health_potion", 17, 3, rotating: true),
                new CityShopEntry("item_iron_sword", 41, 1, rotating: true, unlock: ShopUnlock.Cleared(BiomeType.Mountain)),
                // Pedras da Luz Presa (refino, Docs/Balance/README.md "Refino de item"): the blacksmith sells them ~10% cheaper.
                new CityShopEntry("item_pedra_bencao", 11, 5),
                new CityShopEntry("item_pedra_alma", 32, 2, unlock: ShopUnlock.Cleared(BiomeType.Mountain)),
                // Bode Saltador (Docs/Mounts.md §3.1, M3 wave 1): the Curral do Pico sells through the forge until it has a stall.
                new CityShopEntry("item_whistle_mount_mountain_goat", 70, -1),
                // Map gates (MapRequirementRules): the wool cloak of the Passo do Hahya (14), ginger tea for the Mazans (18).
                new CityShopEntry("item_manta_la", 42, 2),
                new CityShopEntry("item_cha_gengibre", 66, 3, unlock: ShopUnlock.Cleared(BiomeType.Mountain)),
                // The oil lamp of the Grutas (17, 19, 20): every gallery down from the Passo do Hahya already asks for it, so the
                // forge on the way down sells it too (a little dearer than the Vila das Lamparinas), or the Grutas could not be entered.
                new CityShopEntry("item_lamparina_oleo", 30, 2),
            }), houseSlots: 3, weeklySlots: 1),

            // ---- Vila das Lamparinas (Cavernas): lamparinas e botica; and the Móveis da Casa na Rocha (Docs/Housing.md §6.2), the Epic as the week's special ----
            new CityShopSpec(LamparinasBenedita, ShopTheme.Speciality, BiomeType.Caves, ShopRotationPeriod.Daily, 2, WithHouse(LamparinasBenedita, new List<CityShopEntry>
            {
                new CityShopEntry("item_health_potion", 15, 5),
                new CityShopEntry("item_mana_potion", 13, 5),
                new CityShopEntry("item_herb", 3, 15, rotating: true),
                new CityShopEntry("item_iron_ore", 6, 8, rotating: true),
                new CityShopEntry("item_stone", 1, 20, rotating: true),
                new CityShopEntry("item_leather_helmet", 12, 1, rotating: true),
                new CityShopEntry("item_iron_sword", 45, 1, rotating: true, unlock: ShopUnlock.Cleared(BiomeType.Caves)),
                new CityShopEntry("item_amuleto_sorte", 25, 1, rotating: true),
                // Tamanduá-Bandeira (Docs/Mounts.md §3.2, M3 wave 1): the caves' keeper den.
                new CityShopEntry("item_whistle_mount_giant_anteater", 140, -1, unlock: ShopUnlock.Cleared(BiomeType.Desert)),
                // Map gates (MapRequirementRules): the oil lamp of the Grutas (17, 19, 20), the runic lamp (21) and the eternal oil (23).
                new CityShopEntry("item_lamparina_oleo", 27, 3),
                new CityShopEntry("item_lamparina_runica", 150, 1, unlock: ShopUnlock.Cleared(BiomeType.Caves)),
                new CityShopEntry("item_oleo_fogo_eterno", 185, 2, unlock: ShopUnlock.Cleared(BiomeType.Caves)),
            }), houseSlots: 3, weeklySlots: 1),

            // ---- Copas de Garajonay (elfos-da-mata): Dácil's herbs, mist potions and canopy gear; the counter turns daily ----
            new CityShopSpec(ErvariaDacil, ShopTheme.Speciality, BiomeType.Forest, ShopRotationPeriod.Daily, 3, WithHouse(ErvariaDacil, new List<CityShopEntry>
            {
                new CityShopEntry("item_herb", 3, 20),
                new CityShopEntry("item_health_potion", 14, 5),
                // Map gates (MapRequirementRules): the rope of the canopy (05) and a copy of the Garden Key (06).
                new CityShopEntry("item_corda_escalada", 36, 3),
                new CityShopEntry("item_chave_jardim", 60, 1, unlock: ShopUnlock.Cleared(BiomeType.Forest)),
                // The rotation: mist potions, laurel wood and fibre, mount feed (the Lagarto-do-Louro eats it too), Jonay's bows.
                new CityShopEntry("item_mana_potion", 14, 4, rotating: true),
                new CityShopEntry("item_fiber", 1, 20, rotating: true),
                new CityShopEntry("item_wood", 2, 20, rotating: true),
                new CityShopEntry("item_cooked_meat", 6, 6, rotating: true),
                new CityShopEntry("item_mount_feed", 3, 10, rotating: true),
                new CityShopEntry("item_hunting_bow", 33, 1, rotating: true),
                new CityShopEntry("item_amuleto_sorte", 24, 1, rotating: true, unlock: ShopUnlock.Cleared(BiomeType.Forest)),
            })),

            // ---- Taa Pwani: Mwana's pearl bazaar, with the gear of the gates 25, 28 and 30; the counter turns daily ----
            new CityShopSpec(BazarMwana, ShopTheme.Speciality, BiomeType.Forest, ShopRotationPeriod.Daily, 3, WithHouse(BazarMwana, new List<CityShopEntry>
            {
                new CityShopEntry("item_health_potion", 14, 6),
                // Map gates (MapRequirementRules): the dhow out to the reef (25), the breathing shell (28: Kirta itself stands on
                // that map) and the living-root boots (30). The relic of 27 is Lugalbanda's (Aratta), the air bubble of 29 Danel's (Kirta).
                new CityShopEntry("item_passagem_dhow", 300, 3),
                new CityShopEntry("item_concha_respiravel", 450, 2),
                new CityShopEntry("item_botas_raiz", 520, 1),
                // The rotation: mist potions, salt-dried meat, mount feed, rope fibre and timber off the dhows, lucky charms.
                new CityShopEntry("item_mana_potion", 14, 4, rotating: true),
                new CityShopEntry("item_cooked_meat", 6, 8, rotating: true),
                new CityShopEntry("item_mount_feed", 3, 10, rotating: true),
                new CityShopEntry("item_fiber", 1, 20, rotating: true),
                new CityShopEntry("item_wood", 2, 20, rotating: true),
                new CityShopEntry("item_amuleto_sorte", 24, 1, rotating: true),
            }), houseSlots: 3, weeklySlots: 1),

            // ---- Tambor Rubro: Tushratta's Forja de Tambor (copper and bronze smiths), the theriac of map 12; the counter turns weekly ----
            new CityShopSpec(ForjaTambor, ShopTheme.Ferreiro, BiomeType.Desert, ShopRotationPeriod.Weekly, 3, WithHouse(ForjaTambor, new List<CityShopEntry>
            {
                new CityShopEntry("item_iron", 8, 20),
                new CityShopEntry("item_iron_ore", 5, 20),
                new CityShopEntry("item_health_potion", 15, 5),
                // Pedras da Luz Presa (refino): the drums wake the forge's Fogo Vivo, the stones come a little cheaper.
                new CityShopEntry("item_pedra_bencao", 11, 5),
                // Map gate (MapRequirementRules): the Teriaga do Oásis for the Trono de Namrael (12), whose return city this is.
                new CityShopEntry("item_teriaga", 50, 3),
                // The rotation: tools, blades, red leather, Kessi's bows, meat and feed for the war wolves.
                new CityShopEntry("item_hammer", 15, 1, rotating: true),
                new CityShopEntry("item_axe", 18, 1, rotating: true),
                new CityShopEntry("item_leather_chest", 20, 1, rotating: true),
                new CityShopEntry("item_hunting_bow", 35, 1, rotating: true),
                new CityShopEntry("item_cooked_meat", 6, 8, rotating: true),
                new CityShopEntry("item_mount_feed", 3, 10, rotating: true),
                new CityShopEntry("item_iron_sword", 45, 1, rotating: true, unlock: ShopUnlock.Cleared(BiomeType.Desert)),
            })),

            // ---- Salões de Bronze: Puzur's Portões de Runa (bronze runes), the pickaxe and lamp of map 17, the ginger tea of 18; weekly ----
            new CityShopSpec(PortoesRuna, ShopTheme.Speciality, BiomeType.Mountain, ShopRotationPeriod.Weekly, 3, WithHouse(PortoesRuna, new List<CityShopEntry>
            {
                new CityShopEntry("item_health_potion", 15, 5),
                new CityShopEntry("item_mana_potion", 15, 5),
                // Pedras da Luz Presa (refino): the dwarves refined for the whole island before their forge went cold.
                new CityShopEntry("item_pedra_bencao", 11, 5),
                new CityShopEntry("item_pedra_alma", 32, 2, unlock: ShopUnlock.Cleared(BiomeType.Mountain)),
                // Map gates (MapRequirementRules): pickaxe and torch for the Salões (17), ginger tea for the Cume dos Mazans (18).
                new CityShopEntry("item_pickaxe", 18, 2),
                new CityShopEntry("item_lamparina_oleo", 28, 3),
                new CityShopEntry("item_cha_gengibre", 66, 3),
                // The rotation: Puzur's bronze runes, iron and ore from Shilhak's seam, the hall's hammer.
                new CityShopEntry("item_runa_clareza", 15, 2, rotating: true),
                new CityShopEntry("item_runa_couro", 20, 2, rotating: true),
                new CityShopEntry("item_runa_cura", 15, 2, rotating: true),
                new CityShopEntry("item_runa_vento", 20, 2, rotating: true),
                new CityShopEntry("item_runa_cura_grande", 35, 1, rotating: true, unlock: ShopUnlock.Cleared(BiomeType.Mountain)),
                new CityShopEntry("item_iron", 8, 20, rotating: true),
                new CityShopEntry("item_iron_ore", 5, 20, rotating: true),
                new CityShopEntry("item_hammer", 15, 1, rotating: true),
            })),

            // ---- Mercado dos Túneis: the Feira da Miwnay (mended scrap, lamps, bottled Pure Air), the eternal-fire oil of 23 and the canvas wings of 24; daily ----
            new CityShopSpec(FeiraMiwnay, ShopTheme.Feira, BiomeType.Caves, ShopRotationPeriod.Daily, 3, WithHouse(FeiraMiwnay, new List<CityShopEntry>
            {
                new CityShopEntry("item_health_potion", 15, 5),
                new CityShopEntry("item_mana_potion", 14, 5),
                new CityShopEntry("item_lamparina_oleo", 28, 3),
                // Map gate (MapRequirementRules): the Óleo do Fogo Eterno for the Cânion da Roda (23), whose return city this is.
                new CityShopEntry("item_oleo_fogo_eterno", 190, 2),
                // Map gate (MapRequirementRules): Takhsich's canvas wings, the double jump of the Ninho de Mahaway (24).
                new CityShopEntry("item_asas_lona_takhsich", 420, 1),
                // The kobolds' own trade (Docs/Mapas100.md §3.3): bottled Pure Air, for the still air far from the pipes.
                new CityShopEntry("item_garrafa_ar_sereno", 30, 10),
                // The rotation: what the market brought up from the four galleries today.
                new CityShopEntry("item_iron_ore", 5, 12, rotating: true),
                new CityShopEntry("item_stone", 1, 20, rotating: true),
                new CityShopEntry("item_parte_lasca_golem", 10, 5, rotating: true),
                new CityShopEntry("item_parte_asa_morcego", 12, 5, rotating: true),
                new CityShopEntry("item_leather_helmet", 12, 1, rotating: true),
                new CityShopEntry("item_amuleto_sorte", 24, 1, rotating: true),
                new CityShopEntry("item_pickaxe", 18, 1, rotating: true),
                new CityShopEntry("item_lamparina_runica", 155, 1, rotating: true, unlock: ShopUnlock.Cleared(BiomeType.Caves)),
            })),

            // ---- Aratta: the Alforje do Lugalbanda (rope, waterskins, savanna food), the Artífices' relic of map 27; daily ----
            new CityShopSpec(AlforjeLugalbanda, ShopTheme.Speciality, BiomeType.Forest, ShopRotationPeriod.Daily, 3, WithHouse(AlforjeLugalbanda, new List<CityShopEntry>
            {
                new CityShopEntry("item_health_potion", 14, 6),
                new CityShopEntry("item_odre_agua", 30, 3),
                // Map gate (MapRequirementRules): the Relíquia dos Artífices for the Pirâmides de Meroé (27), whose return city this is.
                new CityShopEntry("item_reliquia_artifices", 400, 1),
                // The rotation: what Lugalbanda brought back from his wanderings, and the herders' own trade.
                new CityShopEntry("item_corda_escalada", 36, 2, rotating: true),
                new CityShopEntry("item_mana_potion", 14, 4, rotating: true),
                new CityShopEntry("item_cooked_meat", 6, 8, rotating: true),
                new CityShopEntry("item_mount_feed", 3, 12, rotating: true),
                new CityShopEntry("item_leather_boots", 12, 1, rotating: true),
                new CityShopEntry("item_fiber", 1, 20, rotating: true),
                new CityShopEntry("item_amuleto_sorte", 24, 1, rotating: true),
            })),

            // ---- Kirta: Danel's diving tools, the breathing shell of 28 and the air bubble of 29; daily ----
            new CityShopSpec(MergulhoDanel, ShopTheme.Speciality, BiomeType.Forest, ShopRotationPeriod.Daily, 3, WithHouse(MergulhoDanel, new List<CityShopEntry>
            {
                new CityShopEntry("item_health_potion", 15, 6),
                new CityShopEntry("item_mana_potion", 14, 6),
                // Map gates (MapRequirementRules): the Concha Respirável (28, Kirta's own map; Mwana sells it too, for the way in)
                // and the Bolha de Ar Engarrafada for Kilwa Afundada (29), whose return city this is.
                new CityShopEntry("item_concha_respiravel", 450, 2),
                new CityShopEntry("item_bolha_ar", 500, 2),
                // The rotation: what the reef gives back as the coral is relit.
                new CityShopEntry("item_fish_peixe_dourado", 60, 3, rotating: true),
                new CityShopEntry("item_fiber", 1, 20, rotating: true),
                new CityShopEntry("item_cooked_meat", 6, 8, rotating: true),
                new CityShopEntry("item_amuleto_sorte", 24, 1, rotating: true),
                new CityShopEntry("item_pedra_bencao", 12, 3, rotating: true),
                new CityShopEntry("item_hunting_bow", 34, 1, rotating: true),
            })),

            // ---- Jhilpur: Nuhzadag's boats and trades, the gear of the gates 32–35; daily ----
            new CityShopSpec(TrocasNuhzadag, ShopTheme.Speciality, BiomeType.Forest, ShopRotationPeriod.Daily, 3, WithHouse(TrocasNuhzadag, new List<CityShopEntry>
            {
                new CityShopEntry("item_health_potion", 15, 6),
                new CityShopEntry("item_mana_potion", 15, 6),
                // Map gates (MapRequirementRules): the balance pole of the Pontes de Raiz Viva (32), the reed boat of the Palafitas
                // de Tarchon (33), the antivenom amulet of the Charco do Ninki Nanka (34) and the purifying fruit of the Coração do
                // Lodo (35). The boats and amulets of Tarchon (33, 34) also come down the canals to Nuhzadag.
                new CityShopEntry("item_vara_equilibrio", 480, 2),
                new CityShopEntry("item_barco_junco", 600, 1),
                new CityShopEntry("item_amuleto_antiveneno", 700, 2),
                new CityShopEntry("item_fruta_purificadora", 750, 2),
                // The rotation: what the boats bring up the delta.
                new CityShopEntry("item_corda_escalada", 40, 2, rotating: true),
                new CityShopEntry("item_fish_peixe_dourado", 60, 3, rotating: true),
                new CityShopEntry("item_cooked_meat", 6, 8, rotating: true),
                new CityShopEntry("item_mount_feed", 3, 12, rotating: true),
                new CityShopEntry("item_fiber", 1, 20, rotating: true),
                new CityShopEntry("item_amuleto_sorte", 25, 1, rotating: true),
                new CityShopEntry("item_pedra_bencao", 12, 3, rotating: true),
            }), houseSlots: 3, weeklySlots: 1),

            // ---- Palafitas de Tarchon: Larthia's baskets, the reed boat of 33 and the antivenom amulet of 34; daily ----
            new CityShopSpec(CestariaLarthia, ShopTheme.Speciality, BiomeType.Forest, ShopRotationPeriod.Daily, 3, WithHouse(CestariaLarthia, new List<CityShopEntry>
            {
                new CityShopEntry("item_health_potion", 15, 6),
                new CityShopEntry("item_mana_potion", 15, 5),
                // Map gates (MapRequirementRules): the Barco de Junco for the Palafitas de Tarchon (33: the trols weave the
                // boats; Nuhzadag sells them too, for the way in) and the Amuleto Antiveneno for the Charco do Ninki Nanka (34),
                // whose return city this is.
                new CityShopEntry("item_barco_junco", 600, 2),
                new CityShopEntry("item_amuleto_antiveneno", 700, 2),
                // The Cesto de Lanternas: oil lamps to carry a light through the swamp night (Docs/Mapas100.md §3.3).
                new CityShopEntry("item_lamparina_oleo", 30, 3),
                // Larthia's bags (Docs/Mapas100.md §3.3: +1 row of the backpack, her version of the Alforje): one per save.
                new CityShopEntry(BornalItemId, 160, 1, unlock: ShopUnlock.Cleared(BiomeType.Forest)),
                // Sapo-Gigante de Tarchon (Docs/Mounts.md §3.10): the trols' stilt-to-stilt frog.
                new CityShopEntry("item_whistle_mount_tarchon_frog", 145, -1),
                // The rotation: what the trols weave and fish.
                new CityShopEntry("item_fish_bagre_cego", 15, 4, rotating: true),
                new CityShopEntry("item_fish_traira", 8, 4, rotating: true),
                new CityShopEntry("item_fiber", 1, 20, rotating: true),
                new CityShopEntry("item_herb", 3, 12, rotating: true),
                new CityShopEntry("item_cooked_meat", 6, 8, rotating: true),
                new CityShopEntry("item_mount_feed", 3, 12, rotating: true),
                new CityShopEntry("item_corda_escalada", 40, 2, rotating: true),
                new CityShopEntry("item_amuleto_sorte", 25, 1, rotating: true),
            })),

            // ---- Sangkent: Sisinnios's mountain gear, the gear of the gates 36, 38 and 40; daily ----
            new CityShopSpec(AgasalhosSisinnios, ShopTheme.Speciality, BiomeType.Forest, ShopRotationPeriod.Daily, 3, WithHouse(AgasalhosSisinnios, new List<CityShopEntry>
            {
                new CityShopEntry("item_health_potion", 15, 6),
                new CityShopEntry("item_mana_potion", 15, 6),
                // Map gates (MapRequirementRules): the thermal clothes of Sangkent itself (36, worn in the Chest slot), the
                // crampons of the Geleira do Teto do Mundo (38) and the storm cloak of the Passo de Wakhan (40).
                new CityShopEntry("item_roupa_termica", 900, 2),
                new CityShopEntry("item_grampos_escalada", 950, 2),
                new CityShopEntry("item_capa_tempestade", 1100, 1),
                // For the cold of the passes (Docs/Mapas100.md 38: the medidor de frio): wool cloaks and ginger tea.
                new CityShopEntry("item_manta_la", 45, 2),
                new CityShopEntry("item_cha_gengibre", 70, 3),
                // Iaque do Teto do Mundo (Docs/Mounts.md §3.10): the caravans' yak of the passes.
                new CityShopEntry("item_whistle_mount_roof_yak", 150, -1),
                // The rotation: what the caravans bring up the Rota da Seda.
                new CityShopEntry("item_corda_escalada", 40, 2, rotating: true),
                new CityShopEntry("item_cooked_meat", 6, 8, rotating: true),
                new CityShopEntry("item_mount_feed", 3, 12, rotating: true),
                new CityShopEntry("item_fiber", 1, 20, rotating: true),
                new CityShopEntry("item_herb", 3, 12, rotating: true),
                new CityShopEntry("item_amuleto_sorte", 25, 1, rotating: true),
                new CityShopEntry("item_pedra_bencao", 12, 3, rotating: true),
            }), houseSlots: 3, weeklySlots: 1),

            // ---- Mingcheng: Mōzak's lanterns, the gear of the gate 42 (the Selo do Sol); daily ----
            new CityShopSpec(LanternasMozak, ShopTheme.Speciality, BiomeType.Forest, ShopRotationPeriod.Daily, 3, WithHouse(LanternasMozak, new List<CityShopEntry>
            {
                new CityShopEntry("item_health_potion", 15, 6),
                new CityShopEntry("item_mana_potion", 15, 6),
                // Map gate (MapRequirementRules): the Sun Seal that opens the Torres Brancas de Amanirenas (42), bound to the hero.
                new CityShopEntry("item_selo_sol", 1500, 1),
                // Light for the outer rings at night (the bell shuts the gates: whoever is outside stays outside).
                new CityShopEntry("item_lamparina_oleo", 30, 3),
                new CityShopEntry("item_lamparina_runica", 160, 1),
                // The rotation: what the barges bring up the canal.
                new CityShopEntry("item_cooked_meat", 6, 8, rotating: true),
                new CityShopEntry("item_mount_feed", 3, 12, rotating: true),
                new CityShopEntry("item_fiber", 1, 20, rotating: true),
                new CityShopEntry("item_herb", 3, 12, rotating: true),
                new CityShopEntry("item_corda_escalada", 40, 2, rotating: true),
                new CityShopEntry("item_amuleto_sorte", 25, 1, rotating: true),
                new CityShopEntry("item_pedra_bencao", 12, 3, rotating: true),
            }), houseSlots: 3, weeklySlots: 1),

            // ---- Torres Brancas: Natakamani's mirrors, the stones of the refining; daily ----
            new CityShopSpec(EspelhosNatakamani, ShopTheme.Speciality, BiomeType.Forest, ShopRotationPeriod.Daily, 3, WithHouse(EspelhosNatakamani, new List<CityShopEntry>
            {
                new CityShopEntry("item_health_potion", 15, 6),
                new CityShopEntry("item_mana_potion", 15, 6),
                // Refining with reflected light: the stones of the forge, always on the counter.
                new CityShopEntry("item_pedra_bencao", 12, 6),
                new CityShopEntry("item_pedra_alma", 35, 3),
                new CityShopEntry("item_runa_clareza", 15, 3),
                // The rotation.
                new CityShopEntry("item_lamparina_runica", 160, 1, rotating: true),
                new CityShopEntry("item_runa_cura", 15, 3, rotating: true),
                new CityShopEntry("item_herb", 3, 12, rotating: true),
                new CityShopEntry("item_cooked_meat", 6, 8, rotating: true),
                new CityShopEntry("item_amuleto_sorte", 25, 1, rotating: true),
            })),

            // ---- Acampamento de Soslan: Syrdon's saddlery, felt and feed for the steppe; daily ----
            new CityShopSpec(SelariaNomade, ShopTheme.Speciality, BiomeType.Forest, ShopRotationPeriod.Daily, 3, WithHouse(SelariaNomade, new List<CityShopEntry>
            {
                new CityShopEntry("item_health_potion", 15, 6),
                new CityShopEntry("item_mana_potion", 15, 6),
                // Felt for the saddles (the wool blankets keep the steppe wind out too) and feed for the herds.
                new CityShopEntry("item_manta_la", 45, 3),
                new CityShopEntry("item_mount_feed", 3, 20),
                new CityShopEntry("item_leather_boots", 10, 2),
                // The rotation: what the wagons carry this week.
                new CityShopEntry("item_odre_agua", 30, 2, rotating: true),
                new CityShopEntry("item_corda_escalada", 40, 2, rotating: true),
                new CityShopEntry("item_cooked_meat", 6, 8, rotating: true),
                new CityShopEntry("item_fiber", 1, 20, rotating: true),
                new CityShopEntry("item_herb", 3, 12, rotating: true),
                new CityShopEntry("item_amuleto_sorte", 25, 1, rotating: true),
                new CityShopEntry("item_pedra_bencao", 12, 3, rotating: true),
            }), houseSlots: 3, weeklySlots: 1),

            // ---- Fortim dos Gigantes: Shahmizad's great pot, food for the group and the gear of the gate 52; daily ----
            new CityShopSpec(PanelaGrande, ShopTheme.Speciality, BiomeType.Forest, ShopRotationPeriod.Daily, 3, WithHouse(PanelaGrande, new List<CityShopEntry>
            {
                new CityShopEntry("item_health_potion", 15, 6),
                new CityShopEntry("item_mana_potion", 15, 6),
                // Food for the whole group, ladled from the pot.
                new CityShopEntry("item_cooked_meat", 6, 15),
                new CityShopEntry("item_cha_gengibre", 70, 3),
                // Map gate (MapRequirementRules): the crampons of the Corrente de Amirani (52), whose return city is the Fortim.
                new CityShopEntry("item_grampos_escalada", 950, 2),
                // The rotation.
                new CityShopEntry("item_manta_la", 45, 2, rotating: true),
                new CityShopEntry("item_corda_escalada", 40, 2, rotating: true),
                new CityShopEntry("item_mount_feed", 3, 12, rotating: true),
                new CityShopEntry("item_herb", 3, 12, rotating: true),
                new CityShopEntry("item_amuleto_sorte", 25, 1, rotating: true),
                new CityShopEntry("item_pedra_bencao", 12, 3, rotating: true),
            })),

            // ---- Arvoredo de Haripushpa: Suvarnapushpa's nursery, herbs and healing runes; daily ----
            new CityShopSpec(ViveiroSuvarnapushpa, ShopTheme.Speciality, BiomeType.Forest, ShopRotationPeriod.Daily, 3, WithHouse(ViveiroSuvarnapushpa, new List<CityShopEntry>
            {
                new CityShopEntry("item_health_potion", 15, 6),
                new CityShopEntry("item_mana_potion", 15, 6),
                // What grows slowly in the grove: herbs, fibre and the healing rune.
                new CityShopEntry("item_herb", 3, 20),
                new CityShopEntry("item_fiber", 1, 30),
                new CityShopEntry("item_runa_cura", 15, 3),
                // The rotation.
                new CityShopEntry("item_cooked_meat", 6, 8, rotating: true),
                new CityShopEntry("item_mount_feed", 3, 12, rotating: true),
                new CityShopEntry("item_corda_escalada", 40, 2, rotating: true),
                new CityShopEntry("item_manta_la", 45, 2, rotating: true),
                new CityShopEntry("item_amuleto_sorte", 25, 1, rotating: true),
                new CityShopEntry("item_pedra_bencao", 12, 3, rotating: true),
            })),

            // ---- Vale dos Sátiros: Marsyas' double flute, wind runes, songs and travel food; daily ----
            new CityShopSpec(FlautaDupla, ShopTheme.Speciality, BiomeType.Forest, ShopRotationPeriod.Daily, 3, WithHouse(FlautaDupla, new List<CityShopEntry>
            {
                new CityShopEntry("item_health_potion", 15, 6),
                new CityShopEntry("item_mana_potion", 15, 6),
                // The songs of the road (the wind rune) and the Serene Air bottled on the high paths.
                new CityShopEntry("item_runa_vento", 20, 4),
                new CityShopEntry("item_garrafa_ar_sereno", 30, 6),
                new CityShopEntry("item_cooked_meat", 6, 10),
                // The rotation: what the satyrs bring down from the vines and the peaks.
                new CityShopEntry("item_herb", 3, 12, rotating: true),
                new CityShopEntry("item_fiber", 1, 20, rotating: true),
                new CityShopEntry("item_corda_escalada", 40, 2, rotating: true),
                new CityShopEntry("item_runa_clareza", 15, 2, rotating: true),
                new CityShopEntry("item_amuleto_sorte", 25, 1, rotating: true),
                new CityShopEntry("item_pedra_bencao", 12, 3, rotating: true),
            })),

            // ---- Farnkesh: Sisin's rope rack, the gear of the gates 57 and 58 and of the bridges; daily; and the Móveis do Ninho de Pedra (Docs/Housing.md §6.2), the Epic as the week's special ----
            new CityShopSpec(CordasSisin, ShopTheme.Speciality, BiomeType.Forest, ShopRotationPeriod.Daily, 3, WithHouse(CordasSisin, new List<CityShopEntry>
            {
                new CityShopEntry("item_health_potion", 15, 6),
                new CityShopEntry("item_mana_potion", 15, 6),
                // Map gates (MapRequirementRules): the storm cloak of the Garganta dos Ays (57) and the canvas wings that glide
                // between the islands of the Ninho da Huma (58); both maps return to Farnkesh.
                new CityShopEntry("item_capa_tempestade", 1100, 2),
                new CityShopEntry("item_asas_lona_takhsich", 420, 2),
                // Rope and poles for the bridges, crampons for the hanging hermitages.
                new CityShopEntry("item_corda_escalada", 40, 4),
                new CityShopEntry("item_vara_equilibrio", 480, 2),
                new CityShopEntry("item_grampos_escalada", 950, 1),
                // The rotation.
                new CityShopEntry("item_cha_gengibre", 70, 3, rotating: true),
                new CityShopEntry("item_manta_la", 45, 2, rotating: true),
                new CityShopEntry("item_garrafa_ar_sereno", 30, 4, rotating: true),
                new CityShopEntry("item_runa_vento", 20, 2, rotating: true),
                new CityShopEntry("item_cooked_meat", 6, 8, rotating: true),
                new CityShopEntry("item_amuleto_sorte", 25, 1, rotating: true),
                new CityShopEntry("item_pedra_bencao", 12, 3, rotating: true),
            }), houseSlots: 3, weeklySlots: 1),

            // ---- Porto da Lua Branca: Pamour's furs and boats, the gear of every gate of the Mar Gelado (59–64); daily ----
            new CityShopSpec(PelesPamour, ShopTheme.Speciality, BiomeType.Forest, ShopRotationPeriod.Daily, 3, WithHouse(PelesPamour, new List<CityShopEntry>
            {
                new CityShopEntry("item_health_potion", 15, 6),
                new CityShopEntry("item_mana_potion", 15, 6),
                // Map gates (MapRequirementRules) of the maps that return to the Porto, fixed from the first day: the cold (thermal
                // furs, ginger tea, wool), the canals and the open sea (passages, reed boats), the galleries under the ice (oil and
                // runic lamps), the ice walls (crampons) and the polar wind (storm cloak).
                new CityShopEntry("item_roupa_termica", 900, 2),
                new CityShopEntry("item_cha_gengibre", 70, 4),
                new CityShopEntry("item_manta_la", 45, 3),
                new CityShopEntry("item_passagem_dhow", 300, 3),
                new CityShopEntry("item_barco_junco", 600, 2),
                new CityShopEntry("item_lamparina_oleo", 30, 4),
                new CityShopEntry("item_lamparina_runica", 160, 2),
                new CityShopEntry("item_grampos_escalada", 950, 1),
                new CityShopEntry("item_capa_tempestade", 1100, 1),
                // The rotation: what the boats bring in from the ice.
                new CityShopEntry("item_cooked_meat", 6, 10, rotating: true),
                new CityShopEntry("item_corda_escalada", 40, 3, rotating: true),
                new CityShopEntry("item_garrafa_ar_sereno", 30, 4, rotating: true),
                new CityShopEntry("item_runa_clareza", 15, 2, rotating: true),
                new CityShopEntry("item_runa_vento", 20, 2, rotating: true),
                new CityShopEntry("item_amuleto_sorte", 25, 1, rotating: true),
                new CityShopEntry("item_pedra_bencao", 12, 3, rotating: true),
            }), houseSlots: 3, weeklySlots: 1),

            // ---- Hinokura: Kurogane's black iron, the gear of the gates of the Arquipélago de Hinokura (65–70); daily; and the Móveis do Palácio de Laca (Docs/Housing.md §6.2), the Epic as the week's special ----
            new CityShopSpec(FerroPretoKurogane, ShopTheme.Speciality, BiomeType.Forest, ShopRotationPeriod.Daily, 3, WithHouse(FerroPretoKurogane, new List<CityShopEntry>
            {
                new CityShopEntry("item_health_potion", 15, 6),
                new CityShopEntry("item_mana_potion", 15, 6),
                // Gear of the maps that return to Hinokura, fixed from the first day: the reef and the hot channels (passages,
                // reed boats), the volcano's walls (crampons, rope), the fumaroles (theriac, antivenom amulet), the red bridges of
                // the gardens (balance pole) and the heat of the lava (water skins).
                new CityShopEntry("item_passagem_dhow", 300, 3),
                new CityShopEntry("item_barco_junco", 600, 2),
                new CityShopEntry("item_grampos_escalada", 950, 1),
                new CityShopEntry("item_corda_escalada", 40, 3),
                new CityShopEntry("item_teriaga", 50, 4),
                new CityShopEntry("item_amuleto_antiveneno", 700, 1),
                new CityShopEntry("item_vara_equilibrio", 480, 2),
                new CityShopEntry("item_odre_agua", 30, 5),
                // Cabra-dos-Pastores do Vulcão (Docs/Mounts.md §3.9): the herders of Echeyde sell through the forge.
                new CityShopEntry("item_whistle_mount_volcano_goat", 145, -1),
                // The rotation: what the forges and the boats of the isle bring.
                new CityShopEntry("item_cooked_meat", 6, 10, rotating: true),
                new CityShopEntry("item_iron", 8, 20, rotating: true),
                new CityShopEntry("item_pickaxe", 18, 1, rotating: true),
                new CityShopEntry("item_oleo_fogo_eterno", 190, 2, rotating: true),
                new CityShopEntry("item_runa_couro", 20, 2, rotating: true),
                new CityShopEntry("item_runa_cura", 15, 2, rotating: true),
                new CityShopEntry("item_amuleto_sorte", 25, 1, rotating: true),
                new CityShopEntry("item_pedra_bencao", 12, 3, rotating: true),
            }), houseSlots: 3, weeklySlots: 1),

            // ---- Cais de Mesektet: Khunanup's trades, the gear of the hours of the Rio das Doze Horas (71–76); daily ----
            new CityShopSpec(TrocasKhunanup, ShopTheme.Speciality, BiomeType.Forest, ShopRotationPeriod.Daily, 3, WithHouse(TrocasKhunanup, new List<CityShopEntry>
            {
                new CityShopEntry("item_health_potion", 15, 6),
                new CityShopEntry("item_mana_potion", 15, 6),
                // Gear of the maps that return to the Cais, fixed from the first day: the river and the reed canals (passages,
                // reed boats, root boots), the endless night and Mehen's coils (lamps, eternal oil), the hidden sands and the lake
                // of fire (water skins), the scorpions and snakes (theriac, antivenom amulet) and the sand-sledge (rope).
                new CityShopEntry("item_passagem_dhow", 300, 3),
                new CityShopEntry("item_barco_junco", 600, 2),
                new CityShopEntry("item_botas_raiz", 520, 1),
                new CityShopEntry("item_lamparina_oleo", 28, 3),
                new CityShopEntry("item_lamparina_runica", 155, 2),
                new CityShopEntry("item_oleo_fogo_eterno", 190, 2),
                new CityShopEntry("item_odre_agua", 30, 5),
                new CityShopEntry("item_teriaga", 50, 4),
                new CityShopEntry("item_amuleto_antiveneno", 700, 1),
                new CityShopEntry("item_corda_escalada", 40, 3),
                // The rotation: what the boats of the river bring to the quay.
                new CityShopEntry("item_cooked_meat", 6, 10, rotating: true),
                new CityShopEntry("item_fish_peixe_dourado", 60, 2, rotating: true),
                new CityShopEntry("item_herb", 3, 10, rotating: true),
                new CityShopEntry("item_runa_clareza", 15, 2, rotating: true),
                new CityShopEntry("item_runa_cura", 15, 2, rotating: true),
                new CityShopEntry("item_amuleto_sorte", 25, 1, rotating: true),
                new CityShopEntry("item_pedra_bencao", 12, 3, rotating: true),
                new CityShopEntry("item_pedra_alma", 35, 1, rotating: true),
            }), houseSlots: 3, weeklySlots: 1),

            // ---- Dur-Jarras: Atrahasis' bitumen yard, the gear of the Margens da Mistura (77–82); daily ----
            new CityShopSpec(BetumeAtrahasis, ShopTheme.Speciality, BiomeType.Forest, ShopRotationPeriod.Daily, 3, WithHouse(BetumeAtrahasis, new List<CityShopEntry>
            {
                new CityShopEntry("item_health_potion", 15, 6),
                new CityShopEntry("item_mana_potion", 15, 6),
                // Gear of the maps that return to Dur-Jarras, fixed from the first day: the desert of the clock (water skins), the
                // pitch wells (reed boats, passages, root boots, eternal oil to light the pitch, theriac for the fumes), the climb
                // of the cedars and the shoulder of the giant (rope, crampons) and the lamps.
                new CityShopEntry("item_odre_agua", 30, 5),
                new CityShopEntry("item_barco_junco", 600, 2),
                new CityShopEntry("item_passagem_dhow", 300, 3),
                new CityShopEntry("item_botas_raiz", 520, 1),
                new CityShopEntry("item_oleo_fogo_eterno", 190, 2),
                new CityShopEntry("item_teriaga", 50, 4),
                new CityShopEntry("item_corda_escalada", 40, 3),
                new CityShopEntry("item_grampos_escalada", 950, 1),
                new CityShopEntry("item_lamparina_oleo", 28, 3),
                // Alce da Taiga e Carneiro Gigante da Colina (Docs/Mounts.md §3.9): the rafts bring them down from the cedars and the tablets.
                new CityShopEntry("item_whistle_mount_taiga_elk", 150, -1),
                new CityShopEntry("item_whistle_mount_hill_ram", 150, -1),
                // The rotation: what the rafts bring up the canals.
                new CityShopEntry("item_cooked_meat", 6, 10, rotating: true),
                new CityShopEntry("item_herb", 3, 10, rotating: true),
                new CityShopEntry("item_iron", 8, 20, rotating: true),
                new CityShopEntry("item_runa_clareza", 15, 2, rotating: true),
                new CityShopEntry("item_runa_cura", 15, 2, rotating: true),
                new CityShopEntry("item_amuleto_sorte", 25, 1, rotating: true),
                new CityShopEntry("item_pedra_bencao", 12, 3, rotating: true),
                new CityShopEntry("item_pedra_alma", 35, 1, rotating: true),
            }), houseSlots: 3, weeklySlots: 1),

            // ---- Fortaleza da Última Lâmpada: Tehat's wick workshop, the gear of the Limiar Profundo (83–90); daily ----
            new CityShopSpec(PaviosTehat, ShopTheme.Speciality, BiomeType.Forest, ShopRotationPeriod.Daily, 3, WithHouse(PaviosTehat, new List<CityShopEntry>
            {
                new CityShopEntry("item_health_potion", 15, 6),
                new CityShopEntry("item_mana_potion", 15, 6),
                // Gear of the maps that return to the Fortaleza, fixed from the first day: the smoke (air bottles, theriac), the shaft of the
                // wind (canvas wings, storm cloak), the drowned halls (breathing shell, air bubble), the dark (both lamps), the lamp network
                // (eternal oil), the bridge of the Novelo (balance pole) and rope.
                new CityShopEntry("item_garrafa_ar_sereno", 30, 6),
                new CityShopEntry("item_teriaga", 50, 4),
                new CityShopEntry("item_asas_lona_takhsich", 420, 1),
                new CityShopEntry("item_capa_tempestade", 1100, 1),
                new CityShopEntry("item_concha_respiravel", 450, 2),
                new CityShopEntry("item_bolha_ar", 500, 2),
                new CityShopEntry("item_lamparina_oleo", 28, 3),
                new CityShopEntry("item_lamparina_runica", 155, 1),
                new CityShopEntry("item_oleo_fogo_eterno", 190, 2),
                new CityShopEntry("item_vara_equilibrio", 480, 1),
                new CityShopEntry("item_corda_escalada", 40, 3),
                // The rotation: what the caravans carry down to the last lamp.
                new CityShopEntry("item_cooked_meat", 6, 10, rotating: true),
                new CityShopEntry("item_herb", 3, 10, rotating: true),
                new CityShopEntry("item_iron", 8, 20, rotating: true),
                new CityShopEntry("item_runa_clareza", 15, 2, rotating: true),
                new CityShopEntry("item_runa_cura", 15, 2, rotating: true),
                new CityShopEntry("item_amuleto_sorte", 25, 1, rotating: true),
                new CityShopEntry("item_pedra_bencao", 12, 3, rotating: true),
                new CityShopEntry("item_pedra_alma", 35, 1, rotating: true),
            }), houseSlots: 3, weeklySlots: 1),

            // ---- Cais da Coluna: the last caravan of Ammo (maps 91–100 ask for no gear; what the high road needs); daily ----
            new CityShopSpec(UltimaCaravanaAmmo, ShopTheme.Speciality, BiomeType.Forest, ShopRotationPeriod.Daily, 3, WithHouse(UltimaCaravanaAmmo, new List<CityShopEntry>
            {
                new CityShopEntry("item_health_potion", 15, 6),
                new CityShopEntry("item_mana_potion", 15, 6),
                new CityShopEntry("item_lamparina_runica", 155, 1),
                new CityShopEntry("item_oleo_fogo_eterno", 190, 2),
                new CityShopEntry("item_capa_tempestade", 1100, 1),
                new CityShopEntry("item_asas_lona_takhsich", 420, 1),
                new CityShopEntry("item_corda_escalada", 40, 3),
                new CityShopEntry("item_teriaga", 50, 4),
                // The rotation: what the last caravan carried up.
                new CityShopEntry("item_cooked_meat", 6, 10, rotating: true),
                new CityShopEntry("item_herb", 3, 10, rotating: true),
                new CityShopEntry("item_iron", 8, 20, rotating: true),
                new CityShopEntry("item_runa_clareza", 15, 2, rotating: true),
                new CityShopEntry("item_runa_cura", 15, 2, rotating: true),
                new CityShopEntry("item_amuleto_sorte", 25, 1, rotating: true),
                new CityShopEntry("item_pedra_bencao", 12, 3, rotating: true),
                new CityShopEntry("item_pedra_alma", 35, 1, rotating: true),
            }), houseSlots: 3, weeklySlots: 1),

            // ---- Ferraria do Mestre Adamas (Vila): the village blacksmith ----
            new CityShopSpec(FerrariaSeverino, ShopTheme.Ferreiro, BiomeType.Forest, ShopRotationPeriod.Weekly, 2, new List<CityShopEntry>
            {
                new CityShopEntry("item_iron", 8, 10),
                new CityShopEntry("item_hammer", 15, 1),
                new CityShopEntry("item_pedra_bencao", 12, 3),
                new CityShopEntry("item_axe", 18, 1, rotating: true),
                new CityShopEntry("item_pickaxe", 18, 1, rotating: true),
                new CityShopEntry("item_wooden_sword", 10, 1, rotating: true),
                new CityShopEntry("item_iron_ore", 5, 10, rotating: true),
                new CityShopEntry("item_pedra_alma", 38, 1, rotating: true, unlock: ShopUnlock.Cleared(BiomeType.Desert)),
                // The week's special: Adamas's best blade, for a freguês de casa.
                new CityShopEntry("item_iron_sword", 48, 1, rotating: true, unlock: ShopUnlock.Reputation(ShopReputation.HouseRegular), pool: ShopPool.Weekly),
            }, weeklySlots: 1),

            // ---- Marcenaria do Mestre Omóforo (Vila): house furniture, 10 a day, 1 Epic a week (Housing.md §8.1) ----
            new CityShopSpec(MarcenariaDamiao, ShopTheme.Marcenaria, BiomeType.Forest, ShopRotationPeriod.Daily, 1, WithHouse(MarcenariaDamiao, new List<CityShopEntry>
            {
                new CityShopEntry("item_wood", 2, 30),
                new CityShopEntry("item_hammer", 15, 1, rotating: true),
                new CityShopEntry("item_axe", 18, 1, rotating: true),
            }), houseSlots: 10, weeklySlots: 1),

            // ---- Costureira Mestra Narisah (Vila): 6 looks + 6 cloth house items a day (Housing.md §8.1); the Móveis do Tear de Seda (§6.2) with the Epic as the week's special ----
            new CityShopSpec(CostureiraFatima, ShopTheme.Costureira, BiomeType.Forest, ShopRotationPeriod.Daily, 6, WithHouse(CostureiraFatima, new List<CityShopEntry>
            {
                new CityShopEntry("item_fiber", 1, 30),
                new CityShopEntry("item_leather_helmet", 12, 2, rotating: true),
                new CityShopEntry("item_leather_boots", 10, 2, rotating: true),
                // Looks of Docs/Housing/CatalogoRoupas.md ("Loja: Costureira Mestra Narisah"): placeholders until the
                // wardrobe exposes them (CityShopService.OutfitResolver); until then they never take a slot.
                Outfit("roupa_maithe_001", 21, 5),
                Outfit("roupa_maithe_002", 23, 5),
                Outfit("roupa_maithe_003", 15, 5),
                Outfit("roupa_maithe_005", 30, 3, ShopUnlock.Cleared(BiomeType.Forest)),
                Outfit("roupa_maithe_007", 70, 3, ShopUnlock.Cleared(BiomeType.Forest)),
                Outfit("roupa_maithe_009", 140, 1, ShopUnlock.Cleared(BiomeType.Desert)),
                Outfit("roupa_maithe_011", 120, 1, ShopUnlock.Cleared(BiomeType.Desert)),
                Outfit("roupa_maria_001", 11, 5),
                Outfit("roupa_maria_002", 24, 5),
                Outfit("roupa_maria_003", 24, 5),
                Outfit("roupa_maria_005", 40, 3, ShopUnlock.Cleared(BiomeType.Forest)),
                Outfit("roupa_maria_007", 60, 3, ShopUnlock.Cleared(BiomeType.Forest)),
                Outfit("roupa_maria_009", 145, 1, ShopUnlock.Cleared(BiomeType.Desert)),
                Outfit("roupa_maria_011", 90, 1, ShopUnlock.Cleared(BiomeType.Desert)),
                Outfit("roupa_joaquim_001", 11, 5),
                Outfit("roupa_joaquim_002", 19, 5),
                Outfit("roupa_joaquim_003", 22, 5),
                Outfit("roupa_icaro_001", 24, 5),
                Outfit("roupa_icaro_002", 20, 5),
                Outfit("roupa_icaro_003", 25, 5),
                Outfit("roupa_lavine_001", 19, 5),
                Outfit("roupa_lavine_002", 11, 5),
                Outfit("roupa_lavine_003", 23, 5),
                Outfit("roupa_lavine_005", 55, 3, ShopUnlock.Cleared(BiomeType.Forest)),
                Outfit("roupa_lavine_007", 65, 3, ShopUnlock.Cleared(BiomeType.Forest)),
                Outfit("roupa_lavine_009", 90, 1, ShopUnlock.Cleared(BiomeType.Desert)),
                Outfit("roupa_lavine_011", 150, 1, ShopUnlock.Cleared(BiomeType.Desert)),
                Outfit("roupa_comum_001", 24, 5),
                Outfit("roupa_comum_002", 11, 5),
                Outfit("roupa_comum_005", 45, 3, ShopUnlock.Cleared(BiomeType.Forest)),
                Outfit("roupa_comum_006", 40, 3, ShopUnlock.Cleared(BiomeType.Forest)),
                Outfit("roupa_comum_008", 90, 1, ShopUnlock.Cleared(BiomeType.Desert)),
                Outfit("roupa_comum_011", 60, 3, ShopUnlock.Cleared(BiomeType.Forest)),
                Outfit("roupa_comum_016", 20, 5),
                Outfit("roupa_comum_017", 22, 5),
                Outfit("acess_018", 23, 5),
                Outfit("acess_024", 55, 3, ShopUnlock.Cleared(BiomeType.Forest)),
                Outfit("acess_035", 55, 3, ShopUnlock.Cleared(BiomeType.Forest)),
            }), houseSlots: 6, weeklySlots: 1),

            // ---- Estrebaria do Srosh (Srosh): the starter mounts' whistles (Docs/Mounts.md §3.1, phase M1) ----
            new CityShopSpec(HarasVila, ShopTheme.Speciality, BiomeType.Forest, ShopRotationPeriod.Weekly, 0, new List<CityShopEntry>
            {
                new CityShopEntry("item_whistle_mount_pony", 40, -1),
                new CityShopEntry("item_whistle_mount_big_dog", 60, -1),
                new CityShopEntry("item_whistle_mount_pack_donkey", 65, -1),
                // M3 wave 1 (Docs/Mounts.md §3.1-3.2): the Capivara, and the Cavalo Pampa once the Forest is cleared ("nível 10").
                new CityShopEntry("item_whistle_mount_capybara", 55, -1),
                new CityShopEntry("item_whistle_mount_pinto_horse", 150, -1, unlock: ShopUnlock.Cleared(BiomeType.Forest)),
                new CityShopEntry("item_mount_feed", 3, -1),
            }),

            // ---- Mascate Ammo (one city a day) ----
            new CityShopSpec(MascateBento, ShopTheme.Mascate, null, ShopRotationPeriod.Daily, 3, WithHouse(MascateBento, new List<CityShopEntry>
            {
                new CityShopEntry("item_health_potion", 18, 3, rotating: true),
                new CityShopEntry("item_mana_potion", 18, 3, rotating: true),
                new CityShopEntry("item_cooked_meat", 7, 5, rotating: true),
                new CityShopEntry("item_leather_chest", 24, 1, rotating: true),
                new CityShopEntry("item_hammer", 18, 1, rotating: true),
                new CityShopEntry("item_hunting_bow", 42, 1, rotating: true, unlock: ShopUnlock.Cleared(BiomeType.Forest)),
                new CityShopEntry("item_iron_sword", 54, 1, rotating: true, unlock: ShopUnlock.Cleared(BiomeType.Desert)),
                new CityShopEntry("item_pedra_bencao", 14, 3, rotating: true, unlock: ShopUnlock.Unlocked(BiomeType.Desert)),
                new CityShopEntry("item_amuleto_sorte", 30, 1, rotating: true, unlock: ShopUnlock.Cleared(BiomeType.Desert)),
                // Event room entries (EventRoomRules): also a rare drop.
                new CityShopEntry("item_convite_terreiro", 36, 1, rotating: true, unlock: ShopUnlock.Cleared(BiomeType.Forest)),
                new CityShopEntry("item_chave_engenho", 54, 1, rotating: true, unlock: ShopUnlock.Cleared(BiomeType.Desert)),
                // Patuá da Vó Edna (DeathPenaltyService): pricey on purpose; also Vó Edna's gift for each region boss.
                new CityShopEntry("item_patua_vo_zefa", 120, 1, rotating: true, unlock: ShopUnlock.Cleared(BiomeType.Forest)),
                // Alforje de Feltro: dearer than at Mariam's, but no freguesia needed.
                new CityShopEntry(BornalItemId, 180, 1, rotating: true, unlock: ShopUnlock.Cleared(BiomeType.Forest)),
            }), houseSlots: 2, weeklySlots: 1),
        };

        public static CityShopSpec Get(string shopId) => Shops.FirstOrDefault(s => s.ShopId == shopId);

        /// <summary>
        /// <paramref name="entries"/> plus every <c>HouseCatalog</c> item whose shop source is <paramref name="shopId"/>
        /// (CatalogoCasa.md "Loja: ..."), as rotating <see cref="ShopPool.House"/> lines at the catalog price. Gates by
        /// rarity (Housing.md §8.1): Common from day one, Uncommon after the Forest boss, Rare after the Desert's, Epic
        /// after the Mountain's and only as the week's special; Legendary and priceless items are never sold. Stock:
        /// Common 5, Uncommon 3, Rare/Epic 1.
        /// </summary>
        public static List<CityShopEntry> WithHouse(string shopId, List<CityShopEntry> entries)
        {
            var result = new List<CityShopEntry>(entries ?? new List<CityShopEntry>());
            foreach (HouseItemDefinition item in HouseCatalog.All)
            {
                if (item == null || item.BasePrice <= 0 || item.Rarity == ItemRarity.Legendary || !IsSoldBy(item, shopId))
                {
                    continue;
                }

                result.Add(HouseEntry(item));
            }

            return result;
        }

        /// <summary>The shop line of a house item (see <see cref="WithHouse"/>).</summary>
        public static CityShopEntry HouseEntry(HouseItemDefinition item)
        {
            switch (item.Rarity)
            {
                case ItemRarity.Common:
                    return new CityShopEntry(item.ItemId, item.BasePrice, 5, rotating: true, kind: CityShopEntryKind.HouseItem);
                case ItemRarity.Uncommon:
                    return new CityShopEntry(item.ItemId, item.BasePrice, 3, true, ShopUnlock.Cleared(BiomeType.Forest), CityShopEntryKind.HouseItem);
                case ItemRarity.Rare:
                    return new CityShopEntry(item.ItemId, item.BasePrice, 1, true, ShopUnlock.Cleared(BiomeType.Desert), CityShopEntryKind.HouseItem);
                default:
                    return new CityShopEntry(item.ItemId, item.BasePrice, 1, true, ShopUnlock.Cleared(BiomeType.Mountain), CityShopEntryKind.HouseItem, ShopPool.Weekly);
            }
        }

        private static bool IsSoldBy(HouseItemDefinition item, string shopId)
        {
            if (item.Sources == null)
            {
                return false;
            }

            foreach (HouseItemSource source in item.Sources)
            {
                if (source != null && source.Type == HouseItemSourceType.Shop && source.RefId == shopId)
                {
                    return true;
                }
            }

            return false;
        }

        private static CityShopEntry Outfit(string outfitId, int price, int stock, ShopUnlock unlock = default)
        {
            return new CityShopEntry(outfitId, price, stock, true, unlock, CityShopEntryKind.Outfit);
        }

        /// <summary>The line of <paramref name="itemId"/> in <paramref name="shopId"/>, or null.</summary>
        public static CityShopEntry EntryOf(string shopId, string itemId) => Get(shopId)?.Entries.FirstOrDefault(e => e.ItemId == itemId);

        /// <summary>Whether any themed shop sells <paramref name="itemId"/> as a house item.</summary>
        public static bool IsHouseEntry(string itemId) =>
            !string.IsNullOrEmpty(itemId) && Shops.Any(s => s.Entries.Any(e => e.Kind == CityShopEntryKind.HouseItem && e.ItemId == itemId));

        /// <summary>Whether <paramref name="shopId"/> is an authored <c>ShopDefinition</c> asset rather than a themed shop.</summary>
        public static bool IsAssetShop(string shopId) => !string.IsNullOrEmpty(shopId) && Get(shopId) == null;

        /// <summary>Name key of any hub shop (themed or the legacy asset's key).</summary>
        public static string NameKeyOf(string shopId)
        {
            CityShopSpec spec = Get(shopId);
            return spec != null ? spec.NameKey : shopId == GeneralStore ? "shop.general_store.name" : shopId;
        }
    }
}
