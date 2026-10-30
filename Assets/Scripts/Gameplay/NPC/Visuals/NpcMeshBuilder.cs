using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Rapadura.Gameplay.NPC.Visuals
{
    /// <summary>
    /// Flat-shaded low-poly shape appender for the NPCs (the NPC twin of <c>EnemyMeshBuilder</c>): boxes, frusta, limb
    /// segments, icosphere blobs and flat face decals go into one vertex list, each face coloured through an
    /// <see cref="NpcPalette"/> cell UV, and each vertex rigidly bound to the current <see cref="Bone"/> (weight 1), so a
    /// whole figure is one skinned mesh on the shared palette material (one renderer, five bones). Every shape goes
    /// through <see cref="Matrix"/> plus its own local transform. Pure and deterministic.
    /// </summary>
    public sealed class NpcMeshBuilder
    {
        private readonly List<Vector3> _vertices = new List<Vector3>(2048);
        private readonly List<Vector3> _normals = new List<Vector3>(2048);
        private readonly List<Vector2> _uvs = new List<Vector2>(2048);
        private readonly List<int> _bones = new List<int>(2048);
        private readonly List<int> _triangles = new List<int>(4096);

        private static Vector3[] _icoVertices;
        private static int[] _icoTriangles;
        private static int[] _coarseIcoTriangles;

        /// <summary>Applied to every shape added from now on.</summary>
        public Matrix4x4 Matrix = Matrix4x4.identity;

        /// <summary>Bone index the next vertices are bound to (see <see cref="NpcBones"/>).</summary>
        public int Bone;

        public int VertexCount => _vertices.Count;
        public int TriangleCount => _triangles.Count / 3;
        public IReadOnlyList<Vector3> Vertices => _vertices;
        public IReadOnlyList<int> VertexBones => _bones;

        public void Triangle(Vector3 a, Vector3 b, Vector3 c, NpcSwatch swatch)
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
            Vector2 uv = NpcPalette.UvOf(swatch);
            int start = _vertices.Count;
            _vertices.Add(a);
            _vertices.Add(b);
            _vertices.Add(c);
            for (int i = 0; i < 3; i++)
            {
                _normals.Add(normal);
                _uvs.Add(uv);
                _bones.Add(Bone);
            }

            _triangles.Add(start);
            _triangles.Add(start + 1);
            _triangles.Add(start + 2);
        }

        public void Quad(Vector3 a, Vector3 b, Vector3 c, Vector3 d, NpcSwatch swatch)
        {
            Triangle(a, b, c, swatch);
            Triangle(a, c, d, swatch);
        }

        /// <summary>Double-sided triangle (capes, tails of a scarf).</summary>
        public void TwoSided(Vector3 a, Vector3 b, Vector3 c, NpcSwatch swatch, NpcSwatch? back = null)
        {
            Triangle(a, b, c, swatch);
            Triangle(a, c, b, back ?? swatch);
        }

        /// <summary>A flat rectangle facing local +z (a face decal: eye, brow, mouth, apron pocket), one-sided.</summary>
        public void Decal(Vector3 center, Vector2 size, NpcSwatch swatch, Quaternion? rotation = null)
        {
            Matrix4x4 saved = Matrix;
            Matrix = saved * Matrix4x4.TRS(center, rotation ?? Quaternion.identity, new Vector3(size.x, size.y, 1f));
            Quad(new Vector3(0.5f, -0.5f, 0f), new Vector3(0.5f, 0.5f, 0f), new Vector3(-0.5f, 0.5f, 0f), new Vector3(-0.5f, -0.5f, 0f), swatch);
            Matrix = saved;
        }

        /// <summary>Box centred on <paramref name="center"/>; <paramref name="noBottom"/> skips the unseen underside.</summary>
        public void Box(Vector3 center, Vector3 size, NpcSwatch swatch, Quaternion? rotation = null, NpcSwatch? top = null, bool noBottom = false)
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
            if (!noBottom)
            {
                Quad(p001, p000, p100, p101, swatch);
            }

            Matrix = saved;
        }

        /// <summary>
        /// Frustum along local +y: <paramref name="bottomRadius"/> at <paramref name="baseCenter"/>, <paramref name="topRadius"/>
        /// at <paramref name="height"/> (0 = cone); <paramref name="depth"/> squashes it front-to-back (a torso is not round).
        /// </summary>
        public void Cylinder(Vector3 baseCenter, float bottomRadius, float topRadius, float height, int sides, NpcSwatch swatch,
            NpcSwatch? cap = null, Quaternion? rotation = null, bool bottomCap = false, bool topCap = true, float depth = 1f, float twist = 0f)
        {
            sides = Mathf.Max(3, sides);
            Matrix4x4 saved = Matrix;
            Matrix = saved * Matrix4x4.TRS(baseCenter, (rotation ?? Quaternion.identity) * Quaternion.Euler(0f, twist, 0f), new Vector3(1f, 1f, depth));

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
                    if (topCap)
                    {
                        Triangle(top, t1, t0, cap ?? swatch);
                    }
                }

                if (bottomCap && bottomRadius > 0.0001f)
                {
                    Triangle(Vector3.zero, b0, b1, cap ?? swatch);
                }
            }

            Matrix = saved;
        }

        /// <summary>A tapered limb segment from <paramref name="from"/> to <paramref name="to"/> (both ends capped).</summary>
        public void Segment(Vector3 from, Vector3 to, float fromRadius, float toRadius, int sides, NpcSwatch swatch, NpcSwatch? cap = null, bool caps = true)
        {
            Vector3 along = to - from;
            float length = along.magnitude;
            if (length < 1e-5f)
            {
                return;
            }

            Cylinder(from, fromRadius, toRadius, length, sides, swatch, cap, Quaternion.FromToRotation(Vector3.up, along / length), bottomCap: caps, topCap: caps);
        }

        /// <summary>Icosphere blob of diameter <paramref name="size"/> per axis: 80 faces, 20 when <paramref name="coarse"/>.</summary>
        public void Blob(Vector3 center, Vector3 size, NpcSwatch swatch, Quaternion? rotation = null, bool coarse = false)
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
        /// Upper half of an 80-face icosphere blob (faces whose centre is above <paramref name="cutY"/> in unit space): hair
        /// caps, domes of helmets and hoods without paying for the hidden half.
        /// </summary>
        public void Dome(Vector3 center, Vector3 size, NpcSwatch swatch, Quaternion? rotation = null, float cutY = -0.15f, float backCutZ = 2f)
        {
            EnsureIco();
            Matrix4x4 saved = Matrix;
            Matrix = saved * Matrix4x4.TRS(center, rotation ?? Quaternion.identity, size * 0.5f);
            for (int i = 0; i < _icoTriangles.Length; i += 3)
            {
                Vector3 a = _icoVertices[_icoTriangles[i]];
                Vector3 b = _icoVertices[_icoTriangles[i + 1]];
                Vector3 c = _icoVertices[_icoTriangles[i + 2]];
                Vector3 mid = (a + b + c) / 3f;
                // Keep the top; below the cut keep only the back (hair falling behind the face), if asked.
                if (mid.y >= cutY || mid.z < -backCutZ)
                {
                    Triangle(a, b, c, swatch);
                }
            }

            Matrix = saved;
        }

        /// <summary>Builds the skinned mesh: rigid weights from <see cref="Bone"/>, the given bind poses.</summary>
        public Mesh ToMesh(string name, Matrix4x4[] bindPoses)
        {
            var mesh = new Mesh { name = name, hideFlags = HideFlags.DontSave };
            mesh.indexFormat = _vertices.Count > 65000 ? IndexFormat.UInt32 : IndexFormat.UInt16;
            mesh.SetVertices(_vertices);
            mesh.SetNormals(_normals);
            mesh.SetUVs(0, _uvs);
            mesh.SetTriangles(_triangles, 0);
            var weights = new BoneWeight[_bones.Count];
            for (int i = 0; i < weights.Length; i++)
            {
                weights[i] = new BoneWeight { boneIndex0 = _bones[i], weight0 = 1f };
            }

            mesh.boneWeights = weights;
            mesh.bindposes = bindPoses;
            mesh.RecalculateBounds();
            return mesh;
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
