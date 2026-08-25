using Rapadura.Core.Events;
using Rapadura.Gameplay.Combat;
using UnityEngine;

namespace Rapadura.Gameplay.Player
{
    /// <summary>
    /// Rig-free "animation" for the placeholder capsule visual: since there is no skinned/rigged
    /// character model yet (see Fase 11 — Arte no TODO), this drives believable motion purely
    /// through code on a cosmetic child transform (<see cref="_visualRoot"/>), completely separate
    /// from the actual gameplay root that <see cref="CharacterController"/>/collision depend on.
    ///
    /// Covers three things procedurally:
    /// <list type="bullet">
    /// <item>Locomotion: a speed-scaled vertical bob + forward/side lean while walking/running.</item>
    /// <item>Attack: a quick forward lunge + squash-stretch on <see cref="PlayerAttackPerformedEvent"/>.</item>
    /// <item>Hit reaction: a recoil shake + emissive red flash on <see cref="DamageAppliedEvent"/>.</item>
    /// </list>
    /// All three are independent decaying timers composed together each frame, so they can overlap
    /// (e.g. getting hit mid-swing) without fighting each other. Once a real rig exists, this
    /// component can be removed in favor of Animator blend trees driven by the same PlayerMotor
    /// velocity/state data.
    /// </summary>
    public class PlayerProceduralAnimator : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Cosmetic child transform to animate. Never the Player root (that's owned by CharacterController).")]
        [SerializeField] private Transform _visualRoot;
        [SerializeField] private PlayerMotor _motor;
        [SerializeField] private Renderer _renderer;

        [Header("Locomotion Bob")]
        [SerializeField] private float _bobAmplitude = 0.06f;
        [SerializeField] private float _bobFrequency = 9f;
        [SerializeField] private float _leanDegrees = 8f;
        [SerializeField] private float _poseSmoothing = 12f;

        [Header("Attack Swing")]
        [SerializeField] private float _attackLungeDistance = 0.3f;
        [SerializeField] private float _attackSquash = 0.15f;
        [SerializeField] private float _attackDuration = 0.18f;
        [SerializeField] private AnimationCurve _attackCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        [Header("Hit Reaction")]
        [SerializeField] private float _hitShakeMagnitude = 0.1f;
        [SerializeField] private float _hitShakeDuration = 0.25f;
        [SerializeField] private float _hitFlashDuration = 0.2f;
        [SerializeField] private Color _hitFlashColor = new Color(1f, 0.15f, 0.15f);

        private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

        private Vector3 _restLocalPosition;
        private Quaternion _restLocalRotation;
        private Vector3 _restLocalScale;

        private float _bobPhase;
        private float _currentSpeedFactor;

        private float _attackTimer = -1f;
        private Vector3 _attackDirectionLocal = Vector3.forward;

        private float _hitShakeTimer = -1f;
        private float _hitFlashTimer = -1f;
        private Vector3 _hitShakeSeed;

        private MaterialPropertyBlock _propertyBlock;

        private void Awake()
        {
            if (_visualRoot == null)
            {
                Debug.LogWarning($"[PlayerProceduralAnimator] No visual root assigned on {name}; procedural animation disabled.", this);
                enabled = false;
                return;
            }

            if (_motor == null)
            {
                _motor = GetComponentInParent<PlayerMotor>();
            }

            if (_renderer == null)
            {
                _renderer = _visualRoot.GetComponentInChildren<Renderer>();
            }

            _restLocalPosition = _visualRoot.localPosition;
            _restLocalRotation = _visualRoot.localRotation;
            _restLocalScale = _visualRoot.localScale;

            _propertyBlock = new MaterialPropertyBlock();
        }

        /// <summary>Re-targets which transform receives the bob/lean/attack/hit motion — used by <see cref="PlayerVisualSwapper"/> after swapping the visual model. Recaptures the rest pose from the new root's current local transform.</summary>
        public void SetVisualRoot(Transform visualRoot)
        {
            _visualRoot = visualRoot;
            _renderer = visualRoot != null ? visualRoot.GetComponentInChildren<Renderer>() : null;

            if (_visualRoot != null)
            {
                _restLocalPosition = _visualRoot.localPosition;
                _restLocalRotation = _visualRoot.localRotation;
                _restLocalScale = _visualRoot.localScale;
            }
        }

        private void OnEnable()
        {
            EventBus.Subscribe<DamageAppliedEvent>(OnDamageApplied);
            EventBus.Subscribe<PlayerAttackPerformedEvent>(OnAttackPerformed);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<DamageAppliedEvent>(OnDamageApplied);
            EventBus.Unsubscribe<PlayerAttackPerformedEvent>(OnAttackPerformed);
        }

        private void OnDamageApplied(DamageAppliedEvent evt)
        {
            if (evt.Target != gameObject)
            {
                return;
            }

            _hitShakeTimer = _hitShakeDuration;
            _hitFlashTimer = _hitFlashDuration;
            _hitShakeSeed = new Vector3(Random.Range(-1f, 1f), Random.Range(-0.3f, 0.3f), Random.Range(-1f, 1f)).normalized;
        }

        private void OnAttackPerformed(PlayerAttackPerformedEvent evt)
        {
            if (evt.Attacker != gameObject)
            {
                return;
            }

            _attackTimer = _attackDuration;
        }

        private void Update()
        {
            TickLocomotionBob();
            TickAttackSwing();
            TickHitReaction();
            ComposeVisualPose();
        }

        private void TickLocomotionBob()
        {
            float speed = _motor != null ? _motor.HorizontalVelocity.magnitude : 0f;
            float targetSpeedFactor = Mathf.Clamp01(speed / 5.5f);
            _currentSpeedFactor = Mathf.Lerp(_currentSpeedFactor, targetSpeedFactor, _poseSmoothing * Time.deltaTime);

            bool isMoving = _motor != null && _motor.IsGrounded && speed > 0.15f;

            if (isMoving)
            {
                _bobPhase += Time.deltaTime * _bobFrequency * (0.5f + _currentSpeedFactor);
            }
            else
            {
                // Ease the bob phase toward the nearest rest point instead of snapping, so stopping
                // mid-stride doesn't visibly pop the capsule back to dead-center.
                _bobPhase = Mathf.Lerp(_bobPhase, Mathf.Round(_bobPhase / Mathf.PI) * Mathf.PI, _poseSmoothing * Time.deltaTime);
            }
        }

        private void TickAttackSwing()
        {
            if (_attackTimer < 0f)
            {
                return;
            }

            _attackTimer -= Time.deltaTime;

            if (_attackTimer < 0f)
            {
                _attackTimer = -1f;
            }
        }

        private void TickHitReaction()
        {
            if (_hitShakeTimer >= 0f)
            {
                _hitShakeTimer -= Time.deltaTime;

                if (_hitShakeTimer < 0f)
                {
                    _hitShakeTimer = -1f;
                }
            }

            if (_hitFlashTimer >= 0f)
            {
                _hitFlashTimer -= Time.deltaTime;

                if (_hitFlashTimer < 0f)
                {
                    _hitFlashTimer = -1f;
                }
            }

            ApplyHitFlash();
        }

        private void ComposeVisualPose()
        {
            float bobOffset = Mathf.Sin(_bobPhase) * _bobAmplitude * (0.3f + _currentSpeedFactor);
            float leanAngle = _currentSpeedFactor * _leanDegrees;

            Vector3 position = _restLocalPosition + Vector3.up * bobOffset;
            Quaternion rotation = _restLocalRotation * Quaternion.Euler(leanAngle, 0f, 0f);
            Vector3 scale = _restLocalScale;

            if (_attackTimer >= 0f)
            {
                float progress = 1f - Mathf.Clamp01(_attackTimer / _attackDuration);
                float curveValue = _attackCurve.Evaluate(progress);
                // Lunge out and back (triangle wave over the curve), squash-stretch peaking mid-swing.
                float lunge = Mathf.Sin(curveValue * Mathf.PI) * _attackLungeDistance;
                float squash = Mathf.Sin(curveValue * Mathf.PI) * _attackSquash;

                position += _attackDirectionLocal * lunge;
                scale = new Vector3(_restLocalScale.x * (1f + squash), _restLocalScale.y * (1f - squash * 0.6f), _restLocalScale.z * (1f + squash));
            }

            if (_hitShakeTimer >= 0f)
            {
                float shakeProgress = _hitShakeTimer / _hitShakeDuration;
                position += _hitShakeSeed * (_hitShakeMagnitude * shakeProgress);
            }

            _visualRoot.localPosition = position;
            _visualRoot.localRotation = rotation;
            _visualRoot.localScale = scale;
        }

        private void ApplyHitFlash()
        {
            if (_renderer == null)
            {
                return;
            }

            if (_hitFlashTimer < 0f)
            {
                _renderer.SetPropertyBlock(null);
                return;
            }

            float intensity = _hitFlashTimer / _hitFlashDuration;
            _propertyBlock.Clear();
            _renderer.GetPropertyBlock(_propertyBlock);
            _propertyBlock.SetColor(EmissionColorId, _hitFlashColor * intensity);
            _renderer.SetPropertyBlock(_propertyBlock);
        }
    }
}
