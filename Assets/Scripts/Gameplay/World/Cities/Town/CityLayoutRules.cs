using System.Collections.Generic;
using UnityEngine;

namespace Rapadura.Gameplay.World.Cities.Town
{
    /// <summary>
    /// The walkable-town rules (pure, EditMode-tested on every recipe, Docs/Cities.md §5.4): no building on a street or
    /// on the plaza, no two lots overlapping, every door and exit reachable from the plaza through corridors at least
    /// <see cref="MinCorridor"/> wide (the hero is a 0.35 m capsule), every keeper standing clear of the colliders and
    /// the safe-zone box covering the whole town.
    /// </summary>
    public static class CityLayoutRules
    {
        /// <summary>Narrowest passage a door path may use (m).</summary>
        public const float MinCorridor = 1.2f;

        /// <summary>The hero's CharacterController radius.</summary>
        public const float WalkerRadius = 0.35f;

        public const float GridCell = 0.2f;

        /// <summary>Every rule at once; empty when the town is fine.</summary>
        public static List<string> Validate(CityLayout layout)
        {
            var problems = new List<string>();
            CheckBuildingsOffStreets(layout, problems);
            CheckLotsApart(layout, problems);
            CheckStandPoints(layout, problems);
            CheckSafeZone(layout, problems);
            CheckReachability(layout, problems, MinCorridor);
            return problems;
        }

        public static void CheckBuildingsOffStreets(CityLayout layout, List<string> problems)
        {
            foreach (CityLot lot in layout.Lots)
            {
                if (lot.Kind == CityLotKind.Market)
                {
                    continue;
                }

                foreach (CityStreet street in layout.Streets)
                {
                    if (CityGeometry.Overlaps(lot.Footprint, street.Bounds))
                    {
                        problems.Add($"Lot '{lot.Id}' overlaps the {street.Arm} street.");
                    }
                }

                if (CityGeometry.RectOverlapsCircle(lot.Footprint, layout.PlazaCenter, layout.PlazaRadius))
                {
                    problems.Add($"Lot '{lot.Id}' overlaps the plaza.");
                }
            }

            foreach (CityProp prop in layout.Props)
            {
                if (prop.Kind == CityPropKind.LampPost || prop.Kind == CityPropKind.Gate || prop.Kind == CityPropKind.FlowerBed || prop.Kind == CityPropKind.GrassTuft)
                {
                    continue;
                }

                CitySolid? solid = CitySolids.PropSolid(prop, layout.WalkBounds);
                if (!solid.HasValue)
                {
                    continue;
                }

                foreach (CityStreet street in layout.Streets)
                {
                    if (CityGeometry.Overlaps(solid.Value.Footprint, street.Road))
                    {
                        problems.Add($"{prop.Kind} at {prop.Position} stands on the {street.Arm} road.");
                    }
                }
            }
        }

        public static void CheckLotsApart(CityLayout layout, List<string> problems)
        {
            for (int i = 0; i < layout.Lots.Count; i++)
            {
                for (int j = i + 1; j < layout.Lots.Count; j++)
                {
                    if (CityGeometry.Overlaps(layout.Lots[i].Footprint, layout.Lots[j].Footprint))
                    {
                        problems.Add($"Lots '{layout.Lots[i].Id}' and '{layout.Lots[j].Id}' overlap.");
                    }
                }
            }
        }

        /// <summary>Keepers stand where a capsule fits: at least <see cref="WalkerRadius"/> from every collider.</summary>
        public static void CheckStandPoints(CityLayout layout, List<string> problems)
        {
            foreach (CityLot lot in layout.Lots)
            {
                if (!lot.HasKeeper)
                {
                    continue;
                }

                foreach (CitySolid solid in layout.Solids)
                {
                    float distance = solid.DistanceTo(lot.StandPoint);
                    if (distance < WalkerRadius)
                    {
                        problems.Add($"Keeper of '{lot.Id}' stands {distance:0.00} m from a collider of '{solid.Owner}' (inside a wall/prop).");
                    }
                }

                if (Vector2.Distance(lot.StandPoint, lot.DoorApproach) > lot.InteractRadius)
                {
                    problems.Add($"The door approach of '{lot.Id}' is out of the keeper's interact range.");
                }
            }
        }

        public static void CheckSafeZone(CityLayout layout, List<string> problems)
        {
            foreach (CityLot lot in layout.Lots)
            {
                if (!CityGeometry.Contains(layout.SafeZone, lot.Footprint))
                {
                    problems.Add($"Lot '{lot.Id}' is not inside the safe zone.");
                }
            }

            foreach (CityStreet street in layout.Streets)
            {
                if (!layout.SafeZone.Contains(street.End))
                {
                    problems.Add($"The {street.Arm} street ends outside the safe zone.");
                }
            }

            if (!layout.SafeZone.Contains(layout.Respawn) || !layout.SafeZone.Contains(layout.Arrival))
            {
                problems.Add("Respawn or arrival point outside the safe zone.");
            }
        }

        public static void CheckReachability(CityLayout layout, List<string> problems, float corridor)
        {
            WalkGrid grid = WalkGrid.Build(layout, corridor * 0.5f);
            if (!grid.Flood(layout.PlazaCenter))
            {
                problems.Add("The plaza centre itself is blocked.");
                return;
            }

            foreach (CityLot lot in layout.Lots)
            {
                if (lot.Kind == CityLotKind.House || lot.Kind == CityLotKind.LampHouse || lot.HasKeeper || lot.Kind == CityLotKind.ShipDock)
                {
                    if (!grid.IsReached(lot.DoorApproach))
                    {
                        problems.Add($"The door of '{lot.Id}' cannot be reached from the plaza with {corridor:0.0} m clearance.");
                    }
                }
            }

            foreach (CityExit exit in layout.Exits)
            {
                if (!grid.IsReached(exit.Target))
                {
                    problems.Add($"Exit '{exit.Id}' cannot be reached from the plaza.");
                }
            }

            if (!grid.IsReached(layout.Arrival))
            {
                problems.Add("The arrival point cannot reach the plaza.");
            }
        }

        /// <summary>Distance from <paramref name="point"/> to the nearest collider of the town.</summary>
        public static float Clearance(CityLayout layout, Vector2 point)
        {
            float best = float.PositiveInfinity;
            foreach (CitySolid solid in layout.Solids)
            {
                best = Mathf.Min(best, solid.DistanceTo(point));
            }

            return best;
        }

        /// <summary>Ground grid of the walk bounds: a cell is open when no collider is closer than the half corridor.</summary>
        public sealed class WalkGrid
        {
            private readonly Rect _bounds;
            private readonly int _columns;
            private readonly int _rows;
            private readonly bool[] _blocked;
            private bool[] _reached;

            private WalkGrid(Rect bounds)
            {
                _bounds = bounds;
                _columns = Mathf.CeilToInt(bounds.width / GridCell);
                _rows = Mathf.CeilToInt(bounds.height / GridCell);
                _blocked = new bool[_columns * _rows];
            }

            public static WalkGrid Build(CityLayout layout, float halfCorridor)
            {
                var grid = new WalkGrid(layout.WalkBounds);
                foreach (CitySolid solid in layout.Solids)
                {
                    Rect area = CityGeometry.Inflate(solid.Footprint, halfCorridor + GridCell);
                    grid.ForCells(area, (index, center) =>
                    {
                        if (solid.DistanceTo(center) < halfCorridor)
                        {
                            grid._blocked[index] = true;
                        }
                    });
                }

                return grid;
            }

            private void ForCells(Rect area, System.Action<int, Vector2> visit)
            {
                int x0 = Mathf.Clamp(Mathf.FloorToInt((area.xMin - _bounds.xMin) / GridCell), 0, _columns - 1);
                int x1 = Mathf.Clamp(Mathf.CeilToInt((area.xMax - _bounds.xMin) / GridCell), 0, _columns - 1);
                int y0 = Mathf.Clamp(Mathf.FloorToInt((area.yMin - _bounds.yMin) / GridCell), 0, _rows - 1);
                int y1 = Mathf.Clamp(Mathf.CeilToInt((area.yMax - _bounds.yMin) / GridCell), 0, _rows - 1);
                for (int y = y0; y <= y1; y++)
                {
                    for (int x = x0; x <= x1; x++)
                    {
                        visit(y * _columns + x, CellCenter(x, y));
                    }
                }
            }

            private Vector2 CellCenter(int x, int y) => new Vector2(_bounds.xMin + (x + 0.5f) * GridCell, _bounds.yMin + (y + 0.5f) * GridCell);

            private int IndexOf(Vector2 point)
            {
                int x = Mathf.FloorToInt((point.x - _bounds.xMin) / GridCell);
                int y = Mathf.FloorToInt((point.y - _bounds.yMin) / GridCell);
                return x < 0 || y < 0 || x >= _columns || y >= _rows ? -1 : y * _columns + x;
            }

            public bool IsOpen(Vector2 point)
            {
                int index = IndexOf(point);
                return index >= 0 && !_blocked[index];
            }

            /// <summary>Flood-fills the open cells from <paramref name="start"/>. False when the start cell is blocked.</summary>
            public bool Flood(Vector2 start)
            {
                _reached = new bool[_blocked.Length];
                int first = IndexOf(start);
                if (first < 0 || _blocked[first])
                {
                    return false;
                }

                var queue = new Queue<int>();
                queue.Enqueue(first);
                _reached[first] = true;
                while (queue.Count > 0)
                {
                    int index = queue.Dequeue();
                    int x = index % _columns;
                    int y = index / _columns;
                    Visit(x + 1, y, queue);
                    Visit(x - 1, y, queue);
                    Visit(x, y + 1, queue);
                    Visit(x, y - 1, queue);
                }

                return true;
            }

            private void Visit(int x, int y, Queue<int> queue)
            {
                if (x < 0 || y < 0 || x >= _columns || y >= _rows)
                {
                    return;
                }

                int index = y * _columns + x;
                if (_blocked[index] || _reached[index])
                {
                    return;
                }

                _reached[index] = true;
                queue.Enqueue(index);
            }

            public bool IsReached(Vector2 point)
            {
                int index = IndexOf(point);
                return _reached != null && index >= 0 && _reached[index];
            }
        }
    }
}
