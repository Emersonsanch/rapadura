using System.Collections.Generic;
using Rapadura.Core.Accessibility;
using Rapadura.Core.DI;
using Rapadura.Gameplay.Combat;
using Rapadura.Gameplay.Locomotion;
using UnityEngine;

namespace Rapadura.Gameplay.Enemies.Visuals
{
    /// <summary>
    /// Secondary, per-part motion of a procedural enemy model (on its visual root, next to nothing
    /// else). <see cref="EnemyProceduralAnimator"/> poses the whole visual root (gait, hover, attack,
    /// recoil, topple); this only moves the named children it built, so the two never fight:
    /// bats orbit the swarm centre and flap (power downstroke, glides that grow with travel speed), the golem's and guardian's arms sway, the warlord's cape
    /// swings, the guardian's head and fists float with their rune ring turning, and the dweller's
    /// head breathes, its ears droop and the stolen light pulses. Honours "Reduzir Movimento"
    /// (<see cref="AccessibilitySettings.MotionScaleFor"/>) and stops (back to rest) once the enemy dies.
    /// Set <see cref="DriveLimbs"/> false to leave arms/legs/wings to another animator.
    /// </summary>
    [DisallowMultipleComponent]
    public class ProceduralEnemyRig : MonoBehaviour
    {
        [SerializeField] private ProceduralEnemyKind _kind;
        [Tooltip("Global multiplier on the per-part motion.")]
        [SerializeField, Range(0f, 2f)] private float _intensity = 1f;
        [Tooltip("Animate arms/fists/wings/legs here. Turn off when another animator drives them.")]
        [SerializeField] private bool _driveLimbs = true;

        private struct Tracked
        {
            public Transform Transform;
            public Vector3 Position;
            public Quaternion Rotation;
            public Vector3 Scale;
        }

        private readonly List<Tracked> _parts = new List<Tracked>(32);
        private readonly Dictionary<string, int> _index = new Dictionary<string, int>();
        private readonly List<int> _bats = new List<int>(8);
        private readonly List<int> _wings = new List<int>(16);
        private readonly List<int> _fireflies = new List<int>(12);
        private readonly List<int> _lanterns = new List<int>(12);
        private bool _cached;
        private float _time;
        private float _seed;
        private Health _health;
        private AccessibilitySettings _accessibility;
        private float _accessibilityRetry;
        private EnemyProceduralAnimator _animator;
        private ProceduralLegLocomotion _legRig;
        private float _legRigRetry;
        private float _attack01;
        private float _halo01;
        private float _cage01;

        /// <summary>Werewolf: how far (deg) the torso pitches up when it rears to strike.</summary>
        public const float WerewolfRearDegrees = 52f;

        public ProceduralEnemyKind Kind => _kind;

        /// <summary>
        /// 0..1 attack pose of the night species (werewolf rearing up, jaws/gape opening): rises during the attack
        /// windup, falls back after the strike. A telegraph, so "Reduzir Movimento" does not shrink it.
        /// </summary>
        public float Attack01 => _attack01;

        /// <summary>Attack phase used when there is no <see cref="EnemyProceduralAnimator"/> above (tests, previews); ignored otherwise.</summary>
        public EnemyAttackVisualPhase AttackPhaseOverride { get; set; }

        /// <summary>0..1 how fast the enemy travels (set by <see cref="EnemyProceduralAnimator"/>): bats glide more between beats when flying.</summary>
        public float MoveSpeed01 { get; set; }

        /// <summary>Health fraction used when there is no <see cref="Health"/> above (tests, previews): drives the colossus' phase look.</summary>
        public float Health01Override { get; set; } = 1f;

        /// <summary>Bone colossus: 0..1 how far its skull halo is unfolded (second phase on, <see cref="Rapadura.Gameplay.Dungeon.DungeonRules.ColossusPhaseFor"/>).</summary>
        public float Halo01 => _halo01;

        /// <summary>Bone colossus: 0..1 how far its rib cage is open (last phase).</summary>
        public float Cage01 => _cage01;

        private static readonly string[] GolemParts = { ProceduralEnemyParts.Head };
        private static readonly string[] GuardianParts = { ProceduralEnemyParts.Head };
        private static readonly string[] DwellerParts = { ProceduralEnemyParts.Head, ProceduralEnemyParts.EarLeft, ProceduralEnemyParts.EarRight };
        private static readonly string[] OwlParts = { ProceduralEnemyParts.Head };

        /// <summary>
        /// Follow-through parts this rig already animates for <paramref name="kind"/> (head bob/turn, ear
        /// droop), so the stepping-legs driver leaves them alone; null = none.
        /// </summary>
        public static IList<string> PartsAnimatedBy(ProceduralEnemyKind kind)
        {
            switch (kind)
            {
                case ProceduralEnemyKind.StoneGolem: return GolemParts;
                case ProceduralEnemyKind.RuinGuardian: return GuardianParts;
                case ProceduralEnemyKind.AncientDweller: return DwellerParts;
                case ProceduralEnemyKind.ShroudOwl: return OwlParts;
                default: return null;
            }
        }

        /// <summary>When false, arms/fists/wings are left alone (an external animator owns them).</summary>
        public bool DriveLimbs
        {
            get => _driveLimbs;
            set
            {
                if (_driveLimbs && !value)
                {
                    RestoreLimbs();
                }

                _driveLimbs = value;
            }
        }

        public void Configure(ProceduralEnemyKind kind)
        {
            _kind = kind;
            _cached = false;
        }

        /// <summary>A named part of the model (see <see cref="ProceduralEnemyParts"/>), or null.</summary>
        public Transform GetPart(string partName)
        {
            EnsureCached();
            return _index.TryGetValue(partName, out int i) ? _parts[i].Transform : null;
        }

        private void OnEnable()
        {
            _health = GetComponentInParent<Health>();
            _animator = GetComponentInParent<EnemyProceduralAnimator>();
            _seed = (Mathf.Abs(GetInstanceID()) % 997) * 0.13f;
            _attack01 = 0f;
            _halo01 = 0f;
            _cage01 = 0f;
            _legRigRetry = 0f;
        }

        private void OnDisable()
        {
            RestoreAll();
        }

        private void Update()
        {
            if (_accessibility == null)
            {
                _accessibilityRetry -= Time.deltaTime;
                if (_accessibilityRetry <= 0f)
                {
                    _accessibilityRetry = 1f;
                    ServiceLocator.TryGet(out _accessibility);
                }
            }

            if (_health != null && _health.IsDead)
            {
                RestoreAll();
                return;
            }

            Tick(Time.deltaTime, AccessibilitySettings.MotionScaleFor(_accessibility));
        }

        /// <summary>Advances the motion by <paramref name="deltaTime"/> (public for tests).</summary>
        public void Tick(float deltaTime, float motionScale = 1f)
        {
            EnsureCached();
            _time += Mathf.Max(0f, deltaTime);
            float t = _time + _seed;
            float k = _intensity * Mathf.Clamp01(motionScale);
            float speed = Mathf.Lerp(0.5f, 1f, Mathf.Clamp01(motionScale));

            switch (_kind)
            {
                case ProceduralEnemyKind.BatSwarm:
                    Rotate(ProceduralEnemyParts.Swarm, Quaternion.Euler(0f, _time * 75f * speed * _intensity, 0f));
                    for (int i = 0; i < _bats.Count; i++)
                    {
                        Offset(_bats[i], new Vector3(0f, Mathf.Sin(t * 3.1f + i * 1.7f) * 0.07f * k, 0f));
                    }

                    if (_driveLimbs)
                    {
                        float travel = Mathf.Clamp01(MoveSpeed01);
                        for (int i = 0; i < _wings.Count; i++)
                        {
                            Tracked wing = _parts[_wings[i]];
                            int bat = i / 2;
                            float sign = wing.Transform.name == ProceduralEnemyParts.WingLeft ? -1f : 1f;
                            float flap = BatWingAngle(t, bat, speed, travel) * Mathf.Max(k, 0.25f);
                            wing.Transform.localRotation = wing.Rotation * Quaternion.AngleAxis(flap * sign, Vector3.forward);
                        }
                    }

                    break;

                case ProceduralEnemyKind.StoneGolem:
                    if (_driveLimbs)
                    {
                        Swing(ProceduralEnemyParts.ArmLeft, Mathf.Sin(t * 1.4f) * 4f * k);
                        Swing(ProceduralEnemyParts.ArmRight, -Mathf.Sin(t * 1.4f) * 4f * k);
                    }

                    Rotate(ProceduralEnemyParts.Head, Quaternion.Euler(0f, Mathf.Sin(t * 0.7f) * 6f * k, 0f));
                    break;

                case ProceduralEnemyKind.Warlord:
                    Swing(ProceduralEnemyParts.Cape, -Mathf.Abs(Mathf.Sin(t * 1.1f)) * 6f * k);
                    if (_driveLimbs)
                    {
                        Swing(ProceduralEnemyParts.ArmLeft, Mathf.Sin(t * 1.2f) * 3f * k);
                    }

                    break;

                case ProceduralEnemyKind.RuinGuardian:
                    Offset(ProceduralEnemyParts.Head, new Vector3(0f, Mathf.Sin(t * 1.3f) * 0.07f * k, 0f));
                    Rotate(ProceduralEnemyParts.Runes, Quaternion.Euler(0f, _time * 40f * speed * _intensity, 0f));
                    if (_driveLimbs)
                    {
                        Offset(ProceduralEnemyParts.ArmLeft, new Vector3(0f, Mathf.Sin(t * 1.1f) * 0.05f * k, 0f));
                        Offset(ProceduralEnemyParts.ArmRight, new Vector3(0f, Mathf.Sin(t * 1.1f + 1.6f) * 0.05f * k, 0f));
                        Offset(ProceduralEnemyParts.FistLeft, new Vector3(0f, Mathf.Sin(t * 1.9f) * 0.06f * k, 0f));
                        Offset(ProceduralEnemyParts.FistRight, new Vector3(0f, Mathf.Sin(t * 1.9f + 2.1f) * 0.06f * k, 0f));
                    }

                    break;

                case ProceduralEnemyKind.AncientDweller:
                    Rotate(ProceduralEnemyParts.Head, Quaternion.Euler(Mathf.Sin(t * 0.8f) * 3f * k, 0f, Mathf.Sin(t * 0.4f) * 2.5f * k));
                    Rotate(ProceduralEnemyParts.EarLeft, Quaternion.Euler(0f, 0f, Mathf.Sin(t * 0.9f) * 6f * k));
                    Rotate(ProceduralEnemyParts.EarRight, Quaternion.Euler(0f, 0f, -Mathf.Sin(t * 0.9f + 0.5f) * 6f * k));
                    Pulse(ProceduralEnemyParts.StolenLight, 1f + Mathf.Sin(t * 2.2f) * 0.05f * k);
                    break;

                case ProceduralEnemyKind.Werewolf:
                {
                    // Rears up on the hind legs for the strike: the torso (chest, head, forelegs) pitches up from the
                    // hips, the forelegs let go of the ground, the clawed paws come up and the jaws open; then it slams down.
                    if (_legRig == null)
                    {
                        // Added on the visual by EnemyProceduralAnimator after this rig was enabled: look again now and then.
                        _legRigRetry -= deltaTime;
                        if (_legRigRetry <= 0f)
                        {
                            _legRigRetry = 0.5f;
                            _legRig = GetComponent<ProceduralLegLocomotion>();
                        }
                    }

                    float r = StepAttack(deltaTime, 4.5f, 2.6f);
                    Rotate(ProceduralEnemyParts.Torso, Quaternion.Euler(-WerewolfRearDegrees * r, 0f, 0f));
                    Rotate(WerewolfRecipe.ShoulderLeft, Quaternion.Euler(-24f * r, 0f, -10f * r));
                    Rotate(WerewolfRecipe.ShoulderRight, Quaternion.Euler(-24f * r, 0f, 10f * r));
                    Rotate(ProceduralEnemyParts.Jaw, Quaternion.Euler(22f * r + Mathf.Max(0f, Mathf.Sin(t * 1.7f)) * 4f * k, 0f, 0f));
                    if (_legRig != null)
                    {
                        _legRig.FrontLegsWeight = 1f - r;
                    }

                    break;
                }

                case ProceduralEnemyKind.VampireBat:
                    StepAttack(deltaTime, 5f, 3f);
                    FlapWings(t, speed * 0.55f, Mathf.Max(k, 0.25f) * 1.1f, Mathf.Clamp01(MoveSpeed01));
                    break;

                case ProceduralEnemyKind.ShroudOwl:
                {
                    float r = StepAttack(deltaTime, 5f, 3f);
                    // Slow flaps with long glides; the wings hold wide open (the screech) on the windup.
                    FlapWings(t, speed * 0.42f, Mathf.Max(k, 0.25f) * (1f - 0.6f * r), Mathf.Max(Mathf.Clamp01(MoveSpeed01), 0.4f));
                    // Every few seconds the head swivels way round, then snaps forward again to attack.
                    float glance = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((Mathf.Sin(t * 0.45f) - 0.55f) / 0.3f)) * (1f - r);
                    float direction = Mathf.Sin(t * 0.13f + 1f) >= 0f ? 1f : -1f;
                    Rotate(ProceduralEnemyParts.Head, Quaternion.Euler(0f, glance * 80f * direction * k, 0f));
                    break;
                }

                case ProceduralEnemyKind.FireflySwarm:
                    Rotate(ProceduralEnemyParts.Swarm, Quaternion.Euler(Mathf.Sin(t * 0.7f) * 8f * k, _time * 38f * speed * _intensity, 0f));
                    for (int i = 0; i < _fireflies.Count; i++)
                    {
                        Offset(_fireflies[i], new Vector3(Mathf.Sin(t * 2.3f + i * 2.1f) * 0.05f * k, Mathf.Sin(t * 3.4f + i * 1.3f) * 0.09f * k, 0f));
                    }

                    for (int i = 0; i < _lanterns.Count; i++)
                    {
                        // Blinking lanterns (never off, never above 1.25x: always readable in the dark).
                        float blink = 0.5f + 0.5f * Mathf.Sin(t * 3.1f + i * 1.9f);
                        Pulse(_lanterns[i], Mathf.Lerp(0.8f, 1.25f, blink * Mathf.Max(k, 0.3f)));
                    }

                    break;

                case ProceduralEnemyKind.Potoo:
                {
                    // Sits stiff as a stick; the tiny beak opens into a huge pink gape for the ambush.
                    float r = StepAttack(deltaTime, 6f, 2.2f);
                    Rotate(ProceduralEnemyParts.Jaw, Quaternion.Euler(48f * r, 0f, 0f));
                    float travel = Mathf.Clamp01(MoveSpeed01);
                    FlapWings(t, speed * 0.7f, Mathf.Max(k, 0.25f) * Mathf.Lerp(0.15f, 1f, Mathf.Max(travel, r)), travel);
                    break;
                }

                case ProceduralEnemyKind.EmberVulture:
                case ProceduralEnemyKind.CrestedHawk:
                {
                    // Broad soaring wings: long glides, the wings spread and hold still on the windup (taking aim).
                    float r = StepAttack(deltaTime, 5f, 3f);
                    float rate = _kind == ProceduralEnemyKind.CrestedHawk ? 0.6f : 0.38f;
                    FlapWings(t, speed * rate, Mathf.Max(k, 0.25f) * (1f - 0.7f * r), Mathf.Max(Mathf.Clamp01(MoveSpeed01), 0.5f));
                    Rotate(ProceduralEnemyParts.Head, Quaternion.Euler(-14f * r, Mathf.Sin(t * 0.6f) * 25f * k * (1f - r), 0f));
                    break;
                }

                case ProceduralEnemyKind.BrokenLantern:
                {
                    // Sways on an invisible chain; the trapped light flickers and swells before each bolt.
                    float r = StepAttack(deltaTime, 4f, 2.5f);
                    Rotate(ProceduralEnemyParts.Body, Quaternion.Euler(Mathf.Sin(t * 0.9f) * 6f * k, 0f, Mathf.Sin(t * 0.7f + 1.3f) * 6f * k));
                    float flicker = 0.5f + 0.5f * Mathf.Sin(t * 5.3f) * Mathf.Sin(t * 2.1f);
                    Pulse(ProceduralEnemyParts.Lantern, Mathf.Lerp(0.9f, 1.1f, flicker * Mathf.Max(k, 0.3f)) + 0.3f * r);
                    break;
                }

                case ProceduralEnemyKind.LanternFish:
                {
                    // Swims in the air: tail sweeps, fins paddle, the jaw gapes and the lure brightens to shoot.
                    float r = StepAttack(deltaTime, 5f, 3f);
                    Rotate(ProceduralEnemyParts.Tail, Quaternion.Euler(0f, Mathf.Sin(t * 3f) * 22f * k, 0f));
                    Rotate(ProceduralEnemyParts.Jaw, Quaternion.Euler(35f * r, 0f, 0f));
                    FlapWings(t, speed * 0.9f, Mathf.Max(k, 0.25f) * 0.6f, Mathf.Clamp01(MoveSpeed01));
                    Pulse(ProceduralEnemyParts.Lantern, 1f + Mathf.Sin(t * 2.4f) * 0.08f * k + 0.45f * r);
                    break;
                }

                case ProceduralEnemyKind.FloatingSkull:
                {
                    // Bobs and nods in the air, teeth chattering; the jaw gapes for the dive and the wisps flare.
                    float r = StepAttack(deltaTime, 6f, 3f);
                    Offset(ProceduralEnemyParts.Body, new Vector3(0f, Mathf.Sin(t * 1.7f) * 0.06f * k, 0f));
                    Rotate(ProceduralEnemyParts.Body, Quaternion.Euler(Mathf.Sin(t * 1.3f) * 7f * k - 12f * r, Mathf.Sin(t * 0.6f) * 10f * k, Mathf.Sin(t * 0.9f) * 5f * k));
                    Rotate(ProceduralEnemyParts.Jaw, Quaternion.Euler(Mathf.Max(0f, Mathf.Sin(t * 11f)) * 7f * k + 32f * r, 0f, 0f));
                    Pulse(ProceduralEnemyParts.Wisp, Mathf.Lerp(0.85f, 1.15f, (0.5f + 0.5f * Mathf.Sin(t * 7.3f) * Mathf.Sin(t * 2.9f)) * Mathf.Max(k, 0.3f)) + 0.25f * r);
                    break;
                }

                case ProceduralEnemyKind.SkeletonWarrior:
                {
                    // Raises the copper sword for the blow (the shield arm belongs to the guard behaviour).
                    float r = StepAttack(deltaTime, 5f, 3f);
                    if (_driveLimbs)
                    {
                        Rotate(ProceduralEnemyParts.ArmRight, Quaternion.Euler(-115f * r + Mathf.Sin(t * 1.3f) * 3f * k, 0f, 12f * r));
                    }

                    Rotate(ProceduralEnemyParts.Jaw, Quaternion.Euler(Mathf.Max(0f, Mathf.Sin(t * 5f)) * 5f * k + 14f * r, 0f, 0f));
                    break;
                }

                case ProceduralEnemyKind.SkeletonArcher:
                {
                    // Lifts the bow upright at arm's length and draws the string back to the jaw.
                    float r = StepAttack(deltaTime, 4f, 2.5f);
                    if (_driveLimbs)
                    {
                        Rotate(ProceduralEnemyParts.ArmLeft, Quaternion.Euler(-82f * r + Mathf.Sin(t * 1.1f) * 3f * k, 0f, -8f * r));
                        Rotate(ProceduralEnemyParts.ArmRight, Quaternion.Euler(-88f * r - Mathf.Sin(t * 1.1f + 1f) * 3f * k, 0f, 38f * r));
                    }

                    Rotate(ProceduralEnemyParts.Jaw, Quaternion.Euler(Mathf.Max(0f, Mathf.Sin(t * 4.3f)) * 4f * k, 0f, 0f));
                    break;
                }

                case ProceduralEnemyKind.BoneSorcerer:
                {
                    // The orb turns and breathes over the staff; to cast: staff forward, the free hand up, the orb swells.
                    float r = StepAttack(deltaTime, 4f, 2.5f);
                    if (_driveLimbs)
                    {
                        Rotate(ProceduralEnemyParts.ArmRight, Quaternion.Euler(-38f * r + Mathf.Sin(t * 0.9f) * 3f * k, 0f, 0f));
                        Rotate(ProceduralEnemyParts.ArmLeft, Quaternion.Euler(-70f * r + Mathf.Sin(t * 0.9f + 2f) * 4f * k, 0f, -15f * r));
                    }

                    Offset(ProceduralEnemyParts.Orb, new Vector3(0f, Mathf.Sin(t * 1.6f) * 0.04f * k, 0f));
                    Rotate(ProceduralEnemyParts.Orb, Quaternion.Euler(0f, _time * 60f * speed * _intensity, 0f));
                    Pulse(ProceduralEnemyParts.Orb, 1f + Mathf.Sin(t * 2.6f) * 0.08f * k + 0.5f * r);
                    Rotate(ProceduralEnemyParts.Jaw, Quaternion.Euler(18f * r, 0f, 0f));
                    break;
                }

                case ProceduralEnemyKind.BoneColossus:
                    TickColossus(deltaTime, t, k, speed);
                    break;
            }
        }

        /// <summary>
        /// The colossus shows its fight phases (<see cref="Rapadura.Gameplay.Dungeon.DungeonRules.ColossusPhaseFor"/>): from the
        /// second phase the halo of loose skulls unfolds and circles its head (the skulls it throws), in the last one the rib
        /// cage swings open and the soul-fire core blazes. A state, not decoration: "Reduzir Movimento" keeps it (only slower).
        /// </summary>
        private void TickColossus(float deltaTime, float t, float k, float speed)
        {
            float r = StepAttack(deltaTime, 3.5f, 2f);
            float health01 = _health != null && _health.MaxHealth > 0f ? _health.CurrentHealth / _health.MaxHealth : Health01Override;
            Rapadura.Gameplay.Dungeon.ColossusPhase phase = Rapadura.Gameplay.Dungeon.DungeonRules.ColossusPhaseFor(Mathf.Clamp01(health01));
            float dt = Mathf.Max(0f, deltaTime);
            _halo01 = Mathf.MoveTowards(_halo01, phase >= Rapadura.Gameplay.Dungeon.ColossusPhase.Skulls ? 1f : 0f, dt * 1.5f);
            _cage01 = Mathf.MoveTowards(_cage01, phase >= Rapadura.Gameplay.Dungeon.ColossusPhase.Cage ? 1f : 0f, dt * 1.2f);

            float halo = Mathf.SmoothStep(0f, 1f, _halo01);
            if (_index.TryGetValue(ProceduralEnemyParts.Halo, out int h))
            {
                _parts[h].Transform.localScale = Vector3.one * Mathf.Lerp(OssuaryEnemyRecipes.HaloFoldedScale, 1f, halo);
                _parts[h].Transform.localRotation = _parts[h].Rotation * Quaternion.Euler(Mathf.Sin(t * 0.8f) * 6f * k, _time * 55f * speed * _intensity, 0f);
            }

            float cage = Mathf.SmoothStep(0f, 1f, _cage01);
            float breathe = Mathf.Max(0f, Mathf.Sin(t * 1.4f)) * 4f * k * (0.3f + cage);
            Rotate(ProceduralEnemyParts.RibsLeft, Quaternion.Euler(0f, -(68f * cage + breathe), 0f));
            Rotate(ProceduralEnemyParts.RibsRight, Quaternion.Euler(0f, 68f * cage + breathe, 0f));
            Pulse(ProceduralEnemyParts.Core, 1f + Mathf.Sin(t * (2f + 2f * cage)) * 0.07f * Mathf.Max(k, 0.3f) + 0.35f * cage + 0.15f * r);
            Rotate(ProceduralEnemyParts.Jaw, Quaternion.Euler(16f * r + 6f * cage + Mathf.Max(0f, Mathf.Sin(t * 3f)) * 3f * k, 0f, 0f));
            if (_driveLimbs)
            {
                Swing(ProceduralEnemyParts.ArmLeft, Mathf.Sin(t * 1.1f) * 4f * k);
            }
        }

        /// <summary>Eases <see cref="Attack01"/> toward 1 during the windup (rate <paramref name="up"/>/s), back to 0 otherwise; returns it smoothed.</summary>
        private float StepAttack(float deltaTime, float up, float down)
        {
            EnemyAttackVisualPhase phase = _animator != null ? _animator.AttackPhase : AttackPhaseOverride;
            bool windup = phase == EnemyAttackVisualPhase.Windup;
            _attack01 = Mathf.MoveTowards(_attack01, windup ? 1f : 0f, Mathf.Max(0f, deltaTime) * (windup ? up : down));
            return Mathf.SmoothStep(0f, 1f, _attack01);
        }

        /// <summary>Flaps every <c>Wing_L</c>/<c>Wing_R</c> of a single flyer (one bird or bat, not the swarm).</summary>
        private void FlapWings(float t, float rate, float amplitude, float travel)
        {
            if (!_driveLimbs)
            {
                return;
            }

            for (int i = 0; i < _wings.Count; i++)
            {
                Tracked wing = _parts[_wings[i]];
                float sign = wing.Transform.name == ProceduralEnemyParts.WingLeft ? -1f : 1f;
                float flap = BatWingAngle(t, 0, rate, travel) * amplitude;
                wing.Transform.localRotation = wing.Rotation * Quaternion.AngleAxis(flap * sign, Vector3.forward);
            }
        }

        /// <summary>
        /// Wing angle (deg, + = up) of bat <paramref name="bat"/>: a fast power downstroke and a slower
        /// recovery upstroke, broken by glides with the wings held out (more of them, and longer, while the
        /// swarm travels: <paramref name="travel"/> 0..1). Each bat has its own rhythm.
        /// </summary>
        public static float BatWingAngle(float time, int bat, float rate, float travel)
        {
            float cycle = time * 2.4f * rate + bat * 0.37f;
            float p = cycle - Mathf.Floor(cycle);
            // Downstroke in the first 40 % (+1 → -1), recovery in the rest (-1 → +1).
            float wave = p < 0.4f ? Mathf.Cos(Mathf.PI * p / 0.4f) : -Mathf.Cos(Mathf.PI * (p - 0.4f) / 0.6f);
            float flap = 10f + wave * 46f;

            // Glide windows: a slow per-bat wave above a threshold that drops as the swarm speeds up.
            float glideWave = 0.5f + 0.5f * Mathf.Sin(time * (0.9f + 0.13f * bat) + bat * 2.1f);
            float glide = Mathf.SmoothStep(0f, 1f, (glideWave - Mathf.Lerp(0.85f, 0.5f, Mathf.Clamp01(travel))) / 0.12f);
            return Mathf.Lerp(flap, 14f, glide);
        }

        private void EnsureCached()
        {
            if (_cached)
            {
                return;
            }

            _cached = true;
            _parts.Clear();
            _index.Clear();
            _bats.Clear();
            _wings.Clear();
            _fireflies.Clear();
            _lanterns.Clear();
            foreach (Transform child in GetComponentsInChildren<Transform>(true))
            {
                if (child == transform)
                {
                    continue;
                }

                int i = _parts.Count;
                _parts.Add(new Tracked
                {
                    Transform = child,
                    Position = child.localPosition,
                    Rotation = child.localRotation,
                    Scale = child.localScale,
                });

                string partName = child.name;
                if (!_index.ContainsKey(partName))
                {
                    _index[partName] = i;
                }

                if (partName.StartsWith(ProceduralEnemyParts.BatPrefix))
                {
                    _bats.Add(i);
                }
                else if (partName == ProceduralEnemyParts.WingLeft || partName == ProceduralEnemyParts.WingRight)
                {
                    _wings.Add(i);
                }
                else if (partName.StartsWith(ProceduralEnemyParts.FireflyPrefix))
                {
                    _fireflies.Add(i);
                }
                else if (partName == ProceduralEnemyParts.Lantern)
                {
                    _lanterns.Add(i);
                }
            }
        }

        private void Rotate(string partName, Quaternion delta)
        {
            if (_index.TryGetValue(partName, out int i))
            {
                _parts[i].Transform.localRotation = _parts[i].Rotation * delta;
            }
        }

        private void Swing(string partName, float degrees) => Rotate(partName, Quaternion.AngleAxis(degrees, Vector3.right));

        private void Offset(string partName, Vector3 delta)
        {
            if (_index.TryGetValue(partName, out int i))
            {
                Offset(i, delta);
            }
        }

        private void Offset(int i, Vector3 delta) => _parts[i].Transform.localPosition = _parts[i].Position + delta;

        private void Pulse(string partName, float scale)
        {
            if (_index.TryGetValue(partName, out int i))
            {
                Pulse(i, scale);
            }
        }

        private void Pulse(int i, float scale) => _parts[i].Transform.localScale = _parts[i].Scale * scale;

        private void RestoreAll()
        {
            _attack01 = 0f;
            _halo01 = 0f;
            _cage01 = 0f;
            if (_legRig != null)
            {
                _legRig.FrontLegsWeight = 1f;
            }

            if (!_cached)
            {
                return;
            }

            foreach (Tracked part in _parts)
            {
                if (part.Transform != null)
                {
                    part.Transform.SetLocalPositionAndRotation(part.Position, part.Rotation);
                    part.Transform.localScale = part.Scale;
                }
            }
        }

        private void RestoreLimbs()
        {
            if (!_cached)
            {
                return;
            }

            foreach (Tracked part in _parts)
            {
                if (part.Transform != null && IsLimb(part.Transform.name))
                {
                    part.Transform.SetLocalPositionAndRotation(part.Position, part.Rotation);
                }
            }
        }

        private static bool IsLimb(string partName)
        {
            return partName == ProceduralEnemyParts.ArmLeft || partName == ProceduralEnemyParts.ArmRight
                || partName == ProceduralEnemyParts.FistLeft || partName == ProceduralEnemyParts.FistRight
                || partName == ProceduralEnemyParts.WingLeft || partName == ProceduralEnemyParts.WingRight
                || partName == ProceduralEnemyParts.LegLeft || partName == ProceduralEnemyParts.LegRight;
        }
    }
}
