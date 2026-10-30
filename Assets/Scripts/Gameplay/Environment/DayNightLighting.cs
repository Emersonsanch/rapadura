using Rapadura.Core.DI;
using Rapadura.Core.Events;
using UnityEngine;
using UnityEngine.Rendering;

namespace Rapadura.Gameplay.Environment
{
    /// <summary>
    /// Scene-side presentation of <see cref="TimeOfDayManager"/>: rotates and colours the
    /// Directional Light and sets ambient colour from gradients/curves sampled at the clock's
    /// normalized time, dimmed by <see cref="WeatherSystem.SunIntensityMultiplier"/>.
    ///
    /// Mobile-friendly: lighting is only re-applied every <see cref="_updateInterval"/> seconds
    /// (and immediately on a phase change); Gradient/AnimationCurve evaluation and RenderSettings
    /// writes don't allocate. Restores the scene's original ambient settings when disabled.
    ///
    /// Scene wiring: add one to any object in Main/WorldMap and (optionally) assign the sun; falls
    /// back to RenderSettings.sun, then the first Directional Light in the scene.
    /// </summary>
    public class DayNightLighting : MonoBehaviour
    {
        [SerializeField] private Light _sun;
        [Tooltip("Seconds between lighting refreshes. 0.2-0.5 is plenty: a 20 min day moves the sun ~0.06° per 0.2 s.")]
        [SerializeField] [Min(0.02f)] private float _updateInterval = 0.25f;
        [Tooltip("Compass heading (Y rotation) of the sun's arc.")]
        [SerializeField] private float _sunYaw = -30f;

        [Header("Over the day (x = normalized time, 0 = midnight, 0.5 = noon)")]
        [SerializeField] private Gradient _sunColor;
        [SerializeField] private AnimationCurve _sunIntensity;
        [SerializeField] private Gradient _ambientColor;

        [Header("Ambient")]
        [SerializeField] private bool _driveAmbient = true;
        [Tooltip("Applies weather dimming to the ambient colour as well (half strength).")]
        [SerializeField] private bool _weatherDimsAmbient = true;

        [SerializeField] [HideInInspector] private bool _defaultsApplied;

        private TimeOfDayManager _time;
        private WeatherSystem _weather;
        private float _timer;

        private bool _capturedAmbient;
        private AmbientMode _originalAmbientMode;
        private Color _originalSky;
        private Color _originalEquator;
        private Color _originalGround;
        private float _equatorRatio = 0.75f;
        private float _groundRatio = 0.45f;

        private void Reset()
        {
            ApplyDefaultCurves();
        }

        private void Awake()
        {
            if (!_defaultsApplied)
            {
                ApplyDefaultCurves();
            }

            ResolveSun();
        }

        private void OnEnable()
        {
            CaptureAmbient();
            EventBus.Subscribe<TimeOfDayPhaseChangedEvent>(OnPhaseChanged);
            _timer = _updateInterval; // apply on the first Update
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<TimeOfDayPhaseChangedEvent>(OnPhaseChanged);
            RestoreAmbient();
        }

        private void Update()
        {
            _timer += Time.unscaledDeltaTime;
            if (_timer < _updateInterval)
            {
                return;
            }

            _timer = 0f;
            Apply();
        }

        /// <summary>Re-samples the clock and pushes sun/ambient values now.</summary>
        public void Apply()
        {
            if (_time == null && !ServiceLocator.TryGet(out _time))
            {
                return;
            }

            if (_weather == null)
            {
                ServiceLocator.TryGet(out _weather);
            }

            float t = _time.NormalizedTime;
            float weatherDim = _weather != null ? _weather.SunIntensityMultiplier : 1f;

            if (_sun == null)
            {
                ResolveSun();
            }

            if (_sun != null)
            {
                _sun.transform.rotation = ComputeSunRotation(t, _sunYaw);
                _sun.color = _sunColor.Evaluate(t);
                _sun.intensity = Mathf.Max(0f, _sunIntensity.Evaluate(t)) * weatherDim * EnvironmentOverrides.SunIntensityMultiplier;
            }

            if (_driveAmbient)
            {
                Color ambient = _ambientColor.Evaluate(t);
                if (_weatherDimsAmbient)
                {
                    ambient *= Mathf.Lerp(1f, weatherDim, 0.5f);
                }

                ambient *= EnvironmentOverrides.AmbientTint;

                // "Menos visão à noite": a little darker at night, softened in already-dark places and by
                // "Noite mais clara" (NightVisibilityRules; never below its floor, never pitch black).
                ambient *= Rapadura.Gameplay.Night.NightVisibility.AmbientMultiplier(_time.DaylightFactor);

                ApplyAmbient(ambient);
            }
        }

        /// <summary>Sun pitch follows the clock: on the horizon at 06:00/18:00, overhead at noon, below the ground at night.</summary>
        public static Quaternion ComputeSunRotation(float normalizedTime, float yaw)
        {
            return Quaternion.Euler(normalizedTime * 360f - 90f, yaw, 0f);
        }

        private void OnPhaseChanged(TimeOfDayPhaseChangedEvent _)
        {
            _timer = _updateInterval;
        }

        private void ResolveSun()
        {
            if (_sun != null)
            {
                return;
            }

            if (RenderSettings.sun != null)
            {
                _sun = RenderSettings.sun;
                return;
            }

            foreach (Light light in FindObjectsByType<Light>(FindObjectsSortMode.None))
            {
                if (light.type == LightType.Directional)
                {
                    _sun = light;
                    return;
                }
            }
        }

        private void CaptureAmbient()
        {
            if (!_driveAmbient)
            {
                return;
            }

            _capturedAmbient = true;
            _originalAmbientMode = RenderSettings.ambientMode;
            _originalSky = RenderSettings.ambientSkyColor;
            _originalEquator = RenderSettings.ambientEquatorColor;
            _originalGround = RenderSettings.ambientGroundColor;

            float sky = _originalSky.maxColorComponent;
            if (_originalAmbientMode == AmbientMode.Trilight && sky > 0.001f)
            {
                _equatorRatio = _originalEquator.maxColorComponent / sky;
                _groundRatio = _originalGround.maxColorComponent / sky;
            }

            // Skybox ambient would need DynamicGI.UpdateEnvironment every refresh (too costly on
            // mobile), so drive a flat colour instead while this component is active.
            if (_originalAmbientMode == AmbientMode.Skybox || _originalAmbientMode == AmbientMode.Custom)
            {
                RenderSettings.ambientMode = AmbientMode.Flat;
            }
        }

        private void ApplyAmbient(Color ambient)
        {
            if (RenderSettings.ambientMode == AmbientMode.Trilight)
            {
                RenderSettings.ambientSkyColor = ambient;
                RenderSettings.ambientEquatorColor = ambient * _equatorRatio;
                RenderSettings.ambientGroundColor = ambient * _groundRatio;
            }
            else
            {
                RenderSettings.ambientLight = ambient;
            }
        }

        private void RestoreAmbient()
        {
            if (!_capturedAmbient)
            {
                return;
            }

            _capturedAmbient = false;
            RenderSettings.ambientMode = _originalAmbientMode;
            RenderSettings.ambientSkyColor = _originalSky;
            RenderSettings.ambientEquatorColor = _originalEquator;
            RenderSettings.ambientGroundColor = _originalGround;
        }

        private void ApplyDefaultCurves()
        {
            _defaultsApplied = true;

            // Night blue -> warm dawn -> white noon -> orange dusk -> night blue.
            _sunColor = MakeGradient(
                (0.00f, new Color(0.35f, 0.42f, 0.65f)),
                (0.23f, new Color(1.00f, 0.55f, 0.35f)),
                (0.32f, new Color(1.00f, 0.92f, 0.80f)),
                (0.50f, new Color(1.00f, 0.98f, 0.94f)),
                (0.74f, new Color(1.00f, 0.60f, 0.30f)),
                (0.85f, new Color(0.35f, 0.42f, 0.65f)));

            // Night ambient is the only light after dusk (the sun is below the horizon, so its
            // "moonlight" intensity lights nothing): bright enough that dark enemies (wolves) stay
            // readable on a phone, still clearly night-blue. Was 0.10/0.12/0.22, which only looked
            // "barely darker" because a stray full-intensity directional light (the character
            // preview stage's) was lighting the arena too; with that fixed it went near black.
            _ambientColor = MakeGradient(
                (0.00f, new Color(0.26f, 0.30f, 0.48f)),
                (0.23f, new Color(0.45f, 0.38f, 0.40f)),
                (0.35f, new Color(0.55f, 0.60f, 0.70f)),
                (0.70f, new Color(0.55f, 0.60f, 0.70f)),
                (0.78f, new Color(0.50f, 0.35f, 0.32f)),
                (0.86f, new Color(0.26f, 0.30f, 0.48f)));

            // Low "moonlight" at night so the scene is never pitch black on a phone screen.
            _sunIntensity = new AnimationCurve(
                new Keyframe(0.00f, 0.15f),
                new Keyframe(0.21f, 0.15f),
                new Keyframe(0.29f, 0.90f),
                new Keyframe(0.50f, 1.20f),
                new Keyframe(0.75f, 0.80f),
                new Keyframe(0.83f, 0.15f),
                new Keyframe(1.00f, 0.15f));
        }

        private static Gradient MakeGradient(params (float time, Color color)[] keys)
        {
            var colorKeys = new GradientColorKey[keys.Length];
            for (int i = 0; i < keys.Length; i++)
            {
                colorKeys[i] = new GradientColorKey(keys[i].color, keys[i].time);
            }

            var gradient = new Gradient();
            gradient.SetKeys(colorKeys, new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) });
            return gradient;
        }
    }
}
