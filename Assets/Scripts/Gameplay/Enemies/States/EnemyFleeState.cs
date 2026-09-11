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

            // Was `recovered && safeDistance` (AND) — but Health (used by every enemy) has no
            // passive regeneration at all, so `recovered` can never become true once health drops
            // below the flee threshold (it only ever decreases further from here). Requiring BOTH
            // meant every enemy that ever started fleeing was stuck fleeing permanently, for the
            // rest of the game, even after running arbitrarily far from the player — the
            // safeDistance check was dead code. OR matches the actual design intent: stop fleeing
            // once either condition alone is enough to feel safe.
            if (recovered || safeDistance)
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
