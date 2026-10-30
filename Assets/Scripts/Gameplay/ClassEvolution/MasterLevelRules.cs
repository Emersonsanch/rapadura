using System;
using System.Collections.Generic;
using Rapadura.Gameplay.Skills;

namespace Rapadura.Gameplay.ClassEvolution
{
    /// <summary>One node of the master tree ("Árvore de Mestre"): a small stat bonus per rank.</summary>
    public sealed class MasterNodeSpec
    {
        public readonly string NodeId;
        public readonly StatType Stat;
        public readonly ModifierApplication Application;
        public readonly float ValuePerRank;
        public readonly int MaxRank;
        /// <summary>Node that needs <see cref="RequiredRank"/> ranks before this one opens (null = first row).</summary>
        public readonly string RequiredNodeId;
        public readonly int RequiredRank;

        public MasterNodeSpec(string nodeId, StatType stat, ModifierApplication application, float valuePerRank, int maxRank,
            string requiredNodeId, int requiredRank)
        {
            NodeId = nodeId;
            Stat = stat;
            Application = application;
            ValuePerRank = valuePerRank;
            MaxRank = maxRank;
            RequiredNodeId = requiredNodeId;
            RequiredRank = requiredRank;
        }

        public string NameKey => "class.master.node." + NodeId + ".name";
        public string DescriptionKey => "class.master.node." + NodeId + ".description";
        public float MaxValue => ValuePerRank * MaxRank;
    }

    /// <summary>Why a master point could not be spent.</summary>
    public enum MasterSpendResult
    {
        Spent = 0,
        NotMaster = 1,
        NoPoints = 2,
        UnknownNode = 3,
        MaxRank = 4,
        Locked = 5
    }

    /// <summary>
    /// Master level ("Nível de Mestre", Docs/ClassEvolution.md §4), after the max level like MU Online's Master Level: a cousin
    /// with the 3rd graduation at level <see cref="RequiredLevel"/> stops gaining normal XP and fills a separate master bar
    /// instead; each master level gives one point for a small tree of modest bonuses. Pure: the server owns the record.
    /// </summary>
    public static class MasterLevelRules
    {
        /// <summary>The max level (= ProgressionCurve.DefaultMaxLevel).</summary>
        public const int RequiredLevel = 50;
        public const int RequiredTier = ClassEvolutionCatalog.MaxTier;
        public const int MaxMasterLevel = 30;
        public const int PointsPerMasterLevel = 1;
        public const int BaseMasterExperience = 25000;
        public const float MasterExperienceGrowth = 0.15f;

        /// <summary>Row 1 has no requirement; each row-2 node needs 3 ranks in the node above it. Full tree = 30 points.</summary>
        public static readonly IReadOnlyList<MasterNodeSpec> Nodes = new[]
        {
            new MasterNodeSpec("couro_curtido", StatType.MaxHealth, ModifierApplication.PercentAdditive, 0.01f, 5, null, 0),
            new MasterNodeSpec("mao_firme", StatType.AttackDamage, ModifierApplication.PercentAdditive, 0.005f, 5, null, 0),
            new MasterNodeSpec("folego_de_vaqueiro", StatType.ManaRegen, ModifierApplication.PercentAdditive, 0.02f, 5, null, 0),
            new MasterNodeSpec("pe_no_chao", StatType.Defense, ModifierApplication.PercentAdditive, 0.006f, 5, "couro_curtido", 3),
            new MasterNodeSpec("olho_vivo", StatType.CriticalChance, ModifierApplication.Flat, 0.004f, 5, "mao_firme", 3),
            new MasterNodeSpec("passo_ligeiro", StatType.MoveSpeed, ModifierApplication.PercentAdditive, 0.01f, 5, "folego_de_vaqueiro", 3),
        };

        public static MasterNodeSpec GetNode(string nodeId)
        {
            foreach (MasterNodeSpec node in Nodes)
            {
                if (node.NodeId == nodeId)
                {
                    return node;
                }
            }

            return null;
        }

        public static bool IsMaster(GraduationRecord record, int heroLevel) =>
            record != null && record.tier >= RequiredTier && heroLevel >= RequiredLevel;

        /// <summary>Master XP from master level <paramref name="masterLevel"/> to the next.</summary>
        public static long ExperienceToNext(int masterLevel) =>
            (long)Math.Round(BaseMasterExperience * (1.0 + MasterExperienceGrowth * Math.Max(0, masterLevel)));

        /// <summary>
        /// Adds kill XP to the master bar (only when <see cref="IsMaster"/>). Returns master levels gained; XP past
        /// <see cref="MaxMasterLevel"/> is dropped (the bar stays full).
        /// </summary>
        public static int AddExperience(GraduationRecord record, int heroLevel, long amount)
        {
            if (!IsMaster(record, heroLevel) || amount <= 0 || record.masterLevel >= MaxMasterLevel)
            {
                return 0;
            }

            int gained = 0;
            record.masterExperience += amount;
            while (record.masterLevel < MaxMasterLevel && record.masterExperience >= ExperienceToNext(record.masterLevel))
            {
                record.masterExperience -= ExperienceToNext(record.masterLevel);
                record.masterLevel++;
                gained++;
            }

            if (record.masterLevel >= MaxMasterLevel)
            {
                record.masterExperience = 0;
            }

            return gained;
        }

        public static int RankOf(GraduationRecord record, string nodeId)
        {
            if (record?.masterNodes == null)
            {
                return 0;
            }

            foreach (MasterNodeRank entry in record.masterNodes)
            {
                if (entry != null && entry.nodeId == nodeId)
                {
                    return entry.rank;
                }
            }

            return 0;
        }

        public static int SpentPoints(GraduationRecord record)
        {
            int total = 0;
            if (record?.masterNodes != null)
            {
                foreach (MasterNodeRank entry in record.masterNodes)
                {
                    total += entry != null ? Math.Max(0, entry.rank) : 0;
                }
            }

            return total;
        }

        public static int AvailablePoints(GraduationRecord record) =>
            record == null ? 0 : Math.Max(0, record.masterLevel * PointsPerMasterLevel - SpentPoints(record));

        public static MasterSpendResult CanSpend(GraduationRecord record, int heroLevel, string nodeId)
        {
            if (!IsMaster(record, heroLevel))
            {
                return MasterSpendResult.NotMaster;
            }

            MasterNodeSpec node = GetNode(nodeId);
            if (node == null)
            {
                return MasterSpendResult.UnknownNode;
            }

            if (AvailablePoints(record) <= 0)
            {
                return MasterSpendResult.NoPoints;
            }

            if (RankOf(record, nodeId) >= node.MaxRank)
            {
                return MasterSpendResult.MaxRank;
            }

            if (node.RequiredNodeId != null && RankOf(record, node.RequiredNodeId) < node.RequiredRank)
            {
                return MasterSpendResult.Locked;
            }

            return MasterSpendResult.Spent;
        }

        public static MasterSpendResult Spend(GraduationRecord record, int heroLevel, string nodeId)
        {
            MasterSpendResult result = CanSpend(record, heroLevel, nodeId);
            if (result != MasterSpendResult.Spent)
            {
                return result;
            }

            foreach (MasterNodeRank entry in record.masterNodes)
            {
                if (entry != null && entry.nodeId == nodeId)
                {
                    entry.rank++;
                    return result;
                }
            }

            record.masterNodes.Add(new MasterNodeRank { nodeId = nodeId, rank = 1 });
            return result;
        }

        /// <summary>Gives every spent point back (Vó Edna's "começar de novo"). Returns the points refunded.</summary>
        public static int Reset(GraduationRecord record)
        {
            int spent = SpentPoints(record);
            record?.masterNodes?.Clear();
            return spent;
        }

        /// <summary>Total bonus of one stat from the tree (sum of rank x value of every node on it).</summary>
        public static float Bonus(GraduationRecord record, StatType stat)
        {
            float total = 0f;
            foreach (MasterNodeSpec node in Nodes)
            {
                if (node.Stat == stat)
                {
                    total += node.ValuePerRank * Math.Min(node.MaxRank, RankOf(record, node.NodeId));
                }
            }

            return total;
        }

        /// <summary>The modifiers the tree puts on the hero (one per node with ranks), for the BuffController.</summary>
        public static List<StatModifierDefinition> Modifiers(GraduationRecord record)
        {
            var list = new List<StatModifierDefinition>();
            foreach (MasterNodeSpec node in Nodes)
            {
                int rank = Math.Min(node.MaxRank, RankOf(record, node.NodeId));
                if (rank <= 0)
                {
                    continue;
                }

                list.Add(new StatModifierDefinition
                {
                    displayName = node.NodeId,
                    affectedStat = node.Stat,
                    application = node.Application,
                    value = node.ValuePerRank * rank,
                    durationSeconds = 0f,
                    isDebuff = false,
                });
            }

            return list;
        }

        /// <summary>Repairs the master part of a saved record: level 0..max, XP in range, ranks capped, unknown nodes dropped,
        /// and never more points spent than earned (extra ranks are trimmed from the last nodes).</summary>
        public static void Sanitize(GraduationRecord record)
        {
            if (record == null)
            {
                return;
            }

            record.masterLevel = Math.Max(0, Math.Min(MaxMasterLevel, record.masterLevel));
            record.masterExperience = record.masterLevel >= MaxMasterLevel ? 0 : Math.Max(0, Math.Min(record.masterExperience, ExperienceToNext(record.masterLevel) - 1));
            record.masterNodes ??= new List<MasterNodeRank>();
            record.masterNodes.RemoveAll(entry => entry == null || GetNode(entry.nodeId) == null || entry.rank <= 0);
            var seen = new HashSet<string>();
            record.masterNodes.RemoveAll(entry => !seen.Add(entry.nodeId));
            foreach (MasterNodeRank entry in record.masterNodes)
            {
                entry.rank = Math.Min(GetNode(entry.nodeId).MaxRank, entry.rank);
            }

            int over = SpentPoints(record) - record.masterLevel * PointsPerMasterLevel;
            for (int i = record.masterNodes.Count - 1; i >= 0 && over > 0; i--)
            {
                int take = Math.Min(over, record.masterNodes[i].rank);
                record.masterNodes[i].rank -= take;
                over -= take;
            }

            record.masterNodes.RemoveAll(entry => entry.rank <= 0);
        }
    }
}
