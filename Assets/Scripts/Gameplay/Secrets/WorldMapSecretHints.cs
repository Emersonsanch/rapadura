using System.Collections.Generic;
using Rapadura.Gameplay.World.Map.Dressing;
using UnityEngine;

namespace Rapadura.Gameplay.Secrets
{
    /// <summary>
    /// The world-map side of the secrets, as pure geometry on top of the dressing (EditMode-tested):
    /// <list type="bullet">
    /// <item><see cref="FindSpot"/> — where a secret's hint prop sits next to its region's node: about
    /// <see cref="HintDistance"/> away, preferring the secret's own direction (behind the node, away from the
    /// camera), on dry land of that region, clear of roads, the river, every node, the landmarks, the other hints
    /// and as far as possible from the scattered props (same rules as <see cref="WorldMapDressingLayout"/>).</item>
    /// <item><see cref="AppendHint"/> / <see cref="AppendMarker"/> — the low-poly hint props and the floating
    /// gold marker, in the dressing's palette (<see cref="WorldMapSwatch"/>) so they share its single material.</item>
    /// </list>
    /// </summary>
    public static class WorldMapSecretHints
    {
        public const float HintDistance = 4.7f;
        public const float HintRadius = 0.8f;

        /// <summary>Clearance a spot needs from roads, river, nodes, landmarks and other hints to be taken at once.</summary>
        public const float ComfortableClearance = 0.25f;

        /// <summary>Height of the floating marker above the hint's ground.</summary>
        public const float MarkerHeight = 2.1f;

        private static readonly float[] AngleOffsets = { 0f, 18f, -18f, 36f, -36f, 55f, -55f, 80f, -80f, 120f, -120f, 180f };
        private static readonly float[] Distances = { HintDistance, HintDistance + 0.7f, HintDistance + 1.4f, HintDistance - 0.4f };

        // ---------------------------------------------------------------
        // Placement
        // ---------------------------------------------------------------

        /// <summary>
        /// Ground spot (dressing-local x/z) for a hint next to site <paramref name="siteIndex"/>. Hard rules (region,
        /// dry land, roads, river, nodes, landmarks, other hints — <see cref="Clearance"/>) come first; the scattered
        /// props are a soft rule (<see cref="PropClearance"/>): the first candidate clear of both wins, else the
        /// candidate meeting the hard rules that is farthest from any prop, else the best hard score found (a cramped
        /// map still gets its hints).
        /// </summary>
        public static Vector2 FindSpot(WorldMapTerrain terrain, int siteIndex, float preferredAngle, IReadOnlyList<Vector2> taken,
            IReadOnlyList<WorldMapPropPlacement> props)
        {
            Vector2 origin = terrain.Sites[siteIndex].Position;
            var landmarks = new List<Vector2>(terrain.Sites.Count);
            WorldMapDressingLayout.LandmarkPositions(terrain, landmarks);

            Vector2 bestHard = origin + Direction(preferredAngle) * HintDistance;
            float bestHardScore = float.NegativeInfinity;
            Vector2? bestSoft = null;
            float bestSoftScore = float.NegativeInfinity;

            foreach (float distance in Distances)
            {
                foreach (float offset in AngleOffsets)
                {
                    Vector2 candidate = origin + Direction(preferredAngle + offset) * distance;
                    float hard = Clearance(terrain, siteIndex, candidate, landmarks, taken);
                    if (hard > bestHardScore)
                    {
                        bestHardScore = hard;
                        bestHard = candidate;
                    }

                    if (hard < ComfortableClearance)
                    {
                        continue;
                    }

                    float soft = PropClearance(candidate, props);
                    if (soft >= 0f)
                    {
                        return candidate;
                    }

                    if (soft > bestSoftScore)
                    {
                        bestSoftScore = soft;
                        bestSoft = candidate;
                    }
                }
            }

            return bestSoft ?? bestHard;
        }

        /// <summary>
        /// Smallest gap (m) between a hint at <paramref name="p"/> and anything it must not touch; negative = overlap.
        /// Wrong region / wet ground count as a large overlap.
        /// </summary>
        public static float Clearance(WorldMapTerrain terrain, int siteIndex, Vector2 p, IReadOnlyList<Vector2> landmarks, IReadOnlyList<Vector2> taken)
        {
            WorldMapSite own = terrain.Sites[siteIndex];
            if (!terrain.IsDryLand(p, HintRadius * 0.5f) || terrain.DominantRegion(p) != own.Region)
            {
                return -10f;
            }

            float clearance = terrain.DistanceToPath(p) - WorldMapTerrain.PathHalfWidth - WorldMapDressingLayout.PathClearance - HintRadius;
            if (terrain.HasRiver)
            {
                clearance = Mathf.Min(clearance, terrain.DistanceToRiver(p) - WorldMapTerrain.RiverHalfWidth - HintRadius);
                clearance = Mathf.Min(clearance, Vector2.Distance(p, terrain.BridgeCenter) - 2.5f - HintRadius);
            }

            for (int s = 0; s < terrain.Sites.Count; s++)
            {
                WorldMapSite site = terrain.Sites[s];
                Vector2 delta = p - site.Position;
                // Hints are about tree height: they keep the tall-prop distance from every node.
                clearance = Mathf.Min(clearance, delta.magnitude - WorldMapDressingLayout.NodeClearRadius(site, true) - HintRadius);

                // Out of the strip between the camera (-z) and each node: the hero and labels stay readable.
                if (delta.y < 0f && -delta.y < WorldMapDressingLayout.FrontStripDepth)
                {
                    clearance = Mathf.Min(clearance, Mathf.Abs(delta.x) - WorldMapDressingLayout.FrontStripHalfWidth - HintRadius);
                }
            }

            if (landmarks != null)
            {
                foreach (Vector2 landmark in landmarks)
                {
                    clearance = Mathf.Min(clearance, Vector2.Distance(p, landmark) - WorldMapDressingLayout.LandmarkRadius - HintRadius);
                }
            }

            if (taken != null)
            {
                foreach (Vector2 other in taken)
                {
                    clearance = Mathf.Min(clearance, Vector2.Distance(p, other) - 2f * HintRadius);
                }
            }

            return clearance;
        }

        /// <summary>Gap between a hint at <paramref name="p"/> and the nearest scattered prop (a light overlap with a bush is tolerated).</summary>
        public static float PropClearance(Vector2 p, IReadOnlyList<WorldMapPropPlacement> props)
        {
            float clearance = float.PositiveInfinity;
            if (props != null)
            {
                for (int i = 0; i < props.Count; i++)
                {
                    clearance = Mathf.Min(clearance, Vector2.Distance(p, props[i].Ground) - props[i].Radius - HintRadius * 0.6f);
                }
            }

            return clearance;
        }

        private static Vector2 Direction(float degrees)
        {
            float radians = degrees * Mathf.Deg2Rad;
            return new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));
        }

        // ---------------------------------------------------------------
        // Meshes (local frame: ground at y = 0, ~1.6 wide)
        // ---------------------------------------------------------------

        /// <summary>The floating "something here" marker: a gold diamond (shown while a revealed secret waits).</summary>
        public static void AppendMarker(WorldMapMeshBuilder b)
        {
            b.Cylinder(Vector3.zero, 0.32f, 0f, 0.42f, 4, WorldMapSwatch.Gold);
            // Lower half: a frustum from a point up to the shared rim (a mirrored matrix would flip the winding).
            b.Cylinder(new Vector3(0f, -0.42f, 0f), 0f, 0.32f, 0.42f, 4, WorldMapSwatch.Lantern);
        }

        /// <summary>The always-visible hint prop of a secret.</summary>
        public static void AppendHint(WorldMapMeshBuilder b, SecretHintKind kind)
        {
            switch (kind)
            {
                case SecretHintKind.FlourHouseRuin:
                    // Broken walls, a fallen roof beam and the old millstone.
                    b.Box(new Vector3(-0.35f, 0.3f, 0f), new Vector3(0.16f, 0.6f, 1.1f), WorldMapSwatch.Ruin);
                    b.Box(new Vector3(0.1f, 0.22f, 0.5f), new Vector3(0.9f, 0.44f, 0.16f), WorldMapSwatch.RuinDark);
                    b.Box(new Vector3(0.45f, 0.14f, -0.15f), new Vector3(0.16f, 0.28f, 0.6f), WorldMapSwatch.Ruin);
                    b.Box(new Vector3(0.05f, 0.5f, 0.05f), new Vector3(1.2f, 0.08f, 0.1f), WorldMapSwatch.WoodDark, 28f);
                    b.Cylinder(new Vector3(0.05f, 0f, -0.2f), 0.26f, 0.26f, 0.12f, 10, WorldMapSwatch.Rock, WorldMapSwatch.RockDark);
                    break;

                case SecretHintKind.HollowTree:
                    // A wide old jatobá with a dark hollow in the trunk.
                    b.Cylinder(Vector3.zero, 0.42f, 0.3f, 1.2f, 8, WorldMapSwatch.Trunk);
                    b.Box(new Vector3(0f, 0.45f, -0.34f), new Vector3(0.26f, 0.34f, 0.08f), WorldMapSwatch.Black);
                    b.Blob(new Vector3(0f, 1.55f, 0f), new Vector3(1.7f, 1.0f, 1.6f), WorldMapSwatch.LeafDark);
                    b.Blob(new Vector3(0.35f, 1.85f, 0.2f), new Vector3(0.9f, 0.7f, 0.9f), WorldMapSwatch.Leaf);
                    break;

                case SecretHintKind.MandacaruChest:
                    // A tall mandacaru with two arms and a chest corner poking out of the sand.
                    b.Cylinder(Vector3.zero, 0.16f, 0.14f, 1.5f, 6, WorldMapSwatch.Cactus);
                    b.Cylinder(new Vector3(0.28f, 0.6f, 0f), 0.09f, 0.08f, 0.55f, 6, WorldMapSwatch.CactusDark);
                    b.Box(new Vector3(0.17f, 0.62f, 0f), new Vector3(0.24f, 0.12f, 0.12f), WorldMapSwatch.CactusDark);
                    b.Cylinder(new Vector3(-0.26f, 0.85f, 0f), 0.08f, 0.07f, 0.4f, 6, WorldMapSwatch.Cactus);
                    b.Box(new Vector3(-0.17f, 0.86f, 0f), new Vector3(0.2f, 0.1f, 0.1f), WorldMapSwatch.Cactus);
                    b.Blob(new Vector3(0.1f, 0f, -0.45f), new Vector3(0.9f, 0.3f, 0.6f), WorldMapSwatch.Sand, 0f, true);
                    b.Box(new Vector3(0.12f, 0.1f, -0.5f), new Vector3(0.34f, 0.18f, 0.24f), WorldMapSwatch.Wood, 20f, WorldMapSwatch.Gold);
                    break;

                case SecretHintKind.DryWell:
                    // Stone ring, dark mouth and a wooden frame.
                    b.Cylinder(Vector3.zero, 0.42f, 0.4f, 0.32f, 10, WorldMapSwatch.SandRock, WorldMapSwatch.SandRock);
                    b.Cylinder(new Vector3(0f, 0.3f, 0f), 0.3f, 0.3f, 0.03f, 10, WorldMapSwatch.Black, WorldMapSwatch.Black);
                    b.Box(new Vector3(-0.38f, 0.5f, 0f), new Vector3(0.07f, 0.7f, 0.07f), WorldMapSwatch.Wood);
                    b.Box(new Vector3(0.38f, 0.5f, 0f), new Vector3(0.07f, 0.7f, 0.07f), WorldMapSwatch.Wood);
                    b.Box(new Vector3(0f, 0.84f, 0f), new Vector3(0.86f, 0.07f, 0.07f), WorldMapSwatch.WoodDark);
                    break;

                case SecretHintKind.WaterfallCave:
                    // A rock wall with a waterfall strip, its pool and a dark mouth peeking beside it.
                    b.Box(new Vector3(0f, 0.75f, 0.25f), new Vector3(1.5f, 1.5f, 0.5f), WorldMapSwatch.Rock, 0f, WorldMapSwatch.Snow);
                    b.Box(new Vector3(-0.15f, 0.75f, -0.02f), new Vector3(0.36f, 1.4f, 0.06f), WorldMapSwatch.Crystal);
                    b.Box(new Vector3(0.38f, 0.28f, -0.01f), new Vector3(0.3f, 0.42f, 0.04f), WorldMapSwatch.Black);
                    b.Blob(new Vector3(-0.15f, 0f, -0.3f), new Vector3(0.9f, 0.12f, 0.55f), WorldMapSwatch.DeepWater, 0f, true);
                    break;

                case SecretHintKind.TropeiroCairn:
                    // Stacked stones with a little bell on a stick.
                    b.Blob(new Vector3(0f, 0.16f, 0f), new Vector3(0.75f, 0.36f, 0.7f), WorldMapSwatch.Rock, 0f, true);
                    b.Blob(new Vector3(0.04f, 0.44f, 0f), new Vector3(0.55f, 0.3f, 0.5f), WorldMapSwatch.RockDark, 40f, true);
                    b.Blob(new Vector3(-0.02f, 0.66f, 0.02f), new Vector3(0.36f, 0.22f, 0.34f), WorldMapSwatch.Pebble, 80f, true);
                    b.Box(new Vector3(0.05f, 0.95f, 0f), new Vector3(0.05f, 0.5f, 0.05f), WorldMapSwatch.Wood);
                    b.Cylinder(new Vector3(0.05f, 1.0f, -0.06f), 0.09f, 0.04f, 0.14f, 6, WorldMapSwatch.Gold);
                    break;

                case SecretHintKind.ForgottenWorkshop:
                    // A crooked wooden shed with a big gear leaning on it.
                    b.Box(new Vector3(0f, 0.32f, 0f), new Vector3(0.9f, 0.64f, 0.7f), WorldMapSwatch.Wood, 0f, WorldMapSwatch.WoodDark);
                    b.Box(new Vector3(0f, 0.68f, 0f), new Vector3(1.05f, 0.08f, 0.85f), WorldMapSwatch.Roof, 6f);
                    b.Box(new Vector3(-0.15f, 0.22f, -0.36f), new Vector3(0.24f, 0.4f, 0.04f), WorldMapSwatch.Black);
                    b.Cylinder(new Vector3(0.55f, 0f, -0.25f), 0.24f, 0.24f, 0.08f, 8, WorldMapSwatch.Gold, WorldMapSwatch.Lantern);
                    break;

                case SecretHintKind.RockPaintings:
                    // A cave slab with red figures and two crystals.
                    b.Box(new Vector3(0f, 0.6f, 0.1f), new Vector3(1.3f, 1.2f, 0.35f), WorldMapSwatch.CaveStone, 0f, WorldMapSwatch.CaveDark);
                    b.Box(new Vector3(-0.3f, 0.75f, -0.08f), new Vector3(0.08f, 0.3f, 0.02f), WorldMapSwatch.Banner);
                    b.Box(new Vector3(-0.3f, 0.75f, -0.08f), new Vector3(0.24f, 0.05f, 0.02f), WorldMapSwatch.Banner);
                    b.Box(new Vector3(0.05f, 0.8f, -0.08f), new Vector3(0.08f, 0.36f, 0.02f), WorldMapSwatch.Banner);
                    b.Box(new Vector3(0.32f, 0.62f, -0.08f), new Vector3(0.3f, 0.05f, 0.02f), WorldMapSwatch.Banner, 20f);
                    b.Cylinder(new Vector3(0.6f, 0f, -0.2f), 0.12f, 0f, 0.55f, 5, WorldMapSwatch.Crystal);
                    b.Cylinder(new Vector3(0.75f, 0f, -0.05f), 0.08f, 0f, 0.35f, 5, WorldMapSwatch.Crystal);
                    break;
            }
        }
    }
}
