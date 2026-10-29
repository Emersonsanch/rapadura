using UnityEngine;
using UnityEngine.UIElements;

namespace Rapadura.UI.Common
{
    /// <summary>
    /// Builds a tiny vertical gradient texture for UI Toolkit backgrounds (USS has no
    /// linear-gradient): a 1 px wide column, bilinear + clamped, stretched over the element with
    /// <c>-unity-background-scale-mode: stretch-to-fill</c>. Cheap (one texture per screen) and
    /// asset-free, so the themed screens need no imported art. The caller owns the texture and
    /// destroys it (<see cref="Release"/>).
    /// </summary>
    public static class UiGradientTexture
    {
        public const int DefaultHeight = 128;

        /// <summary>
        /// Evenly spaced colour stops from TOP (index 0) to BOTTOM (last). One stop = flat colour.
        /// Null/empty stops yield null.
        /// </summary>
        public static Texture2D CreateVertical(Color[] stops, int height = DefaultHeight)
        {
            if (stops == null || stops.Length == 0)
            {
                return null;
            }

            height = Mathf.Max(2, height);
            var texture = new Texture2D(1, height, TextureFormat.RGBA32, false)
            {
                name = "UiGradient",
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear,
                hideFlags = HideFlags.DontSave,
            };

            var pixels = new Color[height];
            for (int y = 0; y < height; y++)
            {
                // Texture rows go bottom-up: row 0 is the bottom of the element.
                float fromTop = 1f - y / (float)(height - 1);
                pixels[y] = Evaluate(stops, fromTop);
            }

            texture.SetPixels(pixels);
            texture.Apply(false, false);
            return texture;
        }

        /// <summary>Colour at <paramref name="t"/> (0 = first stop / top, 1 = last stop / bottom) of evenly spaced stops.</summary>
        public static Color Evaluate(Color[] stops, float t)
        {
            if (stops == null || stops.Length == 0)
            {
                return Color.clear;
            }

            if (stops.Length == 1)
            {
                return stops[0];
            }

            float scaled = Mathf.Clamp01(t) * (stops.Length - 1);
            int index = Mathf.Min(Mathf.FloorToInt(scaled), stops.Length - 2);
            return Color.Lerp(stops[index], stops[index + 1], scaled - index);
        }

        /// <summary>Creates the gradient and sets it as <paramref name="element"/>'s background; returns the texture (null when the element is null).</summary>
        public static Texture2D ApplyVertical(VisualElement element, params Color[] stops)
        {
            if (element == null)
            {
                return null;
            }

            Texture2D texture = CreateVertical(stops);
            if (texture != null)
            {
                element.style.backgroundImage = new StyleBackground(texture);
            }

            return texture;
        }

        /// <summary>Destroys a texture made by this class (safe in Edit Mode tests and with null).</summary>
        public static void Release(Texture2D texture)
        {
            if (texture == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Object.Destroy(texture);
            }
            else
            {
                Object.DestroyImmediate(texture);
            }
        }
    }
}
