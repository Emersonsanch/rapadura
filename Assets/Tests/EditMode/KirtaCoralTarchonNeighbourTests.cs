using System.Collections.Generic;
using NUnit.Framework;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.NPC.Visuals;
using Rapadura.Gameplay.World.Cities.Town;
using Rapadura.Gameplay.World.Map.Mechanics;
using Rapadura.Gameplay.World.Map.Overworld;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// Reacender o coral of Kirta (Docs/Mapas100.md 28, <see cref="KirtaCoralRules"/>) and Despertar o vizinho with the swamp night of the
    /// Palafitas de Tarchon (33, <see cref="TarchonLanternRules"/>): the places, the [E] checks, the mending, the lantern, the purse and the mist.
    /// </summary>
    public class KirtaCoralTarchonNeighbourTests
    {
        [Test]
        public void Corals_FiveApart_InTheCosta_AwayFromTheTown()
        {
            KirtaCoral[] corals = OverworldLayouts.KirtaCorals;
            Assert.AreEqual(KirtaCoralRules.CoralCount, corals.Length);
            var ids = new HashSet<string>();
            for (int i = 0; i < corals.Length; i++)
            {
                Assert.IsTrue(ids.Add(corals[i].Id), "unique id " + corals[i].Id);
                Assert.IsTrue(OverworldLayouts.CostaBounds.Contains(corals[i].Position), corals[i].Id + " in the Costa");
                Assert.Greater(Vector2.Distance(corals[i].Position, OverworldLayouts.KirtaOffset), OverworldLayouts.RaceCityGround,
                    corals[i].Id + " stands on the reef, off the town's ground");
                for (int j = i + 1; j < corals.Length; j++)
                {
                    Assert.Greater(Vector2.Distance(corals[i].Position, corals[j].Position), KirtaCoralRules.MendRadius * 2f, "the rings do not overlap");
                }
            }
        }

        [Test]
        public void Coral_ESetsTheSparkFree_OnlyCloseAliveAndOnce()
        {
            Assert.AreEqual(KirtaCoralCheck.Relight, KirtaCoralRules.Check(false, 3f, true));
            Assert.AreEqual(KirtaCoralCheck.TooFar, KirtaCoralRules.Check(false, 4f, true));
            Assert.AreEqual(KirtaCoralCheck.AlreadyLit, KirtaCoralRules.Check(true, 1f, true));
            Assert.AreEqual(KirtaCoralCheck.Dead, KirtaCoralRules.Check(false, 1f, false));
            Assert.IsFalse(KirtaCoralRules.IsLit(0L));
            Assert.IsFalse(KirtaCoralRules.IsLit(long.MinValue), "no mark in the save: dimmed");
            Assert.IsTrue(KirtaCoralRules.IsLit(1_700_000_000L));
            Assert.AreEqual("kirta.coral.leste", KirtaCoralRules.MarkKey("leste"));
        }

        [Test]
        public void LitCoral_MendsInItsRing_OutOfCombat_UpToFull()
        {
            KirtaCoral[] corals = OverworldLayouts.KirtaCorals;
            var lit = new bool[corals.Length];
            Vector2 beside = corals[2].Position + new Vector2(5f, 0f);
            Assert.AreEqual(-1, KirtaCoralRules.MendingCoral(corals, lit, beside), "a dimmed coral mends nothing");
            lit[2] = true;
            Assert.AreEqual(2, KirtaCoralRules.MendingCoral(corals, lit, beside));
            Assert.AreEqual(-1, KirtaCoralRules.MendingCoral(corals, lit, corals[2].Position + new Vector2(9f, 0f)), "past the 8 m ring");

            Assert.IsFalse(KirtaCoralRules.Mends(true, true, 3f), "fighting");
            Assert.IsTrue(KirtaCoralRules.Mends(true, true, 4f));
            Assert.IsFalse(KirtaCoralRules.Mends(true, false, 10f));
            Assert.IsFalse(KirtaCoralRules.Mends(false, true, 10f));
            Assert.AreEqual(20f, KirtaCoralRules.MendOver(100f, 1000f, 1f), 1e-4f, "2% a second");
            Assert.AreEqual(5f, KirtaCoralRules.MendOver(995f, 1000f, 1f), 1e-4f, "never past full");
        }

        [Test]
        public void Corals_FiveLit_AreTheWholeReef_AndCountForTheServer()
        {
            var lit = new[] { true, true, false, true, true };
            Assert.AreEqual(4, KirtaCoralRules.LitCount(lit));
            Assert.IsFalse(KirtaCoralRules.AllLit(lit));
            lit[2] = true;
            Assert.IsTrue(KirtaCoralRules.AllLit(lit));
            Assert.AreEqual(0.5f, KirtaCoralRules.ServerGoalProgress(KirtaCoralRules.ServerGoalCorals / 2), 1e-4f);
            Assert.AreEqual(1f, KirtaCoralRules.ServerGoalProgress(long.MaxValue / 2), 1e-4f);
        }

        [Test]
        public void Neighbours_FiveEtruscan_InTheTown_ApartFromEachOtherAndTheMast()
        {
            TarchonNeighbour[] neighbours = OverworldLayouts.TarchonNeighbours;
            Assert.AreEqual(TarchonLanternRules.NeighbourCount, neighbours.Length);
            CollectionAssert.AreEquivalent(new[] { "arnth", "velia", "laris", "ravnthu", "fasti" }, System.Array.ConvertAll(neighbours, n => n.Id));
            Vector2 mast = OverworldLayouts.TarchonOffset + CityTownRecipes.TarchonWispMast;
            for (int i = 0; i < neighbours.Length; i++)
            {
                Assert.IsTrue(OverworldLayouts.TarchonNightBounds.Contains(neighbours[i].Position), neighbours[i].Id + " in the swamp night's ground");
                Assert.IsTrue(OverworldLayouts.DeltaBounds.Contains(neighbours[i].Position));
                Assert.Less(Vector2.Distance(neighbours[i].Position, OverworldLayouts.TarchonOffset), 19f, neighbours[i].Id + " inside the town");
                Assert.Greater(Vector2.Distance(neighbours[i].Position, mast), TarchonLanternRules.MastReach, "the mast's prompt is not a neighbour's");
                for (int j = i + 1; j < neighbours.Length; j++)
                {
                    Assert.Greater(Vector2.Distance(neighbours[i].Position, neighbours[j].Position), TarchonLanternRules.NeighbourReach * 2f);
                }

                Assert.AreNotEqual(TarchonLanternTexts.NeighbourLine(neighbours[i].Id),
                    TarchonLanternTexts.NeighbourLine(neighbours[(i + 1) % neighbours.Length].Id), "each neighbour has his own line");
            }
        }

        [Test]
        public void Neighbour_WakesWithALitLantern_OncePerDay()
        {
            Assert.AreEqual(NeighbourCheck.Wake, TarchonLanternRules.Check(false, true, 2f));
            Assert.AreEqual(NeighbourCheck.NoLantern, TarchonLanternRules.Check(false, false, 2f));
            Assert.AreEqual(NeighbourCheck.AlreadyAwake, TarchonLanternRules.Check(true, true, 2f));
            Assert.AreEqual(NeighbourCheck.TooFar, TarchonLanternRules.Check(false, true, 3f));

            Assert.IsTrue(TarchonLanternRules.AwakeToday(7, 7));
            Assert.IsFalse(TarchonLanternRules.AwakeToday(6, 7), "dimmed again the next day");
            Assert.IsFalse(TarchonLanternRules.AwakeToday(0, 0), "no mark: dimmed");
            Assert.AreEqual(2, TarchonLanternRules.AwakeCount(new[] { 7, 6, 7, 0, 3 }, 7));
        }

        [Test]
        public void Lantern_BurnsTwoMinutes_LitAtTheMast()
        {
            Assert.IsFalse(TarchonLanternRules.LanternLit(float.NegativeInfinity, 10f), "none in the hands");
            Assert.IsTrue(TarchonLanternRules.LanternLit(10f, 10f));
            Assert.IsTrue(TarchonLanternRules.LanternLit(10f, 129f));
            Assert.IsFalse(TarchonLanternRules.LanternLit(10f, 130f));
            Assert.AreEqual(60f, TarchonLanternRules.LanternLeft(10f, 70f), 1e-4f);
            Assert.AreEqual(0f, TarchonLanternRules.LanternLeft(10f, 200f));
            Assert.IsTrue(TarchonLanternRules.CanLightAtMast(3f, true));
            Assert.IsFalse(TarchonLanternRules.CanLightAtMast(4f, true));
            Assert.IsFalse(TarchonLanternRules.CanLightAtMast(1f, false));
        }

        [Test]
        public void Purse_ForAllFive_OncePerDay()
        {
            Assert.IsFalse(TarchonLanternRules.PaysPurse(4, 0, 7));
            Assert.IsTrue(TarchonLanternRules.PaysPurse(5, 0, 7));
            Assert.IsTrue(TarchonLanternRules.PaysPurse(5, 6, 7), "yesterday's purse does not count");
            Assert.IsFalse(TarchonLanternRules.PaysPurse(5, 7, 7), "already paid today");
        }

        [Test]
        public void SwampNight_MistCutsTheSight_TheLanternGivesItBack()
        {
            Assert.AreEqual(TarchonLanternRules.NightSight, TarchonLanternRules.SightRange(true, true, false));
            Assert.IsFalse(BeastSightRules.Active(TarchonLanternRules.SightRange(true, true, true)), "a lit lantern");
            Assert.IsFalse(BeastSightRules.Active(TarchonLanternRules.SightRange(true, true, false, true)), "the Lanterna de Fogo-Fátuo in the bag");
            Assert.IsNotNull(Resources.Load<Rapadura.Gameplay.Items.ItemDefinition>("Items/" + TarchonLanternRules.WispLanternItem), "the shop's lantern is an item");
            Assert.IsFalse(BeastSightRules.Active(TarchonLanternRules.SightRange(false, true, false)), "by day");
            Assert.IsFalse(BeastSightRules.Active(TarchonLanternRules.SightRange(true, false, false)), "off the town's ground");
            Assert.IsTrue(OverworldLayouts.TarchonNightBounds.Contains(OverworldLayouts.TarchonOffset));
            Assert.IsTrue(OverworldLayouts.DeltaBounds.Contains(OverworldLayouts.TarchonNightBounds.min)
                && OverworldLayouts.DeltaBounds.Contains(OverworldLayouts.TarchonNightBounds.max), "the night lies inside the Delta");
        }

        [Test]
        public void NeighbourFigures_DimAndAwake_Build()
        {
            foreach (TarchonNeighbour neighbour in OverworldLayouts.TarchonNeighbours)
            {
                NpcRecipe dim = TarchonNeighbourRecipes.For(neighbour.Id, false);
                NpcRecipe awake = TarchonNeighbourRecipes.For(neighbour.Id, true);
                Assert.AreNotEqual(dim.Id, awake.Id, "two cached meshes");
                Assert.AreEqual(NpcPosture.Stooped, dim.Posture);
                CollectionAssert.Contains(awake.Props, NpcProp.LanternPole);
                Assert.Greater(NpcModelBuilder.CountTriangles(dim), 0);
                Assert.Greater(NpcModelBuilder.CountTriangles(awake), 0);
            }
        }

        [Test]
        public void Lines_EveryCousinHasOne_AndNoTextIsMissing()
        {
            foreach (CharacterId hero in new[] { CharacterId.Maithe, CharacterId.Icaro, CharacterId.Joaquim, CharacterId.Maria, CharacterId.Lavine })
            {
                Assert.AreNotEqual(KirtaCoralTexts.LineFor(hero), KirtaCoralTexts.Get(null, KirtaCoralTexts.LineFor(hero)));
                Assert.AreNotEqual(TarchonLanternTexts.LineFor(hero), TarchonLanternTexts.Get(null, TarchonLanternTexts.LineFor(hero)));
            }

            foreach (var entry in KirtaCoralTexts.Entries)
            {
                Assert.IsFalse(string.IsNullOrEmpty(entry.PtBR) || entry.PtBR.Contains("Dona ") || entry.PtBR.Contains("Seu "), entry.Key);
            }

            foreach (var entry in TarchonLanternTexts.Entries)
            {
                Assert.IsFalse(string.IsNullOrEmpty(entry.PtBR) || entry.PtBR.Contains("Dona ") || entry.PtBR.Contains("Seu "), entry.Key);
            }
        }
    }
}
