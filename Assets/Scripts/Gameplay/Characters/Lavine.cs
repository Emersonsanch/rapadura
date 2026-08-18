using Rapadura.Gameplay.Player;

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

        public override string Lore =>
            "Lavine nasceu durante um raro eclipse conhecido como Noite Rubra. Desde pequena demonstrou " +
            "uma ligação incomum com energias proibidas. Temida por muitos e incompreendida por outros, ela " +
            "luta constantemente contra a corrupção que tenta dominar sua alma. Apesar da aparência fria, " +
            "Lavine deseja provar que seu poder pode ser usado para proteger o mundo.";

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
            // TODO: substituir por bônus real de Poder Arcano/Mana Máxima quando o sistema de Atributos (Fase 3) existir.
            stats.RestoreMana(stats.MaxMana * 0.75f);
        }
    }
}
