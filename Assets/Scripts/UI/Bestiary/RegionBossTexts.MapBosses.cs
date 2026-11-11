using System.Collections.Generic;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Enemies;

namespace Rapadura.UI.Bestiary
{
    /// <summary>
    /// "Caderno da Vó Edna" pages of the map bosses of the fichas (Docs/Mapas100.md 04, 09, 15, 25, 26, 32, 34, 37, 38, 43, 45, 48, 55, 56, 60, 62, 66, 69; <see cref="RegionBossRules.MapSpawns"/>)
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

        /// <summary>What the Great Smudge's smear leaves clean (<see cref="RegionBossRules.SmudgeColourFor"/>), called out as it warns.</summary>
        public static string SmudgeColourKey(SmudgeColour clean) => $"region_boss.great_smudge.colour.{clean.ToString().ToLowerInvariant()}";

        /// <summary>What Nian does when the hero's noise frightens it out of its pounce (<see cref="RegionBossRules.NianTakesFright"/>).</summary>
        public const string NianFrightKey = "region_boss.nian.fright";

        private static void AddMapBossPages(List<LocalizationEntry> entries)
        {
            AddMidMapBossPages(entries);
            AddFirstMapBossPages(entries);

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

        private static void AddFirstMapBossPages(List<LocalizationEntry> entries)
        {
            // -- Bosque das Amoreiras de Seda (04) --
            Page(entries, RegionBossRules.ForgottenSpinnerId,
                "Before the looms of the Casa do Tear, there was a spinner in the mulberry grove: a great spider who spun the finest silk of the Zarafshan and gave it away to anyone who asked. Then a spark fell into her web, children, and she was so afraid of losing it that she wrapped it in a cocoon and forgot everything else, even her own name. Now she ties a thread to whoever comes near and pulls. Here is the trick: a thread only hurts when it is taut. When she ties it, walk towards her, and it hangs slack; run away and it throws you down. Free her and she unwraps the spark, and the Casa do Tear will have the best weaver it ever had.",
                "Antes dos teares da Casa do Tear, tinha uma fiandeira no bosque das amoreiras: uma aranha enorme que fiava a seda mais fina do Zarafshan e dava pra quem pedisse. Aí uma faísca caiu na teia dela, crianças, e ela teve tanto medo de perder que enrolou num casulo e esqueceu de todo o resto, até do próprio nome. Agora ela amarra um fio em quem chega perto e puxa. O segredo é este: fio só machuca quando está esticado. Quando ela amarrar, andem na direção dela, e o fio fica frouxo; fujam e ele derruba vocês. Libertem ela e ela desenrola a faísca, e a Casa do Tear vai ter a melhor tecelã que já teve.",
                ("When she ties you, step in. Slack thread, no fall.", "Quando ela te amarrar, chega perto. Fio frouxo, ninguém cai."),
                ("She held on to the spark so hard she forgot herself. Holding on isn't the same as keeping.", "Ela segurou tanto a faísca que esqueceu de si. Segurar não é o mesmo que guardar."),
                ("The rule is: when the giant spider pulls, you run TO the giant spider. Great. Love it.", "A regra é: quando a aranha gigante puxar, você corre PRA aranha gigante. Ótimo. Adorei."),
                ("A thread pulls only as far as the distance you give it. Take the distance away and it has no force.", "O fio só puxa até onde a distância deixa. Tira a distância e ele fica sem força."),
                ("Can she make me a scarf after? A small one. A purple one.", "Ela pode fazer um cachecol pra mim depois? Pequenininho. Roxo."));
            Line(entries, RegionBossRules.ForgottenSpinnerId, RegionBossPhase.Storm, "The Spinner throws balls of silk!", "A Fiandeira atira novelos de seda!");
            Line(entries, RegionBossRules.ForgottenSpinnerId, RegionBossPhase.LastStand, "She spins the whole grove shut!", "Ela fecha o bosque inteiro com fios!");
            Blow(entries, RegionBossRules.ForgottenSpinnerId, "Her legs come down!", "Lá vêm as patas!");
            Signature(entries, RegionBossRules.ForgottenSpinnerId, "She tied a thread to you! Go towards her before she pulls!", "Ela amarrou um fio em você! Chega perto antes que ela puxe!");
            Withdraw(entries, RegionBossRules.ForgottenSpinnerId, "The Forgotten Spinner unwraps the cocoon, and the spark inside lights her up. She remembers her name and climbs off towards the Casa do Tear.", "A Fiandeira Esquecida desenrola o casulo, e a faísca de dentro acende ela. Ela lembra o próprio nome e sobe para a Casa do Tear.");

            // -- Galerias do Karez (09) --
            Page(entries, RegionBossRules.WellDrinkingRootId,
                "The karez are the tunnels the old people of the oasis dug to bring water from the mountains under the sand, so it would not dry in the sun. At the end of them there was a vine that drank only what it needed, and gave shade and grapes to everyone. The shadow got into its roots, children, and it drank, and drank, and drank the well dry, and it still feels thirsty. Its roots follow you under the ground. Wherever you step while it is groaning, a root will come up. So don't stand still and don't go back over your own steps: keep walking onto new ground. Free it and it gives the water back to the oasis, and Qocho will have grapes again.",
                "Os karez são os túneis que o povo antigo do oásis cavou pra trazer a água da montanha por baixo da areia, pra ela não secar no sol. No fim deles tinha uma parreira que bebia só o que precisava, e dava sombra e uva pra todo mundo. A sombra entrou nas raízes dela, crianças, e ela bebeu, e bebeu, e bebeu o poço até secar, e ainda sente sede. As raízes seguem vocês por baixo da terra. Onde vocês pisarem enquanto ela geme, sobe uma raiz. Então não fiquem parados e não voltem pelos próprios passos: continuem andando pra chão novo. Libertem ela e ela devolve a água pro oásis, e Qocho vai ter uva de novo.",
                ("It groans, I keep moving. Never back over my own tracks.", "Ela gemeu, eu continuo andando. Nunca volto pelos meus passos."),
                ("It drank everything and it's still thirsty. Taking more never fills you up.", "Ela bebeu tudo e ainda tem sede. Pegar mais nunca enche ninguém."),
                ("Roots chasing my feet. Underground. Vó, the desert was supposed to be the easy one.", "Raiz correndo atrás do meu pé. Debaixo da terra. Vó, o deserto era pra ser o fácil."),
                ("It can only follow where we've been. It has no idea where we're going.", "Ela só consegue seguir onde a gente já esteve. Não faz ideia de pra onde a gente vai."),
                ("I'll walk in a big circle! ...Oh no, a circle comes back. I'll walk in a big line!", "Vou andar num círculo grandão! ...Ih, círculo volta. Vou andar numa linha grandona!"));
            Line(entries, RegionBossRules.WellDrinkingRootId, RegionBossPhase.Storm, "The Root spits the mud of the well!", "A Raiz cospe a lama do poço!");
            Line(entries, RegionBossRules.WellDrinkingRootId, RegionBossPhase.LastStand, "Every root of the karez is waking!", "Todas as raízes do karez estão acordando!");
            Blow(entries, RegionBossRules.WellDrinkingRootId, "Its great root comes down!", "Lá vem a raiz grossa!");
            Signature(entries, RegionBossRules.WellDrinkingRootId, "The roots follow your steps! Keep walking onto new ground!", "As raízes seguem seus passos! Continua andando pra chão novo!");
            Withdraw(entries, RegionBossRules.WellDrinkingRootId, "The Root That Drank the Well lets the water go. It runs back down the karez, and the vine puts out its first green leaf.", "A Raiz que Bebeu o Poço solta a água. Ela corre de volta pelo karez, e a parreira bota a primeira folha verde.");

            // -- Abrigos Pintados (15) --
            Page(entries, RegionBossRules.GreatSmudgeId,
                "Under the sandstone walls of the Kaggen there are paintings older than any of us: elands, people dancing, rain falling in white lines. The little smudges are shadows that come out of the paintings and eat their colours. This one ate so many that it grew as big as a shelter, and it is made of all the colours it took, gone grey. When it shudders, it smears the ground round itself in three bands, near, middle and far. But there is always one colour it has not eaten yet, and that band stays clean: it shouts which. Ochre is right next to it, red is in the middle, white is the far band. Free it and it gives the colours back to the walls.",
                "Debaixo dos paredões de arenito do Kaggen tem pinturas mais velhas que qualquer um de nós: elands, gente dançando, chuva caindo em riscos brancos. Os borrõezinhos são sombras que saem das pinturas e comem as cores delas. Este comeu tanto que ficou do tamanho de um abrigo, e é feito de todas as cores que pegou, já cinzentas. Quando ele treme, borra o chão em volta em três faixas, perto, meio e longe. Mas sempre tem uma cor que ele ainda não comeu, e essa faixa fica limpa: ele grita qual. Ocre é coladinho nele, vermelho é no meio, branco é a faixa de longe. Libertem ele e ele devolve as cores pras paredes.",
                ("Listen for the colour. Ochre close, red middle, white far.", "Escuta a cor. Ocre perto, vermelho no meio, branco longe."),
                ("It ate the colours because it had none of its own. We can share ours.", "Ele comeu as cores porque não tinha nenhuma. A gente pode dividir as nossas."),
                ("A painting that wants to smear me. That's the art critic I deserve.", "Uma pintura que quer me borrar. É o crítico de arte que eu mereço."),
                ("The one colour it hasn't taken is the one place it can't touch. What we still have keeps us.", "A única cor que ele não pegou é o único lugar que ele não alcança. O que a gente ainda tem é o que guarda a gente."),
                ("Give back the eland's colour! It's brown! Elands are brown, everybody knows that!", "Devolve a cor do eland! É marrom! Eland é marrom, todo mundo sabe!"));
            Line(entries, RegionBossRules.GreatSmudgeId, RegionBossPhase.Storm, "The Smudge throws blots of grey!", "O Borrão atira manchas cinzentas!");
            Line(entries, RegionBossRules.GreatSmudgeId, RegionBossPhase.LastStand, "It's eating the colours of the whole wall!", "Ele está comendo as cores do paredão inteiro!");
            Blow(entries, RegionBossRules.GreatSmudgeId, "It falls on you like spilled paint!", "Ele cai em cima feito tinta derramada!");
            Signature(entries, RegionBossRules.GreatSmudgeId, "It's smearing the ground! Stand in the clean colour!", "Ele vai borrar o chão! Fica na cor limpa!");
            entries.Add(new LocalizationEntry(SmudgeColourKey(SmudgeColour.Ochre), "Ochre is clean: stand next to it!", "O ocre está limpo: fica colado nele!"));
            entries.Add(new LocalizationEntry(SmudgeColourKey(SmudgeColour.Red), "Red is clean: the middle band!", "O vermelho está limpo: a faixa do meio!"));
            entries.Add(new LocalizationEntry(SmudgeColourKey(SmudgeColour.White), "White is clean: the far band!", "O branco está limpo: a faixa de longe!"));
            Withdraw(entries, RegionBossRules.GreatSmudgeId, "The Great Smudge breaks into colours, ochre, red and white, and they run back up the wall into the elands and the dancers.", "O Grande Borrão se desfaz em cores, ocre, vermelho e branco, e elas sobem de volta pelo paredão para os elands e os dançarinos.");

            // -- Charco do Ninki Nanka (34) --
            Page(entries, RegionBossRules.NinkiNankaId,
                "In the stories of the rivers of the far west of the old world, the ninki nanka is the dragon of the bog, and they say whoever looks it in the eye falls sick. The one in the Brejo das Palmeiras went dark, and she is not cruel, children: she is shy, terribly shy. When she shines it is because she feels everyone staring at her, and whoever is facing her when she glows feels the sickness of her shame. So when she starts to glow, turn your back. Don't stare. That's all she ever wanted. Free her and she lets you through the bog, and she gives each of you a scale.",
                "Nas histórias dos rios do extremo oeste do mundo velho, a ninki nanka é a dragoa do charco, e dizem que quem olha nos olhos dela fica doente. A do Brejo das Palmeiras apagou, e ela não é cruel, crianças: é tímida, tímida demais. Quando ela brilha é porque sente todo mundo encarando, e quem estiver de frente pra ela quando ela acender sente o enjoo da vergonha dela. Então quando ela começar a brilhar, virem de costas. Não encarem. Era só isso que ela queria. Libertem ela e ela deixa vocês passarem pelo charco, e dá uma escama pra cada um.",
                ("She glows, I turn my back. Then I turn round and hit.", "Ela brilhou, eu viro de costas. Depois viro e bato."),
                ("Imagine being so shy that being looked at hurts. Let's look away for her.", "Imagina ser tão tímida que ser olhada dói. Vamos olhar pro outro lado por ela."),
                ("Turning my back on a dragon. On purpose. In a swamp. This family is something else.", "Virar de costas pra uma dragoa. De propósito. Num pântano. Essa família é demais."),
                ("Not looking is also a way of respecting. Some things we only see by letting them be.", "Não olhar também é um jeito de respeitar. Tem coisa que a gente só enxerga deixando em paz."),
                ("I'm closing my eyes! I'm not looking! ...Can I peek? Just a tiny peek?", "Tô de olho fechado! Não tô olhando! ...Posso espiar? Só uma espiadinha?"));
            Line(entries, RegionBossRules.NinkiNankaId, RegionBossPhase.Storm, "Ninki Nanka spits bog water!", "A Ninki Nanka cospe água do charco!");
            Line(entries, RegionBossRules.NinkiNankaId, RegionBossPhase.LastStand, "She hides in the reeds and lashes out!", "Ela se esconde nos juncos e ataca!");
            Blow(entries, RegionBossRules.NinkiNankaId, "Her tail comes down!", "Lá vem a cauda!");
            Signature(entries, RegionBossRules.NinkiNankaId, "She's glowing! Turn your back, don't look!", "Ela vai brilhar! Vira de costas, não olha!");
            Withdraw(entries, RegionBossRules.NinkiNankaId, "Ninki Nanka hides her face in the reeds, leaves a shining scale on the boardwalk and slides away into the dark water.", "A Ninki Nanka esconde o rosto nos juncos, deixa uma escama brilhando nas tábuas e escorrega para a água escura.");

            // -- Muralha das Mil Torres (45) --
            Page(entries, RegionBossRules.NianId,
                "In the old tales of the far east, the nian is the beast that comes down from the mountains at the turn of the year to eat the harvest, and the people chase it away with red lanterns and a great deal of noise. This one went dark and forgot that it is afraid. Fighting it is not about hitting hard, children: it is about scaring it. When it crouches to pounce, make noise, hit it, shout, bang the drum, and it remembers its fear and jumps back. Stand there quietly and it lands on you. At night it is bolder. Beaten, it doesn't die: it goes back to sleep in the peaks until next year, and the wall lights its beacons.",
                "Nos contos antigos do extremo leste, o nian é a fera que desce das montanhas na virada do ano pra comer a colheita, e o povo espanta ele com lanterna vermelha e muito barulho. Este apagou e esqueceu que tem medo. Lutar com ele não é bater forte, crianças: é assustar. Quando ele se agachar pra pular, façam barulho, batam nele, gritem, toquem o tambor, e ele lembra do medo e pula pra trás. Fiquem quietinhos parados e ele cai em cima. De noite ele fica mais ousado. Vencido, ele não morre: volta a dormir nos picos até o ano que vem, e a muralha acende os faróis.",
                ("It crouches, I hit it hard and fast. Noise first, dodging second.", "Ele agachou, eu bato forte e rápido. Primeiro o barulho, depois a esquiva."),
                ("It's not evil, it forgot to be afraid. A little fear keeps everyone home.", "Ele não é mau, esqueceu de ter medo. Um pouco de medo deixa todo mundo em casa."),
                ("Finally a fight where the plan is: be very, very loud. I was born for this.", "Finalmente uma luta em que o plano é: fazer muito, muito barulho. Eu nasci pra isso."),
                ("The noise isn't the weapon. It's a reminder. It already knows how to leave.", "O barulho não é a arma. É um lembrete. Ele já sabe ir embora."),
                ("BOO! BOOOO! ...It jumped! Did you see? It's scared of ME!", "BU! BUUUU! ...Ele pulou! Viu? Ele tem medo de MIM!"));
            Line(entries, RegionBossRules.NianId, RegionBossPhase.Storm, "Nian spits sparks of the old year!", "O Nian cospe faíscas do ano velho!");
            Line(entries, RegionBossRules.NianId, RegionBossPhase.LastStand, "It roars at the whole wall!", "Ele ruge para a muralha inteira!");
            Blow(entries, RegionBossRules.NianId, "Its great paw comes down!", "Lá vem a pata grande!");
            Signature(entries, RegionBossRules.NianId, "It's crouching to pounce! Make noise, hit it!", "Ele está se agachando pra pular! Faz barulho, bate nele!");
            entries.Add(new LocalizationEntry(NianFrightKey, "Nian takes fright and jumps back!", "O Nian se assusta e pula para trás!"));
            Withdraw(entries, RegionBossRules.NianId, "Nian shakes its mane, yawns and climbs back to the peaks to sleep until next year. One by one, the beacons on the wall light up.", "O Nian sacode a juba, boceja e sobe de volta para os picos para dormir até o ano que vem. Um a um, os faróis da muralha acendem.");

            // -- Kurgans de Ouro (48) --
            Page(entries, RegionBossRules.OldKurganGriffinId,
                "The old storytellers of the steppe wrote that griffins guard the gold of the north, and that no one comes near it. This one has guarded the gold in the kurgans for so long that he has forgotten whose it was, or why. He doesn't chase you for your sake, children: he only cares about his hoard, the ring of stones where he keeps it. When he screeches, he strikes everyone standing on his gold, wherever he is. So fight him away from the ring, and don't step on the treasure. Free him and he remembers, and he leaves a present for each of you before he flies off.",
                "Os contadores antigos da estepe escreveram que os grifos guardam o ouro do norte, e que ninguém chega perto. Este guarda o ouro dos kurgans há tanto tempo que esqueceu de quem era, e por quê. Ele não corre atrás de vocês por vocês, crianças: só liga pro tesouro, o anel de pedras onde guarda tudo. Quando ele grita, ataca todo mundo que estiver em cima do ouro dele, esteja ele onde estiver. Então lutem com ele longe do anel, e não pisem no tesouro. Libertem ele e ele lembra, e deixa um presente pra cada um antes de voar embora.",
                ("Pull him off the ring. Fight where the gold isn't.", "Puxa ele pra fora do anel. Luta onde não tem ouro."),
                ("Guarding something so long you forget why is just being lonely with extra steps.", "Guardar uma coisa tanto tempo que esquece por quê é só ficar sozinho com mais trabalho."),
                ("A giant bird that only gets angry if I touch its stuff. Honestly? Relatable.", "Um passarão que só fica bravo se eu mexer nas coisas dele. Sinceramente? Entendo."),
                ("What isn't given away doesn't stay yours. It only stays heavy.", "O que não se dá não fica sendo seu. Só fica pesado."),
                ("I'm not small, I'm just standing far from the gold! On purpose! Because I'm smart!", "Eu não sou pequena, só tô longe do ouro! De propósito! Porque eu sou esperta!"));
            Line(entries, RegionBossRules.OldKurganGriffinId, RegionBossPhase.Storm, "The Old Griffin throws golden feathers!", "O Grifo Velho atira penas douradas!");
            Line(entries, RegionBossRules.OldKurganGriffinId, RegionBossPhase.LastStand, "He spreads his wings over the whole hoard!", "Ele abre as asas por cima do tesouro inteiro!");
            Blow(entries, RegionBossRules.OldKurganGriffinId, "He dives!", "Ele mergulha!");
            Signature(entries, RegionBossRules.OldKurganGriffinId, "He screeches for his gold! Get off the ring!", "Ele grita pelo ouro! Sai do anel!");
            Withdraw(entries, RegionBossRules.OldKurganGriffinId, "The Old Griffin of the Kurgan sets down five little gold pieces, one for each cousin, and flies off north to the open sky.", "O Grifo Velho do Kurgan deixa cinco pecinhas de ouro, uma para cada primo, e voa para o norte, para o céu aberto.");
        }
    }
}
