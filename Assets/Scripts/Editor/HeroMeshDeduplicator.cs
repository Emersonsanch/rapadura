using System;
using System.Collections.Generic;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Player;
using Rapadura.UI.CharacterSelect;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Rapadura.Editor
{
    /// <summary>
    /// Ships each hero mesh once (menu <c>Rapadura ▸ Build ▸ Dedupe Hero Meshes</c>). For a hero with a
    /// cleaned no-pedestal mesh (<see cref="PedestalMeshUtility"/>), the build used to carry the rigged
    /// FBX mesh (scene references), the cleaned copy (Resources) and, for Joaquim, the static
    /// <c>Joaquim_RO.fbx</c> preview mesh as well, though at runtime every one of them was swapped for
    /// the cleaned mesh. This makes a prefab variant of the rigged FBX whose SkinnedMeshRenderer already
    /// uses the cleaned mesh, and points the build scenes at it:
    /// <list type="bullet">
    /// <item><see cref="PlayerVisualSwapper"/> and <see cref="CharacterPreviewController"/> entries that
    /// used the rigged FBX now use the variant (the world-map token reads the preview entries).</item>
    /// <item>A preview entry that used a static (MeshFilter) model for such a hero uses the variant with
    /// the rigged heroes' placement (0, 180, 0), scale 1 — auto-framing sizes it; the mesh shown is the
    /// same cleaned mesh, in the same bind pose and facing.</item>
    /// <item>Scene SkinnedMeshRenderers still on the rigged FBX mesh (the hand-placed Joaquim
    /// "PlayerVisual") get the cleaned mesh as an override — what <see cref="PlayerVisualSwapper"/>
    /// did at runtime anyway.</item>
    /// </list>
    /// The FBX mesh is then referenced by nothing in the build and drops out. Idempotent. Scenes open
    /// with unsaved changes are skipped (save them first).
    /// </summary>
    public static class HeroMeshDeduplicator
    {
        public const string MenuPath = "Rapadura/Build/Dedupe Hero Meshes";
        public const string VariantFolder = "Assets/Prefabs/Characters";
        public static readonly Vector3 RiggedPreviewEuler = new Vector3(0f, 180f, 0f);

        public static string VariantPathFor(CharacterId id) => $"{VariantFolder}/{id}_Rigged_NoPedestal.prefab";

        [MenuItem(MenuPath)]
        public static void DedupeFromMenu()
        {
            Dictionary<CharacterId, GameObject> variants = EnsureVariants();
            if (variants.Count == 0)
            {
                Debug.LogWarning("[HeroMeshDedupe] No hero has both a cleaned mesh and a rigged FBX; nothing to do.");
                return;
            }

            int changes = 0;
            foreach (EditorBuildSettingsScene buildScene in EditorBuildSettings.scenes)
            {
                if (buildScene.enabled)
                {
                    changes += RewireScene(buildScene.path, variants);
                }
            }

            Debug.Log($"[HeroMeshDedupe] {variants.Count} hero variant(s); {changes} scene reference(s) changed. Run again: 0 expected.");
        }

        /// <summary>Creates or refreshes one variant per hero that has a cleaned mesh and a rigged FBX.</summary>
        public static Dictionary<CharacterId, GameObject> EnsureVariants()
        {
            var variants = new Dictionary<CharacterId, GameObject>();
            foreach (CharacterId id in Enum.GetValues(typeof(CharacterId)))
            {
                Mesh cleaned = PedestalMeshUtility.LoadCleanedMesh(id);
                GameObject rigged = AssetDatabase.LoadAssetAtPath<GameObject>(MeshImportSettingsApplier.SourceModelPathFor(id));
                if (cleaned == null || rigged == null || rigged.GetComponentInChildren<SkinnedMeshRenderer>(true) == null)
                {
                    continue;
                }

                GameObject variant = EnsureVariant(id, rigged, cleaned);
                if (variant != null)
                {
                    variants[id] = variant;
                }
            }

            return variants;
        }

        private static GameObject EnsureVariant(CharacterId id, GameObject rigged, Mesh cleaned)
        {
            string path = VariantPathFor(id);
            GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null && UsesOnly(existing, cleaned))
            {
                return existing;
            }

            if (!AssetDatabase.IsValidFolder(VariantFolder))
            {
                AssetDatabase.CreateFolder("Assets/Prefabs", "Characters");
            }

            // Saving a prefab *instance* root produces a Prefab Variant of the FBX model prefab, so the
            // rig, avatar and materials keep following the FBX; only m_Mesh is overridden.
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(rigged);
            try
            {
                foreach (SkinnedMeshRenderer smr in instance.GetComponentsInChildren<SkinnedMeshRenderer>(true))
                {
                    smr.sharedMesh = cleaned;
                }

                GameObject saved = PrefabUtility.SaveAsPrefabAsset(instance, path, out bool success);
                Debug.Log($"[HeroMeshDedupe] {(success ? "Saved" : "FAILED to save")} {path} (variant of {AssetDatabase.GetAssetPath(rigged)}, mesh {cleaned.name}).");
                return success ? saved : null;
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(instance);
            }
        }

        private static bool UsesOnly(GameObject prefab, Mesh cleaned)
        {
            SkinnedMeshRenderer[] renderers = prefab.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            if (renderers.Length == 0) return false;
            foreach (SkinnedMeshRenderer smr in renderers)
            {
                if (smr.sharedMesh != cleaned) return false;
            }

            return true;
        }

        /// <summary>Applies the rewiring to one scene (opened additively when needed). Returns the number of changes.</summary>
        public static int RewireScene(string scenePath, Dictionary<CharacterId, GameObject> variants)
        {
            Scene scene = SceneManager.GetSceneByPath(scenePath);
            bool openedHere = false;
            if (!scene.IsValid() || !scene.isLoaded)
            {
                scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
                openedHere = true;
            }
            else if (scene.isDirty)
            {
                Debug.LogError($"[HeroMeshDedupe] {scenePath} has unsaved changes; save it and run again. Skipped.");
                return 0;
            }

            int changes = 0;
            try
            {
                var heroMeshes = new Dictionary<Mesh, Mesh>(); // rigged FBX mesh -> cleaned mesh
                foreach (KeyValuePair<CharacterId, GameObject> kv in variants)
                {
                    GameObject rigged = AssetDatabase.LoadAssetAtPath<GameObject>(MeshImportSettingsApplier.SourceModelPathFor(kv.Key));
                    Mesh cleaned = PedestalMeshUtility.LoadCleanedMesh(kv.Key);
                    foreach (SkinnedMeshRenderer smr in rigged.GetComponentsInChildren<SkinnedMeshRenderer>(true))
                    {
                        if (smr.sharedMesh != null) heroMeshes[smr.sharedMesh] = cleaned;
                    }
                }

                foreach (GameObject root in scene.GetRootGameObjects())
                {
                    foreach (PlayerVisualSwapper swapper in root.GetComponentsInChildren<PlayerVisualSwapper>(true))
                    {
                        changes += RewireEntries(swapper, variants, allowStaticSwap: false);
                    }

                    foreach (CharacterPreviewController preview in root.GetComponentsInChildren<CharacterPreviewController>(true))
                    {
                        changes += RewireEntries(preview, variants, allowStaticSwap: true);
                    }

                    foreach (SkinnedMeshRenderer smr in root.GetComponentsInChildren<SkinnedMeshRenderer>(true))
                    {
                        if (smr.sharedMesh != null && heroMeshes.TryGetValue(smr.sharedMesh, out Mesh cleaned))
                        {
                            Undo.RecordObject(smr, "Use cleaned hero mesh");
                            smr.sharedMesh = cleaned;
                            PrefabUtility.RecordPrefabInstancePropertyModifications(smr);
                            changes++;
                        }
                    }
                }

                if (changes > 0)
                {
                    EditorSceneManager.MarkSceneDirty(scene);
                    EditorSceneManager.SaveScene(scene);
                    Debug.Log($"[HeroMeshDedupe] {scenePath}: {changes} change(s) saved.");
                }
            }
            finally
            {
                if (openedHere)
                {
                    EditorSceneManager.CloseScene(scene, true);
                }
            }

            return changes;
        }

        private static int RewireEntries(MonoBehaviour owner, Dictionary<CharacterId, GameObject> variants, bool allowStaticSwap)
        {
            var so = new SerializedObject(owner);
            SerializedProperty entries = so.FindProperty("_entries");
            if (entries == null || !entries.isArray) return 0;

            int changes = 0;
            for (int i = 0; i < entries.arraySize; i++)
            {
                SerializedProperty entry = entries.GetArrayElementAtIndex(i);
                var id = (CharacterId)entry.FindPropertyRelative("characterId").intValue;
                if (!variants.TryGetValue(id, out GameObject variant)) continue;

                SerializedProperty model = entry.FindPropertyRelative("modelPrefab");
                var current = model.objectReferenceValue as GameObject;
                if (current == null || current == variant) continue;

                string currentPath = AssetDatabase.GetAssetPath(current);
                bool isRiggedSource = currentPath == MeshImportSettingsApplier.SourceModelPathFor(id);
                bool isStatic = current.GetComponentInChildren<SkinnedMeshRenderer>(true) == null;
                if (isRiggedSource)
                {
                    model.objectReferenceValue = variant;
                }
                else if (allowStaticSwap && isStatic)
                {
                    model.objectReferenceValue = variant;
                    entry.FindPropertyRelative("localEulerAngles").vector3Value = RiggedPreviewEuler;
                    entry.FindPropertyRelative("uniformScale").floatValue = 1f;
                }
                else
                {
                    continue;
                }

                Debug.Log($"[HeroMeshDedupe] {owner.GetType().Name} on '{owner.name}': {id} {currentPath} -> {VariantPathFor(id)}");
                changes++;
            }

            if (changes > 0)
            {
                so.ApplyModifiedPropertiesWithoutUndo();
            }

            return changes;
        }
    }
}
