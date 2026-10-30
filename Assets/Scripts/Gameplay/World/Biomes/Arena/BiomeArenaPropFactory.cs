using System.Collections.Generic;
using Rapadura.Environment;
using UnityEngine;
using UnityEngine.Rendering;

namespace Rapadura.Gameplay.World.Biomes
{
    /// <summary>
    /// Builds and pools the cheap procedural arena props used by <see cref="BiomeArenaDressing"/>:
    /// stacked Unity primitives (saxauls, stalagmites) and <see cref="ProceduralRockGenerator"/> rocks
    /// (boulders, snow-capped rocks). Every part keeps the primitive's default (pipeline) material
    /// and is coloured through a MaterialPropertyBlock, so no material is ever created or modified.
    /// Only the main part keeps a collider. Props are built once per kind on demand and reused
    /// across applies (<see cref="Release"/> just deactivates them).
    /// </summary>
    public sealed class BiomeArenaPropFactory
    {
        private const int RockMeshVariants = 3;

        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorId = Shader.PropertyToID("_Color");

        public sealed class Prop
        {
            public BiomeArenaPropKind Kind;
            public GameObject Root;
            public readonly List<Renderer> Primary = new List<Renderer>();
            public readonly List<Renderer> Secondary = new List<Renderer>();
        }

        private readonly Transform _parent;
        private readonly bool _colliders;
        private readonly bool _castShadows;
        private readonly Dictionary<BiomeArenaPropKind, Stack<Prop>> _pool = new Dictionary<BiomeArenaPropKind, Stack<Prop>>();
        private readonly List<Mesh> _rockMeshes = new List<Mesh>();
        private readonly MaterialPropertyBlock _block = new MaterialPropertyBlock();
        private int _built;

        public BiomeArenaPropFactory(Transform parent, bool colliders, bool castShadows)
        {
            _parent = parent;
            _colliders = colliders;
            _castShadows = castShadows;
        }

        /// <summary>Total props ever built (pool growth; stays flat across repeated applies).</summary>
        public int BuiltCount => _built;

        /// <summary>Pooled (or newly built) prop of <paramref name="kind"/>, coloured and active.</summary>
        public Prop Acquire(BiomeArenaPropKind kind, Color primary, Color secondary)
        {
            Prop prop = null;
            if (_pool.TryGetValue(kind, out Stack<Prop> stack))
            {
                while (stack.Count > 0 && (prop == null || prop.Root == null))
                {
                    prop = stack.Pop();
                }
            }

            if (prop == null || prop.Root == null)
            {
                prop = Build(kind);
            }

            Paint(prop.Primary, primary);
            Paint(prop.Secondary, secondary);
            prop.Root.SetActive(true);
            return prop;
        }

        public void Release(Prop prop)
        {
            if (prop == null || prop.Root == null)
            {
                return;
            }

            prop.Root.SetActive(false);
            if (!_pool.TryGetValue(prop.Kind, out Stack<Prop> stack))
            {
                stack = new Stack<Prop>();
                _pool[prop.Kind] = stack;
            }

            stack.Push(prop);
        }

        /// <summary>Destroys the generated rock meshes (the prop GameObjects die with their parent).</summary>
        public void Dispose()
        {
            foreach (Mesh mesh in _rockMeshes)
            {
                if (mesh != null)
                {
                    BiomeArenaPropFactory.SafeDestroy(mesh);
                }
            }

            _rockMeshes.Clear();
            _pool.Clear();
        }

        /// <summary>Destroy in Play Mode, DestroyImmediate in Edit Mode (EditMode tests).</summary>
        internal static void SafeDestroy(Object target)
        {
            if (target == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Object.Destroy(target);
            }
            else
            {
                Object.DestroyImmediate(target);
            }
        }

        private Prop Build(BiomeArenaPropKind kind)
        {
            var prop = new Prop { Kind = kind, Root = new GameObject($"ArenaProp_{kind}_{_built}") };
            prop.Root.transform.SetParent(_parent, false);
            _built++;

            switch (kind)
            {
                case BiomeArenaPropKind.Cactus:
                    // The saxaul of the Flaming Sands (kind id kept for saved profiles): a short gnarled pale trunk
                    // (secondary colour) forking into crooked branches, with sparse flat grey-green crowns (primary).
                    Part(prop, PrimitiveType.Cylinder, new Vector3(0f, 0.6f, 0f), new Vector3(0f, 0f, 6f), new Vector3(0.3f, 0.6f, 0.3f), false, main: true);
                    Part(prop, PrimitiveType.Sphere, new Vector3(0.05f, 0.1f, 0f), Vector3.zero, new Vector3(0.6f, 0.3f, 0.55f), false);
                    Part(prop, PrimitiveType.Cylinder, new Vector3(0.38f, 1.5f, 0f), new Vector3(0f, 0f, -32f), new Vector3(0.17f, 0.5f, 0.17f), false);
                    Part(prop, PrimitiveType.Cylinder, new Vector3(-0.32f, 1.45f, 0.12f), new Vector3(18f, 0f, 30f), new Vector3(0.15f, 0.45f, 0.15f), false);
                    Part(prop, PrimitiveType.Cylinder, new Vector3(0.02f, 1.6f, -0.2f), new Vector3(-24f, 0f, 4f), new Vector3(0.12f, 0.42f, 0.12f), false);
                    Part(prop, PrimitiveType.Sphere, new Vector3(0.72f, 2.0f, 0f), Vector3.zero, new Vector3(1.15f, 0.5f, 0.95f), true);
                    Part(prop, PrimitiveType.Sphere, new Vector3(-0.62f, 1.9f, 0.28f), new Vector3(0f, 30f, 0f), new Vector3(1.0f, 0.45f, 0.85f), true);
                    Part(prop, PrimitiveType.Sphere, new Vector3(0.05f, 2.2f, -0.45f), new Vector3(0f, 60f, 0f), new Vector3(0.9f, 0.45f, 0.8f), true);
                    Part(prop, PrimitiveType.Sphere, new Vector3(0.1f, 2.45f, 0.1f), Vector3.zero, new Vector3(0.6f, 0.32f, 0.6f), true);
                    break;

                case BiomeArenaPropKind.Stalagmite:
                    Part(prop, PrimitiveType.Cylinder, new Vector3(0f, 0.3f, 0f), Vector3.zero, new Vector3(1.1f, 0.3f, 1.1f), true, main: true);
                    Part(prop, PrimitiveType.Cylinder, new Vector3(0f, 0.9f, 0f), Vector3.zero, new Vector3(0.7f, 0.35f, 0.7f), true);
                    Part(prop, PrimitiveType.Cylinder, new Vector3(0f, 1.55f, 0f), Vector3.zero, new Vector3(0.42f, 0.32f, 0.42f), true);
                    Part(prop, PrimitiveType.Capsule, new Vector3(0f, 2.15f, 0f), Vector3.zero, new Vector3(0.2f, 0.4f, 0.2f), false);
                    break;

                case BiomeArenaPropKind.SnowyRock:
                    RockPart(prop, new Vector3(0f, 0.35f, 0f), new Vector3(1.3f, 1f, 1.1f), true, main: true);
                    Part(prop, PrimitiveType.Sphere, new Vector3(0f, 0.95f, 0f), Vector3.zero, new Vector3(2f, 0.5f, 1.7f), false);
                    break;

                default:
                    RockPart(prop, new Vector3(0f, 0.3f, 0f), Vector3.one, true, main: true);
                    RockPart(prop, new Vector3(0.9f, 0.12f, 0.4f), new Vector3(0.35f, 0.35f, 0.35f), false);
                    break;
            }

            return prop;
        }

        private GameObject Part(Prop prop, PrimitiveType type, Vector3 position, Vector3 euler, Vector3 scale, bool primary, bool main = false)
        {
            GameObject part = GameObject.CreatePrimitive(type);
            part.name = type.ToString();
            Transform t = part.transform;
            t.SetParent(prop.Root.transform, false);
            t.localPosition = position;
            t.localRotation = Quaternion.Euler(euler);
            t.localScale = scale;

            if (!(main && _colliders))
            {
                Collider collider = part.GetComponent<Collider>();
                if (collider != null)
                {
                    BiomeArenaPropFactory.SafeDestroy(collider);
                }
            }

            Renderer renderer = part.GetComponent<Renderer>();
            renderer.shadowCastingMode = _castShadows && main ? ShadowCastingMode.On : ShadowCastingMode.Off;
            renderer.lightProbeUsage = LightProbeUsage.Off;
            renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
            (primary ? prop.Primary : prop.Secondary).Add(renderer);
            return part;
        }

        private void RockPart(Prop prop, Vector3 position, Vector3 scale, bool primary, bool main = false)
        {
            // A sphere primitive gives the pipeline's default material and a cheap SphereCollider;
            // only its mesh is swapped for a procedural rock.
            GameObject part = Part(prop, PrimitiveType.Sphere, position, new Vector3(0f, _built * 47f, 0f), scale, primary, main);
            part.name = "Rock";
            part.GetComponent<MeshFilter>().sharedMesh = GetRockMesh(_built + prop.Primary.Count + prop.Secondary.Count);
            if (part.TryGetComponent(out SphereCollider sphere))
            {
                sphere.radius = 0.8f; // rock mesh is ~unit radius vs the primitive's 0.5
            }
        }

        private Mesh GetRockMesh(int index)
        {
            if (_rockMeshes.Count < RockMeshVariants)
            {
                Mesh mesh = ProceduralRockGenerator.BuildRockMesh(9001 + _rockMeshes.Count * 131, 2, 0.3f, 0.7f);
                mesh.name = $"ArenaRock_{_rockMeshes.Count}";
                _rockMeshes.Add(mesh);
                return mesh;
            }

            return _rockMeshes[Mathf.Abs(index) % _rockMeshes.Count];
        }

        private void Paint(List<Renderer> renderers, Color color)
        {
            for (int i = 0; i < renderers.Count; i++)
            {
                Renderer renderer = renderers[i];
                if (renderer == null)
                {
                    continue;
                }

                renderer.GetPropertyBlock(_block);
                _block.SetColor(BaseColorId, color);
                _block.SetColor(ColorId, color);
                renderer.SetPropertyBlock(_block);
            }
        }
    }
}
