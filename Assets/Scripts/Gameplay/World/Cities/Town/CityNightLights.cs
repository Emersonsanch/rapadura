using System.Collections.Generic;
using Rapadura.Core.DI;
using Rapadura.Gameplay.Environment;
using UnityEngine;

namespace Rapadura.Gameplay.World.Cities.Town
{
    /// <summary>
    /// Day/night of a walkable town: fades the palette's emission (windows, lanterns, embers, the dock's groove) in as the
    /// daylight of <see cref="TimeOfDayManager"/> goes, and switches a handful of real point lights (the plaza lanterns,
    /// mobile budget: no shadows, small range) on at dusk. The sun, ambient and fog stay with <c>DayNightLighting</c> /
    /// <c>WeatherVisuals</c>, which run in every scene. Checked twice a second, not every frame.
    /// </summary>
    public sealed class CityNightLights : MonoBehaviour
    {
        private const float Interval = 0.5f;
        private const float LightsOnBelowDaylight = 0.55f;

        private readonly List<Light> _lights = new List<Light>();
        private float _next;

        public IReadOnlyList<Light> Lights => _lights;

        /// <summary>0 = full day, 1 = full night (forced by tests / the editor preview when set).</summary>
        public float? NightOverride { get; set; }

        public float Night { get; private set; }

        public void AddLight(Vector3 position, float range = 7f)
        {
            var go = new GameObject("TownLight");
            go.transform.SetParent(transform, false);
            go.transform.position = position;
            var light = go.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(1f, 0.72f, 0.4f);
            light.range = range;
            light.intensity = 0f;
            light.shadows = LightShadows.None;
            light.renderMode = LightRenderMode.Auto;
            light.enabled = false;
            _lights.Add(light);
        }

        private void OnEnable()
        {
            _next = 0f;
            Refresh();
        }

        private void OnDisable()
        {
            TownPalette.SetNightGlow(0f);
        }

        private void Update()
        {
            if (Time.unscaledTime < _next)
            {
                return;
            }

            _next = Time.unscaledTime + Interval;
            Refresh();
        }

        public void Refresh()
        {
            float daylight = 1f;
            if (NightOverride.HasValue)
            {
                daylight = 1f - NightOverride.Value;
            }
            else if (ServiceLocator.TryGet(out TimeOfDayManager time) && time != null)
            {
                daylight = time.DaylightFactor;
            }

            Night = Mathf.Clamp01(1f - daylight);
            TownPalette.SetNightGlow(Night);
            bool on = daylight < LightsOnBelowDaylight;
            float intensity = Mathf.Lerp(0.6f, 1.8f, Mathf.InverseLerp(LightsOnBelowDaylight, 0f, daylight));
            foreach (Light light in _lights)
            {
                if (light == null)
                {
                    continue;
                }

                light.enabled = on;
                light.intensity = on ? intensity : 0f;
            }
        }
    }
}
