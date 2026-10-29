using System.Collections.Generic;
using UnityEngine;

namespace Rapadura.Gameplay.Environment.Impact
{
    /// <summary>
    /// A rock or pebble that reacts to nearby ground strikes (earth skills, heavy stomps, a boss
    /// hitting the floor). <see cref="GroundImpactSystem"/> kicks instances in range and advances
    /// all active reactions in one loop; the pose is a local offset/rotation on top of the rest
    /// pose captured on enable (nothing else may move the object while a reaction plays).
    /// </summary>
    [DisallowMultipleComponent]
    public class GroundImpactShaker : MonoBehaviour
    {
        /// <summary>Every enabled instance (for range queries).</summary>
        public static readonly List<GroundImpactShaker> Registered = new List<GroundImpactShaker>(256);

        [SerializeField] private GroundImpactKind _kind = GroundImpactKind.Boulder;
        [Tooltip("Multiplies the kick this instance takes (small rock = 1.5, huge boulder = 0.4).")]
        [SerializeField] [Min(0f)] private float _sensitivity = 1f;

        private Vector3 _restLocalPosition;
        private Quaternion _restLocalRotation = Quaternion.identity;
        private bool _restCaptured;
        private float _kick;
        private float _elapsed;
        private Vector2 _away;
        private float _phase;

        public GroundImpactKind Kind => _kind;
        public bool IsReacting => _kick > 0f;
        public float CurrentKick => _kick;

        public void Configure(GroundImpactKind kind, float sensitivity = 1f)
        {
            _kind = kind;
            _sensitivity = Mathf.Max(0f, sensitivity);
        }

        private void OnEnable()
        {
            CaptureRest();
            _kick = 0f;
            if (!Registered.Contains(this))
            {
                Registered.Add(this);
            }
        }

        private void OnDisable()
        {
            Registered.Remove(this);
            if (_restCaptured)
            {
                transform.localPosition = _restLocalPosition;
                transform.localRotation = _restLocalRotation;
            }

            _kick = 0f;
        }

        /// <summary>Re-captures the rest pose (after re-placing a pooled prop).</summary>
        public void CaptureRest()
        {
            _restLocalPosition = transform.localPosition;
            _restLocalRotation = transform.localRotation;
            _restCaptured = true;
            Vector3 p = transform.position;
            _phase = Mathf.Repeat(p.x * 0.731f + p.z * 1.273f, 1f);
        }

        /// <summary>
        /// Starts (or strengthens) a reaction. <paramref name="kick"/> is already distance-attenuated;
        /// a stronger kick replaces a weaker one in progress, a weaker one is ignored.
        /// </summary>
        public void Kick(float kick, Vector2 awayXZ)
        {
            kick *= _sensitivity;
            if (kick <= 0.01f || kick < _kick * (1f - Mathf.Clamp01(_elapsed / GroundImpactMath.Duration(_kick, _kind))))
            {
                return;
            }

            if (!_restCaptured)
            {
                CaptureRest();
            }

            _kick = kick;
            _elapsed = 0f;
            _away = awayXZ;
        }

        /// <summary>Advances the reaction; returns false once it has settled back to rest.</summary>
        public bool Step(float deltaTime)
        {
            if (_kick <= 0f)
            {
                return false;
            }

            _elapsed += deltaTime;
            GroundImpactPose pose = GroundImpactMath.Evaluate(_kind, _kick, _elapsed, _away, _phase);
            Transform parent = transform.parent;
            if (pose.Finished)
            {
                _kick = 0f;
                transform.localPosition = _restLocalPosition;
                transform.localRotation = _restLocalRotation;
                return false;
            }

            Vector3 offsetLocal = parent != null ? parent.InverseTransformVector(pose.PositionOffset) : pose.PositionOffset;
            Quaternion rotationLocal = parent != null ? Quaternion.Inverse(parent.rotation) * pose.Rotation * parent.rotation : pose.Rotation;
            transform.localPosition = _restLocalPosition + offsetLocal;
            transform.localRotation = rotationLocal * _restLocalRotation;
            return true;
        }
    }
}
