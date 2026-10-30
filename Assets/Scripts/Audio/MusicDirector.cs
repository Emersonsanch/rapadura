using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Rapadura.Core.Audio;
using Rapadura.Core.Events;
using Rapadura.Core.Interfaces;
using Rapadura.Core.Logging;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Flow;
using Rapadura.Gameplay.Story;
using Rapadura.Gameplay.World.Map;
using Rapadura.UI.Story;
using UnityEngine;

namespace Rapadura.Audio
{
    /// <summary>
    /// Picks the music for the current game context and crossfades to it through
    /// <see cref="AudioManager.PlayMusic"/> (so the Music volume slider and dialogue ducking apply):
    /// title (main menu / character select), world map, encounter per biome, boss encounter.
    /// Context comes from <see cref="GameFlowStateChangedEvent"/> plus the encounter payload on
    /// <see cref="EncounterStartedEvent"/> (biome, and whether any enemy is a boss).
    ///
    /// Clips come from <see cref="SfxEventMap"/> music entries. Without a clip, and while the map's
    /// "procedural music" flag is on, a 16-beat drone + arpeggio loop (~200-300k samples) is baked by
    /// <see cref="ProceduralSfxSynth.GenerateMusicLoop"/> on a worker thread (it is pure C#); only
    /// AudioClip.Create/SetData runs on the main thread once the samples are ready (<see cref="Tick"/>),
    /// so the first entry into a context no longer hitches — the previous track keeps playing until
    /// then. Only the current and previous procedural clips are kept
    /// (<see cref="MaxCachedProceduralClips"/>), and Title + WorldMap are pre-baked at boot, behind the
    /// title screen. Turning the flag off gives silence with the same hooks — assigning real clips in
    /// the asset is the only change a composer needs (Docs/Audio.md).
    ///
    /// Story panels are an overlay context: while <c>StoryPanelController</c> shows a beat (or the credits
    /// after the ending) the music crossfades to <see cref="MusicContext.Story"/> (variant
    /// <see cref="EndingVariant"/> for the epilogue), and back to whatever the flow asked for meanwhile
    /// once the panel is gone. Polled in <see cref="Tick"/> (one static read per frame).
    /// </summary>
    public class MusicDirector : IManager
    {
        /// <summary>Current + previous track (the previous one may still be fading out).</summary>
        public const int MaxCachedProceduralClips = 2;

        private const string LogCategory = "Audio";

        /// <summary>Story music variant for the ending beat and the credits that follow it.</summary>
        public const string EndingVariant = "ending";

        private sealed class PendingBake
        {
            public string Name;
            public Task<float[]> Samples;
        }

        private readonly AudioManager _audio;
        private readonly Dictionary<string, AudioClip> _proceduralCache = new Dictionary<string, AudioClip>(StringComparer.Ordinal);
        // Cache keys, least recently used first.
        private readonly List<string> _cacheOrder = new List<string>();
        private readonly Dictionary<string, PendingBake> _pendingBakes = new Dictionary<string, PendingBake>(StringComparer.Ordinal);
        private readonly List<string> _completedKeys = new List<string>();
        private SfxEventMap _map;
        private GameObject _driverRoot;

        // Cache key of the current context's loop while it is still baking (played as soon as ready).
        private string _awaitingKey;
        private float _awaitingVolume;

        // What the game flow asked for (restored when the story overlay ends).
        private MusicContext _baseContext = MusicContext.None;
        private string _baseVariant = string.Empty;
        private bool _storyActive;
        private readonly Func<string> _storyOverlay;

        public bool IsInitialized { get; private set; }

        public MusicContext CurrentContext { get; private set; } = MusicContext.None;
        public string CurrentVariant { get; private set; } = string.Empty;
        public AudioClip CurrentClip { get; private set; }

        /// <summary>True while the current context's procedural loop is still baking (the previous track keeps playing).</summary>
        public bool IsWaitingForBake => _awaitingKey != null;

        public int PendingBakeCount => _pendingBakes.Count;

        public int CachedProceduralClipCount => _proceduralCache.Count;

        /// <summary>True while the story overlay context is playing.</summary>
        public bool IsStoryOverlayActive => _storyActive;

        /// <summary>The context the game flow wants under any overlay.</summary>
        public MusicContext BaseContext => _baseContext;

        /// <param name="storyOverlay">
        /// Null when no story panel is up, else the <see cref="MusicContext.Story"/> variant ("" or
        /// <see cref="EndingVariant"/>). Defaults to reading <c>StoryPanelController</c>; tests inject it.
        /// </param>
        public MusicDirector(AudioManager audio, SfxEventMap map = null, Func<string> storyOverlay = null)
        {
            _audio = audio;
            _map = map;
            _storyOverlay = storyOverlay ?? DefaultStoryOverlay;
        }

        private string DefaultStoryOverlay()
        {
            if (!StoryPanelController.IsShowing)
            {
                return null;
            }

            StoryBeat beat = StoryPanelController.ShowingBeat;
            if (beat == StoryBeat.None)
            {
                // The credits rolled by the ending: keep the epilogue track.
                return _storyActive ? CurrentVariant : string.Empty;
            }

            return StoryPresentationRules.RollsCredits(beat) ? EndingVariant : string.Empty;
        }

        /// <summary>The flow's context: played now unless the story overlay is up, restored when it ends.</summary>
        private void RequestBase(MusicContext context, string variant = null)
        {
            _baseContext = context;
            _baseVariant = SfxEventMap.Normalize(variant);
            if (!_storyActive)
            {
                SetContext(context, _baseVariant);
            }
        }

        private void PollStoryOverlay()
        {
            string overlay = _storyOverlay != null ? _storyOverlay() : null;
            if (overlay != null)
            {
                _storyActive = true;
                SetContext(MusicContext.Story, overlay);
            }
            else if (_storyActive)
            {
                _storyActive = false;
                SetContext(_baseContext, _baseVariant);
            }
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

            EventBus.Subscribe<GameFlowStateChangedEvent>(OnFlowStateChanged);
            EventBus.Subscribe<EncounterStartedEvent>(OnEncounterStarted);

            if (Application.isPlaying)
            {
                _driverRoot = new GameObject("MusicDirector");
                UnityEngine.Object.DontDestroyOnLoad(_driverRoot);
                _driverRoot.AddComponent<MusicDirectorDriver>().Bind(this);

                // Boot runs behind the title screen: get the first two tracks the player hears ready.
                Prebake(MusicContext.Title);
                Prebake(MusicContext.WorldMap);
            }

            IsInitialized = true;
        }

        public void Shutdown()
        {
            if (!IsInitialized)
            {
                return;
            }

            EventBus.Unsubscribe<GameFlowStateChangedEvent>(OnFlowStateChanged);
            EventBus.Unsubscribe<EncounterStartedEvent>(OnEncounterStarted);

            foreach (AudioClip clip in _proceduralCache.Values)
            {
                DestroyClip(clip);
            }

            _proceduralCache.Clear();
            _cacheOrder.Clear();
            // Worker tasks still running just finish into nothing: their results are never read.
            _pendingBakes.Clear();
            _awaitingKey = null;

            if (_driverRoot != null)
            {
                if (Application.isPlaying) UnityEngine.Object.Destroy(_driverRoot);
                else UnityEngine.Object.DestroyImmediate(_driverRoot);
                _driverRoot = null;
            }

            IsInitialized = false;
        }

        /// <summary>Switches music context (no-op if unchanged). Returns true when the track changed.</summary>
        public bool SetContext(MusicContext context, string variant = null)
        {
            string key = SfxEventMap.Normalize(variant);
            if (context == CurrentContext && key == CurrentVariant)
            {
                return false;
            }

            CurrentContext = context;
            CurrentVariant = key;
            CurrentClip = ResolveClip(context, key, out float volume, out string bakingKey);
            _awaitingKey = bakingKey;
            _awaitingVolume = volume;

            if (bakingKey == null)
            {
                PlayCurrent(volume);
            }

            // else: keep whatever is playing until the loop is baked; Tick() starts it.
            return true;
        }

        private void PlayCurrent(float volume)
        {
            if (_audio == null)
            {
                return;
            }

            float fade = _map != null ? _map.MusicCrossfadeSeconds : 1.5f;
            if (CurrentClip != null) _audio.PlayMusic(CurrentClip, fade, volume);
            else _audio.StopMusic(fade);
        }

        /// <summary>
        /// Main thread, once per frame (driver): turns finished background bakes into AudioClips,
        /// caches them and starts the awaited track. Public so EditMode tests can pump it.
        /// </summary>
        public void Tick()
        {
            PollStoryOverlay();

            if (_pendingBakes.Count == 0)
            {
                return;
            }

            _completedKeys.Clear();
            foreach (KeyValuePair<string, PendingBake> pair in _pendingBakes)
            {
                if (pair.Value.Samples.IsCompleted)
                {
                    _completedKeys.Add(pair.Key);
                }
            }

            foreach (string cacheKey in _completedKeys)
            {
                PendingBake bake = _pendingBakes[cacheKey];
                _pendingBakes.Remove(cacheKey);

                if (bake.Samples.Status != TaskStatus.RanToCompletion || bake.Samples.Result == null)
                {
                    GameLogger.Warning(LogCategory, $"MusicDirector: baking '{bake.Name}' failed: {bake.Samples.Exception?.GetBaseException().Message}");
                    if (cacheKey == _awaitingKey)
                    {
                        _awaitingKey = null;
                        PlayCurrent(_awaitingVolume);
                    }

                    continue;
                }

                AudioClip clip = ProceduralSfxSynth.ToClip(bake.Name, bake.Samples.Result, ProceduralSfxSynth.SampleRate);
                bool awaited = cacheKey == _awaitingKey;
                if (awaited)
                {
                    _awaitingKey = null;
                    CurrentClip = clip;
                }

                StoreInCache(cacheKey, clip);

                if (awaited)
                {
                    PlayCurrent(_awaitingVolume);
                }
            }
        }

        /// <summary>Tests/tooling: waits for every pending bake (up to the timeout each), then <see cref="Tick"/>s. True when none is left pending.</summary>
        public bool FlushPendingBakes(int timeoutMilliseconds = 10000)
        {
            foreach (PendingBake bake in _pendingBakes.Values)
            {
                try
                {
                    bake.Samples.Wait(timeoutMilliseconds);
                }
                catch (AggregateException)
                {
                    // Reported by Tick.
                }
            }

            Tick();
            return _pendingBakes.Count == 0;
        }

        /// <summary>Starts baking a context's procedural loop in the background without switching to it.</summary>
        public void Prebake(MusicContext context, string variant = null)
        {
            ResolveClip(context, SfxEventMap.Normalize(variant), out _, out _);
        }

        /// <summary>Assigned clip or already-baked procedural loop for (context, variant); null while a loop is still baking (it starts baking here).</summary>
        public AudioClip ResolveClip(MusicContext context, string variant, out float volume)
        {
            return ResolveClip(context, variant, out volume, out _);
        }

        private AudioClip ResolveClip(MusicContext context, string variant, out float volume, out string bakingKey)
        {
            bakingKey = null;
            volume = 0.35f;
            if (context == MusicContext.None || _map == null || !_map.TryGetMusic(context, variant, out SfxEventMap.MusicEntry entry))
            {
                return null;
            }

            volume = entry.volume;
            if (entry.clip != null)
            {
                return entry.clip;
            }

            if (!_map.ProceduralMusic)
            {
                return null;
            }

            string cacheKey = context + "/" + SfxEventMap.Normalize(entry.variant);
            if (_proceduralCache.TryGetValue(cacheKey, out AudioClip clip) && clip != null)
            {
                Touch(cacheKey);
                return clip;
            }

            if (!_pendingBakes.ContainsKey(cacheKey))
            {
                float root = entry.rootFrequency;
                bool minor = entry.minor;
                float bpm = entry.bpm;
                _pendingBakes[cacheKey] = new PendingBake
                {
                    Name = "proc_music_" + cacheKey,
                    // GenerateMusicLoop is pure C# math (no Unity object access), safe off the main thread.
                    Samples = Task.Run(() => ProceduralSfxSynth.GenerateMusicLoop(root, minor, bpm, ProceduralSfxSynth.SampleRate)),
                };
            }

            bakingKey = cacheKey;
            return null;
        }

        private void Touch(string cacheKey)
        {
            int index = _cacheOrder.IndexOf(cacheKey);
            if (index >= 0 && index != _cacheOrder.Count - 1)
            {
                _cacheOrder.RemoveAt(index);
                _cacheOrder.Add(cacheKey);
            }
        }

        private void StoreInCache(string cacheKey, AudioClip clip)
        {
            if (_proceduralCache.TryGetValue(cacheKey, out AudioClip old) && old != null && old != clip)
            {
                DestroyClip(old);
            }

            _proceduralCache[cacheKey] = clip;
            _cacheOrder.Remove(cacheKey);
            _cacheOrder.Add(cacheKey);

            // Evict least recently used first, but never the clip the current context plays.
            int i = 0;
            while (_proceduralCache.Count > MaxCachedProceduralClips && i < _cacheOrder.Count)
            {
                string candidate = _cacheOrder[i];
                _proceduralCache.TryGetValue(candidate, out AudioClip evicted);
                if (evicted != null && evicted == CurrentClip)
                {
                    i++;
                    continue;
                }

                _cacheOrder.RemoveAt(i);
                _proceduralCache.Remove(candidate);
                DestroyClip(evicted);
            }
        }

        private static void DestroyClip(AudioClip clip)
        {
            if (clip == null) return;
            if (Application.isPlaying) UnityEngine.Object.Destroy(clip);
            else UnityEngine.Object.DestroyImmediate(clip);
        }

        private void OnFlowStateChanged(GameFlowStateChangedEvent evt)
        {
            switch (evt.Current)
            {
                case GameFlowState.MainMenu:
                case GameFlowState.CharacterSelect:
                    RequestBase(MusicContext.Title);
                    break;
                case GameFlowState.World:
                    RequestBase(MusicContext.WorldMap);
                    break;
                case GameFlowState.Encounter:
                    // EncounterStartedEvent (which carries the biome) normally arrives first.
                    if (_baseContext != MusicContext.Encounter && _baseContext != MusicContext.Boss)
                    {
                        RequestBase(MusicContext.Encounter);
                    }
                    break;
            }
        }

        private void OnEncounterStarted(EncounterStartedEvent evt)
        {
            EncounterPayload payload = evt.Payload;
            string biome = payload != null ? payload.biome.ToString() : null;
            RequestBase(HasBoss(payload) ? MusicContext.Boss : MusicContext.Encounter, biome);
        }

        private static bool HasBoss(EncounterPayload payload)
        {
            if (payload == null || payload.EnemyCount == 0)
            {
                return false;
            }

            foreach (EnemyDefinition enemy in payload.ResolveEnemies())
            {
                if (enemy != null && enemy.IsBoss)
                {
                    return true;
                }
            }

            return false;
        }
    }

    /// <summary>Per-frame pump for <see cref="MusicDirector.Tick"/> (story overlay poll, finished background bakes → AudioClips).</summary>
    internal class MusicDirectorDriver : MonoBehaviour
    {
        private MusicDirector _owner;

        public void Bind(MusicDirector owner) => _owner = owner;

        private void Update() => _owner?.Tick();
    }
}
