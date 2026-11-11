using System.Collections.Generic;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Enemies;

namespace Rapadura.UI.Bestiary
{
    /// <summary>
    /// "Caderno da Vó Edna" pages of the map bosses of the fichas (Docs/Mapas100.md 55, 56, 60, 62, 66, 69; <see cref="RegionBossRules.MapSpawns"/>)
    /// and the lines they call out, en + pt-BR. Each is a creature or an engine gone dark: beaten, it is freed, and its last line says what it goes back to.
    /// </summary>
    public static partial class RegionBossTexts
    {
        /// <summary>What the Shisa King calls out as it opens one mouth or the other (<see cref="RegionBossRules.ShisaMouthFor"/>).</summary>
        public static string ShisaMouthKey(ShisaMouth mouth) => $"region_boss.shisa_king.mouth.{mouth.ToString().ToLowerInvariant()}";

        private static void AddMapBossPages(List<LocalizationEntry> entries)
        {
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
    }
}
