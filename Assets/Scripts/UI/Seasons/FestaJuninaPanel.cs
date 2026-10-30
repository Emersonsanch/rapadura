using System.Collections.Generic;
using Rapadura.Core.Accessibility;
using Rapadura.Core.DI;
using Rapadura.Core.Localization;
using Rapadura.Core.Systems;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Housing;
using Rapadura.Gameplay.Seasons;
using Rapadura.Save;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace Rapadura.UI.Seasons
{
    /// <summary>
    /// The "O Bema da Vó Edna" window of the Bema (the class keeps its old Festa Junina name; opened by <see cref="FestaJuninaButton"/> in the Vila): two
    /// pages, the <b>Hinos do Bema</b> (Master Xroshtag calls, the player answers: the rhythm game of <see cref="QuadrilhaSession"/>: notes slide to the hit zone,
    /// answer with ←↑↓→ / WASD / d-pad or the four buttons) and the <b>Tenda das Oferendas</b> (the Bema's house items
    /// for offerings, <see cref="FestaJuninaProgress.TryBuy"/>). The selected cousin comments at the top
    /// (<see cref="FestaJuninaTexts.CousinLine"/>). Built in code on the map HUD's document with
    /// <c>Resources/UI/FestaJunina.uss</c>, on unscaled time, holding <see cref="GameplayPause"/> while open.
    /// </summary>
    [DisallowMultipleComponent]
    public class FestaJuninaPanel : MonoBehaviour
    {
        public const string OverlayName = "wm-festa-overlay";
        public const string StyleResourcePath = "UI/FestaJunina";
        public const string RelaxedPrefKey = "festa.quadrilha.relaxed";
        public const string BuyButtonPrefix = "wm-festa-buy-";

        private UIDocument _document;
        private VisualElement _overlay;
        private Label _title;
        private Label _subtitle;
        private Label _prendas;
        private Label _line;
        private Button _tabDance;
        private Button _tabStall;
        private VisualElement _dancePage;
        private VisualElement _stallPage;
        private Label _danceIntro;
        private Toggle _relaxed;
        private VisualElement _track;
        private Label _call;
        private Label _judge;
        private Label _score;
        private readonly List<Button> _moveButtons = new List<Button>();
        private Button _start;
        private Label _result;
        private Label _stallIntro;
        private Label _tasks;
        private ScrollView _stallList;
        private Label _status;
        private Button _close;
        private readonly List<Label> _notes = new List<Label>();

        private QuadrilhaSession _session;
        private bool _open;
        private bool _stallTab;
        private string _judgeClass;

        public bool IsOpen => _open;
        public QuadrilhaSession Session => _session;
        public bool IsDancing => _session != null && !_session.IsFinished;

        private static LocalizationManager Localization => ServiceLocator.TryGet(out LocalizationManager localization) ? localization : null;
        private static CharacterId? Hero => ServiceLocator.TryGet(out CharacterSelectionState selection) && selection != null ? selection.Selected : null;
        private static bool ReduceMotion => ServiceLocator.TryGet(out AccessibilitySettings settings) && settings != null && settings.ReduceMotion;

        private void Awake()
        {
            _document = GetComponent<UIDocument>();
        }

        private void OnDisable()
        {
            Close();
            _overlay?.RemoveFromHierarchy();
            _overlay = null;
        }

        public void Open()
        {
            if (_open || !EnsureOverlay())
            {
                return;
            }

            _open = true;
            _session = null;
            _stallTab = false;
            _status.text = string.Empty;
            _result.text = string.Empty;
            LocalizationManager localization = Localization;
            bool again = SeasonCalendar.DayKey(SeasonCalendar.Today) % 2 == 1;
            _line.text = FestaJuninaTexts.CousinLine(localization, Hero, again ? FestaLineKind.GreetAgain : FestaLineKind.Greet);
            _overlay.style.display = DisplayStyle.Flex;
            GameplayPause.Acquire(this);
            RefreshTexts();
            ShowTab(false);
        }

        public void Close()
        {
            if (_overlay != null)
            {
                _overlay.style.display = DisplayStyle.None;
            }

            if (_session != null && !_session.IsFinished)
            {
                // Walking away from the hymn: no offerings for half a hymn.
                _session = null;
            }

            _open = false;
            GameplayPause.Release(this);
        }

        // ------------------------------------------------------------------
        // Build
        // ------------------------------------------------------------------

        private bool EnsureOverlay()
        {
            if (_document == null || _document.rootVisualElement == null)
            {
                return false;
            }

            VisualElement root = _document.rootVisualElement.Q<VisualElement>("wm-hud-root") ?? _document.rootVisualElement;
            if (_overlay != null && _overlay.parent == root)
            {
                return true;
            }

            _overlay?.RemoveFromHierarchy();
            _overlay = new VisualElement { name = OverlayName };
            _overlay.AddToClassList("wm-backpack-overlay");
            _overlay.style.display = DisplayStyle.None;
            var sheet = Resources.Load<StyleSheet>(StyleResourcePath);
            if (sheet != null)
            {
                _overlay.styleSheets.Add(sheet);
            }

            _overlay.RegisterCallback<NavigationMoveEvent>(evt =>
            {
                if (IsDancing)
                {
                    evt.StopPropagation();
                }
            }, TrickleDown.TrickleDown);

            var panel = new VisualElement { name = "wm-festa-panel" };
            panel.AddToClassList("rap-panel");
            panel.AddToClassList("wm-backpack-panel");
            panel.AddToClassList("festa-panel");
            _overlay.Add(panel);

            _title = AddLabel(panel, "wm-backpack-title");
            _subtitle = AddLabel(panel, "wm-backpack-hint");
            _prendas = AddLabel(panel, "festa-prendas");
            _line = AddLabel(panel, "festa-line");

            var tabs = new VisualElement();
            tabs.AddToClassList("festa-tabs");
            panel.Add(tabs);
            _tabDance = NewButton("wm-festa-tab-dance", "festa-tab", () => ShowTab(false));
            _tabStall = NewButton("wm-festa-tab-stall", "festa-tab", () => ShowTab(true));
            tabs.Add(_tabDance);
            tabs.Add(_tabStall);

            // Quadrilha page.
            _dancePage = new VisualElement { name = "wm-festa-dance" };
            _dancePage.AddToClassList("festa-page");
            panel.Add(_dancePage);
            _danceIntro = AddLabel(_dancePage, "festa-intro");
            _relaxed = new Toggle { name = "wm-festa-relaxed", value = PlayerPrefs.GetInt(RelaxedPrefKey, 0) != 0 };
            _relaxed.AddToClassList("festa-relaxed");
            _relaxed.RegisterValueChangedCallback(evt => PlayerPrefs.SetInt(RelaxedPrefKey, evt.newValue ? 1 : 0));
            _dancePage.Add(_relaxed);
            _track = new VisualElement { name = "wm-festa-track" };
            _track.AddToClassList("festa-track");
            var hit = new VisualElement();
            hit.AddToClassList("festa-hit-zone");
            _track.Add(hit);
            _dancePage.Add(_track);
            _call = AddLabel(_dancePage, "festa-call");
            _judge = AddLabel(_dancePage, "festa-judge");
            _score = AddLabel(_dancePage, "festa-score");

            var moves = new VisualElement();
            moves.AddToClassList("festa-moves");
            _dancePage.Add(moves);
            foreach (QuadrilhaMove move in new[] { QuadrilhaMove.Anarrie, QuadrilhaMove.Balance, QuadrilhaMove.Cobra, QuadrilhaMove.CaminhoDaRoca })
            {
                QuadrilhaMove captured = move;
                Button button = NewButton("wm-festa-move-" + move.ToString().ToLowerInvariant(), "festa-move", () => Press(captured));
                moves.Add(button);
                _moveButtons.Add(button);
            }

            _result = AddLabel(_dancePage, "festa-result");
            _start = NewButton("wm-festa-start", "rap-button--primary", StartDance);
            _start.style.alignSelf = Align.Center;
            _dancePage.Add(_start);

            // Tenda das Oferendas page.
            _stallPage = new VisualElement { name = "wm-festa-stall" };
            _stallPage.AddToClassList("festa-page");
            panel.Add(_stallPage);
            _stallIntro = AddLabel(_stallPage, "festa-intro");
            _tasks = AddLabel(_stallPage, "festa-tasks");
            _stallList = new ScrollView(ScrollViewMode.Vertical) { name = "wm-festa-stall-list" };
            _stallList.AddToClassList("festa-stall-list");
            _stallPage.Add(_stallList);

            _status = AddLabel(panel, "festa-status");
            _close = NewButton("wm-festa-close", "wm-backpack-close", Close);
            _close.AddToClassList("rap-button--primary");
            panel.Add(_close);

            root.Add(_overlay);
            return true;
        }

        private static Label AddLabel(VisualElement parent, string className)
        {
            var label = new Label();
            label.AddToClassList(className);
            parent.Add(label);
            return label;
        }

        private static Button NewButton(string name, string className, System.Action onClick)
        {
            var button = new Button(onClick) { name = name, focusable = true };
            button.AddToClassList("rap-button");
            button.AddToClassList(className);
            return button;
        }

        // ------------------------------------------------------------------
        // Texts / pages
        // ------------------------------------------------------------------

        private void RefreshTexts()
        {
            LocalizationManager localization = Localization;
            FestaJuninaProgress progress = FestaJuninaProgress.ResolveShared();
            _title.text = FestaJuninaTexts.Get(localization, FestaJuninaTexts.Title);
            _subtitle.text = FestaJuninaTexts.Get(localization, FestaJuninaTexts.Subtitle);
            _prendas.text = FestaJuninaTexts.Format(localization, FestaJuninaTexts.Prendas, progress.Prendas);
            _tabDance.text = FestaJuninaTexts.Get(localization, FestaJuninaTexts.TabDance);
            _tabStall.text = FestaJuninaTexts.Get(localization, FestaJuninaTexts.TabStall);
            _danceIntro.text = FestaJuninaTexts.Get(localization, FestaJuninaTexts.DanceIntro);
            _relaxed.label = FestaJuninaTexts.Get(localization, FestaJuninaTexts.DanceRelaxed);
            _stallIntro.text = FestaJuninaTexts.Get(localization, FestaJuninaTexts.StallIntro);
            _close.text = FestaJuninaTexts.Get(localization, FestaJuninaTexts.Close);
            _start.text = FestaJuninaTexts.Get(localization, _session != null ? FestaJuninaTexts.DanceAgain : FestaJuninaTexts.DanceStart);
            QuadrilhaMove[] moves = { QuadrilhaMove.Anarrie, QuadrilhaMove.Balance, QuadrilhaMove.Cobra, QuadrilhaMove.CaminhoDaRoca };
            for (int i = 0; i < _moveButtons.Count; i++)
            {
                _moveButtons[i].text = FestaJuninaTexts.Get(localization, FestaJuninaTexts.MoveKey(moves[i]));
            }
        }

        private void ShowTab(bool stall)
        {
            if (stall && IsDancing)
            {
                return;
            }

            _stallTab = stall;
            _dancePage.style.display = stall ? DisplayStyle.None : DisplayStyle.Flex;
            _stallPage.style.display = stall ? DisplayStyle.Flex : DisplayStyle.None;
            _tabDance.EnableInClassList("festa-tab--active", !stall);
            _tabStall.EnableInClassList("festa-tab--active", stall);
            if (stall)
            {
                RebuildStall();
            }
            else
            {
                RenderDance();
                _start.Focus();
            }
        }

        private void RebuildStall()
        {
            LocalizationManager localization = Localization;
            FestaJuninaProgress progress = FestaJuninaProgress.ResolveShared();
            bool active = SeasonCalendar.IsFestaJuninaActive;
            _prendas.text = FestaJuninaTexts.Format(localization, FestaJuninaTexts.Prendas, progress.Prendas);
            _tasks.text = FestaJuninaPresenter.TasksText(localization, progress);
            _stallList.Clear();
            Button first = null;
            foreach (HouseItemDefinition item in FestaJuninaRules.StallItems)
            {
                PrizeRow row = FestaJuninaPresenter.BuildPrizeRow(item, progress.Prendas, active, progress.HasBought(item.ItemId), localization);
                var element = new VisualElement { name = "wm-festa-prize-" + item.ItemId };
                element.AddToClassList("festa-prize");
                element.EnableInClassList("festa-prize--owned", row.Owned);
                var body = new VisualElement();
                body.AddToClassList("festa-prize__body");
                element.Add(body);
                var name = new Label(row.Name);
                name.AddToClassList("festa-prize__name");
                body.Add(name);
                var info = new Label(row.Info);
                info.AddToClassList("festa-prize__info");
                body.Add(info);
                string id = item.ItemId;
                Button buy = NewButton(BuyButtonPrefix + id, "festa-buy", () => Buy(id));
                buy.text = row.ButtonText;
                buy.SetEnabled(row.CanBuy);
                element.Add(buy);
                _stallList.Add(element);
                if (first == null && row.CanBuy)
                {
                    first = buy;
                }
            }

            (first ?? _close).Focus();
        }

        private void Buy(string itemId)
        {
            LocalizationManager localization = Localization;
            FestaJuninaProgress progress = FestaJuninaProgress.ResolveShared();
            bool hadLegendary = progress.LegendaryGranted;
            PrendaBuyCheck check = progress.TryBuy(itemId);
            HouseItemDefinition item = FestaJuninaRules.StallItem(itemId);
            if (check == PrendaBuyCheck.Ok)
            {
                _status.text = FestaJuninaTexts.Format(localization, FestaJuninaTexts.StallBought, HouseItemTexts.Name(localization, item));
                _line.text = FestaJuninaTexts.CousinLine(localization, Hero, FestaLineKind.Stall);
                AnnounceLegendary(hadLegendary, progress, localization);
                Save();
            }
            else if (check == PrendaBuyCheck.NotEnoughPrendas && item != null)
            {
                _status.text = FestaJuninaTexts.Format(localization, FestaJuninaTexts.StallNotEnough, item.EventPrice);
            }
            else if (check == PrendaBuyCheck.FestaClosed)
            {
                _status.text = FestaJuninaTexts.Get(localization, FestaJuninaTexts.StallClosed);
            }

            RebuildStall();
        }

        private void AnnounceLegendary(bool hadLegendary, FestaJuninaProgress progress, LocalizationManager localization)
        {
            if (!hadLegendary && progress.LegendaryGranted)
            {
                HouseItemDefinition legendary = HouseCatalog.Get(FestaJuninaRules.LegendaryItemId);
                _status.text += "\n" + FestaJuninaTexts.Format(localization, FestaJuninaTexts.LegendaryGranted, HouseItemTexts.Name(localization, legendary));
            }
        }

        private static void Save()
        {
            if (ServiceLocator.TryGet(out SaveManager saveManager))
            {
                saveManager.AutoSave();
            }
        }

        // ------------------------------------------------------------------
        // Quadrilha
        // ------------------------------------------------------------------

        private void StartDance()
        {
            if (!_open || IsDancing)
            {
                return;
            }

            bool relaxed = _relaxed.value || ReduceMotion;
            _session = new QuadrilhaSession(SeasonCalendar.DayKey(SeasonCalendar.Today), QuadrilhaSession.DefaultSteps, relaxed);
            _result.text = string.Empty;
            _status.text = string.Empty;
            SetJudge(QuadrilhaJudgement.None);
            _call.text = FestaJuninaTexts.Get(Localization, FestaJuninaTexts.DanceGetReady);
            EnsureNotes(_session.StepCount);
            _start.style.display = DisplayStyle.None;
            _tabStall.SetEnabled(false);
            _track.focusable = true;
            _track.Focus();
        }

        private void EnsureNotes(int count)
        {
            while (_notes.Count < count)
            {
                var note = new Label();
                note.AddToClassList("festa-note");
                note.pickingMode = PickingMode.Ignore;
                _track.Add(note);
                _notes.Add(note);
            }

            for (int i = 0; i < _notes.Count; i++)
            {
                Label note = _notes[i];
                note.style.display = DisplayStyle.None;
                note.ClearClassList();
                note.AddToClassList("festa-note");
                if (i < count)
                {
                    QuadrilhaMove move = _session.Steps[i].Move;
                    note.text = FestaJuninaPresenter.Arrow(move);
                    note.AddToClassList(FestaJuninaPresenter.NoteClass(move));
                }
            }
        }

        private void Update()
        {
            if (!_open || _session == null || _session.IsFinished)
            {
                return;
            }

            ReadInput();
            _session.Tick(Time.unscaledDeltaTime);
            if (_session.LastJudgement != QuadrilhaJudgement.None && _session.LastJudgement != _lastShown)
            {
                SetJudge(_session.LastJudgement);
            }

            RenderDance();
            if (_session.IsFinished)
            {
                FinishDance();
            }
        }

        private QuadrilhaJudgement _lastShown;
        private int _judgedShown;

        private void ReadInput()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard != null)
            {
                if (keyboard.leftArrowKey.wasPressedThisFrame || keyboard.aKey.wasPressedThisFrame) Press(QuadrilhaMove.Anarrie);
                if (keyboard.upArrowKey.wasPressedThisFrame || keyboard.wKey.wasPressedThisFrame) Press(QuadrilhaMove.Balance);
                if (keyboard.downArrowKey.wasPressedThisFrame || keyboard.sKey.wasPressedThisFrame) Press(QuadrilhaMove.Cobra);
                if (keyboard.rightArrowKey.wasPressedThisFrame || keyboard.dKey.wasPressedThisFrame) Press(QuadrilhaMove.CaminhoDaRoca);
            }

            Gamepad gamepad = Gamepad.current;
            if (gamepad != null)
            {
                if (gamepad.dpad.left.wasPressedThisFrame) Press(QuadrilhaMove.Anarrie);
                if (gamepad.dpad.up.wasPressedThisFrame) Press(QuadrilhaMove.Balance);
                if (gamepad.dpad.down.wasPressedThisFrame) Press(QuadrilhaMove.Cobra);
                if (gamepad.dpad.right.wasPressedThisFrame) Press(QuadrilhaMove.CaminhoDaRoca);
            }
        }

        private void Press(QuadrilhaMove move)
        {
            if (!IsDancing)
            {
                return;
            }

            QuadrilhaJudgement judgement = _session.Press(move);
            if (judgement != QuadrilhaJudgement.None)
            {
                SetJudge(judgement);
            }
        }

        private void SetJudge(QuadrilhaJudgement judgement)
        {
            _lastShown = judgement;
            if (_judgeClass != null)
            {
                _judge.RemoveFromClassList(_judgeClass);
            }

            _judgeClass = FestaJuninaPresenter.JudgeClass(judgement);
            if (_judgeClass != null)
            {
                _judge.AddToClassList(_judgeClass);
            }

            _judge.text = FestaJuninaTexts.Get(Localization, FestaJuninaTexts.JudgeKey(judgement));
            _judgedShown = _session != null ? _session.Perfects + _session.Goods + _session.Misses : 0;
            if (judgement == QuadrilhaJudgement.None)
            {
                _lastShown = QuadrilhaJudgement.None;
            }
        }

        private void RenderDance()
        {
            LocalizationManager localization = Localization;
            if (_session == null)
            {
                _score.text = string.Empty;
                _call.text = string.Empty;
                foreach (Label note in _notes)
                {
                    note.style.display = DisplayStyle.None;
                }

                _start.style.display = DisplayStyle.Flex;
                return;
            }

            // A miss of the same kind twice in a row still flashes: re-show when the judged count moves.
            int judged = _session.Perfects + _session.Goods + _session.Misses;
            if (judged != _judgedShown && _session.LastJudgement != QuadrilhaJudgement.None)
            {
                SetJudge(_session.LastJudgement);
            }

            _score.text = FestaJuninaTexts.Format(localization, FestaJuninaTexts.DanceScore, _session.Score, _session.MaxScore, _session.Combo);
            QuadrilhaStep next = _session.NextStep;
            if (next != null && _session.Time >= _session.LeadIn - _session.Beat)
            {
                _call.text = FestaJuninaTexts.Get(localization, FestaJuninaTexts.MoveKey(next.Move));
            }

            for (int i = 0; i < _notes.Count; i++)
            {
                Label note = _notes[i];
                if (i >= _session.Steps.Count || _session.Steps[i].IsJudged)
                {
                    note.style.display = DisplayStyle.None;
                    continue;
                }

                float? percent = FestaJuninaPresenter.NotePercent(_session.Steps[i].Time, _session.Time);
                note.style.display = percent.HasValue ? DisplayStyle.Flex : DisplayStyle.None;
                if (percent.HasValue)
                {
                    note.style.left = new Length(percent.Value, LengthUnit.Percent);
                }
            }
        }

        private void FinishDance()
        {
            LocalizationManager localization = Localization;
            FestaJuninaProgress progress = FestaJuninaProgress.ResolveShared();
            bool hadLegendary = progress.LegendaryGranted;
            QuadrilhaReward reward = progress.RecordDance(_session.Score, _session.MaxScore, _session.IsFullCombo);
            string text = FestaJuninaTexts.Format(localization, FestaJuninaTexts.DanceResult, _session.Score, _session.MaxScore, _session.Perfects,
                _session.Misses, reward.Prendas);
            if (_session.IsFullCombo)
            {
                text += "\n" + FestaJuninaTexts.Get(localization, FestaJuninaTexts.DanceFullCombo);
            }

            if (reward.FirstToday)
            {
                text += "\n" + FestaJuninaTexts.Format(localization, FestaJuninaTexts.DanceFirstToday, FestaJuninaRules.FirstDanceBonus);
            }

            _result.text = text;
            _call.text = string.Empty;
            _line.text = FestaJuninaTexts.CousinLine(localization, Hero,
                FestaJuninaTexts.DanceKind(_session.Score, _session.MaxScore, _session.IsFullCombo));
            _status.text = string.Empty;
            AnnounceLegendary(hadLegendary, progress, localization);
            _tabStall.SetEnabled(true);
            _start.style.display = DisplayStyle.Flex;
            RefreshTexts();
            _start.Focus();
            Save();
        }
    }
}
