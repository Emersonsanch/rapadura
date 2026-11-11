using System.Linq;
using NUnit.Framework;
using Rapadura.Gameplay.World.Map.Areas;
using Rapadura.Gameplay.World.Map.Mechanics;
using Rapadura.Gameplay.World.Map.Overworld;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// The Margens da Mistura mechanics (Docs/Mapas100.md 77–82): the time bubbles (<see cref="MisturaTimeBubbleRules"/>), lighting the pitch
    /// (<see cref="MisturaTarRules"/>), the breathing shoulder (<see cref="MisturaShoulderRules"/>) and the places the layout declares for them.
    /// </summary>
    public class MisturaMechanicsTests
    {
        private static OverworldLayout World => OverworldLayouts.Get(OverworldLayouts.MisturaRegionId);

        // ------------------------------------------------------------------
        // Time bubbles
        // ------------------------------------------------------------------

        private static readonly MisturaTimeBubble[] TwoBubbles =
        {
            new MisturaTimeBubble(Vector2.zero, 10f, MisturaTimeBubbleRules.SlowFactor),
            new MisturaTimeBubble(new Vector2(50f, 0f), 10f, MisturaTimeBubbleRules.FastFactor),
        };

        [Test]
        public void Bubble_GivesItsFactor_InsideOnly()
        {
            Assert.AreEqual(0.7f, MisturaTimeBubbleRules.FactorAt(TwoBubbles, new Vector2(9f, 0f), false), 1e-4f);
            Assert.AreEqual(1.3f, MisturaTimeBubbleRules.FactorAt(TwoBubbles, new Vector2(50f, 9f), false), 1e-4f);
            Assert.AreEqual(1f, MisturaTimeBubbleRules.FactorAt(TwoBubbles, new Vector2(25f, 0f), false), 1e-4f);
        }

        [Test]
        public void Bubble_VanishesAtNight()
        {
            Assert.AreEqual(-1, MisturaTimeBubbleRules.IndexAt(TwoBubbles, Vector2.zero, true));
            Assert.AreEqual(1f, MisturaTimeBubbleRules.FactorAt(TwoBubbles, Vector2.zero, true), 1e-4f);
        }

        [Test]
        public void Bubble_MoveBonus_IsTheFactorAsAModifier()
        {
            Assert.AreEqual(-0.3f, MisturaTimeBubbleRules.MoveBonus(0.7f), 1e-4f);
            Assert.AreEqual(0.3f, MisturaTimeBubbleRules.MoveBonus(1.3f), 1e-4f);
        }

        // ------------------------------------------------------------------
        // The pitch
        // ------------------------------------------------------------------

        [Test]
        public void Tar_LightsWithAFuse_Burns_Scorches_AndComesBack()
        {
            MisturaTarState s = MisturaTarRules.Ignite(MisturaTarRules.Resting);
            Assert.AreEqual(MisturaTarStage.Fuse, s.Stage);
            Assert.AreEqual(MisturaTarRules.FuseSeconds, s.Remaining, 1e-4f);

            s = MisturaTarRules.Advance(s, MisturaTarRules.FuseSeconds - 0.1f);
            Assert.AreEqual(MisturaTarStage.Fuse, s.Stage, "the warning runs its full length");
            s = MisturaTarRules.Advance(s, 0.2f);
            Assert.AreEqual(MisturaTarStage.Burning, s.Stage);
            s = MisturaTarRules.Advance(s, MisturaTarRules.BurnSeconds);
            Assert.AreEqual(MisturaTarStage.Scorched, s.Stage);
            s = MisturaTarRules.Advance(s, MisturaTarRules.ScorchedSeconds);
            Assert.AreEqual(MisturaTarStage.Sticky, s.Stage);
        }

        [Test]
        public void Tar_StaysOpenThirtySeconds_FromTheFirstFlame()
        {
            Assert.AreEqual(30f, MisturaTarRules.BurnSeconds + MisturaTarRules.ScorchedSeconds, 1e-4f);
        }

        [Test]
        public void Tar_ALongFrame_CrossesEveryStage()
        {
            MisturaTarState lit = MisturaTarRules.Ignite(MisturaTarRules.Resting);
            Assert.AreEqual(MisturaTarStage.Sticky, MisturaTarRules.Advance(lit, 1000f).Stage);
        }

        [Test]
        public void Tar_RelightingDoesNotRestartTheClock()
        {
            MisturaTarState burning = MisturaTarRules.Advance(MisturaTarRules.Ignite(MisturaTarRules.Resting), MisturaTarRules.FuseSeconds + 1f);
            MisturaTarState again = MisturaTarRules.Ignite(burning);
            Assert.AreEqual(burning.Stage, again.Stage);
            Assert.AreEqual(burning.Remaining, again.Remaining, 1e-4f);
        }

        [Test]
        public void Tar_OnlyTheRestingPitchSlows_AndNeverStops()
        {
            Assert.AreEqual(MisturaTarRules.StickyFactor, MisturaTarRules.SpeedFactor(MisturaTarStage.Sticky), 1e-4f);
            Assert.Greater(MisturaTarRules.StickyFactor, 0f);
            Assert.AreEqual(1f, MisturaTarRules.SpeedFactor(MisturaTarStage.Fuse), 1e-4f, "the hero must be able to walk out at full pace");
            Assert.AreEqual(1f, MisturaTarRules.SpeedFactor(MisturaTarStage.Burning), 1e-4f);
            Assert.AreEqual(1f, MisturaTarRules.SpeedFactor(MisturaTarStage.Scorched), 1e-4f);
        }

        [Test]
        public void Tar_FuseIsLongEnoughToWalkOut_OfTheWidestPatch()
        {
            float widest = OverworldLayouts.TarPatches.Max(p => p.Radius) + MisturaTarRules.BurnMargin;
            const float heroRunSpeed = 5f;
            Assert.GreaterOrEqual(MisturaTarRules.FuseSeconds * heroRunSpeed, widest);
        }

        [TestCase(1000f, 1000f, 30f)]
        [TestCase(20f, 1000f, 19f)]
        [TestCase(1f, 1000f, 0f)]
        public void Tar_FireNeverKillsTheHero(float current, float max, float expected)
        {
            float bite = MisturaTarRules.HeroBurn(current, max);
            Assert.AreEqual(expected, bite, 1e-3f);
            Assert.GreaterOrEqual(current - bite, 1f);
        }

        [Test]
        public void Tar_BurnsBeastsToo_AndReachesPastTheRim()
        {
            Assert.AreEqual(40f, MisturaTarRules.BeastBurn(1000f), 1e-3f);
            var patch = new MisturaTarPatch(Vector2.zero, 8f);
            Assert.IsTrue(MisturaTarRules.InBurnReach(patch, new Vector2(8f + MisturaTarRules.BurnMargin, 0f)));
            Assert.IsFalse(MisturaTarRules.InBurnReach(patch, new Vector2(8.1f + MisturaTarRules.BurnMargin, 0f)));
        }

        [Test]
        public void Tar_FireReachesByItsAreaAndTheMargin()
        {
            var patch = new MisturaTarPatch(Vector2.zero, 8f);
            Assert.IsTrue(MisturaTarRules.Reaches(patch, new Vector2(8f + MisturaTarRules.IgniteMargin, 0f), 0f));
            Assert.IsFalse(MisturaTarRules.Reaches(patch, new Vector2(8.1f + MisturaTarRules.IgniteMargin, 0f), 0f));
            Assert.IsTrue(MisturaTarRules.Reaches(patch, new Vector2(8f + MisturaTarRules.IgniteMargin + 4f, 0f), 4f));
        }

        // ------------------------------------------------------------------
        // The breathing shoulder
        // ------------------------------------------------------------------

        [TestCase(0f, MisturaBreathPhase.Still)]
        [TestCase(44.9f, MisturaBreathPhase.Still)]
        [TestCase(45f, MisturaBreathPhase.Warning)]
        [TestCase(49.9f, MisturaBreathPhase.Warning)]
        [TestCase(50f, MisturaBreathPhase.Tilting)]
        [TestCase(59.9f, MisturaBreathPhase.Tilting)]
        [TestCase(60f, MisturaBreathPhase.Still)]
        public void Breath_FollowsItsMinute(float time, MisturaBreathPhase expected)
        {
            Assert.AreEqual(expected, MisturaShoulderRules.PhaseAt(time, false));
        }

        [Test]
        public void Breath_AlwaysWarnsBeforeTilting()
        {
            Assert.Greater(MisturaShoulderRules.WarningSeconds, 0f);
            Assert.AreEqual(MisturaShoulderRules.CycleSeconds, MisturaShoulderRules.StillSeconds + MisturaShoulderRules.WarningSeconds + MisturaShoulderRules.TiltSeconds, 1e-4f);
        }

        [Test]
        public void Breath_TheGiantSleepsAtNight()
        {
            Assert.AreEqual(MisturaBreathPhase.Still, MisturaShoulderRules.PhaseAt(55f, true));
        }

        [Test]
        public void Breath_AlternatesNorthAndSouth_SoTheDriftCancels()
        {
            Assert.AreEqual(Vector2.up, MisturaShoulderRules.Direction(MisturaShoulderRules.BreathIndex(55f)));
            Assert.AreEqual(Vector2.down, MisturaShoulderRules.Direction(MisturaShoulderRules.BreathIndex(115f)));
            Assert.AreEqual(Vector2.zero, MisturaShoulderRules.Direction(0) + MisturaShoulderRules.Direction(1));
        }

        [Test]
        public void Breath_SlidesOnlyWhileTilting_LessWhenWalking_NotAtAStone()
        {
            Vector2 up = Vector2.up;
            Assert.AreEqual(Vector2.zero, MisturaShoulderRules.SlideVelocity(MisturaBreathPhase.Still, up, false, false));
            Assert.AreEqual(Vector2.zero, MisturaShoulderRules.SlideVelocity(MisturaBreathPhase.Warning, up, false, false));
            Assert.AreEqual(Vector2.zero, MisturaShoulderRules.SlideVelocity(MisturaBreathPhase.Tilting, up, false, true));
            Assert.AreEqual(MisturaShoulderRules.SlideSpeed, MisturaShoulderRules.SlideVelocity(MisturaBreathPhase.Tilting, up, false, false).magnitude, 1e-4f);
            Assert.AreEqual(MisturaShoulderRules.SlideSpeed * MisturaShoulderRules.WalkingFactor, MisturaShoulderRules.SlideVelocity(MisturaBreathPhase.Tilting, up, true, false).magnitude, 1e-4f);
        }

        [Test]
        public void Breath_AHeroCanWalkAgainstTheSlide()
        {
            Assert.Less(MisturaShoulderRules.SlideSpeed * MisturaShoulderRules.WalkingFactor, 2f);
        }

        [Test]
        public void Breath_StoneShelters_WithinItsReach()
        {
            var stones = new[] { new Vector2(10f, 0f) };
            Assert.IsTrue(MisturaShoulderRules.Sheltered(stones, new Vector2(10f + MisturaShoulderRules.StoneReach, 0f)));
            Assert.IsFalse(MisturaShoulderRules.Sheltered(stones, new Vector2(10.1f + MisturaShoulderRules.StoneReach, 0f)));
        }

        // ------------------------------------------------------------------
        // The layout
        // ------------------------------------------------------------------

        [Test]
        public void Layout_AnchorExists_AndTheBoundsAreTheRegions()
        {
            Assert.IsNotNull(World.Zone(OverworldLayouts.MisturaAnchorZoneId));
            Assert.AreEqual(World.Bounds, OverworldLayouts.MisturaBounds);
        }

        [Test]
        public void Layout_Bubbles_LieInTheDesert_OffTheTowns_MostOnARoad()
        {
            Assert.GreaterOrEqual(OverworldLayouts.TimeBubbles.Length, 8);
            Assert.IsTrue(OverworldLayouts.TimeBubbles.Any(b => b.Factor < 1f));
            Assert.IsTrue(OverworldLayouts.TimeBubbles.Any(b => b.Factor > 1f));
            foreach (MisturaTimeBubble bubble in OverworldLayouts.TimeBubbles)
            {
                Assert.AreEqual(OverworldLayouts.DesertoRelogioMapId, OverworldRules.ZoneAt(World, bubble.Center).MapIdOrOwner, bubble.Center.ToString());
                foreach (OverworldSettlement town in World.Settlements)
                {
                    Assert.IsFalse(Square(bubble.Center, bubble.Radius).Overlaps(town.GroundRect), bubble.Center + " over " + town.Id);
                }
            }

            Assert.GreaterOrEqual(OverworldLayouts.TimeBubbles.Count(b => MapLayoutRules.OnPath(World.World, b.Center, 30f)), 6);
        }

        [Test]
        public void Layout_Tar_LiesInTheWells_Dry_OffTheTowns_SomeOnTheRoads()
        {
            Assert.GreaterOrEqual(OverworldLayouts.TarPatches.Length, 6);
            foreach (MisturaTarPatch patch in OverworldLayouts.TarPatches)
            {
                Assert.AreEqual(OverworldLayouts.PocosBetumeMapId, OverworldRules.ZoneAt(World, patch.Center).MapIdOrOwner, patch.Center.ToString());
                Assert.IsFalse(MapLayoutRules.InRiver(World.World, patch.Center, patch.Radius), patch.Center + " in the water");
                foreach (OverworldSettlement town in World.Settlements)
                {
                    Assert.IsFalse(Square(patch.Center, patch.Radius).Overlaps(town.GroundRect), patch.Center + " over " + town.Id);
                }
            }

            Assert.GreaterOrEqual(OverworldLayouts.TarPatches.Count(p => MapLayoutRules.OnPath(World.World, p.Center, 25f)), 4);
        }

        [Test]
        public void Layout_ShoulderArea_IsTheShouldersMap()
        {
            foreach (Vector2 p in new[] { new Vector2(860f, -1180f), new Vector2(1300f, 1180f), new Vector2(1080f, 300f) })
            {
                Assert.IsTrue(OverworldLayouts.ShoulderArea.Contains(p));
                Assert.AreEqual(OverworldLayouts.OmbroUpelluriMapId, OverworldRules.ZoneAt(World, p).MapIdOrOwner, p.ToString());
            }
        }

        [Test]
        public void Layout_Stones_StandOnTheShoulder_OffTheTowns_AndMostGetTheirShard()
        {
            Assert.GreaterOrEqual(OverworldLayouts.BreathStones.Length, 16);
            int placed = 0;
            foreach (Vector2 stone in OverworldLayouts.BreathStones)
            {
                Assert.IsTrue(OverworldLayouts.ShoulderArea.Contains(stone), stone.ToString());
                Assert.IsNull(OverworldRules.SettlementAt(World, stone), stone.ToString());
                Vector2Int chunk = OverworldRules.ChunkOf(World, stone);
                if (OverworldChunkBuilder.Content(World, chunk).Props.Any(p => p.Decor == OverworldDecor.DioriteShard && Vector2.Distance(p.Position, stone) < 0.1f))
                {
                    placed++;
                }
            }

            Assert.GreaterOrEqual(placed, OverworldLayouts.BreathStones.Length / 2, "a shard can give way to a trail, but most must stand");
        }

        [Test]
        public void Texts_HaveBothLanguages()
        {
            Assert.GreaterOrEqual(MisturaMechanicsTexts.Entries.Count, 9);
            foreach (var entry in MisturaMechanicsTexts.Entries)
            {
                Assert.IsFalse(string.IsNullOrEmpty(entry.En), entry.Key);
                Assert.IsFalse(string.IsNullOrEmpty(entry.PtBR), entry.Key);
            }
        }

        private static Rect Square(Vector2 center, float radius) => new Rect(center.x - radius, center.y - radius, radius * 2f, radius * 2f);
    }
}
