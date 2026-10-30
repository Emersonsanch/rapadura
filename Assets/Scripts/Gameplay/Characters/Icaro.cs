using Rapadura.Gameplay.Player;
using UnityEngine;

namespace Rapadura.Gameplay.Characters
{
    /// <summary>Ícaro — O Mestre das Máquinas. Controle/Dano Tecnológico.</summary>
    public class Icaro : PlayableCharacter
    {
        public override CharacterId Id => CharacterId.Icaro;
        public override string DisplayName => "Ícaro";
        public override string ClassName => "Inventor";
        public override string Role => "Controle / Dano Tecnológico";
        public override string PrimaryAttribute => "Inteligência";
        /// <summary>Cobre/laranja das engrenagens e faíscas de suas máquinas.</summary>
        public override Color AccentColor => new Color(0.85f, 0.48f, 0.15f);
        public override bool HasDedicatedModel => true;
        public override int Age => 11;

        public override string Lore =>
            "Com 11 anos, Ícaro já acredita que a tecnologia pode mudar o mundo. Enquanto as outras " +
            "crianças brincavam, ele desmontava e remontava tudo que encontrava. Depois de achar " +
            "fragmentos de uma civilização perdida num terreno baldio perto de casa, começou a montar " +
            "pequenos equipamentos capazes de surpreender até os magos adultos do sertão. Seu sonho é " +
            "desvendar os segredos da antiga tecnologia que pode salvar ou destruir o sertão. Irmão mais " +
            "novo de Maithe e primo mais velho de Lavine, vive tentando impressionar a irmã mais velha com " +
            "suas invenções.";

        public override string[] AbilityNames => new[]
        {
            "Torreta Automática",
            "Granada Elétrica",
            "Drone de Ataque",
            "Campo Magnético",
            "Exército Mecânico" // Ultimate
        };

        public override void ApplyPassive(PlayerStats stats)
        {
            // Inventor da tecnologia perdida: talento inato reforça Inteligência (seu Atributo
            // Principal), que aumenta a redução de recarga de habilidades (ver
            // AttributeSet.IntelligenceCooldownReductionPerPoint).
            AttributeSet attributeSet = stats.GetComponent<AttributeSet>();

            if (attributeSet != null)
            {
                attributeSet.ApplyCharacterPassiveBonus(AttributeType.Intelligence);
            }

            stats.RestoreMana(stats.MaxMana * 0.5f);
        }
    }
}
