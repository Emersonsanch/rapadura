using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rapadura.Core.DI;
using Rapadura.Core.Events;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Enemies.Visuals;
using Rapadura.Gameplay.Inventory;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.Mounts;
using Rapadura.Gameplay.Mounts.Visuals;
using Rapadura.Gameplay.World.Biomes;
using Rapadura.Gameplay.World.Cities;
using UnityEngine;

namespace Rapadura.Tests.EditMode
{
    /// <summary>
    /// The legendary of the map sheets (<see cref="MountLegendCatalog"/>, Docs/Mounts.md §3.14): the Barquinha de Luz that Apep keeps in her
    /// coils (ficha 76). Budget, source (0.3% drop of Apep, never Fitas, never sold), the freeing line, the float and the ability's numbers.
    /// </summary>
    public class MountLegendTests
    {
        [SetUp]
        public void SetUp()
        {
            EventBus.Clear();
            ServiceLocator.Clear();
        }

        [TearDown]
        public void TearDown()
        {
            EventBus.Clear();
            ServiceLocator.Clear();
        }

        private sealed class AlwaysLucky : System.Random
        {
            public override double NextDouble() => 0d;
        }

        [Test]
        public void TheSkiff_IsLegendary_OnBudget_Aquatic_AndInTheCaderno()
        {
            Assert.AreEqual(1, MountLegendCatalog.Specs.Count);
            MountSpec spec = MountLegendCatalog.Specs[0];
            Assert.AreEqual(MountLegendCatalog.LightSkiff, spec.MountId);
            MountDefinition definition = MountCatalog.Get(spec.MountId);
            Assert.IsNotNull(definition);
            Assert.AreEqual(ItemRarity.Legendary, spec.Rarity);
            Assert.AreEqual(MountRules.BudgetFor(ItemRarity.Legendary), definition.BudgetPoints, 1e-3f, "26 points (§2.4)");
            Assert.That(spec.Speed, Is.InRange(MountRules.MinSpeedMultiplier, MountRules.MaxSpeedMultiplier));
            Assert.That(spec.Defense, Is.InRange(0f, 0.15f));
            Assert.That(spec.Health, Is.InRange(100f, 375f));
            Assert.That(spec.Stamina, Is.InRange(100f, 200f));
            Assert.AreEqual(MountType.Aquatico, spec.Type, "anda sobre água (ficha 76)");
            Assert.AreEqual(BiomeType.Desert, spec.Region);
            Assert.AreEqual(MountAbilityKind.LightCrossing, definition.Ability.Kind);
            Assert.IsFalse(definition.Ability.AllowedInSafeZone);
            Assert.AreEqual(1, MountCatalog.Specs.Count(s => s.AbilityKind == MountAbilityKind.LightCrossing), "one effect per mount");
            Assert.AreEqual(1, MountCatalog.Specs.Count(s => s.Recipe == MountVisualRecipe.LightSkiff), "one model per species");
            CollectionAssert.Contains(MountCatalog.MountIds, spec.MountId);

            MountCodexEntry entry = MountCodex.Entries.Single(e => e.MountId == spec.MountId);
            Assert.AreEqual(ItemRarity.Legendary, entry.Rarity);
            Assert.IsTrue(entry.IsAvailable);
            Assert.AreEqual(MountCatalog.Specs.Count, MountCodex.Entries.Count(e => e.IsAvailable));
            Assert.IsTrue(EncumbranceRules.MountBaseCapacity.ContainsKey(spec.MountId));
        }

        [Test]
        public void OnlyApepFreesIt_AtTheTopOfTheLegendaryBand_NeverFitas_NeverSold()
        {
            CollectionAssert.AreEqual(new[] { (RegionBossRules.ApepId, MountLegendCatalog.LightSkiff) }, MountLegendCatalog.BossSources.ToArray());
            MountDropEntry drop = MountDropRules.Table.Single(e => e.MountId == MountLegendCatalog.LightSkiff);
            Assert.AreEqual(RegionBossRules.ApepId, drop.EnemyId);
            Assert.AreEqual(0.003f, drop.ChanceAt(false), 1e-6f);
            Assert.AreEqual(drop.ChanceAt(false), drop.ChanceAt(true), 1e-6f, "always night on the sandbank: one chance");
            Assert.IsFalse(MountDropRules.GivesRibbons(RegionBossRules.ApepId), "Legendaries never come from Fitas (§3.5)");
            Assert.AreEqual(-1, MountDropRules.CostFor(ItemRarity.Legendary));

            MountSpec spec = MountCatalog.FindSpec(MountLegendCatalog.LightSkiff);
            Assert.AreEqual(0, spec.ShopPrice);
            Assert.IsFalse(CityShopCatalog.Shops.Any(s => s.Entries.Any(e => e.ItemId == spec.WhistleItemId)), "never sold");
            Assert.IsNull(MountSourceHooks.For(spec.MountId), "no quest gives it");
            Assert.IsNotNull(Resources.Load<EnemyDefinition>("Enemies/" + RegionBossRules.ApepId), "the drop reads the asset name");
        }

        [Test]
        public void BeatingApep_CanFreeTheSkiff_WithItsFreedLine()
        {
            int n = 0;
            var collection = new MountCollection { GuidFactory = () => "g" + (++n), UtcNow = () => 0L, DropRandom = new AlwaysLucky(), IsNight = () => true };
            collection.Initialize();
            try
            {
                List<OwnedMount> granted = collection.ResolveEnemyDeath(RegionBossRules.ApepId, isBoss: true, isNight: true);
                CollectionAssert.Contains(granted.Select(m => m.mountId).ToList(), MountLegendCatalog.LightSkiff);
                Assert.IsTrue(collection.Owns(MountLegendCatalog.LightSkiff));
                Assert.AreEqual(0, collection.RibbonsOf(RegionBossRules.ApepId), "no Fitas from Apep");
            }
            finally
            {
                collection.Shutdown();
            }

            string key = MountBossRules.FreedLineKey(MountLegendCatalog.LightSkiff);
            Assert.AreEqual("ui.mount.boss.freed." + MountLegendCatalog.LightSkiff, key);
            Assert.IsTrue(MountTexts.Entries.Any(e => e.Key == key), "the freeing line is localized");
        }

        [Test]
        public void Texts_BothLanguages_LoreNamesACousin_NoFolksyTitles()
        {
            Dictionary<string, Rapadura.Core.Localization.LocalizationEntry> byKey = MountTexts.Entries.ToDictionary(e => e.Key);
            MountDefinition d = MountCatalog.Get(MountLegendCatalog.LightSkiff);
            foreach (string key in new[] { d.DisplayNameKey, d.DescriptionKey, d.LoreKey, d.SourceHintKey, d.Ability.NameKey, d.Ability.DescriptionKey,
                         d.FavoriteLineKey, MountLegendTexts.ToastCrossing, MountLegendTexts.ToastCrossingDazzle })
            {
                Assert.IsTrue(byKey.ContainsKey(key), key);
                Assert.IsFalse(string.IsNullOrEmpty(byKey[key].En), key);
                Assert.IsFalse(string.IsNullOrEmpty(byKey[key].PtBR), key);
                Assert.IsFalse(byKey[key].PtBR.Contains("Dona ") || byKey[key].PtBR.Contains("Seu "), key);
            }

            Assert.AreEqual("Barquinha de Luz", byKey[d.DisplayNameKey].PtBR);
            StringAssert.Contains("Maithe", byKey[d.LoreKey].PtBR);
            Assert.AreEqual(MountTexts.Entries.Count, MountTexts.Entries.Select(e => e.Key).Distinct().Count(), "no duplicate keys");
        }

        [Test]
        public void Float_AMetreOverTheGround_LowInCities_NeverUnder()
        {
            Assert.IsTrue(MountLegendRules.Floats(MountVisualRecipe.LightSkiff));
            foreach (MountSpec spec in MountCatalog.Specs.Where(s => s.MountId != MountLegendCatalog.LightSkiff))
            {
                Assert.IsFalse(MountLegendRules.Floats(spec.Recipe), spec.MountId + " walks");
            }

            for (float t = 0f; t < 5f; t += 0.13f)
            {
                float high = MountLegendRules.Float(t, true, false);
                Assert.That(high, Is.InRange(MountLegendRules.FloatHeight - MountLegendRules.FloatBob, MountLegendRules.FloatHeight + MountLegendRules.FloatBob));
                float low = MountLegendRules.Float(t, false, false);
                Assert.Less(low, high, "glides low where flying is not allowed (§9.2)");
                Assert.GreaterOrEqual(low, 0f);
                Assert.LessOrEqual(Mathf.Abs(MountLegendRules.Float(t, true, true) - MountLegendRules.FloatHeight), MountLegendRules.FloatBob * 0.25f + 1e-5f,
                    "reduced motion barely bobs");
            }

            Assert.AreEqual(1f, MountLegendRules.FloatHeight, 1e-6f, "flutua 1 m (ficha 76)");
        }

        [Test]
        public void Crossing_ShortGroupSpeed_NeverAlwaysOn_BossesNotDazzled()
        {
            MountSpec spec = MountCatalog.FindSpec(MountLegendCatalog.LightSkiff);
            Assert.That(spec.Duration, Is.InRange(5f, 12f));
            Assert.Greater(spec.Cooldown, spec.Duration * 2f, "never always on");
            Assert.That(spec.Magnitude, Is.InRange(0.05f, 0.3f));
            Assert.Greater(spec.Radius, 0f);
            Assert.AreEqual(MountLegendRules.CrossingDazzleSeconds, MountLegendRules.DazzleSeconds(false, spec.BossMultiplier), 1e-6f);
            Assert.AreEqual(0f, MountLegendRules.DazzleSeconds(true, spec.BossMultiplier), 1e-6f);
        }

        [Test]
        public void Model_StaysInBudget_KeepsTheSeat_NoWingsNoSail()
        {
            QuadrupedShape shape = MountRecipe.ShapeFor(MountVisualRecipe.LightSkiff);
            Assert.GreaterOrEqual(shape.BodyLength, MountRecipe.ShapeFor(MountVisualRecipe.Pony).BodyLength);
            Assert.GreaterOrEqual(shape.SeatHeight, 0.55f);
            Assert.IsFalse(shape.FlightWings);
            Assert.IsFalse(shape.Sail, "the Dhow-Tartaruga keeps the only sail");
            Assert.IsTrue(shape.Jewel, "the light on the prow");

            ProceduralEnemyBlueprint blueprint = MountModels.CreateBlueprint(MountVisualRecipe.LightSkiff);
            try
            {
                Assert.LessOrEqual(blueprint.TriangleCount, MountModels.TriangleBudget);
                Assert.GreaterOrEqual(blueprint.IndexOf(MountParts.Seat), 0);
            }
            finally
            {
                blueprint.DestroyMeshes();
            }
        }
    }
}
