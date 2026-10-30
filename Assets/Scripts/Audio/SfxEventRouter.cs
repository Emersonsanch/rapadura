using System;
using System.Collections.Generic;
using Rapadura.Core.Audio;
using Rapadura.Core.Events;
using Rapadura.Core.Interfaces;
using Rapadura.Core.Logging;
using Rapadura.Core.Systems;
using Rapadura.Gameplay.Achievements;
using Rapadura.Gameplay.Combat;
using Rapadura.Gameplay.Crafting;
using Rapadura.Gameplay.Dialogue;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Environment;
using Rapadura.Gameplay.Flow;
using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.Quests;
using Rapadura.Gameplay.Shop;
using Rapadura.Gameplay.Skills;
using Rapadura.Gameplay.Skills.Vfx;
using Rapadura.Gameplay.Story;
using Rapadura.Gameplay.World.Map;
using Rapadura.UI.Dialogue;
using Rapadura.UI.Menus;
using Rapadura.UI.Story;
using Rapadura.UI.WorldMap;
using UnityEngine;

namespace Rapadura.Audio
{
    /// <summary>
    /// Event-driven SFX: listens to the EventBus (combat, skills, progression, quests, shop, UI pause,
    /// encounters, time of day, weather) and plays the cue <see cref="SfxEventMap"/> assigns to each
    /// happening — gameplay code never references audio. Playback goes through
    /// <see cref="AudioManager"/>'s pooled sources, so the Master/Music/SFX/UI volume settings apply
    /// unchanged. On top of that this class adds what the raw pool lacks:
    /// <list type="bullet">
    /// <item><description>per-cue cooldown (a 10-hit combo doesn't stack 10 identical thuds in one frame);</description></item>
    /// <item><description>per-cue and global voice caps (<see cref="SfxEventMap.MaxVoices"/>), skipping instead of cutting older sounds;</description></item>
    /// <item><description>pause: cues flagged <c>pausable</c> are dropped while <see cref="GameplayPause"/> is active and the weather loop pauses;</description></item>
    /// <item><description>a looping ambience bed (rain/wind) with fades, outside the one-shot pool so it is never stolen;</description></item>
    /// <item><description>procedural placeholder clips (<see cref="ProceduralSfxSynth"/>) for every cue without a real clip;</description></item>
    /// <item><description>per-cue pitch/volume jitter (<see cref="SfxEventMap.Cue.pitchJitter"/>), seeded so tests are stable;</description></item>
    /// <item><description>a second looping bed for the title screen (crickets + breeze under the title music);</description></item>
    /// <item><description>which screen took the <see cref="GameplayPause"/>: the village hub plays its wooden chest instead of the
    /// generic <c>ui.open</c>, story panels and dialogue boxes have their own cues and stay silent there.</description></item>
    /// </list>
    /// Time comes from an injectable clock (unscaled time by default, so UI sounds work while paused).
    /// </summary>
    public class SfxEventRouter : IManager
    {
        private const string LogCategory = "Audio";
        private const float AmbienceFadeSeconds = 2f;
        private const float PickupSuppressAfterTradeSeconds = 0.3f;
        private const float MinAudibleVolume = 0.001f;
        private const float TitleAmbienceFadeSeconds = 2.5f;

        /// <summary>Campaign sting waits for the victory fanfare of the same frame to ring out.</summary>
        public const float CampaignStingDelaySeconds = 1.6f;

        /// <summary>Who froze gameplay, as far as the UI sounds care (see <see cref="ClassifyPauseOwners"/>).</summary>
        public enum PauseSource
        {
            Generic = 0,
            /// <summary>Village hub (Vila): wooden chest open/close instead of ui.open/close.</summary>
            Village = 1,
            /// <summary>Story panels: their own page cues (<see cref="StoryPanelChangedEvent"/>), silent here.</summary>
            Story = 2,
            /// <summary>Dialogue box: <c>npc.talk</c> already played on DialogueStartedEvent, silent open.</summary>
            Dialogue = 3,
        }

        private readonly AudioManager _audio;
        private readonly Func<float> _clock;
        private readonly Func<bool> _isGameplayPaused;
        private readonly Func<int> _inventoryCount;
        private SfxEventMap _map;

        private readonly Dictionary<string, float> _lastPlayTime = new Dictionary<string, float>(StringComparer.Ordinal);
        private readonly Dictionary<string, AudioClip> _clipCache = new Dictionary<string, AudioClip>(StringComparer.Ordinal);
        private readonly List<KeyValuePair<string, float>> _voices = new List<KeyValuePair<string, float>>();
        private readonly HashSet<string> _warnedUnknown = new HashSet<string>(StringComparer.Ordinal);
        private readonly LevelUpDetector _levelUp = new LevelUpDetector();

        private GameObject _root;
        private AudioSource _ambienceSource;
        private SfxEventMap.Cue _ambienceCue;
        private float _ambienceFade;
        private float _ambienceFadeTarget;
        private bool _ambiencePaused;
        private bool _flowAllowsAmbience = true;
        private string _requestedAmbienceCueId;
        private int _lastInventoryCount = -1;
        private float _lastTradeTime = float.NegativeInfinity;
        private float _lastKillTime = float.NegativeInfinity;
        private bool _pickupPending;
        private PauseSource _pauseSource;
        private readonly List<object> _ownerBuffer = new List<object>();
        private readonly List<KeyValuePair<float, SfxTrigger>> _scheduled = new List<KeyValuePair<float, SfxTrigger>>();
        // Seeded: the same sequence of plays always gets the same jitter (tests, bug repros).
        private readonly System.Random _jitter = new System.Random(0x5EED);

        private AudioSource _titleSource;
        private SfxEventMap.Cue _titleCue;
        private float _titleFade;
        private bool _titleWanted;

        public bool IsInitialized { get; private set; }

        public SfxEventMap Map => _map;

        /// <summary>Last cue that actually played (null if none). Diagnostics/tests.</summary>
        public string LastPlayedCueId { get; private set; }

        public int PlayedCount { get; private set; }

        /// <summary>Cue id of the active (or fading-in) ambience loop, null when none.</summary>
        public string CurrentAmbienceCueId => _ambienceFadeTarget > 0f ? _requestedAmbienceCueId : null;

        public bool IsAmbiencePaused => _ambiencePaused;

        /// <summary>Pitch / volume (before category volumes) of the last played cue, after jitter. Tests.</summary>
        public float LastPlayedPitch { get; private set; } = 1f;

        public float LastPlayedVolume { get; private set; }

        /// <summary>Title-screen bed cue while it is wanted (flow on main menu / hero select), else null.</summary>
        public string CurrentTitleAmbienceCueId => _titleWanted && _titleCue != null ? _titleCue.id : null;

        /// <summary>Title bed gain before category volumes (0..1). Tests.</summary>
        public float TitleAmbienceFade => _titleFade;

        /// <summary>Delayed cues still waiting (campaign sting). Tests.</summary>
        public int ScheduledCount => _scheduled.Count;

        /// <param name="audio">Volumes + pooled playback. May be null (tests) — cues are then tracked but not heard.</param>
        /// <param name="map">Defaults to <see cref="SfxEventMap.LoadOrDefault"/>.</param>
        /// <param name="clock">Seconds; defaults to <see cref="Time.unscaledTime"/>.</param>
        /// <param name="isGameplayPaused">Defaults to <see cref="GameplayPause.IsPaused"/>.</param>
        /// <param name="inventoryCount">Total player item units; defaults to <see cref="AudioEventHelpers.CountPlayerItems"/>.</param>
        public SfxEventRouter(AudioManager audio, SfxEventMap map = null, Func<float> clock = null, Func<bool> isGameplayPaused = null, Func<int> inventoryCount = null)
        {
            _audio = audio;
            _map = map;
            _clock = clock ?? (() => Time.unscaledTime);
            _isGameplayPaused = isGameplayPaused ?? (() => GameplayPause.IsPaused);
            _inventoryCount = inventoryCount ?? AudioEventHelpers.CountPlayerItems;
        }

        public void Initialize()
        {
            if (IsInitialized)
            {
                return;
            }

            if (_map == null)
            {
                _map = SfxEventMap.LoadOrDefault();
            }

            _root = new GameObject("SfxEventRouter");
            if (Application.isPlaying)
            {
                UnityEngine.Object.DontDestroyOnLoad(_root);
                _root.AddComponent<SfxEventRouterDriver>().Bind(this);
                _root.AddComponent<UiClickSoundBinder>();
            }

            _ambienceSource = _root.AddComponent<AudioSource>();
            _ambienceSource.playOnAwake = false;
            _ambienceSource.loop = true;
            _ambienceSource.spatialBlend = 0f;
            _ambienceSource.volume = 0f;

            _titleSource = _root.AddComponent<AudioSource>();
            _titleSource.playOnAwake = false;
            _titleSource.loop = true;
            _titleSource.spatialBlend = 0f;
            _titleSource.volume = 0f;

            Subscribe(true);
            IsInitialized = true;
        }

        public void Shutdown()
        {
            if (!IsInitialized)
            {
                return;
            }

            Subscribe(false);

            if (_root != null)
            {
                if (Application.isPlaying) UnityEngine.Object.Destroy(_root);
                else UnityEngine.Object.DestroyImmediate(_root);
                _root = null;
            }

            foreach (AudioClip clip in _clipCache.Values)
            {
                if (clip != null)
                {
                    if (Application.isPlaying) UnityEngine.Object.Destroy(clip);
                    else UnityEngine.Object.DestroyImmediate(clip);
                }
            }

            _clipCache.Clear();
            _voices.Clear();
            _lastPlayTime.Clear();
            _scheduled.Clear();
            _titleSource = null;
            _titleCue = null;
            _titleFade = 0f;
            _titleWanted = false;
            _pickupPending = false;
            IsInitialized = false;
        }

        private void Subscribe(bool on)
        {
            Toggle<DamageAppliedEvent>(on, OnDamageApplied);
            Toggle<EnemyDiedEvent>(on, OnEnemyDied);
            Toggle<PlayerDiedEvent>(on, OnPlayerDied);
            Toggle<PlayerExperienceChangedEvent>(on, OnExperienceChanged);
            Toggle<GameLoadedEvent>(on, OnGameLoaded);
            Toggle<NewSessionResetEvent>(on, OnNewSession);
            Toggle<SkillCastEvent>(on, OnSkillCast);
            Toggle<InventoryChangedEvent>(on, OnInventoryChanged);
            Toggle<ItemPurchasedEvent>(on, OnItemPurchased);
            Toggle<ItemSoldEvent>(on, OnItemSold);
            Toggle<RecipeCraftedEvent>(on, OnRecipeCrafted);
            Toggle<QuestStartedEvent>(on, OnQuestStarted);
            Toggle<QuestCompletedEvent>(on, OnQuestCompleted);
            Toggle<AchievementUnlockedEvent>(on, OnAchievementUnlocked);
            Toggle<GamePausedEvent>(on, OnGamePaused);
            Toggle<GameResumedEvent>(on, OnGameResumed);
            Toggle<ShopOpenedEvent>(on, OnShopOpened);
            Toggle<ShopClosedEvent>(on, OnShopClosed);
            Toggle<EncounterStartedEvent>(on, OnEncounterStarted);
            Toggle<EncounterEndedEvent>(on, OnEncounterEnded);
            Toggle<TimeOfDayPhaseChangedEvent>(on, OnPhaseChanged);
            Toggle<WeatherChangedEvent>(on, OnWeatherChanged);
            Toggle<GameFlowStateChangedEvent>(on, OnFlowStateChanged);
            Toggle<StunAppliedEvent>(on, OnStunApplied);
            Toggle<CombatTargetDiedEvent>(on, OnCombatTargetDied);
            Toggle<EnemyLootDroppedEvent>(on, OnLootDropped);
            Toggle<LootOverflowEvent>(on, OnLootOverflow);
            Toggle<StoryPanelChangedEvent>(on, OnStoryPanelChanged);
            Toggle<CampaignCompletedEvent>(on, OnCampaignCompleted);
            Toggle<DialogueStartedEvent>(on, OnDialogueStarted);
            Toggle<EnemyAttackWindupEvent>(on, OnEnemyWindup);
            Toggle<PlayerLandedEvent>(on, OnPlayerLanded);
        }

        private static void Toggle<TEvent>(bool on, Action<TEvent> handler) where TEvent : IGameEvent
        {
            if (on) EventBus.Subscribe(handler);
            else EventBus.Unsubscribe(handler);
        }

        // ------------------------------------------------------------------
        // Playback
        // ------------------------------------------------------------------

        /// <summary>Resolves trigger + variants through the map and plays the cue. False when silent, unmapped or limited.</summary>
        public bool Trigger(SfxTrigger trigger, Vector3? position, params string[] variants)
        {
            string cueId = _map != null ? _map.ResolveFirst(trigger, variants) : null;
            return !string.IsNullOrEmpty(cueId) && PlayCue(cueId, position);
        }

        public bool Trigger(SfxTrigger trigger, params string[] variants) => Trigger(trigger, null, variants);

        /// <summary>
        /// Plays one cue by id, applying pause, volume, cooldown and voice limits. Also the entry point
        /// for <c>UiSoundHooks</c> (ui.click / ui.hover / ui.error).
        /// </summary>
        public bool PlayCue(string cueId, Vector3? position = null)
        {
            if (_map == null || !_map.TryGetCue(cueId, out SfxEventMap.Cue cue))
            {
                if (cueId != null && _warnedUnknown.Add(cueId))
                {
                    GameLogger.Warning(LogCategory, $"SfxEventRouter: unknown cue id '{cueId}'.");
                }

                return false;
            }

            if (cue.loop)
            {
                SetAmbience(cueId);
                return true;
            }

            if (cue.pausable && _isGameplayPaused())
            {
                return false;
            }

            if (EffectiveVolume(cue) <= MinAudibleVolume)
            {
                return false;
            }

            float now = _clock();
            if (_lastPlayTime.TryGetValue(cueId, out float last) && now - last < cue.cooldown)
            {
                return false;
            }

            PruneVoices(now);
            if (_voices.Count >= _map.MaxVoices || CountVoices(cueId) >= Mathf.Max(1, cue.maxVoices))
            {
                return false;
            }

            AudioClip clip = ResolveClip(cue);
            if (clip == null)
            {
                return false;
            }

            float pitch = 1f, volume = cue.volume;
            if (cue.pitchJitter > 0f)
            {
                pitch += ((float)_jitter.NextDouble() * 2f - 1f) * Mathf.Min(cue.pitchJitter, 0.3f);
            }

            if (cue.volumeJitter > 0f)
            {
                volume *= 1f - (float)_jitter.NextDouble() * Mathf.Min(cue.volumeJitter, 0.5f);
            }

            if (_audio != null)
            {
                if (cue.spatial && position.HasValue)
                {
                    _audio.PlaySfxAtPosition(clip, position.Value, volume, cue.category, pitch: pitch);
                }
                else
                {
                    _audio.PlaySfx(clip, volume, cue.category, pitch);
                }

                if (cue.ducksMusic)
                {
                    _audio.DuckMusic(0.35f, Mathf.Clamp(clip.length, 0.5f, 2f));
                }
            }

            _lastPlayTime[cueId] = now;
            _voices.Add(new KeyValuePair<string, float>(cueId, now + Mathf.Max(0.02f, clip.length / pitch)));
            LastPlayedCueId = cueId;
            LastPlayedPitch = pitch;
            LastPlayedVolume = volume;
            PlayedCount++;
            return true;
        }

        public int ActiveVoiceCount
        {
            get
            {
                PruneVoices(_clock());
                return _voices.Count;
            }
        }

        private float EffectiveVolume(SfxEventMap.Cue cue)
        {
            return cue.volume * (_audio != null ? _audio.GetEffectiveVolume(cue.category) : 1f);
        }

        private void PruneVoices(float now)
        {
            // Backwards loop instead of RemoveAll(lambda): this runs on every played cue.
            for (int i = _voices.Count - 1; i >= 0; i--)
            {
                if (_voices[i].Value <= now)
                {
                    _voices.RemoveAt(i);
                }
            }
        }

        private int CountVoices(string cueId)
        {
            int count = 0;
            foreach (KeyValuePair<string, float> voice in _voices)
            {
                if (voice.Key == cueId) count++;
            }

            return count;
        }

        /// <summary>The cue's assigned clip, else a cached procedural placeholder (when enabled in the map).</summary>
        public AudioClip ResolveClip(SfxEventMap.Cue cue)
        {
            if (cue == null)
            {
                return null;
            }

            if (cue.clip != null)
            {
                return cue.clip;
            }

            if (_map == null || !_map.ProceduralSfx || cue.preset == ProceduralSfxPreset.None)
            {
                return null;
            }

            if (!_clipCache.TryGetValue(cue.id, out AudioClip clip) || clip == null)
            {
                clip = ProceduralSfxSynth.CreateClip("proc_" + cue.id, cue.preset, cue.frequency);
                _clipCache[cue.id] = clip;
            }

            return clip;
        }

        // ------------------------------------------------------------------
        // Ambience loop (weather)
        // ------------------------------------------------------------------

        /// <summary>Fades to the given looping cue (null/empty = fade out).</summary>
        public void SetAmbience(string cueId)
        {
            if (string.IsNullOrEmpty(cueId) || _map == null || !_map.TryGetCue(cueId, out SfxEventMap.Cue cue))
            {
                _ambienceFadeTarget = 0f;
                return;
            }

            _requestedAmbienceCueId = cueId;
            _ambienceFadeTarget = 1f;

            if (_ambienceCue == cue && _ambienceSource != null && _ambienceSource.clip != null)
            {
                return;
            }

            _ambienceCue = cue;
            _ambienceFade = 0f;

            if (_ambienceSource != null)
            {
                _ambienceSource.clip = ResolveClip(cue);
                _ambienceSource.volume = 0f;
                if (_ambienceSource.clip != null && Application.isPlaying)
                {
                    _ambienceSource.Play();
                }
            }
        }

        /// <summary>Advances ambience fades / pause / volume, delayed cues and the deferred pickup. Called each frame by the driver; public for EditMode tests.</summary>
        public void Tick(float deltaTime)
        {
            if (_pickupPending)
            {
                // Items arrived right after a kill but no EnemyLootDroppedEvent claimed them: a plain pickup.
                _pickupPending = false;
                Trigger(SfxTrigger.ItemPickup);
            }

            RunScheduled();
            TickTitleAmbience(deltaTime);

            bool shouldPause = _ambienceCue != null && _ambienceCue.pausable && _isGameplayPaused();
            if (shouldPause != _ambiencePaused)
            {
                _ambiencePaused = shouldPause;
                if (_ambienceSource != null && Application.isPlaying)
                {
                    if (shouldPause) _ambienceSource.Pause();
                    else _ambienceSource.UnPause();
                }
            }

            float target = _flowAllowsAmbience ? _ambienceFadeTarget : 0f;
            _ambienceFade = Mathf.MoveTowards(_ambienceFade, target, deltaTime / AmbienceFadeSeconds);

            if (_ambienceSource != null && _ambienceCue != null)
            {
                _ambienceSource.volume = _ambienceFade * EffectiveVolume(_ambienceCue);
                if (_ambienceFade <= 0f && target <= 0f && _ambienceSource.isPlaying)
                {
                    _ambienceSource.Stop();
                }
                else if (target > 0f && !_ambienceSource.isPlaying && !_ambiencePaused && _ambienceSource.clip != null && Application.isPlaying)
                {
                    _ambienceSource.Play();
                }
            }
        }

        /// <summary>Current ambience gain before category volumes (0..1). Tests.</summary>
        public float AmbienceFade => _ambienceFade;

        /// <summary>Plays <paramref name="trigger"/> once <paramref name="delaySeconds"/> (clock time) have passed, from <see cref="Tick"/>.</summary>
        public void Schedule(SfxTrigger trigger, float delaySeconds)
        {
            _scheduled.Add(new KeyValuePair<float, SfxTrigger>(_clock() + Mathf.Max(0f, delaySeconds), trigger));
        }

        private void RunScheduled()
        {
            if (_scheduled.Count == 0)
            {
                return;
            }

            float now = _clock();
            for (int i = 0; i < _scheduled.Count; i++)
            {
                if (_scheduled[i].Key <= now)
                {
                    SfxTrigger trigger = _scheduled[i].Value;
                    _scheduled.RemoveAt(i--);
                    Trigger(trigger);
                }
            }
        }

        // ------------------------------------------------------------------
        // Title-screen bed (crickets + breeze under the title music)
        // ------------------------------------------------------------------

        private void SetTitleAmbienceWanted(bool wanted)
        {
            _titleWanted = wanted;
            if (!wanted || _map == null)
            {
                return;
            }

            string cueId = _map.ResolveFirst(SfxTrigger.TitleAmbience);
            if (string.IsNullOrEmpty(cueId) || !_map.TryGetCue(cueId, out SfxEventMap.Cue cue))
            {
                _titleWanted = false;
                return;
            }

            if (_titleCue == cue && _titleSource != null && _titleSource.clip != null)
            {
                return;
            }

            _titleCue = cue;
            if (_titleSource != null)
            {
                _titleSource.clip = ResolveClip(cue);
                _titleSource.volume = 0f;
            }
        }

        private void TickTitleAmbience(float deltaTime)
        {
            if (_titleCue == null)
            {
                return;
            }

            _titleFade = Mathf.MoveTowards(_titleFade, _titleWanted ? 1f : 0f, deltaTime / TitleAmbienceFadeSeconds);
            if (_titleSource == null)
            {
                return;
            }

            _titleSource.volume = _titleFade * EffectiveVolume(_titleCue);
            if (_titleFade <= 0f && !_titleWanted)
            {
                if (_titleSource.isPlaying) _titleSource.Stop();
            }
            else if (_titleWanted && !_titleSource.isPlaying && _titleSource.clip != null && Application.isPlaying)
            {
                _titleSource.Play();
            }
        }

        // ------------------------------------------------------------------
        // Event handlers
        // ------------------------------------------------------------------

        private static Vector3? PositionOf(GameObject go) => go != null ? go.transform.position : (Vector3?)null;

        private void OnDamageApplied(DamageAppliedEvent evt)
        {
            // A reflected Espinhos hit (delivered synchronously inside ThornsRules.TryReflect): its own prick
            // instead of a second thud on top of the hit that caused it.
            if (ThornsRules.IsReflecting)
            {
                Trigger(SfxTrigger.Thorns, PositionOf(evt.Target));
                return;
            }

            if (AudioEventHelpers.IsPlayer(evt.Target))
            {
                Trigger(SfxTrigger.PlayerHurt, PositionOf(evt.Target));
                return;
            }

            Trigger(evt.IsCritical ? SfxTrigger.CritHit : SfxTrigger.Hit, PositionOf(evt.Target));
        }

        private void OnEnemyDied(EnemyDiedEvent evt) => Trigger(evt.WasBoss ? SfxTrigger.BossDeath : SfxTrigger.EnemyDeath, PositionOf(evt.Enemy));

        private void OnPlayerDied(PlayerDiedEvent evt) => Trigger(SfxTrigger.PlayerDeath);

        private void OnExperienceChanged(PlayerExperienceChangedEvent evt)
        {
            if (_levelUp.Observe(evt.Level))
            {
                Trigger(SfxTrigger.LevelUp);
            }
        }

        private void OnGameLoaded(GameLoadedEvent evt) => ResetBaselines();

        private void OnNewSession(NewSessionResetEvent evt) => ResetBaselines();

        private void ResetBaselines()
        {
            _levelUp.Reset();
            _lastInventoryCount = -1;
        }

        private void OnSkillCast(SkillCastEvent evt)
        {
            SkillVfxCategory category = SkillVfxClassifier.Classify(evt.Skill);
            string element = evt.Skill != null ? VariantKey.Of(evt.Skill.Element) : null;
            string categoryKey = VariantKey.Of(category);
            Vector3? position = PositionOf(evt.Caster);

            if (category == SkillVfxCategory.Ultimate)
            {
                Trigger(SfxTrigger.SkillCast, position, categoryKey, element);
            }
            else
            {
                Trigger(SfxTrigger.SkillCast, position, element, categoryKey);
            }
        }

        private void OnInventoryChanged(InventoryChangedEvent evt)
        {
            int count = _inventoryCount();
            bool gained = _lastInventoryCount >= 0 && count > _lastInventoryCount;
            _lastInventoryCount = count;

            if (!gained || _clock() - _lastTradeTime <= PickupSuppressAfterTradeSeconds)
            {
                return;
            }

            if (_clock() - _lastKillTime <= 0f)
            {
                // Loot of the kill being processed right now: EnemyLootDroppedEvent follows in this same
                // dispatch and plays loot.drop instead. If it never comes, Tick plays the pickup.
                _pickupPending = true;
                return;
            }

            Trigger(SfxTrigger.ItemPickup);
        }

        // The router subscribes at boot, before EnemyLootGranter, so this runs before the loot is granted.
        private void OnCombatTargetDied(CombatTargetDiedEvent evt) => _lastKillTime = _clock();

        private void OnLootDropped(EnemyLootDroppedEvent evt)
        {
            _pickupPending = false;
            if (evt.Drops != null && evt.Drops.Count > 0)
            {
                // Many kills in one frame (AoE) or a boss's pile: the cue's cooldown + single voice keep it to one clink.
                Trigger(SfxTrigger.LootDrop);
            }
        }

        private void OnLootOverflow(LootOverflowEvent evt) => Trigger(SfxTrigger.BagFull);

        private void OnStunApplied(StunAppliedEvent evt) => Trigger(SfxTrigger.Stun, PositionOf(evt.Target));

        private void OnEnemyWindup(EnemyAttackWindupEvent evt) => Trigger(SfxTrigger.EnemyWindup, evt.Position);

        private void OnPlayerLanded(PlayerLandedEvent evt) => Trigger(SfxTrigger.HeroLanding, evt.Position);

        private void OnDialogueStarted(DialogueStartedEvent evt) => Trigger(SfxTrigger.NpcTalk);

        private void OnCampaignCompleted(CampaignCompletedEvent evt) => Schedule(SfxTrigger.CampaignCompleted, CampaignStingDelaySeconds);

        private void OnStoryPanelChanged(StoryPanelChangedEvent evt)
        {
            bool ending = StoryPresentationRules.RollsCredits(evt.Beat);
            switch (evt.Change)
            {
                case StoryPanelChange.Opened: Trigger(SfxTrigger.StoryOpen); break;
                case StoryPanelChange.Advanced: Trigger(SfxTrigger.StoryPage); break;
                case StoryPanelChange.Skipped: Trigger(ending ? SfxTrigger.StoryToCredits : SfxTrigger.StorySkip); break;
                default: Trigger(ending ? SfxTrigger.StoryToCredits : SfxTrigger.StoryClose); break;
            }
        }

        private void MarkTrade() => _lastTradeTime = _clock();

        private void OnItemPurchased(ItemPurchasedEvent evt) { MarkTrade(); Trigger(SfxTrigger.ItemPurchased); }

        private void OnItemSold(ItemSoldEvent evt) { MarkTrade(); Trigger(SfxTrigger.ItemSold); }

        private void OnRecipeCrafted(RecipeCraftedEvent evt) { MarkTrade(); Trigger(SfxTrigger.RecipeCrafted); }

        private void OnQuestStarted(QuestStartedEvent evt) => Trigger(SfxTrigger.QuestStarted);

        private void OnQuestCompleted(QuestCompletedEvent evt) => Trigger(SfxTrigger.QuestCompleted);

        private void OnAchievementUnlocked(AchievementUnlockedEvent evt) => Trigger(SfxTrigger.AchievementUnlocked);

        // Almost every full-screen menu (inventory, shop, settings, pause, death...) holds a
        // GameplayPause, so the first acquire / last release is the "screen opened / closed" signal.
        // The village hub, story panels and dialogue boxes have their own sounds (see PauseSource).
        private void OnGamePaused(GamePausedEvent evt)
        {
            _ownerBuffer.Clear();
            GameplayPause.CopyOwnersTo(_ownerBuffer);
            _pauseSource = ClassifyPauseOwners(_ownerBuffer);
            _ownerBuffer.Clear();

            switch (_pauseSource)
            {
                case PauseSource.Village: Trigger(SfxTrigger.VillageOpen); break;
                case PauseSource.Generic: Trigger(SfxTrigger.UiOpen); break;
            }
        }

        private void OnGameResumed(GameResumedEvent evt)
        {
            PauseSource source = _pauseSource;
            _pauseSource = PauseSource.Generic;

            switch (source)
            {
                case PauseSource.Village: Trigger(SfxTrigger.VillageClose); break;
                case PauseSource.Story: break;
                default: Trigger(SfxTrigger.UiClose); break;
            }
        }

        /// <summary>Which screen holds the pause, by owner type: village hub, story panel, dialogue box, else generic.</summary>
        public static PauseSource ClassifyPauseOwners(IReadOnlyList<object> owners)
        {
            if (owners == null)
            {
                return PauseSource.Generic;
            }

            foreach (object owner in owners)
            {
                switch (owner)
                {
                    case VillageHubButton _: return PauseSource.Village;
                    case StoryPanelController _: return PauseSource.Story;
                    case DialogueUIController _: return PauseSource.Dialogue;
                }
            }

            return PauseSource.Generic;
        }

        private void OnShopOpened(ShopOpenedEvent evt) => Trigger(SfxTrigger.UiOpen);

        private void OnShopClosed(ShopClosedEvent evt) => Trigger(SfxTrigger.UiClose);

        private void OnEncounterStarted(EncounterStartedEvent evt)
        {
            string biome = evt.Payload != null ? VariantKey.Of(evt.Payload.biome) : null;
            Trigger(SfxTrigger.EncounterStart, biome);
        }

        private void OnEncounterEnded(EncounterEndedEvent evt)
        {
            switch (evt.Outcome)
            {
                case EncounterOutcome.Victory: Trigger(SfxTrigger.EncounterVictory); break;
                case EncounterOutcome.Defeat: Trigger(SfxTrigger.EncounterDefeat); break;
                default: Trigger(SfxTrigger.EncounterFled); break;
            }
        }

        private void OnPhaseChanged(TimeOfDayPhaseChangedEvent evt)
        {
            if (_flowAllowsAmbience)
            {
                Trigger(SfxTrigger.TimeOfDayPhase, VariantKey.Of(evt.CurrentPhase));
            }
        }

        private void OnWeatherChanged(WeatherChangedEvent evt)
        {
            string cueId = _map != null ? _map.ResolveFirst(SfxTrigger.WeatherLoop, VariantKey.Of(evt.CurrentWeather)) : null;
            SetAmbience(cueId);
        }

        private void OnFlowStateChanged(GameFlowStateChangedEvent evt)
        {
            _flowAllowsAmbience = evt.Current == GameFlowState.World || evt.Current == GameFlowState.Encounter;
            SetTitleAmbienceWanted(evt.Current == GameFlowState.MainMenu || evt.Current == GameFlowState.CharacterSelect);
        }
    }

    /// <summary>Per-frame driver for <see cref="SfxEventRouter.Tick"/> (unscaled, so fades run while paused).</summary>
    internal class SfxEventRouterDriver : MonoBehaviour
    {
        private SfxEventRouter _owner;

        public void Bind(SfxEventRouter owner) => _owner = owner;

        private void Update() => _owner?.Tick(Time.unscaledDeltaTime);
    }

    /// <summary>
    /// Cached <c>SfxEventMap.Normalize(value.ToString())</c> per enum value, so per-event variant keys
    /// (skill element/category on every cast, biome, phase, weather) don't allocate after the first use.
    /// Main-thread only, like the EventBus handlers that use it.
    /// </summary>
    internal static class VariantKey<T> where T : struct, Enum
    {
        private static readonly Dictionary<T, string> Cache = new Dictionary<T, string>();

        public static string Of(T value)
        {
            if (!Cache.TryGetValue(value, out string key))
            {
                key = SfxEventMap.Normalize(value.ToString());
                Cache[value] = key;
            }

            return key;
        }
    }

    /// <summary>Type-inferring entry point for <see cref="VariantKey{T}"/>.</summary>
    internal static class VariantKey
    {
        public static string Of<T>(T value) where T : struct, Enum => VariantKey<T>.Of(value);
    }
}
