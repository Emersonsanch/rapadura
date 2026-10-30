using System;
using System.Collections.Generic;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Inventory;
using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.Skills;
using Rapadura.Gameplay.Talents;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rapadura.UI.Talents
{
    /// <summary>
    /// The "Roda de Talentos" window, built in code inside the Ficha do personagem's root (it hides the sheet's window while
    /// open, so the sheet's FocusNavigator only walks the wheel). A painted wheel of four quadrants (Ataque, Defesa, Apoio,
    /// Utilidade) with the cousin's 16 nodes as 56px buttons on three rings; tapping a node selects it (name, bonus,
    /// rank, what it still needs), "Aprender" spends a point and "Recomeçar" resets the wheel for gold from the carried bag.
    /// Every rule goes through <see cref="TalentProgress"/>; layout/state/text come from <see cref="TalentWheelPresenter"/>.
    /// </summary>
    public sealed class TalentWheelView
    {
        public const string StyleResourcePath = "UI/Talents";
        public const string RootName = "talent-wheel-window";

        private static readonly Color[] QuadrantColors =
        {
            new Color(0.62f, 0.24f, 0.16f, 0.55f),
            new Color(0.42f, 0.30f, 0.18f, 0.55f),
            new Color(0.30f, 0.46f, 0.22f, 0.55f),
            new Color(0.70f, 0.55f, 0.22f, 0.45f),
        };

        private readonly VisualElement _host;
        private readonly VisualElement _hiddenWhileOpen;
        private readonly Func<LocalizationManager> _localization;
        private readonly Func<(PlayerStats Stats, BuffController Buffs, InventoryManager Bag)> _player;
        private readonly Dictionary<string, Button> _nodeButtons = new Dictionary<string, Button>();

        private VisualElement _window;
        private Label _title;
        private Button _close;
        private VisualElement _wheel;
        private Label _intro;
        private Label _points;
        private Label _quadrants;
        private Label _detailName;
        private Label _detail;
        private Label _status;
        private Button _spend;
        private Button _respec;
        private CharacterId? _builtFor;
        private string _selected;

        public bool IsOpen { get; private set; }

        /// <param name="host">The sheet's rap-theme root.</param>
        /// <param name="hiddenWhileOpen">The sheet's own window (hidden while the wheel is up).</param>
        public TalentWheelView(VisualElement host, VisualElement hiddenWhileOpen, Func<LocalizationManager> localization,
            Func<(PlayerStats, BuffController, InventoryManager)> player)
        {
            _host = host;
            _hiddenWhileOpen = hiddenWhileOpen;
            _localization = localization;
            _player = player;
        }

        public Button FirstFocusable()
        {
            foreach (Button button in _nodeButtons.Values)
            {
                return button;
            }

            return _close;
        }

        public void Open()
        {
            if (_host == null)
            {
                return;
            }

            if (_window == null)
            {
                Build();
            }

            IsOpen = true;
            if (_hiddenWhileOpen != null)
            {
                _hiddenWhileOpen.style.display = DisplayStyle.None;
            }

            _window.style.display = DisplayStyle.Flex;
            _status.text = string.Empty;
            Refresh();
            FirstFocusable()?.Focus();
        }

        public void Close()
        {
            if (!IsOpen)
            {
                return;
            }

            IsOpen = false;
            if (_window != null)
            {
                _window.style.display = DisplayStyle.None;
            }

            if (_hiddenWhileOpen != null)
            {
                _hiddenWhileOpen.style.display = DisplayStyle.Flex;
            }
        }

        // ------------------------------------------------------------------ build

        private void Build()
        {
            StyleSheet sheet = Resources.Load<StyleSheet>(StyleResourcePath);
            if (sheet != null && !_host.styleSheets.Contains(sheet))
            {
                _host.styleSheets.Add(sheet);
            }

            _window = new VisualElement { name = RootName };
            _window.AddToClassList("talent-window");
            _window.AddToClassList("rap-panel");

            var header = new VisualElement();
            header.AddToClassList("talent-header");
            _title = NewLabel("talent-title");
            _title.AddToClassList("rap-heading");
            header.Add(_title);
            _close = new Button(Close) { name = "talent-close-button" };
            _close.AddToClassList("talent-close-button");
            _close.AddToClassList("rap-button");
            header.Add(_close);
            _window.Add(header);

            var body = new VisualElement();
            body.AddToClassList("talent-body");
            _wheel = new VisualElement { name = "talent-wheel" };
            _wheel.AddToClassList("talent-wheel");
            _wheel.generateVisualContent += PaintWheel;
            body.Add(_wheel);

            var side = new VisualElement();
            side.AddToClassList("talent-side");
            _intro = NewLabel("talent-intro");
            _points = NewLabel("talent-points");
            _quadrants = NewLabel("talent-quadrant-summary");
            _detailName = NewLabel("talent-detail-name");
            _detail = NewLabel("talent-detail");
            _status = NewLabel("talent-status");
            side.Add(_intro);
            side.Add(_points);
            side.Add(_quadrants);
            side.Add(_detailName);
            side.Add(_detail);
            var actions = new VisualElement();
            actions.AddToClassList("talent-actions");
            _spend = new Button(OnSpend) { name = "talent-spend-button" };
            _spend.AddToClassList("talent-spend-button");
            _spend.AddToClassList("rap-button");
            _respec = new Button(OnRespec) { name = "talent-respec-button" };
            _respec.AddToClassList("talent-respec-button");
            _respec.AddToClassList("rap-button");
            actions.Add(_spend);
            actions.Add(_respec);
            side.Add(actions);
            side.Add(_status);
            body.Add(side);
            _window.Add(body);

            _window.style.display = DisplayStyle.None;
            _host.Add(_window);
        }

        private static Label NewLabel(string className)
        {
            var label = new Label();
            label.AddToClassList(className);
            label.pickingMode = PickingMode.Ignore;
            return label;
        }

        private static void PaintWheel(MeshGenerationContext context)
        {
            Painter2D painter = context.painter2D;
            var center = new Vector2(TalentWheelPresenter.WheelSize / 2f, TalentWheelPresenter.WheelSize / 2f);
            float radius = TalentWheelPresenter.WheelSize / 2f - 4f;
            for (int q = 0; q < 4; q++)
            {
                // Arc angles are clockwise from +x (screen y points down); quadrant q spans 90q..90q+90 clockwise from up.
                painter.BeginPath();
                painter.MoveTo(center);
                painter.Arc(center, radius, Angle.Degrees(90f * q - 90f), Angle.Degrees(90f * q));
                painter.ClosePath();
                painter.fillColor = QuadrantColors[q];
                painter.Fill();
            }

            painter.strokeColor = new Color(0.89f, 0.62f, 0.27f, 0.6f);
            painter.lineWidth = 2f;
            foreach (float ring in new[] { TalentWheelPresenter.Ring1Radius, TalentWheelPresenter.Ring2Radius, TalentWheelPresenter.KeystoneRadius })
            {
                painter.BeginPath();
                painter.Arc(center, ring, Angle.Degrees(0f), Angle.Degrees(360f));
                painter.Stroke();
            }

            for (int q = 0; q < 4; q++)
            {
                painter.BeginPath();
                painter.MoveTo(center);
                painter.LineTo(TalentWheelPresenter.Polar(90f * q, radius));
                painter.Stroke();
            }
        }

        private void BuildNodes(CharacterId hero)
        {
            foreach (Button button in _nodeButtons.Values)
            {
                button.RemoveFromHierarchy();
            }

            _nodeButtons.Clear();
            _wheel.Query<Label>(className: "talent-quadrant-label").ForEach(label => label.RemoveFromHierarchy());

            foreach (TalentQuadrant quadrant in (TalentQuadrant[])Enum.GetValues(typeof(TalentQuadrant)))
            {
                Label label = NewLabel("talent-quadrant-label");
                label.userData = quadrant;
                Vector2 at = TalentWheelPresenter.LabelCenter(quadrant);
                label.style.left = at.x - 60f;
                label.style.top = at.y - 10f;
                _wheel.Add(label);
            }

            foreach ((TalentNodeSpec node, int index) in TalentWheelPresenter.Layout(hero))
            {
                float size = node.IsKeystone ? 64f : 56f;
                Vector2 at = TalentWheelPresenter.NodeCenter(node, index);
                string id = node.NodeId;
                var button = new Button(() => Select(id)) { name = "talent-node-" + id };
                button.AddToClassList("talent-node");
                button.EnableInClassList("talent-node--keystone", node.IsKeystone);
                button.style.left = at.x - size / 2f;
                button.style.top = at.y - size / 2f;
                _wheel.Add(button);
                _nodeButtons[id] = button;
            }

            _builtFor = hero;
            _selected = null;
        }

        // ------------------------------------------------------------------ refresh

        public void Refresh()
        {
            if (_window == null)
            {
                return;
            }

            LocalizationManager manager = _localization?.Invoke();
            _title.text = TalentTexts.Get(manager, TalentTexts.Title);
            _close.text = TalentTexts.Get(manager, TalentTexts.Close);
            _spend.text = TalentTexts.Get(manager, TalentTexts.SpendButton);
            _intro.text = TalentTexts.Format(manager, TalentTexts.Intro, TalentWheelCatalog.UnlockLevel, TalentWheelCatalog.KeystoneRequiredPoints);

            if (!TalentProgress.TryActiveHero(out CharacterId hero))
            {
                _points.text = TalentTexts.Get(manager, TalentTexts.NoHero);
                _spend.SetEnabled(false);
                _respec.SetEnabled(false);
                return;
            }

            if (_builtFor != hero)
            {
                BuildNodes(hero);
            }

            TalentProgress progress = TalentProgress.ResolveShared();
            TalentRecord record = progress.Record(hero);
            int level = _player?.Invoke().Stats is PlayerStats stats && stats != null ? stats.Level : 1;

            _points.text = TalentWheelRules.IsUnlocked(level)
                ? TalentTexts.Format(manager, TalentTexts.Points, TalentWheelRules.AvailablePoints(record, level))
                : TalentTexts.Format(manager, TalentTexts.LockedLevel, TalentWheelCatalog.UnlockLevel);

            var summary = new List<string>();
            foreach (TalentQuadrant quadrant in (TalentQuadrant[])Enum.GetValues(typeof(TalentQuadrant)))
            {
                summary.Add(TalentTexts.Format(manager, TalentTexts.QuadrantPoints, TalentTexts.Get(manager, TalentTexts.QuadrantKey(quadrant)),
                    TalentWheelRules.SpentIn(record, quadrant)));
            }

            _quadrants.text = string.Join("  ·  ", summary);
            _wheel.Query<Label>(className: "talent-quadrant-label").ForEach(label =>
            {
                if (label.userData is TalentQuadrant q)
                {
                    label.text = TalentTexts.Get(manager, TalentTexts.QuadrantKey(q));
                }
            });

            foreach (KeyValuePair<string, Button> pair in _nodeButtons)
            {
                TalentNodeSpec node = TalentWheelCatalog.GetNode(pair.Key);
                TalentNodeState state = TalentWheelPresenter.State(record, hero, level, node);
                Button button = pair.Value;
                button.text = TalentWheelPresenter.RankText(record, node, manager);
                button.tooltip = TalentTexts.Get(manager, node.NameKey) + " — " + TalentWheelPresenter.BonusText(node, manager);
                button.EnableInClassList("talent-node--locked", state == TalentNodeState.Locked);
                button.EnableInClassList("talent-node--available", state == TalentNodeState.Available);
                button.EnableInClassList("talent-node--learned", state == TalentNodeState.Learned);
                button.EnableInClassList("talent-node--maxed", state == TalentNodeState.Maxed);
                button.EnableInClassList("talent-node--selected", pair.Key == _selected);
            }

            TalentNodeSpec selected = _selected != null ? TalentWheelCatalog.GetNode(_selected) : null;
            if (selected != null)
            {
                string name = TalentTexts.Get(manager, selected.NameKey);
                _detailName.text = selected.IsKeystone ? name + " (" + TalentTexts.Get(manager, TalentTexts.Keystone) + ")" : name;
                string requirement = TalentWheelPresenter.RequirementText(record, hero, level, selected, manager);
                _detail.text = TalentWheelPresenter.BonusText(selected, manager) + "  " + TalentWheelPresenter.RankText(record, selected, manager)
                    + (string.IsNullOrEmpty(requirement) ? string.Empty : "\n" + requirement);
                _spend.SetEnabled(progress.CanSpend(hero, level, selected.NodeId) == TalentSpendResult.Spent);
            }
            else
            {
                _detailName.text = string.Empty;
                _detail.text = string.Empty;
                _spend.SetEnabled(false);
            }

            int cost = progress.RespecCost(hero);
            _respec.text = TalentTexts.Format(manager, TalentTexts.Respec, cost);
            _respec.SetEnabled(cost > 0);
        }

        private void Select(string nodeId)
        {
            _selected = nodeId;
            _status.text = string.Empty;
            Refresh();
        }

        private void OnSpend()
        {
            if (_selected == null || !TalentProgress.TryActiveHero(out CharacterId hero))
            {
                return;
            }

            (PlayerStats stats, BuffController buffs, InventoryManager _) = _player != null ? _player() : default;
            TalentProgress.ResolveShared().Spend(hero, stats != null ? stats.Level : 1, _selected, buffs);
            Refresh();
            if (!_spend.enabledSelf && _nodeButtons.TryGetValue(_selected, out Button node))
            {
                node.Focus();
            }
        }

        private void OnRespec()
        {
            if (!TalentProgress.TryActiveHero(out CharacterId hero))
            {
                return;
            }

            LocalizationManager manager = _localization?.Invoke();
            (PlayerStats _, BuffController buffs, InventoryManager bag) = _player != null ? _player() : default;
            int cost = TalentProgress.ResolveShared().RespecCost(hero);
            TalentRespecResult result = TalentProgress.ResolveShared().Respec(hero, bag, out int refunded, buffs);
            switch (result)
            {
                case TalentRespecResult.Done:
                    _status.text = TalentTexts.Format(manager, TalentTexts.RespecDone, refunded);
                    break;
                case TalentRespecResult.NotEnoughGold:
                    _status.text = TalentTexts.Format(manager, TalentTexts.RespecNoGold, cost);
                    break;
                default:
                    _status.text = TalentTexts.Get(manager, TalentTexts.RespecNothing);
                    break;
            }

            Refresh();
            if (!_respec.enabledSelf)
            {
                FirstFocusable()?.Focus();
            }
        }
    }
}
