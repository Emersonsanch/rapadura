using System.Collections.Generic;
using System.Linq;
using Rapadura.Core.Localization;

namespace Rapadura.Gameplay.Mounts
{
    /// <summary>
    /// Localization (en + pt-BR) of <see cref="MountBossCatalog"/>: description, lore, favourite lines, abilities, toasts and the two
    /// whistles sold in the Copas de Garajonay and in Jhilpur. Merged into <see cref="MountTexts.Entries"/>; names and Caderno hints
    /// live in <see cref="MountCodex"/>.
    /// </summary>
    public static class MountBossTexts
    {
        public const string ToastTail = "ui.mount.boss.tail";
        public const string ToastTailNone = "ui.mount.boss.tail_none";
        public const string ToastMud = "ui.mount.boss.mud";
        public const string ToastWatch = "ui.mount.boss.watch";
        public const string ToastSmoke = "ui.mount.boss.smoke";
        public const string ToastDive = "ui.mount.boss.dive";
        public const string ToastDiveSeen = "ui.mount.boss.dive_seen";
        public const string ToastForge = "ui.mount.boss.forge";

        // id, en desc, pt desc, en lore, pt lore
        private static readonly (string Id, string DescEn, string DescPt, string LoreEn, string LorePt)[] Mounts =
        {
            (MountBossCatalog.LaurelLizard,
                "A giant lizard of the laurel forest of Garajonay, brown with green flecks, long as a bench. Quick on the ground, and when something grabs it, it leaves the tail behind.",
                "Um lagarto gigante da floresta de louro de Garajonay, marrom de pintas verdes, comprido como um banco. Rápido no chão, e quando alguém agarra, deixa o rabo para trás.",
                "The forest elves say the lizards were on the islands before the laurels and will be there after them. Dácil keeps three on her counter, warming in the sun. Maithe was sad about the tail until Dácil showed her it grows back, a little shorter every time.",
                "Os elfos-da-mata dizem que os lagartos já estavam nas ilhas antes dos louros e vão estar depois deles. A Dácil guarda três no balcão, esquentando no sol. A Maithe ficou triste pelo rabo até a Dácil mostrar que ele cresce de novo, um pouco mais curto a cada vez."),
            (MountBossCatalog.JhilpurBuffalo,
                "A grey water buffalo of the rice fields of Jhilpur, with horns swept back like a boat's ribs. It swims the canals and wallows in the mud, and whoever rides it comes out fresh.",
                "Um búfalo-d'água cinzento dos arrozais de Jhilpur, de chifres para trás como as costelas de um barco. Nada nos canais e chafurda na lama, e quem vai em cima sai fresquinho.",
                "Nuhzadag says a buffalo understands every tongue of the delta and answers none of them. In Jhilpur the buffaloes pull the boats up the shallow canals when the water is low. Joaquim tried to pull one with it. The buffalo pulled both.",
                "O Nuhzadag diz que o búfalo entende todas as línguas do delta e não responde em nenhuma. Em Jhilpur os búfalos puxam os barcos pelos canais rasos quando a água baixa. O Joaquim tentou puxar um junto com ele. O búfalo puxou os dois."),
            (MountBossCatalog.PamirArgali,
                "A great wild sheep of the Wakhan Pass, horns curled in a full turn and a pale ruff on the chest. Aydahar kept the herd pinned on the ledges; freed, the old ram watches the pass for the cousins.",
                "Um carneiro-selvagem grande do Passo de Wakhan, de chifres enrolados numa volta inteira e uma gola clara no peito. O Aydahar prendia a manada nas escarpas; livre, o carneiro velho vigia o passo para os primos.",
                "While the dragon lay coiled round the pass, the argali could not come down to the grass, and they waited on the ledges, watching. When Aydahar fell, the herd came down in a line. The oldest ram stayed. Maria says it is still watching, just in case.",
                "Enquanto o dragão ficava enrolado no passo, os argalis não conseguiam descer para o capim, e esperavam nas escarpas, olhando. Quando o Aydahar caiu, a manada desceu em fila. O carneiro mais velho ficou. A Maria diz que ele continua vigiando, só por garantia."),
            (MountBossCatalog.FreedSmokeHorse,
                "The Smoke Horse of Soslan's racetracks, freed: a grey horse now, of flesh and mane, that still trails a little smoke when it runs. Whatever stands in that smoke forgets what it was chasing.",
                "O Cavalo de Fumaça das pistas de Soslan, libertado: agora um cavalo cinzento, de carne e crina, que ainda solta um pouco de fumaça quando corre. Quem fica nessa fumaça esquece o que estava perseguindo.",
                "The shadow had made a stallion of smoke to lead the foals away from the herd. When the cousins beat it, the smoke thinned and a grey horse was left standing, as surprised as anyone. The foals came home behind it. Maria brushed it for an hour; the smoke still comes out of the brush.",
                "A sombra tinha feito um garanhão de fumaça para levar os potros para longe da manada. Quando os primos o venceram, a fumaça afinou e sobrou um cavalo cinzento de pé, tão surpreso quanto todo mundo. Os potros voltaram atrás dele. A Maria escovou o cavalo por uma hora; a escova ainda solta fumaça."),
            (MountBossCatalog.TameNinkiNanka,
                "Ninki Nanka, the dragoness of the bog, freed from the dimness and still very shy: long and scaled, with a crest of reeds. When anyone stares, she shines and dives, and takes her rider under with her.",
                "A Ninki Nanka, a dragoa do charco, libertada do apagamento e ainda muito tímida: comprida e de escamas, com uma crista de junco. Quando alguém encara, ela brilha e mergulha, e leva quem vai em cima junto.",
                "Once freed she left a scale on the boards and slipped into the water. Very rarely she comes back, and then she follows whoever did not stare at her. Lavine never looks at her straight: she talks to her with her back turned, and the dragoness listens.",
                "Depois de libertada ela deixou uma escama nas tábuas e sumiu na água. Muito de vez em quando ela volta, e aí segue quem não ficou encarando. A Lavine nunca olha direto para ela: conversa de costas, e a dragoa escuta."),
            (MountBossCatalog.LivingEmberBoar,
                "A black boar of the Hinokura forges with living embers in its bristles. The Ember Glutton swallowed it with the fire of the forges; beaten, it gave both back. Where it snorts, the blades glow.",
                "Um javali preto das forjas de Hinokura com brasas vivas nas cerdas. O Glutão de Brasa o engoliu junto com o fogo das forjas; vencido, devolveu os dois. Onde ele bufa, as lâminas brilham.",
                "The smiths of Hinokura say the boar guards the forge from the fire that devours, and keeps the fire that lives. That is why the Glutton swallowed it first. Ícaro tried to light his lamp on its back. The boar let him, and then sneezed the lamp out.",
                "Os ferreiros de Hinokura dizem que o javali guarda a forja do fogo que devora, e guarda o fogo que vive. Por isso o Glutão o engoliu primeiro. O Ícaro tentou acender a lamparina nas costas dele. O javali deixou, e depois espirrou a lamparina para longe."),
        };

        // ability id, en name, pt name, en desc, pt desc
        private static readonly (string Id, string NameEn, string NamePt, string DescEn, string DescPt)[] Abilities =
        {
            ("rabo_solto", "Loose Tail", "Rabo Solto",
                "Drops its wriggling tail behind it: common critters within 8 m forget the cousin for 3 s and are drawn up to 3 m toward the tail. Bosses are not fooled. Cooldown 25 s.",
                "Solta o rabo, ainda mexendo, logo atrás: bichos comuns a até 8 m esquecem o primo por 3 s e são puxados até 3 m na direção do rabo. Chefe não cai nessa. Recarga 25 s."),
            ("banho_de_lama", "Mud Wallow", "Banho de Lama",
                "The buffalo rolls in the cool mud and splashes the group: for 8 s the cousin and the group within 6 m get their skills back 15% sooner. Cooldown 35 s.",
                "O búfalo chafurda na lama fresca e espirra no grupo: por 8 s o primo e o grupo a até 6 m recarregam as habilidades 15% mais rápido. Recarga 35 s."),
            ("vigia_da_escarpa", "Ledge Watch", "Vigia da Escarpa",
                "The argali watches from above and shows where to strike: for 8 s the cousin and the group within 8 m get +10% critical chance. Cooldown 35 s.",
                "O argali vigia do alto e mostra onde bater: por 8 s o primo e o grupo a até 8 m ganham +10% de chance de crítico. Recarga 35 s."),
            ("nuvem_de_fumaca", "Smoke Cloud", "Nuvem de Fumaça",
                "Leaves the last of its smoke where it stands, 5 m around, for 5 s: common critters inside forget every target and stop attacking. Bosses walk through. Cooldown 35 s.",
                "Deixa o resto da fumaça onde está, 5 m em volta, por 5 s: bichos comuns dentro esquecem todo alvo e param de atacar. Chefe atravessa. Recarga 35 s."),
            ("mergulho_timido", "Shy Dive", "Mergulho Tímido",
                "She shines and dives: critters within 12 m looking at her are 40% slower for 3 s (bosses 1.5 s), the common ones lose the cousin, and both stay out of sight for 4 s. Cooldown 45 s.",
                "Ela brilha e mergulha: bichos a até 12 m que estão olhando para ela ficam 40% mais lentos por 3 s (chefe 1,5 s), os comuns perdem o primo, e os dois somem de vista por 4 s. Recarga 45 s."),
            ("forja_acesa", "Lit Forge", "Forja Acesa",
                "The boar snorts on its embers: for 10 s the cousin and the group within 8 m deal 30% more critical damage, and the boar lights up 10 m around. Cooldown 50 s.",
                "O javali bufa nas brasas: por 10 s o primo e o grupo a até 8 m dão 30% mais dano crítico, e o javali clareia 10 m em volta. Recarga 50 s."),
        };

        private static readonly LocalizationEntry[] Fixed =
        {
            new LocalizationEntry("mount." + MountBossCatalog.JhilpurBuffalo + ".favorite_line", "Fine, you pull. I'll ride.", "Tá bom, você puxa. Eu vou em cima."),
            new LocalizationEntry("mount." + MountBossCatalog.FreedSmokeHorse + ".favorite_line", "Still smoking? I'll brush you again.", "Ainda soltando fumaça? Eu escovo de novo."),
            new LocalizationEntry("mount." + MountBossCatalog.TameNinkiNanka + ".favorite_line", "I'm not looking. Promise.", "Não estou olhando. Prometo."),
            new LocalizationEntry("mount." + MountBossCatalog.LivingEmberBoar + ".favorite_line", "Can I light just one more lamp?", "Posso acender só mais uma lamparina?"),
            new LocalizationEntry(ToastTail, "{0} drops its tail: the critters go after it!", "{0} solta o rabo: os bichos vão atrás dele!"),
            new LocalizationEntry(ToastTailNone, "{0} drops its tail. Nobody near to fool.", "{0} solta o rabo. Ninguém perto para enganar."),
            new LocalizationEntry(ToastMud, "{0} wallows: cool mud for everyone.", "{0} chafurda: lama fresca para todo mundo."),
            new LocalizationEntry(ToastWatch, "{0} watches from above.", "{0} vigia do alto."),
            new LocalizationEntry(ToastSmoke, "{0} leaves its smoke behind.", "{0} deixa a fumaça para trás."),
            new LocalizationEntry(ToastDive, "{0} dives out of sight.", "{0} mergulha e some de vista."),
            new LocalizationEntry(ToastDiveSeen, "{0} shines and dives: whoever stared is slowed.", "{0} brilha e mergulha: quem encarou ficou lento."),
            new LocalizationEntry(ToastForge, "{0} snorts on its embers: the blades glow.", "{0} bufa nas brasas: as lâminas brilham."),

            // The freeing scene (MountBossFreeing): said over the spot where the boss fell, when its mount drops.
            new LocalizationEntry(MountBossRules.FreedLineKey(MountBossCatalog.PamirArgali),
                "Aydahar uncoils from the ledges: {0} comes down from the cliff and stays with you.",
                "O Aydahar solta as escarpas: {0} desce do penhasco e fica com vocês."),
            new LocalizationEntry(MountBossRules.FreedLineKey(MountBossCatalog.FreedSmokeHorse),
                "The smoke thins away and a grey horse is left standing: {0} is free.",
                "A fumaça afina e sobra um cavalo cinzento de pé: {0} está livre."),
            new LocalizationEntry(MountBossRules.FreedLineKey(MountBossCatalog.TameNinkiNanka),
                "The dimness leaves her scales: {0} lifts her head from the water. Don't stare.",
                "O apagamento sai das escamas: {0} levanta a cabeça da água. Não encare."),
            new LocalizationEntry(MountBossRules.FreedLineKey(MountBossCatalog.LivingEmberBoar),
                "The Glutton gives the fire back, and with it {0}, embers still alive.",
                "O Glutão devolve o fogo, e junto {0}, com as brasas ainda vivas."),
        };

        // mount id, en name, pt name, en mount, pt mount
        private static readonly (string Id, string NameEn, string NamePt, string MountEn, string MountPt)[] Whistles =
        {
            (MountBossCatalog.LaurelLizard, "Laurel Lizard Whistle", "Apito do Lagarto-do-Louro", "the Laurel Lizard", "o Lagarto-do-Louro"),
            (MountBossCatalog.JhilpurBuffalo, "Jhilpur Water Buffalo Whistle", "Apito do Búfalo-d'Água de Jhilpur", "the Jhilpur Water Buffalo", "o Búfalo-d'Água de Jhilpur"),
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
