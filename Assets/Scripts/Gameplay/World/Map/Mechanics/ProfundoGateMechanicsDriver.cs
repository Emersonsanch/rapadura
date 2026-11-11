using System;
using Rapadura.Core.DI;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Combat;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Environment;
using Rapadura.Gameplay.Inventory;
using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.Skills;
using Rapadura.Gameplay.World.Cities.Town;
using Rapadura.Gameplay.World.Map.Areas;
using Rapadura.Gameplay.World.Map.Overworld;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// Scene side of the five gates of the pits of the Fortaleza da Última Lâmpada (map 83; the rules are <see cref="ProfundoGateRules"/>):
    /// added to the overworld root, idle unless the layout holds the Limiar Profundo and the hero stands in it. Reaching a pit's lamp post
    /// breaks its gate's seal (a time mark in <see cref="WorldMapProgress"/>, kept for good); an open gate is a passage between the plaza and
    /// the post, both ways, shut while the Tide of Darkness rises. A passage checks the far zone still admits the hero and sets them down the way
    /// the Limiar lamps do (no controls taken, nothing left mid-way); the breath of the pit is a hazard on PlayerStats that stops at a floor.
    /// </summary>
    public sealed class ProfundoGateMechanicsDriver : MonoBehaviour, IRegionMechanics
    {
        private const string PlayerTag = "Player";
        private const float HintCooldown = 3f;
        private const float DeniedCooldown = 1.5f;
        private const float FeedbackHeight = 2.4f;
        private const float GateViewRange = 60f;
        private const float LandingViewRange = 90f;
        private const float OpenHeight = 7f;
        private const float SealedHeight = 2f;

        private static readonly Color SealedColor = new Color(0.35f, 0.3f, 0.45f, 0.8f);
        private static readonly Color[] ElementColors =
        {
            new Color(0.62f, 0.6f, 0.66f, 1f), new Color(1f, 0.45f, 0.2f, 1f), new Color(0.6f, 0.85f, 1f, 1f), new Color(0.4f, 0.7f, 0.45f, 1f), new Color(0.45f, 0.4f, 0.85f, 1f),
        };

        private readonly bool[] _openLocal = new bool[ProfundoGateRules.GateCount];
        private readonly LineRenderer[] _gateLines = new LineRenderer[ProfundoGateRules.GateCount];
        private readonly LineRenderer[] _landingLines = new LineRenderer[ProfundoGateRules.GateCount];
        private readonly HeroRequirementCache _heroState = new HeroRequirementCache();
        private readonly StaticGroundCache _ground = new StaticGroundCache();

        private OverworldView _view;
        private OverworldLayout _layout;
        private Vector2 _origin;
        private Transform _player;
        private PlayerController _controller;
        private PlayerStats _stats;
        private InventoryManager _bag;
        private Material _material;

        private float _lastPassage = float.NegativeInfinity;
        private float _nextHint;
        private float _nextDenied;
        private int _hintSpot = -1;

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
        }

        private void OnEnable() => _heroState.Attach();

        private void OnDisable()
        {
            Show(Vector2.zero, false);
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
            if (!FindPlayer())
            {
                return;
            }

            Vector2 local = new Vector2(_player.position.x, _player.position.z) - _origin;
            if (!OverworldLayouts.ProfundoBounds.Contains(local))
            {
                Show(local, false);
                _hintSpot = -1;
                return;
            }

            Show(local, true);
            if (_stats != null && !_stats.IsAlive)
            {
                return;
            }

            BreakSeals(local);
            TickPassages(local);
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
        // Seals and passages
        // ------------------------------------------------------------------

        /// <summary>Whether a seal held in the save (or, without a save service, in this session) has broken.</summary>
        public static bool GateOpenIn(WorldMapProgress progress, int gate) => progress != null && ProfundoGateRules.IsOpen(progress.GetTimeMark(ProfundoGateRules.MarkKey(gate)));

        private bool IsOpen(int gate) => ServiceLocator.TryGet(out WorldMapProgress progress) ? GateOpenIn(progress, gate) : _openLocal[gate];

        private void BreakSeals(Vector2 local)
        {
            int gate = ProfundoGateRules.PostAt(OverworldLayouts.ProfundoGates, local);
            if (gate < 0 || IsOpen(gate))
            {
                return;
            }

            _openLocal[gate] = true;
            if (ServiceLocator.TryGet(out WorldMapProgress progress))
            {
                progress.SetTimeMark(ProfundoGateRules.MarkKey(gate), Math.Max(1L, DateTimeOffset.UtcNow.ToUnixTimeSeconds()));
            }

            ServiceLocator.TryGet(out LocalizationManager localization);
            SayText(ProfundoGateTexts.Format(localization, ProfundoGateTexts.Unsealed, PitName(localization, gate)));
        }

        private void TickPassages(Vector2 local)
        {
            ProfundoGate[] gates = OverworldLayouts.ProfundoGates;
            int gate = ProfundoGateRules.SpotAt(gates, local);
            bool outward = gate >= 0;
            if (!outward)
            {
                gate = ProfundoGateRules.LandingAt(gates, local);
            }

            if (gate < 0)
            {
                _hintSpot = -1;
                return;
            }

            Vector2 to = outward ? gates[gate].Landing : gates[gate].Arrival;
            bool tideRising = ProfundoLampRules.PhaseAt(Time.time) == ProfundoTidePhase.Rising;
            ProfundoGateCheck check = ProfundoGateRules.Check(IsOpen(gate), tideRising, ZoneOpen(to), Time.unscaledTime - _lastPassage);
            int spot = outward ? gate : gate + ProfundoGateRules.GateCount;
            if (_hintSpot != spot || Time.unscaledTime >= _nextHint)
            {
                _hintSpot = spot;
                _nextHint = Time.unscaledTime + HintCooldown;
                SayCheck(check, gate, outward);
            }

            // A sealed gate has nothing to use: the button stays off and the hint says why.
            if (check != ProfundoGateCheck.Sealed)
            {
                InteractPromptRegistry.Ping(Time.unscaledTime);
            }

            if (!_controller.Input.InteractPressedThisFrame)
            {
                return;
            }

            if (check == ProfundoGateCheck.Ok)
            {
                Pass(gate, outward);
            }
            else if (Time.unscaledTime >= _nextDenied)
            {
                _nextDenied = Time.unscaledTime + DeniedCooldown;
                SayCheck(check, gate, outward);
            }
        }

        private void SayCheck(ProfundoGateCheck check, int gate, bool outward)
        {
            ServiceLocator.TryGet(out LocalizationManager localization);
            switch (check)
            {
                case ProfundoGateCheck.Ok:
                    SayText(outward ? ProfundoGateTexts.Format(localization, ProfundoGateTexts.Pass, PitName(localization, gate)) : ProfundoGateTexts.Get(localization, ProfundoGateTexts.Back));
                    break;
                case ProfundoGateCheck.Sealed:
                    SayText(ProfundoGateTexts.Format(localization, ProfundoGateTexts.Sealed, PitName(localization, gate)));
                    break;
                case ProfundoGateCheck.TideShut:
                    SayText(ProfundoGateTexts.Get(localization, ProfundoGateTexts.TideShut));
                    break;
                case ProfundoGateCheck.ZoneBlocked:
                    SayText(ProfundoGateTexts.Get(localization, ProfundoGateTexts.Blocked));
                    break;
                default:
                    SayText(ProfundoGateTexts.Get(localization, ProfundoGateTexts.Resting));
                    break;
            }
        }

        private static string PitName(LocalizationManager localization, int gate) => ProfundoGateTexts.Get(localization, ProfundoGateTexts.Pits[gate]);

        private bool ZoneOpen(Vector2 local)
        {
            MapRequirementState state = _heroState.Get(_bag);
            MapModeController mode = MapModeController.Current;
            int level = mode != null ? mode.HeroLevel : 1;
            OverworldZone zone = OverworldRules.ZoneAt(_layout, _origin + local);
            return zone != null && OverworldRules.CheckEnter(zone, level, state.Hero, _heroState.Lookup) == MapAccessCheck.Ok;
        }

        private void Pass(int gate, bool outward)
        {
            ProfundoGate g = OverworldLayouts.ProfundoGates[gate];
            Vector2 to = _origin + (outward ? g.Landing : g.Arrival);
            // Out at the pit facing its lamp post; back on the plaza facing away from the gate.
            Vector2 facing = outward ? g.Post - g.Landing : g.Arrival - g.Spot;
            _lastPassage = Time.unscaledTime;
            _hintSpot = -1;
            _controller.ClickToMove?.ClearDestination();
            _view.WarmUp(to);
            CityModeController.PlaceAt(_player.gameObject, to, facing.sqrMagnitude > 0.01f ? facing : Vector2.down);
            _view.ResetTracking(to);

            // The tunnel breathes the pit's element on whoever walks it, never past the floor of life.
            bool night = ServiceLocator.TryGet(out TimeOfDayManager clock) && clock.Phase == TimeOfDayPhase.Night;
            if (_stats != null && _stats.IsAlive)
            {
                float breath = ProfundoGateRules.BreathAmount(_stats.CurrentHealth, _stats.MaxHealth, night);
                if (breath > 0f)
                {
                    _stats.ApplyHazardDamage(breath);
                    Say(ProfundoGateTexts.Breath);
                }
            }

            if (outward)
            {
                LampAtLanding(to);
            }
        }

        /// <summary>The gate's lamp, coming out at the pit, burns the beasts waiting at the door.</summary>
        private void LampAtLanding(Vector2 world)
        {
            bool burned = false;
            float radiusSq = ProfundoGateRules.LampRadius * ProfundoGateRules.LampRadius;
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

                float amount = ProfundoGateRules.LampAmount(enemy.Health.MaxHealth, enemy.IsBoss);
                if (amount > 0f)
                {
                    enemy.Health.ApplyDamage(amount, ElementType.Holy, _player.gameObject);
                    burned = true;
                }
            }

            if (burned)
            {
                Say(ProfundoGateTexts.LampBurns);
            }
        }

        // ------------------------------------------------------------------
        // Feedback
        // ------------------------------------------------------------------

        private void Show(Vector2 local, bool show)
        {
            ProfundoGate[] gates = OverworldLayouts.ProfundoGates;
            for (int i = 0; i < gates.Length; i++)
            {
                bool open = show && IsOpen(i);
                ShowColumn(_gateLines, i, gates[i].Spot, show && (gates[i].Spot - local).sqrMagnitude < GateViewRange * GateViewRange, open, "Pf_GateColumn");
                ShowColumn(_landingLines, i, gates[i].Landing, open && (gates[i].Landing - local).sqrMagnitude < LandingViewRange * LandingViewRange, true, "Pf_GateLanding");
            }
        }

        private void ShowColumn(LineRenderer[] lines, int i, Vector2 at, bool visible, bool open, string objectName)
        {
            LineRenderer line = lines[i];
            if (!visible)
            {
                if (line != null)
                {
                    line.enabled = false;
                }

                return;
            }

            if (line == null)
            {
                line = lines[i] = NewLine(objectName, 0.35f);
            }

            Vector2 world = _origin + at;
            Vector3 foot = new Vector3(world.x, _ground.GroundAt(_layout, world), world.y);
            line.enabled = true;
            line.startColor = line.endColor = open ? ElementColors[i] : SealedColor;
            line.SetPosition(0, foot);
            line.SetPosition(1, foot + Vector3.up * (open ? OpenHeight : SealedHeight));
        }

        private LineRenderer NewLine(string objectName, float width)
        {
            if (_material == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit") ?? Shader.Find("Particles/Standard Unlit") ?? Shader.Find("Sprites/Default");
                _material = new Material(shader) { name = "Mat_ProfundoGateLine_Runtime" };
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
            SayText(ProfundoGateTexts.Get(localization, key));
        }

        private void SayText(string text) => CombatFxManager.SpawnLootWarningText(_player.position + Vector3.up * FeedbackHeight, text);
    }
}
