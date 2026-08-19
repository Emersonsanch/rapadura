using Rapadura.Gameplay.Player;
using UnityEngine;

namespace Rapadura.Gameplay.Characters
{
    /// <summary>Maithe — A Arqueira dos Ventos. DPS à distância.</summary>
    public class Maithe : PlayableCharacter
    {
        public override CharacterId Id => CharacterId.Maithe;
        public override string DisplayName => "Maithe";
        public override string ClassName => "Caçadora";
        public override string Role => "DPS à distância";
        public override string PrimaryAttribute => "Destreza";

        public override string Lore =>
            "Maithe cresceu explorando montanhas e florestas proibidas. Conhecida por acertar alvos " +
            "impossíveis, tornou-se uma lenda entre os caçadores. Durante uma expedição encontrou um " +
            "artefato antigo chamado Olho do Vento, que lhe permitiu enxergar ameaças invisíveis. Agora " +
            "busca descobrir quem criou os monstros que ameaçam os reinos.";

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
