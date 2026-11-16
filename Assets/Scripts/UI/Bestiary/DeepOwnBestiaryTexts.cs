using System.Collections.Generic;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Enemies;

namespace Rapadura.UI.Bestiary
{
    /// <summary>
    /// "Caderno da Vó Edna" pages of the twelve roles of the Rio das Doze Horas, the Margens da Mistura and the Limiar Profundo (Regiões 13–15,
    /// Docs/Mapas100.md 74–90) that the overworld still played with another region's beast (bible §3.2.1, twenty-second pass): lore in
    /// Grandma Edna's voice and one line per cousin, en + pt-BR. Kept apart like <see cref="HighlandOwnBestiaryTexts"/>; <see cref="BestiaryTexts"/>
    /// appends these to its <see cref="BestiaryTexts.Entries"/>. Never gore (bible §4.3).
    /// </summary>
    public static class DeepOwnBestiaryTexts
    {
        /// <summary>The asset ids these pages are for.</summary>
        public static readonly string[] EnemyIds =
        {
            "enemy_desert_shadow_salamander", "enemy_desert_coil_scorpion", "enemy_mountain_dimmed_ushabti", "enemy_cave_boiled_shadow",
            "enemy_forest_mud_crocodile", "enemy_forest_cracked_ushabti", "enemy_mountain_clock_ushabti", "enemy_mountain_pitch_smoke",
            "enemy_forest_choking_smoke", "enemy_cave_dark_shoal", "enemy_forest_blind_serpent", "enemy_mountain_giant_dimmed_ushabti",
        };

        public static void AddPages(List<LocalizationEntry> entries)
        {
            // -- Anel Externo, Nó Errado (74) --
            Page(entries, "enemy_desert_coil_scorpion",
                "Every night the great serpent Mehen coils round the barge of the sun so that Apep cannot reach it. Scorpions nest between his coils, where it is warm, and they took the sandstone rings for armour. The shadow taught them to guard the coils against the barge instead of for it. They are slow and very hard; hit them from the side, where the rings do not meet.",
                "Toda noite a grande serpente Mehen se enrola em volta da barca do sol pra Apep não alcançar. Os escorpiões fazem ninho entre as voltas dela, onde é quentinho, e pegaram os anéis de arenito como armadura. A sombra ensinou eles a guardar as voltas contra a barca, em vez de pra ela. São lentos e duríssimos; batam do lado, onde os anéis não se encontram.",
                ("Hit it from the side, between the rings.", "Bate do lado, entre os anéis."),
                ("They only wanted a warm place to sleep. Mehen never minded them.", "Eles só queriam um lugar quentinho pra dormir. A Mehen nunca ligou."),
                ("It's wearing the serpent's rings like a belt. Very fashionable. Very rude.", "Ele tá usando os anéis da serpente de cinto. Muito chique. Muito mal-educado."),
                ("A guard turned round to face the thing it guards. That is all the shadow ever does.", "Um guarda virado pra coisa que ele guardava. É só isso que a sombra faz."),
                ("Its tail is a staircase! A pointy staircase!", "O rabo dele é uma escada! Uma escada pontuda!"));

            Page(entries, "enemy_mountain_dimmed_ushabti",
                "The little answering figures ride the night barge so that, when somebody is called to work the fields of the other world, they can answer in his place. Each one has glyphs down its front that say whose it is. Among the Coils of Mehen the shadow called them, and the glyphs went out one by one, until they answered only to it. They swing their hoes slowly. Step back from the swing and strike after it.",
                "As figurinhas que respondem viajam na barca da noite pra que, quando alguém for chamado pra trabalhar nos campos do outro mundo, elas respondam no lugar dele. Cada uma tem hieróglifos na frente dizendo de quem é. Entre as voltas de Mehen a sombra chamou elas, e os hieróglifos foram se apagando um por um, até elas só responderem pra ela. Batem a enxada devagar. Recuem do golpe e ataquem depois dele.",
                ("Back off from the hoe, then hit it on the way back up.", "Recua da enxada e bate na volta dela."),
                ("One glyph is still lit at the bottom. Somebody's name is still in there.", "Ainda tem um hieróglifo aceso lá embaixo. O nome de alguém ainda tá ali."),
                ("A little worker that answers whoever calls. I'd call it to do my chores.", "Um trabalhadorzinho que responde pra quem chamar. Eu chamava pra fazer minhas tarefas."),
                ("It forgot whose it was. Remembering whose you are is the whole fight, Vó says.", "Ela esqueceu de quem era. Lembrar de quem a gente é é a luta toda, a Vó diz."),
                ("Its basket is empty. Should we put a seed in it?", "A cestinha dela tá vazia. A gente põe uma semente?"));

            // -- Lago de Fogo, Beira Fervente, Praia de Brasa (75) --
            Page(entries, "enemy_desert_shadow_salamander",
                "In the stories salamanders live in the fire and do not burn. These came out of the Lake of Fire, where the night of the river burns away what is wicked, and the shadow they carried out with them did not burn either; it only got hotter. They run fast on the hot sand and bite with mouths that still glow. Do not chase them across the embers; let them come, and hit them as they turn.",
                "Nas histórias as salamandras vivem no fogo e não se queimam. Essas saíram do Lago de Fogo, onde a noite do rio queima o que é ruim, e a sombra que elas trouxeram junto também não queimou; só ficou mais quente. Correm rápido na areia quente e mordem com a boca ainda acesa. Não corram atrás delas pelas brasas; deixem elas virem, e batam quando elas virarem.",
                ("Let it come to you, then hit it on the turn.", "Deixa ela vir, depois bate na virada."),
                ("They must be so tired of being hot. Let's cool them down, gently.", "Elas devem estar cansadas de tanto calor. Vamos esfriar elas, com jeito."),
                ("It's got a campfire on its back. Portable campfire!", "Ela tem uma fogueira nas costas. Fogueira portátil!"),
                ("The fire should have cleaned the shadow off them. Some things you have to give up yourself.", "O fogo devia ter limpado a sombra delas. Tem coisa que a gente mesmo é que tem que largar."),
                ("Its mouth is glowing like a lantern! Does it eat lanterns?", "A boca dela brilha que nem lanterna! Ela come lanterna?"));

            Page(entries, "enemy_cave_boiled_shadow",
                "Where the Lake of Fire laps the shore, the shadows that fall in come out boiled: soft, hot and in a very bad mood. They sit on the shore bubbling and spit gobs of hot shadow at whoever walks past, from a good way off. Do not stand in front of it while it swells. Wait for it to spit, then go in.",
                "Onde o Lago de Fogo bate na margem, as sombras que caem lá dentro saem fervidas: moles, quentes e de péssimo humor. Ficam na beira borbulhando e cospem bolotas de sombra quente em quem passa, lá de longe. Não fiquem na frente dela enquanto ela incha. Esperem ela cuspir, depois entrem.",
                ("Wait for the spit, then go in.", "Espera o cuspe, depois entra."),
                ("It fell into the fire and came out angry. I'd be angry too.", "Ela caiu no fogo e saiu brava. Eu também ia ficar."),
                ("It smells like burnt beans. Exactly like my cooking.", "Tem cheiro de feijão queimado. Igualzinho à minha comida."),
                ("It keeps a coal inside to keep itself boiling. It would rather stay angry than cool down.", "Ela guarda uma brasa dentro pra continuar fervendo. Prefere ficar brava a esfriar."),
                ("It's like soup! Angry soup!", "Parece sopa! Sopa brava!"));

            Page(entries, "enemy_forest_cracked_ushabti",
                "Red clay figures were sent down to the shore of the Lake of Fire to carry water for the barge. The heat cracked them, and the fire got in through the cracks; now they carry that instead of the water, and their jars are dry. They are heavy and slow and hit hard with their copper hoes. Keep moving round them and strike their backs.",
                "As figuras de barro vermelho foram mandadas pra beira do Lago de Fogo pra carregar água pra barca. O calor rachou elas, e o fogo entrou pelas rachaduras; agora carregam ele em vez da água, e os jarros estão secos. São pesadas e lentas e batem forte com a enxada de cobre. Fiquem rodeando e batam nas costas.",
                ("Slow and heavy. Circle it, hit its back.", "Lenta e pesada. Rodeia, bate nas costas."),
                ("Its jar is empty. It walked all that way and forgot the water.", "O jarro dela tá vazio. Andou isso tudo e esqueceu a água."),
                ("Cracked all over and still working. Respect.", "Toda rachada e ainda trabalhando. Respeito."),
                ("The fire comes in where we are cracked. So does the light, if we let it.", "O fogo entra onde a gente tá rachado. A luz também, se a gente deixar."),
                ("It's like a pot that's angry!", "Parece um pote bravo!"));

            // -- Banco de Areia de Apep, Rio Seco (76) --
            Page(entries, "enemy_forest_mud_crocodile",
                "At the twelfth hour Apep drinks the river and leaves the barge on a bank of mud. The crocodiles of the bank lie in that mud so long that they turn the colour of it, and the shadow found them there, waiting for whatever runs aground. They lie still until you are close, then lunge. Watch the reeds on their backs; when the reeds move, it is coming.",
                "Na décima segunda hora Apep bebe o rio e deixa a barca num banco de lama. Os crocodilos do banco ficam deitados naquela lama tanto tempo que ficam da cor dela, e a sombra achou eles ali, esperando o que encalhar. Ficam parados até vocês chegarem perto, aí dão o bote. Olhem os juncos nas costas deles; quando os juncos mexem, ele vem.",
                ("When the reeds on its back move, step aside.", "Quando os juncos nas costas mexerem, sai do lado."),
                ("It's been lying in the mud so long it forgot the water.", "Ele ficou tanto tempo na lama que esqueceu da água."),
                ("I thought it was a log. The log thought I was lunch.", "Eu achei que era um tronco. O tronco achou que eu era almoço."),
                ("It waits for things to run aground. The shadow is patient like that.", "Ele espera as coisas encalharem. A sombra é paciente assim."),
                ("It has plants growing on it! It's a garden crocodile!", "Tem planta crescendo nele! É um crocodilo-jardim!"));

            // -- Sombras Atrasadas, Areia das Horas (78) --
            Page(entries, "enemy_mountain_clock_ushabti",
                "In the Desert of the Clock the hours run slow in one place and fast in the next, so the people of the Margins set sandstone figures to count them, each with a sundial on its chest. The shadow made them count wrong, and now they keep everyone else's time wrong with theirs; near them a moment lasts too long. From afar they throw the dial's stolen sunlight like a bolt; get under their guard and strike between two shots, while the dial turns.",
                "No Deserto do Relógio as horas correm devagar num lugar e rápido no outro, então o povo das Margens pôs figuras de arenito pra contar elas, cada uma com um relógio de sol no peito. A sombra fez elas contarem errado, e agora deixam o tempo de todo mundo errado junto com o delas; perto delas um instante dura demais. De longe elas atiram a luz de sol roubada do mostrador; cheguem perto e batam entre um tiro e outro, enquanto o mostrador vira.",
                ("Close in between two shots, while the dial turns.", "Chega perto entre um tiro e outro, enquanto o mostrador vira."),
                ("It just wants to tell the time. Somebody gave it the wrong one.", "Ela só quer dizer a hora. Alguém deu a hora errada pra ela."),
                ("Finally someone later than me.", "Finalmente alguém mais atrasado que eu."),
                ("Its shadow points to the wrong hour. A shadow is a bad thing to tell the time by.", "A sombra dela aponta a hora errada. Sombra é uma coisa ruim pra marcar o tempo."),
                ("It has a clock on its tummy! What time is it, clock?", "Ela tem um relógio na barriga! Que horas são, relógio?"));

            // -- Poças de Piche, Juncal de Piche (79) --
            Page(entries, "enemy_mountain_pitch_smoke",
                "The bitumen wells breathe a black smoke all day. Most of it goes up and away; the part the shadow got into stays low and heavy, dripping pitch, and comes at whoever walks the wells in clouds of three and four. A spark from the lamps got caught in one of them once, and they have been afraid of fire ever since. Burn them, or keep moving so the pitch does not stick.",
                "Os poços de betume soltam uma fumaça preta o dia inteiro. Quase toda ela sobe e vai embora; a parte em que a sombra entrou fica baixa e pesada, pingando piche, e vem pra cima de quem anda pelos poços em nuvens de três, quatro. Uma faísca das lanternas ficou presa numa delas uma vez, e desde então elas morrem de medo de fogo. Queimem, ou fiquem andando pro piche não grudar.",
                ("Keep moving so the pitch doesn't stick. Fire helps.", "Fica andando pro piche não grudar. Fogo ajuda."),
                ("It's so heavy it can't fly away. Poor thing.", "Ela é tão pesada que não consegue ir embora. Coitada."),
                ("I stepped in it. My boot stepped out without me.", "Eu pisei nela. Minha bota saiu sem mim."),
                ("It holds a spark and is afraid of it. Most of the dark is like that.", "Ela segura uma faísca e tem medo dela. Quase toda a treva é assim."),
                ("It's raining from it! Black rain!", "Tá chovendo dela! Chuva preta!"));

            // -- Cova da Fumaça, Ventiladores dos Artífices (84) --
            Page(entries, "enemy_forest_choking_smoke",
                "The old books say the Darkness is made of five things, and Smoke is the first of them. The Pit of Smoke is full of it, and some of it is thick enough to hold a shape. It wraps round whoever comes near and squeezes, the way smoke squeezes a lamp until it goes out. Do not let it close round you; fight it from where the fans of the Artificers blow, where it is thin.",
                "Os livros antigos dizem que a Treva é feita de cinco coisas, e a Fumaça é a primeira delas. A Cova da Fumaça é cheia dela, e um pouco dela é grossa o bastante pra ter forma. Ela se enrola em quem chega perto e aperta, do jeito que a fumaça aperta uma lamparina até apagar. Não deixem ela fechar em volta de vocês; lutem de onde os ventiladores dos Artífices sopram, onde ela é fina.",
                ("Fight it where the fans blow. Thin smoke is weak smoke.", "Luta onde os ventiladores sopram. Fumaça fina é fumaça fraca."),
                ("It squeezes the light because it doesn't know how to hold it.", "Ela aperta a luz porque não sabe segurar."),
                ("It hugged me. Worst hug of my life.", "Ela me abraçou. Pior abraço da minha vida."),
                ("Smoke has no shape of its own. It only borrows the shape of what it chokes.", "A fumaça não tem forma própria. Só empresta a forma do que ela sufoca."),
                ("It has two little eyes in the cloud! Hello, cloud!", "Ela tem dois olhinhos na nuvem! Oi, nuvem!"));

            // -- Cova do Lodo, Salões Alagados (87) --
            Page(entries, "enemy_cave_dark_shoal",
                "In the Pit of Mire the fish swim in black water, and they had never seen a light until the shadow brought one down. Now they swim round it all together, passing it from mouth to mouth so none of them has to hold it long, and they bite all together at whoever brings a lamp of their own, a dozen little mouths at once. They come in a swirl; do not let them surround you, and hit the middle, where the light is.",
                "Na Cova do Lodo os peixes nadam em água preta, e nunca tinham visto uma luz até a sombra trazer uma lá pra baixo. Agora nadam todos juntos em volta dela, passando de boca em boca pra nenhum ter que segurar muito tempo, e mordem todos juntos quem traz uma lamparina própria, uma dúzia de boquinhas de uma vez. Vêm num redemoinho; não deixem eles cercarem vocês, e batam no meio, onde tá a luz.",
                ("Hit the middle of the swirl, where the light is.", "Bate no meio do redemoinho, onde tá a luz."),
                ("They share the light. That part I like.", "Eles dividem a luz. Essa parte eu gostei."),
                ("A whole school of fish, and none of them went to school.", "Um cardume inteiro, e nenhum foi pra escola."),
                ("They never saw a light, and the first one they saw was stolen. No wonder they guard it.", "Nunca viram uma luz, e a primeira que viram era roubada. Não admira que guardem."),
                ("They're dancing in a circle! Can I dance too?", "Eles tão dançando de roda! Eu posso dançar também?"));

            // -- Cova da Treva, Túneis (88) --
            Page(entries, "enemy_forest_blind_serpent",
                "In the Pit of Darkness there is nothing to see, so the serpents stopped making eyes. They find you by the warmth of your body, with two little pits on the snout that glow when they feel it, and the shadow taught them that a hero is the warmest thing in the pit. They strike fast and pull back. Stay still when they rear; they lose you for a moment when you stop.",
                "Na Cova da Treva não tem nada pra ver, então as serpentes pararam de fazer olhos. Elas acham vocês pelo calor do corpo, com duas covinhas no focinho que brilham quando sentem, e a sombra ensinou que um herói é a coisa mais quente da cova. Dão o bote rápido e recuam. Fiquem parados quando ela empinar; ela perde vocês por um instante quando vocês param.",
                ("Freeze when it rears. It loses you for a moment.", "Fica parado quando ela empinar. Ela te perde um instante."),
                ("It can't see us at all. It only knows we're warm.", "Ela não enxerga a gente. Só sabe que a gente é quente."),
                ("Finally a reason to be cool.", "Finalmente um motivo pra ser frio."),
                ("It finds us by our warmth. The shadow always goes after whatever is alive in us.", "Ela acha a gente pelo calor. A sombra sempre vai atrás do que tem de vivo na gente."),
                ("It has no eyes! How does it know where its tail is?", "Ela não tem olho! Como ela sabe onde tá o rabo dela?"));

            // -- Praça dos Pedestais, Portão da Terra da Treva (90) --
            Page(entries, "enemy_mountain_giant_dimmed_ushabti",
                "The Artificers set the biggest of the answering figures before the Gate of the Land of Darkness, to keep it shut whenever they were called. For a long time they answered. Then the shadow on the other side of the Gate called louder, and their glyphs went dark. They are huge and slow and their hoes come down like a door slamming. Never be under the hoe; strike while it lifts it again.",
                "Os Artífices puseram as maiores figuras que respondem diante do Portão da Terra da Treva, pra manter ele fechado sempre que fossem chamadas. Por muito tempo elas responderam. Aí a sombra do outro lado do Portão chamou mais alto, e os hieróglifos delas apagaram. São enormes e lentas e a enxada delas desce que nem porta batendo. Nunca fiquem debaixo da enxada; batam enquanto ela levanta de novo.",
                ("Never under the hoe. Strike while it lifts it.", "Nunca debaixo da enxada. Bate enquanto ela levanta."),
                ("It kept the Gate for so long. It must be very tired.", "Ela guardou o Portão tanto tempo. Deve estar muito cansada."),
                ("It's the size of a house and it still answers to someone else. Be your own boss, big guy.", "Ela é do tamanho de uma casa e ainda obedece outro. Seja seu próprio chefe, grandona."),
                ("It answered whoever called loudest. The light does not shout; you have to listen for it.", "Ela respondeu pra quem gritou mais alto. A luz não grita; tem que escutar."),
                ("It's so tall I can't see its head! Hello up there!", "Ela é tão alta que eu não vejo a cabeça! Oi aí em cima!"));
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
