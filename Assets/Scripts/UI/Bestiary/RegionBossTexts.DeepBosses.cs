using System.Collections.Generic;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Enemies;

namespace Rapadura.UI.Bestiary
{
    /// <summary>
    /// "Caderno da Vó Edna" pages of the map bosses of the other pits of the Limiar Profundo and of the bridge (Docs/Mapas100.md 84, 85, 87, 88, 89) and the
    /// lines they call out, en + pt-BR. Like the others, each is a creature gone dark that the cousins free; the Guardião do Novelo is not even dark: the
    /// cousins cut the threads of the Novelo off him, at his side.
    /// </summary>
    public static partial class RegionBossTexts
    {
        /// <summary>The label over the smoke copy O Rei da Fumaça raises on the hero's spot (<see cref="RegionBossRules.SmokeCopySpot"/>).</summary>
        public const string SmokeCopyKey = "region_boss.smoke_king.copy";

        /// <summary>Which way the stripes A Leoa lights run (<see cref="RegionBossRules.LionessStripesRunNorth"/>), called out as she roars.</summary>
        public static string LionessStripesKey(bool runNorth) => runNorth ? "region_boss.devouring_lioness.stripes.north" : "region_boss.devouring_lioness.stripes.east";

        /// <summary>The label over the dark stripes either side of the hero's (<see cref="RegionBossRules.LionessStripeBurns"/>).</summary>
        public const string LionessDarkKey = "region_boss.devouring_lioness.dark";

        /// <summary>The label over the one lamp O Dragão do Fundo leaves lit (<see cref="RegionBossRules.LampSpot"/>).</summary>
        public const string LampKey = "region_boss.deep_dragon.lamp";

        /// <summary>The Guardião do Novelo's call over his shelter (<see cref="RegionBossRules.InGuardianShelter"/>).</summary>
        public const string ShelterKey = "region_boss.novelo_guardian.shelter";

        private static void AddDeepMapBossPages(List<LocalizationEntry> entries)
        {
            // -- Cova da Fumaça (84) --
            Page(entries, RegionBossRules.SmokeKingId,
                "The first world of the Darkness was the Smoke, children, the world of the ones who walk on two legs, and the Pit of Smoke still remembers it. Its king is made of that smoke, and he has no face of his own: he takes the shape of whoever comes in. When he draws in his breath, a copy of you rises out of the smoke right where you are standing, and the smoke chokes everyone who is nearer the copy than the king. So don't run from him: go to him. Step in past the halfway line, or round behind him, and the copy loses you. Beaten with wind, the old people say, he comes apart and is just air again, and the corridors clear.",
                "O primeiro mundo da Treva era a Fumaça, crianças, o mundo dos que andam em duas pernas, e a Cova da Fumaça ainda lembra disso. O rei dela é feito dessa fumaça, e não tem cara própria: ele pega a forma de quem chega. Quando ele puxa o fôlego, uma cópia de vocês sobe da fumaça bem onde vocês estão, e a fumaça sufoca todo mundo que está mais perto da cópia do que do rei. Então não fujam dele: vão até ele. Passem da metade do caminho, ou deem a volta por trás dele, e a cópia perde vocês. Vencido com vento, contam os antigos, ele se desmancha e vira só ar de novo, e os corredores clareiam.",
                ("My copy comes up where I stand. So I walk into him, past the middle, and hit.", "Minha cópia sobe onde eu estou. Então eu ando pra cima dele, passo do meio, e bato."),
                ("He has no face, so he borrows ours. Imagine never having your own.", "Ele não tem cara, então pega a nossa emprestada. Imagina nunca ter a sua."),
                ("Great, a copy of me. The pit now has two people who can't dance.", "Ótimo, uma cópia minha. Agora a cova tem duas pessoas que não sabem dançar."),
                ("Nearer the copy than him is choked. The line is exactly halfway between.", "Mais perto da cópia que dele, sufoca. A linha é exatamente no meio."),
                ("There are two of me! The smoky one is uglier. Right? Right?", "Tem duas de mim! A de fumaça é mais feia. Né? Né?"));
            Line(entries, RegionBossRules.SmokeKingId, RegionBossPhase.Storm, "The Smoke King throws arrows of soot!", "O Rei da Fumaça atira flechas de fuligem!");
            Line(entries, RegionBossRules.SmokeKingId, RegionBossPhase.LastStand, "The whole pit fills with smoke!", "A cova inteira se enche de fumaça!");
            Blow(entries, RegionBossRules.SmokeKingId, "His fist of smoke comes down!", "Lá vem o punho de fumaça!");
            Signature(entries, RegionBossRules.SmokeKingId, "He's copying you! Get nearer him than your copy!", "Ele está te copiando! Chega mais perto dele que da tua cópia!");
            entries.Add(new LocalizationEntry(SmokeCopyKey, "Your copy!", "Tua cópia!"));
            Withdraw(entries, RegionBossRules.SmokeKingId, "The Smoke King loses the shape he borrowed, then every other shape, and drifts up the old vents of the Artificers as plain grey air.", "O Rei da Fumaça perde a forma que tinha pegado, depois todas as outras, e sobe pelos ventiladores velhos dos Artífices como ar cinza e comum.");

            // -- Cova do Fogo Devorador (85) --
            Page(entries, RegionBossRules.DevouringLionessId,
                "The second world of the Darkness was the Devouring Fire, children, the world of the ones on four legs. It isn't the good fire of the hearth: it is the fire that eats, and the Lioness of the pit eats even the good fire, the lamps, the coals, the warmth. When she roars, the floor of her hall lights up in long stripes, one stripe burning and the next one dark, all the way across, starting with the stripe you are standing on. One time the stripes run north to south, the next time east to west. Take one step across, onto the dark stone. Beaten, she is a gentle ember, the kind you can sit beside on a cold night.",
                "O segundo mundo da Treva era o Fogo Devorador, crianças, o mundo dos de quatro patas. Não é o fogo bom da lareira: é o fogo que come, e a Leoa da cova come até o fogo bom, as lâmpadas, as brasas, o calor. Quando ela ruge, o chão do salão dela acende em faixas compridas, uma faixa queimando e a outra escura, de ponta a ponta, começando pela faixa onde vocês estão. Uma vez as faixas correm de norte a sul, na outra de leste a oeste. Deem um passo atravessado, pra pedra escura. Vencida, ela vira brasa mansa, daquelas que dá pra sentar do lado numa noite fria.",
                ("My stripe burns. One step across, never along it.", "A minha faixa queima. Um passo atravessado, nunca ao longo dela."),
                ("She eats the good fire too. I think she's just very, very cold inside.", "Ela come o fogo bom também. Acho que ela só está com muito, muito frio por dentro."),
                ("A lion that eats lamps. Somebody tell her the menu has soup.", "Uma leoa que come lâmpadas. Alguém avisa ela que no cardápio tem sopa."),
                ("Every other stripe, counted from mine. Odd ones dark. Easy.", "Uma faixa sim, uma não, contando da minha. As ímpares escuras. Fácil."),
                ("Zebra floor! Zebra floor! I'm on the black bit! Am I on the black bit?", "Chão de zebra! Chão de zebra! Tô na parte preta! Tô na parte preta?"));
            Line(entries, RegionBossRules.DevouringLionessId, RegionBossPhase.Storm, "The Lioness spits burning coals!", "A Leoa cospe brasas!");
            Line(entries, RegionBossRules.DevouringLionessId, RegionBossPhase.LastStand, "The whole hall is glowing!", "O salão inteiro está em brasa!");
            Blow(entries, RegionBossRules.DevouringLionessId, "Her claws of fire come down!", "Lá vêm as garras de fogo!");
            Signature(entries, RegionBossRules.DevouringLionessId, "She's roaring! Step across onto a dark stripe!", "Ela está rugindo! Passa pra uma faixa escura!");
            entries.Add(new LocalizationEntry(LionessStripesKey(true), "The stripes run north to south! Step east or west!", "As faixas correm de norte a sul! Passo pro leste ou pro oeste!"));
            entries.Add(new LocalizationEntry(LionessStripesKey(false), "The stripes run east to west! Step north or south!", "As faixas correm de leste a oeste! Passo pro norte ou pro sul!"));
            entries.Add(new LocalizationEntry(LionessDarkKey, "Dark stone!", "Pedra escura!"));
            Withdraw(entries, RegionBossRules.DevouringLionessId, "The fire goes out of the Lioness slowly, like a forge at night, until she is a small warm ember lying in the ash, purring.", "O fogo sai da Leoa devagar, como uma forja à noite, até ela ser uma brasinha morna deitada na cinza, ronronando.");

            // -- Cova do Lodo (87) --
            Page(entries, RegionBossRules.BlackTailFishId,
                "The fourth world of the Darkness was the Mire, the troubled water, children, the world of the ones who swim. Of the five shapes the Prince of Darkness wore, one had the tail of a fish, and this fish of the black water kept it. It turns its back on you and sweeps that black tail over the whole side of it where you were standing. Back off out of its reach, or slip round to its other side, by its head, and the tail sweeps empty water. Beaten, the tail goes pale, and it is a little lantern fish that lights the flooded halls for the people with the lamps.",
                "O quarto mundo da Treva era o Lodo, a água turva, crianças, o mundo dos que nadam. Das cinco formas que o Príncipe da Treva usava, uma tinha cauda de peixe, e este peixe da água preta ficou com ela. Ele vira as costas pra vocês e varre aquela cauda preta por todo o lado dele onde vocês estavam. Recuem pra fora do alcance, ou passem pro outro lado dele, pela cabeça, e a cauda varre água vazia. Vencido, a cauda clareia, e ele é um peixinho-lanterna que ilumina os salões alagados pro povo das lâmpadas.",
                ("He sweeps my side. So I go to his other side, by the head.", "Ele varre o meu lado. Então eu vou pro outro lado, pela cabeça."),
                ("He lives in water nobody can see through. No wonder he's frightened.", "Ele vive numa água que ninguém enxerga. Não é à toa que ele tem medo."),
                ("Fighting a fish in the dark in the mud. My mother thinks I'm studying.", "Brigando com um peixe no escuro, na lama. Minha mãe acha que eu estou estudando."),
                ("Half the circle round him, the half facing where you were. The other half is dry.", "Meio círculo em volta dele, a metade virada pra onde você estava. A outra metade está seca."),
                ("When he's a lantern fish, can he live in my bath? I'll change the water!", "Quando ele virar peixe-lanterna, ele pode morar na minha banheira? Eu troco a água!"));
            Line(entries, RegionBossRules.BlackTailFishId, RegionBossPhase.Storm, "The Fish spits black water!", "O Peixe cospe água preta!");
            Line(entries, RegionBossRules.BlackTailFishId, RegionBossPhase.LastStand, "The black water is rising!", "A água preta está subindo!");
            Blow(entries, RegionBossRules.BlackTailFishId, "His jaws snap shut!", "Lá vem a bocada!");
            Signature(entries, RegionBossRules.BlackTailFishId, "He's turning his tail on you! Get to his other side or back off!", "Ele está virando a cauda pra você! Passa pro outro lado dele ou recua!");
            Withdraw(entries, RegionBossRules.BlackTailFishId, "The black drains out of the tail like ink out of a cloth. What is left is a small fish with a light on its brow, swimming off to light the halls.", "O preto escorre da cauda como tinta de um pano. O que sobra é um peixinho com uma luz na testa, nadando embora pra iluminar os salões.");

            // -- Cova da Treva (88) --
            Page(entries, RegionBossRules.DeepDragonId,
                "The fifth world was the Darkness itself, children, the world of the ones who crawl, and down there the Prince wore the body of a dragon. That body came loose from him and stayed in the pit, crawling in the dark with nobody inside it. Nothing can be seen down there but by the lamps, and when the Dragon of the Deep breathes, he breathes the dark out all round himself. But there is always one lamp he cannot put out, a little way off to one side of him, one side and then the other. Run into its light and the dark goes past. Beaten, its shadow draws back, and what is left is a little lizard of light that follows the cousins around.",
                "O quinto mundo era a própria Treva, crianças, o mundo dos que rastejam, e lá embaixo o Príncipe usava o corpo de um dragão. Esse corpo se soltou dele e ficou na cova, rastejando no escuro sem ninguém dentro. Lá embaixo nada se vê a não ser pelas lâmpadas, e quando o Dragão do Fundo respira, ele solta o escuro em volta de si. Mas sempre tem uma lâmpada que ele não consegue apagar, um pouco ao lado dele, de um lado e depois do outro. Corram pra luz dela e o escuro passa. Vencido, a sombra dele recua, e o que sobra é um lagartinho de luz que segue os primos por aí.",
                ("Find the lamp, get in its light, then hit him from there.", "Acha a lâmpada, entra na luz dela, e bate dali."),
                ("He's a body nobody lives in. My light can keep him company for a bit.", "Ele é um corpo onde ninguém mora. A minha luz pode fazer companhia pra ele um pouco."),
                ("Pitch dark, giant dragon, one lamp. Whoever designed this pit hates us.", "Escuro total, dragão gigante, uma lâmpada. Quem fez essa cova odeia a gente."),
                ("The lamp is always beside the line between him and us. Left, then right.", "A lâmpada fica sempre do lado da linha entre ele e a gente. Esquerda, depois direita."),
                ("I'll name the little lizard Faísca! No, Biscoito! No, Faísca Biscoito!", "Vou chamar o lagartinho de Faísca! Não, Biscoito! Não, Faísca Biscoito!"));
            Line(entries, RegionBossRules.DeepDragonId, RegionBossPhase.Storm, "The Dragon spits blobs of darkness!", "O Dragão cospe bolas de treva!");
            Line(entries, RegionBossRules.DeepDragonId, RegionBossPhase.LastStand, "The lamps are going out!", "As lâmpadas estão apagando!");
            Blow(entries, RegionBossRules.DeepDragonId, "His coils crash down!", "Lá vêm as voltas do corpo dele!");
            Signature(entries, RegionBossRules.DeepDragonId, "He's breathing the dark! Run to the lamp!", "Ele vai soltar o escuro! Corre pra lâmpada!");
            entries.Add(new LocalizationEntry(LampKey, "Light!", "Luz!"));
            Withdraw(entries, RegionBossRules.DeepDragonId, "The great dark body sinks into the floor of the pit like water into sand. Where its heart was, a tiny lizard of light blinks, and runs to the cousins' feet.", "O corpo grande e escuro afunda no chão da cova como água na areia. Onde era o coração dele, um lagartinho de luz pisca, e corre pros pés dos primos.");

            // -- Ponte do Novelo (89) --
            Page(entries, RegionBossRules.NoveloGuardianId,
                "At the end of the story the Darkness is shut up in a ball, children, the Novelo, and the Artificers set an old ushabti on the bridge to hold it closed. He has held it so long that he is tired, and the threads of the Novelo have crept out and wound round his arms, and they move him now, against us. This is no fight against him: the blows you strike cut the threads off him. And he is still on our side: when a wave rolls out of the Novelo, from the east, he plants his feet and shelters whoever stands close behind him. Stand there. Free him, and he sits down on the bridge at last and rests, with the Novelo shut in his lap.",
                "No fim da história a Treva fica trancada numa bola, crianças, o Novelo, e os Artífices puseram um ushabti velho na ponte pra segurar ela fechada. Ele segura há tanto tempo que está cansado, e os fios do Novelo escaparam e se enrolaram nos braços dele, e agora mexem nele, contra a gente. Isso não é luta contra ele: os golpes de vocês cortam os fios dele. E ele ainda está do nosso lado: quando sai uma onda do Novelo, do leste, ele finca os pés e protege quem fica bem atrás dele. Fiquem ali. Libertem ele, e ele finalmente senta na ponte e descansa, com o Novelo fechado no colo.",
                ("When the wave comes, I get behind him. Then I cut more threads.", "Quando a onda vier, eu fico atrás dele. Depois corto mais fios."),
                ("He's been holding the dark shut for all of us. Let's hold it with him.", "Ele está segurando a treva fechada por todos nós. Vamos segurar com ele."),
                ("Hitting the good guy to help the good guy. This family has the strangest plans.", "Bater no mocinho pra ajudar o mocinho. Essa família tem os planos mais estranhos."),
                ("The wave comes from the Novelo, east. His shadow is west. That's the place.", "A onda vem do Novelo, do leste. A sombra dele fica a oeste. É ali o lugar."),
                ("Mister Ushabti, you can have a nap after! I'll guard the ball! I promise!", "Senhor Ushabti, depois você pode tirar um cochilo! Eu cuido da bola! Prometo!"));
            Line(entries, RegionBossRules.NoveloGuardianId, RegionBossPhase.Storm, "The threads throw sparks of the Novelo!", "Os fios atiram faíscas do Novelo!");
            Line(entries, RegionBossRules.NoveloGuardianId, RegionBossPhase.LastStand, "The Novelo is pulling at the last threads!", "O Novelo está puxando os últimos fios!");
            Blow(entries, RegionBossRules.NoveloGuardianId, "The threads swing his hoe!", "Os fios balançam a enxada dele!");
            Signature(entries, RegionBossRules.NoveloGuardianId, "A wave from the Novelo! Get behind the Guardian!", "Uma onda do Novelo! Fica atrás do Guardião!");
            entries.Add(new LocalizationEntry(ShelterKey, "Behind me!", "Atrás de mim!"));
            Withdraw(entries, RegionBossRules.NoveloGuardianId, "The last thread falls from the Guardian's arms. He sits down on the bridge, very slowly, the Novelo shut tight in his lap, and for the first time in an age he closes his eyes.", "O último fio cai dos braços do Guardião. Ele senta na ponte, bem devagar, com o Novelo bem fechado no colo, e pela primeira vez em muito tempo fecha os olhos.");
        }
    }
}
