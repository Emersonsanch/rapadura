using Rapadura.Core.StateMachine;
using UnityEngine;

namespace Rapadura.Gameplay.Enemies.States
{
    /// <summary>
    /// Walks the enemy between its assigned waypoints in order, looping back to the first once the
    /// last is reached. Most enemies in the test scene have no waypoints configured, so this also
    /// falls back to picking random wander points around the enemy's spawn position — otherwise an
    /// enemy with no waypoints simply stood frozen until the player happened to walk within its
    /// (small) detection radius, which looked like broken/missing movement (e.g. the spiders).
    /// </summary>
    public class EnemyPatrolState : IState<EnemyController>
    {
        private const float WanderRadius = 5f;
        private const float MinPauseSeconds = 1.2f;
        private const float MaxPauseSeconds = 2.8f;

        private Vector3 _spawnPosition;
        private Vector3 _wanderTarget;
        private bool _hasWanderTarget;
        private float _pauseTimeRemaining;

        public void Enter(EnemyController context)
        {
            _spawnPosition = context.transform.position;
            _hasWanderTarget = false;
            _pauseTimeRemaining = 0f;
        }

        public void Tick(EnemyController context, float deltaTime)
        {
            EnemyDefinition definition = context.Definition;
            if (definition == null)
            {
                return;
            }

            if (context.Target != null && (context.IsAggroLocked
                    || EnemyDetectionUtility.IsWithinRadius(context.transform.position, context.Target.position,
                        EnemyDetectionUtility.DetectionRadiusFor(definition.DetectionRadius, context.Target))))
            {
                context.MarkTargetSpotted();
                context.StateMachine.ChangeState<EnemyChaseState>();
                return;
            }

            Transform[] waypoints = context.Waypoints;
            if (waypoints == null || waypoints.Length == 0)
            {
                Wander(context, definition, deltaTime);
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
            context.MoveInDirection(direction, definition.PatrolSpeed * context.PaceMultiplier, deltaTime);
        }

        /// <summary>
        /// Walks to a random point near spawn, then pauses briefly before picking the next one —
        /// without the pause, arriving and immediately choosing a new random (often
        /// nearly-opposite) direction made the creature spin/backtrack constantly, reading as
        /// erratic rather than a natural "walk, stop and look around, walk again" wander cycle.
        /// </summary>
        private void Wander(EnemyController context, EnemyDefinition definition, float deltaTime)
        {
            Vector3 position = context.transform.position;

            if (_pauseTimeRemaining > 0f)
            {
                _pauseTimeRemaining -= deltaTime;
                return;
            }

            if (!_hasWanderTarget || EnemyDetectionUtility.HasArrivedAtWaypoint(position, _wanderTarget, definition.WaypointArrivalThreshold))
            {
                Vector2 randomOffset = Random.insideUnitCircle * WanderRadius;
                _wanderTarget = _spawnPosition + new Vector3(randomOffset.x, 0f, randomOffset.y);
                _hasWanderTarget = true;
                _pauseTimeRemaining = Random.Range(MinPauseSeconds, MaxPauseSeconds);
                return;
            }

            // Each creature ambles at its own pace (EnemyController.PaceMultiplier): a pack never moves in lock-step.
            Vector3 direction = EnemyDetectionUtility.DirectionTowards(position, _wanderTarget);
            context.MoveInDirection(direction, definition.PatrolSpeed * context.PaceMultiplier, deltaTime);
        }

        public void FixedTick(EnemyController context, float fixedDeltaTime)
        {
        }

        public void Exit(EnemyController context)
        {
        }
    }
}
