using NUnit.Framework;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.World.Map;
using Rapadura.Gameplay.World.Map.Areas;
using Rapadura.Gameplay.World.Map.Mechanics;
using Rapadura.Gameplay.World.Map.Overworld;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>The Oásis de Qocho getting its water back once the Root That Drank the Well is freed (Docs/Mapas100.md 09, <see cref="QochoOasisRules"/>).</summary>
    public class QochoOasisTests
    {
        [Test]
        public void Root_IsTheBossOfTheKarez()
        {
            Assert.AreEqual(RegionBossRules.WellDrinkingRootId, QochoOasisRules.RootEnemyId);
        }

        [Test]
        public void Mends_OnlyAfterTheRoot_OnTheShore_OutOfCombat()
        {
            Assert.IsTrue(QochoOasisRules.Mends(true, true, true, QochoOasisRules.OutOfCombatSeconds));
            Assert.IsFalse(QochoOasisRules.Mends(false, true, true, 60f), "the Root still drinks the karez");
            Assert.IsFalse(QochoOasisRules.Mends(true, true, false, 60f));
            Assert.IsFalse(QochoOasisRules.Mends(true, false, true, 60f));
            Assert.IsFalse(QochoOasisRules.Mends(true, true, true, 1f), "in a fight");

            Assert.AreEqual(3f, QochoOasisRules.MendOver(50f, 100f, QochoOasisRules.HealPerSecond, 1f), 1e-4f);
            Assert.AreEqual(1f, QochoOasisRules.MendOver(99f, 100f, QochoOasisRules.HealPerSecond, 1f), 1e-4f, "never past full");
            Assert.AreEqual(0f, QochoOasisRules.MendOver(10f, 0f, QochoOasisRules.HealPerSecond, 1f));
        }

        [Test]
        public void Shore_IsTheLake_EastOfTheTown()
        {
            Vector2 mid = (QochoOasisRules.LakeA + QochoOasisRules.LakeB) * 0.5f;
            Assert.IsTrue(QochoOasisRules.OnShore(mid));
            Assert.IsTrue(QochoOasisRules.OnShore(mid + new Vector2(0f, QochoOasisRules.LakeWidth * 0.5f + QochoOasisRules.ShoreReach - 0.5f)));
            Assert.IsFalse(QochoOasisRules.OnShore(mid + new Vector2(0f, 40f)));
            Assert.IsFalse(QochoOasisRules.OnShore(Vector2.zero), "the plaza of Qocho is not the shore");

            OverworldLayout w = OverworldLayouts.Get(OverworldLayouts.AreiasId);
            bool found = false;
            foreach (MapRiver river in w.World.Rivers)
            {
                if (Mathf.Approximately(river.Width, QochoOasisRules.LakeWidth) && river.Points.Count >= 2
                    && (river.Points[0] - QochoOasisRules.LakeA).sqrMagnitude < 1f && (river.Points[1] - QochoOasisRules.LakeB).sqrMagnitude < 1f)
                {
                    found = true;
                }
            }

            Assert.IsTrue(found, "the rules' lake is not the oasis river of the Areias layout any more");
            Assert.IsNull(OverworldRules.SettlementAt(w, mid), "the lake is outside the town");
        }

        [Test]
        public void Texts_EveryCousin_Translated()
        {
            Assert.AreEqual(QochoOasisTexts.LineIcaro, QochoOasisTexts.LineFor(CharacterId.Icaro));
            Assert.AreEqual(QochoOasisTexts.LineJoaquim, QochoOasisTexts.LineFor(null));
            foreach (var entry in QochoOasisTexts.Entries)
            {
                Assert.IsFalse(string.IsNullOrWhiteSpace(entry.En), entry.Key);
                Assert.IsFalse(string.IsNullOrWhiteSpace(entry.PtBR), entry.Key);
            }
        }
    }
}
