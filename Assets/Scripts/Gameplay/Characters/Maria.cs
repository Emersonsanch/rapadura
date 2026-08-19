using Rapadura.Gameplay.Player;
using UnityEngine;

namespace Rapadura.Gameplay.Characters
{
    /// <summary>Maria — A Curandeira da Luz. Suporte/Cura/Buffs.</summary>
    public class Maria : PlayableCharacter
    {
        public override CharacterId Id => CharacterId.Maria;
        public override string DisplayName => "Maria";
        public override string ClassName => "Sacerdotisa";
        public override string Role => "Suporte / Cura / Buffs";
        public override string PrimaryAttribute => "Espírito";

        public override string Lore =>
            "Maria foi criada dentro do Templo Solar, onde aprendeu os segredos da energia vital. Quando " +
            "os cristais sagrados começaram a perder seu brilho, ela recebeu uma visão mostrando que apenas " +
            "um grupo de heróis poderia restaurar o equilíbrio do mundo. Ela parte em uma jornada para " +
            "reunir os escolhidos e impedir que a escuridão consuma Rapadura.";

        public override string[] AbilityNames => new[]
        {
            "Cura Divina",
            "Benção da Vida",
            "Escudo Sagrado",
            "Purificação",
            "Milagre Celestial" // Ultimate
        };

        public override void ApplyPassive(PlayerStats stats)
        {
            // Sacerdotisa do Templo Solar: talento inato reforça Espírito (seu Atributo Principal),
            // que aumenta a mana máxima (ver AttributeSet.SpiritManaPerPoint). Também enche a mana
            // ao spawnar, já que a mana máxima acabou de subir.
            AttributeSet attributeSet = stats.GetComponent<AttributeSet>();

            if (attributeSet != null)
            {
                attributeSet.ApplyCharacterPassiveBonus(AttributeType.Spirit);
            }

            stats.RestoreMana(stats.MaxMana);
        }
    }
}
