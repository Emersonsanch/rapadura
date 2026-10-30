using System.Collections.Generic;
using Rapadura.Audio;
using Rapadura.Core.Audio;
using Rapadura.Core.DI;

namespace Rapadura.Gameplay.Fishing
{
    /// <summary>
    /// The procedural sounds of "Pesca no açude", played by id through <see cref="SfxEventRouter.PlayCue"/> (no
    /// <see cref="SfxTrigger"/> of their own). The cues are added to the live <see cref="SfxEventMap"/> the first time
    /// the panel opens (<see cref="EnsureCues"/>, only ids the map does not have yet: a cue authored in
    /// <c>Resources/Audio/SfxEventMap.asset</c> — e.g. with a real clip — always wins), so the shared table needs no
    /// edit. Placeholders use existing <see cref="ProceduralSfxPreset"/>s; none is pausable, because the fishing panel
    /// itself holds the GameplayPause. List for the sound designer: <c>Docs/Audio.md</c>.
    /// </summary>
    public static class FishingAudio
    {
        public const string Cast = "fishing.cast";
        public const string Splash = "fishing.splash";
        public const string Nibble = "fishing.nibble";
        public const string Tremble = "fishing.tremble";
        public const string Bite = "fishing.bite";
        public const string Hook = "fishing.hook";
        public const string Reel = "fishing.reel";
        public const string Surge = "fishing.surge";
        public const string Snap = "fishing.snap";
        public const string Escape = "fishing.escape";
        public const string Catch = "fishing.catch";
        public const string CatchRare = "fishing.catch_rare";
        public const string Record = "fishing.record";

        private static SfxEventMap _ensuredMap;

        /// <summary>The default cue table (fresh instances on every call).</summary>
        public static List<SfxEventMap.Cue> BuildCues()
        {
            return new List<SfxEventMap.Cue>
            {
                Cue(Cast, ProceduralSfxPreset.Whoosh, 900f, 0.35f, 0.2f, 0.08f),
                Cue(Splash, ProceduralSfxPreset.BlipDown, 520f, 0.3f, 0.2f, 0.1f),
                Cue(Nibble, ProceduralSfxPreset.Click, 1800f, 0.22f, 0.15f, 0.12f),
                Cue(Tremble, ProceduralSfxPreset.Inhale, 1100f, 0.28f, 0.3f, 0.05f),
                Cue(Bite, ProceduralSfxPreset.Blip, 1250f, 0.5f, 0.2f, 0f),
                Cue(Hook, ProceduralSfxPreset.Zap, 700f, 0.4f, 0.2f, 0.06f),
                Cue(Reel, ProceduralSfxPreset.Click, 2400f, 0.12f, 0.09f, 0.1f),
                Cue(Surge, ProceduralSfxPreset.WoodCreak, 300f, 0.32f, 0.4f, 0.1f),
                Cue(Snap, ProceduralSfxPreset.Crunch, 1500f, 0.42f, 0.3f, 0.05f),
                Cue(Escape, ProceduralSfxPreset.BlipDown, 400f, 0.35f, 0.3f, 0.05f),
                Cue(Catch, ProceduralSfxPreset.Chime, 880f, 0.45f, 0.3f, 0.04f),
                Cue(CatchRare, ProceduralSfxPreset.Fanfare, 660f, 0.5f, 0.5f, 0f),
                Cue(Record, ProceduralSfxPreset.Sparkle, 1500f, 0.4f, 0.5f, 0f),
            };
        }

        /// <summary>Adds the fishing cues <paramref name="map"/> lacks. Returns how many were added.</summary>
        public static int EnsureCues(SfxEventMap map)
        {
            if (map == null)
            {
                return 0;
            }

            int added = 0;
            foreach (SfxEventMap.Cue cue in BuildCues())
            {
                if (!map.TryGetCue(cue.id, out _))
                {
                    map.AddCue(cue);
                    added++;
                }
            }

            return added;
        }

        /// <summary>Plays <paramref name="cueId"/> on the live router (no-op without one).</summary>
        public static void Play(string cueId)
        {
            if (!ServiceLocator.TryGet(out SfxEventRouter router) || router == null || !router.IsInitialized)
            {
                return;
            }

            // Once per map instance (the reel tick plays every frame while held).
            if (!ReferenceEquals(_ensuredMap, router.Map))
            {
                EnsureCues(router.Map);
                _ensuredMap = router.Map;
            }

            router.PlayCue(cueId);
        }

        /// <summary>The cue of a session cue, or null for those that stay silent.</summary>
        public static string CueFor(FishingCue cue, FishSpec fish)
        {
            switch (cue)
            {
                case FishingCue.Cast: return Cast;
                case FishingCue.Splash: return Splash;
                case FishingCue.Nibble: return Nibble;
                case FishingCue.Tremble: return Tremble;
                case FishingCue.Bite: return Bite;
                case FishingCue.Hooked: return Hook;
                case FishingCue.Surge: return Surge;
                case FishingCue.Snapped: return Snap;
                case FishingCue.Escaped:
                case FishingCue.TooEarly:
                case FishingCue.TooLate:
                    return Escape;
                case FishingCue.Landed:
                    return fish != null && fish.Rarity >= FishRarity.Rare ? CatchRare : Catch;
                default:
                    return null;
            }
        }

        private static SfxEventMap.Cue Cue(string id, ProceduralSfxPreset preset, float frequency, float volume, float cooldown, float pitchJitter)
        {
            return new SfxEventMap.Cue
            {
                id = id,
                category = AudioCategory.Sfx,
                volume = volume,
                cooldown = cooldown,
                maxVoices = 1,
                pausable = false,
                spatial = false,
                loop = false,
                ducksMusic = false,
                preset = preset,
                frequency = frequency,
                pitchJitter = pitchJitter,
                volumeJitter = 0.1f,
            };
        }
    }
}
