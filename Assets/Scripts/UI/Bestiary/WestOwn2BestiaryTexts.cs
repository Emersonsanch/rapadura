using System.Collections.Generic;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Enemies;

namespace Rapadura.UI.Bestiary
{
    /// <summary>
    /// "Caderno da Vó Edna" pages of the nine roles of the Costa das Monções (Região 5, Docs/Mapas100.md 25–29), the Delta de Jhilpur (Região 6,
    /// 33, 35) and the Planície de Mingcheng (Região 8, 41, 46 and the Vale da Névoa) that the overworld still played with another beast after the
    /// twenty-fourth pass (bible §3.2.1, twenty-seventh pass): lore in Grandma Edna's voice and one line per cousin, en + pt-BR. Kept apart like
    /// <see cref="WestOwnBestiaryTexts"/>; <see cref="BestiaryTexts"/> appends these to its <see cref="BestiaryTexts.Entries"/>. Never gore
    /// (bible §4.3).
    /// </summary>
    public static class WestOwn2BestiaryTexts
    {
        /// <summary>The asset ids these pages are for.</summary>
        public static readonly string[] EnemyIds =
        {
            "enemy_desert_striped_jackal", "enemy_desert_smoke_hyena", "enemy_mountain_dimmed_stone_ram", "enemy_forest_dingonek",
            "enemy_forest_moss_crocodile", "enemy_forest_shadow_gharial", "enemy_mountain_shen_mist", "enemy_mountain_hundun_bubble",
            "enemy_desert_riddle_lantern",
        };

        public static void AddPages(List<LocalizationEntry> entries)
        {
            // -- Dunas das Casuarinas (25) and the savannas of Aratta (26) --
            Page(entries, "enemy_desert_striped_jackal",
                "The side-striped jackals hunted the casuarina dunes at dusk and ate what the tide left behind, and the fishermen of Taa Pwani counted on them to keep the beach clean. The tip of their tail was white as a lamp, and the jackals followed each other home by it. The shadow came down the coast with the monsoon and blew into their dens, and the white went grey. Now they hunt in packs by day, and the fishermen stay off the dunes. They are quick but light: keep moving and they lose each other.",
                "Os chacais-listrados caçavam nas dunas das casuarinas no fim da tarde e comiam o que a maré deixava, e os pescadores de Taa Pwani contavam com eles pra manter a praia limpa. A ponta do rabo era branca que nem lamparina, e os chacais voltavam pra casa um seguindo o rabo do outro. A sombra desceu a costa com a monção e soprou dentro das tocas, e o branco ficou cinza. Agora eles caçam em bando de dia, e os pescadores não sobem mais nas dunas. São rápidos mas leves: fiquem se mexendo que eles se perdem uns dos outros.",
                ("Keep moving. A pack that can't see its tails falls apart.", "Fica se mexendo. Bando que não vê o rabo do outro se desmancha."),
                ("They're lost, not mean. Their way home went grey.", "Eles estão perdidos, não são maus. O caminho de casa ficou cinza."),
                ("If my tail went grey I'd be grumpy too.", "Se o meu rabo ficasse cinza eu também ia ficar de mau humor."),
                ("A little white light on the tail, and the whole pack found its way. Light doesn't have to be big to lead.", "Uma luzinha branca no rabo e o bando inteiro achava o caminho. A luz não precisa ser grande pra guiar."),
                ("Let's paint their tails white again!", "Vamos pintar o rabo deles de branco de novo!"));

            // -- Savana das Acácias (26) --
            Page(entries, "enemy_desert_smoke_hyena",
                "In the acacias the hyenas laughed at night and the people of Aratta laughed back, and that was how each knew where the other was: a laugh from the grass meant the hyenas were off hunting, a laugh from the town meant the fire was lit. The shadow put out the laugh first. The hyenas go silent now, smoke rises off their shoulders where the mane was, and they come out of the dark in a pack without a sound. Watch the smoke over the grass: it shows where they are before they come.",
                "Nas acácias as hienas riam de noite e o povo de Aratta ria de volta, e era assim que um sabia onde o outro estava: risada no capim queria dizer que as hienas tinham saído pra caçar, risada na vila queria dizer que o fogo estava aceso. A sombra apagou primeiro a risada. Agora as hienas andam caladas, sai fumaça dos ombros onde era a crina, e elas saem do escuro em bando sem fazer barulho. Olhem a fumaça por cima do capim: ela mostra onde elas estão antes de chegarem.",
                ("Watch the smoke over the grass. It gives them away.", "Olha a fumaça em cima do capim. Ela entrega elas."),
                ("They used to laugh with the town. Let's give them something to laugh about again.", "Elas riam junto com a vila. Vamos dar motivo pra elas rirem de novo."),
                ("A hyena that doesn't laugh? Now that's scary.", "Hiena que não ri? Isso sim dá medo."),
                ("The Darkness doesn't shout. It takes the voice away first. That's how you know it's near.", "A Treva não grita. Ela tira a voz primeiro. É assim que a gente sabe que ela está perto."),
                ("I'll laugh for them until they remember!", "Eu vou rir por elas até elas lembrarem!"));

            // -- Pirâmides Altas de Meroé (27) --
            Page(entries, "enemy_mountain_dimmed_stone_ram",
                "The rams of the avenues of Meroë lay on their plinths with a little king standing between their front legs, and watched the road to the temples for a thousand years without blinking. The shadow got into the cracks of the sandstone. By day they lie still as ever, but they are off the plinth more and more, and at night the whole avenue gets up and walks. They are heavy and slow to turn: when the ram drops its head, step aside, and hit it while it shakes the dust off its horns.",
                "Os carneiros das avenidas de Meroé ficavam deitados no pedestal com um reizinho em pé entre as patas da frente, e vigiavam a estrada dos templos por mil anos sem piscar. A sombra entrou pelas rachaduras do arenito. De dia eles ficam parados como sempre, mas cada vez mais fora do pedestal, e de noite a avenida inteira levanta e anda. São pesados e demoram pra virar: quando o carneiro baixar a cabeça, saiam do lado e batam enquanto ele sacode a poeira dos chifres.",
                ("Head down means it's coming. Step aside.", "Cabeça baixa quer dizer que lá vem. Sai do lado."),
                ("It's keeping the road like it always did. It just forgot who the road is for.", "Ele está guardando a estrada como sempre. Só esqueceu pra quem é a estrada."),
                ("A thousand years lying down. I'd want to stretch my legs too.", "Mil anos deitado. Eu também ia querer esticar as pernas."),
                ("Stone holds a little of the Light the world was made with. The shadow can move it, but it can't make it its own.", "A pedra guarda um pouco da Luz de que o mundo foi feito. A sombra consegue mexer nela, mas não consegue fazer dela coisa sua."),
                ("Where's the little king? Did he go for a walk too?", "Cadê o reizinho? Ele foi passear também?"));

            // -- Kilwa Afundada (29) --
            Page(entries, "enemy_forest_dingonek",
                "The dingonek kept the fords of the rivers that run down to the coast, and let only the honest cross: the boatmen of Kilwa threw it a fish on the way over and told it the truth about their cargo. When Kilwa went under the sea the fords went under with it, and the shadow found the dingonek alone in the drowned streets, guarding a ford that is not there any more. Now it lets no one cross, and at night it walks the streets of the rays. Its tusks come down slow and hard: back off when it rears, and hit it after.",
                "O dingonek guardava os vaus dos rios que descem pra costa, e só deixava passar quem era honesto: os barqueiros de Kilwa jogavam um peixe pra ele na travessia e contavam a verdade sobre a carga. Quando Kilwa afundou, os vaus afundaram junto, e a sombra achou o dingonek sozinho nas ruas afogadas, guardando um vau que não existe mais. Agora ele não deixa ninguém passar, e de noite anda pelas ruas das raias. As presas descem devagar e com força: recuem quando ele se levantar e batam depois.",
                ("Slow bite, heavy bite. Back off, then hit.", "Mordida lenta, mordida pesada. Recua e depois bate."),
                ("It's still keeping its ford. Somebody has to tell it the river moved.", "Ele ainda está guardando o vau dele. Alguém precisa contar que o rio mudou de lugar."),
                ("I'll tell it the truth: my bag is full of snacks. Can I cross now?", "Vou contar a verdade: minha bolsa está cheia de lanche. Posso passar agora?"),
                ("It let the honest cross. The shadow can't stand that kind of guard, so it took away what it was guarding.", "Ele deixava passar quem era honesto. A sombra não suporta um guarda assim, então tirou dele o que ele guardava."),
                ("It has teeth like a walrus and spots like a leopard! Two animals in one!", "Ele tem dente de morsa e pinta de onça! Dois bichos num só!"));

            // -- Pântano Aberto and Ilhas que Andam (33) --
            Page(entries, "enemy_forest_moss_crocodile",
                "The crocodiles of the open swamp slept so long under the stilts of Tarchon that moss grew on their backs, and the trolls planted reeds in it for luck. A troll child could cross the swamp from back to back without wetting a foot. The shadow came up the swamp in the lilac mist and woke them. Now the moss moves, and the reeds with it, and the nets of Tarchon come up torn. They hit hard and turn slowly: go round the side, never in front of the jaws.",
                "Os crocodilos do pântano aberto dormiram tanto tempo debaixo das palafitas de Tarchon que nasceu musgo nas costas deles, e os trols plantavam junco ali pra dar sorte. Criança trol atravessava o pântano pulando de costa em costa sem molhar o pé. A sombra subiu o pântano na névoa lilás e acordou eles. Agora o musgo anda, e o junco junto, e as redes de Tarchon voltam rasgadas. Batem forte e viram devagar: vão pelo lado, nunca na frente da boca.",
                ("Go round the side. Never in front of the jaws.", "Vai pelo lado. Nunca na frente da boca."),
                ("The trolls planted reeds on them for luck. They were part of the town.", "Os trols plantavam junco neles pra dar sorte. Eles eram parte da vila."),
                ("I stood on a rock to rest. It wasn't a rock.", "Eu subi numa pedra pra descansar. Não era pedra."),
                ("It was asleep and at peace. The shadow doesn't build anything. It only wakes what was resting, the wrong way.", "Ele estava dormindo em paz. A sombra não constrói nada. Só acorda o que descansava, do jeito errado."),
                ("It has a garden on its back! I want a garden on my back!", "Ele tem um jardim nas costas! Eu quero um jardim nas costas!"));

            // -- Raízes Afogadas, Coração do Lodo and Olho do Lodo (35) --
            Page(entries, "enemy_forest_shadow_gharial",
                "The Lodo is the fourth of the worlds of the Darkness, and its people are the ones who swim. The gharials that went down the delta after the fish swam into the black water at the heart of it and did not come back up the same: half of each is shadow now, and the shadow half is the hungry one. Their snout is thin as a reed and snaps sideways, quick. Keep to the light side, where you can see the body, and the jaws come slower.",
                "O Lodo é o quarto dos mundos da Treva, e o povo dele é o que nada. Os gaviais que desceram o delta atrás dos peixes nadaram pra dentro da água preta do coração dele e não voltaram iguais: metade de cada um é sombra agora, e a metade sombra é a que tem fome. O focinho é fino que nem junco e estala de lado, rápido. Fiquem do lado claro, onde dá pra ver o corpo, que a boca vem mais devagar.",
                ("Keep to the side you can see. The shadow side bites faster.", "Fica do lado que dá pra ver. O lado sombra morde mais rápido."),
                ("Half of it is still a gharial. That's the half I'm talking to.", "Metade dele ainda é gavial. É com essa metade que eu estou falando."),
                ("Half a gharial, double the trouble.", "Meio gavial, problema em dobro."),
                ("The five worlds of the Darkness: smoke, fire, wind, water, and the dark itself. This one swam into the water one. We can pull it back out.", "Os cinco mundos da Treva: fumaça, fogo, vento, água e a própria escuridão. Esse nadou pro da água. A gente consegue puxar ele de volta."),
                ("Its nose is so thin! How does it smell anything?", "O nariz dele é tão fininho! Como é que ele cheira alguma coisa?"));

            // -- Vale da Névoa (Mingcheng) --
            Page(entries, "enemy_mountain_shen_mist",
                "The old books of the canal say the shen is a clam of the deep river, and when it sighs its breath draws towers and walls in the air over the water, and the boatmen steered by them as if they were real. In the Vale da Névoa the shen sighed the shadow's smoke. The towers it draws now are towers of nothing, and whoever walks into them gets lost. The mist sinks down before it strikes, and the pearl inside it flares: that is the moment to step out.",
                "Os livros velhos do canal dizem que o shen é uma ostra do fundo do rio, e quando ela suspira o bafo desenha torres e muralhas no ar por cima da água, e os barqueiros se guiavam por elas como se fossem de verdade. No Vale da Névoa o shen suspirou a fumaça da sombra. As torres que ele desenha agora são torres de nada, e quem entra nelas se perde. A névoa afunda antes de bater, e a pérola de dentro brilha: é a hora de sair dali.",
                ("When it sinks and the pearl flares, step out.", "Quando ela afundar e a pérola brilhar, sai dali."),
                ("It's only a sigh. Somebody's sad down there in the river.", "É só um suspiro. Tem alguém triste lá no fundo do rio."),
                ("I walked into a tower and it wasn't there. My nose was, though. Ouch.", "Eu entrei numa torre e ela não estava lá. Meu nariz estava. Ai."),
                ("A picture of a tower isn't a tower. The shadow only ever shows pictures. The Light is the thing itself.", "Desenho de torre não é torre. A sombra só mostra desenho. A Luz é a coisa de verdade."),
                ("Can it draw a castle for me? A real one this time?", "Ela desenha um castelo pra mim? Um de verdade dessa vez?"));

            // -- Cidadela dos Quatro Gulosos (46) --
            Page(entries, "enemy_mountain_hundun_bubble",
                "The old book of the mountains and seas draws Hundun as a yellow sack, red as cinnabar fire, with six legs and four wings and no face at all. It cannot see or hear, but it knows how to dance and sing. In the Citadel of the Four Gluttons it is the one that eats the light, and the bubbles that come off it eat it too, a little at a time, and float on swollen. They flatten before they burst on you: when one squashes down, get out of the way, and pop it after.",
                "O livro velho das montanhas e dos mares desenha o Hundun como um saco amarelo, vermelho que nem fogo de cinábrio, com seis pernas e quatro asas e cara nenhuma. Ele não vê nem ouve, mas sabe dançar e cantar. Na Cidadela dos Quatro Gulosos é ele que come a luz, e as bolhas que saem dele comem também, um pouquinho de cada vez, e saem flutuando estufadas. Elas achatam antes de estourar em cima de vocês: quando uma achatar, saiam da frente e estourem depois.",
                ("When it squashes down, get clear. Then pop it.", "Quando ela achatar, sai de perto. Depois estoura."),
                ("It has no face, but it can dance. That's not nothing.", "Ela não tem cara, mas sabe dançar. Isso não é pouca coisa."),
                ("A bubble that eats light. My soap bubbles only ate my homework.", "Bolha que come luz. As minhas bolhas de sabão só comiam meu dever de casa."),
                ("Every bit of light it swallowed is still in there. Pop it, and the light goes free.", "Cada pedacinho de luz que ela engoliu ainda está lá dentro. Estoura, e a luz fica livre."),
                ("Six legs AND four wings? It can't decide if it walks or flies!", "Seis pernas E quatro asas? Ela não decide se anda ou se voa!"));

            // -- Anéis de Mingcheng and Vilas do Anel (41) --
            Page(entries, "enemy_desert_riddle_lantern",
                "At the Festival of Lanterns the people of Mingcheng hang a riddle on every lantern, and whoever guesses it takes the lantern home. The shadow took the lanterns nobody guessed, and put out the candle in them. Now they drift over the rings and the villages with their riddles still hanging, and throw their cold light at whoever cannot answer. They throw from far off and lean back first: when the lantern tips back, step to the side, then come in close, where it cannot aim.",
                "Na Festa das Lanternas o povo de Mingcheng pendura uma charada em cada lanterna, e quem adivinha leva a lanterna pra casa. A sombra pegou as lanternas que ninguém adivinhou e apagou a vela de dentro. Agora elas ficam boiando por cima dos anéis e das vilas com a charada ainda pendurada, e jogam a luz fria delas em quem não sabe responder. Atiram de longe e se inclinam pra trás antes: quando a lanterna tombar pra trás, deem um passo pro lado e cheguem perto, onde ela não consegue mirar.",
                ("It tips back before it throws. Step aside, then close in.", "Ela tomba pra trás antes de atirar. Passo pro lado e chega perto."),
                ("Nobody guessed its riddle. Poor thing, it waited all night.", "Ninguém adivinhou a charada dela. Coitada, esperou a noite inteira."),
                ("I read the riddle. The answer was \"lantern\". It did not like that.", "Eu li a charada. A resposta era \"lanterna\". Ela não gostou."),
                ("A riddle is a little light hidden in words. Guess it, and the light is yours to carry home.", "Charada é uma luzinha escondida nas palavras. Adivinha, e a luz é sua pra levar pra casa."),
                ("If I guess right, can I keep it? Please?", "Se eu acertar, posso ficar com ela? Por favor?"));
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
