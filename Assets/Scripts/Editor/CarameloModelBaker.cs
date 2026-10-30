using Rapadura.Gameplay.Enemies.Visuals;
using Rapadura.Gameplay.Pets.Visuals;
using UnityEditor;
using UnityEngine;

namespace Rapadura.Editor
{
    /// <summary>
    /// Bakes Caramelo's code-built model (<see cref="CarameloModel"/>) into assets, the same way
    /// <see cref="ProceduralEnemyModelBaker"/> bakes the enemies: the part meshes into
    /// <c>Assets/Models/Pets/Caramelo_Meshes.asset</c> and the prefab into <c>Assets/Resources/Pets/Caramelo.prefab</c>,
    /// which <see cref="CarameloModel.Instantiate(Transform)"/> then prefers over building at runtime. It uses
    /// the enemies' baked palette material (<c>Assets/Models/Enemies/Procedural/EnemyPalette.mat</c>); if that
    /// is missing it runs the enemy bake first. Optional: without it the same model is built at runtime.
    /// Safe to re-run (the prefab is overwritten in place, GUID kept).
    /// Run via Rapadura > Pets > Build Caramelo Model.
    /// </summary>
    public static class CarameloModelBaker
    {
        public const string MeshFolder = "Assets/Models/Pets";
        public const string PrefabFolder = "Assets/Resources/Pets";
        private const string MaterialPath = ProceduralEnemyModelBaker.Folder + "/EnemyPalette.mat";

        [MenuItem("Rapadura/Pets/Build Caramelo Model")]
        public static void Build()
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
            if (material == null)
            {
                ProceduralEnemyModelBaker.BuildAll();
                material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
            }

            if (material == null)
            {
                Debug.LogError($"[CarameloModelBaker] {MaterialPath} not found even after the enemy bake; aborting.");
                return;
            }

            EnsureFolder("Assets/Models", "Pets");
            EnsureFolder("Assets/Resources", "Pets");

            string meshPath = MeshFolder + "/Caramelo_Meshes.asset";
            string prefabPath = PrefabFolder + "/Caramelo.prefab";

            ProceduralEnemyBlueprint blueprint = CarameloModel.CreateBlueprint();
            int triangles = blueprint.TriangleCount;

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

            GameObject instance = CarameloModel.Instantiate(blueprint, material, null);
            instance.name = "Caramelo";
            PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);
            Object.DestroyImmediate(instance);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[CarameloModelBaker] Baked Caramelo ({triangles} tris) → {prefabPath}");
        }

        private static void EnsureFolder(string parent, string name)
        {
            if (!AssetDatabase.IsValidFolder(parent + "/" + name))
            {
                AssetDatabase.CreateFolder(parent, name);
            }
        }
    }
}
