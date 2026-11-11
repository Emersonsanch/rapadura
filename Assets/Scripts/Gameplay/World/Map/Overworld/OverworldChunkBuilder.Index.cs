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

        /// <summary>
        /// The indices into <c>layout.World.Zones</c> of every clearing registered in a chunk within <paramref name="radius"/> of
        /// <paramref name="p"/> (cleared into <paramref name="result"/>, ascending, no repeats: the layout's order). A clearing whose
        /// edge is within <c>radius + GatherMargin</c> of <paramref name="p"/> is always among them (its registered box reaches that
        /// far), so the spawner wake test can read these instead of the whole world.
        /// </summary>
        public static void SpawnsNear(OverworldLayout layout, Vector2 p, float radius, List<int> result)
        {
            result.Clear();
            if (layout == null || layout.World == null)
            {
                return;
            }

            ChunkIndex index = IndexOf(layout);
            Vector2Int min = OverworldRules.ChunkOf(layout, p - new Vector2(radius, radius));
            Vector2Int max = OverworldRules.ChunkOf(layout, p + new Vector2(radius, radius));
            for (int x = min.x; x <= max.x; x++)
            {
                for (int y = min.y; y <= max.y; y++)
                {
                    var c = new Vector2Int(x, y);
                    if (OverworldRules.DistanceToChunk(layout, p, c) > radius || !index.Spawns.TryGetValue(c, out List<int> list))
                    {
                        continue;
                    }

                    result.AddRange(list);
                }
            }

            if (result.Count < 2)
            {
                return;
            }

            result.Sort();
            int write = 1;
            for (int read = 1; read < result.Count; read++)
            {
                if (result[read] != result[write - 1])
                {
                    result[write++] = result[read];
                }
            }

            result.RemoveRange(write, result.Count - write);
        }
    }
}
