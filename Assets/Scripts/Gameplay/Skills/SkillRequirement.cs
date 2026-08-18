using System;

namespace Rapadura.Gameplay.Skills
{
    /// <summary>
    /// Prerequisite required before a skill can be learned — used to build branching skill trees.
    /// A skill can require a minimum player level, a minimum skill-tree points spent, and/or
    /// other specific skills already learned to a given level.
    /// </summary>
    [Serializable]
    public class SkillRequirement
    {
        public int minimumPlayerLevel = 1;
        public SkillDefinition[] requiredSkills = Array.Empty<SkillDefinition>();
        public int requiredSkillLevel = 1;
        public string requiredClassOrSpecialization = string.Empty;
    }
}
