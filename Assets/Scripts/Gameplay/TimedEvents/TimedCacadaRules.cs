using System;
using System.Collections.Generic;

namespace Rapadura.Gameplay.TimedEvents
{
    /// <summary>What the server counted for one hunter during the Caçada (all from its own combat log, never the client).</summary>
    public readonly struct TimedContribution
    {
        public readonly string PlayerId;

        /// <summary>Damage dealt to Ohyah and its minions (capped by their health: no overkill).</summary>
        public readonly double Damage;

        /// <summary>Effective healing on other hunters (overheal does not count).</summary>
        public readonly double Healing;

        /// <summary>Damage taken from Ohyah while it was targeting this hunter (the tank's share).</summary>
        public readonly double Tanked;

        /// <summary>Seconds with an action (hit, heal, skill, taunt, revive) inside the arena.</summary>
        public readonly double ActiveSeconds;

        /// <summary>Seconds inside the arena.</summary>
        public readonly double PresentSeconds;

        public TimedContribution(string playerId, double damage, double healing, double tanked, double activeSeconds, double presentSeconds)
        {
            PlayerId = playerId;
            Damage = damage;
            Healing = healing;
            Tanked = tanked;
            ActiveSeconds = activeSeconds;
            PresentSeconds = presentSeconds;
        }
    }

    /// <summary>The Caçada result for one hunter.</summary>
    public sealed class TimedRaidShare
    {
        public string PlayerId;

        /// <summary>Weighted contribution (<see cref="TimedCacadaRules.Score"/>).</summary>
        public double Score;

        /// <summary>Score / sum of the active hunters' scores (0 for the idle ones).</summary>
        public double Share;

        /// <summary>Did something (not idle): gets at least the floor.</summary>
        public bool Active;

        /// <summary>1-based place by score among the active (0 = idle).</summary>
        public int Rank;

        /// <summary>Gets the floor reward ("prêmio de quem lutou").</summary>
        public bool Floor;

        /// <summary>Won one of Ohyah's items (at most one per hunter).</summary>
        public bool WonItem;

        /// <summary>Most damage / most healing / most tanked: a Marca de Ohyah each.</summary>
        public bool TopDamage;
        public bool TopHealing;
        public bool TopTank;
    }

    /// <summary>
    /// "Caçada a Ohyah" (world boss for many, MU style): Ohyah, the old giant of the Book of Giants, wakes at a set hour;
    /// health grows with the hunters. Loot by contribution: damage ×1, effective healing ×1,2, tanked damage ×0,8 (so
    /// healers and tanks are not behind the damage dealers). Everyone who really fought gets the floor; an idle hunter
    /// (under 45 s of actions or under 30% of their time in the arena active, or zero score) gets nothing.
    /// Ohyah drops 3 items + 1 per 5 active hunters (max 10), one per hunter, drawn by share with a 2% minimum weight, seeded.
    /// </summary>
    public static class TimedCacadaRules
    {
        public const double DamageWeight = 1.0;
        public const double HealingWeight = 1.2;
        public const double TankWeight = 0.8;

        public const double MinActiveSeconds = 45d;
        public const double MinActiveFraction = 0.3;

        public const int BaseItems = 3;
        public const int HuntersPerExtraItem = 5;
        public const int MaxItems = 10;
        public const double MinItemWeight = 0.02;

        /// <summary>Ohyah's health per hunter at room 1; ×1,8 per room above.</summary>
        public const int HealthPerHunter = 60000;
        public const float HealthPerRoom = 0.8f;

        /// <summary>Health of Ohyah for <paramref name="hunters"/> in the arena at the start (minimum 5, so a small band still has a real fight).</summary>
        public static long BossHealth(int roomIndex, int hunters)
        {
            int counted = Math.Max(5, hunters);
            return (long)Math.Round(HealthPerHunter * (double)counted * (1d + HealthPerRoom * Math.Max(0, roomIndex - 1)));
        }

        public static double Score(TimedContribution c) =>
            Math.Max(0d, c.Damage) * DamageWeight + Math.Max(0d, c.Healing) * HealingWeight + Math.Max(0d, c.Tanked) * TankWeight;

        /// <summary>Not idle: ≥ 45 s of actions, ≥ 30% of the time in the arena active and some score.</summary>
        public static bool IsActive(TimedContribution c)
        {
            return !string.IsNullOrEmpty(c.PlayerId) && c.PresentSeconds > 0d && c.ActiveSeconds >= MinActiveSeconds
                && c.ActiveSeconds / c.PresentSeconds >= MinActiveFraction && Score(c) > 0d;
        }

        public static int ItemCount(int activeHunters)
        {
            if (activeHunters <= 0)
            {
                return 0;
            }

            return Math.Min(MaxItems, Math.Min(activeHunters, BaseItems + activeHunters / HuntersPerExtraItem));
        }

        /// <summary>
        /// Splits the Caçada. <paramref name="bossDefeated"/> false → only the floor (no items, no marks).
        /// <paramref name="seed"/> = <see cref="TimedEventAgenda.Seed"/> of the room. Output in input order.
        /// </summary>
        public static List<TimedRaidShare> Distribute(IReadOnlyList<TimedContribution> contributions, bool bossDefeated, int seed)
        {
            var shares = new List<TimedRaidShare>();
            if (contributions == null)
            {
                return shares;
            }

            double total = 0d;
            var seen = new HashSet<string>();
            foreach (TimedContribution c in contributions)
            {
                if (string.IsNullOrEmpty(c.PlayerId) || !seen.Add(c.PlayerId))
                {
                    continue;
                }

                var share = new TimedRaidShare { PlayerId = c.PlayerId, Score = Score(c), Active = IsActive(c) };
                if (share.Active)
                {
                    total += share.Score;
                }

                shares.Add(share);
            }

            var active = shares.FindAll(s => s.Active);
            foreach (TimedRaidShare share in active)
            {
                share.Share = total > 0d ? share.Score / total : 0d;
                share.Floor = true;
            }

            var ranked = new List<TimedRaidShare>(active);
            ranked.Sort((a, b) => a.Score != b.Score ? b.Score.CompareTo(a.Score) : string.CompareOrdinal(a.PlayerId, b.PlayerId));
            for (int i = 0; i < ranked.Count; i++)
            {
                ranked[i].Rank = i + 1;
            }

            if (!bossDefeated || active.Count == 0)
            {
                return shares;
            }

            MarkTop(contributions, active, c => c.Damage, s => s.TopDamage = true);
            MarkTop(contributions, active, c => c.Healing, s => s.TopHealing = true);
            MarkTop(contributions, active, c => c.Tanked, s => s.TopTank = true);

            int items = ItemCount(active.Count);
            for (int drop = 0; drop < items; drop++)
            {
                double weightSum = 0d;
                foreach (TimedRaidShare candidate in ranked)
                {
                    if (!candidate.WonItem)
                    {
                        weightSum += Math.Max(MinItemWeight, candidate.Share);
                    }
                }

                if (weightSum <= 0d)
                {
                    break;
                }

                double roll = TimedEventAgenda.Roll(seed, drop, 0, 77) * weightSum;
                TimedRaidShare winner = null;
                foreach (TimedRaidShare candidate in ranked)
                {
                    if (candidate.WonItem)
                    {
                        continue;
                    }

                    winner = candidate;
                    double weight = Math.Max(MinItemWeight, candidate.Share);
                    if (roll < weight)
                    {
                        break;
                    }

                    roll -= weight;
                }

                if (winner != null)
                {
                    winner.WonItem = true;
                }
            }

            return shares;
        }

        private static void MarkTop(IReadOnlyList<TimedContribution> contributions, List<TimedRaidShare> active,
            Func<TimedContribution, double> value, Action<TimedRaidShare> mark)
        {
            string best = null;
            double bestValue = 0d;
            foreach (TimedContribution c in contributions)
            {
                if (!active.Exists(s => s.PlayerId == c.PlayerId))
                {
                    continue;
                }

                double v = value(c);
                if (v > bestValue || (v == bestValue && v > 0d && best != null && string.CompareOrdinal(c.PlayerId, best) < 0))
                {
                    best = c.PlayerId;
                    bestValue = v;
                }
            }

            if (best != null && bestValue > 0d)
            {
                mark(active.Find(s => s.PlayerId == best));
            }
        }

        /// <summary>Score-board points of a hunter: 1 per 0,1% of the total contribution (max 1000), +50 per top mark.</summary>
        public static int Points(TimedRaidShare share)
        {
            if (share == null || !share.Active)
            {
                return 0;
            }

            int points = (int)Math.Min(1000d, Math.Round(share.Share * 1000d));
            points += (share.TopDamage ? 50 : 0) + (share.TopHealing ? 50 : 0) + (share.TopTank ? 50 : 0);
            return points;
        }
    }
}
