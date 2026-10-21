using Rapadura.Core.Events;
using Rapadura.Gameplay.Enemies;
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

    /// <summary>Raised the first time a region's boss challenge is won (<see cref="WorldMapProgress.MarkRegionCleared"/>).</summary>
    public readonly struct WorldMapRegionClearedEvent : IGameEvent
    {
        public readonly BiomeType Region;

        public WorldMapRegionClearedEvent(BiomeType region)
        {
            Region = region;
        }
    }

    /// <summary>
    /// Raised by <see cref="WorldMapController"/> whenever the "Enfrentar o chefe" offer for the
    /// region the token stands on may have changed (map load, arrival at a region, challenge started).
    /// <see cref="IsAvailable"/> mirrors <see cref="RegionBossProgress.CanChallengeBoss"/> for
    /// <see cref="Region"/>; HUDs can also just poll that API on refresh.
    /// </summary>
    public readonly struct RegionBossChallengeStateChangedEvent : IGameEvent
    {
        public readonly BiomeType Region;
        public readonly EnemyDefinition Boss;
        public readonly bool IsAvailable;

        public RegionBossChallengeStateChangedEvent(BiomeType region, EnemyDefinition boss, bool isAvailable)
        {
            Region = region;
            Boss = boss;
            IsAvailable = isAvailable;
        }
    }

    /// <summary>
    /// Raised by <see cref="EncounterArenaController"/> once an encounter's enemies are spawned: for
    /// <see cref="GraceSeconds"/> they hold their aggro (no chase/attacks) while the "Prepare-se!"
    /// countdown plays (<c>BossIntroBannerController</c>; boss encounters show the boss banner instead).
    /// </summary>
    public readonly struct EncounterIntroEvent : IGameEvent
    {
        public readonly EncounterPayload Payload;
        public readonly float GraceSeconds;
        public readonly bool HasBoss;

        public EncounterIntroEvent(EncounterPayload payload, float graceSeconds, bool hasBoss)
        {
            Payload = payload;
            GraceSeconds = graceSeconds;
            HasBoss = hasBoss;
        }
    }

    /// <summary>
    /// Raised by <see cref="EncounterArenaController"/> right after it spawns an encounter that has a
    /// boss (<see cref="EnemyDefinition.IsBoss"/>), so the boss banner can introduce it.
    /// </summary>
    public readonly struct BossEncounterIntroEvent : IGameEvent
    {
        public readonly EncounterPayload Payload;
        public readonly EnemyDefinition Boss;

        public BossEncounterIntroEvent(EncounterPayload payload, EnemyDefinition boss)
        {
            Payload = payload;
            Boss = boss;
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
