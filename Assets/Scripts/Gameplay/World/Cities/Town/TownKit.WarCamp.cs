using UnityEngine;

namespace Rapadura.Gameplay.World.Cities.Town
{
    /// <summary>
    /// The pieces of the Tambor Rubro, the orcs' war camp in the Ermos do Tambor Rubro (Docs/Mapas100.md §3.3 and map 11):
    /// the log halls (<see cref="LogHallWalls"/>: thick corner posts, log sill and plate, walls of ochre-dyed hide laced to
    /// battens; <see cref="LogHallRoof"/>: a very steep hide gable whose ridge poles cross above both gable ends), the
    /// palisade of sharpened logs (points up, never trophies), the drum tower with the city's guardian lamp on top, clan
    /// tents of dyed hide, clan posts with cloth bands, fire pits, great war drums, the sand training ring and Kessi's
    /// hunting rack. Same rules as the rest of the kit: flat colours from <see cref="TownPalette"/> (the ficha's palette
    /// maps onto red clay, laterite, adobe ochre, teak, iron, ember and cloth yellow), low-poly, deterministic.
    /// </summary>
    public static partial class TownKit
    {
        /// <summary>Side of a <see cref="CityPropKind.DrumTower"/>'s square footprint (× its scale): its collider box.</summary>
        public const float DrumTowerSize = 2.4f;

        /// <summary>Side of a <see cref="CityPropKind.ClanTent"/>'s square footprint (× its scale): its collider box.</summary>
        public const float ClanTentSize = 3.0f;

        /// <summary>Half the thickness of a <see cref="CityPropKind.Palisade"/> run (logs and braces): its collider is the run inflated by this.</summary>
        public const float PalisadeHalfThickness = 0.3f;

        /// <summary>Radius of the sand of a <see cref="CityPropKind.TrainingRing"/> (× its scale).</summary>
        public const float TrainingRingRadius = 5f;

        // ------------------------------------------------------------------
        // The log hall
        // ------------------------------------------------------------------

        /// <summary>
        /// Over the mass of a <see cref="TownBuildingSpec.LogHall"/> house (local frame, front −z, facade at
        /// <paramref name="z0"/>): corner posts that stand proud of the roofline, a log sill and a log plate all round,
        /// vertical battens with the hide lacing on the front and the back, and a yellow cloth band hung beside the door.
        /// </summary>
        public static void LogHallWalls(TownMeshBuilder b, TownBuildingSpec spec, float x0, float x1, float z0, float z1, float top)
        {
            TownSwatch log = spec.JoineryColor == TownSwatch.Beam ? TownSwatch.WoodDark : spec.JoineryColor;
            float y0 = TownBuildingBuilder.FloorY;
            foreach (Vector2 c in new[] { new Vector2(x0, z0), new Vector2(x1, z0), new Vector2(x0, z1), new Vector2(x1, z1) })
            {
                b.Log(new Vector3(c.x, 0f, c.y), new Vector3(c.x, top + 0.35f, c.y), 0.2f, log, 6, TownSwatch.WoodLight);
            }

            // Sill and plate logs on the four sides.
            foreach (float y in new[] { y0 + 0.12f, top - 0.1f })
            {
                b.Log(new Vector3(x0, y, z0 - 0.04f), new Vector3(x1, y, z0 - 0.04f), 0.12f, log, 5);
                b.Log(new Vector3(x0, y, z1 + 0.04f), new Vector3(x1, y, z1 + 0.04f), 0.12f, log, 5);
                b.Log(new Vector3(x0 - 0.04f, y, z0), new Vector3(x0 - 0.04f, y, z1), 0.12f, log, 5);
                b.Log(new Vector3(x1 + 0.04f, y, z0), new Vector3(x1 + 0.04f, y, z1), 0.12f, log, 5);
            }

            // Battens and lacing on the front (skipping the door) and the back.
            float doorX = spec.DoorOffset * spec.Width;
            float doorHalf = (spec.WideDoor ? 1.7f : 1.05f) * 0.5f + 0.25f;
            for (float x = x0 + 1.1f; x < x1 - 0.5f; x += 1.1f)
            {
                if (Mathf.Abs(x - doorX) > doorHalf)
                {
                    b.BoxMinMax(new Vector3(x - 0.05f, y0 + 0.2f, z0 - 0.05f), new Vector3(x + 0.05f, top - 0.18f, z0), log);
                    for (float y = y0 + 0.6f; y < top - 0.4f; y += 0.55f)
                    {
                        b.Box(new Vector3(x, y, z0 - 0.06f), new Vector3(0.26f, 0.04f, 0.02f), TownSwatch.Thatch, 30f);
                    }
                }

                b.BoxMinMax(new Vector3(x - 0.05f, y0 + 0.2f, z1), new Vector3(x + 0.05f, top - 0.18f, z1 + 0.05f), log);
            }

            // The clan's yellow cloth band, hung high on the side away from the keeper.
            float away = -TownBuildingBuilder.KeeperSide(spec);
            float bx = Mathf.Clamp(doorX + away * (doorHalf + 0.55f), x0 + 0.45f, x1 - 0.45f);
            b.BoxMinMax(new Vector3(bx - 0.28f, top - 1.35f, z0 - 0.08f), new Vector3(bx + 0.28f, top - 0.25f, z0 - 0.05f), spec.AccentColor);
            b.BoxMinMax(new Vector3(bx - 0.28f, top - 0.95f, z0 - 0.09f), new Vector3(bx + 0.28f, top - 0.85f, z0 - 0.08f), TownSwatch.RedClay);
        }

        /// <summary>
        /// The very steep hide gable of a log hall (ridge along local x): two hide slopes with lacing ribs, a ridge log,
        /// and at both gable ends a pair of ridge poles crossing above the ridge (the war camp's silhouette); hide gable
        /// triangles with a small smoke vent.
        /// </summary>
        public static void LogHallRoof(TownMeshBuilder b, TownBuildingSpec spec, float x0, float x1, float z0, float z1, float eave, TownSwatch wall)
        {
            const float o = 0.4f;
            float zm = (z0 + z1) * 0.5f;
            float rise = Mathf.Clamp((z1 - z0) * 0.55f, 2.2f, 4.4f);
            float ridge = eave + rise;
            TownSwatch hide = spec.RoofColor;
            TownSwatch rib = spec.JoineryColor == TownSwatch.Teak ? TownSwatch.WoodDark : TownSwatch.Teak;
            var front = new[] { new Vector3(x0 - o, eave - 0.2f, z0 - o), new Vector3(x0 - o, ridge, zm), new Vector3(x1 + o, ridge, zm), new Vector3(x1 + o, eave - 0.2f, z0 - o) };
            var back = new[] { new Vector3(x0 - o, ridge, zm), new Vector3(x0 - o, eave - 0.2f, z1 + o), new Vector3(x1 + o, eave - 0.2f, z1 + o), new Vector3(x1 + o, ridge, zm) };
            b.Plate(front, 0.12f, hide, TownSwatch.WoodDark);
            b.Plate(back, 0.12f, hide, TownSwatch.WoodDark);

            // Lacing ribs down each slope.
            for (float x = x0; x <= x1 + 0.01f; x += Mathf.Max(1f, (x1 - x0) / Mathf.Max(1, Mathf.RoundToInt((x1 - x0) / 1.3f))))
            {
                b.Log(new Vector3(x, eave - 0.12f, z0 - o + 0.02f), new Vector3(x, ridge + 0.05f, zm), 0.045f, rib, 4);
                b.Log(new Vector3(x, ridge + 0.05f, zm), new Vector3(x, eave - 0.12f, z1 + o - 0.02f), 0.045f, rib, 4);
            }

            b.Log(new Vector3(x0 - o - 0.1f, ridge + 0.06f, zm), new Vector3(x1 + o + 0.1f, ridge + 0.06f, zm), 0.13f, TownSwatch.WoodDark, 6);
            foreach (float x in new[] { x0 - o, x1 + o })
            {
                // Crossed ridge poles: from the eaves, crossing at the ridge and standing 1 m above it.
                float dz = rise * 0.42f;
                b.Log(new Vector3(x, ridge - rise * 0.38f, zm - dz), new Vector3(x, ridge + 1.0f, zm + 0.55f), 0.07f, rib, 5);
                b.Log(new Vector3(x, ridge - rise * 0.38f, zm + dz), new Vector3(x, ridge + 1.0f, zm - 0.55f), 0.07f, rib, 5);
                b.Blob(new Vector3(x, ridge + 1.05f, zm + 0.57f), new Vector3(0.14f, 0.14f, 0.14f), spec.AccentColor);
                b.Blob(new Vector3(x, ridge + 1.05f, zm - 0.57f), new Vector3(0.14f, 0.14f, 0.14f), spec.AccentColor);
            }

            foreach (float x in new[] { x0, x1 })
            {
                var facing = new Vector3(x < (x0 + x1) * 0.5f ? -1f : 1f, 0f, 0f);
                b.TriangleFacing(new Vector3(x, eave, z0), new Vector3(x, ridge - 0.1f, zm), new Vector3(x, eave, z1), facing, wall);
                b.Box(new Vector3(x + facing.x * 0.03f, eave + rise * 0.45f, zm), new Vector3(0.04f, 0.45f, 0.45f), TownSwatch.WoodDark);
            }
        }

        /// <summary>Porch posts of a log hall: plain logs at <see cref="TownBuildingBuilder.PorchPostsX"/> (the colliders use the same list) with a cloth band and a beam log.</summary>
        public static void LogHallPosts(TownMeshBuilder b, TownBuildingSpec spec, float z, float y0, float y1)
        {
            TownSwatch log = spec.JoineryColor == TownSwatch.Beam ? TownSwatch.WoodDark : spec.JoineryColor;
            float foot = y0 - TownBuildingBuilder.FloorY + CitySolids.DeckHeight;
            float hw = spec.Width * 0.5f;
            foreach (float x in TownBuildingBuilder.PorchPostsX(spec))
            {
                b.Log(new Vector3(x, foot, z), new Vector3(x, y1, z), CitySolids.PostRadius, log, 6);
                b.Cylinder(new Vector3(x, foot + 1.3f, z), CitySolids.PostRadius + 0.02f, CitySolids.PostRadius + 0.02f, 0.18f, 6, spec.AccentColor, spec.AccentColor);
            }

            b.Log(new Vector3(-hw - 0.1f, y1 - 0.05f, z), new Vector3(hw + 0.1f, y1 - 0.05f, z), 0.12f, log, 5);
        }

        /// <summary>The town gate of the war camp: two tall log posts with sharpened tops, a double crossbeam, a yellow banner and two torches.</summary>
        public static void WarCampGate(TownMeshBuilder b, Vector3 a, Vector3 c)
        {
            Vector3 along = (c - a).normalized;
            foreach (Vector3 p in new[] { a, c })
            {
                b.Log(p, p + Vector3.up * 3.7f, 0.2f, TownSwatch.WoodDark, 6);
                b.Cylinder(p + Vector3.up * 3.7f, 0.2f, 0f, 0.5f, 6, TownSwatch.WoodLight);
                b.Cylinder(p + Vector3.up * 1.4f, 0.22f, 0.22f, 0.2f, 6, TownSwatch.ClothYellow, TownSwatch.ClothYellow);
                // A torch on the inner side of each post.
                Vector3 inward = p == a ? along : -along;
                Vector3 torch = p + inward * 0.3f + Vector3.up * 2.3f;
                b.Log(torch - Vector3.up * 0.35f, torch, 0.04f, TownSwatch.WoodDark, 4);
                b.Cylinder(torch, 0.07f, 0f, 0.28f, 5, TownSwatch.Ember);
            }

            b.Log(a + Vector3.up * 3.2f - along * 0.4f, c + Vector3.up * 3.2f + along * 0.4f, 0.15f, TownSwatch.Teak, 6);
            b.Log(a + Vector3.up * 2.85f, c + Vector3.up * 2.85f, 0.1f, TownSwatch.WoodDark, 5);
            Vector3 mid = (a + c) * 0.5f;
            Vector3 n = Vector3.Cross(along, Vector3.up) * 0.05f;
            b.Quad(mid - along * 0.45f + Vector3.up * 3.05f + n, mid + along * 0.45f + Vector3.up * 3.05f + n, mid + along * 0.45f + Vector3.up * 2.45f + n, mid - along * 0.45f + Vector3.up * 2.45f + n, TownSwatch.ClothYellow);
            b.Quad(mid - along * 0.45f + Vector3.up * 3.05f - n, mid - along * 0.45f + Vector3.up * 2.45f - n, mid + along * 0.45f + Vector3.up * 2.45f - n, mid + along * 0.45f + Vector3.up * 3.05f - n, TownSwatch.ClothYellow);
        }

        // ------------------------------------------------------------------
        // Props
        // ------------------------------------------------------------------

        /// <summary>
        /// A palisade run from the origin to <paramref name="d"/>: upright logs of slightly varying height with sharpened
        /// tips (deterministic by index), two brace logs on the inside and a band of yellow cloth tied every few logs.
        /// </summary>
        private static void Palisade(TownMeshBuilder b, Vector2 d, float s)
        {
            float length = d.magnitude;
            if (length < 0.2f)
            {
                return;
            }

            Vector2 u = d / length;
            const float spacing = 0.42f;
            const float radius = 0.2f;
            int count = Mathf.Max(2, Mathf.RoundToInt(length / spacing) + 1);
            for (int i = 0; i < count; i++)
            {
                Vector2 p = u * (length * i / (count - 1));
                float h = (2.2f + ((i * 7) % 5) * 0.09f) * s;
                var foot = new Vector3(p.x, 0f, p.y);
                b.Cylinder(foot, radius, radius * 0.92f, h, 5, (i % 5) == 2 ? TownSwatch.Teak : TownSwatch.WoodDark);
                b.Cylinder(foot + Vector3.up * h, radius * 0.92f, 0f, 0.45f * s, 5, TownSwatch.WoodLight);
                if (i % 6 == 3)
                {
                    b.Cylinder(foot + Vector3.up * (1.35f * s), radius + 0.02f, radius + 0.02f, 0.22f, 5, TownSwatch.ClothYellow, TownSwatch.ClothYellow);
                }
            }

            // Braces on both faces (the camera sees the outside of one side and the inside of the other).
            var n = new Vector2(-u.y, u.x) * (radius + 0.06f);
            foreach (float y in new[] { 0.55f, 1.75f })
            {
                foreach (float side in new[] { -1f, 1f })
                {
                    Vector2 off = n * side;
                    b.Log(new Vector3(off.x, y * s, off.y), new Vector3(d.x + off.x, y * s, d.y + off.y), 0.07f, TownSwatch.Teak, 4);
                }
            }
        }

        /// <summary>
        /// The drum tower (Ashmunikal's): four corner logs with X braces, a ladder on the front (−z), a mid deck, the top
        /// platform with its rail, the great drum under a steep four-sided hide roof, and the city's guardian lamp in an iron
        /// cage on the roof's peak (glows at night).
        /// </summary>
        private static void DrumTower(TownMeshBuilder b, float s)
        {
            float h = DrumTowerSize * 0.5f * s;
            float c = h - 0.2f * s;
            float deck = 4.4f * s;
            var corners = new[] { new Vector2(-c, -c), new Vector2(c, -c), new Vector2(c, c), new Vector2(-c, c) };
            foreach (Vector2 p in corners)
            {
                b.Log(new Vector3(p.x, 0f, p.y), new Vector3(p.x, deck + 1.9f * s, p.y), 0.18f * s, TownSwatch.WoodDark, 6);
            }

            // X braces on the four faces, below the deck.
            for (int i = 0; i < 4; i++)
            {
                Vector2 a = corners[i];
                Vector2 e = corners[(i + 1) % 4];
                b.Log(new Vector3(a.x, 0.4f * s, a.y), new Vector3(e.x, deck - 0.3f * s, e.y), 0.06f * s, TownSwatch.Teak, 4);
                b.Log(new Vector3(e.x, 0.4f * s, e.y), new Vector3(a.x, deck - 0.3f * s, a.y), 0.06f * s, TownSwatch.Teak, 4);
            }

            // Mid deck and top platform with a rail.
            b.BoxMinMax(new Vector3(-h + 0.05f, 2.2f * s, -h + 0.05f), new Vector3(h - 0.05f, 2.32f * s, h - 0.05f), TownSwatch.WoodLight, TownSwatch.Beam);
            b.BoxMinMax(new Vector3(-h - 0.2f, deck, -h - 0.2f), new Vector3(h + 0.2f, deck + 0.16f * s, h + 0.2f), TownSwatch.WoodLight, TownSwatch.Beam);
            for (int i = 0; i < 4; i++)
            {
                Vector2 a = corners[i] * 1.12f;
                Vector2 e = corners[(i + 1) % 4] * 1.12f;
                b.Log(new Vector3(a.x, deck + 0.75f * s, a.y), new Vector3(e.x, deck + 0.75f * s, e.y), 0.05f * s, TownSwatch.WoodDark, 4);
                b.Box(new Vector3((a.x + e.x) * 0.5f, deck + 0.5f * s, (a.y + e.y) * 0.5f), new Vector3(0.5f, 0.42f, 0.5f) * s, TownSwatch.ClothYellow, Mathf.Atan2(e.x - a.x, e.y - a.y) * Mathf.Rad2Deg);
            }

            // Ladder up the front face.
            float zl = -h - 0.15f;
            foreach (float x in new[] { -0.25f * s, 0.25f * s })
            {
                b.Log(new Vector3(x, 0f, zl - 0.25f), new Vector3(x, deck + 0.2f, zl), 0.035f, TownSwatch.WoodWeathered, 4);
            }

            for (int i = 1; i < 12; i++)
            {
                float t = i / 12f;
                b.Box(new Vector3(0f, Mathf.Lerp(0f, deck + 0.2f, t), Mathf.Lerp(zl - 0.25f, zl, t)), new Vector3(0.5f * s, 0.04f, 0.05f), TownSwatch.WoodWeathered);
            }

            // The great drum on the platform: a wooden shell with iron hoops and a pale hide head.
            var drum = new Vector3(0f, deck + 0.16f * s, 0f);
            b.Cylinder(drum, 0.62f * s, 0.7f * s, 0.85f * s, 10, TownSwatch.Laterite, TownSwatch.Sand);
            foreach (float t in new[] { 0.15f, 0.75f })
            {
                b.Cylinder(drum + Vector3.up * (0.85f * t * s), 0.66f * s, 0.68f * s, 0.06f * s, 10, TownSwatch.Iron, TownSwatch.Iron);
            }

            // Steep four-sided hide roof, and the guardian lamp in its iron cage on the peak.
            const float root2 = 1.4142f;
            float roofBase = deck + 1.9f * s;
            b.Cylinder(new Vector3(0f, roofBase, 0f), (h + 0.35f) * root2, 0.12f, 1.5f * s, 4, TownSwatch.Teak, TownSwatch.Teak, 0.5f);
            var lamp = new Vector3(0f, roofBase + 1.5f * s, 0f);
            b.Log(lamp, lamp + Vector3.up * 0.35f * s, 0.05f, TownSwatch.Iron, 4);
            b.Box(lamp + Vector3.up * 0.65f * s, new Vector3(0.42f, 0.5f, 0.42f) * s, TownSwatch.LampGlow);
            foreach (Vector2 p in new[] { new Vector2(-1, -1), new Vector2(1, -1), new Vector2(1, 1), new Vector2(-1, 1) })
            {
                b.Log(lamp + new Vector3(p.x * 0.24f * s, 0.35f * s, p.y * 0.24f * s), lamp + new Vector3(p.x * 0.24f * s, 0.95f * s, p.y * 0.24f * s), 0.025f, TownSwatch.Iron, 4);
            }

            b.Cylinder(lamp + Vector3.up * 0.95f * s, 0.32f * s, 0f, 0.3f * s, 4, TownSwatch.Iron, null, 0.5f);
        }

        /// <summary>
        /// A clan tent of dyed hide (variant: 0 laterite, 1 red clay, 2 adobe ochre): a ridge tent on two A-frames, ridge
        /// along local z, the front flap (−z) tied open, a low hide wall at the foot of the slopes, a banner pole beside it.
        /// </summary>
        private static void ClanTent(TownMeshBuilder b, float s, int variant)
        {
            TownSwatch hide = (variant % 3) == 0 ? TownSwatch.Laterite : (variant % 3) == 1 ? TownSwatch.RedClay : TownSwatch.AdobeOchre;
            float hw = ClanTentSize * 0.5f * s - 0.1f;
            float hd = ClanTentSize * 0.5f * s - 0.1f;
            float ridge = 2.3f * s;
            float foot = 0.35f * s;
            float inner = hw * 0.18f;

            // The slopes (hide) and the low walls at their feet.
            foreach (float side in new[] { -1f, 1f })
            {
                var slope = new[]
                {
                    new Vector3(side * inner, ridge, -hd), new Vector3(side * hw, foot, -hd), new Vector3(side * hw, foot, hd), new Vector3(side * inner, ridge, hd),
                };
                b.Plate(slope, 0.06f, hide, TownSwatch.Thatch);
                b.BoxMinMax(new Vector3(side > 0f ? hw - 0.06f : -hw, 0f, -hd), new Vector3(side > 0f ? hw : -hw + 0.06f, foot, hd), hide);
            }

            b.Plate(new[] { new Vector3(-inner, ridge, -hd), new Vector3(inner, ridge, -hd), new Vector3(inner, ridge, hd), new Vector3(-inner, ridge, hd) }, 0.06f, hide, TownSwatch.Thatch);

            // Back closed; the front flaps folded aside, the dark doorway between them.
            b.TriangleFacing(new Vector3(-hw, 0f, hd), new Vector3(0f, ridge, hd), new Vector3(hw, 0f, hd), Vector3.forward, hide);
            b.QuadFacing(new Vector3(-hw * 0.45f, 0f, -hd + 0.05f), new Vector3(-hw * 0.12f, ridge * 0.82f, -hd + 0.05f), new Vector3(hw * 0.12f, ridge * 0.82f, -hd + 0.05f), new Vector3(hw * 0.45f, 0f, -hd + 0.05f), Vector3.back, TownSwatch.Black);
            foreach (float side in new[] { -1f, 1f })
            {
                b.TriangleFacing(new Vector3(side * hw, 0f, -hd), new Vector3(side * inner, ridge, -hd), new Vector3(side * hw * 0.45f, 0f, -hd - 0.02f), Vector3.back, hide);
                b.Box(new Vector3(side * hw * 0.5f, ridge * 0.42f, -hd - 0.08f), new Vector3(0.3f * s, 0.7f * s, 0.12f), hide, side * 15f);
            }

            // A-frames, the ridge pole and its cloth stripe.
            foreach (float z in new[] { -hd - 0.05f, hd + 0.05f })
            {
                foreach (float side in new[] { -1f, 1f })
                {
                    b.Log(new Vector3(side * hw * 0.85f, 0f, z), new Vector3(-side * 0.15f, ridge + 0.45f * s, z), 0.05f, TownSwatch.WoodDark, 4);
                }
            }

            b.Log(new Vector3(0f, ridge + 0.05f, -hd - 0.15f), new Vector3(0f, ridge + 0.05f, hd + 0.15f), 0.06f, TownSwatch.WoodDark, 5);
            b.BoxMinMax(new Vector3(-inner - 0.02f, ridge * 0.62f, -hd - 0.03f), new Vector3(inner + 0.02f, ridge * 0.7f, hd + 0.03f), TownSwatch.ClothYellow);

            // The clan's banner pole at the front corner.
            var pole = new Vector3(hw + 0.15f, 0f, -hd + 0.2f);
            b.Log(pole, pole + Vector3.up * 3.0f * s, 0.05f, TownSwatch.WoodDark, 4);
            b.Box(pole + new Vector3(0.3f * s, 2.55f * s, 0f), new Vector3(0.6f * s, 0.75f * s, 0.03f), (variant & 1) == 0 ? TownSwatch.ClothYellow : TownSwatch.RedClay);
        }

        /// <summary>A clan post: a thick carved log with cloth bands, a small drum hung on it and a crossbar with two banners. The clan's sign, never a shrine.</summary>
        private static void ClanPost(TownMeshBuilder b, float s, int variant)
        {
            float h = 2.8f * s;
            b.Cylinder(Vector3.zero, 0.24f * s, 0.2f * s, h, 7, TownSwatch.Teak, TownSwatch.WoodDark);
            b.Cylinder(Vector3.up * h, 0.2f * s, 0f, 0.35f * s, 7, TownSwatch.WoodDark);
            TownSwatch[] bands = { TownSwatch.ClothYellow, TownSwatch.RedClay, TownSwatch.Iron };
            for (int i = 0; i < 3; i++)
            {
                float y = (0.6f + i * 0.62f) * s;
                b.Cylinder(Vector3.up * y, 0.255f * s, 0.25f * s, 0.16f * s, 7, bands[(i + variant) % 3], bands[(i + variant) % 3]);
            }

            // Crossbar with two hanging banners.
            float bar = h - 0.45f * s;
            b.Log(new Vector3(-0.7f * s, bar, 0f), new Vector3(0.7f * s, bar, 0f), 0.05f, TownSwatch.WoodDark, 4);
            foreach (float x in new[] { -0.5f * s, 0.5f * s })
            {
                b.Box(new Vector3(x, bar - 0.42f * s, 0f), new Vector3(0.32f * s, 0.75f * s, 0.03f), x < 0f ? TownSwatch.ClothYellow : TownSwatch.Laterite);
            }

            // The little drum on its cord, facing the front.
            var drum = new Vector3(0f, 1.15f * s, -0.32f * s);
            b.Log(drum + new Vector3(0f, 0f, -0.12f * s), drum + new Vector3(0f, 0f, 0.12f * s), 0.17f * s, TownSwatch.Laterite, 8, TownSwatch.Sand);
        }

        /// <summary>A fire pit: a ring of stones, crossed logs and the flames (ember and lamp glow: they light up at night).</summary>
        private static void Bonfire(TownMeshBuilder b, float s)
        {
            for (int i = 0; i < 9; i++)
            {
                float a = i / 9f * Mathf.PI * 2f;
                b.Crown(new Vector3(Mathf.Cos(a) * 0.62f * s, 0.1f * s, Mathf.Sin(a) * 0.62f * s), new Vector3(0.3f, 0.22f, 0.26f) * s, (i & 1) == 0 ? TownSwatch.StoneDark : TownSwatch.Stone, i * 40f);
            }

            b.Disc(new Vector3(0f, 0.02f, 0f), 0.52f * s, 8, TownSwatch.Black);
            for (int i = 0; i < 4; i++)
            {
                float a = (i * 45f + 20f) * Mathf.Deg2Rad;
                var d = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a)) * 0.5f * s;
                b.Log(-d + Vector3.up * 0.08f, d + Vector3.up * 0.3f * s, 0.07f * s, TownSwatch.WoodDark, 5, TownSwatch.Ember);
            }

            b.Cylinder(new Vector3(0f, 0.1f, 0f), 0.34f * s, 0f, 0.95f * s, 6, TownSwatch.Ember);
            b.Cylinder(new Vector3(0.12f * s, 0.1f, -0.06f * s), 0.2f * s, 0f, 1.25f * s, 5, TownSwatch.LampGlow);
            b.Cylinder(new Vector3(-0.14f * s, 0.1f, 0.08f * s), 0.18f * s, 0f, 0.8f * s, 5, TownSwatch.LampGlow);
        }

        /// <summary>A great war drum lying on an X stand (axis along local x), iron hoops, pale hide heads and two beaters resting on top.</summary>
        private static void WarDrum(TownMeshBuilder b, float s)
        {
            float r = 0.62f * s;
            float half = 0.5f * s;
            float y = r + 0.35f * s;
            foreach (float x in new[] { -half * 0.8f, half * 0.8f })
            {
                b.Log(new Vector3(x, 0f, -0.55f * s), new Vector3(x, y + 0.1f, 0.4f * s), 0.06f * s, TownSwatch.WoodDark, 4);
                b.Log(new Vector3(x, 0f, 0.55f * s), new Vector3(x, y + 0.1f, -0.4f * s), 0.06f * s, TownSwatch.WoodDark, 4);
            }

            b.Log(new Vector3(-half, y, 0f), new Vector3(half, y, 0f), r, TownSwatch.Laterite, 10, TownSwatch.Sand);
            foreach (float x in new[] { -half * 0.82f, 0f, half * 0.82f })
            {
                b.Log(new Vector3(x - 0.04f, y, 0f), new Vector3(x + 0.04f, y, 0f), r + 0.03f, x == 0f ? TownSwatch.ClothYellow : TownSwatch.Iron, 10);
            }

            foreach (float z in new[] { -0.18f * s, 0.18f * s })
            {
                b.Log(new Vector3(-0.3f * s, y + r + 0.03f, z), new Vector3(0.35f * s, y + r + 0.1f, z * 1.5f), 0.03f, TownSwatch.WoodLight, 4);
                b.Blob(new Vector3(0.38f * s, y + r + 0.11f, z * 1.5f), new Vector3(0.12f, 0.12f, 0.12f) * s, TownSwatch.Laterite);
            }
        }

        /// <summary>The sand training ring: a raked disc of pale sand, a ring of darker sand and a few flat border stones. Walkable.</summary>
        private static void TrainingRing(TownMeshBuilder b, float s)
        {
            float r = TrainingRingRadius * s;
            b.Disc(new Vector3(0f, 0.014f, 0f), r, 20, TownSwatch.Sand);
            b.Ring(new Vector3(0f, 0.018f, 0f), r - 0.35f, r, 20, TownSwatch.SandDark);
            for (int i = 0; i < 4; i++)
            {
                float z = (-0.5f + i * 0.33f) * r;
                float w = Mathf.Sqrt(Mathf.Max(0f, r * r - z * z)) * 0.8f;
                b.BoxMinMax(new Vector3(-w, 0.016f, z - 0.03f), new Vector3(w, 0.022f, z + 0.03f), TownSwatch.AdobeOchre);
            }

            for (int i = 0; i < 12; i++)
            {
                float a = i / 12f * Mathf.PI * 2f;
                b.Box(new Vector3(Mathf.Cos(a) * (r + 0.15f), 0.05f, Mathf.Sin(a) * (r + 0.15f)), new Vector3(0.45f, 0.1f, 0.28f), TownSwatch.StoneDark, -a * Mathf.Rad2Deg);
            }
        }

        // ------------------------------------------------------------------
        // Dressings
        // ------------------------------------------------------------------

        /// <summary>Kessi's front: a drying frame with two red hides, a bow and a quiver on pegs, a coil of snare cord.</summary>
        private static void WarCampDressing(TownMeshBuilder b, TownBuildingSpec spec, float x, float y, float z, float zFacade)
        {
            foreach (float lx in new[] { -0.5f, 0.5f })
            {
                b.Log(new Vector3(x + lx, y, z - 0.1f), new Vector3(x + lx, y + 1.55f, z - 0.1f), 0.035f, TownSwatch.WoodDark, 4);
            }

            b.Log(new Vector3(x - 0.6f, y + 1.5f, z - 0.1f), new Vector3(x + 0.6f, y + 1.5f, z - 0.1f), 0.03f, TownSwatch.WoodDark, 4);
            b.Box(new Vector3(x - 0.22f, y + 1.0f, z - 0.1f), new Vector3(0.42f, 0.85f, 0.03f), TownSwatch.Laterite);
            b.Box(new Vector3(x + 0.24f, y + 1.05f, z - 0.1f), new Vector3(0.36f, 0.75f, 0.03f), TownSwatch.RedClay);

            // Bow (three short staves in an arc) and the quiver on the wall.
            float bx = x + 1.0f;
            b.Log(new Vector3(bx, y + 0.7f, zFacade - 0.05f), new Vector3(bx + 0.12f, y + 1.15f, zFacade - 0.05f), 0.02f, TownSwatch.Walnut, 4);
            b.Log(new Vector3(bx + 0.12f, y + 1.15f, zFacade - 0.05f), new Vector3(bx, y + 1.6f, zFacade - 0.05f), 0.02f, TownSwatch.Walnut, 4);
            b.Log(new Vector3(bx - 0.02f, y + 0.7f, zFacade - 0.04f), new Vector3(bx - 0.02f, y + 1.6f, zFacade - 0.04f), 0.006f, TownSwatch.ClothWhite, 4);
            b.Cylinder(new Vector3(bx + 0.4f, y + 0.8f, zFacade - 0.1f), 0.09f, 0.1f, 0.55f, 6, TownSwatch.Teak, TownSwatch.ClothYellow);
            b.Ring(new Vector3(x - 0.9f, y + 0.02f, z - 0.3f), 0.12f, 0.22f, 8, TownSwatch.Thatch);
        }
    }
}
