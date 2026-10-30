using System.Collections.Generic;
using System.Linq;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Content;

namespace Rapadura.Gameplay.Refining
{
    /// <summary>
    /// en + pt-BR text of item refining (FASE 16): the "Pedras do Sertão" items (<c>item.&lt;id&gt;.name/.description</c>,
    /// the pt-BR column equal to the asset text), the refine window of the city hub (<c>ui.refine.*</c>), Mestre Tonho's
    /// line per cousin and the tooltip lines. Same self-contained pattern as the other *Texts helpers: merged into the
    /// live <see cref="LocalizationManager"/> on first use (keys it already has win), pt-BR fallback without one.
    /// </summary>
    public static class RefineTexts
    {
        public const string HubButton = "ui.refine.hub_button";
        public const string Title = "ui.refine.title";
        public const string Subtitle = "ui.refine.subtitle";
        public const string SmithAny = "ui.refine.smith.greeting_any";
        /// <summary>{0} Bênção, {1} Alma, {2} Vida, {3} Amuleto, {4} ouro.</summary>
        public const string Owned = "ui.refine.owned";
        public const string Equipped = "ui.refine.equipped";
        public const string Empty = "ui.refine.empty";
        /// <summary>{0} from, {1} to, {2} chance %.</summary>
        public const string Step = "ui.refine.step";
        /// <summary>{0} stone name.</summary>
        public const string Cost = "ui.refine.cost";
        /// <summary>{0} stone name, {1} gold.</summary>
        public const string CostGold = "ui.refine.cost_gold";
        /// <summary>{0} level kept.</summary>
        public const string FailKeep = "ui.refine.fail_keep";
        /// <summary>{0} level it goes back to.</summary>
        public const string FailDrop = "ui.refine.fail_drop";
        public const string FailNever = "ui.refine.fail_never";
        public const string MaxLevel = "ui.refine.max_level";
        public const string UseCharm = "ui.refine.use_charm";
        public const string RefineButton = "ui.refine.refine_button";
        /// <summary>{0} option level, {1} max, {2} chance %.</summary>
        public const string OptionButton = "ui.refine.option_button";
        /// <summary>{0} item, {1} from, {2} to, {3} chance %, {4} failure line.</summary>
        public const string Confirm = "ui.refine.confirm";
        public const string ConfirmYes = "ui.refine.confirm_yes";
        public const string ConfirmNo = "ui.refine.confirm_no";
        /// <summary>{0} item (with its new +N).</summary>
        public const string ResultSuccess = "ui.refine.result_success";
        /// <summary>{0} item (with its new +N).</summary>
        public const string ResultDropped = "ui.refine.result_dropped";
        /// <summary>{0} item.</summary>
        public const string ResultKept = "ui.refine.result_kept";
        /// <summary>{0} item, {1} option level, {2} max.</summary>
        public const string OptionSuccess = "ui.refine.option_success";
        /// <summary>{0} item.</summary>
        public const string OptionFailed = "ui.refine.option_failed";
        /// <summary>{0} stone name.</summary>
        public const string MissingStone = "ui.refine.missing_stone";
        /// <summary>{0} gold needed.</summary>
        public const string MissingGold = "ui.refine.missing_gold";
        public const string MissingCharm = "ui.refine.missing_charm";
        public const string MaxOption = "ui.refine.max_option";
        public const string Close = "ui.refine.close";
        /// <summary>{0} level, {1} bonus %.</summary>
        public const string TooltipLevel = "ui.refine.tooltip.level";
        /// <summary>{0} option level, {1} max, {2} bonus %.</summary>
        public const string TooltipOptionWeapon = "ui.refine.tooltip.option_weapon";
        /// <summary>{0} option level, {1} max, {2} bonus %.</summary>
        public const string TooltipOptionArmour = "ui.refine.tooltip.option_armour";

        /// <summary>"ui.refine.smith.greeting_joaquim": Mestre Tonho's line when <paramref name="hero"/> opens the bench.</summary>
        public static string GreetingKey(CharacterId hero) => "ui.refine.smith.greeting_" + hero.ToString().ToLowerInvariant();

        public static readonly List<LocalizationEntry> Entries = new List<LocalizationEntry>
        {
            // -- Items (Resources/Items; pt-BR = the asset's text) --
            new LocalizationEntry(ContentText.ItemNameKey(RefineItems.BlessingStone), "Blessing Stone", "Pedra da Bênção"),
            new LocalizationEntry(ContentText.ItemDescriptionKey(RefineItems.BlessingStone),
                "A smooth stone blessed at the village chapel. Refines a piece of gear from +0 up to +6 and never fails.",
                "Pedra lisa benzida na capela do povoado. Refina um equipamento de +0 até +6 e nunca falha."),
            new LocalizationEntry(ContentText.ItemNameKey(RefineItems.SoulStone), "Soul Stone", "Pedra da Alma"),
            new LocalizationEntry(ContentText.ItemDescriptionKey(RefineItems.SoulStone),
                "A bluish stone said to keep the songs of the old cowboys. Refines from +6 up to +15; it can fail.",
                "Pedra azulada que dizem guardar o aboio dos vaqueiros antigos. Refina de +6 até +15; pode falhar."),
            new LocalizationEntry(ContentText.ItemNameKey(RefineItems.LifeStone), "Life Stone", "Pedra da Vida"),
            new LocalizationEntry(ContentText.ItemDescriptionKey(RefineItems.LifeStone),
                "A warm green stone, like a mandacaru seed. Strengthens a piece of gear's extra option.",
                "Pedra verde e quentinha, como semente de mandacaru. Dá mais força à opção extra de um equipamento."),
            new LocalizationEntry(ContentText.ItemNameKey(RefineItems.ChaosStone), "Chaos Stone", "Pedra do Caos"),
            new LocalizationEntry(ContentText.ItemDescriptionKey(RefineItems.ChaosStone),
                "A stone that changes colour on its own. Grandma keeps these for the Enchanted Cauldron.",
                "Pedra que muda de cor sozinha. A Vó guarda essas para o Tacho Encantado."),
            new LocalizationEntry(ContentText.ItemNameKey(RefineItems.LuckCharm), "Lucky Charm", "Amuleto da Sorte"),
            new LocalizationEntry(ContentText.ItemDescriptionKey(RefineItems.LuckCharm),
                "A Bonfim ribbon with a little wooden figa. Raises the chance of a refine that can fail; used up on the try.",
                "Fitinha do Bonfim com uma figa de madeira. Aumenta a chance de um refino que pode falhar; gasta na tentativa."),

            // -- Refine window (city hub → Serviços) --
            new LocalizationEntry(HubButton, "Blacksmith: refine", "Ferreiro: refinar"),
            new LocalizationEntry(Title, "Blacksmith's bench: refining", "Bancada do ferreiro: refino"),
            new LocalizationEntry(Subtitle, "Sertão Stones take gear from +0 to +15. A failed try goes back one level — the item never breaks.",
                "As Pedras do Sertão sobem o equipamento de +0 a +15. Se falhar, volta um nível — o item nunca some."),
            new LocalizationEntry(SmithAny, "Bring it here. Iron listens to whoever is patient.", "Traz pra cá. Ferro escuta quem tem paciência."),
            new LocalizationEntry("ui.refine.smith.greeting_maithe", "Maithe, the eldest! That bow string of yours deserves a blessed stone.",
                "Maithe, a mais velha! Essa corda de arco merece uma pedra benzida."),
            new LocalizationEntry("ui.refine.smith.greeting_maria", "Maria, careful with the bellows — your hands are for healing, I'll do the hammering.",
                "Maria, cuidado com o fole — tua mão é de curar, que o martelo fica comigo."),
            new LocalizationEntry("ui.refine.smith.greeting_joaquim", "Joaquim, a shield is only as good as the arm and the anvil. Let's see both.",
                "Joaquim, escudo bom é braço firme e bigorna quente. Vamos ver os dois."),
            new LocalizationEntry("ui.refine.smith.greeting_icaro", "Ícaro, no, you can't put gears in the soul stone. ...Fine, show me the drawing.",
                "Ícaro, não, não dá pra botar engrenagem na pedra da alma. ...Tá, mostra o desenho."),
            new LocalizationEntry("ui.refine.smith.greeting_lavine", "Lavine, the little one. Sit by the forge — the sparks are prettier than the shadows.",
                "Lavine, a caçula. Senta perto da forja, que faísca é mais bonita que sombra."),
            new LocalizationEntry(Owned, "In the backpack: Blessing {0} · Soul {1} · Life {2} · Charm {3} · Gold {4}",
                "Na mochila: Bênção {0} · Alma {1} · Vida {2} · Amuleto {3} · Ouro {4}"),
            new LocalizationEntry(Equipped, "(equipped)", "(equipado)"),
            new LocalizationEntry(Empty, "No gear to refine. Weapons and armour show up here.", "Nenhum equipamento para refinar. Armas e armaduras aparecem aqui."),
            new LocalizationEntry(Step, "+{0} → +{1} · chance {2}%", "+{0} → +{1} · chance {2}%"),
            new LocalizationEntry(Cost, "Cost: 1 {0}", "Custo: 1 {0}"),
            new LocalizationEntry(CostGold, "Cost: 1 {0} + {1} gold", "Custo: 1 {0} + {1} de ouro"),
            new LocalizationEntry(FailKeep, "If it fails: stays at +{0}.", "Se falhar: fica em +{0}."),
            new LocalizationEntry(FailDrop, "If it fails: back to +{0} (the item is never lost).", "Se falhar: volta para +{0} (o item nunca some)."),
            new LocalizationEntry(FailNever, "Never fails.", "Nunca falha."),
            new LocalizationEntry(MaxLevel, "Maxed out: +15!", "No máximo: +15!"),
            new LocalizationEntry(UseCharm, "Use a Lucky Charm (+10% chance)", "Usar Amuleto da Sorte (+10% de chance)"),
            new LocalizationEntry(RefineButton, "Refine", "Refinar"),
            new LocalizationEntry(OptionButton, "Life Stone: option {0}/{1} ({2}%)", "Pedra da Vida: opção {0}/{1} ({2}%)"),
            new LocalizationEntry(Confirm, "Refine {0} from +{1} to +{2}? Chance {3}%. {4}", "Refinar {0} de +{1} para +{2}? Chance de {3}%. {4}"),
            new LocalizationEntry(ConfirmYes, "Yes, refine", "Sim, refinar"),
            new LocalizationEntry(ConfirmNo, "Not now", "Agora não"),
            new LocalizationEntry(ResultSuccess, "It worked! Now it's {0}.", "Deu certo! Agora é {0}."),
            new LocalizationEntry(ResultDropped, "It didn't take... back to {0}. The stone is spent, the item is whole.",
                "Não pegou... voltou para {0}. A pedra se gastou, o item tá inteiro."),
            new LocalizationEntry(ResultKept, "It didn't take... {0} stays the same.", "Não pegou... {0} continua igual."),
            new LocalizationEntry(OptionSuccess, "{0}: extra option now {1}/{2}.", "{0}: opção extra agora {1}/{2}."),
            new LocalizationEntry(OptionFailed, "The Life Stone didn't take; nothing changed on {0}.", "A Pedra da Vida não pegou; nada mudou em {0}."),
            new LocalizationEntry(MissingStone, "You need a {0} in the backpack.", "Falta {0} na mochila."),
            new LocalizationEntry(MissingGold, "Not enough gold: it costs {0}.", "Falta ouro: custa {0}."),
            new LocalizationEntry(MissingCharm, "No Lucky Charm in the backpack.", "Não tem Amuleto da Sorte na mochila."),
            new LocalizationEntry(MaxOption, "The extra option is already maxed.", "A opção extra já está no máximo."),
            new LocalizationEntry(Close, "Close", "Fechar"),

            // -- Tooltip (TooltipContentBuilder) --
            new LocalizationEntry(TooltipLevel, "Refine +{0}: +{1}% base stat", "Refino +{0}: +{1}% no atributo base"),
            new LocalizationEntry(TooltipOptionWeapon, "Extra option {0}/{1}: +{2}% damage", "Opção extra {0}/{1}: +{2}% de dano"),
            new LocalizationEntry(TooltipOptionArmour, "Extra option {0}/{1}: +{2}% max health", "Opção extra {0}/{1}: +{2}% de vida máxima"),
        };

        private static Dictionary<string, string> _fallbackByKey;
        private static LocalizationManager _mergedInto;

        /// <summary>Merges <see cref="Entries"/> into <paramref name="manager"/>, skipping keys it already has. Safe to call repeatedly.</summary>
        public static void EnsureRegistered(LocalizationManager manager)
        {
            if (manager == null || (ReferenceEquals(manager, _mergedInto) && manager.HasKey(Entries[0].Key)))
            {
                return;
            }

            manager.LoadEntries(Entries.Where(entry => !manager.HasKey(entry.Key)).ToList());
            _mergedInto = manager;
        }

        /// <summary>Localized text via <paramref name="manager"/> when it knows the key, else the pt-BR fallback, else the key.</summary>
        public static string Get(LocalizationManager manager, string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return string.Empty;
            }

            if (manager != null)
            {
                EnsureRegistered(manager);
                if (manager.HasKey(key))
                {
                    return manager.Get(key);
                }
            }

            return GetFallback(key) ?? key;
        }

        /// <summary><see cref="Get"/> + <see cref="string.Format(string, object[])"/>, tolerant of malformed translations.</summary>
        public static string Format(LocalizationManager manager, string key, params object[] args)
        {
            string pattern = Get(manager, key);
            try
            {
                return string.Format(pattern, args);
            }
            catch (System.FormatException)
            {
                return string.Format(GetFallback(key) ?? key, args);
            }
        }

        public static string GetFallback(string key)
        {
            if (key == null)
            {
                return null;
            }

            if (_fallbackByKey == null)
            {
                var table = new Dictionary<string, string>();
                foreach (LocalizationEntry entry in Entries)
                {
                    table[entry.Key] = entry.PtBR;
                }

                _fallbackByKey = table;
            }

            return _fallbackByKey.TryGetValue(key, out string text) ? text : null;
        }

        /// <summary>The stone's item name in the current language (via <see cref="ContentText"/>-style keys).</summary>
        public static string StoneName(LocalizationManager manager, RefineStone stone)
        {
            string id = RefineItems.IdOf(stone);
            return id == null ? string.Empty : Get(manager, ContentText.ItemNameKey(id));
        }
    }
}
