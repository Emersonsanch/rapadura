using System.Collections.Generic;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>One lamp of the Salão de Tages (map 20), in the region's own coordinates.</summary>
    public readonly struct TagesLamp
    {
        public readonly string Id;
        public readonly Vector2 At;

        public TagesLamp(string id, Vector2 at)
        {
            Id = id;
            At = at;
        }
    }

    /// <summary>How the beasts near a lamp stand (<see cref="TagesDarkRules.MoodAt"/>).</summary>
    public enum TagesBeastMood
    {
        /// <summary>No lamp of the hall within reach: as they are.</summary>
        None = 0,

        /// <summary>The nearest lamp is lit: calmer, they hit softer.</summary>
        Calm = 1,

        /// <summary>The nearest lamp is out: the dark makes them bolder, harder and faster.</summary>
        Bold = 2,
    }

    /// <summary>
    /// The fear of the dark of the Salão de Tages (Docs/Mapas100.md 20: "medo do escuro: apagar uma lâmpada deixa os bichos de perto mais fortes;
    /// acender acalma"; "o salão suspira e as lâmpadas tremem"), pure and server-runnable. Six lamps stand in the hall: three in the niches of the
    /// corridor of columns and one by each clearing (the south gallery, the dark ford, the colonnade). A lamp burns 5 minutes after it is lit, then
    /// the hall's sigh blows it out; a lamp never lit is out. The nearest lamp within 16 m of a beast sets its mood: lit, the beast hits 15 % softer;
    /// out, it hits 25 % harder and runs 15 % faster. [E] at an unlit lamp lights it again, from 3 m (Maria, the cousin of Light, from 6 m). Bosses
    /// are left alone. The moment each lamp was lit stays in the save (a time mark).
    /// <code>
    /// 6 lamps · burn 300 s · beasts within 16 m of the nearest lamp: lit → damage × 0.85 · out → damage × 1.25, speed × 1.15
    /// [E] reach 3 m (Maria 6 m) · a lamp is lit only when it is out
    /// </code>
    /// </summary>
    public static class TagesDarkRules
    {
        public const long BurnSeconds = 300L;
        public const float BeastReach = 16f;
        public const float CalmDamageFactor = 0.85f;
        public const float BoldDamageFactor = 1.25f;
        public const float BoldSpeedFactor = 1.15f;
        public const float LightReach = 3f;
        public const float LightBearerReach = 6f;

        /// <summary>The save key of a lamp (a time mark: the unix second it was last lit, 0 = never).</summary>
        public static string LitKey(string lampId) => "limiar.tages.lamp." + lampId;

        /// <summary>Whether a lamp lit at <paramref name="litAt"/> still burns at <paramref name="now"/> (unix seconds).</summary>
        public static bool Burning(long litAt, long now) => litAt > 0L && now >= litAt && now - litAt < BurnSeconds;

        /// <summary>Seconds of light left (0 when out).</summary>
        public static long SecondsLeft(long litAt, long now) => Burning(litAt, now) ? BurnSeconds - (now - litAt) : 0L;

        /// <summary>How far [E] reaches a lamp: Maria (Light) from farther.</summary>
        public static float ReachFor(bool lightBearer) => lightBearer ? LightBearerReach : LightReach;

        /// <summary>The lamp within <paramref name="reach"/> of <paramref name="p"/> (the nearest), or −1.</summary>
        public static int LampAt(IReadOnlyList<TagesLamp> lamps, Vector2 p, float reach)
        {
            int best = -1;
            float bestSq = reach * reach;
            for (int i = 0; lamps != null && i < lamps.Count; i++)
            {
                float sq = (lamps[i].At - p).sqrMagnitude;
                if (sq <= bestSq)
                {
                    best = i;
                    bestSq = sq;
                }
            }

            return best;
        }

        /// <summary>Whether [E] lights the lamp (it is out).</summary>
        public static bool CanLight(long litAt, long now) => !Burning(litAt, now);

        /// <summary>The mood of a beast at <paramref name="beast"/>: set by the nearest lamp within <see cref="BeastReach"/>.</summary>
        public static TagesBeastMood MoodAt(IReadOnlyList<TagesLamp> lamps, IReadOnlyList<long> litMarks, Vector2 beast, long now)
        {
            int nearest = LampAt(lamps, beast, BeastReach);
            if (nearest < 0)
            {
                return TagesBeastMood.None;
            }

            long lit = litMarks != null && nearest < litMarks.Count ? litMarks[nearest] : 0L;
            return Burning(lit, now) ? TagesBeastMood.Calm : TagesBeastMood.Bold;
        }

        /// <summary>The beast's damage factor in <paramref name="mood"/>.</summary>
        public static float DamageFactor(TagesBeastMood mood) => mood == TagesBeastMood.Bold ? BoldDamageFactor : mood == TagesBeastMood.Calm ? CalmDamageFactor : 1f;

        /// <summary>The beast's speed factor in <paramref name="mood"/>.</summary>
        public static float SpeedFactor(TagesBeastMood mood) => mood == TagesBeastMood.Bold ? BoldSpeedFactor : 1f;

        /// <summary>How many of the lamps burn now.</summary>
        public static int BurningCount(IReadOnlyList<long> litMarks, long now)
        {
            int burning = 0;
            for (int i = 0; litMarks != null && i < litMarks.Count; i++)
            {
                if (Burning(litMarks[i], now))
                {
                    burning++;
                }
            }

            return burning;
        }
    }
}
