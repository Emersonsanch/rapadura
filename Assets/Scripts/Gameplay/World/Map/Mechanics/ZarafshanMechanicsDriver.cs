using Rapadura.Core.DI;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Combat;
using Rapadura.Gameplay.Environment;
using Rapadura.Gameplay.Inventory;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.World.Map.Overworld;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// Scene side of the Bosque do Zarafshan mechanics (the rules are <see cref="ZarafshanHarvestRules"/> and <see cref="ZarafshanFordRules"/>):
    /// added to the overworld root, idle unless the layout holds the region and the hero stands in it. It never takes control, deals no damage
    /// and only reads [E] within reach of a tree, so walking through the region is exactly as it was.
    /// </summary>
    public sealed class ZarafshanMechanicsDriver : MonoBehaviour, IRegionMechanics
    {
        private const string PlayerTag = "Player";
        private const float HintCooldown = 3f;
        private const float FordSayCooldown = 20f;
        private const float HealTickSeconds = 1f;
        private const float FeedbackHeight = 2.4f;
        private const float BeaconRange = 90f;
        private const float RipeHeight = 9f;
        private const float PickedHeight = 2f;
        private const float FordHeight = 5f;
        private const float FordHealingHeight = 11f;

        private static readonly Color RipeColor = new Color(1f, 0.72f, 0.3f, 1f);
        private static readonly Color PickedColor = new Color(0.55f, 0.6f, 0.4f, 0.7f);
        private static readonly Color FordColor = new Color(0.3f, 0.8f, 0.76f, 0.9f);

        private OverworldLayout _layout;
        private Vector2 _origin;
        private Transform _player;
        private PlayerController _controller;
        private PlayerStats _stats;
        private InventoryManager _bag;
        private Material _material;
        private LineRenderer[] _trees;
        private LineRenderer[] _fords;
        private float[] _treeY;
        private float[] _fordY;
        private string[] _treeKeys;
        private bool _shown;

        private float _nextHint;
        private int _hintSpot = -1;
        private float _nextFordSay;
        private float _healTick;
        private int _fordInside = -1;

        private Rect[] _areas;

        /// <inheritdoc/>
        public Rect[] MechanicsAreas => _areas;

        /// <inheritdoc/>
        public bool MechanicsBusy => false;

        private void Start()
        {
            OverworldView view = GetComponent<OverworldView>();
            _layout = view != null ? view.Layout : null;
            OverworldZone anchor = _layout?.Zone(OverworldLayouts.ZarafshanAnchorZoneId);
            if (anchor == null)
            {
                enabled = false;
                return;
            }

            _origin = anchor.Origin;
            _areas = new[] { MechanicsDriverHub.WorldArea(OverworldLayouts.ZarafshanBounds, _origin) };
            _trees = new LineRenderer[OverworldLayouts.ZarafshanHarvestSpots.Length];
            _fords = new LineRenderer[OverworldLayouts.ZarafshanFords.Length];
            _treeY = new float[_trees.Length];
            _fordY = new float[_fords.Length];
            _treeKeys = new string[_trees.Length];
            for (int i = 0; i < _treeKeys.Length; i++)
            {
                _treeKeys[i] = ZarafshanHarvestRules.MarkKey(OverworldLayouts.ZarafshanHarvestSpots[i].Id);
            }
        }

        private void OnDisable() => HideBeacons();

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
            bool inRegion = OverworldLayouts.ZarafshanBounds.Contains(local);
            if (!inRegion)
            {
                // Outside the region every beacon is hidden: hide them once instead of walking the trees every frame.
                HideBeacons();
                _hintSpot = -1;
                _fordInside = -1;
                _healTick = 0f;
                return;
            }

            int day = ServiceLocator.TryGet(out TimeOfDayManager clock) ? clock.Day : 1;
            ServiceLocator.TryGet(out WorldMapProgress progress);
            int ford = ZarafshanFordRules.FordAt(local, OverworldLayouts.ZarafshanFords);
            ShowBeacons(local, day, progress, ford);

            TickHarvest(local, day, progress);
            TickFord(ford);
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

            return _controller != null && _stats != null;
        }

        private void TickHarvest(Vector2 local, int day, WorldMapProgress progress)
        {
            int spot = ZarafshanHarvestRules.SpotAt(local, OverworldLayouts.ZarafshanHarvestSpots);
            if (spot < 0 || progress == null || _controller.Input == null)
            {
                _hintSpot = -1;
                return;
            }

            string mark = _treeKeys[spot];
            bool ripe = ZarafshanHarvestRules.IsRipe(progress.GetDailyMark(mark), day);
            if (ripe && (_hintSpot != spot || Time.unscaledTime >= _nextHint))
            {
                _hintSpot = spot;
                _nextHint = Time.unscaledTime + HintCooldown;
                Say(ZarafshanMechanicsTexts.HarvestPrompt);
            }

            if (!_controller.Input.InteractPressedThisFrame)
            {
                return;
            }

            if (!ripe)
            {
                Say(ZarafshanMechanicsTexts.HarvestPicked);
                return;
            }

            Pick(spot, day, progress, mark);
        }

        private void Pick(int spot, int day, WorldMapProgress progress, string mark)
        {
            ItemDefinition item = ItemDatabase.GetById(ZarafshanHarvestRules.ItemFor(spot, day));
            if (item == null || _bag == null)
            {
                return;
            }

            ServiceLocator.TryGet(out LocalizationManager localization);
            _nextHint = Time.unscaledTime + HintCooldown;
            if (_bag.AddItem(item, 1) != 0)
            {
                SayText(string.Format(ZarafshanMechanicsTexts.Get(localization, ZarafshanMechanicsTexts.HarvestBagFull), item.DisplayName));
                return;
            }

            progress.SetDailyMark(mark, day);
            SayText(string.Format(ZarafshanMechanicsTexts.Get(localization, ZarafshanMechanicsTexts.HarvestFound), item.DisplayName));
            SayHarvestLine(spot, day, localization);
        }

        private void SayHarvestLine(int spot, int day, LocalizationManager localization)
        {
            bool grandma = ZarafshanHarvestRules.GrandmaSpeaks(spot, day);
            string key = ZarafshanMechanicsTexts.HarvestLine(MapRequirementHeroState.Capture(_bag).Hero, grandma, day);
            CombatFxManager.SpawnLootWarningText(_player.position + Vector3.up * (FeedbackHeight + 0.7f), ZarafshanMechanicsTexts.Get(localization, key));
        }

        private void TickFord(int ford)
        {
            if (ford < 0 || !_stats.IsAlive)
            {
                _fordInside = -1;
                _healTick = 0f;
                return;
            }

            bool hurt = ZarafshanFordRules.BelowCeiling(_stats.CurrentHealth, _stats.MaxHealth);
            if (hurt && _fordInside != ford && Time.unscaledTime >= _nextFordSay)
            {
                _nextFordSay = Time.unscaledTime + FordSayCooldown;
                Say(ZarafshanMechanicsTexts.FordRefresh);
            }

            _fordInside = ford;
            _healTick += Time.deltaTime;
            if (_healTick < HealTickSeconds)
            {
                return;
            }

            float heal = ZarafshanFordRules.HealOver(_stats.CurrentHealth, _stats.MaxHealth, _healTick);
            _healTick = 0f;
            if (heal > 0f)
            {
                _stats.Heal(heal);
            }
        }

        // ------------------------------------------------------------------
        // Feedback
        // ------------------------------------------------------------------

        private void ShowBeacons(Vector2 local, int day, WorldMapProgress progress, int ford)
        {
            _shown = true;
            ZarafshanHarvestSpot[] spots = OverworldLayouts.ZarafshanHarvestSpots;
            for (int i = 0; i < spots.Length; i++)
            {
                bool ripe = progress != null && ZarafshanHarvestRules.IsRipe(progress.GetDailyMark(_treeKeys[i]), day);
                SetBeacon(_trees, _treeY, i, spots[i].Position, local, ripe ? RipeColor : PickedColor, ripe ? RipeHeight : PickedHeight);
            }

            Vector2[] fords = OverworldLayouts.ZarafshanFords;
            for (int i = 0; i < fords.Length; i++)
            {
                SetBeacon(_fords, _fordY, i, fords[i], local, FordColor, i == ford ? FordHealingHeight : FordHeight);
            }
        }

        private void HideBeacons()
        {
            if (!_shown)
            {
                return;
            }

            _shown = false;
            HideLines(_trees);
            HideLines(_fords);
        }

        private static void HideLines(LineRenderer[] lines)
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

        private void SetBeacon(LineRenderer[] lines, float[] heights, int index, Vector2 position, Vector2 local, Color color, float height)
        {
            bool visible = (position - local).sqrMagnitude < BeaconRange * BeaconRange;
            LineRenderer line = lines[index];
            if (!visible)
            {
                if (line != null)
                {
                    line.enabled = false;
                }

                return;
            }

            Vector2 point = _origin + position;
            if (line == null)
            {
                // The trees and fords never move: the ground under each is sampled once, with its line.
                line = lines[index] = NewLine();
                heights[index] = Mathf.Max(0f, OverworldRules.SurfaceAt(_layout, point));
            }

            float y = heights[index];
            line.enabled = true;
            line.startColor = line.endColor = color;
            line.SetPosition(0, new Vector3(point.x, y, point.y));
            line.SetPosition(1, new Vector3(point.x, y + height, point.y));
        }

        private LineRenderer NewLine()
        {
            if (_material == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit") ?? Shader.Find("Particles/Standard Unlit") ?? Shader.Find("Sprites/Default");
                _material = new Material(shader) { name = "Mat_ZarafshanLine_Runtime" };
            }

            var go = new GameObject("Zf_Beacon");
            go.transform.SetParent(transform, false);
            var line = go.AddComponent<LineRenderer>();
            line.sharedMaterial = _material;
            line.useWorldSpace = true;
            line.positionCount = 2;
            line.startWidth = line.endWidth = 0.4f;
            line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            line.receiveShadows = false;
            line.enabled = false;
            return line;
        }

        private void Say(string key)
        {
            ServiceLocator.TryGet(out LocalizationManager localization);
            SayText(ZarafshanMechanicsTexts.Get(localization, key));
        }

        private void SayText(string text)
        {
            CombatFxManager.SpawnLootWarningText(_player.position + Vector3.up * FeedbackHeight, text);
        }
    }
}
