using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Rapadura.Audio;
using Rapadura.Core.Audio;
using Rapadura.Core.Events;
using Rapadura.Gameplay.Combat;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Environment;
using Rapadura.Gameplay.Flow;
using Rapadura.Gameplay.Quests;
using Rapadura.Gameplay.Skills;
using Rapadura.Gameplay.World.Biomes;
using Rapadura.Gameplay.World.Map;
using Rapadura.UI.Menus;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Rapadura.Tests
{
    /// <summary>
    /// EditMode tests for the event-driven audio + haptics layer (Docs/Audio.md): EventBus event →
    /// cue mapping, cooldown / voice limits, volume and pause rules, procedural placeholders, music
    /// context switching and the "Vibração" toggle. Time is injected, so nothing depends on frames.
    /// </summary>
    public class AudioEventWiringTests
    {
        private readonly List<GameObject> _objects = new List<GameObject>();
        private readonly List<Object> _assets = new List<Object>();
        private AudioManager _audio;
        private SfxEventRouter _router;
        private MusicDirector _music;
        private HapticsService _haptics;
        private float _now;
        private bool _paused;
        private int _inventory;

        private static readonly string[] VolumeKeys = { "Audio.Volume.Master", "Audio.Volume.Music", "Audio.Volume.Sfx", "Audio.Volume.Ui" };

        [SetUp]
        public void SetUp()
        {
            EventBus.Clear();
            foreach (string key in VolumeKeys) PlayerPrefs.DeleteKey(key);
            PlayerPrefs.DeleteKey(HapticsService.EnabledKey);
            _now = 100f;
            _paused = false;
            _inventory = 0;
        }

        [TearDown]
        public void TearDown()
        {
            _router?.Shutdown();
            _music?.Shutdown();
            _haptics?.Shutdown();
            _audio?.Shutdown();
            _router = null;
            _music = null;
            _haptics = null;
            _audio = null;
            foreach (GameObject go in _objects) if (go != null) Object.DestroyImmediate(go);
            foreach (Object asset in _assets) if (asset != null) Object.DestroyImmediate(asset);
            _objects.Clear();
            _assets.Clear();
            foreach (string key in VolumeKeys) PlayerPrefs.DeleteKey(key);
            PlayerPrefs.DeleteKey(HapticsService.EnabledKey);
            EventBus.Clear();
        }

        private SfxEventRouter CreateRouter(SfxEventMap map = null)
        {
            _audio = new AudioManager();
            _audio.Initialize();
            _router = new SfxEventRouter(_audio, map ?? Track(SfxEventMap.CreateDefault()), () => _now, () => _paused, () => _inventory);
            _router.Initialize();
            return _router;
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

        private static void SetField(object target, string field, object value)
        {
            Type type = target.GetType();
            FieldInfo info = null;
            while (type != null && info == null)
            {
                info = type.GetField(field, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                type = type.BaseType;
            }

            Assert.IsNotNull(info, field);
            info.SetValue(target, value);
        }

        // ------------------------------------------------------------------
        // Map data
        // ------------------------------------------------------------------

        [Test]
        public void ShippedAsset_LoadsFromResources_AndCoversEveryTrigger()
        {
            var map = Resources.Load<SfxEventMap>(SfxEventMap.ResourcePath);
            Assert.IsNotNull(map, "Resources/Audio/SfxEventMap.asset missing or not deserializable");
            AssertMapIsComplete(map);
        }

        [Test]
        public void DefaultMap_CoversEveryTrigger_AndEveryBoundCueExists()
        {
            AssertMapIsComplete(Track(SfxEventMap.CreateDefault()));
        }

        private static void AssertMapIsComplete(SfxEventMap map)
        {
            foreach (SfxTrigger trigger in Enum.GetValues(typeof(SfxTrigger)))
            {
                Assert.IsTrue(map.Bindings.Any(b => b.trigger == trigger), $"no binding for {trigger}");
            }

            foreach (SfxEventMap.Binding binding in map.Bindings.Where(b => !string.IsNullOrEmpty(b.cueId)))
            {
                Assert.IsTrue(map.TryGetCue(binding.cueId, out _), $"binding {binding.trigger}/{binding.variant} → unknown cue '{binding.cueId}'");
            }

            Assert.AreEqual(map.Cues.Count, map.Cues.Select(c => c.id).Distinct().Count(), "duplicate cue ids");

            foreach (MusicContext context in new[] { MusicContext.Title, MusicContext.WorldMap, MusicContext.Encounter, MusicContext.Boss })
            {
                Assert.IsTrue(map.TryGetMusic(context, string.Empty, out _), $"no music for {context}");
            }

            foreach (BiomeType biome in Enum.GetValues(typeof(BiomeType)))
            {
                Assert.IsTrue(map.TryGetMusic(MusicContext.Encounter, biome.ToString(), out SfxEventMap.MusicEntry entry));
                Assert.AreEqual(SfxEventMap.Normalize(biome.ToString()), entry.variant, $"encounter music for {biome} falls back to generic");
            }
        }

        [Test]
        public void ResolveFirst_TriesVariantsInOrder_ThenFallback()
        {
            SfxEventMap map = Track(SfxEventMap.CreateDefault());

            Assert.AreEqual("skill.cast.fire", map.ResolveFirst(SfxTrigger.SkillCast, "fire", "heal"));
            Assert.AreEqual("skill.cast.heal", map.ResolveFirst(SfxTrigger.SkillCast, "none", "heal"));
            Assert.AreEqual("skill.cast", map.ResolveFirst(SfxTrigger.SkillCast, "unknown"));
            Assert.AreEqual(string.Empty, map.ResolveFirst(SfxTrigger.WeatherLoop, "clear"), "clear weather is silent on purpose");
        }

        // ------------------------------------------------------------------
        // Event → cue
        // ------------------------------------------------------------------

        [Test]
        public void DamageEvents_MapToHitCritAndPlayerHurt()
        {
            SfxEventRouter router = CreateRouter();
            GameObject enemy = Make("Enemy");
            GameObject player = Make("Player", player: true);

            EventBus.Publish(new DamageAppliedEvent(enemy, player, 10f, ElementType.Physical, false));
            Assert.AreEqual("combat.hit", router.LastPlayedCueId);

            EventBus.Publish(new DamageAppliedEvent(enemy, player, 25f, ElementType.Physical, true));
            Assert.AreEqual("combat.crit", router.LastPlayedCueId);

            EventBus.Publish(new DamageAppliedEvent(player, enemy, 5f, ElementType.Fire, false));
            Assert.AreEqual("combat.player_hurt", router.LastPlayedCueId);
        }

        [Test]
        public void ProgressionAndEncounterEvents_MapToTheirCues()
        {
            SfxEventRouter router = CreateRouter();

            EventBus.Publish(new EnemyDiedEvent(Make("Boss"), true));
            Assert.AreEqual("combat.boss_death", router.LastPlayedCueId);

            EventBus.Publish(new PlayerExperienceChangedEvent(0, 100, 3));
            Assert.AreEqual("combat.boss_death", router.LastPlayedCueId, "first level seen is only a baseline");
            EventBus.Publish(new PlayerExperienceChangedEvent(0, 120, 4));
            Assert.AreEqual("player.level_up", router.LastPlayedCueId);

            EventBus.Publish(new QuestStartedEvent("q1"));
            Assert.AreEqual("quest.started", router.LastPlayedCueId);
            EventBus.Publish(new QuestCompletedEvent("q1"));
            Assert.AreEqual("quest.completed", router.LastPlayedCueId);

            EventBus.Publish(new EncounterEndedEvent(null, EncounterOutcome.Defeat));
            Assert.AreEqual("encounter.defeat", router.LastPlayedCueId);

            EventBus.Publish(new TimeOfDayPhaseChangedEvent(TimeOfDayPhase.Dusk, TimeOfDayPhase.Night, 2));
            Assert.AreEqual("ambience.night", router.LastPlayedCueId);

            EventBus.Publish(new PlayerDiedEvent());
            Assert.AreEqual("player.death", router.LastPlayedCueId);
        }

        [Test]
        public void SkillCast_PrefersElement_ButUltimateCategoryWins()
        {
            SfxEventRouter router = CreateRouter();
            var fire = Track(ScriptableObject.CreateInstance<SkillDefinition>());
            SetField(fire, "_element", ElementType.Fire);

            EventBus.Publish(new SkillCastEvent(Make("Caster"), fire, Vector3.zero));
            Assert.AreEqual("skill.cast.fire", router.LastPlayedCueId);

            var ultimate = Track(ScriptableObject.CreateInstance<SkillDefinition>());
            SetField(ultimate, "_element", ElementType.Ice);
            SetField(ultimate, "_skillType", SkillType.Ultimate);
            EventBus.Publish(new SkillCastEvent(Make("Caster2"), ultimate, Vector3.zero));
            Assert.AreEqual("skill.cast.ultimate", router.LastPlayedCueId);
        }

        [Test]
        public void InventoryChanged_PlaysPickupOnlyWhenItemCountGrows()
        {
            SfxEventRouter router = CreateRouter();
            _inventory = 3;
            EventBus.Publish(new InventoryChangedEvent());
            Assert.AreEqual(0, router.PlayedCount, "first observation is a baseline");

            _inventory = 2;
            EventBus.Publish(new InventoryChangedEvent());
            Assert.AreEqual(0, router.PlayedCount, "removing items is not a pickup");

            _inventory = 5;
            EventBus.Publish(new InventoryChangedEvent());
            Assert.AreEqual("item.pickup", router.LastPlayedCueId);
        }

        [Test]
        public void WeatherChanged_StartsAndStopsAmbienceLoop()
        {
            SfxEventRouter router = CreateRouter();

            EventBus.Publish(new WeatherChangedEvent(WeatherType.Clear, WeatherType.Rain, BiomeType.Forest, 2f));
            Assert.AreEqual("ambience.rain", router.CurrentAmbienceCueId);
            router.Tick(5f);
            Assert.AreEqual(1f, router.AmbienceFade, 1e-4f);

            EventBus.Publish(new WeatherChangedEvent(WeatherType.Rain, WeatherType.Sandstorm, BiomeType.Desert, 2f));
            Assert.AreEqual("ambience.wind", router.CurrentAmbienceCueId);

            EventBus.Publish(new WeatherChangedEvent(WeatherType.Sandstorm, WeatherType.Clear, BiomeType.Desert, 2f));
            Assert.IsNull(router.CurrentAmbienceCueId);
            router.Tick(5f);
            Assert.AreEqual(0f, router.AmbienceFade, 1e-4f);
        }

        // ------------------------------------------------------------------
        // Limits, volume, pause
        // ------------------------------------------------------------------

        [Test]
        public void Cooldown_SuppressesRepeats_UntilItElapses()
        {
            SfxEventRouter router = CreateRouter();

            // shop.purchase: 0.1 s cooldown and 2 voices, so the voice cap doesn't mask the cooldown
            // (single-voice cues like quest.started are still ringing when their cooldown ends).
            Assert.IsTrue(router.PlayCue("shop.purchase"));
            Assert.IsFalse(router.PlayCue("shop.purchase"), "inside 0.1 s cooldown");
            _now += 0.11f;
            Assert.IsTrue(router.PlayCue("shop.purchase"));
        }

        [Test]
        public void VoiceLimits_PerCueAndGlobal()
        {
            SfxEventMap map = Track(ScriptableObject.CreateInstance<SfxEventMap>());
            map.Configure(maxVoices: 3, proceduralSfx: true, proceduralMusic: false);
            map.AddCue(new SfxEventMap.Cue { id = "a", cooldown = 0f, maxVoices = 2, preset = ProceduralSfxPreset.Chime });
            map.AddCue(new SfxEventMap.Cue { id = "b", cooldown = 0f, maxVoices = 5, preset = ProceduralSfxPreset.Chime });
            SfxEventRouter router = CreateRouter(map);

            Assert.IsTrue(router.PlayCue("a"));
            Assert.IsTrue(router.PlayCue("a"));
            Assert.IsFalse(router.PlayCue("a"), "per-cue cap of 2");
            Assert.IsTrue(router.PlayCue("b"));
            Assert.IsFalse(router.PlayCue("b"), "global cap of 3");
            Assert.AreEqual(3, router.ActiveVoiceCount);

            _now += 5f; // chime (~0.7 s) voices have ended
            Assert.AreEqual(0, router.ActiveVoiceCount);
            Assert.IsTrue(router.PlayCue("a"));
        }

        [Test]
        public void MutedCategory_SkipsCue_AndPlaysAtScaledVolume()
        {
            SfxEventRouter router = CreateRouter();

            _audio.SetCategoryVolume(AudioCategory.Sfx, 0f);
            Assert.IsFalse(router.PlayCue("combat.hit"), "SFX volume 0 → nothing played");

            _audio.SetCategoryVolume(AudioCategory.Sfx, 0.5f);
            _audio.SetCategoryVolume(AudioCategory.Master, 0.5f);
            Assert.IsTrue(router.PlayCue("combat.hit"));
            AudioSource source = _audio.SfxPoolForTests.Sources.Last();
            Assert.IsNotNull(source.clip, "procedural placeholder assigned");
            Assert.AreEqual(0.55f * 0.25f, source.volume, 1e-4f);

            _audio.SetCategoryVolume(AudioCategory.Master, 0f);
            Assert.IsFalse(router.PlayCue("ui.click"), "master 0 mutes UI too");
        }

        [Test]
        public void Pause_BlocksPausableCues_ButNotUiOrProgression()
        {
            SfxEventRouter router = CreateRouter();
            _paused = true;

            Assert.IsFalse(router.PlayCue("combat.hit"));
            Assert.IsTrue(router.PlayCue("ui.click"));
            Assert.IsTrue(router.PlayCue("quest.completed"));

            EventBus.Publish(new WeatherChangedEvent(WeatherType.Clear, WeatherType.Rain, BiomeType.Forest, 1f));
            router.Tick(0.1f);
            Assert.IsTrue(router.IsAmbiencePaused);
            _paused = false;
            router.Tick(0.1f);
            Assert.IsFalse(router.IsAmbiencePaused);
        }

        [Test]
        public void AssignedClip_WinsOverProceduralPlaceholder()
        {
            SfxEventMap map = Track(ScriptableObject.CreateInstance<SfxEventMap>());
            AudioClip real = Track(AudioClip.Create("real", 2205, 1, 22050, false));
            map.AddCue(new SfxEventMap.Cue { id = "x", clip = real, preset = ProceduralSfxPreset.Blip });
            map.AddCue(new SfxEventMap.Cue { id = "y", preset = ProceduralSfxPreset.None });
            SfxEventRouter router = CreateRouter(map);

            map.TryGetCue("x", out SfxEventMap.Cue x);
            map.TryGetCue("y", out SfxEventMap.Cue y);
            Assert.AreSame(real, router.ResolveClip(x));
            Assert.IsNull(router.ResolveClip(y));
            Assert.IsFalse(router.PlayCue("y"), "no clip and no preset → silent");
        }

        // ------------------------------------------------------------------
        // Procedural synth
        // ------------------------------------------------------------------

        [Test]
        public void ProceduralSynth_EveryPreset_IsShortBoundedAndDeterministic()
        {
            foreach (ProceduralSfxPreset preset in Enum.GetValues(typeof(ProceduralSfxPreset)))
            {
                float[] a = ProceduralSfxSynth.Generate(preset, 440f);
                if (preset == ProceduralSfxPreset.None)
                {
                    Assert.AreEqual(0, a.Length);
                    continue;
                }

                Assert.Greater(a.Length, 100, preset.ToString());
                Assert.LessOrEqual(a.Length, ProceduralSfxSynth.SampleRate * 4, $"{preset} too long");
                Assert.IsTrue(a.All(v => !float.IsNaN(v) && Mathf.Abs(v) <= 1f), $"{preset} out of range");
                Assert.Greater(a.Max(v => Mathf.Abs(v)), 0.1f, $"{preset} silent");
                CollectionAssert.AreEqual(a, ProceduralSfxSynth.Generate(preset, 440f), $"{preset} not deterministic");
            }
        }

        [Test]
        public void ProceduralMusicLoop_IsBoundedAndAbout16Beats()
        {
            float[] loop = ProceduralSfxSynth.GenerateMusicLoop(220f, minor: true, bpm: 120f);
            Assert.AreEqual(8f * ProceduralSfxSynth.SampleRate, loop.Length, 2f);
            Assert.IsTrue(loop.All(v => Mathf.Abs(v) <= 1f));
        }

        // ------------------------------------------------------------------
        // Music
        // ------------------------------------------------------------------

        [Test]
        public void Music_FollowsFlowStateAndEncounterBiome_AndBoss()
        {
            _audio = new AudioManager();
            _audio.Initialize();
            _music = new MusicDirector(_audio, Track(SfxEventMap.CreateDefault()));
            _music.Initialize();

            EventBus.Publish(new GameFlowStateChangedEvent(GameFlowState.Boot, GameFlowState.MainMenu));
            Assert.AreEqual(MusicContext.Title, _music.CurrentContext);
            Assert.IsTrue(_music.FlushPendingBakes(), "title loop bakes in the background");
            Assert.IsNotNull(_music.CurrentClip, "procedural title loop");

            EventBus.Publish(new GameFlowStateChangedEvent(GameFlowState.CharacterSelect, GameFlowState.World));
            Assert.AreEqual(MusicContext.WorldMap, _music.CurrentContext);

            EventBus.Publish(new EncounterStartedEvent(EncounterPayload.Create(BiomeType.Desert, null, 1)));
            EventBus.Publish(new GameFlowStateChangedEvent(GameFlowState.World, GameFlowState.Encounter));
            Assert.AreEqual(MusicContext.Encounter, _music.CurrentContext);
            Assert.AreEqual("desert", _music.CurrentVariant, "flow event must not override the biome track");

            var boss = Track(ScriptableObject.CreateInstance<EnemyDefinition>());
            SetField(boss, "_isBoss", true);
            EventBus.Publish(new EncounterStartedEvent(EncounterPayload.Create(BiomeType.Caves, new[] { boss }, 2)));
            Assert.AreEqual(MusicContext.Boss, _music.CurrentContext);

            Assert.IsFalse(_music.SetContext(MusicContext.Boss, "caves"), "same context is a no-op (no restart)");
        }

        [Test]
        public void Music_ProceduralLoop_BakesOffMainThread_PlaysWhenReady_AndCachesAtMostTwo()
        {
            _audio = new AudioManager();
            _audio.Initialize();
            _music = new MusicDirector(_audio, Track(SfxEventMap.CreateDefault()));
            _music.Initialize();

            Assert.IsTrue(_music.SetContext(MusicContext.Title));
            Assert.IsNull(_music.CurrentClip, "SetContext must not bake synchronously");
            Assert.IsTrue(_music.IsWaitingForBake);
            Assert.AreEqual(1, _music.PendingBakeCount);
            Assert.IsNull(_audio.ActiveMusicSourceForTests.clip, "nothing starts until the samples are ready");

            Assert.IsTrue(_music.FlushPendingBakes(), "bake completes");
            Assert.IsFalse(_music.IsWaitingForBake);
            Assert.IsNotNull(_music.CurrentClip);
            Assert.AreSame(_music.CurrentClip, _audio.ActiveMusicSourceForTests.clip, "the baked loop is handed to AudioManager");
            Assert.Greater(_music.CurrentClip.samples, 1000);

            var contexts = new (MusicContext context, string variant)[]
            {
                (MusicContext.WorldMap, null),
                (MusicContext.Encounter, "desert"),
                (MusicContext.Boss, "caves"),
                (MusicContext.Encounter, "forest"),
                (MusicContext.Title, null),
            };

            foreach (var (context, variant) in contexts)
            {
                AudioClip previous = _music.CurrentClip;
                Assert.IsTrue(_music.SetContext(context, variant));
                Assert.IsTrue(_music.FlushPendingBakes(), $"{context}/{variant} bakes");
                Assert.IsNotNull(_music.CurrentClip, $"{context}/{variant}");
                Assert.AreNotSame(previous, _music.CurrentClip);
                Assert.LessOrEqual(_music.CachedProceduralClipCount, MusicDirector.MaxCachedProceduralClips, "only current + previous are kept");
            }

            // Re-entering the previous context reuses its clip without a new bake.
            AudioClip title = _music.CurrentClip;
            _music.SetContext(MusicContext.Encounter, "forest");
            Assert.AreEqual(0, _music.PendingBakeCount, "previous track is still cached");
            Assert.IsNotNull(_music.CurrentClip);
            _music.SetContext(MusicContext.Title);
            Assert.AreSame(title, _music.CurrentClip);
        }

        [Test]
        public void Music_ProceduralOff_GivesSilence()
        {
            SfxEventMap map = Track(SfxEventMap.CreateDefault());
            map.Configure(maxVoices: 12, proceduralSfx: true, proceduralMusic: false);
            _music = new MusicDirector(null, map);
            _music.Initialize();

            Assert.IsTrue(_music.SetContext(MusicContext.WorldMap));
            Assert.IsNull(_music.CurrentClip);
        }

        // ------------------------------------------------------------------
        // Haptics
        // ------------------------------------------------------------------

        private sealed class RecordingBackend : IHapticsBackend
        {
            public readonly List<long[]> Played = new List<long[]>();
            public bool IsSupported => true;
            public void Play(long[] timingsMs) => Played.Add(timingsMs);
        }

        private HapticsService CreateHaptics(RecordingBackend backend)
        {
            _haptics = new HapticsService(backend, () => _now);
            _haptics.Initialize();
            return _haptics;
        }

        [Test]
        public void Haptics_CritDealtAndHitTaken_UsePatterns_WithMinInterval()
        {
            var backend = new RecordingBackend();
            HapticsService haptics = CreateHaptics(backend);
            GameObject player = Make("Player", player: true);
            GameObject enemy = Make("Enemy");

            EventBus.Publish(new DamageAppliedEvent(enemy, player, 10f, ElementType.Physical, false));
            Assert.AreEqual(0, backend.Played.Count, "normal hits dealt don't vibrate");

            EventBus.Publish(new DamageAppliedEvent(enemy, player, 30f, ElementType.Physical, true));
            Assert.AreEqual(HapticPattern.Light, haptics.LastPlayed);

            EventBus.Publish(new DamageAppliedEvent(player, enemy, 8f, ElementType.Physical, false));
            Assert.AreEqual(HapticPattern.Medium, haptics.LastPlayed);
            EventBus.Publish(new DamageAppliedEvent(player, enemy, 8f, ElementType.Physical, false));
            Assert.AreEqual(2, backend.Played.Count, "second hit inside the min interval is dropped");

            CollectionAssert.AreEqual(HapticsService.GetTimings(HapticPattern.Medium), backend.Played[1]);
        }

        [Test]
        public void Haptics_Toggle_DisablesAndPersists()
        {
            var backend = new RecordingBackend();
            HapticsService haptics = CreateHaptics(backend);
            Assert.IsTrue(haptics.Enabled, "on by default");

            haptics.SetEnabled(false);
            EventBus.Publish(new PlayerDiedEvent());
            Assert.AreEqual(0, backend.Played.Count);

            var reloaded = new HapticsService(backend, () => _now);
            reloaded.Initialize();
            Assert.IsFalse(reloaded.Enabled, "persisted in PlayerPrefs");
            reloaded.Shutdown();

            haptics.SetEnabled(true);
            EventBus.Publish(new PlayerExperienceChangedEvent(0, 10, 1));
            EventBus.Publish(new PlayerExperienceChangedEvent(0, 10, 2));
            Assert.AreEqual(HapticPattern.Success, haptics.LastPlayed);
        }

        [Test]
        public void HapticsSettingsSection_AppendsToggle_AndWritesService()
        {
            HapticsService haptics = CreateHaptics(new RecordingBackend());
            var panel = new VisualElement();
            panel.Add(new Toggle("existing"));
            var section = new HapticsSettingsSection { Service = haptics };

            section.Attach(panel);
            Assert.AreSame(section.Toggle, panel[panel.childCount - 1], "appended as the last row");
            Assert.IsTrue(section.Toggle.value);
            CollectionAssert.Contains(new[] { "Vibração", "Vibration" }, section.Toggle.label);

            haptics.SetEnabled(false);
            section.Refresh();
            Assert.IsFalse(section.Toggle.value, "row mirrors the service state");

            section.Attach(panel);
            Assert.AreEqual(2, panel.childCount, "attaching twice doesn't duplicate the row");
            section.Detach();
            Assert.AreEqual(1, panel.childCount);
        }
    }
}
