using System.Collections.Generic;
using UnityEngine;

namespace Rapadura.Gameplay.Housing.Visuals
{
    /// <summary>The sockets of the house hull (Housing.md §3.2): each level picks one module per socket.</summary>
    public enum HouseHullSocket
    {
        Foundation,
        Floor,
        Wall,
        Window,
        Door,
        Roof,
        Porch,
        Chimney,
        Extras,
        /// <summary>The casarão's second wing (ala íntima, N5 only).</summary>
        Wing,
    }

    /// <summary>The colours and switches of one level's kit (§3.2–§3.3).</summary>
    public sealed class HouseHullKit
    {
        public int Level;
        public float PorchDepth;
        public float Overhang;
        public float RidgeHeight;
        public float WallThickness;
        /// <summary>Plank walls (N1–N2) instead of solid ones.</summary>
        public bool BoardWalls;
        public HouseSwatch WallMain;
        public HouseSwatch WallAlt;
        public HouseSwatch Batten;
        /// <summary>The 60 cm "barrado" at the bottom of solid walls.</summary>
        public HouseSwatch Band;
        public float BandHeight;
        public HouseSwatch Plate;
        public HouseSwatch Frame;
        public HouseSwatch Shutter;
        public HouseSwatch Door;
        public HouseSwatch Roof;
        public HouseSwatch RoofUnder;
        public HouseSwatch Ridge;
        public HouseSwatch Pillar;
        public HouseSwatch Deck;
        public HouseSwatch DeckAlt;
        public HouseSwatch Foundation;
        public bool Glass;
        public bool IronBars;
        public bool Quoins;
        public bool HipRoof;
        public bool TileRows;
        public int Chimneys;
    }

    public static partial class HouseHullBuilder
    {
        public const float WingWidth = 5f;
        public const float WingDepth = 7f;

        /// <summary>Module ids per socket and level (N1…N5), as in the §3.2 table; "-" = nothing on that socket.</summary>
        private static readonly Dictionary<HouseHullSocket, string[]> Modules = new Dictionary<HouseHullSocket, string[]>
        {
            { HouseHullSocket.Foundation, new[] { "estacas_tora", "estacas_tablado", "baldrame_pedra", "alicerce_tijolo_degraus", "porao_pedra_escadaria" } },
            { HouseHullSocket.Floor, new[] { "tabua_crua", "tabua_corrida", "chao_batido_tijoleira", "ceramica_vermelha", "ladrilho_e_tabua" } },
            { HouseHullSocket.Wall, new[] { "tabua_mata_junta", "tabua_pintada", "taipa_rebocada", "tijolo_caiado_barrado", "alvenaria_cunhais" } },
            { HouseHullSocket.Window, new[] { "janela_tramela", "janela_duas_folhas", "janela_caixilho", "janela_vidro_veneziana", "janela_bandeira_grade" } },
            { HouseHullSocket.Door, new[] { "porta_tabua", "porta_batente", "porta_almofadada", "porta_almofadada_pintada", "porta_dupla_leque" } },
            { HouseHullSocket.Roof, new[] { "zinco_buriti", "tabua_cumeeira", "telha_capa_canal", "telha_beiral", "quatro_aguas_lambrequim" } },
            { HouseHullSocket.Porch, new[] { "alpendre_tora", "pilares_tora_ripa", "pilares_torneados", "pilares_arcos", "varanda_em_l" } },
            { HouseHullSocket.Chimney, new[] { "-", "-", "chamine_barro", "chamine_tijolo", "chamine_dupla" } },
            { HouseHullSocket.Extras, new[] { "varal_toco", "banco_rede_varanda", "calha_cisterna", "caixa_dagua_muro", "catavento_sino_pergolado" } },
            { HouseHullSocket.Wing, new[] { "-", "-", "-", "-", "ala_intima" } },
        };

        /// <summary>The module a level uses on a socket ("-" when the socket is empty at that level).</summary>
        public static string ModuleId(HouseHullSocket socket, int level)
        {
            string[] row = Modules[socket];
            return row[Mathf.Clamp(level, HouseLevels.MinLevel, HouseLevels.MaxLevel) - 1];
        }

        /// <summary>Porch depth in metres (N1 alpendre 1.5 m; N2+ varanda 2–2.5 m); the yard starts past it.</summary>
        public static float PorchDepthOf(HouseLevelSpec spec) => spec == null || spec.Level <= 1 ? PorchDepth : KitOf(spec.Level).PorchDepth;

        /// <summary>The kit of a level (N1 values describe the original builder).</summary>
        public static HouseHullKit KitOf(int level)
        {
            switch (Mathf.Clamp(level, HouseLevels.MinLevel, HouseLevels.MaxLevel))
            {
                case 1:
                    return new HouseHullKit
                    {
                        Level = 1, PorchDepth = PorchDepth, Overhang = 0.35f, RidgeHeight = RidgeHeight, WallThickness = WallThickness, BoardWalls = true,
                        WallMain = HouseSwatch.WoodRaw, WallAlt = HouseSwatch.FloorBoard, Batten = HouseSwatch.WoodJoint, Plate = HouseSwatch.WoodDark,
                        Frame = HouseSwatch.WoodDark, Shutter = HouseSwatch.WoodRaw, Door = HouseSwatch.WoodRaw, Roof = HouseSwatch.Zinc,
                        RoofUnder = HouseSwatch.WoodJoint, Ridge = HouseSwatch.ZincDark, Pillar = HouseSwatch.WoodDark, Deck = HouseSwatch.WoodRaw,
                        DeckAlt = HouseSwatch.FloorBoard, Foundation = HouseSwatch.WoodDark,
                    };
                case 2:
                    // Madeira com varanda: tábua pintada branco-gelo, esquadrias verde-bandeira, telhado de tábua com cumeeira.
                    return new HouseHullKit
                    {
                        Level = 2, PorchDepth = 2f, Overhang = 0.4f, RidgeHeight = 1.3f, WallThickness = WallThickness, BoardWalls = true,
                        WallMain = HouseSwatch.Cream, WallAlt = HouseSwatch.White, Batten = HouseSwatch.PaintTeal, Plate = HouseSwatch.WoodDark,
                        Frame = HouseSwatch.ClothGreen, Shutter = HouseSwatch.ClothGreen, Door = HouseSwatch.ClothGreen, Roof = HouseSwatch.WoodRaw,
                        RoofUnder = HouseSwatch.WoodJoint, Ridge = HouseSwatch.WoodDark, Pillar = HouseSwatch.WoodDark, Deck = HouseSwatch.FloorBoard,
                        DeckAlt = HouseSwatch.WoodRaw, Foundation = HouseSwatch.WoodDark,
                    };
                case 3:
                    // Taipa e telha: taipa ocre rebocada, barrado anil, madeira escura, telha colonial capa-canal, chaminé de barro.
                    return new HouseHullKit
                    {
                        Level = 3, PorchDepth = 2f, Overhang = 0.45f, RidgeHeight = 1.5f, WallThickness = 0.14f, BandHeight = 0.6f,
                        WallMain = HouseSwatch.Taipa, WallAlt = HouseSwatch.Plaster, Band = HouseSwatch.ClothBlue, Plate = HouseSwatch.WoodDark,
                        Frame = HouseSwatch.WoodDark, Shutter = HouseSwatch.Varnish, Door = HouseSwatch.Varnish, Roof = HouseSwatch.Clay,
                        RoofUnder = HouseSwatch.WoodDark, Ridge = HouseSwatch.Clay, Pillar = HouseSwatch.Varnish, Deck = HouseSwatch.Clay,
                        DeckAlt = HouseSwatch.Dirt, Foundation = HouseSwatch.Stone, TileRows = true, Chimneys = 1,
                    };
                case 4:
                    // Alvenaria: cal branca, barrado amarelo-ouro, esquadrias azuis com vidro e veneziana, telha com beiral.
                    return new HouseHullKit
                    {
                        Level = 4, PorchDepth = 2f, Overhang = 0.65f, RidgeHeight = 1.7f, WallThickness = 0.16f, BandHeight = 0.6f,
                        WallMain = HouseSwatch.White, WallAlt = HouseSwatch.Cream, Band = HouseSwatch.ClothYellow, Plate = HouseSwatch.White,
                        Frame = HouseSwatch.ClothBlue, Shutter = HouseSwatch.ClothBlue, Door = HouseSwatch.ClothBlue, Roof = HouseSwatch.Clay,
                        RoofUnder = HouseSwatch.WoodDark, Ridge = HouseSwatch.RoofTileDark, Pillar = HouseSwatch.White, Deck = HouseSwatch.Clay,
                        DeckAlt = HouseSwatch.RoofTileDark, Foundation = HouseSwatch.Clay, Glass = true, TileRows = true, Chimneys = 1,
                    };
                default:
                    // Casarão: cal, cunhais de pedra, janelas altas com bandeira e grade, 4 águas com lambrequim, varanda em L.
                    return new HouseHullKit
                    {
                        Level = 5, PorchDepth = 2.5f, Overhang = 0.7f, RidgeHeight = 2.2f, WallThickness = 0.2f, BandHeight = 0.5f,
                        WallMain = HouseSwatch.White, WallAlt = HouseSwatch.Cream, Band = HouseSwatch.Quoin, Plate = HouseSwatch.Quoin,
                        Frame = HouseSwatch.Varnish, Shutter = HouseSwatch.Varnish, Door = HouseSwatch.Varnish, Roof = HouseSwatch.RoofTileDark,
                        RoofUnder = HouseSwatch.WoodDark, Ridge = HouseSwatch.RoofTileDark, Pillar = HouseSwatch.White, Deck = HouseSwatch.Ladrilho,
                        DeckAlt = HouseSwatch.White, Foundation = HouseSwatch.Quoin, Glass = true, IronBars = true, Quoins = true, HipRoof = true,
                        Chimneys = 2,
                    };
            }
        }

        // ------------------------------------------------------------------
        // Build N2–N5: one module per socket
        // ------------------------------------------------------------------

        private static HouseHullMeshes BuildKit(HouseLevelSpec spec)
        {
            HouseHullKit kit = KitOf(spec.Level);
            string prefix = "HouseN" + spec.Level + "_";
            var meshes = new HouseHullMeshes();
            var b = new HouseMeshBuilder();

            BuildGround(b, spec, kit);
            BuildFoundation(b, spec, kit);
            BuildFloor(b, spec, kit);
            BuildPorchDeck(b, spec, kit);
            BuildCaja(b, spec, YardOrigin(spec), spec.Level);
            BuildExtras(b, spec, kit);
            if (spec.Level >= 5) BuildWingBase(b, spec, kit);
            meshes.Base = b.ToMesh(prefix + "Base");

            foreach (HouseWallSide side in new[] { HouseWallSide.South, HouseWallSide.East, HouseWallSide.North, HouseWallSide.West })
            {
                b.Clear();
                if (kit.BoardWalls) BuildBoardWall(b, spec, side, kit);
                else BuildSolidWall(b, spec, side, kit);
                BuildOpenings(b, spec, side, kit);
                // The second wing hangs off the east side, so it hides with the east wall in the cutaway.
                if (side == HouseWallSide.East && spec.Level >= 5) BuildWingWalls(b, spec, kit);
                meshes.Walls[(int)side] = b.ToMesh(prefix + "Wall_" + side);
            }

            b.Clear();
            BuildKitRoof(b, spec, kit);
            BuildPorchRoof(b, spec, kit);
            BuildChimneys(b, spec, kit);
            if (spec.Level >= 5) BuildWingRoof(b, spec, kit);
            meshes.Roof = b.ToMesh(prefix + "Roof");
            return meshes;
        }

        private static void BuildGround(HouseMeshBuilder b, HouseLevelSpec spec, HouseHullKit kit)
        {
            float w = InteriorWidth(spec);
            float d = InteriorDepth(spec);
            Vector2 yard = YardOrigin(spec);
            float yardW = spec.YardWidth * HouseLevelSpec.YardCellSize;
            float yardD = spec.YardDepth * HouseLevelSpec.YardCellSize;
            float east = Mathf.Max(yard.x + yardW, w + (spec.Level >= 5 ? WingWidth : 0f)) + 3f;
            b.BoxMinMax(new Vector3(Mathf.Min(yard.x, 0f) - 3f, -0.2f, yard.y - 3f), new Vector3(east, -0.02f, d + 3f), HouseSwatch.GrassDark, HouseSwatch.GrassDark);
            b.BoxMinMax(new Vector3(yard.x, -0.05f, yard.y), new Vector3(yard.x + yardW, 0f, yard.y + yardD), HouseSwatch.Grass);

            // Steps up to the porch in front of the door, then the path to the yard entrance.
            HouseOpening door = spec.EntranceDoor;
            float doorU0 = door != null ? door.Start * HouseLevelSpec.InteriorCellSize : 1f;
            float doorU1 = door != null ? (door.Start + door.Width) * HouseLevelSpec.InteriorCellSize : 2f;
            float wide = spec.Level >= 5 ? 0.8f : 0.1f;
            int steps = spec.Level >= 4 ? 3 : 2;
            for (int i = 0; i < steps; i++)
            {
                float stepTop = FloorY - 0.04f - (i + 1) * (FloorY / (steps + 1));
                b.BoxMinMax(new Vector3(doorU0 - wide - i * 0.1f, -0.02f, -kit.PorchDepth - 0.3f * (i + 1)), new Vector3(doorU1 + wide + i * 0.1f, stepTop, -kit.PorchDepth - 0.3f * i),
                    kit.Level >= 3 ? kit.Foundation : HouseSwatch.WoodJoint, kit.Level >= 4 ? HouseSwatch.Concrete : kit.Deck);
            }

            float pathX = yard.x + (spec.YardEntranceX + 0.5f) * HouseLevelSpec.YardCellSize;
            float doorX = (doorU0 + doorU1) * 0.5f;
            b.BoxMinMax(new Vector3(Mathf.Min(pathX, doorX) - 0.45f, -0.01f, yard.y + yardD - 1f), new Vector3(Mathf.Max(pathX, doorX) + 0.45f, 0.005f, -kit.PorchDepth - 0.3f * steps),
                kit.Level >= 4 ? HouseSwatch.Concrete : HouseSwatch.Dirt);
        }

        private static void BuildFoundation(HouseMeshBuilder b, HouseLevelSpec spec, HouseHullKit kit)
        {
            float w = InteriorWidth(spec);
            float d = InteriorDepth(spec);
            float t = kit.WallThickness;
            switch (kit.Level)
            {
                case 2:
                    // Estacas + tablado: log stilts with a board skirt.
                    for (int ix = 0; ix <= 3; ix++)
                    {
                        for (int iz = 0; iz <= 1; iz++)
                        {
                            var foot = new Vector3(0.1f + ix * (w - 0.2f) / 3f, -0.02f, 0.1f + iz * (d - 0.2f));
                            b.Cylinder(foot, 0.11f, 0.1f, FloorY - 0.06f, 6, HouseSwatch.WoodDark, HouseSwatch.WoodJoint);
                        }
                    }

                    b.BoxMinMax(new Vector3(-t, 0.05f, -t - 0.01f), new Vector3(w + t, FloorY - 0.08f, -t), HouseSwatch.WoodJoint);
                    b.BoxMinMax(new Vector3(-t, 0.05f, d + t), new Vector3(w + t, FloorY - 0.08f, d + t + 0.01f), HouseSwatch.WoodJoint);
                    return;
                default:
                    // Baldrame de pedra (N3), alicerce de tijolo (N4), porão de pedra (N5, a little taller and wider).
                    float grow = kit.Level >= 5 ? 0.12f : 0.04f;
                    b.BoxMinMax(new Vector3(-t - grow, -0.02f, -t - grow), new Vector3(w + t + grow, FloorY - 0.06f, d + t + grow), kit.Foundation);
                    if (kit.Level >= 4)
                    {
                        // Calçada de cimento around the back and the sides.
                        b.BoxMinMax(new Vector3(-t - 0.7f, -0.02f, -0.05f), new Vector3(-t - grow, 0.04f, d + t + 0.7f), HouseSwatch.Concrete);
                        b.BoxMinMax(new Vector3(-t - 0.7f, -0.02f, d + t + grow), new Vector3(w + t + 0.7f, 0.04f, d + t + 0.7f), HouseSwatch.Concrete);
                        if (kit.Level < 5) b.BoxMinMax(new Vector3(w + t + grow, -0.02f, -0.05f), new Vector3(w + t + 0.7f, 0.04f, d + t + 0.7f), HouseSwatch.Concrete);
                    }

                    if (kit.Level >= 5)
                    {
                        // Basement vents of the porão.
                        for (int i = 1; i < 6; i++)
                        {
                            float x = w * i / 6f;
                            b.BoxMinMax(new Vector3(x - 0.2f, 0.08f, -t - grow - 0.01f), new Vector3(x + 0.2f, 0.2f, -t - grow), HouseSwatch.Black);
                        }
                    }

                    return;
            }
        }

        private static void BuildFloor(HouseMeshBuilder b, HouseLevelSpec spec, HouseHullKit kit)
        {
            float w = InteriorWidth(spec);
            float d = InteriorDepth(spec);
            float t = kit.WallThickness;
            switch (kit.Level)
            {
                case 2:
                {
                    // Tábua corrida: narrower even boards.
                    const float board = 0.2f;
                    int planks = Mathf.RoundToInt(d / board);
                    for (int i = 0; i < planks; i++)
                    {
                        float z0 = i * board;
                        b.BoxMinMax(new Vector3(-t, FloorY - 0.08f, z0), new Vector3(w + t, FloorY, z0 + board - 0.008f), i % 4 == 3 ? HouseSwatch.WoodRaw : HouseSwatch.FloorBoard);
                    }

                    return;
                }
                case 5:
                {
                    // Ladrilho hidráulico in the front half (the social side), boards in the back (bedrooms).
                    float half = Mathf.Floor(d * 0.5f);
                    b.BoxMinMax(new Vector3(-t, FloorY - 0.1f, 0f), new Vector3(w + t, FloorY, half), HouseSwatch.White);
                    Checker(b, 0f, w, 0f, half, 1f, HouseSwatch.Ladrilho);
                    int planks = Mathf.RoundToInt((d - half) / 0.25f);
                    for (int i = 0; i < planks; i++)
                    {
                        float z0 = half + i * 0.25f;
                        b.BoxMinMax(new Vector3(-t, FloorY - 0.1f, z0), new Vector3(w + t, FloorY, Mathf.Min(d, z0 + 0.24f)), i % 2 == 0 ? HouseSwatch.FloorBoard : HouseSwatch.WoodRaw);
                    }

                    return;
                }
                default:
                {
                    // N3 chão batido + tijoleira; N4 cerâmica vermelha.
                    HouseSwatch slab = kit.Level == 3 ? HouseSwatch.Dirt : HouseSwatch.Clay;
                    HouseSwatch tile = kit.Level == 3 ? HouseSwatch.Clay : HouseSwatch.RoofTileDark;
                    b.BoxMinMax(new Vector3(-t, FloorY - 0.1f, -t), new Vector3(w + t, FloorY, d + t), slab);
                    Checker(b, 0f, w, 0f, d, 1f, tile);
                    return;
                }
            }
        }

        /// <summary>Every other square of a 1 m checker, a hair above the floor (furniture still stands at <see cref="FloorY"/>).</summary>
        private static void Checker(HouseMeshBuilder b, float x0, float x1, float z0, float z1, float size, HouseSwatch swatch)
        {
            int nx = Mathf.Max(1, Mathf.RoundToInt((x1 - x0) / size));
            int nz = Mathf.Max(1, Mathf.RoundToInt((z1 - z0) / size));
            for (int ix = 0; ix < nx; ix++)
            {
                for (int iz = 0; iz < nz; iz++)
                {
                    if ((ix + iz) % 2 != 0) continue;
                    float ax = x0 + ix * size, az = z0 + iz * size;
                    b.BoxMinMax(new Vector3(ax + 0.02f, FloorY - 0.02f, az + 0.02f), new Vector3(Mathf.Min(x1, ax + size) - 0.02f, FloorY + 0.004f, Mathf.Min(z1, az + size) - 0.02f), swatch);
                }
            }
        }

        private static void BuildPorchDeck(HouseMeshBuilder b, HouseLevelSpec spec, HouseHullKit kit)
        {
            float w = InteriorWidth(spec);
            float depth = kit.PorchDepth;
            if (kit.Level == 2)
            {
                int planks = Mathf.RoundToInt(depth / BoardWidth);
                for (int i = 0; i < planks; i++)
                {
                    float z1 = -i * BoardWidth;
                    b.BoxMinMax(new Vector3(-0.05f, FloorY - 0.1f, z1 - BoardWidth + 0.02f), new Vector3(w + 0.05f, FloorY - 0.03f, z1), i % 2 == 0 ? kit.Deck : kit.DeckAlt);
                }

                b.Cylinder(new Vector3(0.15f, -0.02f, -depth + 0.15f), 0.1f, 0.09f, FloorY - 0.08f, 6, HouseSwatch.WoodDark);
                b.Cylinder(new Vector3(w * 0.5f, -0.02f, -depth + 0.15f), 0.1f, 0.09f, FloorY - 0.08f, 6, HouseSwatch.WoodDark);
                b.Cylinder(new Vector3(w - 0.15f, -0.02f, -depth + 0.15f), 0.1f, 0.09f, FloorY - 0.08f, 6, HouseSwatch.WoodDark);
                return;
            }

            // Masonry porch: a solid base with a tiled top; the casarão's turns the west corner (varanda em L).
            float grow = kit.Level >= 5 ? 0.12f : 0.04f;
            b.BoxMinMax(new Vector3(-kit.WallThickness - grow, -0.02f, -depth), new Vector3(w + kit.WallThickness + grow, FloorY - 0.005f, -kit.WallThickness), kit.Foundation, kit.Deck);
            Checker(b, 0f, w, -depth + 0.1f, -kit.WallThickness, 0.5f, kit.DeckAlt);
            if (kit.Level >= 5)
            {
                float d = InteriorDepth(spec);
                b.BoxMinMax(new Vector3(-depth, -0.02f, -depth), new Vector3(-kit.WallThickness - grow, FloorY - 0.005f, d), kit.Foundation, kit.Deck);
                Checker(b, -depth + 0.1f, -kit.WallThickness - grow, -depth, d, 0.5f, kit.DeckAlt);
            }
        }

        /// <summary>The cousin's cajá grows with the house (§2.1): a seedling at N1, a fruiting tree at N5.</summary>
        private static void BuildCaja(HouseMeshBuilder b, HouseLevelSpec spec, Vector2 yard, int level)
        {
            if (level <= 1)
            {
                BuildCajaSeedling(b, spec, yard);
                return;
            }

            CellRect tree = spec.FixedTree;
            var center = new Vector3(yard.x + (tree.X + tree.Width * 0.5f) * HouseLevelSpec.YardCellSize, 0f,
                yard.y + (tree.Y + tree.Height * 0.5f) * HouseLevelSpec.YardCellSize);
            float grow = (level - 1) / 4f; // 0.25 … 1
            float trunk = Mathf.Lerp(0.07f, 0.2f, grow);
            float height = Mathf.Lerp(1.1f, 2.6f, grow);
            b.Cylinder(center, 0.6f, 0.55f, 0.04f, 8, HouseSwatch.Dirt);
            b.Cylinder(center, trunk, trunk * 0.7f, height, 6, HouseSwatch.WoodDark);
            float crown = Mathf.Lerp(0.7f, 2.2f, grow);
            b.Blob(center + Vector3.up * (height + crown * 0.2f), new Vector3(crown, crown * 0.75f, crown), HouseSwatch.Leaf);
            b.Blob(center + new Vector3(crown * 0.3f, height - crown * 0.05f, 0.1f), new Vector3(crown * 0.6f, crown * 0.45f, crown * 0.6f), HouseSwatch.GrassDark);
            b.Blob(center + new Vector3(-crown * 0.28f, height + crown * 0.05f, -crown * 0.15f), new Vector3(crown * 0.55f, crown * 0.42f, crown * 0.55f), HouseSwatch.Leaf);
            if (level >= 4)
            {
                // Yellow cajás once the tree is grown (N4 a few, N5 a full crop).
                int fruits = level >= 5 ? 9 : 4;
                for (int i = 0; i < fruits; i++)
                {
                    float a = i * 2.4f;
                    var p = center + new Vector3(Mathf.Cos(a) * crown * 0.42f, height + Mathf.Sin(i * 1.7f) * crown * 0.18f, Mathf.Sin(a) * crown * 0.42f);
                    b.Blob(p, new Vector3(0.12f, 0.14f, 0.12f), HouseSwatch.Fruit);
                }
            }
        }

        private static void BuildExtras(HouseMeshBuilder b, HouseLevelSpec spec, HouseHullKit kit)
        {
            float w = InteriorWidth(spec);
            float d = InteriorDepth(spec);
            switch (kit.Level)
            {
                case 2:
                {
                    // Banco de varanda against the front wall, right of the door.
                    float x0 = w * 0.62f;
                    b.BoxMinMax(new Vector3(x0, FloorY + 0.4f, -0.55f), new Vector3(x0 + 1.4f, FloorY + 0.46f, -0.15f), HouseSwatch.WoodRaw);
                    b.BoxMinMax(new Vector3(x0 + 0.05f, FloorY - 0.03f, -0.5f), new Vector3(x0 + 0.12f, FloorY + 0.4f, -0.2f), HouseSwatch.WoodDark);
                    b.BoxMinMax(new Vector3(x0 + 1.28f, FloorY - 0.03f, -0.5f), new Vector3(x0 + 1.35f, FloorY + 0.4f, -0.2f), HouseSwatch.WoodDark);
                    return;
                }
                case 3:
                {
                    // Cisterna on the west side.
                    var c = new Vector3(-1.4f, -0.02f, d * 0.35f);
                    b.Cylinder(c, 0.7f, 0.7f, 1.1f, 10, HouseSwatch.Stone, HouseSwatch.Concrete);
                    b.Cylinder(c + Vector3.up * 1.1f, 0.74f, 0.2f, 0.25f, 10, HouseSwatch.Clay);
                    return;
                }
                case 4:
                {
                    // Caixa d'água on four posts (west) and a low wall behind the house.
                    var c = new Vector3(-1.6f, 0f, d * 0.7f);
                    for (int i = 0; i < 4; i++)
                    {
                        var leg = c + new Vector3(i % 2 == 0 ? -0.45f : 0.45f, -0.02f, i < 2 ? -0.45f : 0.45f);
                        b.BoxMinMax(leg + new Vector3(-0.05f, 0f, -0.05f), leg + new Vector3(0.05f, 2.2f, 0.05f), HouseSwatch.Concrete);
                    }

                    b.BoxMinMax(c + new Vector3(-0.6f, 2.2f, -0.6f), c + new Vector3(0.6f, 3.1f, 0.6f), HouseSwatch.Concrete, HouseSwatch.ClothBlue);
                    b.BoxMinMax(new Vector3(-2.5f, -0.02f, d + 1.4f), new Vector3(w + 2.5f, 0.75f, d + 1.6f), HouseSwatch.White, HouseSwatch.Concrete);
                    b.BoxMinMax(new Vector3(-2.5f, -0.02f, d + 1.38f), new Vector3(w + 2.5f, 0.3f, d + 1.62f), HouseSwatch.ClothYellow);
                    return;
                }
                default:
                {
                    // Cata-vento behind the house (west), a pergolado at the back and the gate bell (sino da porteira).
                    var mast = new Vector3(-2.2f, -0.02f, d + 0.8f);
                    b.Cylinder(mast, 0.08f, 0.05f, 5f, 6, HouseSwatch.Iron);
                    for (int i = 0; i < 4; i++)
                    {
                        Matrix4x4 saved = b.Matrix;
                        b.Matrix = saved * Matrix4x4.TRS(mast + Vector3.up * 5f + Vector3.back * 0.12f, Quaternion.Euler(0f, 0f, i * 90f + 20f), Vector3.one);
                        b.Box(new Vector3(0f, 0.55f, 0f), new Vector3(0.16f, 1f, 0.02f), HouseSwatch.White);
                        b.Matrix = saved;
                    }

                    for (int i = 0; i < 4; i++)
                    {
                        float x = w * (0.2f + i * 0.2f);
                        b.BoxMinMax(new Vector3(x - 0.07f, -0.02f, d + 2.4f), new Vector3(x + 0.07f, 2.3f, d + 2.54f), HouseSwatch.Varnish);
                    }

                    b.BoxMinMax(new Vector3(w * 0.15f, 2.3f, d + 2.38f), new Vector3(w * 0.85f, 2.4f, d + 2.56f), HouseSwatch.Varnish);
                    for (int i = 0; i < 7; i++)
                    {
                        float x = w * (0.2f + i * 0.1f);
                        b.BoxMinMax(new Vector3(x - 0.04f, 2.4f, d + 0.4f), new Vector3(x + 0.04f, 2.48f, d + 2.7f), HouseSwatch.WoodDark);
                    }

                    b.Blob(new Vector3(w * 0.3f, 2.5f, d + 1.5f), new Vector3(1.4f, 0.4f, 1.6f), HouseSwatch.Leaf);
                    b.Blob(new Vector3(w * 0.6f, 2.5f, d + 1.2f), new Vector3(1.2f, 0.35f, 1.4f), HouseSwatch.Flower);

                    Vector2 yard = YardOrigin(spec);
                    var post = new Vector3(yard.x - 0.6f, -0.02f, yard.y + 0.6f);
                    b.BoxMinMax(post + new Vector3(-0.07f, 0f, -0.07f), post + new Vector3(0.07f, 2f, 0.07f), HouseSwatch.Varnish);
                    b.BoxMinMax(post + new Vector3(-0.07f, 1.95f, -0.07f), post + new Vector3(0.45f, 2.03f, 0.07f), HouseSwatch.Varnish);
                    b.Cylinder(post + new Vector3(0.35f, 1.62f, 0f), 0.13f, 0.06f, 0.3f, 7, HouseSwatch.Gold);
                    return;
                }
            }
        }

        // ------------------------------------------------------------------
        // Walls
        // ------------------------------------------------------------------

        /// <summary>Plank walls (N2: tábua pintada), same construction as N1 with the kit's paint.</summary>
        private static void BuildBoardWall(HouseMeshBuilder b, HouseLevelSpec spec, HouseWallSide side, HouseHullKit kit)
        {
            float length = spec.WallLength(side) * HouseLevelSpec.InteriorCellSize;
            int boards = Mathf.RoundToInt(length / BoardWidth);
            for (int i = 0; i < boards; i++)
            {
                float u0 = i * BoardWidth;
                float u1 = u0 + BoardWidth;
                HouseSwatch paint = i % 6 == 4 ? kit.WallAlt : kit.WallMain;
                foreach ((float v0, float v1) in OpenRuns(spec, side, u0 + 1e-3f, u1 - 1e-3f))
                {
                    WallBox(b, spec, side, u0, u1, v0, v1, 0f, kit.WallThickness, paint, caps: false);
                }

                if (i % 2 == 1 && i < boards - 1)
                {
                    foreach ((float v0, float v1) in OpenRuns(spec, side, u1 - 0.03f, u1 + 0.03f))
                    {
                        WallBox(b, spec, side, u1 - 0.03f, u1 + 0.03f, v0, v1, kit.WallThickness, kit.WallThickness + 0.025f, kit.Batten, caps: false);
                    }
                }
            }

            WallBox(b, spec, side, -kit.WallThickness, length + kit.WallThickness, -0.08f, 0.04f, 0f, kit.WallThickness + 0.04f, kit.Plate);
            WallBox(b, spec, side, -kit.WallThickness, length + kit.WallThickness, WallHeight - 0.1f, WallHeight, 0f, kit.WallThickness + 0.04f, kit.Plate);
            if (side == HouseWallSide.East || side == HouseWallSide.West)
            {
                WallBox(b, spec, side, -0.1f, 0.02f, -0.08f, WallHeight, -0.02f, kit.WallThickness + 0.06f, kit.Plate);
                WallBox(b, spec, side, length - 0.02f, length + 0.1f, -0.08f, WallHeight, -0.02f, kit.WallThickness + 0.06f, kit.Plate);
            }
        }

        /// <summary>
        /// Solid walls (taipa, alvenaria): one box per run between the openings, the "barrado" band at the bottom, the
        /// kit's corner stones (cunhais) on the casarão.
        /// </summary>
        private static void BuildSolidWall(HouseMeshBuilder b, HouseLevelSpec spec, HouseWallSide side, HouseHullKit kit)
        {
            float length = spec.WallLength(side) * HouseLevelSpec.InteriorCellSize;
            float t = kit.WallThickness;
            // East/west walls run between the south and north walls; south/north close the corners.
            float start = side == HouseWallSide.South || side == HouseWallSide.North ? -t : 0f;
            float end = side == HouseWallSide.South || side == HouseWallSide.North ? length + t : length;
            List<float> cuts = ColumnCuts(spec, side, start, end);
            for (int i = 0; i + 1 < cuts.Count; i++)
            {
                float u0 = cuts[i], u1 = cuts[i + 1];
                if (u1 - u0 < 1e-3f) continue;
                foreach ((float v0, float v1) in OpenRuns(spec, side, (u0 + u1) * 0.5f, (u0 + u1) * 0.5f))
                {
                    if (kit.BandHeight > 0f && v0 < kit.BandHeight)
                    {
                        WallBox(b, spec, side, u0, u1, v0, Mathf.Min(v1, kit.BandHeight), 0f, t, kit.Band);
                        if (v1 > kit.BandHeight) WallBox(b, spec, side, u0, u1, kit.BandHeight, v1, 0f, t, kit.WallMain);
                    }
                    else
                    {
                        WallBox(b, spec, side, u0, u1, v0, v1, 0f, t, kit.WallMain);
                    }
                }
            }

            // Plaster patch in the middle of the taipa (N3: "rebocada no meio").
            if (kit.Level == 3)
            {
                WallBox(b, spec, side, length * 0.3f, length * 0.42f, 0.9f, 1.6f, t, t + 0.01f, kit.WallAlt);
            }

            // Eave band.
            WallBox(b, spec, side, start, end, WallHeight - 0.14f, WallHeight, t, t + 0.03f, kit.Level == 3 ? HouseSwatch.WoodDark : kit.WallAlt);

            if (kit.Quoins && (side == HouseWallSide.South || side == HouseWallSide.North))
            {
                foreach (float corner in new[] { start, end - 0.35f })
                {
                    for (int k = 0; k < 5; k++)
                    {
                        float v0 = 0.05f + k * 0.5f;
                        float wide = k % 2 == 0 ? 0.35f : 0.25f;
                        float u0 = corner < 0.01f ? corner : corner + (0.35f - wide);
                        WallBox(b, spec, side, u0, u0 + wide, v0, v0 + 0.42f, t, t + 0.025f, HouseSwatch.Quoin);
                    }
                }
            }
        }

        /// <summary>Wall columns where a solid wall must be split: its ends and every opening edge.</summary>
        private static List<float> ColumnCuts(HouseLevelSpec spec, HouseWallSide side, float start, float end)
        {
            float cell = HouseLevelSpec.InteriorCellSize;
            var cuts = new List<float> { start, end };
            foreach (HouseOpening opening in spec.Openings)
            {
                if (opening.Side != side) continue;
                cuts.Add(opening.Start * cell);
                cuts.Add((opening.Start + opening.Width) * cell);
            }

            cuts.Sort();
            return cuts;
        }

        /// <summary>The vertical runs (floor to wall top) of a wall column u0..u1 that are not cut by an opening.</summary>
        private static IEnumerable<(float, float)> OpenRuns(HouseLevelSpec spec, HouseWallSide side, float u0, float u1)
        {
            float cell = HouseLevelSpec.InteriorCellSize;
            var holes = new List<(float, float)>();
            foreach (HouseOpening opening in spec.Openings)
            {
                if (opening.Side != side) continue;
                float ou0 = opening.Start * cell, ou1 = (opening.Start + opening.Width) * cell;
                if (u1 < ou0 || u0 > ou1) continue;
                holes.Add((opening.RowStart * cell, (opening.RowStart + opening.Rows) * cell));
            }

            holes.Sort((a, c) => a.Item1.CompareTo(c.Item1));
            float v = 0f;
            foreach ((float h0, float h1) in holes)
            {
                if (h0 > v) yield return (v, h0);
                v = Mathf.Max(v, h1);
            }

            if (v < WallHeight) yield return (v, WallHeight);
        }

        /// <summary>Doors and windows of the kit (§3.2 rows "Parede c/ janela" / "Parede c/ porta").</summary>
        private static void BuildOpenings(HouseMeshBuilder b, HouseLevelSpec spec, HouseWallSide side, HouseHullKit kit)
        {
            float cell = HouseLevelSpec.InteriorCellSize;
            float t = kit.WallThickness;
            foreach (HouseOpening opening in spec.Openings)
            {
                if (opening.Side != side) continue;
                float u0 = opening.Start * cell, u1 = (opening.Start + opening.Width) * cell;
                float v0 = opening.RowStart * cell, v1 = (opening.RowStart + opening.Rows) * cell;
                float width = u1 - u0;

                // Frame (batente / caixilho).
                WallBox(b, spec, side, u0 - 0.07f, u0, v0, v1 + 0.07f, -0.01f, t + 0.04f, kit.Frame);
                WallBox(b, spec, side, u1, u1 + 0.07f, v0, v1 + 0.07f, -0.01f, t + 0.04f, kit.Frame);
                WallBox(b, spec, side, u0 - 0.07f, u1 + 0.07f, v1, v1 + 0.09f, -0.01f, t + 0.04f, kit.Frame);

                if (opening.IsDoor)
                {
                    if (kit.Level >= 5)
                    {
                        // Porta dupla, both leaves open inwards, with a fan light (bandeira em leque) above.
                        WallBox(b, spec, side, u0 - 0.06f, u0 - 0.01f, 0.02f, v1 - 0.04f, -width * 0.5f - 0.02f, -0.02f, kit.Door);
                        WallBox(b, spec, side, u1 + 0.01f, u1 + 0.06f, 0.02f, v1 - 0.04f, -width * 0.5f - 0.02f, -0.02f, kit.Door);
                        WallBox(b, spec, side, u0, u1, v1 + 0.09f, v1 + 0.45f, t * 0.4f, t * 0.6f, HouseSwatch.Glass);
                        for (int r = 0; r < 3; r++)
                        {
                            float u = u0 + width * (0.25f + r * 0.25f);
                            WallBox(b, spec, side, u - 0.02f, u + 0.02f, v1 + 0.09f, v1 + 0.45f, t * 0.35f, t * 0.65f, kit.Frame);
                        }

                        WallBox(b, spec, side, u0 - 0.07f, u1 + 0.07f, v1 + 0.45f, v1 + 0.52f, -0.01f, t + 0.04f, kit.Frame);
                    }
                    else
                    {
                        // One leaf open inwards; from N3 it has raised panels (almofadas).
                        WallBox(b, spec, side, u1 + 0.02f, u1 + 0.07f, 0.02f, v1 - 0.04f, -width - 0.02f, -0.02f, kit.Door);
                        WallBox(b, spec, side, u1 + 0.07f, u1 + 0.1f, v1 * 0.45f, v1 * 0.5f, -width * 0.8f, -width * 0.6f, HouseSwatch.Iron);
                        if (kit.Level >= 3)
                        {
                            WallBox(b, spec, side, u1 + 0.07f, u1 + 0.09f, 0.3f, v1 * 0.42f, -width * 0.85f, -width * 0.15f, kit.Frame);
                            WallBox(b, spec, side, u1 + 0.07f, u1 + 0.09f, v1 * 0.55f, v1 - 0.25f, -width * 0.85f, -width * 0.15f, kit.Frame);
                        }
                    }

                    // Soleira.
                    WallBox(b, spec, side, u0 - 0.1f, u1 + 0.1f, -0.04f, 0.01f, -0.05f, t + 0.08f, kit.Level >= 3 ? HouseSwatch.Quoin : kit.Plate);
                    continue;
                }

                // Sill.
                WallBox(b, spec, side, u0 - 0.08f, u1 + 0.08f, v0 - 0.07f, v0, -0.04f, t + 0.1f, kit.Level >= 5 ? HouseSwatch.Quoin : kit.Frame);
                if (kit.Glass)
                {
                    // Glass panes with a cross bar, and the veneziana shutters folded open outside.
                    WallBox(b, spec, side, u0, u1, v0, v1, t * 0.45f, t * 0.55f, HouseSwatch.Glass);
                    WallBox(b, spec, side, u0, u1, (v0 + v1) * 0.5f - 0.02f, (v0 + v1) * 0.5f + 0.02f, t * 0.4f, t * 0.6f, kit.Frame);
                    WallBox(b, spec, side, (u0 + u1) * 0.5f - 0.02f, (u0 + u1) * 0.5f + 0.02f, v0, v1, t * 0.4f, t * 0.6f, kit.Frame);
                }

                if (kit.IronBars)
                {
                    for (int r = 1; r < 5; r++)
                    {
                        float u = u0 + width * r / 5f;
                        WallBox(b, spec, side, u - 0.012f, u + 0.012f, v0, v1, t + 0.02f, t + 0.045f, HouseSwatch.Iron);
                    }

                    // Bandeira above the tall window.
                    WallBox(b, spec, side, u0, u1, v1 + 0.09f, v1 + 0.35f, t * 0.45f, t * 0.55f, HouseSwatch.Glass);
                    WallBox(b, spec, side, u0 - 0.07f, u1 + 0.07f, v1 + 0.35f, v1 + 0.42f, -0.01f, t + 0.04f, kit.Frame);
                }
                else
                {
                    WallBox(b, spec, side, u0 - 0.07f - width * 0.5f, u0 - 0.07f, v0, v1, t + 0.02f, t + 0.05f, kit.Shutter);
                    WallBox(b, spec, side, u1 + 0.07f, u1 + 0.07f + width * 0.5f, v0, v1, t + 0.02f, t + 0.05f, kit.Shutter);
                    if (kit.Glass)
                    {
                        // Veneziana slats.
                        for (int s = 1; s < 5; s++)
                        {
                            float v = v0 + (v1 - v0) * s / 5f;
                            WallBox(b, spec, side, u0 - 0.07f - width * 0.5f, u0 - 0.07f, v - 0.01f, v + 0.01f, t + 0.05f, t + 0.06f, kit.WallAlt);
                            WallBox(b, spec, side, u1 + 0.07f, u1 + 0.07f + width * 0.5f, v - 0.01f, v + 0.01f, t + 0.05f, t + 0.06f, kit.WallAlt);
                        }
                    }
                }

                // Flower box under every window (§3.3: there is always a flower at the window).
                WallBox(b, spec, side, u0, u1, v0 - 0.24f, v0 - 0.08f, t + 0.02f, t + 0.2f, HouseSwatch.Clay);
                for (int f = 0; f < 3; f++)
                {
                    Vector3 flower = WallPoint(spec, side, u0 + width * (0.2f + 0.3f * f), v0 - 0.02f, t + 0.11f);
                    b.Blob(flower, new Vector3(0.14f, 0.12f, 0.14f), f == 1 ? HouseSwatch.ClothYellow : HouseSwatch.Flower);
                }
            }
        }

        // ------------------------------------------------------------------
        // Roofs
        // ------------------------------------------------------------------

        private static void BuildKitRoof(HouseMeshBuilder b, HouseLevelSpec spec, HouseHullKit kit)
        {
            float w = InteriorWidth(spec);
            float d = InteriorDepth(spec);
            float top = FloorY + WallHeight;
            float o = kit.Overhang;
            float t = kit.WallThickness;

            if (kit.HipRoof)
            {
                HipRoof(b, -t - o, w + t + o, -t - o, d + t + o, top, kit.RidgeHeight, kit.Roof, kit.RoofUnder);
                Lambrequim(b, -t - o, w + t + o, -t - o, d + t + o, top);
                Dormer(b, w * 0.5f, -t - o, d, top, kit);
                return;
            }

            b.GableRoof(-t - o, w + t + o, -t - o, d + t + o, top, kit.RidgeHeight, 0.06f, kit.Roof, kit.RoofUnder);
            b.Log(new Vector3(-t - o - 0.05f, top + kit.RidgeHeight + 0.03f, d * 0.5f), new Vector3(w + t + o + 0.05f, top + kit.RidgeHeight + 0.03f, d * 0.5f), 0.09f, kit.Ridge);

            // Gable ends in the wall's material, both faces.
            HouseSwatch gable = kit.BoardWalls ? kit.WallMain : kit.Level == 3 ? HouseSwatch.Taipa : kit.WallMain;
            foreach (float x in new[] { -t, w + t })
            {
                var a = new Vector3(x, top, -t);
                var c = new Vector3(x, top, d + t);
                var r = new Vector3(x, top + kit.RidgeHeight - 0.06f, d * 0.5f);
                b.Triangle(a, r, c, gable);
                b.Triangle(a, c, r, gable);
            }

            float zMin = -t - o, zMax = d + t + o, zMid = d * 0.5f;
            float spacing = kit.TileRows ? 0.45f : 0.9f;
            int rows = Mathf.FloorToInt((w + 2f * (t + o)) / spacing);
            for (int i = 1; i < rows; i++)
            {
                float x = -t - o + i * spacing;
                // Capa-canal tile rows (N3–N4) or roof battens (N2), down both slopes.
                float r = kit.TileRows ? 0.045f : 0.025f;
                HouseSwatch swatch = kit.TileRows ? kit.Ridge : HouseSwatch.WoodDark;
                b.Log(new Vector3(x, top + 0.04f, zMin + 0.02f), new Vector3(x, top + kit.RidgeHeight + 0.02f, zMid), r, swatch);
                b.Log(new Vector3(x, top + 0.04f, zMax - 0.02f), new Vector3(x, top + kit.RidgeHeight + 0.02f, zMid), r, swatch);
            }

            if (kit.Level == 3)
            {
                // Calha de bambu along the front eave.
                b.Log(new Vector3(-t - o, top - 0.06f, zMin - 0.04f), new Vector3(w + t + o, top - 0.06f, zMin - 0.04f), 0.06f, HouseSwatch.GrassDark, HouseSwatch.Straw);
            }
        }

        /// <summary>Four-slope roof (4 águas) with the ridge along x; slopes face outwards, plus an underside.</summary>
        private static void HipRoof(HouseMeshBuilder b, float xMin, float xMax, float zMin, float zMax, float eaveY, float ridgeHeight, HouseSwatch swatch, HouseSwatch under)
        {
            float zMid = (zMin + zMax) * 0.5f;
            float run = Mathf.Min((zMax - zMin) * 0.5f, (xMax - xMin) * 0.5f);
            var r0 = new Vector3(xMin + run, eaveY + ridgeHeight, zMid);
            var r1 = new Vector3(xMax - run, eaveY + ridgeHeight, zMid);
            var s0 = new Vector3(xMin, eaveY, zMin);
            var s1 = new Vector3(xMax, eaveY, zMin);
            var n0 = new Vector3(xMin, eaveY, zMax);
            var n1 = new Vector3(xMax, eaveY, zMax);
            b.Quad(s0, r0, r1, s1, swatch);
            b.Quad(n1, r1, r0, n0, swatch);
            b.Triangle(s0, n0, r0, swatch);
            b.Triangle(n1, s1, r1, swatch);
            Vector3 down = Vector3.down * 0.06f;
            b.Quad(s0 + down, s1 + down, n1 + down, n0 + down, under);
        }

        /// <summary>The casarão's lambrequim: a row of little white wooden points hanging under the eaves (front and back).</summary>
        private static void Lambrequim(HouseMeshBuilder b, float xMin, float xMax, float zMin, float zMax, float eaveY)
        {
            const float step = 0.3f;
            int count = Mathf.FloorToInt((xMax - xMin) / step);
            for (int i = 0; i < count; i++)
            {
                float x0 = xMin + i * step, x1 = x0 + step;
                foreach (float z in new[] { zMin - 0.005f, zMax + 0.005f })
                {
                    var a = new Vector3(x0, eaveY - 0.02f, z);
                    var c = new Vector3(x1, eaveY - 0.02f, z);
                    var tip = new Vector3((x0 + x1) * 0.5f, eaveY - 0.2f, z);
                    b.Triangle(a, c, tip, HouseSwatch.White);
                    b.Triangle(a, tip, c, HouseSwatch.White);
                }
            }
        }

        /// <summary>The attic's mansard window on the front slope (sótão).</summary>
        private static void Dormer(HouseMeshBuilder b, float x, float eaveZ, float d, float top, HouseHullKit kit)
        {
            float z = eaveZ + (d * 0.5f - eaveZ) * 0.45f;
            float y = top + kit.RidgeHeight * 0.45f;
            b.BoxMinMax(new Vector3(x - 0.7f, y - 0.25f, z - 0.1f), new Vector3(x + 0.7f, y + 0.85f, z + 0.9f), HouseSwatch.White);
            b.BoxMinMax(new Vector3(x - 0.4f, y, z - 0.12f), new Vector3(x + 0.4f, y + 0.6f, z - 0.1f), HouseSwatch.Glass);
            b.BoxMinMax(new Vector3(x - 0.03f, y, z - 0.13f), new Vector3(x + 0.03f, y + 0.6f, z - 0.11f), kit.Frame);
            var l = new Vector3(x - 0.85f, y + 0.85f, z - 0.2f);
            var r = new Vector3(x + 0.85f, y + 0.85f, z - 0.2f);
            var ridge0 = new Vector3(x, y + 1.3f, z - 0.2f);
            var ridge1 = new Vector3(x, y + 1.3f, z + 1f);
            b.Quad(l, ridge0, ridge1, new Vector3(l.x, l.y, z + 1f), kit.Roof);
            b.Quad(new Vector3(r.x, r.y, z + 1f), ridge1, ridge0, r, kit.Roof);
            b.Triangle(l, r, ridge0, HouseSwatch.White);
        }

        /// <summary>Porch roof and pillars (in the roof mesh, hidden inside like N1's alpendre).</summary>
        private static void BuildPorchRoof(HouseMeshBuilder b, HouseLevelSpec spec, HouseHullKit kit)
        {
            float w = InteriorWidth(spec);
            float d = InteriorDepth(spec);
            float t = kit.WallThickness;
            float top = FloorY + WallHeight;
            float depth = kit.PorchDepth;
            float beamY = FloorY + 2.15f;
            float frontZ = -depth + 0.18f;
            int pillars = Mathf.Max(3, Mathf.RoundToInt(w / 2.5f) + 1);
            HouseOpening door = spec.EntranceDoor;
            float doorU0 = door != null ? door.Start * HouseLevelSpec.InteriorCellSize : 1f;
            float doorU1 = door != null ? (door.Start + door.Width) * HouseLevelSpec.InteriorCellSize : 2f;

            var posts = new List<Vector3>();
            for (int i = 0; i < pillars; i++) posts.Add(new Vector3(Mathf.Lerp(0.18f, w - 0.18f, i / (float)(pillars - 1)), FloorY - 0.04f, frontZ));
            if (kit.Level >= 5)
            {
                // Varanda em L: the west leg runs along the side of the house.
                int side = Mathf.Max(2, Mathf.RoundToInt(d / 2.5f));
                for (int i = 1; i <= side; i++) posts.Add(new Vector3(-depth + 0.18f, FloorY - 0.04f, Mathf.Lerp(frontZ, d - 0.18f, i / (float)side)));
                posts.Add(new Vector3(-depth + 0.18f, FloorY - 0.04f, frontZ));
            }

            foreach (Vector3 p in posts) Pillar(b, p, beamY - p.y, kit);

            // Beams.
            b.BoxMinMax(new Vector3(-0.1f - (kit.Level >= 5 ? depth : 0f), beamY, frontZ - 0.1f), new Vector3(w + 0.1f, beamY + 0.18f, frontZ + 0.1f), kit.Level >= 4 ? kit.Pillar : HouseSwatch.WoodDark);
            if (kit.Level >= 5) b.BoxMinMax(new Vector3(-depth + 0.08f, beamY, frontZ), new Vector3(-depth + 0.28f, beamY + 0.18f, d + 0.1f), kit.Pillar);

            // Arches between masonry pillars (N4): little corbels under the beam at each pillar.
            if (kit.Level == 4)
            {
                for (int i = 0; i < pillars; i++)
                {
                    Vector3 p = posts[i];
                    b.BoxMinMax(new Vector3(p.x - 0.45f, beamY - 0.25f, frontZ - 0.09f), new Vector3(p.x + 0.45f, beamY, frontZ + 0.09f), kit.Pillar);
                    b.BoxMinMax(new Vector3(p.x - 0.3f, beamY - 0.4f, frontZ - 0.09f), new Vector3(p.x + 0.3f, beamY - 0.25f, frontZ + 0.09f), kit.Pillar);
                }
            }

            // Guarda-corpo de ripa (N2) / low balustrade (N3+), open in front of the door.
            float railTop = FloorY + 0.85f;
            HouseSwatch rail = kit.Level == 2 ? HouseSwatch.WoodRaw : kit.Level == 3 ? kit.Pillar : HouseSwatch.White;
            foreach ((float x0, float x1) in new[] { (0.1f, doorU0 - 0.4f), (doorU1 + 0.4f, w - 0.1f) })
            {
                if (x1 - x0 < 0.3f) continue;
                b.BoxMinMax(new Vector3(x0, railTop - 0.06f, frontZ - 0.04f), new Vector3(x1, railTop, frontZ + 0.04f), rail);
                int slats = Mathf.FloorToInt((x1 - x0) / 0.22f);
                for (int s = 0; s <= slats; s++)
                {
                    float x = x0 + s * 0.22f;
                    b.BoxMinMax(new Vector3(x - 0.025f, FloorY - 0.03f, frontZ - 0.025f), new Vector3(x + 0.025f, railTop - 0.06f, frontZ + 0.025f), rail);
                }
            }

            // Lean-to porch roof (hip-roofed casarão: it continues the main slopes, so only the beam line shows).
            if (!kit.HipRoof)
            {
                var l0 = new Vector3(-0.2f, top - 0.1f, -t - kit.Overhang + 0.05f);
                var l1 = new Vector3(w + 0.2f, top - 0.1f, -t - kit.Overhang + 0.05f);
                var l2 = new Vector3(w + 0.2f, beamY + 0.2f, -depth - 0.2f);
                var l3 = new Vector3(-0.2f, beamY + 0.2f, -depth - 0.2f);
                b.Quad(l0, l1, l2, l3, kit.Roof);
                b.Quad(l0, l3, l2, l1, kit.RoofUnder);
            }
            else
            {
                float o = kit.Overhang;
                var a0 = new Vector3(-t - o, top - 0.05f, -t - o);
                var a1 = new Vector3(w + t + o, top - 0.05f, -t - o);
                var a2 = new Vector3(w + 0.2f, beamY + 0.2f, -depth - 0.2f);
                var a3 = new Vector3(-depth - 0.2f, beamY + 0.2f, -depth - 0.2f);
                b.Quad(a0, a1, a2, a3, kit.Roof);
                b.Quad(a0, a3, a2, a1, kit.RoofUnder);
                var w0 = new Vector3(-t - o, top - 0.05f, d + t + o);
                var w1 = new Vector3(-depth - 0.2f, beamY + 0.2f, d + 0.2f);
                b.Quad(a0, a3, w1, w0, kit.Roof);
                b.Quad(a0, w0, w1, a3, kit.RoofUnder);
            }

            if (kit.Level == 2)
            {
                // Rede na varanda "já de fábrica": a hammock between the two west pillars.
                Vector3 p0 = posts[pillars - 2] + Vector3.up * 1.5f, p1 = posts[pillars - 1] + Vector3.up * 1.5f;
                Vector3 mid = (p0 + p1) * 0.5f + Vector3.down * 0.7f;
                b.Log(p0, mid, 0.08f, HouseSwatch.ClothRed);
                b.Log(mid, p1, 0.08f, HouseSwatch.ClothYellow);
            }
        }

        private static void Pillar(HouseMeshBuilder b, Vector3 foot, float height, HouseHullKit kit)
        {
            switch (kit.Level)
            {
                case 2:
                    b.Cylinder(foot, 0.08f, 0.07f, height, 6, kit.Pillar);
                    return;
                case 3:
                    // Pilar de madeira torneada: a few rings.
                    b.Cylinder(foot, 0.1f, 0.08f, height * 0.2f, 8, kit.Pillar);
                    b.Cylinder(foot + Vector3.up * height * 0.2f, 0.065f, 0.065f, height * 0.6f, 8, kit.Pillar);
                    b.Cylinder(foot + Vector3.up * height * 0.8f, 0.08f, 0.1f, height * 0.2f, 8, kit.Pillar);
                    return;
                case 4:
                    b.BoxMinMax(foot + new Vector3(-0.14f, 0f, -0.14f), foot + new Vector3(0.14f, height, 0.14f), kit.Pillar);
                    b.BoxMinMax(foot + new Vector3(-0.16f, 0f, -0.16f), foot + new Vector3(0.16f, 0.5f, 0.16f), HouseSwatch.ClothYellow);
                    return;
                default:
                    b.BoxMinMax(foot + new Vector3(-0.17f, 0f, -0.17f), foot + new Vector3(0.17f, 0.3f, 0.17f), HouseSwatch.Quoin);
                    b.Cylinder(foot + Vector3.up * 0.3f, 0.12f, 0.1f, height - 0.45f, 8, kit.Pillar);
                    b.BoxMinMax(foot + new Vector3(-0.16f, height - 0.15f, -0.16f), foot + new Vector3(0.16f, height, 0.16f), kit.Pillar);
                    return;
            }
        }

        private static void BuildChimneys(HouseMeshBuilder b, HouseLevelSpec spec, HouseHullKit kit)
        {
            if (kit.Chimneys <= 0) return;
            float w = InteriorWidth(spec);
            float d = InteriorDepth(spec);
            float top = FloorY + WallHeight;
            for (int i = 0; i < kit.Chimneys; i++)
            {
                float x = i == 0 ? w * 0.8f : w * 0.2f;
                float z = d * 0.72f;
                HouseSwatch body = kit.Level == 3 ? HouseSwatch.Clay : HouseSwatch.RoofTileDark;
                b.BoxMinMax(new Vector3(x - 0.3f, top - 0.2f, z - 0.3f), new Vector3(x + 0.3f, top + kit.RidgeHeight + 0.6f, z + 0.3f), body);
                b.BoxMinMax(new Vector3(x - 0.36f, top + kit.RidgeHeight + 0.6f, z - 0.36f), new Vector3(x + 0.36f, top + kit.RidgeHeight + 0.72f, z + 0.36f), kit.Level == 3 ? HouseSwatch.Dirt : HouseSwatch.Concrete);
                b.BoxMinMax(new Vector3(x - 0.15f, top + kit.RidgeHeight + 0.72f, z - 0.15f), new Vector3(x + 0.15f, top + kit.RidgeHeight + 0.74f, z + 0.15f), HouseSwatch.Black);
            }
        }

        // ------------------------------------------------------------------
        // N5: the second wing (ala íntima), east of the main block, towards the back
        // ------------------------------------------------------------------

        private static void WingBounds(HouseLevelSpec spec, HouseHullKit kit, out float x0, out float x1, out float z0, out float z1)
        {
            float w = InteriorWidth(spec);
            float d = InteriorDepth(spec);
            x0 = w + kit.WallThickness;
            x1 = x0 + WingWidth;
            z1 = d + kit.WallThickness;
            z0 = z1 - WingDepth;
        }

        private static void BuildWingBase(HouseMeshBuilder b, HouseLevelSpec spec, HouseHullKit kit)
        {
            WingBounds(spec, kit, out float x0, out float x1, out float z0, out float z1);
            b.BoxMinMax(new Vector3(x0 - 0.1f, -0.02f, z0 - 0.12f), new Vector3(x1 + 0.12f, FloorY - 0.06f, z1 + 0.12f), kit.Foundation);
            b.BoxMinMax(new Vector3(x0, FloorY - 0.1f, z0), new Vector3(x1, FloorY, z1), HouseSwatch.FloorBoard);
        }

        private static void BuildWingWalls(HouseMeshBuilder b, HouseLevelSpec spec, HouseHullKit kit)
        {
            WingBounds(spec, kit, out float x0, out float x1, out float z0, out float z1);
            float t = kit.WallThickness;
            float h = FloorY + WallHeight;
            // South face with two barred windows, east face with one, north face plain; the west face is the main wall.
            WingFace(b, new Vector3(x0, FloorY, z0), Vector3.right, Vector3.back, WingWidth, new[] { 1.2f, 3.4f }, kit);
            WingFace(b, new Vector3(x1, FloorY, z0), Vector3.forward, Vector3.right, WingDepth, new[] { 3f }, kit);
            WingFace(b, new Vector3(x0, FloorY, z1), Vector3.right, Vector3.forward, WingWidth, new float[0], kit);
            // Quoins at the two free corners.
            foreach (Vector3 corner in new[] { new Vector3(x1, 0f, z0), new Vector3(x1, 0f, z1) })
            {
                for (int k = 0; k < 5; k++)
                {
                    float y = FloorY + 0.05f + k * 0.5f;
                    b.BoxMinMax(new Vector3(corner.x - 0.2f, y, corner.z - 0.2f), new Vector3(corner.x + 0.05f, y + 0.42f, corner.z + 0.2f), HouseSwatch.Quoin);
                }
            }

            b.BoxMinMax(new Vector3(x0, h - 0.14f, z0 - t - 0.03f), new Vector3(x1 + t, h, z0 - t), kit.WallAlt);
        }

        /// <summary>One solid wing face from <paramref name="origin"/> along <paramref name="along"/>, with 1 m barred windows at the given offsets.</summary>
        private static void WingFace(HouseMeshBuilder b, Vector3 origin, Vector3 along, Vector3 outward, float length, float[] windows, HouseHullKit kit)
        {
            float t = kit.WallThickness;
            var cuts = new List<float> { 0f, length };
            foreach (float u in windows)
            {
                cuts.Add(u);
                cuts.Add(u + 1f);
            }

            cuts.Sort();
            for (int i = 0; i + 1 < cuts.Count; i++)
            {
                float u0 = cuts[i], u1 = cuts[i + 1];
                bool window = System.Array.Exists(windows, u => Mathf.Abs(u - u0) < 1e-3f);
                if (window)
                {
                    FaceBox(b, origin, along, outward, u0, u1, 0f, 1f, 0f, t, kit.WallMain);
                    FaceBox(b, origin, along, outward, u0, u1, 2.1f, WallHeight, 0f, t, kit.WallMain);
                    FaceBox(b, origin, along, outward, u0, u1, 1f, 2.1f, t * 0.45f, t * 0.55f, HouseSwatch.Glass);
                    for (int r = 1; r < 5; r++)
                    {
                        float u = u0 + r * 0.2f;
                        FaceBox(b, origin, along, outward, u - 0.012f, u + 0.012f, 1f, 2.1f, t + 0.02f, t + 0.045f, HouseSwatch.Iron);
                    }

                    FaceBox(b, origin, along, outward, u0 - 0.08f, u1 + 0.08f, 0.93f, 1f, -0.02f, t + 0.1f, HouseSwatch.Quoin);
                }
                else
                {
                    FaceBox(b, origin, along, outward, u0, u1, 0f, kit.BandHeight, 0f, t, kit.Band);
                    FaceBox(b, origin, along, outward, u0, u1, kit.BandHeight, WallHeight, 0f, t, kit.WallMain);
                }
            }
        }

        private static void FaceBox(HouseMeshBuilder b, Vector3 origin, Vector3 along, Vector3 outward, float u0, float u1, float v0, float v1, float d0, float d1, HouseSwatch swatch)
        {
            Vector3 p0 = origin + along * u0 + Vector3.up * v0 + outward * d0;
            Vector3 p1 = origin + along * u1 + Vector3.up * v1 + outward * d1;
            b.BoxMinMax(Vector3.Min(p0, p1), Vector3.Max(p0, p1), swatch);
        }

        private static void BuildWingRoof(HouseMeshBuilder b, HouseLevelSpec spec, HouseHullKit kit)
        {
            WingBounds(spec, kit, out float x0, out float x1, out float z0, out float z1);
            float o = kit.Overhang * 0.8f;
            float top = FloorY + WallHeight;
            // Ridge along z for the wing: build it rotated a quarter turn around its centre.
            Matrix4x4 saved = b.Matrix;
            var center = new Vector3((x0 + x1) * 0.5f, 0f, (z0 + z1) * 0.5f);
            b.Matrix = saved * Matrix4x4.TRS(center, Quaternion.Euler(0f, 90f, 0f), Vector3.one);
            float halfLong = (z1 - z0) * 0.5f + o, halfShort = (x1 - x0) * 0.5f + o;
            HipRoof(b, -halfLong, halfLong, -halfShort, halfShort, top, kit.RidgeHeight * 0.75f, kit.Roof, kit.RoofUnder);
            Lambrequim(b, -halfLong, halfLong, -halfShort, halfShort, top);
            b.Matrix = saved;
        }
    }
}
