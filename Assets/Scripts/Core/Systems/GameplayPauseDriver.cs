using Rapadura.Core.Events;
using Rapadura.UI.Menus;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Rapadura.Core.Systems
{
    /// <summary>
    /// Per-frame half of <see cref="GameplayPause"/>. Added automatically to the persistent
    /// GameManager object (no scene wiring). Runs early (<see cref="DefaultExecutionOrderAttribute"/>)
    /// and again in LateUpdate so a timeScale clobbered by someone else (hit-stop restore, the pause
    /// menu's OnEnable) is put back to 0 before enemies' Update and before the next FixedUpdate.
    /// Also forwards the pause menu's <see cref="GamePausedEvent"/>/<see cref="GameResumedEvent"/>.
    /// </summary>
    [DefaultExecutionOrder(-1000)]
    public class GameplayPauseDriver : MonoBehaviour
    {
        private void OnEnable()
        {
            EventBus.Subscribe<GamePausedEvent>(OnGamePaused);
            EventBus.Subscribe<GameResumedEvent>(OnGameResumed);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<GamePausedEvent>(OnGamePaused);
            EventBus.Unsubscribe<GameResumedEvent>(OnGameResumed);
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void Update()
        {
            GameplayPause.Enforce();
        }

        private void LateUpdate()
        {
            GameplayPause.Enforce();
        }

        private void OnGamePaused(GamePausedEvent _) => GameplayPause.HandleExternalPaused();

        private void OnGameResumed(GameResumedEvent _) => GameplayPause.HandleExternalResumed();

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode) => GameplayPause.HandleSceneLoaded();
    }
}
