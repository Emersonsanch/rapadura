using System;
using System.Collections.Generic;
using Rapadura.Gameplay.Content;
using Rapadura.Gameplay.Skills;
using UnityEngine;

namespace Rapadura.Gameplay.ClassEvolution
{
    /// <summary>
    /// Builds the runtime <see cref="SkillDefinition"/> of a <see cref="GraduationSkillSpec"/> (one cached instance per id),
    /// so graduation skills go through the normal <see cref="SkillManager"/> pipeline (cost, cooldown, self buff, damage,
    /// debuff, stun, heal, hotbar, save) without hand-made assets. Name/description are the pt-BR fallback (like the
    /// assets); the UI localizes them through <c>skill.&lt;id&gt;.*</c>.
    /// </summary>
    public static class GraduationSkillFactory
    {
        private static readonly Dictionary<string, SkillDefinition> Cache = new Dictionary<string, SkillDefinition>();

        // Mirrors SkillDefinition's serialized field names; JsonUtility writes them into the private fields.
        [Serializable]
        private class SkillJson
        {
            public string _skillId;
            public string _displayName;
            public string _description;
            public int _skillType;
            public int _category;
            public int _element;
            public float _cooldownSeconds;
            public float _manaCost;
            public float _energyCost;
            public float _castTimeSeconds;
            public float _recoverySeconds = 0.2f;
            public float _baseDamage;
            public float _baseHealing;
            public float _range;
            public float _areaRadius;
            public float _damagePerLevel;
            public float _healingPerLevel;
            public StatModifierDefinition[] _buffs = Array.Empty<StatModifierDefinition>();
            public StatModifierDefinition[] _debuffs = Array.Empty<StatModifierDefinition>();
            public float _stunDurationSeconds;
            public string _animationTrigger = string.Empty;
            public string _vfxPresetId = string.Empty;
            public int _maxLevel = 5;
            public int _experienceToUnlock;
            public RequirementJson _requirement = new RequirementJson();
        }

        [Serializable]
        private class RequirementJson
        {
            public int minimumPlayerLevel = 1;
            public int requiredSkillLevel = 1;
            public string requiredClassOrSpecialization = string.Empty;
        }

        public static SkillDefinition Get(string skillId)
        {
            if (string.IsNullOrEmpty(skillId))
            {
                return null;
            }

            if (Cache.TryGetValue(skillId, out SkillDefinition cached) && cached != null)
            {
                return cached;
            }

            GraduationSkillSpec spec = ClassEvolutionCatalog.GetSkill(skillId);
            if (spec == null)
            {
                return null;
            }

            SkillDefinition built = Build(spec);
            Cache[skillId] = built;
            return built;
        }

        public static List<SkillDefinition> GetAll(IEnumerable<GraduationSkillSpec> specs)
        {
            var list = new List<SkillDefinition>();
            foreach (GraduationSkillSpec spec in specs)
            {
                SkillDefinition skill = Get(spec.SkillId);
                if (skill != null)
                {
                    list.Add(skill);
                }
            }

            return list;
        }

        /// <summary>A fresh (uncached) definition. Tests and the cache use it.</summary>
        public static SkillDefinition Build(GraduationSkillSpec spec)
        {
            GraduationSpec graduation = ClassEvolutionCatalog.GetGraduation(spec.Character, spec.Tier);
            bool group = spec.Kind == GraduationSkillKind.GroupBuff;
            var json = new SkillJson
            {
                _skillId = spec.SkillId,
                _displayName = ClassEvolutionTexts.GetFallback(ContentText.SkillNameKey(spec.SkillId)) ?? spec.SkillId,
                _description = ClassEvolutionTexts.GetFallback(ContentText.SkillDescriptionKey(spec.SkillId)) ?? string.Empty,
                _skillType = (int)SkillType.Active,
                _category = (int)(group ? SkillCategory.Support : spec.StunSeconds > 0f ? SkillCategory.Utility : SkillCategory.Offensive),
                _element = (int)ElementType.None,
                _cooldownSeconds = spec.CooldownSeconds,
                _manaCost = spec.ManaCost,
                _castTimeSeconds = spec.CastTimeSeconds,
                _baseDamage = spec.BaseDamage,
                _baseHealing = spec.BaseHealing,
                _range = group ? spec.AreaRadius : 14f,
                _areaRadius = spec.AreaRadius,
                _damagePerLevel = spec.DamagePerLevel,
                _stunDurationSeconds = spec.StunSeconds,
                _requirement = new RequirementJson
                {
                    minimumPlayerLevel = graduation != null ? graduation.MinLevel : 1,
                    requiredClassOrSpecialization = spec.Character.ToString(),
                },
            };

            if (spec.HasEffect)
            {
                var effect = new StatModifierDefinition
                {
                    displayName = json._displayName,
                    affectedStat = spec.EffectStat,
                    application = spec.EffectApplication,
                    value = spec.EffectValue,
                    durationSeconds = spec.EffectDurationSeconds,
                    isDebuff = !group,
                };
                if (group)
                {
                    json._buffs = new[] { effect };
                }
                else
                {
                    json._debuffs = new[] { effect };
                }
            }

            SkillDefinition skill = ScriptableObject.CreateInstance<SkillDefinition>();
            JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(json), skill);
            skill.name = spec.SkillId;
            skill.hideFlags = HideFlags.DontSave;
            return skill;
        }

        /// <summary>The group-buff effect a graduation skill spreads to allies (null when it is not a group buff).</summary>
        public static StatModifierDefinition AllyEffect(GraduationSkillSpec spec)
        {
            if (spec == null || spec.Kind != GraduationSkillKind.GroupBuff || !spec.HasEffect)
            {
                return null;
            }

            return new StatModifierDefinition
            {
                displayName = ClassEvolutionTexts.GetFallback(ContentText.SkillNameKey(spec.SkillId)) ?? spec.SkillId,
                affectedStat = spec.EffectStat,
                application = spec.EffectApplication,
                value = spec.EffectValue,
                durationSeconds = spec.EffectDurationSeconds,
                isDebuff = false,
            };
        }
    }
}
