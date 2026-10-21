using System.Collections.Generic;
using NUnit.Framework;
using Rapadura.Gameplay.Environment;
using Rapadura.Gameplay.World.Biomes;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// EditMode tests for ambient VFX data/policy: which layers a biome/phase/weather combination
    /// resolves to (built-in defaults and the seeded Resources asset), particle cap clamping and
    /// WorldMap/mobile scaling, and the "reduce flashes" policy (blinking → steady glow). Pure
    /// functions only — no ParticleSystems or AccessibilitySettings PlayerPrefs are touched.
    /// </summary>
    public class AmbientVfxTests
    {
        private AmbientVfxLibrary _library;
        private readonly List<AmbientVfxPreset> _results = new List<AmbientVfxPreset>();

        [SetUp]
        public void SetUp()
        {
            _library = AmbientVfxLibrary.CreateBuiltInDefaults();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_library);
        }

        private string[] Resolve(AmbientVfxLibrary library, BiomeType biome, TimeOfDayPhase phase, WeatherType weather)
        {
            library.ResolveActive(biome, phase, weather, _results);
            var ids = new string[_results.Count];
            for (int i = 0; i < ids.Length; i++)
            {
                ids[i] = _results[i].presetId;
            }

            System.Array.Sort(ids, System.StringComparer.Ordinal);
            return ids;
        }

        // ---------------------------------------------------------------
        // Preset resolution by biome / phase / weather
        // ---------------------------------------------------------------

        [Test]
        public void Forest_FirefliesAtDuskAndNight_LeavesByDay()
        {
            CollectionAssert.AreEqual(new[] { "forest_fireflies" }, Resolve(_library, BiomeType.Forest, TimeOfDayPhase.Night, WeatherType.Clear));
            CollectionAssert.AreEqual(new[] { "forest_fireflies" }, Resolve(_library, BiomeType.Forest, TimeOfDayPhase.Dusk, WeatherType.Clear));
            CollectionAssert.AreEqual(new[] { "forest_leaves" }, Resolve(_library, BiomeType.Forest, TimeOfDayPhase.Day, WeatherType.Clear));
            CollectionAssert.AreEqual(new[] { "forest_leaves" }, Resolve(_library, BiomeType.Forest, TimeOfDayPhase.Dawn, WeatherType.Fog));
        }

        [Test]
        public void Forest_Rain_HidesFireflies()
        {
            CollectionAssert.IsEmpty(Resolve(_library, BiomeType.Forest, TimeOfDayPhase.Night, WeatherType.Rain));
        }

        [Test]
        public void Desert_DustByDay_HeatOnlyMidday_NothingAtNightOrInSandstorm()
        {
            CollectionAssert.AreEqual(new[] { "desert_dust", "desert_heat" }, Resolve(_library, BiomeType.Desert, TimeOfDayPhase.Day, WeatherType.Clear));
            CollectionAssert.AreEqual(new[] { "desert_dust" }, Resolve(_library, BiomeType.Desert, TimeOfDayPhase.Dawn, WeatherType.Clear));
            CollectionAssert.IsEmpty(Resolve(_library, BiomeType.Desert, TimeOfDayPhase.Night, WeatherType.Clear));
            CollectionAssert.IsEmpty(Resolve(_library, BiomeType.Desert, TimeOfDayPhase.Day, WeatherType.Sandstorm));
        }

        [Test]
        public void Mountain_FlurryAndWind_SnowWeatherKeepsOnlyWind()
        {
            foreach (TimeOfDayPhase phase in System.Enum.GetValues(typeof(TimeOfDayPhase)))
            {
                CollectionAssert.AreEqual(new[] { "mountain_flurry", "mountain_wind" }, Resolve(_library, BiomeType.Mountain, phase, WeatherType.Clear), phase.ToString());
            }

            CollectionAssert.AreEqual(new[] { "mountain_wind" }, Resolve(_library, BiomeType.Mountain, TimeOfDayPhase.Day, WeatherType.Snow));
        }

        [Test]
        public void Caves_IgnoreWeatherAndPhase()
        {
            foreach (TimeOfDayPhase phase in System.Enum.GetValues(typeof(TimeOfDayPhase)))
            {
                foreach (WeatherType weather in System.Enum.GetValues(typeof(WeatherType)))
                {
                    CollectionAssert.AreEqual(new[] { "caves_drips", "caves_dust" }, Resolve(_library, BiomeType.Caves, phase, weather), $"{phase}/{weather}");
                }
            }
        }

        [Test]
        public void Resolve_NeverReturnsOtherBiomesLayers()
        {
            foreach (BiomeType biome in System.Enum.GetValues(typeof(BiomeType)))
            {
                foreach (TimeOfDayPhase phase in System.Enum.GetValues(typeof(TimeOfDayPhase)))
                {
                    _library.ResolveActive(biome, phase, WeatherType.Clear, _results);
                    foreach (AmbientVfxPreset preset in _results)
                    {
                        Assert.AreEqual(biome, preset.biome, preset.presetId);
                    }
                }
            }
        }

        [Test]
        public void Masks_MatchEnumBits()
        {
            var preset = new AmbientVfxPreset { phases = AmbientPhaseMask.Night, weathers = AmbientWeatherMask.Snow };
            Assert.IsTrue(preset.AllowsPhase(TimeOfDayPhase.Night));
            Assert.IsFalse(preset.AllowsPhase(TimeOfDayPhase.Day));
            Assert.IsTrue(preset.AllowsWeather(WeatherType.Snow));
            Assert.IsFalse(preset.AllowsWeather(WeatherType.Clear));

            var all = new AmbientVfxPreset { phases = AmbientPhaseMask.All, weathers = AmbientWeatherMask.All };
            foreach (TimeOfDayPhase phase in System.Enum.GetValues(typeof(TimeOfDayPhase)))
            {
                Assert.IsTrue(all.AllowsPhase(phase), phase.ToString());
            }

            foreach (WeatherType weather in System.Enum.GetValues(typeof(WeatherType)))
            {
                Assert.IsTrue(all.AllowsWeather(weather), weather.ToString());
            }
        }

        [Test]
        public void EmptyLibrary_ResolvesNothing_AndClearsResults()
        {
            var empty = ScriptableObject.CreateInstance<AmbientVfxLibrary>();
            try
            {
                empty.Initialize(null);
                _results.Add(new AmbientVfxPreset());
                Assert.AreEqual(0, empty.ResolveActive(BiomeType.Forest, TimeOfDayPhase.Night, WeatherType.Clear, _results));
                CollectionAssert.IsEmpty(_results);
                Assert.IsFalse(empty.TryGetPreset("forest_fireflies", out _));
            }
            finally
            {
                Object.DestroyImmediate(empty);
            }
        }

        // ---------------------------------------------------------------
        // Caps
        // ---------------------------------------------------------------

        [Test]
        public void DefaultPresets_StayWithinParticleCap()
        {
            foreach (AmbientVfxPreset preset in _library.Presets)
            {
                Assert.LessOrEqual(preset.maxParticles, AmbientVfxLibrary.MaxParticlesCap, preset.presetId);
                Assert.LessOrEqual(AmbientVfxLibrary.ScaleCap(preset.maxParticles, 1f), AmbientVfxLibrary.MaxParticlesCap, preset.presetId);
            }
        }

        [Test]
        public void ScaleCap_ClampsToRange()
        {
            Assert.AreEqual(AmbientVfxLibrary.MaxParticlesCap, AmbientVfxLibrary.ScaleCap(500, 1f));
            Assert.AreEqual(AmbientVfxLibrary.MaxParticlesCap, AmbientVfxLibrary.ScaleCap(500, 2f));
            Assert.AreEqual(1, AmbientVfxLibrary.ScaleCap(30, 0.001f));
            Assert.AreEqual(15, AmbientVfxLibrary.ScaleCap(30, 0.5f));
        }

        [Test]
        public void CapMultiplier_LowerInWorldMapAndOnMobile()
        {
            Assert.AreEqual(1f, _library.GetCapMultiplier(false, false), 1e-5f);
            Assert.AreEqual(0.5f, _library.GetCapMultiplier(true, false), 1e-5f);
            Assert.AreEqual(0.75f, _library.GetCapMultiplier(false, true), 1e-5f);
            Assert.AreEqual(0.375f, _library.GetCapMultiplier(true, true), 1e-5f);

            Assert.Less(
                AmbientVfxLibrary.ScaleCap(60, _library.GetCapMultiplier(true, true)),
                AmbientVfxLibrary.ScaleCap(60, _library.GetCapMultiplier(false, false)));
        }

        // ---------------------------------------------------------------
        // Reduce flashes
        // ---------------------------------------------------------------

        [Test]
        public void ReduceFlash_FirefliesStopBlinking_ButKeepGlow()
        {
            Assert.IsTrue(_library.TryGetPreset("forest_fireflies", out AmbientVfxPreset fireflies));

            Assert.IsTrue(AmbientVfxFlashPolicy.ShouldBlink(fireflies, reduceFlashes: false));
            Assert.IsFalse(AmbientVfxFlashPolicy.ShouldBlink(fireflies, reduceFlashes: true));

            // Not flash-like: steady glow keeps its additive blending and colour.
            Assert.IsTrue(AmbientVfxFlashPolicy.UseAdditive(fireflies, reduceFlashes: true));
            Assert.AreEqual(fireflies.primaryColor, AmbientVfxFlashPolicy.ResolveColor(fireflies.primaryColor, fireflies.flashLike, true));
        }

        [Test]
        public void ReduceFlash_LifetimeGradientIsSteady()
        {
            Assert.IsTrue(_library.TryGetPreset("forest_fireflies", out AmbientVfxPreset fireflies));
            var gradient = new Gradient();

            AmbientVfxFlashPolicy.ApplyLifetimeGradient(gradient, fireflies, reduceFlashes: false);
            Assert.IsTrue(AmbientVfxFlashPolicy.IsPulsing(gradient.alphaKeys), "Fireflies should pulse with reduce-flash off.");

            AmbientVfxFlashPolicy.ApplyLifetimeGradient(gradient, fireflies, reduceFlashes: true);
            Assert.IsFalse(AmbientVfxFlashPolicy.IsPulsing(gradient.alphaKeys), "Reduce-flash must give a steady glow.");
        }

        [Test]
        public void ReduceFlash_SparklesAreDimmedAndNotAdditive()
        {
            Assert.IsTrue(_library.TryGetPreset("caves_drips", out AmbientVfxPreset drips));
            Assert.IsTrue(drips.flashLike);

            Assert.IsTrue(AmbientVfxFlashPolicy.UseAdditive(drips, reduceFlashes: false));
            Assert.IsFalse(AmbientVfxFlashPolicy.UseAdditive(drips, reduceFlashes: true));
            Assert.IsFalse(AmbientVfxFlashPolicy.ShouldBlink(drips, reduceFlashes: true));

            Color normal = AmbientVfxFlashPolicy.ResolveColor(drips.primaryColor, drips.flashLike, false);
            Color reduced = AmbientVfxFlashPolicy.ResolveColor(drips.primaryColor, drips.flashLike, true);
            Assert.AreEqual(drips.primaryColor, normal);
            Assert.Less(reduced.a, normal.a);
            Assert.Less(reduced.maxColorComponent, normal.maxColorComponent);
        }

        [Test]
        public void NonBlinkingLayers_NeverPulse()
        {
            foreach (AmbientVfxPreset preset in _library.Presets)
            {
                if (preset.blink)
                {
                    continue;
                }

                Assert.IsFalse(AmbientVfxFlashPolicy.ShouldBlink(preset, false), preset.presetId);
                Assert.IsFalse(AmbientVfxFlashPolicy.IsPulsing(AmbientVfxFlashPolicy.GetAlphaKeys(false)), preset.presetId);
            }
        }

        // ---------------------------------------------------------------
        // Seeded asset
        // ---------------------------------------------------------------

        [Test]
        public void SeededLibraryAsset_LoadsFromResources_AndMatchesBuiltInDefaults()
        {
            var asset = Resources.Load<AmbientVfxLibrary>(AmbientVfxLibrary.ResourcePath);
            Assert.IsNotNull(asset, $"Expected an AmbientVfxLibrary at Resources/{AmbientVfxLibrary.ResourcePath}.");
            Assert.AreEqual(_library.Presets.Count, asset.Presets.Count);
            Assert.AreEqual(_library.WorldMapCapMultiplier, asset.WorldMapCapMultiplier, 1e-5f);
            Assert.AreEqual(_library.MobileCapMultiplier, asset.MobileCapMultiplier, 1e-5f);

            foreach (BiomeType biome in System.Enum.GetValues(typeof(BiomeType)))
            {
                foreach (TimeOfDayPhase phase in System.Enum.GetValues(typeof(TimeOfDayPhase)))
                {
                    foreach (WeatherType weather in System.Enum.GetValues(typeof(WeatherType)))
                    {
                        CollectionAssert.AreEqual(
                            Resolve(_library, biome, phase, weather),
                            Resolve(asset, biome, phase, weather),
                            $"Seeded asset and built-in defaults disagree for {biome}/{phase}/{weather}.");
                    }
                }
            }

            foreach (AmbientVfxPreset preset in asset.Presets)
            {
                Assert.IsTrue(_library.TryGetPreset(preset.presetId, out AmbientVfxPreset builtIn), preset.presetId);
                Assert.AreEqual(builtIn.kind, preset.kind, preset.presetId);
                Assert.AreEqual(builtIn.maxParticles, preset.maxParticles, preset.presetId);
                Assert.AreEqual(builtIn.blink, preset.blink, preset.presetId);
                Assert.AreEqual(builtIn.flashLike, preset.flashLike, preset.presetId);
                Assert.AreEqual(builtIn.additive, preset.additive, preset.presetId);
            }
        }
    }
}
