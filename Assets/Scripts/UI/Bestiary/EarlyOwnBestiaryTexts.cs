using System.Collections.Generic;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Enemies;

namespace Rapadura.UI.Bestiary
{
    /// <summary>
    /// "Caderno da Vó Edna" pages of the roles of Regions 1–4 (Docs/Mapas100.md 03–06, 09, 16, 19, 20) that the continuous world still played
    /// with another beast after the twelfth pass (bible §3.2.1, twenty-third pass): lore in Grandma Edna's voice and one line per cousin, en +
    /// pt-BR. Kept apart like <see cref="SkyOwnBestiaryTexts"/>; <see cref="BestiaryTexts"/> appends these to its <see cref="BestiaryTexts.Entries"/>.
    /// Never gore (bible §4.3).
    /// </summary>
    public static class EarlyOwnBestiaryTexts
    {
        /// <summary>The asset ids these pages are for.</summary>
        public static readonly string[] EnemyIds =
        {
            "enemy_forest_mud_eel", "enemy_forest_cocoon_caterpillar", "enemy_forest_thread_jackdaw", "enemy_forest_sticky_mist",
            "enemy_forest_dimmed_thornrose", "enemy_forest_ash_wolf", "enemy_forest_grotto_spider", "enemy_mountain_blade_stone",
            "enemy_forest_stone_lizard", "enemy_cave_karez_rat", "enemy_desert_water_scorpion",
        };

        public static void AddPages(List<LocalizationEntry> entries)
        {
            // -- Brejo do Lodo Escuro (03) --
            Page(entries, "enemy_forest_mud_eel",
                "Under the dark mire there is another floor, the Mud of the Darkness, and the eels live between the two. This one waits in the puddles off the boardwalk with only its head above the water, and when a foot comes off the logs it is already there. Stay on the boardwalk and it has to come out to reach you. Joaquim's clean water makes it let go.",
                "Debaixo do brejo escuro tem outro chão, o Lodo da Treva, e as enguias vivem entre os dois. Esta espera nas poças fora da passarela só com a cabeça pra fora d'água, e quando um pé sai dos troncos ela já está lá. Fiquem na passarela que ela tem que sair pra alcançar vocês. A água limpa do Joaquim faz ela soltar.",
                ("It drops its jaw before it lunges. Step back onto the logs.", "Ela abaixa a boca antes do bote. Volta pros troncos."),
                ("It only lives in the dark because the spring was swallowed. Poor thing.", "Ela só vive no escuro porque engoliram a nascente. Coitada."),
                ("My water, its mud. I like my odds.", "Minha água, a lama dela. Gosto das minhas chances."),
                ("Two floors in one swamp. Which one is the real one?", "Dois chãos no mesmo brejo. Qual é o de verdade?"),
                ("It has whiskers like a cat! A wet cat!", "Ela tem bigode que nem gato! Um gato molhado!"));

            // -- Bosque das Amoreiras de Seda (04) --
            Page(entries, "enemy_forest_cocoon_caterpillar",
                "When the looms stopped, the silkworms of the grove were left on the mulberries with nobody to gather them. One ate in the dark until it was as big as a sheep, and it will not become a moth: it only spins. It has wound a little stolen light into the cocoon on its back. It is slow and heavy and pushes like a cart; walk round it, not into it.",
                "Quando os teares pararam, os bichos-da-seda do bosque ficaram nas amoreiras sem ninguém pra colher. Um comeu no escuro até ficar do tamanho de uma ovelha, e não vira mariposa: só fia. Enrolou um pouquinho de luz roubada no casulo das costas. É lento e pesado e empurra que nem carroça; deem a volta nele, não vão de frente.",
                ("It squashes down before it shoves. Wait, then hit the side.", "Ele se achata antes de empurrar. Espera e bate do lado."),
                ("It's holding the light so tight. Like it's afraid someone will take it.", "Ele segura a luz tão apertado. Como se tivesse medo de alguém levar."),
                ("It's a sheep made of noodles. I'm not even joking.", "É uma ovelha feita de macarrão. Nem tô brincando."),
                ("It never became a moth. Maybe it didn't want to.", "Ele nunca virou mariposa. Vai ver não quis."),
                ("It's fluffy! Can I hug it? Just a little?", "Ele é fofinho! Posso abraçar? Só um pouquinho?"));

            Page(entries, "enemy_forest_thread_jackdaw",
                "Jackdaws take whatever shines, and in the grove what shines is the silk. This one has been unwinding the Casa do Tear one spool at a time; it carries the spool under its breast and the thread runs out behind it all through the rows. It is quick and it pecks at the face. Follow the thread back and you will find what it took.",
                "Gralha leva tudo o que brilha, e no bosque o que brilha é a seda. Esta anda desenrolando a Casa do Tear um carretel por vez; leva o carretel debaixo do peito e o fio vai ficando pra trás pelas fileiras. É rápida e bica na cara. Sigam o fio de volta que vocês acham o que ela levou.",
                ("The beak opens, then the peck. Duck.", "O bico abre, depois vem a bicada. Abaixa."),
                ("The weaver needs that thread. Let's bring it home.", "A tecelã precisa daquele fio. Vamos levar de volta."),
                ("A thief bird with a getaway rope. Smart. Annoying, but smart.", "Pássaro ladrão com corda de fuga. Esperto. Chato, mas esperto."),
                ("It takes one spool at a time. Patience is its own kind of theft.", "Ela leva um carretel por vez. Paciência é um tipo de roubo também."),
                ("It has white eyes! Is it looking at me?", "Ela tem olho branco! Tá olhando pra mim?"));

            // -- Copas de Garajonay (05) --
            Page(entries, "enemy_forest_sticky_mist",
                "The laurels of Garajonay drink the mist; that is how the forest lives. When the Smoke came up through the trees it mixed with the real mist, and now some of it is sticky. It hangs low and lets down strands to the ground to catch your feet. You can't cut a mist, but you can walk out of it: keep moving and it can't hold you.",
                "Os louros de Garajonay bebem a névoa; é assim que a mata vive. Quando a Fumaça subiu pelas árvores ela se misturou com a névoa de verdade, e agora um pedaço dela gruda. Fica baixa e solta fios até o chão pra pegar os pés. Névoa não se corta, mas dá pra sair dela: não parem de andar que ela não segura vocês.",
                ("It shrinks before it grabs. Move then.", "Ela encolhe antes de agarrar. Sai nessa hora."),
                ("The trees drink the mist. This one makes them sick.", "As árvores bebem a névoa. Essa deixa elas doentes."),
                ("Fighting fog. Grandma, this is a real job?", "Lutar contra neblina. Vó, isso é trabalho de verdade?"),
                ("Half of it is real mist. Which half do we fight?", "Metade dela é névoa de verdade. Qual metade a gente enfrenta?"),
                ("It's a cloud that fell down! It's sticky like honey!", "É uma nuvem que caiu! Gruda que nem mel!"));

            // -- Jardim Esquecido de Mihrshah (06) --
            Page(entries, "enemy_forest_dimmed_thornrose",
                "Mihrshah's roses grew in the four beds of the garden, by the canals. When the canals went dry, one bush pulled its roots out of the bed and went looking for water, and its thorns came with it. Its roses went grey except one at the top, which it keeps lit. It does not chase far, but it holds whoever comes close. Fill the canals and it will want to go back to its bed.",
                "As rosas de Mihrshah cresciam nos quatro canteiros do jardim, junto dos canais. Quando os canais secaram, uma roseira arrancou as raízes do canteiro e saiu procurando água, e os espinhos vieram junto. As rosas dela ficaram cinza, menos uma lá no alto, que ela mantém acesa. Não corre muito atrás, mas segura quem chega perto. Encham os canais que ela vai querer voltar pro canteiro.",
                ("It sinks on its roots before it lashes. Step out of reach.", "Ela se abaixa nas raízes antes de chicotear. Sai do alcance."),
                ("It kept one rose lit. It still remembers the garden.", "Ela manteve uma rosa acesa. Ainda lembra do jardim."),
                ("A plant that walks off to find water. I respect the hustle.", "Uma planta que sai andando atrás de água. Respeito a correria."),
                ("It left the bed to save itself and lost all its flowers.", "Ela saiu do canteiro pra se salvar e perdeu todas as flores."),
                ("The pink one is still pretty! Can we save that one?", "A cor-de-rosa ainda é bonita! Dá pra salvar essa?"));

            Page(entries, "enemy_forest_ash_wolf",
                "Saklas keeps his garden with wolves made of what the fire left. They are grey all through, and the embers in the ruff never quite go out; where they lie down the grass goes grey too. They run in a pack and they are fast, faster than the wolves of the Tugai. They are not his children, they are his ashes. When he is beaten, they go cold.",
                "Saklas cuida do jardim com lobos feitos do que o fogo deixou. São cinza de ponta a ponta, e as brasas da juba nunca apagam de vez; onde deitam a grama fica cinza também. Andam em matilha e são rápidos, mais que os lobos do Tugai. Não são filhos dele, são as cinzas dele. Quando ele for vencido, eles esfriam.",
                ("The jaw drops before the bite. Sidestep and shoot.", "A boca abre antes da mordida. Desvia e atira."),
                ("They're only ashes he keeps warm. That's sad.", "São só cinzas que ele mantém quentes. Que triste."),
                ("Hot wolves. Great. I'll get the water.", "Lobos quentes. Ótimo. Vou buscar a água."),
                ("The grass goes grey wherever they rest. Like grief does.", "A grama fica cinza onde eles deitam. Que nem tristeza."),
                ("It has sparkles in its hair! Like a firework!", "Ele tem brilhinho no cabelo! Que nem fogos!"));

            // -- Galerias do Karez (09) --
            Page(entries, "enemy_cave_karez_rat",
                "The karez carry the water under the sand for miles, and the rats followed the water down. Down there, with no sun and plenty to drink, they grew as big as dogs. They swarm out of the dark sections of the galleries, where the sunbeams don't reach, and they hate the light: stand in a sunbeam and they come at you one at a time instead of all at once.",
                "Os karez levam a água por baixo da areia por léguas, e os ratos foram atrás da água lá pra baixo. Lá embaixo, sem sol e com água à vontade, ficaram do tamanho de cachorro. Saem em bando dos trechos escuros das galerias, onde o facho de sol não chega, e odeiam a luz: fiquem num facho que eles vêm um de cada vez em vez de todos juntos.",
                ("The teeth come out before the bite. Keep moving.", "Os dentes aparecem antes da mordida. Não para."),
                ("They only came for the water. Everyone does, out here.", "Eles só vieram atrás da água. Todo mundo vem, por aqui."),
                ("Dog-sized rats. I'm sleeping with my shield tonight.", "Rato do tamanho de cachorro. Vou dormir abraçado no escudo hoje."),
                ("Things grow big in the dark when nobody looks at them.", "As coisas crescem no escuro quando ninguém olha pra elas."),
                ("It has pink ears! And a pink nose! And a pink tail!", "Ele tem orelha rosa! E nariz rosa! E rabo rosa!"));

            Page(entries, "enemy_desert_water_scorpion",
                "In the karez the water runs clear and cold, and the water scorpion waits at the edge with its forelegs folded and the little tube of its tail up in the air to breathe. It is not a real scorpion and it has no sting. The Darkness dimmed the channel and it grew as big as a dog; it still waits, and it still strikes fast with those folded legs.",
                "No karez a água corre limpa e fria, e o escorpião-d'água espera na beira com as patas da frente dobradas e o canudinho do rabo pra fora pra respirar. Não é escorpião de verdade e não tem ferrão. A Treva apagou o canal e ele cresceu do tamanho de um cachorro; continua esperando, e continua dando o bote rápido com aquelas patas dobradas.",
                ("Watch the folded legs. When they snap, step back.", "Olha as patas dobradas. Quando abrirem, recua."),
                ("It just wants the water clean again, like we do.", "Ele só quer a água limpa de novo, que nem a gente."),
                ("No sting? Then why is it called a scorpion? False advertising.", "Sem ferrão? Então por que chama escorpião? Propaganda enganosa."),
                ("It breathes through a straw so it never leaves the water.", "Ele respira por um canudo pra nunca sair da água."),
                ("It has a straw! Like when I drink juice!", "Ele tem canudinho! Que nem quando eu tomo suco!"));

            // -- Muralhas de Pedra Seca (16) --
            Page(entries, "enemy_forest_stone_lizard",
                "The lizards of the walls are the colour of the granite, so the hawks miss them. This one kept still so long that lichen grew on its back, and now it sleeps in the warm courses of the drystone and runs out at footsteps faster than you would think. It bites and lets go and runs back into the wall. Watch the gaps between the stones.",
                "Os lagartos dos muros têm a cor do granito, pros gaviões errarem. Este ficou parado tanto tempo que nasceu líquen nas costas dele, e agora dorme nas fiadas quentes da pedra seca e sai correndo com o barulho de passo mais rápido do que vocês imaginam. Morde, solta e volta correndo pro muro. Olhem os vãos entre as pedras.",
                ("It opens its mouth before it lunges. Wait for it.", "Ele abre a boca antes do bote. Espera."),
                ("It just wanted a warm stone to sleep on.", "Ele só queria uma pedra quente pra dormir."),
                ("It has a garden on its back. Lazy AND fast. How?", "Ele tem uma horta nas costas. Preguiçoso E rápido. Como?"),
                ("You stop moving long enough and the world grows on you.", "Se você para tempo demais, o mundo cresce em cima de você."),
                ("It looks like a rock! A rock with legs!", "Parece uma pedra! Uma pedra com perna!"));

            // -- Lago do Limiar (19) --
            Page(entries, "enemy_forest_grotto_spider",
                "The spiders of the Limiar have never seen the sun. They hang their webs by the lake where the lamplight reaches, because that is where the moths come, and lately the cousins too. They are pale as tufa and their eyes are little crystals that catch your lamp before you see them. They are quick; light the lamps and you will see them coming.",
                "As aranhas do Limiar nunca viram o sol. Penduram as teias junto do lago onde a luz da lamparina chega, porque é lá que vêm as mariposas, e ultimamente os primos também. São claras que nem tufo e os olhos são cristaizinhos que pegam a luz da lamparina antes de vocês verem elas. São rápidas; acendam as lâmpadas que vocês veem elas chegando.",
                ("The fangs open before the bite. Back off one step.", "As presas abrem antes da mordida. Recua um passo."),
                ("They live by the lamps because they're cold too.", "Elas moram perto das lâmpadas porque também sentem frio."),
                ("Eight eyes, eight legs, zero manners.", "Oito olhos, oito pernas, zero educação."),
                ("They never saw the sun, so they love the lamps. That makes sense.", "Elas nunca viram o sol, então amam as lâmpadas. Faz sentido."),
                ("Its eyes are shiny like Maria's crystals!", "Os olhos dela brilham que nem os cristais da Maria!"));

            // -- Salão de Tages (20) --
            Page(entries, "enemy_mountain_blade_stone",
                "Tages's hall grew its columns drop by drop for ten thousand years. When the hall began to sigh and put out its lamps, a few of the thin ones split off sharp, and now they shuffle about on their base and lean into whoever passes. They are slow and very hard. Lit lamps calm them; in the dark they lean harder.",
                "O salão de Tages fez as colunas gota a gota durante dez mil anos. Quando o salão começou a suspirar e apagar as lâmpadas, umas das fininhas racharam afiadas, e agora andam arrastando a base e se jogam em cima de quem passa. São lentas e muito duras. Lâmpada acesa acalma elas; no escuro elas se jogam mais forte.",
                ("It settles down before it lunges. That's the moment.", "Ela se assenta antes de se jogar. É nessa hora."),
                ("They broke off the hall. They just want to go back.", "Elas se soltaram do salão. Só querem voltar."),
                ("A rock that walks and stabs. The Limiar has no chill.", "Uma pedra que anda e espeta. O Limiar não tem sossego."),
                ("Ten thousand years to grow and one sigh to break.", "Dez mil anos pra crescer e um suspiro pra quebrar."),
                ("It's sparkly in the middle! Is that its heart?", "Ela brilha no meio! É o coração dela?"));
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
