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
            $"{JoineryColor}|{AccentColor}|{PlinthColor}|{WoodenWalls}|{UpperRoom}|{StorkNest}|{DoorOffset:0.00}|{WideDoor}|{FrontWindows}|{SideWindows}|" +
            $"{Chimney}|{Sign}|{SignColor}|{Dressing}|{Seed}";
    }
}
