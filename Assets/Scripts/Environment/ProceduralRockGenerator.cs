using System.Collections.Generic;
using UnityEngine;

namespace Rapadura.Environment
{
    /// <summary>
    /// Builds irregular, naturally-shaped rock meshes at runtime/edit-time — replaces the old
    /// placeholder rocks (flattened/rotated sphere primitives, all visually identical) with real
    /// geometry: a subdivided icosphere perturbed by layered 3D noise per vertex, then squashed
    /// non-uniformly so it reads as a rock silhouette rather than a lumpy ball. Every seed produces
    /// a distinct shape, so scattering many rocks never repeats the same one — and it costs nothing
    /// (no Meshy credits, no external asset), which is the right tradeoff here: rocks don't need
    /// branching structure or fine surface detail the way trees do, so procedural geometry alone
    /// already reads as "real rock" once faceted (hard, non-smoothed normals — natural rock faces
    /// aren't smooth like a sphere).
    /// </summary>
    public static class ProceduralRockGenerator
    {
        /// <summary>
        /// Generates one rock mesh. <paramref name="seed"/> drives every random choice, so the same
        /// seed always reproduces the same rock (useful for prefab variants); different seeds never
        /// look alike. <paramref name="noiseStrength"/>/<paramref name="squash"/> let callers dial
        /// in "small pebble" vs "boulder" silhouettes from the same function.
        /// </summary>
        public static Mesh BuildRockMesh(int seed, int subdivisions = 2, float noiseStrength = 0.35f, float squashY = 0.7f)
        {
            System.Random random = new System.Random(seed);
            Vector3 noiseOffset = new Vector3(
                (float)random.NextDouble() * 100f,
                (float)random.NextDouble() * 100f,
                (float)random.NextDouble() * 100f);

            (List<Vector3> vertices, List<int> triangles) = BuildIcosphere(subdivisions);

            // Layered (fractal) 3D noise per vertex direction — Unity has no built-in 3D Perlin, so
            // three 2D samples (one per axis pair) summed together approximates it well enough for
            // a rock bump field; this is the same trick used for cheap "3D noise" in shader code
            // without a real simplex-3D implementation.
            for (int i = 0; i < vertices.Count; i++)
            {
                Vector3 direction = vertices[i].normalized;
                float displacement = 0f;
                float amplitude = 1f;
                float frequency = 1.6f;
                float totalAmplitude = 0f;

                for (int octave = 0; octave < 3; octave++)
                {
                    Vector3 sample = direction * frequency + noiseOffset;
                    float n = Noise3D(sample);
                    displacement += n * amplitude;
                    totalAmplitude += amplitude;
                    amplitude *= 0.5f;
                    frequency *= 2f;
                }

                displacement /= totalAmplitude;

                float radius = 1f + displacement * noiseStrength;
                Vector3 displaced = direction * radius;
                displaced.y *= squashY;
                vertices[i] = displaced;
            }

            return BuildFacetedMesh(vertices, triangles, "ProceduralRock_" + seed);
        }

        /// <summary>Cheap pseudo-3D noise: three overlapping 2D Perlin samples, one per axis-pair, averaged.</summary>
        private static float Noise3D(Vector3 p)
        {
            float xy = Mathf.PerlinNoise(p.x, p.y);
            float yz = Mathf.PerlinNoise(p.y, p.z);
            float zx = Mathf.PerlinNoise(p.z, p.x);
            return ((xy + yz + zx) / 3f) * 2f - 1f; // remap 0..1 -> -1..1
        }

        /// <summary>Standard 12-vertex/20-face icosahedron, edge-subdivided <paramref name="subdivisions"/> times and re-normalized to a unit sphere each pass.</summary>
        private static (List<Vector3> vertices, List<int> triangles) BuildIcosphere(int subdivisions)
        {
            float t = (1f + Mathf.Sqrt(5f)) / 2f;

            var vertices = new List<Vector3>
            {
                new Vector3(-1, t, 0), new Vector3(1, t, 0), new Vector3(-1, -t, 0), new Vector3(1, -t, 0),
                new Vector3(0, -1, t), new Vector3(0, 1, t), new Vector3(0, -1, -t), new Vector3(0, 1, -t),
                new Vector3(t, 0, -1), new Vector3(t, 0, 1), new Vector3(-t, 0, -1), new Vector3(-t, 0, 1),
            };

            for (int i = 0; i < vertices.Count; i++)
            {
                vertices[i] = vertices[i].normalized;
            }

            var triangles = new List<int>
            {
                0, 11, 5,  0, 5, 1,  0, 1, 7,  0, 7, 10,  0, 10, 11,
                1, 5, 9,  5, 11, 4,  11, 10, 2,  10, 7, 6,  7, 1, 8,
                3, 9, 4,  3, 4, 2,  3, 2, 6,  3, 6, 8,  3, 8, 9,
                4, 9, 5,  2, 4, 11,  6, 2, 10,  8, 6, 7,  9, 8, 1,
            };

            var midpointCache = new Dictionary<long, int>();

            for (int s = 0; s < subdivisions; s++)
            {
                var newTriangles = new List<int>(triangles.Count * 4);
                midpointCache.Clear();

                for (int i = 0; i < triangles.Count; i += 3)
                {
                    int a = triangles[i];
                    int b = triangles[i + 1];
                    int c = triangles[i + 2];

                    int ab = GetMidpoint(a, b, vertices, midpointCache);
                    int bc = GetMidpoint(b, c, vertices, midpointCache);
                    int ca = GetMidpoint(c, a, vertices, midpointCache);

                    newTriangles.AddRange(new[] { a, ab, ca, b, bc, ab, c, ca, bc, ab, bc, ca });
                }

                triangles = newTriangles;
            }

            return (vertices, triangles);
        }

        private static int GetMidpoint(int a, int b, List<Vector3> vertices, Dictionary<long, int> cache)
        {
            long key = a < b ? ((long)a << 32) + b : ((long)b << 32) + a;

            if (cache.TryGetValue(key, out int existing))
            {
                return existing;
            }

            Vector3 midpoint = ((vertices[a] + vertices[b]) * 0.5f).normalized;
            vertices.Add(midpoint);
            int index = vertices.Count - 1;
            cache[key] = index;
            return index;
        }

        /// <summary>
        /// Duplicates vertices per-triangle so each face gets its own flat (non-shared, non-smoothed)
        /// normal — real rock faces are faceted, not smoothly curved like the sphere this started
        /// from; smoothing them would make it look like a bumpy ball instead of a rock.
        /// </summary>
        private static Mesh BuildFacetedMesh(List<Vector3> sourceVertices, List<int> sourceTriangles, string name)
        {
            var vertices = new Vector3[sourceTriangles.Count];
            var triangles = new int[sourceTriangles.Count];
            var uvs = new Vector2[sourceTriangles.Count];

            for (int i = 0; i < sourceTriangles.Count; i++)
            {
                vertices[i] = sourceVertices[sourceTriangles[i]];
                triangles[i] = i;

                // Cheap planar-ish UV from the (pre-displacement) sphere direction — good enough for
                // a tiled rock/dirt texture, no visible seams that matter at this object's scale.
                Vector3 dir = sourceVertices[sourceTriangles[i]].normalized;
                uvs[i] = new Vector2(0.5f + Mathf.Atan2(dir.z, dir.x) / (Mathf.PI * 2f), 0.5f - Mathf.Asin(dir.y) / Mathf.PI);
            }

            var mesh = new Mesh { name = name };
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.uv = uvs;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            mesh.RecalculateTangents();
            return mesh;
        }
    }
}
