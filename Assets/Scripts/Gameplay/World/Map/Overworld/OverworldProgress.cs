using System;
using System.Collections.Generic;
using System.Linq;

namespace Rapadura.Gameplay.World.Map.Overworld
{
    /// <summary>
    /// The docks (Cais da Barca of the settlements, by catalog map id) of the continuous regions the hero has reached on foot:
    /// inside a region the Barcas de Luz only carry the hero between docks already visited; the Vila's (01) is always open.
    /// Saved with the cities (<c>CitySaveState.visitedDocks</c>, <c>CityProgress</c>), cleared on a new session. Pure,
    /// server-side state in the online build.
    /// </summary>
    public static class OverworldProgress
    {
        private static readonly HashSet<string> Visited = new HashSet<string>(StringComparer.Ordinal);

        public static bool IsVisited(string mapId) => !string.IsNullOrEmpty(mapId) && (mapId == MapCatalog.PomarId || Visited.Contains(mapId));

        /// <summary>Records a dock; true the first time.</summary>
        public static bool MarkVisited(string mapId) => !string.IsNullOrEmpty(mapId) && Visited.Add(mapId);

        /// <summary>The docks reached, sorted (save order is stable). The always-open Vila is not listed unless reached.</summary>
        public static IReadOnlyList<string> VisitedMapIds => Visited.OrderBy(id => id, StringComparer.Ordinal).ToList();

        /// <summary>Replaces the visited docks with <paramref name="mapIds"/> (a loaded save); unknown catalog ids are dropped.</summary>
        public static void Restore(IEnumerable<string> mapIds)
        {
            Visited.Clear();
            if (mapIds == null)
            {
                return;
            }

            foreach (string id in mapIds)
            {
                if (MapCatalog.Get(id) != null)
                {
                    Visited.Add(id);
                }
            }
        }

        public static void Clear() => Visited.Clear();
    }
}
