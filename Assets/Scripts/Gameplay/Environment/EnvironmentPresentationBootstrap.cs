using Rapadura.Core.DI;
using Rapadura.Gameplay.Environment.Impact;
using Rapadura.Gameplay.Environment.Wind;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Rapadura.Gameplay.Environment
{
    /// <summary>
    /// Adds a scene-local "[Environment]" object with <see cref="DayNightLighting"/>,
    /// <see cref="WeatherVisuals"/>, <see cref="AmbientVfx"/>, <see cref="WindSystem"/> and
    /// <see cref="GroundImpactSystem"/> to every loaded scene that doesn't already contain them, and
    /// attaches wind/impact behaviour to the scene's props (<see cref="EnvironmentPropAttacher"/>), as long
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

            // Props are scanned on every load (prefab trees, scattered tufts/rocks); the scan is a
            // no-op for objects that already carry their component.
            EnvironmentPropAttacher.AttachAll();

            bool hasLighting = Object.FindAnyObjectByType<DayNightLighting>() != null;
            bool hasWeather = Object.FindAnyObjectByType<WeatherVisuals>() != null;
            bool hasAmbient = Object.FindAnyObjectByType<AmbientVfx>() != null;
            bool hasWind = Object.FindAnyObjectByType<WindSystem>() != null;
            bool hasImpact = Object.FindAnyObjectByType<GroundImpactSystem>() != null;
            if (hasLighting && hasWeather && hasAmbient && hasWind && hasImpact)
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

            if (!hasAmbient)
            {
                go.AddComponent<AmbientVfx>();
            }

            if (!hasWind)
            {
                go.AddComponent<WindSystem>();
            }

            if (!hasImpact)
            {
                go.AddComponent<GroundImpactSystem>();
            }
        }
    }
}
