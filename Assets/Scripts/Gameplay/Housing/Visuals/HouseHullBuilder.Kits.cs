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
        /// <summary>Tandir flue (N3) and windcatchers (badgir, N4–N5).</summary>
        Chimney,
        Extras,
        /// <summary>The courtyard house's second wing (ala íntima, N5 only).</summary>
        Wing,
    }

    /// <summary>The colours and switches of one level's kit (world bible §2.1: adobe, iwan, carved walnut, turquoise detail).</summary>
    public sealed class HouseHullKit
    {
        public int Level;
        public float PorchDepth;
        public float WallThickness;
        public HouseSwatch WallMain;
        /// <summary>Re-plastered patches (N1–N2) and the lime coping of later levels.</summary>
        public HouseSwatch WallAlt;
        /// <summary>The lower band of the walls (mud dado, lime wash or turquoise tile).</summary>
        public HouseSwatch Band;
        public float BandHeight;
        /// <summary>The wooden lintel line just under the roof.</summary>
        public HouseSwatch Plate;
        public HouseSwatch Frame;
        /// <summary>Open shutters (N1) or the carved lattice (panjara) over the windows (N2+).</summary>
        public HouseSwatch Lattice;
        public HouseSwatch Door;
        /// <summary>Top of the flat earth roof.</summary>
        public HouseSwatch RoofTop;
        /// <summary>Roof poles: beam ends under the parapet, iwan joists.</summary>
        public HouseSwatch Beam;
        public HouseSwatch Coping;
        public float ParapetHeight;
        public HouseSwatch Pillar;
        public HouseSwatch Deck;
        public HouseSwatch DeckAlt;
        public HouseSwatch Foundation;
        public bool Glass;
        /// <summary>Carved wooden grilles over tall windows with a fan light (N5).</summary>
        public bool Grilles;
        /// <summary>Fired-brick corner piers (N5).</summary>
        public bool Quoins;
        /// <summary>Pointed arch heads over the windows (N3+).</summary>
        public bool ArchedWindows;
        public int Windcatchers;
    }

    public static partial class HouseHullBuilder
    {
        public const float WingWidth = 5f;
        public const float WingDepth = 7f;

        /// <summary>Module ids per socket and level (N1…N5), as in the §3.2 table; "-" = nothing on that socket.</summary>
        private static readonly Dictionary<HouseHullSocket, string[]> Modules = new Dictionary<HouseHullSocket, string[]>
        {
            { HouseHullSocket.Foundation, new[] { "sufa_barro", "sufa_adobe", "baldrame_pedra", "pedestal_tijolo_degraus", "porao_pedra_escadaria" } },
            { HouseHullSocket.Floor, new[] { "chao_batido_esteira", "tijolo_cru", "tijoleira", "ceramica_turquesa", "ladrilho_e_tabua" } },
            { HouseHullSocket.Wall, new[] { "adobe_cru", "adobe_rebocado", "ganch_barrado_ocre", "cal_barrado_azulejo", "alvenaria_cunhais" } },
            { HouseHullSocket.Window, new[] { "janela_postigo", "janela_panjara", "janela_arco_panjara", "janela_vidro_panjara", "janela_bandeira_grade" } },
            { HouseHullSocket.Door, new[] { "porta_tabua", "porta_batente_roseta", "porta_entalhada", "porta_entalhada_pintada", "porta_dupla_leque" } },
            { HouseHullSocket.Roof, new[] { "teto_plano_junco", "teto_plano_parapeito", "teto_plano_vigas", "teto_plano_cal", "teto_plano_balakhana" } },
            { HouseHullSocket.Porch, new[] { "ayvan_varas", "ayvan_colunas_finas", "iwan_colunas_entalhadas", "iwan_arcos", "iwan_em_l" } },
            { HouseHullSocket.Chimney, new[] { "-", "-", "chamine_tandir", "badgir", "badgir_duplo" } },
            { HouseHullSocket.Extras, new[] { "varal_tandir", "tapchan_iwan", "aryk_ponte", "latada_muro", "latada_ninho_sino" } },
            { HouseHullSocket.Wing, new[] { "-", "-", "-", "-", "ala_intima" } },
        };

        /// <summary>The module a level uses on a socket ("-" when the socket is empty at that level).</summary>
        public static string ModuleId(HouseHullSocket socket, int level)
        {
            string[] row = Modules[socket];
            return row[Mathf.Clamp(level, HouseLevels.MinLevel, HouseLevels.MaxLevel) - 1];
        }

        /// <summary>Porch depth in metres (N1 ayvan 1.5 m; N2+ iwan 2–2.5 m); the yard starts past it.</summary>
        public static float PorchDepthOf(HouseLevelSpec spec) => spec == null || spec.Level <= 1 ? PorchDepth : KitOf(spec.Level).PorchDepth;

        /// <summary>The kit of a level.</summary>
        public static HouseHullKit KitOf(int level)
        {
            switch (Mathf.Clamp(level, HouseLevels.MinLevel, HouseLevels.MaxLevel))
            {
                case 1:
                    // Casinha de adobe: raw apricot adobe with a dark mud dado, a few re-plastered patches, a low parapet,
                    // poplar poles, a pole-and-reed ayvan, little open shutters.
                    return new HouseHullKit
                    {
                        Level = 1, PorchDepth = PorchDepth, WallThickness = WallThickness, BandHeight = 0.35f, ParapetHeight = 0.22f,
                        WallMain = HouseSwatch.Adobe, WallAlt = HouseSwatch.Taipa, Band = HouseSwatch.Ochre, Plate = HouseSwatch.WoodDark,
                        Frame = HouseSwatch.WoodDark, Lattice = HouseSwatch.WoodRaw, Door = HouseSwatch.WoodRaw, RoofTop = HouseSwatch.Dirt,
                        Beam = HouseSwatch.WoodRaw, Coping = HouseSwatch.Taipa, Pillar = HouseSwatch.WoodRaw, Deck = HouseSwatch.Dirt,
                        DeckAlt = HouseSwatch.Dirt, Foundation = HouseSwatch.Ochre,
                    };
                case 2:
                    // Adobe rebocado: smoothed adobe, a lime-washed dado, walnut lattice windows, thin walnut columns.
                    return new HouseHullKit
                    {
                        Level = 2, PorchDepth = 2f, WallThickness = 0.2f, BandHeight = 0.5f, ParapetHeight = 0.32f,
                        WallMain = HouseSwatch.Adobe, WallAlt = HouseSwatch.Plaster, Band = HouseSwatch.White, Plate = HouseSwatch.Walnut,
                        Frame = HouseSwatch.Walnut, Lattice = HouseSwatch.Walnut, Door = HouseSwatch.Walnut, RoofTop = HouseSwatch.Dirt,
                        Beam = HouseSwatch.WoodDark, Coping = HouseSwatch.White, Pillar = HouseSwatch.Walnut, Deck = HouseSwatch.Taipa,
                        DeckAlt = HouseSwatch.Clay, Foundation = HouseSwatch.Ochre,
                    };
                case 3:
                    // Ganch (gypsum plaster) over the adobe, an ochre dado, carved columns with fan capitals, arched lattice
                    // windows, a tandir flue on the roof.
                    return new HouseHullKit
                    {
                        Level = 3, PorchDepth = 2f, WallThickness = 0.22f, BandHeight = 0.6f, ParapetHeight = 0.38f,
                        WallMain = HouseSwatch.Plaster, WallAlt = HouseSwatch.White, Band = HouseSwatch.Ochre, Plate = HouseSwatch.Walnut,
                        Frame = HouseSwatch.Walnut, Lattice = HouseSwatch.Walnut, Door = HouseSwatch.Walnut, RoofTop = HouseSwatch.Taipa,
                        Beam = HouseSwatch.Walnut, Coping = HouseSwatch.White, Pillar = HouseSwatch.Walnut, Deck = HouseSwatch.Clay,
                        DeckAlt = HouseSwatch.RoofTileDark, Foundation = HouseSwatch.Stone, ArchedWindows = true, Windcatchers = 1,
                    };
                case 4:
                    // Lime and tile: white walls, a turquoise tile dado (detail), glazed lattice windows, an arcaded iwan,
                    // one windcatcher.
                    return new HouseHullKit
                    {
                        Level = 4, PorchDepth = 2f, WallThickness = 0.24f, BandHeight = 0.6f, ParapetHeight = 0.42f,
                        WallMain = HouseSwatch.White, WallAlt = HouseSwatch.Plaster, Band = HouseSwatch.Turquoise, Plate = HouseSwatch.Walnut,
                        Frame = HouseSwatch.Walnut, Lattice = HouseSwatch.Walnut, Door = HouseSwatch.Turquoise, RoofTop = HouseSwatch.Taipa,
                        Beam = HouseSwatch.Walnut, Coping = HouseSwatch.Plaster, Pillar = HouseSwatch.White, Deck = HouseSwatch.Clay,
                        DeckAlt = HouseSwatch.Turquoise, Foundation = HouseSwatch.Ochre, Glass = true, ArchedWindows = true, Windcatchers = 1,
                    };
                default:
                    // The courtyard house: lime over fired-brick corners, tall windows with fan lights and carved grilles,
                    // an L-shaped iwan on carved columns, two windcatchers and a rooftop pavilion (balakhana).
                    return new HouseHullKit
                    {
                        Level = 5, PorchDepth = 2.5f, WallThickness = 0.26f, BandHeight = 0.5f, ParapetHeight = 0.45f,
                        WallMain = HouseSwatch.White, WallAlt = HouseSwatch.Plaster, Band = HouseSwatch.Turquoise, Plate = HouseSwatch.Walnut,
                        Frame = HouseSwatch.Walnut, Lattice = HouseSwatch.Walnut, Door = HouseSwatch.Walnut, RoofTop = HouseSwatch.Taipa,
                        Beam = HouseSwatch.Walnut, Coping = HouseSwatch.Ochre, Pillar = HouseSwatch.Walnut, Deck = HouseSwatch.Ladrilho,
                        DeckAlt = HouseSwatch.White, Foundation = HouseSwatch.Quoin, Glass = true, Grilles = true, Quoins = true, Windcatchers = 2,
                    };
            }
        }

        // ------------------------------------------------------------------
        // Build: one module per socket
        // ------------------------------------------------------------------

        private static HouseHullMeshes BuildKit(HouseLevelSpec spec)
        {
            HouseHullKit kit = KitOf(spec.Level);
            string prefix = "HouseN" + kit.Level + "_";
            var meshes = new HouseHullMeshes();
            var b = new HouseMeshBuilder();

            BuildGround(b, spec, kit);
            BuildFoundation(b, spec, kit);
            BuildFloor(b, spec, kit);
            BuildPorchDeck(b, spec, kit);
            BuildYardTree(b, spec, YardOrigin(spec), kit.Level);
            BuildExtras(b, spec, kit);
            if (kit.Level >= 5) BuildWingBase(b, spec, kit);
            meshes.Base = b.ToMesh(prefix + "Base");

            foreach (HouseWallSide side in new[] { HouseWallSide.South, HouseWallSide.East, HouseWallSide.North, HouseWallSide.West })
            {
                b.Clear();
                BuildSolidWall(b, spec, side, kit);
                BuildOpenings(b, spec, side, kit);
                // The second wing hangs off the east side, so it hides with the east wall in the cutaway.
                if (side == HouseWallSide.East && kit.Level >= 5) BuildWingWalls(b, spec, kit);
                meshes.Walls[(int)side] = b.ToMesh(prefix + "Wall_" + side);
            }

            b.Clear();
            BuildFlatRoof(b, spec, kit);
            BuildPorchRoof(b, spec, kit);
            BuildWindcatchers(b, spec, kit);
            if (kit.Level >= 5)
            {
                BuildBalakhana(b, spec, kit);
                BuildWingRoof(b, spec, kit);
            }

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
            float east = Mathf.Max(yard.x + yardW, w + (kit.Level >= 5 ? WingWidth : 0f)) + 3f;
            b.BoxMinMax(new Vector3(Mathf.Min(yard.x, 0f) - 3f, -0.2f, yard.y - 3f), new Vector3(east, -0.02f, d + 3f), HouseSwatch.GrassDark, HouseSwatch.GrassDark);
            b.BoxMinMax(new Vector3(yard.x, -0.05f, yard.y), new Vector3(yard.x + yardW, 0f, yard.y + yardD), HouseSwatch.Grass);

            // Steps up to the iwan in front of the door, then the path to the yard entrance.
            HouseOpening door = spec.EntranceDoor;
            float doorU0 = door != null ? door.Start * HouseLevelSpec.InteriorCellSize : 1f;
            float doorU1 = door != null ? (door.Start + door.Width) * HouseLevelSpec.InteriorCellSize : 2f;
            float wide = kit.Level >= 5 ? 0.8f : 0.1f;
            int steps = kit.Level >= 4 ? 3 : 2;
            for (int i = 0; i < steps; i++)
            {
                float stepTop = FloorY - 0.04f - (i + 1) * (FloorY / (steps + 1));
                b.BoxMinMax(new Vector3(doorU0 - wide - i * 0.1f, -0.02f, -kit.PorchDepth - 0.3f * (i + 1)), new Vector3(doorU1 + wide + i * 0.1f, stepTop, -kit.PorchDepth - 0.3f * i),
                    kit.Foundation, kit.Level >= 3 ? HouseSwatch.Stone : kit.Deck);
            }

            float pathX = yard.x + (spec.YardEntranceX + 0.5f) * HouseLevelSpec.YardCellSize;
            float doorX = (doorU0 + doorU1) * 0.5f;
            b.BoxMinMax(new Vector3(Mathf.Min(pathX, doorX) - 0.45f, -0.01f, yard.y + yardD - 1f), new Vector3(Mathf.Max(pathX, doorX) + 0.45f, 0.005f, -kit.PorchDepth - 0.3f * steps),
                kit.Level >= 4 ? HouseSwatch.Clay : HouseSwatch.Dirt);
        }

        /// <summary>The sufa: an earth (N1), adobe (N2), stone (N3), brick (N4) or stone-and-vents (N5) plinth under the house.</summary>
        private static void BuildFoundation(HouseMeshBuilder b, HouseLevelSpec spec, HouseHullKit kit)
        {
            float w = InteriorWidth(spec);
            float d = InteriorDepth(spec);
            float t = kit.WallThickness;
            float grow = kit.Level >= 5 ? 0.12f : 0.04f;
            b.BoxMinMax(new Vector3(-t - grow, -0.02f, -t - grow), new Vector3(w + t + grow, FloorY - 0.06f, d + t + grow), kit.Foundation);
            if (kit.Level >= 4)
            {
                // Brick paving around the back and the sides.
                b.BoxMinMax(new Vector3(-t - 0.7f, -0.02f, -0.05f), new Vector3(-t - grow, 0.04f, d + t + 0.7f), HouseSwatch.Clay);
                b.BoxMinMax(new Vector3(-t - 0.7f, -0.02f, d + t + grow), new Vector3(w + t + 0.7f, 0.04f, d + t + 0.7f), HouseSwatch.Clay);
                if (kit.Level < 5) b.BoxMinMax(new Vector3(w + t + grow, -0.02f, -0.05f), new Vector3(w + t + 0.7f, 0.04f, d + t + 0.7f), HouseSwatch.Clay);
            }

            if (kit.Level >= 5)
            {
                // Vents of the cellar.
                for (int i = 1; i < 6; i++)
                {
                    float x = w * i / 6f;
                    b.BoxMinMax(new Vector3(x - 0.2f, 0.08f, -t - grow - 0.01f), new Vector3(x + 0.2f, 0.2f, -t - grow), HouseSwatch.Black);
                }
            }
        }

        private static void BuildFloor(HouseMeshBuilder b, HouseLevelSpec spec, HouseHullKit kit)
        {
            float w = InteriorWidth(spec);
            float d = InteriorDepth(spec);
            float t = kit.WallThickness;
            switch (kit.Level)
            {
                case 1:
                    // Beaten earth with a reed mat in the middle.
                    b.BoxMinMax(new Vector3(-t, FloorY - 0.1f, -t), new Vector3(w + t, FloorY, d + t), HouseSwatch.Dirt);
                    b.BoxMinMax(new Vector3(w * 0.2f, FloorY - 0.02f, d * 0.2f), new Vector3(w * 0.8f, FloorY + 0.004f, d * 0.8f), HouseSwatch.Reed);
                    return;
                case 5:
                {
                    // Tiles in the front half (the guests' side), boards in the back (the bedrooms).
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
                    // N2 mud-brick pavers, N3 fired brick, N4 lime floor with turquoise tiles.
                    HouseSwatch slab = kit.Level == 2 ? HouseSwatch.Dirt : kit.Level == 3 ? HouseSwatch.Clay : HouseSwatch.Plaster;
                    HouseSwatch tile = kit.Level == 2 ? HouseSwatch.Taipa : kit.Level == 3 ? HouseSwatch.RoofTileDark : HouseSwatch.Turquoise;
                    b.BoxMinMax(new Vector3(-t, FloorY - 0.1f, -t), new Vector3(w + t, FloorY, d + t), slab);
                    Checker(b, 0f, w, 0f, d, 1f, tile);
                    return;
                }
            }
        }

        /// <summary>Every other square of a checker, a hair above the floor (furniture still stands at <see cref="FloorY"/>).</summary>
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

        /// <summary>The raised earth or tiled deck of the ayvan / iwan; the courtyard house's turns the west corner.</summary>
        private static void BuildPorchDeck(HouseMeshBuilder b, HouseLevelSpec spec, HouseHullKit kit)
        {
            float w = InteriorWidth(spec);
            float depth = kit.PorchDepth;
            float grow = kit.Level >= 5 ? 0.12f : 0.04f;
            b.BoxMinMax(new Vector3(-kit.WallThickness - grow, -0.02f, -depth), new Vector3(w + kit.WallThickness + grow, FloorY - 0.005f, -kit.WallThickness), kit.Foundation, kit.Deck);
            if (kit.Level >= 2) Checker(b, 0f, w, -depth + 0.1f, -kit.WallThickness, kit.Level == 2 ? 1f : 0.5f, kit.DeckAlt);
            if (kit.Level >= 5)
            {
                float d = InteriorDepth(spec);
                b.BoxMinMax(new Vector3(-depth, -0.02f, -depth), new Vector3(-kit.WallThickness - grow, FloorY - 0.005f, d), kit.Foundation, kit.Deck);
                Checker(b, -depth + 0.1f, -kit.WallThickness - grow, -depth, d, 0.5f, kit.DeckAlt);
            }
        }

        /// <summary>The cousin's apricot grows with the house (§2.1): a seedling at N1, a fruiting tree at N5.</summary>
        private static void BuildYardTree(HouseMeshBuilder b, HouseLevelSpec spec, Vector2 yard, int level)
        {
            if (level <= 1)
            {
                BuildSeedling(b, spec, yard);
                return;
            }

            CellRect tree = spec.FixedTree;
            var center = new Vector3(yard.x + (tree.X + tree.Width * 0.5f) * HouseLevelSpec.YardCellSize, 0f,
                yard.y + (tree.Y + tree.Height * 0.5f) * HouseLevelSpec.YardCellSize);
            float grow = (level - 1) / 4f; // 0.25 … 1
            float trunk = Mathf.Lerp(0.07f, 0.2f, grow);
            float height = Mathf.Lerp(1.1f, 2.4f, grow);
            b.Cylinder(center, 0.6f, 0.55f, 0.04f, 8, HouseSwatch.Dirt);
            b.Cylinder(center, trunk, trunk * 0.7f, height, 6, HouseSwatch.WoodDark);
            float crown = Mathf.Lerp(0.7f, 2.2f, grow);
            b.Blob(center + Vector3.up * (height + crown * 0.2f), new Vector3(crown, crown * 0.75f, crown), HouseSwatch.Leaf);
            b.Blob(center + new Vector3(crown * 0.3f, height - crown * 0.05f, 0.1f), new Vector3(crown * 0.6f, crown * 0.45f, crown * 0.6f), HouseSwatch.GrassDark);
            b.Blob(center + new Vector3(-crown * 0.28f, height + crown * 0.05f, -crown * 0.15f), new Vector3(crown * 0.55f, crown * 0.42f, crown * 0.55f), HouseSwatch.Leaf);
            if (level >= 3)
            {
                // Apricots once the tree is grown (a few at N3, a full crop at N5).
                int fruits = level >= 5 ? 9 : level == 4 ? 6 : 3;
                for (int i = 0; i < fruits; i++)
                {
                    float a = i * 2.4f;
                    var p = center + new Vector3(Mathf.Cos(a) * crown * 0.42f, height + Mathf.Sin(i * 1.7f) * crown * 0.18f, Mathf.Sin(a) * crown * 0.42f);
                    b.Blob(p, new Vector3(0.12f, 0.12f, 0.12f), HouseSwatch.Fruit);
                }
            }
        }

        private static void BuildExtras(HouseMeshBuilder b, HouseLevelSpec spec, HouseHullKit kit)
        {
            float w = InteriorWidth(spec);
            float d = InteriorDepth(spec);
            switch (kit.Level)
            {
                case 1:
                {
                    // Clothes line on the east side (suzani colours) and the tandir by the ayvan.
                    b.Cylinder(new Vector3(w + 0.9f, -0.02f, 0.4f), 0.05f, 0.04f, 1.7f, 5, HouseSwatch.WoodDark);
                    b.Cylinder(new Vector3(w + 0.9f, -0.02f, d - 0.4f), 0.05f, 0.04f, 1.7f, 5, HouseSwatch.WoodDark);
                    b.BoxMinMax(new Vector3(w + 0.885f, 1.6f, 0.4f), new Vector3(w + 0.915f, 1.62f, d - 0.4f), HouseSwatch.White);
                    b.BoxMinMax(new Vector3(w + 0.86f, 1.2f, 1.1f), new Vector3(w + 0.94f, 1.6f, 1.6f), HouseSwatch.ClothRed);
                    b.BoxMinMax(new Vector3(w + 0.86f, 1.3f, 2.0f), new Vector3(w + 0.94f, 1.6f, 2.4f), HouseSwatch.ClothYellow);
                    Tandir(b, new Vector3(-0.9f, -0.02f, -PorchDepth - 0.4f), 1f);
                    return;
                }
                case 2:
                {
                    // Tapchan on the iwan, right of the door: a low wooden platform with a rug and a little table.
                    float x0 = w * 0.6f;
                    b.BoxMinMax(new Vector3(x0, FloorY + 0.3f, -1.7f), new Vector3(x0 + 1.5f, FloorY + 0.38f, -0.45f), HouseSwatch.Walnut);
                    foreach (Vector2 c in new[] { new Vector2(0.06f, -1.64f), new Vector2(1.38f, -1.64f), new Vector2(0.06f, -0.58f), new Vector2(1.38f, -0.58f) })
                    {
                        b.BoxMinMax(new Vector3(x0 + c.x, FloorY - 0.01f, c.y), new Vector3(x0 + c.x + 0.07f, FloorY + 0.3f, c.y + 0.07f), HouseSwatch.WoodDark);
                    }

                    b.BoxMinMax(new Vector3(x0 + 0.05f, FloorY + 0.38f, -1.65f), new Vector3(x0 + 1.45f, FloorY + 0.4f, -0.5f), HouseSwatch.ClothRed);
                    b.BoxMinMax(new Vector3(x0 + 0.5f, FloorY + 0.4f, -1.3f), new Vector3(x0 + 1.0f, FloorY + 0.58f, -0.85f), HouseSwatch.Walnut);
                    Tandir(b, new Vector3(-1.1f, -0.02f, -0.6f), 1.1f);
                    return;
                }
                case 3:
                {
                    // An aryk (narrow water channel) along the west side with a plank bridge, and copper jugs by the wall.
                    b.BoxMinMax(new Vector3(-1.45f, -0.06f, -kit.PorchDepth - 2f), new Vector3(-1.05f, 0.03f, d + 1.5f), HouseSwatch.Stone);
                    b.BoxMinMax(new Vector3(-1.38f, -0.06f, -kit.PorchDepth - 2f), new Vector3(-1.12f, 0.035f, d + 1.5f), HouseSwatch.Water);
                    b.BoxMinMax(new Vector3(-1.55f, 0.03f, d * 0.4f - 0.3f), new Vector3(-0.95f, 0.09f, d * 0.4f + 0.3f), HouseSwatch.WoodRaw);
                    for (int i = 0; i < 3; i++)
                    {
                        var jug = new Vector3(-0.55f, -0.02f, d * 0.15f + i * 0.4f);
                        b.Cylinder(jug, 0.12f, 0.17f, 0.25f, 7, HouseSwatch.Copper);
                        b.Cylinder(jug + Vector3.up * 0.25f, 0.17f, 0.06f, 0.2f, 7, HouseSwatch.Copper, HouseSwatch.Black);
                    }

                    return;
                }
                case 4:
                {
                    // A grape trellis (latada) on the west side and a low adobe garden wall (duval) behind the house.
                    Latada(b, -2.6f, -0.6f, 0.2f, d - 0.2f, 2.2f);
                    b.BoxMinMax(new Vector3(-2.8f, -0.02f, d + 1.4f), new Vector3(w + 2.5f, 0.85f, d + 1.65f), HouseSwatch.Adobe, HouseSwatch.Plaster);
                    b.BoxMinMax(new Vector3(-2.8f, -0.02f, d + 1.38f), new Vector3(w + 2.5f, 0.3f, d + 1.67f), HouseSwatch.Ochre);
                    return;
                }
                default:
                {
                    // A grape trellis at the back, the aryk along it and the bell at the orchard gate.
                    Latada(b, w * 0.15f, w * 0.85f, d + 0.6f, d + 2.2f, 2.4f);
                    b.BoxMinMax(new Vector3(w * 0.1f, -0.06f, d + 2.45f), new Vector3(w * 0.9f, 0.03f, d + 2.85f), HouseSwatch.Stone);
                    b.BoxMinMax(new Vector3(w * 0.1f, -0.06f, d + 2.52f), new Vector3(w * 0.9f, 0.035f, d + 2.78f), HouseSwatch.Water);

                    Vector2 yard = YardOrigin(spec);
                    var post = new Vector3(yard.x - 0.6f, -0.02f, yard.y + 0.6f);
                    b.BoxMinMax(post + new Vector3(-0.14f, 0f, -0.14f), post + new Vector3(0.14f, 2f, 0.14f), HouseSwatch.Adobe, HouseSwatch.Plaster);
                    b.BoxMinMax(post + new Vector3(-0.07f, 1.92f, -0.05f), post + new Vector3(0.5f, 2f, 0.05f), HouseSwatch.Walnut);
                    b.Cylinder(post + new Vector3(0.38f, 1.6f, 0f), 0.13f, 0.06f, 0.3f, 7, HouseSwatch.Copper);
                    return;
                }
            }
        }

        /// <summary>The tandir: a round clay oven, wider at the base, with a dark mouth on top and a lid leaning on it.</summary>
        private static void Tandir(HouseMeshBuilder b, Vector3 foot, float s)
        {
            b.Cylinder(foot, 0.42f * s, 0.26f * s, 0.6f * s, 8, HouseSwatch.Clay);
            b.Cylinder(foot + Vector3.up * 0.6f * s, 0.26f * s, 0.2f * s, 0.06f * s, 8, HouseSwatch.Taipa, HouseSwatch.Black);
            b.Box(foot + new Vector3(0.36f, 0.25f, -0.1f) * s, new Vector3(0.05f, 0.4f, 0.3f) * s, HouseSwatch.WoodDark, 15f);
        }

        /// <summary>A grape trellis: posts, beams and slats over a leafy roof with bunches of grapes.</summary>
        private static void Latada(HouseMeshBuilder b, float x0, float x1, float z0, float z1, float height)
        {
            foreach (float x in new[] { x0, x1 })
            {
                foreach (float z in new[] { z0, z1 })
                {
                    b.BoxMinMax(new Vector3(x - 0.06f, -0.02f, z - 0.06f), new Vector3(x + 0.06f, height, z + 0.06f), HouseSwatch.Walnut);
                }

                b.BoxMinMax(new Vector3(x - 0.07f, height, z0 - 0.15f), new Vector3(x + 0.07f, height + 0.1f, z1 + 0.15f), HouseSwatch.Walnut);
            }

            int slats = Mathf.Max(3, Mathf.RoundToInt((z1 - z0) / 0.5f));
            for (int i = 0; i <= slats; i++)
            {
                float z = Mathf.Lerp(z0, z1, i / (float)slats);
                b.BoxMinMax(new Vector3(x0 - 0.2f, height + 0.1f, z - 0.03f), new Vector3(x1 + 0.2f, height + 0.16f, z + 0.03f), HouseSwatch.WoodDark);
            }

            float cx = (x0 + x1) * 0.5f, cz = (z0 + z1) * 0.5f;
            b.Blob(new Vector3(cx, height + 0.25f, cz), new Vector3((x1 - x0) * 0.9f, 0.35f, (z1 - z0) * 0.9f), HouseSwatch.Leaf);
            b.Blob(new Vector3(cx + (x1 - x0) * 0.2f, height + 0.2f, cz - (z1 - z0) * 0.2f), new Vector3((x1 - x0) * 0.5f, 0.3f, (z1 - z0) * 0.45f), HouseSwatch.Poplar);
            for (int i = 0; i < 4; i++)
            {
                var bunch = new Vector3(Mathf.Lerp(x0, x1, 0.2f + i * 0.2f), height - 0.12f, Mathf.Lerp(z0, z1, (i * 0.37f) % 1f * 0.8f + 0.1f));
                b.Blob(bunch, new Vector3(0.16f, 0.24f, 0.16f), HouseSwatch.Grape);
            }
        }

        // ------------------------------------------------------------------
        // Walls
        // ------------------------------------------------------------------

        /// <summary>
        /// Solid adobe / plastered walls: one box per run between the openings, the lower band, re-plastered patches on
        /// the humbler houses, a wooden lintel line under the roof and the courtyard house's brick corner piers.
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

            // Re-plastered patches on the raw adobe (N1–N2): the house is lived in and mended.
            if (kit.Level <= 2)
            {
                WallBox(b, spec, side, length * 0.12f, length * 0.24f, 1.5f, 2.05f, t, t + 0.01f, kit.WallAlt);
                WallBox(b, spec, side, length * 0.7f, length * 0.8f, 0.55f, 0.95f, t, t + 0.01f, kit.WallAlt);
            }

            // Wooden lintel line under the roof.
            WallBox(b, spec, side, start, end, WallHeight - 0.14f, WallHeight - 0.04f, t, t + 0.03f, kit.Plate);

            if (kit.Quoins && (side == HouseWallSide.South || side == HouseWallSide.North))
            {
                foreach (float corner in new[] { start, end - 0.35f })
                {
                    for (int k = 0; k < 5; k++)
                    {
                        float v0 = 0.05f + k * 0.5f;
                        float wide = k % 2 == 0 ? 0.35f : 0.25f;
                        float u0 = corner < 0.01f ? corner : corner + (0.35f - wide);
                        WallBox(b, spec, side, u0, u0 + wide, v0, Mathf.Min(v0 + 0.42f, WallHeight - 0.15f), t, t + 0.025f, HouseSwatch.Ochre);
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

                // Frame (jambs and a heavy walnut lintel).
                WallBox(b, spec, side, u0 - 0.07f, u0, v0, v1 + 0.07f, -0.01f, t + 0.04f, kit.Frame);
                WallBox(b, spec, side, u1, u1 + 0.07f, v0, v1 + 0.07f, -0.01f, t + 0.04f, kit.Frame);
                WallBox(b, spec, side, u0 - 0.12f, u1 + 0.12f, v1, v1 + 0.1f, -0.01f, t + 0.05f, kit.Frame);

                if (opening.IsDoor)
                {
                    if (kit.Level >= 5)
                    {
                        // Double door, both leaves open inwards, with a fan light above.
                        WallBox(b, spec, side, u0 - 0.06f, u0 - 0.01f, 0.02f, v1 - 0.04f, -width * 0.5f - 0.02f, -0.02f, kit.Door);
                        WallBox(b, spec, side, u1 + 0.01f, u1 + 0.06f, 0.02f, v1 - 0.04f, -width * 0.5f - 0.02f, -0.02f, kit.Door);
                        WallBox(b, spec, side, u0, u1, v1 + 0.1f, v1 + 0.45f, t * 0.4f, t * 0.6f, HouseSwatch.Glass);
                        for (int r = 0; r < 3; r++)
                        {
                            float u = u0 + width * (0.25f + r * 0.25f);
                            WallBox(b, spec, side, u - 0.02f, u + 0.02f, v1 + 0.1f, v1 + 0.45f, t * 0.35f, t * 0.65f, kit.Frame);
                        }

                        WallBox(b, spec, side, u0 - 0.07f, u1 + 0.07f, v1 + 0.45f, v1 + 0.52f, -0.01f, t + 0.04f, kit.Frame);
                        WallBox(b, spec, side, u0 - 0.12f, u1 + 0.12f, v1 + 0.52f, v1 + 0.56f, t, t + 0.05f, HouseSwatch.Turquoise);
                    }
                    else
                    {
                        // One leaf open inwards; from N2 it has carved panels with a rosette each.
                        WallBox(b, spec, side, u1 + 0.02f, u1 + 0.07f, 0.02f, v1 - 0.04f, -width - 0.02f, -0.02f, kit.Door);
                        WallBox(b, spec, side, u1 + 0.07f, u1 + 0.1f, v1 * 0.45f, v1 * 0.5f, -width * 0.8f, -width * 0.6f, HouseSwatch.Iron);
                        if (kit.Level >= 2)
                        {
                            WallBox(b, spec, side, u1 + 0.07f, u1 + 0.09f, 0.3f, v1 * 0.42f, -width * 0.85f, -width * 0.15f, HouseSwatch.WoodDark);
                            WallBox(b, spec, side, u1 + 0.07f, u1 + 0.09f, v1 * 0.55f, v1 - 0.25f, -width * 0.85f, -width * 0.15f, HouseSwatch.WoodDark);
                            WallBox(b, spec, side, u1 + 0.09f, u1 + 0.11f, v1 * 0.25f, v1 * 0.25f + 0.12f, -width * 0.56f, -width * 0.44f, HouseSwatch.Gold);
                            WallBox(b, spec, side, u1 + 0.09f, u1 + 0.11f, v1 * 0.7f, v1 * 0.7f + 0.12f, -width * 0.56f, -width * 0.44f, HouseSwatch.Gold);
                        }
                    }

                    // Threshold.
                    WallBox(b, spec, side, u0 - 0.1f, u1 + 0.1f, -0.04f, 0.01f, -0.05f, t + 0.08f, kit.Level >= 3 ? HouseSwatch.Stone : kit.Plate);
                    continue;
                }

                // Sill.
                WallBox(b, spec, side, u0 - 0.08f, u1 + 0.08f, v0 - 0.07f, v0, -0.04f, t + 0.1f, kit.Level >= 5 ? HouseSwatch.Stone : kit.Frame);
                if (kit.Glass)
                {
                    WallBox(b, spec, side, u0, u1, v0, v1, t * 0.45f, t * 0.55f, HouseSwatch.Glass);
                }

                if (kit.Grilles)
                {
                    // Tall window: carved wooden grille and a fan light (bandeira) above.
                    for (int r = 1; r < 5; r++)
                    {
                        float u = u0 + width * r / 5f;
                        WallBox(b, spec, side, u - 0.015f, u + 0.015f, v0, v1, t + 0.02f, t + 0.045f, kit.Lattice);
                    }

                    WallBox(b, spec, side, u0, u1, (v0 + v1) * 0.5f - 0.015f, (v0 + v1) * 0.5f + 0.015f, t + 0.02f, t + 0.045f, kit.Lattice);
                    WallBox(b, spec, side, u0, u1, v1 + 0.1f, v1 + 0.35f, t * 0.45f, t * 0.55f, HouseSwatch.Glass);
                    WallBox(b, spec, side, u0 - 0.07f, u1 + 0.07f, v1 + 0.35f, v1 + 0.42f, -0.01f, t + 0.04f, kit.Frame);
                }
                else if (kit.Level <= 1)
                {
                    // Little board shutters folded open.
                    WallBox(b, spec, side, u0 - 0.07f - width * 0.5f, u0 - 0.07f, v0, v1, t + 0.02f, t + 0.05f, kit.Lattice);
                    WallBox(b, spec, side, u1 + 0.07f, u1 + 0.07f + width * 0.5f, v0, v1, t + 0.02f, t + 0.05f, kit.Lattice);
                }
                else
                {
                    // Panjara: a carved lattice of two mullions and two transoms over the opening.
                    for (int r = 1; r <= 2; r++)
                    {
                        float u = u0 + width * r / 3f;
                        WallBox(b, spec, side, u - 0.02f, u + 0.02f, v0, v1, t + 0.01f, t + 0.04f, kit.Lattice);
                        float v = v0 + (v1 - v0) * r / 3f;
                        WallBox(b, spec, side, u0, u1, v - 0.02f, v + 0.02f, t + 0.01f, t + 0.04f, kit.Lattice);
                    }
                }

                if (kit.ArchedWindows && !kit.Grilles)
                {
                    // Pointed arch head over the lintel, a turquoise key at the top (detail only).
                    float rise = Mathf.Min(0.32f, WallHeight - (v1 + 0.1f) - 0.18f);
                    if (rise > 0.08f)
                    {
                        float mid = (u0 + u1) * 0.5f;
                        WallTriangle(b, spec, side, new Vector2(u0 - 0.12f, v1 + 0.1f), new Vector2(u1 + 0.12f, v1 + 0.1f), new Vector2(mid, v1 + 0.1f + rise), t + 0.03f, kit.Frame);
                        WallTriangle(b, spec, side, new Vector2(u0 + 0.02f, v1 + 0.1f), new Vector2(u1 - 0.02f, v1 + 0.1f), new Vector2(mid, v1 + 0.06f + rise), t + 0.035f, kit.WallAlt);
                        WallBox(b, spec, side, mid - 0.05f, mid + 0.05f, v1 + rise - 0.02f, v1 + rise + 0.1f, t + 0.03f, t + 0.05f, HouseSwatch.Turquoise);
                    }
                }

                // Clay pots on every sill (§3.3: there is always a flower at the window).
                WallBox(b, spec, side, u0, u1, v0 - 0.24f, v0 - 0.08f, t + 0.02f, t + 0.2f, HouseSwatch.Clay);
                for (int f = 0; f < 3; f++)
                {
                    Vector3 flower = WallPoint(spec, side, u0 + width * (0.2f + 0.3f * f), v0 - 0.02f, t + 0.11f);
                    b.Blob(flower, new Vector3(0.14f, 0.12f, 0.14f), f == 1 ? HouseSwatch.ClothYellow : HouseSwatch.Flower);
                    if (kit.Level <= 1) b.Blob(flower + Vector3.down * 0.06f, new Vector3(0.18f, 0.08f, 0.18f), HouseSwatch.Leaf);
                }
            }
        }

        // ------------------------------------------------------------------
        // Roofs: flat earth roofs with a parapet (bible §2.1)
        // ------------------------------------------------------------------

        /// <summary>A flat earth roof over a rectangle: slab, parapet strips with their coping, beam ends front and back, spouts.</summary>
        private static void FlatRoofOver(HouseMeshBuilder b, float x0, float x1, float z0, float z1, float top, HouseHullKit kit, bool beamsFront, bool beamsBack)
        {
            const float slab = 0.2f;
            const float wall = 0.16f;
            float p = kit.ParapetHeight;
            b.BoxMinMax(new Vector3(x0, top - 0.02f, z0), new Vector3(x1, top + slab, z1), kit.WallMain, kit.RoofTop);
            float y0 = top + slab, y1 = top + slab + p;
            b.BoxMinMax(new Vector3(x0, y0, z0), new Vector3(x1, y1, z0 + wall), kit.WallMain);
            b.BoxMinMax(new Vector3(x0, y0, z1 - wall), new Vector3(x1, y1, z1), kit.WallMain);
            b.BoxMinMax(new Vector3(x0, y0, z0 + wall), new Vector3(x0 + wall, y1, z1 - wall), kit.WallMain);
            b.BoxMinMax(new Vector3(x1 - wall, y0, z0 + wall), new Vector3(x1, y1, z1 - wall), kit.WallMain);
            if (kit.Level >= 2)
            {
                b.BoxMinMax(new Vector3(x0 - 0.03f, y1, z0 - 0.03f), new Vector3(x1 + 0.03f, y1 + 0.05f, z0 + wall + 0.03f), kit.Coping);
                b.BoxMinMax(new Vector3(x0 - 0.03f, y1, z1 - wall - 0.03f), new Vector3(x1 + 0.03f, y1 + 0.05f, z1 + 0.03f), kit.Coping);
                b.BoxMinMax(new Vector3(x0 - 0.03f, y1, z0 + wall), new Vector3(x0 + wall + 0.03f, y1 + 0.05f, z1 - wall), kit.Coping);
                b.BoxMinMax(new Vector3(x1 - wall - 0.03f, y1, z0 + wall), new Vector3(x1 + 0.03f, y1 + 0.05f, z1 - wall), kit.Coping);
            }

            for (float x = x0 + 0.3f; x < x1 - 0.15f; x += 0.6f)
            {
                if (beamsFront) b.BoxMinMax(new Vector3(x - 0.05f, top + 0.02f, z0 - 0.18f), new Vector3(x + 0.05f, top + 0.12f, z0), kit.Beam);
                if (beamsBack) b.BoxMinMax(new Vector3(x - 0.05f, top + 0.02f, z1), new Vector3(x + 0.05f, top + 0.12f, z1 + 0.18f), kit.Beam);
            }
        }

        private static void BuildFlatRoof(HouseMeshBuilder b, HouseLevelSpec spec, HouseHullKit kit)
        {
            float w = InteriorWidth(spec);
            float d = InteriorDepth(spec);
            float t = kit.WallThickness;
            float top = FloorY + WallHeight;
            FlatRoofOver(b, -t - 0.06f, w + t + 0.06f, -t - 0.06f, d + t + 0.06f, top, kit, beamsFront: false, beamsBack: true);

            // Wooden spouts through the parapet at the back.
            foreach (float x in new[] { w * 0.25f, w * 0.75f })
            {
                b.BoxMinMax(new Vector3(x - 0.07f, top + 0.18f, d + t), new Vector3(x + 0.07f, top + 0.26f, d + t + 0.45f), HouseSwatch.WoodDark);
            }

            if (kit.Level <= 1)
            {
                // Reed bundles and a tuft of grass up on the earth roof.
                b.Log(new Vector3(w * 0.2f, top + 0.3f, d * 0.55f), new Vector3(w * 0.55f, top + 0.3f, d * 0.7f), 0.1f, HouseSwatch.Reed, HouseSwatch.Straw);
                b.Log(new Vector3(w * 0.25f, top + 0.3f, d * 0.4f), new Vector3(w * 0.6f, top + 0.3f, d * 0.52f), 0.09f, HouseSwatch.Straw, HouseSwatch.Reed);
                b.Blob(new Vector3(w * 0.8f, top + 0.24f, d * 0.3f), new Vector3(0.4f, 0.16f, 0.35f), HouseSwatch.GrassDark);
            }
        }

        /// <summary>The ayvan / iwan: columns along the front (and the west side at N5), a beam, joists and a flat roof in the wall's material.</summary>
        private static void BuildPorchRoof(HouseMeshBuilder b, HouseLevelSpec spec, HouseHullKit kit)
        {
            float w = InteriorWidth(spec);
            float d = InteriorDepth(spec);
            float t = kit.WallThickness;
            float top = FloorY + WallHeight;
            float depth = kit.PorchDepth;
            float beamY = top - 0.2f;
            float frontZ = -depth + 0.18f;
            int pillars = Mathf.Max(3, Mathf.RoundToInt(w / 2.5f) + 1);
            HouseOpening door = spec.EntranceDoor;
            float doorU0 = door != null ? door.Start * HouseLevelSpec.InteriorCellSize : 1f;
            float doorU1 = door != null ? (door.Start + door.Width) * HouseLevelSpec.InteriorCellSize : 2f;
            float west = kit.Level >= 5 ? depth : 0f;

            var posts = new List<Vector3>();
            for (int i = 0; i < pillars; i++) posts.Add(new Vector3(Mathf.Lerp(0.18f, w - 0.18f, i / (float)(pillars - 1)), FloorY - 0.04f, frontZ));
            if (kit.Level >= 5)
            {
                // Iwan em L: the west leg runs along the side of the house.
                int side = Mathf.Max(2, Mathf.RoundToInt(d / 2.5f));
                for (int i = 1; i <= side; i++) posts.Add(new Vector3(-depth + 0.18f, FloorY - 0.04f, Mathf.Lerp(frontZ, d - 0.18f, i / (float)side)));
                posts.Add(new Vector3(-depth + 0.18f, FloorY - 0.04f, frontZ));
            }

            foreach (Vector3 p in posts) Pillar(b, p, beamY - p.y, kit);

            // Beams.
            b.BoxMinMax(new Vector3(-0.1f - west, beamY, frontZ - 0.1f), new Vector3(w + 0.1f, beamY + 0.18f, frontZ + 0.1f), kit.Beam);
            if (kit.Level >= 5) b.BoxMinMax(new Vector3(-depth + 0.08f, beamY, frontZ), new Vector3(-depth + 0.28f, beamY + 0.18f, d + 0.1f), kit.Beam);

            // Pointed arches between the masonry piers (N4: the arcaded iwan, the region's signature form).
            if (kit.Level == 4)
            {
                for (int i = 0; i + 1 < pillars; i++)
                {
                    ArchSpan(b, posts[i].x + 0.16f, posts[i + 1].x - 0.16f, frontZ, beamY - 0.75f, beamY, kit.Pillar);
                }

                b.BoxMinMax(new Vector3(-0.1f, beamY - 0.06f, frontZ - 0.12f), new Vector3(w + 0.1f, beamY, frontZ - 0.1f), HouseSwatch.Turquoise);
            }

            // Joists from the wall to the front beam (a painted ceiling from N3).
            if (kit.Level >= 2)
            {
                for (float x = 0.3f; x < w - 0.1f; x += 0.5f)
                {
                    b.BoxMinMax(new Vector3(x - 0.04f, beamY + 0.04f, frontZ), new Vector3(x + 0.04f, beamY + 0.16f, -t), kit.Level >= 3 ? HouseSwatch.Walnut : HouseSwatch.WoodDark);
                }
            }

            // Low rail (N2–N3 wood, N4+ plaster), open in front of the door.
            if (kit.Level >= 2)
            {
                float railTop = FloorY + 0.75f;
                HouseSwatch rail = kit.Level <= 3 ? kit.Pillar : HouseSwatch.Plaster;
                foreach ((float x0, float x1) in new[] { (0.1f, doorU0 - 0.4f), (doorU1 + 0.4f, w - 0.1f) })
                {
                    if (x1 - x0 < 0.3f) continue;
                    b.BoxMinMax(new Vector3(x0, railTop - 0.06f, frontZ - 0.04f), new Vector3(x1, railTop, frontZ + 0.04f), rail);
                    int slats = Mathf.FloorToInt((x1 - x0) / 0.25f);
                    for (int s = 0; s <= slats; s++)
                    {
                        float x = x0 + s * 0.25f;
                        b.BoxMinMax(new Vector3(x - 0.025f, FloorY - 0.03f, frontZ - 0.025f), new Vector3(x + 0.025f, railTop - 0.06f, frontZ + 0.025f), rail);
                    }
                }
            }

            if (kit.Level <= 1)
            {
                // The N1 ayvan: poplar poles and a reed mat, a little lower than the earth roof.
                float matY = beamY + 0.18f;
                for (float x = 0.25f; x < w - 0.1f; x += 0.45f)
                {
                    b.Log(new Vector3(x, matY + 0.03f, frontZ - 0.25f), new Vector3(x, matY + 0.03f, -t), 0.035f, kit.Beam);
                }

                b.BoxMinMax(new Vector3(-0.2f, matY + 0.06f, frontZ - 0.3f), new Vector3(w + 0.2f, matY + 0.11f, -t), HouseSwatch.Reed);
                return;
            }

            // Flat roof over the iwan, flush with the house's, its parapet only along the open sides.
            float zFront = -depth - 0.1f;
            float x0Roof = -t - 0.06f - west;
            b.BoxMinMax(new Vector3(x0Roof, top - 0.02f, zFront), new Vector3(w + t + 0.06f, top + 0.2f, -t - 0.06f), kit.WallMain, kit.RoofTop);
            float y0 = top + 0.2f, y1 = y0 + kit.ParapetHeight;
            b.BoxMinMax(new Vector3(x0Roof, y0, zFront), new Vector3(w + t + 0.06f, y1, zFront + 0.16f), kit.WallMain);
            b.BoxMinMax(new Vector3(x0Roof - 0.03f, y1, zFront - 0.03f), new Vector3(w + t + 0.09f, y1 + 0.05f, zFront + 0.19f), kit.Coping);
            for (float x = x0Roof + 0.3f; x < w + t - 0.1f; x += 0.6f)
            {
                b.BoxMinMax(new Vector3(x - 0.05f, top + 0.02f, zFront - 0.18f), new Vector3(x + 0.05f, top + 0.12f, zFront), kit.Beam);
            }

            if (kit.Level >= 5)
            {
                // The west leg of the L.
                b.BoxMinMax(new Vector3(x0Roof, top - 0.02f, -t - 0.06f), new Vector3(-t - 0.06f, top + 0.2f, d + t + 0.06f), kit.WallMain, kit.RoofTop);
                b.BoxMinMax(new Vector3(x0Roof, y0, zFront), new Vector3(x0Roof + 0.16f, y1, d + t + 0.06f), kit.WallMain);
                b.BoxMinMax(new Vector3(x0Roof - 0.03f, y1, zFront), new Vector3(x0Roof + 0.19f, y1 + 0.05f, d + t + 0.09f), kit.Coping);
            }
        }

        /// <summary>The spandrel of a pointed arch between two piers: filled from the arch curve up to the beam, both faces.</summary>
        private static void ArchSpan(HouseMeshBuilder b, float x0, float x1, float z, float spring, float topY, HouseSwatch swatch)
        {
            const int steps = 3;
            float mid = (x0 + x1) * 0.5f;
            float rise = Mathf.Min(topY - spring - 0.06f, (x1 - x0) * 0.6f);
            for (int side = 0; side < 2; side++)
            {
                float from = side == 0 ? x0 : x1;
                for (int i = 0; i < steps; i++)
                {
                    float a0 = i / (float)steps, a1 = (i + 1) / (float)steps;
                    // A quarter-circle-ish rise: steep at the spring, flat near the apex.
                    float xa = Mathf.Lerp(from, mid, a0), xb = Mathf.Lerp(from, mid, a1);
                    float ya = spring + rise * Mathf.Sin(a0 * Mathf.PI * 0.5f), yb = spring + rise * Mathf.Sin(a1 * Mathf.PI * 0.5f);
                    foreach (float face in new[] { z - 0.1f, z + 0.1f })
                    {
                        var p0 = new Vector3(xa, ya, face);
                        var p1 = new Vector3(xb, yb, face);
                        var p2 = new Vector3(xb, topY, face);
                        var p3 = new Vector3(xa, topY, face);
                        b.TwoSidedQuad(p0, p3, p2, p1, swatch);
                    }
                }
            }

            b.BoxMinMax(new Vector3(x0, spring - 0.04f, z - 0.11f), new Vector3(x0 + 0.06f, spring + 0.02f, z + 0.11f), HouseSwatch.Walnut);
            b.BoxMinMax(new Vector3(x1 - 0.06f, spring - 0.04f, z - 0.11f), new Vector3(x1, spring + 0.02f, z + 0.11f), HouseSwatch.Walnut);
        }

        private static void Pillar(HouseMeshBuilder b, Vector3 foot, float height, HouseHullKit kit)
        {
            switch (kit.Level)
            {
                case 1:
                    // A plain poplar pole on a flat stone.
                    b.Cylinder(foot, 0.12f, 0.12f, 0.06f, 6, HouseSwatch.Stone, HouseSwatch.Stone);
                    b.Cylinder(foot, 0.065f, 0.055f, height + 0.18f, 6, kit.Pillar);
                    return;
                case 2:
                    // Thin walnut column on a stone base with a plain bracket.
                    b.Cylinder(foot, 0.14f, 0.12f, 0.15f, 8, HouseSwatch.Stone, HouseSwatch.Stone);
                    b.Cylinder(foot + Vector3.up * 0.15f, 0.07f, 0.06f, height - 0.27f, 6, kit.Pillar);
                    b.BoxMinMax(foot + new Vector3(-0.25f, height - 0.12f, -0.09f), foot + new Vector3(0.25f, height, 0.09f), HouseSwatch.WoodDark);
                    return;
                case 4:
                    // Plastered masonry pier with a turquoise band.
                    b.BoxMinMax(foot + new Vector3(-0.16f, 0f, -0.16f), foot + new Vector3(0.16f, height, 0.16f), kit.Pillar);
                    b.BoxMinMax(foot + new Vector3(-0.17f, 0f, -0.17f), foot + new Vector3(0.17f, 0.45f, 0.17f), HouseSwatch.Ochre);
                    b.BoxMinMax(foot + new Vector3(-0.18f, height - 0.85f, -0.18f), foot + new Vector3(0.18f, height - 0.75f, 0.18f), HouseSwatch.Turquoise);
                    return;
                default:
                    // Sogdian carved column (N3, N5): stone base, slender walnut shaft with rings, a fan capital and brackets.
                    b.Cylinder(foot, 0.17f, 0.15f, 0.18f, 8, HouseSwatch.Stone, HouseSwatch.Stone);
                    float shaftTop = height - 0.42f;
                    b.Cylinder(foot + Vector3.up * 0.18f, 0.1f, 0.075f, shaftTop - 0.18f, 8, kit.Pillar);
                    foreach (float f in new[] { 0.3f, 0.62f })
                    {
                        b.Cylinder(foot + Vector3.up * Mathf.Lerp(0.18f, shaftTop, f), 0.11f, 0.11f, 0.07f, 8, HouseSwatch.WoodDark, HouseSwatch.WoodDark);
                    }

                    b.Cylinder(foot + Vector3.up * shaftTop, 0.08f, 0.2f, 0.22f, 4, kit.Pillar, kit.Pillar);
                    b.Box(foot + Vector3.up * (shaftTop + 0.28f), new Vector3(0.6f, 0.1f, 0.24f), HouseSwatch.WoodDark);
                    b.Box(foot + Vector3.up * (shaftTop + 0.37f), new Vector3(0.86f, 0.1f, 0.28f), kit.Pillar);
                    return;
            }
        }

        /// <summary>N3 tandir flue; N4–N5 badgir windcatchers (square towers with vent slots) on the roof.</summary>
        private static void BuildWindcatchers(HouseMeshBuilder b, HouseLevelSpec spec, HouseHullKit kit)
        {
            if (kit.Windcatchers <= 0) return;
            float w = InteriorWidth(spec);
            float d = InteriorDepth(spec);
            float roof = FloorY + WallHeight + 0.2f;
            for (int i = 0; i < kit.Windcatchers; i++)
            {
                float x = i == 0 ? w * 0.8f : w * 0.2f;
                float z = d * 0.72f;
                if (kit.Level == 3)
                {
                    // The kitchen tandir's clay flue.
                    b.Cylinder(new Vector3(x, roof - 0.05f, z), 0.26f, 0.2f, 0.9f, 8, HouseSwatch.Clay);
                    b.Cylinder(new Vector3(x, roof + 0.85f, z), 0.25f, 0.25f, 0.08f, 8, HouseSwatch.Taipa, HouseSwatch.Black);
                    continue;
                }

                const float half = 0.42f;
                float h = 1.7f;
                b.BoxMinMax(new Vector3(x - half, roof - 0.05f, z - half), new Vector3(x + half, roof + h, z + half), kit.WallMain);
                // Vent slots on all four faces near the top.
                for (int s = -1; s <= 1; s++)
                {
                    float o = s * 0.24f;
                    b.BoxMinMax(new Vector3(x + o - 0.06f, roof + h - 0.7f, z - half - 0.01f), new Vector3(x + o + 0.06f, roof + h - 0.15f, z - half), HouseSwatch.Black);
                    b.BoxMinMax(new Vector3(x + o - 0.06f, roof + h - 0.7f, z + half), new Vector3(x + o + 0.06f, roof + h - 0.15f, z + half + 0.01f), HouseSwatch.Black);
                    b.BoxMinMax(new Vector3(x - half - 0.01f, roof + h - 0.7f, z + o - 0.06f), new Vector3(x - half, roof + h - 0.15f, z + o + 0.06f), HouseSwatch.Black);
                    b.BoxMinMax(new Vector3(x + half, roof + h - 0.7f, z + o - 0.06f), new Vector3(x + half + 0.01f, roof + h - 0.15f, z + o + 0.06f), HouseSwatch.Black);
                }

                b.BoxMinMax(new Vector3(x - half - 0.06f, roof + h, z - half - 0.06f), new Vector3(x + half + 0.06f, roof + h + 0.08f, z + half + 0.06f), kit.Coping);
                b.BoxMinMax(new Vector3(x - half - 0.02f, roof + h - 0.85f, z - half - 0.02f), new Vector3(x + half + 0.02f, roof + h - 0.78f, z + half + 0.02f), HouseSwatch.Turquoise);
            }
        }

        /// <summary>The courtyard house's balakhana: a small open-fronted room on the roof (carved posts, its own flat roof) and a stork's nest.</summary>
        private static void BuildBalakhana(HouseMeshBuilder b, HouseLevelSpec spec, HouseHullKit kit)
        {
            float w = InteriorWidth(spec);
            float d = InteriorDepth(spec);
            float roof = FloorY + WallHeight + 0.2f;
            float x0 = w * 0.36f, x1 = w * 0.64f, z0 = d * 0.42f, z1 = d * 0.82f;
            const float h = 2.0f;
            const float t = 0.16f;
            b.BoxMinMax(new Vector3(x0, roof, z1 - t), new Vector3(x1, roof + h, z1), kit.WallMain);
            b.BoxMinMax(new Vector3(x0, roof, z0), new Vector3(x0 + t, roof + h, z1 - t), kit.WallMain);
            b.BoxMinMax(new Vector3(x1 - t, roof, z0), new Vector3(x1, roof + h, z1 - t), kit.WallMain);
            b.BoxMinMax(new Vector3(x0 + t, roof, z0), new Vector3(x1 - t, roof + 0.05f, z1 - t), HouseSwatch.ClothRed);
            for (int i = 1; i < 4; i++)
            {
                var foot = new Vector3(Mathf.Lerp(x0, x1, i / 4f), roof, z0 + 0.12f);
                b.Cylinder(foot, 0.1f, 0.09f, 0.12f, 8, HouseSwatch.Stone, HouseSwatch.Stone);
                b.Cylinder(foot + Vector3.up * 0.12f, 0.06f, 0.05f, h - 0.32f, 6, kit.Pillar);
                b.Cylinder(foot + Vector3.up * (h - 0.2f), 0.05f, 0.14f, 0.2f, 4, kit.Pillar, kit.Pillar);
            }

            var mini = new HouseHullKit
            {
                Level = kit.Level, WallMain = kit.WallMain, RoofTop = kit.RoofTop, Coping = kit.Coping, Beam = kit.Beam, ParapetHeight = 0.25f,
            };
            FlatRoofOver(b, x0 - 0.1f, x1 + 0.1f, z0 - 0.1f, z1 + 0.1f, roof + h, mini, beamsFront: true, beamsBack: false);

            // A stork's nest on the parapet corner (bible §2.1: "cegonha no telhado").
            float t2 = kit.WallThickness;
            var nest = new Vector3(w + t2 - 0.1f, roof + kit.ParapetHeight + 0.05f, d + t2 - 0.1f);
            b.Cylinder(nest, 0.38f, 0.45f, 0.2f, 8, HouseSwatch.WoodRaw, HouseSwatch.Dirt);
            b.Blob(nest + new Vector3(0f, 0.42f, 0f), new Vector3(0.22f, 0.22f, 0.4f), HouseSwatch.White);
            b.Blob(nest + new Vector3(0f, 0.44f, 0.12f), new Vector3(0.24f, 0.14f, 0.24f), HouseSwatch.Black);
            b.Log(nest + new Vector3(0f, 0.48f, -0.14f), nest + new Vector3(0f, 0.78f, -0.22f), 0.035f, HouseSwatch.White);
            b.Log(nest + new Vector3(0f, 0.8f, -0.24f), nest + new Vector3(0f, 0.74f, -0.44f), 0.02f, HouseSwatch.ClothRed);
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
            // South face with two latticed windows, east face with one, north face plain; the west face is the main wall.
            WingFace(b, new Vector3(x0, FloorY, z0), Vector3.right, Vector3.back, WingWidth, new[] { 1.2f, 3.4f }, kit);
            WingFace(b, new Vector3(x1, FloorY, z0), Vector3.forward, Vector3.right, WingDepth, new[] { 3f }, kit);
            WingFace(b, new Vector3(x0, FloorY, z1), Vector3.right, Vector3.forward, WingWidth, new float[0], kit);
            // Brick corner piers at the two free corners.
            foreach (Vector3 corner in new[] { new Vector3(x1, 0f, z0), new Vector3(x1, 0f, z1) })
            {
                for (int k = 0; k < 5; k++)
                {
                    float y = FloorY + 0.05f + k * 0.5f;
                    b.BoxMinMax(new Vector3(corner.x - 0.2f, y, corner.z - 0.2f), new Vector3(corner.x + 0.05f, Mathf.Min(y + 0.42f, h - 0.15f), corner.z + 0.2f), HouseSwatch.Ochre);
                }
            }

            b.BoxMinMax(new Vector3(x0, h - 0.14f, z0 - t - 0.03f), new Vector3(x1 + t, h - 0.04f, z0 - t), kit.Plate);
        }

        /// <summary>One solid wing face from <paramref name="origin"/> along <paramref name="along"/>, with 1 m latticed windows at the given offsets.</summary>
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
                        FaceBox(b, origin, along, outward, u - 0.015f, u + 0.015f, 1f, 2.1f, t + 0.02f, t + 0.045f, kit.Lattice);
                    }

                    FaceBox(b, origin, along, outward, u0 - 0.08f, u1 + 0.08f, 0.93f, 1f, -0.02f, t + 0.1f, HouseSwatch.Stone);
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
            float t = kit.WallThickness;
            FlatRoofOver(b, x0 - 0.04f, x1 + t + 0.06f, z0 - t - 0.06f, z1 + 0.06f, FloorY + WallHeight, kit, beamsFront: true, beamsBack: true);
        }
    }
}
