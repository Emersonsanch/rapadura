using NUnit.Framework;
using Rapadura.Core.Accessibility;
using Rapadura.Gameplay.Items;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Rapadura.Tests
{
    /// <summary>
    /// EditMode tests for the Fase 6 accessibility systems: settings persistence
    /// (<see cref="AccessibilitySettings"/>), colorblind palette mapping
    /// (<see cref="ColorblindPaletteMap"/>) and input rebind save/load
    /// (<see cref="InputRebindManager"/>).
    /// </summary>
    public class AccessibilityTests
    {
        private AccessibilitySettings _settings;

        [SetUp]
        public void SetUp()
        {
            ClearPrefs();
        }

        [TearDown]
        public void TearDown()
        {
            _settings?.Shutdown();
            _settings = null;
            ClearPrefs();
        }

        private static void ClearPrefs()
        {
            PlayerPrefs.DeleteKey("Accessibility.FontScale");
            PlayerPrefs.DeleteKey("Accessibility.HighContrast");
            PlayerPrefs.DeleteKey("Accessibility.ColorblindMode");
            PlayerPrefs.DeleteKey("Accessibility.ScreenShakeIntensity");
            PlayerPrefs.DeleteKey("Accessibility.ReduceFlashEffects");
            PlayerPrefs.DeleteKey("Accessibility.InputBindings.TestAsset");
        }

        private AccessibilitySettings CreateSettings()
        {
            _settings = new AccessibilitySettings();
            _settings.Initialize();
            return _settings;
        }

        // ------------------------------------------------------------------
        // AccessibilitySettings persistence.
        // ------------------------------------------------------------------

        [Test]
        public void Initialize_DefaultsAreSane()
        {
            AccessibilitySettings settings = CreateSettings();

            Assert.AreEqual(1f, settings.FontScale);
            Assert.IsFalse(settings.HighContrast);
            Assert.AreEqual(ColorblindMode.None, settings.ActiveColorblindMode);
            Assert.AreEqual(1f, settings.ScreenShakeIntensity);
            Assert.IsFalse(settings.ReduceFlashEffects);
        }

        [Test]
        public void SetFontScale_ClampsToRange()
        {
            AccessibilitySettings settings = CreateSettings();

            settings.SetFontScale(10f);
            Assert.AreEqual(AccessibilitySettings.MaxFontScale, settings.FontScale);

            settings.SetFontScale(-5f);
            Assert.AreEqual(AccessibilitySettings.MinFontScale, settings.FontScale);
        }

        [Test]
        public void Settings_PersistAcrossInstancesViaPlayerPrefs()
        {
            AccessibilitySettings first = CreateSettings();
            first.SetFontScale(1.5f);
            first.SetHighContrast(true);
            first.SetColorblindMode(ColorblindMode.Deuteranopia);
            first.SetScreenShakeIntensity(0.25f);
            first.SetReduceFlashEffects(true);
            first.Shutdown();

            AccessibilitySettings second = new AccessibilitySettings();
            second.Initialize();

            Assert.AreEqual(1.5f, second.FontScale, 0.0001f);
            Assert.IsTrue(second.HighContrast);
            Assert.AreEqual(ColorblindMode.Deuteranopia, second.ActiveColorblindMode);
            Assert.AreEqual(0.25f, second.ScreenShakeIntensity, 0.0001f);
            Assert.IsTrue(second.ReduceFlashEffects);

            second.Shutdown();
        }

        [Test]
        public void ScaleShakeMagnitude_AppliesIntensityMultiplier()
        {
            AccessibilitySettings settings = CreateSettings();
            settings.SetScreenShakeIntensity(0.5f);

            float scaled = settings.ScaleShakeMagnitude(1f);

            Assert.AreEqual(0.5f, scaled, 0.0001f);
        }

        [Test]
        public void SettingsChanged_FiresOnEverySetter()
        {
            AccessibilitySettings settings = CreateSettings();
            int changeCount = 0;
            settings.SettingsChanged += () => changeCount++;

            settings.SetFontScale(1.2f);
            settings.SetHighContrast(true);
            settings.SetColorblindMode(ColorblindMode.Protanopia);
            settings.SetScreenShakeIntensity(0.1f);
            settings.SetReduceFlashEffects(true);

            Assert.AreEqual(5, changeCount);
        }

        // ------------------------------------------------------------------
        // ColorblindPaletteMap.
        // ------------------------------------------------------------------

        [Test]
        public void GetRarityColor_NoneMode_ReturnsReferenceColors()
        {
            Color legendary = ColorblindPaletteMap.GetRarityColor(ItemRarity.Legendary, ColorblindMode.None);

            Assert.AreEqual(new Color(0.95f, 0.60f, 0.10f), legendary);
        }

        [Test]
        public void GetRarityColor_AllKnownRarities_AreDistinctPerMode()
        {
            foreach (ColorblindMode mode in new[] { ColorblindMode.None, ColorblindMode.Protanopia, ColorblindMode.Deuteranopia, ColorblindMode.Tritanopia })
            {
                var seen = new System.Collections.Generic.List<Color>();
                foreach (ItemRarity rarity in ColorblindPaletteMap.KnownRarities)
                {
                    Color color = ColorblindPaletteMap.GetRarityColor(rarity, mode);
                    Assert.IsFalse(seen.Contains(color), $"Duplicate color for mode {mode} at rarity {rarity}");
                    seen.Add(color);
                }
            }
        }

        [Test]
        public void GetRarityColor_ProtanopiaAndDeuteranopia_ShareTheSameSafeTable()
        {
            foreach (ItemRarity rarity in ColorblindPaletteMap.KnownRarities)
            {
                Color protan = ColorblindPaletteMap.GetRarityColor(rarity, ColorblindMode.Protanopia);
                Color deutan = ColorblindPaletteMap.GetRarityColor(rarity, ColorblindMode.Deuteranopia);
                Assert.AreEqual(protan, deutan);
            }
        }

        [Test]
        public void GetRarityColor_DifferentModes_ProduceDifferentPaletteForSameRarity()
        {
            Color none = ColorblindPaletteMap.GetRarityColor(ItemRarity.Rare, ColorblindMode.None);
            Color tritan = ColorblindPaletteMap.GetRarityColor(ItemRarity.Rare, ColorblindMode.Tritanopia);

            Assert.AreNotEqual(none, tritan);
        }

        // ------------------------------------------------------------------
        // InputRebindManager save/load.
        // ------------------------------------------------------------------

        private static InputActionAsset CreateTestAsset()
        {
            var asset = ScriptableObject.CreateInstance<InputActionAsset>();
            asset.name = "TestAsset";
            InputActionMap map = asset.AddActionMap("TestMap");
            InputAction action = map.AddAction("TestAction", InputActionType.Button, "<Keyboard>/e");
            return asset;
        }

        [Test]
        public void SaveAndLoadBindingOverrides_RoundTripsThroughPlayerPrefs()
        {
            InputActionAsset asset = CreateTestAsset();
            InputAction action = asset["TestMap/TestAction"];

            action.ApplyBindingOverride(0, new InputBinding { overridePath = "<Keyboard>/f" });

            var manager = new InputRebindManager(asset);
            manager.SaveBindingOverrides();

            // Fresh asset simulating a new session, with no overrides applied yet.
            InputActionAsset reloadedAsset = CreateTestAsset();
            var reloadedManager = new InputRebindManager(reloadedAsset);
            bool loaded = reloadedManager.LoadBindingOverrides();

            Assert.IsTrue(loaded);
            InputAction reloadedAction = reloadedAsset["TestMap/TestAction"];
            Assert.AreEqual("<Keyboard>/f", reloadedAction.bindings[0].overridePath);

            Object.DestroyImmediate(asset);
            Object.DestroyImmediate(reloadedAsset);
        }

        [Test]
        public void LoadBindingOverrides_ReturnsFalse_WhenNothingWasSaved()
        {
            InputActionAsset asset = CreateTestAsset();
            var manager = new InputRebindManager(asset);

            bool loaded = manager.LoadBindingOverrides();

            Assert.IsFalse(loaded);
            Object.DestroyImmediate(asset);
        }

        [Test]
        public void ResetBinding_ClearsOverride()
        {
            InputActionAsset asset = CreateTestAsset();
            InputAction action = asset["TestMap/TestAction"];
            action.ApplyBindingOverride(0, new InputBinding { overridePath = "<Keyboard>/g" });

            var manager = new InputRebindManager(asset);
            manager.ResetBinding(action, 0);

            Assert.IsNull(action.bindings[0].overridePath);
            Object.DestroyImmediate(asset);
        }
    }
}
