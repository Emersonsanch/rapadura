using System;
using System.Collections.Generic;
using Rapadura.Core.DI;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Combat;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Environment;
using Rapadura.Gameplay.Inventory;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.Skills;
using Rapadura.Gameplay.World.Cities.Town;
using Rapadura.Gameplay.World.Map.Areas;
using Rapadura.Gameplay.World.Map.Overworld;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// Scene side of the timed descent of the Calhas de Pedra (37, <see cref="TetoCartRules"/>): [E] at the top station boards the cart, which
    /// takes the controls (<see cref="MechanicsRide"/>) down <see cref="OverworldLayouts.TetoCartCourse"/>; the move input steers it sideways
    /// (<see cref="MechanicsRide.Lateral"/>) to catch the sparks, and the cart knocks the beasts it runs into. The controls come back at the foot,
    /// on death, if anything else moves the hero and in OnDisable. The best score and the day the run paid stay in the save.
    /// </summary>
    public sealed class TetoCartMechanicsDriver : MonoBehaviour, IRegionMechanics
    {
        private const string PlayerTag = "Player";
        private const string NuggetItemId = "item_gold";
        private const float HintCooldown = 3f;
        private const float DeniedCooldown = 1.5f;
        private const float FeedbackHeight = 2.4f;
        private const float ViewRange = 140f;
        private const float StationHeight = 7f;
        private const float SparkHeight = 1.6f;
        private const float SparkSize = 0.9f;

        /// <summary>The chutes of map 37 (region coordinates): where the station and the course are shown and [E] is read.</summary>
        public static readonly Rect ChutesBounds = Rect.MinMaxRect(240f, -160f, 760f, 840f);

        private static readonly Color StationColor = new Color(1f, 0.78f, 0.35f, 1f);
        private static readonly Color StationIdleColor = new Color(0.5f, 0.45f, 0.4f, 0.7f);
        private static readonly Color SparkColor = new Color(1f, 0.9f, 0.45f, 1f);

        private readonly HeroRequirementCache _heroState = new HeroRequirementCache();
        private readonly StaticGroundCache _ground = new StaticGroundCache();
        private readonly MechanicsRide _ride = new MechanicsRide();
        private readonly HashSet<EnemyController> _knocked = new HashSet<EnemyController>();
        private bool[] _caught;

        private OverworldView _view;
        private OverworldLayout _layout;
        private Vector2 _origin;
        private Vector2[] _courseWorld;
        private float _courseLength;
        private Transform _player;
        private PlayerController _controller;
        private PlayerStats _stats;
        private InventoryManager _bag;
        private Material _material;
        private LineRenderer _station;
        private LineRenderer _foot;
        private LineRenderer[] _sparkLines;

        private float _nextHint;
        private float _nextDenied;
        private bool _hinting;
        private float _lastRun = float.NegativeInfinity;
        private bool _riding;
        private bool _night;
        private float _lastDistance;
        private long _bestLocal;
        private int _paidLocal;

        private Rect[] _areas;

        /// <inheritdoc/>
        public Rect[] MechanicsAreas => _areas;

        /// <inheritdoc/>
        public bool MechanicsBusy => _ride.Active;

        private void Start()
        {
            _view = GetComponent<OverworldView>();
            _layout = _view != null ? _view.Layout : null;
            OverworldZone anchor = _layout?.Zone(OverworldLayouts.TetoAnchorZoneId);
            if (anchor == null)
            {
                enabled = false;
                return;
            }

            _origin = anchor.Origin;
            // The hub keeps the driver on over the whole region (like the rafts' driver): Update itself idles outside the chutes.
            _areas = new[] { MechanicsDriverHub.WorldArea(OverworldLayouts.TetoBounds, _origin) };
            _courseWorld = RidePath.ToWorld(OverworldLayouts.TetoCartCourse, _origin, false);
            _courseLength = RidePath.Length(_courseWorld);
            _caught = new bool[TetoCartRules.Sparks.Length];
            _sparkLines = new LineRenderer[TetoCartRules.Sparks.Length];
        }

        private void OnEnable() => _heroState.Attach();

        private void OnDisable()
        {
            EndRun(false);
            HideAll();
            _hinting = false;
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
                TickRun();
                return;
            }

            Vector2 local = new Vector2(_player.position.x, _player.position.z) - _origin;
            if (!ChutesBounds.Contains(local))
            {
                HideAll();
                _hinting = false;
                return;
            }

            ShowStations(local);
            if (_stats != null && !_stats.IsAlive)
            {
                return;
            }

            TickStation(local);
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

        private static double NowExact() => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / 1000d;

        private static int Today() => (int)NartesRaceRules.DayOf(DateTimeOffset.UtcNow.ToUnixTimeSeconds());

        private static bool IsNight() => ServiceLocator.TryGet(out TimeOfDayManager clock) && clock.Phase == TimeOfDayPhase.Night;

        private long BestMs() => ServiceLocator.TryGet(out WorldMapProgress progress) ? progress.GetTimeMark(TetoCartRules.BestKey) : _bestLocal;

        private int PaidDay() => ServiceLocator.TryGet(out WorldMapProgress progress) ? progress.GetDailyMark(TetoCartRules.PaidKey) : _paidLocal;

        // ------------------------------------------------------------------
        // The station
        // ------------------------------------------------------------------

        private void TickStation(Vector2 local)
        {
            if (!TetoCartRules.AtStation(OverworldLayouts.TetoCartTop, local) || _controller.Input == null)
            {
                _hinting = false;
                return;
            }

            bool night = IsNight();
            float runSeconds = TetoCartRules.RunSeconds(_courseLength, night);
            double now = NowExact();
            MapRequirementState state = _heroState.Get(_bag);
            MapModeController mode = MapModeController.Current;
            int level = mode != null ? mode.HeroLevel : 1;
            var route = new SkyRoute { Id = "carrinho_das_calhas", Kind = SkyRouteKind.Glide, A = OverworldLayouts.TetoCartTop, B = OverworldLayouts.TetoCartFoot };
            bool zones = SkyRouteRules.AllZonesOpen(_layout, _origin, route, level, state.Hero, _heroState.Lookup);
            TetoCartCheck check = TetoCartRules.Check(zones, TetoCartRules.AvalancheAllows(now, runSeconds), Time.unscaledTime - _lastRun);
            if (!_hinting || Time.unscaledTime >= _nextHint)
            {
                _hinting = true;
                _nextHint = Time.unscaledTime + HintCooldown;
                SayStation(check, now);
            }

            if (check == TetoCartCheck.Ride)
            {
                InteractPromptRegistry.Ping(Time.unscaledTime);
            }

            if (!_controller.Input.InteractPressedThisFrame)
            {
                return;
            }

            if (check == TetoCartCheck.Ride)
            {
                BeginRun(night);
            }
            else if (Time.unscaledTime >= _nextDenied)
            {
                _nextDenied = Time.unscaledTime + DeniedCooldown;
                SayStation(check, now);
            }
        }

        private void SayStation(TetoCartCheck check, double now)
        {
            ServiceLocator.TryGet(out LocalizationManager localization);
            switch (check)
            {
                case TetoCartCheck.Ride:
                    SayText(TetoCartTexts.Get(localization, TetoCartTexts.StationRide));
                    long best = BestMs();
                    if (best > 0L)
                    {
                        SayText(TetoCartTexts.Format(localization, TetoCartTexts.StationRecord, (best / 1000f).ToString("0.0", System.Globalization.CultureInfo.InvariantCulture)), 0.7f);
                    }

                    break;
                case TetoCartCheck.Avalanche:
                    double second = now - TetoAvalancheRules.CycleOf(now) * TetoAvalancheRules.CycleSeconds;
                    int wait = Mathf.Max(1, Mathf.CeilToInt((float)(TetoAvalancheRules.SlidingEnd - second)));
                    SayText(TetoCartTexts.Format(localization, TetoCartTexts.StationAvalanche, wait));
                    break;
                case TetoCartCheck.ZoneBlocked:
                    SayText(TetoCartTexts.Get(localization, TetoCartTexts.StationBlocked));
                    break;
                default:
                    SayText(TetoCartTexts.Get(localization, TetoCartTexts.StationResting));
                    break;
            }
        }

        // ------------------------------------------------------------------
        // The run
        // ------------------------------------------------------------------

        private void BeginRun(bool night)
        {
            if (!_ride.Begin(_player.gameObject, _layout, _courseWorld, TetoCartRules.Speed(night), 0f, TetoCartRules.Hover, TetoCartRules.DropSpeed))
            {
                return;
            }

            _riding = true;
            _night = night;
            _lastDistance = 0f;
            _knocked.Clear();
            Array.Clear(_caught, 0, _caught.Length);
            _hinting = false;
            Say(TetoCartTexts.Started);
        }

        private void TickRun()
        {
            _ride.Lateral = TetoCartRules.Steer(_ride.Lateral, SteerInput(), Time.deltaTime);
            RideStep step = _ride.Tick(Time.deltaTime);
            float distance = _ride.Progress * _courseLength;
            CatchSparks(distance);
            _lastDistance = distance;
            if (step == RideStep.Riding)
            {
                Knock();
                ShowSparks();
                return;
            }

            EndRun(step == RideStep.Arrived);
        }

        /// <summary>The move input as a sideways steer (−1 left … 1 right of the cart), in the camera's frame.</summary>
        private float SteerInput()
        {
            if (_controller == null || _controller.Input == null)
            {
                return 0f;
            }

            Vector2 move = _controller.Input.MoveInput;
            if (move.sqrMagnitude < 0.01f)
            {
                return 0f;
            }

            Camera cam = Camera.main;
            Vector3 forward = cam != null ? Vector3.ProjectOnPlane(cam.transform.forward, Vector3.up).normalized : Vector3.forward;
            Vector3 right = cam != null ? Vector3.ProjectOnPlane(cam.transform.right, Vector3.up).normalized : Vector3.right;
            Vector3 world = forward * move.y + right * move.x;
            Vector3 heading = _player.forward;
            Vector2 side = MechanicsRide.Right(new Vector2(heading.x, heading.z).normalized);
            return Mathf.Clamp(world.x * side.x + world.z * side.y, -1f, 1f);
        }

        private void CatchSparks(float distance)
        {
            TetoCartSpark[] sparks = TetoCartRules.Sparks;
            for (int i = 0; i < sparks.Length; i++)
            {
                if (_caught[i] || !TetoCartRules.Catches(sparks[i], _lastDistance, distance, _ride.Lateral, _night))
                {
                    continue;
                }

                _caught[i] = true;
                if (_sparkLines[i] != null)
                {
                    _sparkLines[i].enabled = false;
                }

                ServiceLocator.TryGet(out LocalizationManager localization);
                SayText(TetoCartTexts.Format(localization, TetoCartTexts.Spark, TetoCartRules.Count(_caught), sparks.Length));
            }
        }

        /// <summary>The cart knocks each live beast it runs into once a run (a slice of its max life through its Health).</summary>
        private void Knock()
        {
            Vector2 here = _ride.Here;
            float radiusSq = TetoCartRules.KnockRadius * TetoCartRules.KnockRadius;
            foreach (EnemyController enemy in EnemyController.EnabledEnemies)
            {
                if (enemy == null || enemy.Health == null || !enemy.Health.IsAlive || _knocked.Contains(enemy))
                {
                    continue;
                }

                Vector3 p = enemy.transform.position;
                if ((new Vector2(p.x, p.z) - here).sqrMagnitude > radiusSq)
                {
                    continue;
                }

                _knocked.Add(enemy);
                float damage = TetoCartRules.KnockAmount(enemy.Health.MaxHealth, enemy.IsBoss);
                if (damage > 0f)
                {
                    enemy.Health.ApplyDamage(damage, ElementType.Physical, _player.gameObject);
                    Say(TetoCartTexts.Knock);
                }
            }
        }

        /// <summary>Hands the controls back (the ride already did, unless the driver is being switched off); at the foot, scores the run.</summary>
        private void EndRun(bool arrived)
        {
            float seconds = _ride.Elapsed;
            _ride.End();
            HideSparks();
            if (!_riding)
            {
                return;
            }

            _riding = false;
            _knocked.Clear();
            _lastRun = Time.unscaledTime;
            if (_player == null)
            {
                return;
            }

            if (!arrived || _view == null)
            {
                Say(TetoCartTexts.Broken);
                return;
            }

            Vector2 foot = OverworldLayouts.TetoCartFoot;
            Vector2 before = OverworldLayouts.TetoCartCourse[OverworldLayouts.TetoCartCourse.Length - 2];
            Vector2 end = _origin + foot;
            _view.WarmUp(end);
            CityModeController.PlaceAt(_player.gameObject, end, foot - before);
            _view.ResetTracking(end);
            Score(new TetoCartRun(TetoCartRules.Count(_caught), seconds));
        }

        private void Score(TetoCartRun run)
        {
            ServiceLocator.TryGet(out LocalizationManager localization);
            ServiceLocator.TryGet(out WorldMapProgress progress);
            var inv = System.Globalization.CultureInfo.InvariantCulture;
            SayText(TetoCartTexts.Format(localization, TetoCartTexts.Finished, run.Seconds.ToString("0.0", inv), run.Sparks, (run.ScoreMs / 1000f).ToString("0.0", inv)));
            if (TetoCartRules.IsRecord(BestMs(), run.ScoreMs))
            {
                _bestLocal = run.ScoreMs;
                progress?.SetTimeMark(TetoCartRules.BestKey, run.ScoreMs);
                SayText(TetoCartTexts.Get(localization, TetoCartTexts.NewRecord), 0.7f);
            }

            int today = Today();
            int nuggets = TetoCartRules.Nuggets(run.Sparks, PaidDay(), today);
            if (nuggets <= 0)
            {
                return;
            }

            ItemDefinition gold = ItemDatabase.GetById(NuggetItemId);
            if (gold == null || _bag == null || _bag.AddItem(gold, nuggets) > 0)
            {
                SayText(TetoCartTexts.Get(localization, TetoCartTexts.BagFull), 1.4f);
                return;
            }

            _paidLocal = today;
            progress?.SetDailyMark(TetoCartRules.PaidKey, today);
            SayText(TetoCartTexts.Format(localization, TetoCartTexts.Paid, nuggets), 1.4f);
        }

        // ------------------------------------------------------------------
        // Shown in the scene
        // ------------------------------------------------------------------

        private void ShowStations(Vector2 local)
        {
            bool ready = Time.unscaledTime - _lastRun >= TetoCartRules.CooldownSeconds;
            PostBeam(ref _station, "Teto_CartStation", OverworldLayouts.TetoCartTop, local, ready ? StationColor : StationIdleColor);
            PostBeam(ref _foot, "Teto_CartFoot", OverworldLayouts.TetoCartFoot, local, StationIdleColor);
        }

        private void PostBeam(ref LineRenderer line, string objectName, Vector2 at, Vector2 local, Color color)
        {
            if ((at - local).sqrMagnitude > ViewRange * ViewRange)
            {
                if (line != null)
                {
                    line.enabled = false;
                }

                return;
            }

            line ??= NewLine(objectName, 0.5f);
            Vector2 world = _origin + at;
            Vector3 foot = new Vector3(world.x, _ground.GroundAt(_layout, world), world.y);
            line.enabled = true;
            line.startColor = line.endColor = color;
            line.SetPosition(0, foot);
            line.SetPosition(1, foot + Vector3.up * StationHeight);
        }

        /// <summary>The sparks still to take within sight ahead of the cart, hung over the chute at their offsets.</summary>
        private void ShowSparks()
        {
            float here = _ride.Progress * _courseLength;
            TetoCartSpark[] sparks = TetoCartRules.Sparks;
            for (int i = 0; i < sparks.Length; i++)
            {
                bool visible = !_caught[i] && sparks[i].Distance > here && sparks[i].Distance - here < ViewRange;
                if (!visible)
                {
                    if (_sparkLines[i] != null)
                    {
                        _sparkLines[i].enabled = false;
                    }

                    continue;
                }

                LineRenderer line = _sparkLines[i] ??= NewLine("Teto_CartSpark", SparkSize);
                Vector2 d = RidePath.DirectionAt(_courseWorld, sparks[i].Distance);
                Vector2 at = RidePath.PointAt(_courseWorld, sparks[i].Distance) + MechanicsRide.Right(d) * sparks[i].Offset;
                float y = _ground.GroundAt(_layout, at) + SparkHeight;
                line.enabled = true;
                line.startColor = line.endColor = SparkColor;
                line.SetPosition(0, new Vector3(at.x, y - SparkSize * 0.5f, at.y));
                line.SetPosition(1, new Vector3(at.x, y + SparkSize * 0.5f, at.y));
            }
        }

        private void HideSparks()
        {
            if (_sparkLines == null)
            {
                return;
            }

            foreach (LineRenderer line in _sparkLines)
            {
                if (line != null)
                {
                    line.enabled = false;
                }
            }
        }

        private void HideAll()
        {
            HideSparks();
            if (_station != null)
            {
                _station.enabled = false;
            }

            if (_foot != null)
            {
                _foot.enabled = false;
            }
        }

        private LineRenderer NewLine(string objectName, float width)
        {
            if (_material == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit") ?? Shader.Find("Particles/Standard Unlit") ?? Shader.Find("Sprites/Default");
                _material = new Material(shader) { name = "Mat_TetoCartLine_Runtime" };
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
            SayText(TetoCartTexts.Get(localization, key));
        }

        private void SayText(string text, float lift = 0f)
        {
            if (_player != null)
            {
                CombatFxManager.SpawnLootWarningText(_player.position + Vector3.up * (FeedbackHeight + lift), text);
            }
        }
    }
}
