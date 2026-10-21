using Rapadura.Core.DI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Rapadura.Gameplay.Environment
{
    /// <summary>
    /// Adds a scene-local "[Environment]" object with <see cref="DayNightLighting"/> and
    /// <see cref="WeatherVisuals"/> to every loaded scene that doesn't already contain them, as long
    /// as GameManager registered a <see cref="TimeOfDayManager"/>. Same self-registering pattern as
    /// InputRebindBootstrap — no scene wiring required. Placing either component in a scene by hand
    /// (to tune gradients/profiles or assign the sun/material) suppresses the auto-created one;
    /// set <see cref="AutoCreate"/> to false to opt out entirely.
    /// </summary>
    public static class EnvironmentPresentationBootstrap
    {
        public static bool AutoCreate = true;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Initialize()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
            EnsurePresentation();
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (mode == LoadSceneMode.Single)
            {
                EnsurePresentation();
            }
        }

        public static void EnsurePresentation()
        {
            if (!AutoCreate || !ServiceLocator.IsRegistered<TimeOfDayManager>())
            {
                return;
            }

            bool hasLighting = Object.FindAnyObjectByType<DayNightLighting>() != null;
            bool hasWeather = Object.FindAnyObjectByType<WeatherVisuals>() != null;
            if (hasLighting && hasWeather)
            {
                return;
            }

            var go = new GameObject("[Environment]");
            if (!hasLighting)
            {
                go.AddComponent<DayNightLighting>();
            }

            if (!hasWeather)
            {
                go.AddComponent<WeatherVisuals>();
            }
        }
    }
}
