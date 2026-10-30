using System;
using System.Collections.Generic;
using System.Linq;
using Rapadura.Core.Events;

// Unity-free on purpose: the online server (H8) decides drops itself and keeps this same record per account.
namespace Rapadura.Gameplay.Housing
{
    /// <summary>Why a house item arrived in the Depósito (H3, Housing.md §7).</summary>
    public enum HouseGrantReason
    {
        EnemyDrop,
        BossTrophy,
        BossDrop,
        Quest,
        DailyBox,
        DailyFirst,
        Achievement,
        Craft,
    }

    /// <summary>
    /// A house item was delivered to the Depósito as an "encomenda" (Housing.md §7.2): the toast and achievements listen.
    /// <see cref="SourceId"/> is the enemy, quest, achievement or recipe id.
    /// </summary>
    public readonly struct HouseItemGrantedEvent : IGameEvent
    {
        public readonly string ItemId;
        public readonly HouseGrantReason Reason;
        public readonly string SourceId;

        public HouseItemGrantedEvent(string itemId, HouseGrantReason reason, string sourceId)
        {
            ItemId = itemId;
            Reason = reason;
            SourceId = sourceId;
        }
    }

    /// <summary>A house recipe was made at the Bancada / Forja / Fogão of the house (counts for "Mestre Artesão" later).</summary>
    public readonly struct HouseRecipeCraftedEvent : IGameEvent
    {
        public readonly string RecipeId;
        public readonly string ResultId;

        public HouseRecipeCraftedEvent(string recipeId, string resultId)
        {
            RecipeId = recipeId;
            ResultId = resultId;
        }
    }

    /// <summary>Victories against one boss (first-win trophy + bad-luck protection, §7.2).</summary>
    [Serializable]
    public class HouseBossRecord
    {
        public string bossId;
        public int wins;
        /// <summary>Victories in a row without a Legendary house item from this boss.</summary>
        public int sinceLegendary;
        public bool trophyGiven;

        public HouseBossRecord Clone() => (HouseBossRecord)MemberwiseClone();
    }

    /// <summary>
    /// Save slice of the H3 sources, inside <c>HouseSaveState.sources</c>: boss records and the one-time rewards already
    /// handed out (quests, achievements, a daily's first time), so a reload or a repeated event never pays twice.
    /// </summary>
    [Serializable]
    public class HouseSourceState
    {
        public HouseBossRecord[] bosses = Array.Empty<HouseBossRecord>();
        /// <summary>"quest:&lt;id&gt;", "ach:&lt;id&gt;", "daily_first:&lt;id&gt;" (ordinal-sorted when saved).</summary>
        public string[] claimed = Array.Empty<string>();
        /// <summary>Common-enemy house drops so far (statistics; the drop chance itself has no memory).</summary>
        public int enemyDrops;

        public HouseSourceState Clone()
        {
            return new HouseSourceState
            {
                bosses = (bosses ?? Array.Empty<HouseBossRecord>()).Where(b => b != null).Select(b => b.Clone()).ToArray(),
                claimed = (claimed ?? Array.Empty<string>()).ToArray(),
                enemyDrops = enemyDrops,
            };
        }
    }

    /// <summary>Live, mutable view over <see cref="HouseSourceState"/> (pure; the service and the tests use it).</summary>
    public sealed class HouseSourceBook
    {
        private readonly Dictionary<string, HouseBossRecord> _bosses = new Dictionary<string, HouseBossRecord>(StringComparer.Ordinal);
        private readonly HashSet<string> _claimed = new HashSet<string>(StringComparer.Ordinal);

        public int EnemyDrops { get; set; }
        public IReadOnlyCollection<string> Claimed => _claimed;
        public IEnumerable<HouseBossRecord> Bosses => _bosses.Values;

        public static string QuestKey(string questId) => "quest:" + questId;
        public static string AchievementKey(string achievementId) => "ach:" + achievementId;
        public static string DailyFirstKey(string questId) => "daily_first:" + questId;

        public bool IsClaimed(string key) => key != null && _claimed.Contains(key);

        /// <summary>True the first time <paramref name="key"/> is claimed.</summary>
        public bool Claim(string key) => !string.IsNullOrEmpty(key) && _claimed.Add(key);

        public HouseBossRecord Boss(string bossId)
        {
            if (string.IsNullOrEmpty(bossId)) return null;
            if (!_bosses.TryGetValue(bossId, out HouseBossRecord record))
            {
                record = new HouseBossRecord { bossId = bossId };
                _bosses.Add(bossId, record);
            }

            return record;
        }

        public HouseBossRecord FindBoss(string bossId) => bossId != null && _bosses.TryGetValue(bossId, out HouseBossRecord r) ? r : null;

        public void Clear()
        {
            _bosses.Clear();
            _claimed.Clear();
            EnemyDrops = 0;
        }

        public HouseSourceState Capture()
        {
            return new HouseSourceState
            {
                bosses = _bosses.Values.OrderBy(b => b.bossId, StringComparer.Ordinal).Select(b => b.Clone()).ToArray(),
                claimed = _claimed.OrderBy(k => k, StringComparer.Ordinal).ToArray(),
                enemyDrops = EnemyDrops,
            };
        }

        public void Restore(HouseSourceState state)
        {
            Clear();
            if (state == null) return;
            foreach (HouseBossRecord record in state.bosses ?? Array.Empty<HouseBossRecord>())
            {
                if (record == null || string.IsNullOrEmpty(record.bossId) || _bosses.ContainsKey(record.bossId)) continue;
                HouseBossRecord copy = record.Clone();
                copy.wins = Math.Max(0, copy.wins);
                copy.sinceLegendary = Math.Max(0, copy.sinceLegendary);
                _bosses.Add(copy.bossId, copy);
            }

            foreach (string key in state.claimed ?? Array.Empty<string>())
            {
                if (!string.IsNullOrEmpty(key)) _claimed.Add(key);
            }

            EnemyDrops = Math.Max(0, state.enemyDrops);
        }
    }
}
