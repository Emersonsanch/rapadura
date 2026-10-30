using System.Collections.Generic;
using Rapadura.Audio;
using Rapadura.Core.Audio;
using Rapadura.Core.DI;
using UnityEngine;

namespace Rapadura.Gameplay.Mounts
{
    /// <summary>
    /// Procedural sounds of the mounts, played by id through <see cref="SfxEventRouter.PlayCue"/>. Like
    /// <c>FishingAudio</c>, the cues are added to the live <see cref="SfxEventMap"/> on first use (<see cref="EnsureCues"/>,
    /// only ids the map lacks, so a cue authored in <c>Resources/Audio/SfxEventMap.asset</c> with a real recording always
    /// wins) and the shared synth is not edited: the three new placeholders (whistle, hooves, cowbell, plus the bray)
    /// are synthesised here (<see cref="GenerateWhistle"/>...) and handed to the cue as its clip. List for the sound
    /// designer: Docs/Audio.md "Montarias".
    /// </summary>
    public static class MountAudio
    {
        public const string Whistle = "mount.whistle";
        public const string HoofStep = "mount.hoof";
        public const string PawStep = "mount.paw";
        public const string Cowbell = "mount.cowbell";
        public const string AbilityReady = "mount.ability_ready";
        public const string MountUp = "mount.mount";
        public const string Dismount = "mount.dismount";
        public const string Dash = "mount.dash";
        public const string Sniff = "mount.sniff";
        public const string Bray = "mount.bray";
        public const string Refused = "mount.refused";

        public const int SampleRate = 22050;

        private static readonly Dictionary<string, AudioClip> Clips = new Dictionary<string, AudioClip>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Clips.Clear();

        /// <summary>The cue table (fresh instances; clips of the custom synths are attached when <paramref name="withClips"/>).</summary>
        public static List<SfxEventMap.Cue> BuildCues(bool withClips = true)
        {
            return new List<SfxEventMap.Cue>
            {
                Cue(Whistle, ProceduralSfxPreset.Chirp, 1800f, 0.45f, 0.4f, 0.04f, withClips ? ClipFor(Whistle, GenerateWhistle) : null),
                Cue(HoofStep, ProceduralSfxPreset.WoodKnock, 420f, 0.16f, 0.06f, 0.12f, withClips ? ClipFor(HoofStep, GenerateHoof) : null, voices: 3, spatial: true),
                Cue(PawStep, ProceduralSfxPreset.SackThump, 260f, 0.1f, 0.06f, 0.15f, null, voices: 3, spatial: true),
                Cue(Cowbell, ProceduralSfxPreset.Clink, 620f, 0.3f, 0.5f, 0.06f, withClips ? ClipFor(Cowbell, GenerateCowbell) : null),
                Cue(AbilityReady, ProceduralSfxPreset.Clink, 620f, 0.28f, 1f, 0.03f, withClips ? ClipFor(Cowbell, GenerateCowbell) : null),
                Cue(MountUp, ProceduralSfxPreset.SackThump, 220f, 0.35f, 0.3f, 0.05f, null),
                Cue(Dismount, ProceduralSfxPreset.Thud, 260f, 0.3f, 0.3f, 0.05f, null),
                Cue(Dash, ProceduralSfxPreset.Whoosh, 700f, 0.4f, 0.3f, 0.06f, null),
                Cue(Sniff, ProceduralSfxPreset.Inhale, 1300f, 0.32f, 0.5f, 0.1f, null),
                Cue(Bray, ProceduralSfxPreset.Buzz, 400f, 0.5f, 1f, 0.05f, withClips ? ClipFor(Bray, GenerateBray) : null),
                Cue(Refused, ProceduralSfxPreset.BlipDown, 380f, 0.25f, 0.4f, 0f, null),
            };
        }

        /// <summary>Adds the mount cues <paramref name="map"/> lacks; returns how many.</summary>
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
        public static void Play(string cueId, Vector3? position = null)
        {
            if (string.IsNullOrEmpty(cueId) || !ServiceLocator.TryGet(out SfxEventRouter router) || router == null || !router.IsInitialized)
            {
                return;
            }

            EnsureCues(router.Map);
            router.PlayCue(cueId, position);
        }

        /// <summary>Step cue of a recipe: hooves for the pony and the donkey, soft paws for the dog.</summary>
        public static string StepCueFor(MountVisualRecipe recipe)
        {
            switch (recipe)
            {
                // Soft feet: dogs, cats, the capybara, the anteater, the carbuncle, the rheas, Boitatá and the tortoise.
                case MountVisualRecipe.BigDog:
                case MountVisualRecipe.Capybara:
                case MountVisualRecipe.ManedWolf:
                case MountVisualRecipe.Anteater:
                case MountVisualRecipe.SnowTiger:
                case MountVisualRecipe.LightJaguar:
                case MountVisualRecipe.Carbuncle:
                case MountVisualRecipe.Rhea:
                case MountVisualRecipe.FireRhea:
                case MountVisualRecipe.Boitata:
                case MountVisualRecipe.ElderTortoise:
                    return PawStep;
                default:
                    return HoofStep;
            }
        }

        // ------------------------------------------------------------------
        // Synths (pure, EditMode-tested)
        // ------------------------------------------------------------------

        /// <summary>"Fiu-fiuuu": a short rising whistle, a gap, then a longer one that rises and falls, with a little vibrato and breath.</summary>
        public static float[] GenerateWhistle()
        {
            float[] s = Buffer(0.72f);
            var rng = new System.Random(7);
            float phase = 0f;
            for (int i = 0; i < s.Length; i++)
            {
                float t = (float)i / SampleRate;
                float f;
                float env;
                if (t < 0.2f)
                {
                    float u = t / 0.2f;
                    f = Mathf.Lerp(1500f, 2300f, u);
                    env = Mathf.Sin(Mathf.PI * u);
                }
                else if (t < 0.28f)
                {
                    f = 2000f;
                    env = 0f;
                }
                else
                {
                    float u = (t - 0.28f) / 0.44f;
                    f = u < 0.45f ? Mathf.Lerp(1700f, 2700f, u / 0.45f) : Mathf.Lerp(2700f, 2050f, (u - 0.45f) / 0.55f);
                    env = Mathf.Clamp01(u * 12f) * Mathf.Clamp01((1f - u) * 5f);
                }

                f *= 1f + 0.012f * Mathf.Sin(2f * Mathf.PI * 6.5f * t);
                phase += 2f * Mathf.PI * f / SampleRate;
                float breath = (float)(rng.NextDouble() * 2.0 - 1.0) * 0.06f;
                s[i] = env * (Mathf.Sin(phase) * 0.9f + breath);
            }

            Normalize(s, 0.8f);
            return s;
        }

        /// <summary>A "clop": a hollow low knock (two damped partials) plus a click of filtered noise.</summary>
        public static float[] GenerateHoof()
        {
            float[] s = Buffer(0.12f);
            var rng = new System.Random(11);
            float low = 0f;
            for (int i = 0; i < s.Length; i++)
            {
                float t = (float)i / SampleRate;
                float body = Mathf.Sin(2f * Mathf.PI * 520f * t) * Mathf.Exp(-t * 55f) + 0.6f * Mathf.Sin(2f * Mathf.PI * 880f * t) * Mathf.Exp(-t * 80f);
                float noise = (float)(rng.NextDouble() * 2.0 - 1.0);
                low += 0.35f * (noise - low);
                float click = low * Mathf.Exp(-t * 160f);
                s[i] = body * 0.8f + click * 0.6f;
            }

            Normalize(s, 0.75f);
            return s;
        }

        /// <summary>Chocalho: a dull metallic "tonk" of inharmonic partials (cowbell), ringing ~0.5 s.</summary>
        public static float[] GenerateCowbell()
        {
            float[] s = Buffer(0.55f);
            for (int i = 0; i < s.Length; i++)
            {
                float t = (float)i / SampleRate;
                float a = Square(562f, t) * 0.5f + Square(845f, t) * 0.35f;
                float ring = Mathf.Sin(2f * Mathf.PI * 1310f * t) * 0.2f * Mathf.Exp(-t * 14f);
                float env = Mathf.Clamp01(t * 600f) * Mathf.Exp(-t * 7.5f);
                s[i] = (a * env + ring) * 0.7f;
            }

            Smooth(s, 0.45f);
            Normalize(s, 0.7f);
            return s;
        }

        /// <summary>Zurro: "ii-óó, ii-óó", a reedy sawtooth alternating high and low with a wobble (playful, not scary).</summary>
        public static float[] GenerateBray()
        {
            float[] s = Buffer(1.0f);
            float phase = 0f;
            for (int i = 0; i < s.Length; i++)
            {
                float t = (float)i / SampleRate;
                float cycle = t / 0.25f;
                int part = Mathf.FloorToInt(cycle);
                float u = cycle - part;
                bool high = part % 2 == 0;
                float f = (high ? 720f : 300f) * (1f + 0.04f * Mathf.Sin(2f * Mathf.PI * 9f * t)) * (high ? Mathf.Lerp(0.9f, 1.05f, u) : Mathf.Lerp(1.1f, 0.92f, u));
                phase += f / SampleRate;
                float saw = 2f * (phase - Mathf.Floor(phase + 0.5f));
                float env = Mathf.Sin(Mathf.PI * u) * Mathf.Clamp01((1f - t) * 4f);
                s[i] = saw * env * (high ? 0.75f : 1f);
            }

            Smooth(s, 0.3f);
            Normalize(s, 0.75f);
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

        private static SfxEventMap.Cue Cue(string id, ProceduralSfxPreset preset, float frequency, float volume, float cooldown, float pitchJitter,
            AudioClip clip, int voices = 1, bool spatial = false)
        {
            return new SfxEventMap.Cue
            {
                id = id,
                clip = clip,
                category = AudioCategory.Sfx,
                volume = volume,
                cooldown = cooldown,
                maxVoices = voices,
                pausable = true,
                spatial = spatial,
                loop = false,
                ducksMusic = false,
                preset = preset,
                frequency = frequency,
                pitchJitter = pitchJitter,
                volumeJitter = 0.1f,
            };
        }

        private static float[] Buffer(float seconds) => new float[Mathf.Max(1, Mathf.RoundToInt(seconds * SampleRate))];

        private static float Square(float f, float t) => Mathf.Sin(2f * Mathf.PI * f * t) >= 0f ? 1f : -1f;

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
