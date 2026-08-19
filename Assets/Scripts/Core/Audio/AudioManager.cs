using System;
using System.Collections.Generic;
using Rapadura.Core.Events;
using Rapadura.Core.Interfaces;
using Rapadura.Core.Logging;
using Rapadura.Gameplay.Combat;
using UnityEngine;

namespace Rapadura.Core.Audio
{
    /// <summary>
    /// Top-level audio system: ambient music with crossfade, pooled SFX (2D and 3D), UI sounds,
    /// per-category volume (persisted via PlayerPrefs) and simple dynamic ducking. Implements
    /// <see cref="IManager"/> like every other top-level system and is meant to be constructed and
    /// registered by <see cref="Rapadura.Core.Managers.GameManager"/> — see the note at the bottom
    /// of this file for the wiring that still needs to happen there.
    ///
    /// MIXING NOTE: this project has never been opened in the Unity Editor, so no real
    /// <c>AudioMixer</c> (.mixer) asset exists yet — those are Editor-authored binary/YAML assets
    /// and can't be safely hand-written. Until someone opens the project in the Editor and builds
    /// one, category "buses" here are a pure C# concept: each category has a 0..1 volume multiplied
    /// directly into every AudioSource.volume that belongs to it. Migrating to real Audio Mixer
    /// groups later means: create the .mixer asset with Master/Music/SFX/UI groups exposing
    /// volume parameters, assign <see cref="AudioSource.outputAudioMixerGroup"/> on pooled sources
    /// instead of scaling volume by hand, and drive the exposed parameters with
    /// AudioMixer.SetFloat (in decibels) instead of <see cref="SetCategoryVolume"/>. The public API
    /// of this class (categories, volumes 0..1, ducking) is designed to stay the same across that
    /// migration so callers would not need to change.
    /// </summary>
    public class AudioManager : IManager
    {
        private const string LogCategory = "Audio";
        private const string PlayerPrefsKeyPrefix = "Audio.Volume.";
        private const int DefaultPoolInitialSize = 8;
        private const int DefaultPoolMaxSize = 24;
        private const float DefaultMusicCrossfadeDuration = 1.5f;
        private const float DefaultDuckAmount = 0.35f;
        private const float DefaultDuckDuration = 2f;

        private readonly Dictionary<AudioCategory, float> _categoryVolumes = new Dictionary<AudioCategory, float>();
        private readonly AudioCueDatabase _cueDatabase;
        private readonly CombatAudioCueMap _combatCueMap;

        private GameObject _root;
        private AudioSourcePool _sfxPool;
        private AudioSourcePool _uiPool;
        private AudioSource _musicSourceA;
        private AudioSource _musicSourceB;
        private bool _musicIsUsingSourceA = true;

        // Crossfade state.
        private bool _isCrossfading;
        private float _crossfadeElapsed;
        private float _crossfadeDuration;
        private float _musicBaseVolume = 1f;

        // Ducking state.
        private float _duckMultiplier = 1f;
        private float _duckTargetMultiplier = 1f;
        private float _duckTimeRemaining;
        private float _duckFadeDuration;

        private AudioManagerDriver _driver;

        public bool IsInitialized { get; private set; }

        /// <summary>Optional data-driven cue table (id -> clip/category/volume). May be null; PlayOneShot(id) then no-ops with a warning.</summary>
        public AudioManager(AudioCueDatabase cueDatabase = null, CombatAudioCueMap combatCueMap = null)
        {
            _cueDatabase = cueDatabase;
            _combatCueMap = combatCueMap;
        }

        public void Initialize()
        {
            if (IsInitialized)
            {
                return;
            }

            foreach (AudioCategory category in (AudioCategory[])Enum.GetValues(typeof(AudioCategory)))
            {
                _categoryVolumes[category] = LoadVolumeFromPrefs(category);
            }

            _root = new GameObject("AudioManager");

            if (Application.isPlaying)
            {
                // DontDestroyOnLoad is only valid in Play mode; EditMode tests construct/tear down
                // AudioManager without ever entering Play mode, so this is skipped there.
                UnityEngine.Object.DontDestroyOnLoad(_root);
            }

            _musicSourceA = CreateMusicSource("MusicSourceA");
            _musicSourceB = CreateMusicSource("MusicSourceB");

            _sfxPool = new AudioSourcePool(_root.transform, DefaultPoolInitialSize, DefaultPoolMaxSize);
            _uiPool = new AudioSourcePool(_root.transform, DefaultPoolInitialSize / 2, DefaultPoolMaxSize / 2);

            _driver = _root.AddComponent<AudioManagerDriver>();
            _driver.Bind(this);

            EventBus.Subscribe<DamageAppliedEvent>(OnDamageApplied);
            EventBus.Subscribe<CombatTargetDiedEvent>(OnCombatTargetDied);

            IsInitialized = true;
            GameLogger.Info(LogCategory, "AudioManager initialized.");
        }

        public void Shutdown()
        {
            if (!IsInitialized)
            {
                return;
            }

            EventBus.Unsubscribe<DamageAppliedEvent>(OnDamageApplied);
            EventBus.Unsubscribe<CombatTargetDiedEvent>(OnCombatTargetDied);

            if (_root != null)
            {
                // DestroyImmediate is required in edit mode (tests); Destroy is preferred at runtime
                // to avoid the perf hit of an immediate destroy during normal play.
                if (Application.isPlaying)
                {
                    UnityEngine.Object.Destroy(_root);
                }
                else
                {
                    UnityEngine.Object.DestroyImmediate(_root);
                }

                _root = null;
            }

            IsInitialized = false;
        }

        private AudioSource CreateMusicSource(string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(_root.transform, false);
            var source = go.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = true;
            source.spatialBlend = 0f;
            source.volume = 0f;
            return source;
        }

        // ------------------------------------------------------------------
        // Category volume (persisted, PlayerPrefs-backed until SaveManager
        // integration is in scope — see Fase 9 note in TODO.md).
        // ------------------------------------------------------------------

        public float GetCategoryVolume(AudioCategory category)
        {
            return _categoryVolumes.TryGetValue(category, out float volume) ? volume : 1f;
        }

        public void SetCategoryVolume(AudioCategory category, float volume)
        {
            volume = Mathf.Clamp01(volume);
            _categoryVolumes[category] = volume;
            PlayerPrefs.SetFloat(PlayerPrefsKeyPrefix + category, volume);
            ApplyVolumesToActiveSources();
            EventBus.Publish(new AudioCategoryVolumeChangedEvent(category, volume));
        }

        /// <summary>Final multiplier applied to any AudioSource in this category: Master * Category (* duck for Music).</summary>
        public float GetEffectiveVolume(AudioCategory category)
        {
            if (category == AudioCategory.Master)
            {
                return GetCategoryVolume(AudioCategory.Master);
            }

            float effective = GetCategoryVolume(AudioCategory.Master) * GetCategoryVolume(category);

            if (category == AudioCategory.Music)
            {
                effective *= _duckMultiplier;
            }

            return effective;
        }

        private float LoadVolumeFromPrefs(AudioCategory category)
        {
            return PlayerPrefs.GetFloat(PlayerPrefsKeyPrefix + category, 1f);
        }

        private void ApplyVolumesToActiveSources()
        {
            if (_musicSourceA != null)
            {
                _musicSourceA.volume = _musicSourceA.isPlaying ? _musicBaseVolume * GetEffectiveVolume(AudioCategory.Music) : _musicSourceA.volume;
            }

            // Pooled one-shots re-read effective volume at play time, so nothing to update here.
        }

        // ------------------------------------------------------------------
        // Music / ambient, with crossfade between two dedicated sources.
        // ------------------------------------------------------------------

        public void PlayMusic(AudioClip clip, float crossfadeDuration = DefaultMusicCrossfadeDuration, float volume = 1f)
        {
            if (clip == null || _musicSourceA == null)
            {
                return;
            }

            AudioSource incoming = _musicIsUsingSourceA ? _musicSourceB : _musicSourceA;
            AudioSource outgoing = _musicIsUsingSourceA ? _musicSourceA : _musicSourceB;

            incoming.clip = clip;
            incoming.volume = 0f;
            incoming.Play();

            _musicBaseVolume = Mathf.Clamp01(volume);
            _crossfadeDuration = Mathf.Max(0.01f, crossfadeDuration);
            _crossfadeElapsed = 0f;
            _isCrossfading = true;
            _musicIsUsingSourceA = !_musicIsUsingSourceA;

            EventBus.Publish(new MusicTrackChangedEvent(clip.name));

            // If nothing was playing before (outgoing silent/stopped), skip the fade-out side.
            if (!outgoing.isPlaying)
            {
                outgoing.volume = 0f;
            }
        }

        public void StopMusic(float fadeOutDuration = DefaultMusicCrossfadeDuration)
        {
            _crossfadeDuration = Mathf.Max(0.01f, fadeOutDuration);
            _crossfadeElapsed = 0f;
            _isCrossfading = true;
            // Both targets fade to 0 by pointing "incoming" at silence: reuse Tick logic by
            // clearing the clip on whichever source is about to become "incoming".
            _musicIsUsingSourceA = !_musicIsUsingSourceA;
            AudioSource incoming = _musicIsUsingSourceA ? _musicSourceB : _musicSourceA;
            incoming.Stop();
            incoming.clip = null;
        }

        /// <summary>
        /// Advances crossfade/ducking timers by <paramref name="deltaTime"/> seconds. Called every
        /// frame by the internal <see cref="AudioManagerDriver"/>; public so EditMode tests (which
        /// never enter Play mode, so Update() never runs) can drive it deterministically.
        /// </summary>
        public void Tick(float deltaTime)
        {
            TickCrossfade(deltaTime);
            TickDuck(deltaTime);
        }

        private void TickCrossfade(float deltaTime)
        {
            if (!_isCrossfading)
            {
                return;
            }

            _crossfadeElapsed += deltaTime;
            float t = Mathf.Clamp01(_crossfadeElapsed / _crossfadeDuration);

            AudioSource incoming = _musicIsUsingSourceA ? _musicSourceA : _musicSourceB;
            AudioSource outgoing = _musicIsUsingSourceA ? _musicSourceB : _musicSourceA;

            float targetVolume = _musicBaseVolume * GetEffectiveVolume(AudioCategory.Music);
            incoming.volume = Mathf.Lerp(0f, targetVolume, t);
            outgoing.volume = Mathf.Lerp(outgoing.volume, 0f, t);

            if (t >= 1f)
            {
                _isCrossfading = false;
                outgoing.Stop();
                outgoing.volume = 0f;
            }
        }

        // ------------------------------------------------------------------
        // Ducking.
        // ------------------------------------------------------------------

        /// <summary>
        /// Temporarily lowers music volume by <paramref name="amount"/> (0..1 fraction to cut) for
        /// <paramref name="duration"/> seconds, then restores it. Generic on purpose: nothing in the
        /// codebase publishes a "dialogue started" event yet (dialogue is Fase 8, not implemented),
        /// so this is the hook a future DialogueManager/cutscene system should call directly, e.g.
        /// <c>audioManager.DuckMusic(0.6f, 3f)</c> when a line of dialogue starts.
        /// </summary>
        public void DuckMusic(float amount, float duration)
        {
            _duckTargetMultiplier = Mathf.Clamp01(1f - Mathf.Clamp01(amount));
            _duckMultiplier = _duckTargetMultiplier;
            _duckTimeRemaining = Mathf.Max(0f, duration);
            _duckFadeDuration = 0.25f;
        }

        public float CurrentDuckMultiplier => _duckMultiplier;

        private void TickDuck(float deltaTime)
        {
            if (_duckTimeRemaining <= 0f)
            {
                if (!Mathf.Approximately(_duckMultiplier, 1f))
                {
                    _duckMultiplier = Mathf.MoveTowards(_duckMultiplier, 1f, deltaTime / Mathf.Max(0.01f, _duckFadeDuration));
                }

                return;
            }

            _duckTimeRemaining -= deltaTime;
        }

        // ------------------------------------------------------------------
        // Generic SFX / UI playback (pooled, no Instantiate/Destroy per sound).
        // ------------------------------------------------------------------

        public void PlaySfx(AudioClip clip, float volume = 1f, AudioCategory category = AudioCategory.Sfx)
        {
            if (clip == null)
            {
                return;
            }

            AudioSourcePool pool = category == AudioCategory.Ui ? _uiPool : _sfxPool;
            AudioSource source = pool?.Acquire();

            if (source == null)
            {
                return;
            }

            source.spatialBlend = 0f;
            source.transform.localPosition = Vector3.zero;
            source.clip = clip;
            source.volume = Mathf.Clamp01(volume) * GetEffectiveVolume(category);
            source.Play();

            MaybeDuckForHighPriority(category, volume);
        }

        /// <summary>Plays a 3D (spatialized) sound at a world position, e.g. an impact effect far from the player.</summary>
        public void PlaySfxAtPosition(AudioClip clip, Vector3 position, float volume = 1f, AudioCategory category = AudioCategory.Sfx, float minDistance = 1f, float maxDistance = 25f)
        {
            if (clip == null)
            {
                return;
            }

            AudioSourcePool pool = category == AudioCategory.Ui ? _uiPool : _sfxPool;
            AudioSource source = pool?.Acquire();

            if (source == null)
            {
                return;
            }

            source.transform.position = position;
            source.spatialBlend = 1f;
            source.minDistance = minDistance;
            source.maxDistance = maxDistance;
            source.clip = clip;
            source.volume = Mathf.Clamp01(volume) * GetEffectiveVolume(category);
            source.Play();

            MaybeDuckForHighPriority(category, volume);
        }

        /// <summary>Plays a cue by id, looked up in the configured <see cref="AudioCueDatabase"/>.</summary>
        public void PlayOneShot(string cueId)
        {
            if (_cueDatabase == null)
            {
                GameLogger.Warning(LogCategory, $"PlayOneShot('{cueId}') called but no AudioCueDatabase is configured.");
                return;
            }

            if (!_cueDatabase.TryGetCue(cueId, out AudioCue cue))
            {
                GameLogger.Warning(LogCategory, $"Unknown audio cue id '{cueId}'.");
                return;
            }

            PlaySfx(cue.Clip, cue.Volume, cue.Category);

            if (cue.DucksMusic)
            {
                DuckMusic(DefaultDuckAmount, DefaultDuckDuration);
            }
        }

        /// <summary>Plays a cue by id at a world position (spatial). See <see cref="PlayOneShot"/>.</summary>
        public void PlayOneShotAtPosition(string cueId, Vector3 position)
        {
            if (_cueDatabase == null || !_cueDatabase.TryGetCue(cueId, out AudioCue cue))
            {
                GameLogger.Warning(LogCategory, $"Unknown audio cue id '{cueId}'.");
                return;
            }

            PlaySfxAtPosition(cue.Clip, position, cue.Volume, cue.Category);

            if (cue.DucksMusic)
            {
                DuckMusic(DefaultDuckAmount, DefaultDuckDuration);
            }
        }

        private void MaybeDuckForHighPriority(AudioCategory category, float volume)
        {
            // A loud SFX (near-max volume) briefly ducks the music, independent of whichever cue
            // metadata triggered it — cheap "dynamic" ducking without needing per-sound flags.
            if (category == AudioCategory.Sfx && volume >= 0.95f)
            {
                DuckMusic(DefaultDuckAmount * 0.5f, 0.6f);
            }
        }

        // ------------------------------------------------------------------
        // Combat event wiring (consumes CombatEvents.cs, never edits it).
        // ------------------------------------------------------------------

        private void OnDamageApplied(DamageAppliedEvent evt)
        {
            if (_combatCueMap == null)
            {
                return;
            }

            CombatAudioTrigger trigger = evt.IsCritical ? CombatAudioTrigger.CriticalHit : CombatAudioTrigger.DamageApplied;

            if (_combatCueMap.TryGetCueId(trigger, out string cueId))
            {
                Vector3 position = evt.Target != null ? evt.Target.transform.position : Vector3.zero;
                PlayOneShotAtPosition(cueId, position);
            }
        }

        private void OnCombatTargetDied(CombatTargetDiedEvent evt)
        {
            if (_combatCueMap == null)
            {
                return;
            }

            if (_combatCueMap.TryGetCueId(CombatAudioTrigger.TargetDied, out string cueId))
            {
                Vector3 position = evt.Target != null ? evt.Target.transform.position : Vector3.zero;
                PlayOneShotAtPosition(cueId, position);
            }
        }

        // Exposed mainly for EditMode tests to inspect pool state; gameplay code should not need these.
        public AudioSourcePool SfxPoolForTests => _sfxPool;
        public AudioSourcePool UiPoolForTests => _uiPool;
    }

    /// <summary>
    /// Tiny MonoBehaviour whose only job is to call <see cref="AudioManager.Tick"/> every frame.
    /// AudioManager itself stays a plain C# class (like every other IManager in this project) so
    /// its logic is unit-testable without a scene; this driver is the one place it touches
    /// MonoBehaviour.Update. Created and owned by AudioManager.Initialize().
    /// </summary>
    // NOTE: registered via the default no-arg AudioManager() constructor in
    // GameManager.BuildManagers() (cueDatabase/combatCueMap are optional; wire real assets there
    // once Resources/Addressables references for them exist).
    internal class AudioManagerDriver : MonoBehaviour
    {
        private AudioManager _owner;

        public void Bind(AudioManager owner)
        {
            _owner = owner;
        }

        private void Update()
        {
            _owner?.Tick(Time.deltaTime);
        }
    }
}
