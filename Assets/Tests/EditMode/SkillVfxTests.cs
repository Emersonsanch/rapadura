using NUnit.Framework;
using Rapadura.Core.DI;
using Rapadura.Gameplay.Skills;
using Rapadura.Gameplay.Skills.Vfx;
using UnityEditor;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// EditMode tests for the procedural skill VFX system: preset resolution order
    /// (override &gt; element &gt; category &gt; default &gt; fallback), the seeded Resources library,
    /// skill → visual category classification, and the "reduce flashes" accessibility policy.
    /// The reduce-flash policy is tested through its pure functions only, so these tests never write
    /// AccessibilitySettings' PlayerPrefs keys.
    /// </summary>
    public class SkillVfxTests
    {
        private SkillVfxLibrary _library;
        private GameObject _host;

        [SetUp]
        public void SetUp()
        {
            ServiceLocator.Clear();
            _library = ScriptableObject.CreateInstance<SkillVfxLibrary>();
            _library.Initialize(
                "default",
                new[]
                {
                    new SkillVfxPreset { presetId = "default" },
                    new SkillVfxPreset { presetId = "fire" },
                    new SkillVfxPreset { presetId = "heal" },
                    new SkillVfxPreset { presetId = "custom" },
                },
                new[] { new SkillVfxElementBinding(ElementType.Fire, "fire") },
                new[] { new SkillVfxCategoryBinding(SkillVfxCategory.Heal, "heal") });
        }

        [TearDown]
        public void TearDown()
        {
            if (_host != null)
            {
                Object.DestroyImmediate(_host);
            }

            Object.DestroyImmediate(_library);
            ServiceLocator.Clear();
        }

        // ---------------------------------------------------------------
        // Resolution order
        // ---------------------------------------------------------------

        [Test]
        public void Resolve_OverrideId_WinsOverElementAndCategory()
        {
            SkillVfxPreset preset = _library.Resolve("custom", ElementType.Fire, SkillVfxCategory.Heal);

            Assert.AreEqual("custom", preset.presetId);
        }

        [Test]
        public void Resolve_UnknownOverride_FallsThroughToElement()
        {
            SkillVfxPreset preset = _library.Resolve("does_not_exist", ElementType.Fire, SkillVfxCategory.Heal);

            Assert.AreEqual("fire", preset.presetId);
        }

        [Test]
        public void Resolve_Element_WinsOverCategory()
        {
            SkillVfxPreset preset = _library.Resolve(string.Empty, ElementType.Fire, SkillVfxCategory.Heal);

            Assert.AreEqual("fire", preset.presetId);
        }

        [Test]
        public void Resolve_UnboundElement_UsesCategory()
        {
            SkillVfxPreset preset = _library.Resolve(null, ElementType.Physical, SkillVfxCategory.Heal);

            Assert.AreEqual("heal", preset.presetId);
        }

        [Test]
        public void Resolve_NothingBound_UsesDefault()
        {
            SkillVfxPreset preset = _library.Resolve(null, ElementType.Ice, SkillVfxCategory.Projectile);

            Assert.AreEqual("default", preset.presetId);
        }

        [Test]
        public void Resolve_MissingDefault_ReturnsNonNullFallback()
        {
            var empty = ScriptableObject.CreateInstance<SkillVfxLibrary>();
            try
            {
                empty.Initialize("missing", null, null, null);

                Assert.IsNotNull(empty.Resolve(null, ElementType.Fire, SkillVfxCategory.Area));
            }
            finally
            {
                Object.DestroyImmediate(empty);
            }
        }

        [Test]
        public void Resolve_BindingToUnknownPreset_IsIgnored()
        {
            _library.Initialize(
                "default",
                new[] { new SkillVfxPreset { presetId = "default" } },
                new[] { new SkillVfxElementBinding(ElementType.Fire, "typo") },
                null);

            Assert.AreEqual("default", _library.Resolve(null, ElementType.Fire, SkillVfxCategory.Area).presetId);
        }

        [Test]
        public void Resolve_SkillDefinition_UsesVfxPresetIdOverrideField()
        {
            SkillDefinition skill = CreateSkill(ElementType.Fire, damage: 10f, range: 12f, area: 0f, vfxPresetId: "custom");
            try
            {
                Assert.AreEqual("custom", _library.Resolve(skill).presetId);
            }
            finally
            {
                Object.DestroyImmediate(skill);
            }
        }

        [Test]
        public void Resolve_SkillDefinition_WithoutOverride_UsesElement()
        {
            SkillDefinition skill = CreateSkill(ElementType.Fire, damage: 10f, range: 12f, area: 0f);
            try
            {
                Assert.AreEqual("fire", _library.Resolve(skill).presetId);
            }
            finally
            {
                Object.DestroyImmediate(skill);
            }
        }

        [Test]
        public void SeededLibraryAsset_LoadsFromResources_AndMatchesBuiltInDefaults()
        {
            var asset = Resources.Load<SkillVfxLibrary>(SkillVfxLibrary.ResourcePath);
            Assert.IsNotNull(asset, $"Expected a SkillVfxLibrary at Resources/{SkillVfxLibrary.ResourcePath}.");

            SkillVfxLibrary builtIn = SkillVfxLibrary.CreateBuiltInDefaults();
            try
            {
                Assert.AreEqual(builtIn.Presets.Count, asset.Presets.Count);
                Assert.AreEqual("fire", asset.Resolve(null, ElementType.Fire, SkillVfxCategory.Projectile).presetId);
                Assert.AreEqual("heal", asset.Resolve(null, ElementType.None, SkillVfxCategory.Heal).presetId);
                Assert.AreEqual("ultimate", asset.Resolve(null, ElementType.Physical, SkillVfxCategory.Ultimate).presetId);

                foreach (SkillVfxCategory category in System.Enum.GetValues(typeof(SkillVfxCategory)))
                {
                    Assert.AreEqual(
                        builtIn.Resolve(null, ElementType.None, category).presetId,
                        asset.Resolve(null, ElementType.None, category).presetId,
                        $"Seeded asset and built-in defaults disagree for category {category}.");
                }

                foreach (ElementType element in System.Enum.GetValues(typeof(ElementType)))
                {
                    Assert.AreEqual(
                        builtIn.Resolve(null, element, SkillVfxCategory.Projectile).presetId,
                        asset.Resolve(null, element, SkillVfxCategory.Projectile).presetId,
                        $"Seeded asset and built-in defaults disagree for element {element}.");
                }
            }
            finally
            {
                Object.DestroyImmediate(builtIn);
            }
        }

        // ---------------------------------------------------------------
        // Classification
        // ---------------------------------------------------------------

        [Test]
        public void Classify_DerivesCategoryFromSkillData()
        {
            Assert.AreEqual(SkillVfxCategory.Projectile, SkillVfxClassifier.Classify(SkillType.Active, SkillCategory.Offensive, 18f, 0f, 20f, 0f, 0, 0), "arrow");
            Assert.AreEqual(SkillVfxCategory.Projectile, SkillVfxClassifier.Classify(SkillType.Active, SkillCategory.Offensive, 25f, 0f, 12f, 2.5f, 0, 0), "fireball (splash)");
            Assert.AreEqual(SkillVfxCategory.Area, SkillVfxClassifier.Classify(SkillType.Active, SkillCategory.Offensive, 30f, 0f, 5f, 4f, 0, 0), "earthquake");
            Assert.AreEqual(SkillVfxCategory.Heal, SkillVfxClassifier.Classify(SkillType.Active, SkillCategory.Support, 0f, 30f, 5f, 0f, 0, 0), "heal");
            Assert.AreEqual(SkillVfxCategory.Buff, SkillVfxClassifier.Classify(SkillType.Active, SkillCategory.Defensive, 0f, 0f, 5f, 0f, 1, 0), "shield");
            Assert.AreEqual(SkillVfxCategory.Debuff, SkillVfxClassifier.Classify(SkillType.Active, SkillCategory.Utility, 0f, 0f, 6f, 6f, 0, 1), "taunt");
            Assert.AreEqual(SkillVfxCategory.Summon, SkillVfxClassifier.Classify(SkillType.Active, SkillCategory.Summon, 10f, 0f, 10f, 3f, 0, 1), "turret");
            Assert.AreEqual(SkillVfxCategory.Ultimate, SkillVfxClassifier.Classify(SkillType.Ultimate, SkillCategory.Summon, 45f, 0f, 12f, 7f, 1, 1), "ultimate beats summon");
        }

        // ---------------------------------------------------------------
        // Reduce-flash policy
        // ---------------------------------------------------------------

        [Test]
        public void FlashPolicy_Disabled_LeavesBurstUntouched()
        {
            var burst = new SkillVfxBurst(10, new Color(1f, 0.5f, 0.2f, 1f), 0.2f, isFlash: true, isFlashLike: true);

            SkillVfxBurst result = SkillVfxFlashPolicy.Apply(burst, reduceFlashes: false);

            Assert.AreEqual(10, result.Count);
            Assert.AreEqual(burst.Color, result.Color);
            Assert.AreEqual(0.2f, result.Size, 1e-5f);
        }

        [Test]
        public void FlashPolicy_Enabled_SkipsFlashSprites()
        {
            var flash = new SkillVfxBurst(1, Color.white, 1f, isFlash: true, isFlashLike: true);

            Assert.IsTrue(SkillVfxFlashPolicy.Apply(flash, reduceFlashes: true).IsSkipped);
        }

        [Test]
        public void FlashPolicy_Enabled_DimsAndThinsFlashLikeBursts()
        {
            var burst = new SkillVfxBurst(10, new Color(1f, 1f, 1f, 1f), 0.2f, isFlash: false, isFlashLike: true);

            SkillVfxBurst result = SkillVfxFlashPolicy.Apply(burst, reduceFlashes: true);

            Assert.AreEqual(5, result.Count);
            Assert.Less(result.Color.a, 1f);
            Assert.Less(result.Color.r, 1f);
            Assert.Less(result.Size, 0.2f);
        }

        [Test]
        public void FlashPolicy_Enabled_NeverDropsFlashLikeBurstToZero()
        {
            var burst = new SkillVfxBurst(1, Color.white, 0.2f, isFlash: false, isFlashLike: true);

            Assert.AreEqual(1, SkillVfxFlashPolicy.Apply(burst, reduceFlashes: true).Count);
        }

        [Test]
        public void FlashPolicy_Enabled_LeavesNonFlashLikeBurstsAlone()
        {
            var burst = new SkillVfxBurst(12, new Color(0.4f, 0.3f, 0.2f, 1f), 0.24f, isFlash: false, isFlashLike: false);

            SkillVfxBurst result = SkillVfxFlashPolicy.Apply(burst, reduceFlashes: true);

            Assert.AreEqual(12, result.Count);
            Assert.AreEqual(burst.Color, result.Color);
        }

        [Test]
        public void FlashPolicy_UseAdditive_DropsGlowOnlyForFlashLikePresetsWhenReduced()
        {
            Assert.IsTrue(SkillVfxFlashPolicy.UseAdditive(true, true, false));
            Assert.IsFalse(SkillVfxFlashPolicy.UseAdditive(true, true, true));
            Assert.IsTrue(SkillVfxFlashPolicy.UseAdditive(true, false, true));
            Assert.IsFalse(SkillVfxFlashPolicy.UseAdditive(false, false, false));
        }

        [Test]
        public void ManagerBuild_WithReduceFlashes_SkipsFlashAndDimsFlashLikePreset()
        {
            var fire = new SkillVfxPreset("fire", new Color(1f, 0.55f, 0.1f), new Color(1f, 0.85f, 0.25f), 14, 0.2f, 3f, 0.5f, -0.6f, isFlashLike: true);

            SkillVfxPlayback normal = SkillVfxManager.Build(fire, false, 1f, 1f, SkillVfxShape.Ring, 2f, true, 1.5f);
            SkillVfxPlayback reduced = SkillVfxManager.Build(fire, true, 1f, 1f, SkillVfxShape.Ring, 2f, true, 1.5f);

            Assert.IsFalse(normal.Flash.IsSkipped);
            Assert.IsTrue(reduced.Flash.IsSkipped);
            Assert.Less(reduced.Burst.Count, normal.Burst.Count);
            Assert.Less(reduced.SecondaryColor.a, normal.SecondaryColor.a);
            Assert.IsTrue(normal.Additive);
            Assert.IsFalse(reduced.Additive);
        }

        // ---------------------------------------------------------------
        // Event-driven spawning (pooled)
        // ---------------------------------------------------------------

        [Test]
        public void Manager_OnSkillCast_SpawnsPooledEmitters()
        {
            _host = new GameObject("SkillVfxManagerTestHost");
            var manager = _host.AddComponent<SkillVfxManager>();
            manager.SetLibraryForTests(SkillVfxLibrary.CreateBuiltInDefaults());
            var caster = new GameObject("Caster");
            caster.transform.SetParent(_host.transform);

            SkillDefinition skill = CreateSkill(ElementType.Fire, damage: 25f, range: 12f, area: 2.5f);
            try
            {
                manager.OnSkillCastForTests(new SkillCastEvent(caster, skill, new Vector3(0f, 0f, 8f)));

                // Cast burst at the caster + travelling projectile (impact is spawned on arrival).
                Assert.AreEqual(2, manager.ActiveEmitterCountForTests);
            }
            finally
            {
                Object.DestroyImmediate(skill);
            }
        }

        private static SkillDefinition CreateSkill(ElementType element, float damage, float range, float area, string vfxPresetId = "")
        {
            var skill = ScriptableObject.CreateInstance<SkillDefinition>();
            var so = new SerializedObject(skill);
            so.FindProperty("_skillId").stringValue = "skill_vfx_test";
            so.FindProperty("_skillType").enumValueIndex = (int)SkillType.Active;
            so.FindProperty("_category").enumValueIndex = (int)SkillCategory.Offensive;
            so.FindProperty("_element").enumValueIndex = (int)element;
            so.FindProperty("_baseDamage").floatValue = damage;
            so.FindProperty("_range").floatValue = range;
            so.FindProperty("_areaRadius").floatValue = area;
            so.FindProperty("_vfxPresetId").stringValue = vfxPresetId;
            so.ApplyModifiedPropertiesWithoutUndo();
            return skill;
        }
    }
}
