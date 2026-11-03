using UnityEngine;

namespace Rapadura.Gameplay.World.Cities.Town
{
    /// <summary>
    /// The pieces of the Mercado dos Túneis, the kobolds' market at the crossing of the four galleries of the Grutas do Limiar
    /// (Docs/Mapas100.md §3.3 and map 22): the stalls set into arched basalt niches (<see cref="TunnelStallHouse"/>: a round
    /// arch, shelves of bottles, a counter, a striped canvas awning on copper poles, bottle lanterns and a copper air pipe up
    /// the rock), mine-cart rails and carts, pulleys, the air pipes with a fan turning inside a tube (they "pull" the Ar
    /// Sereno down from the cracks), the small niche stalls of the plaza and the round tunnel gate at the streets' ends;
    /// Miwnay's bottle racks and the tinkerers' benches. Same rules as the rest of the kit: flat colours from
    /// <see cref="TownPalette"/> (basalt, copper, sail canvas and the row-9 bottle glass and Ar Sereno), low-poly, deterministic.
    /// </summary>
    public static partial class TownKit
    {
        /// <summary>Footprint of a <see cref="CityPropKind.MineCart"/> (local x along its rails × z, × its scale): its collider box.</summary>
        public static readonly Vector2 MineCartSize = new Vector2(1.7f, 1.1f);

        /// <summary>Radius of an <see cref="CityPropKind.AirPipe"/>'s fan housing (× its scale): its collider.</summary>
        public const float AirPipeRadius = 0.6f;

        /// <summary>Footprint of a <see cref="CityPropKind.NicheStall"/> (local x × z, × its scale): its collider box.</summary>
        public static readonly Vector2 NicheStallSize = new Vector2(2.6f, 1.4f);

        /// <summary>Footprint of a <see cref="CityPropKind.Pulley"/> (local x × z, × its scale): its collider box.</summary>
        public static readonly Vector2 PulleySize = new Vector2(2.0f, 0.8f);

        /// <summary>Half the gauge of <see cref="CityPropKind.MineRails"/>.</summary>
        public const float RailHalfGauge = 0.42f;

        // ------------------------------------------------------------------
        // The stall in its niche
        // ------------------------------------------------------------------

        /// <summary>
        /// A <see cref="TownBuildingSpec.TunnelStall"/> house (local frame, front −z): the basalt of the tunnel behind, the
        /// facade with a big round arch (the niche: dark inside, shelves of glowing bottles, a counter beside the door), copper
        /// voussoirs round the arch, a striped canvas awning over the deck on copper poles (at
        /// <see cref="TownBuildingBuilder.PorchPostsX"/>, the colliders use the same list), bottle lanterns under its edge and a
        /// copper air pipe climbing the rock beside the stall.
        /// </summary>
        public static void TunnelStallHouse(TownMeshBuilder b, TownBuildingSpec spec, float hw, float zFront, float zFacade, float zBack, float top)
        {
            float y0 = TownBuildingBuilder.FloorY;
            TownSwatch rock = spec.WallColor;
            b.Crown(new Vector3(0f, top * 0.9f, (zFacade + zBack) * 0.5f + 0.5f), new Vector3(spec.Width + 1.6f, top * 1.8f, spec.Depth + 1.6f), rock);
            b.Crown(new Vector3(hw * 0.3f, top * 1.3f, zBack - 1.4f), new Vector3(spec.Width * 0.8f, top * 1.1f, spec.Depth * 0.6f), TownSwatch.StoneDark, 40f);
            b.BoxMinMax(new Vector3(-hw, y0, zFacade), new Vector3(hw, top + 0.6f, zFacade + 0.6f), rock);

            float doorX = spec.DoorOffset * spec.Width;
            float archW = Mathf.Min(spec.Width - 1.2f, 3.6f);
            float spring = y0 + Mathf.Max(1.6f, top - y0 - archW * 0.5f - 0.15f);
            b.With(new Vector3(0f, 0f, zFacade), 0f, () =>
            {
                // The niche: dark, with two shelves of bottles behind the opening.
                b.BoxMinMax(new Vector3(-archW * 0.5f, y0, -0.02f), new Vector3(archW * 0.5f, spring, 0f), TownSwatch.Black);
                RoundArchFill(b, 0f, spring, archW * 0.5f, -0.02f, TownSwatch.Black);
                foreach (float sy in new[] { y0 + 1.1f, y0 + 1.75f })
                {
                    b.BoxMinMax(new Vector3(-archW * 0.5f + 0.1f, sy - 0.04f, -0.06f), new Vector3(archW * 0.5f - 0.1f, sy, -0.03f), TownSwatch.WoodDark);
                    for (float bx = -archW * 0.5f + 0.25f; bx < archW * 0.5f - 0.15f; bx += 0.26f)
                    {
                        if (Mathf.Abs(bx - doorX) < 0.55f && sy < y0 + 1.5f)
                        {
                            continue;
                        }

                        int k = Mathf.RoundToInt(bx * 10f + sy * 7f);
                        TownSwatch glass = (k % 3) == 0 ? TownSwatch.SereneAir : (k % 3) == 1 ? TownSwatch.GlassGreen : TownSwatch.GlassAmber;
                        b.Box(new Vector3(bx, sy + 0.13f, -0.08f), new Vector3(0.1f, 0.26f, 0.08f), glass, 0f);
                    }
                }

                // Copper voussoirs round the arch and a keystone.
                const int stones = 9;
                for (int i = 0; i <= stones; i++)
                {
                    float a = Mathf.PI * i / stones;
                    var p = new Vector3(Mathf.Cos(a) * (archW * 0.5f + 0.12f), spring + Mathf.Sin(a) * (archW * 0.5f + 0.12f), -0.06f);
                    b.Box(p, new Vector3(0.26f, 0.2f, 0.12f), (i % 2) == 0 ? spec.TrimColor : TownSwatch.StoneDark, 0f);
                }

                b.BoxMinMax(new Vector3(-archW * 0.5f - 0.2f, y0, -0.08f), new Vector3(-archW * 0.5f, spring, 0f), TownSwatch.StoneDark);
                b.BoxMinMax(new Vector3(archW * 0.5f, y0, -0.08f), new Vector3(archW * 0.5f + 0.2f, spring, 0f), TownSwatch.StoneDark);

                // The counter on the keeper's side of the door.
                float side = TownBuildingBuilder.KeeperSide(spec);
                float cx0 = side > 0f ? doorX + 0.55f : -archW * 0.5f + 0.05f;
                float cx1 = side > 0f ? archW * 0.5f - 0.05f : doorX - 0.55f;
                if (cx1 - cx0 > 0.3f)
                {
                    b.BoxMinMax(new Vector3(cx0, y0, -0.35f), new Vector3(cx1, y0 + 0.95f, -0.02f), TownSwatch.WoodDark, spec.JoineryColor);
                }
            });

            if (spec.PorchDepth > 0.01f)
            {
                float deck = CitySolids.DeckHeight;
                b.BoxMinMax(new Vector3(-hw, 0f, zFront), new Vector3(hw, deck, zFacade), TownSwatch.StoneDark, TownSwatch.Cobble);
                float z = zFront + TownBuildingBuilder.PostInset;
                float awningLow = Mathf.Min(top - 0.2f, y0 + 2.55f);
                float awningHigh = Mathf.Min(top + 0.3f, spring + archW * 0.5f + 0.2f);
                foreach (float x in TownBuildingBuilder.PorchPostsX(spec))
                {
                    b.Log(new Vector3(x, deck, z), new Vector3(x, awningLow, z), 0.06f, TownSwatch.Copper, 6);
                    b.Box(new Vector3(x, awningLow - 0.35f, z - 0.12f), new Vector3(0.16f, 0.22f, 0.16f), TownSwatch.GlassAmber, 0f);
                }

                // The striped canvas awning: from the facade over the deck down to the poles.
                int stripes = Mathf.Max(3, Mathf.RoundToInt(spec.Width / 0.7f));
                float sw = (spec.Width + 0.2f) / stripes;
                for (int i = 0; i < stripes; i++)
                {
                    float x0 = -hw - 0.1f + i * sw;
                    float x1 = x0 + sw;
                    TownSwatch cloth = (i % 2) == 0 ? TownSwatch.Sail : spec.AccentColor == TownSwatch.GlassAmber ? TownSwatch.ClothRed : spec.AccentColor;
                    b.TwoSidedQuad(new Vector3(x0, awningHigh, zFacade + 0.02f), new Vector3(x1, awningHigh, zFacade + 0.02f), new Vector3(x1, awningLow, z - 0.15f), new Vector3(x0, awningLow, z - 0.15f), cloth);
                }

                b.Log(new Vector3(-hw - 0.1f, awningLow, z - 0.15f), new Vector3(hw + 0.1f, awningLow, z - 0.15f), 0.04f, TownSwatch.Copper, 5);
            }

            // The copper air pipe up the rock (on the side away from the keeper), its little fan cowl at the top.
            float px = -TownBuildingBuilder.KeeperSide(spec) * (hw - 0.3f);
            b.Log(new Vector3(px, 0f, zFacade + 0.25f), new Vector3(px, top + 2.2f, zFacade + 0.25f), 0.12f, TownSwatch.Copper, 6);
            b.Log(new Vector3(px, top + 1.4f, zFacade + 0.1f), new Vector3(px, top + 1.4f, zFacade + 0.6f), 0.3f, TownSwatch.Copper, 8, TownSwatch.Black);
            b.Box(new Vector3(px, 1.1f, zFacade + 0.08f), new Vector3(0.3f, 0.06f, 0.3f), TownSwatch.Iron, 0f);
        }

        /// <summary>A half disc (the round head of an arch) in the face plane at depth <paramref name="z"/>, facing −z.</summary>
        public static void RoundArchFill(TownMeshBuilder b, float x, float y, float radius, float z, TownSwatch swatch)
        {
            const int steps = 8;
            var c = new Vector3(x, y, z);
            for (int i = 0; i < steps; i++)
            {
                float a0 = Mathf.PI * i / steps;
                float a1 = Mathf.PI * (i + 1) / steps;
                var p0 = new Vector3(x + Mathf.Cos(a0) * radius, y + Mathf.Sin(a0) * radius, z);
                var p1 = new Vector3(x + Mathf.Cos(a1) * radius, y + Mathf.Sin(a1) * radius, z);
                b.TriangleFacing(c, p0, p1, Vector3.back, swatch);
            }
        }

        /// <summary>The round tunnel gate: two basalt piers carrying a ring of voussoirs, a copper pipe over the top and two bottle lamps.</summary>
        public static void TunnelGate(TownMeshBuilder b, Vector3 a, Vector3 c)
        {
            Vector3 along = (c - a).normalized;
            Vector3 mid = (a + c) * 0.5f;
            float r = Vector3.Distance(a, c) * 0.5f;
            const float pier = 2.2f;
            foreach (Vector3 p in new[] { a, c })
            {
                b.Box(p + Vector3.up * pier * 0.5f, new Vector3(0.6f, pier, 0.6f), TownSwatch.Basalt, 0f, TownSwatch.StoneDark);
                Vector3 inward = p == a ? along : -along;
                b.Box(p + inward * 0.42f + Vector3.up * 1.7f, new Vector3(0.2f, 0.3f, 0.2f), TownSwatch.GlassGreen, 0f);
            }

            const int stones = 11;
            for (int i = 0; i <= stones; i++)
            {
                float t = Mathf.PI * i / stones;
                Vector3 p = mid - along * Mathf.Cos(t) * r + Vector3.up * (pier + Mathf.Sin(t) * r);
                b.Box(p, new Vector3(0.5f, 0.45f, 0.6f), (i % 3) == 1 ? TownSwatch.Copper : TownSwatch.Basalt, Mathf.Atan2(along.x, along.z) * Mathf.Rad2Deg);
            }

            b.Log(a + Vector3.up * (pier + r + 0.5f) - along * 0.6f, c + Vector3.up * (pier + r + 0.5f) + along * 0.6f, 0.13f, TownSwatch.Copper, 6);
            b.Box(mid + Vector3.up * (pier + r - 0.1f), new Vector3(0.22f, 0.32f, 0.22f), TownSwatch.GlassAmber, 0f);
        }

        // ------------------------------------------------------------------
        // Props
        // ------------------------------------------------------------------

        /// <summary>Mine-cart rails from the origin to <paramref name="d"/>: wooden sleepers and two iron rails. Walkable.</summary>
        private static void MineRails(TownMeshBuilder b, Vector2 d)
        {
            float length = d.magnitude;
            if (length < 0.3f)
            {
                return;
            }

            b.With(Vector3.zero, YawAlong(d / length), () =>
            {
                for (float x = 0.3f; x < length - 0.1f; x += 0.7f)
                {
                    b.BoxMinMax(new Vector3(x - 0.12f, 0f, -0.68f), new Vector3(x + 0.12f, 0.07f, 0.68f), TownSwatch.WoodDark);
                }

                foreach (float z in new[] { -RailHalfGauge, RailHalfGauge })
                {
                    b.BoxMinMax(new Vector3(0f, 0.07f, z - 0.04f), new Vector3(length, 0.13f, z + 0.04f), TownSwatch.Iron);
                }
            });
        }

        /// <summary>A mine cart (vagonete) on a short stretch of rail along local x: a tapered copper-banded tub on four wheels, loaded with ore (variant 0) or bottles of Ar Sereno (1).</summary>
        private static void MineCart(TownMeshBuilder b, float s, int variant)
        {
            float hl = MineCartSize.x * 0.5f * s;
            float hw = MineCartSize.y * 0.5f * s;
            MineRails(b, new Vector2(-hl - 0.3f, 0f));
            MineRails(b, new Vector2(hl + 0.3f, 0f));
            foreach (float x in new[] { -hl * 0.6f, hl * 0.6f })
            {
                foreach (float z in new[] { -RailHalfGauge, RailHalfGauge })
                {
                    b.Log(new Vector3(x, 0.25f, z - 0.04f * Mathf.Sign(z)), new Vector3(x, 0.25f, z + 0.06f * Mathf.Sign(z)), 0.16f, TownSwatch.Iron, 8);
                }
            }

            b.BoxMinMax(new Vector3(-hl * 0.85f, 0.3f, -hw * 0.8f), new Vector3(hl * 0.85f, 0.4f, hw * 0.8f), TownSwatch.Iron);
            b.Cylinder(new Vector3(0f, 0.4f, 0f), hw * 1.3f, hw * 1.55f, 0.6f * s, 4, TownSwatch.WoodDark, null, 0.5f);
            b.Cylinder(new Vector3(0f, 0.95f * s, 0f), hw * 1.52f, hw * 1.56f, 0.08f, 4, TownSwatch.Copper, TownSwatch.Copper, 0.5f);
            if (variant == 1)
            {
                for (int i = 0; i < 6; i++)
                {
                    b.Box(new Vector3(-0.35f + (i % 3) * 0.35f, 1.1f * s, -0.15f + (i / 3) * 0.3f), new Vector3(0.12f, 0.3f, 0.12f), (i & 1) == 0 ? TownSwatch.SereneAir : TownSwatch.GlassGreen, 0f);
                }
            }
            else
            {
                b.Crown(new Vector3(0f, 0.95f * s, 0f), new Vector3(1.2f, 0.45f, 0.75f) * s, TownSwatch.StoneDark);
                b.Crown(new Vector3(0.2f, 1.1f * s, 0.1f), new Vector3(0.35f, 0.25f, 0.3f) * s, TownSwatch.Indigo, 30f);
                b.Crown(new Vector3(-0.25f, 1.05f * s, -0.1f), new Vector3(0.3f, 0.22f, 0.3f) * s, TownSwatch.Copper, 60f);
            }
        }

        /// <summary>An air pipe: a copper pipe rising into the rock, a tube housing at head height with a four-bladed fan inside, brackets and a valve wheel.</summary>
        private static void AirPipe(TownMeshBuilder b, float s)
        {
            b.Cylinder(Vector3.zero, 0.38f * s, 0.32f * s, 0.3f, 8, TownSwatch.Basalt, TownSwatch.StoneDark);
            b.Cylinder(new Vector3(0f, 0.3f, 0f), 0.2f * s, 0.2f * s, 5.6f * s, 8, TownSwatch.Copper);
            float y = 2.3f * s;
            float r = AirPipeRadius * s - 0.05f;
            // The tube housing (axis along local z: it faces the camera) and the fan inside it.
            b.Log(new Vector3(0f, y, -0.3f * s), new Vector3(0f, y, 0.3f * s), r, TownSwatch.Copper, 10, TownSwatch.Black);
            b.Log(new Vector3(0f, y, -0.32f * s), new Vector3(0f, y, -0.3f * s), r + 0.04f, TownSwatch.Iron, 10);
            for (int i = 0; i < 4; i++)
            {
                float a = i * 90f + 20f;
                Vector3 blade = Quaternion.Euler(0f, 0f, a) * new Vector3(0f, r * 0.5f, 0f);
                b.Box(new Vector3(0f, y, -0.33f * s) + blade, new Vector3(0.14f, r * 0.85f, 0.02f), TownSwatch.Brick, 0f);
            }

            b.Cylinder(new Vector3(0f, y - 0.05f, -0.34f * s), 0.07f, 0.07f, 0.1f, 6, TownSwatch.Iron, TownSwatch.Iron);
            foreach (float yy in new[] { 1.0f, 3.6f, 4.8f })
            {
                b.Box(new Vector3(0f, yy * s, 0f), new Vector3(0.55f, 0.08f, 0.55f) * s, TownSwatch.Iron, 45f);
            }

            b.Log(new Vector3(0.22f * s, 1.4f * s, 0f), new Vector3(0.36f * s, 1.4f * s, 0f), 0.16f * s, TownSwatch.ClothRed, 8, TownSwatch.Iron);
            b.Crown(new Vector3(0f, 5.9f * s, 0.1f), new Vector3(1.6f, 0.8f, 1.6f) * s, TownSwatch.Basalt);
        }

        /// <summary>A small market stall in a basalt arch (front −z): side piers, the round head, a back wall, a counter of bottles, a striped awning, two bottle lanterns.</summary>
        private static void NicheStall(TownMeshBuilder b, float s, int variant)
        {
            float hw = NicheStallSize.x * 0.5f * s;
            float hd = NicheStallSize.y * 0.5f * s;
            float spring = 1.9f * s;
            b.BoxMinMax(new Vector3(-hw, 0f, hd - 0.25f), new Vector3(hw, spring + hw, hd), TownSwatch.Basalt, TownSwatch.StoneDark);
            foreach (float x in new[] { -hw, hw - 0.3f })
            {
                b.BoxMinMax(new Vector3(x, 0f, -hd + 0.2f), new Vector3(x + 0.3f, spring, hd), TownSwatch.Basalt, TownSwatch.StoneDark);
            }

            const int stones = 7;
            for (int i = 0; i <= stones; i++)
            {
                float a = Mathf.PI * i / stones;
                var p = new Vector3(Mathf.Cos(a) * (hw - 0.15f), spring + Mathf.Sin(a) * (hw - 0.15f), -hd + 0.35f);
                b.Box(p, new Vector3(0.32f, 0.3f, 0.3f), (i % 3) == 1 ? TownSwatch.Copper : TownSwatch.Basalt, 0f);
            }

            b.BoxMinMax(new Vector3(-hw + 0.3f, 0f, -hd), new Vector3(hw - 0.3f, 0.9f, -hd + 0.45f), TownSwatch.WoodDark, TownSwatch.Sail);
            for (int i = 0; i < 6; i++)
            {
                float x = -hw + 0.55f + i * (hw * 2f - 1.1f) / 5f;
                TownSwatch glass = ((i + variant) % 3) == 0 ? TownSwatch.SereneAir : ((i + variant) % 3) == 1 ? TownSwatch.GlassGreen : TownSwatch.GlassAmber;
                b.Box(new Vector3(x, 1.05f, -hd + 0.22f), new Vector3(0.1f, 0.3f, 0.1f), glass, 0f);
            }

            TownSwatch stripe = (variant % 2) == 0 ? TownSwatch.ClothRed : TownSwatch.ClothIndigo;
            int n = 4;
            for (int i = 0; i < n; i++)
            {
                float x0 = -hw + i * (hw * 2f / n);
                float x1 = x0 + hw * 2f / n;
                b.TwoSidedQuad(new Vector3(x0, spring + 0.2f, -hd + 0.45f), new Vector3(x1, spring + 0.2f, -hd + 0.45f), new Vector3(x1, spring - 0.25f, -hd - 0.3f), new Vector3(x0, spring - 0.25f, -hd - 0.3f), (i % 2) == 0 ? TownSwatch.Sail : stripe);
            }

            foreach (float x in new[] { -hw + 0.15f, hw - 0.15f })
            {
                b.Log(new Vector3(x, spring - 0.25f, -hd - 0.3f), new Vector3(x, spring - 0.55f, -hd - 0.3f), 0.012f, TownSwatch.Iron, 4);
                b.Box(new Vector3(x, spring - 0.7f, -hd - 0.3f), new Vector3(0.16f, 0.24f, 0.16f), TownSwatch.GlassAmber, 0f);
            }
        }

        /// <summary>A pulley frame (along local x): two posts and a beam, the copper wheel, the rope and a bucket of ore.</summary>
        private static void Pulley(TownMeshBuilder b, float s)
        {
            float hx = PulleySize.x * 0.5f * s - 0.1f;
            float h = 3.2f * s;
            foreach (float x in new[] { -hx, hx })
            {
                b.Log(new Vector3(x, 0f, 0f), new Vector3(x, h, 0f), 0.09f, TownSwatch.WoodDark, 6);
                b.Log(new Vector3(x, 0f, -0.35f * s), new Vector3(x, h * 0.55f, 0f), 0.05f, TownSwatch.WoodDark, 4);
            }

            b.Log(new Vector3(-hx - 0.1f, h, 0f), new Vector3(hx + 0.1f, h, 0f), 0.1f, TownSwatch.WoodDark, 5);
            b.Log(new Vector3(0f, h - 0.32f, -0.07f), new Vector3(0f, h - 0.32f, 0.07f), 0.28f * s, TownSwatch.Copper, 10, TownSwatch.Iron);
            b.Log(new Vector3(0f, h - 0.32f, 0f), new Vector3(0f, h, 0f), 0.03f, TownSwatch.Iron, 4);
            b.Log(new Vector3(0.28f * s, h - 0.32f, 0f), new Vector3(0.28f * s, 1.1f * s, 0f), 0.015f, TownSwatch.Thatch, 4);
            b.Log(new Vector3(-0.28f * s, h - 0.32f, 0f), new Vector3(-0.5f * s, 0.9f * s, 0.2f), 0.015f, TownSwatch.Thatch, 4);
            b.Cylinder(new Vector3(0.28f * s, 0.6f * s, 0f), 0.2f * s, 0.24f * s, 0.45f * s, 7, TownSwatch.WoodLight, TownSwatch.StoneDark);
            b.Crown(new Vector3(0.28f * s, 1.05f * s, 0f), new Vector3(0.35f, 0.18f, 0.35f) * s, TownSwatch.Indigo);
        }

        // ------------------------------------------------------------------
        // Dressings
        // ------------------------------------------------------------------

        /// <summary>Miwnay's bottles of Ar Sereno on a stepped rack, a funnel on a crate of corks; the tinkerers' bench of gears and a little fan in a tube.</summary>
        private static void TunnelDressing(TownMeshBuilder b, TownBuildingSpec spec, float x, float y, float z, float zFacade)
        {
            if (spec.Dressing == TownShopDressing.AirBottles)
            {
                for (int step = 0; step < 3; step++)
                {
                    float sy = y + 0.25f + step * 0.32f;
                    float sz = z - 0.55f + step * 0.25f;
                    b.BoxMinMax(new Vector3(x - 0.6f, y, sz - 0.12f), new Vector3(x + 0.6f, sy, sz + 0.12f), TownSwatch.WoodDark, TownSwatch.Sail);
                    for (int i = 0; i < 5; i++)
                    {
                        b.Box(new Vector3(x - 0.48f + i * 0.24f, sy + 0.15f, sz), new Vector3(0.11f, 0.3f, 0.11f), ((i + step) % 2) == 0 ? TownSwatch.SereneAir : TownSwatch.GlassGreen, 0f);
                    }
                }

                b.Box(new Vector3(x + 0.95f, y + 0.22f, z - 0.3f), new Vector3(0.45f, 0.44f, 0.45f), TownSwatch.WoodLight, 10f, TownSwatch.Thatch);
                b.Cylinder(new Vector3(x + 0.95f, y + 0.44f, z - 0.3f), 0.04f, 0.17f, 0.22f, 7, TownSwatch.Copper);
                return;
            }

            // Tinkerer: a bench with gears, a vice, a box of scrap and a little fan in a tube.
            b.BoxMinMax(new Vector3(x - 0.65f, y + 0.75f, z - 0.75f), new Vector3(x + 0.65f, y + 0.82f, z - 0.25f), TownSwatch.WoodDark);
            foreach (float lx in new[] { -0.55f, 0.55f })
            {
                b.BoxMinMax(new Vector3(x + lx - 0.04f, y, z - 0.7f), new Vector3(x + lx + 0.04f, y + 0.75f, z - 0.3f), TownSwatch.WoodDark);
            }

            b.Log(new Vector3(x - 0.3f, y + 0.9f, z - 0.52f), new Vector3(x - 0.3f, y + 0.92f, z - 0.48f), 0.13f, TownSwatch.Copper, 8);
            b.Log(new Vector3(x - 0.1f, y + 0.88f, z - 0.42f), new Vector3(x - 0.1f, y + 0.9f, z - 0.38f), 0.08f, TownSwatch.Bronze, 6);
            b.Box(new Vector3(x + 0.35f, y + 0.92f, z - 0.5f), new Vector3(0.18f, 0.18f, 0.14f), TownSwatch.Iron, 0f);
            b.Log(new Vector3(x + 0.95f, y + 0.6f, z - 0.55f), new Vector3(x + 0.95f, y + 0.6f, z - 0.35f), 0.28f, TownSwatch.Copper, 8, TownSwatch.Black);
            b.Box(new Vector3(x + 0.95f, y + 0.6f, z - 0.56f), new Vector3(0.4f, 0.06f, 0.02f), TownSwatch.Brick, 30f);
            b.Box(new Vector3(x + 0.95f, y + 0.25f, z - 0.45f), new Vector3(0.1f, 0.5f, 0.1f), TownSwatch.Iron, 0f);
            b.Box(new Vector3(x - 1.0f, y + 0.18f, z - 0.3f), new Vector3(0.5f, 0.36f, 0.4f), TownSwatch.WoodWeathered, -12f, TownSwatch.Iron);
        }
    }
}
