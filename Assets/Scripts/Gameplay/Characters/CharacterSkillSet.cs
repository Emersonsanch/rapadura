using System;
using System.Collections.Generic;
using Rapadura.Gameplay.Skills;
using UnityEngine;

namespace Rapadura.Gameplay.Characters
{
    /// <summary>
    /// Static lookup that resolves a <see cref="CharacterId"/> to its 5 signature
    /// <see cref="SkillDefinition"/> assets (4 actives + the Ultimate), loaded from
    /// Resources/Skills/Characters/&lt;CharacterId&gt;. Ownership is expressed by the folder plus each
    /// asset's <see cref="SkillRequirement.requiredClassOrSpecialization"/> (set to the
    /// <see cref="CharacterId"/> name), so <see cref="SkillDefinition"/> itself stays character-agnostic.
    /// Skills are returned in the same order as <see cref="PlayableCharacter.AbilityNames"/>.
    /// Mirrors the load-on-demand/invalidate/SetForTests pattern of <c>BiomeRegistry</c>.
    /// </summary>
    public static class CharacterSkillSet
    {
        public const string ResourcesRoot = "Skills/Characters";
        public const int SkillsPerCharacter = 5;

        private static Dictionary<CharacterId, SkillDefinition[]> _skillsByCharacter;

        /// <summary>Resources-relative folder holding one character's skill assets (e.g. "Skills/Characters/Icaro").</summary>
        public static string GetResourcesFolder(CharacterId id) => ResourcesRoot + "/" + id;

        /// <summary>Resources-relative path of the per-character skill tree (e.g. "SkillTrees/tree_icaro").</summary>
        public static string GetSkillTreeResourcesPath(CharacterId id) => "SkillTrees/tree_" + id.ToString().ToLowerInvariant();

        public static IReadOnlyList<SkillDefinition> GetSkills(CharacterId id)
        {
            EnsureLoaded(id);
            return _skillsByCharacter[id];
        }

        /// <summary>Loads the character's <see cref="SkillTreeDefinition"/> (may be null if the asset is missing).</summary>
        public static SkillTreeDefinition GetSkillTree(CharacterId id)
        {
            return Resources.Load<SkillTreeDefinition>(GetSkillTreeResourcesPath(id));
        }

        /// <summary>
        /// Gives the selected character their skill set: registers all 5 skills with the
        /// <paramref name="skillManager"/> (so save/load and the skill tree can resolve them), then
        /// learns and equips — in order, into the first free hotbar slots — every skill whose
        /// requirements are already met at <paramref name="casterLevel"/>. At level 1 that is the 4
        /// actives; the Ultimate stays registered but locked until its level/prerequisite are reached
        /// and it is bought in the skill tree. Returns how many skills were newly learned.
        /// </summary>
        public static int GrantTo(SkillManager skillManager, CharacterId id, int casterLevel)
        {
            if (skillManager == null)
            {
                return 0;
            }

            IReadOnlyList<SkillDefinition> skills = GetSkills(id);
            skillManager.RegisterSkills(skills);

            int learnedCount = 0;

            foreach (SkillDefinition skill in skills)
            {
                if (!skillManager.CanLearnSkill(skill, casterLevel))
                {
                    continue;
                }

                if (skillManager.LearnSkill(skill, casterLevel) == null)
                {
                    continue;
                }

                learnedCount++;
                int freeSlot = FindFreeSlot(skillManager);

                if (freeSlot >= 0)
                {
                    skillManager.EquipSkill(freeSlot, skill.SkillId);
                }
            }

            return learnedCount;
        }

        /// <summary>
        /// Finds a character signature skill by <see cref="SkillDefinition.SkillId"/> across every
        /// hero's set (null if none matches). Lets <see cref="SkillManager.RestoreState"/> resolve
        /// character skills from a save even when no hero has been applied/registered yet.
        /// </summary>
        public static SkillDefinition FindSkill(string skillId)
        {
            if (string.IsNullOrEmpty(skillId))
            {
                return null;
            }

            foreach (CharacterId id in (CharacterId[])Enum.GetValues(typeof(CharacterId)))
            {
                foreach (SkillDefinition skill in GetSkills(id))
                {
                    if (skill != null && skill.SkillId == skillId)
                    {
                        return skill;
                    }
                }
            }

            return null;
        }

        private static int FindFreeSlot(SkillManager skillManager)
        {
            IReadOnlyList<SkillInstance> slots = skillManager.EquippedSlots;

            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i] == null)
                {
                    return i;
                }
            }

            return -1;
        }

        private static void EnsureLoaded(CharacterId id)
        {
            if (_skillsByCharacter == null)
            {
                _skillsByCharacter = new Dictionary<CharacterId, SkillDefinition[]>();
            }

            if (_skillsByCharacter.ContainsKey(id))
            {
                return;
            }

            _skillsByCharacter[id] = Sort(id, Resources.LoadAll<SkillDefinition>(GetResourcesFolder(id)));
        }

        /// <summary>Orders skills by their position in <see cref="PlayableCharacter.AbilityNames"/>; unknown names go last.</summary>
        private static SkillDefinition[] Sort(CharacterId id, IEnumerable<SkillDefinition> skills)
        {
            string[] abilityNames = CharacterRegistry.Get(id).AbilityNames;
            var result = new List<SkillDefinition>();

            foreach (SkillDefinition skill in skills)
            {
                if (skill != null)
                {
                    result.Add(skill);
                }
            }

            result.Sort((a, b) => IndexOf(abilityNames, a).CompareTo(IndexOf(abilityNames, b)));
            return result.ToArray();
        }

        private static int IndexOf(string[] abilityNames, SkillDefinition skill)
        {
            int index = Array.IndexOf(abilityNames, skill.DisplayName);
            return index >= 0 ? index : int.MaxValue;
        }

        /// <summary>Forces the next lookup to re-scan Resources. Call after generating/removing skill assets in the editor.</summary>
        public static void Invalidate()
        {
            _skillsByCharacter = null;
        }

        /// <summary>
        /// Plain-C# injection path for EditMode tests that want to avoid the Resources scan for one
        /// character (mirrors <c>BiomeRegistry.SetForTests</c>). Lasts until <see cref="Invalidate"/>.
        /// </summary>
        public static void SetForTests(CharacterId id, IEnumerable<SkillDefinition> skills)
        {
            if (_skillsByCharacter == null)
            {
                _skillsByCharacter = new Dictionary<CharacterId, SkillDefinition[]>();
            }

            _skillsByCharacter[id] = Sort(id, skills ?? Array.Empty<SkillDefinition>());
        }
    }
}
