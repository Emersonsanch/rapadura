using System.Collections.Generic;
using Rapadura.Gameplay.Player;
using UnityEngine;

namespace Rapadura.Gameplay.Locomotion
{
    /// <summary>
    /// Rig-free legged locomotion for models built from separate jointed parts (the procedural enemies,
    /// and anything else following the same naming): feet planted in the world with a ground raycast,
    /// stepping in gait order to a predicted landing when they drift from their rest target (never
    /// sliding), two-bone IK bending the knees toward the rest pose's bend, body bob/roll/pitch from the
    /// footfalls, and spring follow-through on the tail, ears, head, abdomen and claws. The per-leg logic
    /// is <see cref="LegGaitSolver"/> (pure, EditMode-tested); this component only binds transforms,
    /// probes the ground under a global per-frame raycast budget and writes rotations.
    ///
    /// Naming convention (searched under the bound body): a hip pivot <c>Leg_&lt;id&gt;</c> with a knee child
    /// <c>Shin_&lt;id&gt;</c> with a foot/ankle child <c>Foot_&lt;id&gt;</c> (two-bone leg); a <c>Leg_&lt;id&gt;</c>
    /// without a <c>Shin_</c> child is a one-bone leg that swings to its foot target. Legs are sorted into
    /// the <see cref="LegGaitMath"/> order (front to back, left before right). Follow-through parts:
    /// <c>Tail</c> (and its <c>Tail_*</c>/<c>Stinger</c> descendants), <c>Head</c>, <c>Ear_L</c>/<c>Ear_R</c>,
    /// <c>Abdomen</c>, <c>Claw_L</c>/<c>Claw_R</c>.
    ///
    /// Two ways to drive it: an owner animator calls <see cref="Advance"/> (before it poses the body) and
    /// <see cref="Solve"/> (after), as <c>EnemyProceduralAnimator</c> does; or tick <see cref="SelfDriven"/>
    /// to have it measure its own root's motion in LateUpdate. Allocation-free per frame.
    /// </summary>
    [DisallowMultipleComponent]
    public class ProceduralLegLocomotion : MonoBehaviour
    {
        public const string HipPrefix = "Leg_";
        public const string KneePrefix = "Shin_";
        public const string FootPrefix = "Foot_";

        /// <summary>Ground raycasts allowed per frame across every instance (each step costs one or two).</summary>
        public const int MaxRaycastsPerFrame = 40;

        private enum FollowKind
        {
            Tail,
            Head,
            Ear,
            Abdomen,
            Claw,
        }

        private struct Chain
        {
            public Transform Hip;
            public Transform Knee;
            public Transform Foot;
            public Quaternion HipRest;
            public Quaternion KneeRest;
            public Quaternion FootRest;
            public Quaternion FootInSpace;
            public Vector3 PoleLocal;
            public Vector3 EndInHip;
            public Vector3 RestInSpace;
            public Vector3 HipInSpace;
            public float Length;
            public int Side;
            /// <summary>+1 front half, -1 hind half, 0 middle / biped (same as the solver's).</summary>
            public int Front;
        }

        private struct Follow
        {
            public Transform Transform;
            public Quaternion Rest;
            public FollowKind Kind;
            public float Gain;
            public Vector2 Angle;
            public Vector2 Velocity;
        }

        [Tooltip("Layers the feet stand on.")]
        [SerializeField] private LayerMask _groundMask = Physics.DefaultRaycastLayers;

        [Tooltip("Measure the parent's motion and tick in LateUpdate (pets, props). Off when an owner animator drives it.")]
        [SerializeField] private bool _selfDriven;

        [SerializeField] private LegGaitSettings _settings = LegGaitSettings.Default(LegGaitStyle.Quadruped);

        [Tooltip("How much lower than its rest pose (fraction of the leg) the owner rides the body while stepping slowly, so the knees stay bent.")]
        [SerializeField, Range(0f, 0.3f)] private float _kneeBend = 0.08f;

        [Tooltip("Crouch (fraction of the leg) at full speed: deeper knees give the feet a longer reach, like a runner.")]
        [SerializeField, Range(0f, 0.4f)] private float _runKneeBend = 0.2f;

        private static int _budgetFrame = -1;
        private static int _budgetUsed;
        private static readonly RaycastHit[] Hits = new RaycastHit[8];
        private static int _cameraFrame = -1;
        private static Transform _camera;

        private readonly LegGaitSolver _solver = new LegGaitSolver();
        private readonly Chain[] _chains = new Chain[LegGaitSolver.MaxLegs];
        private readonly List<Follow> _follow = new List<Follow>(8);
        private int _chainCount;
        private Transform _space;
        private Transform _body;
        private bool _bound;

        private Vector2 _sway;
        private Vector2 _swayVelocity;
        private float _bob;
        private float _bobVelocity;
        private float _yawRate;
        private Vector3 _velocity;
        private Vector3 _lastBodyPosition;
        private bool _hasLastBody;
        private float _bodyVerticalSpeed;
        private float _bodyLimit = BodyLimitSlack;
        private Vector3 _bodyShift;
        private const float BodyLimitSlack = 1f;
        private const float BodyLimitRiseSpeed = 1.5f;
        private Vector3 _selfLastPosition;
        private float _selfLastYaw;
        private bool _selfPrimed;

        public bool HasLegs => _chainCount > 0;
        public int LegCount => _chainCount;
        public LegGaitSolver Solver => _solver;

        public bool SelfDriven
        {
            get => _selfDriven;
            set => _selfDriven = value;
        }

        public LegGaitSettings Settings
        {
            get => _settings;
            set
            {
                _settings = value;
                _solver.Settings = value;
            }
        }

        /// <summary>Smoothed body bob (m, along the space's up) from the footfalls; add it to the body pose.</summary>
        public float BodyBob => _bob;

        /// <summary>Smoothed body (pitch, roll) in degrees from the footfalls; add them to the body pose.</summary>
        public Vector2 BodySway => _sway;

        /// <summary>Feet that touched down during the last <see cref="Advance"/>, the strongest landing (0..1) and where.</summary>
        public int Landings => _solver.Landings;
        public float LandingStrength => _solver.LandingStrength;
        public Vector3 LandingPoint => _solver.LandingPoint;

        /// <summary>Mean leg length (m) of the bound chains, 0 without legs.</summary>
        public float MeanLegLength { get; private set; }

        /// <summary>How far (m) the owner should lower the body below its rest pose while the legs step at <paramref name="speed01"/> (0..1 of full gait speed).</summary>
        public float KneeBendDrop(float speed01) => Mathf.Lerp(_kneeBend, Mathf.Max(_kneeBend, _runKneeBend), Mathf.Clamp01(speed01)) * MeanLegLength;

        /// <summary>
        /// Highest body offset (m, up, relative to the rest pose) at which every planted foot is still within
        /// reach of its hip — clamp the body's vertical pose to it so bobs, hops and lunges never drag a
        /// planted foot (the classic pelvis-height clamp). Drops at once, recovers smoothly; large when no foot is down.
        /// </summary>
        public float BodyHeightLimit => _bodyLimit;

        /// <summary>
        /// Additive (night-models pass): 0..1 multiplier on the IK weight of the front legs only (rest z in the front
        /// half). Below 1 the forelegs let go of their planted feet and ride the body at their rest pose, so an animal can
        /// rear up on its hind legs (the werewolf's strike, posed by <c>ProceduralEnemyRig</c> pitching the part the
        /// forelegs hang from); they also stop dragging the hips' reactive step. 1 (default) = unchanged behaviour.
        /// </summary>
        public float FrontLegsWeight
        {
            get => _frontLegsWeight;
            set => _frontLegsWeight = Mathf.Clamp01(value);
        }

        private float _frontLegsWeight = 1f;

        /// <summary>Gait style picked from the leg count by <see cref="Bind"/> (2 biped, 4 quadruped, more arthropod).</summary>
        public LegGaitStyle DetectedStyle { get; private set; }

        /// <summary>
        /// Finds the leg chains and follow-through parts under <paramref name="body"/> (the posed visual) and
        /// captures their rest pose; <paramref name="space"/> is the transform that actually moves (the feet's
        /// rest targets ride on it, yaw only) and <paramref name="groundLocalY"/> its local height of the ground
        /// under the feet. Parts named in <paramref name="skipFollow"/> are left to another animator. The body
        /// must be at its rest pose. Returns <see cref="HasLegs"/>.
        /// </summary>
        public bool Bind(Transform space, Transform body, float groundLocalY, IList<string> skipFollow = null)
        {
            RestoreRest();
            _space = space != null ? space : transform.parent;
            _body = body != null ? body : transform;
            _chainCount = 0;
            _follow.Clear();
            _solver.Clear();
            _bound = _space != null;
            if (!_bound)
            {
                return false;
            }

            foreach (Transform t in _body.GetComponentsInChildren<Transform>(true))
            {
                if (t == _body)
                {
                    continue;
                }

                string n = t.name;
                if (n.StartsWith(HipPrefix) && _chainCount < LegGaitSolver.MaxLegs)
                {
                    AddChain(t, n.Substring(HipPrefix.Length));
                }
                else if (skipFollow == null || !skipFollow.Contains(n))
                {
                    AddFollow(t, n);
                }
            }

            SortChains();
            NormalizeTailGains();
            DetectedStyle = _chainCount <= 2 ? LegGaitStyle.Biped : _chainCount == 4 ? LegGaitStyle.Quadruped : LegGaitStyle.Arthropod;

            // Rest targets live in the root's yaw frame, unscaled (world metres), like the solver.
            float groundY = (_space.TransformPoint(new Vector3(0f, groundLocalY, 0f)) - _space.position).y;
            float total = 0f;
            float halfDepth = 0f;
            for (int i = 0; i < _chainCount; i++)
            {
                halfDepth = Mathf.Max(halfDepth, Mathf.Abs(_chains[i].RestInSpace.z));
            }

            for (int i = 0; i < _chainCount; i++)
            {
                total += _chains[i].Length;
            }

            MeanLegLength = _chainCount > 0 ? total / _chainCount : 0f;
            for (int i = 0; i < _chainCount; i++)
            {
                ref Chain c = ref _chains[i];
                int front = DetectedStyle == LegGaitStyle.Biped || halfDepth < 0.01f ? 0 : (c.RestInSpace.z > halfDepth * 0.25f ? 1 : (c.RestInSpace.z < -halfDepth * 0.25f ? -1 : 0));
                c.Front = front;
                _solver.AddLeg(c.RestInSpace, groundY, c.Length, c.Side, front, GeometricReach(c, KneeBendDrop(1f)));
            }

            _solver.Settings = _settings;
            _solver.Probe = ProbeGround;
            ResetState();
            return HasLegs;
        }

        private void AddChain(Transform hip, string id)
        {
            Transform knee = FindChild(hip, KneePrefix + id);
            Transform foot = knee != null ? FindChild(knee, FootPrefix + id) : null;

            var c = new Chain
            {
                Hip = hip,
                Knee = foot != null ? knee : null,
                Foot = foot,
                HipRest = hip.localRotation,
                KneeRest = knee != null ? knee.localRotation : Quaternion.identity,
                FootRest = foot != null ? foot.localRotation : Quaternion.identity,
            };

            Vector3 end;
            if (foot != null)
            {
                end = foot.position;
                c.Length = Vector3.Distance(hip.position, knee.position) + Vector3.Distance(knee.position, foot.position);
                c.FootInSpace = Quaternion.Inverse(YawOnly(_space.rotation)) * foot.rotation;

                // Bend direction = where the rest knee sits off the hip → foot line (spiders: up, dogs' hocks: back).
                Vector3 line = end - hip.position;
                Vector3 offset = knee.position - hip.position;
                Vector3 bend = line.sqrMagnitude > 1e-8f ? offset - line * (Vector3.Dot(offset, line) / line.sqrMagnitude) : Vector3.zero;
                c.PoleLocal = bend.magnitude > c.Length * 0.02f
                    ? _body.InverseTransformDirection(bend.normalized)
                    : Vector3.forward;
            }
            else
            {
                // One-bone leg: its foot is the bottom of its mesh, straight below the pivot.
                end = hip.position + Vector3.down * 0.5f;
                MeshFilter filter = hip.GetComponent<MeshFilter>();
                if (filter != null && filter.sharedMesh != null)
                {
                    Bounds b = filter.sharedMesh.bounds;
                    end = hip.TransformPoint(new Vector3(b.center.x, b.min.y, b.center.z));
                }

                c.EndInHip = hip.InverseTransformPoint(end);
                c.Length = Mathf.Max(0.05f, Vector3.Distance(hip.position, end));
            }

            Quaternion toSpace = Quaternion.Inverse(YawOnly(_space.rotation));
            c.RestInSpace = toSpace * (end - _space.position);
            c.HipInSpace = toSpace * (hip.position - _space.position);
            c.Side = c.RestInSpace.x < 0f ? -1 : 1;
            _chains[_chainCount++] = c;
        }

        /// <summary>
        /// How far (m) the foot may drift from its rest point and still be reached with the hip lowered by
        /// <paramref name="drop"/>: the worst case is straight away from the hip, so the reach is the leg's
        /// horizontal span at that height minus the rest's own horizontal offset (spiders' feet are far out).
        /// </summary>
        private static float GeometricReach(in Chain c, float drop)
        {
            Vector3 hipToFoot = c.RestInSpace - c.HipInSpace;
            float height = Mathf.Max(0.01f, -hipToFoot.y - drop);
            float span = c.Length * 0.97f;
            float horizontal = span > height ? Mathf.Sqrt(span * span - height * height) : 0f;
            float offset = new Vector2(hipToFoot.x, hipToFoot.z).magnitude;
            return Mathf.Max(c.Length * 0.08f, horizontal - offset);
        }

        private void AddFollow(Transform t, string n)
        {
            FollowKind kind;
            float gain = 1f;
            if (n == "Tail" || n.StartsWith("Tail_") || n == "Stinger")
            {
                kind = FollowKind.Tail;
                int depth = 0;
                for (Transform p = t.parent; p != null && p != _body; p = p.parent)
                {
                    depth += p.name.StartsWith("Tail") ? 1 : 0;
                }

                gain = 0.55f + 0.2f * depth;
            }
            else if (n == "Head")
            {
                kind = FollowKind.Head;
            }
            else if (n == "Ear_L" || n == "Ear_R")
            {
                kind = FollowKind.Ear;
                gain = n == "Ear_L" ? 1f : -1f;
            }
            else if (n == "Abdomen")
            {
                kind = FollowKind.Abdomen;
            }
            else if (n == "Claw_L" || n == "Claw_R")
            {
                kind = FollowKind.Claw;
                gain = n == "Claw_L" ? 1f : -1f;
            }
            else
            {
                return;
            }

            _follow.Add(new Follow { Transform = t, Rest = t.localRotation, Kind = kind, Gain = gain });
        }

        /// <summary>Horizontal offset of the posed hips from their rest places on the root (lunge, surge, recoil).</summary>
        private Vector3 HipShift()
        {
            if (_chainCount == 0)
            {
                return Vector3.zero;
            }

            Quaternion yaw = YawOnly(_space.rotation);
            Vector3 sum = Vector3.zero;
            float count = 0f;
            for (int i = 0; i < _chainCount; i++)
            {
                ref Chain c = ref _chains[i];
                if (c.Hip != null)
                {
                    // Forelegs lifted off the ground (FrontLegsWeight < 1) no longer pull the planted feet along.
                    float k = c.Front > 0 ? _frontLegsWeight : 1f;
                    sum += (c.Hip.position - (_space.position + yaw * c.HipInSpace)) * k;
                    count += k;
                }
            }

            if (count <= 0f)
            {
                return Vector3.zero;
            }

            sum /= count;
            return new Vector3(sum.x, 0f, sum.z);
        }

        /// <summary>A long tail (scorpion: five joints) shares the swing so the whole chain curls about as much as a short one.</summary>
        private void NormalizeTailGains()
        {
            int segments = 0;
            foreach (Follow f in _follow)
            {
                segments += f.Kind == FollowKind.Tail ? 1 : 0;
            }

            if (segments <= 2)
            {
                return;
            }

            float scale = 2f / segments;
            for (int i = 0; i < _follow.Count; i++)
            {
                Follow f = _follow[i];
                if (f.Kind == FollowKind.Tail)
                {
                    f.Gain *= scale;
                    _follow[i] = f;
                }
            }
        }

        private static Transform FindChild(Transform parent, string childName)
        {
            for (int i = 0; i < parent.childCount; i++)
            {
                Transform child = parent.GetChild(i);
                if (child.name == childName)
                {
                    return child;
                }
            }

            return null;
        }

        /// <summary>Insertion sort into the gait order: front to back (rest z descending, paired within 5 cm), left before right.</summary>
        private void SortChains()
        {
            for (int i = 1; i < _chainCount; i++)
            {
                Chain key = _chains[i];
                int j = i - 1;
                while (j >= 0 && Before(key, _chains[j]))
                {
                    _chains[j + 1] = _chains[j];
                    j--;
                }

                _chains[j + 1] = key;
            }
        }

        private static bool Before(in Chain a, in Chain b)
        {
            float dz = a.RestInSpace.z - b.RestInSpace.z;
            if (Mathf.Abs(dz) > 0.05f)
            {
                return dz > 0f;
            }

            return a.Side < b.Side;
        }

        /// <summary>Re-plants the feet on their rest targets and clears the springs (spawn, pool reuse, LOD wake-up).</summary>
        public void ResetState()
        {
            _sway = _swayVelocity = Vector2.zero;
            _bob = _bobVelocity = 0f;
            _bodyLimit = BodyLimitSlack;
            _bodyShift = Vector3.zero;
            _hasLastBody = false;
            _selfPrimed = false;
            for (int i = 0; i < _follow.Count; i++)
            {
                Follow f = _follow[i];
                f.Angle = f.Velocity = Vector2.zero;
                _follow[i] = f;
            }

            if (_bound && _chainCount > 0)
            {
                _solver.Reset(_space.position, YawOnly(_space.rotation));
            }
        }

        /// <summary>Puts every bound part back at its rest pose.</summary>
        public void RestoreRest()
        {
            for (int i = 0; i < _chainCount; i++)
            {
                ref Chain c = ref _chains[i];
                if (c.Hip != null) c.Hip.localRotation = c.HipRest;
                if (c.Knee != null) c.Knee.localRotation = c.KneeRest;
                if (c.Foot != null) c.Foot.localRotation = c.FootRest;
            }

            foreach (Follow f in _follow)
            {
                if (f.Transform != null)
                {
                    f.Transform.localRotation = f.Rest;
                }
            }
        }

        /// <summary>
        /// Advances the feet for a body moving at <paramref name="velocity"/> (world m/s) and turning at
        /// <paramref name="yawRateDeg"/>; call before posing the body. <paramref name="motionScale"/> scales
        /// the sway overshoot ("Reduzir Movimento"); the stepping itself is never reduced (it is what keeps
        /// the feet from sliding).
        /// </summary>
        public void Advance(float deltaTime, Vector3 velocity, float yawRateDeg, float motionScale = 1f)
        {
            if (!_bound || deltaTime <= 0f)
            {
                return;
            }

            _velocity = velocity;
            _yawRate = yawRateDeg;
            if (_chainCount > 0)
            {
                // The feet follow where the posed body actually is (lunges, recoil shoves: a reactive step),
                // as of the last Solve; the vertical pose is the body-height clamp's business.
                _solver.Step(deltaTime, _space.position + _bodyShift, YawOnly(_space.rotation), velocity, yawRateDeg);
                UpdateBodyLimit(deltaTime);
            }

            // Sway: underdamped springs → the body settles onto each footfall with a little follow-through.
            float k = Mathf.Clamp01(motionScale);
            float hz = _solver.Settings.Style == LegGaitStyle.Biped ? 3.5f : 6f;
            Vector2 swayTarget = new Vector2(_solver.Pitch, _solver.Roll) * k;
            MotionFeelMath.StepSpring(ref _sway, ref _swayVelocity, swayTarget, hz, 0.45f, deltaTime, Vector2.zero);
            MotionFeelMath.StepSpring(ref _bob, ref _bobVelocity, _solver.Bob * k, hz * 1.4f, 0.5f, deltaTime);
        }

        private void UpdateBodyLimit(float deltaTime)
        {
            Quaternion yaw = YawOnly(_space.rotation);
            Vector3 origin = _space.position + _bodyShift;
            float limit = BodyLimitSlack;
            for (int i = 0; i < _chainCount; i++)
            {
                if (_solver.IsSwinging(i))
                {
                    continue;
                }

                ref Chain c = ref _chains[i];
                Vector3 hip = origin + yaw * c.HipInSpace;
                Vector3 foot = _solver.FootPosition(i);
                float span = c.Length * 0.96f; // slack for the body pitch/roll the clamp does not model
                float d = LegGaitMath.PlanarDistance(hip, foot);
                float highest = d < span ? Mathf.Sqrt(span * span - d * d) : 0f;
                limit = Mathf.Min(limit, highest - (hip.y - foot.y));
            }

            limit = Mathf.Max(limit, -0.3f * MeanLegLength); // a foot that far behind steps next frame anyway
            _bodyLimit = limit < _bodyLimit ? limit : Mathf.MoveTowards(_bodyLimit, limit, BodyLimitRiseSpeed * deltaTime);
        }

        /// <summary>
        /// Writes the leg rotations for the current feet (call after the body was posed this frame) blended
        /// by <paramref name="weight"/> (0 = rest pose, legs ride the body), then steps the follow-through
        /// springs (scaled by <paramref name="motionScale"/>).
        /// </summary>
        public void Solve(float deltaTime, float weight, float motionScale = 1f)
        {
            if (!_bound)
            {
                return;
            }

            float w = Mathf.Clamp01(weight);
            _bodyShift = HipShift() * w;
            for (int i = 0; i < _chainCount; i++)
            {
                SolveChain(ref _chains[i], i, _chains[i].Front > 0 ? w * _frontLegsWeight : w);
            }

            SolveFollow(deltaTime, w * Mathf.Clamp01(motionScale));
        }

        private void SolveChain(ref Chain c, int index, float w)
        {
            if (c.Hip == null)
            {
                return;
            }

            c.Hip.localRotation = c.HipRest;
            if (c.Knee != null)
            {
                c.Knee.localRotation = c.KneeRest;
                c.Foot.localRotation = c.FootRest;
            }

            if (w <= 0f)
            {
                return;
            }

            Vector3 target = _solver.FootPosition(index);
            Vector3 hip = c.Hip.position;
            if (c.Knee != null)
            {
                Vector3 restKnee = c.Knee.position;
                Vector3 restFoot = c.Foot.position;
                Vector3 goal = Vector3.Lerp(restFoot, target, w);
                float upper = (restKnee - hip).magnitude;
                float lower = (restFoot - restKnee).magnitude;
                Vector3 pole = _body.TransformDirection(c.PoleLocal);
                LegGaitMath.SolveTwoBone(hip, goal, upper, lower, pole, out Vector3 joint, out Vector3 end);

                c.Hip.rotation = Quaternion.FromToRotation(restKnee - hip, joint - hip) * c.Hip.rotation;
                Vector3 knee = c.Knee.position;
                c.Knee.rotation = Quaternion.FromToRotation(c.Foot.position - knee, end - knee) * c.Knee.rotation;

                // Feet stay flat on the ground (yaw with the root, tilted to the slope), whatever the leg angle.
                Quaternion level = Quaternion.FromToRotation(Vector3.up, _solver.GroundNormal(index)) * (YawOnly(_space.rotation) * c.FootInSpace);
                c.Foot.rotation = Quaternion.Slerp(c.Foot.rotation, level, w);
            }
            else
            {
                Vector3 restEnd = c.Hip.TransformPoint(c.EndInHip);
                Vector3 goal = Vector3.Lerp(restEnd, target, w);
                c.Hip.rotation = Quaternion.FromToRotation(restEnd - hip, goal - hip) * c.Hip.rotation;
            }
        }

        private void SolveFollow(float deltaTime, float k)
        {
            if (_follow.Count == 0 || deltaTime <= 0f)
            {
                return;
            }

            Vector3 bodyPosition = _body.position;
            float vertical = _hasLastBody ? (bodyPosition.y - _lastBodyPosition.y) / deltaTime : 0f;
            _lastBodyPosition = bodyPosition;
            _hasLastBody = true;
            _bodyVerticalSpeed = Mathf.Lerp(_bodyVerticalSpeed, Mathf.Clamp(vertical, -4f, 4f), Mathf.Clamp01(deltaTime * 20f));

            float yaw = Mathf.Clamp(_yawRate, -360f, 360f);
            Vector3 localVelocity = _space.InverseTransformDirection(_velocity);
            float forward = Mathf.Clamp(localVelocity.z, -8f, 8f);
            float vy = _bodyVerticalSpeed;

            for (int i = 0; i < _follow.Count; i++)
            {
                Follow f = _follow[i];
                if (f.Transform == null)
                {
                    continue;
                }

                Vector2 target; // (pitch, yaw) degrees
                float hz, damping;
                switch (f.Kind)
                {
                    case FollowKind.Tail:
                        // Trails the turns, flicks with the bounce, streams back with speed.
                        target = new Vector2(Mathf.Clamp(-vy * 14f - forward * 2.5f, -30f, 30f), Mathf.Clamp(-yaw * 0.09f, -35f, 35f)) * f.Gain;
                        hz = 2.4f;
                        damping = 0.3f;
                        break;
                    case FollowKind.Head:
                        // Steadies against the body pitch (gaze stabilisation) and leads into turns.
                        target = new Vector2(Mathf.Clamp(-_sway.x * 0.7f + vy * 3f, -12f, 12f), Mathf.Clamp(yaw * 0.05f, -15f, 15f));
                        hz = 3.2f;
                        damping = 0.55f;
                        break;
                    case FollowKind.Ear:
                        // Flop on every footfall, flatten back with speed.
                        target = new Vector2(Mathf.Clamp(-vy * 22f - forward * 3f, -35f, 25f), 0f);
                        hz = 5f;
                        damping = 0.22f;
                        break;
                    case FollowKind.Abdomen:
                        target = new Vector2(Mathf.Clamp(-vy * 18f, -15f, 15f), Mathf.Clamp(-yaw * 0.06f, -18f, 18f));
                        hz = 3f;
                        damping = 0.35f;
                        break;
                    default: // Claw
                        target = new Vector2(Mathf.Clamp(-vy * 10f - forward * 1.5f, -14f, 14f), Mathf.Clamp(-yaw * 0.03f, -10f, 10f) * f.Gain);
                        hz = 3.5f;
                        damping = 0.4f;
                        break;
                }

                MotionFeelMath.StepSpring(ref f.Angle, ref f.Velocity, target * k, hz, damping, deltaTime, Vector2.zero);
                Vector2 a = f.Angle;
                if (f.Kind == FollowKind.Ear)
                {
                    // Ears flop outward (roll) rather than pitch.
                    f.Transform.localRotation = Quaternion.Euler(a.x * 0.3f, 0f, a.x * 0.7f * f.Gain) * f.Rest;
                }
                else
                {
                    f.Transform.localRotation = Quaternion.Euler(a.x, a.y, 0f) * f.Rest;
                }

                _follow[i] = f;
            }
        }

        private void LateUpdate()
        {
            if (!_selfDriven || !_bound)
            {
                return;
            }

            float dt = Time.deltaTime;
            if (dt <= 0f)
            {
                return;
            }

            Vector3 position = _space.position;
            float yaw = _space.eulerAngles.y;
            Vector3 velocity = Vector3.zero;
            float yawRate = 0f;
            if (_selfPrimed)
            {
                velocity = (position - _selfLastPosition) / dt;
                yawRate = Mathf.DeltaAngle(_selfLastYaw, yaw) / dt;
            }

            _selfPrimed = true;
            _selfLastPosition = position;
            _selfLastYaw = yaw;
            Advance(dt, velocity, yawRate);
            Solve(dt, 1f);
        }

        private void OnDisable()
        {
            RestoreRest();
        }

        /// <summary>Ground query for the solver: nearest floor-like hit that isn't this body, a character or a rigidbody; budgeted.</summary>
        private bool ProbeGround(Vector3 point, float up, float down, out Vector3 ground, out Vector3 normal)
        {
            ground = point;
            normal = Vector3.up;
            if (!TryConsumeRaycast())
            {
                return false;
            }

            int count = Physics.RaycastNonAlloc(point + Vector3.up * up, Vector3.down, Hits, up + down, _groundMask, QueryTriggerInteraction.Ignore);
            float best = float.PositiveInfinity;
            for (int i = 0; i < count; i++)
            {
                RaycastHit hit = Hits[i];
                Collider collider = hit.collider;
                if (hit.distance >= best || collider == null || collider is CharacterController || collider.attachedRigidbody != null
                    || hit.normal.y < 0.5f || (_space != null && collider.transform.IsChildOf(_space)))
                {
                    continue;
                }

                best = hit.distance;
                ground = hit.point;
                normal = hit.normal;
            }

            return !float.IsPositiveInfinity(best);
        }

        /// <summary>Takes one raycast from the shared per-frame budget; false when this frame's budget is spent (the foot uses the flat feet plane).</summary>
        public static bool TryConsumeRaycast()
        {
            int frame = Time.frameCount;
            if (frame != _budgetFrame)
            {
                _budgetFrame = frame;
                _budgetUsed = 0;
            }

            if (_budgetUsed >= MaxRaycastsPerFrame)
            {
                return false;
            }

            _budgetUsed++;
            return true;
        }

        /// <summary>Squared distance from the main camera (cached once per frame); +∞ without a camera.</summary>
        public static float SqrDistanceToCamera(Vector3 position)
        {
            int frame = Time.frameCount;
            if (frame != _cameraFrame || _camera == null)
            {
                _cameraFrame = frame;
                Camera main = Camera.main;
                _camera = main != null ? main.transform : null;
            }

            return _camera != null ? (_camera.position - position).sqrMagnitude : float.PositiveInfinity;
        }

        private static Quaternion YawOnly(Quaternion rotation)
        {
            Vector3 forward = rotation * Vector3.forward;
            forward.y = 0f;
            return forward.sqrMagnitude > 1e-8f ? Quaternion.LookRotation(forward, Vector3.up) : Quaternion.identity;
        }
    }
}
