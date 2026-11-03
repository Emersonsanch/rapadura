using UnityEngine;

namespace Rapadura.Gameplay.World.Cities.Town
{
    /// <summary>
    /// The pieces of the Acampamento de Soslan, the centaurs' camp (Docs/Mapas100.md §3.3 and map 47, style
    /// <c>SteppeCamp</c>): the round felt tents on wagons (<see cref="FeltYurtHouse"/>: a plank bed on four spoked wheels, the
    /// felt wall with dyed bands, the domed felt roof with the smoke crown, a painted door and a felt awning in front), small
    /// felt carts, horsehair standards with the gold deer of the Scythian animal style, rope corrals with hay, the collective
    /// stone oven on the plaza and the gate of two standards with pennants on a rope; Syrdon's saddlery. Same rules as the
    /// rest of the kit: flat colours from <see cref="TownPalette"/> (row 16 holds felt, the dyed felts red and blue, steppe
    /// grass, horsehair, rawhide, Scythian gold and the camp fire), low-poly, deterministic. The camp moves across the steppe
    /// (the ficha); in the city it stands still.
    /// </summary>
    public static partial class TownKit
    {
        /// <summary>Radius of a <see cref="CityPropKind.FeltWagon"/> (× its scale), shafts included: its collider.</summary>
        public const float FeltWagonRadius = 1.7f;

        /// <summary>Radius of a <see cref="CityPropKind.HorseStandard"/>'s foot (× its scale): its collider.</summary>
        public const float HorseStandardRadius = 0.25f;

        /// <summary>Half the thickness of a <see cref="CityPropKind.RopeCorral"/>: its collider is the run inflated by this.</summary>
        public const float CorralHalfThickness = 0.15f;

        /// <summary>Radius of the <see cref="CityPropKind.StoneOven"/> (× its scale): its collider.</summary>
        public const float StoneOvenRadius = 1.2f;

        /// <summary>Height of the wagon bed a <see cref="TownBuildingSpec.FeltYurt"/> stands on.</summary>
        private const float WagonBedTop = 0.6f;

        // ------------------------------------------------------------------
        // The felt tent on its wagon
        // ------------------------------------------------------------------

        /// <summary>
        /// A <see cref="TownBuildingSpec.FeltYurt"/> (local frame, front −z): a plank bed on four spoked wheels and two axles,
        /// the round felt wall (<see cref="TownBuildingSpec.WallColor"/>) with two dyed bands (<see cref="TownBuildingSpec.TrimColor"/>
        /// and the gold rim), the domed felt roof (<see cref="TownBuildingSpec.RoofColor"/>) with the wooden smoke crown and
        /// rope ties, a painted door with a gold frame and plank steps down, and in front a plank deck with two poles carrying
        /// a felt awning (the poles at <see cref="TownBuildingBuilder.PorchPostsX"/>, the colliders use the same list) and the
        /// sign hanging from its edge.
        /// </summary>
        public static void FeltYurtHouse(TownMeshBuilder b, TownBuildingSpec spec, float hw, float zFront, float zFacade, float zBack, float top)
        {
            float cz = (zFacade + zBack) * 0.5f;
            float r = Mathf.Max(1.2f, Mathf.Min(hw - 0.35f, (zBack - zFacade) * 0.5f - 0.05f));
            float bed = WagonBedTop;
            float wallH = Mathf.Max(1.5f, spec.WallHeight * 0.6f);
            float wallTop = bed + wallH;
            TownSwatch felt = spec.WallColor;
            TownSwatch band = spec.TrimColor;

            // The wagon: bed, axles and four spoked wheels.
            b.BoxMinMax(new Vector3(-r - 0.1f, bed - 0.18f, cz - r - 0.1f), new Vector3(r + 0.1f, bed, cz + r + 0.1f), TownSwatch.WoodDark, TownSwatch.WoodWeathered);
            foreach (float az in new[] { cz - r * 0.6f, cz + r * 0.6f })
            {
                b.Log(new Vector3(-r - 0.25f, 0.42f, az), new Vector3(r + 0.25f, 0.42f, az), 0.06f, TownSwatch.WoodDark, 4);
                foreach (float side in new[] { -1f, 1f })
                {
                    float x = side * (r + 0.2f);
                    b.Log(new Vector3(x - 0.06f, 0.42f, az), new Vector3(x + 0.06f, 0.42f, az), 0.4f, TownSwatch.WoodDark, 8, TownSwatch.SteppeLeather);
                    b.Log(new Vector3(x + side * 0.07f, 0.1f, az), new Vector3(x + side * 0.07f, 0.74f, az), 0.03f, TownSwatch.WoodWeathered, 4);
                }
            }

            // The felt wall with its bands, the roof and the smoke crown.
            b.Cylinder(new Vector3(0f, bed, cz), r, r, wallH, 12, felt);
            b.Cylinder(new Vector3(0f, bed + wallH * 0.3f, cz), r + 0.02f, r + 0.02f, 0.2f, 12, band);
            b.Cylinder(new Vector3(0f, wallTop - 0.12f, cz), r + 0.03f, r + 0.03f, 0.12f, 12, TownSwatch.ScythianGold);
            float roofH = r * 0.75f;
            b.Cylinder(new Vector3(0f, wallTop, cz), r + 0.12f, r * 0.55f, roofH * 0.55f, 12, spec.RoofColor);
            b.Cylinder(new Vector3(0f, wallTop + roofH * 0.55f, cz), r * 0.55f, r * 0.2f, roofH * 0.45f, 12, spec.RoofColor);
            b.Cylinder(new Vector3(0f, wallTop + roofH, cz), r * 0.2f, r * 0.18f, 0.12f, 8, TownSwatch.WoodDark, TownSwatch.HorseHair);
            for (int i = 0; i < 4; i++)
            {
                float a = (i * 90f + 45f) * Mathf.Deg2Rad;
                var dir = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a));
                b.Log(new Vector3(0f, wallTop + roofH + 0.06f, cz) + dir * r * 0.2f, new Vector3(0f, wallTop + 0.05f, cz) + dir * (r + 0.13f), 0.03f, TownSwatch.SteppeLeather, 4);
            }

            // The painted door on the front of the drum, the gold frame and plank steps down to the deck.
            float doorX = Mathf.Clamp(spec.DoorOffset * spec.Width, -r * 0.45f, r * 0.45f);
            float doorW = spec.WideDoor ? 1.1f : 0.85f;
            float doorZ = cz - Mathf.Sqrt(Mathf.Max(0.01f, r * r - doorX * doorX)) - 0.04f;
            float doorTop = bed + Mathf.Min(1.35f, wallH - 0.15f);
            b.BoxMinMax(new Vector3(doorX - doorW * 0.5f - 0.08f, bed, doorZ - 0.06f), new Vector3(doorX + doorW * 0.5f + 0.08f, doorTop + 0.08f, doorZ + 0.08f), TownSwatch.ScythianGold);
            b.BoxMinMax(new Vector3(doorX - doorW * 0.5f, bed, doorZ - 0.09f), new Vector3(doorX + doorW * 0.5f, doorTop, doorZ - 0.06f), spec.JoineryColor);
            b.Box(new Vector3(doorX, bed + (doorTop - bed) * 0.6f, doorZ - 0.1f), new Vector3(doorW * 0.5f, 0.3f, 0.02f), band, 0f);
            b.BoxMinMax(new Vector3(doorX - doorW * 0.5f, 0f, doorZ - 0.65f), new Vector3(doorX + doorW * 0.5f, bed * 0.5f, doorZ - 0.3f), TownSwatch.WoodWeathered);

            if (spec.PorchDepth > 0.01f)
            {
                float deck = CitySolids.DeckHeight;
                b.BoxMinMax(new Vector3(-hw, 0f, zFront), new Vector3(hw, deck, zFacade), TownSwatch.WoodDark, TownSwatch.WoodWeathered);
                float z = zFront + TownBuildingBuilder.PostInset;
                float awning = Mathf.Max(2.1f, bed + 1.6f);
                foreach (float x in TownBuildingBuilder.PorchPostsX(spec))
                {
                    b.Log(new Vector3(x, deck, z), new Vector3(x, awning + 0.25f, z), 0.06f, spec.JoineryColor, 6);
                    b.Log(new Vector3(x, awning + 0.25f, z), new Vector3(x, awning - 0.35f, z - 0.06f), 0.05f, TownSwatch.HorseHair, 4);
                }

                SlabUp(b, new Vector3(-hw, awning, z - 0.15f), new Vector3(hw, awning, z - 0.15f), new Vector3(hw, awning + 0.4f, zFacade + 0.1f), new Vector3(-hw, awning + 0.4f, zFacade + 0.1f),
                    0.06f, felt, band);
                b.BoxMinMax(new Vector3(-hw - 0.02f, awning - 0.22f, z - 0.2f), new Vector3(hw + 0.02f, awning, z - 0.14f), band);

                if (spec.Sign)
                {
                    var at = new Vector3(doorX, awning - 0.5f, z - 0.25f);
                    b.Log(at + new Vector3(-0.4f, 0.12f, 0f), at + new Vector3(-0.4f, 0.3f, 0f), 0.015f, TownSwatch.SteppeLeather, 3);
                    b.Log(at + new Vector3(0.4f, 0.12f, 0f), at + new Vector3(0.4f, 0.3f, 0f), 0.015f, TownSwatch.SteppeLeather, 3);
                    b.Box(at, new Vector3(1.1f, 0.3f, 0.05f), TownSwatch.ScythianGold, 0f);
                    b.Box(at + new Vector3(0f, 0f, -0.03f), new Vector3(0.94f, 0.2f, 0.02f), spec.SignColor, 0f);
                }
            }
        }

        /// <summary>A slab whose top always faces up, whatever order the four corners come in.</summary>
        private static void SlabUp(TownMeshBuilder b, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float thickness, TownSwatch top, TownSwatch under)
        {
            if (Vector3.Cross(p1 - p0, p3 - p0).y > 0f)
            {
                b.Slab(p0, p1, p2, p3, thickness, top, under);
            }
            else
            {
                b.Slab(p3, p2, p1, p0, thickness, top, under);
            }
        }

        /// <summary>
        /// The gate of the camp at a street's end: two tall standards with horsehair tassels and gold finials, a rope between
        /// them hung with dyed felt pennants, red and blue in turn.
        /// </summary>
        public static void StandardGate(TownMeshBuilder b, Vector3 a, Vector3 c)
        {
            Vector3 span = c - a;
            float w = new Vector2(span.x, span.z).magnitude;
            if (w < 0.5f)
            {
                return;
            }

            b.With(a, YawAlong(new Vector2(span.x, span.z) / w), () =>
            {
                foreach (float x in new[] { 0f, w })
                {
                    b.Blob(new Vector3(x, 0.15f, 0f), new Vector3(0.7f, 0.35f, 0.7f), TownSwatch.Stone);
                    b.Log(new Vector3(x, 0f, 0f), new Vector3(x, 3.8f, 0f), 0.08f, TownSwatch.WoodDark, 6);
                    b.Cylinder(new Vector3(x, 3.8f, 0f), 0.14f, 0f, 0.4f, 4, TownSwatch.ScythianGold, null, 0.5f);
                    for (int i = 0; i < 3; i++)
                    {
                        float a0 = i * 120f * Mathf.Deg2Rad;
                        var off = new Vector3(Mathf.Cos(a0), 0f, Mathf.Sin(a0)) * 0.12f;
                        b.Log(new Vector3(x, 3.5f, 0f) + off, new Vector3(x, 2.6f, 0f) + off * 1.8f, 0.04f, TownSwatch.HorseHair, 4);
                    }
                }

                b.Log(new Vector3(0f, 3.3f, 0f), new Vector3(w * 0.5f, 3.05f, 0f), 0.025f, TownSwatch.SteppeLeather, 4);
                b.Log(new Vector3(w * 0.5f, 3.05f, 0f), new Vector3(w, 3.3f, 0f), 0.025f, TownSwatch.SteppeLeather, 4);
                int count = Mathf.Max(3, Mathf.RoundToInt(w / 0.8f));
                for (int i = 1; i < count; i++)
                {
                    float t = (float)i / count;
                    float x = w * t;
                    float y = 3.3f - 0.25f * (1f - Mathf.Abs(t - 0.5f) * 2f);
                    b.Cylinder(new Vector3(x, y - 0.5f, 0f), 0f, 0.2f, 0.5f, 3, (i & 1) == 0 ? TownSwatch.FeltRed : TownSwatch.FeltBlue);
                }
            });
        }

        // ------------------------------------------------------------------
        // Props
        // ------------------------------------------------------------------

        /// <summary>
        /// A small felt tent on a two-wheeled cart, its shafts resting on the ground towards local −z (Variant 1: loaded with
        /// felt bales and a rolled rug instead of a tent).
        /// </summary>
        private static void FeltWagon(TownMeshBuilder b, float s, int variant)
        {
            b.With(Vector3.zero, 0f, new Vector3(s, s, s), () =>
            {
                b.BoxMinMax(new Vector3(-0.75f, 0.45f, -0.85f), new Vector3(0.75f, 0.6f, 0.95f), TownSwatch.WoodDark, TownSwatch.WoodWeathered);
                b.Log(new Vector3(-0.95f, 0.42f, 0.1f), new Vector3(0.95f, 0.42f, 0.1f), 0.05f, TownSwatch.WoodDark, 4);
                foreach (float x in new[] { -0.9f, 0.9f })
                {
                    b.Log(new Vector3(x - 0.05f, 0.42f, 0.1f), new Vector3(x + 0.05f, 0.42f, 0.1f), 0.42f, TownSwatch.WoodDark, 8, TownSwatch.SteppeLeather);
                    b.Log(new Vector3(x * 0.4f, 0.5f, -0.85f), new Vector3(x * 0.3f, 0.05f, -1.6f), 0.04f, TownSwatch.WoodWeathered, 4);
                }

                if ((variant % 2) == 1)
                {
                    b.Box(new Vector3(-0.3f, 0.82f, 0.3f), new Vector3(0.6f, 0.45f, 0.8f), TownSwatch.Felt, 8f, TownSwatch.FeltRed);
                    b.Box(new Vector3(0.35f, 0.82f, 0.2f), new Vector3(0.55f, 0.45f, 0.7f), TownSwatch.FeltBlue, -6f, TownSwatch.Felt);
                    b.Log(new Vector3(-0.6f, 1.15f, -0.4f), new Vector3(0.6f, 1.15f, -0.4f), 0.16f, TownSwatch.FeltRed, 6, TownSwatch.ScythianGold);
                    return;
                }

                b.Cylinder(new Vector3(0f, 0.6f, 0.05f), 0.68f, 0.68f, 0.85f, 10, TownSwatch.Felt);
                b.Cylinder(new Vector3(0f, 0.85f, 0.05f), 0.7f, 0.7f, 0.12f, 10, TownSwatch.FeltRed);
                b.Cylinder(new Vector3(0f, 1.45f, 0.05f), 0.78f, 0.1f, 0.6f, 10, TownSwatch.Felt, TownSwatch.HorseHair);
                b.Box(new Vector3(0f, 0.95f, -0.66f), new Vector3(0.45f, 0.6f, 0.06f), TownSwatch.FeltBlue, 0f);
            });
        }

        /// <summary>
        /// A horsehair standard: a tall pole on a heap of stones, a crossbar with three horsehair tassels, a dyed felt pennant
        /// (Variant 0: red, 1: blue) and a stag of beaten gold on top (the animal style of the Scythian gold).
        /// </summary>
        private static void HorseStandard(TownMeshBuilder b, float s, int variant)
        {
            float r = HorseStandardRadius * s;
            b.Blob(new Vector3(0f, 0.12f * s, 0f), new Vector3(r * 2.6f, 0.3f * s, r * 2.6f), TownSwatch.Stone);
            float h = 3.6f * s;
            b.Log(Vector3.zero, new Vector3(0f, h, 0f), 0.06f * s, TownSwatch.WoodDark, 6);
            b.Log(new Vector3(-0.45f * s, h - 0.6f * s, 0f), new Vector3(0.45f * s, h - 0.6f * s, 0f), 0.035f * s, TownSwatch.WoodDark, 4);
            foreach (float x in new[] { -0.4f, 0f, 0.4f })
            {
                b.Log(new Vector3(x * s, h - 0.62f * s, 0f), new Vector3(x * 1.1f * s, h - 1.5f * s, -0.04f), 0.05f * s, TownSwatch.HorseHair, 4);
            }

            TownSwatch felt = (variant % 2) == 1 ? TownSwatch.FeltBlue : TownSwatch.FeltRed;
            b.BoxMinMax(new Vector3(0.05f * s, h - 1.75f * s, -0.02f), new Vector3(0.8f * s, h - 0.75f * s, 0.02f), felt);
            b.BoxMinMax(new Vector3(0.05f * s, h - 1.0f * s, -0.03f), new Vector3(0.8f * s, h - 0.9f * s, 0.03f), TownSwatch.ScythianGold);

            // The gold stag: body, neck, head and the curling antlers.
            var body = new Vector3(0f, h + 0.18f * s, 0f);
            b.Box(body, new Vector3(0.38f, 0.14f, 0.1f) * s, TownSwatch.ScythianGold, 0f);
            b.Log(body + new Vector3(0.15f, 0.05f, 0f) * s, body + new Vector3(0.24f, 0.22f, 0f) * s, 0.045f * s, TownSwatch.ScythianGold, 4);
            b.Log(body + new Vector3(0.22f, 0.22f, 0f) * s, body + new Vector3(0.12f, 0.45f, 0f) * s, 0.02f * s, TownSwatch.ScythianGold, 3);
            b.Log(body + new Vector3(0.12f, 0.45f, 0f) * s, body + new Vector3(-0.12f, 0.5f, 0f) * s, 0.02f * s, TownSwatch.ScythianGold, 3);
            b.Log(body + new Vector3(-0.15f, -0.06f, 0f) * s, body + new Vector3(-0.1f, -0.18f, 0f) * s, 0.025f * s, TownSwatch.ScythianGold, 3);
            b.Log(body + new Vector3(0.12f, -0.06f, 0f) * s, body + new Vector3(0.08f, -0.18f, 0f) * s, 0.025f * s, TownSwatch.ScythianGold, 3);
        }

        /// <summary>
        /// A rope corral from the origin to <paramref name="d"/> (the mass on the left, local +z): posts every ~2 m with two
        /// rawhide ropes, a hay pile and a hollowed-log trough behind. Its collider is the run inflated by
        /// <see cref="CorralHalfThickness"/>.
        /// </summary>
        private static void RopeCorral(TownMeshBuilder b, Vector2 d, float s)
        {
            float length = d.magnitude;
            if (length < 0.5f)
            {
                return;
            }

            b.With(Vector3.zero, YawAlong(d / length), () =>
            {
                int posts = Mathf.Max(2, Mathf.RoundToInt(length / 2f) + 1);
                for (int i = 0; i < posts; i++)
                {
                    float x = length * i / (posts - 1);
                    b.Log(new Vector3(x, 0f, 0f), new Vector3(x, 1.2f * s, 0f), 0.07f, TownSwatch.WoodDark, 5);
                }

                foreach (float y in new[] { 0.55f, 0.95f })
                {
                    b.Log(new Vector3(0f, y * s, 0f), new Vector3(length * 0.5f, (y - 0.05f) * s, 0f), 0.025f, TownSwatch.SteppeLeather, 4);
                    b.Log(new Vector3(length * 0.5f, (y - 0.05f) * s, 0f), new Vector3(length, y * s, 0f), 0.025f, TownSwatch.SteppeLeather, 4);
                }

                b.Crown(new Vector3(length * 0.3f, 0.35f * s, 0.9f), new Vector3(1.3f, 0.7f, 0.9f) * s, TownSwatch.Straw);
                b.Crown(new Vector3(length * 0.3f + 0.6f, 0.25f * s, 1.1f), new Vector3(0.8f, 0.5f, 0.7f) * s, TownSwatch.SteppeGrass);
                b.BoxMinMax(new Vector3(length * 0.65f, 0f, 0.6f), new Vector3(length * 0.65f + 1.4f, 0.35f * s, 1.0f), TownSwatch.WoodWeathered, TownSwatch.Water);
            });
        }

        /// <summary>
        /// The collective stone oven of the camp: a ring of stones, a dome of stones with a smoke hole, the mouth facing
        /// local −z glowing with the camp fire, a long bread peel leaning on it and a stack of firewood.
        /// </summary>
        private static void StoneOven(TownMeshBuilder b, float s)
        {
            float r = StoneOvenRadius * s;
            b.Cylinder(Vector3.zero, r, r * 0.95f, 0.3f * s, 10, TownSwatch.StoneDark, TownSwatch.Stone);
            b.Cylinder(new Vector3(0f, 0.3f * s, 0f), r * 0.9f, r * 0.7f, 0.6f * s, 10, TownSwatch.Stone);
            b.Cylinder(new Vector3(0f, 0.9f * s, 0f), r * 0.7f, r * 0.25f, 0.55f * s, 10, TownSwatch.Stone, TownSwatch.StoneDark);
            b.Cylinder(new Vector3(0f, 1.45f * s, 0f), 0.18f * s, 0.15f * s, 0.25f * s, 6, TownSwatch.StoneDark, TownSwatch.Black);
            b.BoxMinMax(new Vector3(-0.38f * s, 0.32f * s, -r * 0.9f - 0.05f), new Vector3(0.38f * s, 0.85f * s, -r * 0.6f), TownSwatch.StoneDark);
            b.BoxMinMax(new Vector3(-0.28f * s, 0.34f * s, -r * 0.9f - 0.07f), new Vector3(0.28f * s, 0.72f * s, -r * 0.9f - 0.05f), TownSwatch.CampFire);
            b.Log(new Vector3(r * 0.6f, 0f, -r * 0.85f), new Vector3(r * 0.45f, 1.5f * s, -r * 0.5f), 0.03f * s, TownSwatch.WoodLight, 4);
            b.Box(new Vector3(r * 0.62f, 0.15f * s, -r * 0.9f), new Vector3(0.3f, 0.03f, 0.35f) * s, TownSwatch.WoodLight, 20f);
            for (int i = 0; i < 3; i++)
            {
                float y = (0.12f + i * 0.18f) * s;
                b.Log(new Vector3(-r * 0.95f, y, -0.5f * s + i * 0.05f), new Vector3(-r * 0.95f, y, 0.5f * s), 0.08f * s, TownSwatch.Trunk, 5, TownSwatch.WoodLight);
            }
        }

        // ------------------------------------------------------------------
        // Dressing
        // ------------------------------------------------------------------

        /// <summary>Syrdon's front: a saddle on a trestle under a felt saddle blanket, a stack of folded blankets and a halter on a post.</summary>
        private static void SteppeCampDressing(TownMeshBuilder b, TownBuildingSpec spec, float x, float y, float z, float zFacade)
        {
            foreach (float lx in new[] { -0.45f, 0.45f })
            {
                b.Log(new Vector3(x + lx, y, z - 0.75f), new Vector3(x + lx, y + 0.7f, z - 0.6f), 0.04f, TownSwatch.WoodDark, 4);
                b.Log(new Vector3(x + lx, y, z - 0.45f), new Vector3(x + lx, y + 0.7f, z - 0.6f), 0.04f, TownSwatch.WoodDark, 4);
            }

            b.Log(new Vector3(x - 0.55f, y + 0.7f, z - 0.6f), new Vector3(x + 0.55f, y + 0.7f, z - 0.6f), 0.05f, TownSwatch.WoodDark, 4);
            b.Box(new Vector3(x, y + 0.75f, z - 0.6f), new Vector3(0.7f, 0.06f, 0.55f), TownSwatch.FeltRed, 0f);
            b.Blob(new Vector3(x, y + 0.86f, z - 0.6f), new Vector3(0.55f, 0.2f, 0.4f), TownSwatch.SteppeLeather);
            b.Box(new Vector3(x + 0.22f, y + 0.98f, z - 0.6f), new Vector3(0.08f, 0.12f, 0.16f), TownSwatch.ScythianGold, 0f);

            TownSwatch[] stack = { TownSwatch.FeltBlue, TownSwatch.Felt, TownSwatch.FeltRed };
            for (int i = 0; i < stack.Length; i++)
            {
                b.Box(new Vector3(x - 1.0f, y + 0.08f + i * 0.14f, z - 0.35f), new Vector3(0.6f, 0.13f, 0.45f), stack[i], i * 7f);
            }

            b.Log(new Vector3(x + 1.0f, y, z - 0.3f), new Vector3(x + 1.0f, y + 1.1f, z - 0.3f), 0.04f, TownSwatch.WoodDark, 4);
            b.Log(new Vector3(x + 1.0f, y + 1.05f, z - 0.32f), new Vector3(x + 0.95f, y + 0.6f, z - 0.4f), 0.02f, TownSwatch.HorseHair, 3);
            b.Log(new Vector3(x + 1.0f, y + 1.05f, z - 0.32f), new Vector3(x + 1.08f, y + 0.6f, z - 0.4f), 0.02f, TownSwatch.HorseHair, 3);
        }
    }
}
