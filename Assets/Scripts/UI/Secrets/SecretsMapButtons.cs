using Rapadura.Core.DI;
using Rapadura.Core.Localization;
using Rapadura.Core.Systems;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Dialogue;
using Rapadura.Gameplay.Secrets;
using Rapadura.UI.WorldMap;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace Rapadura.UI.Secrets
{
    /// <summary>
    /// World map HUD side of "Segredos e áreas ocultas". Self-bootstrapping, no scene edit (same pattern as
    /// <see cref="RegionNpcTalkButton"/>): after every scene load it joins the <see cref="WorldMapHudController"/>'s
    /// GameObject and injects, into the HUD document (so the HUD's pointer blocker and visibility cover them):
    /// <list type="bullet">
    /// <item>"Investigar: {secret}" in <c>wm-action-row</c>, while the idle token stands on a region whose secret is
    /// revealed and not yet investigated (<see cref="WorldMapSecretsDirector.InvestigableHere"/>); "(tem guarda!)"
    /// when a fight guards it.</item>
    /// <item>"Relíquias (n/12)" inside the Mochila summary (<c>wm-backpack-panel</c>), which opens the
    /// <see cref="RelicCollectionView"/> panel — registered as a HUD screen, so Esc / Start closes it and only one
    /// screen is open at a time; it holds GameplayPause while open.</item>
    /// </list>
    /// </summary>
    [DisallowMultipleComponent]
    public class SecretsMapButtons : MonoBehaviour
    {
        public const string InvestigateButtonName = "wm-secret-button";
        public const string RelicsEntryName = "wm-relics-entry";
        public const string ScreenId = "relics";
        public const string ActionRowName = "wm-action-row";
        public const string BackpackPanelName = "wm-backpack-panel";
        public const string BackpackCloseName = "wm-backpack-close-button";
        public const string HudRootName = "wm-hud-root";

        private const float RefreshInterval = 0.2f;

        private WorldMapHudController _hud;
        private UIDocument _hudDocument;
        private Button _investigate;
        private Button _relicsEntry;
        private RelicCollectionView _view;
        private bool _screenRegistered;
        private bool _open;
        private float _nextRefresh;

        public bool IsPanelOpen => _open;

        // ------------------------------------------------------------------
        // Bootstrap
        // ------------------------------------------------------------------

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
            if (hud != null && hud.GetComponent<SecretsMapButtons>() == null)
            {
                hud.gameObject.AddComponent<SecretsMapButtons>();
            }
        }

        private void Awake()
        {
            _hud = GetComponent<WorldMapHudController>();
            _hudDocument = GetComponent<UIDocument>();
        }

        private void OnDisable()
        {
            ClosePanel();
            Remove(ref _investigate, OnInvestigateClicked);
            Remove(ref _relicsEntry, OnRelicsEntryClicked);
            if (_view != null)
            {
                _view.Close.clicked -= ClosePanel;
                _view.Overlay.RemoveFromHierarchy();
                _view = null;
            }
        }

        private static void Remove(ref Button button, System.Action handler)
        {
            if (button != null)
            {
                button.clicked -= handler;
                button.RemoveFromHierarchy();
                button = null;
            }
        }

        private void Update()
        {
            if (Time.unscaledTime < _nextRefresh)
            {
                return;
            }

            _nextRefresh = Time.unscaledTime + RefreshInterval;
            if (!_screenRegistered && _hud != null)
            {
                _hud.RegisterScreen(ScreenId, () => _open, OpenPanel, ClosePanel);
                _screenRegistered = true;
            }

            RefreshInvestigate();
            RefreshRelicsEntry();
        }

        // ------------------------------------------------------------------
        // Pure rules (tested)
        // ------------------------------------------------------------------

        /// <summary>Whether "Investigar" should be on screen.</summary>
        public static bool ShouldShowInvestigate(bool hudVisible, bool hasInvestigable, bool paused, bool dialogueActive)
        {
            return hudVisible && hasInvestigable && !paused && !dialogueActive;
        }

        /// <summary>"Investigar: Moinho abandonado" / "… (tem guarda!)".</summary>
        public static string BuildInvestigateLabel(WorldSecretDefinition secret, LocalizationManager localization)
        {
            string name = SecretTexts.Get(localization, secret.NameKey);
            return SecretTexts.Format(localization, secret.Visit == SecretVisitKind.Encounter ? SecretTexts.InvestigateGuarded : SecretTexts.Investigate, name);
        }

        /// <summary>"Relíquias (3/12)".</summary>
        public static string BuildRelicsEntryLabel(SecretsProgress secrets, LocalizationManager localization)
        {
            return SecretTexts.Get(localization, SecretTexts.RelicsButton) + " (" + (secrets != null ? secrets.OwnedRelicCount : 0) + "/" + RelicCatalog.Count + ")";
        }

        // ------------------------------------------------------------------
        // "Investigar"
        // ------------------------------------------------------------------

        private VisualElement HudRoot => _hudDocument != null ? _hudDocument.rootVisualElement : null;

        private void RefreshInvestigate()
        {
            VisualElement root = HudRoot;
            VisualElement row = root?.Q<VisualElement>(ActionRowName);
            if (row == null)
            {
                return;
            }

            if (_investigate == null || _investigate.parent != row)
            {
                // First time, or the HUD document rebuilt its tree (re-enable): (re)inject.
                Remove(ref _investigate, OnInvestigateClicked);
                SecretsStyles.ApplyTo(root, addThemeClass: false);
                _investigate = new Button { name = InvestigateButtonName, focusable = true };
                _investigate.AddToClassList("wm-explore-button");
                _investigate.AddToClassList(InvestigateButtonName);
                _investigate.style.display = DisplayStyle.None;
                _investigate.clicked += OnInvestigateClicked;
                row.Add(_investigate);
            }

            WorldMapSecretsDirector director = WorldMapSecretsDirector.Current;
            WorldSecretDefinition secret = director != null ? director.InvestigableHere : null;
            ServiceLocator.TryGet(out DialogueManager dialogue);
            bool show = ShouldShowInvestigate(_hud != null && _hud.IsVisible, secret != null, GameplayPause.IsPaused, dialogue != null && dialogue.IsActive);

            _investigate.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
            if (show)
            {
                string label = BuildInvestigateLabel(secret, Localization);
                if (_investigate.text != label)
                {
                    _investigate.text = label;
                }
            }
        }

        private void OnInvestigateClicked()
        {
            WorldMapSecretsDirector director = WorldMapSecretsDirector.Current;
            if (director == null || GameplayPause.IsPaused)
            {
                return;
            }

            if (director.TryInvestigate(out _))
            {
                _investigate.style.display = DisplayStyle.None;
            }
        }

        // ------------------------------------------------------------------
        // "Relíquias" panel
        // ------------------------------------------------------------------

        private void RefreshRelicsEntry()
        {
            VisualElement root = HudRoot;
            VisualElement panel = root?.Q<VisualElement>(BackpackPanelName);
            if (panel == null)
            {
                return;
            }

            if (_relicsEntry == null || _relicsEntry.parent != panel)
            {
                Remove(ref _relicsEntry, OnRelicsEntryClicked);
                SecretsStyles.ApplyTo(root, addThemeClass: false);
                _relicsEntry = new Button { name = RelicsEntryName, focusable = true };
                _relicsEntry.AddToClassList("rap-button");
                _relicsEntry.AddToClassList(RelicsEntryName);
                _relicsEntry.clicked += OnRelicsEntryClicked;
                VisualElement close = panel.Q<Button>(BackpackCloseName);
                int index = close != null ? panel.IndexOf(close) : -1;
                if (index >= 0)
                {
                    panel.Insert(index, _relicsEntry);
                }
                else
                {
                    panel.Add(_relicsEntry);
                }
            }

            string label = BuildRelicsEntryLabel(SecretsProgress.ResolveShared(), Localization);
            if (_relicsEntry.text != label)
            {
                _relicsEntry.text = label;
            }
        }

        private void OnRelicsEntryClicked()
        {
            // From inside the Mochila: swap screens (the Mochila releases its pause first).
            _hud?.CloseBackpack();
            OpenPanel();
        }

        public void OpenPanel()
        {
            if (_open || !EnsureView())
            {
                return;
            }

            LocalizationManager localization = Localization;
            SecretsProgress secrets = SecretsProgress.ResolveShared();
            System.Func<string, string> localize = key => SecretTexts.Get(localization, key);

            _view.Title.text = localize(SecretTexts.PanelTitle);
            _view.Subtitle.text = localize(SecretTexts.PanelSubtitle);
            _view.Count.text = RelicCollectionPresenter.FormatCount(secrets, localize);
            _view.Note.text = secrets.HasAllRelics
                ? localize(SecretTexts.PanelAllFound)
                : secrets.PendingGold > 0 ? SecretTexts.Format(localization, SecretTexts.PanelPendingGold, secrets.PendingGold) : string.Empty;
            _view.Note.style.display = string.IsNullOrEmpty(_view.Note.text) ? DisplayStyle.None : DisplayStyle.Flex;
            _view.Close.text = localize(SecretTexts.PanelClose);
            _view.Populate(RelicCollectionPresenter.Build(secrets, localize, id => CharacterRegistry.Get(id).DisplayName));

            _open = true;
            _view.Overlay.style.display = DisplayStyle.Flex;
            GameplayPause.Acquire(this);
            _view.Close.Focus();
        }

        public void ClosePanel()
        {
            if (!_open)
            {
                return;
            }

            _open = false;
            if (_view != null)
            {
                _view.Overlay.style.display = DisplayStyle.None;
            }

            GameplayPause.Release(this);
        }

        private bool EnsureView()
        {
            VisualElement root = HudRoot;
            if (root == null)
            {
                return false;
            }

            VisualElement host = root.Q<VisualElement>(HudRootName) ?? root;
            if (_view != null && _view.Overlay.parent == host)
            {
                return true;
            }

            if (_view != null)
            {
                _view.Close.clicked -= ClosePanel;
                _view.Overlay.RemoveFromHierarchy();
            }

            _view = new RelicCollectionView();
            _view.Close.clicked += ClosePanel;
            host.Add(_view.Overlay);
            Rapadura.Audio.UiClickSoundBinder.Register(_hudDocument);
            return true;
        }

        private static LocalizationManager Localization => ServiceLocator.TryGet(out LocalizationManager manager) ? manager : null;
    }
}
