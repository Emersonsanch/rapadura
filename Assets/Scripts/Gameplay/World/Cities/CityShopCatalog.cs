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

            // ---- Oásis de Qocho (Deserto): couro de vaqueiro ----
            new CityShopSpec(SelariaVava, ShopTheme.Speciality, BiomeType.Desert, ShopRotationPeriod.Weekly, 2, new List<CityShopEntry>
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
            }),

            // ---- Vila de Litema (Montanha): ferreiro ----
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
                // Pedras do Sertão (refino, Docs/Balance/README.md "Refino de item"): the blacksmith sells them ~10% cheaper.
                new CityShopEntry("item_pedra_bencao", 11, 5),
                new CityShopEntry("item_pedra_alma", 32, 2, unlock: ShopUnlock.Cleared(BiomeType.Mountain)),
                // Bode Saltador (Docs/Mounts.md §3.1, M3 wave 1): the Curral do Pico sells through the forge until it has a stall.
                new CityShopEntry("item_whistle_mount_mountain_goat", 70, -1),
            }), houseSlots: 2),

            // ---- Vila das Lamparinas (Cavernas): lamparinas e botica ----
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
            }), houseSlots: 2),

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

            // ---- Costureira Mestra Narisah (Vila): 6 looks + 6 cloth house items a day (Housing.md §8.1) ----
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
            }), houseSlots: 6),

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
