using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rapadura.Core.Events;
using Rapadura.Core.Managers;
using Rapadura.Gameplay.Achievements;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Combat;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Inventory;
using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.Quests;
using Rapadura.Gameplay.Skills;
using Rapadura.Gameplay.World.Map;
using Rapadura.Save;
using Rapadura.UI.HUD;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Rapadura.Tests.PlayMode
{
    /// <summary>
    /// End-to-end smoke tests against the real Main.unity: catch the integration bugs EditMode
    /// tests can't (a system never placed in the scene, a manager never registered, a flow whose
    /// pieces each work in isolation but are never connected). Every test loads the scene fresh;
    /// any unexpected error/exception log fails it (Unity Test Framework's LogAssert default).
    /// </summary>
    [Category("Smoke")]
    public class MainSceneSmokeTests : SmokeTestBase
    {
        private const CharacterId TestHero = CharacterId.Icaro;
        private const CharacterId OtherHero = CharacterId.Maria;
        // Far outside any slot the UI offers; SmokeTestBase's save-folder restore also deletes it.
        private const int TestSaveSlot = 90071;

        // ------------------------------------------------------------------
        // 1. Scene boots cleanly
        // ------------------------------------------------------------------

        [UnityTest]
        [Timeout(120000)]
        public IEnumerator Main_Loads_WithServicesHudAndPlayerComponents()
        {
            yield return LoadScene(MainSceneName);

            Assert.IsNotNull(GameManager.Instance, "GameManager missing from Main.unity.");

            GetService<SaveManager>();
            GetService<QuestManager>();
            GetService<DailyQuestService>();
            GetService<AchievementManager>();
            GetService<CharacterSelectionState>();
            GetService<WorldMapProgress>();
            GetService<SceneTransitionService>();
            GetService<DefeatedEnemyRegistry>();
            GetService<Rapadura.Gameplay.Crafting.CraftingManager>();
            GetService<Rapadura.Gameplay.Shop.ShopManager>();
            GetService<Rapadura.Gameplay.Dialogue.DialogueManager>();
            GetService<Rapadura.Core.Localization.LocalizationManager>();
            GetService<Rapadura.Core.Audio.AudioManager>();

            HudController hud = FindInScene<HudController>();
            Assert.IsNotNull(hud, "Main.unity has no HudController.");
            Assert.IsTrue(hud.isActiveAndEnabled, "HudController is disabled/inactive.");
            UIDocument hudDocument = hud.GetComponent<UIDocument>();
            Assert.IsNotNull(hudDocument, "HUD has no UIDocument.");
            Assert.IsNotNull(hudDocument.rootVisualElement, "HUD UIDocument built no visual tree.");
            Assert.Greater(hudDocument.rootVisualElement.childCount, 0, "HUD UIDocument visual tree is empty (missing UXML?).");

            GameObject player = FindPlayer();
            Assert.IsNotNull(player.GetComponent<PlayerStats>(), "Player has no PlayerStats.");
            Assert.IsNotNull(player.GetComponent<SkillManager>(), "Player has no SkillManager.");
            Assert.IsNotNull(player.GetComponent<SkillHotbar>(), "Player has no SkillHotbar.");
            Assert.IsNotNull(player.GetComponent<InventoryManager>(), "Player has no InventoryManager.");

            yield return WaitFrames(30);
            LogAssert.NoUnexpectedReceived();
        }

        // ------------------------------------------------------------------
        // 2. Hero selection → hotbar → cast
        // ------------------------------------------------------------------

        [UnityTest]
        [Timeout(120000)]
        public IEnumerator SelectingHero_FillsHotbarWithActives_AndCastingSlot0SpendsManaAndStartsCooldown()
        {
            yield return LoadScene(MainSceneName);
            yield return SelectHero(TestHero);

            GameObject player = FindPlayer();
            SkillHotbar hotbar = player.GetComponent<SkillHotbar>();
            PlayerStats stats = player.GetComponent<PlayerStats>();

            List<SkillDefinition> expectedActives = CharacterSkillSet.GetSkills(TestHero)
                .Where(s => s != null && s.SkillType == SkillType.Active)
                .Take(4)
                .ToList();
            Assert.AreEqual(4, expectedActives.Count, $"{TestHero} should ship 4 active skills under Resources/{CharacterSkillSet.GetResourcesFolder(TestHero)}.");

            yield return WaitUntilOrFail(() => hotbar.GetSlotSkill(0) != null, "hotbar slot 0 to be filled after picking a hero");

            for (int i = 0; i < expectedActives.Count; i++)
            {
                SkillInstance slot = hotbar.GetSlotSkill(i);
                Assert.IsNotNull(slot, $"Hotbar slot {i} is empty after picking {TestHero}.");
                Assert.AreEqual(expectedActives[i].SkillId, slot.Definition.SkillId, $"Hotbar slot {i} holds the wrong skill.");
            }

            SkillInstance first = hotbar.GetSlotSkill(0);
            Assert.IsFalse(first.IsOnCooldown, "Slot 0 should not start on cooldown.");

            var casts = new List<SkillCastEvent>();
            System.Action<SkillCastEvent> onCast = e => casts.Add(e);
            EventBus.Subscribe(onCast);

            try
            {
                float manaBefore = stats.CurrentMana;
                Assert.GreaterOrEqual(manaBefore, first.Definition.ManaCost, "A fresh hero can't afford their own first skill.");

                bool cast = hotbar.TryCastSlot(0);

                Assert.IsTrue(cast, "TryCastSlot(0) failed for a fresh hero with full mana.");
                Assert.IsTrue(first.IsOnCooldown || first.Definition.CooldownSeconds <= 0f, "Cooldown did not start after casting.");
                if (first.Definition.ManaCost > 0f)
                {
                    Assert.Less(stats.CurrentMana, manaBefore, "Mana did not drop after casting a skill with a mana cost.");
                }

                Assert.AreEqual(1, casts.Count, "Exactly one SkillCastEvent should be published per cast.");
                Assert.AreEqual(player, casts[0].Caster);
                Assert.AreEqual(first.Definition, casts[0].Skill);

                Assert.IsFalse(hotbar.TryCastSlot(0), "Casting again while on cooldown must fail.");
            }
            finally
            {
                EventBus.Unsubscribe(onCast);
            }

            yield return WaitFrames(10);
        }

        // ------------------------------------------------------------------
        // 3. Killing an enemy
        // ------------------------------------------------------------------

        [UnityTest]
        [Timeout(120000)]
        public IEnumerator KillingEnemy_PublishesDied_GrantsXp_AndCountsAchievement()
        {
            yield return LoadScene(MainSceneName);
            yield return SelectHero(TestHero);

            GameObject player = FindPlayer();
            PlayerStats stats = player.GetComponent<PlayerStats>();
            InventoryManager inventory = player.GetComponent<InventoryManager>();
            AchievementManager achievements = GetService<AchievementManager>();

            EnemyController enemy = Object.FindObjectsByType<EnemyController>(FindObjectsSortMode.None)
                .Where(e => e.isActiveAndEnabled && e.Definition != null && e.Health != null && e.Health.IsAlive)
                .OrderByDescending(e => e.Definition.ExperienceReward)
                .FirstOrDefault();
            Assert.IsNotNull(enemy, "Main.unity has no active, alive EnemyController with an EnemyDefinition.");

            EnemyDefinition definition = enemy.Definition;
            int levelBefore = stats.Level;
            int xpBefore = stats.CurrentExperience;
            long killsBefore = achievements.GetStat(AchievementConditionType.EnemiesKilled);
            int lootBefore = definition.LootItem != null ? inventory.GetTotalCount(definition.LootItem.ItemId) : 0;

            var deaths = new List<EnemyDiedEvent>();
            System.Action<EnemyDiedEvent> onDied = e => deaths.Add(e);
            EventBus.Subscribe(onDied);

            try
            {
                enemy.Health.ClearInvulnerability();
                enemy.Health.ApplyDamage(1_000_000f, ElementType.Physical, player);

                Assert.IsTrue(enemy.Health.IsDead, "Enemy survived 1,000,000 damage.");
                Assert.AreEqual(1, deaths.Count(d => d.Enemy == enemy.gameObject), "EnemyDiedEvent was not published exactly once for the killed enemy.");
            }
            finally
            {
                EventBus.Unsubscribe(onDied);
            }

            yield return WaitFrames(3);

            if (definition.ExperienceReward > 0)
            {
                bool gained = stats.Level > levelBefore || stats.CurrentExperience > xpBefore;
                Assert.IsTrue(gained, $"Killing '{definition.name}' ({definition.ExperienceReward} XP) granted no experience.");
            }

            Assert.AreEqual(killsBefore + 1, achievements.GetStat(AchievementConditionType.EnemiesKilled), "AchievementManager did not count the kill.");

            if (definition.LootItem != null && definition.LootDropChance >= 1f && definition.LootQuantityMin > 0)
            {
                Assert.Greater(inventory.GetTotalCount(definition.LootItem.ItemId), lootBefore, $"Guaranteed loot '{definition.LootItem.ItemId}' did not reach the player's inventory.");
            }
        }

        // ------------------------------------------------------------------
        // 4. Save / load round trip
        // ------------------------------------------------------------------

        [UnityTest]
        [Timeout(180000)]
        public IEnumerator SaveThenLoad_RestoresHeroInventoryQuestAndWorldMapProgress()
        {
            yield return LoadScene(MainSceneName);
            yield return SelectHero(TestHero);
            yield return WaitFrames(10);

            SaveManager saveManager = GetService<SaveManager>();
            CharacterSelectionState selection = GetService<CharacterSelectionState>();
            QuestManager quests = GetService<QuestManager>();
            WorldMapProgress progress = GetService<WorldMapProgress>();

            GameObject player = FindPlayer();
            InventoryManager inventory = player.GetComponent<InventoryManager>();
            SkillManager skills = player.GetComponent<SkillManager>();

            saveManager.DeleteSlot(TestSaveSlot);

            // --- Arrange a recognisable state -------------------------------------------------
            var item = Rapadura.Gameplay.Items.ItemDatabase.GetAll()
                .FirstOrDefault(i => i != null && !i.IsEquippable && i.IsStackable && inventory.HasWeightCapacityFor(i, 3));
            Assert.IsNotNull(item, "No stackable, non-equippable item under Resources/Items to put in the inventory.");
            int countBefore = inventory.GetTotalCount(item.ItemId);
            inventory.AddItem(item, 3);
            int savedItemCount = inventory.GetTotalCount(item.ItemId);
            Assert.Greater(savedItemCount, countBefore, $"InventoryManager.AddItem did not add '{item.ItemId}'.");

            QuestDefinition quest = Resources.LoadAll<QuestDefinition>("Quests")
                .FirstOrDefault(q => q != null && !string.IsNullOrEmpty(q.QuestId) && q.QuestType != QuestType.Daily
                                     && !quests.IsActive(q.QuestId) && !quests.IsCompleted(q.QuestId) && quests.ArePrerequisitesMet(q));
            Assert.IsNotNull(quest, "No startable (non-daily, no prerequisites) quest under Resources/Quests.");
            Assert.IsTrue(quests.StartQuest(quest), $"QuestManager.StartQuest('{quest.QuestId}') failed.");

            progress.RecordVictory(progress.CurrentRegion);
            progress.RecordVictory(progress.CurrentRegion);
            var region = progress.CurrentRegion;
            int savedVictories = progress.GetVictories(region);

            string heroSkillId = CharacterSkillSet.GetSkills(TestHero).First(s => s.SkillType == SkillType.Active).SkillId;
            Assert.IsTrue(skills.IsSkillLearned(heroSkillId), "Hero's first active skill should be learned before saving.");

            // --- Save ------------------------------------------------------------------------
            saveManager.Save(TestSaveSlot);
            Assert.IsTrue(saveManager.SlotExists(TestSaveSlot), $"Save({TestSaveSlot}) wrote no file.");

            // --- Mutate ----------------------------------------------------------------------
            selection.Select(OtherHero);
            inventory.RemoveById(item.ItemId, inventory.GetTotalCount(item.ItemId));
            quests.AbandonQuest(quest.QuestId);
            progress.ResetToNewGame();

            Assert.AreEqual(OtherHero, selection.Selected);
            Assert.AreEqual(0, inventory.GetTotalCount(item.ItemId));
            Assert.IsFalse(quests.IsActive(quest.QuestId));
            Assert.AreEqual(0, progress.GetVictories(region));

            // --- Load ------------------------------------------------------------------------
            Assert.IsTrue(saveManager.Load(TestSaveSlot), $"Load({TestSaveSlot}) failed.");
            yield return WaitFrames(5);

            Assert.AreEqual(TestHero, selection.Selected, "Chosen hero was not restored.");
            Assert.AreEqual(savedItemCount, inventory.GetTotalCount(item.ItemId), "Inventory was not restored.");
            Assert.IsTrue(quests.IsActive(quest.QuestId), "Active quest was not restored.");
            Assert.AreEqual(savedVictories, progress.GetVictories(region), "World map victories were not restored.");
            Assert.IsTrue(skills.IsSkillLearned(heroSkillId), "Hero's learned skills were not restored.");

            SkillHotbar hotbar = player.GetComponent<SkillHotbar>();
            Assert.IsNotNull(hotbar.GetSlotSkill(0), "Hotbar slot 0 is empty after loading.");

            saveManager.DeleteSlot(TestSaveSlot);
            Assert.IsFalse(saveManager.SlotExists(TestSaveSlot));
        }
    }
}
