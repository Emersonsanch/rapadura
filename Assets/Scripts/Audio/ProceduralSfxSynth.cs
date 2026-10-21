using System;
using UnityEngine;

namespace Rapadura.Audio
{
    /// <summary>
    /// Bakes tiny placeholder sounds into <see cref="AudioClip"/>s (AudioClip.Create + SetData) so the
    /// game has audio before real assets exist. Everything is generated once per cue and cached by the
    /// caller — there is no per-frame synthesis, so the runtime cost is a few milliseconds the first
    /// time a cue plays and ~20-60 KB per one-shot (22.05 kHz mono). Deterministic: the same preset and
    /// frequency always produce the same samples (seeded noise), which keeps tests stable.
    /// </summary>
    public static class ProceduralSfxSynth
    {
        public const int SampleRate = 22050;
        private const float Peak = 0.8f;
        private const float TwoPi = Mathf.PI * 2f;

        public static bool IsLoopPreset(ProceduralSfxPreset preset) => preset == ProceduralSfxPreset.RainLoop || preset == ProceduralSfxPreset.WindLoop;

        public static AudioClip CreateClip(string name, ProceduralSfxPreset preset, float frequency)
        {
            float[] samples = Generate(preset, frequency, SampleRate);
            return samples.Length == 0 ? null : ToClip(name, samples, SampleRate);
        }

        public static AudioClip CreateMusicClip(string name, float rootFrequency, bool minor, float bpm)
        {
            return ToClip(name, GenerateMusicLoop(rootFrequency, minor, bpm, SampleRate), SampleRate);
        }

        public static AudioClip ToClip(string name, float[] samples, int sampleRate)
        {
            AudioClip clip = AudioClip.Create(string.IsNullOrEmpty(name) ? "procedural" : name, Mathf.Max(1, samples.Length), 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        /// <summary>Raw mono samples in [-Peak, Peak]. Empty for <see cref="ProceduralSfxPreset.None"/>.</summary>
        public static float[] Generate(ProceduralSfxPreset preset, float frequency, int sampleRate = SampleRate)
        {
            float f = Mathf.Max(20f, frequency);
            var rng = new System.Random(((int)preset * 7919) ^ Mathf.RoundToInt(f * 10f));
            float[] s;

            switch (preset)
            {
                case ProceduralSfxPreset.Click:
                    s = Buffer(0.03f, sampleRate);
                    for (int i = 0; i < s.Length; i++) { float t = T(i, sampleRate); s[i] = (Sin(f, t) + Noise(rng) * 0.2f) * Mathf.Exp(-t * 180f); }
                    break;
                case ProceduralSfxPreset.Blip:
                case ProceduralSfxPreset.BlipDown:
                {
                    s = Buffer(0.09f, sampleRate);
                    float end = preset == ProceduralSfxPreset.Blip ? f * 1.5f : f * 0.66f;
                    Sweep(s, sampleRate, f, end, 0.8f, 0f);
                    for (int i = 0; i < s.Length; i++) s[i] *= 1f - (float)i / s.Length;
                    break;
                }
                case ProceduralSfxPreset.Thud:
                    s = Buffer(0.14f, sampleRate);
                    Sweep(s, sampleRate, f, f * 0.45f, 1f, 0f);
                    for (int i = 0; i < s.Length; i++) { float t = T(i, sampleRate); s[i] = (s[i] + Noise(rng) * 0.35f * Mathf.Exp(-t * 60f)) * Mathf.Exp(-t * 28f); }
                    break;
                case ProceduralSfxPreset.Crunch:
                {
                    s = Buffer(0.2f, sampleRate);
                    float lp = 0f, a = Coef(f * 6f, sampleRate);
                    for (int i = 0; i < s.Length; i++) { float t = T(i, sampleRate); lp += a * (Noise(rng) - lp); s[i] = (lp * 1.6f + Sin(f, t) * 0.5f) * Mathf.Exp(-t * 16f); }
                    break;
                }
                case ProceduralSfxPreset.Zap:
                    s = Buffer(0.16f, sampleRate);
                    Sweep(s, sampleRate, f, f * 0.25f, 0.7f, 1f);
                    for (int i = 0; i < s.Length; i++) s[i] *= Mathf.Exp(-T(i, sampleRate) * 14f);
                    break;
                case ProceduralSfxPreset.Buzz:
                    s = Buffer(0.2f, sampleRate);
                    for (int i = 0; i < s.Length; i++) { float t = T(i, sampleRate); s[i] = Square(f, t) * 0.45f * (t < 0.08f || t > 0.11f ? 1f : 0f) * (1f - t / 0.2f); }
                    break;
                case ProceduralSfxPreset.Whoosh:
                {
                    s = Buffer(0.32f, sampleRate);
                    float lp = 0f;
                    for (int i = 0; i < s.Length; i++)
                    {
                        float p = (float)i / s.Length;
                        lp += Coef(f * (0.5f + 2.5f * Mathf.Sin(Mathf.PI * p)), sampleRate) * (Noise(rng) - lp);
                        s[i] = lp * 2.2f * Mathf.Sin(Mathf.PI * p);
                    }
                    break;
                }
                case ProceduralSfxPreset.Chime:
                    s = Buffer(0.7f, sampleRate);
                    for (int i = 0; i < s.Length; i++) { float t = T(i, sampleRate); s[i] = (Sin(f, t) + 0.4f * Sin(f * 2.76f, t) + 0.15f * Sin(f * 5.4f, t)) * Mathf.Exp(-t * 6f); }
                    break;
                case ProceduralSfxPreset.ChimeDown:
                    s = Notes(sampleRate, 0.3f, 0.5f, f, f * 0.8409f, f * 0.6674f);
                    break;
                case ProceduralSfxPreset.Fanfare:
                    s = Notes(sampleRate, 0.12f, 0.55f, f, f * 1.2599f, f * 1.4983f, f * 2f);
                    break;
                case ProceduralSfxPreset.Sparkle:
                {
                    var notes = new float[6];
                    for (int n = 0; n < notes.Length; n++) notes[n] = f * (1f + (float)rng.NextDouble() * 1.5f);
                    s = Notes(sampleRate, 0.06f, 0.25f, notes);
                    break;
                }
                case ProceduralSfxPreset.Boom:
                {
                    s = Buffer(0.8f, sampleRate);
                    Sweep(s, sampleRate, f, f * 0.4f, 1f, 0f);
                    float lp = 0f, a = Coef(400f, sampleRate);
                    for (int i = 0; i < s.Length; i++) { float t = T(i, sampleRate); lp += a * (Noise(rng) - lp); s[i] = (s[i] + lp * 2f) * Mathf.Exp(-t * 5f); }
                    break;
                }
                case ProceduralSfxPreset.Swell:
                    s = Buffer(0.9f, sampleRate);
                    for (int i = 0; i < s.Length; i++) { float t = T(i, sampleRate); s[i] = (Sin(f, t) + 0.6f * Sin(f * 1.5f, t)) * Mathf.Sin(Mathf.PI * i / s.Length); }
                    break;
                case ProceduralSfxPreset.Gong:
                    s = Buffer(1.3f, sampleRate);
                    for (int i = 0; i < s.Length; i++) { float t = T(i, sampleRate); s[i] = (Sin(f, t) + 0.6f * Sin(f * 1.47f, t) + 0.4f * Sin(f * 2.09f, t) + 0.2f * Sin(f * 2.56f, t)) * Mathf.Exp(-t * 3f) * Mathf.Min(1f, t * 200f); }
                    break;
                case ProceduralSfxPreset.Chirp:
                {
                    s = Buffer(0.36f, sampleRate);
                    float phase = 0f;
                    for (int i = 0; i < s.Length; i++)
                    {
                        float t = T(i, sampleRate), local = (t % 0.12f) / 0.07f;
                        float on = local <= 1f ? Mathf.Sin(Mathf.PI * local) : 0f;
                        phase += TwoPi * f * (1f + 0.3f * Mathf.Min(1f, local)) / sampleRate;
                        s[i] = Mathf.Sin(phase) * on;
                    }
                    break;
                }
                case ProceduralSfxPreset.Cricket:
                    s = Buffer(0.6f, sampleRate);
                    for (int i = 0; i < s.Length; i++) { float t = T(i, sampleRate); float burst = (t < 0.22f || (t > 0.32f && t < 0.54f)) ? 1f : 0f; s[i] = Sin(f, t) * (Sin(30f, t) > 0f ? 1f : 0f) * burst * 0.6f; }
                    break;
                case ProceduralSfxPreset.RainLoop:
                {
                    s = Buffer(2.2f, sampleRate);
                    float lp = 0f, a = Coef(f, sampleRate);
                    for (int i = 0; i < s.Length; i++) { lp += a * (Noise(rng) - lp); s[i] = lp * 1.5f; }
                    int drops = 40;
                    for (int d = 0; d < drops; d++)
                    {
                        int start = rng.Next(s.Length);
                        float df = 1500f + (float)rng.NextDouble() * 2500f;
                        for (int k = 0; k < sampleRate / 50; k++) { float t = T(k, sampleRate); s[(start + k) % s.Length] += Sin(df, t) * Mathf.Exp(-t * 300f) * 0.25f; }
                    }
                    MakeLoop(ref s, sampleRate / 20);
                    break;
                }
                case ProceduralSfxPreset.WindLoop:
                {
                    s = Buffer(3.2f, sampleRate);
                    float lp = 0f, lp2 = 0f;
                    for (int i = 0; i < s.Length; i++)
                    {
                        float p = (float)i / s.Length;
                        float gust = 0.6f + 0.4f * Mathf.Sin(TwoPi * 2f * p) * Mathf.Sin(TwoPi * 1f * p + 1f);
                        float a = Coef(f * (0.5f + gust), sampleRate);
                        lp += a * (Noise(rng) - lp);
                        lp2 += a * (lp - lp2);
                        s[i] = lp2 * 4f * gust;
                    }
                    MakeLoop(ref s, sampleRate / 10);
                    break;
                }
                default:
                    return Array.Empty<float>();
            }

            Finish(s, sampleRate, !IsLoopPreset(preset));
            return s;
        }

        /// <summary>
        /// A seamless 16-beat loop: a slow-breathing drone (root an octave down + fifth) under an
        /// eighth-note plucked arpeggio over a I-IV-V-I (major) or i-VI-iv-v (minor) progression.
        /// Drone frequencies are quantized to whole cycles per loop and arpeggio tails wrap around, so
        /// the loop point is click-free. ~10 s at 22.05 kHz ≈ 220k samples, baked once per context.
        /// </summary>
        public static float[] GenerateMusicLoop(float rootFrequency, bool minor, float bpm, int sampleRate = SampleRate)
        {
            const int beats = 16;
            float root = Mathf.Max(40f, rootFrequency);
            float beat = 60f / Mathf.Clamp(bpm, 40f, 200f);
            float length = beats * beat;
            int count = Mathf.RoundToInt(length * sampleRate);
            var s = new float[count];

            float droneA = Quantize(root * 0.5f, length), droneB = Quantize(root * 0.75f, length);
            float lfo = 2f / length;
            for (int i = 0; i < count; i++)
            {
                float t = (float)i / sampleRate;
                float breath = 0.75f + 0.25f * Mathf.Sin(TwoPi * lfo * t);
                s[i] = (Mathf.Sin(TwoPi * droneA * t) * 0.18f + Mathf.Sin(TwoPi * droneB * t) * 0.1f) * breath;
            }

            int[] chord = minor ? new[] { 0, 3, 7, 12 } : new[] { 0, 4, 7, 12 };
            int[] progression = minor ? new[] { 0, 8, 5, 7 } : new[] { 0, 5, 7, 0 };
            int[] order = { 0, 1, 2, 3, 2, 1, 0, 1 };
            int steps = beats * 2;
            int stepSamples = count / steps;
            int tail = Mathf.Min(count, Mathf.RoundToInt(0.6f * sampleRate));

            for (int step = 0; step < steps; step++)
            {
                int bar = step / 8;
                float freq = root * Mathf.Pow(2f, (progression[bar % progression.Length] + chord[order[step % order.Length] % chord.Length]) / 12f);
                int start = step * stepSamples;
                float amp = step % 2 == 0 ? 0.22f : 0.15f;
                for (int k = 0; k < tail; k++)
                {
                    float t = (float)k / sampleRate;
                    float v = (Mathf.Sin(TwoPi * freq * t) + 0.3f * Mathf.Sin(TwoPi * freq * 2f * t)) * Mathf.Exp(-t * 7f) * Mathf.Min(1f, k / 40f);
                    s[(start + k) % count] += v * amp;
                }
            }

            Normalize(s, 0.6f);
            return s;
        }

        // ------------------------------------------------------------------

        private static float[] Buffer(float seconds, int sampleRate) => new float[Mathf.Max(1, Mathf.RoundToInt(seconds * sampleRate))];
        private static float T(int i, int sampleRate) => (float)i / sampleRate;
        private static float Sin(float f, float t) => Mathf.Sin(TwoPi * f * t);
        private static float Square(float f, float t) => Sin(f, t) >= 0f ? 1f : -1f;
        private static float Noise(System.Random rng) => (float)(rng.NextDouble() * 2.0 - 1.0);
        private static float Coef(float cutoff, int sampleRate) => 1f - Mathf.Exp(-TwoPi * Mathf.Clamp(cutoff, 20f, sampleRate * 0.45f) / sampleRate);
        private static float Quantize(float f, float length) => Mathf.Max(1f, Mathf.Round(f * length)) / length;

        private static void Sweep(float[] s, int sampleRate, float from, float to, float sineMix, float squareMix)
        {
            float phase = 0f;
            for (int i = 0; i < s.Length; i++)
            {
                float p = (float)i / s.Length;
                phase += TwoPi * Mathf.Lerp(from, to, p) / sampleRate;
                float sin = Mathf.Sin(phase);
                s[i] = sin * sineMix + (sin >= 0f ? 1f : -1f) * squareMix * 0.4f;
            }
        }

        private static float[] Notes(int sampleRate, float spacing, float ring, params float[] freqs)
        {
            float[] s = Buffer(spacing * (freqs.Length - 1) + ring, sampleRate);
            for (int n = 0; n < freqs.Length; n++)
            {
                int start = Mathf.RoundToInt(n * spacing * sampleRate);
                for (int k = 0; start + k < s.Length; k++)
                {
                    float t = T(k, sampleRate);
                    s[start + k] += (Sin(freqs[n], t) + 0.25f * Sin(freqs[n] * 2f, t)) * Mathf.Exp(-t * 7f);
                }
            }

            return s;
        }

        /// <summary>Crossfades the tail into the head and drops the tail, so the wrap point is seamless.</summary>
        private static void MakeLoop(ref float[] s, int fade)
        {
            fade = Mathf.Clamp(fade, 1, s.Length / 2);
            int length = s.Length - fade;
            for (int i = 0; i < fade; i++)
            {
                float w = (float)i / fade;
                s[i] = s[i] * w + s[length + i] * (1f - w);
            }

            Array.Resize(ref s, length);
        }

        private static void Finish(float[] s, int sampleRate, bool fadeEdges)
        {
            if (fadeEdges)
            {
                int edge = Mathf.Min(s.Length / 4, sampleRate / 500);
                for (int i = 0; i < edge; i++)
                {
                    float w = (float)i / edge;
                    s[i] *= w;
                    s[s.Length - 1 - i] *= w;
                }
            }

            Normalize(s, Peak);
        }

        private static void Normalize(float[] s, float peak)
        {
            float max = 0f;
            foreach (float v in s) max = Mathf.Max(max, Mathf.Abs(v));
            if (max <= 1e-6f) return;
            float gain = peak / max;
            for (int i = 0; i < s.Length; i++) s[i] *= gain;
        }
    }
}
