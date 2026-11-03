using System;
using System.Collections.Generic;
using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.Skills;

namespace Rapadura.Gameplay.ClassEvolution
{
    /// <summary>The lines of the master tree (Docs/ClassEvolution.md §4).</summary>
    public enum MasterTreeBranch
    {
        /// <summary>The 6 nodes of the level-50 era: health, damage, mana, defence, critical, speed (30 points).</summary>
        Core = 0,
        /// <summary>"Apoio": the graduation group buff reaches further and lasts longer on allies, health and defence.</summary>
        Support = 1,
        /// <summary>"Utilidade": stamina, mana and master XP. Never damage.</summary>
        Utility = 2,
        /// <summary>"Laço": bonuses for the mount and for Ari. Never damage.</summary>
        Bond = 3
    }

    /// <summary>What a master node gives: a hero stat modifier, or one of the special bonuses read by other systems.</summary>
    public enum MasterNodeEffect
    {
        /// <summary>A <see cref="StatModifierDefinition"/> on the hero (<see cref="MasterLevelRules.Modifiers"/>).</summary>
        HeroStat = 0,
        /// <summary>Metres added to the radius a graduation group buff spreads to allies.</summary>
        GroupBuffRadius = 1,
        /// <summary>Fraction added to the duration of the group-buff copy allies get.</summary>
        GroupBuffDuration = 2,
        /// <summary>Fraction added to the master XP of each kill.</summary>
        MasterExperience = 3,
        /// <summary>Fraction added to the max stamina of the mount (for the mount code, through <see cref="MasterLevelRules.EffectBonus"/>).</summary>
        MountStamina = 4,
        /// <summary>Fraction added to the combat XP of the mount (same hook).</summary>
        MountExperience = 5,
        /// <summary>Fraction added to the radius Ari barks at a creature winding up an attack.</summary>
        CompanionAlertRadius = 6,
        /// <summary>Fraction added to the radius Ari goes fetching drops in.</summary>
        CompanionFetchRadius = 7
    }

    /// <summary>One node of the master tree ("Árvore de Mestre"): a small bonus per rank.</summary>
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
        public readonly MasterTreeBranch Branch;
        /// <summary><see cref="MasterNodeEffect.HeroStat"/> uses <see cref="Stat"/>/<see cref="Application"/>; the others ignore them.</summary>
        public readonly MasterNodeEffect Effect;

        /// <summary>A hero-stat node. Keep each <c>new MasterNodeSpec(...)</c> on one line, positional (power_stack.py reads it).</summary>
        public MasterNodeSpec(string nodeId, StatType stat, ModifierApplication application, float valuePerRank, int maxRank,
            string requiredNodeId, int requiredRank, MasterTreeBranch branch = MasterTreeBranch.Core)
            : this(nodeId, MasterNodeEffect.HeroStat, stat, application, valuePerRank, maxRank, requiredNodeId, requiredRank, branch)
        {
        }

        private MasterNodeSpec(string nodeId, MasterNodeEffect effect, StatType stat, ModifierApplication application, float valuePerRank,
            int maxRank, string requiredNodeId, int requiredRank, MasterTreeBranch branch)
        {
            NodeId = nodeId;
            Effect = effect;
            Stat = stat;
            Application = application;
            ValuePerRank = valuePerRank;
            MaxRank = maxRank;
            RequiredNodeId = requiredNodeId;
            RequiredRank = requiredRank;
            Branch = branch;
        }

        /// <summary>A special node (not a hero stat). Keep each <c>Special(...)</c> on one line, positional (power_stack.py counts it).</summary>
        public static MasterNodeSpec Special(string nodeId, MasterNodeEffect effect, MasterTreeBranch branch, float valuePerRank, int maxRank,
            string requiredNodeId, int requiredRank) =>
            new MasterNodeSpec(nodeId, effect, default, ModifierApplication.Flat, valuePerRank, maxRank, requiredNodeId, requiredRank, branch);

        public bool IsHeroStat => Effect == MasterNodeEffect.HeroStat;

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
    /// with the last graduation (G5) at level <see cref="RequiredLevel"/> stops gaining normal XP and fills a separate master
    /// bar instead; each master level gives one point for a tree of modest bonuses (4 lines, 100 points; outside the 30-point
    /// core nothing adds damage). Pure: the server owns the record.
    /// <para>
    /// The gate moved from level 50 to the 500 cap (Docs/Progressao500.md §3.3). Saves from the level-50 era keep everything:
    /// a cousin with master levels earned then (tier ≥ 3, hero level ≥ <see cref="LegacyRequiredLevel"/>, master level ≥ 1) is a
    /// "kept master" (<see cref="HasKeptMasterProgress"/>): the tree bonuses stay on and the earned points can still be spent
    /// or reset (<see cref="CanUseTree"/>), but kill XP goes to normal levels again (51+) until level 500, where the master bar
    /// starts filling again from where it stopped. Nothing in the record is rewritten.
    /// </para>
    /// </summary>
    public static class MasterLevelRules
    {
        /// <summary>The hero level cap (= <see cref="LevelCurveMath.MaxLevel"/>, 500): master XP only from here.</summary>
        public const int RequiredLevel = LevelCurveMath.MaxLevel;
        /// <summary>The old gate (level 50, before the 500 cap). Only used to recognise master progress earned on old saves.</summary>
        public const int LegacyRequiredLevel = 50;
        /// <summary>The last graduation (G5 "da Coluna", <see cref="ClassEvolutionCatalog.MaxTier"/>).</summary>
        public const int RequiredTier = ClassEvolutionCatalog.MaxTier;
        /// <summary>The old tier gate (the 3rd graduation). Only used to recognise master progress earned on old saves.</summary>
        public const int LegacyRequiredTier = ClassEvolutionCatalog.CampaignMaxTier;
        /// <summary>100 = the whole tree (30 core points + the Support, Utility and Bond lines, Docs/Progressao500.md §3.3).</summary>
        public const int MaxMasterLevel = 100;
        public const int PointsPerMasterLevel = 1;
        /// <summary>Master XP weighs like a level at the cap: xp(500) × 0.5 × (1 + 0.05 × master level) (≈ 10.7 million at master 0).</summary>
        public const double MasterExperienceShareOfCapLevel = 0.5;
        public const double MasterExperienceGrowth = 0.05;

        /// <summary>
        /// Core: row 1 has no requirement, each row-2 node needs 3 ranks in the node above it (30 points). Support, Utility and
        /// Bond: each column starts free and the next node needs 3 ranks in the one before it (25 + 25 + 20 points). Full tree =
        /// 100. Nothing outside the core touches damage, critical, attack speed or cooldowns (tools/balance: boss TTK ≥ 95%).
        /// </summary>
        public static readonly IReadOnlyList<MasterNodeSpec> Nodes = new[]
        {
            new MasterNodeSpec("couro_curtido", StatType.MaxHealth, ModifierApplication.PercentAdditive, 0.01f, 5, null, 0),
            new MasterNodeSpec("mao_firme", StatType.AttackDamage, ModifierApplication.PercentAdditive, 0.005f, 5, null, 0),
            new MasterNodeSpec("folego_de_vaqueiro", StatType.ManaRegen, ModifierApplication.PercentAdditive, 0.02f, 5, null, 0),
            new MasterNodeSpec("pe_no_chao", StatType.Defense, ModifierApplication.PercentAdditive, 0.006f, 5, "couro_curtido", 3),
            new MasterNodeSpec("olho_vivo", StatType.CriticalChance, ModifierApplication.Flat, 0.004f, 5, "mao_firme", 3),
            new MasterNodeSpec("passo_ligeiro", StatType.MoveSpeed, ModifierApplication.PercentAdditive, 0.01f, 5, "folego_de_vaqueiro", 3),

            MasterNodeSpec.Special("luz_partilhada", MasterNodeEffect.GroupBuffRadius, MasterTreeBranch.Support, 0.4f, 5, null, 0),
            MasterNodeSpec.Special("hino_do_chamado", MasterNodeEffect.GroupBuffDuration, MasterTreeBranch.Support, 0.04f, 5, "luz_partilhada", 3),
            new MasterNodeSpec("agua_viva", StatType.HealthRegen, ModifierApplication.Flat, 0.2f, 5, null, 0, MasterTreeBranch.Support),
            new MasterNodeSpec("manto_de_luz", StatType.Defense, ModifierApplication.PercentAdditive, 0.004f, 5, "agua_viva", 3, MasterTreeBranch.Support),
            new MasterNodeSpec("abraco_da_familia", StatType.MaxHealth, ModifierApplication.PercentAdditive, 0.005f, 5, "manto_de_luz", 3, MasterTreeBranch.Support),

            new MasterNodeSpec("passo_de_peregrino", StatType.MaxStamina, ModifierApplication.PercentAdditive, 0.02f, 5, null, 0, MasterTreeBranch.Utility),
            new MasterNodeSpec("folego_do_ouvinte", StatType.StaminaRegen, ModifierApplication.PercentAdditive, 0.02f, 5, "passo_de_peregrino", 3, MasterTreeBranch.Utility),
            new MasterNodeSpec("taca_do_bema", StatType.MaxMana, ModifierApplication.PercentAdditive, 0.01f, 5, null, 0, MasterTreeBranch.Utility),
            MasterNodeSpec.Special("memoria_do_ouvinte", MasterNodeEffect.MasterExperience, MasterTreeBranch.Utility, 0.02f, 5, "taca_do_bema", 3),
            new MasterNodeSpec("fonte_serena", StatType.ManaRegen, ModifierApplication.PercentAdditive, 0.01f, 5, "memoria_do_ouvinte", 3, MasterTreeBranch.Utility),

            MasterNodeSpec.Special("laco_da_sela", MasterNodeEffect.MountStamina, MasterTreeBranch.Bond, 0.02f, 5, null, 0),
            MasterNodeSpec.Special("redea_de_luz", MasterNodeEffect.MountExperience, MasterTreeBranch.Bond, 0.03f, 5, "laco_da_sela", 3),
            MasterNodeSpec.Special("faro_do_ari", MasterNodeEffect.CompanionAlertRadius, MasterTreeBranch.Bond, 0.06f, 5, null, 0),
            MasterNodeSpec.Special("ari_ligeiro", MasterNodeEffect.CompanionFetchRadius, MasterTreeBranch.Bond, 0.06f, 5, "faro_do_ari", 3),
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

        /// <summary>The master bar fills: last graduation (G5) and the 500 cap.</summary>
        public static bool IsMaster(GraduationRecord record, int heroLevel) =>
            record != null && record.tier >= RequiredTier && heroLevel >= RequiredLevel;

        /// <summary>
        /// Master levels earned under the old level-50 gate (save migration, see the class summary): kept, usable, not
        /// growing until <see cref="RequiredLevel"/>.
        /// </summary>
        public static bool HasKeptMasterProgress(GraduationRecord record, int heroLevel) =>
            record != null && record.masterLevel > 0 && record.tier >= LegacyRequiredTier && heroLevel >= LegacyRequiredLevel;

        /// <summary>The tree can be spent/reset: a master, or a cousin with kept master progress from an old save.</summary>
        public static bool CanUseTree(GraduationRecord record, int heroLevel) =>
            IsMaster(record, heroLevel) || HasKeptMasterProgress(record, heroLevel);

        /// <summary>XP of the level at the cap on the shipped curve (21 344 000): the master bar's unit.</summary>
        public static long CapLevelExperience => LevelCurveMath.ShippedExperienceToNext(LevelCurveMath.MaxLevel);

        /// <summary>Master XP from master level <paramref name="masterLevel"/> to the next.</summary>
        public static long ExperienceToNext(int masterLevel) =>
            (long)Math.Round(CapLevelExperience * MasterExperienceShareOfCapLevel * (1.0 + MasterExperienceGrowth * Math.Max(0, masterLevel)));

        /// <summary>
        /// Adds kill XP to the master bar (only when <see cref="IsMaster"/>), plus the "Memória do Ouvinte" bonus. Returns master levels gained; XP past
        /// <see cref="MaxMasterLevel"/> is dropped (the bar stays full).
        /// </summary>
        public static int AddExperience(GraduationRecord record, int heroLevel, long amount)
        {
            if (!IsMaster(record, heroLevel) || amount <= 0 || record.masterLevel >= MaxMasterLevel)
            {
                return 0;
            }

            int gained = 0;
            float bonus = EffectBonus(record, MasterNodeEffect.MasterExperience);
            record.masterExperience += bonus > 0f && amount < long.MaxValue / 4 ? (long)Math.Round(amount * (1.0 + bonus)) : amount;
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
            if (!CanUseTree(record, heroLevel))
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
                if (node.IsHeroStat && node.Stat == stat)
                {
                    total += node.ValuePerRank * Math.Min(node.MaxRank, RankOf(record, node.NodeId));
                }
            }

            return total;
        }

        /// <summary>Total of one special effect (sum of rank x value of every node with it); 0 without ranks.</summary>
        public static float EffectBonus(GraduationRecord record, MasterNodeEffect effect)
        {
            float total = 0f;
            if (record == null || effect == MasterNodeEffect.HeroStat)
            {
                return total;
            }

            foreach (MasterNodeSpec node in Nodes)
            {
                if (node.Effect == effect)
                {
                    total += node.ValuePerRank * Math.Min(node.MaxRank, RankOf(record, node.NodeId));
                }
            }

            return total;
        }

        /// <summary>Points the whole tree takes (100 = <see cref="MaxMasterLevel"/>).</summary>
        public static int TreePoints
        {
            get
            {
                int total = 0;
                foreach (MasterNodeSpec node in Nodes)
                {
                    total += node.MaxRank;
                }

                return total;
            }
        }

        /// <summary>The modifiers the tree puts on the hero (one per node with ranks), for the BuffController.</summary>
        public static List<StatModifierDefinition> Modifiers(GraduationRecord record)
        {
            var list = new List<StatModifierDefinition>();
            foreach (MasterNodeSpec node in Nodes)
            {
                int rank = Math.Min(node.MaxRank, RankOf(record, node.NodeId));
                if (rank <= 0 || !node.IsHeroStat)
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
