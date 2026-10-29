using System;
using System.Collections.Generic;
using System.Globalization;
using Rapadura.Core.DI;
using Rapadura.Core.Events;
using Rapadura.Core.Localization;
using Rapadura.Core.Logging;
using Rapadura.Core.Systems;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Content;
using Rapadura.Gameplay.Flow;
using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.Skills;
using Rapadura.Gameplay.World.Map;
using Rapadura.Save;
using Rapadura.UI.Common;
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
    /// <see cref="GameFlowService.DefaultContinueSlots"/> (auto-save + every manual slot; code-defined,
    /// so a stale <c>_continueSlots</c> value still serialized in a scene is ignored).
    ///
    /// Game flow: the real place to pick a hero is WorldMap.unity, opened by "Novo jogo" on the title
    /// screen (<c>GameFrontEndController</c> / <see cref="GameFlowService"/>); without a player there
    /// it only records the choice. The copy in Main.unity is a dev convenience for opening that scene
    /// directly in the Editor — <see cref="GameFlowMachine.DecideCombatSceneSelect"/> guarantees it
    /// never shows during an encounter. Pausing goes through <see cref="GameplayPause"/> (the old
    /// direct Time.timeScale = 0 was undone by PauseMenuController.OnEnable, letting enemies attack
    /// behind this screen).
    ///
    /// Screen text (title, "Confirmar", prompt, "Continuar com &lt;herói&gt;") goes through
    /// <see cref="UiTexts"/> (<c>ui.character_select.*</c>, static UXML via <see cref="UiDocumentLocalizer"/>);
    /// hero names, classes, roles and lore come from <see cref="ContentText"/> (<c>character.*</c>).
    /// A language switch while the screen is open re-labels the roster buttons and the highlighted
    /// hero's name/class — role/lore in place (no rebuild, so selection, focus and the pause are
    /// untouched); the pure text rules are the public static <c>Build*Text</c> helpers.
    ///
    /// Look (Common/RapaduraTheme.uss): each roster button carries the hero's accent stripe + medal and
    /// role; the detail panel shows the 3D turntable framed in the accent colour, age and family line
    /// (<see cref="HeroFamily"/>), lore and the 5 skills. The screen never opens empty: a hero is
    /// highlighted on open (<see cref="ChooseDefaultHighlight"/>) and moving focus through the roster
    /// previews each one.
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
        [Tooltip("Clear colour given to the preview camera (the one rendering into the RenderTexture) so the turntable sits on the screen's warm brown instead of the camera's own navy.")]
        [SerializeField] private Color _previewBackgroundColor = new Color(0.17f, 0.11f, 0.07f, 1f);

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
        private Label _ageLabel;
        private Label _familyLabel;
        private VisualElement _skillsContainer;
        private VisualElement _previewFrame;
        private VisualElement _accentBar;
        private Button _confirmButton;
        private Button _continueButton;
        private Texture2D _backdropTexture;

        private readonly Dictionary<CharacterId, Button> _rosterButtons = new Dictionary<CharacterId, Button>();
        private readonly Dictionary<CharacterId, Label> _rosterNameLabels = new Dictionary<CharacterId, Label>();
        private readonly Dictionary<CharacterId, Label> _rosterRoleLabels = new Dictionary<CharacterId, Label>();
        private readonly Dictionary<CharacterId, Label> _rosterMedals = new Dictionary<CharacterId, Label>();

        /// <summary>USS class on the roster button of the hero currently shown in the detail panel.</summary>
        public const string RosterSelectedClass = "character-select__roster-button--selected";
        public const string SkillClass = "character-select__skill";
        public const string UltimateSkillClass = "character-select__skill--ultimate";

        /// <summary>Hero select backdrop, top to bottom: night brown into warm caramel at the floor.</summary>
        private static readonly Color[] BackdropStops =
        {
            new Color32(34, 20, 14, 255),
            new Color32(72, 40, 22, 255),
            new Color32(128, 70, 30, 255),
        };
        private CharacterId? _hovered;
        private CharacterId? _continueHero;
        private int _continueSlot = NoContinueSlot;
        private readonly UiDocumentLocalizer _localizer = new UiDocumentLocalizer();
        private bool _isBound;
        private bool _deactivatePending;

        /// <summary>The picker is on screen waiting for a choice (and holds a <see cref="GameplayPause"/>).</summary>
        private bool _isShowing;

        /// <summary>Whether the picker is currently on screen waiting for a hero.</summary>
        public bool IsShowing => _isShowing;

        private void Awake()
        {
            _uiDocument = GetComponent<UIDocument>();

            // Subscribed for the whole lifetime (not OnEnable/OnDisable): a restore usually arrives
            // while this screen is already hidden (loading from the pause menu), and the player
            // still needs its hero's visual/passive/skills re-applied then.
            EventBus.Subscribe<CharacterSelectionRestoredEvent>(OnCharacterSelectionRestored);
            EventBus.Subscribe<CharacterSelectedEvent>(OnCharacterSelectedElsewhere);
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<CharacterSelectionRestoredEvent>(OnCharacterSelectionRestored);
            EventBus.Unsubscribe<CharacterSelectedEvent>(OnCharacterSelectedElsewhere);
            UiGradientTexture.Release(_backdropTexture);
            _backdropTexture = null;
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
            _isShowing = true;
            GameplayPause.Acquire(this);
            BindElementsIfNeeded();
            _localizer.Attach(_root, OnLanguageChanged);
            // Not just the prompt: the roster is built once, so a language switched while this
            // screen was hidden (localizer detached) would leave stale hero labels.
            OnLanguageChanged();
            RefreshContinueOption();
            UiMotionPreferences.ApplyTo(_root?.Q<VisualElement>("character-select-root"), UiMotionPreferences.IsReducedMotion());
            HighlightDefault();
        }

        /// <summary>
        /// Never open on an empty detail panel (the old screen showed a blank preview square until a
        /// click): highlight the hero picked last time this session, else the saved hero "Continuar"
        /// offers, else the first in the roster — and give its button focus so the D-pad starts there.
        /// </summary>
        private void HighlightDefault()
        {
            CharacterId? id = ChooseDefaultHighlight(_rosterButtons.Keys, _hovered, _continueHero);
            if (id is not CharacterId target)
            {
                return;
            }

            ShowDetail(CharacterRegistry.Get(target));
            if (_rosterButtons.TryGetValue(target, out Button button))
            {
                button.Focus();
            }
        }

        /// <summary>
        /// Hero highlighted when the screen opens: <paramref name="current"/> (still in the roster), else
        /// <paramref name="savedHero"/>, else the first roster entry; null only for an empty roster.
        /// </summary>
        public static CharacterId? ChooseDefaultHighlight(IEnumerable<CharacterId> roster, CharacterId? current, CharacterId? savedHero)
        {
            if (roster == null)
            {
                return null;
            }

            CharacterId? first = null;
            bool hasSaved = false;
            foreach (CharacterId id in roster)
            {
                first ??= id;
                if (current.HasValue && id == current.Value)
                {
                    return id;
                }

                hasSaved |= savedHero.HasValue && id == savedHero.Value;
            }

            return hasSaved ? savedHero : first;
        }

        private void OnDisable()
        {
            _localizer.Detach();
            _isShowing = false;
            GameplayPause.Release(this);
        }

        private void HideAndDeactivateNextFrame()
        {
            // SetActive(false) can't be called while this object is still being activated
            // (Unity rejects it from inside OnEnable) — hide now, deactivate next Update.
            _isShowing = false;
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
            _ageLabel = _root.Q<Label>("detail-age");
            _familyLabel = _root.Q<Label>("detail-family");
            _skillsContainer = _root.Q<VisualElement>("detail-skills");
            _previewFrame = _root.Q<VisualElement>("detail-preview-frame");
            _accentBar = _root.Q<VisualElement>("detail-accent-bar");

            if (_backdropTexture == null)
            {
                _backdropTexture = UiGradientTexture.ApplyVertical(_root.Q<VisualElement>("character-select-backdrop"), BackdropStops);
            }

            TintPreviewCamera();

            if (_previewImage != null && _previewRenderTexture != null)
            {
                _previewImage.image = _previewRenderTexture;

                // Belt-and-suspenders against flexbox collapsing this element to near-zero height
                // (observed with just the USS class alone) — force the fixed square size inline.
                // Keep in sync with .character-select__detail-preview (260px, left column of the detail panel).
                _previewImage.style.width = 260;
                _previewImage.style.height = 260;
                _previewImage.style.minHeight = 260;
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
            _rosterButtons.Clear();
            _rosterNameLabels.Clear();
            _rosterRoleLabels.Clear();
            _rosterMedals.Clear();

            foreach (PlayableCharacter character in CharacterRegistry.GetAll())
            {
                // Medal (accent colour + initial) | name over role. The button's own text stays empty:
                // the labels carry it so the role can sit under the name.
                var button = new Button { text = string.Empty, name = $"character-{character.Id}" };
                button.AddToClassList("character-select__roster-button");
                button.style.borderLeftColor = character.AccentColor;

                var medal = new Label { pickingMode = PickingMode.Ignore };
                medal.AddToClassList("character-select__roster-medal");
                medal.style.backgroundColor = character.AccentColor;
                medal.style.color = MedalTextColor(character.AccentColor);

                var text = new VisualElement { pickingMode = PickingMode.Ignore };
                text.AddToClassList("character-select__roster-text");
                var nameLabel = new Label { pickingMode = PickingMode.Ignore };
                nameLabel.AddToClassList("character-select__roster-name");
                var roleLabel = new Label { pickingMode = PickingMode.Ignore };
                roleLabel.AddToClassList("character-select__roster-role");
                text.Add(nameLabel);
                text.Add(roleLabel);

                button.Add(medal);
                button.Add(text);
                button.clicked += () => ShowDetail(character);
                // Gamepad/keyboard: moving through the roster previews each hero (Confirmar picks the one shown).
                button.RegisterCallback<FocusInEvent>(_ => ShowDetail(character));
                _rosterContainer.Add(button);
                _rosterButtons[character.Id] = button;
                _rosterNameLabels[character.Id] = nameLabel;
                _rosterRoleLabels[character.Id] = roleLabel;
                _rosterMedals[character.Id] = medal;
            }

            RefreshRosterTexts();
            ApplyRosterSelection(_rosterButtons, _hovered);
            UiAccessibilityApplier.RequestRefresh();
        }

        /// <summary>Readable initial on the accent medal: dark brown on light accents (Maria's gold), cream on dark ones.</summary>
        public static Color MedalTextColor(Color accent)
        {
            float luminance = 0.2126f * accent.r + 0.7152f * accent.g + 0.0722f * accent.b;
            return luminance > 0.5f ? new Color32(46, 27, 16, 255) : new Color32(255, 246, 228, 255);
        }

        /// <summary>Initial shown on the roster medal ("M", "Í"…), from the localized name.</summary>
        public static string BuildMedalText(PlayableCharacter character, LocalizationManager manager)
        {
            string name = BuildRosterButtonText(character, manager);
            return string.IsNullOrEmpty(name) ? string.Empty : name.Substring(0, 1).ToUpperInvariant();
        }

        /// <summary>
        /// The 5 skill lines of the detail panel, in <see cref="PlayableCharacter.AbilityNames"/> order:
        /// each one's localized name when its <see cref="SkillDefinition"/> is among <paramref name="skills"/>
        /// (matched by display name), else the authored name; the last (the Ultimate) is tagged via
        /// <see cref="UiTexts.CharacterSelectUltimate"/>.
        /// </summary>
        public static List<string> BuildSkillLines(PlayableCharacter character, IReadOnlyList<SkillDefinition> skills, LocalizationManager manager)
        {
            var lines = new List<string>();
            string[] names = character?.AbilityNames;
            if (names == null)
            {
                return lines;
            }

            for (int i = 0; i < names.Length; i++)
            {
                string line = names[i];
                if (skills != null)
                {
                    foreach (SkillDefinition skill in skills)
                    {
                        if (skill != null && skill.DisplayName == names[i])
                        {
                            line = skill.LocalizedName(manager);
                            break;
                        }
                    }
                }

                lines.Add(i == names.Length - 1 ? UiTexts.Format(manager, UiTexts.CharacterSelectUltimate, line) : line);
            }

            return lines;
        }

        /// <summary>"14 anos" / "14 years old".</summary>
        public static string BuildAgeText(PlayableCharacter character, LocalizationManager manager) =>
            character == null ? string.Empty : UiTexts.Format(manager, UiTexts.CharacterSelectAge, character.Age);

        /// <summary>
        /// Marks the roster button of <paramref name="selected"/> with <see cref="RosterSelectedClass"/>
        /// (and clears it from every other one), so the picked hero stays visibly selected — before,
        /// only :hover changed the look and nothing showed which hero "Confirmar" would pick.
        /// </summary>
        public static void ApplyRosterSelection(IReadOnlyDictionary<CharacterId, Button> buttons, CharacterId? selected)
        {
            if (buttons == null)
            {
                return;
            }

            foreach (KeyValuePair<CharacterId, Button> pair in buttons)
            {
                pair.Value?.EnableInClassList(RosterSelectedClass, selected.HasValue && pair.Key == selected.Value);
            }
        }

        /// <summary>The localizer's manager (resolved from the ServiceLocator on attach), or the live one; null means pt-BR asset text.</summary>
        private LocalizationManager Manager => _localizer.Manager ?? ContentText.CurrentManager;

        /// <summary>Roster button label: the hero's name in the current language.</summary>
        public static string BuildRosterButtonText(PlayableCharacter character, LocalizationManager manager) =>
            character == null ? string.Empty : character.LocalizedName(manager);

        /// <summary>Detail line under the name: "Caçadora — DPS à distância" / "Huntress — Ranged DPS".</summary>
        public static string BuildClassRoleText(PlayableCharacter character, LocalizationManager manager) =>
            character == null ? string.Empty : $"{character.LocalizedClassName(manager)} — {character.LocalizedRole(manager)}";

        /// <summary>Lore area: the highlighted hero's lore, or the "pick a hero" prompt while none is highlighted.</summary>
        public static string BuildLoreText(PlayableCharacter highlighted, LocalizationManager manager) =>
            highlighted != null ? highlighted.LocalizedLore(manager) : UiTexts.Get(manager, UiTexts.CharacterSelectPrompt);

        private void RefreshRosterTexts()
        {
            foreach (CharacterId id in _rosterButtons.Keys)
            {
                PlayableCharacter character = CharacterRegistry.Get(id);
                if (_rosterNameLabels.TryGetValue(id, out Label nameLabel)) nameLabel.text = BuildRosterButtonText(character, Manager);
                if (_rosterRoleLabels.TryGetValue(id, out Label roleLabel)) roleLabel.text = character.LocalizedRole(Manager);
                if (_rosterMedals.TryGetValue(id, out Label medal)) medal.text = BuildMedalText(character, Manager);
            }
        }

        /// <summary>Name, class — role, age, family line, skills and lore of <paramref name="character"/>; text only (no preview/confirm side effects).</summary>
        private void ApplyDetailTexts(PlayableCharacter character)
        {
            if (_nameLabel != null) _nameLabel.text = BuildRosterButtonText(character, Manager);
            if (_classRoleLabel != null) _classRoleLabel.text = BuildClassRoleText(character, Manager);
            if (_loreLabel != null) _loreLabel.text = BuildLoreText(character, Manager);
            if (_ageLabel != null) _ageLabel.text = BuildAgeText(character, Manager);
            if (_familyLabel != null) _familyLabel.text = character == null ? string.Empty : HeroFamily.BuildFamilyLine(character.Id, Manager);
            ApplySkillList(character);
        }

        private void ApplySkillList(PlayableCharacter character)
        {
            if (_skillsContainer == null)
            {
                return;
            }

            _skillsContainer.Clear();
            if (character == null)
            {
                return;
            }

            List<string> lines = BuildSkillLines(character, CharacterSkillSet.GetSkills(character.Id), Manager);
            for (int i = 0; i < lines.Count; i++)
            {
                var label = new Label(lines[i]);
                label.AddToClassList(SkillClass);
                label.EnableInClassList(UltimateSkillClass, i == lines.Count - 1);
                _skillsContainer.Add(label);
            }

            UiAccessibilityApplier.RequestRefresh();
        }

        /// <summary>The shown hero's accent on the preview frame and the bar under the name.</summary>
        private void ApplyAccent(PlayableCharacter character)
        {
            if (character == null)
            {
                return;
            }

            if (_previewFrame != null)
            {
                _previewFrame.style.borderTopColor = character.AccentColor;
                _previewFrame.style.borderRightColor = character.AccentColor;
                _previewFrame.style.borderBottomColor = character.AccentColor;
                _previewFrame.style.borderLeftColor = character.AccentColor;
            }

            if (_accentBar != null)
            {
                _accentBar.style.backgroundColor = character.AccentColor;
            }
        }

        /// <summary>Gives the camera rendering into <see cref="_previewRenderTexture"/> the screen's warm clear colour (no scene edit needed).</summary>
        private void TintPreviewCamera()
        {
            if (_previewRenderTexture == null)
            {
                return;
            }

            foreach (Camera previewCamera in Camera.allCameras)
            {
                if (previewCamera.targetTexture == _previewRenderTexture && previewCamera.clearFlags == CameraClearFlags.SolidColor)
                {
                    previewCamera.backgroundColor = _previewBackgroundColor;
                }
            }
        }

        /// <summary>The "pick a hero" hint in the lore area, only while no hero is highlighted (lore is content, not a key).</summary>
        private void ApplyPrompt()
        {
            if (_loreLabel != null && _hovered == null)
            {
                _loreLabel.text = BuildLoreText(null, Manager);
            }
        }

        private void ApplyContinueText()
        {
            if (_continueButton != null && _continueHero.HasValue)
            {
                _continueButton.text = UiTexts.Format(_localizer.Manager, UiTexts.CharacterSelectContinueWith,
                    CharacterRegistry.Get(_continueHero.Value).LocalizedName(Manager));
            }
        }

        private void OnLanguageChanged()
        {
            RefreshRosterTexts();

            if (_hovered is CharacterId hovered)
            {
                ApplyDetailTexts(CharacterRegistry.Get(hovered));
            }
            else
            {
                ApplyPrompt();
            }

            ApplyContinueText();
        }

        private void ShowDetail(PlayableCharacter character)
        {
            _hovered = character.Id;

            ApplyRosterSelection(_rosterButtons, _hovered);
            ApplyDetailTexts(character);
            ApplyAccent(character);

            _previewController?.ShowCharacter(character.Id);

            if (_confirmButton != null)
            {
                _confirmButton.SetEnabled(true);
            }
        }

        private void ConfirmSelection()
        {
            if (_hovered is CharacterId id)
            {
                SelectCharacter(id);
            }
        }

        /// <summary>
        /// Picks <paramref name="id"/> exactly as the "Confirmar" button does: records it in
        /// <see cref="CharacterSelectionState"/> (publishing <see cref="CharacterSelectedEvent"/>),
        /// applies it to the player and closes the screen, releasing its pause.
        /// </summary>
        public void SelectCharacter(CharacterId id)
        {
            if (ServiceLocator.TryGet(out CharacterSelectionState state))
            {
                // CharacterSelectedEvent → OnCharacterSelectedElsewhere applies + closes while open.
                state.Select(id);
            }

            if (_isShowing)
            {
                FinishSelection(id);
            }
        }

        /// <summary>
        /// A hero chosen through <see cref="CharacterSelectionState.Select"/> by anyone (this
        /// screen, a script, a test) while the picker is open: apply it and close, so the screen
        /// never stays up holding <see cref="GameplayPause"/> with a hero already chosen.
        /// </summary>
        private void OnCharacterSelectedElsewhere(CharacterSelectedEvent evt)
        {
            if (_isShowing)
            {
                FinishSelection(evt.CharacterId);
            }
        }

        private void FinishSelection(CharacterId id)
        {
            _isShowing = false;
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
                // Hero switch: only the current hero's innate attribute talent may remain.
                if (_playerStats.TryGetComponent(out AttributeSet attributeSet))
                {
                    attributeSet.ClearCharacterPassiveBonuses();
                }

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
            _isShowing = false;
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

        /// <summary>
        /// Shows "Continuar com &lt;herói&gt;" only when one of <see cref="GameFlowService.DefaultContinueSlots"/> holds a save with a
        /// chosen hero and the picker was not opened by "Novo jogo".
        /// </summary>
        private void RefreshContinueOption()
        {
            if (_continueButton == null)
            {
                return;
            }

            _continueSlot = NoContinueSlot;
            CharacterId? savedHero = null;
            DateTime newest = DateTime.MinValue;

            // "Novo jogo" means start over: never offer to load the old save from this screen.
            bool offerContinue = !ServiceLocator.TryGet(out GameFlowService flow) || flow.OffersSavedContinueInSelect;

            if (offerContinue && ServiceLocator.TryGet(out SaveManager saveManager))
            {
                foreach (int slot in GameFlowService.DefaultContinueSlots)
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

            _continueHero = savedHero;
            _continueButton.style.display = savedHero.HasValue ? DisplayStyle.Flex : DisplayStyle.None;
            ApplyContinueText();
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
