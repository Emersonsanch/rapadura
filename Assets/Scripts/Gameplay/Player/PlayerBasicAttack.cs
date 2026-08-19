using System.Collections.Generic;
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

        private PlayerInputHandler _input;
        private ICombatTarget _self;
        private float _cooldownRemaining;
        private readonly Collider[] _hitBuffer = new Collider[16];

        private void Awake()
        {
            _input = GetComponent<PlayerInputHandler>();
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
            }
        }

        private void PerformAttack()
        {
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
