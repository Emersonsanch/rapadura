using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rapadura.Core.DI;
using Rapadura.Core.Events;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Achievements;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Housing;
using Rapadura.Gameplay.Mounts;
using Rapadura.Gameplay.Quests;
using Rapadura.Gameplay.Shop;
using Rapadura.Gameplay.World.Cities;
using Rapadura.Gameplay.World.Map.Overworld;
using Rapadura.UI.Achievements;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// The 25 achievements of the far content (Regions 10–16 bosses, one-cousin deeds, region mechanics, the whole world,
    /// "As Cartas da Estrada", new mounts and furniture sets): texts in both languages, a requirement that can really be met
    /// (every id exists in its catalog) and the event wiring of the conditions they added.
    /// </summary>
    public class AchievementFarContentTests
    {
        private static readonly string[] NewIds =
        {
            "ach_boss_wind_king", "ach_boss_old_kul", "ach_boss_ember_glutton", "ach_boss_apep", "ach_boss_dream", "ach_boss_great_dark",
            "ach_boss_az_captain", "ach_bosses_far_all", "ach_cousin_maithe_wind", "ach_cousin_joaquim_apep", "ach_cousin_maria_dark",
            "ach_cousin_icaro_dream", "ach_cousin_lavine_az", "ach_nartes_race_gold", "ach_lavine_sky_golds", "ach_amirani_chain_top",
            "ach_porch_story", "ach_sky_whole", "ach_world_all_regions", "ach_letters_complete", "ach_letters_all_cousins",
            "ach_mounts_far_regions", "ach_column_griffin", "ach_house_cais_de_luz", "ach_house_far_sets",
        };

        private readonly List<AchievementManager> _managers = new List<AchievementManager>();
        private readonly List<UnityEngine.Object> _created = new List<UnityEngine.Object>();

        [SetUp]
        public void SetUp()
        {
            TestSaveRoot.Enter();
            EventBus.Clear();
        }

        [TearDown]
        public void TearDown()
        {
            foreach (AchievementManager manager in _managers)
            {
                manager.Shutdown();
            }

            _managers.Clear();
            ServiceLocator.Unregister<CharacterSelectionState>();
            EventBus.Clear();

            foreach (UnityEngine.Object created in _created)
            {
                UnityEngine.Object.DestroyImmediate(created);
            }

            _created.Clear();
            TestSaveRoot.Exit();
        }

        private static AchievementDefinition[] Seeded() => Resources.LoadAll<AchievementDefinition>(AchievementManager.ResourcesFolder);

        private static IEnumerable<AchievementDefinition> NewDefinitions()
        {
            AchievementDefinition[] seeded = Seeded();
            return NewIds.Select(id => seeded.FirstOrDefault(d => d.AchievementId == id));
        }

        private AchievementManager Manager(params AchievementDefinition[] definitions)
        {
            var manager = new AchievementManager(definitions);
            manager.Initialize();
            _managers.Add(manager);
            return manager;
        }

        private AchievementDefinition Def(AchievementConditionType type, params string[] ids)
        {
            var definition = ScriptableObject.CreateInstance<AchievementDefinition>();
            definition.ConfigureForTests("test_" + type, type, ids.Length, ids);
            _created.Add(definition);
            return definition;
        }

        [Test]
        public void Every_new_achievement_exists_and_has_both_languages()
        {
            Assert.AreEqual(25, NewIds.Length);
            Assert.AreEqual(NewIds.Length, NewIds.Distinct().Count());

            foreach (AchievementDefinition definition in NewDefinitions())
            {
                Assert.IsNotNull(definition, "missing asset");
                foreach (string key in new[] { definition.TitleKey, definition.DescriptionKey })
                {
                    LocalizationEntry entry = AchievementTexts.All.FirstOrDefault(e => e.Key == key);
                    Assert.IsNotNull(entry, key);
                    Assert.IsFalse(string.IsNullOrWhiteSpace(entry.En), key);
                    Assert.IsFalse(string.IsNullOrWhiteSpace(entry.PtBR), key);
                }
            }
        }

        [Test]
        public void Every_requirement_points_at_something_that_exists()
        {
            var regions = new HashSet<string>(OverworldLayouts.StitchedWorld.Parts.Select(p => p.Id));

            foreach (AchievementDefinition definition in NewDefinitions())
            {
                string label = definition.AchievementId;
                Assert.IsTrue(definition.UsesTargetIds, label);
                foreach (string id in definition.TargetIds)
                {
                    switch (definition.ConditionType)
                    {
                        case AchievementConditionType.BossesDefeated:
                            AssertWorldBoss(id, label);
                            break;
                        case AchievementConditionType.HeroDeed:
                            AssertHeroDeed(id, label);
                            break;
                        case AchievementConditionType.RegionFeat:
                            Assert.IsTrue(RegionFeats.IsKnown(id, OverworldLayouts.NartesSkyGolds.Length), $"{label}: {id}");
                            break;
                        case AchievementConditionType.WorldRegionsVisited:
                            Assert.IsTrue(regions.Contains(id), $"{label}: {id}");
                            break;
                        case AchievementConditionType.QuestsCompleted:
                            Assert.IsNotNull(LateStoryCatalog.FindStep(id), $"{label}: {id}");
                            break;
                        case AchievementConditionType.MountsOwned:
                            Assert.IsNotNull(MountCatalog.FindSpec(id), $"{label}: {id}");
                            break;
                        case AchievementConditionType.HouseItemsCollected:
                            AssertHouseItemObtainable(id, label);
                            break;
                        default:
                            Assert.Fail($"{label}: unexpected condition {definition.ConditionType}");
                            break;
                    }
                }
            }
        }

        private static void AssertWorldBoss(string enemyId, string label)
        {
            Assert.IsTrue(RegionBossRules.IsRegionBoss(enemyId), $"{label}: {enemyId}");
            EnemyDefinition definition = Resources.Load<EnemyDefinition>("Enemies/" + enemyId);
            Assert.IsNotNull(definition, $"{label}: {enemyId}");
            Assert.IsTrue(definition.IsBoss, $"{label}: {enemyId} must be a boss, or EnemyDiedEvent.WasBoss never counts");
            Assert.AreEqual(enemyId, DefeatedEnemyRegistry.GetId(definition), label);
        }

        private static void AssertHeroDeed(string id, string label)
        {
            string[] parts = id.Split(':');
            Assert.AreEqual(2, parts.Length, $"{label}: {id}");
            Assert.IsTrue(Enum.GetValues(typeof(CharacterId)).Cast<CharacterId>().Any(h => LateStoryCatalog.HeroKey(h) == parts[0]), $"{label}: {id}");
            if (LateStoryCatalog.FindStep(parts[1]) == null)
            {
                AssertWorldBoss(parts[1], label);
            }
        }

        private static void AssertHouseItemObtainable(string itemId, string label)
        {
            HouseItemDefinition item = HouseCatalog.Get(itemId);
            Assert.IsNotNull(item, $"{label}: {itemId}");
            Assert.AreEqual(CityShopEntryKind.HouseItem, CityShopCatalog.EntryOf(item.PrimarySource.RefId, itemId)?.Kind, $"{label}: {itemId} must be on sale");
        }

        [Test]
        public void Lavine_needs_every_Gold_of_Heaven_and_the_rest_of_the_list_is_complete()
        {
            AchievementDefinition[] seeded = Seeded();
            Assert.AreEqual(OverworldLayouts.NartesSkyGolds.Length, seeded.Single(d => d.AchievementId == "ach_lavine_sky_golds").TargetValue);
            CollectionAssert.AreEquivalent(RegionBossRules.Spawns.Select(s => s.EnemyId), seeded.Single(d => d.AchievementId == "ach_bosses_far_all").TargetIds);
            CollectionAssert.AreEquivalent(OverworldLayouts.StitchedWorld.Parts.Select(p => p.Id), seeded.Single(d => d.AchievementId == "ach_world_all_regions").TargetIds);
            CollectionAssert.AreEquivalent(MountFarRegionCatalog.Specs.Select(s => s.MountId), seeded.Single(d => d.AchievementId == "ach_mounts_far_regions").TargetIds);
            Assert.AreEqual(LateStoryCatalog.SkyPorch, seeded.Single(d => d.AchievementId == "ach_letters_complete").TargetIds.Single());
        }

        [Test]
        public void Rewards_point_at_real_achievements_and_items()
        {
            var ids = new HashSet<string>(Seeded().Select(d => d.AchievementId));
            Assert.IsTrue(ids.Contains(MountSourceHooks.AchievementAmigoDaCaipora));
            Assert.IsTrue(ids.Contains(MountSourceHooks.AchievementPacienciaDeJabuti));
            foreach (KeyValuePair<string, string[]> reward in HouseRewardRules.ExtraAchievementRewards)
            {
                Assert.IsTrue(ids.Contains(reward.Key), reward.Key);
                foreach (string item in reward.Value)
                {
                    Assert.IsNotNull(HouseCatalog.Get(item), item);
                    CollectionAssert.Contains(HouseRewardRules.AchievementRewards(reward.Key), item);
                }
            }
        }

        [Test]
        public void Region_feats_unlock_on_their_events_and_count_once()
        {
            AchievementDefinition definition = Def(AchievementConditionType.RegionFeat, RegionFeats.NartesSkyGold(0), RegionFeats.NartesSkyGold(1));
            AchievementManager manager = Manager(definition);

            EventBus.Publish(new RegionFeatEvent(RegionFeats.NartesSkyGold(0)));
            EventBus.Publish(new RegionFeatEvent(RegionFeats.NartesSkyGold(0)));
            Assert.IsFalse(manager.IsUnlocked(definition.AchievementId));

            EventBus.Publish(new RegionFeatEvent(RegionFeats.NartesSkyGold(1)));
            Assert.IsTrue(manager.IsUnlocked(definition.AchievementId));
        }

        [Test]
        public void A_hero_deed_counts_only_for_the_selected_cousin()
        {
            var selection = new CharacterSelectionState();
            selection.Select(CharacterId.Maria);
            ServiceLocator.Register(selection);
            string quest = LateStoryCatalog.SkyPorch;
            AchievementDefinition definition = Def(AchievementConditionType.HeroDeed, AchievementManager.HeroDeedId("lavine", quest));
            AchievementManager manager = Manager(definition);

            EventBus.Publish(new QuestCompletedEvent(quest));
            Assert.IsFalse(manager.IsUnlocked(definition.AchievementId), "Maria finished it, not Lavine");

            selection.Select(CharacterId.Lavine);
            EventBus.Publish(new QuestCompletedEvent(quest));
            Assert.IsTrue(manager.IsUnlocked(definition.AchievementId));
        }

        [Test]
        public void Mounts_and_house_items_unlock_on_their_events()
        {
            AchievementDefinition mounts = Def(AchievementConditionType.MountsOwned, MountSkyCatalog.ColumnGriffin);
            AchievementDefinition house = Def(AchievementConditionType.HouseItemsCollected, "casa_sala_054", "casa_luz_037");
            AchievementManager manager = Manager(mounts, house);

            EventBus.Publish(new MountGrantedEvent(MountSkyCatalog.ColumnGriffin, "guid", MountCollection.SourceAchievement));
            Assert.IsTrue(manager.IsUnlocked(mounts.AchievementId));

            EventBus.Publish(new ItemPurchasedEvent("shop_ultima_caravana_ammo", "casa_sala_054", 1, 10));
            EventBus.Publish(new ItemPurchasedEvent("shop_ultima_caravana_ammo", "item_gold", 1, 10));
            Assert.IsFalse(manager.IsUnlocked(house.AchievementId));

            EventBus.Publish(new HouseItemGrantedEvent("casa_luz_037", HouseGrantReason.Achievement, "ach"));
            Assert.IsTrue(manager.IsUnlocked(house.AchievementId));
        }
    }
}
