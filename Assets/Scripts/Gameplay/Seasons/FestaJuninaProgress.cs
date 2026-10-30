using System;
using System.Collections.Generic;
using Rapadura.Core.DI;
using Rapadura.Core.Events;
using Rapadura.Core.Interfaces;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Fishing;
using Rapadura.Gameplay.Housing;

namespace Rapadura.Gameplay.Seasons
{
    /// <summary>JSON-serializable snapshot of <see cref="FestaJuninaProgress"/>.</summary>
    [Serializable]
    public class FestaJuninaSaveState
    {
        public int prendas;
        public int totalEarned;
        public int dances;
        public int bestScore;
        public bool fullComboDanced;
        public int lastDanceDay;
        public int fishDay;
        public int fishPaidToday;
        public List<string> bought = new List<string>();
        public bool legendaryGranted;
    }

    /// <summary>Prendas or the stall changed (the HUD/panel refresh).</summary>
    public readonly struct FestaJuninaChangedEvent : IGameEvent
    {
        public readonly int Prendas;

        public FestaJuninaChangedEvent(int prendas)
        {
            Prendas = prendas;
        }
    }

    /// <summary>What one quadrilha paid.</summary>
    public readonly struct QuadrilhaReward
    {
        public readonly int Prendas;
        public readonly bool FirstToday;
        public readonly bool NewBest;
        public readonly bool LegendaryGranted;

        public QuadrilhaReward(int prendas, bool firstToday, bool newBest, bool legendaryGranted)
        {
            Prendas = prendas;
            FirstToday = firstToday;
            NewBest = newBest;
            LegendaryGranted = legendaryGranted;
        }
    }

    /// <summary>
    /// The save's Festa Junina: the prendas (event currency, kept from one June to the next), the dances, the prizes
    /// bought at the Barraca de Prendas and the Lendário of the tarefas do arraiá. Plain C# (EditMode-testable), built
    /// by GameManager and saved under <see cref="Key"/>; a save from before the festa loads with nothing. Prizes go to
    /// the house's Depósito (<see cref="HouseLayout.GrantToDepot"/>: a prize is never lost, house or no house yet).
    /// During the festa every fish landed also pays a prenda (the "pescaria", <see cref="FestaJuninaRules.PrendasForFish"/>).
    /// </summary>
    public class FestaJuninaProgress : IManager, ISaveable, IResettable
    {
        public const string Key = "FestaJunina";

        private readonly List<string> _bought = new List<string>();
        private bool _subscribed;
        private int _lastDanceDay;
        private int _fishDay;
        private int _fishPaidToday;

        public string SaveKey => Key;

        /// <summary>Explicit house (tests); null = <see cref="HouseService.ResolveShared"/>.</summary>
        public HouseService House { get; set; }

        /// <summary>Explicit "festa is on" (tests); null = <see cref="SeasonCalendar.IsFestaJuninaActive"/>.</summary>
        public Func<bool> FestaActiveProbe { get; set; }

        /// <summary>Explicit today (tests); null = <see cref="SeasonCalendar.Today"/>.</summary>
        public Func<DateTime> TodayProbe { get; set; }

        public int Prendas { get; private set; }
        public int TotalEarned { get; private set; }
        public int Dances { get; private set; }
        public int BestScore { get; private set; }
        public bool FullComboDanced { get; private set; }
        public bool LegendaryGranted { get; private set; }
        public IReadOnlyList<string> Bought => _bought;
        public int DistinctPrizes => _bought.Count;

        public bool FestaActive => FestaActiveProbe != null ? FestaActiveProbe() : SeasonCalendar.IsFestaJuninaActive;
        private DateTime Today => TodayProbe != null ? TodayProbe().Date : SeasonCalendar.Today;
        private HouseService ResolveHouse() => House ?? HouseService.ResolveShared();

        public bool TasksDone => FestaJuninaRules.TasksDone(Dances, FullComboDanced, DistinctPrizes);

        private static FestaJuninaProgress _standalone;

        /// <summary>The GameManager's festa, or a process-wide unsaved one when a scene is played on its own.</summary>
        public static FestaJuninaProgress ResolveShared()
        {
            if (ServiceLocator.TryGet(out FestaJuninaProgress progress))
            {
                return progress;
            }

            if (_standalone == null)
            {
                _standalone = new FestaJuninaProgress();
                _standalone.Initialize();
            }

            return _standalone;
        }

        public void Initialize()
        {
            if (ServiceLocator.TryGet(out LocalizationManager localization))
            {
                FestaJuninaTexts.EnsureRegistered(localization);
            }

            if (_subscribed)
            {
                return;
            }

            _subscribed = true;
            EventBus.Subscribe<FishCaughtEvent>(OnFishCaught);
        }

        public void Shutdown()
        {
            if (!_subscribed)
            {
                return;
            }

            _subscribed = false;
            EventBus.Unsubscribe<FishCaughtEvent>(OnFishCaught);
        }

        public void ResetForNewSession()
        {
            Prendas = 0;
            TotalEarned = 0;
            Dances = 0;
            BestScore = 0;
            FullComboDanced = false;
            LegendaryGranted = false;
            _bought.Clear();
            _lastDanceDay = 0;
            _fishDay = 0;
            _fishPaidToday = 0;
        }

        public bool HasBought(string itemId) => itemId != null && _bought.Contains(itemId);

        /// <summary>The first quadrilha of today still pays the <see cref="FestaJuninaRules.FirstDanceBonus"/>.</summary>
        public bool FirstDanceBonusAvailable => _lastDanceDay != SeasonCalendar.DayKey(Today);

        /// <summary>Pays a finished quadrilha (nothing outside the festa).</summary>
        public QuadrilhaReward RecordDance(int score, int maxScore, bool fullCombo)
        {
            if (!FestaActive)
            {
                return new QuadrilhaReward(0, false, false, false);
            }

            int today = SeasonCalendar.DayKey(Today);
            bool first = _lastDanceDay != today;
            _lastDanceDay = today;
            int prendas = FestaJuninaRules.PrendasForDance(score, maxScore, fullCombo, first);
            Dances++;
            bool newBest = score > BestScore;
            BestScore = Math.Max(BestScore, score);
            FullComboDanced |= fullCombo;
            Earn(prendas);
            bool legendary = TryGrantLegendary();
            Publish();
            return new QuadrilhaReward(prendas, first, newBest, legendary);
        }

        /// <summary>A prize of the Barraca de Prendas: pays, sends it to the Depósito and checks the Lendário.</summary>
        public PrendaBuyCheck TryBuy(string itemId)
        {
            HouseItemDefinition item = FestaJuninaRules.StallItem(itemId);
            PrendaBuyCheck check = FestaJuninaRules.CheckBuy(item, Prendas, FestaActive);
            if (check != PrendaBuyCheck.Ok)
            {
                return check;
            }

            Prendas -= item.EventPrice;
            ResolveHouse()?.Layout.GrantToDepot(item.ItemId);
            if (!_bought.Contains(item.ItemId))
            {
                _bought.Add(item.ItemId);
            }

            TryGrantLegendary();
            Publish();
            return PrendaBuyCheck.Ok;
        }

        /// <summary>Adds prendas (debug / other festa sources).</summary>
        public void Earn(int prendas)
        {
            if (prendas <= 0)
            {
                return;
            }

            Prendas += prendas;
            TotalEarned += prendas;
        }

        private bool TryGrantLegendary()
        {
            if (LegendaryGranted || !TasksDone)
            {
                return false;
            }

            LegendaryGranted = true;
            ResolveHouse()?.Layout.GrantToDepot(FestaJuninaRules.LegendaryItemId);
            return true;
        }

        private void OnFishCaught(FishCaughtEvent evt)
        {
            RecordFish();
        }

        /// <summary>The pescaria of the arraiá: one prenda a fish during the festa, capped per day. Returns what it paid.</summary>
        public int RecordFish()
        {
            int today = SeasonCalendar.DayKey(Today);
            if (_fishDay != today)
            {
                _fishDay = today;
                _fishPaidToday = 0;
            }

            int prendas = FestaJuninaRules.PrendasForFish(FestaActive, _fishPaidToday);
            if (prendas <= 0)
            {
                return 0;
            }

            _fishPaidToday++;
            Earn(prendas);
            Publish();
            return prendas;
        }

        private void Publish() => EventBus.Publish(new FestaJuninaChangedEvent(Prendas));

        public object CaptureState()
        {
            return new FestaJuninaSaveState
            {
                prendas = Prendas,
                totalEarned = TotalEarned,
                dances = Dances,
                bestScore = BestScore,
                fullComboDanced = FullComboDanced,
                lastDanceDay = _lastDanceDay,
                fishDay = _fishDay,
                fishPaidToday = _fishPaidToday,
                bought = new List<string>(_bought),
                legendaryGranted = LegendaryGranted,
            };
        }

        public void RestoreState(object state)
        {
            if (state is not FestaJuninaSaveState saved)
            {
                return;
            }

            ResetForNewSession();
            Prendas = Math.Max(0, saved.prendas);
            TotalEarned = Math.Max(0, saved.totalEarned);
            Dances = Math.Max(0, saved.dances);
            BestScore = Math.Max(0, saved.bestScore);
            FullComboDanced = saved.fullComboDanced;
            _lastDanceDay = saved.lastDanceDay;
            _fishDay = saved.fishDay;
            _fishPaidToday = Math.Max(0, saved.fishPaidToday);
            LegendaryGranted = saved.legendaryGranted;
            if (saved.bought != null)
            {
                foreach (string id in saved.bought)
                {
                    if (!string.IsNullOrEmpty(id) && !_bought.Contains(id))
                    {
                        _bought.Add(id);
                    }
                }
            }
        }
    }
}
