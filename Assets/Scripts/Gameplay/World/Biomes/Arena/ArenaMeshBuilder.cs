using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Rapadura.Gameplay.World.Biomes
{
    /// <summary>
    /// Tiny flat-shaded, vertex-coloured mesh accumulator for the arena backdrop (tree lines, mesas,
    /// cliffs, cave walls). Every triangle gets its own three vertices and face normal, which gives
    /// the low-poly faceted look and lets one shared vertex-colour material draw the whole backdrop.
    /// Faces are oriented by a "facing" hint, so callers never have to reason about winding order.
    /// </summary>
    public sealed class ArenaMeshBuilder
    {
        private readonly List<Vector3> _vertices = new List<Vector3>();
        private readonly List<Vector3> _normals = new List<Vector3>();
        private readonly List<Color32> _colors = new List<Color32>();
        private readonly List<int> _triangles = new List<int>();

        public int VertexCount => _vertices.Count;
        public IReadOnlyList<Vector3> Vertices => _vertices;

        /// <summary>Adds triangle a-b-c with its front face towards <paramref name="facing"/> (Unity's winding: front = cross(b-a, c-a)).</summary>
        public void Triangle(Vector3 a, Vector3 b, Vector3 c, Color color, Vector3 facing)
        {
            Vector3 normal = Vector3.Cross(b - a, c - a);
            if (normal.sqrMagnitude < 1e-10f)
            {
                return;
            }

            if (Vector3.Dot(normal, facing) < 0f)
            {
                (b, c) = (c, b);
                normal = -normal;
            }

            normal.Normalize();
            int start = _vertices.Count;
            Color32 c32 = color;
            _vertices.Add(a);
            _vertices.Add(b);
            _vertices.Add(c);
            for (int i = 0; i < 3; i++)
            {
                _normals.Add(normal);
                _colors.Add(c32);
                _triangles.Add(start + i);
            }
        }

        /// <summary>Quad a-b-c-d (in order around its edge) facing <paramref name="facing"/>.</summary>
        public void Quad(Vector3 a, Vector3 b, Vector3 c, Vector3 d, Color color, Vector3 facing)
        {
            Triangle(a, b, c, color, facing);
            Triangle(a, c, d, color, facing);
        }

        /// <summary>
        /// Tapered n-gon prism (a frustum) standing on <paramref name="baseCenter"/>, sides facing out
        /// from its axis, optionally capped on top. <paramref name="radiusJitter"/> (per side, 0..1)
        /// roughens the outline deterministically from <paramref name="random"/>.
        /// </summary>
        public void Frustum(Vector3 baseCenter, float bottomRadius, float topRadius, float height, int sides, float yawDegrees,
            Color color, bool capTop, System.Random random = null, float radiusJitter = 0f, Color? capColor = null)
        {
            sides = Mathf.Max(3, sides);
            var bottom = new Vector3[sides];
            var top = new Vector3[sides];
            float yaw = yawDegrees * Mathf.Deg2Rad;
            for (int i = 0; i < sides; i++)
            {
                float angle = yaw + i * Mathf.PI * 2f / sides;
                float jitter = random != null && radiusJitter > 0f ? 1f + ((float)random.NextDouble() * 2f - 1f) * radiusJitter : 1f;
                var dir = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
                bottom[i] = baseCenter + dir * (bottomRadius * jitter);
                top[i] = baseCenter + dir * (topRadius * jitter) + Vector3.up * height;
            }

            Vector3 axisTop = baseCenter + Vector3.up * height;
            for (int i = 0; i < sides; i++)
            {
                int j = (i + 1) % sides;
                Vector3 mid = (bottom[i] + bottom[j] + top[i] + top[j]) * 0.25f;
                Vector3 outward = mid - new Vector3(baseCenter.x, mid.y, baseCenter.z);
                Quad(bottom[i], bottom[j], top[j], top[i], color, outward);
                if (capTop && topRadius > 0.001f)
                {
                    Triangle(axisTop, top[i], top[j], capColor ?? color, Vector3.up);
                }
            }
        }

        /// <summary>n-sided cone (pine tier, stalagmite); a negative <paramref name="height"/> hangs it down (stalactite).</summary>
        public void Cone(Vector3 baseCenter, float radius, float height, int sides, float yawDegrees, Color color, Color? tipColor = null)
        {
            sides = Mathf.Max(3, sides);
            Vector3 tip = baseCenter + Vector3.up * height;
            float yaw = yawDegrees * Mathf.Deg2Rad;
            for (int i = 0; i < sides; i++)
            {
                float a0 = yaw + i * Mathf.PI * 2f / sides;
                float a1 = yaw + (i + 1) * Mathf.PI * 2f / sides;
                Vector3 p0 = baseCenter + new Vector3(Mathf.Cos(a0), 0f, Mathf.Sin(a0)) * radius;
                Vector3 p1 = baseCenter + new Vector3(Mathf.Cos(a1), 0f, Mathf.Sin(a1)) * radius;
                Vector3 mid = (p0 + p1) * 0.5f;
                Vector3 outward = mid - baseCenter + Vector3.up * (height >= 0f ? radius * 0.3f : -radius * 0.3f);
                Triangle(p0, p1, tip, tipColor.HasValue && i % 2 == 0 ? Color.Lerp(color, tipColor.Value, 0.5f) : color, outward);
            }
        }

        /// <summary>Squashed octahedron blob (broadleaf crown, boulder).</summary>
        public void Blob(Vector3 center, Vector3 radii, float yawDegrees, Color color, Color shade)
        {
            Quaternion rotation = Quaternion.Euler(0f, yawDegrees, 0f);
            Vector3 up = center + Vector3.up * radii.y;
            Vector3 down = center - Vector3.up * radii.y * 0.6f;
            var ring = new Vector3[6];
            for (int i = 0; i < ring.Length; i++)
            {
                float angle = i * Mathf.PI * 2f / ring.Length;
                ring[i] = center + rotation * new Vector3(Mathf.Cos(angle) * radii.x, 0f, Mathf.Sin(angle) * radii.z);
            }

            for (int i = 0; i < ring.Length; i++)
            {
                Vector3 a = ring[i];
                Vector3 b = ring[(i + 1) % ring.Length];
                Vector3 outward = (a + b) * 0.5f - center;
                Triangle(a, b, up, color, outward + Vector3.up * 0.5f);
                Triangle(a, b, down, shade, outward - Vector3.up * 0.5f);
            }
        }

        /// <summary>Builds the mesh (bounds recalculated; stays readable).</summary>
        public Mesh ToMesh(string name)
        {
            var mesh = new Mesh { name = name };
            mesh.indexFormat = _vertices.Count > 65000 ? IndexFormat.UInt32 : IndexFormat.UInt16;
            mesh.SetVertices(_vertices);
            mesh.SetNormals(_normals);
            mesh.SetColors(_colors);
            mesh.SetTriangles(_triangles, 0, true);
            return mesh;
        }
    }
}
