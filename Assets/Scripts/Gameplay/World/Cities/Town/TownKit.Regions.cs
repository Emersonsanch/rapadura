using System.Collections.Generic;
using UnityEngine;

namespace Rapadura.Gameplay.World.Cities.Town
{
    /// <summary>
    /// The regional pieces of the town kit (Docs/WorldBible_Maniqueismo.md §2.2–2.4, Docs/Cities.md §5.2):
    /// <list type="bullet">
    /// <item><b>Qocho</b>: rammed-earth lift lines, the grape-drying loft (honeycomb brick lattice), the caravanserai arcade
    /// (brick piers, stepped pointed arches), karez well shafts and the open channel, Yardena's cistern dome, the Meroë-style
    /// ruins of the Artífices (steep pyramid with its chapel, broken lotus columns, ushabtis), the pack camel, the desert poplar;</item>
    /// <item><b>Litema</b>: painted litema bands and the thatch shade of the rondavels, San rock art panels, the Great
    /// Zimbabwe conical tower (and taller dry-stone walls with the chevron frieze in <see cref="WallRun"/>), aloes, the baobab;</item>
    /// <item><b>Lamparinas</b>: the pink tuff cliff with houses cut into it on two levels (ledges, carved stairs, round
    /// windows, lamp niches), rope bridges, strings of lamps across the streets, the vishapakar and the spring fountain;</item>
    /// <item>the dressings of the residents' fronts (saddler, forge, lamps, weaver, drummer, wicks, storyteller, old miner,
    /// cistern, herder, potter).</item>
    /// </list>
    /// Same rules as the rest of the kit: flat colours from <see cref="TownPalette"/>, every module low-poly, deterministic.
    /// Segment props (<see cref="IsSegment"/>) are drawn from the local origin to <c>End − Position</c> with no yaw.
    /// </summary>
    public static partial class TownKit
    {
        public const float CliffDepth = 6f;
        public const float CliffHeight = 8.5f;
        public const float RopeBridgeHeight = 6.3f;
        public const float LampStringHeight = 2.75f;

        /// <summary>Radius of the ring platform of a <see cref="CityPropKind.CanopyTree"/> (× its scale): where its rope bridges land.</summary>
        public const float CanopyPlatformRadius = 3.2f;

        /// <summary>Collider radius of a canopy tree on the ground (× its scale): the trunk and the spiral stair round it.</summary>
        public const float CanopyTrunkSolidRadius = 2.0f;

        /// <summary>Props that run from <see cref="CityProp.Position"/> to <see cref="CityProp.End"/> (no yaw; the view anchors them at their midpoint).</summary>
        public static bool IsSegment(CityPropKind kind) =>
            kind == CityPropKind.FenceRun || kind == CityPropKind.Gate || kind == CityPropKind.KarezChannel || kind == CityPropKind.Cliff ||
            kind == CityPropKind.RopeBridge || kind == CityPropKind.LampString || kind == CityPropKind.Palisade || kind == CityPropKind.RockWall ||
            kind == CityPropKind.MineRails;

        /// <summary>Unit direction of a segment prop and the side its mass stands on (left of the direction: east-going → north).</summary>
        public static Vector2 SegmentBack(Vector2 direction) => new Vector2(-direction.y, direction.x);

        /// <summary>Yaw that turns local +x onto <paramref name="direction"/> (local +z then points to <see cref="SegmentBack"/>).</summary>
        public static float YawAlong(Vector2 direction) => Mathf.Atan2(-direction.y, direction.x) * Mathf.Rad2Deg;

        /// <summary>Draws the regional prop kinds; false when <paramref name="prop"/> is not one of them.</summary>
        private static bool RegionProp(TownMeshBuilder b, CityProp prop, TownStyle style, float s)
        {
            Vector2 d = prop.End - prop.Position;
            switch (prop.Kind)
            {
                case CityPropKind.KarezShaft: KarezShaft(b, s); return true;
                case CityPropKind.KarezChannel: KarezChannel(b, d); return true;
                case CityPropKind.CisternDome: CisternDome(b, s); return true;
                case CityPropKind.Ruin: Ruin(b, prop.Variant, s); return true;
                case CityPropKind.Camel: Camel(b, s); return true;
                case CityPropKind.RockArt: RockArt(b, s); return true;
                case CityPropKind.Cliff: Cliff(b, d, s, prop.Variant); return true;
                case CityPropKind.RopeBridge: RopeBridge(b, d, s); return true;
                case CityPropKind.LampString: LampString(b, d, s); return true;
                case CityPropKind.Vishapakar: Vishapakar(b, s); return true;
                case CityPropKind.Fountain: Fountain(b, s); return true;
                case CityPropKind.ConicalTower: ConicalTower(b, s); return true;
                case CityPropKind.CanopyTree: CanopyTree(b, s); return true;
                case CityPropKind.Dhow: Dhow(b, s, prop.Variant); return true;
                case CityPropKind.CoralTower: CoralTower(b, s); return true;
                case CityPropKind.Mangrove: Mangrove(b, s, prop.Variant); return true;
                case CityPropKind.Palisade: Palisade(b, d, s); return true;
                case CityPropKind.DrumTower: DrumTower(b, s); return true;
                case CityPropKind.ClanTent: ClanTent(b, s, prop.Variant); return true;
                case CityPropKind.ClanPost: ClanPost(b, s, prop.Variant); return true;
                case CityPropKind.Bonfire: Bonfire(b, s); return true;
                case CityPropKind.WarDrum: WarDrum(b, s); return true;
                case CityPropKind.TrainingRing: TrainingRing(b, s); return true;
                case CityPropKind.BronzePillar: BronzePillar(b, s); return true;
                case CityPropKind.GreatForge: GreatForge(b, s); return true;
                case CityPropKind.LavaPit: LavaPit(b, s); return true;
                case CityPropKind.ChainLift: ChainLift(b, s); return true;
                case CityPropKind.RockWall: RockWall(b, d, s, prop.Variant); return true;
                case CityPropKind.MineRails: MineRails(b, d); return true;
                case CityPropKind.MineCart: MineCart(b, s, prop.Variant); return true;
                case CityPropKind.AirPipe: AirPipe(b, s); return true;
                case CityPropKind.NicheStall: NicheStall(b, s, prop.Variant); return true;
                case CityPropKind.Pulley: Pulley(b, s); return true;
                default: return false;
            }
        }

        // ------------------------------------------------------------------
        // Qocho: walls, lofts, arcade
        // ------------------------------------------------------------------

        /// <summary>Rammed earth: the formwork lift lines (a darker course every ~0.55 m) all round the mass.</summary>
        public static void RammedEarthLines(TownMeshBuilder b, float x0, float x1, float z0, float z1, float y0, float y1)
        {
            for (float y = y0 + 0.75f; y < y1 - 0.25f; y += 0.55f)
            {
                b.BoxMinMax(new Vector3(x0 - 0.02f, y, z0 - 0.02f), new Vector3(x1 + 0.02f, y + 0.05f, z1 + 0.02f), TownSwatch.Mud);
            }
        }

        /// <summary>
        /// The grape-drying house on the roof terrace (Turfan): a chamber of honeycomb brick lattice (bricks laid with gaps in a
        /// chequer, dark inside), bunches hanging behind the holes, a flat mud roof. Sits on the side away from the upper room.
        /// </summary>
        public static void DryingLoft(TownMeshBuilder b, TownBuildingSpec spec, float hw, float zFacade, float zBack, float top)
        {
            float w = Mathf.Min(3.2f, spec.Width * 0.48f);
            float x0 = spec.DoorOffset >= 0f ? hw - 0.35f - w : -hw + 0.35f;
            float x1 = x0 + w;
            float z0 = zFacade + (zBack - zFacade) * 0.35f;
            float z1 = zBack - 0.3f;
            const float h = 1.9f;
            float y0 = top;
            // The dark inside (seen through the lattice) and a few bunches.
            b.BoxMinMax(new Vector3(x0 + 0.12f, y0, z0 + 0.12f), new Vector3(x1 - 0.12f, y0 + h - 0.05f, z1 - 0.12f), TownSwatch.DirtDark);
            for (int i = 0; i < 3; i++)
            {
                float x = Mathf.Lerp(x0 + 0.5f, x1 - 0.5f, i / 2f);
                b.Blob(new Vector3(x, y0 + h - 0.55f, z0 + 0.25f), new Vector3(0.18f, 0.34f, 0.18f), i == 1 ? TownSwatch.Vine : TownSwatch.Violet);
            }

            // Corner piers and the lattice of the three visible walls (front, both sides); the back is solid.
            foreach (Vector2 c in new[] { new Vector2(x0, z0), new Vector2(x1, z0), new Vector2(x0, z1), new Vector2(x1, z1) })
            {
                b.Box(new Vector3(c.x, y0 + h * 0.5f, c.y), new Vector3(0.22f, h, 0.22f), TownSwatch.Brick);
            }

            b.BoxMinMax(new Vector3(x0, y0, z1 - 0.1f), new Vector3(x1, y0 + h, z1), TownSwatch.Brick);
            LatticeWall(b, new Vector3(x0, y0, z0), new Vector3(x1, y0, z0), h);
            LatticeWall(b, new Vector3(x0, y0, z0), new Vector3(x0, y0, z1), h);
            LatticeWall(b, new Vector3(x1, y0, z0), new Vector3(x1, y0, z1), h);
            b.BoxMinMax(new Vector3(x0 - 0.1f, y0 + h, z0 - 0.1f), new Vector3(x1 + 0.1f, y0 + h + 0.14f, z1 + 0.1f), TownSwatch.Brick, TownSwatch.Mud);
        }

        /// <summary>A honeycomb brick wall from <paramref name="a"/> to <paramref name="c"/>: bricks in a chequer with open gaps between them.</summary>
        private static void LatticeWall(TownMeshBuilder b, Vector3 a, Vector3 c, float height)
        {
            Vector3 d = c - a;
            float length = d.magnitude;
            if (length < 0.3f)
            {
                return;
            }

            float yaw = Mathf.Atan2(d.x, d.z) * Mathf.Rad2Deg;
            const float brick = 0.34f;
            const float course = 0.3f;
            int columns = Mathf.Max(2, Mathf.RoundToInt(length / brick));
            float step = length / columns;
            int rows = Mathf.FloorToInt(height / course);
            b.With(a, yaw, () =>
            {
                // Solid bottom course, then the chequer, then a solid top course.
                b.Box(new Vector3(0f, course * 0.5f, length * 0.5f), new Vector3(0.16f, course, length), TownSwatch.Brick);
                for (int row = 1; row < rows - 1; row++)
                {
                    for (int col = (row & 1); col < columns; col += 2)
                    {
                        b.Box(new Vector3(0f, (row + 0.5f) * course, (col + 0.5f) * step), new Vector3(0.16f, course - 0.02f, step), TownSwatch.Brick);
                    }
                }

                b.Box(new Vector3(0f, height - course * 0.5f, length * 0.5f), new Vector3(0.16f, course, length), TownSwatch.Brick);
            });
        }

        /// <summary>
        /// The caravanserai arcade (Qocho): brick piers at <see cref="TownBuildingBuilder.PorchPostsX"/> (the same spots as
        /// the post colliders) carrying stepped pointed arches and a deep lintel band, with a beam behind.
        /// </summary>
        public static void Arcade(TownMeshBuilder b, TownBuildingSpec spec, float z, float y0, float y1)
        {
            List<float> xs = TownBuildingBuilder.PorchPostsX(spec);
            xs.Sort();
            const float pier = 0.36f;
            float deck = CitySolids.DeckHeight;
            foreach (float x in xs)
            {
                b.Box(new Vector3(x, deck + 0.1f, z), new Vector3(pier + 0.1f, 0.2f, pier + 0.1f), TownSwatch.Sandstone);
                b.Box(new Vector3(x, (deck + y1) * 0.5f, z), new Vector3(pier, y1 - deck, pier), spec.WallColor);
            }

            float spring = y1 - 1.05f;
            for (int i = 0; i + 1 < xs.Count; i++)
            {
                float xa = xs[i] + pier * 0.5f;
                float xb = xs[i + 1] - pier * 0.5f;
                float span = xb - xa;
                if (span < 0.4f)
                {
                    continue;
                }

                // Three corbelled steps each side close the opening into a pointed arch.
                for (int k = 0; k < 3; k++)
                {
                    float ya = spring + k * 0.25f;
                    float reach = span * (0.1f + 0.13f * k);
                    b.BoxMinMax(new Vector3(xa, ya, z - pier * 0.5f), new Vector3(xa + reach, ya + 0.25f, z + pier * 0.5f), spec.WallColor);
                    b.BoxMinMax(new Vector3(xb - reach, ya, z - pier * 0.5f), new Vector3(xb, ya + 0.25f, z + pier * 0.5f), spec.WallColor);
                }

                // Painted arch outline (detail only).
                b.BoxMinMax(new Vector3(xa + span * 0.42f, spring + 0.75f, z - pier * 0.5f - 0.02f), new Vector3(xb - span * 0.42f, spring + 0.8f, z - pier * 0.5f), spec.AccentColor);
            }

            float x0 = xs.Count > 0 ? xs[0] - pier * 0.5f : -spec.Width * 0.5f;
            float x1 = xs.Count > 0 ? xs[xs.Count - 1] + pier * 0.5f : spec.Width * 0.5f;
            b.BoxMinMax(new Vector3(x0, spring + 0.75f, z - pier * 0.5f), new Vector3(x1, y1 + 0.12f, z + pier * 0.5f), spec.WallColor);
            b.BoxMinMax(new Vector3(x0 - 0.04f, y1 - 0.1f, z - pier * 0.5f - 0.04f), new Vector3(x1 + 0.04f, y1 + 0.02f, z + pier * 0.5f), spec.TrimColor);
        }

        // ------------------------------------------------------------------
        // Litema: rondavel details
        // ------------------------------------------------------------------

        /// <summary>
        /// Painted litema bands on a round wall: a zig-zag of triangles in two colours and a row of diamonds above it
        /// (pattern in colour, not geometry: one triangle per wall face).
        /// </summary>
        public static void LitemaBands(TownMeshBuilder b, Vector3 center, float radius, float y, int sides, TownSwatch a, TownSwatch c)
        {
            float r = radius + 0.02f;
            for (int i = 0; i < sides; i++)
            {
                float a0 = i * Mathf.PI * 2f / sides;
                float a1 = (i + 1) * Mathf.PI * 2f / sides;
                float am = (a0 + a1) * 0.5f;
                var p0 = center + new Vector3(Mathf.Cos(a0) * r, 0f, Mathf.Sin(a0) * r);
                var p1 = center + new Vector3(Mathf.Cos(a1) * r, 0f, Mathf.Sin(a1) * r);
                var pm = center + new Vector3(Mathf.Cos(am) * r * Mathf.Cos(Mathf.PI / sides), 0f, Mathf.Sin(am) * r * Mathf.Cos(Mathf.PI / sides));
                var facing = new Vector3(Mathf.Cos(am), 0f, Mathf.Sin(am));
                bool up = (i & 1) == 0;
                // Zig-zag: triangles pointing up and down in turn.
                b.TriangleFacing(p0 + Vector3.up * (up ? y : y + 0.45f), pm + Vector3.up * (up ? y + 0.45f : y), p1 + Vector3.up * (up ? y : y + 0.45f), facing, up ? a : c);
                // A diamond above (two triangles), every other face.
                if (up)
                {
                    float dy = y + 0.75f;
                    Vector3 l = Vector3.Lerp(p0, p1, 0.25f) + Vector3.up * (dy + 0.14f);
                    Vector3 rr = Vector3.Lerp(p0, p1, 0.75f) + Vector3.up * (dy + 0.14f);
                    b.TriangleFacing(l, pm + Vector3.up * (dy + 0.3f), rr, facing, c);
                    b.TriangleFacing(l, rr, pm + Vector3.up * dy, facing, c);
                }
            }
        }

        /// <summary>The thatch shade in front of a rondavel: posts at the porch-post spots (the colliders) and a sloping thatch roof back to the wall.</summary>
        public static void RondavelShade(TownMeshBuilder b, TownBuildingSpec spec, float zFront, float zWall, TownSwatch thatch)
        {
            List<float> xs = TownBuildingBuilder.PorchPostsX(spec);
            if (xs.Count == 0)
            {
                return;
            }

            float z = zFront + TownBuildingBuilder.PostInset;
            const float postTop = 2.1f;
            foreach (float x in xs)
            {
                b.Log(new Vector3(x, 0f, z), new Vector3(x, postTop, z), 0.09f, TownSwatch.WoodDark);
            }

            float x0 = xs[0] - 0.25f;
            float x1 = xs[xs.Count - 1] + 0.25f;
            b.Log(new Vector3(x0, postTop, z), new Vector3(x1, postTop, z), 0.07f, TownSwatch.WoodDark, 4);
            b.Plate(new[]
            {
                new Vector3(x0, postTop - 0.05f, zFront - 0.15f), new Vector3(x0, postTop + 0.45f, zWall),
                new Vector3(x1, postTop + 0.45f, zWall), new Vector3(x1, postTop - 0.05f, zFront - 0.15f),
            }, 0.12f, thatch, TownSwatch.Beam);
        }

        // ------------------------------------------------------------------
        // Qocho props
        // ------------------------------------------------------------------

        /// <summary>A karez well shaft: a ring of spoil around the dark shaft, a timber frame with a windlass, rope and bucket.</summary>
        private static void KarezShaft(TownMeshBuilder b, float s)
        {
            b.Cylinder(Vector3.zero, 0.95f * s, 0.6f * s, 0.32f * s, 9, TownSwatch.SandDark, TownSwatch.Sand);
            b.Cylinder(new Vector3(0f, 0.32f * s, 0f), 0.42f * s, 0.42f * s, 0.06f, 8, TownSwatch.Sandstone, TownSwatch.Sandstone);
            b.Disc(new Vector3(0f, 0.385f * s, 0f), 0.34f * s, 8, TownSwatch.Black);
            foreach (float x in new[] { -0.5f, 0.5f })
            {
                b.Log(new Vector3(x * s, 0.3f * s, 0f), new Vector3(x * s * 0.8f, 1.35f, 0f), 0.05f, TownSwatch.WoodWeathered, 4);
            }

            b.Log(new Vector3(-0.5f * s, 1.3f, 0f), new Vector3(0.5f * s, 1.3f, 0f), 0.08f, TownSwatch.Walnut, 6, TownSwatch.WoodLight);
            b.Log(new Vector3(0f, 1.27f, 0f), new Vector3(0f, 0.75f, 0f), 0.012f, TownSwatch.Thatch, 4);
            b.Cylinder(new Vector3(0f, 0.55f, 0f), 0.11f, 0.13f, 0.2f, 6, TownSwatch.Copper, TownSwatch.Black);
        }

        /// <summary>The open karez channel (stone curbs, running water, a few cover slabs) from the origin to <paramref name="d"/>.</summary>
        private static void KarezChannel(TownMeshBuilder b, Vector2 d)
        {
            float length = d.magnitude;
            if (length < 0.05f)
            {
                return;
            }

            float yaw = Mathf.Atan2(d.x, d.y) * Mathf.Rad2Deg;
            b.With(new Vector3(d.x * 0.5f, 0f, d.y * 0.5f), yaw, () =>
            {
                b.Box(new Vector3(-0.38f, 0.06f, 0f), new Vector3(0.18f, 0.12f, length + 0.18f), TownSwatch.Sandstone);
                b.Box(new Vector3(0.38f, 0.06f, 0f), new Vector3(0.18f, 0.12f, length + 0.18f), TownSwatch.Sandstone);
                b.Box(new Vector3(0f, 0.025f, 0f), new Vector3(0.58f, 0.05f, length), TownSwatch.Water);
                for (float z = -length * 0.5f + 1.2f; z < length * 0.5f - 0.6f; z += 3.1f)
                {
                    b.Box(new Vector3(0f, 0.13f, z), new Vector3(0.95f, 0.06f, 0.55f), TownSwatch.Stone);
                }
            });
        }

        /// <summary>Yardena's cistern: a low round wall under a mud dome with a lime ring and a lamp, a stepped arched way down, a spout and trough.</summary>
        private static void CisternDome(TownMeshBuilder b, float s)
        {
            float r = 1.75f * s;
            b.Cylinder(Vector3.zero, r + 0.15f, r + 0.1f, 0.25f, 12, TownSwatch.Sandstone, TownSwatch.Sandstone);
            b.Cylinder(new Vector3(0f, 0.25f, 0f), r, r - 0.05f, 0.85f * s, 12, TownSwatch.TaipaRose);
            b.Cylinder(new Vector3(0f, 0.25f + 0.85f * s, 0f), r + 0.05f, r + 0.05f, 0.1f, 12, TownSwatch.LimeWhite, TownSwatch.LimeWhite);
            float y = 0.35f + 0.85f * s;
            float previousR = r;
            float previousY = 0f;
            for (int i = 1; i <= 4; i++)
            {
                float a = i / 4f * Mathf.PI * 0.5f;
                float rr = Mathf.Max(0.001f, Mathf.Cos(a) * r);
                float h = Mathf.Sin(a) * r * 0.95f;
                b.Cylinder(new Vector3(0f, y + previousY, 0f), previousR, rr, h - previousY, 12, TownSwatch.Mud, i == 4 ? (TownSwatch?)TownSwatch.Mud : null);
                previousR = rr;
                previousY = h;
            }

            Lantern(b, new Vector3(0f, y + r * 0.95f - 0.05f, 0f), 0.8f);
            // The entrance on the front: a small pointed portal with steps going down into the dark.
            b.With(new Vector3(0f, 0f, -r + 0.15f), 0f, () =>
            {
                b.BoxMinMax(new Vector3(-0.75f, 0f, -0.45f), new Vector3(0.75f, 1.9f, 0.1f), TownSwatch.TaipaRose);
                b.BoxMinMax(new Vector3(-0.45f, 0.05f, -0.47f), new Vector3(0.45f, 1.35f, -0.45f), TownSwatch.Black);
                ArchFill(b, 0f, 1.35f, 0.9f, 0.35f, -0.47f, TownSwatch.Black);
                b.BoxMinMax(new Vector3(-0.85f, 1.9f, -0.5f), new Vector3(0.85f, 2.0f, 0.12f), TownSwatch.LimeWhite);
                b.BoxMinMax(new Vector3(-0.5f, 0f, -0.9f), new Vector3(0.5f, 0.06f, -0.45f), TownSwatch.Sandstone);
            });

            // Spout and trough on the side.
            b.Log(new Vector3(r - 0.1f, 0.75f, 0.3f), new Vector3(r + 0.35f, 0.7f, 0.3f), 0.06f, TownSwatch.Copper, 6);
            b.Box(new Vector3(r + 0.35f, 0.2f, 0.3f), new Vector3(0.5f, 0.4f, 0.9f), TownSwatch.Sandstone, 0f, TownSwatch.Water);
        }

        /// <summary>The Ruínas dos Artífices: 0 a steep Meroë pyramid with its pylon chapel, 1 broken lotus columns, 2 a row of ushabtis.</summary>
        private static void Ruin(TownMeshBuilder b, int variant, float s)
        {
            const float root2 = 1.4142f;
            switch (variant % 3)
            {
                case 0:
                {
                    // Plinth, steep pyramid (about 70 degrees), a gold cap, a fallen block; the chapel with its pylon in front.
                    b.BoxMinMax(new Vector3(-1.4f, 0f, -0.9f) * s, new Vector3(1.4f, 0.25f, 1.6f) * s, TownSwatch.Sandstone);
                    var foot = new Vector3(0f, 0.25f * s, 0.35f * s);
                    b.Cylinder(foot, 1.2f * s * root2, 0.1f * s * root2, 3.6f * s, 4, TownSwatch.Sandstone, TownSwatch.Gold, 0.5f);
                    foreach (float t in new[] { 0.25f, 0.5f, 0.75f })
                    {
                        float half = Mathf.Lerp(1.2f, 0.1f, t) * s + 0.02f;
                        b.BoxMinMax(new Vector3(-half, foot.y + 3.6f * s * t, foot.z - half), new Vector3(half, foot.y + 3.6f * s * t + 0.05f, foot.z + half), TownSwatch.RedClay);
                    }

                    b.BoxMinMax(new Vector3(-0.55f, 0.25f, -1.6f) * s, new Vector3(0.55f, 1.3f, -0.85f) * s, TownSwatch.Sandstone);
                    b.Cylinder(new Vector3(0f, 0.25f * s, -1.25f * s), 0.95f * s * root2, 0.7f * s * root2, 1.35f * s, 4, TownSwatch.Sandstone, TownSwatch.Sandstone, 0.5f);
                    b.BoxMinMax(new Vector3(-0.22f, 0.25f, -1.97f) * s, new Vector3(0.22f, 1.0f, -1.95f) * s, TownSwatch.Black);
                    b.BoxMinMax(new Vector3(-0.6f, 1.3f, -1.99f) * s, new Vector3(0.6f, 1.45f, -1.9f) * s, TownSwatch.Gold);
                    b.Box(new Vector3(1.25f, 0.2f, -1.4f) * s, new Vector3(0.6f, 0.4f, 0.45f) * s, TownSwatch.Sandstone, 25f);
                    break;
                }

                case 1:
                {
                    // One standing lotus column (flared capital), one broken stump, a fallen drum.
                    b.Cylinder(Vector3.zero, 0.42f * s, 0.4f * s, 0.2f * s, 8, TownSwatch.Sandstone, TownSwatch.Sandstone);
                    b.Cylinder(new Vector3(0f, 0.2f * s, 0f), 0.28f * s, 0.25f * s, 2.3f * s, 8, TownSwatch.Sandstone);
                    b.Cylinder(new Vector3(0f, 2.5f * s, 0f), 0.25f * s, 0.48f * s, 0.45f * s, 8, TownSwatch.Gold, TownSwatch.Sandstone);
                    b.Box(new Vector3(0f, 3.02f * s, 0f), new Vector3(0.75f, 0.14f, 0.75f) * s, TownSwatch.Sandstone);
                    b.Cylinder(new Vector3(0.95f, 0f, 0.3f) * s, 0.3f * s, 0.27f * s, 0.8f * s, 8, TownSwatch.Sandstone, TownSwatch.SandDark);
                    b.Log(new Vector3(-0.5f, 0.27f, 0.6f) * s, new Vector3(-1.3f, 0.27f, 0.9f) * s, 0.27f * s, TownSwatch.Sandstone, 8);
                    break;
                }

                default:
                {
                    // A row of four mummiform ushabtis on a low plinth (one fallen), arms crossed, striped headdress.
                    b.BoxMinMax(new Vector3(-1.5f, 0f, -0.3f) * s, new Vector3(1.5f, 0.2f, 0.3f) * s, TownSwatch.Sandstone);
                    for (int i = 0; i < 4; i++)
                    {
                        float x = (-1.1f + i * 0.73f) * s;
                        if (i == 2)
                        {
                            b.Log(new Vector3(x, 0.32f * s, 0.05f), new Vector3(x + 0.2f * s, 0.3f * s, -1.05f * s), 0.2f * s, TownSwatch.Sandstone, 6);
                            continue;
                        }

                        var f = new Vector3(x, 0.2f * s, 0f);
                        b.Cylinder(f, 0.17f * s, 0.24f * s, 1.05f * s, 6, TownSwatch.Sandstone);
                        b.Box(f + new Vector3(0f, 0.85f, -0.2f) * s, new Vector3(0.36f, 0.1f, 0.06f) * s, TownSwatch.Turquoise);
                        b.Crown(f + new Vector3(0f, 1.25f, 0f) * s, new Vector3(0.36f, 0.42f, 0.34f) * s, TownSwatch.Sandstone);
                        b.Cylinder(f + new Vector3(0f, 1.15f, 0.04f) * s, 0.22f * s, 0.15f * s, 0.32f * s, 6, TownSwatch.Indigo, TownSwatch.Gold);
                    }

                    break;
                }
            }
        }

        /// <summary>A Bactrian pack camel lying down (two humps, folded legs), felt bales on its sides and a tasselled halter.</summary>
        private static void Camel(TownMeshBuilder b, float s)
        {
            b.With(Vector3.zero, 0f, new Vector3(s, s, s), () =>
            {
                b.Crown(new Vector3(0f, 0.55f, 0.1f), new Vector3(0.85f, 0.75f, 1.6f), TownSwatch.AdobeOchre);
                b.Crown(new Vector3(0f, 1.0f, -0.25f), new Vector3(0.45f, 0.45f, 0.42f), TownSwatch.SandDark);
                b.Crown(new Vector3(0f, 1.0f, 0.4f), new Vector3(0.45f, 0.45f, 0.42f), TownSwatch.SandDark);
                b.Log(new Vector3(0f, 0.6f, -0.65f), new Vector3(0f, 1.25f, -0.95f), 0.13f, TownSwatch.AdobeOchre);
                b.Crown(new Vector3(0f, 1.3f, -1.05f), new Vector3(0.24f, 0.24f, 0.42f), TownSwatch.AdobeOchre);
                b.Box(new Vector3(0f, 1.3f, -1.2f), new Vector3(0.26f, 0.06f, 0.08f), TownSwatch.ClothRed);
                foreach (float x in new[] { -0.38f, 0.38f })
                {
                    b.Box(new Vector3(x, 0.12f, -0.4f), new Vector3(0.18f, 0.24f, 0.5f), TownSwatch.SandDark);
                    b.Box(new Vector3(x, 0.12f, 0.6f), new Vector3(0.18f, 0.24f, 0.5f), TownSwatch.SandDark);
                    // Felt bales hung on both sides, between the humps.
                    b.Box(new Vector3(x * 1.45f, 0.75f, 0.08f), new Vector3(0.28f, 0.5f, 0.55f), x < 0f ? TownSwatch.ClothRed : TownSwatch.ClothIndigo, 0f, TownSwatch.ClothYellow);
                }

                b.Log(new Vector3(-0.55f, 1.08f, 0.08f), new Vector3(0.55f, 1.08f, 0.08f), 0.04f, TownSwatch.Thatch, 4);
            });
        }

        // ------------------------------------------------------------------
        // Litema props
        // ------------------------------------------------------------------

        /// <summary>A San rock art panel: a granite outcrop with a flat ochre face painted with elands, dancers and the mantis.</summary>
        private static void RockArt(TownMeshBuilder b, float s)
        {
            b.With(Vector3.zero, 0f, new Vector3(s, s, s), () =>
            {
                b.Crown(new Vector3(0f, 0.9f, 0.25f), new Vector3(3.0f, 2.6f, 1.5f), TownSwatch.Stone, 8f);
                b.Crown(new Vector3(0.9f, 1.9f, 0.4f), new Vector3(1.6f, 1.4f, 1.1f), TownSwatch.StoneDark, 40f);
                b.BoxMinMax(new Vector3(-1.15f, 0.35f, -0.52f), new Vector3(1.1f, 1.75f, -0.4f), TownSwatch.Sandstone);
                // Elands (body, neck and head, four legs) in red ochre and white.
                for (int i = 0; i < 2; i++)
                {
                    float x = -0.65f + i * 0.85f;
                    float y = 1.05f + i * 0.18f;
                    TownSwatch c = i == 0 ? TownSwatch.RedClay : TownSwatch.LimeWhite;
                    b.BoxMinMax(new Vector3(x - 0.24f, y, -0.54f), new Vector3(x + 0.24f, y + 0.16f, -0.52f), c);
                    b.BoxMinMax(new Vector3(x + 0.2f, y + 0.12f, -0.54f), new Vector3(x + 0.28f, y + 0.32f, -0.52f), c);
                    b.BoxMinMax(new Vector3(x + 0.24f, y + 0.26f, -0.545f), new Vector3(x + 0.38f, y + 0.33f, -0.525f), c);
                    foreach (float lx in new[] { -0.2f, -0.12f, 0.12f, 0.2f })
                    {
                        b.BoxMinMax(new Vector3(x + lx - 0.015f, y - 0.2f, -0.54f), new Vector3(x + lx + 0.015f, y, -0.52f), c);
                    }
                }

                // Dancers: stick figures in a line.
                for (int i = 0; i < 4; i++)
                {
                    float x = -0.95f + i * 0.22f;
                    b.BoxMinMax(new Vector3(x - 0.02f, 0.5f, -0.54f), new Vector3(x + 0.02f, 0.78f, -0.52f), TownSwatch.Black);
                    b.BoxMinMax(new Vector3(x - 0.04f, 0.79f, -0.54f), new Vector3(x + 0.04f, 0.86f, -0.52f), TownSwatch.Black);
                    b.BoxMinMax(new Vector3(x - 0.08f, 0.68f, -0.545f), new Vector3(x + 0.08f, 0.7f, -0.525f), TownSwatch.Black);
                }

                // The mantis (|Kaggen) in green, praying arms raised.
                b.BoxMinMax(new Vector3(0.55f, 0.55f, -0.54f), new Vector3(0.6f, 0.9f, -0.52f), TownSwatch.Vine);
                b.BoxMinMax(new Vector3(0.48f, 0.85f, -0.545f), new Vector3(0.67f, 0.88f, -0.525f), TownSwatch.Vine);
                b.BoxMinMax(new Vector3(0.53f, 0.9f, -0.54f), new Vector3(0.62f, 0.98f, -0.52f), TownSwatch.Vine);
            });
        }

        /// <summary>A Great Zimbabwe conical tower: tapering courses of dry stone, a chevron frieze near the top, a flat crown.</summary>
        private static void ConicalTower(TownMeshBuilder b, float s)
        {
            const int sides = 12;
            float r0 = 1.5f * s;
            float r1 = 0.85f * s;
            float h = 5.0f * s;
            b.Cylinder(Vector3.zero, r0 + 0.1f, r0 + 0.05f, 0.25f, sides, TownSwatch.StoneDark, TownSwatch.StoneDark);
            b.Cylinder(new Vector3(0f, 0.25f, 0f), r0, r1, h, sides, TownSwatch.Stone, TownSwatch.Stone);
            for (int k = 1; k < 5; k++)
            {
                float t = k / 5f;
                float rr = Mathf.Lerp(r0, r1, t) + 0.015f;
                b.Cylinder(new Vector3(0f, 0.25f + h * t, 0f), rr, rr, 0.05f, sides, TownSwatch.StoneDark);
            }

            // Chevron frieze: little tilted blocks around the top, alternating left and right.
            float fy = 0.25f + h * 0.86f;
            float fr = Mathf.Lerp(r0, r1, 0.86f) + 0.03f;
            for (int i = 0; i < sides; i++)
            {
                float a = (i + 0.5f) * Mathf.PI * 2f / sides;
                var p = new Vector3(Mathf.Cos(a) * fr, fy, Mathf.Sin(a) * fr);
                float yaw = -a * Mathf.Rad2Deg + 90f;
                b.With(p, yaw, () =>
                {
                    b.Matrix *= Matrix4x4.Rotate(Quaternion.Euler(0f, 0f, (i & 1) == 0 ? 35f : -35f));
                    b.Box(Vector3.zero, new Vector3(0.34f, 0.08f, 0.05f), TownSwatch.LimeWhite);
                });
            }
        }

        /// <summary>Aloe rosette: a ring of fleshy pointed leaves and two red flower candles.</summary>
        private static void AloeBush(TownMeshBuilder b, float s)
        {
            for (int i = 0; i < 8; i++)
            {
                float a = i / 8f * Mathf.PI * 2f;
                var dir = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a));
                var side = new Vector3(-dir.z, 0f, dir.x) * 0.12f * s;
                Vector3 tip = dir * 0.75f * s + Vector3.up * (0.55f + (i & 1) * 0.15f) * s;
                b.TwoSidedQuad(side, tip * 0.5f + Vector3.up * 0.1f * s + side, tip, tip * 0.5f + Vector3.up * 0.1f * s - side, i % 3 == 0 ? TownSwatch.Pine : TownSwatch.Vine);
            }

            foreach (float x in new[] { -0.12f, 0.15f })
            {
                b.Log(new Vector3(x, 0.2f, 0f) * s, new Vector3(x * 1.6f, 1.25f, 0.05f) * s, 0.025f * s, TownSwatch.WoodDark, 4);
                b.Log(new Vector3(x * 1.6f, 1.0f, 0.05f) * s, new Vector3(x * 1.7f, 1.45f, 0.05f) * s, 0.07f * s, TownSwatch.Aloe, 5);
            }
        }

        /// <summary>The baobab: a fat grey bottle trunk and short stubby branches with small crowns.</summary>
        private static void Baobab(TownMeshBuilder b, float s)
        {
            b.Cylinder(Vector3.zero, 0.95f * s, 0.85f * s, 1.2f * s, 8, TownSwatch.WoodWeathered);
            b.Cylinder(new Vector3(0f, 1.2f * s, 0f), 0.85f * s, 0.6f * s, 2.0f * s, 8, TownSwatch.WoodWeathered, TownSwatch.Trunk);
            Vector3 top = new Vector3(0f, 3.1f * s, 0f);
            for (int i = 0; i < 6; i++)
            {
                float a = i / 6f * Mathf.PI * 2f + 0.3f;
                Vector3 tip = top + new Vector3(Mathf.Cos(a) * 1.5f, 0.7f + (i & 1) * 0.35f, Mathf.Sin(a) * 1.5f) * s;
                b.Log(top, tip, 0.16f * s, TownSwatch.WoodWeathered, 5);
                b.Crown(tip + Vector3.up * 0.15f * s, new Vector3(1.1f, 0.5f, 1.1f) * s, i % 3 == 0 ? TownSwatch.Poplar : TownSwatch.LeafDark, i * 30f);
            }
        }

        /// <summary>The desert poplar (Populus euphratica) of the oases: a leaning gnarled trunk and a wide, layered crown.</summary>
        private static void DesertPoplar(TownMeshBuilder b, float s)
        {
            b.Log(Vector3.zero, new Vector3(0.35f, 1.8f, 0.1f) * s, 0.2f * s, TownSwatch.Trunk);
            b.Log(new Vector3(0.35f, 1.8f, 0.1f) * s, new Vector3(-0.3f, 2.9f, 0.2f) * s, 0.13f * s, TownSwatch.Trunk);
            b.Log(new Vector3(0.35f, 1.8f, 0.1f) * s, new Vector3(1.1f, 2.6f, -0.3f) * s, 0.1f * s, TownSwatch.Trunk);
            b.Crown(new Vector3(0f, 3.2f, 0.1f) * s, new Vector3(2.8f, 1.3f, 2.5f) * s, TownSwatch.Poplar, 12f);
            b.Crown(new Vector3(1.1f, 2.8f, -0.3f) * s, new Vector3(1.6f, 0.9f, 1.5f) * s, TownSwatch.LeafGold, 50f);
            b.Crown(new Vector3(-0.6f, 3.8f, 0.3f) * s, new Vector3(1.5f, 0.8f, 1.4f) * s, TownSwatch.Vine, 80f);
        }

        // ------------------------------------------------------------------
        // Lamparinas props
        // ------------------------------------------------------------------

        /// <summary>
        /// The pink tuff cliff (local x along <paramref name="d"/>, the mass behind at +z, the face at z = 0): stepped rock
        /// slabs with weathered crowns, a ledge path at the second level with carved stairs up to it, cave houses cut into
        /// the ledge (arched door, round window, lamp niche) and windows and lamp niches higher up. Variant 1 = a bare spire
        /// (no houses). Height scales with <paramref name="s"/>.
        /// </summary>
        private static void Cliff(TownMeshBuilder b, Vector2 d, float s, int variant)
        {
            float length = d.magnitude;
            if (length < 0.5f)
            {
                return;
            }

            float yaw = YawAlong(d / length);
            float height = CliffHeight * s;
            b.With(Vector3.zero, yaw, () =>
            {
                string key = "cliff" + Mathf.RoundToInt(length * 10f) + "_" + Mathf.RoundToInt(height * 10f);
                int slabs = Mathf.Max(1, Mathf.RoundToInt(length / 3.2f));
                float w = length / slabs;
                for (int i = 0; i < slabs; i++)
                {
                    float x0 = i * w;
                    float x1 = x0 + w;
                    float h = height * (0.82f + TownStyle.Hash01(key, i) * 0.22f);
                    float inset = TownStyle.Hash01(key, 50 + i) * 0.35f;
                    TownSwatch tuff = TownStyle.Hash01(key, 90 + i) < 0.3f ? TownSwatch.TaipaRose : TownSwatch.PinkTuff;
                    b.BoxMinMax(new Vector3(x0 - 0.05f, 0f, inset), new Vector3(x1 + 0.05f, h, CliffDepth), tuff);
                    b.Crown(new Vector3((x0 + x1) * 0.5f, h, CliffDepth * 0.45f + inset), new Vector3(w * 1.25f, 1.8f * s, CliffDepth * 0.9f), TownSwatch.PinkTuff, i * 17f);
                    // A dark streak of basalt at the foot (the layer under the tuff).
                    b.BoxMinMax(new Vector3(x0, 0f, inset - 0.03f), new Vector3(x1, 0.45f, inset + 0.2f), TownSwatch.Basalt);
                }

                if (variant == 1 || height < 5f)
                {
                    return;
                }

                // Ledge path at the second level, with a low parapet.
                const float ledgeY = 3.3f;
                b.BoxMinMax(new Vector3(0f, ledgeY - 0.2f, -0.9f), new Vector3(length, ledgeY, 0.2f), TownSwatch.PinkTuff, TownSwatch.StoneDark);
                b.BoxMinMax(new Vector3(0f, ledgeY, -0.92f), new Vector3(length, ledgeY + 0.35f, -0.78f), TownSwatch.Basalt);
                // Corbels under the ledge.
                for (float x = 0.6f; x < length - 0.3f; x += 1.8f)
                {
                    b.BoxMinMax(new Vector3(x - 0.12f, ledgeY - 0.65f, -0.7f), new Vector3(x + 0.12f, ledgeY - 0.2f, 0f), TownSwatch.Basalt);
                }

                // Cave houses on the ledge, every ~5.5 m.
                int houses = Mathf.Max(1, Mathf.FloorToInt((length - 1.5f) / 5.5f));
                float spacing = length / houses;
                for (int i = 0; i < houses; i++)
                {
                    float x = (i + 0.5f) * spacing;
                    bool lit = TownStyle.Hash01(key, 200 + i) < 0.7f;
                    b.BoxMinMax(new Vector3(x - 1.3f, ledgeY, -0.12f), new Vector3(x + 1.3f, ledgeY + 2.6f, 0.05f), TownSwatch.PinkTuff);
                    b.BoxMinMax(new Vector3(x - 1.4f, ledgeY + 2.6f, -0.2f), new Vector3(x + 1.4f, ledgeY + 2.72f, 0.05f), TownSwatch.Basalt);
                    b.With(new Vector3(0f, 0f, -0.12f), 0f, () =>
                    {
                        Door(b, x - 0.35f, ledgeY, 0.9f, 1.8f, TownSwatch.WoodDark, TownSwatch.Basalt, TownSwatch.Copper, arched: true);
                        if (lit)
                        {
                            RoundWindow(b, x + 0.75f, ledgeY + 1.55f, 0.3f, TownSwatch.Basalt);
                        }

                        LampNiche(b, x + 0.75f, ledgeY + 0.5f);
                    });

                    // A lamp hanging over each door.
                    Lantern(b, new Vector3(x - 0.35f, ledgeY + 2.05f, -0.45f), 0.6f);
                }

                // Stairs carved up to the ledge, at the start and every ~12 m.
                for (float sx = 0.4f; sx < length - 3.5f; sx += 12f)
                {
                    for (int k = 0; k < 8; k++)
                    {
                        float x = sx + k * 0.38f;
                        b.BoxMinMax(new Vector3(x, 0f, -0.9f), new Vector3(x + 0.4f, (k + 1) * ledgeY / 8f, -0.05f), TownSwatch.PinkTuff, TownSwatch.StoneDark);
                    }
                }

                // Higher up: round windows and lamp niches cut into the face.
                if (height > 7f)
                {
                    b.With(new Vector3(0f, 0f, 0f), 0f, () =>
                    {
                        for (float x = 2.2f; x < length - 1f; x += 4.3f)
                        {
                            if (TownStyle.Hash01(key, 300 + Mathf.RoundToInt(x)) < 0.6f)
                            {
                                RoundWindow(b, x, 6.6f, 0.26f, TownSwatch.Basalt);
                            }
                            else
                            {
                                LampNiche(b, x, 6.3f);
                            }
                        }
                    });
                }
            });
        }

        /// <summary>A rope bridge from the origin to <paramref name="d"/> at <see cref="RopeBridgeHeight"/> × <paramref name="s"/>: two sagging foot ropes, planks, hand ropes, a post pair at each end.</summary>
        private static void RopeBridge(TownMeshBuilder b, Vector2 d, float s)
        {
            float length = d.magnitude;
            if (length < 0.5f)
            {
                return;
            }

            float h = RopeBridgeHeight * s;
            float sag = Mathf.Min(1.1f, length * 0.06f);
            float yaw = YawAlong(d / length);
            b.With(Vector3.zero, yaw, () =>
            {
                int segments = Mathf.Max(4, Mathf.RoundToInt(length / 1.1f));
                for (int side = -1; side <= 1; side += 2)
                {
                    float z = side * 0.42f;
                    Vector3 previous = Vector3.zero;
                    Vector3 previousHand = Vector3.zero;
                    for (int i = 0; i <= segments; i++)
                    {
                        float t = i / (float)segments;
                        float y = h - sag * 4f * t * (1f - t);
                        var p = new Vector3(t * length, y, z);
                        var hand = new Vector3(t * length, y + 0.9f - sag * 0.15f * 4f * t * (1f - t), z);
                        if (i > 0)
                        {
                            b.Log(previous, p, 0.03f, TownSwatch.Thatch, 4);
                            b.Log(previousHand, hand, 0.025f, TownSwatch.Thatch, 4);
                        }

                        if (i % 2 == 0)
                        {
                            b.Log(p, hand, 0.012f, TownSwatch.Thatch, 3);
                        }

                        previous = p;
                        previousHand = hand;
                    }

                    // End posts.
                    foreach (float x in new[] { 0f, length })
                    {
                        b.Log(new Vector3(x, h - 0.3f, z), new Vector3(x, h + 1.15f, z), 0.07f, TownSwatch.WoodDark);
                    }
                }

                int planks = Mathf.Max(4, Mathf.RoundToInt(length / 0.42f));
                for (int i = 0; i < planks; i++)
                {
                    float t = (i + 0.5f) / planks;
                    float y = h - sag * 4f * t * (1f - t);
                    b.Box(new Vector3(t * length, y + 0.02f, 0f), new Vector3(0.24f, 0.05f, 0.9f), (i & 1) == 0 ? TownSwatch.WoodWeathered : TownSwatch.WoodLight);
                }

                Lantern(b, new Vector3(0.1f, h + 0.55f, 0.42f), 0.55f);
                Lantern(b, new Vector3(length - 0.1f, h + 0.55f, -0.42f), 0.55f);
            });
        }

        /// <summary>A string of lamps from the origin to <paramref name="d"/>: a sagging cord at <see cref="LampStringHeight"/> with a small lamp every ~1.3 m.</summary>
        private static void LampString(TownMeshBuilder b, Vector2 d, float s)
        {
            float length = d.magnitude;
            if (length < 0.5f)
            {
                return;
            }

            float h = LampStringHeight * s;
            float sag = Mathf.Min(0.7f, length * 0.05f);
            float yaw = YawAlong(d / length);
            b.With(Vector3.zero, yaw, () =>
            {
                int lamps = Mathf.Max(2, Mathf.RoundToInt(length / 1.3f));
                Vector3 previous = new Vector3(0f, h, 0f);
                for (int i = 1; i <= lamps; i++)
                {
                    float t = i / (float)lamps;
                    var p = new Vector3(t * length, h - sag * 4f * t * (1f - t), 0f);
                    b.Log(previous, p, 0.012f, TownSwatch.Iron, 3);
                    previous = p;
                    if (i < lamps)
                    {
                        // Little lamp: copper cap, amber glass, copper base.
                        b.Log(p, p + Vector3.down * 0.18f, 0.008f, TownSwatch.Iron, 3);
                        b.Cylinder(p + Vector3.down * 0.2f, 0.06f, 0.02f, 0.06f, 5, TownSwatch.Copper);
                        b.Box(p + Vector3.down * 0.31f, new Vector3(0.13f, 0.17f, 0.13f), TownSwatch.LampGlow, 45f);
                        b.Box(p + Vector3.down * 0.41f, new Vector3(0.12f, 0.03f, 0.12f), TownSwatch.Copper, 45f);
                    }
                }
            });
        }

        /// <summary>The vishapakar: a tall dressed stone on a base, a fish head carved at the top, scale lines and a fin.</summary>
        private static void Vishapakar(TownMeshBuilder b, float s)
        {
            b.With(Vector3.zero, 0f, new Vector3(s, s, s), () =>
            {
                b.Box(new Vector3(0f, 0.15f, 0f), new Vector3(1.0f, 0.3f, 0.8f), TownSwatch.Basalt);
                b.Cylinder(new Vector3(0f, 0.3f, 0f), 0.42f, 0.3f, 2.3f, 4, TownSwatch.Stone, null, 0.5f);
                // The head: a wedge with the eye, mouth and gill.
                b.Crown(new Vector3(0f, 2.75f, -0.05f), new Vector3(0.62f, 0.8f, 0.55f), TownSwatch.Stone);
                b.Box(new Vector3(0f, 3.05f, -0.24f), new Vector3(0.34f, 0.05f, 0.05f), TownSwatch.StoneDark);
                b.Box(new Vector3(0.18f, 2.85f, -0.22f), new Vector3(0.09f, 0.09f, 0.04f), TownSwatch.Basalt);
                b.Box(new Vector3(-0.18f, 2.85f, -0.22f), new Vector3(0.09f, 0.09f, 0.04f), TownSwatch.Basalt);
                for (int i = 0; i < 5; i++)
                {
                    float y = 0.7f + i * 0.33f;
                    float half = Mathf.Lerp(0.32f, 0.24f, i / 4f);
                    b.Box(new Vector3(0f, y, -half - 0.005f), new Vector3(half * 1.3f, 0.04f, 0.02f), TownSwatch.StoneDark);
                }

                b.Plate(new[] { new Vector3(0f, 0.5f, 0.3f), new Vector3(0f, 1.1f, 0.55f), new Vector3(0f, 1.6f, 0.3f) }, 0.06f, TownSwatch.Stone, TownSwatch.StoneDark);
            });
        }

        /// <summary>The cave spring: a basalt wall with an arched niche, a stone spout pouring into a basin, a lamp niche above.</summary>
        private static void Fountain(TownMeshBuilder b, float s)
        {
            b.With(Vector3.zero, 0f, new Vector3(s, s, s), () =>
            {
                b.BoxMinMax(new Vector3(-1.1f, 0f, 0.1f), new Vector3(1.1f, 1.9f, 0.5f), TownSwatch.Basalt, TownSwatch.StoneDark);
                b.With(new Vector3(0f, 0f, 0.1f), 0f, () =>
                {
                    b.BoxMinMax(new Vector3(-0.45f, 0.55f, -0.02f), new Vector3(0.45f, 1.35f, 0f), TownSwatch.Indigo);
                    ArchFill(b, 0f, 1.35f, 0.9f, 0.3f, -0.021f, TownSwatch.Indigo);
                    LampNiche(b, 0f, 1.55f);
                });

                b.Log(new Vector3(0f, 1.0f, 0.1f), new Vector3(0f, 0.95f, -0.25f), 0.07f, TownSwatch.Stone, 5);
                b.Box(new Vector3(0f, 0.68f, -0.28f), new Vector3(0.06f, 0.55f, 0.06f), TownSwatch.Water);
                b.BoxMinMax(new Vector3(-0.9f, 0f, -0.5f), new Vector3(0.9f, 0.45f, 0.1f), TownSwatch.PinkTuff, TownSwatch.Water);
                b.BoxMinMax(new Vector3(-0.95f, 0.45f, -0.55f), new Vector3(0.95f, 0.5f, -0.45f), TownSwatch.LimeWhite);
            });
        }

        // ------------------------------------------------------------------
        // Dressings of the regional shops and residents
        // ------------------------------------------------------------------

        /// <summary>The regional dressings (called from <see cref="Dressing"/>): goods on the aivan / forecourt beside the door.</summary>
        private static void RegionDressing(TownMeshBuilder b, TownBuildingSpec spec, float x, float y, float z, float zFacade)
        {
            switch (spec.Dressing)
            {
                case TownShopDressing.HuntingRack:
                    WarCampDressing(b, spec, x, y, z, zFacade);
                    break;

                case TownShopDressing.Runes:
                    RuneHallDressing(b, spec, x, y, z, zFacade);
                    break;

                case TownShopDressing.AirBottles:
                case TownShopDressing.Tinkerer:
                    TunnelDressing(b, spec, x, y, z, zFacade);
                    break;

                case TownShopDressing.Pearls:
                case TownShopDressing.RootWeaver:
                    CoastDressing(b, spec, x, y, z, zFacade);
                    break;

                case TownShopDressing.Selaria:
                    // A saddle on a trestle, a rolled felt rug, bridles on the wall, a dutar leaning by the door.
                    foreach (float lx in new[] { -0.35f, 0.35f })
                    {
                        b.Log(new Vector3(x + lx, y, z - 0.55f), new Vector3(x + lx * 0.6f, y + 0.75f, z - 0.55f), 0.03f, TownSwatch.WoodDark, 4);
                    }

                    b.Log(new Vector3(x - 0.3f, y + 0.75f, z - 0.55f), new Vector3(x + 0.3f, y + 0.75f, z - 0.55f), 0.05f, TownSwatch.WoodDark, 4);
                    b.Blob(new Vector3(x, y + 0.9f, z - 0.55f), new Vector3(0.6f, 0.26f, 0.48f), TownSwatch.Walnut);
                    b.Box(new Vector3(x, y + 0.8f, z - 0.55f), new Vector3(0.7f, 0.04f, 0.56f), TownSwatch.ClothRed);
                    b.Log(new Vector3(x - 0.6f, y + 0.12f, z - 0.05f), new Vector3(x + 0.6f, y + 0.12f, z - 0.15f), 0.12f, TownSwatch.ClothIndigo, 6, TownSwatch.ClothYellow);
                    for (int i = 0; i < 3; i++)
                    {
                        float wx = x - 0.4f + i * 0.4f;
                        b.Log(new Vector3(wx, y + 1.7f, zFacade - 0.04f), new Vector3(wx + 0.05f, y + 1.0f, zFacade - 0.05f), 0.02f, TownSwatch.Walnut, 4);
                    }

                    b.Log(new Vector3(x + 0.75f, y, z + 0.1f), new Vector3(x + 0.8f, y + 1.15f, zFacade - 0.08f), 0.025f, TownSwatch.WoodDark, 4);
                    b.Blob(new Vector3(x + 0.76f, y + 0.18f, z + 0.12f), new Vector3(0.3f, 0.34f, 0.12f), TownSwatch.Walnut);
                    break;

                case TownShopDressing.Forja:
                    // Baskets of ore, the quench trough, tongs on the wall and the mine bell on its frame.
                    Basket(b, new Vector3(x - 0.4f, y, z - 0.4f), TownSwatch.StoneDark);
                    Basket(b, new Vector3(x + 0.3f, y, z - 0.6f), TownSwatch.Copper);
                    b.Box(new Vector3(x, y + 0.22f, z + 0.05f), new Vector3(1.1f, 0.44f, 0.45f), TownSwatch.Stone, 0f, TownSwatch.Water);
                    for (int i = 0; i < 2; i++)
                    {
                        b.Log(new Vector3(x - 0.2f + i * 0.3f, y + 1.6f, zFacade - 0.04f), new Vector3(x - 0.1f + i * 0.3f, y + 0.95f, zFacade - 0.05f), 0.018f, TownSwatch.Iron, 4);
                    }

                    b.Log(new Vector3(x + 0.75f, y, z - 0.9f), new Vector3(x + 0.75f, y + 1.7f, z - 0.9f), 0.04f, TownSwatch.WoodDark, 4);
                    b.Log(new Vector3(x + 0.75f, y + 1.7f, z - 0.9f), new Vector3(x + 0.45f, y + 1.7f, z - 0.9f), 0.03f, TownSwatch.WoodDark, 4);
                    b.Cylinder(new Vector3(x + 0.45f, y + 1.35f, z - 0.9f), 0.15f, 0.07f, 0.3f, 7, TownSwatch.Copper, TownSwatch.Copper);
                    break;

                case TownShopDressing.Lamparinas:
                    // A stepped shelf of lamps of every size, oil jars, and three lanterns hanging from the porch beam.
                    for (int step = 0; step < 3; step++)
                    {
                        float sy = y + 0.3f + step * 0.42f;
                        float sz = z + 0.15f - step * 0.0f;
                        b.Box(new Vector3(x, sy, sz), new Vector3(1.3f, 0.06f, 0.4f), TownSwatch.WoodDark);
                        for (int i = 0; i < 4 - step; i++)
                        {
                            float lx = x - 0.45f + i * 0.3f + step * 0.15f;
                            bool clay = ((i + step) & 1) == 0;
                            b.Cylinder(new Vector3(lx, sy + 0.03f, sz), 0.08f, 0.06f, 0.07f, 6, clay ? TownSwatch.AdobeOchre : TownSwatch.Copper, clay ? TownSwatch.AdobeOchre : TownSwatch.Copper);
                            b.Blob(new Vector3(lx, sy + 0.17f, sz), new Vector3(0.07f, 0.14f, 0.07f), TownSwatch.LampGlow);
                        }
                    }

                    foreach (float lx in new[] { -0.6f, 0.6f })
                    {
                        b.Log(new Vector3(x + lx, y, z + 0.15f), new Vector3(x + lx, y + 1.2f, z + 0.15f), 0.025f, TownSwatch.WoodDark, 4);
                    }

                    for (int i = 0; i < 2; i++)
                    {
                        var jar = new Vector3(x - 0.3f + i * 0.55f, y, z - 0.6f);
                        b.Cylinder(jar, 0.14f, 0.22f, 0.3f, 7, TownSwatch.RedClay);
                        b.Cylinder(jar + Vector3.up * 0.3f, 0.22f, 0.08f, 0.22f, 7, TownSwatch.RedClay, TownSwatch.Black);
                    }

                    for (int i = 0; i < 3; i++)
                    {
                        Lantern(b, new Vector3(x - 0.7f + i * 0.7f, spec.EaveHeight - 1.25f - (i & 1) * 0.25f, zFacade - spec.PorchDepth * 0.55f), 0.7f);
                    }

                    break;

                case TownShopDressing.Weaver:
                    // Three Basotho blankets (stripes and bands) on a line between two posts, and a little loom.
                    foreach (float lx in new[] { -0.95f, 0.95f })
                    {
                        b.Log(new Vector3(x + lx, y, z - 0.5f), new Vector3(x + lx, y + 1.65f, z - 0.5f), 0.035f, TownSwatch.WoodDark, 4);
                    }

                    b.Log(new Vector3(x - 0.95f, y + 1.6f, z - 0.5f), new Vector3(x + 0.95f, y + 1.6f, z - 0.5f), 0.012f, TownSwatch.Thatch, 3);
                    for (int i = 0; i < 3; i++)
                    {
                        float bx = x - 0.6f + i * 0.6f;
                        TownSwatch field = i == 0 ? TownSwatch.ClothIndigo : i == 1 ? TownSwatch.ClothYellow : TownSwatch.ClothRed;
                        TownSwatch band = i == 1 ? TownSwatch.ClothIndigo : TownSwatch.ClothYellow;
                        b.BoxMinMax(new Vector3(bx - 0.26f, y + 0.75f, z - 0.52f), new Vector3(bx + 0.26f, y + 1.6f, z - 0.49f), field);
                        b.BoxMinMax(new Vector3(bx - 0.27f, y + 1.0f, z - 0.535f), new Vector3(bx + 0.27f, y + 1.07f, z - 0.52f), band);
                        b.BoxMinMax(new Vector3(bx - 0.27f, y + 1.3f, z - 0.535f), new Vector3(bx + 0.27f, y + 1.37f, z - 0.52f), TownSwatch.LimeWhite);
                    }

                    b.Box(new Vector3(x + 0.2f, y + 0.4f, z + 0.1f), new Vector3(0.8f, 0.06f, 0.4f), TownSwatch.Walnut);
                    b.BoxMinMax(new Vector3(x - 0.15f, y + 0.43f, z), new Vector3(x + 0.55f, y + 0.46f, z + 0.25f), TownSwatch.ClothIndigo);
                    foreach (float lx in new[] { -0.2f, 0.6f })
                    {
                        b.Log(new Vector3(x + lx, y, z + 0.1f), new Vector3(x + lx, y + 0.9f, z + 0.1f), 0.025f, TownSwatch.WoodDark, 4);
                    }

                    b.Log(new Vector3(x - 0.2f, y + 0.9f, z + 0.1f), new Vector3(x + 0.6f, y + 0.9f, z + 0.1f), 0.025f, TownSwatch.WoodDark, 4);
                    break;

                case TownShopDressing.Drums:
                    // Skin drums of three sizes and a lyre leaning on the wall.
                    for (int i = 0; i < 3; i++)
                    {
                        float r = 0.16f + i * 0.05f;
                        var at = new Vector3(x - 0.5f + i * 0.45f, y, z - 0.4f + (i & 1) * 0.25f);
                        b.Cylinder(at, r * 0.8f, r, 0.35f + i * 0.12f, 8, i == 1 ? TownSwatch.Walnut : TownSwatch.WoodLight, TownSwatch.ClothWhite);
                        b.Cylinder(at + Vector3.up * (0.2f + i * 0.06f), r * 0.92f, r * 0.94f, 0.04f, 8, TownSwatch.ClothRed);
                    }

                    b.Log(new Vector3(x + 0.6f, y, z + 0.05f), new Vector3(x + 0.55f, y + 1.0f, zFacade - 0.08f), 0.025f, TownSwatch.Walnut, 4);
                    b.Log(new Vector3(x + 0.95f, y, z + 0.05f), new Vector3(x + 0.9f, y + 1.0f, zFacade - 0.08f), 0.025f, TownSwatch.Walnut, 4);
                    b.Log(new Vector3(x + 0.55f, y + 0.95f, zFacade - 0.1f), new Vector3(x + 0.9f, y + 0.95f, zFacade - 0.1f), 0.03f, TownSwatch.WoodDark, 4);
                    b.Blob(new Vector3(x + 0.74f, y + 0.2f, z), new Vector3(0.45f, 0.35f, 0.16f), TownSwatch.Thatch);
                    break;

                case TownShopDressing.Wicks:
                    // Two baskets of white wicks and the cutting block with its little knife.
                    for (int i = 0; i < 2; i++)
                    {
                        var at = new Vector3(x - 0.35f + i * 0.6f, y, z - 0.4f);
                        b.Cylinder(at, 0.2f, 0.26f, 0.26f, 8, TownSwatch.Thatch, TownSwatch.DirtDark);
                        b.Cylinder(at + Vector3.up * 0.24f, 0.18f, 0.12f, 0.08f, 8, TownSwatch.ClothWhite, TownSwatch.ClothWhite);
                    }

                    b.Cylinder(new Vector3(x + 0.1f, y, z + 0.15f), 0.22f, 0.22f, 0.42f, 7, TownSwatch.Trunk, TownSwatch.WoodLight);
                    b.Box(new Vector3(x + 0.12f, y + 0.45f, z + 0.15f), new Vector3(0.2f, 0.02f, 0.05f), TownSwatch.Iron);
                    break;

                case TownShopDressing.Storyteller:
                    // The carpet where the children sit, three cushions, a low stool, and Ananse on her thread.
                    b.Box(new Vector3(x, y + 0.01f, z - 0.45f), new Vector3(1.5f, 0.02f, 1.0f), TownSwatch.ClothRed);
                    b.Box(new Vector3(x, y + 0.022f, z - 0.45f), new Vector3(1.2f, 0.01f, 0.7f), TownSwatch.ClothIndigo);
                    for (int i = 0; i < 3; i++)
                    {
                        b.Blob(new Vector3(x - 0.5f + i * 0.5f, y + 0.1f, z - 0.75f + (i & 1) * 0.15f), new Vector3(0.38f, 0.18f, 0.34f), i == 1 ? TownSwatch.ClothYellow : TownSwatch.Turquoise);
                    }

                    b.Box(new Vector3(x + 0.1f, y + 0.25f, z + 0.1f), new Vector3(0.45f, 0.06f, 0.35f), TownSwatch.WoodDark);
                    b.Log(new Vector3(x - 0.65f, y + 1.9f, zFacade - 0.15f), new Vector3(x - 0.65f, y + 1.35f, zFacade - 0.15f), 0.006f, TownSwatch.ClothWhite, 3);
                    b.Blob(new Vector3(x - 0.65f, y + 1.3f, zFacade - 0.15f), new Vector3(0.1f, 0.1f, 0.1f), TownSwatch.Black);
                    break;

                case TownShopDressing.OldMiner:
                    // Pick on the wall, a coil of rope, his old miner's lamp on a crate.
                    b.Log(new Vector3(x - 0.3f, y + 0.3f, zFacade - 0.06f), new Vector3(x + 0.2f, y + 1.4f, zFacade - 0.06f), 0.03f, TownSwatch.Walnut, 4);
                    b.Box(new Vector3(x + 0.2f, y + 1.4f, zFacade - 0.08f), new Vector3(0.5f, 0.08f, 0.06f), TownSwatch.Iron, 0f);
                    b.Ring(new Vector3(x - 0.4f, y + 0.09f, z - 0.45f), 0.1f, 0.3f, 8, TownSwatch.Thatch);
                    b.Cylinder(new Vector3(x - 0.4f, y, z - 0.45f), 0.3f, 0.3f, 0.09f, 8, TownSwatch.Thatch);
                    b.Box(new Vector3(x + 0.45f, y + 0.25f, z - 0.35f), new Vector3(0.5f, 0.5f, 0.45f), TownSwatch.WoodWeathered, 10f, TownSwatch.WoodLight);
                    Lantern(b, new Vector3(x + 0.45f, y + 0.5f, z - 0.35f), 0.6f);
                    break;

                case TownShopDressing.Cistern:
                    // Water jars, the copper ladle on a stand, a wet stone trough.
                    Jugs(b, new Vector3(x - 0.1f, y, z - 0.45f));
                    b.Box(new Vector3(x + 0.3f, y + 0.18f, z + 0.1f), new Vector3(0.9f, 0.36f, 0.4f), TownSwatch.Sandstone, 0f, TownSwatch.Water);
                    b.Log(new Vector3(x + 0.85f, y, z - 0.5f), new Vector3(x + 0.85f, y + 0.95f, z - 0.5f), 0.025f, TownSwatch.WoodDark, 4);
                    b.Cylinder(new Vector3(x + 0.85f, y + 0.85f, z - 0.62f), 0.09f, 0.11f, 0.08f, 6, TownSwatch.Copper, TownSwatch.Copper);
                    break;

                case TownShopDressing.Herder:
                    // A white goat at the door, a water trough, the crook against the wall.
                    var goat = new Vector3(x, y, z - 0.6f);
                    b.Crown(goat + new Vector3(0f, 0.5f, 0f), new Vector3(0.32f, 0.32f, 0.62f), TownSwatch.ClothWhite);
                    foreach (Vector2 l in new[] { new Vector2(-0.1f, -0.22f), new Vector2(0.1f, -0.22f), new Vector2(-0.1f, 0.22f), new Vector2(0.1f, 0.22f) })
                    {
                        b.Log(goat + new Vector3(l.x, 0f, l.y), goat + new Vector3(l.x, 0.4f, l.y), 0.025f, TownSwatch.Hair, 4);
                    }

                    b.Crown(goat + new Vector3(0f, 0.72f, -0.38f), new Vector3(0.18f, 0.2f, 0.26f), TownSwatch.ClothWhite);
                    b.Log(goat + new Vector3(-0.05f, 0.8f, -0.33f), goat + new Vector3(-0.08f, 0.95f, -0.22f), 0.015f, TownSwatch.Hair, 3);
                    b.Log(goat + new Vector3(0.05f, 0.8f, -0.33f), goat + new Vector3(0.08f, 0.95f, -0.22f), 0.015f, TownSwatch.Hair, 3);
                    b.Box(new Vector3(x + 0.7f, y + 0.15f, z), new Vector3(0.4f, 0.3f, 0.9f), TownSwatch.Walnut, 0f, TownSwatch.Water);
                    b.Log(new Vector3(x - 0.75f, y, z + 0.15f), new Vector3(x - 0.7f, y + 1.5f, zFacade - 0.08f), 0.025f, TownSwatch.WoodLight, 4);
                    break;

                case TownShopDressing.Pottery:
                    // Jars drying on a plank and the potter's kick wheel.
                    b.Box(new Vector3(x, y + 0.3f, z + 0.05f), new Vector3(1.4f, 0.06f, 0.35f), TownSwatch.WoodLight);
                    foreach (float lx in new[] { -0.6f, 0.6f })
                    {
                        b.Box(new Vector3(x + lx, y + 0.14f, z + 0.05f), new Vector3(0.15f, 0.28f, 0.3f), TownSwatch.Sandstone);
                    }

                    for (int i = 0; i < 4; i++)
                    {
                        var jar = new Vector3(x - 0.5f + i * 0.33f, y + 0.33f, z + 0.05f);
                        TownSwatch clay = (i & 1) == 0 ? TownSwatch.AdobeOchre : TownSwatch.RedClay;
                        b.Cylinder(jar, 0.08f, 0.13f, 0.16f, 7, clay);
                        b.Cylinder(jar + Vector3.up * 0.16f, 0.13f, 0.05f, 0.14f, 7, clay, TownSwatch.Black);
                    }

                    var wheel = new Vector3(x + 0.1f, y, z - 0.65f);
                    b.Cylinder(wheel, 0.32f, 0.32f, 0.08f, 9, TownSwatch.WoodDark, TownSwatch.WoodDark);
                    b.Log(wheel, wheel + Vector3.up * 0.55f, 0.03f, TownSwatch.Iron, 4);
                    b.Cylinder(wheel + Vector3.up * 0.55f, 0.2f, 0.2f, 0.05f, 9, TownSwatch.WoodLight, TownSwatch.Mud);
                    b.Cylinder(wheel + Vector3.up * 0.6f, 0.08f, 0.1f, 0.14f, 7, TownSwatch.Mud);
                    break;
            }
        }

        // ------------------------------------------------------------------
        // Copas de Garajonay: the laurel canopy
        // ------------------------------------------------------------------

        /// <summary>
        /// A giant laurel of Garajonay carrying a ring platform at <see cref="RopeBridgeHeight"/> × <paramref name="s"/> (the
        /// height the rope bridges of the same scale arrive at): buttress roots, the trunk, a spiral stair of planks round it,
        /// four struts under the plank ring, a rail, a round plank house with two layers of laurel-leaf roof (door to the
        /// south, towards the camera), two cocoon lanterns, the crown above and moss hanging from it.
        /// </summary>
        private static void CanopyTree(TownMeshBuilder b, float s)
        {
            float py = RopeBridgeHeight * s;
            float pr = CanopyPlatformRadius * s;

            // Roots and trunk.
            for (int i = 0; i < 4; i++)
            {
                float a = (i * 90f + 45f) * Mathf.Deg2Rad;
                var dir = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a));
                b.Log(dir * 0.6f * s + Vector3.up * 1.1f * s, dir * 1.9f * s, 0.22f * s, TownSwatch.Trunk, 5);
            }

            b.Cylinder(Vector3.zero, 1.0f * s, 0.62f * s, py + 4.2f * s, 8, TownSwatch.Trunk);

            // Spiral stair: planks round the trunk, two turns from the ground to the platform.
            const int steps = 22;
            for (int i = 0; i < steps; i++)
            {
                float t = (i + 0.5f) / steps;
                float a = t * Mathf.PI * 4f;
                var dir = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a));
                float y = Mathf.Lerp(0.35f * s, py - 0.3f * s, t);
                b.Box(dir * 1.45f * s + Vector3.up * y, new Vector3(0.9f, 0.08f, 0.42f) * s, (i & 1) == 0 ? TownSwatch.WoodLight : TownSwatch.WoodWeathered,
                    -a * Mathf.Rad2Deg);
            }

            // The plank ring, its struts and rail.
            b.Cylinder(new Vector3(0f, py - 0.22f * s, 0f), pr, pr, 0.22f * s, 12, TownSwatch.WoodWeathered, TownSwatch.WoodLight);
            for (int i = 0; i < 4; i++)
            {
                float a = i * Mathf.PI * 0.5f;
                var dir = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a));
                b.Log(dir * 0.7f * s + Vector3.up * (py - 2.4f * s), dir * (pr - 0.3f * s) + Vector3.up * (py - 0.25f * s), 0.09f * s, TownSwatch.WoodDark, 4);
            }

            for (int i = 0; i < 10; i++)
            {
                float a = i * Mathf.PI * 2f / 10f;
                var dir = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a));
                b.Log(dir * (pr - 0.1f * s) + Vector3.up * py, dir * (pr - 0.1f * s) + Vector3.up * (py + 0.9f * s), 0.04f * s, TownSwatch.WoodDark, 4);
            }

            b.Ring(new Vector3(0f, py + 0.9f * s, 0f), pr - 0.18f * s, pr - 0.02f * s, 12, TownSwatch.WoodDark);

            // The round plank house (north side of the ring, door to the south) and its leaf roof.
            var house = new Vector3(0f, py, 0.9f * s);
            b.Cylinder(house, 1.5f * s, 1.5f * s, 2.0f * s, 10, TownSwatch.Walnut);
            b.Box(house + new Vector3(0f, 0.85f * s, -1.48f * s), new Vector3(0.7f, 1.5f, 0.06f) * s, TownSwatch.DoorPlank);
            b.Box(house + new Vector3(0.95f * s, 1.25f * s, -1.15f * s), new Vector3(0.4f, 0.35f, 0.05f) * s, TownSwatch.WindowLit, 40f);
            b.Cylinder(house + Vector3.up * 1.9f * s, 2.1f * s, 0f, 1.1f * s, 10, TownSwatch.LeafDark);
            b.Cylinder(house + Vector3.up * 2.55f * s, 1.3f * s, 0f, 0.9f * s, 10, TownSwatch.GrassDark, null, 0.5f);

            // Cocoon lanterns on the rail.
            foreach (float a in new[] { 200f, 340f })
            {
                float r = a * Mathf.Deg2Rad;
                Vector3 at = new Vector3(Mathf.Cos(r), 0f, Mathf.Sin(r)) * (pr - 0.1f * s) + Vector3.up * (py + 1.25f * s);
                b.Log(at + Vector3.up * 0.35f * s, at + Vector3.up * 0.15f * s, 0.015f * s, TownSwatch.Iron, 3);
                b.Blob(at, new Vector3(0.3f, 0.42f, 0.3f) * s, TownSwatch.LampGlow);
                b.Blob(at + Vector3.up * 0.02f * s, new Vector3(0.34f, 0.2f, 0.34f) * s, TownSwatch.ClothWhite);
            }

            // The crown over the platform and the moss hanging from it.
            b.Crown(new Vector3(0f, py + 5.2f * s, 0f), new Vector3(7.5f, 3.6f, 7.5f) * s, TownSwatch.LeafDark, 15f);
            b.Crown(new Vector3(1.2f * s, py + 6.8f * s, -0.6f * s), new Vector3(4.6f, 2.6f, 4.6f) * s, TownSwatch.GrassDark, 50f);
            b.Crown(new Vector3(-1.6f * s, py + 4.6f * s, 1.4f * s), new Vector3(3.6f, 2.0f, 3.6f) * s, TownSwatch.Poplar, 80f);
            for (int i = 0; i < 6; i++)
            {
                float a = (i * 60f + 20f) * Mathf.Deg2Rad;
                var top = new Vector3(Mathf.Cos(a) * 3.1f * s, py + 4.2f * s, Mathf.Sin(a) * 3.1f * s);
                b.Log(top, top + Vector3.down * (1.0f + (i % 3) * 0.4f) * s, 0.05f * s, TownSwatch.Moss, 3);
            }
        }
    }
}
