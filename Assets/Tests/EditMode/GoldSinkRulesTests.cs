using NUnit.Framework;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.Items.Durability;
using Rapadura.Gameplay.Refining;
using Rapadura.Gameplay.Talents;

namespace Rapadura.Tests.EditMode
{
    /// <summary>
    /// The gold destinations above level 100 (Docs/Balanco100a500.md §5.3, sugestão 3), pure rules only: the ferreiro's
    /// refine fee grows with the hero level, the repair prices a piece at most at <see cref="DurabilityRules.RepairValueCap"/>
    /// and the talent reset gets dearer per point above the 44 points of level 100. Mirrored in tools/balance/economy.py.
    /// </summary>
    public class GoldSinkRulesTests
    {
        [Test]
        public void RefineFee_IsTheCampaignPriceUpTo100_AndGrowsWithTheHeroLevel()
        {
            for (int level = 0; level <= RefineRules.MaxLevel; level++)
            {
                Assert.AreEqual(RefineRules.GoldCostFor(level), RefineRules.FeeFor(level, 1), $"+{level} at hero 1");
                Assert.AreEqual(RefineRules.GoldCostFor(level), RefineRules.FeeFor(level, RefineRules.FeeLevelStart), $"+{level} at hero 100");
            }

            Assert.AreEqual(0, RefineRules.FeeFor(8, 500), "+6..+9 still take the stone alone");
            Assert.AreEqual(100, RefineRules.FeeFor(14, 225), "×2 at 225");
            Assert.AreEqual(210, RefineRules.FeeFor(14, 500), "×4.2 at 500");
            Assert.AreEqual(42, RefineRules.FeeFor(9, 500));
            Assert.AreEqual(11, RefineRules.FeeFor(9, 101), "rounded up");
            Assert.Greater(RefineRules.FeeFor(12, 400), RefineRules.FeeFor(12, 300));
        }

        [Test]
        public void RefineFee_GatesTheCheck_AndIsWhatTheAttemptReports()
        {
            int fee = RefineRules.FeeFor(12, 400);
            Assert.AreEqual(RefineCheck.NotEnoughGold, RefineRules.Check(true, 12, 1, fee - 1, false, 0, 400));
            Assert.AreEqual(RefineCheck.Ok, RefineRules.Check(true, 12, 1, fee, false, 0, 400));
            Assert.AreEqual(RefineCheck.Ok, RefineRules.Check(true, 12, 1, RefineRules.GoldCostFor(12), false, 0), "old overload = campaign price");
            Assert.AreEqual(fee, RefineRules.Attempt(12, false, 7, false, 400).Gold);
            Assert.AreEqual(RefineRules.GoldCostFor(12), RefineRules.Attempt(12, false, 7).Gold);
        }

        [Test]
        public void Repair_PricesAPieceAtMostAtTheCap()
        {
            int capped = DurabilityRules.FullRepairCost(DurabilityRules.RepairValueCap, ItemRarity.Epic, 9, false);
            Assert.AreEqual(capped, DurabilityRules.FullRepairCost(10400, ItemRarity.Epic, 9, false), "a set piece of 10 400 repairs like one of the cap");
            Assert.AreEqual(190, capped, "200 × 0.25 × 2 × 1.9");
            Assert.Less(DurabilityRules.FullRepairCost(100, ItemRarity.Epic, 9, false), capped, "below the cap the value still counts");
            Assert.AreEqual(25, DurabilityRules.FullRepairCost(100, ItemRarity.Common, 0, false), "campaign gear unchanged");
        }

        [Test]
        public void Respec_GetsDearerPerPointAboveLevel100Points()
        {
            Assert.AreEqual(0, TalentWheelRules.RespecCostFor(0));
            Assert.AreEqual(TalentWheelRules.RespecMinimumGold, TalentWheelRules.RespecCostFor(2));
            Assert.AreEqual(44 * TalentWheelRules.RespecGoldPerPoint, TalentWheelRules.RespecCostFor(TalentWheelRules.RespecHighPointsStart), "level 100: as before");
            Assert.AreEqual(TalentWheelRules.EarnedPoints(100), TalentWheelRules.RespecHighPointsStart);
            Assert.AreEqual(6080, TalentWheelRules.RespecCostFor(TalentWheelRules.EarnedPoints(300)));
            Assert.AreEqual(14700, TalentWheelRules.RespecCostFor(TalentWheelRules.EarnedPoints(500)));
        }
    }
}
