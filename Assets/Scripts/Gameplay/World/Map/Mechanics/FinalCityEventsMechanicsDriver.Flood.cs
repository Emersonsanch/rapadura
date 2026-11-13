using System;
using Rapadura.Core.DI;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.World.Map.Overworld;
using Rapadura.Gameplay.WorldEvents;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>A Cheia of Dur-Jarras (77, <see cref="DurJarrasFloodRules"/>): the water climbing the ziggurat, five tablets to bring up, the steps they open.</summary>
    public sealed partial class FinalCityEventsMechanicsDriver
    {
        private const float TabletBeamHeight = 3.5f;
        private const float ZigguratBeamHeight = 6f;
        private const int FloodRingPoints = 48;
        private const float FloodRingHeight = 0.6f;

        private static readonly Color TabletColor = new Color(0.85f, 0.62f, 0.38f, 0.95f);
        private static readonly Color ZigguratColor = new Color(0.35f, 0.6f, 0.95f, 0.85f);
        private static readonly Color FloodColor = new Color(0.25f, 0.45f, 0.75f, 0.7f);

        private readonly bool[] _tabletTaken = new bool[DurJarrasFloodRules.TabletCount];
        private readonly bool[] _tabletSaved = new bool[DurJarrasFloodRules.TabletCount];
        private readonly bool[] _tabletSaid = new bool[DurJarrasFloodRules.TabletCount];
        private readonly bool[] _tabletLostSaid = new bool[DurJarrasFloodRules.TabletCount];
        private LineRenderer _tabletBeam;
        private LineRenderer _zigguratBeam;
        private LineRenderer _floodRing;
        private float _floodRingRadius = -1f;

        private long _floodIndex = long.MinValue;
        private long _floodWarnedWindow = long.MinValue;
        private bool _floodStarted;
        private bool _floodSettled;
        private int _floodStepSaid;
        private bool _floodBitten;
        private float _nextFloodBite;
        private long _floodPaidLocal;
        private long _floodOwedIndex = -1L;
        private int _floodOwedSaved;
        private float _nextFloodOwedTry;
        private float _nextFloodHint;
        private long _floodSavedLocal;
        private long _floodStepPaidLocal;

        private void TickFlood(Vector2 local, DateTime utc)
        {
            float distance = Vector2.Distance(local, OverworldLayouts.DurJarrasOffset);
            if (distance > DurJarrasFloodRules.AlertRange)
            {
                HideFlood();
                return;
            }

            if (_floodOwedIndex >= 0 && Time.unscaledTime >= _nextFloodOwedTry)
            {
                PayFlood(_floodOwedIndex, _floodOwedSaved);
            }

            RealTimeWindow window = DurJarrasFloodRules.WindowAt(utc);
            if (!window.IsOpen)
            {
                HideFlood();
                if (DurJarrasFloodRules.IsWarning(window) && _floodWarnedWindow != window.Index)
                {
                    _floodWarnedWindow = window.Index;
                    SayFormat(FinalCityEventsTexts.FloodWarning, WorldEventCalendar.FormatCountdown(window.SecondsToChange));
                }

                // A window that closed while the hero was here is settled on the first frame after it.
                if (_floodIndex != long.MinValue && !_floodSettled)
                {
                    SettleFlood(_floodIndex);
                }

                return;
            }

            if (window.Index != _floodIndex)
            {
                if (_floodIndex != long.MinValue && !_floodSettled)
                {
                    SettleFlood(_floodIndex);
                }

                ResetFlood(window.Index);
            }

            double t = DurJarrasFloodRules.SecondsInto(window, utc);
            if (!_floodStarted)
            {
                _floodStarted = true;
                Say(FinalCityEventsTexts.FloodStart);
            }

            int step = DurJarrasFloodRules.StepAt(t);
            if (step != _floodStepSaid)
            {
                _floodStepSaid = step;
                SayFormat(FinalCityEventsTexts.FloodStep, StepName(step), step, DurJarrasFloodRules.StepCount);
            }

            TickFloodBite(distance, step);
            for (int tablet = 0; tablet < DurJarrasFloodRules.TabletCount; tablet++)
            {
                TickTablet(window.Index, tablet, local, t);
            }

            TickDeliver(local);
            ShowFlood(window.Index, t, step);
        }

        private void ResetFlood(long index)
        {
            _floodIndex = index;
            _floodStarted = false;
            _floodSettled = false;
            _floodStepSaid = 0;
            _floodBitten = false;
            for (int i = 0; i < DurJarrasFloodRules.TabletCount; i++)
            {
                _tabletTaken[i] = _tabletSaved[i] = _tabletSaid[i] = _tabletLostSaid[i] = false;
            }
        }

        private void TickFloodBite(float distance, int step)
        {
            if (!_stats.IsAlive || !DurJarrasFloodRules.Bites(distance, step, Hero() == CharacterId.Joaquim) || Time.time < _nextFloodBite)
            {
                return;
            }

            _nextFloodBite = Time.time + DurJarrasFloodRules.BiteEverySeconds;
            float bite = DurJarrasFloodRules.Bite(_stats.CurrentHealth, _stats.MaxHealth);
            if (bite <= 0f)
            {
                return;
            }

            _stats.ApplyHazardDamage(bite);
            if (!_floodBitten)
            {
                _floodBitten = true;
                Say(FinalCityEventsTexts.FloodCurrent);
            }
        }

        private void TickTablet(long windowIndex, int tablet, Vector2 local, double t)
        {
            FloodTabletPhase phase = DurJarrasFloodRules.PhaseOf(tablet, t);
            if (_tabletTaken[tablet] || phase == FloodTabletPhase.Waiting)
            {
                return;
            }

            int road = DurJarrasFloodRules.RoadOf(windowIndex, tablet);
            if (phase == FloodTabletPhase.Lost)
            {
                if (_tabletSaid[tablet] && !_tabletLostSaid[tablet])
                {
                    _tabletLostSaid[tablet] = true;
                    SayFormat(FinalCityEventsTexts.FloodLost, SideName(road));
                }

                return;
            }

            if (!_tabletSaid[tablet])
            {
                _tabletSaid[tablet] = true;
                SayFormat(FinalCityEventsTexts.FloodTablet, SideName(road), Mathf.CeilToInt((float)DurJarrasFloodRules.SecondsLeft(tablet, t)), tablet + 1,
                    DurJarrasFloodRules.TabletCount);
            }

            Vector2 at = OverworldLayouts.DurJarrasFloodTablets[Mathf.Clamp(road, 0, OverworldLayouts.DurJarrasFloodTablets.Length - 1)];
            if (!_stats.IsAlive || !DurJarrasFloodRules.CanPick(at, local))
            {
                return;
            }

            InteractPromptRegistry.Ping(Time.unscaledTime);
            if (Time.unscaledTime >= _nextFloodHint)
            {
                _nextFloodHint = Time.unscaledTime + HintCooldown;
                Say(FinalCityEventsTexts.FloodPick);
            }

            if (!InteractPressed())
            {
                return;
            }

            _tabletTaken[tablet] = true;
            _nextFloodHint = Time.unscaledTime + HintCooldown;
            Say(FinalCityEventsTexts.FloodPicked);
        }

        private void TickDeliver(Vector2 local)
        {
            if (!DurJarrasFloodRules.CanDeliver(OverworldLayouts.DurJarrasZigguratFoot, local))
            {
                return;
            }

            int delivered = 0;
            for (int i = 0; i < DurJarrasFloodRules.TabletCount; i++)
            {
                if (_tabletTaken[i] && !_tabletSaved[i])
                {
                    _tabletSaved[i] = true;
                    delivered++;
                }
            }

            if (delivered > 0)
            {
                SayFormat(FinalCityEventsTexts.FloodSaved, SavedCount());
            }
        }

        private int SavedCount()
        {
            int saved = 0;
            foreach (bool s in _tabletSaved)
            {
                saved += s ? 1 : 0;
            }

            return saved;
        }

        /// <summary>The window is over: what is still in the hero's arms was not brought up; pay once, count the tablets towards the steps.</summary>
        private void SettleFlood(long index)
        {
            _floodSettled = true;
            HideFlood();
            if (!_floodStarted || DurJarrasFloodRules.IsPaid(GetMark(DurJarrasFloodRules.PaidKey, _floodPaidLocal), index))
            {
                return;
            }

            Say(FinalCityEventsTexts.FloodRecedes);
            int saved = SavedCount();
            if (saved > 0)
            {
                _floodSavedLocal = Math.Max(_floodSavedLocal, GetMark(DurJarrasFloodRules.SavedTotalKey, _floodSavedLocal)) + saved;
                SetMark(DurJarrasFloodRules.SavedTotalKey, _floodSavedLocal);
            }

            PayFlood(index, saved);
            OpenSteps();
        }

        private void PayFlood(long index, int saved)
        {
            if (saved <= 0)
            {
                _floodOwedIndex = -1L;
                _floodPaidLocal = DurJarrasFloodRules.PaidMark(index);
                SetMark(DurJarrasFloodRules.PaidKey, _floodPaidLocal);
                Say(FinalCityEventsTexts.FloodNone);
                return;
            }

            int steps = DurJarrasFloodRules.StepsOpen(GetMark(DurJarrasFloodRules.SavedTotalKey, _floodSavedLocal));
            int gold = DurJarrasFloodRules.GoldFor(saved, steps);
            int stones = DurJarrasFloodRules.StonesFor(saved);
            if (!Grant(gold, stones))
            {
                _floodOwedIndex = index;
                _floodOwedSaved = saved;
                _nextFloodOwedTry = Time.unscaledTime + 5f;
                return;
            }

            _floodOwedIndex = -1L;
            _floodPaidLocal = DurJarrasFloodRules.PaidMark(index);
            SetMark(DurJarrasFloodRules.PaidKey, _floodPaidLocal);
            SayFormat(FinalCityEventsTexts.FloodPaid, saved, gold, stones);
            if (saved >= DurJarrasFloodRules.TabletCount)
            {
                CousinLine("flood");
            }
        }

        /// <summary>The ziggurat's steps the saved tablets opened since the last time: each pays its gold once; the Tower shows the Roda Celeste.</summary>
        private void OpenSteps()
        {
            long total = GetMark(DurJarrasFloodRules.SavedTotalKey, _floodSavedLocal);
            int open = DurJarrasFloodRules.StepsOpen(total);
            long paidStep = Math.Max(1L, GetMark(DurJarrasFloodRules.StepPaidKey, _floodStepPaidLocal));
            for (int step = (int)paidStep + 1; step <= open; step++)
            {
                if (!Grant(DurJarrasFloodRules.GoldForStep(step), 0))
                {
                    return; // the bag is full: the step's gold waits for the next Cheia
                }

                _floodStepPaidLocal = step;
                SetMark(DurJarrasFloodRules.StepPaidKey, step);
                SayFormat(FinalCityEventsTexts.StepOpened, StepName(step), DurJarrasFloodRules.GoldForStep(step));
                if (step >= DurJarrasFloodRules.StepCount)
                {
                    Say(FinalCityEventsTexts.StepTower);
                }
            }

            if (open < DurJarrasFloodRules.StepCount)
            {
                SayFormat(FinalCityEventsTexts.StepNext, DurJarrasFloodRules.TabletsToNextStep(total), StepName(open + 1));
            }
        }

        private string StepName(int step)
        {
            ServiceLocator.TryGet(out LocalizationManager localization);
            return FinalCityEventsTexts.Get(localization, FinalCityEventsTexts.StepKey(step));
        }

        private void ShowFlood(long windowIndex, double t, int step)
        {
            int lying = DurJarrasFloodRules.LyingTablet(t);
            if (lying >= 0 && !_tabletTaken[lying])
            {
                int road = DurJarrasFloodRules.RoadOf(windowIndex, lying);
                Vector2 at = OverworldLayouts.DurJarrasFloodTablets[Mathf.Clamp(road, 0, OverworldLayouts.DurJarrasFloodTablets.Length - 1)];
                Beam(ref _tabletBeam, "Dj_FloodTablet", 0.5f, _durJarrasOrigin + at, TabletBeamHeight, TabletColor);
            }
            else
            {
                Hide(_tabletBeam);
            }

            bool carrying = false;
            for (int i = 0; i < DurJarrasFloodRules.TabletCount; i++)
            {
                carrying |= _tabletTaken[i] && !_tabletSaved[i];
            }

            Beam(ref _zigguratBeam, "Dj_ZigguratFoot", carrying ? 0.7f : 0.35f, _durJarrasOrigin + OverworldLayouts.DurJarrasZigguratFoot, ZigguratBeamHeight, ZigguratColor);
            ShowFloodRing(DurJarrasFloodRules.FloodRadius(step));
        }

        /// <summary>The edge of the water: a ring round the plaza at the flood's radius, a little above the ground.</summary>
        private void ShowFloodRing(float radius)
        {
            if (_floodRing == null)
            {
                _floodRing = NewLine("Dj_FloodEdge", 0.5f, FloodRingPoints);
                _floodRing.loop = true;
            }

            bool wasShown = _floodRing.enabled;
            _floodRing.enabled = true;
            _floodRing.startColor = _floodRing.endColor = FloodColor;
            if (wasShown && Mathf.Approximately(radius, _floodRingRadius))
            {
                return; // the water only moves once a minute
            }

            _floodRingRadius = radius;
            Vector2 centre = _durJarrasOrigin + OverworldLayouts.DurJarrasOffset;
            for (int i = 0; i < FloodRingPoints; i++)
            {
                float a = i * Mathf.PI * 2f / FloodRingPoints;
                Vector2 p = centre + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * radius;
                _floodRing.SetPosition(i, Ground(p) + Vector3.up * FloodRingHeight);
            }
        }

        private void HideFlood()
        {
            Hide(_tabletBeam);
            Hide(_zigguratBeam);
            Hide(_floodRing);
        }
    }
}
