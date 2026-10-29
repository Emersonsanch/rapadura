using System.Collections.Generic;
using Rapadura.Environment;
using UnityEngine;
using UnityEngine.Rendering;

namespace Rapadura.Gameplay.Enemies.Visuals
{
    /// <summary>
    /// Flat-shaded low-poly shape appender for the procedural enemies (the enemy-side twin of
    /// <c>WorldMapMeshBuilder</c>): boxes, frusta/cones, icosphere blobs, crystals, horns and
    /// <see cref="ProceduralRockGenerator"/> rocks go into one vertex list, each face coloured through
    /// an <see cref="EnemyModelPalette"/> cell UV, so a part is one mesh on the shared palette material.
    /// Every shape goes through <see cref="Matrix"/> plus its own local transform. Pure and
    /// deterministic: the same calls always produce the same mesh.
    /// </summary>
    public sealed class EnemyMeshBuilder
    {
        private const int RockVariants = 4;

        private readonly List<Vector3> _vertices = new List<Vector3>(1024);
        private readonly List<Vector3> _normals = new List<Vector3>(1024);
        private readonly List<Vector2> _uvs = new List<Vector2>(1024);
        private readonly List<int> _triangles = new List<int>(2048);

        private static Vector3[] _icoVertices;
        private static int[] _icoTriangles;
        private static int[] _coarseIcoTriangles;
        private static Vector3[][] _rockVertices;
        private static int[][] _rockTriangles;

        /// <summary>Applied to every shape added from now on.</summary>
        public Matrix4x4 Matrix = Matrix4x4.identity;

        public int VertexCount => _vertices.Count;
        public int TriangleCount => _triangles.Count / 3;
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
        public void Triangle(Vector3 a, Vector3 b, Vector3 c, EnemySwatch swatch)
        {
            a = Matrix.MultiplyPoint3x4(a);
            b = Matrix.MultiplyPoint3x4(b);
            c = Matrix.MultiplyPoint3x4(c);
            Vector3 normal = Vector3.Cross(b - a, c - a);
            if (normal.sqrMagnitude < 1e-14f)
            {
                return;
            }

            // A mirroring matrix (negative scale) flips the winding: keep faces pointing outward.
            if (Matrix.determinant < 0f)
            {
                (b, c) = (c, b);
                normal = -normal;
            }

            normal.Normalize();
            Vector2 uv = EnemyModelPalette.UvOf(swatch);
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

        public void Quad(Vector3 a, Vector3 b, Vector3 c, Vector3 d, EnemySwatch swatch)
        {
            Triangle(a, b, c, swatch);
            Triangle(a, c, d, swatch);
        }

        /// <summary>Double-sided triangle (wing membranes, cape): visible and lit from both sides.</summary>
        public void TwoSided(Vector3 a, Vector3 b, Vector3 c, EnemySwatch swatch, EnemySwatch? back = null)
        {
            Triangle(a, b, c, swatch);
            Triangle(a, c, b, back ?? swatch);
        }

        /// <summary>Box centred on <paramref name="center"/>, rotated by <paramref name="rotation"/>.</summary>
        public void Box(Vector3 center, Vector3 size, EnemySwatch swatch, Quaternion? rotation = null, EnemySwatch? top = null)
        {
            Matrix4x4 saved = Matrix;
            Matrix = saved * Matrix4x4.TRS(center, rotation ?? Quaternion.identity, size);

            var p000 = new Vector3(-0.5f, -0.5f, -0.5f);
            var p100 = new Vector3(0.5f, -0.5f, -0.5f);
            var p010 = new Vector3(-0.5f, 0.5f, -0.5f);
            var p110 = new Vector3(0.5f, 0.5f, -0.5f);
            var p001 = new Vector3(-0.5f, -0.5f, 0.5f);
            var p101 = new Vector3(0.5f, -0.5f, 0.5f);
            var p011 = new Vector3(-0.5f, 0.5f, 0.5f);
            var p111 = new Vector3(0.5f, 0.5f, 0.5f);

            Quad(p000, p010, p110, p100, swatch);
            Quad(p101, p111, p011, p001, swatch);
            Quad(p001, p011, p010, p000, swatch);
            Quad(p100, p110, p111, p101, swatch);
            Quad(p010, p011, p111, p110, top ?? swatch);
            Quad(p001, p000, p100, p101, swatch);

            Matrix = saved;
        }

        /// <summary>
        /// Frustum along local +y (rotated by <paramref name="rotation"/>): <paramref name="bottomRadius"/>
        /// at <paramref name="baseCenter"/>, <paramref name="topRadius"/> at <paramref name="height"/> (0 = cone).
        /// </summary>
        public void Cylinder(Vector3 baseCenter, float bottomRadius, float topRadius, float height, int sides, EnemySwatch swatch,
            EnemySwatch? cap = null, Quaternion? rotation = null, bool bottomCap = false, float twist = 0f)
        {
            sides = Mathf.Max(3, sides);
            Matrix4x4 saved = Matrix;
            Matrix = saved * Matrix4x4.TRS(baseCenter, (rotation ?? Quaternion.identity) * Quaternion.Euler(0f, twist, 0f), Vector3.one);

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

                if (bottomCap && bottomRadius > 0.0001f)
                {
                    Triangle(Vector3.zero, b0, b1, swatch);
                }
            }

            Matrix = saved;
        }

        /// <summary>Icosphere blob of diameter <paramref name="size"/> per axis: 80 faces, 20 when <paramref name="coarse"/>.</summary>
        public void Blob(Vector3 center, Vector3 size, EnemySwatch swatch, Quaternion? rotation = null, bool coarse = false)
        {
            EnsureIco();
            Matrix4x4 saved = Matrix;
            Matrix = saved * Matrix4x4.TRS(center, rotation ?? Quaternion.identity, size * 0.5f);
            int[] triangles = coarse ? _coarseIcoTriangles : _icoTriangles;
            for (int i = 0; i < triangles.Length; i += 3)
            {
                Triangle(_icoVertices[triangles[i]], _icoVertices[triangles[i + 1]], _icoVertices[triangles[i + 2]], swatch);
            }

            Matrix = saved;
        }

        /// <summary>
        /// A <see cref="ProceduralRockGenerator"/> rock (80 faces) fitted into <paramref name="size"/>;
        /// faces pointing up more than ~55 degrees take <paramref name="upFacing"/> (snow, moss).
        /// </summary>
        public void Rock(Vector3 center, Vector3 size, EnemySwatch swatch, int variant, Quaternion? rotation = null, EnemySwatch? upFacing = null)
        {
            EnsureRocks();
            int v = Mathf.Abs(variant) % RockVariants;
            Vector3[] vertices = _rockVertices[v];
            int[] triangles = _rockTriangles[v];
            Matrix4x4 saved = Matrix;
            Matrix = saved * Matrix4x4.TRS(center, rotation ?? Quaternion.identity, size);
            for (int i = 0; i < triangles.Length; i += 3)
            {
                Vector3 a = vertices[triangles[i]];
                Vector3 b = vertices[triangles[i + 1]];
                Vector3 c = vertices[triangles[i + 2]];
                EnemySwatch s = swatch;
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

        /// <summary>A crystal: hexagonal-ish prism with a pointed tip, tilted by <paramref name="rotation"/>.</summary>
        public void Crystal(Vector3 baseCenter, float radius, float height, EnemySwatch swatch, Quaternion? rotation = null, int sides = 5)
        {
            Quaternion r = rotation ?? Quaternion.identity;
            float body = height * 0.68f;
            Cylinder(baseCenter, radius * 0.85f, radius, body, sides, swatch, rotation: r);
            Cylinder(baseCenter + r * Vector3.up * body, radius, 0f, height - body, sides, swatch, rotation: r);
        }

        /// <summary>
        /// A curved horn/claw: <paramref name="segments"/> tapering frusta, each bent by
        /// <paramref name="bendPerSegment"/> degrees around <paramref name="bendAxis"/> (default local x,
        /// applied after <paramref name="rotation"/>).
        /// </summary>
        public void Horn(Vector3 baseCenter, float baseRadius, float length, int segments, float bendPerSegment, EnemySwatch swatch,
            Quaternion? rotation = null, int sides = 5, Vector3? bendAxis = null)
        {
            Quaternion r = rotation ?? Quaternion.identity;
            Vector3 axis = bendAxis ?? Vector3.right;
            Vector3 at = baseCenter;
            float segment = length / segments;
            for (int i = 0; i < segments; i++)
            {
                float r0 = baseRadius * (1f - (float)i / segments);
                float r1 = baseRadius * (1f - (float)(i + 1) / segments);
                Cylinder(at, r0, r1, segment * 1.04f, sides, swatch, rotation: r);
                at += r * Vector3.up * segment;
                r = Quaternion.AngleAxis(bendPerSegment, axis) * r;
            }
        }

        /// <summary>
        /// A flat convex plate extruded along local x (axe blades, tablets): <paramref name="profile"/>
        /// points are (z, y), in order around the outline; <paramref name="halfThickness"/> each side.
        /// </summary>
        public void Plate(IList<Vector2> profile, float halfThickness, EnemySwatch face, EnemySwatch? edge = null)
        {
            int n = profile.Count;
            if (n < 3)
            {
                return;
            }

            Vector3 P(int i, float x) => new Vector3(x, profile[i].y, profile[i].x);
            for (int i = 1; i < n - 1; i++)
            {
                TwoSidedFan(P(0, halfThickness), P(i, halfThickness), P(i + 1, halfThickness), face);
                TwoSidedFan(P(0, -halfThickness), P(i + 1, -halfThickness), P(i, -halfThickness), face);
            }

            for (int i = 0; i < n; i++)
            {
                int j = (i + 1) % n;
                Vector3 a = P(i, -halfThickness), b = P(j, -halfThickness), c = P(j, halfThickness), d = P(i, halfThickness);
                Triangle(a, b, c, edge ?? face);
                Triangle(a, c, d, edge ?? face);
                Triangle(a, c, b, edge ?? face);
                Triangle(a, d, c, edge ?? face);
            }
        }

        private void TwoSidedFan(Vector3 a, Vector3 b, Vector3 c, EnemySwatch swatch) => TwoSided(a, b, c, swatch);

        /// <summary>Builds (or refills) a mesh from everything appended so far.</summary>
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

        private static void EnsureRocks()
        {
            if (_rockVertices != null)
            {
                return;
            }

            var vertices = new Vector3[RockVariants][];
            var triangles = new int[RockVariants][];
            for (int i = 0; i < RockVariants; i++)
            {
                Mesh rock = ProceduralRockGenerator.BuildRockMesh(7301 + i * 977, 1, 0.3f, 0.8f);
                Vector3[] v = rock.vertices;
                triangles[i] = rock.triangles;
                Object.DestroyImmediate(rock);

                // Normalise into a unit box centred on the origin, so "size" is the rock's real size.
                var bounds = new Bounds(v[0], Vector3.zero);
                foreach (Vector3 p in v)
                {
                    bounds.Encapsulate(p);
                }

                Vector3 extent = Vector3.Max(bounds.size, new Vector3(1e-4f, 1e-4f, 1e-4f));
                for (int k = 0; k < v.Length; k++)
                {
                    Vector3 p = v[k] - bounds.center;
                    v[k] = new Vector3(p.x / extent.x, p.y / extent.y, p.z / extent.z);
                }

                vertices[i] = v;
            }

            _rockTriangles = triangles;
            _rockVertices = vertices;
        }

        private static void EnsureIco()
        {
            if (_icoVertices != null)
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
            _icoTriangles = triangles.ToArray();
            _icoVertices = vertices.ToArray();
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
