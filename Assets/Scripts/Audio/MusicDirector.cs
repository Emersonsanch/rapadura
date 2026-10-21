using System;
using System.Collections.Generic;
using Rapadura.Core.Audio;
using Rapadura.Core.Events;
using Rapadura.Core.Interfaces;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Flow;
using Rapadura.Gameplay.World.Map;
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
    /// "procedural music" flag is on, a 16-beat drone + arpeggio loop is baked once per context by
    /// <see cref="ProceduralSfxSynth.GenerateMusicLoop"/> (~10 s, a few ms of CPU, then plain clip
    /// playback). Turning that flag off gives silence with the same hooks — assigning real clips in
    /// the asset is the only change a composer needs (Docs/Audio.md).
    /// </summary>
    public class MusicDirector : IManager
    {
        private readonly AudioManager _audio;
        private readonly Dictionary<string, AudioClip> _proceduralCache = new Dictionary<string, AudioClip>(StringComparer.Ordinal);
        private SfxEventMap _map;

        public bool IsInitialized { get; private set; }

        public MusicContext CurrentContext { get; private set; } = MusicContext.None;
        public string CurrentVariant { get; private set; } = string.Empty;
        public AudioClip CurrentClip { get; private set; }

        public MusicDirector(AudioManager audio, SfxEventMap map = null)
        {
            _audio = audio;
            _map = map;
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
                if (clip == null) continue;
                if (Application.isPlaying) UnityEngine.Object.Destroy(clip);
                else UnityEngine.Object.DestroyImmediate(clip);
            }

            _proceduralCache.Clear();
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
            CurrentClip = ResolveClip(context, key, out float volume);

            if (_audio != null)
            {
                float fade = _map != null ? _map.MusicCrossfadeSeconds : 1.5f;
                if (CurrentClip != null) _audio.PlayMusic(CurrentClip, fade, volume);
                else _audio.StopMusic(fade);
            }

            return true;
        }

        public AudioClip ResolveClip(MusicContext context, string variant, out float volume)
        {
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
            if (!_proceduralCache.TryGetValue(cacheKey, out AudioClip clip) || clip == null)
            {
                clip = ProceduralSfxSynth.CreateMusicClip("proc_music_" + cacheKey, entry.rootFrequency, entry.minor, entry.bpm);
                _proceduralCache[cacheKey] = clip;
            }

            return clip;
        }

        private void OnFlowStateChanged(GameFlowStateChangedEvent evt)
        {
            switch (evt.Current)
            {
                case GameFlowState.MainMenu:
                case GameFlowState.CharacterSelect:
                    SetContext(MusicContext.Title);
                    break;
                case GameFlowState.World:
                    SetContext(MusicContext.WorldMap);
                    break;
                case GameFlowState.Encounter:
                    // EncounterStartedEvent (which carries the biome) normally arrives first.
                    if (CurrentContext != MusicContext.Encounter && CurrentContext != MusicContext.Boss)
                    {
                        SetContext(MusicContext.Encounter);
                    }
                    break;
            }
        }

        private void OnEncounterStarted(EncounterStartedEvent evt)
        {
            EncounterPayload payload = evt.Payload;
            string biome = payload != null ? payload.biome.ToString() : null;
            SetContext(HasBoss(payload) ? MusicContext.Boss : MusicContext.Encounter, biome);
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
}
