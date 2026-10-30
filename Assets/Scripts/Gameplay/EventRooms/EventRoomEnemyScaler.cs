using System.Collections;
using Rapadura.Gameplay.Combat;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.Skills;
using UnityEngine;

namespace Rapadura.Gameplay.EventRooms
{
    /// <summary>
    /// Adjusts one event-room enemy to the hero's level bracket (<see cref="EventRoomRules.EnemyScale"/>), the same way
    /// <c>NightVariantTag</c> makes a night variant: a frame after spawn (EnemyController.Start re-applies the
    /// definition's health) it multiplies the health, adds a PercentAdditive AttackDamage buff and scales the visual.
    /// Its kill XP is rescaled through <see cref="IKillExperienceScaler"/> (read by EnemyRewardGranter).
    /// </summary>
    public class EventRoomEnemyScaler : MonoBehaviour, IKillExperienceScaler
    {
        public const string BuffSource = "event_room_scale";

        private bool _applied;

        public EventRoomEnemyScale Scale { get; private set; } = EventRoomEnemyScale.Identity;
        public bool IsApplied => _applied;

        public static EventRoomEnemyScaler Attach(GameObject enemy, EventRoomEnemyScale scale, bool applyNow = false)
        {
            if (enemy == null)
            {
                return null;
            }

            EventRoomEnemyScaler scaler = enemy.GetComponent<EventRoomEnemyScaler>() ?? enemy.AddComponent<EventRoomEnemyScaler>();
            scaler.Scale = scale;
            if (applyNow)
            {
                scaler.ApplyNow();
            }

            return scaler;
        }

        public int ScaleKillExperience(int baseExperience) => EventRoomRules.ScaleExperience(baseExperience, Scale.ExperienceMultiplier);

        private IEnumerator Start()
        {
            yield return null;
            ApplyNow();
        }

        /// <summary>Applies health, damage and size once (idempotent). Public for tests.</summary>
        public void ApplyNow()
        {
            if (_applied)
            {
                return;
            }

            _applied = true;
            EnemyController controller = GetComponent<EnemyController>();
            Health health = controller != null && controller.Health != null ? controller.Health : GetComponent<Health>();
            if (health != null && !Mathf.Approximately(Scale.HealthMultiplier, 1f))
            {
                health.ConfigureMaxHealth(health.MaxHealth * Scale.HealthMultiplier);
            }

            BuffController buffs = controller != null ? controller.Buffs : GetComponent<BuffController>();
            if (buffs != null && !Mathf.Approximately(Scale.DamageBonus, 0f))
            {
                buffs.ApplyEffect(new StatModifierDefinition
                {
                    displayName = BuffSource,
                    affectedStat = StatType.AttackDamage,
                    application = ModifierApplication.PercentAdditive,
                    value = Scale.DamageBonus,
                    durationSeconds = 0f,
                }, BuffSource);
            }

            if (!Mathf.Approximately(Scale.VisualScale, 1f))
            {
                Transform visual = transform.Find(EnemyVisualSwapper.VisualName);
                (visual != null ? visual : transform).localScale *= Scale.VisualScale;
            }
        }
    }
}
