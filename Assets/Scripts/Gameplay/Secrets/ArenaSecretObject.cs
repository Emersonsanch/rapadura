using Rapadura.Core.Events;
using Rapadura.Gameplay.Combat;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Inventory;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.Skills;
using Rapadura.Gameplay.World.Biomes;
using UnityEngine;

namespace Rapadura.Gameplay.Secrets
{
    /// <summary>Raised when an arena secret appears (the toast tells the player to break it before the fight ends).</summary>
    public readonly struct ArenaSecretSpawnedEvent : IGameEvent
    {
        public readonly BiomeType Biome;
        public readonly Vector3 Position;

        public ArenaSecretSpawnedEvent(BiomeType biome, Vector3 position)
        {
            Biome = biome;
            Position = position;
        }
    }

    /// <summary>
    /// The breakable secret of an encounter arena (a cracked rock, a glinting patch of sand, a crystal-crusted
    /// rock), placed by <see cref="ArenaSecretSpawner"/>. An <see cref="ICombatTarget"/> with a solid collider, so
    /// the existing combat loop does everything: tapping it walks the hero over and auto-attacks
    /// (<c>PlayerClickToMove</c> / <c>PlayerBasicAttack</c>), and any hit not dealt by an enemy counts — after
    /// <see cref="SecretRules.ArenaSecretHits"/> hits it breaks into its biome's relic
    /// (<see cref="SecretsProgress.BreakArenaSecret"/>), or gold straight into the bag once the relic is home.
    /// Not an <c>EnemyController</c>: skills' soft-targeting and the arena's victory count ignore it.
    /// </summary>
    public class ArenaSecretObject : MonoBehaviour, ICombatTarget
    {
        private const float PunchSeconds = 0.18f;
        private const float BreakSeconds = 0.45f;
        private const float BobAmplitude = 0.08f;

        private BiomeType _biome;
        private int _hitsTaken;
        private bool _broken;
        private float _punch;
        private float _breakTimer = -1f;
        private Vector3 _baseScale = Vector3.one;
        private Transform _sparkle;
        private Vector3 _sparkleBase;

        public bool IsAlive => !_broken;
        public Transform Transform => transform;
        public BiomeType Biome => _biome;
        public int HitsTaken => _hitsTaken;
        public bool IsBroken => _broken;

        /// <summary>Set up by the spawner right after AddComponent (the visual is already a child).</summary>
        public void Configure(BiomeType biome, Transform sparkle)
        {
            _biome = biome;
            _baseScale = transform.localScale;
            _sparkle = sparkle;
            if (_sparkle != null)
            {
                _sparkleBase = _sparkle.localPosition;
            }
        }

        public void ApplyDamage(float amount, ElementType element, GameObject source)
        {
            if (_broken || !CountsAsHit(source))
            {
                return;
            }

            _hitsTaken++;
            _punch = PunchSeconds;
            if (_hitsTaken >= SecretRules.ArenaSecretHits)
            {
                Break();
            }
        }

        public void ApplyHealing(float amount, GameObject source)
        {
        }

        /// <summary>Enemies' swings and area attacks never open it: only the hero's (or a hit with no known source).</summary>
        public static bool CountsAsHit(GameObject source)
        {
            return source == null || source.GetComponentInParent<EnemyController>() == null;
        }

        private void Break()
        {
            _broken = true;
            _breakTimer = BreakSeconds;

            SecretsProgress secrets = SecretsProgress.ResolveShared();
            secrets.BreakArenaSecret(_biome, out int gold);
            if (gold > 0)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                ItemDefinition coin = ItemDatabase.GetById(RelicPassiveApplier.GoldItemId);
                if (player != null && coin != null && player.TryGetComponent(out InventoryManager inventory))
                {
                    int leftover = inventory.AddItem(coin, gold);
                    secrets.ReturnPendingGold(leftover);
                }
                else
                {
                    secrets.ReturnPendingGold(gold);
                }
            }

            foreach (Collider body in GetComponentsInChildren<Collider>())
            {
                body.enabled = false;
            }
        }

        private void Update()
        {
            float dt = Time.deltaTime;

            if (_sparkle != null && !_broken)
            {
                _sparkle.localPosition = _sparkleBase + Vector3.up * (Mathf.Sin(Time.time * 3f) * BobAmplitude);
                _sparkle.Rotate(0f, 120f * dt, 0f, Space.World);
            }

            if (_breakTimer >= 0f)
            {
                _breakTimer -= dt;
                float t = Mathf.Clamp01(1f - _breakTimer / BreakSeconds);
                transform.localScale = Vector3.Lerp(_baseScale * 1.15f, Vector3.zero, t * t);
                if (_breakTimer <= 0f)
                {
                    Destroy(gameObject);
                }

                return;
            }

            if (_punch > 0f)
            {
                _punch -= dt;
                float k = Mathf.Sin(Mathf.Clamp01(_punch / PunchSeconds) * Mathf.PI);
                transform.localScale = new Vector3(_baseScale.x * (1f + 0.12f * k), _baseScale.y * (1f - 0.1f * k), _baseScale.z * (1f + 0.12f * k));
                if (_punch <= 0f)
                {
                    transform.localScale = _baseScale;
                }
            }
        }
    }
}
