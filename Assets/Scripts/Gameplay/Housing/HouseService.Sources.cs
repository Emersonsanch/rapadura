using System;
using System.Collections.Generic;
using System.Linq;
using Rapadura.Core.DI;
using Rapadura.Core.Events;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Achievements;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Environment;
using Rapadura.Gameplay.Quests;
using UnityEngine;

namespace Rapadura.Gameplay.Housing
{
    /// <summary>
    /// H3 "Fontes" of the house service (Housing.md §7): house items from enemy deaths and boss victories
    /// (<see cref="HouseDropRules"/>), quests / dailies / achievements (<see cref="HouseRewardRules"/>) and the house
    /// workbench (<see cref="HouseCraftRules"/>). Everything lands in the Depósito through <see cref="GrantItem"/> (past the
    /// entry limit if it must — a reward is never lost and never touches the bag) and raises
    /// <see cref="HouseItemGrantedEvent"/>. One-time rewards and boss records live in <see cref="Sources"/> (saved).
    /// </summary>
    public partial class HouseService
    {
        private readonly HouseSourceBook _sources = new HouseSourceBook();
        private System.Random _sourceRandom;

        /// <summary>Boss records and claimed one-time rewards.</summary>
        public HouseSourceBook Sources => _sources;

        /// <summary>RNG of the house drops (tests inject a seeded one).</summary>
        public System.Random SourceRandom
        {
            get => _sourceRandom ??= new System.Random();
            set => _sourceRandom = value;
        }

        /// <summary>Is it night now? (tests inject; default <see cref="TimeOfDayManager.Phase"/>.)</summary>
        public Func<bool> NightProvider { get; set; }

        /// <summary>Is this quest one of the dailies? (tests inject; default <see cref="DailyQuestService.IsDailyQuest"/>.)</summary>
        public Func<string, bool> DailyQuestProvider { get; set; }

        public bool IsNight => NightProvider != null ? NightProvider() : DefaultIsNight();

        private static bool DefaultIsNight()
        {
            return ServiceLocator.TryGet(out TimeOfDayManager clock) && clock != null && clock.Phase == TimeOfDayPhase.Night;
        }

        private bool IsDailyQuest(string questId)
        {
            if (DailyQuestProvider != null) return DailyQuestProvider(questId);
            return ServiceLocator.TryGet(out DailyQuestService dailies) && dailies != null && dailies.IsDailyQuest(questId);
        }

        private void InitializeSources()
        {
            EventBus.Subscribe<EnemyDiedEvent>(OnEnemyDiedForSources);
            EventBus.Subscribe<QuestCompletedEvent>(OnQuestCompletedForSources);
            EventBus.Subscribe<AchievementUnlockedEvent>(OnAchievementUnlockedForSources);
        }

        private void ShutdownSources()
        {
            EventBus.Unsubscribe<EnemyDiedEvent>(OnEnemyDiedForSources);
            EventBus.Unsubscribe<QuestCompletedEvent>(OnQuestCompletedForSources);
            EventBus.Unsubscribe<AchievementUnlockedEvent>(OnAchievementUnlockedForSources);
        }

        // ------------------------------------------------------------------
        // Delivery
        // ------------------------------------------------------------------

        /// <summary>Delivers one house item to the Depósito (never lost) and announces it. False for an unknown id.</summary>
        public bool GrantItem(string itemId, HouseGrantReason reason, string sourceId = null)
        {
            if (Layout.Resolve(itemId) == null)
            {
                return false;
            }

            bool suppressed = _suppressEvents;
            _suppressEvents = true;
            Layout.GrantToDepot(itemId);
            _suppressEvents = suppressed;
            EventBus.Publish(new HouseItemGrantedEvent(itemId, reason, sourceId ?? string.Empty));
            RaiseChanged();
            return true;
        }

        // ------------------------------------------------------------------
        // Enemies and bosses
        // ------------------------------------------------------------------

        /// <summary>A common enemy died: maybe a house item (4%, 5% at night). The item id, or null.</summary>
        public string OnCommonKill(string enemyId)
        {
            string itemId = HouseDropRules.RollCommon(enemyId, IsNight, SourceRandom);
            if (itemId == null || !GrantItem(itemId, HouseGrantReason.EnemyDrop, enemyId))
            {
                return null;
            }

            _sources.EnemyDrops++;
            return itemId;
        }

        /// <summary>A boss victory: the trophy on the first win, then 1–2 rolls with bad-luck protection. The ids granted.</summary>
        public List<string> OnBossVictory(string bossId)
        {
            HouseBossRecord record = _sources.Boss(bossId);
            List<string> drops = HouseDropRules.RollBoss(bossId, record, SourceRandom);
            GrantBossDrops(drops, bossId);
            return drops;
        }

        private void GrantBossDrops(List<string> drops, string bossId)
        {
            HouseDropRules.BossTrophies.TryGetValue(bossId ?? string.Empty, out string trophy);
            foreach (string id in drops)
            {
                GrantItem(id, id == trophy ? HouseGrantReason.BossTrophy : HouseGrantReason.BossDrop, bossId);
            }
        }

        private void OnEnemyDiedForSources(EnemyDiedEvent evt)
        {
            if (evt.Enemy == null) return;
            EnemyController controller = evt.Enemy.GetComponent<EnemyController>();
            if (controller == null || controller.Definition == null) return;
            string enemyId = DefeatedEnemyRegistry.GetId(controller.Definition);
            List<string> granted = evt.WasBoss || controller.Definition.IsBoss ? OnBossVictory(enemyId) : ListOf(OnCommonKill(enemyId));
            ShowLootLines(evt.Enemy.transform.position, granted);
        }

        private static List<string> ListOf(string id) => id != null ? new List<string> { id } : new List<string>();

        /// <summary>The golden "+ Encomenda: …" line over the enemy (CombatFxManager), one per item.</summary>
        private static void ShowLootLines(Vector3 position, List<string> itemIds)
        {
            if (itemIds == null || itemIds.Count == 0) return;
            LocalizationManager l = ServiceLocator.TryGet(out LocalizationManager manager) ? manager : null;
            for (int i = 0; i < itemIds.Count; i++)
            {
                string text = HouseSourceTexts.Format(l, HouseSourceTexts.LootLine, HouseItemTexts.Name(l, itemIds[i]));
                Rapadura.Gameplay.Combat.CombatFxManager.SpawnLootText(position + Vector3.up * (2.6f + 0.35f * i), text);
            }
        }

        // ------------------------------------------------------------------
        // Quests, dailies, achievements
        // ------------------------------------------------------------------

        /// <summary>A quest completed: dailies give the Caixinha da Vó (+ a fixed item the first time); others their items once.</summary>
        public List<string> OnQuestCompleted(string questId)
        {
            var granted = new List<string>();
            if (string.IsNullOrEmpty(questId)) return granted;

            if (IsDailyQuest(questId))
            {
                string box = HouseRewardRules.RollDailyBox(SourceRandom);
                if (box != null && GrantItem(box, HouseGrantReason.DailyBox, questId)) granted.Add(box);
                if (_sources.Claim(HouseSourceBook.DailyFirstKey(questId)))
                {
                    foreach (string id in HouseRewardRules.DailyFirstRewards(questId))
                    {
                        if (GrantItem(id, HouseGrantReason.DailyFirst, questId)) granted.Add(id);
                    }
                }

                return granted;
            }

            List<string> rewards = HouseRewardRules.QuestRewards(questId);
            if (rewards.Count == 0 || !_sources.Claim(HouseSourceBook.QuestKey(questId))) return granted;
            foreach (string id in rewards)
            {
                if (GrantItem(id, HouseGrantReason.Quest, questId)) granted.Add(id);
            }

            return granted;
        }

        public List<string> OnAchievementUnlocked(string achievementId)
        {
            var granted = new List<string>();
            List<string> rewards = HouseRewardRules.AchievementRewards(achievementId);
            if (rewards.Count == 0 || !_sources.Claim(HouseSourceBook.AchievementKey(achievementId))) return granted;
            foreach (string id in rewards)
            {
                if (GrantItem(id, HouseGrantReason.Achievement, achievementId)) granted.Add(id);
            }

            return granted;
        }

        private void OnQuestCompletedForSources(QuestCompletedEvent evt) => OnQuestCompleted(evt.QuestId);

        private void OnAchievementUnlockedForSources(AchievementUnlockedEvent evt)
        {
            if (evt.Achievement != null) OnAchievementUnlocked(evt.Achievement.AchievementId);
        }

        // ------------------------------------------------------------------
        // Workbench
        // ------------------------------------------------------------------

        /// <summary>Is a station of <paramref name="function"/> (or exactly <paramref name="itemId"/>) placed in the house?</summary>
        public bool HasStation(HouseItemFunction function, string itemId = null)
        {
            return Layout.Placed.Any(p => string.IsNullOrEmpty(itemId) ? Layout.Resolve(p.itemId)?.Function == function : p.itemId == itemId);
        }

        public HouseCraftBlock CheckCraft(HouseCraftRecipe recipe, IHouseWallet wallet)
        {
            return HouseCraftRules.Check(recipe, HasHouse, HasStation, IsBossDefeated, wallet != null ? wallet.Count : (Func<string, int>)null);
        }

        /// <summary>
        /// Makes <paramref name="recipeId"/>: pays all or nothing, then a house item goes to the Depósito and a wardrobe
        /// entry to the wardrobe's owed list (claimed by <c>WardrobeService</c>).
        /// </summary>
        public HouseCraftBlock TryCraft(string recipeId, IHouseWallet wallet)
        {
            HouseCraftRecipe recipe = HouseCraftRules.Get(recipeId);
            HouseCraftBlock block = CheckCraft(recipe, wallet);
            if (block != HouseCraftBlock.None) return block;
            if (!HouseUpgradeRules.Pay(recipe.Costs, wallet)) return HouseCraftBlock.PaymentFailed;

            if (recipe.Kind == HouseCraftResultKind.HouseItem)
            {
                GrantItem(recipe.ResultId, HouseGrantReason.Craft, recipe.Id);
            }
            else
            {
                OweWardrobe(recipe.ResultId);
                RaiseChanged();
            }

            EventBus.Publish(new HouseRecipeCraftedEvent(recipe.Id, recipe.ResultId));
            return HouseCraftBlock.None;
        }

        // ------------------------------------------------------------------
        // Wardrobe hand-off (H5)
        // ------------------------------------------------------------------

        /// <summary>Adds a wardrobe entry (look id or dye id) to the owed list the wardrobe claims.</summary>
        public void OweWardrobe(string entryId)
        {
            if (!string.IsNullOrEmpty(entryId)) _owedWardrobe.Add(entryId);
        }

        /// <summary>Hands the owed wardrobe entries over (and clears them). The wardrobe calls this.</summary>
        public List<string> TakeOwedWardrobe()
        {
            var owed = _owedWardrobe.ToList();
            _owedWardrobe.Clear();
            if (owed.Count > 0) RaiseChanged();
            return owed;
        }

        // ------------------------------------------------------------------
        // Save hooks
        // ------------------------------------------------------------------

        private void ResetSources() => _sources.Clear();

        private void CaptureSources(HouseSaveState state) => state.sources = _sources.Capture();

        private void RestoreSources(HouseSaveState saved) => _sources.Restore(saved.sources);
    }
}
