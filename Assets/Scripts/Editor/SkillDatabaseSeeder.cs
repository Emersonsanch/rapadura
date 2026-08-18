using System.Collections.Generic;
using System.IO;
using Rapadura.Gameplay.Skills;
using UnityEditor;
using UnityEngine;

namespace Rapadura.Editor
{
    /// <summary>
    /// One-shot editor tool that generates the project's starter set of 20 example
    /// SkillDefinition assets under Assets/Resources/Skills, so the skill system ships
    /// with real, balanced content instead of an empty database. Safe to re-run: existing
    /// assets with a matching skill id are updated in place rather than duplicated.
    /// Run via Rapadura > Seed Example Skills.
    /// </summary>
    public static class SkillDatabaseSeeder
    {
        private const string SkillFolder = "Assets/Resources/Skills";

        private class SkillSeed
        {
            public string id, name, description, animationTrigger;
            public SkillType type = SkillType.Active;
            public SkillCategory category;
            public ElementType element;
            public float cooldown, mana, energy, castTime = 0f, range = 5f, area = 0f;
            public float damage = 0f, damagePerLevel = 0f, healing = 0f, healingPerLevel = 0f;
            public int maxLevel = 5, expToUnlock = 100;
            public List<StatModifierDefinition> buffs = new List<StatModifierDefinition>();
            public List<StatModifierDefinition> debuffs = new List<StatModifierDefinition>();
        }

        [MenuItem("Rapadura/Seed Example Skills")]
        public static void SeedSkills()
        {
            if (!Directory.Exists(SkillFolder))
            {
                Directory.CreateDirectory(SkillFolder);
            }

            foreach (SkillSeed seed in BuildSeeds())
            {
                CreateOrUpdateSkill(seed);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[SkillDatabaseSeeder] 20 example skills created/updated under " + SkillFolder);
        }

        private static void CreateOrUpdateSkill(SkillSeed seed)
        {
            string path = Path.Combine(SkillFolder, seed.id + ".asset");
            var skill = AssetDatabase.LoadAssetAtPath<SkillDefinition>(path);

            if (skill == null)
            {
                skill = ScriptableObject.CreateInstance<SkillDefinition>();
                AssetDatabase.CreateAsset(skill, path);
            }

            var so = new SerializedObject(skill);
            so.FindProperty("_skillId").stringValue = seed.id;
            so.FindProperty("_displayName").stringValue = seed.name;
            so.FindProperty("_description").stringValue = seed.description;
            so.FindProperty("_skillType").enumValueIndex = (int)seed.type;
            so.FindProperty("_category").enumValueIndex = (int)seed.category;
            so.FindProperty("_element").enumValueIndex = (int)seed.element;
            so.FindProperty("_cooldownSeconds").floatValue = seed.cooldown;
            so.FindProperty("_manaCost").floatValue = seed.mana;
            so.FindProperty("_energyCost").floatValue = seed.energy;
            so.FindProperty("_castTimeSeconds").floatValue = seed.castTime;
            so.FindProperty("_baseDamage").floatValue = seed.damage;
            so.FindProperty("_damagePerLevel").floatValue = seed.damagePerLevel;
            so.FindProperty("_baseHealing").floatValue = seed.healing;
            so.FindProperty("_healingPerLevel").floatValue = seed.healingPerLevel;
            so.FindProperty("_range").floatValue = seed.range;
            so.FindProperty("_areaRadius").floatValue = seed.area;
            so.FindProperty("_animationTrigger").stringValue = seed.animationTrigger;
            so.FindProperty("_maxLevel").intValue = seed.maxLevel;
            so.FindProperty("_experienceToUnlock").intValue = seed.expToUnlock;

            WriteModifierArray(so.FindProperty("_buffs"), seed.buffs);
            WriteModifierArray(so.FindProperty("_debuffs"), seed.debuffs);

            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(skill);
        }

        private static void WriteModifierArray(SerializedProperty arrayProperty, List<StatModifierDefinition> modifiers)
        {
            arrayProperty.arraySize = modifiers.Count;

            for (int i = 0; i < modifiers.Count; i++)
            {
                SerializedProperty element = arrayProperty.GetArrayElementAtIndex(i);
                StatModifierDefinition modifier = modifiers[i];

                element.FindPropertyRelative("displayName").stringValue = modifier.displayName;
                element.FindPropertyRelative("affectedStat").enumValueIndex = (int)modifier.affectedStat;
                element.FindPropertyRelative("application").enumValueIndex = (int)modifier.application;
                element.FindPropertyRelative("value").floatValue = modifier.value;
                element.FindPropertyRelative("durationSeconds").floatValue = modifier.durationSeconds;
                element.FindPropertyRelative("isDebuff").boolValue = modifier.isDebuff;
                element.FindPropertyRelative("ticksOverTime").boolValue = modifier.ticksOverTime;
                element.FindPropertyRelative("tickIntervalSeconds").floatValue = modifier.tickIntervalSeconds;
                element.FindPropertyRelative("tickDamageOrHeal").floatValue = modifier.tickDamageOrHeal;
            }
        }

        private static StatModifierDefinition Modifier(string name, StatType stat, ModifierApplication app, float value, float duration, bool isDebuff, bool ticks = false, float tickInterval = 1f, float tickValue = 0f)
        {
            return new StatModifierDefinition
            {
                displayName = name,
                affectedStat = stat,
                application = app,
                value = value,
                durationSeconds = duration,
                isDebuff = isDebuff,
                ticksOverTime = ticks,
                tickIntervalSeconds = tickInterval,
                tickDamageOrHeal = tickValue
            };
        }

        private static List<SkillSeed> BuildSeeds()
        {
            return new List<SkillSeed>
            {
                new SkillSeed
                {
                    id = "skill_fireball", name = "Bola de Fogo", animationTrigger = "CastFireball",
                    description = "Lança uma esfera flamejante que explode ao atingir o alvo, causando dano de fogo em área.",
                    category = SkillCategory.Offensive, element = ElementType.Fire,
                    cooldown = 4f, mana = 20f, castTime = 0.4f, range = 12f, area = 2.5f,
                    damage = 25f, damagePerLevel = 6f
                },
                new SkillSeed
                {
                    id = "skill_ice_shard", name = "Gelo", animationTrigger = "CastIce",
                    description = "Dispara um estilhaço de gelo que causa dano e reduz a velocidade de movimento do alvo.",
                    category = SkillCategory.Offensive, element = ElementType.Ice,
                    cooldown = 5f, mana = 18f, range = 10f, area = 1.5f,
                    damage = 15f, damagePerLevel = 4f,
                    debuffs = { Modifier("Lentidão", StatType.MoveSpeed, ModifierApplication.PercentAdditive, -0.4f, 3f, true) }
                },
                new SkillSeed
                {
                    id = "skill_lightning_bolt", name = "Raio", animationTrigger = "CastLightning",
                    description = "Convoca um raio instantâneo que atinge o alvo com alto dano elétrico.",
                    category = SkillCategory.Offensive, element = ElementType.Lightning,
                    cooldown = 6f, mana = 25f, range = 15f, area = 1f,
                    damage = 35f, damagePerLevel = 8f
                },
                new SkillSeed
                {
                    id = "skill_heal", name = "Cura", animationTrigger = "CastHeal",
                    description = "Restaura uma quantidade de vida instantaneamente.",
                    category = SkillCategory.Support, element = ElementType.Holy,
                    cooldown = 6f, mana = 22f, healing = 30f, healingPerLevel = 8f
                },
                new SkillSeed
                {
                    id = "skill_shield", name = "Escudo", animationTrigger = "CastShield",
                    description = "Envolve o conjurador em uma barreira que aumenta a defesa por um curto período.",
                    category = SkillCategory.Defensive, element = ElementType.Holy,
                    cooldown = 12f, mana = 20f,
                    buffs = { Modifier("Escudo Mágico", StatType.Defense, ModifierApplication.PercentAdditive, 0.5f, 6f, false) }
                },
                new SkillSeed
                {
                    id = "skill_dash", name = "Dash", animationTrigger = "CastDash",
                    description = "Impulsiona o personagem rapidamente para frente, aumentando a velocidade momentaneamente.",
                    category = SkillCategory.Mobility, element = ElementType.None,
                    cooldown = 3f, energy = 15f,
                    buffs = { Modifier("Impulso", StatType.MoveSpeed, ModifierApplication.PercentAdditive, 1.5f, 0.3f, false) }
                },
                new SkillSeed
                {
                    id = "skill_tornado", name = "Tornado", animationTrigger = "CastTornado",
                    description = "Cria um tornado que arrasta e danifica inimigos próximos continuamente.",
                    category = SkillCategory.Offensive, element = ElementType.Wind,
                    cooldown = 14f, mana = 40f, range = 8f, area = 3.5f,
                    damage = 10f, damagePerLevel = 3f
                },
                new SkillSeed
                {
                    id = "skill_earthquake", name = "Terremoto", animationTrigger = "CastEarthquake",
                    description = "Abala o solo ao redor do conjurador, causando dano em área e derrubando inimigos.",
                    category = SkillCategory.Offensive, element = ElementType.Earth,
                    cooldown = 16f, mana = 45f, area = 4f,
                    damage = 30f, damagePerLevel = 7f
                },
                new SkillSeed
                {
                    id = "skill_poison_dart", name = "Veneno", animationTrigger = "CastPoison",
                    description = "Aplica uma toxina que causa dano contínuo ao longo do tempo.",
                    category = SkillCategory.Offensive, element = ElementType.Poison,
                    cooldown = 8f, mana = 16f, range = 10f, area = 1f,
                    damage = 5f,
                    debuffs = { Modifier("Envenenado", StatType.HealthRegen, ModifierApplication.Flat, 0f, 6f, true, ticks: true, tickInterval: 1f, tickValue: 4f) }
                },
                new SkillSeed
                {
                    id = "skill_invisibility", name = "Invisibilidade", animationTrigger = "CastInvisibility",
                    description = "Torna o conjurador furtivo, reduzindo a chance de ser detectado por inimigos.",
                    category = SkillCategory.Utility, element = ElementType.Dark,
                    cooldown = 25f, mana = 35f,
                    buffs = { Modifier("Furtividade", StatType.MoveSpeed, ModifierApplication.PercentAdditive, 0.2f, 8f, false) }
                },
                new SkillSeed
                {
                    id = "skill_summon", name = "Invocação", animationTrigger = "CastSummon",
                    description = "Invoca uma criatura aliada para lutar ao lado do conjurador por um tempo.",
                    category = SkillCategory.Summon, element = ElementType.Arcane,
                    cooldown = 30f, mana = 50f, maxLevel = 3
                },
                new SkillSeed
                {
                    id = "skill_explosion", name = "Explosão", animationTrigger = "CastExplosion",
                    description = "Detona uma explosão arcana instantânea ao redor do conjurador.",
                    category = SkillCategory.Offensive, element = ElementType.Arcane,
                    cooldown = 10f, mana = 32f, area = 3f,
                    damage = 40f, damagePerLevel = 9f
                },
                new SkillSeed
                {
                    id = "skill_arrow", name = "Flecha", animationTrigger = "CastArrow",
                    description = "Dispara uma flecha precisa de longo alcance contra um único alvo.",
                    category = SkillCategory.Offensive, element = ElementType.Physical,
                    cooldown = 1.5f, energy = 8f, range = 20f,
                    damage = 18f, damagePerLevel = 4f
                },
                new SkillSeed
                {
                    id = "skill_meteor_shower", name = "Chuva de Meteoros", animationTrigger = "CastMeteor",
                    description = "Ultimate: invoca uma chuva de meteoros sobre uma grande área, causando dano massivo.",
                    type = SkillType.Ultimate, category = SkillCategory.Offensive, element = ElementType.Fire,
                    cooldown = 60f, mana = 80f, castTime = 1.5f, area = 6f, maxLevel = 3,
                    damage = 60f, damagePerLevel = 15f
                },
                new SkillSeed
                {
                    id = "skill_thorns", name = "Espinhos", animationTrigger = "CastThorns",
                    description = "Passiva: reflete parte do dano recebido de volta ao atacante através de espinhos mágicos.",
                    type = SkillType.Passive, category = SkillCategory.Defensive, element = ElementType.Earth,
                    cooldown = 0f, mana = 0f,
                    buffs = { Modifier("Casca de Espinhos", StatType.Defense, ModifierApplication.PercentAdditive, 0.15f, 999f, false) }
                },
                new SkillSeed
                {
                    id = "skill_freeze", name = "Congelamento", animationTrigger = "CastFreeze",
                    description = "Congela o alvo, imobilizando-o e reduzindo drasticamente sua velocidade.",
                    category = SkillCategory.Offensive, element = ElementType.Ice,
                    cooldown = 12f, mana = 28f, range = 10f, area = 1.5f,
                    damage = 8f,
                    debuffs = { Modifier("Congelado", StatType.MoveSpeed, ModifierApplication.PercentAdditive, -0.9f, 2.5f, true) }
                },
                new SkillSeed
                {
                    id = "skill_teleport", name = "Teleporte", animationTrigger = "CastTeleport",
                    description = "Teleporta o conjurador instantaneamente para o ponto mirado.",
                    category = SkillCategory.Mobility, element = ElementType.Arcane,
                    cooldown = 9f, mana = 24f, range = 15f
                },
                new SkillSeed
                {
                    id = "skill_regeneration", name = "Regeneração", animationTrigger = "CastRegeneration",
                    description = "Aumenta drasticamente a regeneração de vida por um período.",
                    category = SkillCategory.Support, element = ElementType.Holy,
                    cooldown = 20f, mana = 30f,
                    buffs = { Modifier("Regeneração", StatType.HealthRegen, ModifierApplication.Flat, 0f, 8f, false, ticks: true, tickInterval: 1f, tickValue: 6f) }
                },
                new SkillSeed
                {
                    id = "skill_super_jump", name = "Super Salto", animationTrigger = "CastSuperJump",
                    description = "Concede um salto vertical muito acima do normal.",
                    category = SkillCategory.Mobility, element = ElementType.None,
                    cooldown = 6f, energy = 20f
                },
                new SkillSeed
                {
                    id = "skill_berserk", name = "Berserk", animationTrigger = "CastBerserk",
                    description = "Ultimate: entra em fúria, aumentando dano de ataque em troca de reduzir a defesa.",
                    type = SkillType.Ultimate, category = SkillCategory.Offensive, element = ElementType.Physical,
                    cooldown = 45f, mana = 20f, maxLevel = 3,
                    buffs = { Modifier("Fúria", StatType.AttackDamage, ModifierApplication.PercentAdditive, 0.6f, 10f, false) },
                    debuffs = { Modifier("Exposto", StatType.Defense, ModifierApplication.PercentAdditive, -0.3f, 10f, true) }
                }
            };
        }
    }
}
