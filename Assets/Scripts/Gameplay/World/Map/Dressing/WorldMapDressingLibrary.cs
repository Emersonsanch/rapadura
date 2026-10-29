using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Dressing
{
    /// <summary>
    /// Asset references the runtime <see cref="WorldMapDressing"/> needs but cannot build itself:
    /// the tree prefabs shared with the combat arena (Assets/Prefabs/Environment, loaded through this
    /// Resources asset so builds include them) and the lit material the palette/ground materials are
    /// copied from. Every field is optional: without trees the dressing builds low-poly primitive
    /// trees, without a material it copies the scene ground's (or a primitive's default) material.
    /// </summary>
    [CreateAssetMenu(fileName = "WorldMapDressingLibrary", menuName = "Rapadura/World/World Map Dressing Library")]
    public class WorldMapDressingLibrary : ScriptableObject
    {
        public const string ResourcePath = "WorldMap/WorldMapDressingLibrary";

        [Tooltip("Broadleaf/gnarled tree prefabs for the forest (root named Tree_* with a LODGroup, so WindSystem sways them).")]
        [SerializeField] private GameObject[] _forestTrees = new GameObject[0];
        [Tooltip("Conifers for the mountain slopes.")]
        [SerializeField] private GameObject[] _mountainTrees = new GameObject[0];
        [Tooltip("Opaque URP Lit material copied (never modified) for the palette and ground materials.")]
        [SerializeField] private Material _baseMaterial;

        public GameObject[] ForestTrees => _forestTrees;
        public GameObject[] MountainTrees => _mountainTrees;
        public Material BaseMaterial => _baseMaterial;

        public static WorldMapDressingLibrary Load() => Resources.Load<WorldMapDressingLibrary>(ResourcePath);

#if UNITY_EDITOR
        public void SetDataForTests(GameObject[] forestTrees, GameObject[] mountainTrees, Material baseMaterial)
        {
            _forestTrees = forestTrees ?? new GameObject[0];
            _mountainTrees = mountainTrees ?? new GameObject[0];
            _baseMaterial = baseMaterial;
        }
#endif
    }
}
