using System.Collections.Generic;
using Rapadura.Core.DI;
using Rapadura.Core.Events;
using Rapadura.Gameplay.Combat;
using Rapadura.Gameplay.Content;
using Rapadura.Gameplay.Dungeon;
using Rapadura.Gameplay.Enemies.Visuals;
using Rapadura.Gameplay.Environment;
using Rapadura.Gameplay.Inventory;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.Skills;
using Rapadura.Gameplay.World.Map.Mechanics;
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

        // Taotie's offerings (map 46): coins spilled round the vessel, which Taotie eats; what it ate comes back doubled when it falls.
        private readonly List<GameObject> _offerings = new List<GameObject>(RegionBossRules.TaotieMaxOfferingsOnGround);
        private readonly List<Vector2> _offeringSpots = new List<Vector2>(RegionBossRules.TaotieMaxOfferingsOnGround);
        private int _offeringsSpilled;
        private int _offeringsEaten;
        private float _spillTimer;
        private float _eatTimer;
        private float _eatWindup;
        private GameObject _eatTarget;
        private bool _paidOut;

        // Qiongqi's swap of the hero's steps (Player/HeroControlMirror ends it by itself; this only says so and ends it early).
        private bool _mirrorStarted;

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
            ResetOfferings();
            _mirrorStarted = false;
            EventBus.Subscribe<CombatTargetDiedEvent>(HandleTargetDied);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<CombatTargetDiedEvent>(HandleTargetDied);
            // The boss went to sleep, back to the pool or the scene changed: the offerings go (what Taotie ate is lost: it didn't fall)
            // and the steps come back at once.
            ResetOfferings();
            EndMirror();
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
                if (_health != null && _health.IsDead)
                {
                    EndMirror();
                }

                return;
            }

            Transform hero = _controller.Target;
            TickMirror(hero);
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
            if (_spawn.Signature == RegionBossSignature.FourAppetites)
            {
                TickOfferings(deltaTime, hero);
            }

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

                    if (victim.IsAlive && RegionBossRules.QiongqiSwapsSteps(_appetite, FlatDistance(hero.position, transform.position)))
                    {
                        HeroControlMirror.Begin(RegionBossRules.QiongqiMirrorSeconds);
                        _mirrorStarted = true;
                        DungeonFx.Warn(hero.position + Vector3.up * 2.6f,
                            string.Format(Text(RegionBossTexts.QiongqiMirrorKey), Mathf.RoundToInt(RegionBossRules.QiongqiMirrorSeconds)));
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

        // ---- Qiongqi: the swapped steps ----

        private void TickMirror(Transform hero)
        {
            if (!_mirrorStarted)
            {
                return;
            }

            PlayerStats stats = hero != null ? hero.GetComponentInParent<PlayerStats>() : null;
            if (hero == null || (stats != null && stats.IsDead))
            {
                EndMirror();
                return;
            }

            if (!HeroControlMirror.IsActive)
            {
                _mirrorStarted = false;
                DungeonFx.Say(hero.position + Vector3.up * 2.6f, Text(RegionBossTexts.QiongqiMirrorEndKey));
            }
        }

        /// <summary>Ends early the swap this boss started (never one it didn't start).</summary>
        private void EndMirror()
        {
            if (!_mirrorStarted)
            {
                return;
            }

            _mirrorStarted = false;
            HeroControlMirror.End();
        }

        // ---- Taotie: the offerings on the ground ----

        /// <summary>
        /// Picks up the offerings the hero walks over (whatever the appetite: they stay where they fell), and on Taotie's appetite spills
        /// new ones round the vessel and eats the nearest to its mouth after a ring on it and a warning.
        /// </summary>
        private void TickOfferings(float deltaTime, Transform hero)
        {
            if (hero != null)
            {
                PickUpOfferings(hero);
            }

            if (_eatWindup > 0f)
            {
                _eatWindup -= deltaTime;
                if (_eatWindup <= 0f)
                {
                    _eatWindup = 0f;
                    FinishEating();
                }

                return;
            }

            bool taotie = RegionBossRules.AppetiteFor(UtcDay(), IsNight()) == GluttonAppetite.Taotie;
            if (!taotie || hero == null || FlatDistance(hero.position, transform.position) > RegionBossRules.AbilityRange)
            {
                return;
            }

            _spillTimer -= deltaTime;
            if (_spillTimer <= 0f)
            {
                _spillTimer = RegionBossRules.TaotieSpillIntervalSeconds;
                if (RegionBossRules.TaotieShouldSpill(_offerings.Count, _offeringsSpilled))
                {
                    SpillOffering();
                }
            }

            _eatTimer -= deltaTime;
            if (_eatTimer <= 0f)
            {
                _eatTimer = RegionBossRules.TaotieEatIntervalSeconds;
                int index = RegionBossRules.TaotieChooseOffering(_offeringSpots, Flat(transform.position), Flat(hero.position));
                if (index >= 0)
                {
                    _eatTarget = _offerings[index];
                    _eatWindup = RegionBossRules.TaotieEatWindupSeconds;
                    Vector3 spot = _eatTarget.transform.position;
                    DungeonShockwave.Spawn(spot, null, _controller.gameObject, 0f, 0f);
                    DungeonFx.Warn(spot + Vector3.up * 2.4f, Text(RegionBossTexts.TaotieEatWarningKey));
                }
            }
        }

        private void SpillOffering()
        {
            Vector2 spot = RegionBossRules.TaotieSpillSpot(Flat(transform.position), Random.value, Random.value);
            Vector3 position = new Vector3(spot.x, transform.position.y, spot.y);
            GameObject offering = new GameObject("TaotieOffering");
            offering.transform.position = position;
            Color bronze = new Color(0.86f, 0.62f, 0.24f);
            DungeonProps.Part(offering.transform, PrimitiveType.Cylinder, "Coins", position + Vector3.up * 0.08f, Quaternion.identity,
                new Vector3(0.7f, 0.08f, 0.7f), bronze, null, glow: true);
            DungeonProps.Part(offering.transform, PrimitiveType.Cylinder, "Coins", position + new Vector3(0.12f, 0.22f, -0.08f), Quaternion.identity,
                new Vector3(0.45f, 0.07f, 0.45f), bronze, null, glow: true);
            _offerings.Add(offering);
            _offeringSpots.Add(spot);
            _offeringsSpilled++;
            DungeonShockwave.Spawn(position, null, _controller.gameObject, 0f, 0f);
            DungeonFx.Say(position + Vector3.up * 2f, Text(RegionBossTexts.TaotieSpillKey));
        }

        private void PickUpOfferings(Transform hero)
        {
            Vector2 heroFlat = Flat(hero.position);
            for (int i = _offerings.Count - 1; i >= 0; i--)
            {
                if (!RegionBossRules.TaotieOfferingPicked(_offeringSpots[i], heroFlat))
                {
                    continue;
                }

                Vector3 label = _offerings[i] != null ? _offerings[i].transform.position + Vector3.up * 1.6f : hero.position + Vector3.up * 2f;
                if (GrantCoins(hero.GetComponentInParent<InventoryManager>(), RegionBossRules.TaotieOfferingCoins, label) <= 0)
                {
                    continue; // nothing went in (no bag): it stays on the ground
                }

                if (_offerings[i] == _eatTarget)
                {
                    _eatTarget = null; // grabbed from under its mouth: FinishEating bites the air
                }

                RemoveOffering(i);
            }
        }

        private void FinishEating()
        {
            int index = _eatTarget != null ? _offerings.IndexOf(_eatTarget) : -1;
            _eatTarget = null;
            if (index < 0)
            {
                DungeonFx.Say(transform.position + Vector3.up * 4f, Text(RegionBossTexts.TaotieMissedKey));
                return;
            }

            Vector3 spot = _offerings[index].transform.position;
            RemoveOffering(index);
            _offeringsEaten++;
            DungeonShockwave.Spawn(spot, null, _controller.gameObject, 0f, 0f);
            DungeonFx.Warn(transform.position + Vector3.up * 4f,
                string.Format(Text(RegionBossTexts.TaotieEatenKey), RegionBossRules.TaotieDeathReturn(_offeringsEaten, 0)));
        }

        /// <summary>The boss fell: Taotie gives back what it ate, doubled, and the offerings still lying about go into the bag too.</summary>
        private void HandleTargetDied(CombatTargetDiedEvent evt)
        {
            if (_paidOut || !_armed || _controller == null || evt.Target != _controller.gameObject)
            {
                return;
            }

            EndMirror();
            int coins = RegionBossRules.TaotieDeathReturn(_offeringsEaten, _offerings.Count);
            bool doubled = _offeringsEaten > 0;
            ResetOfferings();
            _paidOut = true;
            if (coins <= 0)
            {
                return;
            }

            InventoryManager inventory = evt.Killer != null ? evt.Killer.GetComponentInParent<InventoryManager>() : null;
            if (inventory == null)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                inventory = player != null ? player.GetComponent<InventoryManager>() : null;
            }

            Vector3 label = transform.position + Vector3.up * 4.4f;
            if (GrantCoins(inventory, coins, label + Vector3.up * 0.4f) > 0 && doubled)
            {
                DungeonFx.Say(label, string.Format(Text(RegionBossTexts.TaotieReturnedKey), coins));
            }
        }

        /// <summary>Puts <paramref name="coins"/> of the offering's coin in <paramref name="inventory"/> through the loot path; returns how many went in.</summary>
        private static int GrantCoins(InventoryManager inventory, int coins, Vector3 label)
        {
            ItemDefinition coin = ItemDatabase.GetById(RegionBossRules.TaotieOfferingItemId);
            if (inventory == null || coin == null || coins <= 0)
            {
                return 0;
            }

            LootGrantResult result = LootGrantRules.Grant(inventory, new LootDrop(coin, coins), null);
            if (result.Granted > 0)
            {
                CombatFxManager.SpawnLootText(label, $"+{result.Granted} {coin.LocalizedName()}");
            }

            return result.Granted;
        }

        private void RemoveOffering(int index)
        {
            if (_offerings[index] != null)
            {
                Destroy(_offerings[index]);
            }

            _offerings.RemoveAt(index);
            _offeringSpots.RemoveAt(index);
        }

        private void ResetOfferings()
        {
            for (int i = _offerings.Count - 1; i >= 0; i--)
            {
                RemoveOffering(i);
            }

            _offeringsSpilled = 0;
            _offeringsEaten = 0;
            _spillTimer = RegionBossRules.TaotieFirstSpillSeconds;
            _eatTimer = RegionBossRules.TaotieFirstEatSeconds;
            _eatWindup = 0f;
            _eatTarget = null;
            _paidOut = false;
        }

        private float Damage(float factor) => _controller.EffectiveAttackDamage * factor * RegionBossRules.AbilityMultiplier(_spawn.StrongerAtNight, IsNight()) * SignatureMultiplier();

        /// <summary>What the boss's signature adds to every ability: Apep's clock near midnight, the form A Grande Treva wears, the Lodo's fountains gone dark.</summary>
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
                case RegionBossSignature.SwallowingMire:
                    return RegionBossRules.MireFountainMultiplier(LodoFountainBoard.LitCount(Time.time));
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
