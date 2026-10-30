using System;
using System.Collections.Generic;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Skills;

namespace Rapadura.Gameplay.Talents
{
    [Serializable]
    public class TalentNodeRank
    {
        public string nodeId;
        public int rank;
    }

    /// <summary>One cousin's training and wheel (JSON-serializable).</summary>
    [Serializable]
    public class TalentRecord
    {
        public string characterId;
        public List<ProficiencyEntry> proficiencies = new List<ProficiencyEntry>();
        public List<TalentNodeRank> nodes = new List<TalentNodeRank>();
        /// <summary>How many times the wheel was reset (informative; the price does not grow with it).</summary>
        public int respecs;
    }

    public enum TalentSpendResult
    {
        Spent = 0,
        Locked = 1,
        NoPoints = 2,
        UnknownNode = 3,
        MaxRank = 4,
        NotYourNode = 5,
        RequirementMissing = 6
    }

    public enum TalentRespecResult
    {
        Done = 0,
        NothingToReset = 1,
        NotEnoughGold = 2
    }

    /// <summary>
    /// Rules of the "Roda de Talentos" (Docs/Balance/README.md §27): points from the hero level, spending with the ring
    /// requirements, the gold price of a reset and the modifiers it puts on the hero. Pure: the server owns the record online.
    /// </summary>
    public static class TalentWheelRules
    {
        public const int RespecGoldPerPoint = 15;
        public const int RespecMinimumGold = 50;

        public static bool IsUnlocked(int heroLevel) => heroLevel >= TalentWheelCatalog.UnlockLevel;

        public static int EarnedPoints(int heroLevel) =>
            IsUnlocked(heroLevel) ? (heroLevel - TalentWheelCatalog.UnlockLevel + 1) * TalentWheelCatalog.PointsPerLevel : 0;

        public static int RankOf(TalentRecord record, string nodeId)
        {
            if (record?.nodes == null)
            {
                return 0;
            }

            foreach (TalentNodeRank entry in record.nodes)
            {
                if (entry != null && entry.nodeId == nodeId)
                {
                    return entry.rank;
                }
            }

            return 0;
        }

        public static int SpentPoints(TalentRecord record)
        {
            int total = 0;
            if (record?.nodes == null)
            {
                return 0;
            }

            foreach (TalentNodeRank entry in record.nodes)
            {
                TalentNodeSpec node = entry != null ? TalentWheelCatalog.GetNode(entry.nodeId) : null;
                total += node != null ? Math.Max(0, entry.rank) * node.CostPerRank : 0;
            }

            return total;
        }

        public static int SpentIn(TalentRecord record, TalentQuadrant quadrant)
        {
            int total = 0;
            if (record?.nodes == null)
            {
                return 0;
            }

            foreach (TalentNodeRank entry in record.nodes)
            {
                TalentNodeSpec node = entry != null ? TalentWheelCatalog.GetNode(entry.nodeId) : null;
                if (node != null && node.Quadrant == quadrant)
                {
                    total += Math.Max(0, entry.rank) * node.CostPerRank;
                }
            }

            return total;
        }

        public static int AvailablePoints(TalentRecord record, int heroLevel) => Math.Max(0, EarnedPoints(heroLevel) - SpentPoints(record));

        public static TalentSpendResult CanSpend(TalentRecord record, CharacterId hero, int heroLevel, string nodeId)
        {
            if (!IsUnlocked(heroLevel))
            {
                return TalentSpendResult.Locked;
            }

            TalentNodeSpec node = TalentWheelCatalog.GetNode(nodeId);
            if (node == null || record == null)
            {
                return TalentSpendResult.UnknownNode;
            }

            if (!TalentWheelCatalog.BelongsTo(node, hero))
            {
                return TalentSpendResult.NotYourNode;
            }

            if (RankOf(record, nodeId) >= node.MaxRank)
            {
                return TalentSpendResult.MaxRank;
            }

            if ((node.RequiredNodeId != null && RankOf(record, node.RequiredNodeId) < node.RequiredRank)
                || SpentIn(record, node.Quadrant) < node.RequiredQuadrantPoints)
            {
                return TalentSpendResult.RequirementMissing;
            }

            if (AvailablePoints(record, heroLevel) < node.CostPerRank)
            {
                return TalentSpendResult.NoPoints;
            }

            return TalentSpendResult.Spent;
        }

        public static TalentSpendResult Spend(TalentRecord record, CharacterId hero, int heroLevel, string nodeId)
        {
            TalentSpendResult result = CanSpend(record, hero, heroLevel, nodeId);
            if (result != TalentSpendResult.Spent)
            {
                return result;
            }

            foreach (TalentNodeRank entry in record.nodes)
            {
                if (entry != null && entry.nodeId == nodeId)
                {
                    entry.rank++;
                    return result;
                }
            }

            record.nodes.Add(new TalentNodeRank { nodeId = nodeId, rank = 1 });
            return result;
        }

        /// <summary>Gold a reset costs: <see cref="RespecGoldPerPoint"/> per spent point, at least <see cref="RespecMinimumGold"/> (0 = nothing spent).</summary>
        public static int RespecCost(TalentRecord record)
        {
            int spent = SpentPoints(record);
            return spent <= 0 ? 0 : Math.Max(RespecMinimumGold, spent * RespecGoldPerPoint);
        }

        public static TalentRespecResult CanRespec(TalentRecord record, int goldOwned)
        {
            if (SpentPoints(record) <= 0)
            {
                return TalentRespecResult.NothingToReset;
            }

            return goldOwned < RespecCost(record) ? TalentRespecResult.NotEnoughGold : TalentRespecResult.Done;
        }

        /// <summary>Clears the wheel (the caller already took <see cref="RespecCost"/> in gold). Returns the points refunded.</summary>
        public static int Reset(TalentRecord record)
        {
            int spent = SpentPoints(record);
            if (record?.nodes == null || spent <= 0)
            {
                return 0;
            }

            record.nodes.Clear();
            record.respecs++;
            return spent;
        }

        /// <summary>Total bonus of one stat on the wheel.</summary>
        public static float Bonus(TalentRecord record, StatType stat)
        {
            float total = 0f;
            foreach (TalentNodeSpec node in TalentWheelCatalog.Nodes)
            {
                if (node.Stat == stat)
                {
                    total += node.ValuePerRank * Math.Min(node.MaxRank, RankOf(record, node.NodeId));
                }
            }

            return total;
        }

        /// <summary>The modifiers the wheel puts on the hero (one per node with ranks), for the BuffController.</summary>
        public static List<StatModifierDefinition> Modifiers(TalentRecord record)
        {
            var list = new List<StatModifierDefinition>();
            foreach (TalentNodeSpec node in TalentWheelCatalog.Nodes)
            {
                int rank = Math.Min(node.MaxRank, RankOf(record, node.NodeId));
                if (rank <= 0)
                {
                    continue;
                }

                list.Add(new StatModifierDefinition
                {
                    displayName = "talent_" + node.NodeId,
                    affectedStat = node.Stat,
                    application = node.Application,
                    value = node.ValuePerRank * rank,
                    durationSeconds = 0f,
                    isDebuff = false,
                });
            }

            return list;
        }

        /// <summary>
        /// Repairs a saved wheel for <paramref name="hero"/>: unknown/other cousins' nodes and duplicates dropped, ranks capped,
        /// nodes whose requirement is gone dropped. Points are not trimmed here (the level is not in the record): a hero that
        /// somehow lost levels just has a negative balance until it levels again, so nothing is ever silently taken.
        /// </summary>
        public static void Sanitize(TalentRecord record, CharacterId hero)
        {
            if (record == null)
            {
                return;
            }

            record.characterId = hero.ToString();
            record.respecs = Math.Max(0, record.respecs);
            record.proficiencies ??= new List<ProficiencyEntry>();
            ProficiencyRules.Sanitize(record.proficiencies);
            record.nodes ??= new List<TalentNodeRank>();
            record.nodes.RemoveAll(e => e == null || e.rank <= 0 || !TalentWheelCatalog.BelongsTo(TalentWheelCatalog.GetNode(e.nodeId), hero));
            var seen = new HashSet<string>();
            record.nodes.RemoveAll(e => !seen.Add(e.nodeId));
            foreach (TalentNodeRank entry in record.nodes)
            {
                entry.rank = Math.Min(TalentWheelCatalog.GetNode(entry.nodeId).MaxRank, entry.rank);
            }

            // Ring requirements, checked against the node's own quadrant without itself (keystones count the small nodes).
            bool changed = true;
            while (changed)
            {
                changed = false;
                foreach (TalentNodeRank entry in record.nodes)
                {
                    TalentNodeSpec node = TalentWheelCatalog.GetNode(entry.nodeId);
                    int others = SpentIn(record, node.Quadrant) - entry.rank * node.CostPerRank;
                    if ((node.RequiredNodeId != null && RankOf(record, node.RequiredNodeId) < node.RequiredRank) || others < node.RequiredQuadrantPoints)
                    {
                        record.nodes.Remove(entry);
                        changed = true;
                        break;
                    }
                }
            }
        }

        public static TalentRecord NewRecord(CharacterId hero)
        {
            var record = new TalentRecord { characterId = hero.ToString() };
            ProficiencyRules.Sanitize(record.proficiencies);
            return record;
        }

        public static ProficiencyEntry Proficiency(TalentRecord record, ProficiencyKind kind)
        {
            if (record?.proficiencies == null)
            {
                return null;
            }

            foreach (ProficiencyEntry entry in record.proficiencies)
            {
                if (entry != null && entry.kind == kind.ToString())
                {
                    return entry;
                }
            }

            var added = new ProficiencyEntry { kind = kind.ToString() };
            record.proficiencies.Add(added);
            return added;
        }
    }
}
