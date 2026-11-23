using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.NPC.Visuals;
using Rapadura.Gameplay.Quests;
using Rapadura.Gameplay.World.Cities;
using Rapadura.Gameplay.World.Cities.Town;
using Rapadura.Gameplay.World.Map;
using Rapadura.Gameplay.World.Map.Areas;
using Rapadura.Gameplay.World.Map.Mechanics;
using Rapadura.Gameplay.World.Map.Overworld;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// What the towns of Kirta (28) and Tarchon (33) do with the reef's corals and the woken neighbours: the mother coral by the corals lit
    /// (<see cref="KirtaMotherCoralRules"/>, <c>CityTownRecipes.Kirta(int)</c>), the trols who come home (<see cref="TarchonCrowdRules"/>,
    /// <c>CityTownRecipes.Tarchon(int)</c>), Larthia buying the freed wisps (<see cref="LarthiaWispRules"/>), Tarchon's daily
    /// (<see cref="TarchonDailyRules"/>, <see cref="CityDailyQuestCatalog"/>) and the places of the five corals and the five neighbours
    /// against the trails, the water and the lots of the houses.
    /// </summary>
    public class KirtaTarchonTownTests
    {
        // ------------------------------------------------------------------
        // Kirta: the mother coral
        // ------------------------------------------------------------------

        [Test]
        public void MotherCoral_RelightsABranchPerCoral_EastFirst_WholeWithFive()
        {
            Assert.AreEqual(3, KirtaMotherCoralRules.LitBranches(0));
            Assert.AreEqual(4, KirtaMotherCoralRules.LitBranches(1));
            Assert.AreEqual(7, KirtaMotherCoralRules.LitBranches(4));
            Assert.AreEqual(7, KirtaMotherCoralRules.LitBranches(9), "never past the seven");
            Assert.IsFalse(KirtaMotherCoralRules.Whole(4));
            Assert.IsTrue(KirtaMotherCoralRules.Whole(5));
            CollectionAssert.AreEquivalent(Enumerable.Range(0, KirtaMotherCoralRules.Branches), KirtaMotherCoralRules.RelightOrder);

            // At the start the lit branches are the east half, the look the town always had (dir.x > 0.05).
            for (int i = 0; i < KirtaMotherCoralRules.Branches; i++)
            {
                bool east = Mathf.Cos((i * 51.4f + 10f) * Mathf.Deg2Rad) > 0.05f;
                Assert.AreEqual(east, KirtaMotherCoralRules.BranchLit(i, 0), "branch " + i);
                Assert.IsTrue(KirtaMotherCoralRules.BranchLit(i, 5));
            }

            for (int n = 0; n < KirtaCoralRules.CoralCount; n++)
            {
                int lit = Enumerable.Range(0, KirtaMotherCoralRules.Branches).Count(i => KirtaMotherCoralRules.BranchLit(i, n));
                int next = Enumerable.Range(0, KirtaMotherCoralRules.Branches).Count(i => KirtaMotherCoralRules.BranchLit(i, n + 1));
                Assert.GreaterOrEqual(next, lit, "lighting a coral never dims a branch");
            }

            Assert.AreEqual(0f, KirtaMotherCoralRules.Glow(0));
            Assert.AreEqual(1f, KirtaMotherCoralRules.Glow(5), 1e-4f);
            Assert.IsTrue(KirtaMotherCoralRules.HasNews(2, 1));
            Assert.IsFalse(KirtaMotherCoralRules.HasNews(2, 2));
            Assert.IsFalse(KirtaMotherCoralRules.HasNews(0, 0));
        }

        [Test]
        public void KirtaRecipe_CarriesTheCoralsLit_OnTheMotherCoral_AndDrawsEachLook()
        {
            Assert.AreEqual(0, MotherOf(CityTownRecipes.Kirta()).Variant, "the game's start");
            var triangles = new HashSet<int>();
            for (int n = 0; n <= KirtaCoralRules.CoralCount; n++)
            {
                CityTownRecipe recipe = CityTownRecipes.Kirta(n);
                CityProp mother = MotherOf(recipe);
                Assert.AreEqual(n, mother.Variant);
                Assert.AreEqual(CityTownRecipes.KirtaMotherCoral, mother.Position);
                var b = new TownMeshBuilder();
                TownKit.Prop(b, mother, recipe.Style);
                Assert.Greater(b.TriangleCount, 0);
                Assert.Less(b.TriangleCount, 2500);
                triangles.Add(b.TriangleCount);
            }

            Assert.AreEqual(KirtaCoralRules.CoralCount, MotherOf(CityTownRecipes.Kirta(99)).Variant, "clamped");
            Assert.AreEqual(OverworldLayouts.KirtaOffset + CityTownRecipes.KirtaMotherCoral, OverworldLayouts.KirtaMotherCoralAt);
            foreach (string key in new[] { KirtaTarchonTownTexts.MotherLine(1), KirtaTarchonTownTexts.MotherLine(5) })
            {
                Assert.AreNotEqual(key, KirtaTarchonTownTexts.Get(null, key));
            }
        }

        private static CityProp MotherOf(CityTownRecipe recipe) => recipe.PlazaProps.Single(p => p.Kind == CityPropKind.CoralMother);

        // ------------------------------------------------------------------
        // Tarchon: the crowd
        // ------------------------------------------------------------------

        [Test]
        public void Crowd_OneTrolHomePerThreshold_LanternsFollow()
        {
            Assert.AreEqual(0, TarchonCrowdRules.Returned(0));
            Assert.AreEqual(0, TarchonCrowdRules.Returned(2));
            Assert.AreEqual(1, TarchonCrowdRules.Returned(3));
            Assert.AreEqual(2, TarchonCrowdRules.Returned(10));
            Assert.AreEqual(TarchonCrowdRules.MaxReturned, TarchonCrowdRules.Returned(10_000));
            Assert.AreEqual(1, TarchonCrowdRules.ToNext(2));
            Assert.AreEqual(7, TarchonCrowdRules.ToNext(3));
            Assert.AreEqual(0, TarchonCrowdRules.ToNext(80));
            Assert.AreEqual(0, TarchonCrowdRules.LanternPairs(1));
            Assert.AreEqual(1, TarchonCrowdRules.LanternPairs(2));
            Assert.AreEqual(2, TarchonCrowdRules.LanternPairs(6));
            Assert.IsTrue(TarchonCrowdRules.HasNews(1, 0));
            Assert.IsFalse(TarchonCrowdRules.HasNews(1, 1));
            Assert.AreEqual(TarchonCrowdRules.MaxReturned, CityTownRecipes.TarchonCrowdSpots.Length, "one spot per trol");

            int lamps0 = CityTownRecipes.Tarchon().PlazaProps.Count(p => p.Kind == CityPropKind.LampPost);
            Assert.AreEqual(lamps0 + 2, CityTownRecipes.Tarchon(2).PlazaProps.Count(p => p.Kind == CityPropKind.LampPost));
            Assert.AreEqual(lamps0 + 4, CityTownRecipes.Tarchon(6).PlazaProps.Count(p => p.Kind == CityPropKind.LampPost));
        }

        [Test]
        public void TarchonRecipe_StaysAValidTown_WithEveryCrowdTier()
        {
            for (int returned = 0; returned <= TarchonCrowdRules.MaxReturned; returned++)
            {
                CityLayout layout = CityLayoutGenerator.Generate(CityTownRecipes.Tarchon(returned));
                List<string> problems = CityLayoutRules.Validate(layout);
                Assert.IsEmpty(problems, returned + " back: " + string.Join("; ", problems));
            }
        }

        [Test]
        public void CrowdSpots_AndNeighbours_StandOnTheWalk_OffEveryLotColliderAndDoor()
        {
            // The fullest town: every lantern hung.
            CityLayout layout = CityLayoutGenerator.Generate(CityTownRecipes.Tarchon(TarchonCrowdRules.MaxReturned));
            Vector2 o = OverworldLayouts.TarchonOffset;
            var people = OverworldLayouts.TarchonNeighbours.Select(n => (n.Id, Local: n.Position - o))
                .Concat(OverworldLayouts.TarchonCrowd.Select(c => (c.Id, Local: c.Position - o))).ToList();
            Assert.AreEqual(TarchonLanternRules.NeighbourCount + TarchonCrowdRules.MaxReturned, people.Select(p => p.Id).Distinct().Count(), "unique names");
            Vector2 trade = OverworldLayouts.TarchonLarthiaTrade - o;
            Vector2 mast = CityTownRecipes.TarchonWispMast;

            foreach (var person in people)
            {
                Assert.IsTrue(layout.WalkBounds.Contains(person.Local), person.Id + " inside the walk bounds");
                Assert.Greater(CityLayoutRules.Clearance(layout, person.Local), 0.5f, person.Id + " clear of every collider");
                foreach (CityLot lot in layout.Lots)
                {
                    Assert.IsFalse(lot.Footprint.Contains(person.Local), person.Id + " on the lot " + lot.Id);
                    if (lot.HasKeeper || lot.Kind == CityLotKind.House || lot.Kind == CityLotKind.Shop)
                    {
                        Assert.Greater(Vector2.Distance(person.Local, lot.DoorApproach), 1.2f, person.Id + " blocks the door of " + lot.Id);
                        Assert.Greater(Vector2.Distance(person.Local, lot.StandPoint), 1.0f, person.Id + " on the keeper of " + lot.Id);
                    }
                }

                bool onStreet = layout.Streets.Any(s => s.Bounds.Contains(person.Local));
                Assert.IsTrue(onStreet || person.Local.magnitude <= layout.PlazaRadius, person.Id + " on the plaza or a street");
                Assert.Greater(Vector2.Distance(person.Local, mast), TarchonLanternRules.MastReach, person.Id + " off the mast's prompt");
                Assert.Greater(Vector2.Distance(person.Local, trade), TarchonLanternRules.NeighbourReach, person.Id + " off Larthia's door");
            }

            for (int i = 0; i < people.Count; i++)
            {
                for (int j = i + 1; j < people.Count; j++)
                {
                    Assert.Greater(Vector2.Distance(people[i].Local, people[j].Local), 1.8f, people[i].Id + " and " + people[j].Id + " stand apart");
                }
            }

            // Larthia's door: the trade point is hers, not a neighbour's prompt.
            Assert.AreEqual(layout.LotOfResident("larthia").DoorApproach, trade);
            foreach (TarchonNeighbour neighbour in OverworldLayouts.TarchonNeighbours)
            {
                Assert.Greater(Vector2.Distance(neighbour.Position - o, trade), TarchonLanternRules.NeighbourReach + 0.5f, neighbour.Id + " near Larthia's door");
            }
        }

        [Test]
        public void CrowdFigures_Build_AndEachHasALine()
        {
            var lines = new HashSet<string>();
            foreach (TarchonNeighbour trol in OverworldLayouts.TarchonCrowd)
            {
                NpcRecipe recipe = TarchonCrowdRecipes.For(trol.Id);
                Assert.Greater(NpcModelBuilder.CountTriangles(recipe), 0, trol.Id);
                Assert.AreNotEqual(NpcGesture.None, recipe.Gesture, trol.Id + " busy at home");
                string key = KirtaTarchonTownTexts.CrowdLine(trol.Id);
                Assert.IsTrue(lines.Add(key), trol.Id + " has its own line");
                Assert.AreNotEqual(key, KirtaTarchonTownTexts.Get(null, key));
            }
        }

        // ------------------------------------------------------------------
        // Tarchon: Larthia buys the wisps
        // ------------------------------------------------------------------

        [Test]
        public void Larthia_BuysTheFreedWisps_GivesHerLanternOnce()
        {
            Assert.AreEqual(1, LarthiaWispRules.Catch(0));
            Assert.AreEqual(LarthiaWispRules.MaxHeld, LarthiaWispRules.Catch(LarthiaWispRules.MaxHeld));
            Assert.IsTrue(LarthiaWispRules.Full(LarthiaWispRules.MaxHeld));
            Assert.IsFalse(LarthiaWispRules.Full(9));
            Assert.AreEqual(4 * LarthiaWispRules.PricePerWisp, LarthiaWispRules.PriceFor(4));
            Assert.AreEqual(0, LarthiaWispRules.PriceFor(-3));
            Assert.IsTrue(LarthiaWispRules.CanSell(1, 2f, true));
            Assert.IsFalse(LarthiaWispRules.CanSell(0, 1f, true), "no wisps");
            Assert.IsFalse(LarthiaWispRules.CanSell(3, 3f, true), "too far");
            Assert.IsFalse(LarthiaWispRules.CanSell(3, 1f, false));
            Assert.IsFalse(LarthiaWispRules.GiftDue(19, false));
            Assert.IsTrue(LarthiaWispRules.GiftDue(20, false));
            Assert.IsFalse(LarthiaWispRules.GiftDue(40, true), "once");
            Assert.IsNotNull(Resources.Load<Rapadura.Gameplay.Items.ItemDefinition>("Items/" + TarchonLanternRules.WispLanternItem));
        }

        // ------------------------------------------------------------------
        // Tarchon: the daily
        // ------------------------------------------------------------------

        [Test]
        public void Daily_StartsOncePerGameDay()
        {
            Assert.AreEqual(TarchonDailyStep.Begin, TarchonDailyRules.Decide(0, 1), "never handed out");
            Assert.AreEqual(TarchonDailyStep.Begin, TarchonDailyRules.Decide(6, 7), "a new day");
            Assert.AreEqual(TarchonDailyStep.None, TarchonDailyRules.Decide(7, 7));
            Assert.AreEqual(0, TarchonDailyRules.Credit(0));
            Assert.AreEqual(2, TarchonDailyRules.Credit(2));
            Assert.AreEqual(TarchonDailyRules.Required, TarchonDailyRules.Credit(5));
        }

        [Test]
        public void Daily_IsARealDailyQuest_FoundLikeTheCityQuests_AndRepeatable()
        {
            QuestDefinition daily = CityQuestCatalog.GetQuest(TarchonDailyRules.QuestId);
            Assert.IsNotNull(daily, "found through the city catalog (dialogue, saves)");
            Assert.AreSame(daily, CityDailyQuestCatalog.GetQuest(TarchonDailyRules.QuestId));
            Assert.IsTrue(CityQuestCatalog.GetAllQuests().Contains(daily), "in the quest log");
            Assert.AreEqual(QuestType.Daily, daily.QuestType);
            QuestObjective objective = daily.Objectives.Single();
            Assert.AreEqual(QuestObjectiveType.CustomFlag, objective.Type);
            Assert.AreEqual(TarchonDailyRules.ObjectiveTarget, objective.TargetId);
            Assert.AreEqual(TarchonDailyRules.Required, objective.RequiredAmount);
            Assert.GreaterOrEqual(daily.RewardExperience, TarchonDailyRules.AuthoredXp);
            Assert.IsTrue(daily.RewardItems.All(r => r.Item != null), "reward items exist");

            foreach (string key in new[] { daily.TitleKey, daily.DescriptionKey, objective.DescriptionKey })
            {
                Assert.IsNotNull(CityQuestTexts.GetFallback(key), key);
            }

            var quests = new QuestManager();
            Assert.IsTrue(quests.StartQuest(daily));
            quests.UpdateObjectiveProgress(QuestObjectiveType.CustomFlag, TarchonDailyRules.ObjectiveTarget, 1);
            quests.UpdateObjectiveProgress(QuestObjectiveType.CustomFlag, "something_else", 1);
            Assert.IsTrue(quests.IsActive(daily.QuestId));
            quests.UpdateObjectiveProgress(QuestObjectiveType.CustomFlag, TarchonDailyRules.ObjectiveTarget, 2);
            Assert.IsTrue(quests.IsCompleted(daily.QuestId), "three wakes finish it");
            Assert.IsFalse(quests.StartQuest(daily), "not twice the same day");
            Assert.IsTrue(quests.ResetDailyQuest(daily.QuestId), "the next game day forgets it");
            Assert.IsTrue(quests.StartQuest(daily));
        }

        [Test]
        public void Texts_CousinsLines_NoFolksyTitles()
        {
            foreach (CharacterId hero in new[] { CharacterId.Maithe, CharacterId.Icaro, CharacterId.Joaquim, CharacterId.Maria, CharacterId.Lavine })
            {
                Assert.AreNotEqual(KirtaTarchonTownTexts.SellLineFor(hero), KirtaTarchonTownTexts.Get(null, KirtaTarchonTownTexts.SellLineFor(hero)));
            }

            Assert.AreEqual(5, new[] { CharacterId.Maithe, CharacterId.Icaro, CharacterId.Joaquim, CharacterId.Maria, CharacterId.Lavine }
                .Select(h => KirtaTarchonTownTexts.SellLineFor(h)).Distinct().Count());
            foreach (var entry in KirtaTarchonTownTexts.Entries.Concat(CityDailyQuestTexts.All))
            {
                Assert.IsFalse(string.IsNullOrEmpty(entry.PtBR) || string.IsNullOrEmpty(entry.En), entry.Key);
                Assert.IsFalse(entry.PtBR.Contains("Dona ") || entry.PtBR.Contains("Seu "), entry.Key);
            }
        }

        // ------------------------------------------------------------------
        // The places: corals on the reef's trails, neighbours in the town
        // ------------------------------------------------------------------

        [Test]
        public void Corals_StandOnTheReefTrails_OutOfTheWater_OffTheTownsAndLandmarks()
        {
            OverworldLayout costa = OverworldLayouts.Get(OverworldLayouts.CostaId);
            foreach (KirtaCoral coral in OverworldLayouts.KirtaCorals)
            {
                float trail = float.PositiveInfinity;
                foreach (MapPath path in costa.World.Paths)
                {
                    trail = Mathf.Min(trail, SegmentDistance(coral.Position, path.A, path.B) - path.Width * 0.5f);
                }

                Assert.LessOrEqual(trail, 0f, coral.Id + " on a trail");
                foreach (MapRiver river in costa.World.Rivers)
                {
                    for (int i = 0; i + 1 < river.Points.Count; i++)
                    {
                        Assert.Greater(SegmentDistance(coral.Position, river.Points[i], river.Points[i + 1]) - river.Width * 0.5f, KirtaCoralRules.MendRadius,
                            coral.Id + " and its ring out of the water");
                    }
                }

                Assert.IsNull(OverworldRules.SettlementAt(costa, coral.Position), coral.Id + " off every town's ground and its lots");
                foreach (MapLandmark landmark in costa.World.Landmarks)
                {
                    Assert.Greater(Vector2.Distance(coral.Position, landmark.Position), KirtaCoralRules.SparkReach + 2f, coral.Id + " off the " + landmark.Kind);
                }
            }
        }

        private static float SegmentDistance(Vector2 p, Vector2 a, Vector2 b)
        {
            Vector2 ab = b - a;
            float t = ab.sqrMagnitude > 1e-6f ? Mathf.Clamp01(Vector2.Dot(p - a, ab) / ab.sqrMagnitude) : 0f;
            return Vector2.Distance(p, a + ab * t);
        }
    }
}
