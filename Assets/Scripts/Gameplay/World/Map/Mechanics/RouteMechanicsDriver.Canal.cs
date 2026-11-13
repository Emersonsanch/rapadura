using System.Collections.Generic;
using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.World.Map.Overworld;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// The Canal das Lanternas: the drum's barges (41, <see cref="DrumBargeRules"/>) and the four locks (43, <see cref="CanalLockRules"/>). The
    /// barges are red pillars moving on the timetable along the canal; at the two landings [E] boards the next one (the hero waits on the landing
    /// until the drum, then rides). Each lock gate is a line across the canal, gold and walkable when shut, thin and blue when open; at either end
    /// [E] turns the keeper's wheel (by day) or walks over the shut gate.
    /// </summary>
    public sealed partial class RouteMechanicsDriver
    {
        private const int GateKey = 400;
        private const int GateEndKey = 420;
        private const int LandingKey = 440;
        private const int BargeKey = 450;
        private const int LandingSpot = 440;
        private const int GateSpot = 420;
        private const float GateLift = 0.9f;
        private const float BargeHover = 0.4f;
        private const float BargeHeight = 3.2f;

        /// <summary>Where the canal part runs (region coordinates of Mingcheng): along the Canal das Lanternas from the east bridge to the carp's lake.</summary>
        public static readonly Rect CanalBounds = Rect.MinMaxRect(300f, 0f, 700f, 1100f);

        private static readonly Color GateShutColor = new Color(1f, 0.8f, 0.3f, 1f);
        private static readonly Color GateOpenColor = new Color(0.35f, 0.6f, 0.95f, 0.6f);
        private static readonly Color LandingColor = new Color(0.95f, 0.3f, 0.2f, 1f);
        private static readonly Color BargeColor = new Color(0.9f, 0.2f, 0.15f, 1f);

        private readonly List<float> _northBarges = new List<float>();
        private readonly List<float> _southBarges = new List<float>();
        private double[] _wheelUntil;
        private float _bargeCourseLength;
        private int _boardedLine = -1;
        private int _boardedDeparture;

        /// <summary>True while the hero stands at a landing or a gate's end.</summary>
        private bool TickCanal(Vector2 local, double now)
        {
            if (!CanalBounds.Contains(local))
            {
                _boardedLine = -1;
                return false;
            }

            float hour = GameHour();
            float secondsPerHour = SecondsPerGameHour();
            DrumBargeRules.UnderWay(BargeLine.North, hour, secondsPerHour, _bargeCourseLength, _northBarges);
            DrumBargeRules.UnderWay(BargeLine.South, hour, secondsPerHour, _bargeCourseLength, _southBarges);
            CanalLock[] locks = OverworldLayouts.CanalLocks;
            ShowCanal(locks, hour, now);
            if (!CanInteract)
            {
                _boardedLine = -1;
                return false;
            }

            if (_boardedLine >= 0)
            {
                TickBoarded(local, hour);
                return true;
            }

            return TickLanding(local, BargeLine.North, OverworldLayouts.DrumBargeSouthLanding, hour)
                || TickLanding(local, BargeLine.South, OverworldLayouts.DrumBargeNorthLanding, hour)
                || TickGates(local, locks, hour, now);
        }

        private bool TickLanding(Vector2 local, BargeLine line, Vector2 landing, float hour)
        {
            if ((local - landing).sqrMagnitude > DrumBargeRules.LandingReach * DrumBargeRules.LandingReach)
            {
                return false;
            }

            int spot = LandingSpot + (int)line;
            int departure = DrumBargeRules.BoardingDeparture(line, hour);
            if (departure < 0)
            {
                int next = DrumBargeRules.NextDeparture(line, hour);
                if (next >= 0 && DrumBargeRules.IsDrumHour(hour))
                {
                    Hint(spot, Format(RouteMechanicsTexts.BargeNext, next));
                }
                else
                {
                    HintKey(spot, RouteMechanicsTexts.BargeNight);
                }

                return true;
            }

            if (!CourseOpen(_mingOrigin, OverworldLayouts.DrumBargeCourse))
            {
                HintKey(spot, RouteMechanicsTexts.ZoneBlocked);
                return true;
            }

            Hint(spot, Format(RouteMechanicsTexts.BargePrompt, departure));
            InteractPromptRegistry.Ping(Time.unscaledTime);
            if (Pressed)
            {
                _boardedLine = (int)line;
                _boardedDeparture = departure;
                SayFormat(RouteMechanicsTexts.BargeBoarded, departure);
            }

            return true;
        }

        /// <summary>On board at the landing: the barge leaves with the drum; stepping off the landing gets off.</summary>
        private void TickBoarded(Vector2 local, float hour)
        {
            var line = (BargeLine)_boardedLine;
            Vector2 landing = line == BargeLine.North ? OverworldLayouts.DrumBargeSouthLanding : OverworldLayouts.DrumBargeNorthLanding;
            if ((local - landing).sqrMagnitude > DrumBargeRules.BoardLeash * DrumBargeRules.BoardLeash || hour < _boardedDeparture - DrumBargeRules.BoardLeadHours - 0.05f)
            {
                _boardedLine = -1;
                Say(RouteMechanicsTexts.BargeLeft);
                return;
            }

            if (hour < _boardedDeparture)
            {
                return;
            }

            _boardedLine = -1;
            if (BeginRide(_mingOrigin, OverworldLayouts.DrumBargeCourse, line == BargeLine.South, DrumBargeRules.Speed, BargeHover, 0f, RouteMechanicsTexts.BargeArrived))
            {
                Say(RouteMechanicsTexts.BargeGo);
            }
        }

        private bool TickGates(Vector2 local, CanalLock[] locks, float hour, double now)
        {
            int index = CanalLockRules.EndAt(locks, local, out bool west);
            if (index < 0)
            {
                return false;
            }

            CanalLock gate = locks[index];
            bool bargeNear = BargeInLock(gate);
            bool shut = CanalLockRules.IsShut(hour, now, _wheelUntil[index], bargeNear);
            Vector2[] course = west ? new[] { gate.WestEnd, gate.Center, gate.EastEnd } : new[] { gate.EastEnd, gate.Center, gate.WestEnd };
            LockCheck check = CanalLockRules.Check(shut, bargeNear, !shut || CourseOpen(_mingOrigin, course));
            int spot = GateSpot + index * 2 + (west ? 0 : 1);
            switch (check)
            {
                case LockCheck.BargePassing:
                    HintKey(spot, RouteMechanicsTexts.LockBarge);
                    return true;
                case LockCheck.ZoneBlocked:
                    HintKey(spot, RouteMechanicsTexts.ZoneBlocked);
                    return true;
                case LockCheck.Turn:
                    Hint(spot, Format(RouteMechanicsTexts.LockTurn, Mathf.RoundToInt(CanalLockRules.ShutSeconds)));
                    break;
                default:
                    HintKey(spot, CanalLockRules.BellShut(hour) ? RouteMechanicsTexts.LockBell : RouteMechanicsTexts.LockCross);
                    break;
            }

            InteractPromptRegistry.Ping(Time.unscaledTime);
            if (!Pressed)
            {
                return true;
            }

            if (check == LockCheck.Turn)
            {
                _wheelUntil[index] = now + CanalLockRules.ShutSeconds;
                _hintSpot = -1;
                Say(RouteMechanicsTexts.LockShutNow);
                return true;
            }

            // A crossing started on a gate the wheel holds keeps it shut until the hero is over.
            float crossing = RidePath.Length(course) / CanalLockRules.CrossSpeed + 1f;
            if (!CanalLockRules.BellShut(hour) && _wheelUntil[index] < now + crossing)
            {
                _wheelUntil[index] = now + crossing;
            }

            BeginRide(_mingOrigin, course, false, CanalLockRules.CrossSpeed, GateLift, 0f, RouteMechanicsTexts.Crossed);
            return true;
        }

        /// <summary>Whether a barge of either line is in <paramref name="gate"/> now (the locks north of the barges' course never have one).</summary>
        private bool BargeInLock(CanalLock gate)
        {
            if (gate.CourseDistance < 0f)
            {
                return false;
            }

            foreach (float d in _northBarges)
            {
                if (CanalLockRules.BargeNear(d, gate.CourseDistance))
                {
                    return true;
                }
            }

            foreach (float d in _southBarges)
            {
                if (CanalLockRules.BargeNear(_bargeCourseLength - d, gate.CourseDistance))
                {
                    return true;
                }
            }

            return false;
        }

        private void ShowCanal(CanalLock[] locks, float hour, double now)
        {
            for (int i = 0; i < locks.Length; i++)
            {
                CanalLock gate = locks[i];
                bool shut = CanalLockRules.IsShut(hour, now, _wheelUntil[i], BargeInLock(gate));
                GroundSegment(GateKey + i, _mingOrigin, gate.WestEnd, gate.EastEnd, GateLift, shut ? 1.4f : 0.25f, shut ? GateShutColor : GateOpenColor);
                Pillar(GateEndKey + i * 2, _mingOrigin + gate.WestEnd, 2.6f, 0.45f, GateShutColor);
                Pillar(GateEndKey + i * 2 + 1, _mingOrigin + gate.EastEnd, 2.6f, 0.45f, GateShutColor);
            }

            Pillar(LandingKey, _mingOrigin + OverworldLayouts.DrumBargeSouthLanding, 4f, 0.5f, LandingColor);
            Pillar(LandingKey + 1, _mingOrigin + OverworldLayouts.DrumBargeNorthLanding, 4f, 0.5f, LandingColor);
            Vector2[] course = OverworldLayouts.DrumBargeCourse;
            for (int k = 0; k < 7; k++)
            {
                if (k < _northBarges.Count)
                {
                    Pillar(BargeKey + k, _mingOrigin + RidePath.PointAt(course, _northBarges[k]), BargeHeight, 1.6f, BargeColor);
                }

                if (k < _southBarges.Count)
                {
                    Pillar(BargeKey + 10 + k, _mingOrigin + RidePath.PointAt(course, _bargeCourseLength - _southBarges[k]), BargeHeight, 1.6f, BargeColor);
                }
            }
        }
    }
}
