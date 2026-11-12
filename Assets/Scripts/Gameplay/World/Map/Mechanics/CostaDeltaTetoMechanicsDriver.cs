using System;
using System.Collections.Generic;
using Rapadura.Core.DI;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Combat;
using Rapadura.Gameplay.Inventory;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.Mounts;
using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.Skills;
using Rapadura.Gameplay.World.Map.Overworld;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// Scene side of the mechanics of Regions 5–7: the tide of the reef (<see cref="CostaTideRules"/>), the wandering reed islands
    /// (<see cref="DeltaIslandRules"/>) with the Fontes do Lodo of map 35 (<see cref="LodoFountainRules"/>, CostaDeltaTetoMechanicsDriver.Lodo.cs) and the avalanches of the chutes (<see cref="TetoAvalancheRules"/>). Added to the overworld root; each region is
    /// idle unless the layout holds it and the hero stands inside it. Positions are region coordinates, the anchor zone's origin says where the region sits
    /// in the stitched world. Damage goes through <see cref="ApplyHazard"/> on the hero's PlayerStats and always stops at a floor of life. Nothing here
    /// takes the hero's controls away, and no method is named after a PlayerInput action.
    /// </summary>
    public sealed partial class CostaDeltaTetoMechanicsDriver : MonoBehaviour, IRegionMechanics
    {
        private const string PlayerTag = "Player";
        private const string TideSource = "costa_tide";
        private const string BogSource = "delta_bog";
        private const float FeedbackHeight = 2.4f;
        private const float HintCooldown = 8f;
        private const float EffectSeconds = 1.2f;
        private const float EffectRefresh = 0.5f;
        private const float RedrawSeconds = 0.15f;
        private const float StateRefresh = 1f;
        private const float FlatRange = 90f;
        private const float BeaconRange = 140f;
        private const float BeaconHeight = 14f;
        private const float BogRange = 170f;
        private const float LaneRange = 220f;
        private const float LaneStep = 20f;
        private const int RingPoints = 40;
        private const int FrontPoints = 8;
        private const int FlatKeys = 0;
        private const int WaterKeys = 100;
        private const int ChestKeys = 200;
        private const int IslandKeys = 300;
        private const int SparkKey = 400;
        private const int BogKey = 401;

        /// <summary>Read back for a reef chest or the spark with no mark in the save (older saves, a new game): never taken.</summary>
        public const long NeverTaken = long.MinValue;

        /// <summary>WorldMapProgress time mark holding the island layout whose spark was last freed.</summary>
        public const string SparkMarkKey = "delta.spark";

        private const string ReefChestMarkPrefix = "costa.reefchest.";

        private static string[] s_reefChestMarkKeys;

        private static readonly Color FlatDryColor = new Color(0.95f, 0.85f, 0.55f, 0.9f);
        private static readonly Color FlatTurningColor = new Color(1f, 0.6f, 0.15f, 1f);
        private static readonly Color FlatHighColor = new Color(0.2f, 0.55f, 0.9f, 0.9f);
        private static readonly Color WaterEdgeColor = new Color(0.45f, 0.85f, 1f, 0.9f);
        private static readonly Color TreasureColor = new Color(1f, 0.9f, 0.5f, 1f);
        private static readonly Color IslandColor = new Color(0.72f, 0.8f, 0.4f, 1f);
        private static readonly Color BogColor = new Color(0.4f, 0.32f, 0.5f, 0.7f);
        private static readonly Color SparkColor = new Color(0.65f, 0.95f, 0.9f, 1f);
        private static readonly Color LaneWarnColor = new Color(1f, 0.3f, 0.15f, 1f);
        private static readonly Color FrontColor = new Color(0.95f, 0.98f, 1f, 1f);

        private readonly Dictionary<int, LineRenderer> _rings = new Dictionary<int, LineRenderer>();
        private readonly Dictionary<int, (Vector2 Center, float Radius)> _ringShapes = new Dictionary<int, (Vector2, float)>();
        private readonly LineRenderer[] _beacons = new LineRenderer[OverworldLayouts.TideFlats.Length];

        private OverworldLayout _layout;
        private Transform _player;
        private PlayerStats _stats;
        private PlayerController _controller;
        private CharacterController _character;
        private BuffController _buffs;
        private InventoryManager _bag;
        private MountController _mounts;
        private Material _material;
        private float _nextHint;
        private float _nextRedraw;
        private float _nextStateRefresh;
        private bool _hasBoat;

        private bool _hasCosta;
        private Vector2 _costaOrigin;
        private bool _inCosta;
        private CostaTidePhase _lastTidePhase = CostaTidePhase.High;
        private int _flatHere = -1;
        private long _warnedTide = long.MinValue;
        private float _nextTideBite;
        private long[] _chestTides;
        private StatModifierDefinition _tideSlow;
        private float _nextTideRefresh;
        private bool _tideApplied;

        private bool _hasDelta;
        private Vector2 _deltaOrigin;
        private bool _inDelta;
        private int _islandHere = -1;
        private bool _wasBog;
        private long _driftSaidLayout = long.MinValue;
        private long _sparkFreedLayout = long.MinValue;
        private StatModifierDefinition _bogSlow;
        private float _nextBogRefresh;
        private bool _bogApplied;

        private bool _hasTeto;
        private Vector2 _tetoOrigin;
        private bool _inTeto;
        private long _warnedCycle = long.MinValue;
        private long _hitCycle = long.MinValue;
        private long _slideCycle = long.MinValue;
        private float _lastProgress;
        private int _shownLane = -1;
        private LineRenderer _laneLine;
        private LineRenderer _frontLine;

        private Rect[] _areas;

        /// <inheritdoc/>
        public Rect[] MechanicsAreas => _areas;

        /// <inheritdoc/>
        public bool MechanicsBusy => false;

        private void Start()
        {
            OverworldView view = GetComponent<OverworldView>();
            _layout = view != null ? view.Layout : null;
            OverworldZone costa = _layout?.Zone(OverworldLayouts.CostaAnchorZoneId);
            OverworldZone delta = _layout?.Zone(OverworldLayouts.DeltaAnchorZoneId);
            OverworldZone teto = _layout?.Zone(OverworldLayouts.TetoAnchorZoneId);
            if (costa == null && delta == null && teto == null)
            {
                enabled = false;
                return;
            }

            if (costa != null)
            {
                _hasCosta = true;
                _costaOrigin = costa.Origin;
                _chestTides = new long[OverworldLayouts.TideFlats.Length];
                for (int i = 0; i < _chestTides.Length; i++)
                {
                    _chestTides[i] = long.MinValue;
                }

                _tideSlow = NewSlow("Maré", CostaTideRules.HighTideMoveBonus);
            }

            if (delta != null)
            {
                _hasDelta = true;
                _deltaOrigin = delta.Origin;
                _bogSlow = NewSlow("Lodo", DeltaIslandRules.BogMoveBonus);
            }

            if (teto != null)
            {
                _hasTeto = true;
                _tetoOrigin = teto.Origin;
            }

            var areas = new List<Rect>(3);
            if (_hasCosta)
            {
                areas.Add(MechanicsDriverHub.WorldArea(OverworldLayouts.CostaBounds, _costaOrigin));
            }

            if (_hasDelta)
            {
                areas.Add(MechanicsDriverHub.WorldArea(OverworldLayouts.DeltaBounds, _deltaOrigin));
            }

            if (_hasTeto)
            {
                areas.Add(MechanicsDriverHub.WorldArea(OverworldLayouts.TetoBounds, _tetoOrigin));
            }

            _areas = areas.ToArray();
        }

        private static StatModifierDefinition NewSlow(string displayName, float bonus)
        {
            return new StatModifierDefinition
            {
                displayName = displayName,
                affectedStat = StatType.MoveSpeed,
                application = ModifierApplication.PercentAdditive,
                value = bonus,
                durationSeconds = EffectSeconds,
                isDebuff = true,
            };
        }

        private void OnDisable()
        {
            // The lit fountains belong to this visit of the Delta: off with the driver (the hero left the region, the scene changed).
            LodoFountainBoard.Clear();
            ClearTideSlow();
            ClearBogSlow();
            if (_inCosta)
            {
                _inCosta = false;
                LeaveCosta();
            }

            if (_inDelta)
            {
                _inDelta = false;
                LeaveDelta();
            }

            if (_inTeto)
            {
                _inTeto = false;
                LeaveTeto();
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
            if (!FindPlayer())
            {
                return;
            }

            double now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / 1000d;
            Vector2 world = new Vector2(_player.position.x, _player.position.z);
            if (Time.unscaledTime >= _nextStateRefresh)
            {
                _nextStateRefresh = Time.unscaledTime + StateRefresh;
                _hasBoat = MapRequirementHeroState.Capture(_bag).Has(MapRequirementItems.ReedBoat);
            }

            bool redraw = Time.unscaledTime >= _nextRedraw;
            if (redraw)
            {
                _nextRedraw = Time.unscaledTime + RedrawSeconds;
            }

            if (_hasCosta && OverworldLayouts.CostaBounds.Contains(world - _costaOrigin))
            {
                _inCosta = true;
                TickTide(world - _costaOrigin, (long)now, redraw);
                TickSavanna(world - _costaOrigin, now);
            }
            else if (_inCosta)
            {
                _inCosta = false;
                LeaveCosta();
            }

            if (_hasDelta && OverworldLayouts.DeltaBounds.Contains(world - _deltaOrigin))
            {
                _inDelta = true;
                TickIslands(world - _deltaOrigin, now, redraw);
                TickLodo(world - _deltaOrigin, redraw);
                TickGaze(world - _deltaOrigin, now);
            }
            else if (_inDelta)
            {
                _inDelta = false;
                LeaveDelta();
            }

            if (_hasTeto && OverworldLayouts.TetoBounds.Contains(world - _tetoOrigin))
            {
                _inTeto = true;
                TickAvalanche(world - _tetoOrigin, now);
                TickCold(world - _tetoOrigin);
                TickVanes(world - _tetoOrigin);
            }
            else if (_inTeto)
            {
                _inTeto = false;
                LeaveTeto();
            }
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
                _stats = go.GetComponent<PlayerStats>();
                _controller = go.GetComponent<PlayerController>();
                _character = go.GetComponent<CharacterController>();
                _buffs = go.GetComponent<BuffController>();
                _bag = go.GetComponentInChildren<InventoryManager>();
                _mounts = go.GetComponent<MountController>();
            }

            return _stats != null && _controller != null;
        }

        private bool RidingAquatic => _mounts != null && _mounts.IsMounted && _mounts.Definition != null && _mounts.Definition.Type == MountType.Aquatico;

        /// <summary>
        /// The one way a hazard of these regions hurts the hero: a bite of <paramref name="fraction"/> of max life through PlayerStats.ApplyDamage, never past
        /// <paramref name="floorFraction"/> of max life. Returns the bite asked for (0 when there is none).
        /// </summary>
        public static float ApplyHazard(PlayerStats stats, float fraction, float floorFraction) => ProfundoMechanicsDriver.ApplyHazard(stats, fraction, floorFraction);

        // ------------------------------------------------------------------
        // What was taken, in the save (WorldMapProgress time marks hold the tide / layout number, not a Unix time)
        // ------------------------------------------------------------------

        /// <summary>The WorldMapProgress key of reef chest <paramref name="chest"/> ("costa.reefchest." + index), built once.</summary>
        public static string ReefChestMarkKey(int chest)
        {
            if (s_reefChestMarkKeys == null)
            {
                var keys = new string[OverworldLayouts.TideFlats.Length];
                for (int i = 0; i < keys.Length; i++)
                {
                    keys[i] = ReefChestMarkPrefix + i;
                }

                s_reefChestMarkKeys = keys;
            }

            return chest >= 0 && chest < s_reefChestMarkKeys.Length ? s_reefChestMarkKeys[chest] : ReefChestMarkPrefix + chest;
        }

        /// <summary>The tide in which reef chest <paramref name="chest"/> was last opened in this save, or <see cref="NeverTaken"/>.</summary>
        public static long ReefChestTide(WorldMapProgress progress, int chest) => progress != null ? progress.GetTimeMark(ReefChestMarkKey(chest), NeverTaken) : NeverTaken;

        public static void StoreReefChest(WorldMapProgress progress, int chest, long tide) => progress?.SetTimeMark(ReefChestMarkKey(chest), tide);

        /// <summary>The island layout whose spark was last freed in this save, or <see cref="NeverTaken"/>.</summary>
        public static long SparkLayout(WorldMapProgress progress) => progress != null ? progress.GetTimeMark(SparkMarkKey, NeverTaken) : NeverTaken;

        public static void StoreSpark(WorldMapProgress progress, long layout) => progress?.SetTimeMark(SparkMarkKey, layout);

        /// <summary>The save's mark when there is a WorldMapProgress, else this component's memory (a standalone scene).</summary>
        private long ChestTide(WorldMapProgress progress, int chest) => progress != null ? ReefChestTide(progress, chest) : _chestTides[chest];

        private long FreedSparkLayout(WorldMapProgress progress) => progress != null ? SparkLayout(progress) : _sparkFreedLayout;

        private static WorldMapProgress Progress() => ServiceLocator.TryGet(out WorldMapProgress progress) ? progress : null;

        // ------------------------------------------------------------------
        // The tide of the reef (Costa)
        // ------------------------------------------------------------------

        private void TickTide(Vector2 local, long unix, bool redraw)
        {
            CostaTideFlat[] flats = OverworldLayouts.TideFlats;
            CostaTidePhase phase = CostaTideRules.PhaseAt(unix);
            long tide = CostaTideRules.TideIndex(unix);
            bool nearReef = false;
            for (int i = 0; i < flats.Length && !nearReef; i++)
            {
                nearReef = Vector2.Distance(local, flats[i].Center) <= flats[i].Radius + FlatRange;
            }

            if (nearReef)
            {
                if (phase == CostaTidePhase.Low && _lastTidePhase != CostaTidePhase.Low)
                {
                    Say(CostaDeltaTetoMechanicsTexts.TideLow);
                }

                if (phase == CostaTidePhase.Turning && _warnedTide != tide)
                {
                    _warnedTide = tide;
                    Say(CostaDeltaTetoMechanicsTexts.TideTurning);
                }
            }

            _lastTidePhase = phase;
            int flat = CostaTideRules.FlatAt(flats, local);
            bool drenched = flat >= 0 && CostaTideRules.Drenches(phase, RidingAquatic);
            if (drenched && _flatHere < 0)
            {
                // Whoever walks in, or stood there as the water came, is told first: the first bite comes a tick later.
                _nextTideBite = Time.time + CostaTideRules.BiteTickSeconds;
                Say(CostaDeltaTetoMechanicsTexts.TideHigh);
            }

            _flatHere = drenched ? flat : -1;
            SetTideSlow(drenched);
            if (drenched && Time.time >= _nextTideBite)
            {
                _nextTideBite = Time.time + CostaTideRules.BiteTickSeconds;
                ApplyHazard(_stats, CostaTideRules.BiteFraction, CostaTideRules.BiteFloorFraction);
            }

            TickReefChest(local, flats, phase, tide);
            if (redraw)
            {
                ShowTide(local, flats, phase, tide, CostaTideRules.WaterLevel(unix));
            }
        }

        private void TickReefChest(Vector2 local, CostaTideFlat[] flats, CostaTidePhase phase, long tide)
        {
            int chest = phase != CostaTidePhase.High ? CostaTideRules.ChestAt(flats, local) : -1;
            if (chest < 0)
            {
                return;
            }

            WorldMapProgress progress = Progress();
            bool canOpen = CostaTideRules.CanOpenChest(ChestTide(progress, chest), tide, phase);
            Hint(canOpen ? CostaDeltaTetoMechanicsTexts.TideChestPrompt : CostaDeltaTetoMechanicsTexts.TideChestSpent);
            if (canOpen)
            {
                InteractPromptRegistry.Ping(Time.unscaledTime);
            }

            if (canOpen && _controller.Input.InteractPressedThisFrame && TryGive(CostaTideRules.ChestItem(chest, tide)))
            {
                _chestTides[chest] = tide;
                StoreReefChest(progress, chest, tide);
            }
        }

        private void ShowTide(Vector2 local, CostaTideFlat[] flats, CostaTidePhase phase, long tide, float water)
        {
            Color border = phase == CostaTidePhase.High ? FlatHighColor : phase == CostaTidePhase.Turning ? FlatTurningColor : FlatDryColor;
            WorldMapProgress progress = Progress();
            for (int i = 0; i < flats.Length; i++)
            {
                float distance = Vector2.Distance(local, flats[i].Center);
                bool near = distance <= flats[i].Radius + FlatRange;
                DrawRing(FlatKeys + i, _costaOrigin, flats[i].Center, flats[i].Radius, border, near, true);
                DrawRing(WaterKeys + i, _costaOrigin, flats[i].Center, Mathf.Max(0.5f, flats[i].Radius * water), WaterEdgeColor, near && water > 0.02f, false);
                bool open = CostaTideRules.CanOpenChest(ChestTide(progress, i), tide, phase);
                DrawRing(ChestKeys + i, _costaOrigin, flats[i].Center, CostaTideRules.ChestReach, TreasureColor, near && open, true);
                ShowBeacon(i, flats[i].Center, open && distance <= BeaconRange);
            }
        }

        private void ShowBeacon(int index, Vector2 center, bool show)
        {
            LineRenderer beacon = _beacons[index];
            if (beacon == null)
            {
                if (!show)
                {
                    return;
                }

                beacon = _beacons[index] = NewLine("Mx_ReefBeacon", 0.35f);
                Vector2 world = _costaOrigin + center;
                float ground = Ground(world);
                beacon.SetPosition(0, new Vector3(world.x, ground + 0.2f, world.y));
                beacon.SetPosition(1, new Vector3(world.x, ground + BeaconHeight, world.y));
                beacon.startColor = TreasureColor;
                beacon.endColor = new Color(TreasureColor.r, TreasureColor.g, TreasureColor.b, 0f);
            }

            beacon.enabled = show;
        }

        private void SetTideSlow(bool slow)
        {
            if (_buffs == null)
            {
                return;
            }

            if (!slow)
            {
                ClearTideSlow();
            }
            else if (Time.time >= _nextTideRefresh)
            {
                _nextTideRefresh = Time.time + EffectRefresh;
                _buffs.ApplyEffect(_tideSlow, TideSource);
                _tideApplied = true;
            }
        }

        private void ClearTideSlow()
        {
            if (_tideApplied && _buffs != null)
            {
                _buffs.RemoveEffectsBySource(TideSource);
            }

            _tideApplied = false;
            _nextTideRefresh = 0f;
        }

        private void LeaveCosta()
        {
            LeaveSavanna();
            _flatHere = -1;
            ClearTideSlow();
            CostaTideFlat[] flats = OverworldLayouts.TideFlats;
            for (int i = 0; i < flats.Length; i++)
            {
                HideRing(FlatKeys + i);
                HideRing(WaterKeys + i);
                HideRing(ChestKeys + i);
                ShowBeacon(i, flats[i].Center, false);
            }
        }

        // ------------------------------------------------------------------
        // The wandering islands (Delta)
        // ------------------------------------------------------------------

        private void TickIslands(Vector2 local, double now, bool redraw)
        {
            Vector2[] stations = OverworldLayouts.ReedIslandStations;
            long layout = DeltaIslandRules.LayoutIndex(now);
            bool near = Vector2.Distance(local, OverworldLayouts.ReedBogCenter) <= OverworldLayouts.ReedBogRadius + BogRange;
            if (!near)
            {
                SetBogSlow(false);
                _islandHere = -1;
                _wasBog = false;
                HideIslands();
                return;
            }

            if (DeltaIslandRules.IsDrifting(now) && _driftSaidLayout != layout)
            {
                _driftSaidLayout = layout;
                Say(CostaDeltaTetoMechanicsTexts.IslandsDrift);
            }

            int island = DeltaIslandRules.IslandAt(stations, now, local);
            if (island >= 0)
            {
                if (_islandHere < 0)
                {
                    Hint(CostaDeltaTetoMechanicsTexts.IslandBoard);
                }

                CarryHero(stations, island, now);
            }

            _islandHere = island;
            bool inBog = DeltaIslandRules.InBog(OverworldLayouts.ReedBogCenter, OverworldLayouts.ReedBogRadius, local, island);
            bool slowed = DeltaIslandRules.BogSlows(inBog, _hasBoat, RidingAquatic);
            if (slowed && !_wasBog)
            {
                Hint(CostaDeltaTetoMechanicsTexts.BogEnter);
            }

            _wasBog = slowed;
            SetBogSlow(slowed);
            TickSpark(local, stations, now, layout);
            if (redraw)
            {
                ShowIslands(stations, now, layout);
            }
        }

        private void CarryHero(Vector2[] stations, int island, double now)
        {
            if (_character == null || !_character.enabled)
            {
                return;
            }

            Vector2 carry = DeltaIslandRules.Carry(stations, island, now, Time.deltaTime);
            GroundedPush.Shove(_character, carry);
        }

        private void TickSpark(Vector2 local, Vector2[] stations, double now, long layout)
        {
            WorldMapProgress progress = Progress();
            if (!DeltaIslandRules.CanFreeSpark(FreedSparkLayout(progress), layout)
                || Vector2.Distance(local, DeltaIslandRules.Position(stations, DeltaIslandRules.SparkIsland(layout), now)) > DeltaIslandRules.SparkReach)
            {
                return;
            }

            Hint(CostaDeltaTetoMechanicsTexts.SparkPrompt);
            InteractPromptRegistry.Ping(Time.unscaledTime);
            if (_controller.Input.InteractPressedThisFrame && TryGive(DeltaIslandRules.SparkItem(layout)))
            {
                _sparkFreedLayout = layout;
                StoreSpark(progress, layout);
            }
        }

        private void ShowIslands(Vector2[] stations, double now, long layout)
        {
            for (int i = 0; i < DeltaIslandRules.Islands; i++)
            {
                DrawRing(IslandKeys + i, _deltaOrigin, DeltaIslandRules.Position(stations, i, now), DeltaIslandRules.IslandRadius, IslandColor, true, false);
            }

            bool spark = DeltaIslandRules.CanFreeSpark(FreedSparkLayout(Progress()), layout);
            DrawRing(SparkKey, _deltaOrigin, DeltaIslandRules.Position(stations, DeltaIslandRules.SparkIsland(layout), now), DeltaIslandRules.SparkReach, SparkColor, spark, false);
            DrawRing(BogKey, _deltaOrigin, OverworldLayouts.ReedBogCenter, OverworldLayouts.ReedBogRadius, BogColor, true, true);
        }

        private void HideIslands()
        {
            for (int i = 0; i < DeltaIslandRules.Islands; i++)
            {
                HideRing(IslandKeys + i);
            }

            HideRing(SparkKey);
            HideRing(BogKey);
        }

        private void SetBogSlow(bool slow)
        {
            if (_buffs == null)
            {
                return;
            }

            if (!slow)
            {
                ClearBogSlow();
            }
            else if (Time.time >= _nextBogRefresh)
            {
                _nextBogRefresh = Time.time + EffectRefresh;
                _buffs.ApplyEffect(_bogSlow, BogSource);
                _bogApplied = true;
            }
        }

        private void ClearBogSlow()
        {
            if (_bogApplied && _buffs != null)
            {
                _buffs.RemoveEffectsBySource(BogSource);
            }

            _bogApplied = false;
            _nextBogRefresh = 0f;
        }

        private void LeaveDelta()
        {
            LeaveGaze();
            _islandHere = -1;
            _wasBog = false;
            ClearBogSlow();
            HideIslands();
            LeaveLodo();
        }

        // ------------------------------------------------------------------
        // The avalanches of the chutes (Teto)
        // ------------------------------------------------------------------

        private void TickAvalanche(Vector2 local, double now)
        {
            TetoChuteLane[] lanes = OverworldLayouts.ChuteLanes;
            TetoAvalanchePhase phase = TetoAvalancheRules.PhaseAt(now);
            int laneIndex = TetoAvalancheRules.LaneAt(now);
            TetoChuteLane lane = lanes[laneIndex];
            long cycle = TetoAvalancheRules.CycleOf(now);
            float distance = TetoAvalancheRules.DistanceToLane(lane, local);

            if (phase == TetoAvalanchePhase.Warning && distance <= TetoAvalancheRules.AlertRange && _warnedCycle != cycle)
            {
                _warnedCycle = cycle;
                Say(CostaDeltaTetoMechanicsTexts.AvalancheWarning);
            }

            if (phase == TetoAvalanchePhase.Sliding)
            {
                if (_slideCycle != cycle)
                {
                    _slideCycle = cycle;
                    _lastProgress = 0f;
                }

                float progress = TetoAvalancheRules.Progress(now);
                if (_hitCycle != cycle && TetoAvalancheRules.CanHit(_warnedCycle, cycle) && TetoAvalancheRules.SweptBy(lane, local, _lastProgress, progress))
                {
                    _hitCycle = cycle;
                    ApplyHazard(_stats, TetoAvalancheRules.HitFraction, TetoAvalancheRules.HitFloorFraction);
                    Say(CostaDeltaTetoMechanicsTexts.AvalancheHit);
                }

                _lastProgress = progress;
            }
            else if (_slideCycle == cycle && _warnedCycle == cycle && _hitCycle != cycle && phase == TetoAvalanchePhase.Quiet)
            {
                _slideCycle = long.MinValue;
                if (distance <= TetoAvalancheRules.AlertRange)
                {
                    Hint(CostaDeltaTetoMechanicsTexts.AvalanchePassed);
                }
            }

            ShowAvalanche(lane, laneIndex, phase, TetoAvalancheRules.Progress(now), distance <= LaneRange);
        }

        private void ShowAvalanche(TetoChuteLane lane, int index, TetoAvalanchePhase phase, float progress, bool near)
        {
            bool showLane = near && phase != TetoAvalanchePhase.Quiet;
            if (!showLane)
            {
                if (_laneLine != null)
                {
                    _laneLine.enabled = false;
                }

                _shownLane = -1;
            }
            else
            {
                if (_laneLine == null)
                {
                    _laneLine = NewLine("Mx_ChuteLane", 0.4f);
                    _laneLine.loop = true;
                    _laneLine.startColor = _laneLine.endColor = LaneWarnColor;
                }

                if (_shownLane != index)
                {
                    _shownLane = index;
                    BuildLaneOutline(lane);
                }

                _laneLine.enabled = true;
            }

            if (phase != TetoAvalanchePhase.Sliding || !near)
            {
                if (_frontLine != null)
                {
                    _frontLine.enabled = false;
                }

                return;
            }

            if (_frontLine == null)
            {
                _frontLine = NewLine("Mx_AvalancheFront", 2.2f);
                _frontLine.positionCount = FrontPoints;
                _frontLine.startColor = _frontLine.endColor = FrontColor;
            }

            Vector2 axis = (lane.Bottom - lane.Top).normalized;
            Vector2 side = new Vector2(-axis.y, axis.x);
            Vector2 middle = _tetoOrigin + Vector2.Lerp(lane.Top, lane.Bottom, progress);
            for (int i = 0; i < FrontPoints; i++)
            {
                Vector2 p = middle + side * (lane.Width * (i / (FrontPoints - 1f) - 0.5f));
                _frontLine.SetPosition(i, new Vector3(p.x, Ground(p) + 0.6f, p.y));
            }

            _frontLine.enabled = true;
        }

        private void BuildLaneOutline(TetoChuteLane lane)
        {
            Vector2 axis = (lane.Bottom - lane.Top).normalized;
            Vector2 side = new Vector2(-axis.y, axis.x) * (lane.Width * 0.5f);
            float length = Vector2.Distance(lane.Top, lane.Bottom);
            int perSide = Mathf.Max(2, Mathf.CeilToInt(length / LaneStep) + 1);
            _laneLine.positionCount = perSide * 2;
            for (int i = 0; i < perSide; i++)
            {
                Vector2 middle = _tetoOrigin + Vector2.Lerp(lane.Top, lane.Bottom, i / (perSide - 1f));
                Vector2 right = middle + side;
                Vector2 left = middle - side;
                _laneLine.SetPosition(i, new Vector3(right.x, Ground(right) + 0.3f, right.y));
                _laneLine.SetPosition(perSide * 2 - 1 - i, new Vector3(left.x, Ground(left) + 0.3f, left.y));
            }
        }

        private void LeaveTeto()
        {
            LeaveCold();
            LeaveVanes();
            if (_laneLine != null)
            {
                _laneLine.enabled = false;
            }

            if (_frontLine != null)
            {
                _frontLine.enabled = false;
            }

            _shownLane = -1;
        }

        // ------------------------------------------------------------------
        // Feedback
        // ------------------------------------------------------------------

        private bool TryGive(string itemId)
        {
            ItemDefinition item = ItemDatabase.GetById(itemId);
            if (item == null)
            {
                return false;
            }

            ServiceLocator.TryGet(out LocalizationManager localization);
            int leftover = _bag != null ? _bag.AddItem(item, 1) : 1;
            SayText(CostaDeltaTetoMechanicsTexts.Format(localization, leftover == 0 ? CostaDeltaTetoMechanicsTexts.RewardFound : CostaDeltaTetoMechanicsTexts.RewardBagFull, item.DisplayName));
            _nextHint = Time.unscaledTime + HintCooldown;
            return leftover == 0;
        }

        private float Ground(Vector2 world) => OverworldRules.SurfaceAt(_layout, world);

        /// <summary>
        /// Draws (or hides) a ring round a centre in region coordinates. The points are placed again only when the centre or the
        /// radius changed since the last draw: a <paramref name="fixedRing"/> (tide flat, reef chest, bog: never moves) follows the
        /// ground point by point, paid once; a moving one (drifting island, spark, water edge) samples the ground once, at its centre.
        /// </summary>
        private void DrawRing(int key, Vector2 origin, Vector2 center, float radius, Color color, bool show, bool fixedRing)
        {
            _rings.TryGetValue(key, out LineRenderer ring);
            if (!show)
            {
                if (ring != null)
                {
                    ring.enabled = false;
                }

                return;
            }

            if (ring == null)
            {
                ring = NewLine("Mx_Ring", 0.25f);
                ring.loop = true;
                ring.positionCount = RingPoints;
                _rings[key] = ring;
                _ringShapes.Remove(key);
            }

            if (!_ringShapes.TryGetValue(key, out (Vector2 Center, float Radius) shape) || shape.Center != center || shape.Radius != radius)
            {
                _ringShapes[key] = (center, radius);
                float centreY = fixedRing ? 0f : Ground(origin + center) + 0.25f;
                for (int i = 0; i < RingPoints; i++)
                {
                    float a = i * Mathf.PI * 2f / RingPoints;
                    Vector2 world = origin + center + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * radius;
                    ring.SetPosition(i, new Vector3(world.x, fixedRing ? Ground(world) + 0.25f : centreY, world.y));
                }
            }

            ring.startColor = ring.endColor = color;
            ring.enabled = true;
        }

        private void HideRing(int key)
        {
            if (_rings.TryGetValue(key, out LineRenderer ring) && ring != null)
            {
                ring.enabled = false;
            }
        }

        private LineRenderer NewLine(string objectName, float width)
        {
            if (_material == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit") ?? Shader.Find("Particles/Standard Unlit") ?? Shader.Find("Sprites/Default");
                _material = new Material(shader) { name = "Mat_CostaDeltaTetoLine_Runtime" };
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

        private void Hint(string key)
        {
            if (Time.unscaledTime < _nextHint)
            {
                return;
            }

            _nextHint = Time.unscaledTime + HintCooldown;
            Say(key);
        }

        private void Say(string key)
        {
            ServiceLocator.TryGet(out LocalizationManager localization);
            SayText(CostaDeltaTetoMechanicsTexts.Get(localization, key));
        }

        private void SayText(string text) => CombatFxManager.SpawnLootWarningText(_player.position + Vector3.up * FeedbackHeight, text);
    }
}
