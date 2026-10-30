using Rapadura.Gameplay.Player;
using UnityEngine;

namespace Rapadura.Gameplay.Characters
{
    /// <summary>Ícaro — O Mestre das Máquinas (elemento Fogo Vivo; remonta os Engenhos da Luz). Controle/Dano Tecnológico.</summary>
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
            "Com 11 anos, Ícaro já acredita que a tecnologia pode mudar o mundo. Enquanto as outras crianças brincavam, " +
            "ele desmontava e remontava tudo que encontrava. Depois de achar peças dos Engenhos da Luz, as máquinas antigas " +
            "dos Artífices, num terreno baldio perto de casa, começou a montar pequenos equipamentos que respondem quando " +
            "ele chama, capazes de surpreender até os magos adultos da ilha. Sonha em ser herdeiro de Kurdalægon, o " +
            "ferreiro do céu dos contos, e desvendar os segredos da tecnologia antiga que pode acender de novo a Ilha da " +
            "Aurora. Irmão mais novo de Maithe e primo mais velho de Lavine, vive tentando impressionar a irmã mais velha " +
            "com suas invenções.";

        public override string[] AbilityNames => new[]
        {
            "Torreta Ushabti",
            "Granada de Faísca",
            "Drone Senmurv",
            "Campo do Engenho",
            "Exército de Ushabtis" // Ultimate
        };

        public override void ApplyPassive(PlayerStats stats)
        {
            // Inventor dos Engenhos da Luz: talento inato reforça Inteligência (seu Atributo
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
