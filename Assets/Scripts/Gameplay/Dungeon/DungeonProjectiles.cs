using Rapadura.Gameplay.Combat;
using Rapadura.Gameplay.Skills;
using UnityEngine;

namespace Rapadura.Gameplay.Dungeon
{
    /// <summary>
    /// A ranged shot of a dungeon enemy (the archer's rune rib arrow with its purple smoke trail, the sorcerer's spectral orb).
    /// Flies straight (the orb leans gently toward the hero), hits when it passes within <see cref="HitRadius"/> of the hero's
    /// chest — a distance check, no physics layers needed — and lands as plain ranged damage (<see cref="ICombatTarget.ApplyDamage"/>:
    /// Defense applies, Espinhos do not, it is not melee). The arrow also slows (-20% move speed for 3 s) through the hero's
    /// <see cref="BuffController"/>. Expires after its range.
    /// </summary>
    public class DungeonProjectile : MonoBehaviour
    {
        public const float HitRadius = 0.7f;
        public const float ChestHeight = 1f;
        public const string SlowSource = "dungeon_arrow_slow";

        private Transform _target;
        private GameObject _source;
        private Vector3 _velocity;
        private float _damage;
        private float _life;
        private float _homing;
        private bool _slows;
        private ElementType _element;
        private float _trailTimer;
        private Color _trail;

        public static DungeonProjectile LaunchArrow(Vector3 from, Transform target, GameObject source, float damage)
        {
            GameObject root = new GameObject("DungeonArrow");
            root.transform.position = from;
            var block = new MaterialPropertyBlock();
            DungeonProps.Part(root.transform, PrimitiveType.Cylinder, "Shaft", from, Quaternion.identity, new Vector3(0.05f, 0.4f, 0.05f), DungeonProps.Ivory, block);
            DungeonProps.Part(root.transform, PrimitiveType.Sphere, "Rune", from, Quaternion.identity, Vector3.one * 0.12f, DungeonProps.SmokePurple, block, glow: true);
            root.transform.GetChild(0).localRotation = Quaternion.Euler(90f, 0f, 0f);
            var projectile = root.AddComponent<DungeonProjectile>();
            projectile.Setup(target, source, damage, DungeonRules.ArrowSpeed, DungeonRules.ArcherRange + 3f, 0f, true, ElementType.Physical, DungeonProps.SmokePurple);
            return projectile;
        }

        public static DungeonProjectile LaunchOrb(Vector3 from, Transform target, GameObject source, float damage)
        {
            GameObject root = new GameObject("DungeonOrb");
            root.transform.position = from;
            DungeonProps.Part(root.transform, PrimitiveType.Sphere, "Orb", from, Quaternion.identity, Vector3.one * 0.32f, DungeonProps.Wisp, new MaterialPropertyBlock(), glow: true);
            DungeonProps.AddLight(root.transform, from, DungeonProps.Wisp, 1.2f, 3f);
            var projectile = root.AddComponent<DungeonProjectile>();
            projectile.Setup(target, source, damage, DungeonRules.OrbSpeed, 18f, 2.2f, false, ElementType.Dark, DungeonProps.Wisp);
            return projectile;
        }

        public void Setup(Transform target, GameObject source, float damage, float speed, float range, float homing, bool slows, ElementType element, Color trail)
        {
            _target = target;
            _source = source;
            _damage = damage;
            _homing = homing;
            _slows = slows;
            _element = element;
            _trail = trail;
            Vector3 aim = target != null ? target.position + Vector3.up * ChestHeight - transform.position : transform.forward;
            _velocity = (aim.sqrMagnitude > 1e-4f ? aim.normalized : Vector3.forward) * speed;
            _life = range / Mathf.Max(0.1f, speed);
            transform.rotation = Quaternion.LookRotation(_velocity);
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            if (dt <= 0f)
            {
                return;
            }

            if (_target != null && _homing > 0f)
            {
                Vector3 to = _target.position + Vector3.up * ChestHeight - transform.position;
                _velocity = Vector3.RotateTowards(_velocity, to, _homing * dt, 0f);
            }

            Vector3 from = transform.position;
            transform.position += _velocity * dt;
            if (_velocity.sqrMagnitude > 1e-4f)
            {
                transform.rotation = Quaternion.LookRotation(_velocity);
            }

            _trailTimer -= dt;
            if (_trailTimer <= 0f)
            {
                _trailTimer = 0.1f;
                DungeonPuff.Spawn(transform.position, _trail, 0.35f, 0.35f, 1);
            }

            if (_target != null && SegmentHits(from, transform.position, _target.position + Vector3.up * ChestHeight, HitRadius))
            {
                Hit();
                return;
            }

            _life -= dt;
            if (_life <= 0f)
            {
                Destroy(gameObject);
            }
        }

        /// <summary>Whether the segment a→b passes within <paramref name="radius"/> of <paramref name="point"/> (pure, tested).</summary>
        public static bool SegmentHits(Vector3 a, Vector3 b, Vector3 point, float radius)
        {
            Vector3 ab = b - a;
            float length = ab.sqrMagnitude;
            float t = length > 1e-8f ? Mathf.Clamp01(Vector3.Dot(point - a, ab) / length) : 0f;
            return (a + ab * t - point).sqrMagnitude <= radius * radius;
        }

        private void Hit()
        {
            ICombatTarget victim = _target != null ? _target.GetComponentInParent<ICombatTarget>() : null;
            if (victim != null && victim.IsAlive)
            {
                victim.ApplyDamage(_damage, _element, _source);
                if (_slows && _target.TryGetComponent(out BuffController buffs))
                {
                    buffs.RemoveEffectsBySource(SlowSource);
                    buffs.ApplyEffect(new StatModifierDefinition
                    {
                        displayName = SlowSource,
                        affectedStat = StatType.MoveSpeed,
                        application = ModifierApplication.PercentAdditive,
                        value = -DungeonRules.ArrowSlowFraction,
                        durationSeconds = DungeonRules.ArrowSlowSeconds,
                        isDebuff = true,
                    }, SlowSource, _source);
                }
            }

            DungeonPuff.Spawn(transform.position, _trail, 0.8f, 0.4f);
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// One of the Colossus's three bouncing skulls: rolls down the hall in hall space, ricochets off the side walls
    /// (<see cref="DungeonRules.StepBounce"/>), then pops after its fuse — or at once when it reaches the hero — hurting
    /// everything within <see cref="DungeonRules.BouncingSkullBlastRadius"/> of it (the hero only; it is the boss's attack).
    /// </summary>
    public class BouncingSkull : MonoBehaviour
    {
        private DungeonHall _hall;
        private Transform _target;
        private GameObject _source;
        private float _x;
        private float _z;
        private float _vx;
        private float _vz;
        private float _fuse;
        private float _damage;
        private int _bounces;
        private float _hop;

        public static BouncingSkull Throw(DungeonHall hall, Vector3 from, Vector3 direction, Transform target, GameObject source, float damage)
        {
            var root = new GameObject("BouncingSkull");
            root.transform.position = from;
            var block = new MaterialPropertyBlock();
            DungeonProps.Part(root.transform, PrimitiveType.Sphere, "Skull", from, Quaternion.identity, new Vector3(0.5f, 0.45f, 0.52f), DungeonProps.Ivory, block);
            DungeonProps.Part(root.transform, PrimitiveType.Sphere, "Eyes", from + Vector3.forward * 0.2f + Vector3.up * 0.05f, Quaternion.identity, new Vector3(0.3f, 0.1f, 0.1f), DungeonProps.Wisp, block, glow: true);
            var skull = root.AddComponent<BouncingSkull>();
            Vector2 local = hall.ToLocal(from);
            skull._hall = hall;
            skull._x = local.x;
            skull._z = local.y;
            skull._vx = Vector3.Dot(direction.normalized, hall.Right) * DungeonRules.BouncingSkullSpeed;
            skull._vz = Vector3.Dot(direction.normalized, hall.Forward) * DungeonRules.BouncingSkullSpeed;
            skull._fuse = DungeonRules.BouncingSkullFuseSeconds;
            skull._target = target;
            skull._source = source;
            skull._damage = damage;
            return skull;
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            if (dt <= 0f || _hall == null)
            {
                Destroy(gameObject);
                return;
            }

            float half = _hall.Layout.HalfWidthAt(_z) - 0.4f;
            if (DungeonRules.StepBounce(ref _x, ref _vx, dt, half))
            {
                _bounces++;
                _hop = 1f;
            }

            _z += _vz * dt;
            if (_z < 0.3f || _z > _hall.Layout.Length - 0.3f)
            {
                _vz = -_vz;
                _z = Mathf.Clamp(_z, 0.3f, _hall.Layout.Length - 0.3f);
                _bounces++;
                _hop = 1f;
            }

            _hop = Mathf.Max(0f, _hop - dt * 3f);
            Vector3 p = _hall.ToWorld(_x, _z);
            p.y = _hall.Origin.y + 0.3f + Mathf.Abs(Mathf.Sin(_fuse * 9f)) * 0.25f + _hop * 0.4f;
            transform.position = p;
            transform.Rotate(new Vector3(_vz, 0f, -_vx) * (60f * dt), Space.World);

            _fuse -= dt;
            bool nearHero = _target != null && FlatDistance(_target.position, p) <= DungeonRules.BouncingSkullBlastRadius * 0.6f;
            if (_fuse <= 0f || nearHero || _bounces > DungeonRules.BouncingSkullMaxBounces + 2)
            {
                Explode(p);
            }
        }

        private void Explode(Vector3 at)
        {
            if (_target != null && FlatDistance(_target.position, at) <= DungeonRules.BouncingSkullBlastRadius)
            {
                ICombatTarget victim = _target.GetComponentInParent<ICombatTarget>();
                if (victim != null && victim.IsAlive)
                {
                    victim.ApplyDamage(_damage, ElementType.Dark, _source);
                }
            }

            DungeonPuff.Spawn(at, DungeonProps.SmokePurple, 1.4f, 0.5f);
            DungeonPuff.Pebbles(at, DungeonProps.Ivory, 5);
            Destroy(gameObject);
        }

        private static float FlatDistance(Vector3 a, Vector3 b)
        {
            a.y = 0f;
            b.y = 0f;
            return Vector3.Distance(a, b);
        }
    }

    /// <summary>The Colossus's slam: a ring of bone dust racing out to <see cref="DungeonRules.ShockwaveRadius"/>; it hurts the hero once when it passes them.</summary>
    public class DungeonShockwave : MonoBehaviour
    {
        private Transform _target;
        private GameObject _source;
        private float _damage;
        private float _age;
        private bool _hit;
        private float _skipInside;
        private Transform[] _stones;

        public static DungeonShockwave Spawn(Vector3 center, Transform target, GameObject source, float damage, float skipInside)
        {
            var root = new GameObject("DungeonShockwave");
            root.transform.position = center;
            var wave = root.AddComponent<DungeonShockwave>();
            wave._target = target;
            wave._source = source;
            wave._damage = damage;
            wave._skipInside = skipInside;
            const int count = 14;
            wave._stones = new Transform[count];
            var block = new MaterialPropertyBlock();
            for (int i = 0; i < count; i++)
            {
                wave._stones[i] = DungeonProps.Part(root.transform, PrimitiveType.Cube, "Dust", center, Quaternion.Euler(0f, i * 360f / count, 0f),
                    new Vector3(0.5f, 0.25f, 0.25f), DungeonProps.BoneDust, block).transform;
            }

            return wave;
        }

        /// <summary>Radius of the ring <paramref name="age"/> seconds after the slam (pure, tested).</summary>
        public static float RadiusAt(float age) => DungeonRules.ShockwaveRadius * Mathf.Clamp01(age / DungeonRules.ShockwaveSeconds);

        private void Update()
        {
            float before = RadiusAt(_age);
            _age += Time.deltaTime;
            float radius = RadiusAt(_age);
            for (int i = 0; i < _stones.Length; i++)
            {
                float a = i * Mathf.PI * 2f / _stones.Length;
                _stones[i].position = transform.position + new Vector3(Mathf.Sin(a), 0.1f, Mathf.Cos(a)) * radius;
            }

            if (!_hit && _target != null)
            {
                Vector3 d = _target.position - transform.position;
                d.y = 0f;
                float distance = d.magnitude;
                // Standing in the slam itself already took the club; jumping over the ring (airborne) dodges it.
                bool airborne = _target.position.y > transform.position.y + 0.6f;
                if (distance > _skipInside && distance >= before - 0.3f && distance <= radius + 0.3f && !airborne)
                {
                    _hit = true;
                    ICombatTarget victim = _target.GetComponentInParent<ICombatTarget>();
                    if (victim != null && victim.IsAlive)
                    {
                        victim.ApplyDamage(_damage, ElementType.Earth, _source);
                    }
                }
            }

            if (_age >= DungeonRules.ShockwaveSeconds + 0.2f)
            {
                Destroy(gameObject);
            }
        }
    }

    /// <summary>
    /// "Prisão de Costelas": a ring of rib bones around the hero (rooted while it holds). Each bar is solid and an
    /// <see cref="ICombatTarget"/>, so the hero's basic attack (an overlap in front) or a tap breaks it: three hits
    /// (<see cref="DungeonRules.CageHitsToBreak"/>) and the hero is free. The Colossus asks <see cref="IsHolding"/> when its
    /// charged blow lands.
    /// </summary>
    public class BoneCage : MonoBehaviour, ICombatTarget
    {
        public const string RootSource = "dungeon_cage_root";

        private Transform _hero;
        private int _hits;
        private bool _broken;
        private float _lifeLeft;

        public bool IsHolding => !_broken;
        public int Hits => _hits;
        public bool IsAlive => !_broken;
        public Transform Transform => transform;

        public static BoneCage Trap(Transform hero, float seconds)
        {
            var root = new GameObject("BoneCage");
            root.transform.position = hero.position;
            var block = new MaterialPropertyBlock();
            const int bars = 7;
            for (int i = 0; i < bars; i++)
            {
                float a = i * Mathf.PI * 2f / bars;
                Vector3 p = hero.position + new Vector3(Mathf.Sin(a), 0f, Mathf.Cos(a)) * 1.05f + Vector3.up * 1.1f;
                GameObject rib = DungeonProps.Part(root.transform, PrimitiveType.Capsule, "Rib", p, Quaternion.Euler(0f, a * Mathf.Rad2Deg, 12f),
                    new Vector3(0.22f, 1.1f, 0.22f), DungeonProps.Ivory, block);
                rib.AddComponent<CapsuleCollider>();
            }

            DungeonProps.Part(root.transform, PrimitiveType.Sphere, "Knot", hero.position + Vector3.up * 2.3f, Quaternion.identity, new Vector3(1.2f, 0.25f, 1.2f), DungeonProps.Ivory, block);
            var cage = root.AddComponent<BoneCage>();
            cage._hero = hero;
            cage._lifeLeft = seconds + 0.5f;
            cage.Root(true);
            return cage;
        }

        public void ApplyDamage(float amount, ElementType element, GameObject source)
        {
            if (_broken || amount <= 0f || (source != null && _hero != null && !source.transform.IsChildOf(_hero) && source.transform != _hero))
            {
                return;
            }

            _hits++;
            DungeonFx.Say(transform.position + Vector3.up * 2.6f, DungeonFx.Text(DungeonTexts.CageHit) + " " + _hits + "/" + DungeonRules.CageHitsToBreak);
            if (_hits >= DungeonRules.CageHitsToBreak)
            {
                Break(true);
            }
        }

        public void ApplyHealing(float amount, GameObject source)
        {
        }

        /// <summary>Opens the cage (freed by the hero, or when the blow has landed).</summary>
        public void Break(bool byHero)
        {
            if (_broken)
            {
                return;
            }

            _broken = true;
            Root(false);
            if (byHero)
            {
                DungeonFx.Say(transform.position + Vector3.up * 2.8f, DungeonFx.Text(DungeonTexts.CageBroken));
            }

            DungeonPuff.Pebbles(transform.position + Vector3.up * 1.2f, DungeonProps.Ivory, 7);
            Destroy(gameObject);
        }

        private void Root(bool on)
        {
            if (_hero == null || !_hero.TryGetComponent(out BuffController buffs))
            {
                return;
            }

            buffs.RemoveEffectsBySource(RootSource);
            if (on)
            {
                buffs.ApplyEffect(new StatModifierDefinition
                {
                    displayName = RootSource,
                    affectedStat = StatType.MoveSpeed,
                    application = ModifierApplication.PercentAdditive,
                    value = -1f,
                    durationSeconds = _lifeLeft,
                    isDebuff = true,
                }, RootSource);
            }
        }

        private void Update()
        {
            _lifeLeft -= Time.deltaTime;
            if (_lifeLeft <= 0f)
            {
                Break(false);
            }
        }

        private void OnDestroy()
        {
            if (!_broken)
            {
                Root(false);
            }
        }
    }
}
