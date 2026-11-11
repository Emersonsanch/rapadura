using System.Collections.Generic;
using System.Linq;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Characters;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>Localization (en + pt-BR) of the Bosque do Zarafshan mechanics. Same self-contained pattern as <c>OverworldTexts</c>.</summary>
    public static class ZarafshanMechanicsTexts
    {
        public const string HarvestPrompt = "ui.zarafshan.harvest_prompt";
        public const string HarvestFound = "ui.zarafshan.harvest_found";
        public const string HarvestBagFull = "ui.zarafshan.harvest_bag_full";
        public const string HarvestPicked = "ui.zarafshan.harvest_picked";
        public const string FordRefresh = "ui.zarafshan.ford_refresh";
        public const string LineGrandmaA = "ui.zarafshan.line.grandma_a";
        public const string LineGrandmaB = "ui.zarafshan.line.grandma_b";
        public const string LineJoaquim = "ui.zarafshan.line.joaquim";
        public const string LineMaria = "ui.zarafshan.line.maria";
        public const string LineMaithe = "ui.zarafshan.line.maithe";
        public const string LineIcaro = "ui.zarafshan.line.icaro";
        public const string LineLavine = "ui.zarafshan.line.lavine";

        private static List<LocalizationEntry> _entries;
        private static Dictionary<string, string> _fallback;

        public static IReadOnlyList<LocalizationEntry> Entries => _entries ??= new List<LocalizationEntry>
        {
            new LocalizationEntry(HarvestPrompt, "[E] Pick from the tree", "[E] Colher da árvore"),
            new LocalizationEntry(HarvestFound, "Picked: {0}", "Colheu: {0}"),
            new LocalizationEntry(HarvestBagFull, "Your bag is full: {0} stays on the tree", "Mochila cheia: {0} fica na árvore"),
            new LocalizationEntry(HarvestPicked, "This tree gives again tomorrow", "Esta árvore dá de novo amanhã"),
            new LocalizationEntry(FordRefresh, "\"Living Water mends, but only so far\", Grandma Edna used to say", "\"Água Viva cura, mas só até um tanto\", dizia a Vó Edna"),
            new LocalizationEntry(LineGrandmaA, "Grandma Edna: \"Take it, there is always more tomorrow.\"", "Vó Edna: \"Pode pegar, amanhã tem mais.\""),
            new LocalizationEntry(LineGrandmaB, "Grandma Edna: \"Whoever picks, eats. Whoever eats, helps in the kitchen.\"", "Vó Edna: \"Quem colhe, come. Quem come, ajuda na cozinha.\""),
            new LocalizationEntry(LineJoaquim, "Joaquim: \"I'd climb it, but Grandma's watching.\"", "Joaquim: \"Eu subia, mas a Vó tá olhando.\""),
            new LocalizationEntry(LineMaria, "Maria: \"Joaquim climbs, I catch. It has always worked.\"", "Maria: \"O Joaquim sobe, eu cato. Sempre deu certo.\""),
            new LocalizationEntry(LineMaithe, "Maithe: \"Ícaro, leave some for the others!\"", "Maithe: \"Ícaro, deixa um pouco pros outros!\""),
            new LocalizationEntry(LineIcaro, "Ícaro: \"One for the bag, one for me. That's the rule.\"", "Ícaro: \"Um pra mochila, um pra mim. É a regra.\""),
            new LocalizationEntry(LineLavine, "Lavine: \"Does it count as luck if it falls on my head?\"", "Lavine: \"Se cair na minha cabeça, conta como sorte?\""),
        };

        /// <summary>The key of the line after a pick: the hero's, or the Vó Edna's (also when the hero is unknown).</summary>
        public static string HarvestLine(CharacterId? hero, bool grandma, int day)
        {
            if (grandma || !hero.HasValue)
            {
                return day % 2 == 0 ? LineGrandmaA : LineGrandmaB;
            }

            switch (hero.Value)
            {
                case CharacterId.Joaquim: return LineJoaquim;
                case CharacterId.Maria: return LineMaria;
                case CharacterId.Maithe: return LineMaithe;
                case CharacterId.Icaro: return LineIcaro;
                default: return LineLavine;
            }
        }

        public static string Get(LocalizationManager manager, string key)
        {
            if (manager != null)
            {
                if (!manager.HasKey(HarvestPrompt))
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
