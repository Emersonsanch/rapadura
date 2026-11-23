using System.Collections.Generic;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>One dimmed coral round Kirta with a spark caught in it, in the Costa's own coordinates (<see cref="Overworld.OverworldLayouts.KirtaCorals"/>).</summary>
    public readonly struct KirtaCoral
    {
        public readonly string Id;
        public readonly Vector2 Position;

        public KirtaCoral(string id, Vector2 position)
        {
            Id = id;
            Position = position;
        }
    }

    /// <summary>What [E] at a coral does (<see cref="KirtaCoralRules.Check"/>).</summary>
    public enum KirtaCoralCheck
    {
        /// <summary>The caught spark goes free and the coral lights again.</summary>
        Relight = 0,

        /// <summary>Too far from the coral.</summary>
        TooFar = 1,

        /// <summary>This coral is already lit.</summary>
        AlreadyLit = 2,

        /// <summary>A fallen hero frees nothing.</summary>
        Dead = 3,
    }

    /// <summary>
    /// Reacender o coral of Kirta (map 28, Docs/Mapas100.md §3.3: "cada faísca libertada no mapa acende um pedaço do recife; o coral aceso é zona
    /// de recuperação de vida (fora de combate). Dá a meta coletiva do servidor"), pure and server-runnable. Five dimmed corals stand round the
    /// atoll, each on a trail of the reef, each with a spark of Living Water caught in its grey branches:
    /// <code>
    /// [E] within 3.5 m of a dimmed coral: the spark goes free, that coral is lit for good (a time mark in the save, written only then)
    /// a lit coral: within 8 m, alive and 4 s out of combat, 2% of max life a second, up to full
    /// all five lit: the mother coral of the plaza is whole again (Kirta's line, once)
    /// </code>
    /// Decision (2026-11-23): the sparks are the corals' own (one caught in each), not the loose sparks of other maps, so the walk round the
    /// reef is the whole task. Online the server keeps the five for the whole map and sums the corals every player lit as the collective goal
    /// of the reef (<see cref="ServerGoalCorals"/>); offline each save lights its own. Nothing is taken from the hero, nothing damages.
    /// </summary>
    public static class KirtaCoralRules
    {
        public const int CoralCount = 5;
        public const float SparkReach = 3.5f;
        public const float MendRadius = 8f;
        public const float HealPerSecond = 0.02f;
        public const float OutOfCombatSeconds = 4f;

        /// <summary>Corals the whole server must light (every player's together) for the reef's collective goal: a hundred times the five.</summary>
        public const int ServerGoalCorals = CoralCount * 100;

        /// <summary>Time mark (value: the unix second it was lit, &gt; 0) of the coral <paramref name="id"/>.</summary>
        public static string MarkKey(string id) => "kirta.coral." + id;

        /// <summary>Time mark set once Kirta has thanked the hero for the five.</summary>
        public const string AllThankedKey = "kirta.coral.all";

        /// <summary>Whether a mark read back from the save means the coral is lit.</summary>
        public static bool IsLit(long mark) => mark > 0L;

        /// <summary>What [E] does at <paramref name="distance"/> metres of a coral (lit or not).</summary>
        public static KirtaCoralCheck Check(bool lit, float distance, bool alive)
        {
            if (!alive)
            {
                return KirtaCoralCheck.Dead;
            }

            if (distance > SparkReach)
            {
                return KirtaCoralCheck.TooFar;
            }

            return lit ? KirtaCoralCheck.AlreadyLit : KirtaCoralCheck.Relight;
        }

        /// <summary>The lit coral whose mending ring holds <paramref name="local"/>, or −1.</summary>
        public static int MendingCoral(IReadOnlyList<KirtaCoral> corals, IReadOnlyList<bool> lit, Vector2 local)
        {
            for (int i = 0; corals != null && lit != null && i < corals.Count && i < lit.Count; i++)
            {
                if (lit[i] && (corals[i].Position - local).sqrMagnitude <= MendRadius * MendRadius)
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>The nearest coral within <paramref name="reach"/> of <paramref name="local"/>, or −1.</summary>
        public static int Nearest(IReadOnlyList<KirtaCoral> corals, Vector2 local, float reach)
        {
            int best = -1;
            float bestSq = reach * reach;
            for (int i = 0; corals != null && i < corals.Count; i++)
            {
                float sq = (corals[i].Position - local).sqrMagnitude;
                if (sq <= bestSq)
                {
                    bestSq = sq;
                    best = i;
                }
            }

            return best;
        }

        /// <summary>Whether a lit coral mends now: alive, in its ring and out of combat for <see cref="OutOfCombatSeconds"/>.</summary>
        public static bool Mends(bool inRing, bool alive, float secondsSinceCombat) => inRing && alive && secondsSinceCombat >= OutOfCombatSeconds;

        /// <summary>Life given back over <paramref name="seconds"/>, never past full.</summary>
        public static float MendOver(float current, float max, float seconds) => QochoOasisRules.MendOver(current, max, HealPerSecond, seconds);

        public static int LitCount(IReadOnlyList<bool> lit)
        {
            int count = 0;
            for (int i = 0; lit != null && i < lit.Count; i++)
            {
                if (lit[i])
                {
                    count++;
                }
            }

            return count;
        }

        public static bool AllLit(IReadOnlyList<bool> lit) => LitCount(lit) >= CoralCount;

        /// <summary>The reef's collective goal, 0–1, from the corals lit by every player of the server.</summary>
        public static float ServerGoalProgress(long corals) => Mathf.Clamp01(corals / (float)ServerGoalCorals);
    }
}
