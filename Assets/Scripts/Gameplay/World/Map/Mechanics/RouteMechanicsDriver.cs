using System;
using System.Collections.Generic;
using Rapadura.Core.DI;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Combat;
using Rapadura.Gameplay.Environment;
using Rapadura.Gameplay.Inventory;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.World.Map.Areas;
using Rapadura.Gameplay.World.Map.Overworld;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// Scene side of the route mechanics of the story road (<c>OverworldLayouts.RouteMechanics.cs</c>): Ammo's caravan through the sandstorm on
    /// the Estrada das Caravanas (07, <see cref="CaravanRoadEscortRules"/>), the monsoon fords and boats of Jhilpur (31, <see cref="JhilpurMonsoonRules"/>),
    /// the rope lifts and chutes of Sangkent (36, <see cref="SangkentLiftRules"/>), and on the Canal das Lanternas the drum's barges (41,
    /// <see cref="DrumBargeRules"/>) and the four locks (43, <see cref="CanalLockRules"/>). Added to the overworld root; each part is idle unless the
    /// layout holds its region and the hero stands near it. Every crossing, lift, chute and barge is a <see cref="MechanicsRide"/> (the controls come
    /// back on arrival, on death, when anything else moves the hero and in OnDisable); the flooded fords push with <see cref="GroundedPush"/>. Every
    /// station is one [E] (the mobile interact button: <see cref="InteractPromptRegistry"/>), marked by a coloured pillar, with its line of text.
    /// </summary>
    public sealed partial class RouteMechanicsDriver : MonoBehaviour, IRegionMechanics
    {
        private const string PlayerTag = "Player";
        private const string GoldItemId = "item_gold";
        private const float FeedbackHeight = 2.4f;
        private const float HintCooldown = 3f;
        private const float SayCooldown = 6f;
        private const float ZoneSampleStep = 10f;

        /// <summary>
        /// True while the hero walks beside Ammo's escorted caravan in the storm (<see cref="CaravanRoadEscortRules.Shelters"/>): the sandstorm's
        /// driver counts it as a shelter (a moving one). Reset whenever the escort is not on.
        /// </summary>
        public static bool HeroBesideCaravan { get; private set; }

        private readonly HeroRequirementCache _heroState = new HeroRequirementCache();
        private readonly StaticGroundCache _ground = new StaticGroundCache();
        private readonly MechanicsRide _ride = new MechanicsRide();
        private readonly Dictionary<int, LineRenderer> _lines = new Dictionary<int, LineRenderer>();
        private readonly HashSet<int> _usedLines = new HashSet<int>();
        private readonly List<int> _lineKeys = new List<int>();

        private OverworldLayout _layout;
        private Transform _player;
        private PlayerController _controller;
        private CharacterController _character;
        private PlayerStats _stats;
        private InventoryManager _bag;
        private Material _material;

        private bool _hasAreias;
        private Vector2 _areiasOrigin;
        private bool _hasDelta;
        private Vector2 _deltaOrigin;
        private bool _hasTeto;
        private Vector2 _tetoOrigin;
        private bool _hasMing;
        private Vector2 _mingOrigin;

        private int _hintSpot = -1;
        private float _nextHint;
        private readonly Dictionary<string, float> _nextSay = new Dictionary<string, float>();

        private string _arrivalKey;
        private object _arrivalArg;

        private Rect[] _areas;

        /// <inheritdoc/>
        public Rect[] MechanicsAreas => _areas;

        /// <inheritdoc/>
        public bool MechanicsBusy => _ride.Active || _caravanOn || _queuedLine >= 0 || _boardedLine >= 0 || _ferryFord >= 0 || _lessonOn;

        private void Start()
        {
            OverworldView view = GetComponent<OverworldView>();
            _layout = view != null ? view.Layout : null;
            OverworldZone areias = _layout?.Zone(OverworldLayouts.AreiasAnchorZoneId);
            OverworldZone delta = _layout?.Zone(OverworldLayouts.DeltaAnchorZoneId);
            OverworldZone teto = _layout?.Zone(OverworldLayouts.TetoAnchorZoneId);
            OverworldZone ming = _layout?.Zone(OverworldLayouts.MingchengAnchorZoneId);
            if (areias == null && delta == null && teto == null && ming == null)
            {
                enabled = false;
                return;
            }

            var areas = new List<Rect>(4);
            if (areias != null)
            {
                _hasAreias = true;
                _areiasOrigin = areias.Origin;
                areas.Add(MechanicsDriverHub.WorldArea(CaravanBounds, _areiasOrigin));
            }

            if (delta != null)
            {
                _hasDelta = true;
                _deltaOrigin = delta.Origin;
                _fordSide = new int[OverworldLayouts.DeltaFords.Length];
                areas.Add(MechanicsDriverHub.WorldArea(OverworldLayouts.DeltaBounds, _deltaOrigin));
            }

            if (teto != null)
            {
                _hasTeto = true;
                _tetoOrigin = teto.Origin;
                areas.Add(MechanicsDriverHub.WorldArea(TerraceBounds, _tetoOrigin));
            }

            if (ming != null)
            {
                _hasMing = true;
                _mingOrigin = ming.Origin;
                _wheelUntil = new double[OverworldLayouts.CanalLocks.Length];
                _bargeCourseLength = RidePath.Length(OverworldLayouts.DrumBargeCourse);
                areas.Add(MechanicsDriverHub.WorldArea(CanalBounds, _mingOrigin));
            }

            _areas = areas.ToArray();
        }

        private void OnEnable() => _heroState.Attach();

        private void OnDisable()
        {
            _ride.End();
            AbandonCaravan(false);
            CancelStations();
            HeroBesideCaravan = false;
            HideAllLines();
            _hintSpot = -1;
            _heroState.Detach();
        }

        private void OnDestroy()
        {
            HeroBesideCaravan = false;
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
                // The lines of the station left behind stay as they were for the ride (hidden on the first frame after it).
                TickRide();
                return;
            }

            _usedLines.Clear();
            Vector2 world = new Vector2(_player.position.x, _player.position.z);
            double now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / 1000d;
            bool spot = false;
            if (_hasAreias)
            {
                spot |= TickCaravan(world - _areiasOrigin, now);
            }
            else
            {
                HeroBesideCaravan = false;
            }

            if (_hasDelta)
            {
                spot |= TickMonsoon(world - _deltaOrigin);
            }

            if (_hasTeto)
            {
                spot |= TickTerraces(world - _tetoOrigin, now);
            }

            if (_hasMing)
            {
                spot |= TickCanal(world - _mingOrigin, now);
            }

            if (!spot)
            {
                _hintSpot = -1;
            }

            HideUnusedLines();
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
                _character = go.GetComponent<CharacterController>();
                _stats = go.GetComponent<PlayerStats>();
                _bag = go.GetComponentInChildren<InventoryManager>();
            }

            return _controller != null && _stats != null;
        }

        /// <summary>Whether a station may take the [E] now: alive, in control (no other driver's ride) and with an input.</summary>
        private bool CanInteract => _stats.IsAlive && _controller.enabled && _controller.Input != null;

        private bool Pressed => _controller.Input != null && _controller.Input.InteractPressedThisFrame;

        private static bool IsNight() => ServiceLocator.TryGet(out TimeOfDayManager clock) && clock.Phase == TimeOfDayPhase.Night;

        private static int GameDay() => ServiceLocator.TryGet(out TimeOfDayManager clock) ? clock.Day : 1;

        private static float GameHour() => ServiceLocator.TryGet(out TimeOfDayManager clock) ? clock.CurrentHour : TimeOfDayManager.DefaultStartHour;

        private static float SecondsPerGameHour() =>
            (ServiceLocator.TryGet(out TimeOfDayManager clock) ? clock.DayLengthSeconds : TimeOfDayManager.DefaultDayLengthSeconds) / TimeOfDayManager.HoursPerDay;

        private static long UtcDay(double now) => (long)Math.Floor(now / 86400d);

        private Vector2 HeroLocal(Vector2 origin) => new Vector2(_player.position.x, _player.position.z) - origin;

        // ------------------------------------------------------------------
        // Rides
        // ------------------------------------------------------------------

        /// <summary>Starts a ride along <paramref name="local"/> (region coordinates of the region at <paramref name="origin"/>); the arrival line is said on arrival.</summary>
        private bool BeginRide(Vector2 origin, Vector2[] local, bool reverse, float speed, float hover, float dropSpeed, string arrivalKey, object arrivalArg = null)
        {
            if (!_ride.Begin(_player.gameObject, _layout, RidePath.ToWorld(local, origin, reverse), speed, 0f, hover, dropSpeed))
            {
                return false;
            }

            _arrivalKey = arrivalKey;
            _arrivalArg = arrivalArg;
            _hintSpot = -1;
            return true;
        }

        private void TickRide()
        {
            RideStep step = _ride.Tick(Time.deltaTime);
            if (step == RideStep.Arrived && _arrivalKey != null)
            {
                if (_arrivalArg != null)
                {
                    SayFormat(_arrivalKey, _arrivalArg);
                }
                else
                {
                    Say(_arrivalKey);
                }
            }

            if (step != RideStep.Riding)
            {
                _arrivalKey = null;
                _arrivalArg = null;
            }
        }

        /// <summary>True when the soft barrier would let the hero through every zone along <paramref name="local"/> (it would throw them back off a ride otherwise).</summary>
        private bool CourseOpen(Vector2 origin, Vector2[] local)
        {
            MapRequirementState state = _heroState.Get(_bag);
            MapModeController mode = MapModeController.Current;
            int level = mode != null ? mode.HeroLevel : 1;
            Func<string, bool> lookup = _heroState.Lookup;
            for (int i = 0; i + 1 < local.Length; i++)
            {
                float leg = Vector2.Distance(local[i], local[i + 1]);
                int steps = Mathf.Max(1, Mathf.CeilToInt(leg / ZoneSampleStep));
                for (int k = 0; k <= steps; k++)
                {
                    OverworldZone zone = OverworldRules.ZoneAt(_layout, origin + Vector2.Lerp(local[i], local[i + 1], k / (float)steps));
                    if (zone == null || OverworldRules.CheckEnter(zone, level, state.Hero, lookup) != MapAccessCheck.Ok)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private void CancelStations()
        {
            _queuedLine = -1;
            _boardedLine = -1;
            _ferryFord = -1;
            _lessonOn = false;
        }

        // ------------------------------------------------------------------
        // Gold
        // ------------------------------------------------------------------

        private bool PayGold(int amount)
        {
            ItemDefinition gold = ItemDatabase.GetById(GoldItemId);
            if (gold == null || _bag == null || _bag.AddItem(gold, amount) > 0)
            {
                Say(RouteMechanicsTexts.BagFull);
                return false;
            }

            return true;
        }

        // ------------------------------------------------------------------
        // Lines
        // ------------------------------------------------------------------

        private float GroundY(Vector2 world) => _ground.GroundAt(_layout, world);

        private float WaterLineY(Vector2 world) => Mathf.Max(OverworldRelief.WaterY, GroundY(world));

        /// <summary>A vertical pillar at <paramref name="world"/> (a station, a camel, a barge), kept on this frame.</summary>
        private void Pillar(int key, Vector2 world, float height, float width, Color color)
        {
            float y = WaterLineY(world);
            Segment(key, new Vector3(world.x, y, world.y), new Vector3(world.x, y + height, world.y), width, color);
        }

        /// <summary>A straight line (a rope, a chute, a gate), kept on this frame.</summary>
        private void Segment(int key, Vector3 a, Vector3 b, float width, Color color)
        {
            if (!_lines.TryGetValue(key, out LineRenderer line) || line == null)
            {
                line = NewLine("Rt_Line_" + key);
                _lines[key] = line;
            }

            line.startWidth = line.endWidth = width;
            line.startColor = line.endColor = color;
            line.SetPosition(0, a);
            line.SetPosition(1, b);
            line.enabled = true;
            _usedLines.Add(key);
        }

        /// <summary>A line between two region points, <paramref name="lift"/> metres over the ground or water at each end.</summary>
        private void GroundSegment(int key, Vector2 origin, Vector2 localA, Vector2 localB, float lift, float width, Color color)
        {
            Vector2 a = origin + localA;
            Vector2 b = origin + localB;
            Segment(key, new Vector3(a.x, WaterLineY(a) + lift, a.y), new Vector3(b.x, WaterLineY(b) + lift, b.y), width, color);
        }

        private void HideUnusedLines()
        {
            _lineKeys.Clear();
            _lineKeys.AddRange(_lines.Keys);
            foreach (int key in _lineKeys)
            {
                LineRenderer line = _lines[key];
                if (line != null && line.enabled && !_usedLines.Contains(key))
                {
                    line.enabled = false;
                }
            }
        }

        private void HideAllLines()
        {
            foreach (LineRenderer line in _lines.Values)
            {
                if (line != null)
                {
                    line.enabled = false;
                }
            }
        }

        private LineRenderer NewLine(string objectName)
        {
            if (_material == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit") ?? Shader.Find("Particles/Standard Unlit") ?? Shader.Find("Sprites/Default");
                _material = new Material(shader) { name = "Mat_RouteMechanicsLine_Runtime" };
            }

            var go = new GameObject(objectName);
            go.transform.SetParent(transform, false);
            var line = go.AddComponent<LineRenderer>();
            line.sharedMaterial = _material;
            line.useWorldSpace = true;
            line.positionCount = 2;
            line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            line.receiveShadows = false;
            line.enabled = false;
            return line;
        }

        // ------------------------------------------------------------------
        // Texts
        // ------------------------------------------------------------------

        /// <summary>Shows a station's line when the hero steps onto it, then again every few seconds while they stay.</summary>
        private void Hint(int spot, string text)
        {
            if (_hintSpot == spot && Time.unscaledTime < _nextHint)
            {
                return;
            }

            _hintSpot = spot;
            _nextHint = Time.unscaledTime + HintCooldown;
            SayText(text);
        }

        private void HintKey(int spot, string key) => Hint(spot, Text(key));

        /// <summary>A line said at most once every <see cref="SayCooldown"/> s (the current of a ford, a waiting caravan).</summary>
        private void SayOnce(string key)
        {
            if (_nextSay.TryGetValue(key, out float next) && Time.unscaledTime < next)
            {
                return;
            }

            _nextSay[key] = Time.unscaledTime + SayCooldown;
            Say(key);
        }

        private static string Text(string key)
        {
            ServiceLocator.TryGet(out LocalizationManager localization);
            return RouteMechanicsTexts.Get(localization, key);
        }

        private static string Format(string key, params object[] args)
        {
            ServiceLocator.TryGet(out LocalizationManager localization);
            return RouteMechanicsTexts.Format(localization, key, args);
        }

        private void Say(string key) => SayText(Text(key));

        private void SayFormat(string key, params object[] args) => SayText(Format(key, args));

        private void SayText(string text)
        {
            if (_player != null)
            {
                CombatFxManager.SpawnLootWarningText(_player.position + Vector3.up * FeedbackHeight, text);
            }
        }
    }
}
