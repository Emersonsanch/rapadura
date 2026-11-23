using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rapadura.Gameplay.World.Cities.Town;
using Rapadura.Gameplay.World.Map.Mechanics;
using Rapadura.Gameplay.World.Map.Overworld;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// The progress-driven looks of the towns in the continuous world: which variant a save asks for (<see cref="CityTownVariantRules"/>,
    /// pure, world state) and that the town built in each variant changes only the pieces it should (Kirta: the mother coral; Tarchon: the
    /// festive lamps), so a live swap (<see cref="OverworldTownVariants"/>, <c>CityTownView.ApplyLayout</c>) never moves a lot or a collider.
    /// </summary>
    public class CityTownVariantTests
    {
        private static System.Func<string, long> Save(Dictionary<string, long> marks) => key => marks.TryGetValue(key, out long v) ? v : 0L;

        private static Dictionary<string, long> CoralsLit(int count)
        {
            var marks = new Dictionary<string, long>();
            foreach (KirtaCoral coral in OverworldLayouts.KirtaCorals.Take(count))
            {
                marks[KirtaCoralRules.MarkKey(coral.Id)] = 1_700_000_000L;
            }

            return marks;
        }

        // ------------------------------------------------------------------
        // Which variant for which save
        // ------------------------------------------------------------------

        [Test]
        public void Variant_FreshSaveAndOtherTowns_AreZero()
        {
            Assert.AreEqual(0, CityTownVariantRules.VariantFor(CityTownRecipes.KirtaCityId, null));
            Assert.AreEqual(0, CityTownVariantRules.VariantFor(CityTownRecipes.KirtaCityId, Save(new Dictionary<string, long>())));
            Assert.AreEqual(0, CityTownVariantRules.VariantFor(CityTownRecipes.TarchonCityId, Save(new Dictionary<string, long>())));
            Assert.AreEqual(0, CityTownVariantRules.VariantFor("vila", Save(CoralsLit(5))));
            Assert.IsFalse(CityTownVariantRules.HasVariants("vila"));
            Assert.IsNull(CityTownVariantRules.RecipeFor("vila", 1), "a town without variants keeps the recipe it was placed with");
        }

        [Test]
        public void Variant_Kirta_IsTheCoralsLit()
        {
            for (int lit = 0; lit <= KirtaCoralRules.CoralCount; lit++)
            {
                Assert.AreEqual(lit, CityTownVariantRules.VariantFor(CityTownRecipes.KirtaCityId, Save(CoralsLit(lit))), lit + " corals");
            }

            Dictionary<string, long> marks = CoralsLit(2);
            marks[TarchonLanternRules.TotalKey] = 80;
            marks["kirta.coral.desconhecido"] = 1;
            Assert.AreEqual(2, CityTownVariantRules.VariantFor(CityTownRecipes.KirtaCityId, Save(marks)), "only the five corals of the reef count");
        }

        [Test]
        public void Variant_Tarchon_IsTheLanternPairs_SoItChangesOnlyTwice()
        {
            var expected = new Dictionary<long, int> { { 0, 0 }, { 3, 0 }, { 9, 0 }, { 10, 1 }, { 54, 1 }, { 55, 2 }, { 80, 2 }, { 9999, 2 } };
            foreach (KeyValuePair<long, int> pair in expected)
            {
                var marks = new Dictionary<string, long> { { TarchonLanternRules.TotalKey, pair.Key } };
                Assert.AreEqual(pair.Value, CityTownVariantRules.VariantFor(CityTownRecipes.TarchonCityId, Save(marks)), pair.Key + " woken");
            }

            for (int pairs = 0; pairs <= 2; pairs++)
            {
                int returned = CityTownVariantRules.TarchonReturnedFor(pairs);
                Assert.AreEqual(pairs, TarchonCrowdRules.LanternPairs(returned));
                Assert.IsTrue(returned == 0 || TarchonCrowdRules.LanternPairs(returned - 1) < pairs, "the fewest trols for that look");
            }
        }

        [Test]
        public void Recipe_ForVariant_HasNoBackdrop_LikeTheWorldPlacesIt()
        {
            Assert.IsFalse(CityTownVariantRules.RecipeFor(CityTownRecipes.KirtaCityId, 3).Backdrop);
            Assert.IsFalse(CityTownVariantRules.RecipeFor(CityTownRecipes.TarchonCityId, 2).Backdrop);
        }

        // ------------------------------------------------------------------
        // Only the expected pieces change
        // ------------------------------------------------------------------

        private static CityLayout Generate(string cityId, int variant) => CityLayoutGenerator.Generate(CityTownVariantRules.RecipeFor(cityId, variant));

        private static void AssertSameLots(CityLayout a, CityLayout b)
        {
            Assert.AreEqual(a.Lots.Count, b.Lots.Count);
            for (int i = 0; i < a.Lots.Count; i++)
            {
                Assert.AreEqual(a.Lots[i].Id, b.Lots[i].Id);
                Assert.AreEqual(a.Lots[i].Center, b.Lots[i].Center, a.Lots[i].Id);
                Assert.AreEqual(a.Lots[i].Footprint, b.Lots[i].Footprint, a.Lots[i].Id);
                Assert.AreEqual(a.Lots[i].StandPoint, b.Lots[i].StandPoint, a.Lots[i].Id);
            }
        }

        private static string PropKey(CityProp p) => p.Kind + "@" + p.Position.x.ToString("F3") + "," + p.Position.y.ToString("F3") + ">" + p.End.x.ToString("F3") + "," + p.End.y.ToString("F3");

        [Test]
        public void Kirta_EachVariant_ChangesOnlyTheMotherCoral()
        {
            CityLayout start = Generate(CityTownRecipes.KirtaCityId, 0);
            for (int v = 1; v <= KirtaCoralRules.CoralCount; v++)
            {
                CityLayout layout = Generate(CityTownRecipes.KirtaCityId, v);
                AssertSameLots(start, layout);
                Assert.IsTrue(OverworldTownVariants.SameSolids(start, layout), "the colliders stay put: v" + v);
                Assert.IsTrue(OverworldTownVariants.SameLamps(start, layout));
                Assert.AreEqual(start.Props.Count, layout.Props.Count);
                for (int i = 0; i < start.Props.Count; i++)
                {
                    Assert.AreEqual(PropKey(start.Props[i]), PropKey(layout.Props[i]));
                    if (start.Props[i].Kind == CityPropKind.CoralMother)
                    {
                        Assert.AreEqual(v, layout.Props[i].Variant, "the mother coral carries the corals lit");
                    }
                    else
                    {
                        Assert.AreEqual(start.Props[i].Variant, layout.Props[i].Variant, PropKey(start.Props[i]));
                    }
                }
            }
        }

        [Test]
        public void Tarchon_EachVariant_OnlyAddsFestiveLampsAndTheirPosts()
        {
            CityLayout start = Generate(CityTownRecipes.TarchonCityId, 0);
            List<string> before = start.Props.Select(PropKey).ToList();
            for (int v = 1; v <= 2; v++)
            {
                CityLayout layout = Generate(CityTownRecipes.TarchonCityId, v);
                AssertSameLots(start, layout);

                List<string> after = layout.Props.Select(PropKey).ToList();
                foreach (string key in before)
                {
                    Assert.IsTrue(after.Remove(key), "kept: " + key);
                }

                Assert.AreEqual(v * 2, after.Count, "two lamps per pair");
                Assert.IsTrue(after.All(k => k.StartsWith(CityPropKind.LampPost + "@")), string.Join(" ", after));

                // Every collider of the start is still there, in its place; the new ones are the lamp posts alone.
                foreach (CitySolid solid in start.Solids)
                {
                    Assert.IsTrue(layout.Solids.Any(s => s.IsCapsule == solid.IsCapsule && s.Footprint == solid.Footprint), "solid kept: " + solid.Center);
                }

                Assert.AreEqual(start.Solids.Count + v * 2, layout.Solids.Count);
                Assert.IsFalse(OverworldTownVariants.SameSolids(start, layout));
                Assert.IsFalse(OverworldTownVariants.SameLamps(start, layout));
            }
        }

        // ------------------------------------------------------------------
        // The swap in the world
        // ------------------------------------------------------------------

        [Test]
        public void World_VariantLayouts_FitTheSettlement_AndAreBuiltOnce()
        {
            OverworldLayout costa = OverworldLayouts.Get(OverworldLayouts.CostaId);
            OverworldSettlement kirta = costa.Settlements.Single(s => s.Id == CityTownRecipes.KirtaCityId);
            Assert.AreSame(kirta.Town, OverworldTownVariants.LayoutFor(kirta, 0), "variant 0 is the world's own town");

            CityLayout lit = OverworldTownVariants.LayoutFor(kirta, 4);
            Assert.AreNotSame(kirta.Town, lit);
            Assert.AreSame(lit, OverworldTownVariants.LayoutFor(kirta, 4), "generated once per variant");
            Assert.AreEqual(kirta.Town.WalkBounds, lit.WalkBounds);
            Assert.AreEqual(kirta.Town.SafeZone, lit.SafeZone);
            Assert.AreEqual(kirta.Town.GroundHalfExtent, lit.GroundHalfExtent);
            Assert.IsTrue(OverworldTownVariants.SameSolids(kirta.Town, lit), "fitted to the world, the colliders still match");

            OverworldSettlement other = costa.Settlements.First(s => !CityTownVariantRules.HasVariants(s.Id));
            Assert.AreSame(other.Town, OverworldTownVariants.LayoutFor(other, 3));

            Assert.AreEqual("ow:k", OverworldTownVariants.PropsKey("ow:k", 0), "variant 0 keeps the cached props of before");
            Assert.AreNotEqual(OverworldTownVariants.PropsKey("ow:k", 1), OverworldTownVariants.PropsKey("ow:k", 2));
        }

        [Test]
        public void Swap_WaitsWhileTheHeroStandsWhereANewLampGoes()
        {
            CityLayout start = Generate(CityTownRecipes.TarchonCityId, 0);
            CityLayout festive = Generate(CityTownRecipes.TarchonCityId, 1);
            CityProp added = festive.Props.First(p => p.Kind == CityPropKind.LampPost && !start.Props.Any(q => q.Kind == p.Kind && q.Position == p.Position));

            Assert.IsTrue(OverworldTownVariants.BlocksHero(start, festive, added.Position, 0.6f), "on the new post");
            Assert.IsTrue(OverworldTownVariants.BlocksHero(start, festive, added.Position + new Vector2(0.5f, 0f), 0.6f), "touching it");
            Assert.IsFalse(OverworldTownVariants.BlocksHero(start, festive, added.Position + new Vector2(1.5f, 0f), 0.6f), "a step away");
            Assert.IsFalse(OverworldTownVariants.BlocksHero(start, festive, Vector2.zero, 0.6f), "in the middle of the plaza");

            // A lamp that was already there never holds the swap (the hero cannot stand inside it anyway).
            CityProp old = start.Props.First(p => p.Kind == CityPropKind.LampPost);
            Assert.IsFalse(OverworldTownVariants.BlocksHero(start, festive, old.Position, 0.6f));

            CityLayout kirta0 = Generate(CityTownRecipes.KirtaCityId, 0);
            CityLayout kirta5 = Generate(CityTownRecipes.KirtaCityId, 5);
            Assert.IsFalse(OverworldTownVariants.BlocksHero(kirta0, kirta5, CityTownRecipes.KirtaMotherCoral, 0.6f), "Kirta adds no solid");
        }
    }
}
