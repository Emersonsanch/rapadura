using Rapadura.Gameplay.World.Cities.Town;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Overworld
{
    /// <summary>The overworld's own dressing pieces (beside the town kit's trees, shrubs and rocks).</summary>
    public enum OverworldDecor
    {
        None = 0,

        // Ground detail, no colliders.
        /// <summary>A piece of crop row (<see cref="OverworldProp.End"/> = its other end; <see cref="OverworldProp.Variant"/> = <see cref="FieldCrop"/>).</summary>
        CropRow = 1,
        Tufts = 2,
        Flowers = 3,
        Reeds = 4,
        Pebbles = 5,
        Ferns = 6,
        Puddle = 7,

        // Solid pieces.
        /// <summary>A piece of hedgerow along a field lane (<see cref="OverworldProp.End"/> = its other end).</summary>
        Hedge = 20,
        Log = 21,
        Stump = 22,
        MossRock = 23,
        /// <summary>The Altos do Mensageiro: stacked-stone cairns.</summary>
        Cairn = 24,
        /// <summary>The Ravina: clusters of basalt columns.</summary>
        Basalt = 25,
        /// <summary>The Ruínas: broken columns and fallen drums.</summary>
        Column = 26,
        /// <summary>The Jardim: tall cypresses.</summary>
        Cypress = 27,
        /// <summary>The Brejo: dead trees.</summary>
        DeadTree = 28,

        // Região 2 — Areias Flamejantes (OverworldDecorKit.Desert.cs): 80–99.
        /// <summary>Bleached bones: a camel's rib cage and skull half in the sand.</summary>
        Bones = 80,
        /// <summary>A broken caravan cart with spilled crates and bales of silk.</summary>
        CaravanWreck = 81,
        /// <summary>A steep Meroë pyramid with its chapel (the town kit's ruin).</summary>
        Pyramid = 82,
        /// <summary>A broken lotus column and its fallen drum.</summary>
        LotusColumn = 83,
        /// <summary>A row of ushabtis on a plinth.</summary>
        Ushabtis = 84,
        /// <summary>A karez well shaft: spoil ring, timber frame, windlass.</summary>
        KarezShaft = 85,
        /// <summary>A stone chimney (hoodoo) of layered red sandstone with its cap rock.</summary>
        Hoodoo = 86,
        /// <summary>A natural sandstone arch.</summary>
        RockArch = 87,
        /// <summary>A cluster of red salt crystals (Namrael's).</summary>
        SaltCrystal = 88,
        /// <summary>A rammed-earth watchtower of Qocho's sentinels (half ruined).</summary>
        TaipaTower = 89,
        /// <summary>A flat polygon of salt crust (ground detail, no collider).</summary>
        SaltCrust = 90,
        /// <summary>A stone marker (stela) along the caravan ways.</summary>
        Marker = 91,
        /// <summary>A resting caravan: camels, bales, jars round a cold hearth.</summary>
        CaravanCamp = 92,
        /// <summary>A clan camp of the crowned orcs: ochre leather tents and a drum (11).</summary>
        ClanTent = 93,

        // Região 4 — Grutas do Limiar (OverworldDecorKit.Caves.cs): 60–79, all solid.
        /// <summary>A run of tall tuff wall (<see cref="OverworldProp.End"/> = its other end): the cave's walls between halls.</summary>
        TuffWall = 60,
        Stalagmite = 61,
        /// <summary>A cluster of glowing crystals.</summary>
        Crystal = 62,
        /// <summary>A natural stone column, floor to "ceiling" (cut off high above the camera).</summary>
        CaveColumn = 63,
        /// <summary>A giant glowing mushroom cluster.</summary>
        GiantMushroom = 64,
        /// <summary>A vishapakar, the dragon stone with a fish head (21).</summary>
        Vishapakar = 65,
        /// <summary>A mine cart on a piece of rail (22).</summary>
        MineCart = 66,
        /// <summary>A copper air pipe on posts (22).</summary>
        AirPipe = 67,
        /// <summary>A bronze jar of the wheel's aqueducts (23).</summary>
        Jar = 68,
        /// <summary>Mahaway's flock: a nest of sticks and bones (24).</summary>
        Nest = 69,
        /// <summary>The posts and ropes at the end of a rope bridge.</summary>
        RopePosts = 70,
        /// <summary>A lamp niche: a tuff post with a lit oil lamp (the Lamparinas' lamps along the trails).</summary>
        LampNiche = 71,
        /// <summary>The wheel of twelve jars (23's centrepiece).</summary>
        JarWheel = 72,
        /// <summary>The barred portal of the Ossuário dos Artífices.</summary>
        OssuaryPortal = 73,
        /// <summary>The black void over a chasm's bed (<see cref="OverworldProp.End"/> = its other end; ground detail, no collider: the bed blocks).</summary>
        Abyss = 74,

        // Região 3 — Serra de Kaggen (OverworldDecorKit.Mountain.cs): 100–119.
        /// <summary>A herder's rondavel: round stone-and-clay wall with a litema band, conical thatch.</summary>
        Rondavel = 100,
        /// <summary>A round dry-stone kraal (curral) with its gap.</summary>
        Kraal = 101,
        /// <summary>A run of dry-stone wall (<see cref="OverworldProp.End"/> = its other end; <see cref="OverworldProp.Scale"/> = height).</summary>
        DryStoneWall = 102,
        /// <summary>A terrace step: low retaining wall with a sorghum row on top (<see cref="OverworldProp.End"/> = its other end).</summary>
        TerraceRow = 103,
        /// <summary>A sandstone overhang with a painted panel (eland, dancers' silhouettes only as shapes).</summary>
        PaintedShelter = 104,
        /// <summary>A tall slab of the rock walls of a narrow pass.</summary>
        RockSpire = 105,
        /// <summary>A Great-Zimbabwe-like conical stone tower.</summary>
        ConicalTower = 106,
        /// <summary>A granite pillar with a bronze cap and runes (Napirasu).</summary>
        BronzePillar = 107,
        /// <summary>A snow drift on the ground (no collider).</summary>
        SnowDrift = 108,
        /// <summary>A low bank of mist (no collider).</summary>
        MistBank = 109,
        /// <summary>A giant granite block of the Mazans.</summary>
        GraniteBlock = 110,
        /// <summary>A tall stone stela (Estela Alta, the summit).</summary>
        Stela = 111,

        // Região 5 — Costa das Monções (OverworldDecorKit.Coast.cs): 120–139.
        /// <summary>A dhow beached on its side on the sand (the town kit's dhow, keeled over).</summary>
        BeachedDhow = 120,
        /// <summary>A wema fish trap: a heart of stakes and palm-rib fence on the beach.</summary>
        FishTrap = 121,
        /// <summary>Fishing nets hung to dry between two poles.</summary>
        NetRack = 122,
        /// <summary>A coral head (live or dimmed coral boulders) on the reef flat.</summary>
        CoralHead = 123,
        /// <summary>A run of ruined coral-stone wall (<see cref="OverworldProp.End"/> = its other end).</summary>
        CoralWall = 124,
        /// <summary>A ruined coral-stone arch (Kilwa, Gedi).</summary>
        CoralArch = 125,
        /// <summary>A flat-topped acacia of the savanna.</summary>
        Acacia = 126,
        /// <summary>A tall termite mound of red earth.</summary>
        TermiteMound = 127,
        /// <summary>A mangrove on its stilt roots (the town kit's mangrove).</summary>
        MangroveClump = 128,
        /// <summary>A line of wrack, seaweed and shells on the sand (ground detail, no collider).</summary>
        Wrack = 129,
        /// <summary>Cloves drying on a mat (ground detail, no collider).</summary>
        CloveMat = 130,
        /// <summary>A striped post marking the edge of a shallow ford (baixio).</summary>
        FordPost = 131,
        /// <summary>A Swahili pillar tomb of coral stone.</summary>
        PillarTomb = 132,

        // Região 6 — Delta de Jhilpur (OverworldDecorKit.Delta.cs): 140–159.
        /// <summary>A banyan fig: a cluster of trunks, curtains of aerial roots dropping to the ground, a wide flat crown.</summary>
        Banyan = 140,
        /// <summary>An arch of living roots over the ground (the root bridges' little sisters, in the gorges).</summary>
        RootArch = 141,
        /// <summary>Lotus pads and a pink flower on the water's edge (ground detail, no collider).</summary>
        LotusPads = 142,
        /// <summary>A fisher's reed hut raised on stilts, a ladder and a net (Tarchon's swamp).</summary>
        StiltHut = 143,
        /// <summary>A lotus lantern hung from a bamboo pole (the Ilhota das Lanternas; glows at night).</summary>
        LanternPole = 144,
        /// <summary>A spring of Living Water in a ring of roots (the Coração do Lodo's fountains; glows).</summary>
        LivingSpring = 145,
        /// <summary>Violet bubbles of the Lodo on black mud (ground detail, no collider).</summary>
        MireBubbles = 146,
        /// <summary>A run of earthen paddy dike (<see cref="OverworldProp.End"/> = its other end).</summary>
        PaddyBund = 147,
        /// <summary>A clump of rice in the flooded paddy (ground detail, no collider).</summary>
        RiceTuft = 148,
        /// <summary>A twisted mass of rotten root, black and grey, bracket fungi.</summary>
        RottenRoot = 149,

        // Região 7 — Teto do Mundo (OverworldDecorKit.Pamir.cs): 160–179.
        /// <summary>An apricot tree in flower (pale pink crown on a crooked trunk): Sangkent's orchards.</summary>
        ApricotBlossom = 160,
        /// <summary>A run of polished stone chute on low pillars, ice in the groove (<see cref="OverworldProp.End"/> = its other end).</summary>
        StoneChute = 161,
        /// <summary>A timber chute station: a roofed platform, a cart on its rails, a lantern.</summary>
        ChuteStation = 162,
        /// <summary>A herder's stone hut: low dry-stone walls, a flat roof of earth and beams, a smoke hole.</summary>
        StoneHut = 163,
        /// <summary>A serac: a leaning tower of blue ice, snow on its top.</summary>
        Serac = 164,
        /// <summary>A blue crevasse line on the ice (ground detail, no collider).</summary>
        Crevasse = 165,
        /// <summary>A wooden wind wheel on a stone tower (the Forte dos Ventos' cata-ventos).</summary>
        WindWheel = 166,
        /// <summary>A line of coloured pennants between two poles (the caravans' ribbons; <see cref="OverworldProp.End"/> = the other pole).</summary>
        RibbonLine = 167,
        /// <summary>A square watch tower of stone with a timber gallery and wind bells.</summary>
        WatchTower = 168,
        /// <summary>A great shed scale of Aydahar stuck slanting in the ground, dark and green-gold.</summary>
        DragonScale = 169,
        /// <summary>A marmot's burrow: a mound of earth with its hole (ground detail, no collider).</summary>
        MarmotMound = 170,
        /// <summary>Ice shards and frost on the ground (ground detail, no collider).</summary>
        IceShards = 171,
        /// <summary>An old twisted juniper (archa): a thick crooked trunk, dark scale-leaf clumps.</summary>
        Juniper = 172,

        // Região 8 — Planície de Mingcheng (OverworldDecorKit.Mingcheng.cs): 180–199.
        /// <summary>A weeping willow: a leaning trunk, long curtains of hanging green.</summary>
        Willow = 180,
        /// <summary>A post with a red paper lantern (the Festa das Lanternas).</summary>
        RedLantern = 181,
        /// <summary>A small pavilion: red lacquered columns under a curved grey-tile roof with upturned eaves.</summary>
        Pavilion = 182,
        /// <summary>A stone guardian lion on its plinth (the gates' statues).</summary>
        StoneLion = 183,
        /// <summary>A stone turtle carrying a stele on its back.</summary>
        StoneTurtle = 184,
        /// <summary>A pit of terracotta soldiers in rows, its earthen rim.</summary>
        TerracottaPit = 185,
        /// <summary>A run of the great wall (grey brick, crenellated, a walkway on top; <see cref="OverworldProp.End"/> = its other end).</summary>
        GreatWallRun = 186,
        /// <summary>A beacon tower of the wall: square grey brick, crenellations, the beacon basin on top.</summary>
        BeaconTower = 187,
        /// <summary>A clump of bamboo culms with their leaf sprays.</summary>
        Bamboo = 188,
        /// <summary>A karst tower of grey limestone with pines on its ledges.</summary>
        KarstPillar = 189,
        /// <summary>An old mountain pine: a bent trunk, flat layered crowns.</summary>
        MountainPine = 190,
        /// <summary>A bronze mirror on a white post (the Torres Brancas pass the light down with them).</summary>
        MirrorPost = 191,
        /// <summary>A small white spire on a stepped base, a gold cap.</summary>
        WhiteSpire = 192,
        /// <summary>A wooden water wheel in its race (the Moinho d'Água of the paddies).</summary>
        WaterWheel = 193,
        /// <summary>A straw scarecrow in a conical hat.</summary>
        Scarecrow = 194,
        /// <summary>Golden millet heads (ground detail, no collider).</summary>
        MilletTuft = 195,
        /// <summary>A drift of fallen bamboo leaves and young shoots (ground detail, no collider).</summary>
        BambooShoots = 196,

        // Região 9 — Estepe dos Nartes (OverworldDecorKit.Nartes.cs): 200–219.
        /// <summary>A round felt tent on a cart (the centaurs' camp that moves): felt walls, a domed roof, two big wheels.</summary>
        FeltYurt = 200,
        /// <summary>A horse standard (tug): a tall pole with horsetail tufts and a small gold finial.</summary>
        HorseStandard = 201,
        /// <summary>A stone baba (balbal): a weathered stone figure holding a cup at its belly, on the steppe or a kurgan.</summary>
        StoneBaba = 202,
        /// <summary>A golden stag in the Scythian animal style on a stone base (the kurgans' gold).</summary>
        GoldStag = 203,
        /// <summary>A birch: thin white trunks with dark marks, a light crown of small leaves.</summary>
        Birch = 204,
        /// <summary>A giant larch of the taiga: a tall straight trunk, soft green-gold tiers.</summary>
        Larch = 205,
        /// <summary>A dream tablet of the hill giants: a tall standing slab carved with lines, moss at its foot.</summary>
        DreamTablet = 206,
        /// <summary>A giant stone pot of the hill giants, as big as a hut, smoke rising from it.</summary>
        GiantCauldron = 207,
        /// <summary>A svan tower: a tall square stone watch tower narrowing upwards, a pyramid roof of slate.</summary>
        SvanTower = 208,
        /// <summary>A run of Amirani's giant chain (old iron links lying up the mountain; <see cref="OverworldProp.End"/> = its other end).</summary>
        ChainRun = 209,
        /// <summary>A stone well with its bucket frame (the Poço da Taça of the prairie).</summary>
        StoneWell = 210,
        /// <summary>An earthen rampart of the giants' hill forts (<see cref="OverworldProp.End"/> = its other end).</summary>
        EarthRampart = 211,
        /// <summary>Feather-grass plumes (ground detail, no collider).</summary>
        FeatherTuft = 212,
        /// <summary>A clump of red poppies (ground detail, no collider).</summary>
        PoppyTuft = 213,
    }

    /// <summary>
    /// Low-poly meshes of the <see cref="OverworldDecor"/> pieces, on the shared town palette (one material, merged per
    /// chunk). Each piece is built around the origin (placed with <see cref="TownMeshBuilder.With(Vector3,float,System.Action)"/>)
    /// except the row pieces (crop rows, hedges), which take their two ground points so they follow the slope.
    /// Kept cheap: a grass tuft is 15 triangles, a flower clump 20, a crop-row piece 6–26.
    /// </summary>
    public static partial class OverworldDecorKit
    {
        public static bool IsSolid(OverworldDecor decor) => (int)decor >= (int)OverworldDecor.Hedge;

        // Colour slots of the recorded ground-detail pieces (painted per stamp).
        private const TownSwatch Slot0 = TownSwatch.Black;
        private const TownSwatch Slot1 = TownSwatch.Iron;

        /// <summary>The ground detail recorded once (unit scale) and stamped: hundreds per chunk at a few vertex copies each.</summary>
        public static readonly TownStamp TuftStamp = TownMeshBuilder.Record(b => Tufts(b, 1f, Slot0, Slot1), Slot0, Slot1);
        public static readonly TownStamp FlowerStamp = TownMeshBuilder.Record(b => Flowers(b, 1f, TownSwatch.GrassDark, Slot0, Slot1), Slot0, Slot1);
        public static readonly TownStamp ReedStamp = TownMeshBuilder.Record(b => Reeds(b, 1f));
        public static readonly TownStamp PebbleStamp = TownMeshBuilder.Record(b => Pebbles(b, 1f, Slot0), Slot0);
        public static readonly TownStamp FernStamp = TownMeshBuilder.Record(b => Ferns(b, 1f, Slot0), Slot0);
        public static readonly TownStamp[] PuddleStamps =
        {
            TownMeshBuilder.Record(b => Puddle(b, 1f, 0)), TownMeshBuilder.Record(b => Puddle(b, 1f, 1)),
            TownMeshBuilder.Record(b => Puddle(b, 1f, 2)), TownMeshBuilder.Record(b => Puddle(b, 1f, 3)),
        };

        /// <summary>A thin three-sided spike (a blade of grass, a reed).</summary>
        public static void Spike(TownMeshBuilder b, Vector3 basePoint, Vector3 tip, float radius, TownSwatch swatch)
        {
            Vector3 axis = tip - basePoint;
            Vector3 side = Vector3.Cross(axis, Vector3.up);
            if (side.sqrMagnitude < 1e-6f)
            {
                side = Vector3.right;
            }

            side = side.normalized * radius;
            Vector3 fwd = Vector3.Cross(side, axis).normalized * radius;
            Vector3 a = basePoint + side;
            Vector3 c = basePoint - side * 0.5f + fwd * 0.87f;
            Vector3 d = basePoint - side * 0.5f - fwd * 0.87f;
            b.TriangleFacing(a, c, tip, (a + c) * 0.5f - basePoint, swatch);
            b.TriangleFacing(c, d, tip, (c + d) * 0.5f - basePoint, swatch);
            b.TriangleFacing(d, a, tip, (d + a) * 0.5f - basePoint, swatch);
        }

        public static void Tufts(TownMeshBuilder b, float s, TownSwatch color, TownSwatch tip)
        {
            for (int i = 0; i < 5; i++)
            {
                float a = i * 2.39996f;
                float r = (0.08f + 0.05f * (i & 1)) * s;
                var at = new Vector3(Mathf.Cos(a) * r, 0f, Mathf.Sin(a) * r);
                float h = (0.32f + 0.08f * (i % 3)) * s;
                Spike(b, at, at * 2.6f + Vector3.up * h, 0.05f * s, (i & 1) == 0 ? color : tip);
            }
        }

        public static void Flowers(TownMeshBuilder b, float s, TownSwatch leaf, TownSwatch a, TownSwatch c)
        {
            for (int i = 0; i < 3; i++)
            {
                float ang = i * 2.1f;
                var at = new Vector3(Mathf.Cos(ang) * 0.12f, 0f, Mathf.Sin(ang) * 0.12f) * s;
                Spike(b, at, at * 2f + Vector3.up * 0.26f * s, 0.045f * s, leaf);
            }

            for (int i = 0; i < 5; i++)
            {
                float ang = i * 1.256f + 0.3f;
                float r = (0.12f + 0.12f * (i & 1)) * s;
                var head = new Vector3(Mathf.Cos(ang) * r, (0.24f + 0.06f * (i % 3)) * s, Mathf.Sin(ang) * r);
                float k = 0.11f * s;
                TownSwatch color = (i % 3) == 0 ? c : a;
                b.Triangle(head + new Vector3(-k, 0f, 0f), head + new Vector3(0f, 0.015f, k), head + new Vector3(k, 0f, 0f), color);
                b.Triangle(head + new Vector3(k, 0f, 0f), head + new Vector3(0f, 0.015f, -k), head + new Vector3(-k, 0f, 0f), color);
            }
        }

        public static void Reeds(TownMeshBuilder b, float s)
        {
            for (int i = 0; i < 7; i++)
            {
                float a = i * 2.39996f;
                float r = (0.1f + 0.06f * (i % 3)) * s;
                var at = new Vector3(Mathf.Cos(a) * r, 0f, Mathf.Sin(a) * r);
                float h = (0.9f + 0.18f * (i % 3)) * s;
                Vector3 tip = at * 1.6f + Vector3.up * h;
                Spike(b, at, tip, 0.04f * s, (i & 1) == 0 ? TownSwatch.Vine : TownSwatch.GrassDark);
                if (i % 3 == 0)
                {
                    Vector3 head = Vector3.Lerp(at, tip, 0.78f);
                    b.Cylinder(head, 0.045f * s, 0.045f * s, 0.16f * s, 4, TownSwatch.Trunk);
                }
            }
        }

        public static void Pebbles(TownMeshBuilder b, float s, TownSwatch color)
        {
            b.Blob(new Vector3(0f, 0.05f * s, 0f), new Vector3(0.34f, 0.16f, 0.26f) * s, color);
            b.Blob(new Vector3(0.28f, 0.035f * s, 0.12f) * s, new Vector3(0.2f, 0.1f, 0.18f) * s, TownSwatch.StoneDark);
        }

        public static void Ferns(TownMeshBuilder b, float s, TownSwatch color)
        {
            for (int i = 0; i < 6; i++)
            {
                float a = i * 1.047f + 0.2f;
                var dir = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a));
                var side = new Vector3(-dir.z, 0f, dir.x) * 0.13f * s;
                Vector3 mid = dir * 0.38f * s + Vector3.up * 0.32f * s;
                Vector3 tip = dir * 0.7f * s + Vector3.up * 0.12f * s;
                TownSwatch c = (i & 1) == 0 ? color : TownSwatch.GrassDark;
                b.TriangleFacing(Vector3.up * 0.04f, mid + side, tip, Vector3.up, c);
                b.TriangleFacing(Vector3.up * 0.04f, tip, mid - side, Vector3.up, c);
            }
        }

        /// <summary>A flat irregular puddle (7 corners).</summary>
        public static void Puddle(TownMeshBuilder b, float s, int seed)
        {
            const int n = 7;
            Vector3 center = Vector3.up * 0.04f;
            for (int i = 0; i < n; i++)
            {
                float a0 = i * Mathf.PI * 2f / n;
                float a1 = (i + 1) * Mathf.PI * 2f / n;
                float r0 = (0.75f + 0.45f * TownStyle.Hash01("pud", seed * 13 + i)) * s;
                float r1 = (0.75f + 0.45f * TownStyle.Hash01("pud", seed * 13 + (i + 1) % n)) * s;
                b.Triangle(center, center + new Vector3(Mathf.Cos(a1) * r1, 0f, Mathf.Sin(a1) * r1), center + new Vector3(Mathf.Cos(a0) * r0, 0f, Mathf.Sin(a0) * r0), TownSwatch.Water);
            }
        }

        public static void Log(TownMeshBuilder b, float s)
        {
            b.Log(new Vector3(-1.3f, 0.2f, 0f) * s, new Vector3(1.3f, 0.24f, 0.05f) * s, 0.22f * s, TownSwatch.Trunk, 6, TownSwatch.WoodLight);
            b.Log(new Vector3(0.4f, 0.3f, 0.1f) * s, new Vector3(0.75f, 0.62f, 0.45f) * s, 0.07f * s, TownSwatch.Trunk, 4);
            b.Blob(new Vector3(-0.5f, 0.42f, 0f) * s, new Vector3(0.7f, 0.16f, 0.36f) * s, TownSwatch.Vine);
        }

        public static void Stump(TownMeshBuilder b, float s)
        {
            b.Cylinder(Vector3.zero, 0.36f * s, 0.3f * s, 0.48f * s, 6, TownSwatch.Trunk, TownSwatch.WoodLight);
            Spike(b, new Vector3(0.28f, 0f, 0.05f) * s, new Vector3(0.62f, 0.02f, 0.12f) * s, 0.08f * s, TownSwatch.Trunk);
            Spike(b, new Vector3(-0.2f, 0f, -0.2f) * s, new Vector3(-0.48f, 0.02f, -0.46f) * s, 0.08f * s, TownSwatch.Trunk);
        }

        public static void MossRock(TownMeshBuilder b, float s, TownSwatch rock)
        {
            b.Crown(new Vector3(0f, 0.32f * s, 0f), new Vector3(1.2f, 0.8f, 1.0f) * s, rock, 20f);
            b.Crown(new Vector3(0.08f * s, 0.62f * s, 0.05f * s), new Vector3(0.9f, 0.26f, 0.7f) * s, TownSwatch.Vine, 70f);
        }

        public static void Cairn(TownMeshBuilder b, float s)
        {
            b.Crown(new Vector3(0f, 0.3f, 0f) * s, new Vector3(1.2f, 0.62f, 1.1f) * s, TownSwatch.Stone, 10f);
            b.Crown(new Vector3(0.05f, 0.8f, 0.02f) * s, new Vector3(0.85f, 0.48f, 0.8f) * s, TownSwatch.StoneDark, 50f);
            b.Crown(new Vector3(0f, 1.18f, 0.04f) * s, new Vector3(0.5f, 0.36f, 0.48f) * s, TownSwatch.Stone, 80f);
        }

        public static void Basalt(TownMeshBuilder b, float s)
        {
            b.Cylinder(Vector3.zero, 0.42f * s, 0.42f * s, 2.6f * s, 6, TownSwatch.Basalt, TownSwatch.StoneDark);
            b.Cylinder(new Vector3(0.75f, 0f, 0.2f) * s, 0.38f * s, 0.38f * s, 1.7f * s, 6, TownSwatch.Basalt, TownSwatch.StoneDark, 0.5f);
            b.Cylinder(new Vector3(0.2f, 0f, 0.78f) * s, 0.36f * s, 0.36f * s, 1.1f * s, 6, TownSwatch.StoneDark, TownSwatch.Basalt);
            b.Cylinder(new Vector3(-0.6f, 0f, 0.45f) * s, 0.34f * s, 0.34f * s, 0.6f * s, 6, TownSwatch.Basalt, TownSwatch.StoneDark, 0.5f);
        }

        public static void Column(TownMeshBuilder b, float s, int variant)
        {
            b.Box(new Vector3(0f, 0.15f, 0f) * s, new Vector3(1f, 0.3f, 1f) * s, TownSwatch.Sandstone, 0f, TownSwatch.Paving);
            float h = (variant & 1) == 0 ? 2.4f : 1.2f;
            b.Cylinder(new Vector3(0f, 0.3f, 0f) * s, 0.36f * s, 0.33f * s, h * s, 8, TownSwatch.Paving, TownSwatch.PavingDark);
            if ((variant & 2) == 0)
            {
                b.Log(new Vector3(0.9f, 0.32f, -0.4f) * s, new Vector3(1.9f, 0.32f, -0.1f) * s, 0.32f * s, TownSwatch.Paving, 8, TownSwatch.PavingDark);
            }
        }

        public static void Cypress(TownMeshBuilder b, float s)
        {
            b.Cylinder(Vector3.zero, 0.14f * s, 0.1f * s, 0.8f * s, 5, TownSwatch.Trunk);
            b.Crown(new Vector3(0f, 2.6f, 0f) * s, new Vector3(1.1f, 4.4f, 1.1f) * s, TownSwatch.LeafDark, 15f);
            b.Crown(new Vector3(0f, 4.6f, 0f) * s, new Vector3(0.6f, 1.6f, 0.6f) * s, TownSwatch.Pine, 50f);
        }

        public static void DeadTree(TownMeshBuilder b, float s)
        {
            Vector3 top = new Vector3(0.2f, 2.8f, 0.1f) * s;
            b.Log(Vector3.zero, top, 0.16f * s, TownSwatch.WoodWeathered, 5);
            b.Log(new Vector3(0.1f, 1.6f, 0.05f) * s, new Vector3(0.9f, 2.4f, 0.3f) * s, 0.07f * s, TownSwatch.WoodWeathered, 4);
            b.Log(new Vector3(0.15f, 2.1f, 0.08f) * s, new Vector3(-0.6f, 2.9f, -0.3f) * s, 0.06f * s, TownSwatch.WoodWeathered, 4);
            b.Log(top, top + new Vector3(0.3f, 0.6f, 0.2f) * s, 0.05f * s, TownSwatch.WoodWeathered, 4);
        }

        // ------------------------------------------------------------------
        // Row pieces (two ground points: they follow the slope)
        // ------------------------------------------------------------------

        /// <summary>A ridge (triangular cross-section) from <paramref name="a"/> to <paramref name="c"/> (ground points): 6 triangles.</summary>
        public static void Ridge(TownMeshBuilder b, Vector3 a, Vector3 c, float width, float height, TownSwatch left, TownSwatch right)
        {
            Vector3 d = c - a;
            d.y = 0f;
            if (d.sqrMagnitude < 1e-4f)
            {
                return;
            }

            Vector3 n = new Vector3(-d.z, 0f, d.x).normalized * (width * 0.5f);
            Vector3 up = Vector3.up * height;
            Vector3 ta = a + up;
            Vector3 tc = c + up;
            b.QuadFacing(a + n, c + n, tc, ta, n + up * 0.5f, left);
            b.QuadFacing(a - n, ta, tc, c - n, -n + up * 0.5f, right);
            b.TriangleFacing(a + n, ta, a - n, -d, left);
            b.TriangleFacing(c - n, tc, c + n, d, right);
        }

        /// <summary>A box-like run from <paramref name="a"/> to <paramref name="c"/> between heights <paramref name="bottom"/> and <paramref name="top"/> over the ground (no underside unless lifted).</summary>
        public static void Run(TownMeshBuilder b, Vector3 a, Vector3 c, float width, float bottom, float top, TownSwatch side, TownSwatch upper)
        {
            Vector3 d = c - a;
            d.y = 0f;
            if (d.sqrMagnitude < 1e-4f)
            {
                return;
            }

            Vector3 n = new Vector3(-d.z, 0f, d.x).normalized * (width * 0.5f);
            Vector3 lo = Vector3.up * bottom;
            Vector3 hi = Vector3.up * top;
            Vector3 a0 = a + n + lo, a1 = a - n + lo, c0 = c + n + lo, c1 = c - n + lo;
            Vector3 a2 = a + n + hi, a3 = a - n + hi, c2 = c + n + hi, c3 = c - n + hi;
            b.QuadFacing(a0, c0, c2, a2, n, side);
            b.QuadFacing(a1, a3, c3, c1, -n, side);
            b.QuadFacing(a2, c2, c3, a3, Vector3.up, upper);
            b.QuadFacing(a0, a2, a3, a1, -d, side);
            b.QuadFacing(c0, c1, c3, c2, d, side);
            if (bottom > 0.05f)
            {
                b.QuadFacing(a0, a1, c1, c0, Vector3.down, side);
            }
        }

        /// <summary>One piece of crop row of <paramref name="crop"/> between ground points <paramref name="a"/> and <paramref name="c"/>.</summary>
        public static void CropRow(TownMeshBuilder b, Vector3 a, Vector3 c, FieldCrop crop, int seed)
        {
            float h01 = TownStyle.Hash01("crop", seed);
            Vector3 mid = (a + c) * 0.5f;
            Vector3 d = c - a;
            switch (crop)
            {
                case FieldCrop.Wheat:
                    Ridge(b, a, c, 0.62f, 0.62f + 0.12f * h01, TownSwatch.LeafGold, TownSwatch.Thatch);
                    break;
                case FieldCrop.Cotton:
                    Ridge(b, a, c, 0.6f, 0.42f, TownSwatch.GrassDark, TownSwatch.LeafDark);
                    for (int i = 0; i < 3; i++)
                    {
                        Vector3 p = Vector3.Lerp(a, c, (i + 0.3f + 0.4f * h01) / 3f) + Vector3.up * 0.36f;
                        b.Cylinder(p, 0.11f, 0f, 0.14f, 3, TownSwatch.ClothWhite, null, i * 0.3f);
                    }

                    break;
                case FieldCrop.Vines:
                    b.Cylinder(a, 0.05f, 0.05f, 1.45f, 4, TownSwatch.WoodWeathered);
                    Run(b, a, c, 0.5f, 0.72f, 1.32f + 0.08f * h01, TownSwatch.Vine, TownSwatch.Grass);
                    b.Cylinder(mid + Vector3.up * 0.55f + Vector3.Cross(d, Vector3.up).normalized * 0.26f, 0.09f, 0f, 0.2f, 3, TownSwatch.Violet);
                    break;
                case FieldCrop.Melons:
                    Ridge(b, a, c, 0.95f, 0.16f, TownSwatch.Vine, TownSwatch.GrassDark);
                    b.Blob(Vector3.Lerp(a, c, 0.25f + 0.2f * h01) + new Vector3(0f, 0.18f, 0f), new Vector3(0.42f, 0.32f, 0.34f), h01 > 0.5f ? TownSwatch.Grass : TownSwatch.ClothYellow);
                    b.Blob(Vector3.Lerp(a, c, 0.75f) + new Vector3(0.12f, 0.16f, 0.1f), new Vector3(0.36f, 0.28f, 0.3f), TownSwatch.Grass);
                    break;
                case FieldCrop.Hay:
                    Ridge(b, a, c, 0.4f, 0.12f, TownSwatch.Thatch, TownSwatch.SandDark);
                    break;
                default:
                    Ridge(b, a, c, 0.55f, 0.2f, TownSwatch.Dirt, TownSwatch.DirtDark);
                    break;
            }
        }

        /// <summary>One piece of hedgerow between ground points <paramref name="a"/> and <paramref name="c"/>.</summary>
        public static void Hedge(TownMeshBuilder b, Vector3 a, Vector3 c, int seed)
        {
            float h01 = TownStyle.Hash01("hedge", seed);
            Run(b, a, c, 1.0f, 0f, 1.05f + 0.2f * h01, TownSwatch.LeafDark, TownSwatch.Poplar);
            Vector3 top = Vector3.Lerp(a, c, 0.3f + 0.4f * h01) + Vector3.up * (1.15f + 0.2f * h01);
            b.Blob(top, new Vector3(1.1f, 0.5f, 1.2f), (seed & 1) == 0 ? TownSwatch.Poplar : TownSwatch.LeafDark);
        }
    }
}
