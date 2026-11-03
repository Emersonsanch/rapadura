using System.Collections.Generic;
using System.Linq;
using Rapadura.Core.Localization;

namespace Rapadura.Gameplay.World.Cities
{
    /// <summary>
    /// Localization (en + pt-BR) of the city side quests (<see cref="CityQuestCatalog"/>): title, description, objectives
    /// and journal (<c>quest.city_*</c>, the same shape as the region quests) and the giver's offer/progress lines. Merged
    /// into the live manager by <see cref="EnsureLoaded"/> (keys it already has win), like <c>RegionQuestTexts</c>.
    /// </summary>
    public static class CityQuestTexts
    {
        public static readonly List<LocalizationEntry> All = new List<LocalizationEntry>
        {
            // ================= Copas de Garajonay — Gara, guardiã das copas =================
            new LocalizationEntry("quest.city_garajonay_dry_mist.title", "The Mist That Forgot the Water", "A Névoa Que Esqueceu a Água"),
            new LocalizationEntry("quest.city_garajonay_dry_mist.description",
                "Since the shadow drank the mist, the laurel lizards of the forest floor run wild and dry. Gara, the canopy warden of Copas de Garajonay, asked the cousins to calm them before they climb the trunks.",
                "Desde que a sombra bebeu a névoa, os lagartos do chão da mata de louro andam bravos e secos. Gara, a guardiã das copas de Garajonay, pediu aos primos que os acalmem antes que subam pelos troncos."),
            new LocalizationEntry("quest.city_garajonay_dry_mist.obj_calm_mist_lizards", "Calm the wild beasts of the laurel floor", "Acalmar os bichos bravos do chão da mata de louro"),
            new LocalizationEntry("quest.city_garajonay_dry_mist.journal",
                "Gara says the laurels drink the cloud and the cloud remembers everything. Grandpa Jorge's tales said the same about the rain that comes sideways. Five of us, one mist.",
                "A Gara diz que o louro bebe a nuvem e a nuvem lembra de tudo. Os contos do Vô Jorge falavam a mesma coisa da chuva que vem de lado. Somos cinco, a névoa é uma só."),
            new LocalizationEntry("quest.city_garajonay_dry_mist.offer",
                "Grandma Edna's grandchildren, at last! The mist forgot the water and the forest floor went wild. Calm six of the beasts down there and the bridges will sing for you.",
                "Os netos da Vó Edna, enfim! A névoa esqueceu a água e o chão da mata ficou bravo. Acalmem seis bichos lá embaixo e as pontes vão cantar para vocês."),
            new LocalizationEntry("quest.city_garajonay_dry_mist.active",
                "The mist is still dry down there. Walk in pairs on the trails: whoever falls from a bridge comes back the long way.",
                "A névoa ainda está seca lá embaixo. Andem em dupla pelas trilhas: quem cai da ponte volta pelo caminho comprido."),

            // ================= Copas de Garajonay — Tinguaro, o rastreador =================
            new LocalizationEntry("quest.city_garajonay_moss_tracks.title", "Tracks in the Moss", "Rastros no Musgo"),
            new LocalizationEntry("quest.city_garajonay_moss_tracks.description",
                "Moss boars dimmed by the shadow are digging up the roots of the giant laurels. Tinguaro, the elves' tracker, read their tracks and needs the cousins to chase them off.",
                "Javalis de musgo apagados pela sombra estão revirando as raízes dos louros gigantes. Tinguaro, o rastreador dos elfos, leu os rastros e precisa dos primos para espantá-los."),
            new LocalizationEntry("quest.city_garajonay_moss_tracks.obj_follow_moss_boars", "Chase off the moss boars", "Espantar javalis de musgo"),
            new LocalizationEntry("quest.city_garajonay_moss_tracks.journal",
                "Tinguaro reads moss the way Ícaro reads gears. He says a track is a letter the animal forgot to send. Maithe wants to learn it all.",
                "O Tinguaro lê musgo do jeito que o Ícaro lê engrenagem. Ele diz que rastro é carta que o bicho esqueceu de mandar. A Maithe quer aprender tudo."),
            new LocalizationEntry("quest.city_garajonay_moss_tracks.offer",
                "Gara told me you calmed the floor. Now the moss boars are digging at the laurel roots. Five of them, and I'll give you one of Jonay's bows.",
                "A Gara me contou que vocês acalmaram o chão. Agora os javalis de musgo estão cavando as raízes dos louros. Cinco deles, e eu te dou um dos arcos do Jonay."),
            new LocalizationEntry("quest.city_garajonay_moss_tracks.active",
                "Follow the turned moss, not the noise. A boar that is running is not where the noise is.",
                "Sigam o musgo revirado, não o barulho. Javali que corre não está onde o barulho está."),
        };

        public static void EnsureLoaded(LocalizationManager localization)
        {
            if (localization == null)
            {
                return;
            }

            localization.LoadEntries(All.Where(entry => !localization.HasKey(entry.Key)).ToList());
        }

        /// <summary>pt-BR text of <paramref name="key"/> without a manager, or null.</summary>
        public static string GetFallback(string key)
        {
            LocalizationEntry entry = All.FirstOrDefault(e => e.Key == key);
            return entry?.PtBR;
        }
    }
}
