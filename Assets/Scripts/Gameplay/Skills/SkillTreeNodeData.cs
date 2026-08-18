using System;
using UnityEngine;

namespace Rapadura.Gameplay.Skills
{
    /// <summary>
    /// Placement of a single skill inside a skill tree's visual grid. Prerequisites/branching
    /// are not duplicated here — they already live on <see cref="SkillDefinition.Requirement"/>,
    /// so the tree UI simply draws a connector line from each node to its required skills' nodes.
    /// </summary>
    [Serializable]
    public class SkillTreeNodeData
    {
        public SkillDefinition skill;
        public Vector2Int gridPosition;
    }
}
