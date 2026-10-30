using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Rapadura.Core.Events;
using Rapadura.Core.Localization;
using Rapadura.Core.Systems;
using Rapadura.Gameplay.Achievements;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.World.Biomes;
using Rapadura.Save;
using Rapadura.UI.Bestiary;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// "Caderno da Vó Zefa": unlocks and kill counts (<see cref="BestiaryProgress"/>), the once-per-save
    /// completion and its achievement, the save round-trip and saves written before the bestiary existed,
    /// the data-derived pages (<see cref="BestiaryCatalog"/>) and that every enemy asset has a page with lore
    /// and the five cousins' comments in both languages (<see cref="BestiaryTexts"/>).
    /// </summary>
    public class BestiaryTests
    {
        private const int Slot = 90061;
        private const int LegacySlot = 90062;

        private readonly List<Object> _created = new List<Object>();
        private readonly List<System.Action> _shutdowns = new List<System.Action>();
        private readonly List<string> _unlockedEvents = new List<string>();
        private int _completedEvents;

        [SetUp]
        public void SetUp()
        {
            TestSaveRoot.Enter();
            EventBus.Clear();
            // Seeded-catalog tests read Resources: never a biome table another test injected.
            BiomeRegistry.Invalidate();
            BestiaryCatalog.Invalidate();
            _unlockedEvents.Clear();
            _completedEvents = 0;
            EventBus.Subscribe<BestiaryEntryUnlockedEvent>(OnUnlocked);
            EventBus.Subscribe<BestiaryCompletedEvent>(OnCompleted);
        }

        [TearDown]
        public void TearDown()
        {
            foreach (System.Action shutdown in _shutdowns)
            {
                shutdown();
            }

            _shutdowns.Clear();
            BestiaryCatalog.Invalidate();

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

        private void OnUnlocked(BestiaryEntryUnlockedEvent evt) => _unlockedEvents.Add(evt.EnemyId);

        private void OnCompleted(BestiaryCompletedEvent evt) => _completedEvents++;

        private static readonly string[] TwoPages = { "enemy_a", "enemy_b" };

        private BestiaryProgress NewProgress(DefeatedEnemyRegistry registry = null, IReadOnlyList<string> ids = null)
        {
            IReadOnlyList<string> catalog = ids ?? TwoPages;
            var progress = new BestiaryProgress(registry, () => catalog);
            progress.Initialize();
            _shutdowns.Add(progress.Shutdown);
            return progress;
        }

        private static void SetField(object target, string fieldName, object value)
        {
            FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(field, $"Field '{fieldName}' not found on {target.GetType().Name}.");
            field.SetValue(target, value);
        }

        private EnemyDefinition MakeEnemy(string id, bool isBoss = false)
        {
            var enemy = ScriptableObject.CreateInstance<EnemyDefinition>();
            enemy.name = id;
            SetField(enemy, "_isBoss", isBoss);
            _created.Add(enemy);
            return enemy;
        }

        // ------------------------------------------------------------------
        // Unlocks and kill counts
        // ------------------------------------------------------------------

        [Test]
        public void RecordKill_FirstDefeatUnlocks_LaterOnesOnlyCount()
        {
            BestiaryProgress progress = NewProgress();
            Assert.IsFalse(progress.IsUnlocked("enemy_a"));

            Assert.IsTrue(progress.RecordKill("enemy_a"));
            Assert.IsFalse(progress.RecordKill("enemy_a"));
            Assert.IsFalse(progress.RecordKill("enemy_a"));

            Assert.IsTrue(progress.IsUnlocked("enemy_a"));
            Assert.AreEqual(3, progress.GetKills("enemy_a"));
            Assert.AreEqual(0, progress.GetKills("enemy_b"));
            CollectionAssert.AreEqual(new[] { "enemy_a" }, _unlockedEvents, "Unlock event fires only on the first defeat.");
            Assert.AreEqual(1, progress.UnlockedCount);
            Assert.AreEqual(2, progress.TotalCount);

            Assert.IsFalse(progress.RecordKill(null));
            Assert.IsFalse(progress.RecordKill(string.Empty));
        }

        [Test]
        public void EnemyDiedEvent_CountsTheKillUnderTheAssetName()
        {
            BestiaryProgress progress = NewProgress();
            EnemyDefinition wolf = MakeEnemy("enemy_a");

            var enemyObject = new GameObject("Enemy_a");
            _created.Add(enemyObject);
            EnemyController controller = enemyObject.AddComponent<EnemyController>();
            SetField(controller, "_definition", wolf);

            EventBus.Publish(new EnemyDiedEvent(enemyObject, false));
            EventBus.Publish(new EnemyDiedEvent(enemyObject, false));
            EventBus.Publish(new EnemyDiedEvent(null, false));

            Assert.AreEqual(2, progress.GetKills("enemy_a"));
        }

        [Test]
        public void LastPage_RaisesCompletedOnce()
        {
            BestiaryProgress progress = NewProgress();

            progress.RecordKill("enemy_a");
            Assert.AreEqual(0, _completedEvents);
            Assert.IsFalse(progress.Completed);

            progress.RecordKill("enemy_b");
            progress.RecordKill("enemy_b");
            progress.RecordKill("not_in_catalog");

            Assert.AreEqual(1, _completedEvents);
            Assert.IsTrue(progress.Completed);
            Assert.IsTrue(progress.AllUnlocked);
            Assert.IsFalse(progress.CheckCompleted(), "Already completed in this save.");
        }

        [Test]
        public void UnknownIds_DoNotCountTowardsCompletion()
        {
            BestiaryProgress progress = NewProgress();
            progress.RecordKill("enemy_a");
            progress.RecordKill("removed_enemy");

            Assert.AreEqual(1, progress.UnlockedCount);
            Assert.IsFalse(progress.AllUnlocked);
            Assert.AreEqual(0, _completedEvents);
        }

        [Test]
        public void EmptyCatalog_IsNeverComplete()
        {
            BestiaryProgress progress = NewProgress(ids: new string[0]);
            progress.RecordKill("enemy_a");
            Assert.IsFalse(progress.AllUnlocked);
            Assert.AreEqual(0, _completedEvents);
        }

        [Test]
        public void BestiaryCompleted_UnlocksTheAchievement()
        {
            var definition = ScriptableObject.CreateInstance<AchievementDefinition>();
            _created.Add(definition);
            definition.ConfigureForTests("ach_test_bestiary", AchievementConditionType.BestiaryCompleted, 1);

            var achievements = new AchievementManager(new[] { definition });
            achievements.Initialize();
            _shutdowns.Add(achievements.Shutdown);

            BestiaryProgress progress = NewProgress();
            progress.RecordKill("enemy_a");
            Assert.IsFalse(achievements.IsUnlocked("ach_test_bestiary"));

            progress.RecordKill("enemy_b");
            Assert.IsTrue(achievements.IsUnlocked("ach_test_bestiary"));
        }

        [Test]
        public void SeededCatalog_HasTheBestiaryAchievement()
        {
            AchievementDefinition[] seeded = Resources.LoadAll<AchievementDefinition>(AchievementManager.ResourcesFolder);
            AchievementDefinition bestiary = seeded.FirstOrDefault(d => d.AchievementId == "ach_bestiary_complete");

            Assert.IsNotNull(bestiary, "Resources/Achievements/ach_bestiary_complete.asset is missing.");
            Assert.AreEqual(AchievementConditionType.BestiaryCompleted, bestiary.ConditionType);
            Assert.AreEqual(1, bestiary.TargetValue);
        }

        // ------------------------------------------------------------------
        // Persistence
        // ------------------------------------------------------------------

        [Test]
        public void CaptureRestore_RoundTripsThroughJson()
        {
            BestiaryProgress progress = NewProgress();
            progress.RecordKill("enemy_a");
            progress.RecordKill("enemy_a");
            progress.RecordKill("enemy_b");

            string json = JsonUtility.ToJson(progress.CaptureState());
            BestiaryProgress restored = NewProgress();
            restored.RestoreState(JsonUtility.FromJson<BestiarySaveState>(json));

            Assert.AreEqual(2, restored.GetKills("enemy_a"));
            Assert.AreEqual(1, restored.GetKills("enemy_b"));
            Assert.IsTrue(restored.Completed, "The completion flag is saved, so it never fires twice.");
            StringAssert.Contains("enemy_a", json);
        }

        [Test]
        public void RestoreState_IgnoresGarbage()
        {
            BestiaryProgress progress = NewProgress();
            progress.RestoreState(new BestiarySaveState
            {
                entries = new List<BestiaryKillRecord>
                {
                    null,
                    new BestiaryKillRecord { enemyId = null, kills = 4 },
                    new BestiaryKillRecord { enemyId = "enemy_a", kills = 0 },
                    new BestiaryKillRecord { enemyId = "enemy_b", kills = 3 },
                    new BestiaryKillRecord { enemyId = "enemy_b", kills = 5 },
                },
            });

            Assert.AreEqual(1, progress.GetKills("enemy_a"), "A page saved with 0 still counts as seen once.");
            Assert.AreEqual(5, progress.GetKills("enemy_b"));

            progress.RestoreState(new BestiarySaveState { entries = null });
            Assert.IsEmpty(progress.UnlockedIds);
            Assert.DoesNotThrow(() => progress.RestoreState("not a bestiary state"));
        }

        [Test]
        public void SaveAndLoad_ThroughSaveManager_KeepsPagesAndKills()
        {
            BestiaryProgress writer = NewProgress();
            var saver = new SaveManager();
            saver.Initialize();
            saver.Register(writer);
            writer.RecordKill("enemy_a");
            writer.RecordKill("enemy_a");
            saver.Save(Slot);
            Assume.That(File.Exists(TestSaveRoot.SlotPath(Slot)));

            BestiaryProgress reader = LoadInto(Slot, null);
            Assert.AreEqual(2, reader.GetKills("enemy_a"));
            Assert.IsFalse(reader.IsUnlocked("enemy_b"));
        }

        [Test]
        public void LegacySave_LoadsAnEmptyNotebook_ButKeepsTheBossesAlreadyBeaten()
        {
            // A save from before the bestiary: only the defeated-boss registry is in it.
            var legacyRegistry = new DefeatedEnemyRegistry();
            legacyRegistry.MarkDefeated("enemy_b");
            var saver = new SaveManager();
            saver.Initialize();
            saver.Register(legacyRegistry);
            saver.Save(LegacySlot);

            var registry = new DefeatedEnemyRegistry();
            BestiaryProgress progress = NewProgress(registry);
            progress.RecordKill("enemy_a"); // from the previous session: must be forgotten by the load
            _unlockedEvents.Clear();

            LoadInto(LegacySlot, registry, progress);

            Assert.IsFalse(progress.IsUnlocked("enemy_a"), "Nothing leaks from the previous session.");
            Assert.IsTrue(progress.IsUnlocked("enemy_b"), "Bosses never respawn: the ones beaten back then are written down.");
            Assert.AreEqual(1, progress.GetKills("enemy_b"));
            Assert.AreEqual(0, _completedEvents);
        }

        [Test]
        public void NewSessionReset_EmptiesTheNotebook()
        {
            BestiaryProgress progress = NewProgress();
            progress.RecordKill("enemy_a");
            progress.RecordKill("enemy_b");

            var reset = new NewSessionReset();
            reset.Register(progress);
            reset.ResetAll();

            Assert.IsEmpty(progress.UnlockedIds);
            Assert.IsFalse(progress.Completed);
        }

        private BestiaryProgress LoadInto(int slot, DefeatedEnemyRegistry registry, BestiaryProgress progress = null)
        {
            progress ??= NewProgress(registry);
            var reset = new NewSessionReset();
            reset.Register(progress);

            var loader = new SaveManager { SessionReset = reset };
            loader.Initialize();
            if (registry != null)
            {
                reset.Register(registry);
                loader.Register(registry);
            }

            loader.Register(progress);
            Assert.IsTrue(loader.Load(slot));
            return progress;
        }

        // ------------------------------------------------------------------
        // Catalog (real data)
        // ------------------------------------------------------------------

        [Test]
        public void EveryEnemyAsset_HasAPage_WithLoreAndFiveCommentsInBothLanguages()
        {
            EnemyDefinition[] enemies = Resources.LoadAll<EnemyDefinition>("Enemies");
            Assert.IsNotEmpty(enemies);
            Dictionary<string, LocalizationEntry> table = BestiaryTexts.Entries.ToDictionary(e => e.Key);
            var problems = new List<string>();

            foreach (EnemyDefinition enemy in enemies)
            {
                string id = DefeatedEnemyRegistry.GetId(enemy);
                if (!BestiaryCatalog.Contains(id))
                {
                    problems.Add($"{id}: no bestiary page");
                }

                var keys = new List<string> { BestiaryCatalog.LoreKey(id) };
                keys.AddRange(BestiaryTexts.Cousins.Select(hero => BestiaryCatalog.CommentKey(id, hero)));
                foreach (string key in keys)
                {
                    if (!table.TryGetValue(key, out LocalizationEntry entry))
                    {
                        problems.Add($"{key}: missing");
                    }
                    else if (string.IsNullOrWhiteSpace(entry.En) || string.IsNullOrWhiteSpace(entry.PtBR))
                    {
                        problems.Add($"{key}: missing a language");
                    }
                }
            }

            Assert.AreEqual(enemies.Length, BestiaryCatalog.Count, "One page per enemy asset.");
            CollectionAssert.IsEmpty(problems, string.Join("\n", problems));
        }

        [Test]
        public void Cousins_AreTheFiveHeroes_InBirthOrder()
        {
            CollectionAssert.AreEqual(
                new[] { CharacterId.Maithe, CharacterId.Maria, CharacterId.Joaquim, CharacterId.Icaro, CharacterId.Lavine },
                BestiaryTexts.Cousins);
            CollectionAssert.AreEquivalent(System.Enum.GetValues(typeof(CharacterId)), BestiaryTexts.Cousins);
        }

        [Test]
        public void NoLoreOrComment_ForAnEnemyThatDoesNotExist()
        {
            var ids = new HashSet<string>(Resources.LoadAll<EnemyDefinition>("Enemies").Select(DefeatedEnemyRegistry.GetId));
            var orphans = BestiaryTexts.Entries
                .Select(e => e.Key)
                .Where(k => k.StartsWith("bestiary.", System.StringComparison.Ordinal))
                .Select(k => k.Split('.')[1])
                .Where(id => !ids.Contains(id))
                .Distinct()
                .ToList();

            CollectionAssert.IsEmpty(orphans, "Bestiary texts for unknown enemies: " + string.Join(", ", orphans));
        }

        [Test]
        public void SeededPages_AreInCampaignOrder_RegularsBeforeTheBoss()
        {
            IReadOnlyList<BestiaryEntry> entries = BestiaryCatalog.Entries;
            for (int i = 1; i < entries.Count; i++)
            {
                Assert.LessOrEqual(BestiaryCatalog.ComparePages(entries[i - 1], entries[i]), 0, $"{entries[i - 1].Id} should not come after {entries[i].Id}");
            }

            // One boss per region, each the last page of its region.
            foreach (IGrouping<BiomeType?, BestiaryEntry> region in entries.GroupBy(e => e.Region))
            {
                Assert.IsNotNull(region.Key, $"{region.First().Id} is in no biome spawn table.");
                Assert.AreEqual(1, region.Count(e => e.IsBoss), $"{region.Key} should have exactly one boss page.");
                Assert.IsTrue(region.Last().IsBoss, $"{region.Key}: the boss page comes last.");
            }
        }

        [Test]
        public void Tips_ComeFromTheEnemyData()
        {
            EnemyDefinition boss = MakeEnemy("enemy_boss", isBoss: true);
            SetField(boss, "_motionArchetype", EnemyMotionArchetype.Heavy);
            SetField(boss, "_attackCooldown", 3f);
            SetField(boss, "_attackDamage", 40f);
            SetField(boss, "_fleeHealthThreshold", 0f);

            EnemyDefinition runner = MakeEnemy("enemy_runner");
            SetField(runner, "_motionArchetype", EnemyMotionArchetype.Quadruped);
            SetField(runner, "_attackCooldown", 0.8f);
            SetField(runner, "_attackDamage", 7f);
            SetField(runner, "_fleeHealthThreshold", 0.2f);

            BestiaryEntry bossPage = BestiaryCatalog.Build(boss, null);
            BestiaryEntry runnerPage = BestiaryCatalog.Build(runner, null);
            List<BestiaryTipKind> bossTips = bossPage.Tips.Select(t => t.Kind).ToList();
            List<BestiaryTipKind> runnerTips = runnerPage.Tips.Select(t => t.Kind).ToList();

            Assert.AreEqual(BestiaryTipKind.Heavy, bossTips[0]);
            CollectionAssert.Contains(bossTips, BestiaryTipKind.HeavyHitter);
            CollectionAssert.Contains(bossTips, BestiaryTipKind.StunResistant);
            CollectionAssert.DoesNotContain(bossTips, BestiaryTipKind.Flees);
            Assert.AreEqual(0.5f, bossPage.Tips.First(t => t.Kind == BestiaryTipKind.StunResistant).Value, 1e-4f, "Bosses take StunRules.BossStunDurationMultiplier.");

            Assert.AreEqual(BestiaryTipKind.PackRunner, runnerTips[0]);
            CollectionAssert.Contains(runnerTips, BestiaryTipKind.RapidAttacks);
            CollectionAssert.Contains(runnerTips, BestiaryTipKind.Flees);
            CollectionAssert.DoesNotContain(runnerTips, BestiaryTipKind.StunResistant);
            CollectionAssert.DoesNotContain(runnerTips, BestiaryTipKind.HeavyHitter);

            float expectedWindup = EnemyMotionMath.ResolveWindup(EnemyMotionProfile.Heavy.BaseWindup, 3f);
            Assert.AreEqual(expectedWindup, bossPage.Tips.First(t => t.Kind == BestiaryTipKind.Telegraph).Value, 1e-4f);
        }

        [Test]
        public void Drops_ComeFromTheBiomeTable_WithRealChances()
        {
            var herb = ScriptableObject.CreateInstance<ItemDefinition>();
            herb.name = "herb";
            _created.Add(herb);
            var gem = ScriptableObject.CreateInstance<ItemDefinition>();
            gem.name = "gem";
            _created.Add(gem);

            EnemyDefinition enemy = MakeEnemy("enemy_a");
            var biome = ScriptableObject.CreateInstance<BiomeDefinition>();
            _created.Add(biome);
            biome.SetDataForTests(BiomeType.Forest, "biome.forest.name", string.Empty, 15f, 25f, string.Empty,
                new List<BiomeEnemySpawnEntry> { new BiomeEnemySpawnEntry(enemy, 1) },
                new List<BiomeLootEntry> { new BiomeLootEntry(herb, 9), new BiomeLootEntry(gem, 1) });
            biome.SetLootRollsForTests(0.5f, 2, 4);

            List<BestiaryDrop> drops = BestiaryCatalog.BuildDrops(enemy, biome);
            Assert.AreEqual(2, drops.Count, "No base loot item on this definition: only the table.");
            Assert.AreSame(herb, drops[0].Item, "Most likely first.");

            // herb: 2 rolls at 0.5 × 9/10 each → 1 - 0.55².
            Assert.AreEqual(1f - 0.55f * 0.55f, drops[0].ChancePerDefeat, 1e-4f);
            Assert.AreEqual(BestiaryDropFrequency.Common, drops[0].Frequency);
            Assert.AreEqual(1f - 0.95f * 0.95f, drops[1].ChancePerDefeat, 1e-4f);
            Assert.AreEqual(BestiaryDropFrequency.Rare, drops[1].Frequency);

            SetField(enemy, "_isBoss", true);
            List<BestiaryDrop> bossDrops = BestiaryCatalog.BuildDrops(enemy, biome);
            // Boss: 4 guaranteed rolls → herb 1 - 0.1⁴.
            Assert.AreEqual(1f - Mathf.Pow(0.1f, 4), bossDrops[0].ChancePerDefeat, 1e-4f);
            Assert.AreEqual(BestiaryDropFrequency.Guaranteed, bossDrops[0].Frequency);
        }

        [Test]
        public void SeededPages_ListTheBiomeMaterials()
        {
            foreach (BestiaryEntry entry in BestiaryCatalog.Entries)
            {
                BiomeDefinition biome = BiomeRegistry.FindBiomeOf(entry.Definition);
                Assert.IsNotNull(biome, entry.Id);
                foreach (BiomeLootEntry loot in biome.Loot.Where(l => l?.Item != null && l.Weight > 0))
                {
                    Assert.IsTrue(entry.Drops.Any(d => d.Item == loot.Item), $"{entry.Id} does not list {loot.Item.ItemId}");
                }

                if (BossBonusRules.HasBonusDrop(entry.Definition))
                {
                    Assert.IsTrue(entry.Drops.Any(d => d.IsBossBonus && d.Item == entry.Definition.BossBonusDropItem), $"{entry.Id} does not list its bonus piece");
                }
            }
        }

        // ------------------------------------------------------------------
        // Presenter
        // ------------------------------------------------------------------

        [Test]
        public void LockedPage_ShowsOnlyTheSilhouette()
        {
            BestiaryEntry page = BestiaryCatalog.Build(MakeEnemy("enemy_forest_wolf"), null);
            BestiaryPageView view = BestiaryPresenter.BuildPage(null, page, unlocked: false, kills: 0, hero: CharacterId.Maithe);

            Assert.IsTrue(view.Locked);
            Assert.AreEqual("???", view.Name);
            Assert.IsEmpty(view.Tips);
            Assert.IsEmpty(view.Drops);
            Assert.IsEmpty(view.Comment);
            Assert.AreEqual("???", BestiaryPresenter.ListLabel(null, page, unlocked: false));
        }

        [Test]
        public void UnlockedPage_ShowsLoreKillsAndTheSelectedCousinsComment()
        {
            BestiaryEntry page = BestiaryCatalog.Get("enemy_forest_wolf");
            Assume.That(page, Is.Not.Null);

            BestiaryPageView view = BestiaryPresenter.BuildPage(null, page, unlocked: true, kills: 3, hero: CharacterId.Lavine);

            Assert.IsFalse(view.Locked);
            Assert.AreEqual(BestiaryTexts.GetFallback(page.LoreKey), view.Lore);
            Assert.AreEqual(BestiaryTexts.GetFallback(page.CommentKey(CharacterId.Lavine)), view.Comment);
            StringAssert.Contains("3", view.Kills);
            Assert.IsNotEmpty(view.Tips);
            Assert.IsNotEmpty(view.Drops);
            Assert.AreEqual(CharacterId.Lavine, view.Hero);
        }

        [Test]
        public void Decimal_UsesTheLanguageSeparator()
        {
            Assert.AreEqual("0,4", BestiaryPresenter.Decimal(null, 0.42f));
            var english = new LocalizationManager();
            english.SetLanguage(LanguageCode.en);
            Assert.AreEqual("0.4", BestiaryPresenter.Decimal(english, 0.42f));
        }

        [Test]
        public void DefaultSelection_IsTheFirstUnlockedPage()
        {
            List<BestiaryEntry> entries = BestiaryCatalog.BuildAll(new[] { MakeEnemy("enemy_a"), MakeEnemy("enemy_b") }, _ => null);
            Assert.AreEqual("enemy_a", BestiaryController.DefaultSelection(entries, _ => false));
            Assert.AreEqual("enemy_b", BestiaryController.DefaultSelection(entries, id => id == "enemy_b"));
            Assert.IsNull(BestiaryController.DefaultSelection(new List<BestiaryEntry>(), _ => true));
        }

        [Test]
        public void MapButton_IgnoredWhileSomethingElseHoldsThePause()
        {
            Assert.IsTrue(BestiaryController.CanToggleFromMap(notebookOpen: false, pausedByOthers: false));
            Assert.IsFalse(BestiaryController.CanToggleFromMap(notebookOpen: false, pausedByOthers: true));
            Assert.IsTrue(BestiaryController.CanToggleFromMap(notebookOpen: true, pausedByOthers: true));
        }
    }
}
