using System.Collections;
using Rapadura.Gameplay.Combat;
using Rapadura.Gameplay.Skills;
using UnityEngine;

namespace Rapadura.Gameplay.Enemies
{
    /// <summary>
    /// Puts a creature's level on its stats (<see cref="EnemyLevelScaling"/>): a frame after spawn (EnemyController.Start
    /// re-applies the definition's health, as for <c>EventRoomEnemyScaler</c> / <c>NightVariantTag</c>) it multiplies the max
    /// health (a common of level 100 or more from at least <see cref="EnemyLevelScaling.CommonHealthFloor"/>) and adds a PercentAdditive AttackDamage buff. Only attached when the level is above the archetype level
    /// (<see cref="EnemyLevelScaling.IsIdentity"/>), so today's campaign encounters never get one.
    /// </summary>
    public class EnemyLevelScaler : MonoBehaviour
    {
        public const string BuffSource = "enemy_level_scale";

        private bool _applied;

        public int Level { get; private set; } = EnemyLevelScaling.ArchetypeLevel;
        public bool IsApplied => _applied;

        /// <summary>Attaches (or updates) the scaler for <paramref name="level"/>; returns null (nothing attached) at identity levels.</summary>
        public static EnemyLevelScaler Attach(GameObject enemy, int level, bool applyNow = false)
        {
            if (enemy == null || EnemyLevelScaling.IsIdentity(level))
            {
                return null;
            }

            EnemyLevelScaler scaler = enemy.GetComponent<EnemyLevelScaler>() ?? enemy.AddComponent<EnemyLevelScaler>();
            scaler.Level = EnemyLevelScaling.ClampLevel(level);
            if (applyNow)
            {
                scaler.ApplyNow();
            }

            return scaler;
        }

        private IEnumerator Start()
        {
            yield return null;
            ApplyNow();
        }

        /// <summary>Applies health and damage once (idempotent). Public for tests.</summary>
        public void ApplyNow()
        {
            if (_applied)
            {
                return;
            }

            _applied = true;
            float damage = EnemyLevelScaling.DamageMultiplier(Level);
            EnemyController controller = GetComponent<EnemyController>();
            Health target = controller != null && controller.Health != null ? controller.Health : GetComponent<Health>();
            if (target != null)
            {
                bool isBoss = controller != null && controller.IsBoss;
                float scaled = EnemyLevelScaling.HealthFor(target.MaxHealth, Level, isBoss);
                if (!Mathf.Approximately(scaled, target.MaxHealth))
                {
                    target.ConfigureMaxHealth(scaled);
                }
            }

            BuffController buffs = controller != null ? controller.Buffs : GetComponent<BuffController>();
            if (buffs != null && !Mathf.Approximately(damage, 1f))
            {
                buffs.ApplyEffect(new StatModifierDefinition
                {
                    displayName = BuffSource,
                    affectedStat = StatType.AttackDamage,
                    application = ModifierApplication.PercentAdditive,
                    value = damage - 1f,
                    durationSeconds = 0f,
                }, BuffSource);
            }
        }
    }
}
