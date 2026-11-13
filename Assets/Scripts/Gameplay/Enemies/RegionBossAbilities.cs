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
    public sealed partial class RegionBossAbilities : MonoBehaviour
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

        // The map bosses (RegionBossRules.MapBosses.cs): where the hero stood when the rope's last moment began, the echo of the bell's
        // last toll, how many times the Shisa King has roared and the Master of the Nine Furnaces has struck in this fight.
        private Vector3 _holdMark;
        private bool _holdMarked;
        private Vector3 _echo;
        private bool _hasEcho;
        private int _roars;
        private ShisaMouth _mouth;
        private int _strokes;
        private float _strokeHeat = 1f;

        // The map bosses of the middle story: the octopus's slams, the bridge's stomps, the Almas's throws and the carp's leaps in this fight
        // (each turns the pattern of the next), and the cart's run-up for the roll being telegraphed.
        private int _slams;
        private int _stomps;
        private int _throws;
        private int _leaps;
        private float _runUp;

        // The map bosses of the first regions and of 34, 45, 48: how far the hero was when the Fiandeira tied her thread, the steps the root marked
        // during its warning, the Borrão's smears and the colour it left, Nian's health when it began to stalk, and where the old griffin keeps his hoard.
        private float _threadTiedAt;
        private readonly List<Vector2> _rootSteps = new List<Vector2>(RegionBossRules.RootMaxSteps);
        private float _rootStepTimer;
        private int _smears;
        private SmudgeColour _smudgeClean;
        private float _healthAtWarning;
        private Vector3 _home;

        // The six more map bosses (05, 10, 22, 33, 39, 42): the worm's songs and the verse it sings, the bottler's uncorkings and where his bottles
        // stand, the fisher's two looks and where he cast, the vane's gusts and the wind, the eclipse's covers and the slice it darkens.
        private int _songs;
        private int _verse;
        private int _uncorkings;
        private Vector2[] _bottles;
        private Vector3 _lureFirstLook;
        private bool _lureFirstSeen;
        private bool _lureCast;
        private Vector2 _lure;
        private int _gusts;
        private VaneWind _wind;
        private int _covers;
        private int _cover;

        // The map bosses of Regions 10–16 (53, 61, 68, 73, 79, 93): the clumsy one's falls, the turtle's wake, the time the hero stood in the tar.
        private int _falls;
        private readonly List<Vector2> _wake = new List<Vector2>(RegionBossRules.WakeSteps);
        private float _wakeTimer;
        private float _tarSeconds;

        // The six more of Regions 1–9 (03, 11, 17, 21, 27, 47): the hero's distance when the Mãe-Lodo opened her mouth, the chief's tosses,
        // the bellows' breaths and the one it is taking, the vishap's rises and the one it is opening.
        private float _tongueAt;
        private int _tosses;
        private int _breaths;
        private bool _blowsOut;
        private int _rises;
        private int _rise;

        // The last of Regions 1–9 (16, 28, 29, 44, 49, 50, 51): the mason's builds and the quarter he walls in, the moray's looks at the hero,
        // the hero's distance when the scarecrow spread his arms, where the hero stood when the saw started turning or the herd started running.
        private int _builds;
        private int _walledIn;
        private float _morayLookTimer;
        private Vector2 _morayLastLook;
        private float _morayStill;
        private float _morayLongestStill;
        private float _scareAt;
        private Vector2 _heroAtWarning;
        private int _runs;
        private HerdWay _herdWay;

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
            _holdMarked = false;
            _hasEcho = false;
            _roars = 0;
            _strokes = 0;
            _strokeHeat = 1f;
            _slams = 0;
            _stomps = 0;
            _throws = 0;
            _leaps = 0;
            _runUp = 0f;
            _threadTiedAt = 0f;
            _rootSteps.Clear();
            _rootStepTimer = 0f;
            _smears = 0;
            _healthAtWarning = 0f;
            _songs = 0;
            _uncorkings = 0;
            _bottles = null;
            _lureFirstSeen = false;
            _lureCast = false;
            _gusts = 0;
            _covers = 0;
            _falls = 0;
            _wake.Clear();
            _wakeTimer = 0f;
            _tarSeconds = 0f;
            ResetUpperMapBosses();
            ResetDeepMapBosses();
            ResetSkyMapBosses();
            ResetRiverMapBosses();
            _tosses = 0;
            _breaths = 0;
            _rises = 0;
            _builds = 0;
            _morayLongestStill = 0f;
            _runs = 0;
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
            // Armed on the first frame after the director put it on its ring: where the old griffin keeps his gold.
            _home = transform.position;
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

            if (signature == RegionBossSignature.MoltenWake)
            {
                // The turtle leaves hot spots where it walks, all fight long: the last few are its wake.
                _wakeTimer += deltaTime;
                if (RegionBossRules.WakeMarksStep(_wakeTimer))
                {
                    _wakeTimer = 0f;
                    if (_wake.Count >= RegionBossRules.WakeSteps)
                    {
                        _wake.RemoveAt(0);
                    }

                    _wake.Add(Flat(transform.position));
                }
            }

            if (_signatureWindup > 0f)
            {
                _signatureWindup -= deltaTime;
                if (signature == RegionBossSignature.RopeCut && !_holdMarked && _signatureWindup <= RegionBossRules.RopeCutHoldSeconds && hero != null)
                {
                    // From here on the hero must be holding still: wherever they are now is where the rope will look for them.
                    _holdMark = hero.position;
                    _holdMarked = true;
                }

                if (signature == RegionBossSignature.RootTrail && hero != null)
                {
                    // The root follows the hero's steps under the ground while it warns.
                    _rootStepTimer += deltaTime;
                    if (RegionBossRules.RootMarksStep(_rootStepTimer) && _rootSteps.Count < RegionBossRules.RootMaxSteps)
                    {
                        _rootStepTimer = 0f;
                        _rootSteps.Add(Flat(hero.position));
                    }
                }

                if (signature == RegionBossSignature.TarSoak && hero != null && RegionBossRules.InTar(FlatDistance(hero.position, transform.position)))
                {
                    _tarSeconds += deltaTime;
                }

                if (signature == RegionBossSignature.StillPrey && hero != null)
                {
                    // The moray looks at the hero every so often while she warns: each look that finds them where they were adds to their stillness.
                    _morayLookTimer += deltaTime;
                    if (_morayLookTimer >= RegionBossRules.MorayLookSeconds)
                    {
                        _morayLookTimer = 0f;
                        Vector2 look = Flat(hero.position);
                        _morayStill = RegionBossRules.MorayLookIsStill(_morayLastLook, look) ? _morayStill + RegionBossRules.MorayLookSeconds : 0f;
                        _morayLongestStill = Mathf.Max(_morayLongestStill, _morayStill);
                        _morayLastLook = look;
                    }
                }

                if (signature == RegionBossSignature.LeadCast && hero != null)
                {
                    // The fisher looks twice at where the hero is going, then casts ahead of them.
                    if (!_lureFirstSeen && _signatureWindup <= RegionBossRules.LureFirstLookSeconds)
                    {
                        _lureFirstLook = hero.position;
                        _lureFirstSeen = true;
                    }
                    else if (_lureFirstSeen && !_lureCast && _signatureWindup <= RegionBossRules.LureSecondLookSeconds)
                    {
                        _lure = RegionBossRules.LureSpot(Flat(_lureFirstLook), Flat(hero.position));
                        _lureCast = true;
                        Vector3 lure = new Vector3(_lure.x, hero.position.y, _lure.y);
                        DungeonShockwave.Spawn(lure, null, _controller.gameObject, 0f, 0f);
                        DungeonFx.Say(lure + Vector3.up * 1.2f, Text(RegionBossTexts.LureKey));
                    }
                }

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
            else if (signature == RegionBossSignature.TwoMouths)
            {
                _mouth = RegionBossRules.ShisaMouthFor(_roars++);
                warning = RegionBossTexts.ShisaMouthKey(_mouth);
            }
            else if (signature == RegionBossSignature.RopeCut)
            {
                _holdMarked = false;
            }
            else if (signature == RegionBossSignature.EchoToll && _hasEcho)
            {
                DungeonShockwave.Spawn(_echo, null, _controller.gameObject, 0f, 0f);
            }
            else if (signature == RegionBossSignature.RunawayRun)
            {
                _runUp = FlatDistance(_signatureImpact, _signatureOrigin);
            }
            else if (signature == RegionBossSignature.SnowSparks)
            {
                // The three sparks it hid in the snow: the only places the burst passes by.
                foreach (Vector2 spark in RegionBossRules.SnowSparkSpots(Flat(_signatureOrigin), _throws))
                {
                    DungeonFx.Say(new Vector3(spark.x, _signatureOrigin.y + 1.2f, spark.y), Text(RegionBossTexts.SnowSparkKey));
                }
            }
            else if (signature == RegionBossSignature.FailedLeap)
            {
                bool fallsShort = RegionBossRules.LeapFallsShort(_leaps);
                warning = RegionBossTexts.CarpLeapKey(fallsShort);
                Vector2 landing = RegionBossRules.LeapLanding(_leaps, Flat(_signatureOrigin), Flat(_signatureImpact));
                DungeonShockwave.Spawn(new Vector3(landing.x, _signatureImpact.y, landing.y), null, _controller.gameObject, 0f, 0f);
            }
            else if (signature == RegionBossSignature.TautThread)
            {
                _threadTiedAt = FlatDistance(_signatureImpact, _signatureOrigin);
            }
            else if (signature == RegionBossSignature.RootTrail)
            {
                _rootSteps.Clear();
                _rootSteps.Add(Flat(_signatureImpact));
                _rootStepTimer = 0f;
            }
            else if (signature == RegionBossSignature.StolenColours)
            {
                _smudgeClean = RegionBossRules.SmudgeColourFor(_smears++);
                warning = RegionBossTexts.SmudgeColourKey(_smudgeClean);
            }
            else if (signature == RegionBossSignature.NoiseFright || signature == RegionBossSignature.GoldenTouch)
            {
                _healthAtWarning = _health != null ? _health.CurrentHealth : 0f;
            }
            else if (signature == RegionBossSignature.HoardGuard)
            {
                // The ring of his gold: the only place he strikes.
                DungeonShockwave.Spawn(_home, null, _controller.gameObject, 0f, 0f);
            }
            else if (signature == RegionBossSignature.DuneVerses)
            {
                _verse = RegionBossRules.DuneVerseOf(_songs++);
                warning = RegionBossTexts.DuneVerseKey(_verse);
            }
            else if (signature == RegionBossSignature.StaleAir)
            {
                // The three great bottles he sets down: the only shelter from the air he lets out.
                _bottles = RegionBossRules.BottleSpots(Flat(_signatureOrigin), _uncorkings++);
                foreach (Vector2 bottle in _bottles)
                {
                    Vector3 at = new Vector3(bottle.x, _signatureOrigin.y, bottle.y);
                    DungeonShockwave.Spawn(at, null, _controller.gameObject, 0f, 0f);
                    DungeonFx.Say(at + Vector3.up * 1.2f, Text(RegionBossTexts.BottleKey));
                }
            }
            else if (signature == RegionBossSignature.LeadCast)
            {
                _lureFirstSeen = false;
                _lureCast = false;
            }
            else if (signature == RegionBossSignature.VaneGust)
            {
                _wind = RegionBossRules.VaneWindFor(_gusts++);
                warning = RegionBossTexts.VaneWindKey(_wind);
            }
            else if (signature == RegionBossSignature.GrowingEclipse)
            {
                _cover = RegionBossRules.EclipsePhaseOf(_covers++);
                warning = RegionBossTexts.EclipseCoverKey(_cover);
            }
            else if (signature == RegionBossSignature.ClumsyFall)
            {
                // Where it will land: the far end of the strip it leans over.
                Vector2 end = Flat(_signatureOrigin) + RegionBossRules.FallDirection(_falls) * RegionBossRules.FallLength;
                DungeonShockwave.Spawn(new Vector3(end.x, _signatureOrigin.y, end.y), null, _controller.gameObject, 0f, 0f);
            }
            else if (signature == RegionBossSignature.MoltenWake)
            {
                foreach (Vector2 spot in _wake)
                {
                    DungeonShockwave.Spawn(new Vector3(spot.x, _signatureOrigin.y, spot.y), null, _controller.gameObject, 0f, 0f);
                }
            }
            else if (signature == RegionBossSignature.SledgeRope)
            {
                DungeonShockwave.Spawn(_home, null, _controller.gameObject, 0f, 0f);
            }
            else if (signature == RegionBossSignature.TarSoak)
            {
                _tarSeconds = 0f;
            }
            else if (signature == RegionBossSignature.TongueSnap)
            {
                _tongueAt = FlatDistance(_signatureImpact, _signatureOrigin);
            }
            else if (signature == RegionBossSignature.TinCrowns)
            {
                foreach (Vector2 crown in RegionBossRules.CrownSpots(Flat(_signatureImpact), _tosses))
                {
                    Vector3 at = new Vector3(crown.x, _signatureImpact.y, crown.y);
                    DungeonShockwave.Spawn(at, null, _controller.gameObject, 0f, 0f);
                    DungeonFx.Say(at + Vector3.up * 1.2f, Text(RegionBossTexts.CrownKey));
                }
            }
            else if (signature == RegionBossSignature.BellowsBreath)
            {
                _blowsOut = RegionBossRules.BellowsBlowsOut(_breaths++);
                warning = RegionBossTexts.BellowsBreathKey(_blowsOut);
            }
            else if (signature == RegionBossSignature.RisingSprings)
            {
                _rise = RegionBossRules.SpringsRiseOf(_rises++);
                warning = RegionBossTexts.SpringsRiseKey(_rise);
                DungeonShockwave.Spawn(_home, null, _controller.gameObject, 0f, 0f);
            }
            else if (signature == RegionBossSignature.ReliefPounce)
            {
                DungeonShockwave.Spawn(_home, null, _controller.gameObject, 0f, 0f);
            }
            else if (signature == RegionBossSignature.TurningWalls)
            {
                _walledIn = RegionBossRules.MasonQuarterOf(_builds, Flat(_signatureOrigin), Flat(_signatureImpact));
            }
            else if (signature == RegionBossSignature.StillPrey)
            {
                _morayLookTimer = 0f;
                _morayLastLook = Flat(_signatureImpact);
                _morayStill = 0f;
                _morayLongestStill = 0f;
            }
            else if (signature == RegionBossSignature.SidewaysScuttle)
            {
                // The row he will scuttle along runs through the hero's spot.
                DungeonShockwave.Spawn(_signatureImpact, null, _controller.gameObject, 0f, 0f);
            }
            else if (signature == RegionBossSignature.ScareOff)
            {
                _scareAt = FlatDistance(_signatureImpact, _signatureOrigin);
            }
            else if (signature == RegionBossSignature.OutrunSaw)
            {
                _heroAtWarning = Flat(_signatureImpact);
            }
            else if (signature == RegionBossSignature.HerdRun)
            {
                _heroAtWarning = Flat(_signatureImpact);
                _herdWay = RegionBossRules.HerdWayFor(_runs++);
                warning = RegionBossTexts.HerdWayKey(_herdWay);
            }
            else if (signature == RegionBossSignature.HoldHatches)
            {
                foreach (Vector2 hatch in RegionBossRules.HatchSpots(Flat(_home)))
                {
                    Vector3 at = new Vector3(hatch.x, _home.y, hatch.y);
                    DungeonShockwave.Spawn(at, null, _controller.gameObject, 0f, 0f);
                    DungeonFx.Say(at + Vector3.up * 1.2f, Text(RegionBossTexts.HatchKey));
                }
            }

            WarnUpperMapBoss(signature, ref warning);
            WarnDeepMapBoss(signature, ref warning);
            WarnSkyMapBoss(signature, ref warning);
            WarnRiverMapBoss(signature, ref warning);
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
            if (ResolveDeepMapBoss(heroFlat, victim))
            {
                return;
            }

            if (ResolveUpperMapBoss(heroFlat, victim) || ResolveSkyMapBoss(heroFlat, victim))
            {
                return;
            }

            if (ResolveRiverMapBoss(heroFlat, victim))
            {
                return;
            }

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

                case RegionBossSignature.RopeCut:
                    DungeonShockwave.Spawn(_signatureImpact, null, _controller.gameObject, 0f, 0f);
                    if (RegionBossRules.RopeCutHits(Flat(_holdMarked ? _holdMark : _signatureImpact), heroFlat))
                    {
                        victim.ApplyDamage(Damage(RegionBossRules.RopeCutDamageFactor), ElementType.Wind, _controller.gameObject);
                    }

                    _holdMarked = false;
                    break;

                case RegionBossSignature.EchoToll:
                    DungeonShockwave.Spawn(_signatureImpact, null, _controller.gameObject, 0f, 0f);
                    if (_hasEcho)
                    {
                        DungeonShockwave.Spawn(_echo, null, _controller.gameObject, 0f, 0f);
                    }

                    if (RegionBossRules.TollHits(Flat(_signatureImpact), _hasEcho ? Flat(_echo) : (Vector2?)null, heroFlat))
                    {
                        victim.ApplyDamage(Damage(RegionBossRules.TollDamageFactor), ElementType.Arcane, _controller.gameObject);
                    }

                    // This toll is the echo of the next one.
                    _echo = _signatureImpact;
                    _hasEcho = true;
                    break;

                case RegionBossSignature.BellyFlop:
                    DungeonShockwave.Spawn(_signatureImpact, null, _controller.gameObject, 0f, 0f);
                    float flop = RegionBossRules.FlopDamageFactor(Vector2.Distance(Flat(_signatureImpact), heroFlat));
                    if (flop > 0f)
                    {
                        victim.ApplyDamage(Damage(flop), ElementType.Ice, _controller.gameObject);
                    }

                    break;

                case RegionBossSignature.CeilingFall:
                    foreach (Vector2 stone in RegionBossRules.CeilingStones(Flat(_signatureImpact)))
                    {
                        DungeonShockwave.Spawn(new Vector3(stone.x, _signatureImpact.y, stone.y), null, _controller.gameObject, 0f, 0f);
                    }

                    if (RegionBossRules.CeilingFallHits(Flat(_signatureImpact), heroFlat))
                    {
                        victim.ApplyDamage(Damage(RegionBossRules.CeilingDamageFactor), ElementType.Earth, _controller.gameObject);
                    }

                    break;

                case RegionBossSignature.TwoMouths:
                    DungeonShockwave.Spawn(transform.position, null, _controller.gameObject, 0f, 0f);
                    if (RegionBossRules.ShisaHits(_mouth, FlatDistance(hero.position, transform.position)))
                    {
                        victim.ApplyDamage(Damage(RegionBossRules.ShisaDamageFactor), ElementType.Fire, _controller.gameObject);
                    }

                    break;

                case RegionBossSignature.ForgeStroke:
                    DungeonShockwave.Spawn(_signatureImpact, null, _controller.gameObject, 0f, 0f);
                    _strokeHeat = RegionBossRules.ForgeHeatMultiplier(_strokes++);
                    if (RegionBossRules.ForgeStrokeHits(Flat(_signatureImpact), heroFlat))
                    {
                        victim.ApplyDamage(Damage(RegionBossRules.ForgeDamageFactor), ElementType.Fire, _controller.gameObject);
                    }

                    _strokeHeat = 1f;
                    break;

                case RegionBossSignature.MonsoonArms:
                    DungeonShockwave.Spawn(transform.position, null, _controller.gameObject, 0f, 0f);
                    if (RegionBossRules.OctopusArmsHit(_slams++, Flat(transform.position), heroFlat))
                    {
                        victim.ApplyDamage(Damage(RegionBossRules.ArmsDamageFactor), ElementType.Wind, _controller.gameObject);
                    }

                    break;

                case RegionBossSignature.HerdSnort:
                    DungeonShockwave.Spawn(transform.position, null, _controller.gameObject, 0f, 0f);
                    if (RegionBossRules.SnortHits(Flat(transform.position), Flat(_signatureImpact) - Flat(_signatureOrigin), heroFlat))
                    {
                        victim.ApplyDamage(Damage(RegionBossRules.SnortDamageFactor), ElementType.Dark, _controller.gameObject);
                    }

                    break;

                case RegionBossSignature.ForgottenPlanks:
                    DungeonShockwave.Spawn(_signatureImpact, null, _controller.gameObject, 0f, 0f);
                    if (RegionBossRules.ForgottenPlankHits(_stomps++, Flat(_signatureOrigin), Flat(_signatureImpact), heroFlat))
                    {
                        victim.ApplyDamage(Damage(RegionBossRules.PlanksDamageFactor), ElementType.Earth, _controller.gameObject);
                    }

                    break;

                case RegionBossSignature.RunawayRun:
                    DungeonShockwave.Spawn(_signatureImpact, null, _controller.gameObject, 0f, 0f);
                    if (RegionBossRules.RunawayHits(Flat(_signatureImpact), heroFlat))
                    {
                        victim.ApplyDamage(Damage(RegionBossRules.RunawayDamageFactor(_runUp)), ElementType.Physical, _controller.gameObject);
                    }

                    break;

                case RegionBossSignature.SnowSparks:
                    DungeonShockwave.Spawn(_signatureOrigin, null, _controller.gameObject, 0f, 0f);
                    if (RegionBossRules.SnowBurstHits(Flat(_signatureOrigin), _throws++, heroFlat))
                    {
                        victim.ApplyDamage(Damage(RegionBossRules.SnowDamageFactor), ElementType.Ice, _controller.gameObject);
                    }

                    break;

                case RegionBossSignature.FailedLeap:
                    Vector2 splash = RegionBossRules.LeapLanding(_leaps, Flat(_signatureOrigin), Flat(_signatureImpact));
                    DungeonShockwave.Spawn(new Vector3(splash.x, _signatureImpact.y, splash.y), null, _controller.gameObject, 0f, 0f);
                    if (RegionBossRules.LeapHits(_leaps++, Flat(_signatureOrigin), Flat(_signatureImpact), heroFlat))
                    {
                        victim.ApplyDamage(Damage(RegionBossRules.LeapDamageFactor), ElementType.Physical, _controller.gameObject);
                    }

                    break;

                case RegionBossSignature.TautThread:
                    DungeonShockwave.Spawn(transform.position, null, _controller.gameObject, 0f, 0f);
                    if (RegionBossRules.TautThreadHits(_threadTiedAt, FlatDistance(hero.position, transform.position)))
                    {
                        victim.ApplyDamage(Damage(RegionBossRules.ThreadDamageFactor), ElementType.Physical, _controller.gameObject);
                    }

                    break;

                case RegionBossSignature.RootTrail:
                    foreach (Vector2 step in _rootSteps)
                    {
                        DungeonShockwave.Spawn(new Vector3(step.x, _signatureImpact.y, step.y), null, _controller.gameObject, 0f, 0f);
                    }

                    if (RegionBossRules.RootTrailHits(_rootSteps, heroFlat))
                    {
                        victim.ApplyDamage(Damage(RegionBossRules.RootDamageFactor), ElementType.Earth, _controller.gameObject);
                    }

                    _rootSteps.Clear();
                    break;

                case RegionBossSignature.StolenColours:
                    DungeonShockwave.Spawn(transform.position, null, _controller.gameObject, 0f, 0f);
                    if (RegionBossRules.SmudgeHits(_smudgeClean, FlatDistance(hero.position, transform.position)))
                    {
                        victim.ApplyDamage(Damage(RegionBossRules.SmudgeDamageFactor), ElementType.Dark, _controller.gameObject);
                    }

                    break;

                case RegionBossSignature.ShyGaze:
                    DungeonShockwave.Spawn(transform.position, null, _controller.gameObject, 0f, 0f);
                    if (RegionBossRules.GazeHits(Flat(transform.position), heroFlat, Flat(hero.forward)))
                    {
                        victim.ApplyDamage(Damage(RegionBossRules.GazeDamageFactor), ElementType.Poison, _controller.gameObject);
                    }

                    break;

                case RegionBossSignature.NoiseFright:
                    bool frightened = _health != null && RegionBossRules.NianTakesFright(_healthAtWarning, _health.CurrentHealth, _health.MaxHealth);
                    if (frightened)
                    {
                        DungeonFx.Warn(transform.position + Vector3.up * 4f, Text(RegionBossTexts.NianFrightKey));
                        break;
                    }

                    DungeonShockwave.Spawn(_signatureImpact, null, _controller.gameObject, 0f, 0f);
                    if (RegionBossRules.NianPounceHits(false, Flat(_signatureImpact), heroFlat))
                    {
                        victim.ApplyDamage(Damage(RegionBossRules.NianDamageFactor), ElementType.Fire, _controller.gameObject);
                    }

                    break;

                case RegionBossSignature.HoardGuard:
                    DungeonShockwave.Spawn(_home, null, _controller.gameObject, 0f, 0f);
                    if (RegionBossRules.HoardHits(Flat(_home), heroFlat))
                    {
                        victim.ApplyDamage(Damage(RegionBossRules.HoardDamageFactor), ElementType.Wind, _controller.gameObject);
                    }

                    break;

                case RegionBossSignature.MistStare:
                    DungeonShockwave.Spawn(transform.position, null, _controller.gameObject, 0f, 0f);
                    if (RegionBossRules.MistSnapHits(Flat(transform.position), heroFlat, Flat(hero.forward)))
                    {
                        victim.ApplyDamage(Damage(RegionBossRules.MistDamageFactor), ElementType.Physical, _controller.gameObject);
                    }

                    break;

                case RegionBossSignature.DuneVerses:
                    DungeonShockwave.Spawn(transform.position, null, _controller.gameObject, 0f, 0f);
                    if (RegionBossRules.VerseHits(_verse, FlatDistance(hero.position, transform.position)))
                    {
                        victim.ApplyDamage(Damage(RegionBossRules.VerseDamageFactor), ElementType.Earth, _controller.gameObject);
                    }

                    break;

                case RegionBossSignature.StaleAir:
                    DungeonShockwave.Spawn(_signatureOrigin, null, _controller.gameObject, 0f, 0f);
                    if (RegionBossRules.StaleAirHits(Flat(_signatureOrigin), _bottles, heroFlat))
                    {
                        victim.ApplyDamage(Damage(RegionBossRules.StaleDamageFactor), ElementType.Wind, _controller.gameObject);
                    }

                    _bottles = null;
                    break;

                case RegionBossSignature.LeadCast:
                    // If the looks were missed (the hero came into range late), he casts where the hero is.
                    Vector2 lureAt = _lureCast ? _lure : heroFlat;
                    DungeonShockwave.Spawn(new Vector3(lureAt.x, hero.position.y, lureAt.y), null, _controller.gameObject, 0f, 0f);
                    if (RegionBossRules.LureHits(lureAt, heroFlat))
                    {
                        victim.ApplyDamage(Damage(RegionBossRules.LureDamageFactor), ElementType.Dark, _controller.gameObject);
                    }

                    _lureFirstSeen = false;
                    _lureCast = false;
                    break;

                case RegionBossSignature.VaneGust:
                    DungeonShockwave.Spawn(transform.position, null, _controller.gameObject, 0f, 0f);
                    if (RegionBossRules.VaneGustHits(_wind, Flat(transform.position), heroFlat))
                    {
                        victim.ApplyDamage(Damage(RegionBossRules.GustDamageFactor), ElementType.Wind, _controller.gameObject);
                    }

                    break;

                case RegionBossSignature.GrowingEclipse:
                    DungeonShockwave.Spawn(transform.position, null, _controller.gameObject, 0f, 0f);
                    if (RegionBossRules.EclipseHits(_cover, Flat(transform.position), Flat(_signatureImpact), heroFlat))
                    {
                        victim.ApplyDamage(Damage(RegionBossRules.EclipseDamageFactor), ElementType.Dark, _controller.gameObject);
                    }

                    break;

                case RegionBossSignature.GoldenTouch:
                    DungeonShockwave.Spawn(transform.position, null, _controller.gameObject, 0f, 0f);
                    if (_health != null && RegionBossRules.GoldenTouchHits(_healthAtWarning, _health.CurrentHealth, _health.MaxHealth, FlatDistance(hero.position, transform.position)))
                    {
                        victim.ApplyDamage(Damage(RegionBossRules.GoldDamageFactor), ElementType.Holy, _controller.gameObject);
                    }

                    break;

                case RegionBossSignature.ClumsyFall:
                    Vector2 landed = Flat(_signatureOrigin) + RegionBossRules.FallDirection(_falls) * (RegionBossRules.FallLength * 0.5f);
                    DungeonShockwave.Spawn(new Vector3(landed.x, _signatureOrigin.y, landed.y), null, _controller.gameObject, 0f, 0f);
                    if (RegionBossRules.ClumsyFallHits(_falls++, Flat(_signatureOrigin), heroFlat))
                    {
                        victim.ApplyDamage(Damage(RegionBossRules.FallDamageFactor), ElementType.Earth, _controller.gameObject);
                    }

                    break;

                case RegionBossSignature.MoltenWake:
                    foreach (Vector2 spot in _wake)
                    {
                        DungeonShockwave.Spawn(new Vector3(spot.x, transform.position.y, spot.y), null, _controller.gameObject, 0f, 0f);
                    }

                    if (RegionBossRules.WakeHits(_wake, heroFlat))
                    {
                        victim.ApplyDamage(Damage(RegionBossRules.WakeDamageFactor), ElementType.Fire, _controller.gameObject);
                    }

                    _wake.Clear();
                    break;

                case RegionBossSignature.SledgeRope:
                    DungeonShockwave.Spawn(transform.position, null, _controller.gameObject, 0f, 0f);
                    if (RegionBossRules.SledgeRopeHits(Flat(_home), Flat(transform.position), heroFlat))
                    {
                        victim.ApplyDamage(Damage(RegionBossRules.SledgeDamageFactor), ElementType.Earth, _controller.gameObject);
                    }

                    break;

                case RegionBossSignature.TarSoak:
                    DungeonShockwave.Spawn(transform.position, null, _controller.gameObject, 0f, 0f);
                    if (RegionBossRules.TarSoakHits(_tarSeconds))
                    {
                        victim.ApplyDamage(Damage(RegionBossRules.TarDamageFactor), ElementType.Dark, _controller.gameObject);
                    }

                    _tarSeconds = 0f;
                    break;

                case RegionBossSignature.HoldHatches:
                    Vector2 hatchOut = RegionBossRules.NearestHatch(Flat(_home), heroFlat);
                    DungeonShockwave.Spawn(new Vector3(hatchOut.x, _home.y, hatchOut.y), null, _controller.gameObject, 0f, 0f);
                    if (RegionBossRules.HatchHits(Flat(_home), heroFlat))
                    {
                        victim.ApplyDamage(Damage(RegionBossRules.HatchDamageFactor), ElementType.Dark, _controller.gameObject);
                    }

                    break;

                case RegionBossSignature.TongueSnap:
                    DungeonShockwave.Spawn(transform.position, null, _controller.gameObject, 0f, 0f);
                    if (RegionBossRules.TongueSnapHits(_tongueAt, FlatDistance(hero.position, transform.position)))
                    {
                        victim.ApplyDamage(Damage(RegionBossRules.TongueDamageFactor), ElementType.Poison, _controller.gameObject);
                    }

                    break;

                case RegionBossSignature.TinCrowns:
                    foreach (Vector2 crown in RegionBossRules.CrownSpots(Flat(_signatureImpact), _tosses))
                    {
                        DungeonShockwave.Spawn(new Vector3(crown.x, _signatureImpact.y, crown.y), null, _controller.gameObject, 0f, 0f);
                    }

                    if (RegionBossRules.TinCrownsHit(Flat(_signatureImpact), _tosses++, heroFlat))
                    {
                        victim.ApplyDamage(Damage(RegionBossRules.CrownDamageFactor), ElementType.Physical, _controller.gameObject);
                    }

                    break;

                case RegionBossSignature.BellowsBreath:
                    DungeonShockwave.Spawn(transform.position, null, _controller.gameObject, 0f, 0f);
                    if (RegionBossRules.BellowsHits(_blowsOut, Flat(_signatureOrigin), Flat(_signatureImpact) - Flat(_signatureOrigin), heroFlat))
                    {
                        victim.ApplyDamage(Damage(RegionBossRules.BellowsDamageFactor), _blowsOut ? ElementType.Fire : ElementType.Wind, _controller.gameObject);
                    }

                    break;

                case RegionBossSignature.RisingSprings:
                    DungeonShockwave.Spawn(_home, null, _controller.gameObject, 0f, 0f);
                    if (RegionBossRules.SpringsHit(_rise, Flat(_home), heroFlat))
                    {
                        victim.ApplyDamage(Damage(RegionBossRules.SpringsDamageFactor), ElementType.Ice, _controller.gameObject);
                    }

                    break;

                case RegionBossSignature.ReliefPounce:
                    DungeonShockwave.Spawn(_signatureImpact, null, _controller.gameObject, 0f, 0f);
                    DungeonShockwave.Spawn(_home, null, _controller.gameObject, 0f, 0f);
                    if (RegionBossRules.ReliefPounceHits(Flat(_signatureImpact), Flat(_home), heroFlat))
                    {
                        victim.ApplyDamage(Damage(RegionBossRules.ReliefDamageFactor), ElementType.Earth, _controller.gameObject);
                    }

                    break;

                case RegionBossSignature.SmokeCorral:
                    DungeonShockwave.Spawn(_signatureImpact, null, _controller.gameObject, 0f, 0f);
                    if (RegionBossRules.SmokeCorralHits(Flat(_signatureImpact), heroFlat))
                    {
                        victim.ApplyDamage(Damage(RegionBossRules.CorralDamageFactor), ElementType.Dark, _controller.gameObject);
                    }

                    break;

                case RegionBossSignature.TurningWalls:
                    DungeonShockwave.Spawn(transform.position, null, _controller.gameObject, 0f, 0f);
                    if (RegionBossRules.MasonWallHits(_builds++, Flat(_signatureOrigin), _walledIn, heroFlat))
                    {
                        victim.ApplyDamage(Damage(RegionBossRules.MasonDamageFactor), ElementType.Earth, _controller.gameObject);
                    }

                    break;

                case RegionBossSignature.StillPrey:
                    DungeonShockwave.Spawn(hero.position, null, _controller.gameObject, 0f, 0f);
                    if (RegionBossRules.MorayBites(_morayLongestStill, FlatDistance(hero.position, transform.position)))
                    {
                        victim.ApplyDamage(Damage(RegionBossRules.MorayDamageFactor), ElementType.Dark, _controller.gameObject);
                    }

                    break;

                case RegionBossSignature.SidewaysScuttle:
                    DungeonShockwave.Spawn(_signatureImpact, null, _controller.gameObject, 0f, 0f);
                    if (RegionBossRules.ScuttleHits(Flat(_signatureOrigin), Flat(_signatureImpact), heroFlat))
                    {
                        victim.ApplyDamage(Damage(RegionBossRules.ScuttleDamageFactor), ElementType.Physical, _controller.gameObject);
                    }

                    break;

                case RegionBossSignature.ScareOff:
                    DungeonShockwave.Spawn(transform.position, null, _controller.gameObject, 0f, 0f);
                    if (RegionBossRules.ScareHits(_scareAt, FlatDistance(hero.position, transform.position)))
                    {
                        victim.ApplyDamage(Damage(RegionBossRules.ScareDamageFactor), ElementType.Wind, _controller.gameObject);
                    }

                    break;

                case RegionBossSignature.FearGrip:
                {
                    // The health the hero has when the dream lands: a potion drunk during the warning is what sends the nightmare off.
                    DungeonShockwave.Spawn(transform.position, null, _controller.gameObject, 0f, 0f);
                    PlayerStats dreamer = hero.GetComponentInParent<PlayerStats>();
                    float factor = dreamer != null
                        ? RegionBossRules.NightmareDamageFactor(dreamer.CurrentHealth, dreamer.MaxHealth, FlatDistance(hero.position, transform.position))
                        : RegionBossRules.NightmareDamageFactor(1f, 1f, FlatDistance(hero.position, transform.position));
                    if (factor > 0f)
                    {
                        victim.ApplyDamage(Damage(factor), ElementType.Dark, _controller.gameObject);
                    }

                    break;
                }

                case RegionBossSignature.OutrunSaw:
                    DungeonShockwave.Spawn(transform.position, null, _controller.gameObject, 0f, 0f);
                    if (RegionBossRules.SawHits(Flat(_signatureOrigin), _heroAtWarning, heroFlat))
                    {
                        victim.ApplyDamage(Damage(RegionBossRules.SawDamageFactor), ElementType.Physical, _controller.gameObject);
                    }

                    break;

                case RegionBossSignature.HerdRun:
                    DungeonShockwave.Spawn(transform.position, null, _controller.gameObject, 0f, 0f);
                    if (RegionBossRules.HerdRunHits(_herdWay, Flat(_signatureOrigin), _heroAtWarning, heroFlat))
                    {
                        victim.ApplyDamage(Damage(RegionBossRules.HerdDamageFactor), ElementType.Earth, _controller.gameObject);
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
                case RegionBossSignature.ForgeStroke:
                    return _strokeHeat;
                default:
                    return 1f;
            }
        }

        /// <summary>The signatures whose danger is drawn where the hero stood (the others are drawn round the boss).</summary>
        private static bool MarksTheHeroSpot(RegionBossSignature signature) =>
            signature == RegionBossSignature.DreamFall || signature == RegionBossSignature.PassBreath || signature == RegionBossSignature.FourAppetites
            || signature == RegionBossSignature.RopeCut || signature == RegionBossSignature.EchoToll || signature == RegionBossSignature.BellyFlop
            || signature == RegionBossSignature.CeilingFall || signature == RegionBossSignature.ForgeStroke
            || signature == RegionBossSignature.ForgottenPlanks || signature == RegionBossSignature.RunawayRun
            || signature == RegionBossSignature.RootTrail || signature == RegionBossSignature.NoiseFright
            || signature == RegionBossSignature.GrowingEclipse || signature == RegionBossSignature.ReliefPounce
            || signature == RegionBossSignature.SmokeCorral;

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
