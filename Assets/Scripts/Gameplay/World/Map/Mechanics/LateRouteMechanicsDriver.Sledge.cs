using System.Collections.Generic;
using Rapadura.Core.DI;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.World.Map.Overworld;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// The sledge of the Areal Escondido (73, <see cref="HiddenSandsSledgeRules"/>): a sand-gold pillar at the Trenó de Areia and at the road's end
    /// in the Portas Fechadas, and the three stone doors on the road between (a pillar each side of the road, gold while the sledge goes
    /// through). At either end [E] takes the rope: alone the sledge crawls and every tap of [E] on the way is a heave (a burst that fades), and
    /// once A Grande Serpente do Areal is freed she pulls it at her own pace. The first run of each UTC day down to the doors' end is paid
    /// (one daily mark, written only then). The road itself is always walkable.
    /// </summary>
    public sealed partial class LateRouteMechanicsDriver
    {
        private const int SledgeLineKey = 400;
        private const int SledgeSpot = 400;
        private const float SledgeHover = 0.4f;
        private const float DoorHalfWidth = 4.5f;

        /// <summary>Where the sledge part runs (region coordinates of the Rio): the road south of the Trenó de Areia through the Portas Fechadas.</summary>
        public static readonly Rect SledgeBounds = Rect.MinMaxRect(-1000f, -380f, -920f, 110f);

        private static readonly Color SledgeStationColor = new Color(0.9f, 0.75f, 0.4f, 1f);
        private static readonly Color DoorColor = new Color(0.42f, 0.38f, 0.33f, 1f);
        private static readonly Color DoorGlowColor = new Color(1f, 0.85f, 0.35f, 1f);

        private readonly List<float> _heaves = new List<float>(8);
        private bool _sledgeOn;
        private bool _sledgeBack;
        private bool _sledgeSerpent;
        private int _doorsGlowed;

        /// <summary>True while the hero stands at an end of the sledge road.</summary>
        private bool TickSledge(Vector2 local, double now)
        {
            if (!SledgeBounds.Contains(local))
            {
                return false;
            }

            Vector2[] course = OverworldLayouts.HiddenSandsSledgeCourse;
            Pillar(SledgeLineKey, course[0] + _rioOrigin, 5f, 0.6f, SledgeStationColor);
            Pillar(SledgeLineKey + 1, course[course.Length - 1] + _rioOrigin, 5f, 0.6f, SledgeStationColor);
            ShowDoors(course, -1f);
            if (!CanInteract)
            {
                return false;
            }

            int station = HiddenSandsSledgeRules.StationAt(course, local);
            if (station < 0)
            {
                return false;
            }

            int spot = SledgeSpot + station;
            if (!CourseOpen(_rioOrigin, course))
            {
                HintKey(spot, LateRouteMechanicsTexts.ZoneBlocked);
                return true;
            }

            bool serpent = SerpentFreed();
            HintKey(spot, serpent ? LateRouteMechanicsTexts.SledgeSerpentPrompt : station == 0 ? LateRouteMechanicsTexts.SledgePrompt : LateRouteMechanicsTexts.SledgeBackPrompt);
            InteractPromptRegistry.Ping(Time.unscaledTime);
            if (!Pressed)
            {
                return true;
            }

            if (BeginRide(_rioOrigin, course, station == 1, HiddenSandsSledgeRules.SpeedFor(serpent, 0, 0), 0f, SledgeHover, LateRouteMechanicsTexts.SledgeArrived))
            {
                _sledgeOn = true;
                _sledgeBack = station == 1;
                _sledgeSerpent = serpent;
                _heaves.Clear();
                _doorsGlowed = 0;
                Say(serpent ? LateRouteMechanicsTexts.SledgeSerpent : LateRouteMechanicsTexts.SledgeHaul);
            }

            return true;
        }

        /// <summary>On the sledge: every [E] is a heave, the speed follows the heaves of the last moments, the doors glow as it passes.</summary>
        private void SteerSledge(double now)
        {
            float t = Time.time;
            if (!_sledgeSerpent && Pressed)
            {
                _heaves.Add(t);
            }

            while (_heaves.Count > 0 && t - _heaves[0] > HiddenSandsSledgeRules.HeaveSeconds)
            {
                _heaves.RemoveAt(0);
            }
            _ride.Speed = HiddenSandsSledgeRules.SpeedFor(_sledgeSerpent, HaulingAllies(), _heaves.Count);

            Vector2[] course = OverworldLayouts.HiddenSandsSledgeCourse;
            float length = RidePath.Length(course);
            float along = _ride.Progress * length;
            ShowDoors(course, _sledgeBack ? length - along : along);
        }

        /// <summary>The run is over: paid once a day when it reached the doors' end.</summary>
        private void EndSledge(bool arrived, double now)
        {
            _sledgeOn = false;
            _heaves.Clear();
            if (!arrived || _sledgeBack || !ServiceLocator.TryGet(out WorldMapProgress progress))
            {
                return;
            }

            long today = UtcDay(now);
            if (HiddenSandsSledgeRules.Pays(progress.GetDailyMark(HiddenSandsSledgeRules.PaidKey), today) && PayGold(HiddenSandsSledgeRules.DailyGold))
            {
                progress.SetDailyMark(HiddenSandsSledgeRules.PaidKey, (int)today);
                SayFormat(LateRouteMechanicsTexts.SledgePaid, HiddenSandsSledgeRules.DailyGold);
            }
        }

        /// <summary>
        /// The allies hauling beside the hero. Single-player has none yet: the rules take the count so the server can add the party's heroes
        /// within <see cref="HiddenSandsSledgeRules.AllyReach"/> once the multiplayer comes.
        /// </summary>
        private static int HaulingAllies() => 0;

        private static bool SerpentFreed() =>
            ServiceLocator.TryGet(out DefeatedEnemyRegistry registry) && registry != null && registry.IsDefeated(HiddenSandsSledgeRules.SerpentEnemyId);

        /// <summary>The three doors; <paramref name="sledgeAlong"/> metres down the road from the Trenó lights the ones it passes (−1: no sledge).</summary>
        private void ShowDoors(Vector2[] course, float sledgeAlong)
        {
            Vector2 dir = RidePath.DirectionAt(course, 0f);
            Vector2 right = MechanicsRide.Right(dir);
            float[] doors = OverworldLayouts.HiddenSandsDoorsAlong;
            for (int i = 0; i < doors.Length; i++)
            {
                bool glows = sledgeAlong >= 0f && HiddenSandsSledgeRules.DoorGlows(doors[i], sledgeAlong);
                if (glows && (_doorsGlowed & (1 << i)) == 0)
                {
                    _doorsGlowed |= 1 << i;
                    Say(LateRouteMechanicsTexts.SledgeDoor);
                }

                Vector2 c = RidePath.PointAt(course, doors[i]) + _rioOrigin;
                Color color = glows ? DoorGlowColor : DoorColor;
                Pillar(SledgeLineKey + 10 + i * 2, c - right * DoorHalfWidth, 6f, 1.2f, color);
                Pillar(SledgeLineKey + 11 + i * 2, c + right * DoorHalfWidth, 6f, 1.2f, color);
            }
        }
    }
}
