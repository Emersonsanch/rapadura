using UnityEngine;

namespace Rapadura.Gameplay.World.Cities.Town
{
    /// <summary>Props, vegetation, shop dressings and the villagers of the town kit.</summary>
    public static partial class TownKit
    {
        // ------------------------------------------------------------------
        // Shop dressings (inside the aivan, on the side away from the keeper)
        // ------------------------------------------------------------------

        public static void Dressing(TownMeshBuilder b, TownBuildingSpec spec, float hw, float zFront, float zFacade)
        {
            float doorX = spec.DoorOffset * spec.Width;
            float side = -TownBuildingBuilder.KeeperSide(spec);
            float x = doorX + side * Mathf.Min(1.7f, hw - 0.7f);
            float z = zFacade - 0.35f;
            float y = CitySolids.DeckHeight;
            switch (spec.Dressing)
            {
                case TownShopDressing.Mercearia:
                    Sack(b, new Vector3(x - 0.3f, y, z), TownSwatch.ClothWhite);
                    Sack(b, new Vector3(x + 0.25f, y, z + 0.05f), TownSwatch.Thatch);
                    Basket(b, new Vector3(x, y, z - 0.55f), TownSwatch.Fruit);
                    b.Crown(new Vector3(x + 0.55f, y + 0.18f, z - 0.45f), new Vector3(0.4f, 0.32f, 0.36f), TownSwatch.Vine);
                    b.Crown(new Vector3(x - 0.55f, y + 0.16f, z - 0.5f), new Vector3(0.34f, 0.3f, 0.3f), TownSwatch.ClothYellow);
                    break;

                case TownShopDressing.Armarinho:
                    for (int i = 0; i < 4; i++)
                    {
                        TownSwatch c = i == 0 ? TownSwatch.ClothRed : i == 1 ? TownSwatch.ClothIndigo : i == 2 ? TownSwatch.ClothYellow : TownSwatch.Turquoise;
                        b.Log(new Vector3(x - 0.6f, y + 0.25f + i * 0.22f, z), new Vector3(x + 0.6f, y + 0.25f + i * 0.22f, z), 0.1f, c, 6);
                    }

                    b.Log(new Vector3(x - 0.7f, y, z), new Vector3(x - 0.7f, y + 1.1f, z), 0.04f, TownSwatch.WoodDark, 4);
                    b.Log(new Vector3(x + 0.7f, y, z), new Vector3(x + 0.7f, y + 1.1f, z), 0.04f, TownSwatch.WoodDark, 4);
                    break;

                case TownShopDressing.GeneralStore:
                    Barrel(b, new Vector3(x - 0.35f, y, z));
                    Barrel(b, new Vector3(x + 0.35f, y, z));
                    b.Box(new Vector3(x, y + 0.25f, z - 0.6f), new Vector3(0.7f, 0.5f, 0.5f), TownSwatch.WoodLight, 15f, TownSwatch.Beam);
                    b.Cylinder(new Vector3(x, y + 0.5f, z - 0.6f), 0.18f, 0.14f, 0.3f, 8, TownSwatch.Copper, TownSwatch.Copper);
                    break;

                case TownShopDressing.Marcenaria:
                    for (int i = 0; i < 3; i++)
                    {
                        b.Log(new Vector3(x - 0.75f, y + 0.12f + i * 0.2f, z - 0.2f + (i & 1) * 0.1f), new Vector3(x + 0.75f, y + 0.12f + i * 0.2f, z - 0.15f), 0.1f, TownSwatch.Trunk, 6, TownSwatch.WoodLight);
                    }

                    // A carved low stool (finished work).
                    b.Box(new Vector3(x + 0.1f, y + 0.4f, z - 0.75f), new Vector3(0.5f, 0.06f, 0.5f), TownSwatch.Walnut, 0f);
                    foreach (Vector2 l in new[] { new Vector2(-0.2f, -0.2f), new Vector2(0.2f, -0.2f), new Vector2(-0.2f, 0.2f), new Vector2(0.2f, 0.2f) })
                    {
                        b.Log(new Vector3(x + 0.1f + l.x, y, z - 0.75f + l.y), new Vector3(x + 0.1f + l.x, y + 0.38f, z - 0.75f + l.y), 0.03f, TownSwatch.WoodDark, 4);
                    }

                    break;

                case TownShopDressing.Costureira:
                    // Dress form and a bolt of silk.
                    b.Log(new Vector3(x, y, z - 0.2f), new Vector3(x, y + 0.7f, z - 0.2f), 0.03f, TownSwatch.WoodDark, 4);
                    b.Cylinder(new Vector3(x, y + 0.65f, z - 0.2f), 0.32f, 0.18f, 0.55f, 8, TownSwatch.Violet, TownSwatch.ClothWhite);
                    b.Blob(new Vector3(x, y + 1.3f, z - 0.2f), new Vector3(0.36f, 0.24f, 0.26f), TownSwatch.Violet);
                    b.Log(new Vector3(x + 0.6f, y + 0.12f, z - 0.5f), new Vector3(x + 0.6f, y + 0.12f, z + 0.1f), 0.12f, TownSwatch.Turquoise, 6, TownSwatch.ClothWhite);
                    Suzani(b, new Vector3(x - 0.2f, y + 1.6f, zFacade - 0.03f), 1f, 0.7f, TownSwatch.Turquoise);
                    break;

                case TownShopDressing.Haras:
                    b.Box(new Vector3(x, y + 0.35f, z - 0.4f), new Vector3(1.2f, 0.7f, 0.7f), TownSwatch.Thatch, 8f, TownSwatch.LeafGold);
                    // Saddle on a rail.
                    b.Log(new Vector3(x - 0.4f, y + 0.9f, z - 1.0f), new Vector3(x + 0.4f, y + 0.9f, z - 1.0f), 0.05f, TownSwatch.WoodDark, 4);
                    b.Blob(new Vector3(x, y + 1.02f, z - 1.0f), new Vector3(0.55f, 0.22f, 0.42f), TownSwatch.Walnut);
                    break;

                case TownShopDressing.Ferraria:
                    b.Cylinder(new Vector3(x, y, z - 0.3f), 0.3f, 0.3f, 0.65f, 8, TownSwatch.Walnut, TownSwatch.Water);
                    for (int i = 0; i < 3; i++)
                    {
                        b.Box(new Vector3(x + 0.6f, y + 0.9f + i * 0.25f, zFacade - 0.04f), new Vector3(0.26f, 0.05f, 0.04f), TownSwatch.Iron, 0f);
                    }

                    break;
            }

            if (spec.Kind == CityLotKind.Grandma)
            {
                // Her tapchan (raised wooden bed) with a carpet and a low table, and the empty chair of Vo Jorge.
                float tx = doorX + (doorX >= 0f ? -1f : 1f) * 2.2f;
                b.Box(new Vector3(tx, y + 0.22f, z - 0.25f), new Vector3(1.6f, 0.42f, 1.0f), TownSwatch.Walnut, 0f, TownSwatch.ClothRed);
                b.Box(new Vector3(tx, y + 0.5f, z - 0.25f), new Vector3(1.2f, 0.12f, 0.7f), TownSwatch.ClothIndigo, 0f);
                b.Box(new Vector3(tx, y + 0.6f, z - 0.25f), new Vector3(0.45f, 0.06f, 0.35f), TownSwatch.WoodDark, 0f);
                b.Cylinder(new Vector3(tx + 0.1f, y + 0.63f, z - 0.25f), 0.07f, 0.06f, 0.1f, 6, TownSwatch.Copper, TownSwatch.Copper);
                Chair(b, new Vector3(doorX - Mathf.Sign(tx - doorX) * 1.3f, y, z - 0.2f), 180f);
                // Flower pots along the aivan edge.
                for (int i = 0; i < 3; i++)
                {
                    float px = -hw + 0.6f + i * 0.45f;
                    b.Cylinder(new Vector3(px, y, zFront + 0.45f), 0.14f, 0.18f, 0.28f, 7, TownSwatch.Brick, TownSwatch.DirtDark);
                    b.Crown(new Vector3(px, y + 0.42f, zFront + 0.45f), new Vector3(0.3f, 0.26f, 0.3f), i == 1 ? TownSwatch.ClothRed : TownSwatch.Tamarisk);
                }
            }
        }

        private static void Sack(TownMeshBuilder b, Vector3 at, TownSwatch color)
        {
            b.Crown(at + Vector3.up * 0.28f, new Vector3(0.45f, 0.56f, 0.4f), color);
            b.Cylinder(at + Vector3.up * 0.5f, 0.1f, 0.06f, 0.14f, 5, color);
        }

        private static void Basket(TownMeshBuilder b, Vector3 at, TownSwatch fruit)
        {
            b.Cylinder(at, 0.22f, 0.3f, 0.28f, 8, TownSwatch.Thatch, TownSwatch.DirtDark);
            for (int i = 0; i < 4; i++)
            {
                float a = i * Mathf.PI * 0.5f + 0.4f;
                b.Blob(at + new Vector3(Mathf.Cos(a) * 0.12f, 0.33f, Mathf.Sin(a) * 0.12f), new Vector3(0.14f, 0.13f, 0.14f), fruit);
            }
        }

        private static void Barrel(TownMeshBuilder b, Vector3 at)
        {
            b.Cylinder(at, 0.26f, 0.3f, 0.35f, 9, TownSwatch.Walnut);
            b.Cylinder(at + Vector3.up * 0.35f, 0.3f, 0.26f, 0.35f, 9, TownSwatch.Walnut, TownSwatch.WoodLight);
            b.Cylinder(at + Vector3.up * 0.12f, 0.29f, 0.29f, 0.05f, 9, TownSwatch.Iron);
            b.Cylinder(at + Vector3.up * 0.55f, 0.29f, 0.29f, 0.05f, 9, TownSwatch.Iron);
        }

        private static void Chair(TownMeshBuilder b, Vector3 at, float yaw)
        {
            b.With(at, yaw, () =>
            {
                b.Box(new Vector3(0f, 0.45f, 0f), new Vector3(0.5f, 0.06f, 0.48f), TownSwatch.Walnut, 0f);
                b.Box(new Vector3(0f, 0.85f, 0.22f), new Vector3(0.5f, 0.7f, 0.05f), TownSwatch.Walnut, 0f);
                foreach (Vector2 l in new[] { new Vector2(-0.2f, -0.2f), new Vector2(0.2f, -0.2f), new Vector2(-0.2f, 0.2f), new Vector2(0.2f, 0.2f) })
                {
                    b.Log(new Vector3(l.x, 0f, l.y), new Vector3(l.x, 0.44f, l.y), 0.025f, TownSwatch.WoodDark, 4);
                }

                b.Box(new Vector3(0f, 0.5f, 0f), new Vector3(0.44f, 0.06f, 0.42f), TownSwatch.ClothIndigo, 0f);
            });
        }

        // ------------------------------------------------------------------
        // Street furniture
        // ------------------------------------------------------------------

        /// <summary>A carved post with a bronze lantern on a bracket (arm along local −z).</summary>
        public static void LampPost(TownMeshBuilder b, Vector3 foot, float yaw, float scale = 1f)
        {
            b.With(foot, yaw, () =>
            {
                float h = 2.9f * scale;
                b.Cylinder(Vector3.zero, 0.16f, 0.14f, 0.3f, 6, TownSwatch.Stone, TownSwatch.Stone);
                b.Cylinder(Vector3.up * 0.3f, 0.07f, 0.055f, h - 0.3f, 6, TownSwatch.Walnut);
                b.Cylinder(Vector3.up * (h * 0.55f), 0.08f, 0.08f, 0.08f, 6, TownSwatch.WoodDark, TownSwatch.WoodDark);
                b.Log(new Vector3(0f, h - 0.05f, 0f), new Vector3(0f, h - 0.05f, -0.55f), 0.03f, TownSwatch.Iron, 4);
                Lantern(b, new Vector3(0f, h - 0.62f, -0.5f), scale);
            });
        }

        /// <summary>Draws one layout prop (world position and yaw already applied by the caller's matrix).</summary>
        public static void Prop(TownMeshBuilder b, CityProp prop, TownStyle style)
        {
            float s = prop.Scale <= 0f ? 1f : prop.Scale;
            switch (prop.Kind)
            {
                case CityPropKind.Well: Well(b, s); break;
                case CityPropKind.Bench: Supa(b); break;
                case CityPropKind.LampPost: LampPost(b, Vector3.zero, 0f); break;
                case CityPropKind.Tree: Tree(b, style.Vegetation, prop.Variant, s); break;
                case CityPropKind.Stall: Stall(b, prop.Variant, style); break;
                case CityPropKind.Bush: b.Crown(new Vector3(0f, 0.45f * s, 0f), new Vector3(1.1f, 0.9f, 1.0f) * s, style.Vegetation == TownVegetation.Forest ? TownSwatch.Tamarisk : TownSwatch.LeafDark); break;
                case CityPropKind.FlowerBed: FlowerBed(b, prop.Variant); break;
                case CityPropKind.Crates:
                    b.Box(new Vector3(0f, 0.25f, 0f), new Vector3(0.6f, 0.5f, 0.6f), TownSwatch.WoodLight, 0f, TownSwatch.Beam);
                    b.Box(new Vector3(0.15f, 0.7f, 0.05f), new Vector3(0.5f, 0.4f, 0.5f), TownSwatch.WoodWeathered, 20f);
                    break;
                case CityPropKind.HayBale:
                    b.Box(new Vector3(0f, 0.38f, 0f), new Vector3(1.2f, 0.75f, 0.75f), TownSwatch.Thatch, 0f, TownSwatch.LeafGold);
                    b.Box(new Vector3(0f, 0.38f, 0f), new Vector3(1.22f, 0.08f, 0.77f), TownSwatch.WoodDark, 0f);
                    break;
                case CityPropKind.Sundial: Sundial(b); break;
                case CityPropKind.Rock:
                    if (prop.Variant == 9)
                    {
                        b.Crown(new Vector3(0f, 0f, 0f), new Vector3(s, s * 0.42f, s * 0.9f), style.GroundAlt);
                    }
                    else
                    {
                        b.Crown(new Vector3(0f, 0.3f * s, 0f), new Vector3(1f, 0.7f, 0.9f) * s, TownSwatch.Stone, prop.Yaw);
                    }

                    break;
                case CityPropKind.GrassTuft:
                    b.Crown(new Vector3(0f, 0.08f, 0f), new Vector3(0.5f, 0.2f, 0.5f), style.GroundAlt);
                    break;
                case CityPropKind.Tandir: Tandir(b); break;
                case CityPropKind.Trellis: Trellis(b, s); break;
                case CityPropKind.Jugs: Jugs(b); break;
                case CityPropKind.Stela: Stela(b, s); break;
            }
        }

        /// <summary>A yard wall run from <paramref name="a"/> to <paramref name="c"/> (adobe with coping, or dry stone).</summary>
        public static void WallRun(TownMeshBuilder b, Vector3 a, Vector3 c, TownStyle style)
        {
            Vector3 d = c - a;
            float length = d.magnitude;
            if (length < 0.05f)
            {
                return;
            }

            float yaw = Mathf.Atan2(d.x, d.z) * Mathf.Rad2Deg;
            bool dryStone = style.Architecture == TownArchitecture.Rondavel || style.Architecture == TownArchitecture.CaveTuff;
            b.With((a + c) * 0.5f, yaw, () =>
            {
                if (dryStone)
                {
                    // Dry-stone courses with a herringbone band (Great Zimbabwe style): stacked, slightly offset blocks.
                    int courses = 4;
                    for (int i = 0; i < courses; i++)
                    {
                        float y = 0.12f + i * 0.24f;
                        b.Box(new Vector3((i & 1) * 0.03f, y, 0f), new Vector3(0.36f - i * 0.03f, 0.24f, length), i == 2 ? TownSwatch.StoneDark : style.Fence, 0f);
                    }

                    for (float z = -length * 0.5f + 0.2f; z < length * 0.5f - 0.1f; z += 0.35f)
                    {
                        b.Box(new Vector3(0.17f, 0.6f, z), new Vector3(0.03f, 0.14f, 0.22f), TownSwatch.Stone, 40f);
                    }
                }
                else
                {
                    b.Box(new Vector3(0f, 0.55f, 0f), new Vector3(0.24f, 1.1f, length), style.Fence, 0f);
                    b.Box(new Vector3(0f, 1.13f, 0f), new Vector3(0.32f, 0.07f, length + 0.06f), TownSwatch.LimeWhite, 0f);
                    b.Box(new Vector3(0f, 0.12f, 0f), new Vector3(0.28f, 0.24f, length + 0.02f), style.Plinth, 0f);
                }
            });
        }

        /// <summary>The town gate: two adobe pylons with a carved beam over the road and lanterns.</summary>
        public static void Gate(TownMeshBuilder b, Vector3 a, Vector3 c, TownStyle style)
        {
            foreach (Vector3 p in new[] { a, c })
            {
                b.Box(p + Vector3.up * 1.6f, new Vector3(0.7f, 3.2f, 0.7f), style.Walls[0], 0f, TownSwatch.LimeWhite);
                b.Box(p + Vector3.up * 3.3f, new Vector3(0.85f, 0.2f, 0.85f), TownSwatch.LimeWhite, 0f);
                b.Crown(p + Vector3.up * 3.55f, new Vector3(0.4f, 0.4f, 0.4f), style.Accent);
                Lantern(b, p + new Vector3(0f, 2.35f, 0f) + (c - a).normalized * (p == a ? 0.45f : -0.45f), 0.8f);
            }

            Vector3 up = Vector3.up * 3.0f;
            b.Log(a + up, c + up, 0.14f, TownSwatch.Walnut, 4);
            b.Log(a + up + Vector3.up * 0.25f, c + up + Vector3.up * 0.25f, 0.09f, TownSwatch.WoodDark, 4);
        }

        private static void Well(TownMeshBuilder b, float s)
        {
            float r = 0.85f * s;
            b.Cylinder(Vector3.zero, r, r, 0.75f, 10, TownSwatch.Stone);
            b.Ring(new Vector3(0f, 0.76f, 0f), r - 0.2f, r + 0.05f, 10, TownSwatch.LimeWhite);
            b.Disc(new Vector3(0f, 0.55f, 0f), r - 0.2f, 10, TownSwatch.Water);
            foreach (float x in new[] { -r, r })
            {
                CarvedColumn(b, new Vector3(x, 0.7f, 0f), 1.4f, TownSwatch.Walnut, TownSwatch.Stone);
            }

            b.Log(new Vector3(-r, 1.85f, 0f), new Vector3(r, 1.85f, 0f), 0.07f, TownSwatch.WoodDark);
            b.Log(new Vector3(0f, 1.82f, 0f), new Vector3(0f, 1.1f, 0f), 0.015f, TownSwatch.Thatch, 4);
            b.Cylinder(new Vector3(0f, 0.92f, 0f), 0.14f, 0.17f, 0.22f, 7, TownSwatch.Copper, TownSwatch.Black);
            b.Plate(new[] { new Vector3(-r - 0.3f, 2.15f, -0.7f), new Vector3(-r - 0.3f, 2.55f, 0f), new Vector3(r + 0.3f, 2.55f, 0f), new Vector3(r + 0.3f, 2.15f, -0.7f) }, 0.06f, TownSwatch.Mud, TownSwatch.Beam);
            b.Plate(new[] { new Vector3(-r - 0.3f, 2.55f, 0f), new Vector3(-r - 0.3f, 2.15f, 0.7f), new Vector3(r + 0.3f, 2.15f, 0.7f), new Vector3(r + 0.3f, 2.55f, 0f) }, 0.06f, TownSwatch.Mud, TownSwatch.Beam);
            Jugs(b, new Vector3(r + 0.35f, 0f, 0.4f));
        }

        /// <summary>A supa / tapchan: a low wooden platform with a rail and a carpet, the bench of the Silk Road plazas.</summary>
        private static void Supa(TownMeshBuilder b)
        {
            b.Box(new Vector3(0f, 0.38f, 0f), new Vector3(1.6f, 0.08f, 0.55f), TownSwatch.Walnut, 0f);
            b.Box(new Vector3(0f, 0.43f, 0f), new Vector3(1.4f, 0.03f, 0.45f), TownSwatch.ClothRed, 0f);
            b.Box(new Vector3(0f, 0.45f, 0f), new Vector3(1.1f, 0.02f, 0.3f), TownSwatch.ClothYellow, 0f);
            b.Box(new Vector3(0f, 0.72f, 0.25f), new Vector3(1.6f, 0.06f, 0.05f), TownSwatch.WoodDark, 0f);
            foreach (float x in new[] { -0.72f, 0f, 0.72f })
            {
                b.Log(new Vector3(x, 0.42f, 0.25f), new Vector3(x, 0.72f, 0.25f), 0.025f, TownSwatch.WoodDark, 4);
            }

            foreach (Vector2 l in new[] { new Vector2(-0.72f, -0.22f), new Vector2(0.72f, -0.22f), new Vector2(-0.72f, 0.22f), new Vector2(0.72f, 0.22f) })
            {
                b.Log(new Vector3(l.x, 0f, l.y), new Vector3(l.x, 0.36f, l.y), 0.04f, TownSwatch.WoodDark, 4);
            }
        }

        /// <summary>A bazaar stall with a suzani-striped awning and goods by variant (fruit, cloth/spice, the caravaneer's copper).</summary>
        private static void Stall(TownMeshBuilder b, int variant, TownStyle style)
        {
            b.Box(new Vector3(0f, 0.45f, 0f), new Vector3(2.0f, 0.9f, 0.9f), TownSwatch.Walnut, 0f, TownSwatch.WoodLight);
            b.Box(new Vector3(0f, 0.6f, -0.46f), new Vector3(1.9f, 0.5f, 0.02f), variant == 1 ? TownSwatch.ClothIndigo : TownSwatch.ClothRed, 0f);
            foreach (Vector2 p in new[] { new Vector2(-0.95f, -0.5f), new Vector2(0.95f, -0.5f), new Vector2(-0.95f, 0.5f), new Vector2(0.95f, 0.5f) })
            {
                b.Log(new Vector3(p.x, 0f, p.y), new Vector3(p.x, p.y < 0f ? 2.05f : 2.35f, p.y), 0.04f, TownSwatch.WoodDark, 4);
            }

            // Striped awning.
            for (int i = 0; i < 6; i++)
            {
                float x0 = -1.05f + i * 0.35f;
                TownSwatch c = (i & 1) == 0 ? (variant == 1 ? TownSwatch.ClothYellow : TownSwatch.ClothWhite) : (variant == 2 ? TownSwatch.ClothIndigo : TownSwatch.Turquoise);
                b.Plate(new[] { new Vector3(x0, 2.05f, -0.75f), new Vector3(x0, 2.4f, 0.6f), new Vector3(x0 + 0.35f, 2.4f, 0.6f), new Vector3(x0 + 0.35f, 2.05f, -0.75f) }, 0.03f, c, c);
            }

            switch (variant)
            {
                case 0:
                    for (int i = 0; i < 5; i++)
                    {
                        b.Crown(new Vector3(-0.75f + i * 0.37f, 1.0f, -0.1f), new Vector3(0.3f, 0.24f, 0.3f), i % 3 == 0 ? TownSwatch.Fruit : i % 3 == 1 ? TownSwatch.Vine : TownSwatch.ClothRed);
                    }

                    break;
                case 1:
                    for (int i = 0; i < 4; i++)
                    {
                        b.Cylinder(new Vector3(-0.6f + i * 0.4f, 0.9f, 0f), 0.15f, 0.12f, 0.16f, 7, TownSwatch.Thatch, i == 0 ? TownSwatch.ClothRed : i == 1 ? TownSwatch.ClothYellow : i == 2 ? TownSwatch.Aloe : TownSwatch.Fruit);
                    }

                    break;
                default:
                    for (int i = 0; i < 4; i++)
                    {
                        b.Cylinder(new Vector3(-0.6f + i * 0.4f, 0.9f, 0f), 0.13f, 0.1f, 0.22f + (i & 1) * 0.1f, 8, TownSwatch.Copper, TownSwatch.Copper);
                    }

                    b.Log(new Vector3(-0.9f, 1.25f, 0.4f), new Vector3(0.9f, 1.25f, 0.4f), 0.11f, TownSwatch.ClothYellow, 6);
                    break;
            }
        }

        private static void FlowerBed(TownMeshBuilder b, int variant)
        {
            b.Box(new Vector3(0f, 0.08f, 0f), new Vector3(1.3f, 0.16f, 0.55f), TownSwatch.Brick, 0f, TownSwatch.DirtDark);
            for (int i = 0; i < 4; i++)
            {
                TownSwatch c = variant == 1 ? TownSwatch.Vine : (i & 1) == 0 ? TownSwatch.Tamarisk : TownSwatch.ClothYellow;
                b.Crown(new Vector3(-0.45f + i * 0.3f, 0.28f, 0f), new Vector3(0.28f, 0.26f, 0.28f), c);
            }
        }

        /// <summary>The Relogio de Zurvan: a round stone dais with hour stones and a bronze gnomon.</summary>
        private static void Sundial(TownMeshBuilder b)
        {
            b.Cylinder(Vector3.zero, 0.7f, 0.65f, 0.25f, 12, TownSwatch.Stone, TownSwatch.LimeWhite);
            b.Cylinder(Vector3.up * 0.25f, 0.45f, 0.42f, 0.35f, 12, TownSwatch.Stone, TownSwatch.Paving);
            for (int i = 0; i < 12; i++)
            {
                float a = i / 12f * Mathf.PI * 2f;
                b.Box(new Vector3(Mathf.Cos(a) * 0.36f, 0.62f, Mathf.Sin(a) * 0.36f), new Vector3(0.05f, 0.03f, 0.05f), TownSwatch.Gold, 0f);
            }

            b.Plate(new[] { new Vector3(0f, 0.6f, -0.3f), new Vector3(0f, 1.05f, 0.2f), new Vector3(0.04f, 1.05f, 0.2f), new Vector3(0.04f, 0.6f, -0.3f) }, 0.02f, TownSwatch.Gold, TownSwatch.Copper);
        }

        /// <summary>Tandir: the clay oven (a squat dome with its mouth), on a little brick base.</summary>
        private static void Tandir(TownMeshBuilder b)
        {
            b.Box(new Vector3(0f, 0.15f, 0f), new Vector3(1.2f, 0.3f, 1.2f), TownSwatch.Brick, 0f);
            b.Cylinder(new Vector3(0f, 0.3f, 0f), 0.5f, 0.42f, 0.4f, 10, TownSwatch.AdobeOchre);
            b.Cylinder(new Vector3(0f, 0.7f, 0f), 0.42f, 0.18f, 0.32f, 10, TownSwatch.AdobeOchre, TownSwatch.Black);
            b.Box(new Vector3(0f, 0.55f, -0.44f), new Vector3(0.3f, 0.22f, 0.04f), TownSwatch.Ember, 0f);
        }

        /// <summary>The vine trellis (latada): four carved posts, a grid of poles, vine crowns and grape bunches.</summary>
        private static void Trellis(TownMeshBuilder b, float s)
        {
            float hx = 2.4f * s;
            float hz = 1.6f * s;
            const float h = 2.5f;
            foreach (Vector2 p in new[] { new Vector2(-hx, -hz), new Vector2(hx, -hz), new Vector2(-hx, hz), new Vector2(hx, hz) })
            {
                CarvedColumn(b, new Vector3(p.x, 0f, p.y), h, TownSwatch.Walnut, TownSwatch.Stone);
            }

            for (float x = -hx; x <= hx + 0.01f; x += hx * 0.5f)
            {
                b.Log(new Vector3(x, h, -hz - 0.2f), new Vector3(x, h, hz + 0.2f), 0.05f, TownSwatch.WoodDark, 4);
            }

            for (int i = 0; i < 6; i++)
            {
                float x = -hx + (i + 0.5f) * hx * 2f / 6f;
                b.Crown(new Vector3(x, h + 0.25f, (i & 1) == 0 ? -0.4f : 0.5f), new Vector3(1.4f, 0.45f, 1.9f) * s, (i % 3) == 0 ? TownSwatch.Vine : TownSwatch.Poplar);
                b.Blob(new Vector3(x + 0.2f, h - 0.25f, (i & 1) == 0 ? 0.3f : -0.6f), new Vector3(0.16f, 0.3f, 0.16f), TownSwatch.Violet);
            }
        }

        /// <summary>A cluster of copper and clay jugs (cantaros).</summary>
        public static void Jugs(TownMeshBuilder b, Vector3 at = default)
        {
            for (int i = 0; i < 3; i++)
            {
                var p = at + new Vector3(i * 0.32f - 0.3f, 0f, (i & 1) * 0.25f);
                TownSwatch c = i == 1 ? TownSwatch.AdobeOchre : TownSwatch.Copper;
                b.Cylinder(p, 0.12f, 0.2f, 0.25f, 8, c);
                b.Cylinder(p + Vector3.up * 0.25f, 0.2f, 0.08f, 0.22f, 8, c);
                b.Cylinder(p + Vector3.up * 0.47f, 0.07f, 0.09f, 0.1f, 8, c, TownSwatch.Black);
            }
        }

        /// <summary>A tall carved stela with false windows and a door (Aksum style), for Litema's plaza.</summary>
        private static void Stela(TownMeshBuilder b, float s)
        {
            float h = 5.5f * s;
            b.Box(new Vector3(0f, 0.15f, 0f), new Vector3(1.6f, 0.3f, 1.2f), TownSwatch.Stone, 0f);
            b.Box(new Vector3(0f, 0.3f + h * 0.5f, 0f), new Vector3(0.9f, h, 0.55f), TownSwatch.Stone, 0f);
            b.Cylinder(new Vector3(0f, 0.3f + h, 0f), 0.45f, 0.1f, 0.5f, 4, TownSwatch.Stone, null, 0.5f);
            b.Box(new Vector3(0f, 0.9f, -0.29f), new Vector3(0.34f, 0.7f, 0.03f), TownSwatch.Basalt, 0f);
            for (int i = 0; i < 5; i++)
            {
                float y = 1.8f + i * (h - 2.2f) / 5f;
                b.Box(new Vector3(-0.2f, y, -0.29f), new Vector3(0.22f, 0.3f, 0.03f), TownSwatch.StoneDark, 0f);
                b.Box(new Vector3(0.2f, y, -0.29f), new Vector3(0.22f, 0.3f, 0.03f), TownSwatch.StoneDark, 0f);
                b.Box(new Vector3(0f, y - 0.25f, -0.3f), new Vector3(0.95f, 0.06f, 0.04f), TownSwatch.Stone, 0f);
            }
        }

        // ------------------------------------------------------------------
        // Vegetation (by region)
        // ------------------------------------------------------------------

        public static void Tree(TownMeshBuilder b, TownVegetation vegetation, int variant, float s)
        {
            switch (vegetation)
            {
                case TownVegetation.Desert:
                    if (variant == 0 || variant == 3)
                    {
                        DatePalm(b, s);
                    }
                    else
                    {
                        Feathery(b, s, variant == 1 ? TownSwatch.Tamarisk : TownSwatch.Vine);
                    }

                    return;
                case TownVegetation.Mountain:
                    if (variant == 1)
                    {
                        TreeAloe(b, s);
                    }
                    else
                    {
                        Cedar(b, s);
                    }

                    return;
                case TownVegetation.Caves:
                    GlowMushroom(b, s, variant);
                    return;
            }

            // Zarafshan tugai: 0 poplar (green), 1 gold poplar, 2 tamarisk, 3 orchard tree (apricot / pomegranate).
            switch (variant)
            {
                case 0:
                case 1:
                    Poplar(b, s, variant == 1 ? TownSwatch.LeafGold : TownSwatch.Poplar);
                    break;
                case 2:
                    Feathery(b, s, TownSwatch.Tamarisk);
                    break;
                default:
                    Orchard(b, s);
                    break;
            }
        }

        private static void Poplar(TownMeshBuilder b, float s, TownSwatch leaf)
        {
            b.Cylinder(Vector3.zero, 0.22f * s, 0.12f * s, 2.2f * s, 6, TownSwatch.Trunk);
            b.Crown(new Vector3(0f, 3.6f * s, 0f), new Vector3(1.9f, 3.6f, 1.9f) * s, leaf, 10f);
            b.Crown(new Vector3(0.15f * s, 5.4f * s, 0.1f * s), new Vector3(1.3f, 2.4f, 1.3f) * s, leaf == TownSwatch.Poplar ? TownSwatch.LeafDark : TownSwatch.Fruit, 40f);
        }

        private static void Feathery(TownMeshBuilder b, float s, TownSwatch leaf)
        {
            b.Log(Vector3.zero, new Vector3(0.3f, 1.4f, 0.1f) * s, 0.12f * s, TownSwatch.Trunk);
            b.Log(new Vector3(0.3f, 1.4f, 0.1f) * s, new Vector3(-0.2f, 2.2f, 0.3f) * s, 0.08f * s, TownSwatch.Trunk);
            b.Crown(new Vector3(0.2f, 2.3f, 0.1f) * s, new Vector3(2.2f, 1.4f, 2.0f) * s, leaf, 15f);
            b.Crown(new Vector3(-0.5f, 2.0f, -0.3f) * s, new Vector3(1.4f, 1.0f, 1.4f) * s, leaf == TownSwatch.Tamarisk ? TownSwatch.Poplar : TownSwatch.LeafDark, 60f);
        }

        private static void Orchard(TownMeshBuilder b, float s)
        {
            b.Cylinder(Vector3.zero, 0.18f * s, 0.12f * s, 1.3f * s, 6, TownSwatch.Trunk);
            b.Crown(new Vector3(0f, 2.1f, 0f) * s, new Vector3(2.6f, 1.8f, 2.4f) * s, TownSwatch.LeafDark, 20f);
            b.Crown(new Vector3(0.5f, 2.6f, 0.3f) * s, new Vector3(1.6f, 1.2f, 1.5f) * s, TownSwatch.Poplar, 70f);
            for (int i = 0; i < 6; i++)
            {
                float a = i * 1.05f;
                b.Blob(new Vector3(Mathf.Cos(a) * 1.05f, 1.75f + (i & 1) * 0.5f, Mathf.Sin(a) * 1.0f) * s, new Vector3(0.18f, 0.18f, 0.18f) * s, (i % 3) == 0 ? TownSwatch.ClothRed : TownSwatch.Fruit);
            }
        }

        private static void DatePalm(TownMeshBuilder b, float s)
        {
            Vector3 top = new Vector3(0.4f, 4.2f, 0.2f) * s;
            b.Log(Vector3.zero, top * 0.5f + new Vector3(0.3f, 0f, 0f) * s, 0.2f * s, TownSwatch.Trunk);
            b.Log(top * 0.5f + new Vector3(0.3f, 0f, 0f) * s, top, 0.16f * s, TownSwatch.Trunk);
            for (int i = 0; i < 7; i++)
            {
                float a = i / 7f * Mathf.PI * 2f;
                var tip = top + new Vector3(Mathf.Cos(a) * 1.9f, -0.9f, Mathf.Sin(a) * 1.9f) * s;
                var mid = top + new Vector3(Mathf.Cos(a) * 1.0f, 0.15f, Mathf.Sin(a) * 1.0f) * s;
                Vector3 side = new Vector3(-Mathf.Sin(a), 0f, Mathf.Cos(a)) * 0.35f * s;
                b.TwoSidedQuad(top, mid + side, tip, mid - side, TownSwatch.Vine);
            }

            b.Blob(top + Vector3.down * 0.3f * s, new Vector3(0.5f, 0.4f, 0.5f) * s, TownSwatch.Fruit);
        }

        private static void Cedar(TownMeshBuilder b, float s)
        {
            b.Log(Vector3.zero, new Vector3(-0.3f, 1.8f, 0.2f) * s, 0.2f * s, TownSwatch.Trunk);
            b.Log(new Vector3(-0.3f, 1.8f, 0.2f) * s, new Vector3(0.4f, 3.0f, 0f) * s, 0.13f * s, TownSwatch.Trunk);
            b.Plate(new[] { new Vector3(-1.4f, 2.4f, -1f) * s, new Vector3(-1.2f, 2.6f, 1.1f) * s, new Vector3(1.2f, 2.5f, 1.0f) * s, new Vector3(1.0f, 2.3f, -1.2f) * s }, 0.35f * s, TownSwatch.Pine, TownSwatch.LeafDark);
            b.Plate(new[] { new Vector3(-0.6f, 3.3f, -0.6f) * s, new Vector3(-0.5f, 3.4f, 0.7f) * s, new Vector3(1.3f, 3.2f, 0.5f) * s, new Vector3(1.1f, 3.1f, -0.7f) * s }, 0.3f * s, TownSwatch.Pine, TownSwatch.LeafDark);
        }

        private static void TreeAloe(TownMeshBuilder b, float s)
        {
            b.Log(Vector3.zero, new Vector3(0f, 1.6f, 0f) * s, 0.16f * s, TownSwatch.Trunk);
            for (int i = 0; i < 3; i++)
            {
                float a = i * 2.1f;
                Vector3 tip = new Vector3(Mathf.Cos(a) * 0.7f, 2.3f, Mathf.Sin(a) * 0.7f) * s;
                b.Log(new Vector3(0f, 1.5f, 0f) * s, tip, 0.09f * s, TownSwatch.Trunk);
                b.Crown(tip, new Vector3(0.9f, 0.45f, 0.9f) * s, TownSwatch.Vine);
                b.Log(tip, tip + Vector3.up * 0.7f * s, 0.05f * s, TownSwatch.Aloe, 4);
            }
        }

        private static void GlowMushroom(TownMeshBuilder b, float s, int variant)
        {
            if (variant == 2)
            {
                // A stalagmite with a fluorescent moss skirt.
                b.Cylinder(Vector3.zero, 0.6f * s, 0f, 3.2f * s, 6, TownSwatch.Basalt);
                b.Cylinder(Vector3.zero, 0.75f * s, 0.6f * s, 0.25f * s, 6, TownSwatch.Moss, TownSwatch.Moss);
                return;
            }

            float h = (variant == 3 ? 1.4f : 2.6f) * s;
            b.Cylinder(Vector3.zero, 0.22f * s, 0.15f * s, h, 7, TownSwatch.ClothWhite);
            b.Cylinder(new Vector3(0f, h, 0f), 1.3f * s * (variant == 3 ? 0.6f : 1f), 0.2f * s, 0.6f * s, 9, TownSwatch.MushroomBlue, TownSwatch.MushroomBlue);
            b.Cylinder(new Vector3(0f, h - 0.05f, 0f), 1.25f * s * (variant == 3 ? 0.6f : 1f), 1.3f * s * (variant == 3 ? 0.6f : 1f), 0.05f, 9, TownSwatch.Indigo);
        }

        // ------------------------------------------------------------------
        // Villagers (static figures for the keepers)
        // ------------------------------------------------------------------

        /// <summary>
        /// A keeper figure (~1.75 m, ~190 triangles): boots, a long robe (chapan) belted, sleeves, head with hair and a
        /// cap or a headscarf. Colours from the resident id (stable); faces −z.
        /// </summary>
        public static void Villager(TownMeshBuilder b, string residentId, TownStyle style)
        {
            string key = residentId ?? "villager";
            TownSwatch[] robes = { TownSwatch.ClothIndigo, TownSwatch.ClothRed, TownSwatch.ClothGreen, TownSwatch.Turquoise, TownSwatch.Violet, TownSwatch.AdobeOchre };
            TownSwatch robe = TownStyle.Pick(robes, key, 61);
            TownSwatch trim = TownStyle.Pick(new[] { TownSwatch.ClothYellow, TownSwatch.ClothWhite, TownSwatch.Gold }, key, 62);
            TownSwatch skin = TownStyle.Hash01(key, 63) < 0.5f ? TownSwatch.Skin : TownSwatch.SkinDark;
            bool scarf = (TownStyle.Hash(key, 64) & 1) == 0;

            foreach (float x in new[] { -0.11f, 0.11f })
            {
                b.Cylinder(new Vector3(x, 0f, 0f), 0.08f, 0.07f, 0.22f, 6, TownSwatch.WoodDark, TownSwatch.WoodDark);
            }

            b.Cylinder(new Vector3(0f, 0.2f, 0f), 0.34f, 0.24f, 0.8f, 8, robe);
            b.Cylinder(new Vector3(0f, 0.98f, 0f), 0.25f, 0.25f, 0.08f, 8, trim, trim);
            b.Cylinder(new Vector3(0f, 1.06f, 0f), 0.25f, 0.2f, 0.36f, 8, robe, robe);
            b.Log(new Vector3(-0.24f, 1.36f, 0f), new Vector3(-0.3f, 0.95f, -0.1f), 0.07f, robe);
            b.Log(new Vector3(0.24f, 1.36f, 0f), new Vector3(0.3f, 0.95f, -0.1f), 0.07f, robe);
            b.Blob(new Vector3(-0.3f, 0.92f, -0.12f), new Vector3(0.1f, 0.1f, 0.1f), skin);
            b.Blob(new Vector3(0.3f, 0.92f, -0.12f), new Vector3(0.1f, 0.1f, 0.1f), skin);
            b.Cylinder(new Vector3(0f, 1.42f, 0f), 0.07f, 0.07f, 0.08f, 6, skin);
            b.Crown(new Vector3(0f, 1.6f, 0f), new Vector3(0.28f, 0.32f, 0.28f), skin);
            if (scarf)
            {
                b.Crown(new Vector3(0f, 1.66f, 0.04f), new Vector3(0.33f, 0.32f, 0.32f), trim);
                b.Cylinder(new Vector3(0f, 1.3f, 0.12f), 0.2f, 0.12f, 0.35f, 6, trim);
            }
            else
            {
                b.Crown(new Vector3(0f, 1.7f, 0.03f), new Vector3(0.3f, 0.16f, 0.3f), TownSwatch.Hair);
                b.Cylinder(new Vector3(0f, 1.72f, 0f), 0.17f, 0.15f, 0.14f, 8, robe == TownSwatch.ClothIndigo ? TownSwatch.ClothRed : TownSwatch.ClothIndigo, TownSwatch.Gold);
            }
        }
    }
}
