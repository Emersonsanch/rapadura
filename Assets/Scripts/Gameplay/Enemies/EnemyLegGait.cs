using Rapadura.Gameplay.Locomotion;
using UnityEngine;

namespace Rapadura.Gameplay.Enemies
{
    /// <summary>
    /// Maps an enemy's <see cref="EnemyMotionProfile"/> (already scaled for its body) onto the generic
    /// <see cref="LegGaitSettings"/> of <see cref="ProceduralLegLocomotion"/>, so the stepping legs keep
    /// the archetype's personality: wolves trot at patrol speed and gallop on the chase with the profile's
    /// rocking pitch; spiders and scorpions skitter at a high cadence with a short, low stride; heavies
    /// and bosses stomp with long, slow strides and a pronounced weight shift. Re-evaluated every frame
    /// (a struct copy) so the quadruped bob/pitch follow the gallop blend.
    /// </summary>
    public static class EnemyLegGait
    {
        /// <summary>How much lower (fraction of the leg) the body rides while the legs step, so the knees always bend a little.</summary>
        public const float KneeBendDrop = 0.05f;

        public static LegGaitSettings SettingsFor(in EnemyMotionProfile p, LegGaitStyle style, float legLength, float sizeScale, float gallop)
        {
            float size = Mathf.Clamp(sizeScale, 0.5f, 2f);
            LegGaitSettings s = LegGaitSettings.Default(style);
            switch (style)
            {
                case LegGaitStyle.Quadruped:
                    s.StrideLength = p.StrideLength * size;
                    s.FullSpeed = p.FullGaitSpeed;
                    s.MinCadence = p.MinStrideHz;
                    s.MaxCadence = Mathf.Max(p.MaxStrideHz, 3.2f) * 1.2f;
                    s.TrotSpeed = 1.4f * Mathf.Sqrt(size);
                    s.GallopStartSpeed = p.GallopStartSpeed > 0f ? p.GallopStartSpeed : 2.8f;
                    s.GallopFullSpeed = p.GallopFullSpeed > s.GallopStartSpeed ? p.GallopFullSpeed : s.GallopStartSpeed + 1.2f;
                    s.ReachFraction = 0.6f;
                    s.BobAmplitude = Mathf.Lerp(p.BobAmplitude, p.GallopBobAmplitude, gallop) * size;
                    s.PitchDegrees = Mathf.Max(p.PitchAmplitude * 2f, p.GallopPitchAmplitude);
                    s.RollDegrees = p.RollAmplitude;
                    break;
                case LegGaitStyle.Arthropod:
                    s.StrideLength = p.StrideLength * size;
                    s.FullSpeed = p.FullGaitSpeed;
                    s.MinCadence = Mathf.Min(p.MinStrideHz, 3f);
                    s.MaxCadence = Mathf.Min(p.MaxStrideHz, 7f);
                    s.ReachFraction = 0.45f;
                    s.BobAmplitude = p.BobAmplitude * size;
                    s.PitchDegrees = p.PitchAmplitude;
                    s.RollDegrees = p.RollAmplitude;
                    break;
                default:
                    s.StrideLength = p.StrideLength * size;
                    s.FullSpeed = p.FullGaitSpeed;
                    s.MinCadence = p.MinStrideHz;
                    s.MaxCadence = Mathf.Max(p.MaxStrideHz * 1.35f, 1.2f);
                    s.TrotSpeed = 2.6f * Mathf.Sqrt(size);
                    s.BobAmplitude = p.BobAmplitude * size * 1.3f;
                    s.PitchDegrees = p.PitchAmplitude * 0.6f;
                    s.RollDegrees = p.RollAmplitude;
                    break;
            }

            // Bigger bodies lift their feet relatively less (weight), small ones flick them high.
            s.StepHeightFraction *= Mathf.Lerp(1.15f, 0.85f, Mathf.InverseLerp(0.6f, 1.8f, size));
            if (legLength > 0f)
            {
                s.StrideLength = Mathf.Max(s.StrideLength, legLength * 0.8f);
            }

            return s;
        }
    }
}
