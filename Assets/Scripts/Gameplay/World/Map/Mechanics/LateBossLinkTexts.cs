using System.Collections.Generic;
using System.Linq;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Characters;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// Localization (en + pt-BR) of the late map links done on 2026-11-16: the resin lanterns of Hobabish (80, <see cref="ResinLanternRules"/>), the
    /// fire on O Rei do Piche's tar (79, <see cref="PitchKingBurnRules"/>), the freed barque against Apep (76, <see cref="ApepBarqueLinkRules"/>),
    /// Hahya and Ohya beside the cousins (82, <see cref="DreamGiantsRules"/>) and the last Arca Resgatada +5 (96, <see cref="AzRescuedChestRules"/>),
    /// with one line per cousin for each. Same self-contained pattern as <see cref="NianScareTexts"/>.
    /// </summary>
    public static class LateBossLinkTexts
    {
        public const string ResinPrompt = "ui.late_links.resin_prompt";
        public const string ResinLit = "ui.late_links.resin_lit";
        public const string ResinAlready = "ui.late_links.resin_already";
        public const string ResinAllLit = "ui.late_links.resin_all_lit";
        public const string PitchBurnt = "ui.late_links.pitch_burnt";
        public const string ApepBarque = "ui.late_links.apep_barque";
        public const string GiantHahya = "ui.late_links.giant_hahya";
        public const string GiantOhya = "ui.late_links.giant_ohya";
        public const string AzChest = "ui.late_links.az_chest";
        public const string AzChestStones = "ui.late_links.az_chest_stones";
        public const string AzChestToday = "ui.late_links.az_chest_today";
        public const string BagFull = "ui.late_links.bag_full";

        /// <summary>The five lines of one moment, one per cousin (Maithe, Ícaro, Joaquim, Maria, Lavine).</summary>
        public enum Moment
        {
            ResinAllLit,
            PitchBurnt,
            GiantsHelp,
            AzChest,
        }

        private static readonly string[] Cousins = { "maithe", "icaro", "joaquim", "maria", "lavine" };

        private static List<LocalizationEntry> _entries;
        private static Dictionary<string, string> _fallback;

        public static string LineKey(Moment moment, int cousin) => "ui.late_links." + moment.ToString().ToLowerInvariant() + "." + Cousins[((cousin % 5) + 5) % 5];

        public static IReadOnlyList<LocalizationEntry> Entries => _entries ??= Build();

        private static List<LocalizationEntry> Build()
        {
            var list = new List<LocalizationEntry>
            {
                new LocalizationEntry(ResinPrompt, "[E] Light the resin lantern (Hobabish loses a mantle)", "[E] Acender a lanterna de resina (Hobabish perde um manto)"),
                new LocalizationEntry(ResinLit, "The resin lantern glows: Hobabish keeps {0} of his 7 mantles today", "A lanterna de resina brilha: Hobabish fica com {0} dos 7 mantos hoje"),
                new LocalizationEntry(ResinAlready, "This lantern already glows today", "Esta lanterna já brilha hoje"),
                new LocalizationEntry(ResinAllLit, "The seven lanterns glow: Hobabish has a single mantle left", "As sete lanternas brilham: Hobabish fica com um manto só"),
                new LocalizationEntry(PitchBurnt, "The fire catches the King's pitch: it burns before it can harden!", "O fogo pega no piche do Rei: ele queima antes de endurecer!"),
                new LocalizationEntry(ApepBarque, "The freed barque holds the river: Apep drinks only sand", "A barca solta segura o rio: Apep só bebe areia"),
                new LocalizationEntry(GiantHahya, "Hahya: Wake up, bad dream!", "Hahya: Acorda, sonho ruim!"),
                new LocalizationEntry(GiantOhya, "Ohya: I dreamed of you, brother of fear. Now it ends!", "Ohya: Eu sonhei com você, irmão do medo. Agora acaba!"),
                new LocalizationEntry(AzChest, "Āz gives back the last Rescued Chest +5: {0} gold, {1} XP", "Āz devolve a última Arca Resgatada +5: {0} de ouro, {1} de XP"),
                new LocalizationEntry(AzChestStones, "In the chest: {0} Blessing Stone(s) and {1} Soul Stone(s)", "Na arca: {0} Pedra(s) da Bênção e {1} Pedra(s) da Alma"),
                new LocalizationEntry(AzChestToday, "Āz already gave the chest back today: she hides nothing more until tomorrow", "Āz já devolveu a arca hoje: até amanhã ela não esconde mais nada"),
                new LocalizationEntry(BagFull, "The bag is full: some of the chest stayed on the ground", "A bolsa está cheia: parte da arca ficou no chão"),
            };

            AddLines(list, Moment.ResinAllLit, new[]
            {
                ("Maithe: The cedars breathe slower now. He can hear we're not here to cut.", "Maithe: Os cedros respiram mais devagar. Ele ouve que a gente não veio cortar."),
                ("Ícaro: Seven lanterns, seven mantles. I like it when the maths is on fire.", "Ícaro: Sete lanternas, sete mantos. Gosto quando a conta pega fogo."),
                ("Joaquim: We promised not to cut a single tree. The light says it for us.", "Joaquim: A gente prometeu não cortar árvore nenhuma. A luz fala por nós."),
                ("Maria: Look how it shines on the resin! Like Vó Edna's honey jars.", "Maria: Olha como brilha na resina! Igual aos potes de mel da Vó Edna."),
                ("Lavine: Vô Jorge says a guardian who sees the light remembers what he guards.", "Lavine: O Vô Jorge diz que guardião que vê a luz lembra o que guarda."),
            });
            AddLines(list, Moment.PitchBurnt, new[]
            {
                ("Maithe: It crackles before it sets. Listen for it and step out.", "Maithe: Ele estala antes de endurecer. Escuta e sai de perto."),
                ("Ícaro: That's what fire is for. Well, that and toast.", "Ícaro: É para isso que serve o fogo. Isso e torrada."),
                ("Joaquim: Burnt pitch, good pitch. The rafts will need it after.", "Joaquim: Piche queimado, piche bom. As balsas vão precisar depois."),
                ("Maria: It smells like the road in summer! Yuck!", "Maria: Cheira a estrada no verão! Eca!"),
                ("Lavine: Vó Edna sealed the old boat with pitch. She'd never let it hold anyone.", "Lavine: A Vó Edna calafetou o barco velho com piche. Ela nunca deixaria ele prender ninguém."),
            });
            AddLines(list, Moment.GiantsHelp, new[]
            {
                ("Maithe: The giants remember us. Their footsteps sound like a promise.", "Maithe: Os gigantes lembram da gente. Os passos deles soam como promessa."),
                ("Ícaro: Two giants on our side. Now it's a fair fight. For us.", "Ícaro: Dois gigantes do nosso lado. Agora a luta está justa. Para nós."),
                ("Joaquim: They dreamed their own end, and came to make it end well.", "Joaquim: Eles sonharam o próprio fim e vieram fazer ele terminar bem."),
                ("Maria: Hahya! Ohya! Hit it again!", "Maria: Hahya! Ohya! Bate de novo!"),
                ("Lavine: Vô Jorge always said: help the one you freed, and he'll help you back.", "Lavine: O Vô Jorge sempre disse: quem você soltou um dia volta para ajudar."),
            });
            AddLines(list, Moment.AzChest, new[]
            {
                ("Maithe: She is ashamed. You can hear it in how quiet the chest closes.", "Maithe: Ela ficou com vergonha. Dá para ouvir no jeito que a arca fecha quietinha."),
                ("Ícaro: The last chest. I'm counting it twice, just to be sure.", "Ícaro: A última arca. Vou contar duas vezes, só para garantir."),
                ("Joaquim: Greed gives everything back in the end. It just takes a garden.", "Joaquim: A cobiça devolve tudo no fim. Só precisa de um jardim."),
                ("Maria: Is it ours now? Can I show Vó Edna?", "Maria: Agora é nosso? Posso mostrar para a Vó Edna?"),
                ("Lavine: Vô Jorge said what was hidden comes back to whoever shares it.", "Lavine: O Vô Jorge dizia que o que foi escondido volta para quem divide."),
            });
            return list;
        }

        private static void AddLines(List<LocalizationEntry> list, Moment moment, (string En, string Pt)[] lines)
        {
            for (int i = 0; i < lines.Length; i++)
            {
                list.Add(new LocalizationEntry(LineKey(moment, i), lines[i].En, lines[i].Pt));
            }
        }

        /// <summary>The cousin's line for <paramref name="moment"/> (Joaquim's when no cousin is known).</summary>
        public static string LineFor(Moment moment, CharacterId? hero)
        {
            switch (hero)
            {
                case CharacterId.Maithe: return LineKey(moment, 0);
                case CharacterId.Icaro: return LineKey(moment, 1);
                case CharacterId.Maria: return LineKey(moment, 3);
                case CharacterId.Lavine: return LineKey(moment, 4);
                default: return LineKey(moment, 2);
            }
        }

        public static string Get(LocalizationManager manager, string key)
        {
            if (manager != null)
            {
                if (!manager.HasKey(ResinPrompt))
                {
                    manager.LoadEntries(Entries.Where(entry => !manager.HasKey(entry.Key)).ToList());
                }

                if (manager.HasKey(key))
                {
                    return manager.Get(key);
                }
            }

            _fallback ??= Entries.ToDictionary(entry => entry.Key, entry => entry.PtBR);
            return key != null && _fallback.TryGetValue(key, out string text) ? text : key;
        }
    }
}
