using System.Collections.Generic;
using Rapadura.Gameplay.World.Biomes;
using UnityEngine;

namespace Rapadura.Gameplay.World.Cities.Town
{
    /// <summary>The four compass arms a town's streets can leave the plaza by (x = east, z = north on the ground plane).</summary>
    public enum CityArm
    {
        North = 0,
        East = 1,
        South = 2,
        West = 3,
    }

    /// <summary>What stands on a lot (drives the building model and the interaction).</summary>
    public enum CityLotKind
    {
        House = 0,
        /// <summary>A shop front: porch, sign, the keeper beside the door.</summary>
        Shop = 1,
        /// <summary>Open-fronted shed (ferraria, marcenaria): roof on posts, the work at the back.</summary>
        Workshop = 2,
        /// <summary>The lamp house at the head of the plaza (Ramrat's guardian lamp): pishtaq portal, dome, lamp tower.</summary>
        LampHouse = 3,
        /// <summary>The stable (Srosh's): long building with stall doors, paddock beside it.</summary>
        Stable = 4,
        /// <summary>Bazar da Nana: market stalls (barracas) instead of a building.</summary>
        Market = 5,
        /// <summary>Cais da Barca (world bible §3.16): a short pier over a groove of light; its trigger opens the Barcas de Luz on the map.</summary>
        ShipDock = 6,
        /// <summary>Vó Edna's house: the deep aivan on carved columns, the tandir oven, the vine trellis and the orchard.</summary>
        Grandma = 7,
    }

    /// <summary>Small things on the ground. Each has its own simple collider (or none) in <see cref="CityLayout.Solids"/>.</summary>
    public enum CityPropKind
    {
        Well = 0,
        Bench = 1,
        LampPost = 2,
        Tree = 3,
        Stall = 4,
        FenceRun = 5,
        Bush = 6,
        FlowerBed = 7,
        Crates = 8,
        HayBale = 9,
        /// <summary>The Relógio de Zurvan: the sundial of the plaza.</summary>
        Sundial = 10,
        Gate = 11,
        Rock = 12,
        GrassTuft = 13,
        /// <summary>Tandir (clay oven).</summary>
        Tandir = 14,
        /// <summary>Vine trellis (latada): four posts and a roof of vines; walkable underneath.</summary>
        Trellis = 15,
        /// <summary>Copper and clay jugs (cântaros).</summary>
        Jugs = 16,
        /// <summary>A tall carved stela (Aksum style, Litema's plaza).</summary>
        Stela = 17,

        // ---- Regional pieces (Qocho, Litema, Lamparinas; drawn by TownKit.Regions) ----

        /// <summary>Qocho: a karez well shaft (a ring of spoil around a timber-framed shaft with a windlass).</summary>
        KarezShaft = 18,
        /// <summary>Qocho: the open stone channel where the karez comes out (<see cref="CityProp.End"/> = the other end); walkable.</summary>
        KarezChannel = 19,
        /// <summary>Qocho: Yardena's cistern under a mud dome, with a stepped arched entrance and a water spout.</summary>
        CisternDome = 20,
        /// <summary>Qocho: the Ruínas dos Artífices at the edge. Variant 0 steep Meroë pyramid with its chapel, 1 broken lotus columns, 2 a row of ushabtis.</summary>
        Ruin = 21,
        /// <summary>Qocho: a Bactrian pack camel lying down with its load (the caravanserai yard).</summary>
        Camel = 22,
        /// <summary>Litema: a rock shelter face painted with elands, dancers and the mantis (San rock art).</summary>
        RockArt = 23,
        /// <summary>Lamparinas: the pink tuff cliff the houses are cut into, from <see cref="CityProp.Position"/> to <see cref="CityProp.End"/> (the mass rises behind that line).</summary>
        Cliff = 24,
        /// <summary>Lamparinas: a rope bridge high over a street, from <see cref="CityProp.Position"/> to <see cref="CityProp.End"/>; overhead, walkable under.</summary>
        RopeBridge = 25,
        /// <summary>Lamparinas: a string of lamps hung across a street (overhead; glows at night).</summary>
        LampString = 26,
        /// <summary>Lamparinas: the vishapakar (dragon stone) at the town's entrance.</summary>
        Vishapakar = 27,
        /// <summary>Lamparinas: the spring of the cave, a basalt wall fountain with a stone spout and a basin.</summary>
        Fountain = 28,
        /// <summary>Litema: a Great Zimbabwe conical tower of dry stone with the chevron frieze.</summary>
        ConicalTower = 29,
        /// <summary>
        /// Copas de Garajonay: a giant laurel with a ring platform at <see cref="TownKit.RopeBridgeHeight"/> × scale (so the rope
        /// bridges of the same scale land on it), a round plank house with a layered leaf roof, a spiral stair round the
        /// trunk and cocoon lanterns; the crown and the hanging moss above.
        /// </summary>
        CanopyTree = 30,

        // ---- Taa Pwani, the Costa das Monções (drawn by TownKit.Coast) ----

        /// <summary>Taa Pwani: a dhow moored in a tidal pool: a teak hull with a raked stem, the mast and its triangular (lateen) sail.</summary>
        Dhow = 31,
        /// <summary>
        /// Taa Pwani: the Farol de Coral, a tall coral-stone lamp tower (the shape of the coast's coral towers, a lighthouse with
        /// no worship): fluted shaft on a square base, a brass band, the lamp gallery and a small cap.
        /// </summary>
        CoralTower = 32,
        /// <summary>Taa Pwani: a mangrove on its stilt roots over a patch of dark mud and brackish water.</summary>
        Mangrove = 33,

        // ---- Tambor Rubro, the orcs' war camp (drawn by TownKit.WarCamp) ----

        /// <summary>
        /// Tambor Rubro: a palisade of sharpened logs from <see cref="CityProp.Position"/> to <see cref="CityProp.End"/>
        /// (points up and leaning out, never trophies), braced on the inside, a walk-board and a band of yellow cloth.
        /// </summary>
        Palisade = 34,
        /// <summary>Tambor Rubro: a square log drum tower with a platform, the great drum under a hide roof and the city's guardian lamp on top.</summary>
        DrumTower = 35,
        /// <summary>Tambor Rubro: a clan tent of ochre-dyed hide on a pole frame, flap open to the front (local −z), a clan banner.</summary>
        ClanTent = 36,
        /// <summary>Tambor Rubro: a clan post (carved log with cloth bands, a small drum and a banner): the clan's sign, not a shrine.</summary>
        ClanPost = 37,
        /// <summary>Tambor Rubro: a fire pit (stone ring, crossed logs, flames that glow at night).</summary>
        Bonfire = 38,
        /// <summary>Tambor Rubro: a great war drum on an X stand, with its two beaters.</summary>
        WarDrum = 39,
        /// <summary>Tambor Rubro: the training arena of sand (a raked disc with a low log border); walkable.</summary>
        TrainingRing = 40,

        // ---- Salões de Bronze de Napirasu, the dwarves' halls (drawn by TownKit.RuneHall) ----

        /// <summary>Salões de Bronze: a square granite pillar with a bronze base and capital and a band of runes that glows.</summary>
        BronzePillar = 41,
        /// <summary>Salões de Bronze: the Grande Forja, gone cold: a stepped granite furnace, bronze bellows, two anvils and the dark crystal of Light.</summary>
        GreatForge = 42,
        /// <summary>Salões de Bronze: a pit of cold lava (dark crust, cracks of cold ember) in a granite rim.</summary>
        LavaPit = 43,
        /// <summary>Salões de Bronze: a chain lift: a bronze frame, the cage and the chains running up into the rock.</summary>
        ChainLift = 44,
        /// <summary>
        /// The rock wall of a city cut into the mountain, from <see cref="CityProp.Position"/> to <see cref="CityProp.End"/> (a
        /// thin wall, its mass behind the line). Variant 0: granite with carved niches and bronze ribs (Salões de Bronze);
        /// 1: a round basalt tunnel wall with a copper air pipe and bottle lamps (Mercado dos Túneis).
        /// </summary>
        RockWall = 45,

        // ---- Mercado dos Túneis, the kobolds' market (drawn by TownKit.TunnelBazaar) ----

        /// <summary>Mercado dos Túneis: mine-cart rails on sleepers from <see cref="CityProp.Position"/> to <see cref="CityProp.End"/>; walkable.</summary>
        MineRails = 46,
        /// <summary>Mercado dos Túneis: a mine cart (vagonete) on a short stretch of rail, loaded with ore or bottles (variant).</summary>
        MineCart = 47,
        /// <summary>Mercado dos Túneis: a copper air pipe rising into the rock with a fan turning inside a tube (it "pulls" the air).</summary>
        AirPipe = 48,
        /// <summary>Mercado dos Túneis: a market stall under a small basalt arch, canvas awning and bottle lanterns, front to local −z.</summary>
        NicheStall = 49,
        /// <summary>Mercado dos Túneis: a pulley frame (two posts, a wheel, a rope and a bucket of ore).</summary>
        Pulley = 50,

        // ---- Aratta, the city of the horns (povo-touro; drawn by TownKit.ReedHorn) ----

        /// <summary>Aratta: a horn post, a way-marker (not a cult object): a reed-bound pole with a pair of aurochs horns and blue ribbons.</summary>
        HornPost = 51,
        /// <summary>Aratta: a round enclosure of dry stone (cercado) with a gap to the front and a hay rack inside.</summary>
        StoneKraal = 52,
        /// <summary>Aratta: the bull-headed lyre of the plaza on its clay plinth (the Lira do Touro), front to local −z.</summary>
        BullLyre = 53,
        /// <summary>Aratta: a flat-topped savanna acacia.</summary>
        Acacia = 54,

        // ---- Kirta, the reef city (povo-do-mar; drawn by TownKit.CoralReef) ----

        /// <summary>Kirta: the mother coral in its pool: a great branching coral, one side dim grey, the relit side glowing.</summary>
        CoralMother = 55,
        /// <summary>Kirta: a free-standing coral tower with round windows and a mother-of-pearl dome.</summary>
        CoralSpire = 56,
        /// <summary>Kirta: a garden of tall kelp ribbons in a ring of shells.</summary>
        KelpGarden = 57,
        /// <summary>Kirta: a strip of reef water from <see cref="CityProp.Position"/> to <see cref="CityProp.End"/> crossed by a shell bridge in the middle; walkable.</summary>
        ShellBridge = 58,
        /// <summary>Kirta: the edge of the reef over the abyss from <see cref="CityProp.Position"/> to <see cref="CityProp.End"/>: a coral rim, the deep water beyond (its mass behind the line).</summary>
        ReefEdge = 59,

        // ---- Jhilpur, the city in the roots (Delta de Jhilpur; drawn by TownKit.RootStilt) ----

        /// <summary>Jhilpur: the Raiz-Mãe, a giant banyan on a round brick platform, curtains of aerial roots, Living Water at its foot.</summary>
        MotherRoot = 60,
        /// <summary>Jhilpur: a banyan fig with aerial roots dropping to the ground round its trunk (Variant: leaf shade).</summary>
        Banyan = 61,
        /// <summary>Jhilpur: a round pond of murky water with lotus pads, pink flowers and a floating lotus lantern, in a brick rim.</summary>
        LotusPond = 62,
        /// <summary>Jhilpur: a canal from <see cref="CityProp.Position"/> to <see cref="CityProp.End"/> crossed by a living-root bridge in the middle; walkable.</summary>
        RootBridge = 63,
        /// <summary>Jhilpur: a brick ghat from <see cref="CityProp.Position"/> to <see cref="CityProp.End"/>: a parapet, steps going down to the water behind the line (its mass).</summary>
        Ghat = 64,

        // ---- Palafitas de Tarchon, the trols' city (Delta de Jhilpur; drawn by TownKit.StiltVillage) ----

        /// <summary>Tarchon: the edge of the open swamp from <see cref="CityProp.Position"/> to <see cref="CityProp.End"/>: a rail of wet piles and rush, the bog water beyond (its mass behind the line).</summary>
        SwampEdge = 65,
        /// <summary>Tarchon: a strip of bog water from <see cref="CityProp.Position"/> to <see cref="CityProp.End"/> crossed by a plank boardwalk on short piles in the middle; walkable.</summary>
        Boardwalk = 66,
        /// <summary>Tarchon: the Mastro dos Fogos-Fátuos, a tall mast on a round deck hung with lanterns of caught will-o'-wisps, a giant basket at its foot.</summary>
        WispMast = 67,
        /// <summary>Tarchon: a giant woven rush basket, taller than a cousin (Variant: fish / empty / lid).</summary>
        GiantBasket = 68,
        /// <summary>Tarchon: two poles and a fishing net drying between them (yaw: along local x).</summary>
        NetRack = 69,
        /// <summary>Tarchon: a floating reed island in a round patch of bog water, rush tufts and a will-o'-wisp over it.</summary>
        ReedIsland = 70,

        // ---- Sangkent, the stair city (Teto do Mundo; drawn by TownKit.StoneTerrace) ----

        /// <summary>Sangkent: a dry-stone retaining wall from <see cref="CityProp.Position"/> to <see cref="CityProp.End"/> with the terraces stepping up behind it (its mass behind the line; Variant 1: the fort on the top terrace).</summary>
        TerraceWall = 71,
        /// <summary>Sangkent: the foot of a polished stone chute coming down the terraces behind it, the landing in front with a plank cart (yaw: the landing faces local −z).</summary>
        StoneChute = 72,
        /// <summary>Sangkent: a plank chute cart on stone runners with the five ribbons of the five lights (Variant: loaded with apricots / with felt bales).</summary>
        ChuteCart = 73,
        /// <summary>Sangkent: an apricot tree in a ring of stones (Variant 0: in blossom, 1: in fruit).</summary>
        ApricotTree = 74,
        /// <summary>Sangkent: a stone irrigation channel from <see cref="CityProp.Position"/> to <see cref="CityProp.End"/> crossed by a stone slab in the middle; walkable.</summary>
        IrrigationChannel = 75,
        /// <summary>Sangkent: the Marco das Caravanas, a tall milestone on a round stone step with the five ribbons of the five lights and a hanging lantern.</summary>
        CaravanStone = 76,

        // ---- Mingcheng, the city of nine rings (Planície de Mingcheng; drawn by TownKit.RingWall) ----

        /// <summary>Mingcheng: a stretch of ring wall (rammed earth faced with grey brick, crenellated) from <see cref="CityProp.Position"/> to <see cref="CityProp.End"/>, the roofs of the inner rings behind it (Variant 1: the closed three-arch gate of the 7th ring with its watchtower).</summary>
        RingWall = 77,
        /// <summary>Mingcheng: the Torre do Tambor e do Sino, the city's clock: a brick base with a lacquered pavilion of two curved roofs, the drum and the bell.</summary>
        BellDrumTower = 78,
        /// <summary>Mingcheng: a weeping willow in a ring of grey brick (Variant 1: with a lantern hanging from it).</summary>
        Willow = 79,
        /// <summary>Mingcheng: a canal from <see cref="CityProp.Position"/> to <see cref="CityProp.End"/> crossing the street, with a humpback stone bridge in the middle; walkable.</summary>
        CanalBridge = 80,
        /// <summary>Mingcheng: a lacquered frame hung with paper lanterns and their riddle slips (the Festa das Lanternas; Variant 1: with the fish lantern).</summary>
        LanternFrame = 81,

        // ---- Torres Brancas de Amanirenas, the high elves' city (drawn by TownKit.WhiteSpire) ----

        /// <summary>Torres Brancas: a white garden-terrace wall from <see cref="CityProp.Position"/> to <see cref="CityProp.End"/> with the terraces and slender spires behind it (Variant 1: the tallest spire with its glass dome and leaf bridges).</summary>
        SpireTerrace = 82,
        /// <summary>Torres Brancas: the Relógio de Espelhos, a white obelisk with the sun disc on a round step ringed by bronze mirrors on posts.</summary>
        MirrorClock = 83,
        /// <summary>Torres Brancas: a garden rill from <see cref="CityProp.Position"/> to <see cref="CityProp.End"/> crossing the street, with a leaf bridge (a curved stone blade) in the middle; walkable.</summary>
        LeafBridge = 84,
        /// <summary>Torres Brancas: a bronze mirror on a slim white post, tilted to pass the light (Variant 1: with a glowing beam stone at its foot).</summary>
        MirrorPost = 85,
        /// <summary>Torres Brancas: a small steep pyramid of Meroë sandstone on a plinth, a little pylon front and a gold cap (the garden's memory of Meroë).</summary>
        SpirePyramid = 86,

        // ---- Acampamento de Soslan, the centaurs' camp (Estepe dos Nartes; drawn by TownKit.SteppeCamp) ----

        /// <summary>Soslan: a small felt tent on a two-wheeled cart (the camp that moves), its shafts on the ground (Variant 1: loaded with felt bales instead of a tent).</summary>
        FeltWagon = 87,
        /// <summary>Soslan: a tall pole with horsehair tassels, a dyed felt pennant and a gold deer of the Scythian animal style on top.</summary>
        HorseStandard = 88,
        /// <summary>Soslan: a rope corral from <see cref="CityProp.Position"/> to <see cref="CityProp.End"/>: posts with two ropes and a hay pile behind (its mass behind the line).</summary>
        RopeCorral = 89,
        /// <summary>Soslan: the collective stone oven, a dome of stones with a glowing mouth, a flat bread peel and firewood.</summary>
        StoneOven = 90,

        // ---- Fortim dos Gigantes-da-Colina (Estepe dos Nartes; drawn by TownKit.HillFort) ----

        /// <summary>Fortim: an earth rampart faced with stone from <see cref="CityProp.Position"/> to <see cref="CityProp.End"/>, the next ring of the hill behind it (Variant 1: with a round stone watch-tower in the middle).</summary>
        EarthRing = 91,
        /// <summary>Fortim: a giants' stone table (a slab on two stone legs) with a bowl and a cup as big as a cousin.</summary>
        GiantTable = 92,
        /// <summary>Fortim: a pot the size of a house over the Living Fire, on three stones, the good smoke rising and a ladle in it.</summary>
        GiantPot = 93,
        /// <summary>Fortim: a giant wooden spoon lying on two stones, used as a bench by the cousins.</summary>
        SpoonBench = 94,

        // ---- Arvoredo de Haripushpa, the trunk folk's grove (Estepe dos Nartes; drawn by TownKit.LivingGrove) ----

        /// <summary>Arvoredo: a mossy standing stone of the circle (Variant 1: a leaning one with a flower ring).</summary>
        MossStone = 95,
        /// <summary>Arvoredo: a tall larch of the taiga (Variant 1: an old one asleep, with a face in the bark).</summary>
        Larch = 96,
        /// <summary>Arvoredo: a meltwater stream from <see cref="CityProp.Position"/> to <see cref="CityProp.End"/> crossing the trail, with a split-log bridge in the middle; walkable.</summary>
        MeltStream = 97,
    }

    public enum CityExitKind
    {
        /// <summary>Back to the world map (the road out of town).</summary>
        ToMap = 0,
        /// <summary>The Cais da Barca: back to the map with the Barcas de Luz (fast travel) window open.</summary>
        ShipDock = 1,
    }

    /// <summary>A collider footprint, the single source for both the real colliders and the layout rules.</summary>
    public readonly struct CitySolid
    {
        public readonly bool IsCapsule;

        /// <summary>Box: axis-aligned footprint. Capsule: centre in <see cref="Rect.center"/>.</summary>
        public readonly Rect Footprint;

        public readonly float Radius;
        public readonly float Height;
        public readonly string Owner;

        private CitySolid(bool capsule, Rect footprint, float radius, float height, string owner)
        {
            IsCapsule = capsule;
            Footprint = footprint;
            Radius = radius;
            Height = height;
            Owner = owner;
        }

        public static CitySolid Box(Rect footprint, float height, string owner) => new CitySolid(false, footprint, 0f, height, owner);

        public static CitySolid Capsule(Vector2 center, float radius, float height, string owner) =>
            new CitySolid(true, new Rect(center.x - radius, center.y - radius, radius * 2f, radius * 2f), radius, height, owner);

        public Vector2 Center => Footprint.center;

        /// <summary>Ground distance from <paramref name="point"/> to this solid (0 inside).</summary>
        public float DistanceTo(Vector2 point)
        {
            if (IsCapsule)
            {
                return Mathf.Max(0f, Vector2.Distance(point, Footprint.center) - Radius);
            }

            return CityGeometry.DistanceToRect(point, Footprint);
        }
    }

    /// <summary>One building plot (or market / cart stop) of a town.</summary>
    public sealed class CityLot
    {
        public string Id;
        public CityLotKind Kind;

        /// <summary><c>CityCatalog</c> resident id of the keeper (null = nobody stands here).</summary>
        public string ResidentId;

        /// <summary>Shop opened by the keeper (<c>CityShopCatalog</c> id), or null.</summary>
        public string ShopId;

        /// <summary>The street this lot fronts (null for plaza / free lots).</summary>
        public CityArm? Arm;

        /// <summary>Distance of the door along <see cref="Arm"/> from the plaza centre.</summary>
        public float DoorAlong;

        /// <summary>Centre of the whole footprint (porch included).</summary>
        public Vector2 Center;

        /// <summary>Unit direction the front (door) faces.</summary>
        public Vector2 Front;

        /// <summary>Frontage (along the street) and total depth (porch + walls).</summary>
        public float Width;
        public float Depth;
        public float PorchDepth;

        /// <summary>World AABB of everything the lot occupies (porch included).</summary>
        public Rect Footprint;

        /// <summary>Middle of the door on the facade wall.</summary>
        public Vector2 Door;

        /// <summary>Where a visitor stands to use the door / talk to the keeper (on the porch edge / sidewalk).</summary>
        public Vector2 DoorApproach;

        /// <summary>Where the keeper stands (beside the door, never in front of it).</summary>
        public Vector2 StandPoint;

        public float InteractRadius = 2.4f;

        public TownBuildingSpec Building;

        /// <summary>Yaw (degrees) that turns the kit's south-facing (−z) front to <see cref="Front"/>.</summary>
        public float Yaw => CityGeometry.YawFacing(Front);

        public bool HasKeeper => !string.IsNullOrEmpty(ResidentId) || !string.IsNullOrEmpty(ShopId);
    }

    public sealed class CityStreet
    {
        public CityArm Arm;
        public float Length;
        public float Width;
        public float SidewalkWidth;

        /// <summary>Road + both sidewalks, from the plaza centre to the end of the arm.</summary>
        public Rect Bounds;

        /// <summary>The carriageway only.</summary>
        public Rect Road;

        public Vector2 Direction => CityGeometry.DirectionOf(Arm);
        public Vector2 End => Direction * Length;
    }

    public sealed class CityProp
    {
        public CityPropKind Kind;
        public Vector2 Position;
        public float Yaw;
        public float Scale = 1f;
        public int Variant;

        /// <summary>Fence runs: the other end (the run goes from <see cref="Position"/> to here).</summary>
        public Vector2 End;
    }

    public sealed class CityExit
    {
        public string Id;
        public CityExitKind Kind;

        /// <summary>Trigger box on the ground.</summary>
        public Rect Trigger;

        /// <summary>Point a walker heads for to use it (inside <see cref="Trigger"/>).</summary>
        public Vector2 Target;
    }

    /// <summary>
    /// A generated walkable town: plaza, streets, lots (with doors, keepers and approach points), props, exits, the
    /// respawn and arrival points and the safe-zone box. Plain data on the ground plane (x, z as a Vector2), built
    /// deterministically by <see cref="CityLayoutGenerator"/> from a <see cref="CityTownRecipe"/>; the scene view
    /// (<see cref="CityTownView"/>) and the EditMode rules (<see cref="CityLayoutRules"/>) both read it.
    /// </summary>
    public sealed class CityLayout
    {
        public string CityId;
        public BiomeType Region;
        public TownStyle Style;

        public Vector2 PlazaCenter;
        public float PlazaRadius;

        public readonly List<CityStreet> Streets = new List<CityStreet>();
        public readonly List<CityLot> Lots = new List<CityLot>();
        public readonly List<CityProp> Props = new List<CityProp>();
        public readonly List<CityExit> Exits = new List<CityExit>();

        /// <summary>Where a defeated hero wakes up (the plaza, Docs/Cities.md §3.8).</summary>
        public Vector2 Respawn;

        /// <summary>Where the hero appears when walking in from the map, and the direction they face.</summary>
        public Vector2 Arrival;
        public Vector2 ArrivalFacing = Vector2.up;

        /// <summary>The safe-zone trigger (SafeZoneRules: ZoneKind.City inside).</summary>
        public Rect SafeZone;

        /// <summary>Invisible walls keep the walker inside this box.</summary>
        public Rect WalkBounds;

        /// <summary>Half size of the painted ground (backdrop forest beyond the walk bounds).</summary>
        public float GroundHalfExtent = 90f;

        /// <summary>Real point lights at night (mobile budget, from the recipe).</summary>
        public int NightLights = 6;

        /// <summary>The town never gets brighter than this "night" (0 = normal day/night; the Lamparinas live in a cave, always lamp-lit).</summary>
        public float AmbientNight;

        private List<CitySolid> _solids;

        public CityLot Lot(string id) => Lots.Find(lot => lot.Id == id);

        public CityLot LotOfResident(string residentId) => Lots.Find(lot => lot.ResidentId == residentId);

        public CityLot LotOfShop(string shopId) => Lots.Find(lot => lot.ShopId == shopId);

        public CityStreet Street(CityArm arm) => Streets.Find(street => street.Arm == arm);

        /// <summary>Every collider footprint of the town (cached; buildings, posts, trunks, props).</summary>
        public IReadOnlyList<CitySolid> Solids => _solids ?? (_solids = CitySolids.Build(this));

        /// <summary>Forget the cached solids (after editing the layout in a test).</summary>
        public void Invalidate() => _solids = null;

        /// <summary>
        /// Walking route from the plaza to <paramref name="lot"/>'s door: plaza centre, the point on the street axis
        /// opposite the door (street lots), then the door approach. Used by the PlayMode walk and as a hint for NPC AI.
        /// </summary>
        public List<Vector2> RouteToDoor(CityLot lot)
        {
            var route = new List<Vector2> { PlazaCenter };
            if (lot == null)
            {
                return route;
            }

            if (lot.Arm.HasValue)
            {
                route.Add(PlazaCenter + CityGeometry.DirectionOf(lot.Arm.Value) * lot.DoorAlong);
            }

            route.Add(lot.DoorApproach);
            return route;
        }
    }

    /// <summary>Ground-plane geometry helpers (pure).</summary>
    public static class CityGeometry
    {
        public static Vector2 DirectionOf(CityArm arm)
        {
            switch (arm)
            {
                case CityArm.North: return Vector2.up;
                case CityArm.East: return Vector2.right;
                case CityArm.South: return Vector2.down;
                default: return Vector2.left;
            }
        }

        /// <summary>Yaw that maps the kit's local −z (front) onto <paramref name="front"/> (x, z).</summary>
        public static float YawFacing(Vector2 front)
        {
            if (front.sqrMagnitude < 1e-6f)
            {
                return 0f;
            }

            float yaw = Mathf.Atan2(-front.x, -front.y) * Mathf.Rad2Deg;
            return yaw < 0f ? yaw + 360f : yaw;
        }

        /// <summary>Rotates a local ground offset (x right, y = local z) by <paramref name="yawDegrees"/> like Unity's Y rotation.</summary>
        public static Vector2 Rotate(Vector2 local, float yawDegrees)
        {
            float r = yawDegrees * Mathf.Deg2Rad;
            float c = Mathf.Cos(r);
            float s = Mathf.Sin(r);
            return new Vector2(local.x * c + local.y * s, -local.x * s + local.y * c);
        }

        public static Rect RectFromCenter(Vector2 center, Vector2 size) => new Rect(center - size * 0.5f, size);

        public static Rect RectFromMinMax(Vector2 min, Vector2 max) => Rect.MinMaxRect(Mathf.Min(min.x, max.x), Mathf.Min(min.y, max.y),
            Mathf.Max(min.x, max.x), Mathf.Max(min.y, max.y));

        /// <summary>AABB of a <paramref name="size"/> rectangle centred on <paramref name="center"/> turned by <paramref name="yaw"/>.</summary>
        public static Rect RotatedBounds(Vector2 center, Vector2 size, float yaw)
        {
            float r = yaw * Mathf.Deg2Rad;
            float c = Mathf.Abs(Mathf.Cos(r));
            float s = Mathf.Abs(Mathf.Sin(r));
            var half = new Vector2(size.x * 0.5f * c + size.y * 0.5f * s, size.x * 0.5f * s + size.y * 0.5f * c);
            return new Rect(center - half, half * 2f);
        }

        public static float DistanceToRect(Vector2 point, Rect rect)
        {
            float dx = Mathf.Max(rect.xMin - point.x, 0f, point.x - rect.xMax);
            float dy = Mathf.Max(rect.yMin - point.y, 0f, point.y - rect.yMax);
            return Mathf.Sqrt(dx * dx + dy * dy);
        }

        /// <summary>Strict overlap (touching edges do not count), shrunk by <paramref name="tolerance"/>.</summary>
        public static bool Overlaps(Rect a, Rect b, float tolerance = 0.001f)
        {
            return a.xMin < b.xMax - tolerance && b.xMin < a.xMax - tolerance && a.yMin < b.yMax - tolerance && b.yMin < a.yMax - tolerance;
        }

        public static bool RectOverlapsCircle(Rect rect, Vector2 center, float radius)
        {
            return DistanceToRect(center, rect) < radius;
        }

        public static bool Contains(Rect outer, Rect inner)
        {
            return inner.xMin >= outer.xMin && inner.xMax <= outer.xMax && inner.yMin >= outer.yMin && inner.yMax <= outer.yMax;
        }

        public static Rect Inflate(Rect rect, float amount) => new Rect(rect.x - amount, rect.y - amount, rect.width + amount * 2f, rect.height + amount * 2f);
    }
}
