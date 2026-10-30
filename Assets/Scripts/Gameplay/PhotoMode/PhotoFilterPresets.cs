using System;
using UnityEngine;

namespace Rapadura.Gameplay.PhotoMode
{
    /// <summary>Photo-mode looks. Order = the "Filtro" selector's order.</summary>
    public enum PhotoFilter
    {
        None = 0,
        /// <summary>"Cordel": sepia booklet print, grain and a dark vignette.</summary>
        Cordel = 1,
        /// <summary>"Xilogravura": high-contrast black and white, like the cordel cover woodcuts.</summary>
        Woodcut = 2,
        /// <summary>"Entardecer": warm late-afternoon light.</summary>
        Sunset = 3,
        /// <summary>"Noite de São João": cool night with the bonfire glow (bloom).</summary>
        SaoJoaoNight = 4,
    }

    /// <summary>
    /// Renderer-agnostic values of one look, in URP Volume units (ColorAdjustments / WhiteBalance /
    /// Vignette / FilmGrain / Bloom — all present in URP 17), so this assembly needs no URP reference:
    /// the URP side lives in Assets/Rendering/PhotoMode (<see cref="IPhotoFilterRenderer"/>).
    /// </summary>
    [Serializable]
    public struct PhotoFilterSettings
    {
        /// <summary>False = no grading at all (the camera's own post-processing state is left untouched).</summary>
        public bool Enabled;
        /// <summary>EV.</summary>
        public float PostExposure;
        /// <summary>-100..100.</summary>
        public float Contrast;
        /// <summary>-100..100 (-100 = monochrome).</summary>
        public float Saturation;
        public Color ColorFilter;
        /// <summary>-100..100 (+ = warmer).</summary>
        public float Temperature;
        /// <summary>-100..100 (+ = magenta).</summary>
        public float Tint;
        /// <summary>0..1.</summary>
        public float VignetteIntensity;
        /// <summary>0.01..1.</summary>
        public float VignetteSmoothness;
        public Color VignetteColor;
        /// <summary>0..1. Film grain is re-randomised every frame — it flickers.</summary>
        public float GrainIntensity;
        /// <summary>≥ 0.</summary>
        public float BloomIntensity;
        public float BloomThreshold;
        public Color BloomTint;
    }

    /// <summary>
    /// Renders a <see cref="PhotoFilterSettings"/> on a camera. Registered at startup by the URP assembly
    /// (Rapadura.PhotoMode.Urp); null when that assembly is absent — the photo mode then hides the filter row.
    /// </summary>
    public interface IPhotoFilterRenderer
    {
        /// <summary>The active pipeline can render the look (URP with post-processing data on its renderer).</summary>
        bool IsSupported { get; }

        void Apply(Camera camera, PhotoFilterSettings settings);

        /// <summary>Removes every trace (volume, camera post-processing flag) of <see cref="Apply"/>.</summary>
        void Clear();
    }

    /// <summary>Hook between the photo mode (Rapadura.Runtime) and the URP implementation (separate assembly).</summary>
    public static class PhotoFilterRendering
    {
        public static IPhotoFilterRenderer Renderer { get; set; }

        public static bool IsAvailable => Renderer != null && Renderer.IsSupported;
    }

    /// <summary>The four looks (+ none), their labels, and the "Reduzir Flashes" adjustment. Pure, EditMode-tested.</summary>
    public static class PhotoFilterPresets
    {
        public static readonly PhotoFilter[] All =
        {
            PhotoFilter.None, PhotoFilter.Cordel, PhotoFilter.Woodcut, PhotoFilter.Sunset, PhotoFilter.SaoJoaoNight,
        };

        /// <summary>Bloom ceiling with "Reduzir Flashes" on (glow stays, no blown-out highlights).</summary>
        public const float ReducedFlashMaxBloom = 0.8f;

        /// <summary>Grain multiplier with "Reduzir Flashes" on (grain is re-randomised every frame).</summary>
        public const float ReducedFlashGrainScale = 0.35f;

        public static string LabelKey(PhotoFilter filter)
        {
            switch (filter)
            {
                case PhotoFilter.Cordel: return "ui.photo.filter.cordel";
                case PhotoFilter.Woodcut: return "ui.photo.filter.xilogravura";
                case PhotoFilter.Sunset: return "ui.photo.filter.entardecer";
                case PhotoFilter.SaoJoaoNight: return "ui.photo.filter.sao_joao";
                default: return "ui.photo.filter.none";
            }
        }

        /// <summary>Next/previous filter in <see cref="All"/>, wrapping.</summary>
        public static PhotoFilter Cycle(PhotoFilter current, int step)
        {
            int index = Array.IndexOf(All, current);
            if (index < 0)
            {
                index = 0;
            }

            int count = All.Length;
            return All[((index + step) % count + count) % count];
        }

        public static PhotoFilterSettings Get(PhotoFilter filter)
        {
            PhotoFilterSettings s = Neutral();
            switch (filter)
            {
                case PhotoFilter.Cordel:
                    // Mono first, then a paper-brown colour filter = sepia; soft edges, visible grain.
                    s.Enabled = true;
                    s.Saturation = -100f;
                    s.Contrast = 18f;
                    s.PostExposure = 0.15f;
                    s.ColorFilter = new Color(1f, 0.85f, 0.62f);
                    s.VignetteIntensity = 0.42f;
                    s.VignetteSmoothness = 0.45f;
                    s.VignetteColor = new Color(0.18f, 0.1f, 0.04f);
                    s.GrainIntensity = 0.55f;
                    break;

                case PhotoFilter.Woodcut:
                    // Hard black and white: pushed contrast, slight overexposure so mid-tones split.
                    s.Enabled = true;
                    s.Saturation = -100f;
                    s.Contrast = 85f;
                    s.PostExposure = 0.35f;
                    s.VignetteIntensity = 0.3f;
                    s.VignetteSmoothness = 0.25f;
                    s.VignetteColor = Color.black;
                    s.GrainIntensity = 0.3f;
                    break;

                case PhotoFilter.Sunset:
                    s.Enabled = true;
                    s.Temperature = 45f;
                    s.Tint = 8f;
                    s.Saturation = 15f;
                    s.Contrast = 10f;
                    s.ColorFilter = new Color(1f, 0.93f, 0.82f);
                    s.VignetteIntensity = 0.25f;
                    s.VignetteSmoothness = 0.5f;
                    s.VignetteColor = new Color(0.25f, 0.08f, 0.02f);
                    s.BloomIntensity = 0.6f;
                    s.BloomThreshold = 0.9f;
                    s.BloomTint = new Color(1f, 0.78f, 0.5f);
                    break;

                case PhotoFilter.SaoJoaoNight:
                    // Blue night, darker exposure; low-threshold warm bloom = lanterns / fogueira glow.
                    s.Enabled = true;
                    s.Temperature = -40f;
                    s.PostExposure = -0.45f;
                    s.Saturation = -10f;
                    s.Contrast = 15f;
                    s.ColorFilter = new Color(0.86f, 0.9f, 1f);
                    s.VignetteIntensity = 0.38f;
                    s.VignetteSmoothness = 0.4f;
                    s.VignetteColor = new Color(0.02f, 0.03f, 0.1f);
                    s.BloomIntensity = 1.8f;
                    s.BloomThreshold = 0.7f;
                    s.BloomTint = new Color(1f, 0.7f, 0.35f);
                    break;
            }

            return s;
        }

        /// <summary>"Reduzir Flashes": caps bloom and tones down the per-frame grain flicker. Off = unchanged.</summary>
        public static PhotoFilterSettings ForAccessibility(PhotoFilterSettings settings, bool reduceFlashes)
        {
            if (!reduceFlashes)
            {
                return settings;
            }

            settings.BloomIntensity = Mathf.Min(settings.BloomIntensity, ReducedFlashMaxBloom);
            settings.GrainIntensity *= ReducedFlashGrainScale;
            return settings;
        }

        /// <summary>Identity values (white filters, no vignette/grain/bloom), disabled.</summary>
        public static PhotoFilterSettings Neutral() => new PhotoFilterSettings
        {
            Enabled = false,
            ColorFilter = Color.white,
            VignetteSmoothness = 0.4f,
            VignetteColor = Color.black,
            BloomThreshold = 0.9f,
            BloomTint = Color.white,
        };
    }
}
