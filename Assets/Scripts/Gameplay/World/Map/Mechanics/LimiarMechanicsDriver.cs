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
    /// Scene side of the lamps of the Grutas do Limiar (the rules are <see cref="LimiarLampRules"/>, the state <see cref="LimiarLampProgress"/>):
    /// added to the overworld root, idle unless the layout holds the region and the hero stands in it. Lighting costs nothing; a trip
    /// checks the destination zone still admits the hero and uses the same placement as the Farnkesh baskets, so nobody is left in
    /// a zone they could not walk into. The wind wells of the Ninho de Mahaway (<see cref="LimiarWindWellRules"/>, map 24) glide a hero with
    /// wings over a chasm: the glide takes the controls for 7 s at most and hands them back on arrival, on death or if anything else moves the hero.
    /// </summary>
    public sealed class LimiarMechanicsDriver : MonoBehaviour, IRegionMechanics
    {
        private const string PlayerTag = "Player";
        private const float HintCooldown = 3f;
        private const float DeniedCooldown = 1.5f;
        private const float FeedbackHeight = 2.4f;
        private const float BeaconRange = 170f;
        private const float LitHeight = 14f;
        private const float UnlitHeight = 3f;
        private const float ExternalMoveTolerance = 3f;
        private const float WellViewRange = 150f;
        private const float WellBlowHeight = 18f;
        private const float WellCalmHeight = 4f;
        private const float LandingHeight = 3f;

        private static readonly Color LitColor = new Color(1f, 0.7f, 0.28f, 1f);
        private static readonly Color UnlitColor = new Color(0.45f, 0.42f, 0.5f, 0.8f);
        private static readonly Color WindColor = new Color(0.72f, 0.88f, 1f, 1f);
        private static readonly Color CalmColor = new Color(0.4f, 0.45f, 0.55f, 0.7f);

        private readonly LimiarLampProgress _progress = new LimiarLampProgress();
        private readonly HeroRequirementCache _heroState = new HeroRequirementCache();

        private OverworldView _view;
        private OverworldLayout _layout;
        private Vector2 _origin;
        private Transform _player;
        private PlayerController _controller;
        private PlayerMotor _motor;
        private CharacterController _character;
        private PlayerStats _stats;
        private InventoryManager _bag;
        private Material _material;
        private LineRenderer[] _beacons;
        private float[] _beaconY;
        private Rapadura.Save.SaveManager _saves;

        private float _lastTravel = float.NegativeInfinity;
        private LineRenderer[] _wellLines;
        private LineRenderer _landingLine;
        private int _hintWell = -1;
        private float _lastGlide = float.NegativeInfinity;

        private int _glide = -1;
        private float _glideT;
        private float _glideElapsed;
        private float _glideSeconds;
        private float _glideFromY;
        private float _glideToY;
        private int _pecksApplied;
        private bool _saidPeck;
        private Vector3 _lastGlidePosition;
        private float _nextHint;
        private float _nextDenied;
        private int _hintLamp = -1;

        private Rect[] _areas;

        /// <inheritdoc/>
        public Rect[] MechanicsAreas => _areas;

        /// <inheritdoc/>
        public bool MechanicsBusy => _glide >= 0;

        private void Start()
        {
            _view = GetComponent<OverworldView>();
            _layout = _view != null ? _view.Layout : null;
            OverworldZone anchor = _layout?.Zone(OverworldLayouts.LimiarAnchorZoneId);
            if (anchor == null)
            {
                enabled = false;
                return;
            }

            _origin = anchor.Origin;
            _areas = new[] { MechanicsDriverHub.WorldArea(OverworldLayouts.LimiarBounds, _origin) };
            _beacons = new LineRenderer[OverworldLayouts.LimiarLamps.Length];
            _beaconY = new float[_beacons.Length];
            _wellLines = new LineRenderer[OverworldLayouts.LimiarWindWells.Length];

            // The lamps are in the save: registered only while the region exists, the SaveManager carries them otherwise.
            if (ServiceLocator.TryGet(out _saves))
            {
                _saves.Register(_progress);
                _saves.RestoreCarriedState();
            }
        }

        private void OnEnable() => _heroState.Attach();

        private void OnDisable()
        {
            EndGlide(false);
            if (_beacons != null)
            {
                ShowBeacons(Vector2.zero, false);
                ShowWells(Vector2.zero, false);
            }

            _heroState.Detach();
        }

        private void OnDestroy()
        {
            if (_saves != null)
            {
                _saves.Unregister(_progress);
            }

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

            if (_glide >= 0)
            {
                TickGlide();
                return;
            }

            Vector2 local = new Vector2(_player.position.x, _player.position.z) - _origin;
            if (!OverworldLayouts.LimiarBounds.Contains(local))
            {
                ShowBeacons(local, false);
                ShowWells(local, false);
                return;
            }

            ShowBeacons(local, true);
            ShowWells(local, true);
            if (!TickWells(local))
            {
                TickLamp(local);
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
                _motor = go.GetComponent<PlayerMotor>();
                _character = go.GetComponent<CharacterController>();
                _stats = go.GetComponent<PlayerStats>();
                _bag = go.GetComponentInChildren<InventoryManager>();
            }

            return _controller != null;
        }

        private void TickLamp(Vector2 local)
        {
            int lamp = LimiarLampRules.LampAt(local, OverworldLayouts.LimiarLamps);
            LimiarLampAction action = LimiarLampRules.ActionAt(lamp, OverworldLayouts.LimiarLamps, _progress.Lit);
            if (lamp < 0 || action == LimiarLampAction.None)
            {
                _hintLamp = -1;
                return;
            }

            if (_hintLamp != lamp || Time.unscaledTime >= _nextHint)
            {
                _hintLamp = lamp;
                _nextHint = Time.unscaledTime + HintCooldown;
                Say(action == LimiarLampAction.Light ? LimiarMechanicsTexts.LightPrompt : action == LimiarLampAction.ReturnToVila ? LimiarMechanicsTexts.BackPrompt : LimiarMechanicsTexts.DeepPrompt);
            }

            InteractPromptRegistry.Ping(Time.unscaledTime);
            if (!_controller.Input.InteractPressedThisFrame)
            {
                return;
            }

            if (action == LimiarLampAction.Light)
            {
                _progress.Light(OverworldLayouts.LimiarLamps[lamp].Id);
                Say(LimiarMechanicsTexts.Lit);
                return;
            }

            Travel(lamp);
        }

        private void Travel(int lamp)
        {
            if (!LimiarLampRules.CanTravel(Time.unscaledTime, _lastTravel))
            {
                Deny(LimiarMechanicsTexts.Resting);
                return;
            }

            int target = LimiarLampRules.TravelTarget(lamp, OverworldLayouts.LimiarLamps, _progress.Lit);
            if (target < 0)
            {
                return;
            }

            Vector2 to = _origin + OverworldLayouts.LimiarLamps[target].Position;
            if (!ZoneOpen(to))
            {
                Deny(LimiarMechanicsTexts.Closed);
                return;
            }

            _lastTravel = Time.unscaledTime;
            _controller.ClickToMove?.ClearDestination();
            _view.WarmUp(to);
            CityModeController.PlaceAt(_player.gameObject, to, Vector2.down);
            _view.ResetTracking(to);
        }

        private bool ZoneOpen(Vector2 world)
        {
            MapRequirementState state = _heroState.Get(_bag);
            MapModeController mode = MapModeController.Current;
            int level = mode != null ? mode.HeroLevel : 1;
            OverworldZone zone = OverworldRules.ZoneAt(_layout, world);
            return zone != null && OverworldRules.CheckEnter(zone, level, state.Hero, _heroState.Lookup) == MapAccessCheck.Ok;
        }

        private void Deny(string key)
        {
            if (Time.unscaledTime >= _nextDenied)
            {
                _nextDenied = Time.unscaledTime + DeniedCooldown;
                Say(key);
            }
        }

        // ------------------------------------------------------------------
        // The wind wells of the Ninho de Mahaway (24)
        // ------------------------------------------------------------------

        /// <summary>True while the hero stands at a well (the lamps then wait).</summary>
        private bool TickWells(Vector2 local)
        {
            LimiarWindWell[] wells = OverworldLayouts.LimiarWindWells;
            int index = LimiarWindWellRules.WellAt(wells, local);
            if (index < 0)
            {
                _hintWell = -1;
                return false;
            }

            if (_stats != null && !_stats.IsAlive)
            {
                return true;
            }

            LimiarWindWell well = wells[index];
            MapRequirementState state = _heroState.Get(_bag);
            bool canGlide = MapRequirementRules.IsMet(MapRequirementTags.WingsOrDoubleJump, state);
            MapModeController mode = MapModeController.Current;
            int level = mode != null ? mode.HeroLevel : 1;
            var route = new SkyRoute { Id = well.Id, Kind = SkyRouteKind.Glide, A = well.Well, B = well.Landing };
            bool zones = SkyRouteRules.AllZonesOpen(_layout, _origin, route, level, state.Hero, _heroState.Lookup);
            LimiarWellCheck check = LimiarWindWellRules.Check(well, Time.time, canGlide, zones, Time.unscaledTime - _lastGlide);
            if (_hintWell != index || Time.unscaledTime >= _nextHint)
            {
                _hintWell = index;
                _nextHint = Time.unscaledTime + HintCooldown;
                SayWell(check, well);
            }

            InteractPromptRegistry.Ping(Time.unscaledTime);
            if (!_controller.Input.InteractPressedThisFrame)
            {
                return true;
            }

            if (check == LimiarWellCheck.Ok && _motor != null && _character != null)
            {
                BeginGlide(index);
            }
            else if (Time.unscaledTime >= _nextDenied)
            {
                _nextDenied = Time.unscaledTime + DeniedCooldown;
                SayWell(check, well);
            }

            return true;
        }

        private void SayWell(LimiarWellCheck check, LimiarWindWell well)
        {
            switch (check)
            {
                case LimiarWellCheck.Ok:
                    Say(LimiarMechanicsTexts.WellReady);
                    break;
                case LimiarWellCheck.NeedsWings:
                    Say(LimiarMechanicsTexts.WellNeedsWings);
                    break;
                case LimiarWellCheck.ZoneBlocked:
                    Say(LimiarMechanicsTexts.WellBlocked);
                    break;
                case LimiarWellCheck.Resting:
                    Say(LimiarMechanicsTexts.WellResting);
                    break;
                default:
                    ServiceLocator.TryGet(out LocalizationManager localization);
                    SayText(LimiarMechanicsTexts.Format(localization, LimiarMechanicsTexts.WellCalm, Mathf.CeilToInt(LimiarWindWellRules.SecondsUntilBlowing(well, Time.time))));
                    break;
            }
        }

        private void BeginGlide(int index)
        {
            LimiarWindWell well = OverworldLayouts.LimiarWindWells[index];
            MountController.Active?.Dismount(startRecall: false, leave: false);
            _glide = index;
            _glideT = 0f;
            _glideElapsed = 0f;
            _pecksApplied = 0;
            _saidPeck = false;
            _glideSeconds = LimiarWindWellRules.GlideSeconds(well);
            Vector2 from = _origin + well.Well;
            Vector2 to = _origin + well.Landing;
            _glideFromY = Mathf.Max(0f, OverworldRules.SurfaceAt(_layout, from));
            _glideToY = Mathf.Max(0f, OverworldRules.SurfaceAt(_layout, to));

            // Clears the motor's velocity so the Animator rests in idle while the controls are off.
            _motor.Teleport(_player.position, Quaternion.LookRotation(new Vector3(to.x - from.x, 0f, to.y - from.y)));
            _controller.ClickToMove?.ClearDestination();
            _controller.enabled = false;
            _character.enabled = false;
            _lastGlidePosition = _player.position;
            Say(LimiarMechanicsTexts.WellLift);
        }

        private void TickGlide()
        {
            bool dead = _stats != null && !_stats.IsAlive;
            if (dead || (_player.position - _lastGlidePosition).sqrMagnitude > ExternalMoveTolerance * ExternalMoveTolerance)
            {
                // The hero died, or something else moved them (a respawn, the barca): the glide is over where they are.
                EndGlide(false);
                return;
            }

            LimiarWindWell well = OverworldLayouts.LimiarWindWells[_glide];
            _glideElapsed += Time.deltaTime;
            _glideT = Mathf.Min(1f, _glideT + Time.deltaTime / _glideSeconds);
            Vector2 ground = _origin + Vector2.Lerp(well.Well, well.Landing, _glideT);
            float y = Mathf.Max(SkyRouteRules.HeightAt(_glideFromY, _glideToY, LimiarWindWellRules.GlideArc, _glideT), OverworldRules.SurfaceAt(_layout, ground) + 1f);
            _player.position = new Vector3(ground.x, y, ground.y);
            _lastGlidePosition = _player.position;
            PeckGlider();
            if (_glideT >= 1f)
            {
                EndGlide(true);
            }
        }

        /// <summary>Mahaway's flock pecks at the hero hanging in the air: a hazard, never past the floor of life.</summary>
        private void PeckGlider()
        {
            int due = LimiarWindWellRules.PecksDue(_glideElapsed);
            if (_stats == null || _pecksApplied >= due)
            {
                return;
            }

            _pecksApplied = due;
            bool night = ServiceLocator.TryGet(out TimeOfDayManager clock) && clock.Phase == TimeOfDayPhase.Night;
            float peck = LimiarWindWellRules.PeckAmount(_stats.CurrentHealth, _stats.MaxHealth, night);
            if (peck <= 0f)
            {
                return;
            }

            _stats.ApplyHazardDamage(peck);
            if (!_saidPeck)
            {
                _saidPeck = true;
                Say(LimiarMechanicsTexts.WellPecked);
            }
        }

        private void EndGlide(bool arrived)
        {
            if (_glide < 0)
            {
                return;
            }

            LimiarWindWell well = OverworldLayouts.LimiarWindWells[_glide];
            _glide = -1;
            _lastGlide = Time.unscaledTime;
            if (_character != null)
            {
                _character.enabled = true;
            }

            if (_controller != null)
            {
                _controller.enabled = true;
            }

            if (!arrived || _view == null || _player == null)
            {
                return;
            }

            Vector2 end = _origin + well.Landing;

            // The landing's collider must exist for the snap to the ground.
            _view.WarmUp(end);
            CityModeController.PlaceAt(_player.gameObject, end, well.Landing - well.Well);
            _view.ResetTracking(end);
            GustAtLanding(end);
        }

        /// <summary>The gust that sets the glider down knocks the beasts waiting on the rim.</summary>
        private void GustAtLanding(Vector2 end)
        {
            bool hit = false;
            float radiusSq = LimiarWindWellRules.GustRadius * LimiarWindWellRules.GustRadius;
            foreach (EnemyController enemy in EnemyController.EnabledEnemies)
            {
                if (enemy == null || enemy.Health == null || !enemy.Health.IsAlive)
                {
                    continue;
                }

                Vector3 p = enemy.transform.position;
                if ((new Vector2(p.x, p.z) - end).sqrMagnitude > radiusSq)
                {
                    continue;
                }

                float gust = LimiarWindWellRules.GustAmount(enemy.Health.MaxHealth, enemy.IsBoss);
                if (gust > 0f)
                {
                    enemy.Health.ApplyDamage(gust, ElementType.Wind, _player.gameObject);
                    hit = true;
                }
            }

            Say(hit ? LimiarMechanicsTexts.WellGust : LimiarMechanicsTexts.WellLanded);
        }

        private void ShowWells(Vector2 local, bool show)
        {
            LimiarWindWell[] wells = OverworldLayouts.LimiarWindWells;
            int near = -1;
            for (int i = 0; i < wells.Length; i++)
            {
                LimiarWindWell well = wells[i];
                bool visible = show && (well.Well - local).sqrMagnitude < WellViewRange * WellViewRange;
                LineRenderer line = _wellLines[i];
                if (!visible)
                {
                    if (line != null)
                    {
                        line.enabled = false;
                    }

                    continue;
                }

                if (line == null)
                {
                    line = _wellLines[i] = NewLine("Lm_WindWell", 1.1f);
                }

                bool blowing = LimiarWindWellRules.IsBlowing(well, Time.time);
                Vector3 foot = Ground(well.Well);
                line.enabled = true;
                line.startColor = blowing ? WindColor : CalmColor;
                line.endColor = new Color(WindColor.r, WindColor.g, WindColor.b, blowing ? 0.15f : 0.05f);
                line.SetPosition(0, foot);
                line.SetPosition(1, foot + Vector3.up * (blowing ? WellBlowHeight : WellCalmHeight));
                if ((well.Well - local).sqrMagnitude <= LimiarWindWellRules.Reach * LimiarWindWellRules.Reach * 4f)
                {
                    near = i;
                }
            }

            // Where the nearest well lets the glider down, on the far rim.
            if (near < 0)
            {
                if (_landingLine != null)
                {
                    _landingLine.enabled = false;
                }

                return;
            }

            _landingLine = _landingLine != null ? _landingLine : NewLine("Lm_WellLanding", 0.35f);
            Vector3 landing = Ground(wells[near].Landing);
            _landingLine.enabled = true;
            _landingLine.startColor = _landingLine.endColor = WindColor;
            _landingLine.SetPosition(0, landing);
            _landingLine.SetPosition(1, landing + Vector3.up * LandingHeight);
        }

        private Vector3 Ground(Vector2 local)
        {
            Vector2 world = _origin + local;
            return new Vector3(world.x, Mathf.Max(0f, OverworldRules.SurfaceAt(_layout, world)), world.y);
        }

        // ------------------------------------------------------------------
        // Feedback
        // ------------------------------------------------------------------

        private void ShowBeacons(Vector2 local, bool show)
        {
            for (int i = 0; i < _beacons.Length; i++)
            {
                LimiarLamp lamp = OverworldLayouts.LimiarLamps[i];
                bool visible = show && (lamp.Position - local).sqrMagnitude < BeaconRange * BeaconRange;
                LineRenderer line = _beacons[i];
                if (!visible)
                {
                    if (line != null)
                    {
                        line.enabled = false;
                    }

                    continue;
                }

                bool lit = LimiarLampRules.IsLit(i, OverworldLayouts.LimiarLamps, _progress.Lit);
                Vector2 point = _origin + lamp.Position;
                if (line == null)
                {
                    // The lamps never move: the ground under each is sampled once, with its line.
                    line = _beacons[i] = NewLine("Lm_LampBeacon", 0.45f);
                    _beaconY[i] = Mathf.Max(0f, OverworldRules.SurfaceAt(_layout, point));
                }

                float y = _beaconY[i];
                line.enabled = true;
                line.startColor = line.endColor = lit ? LitColor : UnlitColor;
                line.SetPosition(0, new Vector3(point.x, y, point.y));
                line.SetPosition(1, new Vector3(point.x, y + (lit ? LitHeight : UnlitHeight), point.y));
            }
        }

        private LineRenderer NewLine(string objectName, float width)
        {
            if (_material == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit") ?? Shader.Find("Particles/Standard Unlit") ?? Shader.Find("Sprites/Default");
                _material = new Material(shader) { name = "Mat_LimiarLine_Runtime" };
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
            SayText(LimiarMechanicsTexts.Get(localization, key));
        }

        private void SayText(string text) => CombatFxManager.SpawnLootWarningText(_player.position + Vector3.up * FeedbackHeight, text);
    }
}
