using System.Collections.Generic;
using Rapadura.Gameplay.World.Map.Overworld;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// The ground height under the fixed points of a region's mechanics (beacons, cracks, piers, ring centres): the relief never
    /// changes while the layout lives, so each point pays <see cref="OverworldRules.SurfaceAt"/> (four HeightAt) once instead of
    /// once per frame. Only for points that do not move; a moving point would just fill the table (it is cleared past
    /// <see cref="MaxPoints"/>).
    /// </summary>
    public sealed class StaticGroundCache
    {
        public const int MaxPoints = 512;

        private readonly Dictionary<Vector2, float> _heights = new Dictionary<Vector2, float>();
        private OverworldLayout _layout;

        /// <summary>How many points were sampled (tests).</summary>
        public int Samples { get; private set; }

        /// <summary><c>max(0, SurfaceAt(layout, world))</c>, sampled once per point.</summary>
        public float GroundAt(OverworldLayout layout, Vector2 world)
        {
            if (!ReferenceEquals(layout, _layout))
            {
                _heights.Clear();
                _layout = layout;
            }

            if (_heights.TryGetValue(world, out float y))
            {
                return y;
            }

            if (_heights.Count >= MaxPoints)
            {
                _heights.Clear();
            }

            y = Mathf.Max(0f, OverworldRules.SurfaceAt(layout, world));
            _heights[world] = y;
            Samples++;
            return y;
        }
    }
}
