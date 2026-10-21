using Rapadura.Core.Events;
using Rapadura.Gameplay.World.Biomes;

namespace Rapadura.Gameplay.World.Map
{
    /// <summary>Raised when the player's token settles on a different world map region.</summary>
    public readonly struct WorldMapRegionChangedEvent : IGameEvent
    {
        public readonly BiomeType PreviousRegion;
        public readonly BiomeType CurrentRegion;

        public WorldMapRegionChangedEvent(BiomeType previousRegion, BiomeType currentRegion)
        {
            PreviousRegion = previousRegion;
            CurrentRegion = currentRegion;
        }
    }

    /// <summary>Raised the first time a world map region becomes travelable.</summary>
    public readonly struct WorldMapRegionUnlockedEvent : IGameEvent
    {
        public readonly BiomeType Region;

        public WorldMapRegionUnlockedEvent(BiomeType region)
        {
            Region = region;
        }
    }

    /// <summary>
    /// Raised on the world map the instant <see cref="RandomEncounterSystem"/> decides an encounter
    /// happens, before any scene transition starts (hook for a "!"/screen-flash presentation).
    /// </summary>
    public readonly struct EncounterTriggeredEvent : IGameEvent
    {
        public readonly EncounterPayload Payload;

        public EncounterTriggeredEvent(EncounterPayload payload)
        {
            Payload = payload;
        }
    }

    /// <summary>Raised by <see cref="SceneTransitionService"/> when it starts loading the combat scene for an encounter.</summary>
    public readonly struct EncounterStartedEvent : IGameEvent
    {
        public readonly EncounterPayload Payload;

        public EncounterStartedEvent(EncounterPayload payload)
        {
            Payload = payload;
        }
    }

    /// <summary>Raised by <see cref="SceneTransitionService"/> when an encounter resolves, right before returning to the world map.</summary>
    public readonly struct EncounterEndedEvent : IGameEvent
    {
        public readonly EncounterPayload Payload;
        public readonly EncounterOutcome Outcome;

        public EncounterEndedEvent(EncounterPayload payload, EncounterOutcome outcome)
        {
            Payload = payload;
            Outcome = outcome;
        }
    }
}
