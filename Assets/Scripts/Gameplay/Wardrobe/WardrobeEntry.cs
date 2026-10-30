using System;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Items;

// Unity-free on purpose (no UnityEngine): the online server compiles the same catalog and rules (Housing.md §12.3).
namespace Rapadura.Gameplay.Wardrobe
{
    /// <summary>What an entry of <c>Docs/Housing/CatalogoRoupas.md</c> is.</summary>
    public enum WardrobeEntryKind
    {
        /// <summary>Clothing (cousin sets and the common clothes): a look for a real equipment slot.</summary>
        Piece,
        /// <summary>Hats, face/neck, back, accessory and Caramelo's outfits.</summary>
        Accessory,
        /// <summary>"Tintura": a consumable that paints one channel of one piece.</summary>
        Dye,
        /// <summary>"Visual de arma": appearance over the cousin's weapon.</summary>
        WeaponSkin,
    }

    /// <summary>
    /// Appearance layers (Housing.md §9.1). The first five mirror <c>EquipmentSlot</c> (Head, Chest, Legs, Boots,
    /// Accessory); FaceNeck, Back and Caramelo are appearance-only; Weapon carries the weapon skins.
    /// </summary>
    public enum AppearanceSlot
    {
        Head,
        Chest,
        Legs,
        Boots,
        Accessory,
        FaceNeck,
        Back,
        Caramelo,
        Weapon,
    }

    /// <summary>The catalog's "Origem", parsed (§9.7). <see cref="WardrobeEntry.SourceRef"/> holds the id it points to.</summary>
    public enum WardrobeSourceType
    {
        Starter,
        Shop,
        /// <summary>Enemy or boss id; "common_forest_desert" = any common enemy of the Floresta or the Deserto.</summary>
        Drop,
        Quest,
        HouseQuest,
        DailyQuest,
        Achievement,
        Event,
        /// <summary>Station id: workbench, forge, stove (house recipes) or loom / master_loom / forge_boss (not craftable yet).</summary>
        Craft,
        /// <summary>Relic id.</summary>
        Secret,
    }

    /// <summary>
    /// One wardrobe entry (Housing.md §12.1 <c>AppearanceDefinition</c> / <c>DyeDefinition</c>), held by the generated table
    /// <c>WardrobeCatalogData.g.cs</c> (imported from <c>CatalogoRoupas.md</c>, like the house catalog). Plain class, no
    /// ScriptableObject: it diffs well and the server can compile it.
    /// </summary>
    [Serializable]
    public sealed class WardrobeEntry
    {
        public const string KeyPrefix = "wardrobe.";
        public const string CommonDropRef = "common_forest_desert";

        public string Id;
        public string NamePtBR;
        public WardrobeEntryKind Kind;
        public AppearanceSlot Slot;
        /// <summary>The cousin the piece is modelled for (cousin sets, weapon skins); null = every cousin (or Caramelo).</summary>
        public CharacterId? Owner;
        public ItemRarity Rarity;
        /// <summary>Set name (pt-BR) and its id ("set_batedora_mata"); empty for loose pieces.</summary>
        public string SetName;
        public string SetId;
        /// <summary>Level band of the piece (0/0 = any level, festive and loose pieces).</summary>
        public int LevelMin;
        public int LevelMax;
        public WardrobeSourceType Source;
        public string SourceRef;
        public string SourceLabel;
        public int Price;
        public int EventPrice;
        /// <summary>Dyes only: the colour name of the catalog (pt-BR) and its RGB as "RRGGBB".</summary>
        public string ColorName;
        public string ColorHex;

        public WardrobeEntry()
        {
        }

        /// <summary>Positional constructor used by the generated table.</summary>
        public WardrobeEntry(string id, string namePtBR, WardrobeEntryKind kind, AppearanceSlot slot, CharacterId? owner, ItemRarity rarity,
            string setName, string setId, int levelMin, int levelMax, WardrobeSourceType source, string sourceRef, string sourceLabel,
            int price, int eventPrice, string colorName, string colorHex)
        {
            Id = id;
            NamePtBR = namePtBR;
            Kind = kind;
            Slot = slot;
            Owner = owner;
            Rarity = rarity;
            SetName = setName ?? string.Empty;
            SetId = setId ?? string.Empty;
            LevelMin = levelMin;
            LevelMax = levelMax;
            Source = source;
            SourceRef = sourceRef ?? string.Empty;
            SourceLabel = sourceLabel ?? string.Empty;
            Price = price;
            EventPrice = eventPrice;
            ColorName = colorName ?? string.Empty;
            ColorHex = colorHex ?? string.Empty;
        }

        public string NameKey => KeyPrefix + Id + ".name";
        public string DescKey => KeyPrefix + Id + ".desc";
        public bool IsDye => Kind == WardrobeEntryKind.Dye;
        public bool IsLook => Kind != WardrobeEntryKind.Dye;
        /// <summary>Festive sets are pure cosmetics (no stats, §9.2).</summary>
        public bool IsFestive => Source == WardrobeSourceType.Event && Kind == WardrobeEntryKind.Piece && Owner.HasValue;

        /// <summary>Can <paramref name="cousin"/> wear it? Cousin sets and weapon skins only fit their owner (§9.3).</summary>
        public bool FitsCousin(CharacterId cousin) => !Owner.HasValue || Owner.Value == cousin;
    }
}
