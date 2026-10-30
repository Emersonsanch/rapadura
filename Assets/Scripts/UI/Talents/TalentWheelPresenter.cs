using System;
using System.Collections.Generic;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Talents;
using UnityEngine;

namespace Rapadura.UI.Talents
{
    /// <summary>How a wheel node is drawn.</summary>
    public enum TalentNodeState
    {
        Locked = 0,
        Available = 1,
        Learned = 2,
        Maxed = 3
    }

    /// <summary>
    /// Pure layout/state/text of the "Roda de Talentos" (<see cref="TalentWheelView"/>) and the sheet's "Treino" rows, so
    /// EditMode tests check them without a panel. Quadrants clockwise from the top-right: Ataque, Defesa, Apoio, Utilidade;
    /// ring 1 near the centre, the two ring-2 nodes either side of the quadrant's middle, the keystone on the rim.
    /// </summary>
    public static class TalentWheelPresenter
    {
        public const float WheelSize = 440f;
        public const float Ring1Radius = 70f;
        public const float Ring2Radius = 130f;
        public const float KeystoneRadius = 180f;
        public const float LabelRadius = 112f;
        public const float Ring2SpreadDegrees = 22f;

        /// <summary>Middle of the quadrant, in degrees clockwise from straight up.</summary>
        public static float QuadrantAngle(TalentQuadrant quadrant) => 45f + 90f * (int)quadrant;

        /// <summary>Centre of a node inside the wheel box (0,0 = top-left). <paramref name="indexInRing"/>: 0 or 1 on ring 2.</summary>
        public static Vector2 NodeCenter(TalentNodeSpec node, int indexInRing)
        {
            float angle = QuadrantAngle(node.Quadrant);
            float radius;
            switch (node.Ring)
            {
                case 1:
                    radius = Ring1Radius;
                    break;
                case 2:
                    radius = Ring2Radius;
                    angle += indexInRing == 0 ? -Ring2SpreadDegrees : Ring2SpreadDegrees;
                    break;
                default:
                    radius = KeystoneRadius;
                    break;
            }

            return Polar(angle, radius);
        }

        /// <summary>Where the quadrant's name sits: between the rings, on the side away from the nodes' ring-2 pair.</summary>
        public static Vector2 LabelCenter(TalentQuadrant quadrant) => Polar(QuadrantAngle(quadrant), LabelRadius);

        public static Vector2 Polar(float degreesClockwiseFromUp, float radius)
        {
            float rad = degreesClockwiseFromUp * Mathf.Deg2Rad;
            float c = WheelSize / 2f;
            return new Vector2(c + radius * Mathf.Sin(rad), c - radius * Mathf.Cos(rad));
        }

        /// <summary>The nodes of <paramref name="hero"/> with their index inside their quadrant's ring (for <see cref="NodeCenter"/>).</summary>
        public static List<(TalentNodeSpec Node, int IndexInRing)> Layout(CharacterId hero)
        {
            var list = new List<(TalentNodeSpec, int)>();
            var seen = new Dictionary<(TalentQuadrant, int), int>();
            foreach (TalentNodeSpec node in TalentWheelCatalog.NodesOf(hero))
            {
                seen.TryGetValue((node.Quadrant, node.Ring), out int index);
                seen[(node.Quadrant, node.Ring)] = index + 1;
                list.Add((node, index));
            }

            return list;
        }

        public static TalentNodeState State(TalentRecord record, CharacterId hero, int heroLevel, TalentNodeSpec node)
        {
            int rank = TalentWheelRules.RankOf(record, node.NodeId);
            if (rank >= node.MaxRank)
            {
                return TalentNodeState.Maxed;
            }

            if (rank > 0)
            {
                return TalentNodeState.Learned;
            }

            TalentSpendResult can = TalentWheelRules.CanSpend(record, hero, heroLevel, node.NodeId);
            return can == TalentSpendResult.Spent || can == TalentSpendResult.NoPoints ? TalentNodeState.Available : TalentNodeState.Locked;
        }

        /// <summary>"2/3" on the node.</summary>
        public static string RankText(TalentRecord record, TalentNodeSpec node, LocalizationManager manager) =>
            TalentTexts.Format(manager, TalentTexts.Rank, TalentWheelRules.RankOf(record, node.NodeId), node.MaxRank);

        /// <summary>"+0,2% dano por ponto (máx. +0,6% dano)"-like line: the next-rank bonus and the node's total.</summary>
        public static string BonusText(TalentNodeSpec node, LocalizationManager manager) =>
            TalentTexts.FormatBonus(manager, node.Stat, node.MaxValue);

        /// <summary>Why the node can't take a point right now (empty when it can).</summary>
        public static string RequirementText(TalentRecord record, CharacterId hero, int heroLevel, TalentNodeSpec node, LocalizationManager manager)
        {
            switch (TalentWheelRules.CanSpend(record, hero, heroLevel, node.NodeId))
            {
                case TalentSpendResult.Locked:
                    return TalentTexts.Format(manager, TalentTexts.LockedLevel, TalentWheelCatalog.UnlockLevel);
                case TalentSpendResult.NoPoints:
                    return TalentTexts.Get(manager, TalentTexts.NoPoints);
                case TalentSpendResult.RequirementMissing:
                    if (node.RequiredNodeId != null && TalentWheelRules.RankOf(record, node.RequiredNodeId) < node.RequiredRank)
                    {
                        return TalentTexts.Format(manager, TalentTexts.NeedsNode, node.RequiredRank,
                            TalentTexts.Get(manager, TalentTexts.NodeNameKey(node.RequiredNodeId)));
                    }

                    return TalentTexts.Format(manager, TalentTexts.NeedsQuadrant, node.RequiredQuadrantPoints,
                        TalentTexts.Get(manager, TalentTexts.QuadrantKey(node.Quadrant)));
                default:
                    return string.Empty;
            }
        }

        /// <summary>"Golpe 7 (42%)" / "Golpe 20 (máx.)" + the bonus it gives now.</summary>
        public static string TrainingText(ProficiencyEntry entry, LocalizationManager manager)
        {
            if (entry == null || !ProficiencyRules.TryParse(entry.kind, out ProficiencyKind kind))
            {
                return string.Empty;
            }

            string name = TalentTexts.Get(manager, TalentTexts.ProficiencyKey(kind));
            string head = entry.level >= ProficiencyRules.MaxLevel
                ? TalentTexts.Format(manager, TalentTexts.TrainingMax, name, entry.level)
                : TalentTexts.Format(manager, TalentTexts.TrainingRow, name, entry.level, (int)Math.Floor(ProficiencyRules.Progress(entry) * 100f));
            ProficiencySpec spec = ProficiencyRules.GetSpec(kind);
            return entry.level > 0 ? head + "  " + TalentTexts.FormatBonus(manager, spec.Stat, ProficiencyRules.Bonus(kind, entry.level)) : head;
        }
    }
}
