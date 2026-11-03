using Rapadura.Core.Events;
using Rapadura.Gameplay.Combat;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Enemies.Visuals;
using Rapadura.Gameplay.Skills;
using UnityEngine;

namespace Rapadura.Gameplay.Dungeon
{
    /// <summary>
    /// Base of the ossuary enemies' special abilities. The regular <see cref="EnemyController"/> brain still does all the
    /// chasing, telegraphed windups and melee swings (and the procedural gait animates it); these components only add on top,
    /// through the controller's public API: watching <see cref="EnemyController.StrikeSequence"/> for the moment of a strike
    /// (to fire a projectile or a shockwave), holding the brain for a moment with <see cref="EnemyController.HoldAggro"/> while
    /// they move it themselves (<see cref="EnemyController.MoveInDirection"/>), and buffs on its <see cref="BuffController"/>.
    /// The numbers live in <see cref="DungeonRules"/>.
    /// </summary>
    public abstract class DungeonEnemyBehaviour : MonoBehaviour
    {
        protected EnemyController Controller { get; private set; }
        protected Health Health { get; private set; }
        protected DungeonHall Hall { get; private set; }
        protected float Alive { get; private set; }

        private int _lastStrike;
        private float _moveLeft;
        private Vector3 _moveDirection;
        private float _moveSpeed;

        public virtual void Configure(DungeonHall hall)
        {
            Hall = hall;
        }

        protected virtual void Awake()
        {
            Controller = GetComponent<EnemyController>();
            Health = GetComponent<Health>();
        }

        protected virtual void Start()
        {
            if (Controller != null)
            {
                _lastStrike = Controller.StrikeSequence;
            }
        }

        protected bool IsDead => Health != null && Health.IsDead;

        protected Transform Hero => Controller != null ? Controller.Target : null;

        protected float FlatDistanceToHero
        {
            get
            {
                if (Hero == null)
                {
                    return float.PositiveInfinity;
                }

                Vector3 d = Hero.position - transform.position;
                d.y = 0f;
                return d.magnitude;
            }
        }

        /// <summary>Damage of one hit worth <paramref name="baseDamage"/>, through this enemy's AttackDamage modifiers (debuffs weaken it).</summary>
        protected float ScaledDamage(float baseDamage)
        {
            BuffController buffs = Controller != null ? Controller.Buffs : null;
            return Mathf.Max(0f, buffs != null ? buffs.GetModifiedValue(StatType.AttackDamage, baseDamage) : baseDamage);
        }

        /// <summary>Holds the brain and slides the enemy along <paramref name="direction"/> for <paramref name="seconds"/> (kept inside the hall).</summary>
        protected void Slide(Vector3 direction, float distance, float seconds)
        {
            direction.y = 0f;
            if (Controller == null || direction.sqrMagnitude < 1e-4f || seconds <= 0f)
            {
                return;
            }

            _moveDirection = direction.normalized;
            _moveLeft = seconds;
            // The held brain still coasts the velocity it is given, so ask for about half: the net slide is ~distance.
            _moveSpeed = distance / seconds * 0.55f;
            Controller.HoldAggro(seconds);
        }

        protected virtual void Update()
        {
            if (Controller == null || IsDead)
            {
                return;
            }

            float dt = Time.deltaTime;
            if (dt <= 0f)
            {
                return;
            }

            Alive += dt;
            if (Controller.StrikeSequence != _lastStrike)
            {
                _lastStrike = Controller.StrikeSequence;
                OnStrike();
            }

            if (_moveLeft > 0f)
            {
                _moveLeft -= dt;
                Vector3 next = transform.position + _moveDirection * (_moveSpeed * dt);
                if (Hall == null || Hall.Layout.Contains(Hall.ToLocal(next), false))
                {
                    Controller.MoveInDirection(_moveDirection, _moveSpeed, dt);
                }
            }

            Tick(dt);
        }

        /// <summary>The brain just landed a strike (the damage moment of its telegraphed attack).</summary>
        protected virtual void OnStrike()
        {
        }

        protected virtual void Tick(float deltaTime)
        {
        }

        /// <summary>A muzzle point in front of the enemy at <paramref name="height"/> m.</summary>
        protected Vector3 Muzzle(float height, float forward = 0.6f) => transform.position + Vector3.up * height + transform.forward * forward;

        protected Transform Part(string name)
        {
            ProceduralEnemyRig rig = GetComponentInChildren<ProceduralEnemyRig>();
            return rig != null ? rig.GetPart(name) : null;
        }
    }

    /// <summary>
    /// Caveira Flutuante: very fast (7.5 m/s), floats at ~1.2 m bobbing on a slow sine (the Flyer hover + its model's own bob in
    /// <see cref="ProceduralEnemyRig"/>), dives at the hero and darts 2 m back right after each bite. Carries a soft blue-green
    /// fairy light (intensity 1.5, range 3 m) that flickers like the fire in its eye sockets.
    /// </summary>
    public class FloatingSkullBehaviour : DungeonEnemyBehaviour
    {
        private Light _light;

        protected override void Start()
        {
            base.Start();
            Transform visual = transform.Find(EnemyVisualSwapper.VisualName);
            _light = DungeonProps.AddLight(visual != null ? visual : transform, transform.position + Vector3.up * DungeonRules.SkullHoverHeight,
                DungeonProps.Wisp, DungeonRules.SkullLightIntensity, DungeonRules.SkullLightRange);
        }

        protected override void OnStrike()
        {
            Vector3 away = transform.position - (Hero != null ? Hero.position : transform.position + transform.forward);
            Slide(away.sqrMagnitude > 1e-4f ? away : -transform.forward, DungeonRules.SkullRetreatDistance, DungeonRules.SkullRetreatSeconds);
        }

        protected override void Tick(float deltaTime)
        {
            if (_light != null)
            {
                float t = Alive * 6f + GetInstanceID() % 13;
                _light.intensity = DungeonRules.SkullLightIntensity * (0.85f + 0.15f * Mathf.Sin(t) * Mathf.Sin(t * 0.37f));
            }
        }

        private void OnDisable()
        {
            if (_light != null)
            {
                _light.enabled = false;
            }
        }
    }

    /// <summary>
    /// Guerreiro Esqueleto de Cobre: 25% permanent armour (percent Defense), and every 6 s it raises its bone shield for 2.5 s
    /// (<see cref="DungeonRules.IsGuarding"/>): +60% damage reduction facing the hero (it turns to face its target while it
    /// fights) and 15% of any projectile damage taken sent back to the shooter (<see cref="DungeonRules.GuardReflectDamage"/>).
    /// The shield arm swings up while guarding.
    /// </summary>
    public class SkeletonGuardBehaviour : DungeonEnemyBehaviour
    {
        public const string ArmorSource = "dungeon_warrior_armor";
        public const string GuardSource = "dungeon_warrior_guard";

        private bool _guarding;
        private Transform _shieldArm;
        private Quaternion _armRest;
        private float _armBlend;

        public bool IsGuardingNow => _guarding;

        protected override void Start()
        {
            base.Start();
            ApplyArmor();
            _shieldArm = Part(ProceduralEnemyParts.ArmLeft);
            if (_shieldArm != null)
            {
                _armRest = _shieldArm.localRotation;
            }
        }

        private void OnEnable()
        {
            EventBus.Subscribe<DamageAppliedEvent>(OnDamageApplied);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<DamageAppliedEvent>(OnDamageApplied);
        }

        /// <summary>Permanent armour (idempotent; public for tests).</summary>
        public void ApplyArmor()
        {
            BuffController buffs = Controller != null ? Controller.Buffs : GetComponent<BuffController>();
            if (buffs == null || buffs.HasEffectsFromSource(ArmorSource))
            {
                return;
            }

            buffs.ApplyEffect(Permanent(ArmorSource, DungeonRules.WarriorArmor), ArmorSource);
        }

        private static StatModifierDefinition Permanent(string name, float value) => new StatModifierDefinition
        {
            displayName = name,
            affectedStat = StatType.Defense,
            application = ModifierApplication.PercentAdditive,
            value = value,
            durationSeconds = 0f,
        };

        protected override void Tick(float deltaTime)
        {
            bool guard = DungeonRules.IsGuarding(Alive);
            if (guard != _guarding)
            {
                SetGuard(guard);
            }

            _armBlend = Mathf.MoveTowards(_armBlend, _guarding ? 1f : 0f, deltaTime * 5f);
            if (_shieldArm != null)
            {
                _shieldArm.localRotation = _armRest * Quaternion.Euler(-70f * _armBlend, 0f, 25f * _armBlend);
            }
        }

        private void SetGuard(bool guard)
        {
            _guarding = guard;
            BuffController buffs = Controller != null ? Controller.Buffs : null;
            if (buffs == null)
            {
                return;
            }

            buffs.RemoveEffectsBySource(GuardSource);
            if (guard)
            {
                buffs.ApplyEffect(Permanent(GuardSource, DungeonRules.GuardDamageReduction), GuardSource);
            }
        }

        private void OnDamageApplied(DamageAppliedEvent evt)
        {
            if (evt.Target != gameObject || evt.Source == null || !_guarding || IsDead)
            {
                return;
            }

            if (evt.Source.GetComponentInParent<EnemyController>() != null)
            {
                return;
            }

            Vector3 d = evt.Source.transform.position - transform.position;
            d.y = 0f;
            float reflect = DungeonRules.GuardReflectDamage(evt.FinalDamage, d.magnitude > DungeonRules.RangedHitDistance, true);
            ICombatTarget shooter = reflect > 0f ? evt.Source.GetComponentInParent<ICombatTarget>() : null;
            if (shooter != null && shooter.IsAlive)
            {
                shooter.ApplyDamage(reflect, ElementType.Physical, gameObject);
            }
        }
    }

    /// <summary>
    /// Arqueiro Esqueleto do Fosso: shoots from up to 14 m (its brain's "attack range"; the swing itself deals nothing) a rune rib
    /// arrow with a purple smoke trail that slows the hero by 20% for 3 s (<see cref="DungeonProjectile.LaunchArrow"/>). When the
    /// hero comes closer than 4 m it backs off three steps while it nocks the next arrow.
    /// </summary>
    public class SkeletonArcherBehaviour : DungeonEnemyBehaviour
    {
        public const float ArrowDamage = 10f;

        private float _retreatCooldown;

        protected override void OnStrike()
        {
            if (Hero != null)
            {
                DungeonProjectile.LaunchArrow(Muzzle(1.35f), Hero, gameObject, ScaledDamage(ArrowDamage));
                DungeonFx.Play(DungeonFx.ArrowCue, transform.position);
            }
        }

        protected override void Tick(float deltaTime)
        {
            _retreatCooldown -= deltaTime;
            if (_retreatCooldown > 0f || Hero == null || !DungeonRules.ArcherShouldRetreat(FlatDistanceToHero))
            {
                return;
            }

            _retreatCooldown = 1.4f;
            Slide(transform.position - Hero.position, DungeonRules.ArcherRetreatDistance, 0.6f);
        }
    }

    /// <summary>
    /// Feiticeiro das Ossadas: casts spectral orbs (the strike of its brain's ranged "attack") and every 12 s channels
    /// "Erguer as Ossadas" for 2 s, relighting up to two fallen copper warriors of its room at 40% health
    /// (<see cref="DungeonDirector.TryRaiseFallen"/>). The relit ones are flagged reanimated: 0 XP and no drop.
    /// </summary>
    public class BoneSorcererBehaviour : DungeonEnemyBehaviour
    {
        public const float OrbDamage = 14f;

        private DungeonDirector _director;
        private float _raiseTimer;
        private float _channelLeft = -1f;
        private float _puffTimer;

        public void Bind(DungeonDirector director) => _director = director;

        protected override void Start()
        {
            base.Start();
            _raiseTimer = DungeonRules.RaiseIntervalSeconds * 0.6f;
        }

        protected override void OnStrike()
        {
            if (Hero != null && _channelLeft < 0f)
            {
                DungeonProjectile.LaunchOrb(Muzzle(1.6f), Hero, gameObject, ScaledDamage(OrbDamage));
            }
        }

        protected override void Tick(float deltaTime)
        {
            if (_channelLeft >= 0f)
            {
                _channelLeft -= deltaTime;
                Controller.HoldAggro(0.1f);
                _puffTimer -= deltaTime;
                if (_puffTimer <= 0f)
                {
                    _puffTimer = 0.35f;
                    DungeonPuff.Spawn(transform.position + Vector3.up * 2f, DungeonProps.SmokePurple, 0.7f, 0.5f, 2);
                }

                if (_channelLeft < 0f && _director != null)
                {
                    _director.TryRaiseFallen(this, transform.position);
                }

                return;
            }

            _raiseTimer -= deltaTime;
            if (_raiseTimer > 0f)
            {
                return;
            }

            _raiseTimer = DungeonRules.RaiseIntervalSeconds;
            if (_director != null && _director.HasFallenToRaise(transform.position))
            {
                _channelLeft = DungeonRules.RaiseChannelSeconds;
                DungeonFx.Play(DungeonFx.RaiseCue, transform.position);
                DungeonFx.Warn(transform.position + Vector3.up * 3f, DungeonFx.Text(DungeonTexts.SorcererRaise));
            }
        }
    }

    /// <summary>
    /// O Colosso de Crânios (boss, 1400 HP, slow and heavy; the boss stun reduction applies and enemies take no knockback in
    /// this project). Three phases by health (<see cref="DungeonRules.ColossusPhaseFor"/>):
    /// <list type="number">
    /// <item>every club slam sends a radial shockwave out to 5 m (<see cref="DungeonShockwave"/>: back off or jump it);</item>
    /// <item>also throws three bouncing skulls down the hall every 7 s (<see cref="BouncingSkull"/>);</item>
    /// <item>also traps the hero in a rib cage every 11 s (<see cref="BoneCage"/>: three hits to break) while it charges a big
    /// blow for 4 s — it only lands if the cage still holds.</item>
    /// </list>
    /// </summary>
    public class BoneColossusBehaviour : DungeonEnemyBehaviour
    {
        private ColossusPhase _phase = ColossusPhase.Slam;
        private float _skullTimer = DungeonRules.SkullThrowIntervalSeconds * 0.5f;
        private float _cageTimer = 2f;
        private BoneCage _cage;
        private float _chargeLeft = -1f;
        private Transform _visual;
        private Vector3 _visualScale;

        public ColossusPhase Phase => _phase;

        protected override void Start()
        {
            base.Start();
            _visual = transform.Find(EnemyVisualSwapper.VisualName);
            _visualScale = _visual != null ? _visual.localScale : Vector3.one;
        }

        protected override void OnStrike()
        {
            float slam = Controller != null ? Controller.EffectiveAttackDamage : 0f;
            DungeonShockwave.Spawn(transform.position, Hero, gameObject, slam * DungeonRules.ShockwaveDamageFactor,
                Controller != null && Controller.Definition != null ? Controller.Definition.AttackRange + 0.3f : 2.5f);
        }

        protected override void Tick(float deltaTime)
        {
            float fraction = Health != null && Health.MaxHealth > 0f ? Health.CurrentHealth / Health.MaxHealth : 1f;
            ColossusPhase phase = DungeonRules.ColossusPhaseFor(fraction);
            if (phase != _phase)
            {
                _phase = phase;
                DungeonFx.Warn(transform.position + Vector3.up * 4f, DungeonFx.Text(phase == ColossusPhase.Skulls ? DungeonTexts.ColossusSkulls : DungeonTexts.ColossusCage));
                if (phase == ColossusPhase.Cage)
                {
                    _cageTimer = 1f;
                }
            }

            if (_chargeLeft >= 0f)
            {
                TickCharge(deltaTime);
                return;
            }

            if (_phase >= ColossusPhase.Skulls)
            {
                _skullTimer -= deltaTime;
                if (_skullTimer <= 0f && Hero != null)
                {
                    _skullTimer = DungeonRules.SkullThrowIntervalSeconds;
                    ThrowSkulls();
                }
            }

            if (_phase == ColossusPhase.Cage)
            {
                _cageTimer -= deltaTime;
                if (_cageTimer <= 0f && Hero != null)
                {
                    _cageTimer = DungeonRules.CageIntervalSeconds;
                    _cage = BoneCage.Trap(Hero, DungeonRules.CageChargeSeconds);
                    _chargeLeft = DungeonRules.CageChargeSeconds;
                    DungeonFx.Warn(Hero.position + Vector3.up * 3f, DungeonFx.Text(DungeonTexts.ColossusCage));
                }
            }
        }

        private void ThrowSkulls()
        {
            if (Hall == null)
            {
                return;
            }

            Controller.HoldAggro(0.6f);
            Vector3 to = Hero.position - transform.position;
            to.y = 0f;
            for (int i = 0; i < DungeonRules.BouncingSkullsPerThrow; i++)
            {
                Vector3 dir = Quaternion.Euler(0f, DungeonRules.SkullThrowAngle(i, DungeonRules.BouncingSkullsPerThrow), 0f) * (to.sqrMagnitude > 1e-4f ? to.normalized : transform.forward);
                BouncingSkull.Throw(Hall, Muzzle(1.8f, 1.2f), dir, Hero, gameObject, ScaledDamage(DungeonRules.BouncingSkullDamage));
            }
        }

        private void TickCharge(float deltaTime)
        {
            _chargeLeft -= deltaTime;
            Controller.HoldAggro(0.1f);
            if (_visual != null)
            {
                // Swells while it gathers the blow (a clear telegraph).
                float k = 1f + 0.08f * Mathf.Clamp01(1f - _chargeLeft / DungeonRules.CageChargeSeconds) * (1f + Mathf.Sin(Alive * 20f) * 0.3f);
                _visual.localScale = _visualScale * k;
            }

            if (_chargeLeft >= 0f)
            {
                return;
            }

            if (_visual != null)
            {
                _visual.localScale = _visualScale;
            }

            bool holding = _cage != null && _cage.IsHolding;
            float damage = ScaledDamage(DungeonRules.CageBlowDamage(holding));
            if (damage > 0f && Hero != null)
            {
                ICombatTarget victim = Hero.GetComponentInParent<ICombatTarget>();
                victim?.ApplyDamage(damage, ElementType.Physical, gameObject);
                DungeonShockwave.Spawn(Hero.position, null, gameObject, 0f, 0f);
            }

            if (_cage != null)
            {
                _cage.Break(false);
                _cage = null;
            }

            _chargeLeft = -1f;
        }

        private void OnDisable()
        {
            if (_cage != null)
            {
                _cage.Break(false);
                _cage = null;
            }
        }
    }
}
