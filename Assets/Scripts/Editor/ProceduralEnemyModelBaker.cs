using System.Collections.Generic;
using System.IO;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Enemies.Visuals;
using UnityEditor;
using UnityEngine;

namespace Rapadura.Editor
{
    /// <summary>
    /// Bakes the code-built enemy models (<see cref="ProceduralEnemyModels"/>) into assets under
    /// Assets/Models/Enemies/Procedural: the palette textures and the shared material, one mesh asset
    /// per enemy (all its part meshes as sub-assets) and one prefab per enemy. Then it assigns
    /// each prefab to its EnemyDefinition's <c>_visualPrefab</c> with an identity visual transform
    /// (position 0, rotation 0, scale 1; EnemyVisualSwapper seats it on the ground by mesh bounds).
    /// You don't have to bake: without it the same models are built at runtime and cached. Baking
    /// makes them visible and editable as assets, and puts the emissive palette material in builds,
    /// which keeps the URP _EMISSION variant so eyes, cracks, runes and crystals glow.
    /// Safe to re-run: the prefabs are overwritten in place, so their GUIDs and references don't change.
    /// Run via Rapadura > Enemies > Build Procedural Enemy Models.
    /// </summary>
    public static class ProceduralEnemyModelBaker
    {
        public const string Folder = "Assets/Models/Enemies/Procedural";
        private const string EnemyFolder = "Assets/Resources/Enemies";

        [MenuItem("Rapadura/Enemies/Build Procedural Enemy Models")]
        public static void BuildAll()
        {
            EnsureFolder(Folder);
            Material material = BakeMaterial();

            var report = new List<string>();
            foreach (KeyValuePair<string, ProceduralEnemyKind> entry in ProceduralEnemyModels.KindsById)
            {
                GameObject prefab = BakeKind(entry.Value, material, out int triangles);
                bool assigned = AssignToDefinition(entry.Key, prefab);
                report.Add($"{entry.Key}: {entry.Value} ({triangles} tris){(assigned ? string.Empty : " - definition NOT found")}");
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[ProceduralEnemyModelBaker] Baked procedural enemy models:\n" + string.Join("\n", report));
        }

        private static Material BakeMaterial()
        {
            string texturePath = Folder + "/EnemyPalette_Textures.asset";
            string materialPath = Folder + "/EnemyPalette.mat";

            AssetDatabase.DeleteAsset(texturePath);
            Texture2D albedo = EnemyModelPalette.CreateTexture();
            Texture2D emission = EnemyModelPalette.CreateEmissionTexture();
            AssetDatabase.CreateAsset(albedo, texturePath);
            AssetDatabase.AddObjectToAsset(emission, texturePath);

            Material fresh = EnemyModelPalette.CreateMaterial(albedo, emission);
            Material existing = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if (existing != null)
            {
                // Keep the material's GUID (prefabs reference it): copy the new settings over.
                existing.shader = fresh.shader;
                existing.CopyPropertiesFromMaterial(fresh);
                existing.shaderKeywords = fresh.shaderKeywords;
                existing.globalIlluminationFlags = fresh.globalIlluminationFlags;
                Object.DestroyImmediate(fresh);
                EditorUtility.SetDirty(existing);
                return existing;
            }

            AssetDatabase.CreateAsset(fresh, materialPath);
            return fresh;
        }

        private static GameObject BakeKind(ProceduralEnemyKind kind, Material material, out int triangles)
        {
            string meshPath = $"{Folder}/{kind}_Meshes.asset";
            string prefabPath = $"{Folder}/{kind}.prefab";

            ProceduralEnemyBlueprint blueprint = ProceduralEnemyModels.CreateBlueprint(kind);
            triangles = blueprint.TriangleCount;

            AssetDatabase.DeleteAsset(meshPath);
            for (int i = 0; i < blueprint.Meshes.Count; i++)
            {
                if (i == 0)
                {
                    AssetDatabase.CreateAsset(blueprint.Meshes[i], meshPath);
                }
                else
                {
                    AssetDatabase.AddObjectToAsset(blueprint.Meshes[i], meshPath);
                }
            }

            // The meshes are now persistent assets, so the instance references them directly.
            GameObject instance = ProceduralEnemyModels.Instantiate(blueprint, material, null);
            instance.name = kind.ToString();
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);
            Object.DestroyImmediate(instance);
            return prefab;
        }

        private static bool AssignToDefinition(string enemyId, GameObject prefab)
        {
            var definition = AssetDatabase.LoadAssetAtPath<EnemyDefinition>($"{EnemyFolder}/{enemyId}.asset");
            if (definition == null)
            {
                Debug.LogWarning($"[ProceduralEnemyModelBaker] {EnemyFolder}/{enemyId}.asset not found; prefab baked but not assigned.");
                return false;
            }

            var so = new SerializedObject(definition);
            so.FindProperty("_visualPrefab").objectReferenceValue = prefab;
            so.FindProperty("_visualLocalPosition").vector3Value = Vector3.zero;
            so.FindProperty("_visualLocalEulerAngles").vector3Value = Vector3.zero;
            so.FindProperty("_visualScale").floatValue = 1f;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(definition);
            return true;
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
            {
                return;
            }

            string parent = Path.GetDirectoryName(path)?.Replace('\\', '/');
            if (!string.IsNullOrEmpty(parent))
            {
                EnsureFolder(parent);
            }

            AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
        }
    }
}
