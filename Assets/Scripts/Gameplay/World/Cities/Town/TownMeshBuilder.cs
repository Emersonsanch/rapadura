using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Rapadura.Gameplay.World.Cities.Town
{
    /// <summary>
    /// Flat-shaded low-poly appender of the town kit (sibling of <c>HouseMeshBuilder</c>/<c>WorldMapMeshBuilder</c>):
    /// boxes, frusta, logs, blobs, discs and slabs into one vertex list, every face coloured through a
    /// <see cref="TownPalette"/> cell UV. Pure and deterministic until <see cref="ToMesh"/> (EditMode-testable).
    /// Everything goes through <see cref="Matrix"/> (set it to place a module).
    /// </summary>
    public sealed class TownMeshBuilder
    {
        private readonly List<Vector3> _vertices = new List<Vector3>(2048);
        private readonly List<Vector3> _normals = new List<Vector3>(2048);
        private readonly List<Vector2> _uvs = new List<Vector2>(2048);
        private readonly List<int> _triangles = new List<int>(4096);

        /// <summary>Applied to every shape added from now on.</summary>
        public Matrix4x4 Matrix = Matrix4x4.identity;

        public int TriangleCount => _triangles.Count / 3;
        public int VertexCount => _vertices.Count;
        public bool IsEmpty => _vertices.Count == 0;
        public IReadOnlyList<Vector3> Vertices => _vertices;

        public void Clear()
        {
            _vertices.Clear();
            _normals.Clear();
            _uvs.Clear();
            _triangles.Clear();
            Matrix = Matrix4x4.identity;
        }

        /// <summary>Runs <paramref name="build"/> with <see cref="Matrix"/> multiplied by a local TRS, then restores it.</summary>
        public void With(Vector3 position, float yaw, Vector3 scale, System.Action build)
        {
            Matrix4x4 saved = Matrix;
            Matrix = saved * Matrix4x4.TRS(position, Quaternion.Euler(0f, yaw, 0f), scale);
            build();
            Matrix = saved;
        }

        public void With(Vector3 position, float yaw, System.Action build) => With(position, yaw, Vector3.one, build);

        /// <summary>Appends everything another builder holds (its own matrix already applied), through this one's matrix.</summary>
        public void Append(TownMeshBuilder other)
        {
            if (other == null || other.IsEmpty)
            {
                return;
            }

            bool identity = Matrix == Matrix4x4.identity;
            bool flip = Matrix.determinant < 0f;
            int start = _vertices.Count;
            for (int i = 0; i < other._vertices.Count; i++)
            {
                _vertices.Add(identity ? other._vertices[i] : Matrix.MultiplyPoint3x4(other._vertices[i]));
                _normals.Add(identity ? other._normals[i] : Matrix.MultiplyVector(other._normals[i]).normalized);
                _uvs.Add(other._uvs[i]);
            }

            for (int i = 0; i < other._triangles.Count; i += 3)
            {
                _triangles.Add(start + other._triangles[i]);
                _triangles.Add(start + (flip ? other._triangles[i + 2] : other._triangles[i + 1]));
                _triangles.Add(start + (flip ? other._triangles[i + 1] : other._triangles[i + 2]));
            }
        }

        /// <summary>One flat triangle (clockwise seen from the front, Unity's winding).</summary>
        public void Triangle(Vector3 a, Vector3 b, Vector3 c, TownSwatch swatch)
        {
            a = Matrix.MultiplyPoint3x4(a);
            b = Matrix.MultiplyPoint3x4(b);
            c = Matrix.MultiplyPoint3x4(c);
            if (Matrix.determinant < 0f)
            {
                (b, c) = (c, b);
            }

            Vector3 normal = Vector3.Cross(b - a, c - a);
            if (normal.sqrMagnitude < 1e-14f)
            {
                return;
            }

            normal.Normalize();
            Vector2 uv = TownPalette.UvOf(swatch);
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
        public void Quad(Vector3 a, Vector3 b, Vector3 c, Vector3 d, TownSwatch swatch)
        {
            Triangle(a, b, c, swatch);
            Triangle(a, c, d, swatch);
        }

        public void TwoSidedQuad(Vector3 a, Vector3 b, Vector3 c, Vector3 d, TownSwatch swatch)
        {
            Quad(a, b, c, d, swatch);
            Quad(a, d, c, b, swatch);
        }

        /// <summary>Triangle wound so its face points along <paramref name="facing"/> (local space): no winding bookkeeping in roof code.</summary>
        public void TriangleFacing(Vector3 a, Vector3 b, Vector3 c, Vector3 facing, TownSwatch swatch)
        {
            if (Vector3.Dot(Vector3.Cross(b - a, c - a), facing) < 0f)
            {
                Triangle(a, c, b, swatch);
            }
            else
            {
                Triangle(a, b, c, swatch);
            }
        }

        /// <summary>Planar quad (a, b, c, d in order around it) wound to face <paramref name="facing"/>.</summary>
        public void QuadFacing(Vector3 a, Vector3 b, Vector3 c, Vector3 d, Vector3 facing, TownSwatch swatch)
        {
            if (Vector3.Dot(Vector3.Cross(b - a, c - a), facing) < 0f)
            {
                Quad(a, d, c, b, swatch);
            }
            else
            {
                Quad(a, b, c, d, swatch);
            }
        }

        /// <summary>
        /// A convex roof plate: the polygon <paramref name="top"/> (in order around it) as the upper face, the same
        /// polygon <paramref name="thickness"/> lower as the soffit, and the edge faces. Windings are fixed automatically.
        /// </summary>
        public void Plate(IReadOnlyList<Vector3> top, float thickness, TownSwatch upper, TownSwatch under)
        {
            int count = top.Count;
            if (count < 3)
            {
                return;
            }

            Vector3 centroid = Vector3.zero;
            foreach (Vector3 p in top)
            {
                centroid += p;
            }

            centroid /= count;
            Vector3 normal = Vector3.Cross(top[1] - top[0], top[2] - top[0]);
            if (normal.y < 0f)
            {
                normal = -normal;
            }

            normal.Normalize();
            Vector3 down = Vector3.down * thickness;
            for (int i = 1; i < count - 1; i++)
            {
                TriangleFacing(top[0], top[i], top[i + 1], normal, upper);
                TriangleFacing(top[0] + down, top[i] + down, top[i + 1] + down, -normal, under);
            }

            for (int i = 0; i < count; i++)
            {
                Vector3 a = top[i];
                Vector3 b = top[(i + 1) % count];
                Vector3 mid = (a + b) * 0.5f;
                Vector3 outward = mid - centroid;
                outward.y = 0f;
                QuadFacing(a, b, b + down, a + down, outward.sqrMagnitude > 1e-6f ? outward : Vector3.forward, under);
            }
        }

        /// <summary>Box centred on <paramref name="center"/>, turned <paramref name="yaw"/> degrees about +y.</summary>
        public void Box(Vector3 center, Vector3 size, TownSwatch swatch, float yaw = 0f, TownSwatch? top = null, bool bottom = false)
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

            Quad(p000, p010, p110, p100, swatch);
            Quad(p101, p111, p011, p001, swatch);
            Quad(p001, p011, p010, p000, swatch);
            Quad(p100, p110, p111, p101, swatch);
            Quad(p010, p011, p111, p110, top ?? swatch);
            if (bottom)
            {
                Quad(p001, p000, p100, p101, swatch);
            }

            Matrix = saved;
        }

        /// <summary>Axis-aligned box spanning <paramref name="min"/>..<paramref name="max"/> (no bottom face: it rests on something).</summary>
        public void BoxMinMax(Vector3 min, Vector3 max, TownSwatch swatch, TownSwatch? top = null)
        {
            Box((min + max) * 0.5f, max - min, swatch, 0f, top);
        }

        /// <summary>
        /// Frustum along +y: <paramref name="bottomRadius"/> at the base, <paramref name="topRadius"/> at
        /// <paramref name="height"/> (0 = cone). Flat-shaded sides; optional top cap.
        /// </summary>
        public void Cylinder(Vector3 baseCenter, float bottomRadius, float topRadius, float height, int sides, TownSwatch swatch,
            TownSwatch? cap = null, float phase = 0f)
        {
            sides = Mathf.Max(3, sides);
            Vector3 top = baseCenter + Vector3.up * height;
            for (int i = 0; i < sides; i++)
            {
                float a0 = (i + phase) * Mathf.PI * 2f / sides;
                float a1 = (i + 1 + phase) * Mathf.PI * 2f / sides;
                var d0 = new Vector3(Mathf.Cos(a0), 0f, Mathf.Sin(a0));
                var d1 = new Vector3(Mathf.Cos(a1), 0f, Mathf.Sin(a1));
                Vector3 b0 = baseCenter + d0 * bottomRadius;
                Vector3 b1 = baseCenter + d1 * bottomRadius;
                if (topRadius > 1e-4f)
                {
                    Vector3 t0 = top + d0 * topRadius;
                    Vector3 t1 = top + d1 * topRadius;
                    Quad(b0, t0, t1, b1, swatch);
                    if (cap.HasValue)
                    {
                        Triangle(top, t1, t0, cap.Value);
                    }
                }
                else
                {
                    Triangle(b0, top, b1, swatch);
                }
            }
        }

        /// <summary>A beam/log from <paramref name="from"/> to <paramref name="to"/> (round with 6 sides, or square with 4).</summary>
        public void Log(Vector3 from, Vector3 to, float radius, TownSwatch swatch, int sides = 6, TownSwatch? ends = null)
        {
            Vector3 axis = to - from;
            float length = axis.magnitude;
            if (length < 1e-4f)
            {
                return;
            }

            Matrix4x4 saved = Matrix;
            Matrix = saved * Matrix4x4.TRS(from, Quaternion.FromToRotation(Vector3.up, axis / length), Vector3.one);
            Cylinder(Vector3.zero, radius, radius, length, sides, swatch, ends ?? swatch, sides == 4 ? 0.5f : 0f);
            Matrix = saved;
        }

        /// <summary>Octahedron "blob" (small foliage, sacks, fruit): 8 flat faces.</summary>
        public void Blob(Vector3 center, Vector3 size, TownSwatch swatch)
        {
            Vector3 h = size * 0.5f;
            Vector3 up = center + Vector3.up * h.y, down = center - Vector3.up * h.y;
            Vector3 e = center + Vector3.right * h.x, w = center - Vector3.right * h.x;
            Vector3 n = center + Vector3.forward * h.z, s = center - Vector3.forward * h.z;
            Triangle(up, n, e, swatch);
            Triangle(up, e, s, swatch);
            Triangle(up, s, w, swatch);
            Triangle(up, w, n, swatch);
            Triangle(down, e, n, swatch);
            Triangle(down, s, e, swatch);
            Triangle(down, w, s, swatch);
            Triangle(down, n, w, swatch);
        }

        private static readonly Vector3[] IcoVertices = BuildIcoVertices();

        private static readonly int[] IcoFaces =
        {
            0, 11, 5, 0, 5, 1, 0, 1, 7, 0, 7, 10, 0, 10, 11,
            1, 5, 9, 5, 11, 4, 11, 10, 2, 10, 7, 6, 7, 1, 8,
            3, 9, 4, 3, 4, 2, 3, 2, 6, 3, 6, 8, 3, 8, 9,
            4, 9, 5, 2, 4, 11, 6, 2, 10, 8, 6, 7, 9, 8, 1,
        };

        private static Vector3[] BuildIcoVertices()
        {
            float t = (1f + Mathf.Sqrt(5f)) / 2f;
            var v = new[]
            {
                new Vector3(-1, t, 0), new Vector3(1, t, 0), new Vector3(-1, -t, 0), new Vector3(1, -t, 0),
                new Vector3(0, -1, t), new Vector3(0, 1, t), new Vector3(0, -1, -t), new Vector3(0, 1, -t),
                new Vector3(t, 0, -1), new Vector3(t, 0, 1), new Vector3(-t, 0, -1), new Vector3(-t, 0, 1),
            };
            for (int i = 0; i < v.Length; i++)
            {
                v[i] = v[i].normalized;
            }

            return v;
        }

        /// <summary>Chunky icosahedron (20 faces) scaled per axis: tree crowns, hedges, hills.</summary>
        public void Crown(Vector3 center, Vector3 size, TownSwatch swatch, float yaw = 0f)
        {
            Matrix4x4 saved = Matrix;
            Matrix = saved * Matrix4x4.TRS(center, Quaternion.Euler(0f, yaw, 0f), size * 0.5f);
            for (int i = 0; i < IcoFaces.Length; i += 3)
            {
                Triangle(IcoVertices[IcoFaces[i]], IcoVertices[IcoFaces[i + 1]], IcoVertices[IcoFaces[i + 2]], swatch);
            }

            Matrix = saved;
        }

        /// <summary>Flat n-gon facing up at height <paramref name="y"/> (a fan).</summary>
        public void Disc(Vector3 center, float radius, int sides, TownSwatch swatch, float phase = 0f)
        {
            sides = Mathf.Max(3, sides);
            for (int i = 0; i < sides; i++)
            {
                float a0 = (i + phase) * Mathf.PI * 2f / sides;
                float a1 = (i + 1 + phase) * Mathf.PI * 2f / sides;
                Triangle(center, center + new Vector3(Mathf.Cos(a1), 0f, Mathf.Sin(a1)) * radius,
                    center + new Vector3(Mathf.Cos(a0), 0f, Mathf.Sin(a0)) * radius, swatch);
            }
        }

        /// <summary>Flat ring (annulus) facing up between two radii.</summary>
        public void Ring(Vector3 center, float innerRadius, float outerRadius, int sides, TownSwatch swatch, TownSwatch? alternate = null)
        {
            sides = Mathf.Max(3, sides);
            for (int i = 0; i < sides; i++)
            {
                float a0 = i * Mathf.PI * 2f / sides;
                float a1 = (i + 1) * Mathf.PI * 2f / sides;
                var d0 = new Vector3(Mathf.Cos(a0), 0f, Mathf.Sin(a0));
                var d1 = new Vector3(Mathf.Cos(a1), 0f, Mathf.Sin(a1));
                TownSwatch s = alternate.HasValue && (i & 1) == 1 ? alternate.Value : swatch;
                Quad(center + d0 * innerRadius, center + d1 * innerRadius, center + d1 * outerRadius, center + d0 * outerRadius, s);
            }
        }

        /// <summary>Upward-facing flat rectangle (min/max on x and z) at height <paramref name="y"/>.</summary>
        public void Flat(float xMin, float zMin, float xMax, float zMax, float y, TownSwatch swatch)
        {
            Quad(new Vector3(xMin, y, zMin), new Vector3(xMin, y, zMax), new Vector3(xMax, y, zMax), new Vector3(xMax, y, zMin), swatch);
        }

        /// <summary>
        /// A sloped roof slab: the quad <paramref name="a"/>-<paramref name="b"/>-<paramref name="c"/>-<paramref name="d"/>
        /// (clockwise seen from above) as a top face, an underside <paramref name="thickness"/> lower and the edges.
        /// </summary>
        public void Slab(Vector3 a, Vector3 b, Vector3 c, Vector3 d, float thickness, TownSwatch top, TownSwatch under)
        {
            Vector3 down = Vector3.down * thickness;
            Quad(a, b, c, d, top);
            Quad(a + down, d + down, c + down, b + down, under);
            Quad(a + down, a, d, d + down, under);
            Quad(b + down, b, a, a + down, under);
            Quad(c + down, c, b, b + down, under);
            Quad(d + down, d, c, c + down, under);
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
    }
}
