using System;
using Rapadura.Core.DI;
using Rapadura.Gameplay.Skills.Vfx;
using UnityEngine;
using UnityEngine.Rendering;

namespace Rapadura.Gameplay.Environment
{
    /// <summary>Per-weather look used by <see cref="WeatherVisuals"/> (fog + precipitation).</summary>
    [Serializable]
    public class WeatherVisualProfile
    {
        public WeatherType Weather = WeatherType.Clear;
        [Tooltip("Exponential fog density at full strength. Clear uses the scene's own fog instead.")]
        [Min(0f)] public float FogDensity;
        public Color FogColor = Color.gray;
        [Tooltip("Particles per second at full strength (0 = no particle system for this weather).")]
        [Min(0f)] public float ParticleRate;
        [Min(1)] public int MaxParticles = 200;
        public Color ParticleColor = Color.white;

        public WeatherVisualProfile()
        {
        }

        public WeatherVisualProfile(WeatherType weather, float fogDensity, Color fogColor, float particleRate, int maxParticles, Color particleColor)
        {
            Weather = weather;
            FogDensity = fogDensity;
            FogColor = fogColor;
            ParticleRate = particleRate;
            MaxParticles = maxParticles;
            ParticleColor = particleColor;
        }
    }

    /// <summary>
    /// Scene-side presentation of <see cref="WeatherSystem"/>: blends RenderSettings fog between the
    /// previous and current weather (Clear = the scene's own authored fog) and drives one lazily
    /// created, reused ParticleSystem per precipitation type (rain/snow/sand), built entirely in code
    /// with low particle caps for mobile. Systems are created once and kept (pooled) — switching
    /// weather only changes emission rates, so there is no Instantiate/Destroy churn or per-frame GC.
    /// Fog colour is darkened at night via <see cref="TimeOfDayManager.DaylightFactor"/>.
    ///
    /// Scene wiring: add one to any object in Main/WorldMap. The emitter box follows
    /// <see cref="_followTarget"/>, else the object tagged Player, else the ground point Camera.main
    /// looks at -- never the camera itself: centring the rain column on the camera made drops fall
    /// right past the lens, where a single stretched billboard filled the screen as a big pale quad
    /// (playtest, map + arena). Assign <see cref="_particleMaterial"/> for art-directed particles;
    /// otherwise the shared soft-dot particle material (<see cref="SkillVfxManager.GetSharedParticleMaterial"/>)
    /// is used, so drops are soft-edged thin streaks rather than solid untextured rectangles.
    /// </summary>
    public class WeatherVisuals : MonoBehaviour
    {
        private const int WeatherTypeCount = 5;
        private const string PlayerTag = "Player";

        /// <summary>Cap on a single particle's on-screen size (fraction of the viewport) so a drop that does get close to the camera stays a speck.</summary>
        public const float MaxParticleScreenSize = 0.02f;

        [SerializeField] private Transform _followTarget;
        [SerializeField] private Vector3 _emitterOffset = new Vector3(0f, 12f, 0f);
        [SerializeField] private Vector2 _emitterArea = new Vector2(24f, 24f);
        [SerializeField] [Min(0.02f)] private float _updateInterval = 0.1f;
        [Tooltip("Without a follow target or Player, the emitter centres on the ground point Camera.main looks at (plane at this height).")]
        [SerializeField] private float _fallbackGroundHeight = 0f;
        [SerializeField] private Material _particleMaterial;
        [Tooltip("Fog colour multiplier at full night.")]
        [SerializeField] private Color _nightFogTint = new Color(0.25f, 0.28f, 0.4f, 1f);
        [SerializeField] private WeatherVisualProfile[] _profiles = CreateDefaultProfiles();

        private readonly ParticleSystem[] _systems = new ParticleSystem[WeatherTypeCount];
        private readonly WeatherVisualProfile[] _profileByType = new WeatherVisualProfile[WeatherTypeCount];

        private WeatherSystem _weather;
        private TimeOfDayManager _time;
        private Transform _root;
        private Transform _player;
        private float _timer;

        private bool _capturedFog;
        private bool _originalFogEnabled;
        private FogMode _originalFogMode;
        private float _originalFogDensity;
        private Color _originalFogColor;

        public static WeatherVisualProfile[] CreateDefaultProfiles()
        {
            return new[]
            {
                new WeatherVisualProfile(WeatherType.Clear, 0f, Color.gray, 0f, 1, Color.white),
                new WeatherVisualProfile(WeatherType.Rain, 0.025f, new Color(0.45f, 0.5f, 0.55f), 220f, 300, new Color(0.75f, 0.82f, 0.95f, 0.55f)),
                new WeatherVisualProfile(WeatherType.Fog, 0.06f, new Color(0.7f, 0.72f, 0.74f), 0f, 1, Color.white),
                new WeatherVisualProfile(WeatherType.Sandstorm, 0.045f, new Color(0.78f, 0.62f, 0.4f), 300f, 1000, new Color(1f, 0.95f, 0.82f, 0.9f)),
                new WeatherVisualProfile(WeatherType.Snow, 0.03f, new Color(0.82f, 0.86f, 0.92f), 70f, 200, new Color(1f, 1f, 1f, 0.9f)),
            };
        }

        private void Awake()
        {
            BuildProfileLookup();
            _root = new GameObject("WeatherParticles").transform;
            _root.SetParent(transform, false);
        }

        private void OnEnable()
        {
            CaptureFog();
            _timer = _updateInterval;
        }

        private void OnDisable()
        {
            RestoreFog();
            for (int i = 0; i < _systems.Length; i++)
            {
                if (_systems[i] != null)
                {
                    _systems[i].gameObject.SetActive(false);
                }
            }
        }

        private void OnValidate()
        {
            BuildProfileLookup();
        }

        private void LateUpdate()
        {
            if (_root != null && TryResolveAnchor(out Vector3 anchor))
            {
                _root.position = anchor + _emitterOffset;
            }

            _timer += Time.unscaledDeltaTime;
            if (_timer < _updateInterval)
            {
                return;
            }

            _timer = 0f;
            Apply();
        }

        private bool TryResolveAnchor(out Vector3 anchor)
        {
            Transform target = _followTarget != null ? _followTarget : _player;

            if (target != null)
            {
                anchor = target.position;
                return true;
            }

            Camera cam = Camera.main;
            if (cam == null)
            {
                anchor = default;
                return false;
            }

            anchor = ResolveCameraGroundPoint(cam.transform.position, cam.transform.forward, _fallbackGroundHeight);
            return true;
        }

        /// <summary>
        /// Where a camera at <paramref name="cameraPosition"/> looking along <paramref name="cameraForward"/>
        /// meets the horizontal plane at <paramref name="groundHeight"/> (capped at 60m); for a camera not
        /// looking down, a point 6m ahead dropped to that plane. Keeps the rain column around what the
        /// player sees instead of around the lens.
        /// </summary>
        public static Vector3 ResolveCameraGroundPoint(Vector3 cameraPosition, Vector3 cameraForward, float groundHeight)
        {
            const float MaxDistance = 60f;
            const float FlatLookAhead = 6f;

            if (cameraForward.y < -0.05f && cameraPosition.y > groundHeight)
            {
                float distance = Mathf.Min((cameraPosition.y - groundHeight) / -cameraForward.y, MaxDistance);
                Vector3 hit = cameraPosition + cameraForward * distance;
                hit.y = groundHeight;
                return hit;
            }

            Vector3 ahead = cameraPosition + cameraForward * FlatLookAhead;
            ahead.y = groundHeight;
            return ahead;
        }

        /// <summary>Re-samples the weather blend and pushes fog/particle values now.</summary>
        public void Apply()
        {
            // Throttled with the rest of Apply (not per frame), and only while unresolved:
            // the player can spawn after this object, and the world map has none.
            if (_followTarget == null && _player == null)
            {
                GameObject player = GameObject.FindGameObjectWithTag(PlayerTag);
                _player = player != null ? player.transform : null;
            }

            if (_weather == null && !ServiceLocator.TryGet(out _weather))
            {
                return;
            }

            if (_time == null)
            {
                ServiceLocator.TryGet(out _time);
            }

            ApplyFog();
            ApplyParticles();
        }

        private void ApplyFog()
        {
            if (!_capturedFog)
            {
                return;
            }

            // A biome arena (EnvironmentOverrides) can replace the authored clear-weather fog baseline.
            bool baseline = EnvironmentOverrides.HasFogBaseline;
            float clearDensity = baseline
                ? EnvironmentOverrides.FogBaselineDensity
                : (_originalFogEnabled ? _originalFogDensity : 0f);
            float progress = _weather.TransitionProgress;

            WeatherVisualProfile from = GetProfile(_weather.PreviousWeather);
            WeatherVisualProfile to = GetProfile(_weather.CurrentWeather);

            float fromDensity = FogDensityOf(from, clearDensity);
            float toDensity = FogDensityOf(to, clearDensity);
            float density = Mathf.Lerp(fromDensity, toDensity, progress);

            Color fromColor = FogColorOf(from);
            Color toColor = FogColorOf(to);
            Color color = Color.Lerp(fromColor, toColor, progress);

            if (_time != null)
            {
                color *= Color.Lerp(_nightFogTint, Color.white, _time.DaylightFactor);
            }

            bool weatherFog = density > clearDensity + 0.0001f;
            if (weatherFog)
            {
                RenderSettings.fog = true;
                RenderSettings.fogMode = FogMode.Exponential;
                RenderSettings.fogDensity = density;
            }
            else if (baseline)
            {
                RenderSettings.fog = clearDensity > 0f;
                RenderSettings.fogMode = FogMode.Exponential;
                RenderSettings.fogDensity = clearDensity;
            }
            else
            {
                RenderSettings.fog = _originalFogEnabled;
                RenderSettings.fogMode = _originalFogMode;
                RenderSettings.fogDensity = _originalFogDensity;
            }

            RenderSettings.fogColor = color;
        }

        private void ApplyParticles()
        {
            for (int i = 0; i < WeatherTypeCount; i++)
            {
                WeatherVisualProfile profile = _profileByType[i];
                float weight = _weather.GetWeight((WeatherType)i);
                float rate = profile != null ? profile.ParticleRate * weight : 0f;

                ParticleSystem system = _systems[i];
                if (rate <= 0.01f)
                {
                    if (system != null && system.gameObject.activeSelf)
                    {
                        ParticleSystem.EmissionModule idle = system.emission;
                        idle.rateOverTime = 0f;
                        if (system.particleCount == 0)
                        {
                            system.gameObject.SetActive(false);
                        }
                    }

                    continue;
                }

                if (system == null)
                {
                    system = _systems[i] = CreateSystem((WeatherType)i, profile);
                }

                if (!system.gameObject.activeSelf)
                {
                    system.gameObject.SetActive(true);
                    system.Play();
                }

                ParticleSystem.EmissionModule emission = system.emission;
                emission.rateOverTime = rate;
            }
        }

        private ParticleSystem CreateSystem(WeatherType weather, WeatherVisualProfile profile)
        {
            var go = new GameObject("Weather_" + weather);
            go.SetActive(false);
            go.transform.SetParent(_root, false);
            // Box shape emits along local +Z; point it at the ground.
            go.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);

            ParticleSystem system = go.AddComponent<ParticleSystem>();
            system.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            ParticleSystem.MainModule main = system.main;
            main.loop = true;
            main.playOnAwake = false;
            main.maxParticles = Mathf.Max(1, profile.MaxParticles);
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.startColor = profile.ParticleColor;

            ParticleSystem.ShapeModule shape = system.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(_emitterArea.x, _emitterArea.y, 1f);

            ParticleSystem.EmissionModule emission = system.emission;
            emission.rateOverTime = 0f;

            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.sharedMaterial = _particleMaterial != null ? _particleMaterial : SkillVfxManager.GetSharedParticleMaterial(false);
            ConfigurePrecipitation(weather, system, renderer);

            return system;
        }

        /// <summary>
        /// Per-weather particle look. Rain = thin stretched streaks (2.5cm wide, ~0.8m long at fall
        /// speed), snow = small soft billboards, sand = short thin streaks. All ignore parent scale
        /// (<see cref="ParticleSystemScalingMode.Local"/>; Hierarchy let a scaled parent blow drops up
        /// into big quads) and are capped on screen by <see cref="MaxParticleScreenSize"/>.
        /// </summary>
        public static void ConfigurePrecipitation(WeatherType weather, ParticleSystem system, ParticleSystemRenderer renderer)
        {
            ParticleSystem.MainModule main = system.main;
            main.scalingMode = ParticleSystemScalingMode.Local;

            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.alignment = ParticleSystemRenderSpace.View;
            renderer.minParticleSize = 0f;
            renderer.maxParticleSize = MaxParticleScreenSize;

            switch (weather)
            {
                case WeatherType.Rain:
                    main.startLifetime = 0.9f;
                    main.startSpeed = 16f;
                    main.startSize = 0.025f;
                    renderer.renderMode = ParticleSystemRenderMode.Stretch;
                    renderer.cameraVelocityScale = 0f;
                    renderer.lengthScale = 1f;
                    renderer.velocityScale = 0.05f;
                    break;

                case WeatherType.Snow:
                    main.startLifetime = 7f;
                    main.startSpeed = new ParticleSystem.MinMaxCurve(1.2f, 2.2f);
                    main.startSize = new ParticleSystem.MinMaxCurve(0.06f, 0.12f);
                    renderer.renderMode = ParticleSystemRenderMode.Billboard;
                    AddDrift(system, 0.6f, 0.4f);
                    break;

                case WeatherType.Sandstorm:
                    // Pale streaks skimming ~3 m above the floor, blown sideways across the view, so
                    // the storm reads as blowing sand and not just a tint (playtest r6). Falling from
                    // the shared 12 m emitter, sand-coloured, they were above the camera / blown out
                    // of frame and the same colour as the ground: invisible. The emitter is dropped
                    // to just above the ground and moved upwind so the streaks cross the arena.
                    system.transform.localPosition = new Vector3(-10f, -9f, 0f);
                    main.startLifetime = 3.5f;
                    main.startSpeed = new ParticleSystem.MinMaxCurve(0.4f, 1.4f);
                    main.startSize = new ParticleSystem.MinMaxCurve(0.03f, 0.06f);
                    renderer.renderMode = ParticleSystemRenderMode.Stretch;
                    renderer.cameraVelocityScale = 0f;
                    renderer.lengthScale = 3f;
                    renderer.velocityScale = 0.08f;
                    AddDrift(system, 6.5f, 1.5f);
                    break;

                default:
                    main.startLifetime = 3f;
                    main.startSpeed = 2f;
                    main.startSize = 0.1f;
                    renderer.renderMode = ParticleSystemRenderMode.Billboard;
                    break;
            }
        }

        private static void AddDrift(ParticleSystem system, float sideways, float jitter)
        {
            ParticleSystem.VelocityOverLifetimeModule velocity = system.velocityOverLifetime;
            velocity.enabled = true;
            velocity.space = ParticleSystemSimulationSpace.World;
            velocity.x = new ParticleSystem.MinMaxCurve(sideways - jitter, sideways + jitter);
            velocity.y = new ParticleSystem.MinMaxCurve(0f, 0f);
            velocity.z = new ParticleSystem.MinMaxCurve(-jitter, jitter);
        }

        private WeatherVisualProfile GetProfile(WeatherType weather)
        {
            int index = (int)weather;
            return index >= 0 && index < WeatherTypeCount ? _profileByType[index] : null;
        }

        private static float FogDensityOf(WeatherVisualProfile profile, float clearDensity)
        {
            if (profile == null || profile.Weather == WeatherType.Clear)
            {
                return clearDensity;
            }

            return Mathf.Max(clearDensity, profile.FogDensity);
        }

        private Color FogColorOf(WeatherVisualProfile profile)
        {
            return profile == null || profile.Weather == WeatherType.Clear ? ClearFogColor : profile.FogColor;
        }

        private Color ClearFogColor
        {
            get { return EnvironmentOverrides.HasFogBaseline ? EnvironmentOverrides.FogBaselineColor : _originalFogColor; }
        }

        private void BuildProfileLookup()
        {
            Array.Clear(_profileByType, 0, _profileByType.Length);
            if (_profiles == null)
            {
                return;
            }

            foreach (WeatherVisualProfile profile in _profiles)
            {
                int index = profile != null ? (int)profile.Weather : -1;
                if (index >= 0 && index < WeatherTypeCount)
                {
                    _profileByType[index] = profile;
                }
            }
        }

        private void CaptureFog()
        {
            _capturedFog = true;
            _originalFogEnabled = RenderSettings.fog;
            _originalFogMode = RenderSettings.fogMode;
            _originalFogDensity = RenderSettings.fogDensity;
            _originalFogColor = RenderSettings.fogColor;
        }

        private void RestoreFog()
        {
            if (!_capturedFog)
            {
                return;
            }

            _capturedFog = false;
            RenderSettings.fog = _originalFogEnabled;
            RenderSettings.fogMode = _originalFogMode;
            RenderSettings.fogDensity = _originalFogDensity;
            RenderSettings.fogColor = _originalFogColor;
        }
    }
}
