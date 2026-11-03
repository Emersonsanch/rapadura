using UnityEngine;

namespace Rapadura.Gameplay.World.Cities.Town
{
    /// <summary>
    /// The pieces of the Fortim dos Gigantes-da-Colina (Docs/Mapas100.md §3.3 and map 49, style <c>HillFort</c>): the round
    /// giant houses (<see cref="GiantRoundhouseHouse"/>: a drum of dry stone on a stone ring, a door as tall as two cousins,
    /// windows lit by the Living Fire, a conical turf roof with the good smoke rising, a porch of two standing stones and a
    /// lintel), the earth ramparts in rings faced with stone with the next ring of the hill behind (one with a round
    /// watch-tower), the giants' stone tables, the pot the size of a house over the Living Fire, the giant spoons the cousins
    /// sit on and the trilithon gate at the streets' ends; Shahmizad's great pot. Same rules as the rest of the kit: flat
    /// colours from <see cref="TownPalette"/> (row 17 holds earth, stone, the good smoke, the Living Fire, hill grass, giant
    /// timber, turf and pot iron), low-poly, deterministic. The giants are penitent and gentle: nothing here is a weapon.
    /// </summary>
    public static partial class TownKit
    {
        /// <summary>Half the thickness of an <see cref="CityPropKind.EarthRing"/>'s stone face: its collider is the run inflated by this.</summary>
        public const float EarthRingHalfThickness = 0.7f;

        /// <summary>Footprint of a <see cref="CityPropKind.GiantTable"/> (× its scale; x along its length): its collider.</summary>
        public static readonly Vector2 GiantTableSize = new Vector2(3.4f, 2.0f);

        /// <summary>Radius of the <see cref="CityPropKind.GiantPot"/> on its stones (× its scale): its collider.</summary>
        public const float GiantPotRadius = 1.5f;

        /// <summary>Footprint of a <see cref="CityPropKind.SpoonBench"/> (× its scale; x along the spoon): its collider.</summary>
        public static readonly Vector2 SpoonBenchSize = new Vector2(3.2f, 0.9f);

        // ------------------------------------------------------------------
        // The round giant house
        // ------------------------------------------------------------------

        /// <summary>
        /// A <see cref="TownBuildingSpec.GiantRoundhouse"/> (local frame, front −z): a stone ring, the drum of dry stone
        /// (<see cref="TownBuildingSpec.WallColor"/>) with two courses of earth (<see cref="TownBuildingSpec.TrimColor"/>), a
        /// giant timber door with iron bands and a stone sill, two small windows lit by the Living Fire, the conical turf roof
        /// (<see cref="TownBuildingSpec.RoofColor"/>) with its grass eave and three puffs of good smoke; in front a flagstone
        /// deck and two standing stones (at <see cref="TownBuildingBuilder.PorchPostsX"/>, the colliders use the same list)
        /// under a stone lintel carrying the sign as a wooden plaque.
        /// </summary>
        public static void GiantRoundhouseHouse(TownMeshBuilder b, TownBuildingSpec spec, float hw, float zFront, float zFacade, float zBack, float top)
        {
            float cz = (zFacade + zBack) * 0.5f;
            float r = Mathf.Max(1.4f, Mathf.Min(hw - 0.2f, (zBack - zFacade) * 0.5f - 0.05f));
            float wallH = Mathf.Max(3.2f, spec.WallHeight);
            float wallTop = wallH;

            b.Cylinder(new Vector3(0f, 0f, cz), r + 0.2f, r + 0.15f, 0.3f, 12, spec.PlinthColor, spec.PlinthColor);
            b.Cylinder(new Vector3(0f, 0.3f, cz), r, r * 0.97f, wallH - 0.3f, 12, spec.WallColor);
            foreach (float y in new[] { wallH * 0.35f, wallH * 0.7f })
            {
                b.Cylinder(new Vector3(0f, y, cz), r * 0.99f + 0.03f, r * 0.985f + 0.03f, 0.16f, 12, spec.TrimColor);
            }

            // The turf roof, its grass eave and the good smoke.
            float roofH = r * 1.05f + 0.6f;
            b.Cylinder(new Vector3(0f, wallTop - 0.1f, cz), r + 0.45f, r + 0.35f, 0.22f, 12, TownSwatch.HillGrass);
            b.Cylinder(new Vector3(0f, wallTop + 0.12f, cz), r + 0.35f, 0f, roofH, 12, spec.RoofColor);
            float apex = wallTop + 0.12f + roofH;
            b.Cylinder(new Vector3(0f, apex - 0.45f, cz), 0.32f, 0.26f, 0.5f, 6, TownSwatch.FortStone, TownSwatch.Black);
            b.Blob(new Vector3(0.1f, apex + 0.35f, cz), new Vector3(0.6f, 0.45f, 0.6f), TownSwatch.GoodSmoke);
            b.Blob(new Vector3(-0.1f, apex + 0.85f, cz + 0.1f), new Vector3(0.75f, 0.55f, 0.75f), TownSwatch.GoodSmoke);
            b.Blob(new Vector3(0.15f, apex + 1.4f, cz + 0.2f), new Vector3(0.9f, 0.6f, 0.9f), TownSwatch.GoodSmoke);

            // The giant door on the front of the drum, its stone sill and the windows of the Living Fire.
            float doorX = Mathf.Clamp(spec.DoorOffset * spec.Width, -r * 0.4f, r * 0.4f);
            float doorW = spec.WideDoor ? 1.9f : 1.5f;
            float doorZ = cz - Mathf.Sqrt(Mathf.Max(0.01f, r * r - doorX * doorX)) - 0.05f;
            float doorTop = Mathf.Min(2.7f, wallH - 0.5f);
            b.BoxMinMax(new Vector3(doorX - doorW * 0.5f - 0.3f, 0f, doorZ - 0.25f), new Vector3(doorX + doorW * 0.5f + 0.3f, 0.32f, doorZ + 0.2f), TownSwatch.FortStone);
            b.BoxMinMax(new Vector3(doorX - doorW * 0.5f - 0.2f, 0.32f, doorZ - 0.1f), new Vector3(doorX + doorW * 0.5f + 0.2f, doorTop + 0.3f, doorZ + 0.15f), TownSwatch.FortStone);
            b.BoxMinMax(new Vector3(doorX - doorW * 0.5f, 0.32f, doorZ - 0.14f), new Vector3(doorX + doorW * 0.5f, doorTop, doorZ - 0.1f), spec.JoineryColor);
            foreach (float y in new[] { 0.9f, doorTop - 0.5f })
            {
                b.BoxMinMax(new Vector3(doorX - doorW * 0.5f, y, doorZ - 0.17f), new Vector3(doorX + doorW * 0.5f, y + 0.1f, doorZ - 0.14f), TownSwatch.PotIron);
            }

            b.Blob(new Vector3(doorX + doorW * 0.3f, 1.4f, doorZ - 0.2f), new Vector3(0.14f, 0.14f, 0.1f), TownSwatch.PotIron);

            int windows = Mathf.Clamp(spec.FrontWindows, 0, 2);
            for (int i = 0; i < windows; i++)
            {
                float side = i == 0 ? -1f : 1f;
                float a = (270f + side * 48f) * Mathf.Deg2Rad;
                float wx = Mathf.Cos(a) * (r + 0.02f);
                float wz = cz + Mathf.Sin(a) * (r + 0.02f);
                if (Mathf.Abs(wx - doorX) < doorW * 0.5f + 0.5f)
                {
                    continue;
                }

                b.Box(new Vector3(wx, wallH * 0.55f, wz), new Vector3(0.55f, 0.6f, 0.12f), TownSwatch.LivingFire, -side * 48f);
                b.Box(new Vector3(wx, wallH * 0.55f + 0.38f, wz), new Vector3(0.8f, 0.16f, 0.2f), TownSwatch.FortStone, -side * 48f);
            }

            if (spec.PorchDepth > 0.01f)
            {
                float deck = CitySolids.DeckHeight;
                b.BoxMinMax(new Vector3(-hw, 0f, zFront), new Vector3(hw, deck, zFacade), TownSwatch.FortStone, TownSwatch.FortEarth);
                float z = zFront + TownBuildingBuilder.PostInset;
                float lintel = Mathf.Min(3.2f, wallH - 0.2f);
                foreach (float x in TownBuildingBuilder.PorchPostsX(spec))
                {
                    b.Box(new Vector3(x, (deck + lintel) * 0.5f, z), new Vector3(0.5f, lintel - deck, 0.5f), TownSwatch.FortStone, 4f, TownSwatch.FortStone);
                }

                b.BoxMinMax(new Vector3(-hw - 0.05f, lintel, z - 0.32f), new Vector3(hw + 0.05f, lintel + 0.45f, z + 0.32f), TownSwatch.FortStone, TownSwatch.HillGrass);
                if (spec.Sign)
                {
                    var at = new Vector3(doorX, lintel + 0.22f, z - 0.36f);
                    b.Box(at, new Vector3(1.6f, 0.34f, 0.06f), TownSwatch.GiantWood, 0f);
                    b.Box(at + new Vector3(0f, 0f, -0.04f), new Vector3(1.4f, 0.22f, 0.02f), spec.SignColor, 0f);
                }
            }
        }

        /// <summary>
        /// The trilithon gate of the Fortim at a street's end: two standing stones twice a cousin's height on stone feet, a
        /// lintel across them with turf on top and a lamp of the Living Fire hanging in the middle.
        /// </summary>
        public static void TrilithonGate(TownMeshBuilder b, Vector3 a, Vector3 c)
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
                    b.Blob(new Vector3(x, 0.15f, 0f), new Vector3(1.4f, 0.4f, 1.3f), TownSwatch.FortEarth);
                    b.Box(new Vector3(x, 1.85f, 0f), new Vector3(0.9f, 3.7f, 0.8f), TownSwatch.FortStone, x > 0f ? 4f : -3f);
                }

                b.BoxMinMax(new Vector3(-0.55f, 3.7f, -0.5f), new Vector3(w + 0.55f, 4.3f, 0.5f), TownSwatch.FortStone, TownSwatch.HillGrass);
                float mid = w * 0.5f;
                b.Log(new Vector3(mid, 3.7f, 0f), new Vector3(mid, 3.2f, 0f), 0.03f, TownSwatch.PotIron, 3);
                b.Cylinder(new Vector3(mid, 2.85f, 0f), 0.22f, 0.26f, 0.35f, 6, TownSwatch.PotIron, TownSwatch.LivingFire);
            });
        }

        // ------------------------------------------------------------------
        // Props
        // ------------------------------------------------------------------

        /// <summary>
        /// An earth rampart from the origin to <paramref name="d"/> (the mass on the left, local +z): a stone face with a grass
        /// top (its collider is the run inflated by <see cref="EarthRingHalfThickness"/>), the earth bank behind it and the next
        /// ring of the hill higher up with the turf roofs of round houses peeking over. Variant 1 raises a round stone
        /// watch-tower with a turf cap and a lit window in the middle of the run.
        /// </summary>
        private static void EarthRing(TownMeshBuilder b, Vector2 d, float s, int variant)
        {
            float length = d.magnitude;
            if (length < 0.5f)
            {
                return;
            }

            string key = "ring" + Mathf.RoundToInt(length * 10f) + "_" + variant;
            b.With(Vector3.zero, YawAlong(d / length), () =>
            {
                float h = EarthRingHalfThickness;
                b.BoxMinMax(new Vector3(-0.3f, 0f, -h), new Vector3(length + 0.3f, 1.7f * s, h), TownSwatch.FortStone, TownSwatch.HillGrass);
                for (int i = 0; i < Mathf.RoundToInt(length / 1.6f); i++)
                {
                    float x = 0.5f + i * 1.6f + 0.4f * TownStyle.Hash01(key, i);
                    b.Box(new Vector3(x, 0.9f * s, -h - 0.02f), new Vector3(0.9f, 0.08f, 0.05f), TownSwatch.FortEarth, 0f);
                }

                b.BoxMinMax(new Vector3(-0.5f, 0f, h), new Vector3(length + 0.5f, 2.6f * s, h + 1.6f), TownSwatch.FortEarth, TownSwatch.HillGrass);
                b.BoxMinMax(new Vector3(-0.7f, 0f, h + 1.6f), new Vector3(length + 0.7f, 3.6f * s, h + 3.4f), TownSwatch.FortStone, TownSwatch.HillGrass);
                int roofs = Mathf.Max(1, Mathf.RoundToInt(length / 5f));
                for (int k = 0; k < roofs; k++)
                {
                    float x = Mathf.Lerp(0.8f, length - 0.8f, (k + 0.5f) / roofs) + 0.6f * (TownStyle.Hash01(key, k + 20) - 0.5f);
                    if (variant == 1 && Mathf.Abs(x - length * 0.5f) < 2.2f)
                    {
                        continue;
                    }

                    b.Cylinder(new Vector3(x, 3.6f * s, h + 2.6f), 1.3f, 1.25f, 0.7f, 8, TownSwatch.FortStone);
                    b.Cylinder(new Vector3(x, 4.3f * s, h + 2.6f), 1.5f, 0f, 1.5f, 8, TownSwatch.TurfRoof);
                }

                if (variant == 1)
                {
                    float mid = length * 0.5f;
                    b.Cylinder(new Vector3(mid, 0f, h + 0.9f), 1.2f, 1.05f, 5.2f * s, 10, TownSwatch.FortStone);
                    b.Cylinder(new Vector3(mid, 2.6f * s, h + 0.9f), 1.18f, 1.14f, 0.14f, 10, TownSwatch.FortEarth);
                    b.Cylinder(new Vector3(mid, 5.2f * s, h + 0.9f), 1.35f, 0f, 1.6f, 10, TownSwatch.TurfRoof);
                    b.Box(new Vector3(mid, 4.0f * s, h + 0.9f - 1.08f), new Vector3(0.45f, 0.55f, 0.1f), TownSwatch.LivingFire, 0f);
                }
            });
        }

        /// <summary>A giants' stone table: a slab on two stone legs, a wooden bowl and a cup each as big as a cousin.</summary>
        private static void GiantTable(TownMeshBuilder b, float s)
        {
            float hx = GiantTableSize.x * 0.5f * s;
            float hz = GiantTableSize.y * 0.5f * s;
            float top = 1.6f * s;
            foreach (float x in new[] { -hx * 0.6f, hx * 0.6f })
            {
                b.Box(new Vector3(x, (top - 0.3f * s) * 0.5f, 0f), new Vector3(0.6f * s, top - 0.3f * s, hz * 1.4f), TownSwatch.FortStone, 0f);
            }

            b.BoxMinMax(new Vector3(-hx, top - 0.3f * s, -hz), new Vector3(hx, top, hz), TownSwatch.FortStone, TownSwatch.FortStone);
            b.Cylinder(new Vector3(-hx * 0.35f, top, 0f), 0.45f * s, 0.7f * s, 0.45f * s, 10, TownSwatch.GiantWood, TownSwatch.FortEarth);
            b.Cylinder(new Vector3(hx * 0.45f, top, hz * 0.2f), 0.28f * s, 0.32f * s, 0.75f * s, 8, TownSwatch.PotIron, TownSwatch.GoodSmoke);
        }

        /// <summary>
        /// The pot the size of a house over the Living Fire: three stones, the fire glowing under the iron belly, the rim, the
        /// soup, a long ladle and the good smoke rising.
        /// </summary>
        private static void GiantPot(TownMeshBuilder b, float s)
        {
            float r = GiantPotRadius * s;
            for (int i = 0; i < 3; i++)
            {
                float a = (i * 120f + 30f) * Mathf.Deg2Rad;
                b.Blob(new Vector3(Mathf.Cos(a) * r * 0.75f, 0.3f * s, Mathf.Sin(a) * r * 0.75f), new Vector3(0.7f, 0.65f, 0.7f) * s, TownSwatch.FortStone);
            }

            b.Crown(new Vector3(0f, 0.25f * s, 0f), new Vector3(1.1f, 0.5f, 1.1f) * s, TownSwatch.LivingFire);
            b.Cylinder(new Vector3(0f, 0.45f * s, 0f), r * 0.6f, r * 0.95f, 0.6f * s, 12, TownSwatch.PotIron);
            b.Cylinder(new Vector3(0f, 1.05f * s, 0f), r * 0.95f, r * 0.9f, 0.9f * s, 12, TownSwatch.PotIron);
            b.Ring(new Vector3(0f, 1.95f * s, 0f), r * 0.82f, r * 0.95f, 12, TownSwatch.PotIron);
            b.Disc(new Vector3(0f, 1.85f * s, 0f), r * 0.83f, 12, TownSwatch.FortEarth);
            b.Log(new Vector3(-r * 0.3f, 1.7f * s, 0f), new Vector3(-r * 1.1f, 3.1f * s, -0.2f * s), 0.07f * s, TownSwatch.GiantWood, 5);
            b.Blob(new Vector3(0.2f * s, 2.5f * s, 0.1f * s), new Vector3(1.0f, 0.6f, 1.0f) * s, TownSwatch.GoodSmoke);
            b.Blob(new Vector3(-0.1f * s, 3.2f * s, 0.25f * s), new Vector3(1.2f, 0.7f, 1.2f) * s, TownSwatch.GoodSmoke);
        }

        /// <summary>A giant wooden spoon lying on two stones along local x (the bowl at +x): the cousins' bench.</summary>
        private static void SpoonBench(TownMeshBuilder b, float s)
        {
            float hx = SpoonBenchSize.x * 0.5f * s;
            b.Blob(new Vector3(-hx * 0.55f, 0.18f * s, 0f), new Vector3(0.6f, 0.36f, 0.6f) * s, TownSwatch.FortStone);
            b.Blob(new Vector3(hx * 0.45f, 0.2f * s, 0f), new Vector3(0.7f, 0.4f, 0.7f) * s, TownSwatch.FortStone);
            b.Log(new Vector3(-hx, 0.5f * s, 0f), new Vector3(hx * 0.2f, 0.45f * s, 0f), 0.13f * s, TownSwatch.GiantWood, 6);
            b.Blob(new Vector3(hx * 0.6f, 0.48f * s, 0f), new Vector3(1.3f, 0.32f, 0.85f) * s, TownSwatch.GiantWood);
            b.Disc(new Vector3(hx * 0.6f, 0.6f * s, 0f), 0.36f * s, 8, TownSwatch.WoodDark);
        }

        // ------------------------------------------------------------------
        // Dressing
        // ------------------------------------------------------------------

        /// <summary>Shahmizad's front: a ladle as tall as a cousin leaning on the wall, a stack of wooden bowls, a sack of grain and a little pot on its fire.</summary>
        private static void HillFortDressing(TownMeshBuilder b, TownBuildingSpec spec, float x, float y, float z, float zFacade)
        {
            b.Log(new Vector3(x - 0.9f, y, z - 0.6f), new Vector3(x - 0.75f, y + 1.7f, zFacade - 0.05f), 0.05f, TownSwatch.GiantWood, 5);
            b.Blob(new Vector3(x - 0.92f, y + 0.18f, z - 0.62f), new Vector3(0.45f, 0.3f, 0.4f), TownSwatch.GiantWood);
            for (int i = 0; i < 4; i++)
            {
                b.Cylinder(new Vector3(x - 0.2f, y + i * 0.12f, z - 0.55f), 0.2f, 0.28f, 0.11f, 8, TownSwatch.GiantWood, TownSwatch.WoodDark);
            }

            Sack(b, new Vector3(x + 0.35f, y, z - 0.3f), TownSwatch.GoodSmoke);
            b.Crown(new Vector3(x + 0.9f, y + 0.1f, z - 0.8f), new Vector3(0.35f, 0.2f, 0.35f), TownSwatch.LivingFire);
            b.Cylinder(new Vector3(x + 0.9f, y + 0.2f, z - 0.8f), 0.18f, 0.24f, 0.3f, 8, TownSwatch.PotIron, TownSwatch.FortEarth);
            b.Blob(new Vector3(x + 0.9f, y + 0.75f, z - 0.8f), new Vector3(0.3f, 0.3f, 0.3f), TownSwatch.GoodSmoke);
        }
    }
}
