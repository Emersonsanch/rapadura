using System.Collections.Generic;
using Rapadura.Core.Accessibility;
using Rapadura.Core.DI;
using Rapadura.Core.Events;
using Rapadura.Core.Performance;
using Rapadura.Gameplay.Skills.Vfx;
using Rapadura.Gameplay.World.Biomes;
using Rapadura.Gameplay.World.Map;
using UnityEngine;
using UnityEngine.Rendering;

namespace Rapadura.Gameplay.Environment
{
    /// <summary>
    /// Scene-side ambient particles (fireflies, falling leaves, dust motes, heat shimmer, snow
    /// flurries, wind wisps, cave drips) chosen per biome / time-of-day phase / weather from an
    /// <see cref="AmbientVfxLibrary"/> (<c>Resources/Vfx/AmbientVfxLibrary</c>, built-in defaults as
    /// fallback). Auto-added by <see cref="EnvironmentPresentationBootstrap"/> next to
    /// <see cref="WeatherVisuals"/>.
    ///
    /// Event-driven: targets only change on <see cref="TimeOfDayPhaseChangedEvent"/>,
    /// <see cref="WeatherChangedEvent"/>, <see cref="WorldMapRegionChangedEvent"/> and
    /// <see cref="AccessibilitySettings.SettingsChanged"/>. The only periodic work is a throttled
    /// (<see cref="_updateInterval"/>) camera follow plus stepping any running fade. One ParticleSystem
    /// per library layer, created lazily and kept (pooled) — layers fade by emission rate and are
    /// deactivated once empty, so there is no Instantiate/Destroy churn and no per-frame allocation.
    /// Materials and the soft-dot texture come from <see cref="SkillVfxManager.GetSharedParticleMaterial"/>.
    ///
    /// Caps are ≤ <see cref="AmbientVfxLibrary.MaxParticlesCap"/> per layer and scaled down in the
    /// WorldMap scene / on mobile (<see cref="FrameRateManager"/>). "Reduce flashes" turns blinking
    /// layers into a steady glow and dims sparkles (<see cref="AmbientVfxFlashPolicy"/>).
    /// </summary>
    public class AmbientVfx : MonoBehaviour
    {
        private sealed class Layer
        {
            public AmbientVfxPreset Preset;
            public ParticleSystem System;
            public ParticleSystemRenderer Renderer;
            public Gradient LifetimeGradient;
            public float Intensity;
            public float Target;
            public float FadeSpeed;
        }

        [SerializeField] private AmbientVfxLibrary _library;
        [SerializeField] private Transform _followTarget;
        [Tooltip("Without a follow target, the emitters centre this far in front of Camera.main (roughly where the player looks).")]
        [SerializeField] [Min(0f)] private float _cameraLookAhead = 6f;
        [SerializeField] [Min(0.05f)] private float _updateInterval = 0.2f;

        private readonly List<AmbientVfxPreset> _activeBuffer = new List<AmbientVfxPreset>(8);

        private Layer[] _layers;
        private Transform _root;
        private AccessibilitySettings _accessibility;
        private float _timer;
        private bool _subscribed;
        private bool _isFading;

        private BiomeType _biome = BiomeType.Forest;
        private TimeOfDayPhase _phase = TimeOfDayPhase.Day;
        private WeatherType _weather = WeatherType.Clear;
        private bool _reduceFlashes;
        private float _capMultiplier = 1f;

        public AmbientVfxLibrary Library => _library;
        public BiomeType Biome => _biome;
        public TimeOfDayPhase Phase => _phase;
        public WeatherType Weather => _weather;
        public float CapMultiplier => _capMultiplier;

        private void Awake()
        {
            if (_library == null)
            {
                _library = Resources.Load<AmbientVfxLibrary>(AmbientVfxLibrary.ResourcePath);
            }

            if (_library == null)
            {
                _library = AmbientVfxLibrary.CreateBuiltInDefaults();
            }

            _root = new GameObject("AmbientParticles").transform;
            _root.SetParent(transform, false);
            BuildLayers();
        }

        private void OnEnable()
        {
            ReadCurrentState();
            Subscribe();
            RefreshTargets(_library.FadeSeconds);
            _timer = _updateInterval;
        }

        private void OnDisable()
        {
            Unsubscribe();
            if (_layers == null)
            {
                return;
            }

            for (int i = 0; i < _layers.Length; i++)
            {
                Layer layer = _layers[i];
                layer.Intensity = 0f;
                layer.Target = 0f;
                if (layer.System != null)
                {
                    layer.System.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                    layer.System.gameObject.SetActive(false);
                }
            }
        }

        private void LateUpdate()
        {
            _timer += Time.unscaledDeltaTime;
            if (_timer < _updateInterval)
            {
                return;
            }

            float elapsed = _timer;
            _timer = 0f;

            FollowTarget();
            if (_isFading)
            {
                StepFades(elapsed);
            }
        }

        // ---------------------------------------------------------------
        // Events
        // ---------------------------------------------------------------

        private void Subscribe()
        {
            if (_subscribed)
            {
                return;
            }

            _subscribed = true;
            EventBus.Subscribe<TimeOfDayPhaseChangedEvent>(OnPhaseChanged);
            EventBus.Subscribe<WeatherChangedEvent>(OnWeatherChanged);
            EventBus.Subscribe<WorldMapRegionChangedEvent>(OnRegionChanged);

            if (ServiceLocator.TryGet(out _accessibility) && _accessibility != null)
            {
                _accessibility.SettingsChanged += OnAccessibilityChanged;
            }
        }

        private void Unsubscribe()
        {
            if (!_subscribed)
            {
                return;
            }

            _subscribed = false;
            EventBus.Unsubscribe<TimeOfDayPhaseChangedEvent>(OnPhaseChanged);
            EventBus.Unsubscribe<WeatherChangedEvent>(OnWeatherChanged);
            EventBus.Unsubscribe<WorldMapRegionChangedEvent>(OnRegionChanged);

            if (_accessibility != null)
            {
                _accessibility.SettingsChanged -= OnAccessibilityChanged;
                _accessibility = null;
            }
        }

        private void OnPhaseChanged(TimeOfDayPhaseChangedEvent evt)
        {
            _phase = evt.CurrentPhase;
            RefreshTargets(_library.FadeSeconds);
        }

        private void OnWeatherChanged(WeatherChangedEvent evt)
        {
            _weather = evt.CurrentWeather;
            _biome = evt.Biome;
            // Follow the weather blend so ambient layers cross-fade with WeatherVisuals' precipitation.
            RefreshTargets(evt.TransitionSeconds > 0f ? evt.TransitionSeconds : _library.FadeSeconds);
        }

        private void OnRegionChanged(WorldMapRegionChangedEvent evt)
        {
            _biome = evt.CurrentRegion;
            RefreshTargets(_library.FadeSeconds);
        }

        /// <summary>
        /// Shows the ambient layers of <paramref name="biome"/> without a region change (e.g. a
        /// combat arena dressed for the encounter's biome while WeatherSystem kept the same weather,
        /// so no WeatherChangedEvent carried the new biome). No-op if unchanged.
        /// </summary>
        public void SetBiome(BiomeType biome)
        {
            if (_biome == biome || _library == null)
            {
                _biome = biome;
                return;
            }

            _biome = biome;
            RefreshTargets(_library.FadeSeconds);
        }

        private void OnAccessibilityChanged()
        {
            bool reduce = _accessibility != null && _accessibility.ReduceFlashEffects;
            if (reduce == _reduceFlashes)
            {
                return;
            }

            _reduceFlashes = reduce;
            for (int i = 0; i < _layers.Length; i++)
            {
                if (_layers[i].System != null)
                {
                    ApplyLook(_layers[i]);
                }
            }
        }

        // ---------------------------------------------------------------
        // State
        // ---------------------------------------------------------------

        private void ReadCurrentState()
        {
            if (ServiceLocator.TryGet(out TimeOfDayManager time) && time != null)
            {
                _phase = time.Phase;
            }

            if (ServiceLocator.TryGet(out WeatherSystem weather) && weather != null)
            {
                _biome = weather.CurrentBiome;
                _weather = weather.CurrentWeather;
            }
            else if (ServiceLocator.TryGet(out WorldMapProgress progress) && progress != null)
            {
                _biome = progress.CurrentRegion;
            }

            _reduceFlashes = ServiceLocator.TryGet(out AccessibilitySettings accessibility) && accessibility != null && accessibility.ReduceFlashEffects;

            bool isMobile = Application.isMobilePlatform;
            string worldMapScene = SceneTransitionService.DefaultWorldMapSceneName;
            if (ServiceLocator.TryGet(out FrameRateManager frameRate) && frameRate != null)
            {
                isMobile = frameRate.IsMobile;
                worldMapScene = frameRate.WorldMapSceneName;
            }

            // This component is scene-local, so its own scene tells the context reliably even while
            // FrameRateManager is still processing the same sceneLoaded callback.
            bool isWorldMap = gameObject.scene.name == worldMapScene;
            float multiplier = _library.GetCapMultiplier(isWorldMap, isMobile);
            if (!Mathf.Approximately(multiplier, _capMultiplier))
            {
                _capMultiplier = multiplier;
                for (int i = 0; i < _layers.Length; i++)
                {
                    if (_layers[i].System != null)
                    {
                        ParticleSystem.MainModule main = _layers[i].System.main;
                        main.maxParticles = AmbientVfxLibrary.ScaleCap(_layers[i].Preset.maxParticles, _capMultiplier);
                    }
                }
            }
        }

        private void RefreshTargets(float fadeSeconds)
        {
            _library.ResolveActive(_biome, _phase, _weather, _activeBuffer);
            float speed = fadeSeconds > 0.01f ? 1f / fadeSeconds : 1000f;

            for (int i = 0; i < _layers.Length; i++)
            {
                Layer layer = _layers[i];
                float target = _activeBuffer.Contains(layer.Preset) ? 1f : 0f;
                layer.FadeSpeed = speed;
                if (Mathf.Approximately(target, layer.Target) && Mathf.Approximately(layer.Intensity, target))
                {
                    continue;
                }

                layer.Target = target;
                if (target > 0f && layer.System == null)
                {
                    CreateSystem(layer);
                }

                if (target > 0f && !layer.System.gameObject.activeSelf)
                {
                    layer.System.gameObject.SetActive(true);
                    layer.System.Play();
                }

                _isFading = true;
            }
        }

        private void StepFades(float elapsed)
        {
            bool anyFading = false;
            for (int i = 0; i < _layers.Length; i++)
            {
                Layer layer = _layers[i];
                if (layer.System == null || !layer.System.gameObject.activeSelf)
                {
                    continue;
                }

                if (!Mathf.Approximately(layer.Intensity, layer.Target))
                {
                    layer.Intensity = Mathf.MoveTowards(layer.Intensity, layer.Target, elapsed * layer.FadeSpeed);
                    ParticleSystem.EmissionModule emission = layer.System.emission;
                    emission.rateOverTime = layer.Preset.rate * _capMultiplier * layer.Intensity;
                    anyFading = true;
                    continue;
                }

                if (layer.Target <= 0f)
                {
                    // Faded out: wait for the last particles to die, then park the system.
                    if (layer.System.particleCount == 0)
                    {
                        layer.System.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                        layer.System.gameObject.SetActive(false);
                    }
                    else
                    {
                        anyFading = true;
                    }
                }
            }

            _isFading = anyFading;
        }

        private void FollowTarget()
        {
            if (_followTarget != null)
            {
                _root.position = _followTarget.position;
                return;
            }

            Camera cam = Camera.main;
            if (cam != null)
            {
                Transform t = cam.transform;
                _root.position = t.position + t.forward * _cameraLookAhead;
            }
        }

        // ---------------------------------------------------------------
        // Particle systems
        // ---------------------------------------------------------------

        private void BuildLayers()
        {
            IReadOnlyList<AmbientVfxPreset> presets = _library.Presets;
            int count = 0;
            for (int i = 0; i < presets.Count; i++)
            {
                if (presets[i] != null)
                {
                    count++;
                }
            }

            _layers = new Layer[count];
            int index = 0;
            for (int i = 0; i < presets.Count; i++)
            {
                if (presets[i] != null)
                {
                    _layers[index++] = new Layer { Preset = presets[i] };
                }
            }
        }

        private void CreateSystem(Layer layer)
        {
            AmbientVfxPreset preset = layer.Preset;
            var go = new GameObject("Ambient_" + preset.presetId);
            go.SetActive(false);
            go.transform.SetParent(_root, false);
            go.transform.localPosition = new Vector3(0f, preset.heightOffset, 0f);

            ParticleSystem system = go.AddComponent<ParticleSystem>();
            system.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            float size = Mathf.Max(0.005f, preset.particleSize);
            float lifetime = Mathf.Max(0.1f, preset.particleLifetime);

            ParticleSystem.MainModule main = system.main;
            main.loop = true;
            main.playOnAwake = false;
            main.duration = 5f;
            main.maxParticles = AmbientVfxLibrary.ScaleCap(preset.maxParticles, _capMultiplier);
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.scalingMode = ParticleSystemScalingMode.Hierarchy;
            main.startSpeed = 0f;
            main.startSize = new ParticleSystem.MinMaxCurve(size * 0.6f, size);
            main.startLifetime = new ParticleSystem.MinMaxCurve(lifetime * 0.7f, lifetime);
            main.gravityModifier = preset.gravity;

            ParticleSystem.ShapeModule shape = system.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(Mathf.Max(0.1f, preset.area.x), Mathf.Max(0.1f, preset.area.y), Mathf.Max(0.1f, preset.area.z));

            ParticleSystem.EmissionModule emission = system.emission;
            emission.enabled = true;
            emission.rateOverTime = 0f;

            layer.LifetimeGradient = new Gradient();
            ParticleSystem.ColorOverLifetimeModule colorOverLifetime = system.colorOverLifetime;
            colorOverLifetime.enabled = true;

            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;

            ConfigureMotion(system, renderer, preset);

            layer.System = system;
            layer.Renderer = renderer;
            ApplyLook(layer);
        }

        /// <summary>Colours, lifetime alpha curve and blending — re-run when "reduce flashes" toggles.</summary>
        private void ApplyLook(Layer layer)
        {
            AmbientVfxPreset preset = layer.Preset;
            ParticleSystem.MainModule main = layer.System.main;
            main.startColor = new ParticleSystem.MinMaxGradient(
                AmbientVfxFlashPolicy.ResolveColor(preset.primaryColor, preset.flashLike, _reduceFlashes),
                AmbientVfxFlashPolicy.ResolveColor(preset.secondaryColor, preset.flashLike, _reduceFlashes));

            AmbientVfxFlashPolicy.ApplyLifetimeGradient(layer.LifetimeGradient, preset, _reduceFlashes);
            ParticleSystem.ColorOverLifetimeModule colorOverLifetime = layer.System.colorOverLifetime;
            colorOverLifetime.color = new ParticleSystem.MinMaxGradient(layer.LifetimeGradient);

            layer.Renderer.sharedMaterial = SkillVfxManager.GetSharedParticleMaterial(AmbientVfxFlashPolicy.UseAdditive(preset, _reduceFlashes));
        }

        private static void ConfigureMotion(ParticleSystem system, ParticleSystemRenderer renderer, AmbientVfxPreset preset)
        {
            float s = Mathf.Max(0f, preset.particleSpeed);

            switch (preset.kind)
            {
                case AmbientVfxKind.Fireflies:
                    SetVelocity(system, -s, s, -s * 0.5f, s * 0.5f, -s, s);
                    AddNoise(system, 0.5f, 0.35f);
                    break;

                case AmbientVfxKind.FallingLeaves:
                    SetVelocity(system, s * 0.5f, s * 1.5f, -s * 2f, -s, -s * 0.5f, s * 0.5f);
                    AddNoise(system, 0.35f, 0.4f);
                    AddSpin(system, 1.5f);
                    break;

                case AmbientVfxKind.HeatShimmer:
                    SetVelocity(system, -s * 0.3f, s * 0.3f, s * 0.5f, s, -s * 0.3f, s * 0.3f);
                    AddNoise(system, 0.15f, 0.6f);
                    break;

                case AmbientVfxKind.SnowFlurry:
                    SetVelocity(system, 0f, s, -s * 1.5f, -s * 0.8f, -s * 0.4f, s * 0.4f);
                    AddNoise(system, 0.3f, 0.3f);
                    break;

                case AmbientVfxKind.WindWisps:
                    SetVelocity(system, s * 0.8f, s * 1.2f, -s * 0.05f, s * 0.05f, -s * 0.1f, s * 0.1f);
                    renderer.renderMode = ParticleSystemRenderMode.Stretch;
                    renderer.lengthScale = 1.5f;
                    renderer.velocityScale = 0.15f;
                    break;

                case AmbientVfxKind.Drips:
                    renderer.renderMode = ParticleSystemRenderMode.Stretch;
                    renderer.lengthScale = 1f;
                    renderer.velocityScale = 0.04f;
                    break;

                default: // DustMotes
                    SetVelocity(system, -s, s, -s * 0.3f, s * 0.3f, -s, s);
                    AddNoise(system, 0.2f, 0.25f);
                    break;
            }
        }

        private static void SetVelocity(ParticleSystem system, float minX, float maxX, float minY, float maxY, float minZ, float maxZ)
        {
            // All three axes must share the same curve mode (TwoConstants here).
            ParticleSystem.VelocityOverLifetimeModule velocity = system.velocityOverLifetime;
            velocity.enabled = true;
            velocity.space = ParticleSystemSimulationSpace.World;
            velocity.x = new ParticleSystem.MinMaxCurve(minX, maxX);
            velocity.y = new ParticleSystem.MinMaxCurve(minY, maxY);
            velocity.z = new ParticleSystem.MinMaxCurve(minZ, maxZ);
        }

        private static void AddNoise(ParticleSystem system, float strength, float frequency)
        {
            ParticleSystem.NoiseModule noise = system.noise;
            noise.enabled = true;
            noise.quality = ParticleSystemNoiseQuality.Low;
            noise.strength = strength;
            noise.frequency = frequency;
            noise.scrollSpeed = 0.2f;
            noise.damping = true;
        }

        private static void AddSpin(ParticleSystem system, float radiansPerSecond)
        {
            ParticleSystem.MainModule main = system.main;
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);

            ParticleSystem.RotationOverLifetimeModule rotation = system.rotationOverLifetime;
            rotation.enabled = true;
            rotation.z = new ParticleSystem.MinMaxCurve(-radiansPerSecond, radiansPerSecond);
        }

#if UNITY_EDITOR
        /// <summary>Editor/test-only: fade target (0/1) of a layer, or -1 when the id is unknown.</summary>
        public float GetLayerTargetForTests(string presetId)
        {
            if (_layers == null)
            {
                return -1f;
            }

            for (int i = 0; i < _layers.Length; i++)
            {
                if (_layers[i].Preset.presetId == presetId)
                {
                    return _layers[i].Target;
                }
            }

            return -1f;
        }
#endif
    }
}
