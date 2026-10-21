using Rapadura.Gameplay.World.Biomes;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map
{
    /// <summary>
    /// One clickable region on the world map (one per <see cref="BiomeType"/>). Authored in
    /// WorldMap.unity: its position is where the player token stands while in that region, its
    /// <see cref="Neighbours"/> define travel connections and its unlock fields feed
    /// <see cref="WorldMapGraph.SetUnlockRule"/>. Needs a Collider for click selection.
    /// </summary>
    [DisallowMultipleComponent]
    public class WorldMapRegionNode : MonoBehaviour
    {
        [SerializeField] private BiomeType _region = BiomeType.Forest;
        [Tooltip("Regions directly reachable from this one (connections are made bidirectional automatically).")]
        [SerializeField] private WorldMapRegionNode[] _neighbours = new WorldMapRegionNode[0];

        [Header("Unlock")]
        [Tooltip("If false, this region never auto-unlocks (starting region, or unlocked by a quest/script).")]
        [SerializeField] private bool _hasUnlockRule = true;
        [SerializeField] private BiomeType _unlockRequiresVictoriesIn = BiomeType.Forest;
        [SerializeField] [Min(0)] private int _victoriesRequired = WorldMapGraph.DefaultVictoriesToUnlock;

        [Header("Presentation (optional)")]
        [Tooltip("Shown while the region is locked (e.g. a padlock/fog object).")]
        [SerializeField] private GameObject _lockedVisual;
        [Tooltip("Shown while the player token is standing on this region.")]
        [SerializeField] private GameObject _currentVisual;

        public BiomeType Region => _region;
        public WorldMapRegionNode[] Neighbours => _neighbours;
        public bool HasUnlockRule => _hasUnlockRule;
        public BiomeType UnlockRequiresVictoriesIn => _unlockRequiresVictoriesIn;
        public int VictoriesRequired => _victoriesRequired;
        public Vector3 Position => transform.position;

        public void RefreshVisuals(bool isUnlocked, bool isCurrent)
        {
            if (_lockedVisual != null)
            {
                _lockedVisual.SetActive(!isUnlocked);
            }

            if (_currentVisual != null)
            {
                _currentVisual.SetActive(isCurrent);
            }
        }

#if UNITY_EDITOR
        /// <summary>Editor/test-only setter, mirroring NpcInteractable.SetInteractionSourceForTests.</summary>
        public void SetDataForTests(BiomeType region, WorldMapRegionNode[] neighbours, bool hasUnlockRule, BiomeType requiresVictoriesIn, int victoriesRequired)
        {
            _region = region;
            _neighbours = neighbours ?? new WorldMapRegionNode[0];
            _hasUnlockRule = hasUnlockRule;
            _unlockRequiresVictoriesIn = requiresVictoriesIn;
            _victoriesRequired = victoriesRequired;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, 0.75f);

            if (_neighbours == null)
            {
                return;
            }

            foreach (WorldMapRegionNode neighbour in _neighbours)
            {
                if (neighbour != null)
                {
                    Gizmos.DrawLine(transform.position, neighbour.transform.position);
                }
            }
        }
#endif
    }
}
