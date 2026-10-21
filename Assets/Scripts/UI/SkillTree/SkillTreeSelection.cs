using System;
using System.Collections.Generic;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Skills;

namespace Rapadura.UI.SkillTree
{
    /// <summary>
    /// Pure rule for which <see cref="SkillTreeDefinition"/> the Skill Tree screen opens on: the
    /// selected hero's own tree (<see cref="CharacterSkillSet.GetSkillTree"/>, injected so EditMode
    /// tests don't hit Resources) when a hero is chosen and that tree exists, otherwise the first of
    /// the scene's configured trees (historically tree_general — which is what every hero used to
    /// see, since nothing ever switched the screen to the hero's tree).
    /// </summary>
    public static class SkillTreeSelection
    {
        public static SkillTreeDefinition Resolve(
            CharacterId? hero,
            Func<CharacterId, SkillTreeDefinition> loadHeroTree,
            IReadOnlyList<SkillTreeDefinition> availableTrees)
        {
            if (hero.HasValue && loadHeroTree != null)
            {
                SkillTreeDefinition heroTree = loadHeroTree(hero.Value);

                if (heroTree != null)
                {
                    return heroTree;
                }
            }

            if (availableTrees != null)
            {
                foreach (SkillTreeDefinition tree in availableTrees)
                {
                    if (tree != null)
                    {
                        return tree;
                    }
                }
            }

            return null;
        }
    }
}
