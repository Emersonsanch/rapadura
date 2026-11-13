using System.Collections.Generic;
using Rapadura.Core.DI;
using Rapadura.Core.Events;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Combat;
using Rapadura.Gameplay.Environment;
using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.Skills;
using Rapadura.Gameplay.World.Map.Areas;
using Rapadura.Gameplay.World.Map.Overworld;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// Scene side of the Limiar Profundo mechanics (the rules are <see cref="ProfundoLampRules"/>, <see cref="ProfundoFloorRules"/> and
    /// <see cref="ProfundoSmokeRules"/>): added to the overworld root, idle unless the layout holds the region and the hero stands in it. Positions are
    /// region coordinates, the anchor zone's origin says where the region sits in the stitched world. Lamps and fans work by standing beside them
    /// (no input is read, nothing is named after a PlayerInput action); damage goes through <see cref="ApplyHazard"/> on the hero's PlayerStats, and
    /// always stops at a floor of life. Nothing here takes the hero's controls away.
    /// </summary>
    public sealed class ProfundoMechanicsDriver : MonoBehaviour, IRegionMechanics
    {
        private const string PlayerTag = "Player";
        private const string SmokeSource = "profundo_smoke";
        private const float FeedbackHeight = 2.4f;
        private const float HintCooldown = 8f;
        private const float EffectSeconds = 1.2f;
        private const float EffectRefresh = 0.5f;
        private const float LampRingRange = 35f;
        private const float LampLightRange = 70f;
        private const float BankRingRange = 60f;
        private const float FrontRefresh = 0.1f;
        private const float SmokeReturnWarning = 4f;
        private const int SmallRingPoints = 32;
        private const int WideRingPoints = 72;
        private const int FloorSpan = 3;
        private const int FloorTiles = (FloorSpan * 2 + 1) * (FloorSpan * 2 + 1);
        private const int LampKeys = 0;
        private const int VentKeys = 100;
        private const int BankKeys = 200;
        private const int SafeKeys = 300;

        private static readonly Color LampOutColor = new Color(0.4f, 0.38f, 0.42f, 0.7f);
        private static readonly Color LampLitColor = new Color(1f, 0.7f, 0.28f, 1f);
        private static readonly Color LampTendColor = new Color(1f, 0.9f, 0.55f, 1f);
        private static readonly Color SafeColor = new Color(1f, 0.75f, 0.35f, 0.9f);
        private static readonly Color FrontColor = new Color(0.35f, 0.2f, 0.55f, 1f);
        private static readonly Color TileWarnColor = new Color(1f, 0.75f, 0.2f, 1f);
        private static readonly Color TileBurnColor = new Color(1f, 0.3f, 0.1f, 1f);
        private static readonly Color SmokeColor = new Color(0.48f, 0.46f, 0.5f, 0.8f);
        private static readonly Color VentColor = new Color(0.6f, 0.8f, 0.9f, 0.8f);
        private static readonly Color ClearColor = new Color(0.55f, 0.9f, 1f, 1f);

        private readonly Dictionary<int, LineRenderer> _rings = new Dictionary<int, LineRenderer>();
        private readonly Light[] _lampLights = new Light[ProfundoLampRules.LampCount];
        private readonly LineRenderer[] _tiles = new LineRenderer[FloorTiles];
        private readonly Vector2Int[] _tileCells = new Vector2Int[FloorTiles];
        private readonly bool[] _tileBuilt = new bool[FloorTiles];

        private OverworldView _view;
        private OverworldLayout _layout;
        private Vector2 _origin;
        private Transform _player;
        private PlayerStats _stats;
        private BuffController _buffs;
        private Material _material;
        private LineRenderer _front;
        private float _nextFrontRefresh;

        private ProfundoLampNetwork _network;
        private ProfundoTidePhase _lastPhase = ProfundoTidePhase.Calm;
        private int _snuffTide = -1;
        private int _snuffsApplied;
        private int _warnedTide = -1;
        private int _roseTide = -1;
        private int _bittenTide = -1;
        private float _nextTideBite;
        private int _hintLamp = -1;
        private float _nextLampHint;

        private bool _onFloor;
        private int _warnedFloorCycle = -1;
        private float _nextFloorBite;
        private float _nextFloorBurnSay;

        private float[] _clearLeft;
        private float _ventProgress;
        private int _ventHere = -1;
        private int _bankHere = -1;
        private float _nextSmokeBite;
        private float _nextSmokeHint;
        private StatModifierDefinition _smokeSlow;
        private float _nextSmokeRefresh;
        private bool _smokeApplied;

        private bool _inRegion;

        private void OnEnable() => EventBus.Subscribe<SkillCastEvent>(HandleSkillCast);

        private void OnDisable()
        {
            EventBus.Unsubscribe<SkillCastEvent>(HandleSkillCast);
            ClearSmokeEffect();
            LeaveRegion();
        }

        private Rect[] _areas;

        /// <inheritdoc/>
        public Rect[] MechanicsAreas => _areas;

        /// <inheritdoc/>
        public bool MechanicsBusy => false;

        private void Start()
        {
            _view = GetComponent<OverworldView>();
            _layout = _view != null ? _view.Layout : null;
            OverworldZone anchor = _layout?.Zone(OverworldLayouts.ProfundoAnchorZoneId);
            if (anchor == null)
            {
                enabled = false;
                return;
            }

            _origin = anchor.Origin;
            _areas = new[] { MechanicsDriverHub.WorldArea(OverworldLayouts.ProfundoBounds, _origin) };
            _network = new ProfundoLampNetwork(OverworldLayouts.WallLamps.Length);
            _clearLeft = new float[OverworldLayouts.SmokeBanks.Length];
            _smokeSlow = new StatModifierDefinition
            {
                displayName = "Fumaça",
                affectedStat = StatType.MoveSpeed,
                application = ModifierApplication.PercentAdditive,
                value = ProfundoSmokeRules.MoveBonus,
                durationSeconds = EffectSeconds,
                isDebuff = true,
            };
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
            if (_network == null || !FindPlayer())
            {
                return;
            }

            Vector2 local = new Vector2(_player.position.x, _player.position.z) - _origin;
            if (!OverworldLayouts.ProfundoBounds.Contains(local))
            {
                LeaveRegion();
                return;
            }

            _inRegion = true;
            bool night = ServiceLocator.TryGet(out TimeOfDayManager clock) && clock.Phase == TimeOfDayPhase.Night;
            TickLamps(local, night);
            TickFloor(local);
            TickSmoke(local);
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
                _buffs = go.GetComponent<BuffController>();
            }

            return _stats != null;
        }

        private void LeaveRegion()
        {
            if (!_inRegion)
            {
                return;
            }

            _inRegion = false;
            _lastPhase = ProfundoTidePhase.Calm;
            _onFloor = false;
            _bankHere = -1;
            _ventHere = -1;
            _ventProgress = 0f;
            _network.Tend(-1, 0f);
            ClearSmokeEffect();
            HideFloor();
            if (_front != null)
            {
                _front.enabled = false;
            }

            foreach (LineRenderer ring in _rings.Values)
            {
                if (ring != null)
                {
                    ring.enabled = false;
                }
            }

            foreach (Light light in _lampLights)
            {
                if (light != null)
                {
                    light.enabled = false;
                }
            }
        }

        /// <summary>
        /// The one way a hazard of the region hurts the hero: a bite of <paramref name="fraction"/> of max life through PlayerStats.ApplyHazardDamage, never past
        /// <paramref name="floorFraction"/> of max life. Returns the bite asked for (0 when there is none).
        /// </summary>
        public static float ApplyHazard(PlayerStats stats, float fraction, float floorFraction)
        {
            if (stats == null || !stats.IsAlive)
            {
                return 0f;
            }

            float bite = ProfundoHazardRules.Bite(stats.CurrentHealth, stats.MaxHealth, fraction, floorFraction);
            if (bite > 0f)
            {
                stats.ApplyHazardDamage(bite);
            }

            return bite;
        }

        // ------------------------------------------------------------------
        // The lamp network and the Tide of Darkness
        // ------------------------------------------------------------------

        private void TickLamps(Vector2 local, bool night)
        {
            Vector2[] lamps = OverworldLayouts.WallLamps;
            Vector2 plaza = OverworldLayouts.FortalezaUltimaLampadaOffset;
            float time = Time.time;
            int tide = ProfundoLampRules.TideIndex(time);
            ProfundoTidePhase phase = ProfundoLampRules.PhaseAt(time);
            float elapsed = ProfundoLampRules.ElapsedInPhase(time);
            bool inReach = Vector2.Distance(plaza, local) <= ProfundoLampRules.TideReach;

            if (phase == ProfundoTidePhase.Rising)
            {
                if (_snuffTide != tide)
                {
                    _snuffTide = tide;
                    _snuffsApplied = 0;
                }

                int due = ProfundoLampRules.SnuffsDue(elapsed, night);
                while (_snuffsApplied < due)
                {
                    _snuffsApplied++;
                    _network.SnuffOutermost();
                }
            }

            int ringsBefore = _network.RingsHeld;
            int here = ProfundoLampRules.LampAt(lamps, local);
            if (_network.Tend(here, Time.deltaTime))
            {
                Say(_network.RingsHeld > ringsBefore ? ProfundoMechanicsTexts.RingGrew : ProfundoMechanicsTexts.LampLit);
            }
            else if (here >= 0 && !_network.IsLit(here) && (here != _hintLamp || Time.unscaledTime >= _nextLampHint))
            {
                _nextLampHint = Time.unscaledTime + HintCooldown;
                Say(ProfundoMechanicsTexts.LampHint);
            }

            _hintLamp = here;
            int rings = _network.RingsHeld;
            if (inReach && phase == ProfundoTidePhase.Warning && _warnedTide != tide)
            {
                _warnedTide = tide;
                Say(ProfundoMechanicsTexts.TideWarning);
            }

            if (phase == ProfundoTidePhase.Rising && ProfundoLampRules.Exposed(lamps, _network, plaza, local))
            {
                BiteOfTheTide(tide, night);
            }

            if (_lastPhase == ProfundoTidePhase.Rising && phase == ProfundoTidePhase.Calm && inReach)
            {
                EndTide(rings);
            }

            _lastPhase = phase;
            ShowLamps(local, lamps, here);
            ShowTide(local, plaza, inReach, phase, elapsed, rings);
        }

        private void BiteOfTheTide(int tide, bool night)
        {
            if (_roseTide != tide)
            {
                // The first tick out of the light only warns, also for whoever walks in while the tide is already up.
                _roseTide = tide;
                _nextTideBite = Time.time + ProfundoLampRules.BiteTickSeconds;
                Say(ProfundoMechanicsTexts.TideRising);
                return;
            }

            if (Time.time < _nextTideBite)
            {
                return;
            }

            _nextTideBite = Time.time + ProfundoLampRules.BiteTickSeconds;
            if (ApplyHazard(_stats, ProfundoLampRules.BiteFractionAt(night), ProfundoLampRules.BiteFloorFraction) > 0f && _bittenTide != tide)
            {
                _bittenTide = tide;
                Say(ProfundoMechanicsTexts.TideBites);
            }
        }

        private void EndTide(int rings)
        {
            // The pay of the Maré de Treva (the first tide held each real hour) is the FinalCityEventsMechanicsDriver's.
            EventBus.Publish(new ProfundoTideEndedEvent(ProfundoLampRules.Held(rings) ? rings : 0));
            if (!ProfundoLampRules.Held(rings))
            {
                Say(ProfundoMechanicsTexts.TideFell);
                return;
            }

            if (_stats != null && _stats.IsAlive)
            {
                _stats.Heal(_stats.MaxHealth * ProfundoLampRules.HeldHealFraction);
            }

            Say(ProfundoMechanicsTexts.TideHeld);
        }

        private void ShowLamps(Vector2 local, Vector2[] lamps, int here)
        {
            for (int i = 0; i < lamps.Length; i++)
            {
                float distance = Vector2.Distance(local, lamps[i]);
                bool lit = _network.IsLit(i);
                Color color = i == here && !lit ? Color.Lerp(LampOutColor, LampTendColor, _network.Progress) : lit ? LampLitColor : LampOutColor;
                ShowRing(LampKeys + i, lamps[i], ProfundoLampRules.KindleReach, color, distance <= LampRingRange, SmallRingPoints);
                ShowLampLight(i, lamps[i], lit && distance <= LampLightRange);
            }
        }

        private void ShowLampLight(int index, Vector2 lamp, bool show)
        {
            Light light = _lampLights[index];
            if (!show)
            {
                if (light != null)
                {
                    light.enabled = false;
                }

                return;
            }

            if (light == null)
            {
                var go = new GameObject("Mx_WallLamp");
                go.transform.SetParent(transform, false);
                light = go.AddComponent<Light>();
                light.type = LightType.Point;
                light.color = LampLitColor;
                light.range = ProfundoLampRules.LampShelter * 1.6f;
                light.shadows = LightShadows.None;
                Vector2 world = _origin + lamp;
                go.transform.position = new Vector3(world.x, Ground(world) + 2.6f, world.y);
                _lampLights[index] = light;
            }

            light.enabled = true;
            light.intensity = 2.4f + Mathf.PerlinNoise(Time.time * 3f, index) * 1.2f;
        }

        private void ShowTide(Vector2 local, Vector2 plaza, bool inReach, ProfundoTidePhase phase, float elapsed, int rings)
        {
            bool alert = inReach && phase != ProfundoTidePhase.Calm;
            for (int i = 0; i <= ProfundoLampRules.RingCount; i++)
            {
                ShowRing(SafeKeys + i, plaza, ProfundoLampRules.SafeRadius(i), SafeColor, alert && i == rings, WideRingPoints);
            }

            if (!alert || phase != ProfundoTidePhase.Warning)
            {
                if (_front != null)
                {
                    _front.enabled = false;
                }

                return;
            }

            if (_front == null)
            {
                _front = NewLine("Mx_TideFront", 0.6f);
                _front.loop = true;
                _front.positionCount = WideRingPoints;
                _front.startColor = _front.endColor = FrontColor;
            }

            _front.enabled = true;
            if (Time.time < _nextFrontRefresh)
            {
                return;
            }

            _nextFrontRefresh = Time.time + FrontRefresh;
            float radius = ProfundoLampRules.FrontRadius(elapsed, ProfundoLampRules.SafeRadius(rings));
            for (int i = 0; i < WideRingPoints; i++)
            {
                float a = i * Mathf.PI * 2f / WideRingPoints;
                Vector2 world = _origin + plaza + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * radius;
                _front.SetPosition(i, new Vector3(world.x, Ground(world) + 0.3f, world.y));
            }
        }

        // ------------------------------------------------------------------
        // The floor that lights
        // ------------------------------------------------------------------

        private void TickFloor(Vector2 local)
        {
            if (!OverworldLayouts.FireFloorArea.Contains(local))
            {
                if (_onFloor)
                {
                    _onFloor = false;
                    HideFloor();
                }

                return;
            }

            if (!_onFloor)
            {
                _onFloor = true;
                Say(ProfundoMechanicsTexts.FloorEnter);
            }

            float time = Time.time;
            ProfundoFloorPhase phase = ProfundoFloorRules.PhaseAt(time);
            int pattern = ProfundoFloorRules.PatternAt(time);
            Vector2Int cell = ProfundoFloorRules.Cell(local);
            bool lit = ProfundoFloorRules.IsLit(pattern, cell);
            int cycle = Mathf.FloorToInt(time / ProfundoFloorRules.CycleSeconds);
            if (phase == ProfundoFloorPhase.Warning && lit && _warnedFloorCycle != cycle)
            {
                _warnedFloorCycle = cycle;
                Say(ProfundoMechanicsTexts.FloorWarning);
            }

            if (phase == ProfundoFloorPhase.Burning && lit && time >= _nextFloorBite)
            {
                _nextFloorBite = time + ProfundoFloorRules.BiteTickSeconds;
                if (ApplyHazard(_stats, ProfundoFloorRules.BiteFraction, ProfundoFloorRules.BiteFloorFraction) > 0f && Time.unscaledTime >= _nextFloorBurnSay)
                {
                    _nextFloorBurnSay = Time.unscaledTime + HintCooldown;
                    Say(ProfundoMechanicsTexts.FloorBurns);
                }
            }

            ShowFloor(cell, phase, pattern);
        }

        private void ShowFloor(Vector2Int center, ProfundoFloorPhase phase, int pattern)
        {
            if (phase == ProfundoFloorPhase.Quiet)
            {
                HideFloor();
                return;
            }

            Color color = phase == ProfundoFloorPhase.Warning ? TileWarnColor : TileBurnColor;
            int index = 0;
            for (int dy = -FloorSpan; dy <= FloorSpan; dy++)
            {
                for (int dx = -FloorSpan; dx <= FloorSpan; dx++, index++)
                {
                    var cell = new Vector2Int(center.x + dx, center.y + dy);
                    bool show = ProfundoFloorRules.IsLit(pattern, cell) && OverworldLayouts.FireFloorArea.Contains(ProfundoFloorRules.CellCenter(cell));
                    LineRenderer tile = _tiles[index];
                    if (!show)
                    {
                        if (tile != null)
                        {
                            tile.enabled = false;
                        }

                        continue;
                    }

                    if (tile == null)
                    {
                        tile = _tiles[index] = NewLine("Mx_FloorTile", 0.3f);
                        tile.positionCount = 6;
                    }

                    if (!_tileBuilt[index] || _tileCells[index] != cell)
                    {
                        _tileBuilt[index] = true;
                        _tileCells[index] = cell;
                        BuildTile(tile, cell);
                    }

                    tile.startColor = tile.endColor = color;
                    tile.enabled = true;
                }
            }
        }

        /// <summary>The outline of a tile and one diagonal, following the ground.</summary>
        private void BuildTile(LineRenderer tile, Vector2Int cell)
        {
            Vector2 min = _origin + new Vector2(cell.x, cell.y) * ProfundoFloorRules.TileSize;
            const float inset = 0.25f;
            float size = ProfundoFloorRules.TileSize - inset * 2f;
            Vector2 a = min + new Vector2(inset, inset);
            Vector2 b = a + new Vector2(size, 0f);
            Vector2 c = a + new Vector2(size, size);
            Vector2 d = a + new Vector2(0f, size);
            Vector2[] path = { a, b, c, d, a, c };
            for (int i = 0; i < path.Length; i++)
            {
                tile.SetPosition(i, new Vector3(path[i].x, Ground(path[i]) + 0.2f, path[i].y));
            }
        }

        private void HideFloor()
        {
            foreach (LineRenderer tile in _tiles)
            {
                if (tile != null)
                {
                    tile.enabled = false;
                }
            }
        }

        // ------------------------------------------------------------------
        // The wind that clears the smoke
        // ------------------------------------------------------------------

        private void HandleSkillCast(SkillCastEvent evt)
        {
            if (_clearLeft == null || !_inRegion || evt.Skill == null || evt.Skill.Element != ElementType.Wind || _player == null || evt.Caster == null
                || (evt.Caster != _player.gameObject && !evt.Caster.transform.IsChildOf(_player)))
            {
                return;
            }

            Vector2 target = new Vector2(evt.TargetPoint.x, evt.TargetPoint.z) - _origin;
            Vector2 caster = new Vector2(_player.position.x, _player.position.z) - _origin;
            ProfundoSmokeBank[] banks = OverworldLayouts.SmokeBanks;
            bool cleared = false;
            for (int i = 0; i < banks.Length; i++)
            {
                if (ProfundoSmokeRules.WindReaches(banks[i], target, evt.Skill.AreaRadius) || ProfundoSmokeRules.WindReaches(banks[i], caster, 0f))
                {
                    _clearLeft[i] = ProfundoSmokeRules.ClearSeconds;
                    cleared = true;
                }
            }

            if (cleared)
            {
                Say(ProfundoMechanicsTexts.SmokeCleared);
            }
        }

        private void TickSmoke(Vector2 local)
        {
            ProfundoSmokeBank[] banks = OverworldLayouts.SmokeBanks;
            for (int i = 0; i < banks.Length; i++)
            {
                float before = _clearLeft[i];
                _clearLeft[i] = ProfundoSmokeRules.Advance(before, Time.deltaTime);
                if (before > SmokeReturnWarning && _clearLeft[i] <= SmokeReturnWarning && Vector2.Distance(local, banks[i].Center) <= banks[i].Radius + BankRingRange)
                {
                    Say(ProfundoMechanicsTexts.SmokeReturns);
                }
            }

            TickVent(local, banks);
            int here = ProfundoSmokeRules.SmokeAt(banks, _clearLeft, local);
            if (here >= 0 && here != _bankHere && Time.unscaledTime >= _nextSmokeHint)
            {
                _nextSmokeHint = Time.unscaledTime + HintCooldown;
                Say(ProfundoMechanicsTexts.SmokeEnter);
            }

            if (here >= 0 && _bankHere < 0)
            {
                _nextSmokeBite = Time.time + ProfundoSmokeRules.BiteTickSeconds;
            }

            _bankHere = here;
            SetSmokeEffect(here >= 0);
            if (here >= 0 && Time.time >= _nextSmokeBite)
            {
                _nextSmokeBite = Time.time + ProfundoSmokeRules.BiteTickSeconds;
                ApplyHazard(_stats, ProfundoSmokeRules.BiteFraction, ProfundoSmokeRules.BiteFloorFraction);
            }

            for (int i = 0; i < banks.Length; i++)
            {
                bool near = Vector2.Distance(local, banks[i].Center) - banks[i].Radius <= BankRingRange;
                ShowRing(BankKeys + i, banks[i].Center, banks[i].Radius, ProfundoSmokeRules.IsClear(_clearLeft[i]) ? ClearColor : SmokeColor, near, WideRingPoints);
                ShowRing(VentKeys + i, banks[i].Vent, ProfundoSmokeRules.VentReach, ProfundoSmokeRules.IsClear(_clearLeft[i]) ? ClearColor : VentColor, near, SmallRingPoints);
            }
        }

        private void TickVent(Vector2 local, ProfundoSmokeBank[] banks)
        {
            int vent = ProfundoSmokeRules.VentAt(banks, local);
            if (vent < 0 || ProfundoSmokeRules.IsClear(_clearLeft[vent]))
            {
                _ventHere = vent;
                _ventProgress = 0f;
                return;
            }

            if (vent != _ventHere)
            {
                _ventHere = vent;
                _ventProgress = 0f;
                Say(ProfundoMechanicsTexts.VentHint);
            }

            _ventProgress += Time.deltaTime;
            if (_ventProgress >= ProfundoSmokeRules.VentSeconds)
            {
                _ventProgress = 0f;
                _clearLeft[vent] = ProfundoSmokeRules.ClearSeconds;
                Say(ProfundoMechanicsTexts.SmokeCleared);
            }
        }

        private void SetSmokeEffect(bool inSmoke)
        {
            if (_buffs == null)
            {
                return;
            }

            if (!inSmoke)
            {
                ClearSmokeEffect();
                return;
            }

            if (Time.time >= _nextSmokeRefresh)
            {
                _nextSmokeRefresh = Time.time + EffectRefresh;
                _buffs.ApplyEffect(_smokeSlow, SmokeSource);
                _smokeApplied = true;
            }
        }

        private void ClearSmokeEffect()
        {
            if (_smokeApplied && _buffs != null)
            {
                _buffs.RemoveEffectsBySource(SmokeSource);
            }

            _smokeApplied = false;
            _nextSmokeRefresh = 0f;
        }

        // ------------------------------------------------------------------
        // Feedback
        // ------------------------------------------------------------------

        private float Ground(Vector2 world) => OverworldRules.SurfaceAt(_layout, world);

        private void ShowRing(int key, Vector2 center, float radius, Color color, bool show, int points)
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
                ring.positionCount = points;
                for (int i = 0; i < points; i++)
                {
                    float a = i * Mathf.PI * 2f / points;
                    Vector2 world = _origin + center + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * radius;
                    ring.SetPosition(i, new Vector3(world.x, Ground(world) + 0.2f, world.y));
                }

                _rings[key] = ring;
            }

            ring.startColor = ring.endColor = color;
            ring.enabled = true;
        }

        private LineRenderer NewLine(string objectName, float width)
        {
            if (_material == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit") ?? Shader.Find("Particles/Standard Unlit") ?? Shader.Find("Sprites/Default");
                _material = new Material(shader) { name = "Mat_ProfundoLine_Runtime" };
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
            CombatFxManager.SpawnLootWarningText(_player.position + Vector3.up * FeedbackHeight, ProfundoMechanicsTexts.Get(localization, key));
        }
    }
}
