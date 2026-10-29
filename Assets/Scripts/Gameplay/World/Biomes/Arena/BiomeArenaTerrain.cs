using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace Rapadura.Gameplay.World.Biomes
{
    /// <summary>
    /// Scene side of the procedural arena terrain, driven by <see cref="BiomeArenaDressing"/>: one
    /// unparented "[BiomeArenaTerrain]" root (in the arena's scene, so it unloads with it) holding
    /// <list type="bullet">
    /// <item>"ArenaGround" — MeshFilter + MeshRenderer + MeshCollider on the Default layer (what
    /// PlayerMotor.SnapToGround, the encounter spawn snap, PlayerFootIK and click-to-move raycast);</item>
    /// <item>"ArenaBackdrop" — MeshFilter + MeshRenderer, no collider.</item>
    /// </list>
    /// Meshes come from <see cref="BiomeArenaTerrainCache"/>; the GameObjects are reused across
    /// <see cref="Show"/> calls (switching biome just swaps meshes) and <see cref="Hide"/> deactivates
    /// them. Both renderers share one vertex-colour material (2 draw calls, no shadow casting).
    /// Plain C# (not a MonoBehaviour) so EditMode tests can drive it.
    /// </summary>
    public sealed class BiomeArenaTerrain
    {
        public const string RootName = "[BiomeArenaTerrain]";
        public const string MaterialResourcePath = "Biomes/ArenaTerrain/Mat_ArenaTerrain";
        private const string VertexColorShader = "Universal Render Pipeline/Particles/Simple Lit";

        private static Material _fallbackMaterial;

        private readonly Scene _scene;
        private GameObject _root;
        private MeshFilter _groundFilter;
        private MeshRenderer _groundRenderer;
        private MeshCollider _groundCollider;
        private MeshFilter _backdropFilter;
        private MeshRenderer _backdropRenderer;

        public BiomeArenaTerrain(Scene scene)
        {
            _scene = scene;
        }

        public bool IsShown => _root != null && _root.activeSelf;
        public BiomeArenaTerrainCache.Entry Current { get; private set; }
        public Transform Root => _root != null ? _root.transform : null;
        public MeshCollider GroundCollider => _groundCollider;
        public MeshRenderer GroundRenderer => _groundRenderer;
        public MeshRenderer BackdropRenderer => _backdropRenderer;

        /// <summary>Shows the terrain of <paramref name="biome"/> / <paramref name="variant"/> with its centre at <paramref name="origin"/>.</summary>
        public void Show(BiomeType biome, int variant, Vector3 origin)
        {
            EnsureObjects();
            BiomeArenaTerrainCache.Entry entry = BiomeArenaTerrainCache.Get(biome, variant);
            Current = entry;

            _root.transform.SetPositionAndRotation(origin, Quaternion.identity);
            _groundFilter.sharedMesh = entry.Ground;
            _backdropFilter.sharedMesh = entry.Backdrop;
            if (_groundCollider.sharedMesh != entry.Ground)
            {
                _groundCollider.sharedMesh = entry.Ground;
            }

            Material material = SharedMaterial();
            _groundRenderer.sharedMaterial = material;
            _backdropRenderer.sharedMaterial = material;
            _root.SetActive(true);
            Physics.SyncTransforms();
        }

        public void Hide()
        {
            if (_root != null)
            {
                _root.SetActive(false);
            }

            Current = null;
        }

        public void Dispose()
        {
            if (_root != null)
            {
                BiomeArenaPropFactory.SafeDestroy(_root);
            }

            _root = null;
            Current = null;
        }

        /// <summary>
        /// The vertex-colour material: Resources/<see cref="MaterialResourcePath"/> (a URP Particles/Simple
        /// Lit material — it multiplies vertex colours, and the Resources reference keeps the shader in
        /// builds), else a runtime material on the same shader, else URP Lit.
        /// </summary>
        public static Material SharedMaterial()
        {
            Material asset = Resources.Load<Material>(MaterialResourcePath);
            if (asset != null)
            {
                return asset;
            }

            if (_fallbackMaterial == null)
            {
                Shader shader = Shader.Find(VertexColorShader) ?? Shader.Find("Universal Render Pipeline/Lit");
                _fallbackMaterial = new Material(shader) { name = "ArenaTerrain (runtime)" };
            }

            return _fallbackMaterial;
        }

        private void EnsureObjects()
        {
            if (_root != null)
            {
                return;
            }

            _root = new GameObject(RootName);
            if (_scene.IsValid() && _root.scene != _scene)
            {
                SceneManager.MoveGameObjectToScene(_root, _scene);
            }

            var ground = new GameObject("ArenaGround");
            ground.transform.SetParent(_root.transform, false);
            _groundFilter = ground.AddComponent<MeshFilter>();
            _groundRenderer = ground.AddComponent<MeshRenderer>();
            _groundCollider = ground.AddComponent<MeshCollider>();
            Configure(_groundRenderer, receiveShadows: true);

            var backdrop = new GameObject("ArenaBackdrop");
            backdrop.transform.SetParent(_root.transform, false);
            _backdropFilter = backdrop.AddComponent<MeshFilter>();
            _backdropRenderer = backdrop.AddComponent<MeshRenderer>();
            Configure(_backdropRenderer, receiveShadows: true);
        }

        private static void Configure(MeshRenderer renderer, bool receiveShadows)
        {
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = receiveShadows;
            renderer.lightProbeUsage = LightProbeUsage.Off;
            renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
            renderer.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
        }
    }
}
