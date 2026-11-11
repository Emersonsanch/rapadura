using System;
using Rapadura.Core.DI;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Combat;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Environment;
using Rapadura.Gameplay.Inventory;
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
    /// Scene side of the rafts of the Campos de Juncos (72, <see cref="RioRaftRules"/>): [E] at a reed clump on a canal bank cuts a bundle, [E] at
    /// a landing ties a raft (three bundles) and sails the long canal to the far landing. Added to the overworld root, idle unless the layout
    /// holds the Rio region and the hero stands in the middle row of the fields. The sail takes the controls (<see cref="MechanicsRide"/>) and
    /// hands them back on arrival, on death, if anything else moves the hero and in OnDisable. The mud hippos' bites are a hazard on PlayerStats
    /// that stops at a floor of life; the clumps cut and the raft tied stay in the save (time marks).
    /// </summary>
    public sealed class RioRaftMechanicsDriver : MonoBehaviour, IRegionMechanics
    {
        private const string PlayerTag = "Player";
        private const float HintCooldown = 3f;
        private const float DeniedCooldown = 1.5f;
        private const float FeedbackHeight = 2.4f;
        private const float ViewRange = 160f;
        private const float ReedHeight = 3.2f;
        private const float ReedCutHeight = 0.6f;
        private const float LandingHeight = 5f;

        /// <summary>The middle row of the fields (region coordinates) where the canals run: the Canteiros, the Campos and the Trigais.</summary>
        public static readonly Rect FieldsBounds = Rect.MinMaxRect(-720f, 360f, 720f, 840f);

        private static readonly Color ReedColor = new Color(0.86f, 0.74f, 0.36f, 1f);
        private static readonly Color CutColor = new Color(0.42f, 0.38f, 0.26f, 0.6f);
        private static readonly Color LandingColor = new Color(0.55f, 0.85f, 0.95f, 1f);
        private static readonly Color LandingIdleColor = new Color(0.4f, 0.45f, 0.5f, 0.7f);

        private readonly HeroRequirementCache _heroState = new HeroRequirementCache();
        private readonly StaticGroundCache _ground = new StaticGroundCache();
        private readonly MechanicsRide _ride = new MechanicsRide();
        private long[] _cutLocal;
        private long _builtLocal;
        private long[] _cutMarks;

        private OverworldView _view;
        private OverworldLayout _layout;
        private Vector2 _origin;
        private Transform _player;
        private PlayerController _controller;
        private PlayerStats _stats;
        private InventoryManager _bag;
        private Material _material;
        private LineRenderer[] _reedLines;
        private LineRenderer[] _landingLines;

        private float _nextHint;
        private float _nextDenied;
        private int _hintSpot = -1;
        private float _lastRide = float.NegativeInfinity;
        private int _rideTo = -1;
        private bool _rideOnHippo;
        private int _bitesApplied;
        private bool _saidBite;

        private Rect[] _areas;

        /// <inheritdoc/>
        public Rect[] MechanicsAreas => _areas;

        /// <inheritdoc/>
        public bool MechanicsBusy => _ride.Active;

        private void Start()
        {
            _view = GetComponent<OverworldView>();
            _layout = _view != null ? _view.Layout : null;
            OverworldZone anchor = _layout?.Zone(OverworldLayouts.RioAnchorZoneId);
            if (anchor == null)
            {
                enabled = false;
                return;
            }

            _origin = anchor.Origin;

            // The hub keeps the driver on over the whole region (like the barque's driver): Update itself idles outside the fields.
            _areas = new[] { MechanicsDriverHub.WorldArea(OverworldLayouts.RioBounds, _origin) };
            int clumps = OverworldLayouts.RioReedClumps.Length;
            _cutLocal = new long[clumps];
            _cutMarks = new long[clumps];
            _reedLines = new LineRenderer[clumps];
            _landingLines = new LineRenderer[OverworldLayouts.RioRaftCanals.Length * 2];
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
                TickRide();
                return;
            }

            Vector2 local = new Vector2(_player.position.x, _player.position.z) - _origin;
            if (!FieldsBounds.Contains(local))
            {
                HideAll();
                _hintSpot = -1;
                return;
            }

            long now = Now();
            ReadMarks();
            Show(local, now);
            if (_stats != null && !_stats.IsAlive)
            {
                return;
            }

            if (!TickReeds(local, now) && !TickLandings(local, now))
            {
                _hintSpot = -1;
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
                _controller = go.GetComponent<PlayerController>();
                _stats = go.GetComponent<PlayerStats>();
                _bag = go.GetComponentInChildren<InventoryManager>();
            }

            return _controller != null;
        }

        // ------------------------------------------------------------------
        // Save
        // ------------------------------------------------------------------

        private static long Now() => Math.Max(1L, DateTimeOffset.UtcNow.ToUnixTimeSeconds());

        private void ReadMarks()
        {
            bool saved = ServiceLocator.TryGet(out WorldMapProgress progress);
            for (int i = 0; i < _cutMarks.Length; i++)
            {
                _cutMarks[i] = saved ? progress.GetTimeMark(RioRaftRules.ReedKey(i)) : _cutLocal[i];
            }
        }

        private long BuiltAt() => ServiceLocator.TryGet(out WorldMapProgress progress) ? progress.GetTimeMark(RioRaftRules.BuiltKey) : _builtLocal;

        private void Mark(string key, long unix)
        {
            if (ServiceLocator.TryGet(out WorldMapProgress progress))
            {
                progress.SetTimeMark(key, unix);
            }
        }

        private int Bundles() => RioRaftRules.Bundles(_cutMarks, BuiltAt());

        // ------------------------------------------------------------------
        // Reeds
        // ------------------------------------------------------------------

        /// <summary>True while the hero stands at a reed clump.</summary>
        private bool TickReeds(Vector2 local, long now)
        {
            int clump = RioRaftRules.ReedAt(OverworldLayouts.RioReedClumps, local);
            if (clump < 0)
            {
                return false;
            }

            bool ready = RioRaftRules.ReedReady(_cutMarks[clump], now);
            int spot = 100 + clump;
            if (_hintSpot != spot || Time.unscaledTime >= _nextHint)
            {
                _hintSpot = spot;
                _nextHint = Time.unscaledTime + HintCooldown;
                SayReed(ready, clump, now);
            }

            if (!ready)
            {
                return true;
            }

            InteractPromptRegistry.Ping(Time.unscaledTime);
            if (!_controller.Input.InteractPressedThisFrame)
            {
                return true;
            }

            _cutLocal[clump] = now;
            _cutMarks[clump] = now;
            Mark(RioRaftRules.ReedKey(clump), now);
            _hintSpot = -1;
            int held = Bundles();
            ServiceLocator.TryGet(out LocalizationManager localization);
            SayText(held >= RioRaftRules.BundlesPerRaft
                ? RioRaftTexts.Get(localization, RioRaftTexts.ReedEnough)
                : RioRaftTexts.Format(localization, RioRaftTexts.ReedCut, held, RioRaftRules.BundlesPerRaft));
            return true;
        }

        private void SayReed(bool ready, int clump, long now)
        {
            ServiceLocator.TryGet(out LocalizationManager localization);
            SayText(ready
                ? RioRaftTexts.Format(localization, RioRaftTexts.ReedReady, Bundles(), RioRaftRules.BundlesPerRaft)
                : RioRaftTexts.Format(localization, RioRaftTexts.ReedGrowing, Mathf.Max(1, Mathf.CeilToInt(RioRaftRules.ReedSecondsLeft(_cutMarks[clump], now) / 60f))));
        }

        // ------------------------------------------------------------------
        // Landings and the sail
        // ------------------------------------------------------------------

        private static bool OnTaweretHippo()
        {
            MountController mount = MountController.Active;
            return mount != null && mount.IsMounted && mount.ActiveMount != null && mount.ActiveMount.mountId == MountWaveCatalog.Capybara;
        }

        /// <summary>True while the hero stands at a landing.</summary>
        private bool TickLandings(Vector2 local, long now)
        {
            RioRaftCanal[] canals = OverworldLayouts.RioRaftCanals;
            int landing = RioRaftRules.LandingAt(canals, local);
            if (landing < 0)
            {
                return false;
            }

            long built = BuiltAt();
            bool live = RioRaftRules.RaftLive(built, now);
            bool hippo = OnTaweretHippo();
            RioRaftCanal canal = canals[landing / 2];
            MapRequirementState state = _heroState.Get(_bag);
            MapModeController mode = MapModeController.Current;
            int level = mode != null ? mode.HeroLevel : 1;
            var route = new SkyRoute { Id = canal.Id, Kind = SkyRouteKind.Glide, A = canal.WestLanding, B = canal.EastLanding };
            bool zones = SkyRouteRules.AllZonesOpen(_layout, _origin, route, level, state.Hero, _heroState.Lookup);
            RioRaftCheck check = RioRaftRules.Check(zones, live, hippo, Bundles(), Time.unscaledTime - _lastRide);
            int spot = 200 + landing;
            if (_hintSpot != spot || Time.unscaledTime >= _nextHint)
            {
                _hintSpot = spot;
                _nextHint = Time.unscaledTime + HintCooldown;
                SayLanding(check, hippo && !live, built, now);
            }

            bool available = check == RioRaftCheck.Ride || check == RioRaftCheck.Build;
            if (available)
            {
                InteractPromptRegistry.Ping(Time.unscaledTime);
            }

            if (!_controller.Input.InteractPressedThisFrame)
            {
                return true;
            }

            if (available)
            {
                if (check == RioRaftCheck.Build)
                {
                    _builtLocal = now;
                    Mark(RioRaftRules.BuiltKey, now);
                    Say(RioRaftTexts.Built);
                }

                BeginRide(landing, RioRaftRules.SwimsOnHippo(check, hippo, live));
            }
            else if (Time.unscaledTime >= _nextDenied)
            {
                _nextDenied = Time.unscaledTime + DeniedCooldown;
                SayLanding(check, hippo, built, now);
            }

            return true;
        }

        private void SayLanding(RioRaftCheck check, bool hippoOnly, long built, long now)
        {
            ServiceLocator.TryGet(out LocalizationManager localization);
            switch (check)
            {
                case RioRaftCheck.Ride:
                    SayText(hippoOnly
                        ? RioRaftTexts.Get(localization, RioRaftTexts.LandingHippo)
                        : RioRaftTexts.Format(localization, RioRaftTexts.LandingRide, Math.Max(0L, RioRaftRules.RaftLifeSeconds - (now - built))));
                    break;
                case RioRaftCheck.Build:
                    Say(RioRaftTexts.LandingBuild);
                    break;
                case RioRaftCheck.NeedReeds:
                    SayText(RioRaftTexts.Format(localization, RioRaftTexts.LandingNeed, Bundles(), RioRaftRules.BundlesPerRaft));
                    break;
                case RioRaftCheck.ZoneBlocked:
                    Say(RioRaftTexts.LandingBlocked);
                    break;
                default:
                    Say(RioRaftTexts.LandingResting);
                    break;
            }
        }

        private void BeginRide(int landing, bool onHippo)
        {
            Vector2[] course = RidePath.ToWorld(RioRaftRules.Course(OverworldLayouts.RioRaftCanals, landing), _origin, false);
            if (!_ride.Begin(_player.gameObject, _layout, course, RioRaftRules.Speed, 0f, RioRaftRules.Hover, keepMount: onHippo))
            {
                return;
            }

            _rideTo = RioRaftRules.FarLanding(landing);
            _rideOnHippo = onHippo;
            _bitesApplied = 0;
            _saidBite = false;
            _hintSpot = -1;
            Say(onHippo ? RioRaftTexts.HippoSwims : RioRaftTexts.Sailing);
        }

        private void TickRide()
        {
            RideStep step = _ride.Tick(Time.deltaTime);
            if (step == RideStep.Riding)
            {
                BiteRider();
                return;
            }

            EndRide(step == RideStep.Arrived);
        }

        /// <summary>The mud hippos snap at the raft: a hazard, never past the floor of life; Taweret's hippo is let through.</summary>
        private void BiteRider()
        {
            int due = RioRaftRules.BitesDue(_ride.Elapsed);
            if (_rideOnHippo || _stats == null || _bitesApplied >= due)
            {
                return;
            }

            _bitesApplied = due;
            float bite = RioRaftRules.BiteAmount(_stats.CurrentHealth, _stats.MaxHealth, IsNight());
            if (bite <= 0f)
            {
                return;
            }

            _stats.ApplyHazardDamage(bite);
            if (!_saidBite)
            {
                _saidBite = true;
                Say(RioRaftTexts.Bitten);
            }
        }

        /// <summary>Hands the controls back (the ride already did, unless the driver is being switched off) and sets the hero on the far landing.</summary>
        private void EndRide(bool arrived)
        {
            _ride.End();
            if (_rideTo < 0)
            {
                return;
            }

            int to = _rideTo;
            _rideTo = -1;
            _lastRide = Time.unscaledTime;
            if (!arrived || _view == null || _player == null)
            {
                return;
            }

            Vector2 local = RioRaftRules.LandingPoint(OverworldLayouts.RioRaftCanals, to);
            Vector2 from = RioRaftRules.LandingPoint(OverworldLayouts.RioRaftCanals, RioRaftRules.FarLanding(to));
            Vector2 end = _origin + local;
            _view.WarmUp(end);
            CityModeController.PlaceAt(_player.gameObject, end, local - from);
            _view.ResetTracking(end);
            Say(Splash(end) ? RioRaftTexts.Splash : RioRaftTexts.Landed);
        }

        /// <summary>The splash of landing knocks the live beasts within reach of the landing; true when one was hit.</summary>
        private bool Splash(Vector2 world)
        {
            bool hit = false;
            float radiusSq = RioRaftRules.SplashRadius * RioRaftRules.SplashRadius;
            foreach (EnemyController enemy in EnemyController.EnabledEnemies)
            {
                if (enemy == null || enemy.Health == null || !enemy.Health.IsAlive)
                {
                    continue;
                }

                Vector3 p = enemy.transform.position;
                if ((new Vector2(p.x, p.z) - world).sqrMagnitude > radiusSq)
                {
                    continue;
                }

                float damage = RioRaftRules.SplashAmount(enemy.Health.MaxHealth, enemy.IsBoss);
                if (damage > 0f)
                {
                    enemy.Health.ApplyDamage(damage, ElementType.Physical, _player.gameObject);
                    hit = true;
                }
            }

            return hit;
        }

        // ------------------------------------------------------------------
        // Shown in the scene
        // ------------------------------------------------------------------

        private void Show(Vector2 local, long now)
        {
            Vector2[] clumps = OverworldLayouts.RioReedClumps;
            for (int i = 0; i < clumps.Length; i++)
            {
                bool ready = RioRaftRules.ReedReady(_cutMarks[i], now);
                Post(ref _reedLines[i], "Rio_ReedClump", 0.9f, clumps[i], local, ready ? ReedHeight : ReedCutHeight, ready ? ReedColor : CutColor);
            }

            bool raft = RioRaftRules.RaftLive(BuiltAt(), now) || Bundles() >= RioRaftRules.BundlesPerRaft || OnTaweretHippo();
            for (int i = 0; i < _landingLines.Length; i++)
            {
                Vector2 at = RioRaftRules.LandingPoint(OverworldLayouts.RioRaftCanals, i);
                Post(ref _landingLines[i], "Rio_RaftLanding", 0.5f, at, local, LandingHeight, raft ? LandingColor : LandingIdleColor);
            }
        }

        private void Post(ref LineRenderer line, string objectName, float width, Vector2 at, Vector2 local, float height, Color color)
        {
            if ((at - local).sqrMagnitude > ViewRange * ViewRange)
            {
                if (line != null)
                {
                    line.enabled = false;
                }

                return;
            }

            if (line == null)
            {
                line = NewLine(objectName, width);
            }

            Vector2 world = _origin + at;
            Vector3 foot = new Vector3(world.x, _ground.GroundAt(_layout, world), world.y);
            line.enabled = true;
            line.startColor = line.endColor = color;
            line.SetPosition(0, foot);
            line.SetPosition(1, foot + Vector3.up * height);
        }

        private void HideAll()
        {
            Hide(_reedLines);
            Hide(_landingLines);
        }

        private static void Hide(LineRenderer[] lines)
        {
            if (lines == null)
            {
                return;
            }

            foreach (LineRenderer line in lines)
            {
                if (line != null)
                {
                    line.enabled = false;
                }
            }
        }

        private LineRenderer NewLine(string objectName, float width)
        {
            if (_material == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit") ?? Shader.Find("Particles/Standard Unlit") ?? Shader.Find("Sprites/Default");
                _material = new Material(shader) { name = "Mat_RioRaftLine_Runtime" };
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

        private static bool IsNight() => ServiceLocator.TryGet(out TimeOfDayManager clock) && clock.Phase == TimeOfDayPhase.Night;

        private void Say(string key)
        {
            ServiceLocator.TryGet(out LocalizationManager localization);
            SayText(RioRaftTexts.Get(localization, key));
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
