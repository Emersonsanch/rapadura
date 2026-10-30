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

        public static bool IsLoopPreset(ProceduralSfxPreset preset) => preset == ProceduralSfxPreset.RainLoop || preset == ProceduralSfxPreset.WindLoop || preset == ProceduralSfxPreset.CountrysideLoop;

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
                case ProceduralSfxPreset.Dizzy:
                {
                    s = Buffer(0.42f, sampleRate);
                    float phaseA = 0f, phaseB = 0f;
                    for (int i = 0; i < s.Length; i++)
                    {
                        float t = T(i, sampleRate), p = (float)i / s.Length;
                        float wobble = 1f + 0.06f * Mathf.Sin(TwoPi * 9f * t);
                        phaseA += TwoPi * f * (1f - 0.35f * p) * wobble / sampleRate;
                        phaseB += TwoPi * f * 1.5f * (1f - 0.3f * p) * (2f - wobble) / sampleRate;
                        s[i] = (Mathf.Sin(phaseA) + 0.3f * Mathf.Sin(2f * phaseA) + 0.35f * Mathf.Sin(phaseB)) * Mathf.Exp(-t * 5f) * Mathf.Min(1f, t * 150f);
                    }
                    break;
                }
                case ProceduralSfxPreset.Clink:
                {
                    s = Buffer(0.24f, sampleRate);
                    AddPluck(s, sampleRate, 0f, f, 1f, 45f);
                    AddPluck(s, sampleRate, 0.07f, f * 1.12f, 0.7f, 50f);
                    int tick = sampleRate / 200;
                    for (int i = 0; i < tick && i < s.Length; i++) s[i] += Noise(rng) * 0.3f * (1f - (float)i / tick);
                    break;
                }
                case ProceduralSfxPreset.WoodKnock:
                    s = Buffer(0.3f, sampleRate);
                    AddKnock(s, sampleRate, rng, 0f, f, 1f);
                    AddKnock(s, sampleRate, rng, 0.11f, f * 1.06f, 0.6f);
                    break;
                case ProceduralSfxPreset.WoodCreak:
                {
                    s = Buffer(0.48f, sampleRate);
                    const float creakEnd = 0.34f;
                    float pulse = 1f, ring = 0f, ringTime = 0f;
                    for (int i = 0; i < s.Length; i++)
                    {
                        float t = T(i, sampleRate);
                        if (t >= creakEnd) break;
                        float p = t / creakEnd;
                        // Stick-slip: a train of tiny impulses whose rate drifts (40 -> 95 Hz), each ringing the wood.
                        pulse += (40f + 55f * p + 10f * Mathf.Sin(TwoPi * 3f * t)) / sampleRate;
                        if (pulse >= 1f) { pulse -= 1f; ring = 0.6f + 0.4f * (float)rng.NextDouble(); ringTime = 0f; }
                        ringTime += 1f / sampleRate;
                        s[i] = ring * Mathf.Exp(-ringTime * 140f) * (Mathf.Sin(TwoPi * f * 3f * ringTime) + 0.5f * Mathf.Sin(TwoPi * f * 4.7f * ringTime)) * Mathf.Sin(Mathf.PI * p) * 0.8f;
                    }
                    AddKnock(s, sampleRate, rng, creakEnd + 0.02f, f * 0.6f, 1f);
                    break;
                }
                case ProceduralSfxPreset.PageRustle:
                {
                    s = Buffer(0.32f, sampleRate);
                    var crinkle = new float[s.Length];
                    int crinkleLength = sampleRate / 25;
                    for (int c = 0; c < 14; c++)
                    {
                        int start = rng.Next(s.Length * 4 / 5);
                        float amp = 0.5f + (float)rng.NextDouble() * 0.5f;
                        for (int k = 0; start + k < s.Length && k < crinkleLength; k++) crinkle[start + k] += amp * Mathf.Exp(-T(k, sampleRate) * 90f);
                    }
                    float lowCut = 0f, smooth = 0f, aLow = Coef(f * 0.4f, sampleRate), aHigh = Coef(f * 1.6f, sampleRate);
                    for (int i = 0; i < s.Length; i++)
                    {
                        float n = Noise(rng);
                        lowCut += aLow * (n - lowCut);
                        smooth += aHigh * ((n - lowCut) - smooth);
                        float body = 0.35f * Mathf.Sin(Mathf.PI * i / s.Length);
                        s[i] = smooth * 2.5f * (body + crinkle[i]);
                    }
                    break;
                }
                case ProceduralSfxPreset.ForroFlourish:
                {
                    // 150 bpm baiao, 16th = 0.1 s, two bars: zabumba (steps 0, 3, 4 of each bar + the closing
                    // hit), triangulo on every 16th (open on the off-beat eighths, muted otherwise) and a major
                    // arpeggio over the second bar, closing on the tonic chord.
                    const float step = 0.1f;
                    s = Buffer(step * 16f + 0.38f, sampleRate);
                    foreach (int k in new[] { 0, 3, 4, 8, 11, 12 }) AddZabumba(s, sampleRate, rng, k * step, k % 8 == 0 ? 1f : 0.7f);
                    AddZabumba(s, sampleRate, rng, 16 * step, 1f);
                    for (int k = 0; k < 16; k++)
                    {
                        bool open = k % 4 == 2;
                        AddTriangle(s, sampleRate, k * step, f * 7.6f, open ? 0.16f : 0.09f, open ? 12f : 80f);
                    }
                    float[] arp = { f, f * 1.2599f, f * 1.4983f, f * 2f };
                    for (int n = 0; n < arp.Length; n++) AddPluck(s, sampleRate, (8 + 2 * n) * step, arp[n], 0.45f, 9f);
                    AddPluck(s, sampleRate, 16 * step, f, 0.4f, 6f);
                    AddPluck(s, sampleRate, 16 * step, f * 1.2599f, 0.3f, 6f);
                    AddPluck(s, sampleRate, 16 * step, f * 1.4983f, 0.3f, 6f);
                    AddTriangle(s, sampleRate, 16 * step, f * 7.6f, 0.2f, 7f);
                    break;
                }
                case ProceduralSfxPreset.Murmur:
                {
                    // "hm-hm": two hummed syllables, harmonics rolling off like a closed-mouth voice.
                    s = Buffer(0.34f, sampleRate);
                    float phase = 0f;
                    for (int i = 0; i < s.Length; i++)
                    {
                        float t = T(i, sampleRate);
                        bool first = t < 0.14f;
                        float local = first ? t / 0.14f : (t - 0.18f) / 0.16f;
                        if (!first && local < 0f) { s[i] = 0f; continue; }
                        float pitch = first ? Mathf.Lerp(f * 1.1f, f, local) : Mathf.Lerp(f, f * 0.88f, local);
                        phase += TwoPi * pitch / sampleRate;
                        float v = 0f;
                        for (int h = 1; h <= 6; h++) v += Mathf.Sin(phase * h) / (h * h);
                        s[i] = v * Mathf.Sin(Mathf.PI * Mathf.Clamp01(local)) * (first ? 1f : 0.8f);
                    }
                    break;
                }
                case ProceduralSfxPreset.Prick:
                {
                    s = Buffer(0.16f, sampleRate);
                    float lp = 0f, a = Coef(f * 0.5f, sampleRate);
                    for (int i = 0; i < s.Length; i++)
                    {
                        float t = T(i, sampleRate), n = Noise(rng);
                        lp += a * (n - lp);
                        s[i] = (Sin(f, t) + 0.5f * Sin(f * 1.47f, t)) * Mathf.Exp(-t * 40f) + (n - lp) * 0.5f * Mathf.Exp(-t * 25f);
                    }
                    break;
                }
                case ProceduralSfxPreset.Inhale:
                {
                    s = Buffer(0.26f, sampleRate);
                    float lp = 0f;
                    for (int i = 0; i < s.Length; i++)
                    {
                        float p = (float)i / s.Length;
                        lp += Coef(f * (0.5f + p), sampleRate) * (Noise(rng) - lp);
                        s[i] = lp * Mathf.Pow(p, 1.5f) * Mathf.Clamp01((1f - p) * 10f);
                    }
                    break;
                }
                case ProceduralSfxPreset.SackThump:
                {
                    s = Buffer(0.3f, sampleRate);
                    Sweep(s, sampleRate, f, f * 0.6f, 1f, 0f);
                    float lp = 0f, a = Coef(600f, sampleRate);
                    int settle = Mathf.RoundToInt(0.09f * sampleRate);
                    for (int i = 0; i < s.Length; i++)
                    {
                        float t = T(i, sampleRate);
                        lp += a * (Noise(rng) - lp);
                        float second = i >= settle ? 0.35f * Mathf.Exp(-(t - 0.09f) * 30f) : 0f;
                        s[i] = s[i] * Mathf.Exp(-t * 20f) + lp * 1.6f * (Mathf.Exp(-t * 14f) + second);
                    }
                    break;
                }
                case ProceduralSfxPreset.CountrysideLoop:
                {
                    // Soft evening breeze (double low-pass noise, slow gusts) + two crickets chirping in
                    // groups of three at seeded, uneven intervals.
                    s = Buffer(3.7f, sampleRate);
                    float lp = 0f, lp2 = 0f;
                    for (int i = 0; i < s.Length; i++)
                    {
                        float p = (float)i / s.Length;
                        float gust = 0.7f + 0.3f * Mathf.Sin(TwoPi * p + 0.5f) * Mathf.Sin(TwoPi * 2f * p);
                        float a = Coef(350f * gust, sampleRate);
                        lp += a * (Noise(rng) - lp);
                        lp2 += a * (lp - lp2);
                        s[i] = lp2 * 2.2f * gust;
                    }
                    for (int voice = 0; voice < 2; voice++)
                    {
                        float carrier = f * (voice == 0 ? 1f : 1.08f);
                        float amp = voice == 0 ? 0.22f : 0.14f;
                        float at = 0.15f + voice * 0.37f;
                        while (at < 3.4f)
                        {
                            for (int pulse = 0; pulse < 3; pulse++) AddPluck(s, sampleRate, at + pulse * 0.045f, carrier, amp, 160f);
                            at += 0.65f + (float)rng.NextDouble() * 0.35f;
                        }
                    }
                    MakeLoop(ref s, sampleRate / 10);
                    break;
                }
                case ProceduralSfxPreset.Bark:
                {
                    // One "woof": a falling voiced tone (a few harmonics) with a breathy noise onset.
                    // Caramelo barks at ~420 Hz; the same preset an octave up is his happy "yip".
                    s = Buffer(0.17f, sampleRate);
                    float phase = 0f, lp = 0f, a = Coef(f * 3f, sampleRate);
                    for (int i = 0; i < s.Length; i++)
                    {
                        float t = T(i, sampleRate), p = (float)i / s.Length;
                        phase += TwoPi * f * Mathf.Lerp(1.35f, 0.8f, Mathf.Sqrt(p)) / sampleRate;
                        float voice = Mathf.Sin(phase) + 0.6f * Mathf.Sin(phase * 2f) + 0.35f * Mathf.Sin(phase * 3f) + 0.15f * Mathf.Sin(phase * 5f);
                        lp += a * (Noise(rng) - lp);
                        s[i] = (voice * 0.7f + lp * 1.2f * Mathf.Exp(-t * 40f)) * Mathf.Clamp01(t * 120f) * Mathf.Exp(-t * 14f);
                    }

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

        /// <summary>Adds a decaying bell-ish pluck (two inharmonic partials) starting at <paramref name="at"/> seconds.</summary>
        private static void AddPluck(float[] s, int sampleRate, float at, float f, float amp, float decay)
        {
            int start = Mathf.RoundToInt(at * sampleRate);
            float top = Mathf.Min(f * 3.9f, sampleRate * 0.45f);
            for (int k = 0; start + k < s.Length; k++)
            {
                float t = T(k, sampleRate);
                float env = Mathf.Exp(-t * decay);
                if (env < 0.001f) break;
                s[start + k] += (Sin(f, t) + 0.5f * Sin(Mathf.Min(f * 2.4f, sampleRate * 0.45f), t) + 0.25f * Sin(top, t)) * env * amp * Mathf.Min(1f, k / 20f);
            }
        }

        /// <summary>Hollow knock on wood: a fast-dropping low tone plus a short band of noise.</summary>
        private static void AddKnock(float[] s, int sampleRate, System.Random rng, float at, float f, float amp)
        {
            int start = Mathf.RoundToInt(at * sampleRate);
            float lp = 0f, a = Coef(f * 8f, sampleRate), phase = 0f;
            for (int k = 0; start + k < s.Length && k < sampleRate / 6; k++)
            {
                float t = T(k, sampleRate);
                lp += a * (Noise(rng) - lp);
                phase += TwoPi * f * (1f - 0.15f * Mathf.Min(1f, t * 20f)) / sampleRate;
                float tone = Mathf.Sin(phase) * Mathf.Exp(-t * 35f) + 0.4f * Sin(f * 2.3f, t) * Mathf.Exp(-t * 50f);
                s[start + k] += (tone + lp * 1.5f * Mathf.Exp(-t * 120f)) * amp;
            }
        }

        /// <summary>Zabumba hit: a pitch-dropping bass thump (110 -> 55 Hz) with a skin click.</summary>
        private static void AddZabumba(float[] s, int sampleRate, System.Random rng, float at, float amp)
        {
            int start = Mathf.RoundToInt(at * sampleRate);
            float phase = 0f;
            for (int k = 0; start + k < s.Length && k < sampleRate / 3; k++)
            {
                float t = T(k, sampleRate);
                phase += TwoPi * Mathf.Lerp(110f, 55f, Mathf.Min(1f, t * 8f)) / sampleRate;
                s[start + k] += (Mathf.Sin(phase) * Mathf.Exp(-t * 18f) + Noise(rng) * 0.25f * Mathf.Exp(-t * 200f)) * amp;
            }
        }

        /// <summary>Triangulo strike: three inharmonic metallic partials; a high <paramref name="decay"/> = muted (hand on the bar).</summary>
        private static void AddTriangle(float[] s, int sampleRate, float at, float f, float amp, float decay)
        {
            int start = Mathf.RoundToInt(at * sampleRate);
            float nyquist = sampleRate * 0.45f;
            float f1 = Mathf.Min(f, nyquist), f2 = Mathf.Min(f * 1.43f, nyquist), f3 = Mathf.Min(f * 1.88f, nyquist);
            for (int k = 0; start + k < s.Length; k++)
            {
                float t = T(k, sampleRate);
                float env = Mathf.Exp(-t * decay);
                if (env < 0.001f) break;
                s[start + k] += (Sin(f1, t) + 0.6f * Sin(f2, t) + 0.35f * Sin(f3, t)) * env * amp * Mathf.Min(1f, k / 8f);
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
