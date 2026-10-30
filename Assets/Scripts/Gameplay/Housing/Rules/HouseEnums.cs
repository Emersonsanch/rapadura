using System;

// Unity-free on purpose (no UnityEngine): the house rules in this folder are meant to be reused as-is by
// the online server (Housing.md §4.7, phase H8).
namespace Rapadura.Gameplay.Housing
{
    /// <summary>The 18 sections of <c>Docs/Housing/CatalogoCasa.md</c> (the <c>casa_&lt;category&gt;_nnn</c> id prefix).</summary>
    public enum HouseItemCategory
    {
        LivingRoom,   // sala
        Bedroom,      // quarto
        Kitchen,      // cozinha
        Bathroom,     // banheiro
        Yard,         // quintal
        Corral,       // curral
        Lighting,     // luz
        WallDecor,    // parede
        Rug,          // tapete
        Curtain,      // cortina
        Plant,        // planta
        Music,        // musica
        Toy,          // brinquedo
        Regional,     // regional
        Trophy,       // trofeu
        Seasonal,     // sazonal
        Family,       // familia
        Utility,      // util
    }

    /// <summary>Where an item may be placed (Housing.md §2.4). An item may allow more than one layer.</summary>
    [Flags]
    public enum HousePlacement
    {
        None = 0,
        Floor = 1 << 0,
        Wall = 1 << 1,
        Ceiling = 1 << 2,
        Surface = 1 << 3,
        Yard = 1 << 4,
        /// <summary>Rugs: the floor's own sub-layer, under the furniture.</summary>
        Rug = 1 << 5,
    }

    /// <summary>The one layer a placed instance lives on.</summary>
    public enum HouseLayer
    {
        Floor,
        Rug,
        Wall,
        Ceiling,
        Surface,
        Yard,
    }

    /// <summary>The four walls of a room, seen from above with +y (world +z) to the north. The entrance is on the south.</summary>
    public enum HouseWallSide
    {
        South,
        East,
        North,
        West,
    }

    /// <summary>What a placed piece of furniture does (Housing.md §5, §12.1).</summary>
    public enum HouseItemFunction
    {
        None,
        Chest,
        Wardrobe,
        Bench,
        Stove,
        Forge,
        Loom,
        Alchemy,
        DyeTub,
        Bed,
        PetBed,
        TrophyStand,
        RelicCase,
        BestiaryShelf,
        GuestBook,
        Mailbox,
        TrainingDummy,
        Well,
        Planter,
        Helper,
        Bell,
        NoticeBoard,
        /// <summary>"Baú-correio da Vó Edna": opens the village chest from home (not in the doc's list; §5).</summary>
        VillageChestLink,
    }

    public enum HouseItemSourceType
    {
        Shop,
        Drop,
        Quest,
        HouseQuest,
        DailyQuest,
        Achievement,
        Event,
        Craft,
        Secret,
    }

    public enum HouseSeason
    {
        None,
        Junina,
        Natal,
        Carnaval,
    }

    /// <summary>Outcome of a placement check (<see cref="HousePlacementRules"/>).</summary>
    public enum HousePlacementResult
    {
        Ok,
        UnknownItem,
        NotInDepot,
        UnknownInstance,
        WrongLayer,
        LevelTooLow,
        OutOfBounds,
        Overlaps,
        OverOpening,
        CurtainNeedsWindow,
        BlocksDoor,
        NeedsFreeFront,
        BlocksAccess,
        NoSurfaceParent,
        SurfaceSlotTaken,
        RotationNotAllowed,
        InteriorLimitReached,
        YardLimitReached,
        FixedDecoration,
        TooManyExtraChests,
        DepotFull,
    }
}
