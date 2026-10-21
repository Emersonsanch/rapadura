using Rapadura.Core.DI;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Crafting;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.Skills;

namespace Rapadura.Gameplay.Content
{
    /// <summary>
    /// Localized names/descriptions for ScriptableObject content (items, skills, enemies, recipes,
    /// combos, skill trees) and the playable characters. Each content type has a key derived from its
    /// id, so no asset needs a new field:
    /// <c>item.&lt;itemId&gt;.name/.description</c>, <c>skill.&lt;skillId&gt;.*</c>,
    /// <c>enemy.&lt;asset name&gt;.name</c>, <c>recipe.&lt;recipeId&gt;.*</c>, <c>combo.&lt;comboId&gt;.*</c>,
    /// <c>tree.&lt;treeId&gt;.*</c> and <c>character.&lt;id&gt;.name/.class/.role/.lore</c>.
    ///
    /// Resolution: the live <see cref="LocalizationManager"/> when it has the key (merged from
    /// <see cref="ContentTexts"/> on first use; an external CSV/table wins), otherwise the asset's own
    /// pt-BR text — so an asset without an entry (or a test scene without a manager) shows exactly
    /// what it showed before. The parameterless extensions look the manager up in
    /// <see cref="ServiceLocator"/>; the overloads taking a manager are for pure presenters
    /// (a null manager there means "pt-BR asset text").
    ///
    /// Gameplay logic (ids, <c>CharacterSkillSet</c> name matching, save data, procedural names) keeps
    /// using the raw <c>DisplayName</c>; only UI goes through these accessors.
    /// </summary>
    public static class ContentText
    {
        public static string ItemNameKey(string itemId) => Key("item", itemId, "name");
        public static string ItemDescriptionKey(string itemId) => Key("item", itemId, "description");
        public static string SkillNameKey(string skillId) => Key("skill", skillId, "name");
        public static string SkillDescriptionKey(string skillId) => Key("skill", skillId, "description");
        public static string EnemyNameKey(string assetName) => Key("enemy", assetName, "name");
        public static string RecipeNameKey(string recipeId) => Key("recipe", recipeId, "name");
        public static string RecipeDescriptionKey(string recipeId) => Key("recipe", recipeId, "description");
        public static string ComboNameKey(string comboId) => Key("combo", comboId, "name");
        public static string ComboDescriptionKey(string comboId) => Key("combo", comboId, "description");
        public static string TreeNameKey(string treeId) => Key("tree", treeId, "name");
        public static string TreeDescriptionKey(string treeId) => Key("tree", treeId, "description");
        public static string CharacterNameKey(CharacterId id) => Key("character", CharacterIdSegment(id), "name");
        public static string CharacterClassKey(CharacterId id) => Key("character", CharacterIdSegment(id), "class");
        public static string CharacterRoleKey(CharacterId id) => Key("character", CharacterIdSegment(id), "role");
        public static string CharacterLoreKey(CharacterId id) => Key("character", CharacterIdSegment(id), "lore");

        /// <summary>Lower-case enum name (<c>maithe</c>, <c>icaro</c>...), the id segment of the <c>character.*</c> keys.</summary>
        public static string CharacterIdSegment(CharacterId id) => id.ToString().ToLowerInvariant();

        /// <summary>The live manager, or null when none is registered (test scenes, tools).</summary>
        public static LocalizationManager CurrentManager => ServiceLocator.TryGet(out LocalizationManager manager) ? manager : null;

        /// <summary>Text for <paramref name="key"/> via <paramref name="manager"/> when it has the key, else <paramref name="fallback"/> (the asset's pt-BR text).</summary>
        public static string Resolve(LocalizationManager manager, string key, string fallback)
        {
            if (manager != null && !string.IsNullOrEmpty(key))
            {
                ContentTexts.EnsureRegistered(manager);
                if (manager.HasKey(key))
                {
                    string text = manager.Get(key);
                    if (!string.IsNullOrEmpty(text))
                    {
                        return text;
                    }
                }
            }

            return fallback ?? string.Empty;
        }

        // -- Items --
        public static string LocalizedName(this ItemDefinition item) => item.LocalizedName(CurrentManager);
        public static string LocalizedDescription(this ItemDefinition item) => item.LocalizedDescription(CurrentManager);
        public static string LocalizedName(this ItemDefinition item, LocalizationManager manager) =>
            item == null ? string.Empty : Resolve(manager, ItemNameKey(item.ItemId), item.DisplayName);
        public static string LocalizedDescription(this ItemDefinition item, LocalizationManager manager) =>
            item == null ? string.Empty : Resolve(manager, ItemDescriptionKey(item.ItemId), item.Description);

        // -- Skills --
        public static string LocalizedName(this SkillDefinition skill) => skill.LocalizedName(CurrentManager);
        public static string LocalizedDescription(this SkillDefinition skill) => skill.LocalizedDescription(CurrentManager);
        public static string LocalizedName(this SkillDefinition skill, LocalizationManager manager) =>
            skill == null ? string.Empty : Resolve(manager, SkillNameKey(skill.SkillId), skill.DisplayName);
        public static string LocalizedDescription(this SkillDefinition skill, LocalizationManager manager) =>
            skill == null ? string.Empty : Resolve(manager, SkillDescriptionKey(skill.SkillId), skill.Description);

        // -- Enemies (keyed by asset name: EnemyDefinition has no id field) --
        public static string LocalizedName(this EnemyDefinition enemy) => enemy.LocalizedName(CurrentManager);
        public static string LocalizedName(this EnemyDefinition enemy, LocalizationManager manager) =>
            enemy == null ? string.Empty : Resolve(manager, EnemyNameKey(enemy.name), enemy.EnemyName);

        // -- Recipes --
        public static string LocalizedName(this RecipeDefinition recipe) => recipe.LocalizedName(CurrentManager);
        public static string LocalizedDescription(this RecipeDefinition recipe) => recipe.LocalizedDescription(CurrentManager);
        public static string LocalizedName(this RecipeDefinition recipe, LocalizationManager manager) =>
            recipe == null ? string.Empty : Resolve(manager, RecipeNameKey(recipe.RecipeId), recipe.DisplayName);
        public static string LocalizedDescription(this RecipeDefinition recipe, LocalizationManager manager) =>
            recipe == null ? string.Empty : Resolve(manager, RecipeDescriptionKey(recipe.RecipeId), recipe.Description);

        // -- Combos --
        public static string LocalizedName(this ComboDefinition combo) => combo.LocalizedName(CurrentManager);
        public static string LocalizedDescription(this ComboDefinition combo) => combo.LocalizedDescription(CurrentManager);
        public static string LocalizedName(this ComboDefinition combo, LocalizationManager manager) =>
            combo == null ? string.Empty : Resolve(manager, ComboNameKey(combo.ComboId), combo.DisplayName);
        public static string LocalizedDescription(this ComboDefinition combo, LocalizationManager manager) =>
            combo == null ? string.Empty : Resolve(manager, ComboDescriptionKey(combo.ComboId), combo.Description);

        // -- Skill trees --
        public static string LocalizedName(this SkillTreeDefinition tree) => tree.LocalizedName(CurrentManager);
        public static string LocalizedDescription(this SkillTreeDefinition tree) => tree.LocalizedDescription(CurrentManager);
        public static string LocalizedName(this SkillTreeDefinition tree, LocalizationManager manager) =>
            tree == null ? string.Empty : Resolve(manager, TreeNameKey(tree.TreeId), tree.DisplayName);
        public static string LocalizedDescription(this SkillTreeDefinition tree, LocalizationManager manager) =>
            tree == null ? string.Empty : Resolve(manager, TreeDescriptionKey(tree.TreeId), tree.Description);

        // -- Playable characters --
        public static string LocalizedName(this PlayableCharacter character) => character.LocalizedName(CurrentManager);
        public static string LocalizedClassName(this PlayableCharacter character) => character.LocalizedClassName(CurrentManager);
        public static string LocalizedRole(this PlayableCharacter character) => character.LocalizedRole(CurrentManager);
        public static string LocalizedLore(this PlayableCharacter character) => character.LocalizedLore(CurrentManager);
        public static string LocalizedName(this PlayableCharacter character, LocalizationManager manager) =>
            character == null ? string.Empty : Resolve(manager, CharacterNameKey(character.Id), character.DisplayName);
        public static string LocalizedClassName(this PlayableCharacter character, LocalizationManager manager) =>
            character == null ? string.Empty : Resolve(manager, CharacterClassKey(character.Id), character.ClassName);
        public static string LocalizedRole(this PlayableCharacter character, LocalizationManager manager) =>
            character == null ? string.Empty : Resolve(manager, CharacterRoleKey(character.Id), character.Role);
        public static string LocalizedLore(this PlayableCharacter character, LocalizationManager manager) =>
            character == null ? string.Empty : Resolve(manager, CharacterLoreKey(character.Id), character.Lore);

        private static string Key(string type, string id, string field) =>
            string.IsNullOrEmpty(id) ? null : $"{type}.{id}.{field}";
    }
}
