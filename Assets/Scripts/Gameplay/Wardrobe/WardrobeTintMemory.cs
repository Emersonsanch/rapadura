using System.Collections.Generic;
using UnityEngine;

namespace Rapadura.Gameplay.Wardrobe
{
    /// <summary>
    /// Remembers, per renderer, the colour <see cref="WardrobeVisuals"/> last wrote and the base it started from, so
    /// re-applying starts from the real base (the material colour, or a graduation tint another system wrote since)
    /// instead of tinting its own tint again.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class WardrobeTintMemory : MonoBehaviour
    {
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorId = Shader.PropertyToID("_Color");
        private readonly Dictionary<Renderer, (Color Written, Color Base)> _colors = new Dictionary<Renderer, (Color, Color)>();

        public Color BaseColor(Renderer renderer, Material material, MaterialPropertyBlock block)
        {
            Color materialColor = material.HasProperty(BaseColorId) ? material.GetColor(BaseColorId)
                : material.HasProperty(ColorId) ? material.GetColor(ColorId) : Color.white;
            if (!renderer.HasPropertyBlock()) return materialColor;
            renderer.GetPropertyBlock(block);
            if (!block.HasColor(BaseColorId)) return materialColor;
            Color current = block.GetColor(BaseColorId);
            return _colors.TryGetValue(renderer, out var seen) && seen.Written == current ? seen.Base : current;
        }

        public void Remember(Renderer renderer, Color written, Color baseColor) => _colors[renderer] = (written, baseColor);
    }
}
