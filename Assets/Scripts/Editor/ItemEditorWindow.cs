using System.Collections.Generic;
using System.IO;
using System.Linq;
using Rapadura.Gameplay.Items;
using UnityEditor;
using UnityEngine;

namespace Rapadura.Editor
{
    /// <summary>
    /// Visual editor for creating and tuning ItemDefinition assets without writing code.
    /// Mirrors SkillEditorWindow: a searchable list on the left, full editable form on the
    /// right. All example items live under Resources/Items so ItemDatabase can find them.
    /// Open via Rapadura > Item Editor.
    /// </summary>
    public class ItemEditorWindow : EditorWindow
    {
        private const string DefaultItemFolder = "Assets/Resources/Items";

        [SerializeField] private ItemDefinition _selectedItem;
        private SerializedObject _selectedSerializedObject;
        private Vector2 _listScrollPosition;
        private Vector2 _detailScrollPosition;
        private string _searchFilter = string.Empty;
        private List<ItemDefinition> _allItems = new List<ItemDefinition>();

        [MenuItem("Rapadura/Item Editor")]
        public static void Open()
        {
            ItemEditorWindow window = GetWindow<ItemEditorWindow>("Item Editor");
            window.minSize = new Vector2(760, 480);
            window.RefreshItemList();
        }

        private void OnEnable()
        {
            RefreshItemList();
        }

        private void RefreshItemList()
        {
            _allItems = AssetDatabase.FindAssets("t:ItemDefinition")
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<ItemDefinition>)
                .Where(item => item != null)
                .OrderBy(item => item.DisplayName)
                .ToList();
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();
            DrawItemList();
            DrawItemDetail();
            EditorGUILayout.EndHorizontal();
        }

        private void DrawItemList()
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(260));

            EditorGUILayout.LabelField("Items", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            _searchFilter = EditorGUILayout.TextField(_searchFilter, EditorStyles.toolbarSearchField);
            if (GUILayout.Button("New", GUILayout.Width(45)))
            {
                CreateNewItem();
            }
            EditorGUILayout.EndHorizontal();

            _listScrollPosition = EditorGUILayout.BeginScrollView(_listScrollPosition);

            foreach (ItemDefinition item in _allItems)
            {
                if (item == null)
                {
                    continue;
                }

                if (!string.IsNullOrEmpty(_searchFilter) &&
                    item.DisplayName.IndexOf(_searchFilter, System.StringComparison.OrdinalIgnoreCase) < 0)
                {
                    continue;
                }

                bool isSelected = item == _selectedItem;
                GUIStyle style = isSelected ? EditorStyles.whiteLabel : EditorStyles.label;

                EditorGUILayout.BeginHorizontal(isSelected ? "SelectionRect" : GUIStyle.none);
                if (GUILayout.Button($"[{item.Rarity}] {item.DisplayName}", style, GUILayout.Height(20)))
                {
                    SelectItem(item);
                }
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void SelectItem(ItemDefinition item)
        {
            _selectedItem = item;
            _selectedSerializedObject = new SerializedObject(item);
        }

        private void CreateNewItem()
        {
            if (!Directory.Exists(DefaultItemFolder))
            {
                Directory.CreateDirectory(DefaultItemFolder);
            }

            string path = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(DefaultItemFolder, "NewItem.asset"));
            var item = ScriptableObject.CreateInstance<ItemDefinition>();

            AssetDatabase.CreateAsset(item, path);
            AssetDatabase.SaveAssets();

            RefreshItemList();
            SelectItem(item);
            ItemDatabase.Invalidate();
        }

        private void DrawItemDetail()
        {
            EditorGUILayout.BeginVertical();

            if (_selectedItem == null)
            {
                EditorGUILayout.HelpBox("Select an item on the left, or click 'New' to create one.", MessageType.Info);
                EditorGUILayout.EndVertical();
                return;
            }

            _selectedSerializedObject.Update();

            _detailScrollPosition = EditorGUILayout.BeginScrollView(_detailScrollPosition);

            EditorGUILayout.LabelField(_selectedItem.DisplayName, EditorStyles.boldLabel);
            EditorGUILayout.Space();

            DrawSection("Identity", "_itemId", "_displayName", "_description", "_icon", "_worldModelPrefab");
            DrawSection("Classification", "_type", "_category", "_rarity");
            DrawSection("Economy & Stacking", "_weight", "_value", "_maxStack");
            DrawSection("Durability", "_hasDurability", "_maxDurability");

            SerializedProperty equipSlotProp = _selectedSerializedObject.FindProperty("_equipSlot");
            bool isEquippable = equipSlotProp != null && equipSlotProp.enumValueIndex != (int)EquipmentSlot.None;

            DrawSection("Equipment", "_equipSlot");
            if (isEquippable)
            {
                EditorGUI.indentLevel++;
                DrawSection(null, "_armorDefense", "_weaponDamage", "_weaponAttackSpeed");
                DrawBuffArraySection("Equip Stat Modifiers", "_equipStatModifiers");
                EditorGUI.indentLevel--;
            }

            SerializedProperty typeProp = _selectedSerializedObject.FindProperty("_type");
            bool isConsumable = typeProp != null && typeProp.enumValueIndex == (int)ItemType.Consumable;

            if (isConsumable)
            {
                DrawSection("Consumable Effects", "_healthRestore", "_staminaRestore", "_manaRestore");
                DrawBuffArraySection("Consume Buffs", "_consumeBuffs");
            }

            EditorGUILayout.EndScrollView();

            if (GUI.changed)
            {
                _selectedSerializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(_selectedItem);
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawSection(string title, params string[] propertyNames)
        {
            if (!string.IsNullOrEmpty(title))
            {
                EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
            }

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

        private void DrawBuffArraySection(string title, string arrayPropertyName)
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
                EditorGUILayout.PropertyField(element, new GUIContent($"Modifier {i + 1}"), true);

                if (GUILayout.Button("Remove", GUILayout.Width(70)))
                {
                    arrayProperty.DeleteArrayElementAtIndex(i);
                    EditorGUILayout.EndVertical();
                    break;
                }

                EditorGUILayout.EndVertical();
            }

            if (GUILayout.Button($"+ Add Modifier", GUILayout.Width(140)))
            {
                arrayProperty.arraySize++;
            }

            EditorGUI.indentLevel--;
            EditorGUILayout.Space();
        }
    }
}
