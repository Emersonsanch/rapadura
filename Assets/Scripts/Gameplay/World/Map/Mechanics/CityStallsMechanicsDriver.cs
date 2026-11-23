using System;
using System.Collections.Generic;
using Rapadura.Core.DI;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Combat;
using Rapadura.Gameplay.Content;
using Rapadura.Gameplay.Dialogue;
using Rapadura.Gameplay.Environment;
using Rapadura.Gameplay.Inventory;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.Refining;
using Rapadura.Gameplay.World.Cities.Town;
using Rapadura.Gameplay.World.Map.Overworld;
using Rapadura.UI.WorldMap;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// Scene side of the third round of city stalls in the seamless world: Shayn's Banca de Troca, Miwnay's fair that walks the four galleries and
    /// O Engarrafador Guloso helping at it in the Mercado dos Túneis (<see cref="TunnelMarketStallRules"/>, .Tunnel.cs), Pughat's diver of the day on
    /// Kirta's bottom (<see cref="KirtaDiverRescueRules"/>, .Kirta.cs), Kurogane's forge of the day in Hinokura (<see cref="HinokuraDayForgeRules"/>,
    /// .Hinokura.cs) and Makarios' moon counter in the Porto da Lua Branca (<see cref="PortoMoonStallRules"/>, .Porto.cs). Added to the overworld
    /// root; each city works only when the layout holds its region. The screens are <see cref="CityServicePanel"/>s; the markers are thin light
    /// beams (no colliders). Saves only on the events: a trade, a bundle bought, a gift taken, a diver freed.
    /// </summary>
    public sealed partial class CityStallsMechanicsDriver : MonoBehaviour, IRegionMechanics
    {
        private const string PlayerTag = "Player";
        private const float HintCooldown = 6f;
        private const float FeedbackHeight = 2.4f;
        private const float AreaHalf = 120f;

        private enum StallKind
        {
            Barter,
            Fair,
            Moon,
        }

        private Transform _player;
        private PlayerController _controller;
        private PlayerStats _stats;
        private InventoryManager _bag;
        private OverworldView _view;
        private Material _beamMaterial;
        private Rect[] _areas;
        private int _hintStall = -1;
        private float _nextHint;

        /// <inheritdoc/>
        public Rect[] MechanicsAreas => _areas;

        /// <inheritdoc/>
        public bool MechanicsBusy => false;

        /// <summary>Where the cousins stand to talk to <paramref name="residentId"/> in <paramref name="cityId"/> (town-local, x = east, z = north).</summary>
        public static Vector2 DoorOf(string cityId, string residentId)
        {
            CityLayout town = CityTownRecipes.LayoutFor(cityId);
            CityLot lot = town?.LotOfResident(residentId);
            return lot != null ? lot.DoorApproach : Vector2.zero;
        }

        private void Start()
        {
            _view = GetComponent<OverworldView>();
            OverworldLayout layout = _view != null ? _view.Layout : null;
            var areas = new List<Rect>();
            StartTunnel(layout, areas);
            StartKirta(layout, areas);
            StartHinokura(layout, areas);
            StartPorto(layout, areas);
            if (areas.Count == 0)
            {
                enabled = false;
                return;
            }

            _areas = areas.ToArray();
        }

        private static Rect Around(Vector2 centre, float half) => new Rect(centre - new Vector2(half, half), new Vector2(half * 2f, half * 2f));

        private void OnDisable()
        {
            _hintStall = -1;
            HideBeams();
        }

        private void OnDestroy()
        {
            if (_beamMaterial != null)
            {
                Destroy(_beamMaterial);
            }
        }

        private void Update()
        {
            if (_areas == null || !FindPlayer())
            {
                return;
            }

            Vector2 hero = new Vector2(_player.position.x, _player.position.z);
            TickTunnel(hero);
            TickKirta(hero);
            TickHinokura(hero);

            bool alive = _stats == null || _stats.IsAlive;
            ServiceLocator.TryGet(out DialogueManager dialogue);
            if (!alive || _controller.Input == null || CityServicePanel.IsOpen || (dialogue != null && dialogue.IsActive))
            {
                _hintStall = -1;
                return;
            }

            StallKind? at = StallAt(hero);
            if (!at.HasValue)
            {
                _hintStall = -1;
                return;
            }

            int stall = (int)at.Value;
            if (_hintStall != stall || Time.unscaledTime >= _nextHint)
            {
                _hintStall = stall;
                _nextHint = Time.unscaledTime + HintCooldown;
                SayText(PromptOf(at.Value, Localization()));
            }

            InteractPromptRegistry.Ping(Time.unscaledTime);
            if (!_controller.Input.InteractPressedThisFrame)
            {
                return;
            }

            CityServicePanel.Open(ScreenOf(at.Value));
        }

        private StallKind? StallAt(Vector2 hero)
        {
            if (_tunnelOn && CityServiceRules.InReach(_barterPoint, hero, TunnelMarketStallRules.BarterReach))
            {
                return StallKind.Barter;
            }

            if (_tunnelOn && CityServiceRules.InReach(TodayFairSpot(), hero, TunnelMarketStallRules.FairReach))
            {
                return StallKind.Fair;
            }

            if (_portoOn && CityServiceRules.InReach(_moonPoint, hero, PortoMoonStallRules.Reach))
            {
                return StallKind.Moon;
            }

            return null;
        }

        private string PromptOf(StallKind kind, LocalizationManager l)
        {
            switch (kind)
            {
                case StallKind.Barter: return CityStallsTexts.Get(l, CityStallsTexts.BarterPrompt);
                case StallKind.Fair: return CityStallsTexts.Format(l, CityStallsTexts.FairPrompt, GalleryName(TodayGallery(), l));
                default: return CityStallsTexts.Get(l, CityStallsTexts.MoonPrompt);
            }
        }

        private ICityServiceScreen ScreenOf(StallKind kind)
        {
            switch (kind)
            {
                case StallKind.Barter: return new BarterScreen(this);
                case StallKind.Fair: return new FairScreen(this);
                default: return new MoonScreen(this);
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
        // Shared helpers
        // ------------------------------------------------------------------

        private static LocalizationManager Localization() => ServiceLocator.TryGet(out LocalizationManager localization) ? localization : null;

        private static CharacterId? Hero() => ServiceLocator.TryGet(out CharacterSelectionState selection) ? selection.Selected : null;

        private static WorldMapProgress Progress() => ServiceLocator.TryGet(out WorldMapProgress progress) ? progress : null;

        private static long UnixNow() => DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        private static long Today() => CityServiceRules.DayOf(UnixNow());

        /// <summary>The in-game day (1 without a clock).</summary>
        private static int GameDay() => ServiceLocator.TryGet(out TimeOfDayManager clock) ? clock.Day : 1;

        private static bool IsNight() => ServiceLocator.TryGet(out TimeOfDayManager clock) && clock.Phase == TimeOfDayPhase.Night;

        private int Gold => _bag != null ? _bag.GetTotalCount(RefineItems.Gold) : 0;

        private static string ItemName(string itemId, LocalizationManager l)
        {
            ItemDefinition item = ItemDatabase.GetById(itemId);
            return item != null ? item.LocalizedName(l) : itemId;
        }

        /// <summary>Hands over every (id, count) or nothing (false: the bag is full; nothing given).</summary>
        private bool GiveAll(IReadOnlyList<(string Id, int Count)> items)
        {
            if (_bag == null)
            {
                return false;
            }

            var list = new List<KeyValuePair<ItemDefinition, int>>();
            foreach ((string id, int count) in items)
            {
                ItemDefinition item = ItemDatabase.GetById(id);
                if (item != null && count > 0)
                {
                    list.Add(new KeyValuePair<ItemDefinition, int>(item, count));
                }
            }

            if (!_bag.CanFitAll(list))
            {
                return false;
            }

            foreach (KeyValuePair<ItemDefinition, int> pair in list)
            {
                _bag.AddItem(pair.Key, pair.Value);
            }

            return true;
        }

        /// <summary>
        /// Buys one bundle of a fair-like counter (gallery fair, moon counter): checks, pays, hands over, marks the day. The status line for the screen.
        /// </summary>
        private string BuyBundle(TunnelFairLine line, string dayKey, int day, string boughtLineKey, LocalizationManager l, out bool bought)
        {
            bought = false;
            WorldMapProgress progress = Progress();
            int boughtDay = progress != null ? progress.GetDailyMark(dayKey) : 0;
            TunnelStallCheck check = TunnelMarketStallRules.CheckFairLine(line, boughtDay, day, Gold);
            if (check == TunnelStallCheck.UsedUp)
            {
                return CityStallsTexts.Get(l, CityStallsTexts.BundleTaken);
            }

            if (check == TunnelStallCheck.NotEnoughGold || _bag == null)
            {
                return CityStallsTexts.Format(l, CityStallsTexts.NoGold, Mathf.Max(0, line.Price - Gold));
            }

            ItemDefinition item = ItemDatabase.GetById(line.ItemId);
            if (item == null || !_bag.CanFitAll(new[] { new KeyValuePair<ItemDefinition, int>(item, line.Count) }))
            {
                return CityStallsTexts.Get(l, CityStallsTexts.BagFull);
            }

            if (!_bag.RemoveById(RefineItems.Gold, line.Price))
            {
                return CityStallsTexts.Format(l, CityStallsTexts.NoGold, line.Price);
            }

            _bag.AddItem(item, line.Count);
            progress?.SetDailyMark(dayKey, day);
            bought = true;
            if (!string.IsNullOrEmpty(boughtLineKey))
            {
                SayAbove(CityStallsTexts.Get(l, boughtLineKey));
            }

            return CityStallsTexts.Format(l, CityStallsTexts.BundleBought, item.LocalizedName(l), line.Count);
        }

        /// <summary>The rows of a fair-like counter.</summary>
        private List<CityServiceRow> BundleRows(TunnelFairLine[] lines, Func<int, string> dayKeyOf, int day, LocalizationManager l)
        {
            WorldMapProgress progress = Progress();
            var rows = new List<CityServiceRow>();
            for (int i = 0; i < lines.Length; i++)
            {
                TunnelFairLine line = lines[i];
                int boughtDay = progress != null ? progress.GetDailyMark(dayKeyOf(i)) : 0;
                TunnelStallCheck check = TunnelMarketStallRules.CheckFairLine(line, boughtDay, day, Gold);
                string button = check == TunnelStallCheck.UsedUp
                    ? CityStallsTexts.Get(l, CityStallsTexts.BundleTaken)
                    : CityStallsTexts.Format(l, CityStallsTexts.BundleBuy, line.Price);
                rows.Add(new CityServiceRow(CityStallsTexts.Format(l, CityStallsTexts.BundleRow, ItemName(line.ItemId, l), line.Count),
                    CityStallsTexts.Format(l, CityStallsTexts.BundlePrice, line.Price, line.CrossingPrice), button, check == TunnelStallCheck.Ready));
            }

            return rows;
        }

        // ------------------------------------------------------------------
        // Beams (no colliders)
        // ------------------------------------------------------------------

        private LineRenderer NewBeam(string objectName, float width)
        {
            if (_beamMaterial == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit") ?? Shader.Find("Particles/Standard Unlit") ?? Shader.Find("Sprites/Default");
                _beamMaterial = new Material(shader) { name = "Mat_CityStallsBeam_Runtime" };
            }

            var go = new GameObject(objectName);
            go.transform.SetParent(transform, false);
            var line = go.AddComponent<LineRenderer>();
            line.sharedMaterial = _beamMaterial;
            line.useWorldSpace = true;
            line.positionCount = 2;
            line.startWidth = line.endWidth = width;
            line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            line.receiveShadows = false;
            line.enabled = false;
            return line;
        }

        private void ShowBeam(LineRenderer line, Vector2 world, float height, Color color, float footOffset = 0f)
        {
            if (line == null)
            {
                return;
            }

            float ground = _view != null ? _view.GroundHeight(world) : 0f;
            Vector3 foot = new Vector3(world.x, ground + footOffset, world.y);
            line.enabled = true;
            line.startColor = line.endColor = color;
            line.SetPosition(0, foot);
            line.SetPosition(1, foot + Vector3.up * height);
        }

        private void HideBeams()
        {
            if (_fairBeam != null) _fairBeam.enabled = false;
            if (_diverBeam != null) _diverBeam.enabled = false;
        }

        // ------------------------------------------------------------------
        // Feedback
        // ------------------------------------------------------------------

        private void SayText(string text)
        {
            if (_player != null && !string.IsNullOrEmpty(text))
            {
                CombatFxManager.SpawnLootWarningText(_player.position + Vector3.up * FeedbackHeight, text);
            }
        }

        private void SayAbove(string text)
        {
            if (_player != null && !string.IsNullOrEmpty(text))
            {
                CombatFxManager.SpawnLootWarningText(_player.position + Vector3.up * (FeedbackHeight + 0.7f), text);
            }
        }

        private void SayCousin(Func<CharacterId, string> lineOf)
        {
            CharacterId? cousin = Hero();
            if (cousin.HasValue)
            {
                SayAbove(CityStallsTexts.Get(Localization(), lineOf(cousin.Value)));
            }
        }
    }
}
