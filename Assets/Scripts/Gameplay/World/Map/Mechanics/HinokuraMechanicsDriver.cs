using System;
using System.Collections.Generic;
using Rapadura.Core.DI;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Combat;
using Rapadura.Gameplay.Cooking;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.Skills;
using Rapadura.Gameplay.World.Map.Overworld;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// Scene side of the Arquipélago de Hinokura mechanics (the rules are <see cref="HinokuraForgeRules"/>, <see cref="HinokuraHeatRules"/> and
    /// <see cref="HinokuraEruptionRules"/>): added to the overworld root, idle unless the layout holds the region and the hero stands in it.
    /// The driver never takes the controls: the heat and the eruption only take health, down to their floors, after their warnings.
    /// The moment each forge was lit is saved per slot in <see cref="WorldMapProgress"/> (a server would keep the same three numbers); with the UTC clock, a forge survives a reload.
    /// </summary>
    public sealed class HinokuraMechanicsDriver : MonoBehaviour, IRegionMechanics
    {
        private const string PlayerTag = "Player";
        private const float HintCooldown = 3f;
        private const float DeniedCooldown = 1.5f;
        private const float FeedbackHeight = 2.4f;
        private const float SluiceViewRange = 220f;
        private const float StoneViewRange = 140f;
        private const float RouteViewRange = 340f;
        private const float GluttonHintRange = 90f;
        private const float GaugeSeconds = 6f;
        private const float BeaconLitHeight = 24f;
        private const float BeaconOutHeight = 6f;
        private const float StoneBeaconHeight = 9f;
        private const float HeatRingRadius = 1.2f;
        private const int HeatRingPoints = 17;
        private const int RouteSegments = 12;

        private static readonly string[] ForgeMarkKeys = { "hinokura.forge.0", "hinokura.forge.1", "hinokura.forge.2" };

        private readonly long[] _litAt = { HinokuraForgeRules.NeverLit, HinokuraForgeRules.NeverLit, HinokuraForgeRules.NeverLit };
        private readonly List<LineRenderer> _sluiceBeacons = new List<LineRenderer>();
        private readonly List<LineRenderer> _stoneBeacons = new List<LineRenderer>();
        private readonly StaticGroundCache _ground = new StaticGroundCache();

        private OverworldView _view;
        private OverworldLayout _layout;
        private Vector2 _origin;
        private Transform _player;
        private PlayerController _controller;
        private PlayerStats _stats;
        private Material _material;
        private LineRenderer _heatRing;
        private LineRenderer _routeLine;

        private float _nextHint;
        private float _nextDenied;
        private int _hintSluice = -1;
        private long _warnedTremorCycle = long.MinValue;
        private long _seenTremorCycle = long.MinValue;

        private float _heat;
        private float _burnTick;
        private float _nextGauge;
        private HinokuraHeatBand _spokenBand;
        private MealService _meals;

        private HinokuraEruptionPhase _spokenPhase = HinokuraEruptionPhase.Calm;
        private float _eruptionTick;

        private float _pulseTimer;
        private bool _saidFed;

        private Rect[] _areas;

        /// <inheritdoc/>
        public Rect[] MechanicsAreas => _areas;

        /// <inheritdoc/>
        public bool MechanicsBusy => false;

        private void Start()
        {
            _view = GetComponent<OverworldView>();
            _layout = _view != null ? _view.Layout : null;
            OverworldZone anchor = _layout?.Zone(OverworldLayouts.HinokuraAnchorZoneId);
            if (anchor == null)
            {
                enabled = false;
                return;
            }

            _origin = anchor.Origin;
            _areas = new[] { MechanicsDriverHub.WorldArea(OverworldLayouts.HinokuraBounds, _origin) };
        }

        private void OnDisable() => HideFeedback();

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

            Vector2 local = new Vector2(_player.position.x, _player.position.z) - _origin;
            if (!OverworldLayouts.HinokuraBounds.Contains(local))
            {
                HideFeedback();
                return;
            }

            long unix = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            if (ServiceLocator.TryGet(out WorldMapProgress progress))
            {
                ReadForges(progress, _litAt);
            }

            TickTremor(unix);
            TickSluices(local, unix);
            TickGlutton(local, unix);
            TickHeat(local);
            TickEruption(local, unix);
        }

        /// <summary>Copies the saved moments into <paramref name="litAt"/> (a forge with no mark is <see cref="HinokuraForgeRules.NeverLit"/>).</summary>
        public static void ReadForges(WorldMapProgress progress, long[] litAt)
        {
            for (int i = 0; i < litAt.Length && i < ForgeMarkKeys.Length; i++)
            {
                litAt[i] = progress.GetTimeMark(ForgeMarkKeys[i], HinokuraForgeRules.NeverLit);
            }
        }

        public static void StoreForge(WorldMapProgress progress, int forge, long unix) => progress?.SetTimeMark(ForgeMarkKeys[forge], unix);

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
            }

            return _controller != null && _stats != null;
        }

        // ------------------------------------------------------------------
        // The Tremor and the sluices
        // ------------------------------------------------------------------

        private void TickTremor(long unix)
        {
            long cycle = HinokuraForgeRules.TremorCycle(unix);
            if (HinokuraForgeRules.IsTremorWarning(unix) && _warnedTremorCycle != cycle)
            {
                _warnedTremorCycle = cycle;
                ServiceLocator.TryGet(out LocalizationManager localization);
                SayText(HinokuraMechanicsTexts.Format(localization, HinokuraMechanicsTexts.TremorWarn, HinokuraForgeRules.SecondsToNextTremor(unix)));
            }

            if (_seenTremorCycle == cycle)
            {
                return;
            }

            bool firstLook = _seenTremorCycle == long.MinValue;
            _seenTremorCycle = cycle;
            int struck = HinokuraForgeRules.StruckForge(cycle);

            // Only a forge this Tremor really put out is news: one already cold says nothing.
            long tremorAt = cycle * HinokuraForgeRules.TremorPeriodSeconds;
            if (!firstLook && HinokuraForgeRules.IsLit(struck, _litAt[struck], tremorAt - 1L))
            {
                ServiceLocator.TryGet(out LocalizationManager localization);
                SayText(HinokuraMechanicsTexts.Format(localization, HinokuraMechanicsTexts.TremorHit,
                    HinokuraMechanicsTexts.Get(localization, HinokuraMechanicsTexts.ForgeNames[struck])));
            }
        }

        private void TickSluices(Vector2 local, long unix)
        {
            ShowSluices(local, unix);
            int sluice = NearestSluice(local);
            if (sluice < 0)
            {
                _hintSluice = -1;
                return;
            }

            HinokuraSluiceCheck check = HinokuraForgeRules.Check(sluice, _litAt[sluice], unix);
            if (_hintSluice != sluice || Time.unscaledTime >= _nextHint)
            {
                _hintSluice = sluice;
                _nextHint = Time.unscaledTime + HintCooldown;
                SaySluice(check, sluice, unix);
            }

            InteractPromptRegistry.Ping(Time.unscaledTime);
            if (_controller.Input == null || !_controller.Input.InteractPressedThisFrame)
            {
                return;
            }

            if (check == HinokuraSluiceCheck.Open)
            {
                _litAt[sluice] = unix;
                StoreForge(ServiceLocator.TryGet(out WorldMapProgress progress) ? progress : null, sluice, unix);
                _hintSluice = -1;
                ServiceLocator.TryGet(out LocalizationManager localization);
                SayText(HinokuraMechanicsTexts.Format(localization, HinokuraMechanicsTexts.ForgeLit, HinokuraForgeRules.LitCount(_litAt, unix)));
            }
            else if (Time.unscaledTime >= _nextDenied)
            {
                _nextDenied = Time.unscaledTime + DeniedCooldown;
                SaySluice(check, sluice, unix);
            }
        }

        private static int NearestSluice(Vector2 local)
        {
            Vector2[] sluices = OverworldLayouts.ForgeSluices;
            int best = -1;
            float bestSqr = HinokuraForgeRules.SluiceReach * HinokuraForgeRules.SluiceReach;
            for (int i = 0; i < sluices.Length; i++)
            {
                float sqr = (sluices[i] - local).sqrMagnitude;
                if (sqr <= bestSqr)
                {
                    bestSqr = sqr;
                    best = i;
                }
            }

            return best;
        }

        private void SaySluice(HinokuraSluiceCheck check, int sluice, long unix)
        {
            ServiceLocator.TryGet(out LocalizationManager localization);
            string forge = HinokuraMechanicsTexts.Get(localization, HinokuraMechanicsTexts.ForgeNames[sluice]);
            switch (check)
            {
                case HinokuraSluiceCheck.Open:
                    SayText(HinokuraMechanicsTexts.Format(localization, HinokuraMechanicsTexts.SluiceOpen, forge));
                    break;
                case HinokuraSluiceCheck.AlreadyLit:
                    long left = HinokuraForgeRules.GoesOutAt(sluice, _litAt[sluice]) - unix;
                    SayText(HinokuraMechanicsTexts.Format(localization, HinokuraMechanicsTexts.SluiceLit, forge, Mathf.CeilToInt(left / 60f)));
                    break;
                default:
                    SayText(HinokuraMechanicsTexts.Format(localization, HinokuraMechanicsTexts.SluiceJammed, HinokuraForgeRules.JamSecondsLeft(unix)));
                    break;
            }
        }

        // ------------------------------------------------------------------
        // The Glutão recoils from the lit forges
        // ------------------------------------------------------------------

        private void TickGlutton(Vector2 local, long unix)
        {
            bool nearArena = (local - OverworldLayouts.GlutaoArena).sqrMagnitude < GluttonHintRange * GluttonHintRange;
            EnemyController glutton = nearArena ? FindGlutton() : null;
            if (glutton == null)
            {
                _saidFed = false;
                _pulseTimer = 0f;
                return;
            }

            int lit = HinokuraForgeRules.LitCount(_litAt, unix);
            if (lit == 0)
            {
                _pulseTimer = 0f;
                if (!_saidFed)
                {
                    _saidFed = true;
                    Say(HinokuraMechanicsTexts.GluttonFed);
                }

                return;
            }

            _saidFed = false;
            _pulseTimer += Time.deltaTime;
            if (_pulseTimer < HinokuraForgeRules.PulseSeconds)
            {
                return;
            }

            _pulseTimer = 0f;
            float damage = HinokuraForgeRules.PulseDamage(glutton.Health.CurrentHealth, glutton.Health.MaxHealth, lit);
            if (damage > 0f)
            {
                glutton.Health.ApplyDamage(damage, ElementType.Physical, null);
            }

            if (glutton.Buffs != null)
            {
                glutton.Buffs.ApplyStun(HinokuraForgeRules.StaggerSeconds(lit));
            }

            ServiceLocator.TryGet(out LocalizationManager localization);
            SayText(HinokuraMechanicsTexts.Format(localization, HinokuraMechanicsTexts.GluttonRecoils, lit));
        }

        private static EnemyController FindGlutton()
        {
            foreach (EnemyController enemy in EnemyController.EnabledEnemies)
            {
                if (enemy != null && enemy.Definition != null && enemy.Definition.name == RegionBossRules.EmberGluttonId && enemy.Health != null && enemy.Health.IsAlive)
                {
                    return enemy;
                }
            }

            return null;
        }

        // ------------------------------------------------------------------
        // The heat of the caldera
        // ------------------------------------------------------------------

        private void TickHeat(Vector2 local)
        {
            ShowStones(local);
            if (!_stats.IsAlive)
            {
                ResetHeat();
                return;
            }

            float distance = RioFireRules.DistanceToFire(OverworldLayouts.CaldeiraLake, local);
            bool onStone = OnColdStone(local);
            float before = _heat;
            _heat = HinokuraHeatRules.Step(_heat, distance, onStone, TeaActive(), Time.deltaTime);
            if (onStone && before >= HinokuraHeatRules.WarmAt)
            {
                Say(HinokuraMechanicsTexts.HeatCooled);
            }

            SpeakHeat();
            ShowHeatRing();

            _burnTick += Time.deltaTime;
            if (_burnTick < 1f)
            {
                return;
            }

            float seconds = _burnTick;
            _burnTick = 0f;
            float burn = HinokuraHeatRules.BurnAmount(_heat, _stats.CurrentHealth, _stats.MaxHealth, seconds);
            if (burn > 0f)
            {
                _stats.ApplyHazardDamage(burn);
            }
        }

        private static bool OnColdStone(Vector2 local)
        {
            foreach (Vector2 stone in OverworldLayouts.CaldeiraColdStones)
            {
                if ((stone - local).sqrMagnitude <= HinokuraHeatRules.ColdStoneRadius * HinokuraHeatRules.ColdStoneRadius)
                {
                    return true;
                }
            }

            return false;
        }

        private bool TeaActive()
        {
            _meals ??= MealService.ResolveShared();
            return _meals != null && _meals.ActiveRecipe != null && _meals.ActiveRecipe.RecipeId == MealCatalog.CafeCoado;
        }

        private void SpeakHeat()
        {
            HinokuraHeatBand band = HinokuraHeatRules.BandOf(_heat);
            if (band > _spokenBand)
            {
                Say(band == HinokuraHeatBand.Scorching ? HinokuraMechanicsTexts.HeatScorching
                    : band == HinokuraHeatBand.Danger ? HinokuraMechanicsTexts.HeatDanger : HinokuraMechanicsTexts.HeatWarm);
                _nextGauge = Time.unscaledTime + GaugeSeconds;
            }
            else if (band != HinokuraHeatBand.Cool && Time.unscaledTime >= _nextGauge)
            {
                _nextGauge = Time.unscaledTime + GaugeSeconds;
                ServiceLocator.TryGet(out LocalizationManager localization);
                SayText(HinokuraMechanicsTexts.Format(localization, HinokuraMechanicsTexts.HeatGauge, Mathf.RoundToInt(_heat)));
            }

            _spokenBand = band;
        }

        private void ResetHeat()
        {
            _heat = 0f;
            _burnTick = 0f;
            _spokenBand = HinokuraHeatBand.Cool;
            ShowHeatRing();
        }

        // ------------------------------------------------------------------
        // The eruption of Echeyde
        // ------------------------------------------------------------------

        private void TickEruption(Vector2 local, long unix)
        {
            HinokuraEruptionPhase phase = HinokuraEruptionRules.PhaseAt(unix);
            int route = HinokuraEruptionRules.RouteAt(unix);
            bool near = (local - OverworldLayouts.EcheydeCenter).sqrMagnitude < RouteViewRange * RouteViewRange;
            ShowRoute(near && phase != HinokuraEruptionPhase.Calm, route, phase);
            if (!near)
            {
                _spokenPhase = phase;
                _eruptionTick = 0f;
                return;
            }

            SpeakEruption(phase, unix);
            if (phase != HinokuraEruptionPhase.Erupting || !_stats.IsAlive)
            {
                _eruptionTick = 0f;
                return;
            }

            _eruptionTick += Time.deltaTime;
            if (_eruptionTick < 1f)
            {
                return;
            }

            float seconds = _eruptionTick;
            _eruptionTick = 0f;
            bool inRoute = HinokuraEruptionRules.InRoute(OverworldLayouts.EruptionRoutes, route, local);
            float damage = HinokuraEruptionRules.DamageAmount(phase, inRoute, _stats.CurrentHealth, _stats.MaxHealth, seconds);
            if (damage > 0f)
            {
                _stats.ApplyHazardDamage(damage);
            }
        }

        private void SpeakEruption(HinokuraEruptionPhase phase, long unix)
        {
            if (phase == _spokenPhase)
            {
                return;
            }

            HinokuraEruptionPhase previous = _spokenPhase;
            _spokenPhase = phase;
            switch (phase)
            {
                case HinokuraEruptionPhase.Warning:
                    ServiceLocator.TryGet(out LocalizationManager localization);
                    SayText(HinokuraMechanicsTexts.Format(localization, HinokuraMechanicsTexts.EruptionHorn, HinokuraEruptionRules.SecondsToChange(unix)));
                    break;
                case HinokuraEruptionPhase.Erupting:
                    Say(HinokuraMechanicsTexts.Erupting);
                    break;
                default:
                    if (previous == HinokuraEruptionPhase.Erupting)
                    {
                        Say(HinokuraMechanicsTexts.EruptionOver);
                    }

                    break;
            }
        }

        // ------------------------------------------------------------------
        // Feedback
        // ------------------------------------------------------------------

        private void ShowSluices(Vector2 local, long unix)
        {
            Vector2[] sluices = OverworldLayouts.ForgeSluices;
            if (_sluiceBeacons.Count == 0)
            {
                foreach (Vector2 sluice in sluices)
                {
                    LineRenderer beacon = NewLine("Hinokura_SluiceBeacon", 0.5f);
                    beacon.SetPosition(0, Ground(sluice));
                    _sluiceBeacons.Add(beacon);
                }
            }

            for (int i = 0; i < sluices.Length; i++)
            {
                LineRenderer beacon = _sluiceBeacons[i];
                beacon.enabled = (sluices[i] - local).sqrMagnitude < SluiceViewRange * SluiceViewRange;
                bool lit = HinokuraForgeRules.IsLit(i, _litAt[i], unix);
                beacon.startColor = beacon.endColor = lit ? new Color(1f, 0.54f, 0.24f, 1f) : new Color(0.45f, 0.42f, 0.42f, 0.85f);
                beacon.SetPosition(1, beacon.GetPosition(0) + Vector3.up * (lit ? BeaconLitHeight : BeaconOutHeight));
            }
        }

        private void ShowStones(Vector2 local)
        {
            Vector2[] stones = OverworldLayouts.CaldeiraColdStones;
            if (_stoneBeacons.Count == 0)
            {
                foreach (Vector2 stone in stones)
                {
                    LineRenderer beacon = NewLine("Hinokura_ColdStone", 0.4f);
                    Vector3 foot = Ground(stone);
                    beacon.startColor = beacon.endColor = new Color(0.55f, 0.85f, 1f, 1f);
                    beacon.SetPosition(0, foot);
                    beacon.SetPosition(1, foot + Vector3.up * StoneBeaconHeight);
                    _stoneBeacons.Add(beacon);
                }
            }

            for (int i = 0; i < stones.Length; i++)
            {
                _stoneBeacons[i].enabled = (stones[i] - local).sqrMagnitude < StoneViewRange * StoneViewRange;
            }
        }

        private void ShowHeatRing()
        {
            if (_heat <= 0f)
            {
                if (_heatRing != null)
                {
                    _heatRing.enabled = false;
                }

                return;
            }

            if (_heatRing == null)
            {
                _heatRing = NewLine("Hinokura_HeatRing", 0.22f);
                _heatRing.positionCount = HeatRingPoints;
            }

            _heatRing.enabled = true;
            float fill = _heat / HinokuraHeatRules.MaxHeat;
            _heatRing.startColor = _heatRing.endColor = Color.Lerp(new Color(1f, 0.82f, 0.35f, 0.7f), new Color(1f, 0.15f, 0.08f, 1f), fill);
            float radius = HeatRingRadius * (0.6f + fill);
            for (int i = 0; i < HeatRingPoints; i++)
            {
                float angle = i * Mathf.PI / 8f + Time.time * 2f;
                _heatRing.SetPosition(i, _player.position + new Vector3(Mathf.Cos(angle) * radius, 0.15f, Mathf.Sin(angle) * radius));
            }
        }

        private void ShowRoute(bool show, int route, HinokuraEruptionPhase phase)
        {
            if (!show)
            {
                if (_routeLine != null)
                {
                    _routeLine.enabled = false;
                }

                return;
            }

            (Vector2 a, Vector2 b, float width) = OverworldLayouts.EruptionRoutes[route];
            if (_routeLine == null)
            {
                _routeLine = NewLine("Hinokura_EruptionRoute", width);
                _routeLine.positionCount = RouteSegments + 1;
            }

            _routeLine.enabled = true;
            _routeLine.startWidth = _routeLine.endWidth = width;
            bool erupting = phase == HinokuraEruptionPhase.Erupting;
            float blink = erupting ? 1f : 0.45f + 0.35f * Mathf.PingPong(Time.time * 2f, 1f);
            _routeLine.startColor = _routeLine.endColor = erupting ? new Color(1f, 0.3f, 0.05f, 0.95f) : new Color(1f, 0.78f, 0.2f, blink);
            for (int i = 0; i <= RouteSegments; i++)
            {
                _routeLine.SetPosition(i, Ground(Vector2.Lerp(a, b, i / (float)RouteSegments)) + Vector3.up * 0.6f);
            }
        }

        private void HideFeedback()
        {
            foreach (LineRenderer beacon in _sluiceBeacons)
            {
                if (beacon != null)
                {
                    beacon.enabled = false;
                }
            }

            foreach (LineRenderer beacon in _stoneBeacons)
            {
                if (beacon != null)
                {
                    beacon.enabled = false;
                }
            }

            ResetHeat();
            ShowRoute(false, 0, HinokuraEruptionPhase.Calm);
            _hintSluice = -1;
            _saidFed = false;
            _pulseTimer = 0f;
            _spokenPhase = HinokuraEruptionPhase.Calm;
            _eruptionTick = 0f;
        }

        /// <summary>The ground under a fixed point (sluices, stones, the 13 points of each eruption route), sampled once per point.</summary>
        private Vector3 Ground(Vector2 local)
        {
            Vector2 world = _origin + local;
            return new Vector3(world.x, _ground.GroundAt(_layout, world), world.y);
        }

        private LineRenderer NewLine(string objectName, float width)
        {
            if (_material == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit") ?? Shader.Find("Particles/Standard Unlit") ?? Shader.Find("Sprites/Default");
                _material = new Material(shader) { name = "Mat_HinokuraLine_Runtime" };
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
            SayText(HinokuraMechanicsTexts.Get(localization, key));
        }

        private void SayText(string text) => CombatFxManager.SpawnLootWarningText(_player.position + Vector3.up * FeedbackHeight, text);
    }
}
