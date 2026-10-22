using System.Collections.Generic;
using System.IO;
using Rapadura.Gameplay.Environment;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Rapadura.Editor
{
    /// <summary>
    /// Builds LOD'd, appropriately-scaled prefabs from the 3 Meshy-generated tree species
    /// (Assets/Models/Environment/Trees/*.glb — raw output is 300k-800k+ vertices, far too heavy to
    /// scatter directly) and replaces the old capsule+sphere placeholder trees (Tree1/Tree2) with a
    /// scattered, varied planting drawn from those 3 species. See TODO.md "Revisão de qualidade
    /// visual do ambiente" for the full plan this executes (phase 2: trees).
    /// </summary>
    public static class NaturalTreeSetup
    {
        private const string TreesSourceFolder = "Assets/Models/Environment/Trees";
        private const string GeneratedFolder = "Assets/Models/Environment/Generated";
        private const string TreeMaterialsFolder = "Assets/Models/Environment/Trees/Materials";
        private const string PrefabFolder = "Assets/Prefabs/Environment";

        private static readonly string[] SpeciesFiles =
        {
            "Tree_Broadleaf_RO.glb",
            "Tree_Pine_RO.glb",
            "Tree_Gnarled_RO.glb",
        };

        /// <summary>Target world height (base to top) every generated tree prefab is normalized to, matching the scale the old placeholder trees/walls already established in Main.unity.</summary>
        private const float TargetHeight = 4.5f;

        [MenuItem("Rapadura/Setup/Build Natural Tree Prefabs")]
        public static void BuildPrefabs()
        {
            EnsureFolder(GeneratedFolder);
            EnsureFolder(PrefabFolder);

            var prefabs = new List<GameObject>();

            foreach (string file in SpeciesFiles)
            {
                GameObject prefab = BuildSpeciesPrefab(file);
                if (prefab != null)
                {
                    prefabs.Add(prefab);
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[NaturalTreeSetup] Built {prefabs.Count} tree species prefab(s) with LODs under {PrefabFolder}.");
        }

        [MenuItem("Rapadura/Setup/Scatter Natural Trees In Scene")]
        public static void ScatterInScene()
        {
            var prefabs = new List<GameObject>();
            foreach (string file in SpeciesFiles)
            {
                string speciesName = Path.GetFileNameWithoutExtension(file);
                string prefabPath = $"{PrefabFolder}/{speciesName}.prefab";
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                if (prefab == null)
                {
                    Debug.LogWarning($"[NaturalTreeSetup] Prefab not found at '{prefabPath}' — run 'Build Natural Tree Prefabs' first.");
                    continue;
                }
                prefabs.Add(prefab);
            }

            if (prefabs.Count == 0)
            {
                Debug.LogError("[NaturalTreeSetup] No tree prefabs available to scatter.");
                return;
            }

            Scene scene = SceneManager.GetActiveScene();

            // Remove the old placeholder trees entirely — real geometry replaces them, not just
            // their foliage (see TreeFoliageImprover.cs, the previous stop-gap for the same trees).
            int removed = 0;
            foreach (string oldName in new[] { "Tree1_Trunk", "Tree2_Trunk" })
            {
                GameObject old = GameObject.Find(oldName);
                if (old != null)
                {
                    Object.DestroyImmediate(old);
                    removed++;
                }
            }

            GameObject parent = GameObject.Find("NaturalTrees");
            if (parent == null)
            {
                parent = new GameObject("NaturalTrees");
            }
            else
            {
                for (int i = parent.transform.childCount - 1; i >= 0; i--)
                {
                    Object.DestroyImmediate(parent.transform.GetChild(i).gameObject);
                }
            }

            // Hand-picked spread positions rather than fully random across the whole 50x50 ground —
            // keeps trees within the area the player actually walks through in this test scene
            // (matching where Tree1/Tree2 and the rock cluster already were), while still reading as
            // a natural, non-grid-aligned planting.
            Vector2[] basePositions =
            {
                new Vector2(-10f, 10f), new Vector2(10f, 10f), new Vector2(-12f, -2f),
                new Vector2(12f, 4f), new Vector2(-6f, 12f), new Vector2(9f, -10f),
                new Vector2(-9f, -9f), new Vector2(2f, 13f), new Vector2(14f, -6f),
                new Vector2(-14f, 4f),
            };

            var random = new System.Random(777);

            for (int i = 0; i < basePositions.Length; i++)
            {
                GameObject speciesPrefab = prefabs[random.Next(prefabs.Count)];
                GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(speciesPrefab, parent.transform);

                Vector2 jitter = new Vector2((float)(random.NextDouble() * 2 - 1), (float)(random.NextDouble() * 2 - 1)) * 1.5f;
                Vector3 position = new Vector3(basePositions[i].x + jitter.x, 0f, basePositions[i].y + jitter.y);
                instance.transform.position = position;

                // Non-uniform-ish scale (mostly uniform with a little height/width independence),
                // random Y rotation, and a slight lean — breaks the "obviously the same prefab
                // copy-pasted" look the user explicitly flagged, without needing separate meshes.
                float scale = 0.75f + (float)random.NextDouble() * 0.6f;
                float heightBias = 0.9f + (float)random.NextDouble() * 0.25f;
                instance.transform.localScale = new Vector3(scale, scale * heightBias, scale);

                float lean = ((float)random.NextDouble() * 2f - 1f) * 6f;
                Vector3 leanAxis = new Vector3((float)random.NextDouble() * 2f - 1f, 0f, (float)random.NextDouble() * 2f - 1f).normalized;
                Quaternion leanRotation = Quaternion.AngleAxis(lean, leanAxis);
                Quaternion yawRotation = Quaternion.Euler(0f, (float)(random.NextDouble() * 360.0), 0f);
                instance.transform.rotation = yawRotation * leanRotation;

                instance.name = $"{speciesPrefab.name}_{i}";

                foreach (var renderer in instance.GetComponentsInChildren<Renderer>())
                {
                    var variation = renderer.gameObject.AddComponent<TreeColorVariation>();
                    variation.ApplyVariationForTests();
                }
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log($"[NaturalTreeSetup] Removed {removed} placeholder tree(s), scattered {basePositions.Length} real trees across {prefabs.Count} species in '{scene.name}'.");
        }

        private static GameObject BuildSpeciesPrefab(string sourceFile)
        {
            string sourcePath = $"{TreesSourceFolder}/{sourceFile}";
            var source = AssetDatabase.LoadAssetAtPath<GameObject>(sourcePath);
            if (source == null)
            {
                Debug.LogWarning($"[NaturalTreeSetup] Source model not found: {sourcePath}");
                return null;
            }

            string speciesName = Path.GetFileNameWithoutExtension(sourceFile);
            MeshFilter sourceMeshFilter = source.GetComponentInChildren<MeshFilter>();
            MeshRenderer sourceRenderer = source.GetComponentInChildren<MeshRenderer>();

            if (sourceMeshFilter == null || sourceRenderer == null)
            {
                Debug.LogWarning($"[NaturalTreeSetup] '{sourceFile}' has no mesh to process.");
                return null;
            }

            Mesh sourceMesh = sourceMeshFilter.sharedMesh;

            // The glb importer (glTFast) embeds its base-colour texture uncompressed (2048 RGB24,
            // ~12 MB per tree in a build). A standalone material + extracted, compressed PNG lives in
            // TreeMaterialsFolder; prefer it when present so regenerating trees keeps the small build.
            Material[] treeMaterials = sourceRenderer.sharedMaterials;
            var extractedMaterial = AssetDatabase.LoadAssetAtPath<Material>($"{TreeMaterialsFolder}/{Path.GetFileNameWithoutExtension(sourceFile)}.mat");
            if (extractedMaterial != null)
            {
                treeMaterials = new[] { extractedMaterial };
            }

            // Two LOD tiers: LOD0 keeps enough detail to read correctly up close (the grid-clustering
            // decimator in MeshDecimator.cs leaves faint seams at very aggressive ratios — confirmed
            // negligible at normal gameplay camera distance, but LOD0 stays gentler for whenever the
            // camera zooms in), LOD1 is aggressively cut for distant instances where nobody can tell.
            Mesh lod0 = MeshDecimator.Decimate(sourceMesh, targetVertexCount: 18000);
            Mesh lod1 = MeshDecimator.Decimate(sourceMesh, targetVertexCount: 3000);

            lod0.name = speciesName + "_LOD0";
            lod1.name = speciesName + "_LOD1";

            AssetDatabase.CreateAsset(lod0, $"{GeneratedFolder}/{speciesName}_LOD0.asset");
            AssetDatabase.CreateAsset(lod1, $"{GeneratedFolder}/{speciesName}_LOD1.asset");

            // Normalize scale/position: raw Meshy output varies in size/pivot per generation, so
            // every species needs to end up the same real-world height with its base at y=0
            // regardless of its native bounds.
            float rawHeight = Mathf.Max(sourceMesh.bounds.size.y, 0.01f);
            float normalizedScale = TargetHeight / rawHeight;
            float baseY = -sourceMesh.bounds.min.y * normalizedScale;

            var root = new GameObject(speciesName);
            var lodHolder = new GameObject("Model");
            lodHolder.transform.SetParent(root.transform, worldPositionStays: false);
            lodHolder.transform.localScale = Vector3.one * normalizedScale;
            lodHolder.transform.localPosition = new Vector3(0f, baseY, 0f);

            GameObject lod0Go = new GameObject("LOD0");
            lod0Go.transform.SetParent(lodHolder.transform, worldPositionStays: false);
            var lod0Filter = lod0Go.AddComponent<MeshFilter>();
            lod0Filter.sharedMesh = lod0;
            var lod0Renderer = lod0Go.AddComponent<MeshRenderer>();
            lod0Renderer.sharedMaterials = treeMaterials;

            GameObject lod1Go = new GameObject("LOD1");
            lod1Go.transform.SetParent(lodHolder.transform, worldPositionStays: false);
            var lod1Filter = lod1Go.AddComponent<MeshFilter>();
            lod1Filter.sharedMesh = lod1;
            var lod1Renderer = lod1Go.AddComponent<MeshRenderer>();
            lod1Renderer.sharedMaterials = treeMaterials;

            var lodGroup = root.AddComponent<LODGroup>();
            lodGroup.SetLODs(new[]
            {
                new LOD(0.25f, new[] { lod0Renderer }),
                new LOD(0.05f, new[] { lod1Renderer }),
            });
            lodGroup.RecalculateBounds();

            foreach (Material material in treeMaterials)
            {
                if (material != null)
                {
                    material.enableInstancing = true;
                    EditorUtility.SetDirty(material);
                }
            }

            string prefabPath = $"{PrefabFolder}/{speciesName}.prefab";
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            Object.DestroyImmediate(root);

            return prefab;
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
            {
                return;
            }

            string parent = Path.GetDirectoryName(path)?.Replace('\\', '/');
            string folderName = Path.GetFileName(path);
            if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent))
            {
                EnsureFolder(parent);
            }
            AssetDatabase.CreateFolder(parent, folderName);
        }
    }
}
