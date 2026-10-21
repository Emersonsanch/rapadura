using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.Skills;
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
        [Tooltip("Renderer of the placeholder player visual (capsule) — tinted with the chosen character's AccentColor on confirm, since there are no per-character models yet.")]
        [SerializeField] private Renderer _playerVisualRenderer;
        [Tooltip("Drives the 3D turntable preview shown next to the roster (see detail-preview Image, bound to its RenderTexture).")]
        [SerializeField] private CharacterPreviewController _previewController;
        [Tooltip("RenderTexture the preview camera renders into — assigned to the detail-preview Image element on bind.")]
        [SerializeField] private RenderTexture _previewRenderTexture;

        [Header("Events")]
        public UnityEvent<CharacterId> OnCharacterSelected;

        private UIDocument _uiDocument;
        private VisualElement _root;
        private VisualElement _rosterContainer;
        private Image _previewImage;
        private Label _nameLabel;
        private Label _classRoleLabel;
        private Label _loreLabel;
        private Button _confirmButton;

        private CharacterId? _hovered;
        private bool _isBound;

        private void Awake()
        {
            _uiDocument = GetComponent<UIDocument>();
        }

        // Element binding happens in OnEnable rather than Awake: UIDocument builds its
        // rootVisualElement from visualTreeAsset during its own OnEnable, and script execution
        // order between components on the same GameObject is not guaranteed, so querying the
        // UXML tree in Awake can run before it exists and silently find nothing (this is the
        // convention already used by HudController/MainMenuController for the same reason).
        private void OnEnable()
        {
            Time.timeScale = 0f;
            BindElementsIfNeeded();
        }

        private void BindElementsIfNeeded()
        {
            if (_isBound)
            {
                return;
            }

            _root = _uiDocument.rootVisualElement;

            _rosterContainer = _root.Q<VisualElement>("roster-container");
            _previewImage = _root.Q<Image>("detail-preview");
            _nameLabel = _root.Q<Label>("detail-name");
            _classRoleLabel = _root.Q<Label>("detail-class-role");
            _loreLabel = _root.Q<Label>("detail-lore");
            _confirmButton = _root.Q<Button>("confirm-button");

            if (_previewImage != null && _previewRenderTexture != null)
            {
                _previewImage.image = _previewRenderTexture;

                // Belt-and-suspenders against flexbox collapsing this element to near-zero height
                // (observed with just the USS class alone) — force the fixed square size inline.
                _previewImage.style.width = 220;
                _previewImage.style.height = 220;
                _previewImage.style.minHeight = 220;
                _previewImage.style.flexShrink = 0;
                _previewImage.style.flexGrow = 0;
            }

            BuildRoster();

            if (_confirmButton != null)
            {
                _confirmButton.SetEnabled(false);
                _confirmButton.clicked += ConfirmSelection;
            }

            _isBound = true;
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

            _previewController?.ShowCharacter(character.Id);

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

                // Each hero brings their own 5 signature skills (Resources/Skills/Characters/<Id>):
                // the 4 actives are learned + equipped right away, the Ultimate unlocks later.
                if (_playerStats.TryGetComponent(out SkillManager skillManager))
                {
                    CharacterSkillSet.GrantTo(skillManager, id, _playerStats.Level);
                }
            }

            if (_playerVisualRenderer != null && !character.HasDedicatedModel)
            {
                // Instance (not shared) material so tinting one character never affects another,
                // or the shared asset other systems might reference. Skipped for characters that
                // already have a real textured model (HasDedicatedModel) — a flat tint would just
                // wash out the texture instead of helping.
                _playerVisualRenderer.material.color = character.AccentColor;
                _playerVisualRenderer.material.SetColor("_EmissionColor", character.AccentColor * 0.25f);
                _playerVisualRenderer.material.EnableKeyword("_EMISSION");
            }

            OnCharacterSelected?.Invoke(id);

            _previewController?.Clear();
            Time.timeScale = 1f;
            gameObject.SetActive(false);
        }

        public void SetPlayerStats(PlayerStats playerStats)
        {
            _playerStats = playerStats;
        }

        public void SetPlayerVisualRenderer(Renderer playerVisualRenderer)
        {
            _playerVisualRenderer = playerVisualRenderer;
        }
    }
}
