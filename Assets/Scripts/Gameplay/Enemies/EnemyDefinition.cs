using UnityEngine;

namespace Rapadura.Gameplay.Enemies
{
    /// <summary>
    /// Data-driven configuration for an enemy archetype. A single <see cref="EnemyController"/>
    /// prefab can be reconfigured at spawn time by swapping the definition it references, which is
    /// how the "boss inicial" variant is produced (same controller, higher stats, <see cref="IsBoss"/> flag)
    /// without requiring a dedicated prefab or scene asset.
    /// </summary>
    [CreateAssetMenu(menuName = "Rapadura/Enemies/Enemy Definition", fileName = "NewEnemyDefinition")]
    public class EnemyDefinition : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string _enemyName = "Enemy";
        [SerializeField] private bool _isBoss;

        [Header("Vitals")]
        [SerializeField] private float _maxHealth = 30f;
        [SerializeField] private float _contactDamage = 10f;

        [Header("Movement")]
        [SerializeField] private float _patrolSpeed = 2f;
        [SerializeField] private float _chaseSpeed = 4f;
        [SerializeField] private float _fleeSpeed = 5f;
        [SerializeField] private float _waypointArrivalThreshold = 0.25f;

        [Header("Detection")]
        [SerializeField] private float _detectionRadius = 8f;
        [SerializeField] private float _loseTargetRadius = 12f;

        [Header("Attack")]
        [SerializeField] private float _attackRange = 1.5f;
        [SerializeField] private float _attackCooldown = 1.2f;
        [SerializeField] private float _attackDamage = 8f;

        [Header("Flee")]
        [Range(0f, 1f)]
        [SerializeField] private float _fleeHealthThreshold = 0.2f;
        [SerializeField] private float _fleeSafeDistance = 10f;

        public string EnemyName => _enemyName;
        public bool IsBoss => _isBoss;

        public float MaxHealth => _maxHealth;
        public float ContactDamage => _contactDamage;

        public float PatrolSpeed => _patrolSpeed;
        public float ChaseSpeed => _chaseSpeed;
        public float FleeSpeed => _fleeSpeed;
        public float WaypointArrivalThreshold => _waypointArrivalThreshold;

        public float DetectionRadius => _detectionRadius;
        public float LoseTargetRadius => _loseTargetRadius;

        public float AttackRange => _attackRange;
        public float AttackCooldown => _attackCooldown;
        public float AttackDamage => _attackDamage;

        public float FleeHealthThreshold => _fleeHealthThreshold;
        public float FleeSafeDistance => _fleeSafeDistance;
    }
}
