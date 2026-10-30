using System.Collections.Generic;
using Rapadura.Core.Localization;

namespace Rapadura.Gameplay.Mounts
{
    /// <summary>
    /// Localization (en + pt-BR) of M3 wave 1 (<see cref="MountWaveCatalog"/>): each walker's description (with the
    /// cousins' little stories of Docs/Mounts.md §8), its ability, the whistles sold in the city shops, the ability
    /// toasts, the drops, the Fitas do Laço and the Festa Junina prendas. Merged into <see cref="MountTexts.Entries"/>
    /// (names and Caderno hints of the species already live in <see cref="MountCodex"/>).
    /// </summary>
    public static class MountWaveTexts
    {
        public const string ToastCleanse = "ui.mount.wave.cleanse";
        public const string ToastFreeGallop = "ui.mount.wave.free_gallop";
        public const string ToastKnockback = "ui.mount.wave.knockback";
        public const string ToastCcImmune = "ui.mount.wave.cc_immune";
        public const string ToastNightVision = "ui.mount.wave.night_vision";
        public const string ToastNightVisionSeen = "ui.mount.wave.night_vision_seen";
        public const string ToastSpeedTrail = "ui.mount.wave.speed_trail";
        public const string ToastTongueHit = "ui.mount.wave.tongue_hit";
        public const string ToastTongueMiss = "ui.mount.wave.tongue_miss";
        public const string ToastRoar = "ui.mount.wave.roar";
        public const string ToastStomp = "ui.mount.wave.stomp";
        public const string ToastHealAura = "ui.mount.wave.heal_aura";
        public const string ToastPounce = "ui.mount.wave.pounce";
        public const string ToastPounceMiss = "ui.mount.wave.pounce_miss";
        public const string ToastLamp = "ui.mount.wave.lamp";
        public const string ToastLampFound = "ui.mount.wave.lamp_found";
        public const string ToastEmberTrail = "ui.mount.wave.ember_trail";
        public const string ToastStealth = "ui.mount.wave.stealth";
        public const string ToastRing = "ui.mount.wave.ring";
        public const string ToastBoiDance = "ui.mount.wave.boi_dance";
        public const string ToastShell = "ui.mount.wave.shell";
        public const string MapShowOff = "ui.mount.wave.map_show_off";

        public const string DropToast = "ui.mount.wave.drop";
        public const string RibbonGained = "ui.mount.wave.ribbon_gained";
        public const string RibbonRedeemed = "ui.mount.wave.ribbon_redeemed";
        public const string RibbonLine = "ui.mount.wave.ribbon_line";
        public const string HookGranted = "ui.mount.wave.hook_granted";
        public const string PrendasBought = "ui.mount.wave.prendas_bought";
        public const string NotEnoughPrendas = "ui.mount.wave.not_enough_prendas";


        private static readonly LocalizationEntry[] Ui =
        {
            new LocalizationEntry(ToastCleanse, "{0} rolled in the mud: no more slow, poison or burns!", "{0} rolou na lama: acabou lentidão, veneno e queimadura!"),
            new LocalizationEntry(ToastFreeGallop, "{0} is running on pure breath!", "{0} está correndo no fôlego puro!"),
            new LocalizationEntry(ToastKnockback, "Out of the way! The critters got pushed aside.", "Sai da frente! Os bichos foram empurrados."),
            new LocalizationEntry(ToastCcImmune, "{0} dug its hooves in: nothing moves it now.", "{0} fincou o pé: agora nada tira do lugar."),
            new LocalizationEntry(ToastNightVision, "{0}'s eyes shine: the night got clear.", "Os olhos do {0} brilham: a noite clareou."),
            new LocalizationEntry(ToastNightVisionSeen, "{0} spotted critters hiding nearby!", "{0} viu bichos escondidos por perto!"),
            new LocalizationEntry(ToastSpeedTrail, "A trail of leaves! Step on it to run faster.", "Uma trilha de folhas! Pise nela para correr mais."),
            new LocalizationEntry(ToastTongueHit, "Slurp! The long tongue reached it.", "Slurp! A língua comprida alcançou."),
            new LocalizationEntry(ToastTongueMiss, "Nothing within reach of the tongue.", "Nada ao alcance da língua."),
            new LocalizationEntry(ToastRoar, "ROAR! The critters lost their nerve.", "RUARRR! Os bichos perderam a coragem."),
            new LocalizationEntry(ToastStomp, "BOOM! Everyone around got dizzy.", "BUM! Todo mundo em volta ficou tonto."),
            new LocalizationEntry(ToastHealAura, "{0} glows softly: the wounds are closing.", "{0} brilha de leve: os machucados estão fechando."),
            new LocalizationEntry(ToastPounce, "Pounce! The next blow on it is a sure critical.", "Bote! O próximo golpe nele é crítico garantido."),
            new LocalizationEntry(ToastPounceMiss, "No critter close enough to pounce on.", "Nenhum bicho perto o bastante para o bote."),
            new LocalizationEntry(ToastLamp, "The jewel lit up the whole cave.", "A joia acendeu a gruta inteira."),
            new LocalizationEntry(ToastLampFound, "The jewel's light shows something hidden!", "A luz da joia mostrou coisa escondida!"),
            new LocalizationEntry(ToastEmberTrail, "Hot embers behind us: critters, keep off!", "Brasa atrás da gente: bicho, não pisa!"),
            new LocalizationEntry(ToastStealth, "Not a single footprint: the critters can barely notice you.", "Nenhuma pegada: os bichos quase não percebem vocês."),
            new LocalizationEntry(ToastRing, "A ring of protective fire! Critters stay outside.", "Um anel de fogo protetor! Bicho fica do lado de fora."),
            new LocalizationEntry(ToastBoiDance, "The Boi dances! Everyone hits harder and faster.", "O Boi dançou! Todo mundo bate mais forte e mais rápido."),
            new LocalizationEntry(ToastShell, "Into the shell! Nobody gets hurt in here.", "Pro casco! Aqui dentro ninguém se machuca."),
            new LocalizationEntry(MapShowOff, "{0} shows off its {1} on the road.", "{0} mostra o {1} na estrada."),

            new LocalizationEntry(DropToast, "{0} decided to follow the cousins home!", "{0} resolveu seguir os primos para casa!"),
            new LocalizationEntry(RibbonGained, "Fita do Laço from {0}: {1}/{2}", "Fita do Laço de {0}: {1}/{2}"),
            new LocalizationEntry(RibbonRedeemed, "The Haras tied {1} ribbons together: {0} is yours!", "No Haras, {1} fitas amarradas: {0} agora é de vocês!"),
            new LocalizationEntry(RibbonLine, "{0}: {1} ribbons (Rare at {2}, Epic at {3})", "{0}: {1} fitas (Rara com {2}, Épica com {3})"),
            new LocalizationEntry(HookGranted, "{0} joined the cousins!", "{0} veio morar com os primos!"),
            new LocalizationEntry(PrendasBought, "Festa Junina prize: {0} for {1} prendas!", "Prenda da Festa Junina: {0} por {1} prendas!"),
            new LocalizationEntry(NotEnoughPrendas, "Not enough prendas yet ({0}/{1}).", "Ainda faltam prendas ({0}/{1})."),
        };

        // id, en desc, pt desc
        private static readonly (string Id, string DescEn, string DescPt)[] Descriptions =
        {
            (MountWaveCatalog.Capybara, "A calm, round capybara who loves mud baths. Lavine says it is the comfiest seat in the world.",
                "Uma capivara calma e redonda que adora banho de lama. A Lavine diz que é o assento mais fofo do mundo."),
            (MountWaveCatalog.MountainGoat, "A bearded goat that jumps where no one else can. Ícaro measures every jump with a stick.",
                "Um bode barbudo que pula onde ninguém alcança. O Ícaro mede cada pulo com um graveto."),
            (MountWaveCatalog.Rhea, "A long-legged rhea that runs and runs and never gets tired.",
                "Uma ema de pernas compridas que corre, corre e não cansa nunca."),
            (MountWaveCatalog.PintoHorse, "A spotted cowboy horse. Vô Jorge rode one just like it when he drove cattle in the sertão.",
                "Um cavalo pampa de vaqueiro. O Vô Jorge montava um igualzinho quando tocava boiada no sertão."),
            (MountWaveCatalog.EnchantedDonkey, "A stubborn, holy donkey that remembers every well in the desert. It only follows who brings it water.",
                "Um jumento teimoso e abençoado que lembra de todo poço do deserto. Só segue quem traz água para ele."),
            (MountWaveCatalog.ManedWolf, "A shy maned wolf with long legs and kind eyes. It sees in the dark like it was daytime.",
                "Um lobo-guará tímido, de pernas compridas e olhar bondoso. Enxerga no escuro como se fosse dia."),
            (MountWaveCatalog.CaiporaPeccary, "The Caipora's own peccary. It leaves fresh leaves behind and only trusts friends of the forest.",
                "O caititu da própria Caipora. Deixa folhas frescas pelo caminho e só confia em amigo da mata."),
            (MountWaveCatalog.GiantAnteater, "A giant anteater with a flag-like tail and a very, very long tongue.",
                "Um tamanduá-bandeira gigante, de rabo de bandeira e língua muito, muito comprida."),
            (MountWaveCatalog.SnowTiger, "A white tiger of the high peaks. Its roar makes even brave critters hesitate.",
                "Um tigre branco dos picos altos. O rugido dele faz até bicho valente hesitar."),
            (MountWaveCatalog.WoollyMammoth, "A huge, woolly mammoth that walks slowly and never falls. Joaquim says it is the only one big enough for him.",
                "Um mamute enorme e peludo que anda devagar e nunca cai. O Joaquim diz que é o único do tamanho dele."),
            (MountWaveCatalog.Unicorn, "A white unicorn of the deep forest whose horn heals. It let Maria come close first.",
                "Um unicórnio branco da mata fechada cujo chifre cura. Deixou a Maria chegar perto primeiro."),
            (MountWaveCatalog.LightJaguar, "A jaguar whose spots glow at night. Silent, fast, and very proud.",
                "Uma onça-pintada de pintas que brilham à noite. Silenciosa, rápida e muito orgulhosa."),
            (MountWaveCatalog.Carbuncle, "A small cave creature with a red jewel on its forehead that lights the dark.",
                "Um bichinho das grutas com uma joia vermelha na testa que acende o escuro."),
            (MountWaveCatalog.FireRhea, "A rhea with feathers like embers, prize of the Festa Junina. Vó Edna says it was born of a bonfire spark.",
                "Uma ema de penas cor de brasa, prenda da Festa Junina. A Vó Edna diz que nasceu de uma faísca da fogueira."),
            (MountWaveCatalog.Kirin, "A dew-coloured kirin that walks without bending the grass.",
                "Um kirin cor de orvalho que anda sem dobrar a grama."),
            (MountWaveCatalog.Boitata, "The good Boitatá, the fire serpent that guards the forest. It scares away whoever wants to burn the woods.",
                "O Boitatá do bem, a cobra de fogo que guarda a mata. Espanta quem quer queimar a floresta."),
            (MountWaveCatalog.BoiBumba, "A starry Boi-Bumbá that dances to every toada. Nobody stands still when it starts.",
                "Um Boi-Bumbá estrelado que dança toda toada. Ninguém fica parado quando ele começa."),
            (MountWaveCatalog.ElderTortoise, "An ancient tortoise older than the Vila. Vô Jorge used to say it knew his own grandfather.",
                "Um jabuti antiquíssimo, mais velho que a Vila. O Vô Jorge dizia que ele conheceu o avô dele."),
        };

        // ability id, en name, pt name, en desc, pt desc
        private static readonly (string Id, string NameEn, string NamePt, string DescEn, string DescPt)[] Abilities =
        {
            ("banho_de_lama", "Mud Bath", "Banho de Lama",
                "Removes slow, poison and burns from the cousin and keeps them away for 3 s.", "Tira lentidão, veneno e queimadura do primo e deixa ele imune por 3 s."),
            ("pulo_duplo", "Double Jump", "Pulo Duplo",
                "A second jump in the air (reaches 2.5 m ledges).", "Um segundo pulo no ar (alcança bordas de 2,5 m)."),
            ("folego_de_ema", "Rhea's Breath", "Fôlego de Ema",
                "Gallops for 8 s without spending stamina.", "Galopa 8 s sem gastar fôlego."),
            ("investida_do_vaqueiro", "Cowboy's Charge", "Investida do Vaqueiro",
                "Charges 8 m and pushes the critters on the way 3 m aside (no damage; bosses stand firm).",
                "Avança 8 m e empurra 3 m os bichos no caminho (sem dano; chefe não sai do lugar)."),
            ("teimosia_santa", "Holy Stubbornness", "Teimosia Santa",
                "Immune to push, stun and slow for 5 s.", "Imune a empurrão, atordoamento e lentidão por 5 s."),
            ("olhos_do_cerrado", "Cerrado Eyes", "Olhos do Cerrado",
                "Night vision for 30 s and critters within 30 m are marked.", "Visão noturna por 30 s e bichos a até 30 m ficam marcados."),
            ("trilha_da_caipora", "Caipora's Trail", "Trilha da Caipora",
                "Leaves a trail of leaves for 6 s: party members on it run 15% faster (within the cap).",
                "Deixa um rastro de folhas por 6 s: quem do grupo pisa nele corre 15% mais (dentro do teto)."),
            ("lingua_comprida", "Long Tongue", "Língua Comprida",
                "Reaches a hidden secret up to 12 m away and opens it a bit.", "Alcança um segredo escondido a até 12 m e abre um pouquinho."),
            ("rugido_do_tigre", "Tiger's Roar", "Rugido do Tigre",
                "Critters within 8 m deal 20% less damage for 6 s (bosses 3 s).", "Bichos a até 8 m dão 20% menos dano por 6 s (chefe 3 s)."),
            ("pisao_do_mamute", "Mammoth Stomp", "Pisão do Mamute",
                "Stuns everything within 4 m for 1.2 s (bosses 0.6 s).", "Atordoa tudo a até 4 m por 1,2 s (chefe 0,6 s)."),
            ("aura_curativa", "Healing Aura", "Aura Curativa",
                "Heals 2% of max health per second to the party within 6 m, for 6 s.", "Cura 2% da vida máxima por segundo do grupo a até 6 m, por 6 s."),
            ("salto_da_onca", "Jaguar's Pounce", "Salto da Onça",
                "Leaps onto a critter up to 12 m away: the cousin's next blow is a sure critical.",
                "Pula num bicho a até 12 m: o próximo golpe do primo é crítico garantido."),
            ("joia_acesa", "Lit Jewel", "Joia Acesa",
                "Lights 15 m around for 60 s and reveals hidden secrets.", "Ilumina 15 m em volta por 60 s e revela segredos escondidos."),
            ("corrida_de_brasa", "Ember Run", "Corrida de Brasa",
                "Leaves a trail of embers for 6 s: critters on it burn 4 per second for 3 s.",
                "Deixa um rastro de brasa por 6 s: bicho que pisa queima 4 por segundo por 3 s."),
            ("passo_sem_pegada", "Footprintless Step", "Passo Sem Pegada",
                "For 10 s critters only notice the cousin at 20% of the usual distance. Attacking ends it.",
                "Por 10 s os bichos só percebem o primo a 20% da distância normal. Atacar cancela."),
            ("anel_de_fogo_protetor", "Protective Ring of Fire", "Anel de Fogo Protetor",
                "A 5 m ring of fire for 6 s: critters inside are pushed out and burn 5 per second (bosses are not pushed).",
                "Um anel de fogo de 5 m por 6 s: bicho que entra é empurrado para fora e queima 5 por segundo (chefe não é empurrado)."),
            ("toada_do_boi", "Boi's Toada", "Toada do Boi",
                "The party within 10 m gets +10% damage and +10% attack speed for 10 s.",
                "O grupo a até 10 m ganha +10% de dano e +10% de velocidade de ataque por 10 s."),
            ("casco_de_pedra", "Stone Shell", "Casco de Pedra",
                "Cousin and tortoise close up in the shell for 3 s: no damage, but no moving either.",
                "Primo e jabuti se fecham no casco por 3 s: ninguém leva dano, mas também não anda."),
        };

        // Whistles sold in the city shops (pt-BR = the Resources/Items asset text).
        private static readonly (string MountId, string NameEn, string NamePt, string MountEn, string MountPt)[] ShopWhistles =
        {
            (MountWaveCatalog.Capybara, "Giant Capybara Whistle", "Apito da Capivara Gigante", "the Giant Capybara", "a Capivara Gigante"),
            (MountWaveCatalog.MountainGoat, "Leaping Goat Whistle", "Apito do Bode Saltador", "the Leaping Goat", "o Bode Saltador"),
            (MountWaveCatalog.Rhea, "Running Rhea Whistle", "Apito da Ema Corredora", "the Running Rhea", "a Ema Corredora"),
            (MountWaveCatalog.PintoHorse, "Cowboy's Pinto Horse Whistle", "Apito do Cavalo Pampa do Vaqueiro", "the Cowboy's Pinto Horse", "o Cavalo Pampa do Vaqueiro"),
            (MountWaveCatalog.GiantAnteater, "Giant Anteater Whistle", "Apito do Tamanduá-Bandeira Gigante", "the Giant Anteater", "o Tamanduá-Bandeira Gigante"),
        };

        // Declared after the arrays it reads: static fields initialize in textual order.
        public static readonly IReadOnlyList<LocalizationEntry> Entries = Build();

        private static List<LocalizationEntry> Build()
        {
            var entries = new List<LocalizationEntry>(Ui);
            foreach (var d in Descriptions)
            {
                entries.Add(new LocalizationEntry("mount." + d.Id + ".desc", d.DescEn, d.DescPt));
            }

            foreach (var a in Abilities)
            {
                entries.Add(new LocalizationEntry("mount.ability." + a.Id + ".name", a.NameEn, a.NamePt));
                entries.Add(new LocalizationEntry("mount.ability." + a.Id + ".desc", a.DescEn, a.DescPt));
            }

            foreach (var w in ShopWhistles)
            {
                string itemId = MountWaveCatalog.WhistleOf(w.MountId);
                entries.Add(new LocalizationEntry(Content.ContentText.ItemNameKey(itemId), w.NameEn, w.NamePt));
                entries.Add(new LocalizationEntry(Content.ContentText.ItemDescriptionKey(itemId),
                    $"Bamboo whistle. Whoever buys it takes {w.MountEn} home.", $"Apito de taquara. Quem compra leva {w.MountPt} para casa."));
            }

            // The favourite cousins' lines (§8).
            entries.Add(new LocalizationEntry("mount." + MountWaveCatalog.WoollyMammoth + ".favorite_line",
                "Joaquim: Easy, big one. You and me, nobody falls behind.", "Joaquim: Calma, grandão. Eu e você, ninguém fica pra trás."));
            entries.Add(new LocalizationEntry("mount." + MountWaveCatalog.Unicorn + ".favorite_line",
                "Maria: Thank you for trusting me. I'll take good care of you.", "Maria: Obrigada por confiar em mim. Vou cuidar bem de você."));
            return entries;
        }
    }
}
