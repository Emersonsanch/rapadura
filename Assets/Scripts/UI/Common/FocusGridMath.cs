using System.Collections.Generic;
using UnityEngine;

namespace Rapadura.UI.Common
{
    /// <summary>Direction of a gamepad D-pad / arrow-key focus move (mirrors NavigationMoveEvent.Direction).</summary>
    public enum FocusDirection
    {
        Up,
        Down,
        Left,
        Right,
        Next,
        Previous,
    }

    /// <summary>
    /// Pure (panel-free) spatial focus math used by <see cref="FocusNavigator"/>: given the focused
    /// element's rect and every focusable candidate's rect (panel/world space, y grows downwards like
    /// UI Toolkit), picks the candidate a D-pad press should move to. Works for grids (inventory
    /// slots, skill nodes), vertical lists (quest/achievement rows) and mixed layouts (a list panel
    /// next to a grid) without each screen describing its layout. Kept separate so EditMode tests can
    /// cover it without a UIDocument, same rationale as <see cref="TooltipPositioning"/>.
    /// </summary>
    public static class FocusGridMath
    {
        /// <summary>Candidates whose center is less than this far along the move axis are "not in that direction".</summary>
        public const float DirectionEpsilon = 1f;

        /// <summary>
        /// Weight of the cross-axis gap: a candidate that doesn't line up with the current element
        /// (e.g. diagonal in a grid) must be this many times closer to win over one that lines up.
        /// </summary>
        public const float CrossAxisWeight = 2f;

        /// <summary>
        /// Index in <paramref name="candidates"/> to move to from <paramref name="current"/> in
        /// <paramref name="direction"/>, or -1 when nothing lies that way (focus then stays put —
        /// it never wraps or leaves the screen). <paramref name="currentIndex"/> (the focused element's
        /// own index in the list, or -1) is skipped. Next/Previous follow list order and wrap, like Tab.
        /// </summary>
        public static int FindNext(Rect current, IReadOnlyList<Rect> candidates, FocusDirection direction, int currentIndex = -1)
        {
            if (candidates == null || candidates.Count == 0)
            {
                return -1;
            }

            if (direction == FocusDirection.Next || direction == FocusDirection.Previous)
            {
                return StepInOrder(currentIndex, candidates.Count, direction == FocusDirection.Next ? 1 : -1);
            }

            Vector2 from = current.center;
            int best = -1;
            float bestScore = float.MaxValue;

            for (int i = 0; i < candidates.Count; i++)
            {
                if (i == currentIndex)
                {
                    continue;
                }

                Rect candidate = candidates[i];
                Vector2 to = candidate.center;
                float primary;
                float crossGap;
                float crossCenterDelta;

                switch (direction)
                {
                    case FocusDirection.Right:
                        primary = to.x - from.x;
                        crossGap = SpanGap(current.yMin, current.yMax, candidate.yMin, candidate.yMax);
                        crossCenterDelta = Mathf.Abs(to.y - from.y);
                        break;
                    case FocusDirection.Left:
                        primary = from.x - to.x;
                        crossGap = SpanGap(current.yMin, current.yMax, candidate.yMin, candidate.yMax);
                        crossCenterDelta = Mathf.Abs(to.y - from.y);
                        break;
                    case FocusDirection.Down:
                        primary = to.y - from.y;
                        crossGap = SpanGap(current.xMin, current.xMax, candidate.xMin, candidate.xMax);
                        crossCenterDelta = Mathf.Abs(to.x - from.x);
                        break;
                    default: // Up
                        primary = from.y - to.y;
                        crossGap = SpanGap(current.xMin, current.xMax, candidate.xMin, candidate.xMax);
                        crossCenterDelta = Mathf.Abs(to.x - from.x);
                        break;
                }

                if (primary < DirectionEpsilon)
                {
                    continue;
                }

                // Tiny center-delta term only breaks ties between equally-aligned candidates
                // (e.g. a wide row below two narrow tiles: prefer the one under the current center).
                float score = primary + crossGap * CrossAxisWeight + crossCenterDelta * 0.01f;

                if (score < bestScore)
                {
                    bestScore = score;
                    best = i;
                }
            }

            return best;
        }

        /// <summary>Next/previous index in list order, wrapping. From "no current" (-1) Next gives 0 and Previous the last.</summary>
        public static int StepInOrder(int currentIndex, int count, int step)
        {
            if (count <= 0)
            {
                return -1;
            }

            if (currentIndex < 0 || currentIndex >= count)
            {
                return step >= 0 ? 0 : count - 1;
            }

            return ((currentIndex + step) % count + count) % count;
        }

        /// <summary>Distance between two 1D spans, 0 when they overlap.</summary>
        public static float SpanGap(float aMin, float aMax, float bMin, float bMax)
        {
            if (bMin > aMax)
            {
                return bMin - aMax;
            }

            if (aMin > bMax)
            {
                return aMin - bMax;
            }

            return 0f;
        }
    }
}
