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
        /// <summary>Luz dourada do Templo Solar — branco quente com toque de ouro.</summary>
        public override Color AccentColor => new Color(0.95f, 0.85f, 0.55f);
        public override bool HasDedicatedModel => true;
        public override int Age => 13;

        public override string Lore =>
            "Maria, de 13 anos, foi criada dentro do Templo Solar, onde as sacerdotisas mais velhas lhe " +
            "ensinaram os primeiros segredos da energia vital. Quando os cristais sagrados começaram a " +
            "perder seu brilho, ela recebeu uma visão mostrando que apenas um grupo de heróis poderia " +
            "restaurar o equilíbrio do mundo. Corajosa mesmo sendo tão nova, parte em uma jornada para " +
            "reunir os escolhidos e impedir que a escuridão consuma Rapadura — e os escolhidos são, antes " +
            "de tudo, sua própria família: sua irmã mais velha Maithe, seu irmão mais novo Joaquim e seus " +
            "primos Ícaro e Lavine.";

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
