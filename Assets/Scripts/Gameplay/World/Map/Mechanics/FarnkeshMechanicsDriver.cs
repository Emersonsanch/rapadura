using System.Collections.Generic;
using Rapadura.Core.DI;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Combat;
using Rapadura.Gameplay.Environment;
using Rapadura.Gameplay.Inventory;
using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.World.Map.Areas;
using Rapadura.Gameplay.World.Cities.Town;
using Rapadura.Gameplay.World.Map.Overworld;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// Scene side of the Picos de Farnkesh mechanics (the rules are <see cref="WindGustRules"/> and <see cref="SkyRouteRules"/>):
    /// added to the overworld root, idle unless the layout holds the region and the hero stands in it. Pushes the walking hero
    /// in a gust through the CharacterController (so walls still hold), and carries them along rope baskets and wind currents
    /// with the player's own controls off, giving them back on arrival or if anything else moves the hero meanwhile (respawn, barca).
    /// </summary>
    public sealed class FarnkeshMechanicsDriver : MonoBehaviour, IRegionMechanics
    {
        private const string PlayerTag = "Player";
        private const int StreakCount = 8;
        private const float HintCooldown = 3f;
        private const float DeniedCooldown = 1.5f;
        private const float ExternalMoveTolerance = 3f;
        private const float FeedbackHeight = 2.4f;

        private readonly List<(SkyRoute Route, bool ToB)> _stations = new List<(SkyRoute, bool)>();
        private readonly LineRenderer[] _streaks = new LineRenderer[StreakCount];
        private readonly HeroRequirementCache _heroState = new HeroRequirementCache();

        private OverworldView _view;
        private OverworldLayout _layout;
        private Vector2 _origin;
        private Transform _player;
        private PlayerController _controller;
        private PlayerMotor _motor;
        private CharacterController _character;
        private PlayerStats _stats;
        private InventoryManager _bag;
        private LineRenderer _rope;
        private Material _material;

        private SkyRoute _ride;
        private bool _rideToB;
        private float _rideT;
        private float _rideSeconds;
        private float _rideFromY;
        private float _rideToY;
        private Vector3 _lastRidePosition;

        private int _warnedGust = -1;
        private int _hitGust = -1;
        private float _nextHint;
        private float _nextDenied;
        private SkyRoute _hintRoute;
        private SkyRoute _ropeRoute;

        private Rect[] _areas;

        /// <inheritdoc/>
        public Rect[] MechanicsAreas => _areas;

        /// <inheritdoc/>
        public bool MechanicsBusy => _ride != null;

        private void Start()
        {
            _view = GetComponent<OverworldView>();
            _layout = _view != null ? _view.Layout : null;
            OverworldZone anchor = _layout?.Zone(OverworldLayouts.FarnkeshAnchorZoneId);
            if (anchor == null)
            {
                enabled = false;
                return;
            }

            _origin = anchor.Origin;
            _areas = new[] { MechanicsDriverHub.WorldArea(OverworldLayouts.FarnkeshBounds, _origin) };
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

            if (_ride != null)
            {
                TickRide();
                return;
            }

            Vector2 local = new Vector2(_player.position.x, _player.position.z) - _origin;
            if (!OverworldLayouts.FarnkeshBounds.Contains(local))
            {
                HideFeedback();
                return;
            }

            TickGusts(local);
            TickStations(local);
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
                _stats = go.GetComponent<PlayerStats>();
                _bag = go.GetComponentInChildren<InventoryManager>();
            }

            return _controller != null && _motor != null && _character != null;
        }

        // ------------------------------------------------------------------
        // Gusts
        // ------------------------------------------------------------------

        private void TickGusts(Vector2 local)
        {
            float time = Time.time;
            WindGustPhase phase = WindGustRules.PhaseAt(time);
            Vector2 axis = Vector2.zero;
            bool inWind = phase != WindGustPhase.Calm && OverworldRules.SettlementAt(_layout, local + _origin) == null
                && WindGustRules.TryGetAxis(local, OverworldLayouts.WindBridgeLines, OverworldLayouts.GustAreas, OverworldLayouts.GorgeWindAxis, out axis);
            if (!inWind)
            {
                SetStreaks(false, Vector2.zero, false);
                return;
            }

            int gust = WindGustRules.GustIndex(time);
            Vector2 direction = WindGustRules.Direction(gust, axis);
            SetStreaks(true, direction, phase == WindGustPhase.Blowing);
            if (phase == WindGustPhase.Warning)
            {
                if (_warnedGust != gust)
                {
                    _warnedGust = gust;
                    Say(FarnkeshMechanicsTexts.GustWarning);
                }

                return;
            }

            MapRequirementState state = _heroState.Get(_bag);
            Vector2 push = WindGustRules.PushVelocity(direction, !_controller.HasMoveIntent, state.Has(MapRequirementItems.StormCloak));
            if (push == Vector2.zero || !_character.enabled)
            {
                return;
            }

            GroundedPush.Shove(_character, push * Time.deltaTime);
            if (_hitGust != gust)
            {
                _hitGust = gust;
                Say(FarnkeshMechanicsTexts.GustHit);
            }
        }

        private void SetStreaks(bool show, Vector2 direction, bool blowing)
        {
            for (int i = 0; i < StreakCount; i++)
            {
                LineRenderer streak = _streaks[i];
                if (!show)
                {
                    if (streak != null)
                    {
                        streak.enabled = false;
                    }

                    continue;
                }

                streak = streak != null ? streak : (_streaks[i] = NewLine("Fk_GustStreak", 0.06f, new Color(1f, 1f, 1f, 0.55f)));
                streak.enabled = true;

                // Warning: high in the sky over the hero. Blowing: round the body, sweeping along the wind.
                float sweep = Mathf.Repeat(Time.time * (blowing ? 1.6f : 0.5f) + i * 0.137f, 1f);
                var dir = new Vector3(direction.x, 0f, direction.y);
                Vector3 side = Vector3.Cross(Vector3.up, dir);
                float across = (i - (StreakCount - 1) * 0.5f) * (blowing ? 1.1f : 2.2f);
                float height = blowing ? 0.5f + (i % 3) * 0.7f : 9f + (i % 3) * 1.5f;
                Vector3 a = _player.position + side * across + dir * Mathf.Lerp(-7f, 7f, sweep) + Vector3.up * height;
                streak.SetPosition(0, a);
                streak.SetPosition(1, a + dir * (blowing ? 2.6f : 1.4f));
            }
        }

        // ------------------------------------------------------------------
        // Rope baskets and wind currents
        // ------------------------------------------------------------------

        private void TickStations(Vector2 local)
        {
            SkyRouteRules.StationsAt(OverworldLayouts.SkyRoutes, local, _stations);
            if (_stations.Count == 0)
            {
                _hintRoute = null;
                ShowRope(null);
                return;
            }

            MapRequirementState state = _heroState.Get(_bag);
            float hour = ServiceLocator.TryGet(out TimeOfDayManager clock) ? clock.CurrentHour : TimeOfDayManager.DefaultStartHour;
            bool canGlide = MapRequirementRules.IsMet(MapRequirementTags.WingsOrDoubleJump, state);
            MapModeController mode = MapModeController.Current;
            int level = mode != null ? mode.HeroLevel : 1;

            // Of the routes leaving from here, the first the hero can take; else the nearest, to explain why not.
            (SkyRoute Route, bool ToB) pick = _stations[0];
            SkyRideCheck check = SkyRideCheck.ZoneBlocked;
            for (int i = 0; i < _stations.Count; i++)
            {
                (SkyRoute route, bool toB) = _stations[i];
                bool open = SkyRouteRules.AllZonesOpen(_layout, _origin, route, level, state.Hero, _heroState.Lookup);
                SkyRideCheck c = SkyRouteRules.Check(route, toB, hour, canGlide, open);
                if (i == 0)
                {
                    check = c;
                }

                if (c == SkyRideCheck.Ok)
                {
                    pick = _stations[i];
                    check = c;
                    break;
                }
            }

            ShowRope(pick.Route);
            if (_hintRoute != pick.Route || Time.unscaledTime >= _nextHint)
            {
                _hintRoute = pick.Route;
                _nextHint = Time.unscaledTime + HintCooldown;
                Say(HintKey(pick.Route, pick.ToB, check));
            }

            InteractPromptRegistry.Ping(Time.unscaledTime);
            if (!_controller.Input.InteractPressedThisFrame)
            {
                return;
            }

            if (check == SkyRideCheck.Ok)
            {
                BeginRide(pick.Route, pick.ToB, canGlide);
            }
            else if (Time.unscaledTime >= _nextDenied)
            {
                _nextDenied = Time.unscaledTime + DeniedCooldown;
                Say(HintKey(pick.Route, pick.ToB, check));
            }
        }

        private static string HintKey(SkyRoute route, bool toB, SkyRideCheck check)
        {
            switch (check)
            {
                case SkyRideCheck.NeedsWings: return FarnkeshMechanicsTexts.NeedsWings;
                case SkyRideCheck.CurrentResting: return FarnkeshMechanicsTexts.CurrentResting;
                case SkyRideCheck.ZoneBlocked: return FarnkeshMechanicsTexts.RouteBlocked;
            }

            if (route.Kind == SkyRouteKind.Glide)
            {
                return FarnkeshMechanicsTexts.GlideReady;
            }

            return toB ? FarnkeshMechanicsTexts.BasketUp : FarnkeshMechanicsTexts.BasketDown;
        }

        private void BeginRide(SkyRoute route, bool toB, bool canGlide)
        {
            _ride = route;
            _rideToB = toB;
            _rideT = 0f;
            _rideSeconds = Mathf.Max(0.1f, SkyRouteRules.Duration(route, toB, canGlide));
            Vector2 from = _origin + SkyRouteRules.PositionAt(route, toB, 0f);
            Vector2 to = _origin + SkyRouteRules.PositionAt(route, toB, 1f);
            _rideFromY = Mathf.Max(0f, OverworldRules.SurfaceAt(_layout, from));
            _rideToY = Mathf.Max(0f, OverworldRules.SurfaceAt(_layout, to));

            // Clears the motor's velocity so the Animator rests in idle while the controls are off.
            _motor.Teleport(_player.position, Quaternion.LookRotation(new Vector3(to.x - from.x, 0f, to.y - from.y)));
            _controller.ClickToMove?.ClearDestination();
            _controller.enabled = false;
            _character.enabled = false;
            _lastRidePosition = _player.position;
            SetStreaks(false, Vector2.zero, false);
            ShowRope(route);
        }

        private void TickRide()
        {
            bool dead = _stats != null && !_stats.IsAlive;
            if (dead || (_player.position - _lastRidePosition).sqrMagnitude > ExternalMoveTolerance * ExternalMoveTolerance)
            {
                // The hero died, or something else moved them (a respawn, the barca): the ride is over where they are.
                EndRide(false);
                return;
            }

            _rideT = Mathf.Min(1f, _rideT + Time.deltaTime / _rideSeconds);
            Vector2 ground = _origin + SkyRouteRules.PositionAt(_ride, _rideToB, _rideT);
            float y = SkyRouteRules.HeightAt(_rideFromY, _rideToY, SkyRouteRules.Arc(_ride), _rideT);
            _player.position = new Vector3(ground.x, y, ground.y);
            _lastRidePosition = _player.position;
            if (_rideT >= 1f)
            {
                EndRide(true);
            }
        }

        private void EndRide(bool arrived)
        {
            if (_ride == null)
            {
                return;
            }

            SkyRoute route = _ride;
            bool toB = _rideToB;
            _ride = null;
            if (_character != null)
            {
                _character.enabled = true;
            }

            if (_controller != null)
            {
                _controller.enabled = true;
            }

            ShowRope(null);
            if (arrived && _view != null && _player != null)
            {
                Vector2 end = _origin + SkyRouteRules.PositionAt(route, toB, 1f);
                Vector2 start = _origin + SkyRouteRules.PositionAt(route, toB, 0f);

                // The landing's collider must exist for the snap to the ground.
                _view.WarmUp(end);
                CityModeController.PlaceAt(_player.gameObject, end, end - start);
                _view.ResetTracking(end);
            }
        }

        // ------------------------------------------------------------------
        // Feedback
        // ------------------------------------------------------------------

        private void ShowRope(SkyRoute route)
        {
            if (route == null)
            {
                if (_rope != null)
                {
                    _rope.enabled = false;
                }

                _ropeRoute = null;
                return;
            }

            // The rope of a route never moves: the ground under its ends is sampled once, when it is first shown.
            if (route == _ropeRoute && _rope != null && _rope.enabled)
            {
                return;
            }

            _ropeRoute = route;
            _rope = _rope != null ? _rope : NewLine("Fk_Rope", 0.05f, new Color(0.45f, 0.32f, 0.18f, 1f));
            _rope.enabled = true;
            float arc = SkyRouteRules.Arc(route);
            float above = route.Kind == SkyRouteKind.Glide ? 1.2f : 3.5f;
            Vector2 a = _origin + route.A;
            Vector2 b = _origin + route.B;
            float ya = Mathf.Max(0f, OverworldRules.SurfaceAt(_layout, a));
            float yb = Mathf.Max(0f, OverworldRules.SurfaceAt(_layout, b));
            const int points = 16;
            _rope.positionCount = points;
            for (int i = 0; i < points; i++)
            {
                float t = i / (points - 1f);
                Vector2 p = Vector2.Lerp(a, b, t);
                _rope.SetPosition(i, new Vector3(p.x, SkyRouteRules.HeightAt(ya, yb, arc, t) + above, p.y));
            }
        }

        private void HideFeedback()
        {
            SetStreaks(false, Vector2.zero, false);
            ShowRope(null);
        }

        private LineRenderer NewLine(string objectName, float width, Color color)
        {
            if (_material == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit") ?? Shader.Find("Particles/Standard Unlit") ?? Shader.Find("Sprites/Default");
                _material = new Material(shader) { name = "Mat_FarnkeshLine_Runtime" };
            }

            var go = new GameObject(objectName);
            go.transform.SetParent(transform, false);
            var line = go.AddComponent<LineRenderer>();
            line.sharedMaterial = _material;
            line.useWorldSpace = true;
            line.positionCount = 2;
            line.startWidth = line.endWidth = width;
            line.startColor = line.endColor = color;
            line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            line.receiveShadows = false;
            line.enabled = false;
            return line;
        }

        private void Say(string key)
        {
            ServiceLocator.TryGet(out LocalizationManager localization);
            CombatFxManager.SpawnLootWarningText(_player.position + Vector3.up * FeedbackHeight, FarnkeshMechanicsTexts.Get(localization, key));
        }
    }
}
