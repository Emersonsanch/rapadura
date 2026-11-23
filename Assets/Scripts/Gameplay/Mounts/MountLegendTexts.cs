using System.Collections.Generic;
using Rapadura.Core.Localization;

namespace Rapadura.Gameplay.Mounts
{
    /// <summary>
    /// Localization (en + pt-BR) of <see cref="MountLegendCatalog"/>: description, lore, favourite line, ability, toasts and the freeing line
    /// said where Apep falls. Merged into <see cref="MountTexts.Entries"/>; name and Caderno hint live in <see cref="MountCodex"/>.
    /// </summary>
    public static class MountLegendTexts
    {
        public const string ToastCrossing = "ui.mount.legend.crossing";
        public const string ToastCrossingDazzle = "ui.mount.legend.crossing_dazzle";

        public static readonly IReadOnlyList<LocalizationEntry> Entries = new[]
        {
            new LocalizationEntry("mount." + MountLegendCatalog.LightSkiff + ".desc",
                "A little skiff of the Barcas de Luz, of gold-white planks that glow from inside, with a prow like a raised hand. It floats a metre over sand and water alike, and rows itself with oars of light.",
                "Uma barquinha das Barcas de Luz, de tábuas branco-douradas que brilham por dentro, com a proa feito uma mão levantada. Flutua um metro acima da areia e da água do mesmo jeito, e rema sozinha com remos de luz."),
            new LocalizationEntry("mount." + MountLegendCatalog.LightSkiff + ".lore",
                "Every night Apep drinks the river so that the boat of the hours runs aground, and once she swallowed a little skiff that had strayed from the Barcas de Luz carrying the light back up. It stayed in her coils, waiting for the river to come back. When the cousins made her spit the river out, the skiff came out with it, very rarely, still glowing. Maithe was the first to climb in; she says it rows toward wherever Grandma Edna is.",
                "Toda noite Apep bebe o rio para a barca das horas encalhar, e uma vez ela engoliu uma barquinha que tinha se perdido das Barcas de Luz levando a luz de volta para cima. Ficou presa nas dobras dela, esperando o rio voltar. Quando os primos fizeram Apep cuspir o rio, a barquinha saiu junto, muito de vez em quando, ainda brilhando. A Maithe foi a primeira a subir; ela diz que a barquinha rema sempre para o lado onde a Vó Edna está."),
            new LocalizationEntry("mount." + MountLegendCatalog.LightSkiff + ".favorite_line", "Row us home, little one. Grandma's waiting.", "Rema a gente para casa, pequena. A Vó está esperando."),
            new LocalizationEntry("mount.ability.travessia_de_luz.name", "Crossing of Light", "Travessia de Luz"),
            new LocalizationEntry("mount.ability.travessia_de_luz.desc",
                "The skiff rows on its own light: for 8 s the cousin and the group within 10 m move 20% faster, the light opens 12 m around, and common critters in it lose track of the cousin for 2 s. Bosses are not dazzled. Cooldown 60 s.",
                "A barquinha rema na própria luz: por 8 s o primo e o grupo a até 10 m andam 20% mais rápido, a luz abre 12 m em volta, e os bichos comuns dentro dela perdem o primo de vista por 2 s. Chefe não se ofusca. Recarga 60 s."),
            new LocalizationEntry(ToastCrossing, "{0} rows on its light: light feet for everyone.", "{0} rema na própria luz: pé leve para todo mundo."),
            new LocalizationEntry(ToastCrossingDazzle, "{0} rows on its light: the critters lose sight of you.", "{0} rema na própria luz: os bichos perdem vocês de vista."),

            // The freeing scene (MountBossFreeing): said over the spot where Apep falls, when the skiff drops.
            new LocalizationEntry(MountBossRules.FreedLineKey(MountLegendCatalog.LightSkiff),
                "Apep spits the river out, and with it {0}, still glowing. Row it back to the light.",
                "Apep cospe o rio, e junto {0}, ainda brilhando. Remem com ela de volta para a luz."),
        };
    }
}
