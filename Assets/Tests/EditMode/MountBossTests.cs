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
    /// The six mounts of <see cref="MountBossCatalog"/> (Docs/Mounts.md §3.13): four freed by a boss (rare drop + Fitas do Laço) and
    /// the last two map-sheet proposals of the Regions 1 and 6 (Uncommon whistles in the region city). Pure rules only.
    /// </summary>
    public class MountBossTests
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

        private static MountCollection NewCollection(System.Random random)
        {
            int n = 0;
            return new MountCollection { GuidFactory = () => "g" + (++n), UtcNow = () => 0L, DropRandom = random, IsNight = () => false };
        }

        /// <summary>A random that always rolls 0, so every drop with a chance above 0 happens.</summary>
        private sealed class AlwaysLucky : System.Random
        {
            public override double NextDouble() => 0d;
        }

        // ---------------- Catalog ----------------

        [Test]
        public void EverySpecies_IsOnItsRarityBudget_WithinTheLimits_AndInTheCaderno()
        {
            Assert.AreEqual(6, MountBossCatalog.Specs.Count);
            foreach (MountSpec spec in MountBossCatalog.Specs)
            {
                MountDefinition definition = MountCatalog.Get(spec.MountId);
                Assert.IsNotNull(definition, spec.MountId);
                Assert.AreEqual(MountRules.BudgetFor(spec.Rarity), definition.BudgetPoints, 1e-3f, spec.MountId);
                Assert.That(spec.Speed, Is.InRange(MountRules.MinSpeedMultiplier, MountRules.MaxSpeedMultiplier), spec.MountId);
                Assert.That(spec.Defense, Is.InRange(0f, 0.15f), spec.MountId);
                Assert.That(spec.Health, Is.InRange(100f, 375f), spec.MountId);
                Assert.That(spec.Stamina, Is.InRange(100f, 200f), spec.MountId);
                Assert.IsNotNull(definition.Ability, spec.MountId);
                Assert.Greater(definition.Ability.CooldownSeconds, 0f, spec.MountId);
                Assert.IsFalse(definition.Ability.AllowedInSafeZone, spec.MountId);
                Assert.AreNotEqual(MountType.Voador, spec.Type, spec.MountId + ": nobody flies");
                Assert.That(spec.Rarity, Is.InRange(ItemRarity.Uncommon, ItemRarity.Epic), spec.MountId);
                Assert.AreEqual(spec.Rarity, MountCodex.Entries.Single(e => e.MountId == spec.MountId).Rarity, spec.MountId);
                Assert.IsTrue(MountCodex.Entries.Single(e => e.MountId == spec.MountId).IsAvailable, spec.MountId);
            }

            Assert.AreEqual(MountCatalog.Specs.Count, MountCodex.Entries.Count(e => e.IsAvailable));
            CollectionAssert.IsSubsetOf(MountBossCatalog.Specs.Select(s => s.MountId), MountCatalog.MountIds);
        }

        [Test]
        public void RegionsRarities_AndTheTwoSwimmers()
        {
            var expected = new Dictionary<string, (BiomeType Region, ItemRarity Rarity)>
            {
                { MountBossCatalog.LaurelLizard, (BiomeType.Forest, ItemRarity.Uncommon) },
                { MountBossCatalog.JhilpurBuffalo, (BiomeType.Forest, ItemRarity.Uncommon) },
                { MountBossCatalog.PamirArgali, (BiomeType.Mountain, ItemRarity.Rare) },
                { MountBossCatalog.FreedSmokeHorse, (BiomeType.Mountain, ItemRarity.Rare) },
                { MountBossCatalog.TameNinkiNanka, (BiomeType.Forest, ItemRarity.Epic) },
                { MountBossCatalog.LivingEmberBoar, (BiomeType.Mountain, ItemRarity.Epic) },
            };

            CollectionAssert.AreEquivalent(expected.Keys, MountBossCatalog.Specs.Select(s => s.MountId));
            foreach (KeyValuePair<string, (BiomeType Region, ItemRarity Rarity)> pair in expected)
            {
                MountSpec spec = MountCatalog.FindSpec(pair.Key);
                Assert.AreEqual(pair.Value.Region, spec.Region, pair.Key);
                Assert.AreEqual(pair.Value.Rarity, spec.Rarity, pair.Key);
            }

            CollectionAssert.AreEquivalent(new[] { MountBossCatalog.JhilpurBuffalo, MountBossCatalog.TameNinkiNanka },
                MountBossCatalog.Specs.Where(s => s.Type == MountType.Aquatico).Select(s => s.MountId), "the delta swimmers (fichas 31 and 34)");
            Assert.IsTrue(MountCatalog.FindSpec(MountBossCatalog.LivingEmberBoar).IsWarMount, "the forge boar");
        }

        [Test]
        public void TheSix_UseNewKinds_OnePerMount()
        {
            var expected = new Dictionary<string, MountAbilityKind>
            {
                { MountBossCatalog.LaurelLizard, MountAbilityKind.TailDecoy },
                { MountBossCatalog.JhilpurBuffalo, MountAbilityKind.MudWallow },
                { MountBossCatalog.PamirArgali, MountAbilityKind.LedgeWatch },
                { MountBossCatalog.FreedSmokeHorse, MountAbilityKind.SmokeCloud },
                { MountBossCatalog.TameNinkiNanka, MountAbilityKind.ShyDive },
                { MountBossCatalog.LivingEmberBoar, MountAbilityKind.EmberForge },
            };

            foreach (KeyValuePair<string, MountAbilityKind> pair in expected)
            {
                Assert.AreEqual(pair.Value, MountCatalog.Get(pair.Key).Ability.Kind, pair.Key);
                Assert.AreEqual(1, MountCatalog.Specs.Count(s => s.AbilityKind == pair.Value), pair.Value + " is used once in the whole catalog");
            }

            IReadOnlyList<MountSpec> all = MountCatalog.Specs;
            Assert.AreEqual(all.Count, all.Select(s => s.AbilityId).Distinct().Count(), "one ability id per species");
            Assert.AreEqual(all.Count, all.Select(s => s.Recipe).Distinct().Count(), "one model per species");
            Assert.AreEqual(all.Count, all.Select(s => (s.Speed, s.Defense, s.Health, s.Stamina)).Distinct().Count(), "distinct stat lines");
        }

        [Test]
        public void Texts_CoverNameDescLoreSourceAbilityAndToasts_InBothLanguages()
        {
            Dictionary<string, Rapadura.Core.Localization.LocalizationEntry> byKey = MountTexts.Entries.ToDictionary(e => e.Key);
            foreach (MountSpec spec in MountBossCatalog.Specs)
            {
                MountDefinition d = MountCatalog.Get(spec.MountId);
                var keys = new List<string> { d.DisplayNameKey, d.DescriptionKey, d.LoreKey, d.SourceHintKey, d.Ability.NameKey, d.Ability.DescriptionKey };
                if (d.HasFavorite)
                {
                    keys.Add(d.FavoriteLineKey);
                }

                foreach (string key in keys)
                {
                    Assert.IsTrue(byKey.ContainsKey(key), key);
                    Assert.IsFalse(string.IsNullOrEmpty(byKey[key].En), key);
                    Assert.IsFalse(string.IsNullOrEmpty(byKey[key].PtBR), key);
                }
            }

            foreach (string toast in new[]
                     {
                         MountBossTexts.ToastTail, MountBossTexts.ToastTailNone, MountBossTexts.ToastMud, MountBossTexts.ToastWatch,
                         MountBossTexts.ToastSmoke, MountBossTexts.ToastDive, MountBossTexts.ToastDiveSeen, MountBossTexts.ToastForge,
                     })
            {
                Assert.IsTrue(byKey.ContainsKey(toast), toast);
            }

            Assert.AreEqual("Lagarto-do-Louro", byKey[MountCatalog.Get(MountBossCatalog.LaurelLizard).DisplayNameKey].PtBR);
            Assert.AreEqual("Javali de Brasa Viva", byKey[MountCatalog.Get(MountBossCatalog.LivingEmberBoar).DisplayNameKey].PtBR);
            Assert.AreEqual(MountTexts.Entries.Count, MountTexts.Entries.Select(e => e.Key).Distinct().Count(), "no duplicate keys");
        }

        [Test]
        public void EveryLore_NamesACousin_AndNoFolksyTitles()
        {
            string[] cousins = { "Maithe", "Ícaro", "Joaquim", "Maria", "Lavine" };
            Dictionary<string, Rapadura.Core.Localization.LocalizationEntry> byKey = MountTexts.Entries.ToDictionary(e => e.Key);
            foreach (MountSpec spec in MountBossCatalog.Specs)
            {
                MountDefinition d = MountCatalog.Get(spec.MountId);
                string lore = byKey[d.LoreKey].PtBR;
                Assert.IsTrue(cousins.Any(c => lore.Contains(c)), spec.MountId + " lore talks about a cousin");
                foreach (string key in new[] { d.DisplayNameKey, d.DescriptionKey, d.LoreKey, d.SourceHintKey, d.Ability.DescriptionKey })
                {
                    Assert.IsFalse(byKey[key].PtBR.Contains("Dona ") || byKey[key].PtBR.Contains("Seu "), key);
                }
            }
        }

        // ---------------- Sources ----------------

        [Test]
        public void TheUncommonOnes_AreSoldInTheirRegionCity_WithinThePriceBand()
        {
            var expected = new Dictionary<string, string>
            {
                { MountWaveCatalog.WhistleOf(MountBossCatalog.LaurelLizard), CityShopCatalog.ErvariaDacil },
                { MountWaveCatalog.WhistleOf(MountBossCatalog.JhilpurBuffalo), CityShopCatalog.TrocasNuhzadag },
            };

            CollectionAssert.AreEquivalent(expected.Keys, MountBossCatalog.ShopWhistles);
            foreach (KeyValuePair<string, string> pair in expected)
            {
                string whistle = pair.Key;
                MountSpec spec = MountCatalog.FindByWhistle(whistle);
                Assert.IsNotNull(spec, whistle);
                Assert.AreEqual(ItemRarity.Uncommon, spec.Rarity, "Docs/Mounts.md §5.1: Rare and up are never sold");
                Assert.That(spec.ShopPrice, Is.InRange(140, 150), "§5.1 Uncommon band");

                ItemDefinition item = ItemDatabase.GetById(whistle);
                Assert.IsNotNull(item, whistle + " asset missing");
                Assert.AreEqual(spec.ShopPrice, item.Value, whistle);

                CityShopEntry entry = CityShopCatalog.Shops.Single(s => s.ShopId == pair.Value).Entries.SingleOrDefault(e => e.ItemId == whistle);
                Assert.IsNotNull(entry, whistle + " not on the counter");
                Assert.AreEqual(spec.ShopPrice, entry.Price, whistle);
                Assert.IsFalse(entry.Rotating, whistle + " is always on the counter");
                Assert.AreEqual(1, CityShopCatalog.Shops.Count(s => s.Entries.Any(e => e.ItemId == whistle)), whistle + " sold in one city only");
                Assert.IsNull(MountSourceHooks.For(spec.MountId), spec.MountId + " is bought, not given");
            }
        }

        [Test]
        public void TheBossOnes_AreNotSold_AndDropFromTheirBoss_AtTheRarityChance()
        {
            var expected = new Dictionary<string, string>
            {
                { MountBossCatalog.PamirArgali, RegionBossRules.AydaharId },
                { MountBossCatalog.FreedSmokeHorse, RegionBossRules.SmokeStallionId },
                { MountBossCatalog.TameNinkiNanka, RegionBossRules.NinkiNankaId },
                { MountBossCatalog.LivingEmberBoar, RegionBossRules.EmberGluttonId },
            };

            CollectionAssert.AreEquivalent(expected.Select(p => (p.Value, p.Key)), MountBossCatalog.BossSources.Select(s => (s.BossId, s.MountId)));
            foreach (KeyValuePair<string, string> pair in expected)
            {
                MountSpec spec = MountCatalog.FindSpec(pair.Key);
                Assert.AreEqual(0, spec.ShopPrice, pair.Key);
                Assert.IsFalse(CityShopCatalog.Shops.Any(s => s.Entries.Any(e => e.ItemId == spec.WhistleItemId)), pair.Key + " must not be sold");
                Assert.IsNull(MountSourceHooks.For(pair.Key), pair.Key + " comes from the boss only");

                MountDropEntry drop = MountDropRules.Table.Single(e => e.MountId == pair.Key);
                Assert.AreEqual(pair.Value, drop.EnemyId, pair.Key);
                Assert.AreEqual(spec.Rarity == ItemRarity.Rare ? 0.02f : 0.008f, drop.ChanceAt(false), 1e-5f, pair.Key + ": Rare 2%, Epic 0.8% (§3.3/§3.4)");

                Assert.IsTrue(MountDropRules.GivesRibbons(pair.Value), pair.Value);
                Assert.AreEqual(MountDropRules.CostFor(spec.Rarity), MountDropRules.CostOf(pair.Value, pair.Key), pair.Key);
                Assert.IsNotNull(Resources.Load<EnemyDefinition>("Enemies/" + pair.Value), pair.Value + ": the drop reads the asset name");
            }
        }

        [Test]
        public void KillingTheBoss_CanFreeTheMount_AndGivesARibbon()
        {
            MountCollection collection = NewCollection(new AlwaysLucky());
            collection.Initialize();
            try
            {
                List<OwnedMount> granted = collection.ResolveEnemyDeath(RegionBossRules.NinkiNankaId, isBoss: true, isNight: false);
                Assert.AreEqual(new[] { MountBossCatalog.TameNinkiNanka }, granted.Select(m => m.mountId).ToArray());
                Assert.IsTrue(collection.Owns(MountBossCatalog.TameNinkiNanka));
                Assert.AreEqual(MountDropRules.RibbonsPerBossVictory, collection.RibbonsOf(RegionBossRules.NinkiNankaId));
            }
            finally
            {
                collection.Shutdown();
            }

            MountCollection unlucky = NewCollection(new System.Random(1));
            unlucky.Initialize();
            try
            {
                unlucky.AddRibbons(RegionBossRules.AydaharId, MountDropRules.RareRibbonCost);
                Assert.IsNull(unlucky.RedeemRibbons(RegionBossRules.AydaharId, MountBossCatalog.LivingEmberBoar), "only the boss's own mount");
                Assert.IsNotNull(unlucky.RedeemRibbons(RegionBossRules.AydaharId, MountBossCatalog.PamirArgali), "bad-luck protection: 25 ribbons");
                Assert.IsTrue(unlucky.Owns(MountBossCatalog.PamirArgali));
            }
            finally
            {
                unlucky.Shutdown();
            }
        }

        // ---------------- Carry ----------------

        [Test]
        public void EveryOne_HasACarryCapacity_AndTheBuffaloIsTheirBigCarrier()
        {
            foreach (MountSpec spec in MountBossCatalog.Specs)
            {
                Assert.IsTrue(EncumbranceRules.MountBaseCapacity.ContainsKey(spec.MountId), spec.MountId);
            }

            float buffalo = EncumbranceRules.MountBaseCapacity[MountBossCatalog.JhilpurBuffalo];
            Assert.AreEqual(buffalo, MountBossCatalog.Specs.Max(s => EncumbranceRules.MountBaseCapacity[s.MountId]));
            Assert.AreEqual(MountBossCatalog.Specs.Min(s => EncumbranceRules.MountBaseCapacity[s.MountId]),
                EncumbranceRules.MountBaseCapacity[MountBossCatalog.LaurelLizard], "the lizard carries least");
        }

        // ---------------- Rules ----------------

        [Test]
        public void Tail_DropsBehind_AndPullsTowardIt_NeverOntoIt()
        {
            Vector3 tail = MountBossRules.TailPoint(Vector3.zero, new Vector3(0f, 3f, 2f));
            Assert.AreEqual(0f, tail.x, 1e-4f);
            Assert.Less(tail.z, 0f, "behind the lizard");
            Assert.AreEqual(0f, tail.y, 1e-4f);

            MountSpec lizard = MountCatalog.FindSpec(MountBossCatalog.LaurelLizard);
            Assert.AreEqual(lizard.Distance, MountBossRules.TailPull(new Vector3(0f, 0f, -10f), Vector3.zero, lizard.Distance), 1e-4f, "capped");
            Assert.AreEqual(1f - MountBossRules.TailStopDistance, MountBossRules.TailPull(new Vector3(1f, 0f, 0f), Vector3.zero, lizard.Distance), 1e-4f);
            Assert.AreEqual(0f, MountBossRules.TailPull(new Vector3(0.2f, 0f, 0f), Vector3.zero, lizard.Distance), 1e-4f);
            Assert.AreEqual(lizard.Duration, MountBossRules.ForgetSeconds(lizard.Duration, false, lizard.BossMultiplier), 1e-4f);
            Assert.AreEqual(0f, MountBossRules.ForgetSeconds(lizard.Duration, true, lizard.BossMultiplier), 1e-4f, "bosses are not fooled");
        }

        [Test]
        public void Cloud_HoldsOnlyCommonsInside()
        {
            MountSpec horse = MountCatalog.FindSpec(MountBossCatalog.FreedSmokeHorse);
            Assert.IsTrue(MountBossRules.InCloud(Vector3.zero, new Vector3(horse.Radius - 0.5f, 0f, 0f), horse.Radius));
            Assert.IsFalse(MountBossRules.InCloud(Vector3.zero, new Vector3(horse.Radius + 0.5f, 0f, 0f), horse.Radius));
            Assert.AreEqual(0f, MountBossRules.ForgetSeconds(MountBossRules.CloudHoldSeconds, true, horse.BossMultiplier), 1e-4f, "bosses walk through");
            Assert.Greater(MountBossRules.CloudHoldSeconds, MountBossRules.CloudTickSeconds, "no gap between ticks");
        }

        [Test]
        public void Dive_SlowsOnlyWhoLooks_AndBossesHalf()
        {
            MountSpec ninki = MountCatalog.FindSpec(MountBossCatalog.TameNinkiNanka);
            Vector3 mount = Vector3.zero;
            Assert.IsTrue(MountBossRules.LooksAt(new Vector3(0f, 0f, 5f), Vector3.back, mount, ninki.Radius), "facing her");
            Assert.IsFalse(MountBossRules.LooksAt(new Vector3(0f, 0f, 5f), Vector3.forward, mount, ninki.Radius), "back turned, like Lavine");
            Assert.IsFalse(MountBossRules.LooksAt(new Vector3(0f, 0f, ninki.Radius + 1f), Vector3.back, mount, ninki.Radius), "too far");
            Assert.AreEqual(ninki.Duration, MountBossRules.GazeSlowSeconds(ninki.Duration, false, ninki.BossMultiplier), 1e-4f);
            Assert.AreEqual(ninki.Duration * 0.5f, MountBossRules.GazeSlowSeconds(ninki.Duration, true, ninki.BossMultiplier), 1e-4f);
            Assert.That(ninki.Magnitude, Is.InRange(0.1f, 0.5f), "a slow, never a root");
        }

        [Test]
        public void GroupBuffs_AreShort_AndNeverAlwaysOn()
        {
            foreach (string id in new[] { MountBossCatalog.JhilpurBuffalo, MountBossCatalog.PamirArgali, MountBossCatalog.LivingEmberBoar })
            {
                MountSpec spec = MountCatalog.FindSpec(id);
                Assert.That(spec.Duration, Is.InRange(5f, 12f), id);
                Assert.Greater(spec.Cooldown, spec.Duration * 2f, id + ": never always on");
                Assert.Greater(spec.Radius, 0f, id);
                Assert.That(spec.Magnitude, Is.InRange(0.05f, 0.3f), id);
            }
        }

        // ---------------- Models ----------------

        [Test]
        public void Models_StayInBudget_KeepTheRiderSeat_AndAreBigEnoughToRide()
        {
            QuadrupedShape pony = MountRecipe.ShapeFor(MountVisualRecipe.Pony);
            foreach (MountSpec spec in MountBossCatalog.Specs)
            {
                QuadrupedShape shape = MountRecipe.ShapeFor(spec.Recipe);
                Assert.GreaterOrEqual(shape.BodyLength, pony.BodyLength, spec.MountId + ": at least as long as the pony");
                Assert.GreaterOrEqual(shape.SeatHeight, 0.55f, spec.MountId + ": no lower than the lantern olm");
                Assert.IsFalse(shape.FlightWings, spec.MountId);
                Assert.IsFalse(shape.Sail, spec.MountId);
                Assert.IsFalse(shape.Humps, spec.MountId);

                ProceduralEnemyBlueprint blueprint = MountModels.CreateBlueprint(spec.Recipe);
                try
                {
                    Assert.LessOrEqual(blueprint.TriangleCount, MountModels.TriangleBudget, spec.MountId);
                    Assert.GreaterOrEqual(blueprint.IndexOf(MountParts.Seat), 0, spec.MountId);
                }
                finally
                {
                    blueprint.DestroyMeshes();
                }
            }

            Assert.Greater(MountRecipe.ShapeFor(MountVisualRecipe.JhilpurBuffalo).BodyWidth, MountRecipe.ShapeFor(MountVisualRecipe.PamirArgali).BodyWidth);
            Assert.IsTrue(MountRecipe.ShapeFor(MountVisualRecipe.LivingEmberBoar).Tusks);
            Assert.IsTrue(MountRecipe.ShapeFor(MountVisualRecipe.LivingEmberBoar).FlameCrest);
        }
    }
}
