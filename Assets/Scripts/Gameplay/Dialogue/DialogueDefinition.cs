using System.Collections.Generic;
using UnityEngine;

namespace Rapadura.Gameplay.Dialogue
{
    /// <summary>
    /// Data-driven dialogue tree: an id, an entry node, and the flat list of nodes it can visit.
    /// Authored as a ScriptableObject asset (same "data as asset" pattern as
    /// <c>SkillDefinition</c>/<c>ItemDefinition</c>/<c>RecipeDefinition</c> elsewhere in this
    /// project) so designers can create/edit dialogue trees in the Inspector without touching
    /// code, and so <see cref="Rapadura.Editor.DialogueSeeder"/> can generate example content the
    /// same way <c>SkillDatabaseSeeder</c> does for skills.
    /// </summary>
    [CreateAssetMenu(fileName = "NewDialogue", menuName = "Rapadura/Dialogue/Dialogue Definition")]
    public class DialogueDefinition : ScriptableObject
    {
        [SerializeField] private string _dialogueId;
        [SerializeField] private string _startNodeId;
        [SerializeField] private List<DialogueNode> _nodes = new List<DialogueNode>();

        public string DialogueId => _dialogueId;
        public string StartNodeId => _startNodeId;
        public IReadOnlyList<DialogueNode> Nodes => _nodes;

        private Dictionary<string, DialogueNode> _lookup;

        /// <summary>Finds a node by id in O(1) after the first call (lazily built lookup, rebuilt if the backing list changes size).</summary>
        public DialogueNode GetNode(string nodeId)
        {
            if (string.IsNullOrEmpty(nodeId))
            {
                return null;
            }

            if (_lookup == null || _lookup.Count != _nodes.Count)
            {
                RebuildLookup();
            }

            return _lookup.TryGetValue(nodeId, out DialogueNode node) ? node : null;
        }

        public DialogueNode GetStartNode()
        {
            return GetNode(_startNodeId);
        }

        private void RebuildLookup()
        {
            _lookup = new Dictionary<string, DialogueNode>();

            foreach (DialogueNode node in _nodes)
            {
                if (node != null && !string.IsNullOrEmpty(node.Id))
                {
                    _lookup[node.Id] = node;
                }
            }
        }

#if UNITY_EDITOR
        /// <summary>Editor/seeder-only helper to populate this asset's fields via SerializedObject
        /// property names (mirrors the pattern used by SkillDatabaseSeeder). Not used at runtime.</summary>
        public const string DialogueIdPropertyName = nameof(_dialogueId);
        public const string StartNodeIdPropertyName = nameof(_startNodeId);
        public const string NodesPropertyName = nameof(_nodes);
#endif

        /// <summary>Plain-C# construction path for EditMode tests, which can't rely on AssetDatabase/CreateAsset.</summary>
        public void SetDataForTests(string dialogueId, string startNodeId, List<DialogueNode> nodes)
        {
            _dialogueId = dialogueId;
            _startNodeId = startNodeId;
            _nodes = nodes ?? new List<DialogueNode>();
            _lookup = null;
        }
    }
}
