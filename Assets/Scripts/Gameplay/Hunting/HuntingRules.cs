using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Rapadura.Gameplay.Enemies;

namespace Rapadura.Gameplay.Hunting
{
    /// <summary>Size of a "Tarefa de caça" from Seu Zé Caçador (kills and rewards in <see cref="HuntingRules"/>).</summary>
    public enum HuntingTaskTier
    {
        /// <summary>"Pequena".</summary>
        Small = 0,
        /// <summary>"Média".</summary>
        Medium = 1,
        /// <summary>"Grande".</summary>
        Large = 2,
    }

    /// <summary>What a "Presa do dia" gives against its creature (one per offer, drawn with the day's seed).</summary>
    public enum PreyBonusKind
    {
        /// <summary>+<see cref="HuntingRules.PreyExperienceBonus"/> kill XP.</summary>
        Experience = 0,
        /// <summary>+<see cref="HuntingRules.PreyLootBonus"/> chance of a second loot roll.</summary>
        Loot = 1,
        /// <summary>-<see cref="HuntingRules.PreyProtectionBonus"/> damage taken from it ("Couro grosso"). Not a damage bonus:
        /// the commons' power budget has no room left (§ "Caça").</summary>
        Protection = 2,
    }

    /// <summary>One creature the hunting features can name: a non-boss page of the Caderno, as plain data (tests build their own).</summary>
    public readonly struct HuntingCreature
    {
        public readonly string Id;
        public readonly EnemyMotionArchetype Archetype;
        public readonly bool IsBoss;
        /// <summary>Page-order region index (0 = Floresta), <see cref="int.MaxValue"/> when the enemy is not placed anywhere.</summary>
        public readonly int RegionIndex;
        public readonly int ExperienceReward;

        public HuntingCreature(string id, EnemyMotionArchetype archetype, bool isBoss, int regionIndex, int experienceReward)
        {
            Id = id;
            Archetype = archetype;
            IsBoss = isBoss;
            RegionIndex = regionIndex;
            ExperienceReward = experienceReward;
        }

        public bool IsPlaced => RegionIndex != int.MaxValue;
    }

    /// <summary>One "Presa do dia" offer: a creature and the bonus picking it gives.</summary>
    [Serializable]
    public class PreyOffer
    {
        public string enemyId;
        public PreyBonusKind bonus;

        public PreyOffer()
        {
        }

        public PreyOffer(string enemyId, PreyBonusKind bonus)
        {
            this.enemyId = enemyId;
            this.bonus = bonus;
        }
    }

    /// <summary>A running "Tarefa de caça": kill <see cref="required"/> of <see cref="enemyId"/>. Rewards are fixed when it is taken.</summary>
    [Serializable]
    public class HuntingTask
    {
        public string enemyId;
        public HuntingTaskTier tier;
        public int required;
        public int progress;
        public int rewardExperience;
        public int rewardGold;
        public int rewardHuntPoints;

        public bool IsComplete => required > 0 && progress >= required;
    }

    /// <summary>
    /// Pure rules of the Tibia-inspired hunting features (Docs/Balance/README.md § "Caça"), kept free of Unity and of
    /// the EventBus so EditMode tests cover them with plain data:
    /// <list type="bullet">
    /// <item><b>Tarefas de caça</b> (Seu Zé Caçador): kill N of one creature, sized Pequena / Média / Grande;
    /// rewards are bonus XP (a fraction of the kills' own XP), gold and "Pontos de Caça".</item>
    /// <item><b>Presa do dia</b>: each day 3 creatures are offered, each with a bonus (XP, loot or damage); the
    /// chosen one lasts until the day turns. No hunting bonus raises the hero's damage (§ "Caça"). One reroll a day, for gold.</item>
    /// <item><b>Bicho destaque do dia</b>: one creature per day gives +50% XP and loot.</item>
    /// <item><b>Pontos do Caderno</b>: kill milestones on each Caderno page (10 / 50 / 200), spent on
    /// <see cref="HuntingCharmCatalog"/> charms.</item>
    /// </list>
    /// Bosses never take part: no task, prey, highlight or charm counts against them (their fights are balanced on
    /// their own, §14). Days are local calendar days ("yyyy-MM-dd", <c>DailyQuestService.ToDayKey</c>); online, the
    /// day key and the seed come from the server clock (§ "Caça").
    /// </summary>
    public static class HuntingRules
    {
        // ------------------------------------------------------------------ tasks

        public const int SmallKills = 10;
        public const int MediumKills = 25;
        public const int LargeKills = 60;

        /// <summary>Bonus XP of a task = kills × the creature's kill XP × this (Pequena / Média / Grande).</summary>
        public const float SmallExperienceFactor = 0.5f;
        public const float MediumExperienceFactor = 0.6f;
        public const float LargeExperienceFactor = 0.75f;

        /// <summary>Gold of a task = kills × this.</summary>
        public const float SmallGoldPerKill = 1.5f;
        public const float MediumGoldPerKill = 1.8f;
        public const float LargeGoldPerKill = 2f;

        public const int SmallHuntPoints = 1;
        public const int MediumHuntPoints = 3;
        public const int LargeHuntPoints = 8;

        /// <summary>Anti-farm: tasks that can be finished per day (a new one can't be taken after that).</summary>
        public const int MaxTasksPerDay = 5;

        /// <summary>Most creatures Seu Zé offers tasks for at once (the eligible ones, in Caderno order).</summary>
        public const int MaxTaskCreatures = 6;

        /// <summary>"Pontos de Caça" Seu Zé takes for one "Ponto do Caderno".</summary>
        public const int HuntPointsPerCadernoPoint = 5;

        // ------------------------------------------------------------------ prey / highlight

        public const int PreyOfferCount = 3;
        public const float PreyExperienceBonus = 0.25f;
        public const float PreyLootBonus = 0.25f;
        /// <summary>Less damage taken from the prey creature (stacks with the type charm, capped by <see cref="MaxProtection"/>).</summary>
        public const float PreyProtectionBonus = 0.1f;
        /// <summary>Most damage reduction hunting bonuses can give against one creature.</summary>
        public const float MaxProtection = 0.3f;
        public const int PreyRerollGold = 15;
        public const int MaxPreyRerollsPerDay = 1;

        public const float BoostedExperienceBonus = 0.5f;
        public const float BoostedLootBonus = 0.5f;

        // ------------------------------------------------------------------ Caderno points

        /// <summary>Kills on one page that each give <see cref="MilestonePoints"/> "Pontos do Caderno".</summary>
        public static readonly int[] KillMilestones = { 10, 50, 200 };
        public static readonly int[] MilestonePoints = { 1, 2, 3 };

        /// <summary>Total "Pontos de Caça" ever earned for each hunter rank (0 = Aprendiz, 1 = Mateiro, 2 = Rastreador, 3 = Caçador do Sertão).</summary>
        public static readonly int[] RankThresholds = { 0, 5, 20, 50 };

        public static int KillsFor(HuntingTaskTier tier)
        {
            switch (tier)
            {
                case HuntingTaskTier.Medium: return MediumKills;
                case HuntingTaskTier.Large: return LargeKills;
                default: return SmallKills;
            }
        }

        public static float ExperienceFactor(HuntingTaskTier tier)
        {
            switch (tier)
            {
                case HuntingTaskTier.Medium: return MediumExperienceFactor;
                case HuntingTaskTier.Large: return LargeExperienceFactor;
                default: return SmallExperienceFactor;
            }
        }

        public static float GoldPerKill(HuntingTaskTier tier)
        {
            switch (tier)
            {
                case HuntingTaskTier.Medium: return MediumGoldPerKill;
                case HuntingTaskTier.Large: return LargeGoldPerKill;
                default: return SmallGoldPerKill;
            }
        }

        public static int HuntPointsFor(HuntingTaskTier tier)
        {
            switch (tier)
            {
                case HuntingTaskTier.Medium: return MediumHuntPoints;
                case HuntingTaskTier.Large: return LargeHuntPoints;
                default: return SmallHuntPoints;
            }
        }

        /// <summary>A new task against <paramref name="creature"/> (null for a boss or an empty id: bosses get no tasks).</summary>
        public static HuntingTask CreateTask(HuntingCreature creature, HuntingTaskTier tier)
        {
            if (string.IsNullOrEmpty(creature.Id) || creature.IsBoss)
            {
                return null;
            }

            int kills = KillsFor(tier);
            return new HuntingTask
            {
                enemyId = creature.Id,
                tier = tier,
                required = kills,
                progress = 0,
                rewardExperience = (int)Math.Round(kills * Math.Max(0, creature.ExperienceReward) * ExperienceFactor(tier), MidpointRounding.AwayFromZero),
                rewardGold = (int)Math.Round(kills * GoldPerKill(tier), MidpointRounding.AwayFromZero),
                rewardHuntPoints = HuntPointsFor(tier),
            };
        }

        /// <summary>Counts one kill of <paramref name="enemyId"/> on <paramref name="task"/>. True only on the kill that completes it.</summary>
        public static bool RecordKill(HuntingTask task, string enemyId)
        {
            if (task == null || task.IsComplete || string.IsNullOrEmpty(enemyId) || enemyId != task.enemyId)
            {
                return false;
            }

            task.progress = Math.Min(task.required, task.progress + 1);
            return task.IsComplete;
        }

        /// <summary>Whether a new task can be taken: none running and today's anti-farm cap not reached.</summary>
        public static bool CanTakeTask(HuntingTask running, int completedToday) =>
            (running == null || string.IsNullOrEmpty(running.enemyId)) && completedToday < MaxTasksPerDay;

        /// <summary>
        /// Creatures tasks and prey can name: non-boss pages already unlocked, in Caderno order; when none is unlocked
        /// yet, the placed non-boss creatures of the first region (a new game still gets offers).
        /// </summary>
        public static List<HuntingCreature> EligibleCreatures(IEnumerable<HuntingCreature> creatures, Func<string, bool> isUnlocked)
        {
            List<HuntingCreature> regular = (creatures ?? Enumerable.Empty<HuntingCreature>())
                .Where(c => !string.IsNullOrEmpty(c.Id) && !c.IsBoss && c.IsPlaced)
                .GroupBy(c => c.Id).Select(g => g.First())
                .ToList();

            List<HuntingCreature> unlocked = regular.Where(c => isUnlocked != null && isUnlocked(c.Id)).ToList();
            if (unlocked.Count > 0)
            {
                return unlocked;
            }

            if (regular.Count == 0)
            {
                return regular;
            }

            int firstRegion = regular.Min(c => c.RegionIndex);
            return regular.Where(c => c.RegionIndex == firstRegion).ToList();
        }

        // ------------------------------------------------------------------ daily seeding

        /// <summary>
        /// Deterministic pick of <paramref name="count"/> distinct ids for a day and a purpose (<paramref name="salt"/>):
        /// ids sorted ordinally, then Fisher-Yates-shuffled by an integer PRNG seeded with FNV-1a("day|salt") (not
        /// System.Random, whose sequence isn't guaranteed across runtimes). Same day + salt = same picks on every device.
        /// </summary>
        public static List<string> PickForDay(string dayKey, string salt, IEnumerable<string> ids, int count)
        {
            List<string> list = (ids ?? Enumerable.Empty<string>())
                .Where(id => !string.IsNullOrEmpty(id))
                .Distinct()
                .OrderBy(id => id, StringComparer.Ordinal)
                .ToList();

            uint state = Seed(dayKey, salt);
            for (int i = list.Count - 1; i > 0; i--)
            {
                state = Next(state);
                int j = (int)(Mix(state) % (uint)(i + 1));
                (list[i], list[j]) = (list[j], list[i]);
            }

            return list.Take(Math.Max(0, Math.Min(count, list.Count))).ToList();
        }

        /// <summary>Stable 32-bit seed of a day key and a salt.</summary>
        public static uint Seed(string dayKey, string salt)
        {
            uint hash = 2166136261u;
            string text = (dayKey ?? string.Empty) + "|" + (salt ?? string.Empty);
            foreach (char c in text)
            {
                hash ^= c;
                hash *= 16777619u;
            }

            return Mix(hash);
        }

        private static uint Next(uint state) => state * 1664525u + 1013904223u;

        private static uint Mix(uint x)
        {
            x ^= x >> 16;
            x *= 0x7feb352du;
            x ^= x >> 15;
            x *= 0x846ca68bu;
            x ^= x >> 16;
            return x;
        }

        /// <summary>"Bicho destaque do dia": one placed non-boss creature (every one can be drawn, unlocked or not), or null.</summary>
        public static string BoostedCreature(string dayKey, IEnumerable<HuntingCreature> creatures)
        {
            IEnumerable<string> ids = (creatures ?? Enumerable.Empty<HuntingCreature>()).Where(c => !c.IsBoss && c.IsPlaced).Select(c => c.Id);
            return PickForDay(dayKey, "boosted", ids, 1).FirstOrDefault();
        }

        /// <summary>
        /// Today's "Presa do dia" offers: up to <see cref="PreyOfferCount"/> eligible creatures (the highlighted one left
        /// out while there are enough others), each with a seeded bonus. <paramref name="reroll"/> (0 = first draw)
        /// changes the salt, so a reroll gives a new set.
        /// </summary>
        public static List<PreyOffer> PreyOffers(string dayKey, int reroll, IReadOnlyList<HuntingCreature> eligible, string boostedId)
        {
            var pool = (eligible ?? Array.Empty<HuntingCreature>()).Select(c => c.Id).Where(id => !string.IsNullOrEmpty(id)).Distinct().ToList();
            if (boostedId != null && pool.Count > PreyOfferCount)
            {
                pool.Remove(boostedId);
            }

            string salt = "prey:" + Math.Max(0, reroll).ToString(CultureInfo.InvariantCulture);
            List<string> picked = PickForDay(dayKey, salt, pool, PreyOfferCount);
            var offers = new List<PreyOffer>(picked.Count);
            for (int i = 0; i < picked.Count; i++)
            {
                uint roll = Mix(Seed(dayKey, salt + ":bonus:" + picked[i]));
                offers.Add(new PreyOffer(picked[i], (PreyBonusKind)(roll % 3u)));
            }

            return offers;
        }

        /// <summary>Whether today's prey can be rerolled: rerolls left and enough gold.</summary>
        public static bool CanRerollPrey(int rerollsToday, int gold) => rerollsToday < MaxPreyRerollsPerDay && gold >= PreyRerollGold;

        /// <summary>
        /// True when <paramref name="today"/> is strictly after <paramref name="current"/> (null current = always). A clock
        /// moved backwards never rolls the day over (no second set of dailies, no prey reset to farm).
        /// </summary>
        public static bool IsNewDay(string current, string today) =>
            !string.IsNullOrEmpty(today) && (string.IsNullOrEmpty(current) || string.CompareOrdinal(today, current) > 0);

        // ------------------------------------------------------------------ bonuses on a kill / hit

        /// <summary>Kill XP multiplier against <paramref name="creature"/>: 1 + prey XP bonus + highlight bonus (bosses: 1).</summary>
        public static float ExperienceMultiplier(HuntingCreature creature, PreyOffer activePrey, string boostedId)
        {
            if (creature.IsBoss || string.IsNullOrEmpty(creature.Id))
            {
                return 1f;
            }

            float bonus = 0f;
            if (IsPrey(activePrey, creature.Id, PreyBonusKind.Experience))
            {
                bonus += PreyExperienceBonus;
            }

            if (creature.Id == boostedId)
            {
                bonus += BoostedExperienceBonus;
            }

            return 1f + bonus;
        }

        /// <summary>Chance (may exceed 1) of extra whole loot rolls against <paramref name="creature"/>: prey loot + highlight (bosses: 0).</summary>
        public static float ExtraLootChance(HuntingCreature creature, PreyOffer activePrey, string boostedId)
        {
            if (creature.IsBoss || string.IsNullOrEmpty(creature.Id))
            {
                return 0f;
            }

            float chance = 0f;
            if (IsPrey(activePrey, creature.Id, PreyBonusKind.Loot))
            {
                chance += PreyLootBonus;
            }

            if (creature.Id == boostedId)
            {
                chance += BoostedLootBonus;
            }

            return chance;
        }

        /// <summary>Extra loot rolls for a chance: its whole part always, plus one more when <paramref name="roll01"/> &lt; the fraction.</summary>
        public static int ExtraLootRolls(float chance, double roll01)
        {
            if (chance <= 0f || float.IsNaN(chance))
            {
                return 0;
            }

            int whole = (int)Math.Floor(chance);
            float fraction = chance - whole;
            return whole + (fraction > 0f && roll01 < fraction ? 1 : 0);
        }

        /// <summary>
        /// Multiplier on the damage the hero takes from <paramref name="creature"/>: 1 - (prey protection + the equipped type
        /// charms), at most <see cref="MaxProtection"/> off (bosses: 1).
        /// </summary>
        public static float DamageTakenMultiplier(HuntingCreature creature, PreyOffer activePrey, IEnumerable<string> equippedCharms)
        {
            if (creature.IsBoss || string.IsNullOrEmpty(creature.Id))
            {
                return 1f;
            }

            float protection = IsPrey(activePrey, creature.Id, PreyBonusKind.Protection) ? PreyProtectionBonus : 0f;
            protection += HuntingCharmCatalog.TypeProtectionBonus(equippedCharms, creature.Archetype);
            return 1f - Math.Min(MaxProtection, protection);
        }

        public static bool IsPrey(PreyOffer prey, string enemyId, PreyBonusKind kind) =>
            prey != null && !string.IsNullOrEmpty(prey.enemyId) && prey.enemyId == enemyId && prey.bonus == kind;

        // ------------------------------------------------------------------ Caderno points / rank

        /// <summary>"Pontos do Caderno" one page gives for <paramref name="kills"/> (cumulative milestones).</summary>
        public static int MilestonePointsFor(int kills)
        {
            int points = 0;
            for (int i = 0; i < KillMilestones.Length; i++)
            {
                if (kills >= KillMilestones[i])
                {
                    points += MilestonePoints[i];
                }
            }

            return points;
        }

        /// <summary>Every page's milestone points.</summary>
        public static int TotalMilestonePoints(IEnumerable<int> killsPerPage) =>
            (killsPerPage ?? Enumerable.Empty<int>()).Sum(MilestonePointsFor);

        /// <summary>The next milestone of a page (kills), or 0 once all are reached.</summary>
        public static int NextMilestone(int kills)
        {
            foreach (int milestone in KillMilestones)
            {
                if (kills < milestone)
                {
                    return milestone;
                }
            }

            return 0;
        }

        /// <summary>Points left: earned (milestones + traded) minus what the unlocked charms cost; never below 0.</summary>
        public static int AvailableCadernoPoints(int milestonePoints, int tradedPoints, int spentPoints) =>
            Math.Max(0, milestonePoints + tradedPoints - spentPoints);

        /// <summary>Hunter rank (index into <see cref="RankThresholds"/>) for the total "Pontos de Caça" ever earned.</summary>
        public static int RankFor(int huntPointsEarned)
        {
            int rank = 0;
            for (int i = 0; i < RankThresholds.Length; i++)
            {
                if (huntPointsEarned >= RankThresholds[i])
                {
                    rank = i;
                }
            }

            return rank;
        }
    }
}
