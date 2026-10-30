using Rapadura.Gameplay.Player;
using UnityEngine;

namespace Rapadura.Gameplay.Characters
{
    /// <summary>Maria — A Curandeira da Luz. Suporte/Cura/Buffs.</summary>
    public class Maria : PlayableCharacter
    {
        public override CharacterId Id => CharacterId.Maria;
        public override string DisplayName => "Maria";
        public override string ClassName => "Curandeira";
        public override string Role => "Suporte / Cura / Buffs";
        public override string PrimaryAttribute => "Espírito";
        /// <summary>Luz dourada do Templo Solar (elemento Luz) — branco quente com toque de ouro.</summary>
        public override Color AccentColor => new Color(0.95f, 0.85f, 0.55f);
        public override bool HasDedicatedModel => true;
        public override int Age => 13;

        public override string Lore =>
            "Maria, de 13 anos, cresceu entre as salas de luz do Templo Solar, onde as curandeiras mais velhas lhe " +
            "ensinaram os primeiros segredos da energia vital. Quando o Cristal do Sol apagou e a luz se espalhou em mil " +
            "faíscas, ela sonhou com um chamado: só um grupo de heróis poderia acender de novo a Ilha da Aurora. Corajosa " +
            "mesmo sendo tão nova, respondeu ao Chamado e partiu para reunir quem precisava vir — e quem precisava vir era, " +
            "antes de tudo, sua própria família: sua prima mais velha Maithe, seu irmão mais novo Joaquim e seus primos " +
            "Ícaro e Lavine.";

        public override string[] AbilityNames => new[]
        {
            "Luz que Cura",
            "Brilho da Vida",
            "Escudo Solar",
            "Purificação",
            "Coluna de Luz" // Ultimate
        };

        public override void ApplyPassive(PlayerStats stats)
        {
            // Curandeira do Templo Solar: talento inato reforça Espírito (seu Atributo Principal),
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
