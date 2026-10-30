using Rapadura.Gameplay.Player;
using UnityEngine;

namespace Rapadura.Gameplay.Characters
{
    /// <summary>Lavine — A Feiticeira das Sombras (elemento Ar Sereno; a sombra macia dela não é a Treva). Dano Mágico/Controle de Multidão.</summary>
    public class Lavine : PlayableCharacter
    {
        public override CharacterId Id => CharacterId.Lavine;
        public override string DisplayName => "Lavine";
        public override string ClassName => "Feiticeira";
        public override string Role => "Dano Mágico / Controle de Multidão";
        public override string PrimaryAttribute => "Poder Arcano";
        /// <summary>Roxo da Noite Rubra e do Ar Sereno (lilás e prata) que carrega.</summary>
        public override Color AccentColor => new Color(0.45f, 0.20f, 0.55f);
        public override bool HasDedicatedModel => true;
        public override int Age => 9;

        public override string Lore =>
            "Lavine, a mais nova de todos com 9 anos, nasceu durante um raro eclipse conhecido como Noite Rubra. Desde " +
            "bebê demonstrou uma ligação incomum com o Ar Sereno, o ar quieto e estrelado do céu da noite, e com uma sombra " +
            "macia que é só dela — algo grande demais pra uma criança carregar. Temida por muitos e incompreendida por " +
            "outros, ela vive provando que a sua sombra não é a Treva, que é fria, pegajosa e barulhenta. Apesar do jeito " +
            "sério e da aparência fria, no fundo ainda é só uma menina que deseja provar que seu poder pode ser usado para " +
            "proteger o mundo. É a mais nova de toda a família, prima de Maithe, Maria, Joaquim e Ícaro, e carrega o desejo " +
            "de ser levada tão a sério quanto os primos mais velhos.";

        public override string[] AbilityNames => new[]
        {
            "Orbe do Ar Sereno",
            "Correntes de Ar",
            "Névoa da Ilusão",
            "Explosão de Estrelas",
            "Céu da Noite Rubra" // Ultimate
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
