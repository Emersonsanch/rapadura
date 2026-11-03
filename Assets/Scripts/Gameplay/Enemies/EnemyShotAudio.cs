using System.Collections.Generic;
using Rapadura.Audio;
using Rapadura.Core.Audio;
using Rapadura.Core.DI;
using UnityEngine;

namespace Rapadura.Gameplay.Enemies
{
    /// <summary>
    /// The shot sounds of the [distância] enemies, one per <see cref="EnemyProjectileKind"/> (they used to share the ossuary's
    /// <c>dungeon.arrow</c> whoosh): the Abutre's spat ember (a puff and a crackling hiss), the Lanterna Quebrada's bolt of
    /// light (a bright falling zap with a shimmer), the Gavião's feather dart (a short airy flutter) and the Peixe-Lanterna's
    /// glob of cold light (a rising "bloop", a water drop). Like <c>MountAudio</c>/<c>DungeonFx</c>, the cues are added to
    /// the live <see cref="SfxEventMap"/> on first use (<see cref="EnsureCues"/>, only ids the map lacks, so a real recording
    /// authored in <c>Resources/Audio/SfxEventMap.asset</c> always wins) and the shared synth is not edited: each clip is
    /// synthesised here and handed to its cue (the preset is only the fallback). List for the sound designer: Docs/Audio.md.
    /// </summary>
    public static class EnemyShotAudio
    {
        public const string EmberCue = "enemy.shot_ember";
        public const string LanternBoltCue = "enemy.shot_light";
        public const string FeatherCue = "enemy.shot_feather";
        public const string LureGlobCue = "enemy.shot_glob";

        public const int SampleRate = 22050;

        public static readonly string[] AllCues = { EmberCue, LanternBoltCue, FeatherCue, LureGlobCue };

        private static readonly Dictionary<string, AudioClip> Clips = new Dictionary<string, AudioClip>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Clips.Clear();

        /// <summary>The cue of a <paramref name="kind"/> shot; null for <see cref="EnemyProjectileKind.None"/> (a swing).</summary>
        public static string CueOf(EnemyProjectileKind kind)
        {
            switch (kind)
            {
                case EnemyProjectileKind.Ember: return EmberCue;
                case EnemyProjectileKind.LanternBolt: return LanternBoltCue;
                case EnemyProjectileKind.Feather: return FeatherCue;
                case EnemyProjectileKind.LureGlob: return LureGlobCue;
                default: return null;
            }
        }

        /// <summary>The cue table (fresh instances; the synthesised clips are attached when <paramref name="withClips"/>).</summary>
        public static List<SfxEventMap.Cue> BuildCues(bool withClips = true)
        {
            return new List<SfxEventMap.Cue>
            {
                Cue(EmberCue, ProceduralSfxPreset.Whoosh, 520f, 0.4f, withClips ? ClipFor(EmberCue, GenerateEmber) : null),
                Cue(LanternBoltCue, ProceduralSfxPreset.Zap, 1400f, 0.36f, withClips ? ClipFor(LanternBoltCue, GenerateLanternBolt) : null),
                Cue(FeatherCue, ProceduralSfxPreset.Whoosh, 1500f, 0.34f, withClips ? ClipFor(FeatherCue, GenerateFeather) : null),
                Cue(LureGlobCue, ProceduralSfxPreset.Blip, 420f, 0.38f, withClips ? ClipFor(LureGlobCue, GenerateLureGlob) : null),
            };
        }

        /// <summary>Adds the shot cues <paramref name="map"/> lacks; returns how many.</summary>
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

        /// <summary>Plays the <paramref name="kind"/> shot at <paramref name="position"/> (silent without a router, e.g. in EditMode).</summary>
        public static void Play(EnemyProjectileKind kind, Vector3 position)
        {
            string cueId = CueOf(kind);
            if (cueId == null || !Application.isPlaying || !ServiceLocator.TryGet(out SfxEventRouter router) || router == null || !router.IsInitialized)
            {
                return;
            }

            EnsureCues(router.Map);
            router.PlayCue(cueId, position);
        }

        // ------------------------------------------------------------------
        // Synths (pure, EditMode-tested)
        // ------------------------------------------------------------------

        /// <summary>Brasa: a low "puff" as it is spat, then a hiss of band-passed noise that fades with a few sharp crackles.</summary>
        public static float[] GenerateEmber()
        {
            float[] s = Buffer(0.38f);
            var rng = new System.Random(23);
            float low = 0f;
            float band = 0f;
            for (int i = 0; i < s.Length; i++)
            {
                float t = (float)i / SampleRate;
                float noise = (float)(rng.NextDouble() * 2.0 - 1.0);
                low += 0.25f * (noise - low);
                band += 0.6f * (low - band);
                float hiss = (low - band) * 2.2f * Mathf.Clamp01(t * 60f) * Mathf.Exp(-t * 7f);
                float puff = Mathf.Sin(2f * Mathf.PI * Mathf.Lerp(180f, 90f, t / 0.38f) * t) * Mathf.Exp(-t * 28f);
                float crackle = rng.NextDouble() < 0.004 ? (float)(rng.NextDouble() * 2.0 - 1.0) * 1.4f : 0f;
                s[i] = hiss + puff * 0.7f + crackle * Mathf.Exp(-t * 4f);
            }

            Smooth(s, 0.7f);
            Normalize(s, 0.75f);
            return s;
        }

        /// <summary>Raio de luz: a glassy zap falling 2400 → 700 Hz with a fifth above and a fast tremolo shimmer.</summary>
        public static float[] GenerateLanternBolt()
        {
            float[] s = Buffer(0.3f);
            float phase = 0f;
            for (int i = 0; i < s.Length; i++)
            {
                float t = (float)i / SampleRate;
                float u = t / 0.3f;
                float f = Mathf.Lerp(2400f, 700f, Mathf.Sqrt(u));
                phase += 2f * Mathf.PI * f / SampleRate;
                float tone = Mathf.Sin(phase) * 0.7f + Mathf.Sin(phase * 1.5f) * 0.3f;
                float shimmer = 0.75f + 0.25f * Mathf.Sin(2f * Mathf.PI * 38f * t);
                float env = Mathf.Clamp01(t * 400f) * Mathf.Exp(-t * 9f);
                s[i] = tone * shimmer * env;
            }

            Normalize(s, 0.7f);
            return s;
        }

        /// <summary>Pena: a short airy "fft", high thin noise with a wing-like flutter, quick in and out.</summary>
        public static float[] GenerateFeather()
        {
            float[] s = Buffer(0.18f);
            var rng = new System.Random(31);
            float low = 0f;
            for (int i = 0; i < s.Length; i++)
            {
                float t = (float)i / SampleRate;
                float u = t / 0.18f;
                float noise = (float)(rng.NextDouble() * 2.0 - 1.0);
                low += 0.5f * (noise - low);
                float high = noise - low;
                float flutter = 0.6f + 0.4f * Mathf.Sin(2f * Mathf.PI * 46f * t);
                float env = Mathf.Sin(Mathf.PI * Mathf.Pow(u, 0.6f));
                s[i] = high * flutter * env;
            }

            Normalize(s, 0.65f);
            return s;
        }

        /// <summary>Gota: a liquid "bloop", a sine that rises fast (300 → 1100 Hz, like a drop) with a soft plop under it.</summary>
        public static float[] GenerateLureGlob()
        {
            float[] s = Buffer(0.24f);
            float phase = 0f;
            for (int i = 0; i < s.Length; i++)
            {
                float t = (float)i / SampleRate;
                float u = t / 0.24f;
                float f = 300f * Mathf.Pow(1100f / 300f, Mathf.Clamp01(u * 1.6f));
                phase += 2f * Mathf.PI * f / SampleRate;
                float env = Mathf.Clamp01(t * 300f) * Mathf.Exp(-t * 14f);
                float plop = Mathf.Sin(2f * Mathf.PI * 140f * t) * Mathf.Exp(-t * 45f);
                s[i] = Mathf.Sin(phase) * env + plop * 0.5f;
            }

            Normalize(s, 0.72f);
            return s;
        }

        // ------------------------------------------------------------------

        private static AudioClip ClipFor(string id, System.Func<float[]> synth)
        {
            if (!Clips.TryGetValue(id, out AudioClip clip) || clip == null)
            {
                clip = ProceduralSfxSynth.ToClip("proc_" + id, synth(), SampleRate);
                Clips[id] = clip;
            }

            return clip;
        }

        private static SfxEventMap.Cue Cue(string id, ProceduralSfxPreset preset, float frequency, float volume, AudioClip clip)
        {
            return new SfxEventMap.Cue
            {
                id = id,
                clip = clip,
                category = AudioCategory.Sfx,
                volume = volume,
                cooldown = 0.12f,
                maxVoices = 3,
                pausable = true,
                spatial = true,
                loop = false,
                ducksMusic = false,
                preset = preset,
                frequency = frequency,
                pitchJitter = 0.06f,
                volumeJitter = 0.1f,
            };
        }

        private static float[] Buffer(float seconds) => new float[Mathf.Max(1, Mathf.RoundToInt(seconds * SampleRate))];

        private static void Smooth(float[] s, float k)
        {
            float y = 0f;
            for (int i = 0; i < s.Length; i++)
            {
                y += k * (s[i] - y);
                s[i] = y;
            }
        }

        private static void Normalize(float[] s, float peak)
        {
            float max = 0f;
            foreach (float v in s)
            {
                max = Mathf.Max(max, Mathf.Abs(v));
            }

            if (max <= 1e-6f)
            {
                return;
            }

            float k = peak / max;
            for (int i = 0; i < s.Length; i++)
            {
                s[i] *= k;
            }
        }
    }
}
