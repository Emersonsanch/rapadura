using Rapadura.Core.DI;
using Rapadura.Gameplay.Combat;
using Rapadura.Gameplay.Dungeon;
using Rapadura.Gameplay.Enemies.Visuals;
using Rapadura.Gameplay.Environment;
using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.Skills;
using Rapadura.UI.Bestiary;
using Rapadura.Core.Localization;
using UnityEngine;

namespace Rapadura.Gameplay.Enemies
{
    /// <summary>
    /// The fight of a world boss of Regions 5–16, on its procedural model (added by <see cref="ProceduralEnemyModels.Instantiate(ProceduralEnemyBlueprint, Material, Transform)"/>,
    /// so every way of spawning one gets it; re-armed on enable for the pool). The regular <see cref="EnemyController"/> brain still chases and swings, like the
    /// ossuary's Colosso: this only adds on top. Each strike sends out a ring (<see cref="RegionBossRules.RingDamageFactor"/>), the Storm phase adds barrages of the boss's
    /// shot, the Last Stand a great blow telegraphed on the ground where the hero stood. Numbers in <see cref="RegionBossRules"/>.
    /// </summary>
    public sealed class RegionBossAbilities : MonoBehaviour
    {
        private EnemyController _controller;
        private Health _health;
        private RegionBossSpawn _spawn;
        private RegionBossPhase _phase;
        private RegionBossPhase _gravestPhase;
        private float _barrageTimer;
        private float _greatBlowTimer;
        private float _windupLeft;
        private Vector3 _impact;
        private int _lastStrike;
        private bool _armed;
        private float _signatureTimer;
        private float _signatureWindup;
        private Vector3 _signatureImpact;
        private float _fightSeconds;
        private GreatDarkForm _form;
        private Vector3 _signatureOrigin;
        private GluttonAppetite _appetite;

        public RegionBossPhase Phase => _phase;

        /// <summary>True while a great blow is telegraphed.</summary>
        public bool IsWindingUpGreatBlow => _windupLeft > 0f;

        public void Configure(string enemyId)
        {
            _spawn = RegionBossRules.SpawnOf(enemyId);
        }

        private void OnEnable()
        {
            _armed = false;
            _phase = RegionBossPhase.Wrath;
            _gravestPhase = RegionBossPhase.Wrath;
            _barrageTimer = RegionBossRules.BarrageIntervalSeconds * 0.5f;
            _greatBlowTimer = 1f;
            _windupLeft = 0f;
            _signatureTimer = 2f;
            _signatureWindup = 0f;
            _fightSeconds = 0f;
            _form = GreatDarkForm.Lion;
        }

        private bool Arm()
        {
            if (_armed)
            {
                return true;
            }

            _controller = GetComponentInParent<EnemyController>();
            if (_controller == null || _controller.Definition == null)
            {
                return false;
            }

            _health = _controller.Health != null ? _controller.Health : GetComponentInParent<Health>();
            _spawn = _spawn ?? RegionBossRules.SpawnOf(_controller.Definition.name);
            _lastStrike = _controller.StrikeSequence;
            _armed = _spawn != null;
            return _armed;
        }

        private void Update()
        {
            float deltaTime = Time.deltaTime;
            if (deltaTime <= 0f || !Arm() || (_health != null && _health.IsDead))
            {
                return;
            }

            Transform hero = _controller.Target;
            UpdatePhase();
            if (_controller.StrikeSequence != _lastStrike)
            {
                _lastStrike = _controller.StrikeSequence;
                if (hero != null)
                {
                    float reach = _controller.Definition.AttackRange + 0.3f;
                    DungeonShockwave.Spawn(transform.position, hero, _controller.gameObject, Damage(RegionBossRules.RingDamageFactor), reach);
                }
            }

            TickSignature(deltaTime, hero);

            if (_windupLeft > 0f)
            {
                TickWindup(deltaTime, hero);
                return;
            }

            if (hero == null || FlatDistance(hero.position, transform.position) > RegionBossRules.AbilityRange)
            {
                return;
            }

            int shots = RegionBossRules.ShotsFor(_phase);
            if (shots > 0)
            {
                _barrageTimer -= deltaTime;
                if (_barrageTimer <= 0f)
                {
                    _barrageTimer = RegionBossRules.BarrageIntervalSeconds;
                    FireBarrage(hero, shots + (_spawn.Signature == RegionBossSignature.FiveForms ? RegionBossRules.FormExtraShots(_form) : 0));
                }
            }

            if (RegionBossRules.GreatBlowEnabled(_phase))
            {
                _greatBlowTimer -= deltaTime;
                if (_greatBlowTimer <= 0f)
                {
                    _greatBlowTimer = RegionBossRules.GreatBlowIntervalSeconds;
                    StartGreatBlow(hero);
                }
            }
        }

        private void UpdatePhase()
        {
            float fraction = _health != null && _health.MaxHealth > 0f ? _health.CurrentHealth / _health.MaxHealth : 1f;
            RegionBossPhase phase = RegionBossRules.PhaseFor(fraction);
            if (phase == _phase)
            {
                return;
            }

            _phase = phase;
            if (phase == RegionBossPhase.LastStand)
            {
                _greatBlowTimer = 1f;
            }

            // Healing back over a threshold (Apep's drink) changes the phase but is not announced, nor is falling again to one already called.
            if (!RegionBossRules.ShouldAnnouncePhase(_gravestPhase, phase))
            {
                return;
            }

            _gravestPhase = phase;
            string key = RegionBossTexts.PhaseWarningKey(_spawn.EnemyId, phase);
            if (key != null)
            {
                DungeonFx.Warn(transform.position + Vector3.up * 4f, Text(key));
            }
        }

        private void FireBarrage(Transform hero, int shots)
        {
            Vector3 muzzle = EnemyRangedAttack.MuzzleOf(_controller.transform);
            Vector3 side = _controller.transform.right;
            float damage = Damage(RegionBossRules.BarrageDamageFactor);
            for (int i = 0; i < shots; i++)
            {
                EnemyRangedAttack.Launch(_spawn.Shot, muzzle + side * RegionBossRules.ShotOffset(i, shots), hero, _controller.gameObject, damage, RegionBossRules.BarrageRange);
            }
        }

        private void StartGreatBlow(Transform hero)
        {
            _impact = hero.position;
            _windupLeft = RegionBossRules.GreatBlowWindupSeconds;
            _controller.HoldAggro(RegionBossRules.GreatBlowWindupSeconds);
            DungeonShockwave.Spawn(_impact, null, _controller.gameObject, 0f, 0f);
            DungeonFx.Warn(_impact + Vector3.up * 3f, Text(RegionBossTexts.GreatBlowKey(_spawn.EnemyId)));
        }

        private void TickWindup(float deltaTime, Transform hero)
        {
            _windupLeft -= deltaTime;
            if (_windupLeft > 0f)
            {
                return;
            }

            _windupLeft = 0f;
            DungeonShockwave.Spawn(_impact, null, _controller.gameObject, 0f, 0f);
            if (hero == null || !RegionBossRules.GreatBlowHits(new Vector2(_impact.x, _impact.z), new Vector2(hero.position.x, hero.position.z)))
            {
                return;
            }

            ICombatTarget victim = hero.GetComponentInParent<ICombatTarget>();
            if (victim != null && victim.IsAlive)
            {
                victim.ApplyDamage(Damage(RegionBossRules.GreatBlowDamageFactor), ElementType.Physical, _controller.gameObject);
            }
        }

        /// <summary>The ability only this boss has (<see cref="RegionBossSignature"/>); the forms run the whole fight, the others from the Storm phase.</summary>
        private void TickSignature(float deltaTime, Transform hero)
        {
            RegionBossSignature signature = _spawn.Signature;
            if (signature == RegionBossSignature.None)
            {
                return;
            }

            if (signature == RegionBossSignature.FiveForms)
            {
                TickForms(deltaTime, hero);
                return;
            }

            if (_signatureWindup > 0f)
            {
                _signatureWindup -= deltaTime;
                if (_signatureWindup <= 0f)
                {
                    _signatureWindup = 0f;
                    ResolveSignature(hero);
                }

                return;
            }

            if (hero == null || !RegionBossRules.SignatureEnabled(signature, _phase) || FlatDistance(hero.position, transform.position) > RegionBossRules.AbilityRange)
            {
                return;
            }

            _signatureTimer -= deltaTime;
            if (_signatureTimer > 0f)
            {
                return;
            }

            _signatureTimer = RegionBossRules.SignatureInterval(signature);
            _signatureWindup = RegionBossRules.SignatureWindup(signature);
            _signatureImpact = hero.position;
            _signatureOrigin = transform.position;
            _controller.HoldAggro(_signatureWindup);
            string warning = RegionBossTexts.SignatureKey(_spawn.EnemyId);
            if (signature == RegionBossSignature.FourAppetites)
            {
                _appetite = RegionBossRules.AppetiteFor(UtcDay(), IsNight());
                warning = RegionBossTexts.AppetiteKey(_appetite);
            }

            DungeonShockwave.Spawn(MarksTheHeroSpot(signature) ? _signatureImpact : transform.position, null, _controller.gameObject, 0f, 0f);
            DungeonFx.Warn(transform.position + Vector3.up * 4f, Text(warning));
        }

        private void TickForms(float deltaTime, Transform hero)
        {
            if (hero == null || FlatDistance(hero.position, transform.position) > RegionBossRules.AbilityRange)
            {
                return;
            }

            _fightSeconds += deltaTime;
            GreatDarkForm form = RegionBossRules.FormAt(_fightSeconds);
            if (form == _form)
            {
                return;
            }

            _form = form;
            if (_phase != RegionBossPhase.Wrath)
            {
                DungeonFx.Warn(transform.position + Vector3.up * 4f, Text(RegionBossTexts.FormKey(form)));
            }
        }

        private void ResolveSignature(Transform hero)
        {
            if (hero == null)
            {
                return;
            }

            ICombatTarget victim = hero.GetComponentInParent<ICombatTarget>();
            if (victim == null || !victim.IsAlive)
            {
                return;
            }

            Vector2 heroFlat = new Vector2(hero.position.x, hero.position.z);
            switch (_spawn.Signature)
            {
                case RegionBossSignature.DrinkTheRiver:
                    DungeonShockwave.Spawn(transform.position, null, _controller.gameObject, 0f, 0f);
                    if (RegionBossRules.DrinkHits(FlatDistance(hero.position, transform.position)))
                    {
                        victim.ApplyDamage(Damage(RegionBossRules.DrinkDamageFactor), ElementType.Dark, _controller.gameObject);
                        if (_health != null)
                        {
                            _health.ApplyHealing(_health.MaxHealth * RegionBossRules.DrinkHealFraction, _controller.gameObject);
                        }
                    }

                    break;

                case RegionBossSignature.DreamFall:
                    DungeonShockwave.Spawn(_signatureImpact, null, _controller.gameObject, 0f, 0f);
                    if (RegionBossRules.DreamFallHits(new Vector2(_signatureImpact.x, _signatureImpact.z), heroFlat))
                    {
                        victim.ApplyDamage(Damage(RegionBossRules.DreamFallDamageFactor), ElementType.Arcane, _controller.gameObject);
                    }

                    break;

                case RegionBossSignature.GluttonBite:
                    DungeonShockwave.Spawn(transform.position, null, _controller.gameObject, 0f, 0f);
                    if (RegionBossRules.BiteHits(Flat(transform.position), Flat(_signatureImpact) - Flat(_signatureOrigin), heroFlat))
                    {
                        victim.ApplyDamage(Damage(RegionBossRules.BiteDamageFactor), ElementType.Physical, _controller.gameObject);
                    }

                    break;

                case RegionBossSignature.SwallowingMire:
                    DungeonShockwave.Spawn(transform.position, null, _controller.gameObject, 0f, 0f);
                    if (RegionBossRules.MireHits(FlatDistance(hero.position, transform.position), _phase))
                    {
                        victim.ApplyDamage(Damage(RegionBossRules.MireDamageFactor), ElementType.Dark, _controller.gameObject);
                    }

                    break;

                case RegionBossSignature.PassBreath:
                    DungeonShockwave.Spawn(_signatureImpact, null, _controller.gameObject, 0f, 0f);
                    if (RegionBossRules.BreathHits(Flat(_signatureOrigin), Flat(_signatureImpact), heroFlat))
                    {
                        victim.ApplyDamage(Damage(RegionBossRules.BreathDamageFactor), ElementType.Fire, _controller.gameObject);
                    }

                    break;

                case RegionBossSignature.FourAppetites:
                    DungeonShockwave.Spawn(_signatureImpact, null, _controller.gameObject, 0f, 0f);
                    if (RegionBossRules.AppetiteHits(_appetite, Flat(_signatureImpact), Flat(transform.position), heroFlat))
                    {
                        victim.ApplyDamage(Damage(RegionBossRules.AppetiteDamageFactor), ElementType.Arcane, _controller.gameObject);
                    }

                    break;

                case RegionBossSignature.ChainSweep:
                    DungeonShockwave.Spawn(transform.position, null, _controller.gameObject, 0f, 0f);
                    if (RegionBossRules.ChainSweepHits(FlatDistance(hero.position, transform.position)))
                    {
                        victim.ApplyDamage(Damage(RegionBossRules.ChainDamageFactor), ElementType.Physical, _controller.gameObject);
                    }

                    break;

                case RegionBossSignature.GreedOfTheArk:
                    // The hero's health lives on PlayerStats, not on a Health. The Ark never kills (GreedDamage stops at 1 life left), so it goes
                    // through the path that cannot pass that floor: ApplyHazardDamage caps the defence multiplier at 1 and keeps 1 life.
                    PlayerStats stats = hero.GetComponentInParent<PlayerStats>();
                    if (stats != null && !stats.IsInvulnerable)
                    {
                        float greed = RegionBossRules.GreedDamage(stats.CurrentHealth);
                        if (greed > 0f)
                        {
                            stats.ApplyHazardDamage(greed);
                        }
                    }

                    break;
            }
        }

        private float Damage(float factor) => _controller.EffectiveAttackDamage * factor * RegionBossRules.AbilityMultiplier(_spawn.StrongerAtNight, IsNight()) * SignatureMultiplier();

        /// <summary>What the boss's signature adds to every ability: Apep's clock near midnight, the form A Grande Treva wears.</summary>
        private float SignatureMultiplier()
        {
            switch (_spawn.Signature)
            {
                case RegionBossSignature.DrinkTheRiver:
                    return ServiceLocator.TryGet(out TimeOfDayManager time) && time != null ? RegionBossRules.MidnightMultiplier(time.CurrentHour) : 1f;
                case RegionBossSignature.FiveForms:
                    return RegionBossRules.FormDamageMultiplier(_form);
                case RegionBossSignature.FourAppetites:
                    return RegionBossRules.AppetiteMultiplier(_appetite, IsNight());
                default:
                    return 1f;
            }
        }

        /// <summary>The signatures whose danger is drawn where the hero stood (the others are drawn round the boss).</summary>
        private static bool MarksTheHeroSpot(RegionBossSignature signature) =>
            signature == RegionBossSignature.DreamFall || signature == RegionBossSignature.PassBreath || signature == RegionBossSignature.FourAppetites;

        private static long UtcDay() => (long)(System.DateTime.UtcNow - new System.DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc)).TotalDays;

        private static Vector2 Flat(Vector3 p) => new Vector2(p.x, p.z);

        private static bool IsNight() => ServiceLocator.TryGet(out TimeOfDayManager time) && time != null && time.Phase == TimeOfDayPhase.Night;

        private static float FlatDistance(Vector3 a, Vector3 b)
        {
            a.y = 0f;
            b.y = 0f;
            return Vector3.Distance(a, b);
        }

        private static string Text(string key)
        {
            ServiceLocator.TryGet(out LocalizationManager localization);
            return BestiaryTexts.Get(localization, key);
        }
    }
}
