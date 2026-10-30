using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rapadura.Core.DI;
using Rapadura.Core.Events;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.Mounts;
using Rapadura.Gameplay.Mounts.Visuals;
using Rapadura.Gameplay.World.Cities;
using UnityEngine;

namespace Rapadura.Tests.EditMode
{
    /// <summary>Phase M3 wave 1 (Docs/Mounts.md §13 M3.1): the 18 walkers, their abilities, models and sources.</summary>
    public class MountWaveTests
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

        private static MountCollection NewCollection(int seed = 1)
        {
            int n = 0;
            return new MountCollection { GuidFactory = () => "g" + (++n), UtcNow = () => 0L, DropRandom = new System.Random(seed), IsNight = () => false };
        }

        // ---------------- Catalog ----------------

        [Test]
        public void Wave_Has18Walkers_EachExactlyOnItsRarityBudget()
        {
            Assert.AreEqual(18, MountWaveCatalog.Specs.Count);
            foreach (MountSpec spec in MountWaveCatalog.Specs)
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
                Assert.AreNotEqual(MountType.Voador, spec.Type, "wave 1 is walkers only");
            }
        }

        [Test]
        public void Catalog_IdsAbilitiesKindsAndStatLines_AreAllDistinct()
        {
            var specs = MountCatalog.Specs;
            Assert.AreEqual(specs.Count, specs.Select(s => s.MountId).Distinct().Count());
            Assert.AreEqual(specs.Count, specs.Select(s => s.AbilityId).Distinct().Count());
            Assert.AreEqual(specs.Count, specs.Select(s => s.AbilityKind).Distinct().Count(), "one effect per mount (§3.7)");
            Assert.AreEqual(specs.Count, specs.Select(s => s.Recipe).Distinct().Count(), "one model per species");
            Assert.AreEqual(specs.Count, specs.Select(s => (s.Speed, s.Defense, s.Health, s.Stamina)).Distinct().Count(), "distinct stat lines");
            foreach (MountSpec spec in specs)
            {
                Assert.IsNotNull(MountCodex.Entries.SingleOrDefault(e => e.MountId == spec.MountId), spec.MountId);
                Assert.AreEqual(MountCodex.Entries.Single(e => e.MountId == spec.MountId).Rarity, spec.Rarity, spec.MountId);
            }
        }

        [Test]
        public void Texts_CoverEveryWaveMount_Ability_AndToast()
        {
            var keys = new HashSet<string>(MountTexts.Entries.Select(e => e.Key));
            foreach (MountSpec spec in MountWaveCatalog.Specs)
            {
                MountDefinition d = MountCatalog.Get(spec.MountId);
                Assert.IsTrue(keys.Contains(d.DisplayNameKey), d.DisplayNameKey);
                Assert.IsTrue(keys.Contains(d.DescriptionKey), d.DescriptionKey);
                Assert.IsTrue(keys.Contains(d.Ability.NameKey), d.Ability.NameKey);
                Assert.IsTrue(keys.Contains(d.Ability.DescriptionKey), d.Ability.DescriptionKey);
                if (!string.IsNullOrEmpty(d.FavoriteLineKey))
                {
                    Assert.IsTrue(keys.Contains(d.FavoriteLineKey), d.FavoriteLineKey);
                }
            }

            foreach (LocalizationEntryCheck e in MountWaveTexts.Entries.Select(e => new LocalizationEntryCheck(e.Key, e.En, e.PtBR)))
            {
                Assert.IsFalse(string.IsNullOrEmpty(e.En) || string.IsNullOrEmpty(e.Pt), e.Key);
                Assert.IsFalse(e.Pt.Contains("Zefa") || e.Pt.Contains("Chico"), "grandparents are Vó Edna / Vô Jorge: " + e.Key);
            }

            Assert.AreEqual(MountTexts.Entries.Count, MountTexts.Entries.Select(e => e.Key).Distinct().Count(), "no duplicate keys");
        }

        private readonly struct LocalizationEntryCheck
        {
            public readonly string Key;
            public readonly string En;
            public readonly string Pt;

            public LocalizationEntryCheck(string key, string en, string pt)
            {
                Key = key;
                En = en;
                Pt = pt;
            }
        }

        // ---------------- Shops ----------------

        [Test]
        public void ShopWhistles_AreSoldAtTheirPrice_AndHaveAnAsset()
        {
            var sold = new Dictionary<string, CityShopEntry>();
            foreach (CityShopSpec shop in CityShopCatalog.Shops)
            {
                foreach (CityShopEntry entry in shop.Entries)
                {
                    sold[entry.ItemId] = entry;
                }
            }

            foreach (string whistle in MountWaveCatalog.ShopWhistles)
            {
                MountSpec spec = MountCatalog.FindByWhistle(whistle);
                Assert.IsNotNull(spec, whistle);
                Assert.Greater(spec.ShopPrice, 0, whistle);
                Assert.IsTrue(sold.ContainsKey(whistle), whistle + " is not in any city shop");
                Assert.AreEqual(spec.ShopPrice, sold[whistle].Price, whistle);
                ItemDefinition item = ItemDatabase.GetById(whistle);
                Assert.IsNotNull(item, whistle + " asset missing");
                Assert.AreEqual(spec.ShopPrice, item.Value, whistle);
            }

            // Non-shop species have no price (drops, ribbons, missions, events).
            foreach (MountSpec spec in MountWaveCatalog.Specs.Where(s => !MountWaveCatalog.ShopWhistles.Contains(s.WhistleItemId)))
            {
                Assert.AreEqual(0, spec.ShopPrice, spec.MountId);
            }
        }

        // ---------------- Drops and Fitas do Laço ----------------

        [Test]
        public void Drops_TableMatchesTheDoc_AndSkipsLaterWaves()
        {
            Assert.AreEqual(0.004f, MountDropRules.EntriesFor(MountDropRules.ForestWolf).Single(e => e.MountId == MountWaveCatalog.ManedWolf).DayChance, 1e-6f);
            Assert.AreEqual(0.008f, MountDropRules.EntriesFor(MountDropRules.ForestWolf).Single(e => e.MountId == MountWaveCatalog.ManedWolf).NightChance, 1e-6f);
            Assert.AreEqual(0f, MountDropRules.EntriesFor(MountDropRules.ForestWolf).Single(e => e.MountId == MountWaveCatalog.LightJaguar).DayChance);
            Assert.AreEqual(0.02f, MountDropRules.EntriesFor(MountDropRules.AlphaWolf).Single().DayChance, 1e-6f);

            // A random that always says "yes" drops every playable row, never a later-wave species.
            var always = new AlwaysZeroRandom();
            List<string> guardian = MountDropRules.Roll(MountDropRules.RuinGuardian, false, always);
            Assert.IsEmpty(guardian, "the Ruin Guardian's mounts are all later waves");
            Assert.AreEqual(0, always.Draws, "unplayable rows draw no number");
            CollectionAssert.AreEquivalent(new[] { MountWaveCatalog.SnowTiger, MountWaveCatalog.WoollyMammoth },
                MountDropRules.Roll(MountDropRules.MountainWarlord, false, always));
            CollectionAssert.AreEqual(new[] { MountWaveCatalog.ManedWolf }, MountDropRules.Roll(MountDropRules.ForestWolf, false, always));
            CollectionAssert.AreEqual(new[] { MountWaveCatalog.ManedWolf, MountWaveCatalog.LightJaguar }, MountDropRules.Roll(MountDropRules.ForestWolf, true, always));
            Assert.IsEmpty(MountDropRules.Roll("enemy_unknown", false, always));
        }

        [Test]
        public void Drops_SameSeed_SameResult_AndRateIsAboutRight()
        {
            int Count(int seed)
            {
                var random = new System.Random(seed);
                int hits = 0;
                for (int i = 0; i < 20000; i++)
                {
                    hits += MountDropRules.Roll(MountDropRules.AlphaWolf, false, random).Count;
                }

                return hits;
            }

            Assert.AreEqual(Count(7), Count(7));
            Assert.That(Count(7), Is.InRange(300, 500), "2% of 20000 = 400");
        }

        [Test]
        public void Collection_BossDeath_GivesARibbon_AndRibbonsBuyTheRare()
        {
            MountCollection collection = NewCollection();
            collection.DropRandom = new NeverRandom();
            for (int i = 0; i < MountDropRules.RareRibbonCost - 1; i++)
            {
                Assert.IsEmpty(collection.ResolveEnemyDeath(MountDropRules.AlphaWolf, true, false));
            }

            Assert.AreEqual(24, collection.RibbonsOf(MountDropRules.AlphaWolf));
            Assert.IsNull(collection.RedeemRibbons(MountDropRules.AlphaWolf, MountWaveCatalog.Unicorn), "24 < 25");
            collection.ResolveEnemyDeath(MountDropRules.AlphaWolf, true, false);
            OwnedMount unicorn = collection.RedeemRibbons(MountDropRules.AlphaWolf, MountWaveCatalog.Unicorn);
            Assert.IsNotNull(unicorn);
            Assert.AreEqual(MountCollection.SourceRibbon, unicorn.source);
            Assert.AreEqual(0, collection.RibbonsOf(MountDropRules.AlphaWolf));

            // A common critter gives no ribbon; a boss's ribbons can't buy another boss's mount, nor a Legendary.
            collection.ResolveEnemyDeath(MountDropRules.ForestWolf, false, false);
            Assert.AreEqual(0, collection.RibbonsOf(MountDropRules.ForestWolf));
            collection.AddRibbons(MountDropRules.CaveDweller, 200);
            Assert.IsNull(collection.RedeemRibbons(MountDropRules.CaveDweller, MountWaveCatalog.Unicorn));
            Assert.IsNull(collection.RedeemRibbons(MountDropRules.CaveDweller, "mount_sertao_phoenix"));
            Assert.IsNotNull(collection.RedeemRibbons(MountDropRules.CaveDweller, MountWaveCatalog.Kirin));
            Assert.AreEqual(120, collection.RibbonsOf(MountDropRules.CaveDweller));
            Assert.AreEqual(-1, MountDropRules.CostFor(ItemRarity.Legendary));
        }

        [Test]
        public void Collection_DropJoinsAtOnce_AndRibbonsSurviveTheSave()
        {
            MountCollection collection = NewCollection();
            collection.DropRandom = new AlwaysZeroRandom();
            List<OwnedMount> dropped = collection.ResolveEnemyDeath(MountDropRules.MountainWarlord, true, false);
            Assert.AreEqual(2, dropped.Count);
            Assert.IsTrue(dropped.All(m => m.source == MountCollection.SourceDrop));
            Assert.IsTrue(collection.Owns(MountWaveCatalog.SnowTiger));

            object state = collection.CaptureState();
            MountCollection loaded = NewCollection();
            loaded.RestoreState(state);
            Assert.AreEqual(1, loaded.RibbonsOf(MountDropRules.MountainWarlord));
            Assert.IsTrue(loaded.Owns(MountWaveCatalog.WoollyMammoth));
        }

        // ---------------- Missions, achievements, secret, event ----------------

        [Test]
        public void Hooks_GrantOnce_AndEventsCostPrendas()
        {
            MountCollection collection = NewCollection();
            Assert.AreEqual(1, collection.ReportHook(MountSourceKind.Quest, MountSourceHooks.QuestAguaProJumento).Count);
            Assert.IsEmpty(collection.ReportHook(MountSourceKind.Quest, MountSourceHooks.QuestAguaProJumento), "once");
            Assert.IsEmpty(collection.ReportHook(MountSourceKind.Achievement, MountSourceHooks.QuestAguaProJumento), "kind must match");
            Assert.AreEqual(MountCollection.SourceAchievement,
                collection.ReportHook(MountSourceKind.Achievement, MountSourceHooks.AchievementAmigoDaCaipora).Single().source);
            Assert.AreEqual(MountWaveCatalog.ElderTortoise,
                collection.ReportHook(MountSourceKind.Achievement, MountSourceHooks.AchievementPacienciaDeJabuti).Single().mountId);
            Assert.AreEqual(MountCollection.SourceSecret,
                collection.ReportHook(MountSourceKind.Secret, MountSourceHooks.SecretMataQueNaoQueima).Single().source);
            Assert.IsEmpty(collection.ReportHook(MountSourceKind.Event, MountSourceHooks.EventFestaJunina), "event mounts are bought");

            int prendas = 100;
            Assert.IsNull(collection.BuyWithPrendas(MountWaveCatalog.BoiBumba, ref prendas));
            Assert.AreEqual(100, prendas);
            Assert.IsNotNull(collection.BuyWithPrendas(MountWaveCatalog.FireRhea, ref prendas));
            Assert.AreEqual(10, prendas);
            Assert.IsNull(collection.BuyWithPrendas(MountWaveCatalog.Unicorn, ref prendas), "not an event mount");

            // Every wave mount has a source: shop, drop, ribbon or hook.
            foreach (MountSpec spec in MountWaveCatalog.Specs)
            {
                bool shop = MountWaveCatalog.ShopWhistles.Contains(spec.WhistleItemId);
                bool drop = MountDropRules.Table.Any(e => e.MountId == spec.MountId);
                bool hook = MountSourceHooks.For(spec.MountId) != null;
                Assert.IsTrue(shop || drop || hook, spec.MountId + " has no source");
            }
        }

        [Test]
        public void Hooks_ListenToTheGameEvents()
        {
            MountCollection collection = NewCollection();
            collection.Initialize();
            try
            {
                EventBus.Publish(new Rapadura.Gameplay.Quests.QuestCompletedEvent(MountSourceHooks.QuestAguaProJumento));
                EventBus.Publish(new Rapadura.Gameplay.Secrets.SecretClaimedEvent(MountSourceHooks.SecretMataQueNaoQueima, 0));
                Assert.IsTrue(collection.Owns(MountWaveCatalog.EnchantedDonkey));
                Assert.IsTrue(collection.Owns(MountWaveCatalog.Boitata));
            }
            finally
            {
                collection.Shutdown();
            }
        }

        // ---------------- Abilities (pure rules) ----------------

        [Test]
        public void AbilityRules_Geometry()
        {
            Assert.IsTrue(MountWaveAbilityRules.InChargePath(Vector3.zero, new Vector3(0, 0, 8), new Vector3(2.5f, 0, 4), 3f));
            Assert.IsFalse(MountWaveAbilityRules.InChargePath(Vector3.zero, new Vector3(0, 0, 8), new Vector3(0, 0, -4), 3f));

            Vector3 out1 = MountWaveAbilityRules.RepelTo(Vector3.zero, new Vector3(1, 0, 0), 5f);
            Assert.AreEqual(5f + MountWaveAbilityRules.RepelMargin, out1.x, 1e-4f);
            Assert.AreEqual(new Vector3(9, 0, 0), MountWaveAbilityRules.RepelTo(Vector3.zero, new Vector3(9, 0, 0), 5f));

            Vector3 land = MountWaveAbilityRules.PounceLanding(Vector3.zero, new Vector3(0, 0, 10));
            Assert.AreEqual(10f - MountWaveAbilityRules.PounceStopShort, land.z, 1e-4f);
            Assert.AreEqual(Vector3.zero, MountWaveAbilityRules.PounceLanding(Vector3.zero, new Vector3(0, 0, 1)));

            Assert.AreEqual(4f, MountWaveAbilityRules.HealPerSecond(200f, 0.02f), 1e-4f);
            Assert.AreEqual(1.6f, MountWaveAbilityRules.DetectionRadius(8f, true, 0.2f), 1e-4f);
            Assert.AreEqual(8f, MountWaveAbilityRules.DetectionRadius(8f, false, 0.2f));
            Assert.IsTrue(MountWaveAbilityRules.OnTrail(new[] { Vector3.zero, new Vector3(0, 0, 3) }, new Vector3(0.5f, 0, 3), 1.2f));
            Assert.IsFalse(MountWaveAbilityRules.OnTrail(new[] { Vector3.zero }, new Vector3(3, 0, 0), 1.2f));
            float v = MountWaveAbilityRules.JumpSpeedFor(2.5f, -9.81f);
            Assert.AreEqual(2.5f, v * v / (2f * 9.81f), 1e-3f);
        }

        [Test]
        public void Stealth_ScalesEnemyDetection_ThroughTheHook()
        {
            Assert.AreEqual(8f, Rapadura.Gameplay.Enemies.EnemyDetectionUtility.DetectionRadiusFor(8f, null));
            var hero = new GameObject("Hero");
            try
            {
                var effects = MountAbilityEffects.For(new MountAbilityContext { Hero = hero });
                effects.StartStealth(10f, 0.2f);
                Assert.AreEqual(1.6f, Rapadura.Gameplay.Enemies.EnemyDetectionUtility.DetectionRadiusFor(8f, hero.transform), 1e-4f);
            }
            finally
            {
                Object.DestroyImmediate(hero);
                Rapadura.Gameplay.Enemies.EnemyDetectionUtility.TargetDetectionScale = null;
            }
        }

        [Test]
        public void Session_FreeGallop_CostsNoStamina_AndShellStopsTheMount()
        {
            var session = new MountSession();
            session.Mount(100f);
            float before = session.Stamina;
            session.StartFreeGallop(8f);
            session.Tick(1f, true, MountPlace.WorldMap);
            Assert.IsTrue(session.IsGalloping);
            Assert.AreEqual(before, session.Stamina, 1e-4f);

            session.StartShell(3f);
            Assert.AreEqual(0f, session.SpeedMultiplier(1f, 1.3f, MountPlace.ArenaFree));
            session.Tick(3.1f, false, MountPlace.ArenaFree);
            Assert.Greater(session.SpeedMultiplier(1f, 1.3f, MountPlace.ArenaFree), 0f);
        }

        // ---------------- Models ----------------

        [Test]
        public void WaveModels_AreDistinctShapes_RheasAreBipeds()
        {
            Assert.IsTrue(MountRecipe.ShapeFor(MountVisualRecipe.Rhea).Biped);
            Assert.IsTrue(MountRecipe.ShapeFor(MountVisualRecipe.FireRhea).Biped);
            Assert.IsFalse(MountRecipe.ShapeFor(MountVisualRecipe.Mammoth).Biped);
            Assert.IsTrue(MountRecipe.ShapeFor(MountVisualRecipe.Mammoth).Trunk);
            Assert.IsTrue(MountRecipe.ShapeFor(MountVisualRecipe.ElderTortoise).Shell);
            Assert.Greater(MountRecipe.ShapeFor(MountVisualRecipe.Mammoth).SeatHeight, MountRecipe.ShapeFor(MountVisualRecipe.Pony).SeatHeight);
        }

        private sealed class AlwaysZeroRandom : System.Random
        {
            public int Draws;

            public override double NextDouble()
            {
                Draws++;
                return 0d;
            }
        }

        private sealed class NeverRandom : System.Random
        {
            public override double NextDouble() => 0.999999d;
        }
    }
}
