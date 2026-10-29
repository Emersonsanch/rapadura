using System;
using System.Collections.Generic;
using UnityEngine;

namespace Rapadura.Gameplay.Enemies.Visuals
{
    /// <summary>The five enemies without a Meshy model, built from code (see <see cref="ProceduralEnemyModels"/>).</summary>
    public enum ProceduralEnemyKind
    {
        None = 0,
        BatSwarm = 1,
        StoneGolem = 2,
        Warlord = 3,
        RuinGuardian = 4,
        AncientDweller = 5,
    }

    /// <summary>
    /// A procedural enemy model as data: a flat list of named parts (index of the parent, local
    /// pose, optional mesh). Meshes can be shared by several parts (the bats of the swarm). Turned
    /// into a GameObject hierarchy by <see cref="ProceduralEnemyModels.Instantiate(ProceduralEnemyBlueprint, Material, Transform)"/>
    /// or baked to assets by the editor menu. Owns its meshes: <see cref="DestroyMeshes"/> them when
    /// the blueprint is not cached.
    /// </summary>
    public sealed class ProceduralEnemyBlueprint
    {
        public struct Part
        {
            public string Name;
            /// <summary>Index of the parent part, or -1 for a child of the model root.</summary>
            public int Parent;
            public Vector3 Position;
            public Quaternion Rotation;
            public Vector3 Scale;
            /// <summary>Null for a pivot-only node.</summary>
            public Mesh Mesh;
        }

        private readonly List<Part> _parts = new List<Part>();
        private readonly List<Mesh> _meshes = new List<Mesh>();
        private readonly Dictionary<string, Mesh> _meshByKey = new Dictionary<string, Mesh>(StringComparer.Ordinal);
        private readonly EnemyMeshBuilder _builder = new EnemyMeshBuilder();

        public ProceduralEnemyBlueprint(ProceduralEnemyKind kind, bool isBoss)
        {
            Kind = kind;
            IsBoss = isBoss;
        }

        public ProceduralEnemyKind Kind { get; }
        public bool IsBoss { get; }
        public IReadOnlyList<Part> Parts => _parts;

        /// <summary>Distinct meshes (each listed once even when several parts share it).</summary>
        public IReadOnlyList<Mesh> Meshes => _meshes;

        /// <summary>Triangles drawn by the whole model (a shared mesh counts once per part using it).</summary>
        public int TriangleCount
        {
            get
            {
                int total = 0;
                foreach (Part part in _parts)
                {
                    if (part.Mesh != null)
                    {
                        total += part.Mesh.triangles.Length / 3;
                    }
                }

                return total;
            }
        }

        public int IndexOf(string partName)
        {
            for (int i = 0; i < _parts.Count; i++)
            {
                if (_parts[i].Name == partName)
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>Adds a pivot-only node; returns its index (pass as <c>parent</c> of its children).</summary>
        public int Node(string name, int parent, Vector3 position, Quaternion? rotation = null, Vector3? scale = null)
        {
            return AddPart(name, parent, position, rotation, scale, null);
        }

        /// <summary>
        /// Adds a part whose mesh is drawn by <paramref name="draw"/> in the part's local space. Parts
        /// passing the same <paramref name="meshKey"/> share one mesh (drawn once, by the first).
        /// </summary>
        public int Mesh(string name, int parent, Vector3 position, Action<EnemyMeshBuilder> draw, Quaternion? rotation = null,
            Vector3? scale = null, string meshKey = null)
        {
            string key = meshKey ?? name;
            if (!_meshByKey.TryGetValue(key, out Mesh mesh))
            {
                _builder.Clear();
                draw(_builder);
                mesh = _builder.ToMesh("ProcEnemy_" + Kind + "_" + key);
                _meshByKey[key] = mesh;
                _meshes.Add(mesh);
            }

            return AddPart(name, parent, position, rotation, scale, mesh);
        }

        private int AddPart(string name, int parent, Vector3 position, Quaternion? rotation, Vector3? scale, Mesh mesh)
        {
            if (parent >= _parts.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(parent), "A part's parent must be added before it.");
            }

            _parts.Add(new Part
            {
                Name = name,
                Parent = parent,
                Position = position,
                Rotation = rotation ?? Quaternion.identity,
                Scale = scale ?? Vector3.one,
                Mesh = mesh,
            });
            return _parts.Count - 1;
        }

        /// <summary>Destroys the meshes (only for blueprints that are not cached/baked).</summary>
        public void DestroyMeshes()
        {
            foreach (Mesh mesh in _meshes)
            {
                if (mesh != null)
                {
                    UnityEngine.Object.DestroyImmediate(mesh);
                }
            }

            _meshes.Clear();
            _meshByKey.Clear();
        }
    }
}
