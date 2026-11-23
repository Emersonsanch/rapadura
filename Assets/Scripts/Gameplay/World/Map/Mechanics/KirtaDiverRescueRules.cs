using System.Collections.Generic;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>Where today's trapped diver waits: which walk along the bottom and which of its air bubbles.</summary>
    public readonly struct KirtaDiverSpot
    {
        /// <summary>Index into the walks (<c>OverworldLayouts.KirtaReefDives</c>).</summary>
        public readonly int Dive;

        /// <summary>Index into that walk's course (always a bubble: never the first or the last point, the stations).</summary>
        public readonly int Bubble;

        public KirtaDiverSpot(int dive, int bubble)
        {
            Dive = dive;
            Bubble = bubble;
        }
    }

    /// <summary>What Pughat's diver of the day answers (<see cref="KirtaDiverRescueRules.Check"/>).</summary>
    public enum KirtaDiverCheck
    {
        /// <summary>The hero reached the diver's bubble along the bottom: freed.</summary>
        Freed,

        /// <summary>Already freed today (UTC): Pughat brings another one tomorrow.</summary>
        DoneToday,

        /// <summary>Not at the bubble, or not walking the bottom.</summary>
        NotThere,
    }

    /// <summary>
    /// Pughat's rescues on the bottom of Kirta's reef (Docs/Mapas100.md §3.3: "Pughat, heroína que cuida de todos (missões de resgate)"; 28:
    /// "Bolhas de Ar (andar no fundo)"), pure and server-runnable; the scene side is <c>CityStallsMechanicsDriver.Kirta.cs</c>. Every UTC day one
    /// diver of the reef is caught by the Lodo at one air bubble of the two walks along the bottom (<see cref="KirtaReefWalkRules"/>): the bubble
    /// glows, Pughat at her door says where, and walking the bottom through it frees the diver. The south walk is the abyss: closed at night, so
    /// a diver there waits for the day.
    /// <code>
    /// spot of the day: one of the bubbles of the two walks (stations excluded), the same for everyone that day
    /// freed when the hero, walking the bottom, passes within 5 m of it · once per UTC day
    /// reward: 75 gold + 1 Pedra da Bênção (the north walk) · 110 gold + 1 Pedra da Bênção + 1 Bolha de Ar Engarrafada (the abyss)
    /// </code>
    /// Decision: the rescue is the reason to take the walk along the bottom every day (the trails go the same way on foot): the abyss pays
    /// more because it wants Danel's bottled bubble and the day.
    /// </summary>
    public static class KirtaDiverRescueRules
    {
        public const float RescueReach = 5f;
        public const float PughatReach = 3f;

        /// <summary>Daily mark: the UTC day of the last diver freed.</summary>
        public const string RescueDailyKey = "kirta.pughat.diver_rescue";

        /// <summary>Time mark: how many divers the cousins freed in all (for Pughat's lines).</summary>
        public const string RescueCountKey = "kirta.pughat.divers_freed";

        public const int NorthGold = 75;
        public const int AbyssGold = 110;
        public const int BlessingStones = 1;
        public const string BubbleItemId = "item_bolha_ar";

        /// <summary>The trapped divers, by the day (Ugaritic names of the epic of Kirta: his son Yassib, his daughters Thitmanat and Ilhu... ).</summary>
        public static readonly string[] DiverIds = { "yassib", "thitmanat", "ilhu", "pabil" };

        /// <summary>All the bubbles of <paramref name="courses"/> (the walks' courses), walk by walk.</summary>
        public static List<KirtaDiverSpot> Bubbles(IReadOnlyList<Vector2[]> courses)
        {
            var list = new List<KirtaDiverSpot>();
            if (courses == null)
            {
                return list;
            }

            for (int dive = 0; dive < courses.Count; dive++)
            {
                Vector2[] course = courses[dive];
                for (int i = 1; course != null && i < course.Length - 1; i++)
                {
                    list.Add(new KirtaDiverSpot(dive, i));
                }
            }

            return list;
        }

        /// <summary>The spot of a UTC day: the same for everyone, never the same bubble two days running (when there are two or more).</summary>
        public static KirtaDiverSpot SpotOf(long day, IReadOnlyList<Vector2[]> courses)
        {
            List<KirtaDiverSpot> bubbles = Bubbles(courses);
            if (bubbles.Count == 0)
            {
                return new KirtaDiverSpot(-1, -1);
            }

            int today = TunnelMarketStallRules.Pick(day * 7L + 3L, bubbles.Count);
            int yesterday = TunnelMarketStallRules.Pick((day - 1L) * 7L + 3L, bubbles.Count);
            return bubbles[today != yesterday ? today : (today + 1) % bubbles.Count];
        }

        /// <summary>The diver of a UTC day (an id of <see cref="DiverIds"/>).</summary>
        public static string DiverOf(long day) => DiverIds[TunnelMarketStallRules.Pick(day * 13L + 1L, DiverIds.Length)];

        public static KirtaDiverCheck Check(bool walkingBottom, Vector2 hero, Vector2 bubble, long freedDay, long today)
        {
            if (freedDay == today)
            {
                return KirtaDiverCheck.DoneToday;
            }

            return walkingBottom && CityServiceRules.InReach(bubble, hero, RescueReach) ? KirtaDiverCheck.Freed : KirtaDiverCheck.NotThere;
        }

        public static int GoldFor(bool abyss) => abyss ? AbyssGold : NorthGold;

        public static int BubblesFor(bool abyss) => abyss ? 1 : 0;
    }
}
