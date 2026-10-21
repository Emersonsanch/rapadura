using System.Collections.Generic;
using NUnit.Framework;
using Rapadura.UI.Common;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// EditMode tests for <see cref="FocusGridMath"/> — the spatial D-pad/arrow-key navigation math
    /// behind <see cref="FocusNavigator"/> (pure, no panel): grids, lists, mixed side-by-side panels,
    /// edges (no wrap) and Tab-style Next/Previous order.
    /// </summary>
    public class FocusGridMathTests
    {
        private const float Tile = 72f;
        private const float Gap = 6f;

        /// <summary>3 columns x 3 rows grid, row-major (index = row * 3 + column), like the backpack tiles.</summary>
        private static List<Rect> Grid(int columns, int rows, float originX = 0f, float originY = 0f)
        {
            var rects = new List<Rect>();

            for (int row = 0; row < rows; row++)
            {
                for (int column = 0; column < columns; column++)
                {
                    rects.Add(new Rect(originX + column * (Tile + Gap), originY + row * (Tile + Gap), Tile, Tile));
                }
            }

            return rects;
        }

        private static int Move(List<Rect> rects, int from, FocusDirection direction)
        {
            return FocusGridMath.FindNext(rects[from], rects, direction, from);
        }

        [Test]
        public void Grid_ArrowsMoveToOrthogonalNeighbours()
        {
            List<Rect> grid = Grid(3, 3);
            int center = 4;

            Assert.AreEqual(1, Move(grid, center, FocusDirection.Up));
            Assert.AreEqual(7, Move(grid, center, FocusDirection.Down));
            Assert.AreEqual(3, Move(grid, center, FocusDirection.Left));
            Assert.AreEqual(5, Move(grid, center, FocusDirection.Right));
        }

        [Test]
        public void Grid_AtEdges_ReturnsMinusOne_NoWrap()
        {
            List<Rect> grid = Grid(3, 3);

            Assert.AreEqual(-1, Move(grid, 0, FocusDirection.Up));
            Assert.AreEqual(-1, Move(grid, 0, FocusDirection.Left));
            Assert.AreEqual(-1, Move(grid, 8, FocusDirection.Down));
            Assert.AreEqual(-1, Move(grid, 8, FocusDirection.Right));
            Assert.AreEqual(-1, Move(grid, 2, FocusDirection.Right), "last column doesn't jump to the next row");
        }

        [Test]
        public void Grid_IncompleteLastRow_DownFromColumnWithoutTileBelow_PicksNearestBelow()
        {
            // 3 columns, 7 items: last row has only index 6 (column 0).
            List<Rect> grid = Grid(3, 3).GetRange(0, 7);

            Assert.AreEqual(6, Move(grid, 5, FocusDirection.Down));
            Assert.AreEqual(6, Move(grid, 3, FocusDirection.Down));
        }

        [Test]
        public void Grid_PrefersAlignedNeighbourOverCloserDiagonal()
        {
            // Current at (0,0); one tile straight below but far, one diagonal and closer.
            var current = new Rect(0f, 0f, Tile, Tile);
            var rects = new List<Rect>
            {
                current,
                new Rect(0f, 200f, Tile, Tile),          // aligned, 200 below
                new Rect(Tile + 40f, 150f, Tile, Tile),  // diagonal, 150 below (closer) but 40px off
            };

            Assert.AreEqual(1, FocusGridMath.FindNext(current, rects, FocusDirection.Down, 0));
        }

        [Test]
        public void VerticalList_UpDownStepRows_LeftRightFindNothing()
        {
            var rows = new List<Rect>();

            for (int i = 0; i < 4; i++)
            {
                rows.Add(new Rect(0f, i * 50f, 300f, 44f));
            }

            Assert.AreEqual(2, Move(rows, 1, FocusDirection.Down));
            Assert.AreEqual(0, Move(rows, 1, FocusDirection.Up));
            Assert.AreEqual(-1, Move(rows, 1, FocusDirection.Left));
            Assert.AreEqual(-1, Move(rows, 1, FocusDirection.Right));
        }

        [Test]
        public void SideBySidePanels_LeftFromGrid_EntersListRowAtSameHeight()
        {
            // Equipment list (x 0..200, rows 44px every 50px) next to a backpack grid starting at x 220.
            var rects = new List<Rect>();

            for (int i = 0; i < 8; i++)
            {
                rects.Add(new Rect(0f, i * 50f, 200f, 44f));
            }

            List<Rect> grid = Grid(4, 3, originX: 220f, originY: 0f);
            int gridStart = rects.Count;
            rects.AddRange(grid);

            int secondRowFirstTile = gridStart + 4; // y 78..150
            int target = FocusGridMath.FindNext(rects[secondRowFirstTile], rects, FocusDirection.Left, secondRowFirstTile);

            Assert.That(target, Is.InRange(1, 2), "a list row overlapping the tile's height (y 50..94 or 100..144)");

            int back = FocusGridMath.FindNext(rects[target], rects, FocusDirection.Right, target);
            Assert.That(back, Is.GreaterThanOrEqualTo(gridStart), "Right from the list goes back into the grid");
        }

        [Test]
        public void HeaderButtons_DownFromCloseButton_GoesIntoContent()
        {
            var close = new Rect(500f, 0f, 44f, 44f);
            var sort = new Rect(400f, 0f, 90f, 44f);
            var rects = new List<Rect> { sort, close };
            rects.AddRange(Grid(6, 2, originX: 40f, originY: 80f));

            int down = FocusGridMath.FindNext(close, rects, FocusDirection.Down, 1);
            Assert.GreaterOrEqual(down, 2);
            Assert.AreEqual(0, FocusGridMath.FindNext(close, rects, FocusDirection.Left, 1), "Left from close reaches Organizar");
        }

        [Test]
        public void NextPrevious_FollowListOrderAndWrap()
        {
            List<Rect> grid = Grid(3, 1);

            Assert.AreEqual(1, Move(grid, 0, FocusDirection.Next));
            Assert.AreEqual(0, Move(grid, 2, FocusDirection.Next));
            Assert.AreEqual(2, Move(grid, 0, FocusDirection.Previous));
            Assert.AreEqual(0, FocusGridMath.StepInOrder(-1, 3, 1), "from nothing focused, Next starts at the first");
            Assert.AreEqual(2, FocusGridMath.StepInOrder(-1, 3, -1));
            Assert.AreEqual(-1, FocusGridMath.StepInOrder(0, 0, 1));
        }

        [Test]
        public void FindNext_NoCandidates_ReturnsMinusOne()
        {
            Assert.AreEqual(-1, FocusGridMath.FindNext(new Rect(0f, 0f, 10f, 10f), new List<Rect>(), FocusDirection.Down));
            Assert.AreEqual(-1, FocusGridMath.FindNext(new Rect(0f, 0f, 10f, 10f), null, FocusDirection.Down));
        }

        [Test]
        public void SpanGap_ZeroWhenOverlapping_DistanceOtherwise()
        {
            Assert.AreEqual(0f, FocusGridMath.SpanGap(0f, 10f, 5f, 20f));
            Assert.AreEqual(5f, FocusGridMath.SpanGap(0f, 10f, 15f, 20f));
            Assert.AreEqual(5f, FocusGridMath.SpanGap(15f, 20f, 0f, 10f));
        }
    }
}
