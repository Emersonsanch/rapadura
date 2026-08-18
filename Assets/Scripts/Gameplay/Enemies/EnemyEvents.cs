using Rapadura.Core.EventBus;
using UnityEngine;

namespace Rapadura.Gameplay.Enemies
{
    /// <summary>Raised once when an enemy first spots a target and begins chasing it.</summary>
    public readonly struct EnemyTargetSpottedEvent : IGameEvent
    {
        public readonly GameObject Enemy;
        public readonly GameObject Target;

        public EnemyTargetSpottedEvent(GameObject enemy, GameObject target)
        {
            Enemy = enemy;
            Target = target;
        }
    }

    /// <summary>Raised when an enemy loses track of its target and returns to patrolling.</summary>
    public readonly struct EnemyTargetLostEvent : IGameEvent
    {
        public readonly GameObject Enemy;

        public EnemyTargetLostEvent(GameObject enemy)
        {
            Enemy = enemy;
        }
    }

    /// <summary>Raised when an enemy commits to an attack action against its current target.</summary>
    public readonly struct EnemyAttackedEvent : IGameEvent
    {
        public readonly GameObject Enemy;
        public readonly GameObject Target;
        public readonly float Damage;

        public EnemyAttackedEvent(GameObject enemy, GameObject target, float damage)
        {
            Enemy = enemy;
            Target = target;
            Damage = damage;
        }
    }

    /// <summary>Raised when an enemy's health drops below its flee threshold and it starts fleeing.</summary>
    public readonly struct EnemyFleeingEvent : IGameEvent
    {
        public readonly GameObject Enemy;

        public EnemyFleeingEvent(GameObject enemy)
        {
            Enemy = enemy;
        }
    }

    /// <summary>Raised when an enemy dies (health reaches zero).</summary>
    public readonly struct EnemyDiedEvent : IGameEvent
    {
        public readonly GameObject Enemy;
        public readonly bool WasBoss;

        public EnemyDiedEvent(GameObject enemy, bool wasBoss)
        {
            Enemy = enemy;
            WasBoss = wasBoss;
        }
    }

    /// <summary>Raised by a spawner/wave director when a new wave of enemies begins.</summary>
    public readonly struct WaveStartedEvent : IGameEvent
    {
        public readonly int WaveIndex;
        public readonly int EnemyCount;

        public WaveStartedEvent(int waveIndex, int enemyCount)
        {
            WaveIndex = waveIndex;
            EnemyCount = enemyCount;
        }
    }

    /// <summary>Raised when every enemy spawned as part of a wave has died.</summary>
    public readonly struct WaveCompletedEvent : IGameEvent
    {
        public readonly int WaveIndex;

        public WaveCompletedEvent(int waveIndex)
        {
            WaveIndex = waveIndex;
        }
    }

    /// <summary>Raised whenever the spawner pulls an enemy instance out of (or returns it to) the pool.</summary>
    public readonly struct EnemySpawnedEvent : IGameEvent
    {
        public readonly GameObject Enemy;

        public EnemySpawnedEvent(GameObject enemy)
        {
            Enemy = enemy;
        }
    }
}
