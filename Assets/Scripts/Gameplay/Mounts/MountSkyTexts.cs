using System.Collections.Generic;
using Rapadura.Core.Localization;

namespace Rapadura.Gameplay.Mounts
{
    /// <summary>
    /// Localization (en + pt-BR) of the flying mounts (<see cref="MountSkyCatalog"/>): description, lore, favourite
    /// line, the "Voo da Coluna" ability and toast, and the "Asas da Coluna" quest (<see cref="ColumnGriffinQuest"/>).
    /// Merged into <see cref="MountTexts.Entries"/>; the name and the Caderno hint live in <see cref="MountCodex"/>.
    /// </summary>
    public static class MountSkyTexts
    {
        public const string ToastFlight = "ui.mount.sky.column_flight";

        public const string QuestTitle = "quest.mount_column_griffin.title";
        public const string QuestDescription = "quest.mount_column_griffin.description";
        public const string QuestObjective = "quest.mount_column_griffin.objective.free_sparks";

        private const string Id = MountSkyCatalog.ColumnGriffin;
        private const string Ability = MountSkyCatalog.ColumnFlight;

        public static readonly IReadOnlyList<LocalizationEntry> Entries = new List<LocalizationEntry>
        {
            new LocalizationEntry("mount." + Id + ".desc",
                "A griffin of light: lion body, eagle head, wings tipped with sparks. It flies a little above the ground, so the cousins cross where only wings go (Mahaway's Roost, Fort of the Winds).",
                "Um grifo de luz: corpo de leão, cabeça de águia, asas com pontas de faísca. Voa um pouco acima do chão, e os primos passam onde só asa passa (Ninho de Mahaway, Forte dos Ventos)."),
            new LocalizationEntry("mount." + Id + ".lore",
                "The griffins guard the Column of Glory, the path the freed sparks climb home. This one was put out and fell into the Threshold Caves. Grandpa Jorge used to say that when enough light goes up, someone always comes down to help.",
                "Os grifos guardam a Coluna da Glória, o caminho por onde as faíscas libertadas sobem para casa. Este foi apagado e caiu nas Grutas do Limiar. O Vô Jorge dizia que, quando sobe luz que chega, sempre desce alguém para ajudar."),
            new LocalizationEntry("mount." + Id + ".favorite_line",
                "Up, up! The air up here is the calmest there is.",
                "Sobe, sobe! O ar lá em cima é o mais sereno que tem."),
            new LocalizationEntry("mount.ability." + Ability + ".name", "Column Flight", "Voo da Coluna"),
            new LocalizationEntry("mount.ability." + Ability + ".desc",
                "The griffin climbs and flies 10 m ahead over the critters, then lands in a flash of light: common critters within 4 m run away for 2.5 s. Walls still stop it. Cooldown 25 s.",
                "O grifo sobe e voa 10 m para a frente por cima dos bichos e pousa num clarão: os bichos comuns a até 4 m fogem por 2,5 s. Parede ainda segura. Recarga 25 s."),
            new LocalizationEntry(ToastFlight, "{0} climbs the light and lands in a flash!", "{0} sobe pela luz e pousa num clarão!"),

            new LocalizationEntry(QuestTitle, "Wings of the Column", "Asas da Coluna"),
            new LocalizationEntry(QuestDescription,
                "Srosh heard wings in the Threshold Caves: a griffin of the Column of Glory fell there, put out. Every critter you beat frees a spark that climbs the Column. With enough light, it will remember the way and come down to you.",
                "O Srosh ouviu asas nas Grutas do Limiar: um grifo da Coluna da Glória caiu lá, apagado. Cada bicho vencido solta uma faísca que sobe pela Coluna. Com luz que chega, ele lembra o caminho e desce até vocês."),
            new LocalizationEntry(QuestObjective, "Free sparks (beat critters)", "Libertar faíscas (vencer bichos)"),
        };
    }
}
