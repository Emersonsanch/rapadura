namespace Rapadura.Gameplay.Mounts
{
    /// <summary>What the mount HUD needs from a rider (the arena's <see cref="MountController"/>, the map's <see cref="WorldMapMountRider"/>).</summary>
    public interface IMountRider
    {
        MountSession Session { get; }
        MountDefinition Definition { get; }
        OwnedMount ActiveMount { get; }
        MountPlace Place { get; }
        string KeyHint { get; }

        /// <summary>Held by the HUD's mount button.</summary>
        bool TouchMountHeld { get; set; }

        /// <summary>One-shot from the HUD's ability button.</summary>
        bool TouchAbilityRequested { get; set; }

        /// <summary>Game (arena) or unscaled (map) clock the session's cooldowns run on.</summary>
        float Now { get; }
    }
}
