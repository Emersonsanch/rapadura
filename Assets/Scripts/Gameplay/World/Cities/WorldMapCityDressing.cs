using System.Collections.Generic;
using Rapadura.Gameplay.World.Biomes;
using Rapadura.Gameplay.World.Map.Dressing;
using UnityEngine;

namespace Rapadura.Gameplay.World.Cities
{
    /// <summary>
    /// The visible city on the world map, built from the dressing's own tools (<see cref="WorldMapMeshBuilder"/>, the
    /// palette swatches, <see cref="WorldMapTerrain"/> geometry) so it shares its single material:
    /// <list type="bullet">
    /// <item><see cref="AppendSafeRing"/> — the city limit painted on the ground: a dashed gold ring of
    /// <see cref="SafeZoneRules.MapCityRadius"/> around the node, open where each road leaves, with a pair of gate posts
    /// (porteira) on every road. Inside it is the zona segura.</item>
    /// <item><see cref="AppendTown"/> — a small cluster of regional houses next to the node (adobe and a juazeiro in the
    /// Deserto, stone with snowy roofs in the Montanha, lamp-lit houses in the Cavernas; the Floresta's landmark already
    /// is Vó Zefa's village, so it only gets a signpost).</item>
    /// <item><see cref="TownSpot"/> — where that cluster goes: the back side of the node, clear of roads, river and the
    /// region landmark. Pure geometry, EditMode-tested.</item>
    /// </list>
    /// </summary>
    public static class WorldMapCityDressing
    {
        public const float TownDistance = 1.95f;
        public const float TownRadius = 0.55f;
        public const float RingWidth = 0.08f;
        public const int RingSegments = 36;
        public const float GatePostOffset = WorldMapTerrain.PathHalfWidth + 0.12f;

        // Behind the node first (camera looks toward +z); never straight in front of it.
        private static readonly float[] TownAngles = { 140f, 40f, 165f, 15f, 100f, 80f, 195f, -15f };

        // ---------------------------------------------------------------
        // Layout
        // ---------------------------------------------------------------

        /// <summary>Directions (degrees from +x, counter-clockwise seen from above) of the roads leaving site <paramref name="siteIndex"/>.</summary>
        public static List<float> RoadAngles(WorldMapTerrain terrain, int siteIndex)
        {
            var angles = new List<float>();
            Vector2 origin = terrain.Sites[siteIndex].Position;
            foreach (WorldMapPath path in terrain.Paths)
            {
                Vector2 other;
                if (path.From == siteIndex)
                {
                    other = path.B;
                }
                else if (path.To == siteIndex)
                {
                    other = path.A;
                }
                else
                {
                    continue;
                }

                Vector2 d = other - origin;
                if (d.sqrMagnitude > 0.0001f)
                {
                    angles.Add(Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg);
                }
            }

            return angles;
        }

        /// <summary>Smallest gap between a town at <paramref name="p"/> and roads, river, bridge, other nodes and the landmark.</summary>
        public static float TownClearance(WorldMapTerrain terrain, int siteIndex, Vector2 p, Vector2 landmark)
        {
            float clearance = terrain.DistanceToPath(p) - WorldMapTerrain.PathHalfWidth - TownRadius;
            if (terrain.HasRiver)
            {
                clearance = Mathf.Min(clearance, terrain.DistanceToRiver(p) - WorldMapTerrain.RiverHalfWidth - TownRadius);
                clearance = Mathf.Min(clearance, Vector2.Distance(p, terrain.BridgeCenter) - 2.5f - TownRadius);
            }

            clearance = Mathf.Min(clearance, Vector2.Distance(p, landmark) - WorldMapDressingLayout.LandmarkRadius - TownRadius);
            for (int s = 0; s < terrain.Sites.Count; s++)
            {
                if (s != siteIndex)
                {
                    clearance = Mathf.Min(clearance, Vector2.Distance(p, terrain.Sites[s].Position) - WorldMapDressingLayout.NodeClearRadius(terrain.Sites[s], true) - TownRadius);
                }
            }

            return clearance;
        }

        /// <summary>Ground spot (dressing-local x/z) of site <paramref name="siteIndex"/>'s town cluster.</summary>
        public static Vector2 TownSpot(WorldMapTerrain terrain, int siteIndex, Vector2 landmark)
        {
            Vector2 origin = terrain.Sites[siteIndex].Position;
            Vector2 best = origin + Direction(TownAngles[0]) * TownDistance;
            float bestScore = float.NegativeInfinity;
            foreach (float angle in TownAngles)
            {
                Vector2 candidate = origin + Direction(angle) * TownDistance;
                float score = TownClearance(terrain, siteIndex, candidate, landmark);
                if (score >= 0.1f)
                {
                    return candidate;
                }

                if (score > bestScore)
                {
                    bestScore = score;
                    best = candidate;
                }
            }

            return best;
        }

        public static Vector2 Direction(float degrees)
        {
            float radians = degrees * Mathf.Deg2Rad;
            return new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));
        }

        /// <summary>Angular half-gap (degrees) the ring leaves open for a road of half-width <see cref="WorldMapTerrain.PathHalfWidth"/>.</summary>
        public static float GateHalfAngle(float radius)
        {
            float ratio = Mathf.Clamp(GatePostOffset / Mathf.Max(0.01f, radius), 0f, 0.99f);
            return Mathf.Asin(ratio) * Mathf.Rad2Deg;
        }

        /// <summary>Whether ring angle <paramref name="degrees"/> falls in a road's gate gap.</summary>
        public static bool IsInGate(float degrees, IReadOnlyList<float> roadAngles, float halfGap)
        {
            if (roadAngles == null)
            {
                return false;
            }

            foreach (float road in roadAngles)
            {
                if (Mathf.Abs(Mathf.DeltaAngle(degrees, road)) < halfGap)
                {
                    return true;
                }
            }

            return false;
        }

        // ---------------------------------------------------------------
        // Meshes (local to the node / the town spot, y = 0 on the ground)
        // ---------------------------------------------------------------

        /// <summary>Dashed city limit around the node, open on every road, with gate posts either side of each road.</summary>
        public static void AppendSafeRing(WorldMapMeshBuilder b, float radius, IReadOnlyList<float> roadAngles)
        {
            float halfGap = GateHalfAngle(radius);
            float step = 360f / RingSegments;
            for (int i = 0; i < RingSegments; i += 2)
            {
                float a0 = i * step;
                float a1 = a0 + step * 0.8f;
                if (IsInGate(a0, roadAngles, halfGap) || IsInGate(a1, roadAngles, halfGap))
                {
                    continue;
                }

                Vector2 d0 = Direction(a0);
                Vector2 d1 = Direction(a1);
                float inner = radius - RingWidth * 0.5f;
                float outer = radius + RingWidth * 0.5f;
                const float y = 0.035f;
                var i0 = new Vector3(d0.x * inner, y, d0.y * inner);
                var o0 = new Vector3(d0.x * outer, y, d0.y * outer);
                var i1 = new Vector3(d1.x * inner, y, d1.y * inner);
                var o1 = new Vector3(d1.x * outer, y, d1.y * outer);
                // Counter-clockwise angles: o0 → i0 → i1 → o1 is clockwise seen from above (faces up).
                b.Quad(o0, i0, i1, o1, WorldMapSwatch.Gold);
            }

            if (roadAngles == null)
            {
                return;
            }

            foreach (float road in roadAngles)
            {
                foreach (float side in new[] { -halfGap, halfGap })
                {
                    Vector2 d = Direction(road + side);
                    var at = new Vector3(d.x * radius, 0f, d.y * radius);
                    b.Box(at + new Vector3(0f, 0.24f, 0f), new Vector3(0.11f, 0.48f, 0.11f), WorldMapSwatch.Wood, road, WorldMapSwatch.WoodDark);
                    b.Box(at + new Vector3(0f, 0.52f, 0f), new Vector3(0.15f, 0.08f, 0.15f), WorldMapSwatch.Gold, road);
                }
            }
        }

        /// <summary>The regional house cluster (centred on the town spot, front facing -z).</summary>
        public static void AppendTown(WorldMapMeshBuilder b, BiomeType region)
        {
            switch (region)
            {
                case BiomeType.Desert:
                    AppendHouse(b, new Vector3(-0.2f, 0f, 0.1f), -10f, 0.55f, WorldMapSwatch.SandRock, WorldMapSwatch.Ruin, flatRoof: true);
                    AppendHouse(b, new Vector3(0.3f, 0f, -0.2f), 15f, 0.42f, WorldMapSwatch.Wall, WorldMapSwatch.SandRock, flatRoof: true);
                    // The juazeiro: an evergreen of the caatinga, shade for whoever passes.
                    b.Cylinder(new Vector3(0.35f, 0f, 0.35f), 0.05f, 0.04f, 0.45f, 5, WorldMapSwatch.Trunk);
                    b.Blob(new Vector3(0.35f, 0.55f, 0.35f), new Vector3(0.45f, 0.3f, 0.45f), WorldMapSwatch.LeafDark, coarse: true);
                    break;
                case BiomeType.Mountain:
                    AppendHouse(b, new Vector3(-0.2f, 0f, 0.1f), 8f, 0.55f, WorldMapSwatch.Rock, WorldMapSwatch.Snow);
                    AppendHouse(b, new Vector3(0.32f, 0f, -0.18f), -18f, 0.42f, WorldMapSwatch.RockDark, WorldMapSwatch.Snow);
                    b.Box(new Vector3(0.38f, 0.15f, 0.32f), new Vector3(0.28f, 0.3f, 0.2f), WorldMapSwatch.WoodDark, 20f, WorldMapSwatch.Snow);
                    break;
                case BiomeType.Caves:
                    AppendHouse(b, new Vector3(-0.2f, 0f, 0.1f), -6f, 0.5f, WorldMapSwatch.CaveStone, WorldMapSwatch.CaveDark);
                    AppendHouse(b, new Vector3(0.3f, 0f, -0.15f), 22f, 0.4f, WorldMapSwatch.CaveStone, WorldMapSwatch.RockDark);
                    AppendLampPost(b, new Vector3(0.45f, 0f, 0.3f));
                    AppendLampPost(b, new Vector3(-0.5f, 0f, -0.3f));
                    break;
                default:
                    AppendSignpost(b, new Vector3(0f, 0f, 0f));
                    return;
            }

            AppendSignpost(b, new Vector3(-0.45f, 0f, -0.4f));
        }

        private static void AppendHouse(WorldMapMeshBuilder b, Vector3 at, float yaw, float size, WorldMapSwatch wall, WorldMapSwatch roof,
            bool flatRoof = false)
        {
            Matrix4x4 saved = b.Matrix;
            b.Matrix = saved * Matrix4x4.TRS(at, Quaternion.Euler(0f, yaw, 0f), Vector3.one * size);
            b.Box(new Vector3(0f, 0.35f, 0f), new Vector3(1f, 0.7f, 0.8f), wall);
            if (flatRoof)
            {
                b.Box(new Vector3(0f, 0.74f, 0f), new Vector3(1.1f, 0.08f, 0.9f), roof);
            }
            else
            {
                var l0 = new Vector3(-0.6f, 0.68f, -0.52f);
                var r0 = new Vector3(0.6f, 0.68f, -0.52f);
                var l1 = new Vector3(-0.6f, 0.68f, 0.52f);
                var r1 = new Vector3(0.6f, 0.68f, 0.52f);
                var lt = new Vector3(-0.6f, 1.15f, 0f);
                var rt = new Vector3(0.6f, 1.15f, 0f);
                b.Quad(l0, lt, rt, r0, roof);
                b.Quad(r1, rt, lt, l1, roof);
                b.Triangle(new Vector3(-0.5f, 0.7f, -0.4f), new Vector3(-0.5f, 0.7f, 0.4f), new Vector3(-0.5f, 1.1f, 0f), wall);
                b.Triangle(new Vector3(0.5f, 0.7f, 0.4f), new Vector3(0.5f, 0.7f, -0.4f), new Vector3(0.5f, 1.1f, 0f), wall);
            }

            b.Box(new Vector3(-0.15f, 0.24f, -0.41f), new Vector3(0.26f, 0.42f, 0.04f), WorldMapSwatch.WoodDark);
            b.Box(new Vector3(0.24f, 0.42f, -0.41f), new Vector3(0.2f, 0.18f, 0.04f), WorldMapSwatch.Lantern);
            b.Matrix = saved;
        }

        private static void AppendLampPost(WorldMapMeshBuilder b, Vector3 at)
        {
            b.Cylinder(at, 0.03f, 0.03f, 0.55f, 4, WorldMapSwatch.WoodDark);
            b.Box(at + new Vector3(0f, 0.6f, 0f), new Vector3(0.12f, 0.12f, 0.12f), WorldMapSwatch.Lantern);
        }

        private static void AppendSignpost(WorldMapMeshBuilder b, Vector3 at)
        {
            b.Cylinder(at, 0.035f, 0.035f, 0.6f, 4, WorldMapSwatch.WoodDark);
            b.Box(at + new Vector3(0f, 0.5f, -0.02f), new Vector3(0.42f, 0.16f, 0.04f), WorldMapSwatch.Wood);
            b.Card(at + new Vector3(0.02f, 0.62f, 0f), at + new Vector3(0.02f, 0.8f, 0f), at + new Vector3(0.24f, 0.74f, 0f), at + new Vector3(0.24f, 0.64f, 0f),
                WorldMapSwatch.Banner, new Vector3(0f, 0.5f, -1f));
        }
    }
}
