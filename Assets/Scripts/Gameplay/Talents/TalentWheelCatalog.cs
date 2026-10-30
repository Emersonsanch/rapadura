using System.Collections.Generic;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Skills;

namespace Rapadura.Gameplay.Talents
{
    /// <summary>The four quadrants of the "Roda de Talentos" (Tibia's Wheel of Destiny), clockwise from the top.</summary>
    public enum TalentQuadrant
    {
        Offence = 0,
        Defence = 1,
        Support = 2,
        Utility = 3
    }

    /// <summary>
    /// One node of the wheel. Ring 1 (inner) is open from the start, ring 2 needs <see cref="RequiredRank"/> ranks in the
    /// ring-1 node of its quadrant, the keystone (ring 3, one per cousin per quadrant) needs <see cref="RequiredQuadrantPoints"/>
    /// points already spent in the quadrant. Each rank costs <see cref="CostPerRank"/> points.
    /// </summary>
    public sealed class TalentNodeSpec
    {
        public readonly string NodeId;
        public readonly TalentQuadrant Quadrant;
        public readonly int Ring;
        /// <summary>Null = every cousin has it; a keystone belongs to one cousin.</summary>
        public readonly CharacterId? Hero;
        public readonly StatType Stat;
        public readonly ModifierApplication Application;
        public readonly float ValuePerRank;
        public readonly int MaxRank;
        public readonly int CostPerRank;
        public readonly string RequiredNodeId;
        public readonly int RequiredRank;
        public readonly int RequiredQuadrantPoints;

        public TalentNodeSpec(string nodeId, TalentQuadrant quadrant, int ring, CharacterId? hero, StatType stat,
            ModifierApplication application, float valuePerRank, int maxRank, int costPerRank, string requiredNodeId,
            int requiredRank, int requiredQuadrantPoints)
        {
            NodeId = nodeId;
            Quadrant = quadrant;
            Ring = ring;
            Hero = hero;
            Stat = stat;
            Application = application;
            ValuePerRank = valuePerRank;
            MaxRank = maxRank;
            CostPerRank = costPerRank;
            RequiredNodeId = requiredNodeId;
            RequiredRank = requiredRank;
            RequiredQuadrantPoints = requiredQuadrantPoints;
        }

        public bool IsKeystone => Ring == TalentWheelCatalog.KeystoneRing;
        public string NameKey => "talent.node." + NodeId + ".name";
        public float MaxValue => ValuePerRank * MaxRank;
        public int MaxCost => CostPerRank * MaxRank;
    }

    /// <summary>
    /// The wheel's nodes (Docs/Balance/README.md §26). Every cousin shares the 12 small nodes (3 per quadrant) and has one
    /// keystone per quadrant. A quadrant holds <see cref="QuadrantCapacity"/> points, the whole wheel 48, and a cousin earns
    /// 39 by level 50: the build is a choice. The offence quadrant alone is capped (12 points), so the extra damage it can
    /// give never grows with level past it — that is what keeps the wheel inside the power budget.
    /// </summary>
    public static class TalentWheelCatalog
    {
        /// <summary>The wheel opens at this hero level with 1 point; +1 per level after (39 at level 50).</summary>
        public const int UnlockLevel = 12;
        public const int PointsPerLevel = 1;
        public const int KeystoneRing = 3;
        public const int SmallNodeRanks = 3;
        public const int Ring2RequiredRank = 2;
        public const int KeystoneCost = 3;
        public const int KeystoneRequiredPoints = 6;
        public const int QuadrantCapacity = SmallNodeRanks * 3 + KeystoneCost;

        private const ModifierApplication Pct = ModifierApplication.PercentAdditive;
        private const ModifierApplication Add = ModifierApplication.Flat;

        private static TalentNodeSpec Small(string id, TalentQuadrant quadrant, int ring, StatType stat, ModifierApplication application, float value, string required) =>
            new TalentNodeSpec(id, quadrant, ring, null, stat, application, value, SmallNodeRanks, 1, required, required != null ? Ring2RequiredRank : 0, 0);

        private static TalentNodeSpec Keystone(string id, TalentQuadrant quadrant, CharacterId hero, StatType stat, ModifierApplication application, float value) =>
            new TalentNodeSpec(id, quadrant, KeystoneRing, hero, stat, application, value, 1, KeystoneCost, null, 0, KeystoneRequiredPoints);

        public static readonly IReadOnlyList<TalentNodeSpec> Nodes = new[]
        {
            // Ataque
            Small("mao_certeira", TalentQuadrant.Offence, 1, StatType.AttackDamage, Pct, 0.002f, null),
            Small("olho_de_gaviao", TalentQuadrant.Offence, 2, StatType.CriticalChance, Add, 0.002f, "mao_certeira"),
            Small("golpe_pesado", TalentQuadrant.Offence, 2, StatType.CriticalDamage, Add, 0.01f, "mao_certeira"),
            Keystone("aboio_de_guerra", TalentQuadrant.Offence, CharacterId.Joaquim, StatType.AttackDamage, Pct, 0.01f),
            Keystone("prece_ardente", TalentQuadrant.Offence, CharacterId.Maria, StatType.CriticalDamage, Add, 0.05f),
            Keystone("flecha_do_carcara", TalentQuadrant.Offence, CharacterId.Maithe, StatType.CriticalChance, Add, 0.01f),
            Keystone("engrenagem_afiada", TalentQuadrant.Offence, CharacterId.Icaro, StatType.CooldownReduction, Add, 0.01f),
            Keystone("chama_do_vagalume", TalentQuadrant.Offence, CharacterId.Lavine, StatType.AttackDamage, Pct, 0.01f),

            // Defesa
            Small("couro_grosso", TalentQuadrant.Defence, 1, StatType.MaxHealth, Pct, 0.01f, null),
            Small("pele_de_mandacaru", TalentQuadrant.Defence, 2, StatType.Defense, Pct, 0.01f, "couro_grosso"),
            Small("espinho_de_xiquexique", TalentQuadrant.Defence, 2, StatType.Thorns, Add, 0.003f, "couro_grosso"),
            Keystone("gibao_de_couro", TalentQuadrant.Defence, CharacterId.Joaquim, StatType.MaxHealth, Pct, 0.04f),
            Keystone("manto_de_reza", TalentQuadrant.Defence, CharacterId.Maria, StatType.Defense, Pct, 0.03f),
            Keystone("pena_ligeira", TalentQuadrant.Defence, CharacterId.Maithe, StatType.Defense, Pct, 0.03f),
            Keystone("blindagem_de_lata", TalentQuadrant.Defence, CharacterId.Icaro, StatType.MaxHealth, Pct, 0.04f),
            Keystone("luz_que_protege", TalentQuadrant.Defence, CharacterId.Lavine, StatType.MaxMana, Pct, 0.05f),

            // Apoio
            Small("cura_de_quintal", TalentQuadrant.Support, 1, StatType.HealthRegen, Pct, 0.05f, null),
            Small("folego_de_reza", TalentQuadrant.Support, 2, StatType.ManaRegen, Pct, 0.03f, "cura_de_quintal"),
            Small("mente_mansa", TalentQuadrant.Support, 2, StatType.ResourceCostReduction, Add, 0.005f, "cura_de_quintal"),
            Keystone("berrante_amigo", TalentQuadrant.Support, CharacterId.Joaquim, StatType.HealthRegen, Pct, 0.10f),
            Keystone("benzedura_forte", TalentQuadrant.Support, CharacterId.Maria, StatType.HealthRegen, Pct, 0.15f),
            Keystone("ninho_seguro", TalentQuadrant.Support, CharacterId.Maithe, StatType.ManaRegen, Pct, 0.08f),
            Keystone("pilha_reserva", TalentQuadrant.Support, CharacterId.Icaro, StatType.ResourceCostReduction, Add, 0.015f),
            Keystone("vaga_lume_sereno", TalentQuadrant.Support, CharacterId.Lavine, StatType.ManaRegen, Pct, 0.10f),

            // Utilidade
            Small("pe_ligeiro", TalentQuadrant.Utility, 1, StatType.MoveSpeed, Pct, 0.01f, null),
            Small("vigor_sertanejo", TalentQuadrant.Utility, 2, StatType.MaxStamina, Pct, 0.02f, "pe_ligeiro"),
            Small("folego_largo", TalentQuadrant.Utility, 2, StatType.StaminaRegen, Pct, 0.03f, "pe_ligeiro"),
            Keystone("trote_de_vaqueiro", TalentQuadrant.Utility, CharacterId.Joaquim, StatType.MaxStamina, Pct, 0.06f),
            Keystone("romeira_incansavel", TalentQuadrant.Utility, CharacterId.Maria, StatType.StaminaRegen, Pct, 0.08f),
            Keystone("voo_rasante", TalentQuadrant.Utility, CharacterId.Maithe, StatType.MoveSpeed, Pct, 0.03f),
            Keystone("bota_de_mola", TalentQuadrant.Utility, CharacterId.Icaro, StatType.MoveSpeed, Pct, 0.03f),
            Keystone("rastro_de_luz", TalentQuadrant.Utility, CharacterId.Lavine, StatType.StaminaRegen, Pct, 0.08f),
        };

        public static TalentNodeSpec GetNode(string nodeId)
        {
            foreach (TalentNodeSpec node in Nodes)
            {
                if (node.NodeId == nodeId)
                {
                    return node;
                }
            }

            return null;
        }

        public static bool BelongsTo(TalentNodeSpec node, CharacterId hero) => node != null && (!node.Hero.HasValue || node.Hero.Value == hero);

        /// <summary>The nodes <paramref name="hero"/> sees (12 small + 4 keystones), in catalog order.</summary>
        public static List<TalentNodeSpec> NodesOf(CharacterId hero)
        {
            var list = new List<TalentNodeSpec>();
            foreach (TalentNodeSpec node in Nodes)
            {
                if (BelongsTo(node, hero))
                {
                    list.Add(node);
                }
            }

            return list;
        }

        public static List<TalentNodeSpec> NodesOf(CharacterId hero, TalentQuadrant quadrant)
        {
            List<TalentNodeSpec> list = NodesOf(hero);
            list.RemoveAll(node => node.Quadrant != quadrant);
            return list;
        }
    }
}
