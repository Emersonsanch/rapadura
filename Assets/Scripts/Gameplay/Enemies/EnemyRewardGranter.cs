using Rapadura.Core.Events;
using Rapadura.Gameplay.Combat;
using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.Story;
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

            // Bosses may multiply this (EnemyDefinition.BossExperienceMultiplier) — see BossBonusRules —
            // and a "Nova Jornada+" cycle multiplies the result (NewGamePlusRules, ×1 on the first campaign).
            int reward = BossBonusRules.GetExperienceReward(enemy.Definition);
            // "Presa do dia" / "Bicho destaque do dia" (EnemyRewardHooks; x1 when nothing is installed).
            reward = EnemyRewardHooks.ScaleExperience(enemy.Definition, reward);
            // Event rooms rescale each enemy to the hero's level bracket (IKillExperienceScaler on the instance).
            if (enemy.TryGetComponent(out IKillExperienceScaler scaler))
            {
                reward = scaler.ScaleKillExperience(reward);
            }
            // "Fôlego diário" (+30% / x1 / -50%, at the campaign pace): the one place kill XP takes the stamina tier.
            reward = Rapadura.Gameplay.Daily.DailyStaminaRules.ScaleExperience(reward, Rapadura.Gameplay.Daily.DailyStaminaService.ResolveExperienceMultiplier());
            // "Descansado" (bed at home, Housing.md §6.1): +2..10% for a few fights; x1 when not rested (HouseService).
            reward = Rapadura.Gameplay.Housing.HouseRestedRules.ScaleExperience(reward, Rapadura.Gameplay.Housing.HouseService.ResolveRestedMultiplier());
            killerStats.AddExperience(NewGamePlusRules.ScaleExperience(reward, NewGamePlusProgress.ResolveCurrentCycle()));
        }
    }
}
