using System;
using UnityEngine;

namespace Rapadura.Gameplay.Skills.Vfx
{
    /// <summary>
    /// Data-only description of one procedural particle look (colors + particle tuning). The
    /// <see cref="SkillVfxManager"/> turns it into ParticleSystem settings at play time — no
    /// prefabs, textures or other external assets are involved. Plain serializable class (same
    /// style as <see cref="StatModifierDefinition"/>) so it edits nicely inside the library asset.
    /// </summary>
    [Serializable]
    public class SkillVfxPreset
    {
        public string presetId = "default";
        public Color primaryColor = new Color(1f, 0.92f, 0.6f, 1f);
        public Color secondaryColor = new Color(1f, 0.75f, 0.35f, 1f);

        [Tooltip("Base particle count for a single burst. Kept low on purpose (mobile).")]
        [Range(1, 32)] public int burstCount = 10;
        public float particleSize = 0.18f;
        public float particleSpeed = 2.5f;
        public float particleLifetime = 0.45f;
        [Tooltip("Negative values make particles rise (heals, embers), positive makes them fall (debris).")]
        public float gravity = 0f;
        [Tooltip("Particles per second emitted by a travelling projectile / self aura.")]
        public float trailRate = 24f;
        [Tooltip("Bright/strobe-like look (fire, lightning, holy...). Dimmed or skipped when the 'reduce flashes' accessibility option is on.")]
        public bool flashLike = false;
        [Tooltip("Additive (glowy) blending instead of plain alpha blending.")]
        public bool additive = true;

        public SkillVfxPreset()
        {
        }

        public SkillVfxPreset(string id, Color primary, Color secondary, int count, float size, float speed, float lifetime, float gravityModifier, bool isFlashLike, bool isAdditive = true, float trail = 24f)
        {
            presetId = id;
            primaryColor = primary;
            secondaryColor = secondary;
            burstCount = count;
            particleSize = size;
            particleSpeed = speed;
            particleLifetime = lifetime;
            gravity = gravityModifier;
            flashLike = isFlashLike;
            additive = isAdditive;
            trailRate = trail;
        }
    }

    /// <summary>Maps an element to a preset id inside a <see cref="SkillVfxLibrary"/>.</summary>
    [Serializable]
    public class SkillVfxElementBinding
    {
        public ElementType element = ElementType.None;
        public string presetId = string.Empty;

        public SkillVfxElementBinding()
        {
        }

        public SkillVfxElementBinding(ElementType elementType, string id)
        {
            element = elementType;
            presetId = id;
        }
    }

    /// <summary>Maps a visual category to a preset id inside a <see cref="SkillVfxLibrary"/>.</summary>
    [Serializable]
    public class SkillVfxCategoryBinding
    {
        public SkillVfxCategory category = SkillVfxCategory.Projectile;
        public string presetId = string.Empty;

        public SkillVfxCategoryBinding()
        {
        }

        public SkillVfxCategoryBinding(SkillVfxCategory vfxCategory, string id)
        {
            category = vfxCategory;
            presetId = id;
        }
    }
}
