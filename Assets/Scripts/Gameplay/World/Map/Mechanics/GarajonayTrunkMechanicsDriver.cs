using Rapadura.Core.DI;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Combat;
using Rapadura.Gameplay.Inventory;
using Rapadura.Gameplay.Mounts;
using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.World.Cities.Town;
using Rapadura.Gameplay.World.Map.Overworld;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// Scene side of the marked trunks of the Copas de Garajonay (05, <see cref="GarajonayTrunkRules"/>): a post of claw marks up each marked
    /// laurel; riding the Laurel Lizard, [E] at one climbs it and runs over the canopy to the other end of the pair (<see cref="MechanicsRide"/>
    /// with the hero kept on the mount), anyone else only reads the hint. Added to the overworld root, idle unless the layout holds the Zarafshan
    /// region and the hero is in the Copas. The run hands the controls back on arrival, on death, if anything else moves the hero and in
    /// OnDisable. Nothing is saved: the trunks are always there.
    /// </summary>
    public sealed class GarajonayTrunkMechanicsDriver : MonoBehaviour, IRegionMechanics
    {
        private const string PlayerTag = "Player";
        private const float HintCooldown = 3f;
        private const float FeedbackHeight = 2.4f;
        private const float ViewRange = 90f;
        private const float MarkHeight = 9f;

        private static readonly Color MarkColor = new Color(0.62f, 0.78f, 0.36f, 0.75f);
        private static readonly Color MarkLizardColor = new Color(0.95f, 0.85f, 0.4f, 1f);

        private readonly StaticGroundCache _ground = new StaticGroundCache();
        private readonly MechanicsRide _ride = new MechanicsRide();
        private OverworldView _view;
        private OverworldLayout _layout;
        private Vector2 _origin;
        private Rect _copas;
        private Transform _player;
        private PlayerController _controller;
        private PlayerStats _stats;
        private InventoryManager _bag;
        private Material _material;
        private LineRenderer[] _marks;

        private float _nextHint;
        private int _hintSpot = -1;
        private float _lastRun = float.NegativeInfinity;
        private Vector2 _rideFrom;
        private Vector2 _rideTo;
        private bool _riding;

        private Rect[] _areas;

        /// <inheritdoc/>
        public Rect[] MechanicsAreas => _areas;

        /// <inheritdoc/>
        public bool MechanicsBusy => _ride.Active;

        private void Start()
        {
            _view = GetComponent<OverworldView>();
            _layout = _view != null ? _view.Layout : null;
            OverworldZone anchor = _layout?.Zone(OverworldLayouts.ZarafshanAnchorZoneId);
            if (anchor == null)
            {
                enabled = false;
                return;
            }

            _origin = anchor.Origin;
            _copas = OverworldLayouts.GarajonayFogBounds;
            _areas = new[] { MechanicsDriverHub.WorldArea(_copas, _origin) };
            _marks = new LineRenderer[OverworldLayouts.GarajonayTrunks.Length * 2];
        }

        private void OnDisable()
        {
            EndRide(false);
            Hide();
            _hintSpot = -1;
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
                RideStep step = _ride.Tick(Time.deltaTime);
                if (step != RideStep.Riding)
                {
                    EndRide(step == RideStep.Arrived);
                }

                return;
            }

            Vector2 local = new Vector2(_player.position.x, _player.position.z) - _origin;
            if (!_copas.Contains(local))
            {
                Hide();
                _hintSpot = -1;
                return;
            }

            bool lizard = OnLizard();
            Show(local, lizard);
            if (_stats != null && !_stats.IsAlive)
            {
                return;
            }

            TickTrunks(local, lizard);
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

        private static string MountedId(out bool mounted)
        {
            MountController mount = MountController.Active;
            mounted = mount != null && mount.IsMounted && mount.ActiveMount != null;
            return mounted ? mount.ActiveMount.mountId : null;
        }

        private static bool OnLizard()
        {
            string id = MountedId(out bool mounted);
            return GarajonayTrunkRules.IsClimber(mounted, id);
        }

        private void TickTrunks(Vector2 local, bool lizard)
        {
            LaurelTrunkLink[] links = OverworldLayouts.GarajonayTrunks;
            int index = GarajonayTrunkRules.TrunkAt(links, local, out bool atA);
            if (index < 0 || _controller.Input == null)
            {
                _hintSpot = -1;
                return;
            }

            string id = MountedId(out bool mounted);
            TrunkCheck check = GarajonayTrunkRules.Check(mounted, id, Time.unscaledTime - _lastRun);
            int spot = index * 2 + (atA ? 0 : 1);
            if (_hintSpot != spot || Time.unscaledTime >= _nextHint)
            {
                _hintSpot = spot;
                _nextHint = Time.unscaledTime + HintCooldown;
                Say(check == TrunkCheck.Climb ? GarajonayTrunkTexts.Prompt : check == TrunkCheck.Resting ? GarajonayTrunkTexts.Resting : GarajonayTrunkTexts.NeedsLizard);
            }

            if (check != TrunkCheck.Climb)
            {
                return;
            }

            InteractPromptRegistry.Ping(Time.unscaledTime);
            if (!_controller.Input.InteractPressedThisFrame)
            {
                return;
            }

            LaurelTrunkLink link = links[index];
            Vector2[] course = RidePath.ToWorld(GarajonayTrunkRules.Course(link, atA), _origin, false);
            if (!_ride.Begin(_player.gameObject, _layout, course, GarajonayTrunkRules.RunSpeed, GarajonayTrunkRules.CanopyArc, 0f, keepMount: true))
            {
                return;
            }

            _riding = true;
            _rideFrom = atA ? link.A : link.B;
            _rideTo = GarajonayTrunkRules.Landing(link, atA);
            _hintSpot = -1;
            Hide();
            MountAudio.Play(MountAudio.Dash, _player.position);
            Say(GarajonayTrunkTexts.Climbs);
        }

        /// <summary>Hands the controls back (the ride already did, unless the driver is being switched off) and stands the hero at the far trunk.</summary>
        private void EndRide(bool arrived)
        {
            _ride.End();
            if (!_riding)
            {
                return;
            }

            _riding = false;
            _lastRun = Time.unscaledTime;
            if (!arrived || _view == null || _player == null)
            {
                return;
            }

            Vector2 end = _origin + _rideTo;
            _view.WarmUp(end);
            CityModeController.PlaceAt(_player.gameObject, end, _rideTo - _rideFrom);
            _view.ResetTracking(end);
            Say(GarajonayTrunkTexts.Landed);
            ServiceLocator.TryGet(out LocalizationManager localization);
            CombatFxManager.SpawnLootWarningText(_player.position + Vector3.up * (FeedbackHeight + 0.7f),
                GarajonayTrunkTexts.Get(localization, GarajonayTrunkTexts.LineFor(MapRequirementHeroState.Capture(_bag).Hero)));
        }

        // ------------------------------------------------------------------
        // Shown in the scene
        // ------------------------------------------------------------------

        /// <summary>A post of claw marks up every marked trunk near the hero: golden when the hero rides the lizard.</summary>
        private void Show(Vector2 local, bool lizard)
        {
            LaurelTrunkLink[] links = OverworldLayouts.GarajonayTrunks;
            float range = ViewRange * ViewRange;
            for (int i = 0; i < links.Length; i++)
            {
                Mark(i * 2, links[i].A, local, range, lizard);
                Mark(i * 2 + 1, links[i].B, local, range, lizard);
            }
        }

        private void Mark(int slot, Vector2 trunk, Vector2 local, float range, bool lizard)
        {
            if ((trunk - local).sqrMagnitude > range)
            {
                if (_marks[slot] != null)
                {
                    _marks[slot].enabled = false;
                }

                return;
            }

            LineRenderer line = _marks[slot];
            if (line == null)
            {
                line = _marks[slot] = NewLine("Garajonay_MarkedTrunk", 0.35f);
            }

            Vector2 world = _origin + trunk;
            var foot = new Vector3(world.x, _ground.GroundAt(_layout, world), world.y);
            line.enabled = true;
            line.startColor = line.endColor = lizard ? MarkLizardColor : MarkColor;
            line.SetPosition(0, foot);
            line.SetPosition(1, foot + Vector3.up * MarkHeight);
        }

        private void Hide()
        {
            if (_marks == null)
            {
                return;
            }

            foreach (LineRenderer line in _marks)
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
                _material = new Material(shader) { name = "Mat_GarajonayTrunk_Runtime" };
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
            if (_player == null)
            {
                return;
            }

            ServiceLocator.TryGet(out LocalizationManager localization);
            CombatFxManager.SpawnLootWarningText(_player.position + Vector3.up * FeedbackHeight, GarajonayTrunkTexts.Get(localization, key));
        }
    }
}
