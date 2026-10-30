using System.Collections.Generic;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Secrets;
using UnityEngine;

namespace Rapadura.Gameplay.Mounts
{
    /// <summary>
    /// What the M1 abilities do in the arena (the map versions live in <see cref="WorldMapMountRider"/>):
    /// <list type="bullet">
    /// <item><b>Arrancada</b> (Dash): <see cref="MountController"/> runs it as a short motor burst (6 m in 0.3 s, walls
    /// stop it); no FOV kick at all, so "Reduzir Movimento" has nothing to soften.</item>
    /// <item><b>Faro de Tesouro</b> (LootMagnet): loot already goes straight into the bag in this build, so the "pull"
    /// half has nothing to pull; the "mark" half flags every unbroken arena secret within <c>Magnitude</c> m for
    /// <c>Duration</c> s (<see cref="FindSecrets"/>).</item>
    /// <item><b>Zurro Espanta-Bicho</b> (Fear): common enemies within <c>Radius</c> m hold their aggro and run away from
    /// the rider for <c>Duration</c> s (<see cref="MountFearDriver"/>); bosses ignore it (<see cref="MountRules.EffectDurationOn"/>).</item>
    /// </list>
    /// </summary>
    public static class MountAbilityExecutor
    {
        /// <summary>Unbroken arena secrets within <paramref name="radius"/> of <paramref name="center"/>.</summary>
        public static List<ArenaSecretObject> FindSecrets(Vector3 center, float radius)
        {
            var found = new List<ArenaSecretObject>();
            foreach (ArenaSecretObject secret in Object.FindObjectsByType<ArenaSecretObject>(FindObjectsSortMode.None))
            {
                if (secret != null && !secret.IsBroken && (secret.transform.position - center).sqrMagnitude <= radius * radius)
                {
                    found.Add(secret);
                }
            }

            return found;
        }

        /// <summary>Scares the common enemies around <paramref name="source"/>; returns how many ran.</summary>
        public static int Scare(Transform source, MountAbilityDefinition ability)
        {
            if (source == null || ability == null)
            {
                return 0;
            }

            float radius = Mathf.Max(0.5f, ability.Radius);
            int scared = 0;
            var snapshot = new List<EnemyController>(EnemyController.EnabledEnemies);
            foreach (EnemyController enemy in snapshot)
            {
                if (enemy == null || (enemy.Health != null && enemy.Health.IsDead))
                {
                    continue;
                }

                if ((enemy.transform.position - source.position).sqrMagnitude > radius * radius)
                {
                    continue;
                }

                float duration = MountRules.EffectDurationOn(ability.DurationSeconds, enemy.IsBoss, ability.BossMultiplier);
                if (duration <= 0f)
                {
                    continue;
                }

                enemy.HoldAggro(duration, forceAggroWhenHoldEnds: true);
                MountFearDriver.Apply(enemy, source, duration);
                scared++;
            }

            return scared;
        }
    }

    /// <summary>
    /// Runs a scared enemy away from the bray's source for a few seconds (its AI holds aggro meanwhile, so it neither
    /// chases nor swings), then removes itself. Added at runtime by <see cref="MountAbilityExecutor.Scare"/>.
    /// </summary>
    [DisallowMultipleComponent]
    public class MountFearDriver : MonoBehaviour
    {
        private EnemyController _enemy;
        private Transform _source;
        private float _remaining;

        public float Remaining => _remaining;

        public static MountFearDriver Apply(EnemyController enemy, Transform source, float seconds)
        {
            MountFearDriver driver = enemy.GetComponent<MountFearDriver>();
            if (driver == null)
            {
                driver = enemy.gameObject.AddComponent<MountFearDriver>();
            }

            driver._enemy = enemy;
            driver._source = source;
            driver._remaining = Mathf.Max(driver._remaining, seconds);
            return driver;
        }

        private void LateUpdate()
        {
            float dt = Time.deltaTime;
            if (_enemy == null || _source == null || (_enemy.Health != null && _enemy.Health.IsDead) || _remaining <= 0f)
            {
                Destroy(this);
                return;
            }

            if (dt <= 0f)
            {
                return;
            }

            _remaining -= dt;
            Vector3 away = transform.position - _source.position;
            away.y = 0f;
            if (away.sqrMagnitude < 0.0001f)
            {
                away = -transform.forward;
            }

            _enemy.MoveInDirection(away.normalized, EnemyDetectionUtility.MaxFleeSpeed, dt);
        }
    }
}
