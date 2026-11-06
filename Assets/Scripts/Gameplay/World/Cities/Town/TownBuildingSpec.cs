namespace Rapadura.Gameplay.World.Cities.Town
{
    public enum TownRoofKind
    {
        /// <summary>Flat packed-earth roof with a parapet and wooden beam ends (Sogdian / oasis adobe).</summary>
        Flat = 0,
        /// <summary>A dome on a low drum (the lamp house, Qocho's cistern domes).</summary>
        Dome = 1,
        /// <summary>Conical thatch (Litema rondavels: the walls become round).</summary>
        Cone = 2,
        /// <summary>Two tiled slopes, ridge parallel to the street (sheds, stables).</summary>
        Gable = 3,
        /// <summary>No roof: the house is a facade cut into a rock mass (Lamparinas tuff houses).</summary>
        Rock = 4,
    }

    /// <summary>The shop dressing in front of / around a shop front (goods in the aivan, sign colour, tools).</summary>
    public enum TownShopDressing
    {
        None = 0,
        GeneralStore = 1,
        Mercearia = 2,
        Armarinho = 3,
        Ferraria = 4,
        Marcenaria = 5,
        Costureira = 6,
        Haras = 7,
        Lamparinas = 8,
        Selaria = 9,
        Forja = 10,
        /// <summary>The Vila's lamp house (Ramrat's guardian lamp): pishtaq portal, dome, lamps in niches.</summary>
        LampHouse = 11,

        // Residents without a counter: what their front says about them (TownKit.Regions).

        /// <summary>Coti (Litema): striped Basotho blankets on a line and a small loom.</summary>
        Weaver = 12,
        /// <summary>Innaios (Litema): skin drums and a lyre by the door.</summary>
        Drums = 13,
        /// <summary>Gabryab (Lamparinas): baskets of wicks and a cutting block.</summary>
        Wicks = 14,
        /// <summary>Aso (Lamparinas): a carpet with cushions where the children sit for the Ananse stories.</summary>
        Storyteller = 15,
        /// <summary>Old Mihrshah (Lamparinas): pick, coil of rope and his miner's lamp.</summary>
        OldMiner = 16,
        /// <summary>Yardena (Qocho): water jars and the cistern's copper ladle.</summary>
        Cistern = 17,
        /// <summary>Uzzai (Qocho): a goat at the door, a water trough and a crook.</summary>
        Herder = 18,
        /// <summary>A Qocho potter's front: jars drying on a plank, a kick wheel.</summary>
        Pottery = 19,
        /// <summary>Mwana (Taa Pwani): the pearl bazaar: trays of pearls and cowries, a brass scale, a rolled kanga.</summary>
        Pearls = 20,
        /// <summary>Zawadi (Taa Pwani): bundles of mangrove root, a rack of woven root boots, a soaking tub.</summary>
        RootWeaver = 21,
        /// <summary>Kessi (Tambor Rubro): a drying frame of red hides, a bow and a quiver on pegs, a coil of snare cord.</summary>
        HuntingRack = 22,
        /// <summary>Puzur (Salões de Bronze): bronze rune plates on a stand, a slate of runes on the wall, a rune lamp.</summary>
        Runes = 23,
        /// <summary>Miwnay (Mercado dos Túneis): racks of glass bottles of Ar Sereno (glowing lilac), a funnel, a crate of corks.</summary>
        AirBottles = 24,
        /// <summary>Shayn / Takhsich (Mercado dos Túneis): mended scrap, gears on a bench, a little fan in a tube.</summary>
        Tinkerer = 25,
        /// <summary>Lugalbanda (Aratta): a reed mat with rolled maps, a coil of rope, a walking staff and a relic of the Artífices.</summary>
        Wanderer = 26,
        /// <summary>Danel (Kirta): a rack of breathing shells, jars of bottled air bubbles, diving stones on a cord, a conch whistle.</summary>
        DivingGear = 27,
        /// <summary>Nuhzadag (Jhilpur): an upturned reed boat, bamboo balance poles in a rack, antivenom amulets on a cord, a basket of purifying fruit.</summary>
        BoatTrader = 28,
        /// <summary>Larthia (Palafitas de Tarchon): stacked rush baskets, a basket as tall as a cousin, bundles of rush, a lantern basket of will-o'-wisps.</summary>
        BasketWeaver = 29,
        /// <summary>Sisinnios (Sangkent): a rack of quilted felt coats, crampons hanging on a peg, a stack of folded storm cloaks, a coil of rope.</summary>
        Outfitter = 30,
        /// <summary>Mōzak (Mingcheng): a rack of paper lanterns (round, tall, the fish lantern), riddle slips hanging from them, a pot of paste and a stack of paper.</summary>
        LanternMaker = 31,
        /// <summary>Natakamani (Torres Brancas): bronze mirrors on stands catching the light, a crate of polished stones and a little brazier of light.</summary>
        Mirrors = 32,
        /// <summary>Syrdon (Acampamento de Soslan): a saddle on a trestle, folded felt saddle blankets, a horsehair halter on a post.</summary>
        Saddlery = 33,
        /// <summary>Shahmizad (Fortim dos Gigantes): a giant ladle, a stack of wooden bowls, a sack of grain and a small fire under a pot.</summary>
        GreatPot = 34,
        /// <summary>Suvarnapushpa (Arvoredo de Haripushpa): rows of seedlings in bark pots, a watering gourd, a young larch in a tub.</summary>
        Nursery = 35,
        /// <summary>Sisin (Farnkesh): coils of yak rope on a peg rack, a stack of bridge planks, a balance pole and a basket.</summary>
        RopeWalker = 36,
        /// <summary>Marsyas (Vale dos Sátiros): double flutes on a stand, a pan-pipe, a drum and a basket of grapes.</summary>
        DoubleFlute = 37,
    }

    /// <summary>
    /// Visual parameters of one building, decided by the layout generator (from the recipe + the region style + a
    /// stable hash of the lot id): size, walls, roof, the aivan (deep porch on carved columns), openings, the chimney of
    /// a tandir/forge and the shop dressing. The kit (<see cref="TownBuildingBuilder"/>) turns it into a mesh in the lot's
    /// local frame (origin at the footprint centre, front facing −z, the aivan in the front <see cref="PorchDepth"/> m).
    /// </summary>
    public sealed class TownBuildingSpec
    {
        public CityLotKind Kind;

        /// <summary>Frontage (x).</summary>
        public float Width = 6f;

        /// <summary>Walls depth (z), aivan not included.</summary>
        public float Depth = 5f;

        public float PorchDepth = 1.4f;

        /// <summary>Carved columns along the whole aivan instead of two corner ones.</summary>
        public bool FullVaranda;

        public float WallHeight = 2.9f;

        /// <summary>1 or 2 (the Loja Geral is a two-storey house).</summary>
        public int Floors = 1;

        public TownRoofKind Roof = TownRoofKind.Flat;

        public TownSwatch WallColor = TownSwatch.Adobe;
        public TownSwatch TrimColor = TownSwatch.Plaster;
        public TownSwatch RoofColor = TownSwatch.Mud;
        public TownSwatch JoineryColor = TownSwatch.Walnut;
        public TownSwatch AccentColor = TownSwatch.Turquoise;
        public TownSwatch PlinthColor = TownSwatch.Stone;

        /// <summary>Timber-framed walls (stable, carpenter's shop) instead of adobe.</summary>
        public bool WoodenWalls;

        /// <summary>A smaller room set back on the roof terrace (Sogdian two-level house).</summary>
        public bool UpperRoom;

        /// <summary>A stork's nest on the roof (Zarafshan).</summary>
        public bool StorkNest;

        /// <summary>Qocho: rammed-earth (taipa) walls, the horizontal lift lines of the formwork and rounded, battered corners.</summary>
        public bool RammedEarth;

        /// <summary>Qocho: a grape-drying chamber on the roof (walls of honeycomb brick lattice, bunches hanging inside).</summary>
        public bool DryingLoft;

        /// <summary>
        /// Taa Pwani: a coral-stone house of the Swahili coast: a rectangular carved teak door frame studded with brass, a
        /// <i>baraza</i> (stone bench) along the facade and stepped merlons on the roof terrace's front parapet.
        /// </summary>
        public bool CoralStone;

        /// <summary>
        /// Tambor Rubro: a log hall of the orcs' war camp: thick corner posts and log sill and plate, walls of ochre-dyed hide
        /// laced to battens, a square timber door and, with <see cref="TownRoofKind.Gable"/>, a very steep hide roof whose
        /// ridge poles cross above both gable ends (TownKit.WarCamp).
        /// </summary>
        public bool LogHall;

        /// <summary>
        /// Salões de Bronze: with <see cref="TownRoofKind.Rock"/>, a hall front carved into the granite: square pillars with
        /// bronze capitals at the porch posts, a massive square bronze door with studs and a rune lintel that glows
        /// (TownKit.RuneHall).
        /// </summary>
        public bool RuneHall;

        /// <summary>
        /// Aratta: a mudhif, the great hall of bound reed (Docs/Mapas100.md §3.3, style <c>ReedHorn</c>): a barrel vault of reed
        /// bundles bent into arches and covered in reed mats, the front a lattice of reed with a tall arched door between two
        /// thick bundled columns, a pair of horns over the door, a mat awning on reed poles over the deck (TownKit.ReedHorn).
        /// </summary>
        public bool ReedHall;

        /// <summary>
        /// Kirta: a coral tower (Docs/Mapas100.md §3.3, style <c>CoralReef</c>): a round tower of live coral over a drowned base
        /// (the old waterline, dim and barnacled), round windows, a mother-of-pearl dome on top and a scallop-shell canopy
        /// over the deck (TownKit.CoralReef).
        /// </summary>
        public bool ReefTower;

        /// <summary>
        /// Jhilpur: a house on root-columns (Docs/Mapas100.md §2.1, style <c>RootStilt</c>): a plank floor raised on twisted
        /// aerial-root columns over a brick footing, walls of woven straw between root-bark posts, the curved "boat-back"
        /// thatch roof (do-chala) with its ridge bowed down at both ends, a plank stair down to the deck and lotus lanterns
        /// hanging from the eaves (TownKit.RootStilt).
        /// </summary>
        public bool RootStilt;

        /// <summary>
        /// Palafitas de Tarchon: a trol long house on piles (Docs/Mapas100.md §3.3, style <c>StiltVillage</c>): tall timber
        /// piles standing in the bog water, a plank floor high over it, the gable end to the street with the door, a steep leaf
        /// roof that comes down to the floor on both sides, fishing nets drying on the eaves, a plank stair up from the deck
        /// and a will-o'-wisp lantern under the gable (TownKit.StiltVillage).
        /// </summary>
        public bool StiltLonghouse;

        /// <summary>
        /// Sangkent: a stone-and-loam house of the Pamir (Docs/Mapas100.md §2.1, style <c>StoneTerrace</c>): two storeys of
        /// rubble stone and loam laced with juniper beams (the beam ends showing), a lattice balcony on the upper floor, a flat
        /// roof with a parapet, a smoke hole and apricots drying on it, a timber porch on posts (TownKit.StoneTerrace).
        /// </summary>
        public bool StoneTerrace;

        /// <summary>
        /// Mingcheng: a courtyard hall of the walled cities (Docs/Mapas100.md §2.1, style <c>RingWall</c>): grey brick on a stone
        /// base, red-lacquered columns, lattice windows, a curved grey-tile roof with upturned eaves and a ridge with end
        /// ornaments (golden tile with <see cref="RoofColor"/> = GoldTile), paper lanterns under the eaves (TownKit.RingWall).
        /// </summary>
        public bool RingWallHall;

        /// <summary>
        /// Torres Brancas: a white spire house of the high elves (Docs/Mapas100.md §3.3, style <c>WhiteSpire</c>): a low pointed
        /// pyramid base of Meroë sandstone, a slender lime-white tower with pale-gold bands, tall windows of light, a glass dome
        /// with a gold finial and a bronze mirror catching the sun, a portico of white pillars (TownKit.WhiteSpire).
        /// </summary>
        public bool SpireHouse;

        /// <summary>
        /// Acampamento de Soslan: a round felt tent on a wagon (Docs/Mapas100.md §3.3, style <c>SteppeCamp</c>): a plank bed on
        /// four spoked wheels, the round lattice wall wrapped in felt with dyed bands, a domed felt roof with the smoke crown, a
        /// painted door and a felt awning on two poles in front (TownKit.SteppeCamp).
        /// </summary>
        public bool FeltYurt;

        /// <summary>
        /// Fortim dos Gigantes-da-Colina: a round giant house (Docs/Mapas100.md §3.3, style <c>HillFort</c>): a drum of dry
        /// stone on a stone ring, a giant timber door, a conical turf roof with smoke rising, a stone porch of two posts and a
        /// lintel big enough for a giant (TownKit.HillFort).
        /// </summary>
        public bool GiantRoundhouse;

        /// <summary>
        /// Arvoredo de Haripushpa: a house inside a huge hollow trunk (Docs/Mapas100.md §3.3, style <c>LivingGrove</c>): a
        /// tapering bark trunk with root flares, an arched door into the dark of the hollow, nest windows, moss, a crown of
        /// larch and new leaf on top and root steps up to the door (TownKit.LivingGrove).
        /// </summary>
        public bool HollowTrunk;

        /// <summary>
        /// Farnkesh, os Ninhos de Pedra: a house on a pinnacle (Docs/Mapas100.md map 54, style <c>PinnacleNest</c>): a sandstone
        /// rock foot coming up out of the clouds, white lime walls leaning in, a band of painted red wood under the eaves with
        /// window frames of painted wood, a shallow plank roof weighed down with stones and a balcony out over the void
        /// (TownKit.PinnacleNest).
        /// </summary>
        public bool PinnacleNest;

        /// <summary>
        /// Vale dos Sátiros: a gourd-shaped wicker house (Docs/Mapas100.md §3.3, style <c>Glade</c>): a swelling wicker body on a
        /// ring of stones, a narrower neck and a leafy cap with a stalk, a round door with a flower garland and a vine pergola in
        /// front (TownKit.Glade).
        /// </summary>
        public bool GourdHut;

        /// <summary>
        /// Mercado dos Túneis: with <see cref="TownRoofKind.Rock"/>, a stall set into an arched basalt niche: the round arch,
        /// a striped canvas awning on copper poles, a counter, bottle lanterns and a copper air pipe up the rock (TownKit.TunnelBazaar).
        /// </summary>
        public bool TunnelStall;

        /// <summary>Qocho caravanserai: the porch columns become brick piers carrying stepped pointed arches.</summary>
        public bool Arcade;

        /// <summary>Door centre as a fraction of the width from the middle (−0.5 .. 0.5).</summary>
        public float DoorOffset;

        /// <summary>Double door (shops, stable).</summary>
        public bool WideDoor;

        /// <summary>Front windows (besides the door).</summary>
        public int FrontWindows = 2;

        /// <summary>Windows on each side wall and on the back.</summary>
        public int SideWindows = 1;

        /// <summary>A chimney (tandir oven, forge).</summary>
        public bool Chimney;

        /// <summary>Sign board over the aivan (shops).</summary>
        public bool Sign;
        public TownSwatch SignColor = TownSwatch.ClothRed;

        public TownShopDressing Dressing;

        /// <summary>Stable seed for small variations.</summary>
        public string Seed = string.Empty;

        /// <summary>Height of the top of the walls.</summary>
        public float EaveHeight => TownBuildingBuilder.FloorY + WallHeight * Floors;

        public TownBuildingSpec Clone() => (TownBuildingSpec)MemberwiseClone();

        /// <summary>Key of the cached mesh: equal specs share one mesh.</summary>
        public string CacheKey =>
            $"{Kind}|{Width:0.00}|{Depth:0.00}|{PorchDepth:0.00}|{FullVaranda}|{WallHeight:0.00}|{Floors}|{Roof}|{WallColor}|{TrimColor}|{RoofColor}|" +
            $"{JoineryColor}|{AccentColor}|{PlinthColor}|{WoodenWalls}|{UpperRoom}|{StorkNest}|{RammedEarth}|{DryingLoft}|{Arcade}|{CoralStone}|{LogHall}|{RuneHall}|{TunnelStall}|{ReedHall}|{ReefTower}|{RootStilt}|{StiltLonghouse}|{StoneTerrace}|{RingWallHall}|{SpireHouse}|{FeltYurt}|{GiantRoundhouse}|{HollowTrunk}|{PinnacleNest}|{GourdHut}|{DoorOffset:0.00}|{WideDoor}|{FrontWindows}|{SideWindows}|" +
            $"{Chimney}|{Sign}|{SignColor}|{Dressing}|{Seed}";
    }
}
