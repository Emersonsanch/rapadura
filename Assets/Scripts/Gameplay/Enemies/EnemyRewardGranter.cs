using Rapadura.Core.Events;
using Rapadura.Gameplay.Combat;
using Rapadura.Gameplay.Player;
using UnityEngine;

namespace Rapadura.Gameplay.Enemies
{
    /// <summary>
    /// Grants <see cref="EnemyDefinition.ExperienceReward"/> to the killer's <see cref="PlayerStats"/>
    /// when an enemy dies. Was entirely missing before this: <see cref="EnemyController"/> published
    /// <see cref="CombatTargetDiedEvent"/> with the killer reference already attached, and
    /// <see cref="PlayerStats.AddExperience"/> already existed and worked (leveling itself is fully
    /// implemented and tested) — nothing in between ever called it, so killing enemies granted zero
    /// XP in real gameplay despite the leveling system working perfectly in isolation.
    /// </summary>
    public class EnemyRewardGranter : MonoBehaviour
    {
        private void OnEnable()
        {
            EventBus.Subscribe<CombatTargetDiedEvent>(OnCombatTargetDied);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<CombatTargetDiedEvent>(OnCombatTargetDied);
        }

        private void OnCombatTargetDied(CombatTargetDiedEvent evt)
        {
            if (evt.Target == null || evt.Killer == null)
            {
                return;
            }

            EnemyController enemy = evt.Target.GetComponent<EnemyController>();
            if (enemy == null || enemy.Definition == null)
            {
                return;
            }

            PlayerStats killerStats = evt.Killer.GetComponentInParent<PlayerStats>();
            if (killerStats == null)
            {
                return;
            }

            killerStats.AddExperience(enemy.Definition.ExperienceReward);
        }
    }
}
