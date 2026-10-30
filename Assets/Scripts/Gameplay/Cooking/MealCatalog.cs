using System.Collections.Generic;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Quests;
using Rapadura.Gameplay.Skills;
using Rapadura.Gameplay.World.Biomes;

namespace Rapadura.Gameplay.Cooking
{
    /// <summary>One ingredient of a <see cref="MealRecipeSpec"/>, by <c>ItemDefinition.ItemId</c>.</summary>
    public sealed class MealIngredientSpec
    {
        public readonly string ItemId;
        public readonly int Quantity;

        public MealIngredientSpec(string itemId, int quantity)
        {
            ItemId = itemId;
            Quantity = quantity;
        }
    }

    /// <summary>
    /// A dish of Vó Zefa's "Fogão a lenha", as plain data. Its effects are ordinary
    /// <see cref="StatModifierDefinition"/>s applied to the hero's <see cref="BuffController"/> during an
    /// encounter (<see cref="MealEffectApplier"/>), so they go through the same stat plumbing as skills,
    /// attributes and equipment.
    /// </summary>
    public sealed class MealRecipeSpec
    {
        public readonly string RecipeId;
        /// <summary>Biome materials and shop basics (wood for the stove, herbs, fibre...), taken from the bag.</summary>
        public readonly IReadOnlyList<MealIngredientSpec> Ingredients;
        /// <summary>"Mantimentos da venda" (corn meal, beans, coffee...): paid in <see cref="MealCatalog.ProvisionsItemId"/>.</summary>
        public readonly int ProvisionsCost;
        /// <summary>Encounters the meal lasts once eaten (every resolved encounter counts, see <see cref="CookingRules.CountEncounter"/>).</summary>
        public readonly int Encounters;
        /// <summary>Applied for the whole encounter (<c>durationSeconds</c> 0 = until the arena unloads).</summary>
        public readonly IReadOnlyList<StatModifierDefinition> Effects;
        /// <summary>Applied once, as the fight starts, for their own <c>durationSeconds</c> (Rapadura's burst). May be empty.</summary>
        public readonly IReadOnlyList<StatModifierDefinition> OpeningBurst;
        /// <summary>The cousin who loves this dish (extra <see cref="FavouriteBonus"/>), or null.</summary>
        public readonly CharacterId? FavouriteOf;
        /// <summary>Extra effects when <see cref="FavouriteOf"/> eats it. Same timing rule as <see cref="Effects"/> (or <see cref="OpeningBurst"/> when <see cref="FavouriteBonusIsBurst"/>).</summary>
        public readonly IReadOnlyList<StatModifierDefinition> FavouriteBonus;
        public readonly bool FavouriteBonusIsBurst;
        /// <summary>Known from the start of a new game.</summary>
        public readonly bool Starter;
        /// <summary>Completing this quest teaches the recipe (any cousin), or null.</summary>
        public readonly string UnlockQuestId;
        /// <summary>
        /// Region whose unlocking lets Vó Zefa teach the dish to <see cref="FavouriteOf"/> at the stove (null = no
        /// gate). Keeps the lesson in step with the quest route: the dish's ingredients and its quest belong there.
        /// </summary>
        public readonly BiomeType? LessonRegion;

        public MealRecipeSpec(string recipeId, MealIngredientSpec[] ingredients, int provisionsCost, int encounters,
            StatModifierDefinition[] effects, StatModifierDefinition[] openingBurst,
            CharacterId? favouriteOf, StatModifierDefinition[] favouriteBonus, bool favouriteBonusIsBurst,
            bool starter, string unlockQuestId, BiomeType? lessonRegion)
        {
            RecipeId = recipeId;
            Ingredients = ingredients ?? new MealIngredientSpec[0];
            ProvisionsCost = provisionsCost;
            Encounters = encounters;
            Effects = effects ?? new StatModifierDefinition[0];
            OpeningBurst = openingBurst ?? new StatModifierDefinition[0];
            FavouriteOf = favouriteOf;
            FavouriteBonus = favouriteBonus ?? new StatModifierDefinition[0];
            FavouriteBonusIsBurst = favouriteBonusIsBurst;
            Starter = starter;
            UnlockQuestId = unlockQuestId;
            LessonRegion = lessonRegion;
        }

        /// <summary>"meal_cuscuz" → "meal.cuscuz" (text keys: <c>.name</c>, <c>.effect</c>, <c>.favourite</c>).</summary>
        public string KeyBase => "meal." + (RecipeId.StartsWith(MealCatalog.RecipeIdPrefix) ? RecipeId.Substring(MealCatalog.RecipeIdPrefix.Length) : RecipeId);
        public string NameKey => KeyBase + ".name";
        public string EffectKey => KeyBase + ".effect";
        public string FavouriteKey => KeyBase + ".favourite";
    }

    /// <summary>
    /// "Cozinha da Vó Zefa": the dishes of the village's wood stove (<c>VillageKitchenController</c>, reached from the
    /// map's village hub). Defined in code like <see cref="RegionQuestCatalog"/>, so no asset/seeder is needed.
    /// Numbers and the reasoning behind them: <c>Docs/Balance/README.md</c> §10.
    /// <list type="bullet">
    /// <item>Two dishes are known from the start (<see cref="MealRecipeSpec.Starter"/>); every other one is taught by
    /// the first region quest of its region, or by Vó Zefa herself at the stove to the cousin who loves it once that
    /// region is open.</item>
    /// <item>Each cousin has a favourite (<see cref="MealRecipeSpec.FavouriteOf"/>): a small extra bonus and a line of
    /// Vó's when they cook it. Maithe's (café coado) is a starter; cuscuz is everyone's and nobody's.</item>
    /// <item>Stats used: only <see cref="StatType"/>s some system reads (PlayerStats regen/max, DamageCalculator
    /// defense, SkillManager cooldowns, PlayerBasicAttack speed, ThornsRules, OutgoingDamage crit), each in the
    /// application its reader expects (CooldownReduction/AttackSpeed/Thorns/CriticalChance as fractions).</item>
    /// </list>
    /// </summary>
    public static class MealCatalog
    {
        public const string RecipeIdPrefix = "meal_";

        public const string Cuscuz = "meal_cuscuz";
        public const string CafeCoado = "meal_cafe_coado";
        public const string Pamonha = "meal_pamonha";
        public const string Rapadura = "meal_rapadura";
        public const string BaiaoDeDois = "meal_baiao_de_dois";
        public const string QueijoCoalho = "meal_queijo_coalho";

        /// <summary>The shop's currency (<c>shop_general_store._currencyItemId</c>): what "mantimentos da venda" cost.</summary>
        public const string ProvisionsItemId = "item_gold";

        public const string Wood = "item_wood";
        public const string Herb = "item_herb";
        public const string Fiber = "item_fiber";
        public const string Stone = "item_stone";
        public const string IronOre = "item_iron_ore";
        public const string CookedMeat = "item_cooked_meat";

        /// <summary>Seconds Rapadura's attack-speed burst lasts from the start of the fight (covers the "Prepare-se!" grace and the opening exchange).</summary>
        public const float RapaduraBurstSeconds = 20f;

        public static readonly IReadOnlyList<MealRecipeSpec> Recipes = new List<MealRecipeSpec>
        {
            // Cuscuz: flocão steamed on the stove — the stamina to run all day.
            new MealRecipeSpec(Cuscuz,
                new[] { new MealIngredientSpec(Wood, 1), new MealIngredientSpec(Herb, 1) }, 2, 3,
                new[] { Effect("Cuscuz", StatType.StaminaRegen, ModifierApplication.PercentAdditive, 0.30f) }, null,
                null, null, false,
                starter: true, unlockQuestId: null, lessonRegion: null),

            // Café coado: strained through the cloth (fibre) — skills come back sooner. Maithe's: +crit.
            new MealRecipeSpec(CafeCoado,
                new[] { new MealIngredientSpec(Wood, 1), new MealIngredientSpec(Fiber, 1) }, 2, 3,
                new[] { Effect("Café coado", StatType.CooldownReduction, ModifierApplication.Flat, 0.06f) }, null,
                CharacterId.Maithe, new[] { Effect("Café da Maithe", StatType.CriticalChance, ModifierApplication.Flat, 0.03f) }, false,
                starter: true, unlockQuestId: null, lessonRegion: null),

            // Pamonha: wrapped in corn husk (fibre) — more health. Maria's: +HP/s.
            new MealRecipeSpec(Pamonha,
                new[] { new MealIngredientSpec(Wood, 2), new MealIngredientSpec(Fiber, 2) }, 3, 3,
                new[] { Effect("Pamonha", StatType.MaxHealth, ModifierApplication.PercentAdditive, 0.10f) }, null,
                CharacterId.Maria, new[] { Effect("Pamonha da Maria", StatType.HealthRegen, ModifierApplication.Flat, 1f) }, false,
                starter: false, unlockQuestId: RegionQuestCatalog.ForestFirst, lessonRegion: BiomeType.Forest),

            // Rapadura: cane boiled in the tacho over a lot of wood, the stone of the mill — a sugar rush at the
            // start of the fight. Ícaro's: runs faster during the rush. One more encounter: the effect is short.
            new MealRecipeSpec(Rapadura,
                new[] { new MealIngredientSpec(Wood, 2), new MealIngredientSpec(Stone, 1) }, 3, 4,
                null, new[] { Effect("Rapadura", StatType.AttackSpeed, ModifierApplication.PercentAdditive, 0.25f, RapaduraBurstSeconds) },
                CharacterId.Icaro, new[] { Effect("Rapadura do Ícaro", StatType.MoveSpeed, ModifierApplication.PercentAdditive, 0.08f, RapaduraBurstSeconds) }, true,
                starter: false, unlockQuestId: RegionQuestCatalog.DesertFirst, lessonRegion: BiomeType.Desert),

            // Baião de dois: rice, beans, carne de sol and coentro — sturdier. Joaquim's: +max health.
            new MealRecipeSpec(BaiaoDeDois,
                new[] { new MealIngredientSpec(Wood, 2), new MealIngredientSpec(Herb, 2), new MealIngredientSpec(CookedMeat, 1) }, 4, 3,
                new[] { Effect("Baião de dois", StatType.Defense, ModifierApplication.PercentAdditive, 0.10f) }, null,
                CharacterId.Joaquim, new[] { Effect("Baião do Joaquim", StatType.MaxHealth, ModifierApplication.PercentAdditive, 0.05f) }, false,
                starter: false, unlockQuestId: RegionQuestCatalog.MountainFirst, lessonRegion: BiomeType.Mountain),

            // Queijo coalho assado: on an iron skewer over the coals, with oregano — whoever bites, gets burnt
            // (thorns-lite). Lavine's: +mana regen.
            new MealRecipeSpec(QueijoCoalho,
                new[] { new MealIngredientSpec(Wood, 1), new MealIngredientSpec(IronOre, 1), new MealIngredientSpec(Herb, 1) }, 4, 3,
                new[] { Effect("Queijo coalho assado", StatType.Thorns, ModifierApplication.PercentAdditive, 0.08f) }, null,
                CharacterId.Lavine, new[] { Effect("Queijo da Lavine", StatType.ManaRegen, ModifierApplication.PercentAdditive, 0.20f) }, false,
                starter: false, unlockQuestId: RegionQuestCatalog.CavesFirst, lessonRegion: BiomeType.Caves),
        };

        private static Dictionary<string, MealRecipeSpec> _byId;

        public static MealRecipeSpec Get(string recipeId)
        {
            if (string.IsNullOrEmpty(recipeId))
            {
                return null;
            }

            if (_byId == null)
            {
                var table = new Dictionary<string, MealRecipeSpec>();
                foreach (MealRecipeSpec recipe in Recipes)
                {
                    table[recipe.RecipeId] = recipe;
                }

                _byId = table;
            }

            return _byId.TryGetValue(recipeId, out MealRecipeSpec spec) ? spec : null;
        }

        /// <summary>The dish <paramref name="hero"/> loves, or null.</summary>
        public static MealRecipeSpec FavouriteOf(CharacterId hero)
        {
            foreach (MealRecipeSpec recipe in Recipes)
            {
                if (recipe.FavouriteOf == hero)
                {
                    return recipe;
                }
            }

            return null;
        }

        /// <summary>The dish the given quest teaches, or null.</summary>
        public static MealRecipeSpec TaughtByQuest(string questId)
        {
            if (string.IsNullOrEmpty(questId))
            {
                return null;
            }

            foreach (MealRecipeSpec recipe in Recipes)
            {
                if (recipe.UnlockQuestId == questId)
                {
                    return recipe;
                }
            }

            return null;
        }

        private static StatModifierDefinition Effect(string displayName, StatType stat, ModifierApplication application, float value, float durationSeconds = 0f)
        {
            return new StatModifierDefinition
            {
                displayName = displayName,
                affectedStat = stat,
                application = application,
                value = value,
                // 0 = permanent for BuffController: lasts until the arena scene (and the hero in it) unloads.
                durationSeconds = durationSeconds,
                isDebuff = false,
            };
        }
    }
}
