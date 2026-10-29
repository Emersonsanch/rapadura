using System.Collections.Generic;
using UnityEngine;

namespace Rapadura.Gameplay.Environment.Wind
{
    /// <summary>
    /// Makes one object lean and flutter in the wind by rotating it about its own pivot (which must
    /// sit at the base: the tree prefabs, grass tufts and arena props all do). Purely a transform
    /// effect, so it works on static meshes with no shader or rig, keeps GPU instancing intact, and
    /// composes with <see cref="TreeColorVariation"/>. The rest rotation is captured when enabled;
    /// nothing else may rotate the object while this is active.
    ///
    /// Instances do not run their own Update: <see cref="WindSystem"/> walks <see cref="Active"/>
    /// once per frame (one callback instead of 130), with scaled time so pausing freezes the wind.
    /// </summary>
    [DisallowMultipleComponent]
    public class WindSway : MonoBehaviour
    {
        /// <summary>Every enabled instance; iterated by <see cref="WindSystem"/>.</summary>
        public static readonly List<WindSway> Active = new List<WindSway>(256);

        [SerializeField] private WindSwayKind _kind = WindSwayKind.Tree;
        [Tooltip("When Kind is None, this profile is used as-is (custom tuning).")]
        [SerializeField] private WindSwayProfile _customProfile = WindSwayProfile.Tree;
        [Tooltip("Multiplies the lean and flutter of this instance (e.g. a sapling = 1.5, an old oak = 0.6).")]
        [SerializeField] [Min(0f)] private float _scale = 1f;

        private WindSwayProfile _profile;
        private Quaternion _restLocalRotation = Quaternion.identity;
        private Vector2 _feltWind;
        private float _phase;
        private bool _restCaptured;

        public WindSwayKind Kind => _kind;
        public WindSwayProfile Profile => _profile;
        /// <summary>Wind vector this instance currently feels (after inertia), for tests/debug.</summary>
        public Vector2 FeltWind => _feltWind;
        public Quaternion RestLocalRotation => _restLocalRotation;

        /// <summary>Sets the kind (and its built-in profile). Safe to call right after AddComponent.</summary>
        public void Configure(WindSwayKind kind, float scale = 1f)
        {
            _kind = kind;
            _scale = Mathf.Max(0f, scale);
            ResolveProfile();
        }

        private void ResolveProfile()
        {
            _profile = _kind == WindSwayKind.None ? _customProfile : WindSwayProfile.For(_kind);
            _profile.MaxTilt *= _scale;
            _profile.FlutterAmplitude *= _scale;
        }

        private void Awake()
        {
            ResolveProfile();
            _phase = WindMath.PhaseFor(transform.position);
        }

        private void OnEnable()
        {
            CaptureRest();
            _feltWind = Vector2.zero;
            if (!Active.Contains(this))
            {
                Active.Add(this);
            }
        }

        private void OnDisable()
        {
            Active.Remove(this);
            if (_restCaptured)
            {
                transform.localRotation = _restLocalRotation; // leave the object exactly as authored
            }
        }

        /// <summary>Re-captures the rest pose (call after moving/rotating the object on purpose, e.g. arena props re-placed from a pool).</summary>
        public void CaptureRest()
        {
            _restLocalRotation = transform.localRotation;
            _restCaptured = true;
            _phase = WindMath.PhaseFor(transform.position);
        }

        /// <summary>
        /// One frame of sway: samples the field at this position, eases the felt wind toward it, and
        /// applies lean + flutter about the base pivot. No allocations.
        /// </summary>
        public void Apply(in WindFieldState field, float time, float deltaTime)
        {
            if (!_restCaptured)
            {
                CaptureRest();
            }

            Vector3 position = transform.position;
            Vector2 sampled = WindMath.Sample(new Vector2(position.x, position.z), time, field.Direction, field.Strength);
            _feltWind = WindMath.Smooth(_feltWind, sampled, _profile.ResponseSeconds, deltaTime);

            Vector3 axisWorld = WindMath.TiltAxis(_feltWind);
            if (axisWorld == Vector3.zero)
            {
                transform.localRotation = _restLocalRotation;
                return;
            }

            float angle = WindMath.TiltAngle(_feltWind, _profile, time, _phase);
            Transform parent = transform.parent;
            Vector3 axisLocal = parent != null ? parent.InverseTransformDirection(axisWorld) : axisWorld;
            transform.localRotation = Quaternion.AngleAxis(angle, axisLocal) * _restLocalRotation;
        }
    }
}
