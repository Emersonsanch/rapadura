using UnityEngine;

namespace Rapadura.Gameplay.Enemies
{
    /// <summary>
    /// Rig-free "animation" for enemy visuals (Wolf/Scorpion/Spider/etc.) — none of them have a
    /// working skeleton (Meshy's rigging API only supports bipeds, see TODO.md), so without this
    /// they just slide across the ground as a stiff static mesh, which reads as broken movement.
    /// Mirrors <see cref="Rapadura.Gameplay.Player.PlayerProceduralAnimator"/>'s bob/lean trick but
    /// computes speed from the parent's own position delta each frame instead of a motor component,
    /// since <see cref="EnemyController"/> has no equivalent velocity property.
    /// </summary>
    public class EnemyProceduralAnimator : MonoBehaviour
    {
        [SerializeField] private Transform _visualRoot;

        [Header("Locomotion Bob")]
        [SerializeField] private float _bobAmplitude = 0.12f;
        [SerializeField] private float _bobFrequency = 9f;
        [SerializeField] private float _leanDegrees = 14f;
        [SerializeField] private float _poseSmoothing = 12f;
        [SerializeField] private float _referenceSpeed = 4f;

        [Header("Locomotion Sway (multi-legged gait illusion)")]
        [Tooltip("Side-to-side hip sway, half the bob frequency so it reads as alternating left/right weight shift instead of a single up-down bounce.")]
        [SerializeField] private float _swayAmplitude = 0.05f;
        [Tooltip("Small yaw wag synced to the sway, like a creature's body twisting slightly with each stride.")]
        [SerializeField] private float _yawWagDegrees = 6f;

        private Transform _root;
        private Vector3 _restLocalPosition;
        private Quaternion _restLocalRotation;

        private Vector3 _lastRootPosition;
        private float _bobPhase;
        private float _currentSpeedFactor;

        private void Awake()
        {
            _root = transform.parent != null ? transform.parent : transform;

            if (_visualRoot == null)
            {
                _visualRoot = transform;
            }

            _restLocalPosition = _visualRoot.localPosition;
            _restLocalRotation = _visualRoot.localRotation;
            _lastRootPosition = _root.position;
        }

        private void Update()
        {
            float deltaTime = Time.deltaTime;
            if (deltaTime <= 0f)
            {
                return;
            }

            Vector3 rootDelta = _root.position - _lastRootPosition;
            rootDelta.y = 0f;
            float speed = rootDelta.magnitude / deltaTime;
            _lastRootPosition = _root.position;

            float targetSpeedFactor = Mathf.Clamp01(speed / _referenceSpeed);
            _currentSpeedFactor = Mathf.Lerp(_currentSpeedFactor, targetSpeedFactor, _poseSmoothing * deltaTime);

            if (speed > 0.1f)
            {
                _bobPhase += deltaTime * _bobFrequency * (0.5f + _currentSpeedFactor);
            }
            else
            {
                _bobPhase = Mathf.Lerp(_bobPhase, Mathf.Round(_bobPhase / Mathf.PI) * Mathf.PI, _poseSmoothing * deltaTime);
            }

            float bobOffset = Mathf.Sin(_bobPhase) * _bobAmplitude * (0.3f + _currentSpeedFactor);
            float leanAngle = _currentSpeedFactor * _leanDegrees;

            // Half-frequency sway/wag so left/right weight-shift reads as an alternating gait
            // (legs stepping in turn) rather than the whole body just bouncing straight up-down —
            // the single-axis bob alone reads as "hopping in place", not "walking".
            float swayPhase = _bobPhase * 0.5f;
            float swayOffset = Mathf.Sin(swayPhase) * _swayAmplitude * _currentSpeedFactor;
            float yawWag = Mathf.Sin(swayPhase) * _yawWagDegrees * _currentSpeedFactor;

            _visualRoot.localPosition = _restLocalPosition + Vector3.up * bobOffset + Vector3.right * swayOffset;
            _visualRoot.localRotation = _restLocalRotation * Quaternion.Euler(leanAngle, yawWag, 0f);
        }
    }
}
