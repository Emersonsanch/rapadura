using System;
using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.World.Map.Overworld;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// The rope lifts and stone chutes of Sangkent's terraces (36, <see cref="SangkentLiftRules"/>): the ropes (brown) and chutes (grey) drawn from
    /// station to station, a pillar at each station (gold when it runs, red while Kustai's bell rings the avalanche). At a lift's foot [E] queues
    /// for the next basket (the hero waits there; stepping off leaves the queue); at a chute's top [E] slides down at once.
    /// </summary>
    public sealed partial class RouteMechanicsDriver
    {
        private const int TerraceLineKey = 300;
        private const int TerraceStationKey = 320;
        private const int TerraceSpot = 300;
        private const float LiftHover = 0.6f;
        private const float ChuteHover = 0.15f;
        private const float RopeLift = 4f;
        private const float QueueHintSeconds = 2f;

        /// <summary>Where the terraces part runs (region coordinates of the Teto): the slope north of Sangkent, from the east road to the Terraços Altos.</summary>
        public static readonly Rect TerraceBounds = Rect.MinMaxRect(-60f, -40f, 180f, 560f);

        private static readonly Color RopeColor = new Color(0.55f, 0.38f, 0.22f, 1f);
        private static readonly Color ChuteColor = new Color(0.7f, 0.72f, 0.75f, 0.9f);
        private static readonly Color StationRunColor = new Color(1f, 0.8f, 0.35f, 1f);
        private static readonly Color StationStopColor = new Color(1f, 0.3f, 0.2f, 1f);

        private int _queuedLine = -1;
        private double _queuedAt;
        private float _nextQueueHint;

        /// <summary>True while the hero stands at a terrace station.</summary>
        private bool TickTerraces(Vector2 local, double now)
        {
            if (!TerraceBounds.Contains(local))
            {
                _queuedLine = -1;
                return false;
            }

            SangkentTerraceLine[] lines = OverworldLayouts.SangkentTerraceLines;
            bool stopped = SangkentLiftRules.Stopped(DateTime.UtcNow);
            ShowTerraces(lines, stopped);
            if (!CanInteract)
            {
                _queuedLine = -1;
                return false;
            }

            if (_queuedLine >= 0)
            {
                TickQueue(local, lines, stopped, now);
                return true;
            }

            int index = SangkentLiftRules.StationAt(lines, local);
            if (index < 0)
            {
                return false;
            }

            SangkentTerraceLine line = lines[index];
            Vector2[] course = { line.From, line.To };
            TerraceCheck check = SangkentLiftRules.Check(line, stopped, CourseOpen(_tetoOrigin, course));
            int spot = TerraceSpot + index;
            switch (check)
            {
                case TerraceCheck.Avalanche:
                    HintKey(spot, RouteMechanicsTexts.TerraceAvalanche);
                    return true;
                case TerraceCheck.ZoneBlocked:
                    HintKey(spot, RouteMechanicsTexts.ZoneBlocked);
                    return true;
                case TerraceCheck.Queue:
                    Hint(spot, Format(RouteMechanicsTexts.LiftPrompt, Mathf.CeilToInt(SangkentLiftRules.SecondsToBasket(now))));
                    break;
                default:
                    HintKey(spot, IsNight() ? RouteMechanicsTexts.ChuteNightPrompt : RouteMechanicsTexts.ChutePrompt);
                    break;
            }

            InteractPromptRegistry.Ping(Time.unscaledTime);
            if (!Pressed)
            {
                return true;
            }

            if (check == TerraceCheck.Queue)
            {
                _queuedLine = index;
                _queuedAt = now;
                _nextQueueHint = Time.unscaledTime + QueueHintSeconds;
                SayFormat(RouteMechanicsTexts.LiftQueued, Mathf.CeilToInt(SangkentLiftRules.SecondsToBasket(now)));
            }
            else if (BeginRide(_tetoOrigin, course, false, SangkentLiftRules.ChuteSpeedFor(IsNight()), ChuteHover, SangkentLiftRules.ChuteDropSpeed,
                RouteMechanicsTexts.TerraceArrived, TerraceNumber(line.To)))
            {
                Say(RouteMechanicsTexts.ChuteDown);
            }

            return true;
        }

        /// <summary>Waiting at a lift's foot for the basket: it leaves on the rope's clock; stepping off, or the bell, empties the queue.</summary>
        private void TickQueue(Vector2 local, SangkentTerraceLine[] lines, bool stopped, double now)
        {
            SangkentTerraceLine line = lines[_queuedLine];
            if ((local - line.From).sqrMagnitude > SangkentLiftRules.QueueLeash * SangkentLiftRules.QueueLeash)
            {
                _queuedLine = -1;
                Say(RouteMechanicsTexts.LiftLeftQueue);
                return;
            }

            if (stopped)
            {
                _queuedLine = -1;
                Say(RouteMechanicsTexts.TerraceAvalanche);
                return;
            }

            if (!SangkentLiftRules.BasketLeft(_queuedAt, now))
            {
                if (Time.unscaledTime >= _nextQueueHint)
                {
                    _nextQueueHint = Time.unscaledTime + QueueHintSeconds;
                    float left = (float)(_queuedAt + SangkentLiftRules.SecondsToBasket(_queuedAt) - now);
                    SayFormat(RouteMechanicsTexts.LiftQueued, Mathf.Max(1, Mathf.CeilToInt(left)));
                }

                return;
            }

            _queuedLine = -1;
            if (BeginRide(_tetoOrigin, new[] { line.From, line.To }, false, SangkentLiftRules.LiftSpeed, LiftHover, 0f, RouteMechanicsTexts.TerraceArrived, TerraceNumber(line.To)))
            {
                Say(RouteMechanicsTexts.LiftUp);
            }
        }

        /// <summary>Which terrace a station is (the foot is the 1st, the north road's bend the 4th, the Terraços Altos the 7th).</summary>
        private static int TerraceNumber(Vector2 station)
        {
            if ((station - OverworldLayouts.SangkentTerraceTop).sqrMagnitude < 1f)
            {
                return 7;
            }

            return (station - OverworldLayouts.SangkentTerraceFoot).sqrMagnitude < 1f ? 1 : 4;
        }

        private void ShowTerraces(SangkentTerraceLine[] lines, bool stopped)
        {
            for (int i = 0; i < lines.Length; i++)
            {
                SangkentTerraceLine line = lines[i];
                GroundSegment(TerraceLineKey + i, _tetoOrigin, line.From, line.To, line.IsLift ? RopeLift : 0.3f, line.IsLift ? 0.18f : 1.2f, line.IsLift ? RopeColor : ChuteColor);
                Pillar(TerraceStationKey + i, _tetoOrigin + line.From, 4.5f, 0.5f, stopped ? StationStopColor : StationRunColor);
            }
        }
    }
}
