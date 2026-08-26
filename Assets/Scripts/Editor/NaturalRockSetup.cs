using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Rapadura.Environment;

namespace Rapadura.Editor
{
    /// <summary>
    /// Replaces the placeholder rocks in Main.unity — flattened/rotated sphere primitives, all
    /// visually identical apart from squash/rotation — with real irregular geometry from
    /// <see cref="ProceduralRockGenerator"/>, and scatters additional small/medium/large rocks for
    /// natural, non-repetitive distribution. Zero external asset/API cost (see TODO.md "Revisão de
    /// qualidade visual do ambiente" for the full cost-comparison plan this executes).
    /// </summary>
    public static class NaturalRockSetup
    {
        [MenuItem("Rapadura/Setup/Regenerate Natural Rocks")]
        public static void Regenerate()
        {
            Scene scene = SceneManager.GetActiveScene();
            Material rockMaterial = FindMaterial("Mat_Rock");

            if (rockMaterial != null)
            {
                rockMaterial.enableInstancing = true;
                EditorUtility.SetDirty(rockMaterial);
            }

            int replaced = 0;
            replaced += ReplaceRock(GameObject.Find("Rock1"), rockMaterial, seedBase: 1001);
            replaced += ReplaceRock(GameObject.Find("Rock2"), rockMaterial, seedBase: 1002);

            GameObject rocksParent = GameObject.Find("Rocks");
            if (rocksParent != null)
            {
                for (int i = 0; i < rocksParent.transform.childCount; i++)
                {
                    Transform child = rocksParent.transform.GetChild(i);
                    replaced += ReplaceRock(child.gameObject, rockMaterial, seedBase: 2000 + i);
                }

                ScatterAdditionalRocks(rocksParent.transform, rockMaterial, count: 10, seedBase: 5000);
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log($"[NaturalRockSetup] Replaced {replaced} rock mesh(es) with procedural geometry and scattered additional rocks in '{scene.name}'.");
        }

        private static int ReplaceRock(GameObject rock, Material material, int seedBase)
        {
            if (rock == null)
            {
                return 0;
            }

            MeshFilter meshFilter = rock.GetComponent<MeshFilter>();
            if (meshFilter == null)
            {
                return 0;
            }

            // Keep the existing hand-placed position/rotation/overall scale (it already reads fine
            // as "a rock roughly this big here") — only the mesh itself changes, from a squashed
            // sphere to real irregular geometry with a per-rock-unique seed.
            int subdivisions = 2;
            float noiseStrength = Random.Range(0.3f, 0.45f);
            Mesh mesh = ProceduralRockGenerator.BuildRockMesh(seedBase, subdivisions, noiseStrength, squashY: 1f);
            meshFilter.sharedMesh = mesh;

            var collider = rock.GetComponent<SphereCollider>();
            if (collider != null)
            {
                Object.DestroyImmediate(collider);
                var meshCollider = rock.AddComponent<MeshCollider>();
                meshCollider.sharedMesh = mesh;
                meshCollider.convex = true;
            }

            if (material != null)
            {
                rock.GetComponent<MeshRenderer>().sharedMaterial = material;
            }

            EditorUtility.SetDirty(rock);
            return 1;
        }

        /// <summary>
        /// Adds a handful of small/medium/large rocks around the existing cluster so the ground
        /// doesn't read as "exactly 7 rocks placed by a designer" — natural terrain has irregular,
        /// denser scatter. Positions are randomized within a radius of the existing rock cluster's
        /// centroid rather than the whole 50x50 ground, so they stay in the area the player
        /// actually walks through in this test scene.
        /// </summary>
        private static void ScatterAdditionalRocks(Transform parent, Material material, int count, int seedBase)
        {
            Vector3 centroid = Vector3.zero;
            int existingCount = 0;
            foreach (Transform child in parent)
            {
                centroid += child.position;
                existingCount++;
            }
            if (existingCount > 0)
            {
                centroid /= existingCount;
            }

            var random = new System.Random(seedBase);

            for (int i = 0; i < count; i++)
            {
                float angle = (float)(random.NextDouble() * Mathf.PI * 2f);
                float distance = 3f + (float)random.NextDouble() * 9f;
                Vector3 position = centroid + new Vector3(Mathf.Cos(angle) * distance, 0f, Mathf.Sin(angle) * distance);

                // Small/medium/large tiers, weighted toward small (natural fields have many small
                // rocks and few boulders) — matches the user's request for varied sizes.
                float sizeRoll = (float)random.NextDouble();
                float baseScale = sizeRoll < 0.55f ? Random.Range(0.35f, 0.6f)
                    : sizeRoll < 0.85f ? Random.Range(0.6f, 1.1f)
                    : Random.Range(1.1f, 1.8f);

                var go = new GameObject($"Rock_Scattered_{i}");
                go.transform.SetParent(parent, worldPositionStays: false);
                position.y = baseScale * 0.25f; // roughly half-buried, matching the hand-placed rocks' feel
                go.transform.position = position;
                go.transform.rotation = Quaternion.Euler(0f, (float)(random.NextDouble() * 360.0), 0f);
                go.transform.localScale = new Vector3(
                    baseScale * Random.Range(0.85f, 1.15f),
                    baseScale * Random.Range(0.55f, 0.8f),
                    baseScale * Random.Range(0.85f, 1.15f));

                var meshFilter = go.AddComponent<MeshFilter>();
                float noiseStrength = Random.Range(0.28f, 0.5f);
                meshFilter.sharedMesh = ProceduralRockGenerator.BuildRockMesh(seedBase + i, subdivisions: baseScale > 1f ? 3 : 2, noiseStrength, squashY: 1f);

                var meshRenderer = go.AddComponent<MeshRenderer>();
                if (material != null)
                {
                    meshRenderer.sharedMaterial = material;
                }

                var meshCollider = go.AddComponent<MeshCollider>();
                meshCollider.sharedMesh = meshFilter.sharedMesh;
                meshCollider.convex = true;
            }
        }

        private static Material FindMaterial(string name)
        {
            string[] guids = AssetDatabase.FindAssets($"{name} t:Material");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var material = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (material != null && material.name == name)
                {
                    return material;
                }
            }
            return null;
        }
    }
}
