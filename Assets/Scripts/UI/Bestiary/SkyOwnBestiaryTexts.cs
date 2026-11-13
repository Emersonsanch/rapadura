using System.Collections.Generic;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Enemies;

namespace Rapadura.UI.Bestiary
{
    /// <summary>
    /// "Caderno da Vó Edna" pages of the ten roles of the Reinos do Céu (Região 16, Docs/Mapas100.md 91–99) that the overworld still played
    /// with another region's beast after the seventeenth pass (bible §3.2.1, eighteenth pass): lore in Grandma Edna's voice and one line per
    /// cousin, en + pt-BR. Kept apart like <see cref="LentRoleBestiaryTexts"/>; <see cref="BestiaryTexts"/> appends these to its
    /// <see cref="BestiaryTexts.Entries"/>. Never gore (bible §4.3).
    /// </summary>
    public static class SkyOwnBestiaryTexts
    {
        /// <summary>The asset ids these pages are for.</summary>
        public static readonly string[] EnemyIds =
        {
            "enemy_forest_runaway_archon", "enemy_forest_stubborn_spark", "enemy_forest_sphere_smoke", "enemy_forest_flower_treader",
            "enemy_forest_smoke_thorn", "enemy_mountain_ember_stowaway", "enemy_forest_noon_shadow", "enemy_forest_last_shadow",
            "enemy_mountain_high_wind", "enemy_forest_fled_archon",
        };

        public static void AddPages(List<LocalizationEntry> entries)
        {
            // -- Cais da Coluna (91) --
            Page(entries, "enemy_forest_runaway_archon",
                "When the Gate of the Land of Darkness opened, the little archons of the Limiar ran up the passes after the light, the way boys run after a cart. They never meant to stay. Now they hang about the edges of the quay with their crowns slipping and a bundle of sparks under the arm, and they can't remember the way down. They hit hard for their size. Take the bundle off them and the sparks go home on their own.",
                "Quando o Portão da Terra da Treva abriu, os arcontezinhos do Limiar subiram as passagens atrás da luz, que nem menino correndo atrás de carroça. Nunca quiseram ficar. Agora ficam rondando a beira do cais com a coroa escorregando e uma trouxinha de faíscas debaixo do braço, e não lembram mais o caminho de volta. Batem forte pro tamanho deles. Tirem a trouxinha deles que as faíscas voltam pra casa sozinhas.",
                ("Both arms go up before the slam. Step back then.", "Os dois braços sobem antes da pancada. Recua nessa hora."),
                ("He looks lost. Maybe he just wants someone to show him the way.", "Ele parece perdido. Vai ver só quer alguém pra mostrar o caminho."),
                ("He stole the sparks AND a barca ticket. Respect.", "Ele roubou as faíscas E uma passagem de barca. Respeito."),
                ("He ran after the light without knowing why. Most of us do.", "Ele correu atrás da luz sem saber por quê. A gente quase sempre faz isso."),
                ("His crown is crooked! Can I fix it for him?", "A coroa dele tá torta! Posso arrumar pra ele?"));

            // -- Convés da Barca da Lua (93) --
            Page(entries, "enemy_forest_stubborn_spark",
                "Every spark that comes up here is supposed to get on the Moon's barca and go home. This one came all the way up, looked at the barca, planted its feet on the deck and crossed its arms. It isn't angry. It just won't. It pushes at anyone who tries to carry it, and it is stronger than it looks. Be patient with it; when it has had its say, it goes.",
                "Toda faísca que chega aqui em cima tem que entrar na barca da Lua e ir pra casa. Esta subiu até o fim, olhou pra barca, fincou os pés no convés e cruzou os braços. Não tá brava. Só não vai. Empurra quem tenta carregar ela, e é mais forte do que parece. Tenham paciência; quando ela terminar de falar o que tem pra falar, ela vai.",
                ("It squashes down before it pushes. Wait for it.", "Ela se achata antes de empurrar. Espera."),
                ("I don't think it's stubborn. I think it's scared of going.", "Eu acho que ela não é teimosa. Acho que tá com medo de ir."),
                ("It's me. That spark is me on a Monday morning.", "Sou eu. Essa faísca sou eu numa segunda de manhã."),
                ("Going home is the hardest trip. Even for light.", "Voltar pra casa é a viagem mais difícil. Até pra luz."),
                ("It has little feet! Tiny glowing feet!", "Ela tem pezinhos! Pezinhos que brilham!"));

            // -- Esferas dos Arcontes (95) --
            Page(entries, "enemy_forest_sphere_smoke",
                "The Living Spirit set the archons in the spheres of the sky so they would stop eating the light, and the spheres hold them well. They don't hold the sulking. It leaks out of every crack as a grey smoke and rolls along the orbits, and wherever it settles it weighs on your legs like a bad mood. Hit the shard it comes out of and the smoke has nowhere to live.",
                "O Espírito Vivo prendeu os arcontes nas esferas do céu pra eles pararem de comer a luz, e as esferas seguram bem. O que elas não seguram é o emburramento. Vaza por toda rachadura como uma fumaça cinza e rola pelas órbitas, e onde ela assenta pesa nas pernas da gente que nem mau humor. Batam no caco de onde ela sai, que a fumaça fica sem lugar pra morar.",
                ("It slows you down. Don't stand in it.", "Ela deixa a gente lento. Não fica parado dentro."),
                ("Somebody in that sphere is very, very sad.", "Alguém dentro daquela esfera tá muito, muito triste."),
                ("I've smelled this before. It's Joaquim's room.", "Eu já senti esse cheiro. É o quarto do Joaquim."),
                ("You can lock someone up. You can't lock up how they feel.", "Dá pra prender alguém. Não dá pra prender o que ele sente."),
                ("It has eyes in the smoke! Grumpy eyes!", "Tem olho na fumaça! Olho emburrado!"));

            // -- Jardim de Luz (96) --
            Page(entries, "enemy_forest_flower_treader",
                "The flowers of the Garden of Light sing when you walk round them. That is the whole rule of the garden: you walk round. This shadow walks straight through on its long thin legs, and every flower it treads on goes quiet, and the petals stick to it. It is quick and it doesn't stop. Get out of its line, and hit it from the side as it goes past.",
                "As flores do Jardim de Luz cantam quando a gente passa em volta delas. Essa é a única regra do jardim: passar em volta. Esta sombra passa reto por cima com as pernas compridas e finas, e toda flor que ela pisa fica quieta, e as pétalas grudam nela. É ligeira e não para. Saiam da frente dela, e batam de lado quando ela passar.",
                ("Fast and straight. Step aside and hit the flank.", "Rápida e reta. Sai de lado e bate no flanco."),
                ("The flowers stop singing where it walks. That makes me so sad.", "As flores param de cantar onde ela pisa. Isso me deixa tão triste."),
                ("Rule number one of the garden and it broke it. Classic.", "Regra número um do jardim e ela quebrou. Clássico."),
                ("It isn't cruel. It just never learned to walk around anything.", "Ela não é má. Só nunca aprendeu a desviar de nada."),
                ("It has petals stuck on it! Like a dress!", "Tem pétala grudada nela! Parece vestido!"));

            Page(entries, "enemy_forest_smoke_thorn",
                "Smoke that drifted into a hedge of the Garden and hardened there into thorns. It grabs at whatever goes past, flowers and children alike, and it keeps what it catches; in the middle of every bramble there is a flower of light it won't let go of. It holds you in place if you get close. Cut it from the edges, and the flower in the middle is set free.",
                "Fumaça que entrou numa sebe do Jardim e endureceu lá dentro em espinho. Agarra tudo que passa, flor e criança, e guarda o que pega; no meio de toda moita tem uma flor de luz que ela não larga. Segura a gente no lugar se chegar perto. Cortem pelas beiradas, que a flor do meio fica livre.",
                ("Don't get caught in the middle. Work the edges.", "Não se deixa prender no meio. Trabalha pelas beiradas."),
                ("It's holding that flower so tight. Like it's afraid of losing it.", "Ela segura aquela flor com tanta força. Parece que tem medo de perder."),
                ("It took my sleeve. I want my sleeve back.", "Ela pegou minha manga. Eu quero minha manga de volta."),
                ("Holding on is not the same as having.", "Segurar não é a mesma coisa que ter."),
                ("Ouch! It's pointy everywhere!", "Ai! Ela é pontuda pra todo lado!"));

            // -- Convés da Barca do Sol (97) --
            Page(entries, "enemy_mountain_ember_stowaway",
                "The dark ones that sneaked onto the Moon's barca came out cold. This one hid on the Sun's barca, curled up by the brazier, and came out hot, cracked all over with embers and very pleased with itself. It runs low across the deck with a coal in its mouth and a singed boarding tag round its neck. It is fast. Don't chase it round the sail; let it come to you.",
                "Os escuros que entraram escondidos na barca da Lua saíram frios. Este se escondeu na barca do Sol, enroladinho do lado do braseiro, e saiu quente, todo rachado de brasa e muito contente consigo mesmo. Corre rasteiro pelo convés com um carvão na boca e uma passagem chamuscada no pescoço. É rápido. Não corram atrás dele em volta da vela; deixem ele vir.",
                ("Low and fast. Let it come, then strike.", "Rasteiro e rápido. Deixa vir, aí bate."),
                ("It's just trying to stay warm. I understand that.", "Ele só tá tentando ficar quentinho. Eu entendo."),
                ("It has a ticket. Why does the stowaway have a ticket and I don't?", "Ele tem passagem. Por que o clandestino tem passagem e eu não?"),
                ("Even the dark wants to sit by the fire.", "Até o escuro quer sentar perto do fogo."),
                ("It's like a little dragon! A little cracked dragon!", "Parece um dragãozinho! Um dragãozinho rachado!"));

            Page(entries, "enemy_forest_noon_shadow",
                "At noon a shadow has nowhere to go, because the sun stands straight overhead. This one squeezed in under the sail of the Sun's barca with a scrap of the canvas over its head, and it comes out to trip whoever walks through its patch of shade. It pulls at your feet and holds you there. Stay out of the shade of the sail when you fight it, and it has nothing to stand on.",
                "Ao meio-dia a sombra não tem pra onde ir, porque o sol fica bem em cima. Esta se espremeu debaixo da vela da barca do Sol com um retalho da lona na cabeça, e sai pra passar rasteira em quem atravessa o pedacinho de sombra dela. Puxa o pé da gente e segura ali. Fiquem fora da sombra da vela quando forem brigar com ela, que ela fica sem onde pisar.",
                ("Fight it in the sun. It's weaker there.", "Briga com ela no sol. Lá ela é mais fraca."),
                ("It just wanted somewhere to hide. The whole sky is so bright.", "Ela só queria um lugar pra se esconder. O céu inteiro é tão claro."),
                ("It's wearing the sail like a hat. I'm stealing that look.", "Ela tá usando a vela de chapéu. Vou copiar esse visual."),
                ("A shadow at noon is the smallest it will ever be. It's still here.", "Sombra ao meio-dia é o menor que ela vai ser. E ainda tá aqui."),
                ("It's hiding! Peekaboo, little shadow!", "Ela tá escondida! Achou, sombrinha!"));

            // -- Pés da Coluna da Glória (98) --
            Page(entries, "enemy_forest_last_shadow",
                "The shadows know there is nowhere higher to hide than the Column. These are the last of them, thin from the long climb, still going up the steps after the light with their arms stretched out to the step above. They don't want you; you are only in the way. They are quick on the steps. When the long arms go up, they come down on you, so step aside.",
                "As sombras sabem que não tem lugar mais alto pra se esconder do que a Coluna. Estas são as últimas, magrinhas da subida comprida, ainda subindo os degraus atrás da luz com os braços esticados pro degrau de cima. Não querem vocês; vocês só estão no caminho. São ligeiras nos degraus. Quando os braços compridos sobem, eles descem em cima de vocês, então saiam de lado.",
                ("Arms up means they're coming down. Move.", "Braço pra cima quer dizer que vai descer. Sai."),
                ("They're trying to go up too. Like us.", "Elas também tão tentando subir. Que nem a gente."),
                ("So many stairs. I'm on their side about the stairs.", "Quanta escada. Nisso eu tô do lado delas."),
                ("The last of something is always the one that tries hardest.", "O último de alguma coisa é sempre o que mais se esforça."),
                ("They're so tall and skinny! Like a stretched cat!", "Elas são tão altas e magrinhas! Que nem gato esticado!"));

            Page(entries, "enemy_mountain_high_wind",
                "Up by the Column the wind blows from below, the way the light goes up. Where it runs into a crack in the sky it twists the wrong way and takes the shape of a bird to come back down, with a little whirl under it that always keeps one foot on the ground. It shoves you off the steps and off the edges of the Lookout. Keep your back to the Column, not to the drop.",
                "Lá perto da Coluna o vento sopra de baixo pra cima, do jeito que a luz sobe. Onde ele bate numa rachadura do céu ele torce pro lado errado e pega forma de passarinho pra descer de volta, com um redemoinho pequeno embaixo que sempre deixa um pé no chão. Empurra a gente dos degraus e da beira do Mirante. Fiquem de costas pra Coluna, não pro precipício.",
                ("It pushes. Never fight it with your back to the edge.", "Ele empurra. Nunca briga com ele de costas pra beira."),
                ("It got turned around. It's not its fault.", "Ele se virou do avesso. Não é culpa dele."),
                ("My hair. Look at my hair. Look what it did.", "Meu cabelo. Olha meu cabelo. Olha o que ele fez."),
                ("Even the wind can go the wrong way once it hits a crack.", "Até o vento vai pro lado errado depois de bater numa rachadura."),
                ("It's a bird made of wind! Can I fly it like a kite?", "É um passarinho de vento! Posso empinar que nem pipa?"));

            // -- Mirante do que Segura o Céu (99) --
            Page(entries, "enemy_forest_fled_archon",
                "He was one of those the Living Spirit set in the spheres. He broke his sphere from the inside and walked out with the pieces still stuck to his shoulders and a broken cuff on one wrist, and now he hides at the Lookout, where the sky is cracked like he is. He is big and he hits like a door slamming. Watch both arms; when they go up, get out from under them.",
                "Ele era um dos que o Espírito Vivo prendeu nas esferas. Quebrou a esfera por dentro e saiu com os cacos ainda grudados nos ombros e uma algema quebrada num pulso, e agora se esconde no Mirante, onde o céu é rachado que nem ele. É grande e bate que nem porta batendo. Olhem os dois braços; quando subirem, saiam debaixo.",
                ("Both arms up, then the slam. Get out from under.", "Os dois braços sobem, depois a pancada. Sai de baixo."),
                ("He broke out and still has nowhere to go. That's lonely.", "Ele fugiu e mesmo assim não tem pra onde ir. Isso é solitário."),
                ("He's still wearing the handcuff. Bold fashion choice.", "Ele ainda tá de algema. Escolha ousada de moda."),
                ("He ran from his sphere to the one place even more broken.", "Ele fugiu da esfera pro único lugar ainda mais quebrado."),
                ("He has crystals on his shoulders! Shiny!", "Ele tem cristal nos ombros! Brilha!"));
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
