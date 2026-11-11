using System;
using Rapadura.Core.DI;
using Rapadura.Core.Events;
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
using Rapadura.UI.Common;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>The hero sat down by Grandpa Jorge's chair on the Varanda (the last step of the main story ends here).</summary>
    public readonly struct RockingChairSatEvent : IGameEvent
    {
    }

    /// <summary>
    /// Scene side of the Reinos do Céu mechanics (the rules are <see cref="CeuMoonBarqueRules"/>, <see cref="CeuSkyHoldRules"/> and <see cref="CeuPorchRules"/>): added to the
    /// overworld root, idle unless the layout holds the region and the hero stands in it. The Barca da Lua takes the controls for 2.5 s at most, handing them back on
    /// arrival, on death or if anything else moves the hero; holding the sky and sitting on the porch never take them at all.
    /// When each crack reopens is saved per slot in <see cref="WorldMapProgress"/>, so reloading cannot unseal one and heal again. The porch keeps nothing: the story comes from the UTC clock and
    /// sitting is a rest with no daily limit or prize, so there is no state to save.
    /// </summary>
    public sealed class CeuMechanicsDriver : MonoBehaviour, IRegionMechanics
    {
        private const string PlayerTag = "Player";
        private const float HintCooldown = 3f;
        private const float DeniedCooldown = 1.5f;
        private const float ExternalMoveTolerance = 3f;
        private const float FeedbackHeight = 2.4f;
        private const float BarqueViewRange = 200f;
        private const float CrackViewRange = 260f;
        private const float ChairViewRange = 60f;
        private const float BeaconLow = 5f;
        private const float BeaconHigh = 24f;
        private const float CrackBeamHeight = 26f;
        private const int RingSegments = 32;

        private static readonly Color Moon = new Color(0.78f, 0.71f, 0.93f, 1f);
        private static readonly Color Gold = new Color(0.91f, 0.76f, 0.35f, 1f);
        private static readonly Color Shadow = new Color(0.2f, 0.12f, 0.35f, 1f);
        private static readonly Color Warning = new Color(1f, 0.28f, 0.2f, 1f);

        private static readonly string[] CrackMarkKeys = { "ceu.crack.0", "ceu.crack.1", "ceu.crack.2", "ceu.crack.3", "ceu.crack.4" };

        private readonly CeuSkyHoldSession _hold = new CeuSkyHoldSession();
        private readonly CeuPorchSession _porch = new CeuPorchSession();
        private readonly LineRenderer[] _crackBeams = new LineRenderer[CeuSkyHoldRules.Cracks];
        private readonly HeroRequirementCache _heroState = new HeroRequirementCache();
        private readonly StaticGroundCache _ground = new StaticGroundCache();

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
        private LineRenderer _pierBeacon;
        private LineRenderer _barqueHull;
        private LineRenderer _berthBeacon;
        private LineRenderer _holdRing;
        private LineRenderer _porchRing;

        private bool _riding;
        private Vector2 _rideAt;
        private Vector2 _rideLand;
        private float _rideT;
        private float _rideY;
        private Vector3 _lastRidePosition;

        private float _nextHint;
        private float _nextDenied;
        private int _hintSpot = -1;
        private bool _announcedNight;
        private bool _wasNight;
        private bool _atChair;
        private float _lastHealth = -1f;

        private Rect[] _areas;

        /// <inheritdoc/>
        public Rect[] MechanicsAreas => _areas;

        /// <inheritdoc/>
        public bool MechanicsBusy => _riding;

        private void Start()
        {
            _view = GetComponent<OverworldView>();
            _layout = _view != null ? _view.Layout : null;
            OverworldZone anchor = _layout?.Zone(OverworldLayouts.CeuAnchorZoneId);
            if (anchor == null)
            {
                enabled = false;
                return;
            }

            _origin = anchor.Origin;
            _areas = new[] { MechanicsDriverHub.WorldArea(OverworldLayouts.CeuBounds, _origin) };
        }

        private void OnEnable() => _heroState.Attach();

        private void OnDisable()
        {
            EndRide(false);
            _hold.Cancel();
            _porch.Cancel();
            Quiet();
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
            if (!OverworldLayouts.CeuBounds.Contains(local))
            {
                Quiet();
                return;
            }

            if (_stats != null && !_stats.IsAlive)
            {
                _hold.Cancel();
                _porch.Cancel();
                return;
            }

            long unix = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            TimeOfDayManager clock = Clock();
            bool isNight = clock != null && TimeOfDayManager.GetPhase(clock.CurrentHour) == TimeOfDayPhase.Night;
            int spot = TickBarque(local, isNight, clock);
            spot = TickCracks(local, unix, spot);
            spot = TickPorch(local, unix, isNight, spot);
            if (spot < 0)
            {
                _hintSpot = -1;
            }
        }

        private static TimeOfDayManager Clock() => ServiceLocator.TryGet(out TimeOfDayManager time) ? time : null;

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

        private bool Interacted => _controller.Input.InteractPressedThisFrame;

        /// <summary>True the first time the hero reaches a spot, and again every <see cref="HintCooldown"/> while the hero stays.</summary>
        private bool HintDue(int spot)
        {
            if (_hintSpot == spot && Time.unscaledTime < _nextHint)
            {
                return false;
            }

            _hintSpot = spot;
            _nextHint = Time.unscaledTime + HintCooldown;
            return true;
        }

        // ------------------------------------------------------------------
        // The Barca da Lua
        // ------------------------------------------------------------------

        private const int SpotPier = 100;
        private const int SpotBerth = 101;
        private const int SpotChair = 102;

        private int TickBarque(Vector2 local, bool isNight, TimeOfDayManager clock)
        {
            ShowBarque(local, isNight);
            bool nearPier = (local - OverworldLayouts.MoonBarquePier).sqrMagnitude < BarqueViewRange * BarqueViewRange;
            if (isNight && !_wasNight && nearPier && !_announcedNight)
            {
                _announcedNight = true;
                Say(CeuMechanicsTexts.BarqueArrives);
            }

            if (!isNight)
            {
                _announcedNight = false;
            }

            _wasNight = isNight;
            if (CeuMoonBarqueRules.WithinReach(OverworldLayouts.MoonBarquePier, local))
            {
                TickPier(isNight, clock);
                return SpotPier;
            }

            if (!CeuMoonBarqueRules.WithinReach(OverworldLayouts.MoonBarqueBerth, local))
            {
                return -1;
            }

            // The way back is always served: a hero on the deck can go down at any hour.
            if (HintDue(SpotBerth))
            {
                Say(CeuMechanicsTexts.BarqueReturn);
            }

            if (Interacted)
            {
                BeginRide(local, OverworldLayouts.MoonBarquePier);
            }

            return SpotBerth;
        }

        private void TickPier(bool isNight, TimeOfDayManager clock)
        {
            CeuBarqueCheck check = CeuMoonBarqueRules.Check(isNight, ZoneOpen(OverworldLayouts.MoonBarqueBerth));
            if (HintDue(SpotPier))
            {
                SayPier(check, clock);
            }

            if (!Interacted)
            {
                return;
            }

            if (check == CeuBarqueCheck.Board)
            {
                Vector2 here = new Vector2(_player.position.x, _player.position.z) - _origin;
                BeginRide(here, OverworldLayouts.MoonBarqueBerth);
            }
            else if (Time.unscaledTime >= _nextDenied)
            {
                _nextDenied = Time.unscaledTime + DeniedCooldown;
                SayPier(check, clock);
            }
        }

        private void SayPier(CeuBarqueCheck check, TimeOfDayManager clock)
        {
            switch (check)
            {
                case CeuBarqueCheck.Board:
                    Say(CeuMechanicsTexts.BarqueBoard);
                    break;
                case CeuBarqueCheck.ZoneBlocked:
                    Say(CeuMechanicsTexts.BarqueBlocked);
                    break;
                default:
                    ServiceLocator.TryGet(out LocalizationManager localization);
                    float hour = clock != null ? clock.CurrentHour : 12f;
                    SayText(CeuMechanicsTexts.Format(localization, CeuMechanicsTexts.BarqueNotNight, CeuMoonBarqueRules.HoursUntilNight(hour, TimeOfDayManager.NightStartHour)));
                    break;
            }
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

        private void BeginRide(Vector2 here, Vector2 land)
        {
            MountController.Active?.Dismount(startRecall: false, leave: false);
            Say(CeuMechanicsTexts.BarqueBoarding);
            _riding = true;
            _rideAt = here;
            _rideLand = land;
            _rideT = 0f;
            _rideY = Mathf.Max(0f, OverworldRules.SurfaceAt(_layout, _origin + here));

            // Clears the motor's velocity so the Animator rests in idle while the controls are off.
            Vector3 facing = new Vector3(land.x - here.x, 0f, land.y - here.y);
            _motor.Teleport(_player.position, facing.sqrMagnitude > 0.01f ? Quaternion.LookRotation(facing) : _player.rotation);
            _controller.ClickToMove?.ClearDestination();
            _controller.enabled = false;
            _character.enabled = false;
            _lastRidePosition = _player.position;
            _hold.Cancel();
            _porch.Cancel();
        }

        private void TickRide()
        {
            bool dead = _stats != null && !_stats.IsAlive;
            if (dead || (_player.position - _lastRidePosition).sqrMagnitude > ExternalMoveTolerance * ExternalMoveTolerance)
            {
                EndRide(false);
                return;
            }

            _rideT = Mathf.Min(1f, _rideT + Time.deltaTime / CeuMoonBarqueRules.BoardingSeconds);
            float bob = Mathf.Sin(_rideT * Mathf.PI) * CeuMoonBarqueRules.BoardingArc;
            Vector2 ground = _origin + _rideAt;
            _player.position = new Vector3(ground.x, _rideY + bob, ground.y);
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

                // The landing's collider must exist for the snap to the ground.
                _view.WarmUp(end);
                CityModeController.PlaceAt(_player.gameObject, end, _rideLand - _rideAt);
                _view.ResetTracking(end);
            }
        }

        // ------------------------------------------------------------------
        // Holding the sky
        // ------------------------------------------------------------------

        private int TickCracks(Vector2 local, long unix, int spot)
        {
            bool nearLookout = (local - OverworldLayouts.MaoAbertaOffset).sqrMagnitude < CrackViewRange * CrackViewRange;
            ShowCracks(nearLookout, unix);
            int active = _hold.ActiveCrack;
            if (active >= 0)
            {
                bool inReach = (OverworldLayouts.SkyCracks[active] - local).sqrMagnitude <= CeuSkyHoldRules.Reach * CeuSkyHoldRules.Reach;
                HandleHold(_hold.Tick(Time.deltaTime, inReach, unix), active, unix);
            }

            int crack = CeuSkyHoldRules.NearestCrack(OverworldLayouts.SkyCracks, local);
            if (crack < 0)
            {
                return spot;
            }

            if (_hold.ActiveCrack < 0)
            {
                bool isSealed = IsCrackSealed(crack, unix);
                if (HintDue(crack))
                {
                    Say(isSealed ? CeuMechanicsTexts.CrackSealed : CeuMechanicsTexts.CrackRaise);
                }

                if (Interacted && !isSealed && _hold.TryRaise(crack, unix))
                {
                    Say(CeuMechanicsTexts.CrackHold);
                }
            }

            return crack;
        }

        private void HandleHold(CeuSkyEvent happened, int crack, long unix)
        {
            switch (happened)
            {
                case CeuSkyEvent.DripWarning:
                    Say(CeuMechanicsTexts.DripWarning);
                    break;

                case CeuSkyEvent.Drip:
                    if (_stats != null)
                    {
                        float amount = CeuSkyHoldRules.DripAmount(_stats.CurrentHealth, _stats.MaxHealth);
                        if (amount > 0f)
                        {
                            _stats.ApplyHazardDamage(amount);
                            Say(CeuMechanicsTexts.DripHit);
                        }
                    }

                    break;

                case CeuSkyEvent.Sealed:
                    SealCrack(crack, unix);
                    break;

                case CeuSkyEvent.Faded:
                    Say(CeuMechanicsTexts.PillarFaded);
                    break;
            }
        }

        /// <summary>True while a saved seal holds; one further off than a whole seal (the clock went back, or another machine's) counts as expired.</summary>
        public static bool CrackSealedIn(WorldMapProgress progress, int crack, long now)
        {
            long left = progress.GetTimeMark(CrackMarkKeys[crack]) - now;
            return left > 0L && left <= CeuSkyHoldRules.SealSeconds;
        }

        public static void StoreSeal(WorldMapProgress progress, int crack, long now) => progress?.SetTimeMark(CrackMarkKeys[crack], now + CeuSkyHoldRules.SealSeconds);

        private bool IsCrackSealed(int crack, long unix) => ServiceLocator.TryGet(out WorldMapProgress progress) ? CrackSealedIn(progress, crack, unix) : _hold.IsSealed(crack, unix);

        private int CracksSealed(long unix)
        {
            int count = 0;
            for (int i = 0; i < CeuSkyHoldRules.Cracks; i++)
            {
                count += IsCrackSealed(i, unix) ? 1 : 0;
            }

            return count;
        }

        private void SealCrack(int crack, long unix)
        {
            StoreSeal(ServiceLocator.TryGet(out WorldMapProgress progress) ? progress : null, crack, unix);
            if (_stats != null && _stats.IsAlive)
            {
                _stats.Heal(CeuSkyHoldRules.SealHeal(_stats.MaxHealth));
            }

            Vector2 center = OverworldLayouts.SkyCracks[crack];
            foreach (EnemyController enemy in EnemyController.EnabledEnemies)
            {
                if (enemy == null || enemy.Health == null || !enemy.Health.IsAlive)
                {
                    continue;
                }

                Vector3 p = enemy.transform.position;
                if ((new Vector2(p.x, p.z) - _origin - center).sqrMagnitude > CeuSkyHoldRules.SealBiteRadius * CeuSkyHoldRules.SealBiteRadius)
                {
                    continue;
                }

                float bite = CeuSkyHoldRules.SealBite(enemy.Health.MaxHealth, enemy.IsBoss);
                if (bite > 0f)
                {
                    enemy.Health.ApplyDamage(bite, ElementType.Holy, _player.gameObject);
                }
            }

            bool skyWhole = CracksSealed(unix) >= CeuSkyHoldRules.Cracks;
            if (skyWhole)
            {
                EventBus.Publish(new Rapadura.Gameplay.Achievements.RegionFeatEvent(Rapadura.Gameplay.Achievements.RegionFeats.CeuSkyWhole));
            }

            Say(skyWhole ? CeuMechanicsTexts.SkyWhole : CeuMechanicsTexts.SealLines[crack]);
        }

        private void ShowCracks(bool near, long unix)
        {
            for (int i = 0; i < _crackBeams.Length; i++)
            {
                if (near && _crackBeams[i] == null)
                {
                    _crackBeams[i] = NewLine("Ceu_CrackBeam", 0.7f);
                }

                LineRenderer beam = _crackBeams[i];
                if (beam == null)
                {
                    continue;
                }

                beam.enabled = near;
                if (!near)
                {
                    continue;
                }

                bool active = _hold.ActiveCrack == i;
                bool isSealed = IsCrackSealed(i, unix);
                Color color = active ? Gold : isSealed ? Color.white : Shadow;
                float height = active ? Mathf.Lerp(BeaconLow, CrackBeamHeight, _hold.Fraction) : isSealed ? CrackBeamHeight : BeaconHigh * 0.5f;
                Vector3 foot = Ground(OverworldLayouts.SkyCracks[i]);
                beam.startColor = beam.endColor = color;
                beam.SetPosition(0, foot);
                beam.SetPosition(1, foot + Vector3.up * height);
            }

            ShowHoldRing();
        }

        private void ShowHoldRing()
        {
            int active = _hold.ActiveCrack;
            if (active < 0)
            {
                if (_holdRing != null)
                {
                    _holdRing.enabled = false;
                }

                return;
            }

            _holdRing = _holdRing != null ? _holdRing : NewRing("Ceu_HoldRing", 0.25f);
            _holdRing.enabled = true;
            _holdRing.startColor = _holdRing.endColor = _hold.DripImminent ? Warning : Gold;
            DrawRing(_holdRing, OverworldLayouts.SkyCracks[active], CeuSkyHoldRules.Reach);
        }

        // ------------------------------------------------------------------
        // The porch
        // ------------------------------------------------------------------

        private int TickPorch(Vector2 local, long unix, bool isNight, int spot)
        {
            Vector2 chair = OverworldLayouts.RockingChairPosition;
            ShowPorch((local - chair).sqrMagnitude < ChairViewRange * ChairViewRange, isNight);
            if (_porch.IsSitting)
            {
                TickSitting(local, chair);
                return SpotChair;
            }

            if (!CeuPorchRules.WithinSeat(chair, local))
            {
                _atChair = false;
                return spot;
            }

            // The touch controls have no interact button: there the hero sits on walking up to the chair, once per arrival.
            bool sitsByTouch = !_atChair && TouchControlsPreferences.HasTouchscreen;
            _atChair = true;
            if (HintDue(SpotChair))
            {
                Say(CeuMechanicsTexts.PorchSit);
            }

            if (Interacted || sitsByTouch)
            {
                _porch.Begin(CeuPorchRules.StoryIndex(unix), isNight);
                _lastHealth = _stats != null ? _stats.CurrentHealth : -1f;
                SayLine();
                EventBus.Publish(new RockingChairSatEvent());
            }

            return SpotChair;
        }

        private void TickSitting(Vector2 local, Vector2 chair)
        {
            bool hurt = _stats != null && _lastHealth >= 0f && _stats.CurrentHealth < _lastHealth - 0.01f;
            switch (_porch.Tick(Time.deltaTime, CeuPorchRules.WithinStay(chair, local), hurt))
            {
                case CeuPorchEvent.NextLine:
                    SayLine();
                    break;
                case CeuPorchEvent.Finished:
                    EventBus.Publish(new Rapadura.Gameplay.Achievements.RegionFeatEvent(Rapadura.Gameplay.Achievements.RegionFeats.CeuPorchStory));
                    Say(CeuMechanicsTexts.PorchEnd);
                    break;
                case CeuPorchEvent.Interrupted:
                    Say(CeuMechanicsTexts.PorchBroken);
                    break;
            }

            if (_stats != null)
            {
                if (_porch.IsSitting)
                {
                    float rest = CeuPorchRules.RestAmount(_stats.CurrentHealth, _stats.MaxHealth, Time.deltaTime);
                    if (rest > 0f)
                    {
                        _stats.Heal(rest);
                    }
                }

                _lastHealth = _stats.CurrentHealth;
            }
        }

        private void SayLine()
        {
            Say(_porch.Line < CeuPorchRules.LinesPerStory ? CeuMechanicsTexts.StoryLine(_porch.Story, _porch.Line) : CeuMechanicsTexts.PorchNightCoda);
        }

        private void ShowPorch(bool near, bool isNight)
        {
            if (!near)
            {
                if (_porchRing != null)
                {
                    _porchRing.enabled = false;
                }

                return;
            }

            _porchRing = _porchRing != null ? _porchRing : NewRing("Ceu_PorchRing", 0.12f);
            _porchRing.enabled = true;
            Color color = isNight ? Moon : Gold;
            color.a = _porch.IsSitting ? 1f : 0.5f;
            _porchRing.startColor = _porchRing.endColor = color;
            DrawRing(_porchRing, OverworldLayouts.RockingChairPosition, CeuPorchRules.SeatReach);
        }

        // ------------------------------------------------------------------
        // Feedback
        // ------------------------------------------------------------------

        private void ShowBarque(Vector2 local, bool isNight)
        {
            bool nearPier = (local - OverworldLayouts.MoonBarquePier).sqrMagnitude < BarqueViewRange * BarqueViewRange;
            bool nearBerth = (local - OverworldLayouts.MoonBarqueBerth).sqrMagnitude < BarqueViewRange * BarqueViewRange;
            if (nearPier && _pierBeacon == null)
            {
                _pierBeacon = NewLine("Ceu_MoonPierBeacon", 0.4f);
                _barqueHull = NewLine("Ceu_MoonBarque", 2.4f);
            }

            if (nearBerth && _berthBeacon == null)
            {
                _berthBeacon = NewLine("Ceu_MoonBerthBeacon", 0.3f);
                _berthBeacon.startColor = _berthBeacon.endColor = Moon;
                Vector3 foot = Ground(OverworldLayouts.MoonBarqueBerth);
                _berthBeacon.SetPosition(0, foot);
                _berthBeacon.SetPosition(1, foot + Vector3.up * BeaconLow);
            }

            if (_berthBeacon != null)
            {
                _berthBeacon.enabled = nearBerth;
            }

            if (_pierBeacon == null)
            {
                return;
            }

            Vector3 pier = Ground(OverworldLayouts.MoonBarquePier);
            _pierBeacon.enabled = nearPier;
            _pierBeacon.startColor = _pierBeacon.endColor = isNight ? Moon : Shadow;
            _pierBeacon.SetPosition(0, pier);
            _pierBeacon.SetPosition(1, pier + Vector3.up * (isNight ? BeaconHigh : BeaconLow));
            _barqueHull.enabled = nearPier && isNight;
            if (_barqueHull.enabled)
            {
                float bob = Mathf.Sin(Time.time * 0.8f) * 0.1f;
                Vector3 stem = pier + new Vector3(-7f, 0.7f + bob, 8f);
                _barqueHull.startColor = _barqueHull.endColor = Moon;
                _barqueHull.SetPosition(0, stem);
                _barqueHull.SetPosition(1, stem + Vector3.right * 14f);
            }
        }

        /// <summary>The ground under a fixed point of the region (cracks, pier, berth, ring centres), sampled once per point.</summary>
        private Vector3 Ground(Vector2 local)
        {
            Vector2 world = _origin + local;
            return new Vector3(world.x, _ground.GroundAt(_layout, world), world.y);
        }

        private void DrawRing(LineRenderer ring, Vector2 center, float radius)
        {
            float y = Ground(center).y + 0.2f;
            for (int i = 0; i <= RingSegments; i++)
            {
                float angle = i * 2f * Mathf.PI / RingSegments;
                Vector2 point = _origin + center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
                ring.SetPosition(i, new Vector3(point.x, y, point.y));
            }
        }

        /// <summary>The hero left the region: everything goes out, and whatever was under way ends.</summary>
        private void Quiet()
        {
            _hold.Cancel();
            _porch.Cancel();
            _hintSpot = -1;
            _atChair = false;
            _announcedNight = false;
            _wasNight = false;
            Hide(_pierBeacon);
            Hide(_barqueHull);
            Hide(_berthBeacon);
            Hide(_holdRing);
            Hide(_porchRing);
            foreach (LineRenderer beam in _crackBeams)
            {
                Hide(beam);
            }
        }

        private static void Hide(LineRenderer line)
        {
            if (line != null)
            {
                line.enabled = false;
            }
        }

        private LineRenderer NewRing(string objectName, float width)
        {
            LineRenderer ring = NewLine(objectName, width);
            ring.positionCount = RingSegments + 1;
            return ring;
        }

        private LineRenderer NewLine(string objectName, float width)
        {
            if (_material == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit") ?? Shader.Find("Particles/Standard Unlit") ?? Shader.Find("Sprites/Default");
                _material = new Material(shader) { name = "Mat_CeuLine_Runtime" };
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
            SayText(CeuMechanicsTexts.Get(localization, key));
        }

        private void SayText(string text) => CombatFxManager.SpawnLootWarningText(_player.position + Vector3.up * FeedbackHeight, text);
    }
}
