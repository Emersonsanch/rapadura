using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Rapadura.Core.Events;
using Rapadura.Core.Systems;
using Rapadura.Gameplay.Achievements;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Dialogue;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.Items.Procedural;
using Rapadura.Gameplay.Quests;
using Rapadura.Gameplay.Story;
using Rapadura.Gameplay.World.Biomes;
using Rapadura.Gameplay.World.Map;
using Rapadura.Save;
using Rapadura.UI.Menus;
using Rapadura.UI.WorldMap;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Rapadura.Tests
{
    /// <summary>
    /// "Nova Jornada+" (<see cref="NewGamePlusRules"/>, <see cref="NewGamePlusProgress"/>): the scaling numbers
    /// (Docs/Balance/README.md §11), what a new journey resets and keeps, the save round-trip, saves written
    /// before it existed (cycle 0), the new-journey prologue and the "finish NG+1" achievement.
    /// </summary>
    public class NewGamePlusTests
    {
        private const int Slot = 90051;
        private const int LegacySlot = 90052;
        private const string PersonalQuestId = "quest_personal_maithe_test";
        private const string BossId = "enemy_forest_alpha_wolf";

        private readonly List<Action> _shutdowns = new List<Action>();
        private readonly List<Object> _created = new List<Object>();
        private readonly List<int> _started = new List<int>();
        private readonly List<int> _cyclesCompleted = new List<int>();

        [SetUp]
        public void SetUp()
        {
            TestSaveRoot.Enter();
            EventBus.Clear();
            _started.Clear();
            _cyclesCompleted.Clear();
            EventBus.Subscribe<NewGamePlusStartedEvent>(OnStarted);
            EventBus.Subscribe<NewGamePlusCycleCompletedEvent>(OnCycleCompleted);
        }

        [TearDown]
        public void TearDown()
        {
            foreach (Action shutdown in _shutdowns)
            {
                shutdown();
            }

            _shutdowns.Clear();
            foreach (Object obj in _created)
            {
                if (obj != null)
                {
                    Object.DestroyImmediate(obj);
                }
            }

            _created.Clear();
            EventBus.Clear();
            TestSaveRoot.Exit();
        }

        private void OnStarted(NewGamePlusStartedEvent evt) => _started.Add(evt.Cycle);

        private void OnCycleCompleted(NewGamePlusCycleCompletedEvent evt) => _cyclesCompleted.Add(evt.Cycle);

        /// <summary>A GameManager-like set: map, story, bosses, quests and Nova Jornada+ wired together.</summary>
        private sealed class Session
        {
            public WorldMapProgress Map;
            public StoryProgress Story;
            public DefeatedEnemyRegistry Bosses;
            public QuestManager Quests;
            public NewGamePlusProgress NewGamePlus;
            public NewSessionReset Reset;
            public SaveManager Saves;
        }

        private Session NewSession()
        {
            var s = new Session { Map = new WorldMapProgress(), Bosses = new DefeatedEnemyRegistry(), Quests = new QuestManager() };
            s.Story = new StoryProgress(s.Map);
            s.NewGamePlus = new NewGamePlusProgress(s.Story, s.Map, s.Bosses, s.Quests);
            s.Story.Initialize();
            s.NewGamePlus.Initialize();
            _shutdowns.Add(s.Story.Shutdown);
            _shutdowns.Add(s.NewGamePlus.Shutdown);

            s.Reset = new NewSessionReset();
            s.Reset.Register(s.Quests);
            s.Reset.Register(s.Map);
            s.Reset.Register(s.Story);
            s.Reset.Register(s.NewGamePlus);
            s.Reset.Register(s.Bosses);

            s.Saves = new SaveManager { SessionReset = s.Reset };
            s.Saves.Initialize();
            s.Saves.Register(s.Quests);
            s.Saves.Register(s.Map);
            s.Saves.Register(s.Story);
            s.Saves.Register(s.NewGamePlus);
            s.Saves.Register(s.Bosses);
            return s;
        }

        /// <summary>Plays the whole campaign: every region cleared, every beat seen (credits included).</summary>
        private static void FinishCampaign(Session s)
        {
            foreach (BiomeType region in (BiomeType[])Enum.GetValues(typeof(BiomeType)))
            {
                s.Map.Unlock(region);
                s.Map.MarkRegionCleared(region);
            }

            s.Story.MarkCampaignComplete();
            for (StoryBeat beat = s.Story.GetPendingBeat(); beat != StoryBeat.None; beat = s.Story.GetPendingBeat())
            {
                s.Story.MarkSeen(beat);
            }
        }

        // ---------------------------------------------------------------
        // Rules
        // ---------------------------------------------------------------

        [Test]
        public void Rules_CycleZero_ChangesNothing()
        {
            Assert.AreEqual(1f, NewGamePlusRules.HealthMultiplier(0));
            Assert.AreEqual(1f, NewGamePlusRules.DamageMultiplier(0));
            Assert.AreEqual(1f, NewGamePlusRules.ExperienceMultiplier(0));
            Assert.AreEqual(1f, NewGamePlusRules.LootQuantityMultiplier(0));
            Assert.AreEqual(0f, NewGamePlusRules.RarityUpgradeChance(0));

            Assert.AreEqual(517.3f, NewGamePlusRules.ScaleHealth(517.3f, 0), "Cycle 0 must not even round.");
            Assert.AreEqual(14.25f, NewGamePlusRules.ScaleDamage(14.25f, 0));
            Assert.AreEqual(90, NewGamePlusRules.ScaleExperience(90, 0));
            Assert.AreEqual(3, NewGamePlusRules.ScaleLootQuantity(3, 0));
            Assert.AreEqual(ItemRarity.Common, NewGamePlusRules.UpgradeRarity(ItemRarity.Common, 0, 0.0));
        }

        [Test]
        public void Rules_MatchTheBalanceDoc_ForNgPlus1()
        {
            // Docs/Balance/README.md §11.1: Lobo Alfa 520 HP / 15 dmg / 90 XP on NG+1 (Floresta ×2,2 / ×2,7),
            // Habitante Ancestral 1180 / 42 (Cavernas ×1,15 / ×1,45).
            Assert.AreEqual(1144f, NewGamePlusRules.ScaleHealth(520f, 1, BiomeType.Forest));
            Assert.AreEqual(40.5f, NewGamePlusRules.ScaleDamage(15f, 1, BiomeType.Forest));
            Assert.AreEqual(1357f, NewGamePlusRules.ScaleHealth(1180f, 1, BiomeType.Caves));
            Assert.AreEqual(60.9f, NewGamePlusRules.ScaleDamage(42f, 1, BiomeType.Caves));
            Assert.AreEqual(1.5f, NewGamePlusRules.HealthMultiplier(1), 1e-5f, "no region: the Deserto numbers");
            Assert.AreEqual(1.8f, NewGamePlusRules.DamageMultiplier(1), 1e-5f);
            Assert.AreEqual(135, NewGamePlusRules.ScaleExperience(90, 1));
            Assert.AreEqual(4, NewGamePlusRules.ScaleLootQuantity(3, 1), "3 × 1,25 = 3,75 → 4");
            Assert.AreEqual(0.1f, NewGamePlusRules.RarityUpgradeChance(1), 1e-6f);

            // Cap: NG+5 Floresta HP ×2,8 / damage ×3,7, Cavernas ×1,75 / ×2,45; XP ×3,5 everywhere.
            Assert.AreEqual(2.8f, NewGamePlusRules.HealthMultiplier(5, BiomeType.Forest), 1e-5f);
            Assert.AreEqual(3.7f, NewGamePlusRules.DamageMultiplier(5, BiomeType.Forest), 1e-5f);
            Assert.AreEqual(1.75f, NewGamePlusRules.HealthMultiplier(5, BiomeType.Caves), 1e-5f);
            Assert.AreEqual(2.45f, NewGamePlusRules.DamageMultiplier(5, BiomeType.Caves), 1e-5f);
            Assert.AreEqual(3.5f, NewGamePlusRules.ExperienceMultiplier(5), 1e-5f);
        }

        [Test]
        public void Rules_GrowEveryCycle_ThenStopAtTheCap()
        {
            for (int cycle = 1; cycle <= NewGamePlusRules.MaxCycle; cycle++)
            {
                Assert.Greater(NewGamePlusRules.HealthMultiplier(cycle), NewGamePlusRules.HealthMultiplier(cycle - 1), $"HP NG+{cycle}");
                Assert.Greater(NewGamePlusRules.DamageMultiplier(cycle), NewGamePlusRules.DamageMultiplier(cycle - 1), $"damage NG+{cycle}");
                Assert.Greater(NewGamePlusRules.ExperienceMultiplier(cycle), NewGamePlusRules.ExperienceMultiplier(cycle - 1), $"XP NG+{cycle}");
                Assert.Greater(NewGamePlusRules.LootQuantityMultiplier(cycle), NewGamePlusRules.LootQuantityMultiplier(cycle - 1), $"loot NG+{cycle}");
                Assert.LessOrEqual(NewGamePlusRules.RarityUpgradeChance(cycle), 0.5f);

                // Per region: tougher every cycle, and the regions the carried-over level outgrows the most (the
                // early ones) always get the most. Damage may outgrow HP: the hero's own HP grows with every level
                // (the r9 model, Balance §14.4), so damage is what keeps a parked hero dying in ~25 s.
                BiomeType[] order = { BiomeType.Forest, BiomeType.Desert, BiomeType.Mountain, BiomeType.Caves };
                for (int i = 0; i < order.Length; i++)
                {
                    Assert.Greater(NewGamePlusRules.HealthMultiplier(cycle, order[i]), NewGamePlusRules.HealthMultiplier(cycle - 1, order[i]), $"HP {order[i]} NG+{cycle}");
                    Assert.Greater(NewGamePlusRules.DamageMultiplier(cycle, order[i]), NewGamePlusRules.DamageMultiplier(cycle - 1, order[i]), $"damage {order[i]} NG+{cycle}");
                    if (i > 0)
                    {
                        Assert.Greater(NewGamePlusRules.HealthMultiplier(cycle, order[i - 1]), NewGamePlusRules.HealthMultiplier(cycle, order[i]), $"HP {order[i - 1]} > {order[i]}");
                        Assert.Greater(NewGamePlusRules.DamageMultiplier(cycle, order[i - 1]), NewGamePlusRules.DamageMultiplier(cycle, order[i]), $"damage {order[i - 1]} > {order[i]}");
                    }
                }
            }

            foreach (BiomeType region in new[] { BiomeType.Forest, BiomeType.Desert, BiomeType.Mountain, BiomeType.Caves })
            {
                Assert.AreEqual(1f, NewGamePlusRules.HealthMultiplier(0, region), $"cycle 0 HP in {region}");
                Assert.AreEqual(1f, NewGamePlusRules.DamageMultiplier(0, region), $"cycle 0 damage in {region}");
                Assert.AreEqual(517.3f, NewGamePlusRules.ScaleHealth(517.3f, 0, region), "cycle 0 never rounds, in any region");
            }

            int max = NewGamePlusRules.MaxCycle;
            Assert.AreEqual(NewGamePlusRules.HealthMultiplier(max), NewGamePlusRules.HealthMultiplier(max + 3));
            Assert.AreEqual(max, NewGamePlusRules.NextCycle(max), "Later journeys stay on the cap.");
            Assert.AreEqual(1, NewGamePlusRules.NextCycle(0));
            Assert.AreEqual(0, NewGamePlusRules.ClampCycle(-2));
            Assert.AreEqual(1f, NewGamePlusRules.HealthMultiplier(-1), "A corrupt negative cycle plays as the first campaign.");
        }

        [Test]
        public void Rules_RarityUpgrade_IsOneTier_AndNeverPastLegendary()
        {
            Assert.AreEqual(ItemRarity.Uncommon, NewGamePlusRules.UpgradeRarity(ItemRarity.Common, 1, 0.05));
            Assert.AreEqual(ItemRarity.Common, NewGamePlusRules.UpgradeRarity(ItemRarity.Common, 1, 0.5), "Roll over the 10% chance: no upgrade.");
            Assert.AreEqual(ItemRarity.Legendary, NewGamePlusRules.UpgradeRarity(ItemRarity.Epic, 5, 0.0));
            Assert.AreEqual(ItemRarity.Legendary, NewGamePlusRules.UpgradeRarity(ItemRarity.Legendary, 5, 0.0));
        }

        [Test]
        public void Rules_Offer_NeedsTheCompletedCampaign_AndItsEnding()
        {
            Assert.IsTrue(NewGamePlusRules.CanOffer(true, true));
            Assert.IsFalse(NewGamePlusRules.CanOffer(true, false), "Boss down, ending/credits not shown yet.");
            Assert.IsFalse(NewGamePlusRules.CanOffer(false, false));
        }

        [Test]
        public void EveryBossScales_ByTheRegionItsBiomeTableListsItIn()
        {
            // EnemyController scales by BiomeRegistry.FindBiomeOf(definition).Type on a new journey.
            var expected = new Dictionary<string, BiomeType>
            {
                { "enemy_forest_alpha_wolf", BiomeType.Forest },
                { "enemy_desert_ruin_guardian", BiomeType.Desert },
                { "enemy_mountain_warlord", BiomeType.Mountain },
                { "enemy_cave_dweller", BiomeType.Caves },
            };
            foreach (KeyValuePair<string, BiomeType> pair in expected)
            {
                EnemyDefinition boss = Resources.Load<EnemyDefinition>("Enemies/" + pair.Key);
                Assert.IsNotNull(boss, pair.Key);
                BiomeDefinition biome = BiomeRegistry.FindBiomeOf(boss);
                Assert.IsNotNull(biome, pair.Key);
                Assert.AreEqual(pair.Value, biome.Type, pair.Key);
            }
        }

        [Test]
        public void ExperienceAndLootScaling_NeverGiveLessThanTheBase()
        {
            for (int cycle = 0; cycle <= NewGamePlusRules.MaxCycle + 1; cycle++)
            {
                foreach (int amount in new[] { 0, 1, 2, 10, 90 })
                {
                    Assert.GreaterOrEqual(NewGamePlusRules.ScaleExperience(amount, cycle), amount);
                    Assert.GreaterOrEqual(NewGamePlusRules.ScaleLootQuantity(amount, cycle), amount);
                }
            }
        }

        // ---------------------------------------------------------------
        // Loot (EnemyLootRoller)
        // ---------------------------------------------------------------

        [Test]
        public void Loot_CycleZero_DrawsExactlyTheSameDropsAsBefore()
        {
            EnemyDefinition wolf = Resources.Load<EnemyDefinition>("Enemies/enemy_forest_wolf");
            Assert.IsNotNull(wolf);
            BiomeDefinition forest = BiomeRegistry.FindBiomeOf(wolf);

            var legacy = new System.Random(11);
            var zero = new System.Random(11);
            for (int i = 0; i < 50; i++)
            {
                List<LootDrop> a = EnemyLootRoller.Roll(wolf, forest, legacy, Array.Empty<ItemAffixDefinition>());
                List<LootDrop> b = EnemyLootRoller.Roll(wolf, forest, zero, Array.Empty<ItemAffixDefinition>(), newGamePlusCycle: 0);
                CollectionAssert.AreEqual(a.Select(Describe).ToList(), b.Select(Describe).ToList(), $"kill {i}");
            }
        }

        [Test]
        public void Loot_OnNgPlus_GivesMoreGold_AndBetterBossGear()
        {
            EnemyDefinition wolf = Resources.Load<EnemyDefinition>("Enemies/enemy_forest_wolf");
            EnemyDefinition warlord = Resources.Load<EnemyDefinition>("Enemies/enemy_mountain_warlord");
            Assert.IsNotNull(wolf);
            Assert.IsNotNull(warlord);

            Assert.Greater(TotalOf(wolf, NewGamePlusRules.MaxCycle, wolf.LootItem.ItemId), TotalOf(wolf, 0, wolf.LootItem.ItemId),
                "NG+5 wolves should drop more gold than first-campaign wolves.");
            Assert.Greater(MeanTableRarity(warlord, NewGamePlusRules.MaxCycle), MeanTableRarity(warlord, 0),
                "NG+5 bosses should roll better gear off their biome table.");
        }

        private static int TotalOf(EnemyDefinition enemy, int cycle, string itemId)
        {
            var random = new System.Random(3);
            BiomeDefinition biome = BiomeRegistry.FindBiomeOf(enemy);
            int total = 0;
            for (int i = 0; i < 300; i++)
            {
                total += EnemyLootRoller.Roll(enemy, biome, random, Array.Empty<ItemAffixDefinition>(), newGamePlusCycle: cycle)
                    .Where(drop => !drop.IsGenerated && drop.Item.ItemId == itemId)
                    .Sum(drop => drop.Quantity);
            }

            return total;
        }

        private static double MeanTableRarity(EnemyDefinition boss, int cycle)
        {
            var random = new System.Random(5);
            BiomeDefinition biome = BiomeRegistry.FindBiomeOf(boss);
            var rarities = new List<int>();
            for (int i = 0; i < 300; i++)
            {
                List<LootDrop> drops = EnemyLootRoller.Roll(boss, biome, random, Array.Empty<ItemAffixDefinition>(), newGamePlusCycle: cycle);
                // The last drop is the fixed-rarity boss bonus; only the table's pieces are rolled.
                int tableDrops = BossBonusRules.HasBonusDrop(boss) ? drops.Count - 1 : drops.Count;
                rarities.AddRange(drops.Take(tableDrops).Where(drop => drop.IsGenerated).Select(drop => (int)drop.Generated.Rarity));
            }

            Assume.That(rarities, Is.Not.Empty, $"{boss.name}'s biome table rolls no equipment.");
            return rarities.Average();
        }

        private static string Describe(LootDrop drop)
        {
            return drop.IsGenerated ? $"{drop.Generated.BaseItem.ItemId}:{drop.Generated.Rarity}" : $"{drop.Item.ItemId}x{drop.Quantity}";
        }

        // ---------------------------------------------------------------
        // Starting a new journey
        // ---------------------------------------------------------------

        [Test]
        public void BeginNextCycle_IsRefused_BeforeTheCampaignAndItsEndingAreDone()
        {
            Session s = NewSession();
            Assert.IsFalse(s.NewGamePlus.CanBeginNextCycle);
            Assert.IsFalse(s.NewGamePlus.BeginNextCycle());

            s.Story.MarkCampaignComplete();
            Assert.IsFalse(s.NewGamePlus.BeginNextCycle(), "Cavernas boss down but the ending not shown yet.");
            Assert.AreEqual(0, s.NewGamePlus.Cycle);
            Assert.IsEmpty(_started);
        }

        [Test]
        public void BeginNextCycle_ResetsStoryMapBossesAndRegionQuests_KeepsPersonalQuests()
        {
            Session s = NewSession();
            FinishCampaign(s);
            s.Bosses.MarkDefeated(BossId);
            s.Quests.RestoreState(new QuestSaveState
            {
                CompletedQuestIds = new List<string> { RegionQuestCatalog.ForestFirst, RegionQuestCatalog.CavesSecond, PersonalQuestId },
            });

            Assert.IsTrue(s.NewGamePlus.BeginNextCycle());

            Assert.AreEqual(1, s.NewGamePlus.Cycle);
            CollectionAssert.AreEqual(new[] { 1 }, _started);
            Assert.IsFalse(s.Story.CampaignComplete);
            Assert.IsEmpty(s.Story.SeenBeats);
            Assert.AreEqual(StoryBeat.Prologue, s.Story.GetPendingBeat(), "The new journey opens with its prologue.");
            CollectionAssert.AreEquivalent(new[] { BiomeType.Forest }, s.Map.UnlockedRegions.ToArray());
            Assert.IsEmpty(s.Map.ClearedRegions);
            Assert.IsFalse(s.Bosses.IsDefeated(BossId), "Bosses are back.");
            Assert.IsFalse(s.Quests.IsCompleted(RegionQuestCatalog.ForestFirst), "Region quests can be taken again.");
            Assert.IsFalse(s.Quests.IsCompleted(RegionQuestCatalog.CavesSecond));
            Assert.IsTrue(s.Quests.IsCompleted(PersonalQuestId), "The cousins' own stories stay done.");
            Assert.IsFalse(s.NewGamePlus.CanBeginNextCycle, "No second offer until this journey is finished too.");
        }

        [Test]
        public void BeginNextCycle_DoesNotTreatTheLiveSessionAsALegacyLoad()
        {
            Session s = NewSession();
            FinishCampaign(s);
            s.NewGamePlus.BeginNextCycle();

            // A carried-state restore on entering an encounter raises GameLoadedEvent: the prologue must stay pending.
            EventBus.Publish(new GameLoadedEvent());
            Assert.AreEqual(StoryBeat.Prologue, s.Story.GetPendingBeat());
        }

        [Test]
        public void FinishingANgPlusCampaign_RaisesTheCycleCompletedEvent_OnlyOnNgPlus()
        {
            Session s = NewSession();
            FinishCampaign(s);
            Assert.IsEmpty(_cyclesCompleted, "The first campaign is not a Nova Jornada+.");
            Assert.AreEqual(0, s.NewGamePlus.HighestCompletedCycle);

            s.NewGamePlus.BeginNextCycle();
            FinishCampaign(s);

            CollectionAssert.AreEqual(new[] { 1 }, _cyclesCompleted);
            Assert.AreEqual(1, s.NewGamePlus.HighestCompletedCycle);
            Assert.IsTrue(s.NewGamePlus.CanBeginNextCycle);
            Assert.AreEqual(2, s.NewGamePlus.NextCycle);
        }

        [Test]
        public void FinishingNgPlus1_UnlocksTheAchievement()
        {
            var definition = ScriptableObject.CreateInstance<AchievementDefinition>();
            _created.Add(definition);
            definition.ConfigureForTests("ach_test_ngplus", AchievementConditionType.NewGamePlusCompleted, 1);
            var achievements = new AchievementManager(new[] { definition });
            achievements.Initialize();
            _shutdowns.Add(achievements.Shutdown);

            Session s = NewSession();
            FinishCampaign(s);
            Assert.IsFalse(achievements.IsUnlocked("ach_test_ngplus"), "Finishing the first campaign is not enough.");

            s.NewGamePlus.BeginNextCycle();
            Assert.IsFalse(achievements.IsUnlocked("ach_test_ngplus"), "Starting NG+1 is not enough either.");

            FinishCampaign(s);
            Assert.IsTrue(achievements.IsUnlocked("ach_test_ngplus"));
        }

        [Test]
        public void SeededCatalog_HasTheNgPlusAchievement()
        {
            AchievementDefinition achievement = Resources.LoadAll<AchievementDefinition>(AchievementManager.ResourcesFolder)
                .FirstOrDefault(d => d.AchievementId == "ach_new_journey_plus");

            Assert.IsNotNull(achievement, "Resources/Achievements/ach_new_journey_plus.asset is missing.");
            Assert.AreEqual(AchievementConditionType.NewGamePlusCompleted, achievement.ConditionType);
            Assert.AreEqual(1, achievement.TargetValue);
        }

        // ---------------------------------------------------------------
        // Save / load
        // ---------------------------------------------------------------

        [Test]
        public void SaveAndLoad_KeepsTheCycle_AndTheResetCampaign()
        {
            Session writer = NewSession();
            FinishCampaign(writer);
            writer.NewGamePlus.BeginNextCycle();
            FinishCampaign(writer);
            writer.NewGamePlus.BeginNextCycle();
            writer.Story.MarkSeen(StoryBeat.Prologue);
            writer.Saves.Save(Slot);
            Assume.That(File.Exists(TestSaveRoot.SlotPath(Slot)));

            Session reader = NewSession();
            Assert.IsTrue(reader.Saves.Load(Slot));

            Assert.AreEqual(2, reader.NewGamePlus.Cycle);
            Assert.AreEqual(1, reader.NewGamePlus.HighestCompletedCycle);
            Assert.IsTrue(reader.Story.HasSeen(StoryBeat.Prologue));
            Assert.IsFalse(reader.Story.CampaignComplete);
            Assert.IsFalse(reader.Map.IsRegionCleared(BiomeType.Forest));
        }

        [Test]
        public void LegacySave_WithoutTheEntry_LoadsAsCycleZero()
        {
            // Pre-Nova Jornada+ schema: no "NewGamePlus" entry in the file.
            var legacyMap = new WorldMapProgress();
            var legacyWriter = new SaveManager();
            legacyWriter.Initialize();
            legacyWriter.Register(legacyMap);
            legacyWriter.Save(LegacySlot);

            // The session in memory was on NG+3: the load must not keep it.
            Session s = NewSession();
            s.NewGamePlus.RestoreState(new NewGamePlusSaveState { cycle = 3, highestCompletedCycle = 2 });
            Assert.IsTrue(s.Saves.Load(LegacySlot));

            Assert.AreEqual(0, s.NewGamePlus.Cycle);
            Assert.AreEqual(0, s.NewGamePlus.HighestCompletedCycle);
        }

        [Test]
        public void RestoreState_ClampsCorruptValues_AndIgnoresOtherTypes()
        {
            Session s = NewSession();
            s.NewGamePlus.RestoreState(new NewGamePlusSaveState { cycle = 99, highestCompletedCycle = -4 });
            Assert.AreEqual(NewGamePlusRules.MaxCycle, s.NewGamePlus.Cycle);
            Assert.AreEqual(0, s.NewGamePlus.HighestCompletedCycle);

            Assert.DoesNotThrow(() => s.NewGamePlus.RestoreState("not a state"));
            Assert.AreEqual(NewGamePlusRules.MaxCycle, s.NewGamePlus.Cycle);

            s.Reset.ResetAll();
            Assert.AreEqual(0, s.NewGamePlus.Cycle, "\"Novo jogo\" starts on the first campaign.");
        }

        [Test]
        public void TitleContinue_ReadsTheCycleOfTheNewestSave()
        {
            Session s = NewSession();
            Assert.AreEqual(0, MainMenuPresenter.GetContinueCycle(s.Saves), "No save at all.");

            s.Saves.Save(0);
            Assert.AreEqual(0, MainMenuPresenter.GetContinueCycle(s.Saves));

            FinishCampaign(s);
            s.NewGamePlus.BeginNextCycle();
            s.Saves.Save(0);
            Assert.AreEqual(1, MainMenuPresenter.GetContinueCycle(s.Saves));
        }

        // ---------------------------------------------------------------
        // Story and HUD texts
        // ---------------------------------------------------------------

        [Test]
        public void NewJourneyPrologue_ReplacesThePrologue_WithAPanelPerCousin()
        {
            CollectionAssert.AreEqual(StoryScript.GetPanelKeys(StoryBeat.Prologue, CharacterId.Maria),
                StoryScript.GetPanelKeys(StoryBeat.Prologue, CharacterId.Maria, 0), "Cycle 0 is the original prologue.");

            var sequence = new StoryPanelSequence(StoryBeat.Prologue, CharacterId.Icaro, 1);
            Assert.AreEqual("story.new_journey.title", sequence.TitleKey);
            CollectionAssert.Contains(sequence.PanelKeys, "story.new_journey.hero.icaro");
            Assert.IsTrue(sequence.PanelKeys.All(key => key.StartsWith("story.new_journey.", StringComparison.Ordinal)));

            CollectionAssert.AreEqual(StoryScript.GetPanelKeys(StoryBeat.InterludeDesert, CharacterId.Lavine),
                StoryScript.GetPanelKeys(StoryBeat.InterludeDesert, CharacterId.Lavine, 2), "Only the prologue changes.");

            foreach (CharacterId hero in (CharacterId[])Enum.GetValues(typeof(CharacterId)))
            {
                foreach (string key in StoryScript.GetPanelKeys(StoryBeat.Prologue, hero, 1))
                {
                    Assert.IsNotNull(StoryTexts.GetFallback(key), $"StoryTexts has no '{key}'.");
                }

                Assert.IsNotNull(NewGamePlusTexts.GetFallback(NewGamePlusTexts.ZefaKey(hero)), $"No Vó Edna offer line for {hero}.");
            }

            Assert.IsNotNull(NewGamePlusTexts.GetFallback(NewGamePlusTexts.ZefaKey(null)));
        }

        [Test]
        public void HudRules_BadgeOnlyOnNgPlus_OfferOnlyInTheVillageWhenAvailable()
        {
            Assert.IsNull(NewGamePlusHud.BadgeText(null, 0));
            Assert.AreEqual("Jornada+2", NewGamePlusHud.BadgeText(null, 2));
            StringAssert.Contains("vida ×1,15\u20132,2", NewGamePlusHud.ResetsText(null, 1));
            StringAssert.Contains("dano ×1,45\u20132,7", NewGamePlusHud.ResetsText(null, 1));

            BiomeType village = VillageNpcCatalog.VillageRegion;
            Assert.IsTrue(NewGamePlusHud.ShouldShowOffer(true, true, true, false, false, village));
            Assert.IsFalse(NewGamePlusHud.ShouldShowOffer(false, true, true, false, false, village), "Campaign not finished.");
            Assert.IsFalse(NewGamePlusHud.ShouldShowOffer(true, true, false, false, false, village), "Token walking.");
            Assert.IsFalse(NewGamePlusHud.ShouldShowOffer(true, true, true, true, false, village), "Paused.");

            foreach (BiomeType region in (BiomeType[])Enum.GetValues(typeof(BiomeType)))
            {
                if (region != village)
                {
                    Assert.IsFalse(NewGamePlusHud.ShouldShowOffer(true, true, true, false, false, region), $"Only Vó Edna's village offers it, not {region}.");
                }
            }
        }
    }
}
