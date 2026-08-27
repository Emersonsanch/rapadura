using UnityEngine;

namespace Rapadura.Gameplay.Environment
{
    /// <summary>
    /// Applies a small per-instance color tint via <see cref="MaterialPropertyBlock"/> (never
    /// touches the shared material) so the same handful of tree/rock prefab meshes read as varied
    /// individuals when scattered many times across a scene — without this, GPU-instanced identical
    /// meshes with an identical shared material are visually indistinguishable clones. Using a
    /// property block instead of a material instance per object keeps GPU Instancing/SRP batching
    /// intact (a real material instance per object would defeat both).
    /// </summary>
    [RequireComponent(typeof(Renderer))]
    public class TreeColorVariation : MonoBehaviour
    {
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

        [Tooltip("Hue/brightness jitter range applied on top of the material's own color, seeded from this instance's position so it's stable across domain reloads/scene reloads without needing to store anything.")]
        [SerializeField] private float _hueVariation = 0.06f;
        [SerializeField] private float _brightnessVariation = 0.12f;

        private void Awake()
        {
            ApplyVariation();
        }

        private void ApplyVariation()
        {
            var renderer = GetComponent<Renderer>();
            if (renderer == null || renderer.sharedMaterial == null)
            {
                return;
            }

            // Deterministic per-instance seed from world position, not Random.state, so re-entering
            // Play Mode (or the editor re-running this in Edit Mode) always reproduces the same
            // per-tree tint instead of re-rolling it every time.
            int seed = Mathf.RoundToInt(transform.position.x * 1000f) ^ (Mathf.RoundToInt(transform.position.z * 1000f) << 16);
            var random = new System.Random(seed);

            Color.RGBToHSV(renderer.sharedMaterial.color, out float h, out float s, out float v);
            h = Mathf.Repeat(h + ((float)random.NextDouble() * 2f - 1f) * _hueVariation, 1f);
            v = Mathf.Clamp01(v + ((float)random.NextDouble() * 2f - 1f) * _brightnessVariation);
            Color tinted = Color.HSVToRGB(h, s, v);
            tinted.a = renderer.sharedMaterial.color.a;

            var block = new MaterialPropertyBlock();
            renderer.GetPropertyBlock(block);
            block.SetColor(BaseColorId, tinted);
            renderer.SetPropertyBlock(block);
        }

#if UNITY_EDITOR
        /// <summary>Editor-only: re-applies the tint immediately without entering Play Mode, for scatter tools that place many instances at edit time.</summary>
        public void ApplyVariationForTests() => ApplyVariation();
#endif
    }
}
