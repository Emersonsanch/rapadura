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
        [SerializeField] private float _bobAmplitude = 0.22f;
        [SerializeField] private float _bobFrequency = 9f;
        [SerializeField] private float _leanDegrees = 22f;
        [SerializeField] private float _poseSmoothing = 12f;
        [Tooltip("Speed (m/s) at which the bob/lean reaches full intensity. Lowered from 4 so patrol/wander speeds (~1.6-2.5 m/s for most enemies) already read as clearly animated, not just chase/attack-range speeds.")]
        [SerializeField] private float _referenceSpeed = 2f;

        [Header("Locomotion Sway (multi-legged gait illusion)")]
        [Tooltip("Side-to-side hip sway, half the bob frequency so it reads as alternating left/right weight shift instead of a single up-down bounce.")]
        [SerializeField] private float _swayAmplitude = 0.09f;
        [Tooltip("Small yaw wag synced to the sway, like a creature's body twisting slightly with each stride.")]
        [SerializeField] private float _yawWagDegrees = 10f;

        private Transform _root;
        private Vector3 _restLocalPosition;
        private Quaternion _restLocalRotation;
        private bool _hasRestPose;

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
            _hasRestPose = true;
            _lastRootPosition = _root.position;
        }

        public Transform VisualRoot => _visualRoot;

        private void Update()
        {
            Tick(Time.deltaTime);
        }

        /// <summary>
        /// Re-targets which transform receives the bob/lean/sway — used by <see cref="EnemyVisualSwapper"/>
        /// after replacing the enemy's visual. Recaptures the rest pose from the new root's current
        /// local transform; the outgoing visual is first put back at its own rest pose so it isn't
        /// left mid-bob (it may be re-activated later, e.g. a pooled enemy going back to the shared
        /// prefab visual). Null falls back to this transform (same as an unassigned field in Awake).
        /// </summary>
        public void SetVisualRoot(Transform visualRoot)
        {
            if (_root == null)
            {
                _root = transform.parent != null ? transform.parent : transform;
                _lastRootPosition = _root.position;
            }

            Transform next = visualRoot != null ? visualRoot : transform;

            if (_hasRestPose && _visualRoot != null && _visualRoot != next)
            {
                _visualRoot.localPosition = _restLocalPosition;
                _visualRoot.localRotation = _restLocalRotation;
            }

            _visualRoot = next;
            _restLocalPosition = _visualRoot.localPosition;
            _restLocalRotation = _visualRoot.localRotation;
            _hasRestPose = true;
        }

        /// <summary>
        /// Core per-frame update, split out from <see cref="Update"/> so it can be driven with an
        /// explicit deltaTime — both for EditMode/headless tests (this environment's
        /// <see cref="Time.deltaTime"/> stays frozen at 0 outside a real focused Play Mode frame
        /// tick, which made this component untestable/unverifiable any other way) and in case
        /// something other than Unity's own frame loop ever needs to drive it deterministically.
        /// </summary>
        public void Tick(float deltaTime)
        {
            if (deltaTime <= 0f || _visualRoot == null || _root == null)
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
