using System.Collections.Generic;
using System.Linq;
using Rapadura.Core.Localization;

namespace Rapadura.Gameplay.Mounts
{
    /// <summary>
    /// Localization (en + pt-BR) of the Regions 10–16 mounts (<see cref="MountFarRegionCatalog"/>): description, lore,
    /// favourite lines, abilities, toasts and the three whistles sold in the cities. Merged into <see cref="MountTexts.Entries"/>;
    /// names and Caderno hints live in <see cref="MountCodex"/>.
    /// </summary>
    public static class MountFarRegionTexts
    {
        public const string ToastGlide = "ui.mount.far.glide";
        public const string ToastPumice = "ui.mount.far.pumice";
        public const string ToastUntieHit = "ui.mount.far.untie_hit";
        public const string ToastUntieMiss = "ui.mount.far.untie_miss";
        public const string ToastAuroraRun = "ui.mount.far.aurora_run";
        public const string ToastThicket = "ui.mount.far.thicket";
        public const string ToastRevealSeen = "ui.mount.far.reveal_seen";
        public const string ToastRevealNone = "ui.mount.far.reveal_none";
        public const string ToastDread = "ui.mount.far.dread";
        public const string ToastBoulder = "ui.mount.far.boulder";

        // id, en desc, pt desc, en lore, pt lore
        private static readonly (string Id, string DescEn, string DescPt, string LoreEn, string LorePt)[] Mounts =
        {
            (MountFarRegionCatalog.GordiasGoat,
                "A golden goat from the Satyr Vale. It chews through any knot, and no rope chest stays shut near it.",
                "Um bode dourado do Vale dos Sátiros. Rói qualquer nó, e baú de corda nenhum fica fechado perto dele.",
                "They say old Gordias tied his cart with a knot nobody could undo. His goat never saw the problem. Ícaro timed it: four seconds.",
                "Dizem que o velho Gordias amarrou a carroça com um nó que ninguém desatava. O bode dele nunca viu problema nisso. O Ícaro cronometrou: quatro segundos."),
            (MountFarRegionCatalog.AuroraReindeer,
                "A pale reindeer of the White Moon Harbor. It runs over thin ice without breaking it and no frost slows it.",
                "Uma rena clara do Porto da Lua Branca. Corre sobre gelo fino sem quebrar, e geada nenhuma a atrasa.",
                "The herders of the north say the aurora is light on its way home, and that the reindeer learned to run from watching it. Maithe says it just likes showing off.",
                "Os pastores do norte dizem que a aurora é luz voltando para casa, e que a rena aprendeu a correr olhando para ela. A Maithe diz que ela só gosta de se exibir."),
            (MountFarRegionCatalog.LacquerLion,
                "A guardian lion of red lacquer from Hinokura. It stands watch on the move and sees what hides around the cousins.",
                "Um leão-guardião de laca vermelha de Hinokura. Monta guarda andando e enxerga o que se esconde em volta dos primos.",
                "On the islands, every gate has its pair of lions, one with the mouth open and one with it shut. This one came down off its gate, and nobody on the island has dared ask it why.",
                "Nas ilhas, todo portão tem o seu par de leões, um de boca aberta e outro de boca fechada. Este desceu do portão, e ninguém na ilha teve coragem de perguntar por quê."),
            (MountFarRegionCatalog.VolcanoGoat,
                "The goat of the herders of Mount Echeyde. Its hooves are hard as pumice, and burns do not stick to it.",
                "A cabra dos pastores do Monte Echeyde. Tem casco duro como pedra-pomes, e queimadura não pega nela.",
                "The shepherds of Echeyde walk their flocks across warm rock. Vó Edna says a goat that has eaten ash is afraid of nothing, least of all her soup.",
                "Os pastores do Echeyde levam o rebanho por cima da pedra quente. A Vó Edna diz que cabra que já comeu cinza não tem medo de nada, muito menos da sopa dela."),
            (MountFarRegionCatalog.MesektetIbis,
                "A great white ibis of the Mesektet Quay. It glides long stretches low over the ground. A true winged mount.",
                "Uma grande íbis branca do Cais de Mesektet. Plana longos trechos rente ao chão. Uma montaria alada de verdade.",
                "The ibises follow the night boat from hour to hour and know every bend of the river. They take the cousins as far as the wind allows, and never a wingbeat more.",
                "As íbis seguem a barca da noite de hora em hora e conhecem cada curva do rio. Levam os primos até onde o vento deixa, e nem uma batida de asa a mais."),
            (MountFarRegionCatalog.Mushhushshu,
                "The dragon-serpent of the glazed walls of Dur-Jarras, tamed. Its roar sends the common critters running.",
                "O dragão-serpente dos muros vidrados de Dur-Jarras, amansado. O rugido dele põe os bichos comuns para correr.",
                "It came down from a wall of blue bricks when the canal keepers fed it for a whole winter. Bosses are not impressed. Everyone else is.",
                "Desceu de um muro de tijolo azul depois que os guardas do canal o alimentaram um inverno inteiro. Chefe não se impressiona. O resto, sim."),
            (MountFarRegionCatalog.TaigaElk,
                "A tall elk of the cedar forests. It pushes through thick brush and nothing slows it down.",
                "Um alce alto dos cedrais. Atravessa mato fechado e nada o atrasa.",
                "Hobabish's woodcutters use it to haul cedar. Lavine asked it to go around a tree once. It looked at her, then at the tree, and went through.",
                "Os lenhadores do Hobabish usam ele para puxar cedro. A Lavine pediu uma vez para ele dar a volta numa árvore. Ele olhou para ela, olhou para a árvore e passou por dentro."),
            (MountFarRegionCatalog.HillRam,
                "A giant's ram from the fields of the dream tablets. Its charge sends critters flying and breaks what is in the way.",
                "Um carneiro de gigante do campo das tábuas do sonho. A cabeçada joga os bichos longe e quebra o que estiver no caminho.",
                "The giants herded them to move stones. Now the stones stay where they are and the rams stay bored, which Joaquim thinks is a shame.",
                "Os gigantes os criavam para mover pedra. Agora as pedras ficam quietas e os carneiros, entediados, o que o Joaquim acha uma pena."),
        };

        // ability id, en name, pt name, en desc, pt desc
        private static readonly (string Id, string NameEn, string NamePt, string DescEn, string DescPt)[] Abilities =
        {
            ("no_de_gordias", "Gordian Knot", "Nó de Gordias",
                "The goat bites the nearest secret within 14 m and undoes it from where it stands. Cooldown 18 s.",
                "O bode morde o segredo mais perto a até 14 m e desfaz ali mesmo. Recarga 18 s."),
            ("trote_da_aurora", "Aurora Trot", "Trote da Aurora",
                "For 10 s the reindeer gallops without tiring and no slow holds it. Cooldown 45 s.",
                "Por 10 s a rena galopa sem cansar e nenhuma lentidão a segura. Recarga 45 s."),
            ("olho_da_guarda", "Guardian's Eye", "Olho da Guarda",
                "Marks every critter and hidden secret within 22 m. Cooldown 30 s.",
                "Marca todo bicho e segredo escondido a até 22 m. Recarga 30 s."),
            ("cascos_de_pedra_pomes", "Pumice Hooves", "Cascos de Pedra-Pomes",
                "Puts out burns and other ailments and keeps them off for 8 s. Cooldown 40 s.",
                "Apaga queimaduras e outros males e os mantém longe por 8 s. Recarga 40 s."),
            ("planar_de_mesektet", "Mesektet Glide", "Planar de Mesektet",
                "The ibis glides 16 m ahead in 1.4 s. Walls still stop it. Cooldown 20 s.",
                "A íbis plana 16 m para a frente em 1,4 s. Parede ainda segura. Recarga 20 s."),
            ("rugido_do_portao", "Roar of the Gate", "Rugido do Portão",
                "Common critters within 8 m run away for 4 s and hit 15% weaker. Bosses do not care. Cooldown 40 s.",
                "Os bichos comuns a até 8 m fogem por 4 s e batem 15% mais fraco. Chefe nem liga. Recarga 40 s."),
            ("passo_do_cedral", "Cedar Stride", "Passo do Cedral",
                "Clears slows and ignores them for 8 s. Cooldown 30 s.",
                "Tira a lentidão e a ignora por 8 s. Recarga 30 s."),
            ("cabecada_de_pedra", "Stone Headbutt", "Cabeçada de Pedra",
                "Charges 9 m: common critters in the way are thrown aside and secrets on the path break. Cooldown 22 s.",
                "Investe 9 m: os bichos comuns no caminho são jogados para o lado e os segredos na frente quebram. Recarga 22 s."),
        };

        private static readonly LocalizationEntry[] Fixed =
        {
            new LocalizationEntry("mount." + MountFarRegionCatalog.GordiasGoat + ".favorite_line", "Four seconds. New record.", "Quatro segundos. Recorde novo."),
            new LocalizationEntry("mount." + MountFarRegionCatalog.AuroraReindeer + ".favorite_line", "Okay, it can show off a little.", "Tá bom, ela pode se exibir um pouquinho."),
            new LocalizationEntry(ToastGlide, "{0} opens its wings and glides.", "{0} abre as asas e plana."),
            new LocalizationEntry(ToastPumice, "Hooves of pumice: nothing burns now.", "Casco de pedra-pomes: agora nada queima."),
            new LocalizationEntry(ToastUntieHit, "Chomp! The knot came undone.", "Nhac! O nó se desfez."),
            new LocalizationEntry(ToastUntieMiss, "No knot within reach.", "Nenhum nó ao alcance."),
            new LocalizationEntry(ToastAuroraRun, "{0} runs like the aurora: no tiring, no slowing.", "{0} corre como a aurora: sem cansar, sem atrasar."),
            new LocalizationEntry(ToastThicket, "{0} goes straight through the brush.", "{0} passa reto pelo mato."),
            new LocalizationEntry(ToastRevealSeen, "{0} stares: there, and there!", "{0} crava o olhar: ali, e ali!"),
            new LocalizationEntry(ToastRevealNone, "{0} looks around. Nothing hiding here.", "{0} olha em volta. Nada escondido aqui."),
            new LocalizationEntry(ToastDread, "The roar of the gate! The critters ran.", "O rugido do portão! Os bichos correram."),
            new LocalizationEntry(ToastBoulder, "Out of the way! Everything flew.", "Sai da frente! Voou tudo."),
        };

        // mount id, en name, pt name, en mount, pt mount
        private static readonly (string Id, string NameEn, string NamePt, string MountEn, string MountPt)[] Whistles =
        {
            (MountFarRegionCatalog.VolcanoGoat, "Volcano Herders' Goat Whistle", "Apito da Cabra-dos-Pastores do Vulcão", "the Volcano Herders' Goat", "a Cabra-dos-Pastores do Vulcão"),
            (MountFarRegionCatalog.TaigaElk, "Taiga Elk Whistle", "Apito do Alce da Taiga", "the Taiga Elk", "o Alce da Taiga"),
            (MountFarRegionCatalog.HillRam, "Hill-Giant Ram Whistle", "Apito do Carneiro Gigante da Colina", "the Hill-Giant Ram", "o Carneiro Gigante da Colina"),
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
