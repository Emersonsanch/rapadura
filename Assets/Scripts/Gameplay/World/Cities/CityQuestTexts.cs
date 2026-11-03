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

            // ================= Taa Pwani — Capitão Liyongo, o nakhoda =================
            new LocalizationEntry("quest.city_taa_pwani_thief_gulls.title", "Thieves of the Sails", "Ladras de Vela"),
            new LocalizationEntry("quest.city_taa_pwani_thief_gulls.description",
                "Gulls dimmed by the shadow swoop on the dhows of Taa Pwani, tear the triangular sails and steal the rope. Captain Liyongo, the nakhoda of the harbour, cannot sail for the reef until they are driven off.",
                "Gaivotas apagadas pela sombra mergulham nos dhows de Taa Pwani, rasgam as velas triangulares e roubam a corda. O Capitão Liyongo, o nakhoda do porto, não consegue sair para o recife enquanto elas não forem espantadas."),
            new LocalizationEntry("quest.city_taa_pwani_thief_gulls.obj_drive_off_thief_gulls", "Drive off the thief gulls", "Espantar as gaivotas ladras"),
            new LocalizationEntry("quest.city_taa_pwani_thief_gulls.journal",
                "Captain Liyongo says the monsoon pushes one way half the year and back the other half. Grandpa Jorge sailed with these people once: Mbaraka still remembers his off-key songs on deck.",
                "O Capitão Liyongo diz que a monção empurra para um lado meio ano e volta no outro meio. O Vô Jorge já navegou com esse povo: o Mbaraka ainda lembra das cantorias desafinadas dele no convés."),
            new LocalizationEntry("quest.city_taa_pwani_thief_gulls.offer",
                "Grandma Edna's five, on my quay! Karibu. The gulls went grey and mean, they tear my sails faster than we can sew. Drive six of them off and the first wind of the monsoon is yours.",
                "Os cinco da Vó Edna, no meu cais! Karibu. As gaivotas ficaram cinzentas e malvadas, rasgam as velas mais rápido do que a gente costura. Espantem seis delas e o primeiro vento da monção é de vocês."),
            new LocalizationEntry("quest.city_taa_pwani_thief_gulls.active",
                "Still gulls over the reef. Watch the sky at low tide: they dive when the sails are drying.",
                "Ainda tem gaivota em cima do recife. Olhem o céu na maré baixa: elas mergulham quando as velas estão secando."),

            // ================= Taa Pwani — Zawadi, o trançador de raízes =================
            new LocalizationEntry("quest.city_taa_pwani_mangrove_claws.title", "Claws in the Mangrove", "Garras no Mangue"),
            new LocalizationEntry("quest.city_taa_pwani_mangrove_claws.description",
                "Giant crabs gone dim are cutting the living roots of the mangroves Zawadi braids his boots from. Without the roots, nobody walks the mud of Liyongo's mangroves.",
                "Caranguejos gigantes apagados estão cortando as raízes vivas dos mangues de onde o Zawadi trança as botas. Sem as raízes, ninguém anda no lodo dos mangues de Liyongo."),
            new LocalizationEntry("quest.city_taa_pwani_mangrove_claws.obj_cut_free_the_roots", "Free the roots from the giant claws", "Livrar as raízes das garras gigantes"),
            new LocalizationEntry("quest.city_taa_pwani_mangrove_claws.journal",
                "Zawadi only braids the roots that fall on their own. He says the mangrove gives what it can spare, like Grandma Edna's orchard. Joaquim doesn't sink in the mud, but he wants a pair anyway.",
                "O Zawadi só trança as raízes que caem sozinhas. Ele diz que o mangue dá o que pode, igual ao pomar da Vó Edna. O Joaquim não afunda no lodo, mas quer um par mesmo assim."),
            new LocalizationEntry("quest.city_taa_pwani_mangrove_claws.offer",
                "The captain says you can handle gulls. Can you handle claws? Five giant crabs are cutting my roots. Free them and the first pair of root boots of the tide is yours.",
                "O capitão diz que vocês dão conta de gaivota. E de garra? Cinco caranguejos gigantes estão cortando as minhas raízes. Livrem elas e o primeiro par de botas de raiz da maré é de vocês."),
            new LocalizationEntry("quest.city_taa_pwani_mangrove_claws.active",
                "Pole pole: a crab backs off sideways. Wait for it to open its claws, then strike.",
                "Pole pole: caranguejo recua de lado. Espera ele abrir a garra, aí bate."),

            // ================= Tambor Rubro — Gurparanzah, a chefe-arqueira =================
            new LocalizationEntry("quest.city_tambor_rubro_red_scorpions.title", "Red Tails on the Mesa", "Caudas Vermelhas na Mesa"),
            new LocalizationEntry("quest.city_tambor_rubro_red_scorpions.description",
                "Scorpions gone dim by the shadow nest in the cracks of the badlands and sting the clans' wolves on the way to the forge. Gurparanzah, the archer chief of the Tambor Rubro, wants the road to the canyons clear before the next drum.",
                "Escorpiões apagados pela sombra fazem ninho nas fendas dos ermos e picam os lobos dos clãs no caminho da forja. A Gurparanzah, chefe-arqueira do Tambor Rubro, quer a estrada dos cânions limpa antes do próximo tambor."),
            new LocalizationEntry("quest.city_tambor_rubro_red_scorpions.obj_clear_red_scorpions", "Clear the red scorpions from the canyon road", "Limpar os escorpiões rubros da estrada dos cânions"),
            new LocalizationEntry("quest.city_tambor_rubro_red_scorpions.journal",
                "Gurparanzah says an orc's word weighs more than an orc's axe. Grandpa Jorge once gave her his word he'd bring a sack of Grandma Edna's apricots, and he did, through a sandstorm.",
                "A Gurparanzah diz que palavra de orc pesa mais que machado de orc. O Vô Jorge uma vez deu a palavra de trazer um saco de damascos da Vó Edna, e trouxe, no meio de uma tempestade de areia."),
            new LocalizationEntry("quest.city_tambor_rubro_red_scorpions.offer",
                "The grandchildren of Jorge and Edna, inside my palisade! Then you keep your word like he did. Six red scorpions are nesting on the canyon road. Clear them and the forge drum will play your names tonight.",
                "Os netos do Jorge e da Edna, dentro da minha paliçada! Então vocês cumprem a palavra como ele cumpria. Seis escorpiões rubros fizeram ninho na estrada dos cânions. Limpem e o tambor da forja toca o nome de vocês hoje à noite."),
            new LocalizationEntry("quest.city_tambor_rubro_red_scorpions.active",
                "Still tails on the road. They hide in the shade of the chimneys of stone at noon: look there first.",
                "Ainda tem cauda na estrada. Ao meio-dia eles se escondem na sombra das chaminés de pedra: olhem lá primeiro."),

            // ================= Tambor Rubro — Kessi, o caçador =================
            new LocalizationEntry("quest.city_tambor_rubro_ember_hawks.title", "Hawks of Ember", "Gaviões de Brasa"),
            new LocalizationEntry("quest.city_tambor_rubro_ember_hawks.description",
                "Hawks gone dim dive on the camp's hide racks and carry off the meat the drums were going to share. Kessi, the hunter of the Tambor Rubro, never hunts more than the camp eats: five of them, and not one more.",
                "Gaviões apagados mergulham nos varais de couro do acampamento e levam a carne que os tambores iam repartir. O Kessi, caçador do Tambor Rubro, nunca caça mais do que o acampamento come: cinco deles, nem um a mais."),
            new LocalizationEntry("quest.city_tambor_rubro_ember_hawks.obj_hunt_ember_hawks", "Hunt the ember hawks", "Caçar os gaviões de brasa"),
            new LocalizationEntry("quest.city_tambor_rubro_ember_hawks.journal",
                "Kessi hunts with the drum's rhythm: three beats to aim, one to loose. Maithe says she'll try it. Lavine says the hawks were only hungry, and Kessi agrees with her.",
                "O Kessi caça no ritmo do tambor: três batidas para mirar, uma para soltar. A Maithe diz que vai tentar. A Lavine diz que os gaviões só estavam com fome, e o Kessi concorda com ela."),
            new LocalizationEntry("quest.city_tambor_rubro_ember_hawks.offer",
                "Gurparanzah says you keep your word. Good: so do I. Five ember hawks are robbing the racks. Bring them down and take this theriac: the Throne of Namrael is all poison, you'll need it.",
                "A Gurparanzah diz que vocês cumprem a palavra. Bom: eu também. Cinco gaviões de brasa estão roubando os varais. Derrubem eles e levem esta teriaga: o Trono de Namrael é só veneno, vocês vão precisar."),
            new LocalizationEntry("quest.city_tambor_rubro_ember_hawks.active",
                "Watch the sky over the canyons. A hawk that is circling is still deciding; strike when it dives.",
                "Olhem o céu em cima dos cânions. Gavião que está rodando ainda está decidindo; acertem quando ele mergulhar."),
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
