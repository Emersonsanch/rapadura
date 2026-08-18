using Rapadura.Gameplay.Player;

namespace Rapadura.Gameplay.Characters
{
    /// <summary>
    /// Base class for every playable character in the roster (Joaquim, Maria, Maithe, Icaro,
    /// Lavine). Holds identity/lore data, class/role classification, the primary attribute
    /// that drives the character's scaling, and the list of ability names (Fase 4 wires these
    /// to real <see cref="Rapadura.Gameplay.Skills.SkillDefinition"/> assets). Concrete
    /// characters override <see cref="ApplyPassive"/> with their own starting-stat behavior.
    /// </summary>
    public abstract class PlayableCharacter
    {
        public abstract CharacterId Id { get; }
        public abstract string DisplayName { get; }
        public abstract string ClassName { get; }
        public abstract string Role { get; }
        public abstract string PrimaryAttribute { get; }
        public abstract string Lore { get; }

        /// <summary>Ability names in order: 4 active abilities followed by the Ultimate.</summary>
        public abstract string[] AbilityNames { get; }

        /// <summary>Applies this character's base-stat modifiers/passive to a freshly spawned instance.</summary>
        public abstract void ApplyPassive(PlayerStats stats);
    }
}
