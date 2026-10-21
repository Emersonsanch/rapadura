using System.Collections.Generic;
using Rapadura.Core.DI;
using Rapadura.Core.Logging;
using Rapadura.Gameplay.Environment;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace Rapadura.Gameplay.World.Biomes
{
    /// <summary>
    /// Re-dresses the shared combat arena (Main.unity, authored as a forest greybox) for the biome an
    /// encounter was rolled in, entirely at runtime and only while an encounter is active
    /// (<see cref="Rapadura.Gameplay.World.Map.EncounterArenaController"/> calls <see cref="Apply(BiomeType, int)"/>
    /// when it begins an arena; the sandbox Main without a payload is never touched):
    /// <list type="bullet">
    /// <item>authored props hidden/recoloured by name-prefix rules (trees hidden in Desert/Caves, rocks recoloured);</item>
    /// <item>ground plane recoloured (optionally texture swapped) through a MaterialPropertyBlock;</item>
    /// <item>fog/ambient/sun baseline via <see cref="EnvironmentOverrides"/> (honoured by WeatherVisuals and DayNightLighting), plus RenderSettings directly when those presenters are absent;</item>
    /// <item>optional procedural-skybox tint on a runtime copy of the skybox material;</item>
    /// <item>a few pooled primitive props (<see cref="BiomeArenaPropFactory"/>) scattered on a ring around the arena;</item>
    /// <item>WeatherSystem/AmbientVfx switched to the encounter biome.</item>
    /// </list>
    /// No material asset is modified. <see cref="Restore"/> undoes everything; when the scene unloads
    /// (leaving the encounter) only the process-wide state needs undoing, which OnDisable does.
    /// </summary>
    public class BiomeArenaDressing : MonoBehaviour
    {
        private const string LogCategory = "BiomeArena";
        private const string PlayerTag = "Player";
        private const string MainCameraTag = "MainCamera";

        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorId = Shader.PropertyToID("_Color");
        private static readonly int BaseMapId = Shader.PropertyToID("_BaseMap");
        private static readonly int MainTexId = Shader.PropertyToID("_MainTex");
        private static readonly int SkyTintId = Shader.PropertyToID("_SkyTint");
        private static readonly int SkyGroundColorId = Shader.PropertyToID("_GroundColor");
        private static readonly int SkyCubeTintId = Shader.PropertyToID("_Tint");
        private static readonly int SkyExposureId = Shader.PropertyToID("_Exposure");

        [Tooltip("Arena centre the props ring around. Empty = the position passed by the caller (the EncounterArenaController), else this transform.")]
        [SerializeField] private Transform _arenaCenter;
        [Tooltip("Ground renderers to recolour. Empty = scene objects named in Ground Object Names.")]
        [SerializeField] private Renderer[] _groundRenderers = new Renderer[0];
        [SerializeField] private string[] _groundObjectNames = { "Ground" };
        [Tooltip("Optional explicit profiles (take precedence). Empty = Resources/Biomes/Arena, then built-in defaults.")]
        [SerializeField] private BiomeArenaProfile[] _profileOverrides = new BiomeArenaProfile[0];

        [Header("Procedural props")]
        [SerializeField] private bool _propColliders = true;
        [SerializeField] private bool _propShadows = true;
        [Tooltip("Height above the arena centre the ground probe ray starts from.")]
        [SerializeField] [Min(1f)] private float _groundProbeHeight = 40f;

        private readonly struct SavedBlock
        {
            public readonly Renderer Renderer;
            public readonly MaterialPropertyBlock Block;
            public readonly int MaterialIndex; // -1 = renderer-wide block

            public SavedBlock(Renderer renderer, MaterialPropertyBlock block, int materialIndex)
            {
                Renderer = renderer;
                Block = block;
                MaterialIndex = materialIndex;
            }
        }

        private readonly List<GameObject> _hidden = new List<GameObject>();
        private readonly List<SavedBlock> _savedBlocks = new List<SavedBlock>();
        private readonly HashSet<Renderer> _touchedRenderers = new HashSet<Renderer>();
        private readonly List<Renderer> _rendererBuffer = new List<Renderer>();
        private readonly List<Renderer> _groundBuffer = new List<Renderer>();
        private readonly List<Material> _materialBuffer = new List<Material>();
        private readonly List<BiomeArenaPropFactory.Prop> _activeProps = new List<BiomeArenaPropFactory.Prop>();
        private readonly List<Vector2> _scatter = new List<Vector2>();
        private readonly List<GameObject> _roots = new List<GameObject>();

        private BiomeArenaPropFactory _factory;
        private Transform _propRoot;

        private bool _applied;
        private BiomeType _appliedBiome;
        private BiomeArenaProfile _appliedProfile;

        private bool _renderSettingsCaptured;
        private bool _savedFog;
        private FogMode _savedFogMode;
        private float _savedFogDensity;
        private Color _savedFogColor;
        private AmbientMode _savedAmbientMode;
        private Color _savedAmbientSky;
        private Color _savedAmbientEquator;
        private Color _savedAmbientGround;

        private Material _originalSkybox;
        private Material _skyboxCopy;

        private bool _weatherBiomeChanged;
        private BiomeType _previousWeatherBiome;
        private bool _ambientVfxBiomeChanged;
        private BiomeType _previousAmbientVfxBiome;

        public bool IsApplied => _applied;
        public BiomeType AppliedBiome => _appliedBiome;
        public BiomeArenaProfile AppliedProfile => _appliedProfile;
        public int HiddenCount => _hidden.Count;
        public int TintedRendererCount => _savedBlocks.Count;
        public int ActivePropCount => _activeProps.Count;

        /// <summary>Profile used for <paramref name="biome"/>: inspector override, else <see cref="BiomeArenaProfiles.Get"/>.</summary>
        public BiomeArenaProfile ResolveProfile(BiomeType biome)
        {
            BiomeArenaProfile explicitProfile = BiomeArenaRules.SelectProfile(biome, _profileOverrides);
            return explicitProfile != null ? explicitProfile : BiomeArenaProfiles.Get(biome);
        }

        public void Apply(BiomeType biome, int seed) => Apply(ResolveProfile(biome), seed, null);

        public void Apply(BiomeType biome, int seed, Vector3 fallbackCenter) => Apply(ResolveProfile(biome), seed, fallbackCenter);

        /// <summary>Dresses the arena with <paramref name="profile"/> (restoring any previous dressing first).</summary>
        public void Apply(BiomeArenaProfile profile, int seed, Vector3? fallbackCenter = null)
        {
            if (profile == null)
            {
                return;
            }

            if (_applied)
            {
                Restore();
            }

            _applied = true;
            _appliedBiome = profile.Biome;
            _appliedProfile = profile;

            SyncEnvironmentBiome(profile.Biome);

            if (profile.IsNeutral)
            {
                GameLogger.Info(LogCategory, $"Arena kept as authored for {profile.Biome}.");
                return;
            }

            ApplyPropRules(profile);
            ApplyGround(profile);
            ApplyAtmosphere(profile);
            ApplySkybox(profile);
            SpawnProps(profile, seed, ResolveCenter(fallbackCenter));

            GameLogger.Info(
                LogCategory,
                $"Arena dressed as {profile.Biome}: {_hidden.Count} hidden, {_savedBlocks.Count} renderers recoloured, {_activeProps.Count} props.");
        }

        /// <summary>Undoes everything <see cref="Apply(BiomeArenaProfile, int, Vector3?)"/> did. Safe to call repeatedly.</summary>
        public void Restore()
        {
            if (!_applied)
            {
                return;
            }

            RestoreSceneObjects();
            RestoreGlobalState();
            _applied = false;
            _appliedProfile = null;
        }

        private void OnDisable()
        {
            // On scene unload the arena's objects die with it; only process-wide state (static
            // overrides, WeatherSystem biome, the skybox copy) must be undone. Restoring scene
            // objects here would SetActive objects mid-unload.
            if (_applied)
            {
                RestoreGlobalState();
                _applied = false;
                _appliedProfile = null;
            }
        }

        private void OnDestroy()
        {
            _factory?.Dispose();
            _factory = null;
            if (_propRoot != null)
            {
                BiomeArenaPropFactory.SafeDestroy(_propRoot.gameObject);
                _propRoot = null;
            }
        }

        // ---------------------------------------------------------------
        // Biome sync
        // ---------------------------------------------------------------

        private void SyncEnvironmentBiome(BiomeType biome)
        {
            if (ServiceLocator.TryGet(out WeatherSystem weather) && weather != null && weather.CurrentBiome != biome)
            {
                _weatherBiomeChanged = true;
                _previousWeatherBiome = weather.CurrentBiome;
                weather.SetBiome(biome);
            }

            AmbientVfx ambient = FindAnyObjectByType<AmbientVfx>();
            if (ambient != null && ambient.Biome != biome)
            {
                _ambientVfxBiomeChanged = true;
                _previousAmbientVfxBiome = ambient.Biome;
                ambient.SetBiome(biome);
            }
        }

        // ---------------------------------------------------------------
        // Authored props
        // ---------------------------------------------------------------

        private void ApplyPropRules(BiomeArenaProfile profile)
        {
            IReadOnlyList<BiomeArenaPropRule> rules = profile.PropRules;
            if (rules == null || rules.Count == 0)
            {
                return;
            }

            bool readTag = BiomeArenaRules.AnyRuleUsesTag(rules);
            gameObject.scene.GetRootGameObjects(_roots);
            for (int i = 0; i < _roots.Count; i++)
            {
                Visit(_roots[i].transform, rules, readTag);
            }

            _roots.Clear();
        }

        private void Visit(Transform node, IReadOnlyList<BiomeArenaPropRule> rules, bool readTag)
        {
            if (node == _propRoot)
            {
                return;
            }

            GameObject go = node.gameObject;
            if (go.CompareTag(PlayerTag) || go.CompareTag(MainCameraTag))
            {
                return;
            }

            string objectTag = readTag ? go.tag : null;
            BiomeArenaPropAction action = BiomeArenaRules.ResolveAction(go.name, objectTag, rules, out Color tint);
            switch (action)
            {
                case BiomeArenaPropAction.Hide:
                    if (go.activeSelf)
                    {
                        go.SetActive(false);
                        _hidden.Add(go);
                    }

                    return;

                case BiomeArenaPropAction.Tint:
                    go.GetComponentsInChildren(true, _rendererBuffer);
                    for (int i = 0; i < _rendererBuffer.Count; i++)
                    {
                        TintRenderer(_rendererBuffer[i], tint, null);
                    }

                    _rendererBuffer.Clear();
                    return;
            }

            // Keep (explicit or no match): look further down.
            for (int i = 0; i < node.childCount; i++)
            {
                Visit(node.GetChild(i), rules, readTag);
            }
        }

        private void TintRenderer(Renderer renderer, Color tint, Texture replaceTexture, bool replaceWithFlat = false)
        {
            if (renderer == null || renderer is ParticleSystemRenderer || !_touchedRenderers.Add(renderer))
            {
                return;
            }

            // Single-material renderers use the renderer-wide block (which also carries
            // TreeColorVariation's per-instance tint, so that variation survives a partial tint);
            // multi-material ones get one block per slot so each submesh keeps its own hue.
            renderer.GetSharedMaterials(_materialBuffer);
            if (_materialBuffer.Count <= 1)
            {
                TintSlot(renderer, -1, _materialBuffer.Count == 1 ? _materialBuffer[0] : null, tint, replaceTexture, replaceWithFlat);
            }
            else
            {
                for (int i = 0; i < _materialBuffer.Count; i++)
                {
                    TintSlot(renderer, i, _materialBuffer[i], tint, replaceTexture, replaceWithFlat);
                }
            }

            _materialBuffer.Clear();
        }

        private void TintSlot(Renderer renderer, int materialIndex, Material material, Color tint, Texture replaceTexture, bool replaceWithFlat)
        {
            var saved = new MaterialPropertyBlock();
            GetBlock(renderer, saved, materialIndex);
            _savedBlocks.Add(new SavedBlock(renderer, saved, materialIndex));

            Color baseColor = saved.HasColor(BaseColorId) ? saved.GetColor(BaseColorId) : SharedColor(material);
            Color tinted = BiomeArenaRules.ApplyTint(baseColor, tint);

            var block = new MaterialPropertyBlock();
            GetBlock(renderer, block, materialIndex);
            block.SetColor(BaseColorId, tinted);
            block.SetColor(ColorId, tinted);
            if (replaceWithFlat || replaceTexture != null)
            {
                Texture texture = replaceTexture != null ? replaceTexture : Texture2D.whiteTexture;
                block.SetTexture(BaseMapId, texture);
                block.SetTexture(MainTexId, texture);
            }

            SetBlock(renderer, block, materialIndex);
        }

        private static void GetBlock(Renderer renderer, MaterialPropertyBlock block, int materialIndex)
        {
            if (materialIndex < 0)
            {
                renderer.GetPropertyBlock(block);
            }
            else
            {
                renderer.GetPropertyBlock(block, materialIndex);
            }
        }

        private static void SetBlock(Renderer renderer, MaterialPropertyBlock block, int materialIndex)
        {
            if (materialIndex < 0)
            {
                renderer.SetPropertyBlock(block);
            }
            else
            {
                renderer.SetPropertyBlock(block, materialIndex);
            }
        }

        private static Color SharedColor(Material material)
        {
            if (material == null)
            {
                return Color.white;
            }

            if (material.HasProperty(BaseColorId))
            {
                return material.GetColor(BaseColorId);
            }

            return material.HasProperty(ColorId) ? material.GetColor(ColorId) : Color.white;
        }

        // ---------------------------------------------------------------
        // Ground
        // ---------------------------------------------------------------

        private void ApplyGround(BiomeArenaProfile profile)
        {
            if (!profile.TintGround)
            {
                return;
            }

            CollectGroundRenderers();
            for (int i = 0; i < _groundBuffer.Count; i++)
            {
                TintRenderer(_groundBuffer[i], profile.GroundTint, profile.GroundTexture, profile.ReplaceGroundTexture);
            }
        }

        private void CollectGroundRenderers()
        {
            _groundBuffer.Clear();
            if (_groundRenderers != null)
            {
                foreach (Renderer renderer in _groundRenderers)
                {
                    if (renderer != null)
                    {
                        _groundBuffer.Add(renderer);
                    }
                }
            }

            if (_groundBuffer.Count > 0 || _groundObjectNames == null || _groundObjectNames.Length == 0)
            {
                return;
            }

            gameObject.scene.GetRootGameObjects(_roots);
            foreach (GameObject root in _roots)
            {
                foreach (string groundName in _groundObjectNames)
                {
                    if (!string.IsNullOrEmpty(groundName) && root.name == groundName && root.TryGetComponent(out Renderer renderer))
                    {
                        _groundBuffer.Add(renderer);
                    }
                }
            }

            _roots.Clear();
        }

        // ---------------------------------------------------------------
        // Atmosphere / sky
        // ---------------------------------------------------------------

        private void ApplyAtmosphere(BiomeArenaProfile profile)
        {
            CaptureRenderSettings();

            if (profile.OverrideFog)
            {
                EnvironmentOverrides.SetFogBaseline(profile.FogColor, profile.FogDensity);
                // Direct write for scenes without WeatherVisuals; WeatherVisuals re-derives the same baseline.
                RenderSettings.fog = profile.FogDensity > 0f;
                RenderSettings.fogMode = FogMode.Exponential;
                RenderSettings.fogDensity = profile.FogDensity;
                RenderSettings.fogColor = profile.FogColor;
            }

            Color ambientTint = profile.AmbientTint;
            EnvironmentOverrides.SetLighting(ambientTint, profile.SunIntensityMultiplier);
            if (ambientTint != Color.white)
            {
                // Direct write for scenes without DayNightLighting (it recomputes from its gradient,
                // already multiplied by the override, so this never compounds).
                if (RenderSettings.ambientMode == AmbientMode.Trilight)
                {
                    RenderSettings.ambientSkyColor = _savedAmbientSky * ambientTint;
                    RenderSettings.ambientEquatorColor = _savedAmbientEquator * ambientTint;
                    RenderSettings.ambientGroundColor = _savedAmbientGround * ambientTint;
                }
                else
                {
                    RenderSettings.ambientLight = _savedAmbientSky * ambientTint;
                }
            }
        }

        private void ApplySkybox(BiomeArenaProfile profile)
        {
            Material skybox = RenderSettings.skybox;
            if (!profile.TintSkybox || skybox == null)
            {
                return;
            }

            // Never touch the shared skybox material (in the Editor that would dirty the asset).
            _originalSkybox = skybox;
            _skyboxCopy = new Material(skybox) { name = $"{skybox.name} ({profile.Biome} arena)" };
            if (_skyboxCopy.HasProperty(SkyTintId))
            {
                _skyboxCopy.SetColor(SkyTintId, profile.SkyTint);
            }
            else if (_skyboxCopy.HasProperty(SkyCubeTintId))
            {
                _skyboxCopy.SetColor(SkyCubeTintId, profile.SkyTint);
            }

            if (_skyboxCopy.HasProperty(SkyGroundColorId))
            {
                _skyboxCopy.SetColor(SkyGroundColorId, profile.SkyGroundColor);
            }

            if (_skyboxCopy.HasProperty(SkyExposureId))
            {
                _skyboxCopy.SetFloat(SkyExposureId, profile.SkyExposure);
            }

            RenderSettings.skybox = _skyboxCopy;
        }

        private void CaptureRenderSettings()
        {
            _renderSettingsCaptured = true;
            _savedFog = RenderSettings.fog;
            _savedFogMode = RenderSettings.fogMode;
            _savedFogDensity = RenderSettings.fogDensity;
            _savedFogColor = RenderSettings.fogColor;
            _savedAmbientMode = RenderSettings.ambientMode;
            _savedAmbientSky = RenderSettings.ambientMode == AmbientMode.Trilight ? RenderSettings.ambientSkyColor : RenderSettings.ambientLight;
            _savedAmbientEquator = RenderSettings.ambientEquatorColor;
            _savedAmbientGround = RenderSettings.ambientGroundColor;
        }

        // ---------------------------------------------------------------
        // Procedural props
        // ---------------------------------------------------------------

        private Vector3 ResolveCenter(Vector3? fallbackCenter)
        {
            if (_arenaCenter != null)
            {
                return _arenaCenter.position;
            }

            return fallbackCenter ?? transform.position;
        }

        private void SpawnProps(BiomeArenaProfile profile, int seed, Vector3 center)
        {
            IReadOnlyList<BiomeArenaPropSpawn> spawns = profile.Props;
            if (spawns == null || spawns.Count == 0)
            {
                return;
            }

            if (_propRoot == null)
            {
                // Unparented (scale-neutral) root in this scene, so it unloads with the arena.
                var root = new GameObject("[BiomeArenaProps]");
                if (root.scene != gameObject.scene && gameObject.scene.IsValid())
                {
                    SceneManager.MoveGameObjectToScene(root, gameObject.scene);
                }

                _propRoot = root.transform;
            }

            _factory ??= new BiomeArenaPropFactory(_propRoot, _propColliders, _propShadows);
            if (_groundBuffer.Count == 0)
            {
                CollectGroundRenderers();
            }

            for (int entry = 0; entry < spawns.Count; entry++)
            {
                BiomeArenaPropSpawn spawn = spawns[entry];
                if (spawn == null || spawn.Count <= 0)
                {
                    continue;
                }

                int entrySeed = BiomeArenaRules.SeedForEntry(seed, entry);
                _scatter.Clear();
                BiomeArenaRules.ScatterOnRing(entrySeed, spawn.Count, profile.ScatterInnerRadius, profile.ScatterOuterRadius, _scatter);
                var random = new System.Random(entrySeed ^ 0x5bd1e995);

                for (int i = 0; i < _scatter.Count; i++)
                {
                    BiomeArenaPropFactory.Prop prop = _factory.Acquire(spawn.Kind, spawn.PrimaryColor, spawn.SecondaryColor);
                    Vector3 position = center + new Vector3(_scatter[i].x, 0f, _scatter[i].y);
                    position.y = ProbeGroundHeight(position, center.y);

                    float scale = Mathf.Lerp(spawn.MinScale, spawn.MaxScale, (float)random.NextDouble());
                    float yaw = (float)random.NextDouble() * 360f;
                    float tilt = spawn.Kind == BiomeArenaPropKind.Stalagmite ? ((float)random.NextDouble() * 2f - 1f) * 6f : 0f;

                    Transform t = prop.Root.transform;
                    t.SetPositionAndRotation(position, Quaternion.Euler(tilt, yaw, 0f));
                    t.localScale = Vector3.one * scale;
                    _activeProps.Add(prop);
                }
            }
        }

        private float ProbeGroundHeight(Vector3 position, float fallbackY)
        {
            var ray = new Ray(new Vector3(position.x, fallbackY + _groundProbeHeight, position.z), Vector3.down);
            float maxDistance = _groundProbeHeight * 2f;
            float best = float.NegativeInfinity;

            // Probe only the ground's own colliders so rocks/enemies/the player never lift a prop.
            for (int i = 0; i < _groundBuffer.Count; i++)
            {
                Renderer ground = _groundBuffer[i];
                if (ground != null && ground.TryGetComponent(out Collider collider) && collider.enabled
                    && collider.Raycast(ray, out RaycastHit hit, maxDistance))
                {
                    best = Mathf.Max(best, hit.point.y);
                }
            }

            return float.IsNegativeInfinity(best) ? fallbackY : best;
        }

        // ---------------------------------------------------------------
        // Restore
        // ---------------------------------------------------------------

        private void RestoreSceneObjects()
        {
            for (int i = _hidden.Count - 1; i >= 0; i--)
            {
                if (_hidden[i] != null)
                {
                    _hidden[i].SetActive(true);
                }
            }

            _hidden.Clear();

            for (int i = 0; i < _savedBlocks.Count; i++)
            {
                SavedBlock saved = _savedBlocks[i];
                if (saved.Renderer != null)
                {
                    SetBlock(saved.Renderer, saved.Block.isEmpty ? null : saved.Block, saved.MaterialIndex);
                }
            }

            _savedBlocks.Clear();
            _touchedRenderers.Clear();
            _groundBuffer.Clear();

            if (_factory != null)
            {
                for (int i = 0; i < _activeProps.Count; i++)
                {
                    _factory.Release(_activeProps[i]);
                }
            }

            _activeProps.Clear();
        }

        private void RestoreGlobalState()
        {
            EnvironmentOverrides.Reset();

            // RenderSettings belong to the active scene; never write the next scene's settings while
            // this one is unloading.
            bool ownsRenderSettings = SceneManager.GetActiveScene() == gameObject.scene;

            if (_skyboxCopy != null)
            {
                if (ownsRenderSettings && RenderSettings.skybox == _skyboxCopy)
                {
                    RenderSettings.skybox = _originalSkybox;
                }

                BiomeArenaPropFactory.SafeDestroy(_skyboxCopy);
                _skyboxCopy = null;
                _originalSkybox = null;
            }

            if (_renderSettingsCaptured)
            {
                _renderSettingsCaptured = false;
                if (ownsRenderSettings)
                {
                    RenderSettings.fog = _savedFog;
                    RenderSettings.fogMode = _savedFogMode;
                    RenderSettings.fogDensity = _savedFogDensity;
                    RenderSettings.fogColor = _savedFogColor;
                    RenderSettings.ambientMode = _savedAmbientMode;
                    if (_savedAmbientMode == AmbientMode.Trilight)
                    {
                        RenderSettings.ambientSkyColor = _savedAmbientSky;
                        RenderSettings.ambientEquatorColor = _savedAmbientEquator;
                        RenderSettings.ambientGroundColor = _savedAmbientGround;
                    }
                    else
                    {
                        RenderSettings.ambientLight = _savedAmbientSky;
                    }
                }
            }

            if (_weatherBiomeChanged)
            {
                _weatherBiomeChanged = false;
                // Only undo our own change (the map may have moved the player's region meanwhile).
                if (ServiceLocator.TryGet(out WeatherSystem weather) && weather != null && weather.CurrentBiome == _appliedBiome)
                {
                    weather.SetBiome(_previousWeatherBiome);
                }
            }

            if (_ambientVfxBiomeChanged)
            {
                _ambientVfxBiomeChanged = false;
                AmbientVfx ambient = FindAnyObjectByType<AmbientVfx>();
                if (ambient != null && ambient.isActiveAndEnabled && ambient.Biome == _appliedBiome)
                {
                    ambient.SetBiome(_previousAmbientVfxBiome);
                }
            }
        }
    }
}
