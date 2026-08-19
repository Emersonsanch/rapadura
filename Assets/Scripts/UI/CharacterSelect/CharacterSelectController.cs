using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Player;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

namespace Rapadura.UI.CharacterSelect
{
    /// <summary>
    /// First screen the player sees: pick one of the 5 heroes from <see cref="CharacterRegistry"/>.
    /// Freezes <see cref="Time.timeScale"/> to 0 while shown (so the player GameObject sitting in the
    /// scene doesn't move/react before a choice is made) and calls
    /// <see cref="PlayableCharacter.ApplyPassive"/> on the target <see cref="PlayerStats"/> once
    /// selected, then hides itself and resumes time. Follows the same UI Toolkit pattern as
    /// <c>MainMenuController</c>/<c>HudController</c>: one <see cref="UIDocument"/>, elements
    /// resolved once via <c>Q&lt;T&gt;()</c>.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class CharacterSelectController : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("The player's PlayerStats — ApplyPassive is invoked on this once a character is chosen.")]
        [SerializeField] private PlayerStats _playerStats;

        [Header("Events")]
        public UnityEvent<CharacterId> OnCharacterSelected;

        private UIDocument _uiDocument;
        private VisualElement _root;
        private VisualElement _rosterContainer;
        private Label _nameLabel;
        private Label _classRoleLabel;
        private Label _loreLabel;
        private Button _confirmButton;

        private CharacterId? _hovered;

        private void Awake()
        {
            _uiDocument = GetComponent<UIDocument>();
            _root = _uiDocument.rootVisualElement;

            _rosterContainer = _root.Q<VisualElement>("roster-container");
            _nameLabel = _root.Q<Label>("detail-name");
            _classRoleLabel = _root.Q<Label>("detail-class-role");
            _loreLabel = _root.Q<Label>("detail-lore");
            _confirmButton = _root.Q<Button>("confirm-button");

            BuildRoster();

            if (_confirmButton != null)
            {
                _confirmButton.SetEnabled(false);
                _confirmButton.clicked += ConfirmSelection;
            }
        }

        private void OnEnable()
        {
            Time.timeScale = 0f;
        }

        private void BuildRoster()
        {
            if (_rosterContainer == null)
            {
                return;
            }

            _rosterContainer.Clear();

            foreach (PlayableCharacter character in CharacterRegistry.GetAll())
            {
                var button = new Button { text = character.DisplayName, name = $"character-{character.Id}" };
                button.AddToClassList("character-select__roster-button");
                button.clicked += () => ShowDetail(character);
                _rosterContainer.Add(button);
            }
        }

        private void ShowDetail(PlayableCharacter character)
        {
            _hovered = character.Id;

            if (_nameLabel != null) _nameLabel.text = character.DisplayName;
            if (_classRoleLabel != null) _classRoleLabel.text = $"{character.ClassName} — {character.Role}";
            if (_loreLabel != null) _loreLabel.text = character.Lore;

            if (_confirmButton != null)
            {
                _confirmButton.SetEnabled(true);
            }
        }

        private void ConfirmSelection()
        {
            if (_hovered is not CharacterId id)
            {
                return;
            }

            PlayableCharacter character = CharacterRegistry.Get(id);

            if (_playerStats != null)
            {
                character.ApplyPassive(_playerStats);
            }

            OnCharacterSelected?.Invoke(id);

            Time.timeScale = 1f;
            gameObject.SetActive(false);
        }

        public void SetPlayerStats(PlayerStats playerStats)
        {
            _playerStats = playerStats;
        }
    }
}
