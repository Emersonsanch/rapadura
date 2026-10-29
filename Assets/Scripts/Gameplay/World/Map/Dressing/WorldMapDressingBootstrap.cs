using UnityEngine;
using UnityEngine.SceneManagement;

namespace Rapadura.Gameplay.World.Map.Dressing
{
    /// <summary>
    /// Adds a <see cref="WorldMapDressing"/> to every loaded scene that has a
    /// <see cref="WorldMapController"/> (WorldMap.unity) and no dressing yet, so the map scene needs
    /// no wiring. Same self-registering pattern as EnvironmentPresentationBootstrap. Runs on
    /// sceneLoaded, i.e. after the scene's Awake (the controller's travel graph exists) and before
    /// its Start. Placing a WorldMapDressing in the scene by hand (to tune seed/density) suppresses
    /// the auto-created one; set <see cref="AutoCreate"/> to false to opt out entirely.
    /// </summary>
    public static class WorldMapDressingBootstrap
    {
        public static bool AutoCreate = true;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Initialize()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
            EnsureDressing(SceneManager.GetActiveScene());
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode) => EnsureDressing(scene);

        public static void EnsureDressing(Scene scene)
        {
            if (!AutoCreate || !scene.IsValid() || !scene.isLoaded)
            {
                return;
            }

            WorldMapController controller = null;
            foreach (WorldMapController candidate in Object.FindObjectsByType<WorldMapController>(FindObjectsSortMode.None))
            {
                if (candidate.gameObject.scene == scene)
                {
                    controller = candidate;
                    break;
                }
            }

            if (controller != null)
            {
                WorldMapDressing.Ensure(controller);
            }
        }
    }
}
