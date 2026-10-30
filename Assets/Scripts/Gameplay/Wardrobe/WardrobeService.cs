using System;
using System.Collections.Generic;
using Rapadura.Core.DI;
using Rapadura.Core.Events;
using Rapadura.Core.Interfaces;
using Rapadura.Gameplay.Achievements;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Housing;
using Rapadura.Gameplay.Quests;
using Rapadura.Gameplay.Secrets;
using Rapadura.Gameplay.Shop;
using Rapadura.Gameplay.World.Cities;

namespace Rapadura.Gameplay.Wardrobe
{
    /// <summary>Something changed in the wardrobe (a look worn, a dye, an outfit, a new entry): the hero's look is re-applied.</summary>
    public readonly struct WardrobeChangedEvent : IGameEvent
    {
        public readonly CharacterId? Cousin;

        public WardrobeChangedEvent(CharacterId? cousin)
        {
            Cousin = cousin;
        }
    }

    /// <summary>A new look entered the Coleção, or a dye entered the pot (toast).</summary>
    public readonly struct WardrobeEntryGrantedEvent : IGameEvent
    {
        public readonly string EntryId;
        public readonly bool IsNewLook;

        public WardrobeEntryGrantedEvent(string entryId, bool isNewLook)
        {
            EntryId = entryId;
            IsNewLook = isNewLook;
        }
    }

    /// <summary>
    /// H5 "Guarda-roupa" (Housing.md §9): the account's Coleção of looks, the pot of dyes, each cousin's look and saved
    /// outfits (<see cref="WardrobeBook"/>, saved under <c>"Wardrobe"</c>), fed by the game's events through
    /// <see cref="WardrobeSourceRules"/>; exposes the outfit lines of the shops to <see cref="CityShopService.OutfitResolver"/>
    /// (<see cref="WardrobeShopBridge"/>) and turns a bought outfit into an unlock. Never touches stats or the equipment.
    /// Restores before the house (<see cref="RestoreOrder"/>) so the house's owed wardrobe rewards are claimed after both load.
    /// </summary>
    public sealed class WardrobeService : IManager, ISaveable, IResettable, IRestoreOrder
    {
        public const string Key = "Wardrobe";

        private readonly WardrobeBook _book = new WardrobeBook();
        private System.Random _random;
        private bool _subscribed;

        public string SaveKey => Key;
        public int RestoreOrder => RestoreOrders.Default - 1;
        public WardrobeBook Book => _book;

        public System.Random Random
        {
            get => _random ??= new System.Random();
            set => _random = value;
        }

        /// <summary>The cousin being played (tests inject). Default: <see cref="CharacterSelectionState.Selected"/>, else Joaquim.</summary>
        public Func<CharacterId> ActiveCousinProvider { get; set; }

        /// <summary>Is the rosewood wardrobe placed? (tests inject; default: the live house.)</summary>
        public Func<bool> RosewoodProvider { get; set; }

        public CharacterId ActiveCousin
        {
            get
            {
                if (ActiveCousinProvider != null) return ActiveCousinProvider();
                return ServiceLocator.TryGet(out CharacterSelectionState selection) && selection?.Selected != null ? selection.Selected.Value : CharacterId.Joaquim;
            }
        }

        public int OutfitSlots
        {
            get
            {
                bool rosewood = RosewoodProvider != null
                    ? RosewoodProvider()
                    : System.Linq.Enumerable.Any(HouseService.ResolveShared().Layout.Placed, p => p.itemId == WardrobeBook.RosewoodWardrobeItemId);
                return WardrobeBook.OutfitSlots(rosewood);
            }
        }

        private static WardrobeService _standalone;

        /// <summary>The GameManager's wardrobe; without one (a scene played alone) a process-wide one, not saved.</summary>
        public static WardrobeService ResolveShared()
        {
            if (ServiceLocator.TryGet(out WardrobeService wardrobe) && wardrobe != null) return wardrobe;
            return _standalone ??= new WardrobeService();
        }

        public void Initialize()
        {
            if (_subscribed) return;
            _subscribed = true;
            EventBus.Subscribe<QuestCompletedEvent>(OnQuestCompleted);
            EventBus.Subscribe<AchievementUnlockedEvent>(OnAchievementUnlocked);
            EventBus.Subscribe<RelicCollectedEvent>(OnRelicCollected);
            EventBus.Subscribe<EnemyDiedEvent>(OnEnemyDied);
            EventBus.Subscribe<EnemyLootDroppedEvent>(OnLootDropped);
            EventBus.Subscribe<ItemPurchasedEvent>(OnItemPurchased);
            EventBus.Subscribe<HouseChangedEvent>(OnHouseChanged);
            CityShopService.OutfitResolver = id => WardrobeShopBridge.Voucher(id, this);
        }

        public void Shutdown()
        {
            if (!_subscribed) return;
            _subscribed = false;
            EventBus.Unsubscribe<QuestCompletedEvent>(OnQuestCompleted);
            EventBus.Unsubscribe<AchievementUnlockedEvent>(OnAchievementUnlocked);
            EventBus.Unsubscribe<RelicCollectedEvent>(OnRelicCollected);
            EventBus.Unsubscribe<EnemyDiedEvent>(OnEnemyDied);
            EventBus.Unsubscribe<EnemyLootDroppedEvent>(OnLootDropped);
            EventBus.Unsubscribe<ItemPurchasedEvent>(OnItemPurchased);
            EventBus.Unsubscribe<HouseChangedEvent>(OnHouseChanged);
            CityShopService.OutfitResolver = null;
        }

        // ------------------------------------------------------------------
        // Gaining
        // ------------------------------------------------------------------

        /// <summary>Gives one entry (look to the Coleção, dye to the pot) and announces it. False for an unknown id or a look already owned.</summary>
        public bool Grant(string entryId, int quantity = 1)
        {
            WardrobeEntry entry = WardrobeCatalog.Get(entryId);
            if (entry == null) return false;
            bool granted = _book.Grant(entryId, quantity);
            if (!granted) return false;
            EventBus.Publish(new WardrobeEntryGrantedEvent(entryId, entry.IsLook));
            EventBus.Publish(new WardrobeChangedEvent(null));
            return true;
        }

        private List<string> GrantOnce(string key, List<string> ids)
        {
            var granted = new List<string>();
            if (ids.Count == 0 || !_book.Claim(key)) return granted;
            foreach (string id in ids)
            {
                if (Grant(id)) granted.Add(id);
            }

            return granted;
        }

        public List<string> OnQuestCompleted(string questId) => GrantOnce(WardrobeSourceRules.QuestKey(questId), WardrobeSourceRules.QuestRewards(questId));

        public List<string> OnAchievementUnlocked(string achievementId) => GrantOnce(WardrobeSourceRules.AchievementKey(achievementId), WardrobeSourceRules.AchievementRewards(achievementId));

        public List<string> OnRelicCollected(string relicId) => GrantOnce(WardrobeSourceRules.RelicKey(relicId), WardrobeSourceRules.RelicRewards(relicId));

        /// <summary>An enemy died: maybe a wardrobe drop (a boss's first win always gives one). The entry id, or null.</summary>
        public string OnEnemyDefeated(string enemyId, bool isBoss)
        {
            bool first = isBoss && _book.Claim(WardrobeSourceRules.BossKey(enemyId));
            string id = WardrobeSourceRules.RollDrop(enemyId, isBoss, first, _book.IsUnlocked, Random);
            return id != null && Grant(id) ? id : null;
        }

        /// <summary>A looted equipment piece unlocks its default look on the cousin who looted it (§9.3).</summary>
        public string OnEquipmentLooted(CharacterId cousin, Items.EquipmentSlot slot, Items.ItemRarity rarity)
        {
            string id = WardrobeSourceRules.LootLook(cousin, slot, rarity);
            return id != null && Grant(id) ? id : null;
        }

        /// <summary>Claims the looks/dyes the house holds for the wardrobe (house quests, house crafting).</summary>
        public int ClaimOwedFromHouse(HouseService house = null)
        {
            house ??= HouseService.ResolveShared();
            if (house == null || house.OwedWardrobe.Count == 0) return 0;
            int count = 0;
            foreach (string id in house.TakeOwedWardrobe())
            {
                WardrobeEntry entry = WardrobeCatalog.Get(id);
                if (entry == null) continue;
                if (Grant(id) || entry.IsLook) count++;
            }

            return count;
        }

        private void OnQuestCompleted(QuestCompletedEvent evt) => OnQuestCompleted(evt.QuestId);

        private void OnAchievementUnlocked(AchievementUnlockedEvent evt)
        {
            if (evt.Achievement != null) OnAchievementUnlocked(evt.Achievement.AchievementId);
        }

        private void OnRelicCollected(RelicCollectedEvent evt) => OnRelicCollected(evt.RelicId);

        private void OnEnemyDied(EnemyDiedEvent evt)
        {
            if (evt.Enemy == null) return;
            EnemyController controller = evt.Enemy.GetComponent<EnemyController>();
            if (controller == null || controller.Definition == null) return;
            OnEnemyDefeated(DefeatedEnemyRegistry.GetId(controller.Definition), evt.WasBoss || controller.Definition.IsBoss);
        }

        private void OnLootDropped(EnemyLootDroppedEvent evt)
        {
            if (evt.Drops == null) return;
            CharacterId cousin = ActiveCousin;
            foreach (LootDrop drop in evt.Drops)
            {
                if (drop.IsGenerated && drop.Generated.BaseItem != null)
                {
                    OnEquipmentLooted(cousin, drop.Generated.BaseItem.EquipSlot, drop.Generated.Rarity);
                }
                else if (drop.Item != null && drop.Item.IsEquippable)
                {
                    OnEquipmentLooted(cousin, drop.Item.EquipSlot, drop.Item.Rarity);
                }
            }
        }

        private void OnItemPurchased(ItemPurchasedEvent evt)
        {
            if (!WardrobeShopBridge.IsOutfitLine(evt.ShopId, evt.ItemId)) return;
            ServiceLocator.TryGet(out ShopManager shops);
            OnOutfitBought(evt.ItemId, Math.Max(1, evt.Quantity), shops?.BuyerInventory);
        }

        /// <summary>A bought outfit line: the voucher leaves the bag and the look / dye enters the wardrobe.</summary>
        public void OnOutfitBought(string entryId, int quantity, Inventory.InventoryManager buyer)
        {
            if (WardrobeCatalog.Get(entryId) == null) return;
            buyer?.RemoveById(entryId, quantity);
            Grant(entryId, quantity);
        }

        private void OnHouseChanged(HouseChangedEvent evt) => ClaimOwedFromHouse();

        // ------------------------------------------------------------------
        // Dressing (each raises WardrobeChangedEvent on success)
        // ------------------------------------------------------------------

        public WardrobeBlock SetLook(CharacterId cousin, AppearanceSlot slot, string lookId) => Changed(cousin, _book.SetLook(cousin, slot, lookId));

        public void ClearLook(CharacterId cousin, AppearanceSlot slot)
        {
            _book.ClearLook(cousin, slot);
            Changed(cousin, WardrobeBlock.None);
        }

        public void SetHidden(CharacterId cousin, AppearanceSlot slot, bool hidden)
        {
            _book.SetHidden(cousin, slot, hidden);
            Changed(cousin, WardrobeBlock.None);
        }

        public WardrobeBlock ApplyDye(CharacterId cousin, AppearanceSlot slot, DyeChannel channel, string dyeId) => Changed(cousin, _book.ApplyDye(cousin, slot, channel, dyeId));

        public void ClearDye(CharacterId cousin, AppearanceSlot slot, DyeChannel channel)
        {
            _book.ClearDye(cousin, slot, channel);
            Changed(cousin, WardrobeBlock.None);
        }

        public WardrobeBlock SaveOutfit(CharacterId cousin, int index, string name) => Changed(cousin, _book.SaveOutfit(cousin, index, name, OutfitSlots));

        public WardrobeBlock WearOutfit(CharacterId cousin, int index) => Changed(cousin, _book.WearOutfit(cousin, index, OutfitSlots));

        private static WardrobeBlock Changed(CharacterId cousin, WardrobeBlock block)
        {
            if (block == WardrobeBlock.None) EventBus.Publish(new WardrobeChangedEvent(cousin));
            return block;
        }

        // ------------------------------------------------------------------
        // Save
        // ------------------------------------------------------------------

        public object CaptureState() => _book.CaptureState();

        public void RestoreState(object state)
        {
            _book.RestoreState(state as WardrobeSaveState);
            EventBus.Publish(new WardrobeChangedEvent(null));
        }

        public void ResetForNewSession()
        {
            _book.Clear();
            EventBus.Publish(new WardrobeChangedEvent(null));
        }
    }
}
