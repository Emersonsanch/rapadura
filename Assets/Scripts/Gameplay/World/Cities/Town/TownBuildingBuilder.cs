using System.Collections.Generic;
using UnityEngine;

namespace Rapadura.Gameplay.World.Cities.Town
{
    /// <summary>
    /// Builds a <see cref="TownBuildingSpec"/> into one flat-shaded mesh in the lot's local frame (origin at the footprint
    /// centre, front facing −z, the aivan in the front <see cref="TownBuildingSpec.PorchDepth"/> metres), from the modular
    /// kit in <see cref="TownKit"/>: an adobe (or timber, tuff, round clay) mass on a stone plinth, doors and windows as
    /// framed openings on its faces, a roof module (flat with parapet and beam ends, dome on a drum, conical thatch, tiled
    /// gable, or a rock mass), the aivan on carved columns, and the shop's dressing. The pure helpers (posts, tower) are
    /// shared with <see cref="CitySolids"/> so the colliders always match the model. Deterministic; meshes are cached by
    /// <see cref="TownBuildingSpec.CacheKey"/> in <see cref="TownMeshCache"/>.
    /// </summary>
    public static partial class TownBuildingBuilder
    {
        /// <summary>Top of the plinth (the floor) above the ground.</summary>
        public const float FloorY = 0.25f;

        /// <summary>Aivan columns stand this far behind the porch front.</summary>
        public const float PostInset = 0.2f;

        /// <summary>Side of the lamp tower beside the lamp house.</summary>
        public const float TowerSize = 2f;

        public const float DoorHeight = 2.15f;
        public const float ParapetHeight = 0.4f;

        /// <summary>Local centre (x, z) of the tower: beside the main mass on its left, at the front.</summary>
        public static Vector2 TowerLocalCenter(float width, float depth)
        {
            return new Vector2(-width * 0.5f - TowerSize * 0.5f, -depth * 0.5f + TowerSize * 0.5f);
        }

        /// <summary>
        /// Local x of the aivan columns: the two front corners, plus intermediate ones every ~2.3 m for a full colonnade,
        /// never within 0.95 m of the door axis (the way in stays open).
        /// </summary>
        public static List<float> PorchPostsX(TownBuildingSpec spec)
        {
            var posts = new List<float>();
            if (spec == null || spec.PorchDepth <= 0.01f || spec.Kind == CityLotKind.ShipDock || spec.Kind == CityLotKind.Market)
            {
                return posts;
            }

            float hw = spec.Width * 0.5f - 0.2f;
            float doorX = spec.DoorOffset * spec.Width;
            posts.Add(-hw);
            if (spec.FullVaranda)
            {
                int spans = Mathf.Max(1, Mathf.RoundToInt(hw * 2f / 2.3f));
                for (int i = 1; i < spans; i++)
                {
                    float x = -hw + hw * 2f * i / spans;
                    if (Mathf.Abs(x - doorX) >= 0.95f)
                    {
                        posts.Add(x);
                    }
                }
            }

            posts.Add(hw);
            return posts;
        }

        /// <summary>Which side of the door the keeper stands on (+1 = local +x).</summary>
        public static float KeeperSide(TownBuildingSpec spec) => spec.DoorOffset > 0f ? -1f : 1f;

        // ------------------------------------------------------------------
        // Build
        // ------------------------------------------------------------------

        /// <summary>The whole building into <paramref name="b"/> (its matrix places it).</summary>
        public static void Build(TownMeshBuilder b, TownBuildingSpec spec)
        {
            switch (spec.Kind)
            {
                case CityLotKind.Market:
                    return;
                case CityLotKind.ShipDock:
                    BuildShipDock(b, spec);
                    return;
                case CityLotKind.LampHouse:
                    BuildLampHouse(b, spec);
                    return;
            }

            if (spec.Roof == TownRoofKind.Cone)
            {
                BuildRondavel(b, spec);
                return;
            }

            float hw = spec.Width * 0.5f;
            float depth = spec.Depth + spec.PorchDepth;
            float zFront = -depth * 0.5f;
            float zFacade = zFront + spec.PorchDepth;
            float zBack = depth * 0.5f;
            float top = spec.EaveHeight;

            if (spec.Roof == TownRoofKind.Rock)
            {
                BuildTuffHouse(b, spec, hw, zFront, zFacade, zBack, top);
                return;
            }

            // The mass on its plinth.
            TownKit.Plinth(b, -hw, hw, zFacade, zBack, spec.PlinthColor);
            TownSwatch wall = spec.WoodenWalls ? TownSwatch.WoodLight : spec.WallColor;
            b.BoxMinMax(new Vector3(-hw, FloorY, zFacade), new Vector3(hw, top, zBack), wall);
            if (spec.WoodenWalls)
            {
                TownKit.TimberFrame(b, -hw, hw, zFacade, zBack, FloorY, top, spec.JoineryColor == TownSwatch.Walnut ? TownSwatch.WoodDark : spec.JoineryColor);
            }
            else
            {
                TownKit.PlasterBand(b, -hw, hw, zFacade, zBack, FloorY, spec.TrimColor);
            }

            if (spec.Floors > 1)
            {
                // String course between the floors.
                float y = FloorY + spec.WallHeight;
                b.BoxMinMax(new Vector3(-hw - 0.05f, y - 0.08f, zFacade - 0.05f), new Vector3(hw + 0.05f, y + 0.08f, zBack + 0.05f), spec.TrimColor);
            }

            BuildOpenings(b, spec, hw, zFacade, zBack);

            // Roof.
            switch (spec.Roof)
            {
                case TownRoofKind.Gable:
                    TownKit.GableRoof(b, -hw, hw, zFront, zBack, top, spec.Width > 7f ? 1.4f : 1.2f, TownSwatch.ClayTile, TownSwatch.Beam, wall);
                    if (spec.PorchDepth > 0.01f)
                    {
                        // The front slope already covers the aivan: only the columns and the deck below.
                        TownKit.AivanDeck(b, -hw, hw, zFront, zFacade, spec);
                        TownKit.AivanColumns(b, spec, zFront + PostInset, FloorY, top - 0.18f);
                    }

                    break;

                case TownRoofKind.Dome:
                    TownKit.FlatRoof(b, -hw, hw, zFront, zBack, top, spec.RoofColor, wall, spec.TrimColor, spec.JoineryColor, spec.PorchDepth > 0.01f);
                    TownKit.Dome(b, new Vector3(0f, top, (zFacade + zBack) * 0.5f), Mathf.Min(spec.Width, spec.Depth) * 0.32f, wall, spec.AccentColor, TownSwatch.Gold);
                    if (spec.PorchDepth > 0.01f)
                    {
                        TownKit.AivanDeck(b, -hw, hw, zFront, zFacade, spec);
                        TownKit.AivanColumns(b, spec, zFront + PostInset, FloorY, top - 0.3f);
                        TownKit.AivanBeam(b, -hw, hw, zFront + PostInset, top - 0.3f, spec.JoineryColor);
                    }

                    break;

                default:
                    TownKit.FlatRoof(b, -hw, hw, zFront, zBack, top, spec.RoofColor, wall, spec.TrimColor, spec.JoineryColor, spec.PorchDepth > 0.01f);
                    if (spec.PorchDepth > 0.01f)
                    {
                        TownKit.AivanDeck(b, -hw, hw, zFront, zFacade, spec);
                        TownKit.AivanColumns(b, spec, zFront + PostInset, FloorY, top - 0.3f);
                        TownKit.AivanBeam(b, -hw, hw, zFront + PostInset, top - 0.3f, spec.JoineryColor);
                        TownKit.AivanCeiling(b, -hw, hw, zFront, zFacade, top - 0.06f);
                    }

                    if (spec.UpperRoom)
                    {
                        TownKit.UpperRoom(b, spec, hw, zFacade, zBack, top, wall);
                    }

                    break;
            }

            if (spec.Chimney)
            {
                TownKit.Chimney(b, new Vector3(hw - 0.75f, top, zBack - 0.8f), spec.Dressing == TownShopDressing.Ferraria);
            }

            if (spec.StorkNest)
            {
                TownKit.StorkNest(b, new Vector3(-hw + 0.6f, top + ParapetHeight, zBack - 0.6f));
            }

            if (spec.Sign && spec.PorchDepth > 0.01f)
            {
                TownKit.SignBoard(b, new Vector3(spec.DoorOffset * spec.Width, top - 0.55f, zFront + 0.05f), spec.SignColor, spec.AccentColor);
            }

            if (spec.Kind == CityLotKind.Shop || spec.Kind == CityLotKind.Workshop || spec.Kind == CityLotKind.Stable || spec.Kind == CityLotKind.Grandma)
            {
                TownKit.Dressing(b, spec, hw, zFront, zFacade);
            }
        }

        /// <summary>Door, windows and the suzani of the facade; windows on the sides and the back.</summary>
        private static void BuildOpenings(TownMeshBuilder b, TownBuildingSpec spec, float hw, float zFacade, float zBack)
        {
            float doorX = spec.DoorOffset * spec.Width;
            bool workshop = spec.Kind == CityLotKind.Workshop;
            float doorWidth = workshop ? Mathf.Min(spec.Width - 1.6f, 3.6f) : spec.WideDoor ? 1.7f : 1.05f;

            // Front face (local frame of the face: x across, y up, −z outwards).
            b.With(new Vector3(0f, 0f, zFacade), 0f, () =>
            {
                if (workshop)
                {
                    TownKit.WorkshopOpening(b, doorX, doorWidth, spec);
                }
                else if (spec.Kind == CityLotKind.Stable)
                {
                    TownKit.StableFront(b, spec);
                }
                else
                {
                    TownKit.Door(b, doorX, FloorY, doorWidth, DoorHeight, spec.JoineryColor, spec.TrimColor, spec.AccentColor, arched: true);
                }

                if (spec.Kind != CityLotKind.Stable)
                {
                    foreach (float x in FrontWindowsX(spec, doorX, doorWidth))
                    {
                        TownKit.Window(b, x, FloorY + 0.95f, 0.9f, 1.05f, spec.JoineryColor, spec.TrimColor, Lit(spec.Seed, x));
                    }
                }

                for (int floor = 1; floor < spec.Floors; floor++)
                {
                    float y = FloorY + spec.WallHeight * floor + 0.8f;
                    foreach (float x in new[] { -hw * 0.6f, 0f, hw * 0.6f })
                    {
                        TownKit.Window(b, x, y, 0.85f, 1.1f, spec.JoineryColor, spec.TrimColor, Lit(spec.Seed, x + 7f));
                    }
                }

                if (spec.PorchDepth > 0.6f && (spec.Kind == CityLotKind.Grandma || spec.Kind == CityLotKind.House))
                {
                    // A suzani hanging on the aivan wall, on the side away from the door.
                    float sx = doorX >= 0f ? -hw * 0.55f : hw * 0.55f;
                    TownKit.Suzani(b, new Vector3(sx, FloorY + 1.1f, -0.03f), 1.1f, 0.8f, spec.AccentColor);
                }
            });

            float sideDepth = zBack - zFacade;
            float sideMid = (zFacade + zBack) * 0.5f;
            int sideWindows = Mathf.Clamp(spec.SideWindows, 0, 2);
            for (int i = 0; i < sideWindows; i++)
            {
                float u = sideWindows == 1 ? 0f : (i == 0 ? -sideDepth * 0.22f : sideDepth * 0.22f);
                bool litA = Lit(spec.Seed, 31f + i);
                bool litB = Lit(spec.Seed, 41f + i);
                b.With(new Vector3(hw, 0f, sideMid), 270f, () => TownKit.Window(b, u, FloorY + 1f, 0.8f, 0.95f, spec.JoineryColor, spec.TrimColor, litA));
                b.With(new Vector3(-hw, 0f, sideMid), 90f, () => TownKit.Window(b, u, FloorY + 1f, 0.8f, 0.95f, spec.JoineryColor, spec.TrimColor, litB));
            }

            // Back: a window and a small door to the yard.
            b.With(new Vector3(0f, 0f, zBack), 180f, () =>
            {
                TownKit.Window(b, -hw * 0.45f, FloorY + 1f, 0.8f, 0.95f, spec.JoineryColor, spec.TrimColor, Lit(spec.Seed, 51f));
                TownKit.Door(b, hw * 0.4f, FloorY, 0.9f, 2f, spec.JoineryColor, spec.TrimColor, spec.AccentColor, arched: false);
            });
        }

        private static IEnumerable<float> FrontWindowsX(TownBuildingSpec spec, float doorX, float doorWidth)
        {
            float hw = spec.Width * 0.5f;
            float leftSpan = doorX - doorWidth * 0.5f - (-hw);
            float rightSpan = hw - (doorX + doorWidth * 0.5f);
            int count = Mathf.Clamp(spec.FrontWindows, 0, 2);
            bool keeperRight = KeeperSide(spec) > 0f;
            if (count >= 1 && (count == 2 || !keeperRight) && rightSpan > 1.5f)
            {
                yield return doorX + doorWidth * 0.5f + rightSpan * 0.5f;
            }

            if (count >= 1 && (count == 2 || keeperRight) && leftSpan > 1.5f)
            {
                yield return doorX - doorWidth * 0.5f - leftSpan * 0.5f;
            }
        }

        /// <summary>About two windows in three are lit at night (stable per building and window).</summary>
        private static bool Lit(string seed, float salt) => TownStyle.Hash01(seed, 900 + Mathf.RoundToInt(salt * 10f)) < 0.68f;

        // ------------------------------------------------------------------
        // Special buildings
        // ------------------------------------------------------------------

        /// <summary>
        /// The lamp house at the head of the plaza (Ramrat's guardian lamp, world bible §3.1), built as a secular
        /// caravanserai gatehouse, never a shrine (bible §0): a flat-roofed plastered block whose front is a broad
        /// square-headed gate between two stout tapering buttress towers, a timber gallery over the gate hung with bronze
        /// lanterns, an open lantern pavilion on the roof (four carved columns, a big lamp and a ring of lanterns under a
        /// low four-sided copper roof), lamps in niches along the sides, and a squat rammed-earth beacon tower on its left
        /// with an open fire cage on top (a Silk Road signal tower, not a slender spire). No dome, no pointed portal.
        /// </summary>
        private static void BuildLampHouse(TownMeshBuilder b, TownBuildingSpec spec)
        {
            float hw = spec.Width * 0.5f;
            float hd = spec.Depth * 0.5f;
            float top = FloorY + spec.WallHeight;
            TownKit.Plinth(b, -hw, hw, -hd, hd, spec.PlinthColor);
            b.BoxMinMax(new Vector3(-hw, FloorY, -hd), new Vector3(hw, top, hd), spec.WallColor);
            TownKit.PlasterBand(b, -hw, hw, -hd, hd, FloorY, TownSwatch.LimeWhite);
            TownKit.FlatRoof(b, -hw, hw, -hd, hd, top, TownSwatch.Mud, spec.WallColor, TownSwatch.LimeWhite, spec.JoineryColor, false);
            LanternPavilion(b, new Vector3(0f, top, hd * 0.1f), Mathf.Min(spec.Width, spec.Depth) * 0.36f, spec);

            b.With(new Vector3(0f, 0f, -hd), 0f, () =>
            {
                GateFront(b, top, spec);
                for (int i = 0; i < 2; i++)
                {
                    TownKit.LampNiche(b, i == 0 ? -hw + 0.4f : hw - 0.4f, FloorY + 2.6f);
                }

                // Steps in front of the gate.
                b.BoxMinMax(new Vector3(-1.4f, 0f, -0.75f), new Vector3(1.4f, 0.12f, 0f), TownSwatch.Stone);
            });

            // Lamp niches along both sides.
            for (int i = 0; i < 3; i++)
            {
                float u = -hd + 1.6f + i * (spec.Depth - 3.2f) / 2f;
                b.With(new Vector3(hw, 0f, 0f), 270f, () => TownKit.LampNiche(b, u, FloorY + 1.6f));
                b.With(new Vector3(-hw, 0f, 0f), 90f, () => TownKit.LampNiche(b, -u, FloorY + 1.6f));
            }

            Vector2 tower = TowerLocalCenter(spec.Width, spec.Depth);
            BeaconTower(b, new Vector3(tower.x, 0f, tower.y), TowerSize, spec);
        }

        /// <summary>Caravanserai gate (face frame): a square-headed timber gateway between two tapering buttress towers, a lantern gallery above.</summary>
        private static void GateFront(TownMeshBuilder b, float top, TownBuildingSpec spec)
        {
            const float gateW = 1.6f;
            const float gateH = 2.6f;
            // Shallow gate block standing proud of the facade, below the parapet (no tall portal).
            b.BoxMinMax(new Vector3(-1.45f, FloorY, -0.3f), new Vector3(1.45f, top - 0.2f, 0f), spec.WallColor);
            b.BoxMinMax(new Vector3(-1.5f, top - 0.2f, -0.36f), new Vector3(1.5f, top - 0.08f, 0.02f), TownSwatch.LimeWhite);
            b.With(new Vector3(0f, 0f, -0.3f), 0f, () => TownKit.Door(b, 0f, FloorY, gateW, gateH, spec.JoineryColor, TownSwatch.WoodDark, spec.TrimColor, arched: false));
            // Heavy timber lintel, with a turquoise tile line under it (detail only).
            b.BoxMinMax(new Vector3(-gateW * 0.5f - 0.35f, FloorY + gateH + 0.16f, -0.44f), new Vector3(gateW * 0.5f + 0.35f, FloorY + gateH + 0.42f, -0.3f), spec.JoineryColor);
            b.BoxMinMax(new Vector3(-gateW * 0.5f - 0.2f, FloorY + gateH + 0.05f, -0.4f), new Vector3(gateW * 0.5f + 0.2f, FloorY + gateH + 0.16f, -0.3f), spec.TrimColor);

            // Two stout buttress towers (round, tapering, crenellated) flanking the gate, like a caravanserai's.
            foreach (float x in new[] { -1.55f, 1.55f })
            {
                var foot = new Vector3(x, 0f, -0.2f);
                b.Cylinder(foot, 0.62f, 0.62f, FloorY + 0.05f, 8, spec.PlinthColor, spec.PlinthColor);
                b.Cylinder(foot + Vector3.up * (FloorY + 0.05f), 0.55f, 0.44f, top + 0.3f - FloorY - 0.05f, 8, spec.WallColor);
                b.Cylinder(foot + Vector3.up * (top + 0.3f), 0.5f, 0.5f, 0.08f, 8, TownSwatch.LimeWhite, TownSwatch.Mud);
                for (int k = 0; k < 4; k++)
                {
                    float a = (k * 90f + 45f) * Mathf.Deg2Rad;
                    b.Box(foot + new Vector3(Mathf.Cos(a) * 0.34f, top + 0.52f, Mathf.Sin(a) * 0.34f), new Vector3(0.2f, 0.28f, 0.2f), spec.WallColor, k * 90f + 45f, TownSwatch.LimeWhite);
                }

                b.Cylinder(foot + Vector3.up * (FloorY + 0.5f), 0.565f, 0.56f, 0.12f, 8, spec.TrimColor);
            }

            // Timber gallery over the gate: deck on beam ends, a low rail, lanterns hanging under it.
            float gy = FloorY + gateH + 0.48f;
            b.BoxMinMax(new Vector3(-1.0f, gy, -0.85f), new Vector3(1.0f, gy + 0.1f, -0.3f), spec.JoineryColor);
            for (int i = 0; i < 5; i++)
            {
                float x = -0.8f + i * 0.4f;
                b.Box(new Vector3(x, gy - 0.08f, -0.6f), new Vector3(0.1f, 0.1f, 0.6f), TownSwatch.WoodDark);
                b.Box(new Vector3(x, gy + 0.32f, -0.82f), new Vector3(0.05f, 0.45f, 0.05f), spec.JoineryColor);
            }

            b.BoxMinMax(new Vector3(-1.0f, gy + 0.52f, -0.86f), new Vector3(1.0f, gy + 0.58f, -0.78f), spec.JoineryColor);
            foreach (float x in new[] { -0.75f, 0.75f })
            {
                TownKit.Lantern(b, new Vector3(x, gy - 0.6f, -0.72f), 0.75f);
            }
        }

        /// <summary>The open lantern pavilion on the roof: four carved columns on a low dais, the big lamp, a ring of lanterns, a low copper pyramid roof.</summary>
        private static void LanternPavilion(TownMeshBuilder b, Vector3 baseCenter, float r, TownBuildingSpec spec)
        {
            float cx = baseCenter.x, cz = baseCenter.z;
            b.BoxMinMax(new Vector3(cx - r, baseCenter.y, cz - r), new Vector3(cx + r, baseCenter.y + 0.2f, cz + r), TownSwatch.Stone);
            float y0 = baseCenter.y + 0.2f;
            const float columnH = 1.6f;
            foreach (Vector2 c in new[] { new Vector2(-1, -1), new Vector2(1, -1), new Vector2(-1, 1), new Vector2(1, 1) })
            {
                TownKit.CarvedColumn(b, new Vector3(cx + c.x * (r - 0.25f), y0, cz + c.y * (r - 0.25f)), columnH, spec.JoineryColor, TownSwatch.Stone);
            }

            float beamY = y0 + columnH;
            b.BoxMinMax(new Vector3(cx - r, beamY, cz - r), new Vector3(cx + r, beamY + 0.16f, cz - r + 0.18f), spec.JoineryColor);
            b.BoxMinMax(new Vector3(cx - r, beamY, cz + r - 0.18f), new Vector3(cx + r, beamY + 0.16f, cz + r), spec.JoineryColor);
            b.BoxMinMax(new Vector3(cx - r, beamY, cz - r), new Vector3(cx - r + 0.18f, beamY + 0.16f, cz + r), spec.JoineryColor);
            b.BoxMinMax(new Vector3(cx + r - 0.18f, beamY, cz - r), new Vector3(cx + r, beamY + 0.16f, cz + r), spec.JoineryColor);
            b.BoxMinMax(new Vector3(cx - r - 0.04f, beamY + 0.04f, cz - r - 0.05f), new Vector3(cx + r + 0.04f, beamY + 0.13f, cz - r), spec.TrimColor);

            // The light itself: a big lamp in a copper bowl on a stand, and four small lanterns hanging from the beams.
            var lamp = new Vector3(cx, y0, cz);
            b.Cylinder(lamp, 0.3f, 0.12f, 0.5f, 8, TownSwatch.Copper, TownSwatch.Copper);
            b.Cylinder(lamp + Vector3.up * 0.5f, 0.32f, 0.48f, 0.14f, 8, TownSwatch.Copper, TownSwatch.Copper);
            b.Blob(lamp + Vector3.up * 0.92f, new Vector3(0.5f, 0.75f, 0.5f), TownSwatch.LampGlow);
            foreach (Vector2 c in new[] { new Vector2(0, -1), new Vector2(1, 0), new Vector2(0, 1), new Vector2(-1, 0) })
            {
                TownKit.Lantern(b, new Vector3(cx + c.x * (r - 0.1f), beamY - 0.5f, cz + c.y * (r - 0.1f)), 0.6f);
            }

            // A low four-sided copper roof with wide eaves and a small knob: a pavilion hat, not a dome.
            const float root2 = 1.4142f;
            var roofBase = new Vector3(cx, beamY + 0.16f, cz);
            float eave = (r + 0.4f) * root2;
            b.Cylinder(roofBase, eave, eave, 0.1f, 4, TownSwatch.WoodDark, null, 0.5f);
            b.Cylinder(roofBase + Vector3.up * 0.1f, eave, 0.16f, 0.8f, 4, TownSwatch.Copper, TownSwatch.Copper, 0.5f);
            b.Box(roofBase + Vector3.up * 0.95f, new Vector3(0.2f, 0.16f, 0.2f), TownSwatch.Gold, 45f);
        }

        /// <summary>
        /// The beacon tower: a stout tapering square of rammed earth with timber courses and a ladder up the front, crowned
        /// by an open fire cage (four posts, a copper fire bowl, a small hat roof). Stays inside its <paramref name="size"/> square.
        /// </summary>
        private static void BeaconTower(TownMeshBuilder b, Vector3 foot, float size, TownBuildingSpec spec)
        {
            const float root2 = 1.4142f;
            float h = size * 0.5f;
            TownKit.Plinth(b, foot.x - h, foot.x + h, foot.z - h, foot.z + h, TownSwatch.Stone);
            const float shaftH = 4.0f;
            float y0 = FloorY + 0.05f;
            float shaftTop = y0 + shaftH;
            // Square frustum (4 sides, phase 0.5: faces along the axes).
            b.Cylinder(new Vector3(foot.x, y0, foot.z), h * 0.92f * root2, h * 0.7f * root2, shaftH, 4, spec.WallColor, TownSwatch.Mud, 0.5f);
            foreach (float t in new[] { 0.3f, 0.65f })
            {
                float y = y0 + shaftH * t;
                float half = Mathf.Lerp(h * 0.92f, h * 0.7f, t) + 0.03f;
                b.BoxMinMax(new Vector3(foot.x - half, y, foot.z - half), new Vector3(foot.x + half, y + 0.1f, foot.z + half), TownSwatch.WoodDark);
            }

            // Ladder leaning on the front face.
            float zBottom = foot.z - h * 0.92f - 0.12f;
            float zTop = foot.z - h * 0.7f - 0.06f;
            foreach (float x in new[] { -0.22f, 0.22f })
            {
                b.Log(new Vector3(foot.x + x, 0.05f, zBottom), new Vector3(foot.x + x, shaftTop, zTop), 0.035f, TownSwatch.WoodWeathered, 4);
            }

            for (int i = 1; i < 9; i++)
            {
                float t = i / 9f;
                b.Box(new Vector3(foot.x, Mathf.Lerp(0.05f, shaftTop, t), Mathf.Lerp(zBottom, zTop, t)), new Vector3(0.44f, 0.04f, 0.05f), TownSwatch.WoodWeathered);
            }

            // Top deck with a lime rim, four posts of the fire cage.
            float deck = h * 0.78f;
            b.BoxMinMax(new Vector3(foot.x - deck, shaftTop, foot.z - deck), new Vector3(foot.x + deck, shaftTop + 0.14f, foot.z + deck), TownSwatch.LimeWhite);
            foreach (Vector2 c in new[] { new Vector2(-1, -1), new Vector2(1, -1), new Vector2(-1, 1), new Vector2(1, 1) })
            {
                var p = new Vector3(foot.x + c.x * (deck - 0.12f), shaftTop + 0.14f, foot.z + c.y * (deck - 0.12f));
                b.Box(p + Vector3.up * 0.65f, new Vector3(0.12f, 1.3f, 0.12f), spec.JoineryColor);
            }

            // Fire bowl on a short stand, the flame (glows at night) and the small hat roof.
            var bowl = new Vector3(foot.x, shaftTop + 0.14f, foot.z);
            b.Cylinder(bowl, 0.16f, 0.12f, 0.35f, 6, TownSwatch.Copper);
            b.Cylinder(bowl + Vector3.up * 0.35f, 0.22f, 0.45f, 0.25f, 8, TownSwatch.Copper, TownSwatch.Ember);
            b.Blob(bowl + Vector3.up * 0.82f, new Vector3(0.55f, 0.6f, 0.55f), TownSwatch.LampGlow);
            var hat = bowl + Vector3.up * 1.3f;
            float hatR = (deck + 0.12f) * root2;
            b.Cylinder(hat, hatR, hatR, 0.08f, 4, spec.JoineryColor, null, 0.5f);
            b.Cylinder(hat + Vector3.up * 0.08f, hatR, 0.08f, 0.45f, 4, TownSwatch.Copper, TownSwatch.Copper, 0.5f);
        }

        /// <summary>Litema rondavel: round clay walls with painted bands, a conical thatch roof (walls fit inside the lot rect).</summary>
        private static void BuildRondavel(TownMeshBuilder b, TownBuildingSpec spec)
        {
            float depth = spec.Depth + spec.PorchDepth;
            float zFacade = -depth * 0.5f + spec.PorchDepth;
            float radius = Mathf.Min(spec.Width, spec.Depth) * 0.5f - 0.05f;
            var center = new Vector3(0f, 0f, zFacade + spec.Depth * 0.5f);
            float wallTop = FloorY + spec.WallHeight * 0.85f;
            b.Cylinder(center, radius + 0.12f, radius + 0.12f, FloorY, 14, spec.PlinthColor, spec.PlinthColor);
            b.Cylinder(center + Vector3.up * FloorY, radius, radius, wallTop - FloorY, 14, spec.WallColor);
            b.Cylinder(center + Vector3.up * (FloorY + 0.45f), radius + 0.02f, radius + 0.02f, 0.28f, 14, spec.TrimColor);
            b.Cylinder(center + Vector3.up * (wallTop - 0.55f), radius + 0.02f, radius + 0.02f, 0.18f, 14, spec.AccentColor);
            b.Cylinder(center + Vector3.up * (wallTop - 0.1f), radius + 0.55f, 0f, radius * 1.25f + 0.6f, 14, spec.RoofColor);
            b.Cylinder(center + Vector3.up * (wallTop - 0.1f), radius + 0.55f, radius + 0.45f, 0.12f, 14, TownSwatch.Thatch);
            b.Log(center + Vector3.up * (wallTop + radius * 1.2f), center + Vector3.up * (wallTop + radius * 1.25f + 0.9f), 0.05f, TownSwatch.WoodDark);
            float doorX = spec.DoorOffset * spec.Width;
            b.With(new Vector3(0f, 0f, center.z - radius), 0f, () =>
            {
                TownKit.Door(b, doorX, FloorY, 0.95f, 1.95f, spec.JoineryColor, spec.TrimColor, spec.AccentColor, arched: false);
            });

            if (spec.PorchDepth > 0.01f)
            {
                // A swept clay forecourt with a low curb instead of an aivan.
                b.BoxMinMax(new Vector3(-spec.Width * 0.5f, 0f, -depth * 0.5f), new Vector3(spec.Width * 0.5f, 0.06f, zFacade + 0.3f), TownSwatch.Mud);
            }
        }

        /// <summary>Lamparinas: the house is a facade on a pink tuff mass, with an arched door, a round window and a lamp niche.</summary>
        private static void BuildTuffHouse(TownMeshBuilder b, TownBuildingSpec spec, float hw, float zFront, float zFacade, float zBack, float top)
        {
            b.Crown(new Vector3(0f, top * 0.55f, (zFacade + zBack) * 0.5f + 0.4f), new Vector3(spec.Width + 1.6f, top * 2.1f, spec.Depth + 1.6f), spec.WallColor);
            b.Crown(new Vector3(hw * 0.4f, top * 1.15f, zBack - 0.5f), new Vector3(spec.Width * 0.9f, top * 1.2f, spec.Depth), TownSwatch.PinkTuff, 30f);
            b.BoxMinMax(new Vector3(-hw, FloorY, zFacade), new Vector3(hw, top, zFacade + 0.6f), spec.WallColor);
            float doorX = spec.DoorOffset * spec.Width;
            b.With(new Vector3(0f, 0f, zFacade), 0f, () =>
            {
                TownKit.Door(b, doorX, FloorY, 1f, 2f, spec.JoineryColor, spec.TrimColor, spec.AccentColor, arched: true);
                TownKit.RoundWindow(b, doorX + (doorX >= 0f ? -1.5f : 1.5f), FloorY + 1.7f, 0.42f, spec.TrimColor);
                TownKit.LampNiche(b, doorX + (doorX >= 0f ? 1f : -1f) * 0.95f, FloorY + 1.4f);
            });

            if (spec.PorchDepth > 0.01f)
            {
                TownKit.AivanDeck(b, -hw, hw, zFront, zFacade, spec);
                TownKit.AivanColumns(b, spec, zFront + PostInset, FloorY, top - 0.4f);
                TownKit.AivanBeam(b, -hw, hw, zFront + PostInset, top - 0.4f, spec.JoineryColor);
            }
        }

        /// <summary>
        /// Cais da Barca (world bible §3.16): a short wooden pier with no water, over a groove of light in the ground,
        /// mooring posts with rope, a lantern at its head and a little shaded bench at the back.
        /// </summary>
        private static void BuildShipDock(TownMeshBuilder b, TownBuildingSpec spec)
        {
            float hw = spec.Width * 0.5f;
            float depth = spec.Depth + spec.PorchDepth;
            float hd = depth * 0.5f;
            float zFacade = -hd + spec.PorchDepth;

            // The groove of light the barca glides along, the full width of the lot.
            b.BoxMinMax(new Vector3(-hw, -0.02f, zFacade - 0.35f), new Vector3(hw, 0.03f, zFacade + 0.05f), TownSwatch.LampGlow);
            b.BoxMinMax(new Vector3(-hw, 0f, zFacade - 0.5f), new Vector3(hw, 0.05f, zFacade - 0.35f), TownSwatch.Stone);
            b.BoxMinMax(new Vector3(-hw, 0f, zFacade + 0.05f), new Vector3(hw, 0.05f, zFacade + 0.2f), TownSwatch.Stone);

            // The pier: planks on short piles along the groove (inside the dock's collider box).
            float px0 = -hw + 0.45f;
            float px1 = hw - 0.45f;
            int plank = 0;
            for (float x = px0; x < px1 - 0.01f; x += 0.42f)
            {
                b.BoxMinMax(new Vector3(x, 0.38f, zFacade + 0.25f), new Vector3(Mathf.Min(px1, x + 0.38f), 0.48f, zFacade + 1.85f),
                    (plank++ & 1) == 0 ? TownSwatch.WoodLight : TownSwatch.WoodWeathered);
            }

            for (float x = px0 + 0.2f; x < px1; x += 1.6f)
            {
                b.Log(new Vector3(x, 0f, zFacade + 0.35f), new Vector3(x, 0.4f, zFacade + 0.35f), 0.09f, TownSwatch.WoodDark);
                b.Log(new Vector3(x, 0f, zFacade + 1.75f), new Vector3(x, 0.4f, zFacade + 1.75f), 0.09f, TownSwatch.WoodDark);
            }

            // Mooring posts with rope, and the lantern at the pier head.
            foreach (float x in new[] { px0 + 0.3f, px1 - 0.3f })
            {
                b.Log(new Vector3(x, 0.48f, zFacade + 0.4f), new Vector3(x, 1.25f, zFacade + 0.4f), 0.1f, TownSwatch.Walnut);
                b.Cylinder(new Vector3(x, 0.95f, zFacade + 0.4f), 0.13f, 0.13f, 0.12f, 6, TownSwatch.Thatch);
            }

            b.Log(new Vector3(px0 + 0.3f, 1.0f, zFacade + 0.4f), new Vector3(px1 - 0.3f, 1.0f, zFacade + 0.4f), 0.025f, TownSwatch.Thatch, 4);
            TownKit.LampPost(b, new Vector3(px1 - 0.9f, 0.48f, zFacade + 1.5f), 0f, 0.9f);

            // Shaded bench at the back (the two back posts are colliders).
            foreach (float x in new[] { -hw + 0.25f, hw - 0.25f })
            {
                TownKit.CarvedColumn(b, new Vector3(x, 0f, hd - 0.25f), 2.4f, TownSwatch.Walnut, TownSwatch.Stone);
            }

            b.BoxMinMax(new Vector3(-hw, 2.4f, zFacade + 1.9f), new Vector3(hw, 2.55f, hd), TownSwatch.Beam);
            b.BoxMinMax(new Vector3(-hw - 0.1f, 2.55f, zFacade + 1.8f), new Vector3(hw + 0.1f, 2.65f, hd + 0.1f), TownSwatch.Mud);
            b.BoxMinMax(new Vector3(-hw + 0.6f, 0f, hd - 0.75f), new Vector3(hw - 0.6f, 0.45f, hd - 0.35f), TownSwatch.WoodLight);
            TownKit.SignBoard(b, new Vector3(0f, 2.05f, zFacade + 1.95f), TownSwatch.ClothIndigo, TownSwatch.Gold);
        }
    }
}
