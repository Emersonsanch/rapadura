using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Rapadura.Gameplay.Housing.Visuals
{
    /// <summary>
    /// Flat-shaded low-poly appender for the house (sibling of <c>EnemyMeshBuilder</c> / <c>WorldMapMeshBuilder</c>):
    /// boxes, frusta, gable prisms and octahedron "blobs" into one vertex list, every face coloured through a
    /// <see cref="HousePalette"/> cell UV. Pure and deterministic: the same calls always give the same mesh.
    /// </summary>
    public sealed class HouseMeshBuilder
    {
        private readonly List<Vector3> _vertices = new List<Vector3>(512);
        private readonly List<Vector3> _normals = new List<Vector3>(512);
        private readonly List<Vector2> _uvs = new List<Vector2>(512);
        private readonly List<int> _triangles = new List<int>(1024);

        /// <summary>Applied to every shape added from now on.</summary>
        public Matrix4x4 Matrix = Matrix4x4.identity;

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

        /// <summary>One flat triangle (clockwise seen from the front).</summary>
        public void Triangle(Vector3 a, Vector3 b, Vector3 c, HouseSwatch swatch)
        {
            a = Matrix.MultiplyPoint3x4(a);
            b = Matrix.MultiplyPoint3x4(b);
            c = Matrix.MultiplyPoint3x4(c);
            Vector3 normal = Vector3.Cross(b - a, c - a);
            if (normal.sqrMagnitude < 1e-14f)
            {
                return;
            }

            if (Matrix.determinant < 0f)
            {
                (b, c) = (c, b);
                normal = -normal;
            }

            normal.Normalize();
            Vector2 uv = HousePalette.UvOf(swatch);
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

        public void Quad(Vector3 a, Vector3 b, Vector3 c, Vector3 d, HouseSwatch swatch)
        {
            Triangle(a, b, c, swatch);
            Triangle(a, c, d, swatch);
        }

        public void TwoSidedQuad(Vector3 a, Vector3 b, Vector3 c, Vector3 d, HouseSwatch swatch)
        {
            Quad(a, b, c, d, swatch);
            Quad(a, d, c, b, swatch);
        }

        /// <summary>Box centred on <paramref name="center"/>, turned <paramref name="yaw"/> degrees about +y.</summary>
        public void Box(Vector3 center, Vector3 size, HouseSwatch swatch, float yaw = 0f, HouseSwatch? top = null)
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
            Quad(p001, p000, p100, p101, swatch);

            Matrix = saved;
        }

        /// <summary>
        /// The four vertical faces of an axis-aligned box (no top, no bottom): wall boards and battens, whose ends are
        /// always covered by a plate, a sill or a frame. 8 triangles instead of 12.
        /// </summary>
        public void BoxSidesMinMax(Vector3 min, Vector3 max, HouseSwatch swatch)
        {
            var p000 = new Vector3(min.x, min.y, min.z);
            var p100 = new Vector3(max.x, min.y, min.z);
            var p010 = new Vector3(min.x, max.y, min.z);
            var p110 = new Vector3(max.x, max.y, min.z);
            var p001 = new Vector3(min.x, min.y, max.z);
            var p101 = new Vector3(max.x, min.y, max.z);
            var p011 = new Vector3(min.x, max.y, max.z);
            var p111 = new Vector3(max.x, max.y, max.z);
            Quad(p000, p010, p110, p100, swatch);
            Quad(p101, p111, p011, p001, swatch);
            Quad(p001, p011, p010, p000, swatch);
            Quad(p100, p110, p111, p101, swatch);
        }

        /// <summary>Box spanning <paramref name="min"/>..<paramref name="max"/> (axis aligned).</summary>
        public void BoxMinMax(Vector3 min, Vector3 max, HouseSwatch swatch, HouseSwatch? top = null)
        {
            Box((min + max) * 0.5f, max - min, swatch, 0f, top);
        }

        /// <summary>Frustum along +y: <paramref name="bottomRadius"/> at the base, <paramref name="topRadius"/> at <paramref name="height"/>.</summary>
        public void Cylinder(Vector3 baseCenter, float bottomRadius, float topRadius, float height, int sides, HouseSwatch swatch, HouseSwatch? cap = null)
        {
            sides = Mathf.Max(3, sides);
            for (int i = 0; i < sides; i++)
            {
                float a0 = i * Mathf.PI * 2f / sides;
                float a1 = (i + 1) * Mathf.PI * 2f / sides;
                var d0 = new Vector3(Mathf.Cos(a0), 0f, Mathf.Sin(a0));
                var d1 = new Vector3(Mathf.Cos(a1), 0f, Mathf.Sin(a1));
                Vector3 b0 = baseCenter + d0 * bottomRadius;
                Vector3 b1 = baseCenter + d1 * bottomRadius;
                Vector3 t0 = baseCenter + d0 * topRadius + Vector3.up * height;
                Vector3 t1 = baseCenter + d1 * topRadius + Vector3.up * height;
                if (topRadius > 1e-4f)
                {
                    Quad(b0, t0, t1, b1, swatch);
                    Triangle(baseCenter + Vector3.up * height, t1, t0, cap ?? swatch);
                }
                else
                {
                    Triangle(b0, t0, b1, swatch);
                }
            }
        }

        /// <summary>A horizontal log from <paramref name="from"/> to <paramref name="to"/> (6-sided).</summary>
        public void Log(Vector3 from, Vector3 to, float radius, HouseSwatch swatch, HouseSwatch? ends = null)
        {
            Vector3 axis = to - from;
            float length = axis.magnitude;
            if (length < 1e-4f) return;
            Matrix4x4 saved = Matrix;
            Matrix = saved * Matrix4x4.TRS(from, Quaternion.FromToRotation(Vector3.up, axis / length), Vector3.one);
            Cylinder(Vector3.zero, radius, radius, length, 6, swatch, ends);
            Matrix = saved;
        }

        /// <summary>Octahedron "blob" (foliage, cushions): 8 flat faces.</summary>
        public void Blob(Vector3 center, Vector3 size, HouseSwatch swatch)
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

        /// <summary>Gable roof slab: ridge along x at <paramref name="ridgeHeight"/> over eaves at y = <paramref name="eaveY"/>.</summary>
        public void GableRoof(float xMin, float xMax, float zMin, float zMax, float eaveY, float ridgeHeight, float thickness, HouseSwatch swatch, HouseSwatch under)
        {
            float zMid = (zMin + zMax) * 0.5f;
            var r0 = new Vector3(xMin, eaveY + ridgeHeight, zMid);
            var r1 = new Vector3(xMax, eaveY + ridgeHeight, zMid);
            var s0 = new Vector3(xMin, eaveY, zMin);
            var s1 = new Vector3(xMax, eaveY, zMin);
            var n0 = new Vector3(xMin, eaveY, zMax);
            var n1 = new Vector3(xMax, eaveY, zMax);
            Vector3 down = Vector3.down * thickness;
            Quad(s0, r0, r1, s1, swatch);
            Quad(n1, r1, r0, n0, swatch);
            Quad(s1 + down, r1 + down, r0 + down, s0 + down, under);
            Quad(n0 + down, r0 + down, r1 + down, n1 + down, under);
            Quad(s0 + down, s0, s1, s1 + down, under);
            Quad(n1 + down, n1, n0, n0 + down, under);
        }

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
