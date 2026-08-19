using Rapadura.Core.EventBus;
using UnityEngine;

namespace Rapadura.Gameplay.Combat.Weapons
{
    /// <summary>
    /// Melee weapon component: on <see cref="TryAttack"/> it opens an existing <see cref="Hitbox"/>
    /// for a short active window (matching the common "enable collider for N seconds of the swing
    /// animation" pattern used by most Unity melee frameworks), then closes it again. Does not
    /// create a new damage pipeline — it only toggles the collider and resets
    /// <see cref="Hitbox.ResetHitTargets"/>, so all actual damage/knockback/hit-stop logic stays
    /// owned by <see cref="Hitbox"/>.
    ///
    /// <see cref="Tick"/> is exposed as a plain method (not only driven from <see cref="Update"/>)
    /// so EditMode tests can advance the swing/cooldown timers deterministically without a running
    /// PlayMode loop, following the same pattern as the enemy FSM states.
    /// </summary>
    public class MeleeWeapon : WeaponController
    {
        [Header("Melee")]
        [SerializeField] private Hitbox _hitbox;
        [SerializeField] private float _fallbackDamage = 10f;
        [SerializeField] private float _activeWindowDuration = 0.2f;
        [SerializeField] private float _attackCooldown = 0.5f;

        private float _cooldownTimer;
        private float _activeTimer;
        private bool _isSwinging;

        public bool IsSwinging => _isSwinging;
        public bool CanAttack => !IsBroken && _cooldownTimer <= 0f && !_isSwinging && HasUsableWeaponEquipped();

        protected override void Awake()
        {
            base.Awake();

            if (_hitbox != null)
            {
                _hitbox.gameObject.SetActive(false);
            }
        }

        private void Update()
        {
            Tick(Time.deltaTime);
        }

        /// <summary>Advances cooldown/active-window timers. Safe to call directly from tests.</summary>
        public void Tick(float deltaTime)
        {
            if (_cooldownTimer > 0f)
            {
                _cooldownTimer -= deltaTime;
            }

            if (!_isSwinging)
            {
                return;
            }

            _activeTimer -= deltaTime;

            if (_activeTimer <= 0f)
            {
                EndSwing();
            }
        }

        /// <summary>Attempts to start a swing. Returns false if on cooldown, broken, or nothing equipped.</summary>
        public bool TryAttack()
        {
            if (!CanAttack)
            {
                return false;
            }

            if (_hitbox != null)
            {
                _hitbox.Damage = ResolveWeaponDamage(_fallbackDamage);
                _hitbox.ResetHitTargets();
                _hitbox.gameObject.SetActive(true);
            }

            _isSwinging = true;
            _activeTimer = _activeWindowDuration;
            _cooldownTimer = _attackCooldown;

            EventBus.Publish(new WeaponMeleeAttackEvent(gameObject, EquippedItem?.ItemId));

            // The swing that exhausts durability still lands (and its hitbox stays open for its
            // normal window) — only the *next* attempt is blocked once IsBroken is set.
            ConsumeDurabilityForUse();
            return true;
        }

        private void EndSwing()
        {
            _isSwinging = false;

            if (_hitbox != null)
            {
                _hitbox.gameObject.SetActive(false);
            }
        }
    }
}
