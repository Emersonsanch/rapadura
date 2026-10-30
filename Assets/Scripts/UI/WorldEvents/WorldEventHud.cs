using System.Collections.Generic;
using Rapadura.Core.DI;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.EventRooms;
using Rapadura.Gameplay.WorldEvents;
using Rapadura.UI.EventRooms;
using Rapadura.UI.HUD;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rapadura.UI.WorldEvents
{
    /// <summary>
    /// Fight HUD of the world bosses and the Invasão do Cangaço: title (boss name / "Invasão do Cangaço"), timer, the
    /// boss's (or the Capitão's) health bar, points and chest tier so far, the objective, a banner (cousin lines, waves,
    /// "fúria") and the results card. Self-bootstrapping like <c>EventRoomHud</c>: one DontDestroyOnLoad host whose
    /// UIDocument borrows the HUD's PanelSettings; it polls <see cref="WorldEventDirector.Current"/> and hides itself when
    /// no world event runs. Picking-mode Ignore throughout (never blocks input).
    /// </summary>
    public class WorldEventHud : MonoBehaviour
    {
        public const float SortingOrder = 560f;
        public const string LayerName = "we-layer";
        private const float RefreshInterval = 0.1f;
        private const float LowTimeSeconds = 30f;

        private static WorldEventHud _instance;

        private UIDocument _document;
        private VisualElement _layer;
        private VisualElement _hud;
        private Label _title;
        private Label _timer;
        private Label _points;
        private Label _goal;
        private VisualElement _bar;
        private VisualElement _fill;
        private VisualElement _bannerRoot;
        private Label _banner;
        private VisualElement _results;
        private VisualElement _resultsCard;
        private float _refresh;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (_instance != null || FindFirstObjectByType<WorldEventHud>() != null)
            {
                return;
            }

            var host = new GameObject(nameof(WorldEventHud));
            host.AddComponent<UIDocument>();
            host.AddComponent<WorldEventHud>();
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

        /// <summary>Lines of the results card (title, chest, points, rewards…, cousin line in quotes last).</summary>
        public static List<string> ResultLines(LocalizationManager localization, WorldEventResult result)
        {
            var lines = new List<string>();
            if (result == null)
            {
                return lines;
            }

            if (result.Kind == WorldEventKind.Invasion)
            {
                lines.Add(WorldBossTexts.Get(localization, result.Won ? WorldBossTexts.ResultInvasionWin : WorldBossTexts.ResultInvasionEnded));
                lines.Add(result.ChestTier > 0
                    ? WorldBossTexts.Format(localization, WorldBossTexts.ChestName, result.ChestTier)
                    : WorldBossTexts.Get(localization, WorldBossTexts.ResultNoChest));
                lines.Add(WorldBossTexts.Format(localization, WorldBossTexts.ResultPoints, result.Points));
            }
            else
            {
                lines.Add(WorldBossTexts.Format(localization, result.Won ? WorldBossTexts.ResultBossWin : WorldBossTexts.ResultBossLeft, result.BossName));
                lines.Add(WorldBossTexts.Get(localization, WorldBossTexts.WorldBossTitle));
            }

            if (result.Won || result.ChestTier > 0)
            {
                if (!result.Paid)
                {
                    lines.Add(WorldBossTexts.Get(localization, WorldBossTexts.ResultAlreadyPaid));
                }
                else
                {
                    lines.Add(WorldBossTexts.Format(localization, WorldBossTexts.ResultRewards, result.Experience, result.Gold));
                    if (result.BlessingStones > 0)
                    {
                        lines.Add(WorldBossTexts.Format(localization, WorldBossTexts.ResultStones, result.BlessingStones, EventRoomTexts.Get(localization, EventRoomTexts.StoneBlessing)));
                    }

                    if (result.SoulStones > 0)
                    {
                        lines.Add(WorldBossTexts.Format(localization, WorldBossTexts.ResultStones, result.SoulStones, EventRoomTexts.Get(localization, EventRoomTexts.StoneSoul)));
                    }

                    if (!string.IsNullOrEmpty(result.HouseItemName))
                    {
                        lines.Add(WorldBossTexts.Format(localization, WorldBossTexts.ResultHouseItem, result.HouseItemName));
                    }

                    if (!string.IsNullOrEmpty(result.MountName))
                    {
                        lines.Add(WorldBossTexts.Format(localization, WorldBossTexts.ResultMount, result.MountName));
                    }
                }
            }

            if (result.BagOverflow)
            {
                lines.Add(WorldBossTexts.Get(localization, WorldBossTexts.ResultBagFull));
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
            _refresh += Time.unscaledDeltaTime;
            if (_refresh < RefreshInterval)
            {
                return;
            }

            _refresh = 0f;
            WorldEventDirector director = WorldEventDirector.Current;
            if (director == null || director.Kind == WorldEventKind.None)
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

        private void Draw(WorldEventDirector director, LocalizationManager localization)
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
            bool invasion = director.Kind == WorldEventKind.Invasion;
            _title.text = invasion
                ? WorldBossTexts.Get(localization, WorldBossTexts.InvasionTitle)
                : WorldBossTexts.BossName(localization, director.Boss, director.IsNight);
            _timer.text = EventRoomHud.FormatClock(director.TimeLeft);
            _timer.EnableInClassList("we-hud-timer--low", director.HasStarted && director.TimeLeft <= LowTimeSeconds);

            _points.style.display = invasion ? DisplayStyle.Flex : DisplayStyle.None;
            if (invasion)
            {
                _points.text = WorldBossTexts.Format(localization, WorldBossTexts.HudPoints, director.Points, InvasionRules.TierFor(director.Points));
            }

            bool bar = !invasion || director.CaptainSpawned;
            _bar.style.display = bar ? DisplayStyle.Flex : DisplayStyle.None;
            if (bar)
            {
                _fill.style.width = Length.Percent(director.BossHealthFraction * 100f);
            }

            _goal.text = director.GoalText(localization);
            string banner = director.Banner;
            _bannerRoot.style.display = string.IsNullOrEmpty(banner) ? DisplayStyle.None : DisplayStyle.Flex;
            _banner.text = banner ?? string.Empty;
        }

        private void BuildResults(List<string> lines)
        {
            for (int i = 0; i < lines.Count; i++)
            {
                var label = new Label(lines[i]) { pickingMode = PickingMode.Ignore };
                label.AddToClassList(i == 0 ? "we-results-title" : i == 1 ? "we-results-tier" : lines[i].StartsWith("“") ? "we-results-quote" : "we-results-line");
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
            layer.AddToClassList("we-layer");
            WorldEventStyles.ApplyTo(layer);

            var hud = Element("we-hud");
            var card = Element("we-hud-card");
            card.Add(Text("we-hud-title"));
            card.Add(Text("we-hud-timer"));
            var bar = Element("we-hud-bar");
            bar.Add(Element("we-hud-fill"));
            card.Add(bar);
            card.Add(Text("we-hud-points"));
            card.Add(Text("we-hud-goal"));
            hud.Add(card);
            layer.Add(hud);

            var banner = Element("we-banner");
            banner.Add(Text("we-banner-text"));
            layer.Add(banner);

            var results = Element("we-results");
            results.Add(Element("we-results-card"));
            results.style.display = DisplayStyle.None;
            layer.Add(results);
            return layer;
        }

        private void Bind(VisualElement layer)
        {
            _hud = layer.Q("we-hud");
            _title = layer.Q<Label>("we-hud-title");
            _timer = layer.Q<Label>("we-hud-timer");
            _bar = layer.Q("we-hud-bar");
            _fill = layer.Q("we-hud-fill");
            _points = layer.Q<Label>("we-hud-points");
            _goal = layer.Q<Label>("we-hud-goal");
            _bannerRoot = layer.Q("we-banner");
            _banner = layer.Q<Label>("we-banner-text");
            _results = layer.Q("we-results");
            _resultsCard = layer.Q("we-results-card");
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
