using System.Collections.Generic;
using Rapadura.Core.Events;
using Rapadura.Core.StateMachine;
using Rapadura.Gameplay.Combat;
using Rapadura.Gameplay.Enemies.States;
using Rapadura.Gameplay.Skills;
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
    public class EnemyController : MonoBehaviour, IStunDurationModifier
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

        /// <summary>Bosses take <see cref="StunRules.BossStunDurationMultiplier"/> of every stun (anti stun-lock), others 1.</summary>
        public float StunDurationMultiplier => StunRules.MultiplierFor(IsBoss);

        /// <summary>True while a stun from a skill (Correntes das Trevas, Granada Elétrica, Terremoto) is running.</summary>
        public bool IsStunned
        {
            get
            {
                BuffController buffs = Buffs;
                return buffs != null && buffs.IsStunned;
            }
        }

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

            // Skill debuffs (slows, Defense/AttackDamage debuffs, DoTs) land through the target's
            // BuffController (SkillManager.ApplyToTarget) — no enemy prefab/scene object had one, so
            // every enemy-side debuff of every skill was silently dropped. Added at runtime so the
            // shared prefab and the scene don't need editing.
            if (!TryGetComponent(out _buffController))
            {
                _buffController = gameObject.AddComponent<BuffController>();
            }

            BuildStateMachine();
        }

        private BuffController _buffController;

        /// <summary>This enemy's BuffController (added in Awake; looked up lazily for EditMode tests). May be null.</summary>
        public BuffController Buffs
        {
            get
            {
                if (_buffController == null)
                {
                    _buffController = GetComponent<BuffController>();
                }

                return _buffController;
            }
        }

        /// <summary>Movement multiplier from MoveSpeed effects (slows, roots), 1 without a BuffController.</summary>
        public float MoveSpeedMultiplier
        {
            get
            {
                BuffController buffs = Buffs;
                return buffs != null ? buffs.GetMoveSpeedMultiplier() : 1f;
            }
        }

        /// <summary>
        /// Damage of one attack: <see cref="EnemyDefinition.AttackDamage"/> through this enemy's
        /// AttackDamage modifiers (Provocação Heroica -25%, Névoa da Ilusão -30%, Purificação -20%), never
        /// below 0. 0 without a definition.
        /// </summary>
        public float EffectiveAttackDamage
        {
            get
            {
                if (_definition == null)
                {
                    return 0f;
                }

                BuffController buffs = Buffs;
                float damage = buffs != null ? buffs.GetModifiedValue(StatType.AttackDamage, _definition.AttackDamage) : _definition.AttackDamage;
                return Mathf.Max(0f, damage);
            }
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
            if ((Health != null && Health.IsDead) || IsHoldingAggro || IsStunned)
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
                _velocity = Vector3.zero;
                CancelAttackWindup();
                return;
            }

            // Frozen (GameplayPause → timeScale 0 → deltaTime 0): no AI step at all. Ticking with 0
            // still ran state transitions, and EnemyAttackState.Enter zeroes its cooldown, so an enemy
            // reaching attack range hit the player behind a pausing hint/modal.
            if (deltaTime <= 0f)
            {
                return;
            }

            _movedThisTick = false;

            if (IsStunned)
            {
                // The swing cooldown is frozen while stunned: a stun costs the enemy time, it never
                // re-arms an early swing (see AttackCooldownRemaining).
                TickStunned();
                return;
            }

            _attackCooldownRemaining = Mathf.Max(0f, _attackCooldownRemaining - deltaTime);

            if (IsHoldingAggro)
            {
                _aggroHoldRemaining = Mathf.Max(0f, _aggroHoldRemaining - deltaTime);
                Coast(deltaTime, 1f);
                FaceTarget(deltaTime);
                if (_aggroHoldRemaining <= 0f && _forceAggroWhenHoldEnds)
                {
                    ForceAggro();
                }

                return;
            }

            _stateMachine.Tick(deltaTime);

            // Patrol pauses, state changes etc.: nobody drove the enemy this tick, so it decelerates
            // instead of stopping dead (acceleration/deceleration smoothing, see MoveInDirection).
            if (!_movedThisTick)
            {
                Coast(deltaTime, 1f);
            }
        }

        /// <summary>
        /// Stunned step: no movement (hard stop, no coasting), no turning, no state tick (so no attack).
        /// A windup in progress is cancelled and the attack stance dropped, so once the stun ends the
        /// enemy re-enters attack range through <see cref="EnemyAttackState.Enter"/> and telegraphs the
        /// whole windup again instead of striking instantly with the time left over.
        /// </summary>
        private void TickStunned()
        {
            _velocity = Vector3.zero;
            _yawVelocity = 0f;
            _movedThisTick = true;
            CancelAttackWindup();

            if (_stateMachine != null && _stateMachine.IsInState<EnemyAttackState>())
            {
                _stateMachine.ChangeState<EnemyChaseState>();
            }
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
            _velocity = Vector3.zero;
            _yawVelocity = 0f;
            _attackCooldownRemaining = 0f;
            CancelAttackWindup();

            if (Health == null)
            {
                Health = GetComponent<Health>();
            }

            // A pooled instance must not carry the last life's slows/debuffs/DoTs into the new one.
            BuffController buffs = Buffs;
            if (buffs != null)
            {
                buffs.ClearAllEffects();
                buffs.ClearStun();
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
            _alertSequence++;
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
        [Tooltip("Scales the archetype's max turn speed (EnemyMotionProfile.MaxTurnSpeed); 360 = as tuned. Instant snapping (very high values) reads as janky/robotic, especially on rig-free mesh visuals.")]
        [SerializeField] private float _turnSpeedDegPerSec = 360f;

        /// <summary>Braking multiplier on the archetype deceleration while planted for an attack.</summary>
        private const float AttackBrakeMultiplier = 2.5f;

        private Vector3 _velocity;
        private float _yawVelocity;
        private bool _movedThisTick;
        private EnemyMotionProfile _motion = EnemyMotionProfile.Quadruped;
        private EnemyDefinition _motionResolvedFor;
        private bool _motionResolved;

        private int _attackSequence;
        private int _strikeSequence;
        private int _attackCancelSequence;
        private int _alertSequence;
        private bool _isWindingUp;
        private float _attackWindupDuration;
        private float _attackCooldownRemaining;

        /// <summary>Planar movement velocity (m/s) after acceleration/deceleration smoothing.</summary>
        public Vector3 Velocity => _velocity;

        /// <summary>Tuning for this enemy's archetype (<see cref="EnemyDefinition.MotionArchetype"/>), cached per definition.</summary>
        public EnemyMotionProfile Motion
        {
            get
            {
                if (!_motionResolved || !ReferenceEquals(_motionResolvedFor, _definition))
                {
                    _motion = EnemyMotionProfile.For(EnemyMotionArchetypes.Resolve(_definition));
                    _motionResolvedFor = _definition;
                    _motionResolved = true;
                }

                return _motion;
            }
        }

        /// <summary>Windup used by <see cref="EnemyAttackState"/> for this enemy (≥ <see cref="EnemyMotionMath.MinTelegraph"/>).</summary>
        public float ResolvedAttackWindup => EnemyMotionMath.ResolveWindup(Motion.BaseWindup, _definition != null ? _definition.AttackCooldown : 0f);

        // Animation hand-off counters: EnemyProceduralAnimator polls these (no delegates/allocations,
        // and a pooled/disabled animator can never miss-subscribe). Each bump = one event.
        public int AttackSequence => _attackSequence;
        public int StrikeSequence => _strikeSequence;
        public int AttackCancelSequence => _attackCancelSequence;
        public int AlertSequence => _alertSequence;
        public bool IsWindingUp => _isWindingUp;
        public float AttackWindupDuration => _attackWindupDuration;

        /// <summary>
        /// Seconds before this enemy may strike again, counted down by <see cref="TickBrain"/> except while
        /// stunned. Set at every strike to max(<see cref="EnemyDefinition.AttackCooldown"/>, windup) and read by
        /// <see cref="EnemyAttackState.Enter"/>: re-entering attack range (after a stun, a dodge or a chase)
        /// no longer re-arms the swing after just one windup. Before r8 a stun dropped the enemy out of the
        /// attack state and the next swing came one windup after the stun ended, so stunning a boss right
        /// after its swing (2.4-3.3 s cooldown, 0.3-0.5 s stun) made it hit back sooner (Docs/Balance/README.md §9).
        /// </summary>
        public float AttackCooldownRemaining => _attackCooldownRemaining;

        /// <summary>Called by <see cref="EnemyAttackState"/> when a telegraphed windup starts; damage lands <paramref name="duration"/> s later.</summary>
        public void NotifyAttackWindup(float duration)
        {
            _isWindingUp = true;
            _attackWindupDuration = duration;
            _attackSequence++;
        }

        /// <summary>Called by <see cref="EnemyAttackState"/> at the damage moment.</summary>
        public void NotifyAttackStrike()
        {
            _isWindingUp = false;
            _strikeSequence++;
            _attackCooldownRemaining = Mathf.Max(_definition != null ? _definition.AttackCooldown : 0f, ResolvedAttackWindup);
        }

        /// <summary>A windup that never struck (target left reach, state change, death, re-spawn).</summary>
        public void CancelAttackWindup()
        {
            if (!_isWindingUp)
            {
                return;
            }

            _isWindingUp = false;
            _attackCancelSequence++;
        }

        /// <summary>
        /// Moves toward (or away from, with a negative-facing direction) a point on the XZ plane. No NavMesh required.
        /// <paramref name="speed"/> is the state's base speed (patrol/chase/flee); it is scaled here by
        /// <see cref="MoveSpeedMultiplier"/> so every state honours slows/roots. The actual velocity eases
        /// toward that target with the archetype's acceleration/deceleration (never above the target
        /// speed, so the flee cap holds), and heading turns with a critically damped yaw (no snapping or
        /// jitter when the direction flips). A rooted enemy stops dead but still turns. EnemyProceduralAnimator
        /// measures real displacement, so the gait follows the smoothed speed on its own.
        /// </summary>
        public void MoveInDirection(Vector3 direction, float speed, float deltaTime)
        {
            if (direction == Vector3.zero || speed <= 0f || deltaTime <= 0f)
            {
                return;
            }

            _movedThisTick = true;
            direction.y = 0f;
            if (direction.sqrMagnitude < 0.000001f)
            {
                return;
            }

            float multiplier = MoveSpeedMultiplier;
            Vector3 desired = direction.normalized * (speed * multiplier);
            if (multiplier <= 0f)
            {
                _velocity = Vector3.zero; // rooted: hard stop, no coasting through the root
            }
            else
            {
                EnemyMotionProfile motion = Motion;
                float rate = desired.sqrMagnitude >= _velocity.sqrMagnitude ? motion.Acceleration : motion.Deceleration;
                _velocity = Vector3.MoveTowards(_velocity, desired, rate * deltaTime);
            }

            Displace(_velocity * deltaTime, deltaTime);
            TurnTowards(direction, deltaTime);
        }

        /// <summary>Brakes hard (attack stance) instead of coasting; marks the enemy as handled this tick.</summary>
        public void StopMoving(float deltaTime)
        {
            _movedThisTick = true;
            Coast(deltaTime, AttackBrakeMultiplier);
        }

        /// <summary>Chase speed capped so the enemy eases into attack range instead of skidding through it.</summary>
        public float GetArriveSpeed(float speed, float remainingDistance)
        {
            return EnemyMotionMath.ArriveSpeed(speed, remainingDistance, Motion.Deceleration);
        }

        /// <summary>Smoothly turns to face the target (attack stance / aggro hold).</summary>
        public void FaceTarget(float deltaTime)
        {
            if (Target == null)
            {
                return;
            }

            TurnTowards(Target.position - transform.position, deltaTime);
        }

        private void TurnTowards(Vector3 direction, float deltaTime)
        {
            direction.y = 0f;
            if (direction.sqrMagnitude < 0.0001f || deltaTime <= 0f)
            {
                return;
            }

            EnemyMotionProfile motion = Motion;
            float maxSpeed = motion.MaxTurnSpeed * Mathf.Max(0.1f, _turnSpeedDegPerSec / 360f);
            float targetYaw = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            float yaw = transform.eulerAngles.y;
            float newYaw = Mathf.SmoothDampAngle(yaw, targetYaw, ref _yawVelocity, motion.TurnSmoothTime, maxSpeed, deltaTime);
            transform.rotation = Quaternion.Euler(0f, newYaw, 0f);
        }

        /// <summary>No state moved the enemy this tick: decelerate and drift to a stop.</summary>
        private void Coast(float deltaTime, float brakeMultiplier)
        {
            if (_velocity.sqrMagnitude < 0.000001f || deltaTime <= 0f)
            {
                _velocity = Vector3.zero;
                return;
            }

            if (MoveSpeedMultiplier <= 0f)
            {
                _velocity = Vector3.zero;
                return;
            }

            _velocity = Vector3.MoveTowards(_velocity, Vector3.zero, Motion.Deceleration * brakeMultiplier * deltaTime);
            Displace(_velocity * deltaTime, deltaTime);
        }

        private void Displace(Vector3 delta, float deltaTime)
        {
            if (_characterController != null && _characterController.enabled)
            {
                _characterController.Move(delta + Physics.gravity * deltaTime);
            }
            else
            {
                transform.position += delta;
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

            // Every enemy attack is melee (no ranged enemies yet): a melee-aware target (the hero) can
            // reflect Thorns back at this enemy. A future ranged attack must use plain ApplyDamage.
            if (combatTarget is IMeleeDamageTarget meleeTarget)
            {
                meleeTarget.ApplyMeleeDamage(amount, Rapadura.Gameplay.Skills.ElementType.Physical, gameObject);
            }
            else
            {
                combatTarget.ApplyDamage(amount, Rapadura.Gameplay.Skills.ElementType.Physical, gameObject);
            }

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
