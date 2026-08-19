using Rapadura.Gameplay.Skills;
using UnityEngine;

namespace Rapadura.Gameplay.Combat.Weapons
{
    /// <summary>
    /// Simple travelling projectile spawned by <see cref="RangedWeapon"/>. Rather than
    /// re-implementing hit resolution, a projectile carries its own <see cref="Hitbox"/> and
    /// simply flies forward until that hitbox reports a hit (or its lifetime expires) — the same
    /// "DamageOnTouch"-style composition used by common Unity projectile frameworks. All actual
    /// damage/knockback/ICombatTarget/DamageInfo handling stays inside <see cref="Hitbox"/>.
    /// </summary>
    [RequireComponent(typeof(Hitbox))]
    public class Projectile : MonoBehaviour
    {
        [SerializeField] private float _maxLifetimeSeconds = 5f;
        [SerializeField] private bool _destroyOnHit = true;

        private Hitbox _hitbox;
        private Vector3 _direction = Vector3.forward;
        private float _speed = 20f;
        private float _lifeTimer;
        private bool _isDead;

        public bool IsDead => _isDead;
        public Hitbox Hitbox => _hitbox;

        private void Awake()
        {
            _hitbox = GetComponent<Hitbox>();
        }

        /// <summary>Configures and starts this projectile flying. Call right after Instantiate.</summary>
        public void Launch(Vector3 direction, float speed, float damage, ElementType element, GameObject source)
        {
            _direction = direction.sqrMagnitude > 0f ? direction.normalized : transform.forward;
            _speed = speed;
            _lifeTimer = 0f;
            _isDead = false;

            if (_hitbox != null)
            {
                _hitbox.Damage = damage;
                _hitbox.ResetHitTargets();
            }

            transform.forward = _direction;
        }

        private void Update()
        {
            AdvanceStep(Time.deltaTime);
        }

        /// <summary>Moves the projectile and expires it past its lifetime. Exposed for deterministic EditMode tests.</summary>
        public void AdvanceStep(float deltaTime)
        {
            if (_isDead)
            {
                return;
            }

            transform.position += _direction * (_speed * deltaTime);
            _lifeTimer += deltaTime;

            if (_lifeTimer >= _maxLifetimeSeconds)
            {
                Expire();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            ProcessCollision(other);
        }

        /// <summary>Runs the same collision handling <see cref="OnTriggerEnter"/> uses. Exposed for EditMode tests that can't rely on physics callbacks.</summary>
        public bool ProcessCollision(Collider other)
        {
            if (_isDead || _hitbox == null)
            {
                return false;
            }

            bool hit = _hitbox.ProcessTriggerEnter(other);

            if (hit && _destroyOnHit)
            {
                Expire();
            }

            return hit;
        }

        private void Expire()
        {
            if (_isDead)
            {
                return;
            }

            _isDead = true;

            if (Application.isPlaying)
            {
                Destroy(gameObject);
            }
        }
    }
}
