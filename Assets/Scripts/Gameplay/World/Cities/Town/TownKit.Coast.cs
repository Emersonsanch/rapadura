using UnityEngine;

namespace Rapadura.Gameplay.World.Cities.Town
{
    /// <summary>
    /// The pieces of Taa Pwani, the Costa da Lâmpada (Docs/Mapas100.md região 5, map 25; Kilwa, Lamu and Gedi as the real
    /// reference): the coral-stone front of the Swahili houses (<see cref="SwahiliFront"/>: a carved teak door frame studded
    /// with brass, the <i>baraza</i> bench, stepped merlons on the roof terrace), the dhows moored in tidal pools with
    /// their triangular sails, the Farol de Coral (a lamp tower of coral stone, a lighthouse with no worship), mangroves
    /// on stilt roots, leaning coconut palms, the pearl trays of Mwana's bazaar and Zawadi's mangrove-root boots.
    /// Same rules as the rest of the kit: flat colours from <see cref="TownPalette"/>, low-poly, deterministic.
    /// </summary>
    public static partial class TownKit
    {
        /// <summary>Length (local z, bow to +z) of a <see cref="CityPropKind.Dhow"/>'s hull × its scale: the collider box.</summary>
        public const float DhowLength = 6.4f;

        /// <summary>Beam (local x) of a dhow's hull × its scale.</summary>
        public const float DhowBeam = 2.0f;

        /// <summary>Collider radius of the Farol de Coral (× its scale): the square base.</summary>
        public const float CoralTowerSolidRadius = 1.45f;

        // ------------------------------------------------------------------
        // The coral-stone house front
        // ------------------------------------------------------------------

        /// <summary>
        /// The Swahili front of a <see cref="TownBuildingSpec.CoralStone"/> house, in the building's local frame (front −z):
        /// the carved teak frame round the door (thick jambs, a deep lintel with brass rosettes, brass studs on the leaves),
        /// the <i>baraza</i> (a plastered stone bench) along the facade on the side away from the keeper (only when no shop
        /// dressing stands there), and stepped merlons along the front parapet of the roof terrace.
        /// </summary>
        public static void SwahiliFront(TownMeshBuilder b, TownBuildingSpec spec, float hw, float zFront, float zFacade, float top)
        {
            float y0 = TownBuildingBuilder.FloorY;
            float doorX = spec.DoorOffset * spec.Width;
            bool door = spec.Kind == CityLotKind.House || spec.Kind == CityLotKind.Shop || spec.Kind == CityLotKind.Grandma;
            float doorWidth = spec.WideDoor ? 1.7f : 1.05f;
            if (door)
            {
                float hd = doorWidth * 0.5f;
                float h = TownBuildingBuilder.DoorHeight;
                TownSwatch teak = spec.JoineryColor;

                // Jambs and the deep carved lintel.
                foreach (float side in new[] { -1f, 1f })
                {
                    float x = doorX + side * (hd + 0.2f);
                    b.BoxMinMax(new Vector3(x - 0.1f, y0, zFacade - 0.13f), new Vector3(x + 0.1f, y0 + h + 0.2f, zFacade - 0.01f), teak);
                    // A carved chain down each jamb.
                    for (float y = y0 + 0.35f; y < y0 + h; y += 0.36f)
                    {
                        b.Box(new Vector3(x, y, zFacade - 0.14f), new Vector3(0.09f, 0.09f, 0.03f), TownSwatch.WoodDark, 45f);
                    }
                }

                b.BoxMinMax(new Vector3(doorX - hd - 0.32f, y0 + h + 0.18f, zFacade - 0.15f), new Vector3(doorX + hd + 0.32f, y0 + h + 0.58f, zFacade - 0.01f), teak);
                for (int i = -1; i <= 1; i++)
                {
                    b.Box(new Vector3(doorX + i * (hd * 0.6f), y0 + h + 0.38f, zFacade - 0.16f), new Vector3(0.13f, 0.13f, 0.03f), spec.AccentColor, 45f);
                }

                // Brass studs on the leaves, four rows of three.
                for (int row = 0; row < 4; row++)
                {
                    for (int col = -1; col <= 1; col++)
                    {
                        float x = doorX + col * hd * 0.55f;
                        float y = y0 + 0.4f + row * (h - 0.6f) / 3f;
                        b.Box(new Vector3(x, y, zFacade - 0.075f), new Vector3(0.06f, 0.06f, 0.03f), spec.AccentColor);
                    }
                }
            }

            // The baraza: on the side away from the keeper, unless the goods of a shop already stand there.
            if (door && spec.Dressing == TownShopDressing.None)
            {
                float away = -TownBuildingBuilder.KeeperSide(spec);
                float from = doorX + away * (doorWidth * 0.5f + 0.5f);
                float to = away > 0f ? Mathf.Min(hw - 0.35f, from + 2.2f) : Mathf.Max(-hw + 0.35f, from - 2.2f);
                if (Mathf.Abs(to - from) > 0.8f)
                {
                    float x0 = Mathf.Min(from, to);
                    float x1 = Mathf.Max(from, to);
                    b.BoxMinMax(new Vector3(x0, 0f, zFacade - 0.5f), new Vector3(x1, y0 + 0.42f, zFacade), spec.WallColor, spec.TrimColor);
                    b.BoxMinMax(new Vector3(x0 - 0.04f, y0 + 0.42f, zFacade - 0.54f), new Vector3(x1 + 0.04f, y0 + 0.48f, zFacade), spec.TrimColor);
                }
            }

            // Stepped merlons on the front parapet of the roof terrace (Kilwa, Gedi).
            if (spec.Roof == TownRoofKind.Flat)
            {
                float yTop = top + TownBuildingBuilder.ParapetHeight + 0.07f;
                float z = zFront + 0.02f;
                for (float x = -hw + 0.35f; x <= hw - 0.3f; x += 0.7f)
                {
                    b.Box(new Vector3(x, yTop + 0.13f, z), new Vector3(0.3f, 0.26f, 0.2f), spec.WallColor);
                    b.Box(new Vector3(x, yTop + 0.34f, z), new Vector3(0.16f, 0.16f, 0.16f), spec.WallColor, 0f, spec.TrimColor);
                }
            }
        }

        // ------------------------------------------------------------------
        // Props
        // ------------------------------------------------------------------

        /// <summary>
        /// A dhow moored in a tidal pool (local z = bow): an elliptical pool of sea with a ring of foam, the teak hull in
        /// segments rising to a sharp raked stem and a high square stern (poop deck with a lantern), the mast raked forward,
        /// the long lateen yard and its triangular sail (variant 1: the sail furled along the yard).
        /// </summary>
        private static void Dhow(TownMeshBuilder b, float s, int variant)
        {
            float l = DhowLength * s;
            float beam = DhowBeam * s;

            // The tidal pool: an ellipse of sea and its pale rim (walkable decoration, drawn flat).
            b.With(Vector3.zero, 0f, new Vector3(0.62f, 1f, 1f), () =>
            {
                b.Disc(new Vector3(0f, 0.012f, 0f), l * 0.66f, 16, TownSwatch.SandDark);
                b.Disc(new Vector3(0f, 0.02f, 0f), l * 0.6f, 16, TownSwatch.Sea);
                b.Ring(new Vector3(0f, 0.024f, 0f), l * 0.56f, l * 0.6f, 16, TownSwatch.Pearl);
            });

            // The hull: segments narrowing to bow and stern, the sheer rising at both ends.
            const int segments = 9;
            float seg = l / segments;
            for (int i = 0; i < segments; i++)
            {
                float t = (i + 0.5f) / segments * 2f - 1f;
                float width = beam * Mathf.Sqrt(Mathf.Max(0.08f, 1f - t * t * 0.92f));
                float deck = (0.95f + t * t * 0.45f) * s;
                float z = -l * 0.5f + (i + 0.5f) * seg;
                b.Box(new Vector3(0f, 0.22f * s, z), new Vector3(width * 0.85f, 0.44f * s, seg + 0.02f), TownSwatch.WoodDark);
                b.Box(new Vector3(0f, (0.44f * s + deck) * 0.5f, z), new Vector3(width, deck - 0.44f * s, seg + 0.02f), TownSwatch.Teak, 0f, TownSwatch.WoodLight);
                b.Box(new Vector3(0f, deck - 0.12f * s, z), new Vector3(width + 0.06f, 0.08f * s, seg + 0.03f), TownSwatch.Sail);
            }

            // Raked stem at the bow, the high stern with its poop deck, rail and lantern.
            b.Log(new Vector3(0f, 0.3f * s, l * 0.47f), new Vector3(0f, 1.9f * s, l * 0.5f + 0.7f * s), 0.12f * s, TownSwatch.Teak, 4);
            b.Box(new Vector3(0f, 1.0f * s, -l * 0.42f), new Vector3(beam * 0.75f, 1.6f * s, 0.8f * s), TownSwatch.Teak, 0f, TownSwatch.WoodLight);
            b.Box(new Vector3(0f, 1.3f * s, -l * 0.465f), new Vector3(beam * 0.7f, 0.5f * s, 0.05f), TownSwatch.Sea);
            b.Log(new Vector3(-beam * 0.36f, 1.8f * s, -l * 0.38f), new Vector3(beam * 0.36f, 1.8f * s, -l * 0.38f), 0.03f * s, TownSwatch.WoodDark, 4);
            b.Log(new Vector3(beam * 0.3f, 1.8f * s, -l * 0.44f), new Vector3(beam * 0.3f, 2.3f * s, -l * 0.44f), 0.03f * s, TownSwatch.Iron, 4);
            b.Blob(new Vector3(beam * 0.3f, 2.35f * s, -l * 0.44f), new Vector3(0.2f, 0.3f, 0.2f) * s, TownSwatch.LampGlow);

            // Mast (raked forward) and the lateen yard, low at the bow, high at the stern.
            var mastFoot = new Vector3(0f, 1.0f * s, l * 0.08f);
            var mastTop = new Vector3(0f, 6.2f * s, l * 0.13f);
            b.Log(mastFoot, mastTop, 0.09f * s, TownSwatch.Teak, 6);
            var yardLow = new Vector3(0f, 1.7f * s, l * 0.46f);
            var yardHigh = new Vector3(0f, 7.0f * s, -l * 0.36f);
            b.Log(yardLow, yardHigh, 0.06f * s, TownSwatch.WoodDark, 4);
            b.Log(mastTop, new Vector3(0f, 1.1f * s, -l * 0.3f), 0.015f * s, TownSwatch.Thatch, 3);
            b.Log(mastTop, new Vector3(0f, 1.3f * s, l * 0.45f), 0.015f * s, TownSwatch.Thatch, 3);

            if (variant == 1)
            {
                // Furled: the sail rolled along the yard.
                b.Log(Vector3.Lerp(yardLow, yardHigh, 0.08f) + Vector3.down * 0.12f * s, Vector3.Lerp(yardLow, yardHigh, 0.9f) + Vector3.down * 0.12f * s, 0.16f * s, TownSwatch.Sail, 6);
                return;
            }

            // Open: a triangle from the yard down to the clew near the stern, bellied a little to one side.
            var clew = new Vector3(0.25f * s, 1.4f * s, -l * 0.3f);
            Vector3 belly = (Vector3.Lerp(yardLow, yardHigh, 0.5f) + clew) * 0.5f + new Vector3(0.55f * s, 0f, 0f);
            Vector3 a = yardLow + Vector3.down * 0.08f * s;
            Vector3 c = yardHigh + Vector3.down * 0.08f * s;
            foreach ((Vector3 p, Vector3 q, Vector3 r) in new[] { (a, c, belly), (c, clew, belly), (clew, a, belly) })
            {
                b.Triangle(p, q, r, TownSwatch.Sail);
                b.Triangle(p, r, q, TownSwatch.Sail);
            }
        }

        /// <summary>
        /// The Farol de Coral of Taa Pwani: a square coral-stone base with a studded teak door, a fluted shaft tapering up
        /// to a brass band, the lamp gallery (teak deck, four posts, the great lamp that glows at night) and a small coral
        /// cap with a brass finial. A lighthouse for the dhows (Taa Pwani = "the Coast of the Lamp"), never a place of worship.
        /// </summary>
        private static void CoralTower(TownMeshBuilder b, float s)
        {
            float baseH = 1.3f * s;
            float shaftTop = 8.6f * s;

            // Base and door (towards the camera, −z).
            b.Box(new Vector3(0f, 0.12f * s, 0f), new Vector3(2.2f, 0.24f, 2.2f) * s, TownSwatch.Laterite);
            b.Box(new Vector3(0f, 0.24f * s + baseH * 0.5f, 0f), new Vector3(2.0f * s, baseH, 2.0f * s), TownSwatch.CoralLime, 0f, TownSwatch.Sail);
            b.BoxMinMax(new Vector3(-0.38f * s, 0.24f * s, -1.04f * s), new Vector3(0.38f * s, 1.3f * s, -0.99f * s), TownSwatch.Teak);
            for (int i = 0; i < 3; i++)
            {
                b.Box(new Vector3(0f, 0.5f * s + i * 0.3f * s, -1.06f * s), new Vector3(0.06f, 0.06f, 0.03f) * s, TownSwatch.Gold);
            }

            // The fluted shaft.
            float y0 = 0.24f * s + baseH;
            b.Cylinder(new Vector3(0f, y0, 0f), 0.95f * s, 0.7f * s, shaftTop - y0, 8, TownSwatch.CoralLime);
            for (int i = 0; i < 8; i++)
            {
                float a = (i + 0.5f) / 8f * Mathf.PI * 2f;
                var dir = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a));
                b.Log(dir * 0.9f * s + Vector3.up * y0, dir * 0.66f * s + Vector3.up * (shaftTop - 0.6f * s), 0.05f * s, TownSwatch.Sail, 4);
            }

            // Little windows up the shaft (lit at night), the brass band.
            foreach (float y in new[] { 3.6f, 5.8f })
            {
                b.Box(new Vector3(0f, y * s, -(0.95f - (y - 1.5f) / 7.1f * 0.25f) * s), new Vector3(0.3f, 0.5f, 0.06f) * s, TownSwatch.WindowLit);
            }

            b.Cylinder(new Vector3(0f, shaftTop - 0.55f * s, 0f), 0.74f * s, 0.74f * s, 0.18f * s, 8, TownSwatch.Gold, TownSwatch.Gold);

            // Gallery, lamp, cap.
            b.Cylinder(new Vector3(0f, shaftTop, 0f), 1.2f * s, 1.25f * s, 0.16f * s, 10, TownSwatch.Teak, TownSwatch.WoodLight);
            for (int i = 0; i < 4; i++)
            {
                float a = (i * 90f + 45f) * Mathf.Deg2Rad;
                var dir = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a));
                b.Log(dir * 0.75f * s + Vector3.up * (shaftTop + 0.16f * s), dir * 0.75f * s + Vector3.up * (shaftTop + 1.25f * s), 0.06f * s, TownSwatch.Teak, 4);
            }

            b.Ring(new Vector3(0f, shaftTop + 0.6f * s, 0f), 1.12f * s, 1.2f * s, 10, TownSwatch.WoodDark);
            b.Blob(new Vector3(0f, shaftTop + 0.75f * s, 0f), new Vector3(0.75f, 0.9f, 0.75f) * s, TownSwatch.LampGlow);
            b.Cylinder(new Vector3(0f, shaftTop + 1.25f * s, 0f), 1.0f * s, 0f, 0.95f * s, 8, TownSwatch.CoralLime);
            b.Log(new Vector3(0f, shaftTop + 2.1f * s, 0f), new Vector3(0f, shaftTop + 2.55f * s, 0f), 0.05f * s, TownSwatch.Gold, 4);
            b.Blob(new Vector3(0f, shaftTop + 2.6f * s, 0f), new Vector3(0.16f, 0.16f, 0.16f) * s, TownSwatch.Gold);
        }

        /// <summary>A mangrove on stilt roots: a patch of dark mud with brackish water, arching roots, a short trunk, a dense crown.</summary>
        private static void Mangrove(TownMeshBuilder b, float s, int variant)
        {
            b.Disc(new Vector3(0f, 0.012f, 0f), 1.7f * s, 10, TownSwatch.DirtDark, variant * 0.3f);
            b.Disc(new Vector3(0.15f * s, 0.02f, -0.1f * s), 1.1f * s, 9, TownSwatch.Water, variant * 0.5f);

            int roots = 7 + (variant & 1);
            for (int i = 0; i < roots; i++)
            {
                float a = (i + 0.3f * variant) / roots * Mathf.PI * 2f;
                var dir = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a));
                Vector3 foot = dir * (1.1f + (i % 3) * 0.15f) * s;
                Vector3 knee = dir * 0.7f * s + Vector3.up * (0.95f + (i & 1) * 0.2f) * s;
                Vector3 join = dir * 0.15f * s + Vector3.up * 1.35f * s;
                b.Log(foot, knee, 0.06f * s, TownSwatch.MangroveRoot, 4);
                b.Log(knee, join, 0.07f * s, TownSwatch.MangroveRoot, 4);
            }

            b.Log(new Vector3(0f, 1.2f * s, 0f), new Vector3(0.12f * s, 2.7f * s, 0.05f * s), 0.16f * s, TownSwatch.MangroveRoot, 5);
            b.Crown(new Vector3(0.1f * s, 3.0f * s, 0f), new Vector3(2.8f, 1.5f, 2.5f) * s, TownSwatch.Mangrove, variant * 40f);
            b.Crown(new Vector3(-0.5f * s, 3.5f * s, 0.4f * s), new Vector3(1.7f, 1.0f, 1.6f) * s, TownSwatch.LeafDark, 70f + variant * 20f);
        }

        // ------------------------------------------------------------------
        // Vegetation of the coast
        // ------------------------------------------------------------------

        /// <summary>The coast's trees: 0 / 2 a leaning coconut palm, 1 a baobab, 3 a casuarina.</summary>
        private static void CoastTree(TownMeshBuilder b, int variant, float s)
        {
            switch (variant)
            {
                case 1:
                    Baobab(b, s);
                    return;
                case 3:
                    Feathery(b, s, TownSwatch.Pine);
                    return;
                default:
                    CoconutPalm(b, s, variant == 2 ? -1f : 1f);
                    return;
            }
        }

        /// <summary>A coconut palm leaning towards the sea: a ringed, curving trunk, drooping fronds and three coconuts.</summary>
        private static void CoconutPalm(TownMeshBuilder b, float s, float lean)
        {
            Vector3 previous = Vector3.zero;
            const int pieces = 5;
            for (int i = 1; i <= pieces; i++)
            {
                float t = i / (float)pieces;
                var p = new Vector3(lean * 1.1f * t * t, 4.8f * t, 0.3f * t) * s;
                b.Log(previous, p, Mathf.Lerp(0.2f, 0.13f, t) * s, (i & 1) == 0 ? TownSwatch.Trunk : TownSwatch.WoodWeathered, 6);
                previous = p;
            }

            Vector3 top = previous;
            for (int i = 0; i < 7; i++)
            {
                float a = (i / 7f) * Mathf.PI * 2f + 0.2f;
                var dir = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a));
                var side = new Vector3(-dir.z, 0f, dir.x) * 0.32f * s;
                Vector3 mid = top + dir * 1.0f * s + Vector3.up * 0.25f * s;
                Vector3 tip = top + dir * 2.0f * s + Vector3.down * 0.75f * s;
                TownSwatch leaf = (i & 1) == 0 ? TownSwatch.Mangrove : TownSwatch.Poplar;
                b.TwoSidedQuad(top, mid + side, tip, mid - side, leaf);
            }

            for (int i = 0; i < 3; i++)
            {
                float a = i / 3f * Mathf.PI * 2f;
                b.Blob(top + new Vector3(Mathf.Cos(a) * 0.22f, -0.25f, Mathf.Sin(a) * 0.22f) * s, new Vector3(0.24f, 0.26f, 0.24f) * s, TownSwatch.WoodDark);
            }
        }

        /// <summary>A sea-grape shrub of the dunes: round leathery leaves in two greens.</summary>
        private static void CoastBush(TownMeshBuilder b, float s)
        {
            b.Crown(new Vector3(0f, 0.4f * s, 0f), new Vector3(1.2f, 0.8f, 1.1f) * s, TownSwatch.Mangrove, 25f);
            b.Crown(new Vector3(0.25f * s, 0.62f * s, -0.15f * s), new Vector3(0.7f, 0.45f, 0.65f) * s, TownSwatch.Vine, 70f);
        }

        // ------------------------------------------------------------------
        // Dressings of the residents' fronts
        // ------------------------------------------------------------------

        /// <summary>Mwana's pearl bazaar and Zawadi's root boots, in front of their doors (same anchor as <see cref="Dressing"/>).</summary>
        private static void CoastDressing(TownMeshBuilder b, TownBuildingSpec spec, float x, float y, float z, float zFacade)
        {
            switch (spec.Dressing)
            {
                case TownShopDressing.Pearls:
                    // A low teak table with the pearl trays, a brass scale, a rolled kanga leaning on the wall.
                    b.Box(new Vector3(x, y + 0.62f, z - 0.35f), new Vector3(1.3f, 0.06f, 0.62f), TownSwatch.Teak, 0f, TownSwatch.WoodLight);
                    foreach (Vector2 l in new[] { new Vector2(-0.58f, -0.25f), new Vector2(0.58f, -0.25f), new Vector2(-0.58f, 0.25f), new Vector2(0.58f, 0.25f) })
                    {
                        b.Log(new Vector3(x + l.x, y, z - 0.35f + l.y), new Vector3(x + l.x, y + 0.6f, z - 0.35f + l.y), 0.03f, TownSwatch.WoodDark, 4);
                    }

                    PearlTrays(b, new Vector3(x - 0.15f, y + 0.65f, z - 0.35f), 0.42f);
                    b.Log(new Vector3(x + 0.5f, y + 0.65f, z - 0.35f), new Vector3(x + 0.5f, y + 1.05f, z - 0.35f), 0.015f, TownSwatch.Gold, 4);
                    b.Log(new Vector3(x + 0.32f, y + 1.05f, z - 0.35f), new Vector3(x + 0.68f, y + 1.05f, z - 0.35f), 0.012f, TownSwatch.Gold, 4);
                    foreach (float px in new[] { 0.32f, 0.68f })
                    {
                        b.Cylinder(new Vector3(x + px, y + 0.85f, z - 0.35f), 0.08f, 0.1f, 0.03f, 7, TownSwatch.Gold, TownSwatch.Gold);
                    }

                    b.Log(new Vector3(x - 0.7f, y, zFacade - 0.1f), new Vector3(x - 0.62f, y + 1.1f, zFacade - 0.05f), 0.09f, TownSwatch.ClothRed, 6, TownSwatch.Sail);
                    break;

                case TownShopDressing.RootWeaver:
                    // Bundles of mangrove root leaning on the wall, a rack with two pairs of root boots, the soaking tub.
                    for (int i = 0; i < 3; i++)
                    {
                        float bx = x - 0.75f + i * 0.18f;
                        b.Log(new Vector3(bx, y, zFacade - 0.35f), new Vector3(bx + 0.08f, y + 1.3f, zFacade - 0.06f), 0.07f, TownSwatch.MangroveRoot, 5);
                    }

                    foreach (float px in new[] { -0.15f, 0.65f })
                    {
                        b.Log(new Vector3(x + px, y, z - 0.45f), new Vector3(x + px, y + 1.0f, z - 0.45f), 0.03f, TownSwatch.WoodDark, 4);
                    }

                    b.Log(new Vector3(x - 0.15f, y + 1.0f, z - 0.45f), new Vector3(x + 0.65f, y + 1.0f, z - 0.45f), 0.03f, TownSwatch.WoodDark, 4);
                    foreach (float bx in new[] { 0.05f, 0.2f, 0.4f, 0.55f })
                    {
                        b.Box(new Vector3(x + bx, y + 0.72f, z - 0.45f), new Vector3(0.11f, 0.3f, 0.2f), TownSwatch.MangroveRoot, 0f, TownSwatch.Mangrove);
                    }

                    b.Cylinder(new Vector3(x + 0.25f, y, z - 1.0f), 0.32f, 0.36f, 0.4f, 8, TownSwatch.Teak, TownSwatch.Water);
                    break;
            }
        }

        /// <summary>Three shallow teak trays of pearls and cowries side by side, centred on <paramref name="at"/> (its y = the table top).</summary>
        public static void PearlTrays(TownMeshBuilder b, Vector3 at, float spacing = 0.6f)
        {
            for (int t = -1; t <= 1; t++)
            {
                Vector3 c = at + new Vector3(t * spacing, 0f, 0f);
                b.Box(c + Vector3.up * 0.03f, new Vector3(0.36f, 0.06f, 0.36f), TownSwatch.Teak, 0f, TownSwatch.WoodLight);
                TownSwatch bead = t == 0 ? TownSwatch.Sail : TownSwatch.Pearl;
                for (int i = 0; i < 5; i++)
                {
                    float a = i / 5f * Mathf.PI * 2f + t;
                    b.Blob(c + new Vector3(Mathf.Cos(a) * 0.1f, 0.1f, Mathf.Sin(a) * 0.1f), new Vector3(0.07f, 0.07f, 0.07f), bead);
                }

                b.Blob(c + Vector3.up * 0.11f, new Vector3(0.09f, 0.08f, 0.09f), t == 0 ? TownSwatch.Pearl : TownSwatch.Sail);
            }
        }
    }
}
