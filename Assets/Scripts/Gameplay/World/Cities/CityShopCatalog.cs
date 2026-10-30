using System.Collections.Generic;
using System.Linq;
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
    }

    /// <summary>Campaign gate of a shop entry.</summary>
    public readonly struct ShopUnlock
    {
        public readonly ShopUnlockKind Kind;
        public readonly BiomeType Region;

        public ShopUnlock(ShopUnlockKind kind, BiomeType region)
        {
            Kind = kind;
            Region = region;
        }

        public static ShopUnlock None => default;
        public static ShopUnlock Unlocked(BiomeType region) => new ShopUnlock(ShopUnlockKind.RegionUnlocked, region);
        public static ShopUnlock Cleared(BiomeType region) => new ShopUnlock(ShopUnlockKind.RegionCleared, region);

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

        public CityShopEntry(string itemId, int price, int stock, bool rotating = false, ShopUnlock unlock = default)
        {
            ItemId = itemId;
            Price = price;
            Stock = stock;
            Rotating = rotating;
            Unlock = unlock;
        }
    }

    /// <summary>A themed shop with its keeper (<see cref="CityResidentSpec.ShopId"/>), home region (null = roaming) and stock.</summary>
    public sealed class CityShopSpec
    {
        public readonly string ShopId;
        public readonly ShopTheme Theme;
        public readonly BiomeType? HomeRegion;
        public readonly ShopRotationPeriod Period;

        /// <summary>How many rotating entries are on the counter each period (fewer if fewer are unlocked).</summary>
        public readonly int RotatingSlots;
        public readonly IReadOnlyList<CityShopEntry> Entries;
        public readonly float SellBackRate;

        public CityShopSpec(string shopId, ShopTheme theme, BiomeType? homeRegion, ShopRotationPeriod period, int rotatingSlots,
            IReadOnlyList<CityShopEntry> entries, float sellBackRate = 0.5f)
        {
            ShopId = shopId;
            Theme = theme;
            HomeRegion = homeRegion;
            Period = period;
            RotatingSlots = rotatingSlots;
            Entries = entries;
            SellBackRate = sellBackRate;
        }

        public bool IsRoaming => !HomeRegion.HasValue;
        public string NameKey => "city.shop." + ShopId + ".name";
    }

    /// <summary>
    /// The themed shops of the cities (FASE 15 "Lojas"). Only existing items (<c>Resources/Items</c>); prices stay within
    /// 80–125% of each item's <c>ItemDefinition.Value</c> (Docs/Balance/README.md, "Cidades"): the speciality of a city sells
    /// its theme ~10% cheaper, the peddler ~20% dearer. Names follow Docs/Housing/CatalogoCasa.md (Armarinho da Dona
    /// Cida, Mercearia do Seu Lázaro, Feira livre, Forja do Mestre Tonho, Lamparinas da Benedita, Mascate Seu Bento).
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

        /// <summary>"Haras da Vila" (Seu Zé, Docs/Mounts.md §3): sells the mount whistles; buying one redeems it (<c>MountCollection</c>).</summary>
        public const string HarasVila = "shop_haras_vila";

        /// <summary>Asset path (under Resources) of shops that are not in <see cref="Shops"/>.</summary>
        public const string AssetFolder = "Shops/";

        public static readonly IReadOnlyList<CityShopSpec> Shops = new List<CityShopSpec>
        {
            // ---- Vila da Vó Zefa (Floresta) ----
            new CityShopSpec(MerceariaLazaro, ShopTheme.Mercearia, BiomeType.Forest, ShopRotationPeriod.Daily, 2, new List<CityShopEntry>
            {
                new CityShopEntry("item_cooked_meat", 6, 10),
                new CityShopEntry("item_health_potion", 15, 5),
                new CityShopEntry("item_herb", 3, 15, rotating: true),
                new CityShopEntry("item_wood", 2, 20, rotating: true),
                new CityShopEntry("item_fiber", 1, 20, rotating: true),
                new CityShopEntry("item_mana_potion", 16, 3, rotating: true, unlock: ShopUnlock.Unlocked(BiomeType.Desert)),
            }),
            new CityShopSpec(ArmarinhoCida, ShopTheme.Armarinho, BiomeType.Forest, ShopRotationPeriod.Weekly, 2, new List<CityShopEntry>
            {
                new CityShopEntry("item_fiber", 1, 30),
                new CityShopEntry("item_leather_boots", 10, 2),
                new CityShopEntry("item_leather_helmet", 12, 2, rotating: true),
                new CityShopEntry("item_leather_chest", 20, 1, rotating: true),
                new CityShopEntry("item_herb", 3, 10, rotating: true),
                new CityShopEntry("item_hunting_bow", 38, 1, rotating: true, unlock: ShopUnlock.Cleared(BiomeType.Forest)),
            }),
            new CityShopSpec(FeiraLivre, ShopTheme.Feira, BiomeType.Forest, ShopRotationPeriod.Daily, 3, new List<CityShopEntry>
            {
                new CityShopEntry("item_herb", 3, 20),
                new CityShopEntry("item_wood", 2, 30, rotating: true),
                new CityShopEntry("item_fiber", 1, 30, rotating: true),
                new CityShopEntry("item_stone", 1, 30, rotating: true),
                new CityShopEntry("item_cooked_meat", 5, 8, rotating: true),
                new CityShopEntry("item_iron_ore", 6, 5, rotating: true, unlock: ShopUnlock.Unlocked(BiomeType.Mountain)),
            }),

            // ---- Povoado de Areia Branca (Deserto): couro de vaqueiro ----
            new CityShopSpec(SelariaVava, ShopTheme.Speciality, BiomeType.Desert, ShopRotationPeriod.Weekly, 2, new List<CityShopEntry>
            {
                new CityShopEntry("item_leather_boots", 9, 3),
                new CityShopEntry("item_leather_helmet", 11, 2),
                new CityShopEntry("item_leather_chest", 18, 2),
                new CityShopEntry("item_health_potion", 16, 3, rotating: true),
                new CityShopEntry("item_cooked_meat", 7, 6, rotating: true),
                new CityShopEntry("item_fiber", 1, 20, rotating: true),
                new CityShopEntry("item_hunting_bow", 33, 1, rotating: true, unlock: ShopUnlock.Cleared(BiomeType.Desert)),
            }),

            // ---- Vila da Serra Alta (Montanha): ferreiro ----
            new CityShopSpec(ForjaTonho, ShopTheme.Ferreiro, BiomeType.Mountain, ShopRotationPeriod.Weekly, 2, new List<CityShopEntry>
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
            }),

            // ---- Vila das Lamparinas (Cavernas): lamparinas e botica ----
            new CityShopSpec(LamparinasBenedita, ShopTheme.Speciality, BiomeType.Caves, ShopRotationPeriod.Daily, 2, new List<CityShopEntry>
            {
                new CityShopEntry("item_health_potion", 15, 5),
                new CityShopEntry("item_mana_potion", 13, 5),
                new CityShopEntry("item_herb", 3, 15, rotating: true),
                new CityShopEntry("item_iron_ore", 6, 8, rotating: true),
                new CityShopEntry("item_stone", 1, 20, rotating: true),
                new CityShopEntry("item_leather_helmet", 12, 1, rotating: true),
                new CityShopEntry("item_iron_sword", 45, 1, rotating: true, unlock: ShopUnlock.Cleared(BiomeType.Caves)),
                new CityShopEntry("item_amuleto_sorte", 25, 1, rotating: true),
            }),

            // ---- Haras da Vila (Seu Zé): the starter mounts' whistles (Docs/Mounts.md §3.1, phase M1) ----
            new CityShopSpec(HarasVila, ShopTheme.Speciality, BiomeType.Forest, ShopRotationPeriod.Weekly, 0, new List<CityShopEntry>
            {
                new CityShopEntry("item_whistle_mount_pony", 40, -1),
                new CityShopEntry("item_whistle_mount_big_dog", 60, -1),
                new CityShopEntry("item_whistle_mount_pack_donkey", 65, -1),
                new CityShopEntry("item_mount_feed", 3, -1),
            }),

            // ---- Mascate Seu Bento (one city a day) ----
            new CityShopSpec(MascateBento, ShopTheme.Mascate, null, ShopRotationPeriod.Daily, 3, new List<CityShopEntry>
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
                // Patuá da Vó Zefa (DeathPenaltyService): pricey on purpose; also Vó Zefa's gift for each region boss.
                new CityShopEntry("item_patua_vo_zefa", 120, 1, rotating: true, unlock: ShopUnlock.Cleared(BiomeType.Forest)),
            }),
        };

        public static CityShopSpec Get(string shopId) => Shops.FirstOrDefault(s => s.ShopId == shopId);

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
