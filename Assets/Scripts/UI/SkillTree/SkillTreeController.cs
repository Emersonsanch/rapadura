using System.Collections.Generic;
using Rapadura.Core.Events;
using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.Skills;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rapadura.UI.SkillTree
{
    /// <summary>
    /// Drives the runtime Skill Tree screen built with UI Toolkit (SkillTreeView.uxml/.uss).
    /// Lays out every node of the active <see cref="SkillTreeDefinition"/> on a grid, draws
    /// connector lines to prerequisite skills, colors nodes by state (locked/available/
    /// learned/maxed) and lets the player spend skill points through <see cref="SkillManager"/>.
    /// Purely a presentation layer — all rules (requirements, points, leveling) live in
    /// SkillManager/SkillInstance; this class only reads and displays that state.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class SkillTreeController : MonoBehaviour
    {
        private const float CellSize = 120f;
        private const float NodeRadius = 42f;

        [Header("References")]
        [SerializeField] private SkillManager _skillManager;
        [SerializeField] private PlayerStats _playerStats;

        [Header("Trees")]
        [SerializeField] private SkillTreeDefinition[] _availableTrees;

        private UIDocument _uiDocument;
        private VisualElement _root;
        private Label _treeTitleLabel;
        private Label _treeDescriptionLabel;
        private Label _skillPointsLabel;
        private Button _closeButton;
        private ScrollView _scrollView;
        private VisualElement _nodeLayer;
        private SkillTreeConnectorElement _connectorLayer;
        private Label _detailNameLabel;
        private Label _detailDescriptionLabel;
        private Label _detailStatsLabel;
        private Button _detailActionButton;

        private SkillTreeDefinition _activeTree;
        private SkillTreeNodeData _selectedNode;
        private readonly Dictionary<string, VisualElement> _nodeElementsBySkillId = new Dictionary<string, VisualElement>();

        private void Awake()
        {
            _uiDocument = GetComponent<UIDocument>();
        }

        private void OnEnable()
        {
            BindElements();
            RegisterCallbacks();

            if (_activeTree == null && _availableTrees != null && _availableTrees.Length > 0)
            {
                ShowTree(_availableTrees[0]);
            }

            Hide();
        }

        private void OnDisable()
        {
            UnregisterCallbacks();
        }

        private void BindElements()
        {
            _root = _uiDocument.rootVisualElement.Q<VisualElement>("skill-tree-root");
            _treeTitleLabel = _uiDocument.rootVisualElement.Q<Label>("tree-title-label");
            _treeDescriptionLabel = _uiDocument.rootVisualElement.Q<Label>("tree-description-label");
            _skillPointsLabel = _uiDocument.rootVisualElement.Q<Label>("skill-points-label");
            _closeButton = _uiDocument.rootVisualElement.Q<Button>("close-button");
            _scrollView = _uiDocument.rootVisualElement.Q<ScrollView>("skill-tree-scroll");
            _nodeLayer = _uiDocument.rootVisualElement.Q<VisualElement>("node-layer");
            _detailNameLabel = _uiDocument.rootVisualElement.Q<Label>("detail-name-label");
            _detailDescriptionLabel = _uiDocument.rootVisualElement.Q<Label>("detail-description-label");
            _detailStatsLabel = _uiDocument.rootVisualElement.Q<Label>("detail-stats-label");
            _detailActionButton = _uiDocument.rootVisualElement.Q<Button>("detail-action-button");

            VisualElement connectorHost = _uiDocument.rootVisualElement.Q<VisualElement>("connector-layer");
            _connectorLayer = new SkillTreeConnectorElement();
            _connectorLayer.style.position = Position.Absolute;
            _connectorLayer.style.left = 0;
            _connectorLayer.style.top = 0;
            _connectorLayer.style.right = 0;
            _connectorLayer.style.bottom = 0;
            connectorHost.Add(_connectorLayer);
        }

        private void RegisterCallbacks()
        {
            _closeButton.clicked += Hide;
            _detailActionButton.clicked += OnDetailActionClicked;

            if (_skillManager != null)
            {
                _skillManager.OnSkillPointsChanged += RefreshAll;
            }

            EventBus.Subscribe<SkillLearnedEvent>(OnSkillProgressionChanged);
            EventBus.Subscribe<SkillLeveledUpEvent>(OnSkillProgressionChanged);
        }

        private void UnregisterCallbacks()
        {
            _closeButton.clicked -= Hide;
            _detailActionButton.clicked -= OnDetailActionClicked;

            if (_skillManager != null)
            {
                _skillManager.OnSkillPointsChanged -= RefreshAll;
            }

            EventBus.Unsubscribe<SkillLearnedEvent>(OnSkillProgressionChanged);
            EventBus.Unsubscribe<SkillLeveledUpEvent>(OnSkillProgressionChanged);
        }

        private void OnSkillProgressionChanged(SkillLearnedEvent evt) => RefreshAll();
        private void OnSkillProgressionChanged(SkillLeveledUpEvent evt) => RefreshAll();

        public void Show()
        {
            _root.style.display = DisplayStyle.Flex;
            RefreshAll();
        }

        public void Hide()
        {
            _root.style.display = DisplayStyle.None;
        }

        public void Toggle()
        {
            bool isVisible = _root.style.display == DisplayStyle.Flex;

            if (isVisible)
            {
                Hide();
            }
            else
            {
                Show();
            }
        }

        public void ShowTree(SkillTreeDefinition tree)
        {
            _activeTree = tree;
            _selectedNode = null;
            BuildTree();
        }

        private void RefreshAll()
        {
            if (_activeTree == null)
            {
                return;
            }

            UpdateHeader();
            UpdateNodeStates();
            UpdateConnectors();
            UpdateDetailPanel();
        }

        private void UpdateHeader()
        {
            _treeTitleLabel.text = _activeTree.DisplayName;
            _treeDescriptionLabel.text = _activeTree.Description;
            int points = _skillManager != null ? _skillManager.AvailableSkillPoints : 0;
            _skillPointsLabel.text = $"Pontos disponíveis: {points}";
        }

        private void BuildTree()
        {
            _nodeLayer.Clear();
            _nodeElementsBySkillId.Clear();

            if (_activeTree == null)
            {
                return;
            }

            foreach (SkillTreeNodeData node in _activeTree.Nodes)
            {
                if (node?.skill == null)
                {
                    continue;
                }

                VisualElement nodeElement = CreateNodeElement(node);
                _nodeLayer.Add(nodeElement);
                _nodeElementsBySkillId[node.skill.SkillId] = nodeElement;
            }

            RefreshAll();
        }

        private VisualElement CreateNodeElement(SkillTreeNodeData node)
        {
            var button = new Button { name = $"node-{node.skill.SkillId}" };
            button.AddToClassList("skill-node");
            button.style.left = node.gridPosition.x * CellSize;
            button.style.top = node.gridPosition.y * CellSize;

            if (node.skill.Icon != null)
            {
                var icon = new Image { sprite = node.skill.Icon };
                icon.AddToClassList("skill-node-icon");
                button.Add(icon);
            }

            var levelLabel = new Label { name = "level-label" };
            levelLabel.AddToClassList("skill-node-level-label");
            button.Add(levelLabel);

            button.clicked += () => SelectNode(node);

            return button;
        }

        private void SelectNode(SkillTreeNodeData node)
        {
            _selectedNode = node;
            UpdateNodeStates();
            UpdateDetailPanel();
        }

        private int GetCasterLevel()
        {
            return _playerStats != null ? _playerStats.Level : 1;
        }

        private void UpdateNodeStates()
        {
            if (_skillManager == null || _activeTree == null)
            {
                return;
            }

            int casterLevel = GetCasterLevel();

            foreach (SkillTreeNodeData node in _activeTree.Nodes)
            {
                if (node?.skill == null || !_nodeElementsBySkillId.TryGetValue(node.skill.SkillId, out VisualElement element))
                {
                    continue;
                }

                element.RemoveFromClassList("skill-node--locked");
                element.RemoveFromClassList("skill-node--available");
                element.RemoveFromClassList("skill-node--learned");
                element.RemoveFromClassList("skill-node--maxlevel");
                element.RemoveFromClassList("skill-node--selected");

                SkillInstance instance = _skillManager.GetLearnedSkill(node.skill.SkillId);
                Label levelLabel = element.Q<Label>("level-label");

                if (instance != null)
                {
                    string stateClass = instance.IsMaxLevel ? "skill-node--maxlevel" : "skill-node--learned";
                    element.AddToClassList(stateClass);
                    levelLabel.text = $"{instance.Level}/{node.skill.MaxLevel}";
                }
                else
                {
                    bool canLearn = _skillManager.CanLearnSkill(node.skill, casterLevel);
                    element.AddToClassList(canLearn ? "skill-node--available" : "skill-node--locked");
                    levelLabel.text = $"0/{node.skill.MaxLevel}";
                }

                if (_selectedNode == node)
                {
                    element.AddToClassList("skill-node--selected");
                }
            }
        }

        private void UpdateConnectors()
        {
            if (_activeTree == null)
            {
                return;
            }

            var lines = new List<(Vector2, Vector2, Color)>();

            foreach (SkillTreeNodeData node in _activeTree.Nodes)
            {
                if (node?.skill == null)
                {
                    continue;
                }

                foreach (SkillDefinition required in node.skill.Requirement.requiredSkills)
                {
                    if (required == null || !_nodeElementsBySkillId.TryGetValue(required.SkillId, out VisualElement fromElement))
                    {
                        continue;
                    }

                    if (!_nodeElementsBySkillId.TryGetValue(node.skill.SkillId, out VisualElement toElement))
                    {
                        continue;
                    }

                    Vector2 from = new Vector2(fromElement.style.left.value.value, fromElement.style.top.value.value) + new Vector2(NodeRadius, NodeRadius);
                    Vector2 to = new Vector2(toElement.style.left.value.value, toElement.style.top.value.value) + new Vector2(NodeRadius, NodeRadius);

                    bool requiredIsLearned = _skillManager != null && _skillManager.IsSkillLearned(required.SkillId);
                    Color color = requiredIsLearned ? new Color(0.35f, 0.85f, 0.5f) : new Color(0.4f, 0.4f, 0.45f);

                    lines.Add((from, to, color));
                }
            }

            _connectorLayer.SetLines(lines);
        }

        private void UpdateDetailPanel()
        {
            if (_selectedNode?.skill == null)
            {
                _detailNameLabel.text = "Selecione uma skill";
                _detailDescriptionLabel.text = string.Empty;
                _detailStatsLabel.text = string.Empty;
                _detailActionButton.style.display = DisplayStyle.None;
                return;
            }

            SkillDefinition skill = _selectedNode.skill;
            SkillInstance instance = _skillManager != null ? _skillManager.GetLearnedSkill(skill.SkillId) : null;

            _detailNameLabel.text = skill.DisplayName;
            _detailDescriptionLabel.text = skill.Description;
            _detailStatsLabel.text = $"Custo: {skill.ManaCost} mana / {skill.EnergyCost} energia — Cooldown: {skill.CooldownSeconds}s";

            _detailActionButton.style.display = DisplayStyle.Flex;

            if (instance == null)
            {
                bool canLearn = _skillManager != null && _skillManager.CanLearnSkill(skill, GetCasterLevel());
                _detailActionButton.text = "Aprender (1 ponto)";
                _detailActionButton.SetEnabled(canLearn && _skillManager.AvailableSkillPoints > 0);
            }
            else if (!instance.IsMaxLevel)
            {
                _detailActionButton.text = "Evoluir (1 ponto)";
                _detailActionButton.SetEnabled(_skillManager.AvailableSkillPoints > 0);
            }
            else
            {
                _detailActionButton.text = "Nível máximo";
                _detailActionButton.SetEnabled(false);
            }
        }

        private void OnDetailActionClicked()
        {
            if (_selectedNode?.skill == null || _skillManager == null)
            {
                return;
            }

            SkillDefinition skill = _selectedNode.skill;
            SkillInstance instance = _skillManager.GetLearnedSkill(skill.SkillId);

            if (instance == null)
            {
                _skillManager.LearnSkillWithPoint(skill, GetCasterLevel());
            }
            else
            {
                _skillManager.LevelUpSkillWithPoint(skill.SkillId);
            }
        }
    }
}
