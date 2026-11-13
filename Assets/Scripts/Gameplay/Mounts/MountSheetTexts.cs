using System.Collections.Generic;
using System.Linq;
using Rapadura.Core.Localization;

namespace Rapadura.Gameplay.Mounts
{
    /// <summary>
    /// Localization (en + pt-BR) of the map-sheet mounts (<see cref="MountSheetCatalog"/>): description, lore, favourite lines,
    /// abilities, toasts and the whistle sold in the Copas de Garajonay. Merged into <see cref="MountTexts.Entries"/>; names and
    /// Caderno hints live in <see cref="MountCodex"/>.
    /// </summary>
    public static class MountSheetTexts
    {
        public const string ToastWade = "ui.mount.sheet.wade";
        public const string ToastHowl = "ui.mount.sheet.howl";
        public const string ToastHowlNone = "ui.mount.sheet.howl_none";
        public const string ToastBound = "ui.mount.sheet.bound";
        public const string ToastSpitHit = "ui.mount.sheet.spit_hit";
        public const string ToastSpitMiss = "ui.mount.sheet.spit_miss";
        public const string ToastStomp = "ui.mount.sheet.stomp";
        public const string ToastStorm = "ui.mount.sheet.storm";

        // id, en desc, pt desc, en lore, pt lore
        private static readonly (string Id, string DescEn, string DescPt, string LoreEn, string LorePt)[] Mounts =
        {
            (MountSheetCatalog.MarshHeron,
                "A grey heron of the Dark Mire Marsh, tall enough to carry a cousin. It wades the shallows as if they were dry ground and takes the group across with it.",
                "Uma garça-cinzenta do Brejo do Lodo Escuro, alta o bastante para levar um primo. Atravessa o raso como se fosse chão seco e leva o grupo junto.",
                "Dácil's herons fish the marsh at dawn and come back to the canopy before the mist lifts. Maithe tried to stand on one leg like hers. She lasted longer than the heron expected.",
                "As garças da Dácil pescam no brejo de madrugada e voltam para as copas antes de a névoa levantar. A Maithe tentou ficar num pé só igual a ela. Aguentou mais do que a garça esperava."),
            (MountSheetCatalog.OchreWarWolf,
                "A big wolf painted with the red ochre of the Red Drum Badlands. Its howl keeps time with the drums, and the critters around lose their nerve.",
                "Um lobo grande pintado com o ocre vermelho dos Ermos do Tambor Rubro. O uivo dele segue o ritmo dos tambores, e os bichos em volta perdem a coragem.",
                "The orcs paint their wolves before every hunt and wash them after, so no wolf carries an old fight into a new one. Ícaro asked to paint his. The wolf let him, and then licked half of it off.",
                "Os orcs pintam os lobos antes de cada caçada e lavam depois, para nenhum lobo levar uma briga velha para uma nova. O Ícaro pediu para pintar o dele. O lobo deixou, e depois lambeu metade da tinta."),
            (MountSheetCatalog.TallGrassEland,
                "A great eland of the Painted Shelters, spiral horns and a dewlap to the knees. Heavy as it looks, it clears the tall grass and the rocks in one leap.",
                "Um eland grande dos Abrigos Pintados, de chifres em espiral e papada até os joelhos. Pesado como parece, passa o capim alto e as pedras num salto só.",
                "The painters of the shelters drew the eland more than any other animal, always in the middle of a jump. The dwarves of the Bronze Halls keep a herd to carry ore down the Kaggen Range. Maithe counted the drawings: two hundred and twelve, and she wants to see it jump over every one.",
                "Os pintores dos abrigos desenharam o eland mais do que qualquer outro bicho, sempre no meio de um pulo. Os anões dos Salões de Bronze cuidam de uma manada para levar minério serra abaixo. A Maithe contou os desenhos: duzentos e doze, e quer ver o eland pular por cima de cada um."),
            (MountSheetCatalog.LanternOlm,
                "A giant blind salamander of the Threshold Lake, pale as a candle, with spots of cold light along its back. It never tires, and it spits light at whatever bothers it.",
                "Uma salamandra-cega gigante do Lago do Limiar, branca como vela, com pintas de luz fria pelas costas. Nunca cansa, e cospe luz em quem incomoda.",
                "The kobolds say the olms drank from the lake where a lamp of the first light sank, and kept a little of it under the skin. Farnkhund's olm pulls his carts when the rails go dark. Lavine says it cannot see her, so she always tells it where she is.",
                "Os kobolds dizem que os proteus beberam do lago onde afundou uma lamparina da primeira luz, e guardaram um pouco dela embaixo da pele. O proteu do Farnkhund puxa os vagonetes quando os trilhos ficam no escuro. A Lavine diz que ele não enxerga, então ela sempre conta para ele onde está."),
            (MountSheetCatalog.ArattaAurochs,
                "A black aurochs of the Aratta savanna, horns wide as a door and a pale stripe down its back. When it stomps, the ground answers.",
                "Um auroque preto da savana de Aratta, de chifres largos como uma porta e uma listra clara no lombo. Quando ele pisa forte, o chão responde.",
                "The bull folk of Aratta run the Course of Shulgi beside their aurochs and never on them: an aurochs carries only whom it chooses. It chose Joaquim on the first day, which he has not stopped mentioning.",
                "O povo-touro de Aratta corre o Percurso de Shulgi ao lado dos auroques e nunca em cima: auroque só leva quem ele escolhe. Ele escolheu o Joaquim no primeiro dia, e o Joaquim não para de comentar."),
            (MountSheetCatalog.WhiteBactrian,
                "A white two-humped camel of the Wakhan Pass, shaggy for the snow and patient for the sand. It turns its back to the storm, and the storm gives up first.",
                "Um camelo-bactriano branco do Passo de Wakhan, peludo para a neve e paciente para a areia. Vira as costas para a tempestade, e a tempestade desiste primeiro.",
                "The caravaneers of Sangkent say a white camel walks in front of every caravan that comes back. Kustai keeps its name on the notice board, the only notice he never had to copy again. Maria says it is a cousin of the stubborn caravan camels, and just as stubborn.",
                "Os caravaneiros de Sangkent dizem que um camelo branco anda na frente de toda caravana que volta. O Kustai guarda o nome dele no quadro de avisos, o único aviso que ele nunca precisou copiar de novo. A Maria diz que ele é parente dos camelos teimosos das caravanas, e igualzinho de teimoso."),
        };

        // ability id, en name, pt name, en desc, pt desc
        private static readonly (string Id, string NameEn, string NamePt, string DescEn, string DescPt)[] Abilities =
        {
            ("passo_de_pernalta", "Wader's Stride", "Passo de Pernalta",
                "For 10 s nothing slows the heron, and the cousins within 6 m lose their slows at once. Cooldown 30 s.",
                "Por 10 s nada deixa a garça lenta, e os primos a até 6 m perdem a lentidão na hora. Recarga 30 s."),
            ("uivo_do_tambor_rubro", "Red Drum Howl", "Uivo do Tambor Rubro",
                "Common critters within 8 m run away for 2 s and come back 25% slower for 3 s more. Bosses do not flinch. Cooldown 35 s.",
                "Bichos comuns a até 8 m fogem por 2 s e voltam 25% mais lentos por mais 3 s. Chefe nem pisca. Recarga 35 s."),
            ("salto_do_capim_alto", "Tall-Grass Leap", "Salto do Capim Alto",
                "One long leap 14 m ahead, over grass and rocks. Common critters within 6 m of the take-off lose sight of the cousin for 2 s. Cooldown 20 s.",
                "Um salto longo de 14 m para a frente, por cima de capim e pedra. Bichos comuns a até 6 m da largada perdem o primo de vista por 2 s. Recarga 20 s."),
            ("cuspe_de_luz", "Light Spit", "Cuspe de Luz",
                "Spits a glob of cold light at the nearest critter ahead within 10 m: everyone within 3 m of the splash hits 25% weaker for 4 s (bosses 2 s), and the olm glows 10 m around for 8 s. Cooldown 30 s.",
                "Cospe uma bola de luz fria no bicho mais perto à frente, a até 10 m: quem estiver a até 3 m do respingo bate 25% mais fraco por 4 s (chefe 2 s), e o proteu brilha 10 m em volta por 8 s. Recarga 30 s."),
            ("pisao_de_aratta", "Aratta Stomp", "Pisão de Aratta",
                "The aurochs stomps: critters within 6 m are 40% slower for 3 s (bosses 1.5 s), and the common ones are shoved 2 m back. Cooldown 25 s.",
                "O auroque pisa forte: bichos a até 6 m ficam 40% mais lentos por 3 s (chefe 1,5 s), e os comuns são empurrados 2 m para trás. Recarga 25 s."),
            ("couro_de_tempestade", "Storm Hide", "Couro de Tempestade",
                "The camel turns its back to the storm: the cousin's debuffs are shaken off and kept off for 10 s, nothing slows the cousin meanwhile, and the group within 6 m loses its slows. Cooldown 40 s.",
                "O camelo vira as costas para a tempestade: os efeitos ruins do primo saem e não voltam por 10 s, nada deixa o primo lento nesse tempo, e o grupo a até 6 m perde a lentidão. Recarga 40 s."),
        };

        private static readonly LocalizationEntry[] Fixed =
        {
            new LocalizationEntry("mount." + MountSheetCatalog.OchreWarWolf + ".favorite_line", "Next time I paint the other side.", "Da próxima vez eu pinto o outro lado."),
            new LocalizationEntry("mount." + MountSheetCatalog.TallGrassEland + ".favorite_line", "Two hundred and thirteen!", "Duzentos e treze!"),
            new LocalizationEntry("mount." + MountSheetCatalog.LanternOlm + ".favorite_line", "I'm right here, on your back.", "Estou aqui, nas suas costas."),
            new LocalizationEntry("mount." + MountSheetCatalog.ArattaAurochs + ".favorite_line", "Did everyone see? He chose me.", "Todo mundo viu? Ele me escolheu."),
            new LocalizationEntry(ToastWade, "{0} wades on: the shallows let go.", "{0} segue no raso: a água soltou."),
            new LocalizationEntry(ToastHowl, "{0} howls with the drums: the critters run!", "{0} uiva com os tambores: os bichos correm!"),
            new LocalizationEntry(ToastHowlNone, "{0} howls. Nobody near to hear it.", "{0} uiva. Ninguém perto para ouvir."),
            new LocalizationEntry(ToastBound, "{0} leaps over the tall grass.", "{0} salta por cima do capim alto."),
            new LocalizationEntry(ToastSpitHit, "{0} spits light: the critters are dazzled.", "{0} cospe luz: os bichos ficaram ofuscados."),
            new LocalizationEntry(ToastSpitMiss, "{0} spits light into the dark.", "{0} cospe luz no escuro."),
            new LocalizationEntry(ToastStomp, "{0} stomps: the ground answers.", "{0} pisa forte: o chão responde."),
            new LocalizationEntry(ToastStorm, "{0} turns its back to the storm.", "{0} vira as costas para a tempestade."),
        };

        // mount id, en name, pt name, en mount, pt mount
        private static readonly (string Id, string NameEn, string NamePt, string MountEn, string MountPt)[] Whistles =
        {
            (MountSheetCatalog.MarshHeron, "Marsh Heron Whistle", "Apito da Garça do Brejo", "the Marsh Heron", "a Garça do Brejo"),
        };

        public static readonly IReadOnlyList<LocalizationEntry> Entries = Build();

        private static List<LocalizationEntry> Build()
        {
            var entries = new List<LocalizationEntry>(Fixed);
            foreach (var m in Mounts)
            {
                entries.Add(new LocalizationEntry("mount." + m.Id + ".desc", m.DescEn, m.DescPt));
                entries.Add(new LocalizationEntry("mount." + m.Id + ".lore", m.LoreEn, m.LorePt));
            }

            foreach (var a in Abilities)
            {
                entries.Add(new LocalizationEntry("mount.ability." + a.Id + ".name", a.NameEn, a.NamePt));
                entries.Add(new LocalizationEntry("mount.ability." + a.Id + ".desc", a.DescEn, a.DescPt));
            }

            foreach (var w in Whistles)
            {
                string item = MountWaveCatalog.WhistleOf(w.Id);
                entries.Add(new LocalizationEntry(Content.ContentText.ItemNameKey(item), w.NameEn, w.NamePt));
                entries.Add(new LocalizationEntry(Content.ContentText.ItemDescriptionKey(item),
                    $"A reed whistle sold in the city. Whoever buys it takes {w.MountEn} home.",
                    $"Apito de junco vendido na cidade. Quem compra leva {w.MountPt} para casa."));
            }

            return entries.ToList();
        }
    }
}
