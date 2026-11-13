using System;
using Rapadura.Core.DI;
using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.World.Map.Overworld;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// The monsoon of the Delta de Jhilpur (31, <see cref="JhilpurMonsoonRules"/>): on monsoon days every ford of the two canals is high water
    /// (a blue band across it) and its current shoves the wader back to the bank they came from; at the four road fords a mooring post on each bank
    /// rows the hero over (alone once the lesson is taken, or with Nuhzadag after a wait); Abiesous's school by the root bridge gives the lesson
    /// and, in the monsoon, sends a rower straight down the canal to the Tarchon ford (and back up from there). The lesson stays in the save.
    /// </summary>
    public sealed partial class RouteMechanicsDriver
    {
        private const int FordKey = 200;
        private const int MooringKey = 220;
        private const int SchoolKey = 240;
        private const int RunLandingKey = 241;
        private const int FordSpot = 200;
        private const int MooringSpot = 220;
        private const int SchoolSpot = 240;
        private const int RunLandingSpot = 241;
        private const float FordViewRange = 220f;
        private const float FordTrackRange = 40f;
        private const float RideHover = 0.25f;

        private static readonly Color FloodColor = new Color(0.25f, 0.55f, 0.95f, 0.95f);
        private static readonly Color MooringColor = new Color(0.35f, 0.9f, 0.85f, 1f);
        private static readonly Color SchoolColor = new Color(1f, 0.82f, 0.35f, 1f);

        private int[] _fordSide;
        private int _lastMonsoonDay = -1;
        private bool _lastMonsoon;
        private int _ferryFord = -1;
        private bool _ferryFromA;
        private float _ferryElapsed;
        private bool _lessonOn;
        private float _lessonElapsed;
        private bool _rowerLocal;

        /// <summary>Whether the hero has had the rowing lesson (in the save; without a save service, in this session).</summary>
        private bool IsRower() =>
            _rowerLocal || (ServiceLocator.TryGet(out WorldMapProgress progress) && JhilpurMonsoonRules.Learned(progress.GetTimeMark(JhilpurMonsoonRules.RowingKey)));

        /// <summary>True while the hero stands at a monsoon station.</summary>
        private bool TickMonsoon(Vector2 local)
        {
            if (!OverworldLayouts.DeltaBounds.Contains(local))
            {
                _ferryFord = -1;
                _lessonOn = false;
                _lastMonsoonDay = -1;
                return false;
            }

            int day = GameDay();
            bool monsoon = JhilpurMonsoonRules.IsMonsoon(day);
            if (_lastMonsoonDay >= 0 && monsoon != _lastMonsoon)
            {
                Say(monsoon ? RouteMechanicsTexts.MonsoonStarts : RouteMechanicsTexts.MonsoonEnds);
            }

            _lastMonsoonDay = day;
            _lastMonsoon = monsoon;
            DeltaFord[] fords = OverworldLayouts.DeltaFords;
            ShowMonsoon(local, fords, monsoon);
            if (!_stats.IsAlive)
            {
                _ferryFord = -1;
                _lessonOn = false;
                return false;
            }

            PushFromFlood(local, fords, monsoon);
            if (!CanInteract)
            {
                return false;
            }

            if (_ferryFord >= 0)
            {
                TickFerry(local, fords);
                return true;
            }

            if (_lessonOn)
            {
                TickLesson(local);
                return true;
            }

            return TickSchool(local, monsoon, day) || TickRunLanding(local, monsoon, day) || TickMoorings(local, fords, monsoon, day);
        }

        /// <summary>In the monsoon the ford's current throws the wader back to the bank they came from.</summary>
        private void PushFromFlood(Vector2 local, DeltaFord[] fords, bool monsoon)
        {
            for (int i = 0; i < fords.Length; i++)
            {
                DeltaFord ford = fords[i];
                if (!JhilpurMonsoonRules.InFordWater(ford, local))
                {
                    if ((local - ford.Center).sqrMagnitude <= FordTrackRange * FordTrackRange)
                    {
                        _fordSide[i] = JhilpurMonsoonRules.SideOf(ford, local);
                    }

                    continue;
                }

                if (!monsoon)
                {
                    continue;
                }

                // Unknown side (a respawn or a portal into the water): back to the nearer bank.
                int side = _fordSide[i] != 0 ? _fordSide[i] : JhilpurMonsoonRules.SideOf(ford, local);
                if (_character != null && _character.enabled)
                {
                    GroundedPush.Shove(_character, JhilpurMonsoonRules.PushStep(ford, side, Time.deltaTime));
                }

                SayOnce(ford.HasMoorings ? RouteMechanicsTexts.FordFlooded : RouteMechanicsTexts.FordFloodedNoBoat);
            }
        }

        private bool TickMoorings(Vector2 local, DeltaFord[] fords, bool monsoon, int day)
        {
            // Out of the monsoon the moorings are bare posts on the road (the ford is waded): no line every time a hero walks by.
            bool atA = false;
            int index = monsoon ? JhilpurMonsoonRules.MooringAt(fords, local, out atA) : -1;
            if (index < 0)
            {
                return false;
            }

            DeltaFord ford = fords[index];
            Vector2[] course = FordCourse(ford, atA);
            MooringCheck check = JhilpurMonsoonRules.CheckMooring(monsoon, IsRower(), CourseOpen(_deltaOrigin, course));
            int spot = MooringSpot + index * 2 + (atA ? 0 : 1);
            switch (check)
            {
                case MooringCheck.Shallow:
                    Hint(spot, Format(RouteMechanicsTexts.MooringShallow, JhilpurMonsoonRules.DaysUntilMonsoon(day)));
                    return true;
                case MooringCheck.ZoneBlocked:
                    HintKey(spot, RouteMechanicsTexts.ZoneBlocked);
                    return true;
                case MooringCheck.Row:
                    HintKey(spot, RouteMechanicsTexts.MooringRow);
                    break;
                default:
                    HintKey(spot, RouteMechanicsTexts.MooringFerry);
                    break;
            }

            InteractPromptRegistry.Ping(Time.unscaledTime);
            if (!Pressed)
            {
                return true;
            }

            if (check == MooringCheck.Row)
            {
                BeginRide(_deltaOrigin, course, false, JhilpurMonsoonRules.RowSpeed, RideHover, 0f, RouteMechanicsTexts.Crossed);
            }
            else
            {
                _ferryFord = index;
                _ferryFromA = atA;
                _ferryElapsed = 0f;
                Say(RouteMechanicsTexts.FerryComing);
            }

            return true;
        }

        private static Vector2[] FordCourse(DeltaFord ford, bool fromA) =>
            fromA ? new[] { ford.MooringA, ford.Center, ford.MooringB } : new[] { ford.MooringB, ford.Center, ford.MooringA };

        /// <summary>Waiting on the mooring for Nuhzadag: stepping off sends him back; after the wait he rows the hero over, slowly.</summary>
        private void TickFerry(Vector2 local, DeltaFord[] fords)
        {
            DeltaFord ford = fords[_ferryFord];
            Vector2 mooring = _ferryFromA ? ford.MooringA : ford.MooringB;
            if ((local - mooring).sqrMagnitude > JhilpurMonsoonRules.LessonLeash * JhilpurMonsoonRules.LessonLeash || !JhilpurMonsoonRules.IsMonsoon(GameDay()))
            {
                _ferryFord = -1;
                Say(RouteMechanicsTexts.FerryLeft);
                return;
            }

            _ferryElapsed += Time.deltaTime;
            if (_ferryElapsed < JhilpurMonsoonRules.FerryWaitSeconds)
            {
                return;
            }

            Vector2[] course = FordCourse(ford, _ferryFromA);
            _ferryFord = -1;
            BeginRide(_deltaOrigin, course, false, JhilpurMonsoonRules.FerrySpeed, RideHover, 0f, RouteMechanicsTexts.Crossed);
        }

        /// <summary>Abiesous's school: the lesson (once), then, in the monsoon, the run down the canal.</summary>
        private bool TickSchool(Vector2 local, bool monsoon, int day)
        {
            Vector2 school = OverworldLayouts.JhilpurBoatSchool;
            if ((local - school).sqrMagnitude > JhilpurMonsoonRules.MooringReach * JhilpurMonsoonRules.MooringReach)
            {
                return false;
            }

            if (!IsRower())
            {
                Hint(SchoolSpot, Format(RouteMechanicsTexts.SchoolLearn, Mathf.CeilToInt(JhilpurMonsoonRules.LessonSeconds)));
                InteractPromptRegistry.Ping(Time.unscaledTime);
                if (Pressed)
                {
                    _lessonOn = true;
                    _lessonElapsed = 0f;
                    Say(RouteMechanicsTexts.SchoolLesson);
                }

                return true;
            }

            if (!monsoon)
            {
                Hint(SchoolSpot, Format(RouteMechanicsTexts.RunNotMonsoon, JhilpurMonsoonRules.DaysUntilMonsoon(day)));
                return true;
            }

            Vector2[] run = OverworldLayouts.JhilpurMonsoonRun;
            if (!CourseOpen(_deltaOrigin, run))
            {
                HintKey(SchoolSpot, RouteMechanicsTexts.ZoneBlocked);
                return true;
            }

            HintKey(SchoolSpot, RouteMechanicsTexts.RunPrompt);
            InteractPromptRegistry.Ping(Time.unscaledTime);
            if (Pressed && BeginRide(_deltaOrigin, run, false, JhilpurMonsoonRules.RunDownSpeed, RideHover, 0f, RouteMechanicsTexts.Crossed))
            {
                Say(RouteMechanicsTexts.RunGo);
            }

            return true;
        }

        /// <summary>The run's landing on Tarchon's road: in the monsoon a rower goes back up the canal from here.</summary>
        private bool TickRunLanding(Vector2 local, bool monsoon, int day)
        {
            Vector2 landing = OverworldLayouts.TarchonRunLanding;
            if ((local - landing).sqrMagnitude > JhilpurMonsoonRules.MooringReach * JhilpurMonsoonRules.MooringReach || !monsoon)
            {
                return false;
            }

            if (!IsRower())
            {
                HintKey(RunLandingSpot, RouteMechanicsTexts.RunNeedsLesson);
                return true;
            }

            Vector2[] run = OverworldLayouts.JhilpurMonsoonRun;
            if (!CourseOpen(_deltaOrigin, run))
            {
                HintKey(RunLandingSpot, RouteMechanicsTexts.ZoneBlocked);
                return true;
            }

            HintKey(RunLandingSpot, RouteMechanicsTexts.RunBackPrompt);
            InteractPromptRegistry.Ping(Time.unscaledTime);
            if (Pressed)
            {
                BeginRide(_deltaOrigin, run, true, JhilpurMonsoonRules.RunUpSpeed, RideHover, 0f, RouteMechanicsTexts.Crossed);
            }

            return true;
        }

        private void TickLesson(Vector2 local)
        {
            Vector2 school = OverworldLayouts.JhilpurBoatSchool;
            if ((local - school).sqrMagnitude > JhilpurMonsoonRules.LessonLeash * JhilpurMonsoonRules.LessonLeash)
            {
                _lessonOn = false;
                Say(RouteMechanicsTexts.SchoolLeft);
                return;
            }

            _lessonElapsed += Time.deltaTime;
            if (!JhilpurMonsoonRules.LessonDone(_lessonElapsed))
            {
                return;
            }

            _lessonOn = false;
            _rowerLocal = true;
            if (ServiceLocator.TryGet(out WorldMapProgress progress))
            {
                progress.SetTimeMark(JhilpurMonsoonRules.RowingKey, Math.Max(1L, DateTimeOffset.UtcNow.ToUnixTimeSeconds()));
            }

            _hintSpot = -1;
            Say(RouteMechanicsTexts.SchoolLearned);
        }

        /// <summary>The high water over every ford near the hero (monsoon only), the mooring posts (monsoon only) and Abiesous's post (always).</summary>
        private void ShowMonsoon(Vector2 local, DeltaFord[] fords, bool monsoon)
        {
            float range = FordViewRange * FordViewRange;
            for (int i = 0; i < fords.Length; i++)
            {
                DeltaFord ford = fords[i];
                if (!monsoon || (ford.Center - local).sqrMagnitude > range)
                {
                    continue;
                }

                // The band of high water along the ford (the ford's length, at the water line).
                GroundSegment(FordKey + i, _deltaOrigin, ford.Center - ford.Along * ford.HalfLength, ford.Center + ford.Along * ford.HalfLength, 0.3f, ford.HalfWidth * 0.6f, FloodColor);
                if (ford.HasMoorings)
                {
                    Pillar(MooringKey + i * 2, _deltaOrigin + ford.MooringA, 3.2f, 0.45f, MooringColor);
                    Pillar(MooringKey + i * 2 + 1, _deltaOrigin + ford.MooringB, 3.2f, 0.45f, MooringColor);
                }
            }

            Vector2 school = OverworldLayouts.JhilpurBoatSchool;
            if ((school - local).sqrMagnitude <= range)
            {
                Pillar(SchoolKey, _deltaOrigin + school, 4f, 0.5f, SchoolColor);
            }

            Vector2 landing = OverworldLayouts.TarchonRunLanding;
            if (monsoon && (landing - local).sqrMagnitude <= range)
            {
                Pillar(RunLandingKey, _deltaOrigin + landing, 4f, 0.5f, SchoolColor);
            }
        }
    }
}
