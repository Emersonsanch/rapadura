using UnityEngine;

namespace Rapadura.Gameplay.Skills
{
    /// <summary>
    /// Data-driven description of one skill tree — typically one per class/specialization
    /// (e.g. "Mage", "Warrior"). Lays out which skills appear where in the grid; branching,
    /// prerequisites and level gates are read from each node's own SkillDefinition.Requirement.
    /// Consumed by the runtime Skill Tree UI (SkillTreeController) to draw the tree and by
    /// SkillManager indirectly through the requirement checks already in place.
    /// </summary>
    [CreateAssetMenu(fileName = "NewSkillTree", menuName = "Rapadura/Skills/Skill Tree Definition", order = 1)]
    public class SkillTreeDefinition : ScriptableObject
    {
        [SerializeField] private string _treeId = "tree_new";
        [SerializeField] private string _displayName = "New Skill Tree";
        [TextArea(2, 4)]
        [SerializeField] private string _description = string.Empty;
        [SerializeField] private SkillTreeNodeData[] _nodes = System.Array.Empty<SkillTreeNodeData>();

        public string TreeId => _treeId;
        public string DisplayName => _displayName;
        public string Description => _description;
        public SkillTreeNodeData[] Nodes => _nodes;
    }
}
