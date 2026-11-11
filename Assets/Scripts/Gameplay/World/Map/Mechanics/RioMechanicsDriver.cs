using System;
using System.Collections.Generic;
using Rapadura.Core.DI;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Combat;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Environment;
using Rapadura.Gameplay.Inventory;
using Rapadura.Gameplay.Mounts;
using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.Skills;
using Rapadura.Gameplay.World.Cities.Town;
using Rapadura.Gameplay.World.Map.Areas;
using Rapadura.Gameplay.World.Map.Overworld;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// Scene side of the Rio das Doze Horas mechanics (the rules are <see cref="RioHourRules"/>, <see cref="RioFireRules"/> and
    /// <see cref="RioMehenRules"/>, and for the stranded barque of 76 <see cref="RioBarqueDefenseRules"/>): added to the overworld root, idle unless the layout holds the region and the hero stands in it.
    /// The barque boards at the pier of the hour and sets the hero down at a post, and every post has a berth back; the ring of Mehen
    /// turns over the scale wall and its gates carry the hero across while an opening is held; the Lake of Fire mends the hero at
    /// peace and bites the beasts on its shore; on Apep's sandbank the hero guards the stranded barque while the rowers free it (the controls
    /// stay with the hero), and the freed barque sails the dry river for the rest of the in-game day (the day is saved with
    /// <c>WorldMapProgress.SetDailyMark</c>). A ride takes the controls for at most 12 s, and hands them back on arrival, on death
    /// or if anything else moves the hero.
    /// </summary>
    public sealed class RioMechanicsDriver : MonoBehaviour, IRegionMechanics
    {
        private const string PlayerTag = "Player";
        private const float HintCooldown = 3f;
        private const float DeniedCooldown = 1.5f;
        private const float ExternalMoveTolerance = 3f;
        private const float FeedbackHeight = 2.4f;
        private const float RiverViewRange = 700f;
        private const float RingViewRange = 320f;
        private const float BerthViewRange = 120f;
        private const float FireCheckRange = 40f;
        private const float RingLift = 5f;
        private const int RingHalfSegments = 24;
        private const float RingOpeningDegrees = 22f;
        private const float BeaconClosedHeight = 5f;
        private const float BeaconOpenHeight = 22f;
        private const float DefenseViewRange = 120f;
        private const float DefenseStatusSeconds = 6f;
        private const int PullRingSegments = 24;
        private const string FreedMarkKey = "rio.barque.freed";

        private readonly List<LineRenderer> _pierBeacons = new List<LineRenderer>();
        private readonly List<LineRenderer> _berthBeacons = new List<LineRenderer>();
        private readonly List<(SkyRoute Route, bool ToB)> _stations = new List<(SkyRoute, bool)>();
        private readonly LineRenderer[] _ringHalves = new LineRenderer[2];
        private readonly HeroRequirementCache _heroState = new HeroRequirementCache();
        private readonly RioBarqueDefense _defense = new RioBarqueDefense();

        private OverworldView _view;
        private OverworldLayout _layout;
        private Vector2 _origin;
        private Transform _player;
        private PlayerController _controller;
        private PlayerMotor _motor;
        private CharacterController _character;
        private InventoryManager _bag;
        private PlayerStats _stats;
        private Material _material;
        private LineRenderer _barque;
        private LineRenderer _fireGlow;

        private bool _riding;
        private Vector2 _rideFrom;
        private Vector2 _rideTo;
        private Vector2 _rideLand;
        private float _rideArc;
        private float _rideT;
        private float _rideSeconds;
        private float _rideFromY;
        private float _rideToY;
        private bool _rideHugsGround;
        private Vector3 _lastRidePosition;

        private int _shownHour = -1;
        private bool _shownHourSpoken;
        private float _nextHint;
        private float _nextDenied;
        private int _hintPier = -1;
        private int _hintBerth = -1;
        private SkyRoute _hintGate;

        private float _lastHurtAt = float.NegativeInfinity;
        private float _lastHealth = -1f;
        private float _fireTick;
        private bool _saidFire;
        private bool _saidFireWaits;
        private bool _saidBurn;

        private float _ringY;

        private LineRenderer _hullLine;
        private LineRenderer _hullBar;
        private LineRenderer _pullRing;
        private bool _hintHull;
        private SkyRoute _hintSail;
        private float _nextDefenseStatus;
        private float _lastDefenseFail = float.NegativeInfinity;
        private int _freedDay;

        private Rect[] _areas;

        /// <inheritdoc/>
        public Rect[] MechanicsAreas => _areas;

        /// <inheritdoc/>
        public bool MechanicsBusy => _riding;

        private void Start()
        {
            _view = GetComponent<OverworldView>();
            _layout = _view != null ? _view.Layout : null;
            OverworldZone anchor = _layout?.Zone(OverworldLayouts.RioAnchorZoneId);
            if (anchor == null)
            {
                enabled = false;
                return;
            }

            _origin = anchor.Origin;
            _areas = new[] { MechanicsDriverHub.WorldArea(OverworldLayouts.RioBounds, _origin) };
        }

        private void OnEnable() => _heroState.Attach();

        private void OnDisable()
        {
            EndRide(false);
            HideFeedback();
            _heroState.Detach();
        }

        private void OnDestroy()
        {
            if (_material != null)
            {
                Destroy(_material);
            }
        }

        private void Update()
        {
            if (!FindPlayer())
            {
                return;
            }

            if (_riding)
            {
                TickRide();
                return;
            }

            Vector2 local = new Vector2(_player.position.x, _player.position.z) - _origin;
            if (!OverworldLayouts.RioBounds.Contains(local))
            {
                HideFeedback();
                return;
            }

            double now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / 1000d;
            long unix = (long)Math.Floor(now);
            TickRiver(local, unix);
            TickRings(local, now);
            TickFire(local);
            TickDefense(local);
        }

        private bool FindPlayer()
        {
            if (_player == null)
            {
                GameObject go = GameObject.FindGameObjectWithTag(PlayerTag);
                if (go == null)
                {
                    return false;
                }

                _player = go.transform;
                _controller = go.GetComponent<PlayerController>();
                _motor = go.GetComponent<PlayerMotor>();
                _character = go.GetComponent<CharacterController>();
                _bag = go.GetComponentInChildren<InventoryManager>();
                _stats = go.GetComponent<PlayerStats>();
            }

            return _controller != null && _motor != null && _character != null;
        }

        /// <summary>The soft barrier's verdict on the zone at a point of the region, for this hero.</summary>
        private bool ZoneOpen(Vector2 local)
        {
            MapRequirementState state = _heroState.Get(_bag);
            MapModeController mode = MapModeController.Current;
            int level = mode != null ? mode.HeroLevel : 1;
            OverworldZone zone = OverworldRules.ZoneAt(_layout, _origin + local);
            return zone != null && OverworldRules.CheckEnter(zone, level, state.Hero, _heroState.Lookup) == MapAccessCheck.Ok;
        }

        // ------------------------------------------------------------------
        // The hours of the barque
        // ------------------------------------------------------------------

        private void TickRiver(Vector2 local, long unix)
        {
            int hour = RioHourRules.HourIndex(unix);
            ShowPiers(local, hour);
            AnnounceHour(local, hour);

            int pier = RioHourRules.NearestWithinReach(OverworldLayouts.HourPiers, local);
            if (pier >= 0)
            {
                TickPier(pier, unix);
                return;
            }

            _hintPier = -1;
            int berth = RioHourRules.NearestWithinReach(OverworldLayouts.BarqueBerths, local);
            if (berth < 0)
            {
                _hintBerth = -1;
                return;
            }

            // The way back is always served: a hero who came by barque, or on foot, can always go home from a post.
            if (_hintBerth != berth || Time.unscaledTime >= _nextHint)
            {
                _hintBerth = berth;
                _nextHint = Time.unscaledTime + HintCooldown;
                Say(RioMechanicsTexts.ReturnBerth);
            }

            InteractPromptRegistry.Ping(Time.unscaledTime);
            if (_controller.Input.InteractPressedThisFrame)
            {
                BeginBarque(OverworldLayouts.QuayBerth);
            }
        }

        private void AnnounceHour(Vector2 local, int hour)
        {
            if (hour == _shownHour && _shownHourSpoken)
            {
                return;
            }

            _shownHour = hour;
            bool nearRiver = Mathf.Abs(local.x) < RiverViewRange && Mathf.Abs(local.y - 200f) < RiverViewRange * 0.5f;
            _shownHourSpoken = nearRiver;
            if (nearRiver)
            {
                ServiceLocator.TryGet(out LocalizationManager localization);
                SayText(RioMechanicsTexts.Format(localization, RioMechanicsTexts.HourChanged, hour + 1, DestinationName(localization, hour)));
            }
        }

        private static string DestinationName(LocalizationManager localization, int pier) =>
            RioMechanicsTexts.Get(localization, RioMechanicsTexts.Destinations[RioHourRules.DestinationOf(pier)]);

        private void TickPier(int pier, long unix)
        {
            Vector2 berth = OverworldLayouts.BarqueBerths[RioHourRules.DestinationOf(pier)];
            RioBarqueCheck check = RioHourRules.Check(pier, unix, ZoneOpen(berth));
            if (_hintPier != pier || Time.unscaledTime >= _nextHint)
            {
                _hintPier = pier;
                _nextHint = Time.unscaledTime + HintCooldown;
                SayPier(check, pier, unix);
            }

            InteractPromptRegistry.Ping(Time.unscaledTime);
            if (!_controller.Input.InteractPressedThisFrame)
            {
                return;
            }

            if (check == RioBarqueCheck.Board)
            {
                BeginBarque(berth);
            }
            else if (Time.unscaledTime >= _nextDenied)
            {
                _nextDenied = Time.unscaledTime + DeniedCooldown;
                SayPier(check, pier, unix);
            }
        }

        private void SayPier(RioBarqueCheck check, int pier, long unix)
        {
            ServiceLocator.TryGet(out LocalizationManager localization);
            switch (check)
            {
                case RioBarqueCheck.Board:
                    SayText(RioMechanicsTexts.Format(localization, RioMechanicsTexts.Board, DestinationName(localization, pier)));
                    break;
                case RioBarqueCheck.ZoneBlocked:
                    SayText(RioMechanicsTexts.Format(localization, RioMechanicsTexts.BarqueBlocked, DestinationName(localization, pier)));
                    break;
                default:
                    SayText(RioMechanicsTexts.Format(localization, RioMechanicsTexts.PierClosed, pier + 1, RioHourRules.Clock(RioHourRules.SecondsUntilOpen(pier, unix))));
                    break;
            }
        }

        private void BeginBarque(Vector2 berth)
        {
            MountController.Active?.Dismount(startRecall: false, leave: false);
            Say(RioMechanicsTexts.Boarding);
            Vector2 here = new Vector2(_player.position.x, _player.position.z) - _origin;
            BeginRide(here, here, berth, RioHourRules.BoardingArc, RioHourRules.BoardingSeconds);
        }

        // ------------------------------------------------------------------
        // The turning ring of Mehen
        // ------------------------------------------------------------------

        private void TickRings(Vector2 local, double now)
        {
            ShowRing(local, now);
            SkyRouteRules.StationsAt(OverworldLayouts.MehenGates, local, _stations);
            if (_stations.Count == 0)
            {
                _hintGate = null;
                return;
            }

            (SkyRoute route, bool toB) = _stations[0];
            int gate = Array.IndexOf(OverworldLayouts.MehenGates, route);
            bool open = RioMehenRules.IsGateOpen(gate, now);
            bool zones = SkyRouteRules.AllZonesOpen(_layout, _origin, route, HeroLevel(), HeroRequirements().hero, HeroRequirements().has);
            if (_hintGate != route || Time.unscaledTime >= _nextHint)
            {
                _hintGate = route;
                _nextHint = Time.unscaledTime + HintCooldown;
                SayGate(open, zones, gate, now);
            }

            InteractPromptRegistry.Ping(Time.unscaledTime);
            if (!_controller.Input.InteractPressedThisFrame)
            {
                return;
            }

            if (open && zones)
            {
                Vector2 from = SkyRouteRules.PositionAt(route, toB, 0f);
                Vector2 to = SkyRouteRules.PositionAt(route, toB, 1f);
                BeginRide(from, to, to, RioMehenRules.CrossArc, RioMehenRules.CrossSeconds(Vector2.Distance(from, to)));
            }
            else if (Time.unscaledTime >= _nextDenied)
            {
                _nextDenied = Time.unscaledTime + DeniedCooldown;
                SayGate(open, zones, gate, now);
            }
        }

        private static int HeroLevel()
        {
            MapModeController mode = MapModeController.Current;
            return mode != null ? mode.HeroLevel : 1;
        }

        private (Characters.CharacterId? hero, Func<string, bool> has) HeroRequirements()
        {
            MapRequirementState state = _heroState.Get(_bag);
            return (state.Hero, _heroState.Lookup);
        }

        private void SayGate(bool open, bool zones, int gate, double now)
        {
            if (!zones)
            {
                Say(RioMechanicsTexts.GateBlocked);
            }
            else if (open)
            {
                Say(RioMechanicsTexts.GateOpen);
            }
            else
            {
                ServiceLocator.TryGet(out LocalizationManager localization);
                SayText(RioMechanicsTexts.Format(localization, RioMechanicsTexts.GateShut, Mathf.CeilToInt(RioMehenRules.SecondsUntilOpen(gate, now))));
            }
        }

        // ------------------------------------------------------------------
        // Rides
        // ------------------------------------------------------------------

        /// <summary><paramref name="land"/> is where the hero is set down at the end; for the barque it is far from where the ride bobs.</summary>
        private void BeginRide(Vector2 from, Vector2 to, Vector2 land, float arc, float seconds, bool hugGround = false)
        {
            _riding = true;
            _rideHugsGround = hugGround;
            _rideFrom = from;
            _rideTo = to;
            _rideLand = land;
            _rideArc = arc;
            _rideT = 0f;
            _rideSeconds = Mathf.Max(0.1f, seconds);
            _rideFromY = Mathf.Max(0f, OverworldRules.SurfaceAt(_layout, _origin + from));
            _rideToY = Mathf.Max(0f, OverworldRules.SurfaceAt(_layout, _origin + to));

            // Clears the motor's velocity so the Animator rests in idle while the controls are off.
            Vector3 facing = new Vector3(land.x - from.x, 0f, land.y - from.y);
            _motor.Teleport(_player.position, facing.sqrMagnitude > 0.01f ? Quaternion.LookRotation(facing) : _player.rotation);
            _controller.ClickToMove?.ClearDestination();
            _controller.enabled = false;
            _character.enabled = false;
            _lastRidePosition = _player.position;
        }

        private void TickRide()
        {
            bool dead = _stats != null && !_stats.IsAlive;
            if (dead || (_player.position - _lastRidePosition).sqrMagnitude > ExternalMoveTolerance * ExternalMoveTolerance)
            {
                EndRide(false);
                return;
            }

            _rideT = Mathf.Min(1f, _rideT + Time.deltaTime / _rideSeconds);
            Vector2 ground = _origin + Vector2.Lerp(_rideFrom, _rideTo, _rideT);
            float y = SkyRouteRules.HeightAt(_rideFromY, _rideToY, _rideArc, _rideT);
            if (_rideHugsGround)
            {
                // The freed barque rides over Apep's coils instead of through them.
                y = Mathf.Max(y, Mathf.Max(0f, OverworldRules.SurfaceAt(_layout, ground)) + RioBarqueDefenseRules.SailArc);
            }

            _player.position = new Vector3(ground.x, y, ground.y);
            _lastRidePosition = _player.position;
            if (_rideT >= 1f)
            {
                EndRide(true);
            }
        }

        private void EndRide(bool arrived)
        {
            if (!_riding)
            {
                return;
            }

            _riding = false;
            if (_character != null)
            {
                _character.enabled = true;
            }

            if (_controller != null)
            {
                _controller.enabled = true;
            }

            if (arrived && _view != null && _player != null)
            {
                Vector2 end = _origin + _rideLand;
                Vector2 heading = _rideLand == _rideFrom ? Vector2.up : _rideLand - _rideFrom;

                // The landing's collider must exist for the snap to the ground.
                _view.WarmUp(end);
                CityModeController.PlaceAt(_player.gameObject, end, heading);
                _view.ResetTracking(end);
            }
        }

        // ------------------------------------------------------------------
        // The fire that chooses
        // ------------------------------------------------------------------

        private void TickFire(Vector2 local)
        {
            if (_stats != null)
            {
                if (_lastHealth >= 0f && _stats.CurrentHealth < _lastHealth - 0.01f)
                {
                    _lastHurtAt = Time.time;
                }

                _lastHealth = _stats.CurrentHealth;
            }

            float distance = RioFireRules.DistanceToFire(OverworldLayouts.LagoDeFogoPools, local);
            bool onShore = distance <= RioFireRules.ShoreBand;
            ShowFireGlow(onShore);
            if (distance > FireCheckRange)
            {
                _saidFire = false;
                _saidFireWaits = false;
                _saidBurn = false;
                return;
            }

            _fireTick += Time.deltaTime;
            if (_fireTick < 1f)
            {
                return;
            }

            float seconds = _fireTick;
            _fireTick = 0f;
            BurnBeasts();
            if (onShore && _stats != null && _stats.IsAlive)
            {
                MendHero(seconds);
            }
        }

        private void MendHero(float seconds)
        {
            float amount = RioFireRules.HealAmount(_stats.CurrentHealth, _stats.MaxHealth, Time.time - _lastHurtAt, seconds);
            if (amount > 0f)
            {
                _stats.Heal(amount);
                _lastHealth = _stats.CurrentHealth;
                if (!_saidFire)
                {
                    _saidFire = true;
                    Say(RioMechanicsTexts.FireHeals);
                }
            }
            else if (!_saidFireWaits && _stats.CurrentHealth < _stats.MaxHealth)
            {
                _saidFireWaits = true;
                Say(RioMechanicsTexts.FireWaits);
            }
        }

        private void BurnBeasts()
        {
            bool burned = false;
            foreach (EnemyController enemy in EnemyController.EnabledEnemies)
            {
                if (enemy == null || enemy.Health == null || !enemy.Health.IsAlive)
                {
                    continue;
                }

                Vector3 p = enemy.transform.position;
                Vector2 enemyLocal = new Vector2(p.x, p.z) - _origin;
                if (!OverworldLayouts.RioBounds.Contains(enemyLocal) || !RioFireRules.OnShore(OverworldLayouts.LagoDeFogoPools, enemyLocal))
                {
                    continue;
                }

                float burn = RioFireRules.BurnAmount(enemy.Health.CurrentHealth, enemy.Health.MaxHealth, enemy.IsBoss);
                if (burn > 0f)
                {
                    enemy.Health.ApplyDamage(burn, ElementType.Fire, null);
                    burned = true;
                }
            }

            if (burned && !_saidBurn)
            {
                _saidBurn = true;
                Say(RioMechanicsTexts.FireBurns);
            }
        }

        // ------------------------------------------------------------------
        // Defending the stranded barque (76)
        // ------------------------------------------------------------------

        private int Today() => ServiceLocator.TryGet(out TimeOfDayManager clock) ? clock.Day : 1;

        private bool BarqueFreedToday()
        {
            int marked = ServiceLocator.TryGet(out WorldMapProgress progress) ? progress.GetDailyMark(FreedMarkKey) : _freedDay;
            return RioBarqueDefenseRules.FreedToday(marked, Today());
        }

        private void TickDefense(Vector2 local)
        {
            Vector2 hull = OverworldLayouts.StrandedBarque;
            float distance = Vector2.Distance(local, hull);
            bool freed = BarqueFreedToday();
            ShowHull(distance < DefenseViewRange, freed);
            if (_defense.Running)
            {
                RunDefense(distance);
                return;
            }

            if (freed)
            {
                TickSails(local);
                return;
            }

            if (distance > RioBarqueDefenseRules.HullReach)
            {
                _hintHull = false;
                return;
            }

            if (_stats != null && !_stats.IsAlive)
            {
                return;
            }

            RioDefenseStart check = RioBarqueDefenseRules.CheckStart(false, Time.time - _lastDefenseFail, ZoneOpen(hull));
            if (!_hintHull || Time.unscaledTime >= _nextHint)
            {
                _hintHull = true;
                _nextHint = Time.unscaledTime + HintCooldown;
                SayDefenseStart(check);
            }

            InteractPromptRegistry.Ping(Time.unscaledTime);
            if (!_controller.Input.InteractPressedThisFrame)
            {
                return;
            }

            if (check == RioDefenseStart.Ok)
            {
                _defense.Begin();
                _nextDefenseStatus = Time.unscaledTime + DefenseStatusSeconds;
                Say(RioMechanicsTexts.DefenseBegins);
            }
            else if (Time.unscaledTime >= _nextDenied)
            {
                _nextDenied = Time.unscaledTime + DeniedCooldown;
                SayDefenseStart(check);
            }
        }

        private void SayDefenseStart(RioDefenseStart check)
        {
            switch (check)
            {
                case RioDefenseStart.Ok:
                    Say(RioMechanicsTexts.DefensePrompt);
                    break;
                case RioDefenseStart.ZoneBlocked:
                    Say(RioMechanicsTexts.BankBlocked);
                    break;
                default:
                    ServiceLocator.TryGet(out LocalizationManager localization);
                    float wait = RioBarqueDefenseRules.RetrySeconds - (Time.time - _lastDefenseFail);
                    SayText(RioMechanicsTexts.Format(localization, RioMechanicsTexts.DefenseResting, Mathf.Max(1, Mathf.CeilToInt(wait))));
                    break;
            }
        }

        private void RunDefense(float distance)
        {
            if (_stats != null && !_stats.IsAlive)
            {
                _defense.Cancel();
                _lastDefenseFail = Time.time;
                return;
            }

            CountGnawers(out int beasts, out int bosses);
            RioDefenseEvent happened = _defense.Tick(Time.deltaTime, distance, RioBarqueDefenseRules.GnawRate(beasts, bosses));
            if ((happened & RioDefenseEvent.PullWarning) != 0)
            {
                Say(RioMechanicsTexts.PullWarning);
            }

            if ((happened & RioDefenseEvent.Pull) != 0)
            {
                PullLands();
            }

            if ((happened & RioDefenseEvent.Oars) != 0)
            {
                StrikeBeasts(RioBarqueDefenseRules.OarRadius, false);
            }

            if ((happened & RioDefenseEvent.Freed) != 0)
            {
                FreeBarque();
                return;
            }

            if ((happened & (RioDefenseEvent.Sunk | RioDefenseEvent.Abandoned)) != 0)
            {
                _lastDefenseFail = Time.time;
                Say((happened & RioDefenseEvent.Sunk) != 0 ? RioMechanicsTexts.DefenseSunk : RioMechanicsTexts.DefenseAbandoned);
                return;
            }

            if (Time.unscaledTime < _nextDefenseStatus)
            {
                return;
            }

            _nextDefenseStatus = Time.unscaledTime + DefenseStatusSeconds;
            ServiceLocator.TryGet(out LocalizationManager localization);
            string key = (happened & RioDefenseEvent.HeroAway) != 0 ? RioMechanicsTexts.DefenseComeBack : RioMechanicsTexts.DefenseStatus;
            SayText(RioMechanicsTexts.Format(localization, key, Mathf.RoundToInt(_defense.Progress * 100f), Mathf.CeilToInt(_defense.HullFraction * 100f)));
        }

        /// <summary>Apep drinks the river under the hull: the hull pays, or the hero standing on the bow does (a hazard, never past the floor).</summary>
        private void PullLands()
        {
            if (!_defense.LastPullOnHero || _stats == null)
            {
                Say(RioMechanicsTexts.PullHull);
                return;
            }

            float bite = RioBarqueDefenseRules.PullOnHero(_stats.CurrentHealth, _stats.MaxHealth);
            if (bite > 0f)
            {
                _stats.ApplyHazardDamage(bite);
            }

            Say(RioMechanicsTexts.PullHero);
        }

        private void CountGnawers(out int beasts, out int bosses)
        {
            beasts = 0;
            bosses = 0;
            float radiusSq = RioBarqueDefenseRules.GnawRadius * RioBarqueDefenseRules.GnawRadius;
            Vector2 hull = _origin + OverworldLayouts.StrandedBarque;
            foreach (EnemyController enemy in EnemyController.EnabledEnemies)
            {
                if (enemy == null || enemy.Health == null || !enemy.Health.IsAlive)
                {
                    continue;
                }

                Vector3 p = enemy.transform.position;
                if ((new Vector2(p.x, p.z) - hull).sqrMagnitude > radiusSq)
                {
                    continue;
                }

                if (enemy.IsBoss)
                {
                    bosses++;
                }
                else
                {
                    beasts++;
                }
            }
        }

        /// <summary>The rowers' oars (<paramref name="burst"/> false) or the light of the freed barque (true) on the beasts by the hull.</summary>
        private void StrikeBeasts(float radius, bool burst)
        {
            float radiusSq = radius * radius;
            Vector2 hull = _origin + OverworldLayouts.StrandedBarque;
            foreach (EnemyController enemy in EnemyController.EnabledEnemies)
            {
                if (enemy == null || enemy.Health == null || !enemy.Health.IsAlive)
                {
                    continue;
                }

                Vector3 p = enemy.transform.position;
                if ((new Vector2(p.x, p.z) - hull).sqrMagnitude > radiusSq)
                {
                    continue;
                }

                float amount = burst
                    ? RioBarqueDefenseRules.BurstAmount(enemy.Health.MaxHealth, enemy.IsBoss)
                    : RioBarqueDefenseRules.OarAmount(enemy.Health.MaxHealth, enemy.IsBoss);
                if (amount > 0f)
                {
                    enemy.Health.ApplyDamage(amount, burst ? ElementType.Holy : ElementType.Physical, burst ? _player.gameObject : null);
                }
            }
        }

        private void FreeBarque()
        {
            int today = Today();
            _freedDay = today;
            if (ServiceLocator.TryGet(out WorldMapProgress progress))
            {
                progress.SetDailyMark(FreedMarkKey, today);
            }

            if (_stats != null && _stats.IsAlive)
            {
                _stats.Heal(_stats.MaxHealth * RioBarqueDefenseRules.FreedHealFraction);
            }

            StrikeBeasts(RioBarqueDefenseRules.BurstRadius, true);
            Say(RioMechanicsTexts.DefenseFreed);
        }

        /// <summary>The freed barque's sails along the dry river (the rest of the in-game day): beside the hull, and back from each far end.</summary>
        private void TickSails(Vector2 local)
        {
            SkyRouteRules.StationsAt(OverworldLayouts.FreedBarqueSails, local, _stations);
            if (_stations.Count == 0)
            {
                _hintSail = null;
                return;
            }

            (SkyRoute route, bool toB) = _stations[0];
            bool zones = SkyRouteRules.AllZonesOpen(_layout, _origin, route, HeroLevel(), HeroRequirements().hero, HeroRequirements().has);
            if (_hintSail != route || Time.unscaledTime >= _nextHint)
            {
                _hintSail = route;
                _nextHint = Time.unscaledTime + HintCooldown;
                Say(zones ? (toB ? RioMechanicsTexts.SailOut : RioMechanicsTexts.SailBack) : RioMechanicsTexts.BankBlocked);
            }

            InteractPromptRegistry.Ping(Time.unscaledTime);
            if (!_controller.Input.InteractPressedThisFrame)
            {
                return;
            }

            if (!zones)
            {
                if (Time.unscaledTime >= _nextDenied)
                {
                    _nextDenied = Time.unscaledTime + DeniedCooldown;
                    Say(RioMechanicsTexts.BankBlocked);
                }

                return;
            }

            MountController.Active?.Dismount(startRecall: false, leave: false);
            Vector2 from = SkyRouteRules.PositionAt(route, toB, 0f);
            Vector2 to = SkyRouteRules.PositionAt(route, toB, 1f);
            Say(RioMechanicsTexts.Sailing);
            BeginRide(from, to, to, RioBarqueDefenseRules.SailArc, RioBarqueDefenseRules.SailSeconds(Vector2.Distance(from, to)), true);
        }

        private void ShowHull(bool near, bool freed)
        {
            if (!near)
            {
                HideHull();
                return;
            }

            if (_hullLine == null)
            {
                _hullLine = NewLine("Rio_StrandedBarque", 2.6f);
                _hullBar = NewLine("Rio_BarqueHullBar", 0.5f);
                _pullRing = NewLine("Rio_ApepPullRing", 0.25f);
                _pullRing.positionCount = PullRingSegments + 1;
            }

            Vector3 foot = Ground(OverworldLayouts.StrandedBarque);
            float lift = freed ? 0.9f + Mathf.Sin(Time.time * 0.8f) * 0.1f : 0.4f;
            Vector3 stem = foot + new Vector3(-7f, lift, 0f);
            _hullLine.enabled = true;
            _hullLine.startColor = _hullLine.endColor = freed ? new Color(1f, 0.85f, 0.4f, 1f) : new Color(0.83f, 0.69f, 0.22f, 1f);
            _hullLine.SetPosition(0, stem);
            _hullLine.SetPosition(1, stem + Vector3.right * 14f);

            bool running = _defense.Running;
            _hullBar.enabled = running;
            if (running)
            {
                // The bar over the hull: its length is the hull left, its colour runs from gold to red.
                float hull = _defense.HullFraction;
                Vector3 left = foot + new Vector3(-6f, 4.5f, 0f);
                _hullBar.startColor = _hullBar.endColor = Color.Lerp(new Color(1f, 0.25f, 0.2f, 1f), new Color(1f, 0.82f, 0.3f, 1f), hull);
                _hullBar.SetPosition(0, left);
                _hullBar.SetPosition(1, left + Vector3.right * Mathf.Max(0.1f, 12f * hull));
            }

            bool pulling = running && _defense.PullImminent;
            _pullRing.enabled = pulling;
            if (pulling)
            {
                _pullRing.startColor = _pullRing.endColor = new Color(0.55f, 0.3f, 0.7f, 1f);
                for (int i = 0; i <= PullRingSegments; i++)
                {
                    float angle = i * 2f * Mathf.PI / PullRingSegments;
                    _pullRing.SetPosition(i, foot + new Vector3(Mathf.Cos(angle) * RioBarqueDefenseRules.BowReach, 0.2f, Mathf.Sin(angle) * RioBarqueDefenseRules.BowReach));
                }
            }
        }

        private void HideHull()
        {
            foreach (LineRenderer line in new[] { _hullLine, _hullBar, _pullRing })
            {
                if (line != null)
                {
                    line.enabled = false;
                }
            }
        }

        // ------------------------------------------------------------------
        // Feedback
        // ------------------------------------------------------------------

        private void ShowPiers(Vector2 local, int hour)
        {
            bool near = Mathf.Abs(local.x) < RiverViewRange && Mathf.Abs(local.y - 200f) < RiverViewRange * 0.5f;
            if (near && _pierBeacons.Count == 0)
            {
                foreach (Vector2 pier in OverworldLayouts.HourPiers)
                {
                    LineRenderer beacon = NewLine("Rio_PierBeacon", 0.4f);
                    beacon.SetPosition(0, Ground(pier));
                    _pierBeacons.Add(beacon);
                }

                _barque = NewLine("Rio_Barque", 2.4f);
            }

            for (int i = 0; i < _pierBeacons.Count; i++)
            {
                LineRenderer beacon = _pierBeacons[i];
                beacon.enabled = near;
                bool open = i == hour;
                beacon.startColor = beacon.endColor = open ? new Color(1f, 0.82f, 0.3f, 1f) : new Color(0.35f, 0.45f, 0.75f, 0.8f);
                beacon.SetPosition(1, beacon.GetPosition(0) + Vector3.up * (open ? BeaconOpenHeight : BeaconClosedHeight));
            }

            if (_barque != null)
            {
                _barque.enabled = near;
                if (near)
                {
                    Vector2 pier = OverworldLayouts.HourPiers[hour];
                    float bob = Mathf.Sin(Time.time * 0.8f) * 0.08f;
                    Vector3 stem = new Vector3(_origin.x + pier.x - 6f, 0.5f + bob, _origin.y + pier.y + 11f);
                    _barque.startColor = _barque.endColor = new Color(0.83f, 0.69f, 0.22f, 1f);
                    _barque.SetPosition(0, stem);
                    _barque.SetPosition(1, stem + Vector3.right * 12f);
                }
            }

            ShowBerths(local);
        }

        private void ShowBerths(Vector2 local)
        {
            Vector2[] berths = OverworldLayouts.BarqueBerths;
            if (_berthBeacons.Count == 0)
            {
                foreach (Vector2 berth in berths)
                {
                    LineRenderer beacon = NewLine("Rio_BerthBeacon", 0.3f);
                    Vector3 foot = Ground(berth);
                    beacon.startColor = beacon.endColor = new Color(0.75f, 0.85f, 1f, 1f);
                    beacon.SetPosition(0, foot);
                    beacon.SetPosition(1, foot + Vector3.up * 8f);
                    _berthBeacons.Add(beacon);
                }
            }

            for (int i = 0; i < berths.Length; i++)
            {
                _berthBeacons[i].enabled = (berths[i] - local).sqrMagnitude < BerthViewRange * BerthViewRange;
            }
        }

        private void ShowRing(Vector2 local, double now)
        {
            bool near = (local - OverworldLayouts.MehenCenter).sqrMagnitude < RingViewRange * RingViewRange;
            if (near && _ringHalves[0] == null)
            {
                _ringY = Mathf.Max(0f, OverworldRules.SurfaceAt(_layout, _origin + OverworldLayouts.MehenCenter)) + RingLift;
                for (int i = 0; i < _ringHalves.Length; i++)
                {
                    _ringHalves[i] = NewLine("Rio_MehenRing", 0.8f);
                    _ringHalves[i].positionCount = RingHalfSegments + 1;
                }
            }

            for (int half = 0; half < _ringHalves.Length; half++)
            {
                LineRenderer line = _ringHalves[half];
                if (line == null)
                {
                    continue;
                }

                line.enabled = near;
                if (!near)
                {
                    continue;
                }

                line.startColor = line.endColor = RioMehenRules.IsHolding(now) ? new Color(1f, 0.82f, 0.3f, 1f) : new Color(0.37f, 0.56f, 0.35f, 1f);
                float radius = (OverworldLayouts.MehenGateInner + OverworldLayouts.MehenGateOuter) * 0.5f;
                float from = RioMehenRules.OpeningBearing(now) + half * 180f + RingOpeningDegrees * 0.5f;
                float sweep = (180f - RingOpeningDegrees) / RingHalfSegments;
                for (int i = 0; i <= RingHalfSegments; i++)
                {
                    float angle = (from + i * sweep) * Mathf.Deg2Rad;
                    Vector2 point = _origin + OverworldLayouts.MehenCenter + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
                    line.SetPosition(i, new Vector3(point.x, _ringY, point.y));
                }
            }
        }

        private void ShowFireGlow(bool show)
        {
            if (!show)
            {
                if (_fireGlow != null)
                {
                    _fireGlow.enabled = false;
                }

                return;
            }

            _fireGlow = _fireGlow != null ? _fireGlow : NewLine("Rio_FireGlow", 0.25f);
            _fireGlow.enabled = true;
            _fireGlow.startColor = _fireGlow.endColor = new Color(1f, 0.6f, 0.24f, 1f);
            _fireGlow.positionCount = 17;
            for (int i = 0; i < 17; i++)
            {
                float angle = i * Mathf.PI / 8f + Time.time;
                _fireGlow.SetPosition(i, _player.position + new Vector3(Mathf.Cos(angle) * 1.1f, 0.15f, Mathf.Sin(angle) * 1.1f));
            }
        }

        private Vector3 Ground(Vector2 local)
        {
            Vector2 world = _origin + local;
            return new Vector3(world.x, Mathf.Max(0f, OverworldRules.SurfaceAt(_layout, world)), world.y);
        }

        private void HideFeedback()
        {
            foreach (LineRenderer beacon in _pierBeacons)
            {
                if (beacon != null)
                {
                    beacon.enabled = false;
                }
            }

            foreach (LineRenderer beacon in _berthBeacons)
            {
                if (beacon != null)
                {
                    beacon.enabled = false;
                }
            }

            foreach (LineRenderer half in _ringHalves)
            {
                if (half != null)
                {
                    half.enabled = false;
                }
            }

            if (_barque != null)
            {
                _barque.enabled = false;
            }

            ShowFireGlow(false);
            HideHull();
            if (_defense.Running)
            {
                // Leaving the bank drops the oars, as walking away would.
                _defense.Cancel();
                _lastDefenseFail = Time.time;
            }

            _hintHull = false;
            _hintSail = null;
            _shownHour = -1;
            _shownHourSpoken = false;
            _lastHealth = -1f;
        }

        private LineRenderer NewLine(string objectName, float width)
        {
            if (_material == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit") ?? Shader.Find("Particles/Standard Unlit") ?? Shader.Find("Sprites/Default");
                _material = new Material(shader) { name = "Mat_RioLine_Runtime" };
            }

            var go = new GameObject(objectName);
            go.transform.SetParent(transform, false);
            var line = go.AddComponent<LineRenderer>();
            line.sharedMaterial = _material;
            line.useWorldSpace = true;
            line.positionCount = 2;
            line.startWidth = line.endWidth = width;
            line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            line.receiveShadows = false;
            line.enabled = false;
            return line;
        }

        private void Say(string key)
        {
            ServiceLocator.TryGet(out LocalizationManager localization);
            SayText(RioMechanicsTexts.Get(localization, key));
        }

        private void SayText(string text) => CombatFxManager.SpawnLootWarningText(_player.position + Vector3.up * FeedbackHeight, text);
    }
}
