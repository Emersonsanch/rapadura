using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Rapadura.Gameplay.World.Map.Dressing
{
    /// <summary>
    /// Appends flat-shaded low-poly shapes (boxes, cylinders/cones/frusta, icospheres, any mesh) into
    /// one vertex list, each coloured through a <see cref="WorldMapPalette"/> cell UV, so a whole set
    /// of props becomes a single mesh drawn with the shared palette material. Every shape goes
    /// through <see cref="Matrix"/> (set it to place a prop) and its own local transform.
    /// </summary>
    public sealed class WorldMapMeshBuilder
    {
        private readonly List<Vector3> _vertices = new List<Vector3>(4096);
        private readonly List<Vector3> _normals = new List<Vector3>(4096);
        private readonly List<Vector2> _uvs = new List<Vector2>(4096);
        private readonly List<int> _triangles = new List<int>(8192);

        private static Vector3[] _unitIcoVertices;
        private static int[] _unitIcoTriangles;
        private static int[] _coarseIcoTriangles;

        /// <summary>Applied to every shape added from now on (prop placement).</summary>
        public Matrix4x4 Matrix = Matrix4x4.identity;

        public int VertexCount => _vertices.Count;
        public bool IsEmpty => _vertices.Count == 0;

        public void Clear()
        {
            _vertices.Clear();
            _normals.Clear();
            _uvs.Clear();
            _triangles.Clear();
            Matrix = Matrix4x4.identity;
        }

        /// <summary>One flat triangle (clockwise seen from the front, Unity's winding).</summary>
        public void Triangle(Vector3 a, Vector3 b, Vector3 c, WorldMapSwatch swatch)
        {
            a = Matrix.MultiplyPoint3x4(a);
            b = Matrix.MultiplyPoint3x4(b);
            c = Matrix.MultiplyPoint3x4(c);
            Vector3 normal = Vector3.Cross(b - a, c - a);
            if (normal.sqrMagnitude < 1e-12f)
            {
                return;
            }

            normal.Normalize();
            Vector2 uv = WorldMapPalette.UvOf(swatch);
            int start = _vertices.Count;
            _vertices.Add(a);
            _vertices.Add(b);
            _vertices.Add(c);
            for (int i = 0; i < 3; i++)
            {
                _normals.Add(normal);
                _uvs.Add(uv);
            }

            _triangles.Add(start);
            _triangles.Add(start + 1);
            _triangles.Add(start + 2);
        }

        /// <summary>A flat quad (corners in order, clockwise from the front).</summary>
        public void Quad(Vector3 a, Vector3 b, Vector3 c, Vector3 d, WorldMapSwatch swatch)
        {
            Triangle(a, b, c, swatch);
            Triangle(a, c, d, swatch);
        }

        /// <summary>A double-sided quad with an "up-ish" normal (grass blades, flags): never black from behind.</summary>
        public void Card(Vector3 a, Vector3 b, Vector3 c, Vector3 d, WorldMapSwatch swatch, Vector3 normal)
        {
            Vector2 uv = WorldMapPalette.UvOf(swatch);
            Vector3 n = Matrix.MultiplyVector(normal).normalized;
            int start = _vertices.Count;
            _vertices.Add(Matrix.MultiplyPoint3x4(a));
            _vertices.Add(Matrix.MultiplyPoint3x4(b));
            _vertices.Add(Matrix.MultiplyPoint3x4(c));
            _vertices.Add(Matrix.MultiplyPoint3x4(d));
            for (int i = 0; i < 4; i++)
            {
                _normals.Add(n);
                _uvs.Add(uv);
            }

            _triangles.Add(start); _triangles.Add(start + 1); _triangles.Add(start + 2);
            _triangles.Add(start); _triangles.Add(start + 2); _triangles.Add(start + 3);
            _triangles.Add(start); _triangles.Add(start + 2); _triangles.Add(start + 1);
            _triangles.Add(start); _triangles.Add(start + 3); _triangles.Add(start + 2);
        }

        /// <summary>Axis-aligned box (in the local frame given by <paramref name="center"/>/<paramref name="yaw"/>).</summary>
        public void Box(Vector3 center, Vector3 size, WorldMapSwatch swatch, float yaw = 0f, WorldMapSwatch? top = null)
        {
            Matrix4x4 saved = Matrix;
            Matrix = saved * Matrix4x4.TRS(center, Quaternion.Euler(0f, yaw, 0f), size);

            var p000 = new Vector3(-0.5f, -0.5f, -0.5f);
            var p100 = new Vector3(0.5f, -0.5f, -0.5f);
            var p010 = new Vector3(-0.5f, 0.5f, -0.5f);
            var p110 = new Vector3(0.5f, 0.5f, -0.5f);
            var p001 = new Vector3(-0.5f, -0.5f, 0.5f);
            var p101 = new Vector3(0.5f, -0.5f, 0.5f);
            var p011 = new Vector3(-0.5f, 0.5f, 0.5f);
            var p111 = new Vector3(0.5f, 0.5f, 0.5f);

            Quad(p000, p010, p110, p100, swatch);             // front (-z)
            Quad(p101, p111, p011, p001, swatch);             // back (+z)
            Quad(p001, p011, p010, p000, swatch);             // left (-x)
            Quad(p100, p110, p111, p101, swatch);             // right (+x)
            Quad(p010, p011, p111, p110, top ?? swatch);      // top
            Quad(p001, p000, p100, p101, swatch);             // bottom

            Matrix = saved;
        }

        /// <summary>
        /// Frustum around +y: <paramref name="bottomRadius"/> at <paramref name="baseCenter"/>,
        /// <paramref name="topRadius"/> at <paramref name="height"/> (0 = cone). Flat-shaded sides.
        /// </summary>
        public void Cylinder(Vector3 baseCenter, float bottomRadius, float topRadius, float height, int sides, WorldMapSwatch swatch,
            WorldMapSwatch? cap = null, float yaw = 0f, bool bottomCap = false)
        {
            sides = Mathf.Max(3, sides);
            Matrix4x4 saved = Matrix;
            Matrix = saved * Matrix4x4.TRS(baseCenter, Quaternion.Euler(0f, yaw, 0f), Vector3.one);

            Vector3 top = Vector3.up * height;
            for (int i = 0; i < sides; i++)
            {
                float a0 = (float)i / sides * Mathf.PI * 2f;
                float a1 = (float)(i + 1) / sides * Mathf.PI * 2f;
                var d0 = new Vector3(Mathf.Cos(a0), 0f, Mathf.Sin(a0));
                var d1 = new Vector3(Mathf.Cos(a1), 0f, Mathf.Sin(a1));
                Vector3 b0 = d0 * bottomRadius;
                Vector3 b1 = d1 * bottomRadius;

                if (topRadius <= 0.0001f)
                {
                    Triangle(b1, b0, top, swatch);
                }
                else
                {
                    Vector3 t0 = d0 * topRadius + top;
                    Vector3 t1 = d1 * topRadius + top;
                    Quad(b1, b0, t0, t1, swatch);
                    Triangle(top, t1, t0, cap ?? swatch);
                }

                if (bottomCap)
                {
                    Triangle(Vector3.zero, b0, b1, swatch);
                }
            }

            Matrix = saved;
        }

        /// <summary>
        /// Low-poly icosphere blob scaled per axis: 80 faces, or 20 when <paramref name="coarse"/>
        /// (clouds, fog, pebbles: chunkier and a quarter of the vertices).
        /// </summary>
        public void Blob(Vector3 center, Vector3 size, WorldMapSwatch swatch, float yaw = 0f, bool coarse = false)
        {
            EnsureIco();
            Matrix4x4 saved = Matrix;
            Matrix = saved * Matrix4x4.TRS(center, Quaternion.Euler(0f, yaw, 0f), size * 0.5f);
            int[] triangles = coarse ? _coarseIcoTriangles : _unitIcoTriangles;
            for (int i = 0; i < triangles.Length; i += 3)
            {
                Triangle(_unitIcoVertices[triangles[i]], _unitIcoVertices[triangles[i + 1]], _unitIcoVertices[triangles[i + 2]], swatch);
            }

            Matrix = saved;
        }

        /// <summary>Appends <paramref name="mesh"/>'s triangles (e.g. a ProceduralRockGenerator rock), recoloured.</summary>
        public void AddMesh(Mesh mesh, Matrix4x4 local, WorldMapSwatch swatch, WorldMapSwatch? upFacing = null)
        {
            if (mesh == null)
            {
                return;
            }

            Matrix4x4 saved = Matrix;
            Matrix = saved * local;
            Vector3[] vertices = mesh.vertices;
            int[] triangles = mesh.triangles;
            for (int i = 0; i < triangles.Length; i += 3)
            {
                Vector3 a = vertices[triangles[i]];
                Vector3 b = vertices[triangles[i + 1]];
                Vector3 c = vertices[triangles[i + 2]];
                WorldMapSwatch s = swatch;
                if (upFacing.HasValue)
                {
                    Vector3 n = Matrix.MultiplyVector(Vector3.Cross(b - a, c - a)).normalized;
                    if (n.y > 0.55f)
                    {
                        s = upFacing.Value;
                    }
                }

                Triangle(a, b, c, s);
            }

            Matrix = saved;
        }

        /// <summary>Builds (or refills) a mesh; 32-bit indices only when needed.</summary>
        public Mesh ToMesh(string name, Mesh target = null)
        {
            Mesh mesh = target != null ? target : new Mesh();
            mesh.Clear();
            mesh.name = name;
            mesh.indexFormat = _vertices.Count > 65000 ? IndexFormat.UInt32 : IndexFormat.UInt16;
            mesh.SetVertices(_vertices);
            mesh.SetNormals(_normals);
            mesh.SetUVs(0, _uvs);
            mesh.SetTriangles(_triangles, 0);
            mesh.RecalculateBounds();
            return mesh;
        }

        private static void EnsureIco()
        {
            if (_unitIcoVertices != null)
            {
                return;
            }

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

            int[] faces =
            {
                0, 11, 5, 0, 5, 1, 0, 1, 7, 0, 7, 10, 0, 10, 11,
                1, 5, 9, 5, 11, 4, 11, 10, 2, 10, 7, 6, 7, 1, 8,
                3, 9, 4, 3, 4, 2, 3, 2, 6, 3, 6, 8, 3, 8, 9,
                4, 9, 5, 2, 4, 11, 6, 2, 10, 8, 6, 7, 9, 8, 1,
            };

            // One subdivision (80 faces): round enough for clouds/bushes, still chunky low-poly.
            var triangles = new List<int>(faces.Length * 4);
            var cache = new Dictionary<long, int>();
            for (int i = 0; i < faces.Length; i += 3)
            {
                int a = faces[i];
                int b = faces[i + 1];
                int c = faces[i + 2];
                int ab = Midpoint(a, b, vertices, cache);
                int bc = Midpoint(b, c, vertices, cache);
                int ca = Midpoint(c, a, vertices, cache);
                triangles.AddRange(new[] { a, ab, ca, b, bc, ab, c, ca, bc, ab, bc, ca });
            }

            _coarseIcoTriangles = faces;
            _unitIcoVertices = vertices.ToArray();
            _unitIcoTriangles = triangles.ToArray();
        }

        private static int Midpoint(int a, int b, List<Vector3> vertices, Dictionary<long, int> cache)
        {
            long key = a < b ? ((long)a << 32) + b : ((long)b << 32) + a;
            if (cache.TryGetValue(key, out int index))
            {
                return index;
            }

            vertices.Add(((vertices[a] + vertices[b]) * 0.5f).normalized);
            index = vertices.Count - 1;
            cache[key] = index;
            return index;
        }
    }
}
