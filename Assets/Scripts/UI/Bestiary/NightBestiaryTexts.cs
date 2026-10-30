using System.Collections.Generic;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Enemies;

namespace Rapadura.UI.Bestiary
{
    /// <summary>
    /// "Caderno da Vó Zefa" pages of the night-only species (Docs/DayNight.md): lore in Vó Zefa's voice and one line
    /// per cousin, en + pt-BR. Kept apart from <see cref="BestiaryTexts"/> so the night pass and the bestiary don't edit
    /// the same lines; <see cref="BestiaryTexts"/> appends these to its own <see cref="BestiaryTexts.Entries"/>, so they are
    /// merged, covered by the localization tests and looked up exactly like every other page.
    /// Tone: folklore for kids — a little spooky, always fair, with a way to win and a touch of humour.
    /// </summary>
    public static class NightBestiaryTexts
    {
        public static void AddPages(List<LocalizationEntry> entries)
        {
            // -- Floresta --
            Page(entries, "enemy_forest_werewolf",
                "On full-moon nights an old wanderer of the scrub turns into the Sertão Werewolf: shaggy, smelly and with one ear that never stands up. He runs on all fours and rises on his hind legs to swipe. Grandma says he isn't evil, just grumpy from missing supper. When he howls, step aside: the big swipe comes right after.",
                "Em noite de lua cheia, um andarilho velho da caatinga vira o Lobisomem-do-sertão: peludo, fedido e com uma orelha que nunca fica em pé. Corre de quatro e se levanta nas patas de trás pra dar a patada. A Vó diz que ele não é malvado, só ranzinza de quem perdeu a janta. Quando ele uivar, saia de lado: a patada grande vem logo depois.",
                ("When he stands up, he's slow to come down. That's my shot.", "Quando ele fica em pé, demora pra descer. É a minha flechada."),
                ("He smells like a wet dog... I'll heal you, but nobody hugs him!", "Ele tem cheiro de cachorro molhado... eu curo vocês, mas ninguém abraça ele!"),
                ("Big claws, big shield. Come on, hairy!", "Unha grande, escudo grande. Pode vir, cabeludo!"),
                ("His ear is bent. Asymmetric. It bothers me more than the claws.", "A orelha dele é torta. Assimétrica. Me incomoda mais que as unhas."),
                ("He howled at me and I howled back. He looked confused.", "Ele uivou pra mim e eu uivei de volta. Ele ficou confuso."));

            Page(entries, "enemy_forest_shroud_owl",
                "The shroud-tearer owl got its name from its screech, which sounds like someone ripping a sheet in the dark. That white heart face floats between the trees like a little moon. It swoops down fast, but the lantern light dazzles it: stay in the light and hit it when it comes down.",
                "A coruja-rasga-mortalha tem esse nome por causa do grito, que parece alguém rasgando um lençol no escuro. Aquela cara branca de coração flutua entre as árvores feito uma luazinha. Ela desce num rasante ligeiro, mas a luz da lamparina deixa ela tonta: fiquem na claridade e batam quando ela descer.",
                ("Her face shines in the dark. Easy to aim at.", "A cara dela brilha no escuro. Fácil de mirar."),
                ("That scream gave me goosebumps. Joaquim, stay close to the light.", "Esse grito me arrepiou toda. Joaquim, fica perto da luz."),
                ("I'm not scared. I just jumped because I wanted to.", "Eu não tô com medo. Eu pulei porque quis."),
                ("She turned her head all the way back! How many joints does that neck have?", "Ela virou a cabeça toda pra trás! Quantas juntas tem esse pescoço?"),
                ("She's my favourite. She's spooky AND pretty.", "Ela é a minha preferida. É assustadora E bonita."));

            Page(entries, "enemy_forest_peccary",
                "Queixadas walk in herds, and these belong to the Caipora, keeper of the forest animals. They charge in a straight line, one after the other, clacking their teeth. Step out of the line and hit them from the side. And no greed: if you knock down too many too fast, the Caipora whistles and the next herd runs away.",
                "Queixada anda em bando, e essas aí são da Caipora, a dona dos bichos da mata. Elas investem em linha reta, uma atrás da outra, batendo os dentes. Saiam da linha e batam de lado. E nada de ganância: se derrubar demais muito depressa, a Caipora assobia e o próximo bando foge.",
                ("They only run straight. I just have to not be in front.", "Elas só correm reto. É só eu não estar na frente."),
                ("Careful, they're in a group! Nobody gets trampled today.", "Cuidado, elas vêm em grupo! Hoje ninguém é atropelado."),
                ("Three at once? I'll hold all three!", "Três de uma vez? Eu seguro as três!"),
                ("If I put a ramp in front of them... no, Maithe, I'm not doing it.", "Se eu botar uma rampa na frente delas... não, Maithe, eu não vou fazer isso."),
                ("I heard a whistle in the trees. I didn't like it.", "Eu ouvi um assobio no mato. Não gostei."));

            // -- Deserto --
            Page(entries, "enemy_desert_firefly_swarm",
                "Fire-fireflies are sparks that forgot to go out. They come at dusk, dancing over the dunes and the edge of the forest, and pinch like a hot pepper. Pretty from far away, a nuisance up close. A good sweeping blow puts out the whole swarm at once — and their light makes a fine lantern.",
                "Vaga-lume-de-fogo é faísca que esqueceu de apagar. Eles chegam no entardecer, dançando por cima das dunas e da beira da mata, e beliscam que nem pimenta. De longe é bonito, de perto é amolação. Uma boa pancada em roda apaga o enxame inteiro de uma vez — e a luz deles dá uma lamparina e tanto.",
                ("So many little lights... I'll aim at the middle of the cloud.", "Tanta luzinha... vou mirar no meio da nuvem."),
                ("They singe, but just a bit. Come here, I'll blow on it.", "Eles queimam, mas só um pouquinho. Vem cá que eu assopro."),
                ("I swung the shield around and three went out. Three!", "Girei o escudo e apagaram três. Três!"),
                ("Cold light! Well, warm light. I need to study this.", "Luz fria! Quer dizer, luz quente. Preciso estudar isso."),
                ("I wanted to keep one in a jar. Grandma said no.", "Eu queria guardar um num pote. A Vó disse que não."));

            // -- Montanha --
            Page(entries, "enemy_mountain_potoo",
                "The moon-mother sits so still on a dry branch that she looks like part of the tree. At night she sings a sad song that goes down, down, down... and calls other critters. Only her big yellow eyes give her away. Find her before she sings — and watch that beak: it's tiny, but it opens enormous.",
                "A mãe-da-lua fica tão paradinha num galho seco que parece pedaço da árvore. De noite ela canta uma cantiga triste que vai descendo, descendo... e chama outros bichos. Só os olhões amarelos entregam. Achem ela antes que cante — e cuidado com o bico: é pequenininho, mas abre enorme.",
                ("I found her first. Those eyes don't fool me.", "Eu achei ela primeiro. Esses olhos não me enganam."),
                ("Her song is so sad... I almost wanted to sing along.", "A cantiga dela é tão triste... quase deu vontade de cantar junto."),
                ("I thought it was a branch. I sat next to it. It was not a branch.", "Eu achei que era um galho. Sentei do lado. Não era um galho."),
                ("She flew off with the branch still in her feet. That's not aerodynamic!", "Ela voou com o galho ainda nos pés. Isso não é aerodinâmico!"),
                ("We had a staring contest. I lost.", "A gente fez um concurso de quem pisca primeiro. Eu perdi."));

            // -- Cavernas --
            Page(entries, "enemy_cave_vampire_bat",
                "The giant vampire bat is the grandfather of all cave bats: wings as wide as a hammock and two fangs it's very proud of. It bites and drinks a little of your strength to feel better. Keep the lantern lit and hit it right after it dives — it's big, so it takes a while to climb back up.",
                "O morcego-vampiro gigante é o avô de todos os morcegos da caverna: asa do tamanho de uma rede e dois dentões de que ele morre de orgulho. Ele morde e bebe um tiquinho da força da gente pra ficar melhor. Deixem a lamparina acesa e batam logo depois do mergulho — ele é grande, demora pra subir de novo.",
                ("Big wings, big target. Thank you, bat.", "Asa grande, alvo grande. Obrigada, morcego."),
                ("He drinks our strength? Then I'll heal faster than he drinks.", "Ele bebe nossa força? Então eu curo mais rápido do que ele bebe."),
                ("He tried to bite my shield. Now he has a crooked fang.", "Ele tentou morder meu escudo. Agora tem um dente torto."),
                ("Wingspan of at least two meters. Maithe, did you see? Two meters!", "Envergadura de pelo menos dois metros. Maithe, viu? Dois metros!"),
                ("He's dark like me, but I don't bite anybody. Usually.", "Ele é escuro que nem eu, mas eu não mordo ninguém. Geralmente."));
        }

        /// <summary>Lore + one comment per cousin (Maithe, Maria, Joaquim, Ícaro, Lavine, in that order).</summary>
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
