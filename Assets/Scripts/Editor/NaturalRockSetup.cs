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
            GameObject rock1 = GameObject.Find("Rock1");
            GameObject rock2 = GameObject.Find("Rock2");
            replaced += ReplaceRock(rock1, rockMaterial, seedBase: 1001);
            replaced += ReplaceRock(rock2, rockMaterial, seedBase: 1002);
            ClearHandPlacedRock(rock1);
            ClearHandPlacedRock(rock2);

            GameObject rocksParent = GameObject.Find("Rocks");
            if (rocksParent != null)
            {
                // Remove any previously-scattered rocks before re-scattering — re-running this menu
                // item used to duplicate the scattered set every time (15 -> 25 -> 35...) since
                // ScatterAdditionalRocks always adds `count` new ones without ever clearing what a
                // prior run left behind. Hand-placed rocks (anything not named "Rock_Scattered_*")
                // are left untouched.
                for (int i = rocksParent.transform.childCount - 1; i >= 0; i--)
                {
                    Transform child = rocksParent.transform.GetChild(i);
                    if (child.name.StartsWith("Rock_Scattered_"))
                    {
                        Object.DestroyImmediate(child.gameObject);
                    }
                }

                for (int i = 0; i < rocksParent.transform.childCount; i++)
                {
                    Transform child = rocksParent.transform.GetChild(i);
                    replaced += ReplaceRock(child.gameObject, rockMaterial, seedBase: 2000 + i);
                }

                ScatterAdditionalRocks(rocksParent.transform, rockMaterial, count: 10, seedBase: 5000);

                // Original hand-placed rocks predate the camera/player clearance check above (it only
                // ever applied to newly-scattered ones) — confirmed via screenshot that "Rock_5"
                // (hand-placed, at (3.5, 0.25, -8), scale 1.6) sits only ~4 units from the camera's
                // rest position and fills the entire view the instant Play starts. Nudge any
                // hand-placed rock the same way scattered ones are kept clear, instead of leaving a
                // pre-existing placement bug in a scene this session is explicitly fixing for playability.
                foreach (Transform child in rocksParent.transform)
                {
                    if (child.name.StartsWith("Rock_Scattered_"))
                    {
                        continue;
                    }

                    ClearHandPlacedRock(child.gameObject);
                }
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log($"[NaturalRockSetup] Replaced {replaced} rock mesh(es) with procedural geometry and scattered additional rocks in '{scene.name}'.");
        }

        private static void ClearHandPlacedRock(GameObject rock)
        {
            if (rock == null)
            {
                return;
            }

            Vector3 cleared = ClearExclusionZone(rock.transform.position, PlayerSpawnPosition, MinDistanceFromPlayerSpawn, 0f);
            cleared = ClearExclusionZone(cleared, CameraRestPosition, MinDistanceFromCamera, 0f);
            if (cleared != rock.transform.position)
            {
                rock.transform.position = cleared;
                EditorUtility.SetDirty(rock);
            }
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
        /// <summary>
        /// Player spawns at the world origin (see Main.unity) and the isometric follow-camera rests
        /// at roughly (0, 1, -10) relative to it — scattered rocks must stay clear of BOTH points, not
        /// just the player, or a rock can pass the player-clearance check while still landing right in
        /// front of the camera lens. First attempt only excluded the player point and still produced a
        /// rock 2.49 units from the camera (confirmed via screenshot: a boulder filling the entire
        /// view on spawn) because the camera sits ~10 units back along -Z, well outside a 4.5-unit
        /// player-only exclusion ring.
        /// </summary>
        private const float MinDistanceFromPlayerSpawn = 4.5f;
        private const float MinDistanceFromCamera = 5f;
        private static readonly Vector3 PlayerSpawnPosition = Vector3.zero;
        private static readonly Vector3 CameraRestPosition = new Vector3(0f, 1f, -10f);

        /// <summary>Pushes <paramref name="position"/> radially away from <paramref name="exclusionCenter"/> (compared on the XZ plane only, since the camera's exclusion point sits above ground level) until it clears <paramref name="minDistance"/>, reusing <paramref name="fallbackAngle"/> if the point starts exactly on the center.</summary>
        private static Vector3 ClearExclusionZone(Vector3 position, Vector3 exclusionCenter, float minDistance, float fallbackAngle)
        {
            Vector2 flatPosition = new Vector2(position.x, position.z);
            Vector2 flatCenter = new Vector2(exclusionCenter.x, exclusionCenter.z);
            Vector2 fromCenter = flatPosition - flatCenter;

            if (fromCenter.magnitude >= minDistance)
            {
                return position;
            }

            if (fromCenter.sqrMagnitude < 0.0001f)
            {
                fromCenter = new Vector2(Mathf.Cos(fallbackAngle), Mathf.Sin(fallbackAngle));
            }

            Vector2 cleared = flatCenter + fromCenter.normalized * minDistance;
            return new Vector3(cleared.x, position.y, cleared.y);
        }

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

                position = ClearExclusionZone(position, PlayerSpawnPosition, MinDistanceFromPlayerSpawn, angle);
                position = ClearExclusionZone(position, CameraRestPosition, MinDistanceFromCamera, angle);

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
