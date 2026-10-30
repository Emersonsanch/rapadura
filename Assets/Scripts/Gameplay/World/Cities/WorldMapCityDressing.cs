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
    /// <see cref="SafeZoneRules.MapCityRadius"/> around the node, open where each road leaves, with a pair of adobe
    /// pillars (the city's portal, world bible §3.22) on every road. Inside it is the zona segura.</item>
    /// <item><see cref="AppendTown"/> — a small cluster of houses next to the node in the region's signature form (bible
    /// §2.5): Qocho's rose taipa with a lattice-walled grape-drying house, a cistern dome and a tamarisk in the Areias
    /// Flamejantes; Litema's rondavels with litema bands, a curved dry-stone wall, aloes and a baobab in the Serra de
    /// Kaggen; doors and round windows cut in pink tuff with lamp posts in the Grutas do Limiar. The Bosque do
    /// Zarafshan's landmark already is the Vila da Vó Edna, so it only gets a signpost and an apricot tree.</item>
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
                    // An adobe pillar on an ochre foot, a lime cap and a thin gold band (the portal).
                    b.Box(at + new Vector3(0f, 0.03f, 0f), new Vector3(0.2f, 0.06f, 0.2f), WorldMapSwatch.AdobeDark, road);
                    b.Box(at + new Vector3(0f, 0.3f, 0f), new Vector3(0.15f, 0.5f, 0.15f), WorldMapSwatch.Adobe, road, WorldMapSwatch.AdobeDark);
                    b.Box(at + new Vector3(0f, 0.44f, 0f), new Vector3(0.16f, 0.035f, 0.16f), WorldMapSwatch.Gold, road);
                    b.Box(at + new Vector3(0f, 0.58f, 0f), new Vector3(0.19f, 0.06f, 0.19f), WorldMapSwatch.Wall, road);
                }
            }
        }

        /// <summary>The regional house cluster (centred on the town spot, front facing -z).</summary>
        public static void AppendTown(WorldMapMeshBuilder b, BiomeType region)
        {
            switch (region)
            {
                case BiomeType.Desert:
                    // Oásis de Qocho: rose taipa houses, a grape-drying house with lattice walls, a cistern dome, a tamarisk.
                    WorldMapPropMeshes.AppendAdobeHouse(b, new Vector3(-0.2f, 0f, 0.1f), -10f, 0.55f, WorldMapSwatch.SandRock, WorldMapSwatch.Ruin, iwan: false);
                    AppendDryingHouse(b, new Vector3(0.3f, 0f, -0.2f), 15f, 0.42f);
                    b.Cylinder(new Vector3(-0.05f, 0f, -0.32f), 0.16f, 0.16f, 0.08f, 8, WorldMapSwatch.Ruin);
                    b.Blob(new Vector3(-0.05f, 0.08f, -0.32f), new Vector3(0.3f, 0.26f, 0.3f), WorldMapSwatch.Ruin);
                    WorldMapPropMeshes.AppendTamarisk(b, new Vector3(0.38f, 0f, 0.35f), 0.4f);
                    break;
                case BiomeType.Mountain:
                    // Vila de Litema: rondavels with painted bands, a curved dry-stone wall, aloes and a baobab.
                    AppendRondavel(b, new Vector3(-0.2f, 0f, 0.12f), 0.55f);
                    AppendRondavel(b, new Vector3(0.3f, 0f, -0.15f), 0.42f);
                    for (int i = 0; i < 6; i++)
                    {
                        float a = (200f + i * 22f) * Mathf.Deg2Rad;
                        b.Box(new Vector3(Mathf.Cos(a) * 0.55f, 0.06f, Mathf.Sin(a) * 0.55f + 0.05f), new Vector3(0.2f, 0.12f, 0.08f), i % 2 == 0 ? WorldMapSwatch.Rock : WorldMapSwatch.RockDark,
                            -(200f + i * 22f) + 90f, WorldMapSwatch.RockDark);
                    }

                    WorldMapPropMeshes.AppendBaobab(b, new Vector3(0.4f, 0f, 0.36f), 0.5f);
                    WorldMapPropMeshes.AppendAloe(b, new Vector3(0.05f, 0f, -0.45f), 0.5f);
                    WorldMapPropMeshes.AppendAloe(b, new Vector3(0.55f, 0f, 0.05f), 0.4f);
                    break;
                case BiomeType.Caves:
                    // Vila das Lamparinas: doors and round windows cut in pink tuff, lamps lit at the doors.
                    AppendTuffHouse(b, new Vector3(-0.2f, 0f, 0.1f), -6f, 0.55f);
                    AppendTuffHouse(b, new Vector3(0.3f, 0f, -0.15f), 22f, 0.42f);
                    AppendLampPost(b, new Vector3(0.45f, 0f, 0.3f));
                    AppendLampPost(b, new Vector3(-0.5f, 0f, -0.3f));
                    break;
                default:
                    AppendSignpost(b, new Vector3(0f, 0f, 0f));
                    WorldMapPropMeshes.AppendApricot(b, new Vector3(0.35f, 0f, 0.2f), 0.45f);
                    return;
            }

            AppendSignpost(b, new Vector3(-0.45f, 0f, -0.4f));
        }

        /// <summary>A Turfan grape-drying house: a tall taipa box whose walls are open brick lattice (dark holes in a checker).</summary>
        private static void AppendDryingHouse(WorldMapMeshBuilder b, Vector3 at, float yaw, float size)
        {
            Matrix4x4 saved = b.Matrix;
            b.Matrix = saved * Matrix4x4.TRS(at, Quaternion.Euler(0f, yaw, 0f), Vector3.one * size);
            b.Box(new Vector3(0f, 0.45f, 0f), new Vector3(0.9f, 0.9f, 0.75f), WorldMapSwatch.Ruin, top: WorldMapSwatch.RuinDark);
            for (int row = 0; row < 3; row++)
            {
                for (int col = 0; col < 4; col++)
                {
                    if ((row + col) % 2 != 0) continue;
                    var hole = new Vector3(-0.3f + col * 0.2f, 0.25f + row * 0.22f, -0.38f);
                    b.Box(hole, new Vector3(0.1f, 0.1f, 0.02f), WorldMapSwatch.Black);
                    b.Box(new Vector3(0.455f, hole.y, -0.25f + col * 0.17f), new Vector3(0.02f, 0.1f, 0.1f), WorldMapSwatch.Black);
                }
            }

            b.Matrix = saved;
        }

        /// <summary>A Litema rondavel: a round clay house with a painted band, a dark door and a conical thatch roof.</summary>
        private static void AppendRondavel(WorldMapMeshBuilder b, Vector3 at, float size)
        {
            b.Cylinder(at, 0.42f * size, 0.42f * size, 0.62f * size, 9, WorldMapSwatch.Wall);
            b.Cylinder(at + Vector3.up * 0.12f * size, 0.43f * size, 0.43f * size, 0.12f * size, 9, WorldMapSwatch.Roof);
            b.Cylinder(at + Vector3.up * 0.3f * size, 0.43f * size, 0.43f * size, 0.04f * size, 9, WorldMapSwatch.Banner);
            b.Box(at + new Vector3(0f, 0.22f, -0.41f) * size, new Vector3(0.18f, 0.36f, 0.04f) * size, WorldMapSwatch.Black);
            b.Cylinder(at + Vector3.up * 0.6f * size, 0.56f * size, 0f, 0.55f * size, 9, WorldMapSwatch.Sand);
            b.Cylinder(at + Vector3.up * 1.05f * size, 0.12f * size, 0f, 0.14f * size, 6, WorldMapSwatch.Snow);
        }

        /// <summary>A Lamparinas tuff house: a rounded pink rock mass with a door, a round window and a little lamp niche.</summary>
        private static void AppendTuffHouse(WorldMapMeshBuilder b, Vector3 at, float yaw, float size)
        {
            Matrix4x4 saved = b.Matrix;
            b.Matrix = saved * Matrix4x4.TRS(at, Quaternion.Euler(0f, yaw, 0f), Vector3.one * size);
            b.Blob(new Vector3(0f, 0.45f, 0.1f), new Vector3(1.1f, 1.05f, 0.9f), WorldMapSwatch.Tamarisk, 0f, true);
            b.Box(new Vector3(0f, 0.3f, -0.33f), new Vector3(0.9f, 0.6f, 0.12f), WorldMapSwatch.Tamarisk, top: WorldMapSwatch.CaveStone);
            b.Box(new Vector3(-0.15f, 0.21f, -0.4f), new Vector3(0.22f, 0.36f, 0.03f), WorldMapSwatch.WoodDark);
            b.Box(new Vector3(-0.15f, 0.41f, -0.4f), new Vector3(0.26f, 0.05f, 0.04f), WorldMapSwatch.CaveStone);
            b.Blob(new Vector3(0.22f, 0.42f, -0.39f), new Vector3(0.14f, 0.14f, 0.03f), WorldMapSwatch.Lantern, 0f, true);
            b.Box(new Vector3(0.22f, 0.15f, -0.41f), new Vector3(0.08f, 0.08f, 0.02f), WorldMapSwatch.Lantern);
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
