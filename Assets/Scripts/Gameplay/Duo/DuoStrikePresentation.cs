using UnityEngine;

namespace Rapadura.Gameplay.Duo
{
    /// <summary>
    /// How loud the duo strike looks, after the accessibility options (pure, EditMode-tested):
    /// <list type="bullet">
    /// <item><b>Reduzir Flashes</b>: no flash sprite in the burst (SkillVfxFlashPolicy already drops it), the partner
    /// silhouette is dimmer and alpha-blended instead of additive, no golden pulse, no hit-stop.</item>
    /// <item><b>Reduzir Movimento</b>: the partner appears and fades in place (no lunge, no arm swing), no camera shake,
    /// no hit-stop, the shout banner shows without a fade.</item>
    /// </list>
    /// Sibling combos ("laço de irmãos") get a second burst ring, a brighter gold and a stronger shake.
    /// </summary>
    public readonly struct DuoStrikePresentation
    {
        public readonly float SilhouetteAlpha;
        public readonly bool AdditiveSilhouette;
        public readonly bool Pulse;
        public readonly bool Lunge;
        public readonly float ShakeDuration;
        public readonly float ShakeMagnitude;
        public readonly float HitStopDuration;
        public readonly bool SecondRing;
        public readonly bool BannerFade;

        public DuoStrikePresentation(float silhouetteAlpha, bool additiveSilhouette, bool pulse, bool lunge, float shakeDuration,
            float shakeMagnitude, float hitStopDuration, bool secondRing, bool bannerFade)
        {
            SilhouetteAlpha = silhouetteAlpha;
            AdditiveSilhouette = additiveSilhouette;
            Pulse = pulse;
            Lunge = lunge;
            ShakeDuration = shakeDuration;
            ShakeMagnitude = shakeMagnitude;
            HitStopDuration = hitStopDuration;
            SecondRing = secondRing;
            BannerFade = bannerFade;
        }

        public const float NormalAlpha = 0.65f;
        public const float ReducedFlashAlpha = 0.4f;
        public const float SiblingShakeMagnitude = 0.35f;
        public const float ShakeMagnitudeDefault = 0.22f;
        public const float HitStopSeconds = 0.08f;

        public static DuoStrikePresentation Resolve(bool siblingBond, bool reduceFlashes, bool reduceMotion)
        {
            bool shake = !reduceMotion;
            return new DuoStrikePresentation(
                silhouetteAlpha: reduceFlashes ? ReducedFlashAlpha : NormalAlpha,
                additiveSilhouette: !reduceFlashes,
                pulse: !reduceFlashes && !reduceMotion,
                lunge: !reduceMotion,
                shakeDuration: shake ? (siblingBond ? 0.3f : 0.2f) : 0f,
                shakeMagnitude: shake ? (siblingBond ? SiblingShakeMagnitude : ShakeMagnitudeDefault) : 0f,
                hitStopDuration: reduceFlashes || reduceMotion ? 0f : HitStopSeconds,
                secondRing: siblingBond,
                bannerFade: !reduceMotion && !reduceFlashes);
        }

        /// <summary>Partner tint: their accent colour pulled toward gold (siblings: almost pure gold).</summary>
        public static Color SilhouetteColor(Color accent, bool siblingBond)
        {
            var gold = new Color(1f, 0.84f, 0.42f, 1f);
            Color tint = Color.Lerp(accent, gold, siblingBond ? 0.8f : 0.55f);
            tint.a = 1f;
            return tint;
        }
    }
}
