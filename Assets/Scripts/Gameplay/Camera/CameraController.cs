using UnityEngine;

namespace Rapadura.Gameplay.Cameras
{
    /// <summary>
    /// MU Online-style fixed isometric camera: locked yaw/pitch (default 45°/57.5°), far from the
    /// character (10-25m, default 15m), smoothly follows only the target's <em>position</em> — never
    /// the character's rotation, never sits behind the character like an action-game camera. Reads
    /// its tuning from a <see cref="CameraSettings"/> asset so designers can author presets without
    /// touching code. Delegates zoom, optional Q/E snap-rotation, and obstruction handling to
    /// <see cref="CameraZoom"/>/<see cref="CameraRotation"/>/<see cref="CameraCollision"/> so each
    /// concern stays small and independently tunable/testable.
    ///
    /// Replaces the old free-orbit <c>PlayerCamera</c> (removed) — <see cref="ApplyLook"/> is kept as
    /// a documented no-op purely so <c>PlayerController</c> doesn't need its input-forwarding branch
    /// ripped out; per the MU Online reference this camera intentionally ignores mouse/stick look.
    /// </summary>
    public class CameraController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform _target;
        [SerializeField] private CameraSettings _settings;

        [Header("Shake")]
        [SerializeField] private float _shakeDecay = 2.5f;

        private CameraZoom _zoom;
        private CameraRotation _rotation;
        private CameraCollision _collision;
        private Vector3 _followVelocity;
        private float _shakeTimeRemaining;
        private float _shakeMagnitude;
        private Vector3 _shakeOffset;

        private void Awake()
        {
            if (_settings == null)
            {
                // Fallback so the component still works if nobody assigned a preset asset — keeps
                // MainSceneBootstrapper simple (it doesn't have to author/find a CameraSettings asset).
                _settings = ScriptableObject.CreateInstance<CameraSettings>();
            }

            _zoom = new CameraZoom(_settings);
            _rotation = new CameraRotation(_settings);
            _collision = new CameraCollision(_settings, _settings.defaultZoom);
        }

        private void Update()
        {
            _zoom.Tick(Time.deltaTime);
            _rotation.Tick(Time.deltaTime);
            UpdateShake();
        }

        private void LateUpdate()
        {
            if (_target == null)
            {
                return;
            }

            float yaw = _settings.cameraYaw + _rotation.CurrentYawOffset;
            Quaternion rotation = Quaternion.Euler(_settings.cameraPitch, yaw, 0f);
            Vector3 backDirection = rotation * Vector3.back;

            float safeDistance = _collision.Resolve(_target.position, backDirection, _zoom.CurrentDistance, Time.deltaTime);
            Vector3 desiredPosition = _target.position + backDirection * safeDistance + _shakeOffset;

            // Smooth follow of position only — rotation is set directly above, never smoothed against
            // the character's own rotation, matching "não deve acompanhar a rotação do personagem".
            transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref _followVelocity, _settings.followSpeed);
            transform.rotation = rotation;
        }

        /// <summary>Triggers a short camera shake — call on hit impacts, explosions, etc.</summary>
        public void Shake(float duration, float magnitude)
        {
            _shakeTimeRemaining = duration;
            _shakeMagnitude = magnitude;
        }

        private void UpdateShake()
        {
            if (_shakeTimeRemaining <= 0f)
            {
                _shakeOffset = Vector3.zero;
                return;
            }

            _shakeTimeRemaining -= Time.deltaTime;
            float falloff = Mathf.Clamp01(_shakeTimeRemaining) * _shakeMagnitude;
            _shakeOffset = Random.insideUnitSphere * falloff;
            _shakeMagnitude = Mathf.Max(0f, _shakeMagnitude - _shakeDecay * Time.deltaTime);
        }

        /// <summary>No-op: this camera intentionally ignores look input (see class docs). Kept so callers don't need branching.</summary>
        public void ApplyLook(Vector2 lookDelta, bool isTouch)
        {
        }

        /// <summary>Manually requests a Q/E-style rotation step; wire to UI buttons on touch if needed.</summary>
        public void RotateLeft() => _rotation?.RotateLeft();

        /// <summary>Manually requests a Q/E-style rotation step; wire to UI buttons on touch if needed.</summary>
        public void RotateRight() => _rotation?.RotateRight();

        public void SetTarget(Transform target)
        {
            _target = target;
        }

        /// <summary>Kept for compatibility with existing bootstrap code that wired the old PlayerCamera's pivot.</summary>
        public void SetPivot(Transform pivot) => SetTarget(pivot);
    }
}
