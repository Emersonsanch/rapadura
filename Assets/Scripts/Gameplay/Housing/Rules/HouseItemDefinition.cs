using System;
using System.Collections.Generic;
using Rapadura.Gameplay.Items;

namespace Rapadura.Gameplay.Housing
{
    /// <summary>One origin of a house item (Housing.md §7): shop, enemy drop, quest, achievement, event, craft or secret.</summary>
    [Serializable]
    public sealed class HouseItemSource
    {
        public HouseItemSourceType Type;
        /// <summary>Shop id, enemy id, quest id, achievement id, event id, crafting station or secret id.</summary>
        public string RefId;
        /// <summary>Weight in the source's draw (drops: 3 for an enemy's own items, Housing.md §7.2).</summary>
        public int Weight;
        /// <summary>The catalog's "Origem" text as written (pt-BR), for descriptions and tooltips.</summary>
        public string Label;

        public HouseItemSource()
        {
        }

        public HouseItemSource(HouseItemSourceType type, string refId, int weight = 1, string label = null)
        {
            Type = type;
            RefId = refId;
            Weight = weight;
            Label = label ?? string.Empty;
        }
    }

    /// <summary>
    /// A house item of the catalog (Housing.md §12.1). Deviation from the doc: a plain serializable class held by
    /// one generated table (<see cref="HouseCatalog"/>, <c>HouseCatalogData.g.cs</c>), not 580 ScriptableObject
    /// assets: it diffs well, needs no asset import to exist (tests and localization see it right away) and,
    /// being Unity-free, the online server can compile the same catalog. Footprints are plain ints for the same
    /// reason; <c>HouseItemDefinitionExtensions</c> (Unity side) exposes them as <c>Vector2Int</c>.
    /// </summary>
    [Serializable]
    public sealed class HouseItemDefinition
    {
        public const string KeyPrefix = "house.";

        public string _itemId;
        /// <summary>pt-BR name from the catalog (the localization fallback; <see cref="NameKey"/> is what the UI uses).</summary>
        public string _namePtBR;
        public HouseItemCategory _category;
        public ItemRarity _rarity;
        public HousePlacement _placement;
        /// <summary>Footprint in cells of the layer's grid (wall: width × rows).</summary>
        public int _footprintX = 1;
        public int _footprintY = 1;
        /// <summary>0 (does not turn; walls mirror instead), 45 or 90 degrees.</summary>
        public int _rotationStep;
        /// <summary>Surface spots this piece offers on top (table 2×1 = 4, shelf = 3...).</summary>
        public int _surfaceSlots;
        public HouseItemFunction _function;
        /// <summary>Function parameter, e.g. the extra chest slots of an extra chest.</summary>
        public int _functionParam;
        public int _comfort;
        public string _setId = string.Empty;
        /// <summary>Gold in a shop; 0 = not sold in shops.</summary>
        public int _basePrice;
        /// <summary>Event currency ("prendas").</summary>
        public int _eventPrice;
        public bool _sellable;
        public bool _tradeable;
        public bool _accountBound;
        public int _minHouseLevel = 1;
        public HouseItemSource[] _sources = Array.Empty<HouseItemSource>();
        public HouseSeason _seasonal;
        public string _prefabKey = string.Empty;

        public string ItemId => _itemId;
        public string NamePtBR => _namePtBR;
        public string NameKey => KeyPrefix + _itemId + ".name";
        public string DescKey => KeyPrefix + _itemId + ".desc";
        public HouseItemCategory Category => _category;
        public ItemRarity Rarity => _rarity;
        public HousePlacement Placement => _placement;
        public int FootprintX => _footprintX;
        public int FootprintY => _footprintY;
        public int RotationStep => _rotationStep;
        public int SurfaceSlots => _surfaceSlots;
        public HouseItemFunction Function => _function;
        public int FunctionParam => _functionParam;
        public int Comfort => _comfort;
        public string SetId => _setId;
        public int BasePrice => _basePrice;
        public int EventPrice => _eventPrice;
        public bool Sellable => _sellable;
        public bool Tradeable => _tradeable;
        public bool AccountBound => _accountBound;
        public int MinHouseLevel => _minHouseLevel;
        public IReadOnlyList<HouseItemSource> Sources => _sources;
        public HouseSeason Seasonal => _seasonal;
        public string PrefabKey => _prefabKey;

        public HouseItemSource PrimarySource => _sources != null && _sources.Length > 0 ? _sources[0] : null;

        public bool Allows(HouseLayer layer) => (_placement & HousePlacementRules.FlagOf(layer)) != 0;

        public HouseItemDefinition()
        {
        }

        /// <summary>Positional constructor used by the generated table (one line per item).</summary>
        public HouseItemDefinition(string itemId, string namePtBR, HouseItemCategory category, ItemRarity rarity, HousePlacement placement,
            int footprintX, int footprintY, int rotationStep, int surfaceSlots, HouseItemFunction function, int functionParam, int comfort,
            string setId, int basePrice, int eventPrice, bool sellable, bool tradeable, bool accountBound, int minHouseLevel,
            HouseSeason seasonal, params HouseItemSource[] sources)
        {
            _itemId = itemId;
            _namePtBR = namePtBR;
            _category = category;
            _rarity = rarity;
            _placement = placement;
            _footprintX = Math.Max(1, footprintX);
            _footprintY = Math.Max(1, footprintY);
            _rotationStep = rotationStep;
            _surfaceSlots = surfaceSlots;
            _function = function;
            _functionParam = functionParam;
            _comfort = comfort;
            _setId = setId ?? string.Empty;
            _basePrice = basePrice;
            _eventPrice = eventPrice;
            _sellable = sellable;
            _tradeable = tradeable;
            _accountBound = accountBound;
            _minHouseLevel = Math.Max(1, minHouseLevel);
            _seasonal = seasonal;
            _sources = sources ?? Array.Empty<HouseItemSource>();
            _prefabKey = itemId;
        }
    }
}
