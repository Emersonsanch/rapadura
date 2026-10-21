using System.Collections.Generic;
using System.IO;
using Rapadura.Gameplay.Skills;
using UnityEditor;
using UnityEngine;

namespace Rapadura.Editor
{
    /// <summary>
    /// Builds a single <see cref="SkillTreeDefinition"/> laying out every seeded example skill
    /// (Resources/Skills/*.asset — 20 assets, see SkillDatabaseSeeder-family scripts from earlier
    /// sessions) in a simple grid, so the Skill Tree UI (SkillTreeController) has real data to draw
    /// instead of an empty screen. See TODO.md "sistema de skills inteiro nunca estava na cena".
    /// </summary>
    public static class SkillTreeSeeder
    {
        private const string OutputPath = "Assets/Resources/SkillTrees/tree_general.asset";
        private const int ColumnCount = 5;

        [MenuItem("Rapadura/Seed Example Skill Tree")]
        public static void Seed()
        {
            string folder = Path.GetDirectoryName(OutputPath)?.Replace('\\', '/');
            if (!string.IsNullOrEmpty(folder) && !AssetDatabase.IsValidFolder(folder))
            {
                string parent = Path.GetDirectoryName(folder)?.Replace('\\', '/');
                string name = Path.GetFileName(folder);
                if (!AssetDatabase.IsValidFolder(parent))
                {
                    AssetDatabase.CreateFolder("Assets/Resources", "SkillTrees");
                }
                else
                {
                    AssetDatabase.CreateFolder(parent, name);
                }
            }

            string[] guids = AssetDatabase.FindAssets("t:SkillDefinition", new[] { "Assets/Resources/Skills" });
            var nodes = new List<SkillTreeNodeData>();

            int index = 0;
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var skill = AssetDatabase.LoadAssetAtPath<SkillDefinition>(path);
                // FindAssets is recursive: skip the per-character signature skills under
                // Resources/Skills/Characters — they live in their own tree_<character> assets.
                if (skill == null || path.StartsWith("Assets/Resources/Skills/Characters/"))
                {
                    continue;
                }

                nodes.Add(new SkillTreeNodeData
                {
                    skill = skill,
                    gridPosition = new Vector2Int(index % ColumnCount, index / ColumnCount),
                });
                index++;
            }

            SkillTreeDefinition tree = AssetDatabase.LoadAssetAtPath<SkillTreeDefinition>(OutputPath);
            bool isNew = tree == null;
            if (isNew)
            {
                tree = ScriptableObject.CreateInstance<SkillTreeDefinition>();
            }

            var so = new SerializedObject(tree);
            so.FindProperty("_treeId").stringValue = "tree_general";
            so.FindProperty("_displayName").stringValue = "Árvore Geral";
            so.FindProperty("_description").stringValue = "Todas as habilidades de exemplo disponíveis, organizadas em grade.";
            SerializedProperty nodesProp = so.FindProperty("_nodes");
            nodesProp.arraySize = nodes.Count;
            for (int i = 0; i < nodes.Count; i++)
            {
                SerializedProperty nodeProp = nodesProp.GetArrayElementAtIndex(i);
                nodeProp.FindPropertyRelative("skill").objectReferenceValue = nodes[i].skill;
                nodeProp.FindPropertyRelative("gridPosition").vector2IntValue = nodes[i].gridPosition;
            }
            so.ApplyModifiedProperties();

            if (isNew)
            {
                AssetDatabase.CreateAsset(tree, OutputPath);
            }
            else
            {
                EditorUtility.SetDirty(tree);
            }

            AssetDatabase.SaveAssets();
            Debug.Log($"[SkillTreeSeeder] Seeded '{OutputPath}' with {nodes.Count} skill node(s).");
        }
    }
}
