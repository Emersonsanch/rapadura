using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Rapadura.Audio;
using Rapadura.Core.Audio;
using Rapadura.Core.Events;
using Rapadura.Core.Systems;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Combat;
using Rapadura.Gameplay.Dialogue;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Flow;
using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.Story;
using Rapadura.Gameplay.Skills;
using Rapadura.UI.Story;
using Rapadura.UI.WorldMap;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Rapadura.Tests
{
    /// <summary>
    /// The cues added for stun, loot / bag full, story panels, campaign completion, village hub, NPC talk,
    /// Espinhos, enemy windup, hero landing and the title-screen bed (Docs/Audio.md §2 "Novos cues"), the
    /// pitch/volume jitter, the loot spam cooldown and the story music overlay. Clock injected, no frames.
    /// </summary>
    public class AudioNewCuesTests
    {
        private static readonly string[] VolumeKeys = { "Audio.Volume.Master", "Audio.Volume.Music", "Audio.Volume.Sfx", "Audio.Volume.Ui" };

        private static readonly (SfxTrigger trigger, string cueId)[] NewCues =
        {
            (SfxTrigger.Stun, "combat.stun"),
            (SfxTrigger.LootDrop, "loot.drop"),
            (SfxTrigger.BagFull, "loot.bag_full"),
            (SfxTrigger.StoryOpen, "story.open"),
            (SfxTrigger.StoryPage, "story.page"),
            (SfxTrigger.StorySkip, "story.skip"),
            (SfxTrigger.StoryClose, "story.close"),
            (SfxTrigger.StoryToCredits, "story.credits"),
            (SfxTrigger.CampaignCompleted, "campaign.completed"),
            (SfxTrigger.VillageOpen, "village.open"),
            (SfxTrigger.VillageClose, "village.close"),
            (SfxTrigger.NpcTalk, "npc.talk"),
            (SfxTrigger.Thorns, "combat.thorns"),
            (SfxTrigger.EnemyWindup, "enemy.windup"),
            (SfxTrigger.HeroLanding, "hero.land"),
            (SfxTrigger.TitleAmbience, "ambience.title"),
        };

        private readonly List<GameObject> _objects = new List<GameObject>();
        private readonly List<Object> _assets = new List<Object>();
        private readonly List<SfxEventRouter> _routers = new List<SfxEventRouter>();
        private AudioManager _audio;
        private MusicDirector _music;
        private float _now;
        private int _inventory;

        [SetUp]
        public void SetUp()
        {
            EventBus.Clear();
            GameplayPause.Reset();
            foreach (string key in VolumeKeys) PlayerPrefs.DeleteKey(key);
            _now = 100f;
            _inventory = 0;
        }

        [TearDown]
        public void TearDown()
        {
            foreach (SfxEventRouter router in _routers) router.Shutdown();
            _routers.Clear();
            _music?.Shutdown();
            _music = null;
            _audio?.Shutdown();
            _audio = null;
            GameplayPause.Reset();
            Time.timeScale = 1f;
            SetThornsReflecting(false);
            foreach (GameObject go in _objects) if (go != null) Object.DestroyImmediate(go);
            foreach (Object asset in _assets) if (asset != null) Object.DestroyImmediate(asset);
            _objects.Clear();
            _assets.Clear();
            foreach (string key in VolumeKeys) PlayerPrefs.DeleteKey(key);
            EventBus.Clear();
        }

        private SfxEventRouter CreateRouter(SfxEventMap map = null)
        {
            if (_audio == null)
            {
                _audio = new AudioManager();
                _audio.Initialize();
            }

            var router = new SfxEventRouter(_audio, map ?? Track(SfxEventMap.CreateDefault()), () => _now, () => false, () => _inventory);
            router.Initialize();
            _routers.Add(router);
            return router;
        }

        private T Track<T>(T asset) where T : Object
        {
            _assets.Add(asset);
            return asset;
        }

        private GameObject Make(string name, bool player = false)
        {
            var go = new GameObject(name);
            if (player) go.tag = "Player";
            _objects.Add(go);
            return go;
        }

        private static void SetThornsReflecting(bool value)
        {
            FieldInfo field = typeof(ThornsRules).GetField("_reflecting", BindingFlags.Static | BindingFlags.NonPublic);
            Assert.IsNotNull(field, "ThornsRules._reflecting");
            field.SetValue(null, value);
        }

        /// <summary>Publishes, then asserts exactly one cue played and which one. Moves the clock past every cooldown first.</summary>
        private void AssertPlaysOnce(SfxEventRouter router, string expectedCue, Action publish)
        {
            _now += 10f;
            int before = router.PlayedCount;
            publish();
            Assert.AreEqual(before + 1, router.PlayedCount, $"{expectedCue}: exactly one cue");
            Assert.AreEqual(expectedCue, router.LastPlayedCueId);
        }

        // ------------------------------------------------------------------
        // Data
        // ------------------------------------------------------------------

        [Test]
        public void NewCues_AreBoundAndExist_InDefaultAndShippedMap()
        {
            var shipped = Resources.Load<SfxEventMap>(SfxEventMap.ResourcePath);
            Assert.IsNotNull(shipped, "Resources/Audio/SfxEventMap.asset");

            foreach (SfxEventMap map in new[] { Track(SfxEventMap.CreateDefault()), shipped })
            {
                foreach (var (trigger, cueId) in NewCues)
                {
                    Assert.IsTrue(map.TryResolve(trigger, string.Empty, out string bound), $"{map.name}: no binding for {trigger}");
                    Assert.AreEqual(cueId, bound, $"{map.name}: {trigger}");
                    Assert.IsTrue(map.TryGetCue(cueId, out SfxEventMap.Cue cue), $"{map.name}: cue {cueId}");
                    Assert.AreNotEqual(ProceduralSfxPreset.None, cue.preset, $"{cueId} has a placeholder");
                    Assert.LessOrEqual(cue.volume, 0.75f, $"{cueId} mixed under the existing cues");
                }

                Assert.IsTrue(map.TryGetCue("ambience.title", out SfxEventMap.Cue title));
                Assert.IsTrue(title.loop && !title.pausable, "title bed loops and plays over menus");
                Assert.IsTrue(map.TryGetCue("loot.drop", out SfxEventMap.Cue loot));
                Assert.GreaterOrEqual(loot.cooldown, 0.25f, "loot spam cooldown");
                Assert.AreEqual(1, loot.maxVoices);

                Assert.IsTrue(map.TryGetMusic(MusicContext.Story, string.Empty, out SfxEventMap.MusicEntry story), $"{map.name}: story music");
                Assert.IsTrue(map.TryGetMusic(MusicContext.Story, MusicDirector.EndingVariant, out SfxEventMap.MusicEntry ending));
                Assert.AreEqual(MusicDirector.EndingVariant, ending.variant, "epilogue has its own track");
                Assert.AreNotSame(story, ending);
            }

            // Shipped asset and the in-code default agree on every new cue.
            SfxEventMap defaults = Track(SfxEventMap.CreateDefault());
            foreach (var (_, cueId) in NewCues)
            {
                defaults.TryGetCue(cueId, out SfxEventMap.Cue a);
                shipped.TryGetCue(cueId, out SfxEventMap.Cue b);
                Assert.AreEqual(a.preset, b.preset, cueId);
                Assert.AreEqual(a.category, b.category, cueId);
                Assert.AreEqual(a.volume, b.volume, 1e-4f, cueId);
                Assert.AreEqual(a.cooldown, b.cooldown, 1e-4f, cueId);
                Assert.AreEqual(a.pitchJitter, b.pitchJitter, 1e-4f, cueId);
            }
        }

        [Test]
        public void NewPresets_AreShortDeterministic_AndOnlyTheTitleBedLoops()
        {
            ProceduralSfxPreset[] presets =
            {
                ProceduralSfxPreset.Dizzy, ProceduralSfxPreset.Clink, ProceduralSfxPreset.WoodKnock, ProceduralSfxPreset.WoodCreak,
                ProceduralSfxPreset.PageRustle, ProceduralSfxPreset.ForroFlourish, ProceduralSfxPreset.Murmur, ProceduralSfxPreset.Prick,
                ProceduralSfxPreset.Inhale, ProceduralSfxPreset.SackThump, ProceduralSfxPreset.CountrysideLoop,
            };

            foreach (ProceduralSfxPreset preset in presets)
            {
                float[] a = ProceduralSfxSynth.Generate(preset, 600f);
                float[] b = ProceduralSfxSynth.Generate(preset, 600f);
                CollectionAssert.AreEqual(a, b, $"{preset} deterministic");
                Assert.IsTrue(a.All(v => !float.IsNaN(v) && Mathf.Abs(v) <= 1f), $"{preset} bounded");

                bool loop = preset == ProceduralSfxPreset.CountrysideLoop;
                Assert.AreEqual(loop, ProceduralSfxSynth.IsLoopPreset(preset), preset.ToString());
                float seconds = (float)a.Length / ProceduralSfxSynth.SampleRate;
                Assert.LessOrEqual(seconds, loop ? 4f : preset == ProceduralSfxPreset.ForroFlourish ? 2.1f : 0.5f, $"{preset} short");
            }

            CollectionAssert.AreNotEqual(ProceduralSfxSynth.Generate(ProceduralSfxPreset.PageRustle, 2400f), ProceduralSfxSynth.Generate(ProceduralSfxPreset.PageRustle, 3000f),
                "story open and page turns are different rustles");
        }

        // ------------------------------------------------------------------
        // Event → cue, once
        // ------------------------------------------------------------------

        [Test]
        public void NewEvents_EachPlaysItsCueOnce()
        {
            SfxEventRouter router = CreateRouter();
            GameObject enemy = Make("Enemy");
            GameObject player = Make("Player", player: true);

            AssertPlaysOnce(router, "combat.stun", () => EventBus.Publish(new StunAppliedEvent(enemy, 1.5f)));
            AssertPlaysOnce(router, "loot.bag_full", () => EventBus.Publish(new LootOverflowEvent(Vector3.zero, null)));
            AssertPlaysOnce(router, "npc.talk", () => EventBus.Publish(new DialogueStartedEvent("npc_floresta", "start")));
            AssertPlaysOnce(router, "enemy.windup", () => EventBus.Publish(new EnemyAttackWindupEvent(enemy, Vector3.one, 0.6f)));
            AssertPlaysOnce(router, "hero.land", () => EventBus.Publish(new PlayerLandedEvent(player, Vector3.zero, 0.8f)));

            SetThornsReflecting(true);
            AssertPlaysOnce(router, "combat.thorns", () => EventBus.Publish(new DamageAppliedEvent(enemy, player, 4f, ElementType.Physical, false)));
            SetThornsReflecting(false);
            AssertPlaysOnce(router, "combat.hit", () => EventBus.Publish(new DamageAppliedEvent(enemy, player, 4f, ElementType.Physical, false)));
        }

        [Test]
        public void StoryPanels_OpenPageSkipCloseAndCredits()
        {
            SfxEventRouter router = CreateRouter();
            StoryPanelSequence sequence = null;

            AssertPlaysOnce(router, "story.open", () => sequence = new StoryPanelSequence(StoryBeat.InterludeForest, CharacterId.Lavine));
            Assert.Greater(sequence.Count, 1, "interlude has several panels");
            AssertPlaysOnce(router, "story.page", () => sequence.Advance());
            AssertPlaysOnce(router, "story.skip", () => sequence.Skip());

            _now += 10f;
            var read = new StoryPanelSequence(StoryBeat.InterludeForest, CharacterId.Maithe);
            while (!read.IsLastPanel)
            {
                _now += 10f;
                read.Advance();
            }

            AssertPlaysOnce(router, "story.close", () => read.Advance());

            _now += 10f;
            var ending = new StoryPanelSequence(StoryBeat.Ending, CharacterId.Joaquim);
            AssertPlaysOnce(router, "story.credits", () => ending.Skip());
        }

        [Test]
        public void CampaignCompleted_PlaysOnce_AfterTheVictorySting()
        {
            SfxEventRouter router = CreateRouter();

            EventBus.Publish(new CampaignCompletedEvent());
            Assert.AreEqual(0, router.PlayedCount, "waits for the victory fanfare of the same frame");
            Assert.AreEqual(1, router.ScheduledCount);

            _now += SfxEventRouter.CampaignStingDelaySeconds * 0.5f;
            router.Tick(0.1f);
            Assert.AreEqual(0, router.PlayedCount);

            _now += SfxEventRouter.CampaignStingDelaySeconds;
            router.Tick(0.1f);
            Assert.AreEqual(1, router.PlayedCount);
            Assert.AreEqual("campaign.completed", router.LastPlayedCueId);

            _now += 30f;
            router.Tick(0.1f);
            Assert.AreEqual(1, router.PlayedCount, "played once");
            Assert.AreEqual(0, router.ScheduledCount);
        }

        [Test]
        public void VillageHubPause_PlaysChestSounds_InsteadOfGenericUi()
        {
            SfxEventRouter router = CreateRouter();
            VillageHubButton hub = Make("VillageHub").AddComponent<VillageHubButton>();

            _now += 10f;
            GameplayPause.Acquire(hub);
            Assert.AreEqual("village.open", router.LastPlayedCueId);
            Assert.AreEqual(1, router.PlayedCount);

            _now += 10f;
            GameplayPause.Release(hub);
            Assert.AreEqual("village.close", router.LastPlayedCueId);
            Assert.AreEqual(2, router.PlayedCount);

            // Any other screen keeps the generic sounds.
            var other = new object();
            _now += 10f;
            GameplayPause.Acquire(other);
            Assert.AreEqual("ui.open", router.LastPlayedCueId);
            _now += 10f;
            GameplayPause.Release(other);
            Assert.AreEqual("ui.close", router.LastPlayedCueId);
        }

        [Test]
        public void StoryPanelPause_IsSilent_ItsPageCuesSpeakInstead()
        {
            SfxEventRouter router = CreateRouter();
            StoryPanelController panel = Make("Story").AddComponent<StoryPanelController>();

            Assert.AreEqual(SfxEventRouter.PauseSource.Story, SfxEventRouter.ClassifyPauseOwners(new object[] { panel }));
            Assert.AreEqual(SfxEventRouter.PauseSource.Generic, SfxEventRouter.ClassifyPauseOwners(new object[] { new object() }));
            Assert.AreEqual(SfxEventRouter.PauseSource.Generic, SfxEventRouter.ClassifyPauseOwners(null));

            _now += 10f;
            GameplayPause.Acquire(panel);
            _now += 10f;
            GameplayPause.Release(panel);
            Assert.AreEqual(0, router.PlayedCount, "no ui.open / ui.close under the page rustle");
        }

        // ------------------------------------------------------------------
        // Loot: replaces the pickup, spam cooldown
        // ------------------------------------------------------------------

        private static IReadOnlyList<LootDrop> SomeDrops()
        {
            var wood = ScriptableObject.CreateInstance<Rapadura.Gameplay.Items.ItemDefinition>();
            return new[] { new LootDrop(wood, 2) };
        }

        [Test]
        public void LootDrop_ReplacesThePickupOfThatKill_AndCooldownStopsSpam()
        {
            SfxEventRouter router = CreateRouter();
            GameObject enemy = Make("Enemy");
            GameObject player = Make("Player", player: true);
            IReadOnlyList<LootDrop> drops = SomeDrops();
            Track((Object)drops[0].Item);

            EventBus.Publish(new InventoryChangedEvent()); // baseline
            _now += 10f;

            // Kill → items land in the bag → loot event, all in one dispatch.
            EventBus.Publish(new CombatTargetDiedEvent(enemy, player));
            _inventory += 2;
            EventBus.Publish(new InventoryChangedEvent());
            EventBus.Publish(new EnemyLootDroppedEvent(enemy, player, drops));
            Assert.AreEqual(1, router.PlayedCount);
            Assert.AreEqual("loot.drop", router.LastPlayedCueId);
            router.Tick(0.016f);
            Assert.AreEqual(1, router.PlayedCount, "no item.pickup on top of loot.drop");

            // An AoE drops five piles in the same instant: one clink.
            for (int i = 0; i < 5; i++) EventBus.Publish(new EnemyLootDroppedEvent(enemy, player, drops));
            Assert.AreEqual(1, router.PlayedCount, "cooldown swallows the burst");

            router.Map.TryGetCue("loot.drop", out SfxEventMap.Cue cue);
            _now += cue.cooldown + 0.01f;
            EventBus.Publish(new EnemyLootDroppedEvent(enemy, player, drops));
            Assert.AreEqual(2, router.PlayedCount, "plays again once the cooldown elapsed");

            // Empty loot is silent.
            _now += 10f;
            EventBus.Publish(new EnemyLootDroppedEvent(enemy, player, Array.Empty<LootDrop>()));
            Assert.AreEqual(2, router.PlayedCount);
        }

        [Test]
        public void KillWithoutLootEvent_FallsBackToPickup_OnTheNextTick()
        {
            SfxEventRouter router = CreateRouter();
            GameObject enemy = Make("Enemy");

            EventBus.Publish(new InventoryChangedEvent()); // baseline
            _now += 10f;
            EventBus.Publish(new CombatTargetDiedEvent(enemy, null));
            _inventory += 1;
            EventBus.Publish(new InventoryChangedEvent());
            Assert.AreEqual(0, router.PlayedCount);

            router.Tick(0.016f);
            Assert.AreEqual("item.pickup", router.LastPlayedCueId);
            Assert.AreEqual(1, router.PlayedCount);
            router.Tick(0.016f);
            Assert.AreEqual(1, router.PlayedCount);
        }

        // ------------------------------------------------------------------
        // Jitter, volume, title bed
        // ------------------------------------------------------------------

        [Test]
        public void PitchJitter_StaysInRange_VariesOnRepeats_AndIsDeterministic()
        {
            SfxEventRouter a = CreateRouter();
            SfxEventRouter b = CreateRouter();
            a.Map.TryGetCue("story.page", out SfxEventMap.Cue page);
            Assert.Greater(page.pitchJitter, 0f);

            var pitches = new List<float>();
            for (int i = 0; i < 8; i++)
            {
                _now += 1f;
                Assert.IsTrue(a.PlayCue("story.page"));
                Assert.IsTrue(b.PlayCue("story.page"));
                Assert.AreEqual(a.LastPlayedPitch, b.LastPlayedPitch, 1e-6f, "seeded jitter");
                Assert.AreEqual(a.LastPlayedVolume, b.LastPlayedVolume, 1e-6f);
                Assert.That(a.LastPlayedPitch, Is.InRange(1f - page.pitchJitter, 1f + page.pitchJitter));
                Assert.That(a.LastPlayedVolume, Is.InRange(page.volume * (1f - page.volumeJitter), page.volume));
                pitches.Add(a.LastPlayedPitch);
            }

            Assert.Greater(pitches.Distinct().Count(), 4, "repeats are not identical");

            _now += 1f;
            Assert.IsTrue(a.PlayCue("shop.purchase"));
            Assert.AreEqual(1f, a.LastPlayedPitch, "cues without jitter keep their pitch");
            AudioSource source = _audio.UiPoolForTests.Sources.First(s => s.clip != null && s.clip.name == "proc_shop.purchase");
            Assert.AreEqual(1f, source.pitch, 1e-6f, "a reused pooled source does not keep the previous jittered pitch");
        }

        [Test]
        public void NewCues_RespectTheirVolumeBus()
        {
            SfxEventRouter router = CreateRouter();

            _audio.SetCategoryVolume(AudioCategory.Ui, 0f);
            Assert.IsFalse(router.PlayCue("village.open"), "UI slider at 0 mutes the village chest");
            Assert.IsFalse(router.PlayCue("story.page"));
            Assert.IsTrue(router.PlayCue("combat.stun"), "SFX bus still on");

            _audio.SetCategoryVolume(AudioCategory.Sfx, 0f);
            _now += 10f;
            Assert.IsFalse(router.PlayCue("loot.drop"), "SFX slider at 0 mutes loot");
        }

        [Test]
        public void TitleAmbience_FadesInOnTitleScreens_AndOutElsewhere()
        {
            SfxEventRouter router = CreateRouter();
            Assert.IsNull(router.CurrentTitleAmbienceCueId);

            EventBus.Publish(new GameFlowStateChangedEvent(GameFlowState.Boot, GameFlowState.MainMenu));
            Assert.AreEqual("ambience.title", router.CurrentTitleAmbienceCueId);
            router.Tick(1f);
            Assert.That(router.TitleAmbienceFade, Is.InRange(0.01f, 0.99f), "fades in, no pop");
            router.Tick(5f);
            Assert.AreEqual(1f, router.TitleAmbienceFade, 1e-4f);

            EventBus.Publish(new GameFlowStateChangedEvent(GameFlowState.MainMenu, GameFlowState.CharacterSelect));
            Assert.AreEqual("ambience.title", router.CurrentTitleAmbienceCueId, "hero select keeps the bed");

            EventBus.Publish(new GameFlowStateChangedEvent(GameFlowState.CharacterSelect, GameFlowState.World));
            Assert.IsNull(router.CurrentTitleAmbienceCueId);
            router.Tick(5f);
            Assert.AreEqual(0f, router.TitleAmbienceFade, 1e-4f);
            Assert.AreEqual(0, router.PlayedCount, "the bed is not a one-shot");
        }

        // ------------------------------------------------------------------
        // Story music overlay
        // ------------------------------------------------------------------

        [Test]
        public void Music_StoryOverlay_TakesOver_AndRestoresWhatTheFlowAskedMeanwhile()
        {
            string overlay = null;
            _audio = new AudioManager();
            _audio.Initialize();
            _music = new MusicDirector(_audio, Track(SfxEventMap.CreateDefault()), () => overlay);
            _music.Initialize();

            EventBus.Publish(new GameFlowStateChangedEvent(GameFlowState.CharacterSelect, GameFlowState.World));
            Assert.AreEqual(MusicContext.WorldMap, _music.CurrentContext);

            _music.Tick();
            Assert.AreEqual(MusicContext.WorldMap, _music.CurrentContext, "no panel, no change");

            overlay = string.Empty;
            _music.Tick();
            Assert.AreEqual(MusicContext.Story, _music.CurrentContext);
            Assert.IsTrue(_music.IsStoryOverlayActive);
            Assert.IsTrue(_music.FlushPendingBakes(), "story loop bakes");
            Assert.IsNotNull(_music.CurrentClip);

            // The flow moves while the panel is up: the panel keeps the music, the new base is remembered.
            EventBus.Publish(new GameFlowStateChangedEvent(GameFlowState.World, GameFlowState.MainMenu));
            Assert.AreEqual(MusicContext.Story, _music.CurrentContext);
            Assert.AreEqual(MusicContext.Title, _music.BaseContext);

            overlay = MusicDirector.EndingVariant;
            _music.Tick();
            Assert.AreEqual(MusicContext.Story, _music.CurrentContext);
            Assert.AreEqual(MusicDirector.EndingVariant, _music.CurrentVariant, "epilogue track for the ending");

            overlay = null;
            _music.Tick();
            Assert.IsFalse(_music.IsStoryOverlayActive);
            Assert.AreEqual(MusicContext.Title, _music.CurrentContext, "back to what the flow asked for");
        }
    }
}
