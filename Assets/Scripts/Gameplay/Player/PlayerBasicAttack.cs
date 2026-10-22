using System;
using System.Collections.Generic;
using Rapadura.Core.Events;
using Rapadura.Gameplay.Combat;
using Rapadura.Gameplay.Skills;
using UnityEngine;

namespace Rapadura.Gameplay.Player
{
    /// <summary>
    /// Minimal placeholder attack so the player has a way to fight back in the test scene: on
    /// <see cref="PlayerInputHandler.AttackPressedThisFrame"/>, deals flat damage to every
    /// <see cref="ICombatTarget"/> caught by an <see cref="Physics.OverlapSphere"/> in front of the
    /// player, on a cooldown. This is intentionally simple (no weapon/animation) — a real melee/ranged
    /// weapon (<c>Gameplay/Combat/Weapons/*</c>) can replace this once the player has an equipped item
    /// and animation timing to drive it from.
    /// </summary>
    [RequireComponent(typeof(PlayerInputHandler))]
    public class PlayerBasicAttack : MonoBehaviour
    {
        [Header("Attack")]
        [Tooltip("Fallback only: the shipped Resources/Balance/PlayerCombatBalance asset's Basic Attack Damage wins when it exists (see Docs/Balance/README.md).")]
        [SerializeField] private float _damage = 15f;
        [SerializeField] private float _range = 2.5f;
        [SerializeField] private float _radius = 1.2f;
        [SerializeField] private float _cooldownSeconds = 0.5f;
        [SerializeField] private LayerMask _targetMask = ~0;

        [Header("Balance")]
        [Tooltip("Base damage and crit tuning. Left empty, the shipped Resources/Balance/PlayerCombatBalance asset is used.")]
        [SerializeField] private PlayerCombatBalance _balance;

        [Header("Click-to-Attack")]
        [Tooltip("Degrees/sec the player turns to face a pursued enemy before/while auto-attacking it. Instant snapping reads as janky, same reasoning as EnemyController's turn speed.")]
        [SerializeField] private float _turnSpeedDegPerSec = 540f;

        [Header("Timing")]
        [Tooltip("Seconds between the swing starting and the damage landing, when the hero has no attack clip. With clips, the Animator's baked AttackStrikeTime is used so the hit lands on the fastest frame of the swing.")]
        [SerializeField, Min(0f)] private float _strikeDelayFallback = HeroActionTiming.StrikeDelay;

        private PlayerInputHandler _input;
        private PlayerClickToMove _clickToMove;
        private PlayerActionAnimator _actions;
        private PlayerStats _stats;
        private ICombatTarget _self;
        private float _cooldownRemaining;
        private float _pendingStrike = -1f;
        private readonly Collider[] _hitBuffer = new Collider[16];
        private bool _balanceLookupDone;

        /// <summary>Uniform [0, 1] sampler for crit rolls. Null = Unity's global RNG; tests inject a fixed value.</summary>
        public Func<float> CriticalRoll { get; set; }

        private void Awake()
        {
            _input = GetComponent<PlayerInputHandler>();
            _clickToMove = GetComponent<PlayerClickToMove>();
            _actions = GetComponent<PlayerActionAnimator>();
            _stats = GetComponent<PlayerStats>();
            _self = GetComponent<ICombatTarget>();
        }

        /// <summary>Seconds from the swing starting to the hit landing (the clip's fastest-hand frame when there is one).</summary>
        public float StrikeDelay
        {
            get
            {
                if (_actions == null)
                {
                    _actions = GetComponent<PlayerActionAnimator>();
                }

                return _actions != null && _actions.HasClipActions ? _actions.AttackStrikeDelay : _strikeDelayFallback;
            }
        }

        /// <summary>A swing has started and its damage has not landed yet.</summary>
        public bool IsStrikePending => _pendingStrike >= 0f;

        private PlayerCombatBalance Balance
        {
            get
            {
                if (_balance == null && !_balanceLookupDone)
                {
                    _balanceLookupDone = true;
                    _balance = PlayerCombatBalance.LoadDefault();
                }

                return _balance;
            }
        }

        /// <summary>Base damage per hit before buffs/level/crit: the balance asset's value, else the inspector field.</summary>
        public float BaseDamage => Balance != null ? Balance.BasicAttackDamage : _damage;

        /// <summary>
        /// Damage one basic-attack hit deals right now, through the same pipeline as skills
        /// (<see cref="OutgoingDamage.Resolve"/>): level damage multiplier from the progression curve,
        /// AttackDamage modifiers (Poder Arcano, Berserk, combos, affixes) and a crit roll (Destreza).
        /// Previously a flat 15 that ignored all of them.
        /// </summary>
        public float ComputeHitDamage(out bool isCritical)
        {
            var stats = GetComponent<PlayerStats>();
            float levelMultiplier = stats != null && stats.Curve != null ? stats.Curve.GetDamageMultiplier(stats.Level) : 1f;
            return OutgoingDamage.Resolve(BaseDamage, levelMultiplier, GetComponent<BuffController>(), Balance, CriticalRoll, out isCritical);
        }

        private void Update()
        {
            if (_cooldownRemaining > 0f)
            {
                _cooldownRemaining -= Time.deltaTime;
            }

            TickPendingStrike(Time.deltaTime);

            if (_input.AttackPressedThisFrame && _cooldownRemaining <= 0f)
            {
                PerformAttack();
                _cooldownRemaining = _cooldownSeconds;
                return;
            }

            TickClickToAttack();
        }

        /// <summary>
        /// While the player is pursuing an enemy clicked via <see cref="PlayerClickToMove"/> (see
        /// TODO.md "clicar no inimigo = atacar direto"), once close enough, auto-faces and attacks
        /// it on cooldown without requiring a separate explicit attack input — the click itself
        /// already expressed "fight this enemy". Does nothing if no target is being pursued, or a
        /// manual attack already fired this frame (handled above).
        /// </summary>
        private void TickClickToAttack()
        {
            if (_clickToMove == null)
            {
                return;
            }

            ICombatTarget target = _clickToMove.AttackTarget;

            if (target == null)
            {
                return;
            }

            if (!target.IsAlive)
            {
                _clickToMove.ClearAttackTarget();
                return;
            }

            Vector3 toTarget = target.Transform.position - transform.position;
            toTarget.y = 0f;

            if (toTarget.sqrMagnitude > _range * _range)
            {
                // Still out of range — PlayerMotor.MoveToward (driven by the walk/run states) is
                // already carrying the player toward AttackTarget's stopping-distance destination;
                // nothing to do here until that closes the gap.
                return;
            }

            if (toTarget.sqrMagnitude > 0.0001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(toTarget, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, _turnSpeedDegPerSec * Time.deltaTime);
            }

            if (_cooldownRemaining <= 0f)
            {
                PerformAttack();
                _cooldownRemaining = _cooldownSeconds;
            }
        }

        /// <summary>
        /// Starts a swing: the animation (and everyone listening to the event) begins now, the
        /// damage lands <see cref="StrikeDelay"/> seconds later in <see cref="TickPendingStrike"/>,
        /// on the frame the swing is fastest. A swing in flight when the hero dies is dropped.
        /// </summary>
        private void PerformAttack()
        {
            // Read the delay before the event: PlayerActionAnimator alternates the swing variant
            // when it handles the event, and the delay belongs to the variant about to play.
            float delay = StrikeDelay;
            EventBus.Publish(new PlayerAttackPerformedEvent(gameObject));
            if (delay <= 0f)
            {
                _pendingStrike = -1f;
                LandStrike();
                return;
            }

            _pendingStrike = delay;
        }

        private void TickPendingStrike(float deltaTime)
        {
            if (_pendingStrike < 0f)
            {
                return;
            }

            if (_stats != null && _stats.IsDead)
            {
                _pendingStrike = -1f;
                return;
            }

            _pendingStrike -= deltaTime;
            if (_pendingStrike <= 0f)
            {
                _pendingStrike = -1f;
                LandStrike();
            }
        }

        /// <summary>Applies the swing's damage in front of the hero (its facing at this moment, so a turn during the windup is honoured).</summary>
        private void LandStrike()
        {
            Vector3 origin = transform.position + Vector3.up + transform.forward * (_range * 0.5f);
            int hitCount = Physics.OverlapSphereNonAlloc(origin, _radius, _hitBuffer, _targetMask, QueryTriggerInteraction.Collide);

            var alreadyHit = new HashSet<ICombatTarget>();
            // One roll per swing: every target caught by the same swing shares the crit.
            float damage = ComputeHitDamage(out bool isCritical);

            for (int i = 0; i < hitCount; i++)
            {
                var target = _hitBuffer[i].GetComponentInParent<ICombatTarget>();
                if (target == null || target == _self || !alreadyHit.Add(target))
                {
                    continue;
                }

                OutgoingDamage.DeliverDamage(target, damage, ElementType.Physical, gameObject, isCritical);
            }
        }

#if UNITY_EDITOR
        /// <summary>Test-only: start a swing and advance it without Update/input.</summary>
        public void PerformAttackForTests() => PerformAttack();
        public void TickPendingStrikeForTests(float deltaTime) => TickPendingStrike(deltaTime);
#endif

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Vector3 origin = transform.position + Vector3.up + transform.forward * (_range * 0.5f);
            Gizmos.DrawWireSphere(origin, _radius);
        }
    }
}
