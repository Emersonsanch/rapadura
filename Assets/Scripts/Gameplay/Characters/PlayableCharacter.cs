using Rapadura.Gameplay.Player;
using UnityEngine;

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

        /// <summary>
        /// Idade do personagem. Decisão de design de 2026-08-24: o roster foi revisado de adultos
        /// para crianças/pré-adolescentes heroicos (mantendo a ordem de nascimento já estabelecida:
        /// Maithe &gt; Maria &gt; Joaquim &gt; Ícaro &gt; Lavine). Afeta geração de arte 3D e deve
        /// eventualmente refletir em GDD/quests/diálogos (ver TODO.md — reescrita ainda pendente
        /// pra esse conteúdo mais extenso).
        /// </summary>
        public abstract int Age { get; }

        /// <summary>Ability names in order: 4 active abilities followed by the Ultimate.</summary>
        public abstract string[] AbilityNames { get; }

        /// <summary>
        /// Signature color for this character, used to tint the (currently shared, placeholder)
        /// player visual on selection so each hero reads as visually distinct in-game before real
        /// per-character models/materials exist (see Fase 11 — Arte). Also usable later for UI
        /// theming (roster highlight, health bar accent, etc.).
        /// </summary>
        public abstract Color AccentColor { get; }

        /// <summary>
        /// True once this character has a real generated/authored 3D model assigned as the
        /// player visual (see Fase 11 — Arte no TODO), instead of sharing the placeholder capsule.
        /// <see cref="CharacterSelectController"/> uses this to skip the flat <see cref="AccentColor"/>
        /// tint for characters with a real texture — tinting would wash it out. Remove this flag
        /// entirely once every character has a dedicated model and the placeholder is retired.
        /// </summary>
        public virtual bool HasDedicatedModel => false;

        /// <summary>Applies this character's base-stat modifiers/passive to a freshly spawned instance.</summary>
        public abstract void ApplyPassive(PlayerStats stats);
    }
}
