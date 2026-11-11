using System.Collections.Generic;
using System.Linq;
using Rapadura.Core.Localization;

namespace Rapadura.Gameplay.Mounts
{
    /// <summary>
    /// Localization (en + pt-BR) of the Regions 5–9 mounts (<see cref="MountMidRegionCatalog"/>): description, lore, favourite
    /// lines, abilities, toasts and the two whistles sold in the cities. Merged into <see cref="MountTexts.Entries"/>; names and
    /// Caderno hints live in <see cref="MountCodex"/>.
    /// </summary>
    public static class MountMidRegionTexts
    {
        public const string ToastSail = "ui.mount.mid.sail";
        public const string ToastLeap = "ui.mount.mid.leap";
        public const string ToastLeapSplash = "ui.mount.mid.leap_splash";
        public const string ToastFleece = "ui.mount.mid.fleece";
        public const string ToastLightTrail = "ui.mount.mid.light_trail";
        public const string ToastBreath = "ui.mount.mid.breath";

        // id, en desc, pt desc, en lore, pt lore
        private static readonly (string Id, string DescEn, string DescPt, string LoreEn, string LorePt)[] Mounts =
        {
            (MountMidRegionCatalog.DhowTurtle,
                "A great green sea turtle of the Monsoon Coast with a dhow's sail on its shell. It crosses the reef at high tide and runs with the wind.",
                "Uma grande tartaruga-verde da Costa das Monções com uma vela de dhow no casco. Cruza o recife na maré alta e corre a favor do vento.",
                "The fishermen of Taa Pwani say the first dhow was copied from a turtle that let a sail grow on its back. Joaquim tried to steer it with a rope. It went where the wind said, and he pretended that was the plan.",
                "Os pescadores de Taa Pwani dizem que o primeiro dhow foi copiado de uma tartaruga que deixou crescer uma vela nas costas. O Joaquim tentou guiar com uma corda. Ela foi para onde o vento mandou, e ele fingiu que era esse o plano."),
            (MountMidRegionCatalog.TarchonFrog,
                "A giant frog of the Tarchon stilts. It floats over mud and reeds and leaps far, splashing whatever is waiting on the other side.",
                "Um sapo gigante das palafitas de Tarchon. Boia por cima de lama e junco e pula longe, espirrando lama em quem estiver esperando do outro lado.",
                "The trols ride them from stilt to stilt when the walkways rot. Maithe says it smells of pond. Ícaro says that is the best part, and wants to know how far it jumps with two cousins on top.",
                "Os trols pulam de palafita em palafita neles quando as passarelas apodrecem. A Maithe diz que ele tem cheiro de lagoa. O Ícaro diz que essa é a melhor parte, e quer saber até onde ele pula com dois primos em cima."),
            (MountMidRegionCatalog.RoofYak,
                "A shaggy yak of the Roof of the World. Slow and stubborn, it carries a whole caravan's load and its wool keeps the cold off the group.",
                "Um iaque peludo do Teto do Mundo. Lento e teimoso, carrega a carga de uma caravana inteira, e a lã dele tira o frio do grupo.",
                "The caravans of Sangkent cross the passes behind them, bell by bell. Vô Jorge used to say a yak never hurries and never gets lost. Maria thinks that is exactly what he said about himself.",
                "As caravanas de Sangkent cruzam os passos atrás deles, sino por sino. O Vô Jorge dizia que iaque nunca tem pressa e nunca se perde. A Maria acha que era exatamente o que ele dizia de si mesmo."),
            (MountMidRegionCatalog.WhiteLightStag,
                "A stag of white light from the White Spires of Amanirenas. Where it steps the ground keeps shining a little, and the light heals.",
                "Um cervo de luz branca das Torres Brancas de Amanirenas. Onde ele pisa o chão fica brilhando um pouco, e a luz cura.",
                "The high elves say it was a spark that refused to go back up the Column until the last lamp of the plain was lit. Lavine walks in its trail on purpose, just to watch her feet glow.",
                "Os altos-elfos dizem que ele foi uma faísca que não quis subir pela Coluna antes de acender a última lamparina da planície. A Lavine anda no rastro dele de propósito, só para ver os pés brilhando."),
            (MountMidRegionCatalog.GoldenSteppeHorse,
                "A golden horse of the Nartes steppe, its coat shining like metal. It runs further than any other, and its breath comes back in a blink.",
                "Um cavalo dourado da estepe dos Nartes, de pelo que brilha como metal. Corre mais longe que qualquer outro, e o fôlego dele volta num piscar.",
                "Soslan's centaurs race them across the prairie and lose on purpose, out of courtesy. Maria never loses on purpose. The horse seems to approve.",
                "Os centauros do Soslan apostam corrida com eles pela pradaria e perdem de propósito, por educação. A Maria nunca perde de propósito. O cavalo parece aprovar."),
        };

        // ability id, en name, pt name, en desc, pt desc
        private static readonly (string Id, string NameEn, string NamePt, string DescEn, string DescPt)[] Abilities =
        {
            ("vela_da_moncao", "Monsoon Sail", "Vela da Monção",
                "The sail fills: for 8 s the turtle gallops without tiring, and the gust shoves common critters within 6 m back 4 m. Bosses hold their ground. Cooldown 40 s.",
                "A vela enche: por 8 s a tartaruga galopa sem cansar, e a rajada empurra 4 m os bichos comuns a até 6 m. Chefe não sai do lugar. Recarga 40 s."),
            ("salto_do_charco", "Bog Leap", "Salto do Charco",
                "Leaps 10 m ahead over water and mud. The splash slows critters within 4 m of the landing by 30% for 3 s (bosses 1.5 s). Cooldown 16 s.",
                "Pula 10 m para a frente por cima de água e lama. O espirro deixa os bichos a até 4 m do pouso 30% mais lentos por 3 s (chefe 1,5 s). Recarga 16 s."),
            ("la_do_teto", "Roof Wool", "Lã do Teto",
                "The yak shakes its wool: the cousin and the group within 6 m lose their slows and get +15% defense for 8 s. Cooldown 35 s.",
                "O iaque sacode a lã: o primo e o grupo a até 6 m perdem a lentidão e ganham +15% de defesa por 8 s. Recarga 35 s."),
            ("rastro_de_luz_branca", "White Light Trail", "Rastro de Luz Branca",
                "For 8 s the stag leaves a trail of white light and lights up 12 m around. Whoever of the group stands on the trail heals 2% of max life per second. Cooldown 40 s.",
                "Por 8 s o cervo deixa um rastro de luz branca e ilumina 12 m em volta. Quem do grupo pisa no rastro cura 2% da vida máxima por segundo. Recarga 40 s."),
            ("folego_de_soslan", "Soslan's Breath", "Fôlego de Soslan",
                "The horse gets all its stamina back at once, and the cousins within 8 m recover 30% of theirs. Cooldown 45 s.",
                "O cavalo recupera todo o fôlego na hora, e os primos a até 8 m recuperam 30% do deles. Recarga 45 s."),
        };

        private static readonly LocalizationEntry[] Fixed =
        {
            new LocalizationEntry("mount." + MountMidRegionCatalog.DhowTurtle + ".favorite_line", "See? Exactly where I wanted to go.", "Viu? Exatamente para onde eu queria ir."),
            new LocalizationEntry("mount." + MountMidRegionCatalog.WhiteLightStag + ".favorite_line", "Look, my feet are glowing again!", "Olha, meus pés estão brilhando de novo!"),
            new LocalizationEntry("mount." + MountMidRegionCatalog.GoldenSteppeHorse + ".favorite_line", "Again. And this time nobody lets me win.", "De novo. E dessa vez ninguém me deixa ganhar."),
            new LocalizationEntry(ToastSail, "{0} raises the sail: the monsoon pushes!", "{0} levanta a vela: a monção empurra!"),
            new LocalizationEntry(ToastLeap, "{0} leaps over the mud.", "{0} pula por cima da lama."),
            new LocalizationEntry(ToastLeapSplash, "Splash! Mud on every critter.", "Chuá! Lama em todo bicho."),
            new LocalizationEntry(ToastFleece, "{0} shakes its wool: the cold let go.", "{0} sacode a lã: o frio soltou."),
            new LocalizationEntry(ToastLightTrail, "{0} leaves a trail of white light.", "{0} deixa um rastro de luz branca."),
            new LocalizationEntry(ToastBreath, "{0} snorts: everyone got their breath back.", "{0} bufa: todo mundo recuperou o fôlego."),
        };

        // mount id, en name, pt name, en mount, pt mount
        private static readonly (string Id, string NameEn, string NamePt, string MountEn, string MountPt)[] Whistles =
        {
            (MountMidRegionCatalog.TarchonFrog, "Tarchon Giant Frog Whistle", "Apito do Sapo-Gigante de Tarchon", "the Tarchon Giant Frog", "o Sapo-Gigante de Tarchon"),
            (MountMidRegionCatalog.RoofYak, "Roof-of-the-World Yak Whistle", "Apito do Iaque do Teto do Mundo", "the Roof-of-the-World Yak", "o Iaque do Teto do Mundo"),
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
