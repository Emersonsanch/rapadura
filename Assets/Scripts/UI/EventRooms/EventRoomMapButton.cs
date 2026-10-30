using System.Collections.Generic;
using Rapadura.Core.DI;
using Rapadura.Core.Localization;
using Rapadura.Core.Systems;
using Rapadura.Gameplay.Content;
using Rapadura.Gameplay.Dialogue;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.EventRooms;
using Rapadura.Gameplay.Inventory;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.World.Biomes;
using Rapadura.Gameplay.World.Map;
using Rapadura.Save;
using Rapadura.UI.WorldMap;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace Rapadura.UI.EventRooms
{
    /// <summary>
    /// The event rooms on the world map, self-bootstrapping like <c>FishingSpotButton</c> (no scene edit): added to the
    /// <see cref="WorldMapHudController"/>'s GameObject after every scene load, it injects
    /// <list type="bullet">
    /// <item>a schedule pill at the top ("Terreiro das Assombrações: ABERTO até 01:00!" / "…: abre às 04:00"), one line
    /// per room, visible with the HUD;</item>
    /// <item>one "Entrar: …" button per room in the HUD's <c>wm-action-row</c> while its window is open and not yet
    /// entered this time. Click: checks the level (carried PlayerStats) and spends the entry item from the carried bag
    /// (<see cref="CarriedInventorySession"/>), marks the window as entered and starts the room through
    /// <see cref="WorldMapController.TryStartEventRoom"/> (the item comes back if the hop fails). Problems show as a
    /// short message under the pill.</item>
    /// </list>
    /// </summary>
    [DisallowMultipleComponent]
    public class EventRoomMapButton : MonoBehaviour
    {
        public const string PillName = "er-map-pill";
        public const string ButtonNamePrefix = "wm-event-room-";
        private const float RefreshInterval = 0.25f;
        private const float MessageSeconds = 3.5f;
        private const string PlayerStatsSaveKey = "PlayerStats";

        private WorldMapHudController _hud;
        private UIDocument _hudDocument;
        private WorldMapController _worldMap;
        private VisualElement _pill;
        private readonly Dictionary<EventRoomKind, Label> _lines = new Dictionary<EventRoomKind, Label>();
        private readonly Dictionary<EventRoomKind, Button> _buttons = new Dictionary<EventRoomKind, Button>();
        private Label _message;
        private float _messageLeft;
        private float _nextRefresh;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
            AttachToHud();
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode) => AttachToHud();

        private static void AttachToHud()
        {
            WorldMapHudController hud = FindFirstObjectByType<WorldMapHudController>(FindObjectsInactive.Include);
            if (hud != null && hud.GetComponent<EventRoomMapButton>() == null)
            {
                hud.gameObject.AddComponent<EventRoomMapButton>();
            }
        }

        private void Awake()
        {
            _hud = GetComponent<WorldMapHudController>();
            _hudDocument = GetComponent<UIDocument>();
        }

        private void OnDisable()
        {
            foreach (Button button in _buttons.Values)
            {
                button?.RemoveFromHierarchy();
            }

            _buttons.Clear();
            _pill?.RemoveFromHierarchy();
            _pill = null;
            _lines.Clear();
            _message = null;
        }

        private static LocalizationManager Localization => ServiceLocator.TryGet(out LocalizationManager localization) ? localization : null;

        private void Update()
        {
            if (_messageLeft > 0f)
            {
                _messageLeft -= Time.unscaledDeltaTime;
            }

            if (Time.unscaledTime < _nextRefresh)
            {
                return;
            }

            _nextRefresh = Time.unscaledTime + RefreshInterval;
            Refresh();
        }

        // ------------------------------------------------------------------
        // Pure helpers (tested)
        // ------------------------------------------------------------------

        /// <summary>"16:00" for an in-game hour of day.</summary>
        public static string FormatHour(float hourOfDay)
        {
            float hour = Mathf.Repeat(hourOfDay, 24f);
            int minutes = Mathf.RoundToInt(hour * 60f) % (24 * 60);
            return $"{minutes / 60:00}:{minutes % 60:00}";
        }

        /// <summary>The pill line of one room.</summary>
        public static string PillLine(LocalizationManager localization, EventRoomKind kind, EventRoomWindow window, bool alreadyEntered)
        {
            string room = EventRoomTexts.Get(localization, EventRoomTexts.RoomNameKey(kind));
            if (window.IsOpen && !alreadyEntered)
            {
                return EventRoomTexts.Format(localization, EventRoomTexts.MapOpen, room, FormatHour(window.NextChangeHourOfDay));
            }

            if (window.IsOpen)
            {
                // Entered this opening: next one starts a period after this one did.
                return EventRoomTexts.Format(localization, EventRoomTexts.MapEntered, room, FormatHour(window.NextChangeHourOfDay));
            }

            return EventRoomTexts.Format(localization, EventRoomTexts.MapClosed, room, FormatHour(window.NextChangeHourOfDay));
        }

        /// <summary>Message for a blocked entry (empty for None).</summary>
        public static string BlockMessage(LocalizationManager localization, EventRoomEntryBlock block, EventRoomKind kind)
        {
            switch (block)
            {
                case EventRoomEntryBlock.Closed:
                    return EventRoomTexts.Get(localization, EventRoomTexts.MapClosedNow);
                case EventRoomEntryBlock.AlreadyEntered:
                    return EventRoomTexts.Get(localization, EventRoomTexts.MapAlreadyEntered);
                case EventRoomEntryBlock.Level:
                    return EventRoomTexts.Format(localization, EventRoomTexts.MapNeedLevel, EventRoomRules.MinLevel(kind));
                case EventRoomEntryBlock.NoItem:
                    string item = EventRoomTexts.Get(localization, kind == EventRoomKind.Torre ? EventRoomTexts.KeyNameKey : EventRoomTexts.TicketNameKey);
                    return EventRoomTexts.Format(localization, EventRoomTexts.MapNeedItem, item);
                default:
                    return string.Empty;
            }
        }

        // ------------------------------------------------------------------
        // View
        // ------------------------------------------------------------------

        private void Refresh()
        {
            if (!EnsureView())
            {
                return;
            }

            if (_worldMap == null)
            {
                _worldMap = FindFirstObjectByType<WorldMapController>();
            }

            bool hudVisible = _hud != null && _hud.IsVisible;
            _pill.style.display = hudVisible ? DisplayStyle.Flex : DisplayStyle.None;
            ServiceLocator.TryGet(out DialogueManager dialogue);
            bool canAct = hudVisible && _worldMap != null && _worldMap.CanExplore && !GameplayPause.IsPaused && (dialogue == null || !dialogue.IsActive);

            LocalizationManager localization = Localization;
            EventRoomProgress progress = EventRoomProgress.ResolveShared();
            foreach (EventRoomKind kind in EventRoomProgress.Rooms)
            {
                EventRoomWindow window = progress.WindowNow(kind);
                bool entered = window.IsOpen && window.WindowIndex == progress.LastEnteredWindow(kind);
                SetText(_lines[kind], PillLine(localization, kind, window, entered));
                _lines[kind].EnableInClassList("er-map-line--open", window.IsOpen && !entered);

                Button button = _buttons[kind];
                bool show = canAct && window.IsOpen && !entered;
                button.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
                if (show)
                {
                    SetText(button, EventRoomTexts.Format(localization, EventRoomTexts.MapEnterButton, EventRoomTexts.Get(localization, EventRoomTexts.RoomNameKey(kind))));
                }
            }

            _message.style.display = hudVisible && _messageLeft > 0f ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private static void SetText(TextElement element, string text)
        {
            if (element != null && element.text != text)
            {
                element.text = text;
            }
        }

        private bool EnsureView()
        {
            if (_hudDocument == null || _hudDocument.rootVisualElement == null)
            {
                return false;
            }

            VisualElement root = _hudDocument.rootVisualElement;
            VisualElement row = root.Q<VisualElement>(RegionNpcTalkButton.ActionRowName);
            if (row == null)
            {
                return false;
            }

            if (_pill == null || _pill.panel == null)
            {
                _pill?.RemoveFromHierarchy();
                _lines.Clear();
                _pill = new VisualElement { name = PillName, pickingMode = PickingMode.Ignore };
                _pill.AddToClassList("er-map-pill");
                EventRoomStyles.ApplyTo(_pill);
                var card = new VisualElement { pickingMode = PickingMode.Ignore };
                card.AddToClassList("er-map-card");
                foreach (EventRoomKind kind in EventRoomProgress.Rooms)
                {
                    var line = new Label { pickingMode = PickingMode.Ignore };
                    line.AddToClassList("er-map-line");
                    card.Add(line);
                    _lines[kind] = line;
                }

                _message = new Label { pickingMode = PickingMode.Ignore };
                _message.AddToClassList("er-map-message");
                _message.style.display = DisplayStyle.None;
                card.Add(_message);
                _pill.Add(card);
                root.Add(_pill);
            }

            foreach (EventRoomKind kind in EventRoomProgress.Rooms)
            {
                if (_buttons.TryGetValue(kind, out Button existing) && existing != null && existing.parent == row)
                {
                    continue;
                }

                existing?.RemoveFromHierarchy();
                EventRoomKind captured = kind;
                var button = new Button(() => OnClicked(captured)) { name = ButtonNamePrefix + EventRoomRules.RoomId(kind), focusable = true };
                // Same look as the "Conversar" button next to it.
                button.AddToClassList("wm-explore-button");
                button.AddToClassList(RegionNpcTalkButton.ButtonClass);
                button.AddToClassList("er-map-button--open");
                EventRoomStyles.ApplyTo(button);
                button.style.marginLeft = 8f;
                button.style.display = DisplayStyle.None;
                row.Add(button);
                _buttons[kind] = button;
            }

            return true;
        }

        private void ShowMessage(string text)
        {
            if (_message == null || string.IsNullOrEmpty(text))
            {
                return;
            }

            _message.text = text;
            _messageLeft = MessageSeconds;
            _message.style.display = DisplayStyle.Flex;
        }

        // ------------------------------------------------------------------
        // Entry
        // ------------------------------------------------------------------

        private void OnClicked(EventRoomKind kind)
        {
            if (GameplayPause.IsPaused || (ServiceLocator.TryGet(out DialogueManager dialogue) && dialogue.IsActive))
            {
                return;
            }

            if (_worldMap == null)
            {
                _worldMap = FindFirstObjectByType<WorldMapController>();
            }

            if (_worldMap == null || !_worldMap.CanExplore)
            {
                return;
            }

            LocalizationManager localization = Localization;
            EventRoomProgress progress = EventRoomProgress.ResolveShared();
            EventRoomWindow window = progress.WindowNow(kind);
            int level = HeroLevel();
            string itemId = EventRoomRules.EntryItemId(kind);

            ServiceLocator.TryGet(out SaveManager saveManager);
            var session = new CarriedInventorySession(saveManager);
            InventoryManager bag = session.Open(transform);
            int held = bag != null ? bag.GetTotalCount(itemId) : 0;

            EventRoomEntryBlock block = EventRoomEntry.CheckEntry(window, progress.LastEnteredWindow(kind), level, kind, held);
            if (block != EventRoomEntryBlock.None || bag == null || !bag.RemoveById(itemId, 1))
            {
                session.Close();
                ShowMessage(BlockMessage(localization, block == EventRoomEntryBlock.None ? EventRoomEntryBlock.NoItem : block, kind));
                return;
            }

            session.Close();

            List<EnemyDefinition> pool = EventRoomEntry.BuildPool(BiomeRegistry.GetAll());
            BiomeType region = _worldMap.CurrentRegion ?? BiomeType.Forest;
            EncounterPayload payload = EventRoomEntry.BuildPayload(kind, region, level, System.Environment.TickCount, pool);
            progress.MarkEntered(kind, window.WindowIndex);
            if (payload != null && _worldMap.TryStartEventRoom(payload))
            {
                return;
            }

            // The hop did not happen: give the item and the opening back.
            progress.MarkEntered(kind, -1);
            var refundSession = new CarriedInventorySession(saveManager);
            InventoryManager refund = refundSession.Open(transform);
            if (refund != null && ItemDatabase.TryGetById(itemId, out ItemDefinition item))
            {
                refund.AddItemIgnoringWeight(item, 1);
            }

            refundSession.Close();
        }

        private static int HeroLevel()
        {
            string json = null;
            if (ServiceLocator.TryGet(out SaveManager saveManager))
            {
                saveManager.TryGetCarriedJson(PlayerStatsSaveKey, out json);
            }

            return WorldMapHudPresenter.ParseLevel(json);
        }
    }

    /// <summary>Loads Resources/UI/EventRooms.uss (cached) and attaches it with the theme class.</summary>
    public static class EventRoomStyles
    {
        public const string ResourcePath = "UI/EventRooms";
        public const string ThemeClass = "rap-theme";

        private static StyleSheet _sheet;

        public static StyleSheet Sheet
        {
            get
            {
                if (_sheet == null)
                {
                    _sheet = Resources.Load<StyleSheet>(ResourcePath);
                }

                return _sheet;
            }
        }

        public static void ApplyTo(VisualElement root)
        {
            if (root == null)
            {
                return;
            }

            StyleSheet sheet = Sheet;
            if (sheet != null && !root.styleSheets.Contains(sheet))
            {
                root.styleSheets.Add(sheet);
            }

            root.AddToClassList(ThemeClass);
        }
    }
}
