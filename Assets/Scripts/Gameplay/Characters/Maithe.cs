using Rapadura.Gameplay.Player;
using UnityEngine;

namespace Rapadura.Gameplay.Characters
{
    /// <summary>Maithe — A Arqueira dos Ventos (elemento Vento Vivo). DPS à distância.</summary>
    public class Maithe : PlayableCharacter
    {
        public override CharacterId Id => CharacterId.Maithe;
        public override string DisplayName => "Maithe";
        public override string ClassName => "Caçadora";
        public override string Role => "DPS à distância";
        public override string PrimaryAttribute => "Destreza";
        /// <summary>Verde-floresta das montanhas onde cresceu, com o Olho do Vento em mente.</summary>
        public override Color AccentColor => new Color(0.30f, 0.55f, 0.32f);
        public override int Age => 14;

        public override string Lore =>
            "Aos 14 anos, Maithe já passa mais tempo explorando montanhas e florestas proibidas do que em " +
            "casa. Precoce com o arco, acerta alvos que caçadores adultos errariam, e já é conhecida na " +
            "vizinhança por isso. Numa de suas escapadas encontrou um artefato antigo chamado Olho do " +
            "Vento, que lhe permitiu enxergar ameaças invisíveis. Agora busca descobrir quem criou os " +
            "monstros que ameaçam os reinos. É a mais velha entre os cinco primos e irmã mais velha de " +
            "Ícaro, o que a torna a referência natural de toda a família quando o assunto é enfrentar o " +
            "perigo de frente — mesmo sendo, ela também, só uma criança.";

        public override string[] AbilityNames => new[]
        {
            "Flecha Precisa",
            "Chuva de Flechas",
            "Disparo Explosivo",
            "Passo do Vento",
            "Tempestade de Mil Flechas" // Ultimate
        };

        public override void ApplyPassive(PlayerStats stats)
        {
            // Caçadora do Olho do Vento: talento inato reforça Destreza (seu Atributo Principal), que
            // aumenta a chance de crítico (ver AttributeSet.DexterityCriticalChancePerPoint). Também
            // enche a stamina ao spawnar.
            AttributeSet attributeSet = stats.GetComponent<AttributeSet>();

            if (attributeSet != null)
            {
                attributeSet.ApplyCharacterPassiveBonus(AttributeType.Dexterity);
            }

            stats.RestoreStamina(stats.MaxStamina);
        }
    }
}
