using System.Collections.Generic;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Enemies;

namespace Rapadura.UI.Bestiary
{
    /// <summary>
    /// "Caderno da Vó Edna" pages of the last four roles of the Limiar Profundo (Região 15, Docs/Mapas100.md 83, 86, 88, 89) that the
    /// overworld still played with another region's beast after <see cref="DeepOwnBestiaryTexts"/> (bible §3.2.1, twenty-second pass, second
    /// half): lore in Grandma Edna's voice and one line per cousin, en + pt-BR. <see cref="BestiaryTexts"/> appends these to its
    /// <see cref="BestiaryTexts.Entries"/>. Never gore (bible §4.3).
    /// </summary>
    public static class DeepOwn2BestiaryTexts
    {
        /// <summary>The asset ids these pages are for.</summary>
        public static readonly string[] EnemyIds =
        {
            "enemy_mountain_rampart_ushabti", "enemy_mountain_gate_smoke", "enemy_mountain_ruin_whirlwind", "enemy_forest_dark_crawler",
        };

        public static void AddPages(List<LocalizationEntry> entries)
        {
            // -- Muralha em Arco, Cisternas de Óleo, Varais de Pavios (83) --
            Page(entries, "enemy_mountain_rampart_ushabti",
                "The lamp-keepers of the Last Lamp set basalt answering figures along the Arched Rampart, each with a lamp on its head and a snuffer to trim the wicks, so the lamps would burn all night while the keepers slept. The shadow that climbs out of the abyss called them, and the glyphs on their fronts went dark one by one. Now they march the wall and put the lamps out. They are slow and very heavy; step back from the snuffer and strike after it comes down.",
                "Os guardiões da Última Lâmpada puseram figuras de basalto que respondem ao longo da Muralha em Arco, cada uma com uma lâmpada na cabeça e um apagador pra aparar os pavios, pra que as lâmpadas queimassem a noite inteira enquanto os guardiões dormiam. A sombra que sobe do abismo chamou elas, e os hieróglifos da frente foram se apagando um por um. Agora elas marcham pela muralha e apagam as lâmpadas. São lentas e pesadíssimas; recuem do apagador e batam depois que ele descer.",
                ("Back off from the snuffer, then hit it on the way up.", "Recua do apagador e bate na subida dele."),
                ("The lamp on its head is cold. When we're done, can we light it again?", "A lâmpada na cabeça dela tá fria. Quando a gente terminar, dá pra acender de novo?"),
                ("Somebody gave it a candle snuffer and a lot of free time. Bad idea.", "Alguém deu um apagador de vela e muito tempo livre pra ela. Péssima ideia."),
                ("One glyph at the top still burns. Pamour says that's the keeper who made it, still asking it to remember.", "Um hieróglifo lá em cima ainda queima. O Pamour diz que é o guardião que fez ela, ainda pedindo pra ela lembrar."),
                ("It has a lamp for a hat! I want a lamp for a hat.", "Ela tem uma lâmpada de chapéu! Eu quero uma lâmpada de chapéu."));

            // -- Nichos de Basalto, Varais de Pavios (83) --
            Page(entries, "enemy_mountain_gate_smoke",
                "The five pits under the fortress are sealed from the other side, but smoke goes where doors do not. It seeps under the five gates and creeps along the niches, wide and low, and sits on the lamps until they choke. Sometimes it swallows a wick and carries it about, still smouldering, like a prize. It is soft but it smothers; keep to the lit ground and hit it where the wick glows.",
                "As cinco covas debaixo da fortaleza estão seladas do outro lado, mas a fumaça passa onde porta não passa. Ela vaza por baixo dos cinco portões e se arrasta pelos nichos, larga e baixa, e senta em cima das lâmpadas até elas engasgarem. Às vezes engole um pavio e fica carregando ele por aí, ainda fumegando, como um prêmio. Ela é mole mas abafa; fiquem no chão aceso e batam onde o pavio brilha.",
                ("Stay on the lit ground and strike at the wick.", "Fica no chão aceso e bate no pavio."),
                ("It took a wick home like a treasure. Maybe it only wanted a little light too.", "Ela levou um pavio pra casa como um tesouro. Vai ver ela também só queria um pouco de luz."),
                ("It got in under the door. Like my cousin when there's cake.", "Entrou por baixo da porta. Igual minha prima quando tem bolo."),
                ("A seal holds a door, not the air around it. Pamour's guardians know that; that's why they keep the lamps.", "Selo segura porta, não o ar em volta dela. Os guardiões do Pamour sabem; é por isso que cuidam das lâmpadas."),
                ("It's a cloud that walks on the floor! Clouds should be up!", "É uma nuvem que anda no chão! Nuvem tem que ficar lá em cima!"));

            // -- Plataformas Soltas, Correntes Ascendentes (86) --
            Page(entries, "enemy_mountain_ruin_whirlwind",
                "The Ruinous Wind is the third of the five dark elements, the one of the things that fly. In its pit it turns round and round the shaft, and where it turns tight enough it holds a shape: a funnel, narrow at the floor and wide on top, full of grit. One of them tore a spark off the third lamp and keeps it spinning in its eye where it can never fall. It hits hard and spins away before you can answer; brace, step inside its turn and strike the eye.",
                "O Vento Destruidor é o terceiro dos cinco elementos escuros, o das coisas que voam. Na cova dele ele gira e gira em volta do poço, e onde gira apertado o bastante ganha forma: um funil, estreito no chão e largo em cima, cheio de areia. Um deles arrancou uma faísca da terceira lâmpada e deixa ela girando no olho dele, onde nunca cai. Ele bate forte e sai girando antes de vocês responderem; firmem os pés, entrem na volta dele e batam no olho.",
                ("Brace, step into its turn and hit the eye.", "Firma os pés, entra na volta dele e bate no olho."),
                ("That spark has been spinning for so long. It must be dizzy.", "Essa faísca tá girando faz tanto tempo. Deve tá tonta."),
                ("I'd ride it if it went anywhere useful.", "Eu montava nele se ele fosse pra algum lugar que prestasse."),
                ("The wind is not bad. This one forgot that it is supposed to carry things, not throw them.", "O vento não é ruim. Esse esqueceu que era pra carregar as coisas, não arremessar."),
                ("It's a spinning top as tall as Joaquim!", "É um pião do tamanho do Joaquim!"));

            // -- Túneis Cegos (88), Lances do Novelo (89) --
            Page(entries, "enemy_forest_dark_crawler",
                "The Darkness is the fifth element, the one of the things that crawl, and the Prince of Darkness took his dragon's body from it. The crawlers are what fell off that body when he went up out of the pit: small, low, six-legged, with spines down the back, and they never come alone. They follow the warmth of a lamp in the dark and bite it all together. Keep your light up, put your back to a wall, and do not let them get round you.",
                "A Treva é o quinto elemento, o das coisas que rastejam, e o Príncipe das Trevas tirou dela o corpo de dragão. Os rastejantes são o que caiu desse corpo quando ele subiu da cova: pequenos, baixos, de seis patas, com espinhos nas costas, e nunca vêm sozinhos. Seguem o calor de uma lamparina no escuro e mordem todos juntos. Mantenham a luz alta, encostem numa parede e não deixem eles darem a volta.",
                ("Back to the wall, light up, and don't let them get round.", "Costas na parede, luz alta, e não deixa eles darem a volta."),
                ("They're so small. They only follow us because we're warm.", "São tão pequenininhos. Só seguem a gente porque a gente é quentinho."),
                ("Six legs and still they can't walk straight.", "Seis patas e mesmo assim não andam reto."),
                ("Pieces of a dragon that forgot it was ever a dragon. They still look for the heat it lost.", "Pedaços de um dragão que esqueceu que um dia foi dragão. Ainda procuram o calor que ele perdeu."),
                ("Their backs have little mountains on them!", "As costas deles têm umas montanhinhas!"));
        }

        private static void Page(
            List<LocalizationEntry> entries, string enemyId, string loreEn, string lorePt,
            (string en, string pt) maithe, (string en, string pt) maria, (string en, string pt) joaquim,
            (string en, string pt) icaro, (string en, string pt) lavine)
        {
            entries.Add(new LocalizationEntry(BestiaryCatalog.LoreKey(enemyId), loreEn, lorePt));
            (string en, string pt)[] comments = { maithe, maria, joaquim, icaro, lavine };
            for (int i = 0; i < BestiaryTexts.Cousins.Length; i++)
            {
                entries.Add(new LocalizationEntry(BestiaryCatalog.CommentKey(enemyId, BestiaryTexts.Cousins[i]), comments[i].en, comments[i].pt));
            }
        }
    }
}
