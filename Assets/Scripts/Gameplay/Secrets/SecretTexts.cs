using System.Collections.Generic;
using System.Linq;
using Rapadura.Core.Localization;

namespace Rapadura.Gameplay.Secrets
{
    /// <summary>
    /// Localization keys (en + pt-BR) for "Segredos e áreas ocultas": UI chrome (<c>ui.secrets.*</c>), the world-map
    /// secrets (<c>secret.&lt;id&gt;.name/.reveal</c>), the 12 "Lembranças do Vô Jorge" (old name: Relíquias do Sertão) (<c>relic.&lt;id&gt;.name/.lore/
    /// .passive/.clue</c>) (the collection achievement's title/description live in <c>AchievementTexts</c>, with the other achievements). Same self-contained pattern as <c>VillageChestTexts</c>:
    /// merged into the live <see cref="LocalizationManager"/> on first use (keys it already has win), pt-BR fallback
    /// without a manager.
    ///
    /// Family canon (Docs/GDD.md): Maithe and Ícaro are siblings, Joaquim and Maria are siblings, Lavine is cousin to
    /// all four; Vó Edna keeps the house and orchard by the Zarafshan Woods; Vô Jorge — the grandfather, remembered with
    /// love, who travelled with the caravans and told the old tales in the iwan — hid these keepsakes around the island
    /// for the grandchildren to find.
    /// </summary>
    public static class SecretTexts
    {
        /// <summary>{0} = secret name.</summary>
        public const string Investigate = "ui.secrets.investigate";
        /// <summary>{0} = secret name (a guardian fight starts).</summary>
        public const string InvestigateGuarded = "ui.secrets.investigate_guarded";
        public const string RelicsButton = "ui.secrets.relics_button";
        public const string PanelTitle = "ui.secrets.panel_title";
        public const string PanelSubtitle = "ui.secrets.panel_subtitle";
        /// <summary>{0} = owned, {1} = total.</summary>
        public const string PanelCount = "ui.secrets.panel_count";
        public const string PanelClose = "ui.secrets.panel_close";
        public const string PanelAllFound = "ui.secrets.panel_all_found";
        /// <summary>{0} = gold waiting for the bag.</summary>
        public const string PanelPendingGold = "ui.secrets.panel_pending_gold";
        public const string UnknownName = "ui.secrets.unknown_name";
        /// <summary>{0} = hero name.</summary>
        public const string OwnerBonus = "ui.secrets.owner_bonus";
        public const string FamilyTag = "ui.secrets.family_tag";
        public const string ToastSecretTitle = "ui.secrets.toast_secret_title";
        public const string ToastRelicTitle = "ui.secrets.toast_relic_title";
        /// <summary>{0} = relic name, {1} = owned, {2} = total.</summary>
        public const string ToastRelicBody = "ui.secrets.toast_relic_body";
        /// <summary>{0} = gold.</summary>
        public const string ToastMapGold = "ui.secrets.toast_map_gold";
        public const string ToastArenaTitle = "ui.secrets.toast_arena_title";
        public const string ToastArenaRock = "ui.secrets.toast_arena_rock";
        public const string ToastArenaDig = "ui.secrets.toast_arena_dig";
        /// <summary>{0} = gold.</summary>
        public const string ToastArenaGold = "ui.secrets.toast_arena_gold";
        public const string ToastCollectionDone = "ui.secrets.toast_collection_done";

        public static readonly List<LocalizationEntry> Entries = Flatten(new object[]
        {
            new LocalizationEntry(Investigate, "Investigate: {0}", "Investigar: {0}"),
            new LocalizationEntry(InvestigateGuarded, "Investigate: {0} (guarded!)", "Investigar: {0} (tem guarda!)"),
            new LocalizationEntry(RelicsButton, "Keepsakes", "Lembranças"),
            new LocalizationEntry(PanelTitle, "Grandpa Jorge's Keepsakes", "Lembranças do Vô Jorge"),
            new LocalizationEntry(PanelSubtitle, "Keepsakes Grandpa Jorge hid around the island for his grandchildren to find one day.",
                "Lembranças que o Vô Jorge escondeu pela ilha para os netos acharem um dia."),
            new LocalizationEntry(PanelCount, "{0}/{1} found", "{0}/{1} encontradas"),
            new LocalizationEntry(PanelClose, "Close", "Fechar"),
            new LocalizationEntry(PanelAllFound, "The whole collection! Grandma Edna will cry with joy.", "A coleção inteira! A Vó Edna vai chorar de alegria."),
            new LocalizationEntry(PanelPendingGold, "{0} gold from the secrets goes into the backpack at the next fight.", "{0} de ouro dos segredos entra na mochila na próxima luta."),
            new LocalizationEntry(UnknownName, "???", "???"),
            new LocalizationEntry(OwnerBonus, "Twice as strong with {0}", "Rende o dobro com {0}"),
            new LocalizationEntry(FamilyTag, "The whole family's", "Da família toda"),
            new LocalizationEntry(ToastSecretTitle, "A secret!", "Um segredo!"),
            new LocalizationEntry(ToastRelicTitle, "Keepsake found!", "Lembrança encontrada!"),
            new LocalizationEntry(ToastRelicBody, "{0} ({1}/{2})", "{0} ({1}/{2})"),
            new LocalizationEntry(ToastMapGold, "+{0} gold — it goes into the backpack at the next fight.", "+{0} de ouro — entra na mochila na próxima luta."),
            new LocalizationEntry(ToastArenaTitle, "Something glints nearby...", "Algo brilha por perto..."),
            new LocalizationEntry(ToastArenaRock, "A cracked rock is shining. Break it before the fight ends!", "Uma pedra rachada está brilhando. Quebre-a antes da luta acabar!"),
            new LocalizationEntry(ToastArenaDig, "Something shines under the sand. Dig (attack) before the fight ends!", "Algo brilha debaixo da areia. Cave (ataque) antes da luta acabar!"),
            new LocalizationEntry(ToastArenaGold, "It was hiding {0} gold.", "Escondia {0} de ouro."),
            new LocalizationEntry(ToastCollectionDone, "Every one of Grandpa Jorge's keepsakes is home!", "Todas as lembranças do Vô Jorge voltaram pra casa!"),

            // ---- World-map secrets ----
            Secret(WorldSecretCatalog.CasaDeFarinha, "Grandpa's old mill", "Moinho velho do Vô",
                "Walls white with old flour, a cracked millstone... Grandpa Jorge's old orchard mill is still standing.",
                "Paredes brancas de farinha antiga, uma mó rachada... o velho moinho do pomar do Vô Jorge ainda está de pé."),
            Secret(WorldSecretCatalog.JatobaOco, "Hollow poplar", "Choupo oco",
                "At night, fireflies swirl around the hollow of the old tugai poplar — something is hidden inside.",
                "De noite, vaga-lumes rodeiam o oco do velho choupo do tugai — tem alguma coisa escondida lá dentro."),
            Secret(WorldSecretCatalog.BauMandacaru, "Chest under the saxaul", "Baú sob o saxaul",
                "The wind blew the sand away from the roots of the big saxaul: the corner of a chest shows.",
                "O vento levou a areia das raízes do saxaul grande: aparece a quina de um baú."),
            Secret(WorldSecretCatalog.CacimbaSeca, "Dry karez well", "Poço de karez seco",
                "With the Flaming Sands calm again, voices echo up the dry karez well. Someone is guarding what fell in there.",
                "Com as Areias Flamejantes em paz de novo, ecoam vozes no poço de karez seco. Alguém guarda o que caiu lá dentro."),
            Secret(WorldSecretCatalog.GrutaCachoeira, "Cave behind the waterfall", "Gruta atrás da cachoeira",
                "With the Kaggen Range freed, the waterfall runs thin — and a cave mouth appears behind it.",
                "Com a Serra de Kaggen livre, a cachoeira afinou — e atrás dela aparece a boca de uma gruta."),
            Secret(WorldSecretCatalog.MarcoTropeiros, "Caravan cairn", "Marco das caravanas",
                "Stand still long enough and you hear it: a little bell ringing inside the caravans' stone cairn.",
                "Quem fica parado escuta: um sininho tocando dentro do marco de pedra das caravanas."),
            Secret(WorldSecretCatalog.OficinaEsquecida, "Forgotten workshop", "Oficina esquecida",
                "Among the rocks there is a little shed full of springs and cogs. It looks a lot like Ícaro's handwriting.",
                "Entre as pedras tem um barraquinho cheio de molas e engrenagens. Parece muito a letra do Ícaro."),
            Secret(WorldSecretCatalog.PinturasRocha, "Rock paintings", "Pinturas na rocha",
                "At night the old paintings on the rock glow red — the same red as the night Lavine was born.",
                "De noite as pinturas antigas na rocha brilham vermelhas — o mesmo vermelho da noite em que a Lavine nasceu."),

            // ---- Keepsakes (Lembranças do Vô Jorge) ----
            Relic(RelicCatalog.GaitaVoChico, "Grandpa Jorge's ney flute", "Flauta Ney do Vô Jorge",
                "A reed flute from the caravan roads. He played it in the iwan, Grandma's deep porch, every night, out of tune, while Grandma Edna pretended to complain. The cousins fell asleep to it.",
                "Uma flauta de cana das estradas de caravana. Ele tocava toda noite no iwan, o alpendre da Vó, desafinado, e a Vó Edna fingia reclamar. Os primos dormiam ouvindo.",
                "+8% stamina regeneration", "+8% de regeneração de vigor",
                "Grandpa's old mill, in the Zarafshan Woods. Stay still a while and look around.",
                "O moinho velho do Vô, no Bosque do Zarafshan. Fique parado um tempo e olhe em volta."),
            Relic(RelicCatalog.ArcoMaithe, "Maithe's first bow", "Primeiro arco da Maithe",
                "Grandpa carved it from an apricot branch when she was seven. She missed every shot for a month — then never missed again.",
                "O Vô entalhou de um galho de damasqueiro quando ela tinha sete anos. Ela errou todo tiro por um mês — depois nunca mais errou.",
                "+1% critical chance", "+1% de chance de crítico",
                "A hollow tree in the Zarafshan Woods. Fireflies only come out at night.",
                "Uma árvore oca no Bosque do Zarafshan. Vaga-lume só aparece de noite."),
            Relic(RelicCatalog.CartasVoChico, "Grandpa Jorge's letters to Grandma Edna", "Cartas do Vô Jorge para a Vó Edna",
                "Tied with a ribbon in a tin, from when he travelled with the caravans across the Flaming Sands. Every one ends with \"back soon, Edna\".",
                "Amarradas com fita numa lata, do tempo em que ele viajava com as caravanas pelas Areias Flamejantes. Toda carta termina com \"volto logo, Edna\".",
                "+10 max health", "+10 de vida máxima",
                "Something buried under a big saxaul in the Flaming Sands. Patience uncovers it.",
                "Algo enterrado sob um saxaul grande nas Areias Flamejantes. A paciência desenterra."),
            Relic(RelicCatalog.EscudinhoJoaquim, "Joaquim's barrel-lid shield", "Escudinho de tampa de barril",
                "Joaquim's first shield, made from a barrel lid when he was six, to protect Maria from the geese. He lost it in the Flaming Sands.",
                "O primeiro escudo do Joaquim, feito de tampa de barril aos seis anos pra proteger a Maria dos gansos. Ele perdeu nas Areias Flamejantes.",
                "+2 defense", "+2 de defesa",
                "A dry karez well in the Flaming Sands. Come back once the Great Ushabti remembers who it is.",
                "Um poço de karez seco nas Areias Flamejantes. Volte depois que o Grande Ushabti lembrar quem é."),
            Relic(RelicCatalog.MedalhaMaria, "Maria's little sun medal", "Medalhinha de sol da Maria",
                "Grandma Edna hung it on Maria's neck on her first day at the Solar Temple: \"so the light knows where to find you\".",
                "A Vó Edna pendurou no pescoço da Maria no primeiro dia dela no Templo Solar: \"pra luz saber onde te achar\".",
                "+0.4 health regeneration per second", "+0,4 de regeneração de vida por segundo",
                "Behind a waterfall in the Kaggen Range. It only thins out once Hahya the Giant Warrior remembers who he is.",
                "Atrás de uma cachoeira da Serra de Kaggen. Ela só afina depois que o Hahya, o Gigante Guerreiro, lembrar quem é."),
            Relic(RelicCatalog.ChocalhoTeimosa, "Teimosa the camel's bell", "Sino da camela Teimosa",
                "Grandpa's Bactrian camel was called Teimosa — Stubborn — and lived up to it. Her caravan bell still rings when no one is touching it.",
                "A camela bactriana do Vô se chamava Teimosa e fazia jus ao nome. O sino de caravana dela ainda toca quando ninguém está mexendo.",
                "+3% movement speed", "+3% de velocidade de movimento",
                "The caravans' stone cairn in the Kaggen Range. Stand still and listen.",
                "O marco de pedra das caravanas, na Serra de Kaggen. Fique parado e escute."),
            Relic(RelicCatalog.EngrenagemIcaro, "Ícaro's first gear", "Primeira engrenagem do Ícaro",
                "From the pomegranate-peeling machine he built at five. It peeled nothing, exploded twice, and Grandpa kept the gear as a trophy.",
                "Da máquina de descascar romã que ele montou aos cinco anos. Não descascou nada, explodiu duas vezes, e o Vô guardou a engrenagem como troféu.",
                "-3% skill cooldowns", "-3% na recarga das habilidades",
                "A forgotten workshop among the rocks of the Threshold Caves. Take your time looking.",
                "Uma oficina esquecida entre as pedras das Grutas do Limiar. Procure com calma."),
            Relic(RelicCatalog.FitaLavine, "Lavine's red ribbon", "Fita vermelha da Lavine",
                "Grandma tied it on Lavine's wrist on the Noite Rubra she was born, \"so the dark knows she belongs to us\".",
                "A Vó amarrou no pulso da Lavine na Noite Rubra em que ela nasceu, \"pro escuro saber que ela é nossa\".",
                "+8 max mana", "+8 de mana máxima",
                "Old paintings on a rock in the Threshold Caves. They glow only at night.",
                "Pinturas antigas numa rocha das Grutas do Limiar. Só brilham de noite."),
            Relic(RelicCatalog.CaniveteVoChico, "Grandpa Jorge's pocketknife", "Canivete do Vô Jorge",
                "He used it to cut the Golden Brick, sharpen pencils and settle arguments about who got the last piece.",
                "Ele usava pra cortar Torrão Dourado, apontar lápis e resolver briga de quem ficava com o último pedaço.",
                "+1 attack damage", "+1 de dano de ataque",
                "Sometimes, in a fight in the Zarafshan Woods, a cracked rock shines. Break it before the fight ends.",
                "Às vezes, numa luta no Bosque do Zarafshan, uma pedra rachada brilha. Quebre-a antes da luta acabar."),
            Relic(RelicCatalog.ChapeuCouro, "Grandpa Jorge's felt cap", "Gorro de Feltro do Vô Jorge",
                "The caravaneer's felt cap he wore across the Flaming Sands. It still smells of sun and of Grandma's tea.",
                "O gorro de feltro de caravaneiro que ele usava atravessando as Areias Flamejantes. Ainda tem cheiro de sol e do chá da Vó.",
                "+8 max stamina", "+8 de vigor máximo",
                "Sometimes, in a fight in the Flaming Sands, the sand glints. Dig it up (attack) before the fight ends.",
                "Às vezes, numa luta nas Areias Flamejantes, a areia brilha. Cave (ataque) antes da luta acabar."),
            Relic(RelicCatalog.PiaoIrmaos, "Maria and Joaquim's spinning top", "Pião de Maria e Joaquim",
                "The siblings' only shared toy, and the cause of every fight between them. Maria always won; Joaquim says otherwise.",
                "O único brinquedo que os irmãos dividiam, e motivo de toda briga entre os dois. A Maria sempre ganhava; o Joaquim diz que não.",
                "+3% attack speed", "+3% de velocidade de ataque",
                "Sometimes, in a fight in the Kaggen Range, a cracked rock shines. Break it before the fight ends.",
                "Às vezes, numa luta na Serra de Kaggen, uma pedra rachada brilha. Quebre-a antes da luta acabar."),
            Relic(RelicCatalog.RetratoPrimos, "Portrait of the five cousins", "Retrato dos cinco primos",
                "Maithe, Maria, Joaquim, Ícaro and Lavine, the youngest, in Grandma's iwan, all blurry because nobody would stand still.",
                "Maithe, Maria, Joaquim, Ícaro e a caçula Lavine no iwan da Vó, todos borrados porque ninguém parava quieto.",
                "-3% skill costs", "-3% no custo das habilidades",
                "Sometimes, in a fight in the Threshold Caves, a crystal-crusted rock shines. Break it before the fight ends.",
                "Às vezes, numa luta nas Grutas do Limiar, uma rocha de cristal brilha. Quebre-a antes da luta acabar."),
        });

        /// <summary>Single entries and the per-secret / per-relic groups, in order.</summary>
        private static List<LocalizationEntry> Flatten(object[] items)
        {
            var entries = new List<LocalizationEntry>();
            foreach (object item in items)
            {
                if (item is LocalizationEntry entry)
                {
                    entries.Add(entry);
                }
                else if (item is LocalizationEntry[] group)
                {
                    entries.AddRange(group);
                }
            }

            return entries;
        }

        private static LocalizationEntry[] Secret(string id, string nameEn, string namePt, string revealEn, string revealPt)
        {
            WorldSecretDefinition secret = WorldSecretCatalog.Get(id);
            return new[]
            {
                new LocalizationEntry(secret.NameKey, nameEn, namePt),
                new LocalizationEntry(secret.RevealKey, revealEn, revealPt),
            };
        }

        private static LocalizationEntry[] Relic(string id, string nameEn, string namePt, string loreEn, string lorePt,
            string passiveEn, string passivePt, string clueEn, string cluePt)
        {
            RelicDefinition relic = RelicCatalog.Get(id);
            return new[]
            {
                new LocalizationEntry(relic.NameKey, nameEn, namePt),
                new LocalizationEntry(relic.LoreKey, loreEn, lorePt),
                new LocalizationEntry(relic.PassiveKey, passiveEn, passivePt),
                new LocalizationEntry(relic.ClueKey, clueEn, cluePt),
            };
        }

        private static Dictionary<string, string> _fallbackByKey;
        private static LocalizationManager _mergedInto;

        /// <summary>Merges <see cref="Entries"/> into <paramref name="manager"/>, skipping keys it already has. Safe to call repeatedly.</summary>
        public static void EnsureRegistered(LocalizationManager manager)
        {
            if (manager == null || (ReferenceEquals(manager, _mergedInto) && manager.HasKey(Entries[0].Key)))
            {
                return;
            }

            manager.LoadEntries(Entries.Where(entry => !manager.HasKey(entry.Key)).ToList());
            _mergedInto = manager;
        }

        /// <summary>Localized text via <paramref name="manager"/> when it knows the key, else the pt-BR fallback, else the key.</summary>
        public static string Get(LocalizationManager manager, string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return string.Empty;
            }

            if (manager != null)
            {
                EnsureRegistered(manager);
                if (manager.HasKey(key))
                {
                    return manager.Get(key);
                }
            }

            return GetFallback(key) ?? key;
        }

        /// <summary><see cref="Get"/> + <see cref="string.Format(string, object[])"/>, tolerant of malformed translations.</summary>
        public static string Format(LocalizationManager manager, string key, params object[] args)
        {
            string pattern = Get(manager, key);
            try
            {
                return string.Format(pattern, args);
            }
            catch (System.FormatException)
            {
                return string.Format(GetFallback(key) ?? key, args);
            }
        }

        public static string GetFallback(string key)
        {
            if (key == null)
            {
                return null;
            }

            if (_fallbackByKey == null)
            {
                var table = new Dictionary<string, string>();
                foreach (LocalizationEntry entry in Entries)
                {
                    table[entry.Key] = entry.PtBR;
                }

                _fallbackByKey = table;
            }

            return _fallbackByKey.TryGetValue(key, out string text) ? text : null;
        }
    }
}
