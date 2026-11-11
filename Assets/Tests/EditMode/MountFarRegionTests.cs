using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rapadura.Core.DI;
using Rapadura.Core.Events;
using Rapadura.Gameplay.Enemies.Visuals;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.Mounts;
using Rapadura.Gameplay.Mounts.Visuals;
using Rapadura.Gameplay.World.Cities;
using Rapadura.Gameplay.World.Map;
using Rapadura.Gameplay.World.Map.Mechanics;
using UnityEngine;

namespace Rapadura.Tests.EditMode
{
    /// <summary>The eight mounts named by the map sheets of Regions 10–16 (Docs/Mounts.md §3.9, <see cref="MountFarRegionCatalog"/>).</summary>
    public class MountFarRegionTests
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

        // ---------------- Catalog ----------------

        [Test]
        public void EverySpecies_IsOnItsRarityBudget_WithinTheLimits_AndInTheCaderno()
        {
            Assert.AreEqual(8, MountFarRegionCatalog.Specs.Count);
            foreach (MountSpec spec in MountFarRegionCatalog.Specs)
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
                Assert.Less(spec.Rarity, ItemRarity.Legendary, "Legendaries wait for their own source");
                Assert.AreEqual(spec.Rarity, MountCodex.Entries.Single(e => e.MountId == spec.MountId).Rarity, spec.MountId);
            }
        }

        [Test]
        public void IdsAbilitiesKindsAndModels_AreDistinct_AcrossTheWholeCatalog()
        {
            IReadOnlyList<MountSpec> specs = MountCatalog.Specs;
            Assert.AreEqual(specs.Count, specs.Select(s => s.MountId).Distinct().Count());
            Assert.AreEqual(specs.Count, specs.Select(s => s.AbilityId).Distinct().Count());
            Assert.AreEqual(specs.Count, specs.Select(s => s.AbilityKind).Distinct().Count(), "one effect per mount");
            Assert.AreEqual(specs.Count, specs.Select(s => s.Recipe).Distinct().Count(), "one model per species");
            Assert.AreEqual(specs.Count, specs.Select(s => (s.Speed, s.Defense, s.Health, s.Stamina)).Distinct().Count());
        }

        [Test]
        public void Texts_CoverNameDescLoreSourceAbilityAndToasts_InBothLanguages()
        {
            Dictionary<string, Rapadura.Core.Localization.LocalizationEntry> byKey = MountTexts.Entries.ToDictionary(e => e.Key);
            foreach (MountSpec spec in MountFarRegionCatalog.Specs)
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
                         MountFarRegionTexts.ToastGlide, MountFarRegionTexts.ToastPumice, MountFarRegionTexts.ToastUntieHit, MountFarRegionTexts.ToastUntieMiss,
                         MountFarRegionTexts.ToastAuroraRun, MountFarRegionTexts.ToastThicket, MountFarRegionTexts.ToastRevealSeen, MountFarRegionTexts.ToastRevealNone,
                         MountFarRegionTexts.ToastDread, MountFarRegionTexts.ToastBoulder,
                     })
            {
                Assert.IsTrue(byKey.ContainsKey(toast), toast);
            }

            Assert.AreEqual("Íbis Real de Mesektet", byKey[MountCatalog.Get(MountFarRegionCatalog.MesektetIbis).DisplayNameKey].PtBR);
        }

        // ---------------- Sources ----------------

        [Test]
        public void ShopWhistles_AreUncommon_SoldInTheRegionCity_WithinThePriceBand()
        {
            var expected = new Dictionary<string, string>
            {
                { MountWaveCatalog.WhistleOf(MountFarRegionCatalog.VolcanoGoat), CityShopCatalog.FerroPretoKurogane },
                { MountWaveCatalog.WhistleOf(MountFarRegionCatalog.TaigaElk), CityShopCatalog.BetumeAtrahasis },
                { MountWaveCatalog.WhistleOf(MountFarRegionCatalog.HillRam), CityShopCatalog.BetumeAtrahasis },
            };
            CollectionAssert.AreEquivalent(expected.Keys, MountFarRegionCatalog.ShopWhistles);

            foreach (KeyValuePair<string, string> sale in expected)
            {
                MountSpec spec = MountCatalog.FindByWhistle(sale.Key);
                Assert.IsNotNull(spec, sale.Key);
                Assert.AreEqual(ItemRarity.Uncommon, spec.Rarity, "Docs/Mounts.md §5.1: Rare and up are never sold");

                ItemDefinition item = ItemDatabase.GetById(sale.Key);
                Assert.IsNotNull(item, sale.Key + " asset missing");
                Assert.AreEqual(spec.ShopPrice, item.Value, sale.Key);

                CityShopEntry entry = CityShopCatalog.Shops.Single(s => s.ShopId == sale.Value).Entries.SingleOrDefault(e => e.ItemId == sale.Key);
                Assert.IsNotNull(entry, sale.Key + " not on the counter");
                Assert.AreEqual(spec.ShopPrice, entry.Price, sale.Key);
                Assert.That(entry.Price, Is.InRange(item.Value * 0.8f, item.Value * 1.25f), sale.Key);
            }
        }

        [Test]
        public void RareAndEpic_AreNotSold_AndComeFromAnExistingCityQuestOfTheirRegion()
        {
            var expected = new Dictionary<string, string>
            {
                { MountFarRegionCatalog.GordiasGoat, CityQuestCatalog.ValeVineFoxes },
                { MountFarRegionCatalog.AuroraReindeer, CityQuestCatalog.PortoBears },
                { MountFarRegionCatalog.LacquerLion, CityQuestCatalog.HinokuraEmberGulls },
                { MountFarRegionCatalog.MesektetIbis, CityQuestCatalog.MesektetScorpions },
                { MountFarRegionCatalog.Mushhushshu, CityQuestCatalog.DurJarrasBeetles },
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
            }
        }

        [Test]
        public void CompletingTheQuest_GrantsTheMount_Once()
        {
            MountCollection collection = NewCollection();
            collection.Initialize();
            try
            {
                Assert.AreEqual(1, collection.ReportHook(MountSourceKind.Quest, CityQuestCatalog.MesektetScorpions).Count);
                Assert.IsTrue(collection.Owns(MountFarRegionCatalog.MesektetIbis));
                Assert.IsEmpty(collection.ReportHook(MountSourceKind.Quest, CityQuestCatalog.MesektetScorpions), "given once");
                Assert.AreEqual(1, collection.Mounts.Count(m => m.mountId == MountFarRegionCatalog.MesektetIbis));
            }
            finally
            {
                collection.Shutdown();
            }
        }

        // ---------------- Map gates ----------------

        [Test]
        public void OnlyTheIbis_OpensWingGates()
        {
            foreach (MountSpec spec in MountFarRegionCatalog.Specs)
            {
                MapRequirementState state = MapRequirementHeroState.Capture(null, null, null, new[] { new OwnedMount { guid = "a", mountId = spec.MountId } });
                bool flyer = spec.MountId == MountFarRegionCatalog.MesektetIbis;

                Assert.AreEqual(flyer, MountFarRegionRules.AnswersWingGates(spec), spec.MountId);
                Assert.AreEqual(flyer, spec.Type == MountType.Voador, spec.MountId);
                Assert.AreEqual(flyer, MapRequirementRules.IsMet(MapRequirementTags.WingsRequired, state), spec.MountId);
                Assert.AreEqual(flyer, MapRequirementRules.IsMet(MapRequirementTags.WingsOrDoubleJump, state), spec.MountId);
            }
        }

        // ---------------- Rules ----------------

        [Test]
        public void NearestInRange_PicksTheClosest_OnTheFlat_AndNothingOutOfReach()
        {
            var points = new[] { new Vector3(10f, 0f, 0f), new Vector3(0f, 50f, 3f), new Vector3(0f, 0f, 20f) };
            Assert.AreEqual(1, MountFarRegionRules.NearestInRange(Vector3.zero, points, 14f), "height does not count");
            Assert.AreEqual(-1, MountFarRegionRules.NearestInRange(Vector3.zero, points, 2f));
            Assert.AreEqual(-1, MountFarRegionRules.NearestInRange(Vector3.zero, new Vector3[0], 14f));
            Assert.AreEqual(-1, MountFarRegionRules.NearestInRange(Vector3.zero, null, 14f));
        }

        [Test]
        public void Dread_RunsOffCommons_ButNeverBosses()
        {
            MountSpec spec = MountCatalog.FindSpec(MountFarRegionCatalog.Mushhushshu);
            Assert.AreEqual(spec.Duration, MountFarRegionRules.DreadSeconds(spec.Duration, false, spec.BossMultiplier), 1e-4f);
            Assert.AreEqual(0f, MountFarRegionRules.DreadSeconds(spec.Duration, true, spec.BossMultiplier), "§ ficha 77: não em chefe");
            Assert.Greater(spec.Magnitude, 0f);
        }

        [Test]
        public void Reindeer_CrossesThinIce_ButTheMammothStillBreaksIt()
        {
            Assert.IsTrue(MountFarRegionRules.CrossesThinIce(MountFarRegionCatalog.AuroraReindeer));
            Assert.IsFalse(ThinIceRules.IsHeavyMount(MountFarRegionCatalog.AuroraReindeer));
            Assert.IsFalse(MountFarRegionRules.CrossesThinIce(MountWaveCatalog.WoollyMammoth));
        }

        [Test]
        public void Glide_FliesTheWholeDistance_OnTheGroundPlane()
        {
            MountDefinition ibis = MountCatalog.Get(MountFarRegionCatalog.MesektetIbis);
            Assert.AreEqual(MountAbilityKind.Glide, ibis.Ability.Kind);
            Assert.AreEqual(MountFlightRules.CruiseHover + MountFlightRules.SoarPeak, ibis.Ability.ServerMaxAltitude, 1e-4f);
            Vector3 landing = MountFlightRules.FlightLanding(new Vector3(1f, 2f, 3f), Vector3.forward, ibis.Ability.Distance);
            Assert.AreEqual(new Vector3(1f, 2f, 3f + ibis.Ability.Distance), landing);
        }

        // ---------------- Models ----------------

        [Test]
        public void Models_StayInBudget_AndKeepTheRiderSeat()
        {
            foreach (MountSpec spec in MountFarRegionCatalog.Specs)
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
        public void Ibis_FlapsAndHasABeak_TheWalkersDoNot()
        {
            QuadrupedShape ibis = MountRecipe.ShapeFor(MountVisualRecipe.MesektetIbis);
            Assert.IsTrue(ibis.FlightWings);
            Assert.IsTrue(ibis.Beak);
            Assert.IsTrue(ibis.Biped);
            foreach (MountSpec spec in MountFarRegionCatalog.Specs.Where(s => s.Type != MountType.Voador))
            {
                Assert.IsFalse(MountRecipe.ShapeFor(spec.Recipe).FlightWings, spec.MountId);
            }

            MountActor actor = MountActor.Spawn(MountCatalog.Get(MountFarRegionCatalog.MesektetIbis), Vector3.zero, Quaternion.identity);
            try
            {
                Assert.IsTrue(actor.IsFlyer);
                actor.Soar(1.4f);
                Assert.IsTrue(actor.IsSoaring);
            }
            finally
            {
                Object.DestroyImmediate(actor.gameObject);
            }
        }
    }
}
