using System.Collections.Generic;
using UnityEngine;

namespace Rapadura.Editor
{
    /// <summary>
    /// Coarse mesh simplification via 3D grid vertex clustering — every vertex snaps to the
    /// nearest cell of a uniform grid sized to hit roughly <c>targetVertexCount</c>, cells collapse
    /// to one representative vertex (averaged position/normal/UV), and any triangle that degenerates
    /// (two or more corners landing in the same cell) is dropped. Not as shape-preserving as a real
    /// edge-collapse/quadric-error decimator, but simple, dependency-free, and more than adequate
    /// for turning Meshy's raw text-to-3D output (routinely 300k-800k+ vertices — far too heavy to
    /// scatter dozens of across a mobile scene) down to a few thousand triangles for background
    /// environment props like trees, where exact silhouette fidelity at a distance doesn't matter.
    /// An external package (UnityMeshSimplifier) was tried first and failed to install from this
    /// environment (git package resolution unavailable) — this exists specifically to unblock that.
    /// </summary>
    public static class MeshDecimator
    {
        public static Mesh Decimate(Mesh source, int targetVertexCount)
        {
            Vector3[] sourceVertices = source.vertices;
            Vector3[] sourceNormals = source.normals;
            Vector2[] sourceUvs = source.uv;
            int[] sourceTriangles = source.triangles;

            bool hasNormals = sourceNormals != null && sourceNormals.Length == sourceVertices.Length;
            bool hasUvs = sourceUvs != null && sourceUvs.Length == sourceVertices.Length;

            Bounds bounds = source.bounds;
            float volume = Mathf.Max(bounds.size.x, 0.0001f) * Mathf.Max(bounds.size.y, 0.0001f) * Mathf.Max(bounds.size.z, 0.0001f);
            // Cell size such that (extent / cellSize)^3 roughly equals the target vertex count.
            float cellSize = Mathf.Pow(volume / Mathf.Max(targetVertexCount, 8), 1f / 3f);
            cellSize = Mathf.Max(cellSize, 0.0001f);

            // UV bucket is coarse but non-zero: two vertices only cluster together if they're both
            // spatially close AND land in roughly the same patch of texture space. Positional
            // clustering alone (the first version of this) happily merged vertices from unrelated
            // UV islands whenever they happened to be near each other in 3D — very common for
            // foliage made of many small overlapping leaf-card blobs — which produced triangles
            // spanning distant, unrelated regions of the texture atlas: a scrambled/cracked mess,
            // confirmed by comparing against the un-decimated source (which looks correct).
            const float uvBucketSize = 0.08f;
            var cellToIndex = new Dictionary<(int, int, int, int, int), int>();
            var vertexAccum = new List<Vector3>();
            var normalAccum = new List<Vector3>();
            // UVs are deliberately NOT averaged across a cluster: a cluster can straddle two
            // unrelated UV islands (e.g. trunk bark vs. leaf texture, or two separate leaf blobs),
            // and averaging their coordinates produces nonsense mid-texture-sheet samples — visibly
            // a scrambled/cracked mess rather than a slightly-blurred texture. Keeping the first
            // vertex's UV per cluster is not perfect (a small chance of a "wrong" pick at a cluster
            // boundary) but never produces garbage, unlike an average ever can.
            var firstUv = new List<Vector2>();
            var accumCount = new List<int>();
            var remap = new int[sourceVertices.Length];

            for (int i = 0; i < sourceVertices.Length; i++)
            {
                Vector3 v = sourceVertices[i];
                Vector2 uv = hasUvs ? sourceUvs[i] : Vector2.zero;
                var cell = (
                    Mathf.RoundToInt(v.x / cellSize),
                    Mathf.RoundToInt(v.y / cellSize),
                    Mathf.RoundToInt(v.z / cellSize),
                    hasUvs ? Mathf.RoundToInt(uv.x / uvBucketSize) : 0,
                    hasUvs ? Mathf.RoundToInt(uv.y / uvBucketSize) : 0);

                if (!cellToIndex.TryGetValue(cell, out int clusterIndex))
                {
                    clusterIndex = vertexAccum.Count;
                    cellToIndex[cell] = clusterIndex;
                    vertexAccum.Add(Vector3.zero);
                    normalAccum.Add(Vector3.zero);
                    firstUv.Add(hasUvs ? sourceUvs[i] : Vector2.zero);
                    accumCount.Add(0);
                }

                vertexAccum[clusterIndex] += v;
                if (hasNormals)
                {
                    normalAccum[clusterIndex] += sourceNormals[i];
                }
                accumCount[clusterIndex]++;
                remap[i] = clusterIndex;
            }

            var newVertices = new List<Vector3>(vertexAccum.Count);
            var newNormals = new List<Vector3>(vertexAccum.Count);
            var newUvs = new List<Vector2>(vertexAccum.Count);

            for (int i = 0; i < vertexAccum.Count; i++)
            {
                float count = accumCount[i];
                newVertices.Add(vertexAccum[i] / count);
                newNormals.Add(hasNormals ? (normalAccum[i] / count).normalized : Vector3.up);
                newUvs.Add(firstUv[i]);
            }

            var newTriangles = new List<int>(sourceTriangles.Length);
            for (int i = 0; i < sourceTriangles.Length; i += 3)
            {
                int a = remap[sourceTriangles[i]];
                int b = remap[sourceTriangles[i + 1]];
                int c = remap[sourceTriangles[i + 2]];

                if (a == b || b == c || c == a)
                {
                    continue; // degenerate — all 3 corners collapsed into fewer than 3 distinct cells
                }

                newTriangles.Add(a);
                newTriangles.Add(b);
                newTriangles.Add(c);
            }

            var result = new Mesh
            {
                name = source.name + "_LOD",
                indexFormat = newVertices.Count > 65000
                    ? UnityEngine.Rendering.IndexFormat.UInt32
                    : UnityEngine.Rendering.IndexFormat.UInt16,
            };
            result.SetVertices(newVertices);
            if (hasUvs)
            {
                result.SetUVs(0, newUvs);
            }
            result.SetTriangles(newTriangles, 0);

            if (hasNormals)
            {
                result.SetNormals(newNormals);
            }
            else
            {
                result.RecalculateNormals();
            }

            result.RecalculateBounds();
            result.RecalculateTangents();
            return result;
        }
    }
}
