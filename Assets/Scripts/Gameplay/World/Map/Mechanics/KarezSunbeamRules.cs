using System.Collections.Generic;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// The sunbeams of the Galerias do Karez (Docs/Mapas100.md 09: "fachos de sol: só dentro deles o herói regenera mana; os bichos evitam a luz";
    /// "Noite: sem fachos (só lâmpadas)"), pure and server-runnable. By day a beam of sun falls down every fourth shaft of the karez lines over the
    /// galleries (6 m round the shaft). In the galleries by day the dark keeps the hero's mana back (regen × 0.5) and a beam gives it back
    /// generously (× 2.5); the post (Boca do Karez) is ordinary ground. A beast standing in a beam is dazzled: it hits 20 % softer and moves 25 %
    /// slower (bosses are left alone), so the beams are where the hero makes a stand. At night there are no beams and nothing changes (the
    /// galleries' lamps are lit).
    /// <code>
    /// beam = every 4th shaft of the karez lines · radius 6 m · day = daylight ≥ 0.5
    /// day, galleries: hero mana regen × 0.5 in the dark, × 2.5 in a beam · beasts in a beam: damage × 0.8, speed × 0.75
    /// night: no beams, no change
    /// </code>
    /// </summary>
    public static class KarezSunbeamRules
    {
        public const float BeamRadius = 6f;
        public const int ShaftsPerBeam = 4;
        public const float DaylightThreshold = 0.5f;
        public const float BeamManaFactor = 2.5f;
        public const float DarkManaFactor = 0.5f;
        public const float DazzleDamageFactor = 0.8f;
        public const float DazzleSpeedFactor = 0.75f;

        /// <summary>Whether the sun is up enough to fall down the shafts (the clock's daylight factor: 1 = day, 0 = night).</summary>
        public static bool IsDay(float daylightFactor) => daylightFactor >= DaylightThreshold;

        /// <summary>
        /// The beams of one karez line, the way the line places its shafts (<c>n = floor(length / step)</c> shafts evenly from
        /// <paramref name="from"/> to <paramref name="to"/>, both ends included): every <see cref="ShaftsPerBeam"/>th shaft, from the first.
        /// </summary>
        public static void AddBeamsAlong(List<Vector2> beams, Vector2 from, Vector2 to, float step)
        {
            if (beams == null || step <= 0f)
            {
                return;
            }

            int n = Mathf.Max(1, Mathf.FloorToInt(Vector2.Distance(from, to) / step));
            for (int i = 0; i <= n; i += ShaftsPerBeam)
            {
                beams.Add(Vector2.Lerp(from, to, i / (float)n));
            }
        }

        /// <summary>The beam <paramref name="p"/> stands in (the nearest within <see cref="BeamRadius"/>), or −1.</summary>
        public static int BeamAt(IReadOnlyList<Vector2> beams, Vector2 p)
        {
            int best = -1;
            float bestSq = BeamRadius * BeamRadius;
            for (int i = 0; beams != null && i < beams.Count; i++)
            {
                float sq = (beams[i] - p).sqrMagnitude;
                if (sq <= bestSq)
                {
                    best = i;
                    bestSq = sq;
                }
            }

            return best;
        }

        /// <summary>The factor on the hero's mana regen: only by day in the galleries, off the post.</summary>
        public static float HeroManaFactor(bool inGalleries, bool onPost, bool day, bool inBeam)
        {
            if (!inGalleries || onPost || !day)
            {
                return 1f;
            }

            return inBeam ? BeamManaFactor : DarkManaFactor;
        }

        /// <summary>Whether a beast is dazzled: in a beam, by day, and not a boss.</summary>
        public static bool Dazzled(bool day, bool inBeam, bool isBoss) => day && inBeam && !isBoss;
    }
}
