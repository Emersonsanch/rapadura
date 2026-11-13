using System;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.World.Map.Overworld;
using Rapadura.Gameplay.WorldEvents;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>A Chegada da Caravana of the Cais da Coluna (91, <see cref="CaisCaravanRules"/>): six gifts from Ammo's caravan, three piers of the Barques of Light.</summary>
    public sealed partial class FinalCityEventsMechanicsDriver
    {
        private const float CaravanBeamHeight = 5f;
        private const float PierBeamHeight = 4.5f;

        private static readonly Color CaravanColor = new Color(1f, 0.85f, 0.5f, 0.95f);
        private static readonly Color PierIdleColor = new Color(0.9f, 0.92f, 1f, 0.45f);
        private static readonly Color PierCalledColor = new Color(1f, 0.95f, 0.65f, 1f);

        private readonly bool[] _giftTaken = new bool[CaisCaravanRules.GiftCount];
        private readonly bool[] _giftDelivered = new bool[CaisCaravanRules.GiftCount];
        private readonly bool[] _giftSaid = new bool[CaisCaravanRules.GiftCount];
        private readonly bool[] _giftGoneSaid = new bool[CaisCaravanRules.GiftCount];
        private LineRenderer _caravanBeam;
        private LineRenderer[] _pierBeams;

        private long _caravanIndex = long.MinValue;
        private long _caravanWarnedWindow = long.MinValue;
        private bool _caravanStarted;
        private bool _caravanSettled;
        private long _caravanPaidLocal;
        private long _caravanOwedIndex = -1L;
        private int _caravanOwedDelivered;
        private float _nextCaravanOwedTry;
        private float _nextCaravanHint;

        private void TickCaravan(Vector2 local, DateTime utc)
        {
            float distance = Vector2.Distance(local, OverworldLayouts.CaisDaColunaOffset);
            if (distance > CaisCaravanRules.AlertRange)
            {
                HideCaravan();
                return;
            }

            if (_caravanOwedIndex >= 0 && Time.unscaledTime >= _nextCaravanOwedTry)
            {
                PayCaravan(_caravanOwedIndex, _caravanOwedDelivered);
            }

            RealTimeWindow window = CaisCaravanRules.WindowAt(utc);
            if (!window.IsOpen)
            {
                HideCaravan();
                if (CaisCaravanRules.IsWarning(window) && _caravanWarnedWindow != window.Index)
                {
                    _caravanWarnedWindow = window.Index;
                    SayFormat(FinalCityEventsTexts.CaravanWarning, WorldEventCalendar.FormatCountdown(window.SecondsToChange));
                }

                return;
            }

            if (window.Index != _caravanIndex)
            {
                ResetCaravan(window.Index);
            }

            if (!_caravanStarted)
            {
                _caravanStarted = true;
                Say(FinalCityEventsTexts.CaravanStart);
            }

            double t = CaisCaravanRules.SecondsInto(window, utc);
            for (int gift = 0; gift < CaisCaravanRules.GiftCount; gift++)
            {
                TickGift(window.Index, gift, local, t);
            }

            ShowCaravan(window.Index, t);
            if (!_caravanSettled && CaisCaravanRules.AllDone(t))
            {
                _caravanSettled = true;
                if (Array.IndexOf(_giftSaid, true) >= 0 && !CaisCaravanRules.IsPaid(GetMark(CaisCaravanRules.PaidKey, _caravanPaidLocal), window.Index))
                {
                    PayCaravan(window.Index, DeliveredCount());
                }
            }
        }

        private void ResetCaravan(long index)
        {
            _caravanIndex = index;
            _caravanStarted = false;
            _caravanSettled = false;
            for (int i = 0; i < CaisCaravanRules.GiftCount; i++)
            {
                _giftTaken[i] = _giftDelivered[i] = _giftSaid[i] = _giftGoneSaid[i] = false;
            }
        }

        private int DeliveredCount()
        {
            int delivered = 0;
            foreach (bool d in _giftDelivered)
            {
                delivered += d ? 1 : 0;
            }

            return delivered;
        }

        private void TickGift(long windowIndex, int gift, Vector2 local, double t)
        {
            CaravanGiftPhase phase = CaisCaravanRules.PhaseOf(gift, t);
            if (phase == CaravanGiftPhase.Waiting || _giftDelivered[gift])
            {
                return;
            }

            int pier = CaisCaravanRules.PierOf(windowIndex, gift);
            if (phase == CaravanGiftPhase.Gone)
            {
                if (_giftSaid[gift] && !_giftGoneSaid[gift])
                {
                    _giftGoneSaid[gift] = true;
                    SayFormat(FinalCityEventsTexts.CaravanGone, SideName(pier));
                }

                return;
            }

            if (!_giftSaid[gift])
            {
                _giftSaid[gift] = true;
                SayFormat(FinalCityEventsTexts.CaravanGift, SideName(pier), Mathf.CeilToInt((float)CaisCaravanRules.SecondsLeft(gift, t)), gift + 1, CaisCaravanRules.GiftCount);
            }

            if (!_stats.IsAlive)
            {
                return;
            }

            if (!_giftTaken[gift])
            {
                if (!CaisCaravanRules.CanTake(OverworldLayouts.CaisCaravanMooring, local))
                {
                    return;
                }

                InteractPromptRegistry.Ping(Time.unscaledTime);
                CaravanHint(FinalCityEventsTexts.CaravanTake);
                if (InteractPressed())
                {
                    _giftTaken[gift] = true;
                    _nextCaravanHint = Time.unscaledTime + HintCooldown;
                    SayFormat(FinalCityEventsTexts.CaravanTaken, SideName(pier));
                }

                return;
            }

            Vector2 at = OverworldLayouts.CaisCaravanPiers[Mathf.Clamp(pier, 0, OverworldLayouts.CaisCaravanPiers.Length - 1)];
            if (!CaisCaravanRules.CanBoard(at, local, Hero() == CharacterId.Lavine))
            {
                return;
            }

            // At the pier with the gift: it goes aboard by itself (the barque's rowers take it).
            _giftDelivered[gift] = true;
            SayFormat(FinalCityEventsTexts.CaravanDelivered, DeliveredCount(), CaisCaravanRules.GiftCount);
        }

        private void CaravanHint(string key)
        {
            if (Time.unscaledTime < _nextCaravanHint)
            {
                return;
            }

            _nextCaravanHint = Time.unscaledTime + HintCooldown;
            Say(key);
        }

        private void PayCaravan(long index, int delivered)
        {
            if (delivered <= 0)
            {
                _caravanOwedIndex = -1L;
                _caravanPaidLocal = CaisCaravanRules.PaidMark(index);
                SetMark(CaisCaravanRules.PaidKey, _caravanPaidLocal);
                Say(FinalCityEventsTexts.CaravanNone);
                return;
            }

            int gold = CaisCaravanRules.GoldFor(delivered);
            int stones = CaisCaravanRules.StonesFor(delivered);
            if (!Grant(gold, stones))
            {
                _caravanOwedIndex = index;
                _caravanOwedDelivered = delivered;
                _nextCaravanOwedTry = Time.unscaledTime + 5f;
                return;
            }

            _caravanOwedIndex = -1L;
            _caravanPaidLocal = CaisCaravanRules.PaidMark(index);
            SetMark(CaisCaravanRules.PaidKey, _caravanPaidLocal);
            SayFormat(FinalCityEventsTexts.CaravanPaid, delivered, gold, stones);
            if (delivered >= CaisCaravanRules.GiftCount)
            {
                CousinLine("caravan");
            }
        }

        private void ShowCaravan(long windowIndex, double t)
        {
            Vector2[] piers = OverworldLayouts.CaisCaravanPiers;
            _pierBeams ??= new LineRenderer[piers.Length];
            int gift = CaisCaravanRules.ReadyGift(t);
            bool waiting = gift >= 0 && !_giftDelivered[gift];
            int called = waiting ? CaisCaravanRules.PierOf(windowIndex, gift) : -1;
            for (int i = 0; i < piers.Length; i++)
            {
                Beam(ref _pierBeams[i], "Cc_LightPier", i == called ? 0.6f : 0.3f, _colunaOrigin + piers[i], PierBeamHeight, i == called ? PierCalledColor : PierIdleColor);
            }

            // The caravan glows while a gift waits in it.
            bool inCaravan = waiting && !_giftTaken[gift];
            Beam(ref _caravanBeam, "Cc_Caravan", inCaravan ? 0.8f : 0.35f, _colunaOrigin + OverworldLayouts.CaisCaravanMooring, CaravanBeamHeight,
                inCaravan ? CaravanColor : PierIdleColor);
        }

        private void HideCaravan()
        {
            Hide(_caravanBeam);
            Hide(_pierBeams);
        }
    }
}
