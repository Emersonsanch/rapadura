using System;
using Rapadura.Core.Systems;
using Rapadura.Gameplay.Combat;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Rapadura.Gameplay.Player
{
    /// <summary>
    /// MU Online-style point-and-click movement: left-click/tap a point on the ground and the
    /// player walks/runs toward it, matching the fixed isometric camera already used by
    /// <c>Gameplay/Cameras/CameraController</c>. Purely an *input-to-intent* layer — it only
    /// decides where the player currently wants to go; <see cref="PlayerMotor.MoveToward"/> does
    /// the actual movement, and the Walk/Run states decide when to consult it (see
    /// <c>PlayerWalkState</c>/<c>PlayerRunState</c>).
    ///
    /// Manual movement (WASD/joystick) always takes priority and immediately cancels any pending
    /// click destination, so the two input styles never fight each other in the same frame.
    ///
    /// Hold-to-walk (MU-style): while the press that set the destination stays held
    /// (<see cref="PlayerInputHandler.IsMoveClickHeld"/>), the destination is re-aimed at the pointer
    /// every <see cref="HoldToWalkTimer.Interval"/> seconds (skipped while the pointer is over UI). A
    /// second world finger turns the gesture into a camera pinch and drops the destination that
    /// gesture's first finger had set.
    /// </summary>
    [RequireComponent(typeof(PlayerInputHandler))]
    public class PlayerClickToMove : MonoBehaviour
    {
        [Tooltip("Layers considered walkable ground for the click-to-move raycast. Defaults to everything so it works out of the box; narrow this to a dedicated Ground layer once one exists in the project.")]
        [SerializeField] private LayerMask _groundMask = ~0;
        [SerializeField] private float _maxRayDistance = 200f;

        [Tooltip("How close to walk when the click lands on an enemy, so the player stops at melee range instead of walking into it. Should roughly match PlayerBasicAttack's range.")]
        [SerializeField] private float _attackStoppingDistance = 2f;

        [Tooltip("Pursuing a clicked enemy farther than this (m) runs instead of walking, so a fleeing enemy can be caught.")]
        [SerializeField, Min(0f)] private float _autoRunDistance = DefaultAutoRunDistance;

        [Tooltip("A pursued enemy farther than this (m) is given up on. 0 = never.")]
        [SerializeField, Min(0f)] private float _maxPursuitDistance = 25f;

        [Tooltip("Hold-to-walk: seconds between re-aims of the destination while the pointer stays held on the ground.")]
        [SerializeField, Range(0.05f, 0.5f)] private float _holdReaimInterval = HoldToWalkTimer.DefaultInterval;

        /// <summary>Default <see cref="_autoRunDistance"/>: beyond ~4 m a walking (2.5 m/s) hero never closed on a fleeing enemy.</summary>
        public const float DefaultAutoRunDistance = 4f;

        /// <summary>How far past the stopping distance the target may drift before the pursuit destination is re-aimed.</summary>
        private const float RepursueSlack = 0.4f;

        private PlayerInputHandler _input;
        private Camera _camera;
        private HoldToWalkTimer _holdTimer;
        private PlayerMotor _motor;
        private readonly BlockedMoveDetector _blocked = new BlockedMoveDetector();

        /// <summary>
        /// True while pursuing <see cref="AttackTarget"/> from farther than <see cref="_autoRunDistance"/>:
        /// the walk state then moves at run speed (click-to-move otherwise always walks).
        /// </summary>
        public bool WantsAutoRun { get; private set; }

        /// <summary>Pure rule behind <see cref="WantsAutoRun"/> (EditMode-testable).</summary>
        public static bool ShouldAutoRun(float distanceToTarget, float autoRunDistance)
        {
            return autoRunDistance > 0f && distanceToTarget > autoRunDistance;
        }

        public bool HasDestination { get; private set; }
        public Vector3 Destination { get; private set; }

        /// <summary>
        /// Set when the click/tap landed on an <see cref="ICombatTarget"/> instead of empty
        /// ground — <see cref="PlayerBasicAttack"/> reads this to auto-attack once in range,
        /// instead of requiring a separate explicit attack input on top of the click (see TODO.md
        /// "clicar no inimigo = atacar direto"). Cleared whenever a new destination is set that
        /// isn't a combat target, or the destination is otherwise cleared.
        /// </summary>
        public ICombatTarget AttackTarget { get; private set; }

        /// <summary>Raised with the world-space point whenever a new click-to-move destination is set — a ground marker (see <see cref="ClickToMoveIndicator"/>) subscribes to show where the player is headed, since there was previously no feedback at all for where a click landed.</summary>
        public event Action<Vector3> DestinationSet;

        /// <summary>Raised when the destination is cleared (arrived, or overridden by manual movement).</summary>
        public event Action DestinationCleared;

        private void Awake()
        {
            _input = GetComponent<PlayerInputHandler>();
            _holdTimer = new HoldToWalkTimer(_holdReaimInterval);
            _motor = GetComponent<PlayerMotor>();
        }

        private void Update()
        {
            if (_input.MoveInput.sqrMagnitude > 0.01f)
            {
                // Manual movement always wins — abandon any pending click destination immediately.
                AbandonDestination();
                WantsAutoRun = false;
                _holdTimer.Tick(false, 0f);
                return;
            }

            if (_input.MoveHoldCancelledByPinchThisFrame)
            {
                // The first finger of a pinch landed as a tap-to-move: that wasn't a walk request.
                AbandonDestination();
            }

            TrackAttackTarget();
            TickBlockedDestination();

            if (_input.MoveClickPressedThisFrame)
            {
                _holdTimer.Restart();

                if (!IsPointerOverUi())
                {
                    TrySetDestinationFromScreenPoint(_input.PointerPosition);
                }

                return;
            }

            if (_holdTimer.Tick(_input.IsMoveClickHeld, Time.deltaTime)
                && !UiPointerQuery.IsPointerOverUi(_input.PointerPosition))
            {
                TrySetDestinationFromScreenPoint(_input.PointerPosition);
            }
        }

        /// <summary>
        /// Keeps pursuing a clicked enemy that moved (e.g. one fleeing at low HP): re-aims the
        /// destination at its stopping-distance point whenever it drifts out of reach, and flags
        /// <see cref="WantsAutoRun"/> while it is far. The click-time destination used to stay fixed,
        /// so the hero walked to where the enemy had been and stood there.
        /// </summary>
        private void TrackAttackTarget()
        {
            WantsAutoRun = false;
            ICombatTarget target = AttackTarget;

            if (target == null)
            {
                return;
            }

            if (target is UnityEngine.Object unityObject && unityObject == null)
            {
                // Destroyed (scene unload / despawn) — the interface reference outlives the object.
                AttackTarget = null;
                return;
            }

            if (!target.IsAlive)
            {
                // PlayerBasicAttack clears it (it also stops auto-attacking).
                return;
            }

            Vector3 toTarget = target.Transform.position - transform.position;
            toTarget.y = 0f;
            float distance = toTarget.magnitude;

            if (_maxPursuitDistance > 0f && distance > _maxPursuitDistance)
            {
                AbandonDestination();
                AttackTarget = null;
                return;
            }

            WantsAutoRun = ShouldAutoRun(distance, _autoRunDistance);

            if (distance > _attackStoppingDistance + RepursueSlack)
            {
                bool wasMoving = HasDestination;
                Destination = transform.position + toTarget.normalized * (distance - _attackStoppingDistance);
                HasDestination = true;
                if (!wasMoving)
                {
                    DestinationSet?.Invoke(Destination);
                }
            }
        }

        /// <summary>
        /// Gives up a ground destination the hero has been pushing into a solid prop (trunk, rock,
        /// bench) without getting closer — MU-style stop instead of running in place. Not while the
        /// pointer is held (hold-to-walk keeps re-aiming) or pursuing an enemy. See Docs/Collision.md.
        /// </summary>
        private void TickBlockedDestination()
        {
            if (!HasDestination || AttackTarget != null || _input.IsMoveClickHeld)
            {
                _blocked.Reset();
                return;
            }

            Vector3 toDestination = Destination - transform.position;
            toDestination.y = 0f;
            bool pushing = _motor != null && _motor.TargetHorizontalSpeed > 0.1f;
            if (_blocked.Tick(Destination, toDestination.magnitude, pushing, Time.deltaTime))
            {
                AbandonDestination();
            }
        }

        private void AbandonDestination()
        {
            if (!HasDestination)
            {
                return;
            }

            HasDestination = false;
            AttackTarget = null;
            DestinationCleared?.Invoke();
        }

        private bool IsPointerOverUi()
        {
            return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
        }

        private void TrySetDestinationFromScreenPoint(Vector2 screenPosition)
        {
            if (_camera == null)
            {
                _camera = Camera.main;
            }

            if (_camera == null)
            {
                return;
            }

            Ray ray = _camera.ScreenPointToRay(screenPosition);

            if (Physics.Raycast(ray, out RaycastHit hit, _maxRayDistance, _groundMask, QueryTriggerInteraction.Ignore))
            {
                ICombatTarget target = hit.collider.GetComponentInParent<ICombatTarget>();

                // Never target self (the raycast can hit the player's own collider) or anything
                // already dead (a corpse's collider may briefly linger before despawn/pooling).
                if (target != null && target.Transform == transform)
                {
                    target = null;
                }

                if (target != null && target.IsAlive)
                {
                    AttackTarget = target;

                    // Stop at melee range instead of walking into the enemy's collider — walk
                    // toward it along the ground-projected direction, stopping short.
                    Vector3 toTarget = target.Transform.position - transform.position;
                    toTarget.y = 0f;
                    float distance = toTarget.magnitude;

                    Destination = distance > _attackStoppingDistance
                        ? transform.position + toTarget.normalized * (distance - _attackStoppingDistance)
                        : transform.position;
                }
                else
                {
                    AttackTarget = null;
                    Destination = hit.point;
                }

                HasDestination = true;
                DestinationSet?.Invoke(Destination);
            }
        }

        /// <summary>
        /// Walks to <paramref name="destination"/> exactly as a tap on the ground there would (no attack target): the
        /// movement states take it from here. Used by scripted walks (the walkable-city PlayMode tests, guided paths).
        /// </summary>
        public void SetDestination(Vector3 destination)
        {
            AttackTarget = null;
            Destination = destination;
            HasDestination = true;
            DestinationSet?.Invoke(Destination);
        }

        /// <summary>
        /// Called by the movement states once the destination has been reached. Deliberately does
        /// NOT clear <see cref="AttackTarget"/> — arriving at melee range of a clicked enemy is
        /// the point where <see cref="PlayerBasicAttack"/> should take over and start attacking,
        /// not where the pursuit intent should be forgotten. Use <see cref="ClearAttackTarget"/>
        /// once the target is actually dealt with.
        /// </summary>
        public void ClearDestination()
        {
            if (!HasDestination)
            {
                return;
            }

            HasDestination = false;
            DestinationCleared?.Invoke();
        }

        /// <summary>Called by <see cref="PlayerBasicAttack"/> once the pursued target dies (or otherwise becomes invalid).</summary>
        public void ClearAttackTarget()
        {
            AttackTarget = null;
        }
    }
}
