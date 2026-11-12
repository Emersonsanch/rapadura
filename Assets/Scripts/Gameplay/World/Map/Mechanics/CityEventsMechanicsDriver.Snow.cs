using System;
using Rapadura.Core.DI;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.World.Map.Overworld;
using Rapadura.Gameplay.WorldEvents;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>The Avalanche of Sangkent (36, <see cref="SangkentAvalancheRules"/>): three snow lanes, a fence of stakes on each, the terraces below.</summary>
    public sealed partial class CityEventsMechanicsDriver
    {
        private const float FenceBeamHeight = 4f;
        private const float LaneLift = 0.2f;

        private static readonly Color FenceIdleColor = new Color(0.75f, 0.85f, 1f, 0.8f);
        private static readonly Color FencePlantedColor = new Color(0.95f, 0.7f, 0.3f, 0.95f);
        private static readonly Color FenceHeldColor = new Color(1f, 0.9f, 0.45f, 1f);
        private static readonly Color LaneColor = new Color(0.7f, 0.85f, 1f, 0.45f);
        private static readonly Color FrontColor = new Color(0.97f, 0.98f, 1f, 0.95f);
        private static readonly Color BuriedColor = new Color(0.93f, 0.95f, 1f, 0.9f);

        private readonly bool[] _planted = new bool[SangkentAvalancheRules.LaneCount];
        private readonly bool[] _resolved = new bool[SangkentAvalancheRules.LaneCount];
        private readonly bool[] _held = new bool[SangkentAvalancheRules.LaneCount];
        private readonly bool[] _laneWarned = new bool[SangkentAvalancheRules.LaneCount];
        private readonly bool[] _laneHit = new bool[SangkentAvalancheRules.LaneCount];
        private readonly float[] _lastProgress = new float[SangkentAvalancheRules.LaneCount];
        private LineRenderer[] _fenceBeams;
        private LineRenderer[] _laneLines;
        private LineRenderer[] _frontLines;
        private LineRenderer[] _buriedLines;

        private long _snowIndex = long.MinValue;
        private long _snowWarned = long.MinValue;
        private bool _snowSettled;
        private long _snowPaidLocal;
        private long _snowOwedIndex = -1L;
        private int _snowOwedSaved;
        private float _nextSnowOwedTry;
        private float _nextSnowHint;

        private void TickSnow(Vector2 local, DateTime utc)
        {
            float distance = Vector2.Distance(local, OverworldLayouts.SangkentOffset);
            if (distance > SangkentAvalancheRules.AlertRange)
            {
                HideSnow();
                return;
            }

            if (_snowOwedIndex >= 0 && Time.unscaledTime >= _nextSnowOwedTry)
            {
                PaySnow(_snowOwedIndex, _snowOwedSaved);
            }

            RealTimeWindow window = SangkentAvalancheRules.WindowAt(utc);
            if (!window.IsOpen)
            {
                HideSnow();
                if (SangkentAvalancheRules.IsWarning(window) && _snowWarned != window.Index)
                {
                    _snowWarned = window.Index;
                    SayFormat(CityEventsTexts.SnowWarning, WorldEventCalendar.FormatCountdown(window.SecondsToChange));
                }

                return;
            }

            if (window.Index != _snowIndex)
            {
                ResetSnow(window.Index);
            }

            double t = SangkentAvalancheRules.SecondsInto(window, utc);
            SangkentTerraceLane[] lanes = OverworldLayouts.SangkentTerraceLanes;
            int planting = -1;
            for (int i = 0; i < lanes.Length && i < SangkentAvalancheRules.LaneCount; i++)
            {
                TickLane(i, lanes[i], local, t, ref planting);
            }

            TickPlanting(planting, local);
            ShowSnow(lanes, t);
            if (!_snowSettled && SangkentAvalancheRules.AllDone(t))
            {
                _snowSettled = true;
                if (!SangkentAvalancheRules.IsPaid(GetMark(SangkentAvalancheRules.PaidKey, _snowPaidLocal), window.Index))
                {
                    int saved = 0;
                    foreach (bool held in _held)
                    {
                        saved += held ? 1 : 0;
                    }

                    PaySnow(window.Index, saved);
                }
            }
        }

        private void ResetSnow(long index)
        {
            _snowIndex = index;
            _snowSettled = false;
            for (int i = 0; i < SangkentAvalancheRules.LaneCount; i++)
            {
                _planted[i] = _resolved[i] = _held[i] = _laneWarned[i] = _laneHit[i] = false;
                _lastProgress[i] = 0f;
            }
        }

        private void TickLane(int i, SangkentTerraceLane lane, Vector2 local, double t, ref int planting)
        {
            SangkentLanePhase phase = SangkentAvalancheRules.PhaseOf(i, t);
            if (phase == SangkentLanePhase.Warning && !_laneWarned[i])
            {
                _laneWarned[i] = true;
                SayLane(i, SangkentAvalancheRules.SecondsToSlide(i, t));
            }

            if ((phase == SangkentLanePhase.Quiet || phase == SangkentLanePhase.Warning) && !_planted[i] && SangkentAvalancheRules.CanPlant(lane.Fence, local))
            {
                planting = i;
            }

            if (phase != SangkentLanePhase.Sliding && phase != SangkentLanePhase.Done)
            {
                return;
            }

            float fenceAlong = SangkentAvalancheRules.StopAlong(lane, true);
            float progress = SangkentAvalancheRules.Progress(i, t);
            if (!_resolved[i] && (progress >= fenceAlong || phase == SangkentLanePhase.Done))
            {
                _resolved[i] = true;
                _held[i] = SangkentAvalancheRules.Holds(_planted[i], lane.Fence, local);
                if (_held[i])
                {
                    int saved = 0;
                    foreach (bool held in _held)
                    {
                        saved += held ? 1 : 0;
                    }

                    SayFormat(CityEventsTexts.SnowHeld, saved);
                }
                else
                {
                    Say(CityEventsTexts.SnowBuried);
                }
            }

            bool stopped = _resolved[i] && _held[i];
            float cap = _resolved[i] ? SangkentAvalancheRules.StopAlong(lane, stopped) : fenceAlong;
            float capped = Mathf.Min(progress, cap);
            if (phase == SangkentLanePhase.Sliding && _laneWarned[i] && !_laneHit[i] && _stats.IsAlive
                && SangkentAvalancheRules.Sweeps(lane, local, _lastProgress[i], capped, stopped))
            {
                _laneHit[i] = true;
                _stats.ApplyHazardDamage(SangkentAvalancheRules.Hit(_stats.CurrentHealth, _stats.MaxHealth));
                Say(CityEventsTexts.SnowHit);
            }

            _lastProgress[i] = capped;
        }

        private void SayLane(int lane, double seconds)
        {
            ServiceLocator.TryGet(out LocalizationManager localization);
            string name = CityEventsTexts.Get(localization, CityEventsTexts.LaneKey(lane));
            SayFormat(CityEventsTexts.SnowLane, name, Mathf.CeilToInt((float)seconds));
        }

        private void TickPlanting(int lane, Vector2 local)
        {
            if (lane < 0 || !_stats.IsAlive)
            {
                return;
            }

            InteractPromptRegistry.Ping(Time.unscaledTime);
            if (Time.unscaledTime >= _nextSnowHint)
            {
                _nextSnowHint = Time.unscaledTime + HintCooldown;
                Say(CityEventsTexts.SnowPlant);
            }

            if (!InteractPressed())
            {
                return;
            }

            _planted[lane] = true;
            _nextSnowHint = Time.unscaledTime + HintCooldown;
            Say(CityEventsTexts.SnowPlanted);
        }

        private void PaySnow(long index, int saved)
        {
            if (saved <= 0)
            {
                _snowOwedIndex = -1L;
                _snowPaidLocal = SangkentAvalancheRules.PaidMark(index);
                SetMark(SangkentAvalancheRules.PaidKey, _snowPaidLocal);
                Say(CityEventsTexts.SnowNone);
                return;
            }

            int gold = SangkentAvalancheRules.GoldFor(saved);
            int stones = SangkentAvalancheRules.StonesFor(saved);
            if (!Grant(gold, stones))
            {
                _snowOwedIndex = index;
                _snowOwedSaved = saved;
                _nextSnowOwedTry = Time.unscaledTime + 5f;
                return;
            }

            _snowOwedIndex = -1L;
            _snowPaidLocal = SangkentAvalancheRules.PaidMark(index);
            SetMark(SangkentAvalancheRules.PaidKey, _snowPaidLocal);
            SayFormat(CityEventsTexts.SnowPaid, saved, gold, stones);
            if (saved >= SangkentAvalancheRules.LaneCount)
            {
                CousinLine("snow");
            }
        }

        private void ShowSnow(SangkentTerraceLane[] lanes, double t)
        {
            int n = lanes.Length;
            _fenceBeams ??= new LineRenderer[n];
            _laneLines ??= new LineRenderer[n];
            _frontLines ??= new LineRenderer[n];
            _buriedLines ??= new LineRenderer[n];
            for (int i = 0; i < n; i++)
            {
                SangkentTerraceLane lane = lanes[i];
                Color fenceColor = _resolved[i] && _held[i] ? FenceHeldColor : _planted[i] ? FencePlantedColor : FenceIdleColor;
                Beam(ref _fenceBeams[i], "Sk_SnowFence", 0.5f, _tetoOrigin + lane.Fence, FenceBeamHeight, fenceColor);

                SangkentLanePhase phase = SangkentAvalancheRules.PhaseOf(i, t);
                Vector2 top = _tetoOrigin + lane.Lane.Top;
                Vector2 bottom = _tetoOrigin + lane.Lane.Bottom;
                if (phase == SangkentLanePhase.Warning || phase == SangkentLanePhase.Sliding)
                {
                    Post(ref _laneLines[i], "Sk_SnowLane", 1.2f, Ground(top) + Vector3.up * LaneLift, Ground(bottom) + Vector3.up * LaneLift, LaneColor);
                }
                else
                {
                    Hide(_laneLines[i]);
                }

                if (phase == SangkentLanePhase.Sliding)
                {
                    Vector2 along = (bottom - top).normalized;
                    Vector2 side = new Vector2(along.y, -along.x) * (lane.Lane.Width * 0.5f);
                    Vector2 front = Vector2.Lerp(top, bottom, _lastProgress[i]);
                    Post(ref _frontLines[i], "Sk_SnowFront", 2.5f, Ground(front - side) + Vector3.up * 0.8f, Ground(front + side) + Vector3.up * 0.8f, FrontColor);
                }
                else
                {
                    Hide(_frontLines[i]);
                }

                if (_resolved[i] && !_held[i])
                {
                    // The buried terrace: a bank of snow across the lane's foot for the rest of the window.
                    Vector2 along = (bottom - top).normalized;
                    Vector2 side = new Vector2(along.y, -along.x) * (lane.Lane.Width * 0.5f);
                    Vector2 foot = bottom - along * 4f;
                    Post(ref _buriedLines[i], "Sk_SnowBuried", 6f, Ground(foot - side) + Vector3.up * 0.3f, Ground(foot + side) + Vector3.up * 0.3f, BuriedColor);
                }
                else
                {
                    Hide(_buriedLines[i]);
                }
            }
        }

        private void HideSnow()
        {
            Hide(_fenceBeams);
            Hide(_laneLines);
            Hide(_frontLines);
            Hide(_buriedLines);
        }
    }
}
