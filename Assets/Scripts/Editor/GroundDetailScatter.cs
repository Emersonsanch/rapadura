using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Rapadura.Environment;
using Rapadura.Gameplay.Environment;

namespace Rapadura.Editor
{
    /// <summary>
    /// Scatters cheap small-scale ground detail (grass tufts, pebbles, fallen twigs) across the
    /// playable area — phase 3 of the environment visual-quality plan (see TODO.md). All geometry is
    /// generated procedurally (tufts: a few crossed thin quads; pebbles: tiny
    /// <see cref="ProceduralRockGenerator"/> instances; twigs: thin stretched boxes) so this costs no
    /// API credits. Everything shares one material per detail type with GPU Instancing enabled, and
    /// per-instance color variation comes from <see cref="TreeColorVariation"/> via
    /// MaterialPropertyBlock, so batching survives.
    /// </summary>
    public static class GroundDetailScatter
    {
        [MenuItem("Rapadura/Setup/Scatter Ground Detail")]
        public static void Scatter()
        {
            Scene scene = SceneManager.GetActiveScene();

            GameObject parent = GameObject.Find("GroundDetail");
            if (parent == null)
            {
                parent = new GameObject("GroundDetail");
            }
            else
            {
                for (int i = parent.transform.childCount - 1; i >= 0; i--)
                {
                    Object.DestroyImmediate(parent.transform.GetChild(i).gameObject);
                }
            }

            Material tuftMaterial = BuildTuftMaterial();
            Material pebbleMaterial = FindMaterial("Mat_Rock");
            Material twigMaterial = BuildTwigMaterial();

            var random = new System.Random(4242);

            // Same hand-picked playable footprint as NaturalTreeSetup's tree spread — dense but
            // random-looking detail scatter across roughly a 26x26 area centered on the ground,
            // avoiding a perfectly uniform grid (which would itself look artificial).
            const int tuftCount = 120;
            const int pebbleCount = 40;
            const int twigCount = 18;

            for (int i = 0; i < tuftCount; i++)
            {
                Vector3 pos = RandomGroundPosition(random);
                GameObject tuft = BuildTuft(random, tuftMaterial);
                tuft.transform.SetParent(parent.transform, worldPositionStays: false);
                tuft.transform.position = pos;
                tuft.transform.rotation = Quaternion.Euler(0f, (float)(random.NextDouble() * 360.0), 0f);
                float scale = 0.6f + (float)random.NextDouble() * 0.7f;
                tuft.transform.localScale = Vector3.one * scale;
                tuft.name = $"Tuft_{i}";

                var variation = tuft.AddComponent<TreeColorVariation>();
                variation.ApplyVariationForTests();
            }

            for (int i = 0; i < pebbleCount; i++)
            {
                Vector3 pos = RandomGroundPosition(random);
                var go = new GameObject($"Pebble_{i}");
                go.transform.SetParent(parent.transform, worldPositionStays: false);
                float scale = 0.08f + (float)random.NextDouble() * 0.14f;
                pos.y = scale * 0.3f;
                go.transform.position = pos;
                go.transform.rotation = Quaternion.Euler(0f, (float)(random.NextDouble() * 360.0), 0f);
                go.transform.localScale = new Vector3(scale, scale * 0.7f, scale);

                var meshFilter = go.AddComponent<MeshFilter>();
                meshFilter.sharedMesh = ProceduralRockGenerator.BuildRockMesh(9000 + i, subdivisions: 1, noiseStrength: 0.4f, squashY: 1f);
                var meshRenderer = go.AddComponent<MeshRenderer>();
                if (pebbleMaterial != null)
                {
                    meshRenderer.sharedMaterial = pebbleMaterial;
                }
            }

            for (int i = 0; i < twigCount; i++)
            {
                Vector3 pos = RandomGroundPosition(random);
                GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                Object.DestroyImmediate(go.GetComponent<BoxCollider>());
                go.transform.SetParent(parent.transform, worldPositionStays: false);
                float length = 0.4f + (float)random.NextDouble() * 0.5f;
                pos.y = 0.015f;
                go.transform.position = pos;
                go.transform.rotation = Quaternion.Euler(0f, (float)(random.NextDouble() * 360.0), (float)(random.NextDouble() * 10.0 - 5.0));
                go.transform.localScale = new Vector3(length, 0.03f, 0.05f);
                go.name = $"Twig_{i}";
                go.GetComponent<MeshRenderer>().sharedMaterial = twigMaterial;
            }

            if (tuftMaterial != null) tuftMaterial.enableInstancing = true;
            if (twigMaterial != null) twigMaterial.enableInstancing = true;

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log($"[GroundDetailScatter] Scattered {tuftCount} tufts, {pebbleCount} pebbles, {twigCount} twigs in '{scene.name}'.");
        }

        private static Vector3 RandomGroundPosition(System.Random random)
        {
            float x = (float)(random.NextDouble() * 26.0 - 13.0);
            float z = (float)(random.NextDouble() * 26.0 - 13.0);
            return new Vector3(x, 0f, z);
        }

        private static GameObject BuildTuft(System.Random random, Material material)
        {
            // A tuft is 3 thin crossed quads (like classic billboard grass) — cheap (12 tris),
            // reads as a grass clump from the isometric gameplay camera without needing a mesh asset.
            var mesh = new Mesh { name = "GrassTuft" };
            var vertices = new List<Vector3>();
            var triangles = new List<int>();
            var uvs = new List<Vector2>();
            var normals = new List<Vector3>();

            const float width = 0.25f;
            const float height = 0.35f;
            int bladeCount = 3;

            for (int b = 0; b < bladeCount; b++)
            {
                float angle = (360f / bladeCount) * b + (float)random.NextDouble() * 20f;
                Quaternion rot = Quaternion.Euler(0f, angle, 0f);
                int baseIndex = vertices.Count;

                Vector3 right = rot * Vector3.right * width * 0.5f;
                vertices.Add(-right);
                vertices.Add(right);
                vertices.Add(right + Vector3.up * height);
                vertices.Add(-right + Vector3.up * height);

                uvs.Add(new Vector2(0f, 0f));
                uvs.Add(new Vector2(1f, 0f));
                uvs.Add(new Vector2(1f, 1f));
                uvs.Add(new Vector2(0f, 1f));

                // Fixed up-facing normals rather than RecalculateNormals: this mesh renders both
                // faces of each quad from the same shared vertices (Cull Off material), and letting
                // Unity derive normals from triangle winding makes the front/back face normals at
                // each vertex point opposite directions and cancel to zero — that read as solid
                // black silhouettes in URP Lit (confirmed via screenshot). An explicit mostly-up
                // normal is the standard cheap-grass-billboard trick: not physically correct per
                // face, but avoids the cancellation and lights consistently from any camera angle.
                Vector3 fakeNormal = (Vector3.up + rot * Vector3.forward * 0.3f).normalized;
                normals.Add(fakeNormal);
                normals.Add(fakeNormal);
                normals.Add(fakeNormal);
                normals.Add(fakeNormal);

                triangles.Add(baseIndex); triangles.Add(baseIndex + 2); triangles.Add(baseIndex + 1);
                triangles.Add(baseIndex); triangles.Add(baseIndex + 3); triangles.Add(baseIndex + 2);
                // Back faces too, so the crossed quads don't disappear from the opposite side.
                triangles.Add(baseIndex); triangles.Add(baseIndex + 1); triangles.Add(baseIndex + 2);
                triangles.Add(baseIndex); triangles.Add(baseIndex + 2); triangles.Add(baseIndex + 3);
            }

            mesh.SetVertices(vertices);
            mesh.SetUVs(0, uvs);
            mesh.SetNormals(normals);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateBounds();

            var go = new GameObject("Tuft");
            var filter = go.AddComponent<MeshFilter>();
            filter.sharedMesh = mesh;
            var renderer = go.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            return go;
        }

        private static Material BuildTuftMaterial()
        {
            const string path = "Assets/Materials/Mat_GrassTuft.mat";
            var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (existing != null)
            {
                return existing;
            }

            var shader = Shader.Find("Universal Render Pipeline/Lit");
            var mat = new Material(shader) { name = "Mat_GrassTuft" };
            mat.SetColor("_BaseColor", new Color(0.22f, 0.42f, 0.14f));
            mat.SetFloat("_Cull", 0f); // two-sided, since tuft blades are single-quad-thin
            mat.enableInstancing = true;
            AssetDatabase.CreateAsset(mat, path);
            return mat;
        }

        private static Material BuildTwigMaterial()
        {
            const string path = "Assets/Materials/Mat_Twig.mat";
            var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (existing != null)
            {
                return existing;
            }

            var shader = Shader.Find("Universal Render Pipeline/Lit");
            var mat = new Material(shader) { name = "Mat_Twig" };
            mat.SetColor("_BaseColor", new Color(0.36f, 0.26f, 0.16f));
            mat.enableInstancing = true;
            AssetDatabase.CreateAsset(mat, path);
            return mat;
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
