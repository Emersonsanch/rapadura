using Rapadura.Core.StateMachine;
using UnityEngine;

namespace Rapadura.Gameplay.Enemies.States
{
    /// <summary>Runs directly away from the target while health is critical; returns to patrolling once safely clear or health has recovered.</summary>
    public class EnemyFleeState : IState<EnemyController>
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

            Vector3 selfPosition = context.transform.position;
            Vector3 targetPosition = context.Target.position;

            bool recovered = context.Health != null &&
                              EnemyDetectionUtility.CanRecoverFromFlee(context.Health.CurrentHealth, context.Health.MaxHealth, definition.FleeHealthThreshold);
            bool safeDistance = EnemyDetectionUtility.IsFarEnoughToStopFleeing(selfPosition, targetPosition, definition.FleeSafeDistance);

            if (recovered && safeDistance)
            {
                context.MarkTargetLost();
                context.StateMachine.ChangeState<EnemyPatrolState>();
                return;
            }

            Vector3 direction = EnemyDetectionUtility.DirectionAwayFrom(selfPosition, targetPosition);
            context.MoveInDirection(direction, definition.FleeSpeed, deltaTime);
        }

        public void FixedTick(EnemyController context, float fixedDeltaTime)
        {
        }

        public void Exit(EnemyController context)
        {
        }
    }
}
