using Rapadura.Core.DI;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Combat;
using Rapadura.Gameplay.Inventory;
using Rapadura.Gameplay.Player;
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
    /// a zone they could not walk into.
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

        private static readonly Color LitColor = new Color(1f, 0.7f, 0.28f, 1f);
        private static readonly Color UnlitColor = new Color(0.45f, 0.42f, 0.5f, 0.8f);

        private readonly LimiarLampProgress _progress = new LimiarLampProgress();
        private readonly HeroRequirementCache _heroState = new HeroRequirementCache();

        private OverworldView _view;
        private OverworldLayout _layout;
        private Vector2 _origin;
        private Transform _player;
        private PlayerController _controller;
        private InventoryManager _bag;
        private Material _material;
        private LineRenderer[] _beacons;
        private float[] _beaconY;
        private Rapadura.Save.SaveManager _saves;

        private float _lastTravel = float.NegativeInfinity;
        private float _nextHint;
        private float _nextDenied;
        private int _hintLamp = -1;

        private Rect[] _areas;

        /// <inheritdoc/>
        public Rect[] MechanicsAreas => _areas;

        /// <inheritdoc/>
        public bool MechanicsBusy => false;

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
            if (_beacons != null)
            {
                ShowBeacons(Vector2.zero, false);
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

            Vector2 local = new Vector2(_player.position.x, _player.position.z) - _origin;
            if (!OverworldLayouts.LimiarBounds.Contains(local))
            {
                ShowBeacons(local, false);
                return;
            }

            ShowBeacons(local, true);
            TickLamp(local);
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
                    line = _beacons[i] = NewLine();
                    _beaconY[i] = Mathf.Max(0f, OverworldRules.SurfaceAt(_layout, point));
                }

                float y = _beaconY[i];
                line.enabled = true;
                line.startColor = line.endColor = lit ? LitColor : UnlitColor;
                line.SetPosition(0, new Vector3(point.x, y, point.y));
                line.SetPosition(1, new Vector3(point.x, y + (lit ? LitHeight : UnlitHeight), point.y));
            }
        }

        private LineRenderer NewLine()
        {
            if (_material == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit") ?? Shader.Find("Particles/Standard Unlit") ?? Shader.Find("Sprites/Default");
                _material = new Material(shader) { name = "Mat_LimiarLine_Runtime" };
            }

            var go = new GameObject("Lm_LampBeacon");
            go.transform.SetParent(transform, false);
            var line = go.AddComponent<LineRenderer>();
            line.sharedMaterial = _material;
            line.useWorldSpace = true;
            line.positionCount = 2;
            line.startWidth = line.endWidth = 0.45f;
            line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            line.receiveShadows = false;
            line.enabled = false;
            return line;
        }

        private void Say(string key)
        {
            ServiceLocator.TryGet(out LocalizationManager localization);
            CombatFxManager.SpawnLootWarningText(_player.position + Vector3.up * FeedbackHeight, LimiarMechanicsTexts.Get(localization, key));
        }
    }
}
