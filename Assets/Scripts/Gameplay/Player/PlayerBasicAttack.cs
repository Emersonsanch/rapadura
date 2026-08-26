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
        [SerializeField] private float _damage = 15f;
        [SerializeField] private float _range = 2.5f;
        [SerializeField] private float _radius = 1.2f;
        [SerializeField] private float _cooldownSeconds = 0.5f;
        [SerializeField] private LayerMask _targetMask = ~0;

        [Header("Click-to-Attack")]
        [Tooltip("Degrees/sec the player turns to face a pursued enemy before/while auto-attacking it. Instant snapping reads as janky, same reasoning as EnemyController's turn speed.")]
        [SerializeField] private float _turnSpeedDegPerSec = 540f;

        private PlayerInputHandler _input;
        private PlayerClickToMove _clickToMove;
        private ICombatTarget _self;
        private float _cooldownRemaining;
        private readonly Collider[] _hitBuffer = new Collider[16];

        private void Awake()
        {
            _input = GetComponent<PlayerInputHandler>();
            _clickToMove = GetComponent<PlayerClickToMove>();
            _self = GetComponent<ICombatTarget>();
        }

        private void Update()
        {
            if (_cooldownRemaining > 0f)
            {
                _cooldownRemaining -= Time.deltaTime;
            }

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

        private void PerformAttack()
        {
            EventBus.Publish(new PlayerAttackPerformedEvent(gameObject));

            Vector3 origin = transform.position + Vector3.up + transform.forward * (_range * 0.5f);
            int hitCount = Physics.OverlapSphereNonAlloc(origin, _radius, _hitBuffer, _targetMask, QueryTriggerInteraction.Collide);

            var alreadyHit = new HashSet<ICombatTarget>();

            for (int i = 0; i < hitCount; i++)
            {
                var target = _hitBuffer[i].GetComponentInParent<ICombatTarget>();
                if (target == null || target == _self || !alreadyHit.Add(target))
                {
                    continue;
                }

                target.ApplyDamage(_damage, ElementType.Physical, gameObject);
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Vector3 origin = transform.position + Vector3.up + transform.forward * (_range * 0.5f);
            Gizmos.DrawWireSphere(origin, _radius);
        }
    }
}
