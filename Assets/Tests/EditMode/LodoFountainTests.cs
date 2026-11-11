using NUnit.Framework;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.World.Map.Areas;
using Rapadura.Gameplay.World.Map.Mechanics;
using Rapadura.Gameplay.World.Map.Overworld;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// The five Fontes do Lodo of the Coração do Lodo (Docs/Mapas100.md 35, <see cref="LodoFountainRules"/>): purifying, the two minutes lit,
    /// the mud that shrinks and bites, the prize of the day, the Lodo-que-Engole that waits for the five (<see cref="RegionBossRules.MireSummonReady"/>)
    /// and the places the layout gives them. Pure rules only.
    /// </summary>
    public class LodoFountainTests
    {
        private const float HeroRunSpeed = 5f;

        private static OverworldLayout Delta => OverworldLayouts.Get(OverworldLayouts.DeltaId);

        private static float[] Board(params float[] litUntil) => litUntil;

        [TearDown]
        public void ClearBoard() => LodoFountainBoard.Clear();

        [Test]
        public void Purify_LightsForTwoMinutes_AndRenewsOnlyNearTheEnd()
        {
            Assert.AreEqual(120f, LodoFountainRules.LitSeconds);
            float until = LodoFountainRules.LitUntilAfterPurify(10f);
            Assert.IsTrue(LodoFountainRules.IsLit(until, 10f + 119.9f));
            Assert.IsFalse(LodoFountainRules.IsLit(until, 10f + 120f));
            Assert.IsTrue(LodoFountainRules.CanPurify(float.NegativeInfinity, 0f), "a dark fountain can always be purified");
            Assert.IsFalse(LodoFountainRules.CanPurify(until, 10f + 30f), "fresh: nothing to renew");
            Assert.IsTrue(LodoFountainRules.CanPurify(until, 10f + LodoFountainRules.LitSeconds - LodoFountainRules.RenewWindowSeconds), "the last minute can be renewed");
        }

        [Test]
        public void LitCount_CountsOnlyTheLitOnes()
        {
            float[] board = Board(50f, float.NegativeInfinity, 5f, 200f, 9.99f);
            Assert.AreEqual(3, LodoFountainRules.LitCount(board, 5f));
            Assert.AreEqual(2, LodoFountainRules.LitCount(board, 10f));
            Assert.AreEqual(0, LodoFountainRules.LitCount(null, 0f));
        }

        [Test]
        public void Board_LightsAndClears()
        {
            LodoFountainBoard.Clear();
            Assert.AreEqual(0, LodoFountainBoard.LitCount(0f));
            for (int i = 0; i < LodoFountainRules.Count; i++)
            {
                LodoFountainBoard.Light(i, 100f);
            }

            LodoFountainBoard.Light(-1, 100f);
            LodoFountainBoard.Light(LodoFountainRules.Count, 100f);
            Assert.AreEqual(LodoFountainRules.Count, LodoFountainBoard.LitCount(150f));
            Assert.AreEqual(0, LodoFountainBoard.LitCount(100f + LodoFountainRules.LitSeconds));
            LodoFountainBoard.Clear();
            Assert.AreEqual(0, LodoFountainBoard.LitCount(150f));
        }

        [Test]
        public void Mud_ShrinksAFifthPerFountain_GoneWithTheFive_GrowsAtNight()
        {
            Assert.AreEqual(LodoFountainRules.MudBaseRadius, LodoFountainRules.MudRadius(0, false), 1e-3f);
            for (int lit = 1; lit <= LodoFountainRules.Count; lit++)
            {
                Assert.Less(LodoFountainRules.MudRadius(lit, false), LodoFountainRules.MudRadius(lit - 1, false));
            }

            Assert.AreEqual(0f, LodoFountainRules.MudRadius(LodoFountainRules.Count, false));
            Assert.AreEqual(0f, LodoFountainRules.MudRadius(LodoFountainRules.Count, true), "the five clean: no mud even at night");
            Assert.AreEqual(LodoFountainRules.MudBaseRadius * LodoFountainRules.MudNightGrowth, LodoFountainRules.MudRadius(0, true), 1e-3f);
            Assert.AreEqual(LodoFountainRules.MudBaseRadius, LodoFountainRules.MudRadius(-3, false), 1e-3f);
        }

        [Test]
        public void Mud_SafeZoneOfALitFountainKeepsItOff()
        {
            Vector2[] fountains = { new Vector2(0f, 50f), new Vector2(0f, -50f) };
            float[] board = Board(100f, float.NegativeInfinity);
            Vector2 eye = Vector2.zero;
            Vector2 byLit = new Vector2(0f, 50f + LodoFountainRules.SafeRadius - 0.1f);
            Vector2 byDark = new Vector2(0f, -50f);
            Assert.IsTrue(LodoFountainRules.InSafeZone(fountains, board, 10f, byLit));
            Assert.IsFalse(LodoFountainRules.InSafeZone(fountains, board, 10f, byDark), "a dark fountain is no shelter");
            Assert.IsFalse(LodoFountainRules.InSafeZone(fountains, board, 100f, byLit), "gone out");
            Assert.IsFalse(LodoFountainRules.InMud(eye, 200f, true, byLit));
            Assert.IsTrue(LodoFountainRules.InMud(eye, 200f, false, byDark));
            Assert.IsFalse(LodoFountainRules.InMud(eye, 40f, false, byDark), "outside the radius");
            Assert.IsFalse(LodoFountainRules.InMud(eye, 0f, false, eye), "no mud with the five");
        }

        [Test]
        public void Mud_DragsTowardTheEye_AndStopsAtTheRing()
        {
            Vector2 eye = new Vector2(10f, 10f);
            Vector2 pull = LodoFountainRules.Pull(eye, new Vector2(110f, 10f), 0.5f);
            Assert.AreEqual(-LodoFountainRules.PullSpeed * 0.5f, pull.x, 1e-4f);
            Assert.AreEqual(0f, pull.y, 1e-4f);
            Assert.AreEqual(Vector2.zero, LodoFountainRules.Pull(eye, eye + new Vector2(LodoFountainRules.PullStopRadius, 0f), 0.5f));
            Vector2 near = LodoFountainRules.Pull(eye, eye + new Vector2(LodoFountainRules.PullStopRadius + 0.1f, 0f), 1f);
            Assert.AreEqual(0.1f, near.magnitude, 1e-4f, "never past the stone ring");
            Assert.AreEqual(Vector2.zero, LodoFountainRules.Pull(eye, Vector2.zero, 0f));
        }

        [Test]
        public void Bite_IsAHazard_ThatNeverKills()
        {
            Assert.LessOrEqual(LodoFountainRules.BiteFraction, 0.04f, "the hazards' ceiling");
            Assert.AreEqual(4f, ProfundoHazardRules.Bite(100f, 100f, LodoFountainRules.BiteFraction, LodoFountainRules.BiteFloorFraction), 1e-4f);
            Assert.AreEqual(0f, ProfundoHazardRules.Bite(30f, 100f, LodoFountainRules.BiteFraction, LodoFountainRules.BiteFloorFraction));
        }

        [Test]
        public void FountainAt_TheNearestInReach()
        {
            Vector2[] fountains = { new Vector2(0f, 0f), new Vector2(5f, 0f) };
            Assert.AreEqual(1, LodoFountainRules.FountainAt(fountains, new Vector2(3f, 0f)));
            Assert.AreEqual(0, LodoFountainRules.FountainAt(fountains, new Vector2(-LodoFountainRules.PurifyReach, 0f)));
            Assert.AreEqual(-1, LodoFountainRules.FountainAt(fountains, new Vector2(0f, LodoFountainRules.PurifyReach + 0.1f)));
            Assert.AreEqual(-1, LodoFountainRules.FountainAt(null, Vector2.zero));
        }

        [Test]
        public void Light_BurnsTheBeasts_NotTheBosses()
        {
            Assert.AreEqual(300f, LodoFountainRules.BeastStrike(1000f, false, true), 1e-3f);
            Assert.AreEqual(50f, LodoFountainRules.BeastStrike(1000f, false, false), 1e-3f);
            Assert.AreEqual(0f, LodoFountainRules.BeastStrike(1000f, true, true));
            Assert.AreEqual(0f, LodoFountainRules.BeastStrike(-5f, false, true));
        }

        [Test]
        public void Reward_OnceADay_WhenTheFiveLight()
        {
            Assert.IsTrue(LodoFountainRules.GivesReward(4, 5, 0, 3));
            Assert.IsFalse(LodoFountainRules.GivesReward(4, 5, 3, 3), "already given today");
            Assert.IsTrue(LodoFountainRules.GivesReward(4, 5, 2, 3), "a new day");
            Assert.IsFalse(LodoFountainRules.GivesReward(5, 5, 0, 3), "renewing one of the five");
            Assert.IsFalse(LodoFountainRules.GivesReward(3, 4, 0, 3));
            Assert.AreEqual("item_pedra_vida", LodoFountainRules.RewardItemId);
        }

        [Test]
        public void Boss_WaitsForTheFive_AndGrowsAsTheyGoOut()
        {
            RegionBossSpawn lodo = RegionBossRules.SpawnOf(RegionBossRules.SwallowingMireId);
            RegionBossSpawn hedammu = RegionBossRules.SpawnOf(RegionBossRules.HedammuId);
            Assert.IsFalse(RegionBossRules.SummonReady(lodo, LodoFountainRules.Count - 1));
            Assert.IsTrue(RegionBossRules.SummonReady(lodo, LodoFountainRules.Count));
            Assert.IsTrue(RegionBossRules.SummonReady(hedammu, 0), "the other bosses never wait for fountains");
            Assert.IsFalse(RegionBossRules.SummonReady(null, LodoFountainRules.Count));
            Assert.AreEqual(1f, RegionBossRules.MireFountainMultiplier(LodoFountainRules.Count), 1e-5f);
            Assert.AreEqual(1.1f, RegionBossRules.MireFountainMultiplier(LodoFountainRules.Count - 1), 1e-5f);
            Assert.AreEqual(1.5f, RegionBossRules.MireFountainMultiplier(0), 1e-5f);
            Assert.AreEqual(1.5f, RegionBossRules.MireFountainMultiplier(-2), 1e-5f);
        }

        [Test]
        public void Layout_FiveFountains_ASpiralDownToTheEye_InMap35_OffWater()
        {
            Vector2[] fountains = OverworldLayouts.LodoFountains;
            Vector2 eye = OverworldLayouts.LodoArena;
            Assert.AreEqual(LodoFountainRules.Count, fountains.Length);
            float walk = 0f;
            for (int i = 0; i < fountains.Length; i++)
            {
                Vector2 f = fountains[i];
                float distance = Vector2.Distance(f, eye);
                Assert.AreEqual(OverworldLayouts.CoracaoLodoId, OverworldRules.ZoneAt(Delta, f).MapIdOrOwner, "fountain " + i);
                Assert.IsTrue(OverworldLayouts.DeltaBounds.Contains(f), "fountain " + i + " outside the driver's region");
                Assert.IsFalse(MapLayoutRules.InRiver(Delta.World, f, LodoFountainRules.PurifyReach), "fountain " + i + " in the water");
                Assert.IsNull(OverworldRules.SettlementAt(Delta, f), "fountain " + i);
                Assert.Greater(distance, RegionBossRules.ArenaClearRadius, "fountain " + i + " on the stone ring");
                Assert.Less(distance, LodoFountainRules.MudBaseRadius, "fountain " + i + " outside the mud it should push back");
                if (i > 0)
                {
                    Assert.Less(distance, Vector2.Distance(fountains[i - 1], eye), "the spiral goes down to the eye");
                    walk += Vector2.Distance(fountains[i - 1], f);
                }
            }

            Assert.LessOrEqual(Vector2.Distance(fountains[fountains.Length - 1], eye), RegionBossRules.ActiveRadius, "the last fountain is in the boss's waking range");
            float seconds = walk / HeroRunSpeed + LodoFountainRules.Count * LodoFountainRules.PurifySeconds;
            Assert.Less(seconds, LodoFountainRules.LitSeconds * 0.8f, "the five can be lit before the first goes out");
        }

        [Test]
        public void Layout_TheNightMud_StaysOffTheRaizMae()
        {
            float nightMud = LodoFountainRules.MudRadius(0, true);
            Assert.Greater(Vector2.Distance(OverworldLayouts.RaizMaeOffset, OverworldLayouts.LodoArena), nightMud + 30f);
        }
    }
}
