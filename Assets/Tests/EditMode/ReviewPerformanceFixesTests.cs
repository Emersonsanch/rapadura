using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Rapadura.Audio;
using Rapadura.Core.Audio;
using Rapadura.Core.Events;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Crafting;
using Rapadura.Gameplay.Inventory;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.Items.Procedural;
using Rapadura.Gameplay.Tutorial;
using Rapadura.UI.Tutorial;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Rapadura.Tests
{
    /// <summary>
    /// Regression tests for the second review pass (audio/tutorial/inventory performance and
    /// correctness): StopMusic fading the audible track, the SfxEventMap binding dictionary matching
    /// the old linear scan, UiClickSoundBinder's Register API, the tutorial highlight cache, the
    /// throttled crafting probe and the inventory sort using the on-screen (composed, localized) name.
    /// Music baking off the main thread is covered in <see cref="AudioEventWiringTests"/>.
    /// </summary>
    public class ReviewPerformanceFixesTests
    {
        private static readonly string[] VolumeKeys = { "Audio.Volume.Master", "Audio.Volume.Music", "Audio.Volume.Sfx", "Audio.Volume.Ui" };

        private readonly List<Object> _created = new List<Object>();
        private AudioManager _audio;

        [SetUp]
        public void SetUp()
        {
            EventBus.Clear();
            foreach (string key in VolumeKeys) PlayerPrefs.DeleteKey(key);
        }

        [TearDown]
        public void TearDown()
        {
            _audio?.Shutdown();
            _audio = null;
            foreach (Object obj in _created)
            {
                if (obj != null) Object.DestroyImmediate(obj);
            }

            _created.Clear();
            foreach (string key in VolumeKeys) PlayerPrefs.DeleteKey(key);
            EventBus.Clear();
        }

        private T Track<T>(T obj) where T : Object
        {
            _created.Add(obj);
            return obj;
        }

        // ------------------------------------------------------------------
        // AudioManager.StopMusic
        // ------------------------------------------------------------------

        private AudioManager CreateAudio()
        {
            _audio = new AudioManager();
            _audio.Initialize();
            return _audio;
        }

        private AudioClip Clip(string name) => Track(AudioClip.Create(name, 64, 1, 22050, false));

        [Test]
        public void StopMusic_FadesTheActiveTrackOut_ThenStopsAndClearsIt()
        {
            AudioManager audio = CreateAudio();
            AudioClip track = Clip("track");

            audio.PlayMusic(track, 0.01f, 1f);
            audio.Tick(0.1f);
            AudioSource active = audio.ActiveMusicSourceForTests;
            Assert.AreSame(track, active.clip);
            Assert.AreEqual(1f, active.volume, 0.0001f);

            audio.StopMusic(1f);
            Assert.IsTrue(audio.IsMusicFadingOut);
            Assert.AreSame(track, active.clip, "the audible track must not be cut immediately");
            Assert.AreSame(active, audio.ActiveMusicSourceForTests, "StopMusic doesn't swap sources");
            Assert.IsNull(audio.InactiveMusicSourceForTests.clip, "the other source is cleared");

            audio.Tick(0.5f);
            Assert.AreEqual(0.5f, active.volume, 0.01f, "half way through the fade");
            Assert.AreSame(track, active.clip);

            audio.Tick(0.6f);
            Assert.IsFalse(audio.IsMusicFadingOut);
            Assert.IsNull(active.clip, "fade done: stopped and cleared");
            Assert.AreEqual(0f, active.volume);

            audio.Tick(0.1f);
            Assert.AreEqual(0f, active.volume, "volume upkeep must not revive a stopped source");
        }

        [Test]
        public void StopMusic_VolumeChangesDuringFade_DoNotBumpTheFadingTrack()
        {
            AudioManager audio = CreateAudio();
            audio.PlayMusic(Clip("track"), 0.01f, 1f);
            audio.Tick(0.1f);

            audio.StopMusic(1f);
            audio.Tick(0.5f);
            audio.SetCategoryVolume(AudioCategory.Music, 1f);

            Assert.AreEqual(0.5f, audio.ActiveMusicSourceForTests.volume, 0.01f);
        }

        [Test]
        public void StopMusic_DuringCrossfade_ClearsOutgoingAndFadesIncoming()
        {
            AudioManager audio = CreateAudio();
            AudioClip first = Clip("first");
            AudioClip second = Clip("second");

            audio.PlayMusic(first, 0.01f, 1f);
            audio.Tick(0.1f);
            audio.PlayMusic(second, 2f, 1f);
            audio.Tick(1f);
            Assert.IsTrue(audio.IsMusicCrossfading);

            audio.StopMusic(1f);
            Assert.IsFalse(audio.IsMusicCrossfading, "crossfade cancelled");
            Assert.IsTrue(audio.IsMusicFadingOut);
            Assert.AreSame(second, audio.ActiveMusicSourceForTests.clip, "the newer track is the one fading out");
            Assert.IsNull(audio.InactiveMusicSourceForTests.clip, "the older one is cleared");

            audio.Tick(1.1f);
            Assert.IsNull(audio.ActiveMusicSourceForTests.clip);
            Assert.IsNull(audio.InactiveMusicSourceForTests.clip);
        }

        [Test]
        public void PlayMusic_AfterStopMusic_CancelsTheFadeOutAndCrossfadesIn()
        {
            AudioManager audio = CreateAudio();
            AudioClip first = Clip("first");
            AudioClip second = Clip("second");

            audio.PlayMusic(first, 0.01f, 1f);
            audio.Tick(0.1f);
            audio.StopMusic(1f);
            audio.Tick(0.25f);

            audio.PlayMusic(second, 0.5f, 1f);
            Assert.IsFalse(audio.IsMusicFadingOut);
            Assert.IsTrue(audio.IsMusicCrossfading);

            audio.Tick(0.6f);
            Assert.AreSame(second, audio.ActiveMusicSourceForTests.clip);
            Assert.AreEqual(1f, audio.ActiveMusicSourceForTests.volume, 0.0001f);
        }

        [Test]
        public void StopMusic_WithNothingPlaying_IsANoOp()
        {
            AudioManager audio = CreateAudio();
            Assert.DoesNotThrow(() => audio.StopMusic(1f));
            Assert.IsFalse(audio.IsMusicFadingOut);
        }

        // ------------------------------------------------------------------
        // SfxEventMap binding dictionary
        // ------------------------------------------------------------------

        /// <summary>The pre-dictionary TryResolve, kept verbatim as the oracle.</summary>
        private static bool LinearResolve(SfxEventMap map, SfxTrigger trigger, string variant, out string cueId)
        {
            string key = SfxEventMap.Normalize(variant);
            foreach (SfxEventMap.Binding binding in map.Bindings)
            {
                if (binding != null && binding.trigger == trigger && SfxEventMap.Normalize(binding.variant) == key)
                {
                    cueId = binding.cueId ?? string.Empty;
                    return true;
                }
            }

            cueId = null;
            return false;
        }

        private static void AssertDictionaryMatchesLinear(SfxEventMap map)
        {
            Assert.Greater(map.Bindings.Count, 0);
            var variants = new HashSet<string>(StringComparer.Ordinal) { null, string.Empty, "nonexistent", "  ", "FOREST" };
            foreach (SfxEventMap.Binding binding in map.Bindings)
            {
                if (binding == null) continue;
                string v = binding.variant ?? string.Empty;
                variants.Add(v);
                variants.Add(v.ToUpperInvariant());
                variants.Add("  " + v + " ");
            }

            int checkedCount = 0;
            foreach (SfxTrigger trigger in (SfxTrigger[])Enum.GetValues(typeof(SfxTrigger)))
            {
                foreach (string variant in variants)
                {
                    bool expected = LinearResolve(map, trigger, variant, out string expectedCue);
                    bool actual = map.TryResolve(trigger, variant, out string actualCue);
                    Assert.AreEqual(expected, actual, $"{trigger}/'{variant}' found");
                    Assert.AreEqual(expectedCue, actualCue, $"{trigger}/'{variant}' cue");
                    checkedCount++;
                }
            }

            Assert.Greater(checkedCount, map.Bindings.Count);
        }

        [Test]
        public void SfxEventMap_TryResolve_MatchesLinearScan_ForShippedAsset()
        {
            var map = Resources.Load<SfxEventMap>(SfxEventMap.ResourcePath);
            Assert.IsNotNull(map, "Resources/Audio/SfxEventMap.asset missing");
            map.InvalidateLookups();
            AssertDictionaryMatchesLinear(map);
        }

        [Test]
        public void SfxEventMap_TryResolve_MatchesLinearScan_ForCodeDefaults()
        {
            AssertDictionaryMatchesLinear(Track(SfxEventMap.CreateDefault()));
        }

        [Test]
        public void SfxEventMap_TryResolve_FirstBindingWins_AndAddBindingInvalidates()
        {
            SfxEventMap map = Track(ScriptableObject.CreateInstance<SfxEventMap>());
            map.AddBinding(SfxTrigger.Hit, "fire", "first");
            map.AddBinding(SfxTrigger.Hit, "FIRE ", "second");

            Assert.IsTrue(map.TryResolve(SfxTrigger.Hit, "fire", out string cue));
            Assert.AreEqual("first", cue, "duplicate (trigger, variant): first binding wins like the linear scan");
            Assert.IsFalse(map.TryResolve(SfxTrigger.Hit, "ice", out _));

            map.AddBinding(SfxTrigger.Hit, "ice", "ice_cue");
            Assert.IsTrue(map.TryResolve(SfxTrigger.Hit, " Ice", out cue), "lookup rebuilt after AddBinding");
            Assert.AreEqual("ice_cue", cue);
        }

        // ------------------------------------------------------------------
        // UiClickSoundBinder.Register
        // ------------------------------------------------------------------

        [Test]
        public void UiClickSoundBinder_RegisterUnregister_TracksDocuments()
        {
            var go = Track(new GameObject("BinderDoc"));
            UIDocument document = go.AddComponent<UIDocument>();
            int before = UiClickSoundBinder.RegisteredCount;

            Assert.DoesNotThrow(() => UiClickSoundBinder.Register(null));
            Assert.DoesNotThrow(() => UiClickSoundBinder.Unregister(null));
            Assert.IsFalse(UiClickSoundBinder.IsRegistered(document));

            UiClickSoundBinder.Register(document);
            UiClickSoundBinder.Register(document);
            Assert.IsTrue(UiClickSoundBinder.IsRegistered(document));
            Assert.AreEqual(before + 1, UiClickSoundBinder.RegisteredCount, "registering twice counts once");

            UiClickSoundBinder.Unregister(document);
            Assert.IsFalse(UiClickSoundBinder.IsRegistered(document));
            Assert.AreEqual(before, UiClickSoundBinder.RegisteredCount);

            UiClickSoundBinder.Register(document);
            Object.DestroyImmediate(go);
            Assert.AreEqual(before, UiClickSoundBinder.RegisteredCount, "destroyed documents are dropped");
        }

        [Test]
        public void UiClickSoundBinder_RescanSchedule_IsFiniteAndStartsImmediately()
        {
            Assert.That(UiClickSoundBinder.RescanDelays, Is.Not.Empty);
            Assert.AreEqual(0f, UiClickSoundBinder.RescanDelays[0]);
            Assert.That(UiClickSoundBinder.RescanDelays.Length, Is.LessThanOrEqualTo(4), "a few delayed rescans, not a poll");
        }

        // ------------------------------------------------------------------
        // Tutorial highlight cache
        // ------------------------------------------------------------------

        [Test]
        public void TutorialHighlightCache_LooksUpHudOncePerScene()
        {
            var cache = new TutorialHighlightCache();
            Assert.IsTrue(cache.NeedsHudLookup);

            cache.MarkHudLookedUp();
            Assert.IsFalse(cache.NeedsHudLookup, "no repeat lookup every frame (WorldMap has no HudController)");

            cache.InvalidateScene();
            Assert.IsTrue(cache.NeedsHudLookup, "sceneLoaded allows a new lookup");

            cache.MarkHudLookedUp();
            cache.RequestLookup();
            Assert.IsTrue(cache.NeedsHudLookup, "a new hint allows one more lookup");
        }

        [Test]
        public void TutorialHighlightCache_RewritesStylesOnlyWhenBoundsChange()
        {
            var cache = new TutorialHighlightCache();
            var rect = new Rect(10f, 20f, 100f, 40f);

            Assert.IsTrue(cache.TryApply(rect));
            Assert.IsFalse(cache.TryApply(rect), "same worldBound: nothing to rewrite");
            Assert.IsTrue(cache.TryApply(new Rect(11f, 20f, 100f, 40f)), "moved");
            Assert.IsTrue(cache.IsVisible);

            Assert.IsTrue(cache.TryHide());
            Assert.IsFalse(cache.TryHide(), "already hidden");
            Assert.IsTrue(cache.TryApply(rect), "shown again after hiding");

            cache.InvalidateScene();
            Assert.IsTrue(cache.TryApply(rect), "new scene: styles are written again");
        }

        [Test]
        public void TutorialHighlightCache_MapsHudNamesToWorldMapHud()
        {
            Assert.AreEqual("wm-quests-button", TutorialHighlightCache.WorldMapElementName("quests-button"));
            Assert.AreEqual("wm-inventory-button", TutorialHighlightCache.WorldMapElementName("inventory-button"));
            Assert.AreEqual("wm-quests-button", TutorialHighlightCache.WorldMapElementName("wm-quests-button"));
            Assert.IsNull(TutorialHighlightCache.WorldMapElementName(null));
            Assert.IsNull(TutorialHighlightCache.WorldMapElementName(string.Empty));
        }

        // ------------------------------------------------------------------
        // TutorialManager crafting probe throttle
        // ------------------------------------------------------------------

        [Test]
        public void TutorialManager_CraftableProbe_ThrottledOnInventoryChanges_ForcedOnRecipeUnlock()
        {
            float now = 50f;
            int probes = 0;
            var manager = new TutorialManager(TutorialSteps.All)
            {
                CraftableRecipeProbe = () => { probes++; return false; },
                Clock = () => now,
            };
            manager.Initialize();

            try
            {
                for (int i = 0; i < 5; i++)
                {
                    EventBus.Publish(new InventoryChangedEvent());
                }

                Assert.AreEqual(1, probes, "a burst of inventory changes probes once");

                now += 0.5f;
                EventBus.Publish(new InventoryChangedEvent());
                Assert.AreEqual(1, probes, "still inside the interval");

                now += 0.6f;
                EventBus.Publish(new InventoryChangedEvent());
                Assert.AreEqual(2, probes, "interval elapsed");

                EventBus.Publish(new RecipeUnlockedEvent(null));
                Assert.AreEqual(3, probes, "a new recipe always probes");

                manager.CraftableRecipeProbe = () => { probes++; return true; };
                now += 1.1f;
                EventBus.Publish(new InventoryChangedEvent());
                Assert.IsTrue(manager.IsPending(TutorialSteps.Crafting));
            }
            finally
            {
                manager.Shutdown();
            }
        }

        // ------------------------------------------------------------------
        // Inventory sort by on-screen name
        // ------------------------------------------------------------------

        [Test]
        public void BuildSortedSlots_SortsGeneratedItemsByComposedLocalizedName_InEnglish()
        {
            var sword = Track(ScriptableObject.CreateInstance<ItemDefinition>());
            SetPrivateField(sword, "_itemId", "item_iron_sword");
            SetPrivateField(sword, "_displayName", "Espada de Ferro");
            SetPrivateField(sword, "_equipSlot", EquipmentSlot.MainHand);

            var affixes = new Dictionary<string, ItemAffixDefinition>
            {
                { "affix_sharp", CreateAffix("affix_sharp", "Afiada", AffixSlotKind.Prefix) },
                { "affix_of_the_wind", CreateAffix("affix_of_the_wind", "do Vento", AffixSlotKind.Suffix) },
            };
            Func<string, ItemAffixDefinition> resolveAffix = id => affixes.TryGetValue(id, out ItemAffixDefinition a) ? a : null;
            Func<string, ItemDefinition> resolveItem = id => id == "item_iron_sword" ? sword : null;

            // Stored (pt-BR) names sort Sharp first ("Afiada" < "do"); English on screen is the reverse.
            InventorySlotData sharp = GeneratedSword("Espada de Ferro Afiada", "affix_sharp");
            InventorySlotData wind = GeneratedSword("Espada de Ferro do Vento", "affix_of_the_wind");

            var localization = new LocalizationManager();
            localization.Initialize();

            localization.SetLanguage(LanguageCode.en);
            Assert.AreEqual("Sharp Iron Sword", GeneratedItemName.Compose(sword, sharp, localization, resolveAffix));
            Assert.AreEqual("Iron Sword of the Wind", GeneratedItemName.Compose(sword, wind, localization, resolveAffix));

            InventorySlotData[] english = InventoryManager.BuildSortedSlots(new[] { sharp, wind }, resolveItem, localization, resolveAffix);
            Assert.AreSame(wind, english[0], "\"Iron Sword of the Wind\" < \"Sharp Iron Sword\"");
            Assert.AreSame(sharp, english[1]);

            localization.SetLanguage(LanguageCode.ptBR);
            InventorySlotData[] portuguese = InventoryManager.BuildSortedSlots(new[] { wind, sharp }, resolveItem, localization, resolveAffix);
            Assert.AreSame(sharp, portuguese[0], "\"Espada de Ferro Afiada\" < \"Espada de Ferro do Vento\"");
            Assert.AreSame(wind, portuguese[1]);
        }

        private static InventorySlotData GeneratedSword(string storedName, string affixId)
        {
            return new InventorySlotData
            {
                itemId = "item_iron_sword",
                quantity = 1,
                generatedAffixIds = new[] { affixId },
                generatedAffixValues = new[] { 1f },
                generatedName = storedName,
                generatedRarity = ItemRarity.Rare,
            };
        }

        private ItemAffixDefinition CreateAffix(string affixId, string ptBRFallback, AffixSlotKind slotKind)
        {
            var affix = Track(ScriptableObject.CreateInstance<ItemAffixDefinition>());
            SetPrivateField(affix, "_affixId", affixId);
            SetPrivateField(affix, "_nameLocalizationKey", "affix." + affixId);
            SetPrivateField(affix, "_fallbackDisplayText", ptBRFallback);
            SetPrivateField(affix, "_slotKind", slotKind);
            SetPrivateField(affix, "_requiredEquipSlot", EquipmentSlot.None);
            return affix;
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(field, $"expected private field '{fieldName}' on {target.GetType().Name}");
            field.SetValue(target, value);
        }
    }
}
