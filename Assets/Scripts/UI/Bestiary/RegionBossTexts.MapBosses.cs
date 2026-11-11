using System.Collections.Generic;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Enemies;

namespace Rapadura.UI.Bestiary
{
    /// <summary>
    /// "Caderno da Vó Edna" pages of the map bosses of the fichas (Docs/Mapas100.md 25, 26, 32, 37, 38, 43, 55, 56, 60, 62, 66, 69; <see cref="RegionBossRules.MapSpawns"/>)
    /// and the lines they call out, en + pt-BR. Each is a creature or an engine gone dark: beaten, it is freed, and its last line says what it goes back to.
    /// </summary>
    public static partial class RegionBossTexts
    {
        /// <summary>What the Shisa King calls out as it opens one mouth or the other (<see cref="RegionBossRules.ShisaMouthFor"/>).</summary>
        public static string ShisaMouthKey(ShisaMouth mouth) => $"region_boss.shisa_king.mouth.{mouth.ToString().ToLowerInvariant()}";

        /// <summary>What the carp calls out as it leaps (<see cref="RegionBossRules.LeapFallsShort"/>): short of the mark, or past it.</summary>
        public static string CarpLeapKey(bool fallsShort) => fallsShort ? "region_boss.unleaped_carp.leap.short" : "region_boss.unleaped_carp.leap.long";

        /// <summary>The label over each spark the Great Almas hid in the snow (<see cref="RegionBossRules.SnowSparkSpots"/>).</summary>
        public const string SnowSparkKey = "region_boss.great_almas.spark";

        private static void AddMapBossPages(List<LocalizationEntry> entries)
        {
            AddMidMapBossPages(entries);

            // -- Pontes de Vento (55) --
            Page(entries, RegionBossRules.RopeCutterId,
                "The Rope Cutter is a bird of shadow that found out it likes to watch people fall. It doesn't eat them, children, it doesn't even look twice: it only wants the moment when the rope goes. Every feather of its wings ends in a little blade. When it goes high and quiet over a bridge, it is choosing a rope, and the people of the Ermida Baixa will tell you what to do, because they have done it all their lives: stop, and hold on. Whoever keeps walking when the rope snaps goes down with it. Whoever stands still only swings. Free it and it remembers it was a bird of the peaks, and it ends up guarding the very bridges it cut.",
                "O Cortador de Cordas é uma ave de sombra que descobriu que gosta de ver gente cair. Não come ninguém, crianças, nem olha duas vezes: só quer o instante em que a corda vai. Cada pena das asas termina numa laminazinha. Quando ele sobe e fica quieto em cima de uma ponte, está escolhendo uma corda, e o povo da Ermida Baixa vai dizer o que fazer, porque faz isso a vida inteira: parem e segurem. Quem continua andando quando a corda arrebenta desce junto. Quem fica parado só balança. Libertem ele e ele lembra que era ave dos picos, e acaba guardando as mesmas pontes que cortava.",
                ("When it goes quiet, you go still. Then hit it twice as hard.", "Quando ele ficar quieto, você fica parada. Depois bate em dobro."),
                ("Liking to watch someone fall is the loneliest thing I can think of.", "Gostar de ver alguém cair é a coisa mais solitária que eu consigo imaginar."),
                ("Standing still on a rope bridge in a storm. Sure. Very natural. No problem.", "Ficar parado numa ponte de corda no vento. Claro. Super natural. Sem problema."),
                ("Motion is what the cut needs. Take the motion away and the cut has nothing to work with.", "O corte precisa do movimento. Tira o movimento e o corte fica sem ter o que fazer."),
                ("I'm holding on! I'm holding on super tight! Is it gone yet?", "Tô segurando! Tô segurando bem forte! Ele já foi?"));
            Line(entries, RegionBossRules.RopeCutterId, RegionBossPhase.Storm, "The Rope Cutter throws its blades!", "O Cortador atira as lâminas!");
            Line(entries, RegionBossRules.RopeCutterId, RegionBossPhase.LastStand, "Every rope at once!", "Todas as cordas de uma vez!");
            Blow(entries, RegionBossRules.RopeCutterId, "It dives!", "Ele mergulha!");
            Signature(entries, RegionBossRules.RopeCutterId, "It's cutting the rope! Stop and hold on!", "Ele vai cortar a corda! Pare e segure!");
            Withdraw(entries, RegionBossRules.RopeCutterId, "The Rope Cutter shakes the shadow off its wings and flies up to watch over the bridges.", "O Cortador de Cordas sacode a sombra das asas e sobe para vigiar as pontes.");

            // -- Ermidas Suspensas (56) --
            Page(entries, RegionBossRules.EndlessBellId,
                "The great bell of the hermitages called the hermits to their prayers for a hundred years, and then the shadow got into it and it never stopped ringing. Day and night, children. It rang so long it got up and walked, and now it rings at whoever comes. Where it tolls, the sound falls on you; and the bell remembers its last note, so when it tolls again the old note rings at the same time where it fell before. Don't go back to where you were. Find a new quiet place each time. Stop it, and the whole cliff goes silent for the first time in years. The hermits will cry. Let them.",
                "O sino grande das ermidas chamou os eremitas pra reza por cem anos, e aí a sombra entrou nele e ele nunca mais parou de tocar. Dia e noite, crianças. Tocou tanto que levantou e saiu andando, e agora toca em quem chega. Onde ele bate, o som cai em cima de vocês; e o sino lembra da última nota, então quando bate de novo a nota velha toca junto lá onde caiu antes. Não voltem pra onde estavam. Achem um lugar quieto novo cada vez. Parem ele, e o paredão inteiro fica em silêncio pela primeira vez em anos. Os eremitas vão chorar. Deixem.",
                ("Never step back into the last mark. The echo is still there.", "Nunca volta pra última marca. O eco ainda tá lá."),
                ("It doesn't want to ring. It just doesn't know how to stop. That I understand.", "Ele não quer tocar. Só não sabe parar. Isso eu entendo."),
                ("My ears are ringing. My teeth are ringing. I think my hair is ringing.", "Meu ouvido tá zunindo. Meu dente tá zunindo. Acho que meu cabelo tá zunindo."),
                ("A sound that remembers where it fell. That's what an echo is, really: a memory that won't leave the room.", "Um som que lembra onde caiu. Eco é isso, no fundo: uma lembrança que não sai da sala."),
                ("Shhh! Shhhhh! Bell! It's bedtime!", "Shhh! Shhhhh! Sino! Já é hora de dormir!"));
            Line(entries, RegionBossRules.EndlessBellId, RegionBossPhase.Storm, "The bell rings faster!", "O sino toca mais rápido!");
            Line(entries, RegionBossRules.EndlessBellId, RegionBossPhase.LastStand, "The whole cliff is ringing!", "O paredão inteiro está tocando!");
            Blow(entries, RegionBossRules.EndlessBellId, "The bell comes down!", "O sino vai cair!");
            Signature(entries, RegionBossRules.EndlessBellId, "It tolls! And the last note rings again!", "Ele bate! E a nota de antes toca de novo!");
            Withdraw(entries, RegionBossRules.EndlessBellId, "The bell gives one last, soft note and goes still. The cliff is silent.", "O sino dá uma última nota baixinha e para. O paredão fica em silêncio.");

            // -- Canais de Gelo (60) --
            Page(entries, RegionBossRules.WalrusQueenId,
                "The Walrus Queen crowned herself, and nobody argued, because she is the size of a boatmen's house. She wants every fish in the canals, all of them, and she keeps them in that great belly, where you can see them shining. When she rears up and throws herself down on you, the middle of the splash is the worst of it: at the edge it is only cold water. So when she rises, children, don't stand and stare, walk out to the edge. At night the ice cracks more and she gets bolder. Free her and she remembers that fish are for sharing, and the boatmen will have a fat winter.",
                "A Morsa-Rainha coroou a si mesma, e ninguém discutiu, porque ela tem o tamanho de uma casa dos barqueiros. Quer todos os peixes dos canais, todinhos, e guarda eles naquela barrigona, onde dá pra ver eles brilhando. Quando ela empina e se joga em cima de vocês, o meio do tombo é o pior: na beirada é só água fria. Então quando ela subir, crianças, não fiquem parados olhando, andem pra beirada. De noite o gelo racha mais e ela fica mais atrevida. Libertem ela e ela lembra que peixe é pra dividir, e os barqueiros vão ter um inverno gordo.",
                ("She rises, you walk out. The edge only gets you wet.", "Ela empinou, você sai andando. A beirada só molha."),
                ("She's keeping all the fish so she'll never be hungry again. Somebody must have let her go hungry once.", "Ela guarda os peixes todos pra nunca mais passar fome. Alguém deixou ela com fome uma vez."),
                ("Your Majesty, with respect, that is a LOT of walrus coming down.", "Vossa Majestade, com todo o respeito, é MUITA morsa caindo."),
                ("The force spreads out from where she lands. Distance divides it. So does she, eventually.", "A força se espalha de onde ela cai. A distância divide. Ela também vai dividir, no fim."),
                ("She has fish in her tummy too! Like the Old Kul! Do all the big ones keep fish?", "Ela também tem peixe na barriga! Igual o Velho Kul! Todos os grandões guardam peixe?"));
            Line(entries, RegionBossRules.WalrusQueenId, RegionBossPhase.Storm, "The Queen spits icy water!", "A Rainha cospe água gelada!");
            Line(entries, RegionBossRules.WalrusQueenId, RegionBossPhase.LastStand, "The ice is breaking under her!", "O gelo está quebrando debaixo dela!");
            Blow(entries, RegionBossRules.WalrusQueenId, "Her tusks come down!", "Lá vêm as presas!");
            Signature(entries, RegionBossRules.WalrusQueenId, "She's rearing up! Get to the edge!", "Ela empinou! Vai pra beirada!");
            Withdraw(entries, RegionBossRules.WalrusQueenId, "The Walrus Queen lets the fish go back into the canal and slides off to share the ice.", "A Morsa-Rainha solta os peixes de volta no canal e escorrega embora para dividir o gelo.");

            // -- Galerias dos Mamutes (62) --
            Page(entries, RegionBossRules.GrandfatherMammothId,
                "In the old stories of the far north, the mammoth lives under the earth and digs the valleys, and it dies if it ever sees the sun. This one is the oldest of them all, and he has been digging so long he forgot which way is up. He isn't angry, children. He is lost, and lost things push. When he shakes the gallery the ceiling comes down in a cross round where you stand: the stone above you and four more, ahead, behind and to each side. Step slantwise, never straight. Free him and he remembers the way, and he opens a whole new valley on his way up. I'd like to see that valley before I die.",
                "Nas histórias antigas do norte distante, o mamute vive debaixo da terra e cava os vales, e morre se um dia vir o sol. Este é o mais velho de todos, e cava faz tanto tempo que esqueceu qual lado é pra cima. Não está bravo, crianças. Está perdido, e coisa perdida empurra. Quando ele sacode a galeria, o teto cai em cruz em volta de onde vocês estão: a pedra em cima de vocês e mais quatro, na frente, atrás e de cada lado. Pisem na diagonal, nunca reto. Libertem ele e ele lembra o caminho, e abre um vale novinho na subida. Eu queria ver esse vale antes de morrer.",
                ("Never sideways, never back. Slantwise, every time.", "Nem pro lado, nem pra trás. Na diagonal, toda vez."),
                ("He's been looking for the way up for longer than anyone's been alive. Let's show him.", "Ele procura o caminho pra cima há mais tempo do que qualquer um está vivo. Vamos mostrar."),
                ("Grandpa, I respect you, but your ceiling is falling on my head.", "Vovô, eu te respeito, mas o seu teto tá caindo na minha cabeça."),
                ("A cross has four arms and four gaps. The gaps are where the arithmetic lets you live.", "Uma cruz tem quatro braços e quatro vãos. Os vãos são onde a conta deixa a gente viver."),
                ("He's so fluffy! Vó, can we keep him? He can live in the garden! The garden is small.", "Ele é muito peludo! Vó, a gente pode ficar com ele? Ele mora no quintal! O quintal é pequeno."));
            Line(entries, RegionBossRules.GrandfatherMammothId, RegionBossPhase.Storm, "The Grandfather throws the crystals of the walls!", "O Avô atira os cristais das paredes!");
            Line(entries, RegionBossRules.GrandfatherMammothId, RegionBossPhase.LastStand, "The whole gallery shakes!", "A galeria inteira treme!");
            Blow(entries, RegionBossRules.GrandfatherMammothId, "His foot comes down!", "Lá vem a pata!");
            Signature(entries, RegionBossRules.GrandfatherMammothId, "The ceiling is falling in a cross! Step slantwise!", "O teto vai cair em cruz! Pisa na diagonal!");
            Withdraw(entries, RegionBossRules.GrandfatherMammothId, "The Grandfather Mammoth lifts his trunk, remembers the way up, and goes digging towards a new valley.", "O Mamute Avô levanta a tromba, lembra o caminho para cima e sai cavando rumo a um vale novo.");

            // -- Jardins de Lava Laqueada (66) --
            Page(entries, RegionBossRules.ShisaKingId,
                "The shisa are the lion-dogs of the roofs of the islands far to the east, always in pairs: one with its mouth open to let the good things in, and one with its mouth shut to keep the bad things out. This one is the king of the palace roof. It came down to bark at a tremor and forgot how to go back up, and with nobody beside it, it does both jobs at once, one after the other. When it opens its mouth, it breathes in the whole garden, so get right up under its chin. When it shuts its mouth, it pushes everything out, so get far away. Open, close. Close, far. Free it and it climbs back onto the roof and barks at every tremor, just as it should.",
                "As shisa são os leões-cão dos telhados das ilhas lá longe no leste, sempre em par: uma de boca aberta pra deixar as coisas boas entrarem, e uma de boca fechada pra deixar as ruins de fora. Este é o rei do telhado do palácio. Desceu pra latir pra um tremor e esqueceu como voltar, e sem ninguém do lado faz os dois trabalhos, um depois do outro. Quando abre a boca, puxa o jardim inteiro pra dentro, então cheguem bem debaixo do queixo. Quando fecha a boca, empurra tudo pra fora, então fiquem bem longe. Aberta, perto. Fechada, longe. Libertem ele e ele sobe de volta pro telhado e late pra todo tremor, como deve ser.",
                ("Open, close in. Shut, back off. Say it out loud if you have to.", "Aberta, chega perto. Fechada, se afasta. Fala em voz alta se precisar."),
                ("It's doing the work of two because its other half isn't here. That's a lot for anybody.", "Está fazendo o trabalho de dois porque a outra metade não está aqui. É muito pra qualquer um."),
                ("Open mouth, I get close to the teeth. Great plan. Love it. Who made this plan?", "Boca aberta, eu chego perto dos dentes. Ótimo plano. Adorei. Quem fez esse plano?"),
                ("A guardian with no partner alternates. The pattern is the absence of the other one.", "Um guardião sem par alterna. O padrão é a falta do outro."),
                ("Good doggy! Big good doggy! Sit! ...It didn't sit.", "Cachorrinho bonzinho! Cachorrão bonzinho! Senta! ...Ele não sentou."));
            Line(entries, RegionBossRules.ShisaKingId, RegionBossPhase.Storm, "The Shisa King spits embers from the garden!", "O Rei Shisa cospe brasas do jardim!");
            Line(entries, RegionBossRules.ShisaKingId, RegionBossPhase.LastStand, "It barks like the whole island is shaking!", "Ele late como se a ilha inteira tremesse!");
            Blow(entries, RegionBossRules.ShisaKingId, "Its paw comes down!", "Lá vem a pata!");
            Signature(entries, RegionBossRules.ShisaKingId, "The Shisa King roars!", "O Rei Shisa ruge!");
            entries.Add(new LocalizationEntry(ShisaMouthKey(ShisaMouth.Open), "Open mouth! Get under its chin!", "Boca aberta! Pra debaixo do queixo!"));
            entries.Add(new LocalizationEntry(ShisaMouthKey(ShisaMouth.Closed), "Mouth shut! Get far away!", "Boca fechada! Pra bem longe!"));
            Withdraw(entries, RegionBossRules.ShisaKingId, "The Shisa King shakes its mane, climbs back up to the palace roof and barks once at the volcano.", "O Rei Shisa sacode a juba, sobe de volta para o telhado do palácio e late uma vez para o vulcão.");

            // -- Castelo das Nove Fornalhas (69) --
            Page(entries, RegionBossRules.FurnaceMasterId,
                "The Artificers built the Master of the Nine Furnaces to keep the greatest forge of the islands burning, and then they went away, and Āz found it and told it to forge for her. It has been forging ever since, because nobody ever taught it to stop. Every stroke of its hammer makes the next one hotter, nine of them, one for each furnace, and on the ninth the furnaces vent and it starts again cooler. So count, children. The first strokes you can take on your shield. By the eighth, get out from under it. At night the furnaces burn hotter still. Stop it and the nine furnaces become the forge of everyone, which is what they were for.",
                "Os Artífices fizeram o Mestre das Nove Fornalhas pra manter acesa a maior forja das ilhas, e depois foram embora, e Āz achou ele e mandou ele forjar pra ela. Forja desde então, porque ninguém nunca ensinou ele a parar. Cada martelada deixa a próxima mais quente, nove delas, uma por fornalha, e na nona as fornalhas soltam o vapor e ele recomeça mais frio. Então contem, crianças. As primeiras marteladas dá pra aguentar no escudo. Lá pela oitava, saiam de baixo. De noite as fornalhas queimam mais quente ainda. Parem ele e as nove fornalhas viram a forja de todo mundo, que é pra isso que elas eram.",
                ("Count the strokes. Seven, eight, and out from under the hammer.", "Conta as marteladas. Sete, oito, e sai de baixo do martelo."),
                ("Nobody ever told it it was allowed to rest. I'll tell it.", "Ninguém nunca disse pra ele que podia descansar. Eu vou dizer."),
                ("I like forges. I like hammers. I would like this hammer a lot less close to my face.", "Eu gosto de forja. Gosto de martelo. Gostaria bem mais desse martelo longe da minha cara."),
                ("Heat accumulates until it is released. Every machine needs somewhere for its heat to go. So does every person.", "O calor acumula até ser solto. Toda máquina precisa de um lugar pra pôr o calor. Toda pessoa também."),
                ("One, two, three... Joaquim, what comes after eight? Quick!", "Um, dois, três... Joaquim, o que vem depois do oito? Rápido!"));
            Line(entries, RegionBossRules.FurnaceMasterId, RegionBossPhase.Storm, "The Master throws sparks from the furnaces!", "O Mestre atira faíscas das fornalhas!");
            Line(entries, RegionBossRules.FurnaceMasterId, RegionBossPhase.LastStand, "All nine furnaces roar!", "As nove fornalhas rugem!");
            Blow(entries, RegionBossRules.FurnaceMasterId, "The great hammer comes down!", "O martelo grande vai descer!");
            Signature(entries, RegionBossRules.FurnaceMasterId, "It strikes the anvil! Each stroke is hotter!", "Ele bate na bigorna! Cada martelada é mais quente!");
            Withdraw(entries, RegionBossRules.FurnaceMasterId, "The Master of the Nine Furnaces lowers its hammer. The furnaces breathe out and burn for everyone now.", "O Mestre das Nove Fornalhas abaixa o martelo. As fornalhas soltam o ar e agora queimam para todo mundo.");
        }

        /// <summary>The map bosses on the road of the middle story (Docs/Mapas100.md 25, 26, 32, 37, 38, 43).</summary>
        private static void AddMidMapBossPages(List<LocalizationEntry> entries)
        {
            // -- Taa Pwani e o Recife (25) --
            Page(entries, RegionBossRules.MonsoonOctopusId,
                "Every year the monsoon wind comes over the sea and fills the sails of the dhows, and every year the dhows of Taa Pwani go out with it. This year the wind came and the boats stayed on the sand, because an octopus gone dark had wrapped its arms round the masts and would not let go. It is not greedy, children: it is afraid of the wind, of everything that leaves and might not come back. When it slams its eight arms down round itself, don't stay under its body and don't stand on an arm. Step out a little and stand in the gap between two arms. And watch: the next slam the arms come down where the gaps were. Free it and it lets go of the sails, and the monsoon carries the boats out again.",
                "Todo ano o vento da monção vem do mar e enche as velas das dhows, e todo ano as dhows de Taa Pwani saem com ele. Este ano o vento veio e os barcos ficaram na areia, porque um polvo apagado enrolou os braços nos mastros e não quis soltar. Ele não é guloso, crianças: tem medo do vento, de tudo que vai embora e pode não voltar. Quando ele bate os oito braços em volta de si, não fiquem debaixo do corpo e não fiquem em cima de um braço. Saiam um pouco e fiquem no vão entre dois braços. E reparem: na batida seguinte os braços caem onde estavam os vãos. Libertem ele e ele solta as velas, e a monção leva os barcos de novo.",
                ("Out of the middle, into a gap. Next slam, the next gap over.", "Sai do meio, entra num vão. Na outra batida, o vão do lado."),
                ("It holds on to the boats because it's scared they'll never come back. They always come back.", "Ele segura os barcos porque tem medo que eles nunca voltem. Eles sempre voltam."),
                ("Eight arms. I have two. This is not a fair hug.", "Oito braços. Eu tenho dois. Esse abraço não é justo."),
                ("Eight arms, eight gaps. The gaps turn as well. Danger and safety trade places, like the tide.", "Oito braços, oito vãos. Os vãos giram também. Perigo e abrigo trocam de lugar, igual à maré."),
                ("Let go of the boat! Let go! ...Vó, can octopuses say sorry?", "Solta o barco! Solta! ...Vó, polvo sabe pedir desculpa?"));
            Line(entries, RegionBossRules.MonsoonOctopusId, RegionBossPhase.Storm, "The Octopus spits dark ink!", "O Polvo cospe tinta escura!");
            Line(entries, RegionBossRules.MonsoonOctopusId, RegionBossPhase.LastStand, "It grabs at every sail at once!", "Ele agarra todas as velas de uma vez!");
            Blow(entries, RegionBossRules.MonsoonOctopusId, "Its great arm comes down!", "Lá vem o braço grande!");
            Signature(entries, RegionBossRules.MonsoonOctopusId, "Eight arms! Stand in a gap!", "Oito braços! Fica num vão!");
            Withdraw(entries, RegionBossRules.MonsoonOctopusId, "The Monsoon Octopus lets go of the masts and slips back into the sea. The wind fills the sails.", "O Polvo das Monções solta os mastros e escorrega de volta para o mar. O vento enche as velas.");

            // -- Savana dos Auroques (26) --
            Page(entries, RegionBossRules.SmokeAurochsId,
                "The aurochs are the old wild cattle, bigger than any ox, and the bull-folk of Aratta keep the memory of them in their horns. This one was the leader of the herd. The smoke of the old grass got into it, the dark smoke, not the clean low fire the bull-folk use to renew the savanna, and now it tramples the grass and doesn't know where the herd is. When it lowers its head and snorts, the smoke goes all round it, in front and to the sides, children. Only straight behind it is the air clean, where the herd always walked. Go behind the bull, or go far away. Free it and it remembers the herd, and it walks in front of Aratta's cattle again, where a leader belongs.",
                "Os auroques são o gado selvagem antigo, maior que qualquer boi, e o povo-touro de Aratta guarda a lembrança deles nos chifres. Este era o guia da manada. A fumaça do capim velho entrou nele, a fumaça escura, não o fogo baixo e limpo que o povo-touro usa pra renovar a savana, e agora ele pisoteia o capim e não sabe onde está a manada. Quando ele abaixa a cabeça e bufa, a fumaça vai pra todo lado, na frente e dos lados, crianças. Só bem atrás dele o ar fica limpo, onde a manada sempre andou. Vão pra trás do touro, ou vão pra longe. Libertem ele e ele lembra da manada, e volta a andar na frente do gado de Aratta, que é o lugar de um guia.",
                ("Head down means smoke. Get round behind it, fast.", "Cabeça baixa é fumaça. Dá a volta e fica atrás dele, rápido."),
                ("It's not angry. It lost its herd. Everyone gets a bit wild when they lose their people.", "Ele não está bravo. Perdeu a manada. Todo mundo fica meio bravo quando perde a sua gente."),
                ("Running behind a bull on purpose. Vó Edna, write down that it was your idea.", "Correr pra trás de um touro de propósito. Vó Edna, anota aí que a ideia foi sua."),
                ("The herd walks behind the leader, so behind is where it learned no one can be hurt.", "A manada anda atrás do guia, então atrás é onde ele aprendeu que ninguém pode se machucar."),
                ("Moo! MOO! ...I'm talking to it in cow. It didn't understand.", "Muu! MUUU! ...Eu tô falando com ele em vaquês. Ele não entendeu."));
            Line(entries, RegionBossRules.SmokeAurochsId, RegionBossPhase.Storm, "The Aurochs kicks up burning cinders!", "O Auroque levanta brasas do capim!");
            Line(entries, RegionBossRules.SmokeAurochsId, RegionBossPhase.LastStand, "The whole savanna shakes under its hooves!", "A savana inteira treme debaixo dos cascos!");
            Blow(entries, RegionBossRules.SmokeAurochsId, "Its horns come down!", "Lá vêm os chifres!");
            Signature(entries, RegionBossRules.SmokeAurochsId, "It snorts smoke! Get behind it!", "Ele vai bufar fumaça! Fica atrás dele!");
            Withdraw(entries, RegionBossRules.SmokeAurochsId, "The Smoke Aurochs shakes the dark smoke from its hide and trots off towards Aratta to lead the herd.", "O Touro de Fumaça sacode a fumaça escura do couro e sai trotando para Aratta, para guiar a manada.");

            // -- Pontes de Raiz Viva (32) --
            Page(entries, RegionBossRules.ForgetfulBridgeId,
                "The people of Jhilpur don't build their bridges, children, they grow them: they guide the roots of the fig trees across the gorges with the Living Water, for years, until the roots hold. One of those bridges was left alone so long that it forgot it was a bridge. It pulled its roots out of the rock and walked off. It still has its planks of root, but it keeps forgetting them: when it stomps, every other plank across your path forgets itself and drops. Going sideways does no good, the planks are long. Step forward or step back, one plank. And the next stomp it forgets the other ones. Free it and it remembers, and it puts its roots back over the gorge, and there is a shortcut there forever.",
                "O povo de Jhilpur não constrói as pontes, crianças, planta: guia as raízes das figueiras por cima das gargantas com a Água Viva, por anos, até as raízes aguentarem. Uma dessas pontes ficou sozinha tanto tempo que esqueceu que era ponte. Arrancou as raízes da pedra e saiu andando. Ainda tem as tábuas de raiz, mas vive esquecendo delas: quando ela pisa forte, uma tábua sim, uma não, no caminho de vocês esquece de si e cai. Ir pro lado não adianta, as tábuas são compridas. Deem um passo pra frente ou pra trás, uma tábua. E na pisada seguinte ela esquece as outras. Libertem ela e ela lembra, e põe as raízes de volta por cima da garganta, e ali fica um atalho pra sempre.",
                ("Forward or back, never sideways. One plank, every stomp.", "Pra frente ou pra trás, nunca pro lado. Uma tábua, a cada pisada."),
                ("Imagine forgetting what you are. I'd want someone to remind me gently.", "Imagina esquecer o que você é. Eu ia querer alguém me lembrando com carinho."),
                ("A bridge that walks away. Somewhere an engineer is crying.", "Uma ponte que sai andando. Em algum lugar um engenheiro está chorando."),
                ("Memory holds a thing together. Lose it and you fall apart one plank at a time.", "A memória segura as coisas. Perde ela e você desmonta uma tábua de cada vez."),
                ("Little bridge, you're a bridge! Bridges stay still! Like this! Look at me!", "Pontezinha, você é uma ponte! Ponte fica parada! Assim! Olha eu!"));
            Line(entries, RegionBossRules.ForgetfulBridgeId, RegionBossPhase.Storm, "The Bridge throws splinters of root!", "A Ponte atira lascas de raiz!");
            Line(entries, RegionBossRules.ForgetfulBridgeId, RegionBossPhase.LastStand, "It's forgetting everything at once!", "Ela está esquecendo tudo de uma vez!");
            Blow(entries, RegionBossRules.ForgetfulBridgeId, "Its root comes down!", "Lá vem a raiz!");
            Signature(entries, RegionBossRules.ForgetfulBridgeId, "It forgets the planks! One step forward or back!", "Ela vai esquecer as tábuas! Um passo pra frente ou pra trás!");
            Withdraw(entries, RegionBossRules.ForgetfulBridgeId, "The Bridge That Forgot remembers. It walks back to the gorge and lays its roots across it again.", "A Ponte que Esqueceu lembra. Volta para a garganta e estende de novo as raízes por cima dela.");

            // -- Calhas de Pedra (37) --
            Page(entries, RegionBossRules.RunawayCartId,
                "The Artificers made the chutes of Sangkent to bring the ore down the mountain, and they made carts with a little engine in them so the carts would climb back up on their own. One of the engines went dark and forgot how to stop. Now it runs up and down the slope with nobody inside, and if it sees you it comes at you. The thing about a runaway cart, children, is that it needs room. From far away it gets going and hits like an avalanche; from close up it has no room to speed up and only bumps you. So don't run from it: stay close, and step out of where it's aiming. On cold nights the chutes freeze and it runs faster still. Free it and it becomes the fastest cart in Sangkent, and it will give you a ride.",
                "Os Artífices fizeram as calhas de Sangkent pra descer o minério da montanha, e fizeram carrinhos com um engenhozinho dentro pra eles subirem de volta sozinhos. Um dos engenhos apagou e esqueceu como parar. Agora ele sobe e desce a encosta sem ninguém dentro, e se vê vocês, vem pra cima. O negócio de um carrinho desgovernado, crianças, é que ele precisa de espaço. De longe ele embala e bate feito avalanche; de perto ele não tem espaço pra correr e só dá um tranco. Então não fujam dele: fiquem perto, e saiam de onde ele está mirando. Nas noites frias as calhas congelam e ele corre mais ainda. Libertem ele e ele vira o carrinho mais rápido de Sangkent, e vai dar carona pra vocês.",
                ("Don't give it a run-up. Stay close, then sidestep.", "Não dá distância pra ele embalar. Fica perto e depois sai do lado."),
                ("It just wants someone to carry. An empty cart is a sad cart.", "Ele só quer alguém pra levar. Carrinho vazio é carrinho triste."),
                ("The safest place is right next to the runaway cart. I hate this mountain.", "O lugar mais seguro é colado no carrinho desgovernado. Eu odeio essa montanha."),
                ("Speed is distance spent. Give it no distance and it has nothing to spend.", "Velocidade é distância gasta. Não dá distância e ele não tem o que gastar."),
                ("Can I ride it after? Can I? Can I ride it first?", "Eu posso andar nele depois? Posso? Posso andar primeiro?"));
            Line(entries, RegionBossRules.RunawayCartId, RegionBossPhase.Storm, "The Cart spits glowing ore!", "O Carrinho cospe minério aceso!");
            Line(entries, RegionBossRules.RunawayCartId, RegionBossPhase.LastStand, "Its engine is screaming!", "O engenho dele está gritando!");
            Blow(entries, RegionBossRules.RunawayCartId, "It tips its whole load!", "Ele vai virar a carga inteira!");
            Signature(entries, RegionBossRules.RunawayCartId, "It's rolling! The farther it runs, the harder it hits!", "Ele vai descer! Quanto mais longe, mais forte bate!");
            Withdraw(entries, RegionBossRules.RunawayCartId, "The Runaway Cart's engine coughs, settles and purrs. It rolls gently off towards Sangkent's station.", "O engenho do Carrinho Desgovernado tosse, assenta e ronrona. Ele desce devagarinho para a estação de Sangkent.");

            // -- Geleira do Teto do Mundo (38) --
            Page(entries, RegionBossRules.GreatAlmasId,
                "In the stories of the steppes and the high mountains, the almas is the wild man of the snow, hairy all over and very shy, who throws snow at anyone who comes too near. This is the biggest of them. He found sparks of light frozen in the glacier, and he was so afraid someone would take them that he hid them in the snow and sat on top. When he roars, the snow bursts all round him, children. But he can't help keeping his sparks safe, even then: the three places where he hid them, the snow passes by. Stand on a spark. He hides them somewhere new each time, so look before you step. At night he comes out and the cold doubles. Free him and he shows you where every spark is, and he becomes the guide of the glacier.",
                "Nas histórias das estepes e das montanhas altas, o almas é o homem selvagem da neve, peludo inteirinho e muito tímido, que joga neve em quem chega perto demais. Este é o maior deles. Achou faíscas de luz congeladas na geleira, e teve tanto medo que alguém pegasse que escondeu elas na neve e sentou em cima. Quando ele urra, a neve estoura toda em volta dele, crianças. Mas nem assim ele deixa de proteger as faíscas: nos três lugares onde ele escondeu, a neve passa reto. Fiquem em cima de uma faísca. Ele esconde num lugar novo a cada vez, então olhem antes de pisar. De noite ele sai e o frio dobra. Libertem ele e ele mostra onde está cada faísca, e vira o guia da geleira.",
                ("Find the spark, stand on it. It moves round every throw.", "Acha a faísca, fica em cima. Ela gira a cada arremesso."),
                ("He's hiding the light because he's scared of losing it. We'll show him light grows when you share it.", "Ele esconde a luz porque tem medo de perder. A gente mostra que luz cresce quando divide."),
                ("A giant snowball fight against a giant. I'm losing, but I'm having fun.", "Guerra de bola de neve contra um gigante. Tô perdendo, mas tô me divertindo."),
                ("He protects the sparks even while he attacks. What we love most is what we can't stop guarding.", "Ele protege as faíscas até no meio do ataque. O que a gente mais ama é o que a gente não consegue parar de guardar."),
                ("He's so fluffy and so shy! Mister Almas, I'm shy too! We can be shy together!", "Ele é tão peludo e tão tímido! Almas, eu também sou tímida! A gente pode ser tímido junto!"));
            Line(entries, RegionBossRules.GreatAlmasId, RegionBossPhase.Storm, "The Great Almas throws snowballs!", "O Grande Almas atira bolas de neve!");
            Line(entries, RegionBossRules.GreatAlmasId, RegionBossPhase.LastStand, "He roars and the whole glacier answers!", "Ele urra e a geleira inteira responde!");
            Blow(entries, RegionBossRules.GreatAlmasId, "His huge fists come down!", "Lá vêm os punhos enormes!");
            Signature(entries, RegionBossRules.GreatAlmasId, "The snow is bursting! Stand on a spark!", "A neve vai estourar! Fica em cima de uma faísca!");
            entries.Add(new LocalizationEntry(SnowSparkKey, "Spark!", "Faísca!"));
            Withdraw(entries, RegionBossRules.GreatAlmasId, "The Great Almas digs up his sparks, holds them out shyly, and lumbers off to show the way across the glacier.", "O Grande Almas desenterra as faíscas, oferece com timidez e sai pesadão para mostrar o caminho da geleira.");

            // -- Canal das Lanternas (43) --
            Page(entries, RegionBossRules.UnleapedCarpId,
                "In the old tale of the canal, the carp that swims all the way up the river and leaps the great waterfall becomes a dragon of light. This one swam all the way, children. It got to the foot of the falls and was afraid, and it never leapt, and it stayed there in the dark water until it went dark too. Now it leaps at everyone, and never lands where it aims: once it falls short, halfway there, and the next time it flies right past you. Short, then long. Count the leaps. When it falls short, don't run towards it; when it flies long, don't run away. Free it and it leaps at last, the waterfall and all, and it becomes an arch of light over the canal.",
                "No conto antigo do canal, a carpa que nada o rio inteiro e pula a cascata grande vira um dragão de luz. Esta nadou o caminho todo, crianças. Chegou no pé da cascata e teve medo, e nunca pulou, e ficou ali na água escura até apagar também. Agora pula em todo mundo, e nunca cai onde mira: uma vez cai antes, no meio do caminho, e na outra passa voando por cima de vocês. Curto, depois comprido. Contem os pulos. Quando ela cair curto, não corram pra perto dela; quando ela pular comprido, não fujam pra longe. Libertem ela e ela pula enfim, cascata e tudo, e vira um arco de luz por cima do canal.",
                ("Short, then long. Short: don't go in. Long: don't back off.", "Curto, depois comprido. Curto: não chega perto. Comprido: não recua."),
                ("It was one leap away and got scared. I've been one leap away from things too.", "Ela estava a um pulo e ficou com medo. Eu também já fiquei a um pulo das coisas."),
                ("A fish that misses on purpose. Finally, someone with worse aim than me.", "Um peixe que erra de propósito. Finalmente alguém com a mira pior que a minha."),
                ("Fear doesn't stop the leap. It only changes where you land.", "O medo não impede o pulo. Só muda onde a gente cai."),
                ("Jump, fishy! You can do it! I'll jump too! ...I can't jump a waterfall.", "Pula, peixinha! Você consegue! Eu pulo junto! ...Eu não consigo pular cascata."));
            Line(entries, RegionBossRules.UnleapedCarpId, RegionBossPhase.Storm, "The Carp spits lantern light!", "A Carpa cospe luz de lanterna!");
            Line(entries, RegionBossRules.UnleapedCarpId, RegionBossPhase.LastStand, "The whole canal churns!", "O canal inteiro se agita!");
            Blow(entries, RegionBossRules.UnleapedCarpId, "Its tail comes down!", "Lá vem o rabo!");
            Signature(entries, RegionBossRules.UnleapedCarpId, "The Carp leaps!", "A Carpa vai pular!");
            entries.Add(new LocalizationEntry(CarpLeapKey(true), "It leaps short! Don't run towards it!", "Pulo curto! Não corre pra perto dela!"));
            entries.Add(new LocalizationEntry(CarpLeapKey(false), "It leaps long! Don't back away!", "Pulo comprido! Não recua!"));
            Withdraw(entries, RegionBossRules.UnleapedCarpId, "The Carp That Never Leapt leaps at last, over the falls, and becomes an arch of light across the canal.", "A Carpa que Não Pulou pula enfim, por cima da cascata, e vira um arco de luz por cima do canal.");
        }
    }
}
