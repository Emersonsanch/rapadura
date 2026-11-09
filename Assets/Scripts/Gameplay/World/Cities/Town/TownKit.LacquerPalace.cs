using System.Collections.Generic;
using UnityEngine;

namespace Rapadura.Gameplay.World.Cities.Town
{
    /// <summary>
    /// The pieces of Hinokura, a Ilha-Forja, the capital of the Arquipélago de Hinokura (Docs/Mapas100.md map 65, style
    /// <c>LacquerPalace</c>): the halls of red lacquer (<see cref="LacquerHallHouse"/>: a curved base wall of coral stone on
    /// black volcanic footings with its corners rising, red lacquered walls between dark posts, a gold band under the eave, a
    /// hipped roof of red tiles with white mortar ridges and up-turned corners, a double door with gold studs, a porch of black
    /// stone with a small stone lantern), the Forja de Hinokura on the plaza (Ícaro's forge: a round hearth fed by a runnel of
    /// lava, the anvil, the great bellows and the tiled hood), the channels of tamed lava with their sluice gates and bridges of
    /// black stone, the stone lanterns, the pines pruned in cloud pads on lava rock, the boulders of volcanic rock and the gate
    /// of red posts under a tiled double roof; Kurogane's black-iron forge front. The palaces of Shuri and the gusuku of Ryukyu
    /// and the Japanese castle towns are form only: no shrine, no figure of a faith (the shisa guardians belong to the gardens of
    /// map 66 as creatures, not to these roofs). Same rules as the rest of the kit: flat colours from <see cref="TownPalette"/>
    /// (row 22 holds lacquer, red tile, volcanic stone, coral stone, the glowing lava, pine, obsidian and pumice), low-poly,
    /// deterministic.
    /// </summary>
    public static partial class TownKit
    {
        /// <summary>Radius of the <see cref="CityPropKind.IsleForge"/>'s round hearth (× its scale): its collider.</summary>
        public const float IsleForgeRadius = 1.3f;

        /// <summary>Radius of a <see cref="CityPropKind.StoneLantern"/> (× its scale): its collider.</summary>
        public const float StoneLanternRadius = 0.6f;

        /// <summary>Radius of a <see cref="CityPropKind.PrunedPine"/>'s rock and trunk (× its scale): its collider.</summary>
        public const float PrunedPineRadius = 1.0f;

        /// <summary>Radius of a <see cref="CityPropKind.LavaRock"/> (× its scale): its collider.</summary>
        public const float LavaRockRadius = 0.9f;

        /// <summary>Half the width of the glowing lava of a <see cref="CityPropKind.LavaSluice"/>.</summary>
        public const float LavaSluiceHalfWidth = 0.8f;

        /// <summary>Height of the curved coral-stone base wall a <see cref="TownBuildingSpec.LacquerHall"/> stands on.</summary>
        private const float CoralBaseTop = 0.55f;

        // ------------------------------------------------------------------
        // The hall of red lacquer
        // ------------------------------------------------------------------

        /// <summary>
        /// A <see cref="TownBuildingSpec.LacquerHall"/> (local frame, front −z): black volcanic footings, the curved base wall of
        /// coral stone (<see cref="TownBuildingSpec.PlinthColor"/>) whose front corners rise like the gusuku walls, red lacquered
        /// walls (<see cref="TownBuildingSpec.WallColor"/>) between dark posts (<see cref="TownBuildingSpec.JoineryColor"/>), a
        /// gold band (<see cref="TownBuildingSpec.AccentColor"/>) under the eave, latticed windows, a double door with gold studs;
        /// a hipped roof of red tiles (<see cref="TownBuildingSpec.RoofColor"/>) with white mortar ridges and up-turned corners;
        /// in front a porch of black stone on two lacquered posts (at <see cref="TownBuildingBuilder.PorchPostsX"/>, the colliders
        /// use the same list) under a tiled lean-to, a small stone lantern and the sign as a plaque under the eave.
        /// </summary>
        public static void LacquerHallHouse(TownMeshBuilder b, TownBuildingSpec spec, float hw, float zFront, float zFacade, float zBack, float top)
        {
            float eave = Mathf.Max(top, CoralBaseTop + 2.3f);
            float ridge = eave + Mathf.Min(1.5f, hw * 0.42f);
            float wx = hw - 0.25f;
            float zw = zFacade + 0.2f, zb = zBack - 0.2f;

            // Black volcanic footings and the curved base wall of coral stone: the front bows out a little and its corners rise.
            b.BoxMinMax(new Vector3(-hw, 0f, zFacade), new Vector3(hw, 0.12f, zBack), TownSwatch.VolcanicStone, TownSwatch.VolcanicStone);
            b.BoxMinMax(new Vector3(-hw + 0.08f, 0.12f, zFacade + 0.08f), new Vector3(hw - 0.08f, CoralBaseTop, zBack - 0.08f), spec.PlinthColor, spec.PlinthColor);
            const int courses = 6;
            for (int i = 0; i < courses; i++)
            {
                float t = (i + 0.5f) / courses;
                float x = Mathf.Lerp(-hw + 0.3f, hw - 0.3f, t);
                float bow = 0.14f * Mathf.Sin(t * Mathf.PI);
                float rise = 0.28f * Mathf.Pow(Mathf.Abs(t - 0.5f) * 2f, 2f);
                b.Box(new Vector3(x, (CoralBaseTop + rise) * 0.5f + 0.06f, zFacade + 0.02f - bow), new Vector3(hw * 2f / courses - 0.04f, CoralBaseTop + rise - 0.1f, 0.22f),
                    spec.PlinthColor, 0f, TownSwatch.Pumice);
            }

            // The lacquered walls, the dark posts and the gold band under the eave.
            b.BoxMinMax(new Vector3(-wx, CoralBaseTop, zw), new Vector3(wx, eave, zb), spec.WallColor, spec.WallColor);
            int posts = Mathf.Clamp(Mathf.RoundToInt(wx * 2f / 1.6f) + 1, 3, 7);
            for (int i = 0; i < posts; i++)
            {
                float x = Mathf.Lerp(-wx, wx, i / (posts - 1f));
                b.Box(new Vector3(x, (CoralBaseTop + eave) * 0.5f, zw - 0.04f), new Vector3(0.16f, eave - CoralBaseTop, 0.1f), spec.JoineryColor, 0f);
            }

            b.Box(new Vector3(0f, eave - 0.2f, zw - 0.06f), new Vector3(wx * 2f + 0.1f, 0.16f, 0.06f), spec.AccentColor, 0f);

            // The double door with gold studs, its black frame and a transom.
            float doorX = Mathf.Clamp(spec.DoorOffset * spec.Width, -wx + 0.9f, wx - 0.9f);
            float leaf = spec.WideDoor ? 0.68f : 0.55f;
            float doorTop = CoralBaseTop + 2.0f;
            b.BoxMinMax(new Vector3(doorX - leaf * 2f * 0.5f - 0.14f, CoralBaseTop, zw - 0.08f), new Vector3(doorX + leaf + 0.14f, doorTop + 0.14f, zw - 0.02f), spec.JoineryColor);
            foreach (float side in new[] { -1f, 1f })
            {
                float cx = doorX + side * leaf * 0.5f;
                b.Box(new Vector3(cx, (CoralBaseTop + doorTop) * 0.5f, zw - 0.1f), new Vector3(leaf - 0.04f, doorTop - CoralBaseTop, 0.04f), TownSwatch.PalaceLacquer, 0f);
                for (int row = 0; row < 3; row++)
                {
                    b.Box(new Vector3(cx, CoralBaseTop + 0.5f + row * 0.55f, zw - 0.13f), new Vector3(leaf * 0.6f, 0.05f, 0.03f), spec.AccentColor, 0f);
                }
            }

            b.Box(new Vector3(doorX, doorTop + 0.06f, zw - 0.12f), new Vector3(leaf * 2f, 0.1f, 0.04f), spec.AccentColor, 0f);

            // Latticed windows of lit paper either side of the door.
            int windows = Mathf.Clamp(spec.FrontWindows, 0, 2);
            for (int i = 0; i < windows; i++)
            {
                float side = i == 0 ? -1f : 1f;
                float x = Mathf.Clamp(doorX + side * (leaf + 1.1f), -wx + 0.5f, wx - 0.5f);
                if (Mathf.Abs(x - doorX) < leaf + 0.5f)
                {
                    continue;
                }

                float y = eave - 0.95f;
                b.Box(new Vector3(x, y, zw - 0.05f), new Vector3(0.74f, 0.6f, 0.06f), spec.JoineryColor, 0f);
                b.Box(new Vector3(x, y, zw - 0.08f), new Vector3(0.6f, 0.46f, 0.03f), TownSwatch.WindowLit, 0f);
                b.Box(new Vector3(x, y, zw - 0.1f), new Vector3(0.04f, 0.46f, 0.02f), spec.JoineryColor, 0f);
                b.Box(new Vector3(x, y, zw - 0.1f), new Vector3(0.6f, 0.04f, 0.02f), spec.JoineryColor, 0f);
            }

            // The hipped roof of red tiles: front and back slopes, two hip ends, white mortar ridges and up-turned corners.
            float over = 0.45f;
            float x0 = -hw - over, x1 = hw + over, z0 = zFacade - over, z1 = zBack + over;
            float cz = (zFacade + zBack) * 0.5f;
            float rx = Mathf.Max(0.3f, hw * 0.4f);
            float y0 = eave - 0.1f, yr = ridge + 0.1f;
            var tl = new Vector3(-rx, yr, cz);
            var tr = new Vector3(rx, yr, cz);
            b.Plate(new List<Vector3> { new Vector3(x0, y0, z0), new Vector3(x1, y0, z0), tr, tl }, 0.2f, spec.RoofColor, spec.JoineryColor);
            b.Plate(new List<Vector3> { new Vector3(x1, y0, z1), new Vector3(x0, y0, z1), tl, tr }, 0.2f, spec.RoofColor, spec.JoineryColor);
            b.Plate(new List<Vector3> { new Vector3(x0, y0, z1), new Vector3(x0, y0, z0), tl }, 0.2f, spec.RoofColor, spec.JoineryColor);
            b.Plate(new List<Vector3> { new Vector3(x1, y0, z0), new Vector3(x1, y0, z1), tr }, 0.2f, spec.RoofColor, spec.JoineryColor);
            b.Log(tl, tr, 0.09f, spec.TrimColor, 4);
            foreach (Vector3 corner in new[] { new Vector3(x0, y0, z0), new Vector3(x1, y0, z0), new Vector3(x0, y0, z1), new Vector3(x1, y0, z1) })
            {
                Vector3 inner = corner.x < 0f ? tl : tr;
                b.Log(corner + Vector3.up * 0.04f, inner + Vector3.up * 0.04f, 0.06f, spec.TrimColor, 4);
                Vector3 outward = new Vector3(Mathf.Sign(corner.x), 0f, Mathf.Sign(corner.z - cz)).normalized;
                b.Log(corner, corner + outward * 0.3f + Vector3.up * 0.32f, 0.07f, spec.RoofColor, 4);
            }

            // The ridge ends curl up like the roofs of Shuri.
            foreach (Vector3 end in new[] { tl, tr })
            {
                b.Log(end, end + new Vector3(Mathf.Sign(end.x) * 0.15f, 0.3f, 0f), 0.08f, spec.TrimColor, 4);
            }

            if (spec.PorchDepth > 0.01f)
            {
                // The porch: a deck of black stone, two lacquered posts, a beam, a tiled lean-to, a small stone lantern and the plaque.
                float deck = CitySolids.DeckHeight;
                b.BoxMinMax(new Vector3(-hw, 0f, zFront), new Vector3(hw, deck, zFacade), TownSwatch.VolcanicStone, TownSwatch.Obsidian);
                float z = zFront + TownBuildingBuilder.PostInset;
                float beamY = eave - 0.3f;
                foreach (float x in TownBuildingBuilder.PorchPostsX(spec))
                {
                    b.Cylinder(new Vector3(x, deck, z), 0.13f, 0.13f, 0.18f, 6, TownSwatch.CoralStone, TownSwatch.CoralStone);
                    b.Box(new Vector3(x, (deck + beamY) * 0.5f + 0.09f, z), new Vector3(0.18f, beamY - deck - 0.18f, 0.18f), TownSwatch.PalaceLacquer, 0f);
                }

                b.Box(new Vector3(0f, beamY, z), new Vector3(hw * 2f - 0.1f, 0.18f, 0.22f), spec.JoineryColor, 0f, spec.AccentColor);
                b.Slab(new Vector3(-hw - 0.1f, beamY + 0.14f, z - 0.3f), new Vector3(-hw - 0.1f, eave - 0.05f, zw),
                    new Vector3(hw + 0.1f, eave - 0.05f, zw), new Vector3(hw + 0.1f, beamY + 0.14f, z - 0.3f), 0.14f, spec.RoofColor, spec.JoineryColor);

                float lanternX = Mathf.Clamp(doorX + (doorX >= 0f ? -1f : 1f) * (leaf + 1.0f), -hw + 0.45f, hw - 0.45f);
                SmallLantern(b, new Vector3(lanternX, deck, (zFront + zFacade) * 0.5f), 0.75f);

                if (spec.Sign)
                {
                    var at = new Vector3(doorX, beamY - 0.3f, z - 0.13f);
                    b.Box(at, new Vector3(1.15f, 0.36f, 0.05f), spec.JoineryColor, 0f);
                    b.Box(at + new Vector3(0f, 0f, -0.03f), new Vector3(0.98f, 0.24f, 0.02f), spec.SignColor, 0f);
                }
            }
        }

        /// <summary>A small stone lantern of <paramref name="s"/> on <paramref name="foot"/>: a foot, a post, the lit fire box and a cap.</summary>
        private static void SmallLantern(TownMeshBuilder b, Vector3 foot, float s)
        {
            b.Cylinder(foot, 0.22f * s, 0.18f * s, 0.12f * s, 6, TownSwatch.VolcanicStone, TownSwatch.VolcanicStone);
            b.Cylinder(foot + Vector3.up * 0.12f * s, 0.09f * s, 0.09f * s, 0.55f * s, 6, TownSwatch.Pumice);
            b.Box(foot + Vector3.up * 0.7f * s, new Vector3(0.34f, 0.06f, 0.34f) * s, TownSwatch.Pumice, 0f);
            b.Box(foot + Vector3.up * 0.86f * s, new Vector3(0.24f, 0.26f, 0.24f) * s, TownSwatch.LavaGlow, 0f);
            b.Cylinder(foot + Vector3.up * 0.99f * s, 0.3f * s, 0.04f * s, 0.2f * s, 6, TownSwatch.VolcanicStone, null, 0.5f);
        }

        // ------------------------------------------------------------------
        // The gate
        // ------------------------------------------------------------------

        /// <summary>
        /// The gate of Hinokura at a street's end (the form of the ceremonial gates of Shuri, not a shrine gate): two red
        /// lacquered posts on coral feet with braces, a black lintel with a gold band, a double roof of red tiles with white
        /// ridges on top and a hanging lantern of lit paper in the middle.
        /// </summary>
        public static void LacquerGate(TownMeshBuilder b, Vector3 a, Vector3 c)
        {
            Vector3 span = c - a;
            float w = new Vector2(span.x, span.z).magnitude;
            if (w < 0.5f)
            {
                return;
            }

            b.With(a, YawAlong(new Vector2(span.x, span.z) / w), () =>
            {
                const float postTop = 3.4f;
                foreach (float x in new[] { 0f, w })
                {
                    b.Cylinder(new Vector3(x, 0f, 0f), 0.4f, 0.34f, 0.4f, 6, TownSwatch.CoralStone, TownSwatch.Pumice);
                    b.Box(new Vector3(x, 0.4f + (postTop - 0.4f) * 0.5f, 0f), new Vector3(0.32f, postTop - 0.4f, 0.32f), TownSwatch.PalaceLacquer, 0f);
                    foreach (float dz in new[] { -0.45f, 0.45f })
                    {
                        b.Log(new Vector3(x, 0.4f, dz), new Vector3(x, 1.6f, 0f), 0.06f, TownSwatch.Obsidian, 4);
                    }
                }

                b.Box(new Vector3(w * 0.5f, postTop - 0.35f, 0f), new Vector3(w + 0.2f, 0.26f, 0.3f), TownSwatch.Obsidian, 0f);
                b.Box(new Vector3(w * 0.5f, postTop - 0.35f, -0.16f), new Vector3(w * 0.8f, 0.1f, 0.02f), TownSwatch.Gold, 0f);
                b.Box(new Vector3(w * 0.5f, postTop + 0.05f, 0f), new Vector3(w + 0.4f, 0.18f, 0.4f), TownSwatch.PalaceLacquer, 0f);

                // The double roof of red tiles.
                for (int tier = 0; tier < 2; tier++)
                {
                    float y = postTop + 0.15f + tier * 0.55f;
                    float hx = (w * 0.5f + 0.9f) - tier * 0.45f, hz = 0.85f - tier * 0.2f;
                    float m = w * 0.5f;
                    var p = new List<Vector3>
                    {
                        new Vector3(m - hx, y, -hz), new Vector3(m + hx, y, -hz), new Vector3(m + hx * 0.55f, y + 0.4f, 0f), new Vector3(m - hx * 0.55f, y + 0.4f, 0f),
                    };
                    b.Plate(p, 0.12f, TownSwatch.FireTile, TownSwatch.Obsidian);
                    var q = new List<Vector3>
                    {
                        new Vector3(m + hx, y, hz), new Vector3(m - hx, y, hz), new Vector3(m - hx * 0.55f, y + 0.4f, 0f), new Vector3(m + hx * 0.55f, y + 0.4f, 0f),
                    };
                    b.Plate(q, 0.12f, TownSwatch.FireTile, TownSwatch.Obsidian);
                    b.Log(new Vector3(m - hx * 0.55f, y + 0.42f, 0f), new Vector3(m + hx * 0.55f, y + 0.42f, 0f), 0.06f, TownSwatch.CoralStone, 4);
                    foreach (float sx in new[] { -1f, 1f })
                    {
                        b.Log(new Vector3(m + sx * hx, y, -hz), new Vector3(m + sx * (hx + 0.2f), y + 0.25f, -hz - 0.1f), 0.05f, TownSwatch.FireTile, 4);
                        b.Log(new Vector3(m + sx * hx, y, hz), new Vector3(m + sx * (hx + 0.2f), y + 0.25f, hz + 0.1f), 0.05f, TownSwatch.FireTile, 4);
                    }
                }

                // The hanging lantern of lit paper.
                float mid = w * 0.5f;
                b.Log(new Vector3(mid, postTop - 0.48f, 0f), new Vector3(mid, postTop - 0.8f, 0f), 0.012f, TownSwatch.Iron, 3);
                b.Cylinder(new Vector3(mid, postTop - 1.35f, 0f), 0.22f, 0.26f, 0.55f, 8, TownSwatch.LanternPaper, TownSwatch.Obsidian);
            });
        }

        // ------------------------------------------------------------------
        // Props
        // ------------------------------------------------------------------

        /// <summary>
        /// The Forja de Hinokura (Ícaro's forge, on the plaza): a round hearth of black volcanic stone with a coral rim, its fire
        /// of tamed lava fed by a short runnel from the north, an anvil on a block of obsidian to the south (where the cousins
        /// come), the great bellows of lacquered wood to the east, a rack of tongs and the hood of red tiles on four posts with a
        /// chimney of black stone.
        /// </summary>
        private static void IsleForge(TownMeshBuilder b, float s)
        {
            float r = IsleForgeRadius * s;
            b.Cylinder(Vector3.zero, r, r * 0.92f, 0.7f * s, 10, TownSwatch.VolcanicStone, TownSwatch.VolcanicStone);
            b.Ring(new Vector3(0f, 0.71f * s, 0f), r * 0.6f, r * 0.92f, 10, TownSwatch.CoralStone, TownSwatch.Pumice);
            b.Disc(new Vector3(0f, 0.68f * s, 0f), r * 0.6f, 10, TownSwatch.LavaGlow);
            for (int i = 0; i < 5; i++)
            {
                float a = (i * 72f + 15f) * Mathf.Deg2Rad;
                b.Blob(new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a)) * (r * 0.35f) + Vector3.up * 0.75f * s, new Vector3(0.22f, 0.12f, 0.2f) * s, TownSwatch.Obsidian);
            }

            // The runnel of lava from the north into the hearth.
            b.Box(new Vector3(0f, 0.35f * s, r * 0.95f), new Vector3(0.5f, 0.7f, 0.5f) * s, TownSwatch.VolcanicStone, 0f, TownSwatch.LavaGlow);

            // The anvil on its obsidian block.
            var anvil = new Vector3(0.1f * s, 0f, -r * 0.82f);
            b.Box(anvil + Vector3.up * 0.25f * s, new Vector3(0.5f, 0.5f, 0.4f) * s, TownSwatch.Obsidian, 0f);
            b.Box(anvil + Vector3.up * 0.6f * s, new Vector3(0.6f, 0.18f, 0.24f) * s, TownSwatch.Iron, 0f);
            b.Log(anvil + new Vector3(0.3f, 0.62f, 0f) * s, anvil + new Vector3(0.52f, 0.6f, 0f) * s, 0.06f * s, TownSwatch.Iron, 4);

            // The great bellows to the east.
            var bellows = new Vector3(r * 0.95f, 0.45f * s, 0f);
            b.Box(bellows, new Vector3(0.5f, 0.45f, 0.8f) * s, TownSwatch.PalaceLacquer, 0f, TownSwatch.Obsidian);
            b.Log(bellows + new Vector3(-0.2f, 0f, 0f) * s, new Vector3(r * 0.55f, 0.6f * s, 0f), 0.05f * s, TownSwatch.Iron, 4);
            b.Log(bellows + new Vector3(0.1f, 0.25f, 0f) * s, bellows + new Vector3(0.4f, 0.75f, 0f) * s, 0.04f * s, TownSwatch.WoodDark, 4);

            // The hood of red tiles on four posts and the chimney.
            float hoodY = 2.6f * s;
            foreach (float deg in new[] { 45f, 135f, 225f, 315f })
            {
                float a = deg * Mathf.Deg2Rad;
                var foot = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a)) * (r * 0.88f);
                b.Box(foot + Vector3.up * (0.7f * s + (hoodY - 0.7f * s) * 0.5f), new Vector3(0.14f, hoodY - 0.7f * s, 0.14f), TownSwatch.Obsidian, deg);
            }

            b.Cylinder(new Vector3(0f, hoodY, 0f), r * 1.1f, 0.35f * s, 0.8f * s, 8, TownSwatch.FireTile, null, 0.5f);
            b.Ring(new Vector3(0f, hoodY + 0.01f, 0f), r * 1.02f, r * 1.12f, 8, TownSwatch.CoralStone);
            b.Cylinder(new Vector3(0f, hoodY + 0.75f * s, 0f), 0.3f * s, 0.26f * s, 0.9f * s, 6, TownSwatch.VolcanicStone, TownSwatch.Obsidian);
            b.Blob(new Vector3(0f, hoodY + 1.8f * s, 0f), new Vector3(0.4f, 0.3f, 0.4f) * s, TownSwatch.Ember);
        }

        /// <summary>
        /// A channel of tamed lava from the origin to <paramref name="d"/> (the glowing lava <see cref="LavaSluiceHalfWidth"/> each
        /// side between lips of black stone), with a sluice gate of iron and lacquered wood at each end of the bridge and, at its
        /// middle, a bridge of black stone slabs with low red rails. Walkable: the slabs stay under the walker's step.
        /// </summary>
        private static void LavaSluice(TownMeshBuilder b, Vector2 d)
        {
            float length = d.magnitude;
            if (length < 0.5f)
            {
                return;
            }

            float w = LavaSluiceHalfWidth;
            b.With(Vector3.zero, YawAlong(d / length), () =>
            {
                b.Flat(0f, -w, length, w, 0.02f, TownSwatch.LavaGlow);
                foreach (float z in new[] { -w - 0.12f, w + 0.12f })
                {
                    b.Box(new Vector3(length * 0.5f, 0.06f, z), new Vector3(length, 0.12f, 0.24f), TownSwatch.VolcanicStone, 0f, TownSwatch.Obsidian);
                }

                int crusts = Mathf.Max(2, Mathf.RoundToInt(length / 1.2f));
                for (int i = 0; i < crusts; i++)
                {
                    float x = length * (i + 0.5f) / crusts;
                    if (Mathf.Abs(x - length * 0.5f) < 1.3f)
                    {
                        continue;
                    }

                    b.Blob(new Vector3(x, 0.04f, (i % 2 == 0 ? -0.3f : 0.25f) * w), new Vector3(0.5f, 0.08f, 0.35f), TownSwatch.Obsidian);
                }

                // The bridge of black stone slabs with low red rails.
                float xm = length * 0.5f;
                for (int i = -2; i <= 2; i++)
                {
                    b.Box(new Vector3(xm + i * 0.42f, 0.1f, 0f), new Vector3(0.38f, 0.1f, w * 2f + 0.6f), TownSwatch.VolcanicStone, 0f, i == 0 ? TownSwatch.CoralStone : TownSwatch.VolcanicStone);
                }

                foreach (float z in new[] { -w - 0.25f, w + 0.25f })
                {
                    foreach (float x in new[] { xm - 1.0f, xm + 1.0f })
                    {
                        b.Box(new Vector3(x, 0.4f, z), new Vector3(0.1f, 0.6f, 0.1f), TownSwatch.PalaceLacquer, 0f, TownSwatch.Gold);
                    }

                    b.Box(new Vector3(xm, 0.62f, z), new Vector3(2.1f, 0.08f, 0.08f), TownSwatch.PalaceLacquer, 0f);
                }

                // The sluice gates near each end: two iron posts and a lacquered board half raised.
                foreach (float x in new[] { Mathf.Min(0.6f, length * 0.15f), Mathf.Max(length - 0.6f, length * 0.85f) })
                {
                    foreach (float z in new[] { -w - 0.1f, w + 0.1f })
                    {
                        b.Box(new Vector3(x, 0.5f, z), new Vector3(0.12f, 1.0f, 0.12f), TownSwatch.Iron, 0f);
                    }

                    b.Box(new Vector3(x, 0.55f, 0f), new Vector3(0.08f, 0.5f, w * 2f), TownSwatch.Obsidian, 0f, TownSwatch.PalaceLacquer);
                    b.Box(new Vector3(x, 1.02f, 0f), new Vector3(0.14f, 0.08f, w * 2f + 0.3f), TownSwatch.Iron, 0f);
                }
            });
        }

        /// <summary>
        /// A stone lantern: a foot of volcanic stone, a post and a deck of pumice, the fire box lit with tamed lava, a hipped cap
        /// with a knob (Variant 1: taller, a second ring round the post and a tiled cap).
        /// </summary>
        private static void StoneLantern(TownMeshBuilder b, float s, int variant)
        {
            bool tall = (variant % 2) == 1;
            float k = tall ? 1.35f : 1f;
            float r = StoneLanternRadius * s;
            b.Cylinder(Vector3.zero, r * 0.8f, r * 0.65f, 0.25f * s, 6, TownSwatch.VolcanicStone, TownSwatch.VolcanicStone);
            b.Cylinder(new Vector3(0f, 0.25f * s, 0f), 0.13f * s, 0.11f * s, 0.75f * s * k, 6, TownSwatch.Pumice);
            if (tall)
            {
                b.Cylinder(new Vector3(0f, 0.55f * s, 0f), 0.2f * s, 0.2f * s, 0.1f * s, 6, TownSwatch.VolcanicStone, TownSwatch.VolcanicStone);
            }

            float deck = 0.25f * s + 0.75f * s * k;
            b.Box(new Vector3(0f, deck + 0.05f * s, 0f), new Vector3(0.6f, 0.1f, 0.6f) * s, TownSwatch.Pumice, 0f);
            b.Box(new Vector3(0f, deck + 0.3f * s, 0f), new Vector3(0.4f, 0.4f, 0.4f) * s, TownSwatch.LavaGlow, 0f);
            foreach (float sx in new[] { -1f, 1f })
            {
                foreach (float sz in new[] { -1f, 1f })
                {
                    b.Box(new Vector3(sx * 0.19f * s, deck + 0.3f * s, sz * 0.19f * s), new Vector3(0.06f, 0.42f, 0.06f) * s, TownSwatch.Pumice, 0f);
                }
            }

            b.Cylinder(new Vector3(0f, deck + 0.5f * s, 0f), 0.48f * s, 0.06f * s, 0.32f * s, 4, tall ? TownSwatch.FireTile : TownSwatch.VolcanicStone, null, 0.5f);
            b.Blob(new Vector3(0f, deck + 0.88f * s, 0f), new Vector3(0.14f, 0.16f, 0.14f) * s, TownSwatch.Pumice);
        }

        /// <summary>
        /// A pine pruned in cloud pads on a block of lava rock: the rock, a twisting trunk in three pieces, three flat pads of
        /// dark needles (Variant 1: the trunk leans and a crutch of lacquered wood holds up the long branch).
        /// </summary>
        private static void PrunedPine(TownMeshBuilder b, float s, int variant)
        {
            bool leaning = (variant % 2) == 1;
            float r = PrunedPineRadius * s;
            b.Crown(new Vector3(0f, 0.25f * s, 0f), new Vector3(r * 2f, 0.6f * s, r * 1.7f), TownSwatch.VolcanicStone, 20f);
            b.Blob(new Vector3(r * 0.4f, 0.15f * s, -r * 0.5f), new Vector3(0.5f, 0.3f, 0.4f) * s, TownSwatch.Pumice);

            float lean = leaning ? 0.5f : 0.2f;
            var p0 = new Vector3(0f, 0.45f * s, 0f);
            var p1 = new Vector3(lean * 0.6f * s, 1.3f * s, 0.1f * s);
            var p2 = new Vector3(-lean * 0.2f * s, 2.0f * s, -0.05f * s);
            var p3 = new Vector3(lean * 0.9f * s, 2.6f * s, 0.05f * s);
            b.Log(p0, p1, 0.16f * s, TownSwatch.Trunk, 5);
            b.Log(p1, p2, 0.13f * s, TownSwatch.Trunk, 5);
            b.Log(p2, p3, 0.1f * s, TownSwatch.Trunk, 5);

            var branch = new Vector3((leaning ? 1.3f : -0.9f) * s, 1.6f * s, 0f);
            b.Log(p1, branch, 0.07f * s, TownSwatch.Trunk, 4);
            b.Crown(branch + Vector3.up * 0.1f * s, new Vector3(1.2f, 0.35f, 0.9f) * s, TownSwatch.IslandPine, 10f);
            b.Crown(p2 + new Vector3(0.4f, 0.1f, 0f) * s, new Vector3(1.0f, 0.32f, 0.8f) * s, TownSwatch.IslandPine, 40f);
            b.Crown(p3 + Vector3.up * 0.1f * s, new Vector3(1.1f, 0.4f, 0.9f) * s, TownSwatch.IslandPine, 70f);
            if (leaning)
            {
                b.Log(new Vector3(branch.x, 0.3f * s, 0f), branch + Vector3.down * 0.05f * s, 0.05f * s, TownSwatch.PalaceLacquer, 4);
                b.Box(new Vector3(branch.x, branch.y - 0.05f * s, 0f), new Vector3(0.3f, 0.06f, 0.12f) * s, TownSwatch.Obsidian, 0f);
            }
        }

        /// <summary>
        /// A boulder of volcanic rock with cracks of glowing lava across it and a cap of pumice (Variant 1: larger, a wisp of
        /// steam over it and loose pumice at its foot).
        /// </summary>
        private static void LavaRock(TownMeshBuilder b, float s, int variant)
        {
            bool large = (variant % 2) == 1;
            float r = LavaRockRadius * s;
            float h = (large ? 1.6f : 1.1f) * s;
            b.Crown(new Vector3(0f, h * 0.45f, 0f), new Vector3(r * 1.9f, h, r * 1.7f), TownSwatch.VolcanicStone, 25f);
            b.Crown(new Vector3(r * 0.35f, h * 0.25f, -r * 0.3f), new Vector3(r * 0.9f, h * 0.55f, r * 0.8f), TownSwatch.Obsidian, -15f);
            b.Box(new Vector3(-r * 0.15f, h * 0.55f, -r * 0.82f), new Vector3(0.06f, h * 0.55f, 0.05f), TownSwatch.LavaGlow, 20f);
            b.Box(new Vector3(r * 0.25f, h * 0.4f, -r * 0.78f), new Vector3(0.45f * s, 0.05f, 0.05f), TownSwatch.LavaGlow, -30f);
            b.Blob(new Vector3(-r * 0.1f, h * 0.92f, 0f), new Vector3(r * 0.8f, 0.2f * s, r * 0.7f), TownSwatch.Pumice);
            if (large)
            {
                b.Blob(new Vector3(0.1f * s, h + 0.5f * s, 0f), new Vector3(0.5f, 0.6f, 0.5f) * s, TownSwatch.Snowfield);
                b.Blob(new Vector3(-0.15f * s, h + 1.0f * s, 0.1f * s), new Vector3(0.35f, 0.45f, 0.35f) * s, TownSwatch.Snowfield);
                b.Blob(new Vector3(-r * 0.8f, 0.1f * s, -r * 0.6f), new Vector3(0.35f, 0.2f, 0.3f) * s, TownSwatch.Pumice);
                b.Blob(new Vector3(r * 0.9f, 0.08f * s, r * 0.4f), new Vector3(0.3f, 0.16f, 0.28f) * s, TownSwatch.Pumice);
            }
        }

        // ------------------------------------------------------------------
        // Dressing
        // ------------------------------------------------------------------

        /// <summary>Kurogane's front: an anvil on a block of obsidian, a quench trough of lava-dark water, a rack of black-iron tools, crampons on a peg and a glowing ingot on the anvil.</summary>
        private static void LacquerPalaceDressing(TownMeshBuilder b, TownBuildingSpec spec, float x, float y, float z, float zFacade)
        {
            // The anvil and its glowing ingot.
            b.Box(new Vector3(x - 0.9f, y + 0.25f, z - 0.45f), new Vector3(0.45f, 0.5f, 0.38f), TownSwatch.Obsidian, 0f);
            b.Box(new Vector3(x - 0.9f, y + 0.58f, z - 0.45f), new Vector3(0.55f, 0.16f, 0.22f), TownSwatch.Iron, 0f);
            b.Box(new Vector3(x - 0.85f, y + 0.69f, z - 0.45f), new Vector3(0.24f, 0.05f, 0.08f), TownSwatch.LavaGlow, 0f);

            // The quench trough.
            b.Box(new Vector3(x + 0.1f, y + 0.22f, z - 0.5f), new Vector3(0.9f, 0.44f, 0.4f), TownSwatch.VolcanicStone, 0f, TownSwatch.ColdWater);

            // The rack of black-iron tools and the crampons on a peg.
            b.Log(new Vector3(x + 0.9f, y, z - 0.25f), new Vector3(x + 0.9f, y + 1.5f, z - 0.25f), 0.04f, TownSwatch.Obsidian, 4);
            b.Log(new Vector3(x + 1.6f, y, z - 0.25f), new Vector3(x + 1.6f, y + 1.5f, z - 0.25f), 0.04f, TownSwatch.Obsidian, 4);
            b.Log(new Vector3(x + 0.85f, y + 1.4f, z - 0.25f), new Vector3(x + 1.65f, y + 1.4f, z - 0.25f), 0.035f, TownSwatch.PalaceLacquer, 4);
            for (int i = 0; i < 3; i++)
            {
                float tx = x + 1.0f + i * 0.25f;
                b.Log(new Vector3(tx, y + 1.38f, z - 0.27f), new Vector3(tx, y + 0.75f, z - 0.27f), 0.025f, TownSwatch.Iron, 4);
                b.Box(new Vector3(tx, y + 0.72f, z - 0.27f), new Vector3(0.1f, 0.12f, 0.04f), TownSwatch.Iron, 0f);
            }

            b.Box(new Vector3(x - 1.5f, y + 1.2f, zFacade - 0.06f), new Vector3(0.3f, 0.06f, 0.1f), TownSwatch.Iron, 0f);
            b.Box(new Vector3(x - 1.5f, y + 1.05f, zFacade - 0.08f), new Vector3(0.24f, 0.24f, 0.03f), TownSwatch.Iron, 0f);
            b.Blob(new Vector3(x + 0.1f, y + 0.95f, zFacade - 0.08f), new Vector3(0.2f, 0.24f, 0.2f), TownSwatch.LanternPaper);
        }
    }
}
