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
using Rapadura.Gameplay.World.Biomes;
using Rapadura.Gameplay.World.Cities;
using Rapadura.Gameplay.World.Map;
using UnityEngine;

namespace Rapadura.Tests.EditMode
{
    /// <summary>
    /// The six map-sheet mounts (Docs/Mounts.md §3.12, <see cref="MountSheetCatalog"/>): one for each of the Regions 1–4 and two more
    /// for the Regions 5 and 7. Pure rules only.
    /// </summary>
    public class MountSheetTests
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
            Assert.AreEqual(6, MountSheetCatalog.Specs.Count);
            foreach (MountSpec spec in MountSheetCatalog.Specs)
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
                Assert.AreEqual(MountType.Terrestre, spec.Type, "no flyer and no swimmer: the gates of the maps stay with the others");
                Assert.That(spec.Rarity, Is.InRange(ItemRarity.Uncommon, ItemRarity.Rare), spec.MountId);
                Assert.AreEqual(spec.Rarity, MountCodex.Entries.Single(e => e.MountId == spec.MountId).Rarity, spec.MountId);
            }

            Assert.AreEqual(MountCatalog.Specs.Count, MountCodex.Entries.Count(e => e.IsAvailable));
            CollectionAssert.IsSubsetOf(MountSheetCatalog.Specs.Select(s => s.MountId), MountCatalog.MountIds);
        }

        [Test]
        public void OnePerRegion1To4_PlusTheCostaAndTheTeto()
        {
            var expected = new Dictionary<string, BiomeType>
            {
                { MountSheetCatalog.MarshHeron, BiomeType.Forest },
                { MountSheetCatalog.OchreWarWolf, BiomeType.Desert },
                { MountSheetCatalog.TallGrassEland, BiomeType.Mountain },
                { MountSheetCatalog.LanternOlm, BiomeType.Caves },
                { MountSheetCatalog.ArattaAurochs, BiomeType.Desert },
                { MountSheetCatalog.WhiteBactrian, BiomeType.Mountain },
            };

            CollectionAssert.AreEquivalent(expected.Keys, MountSheetCatalog.Specs.Select(s => s.MountId));
            foreach (KeyValuePair<string, BiomeType> pair in expected)
            {
                Assert.AreEqual(pair.Value, MountCatalog.FindSpec(pair.Key).Region, pair.Key);
            }

            Assert.IsTrue(MountCatalog.FindSpec(MountSheetCatalog.OchreWarWolf).IsWarMount, "the orcs' war wolf (ficha 11)");
            Assert.IsTrue(MountCatalog.FindSpec(MountSheetCatalog.ArattaAurochs).IsWarMount, "the aurochs of the course (ficha 26)");
        }

        [Test]
        public void TheSix_UseNewKinds_OnePerMount()
        {
            var expected = new Dictionary<string, MountAbilityKind>
            {
                { MountSheetCatalog.MarshHeron, MountAbilityKind.ShallowsWade },
                { MountSheetCatalog.OchreWarWolf, MountAbilityKind.DrumHowl },
                { MountSheetCatalog.TallGrassEland, MountAbilityKind.LongBound },
                { MountSheetCatalog.LanternOlm, MountAbilityKind.GlowSpit },
                { MountSheetCatalog.ArattaAurochs, MountAbilityKind.AurochsStomp },
                { MountSheetCatalog.WhiteBactrian, MountAbilityKind.StormHide },
            };

            foreach (KeyValuePair<string, MountAbilityKind> pair in expected)
            {
                Assert.AreEqual(pair.Value, MountCatalog.Get(pair.Key).Ability.Kind, pair.Key);
                Assert.AreEqual(1, MountCatalog.Specs.Count(s => s.AbilityKind == pair.Value), pair.Value + " is used once in the whole catalog");
            }

            IReadOnlyList<MountSpec> all = MountCatalog.Specs;
            Assert.AreEqual(all.Count, all.Select(s => s.Recipe).Distinct().Count(), "one model per species");
            Assert.AreEqual(all.Count, all.Select(s => (s.Speed, s.Defense, s.Health, s.Stamina)).Distinct().Count(), "distinct stat lines");
        }

        [Test]
        public void Texts_CoverNameDescLoreSourceAbilityAndToasts_InBothLanguages()
        {
            Dictionary<string, Rapadura.Core.Localization.LocalizationEntry> byKey = MountTexts.Entries.ToDictionary(e => e.Key);
            foreach (MountSpec spec in MountSheetCatalog.Specs)
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
                         MountSheetTexts.ToastWade, MountSheetTexts.ToastHowl, MountSheetTexts.ToastHowlNone, MountSheetTexts.ToastBound,
                         MountSheetTexts.ToastSpitHit, MountSheetTexts.ToastSpitMiss, MountSheetTexts.ToastStomp, MountSheetTexts.ToastStorm,
                     })
            {
                Assert.IsTrue(byKey.ContainsKey(toast), toast);
            }

            Assert.AreEqual("Garça do Brejo", byKey[MountCatalog.Get(MountSheetCatalog.MarshHeron).DisplayNameKey].PtBR);
            Assert.AreEqual("Camelo-Bactriano Branco", byKey[MountCatalog.Get(MountSheetCatalog.WhiteBactrian).DisplayNameKey].PtBR);
            Assert.AreEqual(MountTexts.Entries.Count, MountTexts.Entries.Select(e => e.Key).Distinct().Count(), "no duplicate keys");
        }

        [Test]
        public void EveryLore_NamesACousin_AndNoFolksyTitles()
        {
            string[] cousins = { "Maithe", "Ícaro", "Joaquim", "Maria", "Lavine" };
            Dictionary<string, Rapadura.Core.Localization.LocalizationEntry> byKey = MountTexts.Entries.ToDictionary(e => e.Key);
            foreach (MountSpec spec in MountSheetCatalog.Specs)
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
        public void TheHeron_IsUncommon_SoldAtDacilsCounter_WithinThePriceBand()
        {
            string whistle = MountWaveCatalog.WhistleOf(MountSheetCatalog.MarshHeron);
            CollectionAssert.AreEquivalent(new[] { whistle }, MountSheetCatalog.ShopWhistles);

            MountSpec spec = MountCatalog.FindByWhistle(whistle);
            Assert.IsNotNull(spec, whistle);
            Assert.AreEqual(ItemRarity.Uncommon, spec.Rarity, "Docs/Mounts.md §5.1: Rare and up are never sold");
            Assert.That(spec.ShopPrice, Is.InRange(140, 150), "§5.1 Uncommon band");

            ItemDefinition item = ItemDatabase.GetById(whistle);
            Assert.IsNotNull(item, whistle + " asset missing");
            Assert.AreEqual(spec.ShopPrice, item.Value, whistle);

            CityShopEntry entry = CityShopCatalog.Shops.Single(s => s.ShopId == CityShopCatalog.ErvariaDacil).Entries.SingleOrDefault(e => e.ItemId == whistle);
            Assert.IsNotNull(entry, whistle + " not on the counter");
            Assert.AreEqual(spec.ShopPrice, entry.Price, whistle);
            Assert.IsFalse(entry.Rotating, whistle + " is always on the counter");
            Assert.AreEqual(1, CityShopCatalog.Shops.Count(s => s.Entries.Any(e => e.ItemId == whistle)), whistle + " sold in one city only");
        }

        [Test]
        public void TheRareOnes_AreNotSold_AndComeFromTheSecondCityQuestOfTheirRegion()
        {
            var expected = new Dictionary<string, string>
            {
                { MountSheetCatalog.OchreWarWolf, CityQuestCatalog.TamborRubroHawks },
                { MountSheetCatalog.TallGrassEland, CityQuestCatalog.SaloesBats },
                { MountSheetCatalog.LanternOlm, CityQuestCatalog.MercadoRails },
                { MountSheetCatalog.ArattaAurochs, CityQuestCatalog.ArattaRace },
                { MountSheetCatalog.WhiteBactrian, CityQuestCatalog.SangkentBoard },
            };

            foreach (KeyValuePair<string, string> reward in expected)
            {
                MountSpec spec = MountCatalog.FindSpec(reward.Key);
                Assert.AreEqual(ItemRarity.Rare, spec.Rarity, reward.Key);
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
                Assert.AreEqual(1, collection.ReportHook(MountSourceKind.Quest, CityQuestCatalog.MercadoRails).Count);
                Assert.IsTrue(collection.Owns(MountSheetCatalog.LanternOlm));
                Assert.IsEmpty(collection.ReportHook(MountSourceKind.Quest, CityQuestCatalog.MercadoRails), "given once");
                Assert.AreEqual(1, collection.Mounts.Count(m => m.mountId == MountSheetCatalog.LanternOlm));
            }
            finally
            {
                collection.Shutdown();
            }
        }

        // ---------------- Carry and gates ----------------

        [Test]
        public void EveryOne_HasACarryCapacity_AndTheCamelIsTheirBigCarrier()
        {
            foreach (MountSpec spec in MountSheetCatalog.Specs)
            {
                Assert.IsTrue(EncumbranceRules.MountBaseCapacity.ContainsKey(spec.MountId), spec.MountId);
            }

            float camel = EncumbranceRules.MountBaseCapacity[MountSheetCatalog.WhiteBactrian];
            Assert.AreEqual(camel, MountSheetCatalog.Specs.Max(s => EncumbranceRules.MountBaseCapacity[s.MountId]));
            Assert.Less(camel, EncumbranceRules.MountBaseCapacity[MountMidRegionCatalog.RoofYak], "the yak stays the carrier of the passes");
            Assert.AreEqual(MountSheetCatalog.Specs.Min(s => EncumbranceRules.MountBaseCapacity[s.MountId]),
                EncumbranceRules.MountBaseCapacity[MountSheetCatalog.MarshHeron], "the wader carries least");
        }

        [Test]
        public void NoneOfThem_OpensTheBoatOrTheSaddleGates()
        {
            foreach (MountSpec spec in MountSheetCatalog.Specs)
            {
                MapRequirementState state = MapRequirementHeroState.Capture(null, null, null, new[] { new OwnedMount { guid = "a", mountId = spec.MountId } });
                Assert.IsFalse(MapRequirementRules.IsMet(MapRequirementTags.BoatOrDock, state), spec.MountId);
                Assert.IsFalse(MapRequirementRules.IsMet(MapRequirementTags.SaddleRequired, state), spec.MountId);
            }
        }

        // ---------------- Rules ----------------

        [Test]
        public void Howl_ScaresCommons_ThenSlowsThem_NeverBosses()
        {
            MountSpec wolf = MountCatalog.FindSpec(MountSheetCatalog.OchreWarWolf);
            float flee = MountSheetRules.HowlFleeSeconds(wolf.Duration, false, wolf.BossMultiplier);
            Assert.AreEqual(2f, flee, 1e-4f);
            Assert.AreEqual(0f, MountSheetRules.HowlFleeSeconds(wolf.Duration, true, wolf.BossMultiplier), 1e-4f, "bosses do not run");
            Assert.AreEqual(flee + MountSheetRules.HowlSlowSeconds, MountSheetRules.HowlSlowTotal(flee), 1e-4f);
            Assert.AreEqual(0f, MountSheetRules.HowlSlowTotal(0f), 1e-4f, "no run, no slow");
            Assert.That(wolf.Magnitude, Is.InRange(0.1f, 0.5f), "a slow, never a root");
        }

        [Test]
        public void Bound_LandsFarAhead_AndOnlyCommonsLoseTheRider()
        {
            MountSpec eland = MountCatalog.FindSpec(MountSheetCatalog.TallGrassEland);
            MountSpec frog = MountCatalog.FindSpec(MountMidRegionCatalog.TarchonFrog);
            Vector3 landing = MountSheetRules.BoundLanding(new Vector3(1f, 2f, 3f), new Vector3(0f, 5f, 2f), eland.Distance);
            Assert.AreEqual(new Vector3(1f, 2f, 3f + eland.Distance), landing, "flat forward, height kept");
            Assert.Greater(eland.Distance, frog.Distance, "the long jumper of the catalog");
            Assert.Greater(MountSheetRules.BoundHeight, MountMidRegionRules.LeapHeight);
            Assert.AreEqual(eland.Magnitude, MountSheetRules.LostSightSeconds(eland.Magnitude, false, eland.BossMultiplier), 1e-4f);
            Assert.AreEqual(0f, MountSheetRules.LostSightSeconds(eland.Magnitude, true, eland.BossMultiplier), 1e-4f);
        }

        [Test]
        public void Spit_GoesToTheNearestCritterAhead_OrStraightOn()
        {
            MountSpec olm = MountCatalog.FindSpec(MountSheetCatalog.LanternOlm);
            Vector3 from = Vector3.zero;
            Vector3 forward = Vector3.forward;
            Assert.AreEqual(new Vector3(0f, 0f, olm.Distance), MountSheetRules.SpitTarget(from, forward, olm.Distance, null), "nobody: straight on");

            var critters = new List<Vector3> { new Vector3(0f, 0f, -2f), new Vector3(1f, 0f, 6f), new Vector3(0f, 0f, 4f), new Vector3(0f, 0f, 30f) };
            Assert.AreEqual(new Vector3(0f, 0f, 4f), MountSheetRules.SpitTarget(from, forward, olm.Distance, critters), "the nearest ahead, never behind or out of range");

            Assert.AreEqual(olm.Duration, MountSheetRules.EffectSeconds(olm.Duration, false, olm.BossMultiplier), 1e-4f);
            Assert.AreEqual(olm.Duration * 0.5f, MountSheetRules.EffectSeconds(olm.Duration, true, olm.BossMultiplier), 1e-4f);
            Assert.Greater(MountSheetRules.SpitLampRadius, olm.Radius);
        }

        [Test]
        public void Stomp_SlowsEveryone_ButShovesOnlyCommons()
        {
            MountSpec aurochs = MountCatalog.FindSpec(MountSheetCatalog.ArattaAurochs);
            Assert.AreEqual(aurochs.Duration, MountSheetRules.EffectSeconds(aurochs.Duration, false, aurochs.BossMultiplier), 1e-4f);
            Assert.AreEqual(aurochs.Duration * 0.5f, MountSheetRules.EffectSeconds(aurochs.Duration, true, aurochs.BossMultiplier), 1e-4f);
            Assert.IsTrue(MountSheetRules.StompMoves(false));
            Assert.IsFalse(MountSheetRules.StompMoves(true));
            Assert.That(aurochs.Magnitude, Is.InRange(0.1f, 0.5f), "a slow, never a root");
        }

        [Test]
        public void WadeAndStorm_ProtectTheGroup_ForAShortWhile()
        {
            foreach (string id in new[] { MountSheetCatalog.MarshHeron, MountSheetCatalog.WhiteBactrian })
            {
                MountSpec spec = MountCatalog.FindSpec(id);
                Assert.That(spec.Duration, Is.InRange(5f, 12f), id);
                Assert.Greater(spec.Cooldown, spec.Duration, id + ": never always on");
                Assert.Greater(spec.Radius, 0f, id);
            }
        }

        // ---------------- Models ----------------

        [Test]
        public void Models_StayInBudget_AndKeepTheRiderSeat()
        {
            foreach (MountSpec spec in MountSheetCatalog.Specs)
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
        public void TheHeronWalksOnTwoLegs_TheCamelHasHumps_AndNoneFlies()
        {
            QuadrupedShape heron = MountRecipe.ShapeFor(MountVisualRecipe.MarshHeron);
            Assert.IsTrue(heron.Biped);
            Assert.IsTrue(heron.Bill);
            Assert.IsTrue(MountRecipe.ShapeFor(MountVisualRecipe.WhiteBactrian).Humps);
            Assert.IsTrue(MountRecipe.ShapeFor(MountVisualRecipe.TallGrassEland).Dewlap);
            foreach (MountSpec spec in MountSheetCatalog.Specs)
            {
                QuadrupedShape shape = MountRecipe.ShapeFor(spec.Recipe);
                Assert.IsFalse(shape.FlightWings, spec.MountId);
                Assert.IsFalse(shape.Sail, spec.MountId);
            }

            foreach (MountSpec spec in MountCatalog.Specs.Where(s => s.MountId != MountSheetCatalog.WhiteBactrian))
            {
                Assert.IsFalse(MountRecipe.ShapeFor(spec.Recipe).Humps, spec.MountId);
            }
        }
    }
}
