using System.Reflection;
using Rapadura.Core.Events;
using Rapadura.Core.Logging;
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

        public EnemyDefinition Definition => _definition;
        public Health Health { get; private set; }
        public Transform Target { get; set; }
        public Hitbox Hitbox => _hitbox;
        public StateMachine<EnemyController> StateMachine => _stateMachine;
        public bool IsBoss => _definition != null && _definition.IsBoss;

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
            EventBus.Subscribe<CombatTargetDiedEvent>(OnCombatTargetDied);
        }

        private void OnDisable()
        {
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
            if (Health != null && Health.IsDead)
            {
                return;
            }

            _stateMachine.Tick(Time.deltaTime);
        }

        private void FixedUpdate()
        {
            if (Health != null && Health.IsDead)
            {
                return;
            }

            _stateMachine.FixedTick(Time.fixedDeltaTime);
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

            if (Health == null)
            {
                Health = GetComponent<Health>();
            }

            ApplyDefinitionToHealth();

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
        /// Bridges data-driven config onto the existing <see cref="Health"/> component without modifying it:
        /// Health exposes no public setter for max health, so the same shared prefab/controller can still be
        /// reused for a tougher "boss" variant purely by swapping the <see cref="EnemyDefinition"/> asset.
        /// Fails silently (with a log) if reflection ever can't find the expected backing fields.
        /// </summary>
        private void ApplyDefinitionToHealth()
        {
            if (_definition == null || Health == null)
            {
                return;
            }

            try
            {
                FieldInfo maxHealthField = typeof(Health).GetField("_maxHealth", BindingFlags.NonPublic | BindingFlags.Instance);
                maxHealthField?.SetValue(Health, _definition.MaxHealth);

                FieldInfo currentHealthBackingField = typeof(Health).GetField("<CurrentHealth>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance);
                currentHealthBackingField?.SetValue(Health, _definition.MaxHealth);

                FieldInfo isDeadBackingField = typeof(Health).GetField("<IsDead>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance);
                isDeadBackingField?.SetValue(Health, false);
            }
            catch (System.Exception exception)
            {
                GameLogger.Exception("Enemy", exception);
            }

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
                transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
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
