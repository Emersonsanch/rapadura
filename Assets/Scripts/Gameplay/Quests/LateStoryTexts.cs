using System.Collections.Generic;
using System.Linq;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Characters;

namespace Rapadura.Gameplay.Quests
{
    /// <summary>
    /// Localization (en + pt-BR) of "As Cartas da Estrada" (<see cref="LateStoryCatalog"/>, Docs/HistoriaFinal_Cartas.md): per step the title, the description (a letter of
    /// Vô Jorge), the objectives, the journal (Vó Edna on the radio), the progress line and one offer per cousin; and, for the whole chain, one "too low" and one closing
    /// line per cousin. Keys <c>quest.late_story_*</c>; carried by <c>CityQuestTexts</c>, which every loader of quest texts already goes through.
    /// <see cref="All"/> also carries the texts of "A Lua que Não Enche" (<see cref="MidStoryTexts"/>, keys <c>quest.mid_story_*</c>).
    /// </summary>
    public static class LateStoryTexts
    {
        /// <summary>The order of the five offers of a step.</summary>
        private static readonly CharacterId[] OfferOrder = { CharacterId.Joaquim, CharacterId.Maria, CharacterId.Maithe, CharacterId.Icaro, CharacterId.Lavine };

        public static readonly List<LocalizationEntry> All = Build();

        public static void EnsureLoaded(LocalizationManager localization)
        {
            if (localization == null)
            {
                return;
            }

            localization.LoadEntries(All.Where(entry => !localization.HasKey(entry.Key)).ToList());
        }

        /// <summary>pt-BR text of <paramref name="key"/> without a manager, or null.</summary>
        public static string GetFallback(string key) => All.FirstOrDefault(e => e.Key == key)?.PtBR;

        internal static (string En, string Pt) T(string en, string pt) => (en, pt);

        internal static void Step(List<LocalizationEntry> all, string id, (string En, string Pt) title, (string En, string Pt) description, (string En, string Pt) reach,
            (string En, string Pt)? kill, (string En, string Pt) journal, (string En, string Pt) active, params (string En, string Pt)[] offers)
        {
            string key = "quest." + id;
            all.Add(new LocalizationEntry(key + ".title", title.En, title.Pt));
            all.Add(new LocalizationEntry(key + ".description", description.En, description.Pt));
            all.Add(new LocalizationEntry(key + ".obj_" + LateStoryCatalog.ReachObjectiveId, reach.En, reach.Pt));
            if (kill.HasValue)
            {
                all.Add(new LocalizationEntry(key + ".obj_" + LateStoryCatalog.KillObjectiveId, kill.Value.En, kill.Value.Pt));
            }

            all.Add(new LocalizationEntry(key + ".journal", journal.En, journal.Pt));
            all.Add(new LocalizationEntry(key + ".active", active.En, active.Pt));
            for (int i = 0; i < OfferOrder.Length; i++)
            {
                all.Add(new LocalizationEntry(key + ".offer." + LateStoryCatalog.HeroKey(OfferOrder[i]), offers[i].En, offers[i].Pt));
            }
        }

        private static List<LocalizationEntry> Build()
        {
            var all = new List<LocalizationEntry>();

            // ================= Região 9, Estepe dos Nartes =================
            Step(all, "late_story_r09_foals",
                T("The First Letter", "A Primeira Carta"),
                T("In the lining of the Caderno, Grandma Edna found a note in Grandpa Jorge's crooked hand: eight letters, left with friends up the north road, for whoever got far enough. The first one is with Soslan, and his racing track is full of young griffins.",
                  "No forro do Caderno, a Vó Edna achou um bilhete com a letra torta do Vô Jorge: oito cartas, deixadas com amigos na estrada do norte, para quem chegasse longe. A primeira está com o Soslan, e a pista de corrida dele está cheia de grifos jovens."),
                T("Reach the racing tracks of the camp", "Chegar às pistas de corrida do acampamento"),
                T("Free the young griffins that scare the foals", "Libertar os grifos jovens que assustam os potros"),
                T("Grandma Edna read the first letter over the radio and laughed the whole way through. He says he lost a race here to a foal. She says it was two foals. Five of us, one letter, and she cried a little at the end and said it was the onion.",
                  "A Vó Edna leu a primeira carta pelo rádio e riu do começo ao fim. Ele diz que perdeu uma corrida aqui para um potro. Ela diz que foram dois potros. Somos cinco, a carta é uma só, e no fim ela chorou um pouquinho e disse que era a cebola."),
                T("The griffins nest where the track bends. The letter is in the saddlebag I dropped running from them.",
                  "Os grifos fazem ninho na curva da pista. A carta está no alforje que eu derrubei fugindo deles."),
                T("Joaquim! You have your grandfather's shoulders and none of his sense. Go stand in front of those griffins, it's what you do. There's a letter of his at the bend.",
                  "Joaquim! Você tem os ombros do seu avô e nenhum juízo dele. Vai ficar na frente daqueles grifos, que é o que você faz. Tem uma carta dele na curva da pista."),
                T("Maria, they say you hear what's calling before the rest of us. Then you already know why Jorge left a letter on my track. The griffins forgot they were young. Remind them.",
                  "Maria, dizem que você ouve o que chama antes de todo mundo. Então já sabe por que o Jorge deixou uma carta na minha pista. Os grifos esqueceram que são jovens. Lembre eles."),
                T("Maithe, I saw you count your cousins from the gate. Good. Count five griffins for me on the track, down wind, and bring me back what Jorge left in the saddlebag.",
                  "Maithe, vi você contar os primos lá do portão. Bom. Conta cinco grifos para mim na pista, a favor do vento, e me traz o que o Jorge deixou no alforje."),
                T("Ícaro, your grandfather swore a foal could be timed with a water clock. Mine stopped. Fix the track first: five griffins off it, then measure anything you like.",
                  "Ícaro, o seu avô jurava que dava para cronometrar potro com relógio de água. O meu parou. Primeiro arruma a pista: cinco grifos fora dela, depois mede o que quiser."),
                T("Lavine, Jorge's letter says the youngest will read it first. I don't argue with letters. The griffins are at the bend, and no, nobody here thinks you're small.",
                  "Lavine, a carta do Jorge diz que a mais nova vai ler primeiro. Eu não discuto com carta. Os grifos estão na curva, e não, ninguém aqui acha você pequena."));

            Step(all, "late_story_r09_kurgans",
                T("Under the Golden Mound", "Debaixo do Kurgan de Ouro"),
                T("Grandpa Jorge's first letter ends in the middle of a sentence. The rest, Satana says, he buried under a guardian lamp at the golden kurgans, and bronze wolves dimmed by the shadow have been circling it ever since.",
                  "A primeira carta do Vô Jorge termina no meio de uma frase. O resto, diz a Satana, ele enterrou debaixo de uma lâmpada guardiã nos kurgans de ouro, e lobos de bronze apagados pela sombra andam em volta dela desde então."),
                T("Reach the golden kurgans", "Chegar aos kurgans de ouro"),
                T("Free the bronze wolves that circle the lamp", "Libertar os lobos de bronze que rondam a lâmpada"),
                T("The end of the sentence was: and that is why you should always pack a second pair of socks. Grandma Edna said over the radio that he never once packed the first pair. Then she asked which of us was wearing socks.",
                  "O fim da frase era: e é por isso que sempre se leva um segundo par de meias. A Vó Edna disse pelo rádio que ele nunca levou nem o primeiro. Depois perguntou quem de nós estava de meia."),
                T("Six wolves round one lamp, and the lamp is losing. Walk softly near the mound.",
                  "Seis lobos em volta de uma lâmpada, e a lâmpada está perdendo. Andem devagar perto do kurgan."),
                T("Joaquim, sit, eat something, and then go and shout at six bronze wolves for me. You'll enjoy it, and my lamp will finally get some sleep.",
                  "Joaquim, senta, come alguma coisa e depois vai gritar com seis lobos de bronze para mim. Você vai gostar, e a minha lâmpada finalmente vai dormir."),
                T("Maria, I keep a lamp for everyone who leaves this camp. Your grandfather asked me to keep his under the gold, for when you came. Go and bring its light back from the wolves.",
                  "Maria, eu acendo uma lâmpada para cada um que sai deste acampamento. O seu avô me pediu para deixar a dele debaixo do ouro, para quando você viesse. Vai trazer a luz dela de volta dos lobos."),
                T("Maithe, wolves don't like being counted. Six of them, round the mound, and the wind there changes twice before noon. Your eye will tell you the rest.",
                  "Maithe, lobo não gosta de ser contado. Seis deles, em volta do kurgan, e o vento lá muda duas vezes antes do meio-dia. O teu olho conta o resto."),
                T("Ícaro, those wolves are bronze from the teeth in. Someone made them, and someone forgot them. Bring me back six that remember, and bring your notebook.",
                  "Ícaro, esses lobos são de bronze dos dentes para dentro. Alguém fez, e alguém esqueceu. Me traz seis que lembrem, e leva o teu caderno."),
                T("Lavine, you read the first page. So you know he stopped in the middle. Whoever stops in the middle wants someone to keep going. Six wolves, then the rest of the sentence.",
                  "Lavine, você leu a primeira página. Então sabe que ele parou no meio. Quem para no meio quer que alguém continue. Seis lobos, e depois o resto da frase."));

            // ================= Região 10, Picos de Farnkesh =================
            Step(all, "late_story_r10_bridges",
                T("Tied to the Wind", "Amarrada ao Vento"),
                T("Grandpa Jorge's next letter hangs on the rope bridges above Farnkesh, tied there, Tadi says, by Jorge himself on a dare. Rope ushabtis have been re-tying every knot on the bridges, and the letter is now somewhere inside the knots.",
                  "A carta seguinte do Vô Jorge está pendurada nas pontes de corda acima de Farnkesh, amarrada lá, diz a Tadi, pelo próprio Jorge numa aposta. Ushabtis de corda andam refazendo todos os nós das pontes, e a carta agora está em algum lugar dentro dos nós."),
                T("Reach the Bridges of Wind", "Chegar às Pontes de Vento"),
                T("Free the rope ushabtis that tie the bridges", "Libertar os ushabtis de corda que amarram as pontes"),
                T("The letter said the bridges were safe, and that he crossed them singing. Grandma Edna said on the radio that he crossed them on his knees, because she was there. Nobody knows which of them to believe. Maithe says both.",
                  "A carta dizia que as pontes são seguras, e que ele atravessou cantando. A Vó Edna disse pelo rádio que ele atravessou de joelhos, porque ela estava lá. Ninguém sabe em qual dos dois acreditar. A Maithe diz que nos dois."),
                T("They re-tie what you untie. Break the ones doing the tying.",
                  "Eles reamarram o que vocês desamarram. Quebrem os que estão amarrando."),
                T("Joaquim, those bridges hold everyone but a boy who won't stop to think. Six ushabtis, and please cross the planks, not the air.",
                  "Joaquim, essas pontes aguentam todo mundo, menos um menino que não para para pensar. Seis ushabtis, e por favor passe pelas tábuas, não pelo ar."),
                T("Maria, the ushabtis were made to answer, and now they only tie knots. Your grandfather wrote that a thing that forgot its name still wants to be called. Call six of them for me.",
                  "Maria, os ushabtis foram feitos para responder, e agora só dão nó. O seu avô escreveu que quem esqueceu o nome ainda quer ser chamado. Chame seis deles para mim."),
                T("Maithe, on my bridges the wind tells lies. You know that one already. Six rope ushabtis, and trust whoever is behind you on the planks.",
                  "Maithe, nas minhas pontes o vento mente. Essa você já conhece. Seis ushabtis de corda, e confia em quem vem atrás de você nas tábuas."),
                T("Ícaro, those knots are a pattern, I'm sure of it, and I've been sure for thirty years. Break six of the knotters and copy down what's left. Your grandfather never could.",
                  "Ícaro, esses nós são um padrão, eu tenho certeza, e tenho essa certeza há trinta anos. Quebra seis dos que dão nó e copia o que sobrar. O teu avô nunca conseguiu."),
                T("Lavine, your grandfather tied that letter here because he said only a light person would go out far enough to reach it. I checked your boots. You'll do. Six ushabtis first.",
                  "Lavine, o seu avô amarrou essa carta aqui porque disse que só gente leve vai longe o bastante para alcançar. Olhei as tuas botas. Você serve. Seis ushabtis primeiro."));

            Step(all, "late_story_r10_bells",
                T("The Bell That Kept a Letter", "O Sino Que Guardou Uma Carta"),
                T("Grandpa Jorge folded the fourth letter inside the clapper of the great bell at the Pináculo. Shad-Ohrmizd cannot ring it: broken lanterns drifted up from the hermitages and sit on the bell, and every lantern dims what it sits on.",
                  "O Vô Jorge dobrou a quarta carta dentro do badalo do sino grande do Pináculo. O Shad-Ohrmizd não consegue tocar: lanternas quebradas subiram das ermidas e sentam no sino, e toda lanterna apaga o que senta nela."),
                T("Reach the Pinnacle of the Bell", "Chegar ao Pináculo do Sino"),
                T("Free the broken lanterns that sit on the bell", "Libertar as lanternas quebradas que sentam no sino"),
                T("Grandma Edna asked us to hold the radio up to the bell when it rang. It rang. She said nothing for a while. Then she said: he always was late for dinner. She kept the radio on for a long time after that.",
                  "A Vó Edna pediu para a gente segurar o rádio perto do sino quando tocasse. Tocou. Ela ficou um tempo sem falar. Depois disse: ele sempre chegou atrasado para o jantar. E deixou o rádio ligado por muito tempo depois disso."),
                T("A bell with a lantern on it only says tock. Clear six, and it says the rest.",
                  "Sino com lanterna em cima só diz toc. Tirem seis, e ele diz o resto."),
                T("Joaquim, you may hit the lanterns as hard as you like. Six of them. You may not hit the bell, your grandfather already tried that and it cost him a tooth.",
                  "Joaquim, pode bater nas lanternas com a força que quiser. Seis delas. No sino não, o seu avô já tentou isso e custou um dente."),
                T("Maria, lanterns are only lost lamps. Take six of them down from my bell gently, and tell them your grandfather sent you. Most things up here remember him.",
                  "Maria, lanterna é só lâmpada perdida. Tire seis delas do meu sino com calma, e diga que foi o seu avô quem mandou. Quase tudo aqui em cima lembra dele."),
                T("Maithe, the wind says there are ten lanterns up there. I say six. You can count them from the pinnacle, and then you'll know which of us to believe.",
                  "Maithe, o vento diz que tem dez lanternas lá em cima. Eu digo seis. Você conta do pináculo, e aí vai saber em quem acreditar."),
                T("Ícaro, the clapper has a hollow in it. Your grandfather carved it with a pocketknife and it put the bell out of tune by one note. I've measured. You'll want to as well, after six lanterns.",
                  "Ícaro, o badalo tem um oco. O teu avô cavou com canivete e desafinou o sino em uma nota. Eu medi. Você vai querer medir também, depois de seis lanternas."),
                T("Lavine, it's very high and very dark and everyone asked me whether you should go. I said ask her. So. Six lanterns, Lavine. Are you going?",
                  "Lavine, é muito alto e muito escuro e todo mundo me perguntou se você devia ir. Eu disse pergunta para ela. Então. Seis lanternas, Lavine. Você vai?"));

            // ================= Região 11, Mar Gelado =================
            Step(all, "late_story_r11_canals",
                T("The Pike and the Tin", "O Lúcio e a Lata"),
                T("Grandpa Jorge kept the fifth letter in a tin on the ice of the canals, for a joke. Then the ice moved, and Yirkap watched a giant pike swallow the tin in one bite. It has to come out of a pike, and the canals are full of them.",
                  "O Vô Jorge guardou a quinta carta numa lata no gelo dos canais, de brincadeira. Aí o gelo mexeu, e o Yirkap viu um lúcio gigante engolir a lata de uma bocada. Tem que sair de dentro de um lúcio, e os canais estão cheios deles."),
                T("Reach the Ice Canals", "Chegar aos Canais de Gelo"),
                T("Free the giant pike of the canals", "Libertar os lúcios gigantes dos canais"),
                T("The letter smells of fish and Grandma Edna made Lavine read it twice, for the pleasure of hearing her complain. He says the cold here is honest. She says he wore her good scarf for this and never brought it back.",
                  "A carta tem cheiro de peixe e a Vó Edna fez a Lavine ler duas vezes, pelo gosto de ouvir ela reclamar. Ele diz que o frio daqui é honesto. Ela diz que ele levou o cachecol bom dela e nunca trouxe de volta."),
                T("Pike come up where the ice is thin. So do cousins. Keep your weight spread.",
                  "Lúcio aparece onde o gelo é fino. Primo também. Espalhem o peso."),
                T("Joaquim, you are the only one here heavy enough to be good bait. Six pike, on the canal, and if the ice cracks, stand still and shout for your sister.",
                  "Joaquim, você é o único aqui pesado o bastante para ser boa isca. Seis lúcios, no canal, e se o gelo rachar fica parado e grita pela sua irmã."),
                T("Maria, one of those pike has a tin in it, and in the tin is your grandfather's handwriting. The fish doesn't know. Free six and we will find which one was carrying it.",
                  "Maria, um desses lúcios tem uma lata dentro, e na lata está a letra do seu avô. O peixe não sabe. Liberte seis e a gente acha qual estava carregando."),
                T("Maithe, I'll lend you my skis. No? Then stay off the thin ice and shoot them through the holes. Six pike, and I want to watch you do it.",
                  "Maithe, te empresto os esquis. Não? Então fica longe do gelo fino e atira pelos buracos. Seis lúcios, e eu quero ver."),
                T("Ícaro, your grandfather bet me the tin would float. It sank. Tell me how far a pike carries a tin in a month, then we'll know which six to go after.",
                  "Ícaro, o teu avô apostou comigo que a lata ia boiar. Afundou. Me diz até onde um lúcio carrega uma lata em um mês, e a gente sabe atrás de quais seis ir."),
                T("Lavine, you are the first person to look at a pike and tell me it's ugly and be right. Go and tell six more. Mind the holes, they don't care how big anyone is.",
                  "Lavine, você é a primeira pessoa a olhar um lúcio e dizer que é feio e ter razão. Vai contar para mais seis. Cuidado com os buracos, que eles não ligam para o tamanho de ninguém."));

            Step(all, "late_story_r11_galleries",
                T("Where the Mammoths Sleep", "Onde os Mamutes Dormem"),
                T("Grandpa Jorge spent one whole winter in the galleries of the mammoths, Pera says, snowed in with a giant and a very poor deck of cards. The sixth letter is still there, and the mammoths have dug out of their sleep and gone dim.",
                  "O Vô Jorge passou um inverno inteiro nas galerias dos mamutes, diz o Pera, preso pela neve com um gigante e um baralho muito ruim. A sexta carta ainda está lá, e os mamutes acordaram cavando e ficaram apagados."),
                T("Reach the Mammoth Galleries", "Chegar às Galerias dos Mamutes"),
                T("Free the mammoths that dug out dim", "Libertar os mamutes que acordaram apagados"),
                T("Grandma Edna says that winter he sent her one letter in five months and it was about cards. She read this one all the way down, then said: well. He got here in the end. She didn't say where here was.",
                  "A Vó Edna diz que naquele inverno ele mandou uma carta em cinco meses, e era sobre baralho. Leu esta até o fim, e disse: bom. Ele chegou aqui, afinal. Não disse onde era aqui."),
                T("Four mammoths, and they are big, even for me. Do not stand in the galleries' narrow part.",
                  "Quatro mamutes, e são grandes, até para mim. Não fiquem na parte estreita das galerias."),
                T("Joaquim, I am Pera, and I carried your grandfather out of here on one shoulder. Four mammoths, Joaquim. Even you can step aside for a mammoth. I'll know if you didn't.",
                  "Joaquim, eu sou o Pera, e carreguei o seu avô daqui em um ombro só. Quatro mamutes, Joaquim. Até você pode sair da frente de um mamute. Eu vou saber se não saiu."),
                T("Maria, they weren't bad animals. They slept next to him all winter and kept him warm. Go down and tell four of them it is morning. Your light will do the telling.",
                  "Maria, não eram bichos ruins. Dormiram do lado dele o inverno inteiro e esquentaram ele. Desce e conta para quatro deles que já amanheceu. A tua luz conta."),
                T("Maithe, your grandfather couldn't hit a mammoth at ten paces, and I know, I stood behind him. Four of them, from the far end of the gallery. Show me how it's done.",
                  "Maithe, o teu avô não acertava um mamute a dez passos, e eu sei, eu estava atrás dele. Quatro, do fundo da galeria. Mostra como se faz."),
                T("Ícaro, the galleries were dug by something older than mammoths, and your grandfather drew a map of them with a burnt stick. It's wrong. Four mammoths, and then correct it.",
                  "Ícaro, as galerias foram cavadas por algo mais velho que mamute, e o teu avô desenhou um mapa delas com graveto queimado. Está errado. Quatro mamutes, depois corrige."),
                T("Lavine, the dark down there is the soft kind, I promise. Your grandfather slept in it for a season and came out telling lies about it. Four mammoths. I'll be right behind you.",
                  "Lavine, o escuro lá embaixo é do macio, prometo. O teu avô dormiu nele uma estação e saiu contando mentira. Quatro mamutes. Eu fico logo atrás de você."));

            // ================= Região 12, Arquipélago de Hinokura =================
            Step(all, "late_story_r12_gardens",
                T("Letter in Lacquer", "Carta na Laca"),
                T("Mestra Akane had Grandpa Jorge's seventh page lacquered into a screen in the lava gardens: he paid for a tea with it. Shisa dimmed by the volcano's smoke have taken over the garden and will not let anyone near the screen.",
                  "A Mestra Akane mandou laquear a sétima folha do Vô Jorge num biombo dos jardins de lava: ele pagou um chá com ela. Shisas apagadas pela fumaça do vulcão tomaram o jardim e não deixam ninguém chegar perto do biombo."),
                T("Reach the Lacquered Lava Gardens", "Chegar aos Jardins de Lava Laqueada"),
                T("Free the dimmed shisa of the garden", "Libertar as shisas apagadas do jardim"),
                T("Grandma Edna says he never had the money for tea in his life. Then she went quiet and told us he drank tea sitting on the porch every morning with his eyes closed, and that she had always thought he was only pretending to sleep.",
                  "A Vó Edna diz que ele nunca teve dinheiro para chá na vida. Depois ficou quieta e contou que ele tomava chá na varanda toda manhã de olhos fechados, e que ela sempre achou que ele fingia que dormia."),
                T("Shisa guard the door of what they love. They have forgotten what they love. Be kind about it.",
                  "Shisa guarda a porta do que ama. Elas esqueceram o que amam. Sejam gentis com isso."),
                T("Joaquim, a shisa is a guardian, like you, and like you it does not know when to stop. Six of them, in my garden. Do not step on the moss, it took forty years.",
                  "Joaquim, shisa é guardiã, como você, e como você não sabe a hora de parar. Seis delas, no meu jardim. Não pisa no musgo, levou quarenta anos."),
                T("Maria, I lacquered your grandfather's page because the words did not want to go out. Lacquer is only a slow way of keeping a light. Six shisa, and we shall see what it says.",
                  "Maria, eu laqueei a página do seu avô porque as palavras não queriam apagar. Laca é só um jeito lento de guardar uma luz. Seis shisas, e a gente vê o que ela diz."),
                T("Maithe, my garden has no wind in it at all, and you are already frowning. Good. Six shisa, and listen to your own feet.",
                  "Maithe, meu jardim não tem vento nenhum, e você já está de testa franzida. Bom. Seis shisas, e escuta os teus próprios pés."),
                T("Ícaro, I have a screen with a letter in it, and the letter is inside seven coats. You will want to know how. First: six shisa. Then a very long talk about varnish.",
                  "Ícaro, tenho um biombo com uma carta dentro, e a carta está debaixo de sete demãos. Você vai querer saber como. Primeiro: seis shisas. Depois uma conversa muito longa sobre verniz."),
                T("Lavine, I do not lacquer for people who pretend. Your grandfather pretended constantly. You, I am told, do not. Six shisa, and you may read the screen first.",
                  "Lavine, eu não laqueio para gente que finge. O teu avô fingia o tempo todo. Você, me contaram, não finge. Seis shisas, e você lê o biombo primeiro."));

            Step(all, "late_story_r12_furnaces",
                T("Nine Furnaces, One Letter", "Nove Fornalhas, Uma Carta"),
                T("Sango carried Grandpa Jorge across the strait once and he paid in a page again, this one to be left at the Nine Furnaces. Furnace ogres have taken the courtyard and sit on it. Grandma Edna says the page is about her, and wants it back, and will not say why.",
                  "A Sango atravessou o Vô Jorge pelo estreito uma vez e ele pagou com uma folha de novo, essa para deixar nas Nove Fornalhas. Ogros de fornalha tomaram o pátio e sentam em cima dela. A Vó Edna diz que a folha é sobre ela, quer de volta e não diz por quê."),
                T("Reach the Courtyard of the Furnaces", "Chegar ao Pátio das Fornalhas"),
                T("Free the furnace ogres of the courtyard", "Libertar os ogros de fornalha do pátio"),
                T("Grandma Edna asked us to read this one in private, and then to read it to her, and then not to tell anyone what it said, and she said that it was a poem, and that it did not rhyme, and that she had kept the first draft.",
                  "A Vó Edna pediu para a gente ler esta em particular, depois ler para ela, e depois não contar para ninguém o que dizia, e disse que era um poema, que não rimava, e que ela guardou o rascunho."),
                T("Ogres like the heat. You won't. Do not stand near the open doors.",
                  "Ogro gosta do calor. Vocês não. Não fiquem perto das portas abertas."),
                T("Joaquim, four ogres, each one twice your size and with a shovel. You will tell me you are not tired afterwards. I will row you home anyway.",
                  "Joaquim, quatro ogros, cada um com duas vezes o teu tamanho e uma pá. Você vai me dizer que não ficou cansado. Eu remo você de volta mesmo assim."),
                T("Maria, your grandmother asked me to bring this one back. Your grandmother has never asked me for a single thing in fifty years. Four ogres, and do it gently if you can.",
                  "Maria, a sua avó me pediu para trazer esta de volta. A sua avó nunca me pediu nada em cinquenta anos. Quatro ogros, e com jeito, se der."),
                T("Maithe, the smoke over the courtyard will hide two of them. I have seen you count. Count the shovels instead.",
                  "Maithe, a fumaça do pátio esconde dois deles. Já vi você contar. Conta as pás."),
                T("Ícaro, the Nine Furnaces were built by someone who knew what they were doing and then left. Four ogres, then you can stand and be amazed for as long as you like.",
                  "Ícaro, as Nove Fornalhas foram feitas por alguém que sabia o que fazia e depois foi embora. Quatro ogros, e aí você fica admirando o tempo que quiser."),
                T("Lavine, it is very hot and very loud and they are very ugly. You will tell them so. Four ogres, and I will have the boat ready if you want to come home quietly after.",
                  "Lavine, é muito quente e muito barulhento e eles são muito feios. Você vai falar isso para eles. Quatro ogros, e eu deixo o barco pronto se quiser voltar quieta depois."));

            // ================= Região 13, Rio das Doze Horas =================
            Step(all, "late_story_r13_papyrus",
                T("Scarabs on the Papyrus", "Escaravelhos no Papiro"),
                T("Grandpa Jorge wrote the ninth page on papyrus, Sinuhe says, because he had run out of everything else, and left it with the boatmen of the papyrus bank. Shadow scarabs are eating the bank from the water up.",
                  "O Vô Jorge escreveu a nona folha em papiro, diz o Sinuhe, porque tinha acabado todo o resto, e deixou com os barqueiros da margem de papiro. Escaravelhos de sombra estão comendo a margem da água para cima."),
                T("Reach the Papyrus Bank", "Chegar à Margem de Papiro"),
                T("Free the shadow scarabs of the bank", "Libertar os escaravelhos de sombra da margem"),
                T("The scarabs had eaten the bottom half of the page. Grandma Edna said on the radio she knew exactly what it said anyway, and told us, and we are not sure she was telling the truth, and we wrote it down.",
                  "Os escaravelhos comeram a metade de baixo da folha. A Vó Edna disse pelo rádio que sabia exatamente o que dizia, e contou, e a gente não tem certeza se era verdade, e anotou assim mesmo."),
                T("Keep to the boards at the bank. What the scarabs have eaten will not hold you.",
                  "Fiquem nas tábuas da margem. O que os escaravelhos comeram não aguenta vocês."),
                T("Joaquim, eight scarabs is a morning's work to a boatman, and a morning's work to you is a bowl of plov and a nap. Off you go. The river is not going to wait for you.",
                  "Joaquim, oito escaravelhos é trabalho de uma manhã para um barqueiro, e trabalho de uma manhã para você é um prato de plov e um cochilo. Vai. O rio não espera."),
                T("Maria, the night is long on this river, and your grandfather said the only way to cross it is to carry something bright and not look down. Eight scarabs, and I'll keep the barque waiting.",
                  "Maria, a noite é longa neste rio, e o seu avô dizia que o único jeito de atravessar é levar algo claro e não olhar para baixo. Oito escaravelhos, e eu seguro a barca."),
                T("Maithe, there are eight scarabs and one of you, and the bank is mud, and your wind doesn't work well down here. Count on your feet, then.",
                  "Maithe, são oito escaravelhos e uma de você, e a margem é lama, e o teu vento não funciona bem aqui embaixo. Conta com os pés, então."),
                T("Ícaro, the page is half eaten. Eaten in a pattern, I think, starting with his name. Eight scarabs, and then tell me what kind of thing bites a name first.",
                  "Ícaro, a folha está meio comida. Comida em padrão, acho, começando pelo nome dele. Oito escaravelhos, e depois me diz que tipo de bicho morde o nome primeiro."),
                T("Lavine, a barge goes down this river every night with nobody at the oars. Nobody has ever asked you to row it. I am not asking either. Eight scarabs, please.",
                  "Lavine, uma barca desce este rio toda noite sem ninguém nos remos. Nunca ninguém te pediu para remar. Eu também não estou pedindo. Oito escaravelhos, por favor."));

            Step(all, "late_story_r13_rowers",
                T("What the Rowers Keep", "O Que os Remadores Guardam"),
                T("The silent rowers of the night keep everything they are given, and Djedi says Grandpa Jorge gave them a letter to hold until five people came asking. They will not hand it over while dimmed ibises stand on their beach.",
                  "Os remadores calados da noite guardam tudo o que recebem, e o Djedi diz que o Vô Jorge deu a eles uma carta para segurar até cinco pessoas virem pedir. Eles não entregam enquanto íbis apagadas estiverem na praia."),
                T("Reach the Beach of the Rowers", "Chegar à Praia dos Remadores"),
                T("Free the dimmed ibises of the beach", "Libertar as íbis apagadas da praia"),
                T("The letter was addressed to the five of us by name, written before Lavine was born. Grandma Edna didn't say anything for some time. Then she said: he told me we would have five. He was always doing that.",
                  "A carta estava endereçada aos cinco, pelo nome, e foi escrita antes da Lavine nascer. A Vó Edna ficou um tempo sem dizer nada. Depois disse: ele me falou que seriam cinco. Ele vivia fazendo isso."),
                T("The rowers do not speak. They will look at you and you will have the feeling you owe them something. You do.",
                  "Os remadores não falam. Vão olhar para vocês, e vocês vão ter a impressão de que devem algo. Devem."),
                T("Joaquim, the rowers are a very great way off from being impressed. Six ibises and try not to step in the river. It is not that kind of river.",
                  "Joaquim, os remadores estão muito longe de ficar impressionados. Seis íbis e tenta não pisar no rio. Não é esse tipo de rio."),
                T("Maria, some people say the rowers carry the light that nobody else will. I can do three tricks with a goose, and I have never once understood them. Six ibises, and be gentle.",
                  "Maria, tem gente que diz que os remadores carregam a luz que mais ninguém carrega. Eu sei três truques com um ganso e nunca entendi eles. Seis íbis, com jeito."),
                T("Maithe, ibises stand on one leg and watch. So do you, from what I hear. Six of them, from a long way back, before they start watching you.",
                  "Maithe, íbis fica num pé só e observa. Você também, pelo que me disseram. Seis delas, de bem longe, antes que elas comecem a te observar."),
                T("Ícaro, I can make a goose lose its head and find it again. I have no idea how. I'm told you would find that intolerable. Six ibises, and I'll show you afterwards.",
                  "Ícaro, eu faço um ganso perder a cabeça e achar de novo. Não tenho ideia de como. Me disseram que você não ia aguentar isso. Seis íbis, e depois eu mostro."),
                T("Lavine, your name is on this letter, in an old hand, from before you were born. I am a magician and I do not know how that was done. Six ibises, and then you may open it.",
                  "Lavine, o teu nome está nesta carta, com letra antiga, de antes de você nascer. Eu sou mágico e não sei como isso foi feito. Seis íbis, e depois você abre."));

            // ================= Região 14, Margens da Mistura =================
            Step(all, "late_story_r14_barley",
                T("Locusts in the Barley", "Gafanhotos na Cevada"),
                T("Utnapishtim has lived through one flood and is not impressed by much. Grandpa Jorge sat on his terraces and left a letter for the cousins in the barley store, and locusts of bitumen are now eating everything between the store and the door.",
                  "O Utnapishtim sobreviveu a uma enchente e não se impressiona com pouco. O Vô Jorge sentou nos terraços dele e deixou uma carta para os primos no celeiro de cevada, e gafanhotos de betume estão comendo tudo entre o celeiro e a porta."),
                T("Reach the Barley Terraces", "Chegar aos Terraços de Cevada"),
                T("Free the bitumen locusts of the terraces", "Libertar os gafanhotos de betume dos terraços"),
                T("Grandma Edna asked how the barley was. We said eaten. She said Jorge always said the same thing about what she cooked, and then she told us to eat something, which we did, standing up, on a terrace, with the radio on.",
                  "A Vó Edna perguntou como estava a cevada. A gente disse comida. Ela disse que o Jorge sempre falava isso do que ela cozinhava, e mandou a gente comer alguma coisa, e a gente comeu, em pé, num terraço, com o rádio ligado."),
                T("They eat in rows. Start at the far end of a row and walk toward the door.",
                  "Eles comem em fileiras. Comecem no fim da fileira e venham andando para a porta."),
                T("Joaquim, I built a boat once and I packed it properly. Eight locusts is a small thing to a man who has counted every animal he owns twice. Off with you, and take bread.",
                  "Joaquim, eu construí um barco uma vez e arrumei direito. Oito gafanhotos é pouco para quem contou duas vezes cada bicho que tem. Vai, e leva pão."),
                T("Maria, a flood is a very good way of learning who answers when you call. Your grandfather knew this and I didn't have to tell him. Eight locusts, and tell them it's over.",
                  "Maria, enchente é um ótimo jeito de aprender quem responde quando se chama. O seu avô sabia, e eu nem precisei contar. Oito gafanhotos, e diga que acabou."),
                T("Maithe, eight locusts, on four terraces, in a wind that keeps changing its mind. I'm told you find that sort of thing restful.",
                  "Maithe, oito gafanhotos, em quatro terraços, num vento que vive mudando de ideia. Me disseram que você acha isso descansado."),
                T("Ícaro, I counted the animals on my boat and it came out different every time. I've come to believe that's the point. Eight locusts. Then count them yourself.",
                  "Ícaro, contei os bichos do meu barco e cada vez dava um número. Acabei achando que é para ser assim. Oito gafanhotos. Depois você conta."),
                T("Lavine, everybody on my boat was bigger than you, and nobody was any use. Eight locusts. Your grandfather left the letter on the top shelf and I have no idea why.",
                  "Lavine, todo mundo no meu barco era maior que você, e ninguém servia para nada. Oito gafanhotos. O teu avô deixou a carta na prateleira de cima e eu não faço ideia por quê."));

            Step(all, "late_story_r14_gate",
                T("The Tablet at the Gate", "A Tábua no Portão"),
                T("Sin-leqi-unninni is the man who writes things down, and Grandpa Jorge dictated the eleventh letter to him at the Brick Gate, on a clay tablet, walking up and down and changing his mind. The tablet is still in the gate, and dimmed brick lions have taken up position in front of it.",
                  "O Sin-leqi-unninni é o homem que escreve as coisas, e o Vô Jorge ditou a décima primeira carta para ele no Portão de Tijolo, numa tábua de barro, andando de um lado para o outro e mudando de ideia. A tábua ainda está no portão, e leões de tijolo apagados montaram guarda na frente dela."),
                T("Reach the Brick Gate", "Chegar ao Portão de Tijolo"),
                T("Free the brick lions of the gate", "Libertar os leões de tijolo do portão"),
                T("It is the first letter with no jokes in it. It says the shadow will climb after us. Grandma Edna read it once, turned the radio down, and told us to keep our coats on, and to answer whoever calls, and not to go anywhere alone. So we didn't.",
                  "É a primeira carta sem nenhuma piada. Diz que a sombra vai subir atrás da gente. A Vó Edna leu uma vez, abaixou o rádio, mandou a gente não tirar o casaco, responder a quem chamasse e não ir a lugar nenhum sozinho. E a gente não foi."),
                T("A lion of brick cannot be frightened. It can be taken apart, carefully, in order.",
                  "Leão de tijolo não se assusta. Dá para desmontar, com cuidado, por ordem."),
                T("Joaquim, your grandfather dictated while he walked, and I wrote while I chased him. Six lions, and try to leave the gate standing. It's older than the idea of gates.",
                  "Joaquim, o seu avô ditava andando, e eu escrevia correndo atrás dele. Seis leões, e tenta deixar o portão de pé. É mais velho que a ideia de portão."),
                T("Maria, your grandfather stopped three times while dictating this and looked north. Whatever he was looking at, he was looking at it for you. Six lions, and then you may read it.",
                  "Maria, o seu avô parou três vezes enquanto ditava e olhou para o norte. O que quer que fosse, ele olhava por você. Seis leões, e aí você lê."),
                T("Maithe, I write everything down, and I wrote down that you would count the lions before you drew. Six. You needn't check. You are going to check.",
                  "Maithe, eu escrevo tudo, e escrevi que você ia contar os leões antes de atirar. Seis. Não precisa conferir. Você vai conferir."),
                T("Ícaro, this is the only tablet I have ever made that has an error in it, and the error is deliberate, and I have been looking for it for thirty years. Six lions and you may look too.",
                  "Ícaro, esta é a única tábua minha com um erro, e o erro é de propósito, e eu procuro faz trinta anos. Seis leões, e você procura também."),
                T("Lavine, I have written about floods and kings. Your grandfather asked me to add one line, in very small letters, saying that the youngest is not small. Go and read it. After six lions.",
                  "Lavine, já escrevi sobre enchentes e reis. O teu avô me pediu para acrescentar uma linha, em letra bem miúda, dizendo que a mais nova não é pequena. Vai ler. Depois de seis leões."));

            // ================= Região 15, Limiar Profundo =================
            Step(all, "late_story_r15_rampart",
                T("The Last Lamps", "As Últimas Lâmpadas"),
                T("Grandpa Jorge's letter from the Limiar arrived in Pamour's hands before anyone else was awake. It asks for the rampart lamps to be kept burning for the five of them. Lacquer ushabtis, marching the wall at night and forgetting why, are putting them out.",
                  "A carta do Vô Jorge do Limiar chegou às mãos do Pamour antes de qualquer um acordar. Pede que as lâmpadas da muralha fiquem acesas para os cinco. Ushabtis de laca, marchando na muralha à noite sem lembrar por quê, estão apagando elas."),
                T("Reach the Arched Rampart", "Chegar à Muralha em Arco"),
                T("Free the lacquer ushabtis of the rampart", "Libertar os ushabtis de laca da muralha"),
                T("Grandma Edna does not trust the Limiar and said so, at length, and then asked Pamour on the radio whether the lamps were burning. They were. She said: good. Then: tell the children to eat. Then she didn't hang up.",
                  "A Vó Edna não confia no Limiar e disse isso, bastante, e perguntou ao Pamour pelo rádio se as lâmpadas estavam acesas. Estavam. Ela disse: bom. Depois: fala para as crianças comerem. E não desligou."),
                T("Every lamp they put out, I light again. I am getting slower. They are not.",
                  "Cada lâmpada que eles apagam, eu acendo de novo. Eu estou ficando mais lento. Eles não."),
                T("Joaquim, on this wall the shield is not a figure of speech. Six ushabtis, and stand where I tell you, please. Nobody else here has listened in years.",
                  "Joaquim, nesta muralha o escudo não é modo de falar. Seis ushabtis, e fica onde eu mandar, por favor. Ninguém mais aqui escuta faz anos."),
                T("Maria, I keep one lamp and it has been burning for you since your grandfather's letter came. You may look at it. Then go and show six ushabtis what they were built to answer.",
                  "Maria, eu cuido de uma lâmpada, e ela está acesa por você desde que chegou a carta do seu avô. Pode olhar. Depois vai mostrar a seis ushabtis ao que eles foram feitos para responder."),
                T("Maithe, there is no wind along this wall, none at all, and it is the only wall I know of where that's frightening. Six ushabtis. Count me, too, when you've finished.",
                  "Maithe, não tem vento nenhum nesta muralha, e é a única muralha que eu conheço em que isso dá medo. Seis ushabtis. Me conta também, quando terminar."),
                T("Ícaro, these ushabtis were lacquered, then fitted, then told something. I would give a lot to know what. Six of them, and bring me the part behind the ear.",
                  "Ícaro, esses ushabtis foram laqueados, depois montados, depois alguém contou algo para eles. Eu daria muito para saber o quê. Seis deles, e me traz a peça de trás da orelha."),
                T("Lavine, this is the most frightening place in the world and you are standing in it with your arms crossed. I've run out of things to tell you. Six ushabtis.",
                  "Lavine, este é o lugar mais assustador do mundo e você está nele de braços cruzados. Acabou o que eu tinha para falar. Seis ushabtis."));

            Step(all, "late_story_r15_brink",
                T("The Postman at the Brink", "O Carteiro na Beira"),
                T("Makarios has carried letters to the very edge of the Limiar for sixty years, and the last of Grandpa Jorge's is in his bag: he has been told to give it to no one before they stand on the Brink. Little shadows have made the Brink a bad place to stand.",
                  "O Makarios leva cartas até a beira do Limiar faz sessenta anos, e a última do Vô Jorge está na bolsa dele: foi avisado para não entregar a ninguém antes de estar na Beira do Abismo. Sombras pequenas transformaram a Beira num lugar ruim de ficar."),
                T("Reach the Brink of the Abyss", "Chegar à Beira do Abismo"),
                T("Free the little shadows of the brink", "Libertar as sombras pequenas da beira"),
                T("It said to go up, not down, that the road goes on past the Column, and that he had kept a seat. Grandma Edna listened to all of it and said: go on, then. Her voice was very steady. Then she asked Maithe to count everybody, out loud, for her.",
                  "Dizia para subir, não descer, que a estrada continua depois da Coluna, e que ele tinha guardado um lugar. A Vó Edna ouviu tudo e disse: então vai. A voz dela estava muito firme. Depois pediu à Maithe que contasse todo mundo em voz alta, para ela."),
                T("I have delivered to worse addresses. Not many. Eight, and stand on my left, away from the edge.",
                  "Já entreguei em endereço pior. Poucos. Oito, e fiquem do meu lado esquerdo, longe da beira."),
                T("Joaquim, I am eighty-one and I have never once been late. I will be very upset if this is the first time. Eight of them, at the Brink, and hurry up.",
                  "Joaquim, eu tenho oitenta e um anos e nunca atrasei uma entrega. Vou ficar muito chateado se for a primeira. Oito deles, na Beira, e depressa."),
                T("Maria, I have carried this one for longer than you have been alive. It weighs more than the others. Eight shadows, and then I will give it to you, and I'll sit down, if it's all the same.",
                  "Maria, eu carrego esta faz mais tempo do que você tem de vida. Ela pesa mais que as outras. Oito sombras, e depois eu te entrego, e vou sentar, se não for incômodo."),
                T("Maithe, your wind will be no use on the Brink. Your grandfather wrote that you would mind that. Eight shadows, and he says to let somebody else go first, once.",
                  "Maithe, o teu vento não vai adiantar na Beira. O teu avô escreveu que você ia se incomodar com isso. Oito sombras, e ele diz para deixar outra pessoa ir na frente, uma vez."),
                T("Ícaro, the address on this letter is a place that isn't on any map. I've looked. I would very much like someone to explain that to me. Eight shadows first.",
                  "Ícaro, o endereço desta carta é um lugar que não está em mapa nenhum. Eu procurei. Queria muito que alguém me explicasse. Oito sombras primeiro."),
                T("Lavine, the shadows down here are the cold kind, not yours. You know the difference and I'm told you explain it to people. Eight of them. Explain it to them.",
                  "Lavine, as sombras daqui são das frias, não das tuas. Você sabe a diferença e me disseram que você explica para as pessoas. Oito delas. Explica para elas."));

            // ================= Região 16, Reinos do Céu =================
            Step(all, "late_story_r16_river",
                T("The River of Light", "O Rio de Luz"),
                T("Adda, who keeps the Wheel and tells it like it is, says Grandpa Jorge came up as far as the River of Light once, in a dream or a very good lie, and dropped the page of a letter in the water. Dimmed fish of light have been carrying it round the river ever since.",
                  "O Adda, que cuida da Roda e diz as coisas como são, conta que o Vô Jorge subiu até o Rio de Luz uma vez, em sonho ou numa mentira muito boa, e deixou cair a folha de uma carta na água. Peixes de luz apagados andam carregando ela pelo rio desde então."),
                T("Reach the River of Light", "Chegar ao Rio de Luz"),
                T("Free the dimmed fish of light", "Libertar os peixes de luz apagados"),
                T("Grandma Edna asked what the river looked like, and we told her, and she said that it sounded like the Zarafshan in April, and that he'd said so too. Then she told us to wash behind our ears, up there as well.",
                  "A Vó Edna perguntou como era o rio, a gente contou, e ela disse que parecia o Zarafshan em abril, e que ele falou a mesma coisa. Depois mandou a gente lavar atrás da orelha, lá em cima também."),
                T("A fish of light that forgets it is light swims in circles. Break the circle.",
                  "Peixe de luz que esquece que é luz nada em círculos. Quebrem o círculo."),
                T("Joaquim, up here water doesn't push back, and you are going to take that personally. Seven fish. Your grandfather fell in twice. I counted.",
                  "Joaquim, aqui em cima a água não empurra de volta, e você vai levar isso para o lado pessoal. Sete peixes. O seu avô caiu duas vezes. Eu contei."),
                T("Maria, you dreamed this river before you ever came here, and you don't have to tell me. Seven fish, gently. They are only trying to go home.",
                  "Maria, você sonhou com este rio antes de vir aqui, e não precisa me contar. Sete peixes, com jeito. Eles só estão tentando voltar para casa."),
                T("Maithe, there are seven fish, and no wind on the water at all. I kept the Tower for years. I know that look. Go on.",
                  "Maithe, são sete peixes, e nenhum vento na água. Guardei a Torre por anos. Conheço essa cara. Vai."),
                T("Ícaro, this river runs uphill and the Wheel does not explain why. Seven fish, then write it down, then bring it to me, then we'll both be confused.",
                  "Ícaro, este rio corre para cima, e a Roda não explica por quê. Sete peixes, depois anota, depois me traz, e aí a gente fica os dois sem entender."),
                T("Lavine, you can walk on this water. Your grandfather couldn't. I am not going to tell you why. Seven fish, and don't let your cousins see you smiling.",
                  "Lavine, você consegue andar nesta água. O seu avô não conseguia. Eu não vou te contar por quê. Sete peixes, e não deixa os teus primos verem que você sorriu."));

            Step(all, "late_story_r16_column",
                T("The Foot of the Column", "Os Pés da Coluna"),
                T("The Coluna da Glória stands in front of them at last, and the very last of the dark has crept up after the cousins and is climbing the steps. Grandpa Jorge wrote: it will follow you up, because that is what it does, and that is why you do not go alone.",
                  "A Coluna da Glória está na frente deles, afinal, e o último resto do escuro subiu atrás dos primos e está pisando nos degraus. O Vô Jorge escreveu: ela vai subir atrás de vocês, porque é o que ela faz, e é por isso que não se vai sozinho."),
                T("Reach the Foot of the Column of Glory", "Chegar aos Pés da Coluna da Glória"),
                T("Free the last of the creeping dark on the steps", "Libertar o que resta do escuro rastejante nos degraus"),
                T("We told Grandma Edna we were at the foot of the Column. She said: I know, I can see it from the porch. She meant her porch. She meant the one at home. We asked what it looks like from there, and she said: like him, showing off.",
                  "A gente contou para a Vó Edna que chegou nos pés da Coluna. Ela disse: eu sei, estou vendo daqui da varanda. Era a varanda dela. Era a de casa. A gente perguntou como é de lá, e ela disse: parece ele, se exibindo."),
                T("It only climbs while someone is climbing. Do not stop on the steps. Do not hurry either.",
                  "Ele só sobe enquanto tem alguém subindo. Não parem nos degraus. E não corram também."),
                T("Joaquim, this is the part where you stand in front. I wouldn't have thought I'd say that to you. Eight shadows on the steps, and step aside the moment they're done.",
                  "Joaquim, esta é a hora de ficar na frente. Não pensei que eu fosse dizer isso. Oito sombras nos degraus, e sai da frente assim que elas acabarem."),
                T("Maria, this is what you have been hearing all along. You don't need me for this one. Eight shadows on the steps, and go with light.",
                  "Maria, é isto que você ouve desde o começo. Para esta você não precisa de mim. Oito sombras nos degraus, e vai com luz."),
                T("Maithe, count them, please. Eight shadows and five of you. I'd like to hear you say five. Then say it again when you come down.",
                  "Maithe, conta, por favor. Oito sombras e cinco de vocês. Eu quero ouvir você dizer cinco. E dizer de novo quando descer."),
                T("Ícaro, nothing up here measures. Not your meter, not my Wheel. Your grandfather wrote that it would bother you and he hoped it would. Eight shadows.",
                  "Ícaro, nada aqui em cima se mede. Nem o teu medidor, nem a minha Roda. O teu avô escreveu que isso ia te incomodar, e que ele esperava que sim. Oito sombras."),
                T("Lavine, these are the last of the cold ones. They are very small now. So you may tell them what you tell everybody. Eight of them. And thank you.",
                  "Lavine, estas são as últimas das frias. Estão bem pequenas agora. Então você pode falar para elas o que fala para todo mundo. Oito delas. E obrigado."));

            Step(all, "late_story_r16_porch",
                T("The Porch of the Time After", "A Varanda do Tempo de Depois"),
                T("Grandpa Jorge's very last letter is only one line, and Adda has been holding it for a very long time. It says: the chair is on the left, and it is not for sitting on. Go up and see. The road to the Varanda runs on from the Column.",
                  "A última carta do Vô Jorge é uma linha só, e o Adda segura ela faz muito tempo. Diz: a cadeira fica à esquerda, e não é para sentar. Sobe para ver. O caminho para a Varanda continua depois da Coluna."),
                T("Sit beside Grandpa Jorge's chair on the Porch of the Time After", "Sentar ao lado da cadeira do Vô Jorge na Varanda do Tempo de Depois"),
                null,
                T("The chair is empty and it rocks a little. The orchard is made of light. Grandma Edna asked us to hold the radio near the chair, and we did, and for a long time neither she nor any of us said anything. Then she started singing, badly, the way he did, and all five of us sang too, and a spark went up.",
                  "A cadeira está vazia e balança um pouquinho. O pomar é de luz. A Vó Edna pediu para a gente segurar o rádio perto da cadeira, e a gente segurou, e por muito tempo nem ela nem nenhum de nós disse nada. Depois ela começou a cantar, mal, do jeito que ele cantava, e os cinco cantaram junto, e uma faísca subiu."),
                T("Keep going past the Column. You will know the place. It looks like home, only with more sky.",
                  "Continuem depois da Coluna. Vocês vão reconhecer o lugar. Parece a casa, só que com mais céu."),
                T("Joaquim, you may put the shield down up there, if you want to. Nobody is going to ask you to. Go up. Your grandfather kept the good chair for someone, and it isn't you, but go up.",
                  "Joaquim, lá em cima você pode encostar o escudo, se quiser. Ninguém vai pedir. Sobe. O teu avô guardou a cadeira boa para alguém, e não é você, mas sobe."),
                T("Maria, this is the place in the dream, the one with five cousins and one empty seat. You heard it first. Go and tell the others that they may sit down.",
                  "Maria, este é o lugar do sonho, aquele com cinco primos e um lugar vazio. Você ouviu primeiro. Vai dizer aos outros que eles podem sentar."),
                T("Maithe, there are five of you on that road. Say it once, out loud, at the top. It is the only count he ever asked you to make.",
                  "Maithe, são cinco de vocês nessa estrada. Fala uma vez, em voz alta, lá em cima. É a única conta que ele te pediu para fazer."),
                T("Ícaro, I can't tell you what's up there, and neither can your meter, and that is all right. Take the last page of your notebook. You will know what to write on it.",
                  "Ícaro, eu não sei te contar o que tem lá em cima, nem o teu medidor sabe, e tudo bem. Leva a última página do teu caderno. Você vai saber o que escrever."),
                T("Lavine, you go first. That was the instruction, it is in his hand, and it is underlined twice. Go on. You are the youngest, and that's why, and that is not the same as small.",
                  "Lavine, você vai primeiro. Essa era a instrução, está na letra dele, sublinhada duas vezes. Vai. Você é a mais nova, é por isso, e isso não é a mesma coisa que pequena."));

            // ================= Linhas da cadeia =================
            all.Add(new LocalizationEntry("quest.late_story.too_low.joaquim", "Joaquim, I know you'd walk there on your hands. The road up there asks for more than nerve. Train, eat, come back.",
                "Joaquim, eu sei que você ia até andando de mãos. A estrada lá em cima pede mais do que coragem. Treina, come e volta."));
            all.Add(new LocalizationEntry("quest.late_story.too_low.maria", "Maria, not yet. The call will wait, and so will the letter, and you are worth waiting for. Go with light and come back stronger.",
                "Maria, ainda não. O chamado espera, a carta também, e você vale a espera. Vai com luz e volta mais forte."));
            all.Add(new LocalizationEntry("quest.late_story.too_low.maithe", "Maithe, I know that look and the answer is still no. You'd make it there. Your cousins wouldn't. Bring all of them up to it first.",
                "Maithe, conheço essa cara e a resposta continua sendo não. Você chegava. Os primos não. Sobe todo mundo primeiro."));
            all.Add(new LocalizationEntry("quest.late_story.too_low.icaro", "Ícaro, your odds are poor. I've done the sums, and I'm no good at sums, and they're still poor. Come back with a better number.",
                "Ícaro, as tuas chances estão ruins. Fiz a conta, eu nem sei fazer conta, e ainda assim estão ruins. Volta com um número melhor."));
            all.Add(new LocalizationEntry("quest.late_story.too_low.lavine", "Lavine, this isn't because you're the youngest. It's because you are not ready yet. Those are two different things, and you of all people know it.",
                "Lavine, não é porque você é a mais nova. É porque ainda não está pronta. São duas coisas diferentes, e justamente você sabe disso."));

            all.Add(new LocalizationEntry(LateStoryCatalog.FinaleKeyBase + "joaquim", "You came back down, Joaquim, and the shield stayed up there against the wall. Grandma Edna says it can stay. She also says dinner. Grandpa Jorge would have got here late for it.",
                "Você desceu, Joaquim, e o escudo ficou lá em cima, encostado na parede. A Vó Edna diz que pode ficar. Diz também que o jantar está pronto. O Vô Jorge teria chegado atrasado."));
            all.Add(new LocalizationEntry(LateStoryCatalog.FinaleKeyBase + "maria", "Maria, the letters are done and the road isn't. He wrote it himself: when the light calls, we answer. Grandma Edna says she has never once heard you say otherwise. Go with light.",
                "Maria, as cartas acabaram e a estrada não. Ele mesmo escreveu: quando a luz chama, a gente responde. A Vó Edna diz que nunca ouviu você falar diferente. Vai com luz."));
            all.Add(new LocalizationEntry(LateStoryCatalog.FinaleKeyBase + "maithe", "Five, Maithe. I heard you say it, all the way down here. Grandma Edna heard it on the radio. Grandpa Jorge's chair kept on rocking for a while after, and nobody touched it.",
                "Cinco, Maithe. Eu ouvi você dizer, lá de baixo. A Vó Edna ouviu pelo rádio. A cadeira do Vô Jorge continuou balançando um tempo depois, e ninguém encostou nela."));
            all.Add(new LocalizationEntry(LateStoryCatalog.FinaleKeyBase + "icaro", "Ícaro, I read the last page of your notebook. Yes, you left it where I would find it. Grandpa Jorge would have said it was the best invention of the lot, and Grandma Edna says she told you so first.",
                "Ícaro, eu li a última página do teu caderno. Sim, você deixou onde eu ia achar. O Vô Jorge ia dizer que foi a melhor invenção de todas, e a Vó Edna diz que ela te falou primeiro."));
            all.Add(new LocalizationEntry(LateStoryCatalog.FinaleKeyBase + "lavine", "You went first, Lavine, and you were not small, and nobody needs telling now. Grandma Edna is keeping a place for you beside her. Grandpa Jorge underlined it twice.",
                "Você foi primeiro, Lavine, e não era pequena, e ninguém precisa mais ouvir isso. A Vó Edna guardou um lugar do lado dela para você. O Vô Jorge sublinhou duas vezes."));

            // "A Lua que Não Enche" (MidStoryCatalog), the chain before the letters: same keys, same loaders.
            MidStoryTexts.AddTo(all);
            return all;
        }
    }
}
