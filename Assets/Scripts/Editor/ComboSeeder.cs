using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Rapadura.Gameplay.Skills;

namespace Rapadura.Editor
{
    /// <summary>
    /// Seeds a couple of example <see cref="ComboDefinition"/> assets under Resources/Combos.
    /// ComboTracker (wired to the Player earlier this session, full sequence-detection logic
    /// already implemented and tested in isolation) had zero combo data anywhere in the project —
    /// same class of gap already fixed for the Skill Tree/Crafting recipes. Uses skills already
    /// seeded under Resources/Skills.
    /// </summary>
    public static class ComboSeeder
    {
        private const string OutputFolder = "Assets/Resources/Combos";

        private class ComboSeed
        {
            public string Id;
            public string DisplayName;
            public string Description;
            public string[] SkillIds;
            public float WindowSeconds;
            public ComboBonusType BonusType;
            public float BonusValue;
            public float BonusDurationSeconds;
        }

        [MenuItem("Rapadura/Seed Example Combos")]
        public static void Seed()
        {
            if (!AssetDatabase.IsValidFolder(OutputFolder))
            {
                AssetDatabase.CreateFolder("Assets/Resources", "Combos");
            }

            var seeds = new List<ComboSeed>
            {
                new ComboSeed
                {
                    Id = "combo_fire_and_ice",
                    DisplayName = "Fogo e Gelo",
                    Description = "Conjure Bola de Fogo seguida de Estilhaço de Gelo pra causar dano extra por um tempo.",
                    SkillIds = new[] { "skill_fireball", "skill_ice_shard" },
                    WindowSeconds = 3f,
                    BonusType = ComboBonusType.DamageMultiplier,
                    BonusValue = 0.25f,
                    BonusDurationSeconds = 5f,
                },
                new ComboSeed
                {
                    Id = "combo_dash_strike",
                    DisplayName = "Investida Relâmpago",
                    Description = "Dash seguido de um Ataque Relâmpago reduz o custo das próximas habilidades.",
                    SkillIds = new[] { "skill_dash", "skill_lightning_bolt" },
                    WindowSeconds = 2f,
                    BonusType = ComboBonusType.CostReduction,
                    BonusValue = 0.3f,
                    BonusDurationSeconds = 4f,
                },
            };

            int created = 0;
            foreach (ComboSeed seed in seeds)
            {
                string path = $"{OutputFolder}/{seed.Id}.asset";
                ComboDefinition combo = AssetDatabase.LoadAssetAtPath<ComboDefinition>(path);
                bool isNew = combo == null;
                if (isNew)
                {
                    combo = ScriptableObject.CreateInstance<ComboDefinition>();
                }

                var sequence = new List<SkillDefinition>();
                bool allFound = true;
                foreach (string skillId in seed.SkillIds)
                {
                    SkillDefinition skill = AssetDatabase.LoadAssetAtPath<SkillDefinition>($"Assets/Resources/Skills/{skillId}.asset");
                    if (skill == null)
                    {
                        Debug.LogWarning($"[ComboSeeder] Skill '{skillId}' not found, skipping combo '{seed.Id}'.");
                        allFound = false;
                        break;
                    }
                    sequence.Add(skill);
                }

                if (!allFound)
                {
                    continue;
                }

                var so = new SerializedObject(combo);
                so.FindProperty("_comboId").stringValue = seed.Id;
                so.FindProperty("_displayName").stringValue = seed.DisplayName;
                so.FindProperty("_description").stringValue = seed.Description;
                so.FindProperty("_windowSeconds").floatValue = seed.WindowSeconds;
                so.FindProperty("_bonusType").enumValueIndex = (int)seed.BonusType;
                so.FindProperty("_bonusValue").floatValue = seed.BonusValue;
                so.FindProperty("_bonusDurationSeconds").floatValue = seed.BonusDurationSeconds;

                SerializedProperty sequenceProp = so.FindProperty("_sequence");
                sequenceProp.arraySize = sequence.Count;
                for (int i = 0; i < sequence.Count; i++)
                {
                    sequenceProp.GetArrayElementAtIndex(i).objectReferenceValue = sequence[i];
                }
                so.ApplyModifiedProperties();

                if (isNew)
                {
                    AssetDatabase.CreateAsset(combo, path);
                }
                else
                {
                    EditorUtility.SetDirty(combo);
                }

                created++;
            }

            AssetDatabase.SaveAssets();
            Debug.Log($"[ComboSeeder] Seeded {created} combo(s) under {OutputFolder}.");
        }
    }
}
