using System.Collections.Generic;
using System.Linq;
using Rapadura.Core.Localization;

namespace Rapadura.Gameplay.Secrets
{
    /// <summary>
    /// Localization keys (en + pt-BR) for "Segredos e áreas ocultas": UI chrome (<c>ui.secrets.*</c>), the world-map
    /// secrets (<c>secret.&lt;id&gt;.name/.reveal</c>), the 12 "Relíquias do Sertão" (<c>relic.&lt;id&gt;.name/.lore/
    /// .passive/.clue</c>) (the collection achievement's title/description live in <c>AchievementTexts</c>, with the other achievements). Same self-contained pattern as <c>VillageChestTexts</c>:
    /// merged into the live <see cref="LocalizationManager"/> on first use (keys it already has win), pt-BR fallback
    /// without a manager.
    ///
    /// Family canon (Docs/GDD.md): Maithe and Ícaro are siblings, Joaquim and Maria are siblings, Lavine is cousin to
    /// all four; Vó Zefa keeps the farm by the Forest; Vô Chico — the late grandfather, a tropeiro who later ran the
    /// casa de farinha — hid these keepsakes around the island for the grandchildren to find.
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
            new LocalizationEntry(RelicsButton, "Relics", "Relíquias"),
            new LocalizationEntry(PanelTitle, "Relics of the Sertão", "Relíquias do Sertão"),
            new LocalizationEntry(PanelSubtitle, "Keepsakes Grandpa Chico hid around the island for his grandchildren to find one day.",
                "Lembranças que o Vô Chico escondeu pela ilha para os netos acharem um dia."),
            new LocalizationEntry(PanelCount, "{0}/{1} found", "{0}/{1} encontradas"),
            new LocalizationEntry(PanelClose, "Close", "Fechar"),
            new LocalizationEntry(PanelAllFound, "The whole collection! Grandma Zefa will cry with joy.", "A coleção inteira! A Vó Zefa vai chorar de alegria."),
            new LocalizationEntry(PanelPendingGold, "{0} gold from the secrets goes into the backpack at the next fight.", "{0} de ouro dos segredos entra na mochila na próxima luta."),
            new LocalizationEntry(UnknownName, "???", "???"),
            new LocalizationEntry(OwnerBonus, "Twice as strong with {0}", "Rende o dobro com {0}"),
            new LocalizationEntry(FamilyTag, "The whole family's", "Da família toda"),
            new LocalizationEntry(ToastSecretTitle, "A secret!", "Um segredo!"),
            new LocalizationEntry(ToastRelicTitle, "Relic found!", "Relíquia encontrada!"),
            new LocalizationEntry(ToastRelicBody, "{0} ({1}/{2})", "{0} ({1}/{2})"),
            new LocalizationEntry(ToastMapGold, "+{0} gold — it goes into the backpack at the next fight.", "+{0} de ouro — entra na mochila na próxima luta."),
            new LocalizationEntry(ToastArenaTitle, "Something glints nearby...", "Algo brilha por perto..."),
            new LocalizationEntry(ToastArenaRock, "A cracked rock is shining. Break it before the fight ends!", "Uma pedra rachada está brilhando. Quebre-a antes da luta acabar!"),
            new LocalizationEntry(ToastArenaDig, "Something shines under the sand. Dig (attack) before the fight ends!", "Algo brilha debaixo da areia. Cave (ataque) antes da luta acabar!"),
            new LocalizationEntry(ToastArenaGold, "It was hiding {0} gold.", "Escondia {0} de ouro."),
            new LocalizationEntry(ToastCollectionDone, "Every relic of the Sertão is home!", "Todas as relíquias do Sertão voltaram pra casa!"),

            // ---- World-map secrets ----
            Secret(WorldSecretCatalog.CasaDeFarinha, "Abandoned flour house", "Casa de farinha abandonada",
                "Smoke-stained walls, a cracked millstone... Grandpa Chico's old flour house is still standing.",
                "Paredes pretas de fumaça, uma mó rachada... a velha casa de farinha do Vô Chico ainda está de pé."),
            Secret(WorldSecretCatalog.JatobaOco, "Hollow jatobá tree", "Jatobá oco",
                "At night, fireflies swirl around the hollow of the old jatobá — something is hidden inside.",
                "De noite, vaga-lumes rodeiam o oco do velho jatobá — tem alguma coisa escondida lá dentro."),
            Secret(WorldSecretCatalog.BauMandacaru, "Chest under the mandacaru", "Baú sob o mandacaru",
                "The wind blew the sand away from the roots of the big mandacaru: the corner of a chest shows.",
                "O vento levou a areia das raízes do mandacaru grande: aparece a quina de um baú."),
            Secret(WorldSecretCatalog.CacimbaSeca, "Dry cacimba", "Cacimba seca",
                "With the Desert calm again, voices echo up the dry cacimba. Someone is guarding what fell in there.",
                "Com o Deserto em paz de novo, ecoam vozes na cacimba seca. Alguém guarda o que caiu lá dentro."),
            Secret(WorldSecretCatalog.GrutaCachoeira, "Cave behind the waterfall", "Gruta atrás da cachoeira",
                "With the Mountain freed, the waterfall runs thin — and a cave mouth appears behind it.",
                "Com a Montanha livre, a cachoeira afinou — e atrás dela aparece a boca de uma gruta."),
            Secret(WorldSecretCatalog.MarcoTropeiros, "Muleteers' cairn", "Marco dos tropeiros",
                "Stand still long enough and you hear it: a little bell ringing inside the muleteers' stone cairn.",
                "Quem fica parado escuta: um chocalhinho tocando dentro do marco de pedra dos tropeiros."),
            Secret(WorldSecretCatalog.OficinaEsquecida, "Forgotten workshop", "Oficina esquecida",
                "Among the rocks there is a little shed full of springs and cogs. It looks a lot like Ícaro's handwriting.",
                "Entre as pedras tem um barraquinho cheio de molas e engrenagens. Parece muito a letra do Ícaro."),
            Secret(WorldSecretCatalog.PinturasRocha, "Rock paintings", "Pinturas na rocha",
                "At night the old paintings on the rock glow red — the same red as the night Lavine was born.",
                "De noite as pinturas antigas na rocha brilham vermelhas — o mesmo vermelho da noite em que a Lavine nasceu."),

            // ---- Relics ----
            Relic(RelicCatalog.GaitaVoChico, "Grandpa Chico's harmonica", "Gaita do Vô Chico",
                "He played it on the porch every night, out of tune, while Grandma Zefa pretended to complain. The cousins fell asleep to it.",
                "Ele tocava toda noite na varanda, desafinado, e a Vó Zefa fingia reclamar. Os primos dormiam ouvindo.",
                "+8% stamina regeneration", "+8% de regeneração de vigor",
                "Grandpa's flour house, in the Forest. Stay still a while and look around.",
                "A casa de farinha do Vô, na Floresta. Fique parado um tempo e olhe em volta."),
            Relic(RelicCatalog.ArcoMaithe, "Maithe's first bow", "Primeiro arco da Maithe",
                "Grandpa carved it from a guava branch when she was seven. She missed every shot for a month — then never missed again.",
                "O Vô entalhou de um galho de goiabeira quando ela tinha sete anos. Ela errou todo tiro por um mês — depois nunca mais errou.",
                "+1% critical chance", "+1% de chance de crítico",
                "A hollow tree in the Forest. Fireflies only come out at night.",
                "Uma árvore oca na Floresta. Vaga-lume só aparece de noite."),
            Relic(RelicCatalog.CartasVoChico, "Grandpa's letters to Grandma", "Cartas do Vô Chico para a Vó Zefa",
                "Tied with a ribbon in a tin, from when he drove mules across the Desert. Every one ends with \"come back soon, Zefa\".",
                "Amarradas com fita numa lata, do tempo em que ele tocava tropa pelo Deserto. Toda carta termina com \"volto logo, Zefa\".",
                "+10 max health", "+10 de vida máxima",
                "Something buried under a big mandacaru in the Desert. Patience uncovers it.",
                "Algo enterrado sob um mandacaru grande no Deserto. A paciência desenterra."),
            Relic(RelicCatalog.EscudinhoJoaquim, "Joaquim's barrel-lid shield", "Escudinho de tampa de barril",
                "Joaquim's first shield, made from a barrel lid when he was six, to protect Maria from the geese. He lost it in the Desert.",
                "O primeiro escudo do Joaquim, feito de tampa de barril aos seis anos pra proteger a Maria dos gansos. Ele perdeu no Deserto.",
                "+2 defense", "+2 de defesa",
                "A dry cacimba in the Desert. Come back once the Desert boss is beaten.",
                "Uma cacimba seca no Deserto. Volte depois de vencer o chefe do Deserto."),
            Relic(RelicCatalog.MedalhaMaria, "Maria's little sun medal", "Medalhinha de sol da Maria",
                "Grandma Zefa hung it on Maria's neck on her first day at the Temple: \"so the light knows where to find you\".",
                "A Vó Zefa pendurou no pescoço da Maria no primeiro dia dela no Templo: \"pra luz saber onde te achar\".",
                "+0.4 health regeneration per second", "+0,4 de regeneração de vida por segundo",
                "Behind a Mountain waterfall. It only thins out once the Mountain boss is beaten.",
                "Atrás de uma cachoeira da Montanha. Ela só afina depois de vencer o chefe da Montanha."),
            Relic(RelicCatalog.ChocalhoTeimosa, "Teimosa the mule's bell", "Chocalho da mula Teimosa",
                "Grandpa's mule was called Teimosa — Stubborn — and lived up to it. Her bell still rings when no one is touching it.",
                "A mula do Vô se chamava Teimosa e fazia jus ao nome. O chocalho dela ainda toca quando ninguém está mexendo.",
                "+3% movement speed", "+3% de velocidade de movimento",
                "The muleteers' stone cairn in the Mountain. Stand still and listen.",
                "O marco de pedra dos tropeiros, na Montanha. Fique parado e escute."),
            Relic(RelicCatalog.EngrenagemIcaro, "Ícaro's first gear", "Primeira engrenagem do Ícaro",
                "From the cajá-shelling machine he built at five. It shelled nothing, exploded twice, and Grandpa kept the gear as a trophy.",
                "Da máquina de descascar cajá que ele montou aos cinco anos. Não descascou nada, explodiu duas vezes, e o Vô guardou a engrenagem como troféu.",
                "-3% skill cooldowns", "-3% na recarga das habilidades",
                "A forgotten workshop among the Caves' rocks. Take your time looking.",
                "Uma oficina esquecida entre as pedras das Cavernas. Procure com calma."),
            Relic(RelicCatalog.FitaLavine, "Lavine's red ribbon", "Fita vermelha da Lavine",
                "Grandma tied it on Lavine's wrist on the Noite Rubra she was born, \"so the dark knows she belongs to us\".",
                "A Vó amarrou no pulso da Lavine na Noite Rubra em que ela nasceu, \"pro escuro saber que ela é nossa\".",
                "+8 max mana", "+8 de mana máxima",
                "Old paintings on a Caves rock. They glow only at night.",
                "Pinturas antigas numa rocha das Cavernas. Só brilham de noite."),
            Relic(RelicCatalog.CaniveteVoChico, "Grandpa Chico's pocketknife", "Canivete do Vô Chico",
                "He used it to cut rapadura, sharpen pencils and settle arguments about who got the last piece.",
                "Ele usava pra cortar rapadura, apontar lápis e resolver briga de quem ficava com o último pedaço.",
                "+1 attack damage", "+1 de dano de ataque",
                "Sometimes, in a Forest fight, a cracked rock shines. Break it before the fight ends.",
                "Às vezes, numa luta na Floresta, uma pedra rachada brilha. Quebre-a antes da luta acabar."),
            Relic(RelicCatalog.ChapeuCouro, "Grandpa's leather hat", "Chapéu de couro do Vô Chico",
                "The tropeiro hat he wore across the Desert. It still smells of sun and of Grandma's coffee.",
                "O chapéu de tropeiro que ele usava atravessando o Deserto. Ainda tem cheiro de sol e do café da Vó.",
                "+8 max stamina", "+8 de vigor máximo",
                "Sometimes, in a Desert fight, the sand glints. Dig it up (attack) before the fight ends.",
                "Às vezes, numa luta no Deserto, a areia brilha. Cave (ataque) antes da luta acabar."),
            Relic(RelicCatalog.PiaoIrmaos, "Maria and Joaquim's spinning top", "Pião de Maria e Joaquim",
                "The siblings' only shared toy, and the cause of every fight between them. Maria always won; Joaquim says otherwise.",
                "O único brinquedo que os irmãos dividiam, e motivo de toda briga entre os dois. A Maria sempre ganhava; o Joaquim diz que não.",
                "+3% attack speed", "+3% de velocidade de ataque",
                "Sometimes, in a Mountain fight, a cracked rock shines. Break it before the fight ends.",
                "Às vezes, numa luta na Montanha, uma pedra rachada brilha. Quebre-a antes da luta acabar."),
            Relic(RelicCatalog.RetratoPrimos, "Portrait of the five cousins", "Retrato dos cinco primos",
                "Maithe, Maria, Joaquim, Ícaro and little Lavine on Grandma's porch, all blurry because nobody would stand still.",
                "Maithe, Maria, Joaquim, Ícaro e a pequena Lavine na varanda da Vó, todos borrados porque ninguém parava quieto.",
                "-3% skill costs", "-3% no custo das habilidades",
                "Sometimes, in a Caves fight, a crystal-crusted rock shines. Break it before the fight ends.",
                "Às vezes, numa luta nas Cavernas, uma rocha de cristal brilha. Quebre-a antes da luta acabar."),
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
