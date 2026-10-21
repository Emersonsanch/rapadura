using System;
using System.Collections.Generic;
using System.Linq;
using Rapadura.Core.Localization;

namespace Rapadura.UI.Common
{
    /// <summary>
    /// Localization keys (en + pt-BR) for the general screens: HUD, Shop, Pause, Main Menu,
    /// Crafting, Skill Tree, Save/Load, Credits, Death and Character select. Same self-contained pattern as
    /// <see cref="TooltipLocalization"/>/<c>SettingsMenuTexts</c>: merged into the live
    /// <see cref="LocalizationManager"/> on first use (only keys it doesn't have yet, so an external
    /// CSV/table wins), and <see cref="Get"/> falls back to pt-BR when no manager is registered.
    ///
    /// Static UXML text is localized by convention through <see cref="UiDocumentLocalizer"/>
    /// (class <c>loc</c> + <c>view-data-key="ui.…"</c>); text built in code uses the constants below.
    /// A few keys live in the core table (<c>ui.pause.title/resume</c>, <c>ui.menu.start/settings/quit</c>)
    /// and are only referenced here, never redefined.
    /// </summary>
    public static class UiTexts
    {
        // -- HUD --
        public const string HudHealth = "ui.hud.health";
        public const string HudStamina = "ui.hud.stamina";
        public const string HudMana = "ui.hud.mana";
        public const string HudLevel = "ui.hud.level";
        public const string HudRun = "ui.hud.run";
        public const string HudJump = "ui.hud.jump";
        public const string HudQuests = "ui.hud.quests";
        public const string HudInventory = "ui.hud.inventory";
        public const string HudSkills = "ui.hud.skills";
        public const string HudMenu = "ui.hud.menu";

        // -- Shop --
        public const string ShopGold = "ui.shop.gold";
        public const string ShopBuy = "ui.shop.buy";
        public const string ShopSell = "ui.shop.sell";
        public const string ShopBuyRow = "ui.shop.buy_row";
        public const string ShopSellRow = "ui.shop.sell_row";

        // -- Pause (title/resume are core keys) --
        public const string PauseTitle = "ui.pause.title";
        public const string PauseResume = "ui.pause.resume";
        public const string PauseSave = "ui.pause.save";
        public const string PauseAchievements = "ui.pause.achievements";
        public const string PauseCrafting = "ui.pause.crafting";
        public const string PauseMainMenu = "ui.pause.main_menu";

        // -- Main menu (start/settings/quit are core keys) --
        public const string MenuStart = "ui.menu.start";
        public const string MenuSettings = "ui.menu.settings";
        public const string MenuQuit = "ui.menu.quit";
        public const string MenuContinue = "ui.menu.continue";
        public const string MenuCredits = "ui.menu.credits";
        /// <summary>"Novo jogo" (the title screen also has "Continuar", so not the core "Iniciar Jogo" of <see cref="MenuStart"/>).</summary>
        public const string MenuNewGame = "ui.menu.new_game";

        // -- Crafting --
        public const string CraftingTitle = "ui.crafting.title";
        public const string CraftingRecipes = "ui.crafting.recipes";
        public const string CraftingMaterials = "ui.crafting.materials";
        public const string CraftingCraft = "ui.crafting.craft";
        public const string CraftingEmpty = "ui.crafting.empty";
        public const string CraftingRequires = "ui.crafting.requires";
        public const string CraftingResult = "ui.crafting.result";
        public const string CraftingCrafted = "ui.crafting.crafted";
        public const string CraftingCraftedItem = "ui.crafting.crafted_item";
        public const string CraftingFailed = "ui.crafting.failed";
        public const string CraftingUnknownRecipe = "ui.crafting.reason.unknown_recipe";
        public const string CraftingMissingMaterials = "ui.crafting.reason.missing_materials";
        public const string CraftingNoInventory = "ui.crafting.reason.no_inventory";
        /// <summary>Prefix + snake_case <c>CraftingStationType</c> name (see <see cref="TooltipLocalization.ToSnakeCase"/>).</summary>
        public const string CraftingStationPrefix = "ui.crafting.station.";

        // -- Skill tree --
        public const string SkillTreeClose = "ui.skilltree.close";
        public const string SkillTreePoints = "ui.skilltree.points";
        public const string SkillTreeSelectSkill = "ui.skilltree.select_skill";
        public const string SkillTreeStats = "ui.skilltree.stats";
        public const string SkillTreeLearnCost = "ui.skilltree.learn_cost";
        public const string SkillTreeUpgradeCost = "ui.skilltree.upgrade_cost";
        public const string SkillTreeMaxLevel = "ui.skilltree.max_level";

        // -- Save / Load --
        public const string SaveLoadTitleSave = "ui.saveload.title_save";
        public const string SaveLoadTitleLoad = "ui.saveload.title_load";
        public const string SaveLoadClose = "ui.saveload.close";
        public const string SaveLoadSlot = "ui.saveload.slot";
        public const string SaveLoadSaved = "ui.saveload.saved";
        public const string SaveLoadEmpty = "ui.saveload.empty";
        public const string SaveLoadOverwrite = "ui.saveload.overwrite";
        public const string SaveLoadSave = "ui.saveload.save";
        public const string SaveLoadLoad = "ui.saveload.load";
        public const string SaveLoadDelete = "ui.saveload.delete";

        // -- Credits --
        public const string CreditsTitle = "ui.credits.title";
        public const string CreditsBack = "ui.credits.back";
        public const string CreditsSectionTeam = "ui.credits.section.team";
        public const string CreditsSectionTools = "ui.credits.section.tools";
        public const string CreditsSectionThirdParty = "ui.credits.section.third_party";
        public const string CreditsSectionThanks = "ui.credits.section.thanks";

        // -- Death screen --
        public const string DeathTitle = "ui.death.title";
        public const string DeathMessage = "ui.death.message";
        public const string DeathContinue = "ui.death.continue";
        public const string DeathLoad = "ui.death.load";
        public const string DeathReturnToMap = "ui.death.return_to_map";
        public const string DeathCountdown = "ui.death.countdown";

        // -- Character select (hero names/classes/lore stay pt-BR content, see Docs/Localization.md) --
        public const string CharacterSelectTitle = "ui.character_select.title";
        public const string CharacterSelectPrompt = "ui.character_select.prompt";
        public const string CharacterSelectConfirm = "ui.character_select.confirm";
        public const string CharacterSelectContinueWith = "ui.character_select.continue_with";

        public static readonly List<LocalizationEntry> Entries = new List<LocalizationEntry>
        {
            new LocalizationEntry(HudHealth, "Health", "Vida"),
            new LocalizationEntry(HudStamina, "Stamina", "Stamina"),
            new LocalizationEntry(HudMana, "Mana", "Mana"),
            new LocalizationEntry(HudLevel, "Lv. {0}", "Nv. {0}"),
            new LocalizationEntry(HudRun, "Run", "Correr"),
            new LocalizationEntry(HudJump, "Jump", "Pular"),
            new LocalizationEntry(HudQuests, "Quests", "Missões"),
            new LocalizationEntry(HudInventory, "Backpack", "Mochila"),
            new LocalizationEntry(HudSkills, "Skills", "Skills"),
            new LocalizationEntry(HudMenu, "Menu", "Menu"),

            new LocalizationEntry(ShopGold, "Gold:", "Ouro:"),
            new LocalizationEntry(ShopBuy, "Buy", "Comprar"),
            new LocalizationEntry(ShopSell, "Sell", "Vender"),
            new LocalizationEntry(ShopBuyRow, "{0}  -  {1}  (x{2})", "{0}  -  {1}  (x{2})"),
            new LocalizationEntry(ShopSellRow, "{0} x{1}  -  {2} each", "{0} x{1}  -  {2} cada"),

            new LocalizationEntry(PauseSave, "Save", "Salvar"),
            new LocalizationEntry(PauseAchievements, "Achievements", "Conquistas"),
            new LocalizationEntry(PauseCrafting, "Crafting", "Criação"),
            new LocalizationEntry(PauseMainMenu, "Main Menu", "Menu Principal"),

            new LocalizationEntry(MenuContinue, "Continue", "Continuar"),
            new LocalizationEntry(MenuCredits, "Credits", "Créditos"),
            new LocalizationEntry(MenuNewGame, "New game", "Novo jogo"),

            new LocalizationEntry(CraftingTitle, "Crafting", "Criação"),
            new LocalizationEntry(CraftingRecipes, "Recipes", "Receitas"),
            new LocalizationEntry(CraftingMaterials, "Materials", "Materiais"),
            new LocalizationEntry(CraftingCraft, "Craft", "Criar"),
            new LocalizationEntry(CraftingEmpty, "No known recipes.", "Nenhuma receita conhecida."),
            new LocalizationEntry(CraftingRequires, "Requires: {0}", "Requer: {0}"),
            new LocalizationEntry(CraftingResult, "Result: {0} x{1}", "Resultado: {0} x{1}"),
            new LocalizationEntry(CraftingCrafted, "Crafted!", "Criado!"),
            new LocalizationEntry(CraftingCraftedItem, "Crafted: {0} x{1}!", "Criado: {0} x{1}!"),
            new LocalizationEntry(CraftingFailed, "Couldn't craft (no room in the backpack?)", "Não foi possível criar (sem espaço no inventário?)"),
            new LocalizationEntry(CraftingUnknownRecipe, "Unknown recipe", "Receita desconhecida"),
            new LocalizationEntry(CraftingMissingMaterials, "Not enough materials", "Materiais insuficientes"),
            new LocalizationEntry(CraftingNoInventory, "Inventory unavailable", "Inventário indisponível"),
            new LocalizationEntry(CraftingStationPrefix + "none", "No station", "Nenhuma estação"),
            new LocalizationEntry(CraftingStationPrefix + "campfire", "Campfire", "Fogueira"),
            new LocalizationEntry(CraftingStationPrefix + "workbench", "Workbench", "Bancada"),
            new LocalizationEntry(CraftingStationPrefix + "forge", "Forge", "Forja"),
            new LocalizationEntry(CraftingStationPrefix + "alchemy_table", "Alchemy table", "Mesa de alquimia"),

            new LocalizationEntry(SkillTreeClose, "Close", "Fechar"),
            new LocalizationEntry(SkillTreePoints, "Available points: {0}", "Pontos disponíveis: {0}"),
            new LocalizationEntry(SkillTreeSelectSkill, "Select a skill", "Selecione uma skill"),
            new LocalizationEntry(SkillTreeStats, "Cost: {0} mana / {1} energy — Cooldown: {2}s", "Custo: {0} mana / {1} energia — Cooldown: {2}s"),
            new LocalizationEntry(SkillTreeLearnCost, "Learn (1 point)", "Aprender (1 ponto)"),
            new LocalizationEntry(SkillTreeUpgradeCost, "Upgrade (1 point)", "Evoluir (1 ponto)"),
            new LocalizationEntry(SkillTreeMaxLevel, "Max level", "Nível máximo"),

            new LocalizationEntry(SaveLoadTitleSave, "Save Game", "Salvar Jogo"),
            new LocalizationEntry(SaveLoadTitleLoad, "Load Game", "Carregar Jogo"),
            new LocalizationEntry(SaveLoadClose, "Close", "Fechar"),
            new LocalizationEntry(SaveLoadSlot, "Slot {0}", "Slot {0}"),
            new LocalizationEntry(SaveLoadSaved, "Saved", "Salvo"),
            new LocalizationEntry(SaveLoadEmpty, "Empty", "Vazio"),
            new LocalizationEntry(SaveLoadOverwrite, "Overwrite", "Sobrescrever"),
            new LocalizationEntry(SaveLoadSave, "Save", "Salvar"),
            new LocalizationEntry(SaveLoadLoad, "Load", "Carregar"),
            new LocalizationEntry(SaveLoadDelete, "Delete", "Excluir"),

            new LocalizationEntry(CreditsTitle, "Credits", "Créditos"),
            new LocalizationEntry(CreditsBack, "Back", "Voltar"),
            new LocalizationEntry(CreditsSectionTeam, "Team", "Equipe"),
            new LocalizationEntry(CreditsSectionTools, "Tools", "Ferramentas"),
            new LocalizationEntry(CreditsSectionThirdParty, "Third-party libraries and licenses", "Bibliotecas e licenças de terceiros"),
            new LocalizationEntry(CreditsSectionThanks, "Thanks", "Agradecimentos"),

            new LocalizationEntry(DeathTitle, "You fell...", "Você caiu..."),
            new LocalizationEntry(DeathMessage, "The sertão shows no mercy, but the journey goes on.", "O sertão não perdoa, mas a jornada continua."),
            new LocalizationEntry(DeathContinue, "Continue", "Continuar"),
            new LocalizationEntry(DeathLoad, "Load game", "Carregar jogo"),
            new LocalizationEntry(DeathReturnToMap, "Back to the map", "Voltar ao mapa"),
            new LocalizationEntry(DeathCountdown, "Continuing in {0}s", "Continuando em {0}s"),

            new LocalizationEntry(CharacterSelectTitle, "Choose your Hero", "Escolha seu Herói"),
            new LocalizationEntry(CharacterSelectPrompt, "Select a character to see their story.", "Selecione um personagem para ver a história."),
            new LocalizationEntry(CharacterSelectConfirm, "Confirm", "Confirmar"),
            new LocalizationEntry(CharacterSelectContinueWith, "Continue as {0}", "Continuar com {0}"),
        };

        private static Dictionary<string, string> _fallbackByKey;
        private static LocalizationManager _mergedInto;

        /// <summary>Merges <see cref="Entries"/> into <paramref name="manager"/>, skipping keys it already has (external tables take precedence). Safe to call repeatedly.</summary>
        public static void EnsureRegistered(LocalizationManager manager)
        {
            if (manager == null || (ReferenceEquals(manager, _mergedInto) && manager.HasKey(Entries[0].Key)))
            {
                return;
            }

            manager.LoadEntries(Entries.Where(entry => !manager.HasKey(entry.Key)).ToList());
            _mergedInto = manager;
        }

        /// <summary>Localized text via <paramref name="manager"/> when it knows the key, else the pt-BR fallback (ours or the core table's), else the key.</summary>
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
            catch (FormatException)
            {
                return string.Format(GetFallback(key) ?? key, args);
            }
        }

        /// <summary>pt-BR text for <paramref name="key"/> from <see cref="Entries"/> or the core table, or null if unknown.</summary>
        public static string GetFallback(string key)
        {
            if (_fallbackByKey == null)
            {
                var table = new Dictionary<string, string>();
                foreach (LocalizationEntry entry in LocalizationManager.DefaultEntries.All)
                {
                    if (entry != null && !string.IsNullOrEmpty(entry.Key) && entry.Key.StartsWith("ui.", StringComparison.Ordinal))
                    {
                        table[entry.Key] = entry.PtBR;
                    }
                }

                foreach (LocalizationEntry entry in Entries)
                {
                    if (!table.ContainsKey(entry.Key))
                    {
                        table[entry.Key] = entry.PtBR;
                    }
                }

                _fallbackByKey = table;
            }

            return key != null && _fallbackByKey.TryGetValue(key, out string text) ? text : null;
        }
    }
}
