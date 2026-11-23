using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rapadura.Gameplay.World.Cities.Town;
using Rapadura.Gameplay.World.Map.Mechanics;
using Rapadura.Gameplay.World.Map.Overworld;

namespace Rapadura.Tests
{
    /// <summary>
    /// The city mode (the walkable town outside the continuous world) builds Kirta and Tarchon in the look the save asks for
    /// (<see cref="CityTownVariantRules.VariantFor"/>), and the caravaneer's daily stall still comes and goes on top of any look.
    /// </summary>
    public class CityModeTownVariantTests
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

        private static int MotherCoralVariant(CityLayout layout) => layout.Props.Single(p => p.Kind == CityPropKind.CoralMother).Variant;

        private static int Lamps(CityLayout layout) => layout.Props.Count(p => p.Kind == CityPropKind.LampPost);

        [SetUp]
        public void SetUp() => CityTownRecipes.ClearCache();

        [TearDown]
        public void TearDown() => CityTownRecipes.ClearCache();

        [Test]
        public void Kirta_CityMode_FollowsTheCoralsLit()
        {
            Assert.AreEqual(0, MotherCoralVariant(CityTownRecipes.LayoutForSave(CityTownRecipes.KirtaCityId, false, null)), "a fresh save");
            for (int lit = 0; lit <= KirtaCoralRules.CoralCount; lit++)
            {
                CityLayout layout = CityTownRecipes.LayoutForSave(CityTownRecipes.KirtaCityId, false, Save(CoralsLit(lit)));
                Assert.AreEqual(lit, MotherCoralVariant(layout), lit + " corals");
            }
        }

        [Test]
        public void Tarchon_CityMode_FollowsTheLanternPairs()
        {
            int start = Lamps(CityTownRecipes.LayoutForSave(CityTownRecipes.TarchonCityId, false, null));
            var marks = new Dictionary<string, long> { { TarchonLanternRules.TotalKey, 80 } };
            CityLayout festive = CityTownRecipes.LayoutForSave(CityTownRecipes.TarchonCityId, false, Save(marks));
            Assert.AreEqual(start + 4, Lamps(festive), "two pairs of festive lanterns");
            Assert.AreEqual(2, CityTownVariantRules.VariantFor(CityTownRecipes.TarchonCityId, Save(marks)));
        }

        [Test]
        public void Variant_And_DailyMascate_AreIndependent_AndCachedApart()
        {
            string kirta = CityTownRecipes.KirtaCityId;
            CityLayout plain = CityTownRecipes.LayoutFor(kirta, false, 3);
            CityLayout withMascate = CityTownRecipes.LayoutFor(kirta, true, 3);

            Assert.AreEqual(3, MotherCoralVariant(plain));
            Assert.AreEqual(3, MotherCoralVariant(withMascate), "the stall does not undo the look");
            Assert.IsFalse(plain.Lots.Any(l => l.Id == CityTownRecipes.MascateLotId));
            Assert.IsTrue(withMascate.Lots.Any(l => l.Id == CityTownRecipes.MascateLotId), "the caravaneer's stall on top of the look");

            Assert.AreSame(plain, CityTownRecipes.LayoutFor(kirta, false, 3), "cached");
            Assert.AreNotSame(plain, CityTownRecipes.LayoutFor(kirta, false, 0));
            Assert.AreSame(CityTownRecipes.LayoutFor(kirta), CityTownRecipes.LayoutFor(kirta, false, 0), "variant 0 is the old layout");

            Assert.AreEqual(kirta, CityTownRecipes.CacheKey(kirta, false, 0), "variant 0 keeps the plain mesh key");
            Assert.AreEqual(kirta + "+mascate", CityTownRecipes.CacheKey(kirta, true, 0));
            var keys = new HashSet<string>
            {
                CityTownRecipes.CacheKey(kirta, false, 0), CityTownRecipes.CacheKey(kirta, true, 0),
                CityTownRecipes.CacheKey(kirta, false, 3), CityTownRecipes.CacheKey(kirta, true, 3),
            };
            Assert.AreEqual(4, keys.Count, "every look and stall has its own key");
        }

        [Test]
        public void TownWithoutVariants_IgnoresTheVariant()
        {
            string vila = Rapadura.Gameplay.World.Cities.CityCatalog.VilaZefa;
            Assert.AreEqual(CityTownRecipes.LayoutFor(vila).Props.Count, CityTownRecipes.LayoutFor(vila, false, 4).Props.Count);
            Assert.AreEqual(0, CityTownVariantRules.VariantFor(vila, Save(CoralsLit(5))));
        }
    }
}
