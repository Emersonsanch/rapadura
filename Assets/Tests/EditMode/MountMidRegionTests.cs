using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rapadura.Core.DI;
using Rapadura.Core.Events;
using Rapadura.Gameplay.Enemies.Visuals;
using Rapadura.Gameplay.Inventory;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.Mounts;
using Rapadura.Gameplay.Mounts.Visuals;
using Rapadura.Gameplay.World.Cities;
using Rapadura.Gameplay.World.Map;
using Rapadura.Gameplay.World.Map.Mechanics;
using UnityEngine;

namespace Rapadura.Tests.EditMode
{
    /// <summary>The five mounts of the Regions 5–9, one per region (Docs/Mounts.md §3.10, <see cref="MountMidRegionCatalog"/>). Pure rules only.</summary>
    public class MountMidRegionTests
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

        private static MountCollection NewCollection()
        {
            int n = 0;
            return new MountCollection { GuidFactory = () => "g" + (++n), UtcNow = () => 0L, DropRandom = new System.Random(1), IsNight = () => false };
        }

        private static readonly string[] Aquatic = { MountMidRegionCatalog.DhowTurtle, MountMidRegionCatalog.TarchonFrog };

        // ---------------- Catalog ----------------

        [Test]
        public void EverySpecies_IsOnItsRarityBudget_WithinTheLimits_AndInTheCaderno()
        {
            Assert.AreEqual(5, MountMidRegionCatalog.Specs.Count, "one per region, 5 to 9");
            foreach (MountSpec spec in MountMidRegionCatalog.Specs)
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
                Assert.AreNotEqual(MountType.Voador, spec.Type, "no flyer in the middle regions");
                Assert.Less(spec.Rarity, ItemRarity.Legendary, "Legendaries wait for their own source");
                Assert.AreEqual(spec.Rarity, MountCodex.Entries.Single(e => e.MountId == spec.MountId).Rarity, spec.MountId);
            }

            Assert.AreEqual(MountCatalog.Specs.Count, MountCodex.Entries.Count(e => e.IsAvailable));
            CollectionAssert.IsSubsetOf(MountMidRegionCatalog.Specs.Select(s => s.MountId), MountCatalog.MountIds);
        }

        [Test]
        public void TheFive_UseTheNewKinds_OnePerMount()
        {
            var expected = new Dictionary<string, MountAbilityKind>
            {
                { MountMidRegionCatalog.DhowTurtle, MountAbilityKind.MonsoonSail },
                { MountMidRegionCatalog.TarchonFrog, MountAbilityKind.BogLeap },
                { MountMidRegionCatalog.RoofYak, MountAbilityKind.WarmFleece },
                { MountMidRegionCatalog.WhiteLightStag, MountAbilityKind.LightTrail },
                { MountMidRegionCatalog.GoldenSteppeHorse, MountAbilityKind.SecondWind },
            };

            foreach (KeyValuePair<string, MountAbilityKind> pair in expected)
            {
                Assert.AreEqual(pair.Value, MountCatalog.Get(pair.Key).Ability.Kind, pair.Key);
                Assert.AreEqual(1, MountCatalog.Specs.Count(s => s.AbilityKind == pair.Value), pair.Value + " is used once in the whole catalog");
            }

            Assert.AreEqual(1, MountCatalog.Specs.Count(s => s.MountId == "mount_capybara"), "the Hipopotinha de Taweret stays the only hippo");
            Assert.AreEqual(1, MountCatalog.Specs.Count(s => s.Recipe == MountVisualRecipe.Capybara));
        }

        [Test]
        public void Texts_CoverNameDescLoreSourceAbilityAndToasts_InBothLanguages()
        {
            Dictionary<string, Rapadura.Core.Localization.LocalizationEntry> byKey = MountTexts.Entries.ToDictionary(e => e.Key);
            foreach (MountSpec spec in MountMidRegionCatalog.Specs)
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
                         MountMidRegionTexts.ToastSail, MountMidRegionTexts.ToastLeap, MountMidRegionTexts.ToastLeapSplash,
                         MountMidRegionTexts.ToastFleece, MountMidRegionTexts.ToastLightTrail, MountMidRegionTexts.ToastBreath,
                     })
            {
                Assert.IsTrue(byKey.ContainsKey(toast), toast);
            }

            Assert.AreEqual("Dhow-Tartaruga", byKey[MountCatalog.Get(MountMidRegionCatalog.DhowTurtle).DisplayNameKey].PtBR);
            Assert.AreEqual("Cervo de Luz Branca", byKey[MountCatalog.Get(MountMidRegionCatalog.WhiteLightStag).DisplayNameKey].PtBR);
        }

        [Test]
        public void EveryLore_NamesACousin()
        {
            string[] cousins = { "Maithe", "Ícaro", "Joaquim", "Maria", "Lavine" };
            Dictionary<string, Rapadura.Core.Localization.LocalizationEntry> byKey = MountTexts.Entries.ToDictionary(e => e.Key);
            foreach (MountSpec spec in MountMidRegionCatalog.Specs)
            {
                string lore = byKey[MountCatalog.Get(spec.MountId).LoreKey].PtBR;
                Assert.IsTrue(cousins.Any(c => lore.Contains(c)), spec.MountId + " lore talks about a cousin");
            }
        }

        // ---------------- Sources ----------------

        [Test]
        public void ShopWhistles_AreUncommon_SoldInTheRegionCity_WithinThePriceBand()
        {
            var expected = new Dictionary<string, string>
            {
                { MountWaveCatalog.WhistleOf(MountMidRegionCatalog.TarchonFrog), CityShopCatalog.CestariaLarthia },
                { MountWaveCatalog.WhistleOf(MountMidRegionCatalog.RoofYak), CityShopCatalog.AgasalhosSisinnios },
            };
            CollectionAssert.AreEquivalent(expected.Keys, MountMidRegionCatalog.ShopWhistles);

            foreach (KeyValuePair<string, string> sale in expected)
            {
                MountSpec spec = MountCatalog.FindByWhistle(sale.Key);
                Assert.IsNotNull(spec, sale.Key);
                Assert.AreEqual(ItemRarity.Uncommon, spec.Rarity, "Docs/Mounts.md §5.1: Rare and up are never sold");
                Assert.That(spec.ShopPrice, Is.InRange(140, 150), "§5.1 Uncommon band");

                ItemDefinition item = ItemDatabase.GetById(sale.Key);
                Assert.IsNotNull(item, sale.Key + " asset missing");
                Assert.AreEqual(spec.ShopPrice, item.Value, sale.Key);

                CityShopEntry entry = CityShopCatalog.Shops.Single(s => s.ShopId == sale.Value).Entries.SingleOrDefault(e => e.ItemId == sale.Key);
                Assert.IsNotNull(entry, sale.Key + " not on the counter");
                Assert.AreEqual(spec.ShopPrice, entry.Price, sale.Key);
                Assert.IsFalse(entry.Rotating, sale.Key + " is always on the counter");
                Assert.AreEqual(1, CityShopCatalog.Shops.Count(s => s.Entries.Any(e => e.ItemId == sale.Key)), sale.Key + " sold in one city only");
            }
        }

        [Test]
        public void RareAndEpic_AreNotSold_AndComeFromTheSecondCityQuestOfTheirRegion()
        {
            var expected = new Dictionary<string, string>
            {
                { MountMidRegionCatalog.DhowTurtle, CityQuestCatalog.TaaPwaniClaws },
                { MountMidRegionCatalog.WhiteLightStag, CityQuestCatalog.TorresIbises },
                { MountMidRegionCatalog.GoldenSteppeHorse, CityQuestCatalog.SoslanEagles },
            };

            foreach (KeyValuePair<string, string> reward in expected)
            {
                MountSpec spec = MountCatalog.FindSpec(reward.Key);
                Assert.GreaterOrEqual(spec.Rarity, ItemRarity.Rare, reward.Key);
                Assert.AreEqual(0, spec.ShopPrice, reward.Key);
                Assert.IsFalse(CityShopCatalog.Shops.Any(s => s.Entries.Any(e => e.ItemId == spec.WhistleItemId)), reward.Key + " must not be sold");

                MountSourceHook hook = MountSourceHooks.For(reward.Key);
                Assert.IsNotNull(hook, reward.Key);
                Assert.AreEqual(MountSourceKind.Quest, hook.Kind, reward.Key);
                Assert.AreEqual(reward.Value, hook.HookId, reward.Key);
                Assert.IsTrue(CityQuestCatalog.IsCityQuest(reward.Value), reward.Value + " is not a city quest");
                Assert.AreEqual(1, MountSourceHooks.Matching(MountSourceKind.Quest, reward.Value).Count(), reward.Value + " gives one mount");
            }
        }

        [Test]
        public void CompletingTheQuest_GrantsTheMount_Once()
        {
            MountCollection collection = NewCollection();
            collection.Initialize();
            try
            {
                Assert.AreEqual(1, collection.ReportHook(MountSourceKind.Quest, CityQuestCatalog.TaaPwaniClaws).Count);
                Assert.IsTrue(collection.Owns(MountMidRegionCatalog.DhowTurtle));
                Assert.IsEmpty(collection.ReportHook(MountSourceKind.Quest, CityQuestCatalog.TaaPwaniClaws), "given once");
                Assert.AreEqual(1, collection.Mounts.Count(m => m.mountId == MountMidRegionCatalog.DhowTurtle));
            }
            finally
            {
                collection.Shutdown();
            }
        }

        // ---------------- Carry ----------------

        [Test]
        public void EveryOne_HasACarryCapacity_AndTheYakIsTheBigCarrier()
        {
            foreach (MountSpec spec in MountMidRegionCatalog.Specs)
            {
                Assert.IsTrue(EncumbranceRules.MountBaseCapacity.ContainsKey(spec.MountId), spec.MountId);
            }

            float yak = EncumbranceRules.MountBaseCapacity[MountMidRegionCatalog.RoofYak];
            Assert.Less(yak, EncumbranceRules.MountBaseCapacity["mount_pack_donkey"], "the Asno de Caravana still carries most");
            Assert.AreEqual(yak, MountMidRegionCatalog.Specs.Max(s => EncumbranceRules.MountBaseCapacity[s.MountId]));
            Assert.Greater(EncumbranceRules.MountBaseCapacity[MountMidRegionCatalog.DhowTurtle], EncumbranceRules.MountBaseCapacity[MountMidRegionCatalog.TarchonFrog]);
        }

        // ---------------- Water and mud ----------------

        [Test]
        public void TheTurtleAndTheFrog_AreAquatic_AndAnswerTheBoatGates_TheTideAndTheBog()
        {
            foreach (MountSpec spec in MountMidRegionCatalog.Specs)
            {
                bool water = Aquatic.Contains(spec.MountId);
                Assert.AreEqual(water, spec.Type == MountType.Aquatico, spec.MountId);

                MapRequirementState state = MapRequirementHeroState.Capture(null, null, null, new[] { new OwnedMount { guid = "a", mountId = spec.MountId } });
                Assert.AreEqual(water, MapRequirementRules.IsMet(MapRequirementTags.BoatOrDock, state), spec.MountId);
                Assert.AreEqual(water, MapRequirementRules.IsMet(MapRequirementTags.SaddleRequired, state), spec.MountId);
                Assert.AreEqual(!water, CostaTideRules.Drenches(CostaTidePhase.High, spec.Type == MountType.Aquatico), spec.MountId);
                Assert.AreEqual(!water, DeltaIslandRules.BogSlows(true, false, spec.Type == MountType.Aquatico), spec.MountId);
            }
        }

        // ---------------- Rules ----------------

        [Test]
        public void SailGust_MovesCommons_NeverBosses()
        {
            MountSpec spec = MountCatalog.FindSpec(MountMidRegionCatalog.DhowTurtle);
            Assert.IsTrue(MountMidRegionRules.GustMoves(false, spec.BossMultiplier));
            Assert.IsFalse(MountMidRegionRules.GustMoves(true, spec.BossMultiplier));
            Assert.Greater(spec.Duration, 0f, "the free gallop of the sail");
            Assert.Greater(spec.Radius, 0f);
        }

        [Test]
        public void BogLeap_LandsAhead_AndTheSplashIsHalfOnBosses()
        {
            MountSpec spec = MountCatalog.FindSpec(MountMidRegionCatalog.TarchonFrog);
            Vector3 landing = MountMidRegionRules.LeapLanding(new Vector3(1f, 2f, 3f), new Vector3(0f, 5f, 2f), spec.Distance);
            Assert.AreEqual(new Vector3(1f, 2f, 3f + spec.Distance), landing, "flat forward, height kept");
            Assert.AreEqual(MountMidRegionRules.SplashSlowSeconds, MountMidRegionRules.SplashSeconds(false, spec.BossMultiplier), 1e-4f);
            Assert.AreEqual(MountMidRegionRules.SplashSlowSeconds * 0.5f, MountMidRegionRules.SplashSeconds(true, spec.BossMultiplier), 1e-4f);
            Assert.That(spec.Magnitude, Is.InRange(0.1f, 0.5f), "a slow, never a root");
        }

        [Test]
        public void SecondWind_RefillsTheMount_OnlyWhileMounted_AndGivesAlliesAShare()
        {
            var session = new MountSession();
            session.RefillStamina();
            Assert.Less(session.Stamina, 0f, "not mounted: nothing happens");

            session.Mount(150f);
            for (int i = 0; i < 40; i++)
            {
                session.Tick(0.25f, true, MountPlace.ArenaFree);
            }

            Assert.Less(session.Stamina, session.MaxStamina, "the gallop spent stamina");
            session.RefillStamina();
            Assert.AreEqual(session.MaxStamina, session.Stamina, 1e-4f);
            Assert.IsFalse(session.IsExhausted);

            MountSpec spec = MountCatalog.FindSpec(MountMidRegionCatalog.GoldenSteppeHorse);
            Assert.AreEqual(30f, MountMidRegionRules.AllyStamina(100f, spec.Magnitude), 1e-4f);
            Assert.AreEqual(0f, MountMidRegionRules.AllyStamina(-5f, spec.Magnitude), 1e-4f);
            Assert.AreEqual(100f, MountMidRegionRules.AllyStamina(100f, 3f), 1e-4f, "never more than the whole bar");
        }

        [Test]
        public void YakAndStag_BuffTheGroup_WithSmallNumbers()
        {
            MountSpec yak = MountCatalog.FindSpec(MountMidRegionCatalog.RoofYak);
            Assert.That(yak.Magnitude, Is.InRange(0.05f, 0.2f), "defense bonus");
            Assert.Greater(yak.Radius, 0f);

            MountSpec stag = MountCatalog.FindSpec(MountMidRegionCatalog.WhiteLightStag);
            Assert.That(stag.Magnitude, Is.InRange(0.01f, 0.03f), "heal per second on the trail");
            Assert.AreEqual(2f, MountWaveAbilityRules.HealPerSecond(100f, stag.Magnitude), 1e-4f);
            Assert.Greater(MountMidRegionRules.StagLampRadius, stag.Radius);
        }

        // ---------------- Models ----------------

        [Test]
        public void Models_StayInBudget_AndKeepTheRiderSeat()
        {
            foreach (MountSpec spec in MountMidRegionCatalog.Specs)
            {
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
        }

        [Test]
        public void OnlyTheTurtle_CarriesAShellAndASail()
        {
            QuadrupedShape turtle = MountRecipe.ShapeFor(MountVisualRecipe.DhowTurtle);
            Assert.IsTrue(turtle.Shell);
            Assert.IsTrue(turtle.Sail);
            Assert.Greater(turtle.SeatLift, 0f, "the saddle sits on the shell");
            foreach (MountSpec spec in MountCatalog.Specs.Where(s => s.MountId != MountMidRegionCatalog.DhowTurtle))
            {
                Assert.IsFalse(MountRecipe.ShapeFor(spec.Recipe).Sail, spec.MountId);
            }

            foreach (MountSpec spec in MountMidRegionCatalog.Specs)
            {
                Assert.IsFalse(MountRecipe.ShapeFor(spec.Recipe).FlightWings, spec.MountId);
            }

            Assert.IsTrue(MountRecipe.ShapeFor(MountVisualRecipe.RoofYak).Shaggy, "the yak's wool");
        }
    }
}
