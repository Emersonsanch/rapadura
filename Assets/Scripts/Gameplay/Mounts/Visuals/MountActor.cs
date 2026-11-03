using Rapadura.Core.Accessibility;
using Rapadura.Core.DI;
using Rapadura.Gameplay.Locomotion;
using UnityEngine;

namespace Rapadura.Gameplay.Mounts.Visuals
{
    /// <summary>
    /// A mount in the world: its procedural model (<see cref="MountModels"/>) animated by the shared leg gait engine
    /// (<see cref="ProceduralLegLocomotion"/>, driven here like <c>EnemyProceduralAnimator</c> does: Advance → pose the
    /// body → Solve), plus hoof/paw sounds on the footfalls and the cowbell swinging. It either rides under a rider
    /// (<see cref="AttachTo"/>, the rider's motor moves it) or walks on its own (<see cref="RunTo"/>: coming at the
    /// whistle, going home, waiting at the edge of the arena). Velocity is measured from its own motion, so both cases
    /// animate the same. If the model has no legs the engine can bind (or the engine is missing), it falls back to a
    /// simple bob. "Reduzir Movimento" halves the sway/bob/bell (the stepping itself is kept: it stops foot sliding).
    /// Runs after the rider's animators (order 120) so the seat the rider reads is this frame's.
    /// </summary>
    [DefaultExecutionOrder(120)]
    [DisallowMultipleComponent]
    public class MountActor : MonoBehaviour
    {
        private const float ReducedMotionScale = AccessibilitySettings.ReducedMotionScale;

        private MountDefinition _definition;
        private Transform _visual;
        private Transform _seat;
        private Transform _bell;
        private ProceduralLegLocomotion _legs;
        private QuadrupedShape _shape;
        private float _scale = 1f;

        private Vector3 _lastPosition;
        private float _lastYaw;
        private bool _primed;
        private Vector3 _velocity;
        private float _speed;
        private float _bobPhase;
        private float _bellAngle;
        private float _bellVelocity;

        private Transform _follow;
        private float _followDown;

        // Flyers (MountType.Voador, MountFlightRules): hover, wing beat, legs tucked. Visual only.
        private bool _flyer;
        private Transform _wingLeft;
        private Transform _wingRight;
        private Transform _wingTipLeft;
        private Transform _wingTipRight;
        private Transform[] _legHips;
        private float _beatPhase;
        private float _hover = -1f;
        private float _soarRemaining;
        private float _soarDuration;

        private bool _hasTarget;
        private Vector3 _target;
        private float _runSpeed;
        private System.Action _onArrive;

        public MountDefinition Definition => _definition;

        /// <summary>The owned mount this actor shows (its cosmetics), or null.</summary>
        public string OwnedGuid { get; private set; }

        /// <summary>Saddle + blanket worn when built; a different one needs a respawn.</summary>
        public string CosmeticSignature { get; private set; }

        public static string Signature(OwnedMount owned) => owned == null ? string.Empty : owned.saddleId + "|" + owned.blanketId;
        public Transform Visual => _visual;

        /// <summary>The saddle pivot (rider hips), or the actor itself.</summary>
        public Transform Seat => _seat != null ? _seat : transform;

        public bool HasLegRig => _legs != null && _legs.HasLegs;
        public bool IsRunning => _hasTarget;
        public float Speed => _speed;

        /// <summary>Galloping look (more pitch, longer bounds) — set by the rider controller.</summary>
        public bool Galloping { get; set; }

        /// <summary>Saddle height above the ground in world units at the current scale.</summary>
        public float SeatHeight => _shape.SeatHeight * _scale;

        /// <summary>Plays hoof/paw sounds (off for the map token, which is far and tiny).</summary>
        public bool StepSounds { get; set; } = true;

        /// <summary>A flyer: hovers and beats its wings instead of walking (<see cref="MountFlightRules"/>).</summary>
        public bool IsFlyer => _flyer;

        /// <summary>Flyers fly high here (false in cities and houses: low glide). Set by the rider controller.</summary>
        public bool FlightAllowed { get; set; } = true;

        /// <summary>Current height of the flyer's model above its root (world units), 0 for walkers.</summary>
        public float HoverHeight => _flyer && _hover > 0f ? _hover * _scale : 0f;

        /// <summary>Whether the "Voo da Coluna" arc is running.</summary>
        public bool IsSoaring => _soarRemaining > 0f;

        /// <summary>Creates a mount actor (root + model) at <paramref name="position"/>.</summary>
        public static MountActor Spawn(MountDefinition definition, Vector3 position, Quaternion rotation, float scale = 1f, Transform parent = null,
            OwnedMount owned = null)
        {
            var root = new GameObject("Mount_" + (definition != null ? definition.MountId : "unknown"));
            if (parent != null)
            {
                root.transform.SetParent(parent, false);
            }

            root.transform.SetPositionAndRotation(position, rotation);
            root.transform.localScale = Vector3.one * Mathf.Max(0.01f, scale);
            var actor = root.AddComponent<MountActor>();
            actor.Build(definition, scale, owned);
            return actor;
        }

        private void Build(MountDefinition definition, float scale, OwnedMount owned)
        {
            _definition = definition;
            _scale = Mathf.Max(0.01f, scale);
            MountVisualRecipe recipe = definition != null ? definition.VisualRecipe : MountVisualRecipe.Pony;
            _shape = MountRecipe.ShapeFor(recipe);
            MountCosmeticSpec saddle = owned != null ? MountCosmetics.Get(owned.saddleId) : null;
            MountCosmeticSpec blanket = owned != null ? MountCosmetics.Get(owned.blanketId) : null;
            GameObject visual = MountModels.Instantiate(recipe, transform, saddle?.Swatch, blanket?.Swatch);
            OwnedGuid = owned?.guid;
            CosmeticSignature = Signature(owned);
            _visual = visual.transform;
            _seat = FindDeep(_visual, MountParts.Seat);
            _bell = FindDeep(_visual, MountParts.Bell);

            _flyer = definition != null && definition.Type == MountType.Voador && _shape.FlightWings;
            if (_flyer)
            {
                // Flyers never touch the ground: no gait engine (it would plant the feet), the legs fold under the body.
                _wingLeft = FindDeep(_visual, MountParts.WingLeft);
                _wingRight = FindDeep(_visual, MountParts.WingRight);
                _wingTipLeft = FindDeep(_visual, MountParts.WingTipLeft);
                _wingTipRight = FindDeep(_visual, MountParts.WingTipRight);
                _legHips = new[]
                {
                    FindDeep(_visual, "Leg_FL"), FindDeep(_visual, "Leg_FR"), FindDeep(_visual, "Leg_HL"), FindDeep(_visual, "Leg_HR"),
                };
                _lastPosition = transform.position;
                _lastYaw = transform.eulerAngles.y;
                return;
            }

            _legs = visual.AddComponent<ProceduralLegLocomotion>();
            _legs.SelfDriven = false;
            if (!_legs.Bind(transform, _visual, 0f) || !_legs.HasLegs)
            {
                // Engine could not bind this model: keep the cheap bob only.
                Destroy(_legs);
                _legs = null;
            }
            else
            {
                _legs.Settings = MountGait.SettingsFor(_legs.MeanLegLength, 0f, _scale, _legs.DetectedStyle);
                _legs.ResetState();
            }

            _lastPosition = transform.position;
            _lastYaw = transform.eulerAngles.y;
        }

        /// <summary>Rides under <paramref name="rider"/> (local pose zeroed): the rider's motor now moves the mount.</summary>
        public void AttachTo(Transform rider, Vector3 localOffset)
        {
            _hasTarget = false;
            _onArrive = null;
            transform.SetParent(rider, true);
            transform.localPosition = localOffset;
            transform.localRotation = Quaternion.identity;
            ResetLegs();
        }

        /// <summary>Leaves the rider and stands on its own where it is (scene root).</summary>
        public void Detach()
        {
            transform.SetParent(null, true);
            Vector3 euler = transform.eulerAngles;
            transform.rotation = Quaternion.Euler(0f, euler.y, 0f);
        }

        /// <summary>Walks/gallops to <paramref name="target"/> at <paramref name="speed"/>, then calls <paramref name="onArrive"/>.</summary>
        public void RunTo(Vector3 target, float speed, System.Action onArrive = null)
        {
            _target = target;
            _runSpeed = Mathf.Max(0.1f, speed);
            _onArrive = onArrive;
            _hasTarget = true;
        }

        public void Stop()
        {
            _hasTarget = false;
            _onArrive = null;
        }

        /// <summary>
        /// Rides under a transform it must not be parented to (the map token is scaled): every frame it snaps to
        /// <paramref name="target"/> minus <paramref name="downOffset"/> on the up axis, yaw only. Null stops following.
        /// </summary>
        public void Follow(Transform target, float downOffset)
        {
            _follow = target;
            _followDown = downOffset;
            _hasTarget = false;
            _onArrive = null;
            if (target != null)
            {
                SnapToFollow();
                ResetLegs();
            }
        }

        private void SnapToFollow()
        {
            transform.SetPositionAndRotation(_follow.position + Vector3.down * _followDown, Quaternion.Euler(0f, _follow.eulerAngles.y, 0f));
        }

        public void ResetLegs()
        {
            _primed = false;
            _legs?.ResetState();
        }

        /// <summary>"Voo da Coluna": the flyer climbs in an arc for <paramref name="seconds"/> and comes back down (look only).</summary>
        public void Soar(float seconds)
        {
            if (!_flyer)
            {
                return;
            }

            _soarDuration = Mathf.Max(0.1f, seconds);
            _soarRemaining = _soarDuration;
        }

        private void Update()
        {
            if (!_hasTarget)
            {
                return;
            }

            float dt = Time.deltaTime;
            Vector3 position = transform.position;
            Vector3 to = _target - position;
            to.y = 0f;
            float distance = to.magnitude;
            if (distance <= Mathf.Max(0.05f, _runSpeed * dt))
            {
                transform.position = new Vector3(_target.x, _target.y, _target.z);
                _hasTarget = false;
                System.Action arrive = _onArrive;
                _onArrive = null;
                arrive?.Invoke();
                return;
            }

            Vector3 direction = to / distance;
            float step = _runSpeed * dt;
            Vector3 next = position + direction * step;
            next.y = Mathf.MoveTowards(position.y, _target.y, step);
            transform.position = next;
            Quaternion facing = Quaternion.LookRotation(direction, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, facing, 540f * dt);
        }

        private void LateUpdate()
        {
            float dt = Time.deltaTime;
            if (dt <= 0f || _visual == null)
            {
                return;
            }

            if (_follow != null)
            {
                SnapToFollow();
            }

            Vector3 position = transform.position;
            float yaw = transform.eulerAngles.y;
            float yawRate = 0f;
            if (_primed)
            {
                Vector3 raw = (position - _lastPosition) / dt;
                raw.y = 0f;
                _velocity = Vector3.Lerp(_velocity, raw, 1f - Mathf.Exp(-dt * 14f));
                yawRate = Mathf.DeltaAngle(_lastYaw, yaw) / dt;
            }

            _primed = true;
            _lastPosition = position;
            _lastYaw = yaw;
            _speed = _velocity.magnitude;

            bool reduced = ServiceLocator.TryGet(out AccessibilitySettings accessibility) && accessibility != null && accessibility.ReduceMotion;
            float motion = reduced ? ReducedMotionScale : 1f;
            float worldSpeed01 = Mathf.Clamp01(_speed / (MountRules.HeroRunSpeed * MountRules.MaxRideMultiplier * _scale));

            if (_flyer)
            {
                TickFlight(dt, reduced, worldSpeed01, position);
            }
            else if (_legs != null)
            {
                _legs.Settings = MountGait.SettingsFor(_legs.MeanLegLength, Galloping ? 1f : 0f, _scale, _legs.DetectedStyle);
                _legs.Advance(dt, _velocity, yawRate, motion);
                float offset = _legs.BodyBob - _legs.KneeBendDrop(worldSpeed01);
                offset = Mathf.Min(offset, _legs.BodyHeightLimit);
                Vector2 sway = _legs.BodySway;
                // Local units: the visual sits under the scaled root.
                _visual.localPosition = new Vector3(0f, offset / _scale, 0f);
                _visual.localRotation = Quaternion.Euler(sway.x, 0f, sway.y);
                _legs.Solve(dt, 1f, motion);

                if (StepSounds && _legs.Landings > 0 && _speed > 0.4f && _definition != null)
                {
                    MountAudio.Play(MountAudio.StepCueFor(_definition.VisualRecipe), position);
                }
            }
            else
            {
                // Fallback: a plain trot bob and a little pitch, faster with speed.
                _bobPhase += dt * Mathf.Lerp(0f, 9f, Mathf.Clamp01(_speed / (3f * _scale)));
                float bob = Mathf.Abs(Mathf.Sin(_bobPhase)) * 0.05f * motion * Mathf.Clamp01(_speed);
                _visual.localPosition = new Vector3(0f, bob, 0f);
                _visual.localRotation = Quaternion.Euler(Mathf.Sin(_bobPhase * 2f) * 2.5f * motion * Mathf.Clamp01(_speed), 0f, 0f);
            }

            if (_bell != null)
            {
                // The chocalho swings back with acceleration and rings on big swings.
                float push = -Vector3.Dot(transform.InverseTransformDirection(_velocity), Vector3.forward) * 4f;
                _bellVelocity += (-_bellAngle * 60f - _bellVelocity * 5f + push) * dt;
                _bellAngle = Mathf.Clamp(_bellAngle + _bellVelocity * dt * motion, -35f, 35f);
                _bell.localRotation = Quaternion.Euler(_bellAngle, 0f, 0f);
                if (StepSounds && Mathf.Abs(_bellVelocity) > 140f && _speed > 2f)
                {
                    MountAudio.Play(MountAudio.Cowbell, _bell.position);
                }
            }
        }

        /// <summary>Hover, bob, pitch with speed, wing beat (tips lagging), legs tucked and a wing "whoomp" on each downstroke.</summary>
        private void TickFlight(float dt, bool reduced, float speed01, Vector3 position)
        {
            bool soaring = _soarRemaining > 0f;
            float soar01 = -1f;
            if (soaring)
            {
                _soarRemaining = Mathf.Max(0f, _soarRemaining - dt);
                soar01 = 1f - _soarRemaining / _soarDuration;
            }

            float target = MountFlightRules.HoverHeight(FlightAllowed, speed01, soar01);
            // The soar arc is followed exactly; the rest eases (take-off on spawn, landing in a city).
            _hover = _hover < 0f || soaring ? target : Mathf.Lerp(_hover, target, 1f - Mathf.Exp(-dt * MountFlightRules.HeightEase));

            float previous = _beatPhase;
            _beatPhase += dt * MountFlightRules.BeatsPerSecond(speed01, soaring);
            if (_beatPhase > 1000f)
            {
                _beatPhase -= 1000f;
                previous -= 1000f;
            }

            float amplitude = MountFlightRules.FlapAmplitude(speed01, soaring, reduced);
            float wing = MountFlightRules.WingAngle(_beatPhase, amplitude);
            float tip = MountFlightRules.WingAngle(_beatPhase - MountFlightRules.TipLag, amplitude) * 0.6f;
            SetWing(_wingLeft, wing, -1);
            SetWing(_wingRight, wing, 1);
            SetWing(_wingTipLeft, tip, -1);
            SetWing(_wingTipRight, tip, 1);

            float bob = MountFlightRules.Bob(_beatPhase, reduced);
            float pitch = (soaring ? -8f : 6f * speed01) * MountFlightRules.MotionScale(reduced);
            _visual.localPosition = new Vector3(0f, _hover + bob, 0f);
            _visual.localRotation = Quaternion.Euler(pitch, 0f, 0f);

            if (_legHips != null)
            {
                for (int i = 0; i < _legHips.Length; i++)
                {
                    if (_legHips[i] != null)
                    {
                        _legHips[i].localRotation = Quaternion.Euler(i < 2 ? MountFlightRules.FrontLegTuck : MountFlightRules.HindLegTuck, 0f, 0f);
                    }
                }
            }

            if (StepSounds && _definition != null && MountFlightRules.DownstrokeStarted(previous, _beatPhase) && (speed01 > 0.05f || soaring))
            {
                MountAudio.Play(MountAudio.WingBeat, position);
            }
        }

        /// <summary>Rolls a wing part up by <paramref name="degrees"/> (+ = up) on its side.</summary>
        private static void SetWing(Transform part, float degrees, int side)
        {
            if (part != null)
            {
                part.localRotation = Quaternion.Euler(0f, 0f, degrees * side);
            }
        }

        private static Transform FindDeep(Transform root, string name)
        {
            if (root.name == name)
            {
                return root;
            }

            for (int i = 0; i < root.childCount; i++)
            {
                Transform found = FindDeep(root.GetChild(i), name);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }
    }

    /// <summary>
    /// Gait tuning of the mounts on top of the engine's quadruped default: they move much faster than the enemies
    /// (8–12 m/s mounted), so the strides grow longer and reach further before the cadence tops out; the gallop blend
    /// adds the rocking pitch. Distances and speeds scale with the model (the map token rides a ×2.4 mount).
    /// </summary>
    public static class MountGait
    {
        public static LegGaitSettings SettingsFor(float legLength, float gallop01, float scale, LegGaitStyle style = LegGaitStyle.Quadruped)
        {
            float k = Mathf.Max(0.01f, scale);
            float g = Mathf.Clamp01(gallop01);
            // The rheas run on two legs (M3): same tuning on the engine's biped default.
            LegGaitSettings s = LegGaitSettings.Default(style == LegGaitStyle.Biped ? LegGaitStyle.Biped : LegGaitStyle.Quadruped);
            s.StrideLength = Mathf.Max(1.1f * k, legLength * 2f);
            s.StrideGrowth = 1.2f;
            s.FullSpeed = 10f * k;
            s.MaxCadence = 4.2f;
            s.TrotSpeed = 1.8f * k;
            s.GallopStartSpeed = 6f * k;
            s.GallopFullSpeed = 8.5f * k;
            s.ReachFraction = 0.85f;
            s.BobAmplitude = Mathf.Lerp(0.035f, 0.06f, g) * k;
            s.PitchDegrees = Mathf.Lerp(5f, 9f, g);
            s.RollDegrees = 2f;
            return s;
        }
    }
}
