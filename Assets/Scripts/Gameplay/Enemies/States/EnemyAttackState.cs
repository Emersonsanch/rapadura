using Rapadura.Core.StateMachine;
using UnityEngine;

namespace Rapadura.Gameplay.Enemies.States
{
    /// <summary>Stays in place attacking the target on a cooldown, using the shared Hitbox if one is assigned, or applying damage directly to the target's ICombatTarget otherwise.</summary>
    public class EnemyAttackState : IState<EnemyController>
    {
        private float _cooldownTimer;

        public void Enter(EnemyController context)
        {
            // Attack immediately on entering range, then respect the cooldown afterwards.
            _cooldownTimer = 0f;
        }

        public void Tick(EnemyController context, float deltaTime)
        {
            EnemyDefinition definition = context.Definition;
            if (definition == null || context.Target == null)
            {
                context.StateMachine.ChangeState<EnemyPatrolState>();
                return;
            }

            if (context.Health != null && EnemyDetectionUtility.ShouldFlee(context.Health.CurrentHealth, context.Health.MaxHealth, definition.FleeHealthThreshold))
            {
                context.PublishFleeing();
                context.StateMachine.ChangeState<EnemyFleeState>();
                return;
            }

            Vector3 selfPosition = context.transform.position;
            Vector3 targetPosition = context.Target.position;

            if (!EnemyDetectionUtility.IsWithinAttackRange(selfPosition, targetPosition, definition.AttackRange))
            {
                context.StateMachine.ChangeState<EnemyChaseState>();
                return;
            }

            _cooldownTimer -= deltaTime;
            if (_cooldownTimer <= 0f)
            {
                PerformAttack(context, definition);
                _cooldownTimer = definition.AttackCooldown;
            }
        }

        private static void PerformAttack(EnemyController context, EnemyDefinition definition)
        {
            if (context.Hitbox != null)
            {
                context.Hitbox.Damage = definition.AttackDamage;
                context.Hitbox.ResetHitTargets();
            }

            context.ApplyDamageToTarget(definition.AttackDamage);
        }

        public void FixedTick(EnemyController context, float fixedDeltaTime)
        {
        }

        public void Exit(EnemyController context)
        {
        }
    }
}
