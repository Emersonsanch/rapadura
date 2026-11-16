using System.Collections.Generic;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// The red lanterns and the drum of the Nian's ring (map 45, Docs/Mapas100.md: "Nian, a Fera do Ano: assusta-se com lanternas vermelhas e
    /// com barulho (granada do Ícaro, tambor); a luta é espantar, não bater"), pure and server-runnable. The noise of a hard blow already
    /// frightens it (<c>RegionBossRules.NianTakesFright</c>); round its stone ring stand three red lanterns and one drum, and these frighten it too:
    /// <code>
    /// 3 red lanterns 9 m from the ring's centre (north, then every 120°) · the drum 9 m south · [E] within 3 m
    /// a lantern burns 40 s once lit; a pounce marked within 6 m of a lit lantern frightens Nian, and that lantern goes out
    ///   (the beast swats it): 20 s before it can be lit again
    /// the drum: a beat during the pounce's warning (2.4 s) frightens Nian · 6 s between beats (time it to the crouch)
    /// </code>
    /// Decision (2026-11-16): the lantern is spent when it saves the hero, so the three lanterns are three answers to share, not a safe spot to
    /// stand in; the drum needs timing. The time is any clock the caller keeps (seconds); online the server keeps it.
    /// </summary>
    public static class NianScareRules
    {
        public const int LanternCount = 3;
        public const float RingRadius = 9f;
        public const float Reach = 3f;
        public const float ShelterRadius = 6f;
        public const float LitSeconds = 40f;
        public const float CoolSeconds = 20f;
        public const float DrumCooldownSeconds = 6f;

        /// <summary>Where lantern <paramref name="index"/> stands round a ring at <paramref name="arena"/> (north first, clockwise every 120°).</summary>
        public static Vector2 LanternAt(Vector2 arena, int index)
        {
            float a = (((index % LanternCount) + LanternCount) % LanternCount) * (360f / LanternCount) * Mathf.Deg2Rad;
            return arena + new Vector2(Mathf.Sin(a), Mathf.Cos(a)) * RingRadius;
        }

        /// <summary>Where the drum stands: south of the ring.</summary>
        public static Vector2 DrumAt(Vector2 arena) => arena + Vector2.down * RingRadius;

        /// <summary>The lantern within <see cref="Reach"/> of <paramref name="p"/> (the nearest), or −1.</summary>
        public static int LanternNear(Vector2 arena, Vector2 p)
        {
            int best = -1;
            float bestSq = Reach * Reach;
            for (int i = 0; i < LanternCount; i++)
            {
                float sq = (LanternAt(arena, i) - p).sqrMagnitude;
                if (sq <= bestSq)
                {
                    best = i;
                    bestSq = sq;
                }
            }

            return best;
        }

        public static bool AtDrum(Vector2 arena, Vector2 p) => (DrumAt(arena) - p).sqrMagnitude <= Reach * Reach;

        public static bool IsLit(double litUntil, double now) => now < litUntil;

        /// <summary>Whether a lantern can be lit: not burning and past the cool-down after the Nian swatted it.</summary>
        public static bool CanLight(double litUntil, double coolUntil, double now) => !IsLit(litUntil, now) && now >= coolUntil;

        public static bool CanDrum(double lastBeat, double now) => now - lastBeat >= DrumCooldownSeconds;

        /// <summary>Whether a beat at <paramref name="beatAt"/> fell in the pounce's warning, from <paramref name="warningAt"/> to <paramref name="strikeAt"/>.</summary>
        public static bool DrumScares(double beatAt, double warningAt, double strikeAt) => beatAt >= warningAt && beatAt <= strikeAt;

        /// <summary>The lit lantern (the nearest) within <see cref="ShelterRadius"/> of the pounce's <paramref name="mark"/>, or −1: that one frightens Nian.</summary>
        public static int ShelteringLantern(IReadOnlyList<Vector2> lanterns, IReadOnlyList<double> litUntil, double now, Vector2 mark)
        {
            int best = -1;
            float bestSq = ShelterRadius * ShelterRadius;
            for (int i = 0; lanterns != null && litUntil != null && i < lanterns.Count && i < litUntil.Count; i++)
            {
                if (!IsLit(litUntil[i], now))
                {
                    continue;
                }

                float sq = (lanterns[i] - mark).sqrMagnitude;
                if (sq <= bestSq)
                {
                    best = i;
                    bestSq = sq;
                }
            }

            return best;
        }

        /// <summary>Nian takes fright from the noise of a blow, the drum or a red lantern by the mark.</summary>
        public static bool Frightened(bool noise, bool drum, bool lantern) => noise || drum || lantern;
    }
}
