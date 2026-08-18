using System.Collections.Generic;
using Rapadura.Core.EventBus;
using Rapadura.Gameplay.Skills;
using UnityEngine;

namespace Rapadura.Gameplay.Combat
{
    /// <summary>
    /// Trigger-collider component that deals damage to whatever <see cref="Hurtbox"/> it overlaps.
    /// Attached to weapons, projectiles, AoE zones, etc. Fully decoupled from its target's
    /// concrete type — it only ever talks to <see cref="ICombatTarget"/> — and from the
    /// presentation layer, which it talks to only through the EventBus
    /// (<see cref="CameraShakeRequestedEvent"/>, <see cref="HitStopRequestedEvent"/>).
    ///
    /// Owns a small per-activation "already hit" set so a hitbox that stays active for several
    /// physics frames (e.g. a sword swing) does not multi-hit the same target every frame.
    /// Call <see cref="ResetHitTargets"/> when a new swing/activation begins.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class Hitbox : MonoBehaviour
    {
        [Header("Damage")]
        [SerializeField] private float _damage = 10f;
        [SerializeField] private ElementType _element = ElementType.Physical;
        [SerializeField] private bool _isCritical;

        [Header("Knockback")]
        [SerializeField] private float _knockbackForce = 0f;

        [Header("Game Feel")]
        [SerializeField] private bool _triggersHitStop = true;
        [SerializeField] private float _hitStopDuration = 0.05f;
        [SerializeField] private float _hitStopTimeScale = 0.02f;
        [SerializeField] private bool _triggersCameraShake = true;
        [SerializeField] private float _shakeDuration = 0.15f;
        [SerializeField] private float _shakeMagnitude = 0.2f;

        [Header("Source")]
        [Tooltip("GameObject credited as the damage source. Defaults to this hitbox's root if unset.")]
        [SerializeField] private GameObject _source;

        private readonly HashSet<ICombatTarget> _alreadyHitThisActivation = new HashSet<ICombatTarget>();

        public float Damage { get => _damage; set => _damage = value; }
        public GameObject Source => _source != null ? _source : gameObject;

        private void OnTriggerEnter(Collider other)
        {
            ProcessTriggerEnter(other);
        }

        /// <summary>
        /// Core hit-resolution logic, split out from <see cref="OnTriggerEnter"/> so it can be
        /// exercised directly from EditMode tests without needing the physics engine to run.
        /// </summary>
        public bool ProcessTriggerEnter(Collider other)
        {
            if (other == null)
            {
                return false;
            }

            Hurtbox hurtbox = other.GetComponentInParent<Hurtbox>();
            if (hurtbox == null)
            {
                return false;
            }

            ICombatTarget target = hurtbox.Owner;
            if (target == null || !target.IsAlive)
            {
                return false;
            }

            if (!_alreadyHitThisActivation.Add(target))
            {
                return false;
            }

            Vector3 hitPoint = other.ClosestPoint(transform.position);
            Vector3 knockbackDirection = target.Transform.position - transform.position;
            knockbackDirection.y = 0f;

            var info = new DamageInfo(
                _damage * hurtbox.DamageMultiplier,
                _element,
                Source,
                hitPoint,
                knockbackDirection,
                _knockbackForce,
                _isCritical);

            ApplyHit(target, hurtbox, info);
            return true;
        }

        private void ApplyHit(ICombatTarget target, Hurtbox hurtbox, DamageInfo info)
        {
            if (target is Health health)
            {
                health.ApplyDamage(info);
            }
            else
            {
                target.ApplyDamage(info.Amount, info.Element, info.Source);
            }

            if (_knockbackForce > 0f)
            {
                KnockbackReceiver receiver = hurtbox.GetComponentInParent<KnockbackReceiver>();
                receiver?.ApplyKnockback(info.KnockbackDirection, info.KnockbackForce);
            }

            if (_triggersHitStop)
            {
                EventBus.Publish(new HitStopRequestedEvent(_hitStopDuration, _hitStopTimeScale));
            }

            if (_triggersCameraShake)
            {
                EventBus.Publish(new CameraShakeRequestedEvent(_shakeDuration, _shakeMagnitude));
            }
        }

        /// <summary>Call when a new swing/activation starts so previously-hit targets can be hit again.</summary>
        public void ResetHitTargets()
        {
            _alreadyHitThisActivation.Clear();
        }
    }
}
