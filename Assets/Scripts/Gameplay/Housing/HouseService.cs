using System;
using Rapadura.Core.DI;
using Rapadura.Core.Events;
using Rapadura.Core.Interfaces;
using Rapadura.Gameplay.Inventory;
using Rapadura.Gameplay.World.Biomes;
using Rapadura.Gameplay.World.Map;

namespace Rapadura.Gameplay.Housing
{
    /// <summary>
    /// Save slice <c>"House"</c> (Housing.md §12.2). One house per account (user decision 2026-10-29): offline the
    /// save is the account, so the house belongs to the save and is shared by the five cousins — switching hero
    /// never changes it, and nothing here is per character.
    /// </summary>
    [Serializable]
    public class HouseSaveState
    {
        public int version = HouseService.SaveVersion;
        public string ownerAccountId = HouseRules.LocalAccountId;
        public string cityId = HouseService.DefaultCityId;
        public int level = 1;
        public int houseQuestStep;
        public int nextInstanceId = 1;
        public HousePlacedData[] placed = Array.Empty<HousePlacedData>();
        public HouseDepotEntry[] depot = Array.Empty<HouseDepotEntry>();
        /// <summary>Baú da casa, same slot format as the village chest (affixes/durability kept).</summary>
        public InventorySlotData[] chest = Array.Empty<InventorySlotData>();
        /// <summary>Day of the last night slept in the house bed (0 = never).</summary>
        public int lastSleepDay;

        // ---- v2 (H4 upgrades, H6 life in the house). A v1 save simply lacks these and loads with the defaults. ----
        /// <summary>"Descansado" (§6.1).</summary>
        public HouseRestedState rested = new HouseRestedState();
        /// <summary>Comfort and tier when saved (a cache for screens that do not rebuild the layout; recomputed on load).</summary>
        public int comfortCache;
        public int comfortTier;
        /// <summary>Plant watering timers by placed instance (§11.9).</summary>
        public HousePlantTimer[] plants = Array.Empty<HousePlantTimer>();
        /// <summary>Guest book, newest last (≤ 100, §5).</summary>
        public HouseGuestEntry[] guestBook = Array.Empty<HouseGuestEntry>();
        /// <summary>In-game day of the last NPC visit (one a day at most, §4.6).</summary>
        public int lastVisitDay;
        /// <summary>"Missões da Casa" 2–6 completed (ids).</summary>
        public string[] houseQuests = Array.Empty<string>();
        /// <summary>Wardrobe rewards of house quests waiting for the wardrobe (H5).</summary>
        public string[] owedWardrobe = Array.Empty<string>();

        // ---- v3 (H3 sources). Older saves lack it: no boss record, nothing claimed yet. ----
        /// <summary>Boss records (first-win trophy, bad-luck protection) and one-time rewards already paid.</summary>
        public HouseSourceState sources = new HouseSourceState();
    }

    /// <summary>Raised whenever the house changes (granted, furniture, depot, chest capacity, load, new game).</summary>
    public readonly struct HouseChangedEvent : IGameEvent
    {
    }

    /// <summary>"Missão da Casa" step reached (H1: "Um Cantinho Seu" completed).</summary>
    public readonly struct HouseQuestCompletedEvent : IGameEvent
    {
        public readonly string QuestId;

        public HouseQuestCompletedEvent(string questId)
        {
            QuestId = questId;
        }
    }

    /// <summary>
    /// The player's house (Housing.md §2, H1 offline): level, furniture layout + Depósito (<see cref="HouseLayout"/>,
    /// pure rules), the Baú da casa (a <see cref="VillageChestStorage"/> sized by level + extra chests) and the
    /// "Um Cantinho Seu" quest step. GameManager-built, registered with SaveManager under <see cref="Key"/> and with
    /// NewSessionReset. A save written before houses existed has no entry and loads without a house; the house is
    /// handed over at the quest beat (3 Forest victories, <see cref="HouseRules.IsHouseOffered"/>).
    /// </summary>
    public partial class HouseService : IManager, ISaveable, IResettable
    {
        public const string Key = "House";
        /// <summary>1 = H1; 2 = H4/H6 (rested, plants, guest book, house quests, comfort cache); 3 = H3 sources. Older saves load as is.</summary>
        public const int SaveVersion = 3;
        /// <summary>The Vila da Vó Zefa (<c>CityCatalog.VilaZefa</c>): offline the one house is always there (§1).</summary>
        public const string DefaultCityId = "vila_zefa";

        private readonly Func<int> _forestVictories;
        private HouseQuestStep _questStep;
        private int _level = 1;
        private int _lastSleepDay;
        private VillageChestStorage _chest;
        private bool _suppressEvents;

        /// <param name="forestVictories">Victories in the Floresta (the quest beat); defaults to <see cref="WorldMapProgress"/>.</param>
        public HouseService(Func<int> forestVictories = null)
        {
            _forestVictories = forestVictories ?? DefaultForestVictories;
            Layout = new HouseLayout(HouseLevels.Get(1), HouseCatalog.Get);
            Layout.Changed += OnLayoutChanged;
            _chest = new VillageChestStorage(HouseRules.ChestCapacity(Layout.Spec, 0));
        }

        private static HouseService _standalone;

        /// <summary>The GameManager's house (ServiceLocator); without one (a scene played alone) a process-wide one, not saved.</summary>
        public static HouseService ResolveShared()
        {
            if (ServiceLocator.TryGet(out HouseService house))
            {
                return house;
            }

            return _standalone ??= new HouseService();
        }

        private static int DefaultForestVictories()
        {
            return ServiceLocator.TryGet(out WorldMapProgress progress) ? progress.GetVictories(BiomeType.Forest) : 0;
        }

        public string SaveKey => Key;
        public HouseLayout Layout { get; }
        public VillageChestStorage Chest => _chest;
        public int Level => _level;
        public HouseLevelSpec Spec => Layout.Spec;
        public string CityId => DefaultCityId;
        public string OwnerAccountId => HouseRules.LocalAccountId;
        public HouseQuestStep QuestStep => _questStep;
        public bool HasHouse => HouseRules.HasHouse(_questStep);
        public int LastSleepDay => _lastSleepDay;
        public int ForestVictories => _forestVictories();
        public bool IsOffered => HouseRules.IsHouseOffered(_questStep, ForestVictories);
        public bool HasQuestNews => HouseRules.HasQuestNews(_questStep, ForestVictories);

        public void Initialize()
        {
            EventBus.Subscribe<EncounterEndedEvent>(OnEncounterEnded);
            InitializeSources();
        }

        public void Shutdown()
        {
            EventBus.Unsubscribe<EncounterEndedEvent>(OnEncounterEnded);
            ShutdownSources();
        }

        // ------------------------------------------------------------------
        // Quest "Um Cantinho Seu"
        // ------------------------------------------------------------------

        /// <summary>Vó Zefa hands over the house: N1 + the quest rewards and the starter set land in the depot. Once.</summary>
        public bool GrantHouse()
        {
            if (HasHouse)
            {
                return false;
            }

            _suppressEvents = true;
            _questStep = HouseQuestStep.HouseGiven;
            foreach (string itemId in HouseRules.FirstQuestRewards) Layout.AddToDepot(itemId);
            foreach (string itemId in HouseRules.StarterItems) Layout.AddToDepot(itemId);
            _suppressEvents = false;
            RaiseChanged();
            return true;
        }

        /// <summary>The cousin walked in: completes the quest the first time. True when it just completed.</summary>
        public bool MarkEntered()
        {
            if (_questStep != HouseQuestStep.HouseGiven)
            {
                return false;
            }

            _questStep = HouseQuestStep.Completed;
            EventBus.Publish(new HouseQuestCompletedEvent(HouseRules.FirstQuestId));
            RaiseChanged();
            return true;
        }

        // ------------------------------------------------------------------
        // Furniture
        // ------------------------------------------------------------------

        /// <summary>
        /// Stores a placed piece back in the depot; an extra chest only if the house chest's contents still fit
        /// without it (<see cref="HouseRules.CanShrinkChest"/>).
        /// </summary>
        public bool TryStoreInDepot(int instanceId)
        {
            HousePlacedData placed = Layout.Find(instanceId);
            if (placed == null)
            {
                return false;
            }

            HouseItemDefinition item = Layout.Resolve(placed.itemId);
            if (HousePlacementRules.IsExtraChest(item))
            {
                int after = HouseRules.ChestCapacity(Spec, Layout.ExtraChestSlots() - item.FunctionParam);
                if (!HouseRules.CanShrinkChest(_chest.UsedSlotCount, after))
                {
                    return false;
                }
            }

            return Layout.StoreInDepot(instanceId) > 0;
        }

        /// <summary>A night in the house bed: the day it happened (the bed jumps the clock and saves, see the UI).</summary>
        public void RecordSleep(int day)
        {
            _lastSleepDay = day;
            RaiseChanged();
        }

        private void OnLayoutChanged()
        {
            ResizeChest();
            OnLifeLayoutChanged();
            RaiseChanged();
        }

        private void ResizeChest()
        {
            int capacity = HouseRules.ChestCapacity(Spec, Layout.ExtraChestSlots());
            if (_chest != null && _chest.SlotCount == capacity)
            {
                return;
            }

            object contents = _chest?.CaptureState();
            _chest = new VillageChestStorage(capacity);
            if (contents != null)
            {
                _chest.RestoreState(contents);
            }
        }

        private void RaiseChanged()
        {
            if (!_suppressEvents)
            {
                EventBus.Publish(new HouseChangedEvent());
            }
        }

        // ------------------------------------------------------------------
        // Save
        // ------------------------------------------------------------------

        public void ResetForNewSession()
        {
            _suppressEvents = true;
            _questStep = HouseQuestStep.NotStarted;
            _level = 1;
            _lastSleepDay = 0;
            ResetLife();
            ResetSources();
            Layout.Restore(HouseLevels.Get(1), null, null, 1);
            _chest = new VillageChestStorage(HouseRules.ChestCapacity(Spec, 0));
            _suppressEvents = false;
            RaiseChanged();
        }

        public object CaptureState()
        {
            var chest = (VillageChestSaveState)_chest.CaptureState();
            var state = new HouseSaveState
            {
                ownerAccountId = OwnerAccountId,
                cityId = CityId,
                level = _level,
                houseQuestStep = (int)_questStep,
                nextInstanceId = Layout.NextInstanceId,
                placed = Layout.CapturePlaced(),
                depot = Layout.CaptureDepot(),
                chest = chest.slots,
                lastSleepDay = _lastSleepDay,
            };
            CaptureLife(state);
            CaptureSources(state);
            return state;
        }

        public void RestoreState(object state)
        {
            if (state is not HouseSaveState saved)
            {
                return;
            }

            _suppressEvents = true;
            _level = Math.Max(HouseLevels.MinLevel, Math.Min(HouseLevels.MaxLevel, saved.level));
            _questStep = (HouseQuestStep)Math.Max(0, Math.Min((int)HouseQuestStep.Completed, saved.houseQuestStep));
            _lastSleepDay = saved.lastSleepDay;
            RestoreLife(saved);
            RestoreSources(saved);
            Layout.Restore(HouseLevels.Get(_level), saved.placed, saved.depot, saved.nextInstanceId);

            // Sized for what is placed now, then filled (anything over capacity is packed into free slots).
            _chest = new VillageChestStorage(HouseRules.ChestCapacity(Spec, Layout.ExtraChestSlots()));
            _chest.RestoreState(new VillageChestSaveState { slots = saved.chest ?? Array.Empty<InventorySlotData>() });
            OnLifeLayoutChanged();
            _suppressEvents = false;
            RaiseChanged();
        }
    }
}
