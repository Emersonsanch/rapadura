using System;
using System.Globalization;
using Rapadura.Core.DI;
using Rapadura.Core.Events;
using Rapadura.Core.Logging;
using Rapadura.Core.Systems;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Flow;
using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.Skills;
using Rapadura.Gameplay.World.Map;
using Rapadura.Save;
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
    ///
    /// The chosen hero is recorded in <see cref="CharacterSelectionState"/> (saved with the game).
    /// Whenever a hero is already chosen — a save containing one was loaded (see
    /// <see cref="CharacterSelectionRestoredEvent"/>), or this scene was re-entered after picking —
    /// the screen re-applies that hero to the player and closes itself instead of asking again. The
    /// optional "continue-saved-button" offers exactly that from the most recent save among
    /// <see cref="_continueSlots"/>.
    ///
    /// Game flow: the real place to pick a hero is WorldMap.unity, opened by "Novo jogo" on the title
    /// screen (<c>GameFrontEndController</c> / <see cref="GameFlowService"/>); without a player there
    /// it only records the choice. The copy in Main.unity is a dev convenience for opening that scene
    /// directly in the Editor — <see cref="GameFlowMachine.DecideCombatSceneSelect"/> guarantees it
    /// never shows during an encounter. Pausing goes through <see cref="GameplayPause"/> (the old
    /// direct Time.timeScale = 0 was undone by PauseMenuController.OnEnable, letting enemies attack
    /// behind this screen).
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

        [Header("Continue")]
        [Tooltip("Save slots checked for the 'Continuar com <herói>' shortcut; the most recently saved one that has a chosen hero wins. -1 is SaveManager.AutoSaveSlot.")]
        [SerializeField] private int[] _continueSlots = { SaveManager.AutoSaveSlot, 0 };

        [Header("Events")]
        public UnityEvent<CharacterId> OnCharacterSelected;

        private const int NoContinueSlot = int.MinValue;

        private UIDocument _uiDocument;
        private VisualElement _root;
        private VisualElement _rosterContainer;
        private Image _previewImage;
        private Label _nameLabel;
        private Label _classRoleLabel;
        private Label _loreLabel;
        private Button _confirmButton;
        private Button _continueButton;

        private CharacterId? _hovered;
        private int _continueSlot = NoContinueSlot;
        private bool _isBound;
        private bool _deactivatePending;

        private void Awake()
        {
            _uiDocument = GetComponent<UIDocument>();

            // Subscribed for the whole lifetime (not OnEnable/OnDisable): a restore usually arrives
            // while this screen is already hidden (loading from the pause menu), and the player
            // still needs its hero's visual/passive/skills re-applied then.
            EventBus.Subscribe<CharacterSelectionRestoredEvent>(OnCharacterSelectionRestored);
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<CharacterSelectionRestoredEvent>(OnCharacterSelectionRestored);
        }

        // Element binding happens in OnEnable rather than Awake: UIDocument builds its
        // rootVisualElement from visualTreeAsset during its own OnEnable, and script execution
        // order between components on the same GameObject is not guaranteed, so querying the
        // UXML tree in Awake can run before it exists and silently find nothing (this is the
        // convention already used by HudController/MainMenuController for the same reason).
        private void OnEnable()
        {
            // A hero was already chosen this session (e.g. Main.unity re-entered from the world map;
            // GameManager and its CharacterSelectionState persist across scenes): re-apply it to
            // this scene's fresh player instead of asking again. Fresh player, so grant the skills.
            ServiceLocator.TryGet(out CharacterSelectionState state);
            SceneTransitionService transition = ServiceLocator.TryGet(out SceneTransitionService service) ? service : null;
            bool encounterPending = transition != null && transition.HasPendingEncounter;

            switch (GameFlowMachine.DecideCombatSceneSelect(state != null && state.HasSelection, encounterPending))
            {
                case CharacterSelectDecision.ReapplyAndSkip:
                    ApplyCharacter(state.Selected.Value, grantSkills: true);
                    HideAndDeactivateNextFrame();
                    return;

                case CharacterSelectDecision.AbortEncounter:
                    // Unreachable through the normal flow (the title screen forces a hero before the
                    // map). Never show the picker over a live arena: back out to the map, whose
                    // front-end routes a hero-less session to the picker there.
                    GameLogger.Error("CharacterSelect", "Encounter started without a chosen hero; returning to the world map to pick one.");
                    HideAndDeactivateNextFrame();
                    transition.CompleteEncounter(EncounterOutcome.Fled);
                    return;
            }

            SetRootVisible(true);
            GameplayPause.Acquire(this);
            BindElementsIfNeeded();
            RefreshContinueOption();
        }

        private void OnDisable()
        {
            GameplayPause.Release(this);
        }

        private void HideAndDeactivateNextFrame()
        {
            // SetActive(false) can't be called while this object is still being activated
            // (Unity rejects it from inside OnEnable) — hide now, deactivate next Update.
            GameplayPause.Release(this);
            SetRootVisible(false);
            _deactivatePending = true;
        }

        private void SetRootVisible(bool visible)
        {
            VisualElement root = _uiDocument != null ? _uiDocument.rootVisualElement : null;

            if (root != null)
            {
                root.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }

        private void Update()
        {
            if (_deactivatePending)
            {
                _deactivatePending = false;
                gameObject.SetActive(false);
            }
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
            _continueButton = _root.Q<Button>("continue-saved-button");

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

            if (_continueButton != null)
            {
                _continueButton.clicked += ContinueFromSave;
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

            if (ServiceLocator.TryGet(out CharacterSelectionState state))
            {
                state.Select(id);
            }

            ApplyCharacter(id, grantSkills: true);
            Close();
        }

        /// <summary>
        /// Applies a hero to the player: passive, signature skill set, placeholder tint and
        /// <see cref="OnCharacterSelected"/> (wired to PlayerVisualSwapper.SwapTo in Main.unity).
        /// With <paramref name="grantSkills"/> false (restoring a save) the skills are only
        /// registered — the save's own SkillManager entry, restored right after, decides what is
        /// learned/equipped; likewise PlayerStats/AttributeSet overwrite the passive's heal/mana.
        /// </summary>
        private void ApplyCharacter(CharacterId id, bool grantSkills)
        {
            PlayableCharacter character = CharacterRegistry.Get(id);

            if (_playerStats != null)
            {
                character.ApplyPassive(_playerStats);

                // Each hero brings their own 5 signature skills (Resources/Skills/Characters/<Id>):
                // the 4 actives are learned + equipped right away, the Ultimate unlocks later.
                if (_playerStats.TryGetComponent(out SkillManager skillManager))
                {
                    if (grantSkills)
                    {
                        CharacterSkillSet.GrantTo(skillManager, id, _playerStats.Level);
                    }
                    else
                    {
                        skillManager.RegisterSkills(CharacterSkillSet.GetSkills(id));
                    }
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
        }

        private void Close()
        {
            _previewController?.Clear();
            GameplayPause.Release(this);
            gameObject.SetActive(false);
        }

        /// <summary>A loaded save brought its hero back: re-apply it, and skip this screen if it is open.</summary>
        private void OnCharacterSelectionRestored(CharacterSelectionRestoredEvent evt)
        {
            ApplyCharacter(evt.CharacterId, grantSkills: false);

            if (gameObject.activeSelf)
            {
                Close();
            }
        }

        /// <summary>Shows "Continuar com &lt;herói&gt;" only when one of <see cref="_continueSlots"/> holds a save with a chosen hero.</summary>
        private void RefreshContinueOption()
        {
            if (_continueButton == null)
            {
                return;
            }

            _continueSlot = NoContinueSlot;
            CharacterId? savedHero = null;
            DateTime newest = DateTime.MinValue;

            if (ServiceLocator.TryGet(out SaveManager saveManager) && _continueSlots != null)
            {
                foreach (int slot in _continueSlots)
                {
                    if (!saveManager.SlotExists(slot)
                        || !TryReadSavedHero(saveManager.PeekSlot(slot), out CharacterId hero, out DateTime savedAt))
                    {
                        continue;
                    }

                    if (savedHero == null || savedAt > newest)
                    {
                        savedHero = hero;
                        newest = savedAt;
                        _continueSlot = slot;
                    }
                }
            }

            _continueButton.style.display = savedHero.HasValue ? DisplayStyle.Flex : DisplayStyle.None;

            if (savedHero.HasValue)
            {
                _continueButton.text = $"Continuar com {CharacterRegistry.Get(savedHero.Value).DisplayName}";
            }
        }

        /// <summary>
        /// Reads the hero stored in a peeked save (see <see cref="SaveManager.PeekSlot"/>). False for
        /// saves without one — older saves (schema &lt; 2) or ones written before a hero was picked.
        /// </summary>
        public static bool TryReadSavedHero(SaveData saveData, out CharacterId hero, out DateTime savedAtUtc)
        {
            hero = default;
            savedAtUtc = DateTime.MinValue;

            if (!SaveManager.TryGetEntryState(saveData, CharacterSelectionState.Key, out CharacterSelectionSaveState state)
                || !state.hasSelection
                || !CharacterSelectionState.TryParse(state.characterId, out hero))
            {
                return false;
            }

            DateTime.TryParse(saveData.lastSavedAtIso8601, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out savedAtUtc);
            return true;
        }

        /// <summary>Loads the save picked by <see cref="RefreshContinueOption"/>; its restored hero closes this screen via <see cref="OnCharacterSelectionRestored"/>.</summary>
        private void ContinueFromSave()
        {
            if (_continueSlot == NoContinueSlot || !ServiceLocator.TryGet(out SaveManager saveManager))
            {
                return;
            }

            saveManager.Load(_continueSlot);
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
