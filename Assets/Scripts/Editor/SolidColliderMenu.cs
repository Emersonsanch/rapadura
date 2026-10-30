using System.IO;
using Rapadura.Gameplay.Environment;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Rapadura.Editor
{
    /// <summary>
    /// Bakes <see cref="SolidPropBinder"/> colliders into assets so the runtime load pass has nothing
    /// left to compute: first the environment prefabs (Assets/Prefabs/Environment — the tree species
    /// every scene/arena/map instance comes from), then every open scene (refit, so stale or
    /// hand-tuned SolidCollider children are recomputed; authored colliders are kept unless they are
    /// a stale undersized rock collider). The scene is marked dirty, not saved — save it after
    /// checking the result. See Docs/Collision.md.
    /// </summary>
    public static class SolidColliderMenu
    {
        private const string EnvironmentPrefabFolder = "Assets/Prefabs/Environment";

        [MenuItem("Rapadura/Environment/Apply Solid Colliders To Open Scene")]
        public static void ApplyToOpenScene()
        {
            int prefabs = ApplyToEnvironmentPrefabs();

            SolidPropBinder.Report report = SolidPropBinder.BindAll(refit: true);
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (scene.isLoaded)
                {
                    EditorSceneManager.MarkSceneDirty(scene);
                }
            }

            Debug.Log($"[SolidColliderMenu] {prefabs} environment prefab(s) updated; open scene(s): {report}. Save the scene to keep it.");
        }

        [MenuItem("Rapadura/Environment/Apply Solid Colliders To Environment Prefabs")]
        public static void ApplyToEnvironmentPrefabsMenu()
        {
            int count = ApplyToEnvironmentPrefabs();
            Debug.Log($"[SolidColliderMenu] {count} environment prefab(s) updated under {EnvironmentPrefabFolder}.");
        }

        /// <summary>Binds each prefab under <see cref="EnvironmentPrefabFolder"/>; returns how many ended up with a solid collider.</summary>
        public static int ApplyToEnvironmentPrefabs()
        {
            if (!AssetDatabase.IsValidFolder(EnvironmentPrefabFolder))
            {
                return 0;
            }

            int count = 0;
            foreach (string guid in AssetDatabase.FindAssets("t:Prefab", new[] { EnvironmentPrefabFolder }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (Path.GetExtension(path) != ".prefab")
                {
                    continue;
                }

                GameObject contents = PrefabUtility.LoadPrefabContents(path);
                try
                {
                    SolidPropBinder.Report report = SolidPropBinder.BindHierarchy(contents.transform, refit: true);
                    if (report.Solid > 0)
                    {
                        PrefabUtility.SaveAsPrefabAsset(contents, path);
                        count++;
                    }
                }
                finally
                {
                    PrefabUtility.UnloadPrefabContents(contents);
                }
            }

            AssetDatabase.SaveAssets();
            return count;
        }
    }
}
