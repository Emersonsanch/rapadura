using UnityEngine;

namespace Rapadura.Gameplay.Building
{
    /// <summary>
    /// Pure math helpers for grid-based building placement: world-to-grid snapping and axis-aligned
    /// footprint overlap checks. Deliberately has zero dependency on Unity physics (no Colliders,
    /// no Physics.OverlapBox) so it can be exercised in EditMode tests with synthetic data, and so
    /// placement validation works identically whether or not a real Scene/Collider setup exists yet.
    /// </summary>
    public static class GridPlacementUtility
    {
        /// <summary>Snaps a world-space X/Z position to the nearest grid cell center for the given cell size.</summary>
        public static Vector3 SnapToGrid(Vector3 worldPosition, float cellSize)
        {
            if (cellSize <= 0f)
            {
                return worldPosition;
            }

            float x = Mathf.Round(worldPosition.x / cellSize) * cellSize;
            float z = Mathf.Round(worldPosition.z / cellSize) * cellSize;
            return new Vector3(x, worldPosition.y, z);
        }

        /// <summary>Converts a world-space X/Z position into integer grid coordinates.</summary>
        public static Vector2Int WorldToGridCoord(Vector3 worldPosition, float cellSize)
        {
            if (cellSize <= 0f)
            {
                return Vector2Int.zero;
            }

            int gx = Mathf.RoundToInt(worldPosition.x / cellSize);
            int gy = Mathf.RoundToInt(worldPosition.z / cellSize);
            return new Vector2Int(gx, gy);
        }

        /// <summary>
        /// Returns the axis-aligned footprint, in grid cells, of a structure placed at
        /// <paramref name="origin"/> with the given footprint size and rotation. A 90/270 degree
        /// rotation swaps width and height.
        /// </summary>
        public static RectInt GetFootprintBounds(Vector2Int origin, Vector2Int footprintSize, int rotationSteps)
        {
            bool swapped = IsSwapped(rotationSteps);
            int width = swapped ? footprintSize.y : footprintSize.x;
            int height = swapped ? footprintSize.x : footprintSize.y;
            return new RectInt(origin.x, origin.y, Mathf.Max(1, width), Mathf.Max(1, height));
        }

        private static bool IsSwapped(int rotationSteps)
        {
            int normalized = ((rotationSteps % 4) + 4) % 4;
            return normalized == 1 || normalized == 3;
        }

        /// <summary>True if two axis-aligned grid footprints overlap (touching edges do NOT count as overlap).</summary>
        public static bool Overlaps(RectInt a, RectInt b)
        {
            return a.xMin < b.xMax && a.xMax > b.xMin && a.yMin < b.yMax && a.yMax > b.yMin;
        }
    }
}
