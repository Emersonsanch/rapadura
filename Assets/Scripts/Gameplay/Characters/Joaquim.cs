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
        /// <summary>Aço/pedra do Escudo das Raízes Eternas — tom terroso e resistente, condizente com um tanque.</summary>
        public override Color AccentColor => new Color(0.34f, 0.42f, 0.32f);
        /// <summary>Modelo v1 gerado 2026-08-24 (Assets/Models/Characters/Joaquim/Joaquim.fbx) era adulto — pendente regenerar com proporções infantis após a revisão de idade abaixo.</summary>
        public override bool HasDedicatedModel => true;
        public override int Age => 12;

        public override string Lore =>
            "Joaquim tem apenas 12 anos, mas cresceu ouvindo os anciãos de sua vila, cercada por florestas " +
            "antigas, contarem como proteger os outros dos perigos que surgiam durante a noite — e decidiu " +
            "que seria ele a fazer isso um dia. Quando criaturas sombrias vindas das profundezas do " +
            "sertão destruíram a vila velha onde ele nasceu, a família foi morar perto da Vó Zefa, e o garoto " +
            "jurou, com toda a teimosia de uma criança, proteger todos os inocentes. Carrega consigo o Escudo das Raízes Eternas, um artefato herdado de seus " +
            "ancestrais — grande demais pra ele, mas que absorve parte do dano sofrido pelos aliados. Irmão " +
            "mais novo de Maria e primo de Maithe, Ícaro e Lavine, é o mais velho entre os meninos da " +
            "família e insiste em bancar o protetor também dos primos mais novos, Ícaro e Lavine, mesmo " +
            "quando ninguém pediu.";

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
