using System;
using System.Collections.Generic;
using Rapadura.Core.DI;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Combat;
using Rapadura.Gameplay.Environment;
using Rapadura.Gameplay.Inventory;
using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.World.Map.Areas;
using Rapadura.Gameplay.World.Map.Overworld;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// Scene side of the ring gates of Mingcheng (41, <see cref="MingchengRingGateRules"/>, <c>OverworldLayouts.MingchengRings.cs</c>): added to the
    /// overworld root, idle unless the layout holds the Planície de Mingcheng and the hero stands near the city. By day only the gates' towers
    /// show (gold pillars where the roads cross the 4th ring). When the bell rings at 20 h the lantern fence of the 4th ring lights up between
    /// the gates and holds whoever is outside: a hero who came from outside is shoved back over the ring line (<see cref="GroundedPush"/>), the
    /// shut gates are bars across the roads, and the station outside each gate (a red pillar) takes one [E] (the mobile interact button:
    /// <see cref="InteractPromptRegistry"/>): in for free at a watch or with tonight's safe-conduct, for the toll otherwise, a wait line without
    /// the gold. Going in is a short <see cref="MechanicsRide"/> through the gate. Leaving is never held. The save only changes when the toll is
    /// paid (the night of the safe-conduct), never per frame.
    /// </summary>
    public sealed class MingchengRingGateMechanicsDriver : MonoBehaviour, IRegionMechanics
    {
        private const string PlayerTag = "Player";
        private const string GoldItemId = "item_gold";
        private const float FeedbackHeight = 2.4f;
        private const float HintCooldown = 3f;
        private const float SayCooldown = 6f;
        private const float DayHintCooldown = 60f;
        private const float ZoneSampleStep = 10f;
        private const float AnnounceRange = 110f;
        private const int ArcPoints = 24;
        private const float FenceLift = 1.6f;
        private const float GateLift = 1.2f;
        private const float TowerHeight = 5.5f;

        private const int TowerKey = 0;
        private const int BarKey = 10;
        private const int StationKey = 20;
        private const int GateSpot = 0;

        /// <summary>Where the driver works (region coordinates of Mingcheng): the city and the 4th ring with room round it.</summary>
        public static readonly Rect RingBounds = Rect.MinMaxRect(-120f, -120f, 120f, 120f);

        private static readonly Color TowerColor = new Color(1f, 0.78f, 0.3f, 1f);
        private static readonly Color FenceColor = new Color(1f, 0.35f, 0.2f, 0.9f);
        private static readonly Color BarShutColor = new Color(0.85f, 0.15f, 0.1f, 1f);
        private static readonly Color BarOpenColor = new Color(0.45f, 0.95f, 0.55f, 0.7f);
        private static readonly Color StationColor = new Color(0.95f, 0.3f, 0.2f, 1f);

        private readonly HeroRequirementCache _heroState = new HeroRequirementCache();
        private readonly StaticGroundCache _ground = new StaticGroundCache();
        private readonly MechanicsRide _ride = new MechanicsRide();
        private readonly Dictionary<int, LineRenderer> _lines = new Dictionary<int, LineRenderer>();
        private readonly HashSet<int> _usedLines = new HashSet<int>();
        private readonly List<int> _lineKeys = new List<int>();
        private readonly List<LineRenderer> _fence = new List<LineRenderer>();
        private readonly Dictionary<string, float> _nextSay = new Dictionary<string, float>();

        private OverworldLayout _layout;
        private Transform _player;
        private PlayerController _controller;
        private CharacterController _character;
        private PlayerStats _stats;
        private InventoryManager _bag;
        private Material _material;

        private Vector2 _origin;
        private Rect[] _areas;
        private List<Vector2[]> _arcs;
        private bool _fenceBuilt;
        private bool _fenceOn;

        private int _side = MingchengRingGateRules.Unknown;
        private Vector2 _last;
        private int _lastCurfew = -1;
        private int _localPassNight = int.MinValue;
        private string _arrivalKey;

        private int _hintSpot = -1;
        private float _nextHint;

        /// <inheritdoc/>
        public Rect[] MechanicsAreas => _areas;

        /// <inheritdoc/>
        public bool MechanicsBusy => _ride.Active;

        private void Start()
        {
            OverworldView view = GetComponent<OverworldView>();
            _layout = view != null ? view.Layout : null;
            OverworldZone ming = _layout?.Zone(OverworldLayouts.MingchengAnchorZoneId);
            if (ming == null)
            {
                enabled = false;
                return;
            }

            _origin = ming.Origin;
            _arcs = MingchengRingGateRules.WallArcs(OverworldLayouts.MingchengRingGates, OverworldLayouts.MingchengRingCenter, ArcPoints);
            _areas = new[] { MechanicsDriverHub.WorldArea(RingBounds, _origin) };
        }

        private void OnEnable() => _heroState.Attach();

        private void OnDisable()
        {
            _ride.End();
            _arrivalKey = null;
            _side = MingchengRingGateRules.Unknown;
            _lastCurfew = -1;
            HideAllLines();
            ShowFence(false);
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
                    if (step == RideStep.Arrived && _arrivalKey != null)
                    {
                        Say(_arrivalKey);
                    }

                    _arrivalKey = null;
                    _side = MingchengRingGateRules.Unknown;
                }

                return;
            }

            _usedLines.Clear();
            Vector2 local = new Vector2(_player.position.x, _player.position.z) - _origin;
            int day = GameDay();
            float hour = GameHour();
            bool curfew = MingchengRingGateRules.Curfew(day, hour);
            bool inBounds = RingBounds.Contains(local);

            Announce(local, curfew, inBounds);
            HoldAtRing(local, curfew);
            bool spot = inBounds && TickGates(local, day, hour, curfew);
            if (!spot)
            {
                _hintSpot = -1;
            }

            ShowFence(inBounds && curfew);
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

        private bool CanInteract => _stats.IsAlive && _controller.enabled && _controller.Input != null;

        private bool Pressed => _controller.Input != null && _controller.Input.InteractPressedThisFrame;

        private static int GameDay() => ServiceLocator.TryGet(out TimeOfDayManager clock) ? clock.Day : 1;

        private static float GameHour() => ServiceLocator.TryGet(out TimeOfDayManager clock) ? clock.CurrentHour : TimeOfDayManager.DefaultStartHour;

        // ------------------------------------------------------------------
        // The bell and the drum
        // ------------------------------------------------------------------

        /// <summary>The bell and the drum are heard near the city when they ring (not on arrival: only a change of the curfew speaks).</summary>
        private void Announce(Vector2 local, bool curfew, bool inBounds)
        {
            int now = curfew ? 1 : 0;
            int before = _lastCurfew;
            _lastCurfew = now;
            if (before < 0 || before == now || !inBounds || (local - OverworldLayouts.MingchengRingCenter).magnitude > AnnounceRange)
            {
                return;
            }

            if (!curfew)
            {
                Say(MingchengRingGateTexts.Drum);
                return;
            }

            bool inside = MingchengRingGateRules.SideOf(local, OverworldLayouts.MingchengRingCenter) == MingchengRingGateRules.Inside;
            if (inside)
            {
                Say(MingchengRingGateTexts.BellInside);
            }
            else
            {
                SayFormat(MingchengRingGateTexts.BellOutside, MingchengRingGateRules.Toll);
            }

            ServiceLocator.TryGet(out LocalizationManager localization);
            string line = MingchengRingGateTexts.BellLineFor(_heroState.Get(_bag).Hero);
            SayText(MingchengRingGateTexts.Get(localization, line), 0.7f);
        }

        // ------------------------------------------------------------------
        // The lantern fence
        // ------------------------------------------------------------------

        /// <summary>Under the curfew a hero who came from outside is shoved back over the ring line; going out is never held.</summary>
        private void HoldAtRing(Vector2 local, bool curfew)
        {
            Vector2 center = OverworldLayouts.MingchengRingCenter;
            if (!_stats.IsAlive || !_controller.enabled)
            {
                // Dead, or carried by another driver's ride: take the side again afterwards.
                _side = MingchengRingGateRules.Unknown;
                _last = local;
                return;
            }

            int before = _side;
            _side = MingchengRingGateRules.NextSide(_side, _last, local, center, curfew);
            Vector2 push = MingchengRingGateRules.WallPush(_side, local, center, curfew);
            if (push != Vector2.zero)
            {
                if (_character != null && _character.enabled)
                {
                    GroundedPush.Shove(_character, push);
                    local += push;
                }

                SayOnce(MingchengRingGateTexts.WallShut);
            }
            else if (curfew && before == MingchengRingGateRules.Inside && _side == MingchengRingGateRules.Outside)
            {
                SayFormat(MingchengRingGateTexts.LeftAtNight, MingchengRingGateRules.Toll);
            }

            _last = local;
        }

        // ------------------------------------------------------------------
        // The gates
        // ------------------------------------------------------------------

        /// <summary>True while the hero stands at a gate's outside station (or, by day, in its passage).</summary>
        private bool TickGates(Vector2 local, int day, float hour, bool curfew)
        {
            RingGate[] gates = OverworldLayouts.MingchengRingGates;
            int night = MingchengRingGateRules.NightOf(day, hour);
            bool pass = curfew && HasPass(night);
            int watch = MingchengRingGateRules.OpenWatch(hour);
            bool posternOpen = !curfew || pass || watch >= 0;
            ShowGates(gates, curfew, posternOpen);
            if (!CanInteract)
            {
                return false;
            }

            if (!curfew)
            {
                int crossing = MingchengRingGateRules.CrossingAt(gates, local);
                if (crossing < 0)
                {
                    return false;
                }

                // Once a minute per gate: the roads through the gates are walked all day.
                string key = MingchengRingGateTexts.GateName(crossing);
                if (!_nextSay.TryGetValue(key, out float sayAt) || Time.unscaledTime >= sayAt)
                {
                    _nextSay[key] = Time.unscaledTime + DayHintCooldown;
                    bool festa = !DrumBargeRules.IsDrumHour(hour);
                    SayText(festa ? Text(MingchengRingGateTexts.FestaOpen) : Format(MingchengRingGateTexts.DayOpen, Text(key)));
                }

                return false;
            }

            int index = MingchengRingGateRules.OuterAt(gates, local);
            if (index < 0)
            {
                return false;
            }

            RingGate gate = gates[index];
            int spot = GateSpot + index;
            int gold = _bag != null ? _bag.GetTotalCount(GoldItemId) : 0;
            RingGateCheck check = MingchengRingGateRules.Check(true, watch >= 0, pass, gold);
            int next = MingchengRingGateRules.NextOpening(hour);
            if (check == RingGateCheck.Wait)
            {
                Hint(spot, Format(MingchengRingGateTexts.WaitHint, next, MingchengRingGateRules.Toll));
                return true;
            }

            if (!CourseOpen(gate.Course))
            {
                Hint(spot, Text(MingchengRingGateTexts.ZoneBlocked));
                return true;
            }

            switch (check)
            {
                case RingGateCheck.Watch:
                    Hint(spot, Format(MingchengRingGateTexts.WatchPrompt, watch));
                    break;
                case RingGateCheck.Pass:
                    Hint(spot, Text(MingchengRingGateTexts.PassPrompt));
                    break;
                default:
                    Hint(spot, Format(MingchengRingGateTexts.TollPrompt, MingchengRingGateRules.Toll, next));
                    break;
            }

            InteractPromptRegistry.Ping(Time.unscaledTime);
            if (!Pressed)
            {
                return true;
            }

            if (check == RingGateCheck.Toll)
            {
                if (_bag == null || !_bag.RemoveById(GoldItemId, MingchengRingGateRules.Toll))
                {
                    return true;
                }

                // The one write of this mechanic: the night of the safe-conduct.
                _localPassNight = night;
                if (ServiceLocator.TryGet(out WorldMapProgress progress) && progress != null)
                {
                    progress.SetTimeMark(MingchengRingGateRules.PassKey, MingchengRingGateRules.StorePass(night));
                }

                SayFormat(MingchengRingGateTexts.Paid, MingchengRingGateRules.Toll);
            }
            else
            {
                Say(check == RingGateCheck.Watch ? MingchengRingGateTexts.WatchIn : MingchengRingGateTexts.PassIn);
            }

            if (_ride.Begin(_player.gameObject, _layout, RidePath.ToWorld(gate.Course, _origin, false), MingchengRingGateRules.CrossSpeed, 0f, 0f))
            {
                _arrivalKey = MingchengRingGateTexts.Arrived;
                _hintSpot = -1;
            }

            return true;
        }

        private bool HasPass(int night)
        {
            if (_localPassNight == night)
            {
                return true;
            }

            return ServiceLocator.TryGet(out WorldMapProgress progress) && progress != null
                && MingchengRingGateRules.PassValid(progress.GetTimeMark(MingchengRingGateRules.PassKey), night);
        }

        /// <summary>True when the soft barrier would let the hero through every zone along <paramref name="local"/>.</summary>
        private bool CourseOpen(Vector2[] local)
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
                    OverworldZone zone = OverworldRules.ZoneAt(_layout, _origin + Vector2.Lerp(local[i], local[i + 1], k / (float)steps));
                    if (zone == null || OverworldRules.CheckEnter(zone, level, state.Hero, lookup) != MapAccessCheck.Ok)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        // ------------------------------------------------------------------
        // Lines
        // ------------------------------------------------------------------

        private float GroundY(Vector2 world) => Mathf.Max(OverworldRelief.WaterY, _ground.GroundAt(_layout, world));

        private void ShowGates(RingGate[] gates, bool curfew, bool posternOpen)
        {
            for (int i = 0; i < gates.Length; i++)
            {
                RingGate gate = gates[i];
                Vector2 left = _origin + gate.Crossing + gate.Across * MingchengRingGateRules.GateHalfWidth;
                Vector2 right = _origin + gate.Crossing - gate.Across * MingchengRingGateRules.GateHalfWidth;
                Pillar(TowerKey + i * 2, left, TowerHeight, 0.7f, TowerColor);
                Pillar(TowerKey + i * 2 + 1, right, TowerHeight, 0.7f, TowerColor);
                if (!curfew)
                {
                    continue;
                }

                Segment(BarKey + i, new Vector3(left.x, GroundY(left) + GateLift, left.y), new Vector3(right.x, GroundY(right) + GateLift, right.y),
                    posternOpen ? 0.25f : 1.2f, posternOpen ? BarOpenColor : BarShutColor);
                Pillar(StationKey + i, _origin + gate.Outer, 3f, 0.45f, StationColor);
            }
        }

        private void ShowFence(bool on)
        {
            if (on && !_fenceBuilt)
            {
                BuildFence();
            }

            if (_fenceOn == on)
            {
                return;
            }

            _fenceOn = on;
            foreach (LineRenderer line in _fence)
            {
                if (line != null)
                {
                    line.enabled = on;
                }
            }
        }

        /// <summary>The fence's arcs between the gates, laid once over the ground (the plain round the city does not move).</summary>
        private void BuildFence()
        {
            _fenceBuilt = true;
            if (_arcs == null)
            {
                return;
            }

            for (int a = 0; a < _arcs.Count; a++)
            {
                Vector2[] arc = _arcs[a];
                LineRenderer line = NewLine("Mrg_Fence_" + a);
                line.positionCount = arc.Length;
                line.startWidth = line.endWidth = 0.35f;
                line.startColor = line.endColor = FenceColor;
                for (int k = 0; k < arc.Length; k++)
                {
                    Vector2 world = _origin + arc[k];
                    line.SetPosition(k, new Vector3(world.x, GroundY(world) + FenceLift, world.y));
                }

                _fence.Add(line);
            }
        }

        private void Pillar(int key, Vector2 world, float height, float width, Color color)
        {
            float y = GroundY(world);
            Segment(key, new Vector3(world.x, y, world.y), new Vector3(world.x, y + height, world.y), width, color);
        }

        private void Segment(int key, Vector3 a, Vector3 b, float width, Color color)
        {
            if (!_lines.TryGetValue(key, out LineRenderer line) || line == null)
            {
                line = NewLine("Mrg_Line_" + key);
                _lines[key] = line;
            }

            line.startWidth = line.endWidth = width;
            line.startColor = line.endColor = color;
            line.SetPosition(0, a);
            line.SetPosition(1, b);
            line.enabled = true;
            _usedLines.Add(key);
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
                _material = new Material(shader) { name = "Mat_MingchengRingGateLine_Runtime" };
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
            return MingchengRingGateTexts.Get(localization, key);
        }

        private static string Format(string key, params object[] args)
        {
            ServiceLocator.TryGet(out LocalizationManager localization);
            return MingchengRingGateTexts.Format(localization, key, args);
        }

        private void Say(string key) => SayText(Text(key));

        private void SayFormat(string key, params object[] args) => SayText(Format(key, args));

        private void SayText(string text, float lift = 0f)
        {
            if (_player != null)
            {
                CombatFxManager.SpawnLootWarningText(_player.position + Vector3.up * (FeedbackHeight + lift), text);
            }
        }
    }
}
