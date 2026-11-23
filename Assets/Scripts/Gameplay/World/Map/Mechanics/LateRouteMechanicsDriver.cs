using System;
using System.Collections.Generic;
using Rapadura.Core.DI;
using Rapadura.Core.Events;
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
    /// Scene side of the late route mechanics (<c>OverworldLayouts.LateRoute.cs</c>): the walks along the bottom of Kirta's reef (28, Costa,
    /// <see cref="KirtaReefWalkRules"/>), the tunnels that change of the Galerias dos Mamutes (62) and the floes of the Mar das Auroras (63, both
    /// Mar Gelado, <see cref="MammothGalleryRules"/>, <see cref="AuroraFloeRules"/>), the sledge of the Areal Escondido (73, Rio,
    /// <see cref="HiddenSandsSledgeRules"/>), the dark of the Cova da Treva (88, Limiar Profundo, <see cref="DarkPitLightRules"/>) and the lanterns
    /// between the barques (93 → 97, Céu, <see cref="SunShipLanternRules"/>). Added to the overworld root; each part is idle unless the layout
    /// holds its region and the hero stands near it. Every walk, tunnel, hop and sledge run is a <see cref="MechanicsRide"/> (the controls come
    /// back on arrival, on death, when anything else moves the hero and in OnDisable). Every station is one [E] (the mobile interact button:
    /// <see cref="InteractPromptRegistry"/>), marked by a coloured pillar, with its line of text. The save only changes on an [E] or a payment
    /// (the lit stands, the paid sledge run, the lanterns set today), never per frame.
    /// </summary>
    public sealed partial class LateRouteMechanicsDriver : MonoBehaviour, IRegionMechanics
    {
        private const string PlayerTag = "Player";
        private const string GoldItemId = "item_gold";
        private const float FeedbackHeight = 2.4f;
        private const float HintCooldown = 3f;
        private const float SayCooldown = 6f;
        private const float ZoneSampleStep = 10f;

        private readonly HeroRequirementCache _heroState = new HeroRequirementCache();
        private readonly StaticGroundCache _ground = new StaticGroundCache();
        private readonly MechanicsRide _ride = new MechanicsRide();
        private readonly Dictionary<int, LineRenderer> _lines = new Dictionary<int, LineRenderer>();
        private readonly HashSet<int> _usedLines = new HashSet<int>();
        private readonly List<int> _lineKeys = new List<int>();
        private readonly Action<DamageAppliedEvent> _damageHandler;

        private OverworldLayout _layout;
        private Transform _player;
        private PlayerController _controller;
        private PlayerStats _stats;
        private InventoryManager _bag;
        private Material _material;
        private bool _subscribed;

        private bool _hasCosta;
        private Vector2 _costaOrigin;
        private bool _hasGelado;
        private Vector2 _geladoOrigin;
        private bool _hasRio;
        private Vector2 _rioOrigin;
        private bool _hasProfundo;
        private Vector2 _profundoOrigin;
        private bool _hasCeu;
        private Vector2 _ceuOrigin;

        private int _hintSpot = -1;
        private float _nextHint;
        private readonly Dictionary<string, float> _nextSay = new Dictionary<string, float>();

        private string _arrivalKey;
        private object _arrivalArg;

        private Rect[] _areas;

        public LateRouteMechanicsDriver() => _damageHandler = OnDamage;

        /// <inheritdoc/>
        public Rect[] MechanicsAreas => _areas;

        /// <inheritdoc/>
        public bool MechanicsBusy => _ride.Active || _lanternOn || _hopLeg >= 0;

        private void Start()
        {
            OverworldView view = GetComponent<OverworldView>();
            _layout = view != null ? view.Layout : null;
            OverworldZone costa = _layout?.Zone(OverworldLayouts.CostaAnchorZoneId);
            OverworldZone gelado = _layout?.Zone(OverworldLayouts.MarGeladoAnchorZoneId);
            OverworldZone rio = _layout?.Zone(OverworldLayouts.RioAnchorZoneId);
            OverworldZone profundo = _layout?.Zone(OverworldLayouts.ProfundoAnchorZoneId);
            OverworldZone ceu = _layout?.Zone(OverworldLayouts.CeuAnchorZoneId);
            if (costa == null && gelado == null && rio == null && profundo == null && ceu == null)
            {
                enabled = false;
                return;
            }

            var areas = new List<Rect>(6);
            if (costa != null)
            {
                _hasCosta = true;
                _costaOrigin = costa.Origin;
                areas.Add(MechanicsDriverHub.WorldArea(ReefBounds, _costaOrigin));
            }

            if (gelado != null)
            {
                _hasGelado = true;
                _geladoOrigin = gelado.Origin;
                areas.Add(MechanicsDriverHub.WorldArea(GalleryBounds, _geladoOrigin));
                areas.Add(MechanicsDriverHub.WorldArea(FloeBounds, _geladoOrigin));
            }

            if (rio != null)
            {
                _hasRio = true;
                _rioOrigin = rio.Origin;
                areas.Add(MechanicsDriverHub.WorldArea(SledgeBounds, _rioOrigin));
            }

            if (profundo != null)
            {
                _hasProfundo = true;
                _profundoOrigin = profundo.Origin;
                foreach (Rect pit in OverworldLayouts.DarkPitAreas)
                {
                    areas.Add(MechanicsDriverHub.WorldArea(pit, _profundoOrigin));
                }
            }

            if (ceu != null)
            {
                _hasCeu = true;
                _ceuOrigin = ceu.Origin;
                areas.Add(MechanicsDriverHub.WorldArea(LanternBounds, _ceuOrigin));
            }

            _areas = areas.ToArray();
        }

        private void OnEnable()
        {
            _heroState.Attach();
            if (!_subscribed)
            {
                _subscribed = true;
                EventBus.Subscribe(_damageHandler);
            }
        }

        private void OnDisable()
        {
            _ride.End();
            _hopLeg = -1;
            _sledgeOn = false;
            DropLantern(null);
            LeaveDark();
            HideAllLines();
            _hintSpot = -1;
            _heroState.Detach();
            if (_subscribed)
            {
                _subscribed = false;
                EventBus.Unsubscribe(_damageHandler);
            }
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

            double now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / 1000d;
            Vector2 world = new Vector2(_player.position.x, _player.position.z);

            // The lantern burns whatever the hero does (a ride of this driver puts it out like any other).
            if (_hasCeu)
            {
                TickLanternCarry(world);
            }

            if (_ride.Active)
            {
                // The lines of the station left behind stay as they were for the ride (hidden on the first frame after it).
                TickRide(now);
                return;
            }

            _usedLines.Clear();
            bool spot = false;
            if (_hasCosta)
            {
                spot |= TickReef(world - _costaOrigin);
            }

            if (_hasGelado)
            {
                spot |= TickGallery(world - _geladoOrigin, now);
                spot |= TickFloes(world - _geladoOrigin, now);
            }

            if (_hasRio)
            {
                spot |= TickSledge(world - _rioOrigin, now);
            }

            if (_hasProfundo)
            {
                spot |= TickDark(world - _profundoOrigin, now);
            }

            if (_hasCeu)
            {
                spot |= TickLanternStations(world - _ceuOrigin, now);
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
                _stats = go.GetComponent<PlayerStats>();
                _bag = go.GetComponentInChildren<InventoryManager>();
            }

            return _controller != null && _stats != null;
        }

        /// <summary>Whether a station may take the [E] now: alive, in control (no other driver's ride) and with an input.</summary>
        private bool CanInteract => _stats.IsAlive && _controller.enabled && _controller.Input != null;

        private bool Pressed => _controller.Input != null && _controller.Input.InteractPressedThisFrame;

        private static bool IsNight() => ServiceLocator.TryGet(out TimeOfDayManager clock) && clock.Phase == TimeOfDayPhase.Night;

        private static long UtcDay(double now) => (long)Math.Floor(now / 86400d);

        private void OnDamage(DamageAppliedEvent evt)
        {
            if (_player == null)
            {
                return;
            }

            GameObject hero = _player.gameObject;
            if (evt.Target == hero && evt.FinalDamage > 0f)
            {
                _pendingBlows++;
            }
            else if (evt.Source == hero && evt.Target != null)
            {
                OnHeroHit(evt.Target, evt.Element);
            }
        }

        // ------------------------------------------------------------------
        // Rides
        // ------------------------------------------------------------------

        /// <summary>Starts a ride along <paramref name="local"/> (region coordinates of the region at <paramref name="origin"/>); the arrival line is said on arrival.</summary>
        private bool BeginRide(Vector2 origin, Vector2[] local, bool reverse, float speed, float arc, float hover, string arrivalKey, object arrivalArg = null)
        {
            if (!_ride.Begin(_player.gameObject, _layout, RidePath.ToWorld(local, origin, reverse), speed, arc, hover))
            {
                return false;
            }

            _arrivalKey = arrivalKey;
            _arrivalArg = arrivalArg;
            _hintSpot = -1;
            return true;
        }

        private void TickRide(double now)
        {
            if (_sledgeOn)
            {
                SteerSledge(now);
            }

            if (_hopLeg >= 0)
            {
                SteerHop(now);
            }

            RideStep step = _ride.Tick(Time.deltaTime);
            if (_diveOn && step == RideStep.Riding)
            {
                TickDiveBreaths();
            }

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
                _diveOn = false;
                if (_sledgeOn)
                {
                    EndSledge(step == RideStep.Arrived, now);
                }

                if (_hopLeg >= 0)
                {
                    NextHop(step == RideStep.Arrived);
                }
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

        // ------------------------------------------------------------------
        // Gold
        // ------------------------------------------------------------------

        private bool PayGold(int amount)
        {
            ItemDefinition gold = ItemDatabase.GetById(GoldItemId);
            if (gold == null || _bag == null || !_bag.CanFit(gold, amount) || _bag.AddItem(gold, amount) > 0)
            {
                Say(LateRouteMechanicsTexts.BagFull);
                return false;
            }

            return true;
        }

        // ------------------------------------------------------------------
        // Lines
        // ------------------------------------------------------------------

        private float GroundY(Vector2 world) => _ground.GroundAt(_layout, world);

        private float WaterLineY(Vector2 world) => Mathf.Max(OverworldRelief.WaterY, GroundY(world));

        /// <summary>A vertical pillar at <paramref name="world"/> (a station, a bubble, a stand), kept on this frame.</summary>
        private void Pillar(int key, Vector2 world, float height, float width, Color color)
        {
            float y = WaterLineY(world);
            Segment(key, new Vector3(world.x, y, world.y), new Vector3(world.x, y + height, world.y), width, color);
        }

        /// <summary>A straight line (a rope, a floe, a tunnel), kept on this frame.</summary>
        private void Segment(int key, Vector3 a, Vector3 b, float width, Color color)
        {
            if (!_lines.TryGetValue(key, out LineRenderer line) || line == null)
            {
                line = NewLine("Lr_Line_" + key);
                _lines[key] = line;
            }

            line.startWidth = line.endWidth = width;
            line.startColor = line.endColor = color;
            line.SetPosition(0, a);
            line.SetPosition(1, b);
            line.enabled = true;
            _usedLines.Add(key);
        }

        /// <summary>A line between two world points, <paramref name="lift"/> metres over the ground or water at each end.</summary>
        private void GroundSegment(int key, Vector2 a, Vector2 b, float lift, float width, Color color)
        {
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
                _material = new Material(shader) { name = "Mat_LateRouteMechanicsLine_Runtime" };
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

        /// <summary>A line said at most once every <see cref="SayCooldown"/> s.</summary>
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
            return LateRouteMechanicsTexts.Get(localization, key);
        }

        private static string Format(string key, params object[] args)
        {
            ServiceLocator.TryGet(out LocalizationManager localization);
            return LateRouteMechanicsTexts.Format(localization, key, args);
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
