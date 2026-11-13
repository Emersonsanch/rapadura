using System.Collections.Generic;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Enemies;

namespace Rapadura.UI.Bestiary
{
    /// <summary>
    /// "Caderno da Vó Edna" pages of the last map bosses of Regions 1–9 (Docs/Mapas100.md 16, 28, 29, 44, 49, 50, 51; <see cref="RegionBossRules.MapSpawns"/>)
    /// and the lines they call out, en + pt-BR. Kept apart from <c>RegionBossTexts.MapBosses.cs</c>; <see cref="AddMapBossPages"/> appends them.
    /// </summary>
    public static partial class RegionBossTexts
    {
        /// <summary>Which way the Grey Stallion leads the herd (<see cref="RegionBossRules.HerdWayFor"/>), called out as it warns.</summary>
        public static string HerdWayKey(HerdWay way) => $"region_boss.grey_stallion.way.{way.ToString().ToLowerInvariant()}";

        private static void AddLastMapBossPages(List<LocalizationEntry> entries)
        {
            // -- Muralhas de Pedra Seca (16) --
            Page(entries, RegionBossRules.MazanMasonId,
                "The mazans are the stone giants of the old books, children, quarrelsome and proud, and most of them live up on the summit. This one came down to the drystone walls and fell in love with building. Not a bad love, you'd think. But the shadow got into it, and now he wants to wall in the whole Serra, every valley and every sheep, so nothing can ever get out. When he crouches, he throws up two walls crossing at his feet and crushes the quarter you are standing in. So climb over a wall, into the next quarter, or get well away. And watch: each time, his walls stand a little slanted from the last. Free him and he mends the walls of the Recinto Curvo instead, with a gate in every one.",
                "Os mazans são os gigantes de pedra dos livros antigos, crianças, briguentos e orgulhosos, e a maioria mora lá no cume. Este desceu pras muralhas de pedra seca e se apaixonou por construir. Não é um amor ruim, vocês diriam. Mas a sombra entrou nele, e agora ele quer murar a Serra inteira, cada vale e cada ovelha, pra nada nunca mais sair. Quando ele se agacha, levanta dois muros cruzando nos pés dele e esmaga o quarto em que vocês estão. Então pulem um muro, pro quarto do lado, ou fiquem bem longe. E reparem: cada vez os muros ficam um pouco tortos em relação aos de antes. Libertem ele e ele conserta os muros do Recinto Curvo, com um portão em cada um.",
                ("He walls me in, I climb into the next quarter. Never stay put.", "Ele me murou, eu pulo pro quarto do lado. Nunca fico parada."),
                ("He wants to keep everything safe by locking it in. That's not safe, that's lonely.", "Ele quer deixar tudo seguro trancando. Isso não é seguro, é solidão."),
                ("A giant who builds walls really fast. Of all the giants, the one with a hobby.", "Um gigante que levanta muro muito rápido. De todos os gigantes, o que tem um passatempo."),
                ("Two walls make four quarters. He can only crush one at a time.", "Dois muros fazem quatro quartos. Ele só consegue esmagar um de cada vez."),
                ("Can he build me a little house? With a window? And a door that opens?", "Ele pode construir uma casinha pra mim? Com janela? E uma porta que abre?"));
            Line(entries, RegionBossRules.MazanMasonId, RegionBossPhase.Storm, "The Mazan Mason throws loose stones!", "O Pedreiro Mazan atira pedras soltas!");
            Line(entries, RegionBossRules.MazanMasonId, RegionBossPhase.LastStand, "He's walling up the whole valley!", "Ele está murando o vale inteiro!");
            Blow(entries, RegionBossRules.MazanMasonId, "His stone fist comes down!", "Lá vem o punho de pedra!");
            Signature(entries, RegionBossRules.MazanMasonId, "He's walling in your quarter! Climb over into the next one!", "Ele vai murar o seu quarto! Pula pro quarto do lado!");
            Withdraw(entries, RegionBossRules.MazanMasonId, "The Mazan Mason lays one last stone, looks at it for a long time, and then pulls it out again to make a gate. He walks off towards the Recinto Curvo with a wall under each arm.", "O Pedreiro Mazan assenta uma última pedra, olha pra ela um tempão e depois tira de novo pra fazer um portão. Vai embora pro Recinto Curvo com um muro debaixo de cada braço.");

            // -- Cidade-Recife de Kirta (28) --
            Page(entries, RegionBossRules.GreyMorayId,
                "Under Kirta there is a mother coral, children, older than the city, and her colours are the colours of the whole reef. A great moray lived in a hole in her, as morays do, and when the shadow came up out of the deep the moray coiled round the mother coral to keep it away. She held on so tight and so long that the shadow got into her instead, and the coral went grey under her. She lives in the Lagoa Grande now, and she hunts the way morays hunt: she waits in her hole and bites whatever stops. So when she sinks out of sight, keep moving, never stand still, and she'll snap at the water. Free her and she lets go of the coral, and the whole reef lights up again.",
                "Debaixo de Kirta tem um coral-mãe, crianças, mais velho que a cidade, e as cores dele são as cores do recife inteiro. Uma moreia enorme morava num buraco dele, como as moreias moram, e quando a sombra subiu do fundo a moreia se enrolou no coral-mãe pra proteger. Segurou tão forte e por tanto tempo que a sombra entrou foi nela, e o coral ficou cinza debaixo dela. Agora ela mora na Lagoa Grande e caça do jeito que as moreias caçam: espera no buraco e morde o que para. Então quando ela sumir de vista, fiquem andando, nunca fiquem paradas, e ela morde a água. Libertem ela e ela solta o coral, e o recife inteiro acende de novo.",
                ("She sinks, I keep walking. Little circles, never still.", "Ela afundou, eu continuo andando. Rodinha, nunca parada."),
                ("She was hugging the coral to protect it. Nobody told her she could let go.", "Ela abraçava o coral pra proteger. Ninguém contou pra ela que podia soltar."),
                ("An eel that bites you for standing still. So that's why I never stand still. Science.", "Uma enguia que morde quem fica parado. Então é por isso que eu nunca paro. Ciência."),
                ("A hunter who waits learns to see stillness. Give her nothing still to see.", "Caçador que espera aprende a ver o que está parado. Não dá nada parado pra ela ver."),
                ("I'm dancing! Look, Vó, I'm dancing so she can't bite me!", "Tô dançando! Olha, Vó, tô dançando pra ela não me morder!"));
            Line(entries, RegionBossRules.GreyMorayId, RegionBossPhase.Storm, "The Grey Moray spits grey mud!", "A Moreia Cinza cospe lama cinza!");
            Line(entries, RegionBossRules.GreyMorayId, RegionBossPhase.LastStand, "She's coiling through the whole lagoon!", "Ela se enrola pela lagoa inteira!");
            Blow(entries, RegionBossRules.GreyMorayId, "She lunges out of the coral!", "Ela salta do coral!");
            Signature(entries, RegionBossRules.GreyMorayId, "She sank into her hole! Keep moving, don't stand still!", "Ela afundou no buraco! Fica andando, não para!");
            Withdraw(entries, RegionBossRules.GreyMorayId, "The Grey Moray uncoils, slowly, and slides back into her hole. Far off under Kirta a pink light runs through the mother coral, then orange, then every colour of the reef.", "A Moreia do Coral Cinza se desenrola, devagar, e escorrega de volta pro buraco dela. Lá longe debaixo de Kirta uma luz rosa corre pelo coral-mãe, depois laranja, depois todas as cores do recife.");

            // -- Kilwa Afundada (29) --
            Page(entries, RegionBossRules.PalaceCrabId,
                "When the sea came into Kilwa, a hermit crab found the dome of the old palace lying in the water and thought: what a fine shell. It is a big crab now, children, as big as the dome, and the shadow told it the palace is its own and the whole city with it. It walks the ruins of Gedi with the dome on its back, and like every crab it walks sideways. When it scuttles it runs a long row right across where you stand: stepping to the side won't save you, the row is too long. Step towards it, or step away. Free it and it gives the dome back, and Kilwa gets its palace roof again.",
                "Quando o mar entrou em Kilwa, um caranguejo-ermitão achou a cúpula do palácio velho caída na água e pensou: que concha bonita. Hoje ele é um caranguejo enorme, crianças, do tamanho da cúpula, e a sombra disse pra ele que o palácio é dele e a cidade inteira junto. Ele anda pelas ruínas de Gedi com a cúpula nas costas, e como todo caranguejo anda de lado. Quando ele dispara, corre uma fileira comprida bem por cima de onde vocês estão: pisar pro lado não salva, a fileira é comprida demais. Deem um passo na direção dele, ou pra longe. Libertem ele e ele devolve a cúpula, e Kilwa ganha de novo o telhado do palácio.",
                ("He runs sideways, I step forward or back. Never to the side.", "Ele corre de lado, eu dou um passo pra frente ou pra trás. Nunca pro lado."),
                ("He just wanted a home. We'll help him find one that isn't a palace.", "Ele só queria uma casa. A gente ajuda ele a achar uma que não seja um palácio."),
                ("He stole a building. And wears it. That takes confidence.", "Ele roubou um prédio. E está usando. Isso é que é autoestima."),
                ("A crab moves across, never along. Go along.", "Caranguejo anda atravessado, nunca pra frente. Vai pra frente."),
                ("Hey, crab! Your house is too big for you! ...He waved his claw. I think that's a no.", "Ei, caranguejo! Sua casa é grande demais pra você! ...Ele balançou a garra. Acho que é um não."));
            Line(entries, RegionBossRules.PalaceCrabId, RegionBossPhase.Storm, "The Palace Crab flicks bits of old coral!", "O Caranguejo-Palácio atira pedaços de coral velho!");
            Line(entries, RegionBossRules.PalaceCrabId, RegionBossPhase.LastStand, "He clacks his claws against the dome!", "Ele bate as garras na cúpula!");
            Blow(entries, RegionBossRules.PalaceCrabId, "His big claw comes down!", "Lá vem a garra grande!");
            Signature(entries, RegionBossRules.PalaceCrabId, "He's going to scuttle across you! Step towards him or away, not sideways!", "Ele vai correr de lado por cima de você! Pra frente ou pra trás, não pro lado!");
            Withdraw(entries, RegionBossRules.PalaceCrabId, "The Palace Crab sets the dome down on the stones of Gedi, very carefully, and creeps off sideways to find a shell its own size.", "O Caranguejo-Palácio põe a cúpula nas pedras de Gedi, com muito cuidado, e sai de lado procurar uma concha do tamanho dele.");

            // -- Arrozais Espelhados (44) --
            Page(entries, RegionBossRules.GoldenScarecrowId,
                "The people of the terraces made a scarecrow of golden straw to keep the locusts off the rice, and it was so good at its job that it got proud. Then the shadow whispered that it could scare anything at all, and now it scares even the light: the moon in the paddies hides when it walks. When it spreads its arms wide, children, it frightens everyone near it, and the only thing that works is to do what a frightened sparrow does. Back away. Standing your ground is no good, and coming closer is worse. Free it and it goes back to its pole and scares only the locusts, as it should.",
                "O povo dos terraços fez um espantalho de palha dourada pra espantar os gafanhotos do arroz, e ele era tão bom no trabalho que ficou orgulhoso. Aí a sombra cochichou que ele podia espantar qualquer coisa, e agora ele espanta até a luz: a lua dos arrozais se esconde quando ele anda. Quando ele abre bem os braços, crianças, assusta todo mundo perto dele, e a única coisa que funciona é fazer o que um pardal assustado faz. Recuem. Ficar firme não adianta, e chegar perto é pior. Libertem ele e ele volta pro mastro e espanta só os gafanhotos, como deve ser.",
                ("Arms out, I back off. Three good steps.", "Abriu os braços, eu recuo. Três passos bons."),
                ("He was proud of doing his job. The shadow just made the job too big.", "Ele tinha orgulho do trabalho dele. A sombra só fez o trabalho ficar grande demais."),
                ("I'm not scared of a scarecrow. I'm just backing away very fast. Different thing.", "Eu não tenho medo de espantalho. Só estou recuando muito rápido. É diferente."),
                ("Fear works on those who stay. Distance answers it.", "O medo funciona em quem fica. A distância responde."),
                ("Boo! ...He didn't jump. Vó, why didn't he jump?", "Bu! ...Ele não pulou. Vó, por que ele não pulou?"));
            Line(entries, RegionBossRules.GoldenScarecrowId, RegionBossPhase.Storm, "The Scarecrow throws handfuls of golden straw!", "O Espantalho atira punhados de palha dourada!");
            Line(entries, RegionBossRules.GoldenScarecrowId, RegionBossPhase.LastStand, "Even the moon in the paddies is hiding!", "Até a lua dos arrozais está se escondendo!");
            Blow(entries, RegionBossRules.GoldenScarecrowId, "His pole comes down!", "Lá vem o mastro!");
            Signature(entries, RegionBossRules.GoldenScarecrowId, "He's spreading his arms! Back away from him!", "Ele está abrindo os braços! Recua pra longe dele!");
            Withdraw(entries, RegionBossRules.GoldenScarecrowId, "The Golden-Straw Scarecrow lowers its arms, shakes the shadow out of its straw, and walks back to its pole in the terraces. The moon comes out in every paddy at once.", "O Espantalho de Palha Dourada baixa os braços, sacode a sombra da palha e volta pro mastro dele nos terraços. A lua aparece em todos os arrozais de uma vez.");

            // -- Fortim dos Gigantes-da-Colina (49) --
            Page(entries, RegionBossRules.TabletNightmareId,
                "The hill giants write their dreams on clay tablets, children, and Virogdad reads them out in the Fortim, as the giants of the old book did. One night a giant dreamt something so bad that the shadow of it climbed out of the tablet and ran off into the valley. That is the Tablet Nightmare. It feeds on fear, and fear is easiest to find in the tired and the hurt: when it dreams at you, whoever is low on health takes the whole of it, and whoever is well only has a bad dream. So when it starts to dream, drink something, heal, stand tall. Beat it and the dream goes back into the tablet and turns into a good one.",
                "Os gigantes-da-colina escrevem os sonhos em tábuas de barro, crianças, e o Virogdad lê em voz alta no Fortim, como faziam os gigantes do livro antigo. Uma noite um gigante sonhou uma coisa tão ruim que a sombra dela saiu da tábua e fugiu pro vale. Esse é o Pesadelo da Tábua. Ele se alimenta de medo, e o medo é mais fácil de achar em quem está cansado e machucado: quando ele sonha em vocês, quem estiver com pouca vida leva tudo, e quem estiver bem só tem um pesadelo. Então quando ele começar a sonhar, bebam alguma coisa, se curem, fiquem de pé. Vençam ele e o sonho volta pra tábua e vira um sonho bom.",
                ("It starts dreaming, I drink a potion. Full health, no fear.", "Começou a sonhar, eu tomo uma poção. Vida cheia, sem medo."),
                ("Somebody had a dream so bad it ran away. I hope the giant is all right.", "Alguém teve um sonho tão ruim que ele fugiu. Espero que o gigante esteja bem."),
                ("I've had worse nightmares. Mostly about tests. This one is worse.", "Eu já tive pesadelos piores. Quase todos sobre prova. Esse é pior."),
                ("A nightmare grips only what it can frighten. Be whole and it slips off.", "Pesadelo só agarra o que consegue assustar. Esteja inteiro e ele escorrega."),
                ("When I have a bad dream I go to Vó's bed. Can it come to Vó's bed too?", "Quando eu tenho pesadelo eu vou pra cama da Vó. Ele pode ir pra cama da Vó também?"));
            Line(entries, RegionBossRules.TabletNightmareId, RegionBossPhase.Storm, "The Nightmare throws shards of its tablet!", "O Pesadelo atira cacos da tábua dele!");
            Line(entries, RegionBossRules.TabletNightmareId, RegionBossPhase.LastStand, "The whole valley is dreaming badly!", "O vale inteiro está sonhando mal!");
            Blow(entries, RegionBossRules.TabletNightmareId, "It falls on you like a bad night!", "Ele cai em cima de você feito noite ruim!");
            Signature(entries, RegionBossRules.TabletNightmareId, "It's dreaming at you! Heal up, it grips the weak!", "Ele está sonhando em você! Se cura, ele pega quem está fraco!");
            Withdraw(entries, RegionBossRules.TabletNightmareId, "The Tablet Nightmare shrinks into a line of writing and slips back into its clay tablet. Up in the Fortim a giant turns over in its sleep, and smiles.", "O Pesadelo da Tábua encolhe até virar uma linha de escrita e escorrega de volta pra tábua de barro. Lá no Fortim um gigante se vira no sono e sorri.");

            // -- Arvoredo de Haripushpa (50) --
            Page(entries, RegionBossRules.AzSawId,
                "Āz, the Greed, wanted the gold of the taiga, and the gold of the taiga is its old trees, so she had a sawing engine built to cut them down, all of them, and sell the wood. The engine is still out there among the larches, children, iron and teeth and a big wheel, and it turns to follow whatever moves, to cut it. But it turns slowly. Running straight away from it does no good: it just keeps facing you. Run round it instead, and get to its side before it has turned. Break it and the trunk folk take it apart and make tools of it, and the trees grow back.",
                "Āz, a Cobiça, queria o ouro da taiga, e o ouro da taiga são as árvores velhas, então ela mandou fazer um engenho de serrar pra derrubar todas e vender a madeira. O engenho ainda está lá no meio dos lariços, crianças, ferro e dentes e uma roda grande, e ele gira pra seguir o que se mexe, pra cortar. Mas gira devagar. Correr reto pra longe não adianta: ele só continua virado pra vocês. Corram em volta dele, e cheguem do lado antes que ele termine de virar. Quebrem ele e o povo-tronco desmonta e faz ferramenta, e as árvores crescem de novo.",
                ("It turns to me, I run round it. Get to its side.", "Ele virou pra mim, eu corro em volta. Chego do lado dele."),
                ("It doesn't hate the trees. It doesn't feel anything. That's what Āz made it for.", "Ele não odeia as árvores. Ele não sente nada. Foi pra isso que a Āz fez ele."),
                ("A saw with legs. I'm officially done with forests.", "Uma serra com pernas. Oficialmente terminei com florestas."),
                ("Close in, a small circle is enough. Far out, it takes the whole lap.", "De perto, um círculo pequeno basta. De longe, precisa da volta inteira."),
                ("Bad saw! Trees are for climbing! And for hugging!", "Serra feia! Árvore é pra subir! E pra abraçar!"));
            Line(entries, RegionBossRules.AzSawId, RegionBossPhase.Storm, "The Saw of Āz throws hot sparks!", "A Serra de Āz atira faíscas quentes!");
            Line(entries, RegionBossRules.AzSawId, RegionBossPhase.LastStand, "Its wheel spins faster and faster!", "A roda dela gira cada vez mais rápido!");
            Blow(entries, RegionBossRules.AzSawId, "The blade comes down!", "Lá vem a lâmina!");
            Signature(entries, RegionBossRules.AzSawId, "It's turning to cut you! Run round it, not away!", "Ela está virando pra te cortar! Corre em volta, não pra longe!");
            Withdraw(entries, RegionBossRules.AzSawId, "The Saw of Āz grinds, shudders and stops for good. The trunk folk come out of the larches with their baskets, and by evening its iron is axes, spades and a new kettle.", "A Serra de Āz range, estremece e para de vez. O povo-tronco sai dos lariços com os cestos, e de tardinha o ferro dela já é machado, enxada e uma chaleira nova.");

            // -- Pradaria dos Mil Cavalos (51) --
            Page(entries, RegionBossRules.GreyStallionId,
                "The thousand horses of the prairie have a chief, children, a great grey stallion who leads them to water and away from wolves. The shadow frightened him so badly that he forgot which way is safe, and now he leads the herd round and round the prairie, any way at all, and tramples everyone in the path. He always calls out where he is going: north, then east, then south, then west. So when he whinnies, don't stand there and don't cross in front of them: run the same way the herd runs, with them. Free him and he remembers, and the herd follows you cousins all the way to Soslan's camp.",
                "Os mil cavalos da pradaria têm um chefe, crianças, um garanhão cinzento enorme que leva eles pra água e pra longe dos lobos. A sombra assustou ele tanto que ele esqueceu pra que lado é seguro, e agora leva o rebanho em volta da pradaria, pra qualquer lado, e atropela quem estiver no caminho. Ele sempre avisa pra onde vai: norte, depois leste, depois sul, depois oeste. Então quando ele relinchar, não fiquem parados e não atravessem na frente: corram pro mesmo lado que o rebanho corre, junto com eles. Libertem ele e ele lembra, e o rebanho segue vocês, primos, até o acampamento de Soslan.",
                ("He calls the way, I run that way. With the herd, never across.", "Ele diz o lado, eu corro pra lá. Com o rebanho, nunca atravessado."),
                ("He's leading them in circles because he's scared. Somebody has to lead him.", "Ele leva eles em roda porque está com medo. Alguém tem que levar ele."),
                ("Run with a thousand horses. Sure. I'll just keep up with a thousand horses.", "Correr com mil cavalos. Claro. Vou só acompanhar mil cavalos."),
                ("A herd only tramples what stands against it. Become part of it.", "Rebanho só atropela o que fica contra ele. Vira parte dele."),
                ("I'm a horse too! Neigh! I'm running with them! Neigh!", "Eu também sou cavalo! Iiirrí! Tô correndo com eles! Iiirrí!"));
            Line(entries, RegionBossRules.GreyStallionId, RegionBossPhase.Storm, "The Grey Stallion kicks up clods of the prairie!", "O Garanhão Cinzento atira torrões da pradaria!");
            Line(entries, RegionBossRules.GreyStallionId, RegionBossPhase.LastStand, "The whole herd is stampeding!", "O rebanho inteiro está em disparada!");
            Blow(entries, RegionBossRules.GreyStallionId, "He rears and his hooves come down!", "Ele empina e lá vêm os cascos!");
            Signature(entries, RegionBossRules.GreyStallionId, "He's leading the herd! Run the way they run!", "Ele vai levar o rebanho! Corre pro lado que eles correm!");
            entries.Add(new LocalizationEntry(HerdWayKey(HerdWay.North), "The herd runs north: run north with it!", "O rebanho corre pro norte: corre pro norte junto!"));
            entries.Add(new LocalizationEntry(HerdWayKey(HerdWay.East), "The herd runs east: run east with it!", "O rebanho corre pro leste: corre pro leste junto!"));
            entries.Add(new LocalizationEntry(HerdWayKey(HerdWay.South), "The herd runs south: run south with it!", "O rebanho corre pro sul: corre pro sul junto!"));
            entries.Add(new LocalizationEntry(HerdWayKey(HerdWay.West), "The herd runs west: run west with it!", "O rebanho corre pro oeste: corre pro oeste junto!"));
            Withdraw(entries, RegionBossRules.GreyStallionId, "The Grey Stallion slows, stops and lowers his head. Then he turns towards Soslan's camp and walks, and behind him, one by one, the thousand horses follow.", "O Garanhão Cinzento diminui, para e abaixa a cabeça. Depois vira pro acampamento de Soslan e sai andando, e atrás dele, um por um, os mil cavalos seguem.");
        }
    }
}
