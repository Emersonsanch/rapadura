using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Rapadura.Gameplay.World.Map.Areas;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Overworld
{
    /// <summary>
    /// The trails, spawn clearings and landmarks of a layout bucketed by chunk (built once per layout), so gathering what is
    /// near a chunk reads a handful of candidates instead of the whole world (the stitched world has ~3 000 trail pieces
    /// and ~1 600 clearings). Each list keeps the layout's order, so the chunk content is the same as a full pass.
    /// </summary>
    public static partial class OverworldChunkBuilder
    {
        /// <summary>Margin (m) of <see cref="Gather"/>'s "near the chunk" rectangle.</summary>
        private const float GatherMargin = 16f;

        private sealed class ChunkIndex
        {
            public int PathCount;
            public int SpawnCount;
            public int LandmarkCount;
            public readonly Dictionary<Vector2Int, List<int>> Paths = new Dictionary<Vector2Int, List<int>>();
            public readonly Dictionary<Vector2Int, List<int>> Spawns = new Dictionary<Vector2Int, List<int>>();
            public readonly Dictionary<Vector2Int, List<int>> Landmarks = new Dictionary<Vector2Int, List<int>>();
        }

        private static readonly ConditionalWeakTable<OverworldLayout, ChunkIndex> Indexes = new ConditionalWeakTable<OverworldLayout, ChunkIndex>();

        private static readonly List<int> NoIndex = new List<int>();

        private static ChunkIndex IndexOf(OverworldLayout layout)
        {
            MapLayout world = layout.World;
            if (Indexes.TryGetValue(layout, out ChunkIndex index) && index.PathCount == world.Paths.Count && index.SpawnCount == world.Zones.Count
                && index.LandmarkCount == world.Landmarks.Count)
            {
                return index;
            }

            index = new ChunkIndex { PathCount = world.Paths.Count, SpawnCount = world.Zones.Count, LandmarkCount = world.Landmarks.Count };
            for (int i = 0; i < world.Paths.Count; i++)
            {
                MapPath path = world.Paths[i];
                float pad = path.Width + GatherMargin + 1f;
                Register(layout, index.Paths, i, Rect.MinMaxRect(Mathf.Min(path.A.x, path.B.x) - pad, Mathf.Min(path.A.y, path.B.y) - pad,
                    Mathf.Max(path.A.x, path.B.x) + pad, Mathf.Max(path.A.y, path.B.y) + pad));
            }

            for (int i = 0; i < world.Zones.Count; i++)
            {
                MapSpawnZone spawn = world.Zones[i];
                float pad = spawn.Radius + GatherMargin + 1f;
                Register(layout, index.Spawns, i, new Rect(spawn.Center - new Vector2(pad, pad), new Vector2(pad, pad) * 2f));
            }

            for (int i = 0; i < world.Landmarks.Count; i++)
            {
                float pad = 10f + GatherMargin + 1f;
                Register(layout, index.Landmarks, i, new Rect(world.Landmarks[i].Position - new Vector2(pad, pad), new Vector2(pad, pad) * 2f));
            }

            Indexes.AddOrUpdate(layout, index);
            return index;
        }

        private static void Register(OverworldLayout layout, Dictionary<Vector2Int, List<int>> map, int item, Rect box)
        {
            Vector2Int min = OverworldRules.ChunkOf(layout, box.min);
            Vector2Int max = OverworldRules.ChunkOf(layout, box.max);
            for (int x = min.x; x <= max.x; x++)
            {
                for (int y = min.y; y <= max.y; y++)
                {
                    var c = new Vector2Int(x, y);
                    if (!map.TryGetValue(c, out List<int> list))
                    {
                        list = new List<int>();
                        map[c] = list;
                    }

                    list.Add(item);
                }
            }
        }

        private static List<int> Near(Dictionary<Vector2Int, List<int>> map, Vector2Int chunk) => map.TryGetValue(chunk, out List<int> list) ? list : NoIndex;
    }
}
