using System.Collections.Generic;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Characters;

namespace Rapadura.Gameplay.Mounts
{
    /// <summary>
    /// Localization (en + pt-BR) of <see cref="MountReefCatalog"/>: description, lore, ability, toast, the Apito de Concha sold at Danel's
    /// and the five cousins' lines when the Hipocampo first comes home (<see cref="MountReefRules.ArrivalLineKey"/>). Merged into
    /// <see cref="MountTexts.Entries"/>; name and Caderno hint live in <see cref="MountCodex"/>.
    /// </summary>
    public static class MountReefTexts
    {
        public const string ToastSong = "ui.mount.reef.song";

        // cousin, en, pt (each one remembers Grandma Edna or Grandpa Jorge)
        private static readonly (CharacterId Cousin, string En, string Pt)[] Arrivals =
        {
            (CharacterId.Joaquim,
                "Grandpa Jorge used to say the sea only listens to whoever whistles right. I'll practise until the conch answers first time.",
                "O Vô Jorge dizia que o mar só obedece quem assobia certo. Vou treinar até a concha responder de primeira."),
            (CharacterId.Maria,
                "A horse with a fish's tail! Grandma Edna will say I made it up. I'll have to bring her to see it.",
                "Um cavalo com rabo de peixe! A Vó Edna vai dizer que eu inventei. Vou ter que trazer ela para ver."),
            (CharacterId.Maithe,
                "He heard the conch all the way from the bottom. Grandma Edna called us in from the yard like that, without even raising her voice.",
                "Ele ouviu a concha lá do fundo. A Vó Edna chamava a gente do quintal assim, sem nem levantar a voz."),
            (CharacterId.Icaro,
                "Maithe, look: he swims and he walks! Grandpa Jorge would want to know how many fish he catches at once.",
                "Maithe, olha: ele nada e anda! O Vô Jorge ia querer saber quanto peixe ele pega de uma vez."),
            (CharacterId.Lavine,
                "Grandpa Jorge kept a conch on the shelf and said the whole sea was inside it. I think it was this one calling.",
                "O Vô Jorge guardava uma concha na estante e dizia que o mar inteiro morava dentro dela. Acho que era esta que chamava."),
        };

        public static readonly IReadOnlyList<LocalizationEntry> Entries = Build();

        private static List<LocalizationEntry> Build()
        {
            string id = MountReefCatalog.KirtaHippocamp;
            string whistle = MountReefCatalog.ConchWhistle;
            var entries = new List<LocalizationEntry>
            {
                new LocalizationEntry("mount." + id + ".desc",
                    "A sea horse of Kirta's reef, with a horse's chest and a fish's tail, coral fins for a mane. It walks the dry atoll and swims the reef, and when the conch sings the corals round it glow.",
                    "Um cavalo-marinho do recife de Kirta, peito de cavalo e rabo de peixe, barbatanas de coral no lugar da crina. Anda no atol seco e nada no recife, e quando a concha canta os corais em volta acendem."),
                new LocalizationEntry("mount." + id + ".lore",
                    "The povo-do-mar say the hippocamps followed King Kirta's boats home the night his family was lost at sea, and stayed to keep him company. They answer only a conch from the reef. Danel carves the whistles himself and judges who may buy one: so far he has never said no to a cousin.",
                    "O povo-do-mar diz que os hipocampos seguiram os barcos do rei Kirta até em casa na noite em que ele perdeu a família no mar, e ficaram para fazer companhia. Só atendem uma concha do recife. O Danel entalha os apitos ele mesmo e julga quem pode comprar: até hoje nunca disse não a um primo."),
                new LocalizationEntry("mount.ability." + MountReefRules.ReefSongId + ".name", "Reef Song", "Canto do Recife"),
                new LocalizationEntry("mount.ability." + MountReefRules.ReefSongId + ".desc",
                    "The conch sings: for 6 s a coral light opens 10 m around, and whoever of the group stands within 6 m heals 1.5% of max life per second. Cooldown 40 s.",
                    "A concha canta: por 6 s uma luz de coral abre 10 m em volta, e quem do grupo está a até 6 m cura 1,5% da vida máxima por segundo. Recarga 40 s."),
                new LocalizationEntry(ToastSong, "{0} blows the conch: the reef lights up and the wounds close.", "{0} sopra a concha: o recife acende em volta e os machucados fecham."),
                new LocalizationEntry(Content.ContentText.ItemNameKey(whistle), "Conch Whistle", "Apito de Concha"),
                new LocalizationEntry(Content.ContentText.ItemDescriptionKey(whistle),
                    "A conch of Kirta's reef, carved by Danel to whistle. Whoever buys it takes the Kirta Hippocamp home.",
                    "Concha do recife de Kirta, furada pelo Danel para apitar. Quem compra leva o Hipocampo de Kirta para casa."),
            };

            foreach (var a in Arrivals)
            {
                entries.Add(new LocalizationEntry(MountReefRules.ArrivalLineKey(a.Cousin), a.En, a.Pt));
            }

            return entries;
        }
    }
}
