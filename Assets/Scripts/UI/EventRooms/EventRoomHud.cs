using System.Collections.Generic;
using Rapadura.Core.DI;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.EventRooms;
using Rapadura.UI.HUD;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rapadura.UI.EventRooms
{
    /// <summary>
    /// In-room HUD of the timed event rooms: room name, timer (red in the last 20 s), points, live combo, the objective
    /// (Torre: the phase, with the gate's HP bar), a banner for cousin lines / phase news, and the results card when the
    /// run ends. Self-bootstrapping like <c>NightDangerIndicator</c>: one DontDestroyOnLoad host whose UIDocument borrows
    /// the HUD's PanelSettings; it polls <see cref="EventRoomDirector.Current"/> a few times per second (real time)
    /// and is hidden whenever no room is running. Picking-mode Ignore throughout (never blocks input).
    /// </summary>
    public class EventRoomHud : MonoBehaviour
    {
        public const float SortingOrder = 560f;
        public const string LayerName = "er-layer";
        private const float RefreshInterval = 0.1f;
        private const float LowTimeSeconds = 20f;

        private static EventRoomHud _instance;

        private UIDocument _document;
        private VisualElement _layer;
        private VisualElement _hud;
        private Label _title;
        private Label _timer;
        private Label _score;
        private Label _combo;
        private Label _goal;
        private VisualElement _gateBar;
        private VisualElement _gateFill;
        private VisualElement _bannerRoot;
        private Label _banner;
        private VisualElement _results;
        private VisualElement _resultsCard;
        private float _timerRefresh;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (_instance != null || FindFirstObjectByType<EventRoomHud>() != null)
            {
                return;
            }

            var host = new GameObject(nameof(EventRoomHud));
            host.AddComponent<UIDocument>();
            host.AddComponent<EventRoomHud>();
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            if (transform.parent == null)
            {
                DontDestroyOnLoad(gameObject);
            }

            _document = GetComponent<UIDocument>() ?? gameObject.AddComponent<UIDocument>();
            _document.sortingOrder = SortingOrder;
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }

        private void OnDisable()
        {
            _layer = null;
        }

        // ------------------------------------------------------------------
        // Pure helpers (tested)
        // ------------------------------------------------------------------

        /// <summary>"2:05" for a number of seconds (rounded up, so 0:00 only when it is really over).</summary>
        public static string FormatClock(float seconds)
        {
            int total = Mathf.Max(0, Mathf.CeilToInt(seconds));
            return $"{total / 60}:{total % 60:00}";
        }

        /// <summary>Lines of the results card (title, tier, score, best, stats, rewards, stones, bag, cousin).</summary>
        public static List<string> ResultLines(LocalizationManager localization, EventRoomResult result)
        {
            var lines = new List<string>();
            if (result == null)
            {
                return lines;
            }

            string room = EventRoomTexts.Get(localization, EventRoomTexts.RoomNameKey(result.Kind));
            lines.Add(EventRoomTexts.Format(localization, result.Cleared ? EventRoomTexts.ResultTitleCleared : EventRoomTexts.ResultTitleEnded, room));
            lines.Add(EventRoomTexts.Format(localization, EventRoomTexts.ResultTier, EventRoomTexts.Get(localization, EventRoomTexts.TierKey(result.Tier))));
            lines.Add(EventRoomTexts.Format(localization, EventRoomTexts.ResultScore, result.Score));
            lines.Add(result.NewBest
                ? EventRoomTexts.Get(localization, EventRoomTexts.ResultNewBest)
                : EventRoomTexts.Format(localization, EventRoomTexts.ResultBest, result.BestScore));
            lines.Add(EventRoomTexts.Format(localization, EventRoomTexts.ResultStats, result.Kills, result.BestCombo));
            if (result.Reward.IsEmpty)
            {
                lines.Add(EventRoomTexts.Get(localization, EventRoomTexts.ResultNoReward));
            }
            else
            {
                lines.Add(EventRoomTexts.Format(localization, EventRoomTexts.ResultRewards, result.Reward.Experience, result.Reward.Gold));
                if (result.BlessingStonesGranted > 0)
                {
                    lines.Add(EventRoomTexts.Format(localization, EventRoomTexts.ResultStones, result.BlessingStonesGranted, EventRoomTexts.Get(localization, EventRoomTexts.StoneBlessing)));
                }

                if (result.SoulStonesGranted > 0)
                {
                    lines.Add(EventRoomTexts.Format(localization, EventRoomTexts.ResultStones, result.SoulStonesGranted, EventRoomTexts.Get(localization, EventRoomTexts.StoneSoul)));
                }
            }

            if (result.BagOverflow)
            {
                lines.Add(EventRoomTexts.Get(localization, EventRoomTexts.ResultBagFull));
            }

            if (!string.IsNullOrEmpty(result.CousinLine))
            {
                lines.Add("“" + result.CousinLine + "”");
            }

            return lines;
        }

        // ------------------------------------------------------------------
        // Loop
        // ------------------------------------------------------------------

        private void Update()
        {
            _timerRefresh += Time.unscaledDeltaTime;
            if (_timerRefresh < RefreshInterval)
            {
                return;
            }

            _timerRefresh = 0f;
            EventRoomDirector director = EventRoomDirector.Current;
            if (director == null || director.Kind == EventRoomKind.None)
            {
                if (_layer != null)
                {
                    _layer.style.display = DisplayStyle.None;
                }

                return;
            }

            if (!EnsureBound())
            {
                return;
            }

            _layer.style.display = DisplayStyle.Flex;
            ServiceLocator.TryGet(out LocalizationManager localization);
            Draw(director, localization);
        }

        private void Draw(EventRoomDirector director, LocalizationManager localization)
        {
            bool finished = director.IsFinished && director.Result != null;
            _hud.style.display = finished ? DisplayStyle.None : DisplayStyle.Flex;
            _results.style.display = finished ? DisplayStyle.Flex : DisplayStyle.None;

            if (finished)
            {
                if (_resultsCard.childCount == 0)
                {
                    BuildResults(ResultLines(localization, director.Result));
                }

                _bannerRoot.style.display = DisplayStyle.None;
                return;
            }

            _resultsCard.Clear();
            _title.text = EventRoomTexts.Get(localization, EventRoomTexts.RoomNameKey(director.Kind));
            _timer.text = EventRoomTexts.Format(localization, EventRoomTexts.HudTimer, FormatClock(director.TimeLeft));
            _timer.EnableInClassList("er-hud-timer--low", director.HasStarted && director.TimeLeft <= LowTimeSeconds);
            _score.text = EventRoomTexts.Format(localization, EventRoomTexts.HudScore, director.Score.Total);

            bool comboAlive = director.Score.IsComboAlive(director.Elapsed) && director.Score.Combo > 1;
            _combo.style.display = comboAlive ? DisplayStyle.Flex : DisplayStyle.None;
            if (comboAlive)
            {
                float multiplier = EventRoomRules.ComboMultiplier(director.Score.Combo);
                _combo.text = EventRoomTexts.Format(localization, EventRoomTexts.HudCombo, director.Score.Combo, "×" + multiplier.ToString("0.0"));
            }

            _goal.text = director.GoalText(localization);
            bool gate = director.Kind == EventRoomKind.Torre && director.Phase == TorrePhase.BreakGate && director.Gate != null;
            _gateBar.style.display = gate ? DisplayStyle.Flex : DisplayStyle.None;
            if (gate)
            {
                _gateFill.style.width = Length.Percent(director.Gate.Fraction * 100f);
            }

            string banner = director.Banner;
            _bannerRoot.style.display = string.IsNullOrEmpty(banner) ? DisplayStyle.None : DisplayStyle.Flex;
            _banner.text = banner ?? string.Empty;
        }

        private void BuildResults(List<string> lines)
        {
            for (int i = 0; i < lines.Count; i++)
            {
                var label = new Label(lines[i]) { pickingMode = PickingMode.Ignore };
                if (i == 0)
                {
                    label.AddToClassList("er-results-title");
                }
                else if (i == 1)
                {
                    label.AddToClassList("er-results-tier");
                }
                else if (lines[i].StartsWith("“"))
                {
                    label.AddToClassList("er-results-quote");
                }
                else
                {
                    label.AddToClassList("er-results-line");
                    if (i == 3)
                    {
                        label.AddToClassList("er-results-best");
                    }
                }

                _resultsCard.Add(label);
            }
        }

        // ------------------------------------------------------------------
        // View
        // ------------------------------------------------------------------

        private bool EnsureBound()
        {
            if (_document == null)
            {
                return false;
            }

            if (_document.panelSettings == null)
            {
                PanelSettings donor = FindPanelSettings();
                if (donor == null)
                {
                    return false;
                }

                _document.panelSettings = donor;
                _document.sortingOrder = SortingOrder;
            }

            VisualElement root = _document.rootVisualElement;
            if (root == null)
            {
                return false;
            }

            if (_layer != null && _layer.parent == root)
            {
                return true;
            }

            root.pickingMode = PickingMode.Ignore;
            _layer = CreateView();
            Bind(_layer);
            root.Add(_layer);
            return true;
        }

        /// <summary>The whole tree (hidden parts included), picking-mode Ignore throughout. EditMode-testable.</summary>
        public static VisualElement CreateView()
        {
            var layer = new VisualElement { name = LayerName, pickingMode = PickingMode.Ignore };
            layer.AddToClassList("er-layer");
            EventRoomStyles.ApplyTo(layer);

            var hud = Element("er-hud");
            var card = Element("er-hud-card");
            card.Add(Text("er-hud-title"));
            card.Add(Text("er-hud-timer"));
            var row = Element("er-hud-row");
            row.Add(Text("er-hud-score"));
            row.Add(Text("er-hud-combo"));
            card.Add(row);
            card.Add(Text("er-hud-goal"));
            var bar = Element("er-hud-gate-bar");
            bar.Add(Element("er-hud-gate-fill"));
            card.Add(bar);
            hud.Add(card);
            layer.Add(hud);

            var banner = Element("er-banner");
            banner.Add(Text("er-banner-text"));
            layer.Add(banner);

            var results = Element("er-results");
            results.Add(Element("er-results-card"));
            results.style.display = DisplayStyle.None;
            layer.Add(results);
            return layer;
        }

        private void Bind(VisualElement layer)
        {
            _hud = layer.Q("er-hud");
            _title = layer.Q<Label>("er-hud-title");
            _timer = layer.Q<Label>("er-hud-timer");
            _score = layer.Q<Label>("er-hud-score");
            _combo = layer.Q<Label>("er-hud-combo");
            _goal = layer.Q<Label>("er-hud-goal");
            _gateBar = layer.Q("er-hud-gate-bar");
            _gateFill = layer.Q("er-hud-gate-fill");
            _bannerRoot = layer.Q("er-banner");
            _banner = layer.Q<Label>("er-banner-text");
            _results = layer.Q("er-results");
            _resultsCard = layer.Q("er-results-card");
        }

        private static VisualElement Element(string name)
        {
            var element = new VisualElement { name = name, pickingMode = PickingMode.Ignore };
            element.AddToClassList(name);
            return element;
        }

        private static Label Text(string name)
        {
            var label = new Label { name = name, pickingMode = PickingMode.Ignore };
            label.AddToClassList(name);
            return label;
        }

        private PanelSettings FindPanelSettings()
        {
            HudController hud = FindFirstObjectByType<HudController>();
            UIDocument hudDocument = hud != null ? hud.GetComponent<UIDocument>() : null;
            if (hudDocument != null && hudDocument.panelSettings != null)
            {
                return hudDocument.panelSettings;
            }

            foreach (UIDocument document in FindObjectsByType<UIDocument>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (document != _document && document.panelSettings != null)
                {
                    return document.panelSettings;
                }
            }

            return null;
        }
    }
}
