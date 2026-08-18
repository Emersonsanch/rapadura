using Rapadura.Gameplay.Player;

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

        public override string Lore =>
            "Ícaro sempre acreditou que a tecnologia poderia mudar o mundo. Enquanto outros estudavam " +
            "magia, ele construía máquinas. Após encontrar fragmentos de uma civilização perdida, " +
            "desenvolveu equipamentos capazes de rivalizar com os maiores magos do continente. Seu objetivo " +
            "é desvendar os segredos da antiga tecnologia que pode salvar ou destruir Rapadura.";

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
            // TODO: substituir por bônus real de Inteligência/Mana Máxima quando o sistema de Atributos (Fase 3) existir.
            stats.RestoreMana(stats.MaxMana * 0.5f);
        }
    }
}
