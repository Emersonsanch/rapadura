using Rapadura.Core.StateMachine;
using UnityEngine;

namespace Rapadura.Gameplay.Enemies.States
{
    /// <summary>Moves the enemy straight toward its target while it stays within the leash radius, until it's close enough to attack or health drops low enough to flee.</summary>
    public class EnemyChaseState : IState<EnemyController>
    {
        public void Enter(EnemyController context)
        {
        }

        public void Tick(EnemyController context, float deltaTime)
        {
            EnemyDefinition definition = context.Definition;
            if (definition == null || context.Target == null)
            {
                context.StateMachine.ChangeState<EnemyPatrolState>();
                return;
            }

            if (context.CanFlee && context.Health != null && EnemyDetectionUtility.ShouldFlee(context.Health.CurrentHealth, context.Health.MaxHealth, definition.FleeHealthThreshold))
            {
                context.PublishFleeing();
                context.StateMachine.ChangeState<EnemyFleeState>();
                return;
            }

            Vector3 selfPosition = context.transform.position;
            Vector3 targetPosition = context.Target.position;

            // Encounter enemies are aggro-locked: the arena fight never "loses" the hero.
            if (!context.IsAggroLocked && EnemyDetectionUtility.IsTargetLost(selfPosition, targetPosition, definition.LoseTargetRadius))
            {
                context.MarkTargetLost();
                context.StateMachine.ChangeState<EnemyPatrolState>();
                return;
            }

            if (EnemyDetectionUtility.IsWithinAttackRange(selfPosition, targetPosition, definition.AttackRange))
            {
                context.StateMachine.ChangeState<EnemyAttackState>();
                return;
            }

            // Ease in over the last stretch (aiming slightly inside attack range) so the smoothed
            // velocity has bled off on arrival instead of coasting into the target.
            Vector3 direction = EnemyDetectionUtility.DirectionTowards(selfPosition, targetPosition);
            Vector3 toTarget = targetPosition - selfPosition;
            toTarget.y = 0f;
            float remaining = toTarget.magnitude - definition.AttackRange * 0.8f;
            context.MoveInDirection(direction, context.GetArriveSpeed(definition.ChaseSpeed, remaining), deltaTime);
        }

        public void FixedTick(EnemyController context, float fixedDeltaTime)
        {
        }

        public void Exit(EnemyController context)
        {
        }
    }
}
