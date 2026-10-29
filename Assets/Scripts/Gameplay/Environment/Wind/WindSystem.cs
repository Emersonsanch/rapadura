using Rapadura.Core.DI;
using UnityEngine;

namespace Rapadura.Gameplay.Environment.Wind
{
    /// <summary>Prevailing wind this frame: normalised XZ direction and strength (0..1, sandstorm = 1).</summary>
    public readonly struct WindFieldState
    {
        public readonly Vector2 Direction;
        public readonly float Strength;

        public WindFieldState(Vector2 direction, float strength)
        {
            Direction = direction;
            Strength = strength;
        }

        public static readonly WindFieldState Calm = new WindFieldState(new Vector2(0.6f, 0.8f), 0f);
    }

    /// <summary>
    /// Scene-side driver of the wind: reads <see cref="WeatherSystem"/> (strength follows the
    /// weather, blending through transitions, with a light breeze in clear skies) and a slowly
    /// veering prevailing direction, then applies one frame of sway to every <see cref="WindSway"/>
    /// in a single loop. Added to "[Environment]" by <see cref="EnvironmentPresentationBootstrap"/>;
    /// placing one in a scene by hand (to set the base bearing or a strength override) suppresses
    /// the auto-created one.
    /// </summary>
    public class WindSystem : MonoBehaviour
    {
        [Tooltip("Compass bearing (degrees) the wind blows toward in calm conditions; it veers ±45° over minutes.")]
        [SerializeField] [Range(0f, 360f)] private float _baseBearing = 35f;
        [Tooltip("Multiplies the weather-driven strength (0 = no wind at all).")]
        [SerializeField] [Min(0f)] private float _strengthMultiplier = 1f;
        [Tooltip("Ignore the weather and use this strength (0..1). Negative = follow the weather.")]
        [SerializeField] private float _strengthOverride = -1f;
        [Tooltip("Seconds between re-reading the weather (the blend between readings is smooth anyway).")]
        [SerializeField] [Min(0.05f)] private float _weatherPollInterval = 0.25f;

        private WeatherSystem _weather;
        private float _pollTimer;
        private float _targetStrength = WindMath.StrengthFor(WeatherType.Clear);
        private float _strength;
        private float _time;
        private WindFieldState _state = WindFieldState.Calm;

        /// <summary>Current prevailing wind (what swayers sample from).</summary>
        public WindFieldState State => _state;
        public float StrengthMultiplier { get => _strengthMultiplier; set => _strengthMultiplier = Mathf.Max(0f, value); }
        public float StrengthOverride { get => _strengthOverride; set => _strengthOverride = value; }

        private void OnEnable()
        {
            _strength = _targetStrength;
            _pollTimer = 0f;
        }

        private void Update()
        {
            float deltaTime = Time.deltaTime;
            _time += deltaTime;

            _pollTimer -= deltaTime;
            if (_pollTimer <= 0f)
            {
                _pollTimer = _weatherPollInterval;
                _targetStrength = ResolveTargetStrength();
            }

            // Ease the global strength so a weather change never snaps every tree at once.
            _strength = Mathf.MoveTowards(_strength, _targetStrength, deltaTime * 0.35f);
            _state = new WindFieldState(WindMath.PrevailingDirection(_baseBearing, _time), _strength * _strengthMultiplier);

            var active = WindSway.Active;
            for (int i = active.Count - 1; i >= 0; i--)
            {
                WindSway sway = active[i];
                if (sway == null)
                {
                    active.RemoveAt(i);
                    continue;
                }

                sway.Apply(in _state, _time, deltaTime);
            }
        }

        private float ResolveTargetStrength()
        {
            if (_strengthOverride >= 0f)
            {
                return Mathf.Clamp(_strengthOverride, 0f, 1f);
            }

            if (_weather == null && !ServiceLocator.TryGet(out _weather))
            {
                return WindMath.StrengthFor(WeatherType.Clear);
            }

            return WindMath.BlendedStrength(_weather.PreviousWeather, _weather.CurrentWeather, _weather.TransitionProgress);
        }

#if UNITY_EDITOR
        /// <summary>Test-only: run one frame with an explicit state (EditMode never calls Update).</summary>
        public void TickForTests(WindFieldState state, float time, float deltaTime)
        {
            _state = state;
            var active = WindSway.Active;
            for (int i = active.Count - 1; i >= 0; i--)
            {
                if (active[i] != null)
                {
                    active[i].Apply(in state, time, deltaTime);
                }
            }
        }
#endif
    }
}
