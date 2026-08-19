using System;
using UnityEngine;

namespace Rapadura.Gameplay.Building
{
    /// <summary>
    /// Data-driven definition of a buildable structure, following the same ScriptableObject
    /// pattern as <c>ItemDefinition</c>/<c>RecipeDefinition</c>. The visual is referenced only by
    /// an Addressable/prefab key string rather than a direct prefab reference, because this
    /// project has never been opened in the Unity Editor yet (see TODO.md Fase 1 note) — no
    /// Scene/Prefab assets exist to reference safely from a script written outside the Editor.
    /// Whatever system later instantiates the structure resolves this key through Addressables
    /// (or a simple Resources.Load&lt;GameObject&gt;(key) fallback) once real prefabs exist.
    /// </summary>
    [CreateAssetMenu(fileName = "NewStructure", menuName = "Rapadura/Building/Structure Definition", order = 0)]
    public class StructureDefinition : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string _structureId = "structure_new";
        [SerializeField] private string _displayName = "New Structure";
        [TextArea(2, 4)]
        [SerializeField] private string _description = string.Empty;

        [Header("Visual (resolved at runtime, no direct prefab reference)")]
        [SerializeField] private string _prefabAddressableKey = string.Empty;

        [Header("Footprint (grid cells)")]
        [SerializeField] private Vector2Int _footprintSize = Vector2Int.one;

        [Header("Levels")]
        [Tooltip("Index 0 = level 1 (placement cost). Each subsequent entry is the cost to upgrade to that level.")]
        [SerializeField] private StructureLevelData[] _levels = { new StructureLevelData { level = 1 } };

        public string StructureId => _structureId;
        public string DisplayName => _displayName;
        public string Description => _description;

        public string PrefabAddressableKey => _prefabAddressableKey;

        public Vector2Int FootprintSize => new Vector2Int(Mathf.Max(1, _footprintSize.x), Mathf.Max(1, _footprintSize.y));

        public StructureLevelData[] Levels => _levels;
        public int MaxLevel => _levels != null && _levels.Length > 0 ? _levels[_levels.Length - 1].level : 1;

        public StructureLevelData GetLevelData(int level)
        {
            if (_levels == null)
            {
                return null;
            }

            foreach (StructureLevelData data in _levels)
            {
                if (data.level == level)
                {
                    return data;
                }
            }

            return null;
        }
    }
}
