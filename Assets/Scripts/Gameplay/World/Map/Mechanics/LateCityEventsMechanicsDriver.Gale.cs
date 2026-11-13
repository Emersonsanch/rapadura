using System;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.World.Map.Overworld;
using Rapadura.Gameplay.WorldEvents;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>The Vendaval of Farnkesh (54, <see cref="FarnkeshGaleRules"/>): five gusts, three bridge moorings, the wind turning between them.</summary>
    public sealed partial class LateCityEventsMechanicsDriver
    {
        private const float MooringBeamHeight = 4.5f;

        private static readonly Color MooringIdleColor = new Color(0.85f, 0.9f, 1f, 0.55f);
        private static readonly Color MooringCalledColor = new Color(0.55f, 0.85f, 1f, 0.95f);
        private static readonly Color MooringGrabbedColor = new Color(0.95f, 0.8f, 0.4f, 1f);
        private static readonly Color GustColor = new Color(0.9f, 0.97f, 1f, 0.9f);

        private readonly bool[] _galeGrabbed = new bool[FarnkeshGaleRules.GustCount];
        private readonly bool[] _galeResolved = new bool[FarnkeshGaleRules.GustCount];
        private readonly bool[] _galeHeld = new bool[FarnkeshGaleRules.GustCount];
        private readonly bool[] _galeWarned = new bool[FarnkeshGaleRules.GustCount];
        private LineRenderer[] _mooringBeams;
        private LineRenderer _gustLine;

        private long _galeIndex = long.MinValue;
        private long _galeWarnedWindow = long.MinValue;
        private bool _galeStarted;
        private bool _galeSettled;
        private long _galePaidLocal;
        private long _galeOwedIndex = -1L;
        private int _galeOwedHeld;
        private float _nextGaleOwedTry;
        private float _nextGaleHint;

        private void TickGale(Vector2 local, DateTime utc)
        {
            float distance = Vector2.Distance(local, OverworldLayouts.FarnkeshOffset);
            if (distance > FarnkeshGaleRules.AlertRange)
            {
                HideGale();
                return;
            }

            if (_galeOwedIndex >= 0 && Time.unscaledTime >= _nextGaleOwedTry)
            {
                PayGale(_galeOwedIndex, _galeOwedHeld);
            }

            RealTimeWindow window = FarnkeshGaleRules.WindowAt(utc);
            if (!window.IsOpen)
            {
                HideGale();
                if (FarnkeshGaleRules.IsWarning(window) && _galeWarnedWindow != window.Index)
                {
                    _galeWarnedWindow = window.Index;
                    SayFormat(LateCityEventsTexts.GaleWarning, WorldEventCalendar.FormatCountdown(window.SecondsToChange));
                }

                return;
            }

            if (window.Index != _galeIndex)
            {
                ResetGale(window.Index);
            }

            if (!_galeStarted)
            {
                _galeStarted = true;
                Say(LateCityEventsTexts.GaleStart);
            }

            double t = FarnkeshGaleRules.SecondsInto(window, utc);
            Vector2[] moorings = OverworldLayouts.FarnkeshBridgeMoorings;
            for (int gust = 0; gust < FarnkeshGaleRules.GustCount; gust++)
            {
                TickGust(window.Index, gust, moorings, local, t);
            }

            ShowGale(window.Index, moorings, t);
            if (!_galeSettled && FarnkeshGaleRules.AllDone(t))
            {
                _galeSettled = true;
                if (Array.IndexOf(_galeWarned, true) >= 0 && !FarnkeshGaleRules.IsPaid(GetMark(FarnkeshGaleRules.PaidKey, _galePaidLocal), window.Index))
                {
                    PayGale(window.Index, HeldCount());
                }
            }
        }

        private void ResetGale(long index)
        {
            _galeIndex = index;
            _galeStarted = false;
            _galeSettled = false;
            for (int i = 0; i < FarnkeshGaleRules.GustCount; i++)
            {
                _galeGrabbed[i] = _galeResolved[i] = _galeHeld[i] = _galeWarned[i] = false;
            }
        }

        private int HeldCount()
        {
            int held = 0;
            foreach (bool h in _galeHeld)
            {
                held += h ? 1 : 0;
            }

            return held;
        }

        private void TickGust(long windowIndex, int gust, Vector2[] moorings, Vector2 local, double t)
        {
            GalePhase phase = FarnkeshGaleRules.PhaseOf(gust, t);
            int side = FarnkeshGaleRules.MooringOf(windowIndex, gust);
            Vector2 mooring = moorings[Mathf.Clamp(side, 0, moorings.Length - 1)];
            if (phase == GalePhase.Warning)
            {
                if (!_galeWarned[gust])
                {
                    _galeWarned[gust] = true;
                    SayFormat(LateCityEventsTexts.GaleGust, SideName(side), Mathf.CeilToInt((float)FarnkeshGaleRules.SecondsToGust(gust, t)), gust + 1);
                }

                if (!_galeGrabbed[gust] && _stats.IsAlive && FarnkeshGaleRules.CanGrab(mooring, local, true))
                {
                    TickGrab(gust, mooring, local);
                }

                return;
            }

            if (phase != GalePhase.Blowing && phase != GalePhase.Done)
            {
                return;
            }

            if (_galeResolved[gust])
            {
                return;
            }

            // The gust comes: it is decided once, at its first frame (or later, when the hero came in after it blew).
            _galeResolved[gust] = true;
            _galeHeld[gust] = FarnkeshGaleRules.Holds(_galeGrabbed[gust], mooring, local);
            if (!_galeWarned[gust])
            {
                return; // blew before the hero came near: nothing to say
            }

            if (_galeHeld[gust])
            {
                SayFormat(LateCityEventsTexts.GaleHeld, HeldCount());
                return;
            }

            SayFormat(LateCityEventsTexts.GaleSwung, SideName(side));
            if (phase == GalePhase.Blowing && _stats.IsAlive && FarnkeshGaleRules.Thrown(false, mooring, local))
            {
                _stats.ApplyHazardDamage(FarnkeshGaleRules.Hit(_stats.CurrentHealth, _stats.MaxHealth));
                Say(LateCityEventsTexts.GaleThrown);
            }
        }

        /// <summary>The hero is within Maithe's reach of the called mooring: the real reach depends on who is playing.</summary>
        private void TickGrab(int gust, Vector2 mooring, Vector2 local)
        {
            if (!FarnkeshGaleRules.CanGrab(mooring, local, Hero() == CharacterId.Maithe))
            {
                return;
            }

            InteractPromptRegistry.Ping(Time.unscaledTime);
            if (Time.unscaledTime >= _nextGaleHint)
            {
                _nextGaleHint = Time.unscaledTime + HintCooldown;
                Say(LateCityEventsTexts.GaleGrab);
            }

            if (!InteractPressed())
            {
                return;
            }

            _galeGrabbed[gust] = true;
            _nextGaleHint = Time.unscaledTime + HintCooldown;
            Say(LateCityEventsTexts.GaleGrabbed);
        }

        private void PayGale(long index, int held)
        {
            if (held <= 0)
            {
                _galeOwedIndex = -1L;
                _galePaidLocal = FarnkeshGaleRules.PaidMark(index);
                SetMark(FarnkeshGaleRules.PaidKey, _galePaidLocal);
                Say(LateCityEventsTexts.GaleNone);
                return;
            }

            int gold = FarnkeshGaleRules.GoldFor(held);
            int stones = FarnkeshGaleRules.StonesFor(held);
            if (!Grant(gold, stones))
            {
                _galeOwedIndex = index;
                _galeOwedHeld = held;
                _nextGaleOwedTry = Time.unscaledTime + 5f;
                return;
            }

            _galeOwedIndex = -1L;
            _galePaidLocal = FarnkeshGaleRules.PaidMark(index);
            SetMark(FarnkeshGaleRules.PaidKey, _galePaidLocal);
            SayFormat(LateCityEventsTexts.GalePaid, held, gold, stones);
            if (held >= FarnkeshGaleRules.GustCount)
            {
                CousinLine("gale");
            }
        }

        private void ShowGale(long windowIndex, Vector2[] moorings, double t)
        {
            _mooringBeams ??= new LineRenderer[moorings.Length];
            int gust = FarnkeshGaleRules.CurrentGust(t);
            int called = gust >= 0 ? FarnkeshGaleRules.MooringOf(windowIndex, gust) : -1;
            for (int i = 0; i < moorings.Length; i++)
            {
                Color color = i != called ? MooringIdleColor : _galeGrabbed[gust] ? MooringGrabbedColor : MooringCalledColor;
                Beam(ref _mooringBeams[i], "Fk_GaleMooring", i == called ? 0.6f : 0.35f, _farnkeshOrigin + moorings[i], MooringBeamHeight, color);
            }

            if (gust >= 0 && FarnkeshGaleRules.PhaseOf(gust, t) == GalePhase.Blowing && called >= 0)
            {
                // The gust: a long streak of wind across the called bridge, from the plaza's side out over the mooring.
                Vector2 mooring = _farnkeshOrigin + moorings[called];
                Vector2 town = _farnkeshOrigin + OverworldLayouts.FarnkeshOffset;
                Vector2 across = (mooring - town).normalized;
                Vector2 side = new Vector2(across.y, -across.x) * FarnkeshGaleRules.SwingRadius;
                Post(ref _gustLine, "Fk_GaleGust", 1.4f, Ground(mooring - side) + Vector3.up * 2.5f, Ground(mooring + side) + Vector3.up * 2.5f, GustColor);
            }
            else
            {
                Hide(_gustLine);
            }
        }

        private void HideGale()
        {
            Hide(_mooringBeams);
            Hide(_gustLine);
        }
    }
}
