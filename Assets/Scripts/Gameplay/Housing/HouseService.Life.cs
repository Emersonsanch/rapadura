using System;
using System.Collections.Generic;
using System.Linq;
using Rapadura.Core.DI;
using Rapadura.Core.Events;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Environment;
using Rapadura.Gameplay.Quests;
using Rapadura.Gameplay.World.Map;

namespace Rapadura.Gameplay.Housing
{
    /// <summary>The house went up a level (H4). <see cref="MovedToDepot"/> pieces did not fit and wait in the Depósito.</summary>
    public readonly struct HouseUpgradedEvent : IGameEvent
    {
        public readonly int FromLevel;
        public readonly int ToLevel;
        public readonly int MovedToDepot;

        public HouseUpgradedEvent(int fromLevel, int toLevel, int movedToDepot)
        {
            FromLevel = fromLevel;
            ToLevel = toLevel;
            MovedToDepot = movedToDepot;
        }
    }

    /// <summary>"Descansado" started, ticked down a fight or ended (HUD/house text).</summary>
    public readonly struct HouseRestedChangedEvent : IGameEvent
    {
        public readonly int Percent;
        public readonly int FightsLeft;

        public HouseRestedChangedEvent(int percent, int fightsLeft)
        {
            Percent = percent;
            FightsLeft = fightsLeft;
        }

        public bool Rested => FightsLeft > 0 && Percent > 0;
    }

    /// <summary>An NPC visit signed the guest book (offline, §4.6).</summary>
    public readonly struct HouseVisitEvent : IGameEvent
    {
        public readonly HouseGuestEntry Entry;

        public HouseVisitEvent(HouseGuestEntry entry)
        {
            Entry = entry;
        }
    }

    /// <summary>What happened when the cousin came home (for the status line).</summary>
    public sealed class HouseHomecoming
    {
        public HouseGuestEntry Visit;
        public int PlantsWateredByRain;
        public readonly List<string> CompletedQuests = new List<string>();
    }

    /// <summary>
    /// H4 + H6 of the house service: upgrades (<see cref="HouseUpgradeRules"/>), "Missões da Casa" 2–6
    /// (<see cref="HouseQuestChain"/>), comfort and sets (<see cref="HouseComfortRules"/>), "Descansado"
    /// (<see cref="HouseRestedRules"/>, applied to kill XP in <c>EnemyRewardGranter</c> through
    /// <see cref="ResolveRestedMultiplier"/> and ticked down on <see cref="EncounterEndedEvent"/>), plants
    /// (<see cref="HousePlantRules"/>) and the guest book with offline visits (<see cref="HouseVisitRules"/>).
    /// </summary>
    public partial class HouseService
    {
        private HouseRestedState _rested = new HouseRestedState();
        private List<HousePlantTimer> _plants = new List<HousePlantTimer>();
        private readonly List<HouseGuestEntry> _guestBook = new List<HouseGuestEntry>();
        private readonly HashSet<string> _houseQuests = new HashSet<string>(StringComparer.Ordinal);
        private readonly List<string> _owedWardrobe = new List<string>();
        private int _lastVisitDay;
        private HouseComfortReport _comfort;
        private int _comfortDay = int.MinValue;

        /// <summary>In-game day (tests inject; default <see cref="TimeOfDayManager.Day"/>, 1 without one).</summary>
        public Func<int> GameDayProvider { get; set; }

        /// <summary>Calendar clock of the rest charges (default the device clock, like the dailies).</summary>
        public IDailyClock Clock { get; set; }

        /// <summary>Boss defeated? (tests inject; default <see cref="DefeatedEnemyRegistry"/>).</summary>
        public Func<string, bool> BossDefeatedProvider { get; set; }

        public int Today => GameDayProvider != null ? GameDayProvider() : DefaultGameDay();
        public int TodayKey => HouseRestedRules.DateKey((Clock ?? SystemDailyClock.Instance).Now);
        public HouseRestedState Rested => _rested;
        public bool IsRested => HouseRestedRules.IsRested(_rested);
        public float RestedMultiplier => HouseRestedRules.Multiplier(_rested);
        public int RestChargesLeft => HouseRestedRules.ChargesLeft(_rested, TodayKey);
        public IReadOnlyList<HouseGuestEntry> GuestBook => _guestBook;
        public IReadOnlyCollection<string> CompletedHouseQuests => _houseQuests;
        public IReadOnlyList<string> OwedWardrobe => _owedWardrobe;
        public IReadOnlyList<HousePlantTimer> PlantTimers => _plants;
        public int LastVisitDay => _lastVisitDay;
        public int BossesDefeated => HouseUpgradeRules.BossIds.Count(IsBossDefeated);

        /// <summary>Comfort, sets and wilted plants right now (cached until the layout, the plants or the day change).</summary>
        public HouseComfortReport Comfort
        {
            get
            {
                int today = Today;
                if (_comfort == null || _comfortDay != today)
                {
                    _comfort = HouseComfortRules.Evaluate(Layout.Placed, Layout.Resolve, IsWilted);
                    _comfortDay = today;
                }

                return _comfort;
            }
        }

        /// <summary>The live house's "Descansado" multiplier for kill XP, or ×1 (no house service, not rested).</summary>
        public static float ResolveRestedMultiplier()
        {
            return ServiceLocator.TryGet(out HouseService house) && house != null ? house.RestedMultiplier : 1f;
        }

        public bool IsBossDefeated(string bossId)
        {
            if (string.IsNullOrEmpty(bossId)) return false;
            return BossDefeatedProvider != null ? BossDefeatedProvider(bossId) : DefaultBossDefeated(bossId);
        }

        public static bool DefaultBossDefeated(string bossId)
        {
            return ServiceLocator.TryGet(out DefeatedEnemyRegistry registry) && registry != null && registry.IsDefeated(bossId);
        }

        private static int DefaultGameDay()
        {
            return ServiceLocator.TryGet(out TimeOfDayManager clock) && clock != null ? clock.Day : 1;
        }

        /// <summary>A keepsake counts when it is in the house: placed or in the Depósito.</summary>
        public bool HasInHouse(string itemId)
        {
            return !string.IsNullOrEmpty(itemId) && (Layout.DepotCount(itemId) > 0 || Layout.Placed.Any(p => p.itemId == itemId));
        }

        // ------------------------------------------------------------------
        // H4 — upgrades
        // ------------------------------------------------------------------

        public HouseUpgradeCheck CheckUpgrade(int heroLevel, IHouseWallet wallet)
        {
            return HouseUpgradeRules.Evaluate(_level, HasHouse, heroLevel, IsBossDefeated, wallet != null ? wallet.Count : (Func<string, int>)null, HasInHouse);
        }

        /// <summary>Checks, pays (all or nothing) and moves the house up one level. Nothing is ever lost.</summary>
        public HouseUpgradeResult TryUpgrade(int heroLevel, IHouseWallet wallet)
        {
            HouseUpgradeCheck check = CheckUpgrade(heroLevel, wallet);
            var result = new HouseUpgradeResult { Check = check, FromLevel = _level, ToLevel = _level };
            if (!check.CanUpgrade)
            {
                return result;
            }

            if (!HouseUpgradeRules.Pay(HouseUpgradeRules.SpentCosts(check.Target), wallet))
            {
                check.Block = HouseUpgradeBlock.PaymentFailed;
                return result;
            }

            result.MovedToDepot = ApplyLevel(check.Target.Level);
            result.ToLevel = _level;
            result.CompletedQuests.AddRange(CheckAutoQuests());
            EventBus.Publish(new HouseUpgradedEvent(result.FromLevel, result.ToLevel, result.MovedToDepot));
            RaiseChanged();
            return result;
        }

        /// <summary>
        /// Moves the layout to <paramref name="level"/>'s grids (<see cref="HouseUpgradeRules.ShiftForUpgrade"/> then
        /// <see cref="HouseLayout.Restore"/>): what does not fit goes to the Depósito. Returns how many pieces did.
        /// </summary>
        public int ApplyLevel(int level)
        {
            int target = Math.Max(HouseLevels.MinLevel, Math.Min(HouseLevels.MaxLevel, level));
            if (target == _level)
            {
                return 0;
            }

            HouseLevelSpec from = Spec;
            HouseLevelSpec to = HouseLevels.Get(target);
            bool suppressed = _suppressEvents;
            _suppressEvents = true;
            HousePlacedData[] shifted = HouseUpgradeRules.ShiftForUpgrade(Layout.CapturePlaced(), from, to);
            _level = to.Level;
            int moved = Layout.Restore(to, shifted, Layout.CaptureDepot(), Layout.NextInstanceId);
            ResizeChest();
            _suppressEvents = suppressed;
            RaiseChanged();
            return moved;
        }

        // ------------------------------------------------------------------
        // Missões da Casa 2–6
        // ------------------------------------------------------------------

        public HouseQuestContext QuestContext(IHouseWallet wallet = null)
        {
            return new HouseQuestContext
            {
                FirstQuestDone = _questStep == HouseQuestStep.Completed,
                HouseLevel = _level,
                Completed = _houseQuests,
                BossDefeated = IsBossDefeated,
                Owned = wallet != null ? wallet.Count : (Func<string, int>)null,
                CousinsSigned = HouseVisitRules.SignedCousins(_guestBook).Count,
            };
        }

        public HouseQuestState QuestState(HouseQuestDefinition quest, IHouseWallet wallet = null) => HouseQuestChain.StateOf(quest, QuestContext(wallet));

        public bool IsHouseQuestDone(string questId) => questId != null && _houseQuests.Contains(questId);

        /// <summary>Hands a ready quest in (a delivery is paid from <paramref name="wallet"/>). Rewards go to the Depósito.</summary>
        public bool TryClaimQuest(string questId, IHouseWallet wallet)
        {
            HouseQuestDefinition quest = HouseQuestChain.Get(questId);
            if (QuestState(quest, wallet) != HouseQuestState.Ready)
            {
                return false;
            }

            if (quest.Kind == HouseQuestKind.Deliver && !HouseUpgradeRules.Pay(quest.Deliver, wallet))
            {
                return false;
            }

            CompleteQuest(quest);
            RaiseChanged();
            return true;
        }

        /// <summary>Completes the level/guest quests that are ready (after an upgrade, a visit, a load). Returns their ids.</summary>
        public List<string> CheckAutoQuests()
        {
            var done = new List<string>();
            foreach (HouseQuestDefinition quest in HouseQuestChain.All)
            {
                if (HouseQuestChain.CompletesByItself(quest) && QuestState(quest) == HouseQuestState.Ready)
                {
                    CompleteQuest(quest);
                    done.Add(quest.Id);
                }
            }

            if (done.Count > 0) RaiseChanged();
            return done;
        }

        private void CompleteQuest(HouseQuestDefinition quest)
        {
            if (!_houseQuests.Add(quest.Id))
            {
                return;
            }

            bool suppressed = _suppressEvents;
            _suppressEvents = true;
            foreach (string reward in quest.Rewards) Layout.GrantToDepot(reward);
            foreach (string look in quest.WardrobeRewards)
            {
                if (!_owedWardrobe.Contains(look)) _owedWardrobe.Add(look);
            }

            _suppressEvents = suppressed;
            EventBus.Publish(new HouseQuestCompletedEvent(quest.Id));
        }

        // ------------------------------------------------------------------
        // Bed: "Descansado"
        // ------------------------------------------------------------------

        /// <summary>A night in the house bed: the rest bonus of today's comfort tier, if a charge is left today.</summary>
        public bool RestInBed()
        {
            HouseComfortReport comfort = Comfort;
            int extra = comfort.HasPerk(HouseSetPerk.RestedExtraFights) ? HouseSetCatalog.LuzMariaExtraFights : 0;
            bool granted = HouseRestedRules.Sleep(_rested, TodayKey, comfort.Tier, extra);
            if (granted)
            {
                EventBus.Publish(new HouseRestedChangedEvent(_rested.percent, _rested.fightsLeft));
                RaiseChanged();
            }

            return granted;
        }

        /// <summary>One fight ended (any outcome): the rest bonus lasts one fight less.</summary>
        public void ConsumeRestedFight()
        {
            if (!IsRested)
            {
                return;
            }

            HouseRestedRules.ConsumeFight(_rested);
            EventBus.Publish(new HouseRestedChangedEvent(_rested.percent, _rested.fightsLeft));
        }

        private void OnEncounterEnded(EncounterEndedEvent evt) => ConsumeRestedFight();

        // ------------------------------------------------------------------
        // Plants
        // ------------------------------------------------------------------

        public HousePlantMood PlantMood(int instanceId)
        {
            foreach (HousePlantTimer timer in _plants)
            {
                if (timer.instanceId == instanceId) return HousePlantRules.MoodOf(timer.lastWateredDay, Today);
            }

            return HousePlantMood.Happy;
        }

        public bool IsWilted(int instanceId) => PlantMood(instanceId) == HousePlantMood.Wilted;

        public int PlantsNeedingWater => _plants.Count(t => HousePlantRules.MoodOf(t.lastWateredDay, Today) != HousePlantMood.Happy);

        /// <summary>The watering can: every plant happy again. Returns how many were thirsty or wilted.</summary>
        public int WaterPlants()
        {
            int perked = HousePlantRules.Water(_plants, Today);
            _comfort = null;
            RaiseChanged();
            return perked;
        }

        /// <summary>Rain waters the yard (not the plants indoors).</summary>
        public int WaterYardByRain()
        {
            int perked = HousePlantRules.Water(_plants, Today, id => Layout.Find(id)?.layer == HouseLayer.Yard);
            _comfort = null;
            return perked;
        }

        private void SyncPlants()
        {
            _plants = HousePlantRules.Sync(_plants, Layout.Placed, Layout.Resolve, Today);
        }

        // ------------------------------------------------------------------
        // Coming home: rain, a visit, quests
        // ------------------------------------------------------------------

        /// <summary>The cousin walks in: rain waters the yard, maybe someone visits and signs, ready quests complete.</summary>
        public HouseHomecoming ComeHome(string activeCousin, bool raining)
        {
            var home = new HouseHomecoming();
            if (!HasHouse)
            {
                return home;
            }

            if (raining) home.PlantsWateredByRain = WaterYardByRain();
            home.Visit = TryVisit(activeCousin);
            home.CompletedQuests.AddRange(CheckAutoQuests());
            RaiseChanged();
            return home;
        }

        /// <summary>One offline visit a day at most, only with the guest book placed. The new signature, or null.</summary>
        public HouseGuestEntry TryVisit(string activeCousin)
        {
            int today = Today;
            if (!HouseVisitRules.CanVisit(_lastVisitDay, today, Layout.HasPlaced(HouseItemFunction.GuestBook)))
            {
                return null;
            }

            List<string> candidates = HouseVisitRules.Candidates(activeCousin, BossesDefeated);
            string visitor = HouseVisitRules.PickVisitor(today, candidates, HouseVisitRules.SignedCousins(_guestBook));
            if (visitor == null)
            {
                return null;
            }

            List<string> comfortable = Layout.Placed.Select(p => p.itemId).Distinct()
                .Where(id => (Layout.Resolve(id)?.Comfort ?? 0) > 0).OrderBy(id => id, StringComparer.Ordinal).ToList();
            var entry = new HouseGuestEntry
            {
                visitorId = visitor,
                line = HouseVisitRules.LineFor(today, visitor),
                itemId = HouseVisitRules.PickItem(today, comfortable) ?? string.Empty,
                day = today,
            };
            HouseVisitRules.Append(_guestBook, entry);
            _lastVisitDay = today;
            EventBus.Publish(new HouseVisitEvent(entry));
            return entry;
        }

        // ------------------------------------------------------------------
        // Save hooks
        // ------------------------------------------------------------------

        private void OnLifeLayoutChanged()
        {
            SyncPlants();
            _comfort = null;
        }

        private void ResetLife()
        {
            _rested = new HouseRestedState();
            _plants = new List<HousePlantTimer>();
            _guestBook.Clear();
            _houseQuests.Clear();
            _owedWardrobe.Clear();
            _lastVisitDay = 0;
            _comfort = null;
        }

        private void CaptureLife(HouseSaveState state)
        {
            HouseComfortReport comfort = Comfort;
            state.rested = _rested.Clone();
            state.comfortCache = comfort.Total;
            state.comfortTier = comfort.Tier;
            state.plants = _plants.Select(t => new HousePlantTimer(t.instanceId, t.lastWateredDay)).ToArray();
            state.guestBook = _guestBook.ToArray();
            state.lastVisitDay = _lastVisitDay;
            state.houseQuests = _houseQuests.OrderBy(id => id, StringComparer.Ordinal).ToArray();
            state.owedWardrobe = _owedWardrobe.ToArray();
        }

        /// <summary>A v1 (H1) save has none of these: everything starts fresh, plants as just watered.</summary>
        private void RestoreLife(HouseSaveState saved)
        {
            ResetLife();
            if (saved.rested != null) _rested = saved.rested.Clone();
            if (saved.plants != null) _plants = saved.plants.Where(t => t != null).Select(t => new HousePlantTimer(t.instanceId, t.lastWateredDay)).ToList();
            if (saved.guestBook != null)
            {
                foreach (HouseGuestEntry entry in saved.guestBook) HouseVisitRules.Append(_guestBook, entry);
            }

            if (saved.houseQuests != null)
            {
                foreach (string id in saved.houseQuests)
                {
                    if (!string.IsNullOrEmpty(id)) _houseQuests.Add(id);
                }
            }

            if (saved.owedWardrobe != null) _owedWardrobe.AddRange(saved.owedWardrobe.Where(id => !string.IsNullOrEmpty(id)));
            _lastVisitDay = saved.lastVisitDay;
        }
    }
}
