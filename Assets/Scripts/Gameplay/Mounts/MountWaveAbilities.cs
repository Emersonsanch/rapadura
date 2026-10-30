using System.Collections.Generic;
using Rapadura.Core.Events;
using Rapadura.Gameplay.Combat;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.Secrets;
using Rapadura.Gameplay.Skills;
using UnityEngine;

namespace Rapadura.Gameplay.Mounts
{
    /// <summary>What a wave-1 ability needs from the rider in the 3D scene.</summary>
    public struct MountAbilityContext
    {
        public GameObject Hero;
        public MountSession Session;
        public BuffController Buffs;
        public PlayerStats Stats;
        public PlayerMotor Motor;
        public CharacterController Controller;
    }

    /// <summary>
    /// What the M3 wave-1 abilities do in the arena (Docs/Mounts.md §3; the starters' three stay in
    /// <see cref="MountAbilityExecutor"/>). Instant parts run here; timed parts (immunities, auras, trails, the ring, the
    /// shell, hiding, the lamp) are ticked by the hero's <see cref="MountAbilityEffects"/>. The numbers come from the
    /// ability definition and <see cref="MountWaveAbilityRules"/>, never from a random roll.
    /// Returns the toast key to show (<see cref="MountWaveTexts"/>), or null.
    /// </summary>
    public static class MountWaveAbilities
    {
        public const string BuffSourcePrefix = "mount_ability_";

        public static string Execute(MountAbilityDefinition ability, MountAbilityContext ctx)
        {
            if (ability == null || ctx.Hero == null)
            {
                return null;
            }

            MountAbilityEffects effects = MountAbilityEffects.For(ctx);
            Vector3 at = ctx.Hero.transform.position;
            switch (ability.Kind)
            {
                case MountAbilityKind.Cleanse:
                    if (ctx.Buffs != null)
                    {
                        ctx.Buffs.RemoveDebuffs();
                    }

                    effects.CleanseRemaining = Mathf.Max(effects.CleanseRemaining, ability.DurationSeconds);
                    MountAudio.Play(MountAudio.Sniff, at);
                    return MountWaveTexts.ToastCleanse;

                case MountAbilityKind.DoubleJump:
                    effects.StartHop(ability.Distance);
                    MountAudio.Play(MountAudio.Dash, at);
                    return null;

                case MountAbilityKind.FreeGallop:
                    ctx.Session?.StartFreeGallop(ability.DurationSeconds);
                    MountAudio.Play(MountAudio.Dash, at);
                    return MountWaveTexts.ToastFreeGallop;

                case MountAbilityKind.Knockback:
                {
                    Vector3 forward = Flat(ctx.Hero.transform.forward);
                    Vector3 end = at + forward * ability.Distance;
                    int pushed = 0;
                    foreach (EnemyController enemy in LiveEnemies())
                    {
                        if (!MountWaveAbilityRules.InChargePath(at, end, enemy.transform.position, ability.Radius)
                            || MountRules.EffectDurationOn(1f, enemy.IsBoss, ability.BossMultiplier) <= 0f)
                        {
                            continue;
                        }

                        Vector3 dir = MountWaveAbilityRules.PushDirection(at, enemy.transform.position, forward);
                        MountPushDriver.Apply(enemy, dir, MountWaveAbilityRules.KnockbackPush, MountWaveAbilityRules.KnockbackPushSeconds);
                        pushed++;
                    }

                    effects.StartLunge(end, ability.DurationSeconds);
                    MountAudio.Play(MountAudio.Dash, at);
                    return pushed > 0 ? MountWaveTexts.ToastKnockback : null;
                }

                case MountAbilityKind.CCImmunity:
                    ctx.Buffs?.ClearStun();
                    ctx.Buffs?.RemoveDebuffs(StatType.MoveSpeed);
                    effects.CcImmuneRemaining = Mathf.Max(effects.CcImmuneRemaining, ability.DurationSeconds);
                    MountAudio.Play(MountAudio.Bray, at);
                    return MountWaveTexts.ToastCcImmune;

                case MountAbilityKind.NightVision:
                {
                    int seen = 0;
                    foreach (EnemyController enemy in LiveEnemies())
                    {
                        if (MountWaveAbilityRules.InRadius(at, enemy.transform.position, ability.Radius))
                        {
                            CombatFxManager.SpawnLootText(enemy.transform.position + Vector3.up * 2f, "!");
                            seen++;
                        }
                    }

                    effects.StartNightVision(ability.DurationSeconds);
                    MountAudio.Play(MountAudio.Sniff, at);
                    return seen > 0 ? MountWaveTexts.ToastNightVisionSeen : MountWaveTexts.ToastNightVision;
                }

                case MountAbilityKind.PartySpeedTrail:
                    effects.StartTrail(MountAbilityEffects.TrailKind.Speed, ability.DurationSeconds, ability.Radius, ability.Magnitude);
                    MountAudio.Play(MountAudio.Dash, at);
                    return MountWaveTexts.ToastSpeedTrail;

                case MountAbilityKind.RangedInteract:
                {
                    ArenaSecretObject nearest = null;
                    float best = float.MaxValue;
                    foreach (ArenaSecretObject secret in MountAbilityExecutor.FindSecrets(at, ability.Distance))
                    {
                        float d = (secret.transform.position - at).sqrMagnitude;
                        if (d < best)
                        {
                            best = d;
                            nearest = secret;
                        }
                    }

                    if (nearest == null)
                    {
                        return MountWaveTexts.ToastTongueMiss;
                    }

                    nearest.ApplyDamage(1f, ElementType.None, ctx.Hero);
                    MountAudio.Play(MountAudio.Sniff, nearest.transform.position);
                    return MountWaveTexts.ToastTongueHit;
                }

                case MountAbilityKind.Intimidate:
                {
                    int scared = 0;
                    foreach (EnemyController enemy in LiveEnemies())
                    {
                        float seconds = MountRules.EffectDurationOn(ability.DurationSeconds, enemy.IsBoss, ability.BossMultiplier);
                        if (seconds <= 0f || enemy.Buffs == null || !MountWaveAbilityRules.InRadius(at, enemy.transform.position, ability.Radius))
                        {
                            continue;
                        }

                        enemy.Buffs.ApplyEffect(Modifier("rugido", StatType.AttackDamage, -ability.Magnitude, seconds, debuff: true),
                            BuffSourcePrefix + ability.AbilityId, ctx.Hero);
                        scared++;
                    }

                    MountAudio.Play(MountAudio.Bray, at);
                    return scared > 0 ? MountWaveTexts.ToastRoar : null;
                }

                case MountAbilityKind.AreaStun:
                {
                    int stunned = 0;
                    foreach (EnemyController enemy in LiveEnemies())
                    {
                        if (enemy.Buffs != null && MountWaveAbilityRules.InRadius(at, enemy.transform.position, ability.Radius))
                        {
                            // The boss halving is BuffController's own (StunRules, 1.2 s -> 0.6 s).
                            if (enemy.Buffs.ApplyStun(ability.DurationSeconds) > 0f)
                            {
                                stunned++;
                            }
                        }
                    }

                    MountAudio.Play(MountAudio.Bray, at);
                    return stunned > 0 ? MountWaveTexts.ToastStomp : null;
                }

                case MountAbilityKind.HealAura:
                    effects.StartHealAura(ability.DurationSeconds, ability.Radius, ability.Magnitude);
                    MountAudio.Play(MountAudio.AbilityReady, at);
                    return MountWaveTexts.ToastHealAura;

                case MountAbilityKind.PounceMark:
                {
                    EnemyController target = Nearest(at, ability.Distance);
                    if (target == null)
                    {
                        return MountWaveTexts.ToastPounceMiss;
                    }

                    effects.StartLunge(MountWaveAbilityRules.PounceLanding(at, target.transform.position), ability.DurationSeconds);
                    ctx.Buffs?.ApplyEffect(Modifier("salto_da_onca", StatType.CriticalChance, 1f, MountWaveAbilityRules.PounceMarkSeconds, false,
                        ModifierApplication.Flat), BuffSourcePrefix + ability.AbilityId, ctx.Hero);
                    effects.PounceMarkSource = BuffSourcePrefix + ability.AbilityId;
                    CombatFxManager.SpawnLootText(target.transform.position + Vector3.up * 2f, "!");
                    MountAudio.Play(MountAudio.Dash, at);
                    return MountWaveTexts.ToastPounce;
                }

                case MountAbilityKind.Light:
                {
                    int found = 0;
                    foreach (ArenaSecretObject secret in MountAbilityExecutor.FindSecrets(at, ability.Radius))
                    {
                        CombatFxManager.SpawnLootText(secret.transform.position + Vector3.up * 1.6f, "!");
                        found++;
                    }

                    effects.StartLamp(ability.DurationSeconds, ability.Radius);
                    MountAudio.Play(MountAudio.AbilityReady, at);
                    return found > 0 ? MountWaveTexts.ToastLampFound : MountWaveTexts.ToastLamp;
                }

                case MountAbilityKind.DamageTrail:
                    effects.StartTrail(MountAbilityEffects.TrailKind.Burn, ability.DurationSeconds, ability.Radius, ability.Magnitude);
                    MountAudio.Play(MountAudio.Dash, at);
                    return MountWaveTexts.ToastEmberTrail;

                case MountAbilityKind.Stealth:
                    effects.StartStealth(ability.DurationSeconds, ability.Magnitude);
                    MountAudio.Play(MountAudio.Sniff, at);
                    return MountWaveTexts.ToastStealth;

                case MountAbilityKind.RepelRing:
                    effects.StartRing(ability.DurationSeconds, ability.Radius, ability.Magnitude, ability.BossMultiplier);
                    MountAudio.Play(MountAudio.Bray, at);
                    return MountWaveTexts.ToastRing;

                case MountAbilityKind.PartyBuff:
                {
                    foreach (PlayerStats ally in Allies(at, ability.Radius))
                    {
                        if (ally.TryGetComponent(out BuffController buffs))
                        {
                            string source = BuffSourcePrefix + ability.AbilityId;
                            buffs.ApplyEffect(Modifier("toada_dano", StatType.AttackDamage, ability.Magnitude, ability.DurationSeconds, false), source, ctx.Hero);
                            buffs.ApplyEffect(Modifier("toada_ritmo", StatType.AttackSpeed, ability.Magnitude, ability.DurationSeconds, false), source, ctx.Hero);
                        }
                    }

                    MountAudio.Play(MountAudio.Cowbell, at);
                    return MountWaveTexts.ToastBoiDance;
                }

                case MountAbilityKind.Invulnerable:
                    ctx.Session?.StartShell(ability.DurationSeconds);
                    ctx.Stats?.GrantInvulnerability(ability.DurationSeconds);
                    MountAudio.Play(MountAudio.Bray, at);
                    return MountWaveTexts.ToastShell;
            }

            return null;
        }

        // ------------------------------------------------------------------
        // Helpers
        // ------------------------------------------------------------------

        public static StatModifierDefinition Modifier(string name, StatType stat, float value, float seconds, bool debuff,
            ModifierApplication application = ModifierApplication.PercentAdditive)
        {
            return new StatModifierDefinition
            {
                displayName = name,
                affectedStat = stat,
                application = application,
                value = value,
                durationSeconds = seconds,
                isDebuff = debuff,
            };
        }

        public static StatModifierDefinition Burn(float damagePerSecond, float seconds)
        {
            return new StatModifierDefinition
            {
                displayName = "brasa",
                affectedStat = StatType.HealthRegen,
                application = ModifierApplication.Flat,
                value = 0f,
                durationSeconds = seconds,
                isDebuff = true,
                ticksOverTime = true,
                tickIntervalSeconds = 1f,
                tickDamageOrHeal = damagePerSecond,
            };
        }

        public static List<EnemyController> LiveEnemies()
        {
            var live = new List<EnemyController>();
            foreach (EnemyController enemy in EnemyController.EnabledEnemies)
            {
                if (enemy != null && (enemy.Health == null || !enemy.Health.IsDead))
                {
                    live.Add(enemy);
                }
            }

            return live;
        }

        public static IEnumerable<PlayerStats> Allies(Vector3 center, float radius)
        {
            foreach (PlayerStats stats in Object.FindObjectsByType<PlayerStats>(FindObjectsSortMode.None))
            {
                if (stats != null && !stats.IsDead && MountWaveAbilityRules.InRadius(center, stats.transform.position, radius))
                {
                    yield return stats;
                }
            }
        }

        private static EnemyController Nearest(Vector3 at, float range)
        {
            EnemyController best = null;
            float bestSqr = range * range;
            foreach (EnemyController enemy in LiveEnemies())
            {
                float d = (enemy.transform.position - at).sqrMagnitude;
                if (d <= bestSqr)
                {
                    bestSqr = d;
                    best = enemy;
                }
            }

            return best;
        }

        private static Vector3 Flat(Vector3 v)
        {
            v.y = 0f;
            return v.sqrMagnitude < 1e-6f ? Vector3.forward : v.normalized;
        }
    }

    /// <summary>
    /// The timed half of the wave-1 abilities on the hero: added on first use by <see cref="MountWaveAbilities"/>. Every
    /// timer runs out on its own; getting off the mount ends the ones tied to riding (trails, ring, lamp, hiding).
    /// </summary>
    [DisallowMultipleComponent]
    public class MountAbilityEffects : MonoBehaviour
    {
        public enum TrailKind
        {
            Speed,
            Burn,
        }

        /// <summary>Hook read by the enemies' detection (<see cref="EnemyDetectionUtility.TargetDetectionScale"/>).</summary>
        private static readonly List<MountAbilityEffects> Hidden = new List<MountAbilityEffects>();

        private readonly List<Vector3> _trail = new List<Vector3>();
        private MountAbilityContext _ctx;
        private float _trailRemaining;
        private float _trailRadius;
        private float _trailMagnitude;
        private float _trailDropTimer;
        private TrailKind _trailKind;
        private float _healRemaining;
        private float _healRadius;
        private float _healFraction;
        private float _healTick;
        private float _ringRemaining;
        private float _ringRadius;
        private float _ringBurn;
        private float _ringBossMultiplier;
        private float _stealthRemaining;
        private float _stealthScale = 1f;
        private float _lampRemaining;
        private Light _lamp;
        private float _nightVisionRemaining;
        private Color _ambientBefore;
        private bool _ambientBoosted;
        private float _pounceClearIn;
        private float _hopSpeed;
        private Vector3 _lungeTarget;
        private float _lungeRemaining;
        private bool _subscribed;

        public float CleanseRemaining { get; set; }
        public float CcImmuneRemaining { get; set; }
        public string PounceMarkSource { get; set; }
        public bool IsHidden => _stealthRemaining > 0f;
        public IReadOnlyList<Vector3> Trail => _trail;

        public static MountAbilityEffects For(MountAbilityContext ctx)
        {
            MountAbilityEffects effects = ctx.Hero.GetComponent<MountAbilityEffects>();
            if (effects == null)
            {
                effects = ctx.Hero.AddComponent<MountAbilityEffects>();
            }

            effects._ctx = ctx;
            return effects;
        }

        /// <summary>Detection radius multiplier for <paramref name="target"/>: the Kirin's scale while hidden, 1 otherwise.</summary>
        public static float DetectionScaleFor(Transform target)
        {
            if (target == null)
            {
                return 1f;
            }

            foreach (MountAbilityEffects effects in Hidden)
            {
                if (effects != null && effects.IsHidden && (effects.transform == target || target.IsChildOf(effects.transform)))
                {
                    return MountWaveAbilityRules.DetectionRadius(1f, true, effects._stealthScale);
                }
            }

            return 1f;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            Hidden.Clear();
            EnemyDetectionUtility.TargetDetectionScale = null;
        }

        public void StartHop(float height)
        {
            float gravity = Physics.gravity.y;
            _hopSpeed = MountWaveAbilityRules.JumpSpeedFor(Mathf.Max(0.5f, height), gravity);
            if (_ctx.Motor != null && _ctx.Motor.IsGrounded)
            {
                _ctx.Motor.Jump();
            }
        }

        public void StartLunge(Vector3 target, float seconds)
        {
            _lungeTarget = target;
            _lungeRemaining = Mathf.Max(0.05f, seconds);
        }

        public void StartTrail(TrailKind kind, float seconds, float radius, float magnitude)
        {
            _trailKind = kind;
            _trailRemaining = seconds;
            _trailRadius = Mathf.Max(0.3f, radius);
            _trailMagnitude = magnitude;
            _trailDropTimer = 0f;
            _trail.Clear();
        }

        public void StartHealAura(float seconds, float radius, float fraction)
        {
            _healRemaining = seconds;
            _healRadius = radius;
            _healFraction = fraction;
            _healTick = 0f;
        }

        public void StartRing(float seconds, float radius, float burnPerSecond, float bossMultiplier)
        {
            _ringRemaining = seconds;
            _ringRadius = radius;
            _ringBurn = burnPerSecond;
            _ringBossMultiplier = bossMultiplier;
        }

        public void StartStealth(float seconds, float scale)
        {
            _stealthRemaining = seconds;
            _stealthScale = scale;
            if (!Hidden.Contains(this))
            {
                Hidden.Add(this);
            }

            EnemyDetectionUtility.TargetDetectionScale = DetectionScaleFor;
        }

        public void StartLamp(float seconds, float radius)
        {
            _lampRemaining = seconds;
            if (_lamp == null)
            {
                var go = new GameObject("MountLamp");
                go.transform.SetParent(transform, false);
                go.transform.localPosition = new Vector3(0f, 2.2f, 0.6f);
                _lamp = go.AddComponent<Light>();
                _lamp.type = LightType.Point;
                _lamp.color = new Color(1f, 0.55f, 0.35f);
                _lamp.intensity = 2.5f;
                _lamp.shadows = LightShadows.None;
            }

            _lamp.range = Mathf.Max(1f, radius);
            _lamp.enabled = true;
        }

        public void StartNightVision(float seconds)
        {
            if (!_ambientBoosted)
            {
                _ambientBoosted = true;
                _ambientBefore = RenderSettings.ambientLight;
                RenderSettings.ambientLight = Color.Lerp(_ambientBefore, new Color(0.55f, 0.65f, 0.6f), 0.6f);
            }

            _nightVisionRemaining = seconds;
        }

        private void OnEnable()
        {
            if (!_subscribed)
            {
                _subscribed = true;
                EventBus.Subscribe<PlayerAttackPerformedEvent>(OnAttack);
            }
        }

        private void OnDisable()
        {
            if (_subscribed)
            {
                _subscribed = false;
                EventBus.Unsubscribe<PlayerAttackPerformedEvent>(OnAttack);
            }

            EndRideEffects();
            EndNightVision();
        }

        private void OnAttack(PlayerAttackPerformedEvent evt)
        {
            if (evt.Attacker == gameObject)
            {
                // Attacking breaks the Kirin's hiding (§3.4); the Onça's mark is spent by this blow.
                _stealthRemaining = 0f;
                if (!string.IsNullOrEmpty(PounceMarkSource) && _pounceClearIn <= 0f)
                {
                    _pounceClearIn = 0.6f;
                }
            }
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            if (dt <= 0f)
            {
                return;
            }

            bool mounted = _ctx.Session != null && _ctx.Session.IsMounted;
            if (!mounted)
            {
                EndRideEffects();
            }

            Vector3 at = transform.position;
            TickImmunities(dt);
            TickMovement(dt);
            TickTrail(dt, at);
            TickHeal(dt, at);
            TickRing(dt, at);

            _stealthRemaining = Mathf.Max(0f, _stealthRemaining - dt);
            if (_pounceClearIn > 0f)
            {
                _pounceClearIn -= dt;
                if (_pounceClearIn <= 0f)
                {
                    _ctx.Buffs?.RemoveEffectsBySource(PounceMarkSource);
                    PounceMarkSource = null;
                }
            }

            if (_lampRemaining > 0f)
            {
                _lampRemaining -= dt;
                if (_lampRemaining <= 0f && _lamp != null)
                {
                    _lamp.enabled = false;
                }
            }

            if (_nightVisionRemaining > 0f)
            {
                _nightVisionRemaining -= dt;
                if (_nightVisionRemaining <= 0f)
                {
                    EndNightVision();
                }
            }
        }

        private void TickImmunities(float dt)
        {
            if (CleanseRemaining > 0f)
            {
                CleanseRemaining -= dt;
                _ctx.Buffs?.RemoveDebuffs();
            }

            if (CcImmuneRemaining > 0f)
            {
                CcImmuneRemaining -= dt;
                if (_ctx.Buffs != null)
                {
                    _ctx.Buffs.ClearStun();
                    _ctx.Buffs.RemoveDebuffs(StatType.MoveSpeed);
                }
            }
        }

        private void TickMovement(float dt)
        {
            CharacterController cc = _ctx.Controller;
            if (cc == null || !cc.enabled)
            {
                _hopSpeed = 0f;
                _lungeRemaining = 0f;
                return;
            }

            if (_hopSpeed > 0f)
            {
                cc.Move(Vector3.up * (_hopSpeed * dt));
                _hopSpeed = Mathf.Max(0f, _hopSpeed - Mathf.Abs(Physics.gravity.y) * dt);
            }

            if (_lungeRemaining > 0f)
            {
                Vector3 to = _lungeTarget - transform.position;
                to.y = 0f;
                float step = Mathf.Min(1f, dt / _lungeRemaining);
                _lungeRemaining -= dt;
                float slow = _ctx.Buffs != null ? Mathf.Min(1f, _ctx.Buffs.GetMoveSpeedMultiplier()) : 1f;
                cc.Move(to * (step * slow));
            }
        }

        private void TickTrail(float dt, Vector3 at)
        {
            if (_trailRemaining <= 0f)
            {
                if (_trail.Count > 0)
                {
                    _trail.Clear();
                }

                return;
            }

            _trailRemaining -= dt;
            _trailDropTimer -= dt;
            if (_trailDropTimer <= 0f)
            {
                _trailDropTimer = MountWaveAbilityRules.TrailPointSeconds;
                _trail.Add(at);
            }

            if (_trailKind == TrailKind.Speed)
            {
                foreach (PlayerStats ally in MountWaveAbilities.Allies(at, 60f))
                {
                    if (MountWaveAbilityRules.OnTrail(_trail, ally.transform.position, _trailRadius) && ally.TryGetComponent(out BuffController buffs))
                    {
                        buffs.ApplyEffect(MountWaveAbilities.Modifier("trilha", StatType.MoveSpeed, _trailMagnitude, MountWaveAbilityRules.TrailBuffSeconds, false),
                            MountWaveAbilities.BuffSourcePrefix + "trilha_da_caipora", gameObject);
                    }
                }
            }
            else
            {
                foreach (EnemyController enemy in MountWaveAbilities.LiveEnemies())
                {
                    if (enemy.Buffs != null && MountWaveAbilityRules.OnTrail(_trail, enemy.transform.position, _trailRadius))
                    {
                        enemy.Buffs.ApplyEffect(MountWaveAbilities.Burn(_trailMagnitude, MountWaveAbilityRules.BurnSeconds),
                            MountWaveAbilities.BuffSourcePrefix + "corrida_de_brasa", gameObject);
                    }
                }
            }
        }

        private void TickHeal(float dt, Vector3 at)
        {
            if (_healRemaining <= 0f)
            {
                return;
            }

            _healRemaining -= dt;
            _healTick += dt;
            while (_healTick >= 1f)
            {
                _healTick -= 1f;
                foreach (PlayerStats ally in MountWaveAbilities.Allies(at, _healRadius))
                {
                    ally.Heal(MountWaveAbilityRules.HealPerSecond(ally.MaxHealth, _healFraction));
                }
            }
        }

        private void TickRing(float dt, Vector3 at)
        {
            if (_ringRemaining <= 0f)
            {
                return;
            }

            _ringRemaining -= dt;
            foreach (EnemyController enemy in MountWaveAbilities.LiveEnemies())
            {
                Vector3 pos = enemy.transform.position;
                if (!MountWaveAbilityRules.InRadius(at, pos, _ringRadius))
                {
                    continue;
                }

                if (enemy.Buffs != null)
                {
                    enemy.Buffs.ApplyEffect(MountWaveAbilities.Burn(_ringBurn, MountWaveAbilityRules.BurnSeconds),
                        MountWaveAbilities.BuffSourcePrefix + "anel_de_fogo_protetor", gameObject);
                }

                if (MountRules.EffectDurationOn(1f, enemy.IsBoss, _ringBossMultiplier) > 0f)
                {
                    Vector3 edge = MountWaveAbilityRules.RepelTo(at, pos, _ringRadius);
                    MountPushDriver.Apply(enemy, MountWaveAbilityRules.PushDirection(at, pos, transform.forward), (edge - pos).magnitude, 0.15f);
                }
            }
        }

        private void EndRideEffects()
        {
            _trailRemaining = 0f;
            _trail.Clear();
            _ringRemaining = 0f;
            _stealthRemaining = 0f;
            _lampRemaining = 0f;
            if (_lamp != null)
            {
                _lamp.enabled = false;
            }
        }

        private void EndNightVision()
        {
            if (_ambientBoosted)
            {
                _ambientBoosted = false;
                RenderSettings.ambientLight = _ambientBefore;
            }

            _nightVisionRemaining = 0f;
        }

        private void OnDestroy()
        {
            Hidden.Remove(this);
        }
    }

    /// <summary>Slides an enemy <c>distance</c> m along a direction over a few tenths of a second (Investida, Anel de Fogo), then removes itself.</summary>
    [DisallowMultipleComponent]
    public class MountPushDriver : MonoBehaviour
    {
        private EnemyController _enemy;
        private Vector3 _direction;
        private float _speed;
        private float _remaining;

        public static MountPushDriver Apply(EnemyController enemy, Vector3 direction, float distance, float seconds)
        {
            if (enemy == null || distance <= 0f)
            {
                return null;
            }

            MountPushDriver driver = enemy.GetComponent<MountPushDriver>();
            if (driver == null)
            {
                driver = enemy.gameObject.AddComponent<MountPushDriver>();
            }

            float time = Mathf.Max(0.05f, seconds);
            driver._enemy = enemy;
            driver._direction = direction;
            driver._speed = distance / time;
            driver._remaining = time;
            return driver;
        }

        private void LateUpdate()
        {
            float dt = Time.deltaTime;
            if (_enemy == null || _remaining <= 0f || (_enemy.Health != null && _enemy.Health.IsDead))
            {
                Destroy(this);
                return;
            }

            if (dt <= 0f)
            {
                return;
            }

            float step = Mathf.Min(dt, _remaining);
            _remaining -= step;
            _enemy.MoveInDirection(_direction, _speed, step);
        }
    }
}
