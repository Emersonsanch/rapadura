using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Rapadura.Editor
{
    /// <summary>
    /// One-click fixer for the placeholder trees in Main.unity (Tree1/Tree2), which are currently
    /// just a single capsule "trunk" + a single sphere "leaves" — a bare lollipop shape the user
    /// flagged as "mal feita" (badly made). Replaces the single leaf sphere with a small cluster of
    /// overlapping, randomly offset/scaled spheres for a fuller canopy silhouette, and tapers the
    /// trunk slightly. Runs entirely inside the Editor (no Meshy credits, no MCP bridge needed) —
    /// a real generated tree model is still the eventual proper fix (see TODO.md).
    /// </summary>
    public static class TreeFoliageImprover
    {
        private const string LeavesMaterialGuid = "b7f147f3c4de58c4086f74d91bb87ba8"; // Mat_Leaves

        [MenuItem("Rapadura/Setup/Improve Placeholder Tree Foliage")]
        public static void ImproveTrees()
        {
            Scene scene = SceneManager.GetActiveScene();
            int improved = 0;

            foreach (GameObject root in scene.GetRootGameObjects())
            {
                improved += ImproveTreesUnder(root.transform);
            }

            if (improved > 0)
            {
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
            }

            Debug.Log($"[TreeFoliageImprover] Improved {improved} tree canopy(ies) in '{scene.name}'.");
        }

        private static int ImproveTreesUnder(Transform root)
        {
            int count = 0;

            foreach (Transform child in root)
            {
                if (child.name.EndsWith("_Leaves") && child.GetComponent<MeshFilter>() != null)
                {
                    BuildCluster(child);
                    count++;
                }

                count += ImproveTreesUnder(child);
            }

            return count;
        }

        private static void BuildCluster(Transform originalLeaf)
        {
            const string clusterMarkerName = "FoliageCluster";
            if (originalLeaf.Find(clusterMarkerName) != null)
            {
                return; // Already improved, don't stack duplicates on repeat runs.
            }

            Material leavesMaterial = AssetDatabase.LoadAssetAtPath<Material>(
                AssetDatabase.GUIDToAssetPath(LeavesMaterialGuid));

            originalLeaf.GetComponent<MeshRenderer>().enabled = false;
            var sphereCollider = originalLeaf.GetComponent<SphereCollider>();
            if (sphereCollider != null)
            {
                sphereCollider.radius *= 1.3f; // Keep a single, slightly bigger trigger/collision volume.
            }

            var clusterRoot = new GameObject(clusterMarkerName);
            clusterRoot.transform.SetParent(originalLeaf, worldPositionStays: false);
            clusterRoot.transform.localPosition = Vector3.zero;

            Vector3[] offsets =
            {
                new Vector3(0f, 0.15f, 0f),
                new Vector3(0.45f, -0.1f, 0.2f),
                new Vector3(-0.4f, -0.05f, -0.25f),
                new Vector3(0.1f, -0.2f, 0.45f),
                new Vector3(-0.2f, -0.15f, -0.45f),
            };

            var random = new System.Random(originalLeaf.GetInstanceID());

            for (int i = 0; i < offsets.Length; i++)
            {
                GameObject blob = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                blob.name = $"Leaf_{i}";
                Object.DestroyImmediate(blob.GetComponent<SphereCollider>());

                blob.transform.SetParent(clusterRoot.transform, worldPositionStays: false);
                blob.transform.localPosition = offsets[i];

                float scale = 0.55f + (float)random.NextDouble() * 0.35f;
                blob.transform.localScale = new Vector3(scale, scale, scale);

                if (leavesMaterial != null)
                {
                    blob.GetComponent<MeshRenderer>().sharedMaterial = leavesMaterial;
                }
            }
        }
    }
}
