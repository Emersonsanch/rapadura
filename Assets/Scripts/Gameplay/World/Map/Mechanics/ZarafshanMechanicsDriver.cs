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
    public sealed class ZarafshanMechanicsDriver : MonoBehaviour
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

        private float _nextHint;
        private int _hintSpot = -1;
        private float _nextFordSay;
        private float _healTick;
        private int _fordInside = -1;

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
            _trees = new LineRenderer[OverworldLayouts.ZarafshanHarvestSpots.Length];
            _fords = new LineRenderer[OverworldLayouts.ZarafshanFords.Length];
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
            bool inRegion = OverworldLayouts.ZarafshanBounds.Contains(local);
            int day = ServiceLocator.TryGet(out TimeOfDayManager clock) ? clock.Day : 1;
            ServiceLocator.TryGet(out WorldMapProgress progress);
            int ford = inRegion ? ZarafshanFordRules.FordAt(local, OverworldLayouts.ZarafshanFords) : -1;
            ShowBeacons(local, inRegion, day, progress, ford);
            if (!inRegion)
            {
                _hintSpot = -1;
                _fordInside = -1;
                _healTick = 0f;
                return;
            }

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

            string mark = ZarafshanHarvestRules.MarkKey(OverworldLayouts.ZarafshanHarvestSpots[spot].Id);
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

        private void ShowBeacons(Vector2 local, bool inRegion, int day, WorldMapProgress progress, int ford)
        {
            ZarafshanHarvestSpot[] spots = OverworldLayouts.ZarafshanHarvestSpots;
            for (int i = 0; i < spots.Length; i++)
            {
                bool ripe = progress != null && ZarafshanHarvestRules.IsRipe(progress.GetDailyMark(ZarafshanHarvestRules.MarkKey(spots[i].Id)), day);
                SetBeacon(_trees, i, spots[i].Position, local, inRegion, ripe ? RipeColor : PickedColor, ripe ? RipeHeight : PickedHeight);
            }

            Vector2[] fords = OverworldLayouts.ZarafshanFords;
            for (int i = 0; i < fords.Length; i++)
            {
                SetBeacon(_fords, i, fords[i], local, inRegion, FordColor, i == ford ? FordHealingHeight : FordHeight);
            }
        }

        private void SetBeacon(LineRenderer[] lines, int index, Vector2 position, Vector2 local, bool inRegion, Color color, float height)
        {
            bool visible = inRegion && (position - local).sqrMagnitude < BeaconRange * BeaconRange;
            LineRenderer line = lines[index];
            if (!visible)
            {
                if (line != null)
                {
                    line.enabled = false;
                }

                return;
            }

            line = line != null ? line : (lines[index] = NewLine());
            Vector2 point = _origin + position;
            float y = Mathf.Max(0f, OverworldRules.SurfaceAt(_layout, point));
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
