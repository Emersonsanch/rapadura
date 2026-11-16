using System.Collections.Generic;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Enemies;

namespace Rapadura.UI.Bestiary
{
    /// <summary>
    /// "Caderno da Vó Edna" pages of the roles of Regions 2 and 3 (Docs/Mapas100.md 09, 12, 15 and 17) that the continuous world still played
    /// with another beast after the twenty-third pass (bible §3.2.1, twenty-sixth pass): lore in Grandma Edna's voice and one line per cousin,
    /// en + pt-BR. Kept apart like <see cref="EarlyOwnBestiaryTexts"/>; <see cref="BestiaryTexts"/> appends these to its
    /// <see cref="BestiaryTexts.Entries"/>. Never gore (bible §4.3).
    /// </summary>
    public static class EarlyOwn2BestiaryTexts
    {
        /// <summary>The asset ids these pages are for.</summary>
        public static readonly string[] EnemyIds =
        {
            "enemy_desert_thirsty_root", "enemy_desert_crystal_scorpion", "enemy_desert_salt_serpent", "enemy_mountain_dimmed_black_eagle",
            "enemy_cave_cold_salamander",
        };

        public static void AddPages(List<LocalizationEntry> entries)
        {
            // -- Galerias do Karez (09) --
            Page(entries, "enemy_desert_thirsty_root",
                "The vines of the oasis sent their roots down the shafts of the karez to find the water. When the Root that Drank the Well went dark, a knot of them broke off and now goes along the channels on its own tendrils, drinking every puddle it finds. Where it has passed the mud is dry and cracked. It holds whoever steps too close; it hates the sunbeams, so fight it in the light.",
                "As parreiras do oásis mandaram as raízes pelos poços do karez atrás da água. Quando a Raiz que Bebeu o Poço se apagou, um nó delas se soltou e agora anda pelos canais nas próprias gavinhas, bebendo toda poça que acha. Onde ela passou a lama fica seca e rachada. Segura quem chega perto demais; odeia os fachos de sol, então lutem com ela na luz.",
                ("It crouches on its roots before it grabs. Jump back then.", "Ela se agacha nas raízes antes de agarrar. Pula pra trás nessa hora."),
                ("It's only thirsty. Everything down here is thirsty.", "Ela só tem sede. Tudo aqui embaixo tem sede."),
                ("A plant stealing water from MY water. Not today.", "Uma planta roubando água da MINHA água. Hoje não."),
                ("The drop inside it is the only light it has.", "A gota dentro dela é a única luz que ela tem."),
                ("It has little leaves on its head! Dry ones!", "Ela tem folhinhas na cabeça! Secas!"));

            // -- Trono de Namrael (12) --
            Page(entries, "enemy_desert_crystal_scorpion",
                "On Namrael's salt the red crystal grows on everything that keeps still. These scorpions keep still all day under the crust, and the crystal grew on them too, plate by plate, until their sting became a clear point. They are slow and very hard, and the plates ring when you hit them. Hit where the crystal is thin: under the tail and at the joints of the legs.",
                "No sal de Namrael o cristal vermelho cresce em tudo o que fica parado. Esses escorpiões ficam parados o dia todo debaixo da crosta, e o cristal cresceu neles também, placa por placa, até o ferrão virar uma ponta transparente. São lentos e muito duros, e as placas tocam que nem sino quando a gente bate. Batam onde o cristal é fino: debaixo do rabo e nas juntas das pernas.",
                ("The mouth opens before the sting. Step aside.", "A boca abre antes da ferroada. Sai de lado."),
                ("It got so hard because it stayed still so long. Like people.", "Ficou tão duro porque ficou parado tempo demais. Que nem gente."),
                ("It rings when I hit it! I'm a musician now.", "Ele toca quando eu bato! Agora sou músico."),
                ("The crystal grew on it because it never moved. A warning.", "O cristal cresceu nele porque ele nunca se mexeu. Um aviso."),
                ("It's like a ruby with legs! Can I keep a little piece?", "Parece um rubi com perna! Posso ficar com um pedacinho?"));

            Page(entries, "enemy_desert_salt_serpent",
                "Under the crust of Namrael's plain the salt serpents swim as if it were sand, and they come up where the crust is thin to strike at feet. They are the colour of the salt, so at noon, when the salt blinds you, you only see the red crystals down their backs. They are quick and they go back under fast. Stand on the thick crust, near the towers, and they have to come out to reach you.",
                "Debaixo da crosta da planície de Namrael as serpentes de sal nadam como se fosse areia, e sobem onde a crosta é fina pra dar o bote nos pés. São da cor do sal, então ao meio-dia, quando o sal cega, vocês só veem os cristais vermelhos nas costas delas. São rápidas e voltam pra baixo depressa. Fiquem na crosta grossa, perto das torres, que elas têm que sair pra alcançar vocês.",
                ("Its jaw drops before the bite. Jump.", "A boca abre antes da mordida. Pula."),
                ("It hides in the salt because the sun hurts it too.", "Ela se esconde no sal porque o sol machuca ela também."),
                ("Snake in the floor. Floor snake. I hate this place.", "Cobra no chão. Cobra de chão. Odeio esse lugar."),
                ("Follow the red, not the white. The white lies.", "Sigam o vermelho, não o branco. O branco engana."),
                ("It's white like sugar! Is it sweet?", "Ela é branca que nem açúcar! Ela é doce?"));

            // -- Abrigos Pintados (15) --
            Page(entries, "enemy_mountain_dimmed_black_eagle",
                "The black eagles nest on the ledges above the painted shelters and hunt the hyraxes on the scree; you know them by the white V on their backs. The Borrão ate the colour off the walls and the eagles went dull with it. They still throw their hard flight feathers like darts at whoever climbs up to the paintings. Keep close to the wall, under the overhang, where they can't dive.",
                "As águias-pretas fazem ninho nas saliências acima dos abrigos pintados e caçam os hírax no cascalho; dá pra conhecer pelo V branco nas costas. O Borrão comeu as cores das paredes e as águias ficaram foscas junto. Ainda jogam as penas duras que nem dardo em quem sobe até as pinturas. Fiquem colados na parede, debaixo da saliência, onde elas não conseguem mergulhar.",
                ("It stops flapping before it throws. Get behind a rock.", "Ela para de bater asa antes de jogar. Se esconde atrás de uma pedra."),
                ("It lost its colours with the paintings. Let's give them back.", "Ela perdeu as cores junto com as pinturas. Vamos devolver."),
                ("It throws feathers. Feathers! How does that even hurt?", "Ela joga pena. Pena! Como isso machuca?"),
                ("The V on its back points down at the paintings. Interesting.", "O V nas costas dela aponta pras pinturas. Interessante."),
                ("It has yellow feet! Like a duck! A big angry duck!", "Ela tem pé amarelo! Que nem pato! Um pato grande e bravo!"));

            // -- Salões de Bronze de Napirasu (17) --
            Page(entries, "enemy_cave_cold_salamander",
                "The dwarves kept salamanders in the Grande Forja: they lived in the coals, and the fire never went out while they did. When the Bellows went dark the salamanders went cold, their yellow spots turned to ash, and now they run through the halls looking for a fire to lie in. One spot on each still glows. They are fast and they bite; light a brazier and they forget you to go and lie in it.",
                "Os anões criavam salamandras na Grande Forja: elas moravam nas brasas, e o fogo nunca apagava enquanto elas estavam lá. Quando o Fole se apagou as salamandras esfriaram, as manchas amarelas viraram cinza, e agora correm pelos salões procurando um fogo pra deitar. Uma mancha em cada uma ainda brilha. São rápidas e mordem; acendam um braseiro que elas esquecem vocês e vão deitar nele.",
                ("It opens its mouth before it bites. Watch the head.", "Ela abre a boca antes de morder. Olha a cabeça."),
                ("It's cold. It only wants to be warm again.", "Ela tá com frio. Só quer ficar quentinha de novo."),
                ("A fire lizard with no fire. That's just a lizard. A sad lizard.", "Lagarto de fogo sem fogo. Isso é só um lagarto. Um lagarto triste."),
                ("One spot still burns. It's waiting for the forge.", "Uma mancha ainda queima. Ela tá esperando a forja."),
                ("It's smoking like Grandpa Jorge's pipe!", "Ela solta fumaça que nem o cachimbo do Vô Jorge!"));
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
