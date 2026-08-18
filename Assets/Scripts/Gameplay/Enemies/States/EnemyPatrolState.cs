using Rapadura.Core.StateMachine;
using UnityEngine;

namespace Rapadura.Gameplay.Enemies.States
{
    /// <summary>Walks the enemy between its assigned waypoints in order, looping back to the first once the last is reached.</summary>
    public class EnemyPatrolState : IState<EnemyController>
    {
        public void Enter(EnemyController context)
        {
        }

        public void Tick(EnemyController context, float deltaTime)
        {
            EnemyDefinition definition = context.Definition;
            if (definition == null)
            {
                return;
            }

            if (context.Target != null && EnemyDetectionUtility.IsWithinRadius(context.transform.position, context.Target.position, definition.DetectionRadius))
            {
                context.MarkTargetSpotted();
                context.StateMachine.ChangeState<EnemyChaseState>();
                return;
            }

            Transform[] waypoints = context.Waypoints;
            if (waypoints == null || waypoints.Length == 0)
            {
                return;
            }

            Transform waypoint = waypoints[context.CurrentWaypointIndex % waypoints.Length];
            if (waypoint == null)
            {
                return;
            }

            if (EnemyDetectionUtility.HasArrivedAtWaypoint(context.transform.position, waypoint.position, definition.WaypointArrivalThreshold))
            {
                context.CurrentWaypointIndex = (context.CurrentWaypointIndex + 1) % waypoints.Length;
                return;
            }

            Vector3 direction = EnemyDetectionUtility.DirectionTowards(context.transform.position, waypoint.position);
            context.MoveInDirection(direction, definition.PatrolSpeed, deltaTime);
        }

        public void FixedTick(EnemyController context, float fixedDeltaTime)
        {
        }

        public void Exit(EnemyController context)
        {
        }
    }
}
