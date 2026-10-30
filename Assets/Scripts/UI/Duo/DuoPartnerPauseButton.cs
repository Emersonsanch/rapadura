using Rapadura.Core.DI;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Duo;
using Rapadura.UI.Menus;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace Rapadura.UI.Duo
{
    /// <summary>
    /// "Parceiro da dupla: Ícaro" in the pause menu (arena and world map, so the partner can be picked before leaving
    /// the village too). Self-bootstrapping, no scene/UXML edit: joins every <see cref="PauseMenuController"/> after a
    /// scene load and injects one focusable button into <c>pause-menu-button-list</c>, right before "Menu Principal".
    /// Each press moves to the next unlocked cousin in birth order (<see cref="DuoPartnerState.CyclePartner"/>; the
    /// hero themselves is skipped); the choice is saved with the slot. Hidden until a hero has been chosen.
    /// </summary>
    [DisallowMultipleComponent]
    public class DuoPartnerPauseButton : MonoBehaviour
    {
        public const string ButtonName = "duo-partner-button";
        private const string ListName = "pause-menu-button-list";
        private const string BeforeButtonName = "main-menu-button";
        private const float RefreshInterval = 0.25f;

        private UIDocument _document;
        private PauseMenuController _pause;
        private Button _button;
        private float _nextRefresh;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
            Attach();
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode) => Attach();

        private static void Attach()
        {
            foreach (PauseMenuController pause in FindObjectsByType<PauseMenuController>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (pause.GetComponent<DuoPartnerPauseButton>() == null)
                {
                    pause.gameObject.AddComponent<DuoPartnerPauseButton>();
                }
            }
        }

        private void Awake()
        {
            _document = GetComponent<UIDocument>();
            _pause = GetComponent<PauseMenuController>();
        }

        private void OnDisable()
        {
            if (_button != null)
            {
                _button.clicked -= OnClicked;
                _button.RemoveFromHierarchy();
                _button = null;
            }
        }

        /// <summary>Caption for the hero's current partner.</summary>
        public static string Label(LocalizationManager localization, CharacterId partner)
        {
            return DuoTexts.Format(localization, DuoTexts.PausePartner, DuoTexts.CousinName(partner));
        }

        private void Update()
        {
            // Unscaled: the pause menu freezes time.
            if (Time.unscaledTime < _nextRefresh || _pause == null || !_pause.IsOpen)
            {
                return;
            }

            _nextRefresh = Time.unscaledTime + RefreshInterval;
            if (!EnsureButton())
            {
                return;
            }

            Refresh();
        }

        private bool EnsureButton()
        {
            VisualElement list = _document != null && _document.rootVisualElement != null ? _document.rootVisualElement.Q<VisualElement>(ListName) : null;
            if (list == null)
            {
                return false;
            }

            if (_button != null && _button.parent == list)
            {
                return true;
            }

            _button = new Button { name = ButtonName, focusable = true };
            _button.AddToClassList("pause-menu-button");
            _button.AddToClassList("rap-button");
            _button.clicked += OnClicked;

            Button before = list.Q<Button>(BeforeButtonName);
            int index = before != null ? list.IndexOf(before) : -1;
            if (index >= 0)
            {
                list.Insert(index, _button);
            }
            else
            {
                list.Add(_button);
            }

            Rapadura.UI.Common.UiAccessibilityApplier.RequestRefresh();
            return true;
        }

        private void Refresh()
        {
            CharacterId? hero = DuoStrikeController.CurrentHero;
            CharacterId? partner = hero.HasValue ? DuoPartnerState.ResolveShared().GetPartnerFor(hero.Value) : null;
            _button.style.display = partner.HasValue ? DisplayStyle.Flex : DisplayStyle.None;
            if (partner.HasValue)
            {
                ServiceLocator.TryGet(out LocalizationManager localization);
                _button.text = Label(localization, partner.Value);
            }
        }

        private void OnClicked()
        {
            CharacterId? hero = DuoStrikeController.CurrentHero;
            if (!hero.HasValue)
            {
                return;
            }

            DuoPartnerState.ResolveShared().CyclePartner(hero.Value);
            Refresh();
        }
    }
}
