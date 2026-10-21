using System;
using System.Collections.Generic;
using UnityEngine;

namespace Rapadura.Gameplay.Skills.Vfx
{
    /// <summary>
    /// Data-driven table of procedural skill VFX presets, loaded by <see cref="SkillVfxManager"/>
    /// from <c>Resources/Vfx/SkillVfxLibrary</c>. Resolution order for a skill:
    ///   1. <see cref="SkillDefinition.VfxPresetId"/> (per-skill override), if it names a known preset;
    ///   2. the binding for the skill's <see cref="ElementType"/>;
    ///   3. the binding for its derived <see cref="SkillVfxCategory"/>;
    ///   4. <see cref="DefaultPresetId"/>;
    ///   5. a hardcoded fallback preset (never returns null).
    /// The preset only controls the look; the spawn pattern always follows the category, so a Fire
    /// projectile and a Fire area share colors but not shape.
    /// </summary>
    [CreateAssetMenu(fileName = "SkillVfxLibrary", menuName = "Rapadura/Skills/Skill VFX Library", order = 20)]
    public class SkillVfxLibrary : ScriptableObject
    {
        public const string ResourcePath = "Vfx/SkillVfxLibrary";

        [SerializeField] private string _defaultPresetId = "default";
        [SerializeField] private List<SkillVfxPreset> _presets = new List<SkillVfxPreset>();
        [SerializeField] private List<SkillVfxElementBinding> _elementBindings = new List<SkillVfxElementBinding>();
        [SerializeField] private List<SkillVfxCategoryBinding> _categoryBindings = new List<SkillVfxCategoryBinding>();

        private static readonly SkillVfxPreset FallbackPreset = new SkillVfxPreset();

        private Dictionary<string, SkillVfxPreset> _presetsById;
        private Dictionary<ElementType, SkillVfxPreset> _presetsByElement;
        private Dictionary<SkillVfxCategory, SkillVfxPreset> _presetsByCategory;

        public string DefaultPresetId => _defaultPresetId;
        public IReadOnlyList<SkillVfxPreset> Presets => _presets;

        private void OnEnable()
        {
            InvalidateCache();
        }

        private void OnValidate()
        {
            InvalidateCache();
        }

        /// <summary>Replaces the library contents in code (runtime fallback when no asset exists, and EditMode tests).</summary>
        public void Initialize(string defaultPresetId, IEnumerable<SkillVfxPreset> presets, IEnumerable<SkillVfxElementBinding> elementBindings, IEnumerable<SkillVfxCategoryBinding> categoryBindings)
        {
            _defaultPresetId = defaultPresetId ?? string.Empty;
            _presets = presets != null ? new List<SkillVfxPreset>(presets) : new List<SkillVfxPreset>();
            _elementBindings = elementBindings != null ? new List<SkillVfxElementBinding>(elementBindings) : new List<SkillVfxElementBinding>();
            _categoryBindings = categoryBindings != null ? new List<SkillVfxCategoryBinding>(categoryBindings) : new List<SkillVfxCategoryBinding>();
            InvalidateCache();
        }

        public bool TryGetPreset(string presetId, out SkillVfxPreset preset)
        {
            EnsureCache();
            preset = null;
            return !string.IsNullOrEmpty(presetId) && _presetsById.TryGetValue(presetId, out preset);
        }

        public SkillVfxPreset Resolve(SkillDefinition skill)
        {
            if (skill == null)
            {
                return Resolve(null, ElementType.None, SkillVfxCategory.Projectile);
            }

            return Resolve(skill.VfxPresetId, skill.Element, SkillVfxClassifier.Classify(skill));
        }

        /// <summary>Resolves override &gt; element &gt; category &gt; default &gt; built-in fallback. Allocation-free after the first call.</summary>
        public SkillVfxPreset Resolve(string overridePresetId, ElementType element, SkillVfxCategory category)
        {
            EnsureCache();

            if (TryGetPreset(overridePresetId, out SkillVfxPreset preset))
            {
                return preset;
            }

            if (_presetsByElement.TryGetValue(element, out preset))
            {
                return preset;
            }

            if (_presetsByCategory.TryGetValue(category, out preset))
            {
                return preset;
            }

            if (TryGetPreset(_defaultPresetId, out preset))
            {
                return preset;
            }

            return FallbackPreset;
        }

        private void InvalidateCache()
        {
            _presetsById = null;
            _presetsByElement = null;
            _presetsByCategory = null;
        }

        private void EnsureCache()
        {
            if (_presetsById != null)
            {
                return;
            }

            _presetsById = new Dictionary<string, SkillVfxPreset>(StringComparer.Ordinal);
            _presetsByElement = new Dictionary<ElementType, SkillVfxPreset>();
            _presetsByCategory = new Dictionary<SkillVfxCategory, SkillVfxPreset>();

            if (_presets != null)
            {
                foreach (SkillVfxPreset preset in _presets)
                {
                    if (preset != null && !string.IsNullOrEmpty(preset.presetId))
                    {
                        _presetsById[preset.presetId] = preset;
                    }
                }
            }

            if (_elementBindings != null)
            {
                foreach (SkillVfxElementBinding binding in _elementBindings)
                {
                    if (binding != null && !string.IsNullOrEmpty(binding.presetId) && _presetsById.TryGetValue(binding.presetId, out SkillVfxPreset preset))
                    {
                        _presetsByElement[binding.element] = preset;
                    }
                }
            }

            if (_categoryBindings != null)
            {
                foreach (SkillVfxCategoryBinding binding in _categoryBindings)
                {
                    if (binding != null && !string.IsNullOrEmpty(binding.presetId) && _presetsById.TryGetValue(binding.presetId, out SkillVfxPreset preset))
                    {
                        _presetsByCategory[binding.category] = preset;
                    }
                }
            }
        }

        /// <summary>
        /// In-memory library with the same presets/bindings as the seeded
        /// <c>Resources/Vfx/SkillVfxLibrary.asset</c>, used when that asset is missing so skills
        /// still get effects. Physical/None elements are deliberately unbound so they fall through
        /// to the category look.
        /// </summary>
        public static SkillVfxLibrary CreateBuiltInDefaults()
        {
            var library = CreateInstance<SkillVfxLibrary>();
            library.name = "SkillVfxLibrary_BuiltIn";
            library.hideFlags = HideFlags.DontSave;

            var presets = new[]
            {
                new SkillVfxPreset("default", new Color(1f, 0.92f, 0.6f), new Color(1f, 0.75f, 0.35f), 10, 0.18f, 2.5f, 0.45f, 0f, false),
                new SkillVfxPreset("projectile", new Color(1f, 0.95f, 0.75f), new Color(0.9f, 0.8f, 0.55f), 8, 0.14f, 3f, 0.35f, 0.4f, false),
                new SkillVfxPreset("area", new Color(1f, 0.85f, 0.5f), new Color(0.85f, 0.6f, 0.3f), 16, 0.22f, 3.5f, 0.5f, 0.6f, false),
                new SkillVfxPreset("heal", new Color(0.45f, 1f, 0.55f), new Color(0.85f, 1f, 0.7f), 12, 0.16f, 1.2f, 0.8f, -0.4f, false),
                new SkillVfxPreset("buff", new Color(1f, 0.9f, 0.45f), new Color(1f, 1f, 0.8f), 12, 0.14f, 1f, 0.7f, -0.3f, false),
                new SkillVfxPreset("debuff", new Color(0.55f, 0.25f, 0.7f), new Color(0.3f, 0.1f, 0.35f), 12, 0.2f, 1.5f, 0.7f, 0.5f, false, false),
                new SkillVfxPreset("summon", new Color(0.6f, 0.85f, 1f), new Color(1f, 1f, 1f), 14, 0.18f, 2f, 0.7f, -0.6f, false),
                new SkillVfxPreset("ultimate", new Color(1f, 0.85f, 0.3f), new Color(1f, 0.5f, 0.15f), 20, 0.26f, 4.5f, 0.6f, 0.2f, true),
                new SkillVfxPreset("fire", new Color(1f, 0.55f, 0.1f), new Color(1f, 0.85f, 0.25f), 14, 0.2f, 3f, 0.5f, -0.6f, true),
                new SkillVfxPreset("ice", new Color(0.6f, 0.9f, 1f), new Color(0.9f, 0.98f, 1f), 12, 0.16f, 2.8f, 0.55f, 0.8f, false),
                new SkillVfxPreset("lightning", new Color(0.75f, 0.85f, 1f), new Color(1f, 1f, 0.6f), 10, 0.12f, 6f, 0.25f, 0f, true),
                new SkillVfxPreset("poison", new Color(0.45f, 0.9f, 0.2f), new Color(0.25f, 0.55f, 0.1f), 12, 0.2f, 1.4f, 0.8f, -0.2f, false, false),
                new SkillVfxPreset("earth", new Color(0.6f, 0.45f, 0.28f), new Color(0.4f, 0.32f, 0.2f), 16, 0.24f, 3.5f, 0.6f, 1.6f, false, false),
                new SkillVfxPreset("wind", new Color(0.85f, 1f, 0.92f), new Color(0.6f, 0.9f, 0.85f), 12, 0.14f, 4f, 0.45f, -0.1f, false),
                new SkillVfxPreset("holy", new Color(1f, 0.95f, 0.6f), new Color(1f, 1f, 0.95f), 12, 0.18f, 1.8f, 0.6f, -0.5f, true),
                new SkillVfxPreset("dark", new Color(0.4f, 0.15f, 0.55f), new Color(0.12f, 0.05f, 0.18f), 12, 0.22f, 1.6f, 0.7f, -0.2f, false, false),
                new SkillVfxPreset("arcane", new Color(0.75f, 0.4f, 1f), new Color(0.4f, 0.6f, 1f), 12, 0.18f, 2.6f, 0.55f, -0.2f, true),
            };

            var elements = new[]
            {
                new SkillVfxElementBinding(ElementType.Fire, "fire"),
                new SkillVfxElementBinding(ElementType.Ice, "ice"),
                new SkillVfxElementBinding(ElementType.Lightning, "lightning"),
                new SkillVfxElementBinding(ElementType.Poison, "poison"),
                new SkillVfxElementBinding(ElementType.Earth, "earth"),
                new SkillVfxElementBinding(ElementType.Wind, "wind"),
                new SkillVfxElementBinding(ElementType.Holy, "holy"),
                new SkillVfxElementBinding(ElementType.Dark, "dark"),
                new SkillVfxElementBinding(ElementType.Arcane, "arcane"),
            };

            var categories = new[]
            {
                new SkillVfxCategoryBinding(SkillVfxCategory.Projectile, "projectile"),
                new SkillVfxCategoryBinding(SkillVfxCategory.Area, "area"),
                new SkillVfxCategoryBinding(SkillVfxCategory.Heal, "heal"),
                new SkillVfxCategoryBinding(SkillVfxCategory.Buff, "buff"),
                new SkillVfxCategoryBinding(SkillVfxCategory.Debuff, "debuff"),
                new SkillVfxCategoryBinding(SkillVfxCategory.Summon, "summon"),
                new SkillVfxCategoryBinding(SkillVfxCategory.Ultimate, "ultimate"),
            };

            library.Initialize("default", presets, elements, categories);
            return library;
        }
    }
}
