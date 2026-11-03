using UnityEngine;

namespace Rapadura.Gameplay.World.Map
{
    /// <summary>
    /// Optional designer-facing ScriptableObject for one map (the fields of TODO.md "MapDefinition"). The source of truth
    /// stays the plain catalog (<see cref="MapCatalog"/>, server-runnable, like <c>CityCatalog</c>); an asset only
    /// overrides the numbers of the map with the same <see cref="MapId"/> when converted with <see cref="ToDefinition"/>,
    /// so balancing can be tried in the Inspector without touching code. No asset ships today.
    /// </summary>
    [CreateAssetMenu(fileName = "MapDefinition", menuName = "Rapadura/World/Map Definition")]
    public sealed class MapDefinitionAsset : ScriptableObject
    {
        [SerializeField] [Min(1)] private int _mapId = 1;
        [Tooltip("Localization key of the name; empty = the catalog's map.<id>.name.")]
        [SerializeField] private string _mapNameKey;
        [SerializeField] [Min(1)] private int _walkLevelRequirement = 1;
        [SerializeField] [Min(1)] private int _warpLevelRequirement = 1;
        [SerializeField] [Min(0)] private int _baseWarpGoldFee = MapAccessRules.BaseWarpFee;
        [Tooltip("MapRequirementTags value (WingsRequired, TorchRequired, SaddleRequired...); empty = none.")]
        [SerializeField] private string _specialRequirementTag;
        [Tooltip("CityCatalog id of the cidade de retorno.")]
        [SerializeField] private string _returnCityId;

        public int MapId => _mapId;
        public string MapNameKey => _mapNameKey;
        public int WalkLevelRequirement => _walkLevelRequirement;
        public int WarpLevelRequirement => _warpLevelRequirement;
        public int BaseWarpGoldFee => _baseWarpGoldFee;
        public string SpecialRequirementTag => _specialRequirementTag;
        public string ReturnCityId => _returnCityId;

        /// <summary>The catalog entry of <see cref="MapId"/> with this asset's numbers on top (null for an unknown map).</summary>
        public MapDefinition ToDefinition()
        {
            MapDefinition baseMap = MapCatalog.Get(_mapId);
            if (baseMap == null)
            {
                return null;
            }

            return new MapDefinition(baseMap.MapId, baseMap.Id, baseMap.Region, baseMap.RegionType, baseMap.Size, baseMap.MonsterLevelMin,
                baseMap.MonsterLevelMax, _walkLevelRequirement, _warpLevelRequirement, _baseWarpGoldFee, _specialRequirementTag,
                string.IsNullOrEmpty(_returnCityId) ? baseMap.ReturnCityId : _returnCityId, baseMap.Settlement, baseMap.SettlementId, baseMap.IsBuilt);
        }

        /// <summary>Copies a catalog entry into this asset (editor seeding).</summary>
        public void CopyFrom(MapDefinition map)
        {
            if (map == null)
            {
                return;
            }

            _mapId = map.MapId;
            _mapNameKey = map.MapNameKey;
            _walkLevelRequirement = map.WalkLevelRequirement;
            _warpLevelRequirement = map.WarpLevelRequirement;
            _baseWarpGoldFee = map.BaseWarpGoldFee;
            _specialRequirementTag = map.SpecialRequirementTag;
            _returnCityId = map.ReturnCityId;
        }
    }
}
