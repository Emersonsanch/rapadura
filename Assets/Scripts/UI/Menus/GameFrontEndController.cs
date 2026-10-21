using Rapadura.Core.DI;
using Rapadura.Core.Events;
using Rapadura.Core.Logging;
using Rapadura.Core.Systems;
using Rapadura.Gameplay.Flow;
using Rapadura.Gameplay.World.Map;
using Rapadura.UI.CharacterSelect;
using Rapadura.UI.Quests;
using UnityEngine;

namespace Rapadura.UI.Menus
{
    /// <summary>
    /// Scene side of <see cref="GameFlowService"/> in WorldMap.unity (the first scene in the build):
    /// shows the title screen (<see cref="MainMenuController"/>) on boot, the hero picker
    /// (<see cref="CharacterSelectController"/>) for "Novo jogo" (or a "Continuar" whose save has no
    /// hero), and hands control to the world map (<see cref="WorldMapController"/>) once a hero is
    /// set. Coming back from an encounter the flow is already in <see cref="GameFlowState.World"/>,
    /// so neither screen appears.
    ///
    /// While a front-end screen is up the map controller is disabled (no token movement / random
    /// encounters behind the menu) and gameplay is paused through <see cref="GameplayPause"/>.
    /// Every reference is optional and resolved from the scene (inactive objects included) when left
    /// empty, so the only wiring needed is adding this component plus the two screens to the scene.
    /// Keep the MainMenu and CharacterSelect GameObjects <b>inactive</b> in the scene: this
    /// component activates them for the current state (an active-by-default MainMenu would flash for
    /// a frame on return from an encounter, and an active CharacterSelect would ask for a hero
    /// before the title screen).
    /// </summary>
    public class GameFrontEndController : MonoBehaviour
    {
        private const string LogCategory = "GameFrontEnd";

        [Header("Screens (auto-resolved from the scene when empty)")]
        [SerializeField] private MainMenuController _mainMenu;
        [SerializeField] private CharacterSelectController _characterSelect;
        [SerializeField] private WorldMapController _worldMap;

        [Header("Optional")]
        [Tooltip("Objects hidden while the title/hero screens are up (e.g. world-map HUD, quest tracker).")]
        [SerializeField] private GameObject[] _hideWhileInFrontEnd = new GameObject[0];

        private GameFlowService _flow;

        private void Awake()
        {
            if (_mainMenu == null) _mainMenu = FindFirstObjectByType<MainMenuController>(FindObjectsInactive.Include);
            if (_characterSelect == null) _characterSelect = FindFirstObjectByType<CharacterSelectController>(FindObjectsInactive.Include);
            if (_worldMap == null) _worldMap = FindFirstObjectByType<WorldMapController>(FindObjectsInactive.Include);

            if (_mainMenu != null)
            {
                _mainMenu.OnNewGameRequested.AddListener(OnNewGameRequested);
                _mainMenu.OnContinueRequested.AddListener(OnContinueRequested);
            }

            // Every Awake runs before any Start: keep the map from starting (its Start reads
            // WorldMapProgress, which "Continuar" is about to restore) until a hero is in play.
            // On the very first scene GameManager may not have registered the flow yet — that is
            // the boot case, which always opens the title screen.
            bool inWorld = ServiceLocator.TryGet(out GameFlowService flow) && GameFlowMachine.IsWorldInteractive(flow.State) && flow.HasHero;
            if (!inWorld && _worldMap != null)
            {
                _worldMap.enabled = false;
            }
        }

        private void OnEnable()
        {
            EventBus.Subscribe<GameFlowStateChangedEvent>(OnFlowStateChanged);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<GameFlowStateChangedEvent>(OnFlowStateChanged);
            GameplayPause.Release(this);
        }

        private void OnDestroy()
        {
            if (_mainMenu != null)
            {
                _mainMenu.OnNewGameRequested.RemoveListener(OnNewGameRequested);
                _mainMenu.OnContinueRequested.RemoveListener(OnContinueRequested);
            }
        }

        private void Start()
        {
            if (!ServiceLocator.TryGet(out _flow))
            {
                // WorldMap played without a GameManager: no flow, no persistence — just the map.
                GameLogger.Warning(LogCategory, "No GameFlowService registered (missing GameManager?); skipping the title screen.");
                ApplyState(GameFlowState.World);
                return;
            }

            ApplyState(_flow.EnterFrontEnd());
        }

        private void OnNewGameRequested()
        {
            if (_flow == null)
            {
                return;
            }

            QuestTracking.Reset();
            _flow.StartNewGame();
        }

        private void OnContinueRequested()
        {
            if (_flow != null && !_flow.Continue())
            {
                GameLogger.Warning(LogCategory, "Continuar failed; staying on the title screen.");
            }
        }

        private void OnFlowStateChanged(GameFlowStateChangedEvent evt)
        {
            ApplyState(evt.Current);
        }

        private void ApplyState(GameFlowState state)
        {
            bool showMenu = state == GameFlowState.MainMenu;
            bool showSelect = state == GameFlowState.CharacterSelect;
            bool frontEnd = showMenu || showSelect;

            if (_mainMenu != null && _mainMenu.gameObject.activeSelf != showMenu)
            {
                _mainMenu.gameObject.SetActive(showMenu);
            }

            if (_characterSelect != null)
            {
                if (showSelect && !_characterSelect.gameObject.activeSelf)
                {
                    _characterSelect.gameObject.SetActive(true);
                }
                else if (showMenu && _characterSelect.gameObject.activeSelf)
                {
                    // Leaving the picker for World is the picker's own job: it closes itself on any
                    // CharacterSelectedEvent/restore while open — and GameFlowService hears that
                    // event first, so deactivating it here would stop it applying the hero.
                    _characterSelect.gameObject.SetActive(false);
                }
            }

            foreach (GameObject target in _hideWhileInFrontEnd)
            {
                if (target != null && target.activeSelf == frontEnd)
                {
                    target.SetActive(!frontEnd);
                }
            }

            if (_worldMap != null)
            {
                _worldMap.enabled = GameFlowMachine.IsWorldInteractive(state);
            }

            if (frontEnd)
            {
                GameplayPause.Acquire(this);
            }
            else
            {
                GameplayPause.Release(this);
            }
        }
    }
}
