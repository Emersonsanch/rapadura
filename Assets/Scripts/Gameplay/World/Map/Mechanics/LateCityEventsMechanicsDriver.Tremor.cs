using System;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.World.Map.Overworld;
using Rapadura.Gameplay.WorldEvents;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>The Grande Tremor of Hinokura (65, <see cref="HinokuraGreatTremorRules"/>): four surges of lava, three diverter stones, the channels changing.</summary>
    public sealed partial class LateCityEventsMechanicsDriver
    {
        private const float StoneBeamHeight = 4f;
        private const float SurgeLift = 0.3f;

        private static readonly Color StoneIdleColor = new Color(0.35f, 0.3f, 0.3f, 0.6f);
        private static readonly Color StoneCalledColor = new Color(1f, 0.45f, 0.15f, 0.95f);
        private static readonly Color StoneTurnedColor = new Color(1f, 0.85f, 0.35f, 1f);
        private static readonly Color SurgeColor = new Color(1f, 0.38f, 0.08f, 0.95f);
        private static readonly Color SpillColor = new Color(0.95f, 0.3f, 0.05f, 0.85f);

        private readonly bool[] _tremorTurned = new bool[HinokuraGreatTremorRules.SurgeCount];
        private readonly bool[] _tremorResolved = new bool[HinokuraGreatTremorRules.SurgeCount];
        private readonly bool[] _tremorFed = new bool[HinokuraGreatTremorRules.SurgeCount];
        private readonly bool[] _tremorCalled = new bool[HinokuraGreatTremorRules.SurgeCount];
        private LineRenderer[] _stoneBeams;
        private LineRenderer _surgeLine;
        private LineRenderer _spillLine;

        private long _tremorIndex = long.MinValue;
        private long _tremorWarnedWindow = long.MinValue;
        private bool _tremorSettled;
        private long _tremorPaidLocal;
        private long _tremorOwedIndex = -1L;
        private int _tremorOwedFed;
        private float _nextTremorOwedTry;
        private float _nextTremorHint;
        private float _nextBurn;

        private void TickTremor(Vector2 local, DateTime utc)
        {
            float distance = Vector2.Distance(local, OverworldLayouts.HinokuraOffset);
            if (distance > HinokuraGreatTremorRules.AlertRange)
            {
                HideTremor();
                return;
            }

            if (_tremorOwedIndex >= 0 && Time.unscaledTime >= _nextTremorOwedTry)
            {
                PayTremor(_tremorOwedIndex, _tremorOwedFed);
            }

            RealTimeWindow window = HinokuraGreatTremorRules.WindowAt(utc);
            if (!window.IsOpen)
            {
                HideTremor();
                if (HinokuraGreatTremorRules.IsWarning(window) && _tremorWarnedWindow != window.Index)
                {
                    _tremorWarnedWindow = window.Index;
                    SayFormat(LateCityEventsTexts.TremorWarning, WorldEventCalendar.FormatCountdown(window.SecondsToChange));
                }

                return;
            }

            if (window.Index != _tremorIndex)
            {
                ResetTremor(window.Index);
            }

            double t = HinokuraGreatTremorRules.SecondsInto(window, utc);
            HinokuraLavaDiverter[] diverters = OverworldLayouts.HinokuraLavaDiverters;
            for (int surge = 0; surge < HinokuraGreatTremorRules.SurgeCount; surge++)
            {
                TickSurge(window.Index, surge, diverters, local, t);
            }

            TickSpillBurn(window.Index, diverters, local, t);
            ShowTremor(window.Index, diverters, t);
            if (!_tremorSettled && HinokuraGreatTremorRules.AllDone(t))
            {
                _tremorSettled = true;
                if (Array.IndexOf(_tremorCalled, true) >= 0
                    && !HinokuraGreatTremorRules.IsPaid(GetMark(HinokuraGreatTremorRules.PaidKey, _tremorPaidLocal), window.Index))
                {
                    PayTremor(window.Index, FedCount());
                }
            }
        }

        private void ResetTremor(long index)
        {
            _tremorIndex = index;
            _tremorSettled = false;
            for (int i = 0; i < HinokuraGreatTremorRules.SurgeCount; i++)
            {
                _tremorTurned[i] = _tremorResolved[i] = _tremorFed[i] = _tremorCalled[i] = false;
            }
        }

        private int FedCount()
        {
            int fed = 0;
            foreach (bool f in _tremorFed)
            {
                fed += f ? 1 : 0;
            }

            return fed;
        }

        private void TickSurge(long windowIndex, int surge, HinokuraLavaDiverter[] diverters, Vector2 local, double t)
        {
            TremorSurgePhase phase = HinokuraGreatTremorRules.PhaseOf(surge, t);
            int channel = HinokuraGreatTremorRules.ChannelOf(windowIndex, surge);
            HinokuraLavaDiverter diverter = diverters[Mathf.Clamp(channel, 0, diverters.Length - 1)];
            if (phase == TremorSurgePhase.Warning)
            {
                if (!_tremorCalled[surge])
                {
                    _tremorCalled[surge] = true;
                    SayFormat(LateCityEventsTexts.TremorCall, SideName(channel), Mathf.CeilToInt((float)HinokuraGreatTremorRules.SecondsToSurge(surge, t)), surge + 1);
                }

                if (!_tremorTurned[surge] && _stats.IsAlive && HinokuraGreatTremorRules.CanTurn(diverter.Stone, local, true))
                {
                    TickTurn(surge, diverter, local);
                }

                return;
            }

            if (_tremorResolved[surge] || !HinokuraGreatTremorRules.Arrived(surge, t))
            {
                return;
            }

            // The surge reached the stone: fed or spilled, decided once.
            _tremorResolved[surge] = true;
            _tremorFed[surge] = _tremorTurned[surge];
            if (!_tremorCalled[surge])
            {
                return; // ran before the hero came near: nothing to say
            }

            if (_tremorFed[surge])
            {
                SayFormat(LateCityEventsTexts.TremorFed, FedCount());
            }
            else
            {
                SayFormat(LateCityEventsTexts.TremorSpilled, SideName(channel));
            }
        }

        /// <summary>The hero is within Ícaro's reach of the called stone: the real reach depends on who is playing.</summary>
        private void TickTurn(int surge, HinokuraLavaDiverter diverter, Vector2 local)
        {
            if (!HinokuraGreatTremorRules.CanTurn(diverter.Stone, local, Hero() == CharacterId.Icaro))
            {
                return;
            }

            InteractPromptRegistry.Ping(Time.unscaledTime);
            if (Time.unscaledTime >= _nextTremorHint)
            {
                _nextTremorHint = Time.unscaledTime + HintCooldown;
                Say(LateCityEventsTexts.TremorTurn);
            }

            if (!InteractPressed())
            {
                return;
            }

            _tremorTurned[surge] = true;
            _nextTremorHint = Time.unscaledTime + HintCooldown;
            Say(LateCityEventsTexts.TremorTurned);
        }

        /// <summary>The spilled lava burns whoever stands in it, once a second (never below the floor).</summary>
        private void TickSpillBurn(long windowIndex, HinokuraLavaDiverter[] diverters, Vector2 local, double t)
        {
            if (!_stats.IsAlive || Time.time < _nextBurn)
            {
                return;
            }

            for (int surge = 0; surge < HinokuraGreatTremorRules.SurgeCount; surge++)
            {
                if (!_tremorResolved[surge] || _tremorFed[surge] || !_tremorCalled[surge] || !HinokuraGreatTremorRules.SpillBurning(surge, t))
                {
                    continue;
                }

                HinokuraLavaDiverter diverter = diverters[Mathf.Clamp(HinokuraGreatTremorRules.ChannelOf(windowIndex, surge), 0, diverters.Length - 1)];
                if (!HinokuraGreatTremorRules.InSpill(diverter.Spill, local))
                {
                    continue;
                }

                _nextBurn = Time.time + HinokuraGreatTremorRules.BurnEverySeconds;
                float burn = HinokuraGreatTremorRules.Burn(_stats.CurrentHealth, _stats.MaxHealth);
                if (burn > 0f)
                {
                    _stats.ApplyHazardDamage(burn);
                    Say(LateCityEventsTexts.TremorBurn);
                }

                return;
            }
        }

        private void PayTremor(long index, int fed)
        {
            if (fed <= 0)
            {
                _tremorOwedIndex = -1L;
                _tremorPaidLocal = HinokuraGreatTremorRules.PaidMark(index);
                SetMark(HinokuraGreatTremorRules.PaidKey, _tremorPaidLocal);
                Say(LateCityEventsTexts.TremorNone);
                return;
            }

            int gold = HinokuraGreatTremorRules.GoldFor(fed);
            int stones = HinokuraGreatTremorRules.StonesFor(fed);
            if (!Grant(gold, stones))
            {
                _tremorOwedIndex = index;
                _tremorOwedFed = fed;
                _nextTremorOwedTry = Time.unscaledTime + 5f;
                return;
            }

            _tremorOwedIndex = -1L;
            _tremorPaidLocal = HinokuraGreatTremorRules.PaidMark(index);
            SetMark(HinokuraGreatTremorRules.PaidKey, _tremorPaidLocal);
            SayFormat(LateCityEventsTexts.TremorPaid, fed, gold, stones);
            if (fed >= HinokuraGreatTremorRules.SurgeCount)
            {
                CousinLine("tremor");
            }
        }

        private void ShowTremor(long windowIndex, HinokuraLavaDiverter[] diverters, double t)
        {
            _stoneBeams ??= new LineRenderer[diverters.Length];
            int surge = HinokuraGreatTremorRules.CurrentSurge(t);
            int called = surge >= 0 ? HinokuraGreatTremorRules.ChannelOf(windowIndex, surge) : -1;
            for (int i = 0; i < diverters.Length; i++)
            {
                Color color = i != called ? StoneIdleColor : _tremorTurned[surge] ? StoneTurnedColor : StoneCalledColor;
                Beam(ref _stoneBeams[i], "Hk_TremorStone", i == called ? 0.6f : 0.35f, _hinokuraOrigin + diverters[i].Stone, StoneBeamHeight, color);
            }

            if (surge >= 0 && called >= 0 && HinokuraGreatTremorRules.PhaseOf(surge, t) == TremorSurgePhase.Surging)
            {
                // The surge: a glowing line from the mouth to where its front has come.
                HinokuraLavaDiverter diverter = diverters[called];
                Vector2 mouth = _hinokuraOrigin + diverter.Mouth;
                Vector2 front = Vector2.Lerp(mouth, _hinokuraOrigin + diverter.Stone, HinokuraGreatTremorRules.Progress(surge, t));
                Post(ref _surgeLine, "Hk_TremorSurge", 1.6f, Ground(mouth) + Vector3.up * SurgeLift, Ground(front) + Vector3.up * SurgeLift, SurgeColor);
            }
            else
            {
                Hide(_surgeLine);
            }

            ShowSpill(windowIndex, diverters, t);
        }

        private void ShowSpill(long windowIndex, HinokuraLavaDiverter[] diverters, double t)
        {
            for (int s = 0; s < HinokuraGreatTremorRules.SurgeCount; s++)
            {
                if (!_tremorResolved[s] || _tremorFed[s] || !_tremorCalled[s] || !HinokuraGreatTremorRules.SpillBurning(s, t))
                {
                    continue;
                }

                // The pool across the road: a wide glowing bar as wide as the spill.
                HinokuraLavaDiverter diverter = diverters[Mathf.Clamp(HinokuraGreatTremorRules.ChannelOf(windowIndex, s), 0, diverters.Length - 1)];
                Vector2 spill = _hinokuraOrigin + diverter.Spill;
                Vector2 along = (diverter.Stone - diverter.Spill).normalized;
                Vector2 side = new Vector2(along.y, -along.x) * HinokuraGreatTremorRules.SpillRadius;
                Post(ref _spillLine, "Hk_TremorSpill", HinokuraGreatTremorRules.SpillRadius * 2f, Ground(spill - side) + Vector3.up * 0.15f,
                    Ground(spill + side) + Vector3.up * 0.15f, SpillColor);
                return;
            }

            Hide(_spillLine);
        }

        private void HideTremor()
        {
            Hide(_stoneBeams);
            Hide(_surgeLine);
            Hide(_spillLine);
        }
    }
}
