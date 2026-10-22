using System.Collections.Generic;
using Rapadura.Core.Events;
using Rapadura.Core.StateMachine;
using Rapadura.Gameplay.Combat;
using Rapadura.Gameplay.Enemies.States;
using UnityEngine;

namespace Rapadura.Gameplay.Enemies
{
    /// <summary>
    /// Basic enemy AI brain built on top of the generic <see cref="StateMachine{TContext}"/>, mirroring
    /// how <c>PlayerController</c> wires its own states. Moves via <see cref="CharacterController"/> when
    /// present, falling back to direct <see cref="Transform"/> translation otherwise, so it works without
    /// any NavMesh bake. Combat is delegated to the existing <see cref="Hitbox"/>/<see cref="Health"/>
    /// components; this class only drives movement/state decisions.
    /// </summary>
    [RequireComponent(typeof(Health))]
    public class EnemyController : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private EnemyDefinition _definition;

        [Header("Patrol")]
        [SerializeField] private Transform[] _waypoints;

        [Header("Target")]
        [Tooltip("Tag used to auto-discover a target if none is assigned explicitly (e.g. the player).")]
        [SerializeField] private string _targetTag = "Player";

        [Header("Attack")]
        [Tooltip("Optional hitbox used to deal damage. If absent, damage is applied directly to the target's ICombatTarget.")]
        [SerializeField] private Hitbox _hitbox;

        private StateMachine<EnemyController> _stateMachine;
        private CharacterController _characterController;
        private int _currentWaypointIndex;
        private bool _hasSpottedTarget;
        private float _aggroHoldRemaining;
        private bool _forceAggroWhenHoldEnds;
        private bool _aggroLocked;
        private bool _fleeExhausted;
        private int _spawnGeneration;

        private static readonly HashSet<EnemyController> Enabled = new HashSet<EnemyController>();

        /// <summary>Enemies currently enabled in any loaded scene (alive or dying). Read-only view for diagnostics (debug overlay).</summary>
        public static IReadOnlyCollection<EnemyController> EnabledEnemies => Enabled;

        /// <summary>Enabled enemies that are not dead.</summary>
        public static int CountAlive()
        {
            int alive = 0;
            foreach (EnemyController enemy in Enabled)
            {
                if (enemy != null && (enemy.Health == null || !enemy.Health.IsDead))
                {
                    alive++;
                }
            }

            return alive;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            Enabled.Clear();
        }

        /// <summary>
        /// Bumped by every <see cref="ResetForSpawn"/> and every <see cref="EnemyPool.Get"/> (see
        /// <see cref="MarkReused"/>). Lets effects that outlive a death (e.g. <see cref="EnemyDeathVfx"/>'s
        /// sink coroutine) notice that a pooled instance was handed out again and stop touching it.
        /// </summary>
        public int SpawnGeneration => _spawnGeneration;

        /// <summary>
        /// Invalidates anything still tracking this instance's previous life (bumps
        /// <see cref="SpawnGeneration"/>). Called by <see cref="EnemyPool.Get"/>, which can hand a corpse
        /// out without <see cref="ResetForSpawn"/>.
        /// </summary>
        public void MarkReused()
        {
            _spawnGeneration++;
        }

        public EnemyDefinition Definition => _definition;
        public Health Health { get; private set; }
        public Transform Target { get; set; }
        public Hitbox Hitbox => _hitbox;
        public StateMachine<EnemyController> StateMachine => _stateMachine;
        public bool IsBoss => _definition != null && _definition.IsBoss;

        /// <summary>
        /// True while <see cref="HoldAggro"/>'s grace is running: the AI stands still (no chase, no
        /// attacks) and only turns to face its target. Damage <i>to</i> the enemy is unaffected.
        /// </summary>
        public bool IsHoldingAggro => _aggroHoldRemaining > 0f;

        public float AggroHoldRemaining => _aggroHoldRemaining;

        /// <summary>
        /// Set by <see cref="ForceAggro"/> (encounter enemies): the AI always pursues its target - patrol
        /// re-acquires it regardless of <see cref="EnemyDefinition.DetectionRadius"/> and chase never
        /// drops it for <see cref="EnemyDefinition.LoseTargetRadius"/>. Cleared by <see cref="ResetForSpawn"/>.
        /// </summary>
        public bool IsAggroLocked => _aggroLocked;

        /// <summary>False once this life's flee was spent (tired or reached safety) - it then fights to the end.</summary>
        public bool CanFlee => !_fleeExhausted;

        /// <summary>Called by <see cref="EnemyFleeState"/> when a flee ends; see <see cref="CanFlee"/>.</summary>
        public void MarkFleeExhausted()
        {
            _fleeExhausted = true;
        }

        public Transform[] Waypoints => _waypoints;
        public int CurrentWaypointIndex
        {
            get => _currentWaypointIndex;
            set => _currentWaypointIndex = value;
        }

        private void Awake()
        {
            Health = GetComponent<Health>();
            _characterController = GetComponent<CharacterController>();
            BuildStateMachine();
        }

        private void OnEnable()
        {
            Enabled.Add(this);
            EventBus.Subscribe<CombatTargetDiedEvent>(OnCombatTargetDied);
        }

        private void OnDisable()
        {
            Enabled.Remove(this);
            EventBus.Unsubscribe<CombatTargetDiedEvent>(OnCombatTargetDied);
        }

        private void OnCombatTargetDied(CombatTargetDiedEvent evt)
        {
            if (evt.Target != gameObject)
            {
                return;
            }

            PublishDied();
        }

        private void Start()
        {
            if (_definition != null)
            {
                ApplyDefinitionToHealth();
            }

            if (Target == null && !string.IsNullOrEmpty(_targetTag))
            {
                GameObject found = GameObject.FindGameObjectWithTag(_targetTag);
                if (found != null)
                {
                    Target = found.transform;
                }
            }
        }

        private void Update()
        {
            TickBrain(Time.deltaTime);
        }

        private void FixedUpdate()
        {
            if ((Health != null && Health.IsDead) || IsHoldingAggro)
            {
                return;
            }

            _stateMachine.FixedTick(Time.fixedDeltaTime);
        }

        /// <summary>
        /// One AI step (what <c>Update</c> runs): nothing while dead; while an aggro hold is running it
        /// only counts the hold down and faces the target; otherwise ticks the state machine.
        /// </summary>
        public void TickBrain(float deltaTime)
        {
            if (Health != null && Health.IsDead)
            {
                return;
            }

            // Frozen (GameplayPause → timeScale 0 → deltaTime 0): no AI step at all. Ticking with 0
            // still ran state transitions, and EnemyAttackState.Enter zeroes its cooldown, so an enemy
            // reaching attack range hit the player behind a pausing hint/modal.
            if (deltaTime <= 0f)
            {
                return;
            }

            if (IsHoldingAggro)
            {
                _aggroHoldRemaining = Mathf.Max(0f, _aggroHoldRemaining - deltaTime);
                FaceTarget(deltaTime);
                if (_aggroHoldRemaining <= 0f && _forceAggroWhenHoldEnds)
                {
                    ForceAggro();
                }

                return;
            }

            _stateMachine.Tick(deltaTime);
        }

        /// <summary>
        /// Keeps the AI idle for <paramref name="seconds"/> (encounter intro / "Prepare-se!"): no chase
        /// and no attacks until it runs out. Never shortens a longer hold already running.
        /// </summary>
        /// <param name="forceAggroWhenHoldEnds">
        /// Encounters: once the grace ends (or <see cref="ReleaseAggro"/>), <see cref="ForceAggro"/> on the
        /// target - spiders/scorpions (7-8 m detection) spawned 8-9 m away otherwise never engaged.
        /// With <paramref name="seconds"/> &lt;= 0 (no grace) aggro is forced at once.
        /// </param>
        public void HoldAggro(float seconds, bool forceAggroWhenHoldEnds = false)
        {
            if (seconds > _aggroHoldRemaining)
            {
                _aggroHoldRemaining = seconds;
            }

            _forceAggroWhenHoldEnds |= forceAggroWhenHoldEnds;
            if (_forceAggroWhenHoldEnds && _aggroHoldRemaining <= 0f)
            {
                ForceAggro();
            }
        }

        /// <summary>Ends an <see cref="HoldAggro"/> grace early (forcing aggro if that hold asked for it).</summary>
        public void ReleaseAggro()
        {
            _aggroHoldRemaining = 0f;
            if (_forceAggroWhenHoldEnds)
            {
                ForceAggro();
            }
        }

        /// <summary>
        /// Engages the target right now regardless of detection radius and keeps chasing it
        /// (<see cref="IsAggroLocked"/>). Finds the target by tag when none is assigned; no-op when dead
        /// or no target exists.
        /// </summary>
        public void ForceAggro()
        {
            _forceAggroWhenHoldEnds = false;

            if (Health != null && Health.IsDead)
            {
                return;
            }

            if (Target == null && !string.IsNullOrEmpty(_targetTag))
            {
                GameObject found = GameObject.FindGameObjectWithTag(_targetTag);
                if (found != null)
                {
                    Target = found.transform;
                }
            }

            if (Target == null)
            {
                return;
            }

            _aggroLocked = true;
            _aggroHoldRemaining = 0f;
            MarkTargetSpotted();

            if (_stateMachine == null)
            {
                BuildStateMachine();
            }

            if (_stateMachine.IsInState<EnemyPatrolState>())
            {
                _stateMachine.ChangeState<EnemyChaseState>();
            }
        }

        private void FaceTarget(float deltaTime)
        {
            if (Target == null)
            {
                return;
            }

            Vector3 direction = Target.position - transform.position;
            direction.y = 0f;
            if (direction.sqrMagnitude < 0.0001f)
            {
                return;
            }

            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(direction, Vector3.up), _turnSpeedDegPerSec * deltaTime);
        }

        /// <summary>Resets transient AI state so a pooled instance behaves like a fresh spawn.</summary>
        public void ResetForSpawn(EnemyDefinition definition, Vector3 position, Transform[] waypoints = null)
        {
            _definition = definition;
            transform.position = position;
            if (waypoints != null)
            {
                _waypoints = waypoints;
            }

            _currentWaypointIndex = 0;
            _hasSpottedTarget = false;
            _aggroHoldRemaining = 0f;
            _forceAggroWhenHoldEnds = false;
            _aggroLocked = false;
            _fleeExhausted = false;
            _spawnGeneration++;

            if (Health == null)
            {
                Health = GetComponent<Health>();
            }

            ApplyDefinitionToHealth();
            if (Health != null)
            {
                // A reused corpse may still carry the post-hit immunity window from its last life.
                Health.ClearInvulnerability();
            }

            if (_stateMachine == null)
            {
                BuildStateMachine();
            }
            else
            {
                _stateMachine.ChangeState<EnemyPatrolState>();
            }
        }

        private void BuildStateMachine()
        {
            _stateMachine = new StateMachine<EnemyController>(this);
            _stateMachine.RegisterState(new EnemyPatrolState());
            _stateMachine.RegisterState(new EnemyChaseState());
            _stateMachine.RegisterState(new EnemyAttackState());
            _stateMachine.RegisterState(new EnemyFleeState());
            _stateMachine.ChangeState<EnemyPatrolState>();
        }

        /// <summary>
        /// Bridges data-driven config onto the existing <see cref="Health"/> component: the same
        /// shared prefab/controller can be reused for a tougher "boss" variant purely by swapping
        /// the <see cref="EnemyDefinition"/> asset, via <see cref="Health.ConfigureMaxHealth"/>.
        /// </summary>
        private void ApplyDefinitionToHealth()
        {
            if (_definition == null || Health == null)
            {
                return;
            }

            Health.ConfigureMaxHealth(_definition.MaxHealth);

            if (_hitbox != null)
            {
                _hitbox.Damage = _definition.AttackDamage;
            }
        }

        public void MarkTargetSpotted()
        {
            if (_hasSpottedTarget)
            {
                return;
            }

            _hasSpottedTarget = true;
            EventBus.Publish(new EnemyTargetSpottedEvent(gameObject, Target != null ? Target.gameObject : null));
        }

        public void MarkTargetLost()
        {
            if (!_hasSpottedTarget)
            {
                return;
            }

            _hasSpottedTarget = false;
            EventBus.Publish(new EnemyTargetLostEvent(gameObject));
        }

        [Header("Movement Feel")]
        [Tooltip("Degrees/sec the enemy turns to face its movement direction. Instant snapping (very high values) reads as janky/robotic, especially on rig-free mesh visuals.")]
        [SerializeField] private float _turnSpeedDegPerSec = 360f;

        /// <summary>Moves toward (or away from, with a negative-facing direction) a point on the XZ plane. No NavMesh required.</summary>
        public void MoveInDirection(Vector3 direction, float speed, float deltaTime)
        {
            if (direction == Vector3.zero || speed <= 0f)
            {
                return;
            }

            Vector3 delta = direction.normalized * speed * deltaTime;

            if (_characterController != null && _characterController.enabled)
            {
                _characterController.Move(delta + Physics.gravity * deltaTime);
            }
            else
            {
                transform.position += delta;
            }

            if (direction.sqrMagnitude > 0.0001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, _turnSpeedDegPerSec * deltaTime);
            }
        }

        public void ApplyDamageToTarget(float amount)
        {
            if (Target == null)
            {
                return;
            }

            ICombatTarget combatTarget = Target.GetComponentInParent<ICombatTarget>();
            if (combatTarget == null || !combatTarget.IsAlive)
            {
                return;
            }

            combatTarget.ApplyDamage(amount, Rapadura.Gameplay.Skills.ElementType.Physical, gameObject);
            EventBus.Publish(new EnemyAttackedEvent(gameObject, Target.gameObject, amount));
        }

        public void PublishFleeing()
        {
            EventBus.Publish(new EnemyFleeingEvent(gameObject));
        }

        public void PublishDied()
        {
            EventBus.Publish(new EnemyDiedEvent(gameObject, IsBoss));
        }
    }
}
