using System.Collections.Generic;
using NUnit.Framework;
using Rapadura.Gameplay.Environment;
using Rapadura.Gameplay.World.Biomes;
using UnityEngine;

namespace Rapadura.Tests
{
    public class BiomeArenaDressingTests
    {
        private readonly List<Object> _created = new List<Object>();

        [TearDown]
        public void TearDown()
        {
            BiomeArenaProfiles.Invalidate();
            EnvironmentOverrides.Reset();

            foreach (BiomeArenaDressing dressing in Object.FindObjectsByType<BiomeArenaDressing>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                dressing.Restore();
            }

            GameObject props = GameObject.Find("[BiomeArenaProps]");
            if (props != null)
            {
                Object.DestroyImmediate(props);
            }

            foreach (Object created in _created)
            {
                if (created != null)
                {
                    Object.DestroyImmediate(created);
                }
            }

            _created.Clear();
        }

        private BiomeArenaProfile MakeProfile(BiomeType biome, params BiomeArenaPropRule[] rules)
        {
            var profile = ScriptableObject.CreateInstance<BiomeArenaProfile>();
            profile.SetDataForTests(biome, new List<BiomeArenaPropRule>(rules));
            _created.Add(profile);
            return profile;
        }

        private GameObject MakeObject(string name, bool withRenderer = false)
        {
            GameObject go = withRenderer ? GameObject.CreatePrimitive(PrimitiveType.Cube) : new GameObject();
            go.name = name;
            _created.Add(go);
            return go;
        }

        // ---------------------------------------------------------------
        // Profile resolution
        // ---------------------------------------------------------------

        [Test]
        public void ShouldDress_OnlyWithPendingEncounterAndProfile()
        {
            BiomeArenaProfile profile = MakeProfile(BiomeType.Desert);

            Assert.IsFalse(BiomeArenaRules.ShouldDress(false, profile), "Sandbox Main (no payload) must stay as authored.");
            Assert.IsFalse(BiomeArenaRules.ShouldDress(true, null));
            Assert.IsTrue(BiomeArenaRules.ShouldDress(true, profile));
        }

        [Test]
        public void SelectProfile_PicksFirstMatchingBiome_SkippingNulls()
        {
            BiomeArenaProfile forest = MakeProfile(BiomeType.Forest);
            BiomeArenaProfile desertA = MakeProfile(BiomeType.Desert);
            BiomeArenaProfile desertB = MakeProfile(BiomeType.Desert);
            var candidates = new[] { null, forest, desertA, desertB };

            Assert.AreSame(desertA, BiomeArenaRules.SelectProfile(BiomeType.Desert, candidates));
            Assert.AreSame(forest, BiomeArenaRules.SelectProfile(BiomeType.Forest, candidates));
            Assert.IsNull(BiomeArenaRules.SelectProfile(BiomeType.Caves, candidates));
            Assert.IsNull(BiomeArenaRules.SelectProfile(BiomeType.Caves, null));
        }

        [Test]
        public void Registry_PrefersInjectedAsset_ElseBuiltInDefault()
        {
            BiomeArenaProfile desert = MakeProfile(BiomeType.Desert);
            BiomeArenaProfiles.SetForTests(new[] { desert });

            Assert.AreSame(desert, BiomeArenaProfiles.Get(BiomeType.Desert));
            Assert.IsTrue(BiomeArenaProfiles.HasAsset(BiomeType.Desert));

            Assert.IsFalse(BiomeArenaProfiles.HasAsset(BiomeType.Caves));
            BiomeArenaProfile caves = BiomeArenaProfiles.Get(BiomeType.Caves);
            Assert.IsNotNull(caves, "Missing assets fall back to the built-in default.");
            Assert.AreEqual(BiomeType.Caves, caves.Biome);
            Assert.AreSame(caves, BiomeArenaProfiles.Get(BiomeType.Caves), "Built-in fallback is cached.");
        }

        [Test]
        public void ShippedAssets_ExistForEveryBiome_AndMatchBuiltInTreeRules()
        {
            BiomeArenaProfiles.Invalidate();
            string[] probes = { "NaturalTrees", "Tree_Pine_RO", "Tree_Broadleaf_RO", "Tuft_12", "Rock_Scattered_3", "Ground" };

            foreach (BiomeType biome in System.Enum.GetValues(typeof(BiomeType)))
            {
                Assert.IsTrue(BiomeArenaProfiles.HasAsset(biome), $"Resources/Biomes/Arena/{biome}.asset missing.");
                BiomeArenaProfile asset = BiomeArenaProfiles.Get(biome);
                BiomeArenaProfile builtIn = BiomeArenaProfile.CreateBuiltInDefault(biome);
                _created.Add(builtIn);

                Assert.AreEqual(biome, asset.Biome);
                Assert.AreEqual(builtIn.IsNeutral, asset.IsNeutral, $"{biome}: neutrality differs from the code default.");
                foreach (string probe in probes)
                {
                    Assert.AreEqual(
                        BiomeArenaRules.ResolveAction(probe, null, builtIn.PropRules, out _),
                        BiomeArenaRules.ResolveAction(probe, null, asset.PropRules, out _),
                        $"{biome}: '{probe}' handled differently by the asset and the code default.");
                }
            }
        }

        // ---------------------------------------------------------------
        // Show / hide rules
        // ---------------------------------------------------------------

        [Test]
        public void ResolveAction_FirstMatchWins_AndNoMatchKeeps()
        {
            var rules = new List<BiomeArenaPropRule>
            {
                BiomeArenaPropRule.Keep("Tree_Pine"),
                BiomeArenaPropRule.Hide("Tree_"),
                BiomeArenaPropRule.Recolor("Rock", new Color(1f, 0f, 0f, 0.5f)),
            };

            Assert.AreEqual(BiomeArenaPropAction.Keep, BiomeArenaRules.ResolveAction("Tree_Pine_RO", null, rules, out _));
            Assert.AreEqual(BiomeArenaPropAction.Hide, BiomeArenaRules.ResolveAction("Tree_Broadleaf_RO", null, rules, out _));
            Assert.AreEqual(BiomeArenaPropAction.Tint, BiomeArenaRules.ResolveAction("Rock_Scattered_1", null, rules, out Color tint));
            Assert.AreEqual(new Color(1f, 0f, 0f, 0.5f), tint);
            Assert.AreEqual(BiomeArenaPropAction.Keep, BiomeArenaRules.ResolveAction("Player", null, rules, out Color none));
            Assert.AreEqual(Color.white, none);
            Assert.AreEqual(BiomeArenaPropAction.Keep, BiomeArenaRules.ResolveAction("tree_lowercase", null, rules, out _), "Prefixes are case-sensitive.");
            Assert.AreEqual(BiomeArenaPropAction.Keep, BiomeArenaRules.ResolveAction("Tree_X", null, null, out _));
        }

        [Test]
        public void Matches_RespectsTagFilter_AndIgnoresEmptyPrefix()
        {
            var tagged = new BiomeArenaPropRule("Rock", BiomeArenaPropAction.Hide, tag: "Prop");

            Assert.IsTrue(BiomeArenaRules.Matches(tagged, "Rock_1", "Prop"));
            Assert.IsFalse(BiomeArenaRules.Matches(tagged, "Rock_1", "Untagged"));
            Assert.IsFalse(BiomeArenaRules.Matches(BiomeArenaPropRule.Hide(string.Empty), "Anything", null));
            Assert.IsFalse(BiomeArenaRules.Matches(null, "Anything", null));
            Assert.IsTrue(BiomeArenaRules.AnyRuleUsesTag(new[] { BiomeArenaPropRule.Hide("A"), tagged }));
            Assert.IsFalse(BiomeArenaRules.AnyRuleUsesTag(new[] { BiomeArenaPropRule.Hide("A") }));
        }

        [TestCase(BiomeType.Desert)]
        [TestCase(BiomeType.Caves)]
        public void BuiltIn_DesertAndCaves_HideTreesAndGrass_RecolourRocks(BiomeType biome)
        {
            BiomeArenaProfile profile = BiomeArenaProfile.CreateBuiltInDefault(biome);
            _created.Add(profile);

            foreach (string hidden in new[] { "NaturalTrees", "Tree_Pine_RO", "Tree_Gnarled_RO", "Tree_Broadleaf_RO", "Tuft_7", "Twig_3" })
            {
                Assert.AreEqual(BiomeArenaPropAction.Hide, BiomeArenaRules.ResolveAction(hidden, null, profile.PropRules, out _), $"{biome} should hide {hidden}.");
            }

            foreach (string rock in new[] { "Rocks", "Rock_Scattered_4", "ProceduralRock_9", "Pebble_2" })
            {
                Assert.AreEqual(BiomeArenaPropAction.Tint, BiomeArenaRules.ResolveAction(rock, null, profile.PropRules, out _), $"{biome} should recolour {rock}.");
            }

            Assert.AreEqual(BiomeArenaPropAction.Keep, BiomeArenaRules.ResolveAction("Ground", null, profile.PropRules, out _));
            Assert.IsTrue(profile.TintGround);
            Assert.Greater(profile.Props.Count, 0);
        }

        [Test]
        public void BuiltIn_Mountain_KeepsPinesVisible_HidesBroadleaf()
        {
            BiomeArenaProfile profile = BiomeArenaProfile.CreateBuiltInDefault(BiomeType.Mountain);
            _created.Add(profile);

            Assert.AreEqual(BiomeArenaPropAction.Tint, BiomeArenaRules.ResolveAction("Tree_Pine_RO", null, profile.PropRules, out _));
            Assert.AreEqual(BiomeArenaPropAction.Hide, BiomeArenaRules.ResolveAction("Tree_Broadleaf_RO", null, profile.PropRules, out _));
            Assert.AreEqual(BiomeArenaPropAction.Keep, BiomeArenaRules.ResolveAction("NaturalTrees", null, profile.PropRules, out _), "The tree root must stay so pines remain.");
        }

        [Test]
        public void BuiltIn_Forest_IsNeutral()
        {
            BiomeArenaProfile profile = BiomeArenaProfile.CreateBuiltInDefault(BiomeType.Forest);
            _created.Add(profile);

            Assert.IsTrue(profile.IsNeutral);
            Assert.AreEqual(BiomeArenaPropAction.Keep, BiomeArenaRules.ResolveAction("Tree_Pine_RO", null, profile.PropRules, out _));
        }

        // ---------------------------------------------------------------
        // Tint / scatter maths
        // ---------------------------------------------------------------

        [Test]
        public void ApplyTint_BlendsByTintAlpha_KeepingBaseAlpha()
        {
            var baseColor = new Color(0.2f, 0.4f, 0.6f, 0.9f);

            Assert.AreEqual(baseColor, BiomeArenaRules.ApplyTint(baseColor, new Color(1f, 0f, 0f, 0f)));
            Assert.AreEqual(new Color(1f, 0f, 0f, 0.9f), BiomeArenaRules.ApplyTint(baseColor, new Color(1f, 0f, 0f, 1f)));

            Color half = BiomeArenaRules.ApplyTint(baseColor, new Color(1f, 0f, 0f, 0.5f));
            Assert.AreEqual(0.6f, half.r, 1e-4f);
            Assert.AreEqual(0.2f, half.g, 1e-4f);
            Assert.AreEqual(0.9f, half.a, 1e-4f);
        }

        [Test]
        public void ScatterOnRing_IsDeterministic_AndStaysInsideTheRing()
        {
            var a = new List<Vector2>();
            var b = new List<Vector2>();
            BiomeArenaRules.ScatterOnRing(42, 12, 9f, 26f, a);
            BiomeArenaRules.ScatterOnRing(42, 12, 9f, 26f, b);

            Assert.AreEqual(12, a.Count);
            CollectionAssert.AreEqual(a, b);
            foreach (Vector2 point in a)
            {
                Assert.That(point.magnitude, Is.InRange(9f - 1e-3f, 26f + 1e-3f));
            }

            var other = new List<Vector2>();
            BiomeArenaRules.ScatterOnRing(43, 12, 9f, 26f, other);
            CollectionAssert.AreNotEqual(a, other);

            var none = new List<Vector2>();
            BiomeArenaRules.ScatterOnRing(1, 0, 9f, 26f, none);
            Assert.AreEqual(0, none.Count);
        }

        // ---------------------------------------------------------------
        // Component (Edit Mode objects, no Play Mode)
        // ---------------------------------------------------------------

        [Test]
        public void Dressing_HidesAndTints_ThenRestoreUndoesEverything()
        {
            GameObject tree = MakeObject("Tree_Test");
            GameObject rock = MakeObject("Rock_Test", withRenderer: true);
            GameObject keep = MakeObject("Workbench_Test");
            GameObject host = MakeObject("Arena");
            var dressing = host.AddComponent<BiomeArenaDressing>();

            BiomeArenaProfile profile = MakeProfile(
                BiomeType.Desert,
                BiomeArenaPropRule.Hide("Tree_"),
                BiomeArenaPropRule.Recolor("Rock_", new Color(1f, 0f, 0f, 1f)));

            Renderer rockRenderer = rock.GetComponent<Renderer>();
            Assert.IsFalse(rockRenderer.HasPropertyBlock());

            dressing.Apply(profile, 7);

            Assert.IsTrue(dressing.IsApplied);
            Assert.AreEqual(BiomeType.Desert, dressing.AppliedBiome);
            Assert.IsFalse(tree.activeSelf);
            Assert.IsTrue(keep.activeSelf);
            Assert.IsTrue(rockRenderer.HasPropertyBlock());
            var block = new MaterialPropertyBlock();
            rockRenderer.GetPropertyBlock(block);
            Color tinted = block.GetColor(Shader.PropertyToID("_BaseColor"));
            Assert.AreEqual(1f, tinted.r, 1e-4f);
            Assert.AreEqual(0f, tinted.g, 1e-4f);

            dressing.Restore();

            Assert.IsFalse(dressing.IsApplied);
            Assert.IsTrue(tree.activeSelf);
            Assert.IsFalse(rockRenderer.HasPropertyBlock(), "Restore puts the original (empty) property block back.");
        }

        [Test]
        public void Dressing_ReusesPooledProps_AcrossApplies()
        {
            GameObject host = MakeObject("Arena");
            var dressing = host.AddComponent<BiomeArenaDressing>();
            BiomeArenaProfile profile = MakeProfile(BiomeType.Caves);
            profile.SetDataForTests(
                BiomeType.Caves,
                new List<BiomeArenaPropRule>(),
                new List<BiomeArenaPropSpawn> { new BiomeArenaPropSpawn(BiomeArenaPropKind.Stalagmite, 3, Color.gray, Color.white, 1f, 1f) });

            dressing.Apply(profile, 1);
            Assert.AreEqual(3, dressing.ActivePropCount);
            GameObject root = GameObject.Find("[BiomeArenaProps]");
            Assert.IsNotNull(root);
            int built = root.transform.childCount;

            dressing.Apply(profile, 2);
            Assert.AreEqual(3, dressing.ActivePropCount);
            Assert.AreEqual(built, root.transform.childCount, "Re-applying reuses pooled props instead of building new ones.");

            dressing.Restore();
            Assert.AreEqual(0, dressing.ActivePropCount);
            foreach (Transform child in root.transform)
            {
                Assert.IsFalse(child.gameObject.activeSelf);
            }
        }

        [Test]
        public void Dressing_SetsAndClearsEnvironmentOverrides()
        {
            GameObject host = MakeObject("Arena");
            var dressing = host.AddComponent<BiomeArenaDressing>();
            BiomeArenaProfile profile = MakeProfile(BiomeType.Caves);
            profile.SetAtmosphereForTests(true, Color.black, 0.05f, new Color(0.5f, 0.5f, 0.5f, 1f), 0.25f);

            dressing.Apply(profile, 0);
            Assert.IsTrue(EnvironmentOverrides.HasFogBaseline);
            Assert.AreEqual(0.05f, EnvironmentOverrides.FogBaselineDensity, 1e-5f);
            Assert.AreEqual(0.25f, EnvironmentOverrides.SunIntensityMultiplier, 1e-5f);

            dressing.Restore();
            Assert.IsFalse(EnvironmentOverrides.IsAnyActive);
        }
    }
}
