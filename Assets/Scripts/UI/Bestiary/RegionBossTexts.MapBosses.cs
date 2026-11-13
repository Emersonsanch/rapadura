using System.Collections.Generic;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Enemies;

namespace Rapadura.UI.Bestiary
{
    /// <summary>
    /// "Caderno da Vó Edna" pages of the map bosses of the fichas (Docs/Mapas100.md 03, 04, 05, 09, 10, 11, 15, 17, 21, 22, 25, 26, 27, 32, 33, 34, 37, 38, 39, 42, 43, 45, 47, 48, 53, 55, 56, 60, 61, 62, 66, 68, 69, 73, 79, 93; <see cref="RegionBossRules.MapSpawns"/>)
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

        /// <summary>The verse the Dune Choir sings (<see cref="RegionBossRules.DuneVerseOf"/>), called out as it warns: how far the sand will burst.</summary>
        public static string DuneVerseKey(int verse) => $"region_boss.dune_choir.verse{RegionBossRules.DuneVerseOf(verse)}";

        /// <summary>The label over each great bottle the Greedy Bottler sets down (<see cref="RegionBossRules.BottleSpots"/>).</summary>
        public const string BottleKey = "region_boss.greedy_bottler.bottle";

        /// <summary>The label over the spot the Light Fisher casts his lure on (<see cref="RegionBossRules.LureSpot"/>).</summary>
        public const string LureKey = "region_boss.light_fisher.lure";

        /// <summary>Where the wind comes from in the Vane Lord's gust (<see cref="RegionBossRules.VaneWindFor"/>), called out as he warns.</summary>
        public static string VaneWindKey(VaneWind from) => $"region_boss.vane_lord.wind.{from.ToString().ToLowerInvariant()}";

        /// <summary>How much of the light the Little Eclipse covers (<see cref="RegionBossRules.EclipsePhaseOf"/>), called out as it warns.</summary>
        /// <summary>The label over each hatch of the Moon Ship's hold the Great Stowaway can burst out of (<see cref="RegionBossRules.HatchSpots"/>).</summary>
        public const string HatchKey = "region_boss.great_stowaway.hatch";

        /// <summary>The label over each tin crown the Crowned Chief throws (<see cref="RegionBossRules.CrownSpots"/>).</summary>
        public const string CrownKey = "region_boss.crowned_chief.crown";

        /// <summary>What the Dark Bellows calls out as it breathes (<see cref="RegionBossRules.BellowsBlowsOut"/>): blowing out in front, or drawing in behind.</summary>
        public static string BellowsBreathKey(bool blowsOut) => blowsOut ? "region_boss.dark_bellows.out" : "region_boss.dark_bellows.in";

        /// <summary>How far the Sleeping Vishap's springs rise (<see cref="RegionBossRules.SpringsRiseOf"/>), called out as it warns.</summary>
        public static string SpringsRiseKey(int rise) => $"region_boss.sleeping_vishap.rise{RegionBossRules.SpringsRiseOf(rise)}";

        public static string EclipseCoverKey(int cover) => $"region_boss.little_eclipse.cover{RegionBossRules.EclipsePhaseOf(cover)}";

        private static void AddMapBossPages(List<LocalizationEntry> entries)
        {
            AddMidMapBossPages(entries);
            AddFirstMapBossPages(entries);
            AddMoreMapBossPages(entries);
            AddHighMapBossPages(entries);
            AddUpperMapBossPages(entries);
            AddDeepMapBossPages(entries);
            AddSkyMapBossPages(entries);
            AddRiverMapBossPages(entries);
            AddLastFirstMapBossPages(entries);
            AddLastMapBossPages(entries);

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
                ("Running behind a bull on purpose. Grandma Edna, write down that it was your idea.", "Correr pra trás de um touro de propósito. Vó Edna, anota aí que a ideia foi sua."),
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

        private static void AddMoreMapBossPages(List<LocalizationEntry> entries)
        {
            // -- Copas de Garajonay (05) --
            Page(entries, RegionBossRules.MistElderId,
                "The laurel forest of Garajonay drinks the mist, children: the leaves catch it and it drips down to the roots. The Old One of the Mist is a lizard as big as a house who lived under those leaves for longer than the elves can count, licking the drops and guarding the forest floor. Then the shadow dried the drops on his tongue, and he has been thirsty and cross ever since. When the mist rises he hides in it and snaps at whoever turns their back on him. So don't run from him in the mist: face him. As long as you keep him in sight he cannot find a back to bite. Free him and he drinks again, and he goes back to keeping the forest.",
                "A mata de louro de Garajonay bebe a névoa, crianças: as folhas seguram e ela pinga até as raízes. O Velho da Névoa é um lagarto do tamanho de uma casa que viveu debaixo dessas folhas mais tempo do que os elfos sabem contar, lambendo as gotas e guardando o chão da mata. Aí a sombra secou as gotas na língua dele, e desde então ele vive com sede e de mau humor. Quando a névoa sobe ele se esconde nela e morde quem estiver de costas pra ele. Então não fujam dele na névoa: encarem. Enquanto vocês tiverem ele na vista, ele não acha costas pra morder. Libertem ele e ele volta a beber, e volta a guardar a mata.",
                ("The mist rises, I turn to him. Eyes on the lizard, always.", "A névoa subiu, eu viro pra ele. Olho no lagarto, sempre."),
                ("He's not mean, he's thirsty. Anyone gets cross when their mouth is dry.", "Ele não é mau, está com sede. Qualquer um fica bravo de boca seca."),
                ("So the rule with giant lizards is: stare at them. Not where I'd have started, but okay.", "Então a regra com lagarto gigante é: encarar. Não é por onde eu começaria, mas tá bom."),
                ("The mist hides only what we stop looking at.", "A névoa só esconde o que a gente para de olhar."),
                ("I'm looking at him SO hard. He can't bite me. I'm the best at looking.", "Tô olhando pra ele com TODA a força. Ele não pode me morder. Eu sou a melhor em olhar."));
            Line(entries, RegionBossRules.MistElderId, RegionBossPhase.Storm, "The Old One spits balls of dry moss!", "O Velho cospe bolas de musgo seco!");
            Line(entries, RegionBossRules.MistElderId, RegionBossPhase.LastStand, "He whips his tail through the laurels!", "Ele chicoteia o rabo pelos louros!");
            Blow(entries, RegionBossRules.MistElderId, "His great jaw comes down!", "Lá vem a bocarra!");
            Signature(entries, RegionBossRules.MistElderId, "The mist is rising! Face him, don't turn your back!", "A névoa está subindo! Encara ele, não vira de costas!");
            Withdraw(entries, RegionBossRules.MistElderId, "The Old One of the Mist opens his mouth to the mist, drinks his fill for the first time in years, and walks slowly back under the laurels to keep the forest.", "O Velho da Névoa abre a boca para a névoa, bebe à vontade pela primeira vez em anos e volta devagar para debaixo dos louros, guardando a mata.");

            // -- Dunas que Cantam (10) --
            Page(entries, RegionBossRules.DuneChoirId,
                "Have you ever heard a dune sing? When the sand slides down it hums, low, like a choir far away. The old people of the oasis said it was the dunes calling the light home at sunset. The Choir of the Dune is the worm that lives under the singing sand, and it went dark and kept on singing, only now its song calls the shadow. It sings in verses, children, and each verse makes the sand burst further round it: the first one close, the second further, the third far away, and then it starts again. Listen for the verse and step just past it. Free it and the dune sings beautifully again, and the light comes home.",
                "Vocês já ouviram uma duna cantar? Quando a areia escorrega ela zune, baixinho, feito um coral bem longe. O povo antigo do oásis dizia que eram as dunas chamando a luz pra casa no pôr do sol. O Coral da Duna é o verme que mora debaixo da areia que canta, e ele apagou e continuou cantando, só que agora o canto dele chama a sombra. Ele canta em versos, crianças, e cada verso faz a areia estourar mais longe em volta dele: o primeiro perto, o segundo mais longe, o terceiro bem longe, e aí começa de novo. Escutem o verso e passem um pouco além dele. Libertem ele e a duna volta a cantar bonito, e a luz volta pra casa.",
                ("First verse close, second further, third far. Count and step past it.", "Primeiro verso perto, segundo mais longe, terceiro longe. Conta e passa dele."),
                ("It never stopped singing, even in the dark. That's a kind of bravery.", "Ele nunca parou de cantar, nem no escuro. Isso é um tipo de coragem."),
                ("A worm with a choir. Of course. Why would the desert have a normal worm.", "Um verme com coral. Claro. Por que o deserto ia ter um verme normal."),
                ("A song has a shape. Learn the shape and you know where to stand.", "Toda música tem um desenho. Aprende o desenho e você sabe onde ficar."),
                ("I'm singing too! La-laaa! ...Why is everybody running?", "Eu também tô cantando! Lá-láaa! ...Por que todo mundo está correndo?"));
            Line(entries, RegionBossRules.DuneChoirId, RegionBossPhase.Storm, "The Choir spits sparks of hot sand!", "O Coral cospe faíscas de areia quente!");
            Line(entries, RegionBossRules.DuneChoirId, RegionBossPhase.LastStand, "The whole dune is singing!", "A duna inteira está cantando!");
            Blow(entries, RegionBossRules.DuneChoirId, "It rears up and crashes down!", "Ele se ergue e desaba!");
            Signature(entries, RegionBossRules.DuneChoirId, "It's singing a verse! Step past it!", "Ele vai cantar um verso! Passa além dele!");
            entries.Add(new LocalizationEntry(DuneVerseKey(0), "First verse: the sand bursts close to it!", "Primeiro verso: a areia estoura perto dele!"));
            entries.Add(new LocalizationEntry(DuneVerseKey(1), "Second verse: further out!", "Segundo verso: mais longe!"));
            entries.Add(new LocalizationEntry(DuneVerseKey(2), "Third verse: far out, get well away!", "Terceiro verso: bem longe, se afasta bastante!"));
            Withdraw(entries, RegionBossRules.DuneChoirId, "The Choir of the Dune sinks into the sand humming softly, and the whole dune sings with it, low and beautiful, calling the light home.", "O Coral da Duna afunda na areia cantarolando baixinho, e a duna inteira canta junto, grave e bonito, chamando a luz pra casa.");

            // -- Mercado dos Túneis (22) --
            Page(entries, RegionBossRules.GreedyBottlerId,
                "In the tunnels the air stands still, children, and the kobolds bottle the good air of the pipes, the Pure Air, and share it at the fair so nobody runs out of breath. The Greedy Bottler was the best of them at it, until Āz whispered in his ear that whoever holds all the air holds everyone. He bottled every breath he could find and hid it for her. Now he sets down his three biggest bottles and lets out the stale air all round him, and only behind a bottle can you breathe. Keep a bottle between you and him. Beaten, he uncorks the bottles, and he ends up helping at the fair, handing the air out again.",
                "Nos túneis o ar fica parado, crianças, e os kobolds engarrafam o ar bom dos canos, o Ar Sereno, e dividem na feira pra ninguém ficar sem fôlego. O Engarrafador Guloso era o melhor deles nisso, até Āz cochichar no ouvido dele que quem segura todo o ar segura todo mundo. Ele engarrafou cada fôlego que achou e guardou pra ela. Agora ele bota no chão as três garrafas maiores e solta o ar parado em volta dele, e só atrás de uma garrafa dá pra respirar. Fiquem com uma garrafa entre vocês e ele. Vencido, ele destampa as garrafas, e acaba ajudando na feira, distribuindo o ar de novo.",
                ("He sets the bottles down, I get behind one. Then I come round and hit.", "Ele botou as garrafas, eu fico atrás de uma. Depois dou a volta e bato."),
                ("Keeping all the air for yourself and still feeling short of breath. That's what greed does.", "Guardar todo o ar pra si e ainda se sentir sem fôlego. É isso que a ganância faz."),
                ("Hiding behind a jar from a kobold. My heroic career, everyone.", "Me escondendo atrás de um pote fugindo de um kobold. Minha carreira de herói, gente."),
                ("Air that is held goes stale. Only the air that is shared stays good.", "Ar guardado fica parado. Só o ar dividido continua bom."),
                ("Can I keep one bottle? A little one? For breathing later?", "Posso ficar com uma garrafa? Uma pequenininha? Pra respirar depois?"));
            Line(entries, RegionBossRules.GreedyBottlerId, RegionBossPhase.Storm, "The Bottler throws bottle lanterns!", "O Engarrafador atira lanternas de garrafa!");
            Line(entries, RegionBossRules.GreedyBottlerId, RegionBossPhase.LastStand, "He uncorks every bottle at once!", "Ele destampa todas as garrafas de uma vez!");
            Blow(entries, RegionBossRules.GreedyBottlerId, "He swings his great jug!", "Lá vem o garrafão!");
            Signature(entries, RegionBossRules.GreedyBottlerId, "He's letting out the stale air! Get behind a bottle!", "Ele vai soltar o ar parado! Fica atrás de uma garrafa!");
            entries.Add(new LocalizationEntry(BottleKey, "Shelter!", "Abrigo!"));
            Withdraw(entries, RegionBossRules.GreedyBottlerId, "The Greedy Bottler pulls the corks out one by one, and the Pure Air runs back down the pipes. Wiping his snout, he goes off to help at the fair.", "O Engarrafador Guloso tira as rolhas uma a uma, e o Ar Sereno volta a correr pelos canos. Limpando o focinho, ele vai ajudar na feira.");

            // -- Palafitas de Tarchon (33) --
            Page(entries, RegionBossRules.LightFisherId,
                "The will-o'-wisps of the marsh are little sparks of light that got lost on their way up to the moon, children. The Fisher of Lights is a catfish as big as a boat who swallowed them one by one, so he could have light all to himself, and the more he swallowed the darker he got inside. He fishes with a lure of light, and he is clever: he doesn't cast where you are, he casts where you are going. If you keep walking the way you were, or stand there waiting, he hooks you. Change course right when he looks at you. Free him and he lets the sparks go, a whole cloud of them, back up towards the moon.",
                "Os fogos-fátuos do pântano são faisquinhas de luz que se perderam no caminho pra lua, crianças. O Pescador de Luzes é um bagre do tamanho de um barco que engoliu elas uma por uma, pra ter luz só pra ele, e quanto mais engolia mais escuro ficava por dentro. Ele pesca com uma isca de luz, e é esperto: não joga onde vocês estão, joga onde vocês vão estar. Se continuarem andando do mesmo jeito, ou ficarem parados esperando, ele fisga. Mudem de rumo bem na hora em que ele olhar pra vocês. Libertem ele e ele solta as faíscas, uma nuvem inteira, de volta pra lua.",
                ("He looks twice, then casts ahead. So on the second look, I turn.", "Ele olha duas vezes e joga na frente. Então no segundo olhar, eu viro."),
                ("He swallowed all that light and got darker. Light doesn't stay where it's kept.", "Ele engoliu toda aquela luz e ficou mais escuro. Luz não fica onde é guardada."),
                ("A fish that predicts my moves. Even the fish here are smarter than me.", "Um peixe que adivinha meus passos. Até os peixes daqui são mais espertos que eu."),
                ("Whoever only follows the road ahead is easy to catch. Turn when it matters.", "Quem só segue a estrada da frente é fácil de pegar. Vira quando importa."),
                ("Zig! Zag! Zig! ...He can't catch a zigzag! Nobody can catch a zigzag!", "Zig! Zag! Zig! ...Ele não pega zigue-zague! Ninguém pega zigue-zague!"));
            Line(entries, RegionBossRules.LightFisherId, RegionBossPhase.Storm, "The Fisher spits balls of marsh light!", "O Pescador cospe bolas de luz do pântano!");
            Line(entries, RegionBossRules.LightFisherId, RegionBossPhase.LastStand, "He thrashes the whole mud bank!", "Ele se debate no banco de lama inteiro!");
            Blow(entries, RegionBossRules.LightFisherId, "His tail slaps down!", "Lá vem o rabo!");
            Signature(entries, RegionBossRules.LightFisherId, "He's watching where you go! Change course!", "Ele está vendo pra onde você vai! Muda de rumo!");
            entries.Add(new LocalizationEntry(LureKey, "The lure!", "A isca!"));
            Withdraw(entries, RegionBossRules.LightFisherId, "The Fisher of Lights opens his great mouth and a cloud of will-o'-wisps pours out and floats up towards the moon. Lighter at last, he slides back into the marsh.", "O Pescador de Luzes abre a bocarra e uma nuvem de fogos-fátuos sai e sobe flutuando pra lua. Mais leve enfim, ele escorrega de volta pro pântano.");

            // -- Forte dos Ventos (39) --
            Page(entries, RegionBossRules.VaneLordId,
                "The Artificers built great wind vanes round the Fort of the Winds to tell the wind of the valley which way to blow, and the biggest of them was the Lord of the Vane: an engine of walnut wood and bronze that turned slowly and kept the breeze kind. The shadow got into his gears and now he turns a quarter every gust and blows the whole valley down. Here is the trick, children: wind only pushes one way. When he calls where the wind comes from, go to that side of him, upwind, and the gust passes you by. Free him and he turns the right way round, and the valley has a good breeze again.",
                "Os Artífices construíram cata-ventos enormes em volta do Forte dos Ventos pra dizer ao vento do vale pra onde soprar, e o maior deles era o Senhor do Cata-Vento: um engenho de nogueira e bronze que girava devagar e deixava a brisa mansa. A sombra entrou nas engrenagens dele e agora ele gira um quarto a cada rajada e sopra o vale inteiro pra baixo. O segredo é este, crianças: vento só empurra pra um lado. Quando ele disser de onde vem o vento, vão pra esse lado dele, contra o vento, e a rajada passa longe. Libertem ele e ele gira pro lado certo, e o vale volta a ter brisa boa.",
                ("He calls the wind, I go stand where it comes from.", "Ele diz o vento, eu vou ficar de onde ele vem."),
                ("He was made to keep the breeze kind. He just forgot which way kind was.", "Ele foi feito pra deixar a brisa mansa. Só esqueceu pra que lado era mansa."),
                ("Walk INTO the wind to dodge the wind. Physics in this family is optional.", "Andar CONTRA o vento pra escapar do vento. Física nessa família é opcional."),
                ("The wind pushes away from where it comes. Stand at the source and it has nothing to push.", "O vento empurra pra longe de onde vem. Fica na origem e ele não tem o que empurrar."),
                ("North! No, east! No... which one is north again?", "Norte! Não, leste! Não... qual é o norte mesmo?"));
            Line(entries, RegionBossRules.VaneLordId, RegionBossPhase.Storm, "The Vane Lord throws bronze feathers!", "O Senhor do Cata-Vento atira penas de bronze!");
            Line(entries, RegionBossRules.VaneLordId, RegionBossPhase.LastStand, "His blades spin faster and faster!", "As pás dele giram cada vez mais rápido!");
            Blow(entries, RegionBossRules.VaneLordId, "His great blade comes round!", "Lá vem a pá grande!");
            Signature(entries, RegionBossRules.VaneLordId, "He's turning the wind! Go upwind of him!", "Ele vai virar o vento! Vai pro lado de onde ele vem!");
            entries.Add(new LocalizationEntry(VaneWindKey(VaneWind.North), "Wind from the north: go north of him!", "Vento do norte: vai pro norte dele!"));
            entries.Add(new LocalizationEntry(VaneWindKey(VaneWind.East), "Wind from the east: go east of him!", "Vento do leste: vai pro leste dele!"));
            entries.Add(new LocalizationEntry(VaneWindKey(VaneWind.South), "Wind from the south: go south of him!", "Vento do sul: vai pro sul dele!"));
            entries.Add(new LocalizationEntry(VaneWindKey(VaneWind.West), "Wind from the west: go west of him!", "Vento do oeste: vai pro oeste dele!"));
            Withdraw(entries, RegionBossRules.VaneLordId, "The Lord of the Vane creaks, slows and turns the right way round. A soft breeze runs down the valley, and the wind bells of the fort ring all together.", "O Senhor do Cata-Vento range, desacelera e gira pro lado certo. Uma brisa mansa desce o vale, e os sinos de vento do forte tocam todos juntos.");

            // -- Torres Brancas de Amanirenas (42) --
            Page(entries, RegionBossRules.LittleEclipseId,
                "The mirrors of the White Spires pass the light of the sun from tower to tower so the whole hill is bright. The Little Eclipse is a disc of shadow that stuck itself to the towers and eats that light, a little more every time. It isn't big, children, but it is patient: first it covers a slice of the light towards you, then half, then more, and then the whole round goes dark at once. So watch how much it covers. Step out of the slice when it is small, and when it says the light is all gone, run beyond its shadow. Free it and it stays in the square as a shadow clock, telling the hour instead of hiding it.",
                "Os espelhos das Torres Brancas passam a luz do sol de torre em torre pra colina inteira ficar clara. O Pequeno Eclipse é um disco de sombra que grudou nas torres e come essa luz, um pouco mais a cada vez. Ele não é grande, crianças, mas é paciente: primeiro cobre uma fatia da luz na direção de vocês, depois metade, depois mais, e então a roda inteira escurece de uma vez. Então vejam quanto ele cobre. Saiam da fatia quando ela for pequena, e quando ele disser que a luz acabou, corram pra fora da sombra dele. Libertem ele e ele fica na praça como relógio de sombra, dizendo a hora em vez de escondê-la.",
                ("A quarter, a half, three quarters... and on the fourth, I run.", "Um quarto, metade, três quartos... e no quarto, eu corro."),
                ("A shadow that only takes a little at a time. Those are the hardest to notice.", "Uma sombra que só pega um pouquinho de cada vez. Essas são as mais difíceis de perceber."),
                ("It's called LITTLE Eclipse. It's covering the WHOLE sky. Somebody lied.", "Chama PEQUENO Eclipse. Está cobrindo o céu INTEIRO. Alguém mentiu."),
                ("What grows a little each time, we must count. Counting is how we see it coming.", "O que cresce um pouco a cada vez, a gente precisa contar. Contar é como a gente vê chegando."),
                ("I'll hold the sun with my hands! ...It's too hot. Never mind.", "Vou segurar o sol com as mãos! ...Está quente demais. Deixa pra lá."));
            Line(entries, RegionBossRules.LittleEclipseId, RegionBossPhase.Storm, "The Eclipse throws bolts of stolen light!", "O Eclipse atira raios de luz roubada!");
            Line(entries, RegionBossRules.LittleEclipseId, RegionBossPhase.LastStand, "It spreads over every tower!", "Ele se espalha por todas as torres!");
            Blow(entries, RegionBossRules.LittleEclipseId, "Its rim comes down on you!", "Lá vem a borda!");
            Signature(entries, RegionBossRules.LittleEclipseId, "It's covering the light! Get out of its slice!", "Ele vai cobrir a luz! Sai da fatia dele!");
            entries.Add(new LocalizationEntry(EclipseCoverKey(0), "A quarter of the light: a thin slice towards you!", "Um quarto da luz: uma fatia fina na sua direção!"));
            entries.Add(new LocalizationEntry(EclipseCoverKey(1), "Half the light: step well to the side!", "Metade da luz: vai bem pro lado!"));
            entries.Add(new LocalizationEntry(EclipseCoverKey(2), "Three quarters: only behind it is light!", "Três quartos: só atrás dele tem luz!"));
            entries.Add(new LocalizationEntry(EclipseCoverKey(3), "The whole light! Run out of its shadow!", "A luz inteira! Corre pra fora da sombra!"));
            Withdraw(entries, RegionBossRules.LittleEclipseId, "The Little Eclipse slides off the towers and settles in the middle of the square, a round shadow on the stone that tells the hour as the sun goes by.", "O Pequeno Eclipse escorrega das torres e assenta no meio da praça, uma sombra redonda na pedra que diz a hora enquanto o sol passa.");
        }

        private static void AddHighMapBossPages(List<LocalizationEntry> entries)
        {
            // -- Vale dos Sátiros (53) --
            Page(entries, RegionBossRules.MidasGoldId,
                "You know the old tale of the king of Phrygia who wished that everything he touched would turn to gold, and then could not eat, or drink, or hug his own daughter? He washed his hands in the River Pactolus and the gold went into the sand. In the Vale of the Satyrs the gold remembered: a statue of the king, all of solid gold, got up off its plinth and walks the river bank, and the shadow of Āz shines in it. Here is the trick, children: when it glows, hold your blows. Whoever strikes it hard while it shines gets a little of its gold, and gold is heavy. Strike between the glows. When Midas comes to wash his hands in the river, the statue stops and stands still for good.",
                "Vocês conhecem o conto velho do rei da Frígia que pediu que tudo o que ele tocasse virasse ouro, e aí não podia comer, nem beber, nem abraçar a própria filha? Ele lavou as mãos no rio Pactolo e o ouro foi pra areia. No Vale dos Sátiros o ouro lembrou: uma estátua do rei, toda de ouro maciço, desceu do pedestal e anda na beira do rio, e a sombra de Āz brilha nela. O segredo é este, crianças: quando ela brilhar, segurem o golpe. Quem bate forte nela enquanto brilha leva um pouco do ouro dela, e ouro pesa. Batam entre um brilho e outro. Quando o Midas vem lavar as mãos no rio, a estátua para e fica em pé de vez.",
                ("It glows, I stop. It dims, I hit. Simple.", "Brilhou, eu paro. Apagou, eu bato. Simples."),
                ("He wanted everything to be gold and lost everything that wasn't. That's the saddest wish.", "Ele quis que tudo fosse ouro e perdeu tudo que não era. É o desejo mais triste."),
                ("A statue that punishes you for hitting it. Finally, an enemy with my conflict-avoidance strategy.", "Uma estátua que castiga quem bate nela. Finalmente um inimigo com a minha estratégia de evitar briga."),
                ("Greed isn't beaten by force. Sometimes it's beaten by not reaching for it.", "Ganância não se vence com força. Às vezes se vence não estendendo a mão."),
                ("If I touch it, will I turn into gold? ...Would that be so bad? I'd be shiny.", "Se eu encostar, eu viro ouro? ...Seria tão ruim assim? Eu ia ficar brilhante."));
            Line(entries, RegionBossRules.MidasGoldId, RegionBossPhase.Storm, "The Gold of Midas throws golden sparks!", "O Ouro de Midas atira faíscas douradas!");
            Line(entries, RegionBossRules.MidasGoldId, RegionBossPhase.LastStand, "Everything around it is turning gold!", "Tudo em volta dele está virando ouro!");
            Blow(entries, RegionBossRules.MidasGoldId, "Its golden hand comes down!", "Lá vem a mão de ouro!");
            Signature(entries, RegionBossRules.MidasGoldId, "It's glowing! Hold your blows!", "Ele está brilhando! Segura o golpe!");
            Withdraw(entries, RegionBossRules.MidasGoldId, "The Gold of Midas walks to the river, kneels where the king washed his hands, and stands still at last, a statue again, the gold running off it into the sand.", "O Ouro de Midas anda até o rio, se ajoelha onde o rei lavou as mãos e fica parado enfim, estátua de novo, o ouro escorrendo dele para a areia.");

            // -- Tundra de Omöl (61) --
            Page(entries, RegionBossRules.FirstClumsyId,
                "In the tale of the far north there were two brothers who made the world: Yen made the good things, and Omöl made the clumsy ones, the twig bird, the stone hare, the clay reindeer. The very first thing Omöl ever made is this one, children: lopsided, one arm long and one short, a stone in its side where no stone should be. It was so sad about being crooked that the shadow found it easily. It leans, and leans, and then it falls over, a long way, onto whatever is on that side. Watch which way it leans and stand beside it, not in front. Free it, and it finds out that crooked things are good for something too.",
                "No conto do extremo norte tinha dois irmãos que fizeram o mundo: o Yen fez as coisas boas, e o Omöl fez as desajeitadas, a ave-graveto, a lebre-pedra, a rena de barro. A primeiríssima coisa que o Omöl fez é esta, crianças: torta, um braço comprido e um curto, uma pedra do lado onde não devia ter pedra. Ficou tão triste de ser torta que a sombra achou ela fácil. Ela se inclina, e se inclina, e aí cai, comprido, em cima do que estiver daquele lado. Vejam pra que lado ela pende e fiquem do lado dela, não na frente. Libertem ela, e ela descobre que coisa torta também serve pra alguma coisa.",
                ("It leans, I step to the side of the lean. Then I hit the clumsy thing.", "Pendeu, eu passo pro lado. Depois bato na coisa torta."),
                ("Being crooked isn't a fault. It's just a different way of standing.", "Ser torto não é defeito. É só um jeito diferente de ficar em pé."),
                ("Finally someone who trips more than I do. I feel seen.", "Finalmente alguém que tropeça mais do que eu. Me senti representado."),
                ("It falls the way it leans. Watch the lean, not the fall.", "Ela cai pra onde pende. Olha o pender, não a queda."),
                ("I like it! It looks like my drawings! Can we keep it?", "Eu gostei! Parece meus desenhos! A gente pode ficar com ela?"));
            Line(entries, RegionBossRules.FirstClumsyId, RegionBossPhase.Storm, "The First Clumsy One throws twigs everywhere!", "O Primeiro Desajeitado atira gravetos pra todo lado!");
            Line(entries, RegionBossRules.FirstClumsyId, RegionBossPhase.LastStand, "It wobbles worse than ever!", "Ele balança mais do que nunca!");
            Blow(entries, RegionBossRules.FirstClumsyId, "Its long arm swings round!", "Lá vem o braço comprido!");
            Signature(entries, RegionBossRules.FirstClumsyId, "It's leaning! Get out from in front of the lean!", "Ele está pendendo! Sai da frente de onde ele vai cair!");
            Withdraw(entries, RegionBossRules.FirstClumsyId, "The First Clumsy One sits down crookedly in the snow, and a stone hare comes and curls up in the hollow of its long arm. It laughs, crooked too.", "O Primeiro Desajeitado senta torto na neve, e uma lebre-pedra vem se enroscar na curva do braço comprido dele. Ele ri, torto também.");

            // -- Recife de Obsidiana (68) --
            Page(entries, RegionBossRules.VolcanoTurtleId,
                "Where the lava ran into the sea it turned to black glass, and that is the Obsidian Reef. The Volcano Turtle carries a little island on its back, with its own little volcano, and it swam in these warm waters so long that the island got too hot, and the shadow got into the heat. Now everywhere it walks it leaves the ground burning behind it, children, and when it rumbles, all the spots it walked over in the last few moments burst with lava. So never follow in its wake: walk beside it, not behind it. Free it and it cools down at last, and the island on its back turns green.",
                "Onde a lava correu pro mar ela virou vidro preto, e esse é o Recife de Obsidiana. A Tartaruga-Vulcão carrega uma ilhazinha nas costas, com um vulcãozinho próprio, e nadou tanto nessas águas quentes que a ilha esquentou demais, e a sombra entrou no calor. Agora por onde ela anda deixa o chão queimando atrás, crianças, e quando ela ronca, todos os lugares por onde ela passou nos últimos instantes estouram em lava. Então nunca sigam o rastro dela: andem do lado dela, não atrás. Libertem ela e ela esfria enfim, e a ilha das costas fica verde.",
                ("Never walk in its wake. Beside it, always beside it.", "Nunca anda no rastro dela. Do lado, sempre do lado."),
                ("Carrying a whole island and nobody ever asked if it was heavy.", "Carregando uma ilha inteira e ninguém nunca perguntou se pesava."),
                ("A turtle that leaves lava footprints. Slow, sure. Harmless? Not even a little.", "Uma tartaruga que deixa pegada de lava. Lenta, tudo bem. Inofensiva? Nem um pouco."),
                ("The burn is in where it has been. Where it is going is still cool.", "A queimadura está onde ela já passou. Pra onde ela vai ainda está frio."),
                ("Can I ride on the island after? When it's cold? Please?", "Posso andar na ilha depois? Quando esfriar? Por favor?"));
            Line(entries, RegionBossRules.VolcanoTurtleId, RegionBossPhase.Storm, "The Volcano Turtle spits embers!", "A Tartaruga-Vulcão cospe brasas!");
            Line(entries, RegionBossRules.VolcanoTurtleId, RegionBossPhase.LastStand, "The volcano on its back is erupting!", "O vulcão das costas está entrando em erupção!");
            Blow(entries, RegionBossRules.VolcanoTurtleId, "It slams down its shell!", "Ela bate o casco no chão!");
            Signature(entries, RegionBossRules.VolcanoTurtleId, "It's rumbling! Get off its trail!", "Ela está roncando! Sai do rastro dela!");
            Withdraw(entries, RegionBossRules.VolcanoTurtleId, "The Volcano Turtle sinks into the sea with a long hiss of steam. When it rises again its volcano is quiet, and moss is already growing on the island.", "A Tartaruga-Vulcão afunda no mar com um chiado comprido de vapor. Quando sobe de novo, o vulcão está quieto, e já tem musgo crescendo na ilha.");

            // -- Areal Escondido (73) --
            Page(entries, RegionBossRules.SledgeSerpentId,
                "In the fourth hour of the night, say the old books of Kemet, the barca of light has to cross a hidden desert, and there it becomes a sledge pulled by a great serpent across the sand. This serpent pulled it for ages, children, and then the shadow made it forget: it let go of the barca and kept the rope. Its sledge still lies on the stone ring where it left it, and when the serpent whips the rope it swings in a line from the sledge, through the serpent, and on past it. Don't stand on that line: step to the side, and the rope goes by. Free it and it remembers, and it goes back to pulling the barca across the sand.",
                "Na quarta hora da noite, dizem os livros velhos de Kemet, a barca de luz precisa atravessar um deserto escondido, e lá ela vira um trenó puxado por uma serpente enorme pela areia. Esta serpente puxou ela por eras, crianças, e aí a sombra fez ela esquecer: soltou a barca e ficou com a corda. O trenó dela ainda está no anel de pedra onde ela largou, e quando a serpente chicoteia a corda, ela passa numa linha que sai do trenó, atravessa a serpente e vai além. Não fiquem nessa linha: deem um passo pro lado, e a corda passa. Libertem ela e ela lembra, e volta a puxar a barca pela areia.",
                ("Find the sledge, find the line. Then stay off it.", "Acha o trenó, acha a linha. Aí sai dela."),
                ("She held that rope so long she forgot what it was for.", "Ela segurou essa corda tanto tempo que esqueceu pra que servia."),
                ("A snake with a jump rope. Of all the things to be good at.", "Uma cobra com corda de pular. De todas as coisas pra ser bom."),
                ("A rope only reaches where it's pointed. Stand where it isn't pointing.", "Corda só alcança pra onde aponta. Fica onde ela não aponta."),
                ("I want to pull the sledge too! I'm strong! ...With help.", "Eu também quero puxar o trenó! Eu sou forte! ...Com ajuda."));
            Line(entries, RegionBossRules.SledgeSerpentId, RegionBossPhase.Storm, "The Serpent spits sand!", "A Serpente cospe areia!");
            Line(entries, RegionBossRules.SledgeSerpentId, RegionBossPhase.LastStand, "She coils up and lashes the whole sand!", "Ela se enrola e chicoteia o areal inteiro!");
            Blow(entries, RegionBossRules.SledgeSerpentId, "She strikes!", "Ela dá o bote!");
            Signature(entries, RegionBossRules.SledgeSerpentId, "She's whipping the rope! Off the line from the sledge!", "Ela vai chicotear a corda! Sai da linha do trenó!");
            Withdraw(entries, RegionBossRules.SledgeSerpentId, "The Great Serpent of the Sands takes the rope in her jaws, finds the sledge, and starts to pull, slowly, the way she always did, across the hidden sands.", "A Grande Serpente do Areal pega a corda nos dentes, acha o trenó e começa a puxar, devagar, do jeito que sempre puxou, pelo areal escondido.");

            // -- Poços de Betume (79) --
            Page(entries, RegionBossRules.PitchKingId,
                "The old people between the rivers sealed their boats and their bricks with bitumen, the black pitch that comes up out of the ground; the boat of the great flood was sealed with it. The King of Pitch is a great bubble of it that went dark in the wells. He doesn't hit hard, children, he sticks. When he shudders, his tar spreads around him, and whoever lingers in it too long gets stuck when it hardens. So don't stand next to him through the warning: get in, get out. Free him and he becomes the good pitch again, the one that keeps the rafts afloat.",
                "O povo antigo entre os rios calafetava os barcos e os tijolos com betume, o piche preto que sobe da terra; o barco do grande dilúvio foi calafetado com ele. O Rei do Piche é uma bolha enorme dele que apagou nos poços. Ele não bate forte, crianças, ele gruda. Quando ele treme, o piche se espalha em volta, e quem fica tempo demais nele gruda quando endurece. Então não fiquem do lado dele o aviso inteiro: entra, bate, sai. Libertem ele e ele vira o piche bom de novo, o que segura as balsas na água.",
                ("In, two hits, out. Never stay in the tar.", "Entra, dois golpes, sai. Nunca fica no piche."),
                ("Something meant to hold boats together, now just holding on. Poor thing.", "Uma coisa feita pra segurar barco, agora só segurando. Coitado."),
                ("I just washed these boots. In a river. Of light. And now this.", "Eu acabei de lavar essas botas. Num rio. De luz. E agora isso."),
                ("What sticks only catches what stays. Keep moving and it has nothing to hold.", "O que gruda só pega o que fica. Continua andando e ele não tem o que segurar."),
                ("Ew. Ew! It's on my shoe! Why is it always my shoe?", "Eca. Eca! Está no meu sapato! Por que é sempre o meu sapato?"));
            Line(entries, RegionBossRules.PitchKingId, RegionBossPhase.Storm, "The King of Pitch spits blobs of tar!", "O Rei do Piche cospe bolas de piche!");
            Line(entries, RegionBossRules.PitchKingId, RegionBossPhase.LastStand, "The whole well is bubbling!", "O poço inteiro está borbulhando!");
            Blow(entries, RegionBossRules.PitchKingId, "He slaps down a sticky arm!", "Lá vem o braço grudento!");
            Signature(entries, RegionBossRules.PitchKingId, "His tar is spreading! Don't linger next to him!", "O piche dele está se espalhando! Não fica parado do lado dele!");
            Withdraw(entries, RegionBossRules.PitchKingId, "The King of Pitch settles into the well, warm and amber, and a raft drifting by is glad of a fresh coat of good pitch.", "O Rei do Piche se acomoda no poço, morno e cor de âmbar, e uma balsa que passa fica feliz com uma camada nova de piche bom.");

            // -- Convés da Barca da Lua (93) --
            Page(entries, RegionBossRules.GreatStowawayId,
                "The Moon Ship gathers the light that comes up from the world and carries it on to the Sun. Down in its hold, among the jars of light, a big shadow hid itself, children, wrapped in a stolen sail-cloth, hoping to sneak up into the sky. It is more scared than wicked. It hides under the four hatches of the hold, and when it hears you it bursts out of the hatch nearest to you. So keep away from the hatches: the middle of the hold, between them, is safe, and so is far away. Beaten, it climbs back down to the world where it belongs, and gives back the lantern it stole.",
                "A Barca da Lua junta a luz que sobe do mundo e leva até o Sol. Lá embaixo no porão, entre as jarras de luz, uma sombra grande se escondeu, crianças, enrolada numa lona roubada, querendo subir escondida pro céu. Ela tem mais medo do que maldade. Ela se esconde debaixo dos quatro alçapões do porão, e quando escuta vocês sai pelo alçapão mais perto. Então fiquem longe dos alçapões: o meio do porão, entre eles, é seguro, e longe também. Vencida, ela desce de volta pro mundo, que é o lugar dela, e devolve a lanterna que roubou.",
                ("Middle of the hold, away from every hatch. Let it come to me.", "No meio do porão, longe de todo alçapão. Deixa ela vir até mim."),
                ("It only wanted to go up to the sky. So did we, once.", "Ela só queria subir pro céu. A gente também quis, um dia."),
                ("A stowaway hiding from us. On a flying moon boat. Okay, sure, why not.", "Uma clandestina se escondendo da gente. Num barco-lua voador. Tá, claro, por que não."),
                ("The light can't be stolen up into the sky. It has to be carried.", "A luz não se rouba pro céu. Ela tem que ser levada."),
                ("Hide and seek! I'm good at hide and seek! ...It's better.", "Esconde-esconde! Eu sou boa em esconde-esconde! ...Ela é melhor."));
            Line(entries, RegionBossRules.GreatStowawayId, RegionBossPhase.Storm, "The Stowaway throws stolen light!", "O Clandestino atira luz roubada!");
            Line(entries, RegionBossRules.GreatStowawayId, RegionBossPhase.LastStand, "It's darting between all the hatches!", "Ele está pulando entre todos os alçapões!");
            Blow(entries, RegionBossRules.GreatStowawayId, "It lunges out of the dark!", "Ele salta do escuro!");
            Signature(entries, RegionBossRules.GreatStowawayId, "It's in the hatches! Keep away from them!", "Ele está nos alçapões! Fica longe deles!");
            entries.Add(new LocalizationEntry(HatchKey, "Hatch!", "Alçapão!"));
            Withdraw(entries, RegionBossRules.GreatStowawayId, "The Great Stowaway leaves the stolen lantern on the deck, folds its sail-cloth, and climbs down the side of the Moon Ship, back to the world below.", "O Clandestino Grande deixa a lanterna roubada no convés, dobra a lona e desce pelo costado da Barca da Lua, de volta para o mundo lá embaixo.");
        }

        private static void AddLastFirstMapBossPages(List<LocalizationEntry> entries)
        {
            // -- Brejo do Lodo Escuro (03) --
            Page(entries, RegionBossRules.MireMotherId,
                "The Lodo is the dark mud of the old tales, the opposite of Living Water. When it crept into the marsh, the great mother toad who looked after the spring swallowed the spring whole, to keep it safe from the mud, and the dark got into her with it. Now she sits by the Poço Turvo with the spring glowing in her throat and snaps at anything that comes near. She is slow, children, but her tongue is quick, and it catches whoever walks into her mouth: if you step closer when she opens it, or stand at her lips, it gets you. Hold your ground or step back, and strike when it shuts. Free her and she gives the spring back, and goes back to looking after the marsh.",
                "O Lodo é a lama escura dos contos velhos, o contrário da Água Viva. Quando ele entrou no brejo, a sapa-mãe enorme que cuidava da fonte engoliu a fonte inteira, pra guardar ela da lama, e o escuro entrou nela junto. Agora ela fica sentada no Poço Turvo com a fonte brilhando na garganta e abocanha o que chega perto. Ela é lenta, crianças, mas a língua é rápida, e pega quem anda pra dentro da boca dela: se vocês chegarem mais perto quando ela abrir, ou ficarem colados nos lábios dela, ela pega. Fiquem onde estão ou recuem, e batam quando ela fechar. Libertem ela e ela devolve a fonte, e volta a cuidar do brejo.",
                ("Mouth open, I hold still or step back. Mouth shut, I go in.", "Boca aberta, eu paro ou recuo. Boca fechada, eu entro."),
                ("She swallowed the spring to protect it. She just held on too long.", "Ela engoliu a fonte pra proteger. Só segurou tempo demais."),
                ("A toad the size of a house. With a tongue. And I'm the one standing closest. Of course.", "Uma sapa do tamanho de uma casa. Com língua. E eu sou quem está mais perto. Claro."),
                ("She can only catch what comes to her. Let her come to us.", "Ela só pega o que vai até ela. Deixa ela vir até a gente."),
                ("Ribbit! ...She didn't answer. Maybe she speaks a different frog.", "Croac! ...Ela não respondeu. Vai ver ela fala outro sapo."));
            Line(entries, RegionBossRules.MireMotherId, RegionBossPhase.Storm, "Mother Mire spits balls of mud!", "A Mãe-Lodo cospe bolas de lama!");
            Line(entries, RegionBossRules.MireMotherId, RegionBossPhase.LastStand, "The whole marsh is bubbling around her!", "O brejo inteiro borbulha em volta dela!");
            Blow(entries, RegionBossRules.MireMotherId, "She jumps and comes down hard!", "Ela pula e cai com tudo!");
            Signature(entries, RegionBossRules.MireMotherId, "She's opening her mouth! Don't step closer!", "Ela está abrindo a boca! Não chega mais perto!");
            Withdraw(entries, RegionBossRules.MireMotherId, "Mother Mire opens her mouth wide, and the spring pours out of it, clear, into the Poço Turvo. She croaks once, softly, and settles down beside it to keep watch.", "A Mãe-Lodo abre bem a boca, e a fonte sai dela, limpa, para o Poço Turvo. Ela coaxa uma vez, baixinho, e se acomoda do lado para vigiar.");

            // -- Ermos do Tambor Rubro (11) --
            Page(entries, RegionBossRules.CrownedChiefId,
                "The orcs of the Red Drum are a good people, children, drummers and hunters; but some of them were touched by Āz, the greed, and wear little tin crowns she gave them. This one gathered more gold for her than any other, and carries it on his back in a chest, and the crown on his head is the biggest. When he roars he throws tin crowns all round where you stand, three of them. Don't run off onto one: stay right where you are, in the middle, and they land around you. Beaten, he takes off his crown, gives the chest back to the town, and asks to drum again with his clan.",
                "Os orcs do Tambor Rubro são um povo bom, crianças, tocadores de tambor e caçadores; mas alguns foram tocados por Āz, a ganância, e usam coroinhas de lata que ela deu. Este juntou mais ouro pra ela do que qualquer outro, e carrega nas costas numa arca, e a coroa da cabeça dele é a maior. Quando ele ruge, atira coroas de lata em volta de onde vocês estão, três delas. Não saiam correndo pra cima de uma: fiquem bem onde estão, no meio, e elas caem em volta. Vencido, ele tira a coroa, devolve a arca pra cidade e pede pra tocar tambor de novo com o clã dele.",
                ("Crowns in the air, I plant my feet. Middle of the three.", "Coroa no ar, eu finco os pés. No meio das três."),
                ("A crown that makes you carry everyone's gold isn't a crown. It's a weight.", "Uma coroa que faz você carregar o ouro de todo mundo não é coroa. É peso."),
                ("Dodging by not moving. My favourite kind of dodging.", "Desviar ficando parado. Meu tipo preferido de desvio."),
                ("What greed throws at you falls around you if you don't go to meet it.", "O que a ganância atira em você cai em volta se você não for ao encontro."),
                ("Can I wear one of the crowns? Just to see? I'll give it back!", "Posso usar uma das coroas? Só pra ver? Eu devolvo!"));
            Line(entries, RegionBossRules.CrownedChiefId, RegionBossPhase.Storm, "The Crowned Chief throws burning coins!", "O Chefe Coroado atira moedas em brasa!");
            Line(entries, RegionBossRules.CrownedChiefId, RegionBossPhase.LastStand, "He beats his chest like a war drum!", "Ele bate no peito feito tambor de guerra!");
            Blow(entries, RegionBossRules.CrownedChiefId, "His big fist comes down!", "Lá vem o punho grande!");
            Signature(entries, RegionBossRules.CrownedChiefId, "He's throwing the crowns! Stand still in the middle!", "Ele vai atirar as coroas! Fica parado no meio!");
            entries.Add(new LocalizationEntry(CrownKey, "Crown!", "Coroa!"));
            Withdraw(entries, RegionBossRules.CrownedChiefId, "The Crowned Chief takes off his tin crown, sets the chest of gold down in the dust, and walks off towards the town, already drumming on his own belly.", "O Chefe Coroado tira a coroa de lata, põe a arca de ouro na poeira e vai embora para a cidade, já batucando na própria barriga.");

            // -- Salões de Bronze de Napirasu (17) --
            Page(entries, RegionBossRules.DarkBellowsId,
                "Deep in the Bronze Halls the dwarves had a Great Forge, and it was blown by a great bellows of walnut wood, leather and bronze, so big that four dwarves pumped it. When the forge went out, the shadow got into the bellows, and now it walks about on its iron legs and breathes on its own: out through the nozzle in front, hot as the forge, and in through the back, pulling everything to it. It does one, then the other, children, and it says which. Out in front: get behind it. Drawing in: stay in front. Beside it is always safe. Free it and it lights the first fire of the Great Forge again.",
                "Lá no fundo dos Salões de Bronze os anões tinham uma Grande Forja, e ela era soprada por um fole enorme de nogueira, couro e bronze, tão grande que quatro anões bombeavam. Quando a forja apagou, a sombra entrou no fole, e agora ele anda nas pernas de ferro e respira sozinho: sopra pelo bico na frente, quente como a forja, e puxa pelas costas, arrastando tudo pra ele. Faz um, depois o outro, crianças, e diz qual. Soprando na frente: vão pra trás dele. Puxando: fiquem na frente. Do lado é sempre seguro. Libertem ele e ele acende de novo o primeiro fogo da Grande Forja.",
                ("Out, I go behind. In, I go in front. Or just stay beside.", "Soprou, eu vou pra trás. Puxou, eu vou pra frente. Ou fico do lado."),
                ("It just wants to breathe for the forge again. It doesn't know the forge is out.", "Ele só quer respirar pra forja de novo. Não sabe que a forja apagou."),
                ("An angry accordion. I am fighting an angry accordion.", "Uma sanfona brava. Eu estou lutando com uma sanfona brava."),
                ("Every breath has two halves. Learn both and you'll never be caught.", "Toda respiração tem duas metades. Aprende as duas e nunca te pegam."),
                ("When it breathes out it's warm! It's like a big hairdryer!", "Quando ele sopra é quentinho! Parece um secador gigante!"));
            Line(entries, RegionBossRules.DarkBellowsId, RegionBossPhase.Storm, "The Bellows blows out sparks!", "O Fole sopra faíscas!");
            Line(entries, RegionBossRules.DarkBellowsId, RegionBossPhase.LastStand, "It pumps faster and faster!", "Ele bombeia cada vez mais rápido!");
            Blow(entries, RegionBossRules.DarkBellowsId, "It slams its boards together!", "Ele bate as tábuas!");
            Signature(entries, RegionBossRules.DarkBellowsId, "It's taking a breath!", "Ele vai respirar!");
            entries.Add(new LocalizationEntry(BellowsBreathKey(true), "It's blowing out in front: get behind it!", "Ele vai soprar na frente: vai pra trás dele!"));
            entries.Add(new LocalizationEntry(BellowsBreathKey(false), "It's drawing in behind: stay in front!", "Ele vai puxar por trás: fica na frente!"));
            Withdraw(entries, RegionBossRules.DarkBellowsId, "The Dark Bellows lets out one long, warm breath. Somewhere below, in the Great Forge, a first small flame catches and holds.", "O Fole Apagado solta um sopro comprido e morno. Lá embaixo, na Grande Forja, uma primeira chaminha pega e fica.");

            // -- Campo das Pedras-Dragão (21) --
            Page(entries, RegionBossRules.SleepingVishapId,
                "In the old mountains of the north-west, people raised tall stones with fish heads carved on them and called them dragon-stones, vishapakar, because the vishaps, the water dragons, lived under them by the springs. Under the biggest stone of this field a vishap slept for so long that the shadow crept into its dreams. It is still half asleep, children, and when it dreams it opens the springs, and the water rises everywhere around the great stone, more each time: first only far away, then closer, then almost up to the stone. So go towards the great stone when it dreams. Free it and it wakes up properly, and the springs run clean again.",
                "Nas montanhas antigas do noroeste, o povo erguia pedras altas com cabeças de peixe entalhadas e chamava de pedras-dragão, vishapakar, porque os vishaps, os dragões d'água, moravam debaixo delas junto das fontes. Debaixo da maior pedra deste campo um vishap dormiu tanto tempo que a sombra entrou nos sonhos dele. Ele ainda está meio dormindo, crianças, e quando sonha abre as fontes, e a água sobe em volta da pedra grande, mais a cada vez: primeiro só longe, depois mais perto, depois quase até a pedra. Então vão pra perto da pedra grande quando ele sonhar. Libertem ele e ele acorda de verdade, e as fontes correm limpas de novo.",
                ("It dreams, I run to the great stone. Closer each time.", "Ele sonhou, eu corro pra pedra grande. Mais perto a cada vez."),
                ("Imagine having a bad dream for a hundred years. Let's wake him gently.", "Imagina ter um pesadelo por cem anos. Vamos acordar ele com jeito."),
                ("The dragon is asleep and still winning. That's honestly impressive.", "O dragão está dormindo e ainda está ganhando. Sinceramente, impressionante."),
                ("The water rises from the edges. The stone stays dry the longest.", "A água sobe pelas beiradas. A pedra é a que fica seca por mais tempo."),
                ("Shhh! Everybody whisper! ...Why are we hitting him if he's sleeping?", "Shhh! Todo mundo cochichando! ...Por que a gente bate nele se ele está dormindo?"));
            Line(entries, RegionBossRules.SleepingVishapId, RegionBossPhase.Storm, "The Vishap spits spring water in its sleep!", "O Vishap cospe água da fonte dormindo!");
            Line(entries, RegionBossRules.SleepingVishapId, RegionBossPhase.LastStand, "It thrashes in a nightmare!", "Ele se debate num pesadelo!");
            Blow(entries, RegionBossRules.SleepingVishapId, "It rolls over in its sleep!", "Ele rola no sono!");
            Signature(entries, RegionBossRules.SleepingVishapId, "It's dreaming! The springs are rising, go to the great stone!", "Ele está sonhando! As fontes vão subir, vai pra pedra grande!");
            entries.Add(new LocalizationEntry(SpringsRiseKey(0), "The water rises far away: stay within ten steps of the stone!", "A água sobe longe: fica a dez passos da pedra!"));
            entries.Add(new LocalizationEntry(SpringsRiseKey(1), "The water rises closer: within seven steps!", "A água sobe mais perto: a sete passos!"));
            entries.Add(new LocalizationEntry(SpringsRiseKey(2), "The water is almost at the stone: get right up to it!", "A água está quase na pedra: cola nela!"));
            Withdraw(entries, RegionBossRules.SleepingVishapId, "The Sleeping Vishap opens its eyes at last, yawns, and slides back under the great stone. All over the field, the springs bubble up clear.", "O Vishap Adormecido abre enfim os olhos, boceja e escorrega de volta para debaixo da pedra grande. Pelo campo inteiro, as fontes brotam limpas.");

            // -- Pirâmides Altas de Meroë (27) --
            Page(entries, RegionBossRules.ReliefLionId,
                "At Meroë, among the steep pyramids of the old kings of Kush, there is a courtyard whose walls are covered in carvings, and the guardian of the courtyard was a lion carved in the stone, with its paint still bright. The shadow got into the wall and the lion came off it, children. It still guards the courtyard, but it no longer knows who is a friend. When it crouches it pounces on where you stand, and then it leaps straight back to its place on the wall: both spots get hit. So don't stand where you were, and don't stand by its wall. Free it and it goes back into the relief, and keeps watch from there, gently.",
                "Em Meroé, entre as pirâmides altas dos reis antigos de Kush, tem um pátio com as paredes cobertas de entalhes, e o guardião do pátio era um leão entalhado na pedra, com a tinta ainda viva. A sombra entrou na parede e o leão saiu dela, crianças. Ele ainda guarda o pátio, mas não sabe mais quem é amigo. Quando ele se agacha, pula onde vocês estão, e depois salta direto de volta pro lugar dele na parede: os dois lugares levam o golpe. Então não fiquem onde estavam, e não fiquem perto da parede dele. Libertem ele e ele volta pro relevo, e vigia de lá, manso.",
                ("It crouches, I move. Not where I was, not by the wall.", "Agachou, eu saio. Nem onde eu estava, nem perto da parede."),
                ("It's still guarding. It just forgot what it was guarding against.", "Ele ainda está guardando. Só esqueceu contra o quê."),
                ("Two-dimensional lion. Three-dimensional teeth. Not fair.", "Leão de duas dimensões. Dentes de três dimensões. Não é justo."),
                ("Whoever always returns to the same place can be read. Watch where it comes from.", "Quem sempre volta pro mesmo lugar dá pra ler. Olha de onde ele vem."),
                ("It's painted! Look, it has stripes! Red and gold! ...Okay, running now.", "Ele é pintado! Olha, tem listras! Vermelho e dourado! ...Tá, correndo agora."));
            Line(entries, RegionBossRules.ReliefLionId, RegionBossPhase.Storm, "The Lion of the Reliefs throws bolts of painted light!", "O Leão dos Relevos atira raios de luz pintada!");
            Line(entries, RegionBossRules.ReliefLionId, RegionBossPhase.LastStand, "It roars and the whole courtyard shakes!", "Ele ruge e o pátio inteiro treme!");
            Blow(entries, RegionBossRules.ReliefLionId, "Its stone paw comes down!", "Lá vem a pata de pedra!");
            Signature(entries, RegionBossRules.ReliefLionId, "It's crouching! Get away from your spot and from its wall!", "Ele está se agachando! Sai do seu lugar e de perto da parede dele!");
            Withdraw(entries, RegionBossRules.ReliefLionId, "The Lion of the Reliefs walks back to its wall, turns round three times and steps into the stone, flat again, its paint shining a little brighter than before.", "O Leão dos Relevos volta para a parede, dá três voltas e entra na pedra, chato de novo, a tinta brilhando um pouco mais do que antes.");

            // -- Acampamento de Soslan (47) --
            Page(entries, RegionBossRules.SmokeStallionId,
                "The Nart heroes of the old sagas rode horses that were almost people, and the centaurs of Soslan's camp still race across the steppe. Lately the foals have been going missing: a stallion made of smoke comes and gallops round them and carries them off. Under the smoke there is a real horse, children, one the shadow took first. When it rears it gallops a ring of smoke round where you are standing. The ring is what hurts: inside it, with the foals, you are safe, and far outside it too. Just don't be standing on the ring. Free it and the smoke blows away, and it is a real horse again, and leads the foals home.",
                "Os heróis nartes das sagas antigas montavam cavalos que eram quase gente, e os centauros do acampamento de Soslan ainda correm pela estepe. Ultimamente os potros andam sumindo: um garanhão feito de fumaça vem, galopa em volta deles e leva embora. Debaixo da fumaça tem um cavalo de verdade, crianças, um que a sombra pegou primeiro. Quando ele empina, galopa um anel de fumaça em volta de onde vocês estão. O anel é que machuca: dentro dele, junto dos potros, é seguro, e bem fora também. Só não fiquem em cima do anel. Libertem ele e a fumaça vai embora, e ele vira cavalo de verdade de novo, e leva os potros pra casa.",
                ("It rears, I stay in the middle of the ring. With the foals.", "Empinou, eu fico no meio do anel. Junto dos potros."),
                ("There's a real horse in there, scared. That's who we're freeing.", "Tem um cavalo de verdade ali dentro, com medo. É ele que a gente vai libertar."),
                ("Standing still while a smoke horse runs circles around me. Very calm. Totally calm.", "Ficar parado enquanto um cavalo de fumaça corre em volta de mim. Muito calmo. Totalmente calmo."),
                ("A circle closes on the edge, never in the middle.", "Um círculo fecha na borda, nunca no meio."),
                ("Can I ride it after? It'll be a real horse! A real one!", "Posso montar nele depois? Vai ser um cavalo de verdade! De verdade!"));
            Line(entries, RegionBossRules.SmokeStallionId, RegionBossPhase.Storm, "The Smoke Stallion throws plumes of smoke!", "O Cavalo de Fumaça atira plumas de fumaça!");
            Line(entries, RegionBossRules.SmokeStallionId, RegionBossPhase.LastStand, "It gallops round the whole camp!", "Ele galopa em volta do acampamento inteiro!");
            Blow(entries, RegionBossRules.SmokeStallionId, "Its hooves come down!", "Lá vêm os cascos!");
            Signature(entries, RegionBossRules.SmokeStallionId, "It's rearing! Stay inside the ring, don't stand on it!", "Ele está empinando! Fica dentro do anel, não em cima dele!");
            Withdraw(entries, RegionBossRules.SmokeStallionId, "The smoke blows off the stallion in long grey ribbons, and underneath is a bay horse, trembling. It whinnies, and three foals come trotting out of the grass to follow it home.", "A fumaça se solta do garanhão em fitas cinzentas compridas, e embaixo tem um cavalo baio, tremendo. Ele relincha, e três potros saem trotando do capim para seguir ele até em casa.");
        }
    }
}
