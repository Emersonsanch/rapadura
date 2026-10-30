using System;
using Rapadura.Core.DI;
using Rapadura.Core.Localization;
using Rapadura.Core.Systems;
using Rapadura.Gameplay.Dialogue;
using Rapadura.Gameplay.World.Biomes;
using Rapadura.Gameplay.World.Map;
using Rapadura.Gameplay.WorldEvents;
using Rapadura.Save;
using Rapadura.UI.WorldMap;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace Rapadura.UI.WorldEvents
{
    /// <summary>
    /// The scheduled world events on the world map, self-bootstrapping like <c>EventRoomMapButton</c> (no scene edit):
    /// added to the <see cref="WorldMapHudController"/>'s GameObject after every scene load, it injects
    /// <list type="bullet">
    /// <item>a countdown pill (under the event-room pill): the world boss that is up now ("…está na Floresta! Vai embora
    /// em 12:04") or the next one, and the Invasão de Āz now or next — all from the real clock
    /// (<see cref="WorldEventCalendar"/>), so it is the same for everyone;</item>
    /// <item>a cousin's announcement line for a few seconds when a window opens while the map is up;</item>
    /// <item>"Enfrentar: …" / "Defender de Āz" in the HUD's <c>wm-action-row</c> while the hero stands in the
    /// region where it is happening and this window's prize is not taken yet. Click: level check (carried
    /// PlayerStats), then the fight through <see cref="WorldMapController.TryStartEventRoom"/> (directed payload).</item>
    /// </list>
    /// </summary>
    [DisallowMultipleComponent]
    public class WorldEventMapPanel : MonoBehaviour
    {
        public const string PillName = "we-map-pill";
        public const string BossButtonName = "wm-world-boss";
        public const string InvasionButtonName = "wm-invasion";
        private const float RefreshInterval = 0.25f;
        private const float MessageSeconds = 6f;
        private const string PlayerStatsSaveKey = "PlayerStats";

        private WorldMapHudController _hud;
        private UIDocument _hudDocument;
        private WorldMapController _worldMap;
        private VisualElement _pill;
        private Label _bossLine;
        private Label _invasionLine;
        private Label _message;
        private Button _bossButton;
        private Button _invasionButton;
        private float _messageLeft;
        private float _nextRefresh;
        private long _announcedBossWindow = long.MinValue;
        private string _announcedBossId;
        private long _announcedInvasionWindow = long.MinValue;

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
            if (hud != null && hud.GetComponent<WorldEventMapPanel>() == null)
            {
                hud.gameObject.AddComponent<WorldEventMapPanel>();
            }
        }

        private void Awake()
        {
            _hud = GetComponent<WorldMapHudController>();
            _hudDocument = GetComponent<UIDocument>();
        }

        private void OnDisable()
        {
            _bossButton?.RemoveFromHierarchy();
            _invasionButton?.RemoveFromHierarchy();
            _pill?.RemoveFromHierarchy();
            _bossButton = null;
            _invasionButton = null;
            _pill = null;
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

        /// <summary>The boss that is up now (first open window whose prize is not taken), else null.</summary>
        public static WorldBossDefinition OpenBoss(DateTime utc, WorldEventProgress progress, out RealTimeWindow window)
        {
            foreach (WorldBossDefinition boss in WorldBossRules.All)
            {
                RealTimeWindow w = WorldBossRules.WindowAt(boss, utc);
                if (w.IsOpen && (progress == null || progress.CanFightBoss(boss.Id, w)))
                {
                    window = w;
                    return boss;
                }
            }

            window = default;
            return null;
        }

        /// <summary>The next boss to show up (soonest window start in the future).</summary>
        public static WorldBossDefinition NextBoss(DateTime utc, out RealTimeWindow window)
        {
            WorldBossDefinition best = null;
            window = default;
            foreach (WorldBossDefinition boss in WorldBossRules.All)
            {
                RealTimeWindow w = WorldBossRules.WindowAt(boss, utc);
                RealTimeWindow next = w.IsOpen ? WorldEventCalendar.WindowAt(boss.Schedule, w.EndUtc.AddSeconds(1)) : w;
                if (best == null || next.StartUtc < window.StartUtc)
                {
                    best = boss;
                    window = next;
                }
            }

            return best;
        }

        public static string BossLine(LocalizationManager localization, DateTime utc, WorldEventProgress progress)
        {
            WorldBossDefinition open = OpenBoss(utc, progress, out RealTimeWindow window);
            if (open != null)
            {
                bool night = WorldBossRules.IsNightWindow(window);
                return WorldBossTexts.Format(localization, WorldBossTexts.MapBossOpen, WorldBossTexts.BossName(localization, open, night),
                    WorldBossTexts.RegionIn(localization, open.Region), WorldEventCalendar.FormatCountdown(window.SecondsToChange));
            }

            WorldBossDefinition next = NextBoss(utc, out RealTimeWindow nextWindow);
            bool nextNight = WorldBossRules.IsNightWindow(nextWindow);
            return WorldBossTexts.Format(localization, WorldBossTexts.MapBossNext, WorldBossTexts.BossName(localization, next, nextNight),
                WorldBossTexts.RegionIn(localization, next.Region), WorldEventCalendar.FormatCountdown((nextWindow.StartUtc - utc).TotalSeconds));
        }

        public static string InvasionLine(LocalizationManager localization, DateTime utc, WorldEventProgress progress)
        {
            RealTimeWindow window = InvasionRules.WindowAt(utc);
            if (window.IsOpen && (progress == null || progress.CanDefend(window)))
            {
                return WorldBossTexts.Format(localization, WorldBossTexts.MapInvasionOpen,
                    WorldBossTexts.RegionIn(localization, InvasionRules.RegionFor(window.Index)), WorldEventCalendar.FormatCountdown(window.SecondsToChange));
            }

            RealTimeWindow next = window.IsOpen ? InvasionRules.WindowAt(window.EndUtc.AddSeconds(1)) : window;
            return WorldBossTexts.Format(localization, WorldBossTexts.MapInvasionNext,
                WorldBossTexts.RegionIn(localization, InvasionRules.RegionFor(next.Index)), WorldEventCalendar.FormatCountdown((next.StartUtc - utc).TotalSeconds));
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

            DateTime now = WorldEventCalendar.UtcNow();
            LocalizationManager localization = Localization;
            WorldEventProgress progress = WorldEventProgress.ResolveShared();
            bool hudVisible = _hud != null && _hud.IsVisible;
            _pill.style.display = hudVisible ? DisplayStyle.Flex : DisplayStyle.None;

            SetText(_bossLine, BossLine(localization, now, progress));
            SetText(_invasionLine, InvasionLine(localization, now, progress));

            WorldBossDefinition openBoss = OpenBoss(now, progress, out RealTimeWindow bossWindow);
            RealTimeWindow invasion = InvasionRules.WindowAt(now);
            bool invasionOpen = invasion.IsOpen && progress.CanDefend(invasion);
            _bossLine.EnableInClassList("we-map-line--open", openBoss != null);
            _invasionLine.EnableInClassList("we-map-line--open", invasionOpen);

            Announce(openBoss, bossWindow, invasionOpen ? invasion : (RealTimeWindow?)null);

            ServiceLocator.TryGet(out DialogueManager dialogue);
            bool canAct = hudVisible && _worldMap != null && _worldMap.CanExplore && !GameplayPause.IsPaused && (dialogue == null || !dialogue.IsActive);
            BiomeType? here = _worldMap != null ? _worldMap.CurrentRegion : null;

            WorldBossDefinition bossHere = here.HasValue ? WorldBossRules.For(here.Value) : null;
            RealTimeWindow hereWindow = bossHere != null ? WorldBossRules.WindowAt(bossHere, now) : default;
            bool showBoss = canAct && bossHere != null && progress.CanFightBoss(bossHere.Id, hereWindow);
            _bossButton.style.display = showBoss ? DisplayStyle.Flex : DisplayStyle.None;
            if (showBoss)
            {
                SetText(_bossButton, WorldBossTexts.Format(localization, WorldBossTexts.MapFightButton,
                    WorldBossTexts.BossName(localization, bossHere, WorldBossRules.IsNightWindow(hereWindow))));
            }

            bool showInvasion = canAct && invasionOpen && here.HasValue && InvasionRules.RegionFor(invasion.Index) == here.Value;
            _invasionButton.style.display = showInvasion ? DisplayStyle.Flex : DisplayStyle.None;
            if (showInvasion)
            {
                SetText(_invasionButton, WorldBossTexts.Get(localization, WorldBossTexts.MapDefendButton));
            }

            _message.style.display = hudVisible && _messageLeft > 0f ? DisplayStyle.Flex : DisplayStyle.None;
        }

        /// <summary>A cousin's line when a window opens while the map is up (once per window; the first refresh only primes).</summary>
        private void Announce(WorldBossDefinition openBoss, RealTimeWindow bossWindow, RealTimeWindow? invasion)
        {
            bool primed = _announcedBossWindow != long.MinValue;
            long bossIndex = openBoss != null ? bossWindow.Index : -1;
            string bossId = openBoss?.Id;
            if (bossIndex != _announcedBossWindow || bossId != _announcedBossId)
            {
                if (primed && openBoss != null)
                {
                    ShowMessage(WorldEventDirector.CousinLine(WorldEventMoment.BossAppears));
                }

                _announcedBossWindow = bossIndex;
                _announcedBossId = bossId;
            }

            long invasionIndex = invasion.HasValue ? invasion.Value.Index : -1;
            if (invasionIndex != _announcedInvasionWindow)
            {
                if (_announcedInvasionWindow != long.MinValue && invasion.HasValue)
                {
                    ShowMessage(WorldEventDirector.CousinLine(WorldEventMoment.InvasionAppears));
                }

                _announcedInvasionWindow = invasionIndex;
            }
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
                _pill = new VisualElement { name = PillName, pickingMode = PickingMode.Ignore };
                _pill.AddToClassList("we-map-pill");
                WorldEventStyles.ApplyTo(_pill);
                var card = new VisualElement { pickingMode = PickingMode.Ignore };
                card.AddToClassList("we-map-card");
                _bossLine = new Label { pickingMode = PickingMode.Ignore };
                _bossLine.AddToClassList("we-map-line");
                _invasionLine = new Label { pickingMode = PickingMode.Ignore };
                _invasionLine.AddToClassList("we-map-line");
                _message = new Label { pickingMode = PickingMode.Ignore };
                _message.AddToClassList("we-map-message");
                _message.style.display = DisplayStyle.None;
                card.Add(_bossLine);
                card.Add(_invasionLine);
                card.Add(_message);
                _pill.Add(card);
                root.Add(_pill);
            }

            _bossButton = EnsureButton(row, _bossButton, BossButtonName, OnBossClicked);
            _invasionButton = EnsureButton(row, _invasionButton, InvasionButtonName, OnInvasionClicked);
            return true;
        }

        private static Button EnsureButton(VisualElement row, Button existing, string name, Action onClick)
        {
            if (existing != null && existing.parent == row)
            {
                return existing;
            }

            existing?.RemoveFromHierarchy();
            var button = new Button(onClick) { name = name, focusable = true };
            button.AddToClassList("wm-explore-button");
            button.AddToClassList(RegionNpcTalkButton.ButtonClass);
            button.AddToClassList("we-map-button");
            WorldEventStyles.ApplyTo(button);
            button.style.marginLeft = 8f;
            button.style.display = DisplayStyle.None;
            row.Add(button);
            return button;
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

        private bool CanClick()
        {
            if (GameplayPause.IsPaused || (ServiceLocator.TryGet(out DialogueManager dialogue) && dialogue.IsActive))
            {
                return false;
            }

            if (_worldMap == null)
            {
                _worldMap = FindFirstObjectByType<WorldMapController>();
            }

            return _worldMap != null && _worldMap.CanExplore && _worldMap.CurrentRegion.HasValue;
        }

        private void OnBossClicked()
        {
            if (!CanClick())
            {
                return;
            }

            LocalizationManager localization = Localization;
            BiomeType region = _worldMap.CurrentRegion.Value;
            WorldBossDefinition boss = WorldBossRules.For(region);
            RealTimeWindow window = WorldBossRules.WindowAt(boss, WorldEventCalendar.UtcNow());
            WorldEventProgress progress = WorldEventProgress.ResolveShared();
            if (!window.IsOpen)
            {
                ShowMessage(WorldBossTexts.Get(localization, WorldBossTexts.MapClosedNow));
                return;
            }

            if (!progress.CanFightBoss(boss.Id, window))
            {
                ShowMessage(WorldBossTexts.Get(localization, WorldBossTexts.MapAlreadyDone));
                return;
            }

            if (HeroLevel() < boss.MinLevel)
            {
                ShowMessage(WorldBossTexts.Format(localization, WorldBossTexts.MapNeedLevel, boss.MinLevel));
                return;
            }

            Launch(WorldEventDirector.BuildPayload(WorldEventKind.WorldBoss, region, window.Index, WorldBossRules.IsNightWindow(window), System.Environment.TickCount));
        }

        private void OnInvasionClicked()
        {
            if (!CanClick())
            {
                return;
            }

            LocalizationManager localization = Localization;
            RealTimeWindow window = InvasionRules.WindowAt(WorldEventCalendar.UtcNow());
            BiomeType region = _worldMap.CurrentRegion.Value;
            if (!window.IsOpen || InvasionRules.RegionFor(window.Index) != region)
            {
                ShowMessage(WorldBossTexts.Get(localization, WorldBossTexts.MapClosedNow));
                return;
            }

            if (!WorldEventProgress.ResolveShared().CanDefend(window))
            {
                ShowMessage(WorldBossTexts.Get(localization, WorldBossTexts.MapAlreadyDone));
                return;
            }

            if (HeroLevel() < InvasionRules.MinLevel)
            {
                ShowMessage(WorldBossTexts.Format(localization, WorldBossTexts.MapNeedLevel, InvasionRules.MinLevel));
                return;
            }

            Launch(WorldEventDirector.BuildPayload(WorldEventKind.Invasion, region, window.Index, WorldEventCalendar.IsNight(window.StartUtc), System.Environment.TickCount));
        }

        private void Launch(EncounterPayload payload)
        {
            if (payload == null || !_worldMap.TryStartEventRoom(payload))
            {
                ShowMessage(WorldBossTexts.Get(Localization, WorldBossTexts.MapClosedNow));
            }
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

    /// <summary>Loads Resources/UI/WorldEvents.uss (cached) and attaches it with the theme class.</summary>
    public static class WorldEventStyles
    {
        public const string ResourcePath = "UI/WorldEvents";
        public const string ThemeClass = "rap-theme";

        private static StyleSheet _sheet;

        public static StyleSheet Sheet => _sheet != null ? _sheet : (_sheet = Resources.Load<StyleSheet>(ResourcePath));

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
