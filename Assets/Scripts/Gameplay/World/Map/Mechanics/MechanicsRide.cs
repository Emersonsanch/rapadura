using Rapadura.Gameplay.Mounts;
using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.World.Map.Overworld;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// A polyline in world x/z, walked by distance (pure): the course of a raft along a canal, of an updraft up a shaft.
    /// </summary>
    public static class RidePath
    {
        /// <summary>Total length (m) of the polyline.</summary>
        public static float Length(Vector2[] points)
        {
            float total = 0f;
            for (int i = 0; points != null && i + 1 < points.Length; i++)
            {
                total += Vector2.Distance(points[i], points[i + 1]);
            }

            return total;
        }

        /// <summary>The point <paramref name="distance"/> metres along the polyline (clamped to its two ends).</summary>
        public static Vector2 PointAt(Vector2[] points, float distance)
        {
            if (points == null || points.Length == 0)
            {
                return Vector2.zero;
            }

            float left = Mathf.Max(0f, distance);
            for (int i = 0; i + 1 < points.Length; i++)
            {
                float leg = Vector2.Distance(points[i], points[i + 1]);
                if (left <= leg)
                {
                    return leg > 1e-4f ? Vector2.Lerp(points[i], points[i + 1], left / leg) : points[i + 1];
                }

                left -= leg;
            }

            return points[points.Length - 1];
        }

        /// <summary>The direction of travel at <paramref name="distance"/> metres (the leg the point is on; forward for a one-point path).</summary>
        public static Vector2 DirectionAt(Vector2[] points, float distance)
        {
            if (points == null || points.Length < 2)
            {
                return Vector2.up;
            }

            float left = Mathf.Max(0f, distance);
            for (int i = 0; i + 1 < points.Length; i++)
            {
                float leg = Vector2.Distance(points[i], points[i + 1]);
                if (left <= leg || i + 2 == points.Length)
                {
                    Vector2 d = points[i + 1] - points[i];
                    return d.sqrMagnitude > 1e-6f ? d.normalized : Vector2.up;
                }

                left -= leg;
            }

            return Vector2.up;
        }

        /// <summary>The same points moved by <paramref name="origin"/> (region to world), in the given or the reverse order.</summary>
        public static Vector2[] ToWorld(Vector2[] local, Vector2 origin, bool reverse)
        {
            var world = new Vector2[local.Length];
            for (int i = 0; i < local.Length; i++)
            {
                world[i] = origin + local[reverse ? local.Length - 1 - i : i];
            }

            return world;
        }
    }

    /// <summary>What one frame of a <see cref="MechanicsRide"/> did.</summary>
    public enum RideStep
    {
        Idle = 0,
        Riding = 1,
        Arrived = 2,
        Broken = 3,
    }

    /// <summary>
    /// Scene side of a ride that takes the hero's controls (a raft, an updraft): the hero follows a polyline at a set speed, above the drawn
    /// ground or the water line, on an optional arc. The controls come back on arrival, on death, when anything else moves the hero (a respawn,
    /// a portal) and on <see cref="End"/>, which every driver calls from its OnDisable. Not a MonoBehaviour: each driver owns one.
    /// </summary>
    public sealed class MechanicsRide
    {
        /// <summary>Metres the hero may be moved by something else between two frames before the ride gives up.</summary>
        public const float ExternalMoveTolerance = 3f;

        private Transform _player;
        private PlayerController _controller;
        private CharacterController _character;
        private PlayerStats _stats;
        private OverworldLayout _layout;
        private Vector2[] _points;
        private float _length;
        private float _speed;
        private float _arc;
        private float _hover;
        private float _fromY;
        private float _toY;
        private float _travelled;
        private Vector3 _last;
        private float _dropSpeed;

        /// <summary>
        /// Metres to the right of the line of travel, set by the driver each frame (a steered cart, map 37); 0 rides the line itself. Begin
        /// puts it back to 0.
        /// </summary>
        public float Lateral { get; set; }

        /// <summary>Whether a ride begun with <paramref name="keepMount"/> gets the hero off the mount first (pure): every ride but the mount's own.</summary>
        public static bool DismountsHero(bool keepMount) => !keepMount;

        /// <summary>True when the last <see cref="Begin"/> left the hero on the mount; <see cref="End"/> never dismounts either way.</summary>
        public bool KeepsMount { get; private set; }

        /// <summary>True while the ride holds the controls.</summary>
        public bool Active { get; private set; }

        /// <summary>Seconds since the ride began.</summary>
        public float Elapsed { get; private set; }

        /// <summary>0 at the start, 1 on arrival.</summary>
        public float Progress => _length > 1e-3f ? Mathf.Clamp01(_travelled / _length) : 1f;

        /// <summary>The last point of the course (world x/z).</summary>
        public Vector2 End2D => _points != null && _points.Length > 0 ? _points[_points.Length - 1] : Vector2.zero;

        /// <summary>Where the hero is now (world x/z).</summary>
        public Vector2 Here => _points != null ? RidePath.PointAt(_points, _travelled) + Right(RidePath.DirectionAt(_points, _travelled)) * Lateral : Vector2.zero;

        /// <summary>
        /// Takes the controls and starts along <paramref name="worldPoints"/> at <paramref name="speed"/> m/s, <paramref name="hover"/> metres over the
        /// ground or water and <paramref name="arc"/> metres higher at the middle. False (nothing changes) when the hero's parts are missing.
        /// <paramref name="dropSpeed"/> &gt; 0 lets the hero fall at most that many metres a second when the ground drops away (a cart off a
        /// terrace step) instead of snapping down; climbing always follows the ground at once. <paramref name="keepMount"/> leaves the hero on
        /// the mount (Taweret's hippo swimming a canal): the mount rides along, parented to the hero, and the hero is still mounted when the
        /// controls come back; every other ride gets the hero off first (<see cref="DismountsHero"/>).
        /// </summary>
        public bool Begin(GameObject hero, OverworldLayout layout, Vector2[] worldPoints, float speed, float arc, float hover, float dropSpeed = 0f,
            bool keepMount = false)
        {
            if (Active || hero == null || worldPoints == null || worldPoints.Length < 2 || speed <= 0f)
            {
                return false;
            }

            var controller = hero.GetComponent<PlayerController>();
            var motor = hero.GetComponent<PlayerMotor>();
            var character = hero.GetComponent<CharacterController>();
            if (controller == null || motor == null || character == null)
            {
                return false;
            }

            if (DismountsHero(keepMount))
            {
                MountController.Active?.Dismount(startRecall: false, leave: false);
            }

            KeepsMount = keepMount;
            _player = hero.transform;
            _controller = controller;
            _character = character;
            _stats = hero.GetComponent<PlayerStats>();
            _layout = layout;
            _points = worldPoints;
            _length = RidePath.Length(worldPoints);
            _speed = speed;
            _arc = arc;
            _hover = hover;
            _fromY = Floor(worldPoints[0]);
            _toY = Floor(worldPoints[worldPoints.Length - 1]);
            _travelled = 0f;
            _dropSpeed = Mathf.Max(0f, dropSpeed);
            Lateral = 0f;
            Elapsed = 0f;

            // Clears the motor's velocity so the Animator rests in idle while the controls are off.
            Vector2 d = RidePath.DirectionAt(worldPoints, 0f);
            motor.Teleport(_player.position, Quaternion.LookRotation(new Vector3(d.x, 0f, d.y)));
            controller.ClickToMove?.ClearDestination();
            controller.enabled = false;
            character.enabled = false;
            _last = _player.position;
            Active = true;
            return true;
        }

        /// <summary>Moves the hero one frame along the course.</summary>
        public RideStep Tick(float deltaTime)
        {
            if (!Active)
            {
                return RideStep.Idle;
            }

            bool dead = _stats != null && !_stats.IsAlive;
            if (_player == null || dead || (_player.position - _last).sqrMagnitude > ExternalMoveTolerance * ExternalMoveTolerance)
            {
                End();
                return RideStep.Broken;
            }

            Elapsed += deltaTime;
            _travelled = Mathf.Min(_length, _travelled + _speed * deltaTime);
            float t = Progress;
            Vector2 d = RidePath.DirectionAt(_points, _travelled);
            Vector2 ground = RidePath.PointAt(_points, _travelled) + Right(d) * Lateral;
            float y = Mathf.Max(SkyRouteRules.HeightAt(_fromY, _toY, _arc, t), Floor(ground));
            if (_dropSpeed > 0f && y < _last.y)
            {
                y = Mathf.Max(y, _last.y - _dropSpeed * deltaTime);
            }

            _player.position = new Vector3(ground.x, y, ground.y);
            _player.rotation = Quaternion.LookRotation(new Vector3(d.x, 0f, d.y));
            _last = _player.position;
            if (t >= 1f)
            {
                End();
                return RideStep.Arrived;
            }

            return RideStep.Riding;
        }

        /// <summary>Hands the controls back (safe to call at any time, more than once).</summary>
        public void End()
        {
            if (!Active)
            {
                return;
            }

            Active = false;
            if (_character != null)
            {
                _character.enabled = true;
            }

            if (_controller != null)
            {
                _controller.enabled = true;
            }
        }

        /// <summary>The right-hand side of a direction of travel (world x/z).</summary>
        public static Vector2 Right(Vector2 direction) => new Vector2(direction.y, -direction.x);

        private float Floor(Vector2 p)
        {
            float ground = _layout != null ? OverworldRules.SurfaceAt(_layout, p) : 0f;
            return Mathf.Max(ground, OverworldRelief.WaterY) + _hover;
        }
    }
}
