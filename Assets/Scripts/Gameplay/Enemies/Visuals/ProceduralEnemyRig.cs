using System.Collections.Generic;
using Rapadura.Core.Accessibility;
using Rapadura.Core.DI;
using Rapadura.Gameplay.Combat;
using UnityEngine;

namespace Rapadura.Gameplay.Enemies.Visuals
{
    /// <summary>
    /// Secondary, per-part motion of a procedural enemy model (on its visual root, next to nothing
    /// else). <see cref="EnemyProceduralAnimator"/> poses the whole visual root (gait, hover, attack,
    /// recoil, topple); this only moves the named children it built, so the two never fight:
    /// bats orbit the swarm centre and flap, the golem's and guardian's arms sway, the warlord's cape
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
        private bool _cached;
        private float _time;
        private float _seed;
        private Health _health;
        private AccessibilitySettings _accessibility;
        private float _accessibilityRetry;

        public ProceduralEnemyKind Kind => _kind;

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
            _seed = (Mathf.Abs(GetInstanceID()) % 997) * 0.13f;
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
                        for (int i = 0; i < _wings.Count; i++)
                        {
                            Tracked wing = _parts[_wings[i]];
                            float sign = wing.Transform.name == ProceduralEnemyParts.WingLeft ? -1f : 1f;
                            float flap = Mathf.Sin(t * 15f * speed + (i / 2) * 1.3f) * 48f * Mathf.Max(k, 0.25f);
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
            }
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
                _parts[i].Transform.localScale = _parts[i].Scale * scale;
            }
        }

        private void RestoreAll()
        {
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
