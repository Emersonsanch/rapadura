using System;
using Rapadura.Core.DI;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Combat;
using Rapadura.Gameplay.Environment;
using Rapadura.Gameplay.Inventory;
using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.World.Cities.Town;
using Rapadura.Gameplay.World.Map.Areas;
using Rapadura.Gameplay.World.Map.Overworld;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// Scene side of the current of the Rio de Luz (92, <see cref="CeuRiverRules"/>): [E] at a dark buoy on the south bank lights it, [E] at a
    /// lit buoy steps onto the light and the current carries the hero to the next lit buoy the way they face (<see cref="MechanicsRide"/>: east
    /// fast, west slow). The ride hands the controls back on arrival, on death, if anything else moves the hero and in OnDisable. Added to the
    /// overworld root, idle unless the layout holds the Céu region; it works only in the river row. The lit buoys stay in the save (time marks).
    /// </summary>
    public sealed class CeuRiverMechanicsDriver : MonoBehaviour, IRegionMechanics
    {
        private const string PlayerTag = "Player";
        private const float HintCooldown = 3f;
        private const float DeniedCooldown = 1.5f;
        private const float FeedbackHeight = 2.4f;
        private const float ViewRange = 220f;
        private const float LitHeight = 8f;
        private const float DarkHeight = 3f;

        private static readonly Color LitColor = new Color(1f, 0.88f, 0.45f, 1f);
        private static readonly Color DarkColor = new Color(0.45f, 0.45f, 0.55f, 0.7f);

        private readonly HeroRequirementCache _heroState = new HeroRequirementCache();
        private readonly StaticGroundCache _ground = new StaticGroundCache();
        private readonly MechanicsRide _ride = new MechanicsRide();
        private long[] _lit;
        private long[] _litLocal;

        private OverworldView _view;
        private OverworldLayout _layout;
        private Vector2 _origin;
        private Transform _player;
        private PlayerController _controller;
        private PlayerStats _stats;
        private InventoryManager _bag;
        private Material _material;
        private LineRenderer[] _beams;

        private float _nextHint;
        private float _nextDenied;
        private int _hintSpot = -1;
        private bool _hintDown;
        private float _lastRide = float.NegativeInfinity;
        private int _rideFrom = -1;
        private int _rideTo = -1;

        private Rect[] _areas;

        /// <inheritdoc/>
        public Rect[] MechanicsAreas => _areas;

        /// <inheritdoc/>
        public bool MechanicsBusy => _ride.Active;

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
            // The hub keeps the driver on over the whole region (like the rafts' driver): Update itself idles outside the river row.
            _areas = new[] { MechanicsDriverHub.WorldArea(OverworldLayouts.CeuBounds, _origin) };
            int n = OverworldLayouts.RioDeLuzBuoys.Length;
            _lit = new long[n];
            _litLocal = new long[n];
            _beams = new LineRenderer[n];
        }

        private void OnEnable() => _heroState.Attach();

        private void OnDisable()
        {
            EndRide(false);
            HideAll();
            _hintSpot = -1;
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
            if (_areas == null || !FindPlayer())
            {
                return;
            }

            if (_ride.Active)
            {
                RideStep step = _ride.Tick(Time.deltaTime);
                if (step != RideStep.Riding)
                {
                    EndRide(step == RideStep.Arrived);
                }

                return;
            }

            Vector2 local = new Vector2(_player.position.x, _player.position.z) - _origin;
            if (!OverworldLayouts.RioDeLuzRow.Contains(local))
            {
                HideAll();
                _hintSpot = -1;
                return;
            }

            ReadMarks();
            Show(local);
            if (_stats != null && !_stats.IsAlive)
            {
                return;
            }

            TickBuoys(local);
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
                _stats = go.GetComponent<PlayerStats>();
                _bag = go.GetComponentInChildren<InventoryManager>();
            }

            return _controller != null;
        }

        private void ReadMarks()
        {
            bool saved = ServiceLocator.TryGet(out WorldMapProgress progress);
            for (int i = 0; i < _lit.Length; i++)
            {
                _lit[i] = saved ? progress.GetTimeMark(CeuRiverRules.LitKey(i)) : _litLocal[i];
            }
        }

        private static bool IsNight() => ServiceLocator.TryGet(out TimeOfDayManager clock) && clock.Phase == TimeOfDayPhase.Night;

        // ------------------------------------------------------------------
        // The buoys
        // ------------------------------------------------------------------

        private void TickBuoys(Vector2 local)
        {
            Vector2[] buoys = OverworldLayouts.RioDeLuzBuoys;
            int buoy = CeuRiverRules.BuoyAt(buoys, local);
            if (buoy < 0 || _controller.Input == null)
            {
                _hintSpot = -1;
                return;
            }

            Vector3 f = _player.forward;
            bool down = CeuRiverRules.FacesDownstream(new Vector2(f.x, f.z));
            int to = CeuRiverRules.NextLit(_lit, buoy, down);
            bool zones = to < 0 || ZonesOpen(buoys[buoy], buoys[to]);
            CeuRiverCheck check = CeuRiverRules.Check(_lit, buoy, down, zones, Time.unscaledTime - _lastRide);
            if (_hintSpot != buoy || _hintDown != down || Time.unscaledTime >= _nextHint)
            {
                _hintSpot = buoy;
                _hintDown = down;
                _nextHint = Time.unscaledTime + HintCooldown;
                Hint(check, buoy, to);
            }

            bool available = check == CeuRiverCheck.Light || check == CeuRiverCheck.Ride;
            if (available)
            {
                InteractPromptRegistry.Ping(Time.unscaledTime);
            }

            if (!_controller.Input.InteractPressedThisFrame)
            {
                return;
            }

            if (check == CeuRiverCheck.Light)
            {
                long now = Math.Max(1L, DateTimeOffset.UtcNow.ToUnixTimeSeconds());
                _litLocal[buoy] = now;
                _lit[buoy] = now;
                if (ServiceLocator.TryGet(out WorldMapProgress progress))
                {
                    progress.SetTimeMark(CeuRiverRules.LitKey(buoy), now);
                }

                _hintSpot = -1;
                _nextHint = Time.unscaledTime + HintCooldown;
                Say(CeuRiverTexts.BuoyLit);
            }
            else if (check == CeuRiverCheck.Ride)
            {
                BeginRide(buoy, to);
            }
            else if (Time.unscaledTime >= _nextDenied)
            {
                _nextDenied = Time.unscaledTime + DeniedCooldown;
                Hint(check, buoy, to);
            }
        }

        private bool ZonesOpen(Vector2 a, Vector2 b)
        {
            MapRequirementState state = _heroState.Get(_bag);
            MapModeController mode = MapModeController.Current;
            int level = mode != null ? mode.HeroLevel : 1;
            var route = new SkyRoute { Id = "rio_de_luz", Kind = SkyRouteKind.Glide, A = a, B = b };
            return SkyRouteRules.AllZonesOpen(_layout, _origin, route, level, state.Hero, _heroState.Lookup);
        }

        private void Hint(CeuRiverCheck check, int buoy, int to)
        {
            ServiceLocator.TryGet(out LocalizationManager localization);
            switch (check)
            {
                case CeuRiverCheck.Light:
                    Say(CeuRiverTexts.BuoyDark);
                    break;
                case CeuRiverCheck.Ride:
                    Vector2[] buoys = OverworldLayouts.RioDeLuzBuoys;
                    int seconds = Mathf.CeilToInt(CeuRiverRules.RideSeconds(OverworldLayouts.RioDeLuz, buoys, buoy, to, IsNight()));
                    SayText(CeuRiverTexts.Format(localization, buoys[to].x > buoys[buoy].x ? CeuRiverTexts.RideDown : CeuRiverTexts.RideUp, seconds));
                    break;
                case CeuRiverCheck.ZoneBlocked:
                    Say(CeuRiverTexts.Blocked);
                    break;
                case CeuRiverCheck.Resting:
                    Say(CeuRiverTexts.Resting);
                    break;
                default:
                    Say(CeuRiverTexts.NoBuoy);
                    break;
            }
        }

        // ------------------------------------------------------------------
        // The ride
        // ------------------------------------------------------------------

        private void BeginRide(int from, int to)
        {
            Vector2[] buoys = OverworldLayouts.RioDeLuzBuoys;
            bool down = buoys[to].x > buoys[from].x;
            Vector2[] course = RidePath.ToWorld(CeuRiverRules.Course(OverworldLayouts.RioDeLuz, buoys, from, to), _origin, false);
            if (!_ride.Begin(_player.gameObject, _layout, course, CeuRiverRules.Speed(down, IsNight()), 0f, CeuRiverRules.Hover))
            {
                return;
            }

            _rideFrom = from;
            _rideTo = to;
            _hintSpot = -1;
            Say(down ? CeuRiverTexts.CarriedDown : CeuRiverTexts.CarriedUp);
        }

        /// <summary>Hands the controls back (the ride already did, unless the driver is being switched off) and sets the hero on the bank by the buoy.</summary>
        private void EndRide(bool arrived)
        {
            _ride.End();
            if (_rideTo < 0)
            {
                return;
            }

            int from = _rideFrom;
            int to = _rideTo;
            _rideFrom = -1;
            _rideTo = -1;
            _lastRide = Time.unscaledTime;
            if (!arrived || _view == null || _player == null)
            {
                return;
            }

            Vector2[] buoys = OverworldLayouts.RioDeLuzBuoys;
            Vector2 end = _origin + buoys[to];
            _view.WarmUp(end);
            CityModeController.PlaceAt(_player.gameObject, end, buoys[to] - buoys[from]);
            _view.ResetTracking(end);
            Say(CeuRiverTexts.Arrived);
        }

        // ------------------------------------------------------------------
        // Shown in the scene
        // ------------------------------------------------------------------

        private void Show(Vector2 local)
        {
            Vector2[] buoys = OverworldLayouts.RioDeLuzBuoys;
            for (int i = 0; i < buoys.Length; i++)
            {
                if ((buoys[i] - local).sqrMagnitude > ViewRange * ViewRange)
                {
                    if (_beams[i] != null)
                    {
                        _beams[i].enabled = false;
                    }

                    continue;
                }

                LineRenderer line = _beams[i] ??= NewLine();
                bool lit = CeuRiverRules.IsLit(_lit[i]);
                Vector2 world = _origin + buoys[i];
                Vector3 foot = new Vector3(world.x, _ground.GroundAt(_layout, world), world.y);
                line.enabled = true;
                line.startColor = line.endColor = lit ? LitColor : DarkColor;
                line.SetPosition(0, foot);
                line.SetPosition(1, foot + Vector3.up * (lit ? LitHeight : DarkHeight));
            }
        }

        private void HideAll()
        {
            if (_beams == null)
            {
                return;
            }

            foreach (LineRenderer line in _beams)
            {
                if (line != null)
                {
                    line.enabled = false;
                }
            }
        }

        private LineRenderer NewLine()
        {
            if (_material == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit") ?? Shader.Find("Particles/Standard Unlit") ?? Shader.Find("Sprites/Default");
                _material = new Material(shader) { name = "Mat_CeuRiverLine_Runtime" };
            }

            var go = new GameObject("Ceu_RiverBuoy");
            go.transform.SetParent(transform, false);
            var line = go.AddComponent<LineRenderer>();
            line.sharedMaterial = _material;
            line.useWorldSpace = true;
            line.positionCount = 2;
            line.startWidth = line.endWidth = 0.6f;
            line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            line.receiveShadows = false;
            line.enabled = false;
            return line;
        }

        private void Say(string key)
        {
            ServiceLocator.TryGet(out LocalizationManager localization);
            SayText(CeuRiverTexts.Get(localization, key));
        }

        private void SayText(string text)
        {
            if (_player != null)
            {
                CombatFxManager.SpawnLootWarningText(_player.position + Vector3.up * FeedbackHeight, text);
            }
        }
    }
}
