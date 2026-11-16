using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.World.Map.Overworld;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// The walks along the bottom of Kirta's reef (28, <see cref="KirtaReefWalkRules"/>): a teal pillar at each end of a walk and a pale blue one
    /// over each air bubble. At an end [E] sinks the hero to the coral and walks them along the bottom bubble to bubble (a ride that keeps
    /// <see cref="KirtaReefWalkRules.DiveDepth"/> under the water line where the bottom lies deeper); the line says when the breath is too short
    /// for the longest stretch (and which shell or bubble helps) and, at night, that the sharks close the south walk. Both walks are shortcuts:
    /// the trails out of Kirta go the same way on foot.
    /// </summary>
    public sealed partial class LateRouteMechanicsDriver
    {
        private const int ReefLineKey = 100;
        private const int ReefSpot = 100;

        /// <summary>Where the reef part runs (region coordinates of the Costa): Kirta's atoll, the lagoon north to the islets' ford, the abyss south.</summary>
        public static readonly Rect ReefBounds = Rect.MinMaxRect(820f, -520f, 1080f, 400f);

        private static readonly Color ReefStationColor = new Color(0.25f, 0.85f, 0.8f, 1f);
        private static readonly Color ReefBubbleColor = new Color(0.7f, 0.9f, 1f, 0.75f);

        private bool _diveOn;
        private Vector2[] _diveCourse;
        private bool _diveReverse;
        private float _diveLength;
        private int _diveBreaths;

        /// <summary>True while the hero stands at a dive station.</summary>
        private bool TickReef(Vector2 local)
        {
            if (!ReefBounds.Contains(local))
            {
                return false;
            }

            ReefDive[] dives = OverworldLayouts.KirtaReefDives;
            ShowReef(dives);
            if (!CanInteract)
            {
                return false;
            }

            int station = KirtaReefWalkRules.StationAt(dives, local);
            if (station < 0)
            {
                return false;
            }

            ReefDive dive = dives[station / 2];
            bool reverse = station % 2 == 1;
            MapRequirementState state = _heroState.Get(_bag);
            float breath = KirtaReefWalkRules.BreathSeconds(state.Has(MapRequirementItems.BreathingShell), state.Has(MapRequirementItems.AirBubble));
            ReefDiveCheck check = KirtaReefWalkRules.Check(dive, breath, IsNight(), CourseOpen(_costaOrigin, dive.Course));
            int spot = ReefSpot + station;
            switch (check)
            {
                case ReefDiveCheck.Sharks:
                    HintKey(spot, LateRouteMechanicsTexts.DiveSharks);
                    return true;
                case ReefDiveCheck.NoAir:
                    Hint(spot, Format(LateRouteMechanicsTexts.DiveNoAir, Mathf.CeilToInt(KirtaReefWalkRules.BreathNeeded(dive.Course)), Mathf.FloorToInt(breath)));
                    return true;
                case ReefDiveCheck.ZoneBlocked:
                    HintKey(spot, LateRouteMechanicsTexts.ZoneBlocked);
                    return true;
            }

            HintKey(spot, reverse ? LateRouteMechanicsTexts.DivePromptBack : dive.Abyss ? LateRouteMechanicsTexts.DivePromptSouth : LateRouteMechanicsTexts.DivePromptNorth);
            InteractPromptRegistry.Ping(Time.unscaledTime);
            if (!Pressed)
            {
                return true;
            }

            if (BeginRide(_costaOrigin, dive.Course, reverse, KirtaReefWalkRules.DiveSpeed, 0f, 0f, LateRouteMechanicsTexts.DiveUp))
            {
                _ride.Depth = KirtaReefWalkRules.DiveDepth;
                _diveOn = true;
                _diveCourse = dive.Course;
                _diveReverse = reverse;
                _diveLength = RidePath.Length(dive.Course);
                _diveBreaths = 1;
                Say(LateRouteMechanicsTexts.DiveDown);
            }

            return true;
        }

        /// <summary>On the bottom: a full breath at every bubble passed.</summary>
        private void TickDiveBreaths()
        {
            if (_diveCourse == null)
            {
                return;
            }

            int breaths = KirtaReefWalkRules.BreathsTaken(_diveCourse, _ride.Progress * _diveLength, _diveReverse);
            if (breaths > _diveBreaths)
            {
                _diveBreaths = breaths;
                Say(LateRouteMechanicsTexts.DiveBreath);
            }
        }

        private void ShowReef(ReefDive[] dives)
        {
            int key = ReefLineKey;
            foreach (ReefDive dive in dives)
            {
                for (int i = 0; i < dive.Course.Length; i++)
                {
                    bool end = i == 0 || i == dive.Course.Length - 1;
                    Pillar(key++, dive.Course[i] + _costaOrigin, end ? 5f : 3f, end ? 0.5f : 0.8f, end ? ReefStationColor : ReefBubbleColor);
                }
            }
        }
    }
}
