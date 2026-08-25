using Rapadura.Gameplay.Player;
using UnityEngine;

namespace Rapadura.Gameplay.Characters
{
    /// <summary>Lavine — A Feiticeira das Sombras. Dano Mágico/Controle de Multidão.</summary>
    public class Lavine : PlayableCharacter
    {
        public override CharacterId Id => CharacterId.Lavine;
        public override string DisplayName => "Lavine";
        public override string ClassName => "Feiticeira";
        public override string Role => "Dano Mágico / Controle de Multidão";
        public override string PrimaryAttribute => "Poder Arcano";
        /// <summary>Roxo sombrio da Noite Rubra e das energias proibidas que carrega.</summary>
        public override Color AccentColor => new Color(0.45f, 0.20f, 0.55f);
        public override bool HasDedicatedModel => true;
        public override int Age => 9;

        public override string Lore =>
            "Lavine, a mais nova de todos com 9 anos, nasceu durante um raro eclipse conhecido como Noite " +
            "Rubra. Desde bebê demonstrou uma ligação incomum com energias proibidas — algo grande demais " +
            "pra uma criança carregar. Temida por muitos e incompreendida por outros, ela luta " +
            "constantemente contra a corrupção que tenta dominar sua alma. Apesar do jeito sério e da " +
            "aparência fria, no fundo ainda é só uma menina que deseja provar que seu poder pode ser usado " +
            "para proteger o mundo. É a mais nova de toda a família, prima de Maithe, Maria, Joaquim e " +
            "Ícaro, e carrega o desejo de ser levada tão a sério quanto os primos mais velhos.";

        public override string[] AbilityNames => new[]
        {
            "Orbe Sombria",
            "Correntes das Trevas",
            "Névoa da Ilusão",
            "Explosão Arcana",
            "Eclipse Eterno" // Ultimate
        };

        public override void ApplyPassive(PlayerStats stats)
        {
            // Feiticeira da Noite Rubra: talento inato reforça Poder Arcano (seu Atributo Principal),
            // que aumenta o dano de ataque (ver AttributeSet.ArcanePowerAttackDamagePerPoint).
            AttributeSet attributeSet = stats.GetComponent<AttributeSet>();

            if (attributeSet != null)
            {
                attributeSet.ApplyCharacterPassiveBonus(AttributeType.ArcanePower);
            }

            stats.RestoreMana(stats.MaxMana * 0.75f);
        }
    }
}
