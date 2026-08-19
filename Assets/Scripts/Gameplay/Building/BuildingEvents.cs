using Rapadura.Core.EventBus;

namespace Rapadura.Gameplay.Building
{
    /// <summary>Raised when a structure is successfully placed in the world.</summary>
    public readonly struct StructurePlacedEvent : IGameEvent
    {
        public readonly PlacedStructure Structure;

        public StructurePlacedEvent(PlacedStructure structure)
        {
            Structure = structure;
        }
    }

    /// <summary>Raised when a placed structure is upgraded to a new level.</summary>
    public readonly struct StructureUpgradedEvent : IGameEvent
    {
        public readonly PlacedStructure Structure;
        public readonly int NewLevel;

        public StructureUpgradedEvent(PlacedStructure structure, int newLevel)
        {
            Structure = structure;
            NewLevel = newLevel;
        }
    }

    /// <summary>Raised when a placed structure is removed/demolished.</summary>
    public readonly struct StructureRemovedEvent : IGameEvent
    {
        public readonly PlacedStructure Structure;

        public StructureRemovedEvent(PlacedStructure structure)
        {
            Structure = structure;
        }
    }
}
