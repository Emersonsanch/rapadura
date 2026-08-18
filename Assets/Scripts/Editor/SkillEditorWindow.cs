using System.Collections.Generic;
using System.IO;
using System.Linq;
using Rapadura.Gameplay.Skills;
using UnityEditor;
using UnityEngine;

namespace Rapadura.Editor
{
    /// <summary>
    /// Visual editor for creating and tuning SkillDefinition assets without writing code.
    /// Lists every skill in the project on the left, and shows a full editable form (including
    /// add/remove buttons for buffs and debuffs) on the right. Open via Rapadura > Skill Editor.
    /// </summary>
    public class SkillEditorWindow : EditorWindow
    {
        private const string DefaultSkillFolder = "Assets/Resources/Skills";

        [SerializeField] private SkillDefinition _selectedSkill;
        private SerializedObject _selectedSerializedObject;
        private Vector2 _listScrollPosition;
        private Vector2 _detailScrollPosition;
        private string _searchFilter = string.Empty;
        private List<SkillDefinition> _allSkills = new List<SkillDefinition>();

        [MenuItem("Rapadura/Skill Editor")]
        public static void Open()
        {
            SkillEditorWindow window = GetWindow<SkillEditorWindow>("Skill Editor");
            window.minSize = new Vector2(760, 480);
            window.RefreshSkillList();
        }

        private void OnEnable()
        {
            RefreshSkillList();
        }

        private void RefreshSkillList()
        {
            _allSkills = AssetDatabase.FindAssets("t:SkillDefinition")
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<SkillDefinition>)
                .Where(skill => skill != null)
                .OrderBy(skill => skill.DisplayName)
                .ToList();
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();
            DrawSkillList();
            DrawSkillDetail();
            EditorGUILayout.EndHorizontal();
        }

        private void DrawSkillList()
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(260));

            EditorGUILayout.LabelField("Skills", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            _searchFilter = EditorGUILayout.TextField(_searchFilter, EditorStyles.toolbarSearchField);
            if (GUILayout.Button("New", GUILayout.Width(45)))
            {
                CreateNewSkill();
            }
            EditorGUILayout.EndHorizontal();

            _listScrollPosition = EditorGUILayout.BeginScrollView(_listScrollPosition);

            foreach (SkillDefinition skill in _allSkills)
            {
                if (skill == null)
                {
                    continue;
                }

                if (!string.IsNullOrEmpty(_searchFilter) &&
                    skill.DisplayName.IndexOf(_searchFilter, System.StringComparison.OrdinalIgnoreCase) < 0)
                {
                    continue;
                }

                bool isSelected = skill == _selectedSkill;
                GUIStyle style = isSelected ? EditorStyles.whiteLabel : EditorStyles.label;

                EditorGUILayout.BeginHorizontal(isSelected ? "SelectionRect" : GUIStyle.none);
                if (GUILayout.Button($"[{skill.Category}] {skill.DisplayName}", style, GUILayout.Height(20)))
                {
                    SelectSkill(skill);
                }
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void SelectSkill(SkillDefinition skill)
        {
            _selectedSkill = skill;
            _selectedSerializedObject = new SerializedObject(skill);
        }

        private void CreateNewSkill()
        {
            if (!Directory.Exists(DefaultSkillFolder))
            {
                Directory.CreateDirectory(DefaultSkillFolder);
            }

            string path = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(DefaultSkillFolder, "NewSkill.asset"));
            var skill = ScriptableObject.CreateInstance<SkillDefinition>();

            AssetDatabase.CreateAsset(skill, path);
            AssetDatabase.SaveAssets();

            RefreshSkillList();
            SelectSkill(skill);
        }

        private void DrawSkillDetail()
        {
            EditorGUILayout.BeginVertical();

            if (_selectedSkill == null)
            {
                EditorGUILayout.HelpBox("Select a skill on the left, or click 'New' to create one.", MessageType.Info);
                EditorGUILayout.EndVertical();
                return;
            }

            _selectedSerializedObject.Update();

            _detailScrollPosition = EditorGUILayout.BeginScrollView(_detailScrollPosition);

            EditorGUILayout.LabelField(_selectedSkill.DisplayName, EditorStyles.boldLabel);
            EditorGUILayout.Space();

            DrawSection("Identity", "_skillId", "_displayName", "_description", "_icon");
            DrawSection("Classification", "_skillType", "_category", "_element");
            DrawSection("Costs & Timing", "_cooldownSeconds", "_manaCost", "_energyCost", "_castTimeSeconds", "_recoverySeconds");
            DrawSection("Power", "_baseDamage", "_damagePerLevel", "_baseHealing", "_healingPerLevel", "_range", "_areaRadius");
            DrawBuffDebuffSection("Buffs", "_buffs");
            DrawBuffDebuffSection("Debuffs", "_debuffs");
            DrawSection("Presentation", "_animationTrigger", "_castSound", "_castParticlesPrefab", "_impactVfxPrefab");
            DrawSection("Progression", "_maxLevel", "_experienceToUnlock", "_requirement");

            EditorGUILayout.EndScrollView();

            if (GUI.changed)
            {
                _selectedSerializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(_selectedSkill);
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawSection(string title, params string[] propertyNames)
        {
            EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
            EditorGUI.indentLevel++;

            foreach (string propertyName in propertyNames)
            {
                SerializedProperty property = _selectedSerializedObject.FindProperty(propertyName);

                if (property != null)
                {
                    EditorGUILayout.PropertyField(property, true);
                }
            }

            EditorGUI.indentLevel--;
            EditorGUILayout.Space();
        }

        private void DrawBuffDebuffSection(string title, string arrayPropertyName)
        {
            SerializedProperty arrayProperty = _selectedSerializedObject.FindProperty(arrayPropertyName);

            if (arrayProperty == null)
            {
                return;
            }

            EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
            EditorGUI.indentLevel++;

            for (int i = 0; i < arrayProperty.arraySize; i++)
            {
                SerializedProperty element = arrayProperty.GetArrayElementAtIndex(i);

                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.PropertyField(element, new GUIContent($"Effect {i + 1}"), true);

                if (GUILayout.Button("Remove", GUILayout.Width(70)))
                {
                    arrayProperty.DeleteArrayElementAtIndex(i);
                    EditorGUILayout.EndVertical();
                    break;
                }

                EditorGUILayout.EndVertical();
            }

            if (GUILayout.Button($"+ Add {title.TrimEnd('s')}", GUILayout.Width(140)))
            {
                arrayProperty.arraySize++;
            }

            EditorGUI.indentLevel--;
            EditorGUILayout.Space();
        }
    }
}
