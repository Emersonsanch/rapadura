using Rapadura.Core.StateMachine;
using UnityEngine;

namespace Rapadura.Gameplay.Enemies.States
{
    /// <summary>
    /// Runs directly away from the target while health is critical, at most at
    /// <see cref="EnemyDetectionUtility.MaxFleeSpeed"/>; returns to patrolling once safely clear or
    /// health has recovered, and tires after <see cref="EnemyDetectionUtility.FleeMaxDurationSeconds"/>,
    /// turning back to chase. Either way the flee is spent for this life
    /// (<see cref="EnemyController.MarkFleeExhausted"/>), so a low-HP enemy can't kite the hero forever.
    /// </summary>
    public class EnemyFleeState : IState<EnemyController>
    {
        private float _elapsed;

        public void Enter(EnemyController context)
        {
            _elapsed = 0f;
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
                context.MarkFleeExhausted();
                if (context.IsAggroLocked)
                {
                    context.StateMachine.ChangeState<EnemyChaseState>();
                    return;
                }

                context.MarkTargetLost();
                context.StateMachine.ChangeState<EnemyPatrolState>();
                return;
            }

            _elapsed += deltaTime;
            if (EnemyDetectionUtility.IsFleeExhausted(_elapsed, EnemyDetectionUtility.FleeMaxDurationSeconds))
            {
                // Tired: turn back and fight to the end instead of dragging the fight out.
                context.MarkFleeExhausted();
                context.StateMachine.ChangeState<EnemyChaseState>();
                return;
            }

            Vector3 direction = EnemyDetectionUtility.DirectionAwayFrom(selfPosition, targetPosition);
            context.MoveInDirection(direction, EnemyDetectionUtility.GetEffectiveFleeSpeed(definition.FleeSpeed), deltaTime);
        }

        public void FixedTick(EnemyController context, float fixedDeltaTime)
        {
        }

        public void Exit(EnemyController context)
        {
        }
    }
}
