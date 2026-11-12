using System;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>The four answers of the riddles, one lantern each on the plaza of Mingcheng.</summary>
    public enum LanternAnswer
    {
        Sun = 0,
        Moon = 1,
        Carp = 2,
        Bell = 3,
    }

    /// <summary>
    /// The Festa das Lanternas of Mingcheng (Docs/Mapas100.md 41; WorldBible §3.20, Yuanxiao: the first full moon's night of lanterns and riddles),
    /// pure and server-runnable on the game clock (its day and hour, like the moon of the Mar Gelado, <see cref="MoonCanalRules"/>).
    /// <code>
    /// on the three bright nights of the moon (the days the Mar Gelado's canals freeze: moon 3–5 of 8), from 19 h to 1 h of the next day
    /// Mestre Mōzak lights the first lanterns at 18 h (the warning) · five riddle lanterns, four answer lanterns (Sun, Moon, Carp, Bell) on the plaza
    /// [E] at a riddle lantern reads its riddle and takes it; [E] at the right answer lantern solves it, at a wrong one the riddle lantern goes dark 6 s
    /// riddles of a night: (festa day × 5 + lantern) mod 8 — five different ones each night, the same for everyone on that night
    /// each riddle solved pays 30 gold; all five on one night: +1 Pedra-Faísca (once per night; the solved ones stay in the save for the night)
    /// </code>
    /// A festa night takes 6 game hours (5 real minutes at 20 min a day): the five lanterns are 140 m of walking from the plaza and back.
    /// </summary>
    public static class MingchengLanternFestaRules
    {
        public const float StartHour = 19f;
        public const float EndHour = 1f;
        public const float WarningHour = 18f;

        public const int LanternCount = 5;
        public const int RiddleCount = 8;
        public const float LanternReach = 3f;
        public const float AnswerReach = 1.8f;
        public const float WrongDarkSeconds = 6f;

        public const int GoldPerRiddle = 30;
        public const int AllSolvedStones = 1;

        /// <summary>Save keys (<see cref="WorldMapProgress"/> time marks): the festa day the marks belong to, its solved lanterns (bits) and the paid bonus.</summary>
        public const string NightKey = "city_event.festa_lanternas.night";
        public const string SolvedKey = "city_event.festa_lanternas.solved";
        public const string BonusKey = "city_event.festa_lanternas.bonus";

        /// <summary>The answer of each riddle, in riddle order (two riddles per answer).</summary>
        public static readonly LanternAnswer[] RiddleAnswers =
        {
            LanternAnswer.Sun, LanternAnswer.Moon, LanternAnswer.Carp, LanternAnswer.Bell,
            LanternAnswer.Sun, LanternAnswer.Moon, LanternAnswer.Carp, LanternAnswer.Bell,
        };

        /// <summary>A festa day: one of the three bright nights of the moon (moon 3–5).</summary>
        public static bool IsFestaDay(int day) => Mathf.Abs(MoonCanalRules.MoonIndex(day) - MoonCanalRules.FullMoonIndex) <= 1;

        /// <summary>The festa is on at game <paramref name="day"/>/<paramref name="hour"/>; <paramref name="festaDay"/> is the day whose evening it began.</summary>
        public static bool IsOpen(int day, float hour, out int festaDay)
        {
            if (hour >= StartHour && IsFestaDay(day))
            {
                festaDay = day;
                return true;
            }

            if (hour < EndHour && IsFestaDay(day - 1))
            {
                festaDay = day - 1;
                return true;
            }

            festaDay = 0;
            return false;
        }

        public static bool IsOpen(int day, float hour) => IsOpen(day, hour, out _);

        /// <summary>The hour before: Mōzak lights the first lanterns.</summary>
        public static bool IsWarning(int day, float hour) => IsFestaDay(day) && hour >= WarningHour && hour < StartHour;

        /// <summary>Game days until the next festa evening (0: tonight).</summary>
        public static int DaysUntilFesta(int day)
        {
            for (int wait = 0; wait < MoonCanalRules.CycleDays; wait++)
            {
                if (IsFestaDay(day + wait))
                {
                    return wait;
                }
            }

            return 0;
        }

        /// <summary>The riddle hung on <paramref name="lantern"/> on <paramref name="festaDay"/>'s night (five different ones per night).</summary>
        public static int RiddleAt(int festaDay, int lantern)
        {
            long index = ((long)festaDay * LanternCount + lantern) % RiddleCount;
            return (int)(index < 0 ? index + RiddleCount : index);
        }

        public static LanternAnswer AnswerOf(int riddle) => RiddleAnswers[Mathf.Clamp(riddle, 0, RiddleCount - 1)];

        public static bool IsRight(int riddle, LanternAnswer answer) => AnswerOf(riddle) == answer;

        // ------------------------------------------------------------------
        // The night's marks
        // ------------------------------------------------------------------

        /// <summary>The solved bits that count tonight: the saved ones only if they were saved for this same festa day.</summary>
        public static long SolvedTonight(long savedNight, long savedSolved, int festaDay) => savedNight == festaDay ? savedSolved : 0L;

        public static bool IsSolved(long solved, int lantern) => lantern >= 0 && lantern < LanternCount && (solved & (1L << lantern)) != 0L;

        public static long Solve(long solved, int lantern) => lantern >= 0 && lantern < LanternCount ? solved | (1L << lantern) : solved;

        public static int SolvedCount(long solved)
        {
            int count = 0;
            for (int i = 0; i < LanternCount; i++)
            {
                if (IsSolved(solved, i))
                {
                    count++;
                }
            }

            return count;
        }

        public static bool AllSolved(long solved) => SolvedCount(solved) >= LanternCount;

        /// <summary>The bonus of the five is paid once per festa day.</summary>
        public static bool BonusDue(long solved, long bonusPaidDay, int festaDay) => AllSolved(solved) && bonusPaidDay != festaDay;

        /// <summary>The nearest point of <paramref name="points"/> within <paramref name="reach"/> of <paramref name="hero"/>, or −1.</summary>
        public static int NearestWithin(Vector2[] points, Vector2 hero, float reach)
        {
            int best = -1;
            float bestDistance = reach;
            for (int i = 0; points != null && i < points.Length; i++)
            {
                float distance = Vector2.Distance(points[i], hero);
                if (distance <= bestDistance)
                {
                    best = i;
                    bestDistance = distance;
                }
            }

            return best;
        }
    }
}
