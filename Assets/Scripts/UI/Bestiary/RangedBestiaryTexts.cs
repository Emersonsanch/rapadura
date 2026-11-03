using System.Collections.Generic;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Enemies;

namespace Rapadura.UI.Bestiary
{
    /// <summary>
    /// "Caderno da Vó Edna" pages of the [distância] commons of the playable maps (Docs/Mapas100.md 07, 08, 13, 19): lore in
    /// Vó Edna's voice and one line per cousin, en + pt-BR. Kept apart like <see cref="NightBestiaryTexts"/>;
    /// <see cref="BestiaryTexts"/> appends these to its <see cref="BestiaryTexts.Entries"/>.
    /// </summary>
    public static class RangedBestiaryTexts
    {
        public static void AddPages(List<LocalizationEntry> entries)
        {
            // -- Areias Flamejantes (Deserto) --
            Page(entries, "enemy_desert_ember_vulture",
                "The Ember Vulture circles over the Caravan Road waiting for someone to get tired. It swallowed a coal from a dying campfire long ago and never let go of it, so now it spits embers from up high instead of coming down to fight. Grandma says vultures are the sweepers of the desert, good birds, and this one only forgot how to rest. When it stops in the air and spreads its wings, the ember is coming: step aside, then run underneath it, where it can't aim.",
                "O Abutre de Brasa fica rodando por cima da Estrada das Caravanas esperando alguém se cansar. Faz tempo ele engoliu um carvão de uma fogueira que se apagava e nunca mais soltou, então agora cospe brasa lá de cima em vez de descer pra brigar. A Vó diz que abutre é o varredor do deserto, bicho bom, e esse só esqueceu como se descansa. Quando ele para no ar e abre as asas, a brasa vem: pule de lado e corra pra baixo dele, onde ele não consegue mirar.",
                ("He aims from far away, like me. Too bad for him I aim better.", "Ele mira de longe, que nem eu. Azar dele que eu miro melhor."),
                ("Ember burns! Come here, let me see that arm.", "Brasa queima! Vem cá que eu vejo esse braço."),
                ("He spits, I block. He spits again, I block again. He gets tired first.", "Ele cospe, eu seguro. Cospe de novo, seguro de novo. Ele cansa primeiro."),
                ("He keeps a coal in his throat and doesn't burn. I want to know how.", "Ele guarda um carvão na garganta e não se queima. Eu quero saber como."),
                ("His head is bald and pink. I'm not laughing. Okay, I'm laughing.", "A cabeça dele é careca e rosa. Não tô rindo. Tá, tô rindo."));

            Page(entries, "enemy_desert_broken_lantern",
                "Long ago the Artificers made lanterns that walked by themselves, to light the way between the pyramids. When the shadow came, it trapped a little light inside one of them and cracked its glass, and now the lantern floats among the ruins shooting that trapped light at anyone who comes close, as if it were defending a road nobody uses anymore. It's an engine, not a creature: it doesn't flee and it doesn't get tired. Watch the core: when it shines brighter, the bolt is coming. Break it gently and the light goes home.",
                "Há muito tempo os Artífices fizeram lanternas que andavam sozinhas, pra iluminar o caminho entre as pirâmides. Quando a sombra chegou, prendeu um pouquinho de luz dentro de uma delas e rachou o vidro, e agora a lanterna flutua pelas ruínas atirando essa luz presa em quem chega perto, como se defendesse uma estrada que ninguém usa mais. É engenho, não bicho: não foge e não cansa. Olhem o miolo: quando ele brilhar mais forte, o tiro vem. Quebrem com jeito e a luz volta pra casa.",
                ("Its light gives away the shot. I'll wait for it to blink.", "A luz dela entrega o tiro. Eu espero ela piscar."),
                ("Poor thing, it's still trying to light a road. Let's free the light.", "Coitada, ainda tá tentando iluminar uma estrada. Vamos soltar essa luz."),
                ("The bolt bends toward you. So I stand in front of everyone.", "O tiro dela faz curva atrás da gente. Então eu fico na frente de todo mundo."),
                ("Gears, runes, a cracked pane... Can I take it apart afterwards? Please?", "Engrenagem, runa, vidro rachado... Posso desmontar depois? Por favor?"),
                ("It sways like it's sleepy. I swayed back. It shot me.", "Ela balança que nem quem tá com sono. Eu balancei também. Ela atirou em mim."));

            // -- Serra de Kaggen (Montanha) --
            Page(entries, "enemy_mountain_crested_hawk",
                "The Crested Hawk lives on the windy ledges of the Kaggen Range, with a little crown of dark feathers that stands up when it's angry. It's a dimmed one: instead of hunting, it pulls out its own hard flight feathers and throws them like darts from the top of the rocks. It's quick and sees everything from up there, but its feathers fly straight, never in curves. Move sideways and keep going: it only throws again after it sits up to take aim.",
                "O Gavião-de-Penacho mora nas beiradas de vento da Serra de Kaggen, com uma coroinha de penas escuras que fica em pé quando ele se zanga. Esse é dos apagados: em vez de caçar, arranca as próprias penas duras de voo e atira feito dardo lá de cima das pedras. É ligeiro e vê tudo do alto, mas a pena dele voa reta, nunca faz curva. Andem de lado e não parem: ele só atira de novo depois de se ajeitar pra mirar.",
                ("A rival! His feathers against my arrows. Let's see.", "Um rival! As penas dele contra as minhas flechas. Vamos ver."),
                ("He's pulling out his own feathers... That has to hurt. Let's help him stop.", "Ele tá arrancando as próprias penas... Isso deve doer. Vamos ajudar ele a parar."),
                ("A feather got stuck in my shield. I'm keeping it.", "Uma pena ficou presa no meu escudo. Vou guardar."),
                ("Straight trajectory, no wind correction. Predictable. I like predictable.", "Trajetória reta, sem correção de vento. Previsível. Eu gosto de previsível."),
                ("His crest stands up when he's angry. Mine does too, Maria says.", "O penacho dele fica em pé quando ele se zanga. O meu também, diz a Maria."));

            // -- Grutas do Limiar (Cavernas) --
            Page(entries, "enemy_cave_lantern_fish",
                "In the dark lake of the Threshold lives the Lanternfish, a big-mouthed fish that learned to swim in the cave air. On its forehead it carries a little crystal light on a stalk, like a fishing rod, and it uses that light to fool the lost: it flicks drops of cold light that chase you a little before falling. It was dimmed when the shadow drank the crystals. It's slow, so don't run away from it: walk around it and hit it from the side, away from that mouth.",
                "No lago escuro do Limiar mora o Peixe-Lanterna, um peixe bocudo que aprendeu a nadar no ar da gruta. Na testa ele carrega uma luzinha de cristal na ponta de uma vareta, feito vara de pescar, e usa essa luz pra enganar quem se perde: joga gotas de luz fria que perseguem a gente um pouquinho antes de cair. Ele se apagou quando a sombra bebeu os cristais. É lento, então não fujam dele: deem a volta e batam de lado, longe daquela bocarra.",
                ("The light on its head is the perfect target. Thank you, fish.", "A luz na cabeça dele é o alvo perfeito. Obrigada, peixe."),
                ("Those drops are cold! Stay close, I'll warm your hands after.", "Essas gotas são geladas! Fiquem perto, depois eu esquento as mãos de vocês."),
                ("A fish. In the air. Nobody told me that was allowed.", "Um peixe. No ar. Ninguém me avisou que isso podia."),
                ("Bioluminescence! Maithe, did you see? Real bioluminescence!", "Bioluminescência! Maithe, viu? Bioluminescência de verdade!"),
                ("It has more teeth than I have fingers. I counted. Twice.", "Ele tem mais dente do que eu tenho dedo. Eu contei. Duas vezes."));
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
