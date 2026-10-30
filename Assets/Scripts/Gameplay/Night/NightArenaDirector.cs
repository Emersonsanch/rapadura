using Rapadura.Core.DI;
using Rapadura.Gameplay.Environment;
using Rapadura.Gameplay.World.Biomes;
using Rapadura.Gameplay.World.Map;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Rapadura.Gameplay.Night
{
    /// <summary>
    /// The arena half of "menos visão à noite", self-bootstrapping (no scene edit) like <c>WorldEventArenaHooks</c>:
    /// after every scene load with a pending encounter (Main.unity entered from the map) a "NightArenaDirector" object is
    /// created in that scene and dies with it. While alive it
    /// <list type="bullet">
    /// <item>flags <see cref="NightVisibility.InArena"/> (night fog through WeatherVisuals) and
    /// <see cref="NightVisibility.DarkPlace"/> for the Cavernas (their own darkness is not doubled);</item>
    /// <item>shortens the main camera's far plane at night (<see cref="NightVisibilityRules.FarClip"/>), restoring it on the way out;</item>
    /// <item>lights the hero's lamparina: a warm, shadowless point light whose radius is
    /// <see cref="NightVisibilityRules.LightRadius"/>(<see cref="NightVisibility.LightRadiusBonus"/>).</item>
    /// </list>
    /// Refreshes a few times per second in real time (lighting presenters do the same).
    /// </summary>
    public class NightArenaDirector : MonoBehaviour
    {
        public const string HostName = "NightArenaDirector";
        public const string LampName = "Lamparina";
        private const string PlayerTag = "Player";
        private const float RefreshInterval = 0.2f;

        private TimeOfDayManager _time;
        private Camera _camera;
        private float _sceneFarClip = -1f;
        private Light _lamp;
        private float _timer = RefreshInterval;

        public Light Lamp => _lamp;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (!ServiceLocator.TryGet(out SceneTransitionService transition) || !transition.HasPendingEncounter)
            {
                return;
            }

            if (FindFirstObjectByType<NightArenaDirector>() != null)
            {
                return;
            }

            var host = new GameObject(HostName);
            SceneManager.MoveGameObjectToScene(host, scene);
            host.AddComponent<NightArenaDirector>().Begin(transition.PendingEncounter != null ? transition.PendingEncounter.biome : BiomeType.Forest);
        }

        public void Begin(BiomeType region)
        {
            NightVisibility.InArena = true;
            NightVisibility.DarkPlace = region == BiomeType.Caves;
        }

        private void OnDestroy()
        {
            NightVisibility.InArena = false;
            NightVisibility.DarkPlace = false;
            if (_camera != null && _sceneFarClip > 0f)
            {
                _camera.farClipPlane = _sceneFarClip;
            }
        }

        private void Update()
        {
            _timer += Time.unscaledDeltaTime;
            if (_timer < RefreshInterval)
            {
                return;
            }

            _timer = 0f;
            if (_time == null && !ServiceLocator.TryGet(out _time))
            {
                return;
            }

            float daylight = _time.DaylightFactor;
            bool clearer = NightVisibility.ClearerNight;
            Transform hero = FindHero();
            UpdateCamera(daylight, clearer, hero);
            UpdateLamp(daylight, hero);
        }

        private Transform FindHero()
        {
            if (_lamp != null && _lamp.transform.parent != null)
            {
                return _lamp.transform.parent;
            }

            GameObject player = GameObject.FindGameObjectWithTag(PlayerTag);
            return player != null ? player.transform : null;
        }

        private void UpdateCamera(float daylight, bool clearer, Transform hero)
        {
            Camera main = Camera.main;
            if (main == null)
            {
                return;
            }

            if (main != _camera)
            {
                if (_camera != null && _sceneFarClip > 0f)
                {
                    _camera.farClipPlane = _sceneFarClip;
                }

                _camera = main;
                _sceneFarClip = main.farClipPlane;
            }

            float distance = hero != null ? Vector3.Distance(main.transform.position, hero.position) : 0f;
            main.farClipPlane = NightVisibilityRules.FarClip(_sceneFarClip, daylight, clearer, distance);
        }

        private void UpdateLamp(float daylight, Transform hero)
        {
            float intensity = NightVisibilityRules.LightIntensityFor(daylight, NightVisibility.DarkPlace);
            if (_lamp == null)
            {
                if (hero == null || intensity <= 0f)
                {
                    return;
                }

                _lamp = CreateLamp(hero);
            }

            _lamp.intensity = intensity;
            _lamp.range = NightVisibilityRules.LightRadius(NightVisibility.LightRadiusBonus);
            _lamp.enabled = intensity > 0.01f;
        }

        /// <summary>The lamparina: a warm point light 1.6 m above the hero's feet, no shadows (cheap on phones).</summary>
        public static Light CreateLamp(Transform hero)
        {
            Transform existing = hero.Find(LampName);
            if (existing != null && existing.TryGetComponent(out Light found))
            {
                return found;
            }

            var lampObject = new GameObject(LampName);
            lampObject.transform.SetParent(hero, false);
            lampObject.transform.localPosition = new Vector3(0f, 1.6f, 0.3f);
            Light lamp = lampObject.AddComponent<Light>();
            lamp.type = LightType.Point;
            lamp.color = NightVisibilityRules.LightColor;
            lamp.shadows = LightShadows.None;
            lamp.range = NightVisibilityRules.LightRadius(NightVisibility.LightRadiusBonus);
            lamp.intensity = 0f;
            lamp.renderMode = LightRenderMode.ForcePixel;
            return lamp;
        }
    }
}
