using System.Collections.Generic;
using Rapadura.Core.DI;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Combat;
using Rapadura.Gameplay.Content;
using Rapadura.Gameplay.Environment;
using Rapadura.Gameplay.Fishing;
using Rapadura.Gameplay.Inventory;
using Rapadura.Gameplay.Items;
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
    /// Scene side of the Mar Gelado mechanics (the rules are <see cref="MoonCanalRules"/>, <see cref="ThinIceRules"/> and
    /// <see cref="IceFishingSession"/>): added to the overworld root, idle unless the layout holds the region and the hero stands
    /// in it. The canals freeze with the moon and the landings carry the hero across; the thin ice patches load under a heavy
    /// mount and throw the rider off with a warning first; the holes of Old Kul's lake fish with one button, the hero keeping
    /// every control. Nothing here can hold the hero: the crossing hands control back on arrival or if anything else moves them.
    /// </summary>
    public sealed class MarGeladoMechanicsDriver : MonoBehaviour, IRegionMechanics
    {
        private const string PlayerTag = "Player";
        private const float HintCooldown = 3f;
        private const float DeniedCooldown = 1.5f;
        private const float ExternalMoveTolerance = 3f;
        private const float FeedbackHeight = 2.4f;
        private const float OutlineRange = 40f;
        private const float FrozenSheetWidth = 11.5f;
        private const float FrozenSheetLift = 0.12f;

        private readonly List<(SkyRoute Route, bool ToB)> _stations = new List<(SkyRoute, bool)>();
        private readonly Dictionary<int, float> _holeRestUntil = new Dictionary<int, float>();
        private readonly List<LineRenderer> _sheets = new List<LineRenderer>();
        private readonly HeroRequirementCache _heroState = new HeroRequirementCache();

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
        private LineRenderer _outline;
        private int _outlinePatch = -1;
        private LineRenderer _fishLine;

        private SkyRoute _ride;
        private bool _rideToB;
        private float _rideT;
        private float _rideSeconds;
        private float _rideFromY;
        private float _rideToY;
        private Vector3 _lastRidePosition;

        private bool _sheetsShown;
        private float _iceLoad;
        private bool _warnedPatch;
        private bool _warnedCreak;
        private float _nextHint;
        private float _nextDenied;
        private SkyRoute _hintRoute;

        private IceFishingSession _fishing;
        private int _fishingHole = -1;
        private bool _saidBite;
        private int _hintHole = -1;

        private Rect[] _areas;

        /// <inheritdoc/>
        public Rect[] MechanicsAreas => _areas;

        /// <inheritdoc/>
        public bool MechanicsBusy => _ride != null;

        private void Start()
        {
            _view = GetComponent<OverworldView>();
            _layout = _view != null ? _view.Layout : null;
            OverworldZone anchor = _layout?.Zone(OverworldLayouts.MarGeladoAnchorZoneId);
            if (anchor == null)
            {
                enabled = false;
                return;
            }

            _origin = anchor.Origin;
            _areas = new[] { MechanicsDriverHub.WorldArea(OverworldLayouts.MarGeladoBounds, _origin) };
        }

        private void OnEnable() => _heroState.Attach();

        private void OnDisable()
        {
            EndRide(false);
            _fishing = null;
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
            if (!OverworldLayouts.MarGeladoBounds.Contains(local))
            {
                HideFeedback();
                return;
            }

            int day = ServiceLocator.TryGet(out TimeOfDayManager clock) ? clock.Day : 1;
            ShowSheets(MoonCanalRules.IsFrozen(day));
            TickThinIce(local, clock);
            TickCrossings(local, day);
            TickFishing(local, clock);
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

        private static bool RidingHeavyMount()
        {
            MountController rider = MountController.Active;
            return rider != null && rider.IsMounted && ThinIceRules.IsHeavyMount(rider.Definition != null ? rider.Definition.MountId : null);
        }

        // ------------------------------------------------------------------
        // Thin ice
        // ------------------------------------------------------------------

        private void TickThinIce(Vector2 local, TimeOfDayManager clock)
        {
            int patch = ThinIceRules.PatchAt(OverworldLayouts.ThinIcePatches, local);
            bool heavy = RidingHeavyMount();
            bool loading = heavy && patch >= 0;
            bool night = clock != null && clock.Phase == TimeOfDayPhase.Night;
            _iceLoad = ThinIceRules.Advance(_iceLoad, loading, night, Time.deltaTime);
            ThinIceStage stage = ThinIceRules.Stage(_iceLoad);
            ShowOutline(local, stage);
            if (!loading)
            {
                _warnedPatch = false;
                if (stage == ThinIceStage.Holding)
                {
                    _warnedCreak = false;
                }

                return;
            }

            if (!_warnedPatch)
            {
                _warnedPatch = true;
                Say(MarGeladoMechanicsTexts.ThinIceAhead);
            }

            if (stage == ThinIceStage.Creaking && !_warnedCreak)
            {
                _warnedCreak = true;
                Say(MarGeladoMechanicsTexts.IceCreaks);
            }
            else if (stage == ThinIceStage.Broken)
            {
                BreakIce();
            }
        }

        private void BreakIce()
        {
            _iceLoad = 0f;
            _warnedPatch = false;
            _warnedCreak = false;
            MountController.Active?.Dismount(startRecall: true, leave: true);
            if (_stats != null && _stats.IsAlive)
            {
                float bite = ThinIceRules.BreakDamage(_stats.CurrentHealth, _stats.MaxHealth);
                if (bite > 0f)
                {
                    _stats.ApplyHazardDamage(bite);
                }
            }

            Say(MarGeladoMechanicsTexts.IceBroke);
        }

        private void ShowOutline(Vector2 local, ThinIceStage stage)
        {
            Rect[] patches = OverworldLayouts.ThinIcePatches;
            int nearest = -1;
            float best = OutlineRange;
            for (int i = 0; i < patches.Length; i++)
            {
                float d = CityGeometry.DistanceToRect(local, patches[i]);
                if (d < best)
                {
                    best = d;
                    nearest = i;
                }
            }

            if (nearest < 0)
            {
                if (_outline != null)
                {
                    _outline.enabled = false;
                }

                return;
            }

            if (_outline == null)
            {
                _outline = NewLine("Mg_ThinIceOutline", 0.22f);
                _outlinePatch = -1;
            }

            Color color = stage == ThinIceStage.Holding ? new Color(0.55f, 0.85f, 1f, 1f) : new Color(1f, 0.35f, 0.2f, 1f);
            _outline.startColor = _outline.endColor = color;
            _outline.enabled = true;
            if (nearest == _outlinePatch)
            {
                return;
            }

            // The patches never move: the five corners (and their ground) are set again only when the nearest patch changes.
            _outlinePatch = nearest;
            Rect r = patches[nearest];
            _outline.positionCount = 5;
            SetOutlineCorner(0, new Vector2(r.xMin, r.yMin));
            SetOutlineCorner(1, new Vector2(r.xMax, r.yMin));
            SetOutlineCorner(2, new Vector2(r.xMax, r.yMax));
            SetOutlineCorner(3, new Vector2(r.xMin, r.yMax));
            SetOutlineCorner(4, new Vector2(r.xMin, r.yMin));
        }

        private void SetOutlineCorner(int index, Vector2 corner)
        {
            Vector2 world = _origin + corner;
            _outline.SetPosition(index, new Vector3(world.x, Mathf.Max(0f, OverworldRules.SurfaceAt(_layout, world)) + 0.25f, world.y));
        }

        // ------------------------------------------------------------------
        // Canal crossings
        // ------------------------------------------------------------------

        private void TickCrossings(Vector2 local, int day)
        {
            SkyRouteRules.StationsAt(OverworldLayouts.CanalCrossings, local, _stations);
            if (_stations.Count == 0)
            {
                _hintRoute = null;
                return;
            }

            (SkyRoute route, bool toB) = _stations[0];
            MapRequirementState state = _heroState.Get(_bag);
            MapModeController mode = MapModeController.Current;
            int level = mode != null ? mode.HeroLevel : 1;
            bool open = SkyRouteRules.AllZonesOpen(_layout, _origin, route, level, state.Hero, _heroState.Lookup);
            bool hasBoat = state.Has(MapRequirementItems.ReedBoat) || MapRequirementRules.IsMet(MapRequirementTags.BoatOrDock, state);
            CanalCrossCheck check = MoonCanalRules.Check(day, hasBoat, RidingHeavyMount(), open);
            if (_hintRoute != route || Time.unscaledTime >= _nextHint)
            {
                _hintRoute = route;
                _nextHint = Time.unscaledTime + HintCooldown;
                SayCrossing(check, day);
            }

            InteractPromptRegistry.Ping(Time.unscaledTime);
            if (!_controller.Input.InteractPressedThisFrame)
            {
                return;
            }

            if (check == CanalCrossCheck.WalkOnIce || check == CanalCrossCheck.ByBoat)
            {
                BeginRide(route, toB, check);
            }
            else if (Time.unscaledTime >= _nextDenied)
            {
                _nextDenied = Time.unscaledTime + DeniedCooldown;
                SayCrossing(check, day);
            }
        }

        private void SayCrossing(CanalCrossCheck check, int day)
        {
            switch (check)
            {
                case CanalCrossCheck.WalkOnIce: Say(MarGeladoMechanicsTexts.CrossOnIce); break;
                case CanalCrossCheck.ByBoat: Say(MarGeladoMechanicsTexts.CrossByBoat); break;
                case CanalCrossCheck.HeavyMount: Say(MarGeladoMechanicsTexts.HeavyOnCanal); break;
                case CanalCrossCheck.ZoneBlocked: Say(MarGeladoMechanicsTexts.CrossingBlocked); break;
                default:
                    ServiceLocator.TryGet(out LocalizationManager localization);
                    SayText(MarGeladoMechanicsTexts.Format(localization, MarGeladoMechanicsTexts.NeedsBoat, MoonCanalRules.DaysUntilFrozen(day)));
                    break;
            }
        }

        private void BeginRide(SkyRoute route, bool toB, CanalCrossCheck how)
        {
            _fishing = null;
            ShowFishLine(false, false);
            _ride = route;
            _rideToB = toB;
            _rideT = 0f;
            _rideSeconds = Mathf.Max(0.1f, MoonCanalRules.Duration(Vector2.Distance(route.A, route.B), how));
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
        }

        private void TickRide()
        {
            if ((_player.position - _lastRidePosition).sqrMagnitude > ExternalMoveTolerance * ExternalMoveTolerance)
            {
                // Something else moved the hero (a respawn, the barca): the crossing is over where they are.
                EndRide(false);
                return;
            }

            _rideT = Mathf.Min(1f, _rideT + Time.deltaTime / _rideSeconds);
            Vector2 ground = _origin + SkyRouteRules.PositionAt(_ride, _rideToB, _rideT);
            float y = SkyRouteRules.HeightAt(_rideFromY, _rideToY, MoonCanalRules.CrossingArc, _rideT);
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
        // Ice fishing
        // ------------------------------------------------------------------

        private void TickFishing(Vector2 local, TimeOfDayManager clock)
        {
            if (_fishing != null)
            {
                TickFishingSession(local);
                return;
            }

            ShowFishLine(false, false);
            int hole = IceFishingRules.HoleAt(OverworldLayouts.IceFishingHoles, local);
            if (hole < 0)
            {
                _hintHole = -1;
                return;
            }

            bool mounted = MountController.Active != null && MountController.Active.IsMounted;
            bool resting = _holeRestUntil.TryGetValue(hole, out float until) && Time.time < until;
            string prompt = mounted ? MarGeladoMechanicsTexts.FishDismount : resting ? MarGeladoMechanicsTexts.FishResting : MarGeladoMechanicsTexts.FishPrompt;
            if (_hintHole != hole || Time.unscaledTime >= _nextHint)
            {
                _hintHole = hole;
                _nextHint = Time.unscaledTime + HintCooldown;
                Say(prompt);
            }

            InteractPromptRegistry.Ping(Time.unscaledTime);
            if (!_controller.Input.InteractPressedThisFrame || Time.unscaledTime < _nextDenied)
            {
                return;
            }

            if (mounted || resting)
            {
                _nextDenied = Time.unscaledTime + DeniedCooldown;
                Say(prompt);
                return;
            }

            bool night = clock != null && clock.Phase == TimeOfDayPhase.Night;
            _fishing = new IceFishingSession(unchecked(System.Environment.TickCount * 31 + hole), night, OverworldLayouts.IceFishingHoles[hole].Great);
            _fishingHole = hole;
            _saidBite = false;
            _holeRestUntil[hole] = Time.time + IceFishingRules.HoleCooldownSeconds;
            Say(MarGeladoMechanicsTexts.FishLineDown);
        }

        private void TickFishingSession(Vector2 local)
        {
            bool stillThere = IceFishingRules.HoleAt(OverworldLayouts.IceFishingHoles, local) == _fishingHole;
            bool alive = _stats == null || _stats.IsAlive;
            if (_controller.HasMoveIntent || !stillThere || !alive)
            {
                _fishing = null;
                ShowFishLine(false, false);
                Say(MarGeladoMechanicsTexts.FishCancelled);
                return;
            }

            InteractPromptRegistry.Ping(Time.unscaledTime);
            if (_controller.Input.InteractPressedThisFrame)
            {
                _fishing.Press();
            }

            _fishing.Tick(Time.deltaTime);
            ShowFishLine(true, _fishing.State == IceFishingState.Bite);
            if (_fishing.State == IceFishingState.Bite && !_saidBite)
            {
                _saidBite = true;
                Say(MarGeladoMechanicsTexts.FishBite);
            }

            if (_fishing.IsOver)
            {
                FinishFishing();
            }
        }

        private void FinishFishing()
        {
            IceFishingSession session = _fishing;
            _fishing = null;
            ShowFishLine(false, false);
            switch (session.State)
            {
                case IceFishingState.Caught: Land(session); break;
                case IceFishingState.ScaredOff: Say(MarGeladoMechanicsTexts.FishScared); break;
                default: Say(MarGeladoMechanicsTexts.FishLost); break;
            }
        }

        private void Land(IceFishingSession session)
        {
            ServiceLocator.TryGet(out LocalizationManager localization);
            FishingProgress.ResolveShared().RecordCatch(session.Fish.Id, session.Grams);
            ItemDefinition item = ItemDatabase.GetById(session.Fish.Id);
            int leftover = item != null && _bag != null ? _bag.AddItem(item, 1) : 1;
            string name = FishingTexts.Get(localization, ContentText.ItemNameKey(session.Fish.Id));
            SayText(MarGeladoMechanicsTexts.Format(localization, leftover == 0 ? MarGeladoMechanicsTexts.FishCaught : MarGeladoMechanicsTexts.FishBagFull, name));
        }

        private void ShowFishLine(bool show, bool biting)
        {
            if (!show)
            {
                if (_fishLine != null)
                {
                    _fishLine.enabled = false;
                }

                return;
            }

            _fishLine = _fishLine != null ? _fishLine : NewLine("Mg_FishLine", 0.03f);
            _fishLine.startColor = _fishLine.endColor = biting ? new Color(1f, 0.85f, 0.3f, 1f) : new Color(0.9f, 0.95f, 1f, 1f);
            _fishLine.enabled = true;
            Vector2 center = _origin + OverworldLayouts.IceFishingHoles[_fishingHole].Center;
            float bob = biting ? Mathf.Sin(Time.time * 30f) * 0.25f : Mathf.Sin(Time.time * 2f) * 0.05f;
            _fishLine.positionCount = 2;
            _fishLine.SetPosition(0, _player.position + Vector3.up * 1.4f);
            _fishLine.SetPosition(1, new Vector3(center.x, OverworldRelief.WaterY + 0.1f + bob, center.y));
        }

        // ------------------------------------------------------------------
        // Feedback
        // ------------------------------------------------------------------

        /// <summary>The frozen surface of the three canals: one flat ribbon each, on while the moon holds them.</summary>
        private void ShowSheets(bool show)
        {
            if (show == _sheetsShown)
            {
                return;
            }

            _sheetsShown = show;
            if (show && _sheets.Count == 0)
            {
                foreach (Vector2[] canal in new[] { OverworldLayouts.CanalOeste, OverworldLayouts.CanalLeste, OverworldLayouts.CanalDoMeio })
                {
                    _sheets.Add(NewSheet(canal));
                }
            }

            foreach (LineRenderer sheet in _sheets)
            {
                if (sheet != null)
                {
                    sheet.enabled = show;
                }
            }
        }

        private LineRenderer NewSheet(Vector2[] canal)
        {
            LineRenderer line = NewLine("Mg_FrozenCanal", FrozenSheetWidth);
            line.alignment = LineAlignment.TransformZ;
            line.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            line.startColor = line.endColor = new Color(0.82f, 0.91f, 0.96f, 1f);
            line.positionCount = canal.Length;
            line.numCornerVertices = 4;
            for (int i = 0; i < canal.Length; i++)
            {
                Vector2 world = _origin + canal[i];
                line.SetPosition(i, new Vector3(world.x, OverworldRelief.WaterY + FrozenSheetLift, world.y));
            }

            return line;
        }

        private void HideFeedback()
        {
            ShowSheets(false);
            if (_outline != null)
            {
                _outline.enabled = false;
            }

            _fishing = null;
            ShowFishLine(false, false);
            _iceLoad = 0f;
        }

        private LineRenderer NewLine(string objectName, float width)
        {
            if (_material == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit") ?? Shader.Find("Particles/Standard Unlit") ?? Shader.Find("Sprites/Default");
                _material = new Material(shader) { name = "Mat_MarGeladoLine_Runtime" };
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
            SayText(MarGeladoMechanicsTexts.Get(localization, key));
        }

        private void SayText(string text) => CombatFxManager.SpawnLootWarningText(_player.position + Vector3.up * FeedbackHeight, text);
    }
}
