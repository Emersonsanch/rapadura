using Rapadura.Gameplay.Player;
using UnityEngine;

namespace Rapadura.Gameplay.Characters
{
    /// <summary>Joaquim — O Guardião das Raízes. Tanque/Defesa, protetor do grupo.</summary>
    public class Joaquim : PlayableCharacter
    {
        public override CharacterId Id => CharacterId.Joaquim;
        public override string DisplayName => "Joaquim";
        public override string ClassName => "Guardião";
        public override string Role => "Tanque / Defesa / Proteção do grupo";
        public override string PrimaryAttribute => "Vitalidade";

        public override string Lore =>
            "Joaquim nasceu em uma pequena vila cercada por florestas antigas. Desde criança aprendeu a " +
            "proteger sua comunidade dos perigos que surgiam durante a noite. Após a destruição da vila " +
            "por criaturas sombrias vindas das profundezas do mundo de Rapadura, ele jurou proteger todos " +
            "os inocentes. Carrega consigo o Escudo das Raízes Eternas, um artefato herdado de seus " +
            "ancestrais que absorve parte do dano sofrido pelos aliados.";

        public override string[] AbilityNames => new[]
        {
            "Muralha de Pedra",
            "Provocação Heroica",
            "Escudo das Raízes",
            "Terremoto",
            "Última Resistência" // Ultimate
        };

        public override void ApplyPassive(PlayerStats stats)
        {
            // Guardião das Raízes: talento inato reforça Vitalidade (seu Atributo Principal), que por
            // sua vez aumenta a vida máxima (ver AttributeSet.VitalityHealthPerPoint). Também cura o
            // personagem ao spawnar, já que sua vida máxima acabou de subir.
            AttributeSet attributeSet = stats.GetComponent<AttributeSet>();

            if (attributeSet != null)
            {
                attributeSet.ApplyCharacterPassiveBonus(AttributeType.Vitality);
            }

            stats.Heal(stats.MaxHealth);
        }
    }
}
