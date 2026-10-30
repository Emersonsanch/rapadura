using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rapadura.Core.DI;
using Rapadura.Core.Events;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Cooking;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.Quests;
using Rapadura.Gameplay.Skills;
using Rapadura.Gameplay.World.Biomes;
using Rapadura.Gameplay.World.Map;
using Rapadura.UI.WorldMap;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// "Cozinha da Vó Zefa" (<see cref="MealCatalog"/>, <see cref="CookingRules"/>, <see cref="MealService"/>,
    /// <see cref="KitchenPresenter"/>): recipe validation, ingredient consumption, one meal at a time, the countdown
    /// per encounter, learning (starters, quests, Vó's lessons), save round trip and the effects on a BuffController.
    /// </summary>
    public class CookingTests
    {
        private readonly List<GameObject> _created = new List<GameObject>();
        private readonly List<MealService> _services = new List<MealService>();

        [SetUp]
        public void SetUp()
        {
            EventBus.Clear();
            ServiceLocator.Clear();
            ItemDatabase.Invalidate();
        }

        [TearDown]
        public void TearDown()
        {
            foreach (MealService service in _services)
            {
                service.Shutdown();
            }

            foreach (GameObject go in _created)
            {
                Object.DestroyImmediate(go);
            }

            _services.Clear();
            _created.Clear();
            EventBus.Clear();
            ServiceLocator.Clear();
        }

        // ------------------------------------------------------------------
        // Catalog content
        // ------------------------------------------------------------------

        [Test]
        public void Catalog_TwoStarters_EveryOtherDishHasAQuestAndALesson()
        {
            Assert.AreEqual(2, MealCatalog.Recipes.Count(r => r.Starter));
            CollectionAssert.AreEquivalent(new[] { MealCatalog.Cuscuz, MealCatalog.CafeCoado }, MealCatalog.Recipes.Where(r => r.Starter).Select(r => r.RecipeId));

            var regionQuestIds = new HashSet<string>(RegionQuestCatalog.Quests.Select(q => q.QuestId));
            // Dishes taught by a catch at "Pesca no açude" have their own route (FishingTests).
            foreach (MealRecipeSpec recipe in MealCatalog.Recipes.Where(r => !r.Starter && r.TaughtByCatchItemId == null))
            {
                Assert.IsTrue(regionQuestIds.Contains(recipe.UnlockQuestId), $"{recipe.RecipeId}: unlock quest {recipe.UnlockQuestId} is not a region quest");
                Assert.IsTrue(recipe.FavouriteOf.HasValue, $"{recipe.RecipeId}: no cousin to be taught by Vó Zefa");
                Assert.IsTrue(recipe.LessonRegion.HasValue, $"{recipe.RecipeId}: lesson has no region gate");
                RegionQuestSpec quest = RegionQuestCatalog.Quests.First(q => q.QuestId == recipe.UnlockQuestId);
                Assert.AreEqual(recipe.LessonRegion.Value, quest.Region, $"{recipe.RecipeId}: quest and lesson open in different regions");
            }
        }

        [Test]
        public void Catalog_EveryCousinHasExactlyOneFavourite()
        {
            foreach (CharacterId hero in System.Enum.GetValues(typeof(CharacterId)))
            {
                Assert.AreEqual(1, MealCatalog.Recipes.Count(r => r.FavouriteOf == hero), $"{hero} needs exactly one favourite dish");
                Assert.IsNotNull(MealCatalog.FavouriteOf(hero));
                Assert.IsNotEmpty(MealCatalog.FavouriteOf(hero).FavouriteBonus, $"{hero}'s favourite has no extra bonus");
            }
        }

        [Test]
        public void Catalog_IngredientsAreRealItems_AndEveryDishLastsAndDoesSomething()
        {
            foreach (MealRecipeSpec recipe in MealCatalog.Recipes)
            {
                Assert.Greater(recipe.Encounters, 0, recipe.RecipeId);
                Assert.IsTrue(recipe.Effects.Count + recipe.OpeningBurst.Count > 0, $"{recipe.RecipeId} has no effect");
                Assert.IsTrue(recipe.Ingredients.Any(i => i.ItemId == MealCatalog.Wood), $"{recipe.RecipeId}: a wood stove needs wood");
                foreach (MealIngredientSpec ingredient in recipe.Ingredients)
                {
                    Assert.IsNotNull(ItemDatabase.GetById(ingredient.ItemId), $"{recipe.RecipeId}: {ingredient.ItemId} is not in Resources/Items");
                    Assert.Greater(ingredient.Quantity, 0);
                }
            }

            Assert.IsNotNull(ItemDatabase.GetById(MealCatalog.ProvisionsItemId));
        }

        [Test]
        public void Catalog_EffectsUseStatsTheirReadersUnderstand()
        {
            foreach (MealRecipeSpec recipe in MealCatalog.Recipes)
            {
                foreach (StatModifierDefinition effect in recipe.Effects.Concat(recipe.OpeningBurst).Concat(recipe.FavouriteBonus))
                {
                    Assert.IsFalse(effect.isDebuff, recipe.RecipeId);
                    Assert.Greater(effect.value, 0f, recipe.RecipeId);
                    // Convention: 0-based fraction stats are authored Flat (the reader sums both since Balance §7.6).
                    if (effect.affectedStat == StatType.CooldownReduction)
                    {
                        Assert.AreEqual(ModifierApplication.Flat, effect.application, $"{recipe.RecipeId}: CDR must be Flat");
                    }
                }

                foreach (StatModifierDefinition effect in recipe.Effects)
                {
                    Assert.AreEqual(0f, effect.durationSeconds, $"{recipe.RecipeId}: encounter effects last the whole fight");
                }

                foreach (StatModifierDefinition effect in recipe.OpeningBurst)
                {
                    Assert.Greater(effect.durationSeconds, 0f, $"{recipe.RecipeId}: a burst must run out");
                }
            }
        }

        // ------------------------------------------------------------------
        // Validation and consumption
        // ------------------------------------------------------------------

        [Test]
        public void Validate_ChecksLearnedThenIngredientsThenProvisions()
        {
            MealRecipeSpec cuscuz = MealCatalog.Get(MealCatalog.Cuscuz);
            var bag = Bag((MealCatalog.Wood, 1), (MealCatalog.Herb, 1), (MealCatalog.ProvisionsItemId, cuscuz.ProvisionsCost));

            Assert.AreEqual(CookCheck.UnknownRecipe, CookingRules.Validate(null, true, Count(bag)));
            Assert.AreEqual(CookCheck.NotLearned, CookingRules.Validate(cuscuz, false, Count(bag)));
            Assert.AreEqual(CookCheck.Ok, CookingRules.Validate(cuscuz, true, Count(bag)));

            bag[MealCatalog.Herb] = 0;
            Assert.AreEqual(CookCheck.MissingIngredients, CookingRules.Validate(cuscuz, true, Count(bag)));

            bag[MealCatalog.Herb] = 1;
            bag[MealCatalog.ProvisionsItemId] = cuscuz.ProvisionsCost - 1;
            Assert.AreEqual(CookCheck.NotEnoughProvisions, CookingRules.Validate(cuscuz, true, Count(bag)));
        }

        [Test]
        public void TryCook_TakesExactlyTheIngredients_AndAFailedCookTakesNothing()
        {
            MealService meals = NewService();
            MealRecipeSpec baiao = MealCatalog.Get(MealCatalog.BaiaoDeDois);
            meals.Learn(baiao.RecipeId, MealLearnSource.Quest);

            var bag = Bag((MealCatalog.Wood, 5), (MealCatalog.Herb, 2), (MealCatalog.CookedMeat, 0), (MealCatalog.ProvisionsItemId, 10));
            Assert.AreEqual(CookCheck.MissingIngredients, meals.TryCook(baiao.RecipeId, Count(bag), Remove(bag), out _));
            Assert.AreEqual(5, bag[MealCatalog.Wood], "a failed cook must not burn the wood");
            Assert.AreEqual(10, bag[MealCatalog.ProvisionsItemId]);
            Assert.IsFalse(meals.Active.IsActive);

            bag[MealCatalog.CookedMeat] = 1;
            Assert.AreEqual(CookCheck.Ok, meals.TryCook(baiao.RecipeId, Count(bag), Remove(bag), out ServeOutcome outcome));
            Assert.AreEqual(ServeOutcome.Started, outcome);
            Assert.AreEqual(3, bag[MealCatalog.Wood]);
            Assert.AreEqual(0, bag[MealCatalog.Herb]);
            Assert.AreEqual(0, bag[MealCatalog.CookedMeat]);
            Assert.AreEqual(10 - baiao.ProvisionsCost, bag[MealCatalog.ProvisionsItemId]);
            Assert.AreEqual(baiao.RecipeId, meals.Active.recipeId);
            Assert.AreEqual(baiao.Encounters, meals.Active.encountersRemaining);
        }

        [Test]
        public void TryCook_UnlearnedDish_IsRefused()
        {
            MealService meals = NewService();
            var bag = Bag((MealCatalog.Wood, 9), (MealCatalog.Fiber, 9), (MealCatalog.ProvisionsItemId, 99));
            Assert.AreEqual(CookCheck.NotLearned, meals.TryCook(MealCatalog.Pamonha, Count(bag), Remove(bag), out _));
            Assert.AreEqual(9, bag[MealCatalog.Wood]);
        }

        // ------------------------------------------------------------------
        // Stacking and duration
        // ------------------------------------------------------------------

        [Test]
        public void Serve_SameDishRefreshesToFull_NeverAddsUp_AnotherDishReplaces()
        {
            MealRecipeSpec cuscuz = MealCatalog.Get(MealCatalog.Cuscuz);
            MealRecipeSpec cafe = MealCatalog.Get(MealCatalog.CafeCoado);

            ActiveMeal meal = CookingRules.Serve(ActiveMeal.None, cuscuz, out ServeOutcome outcome);
            Assert.AreEqual(ServeOutcome.Started, outcome);
            Assert.AreEqual(cuscuz.Encounters, meal.encountersRemaining);

            meal = CookingRules.CountEncounter(meal, out _);
            meal = CookingRules.Serve(meal, cuscuz, out outcome);
            Assert.AreEqual(ServeOutcome.Refreshed, outcome);
            Assert.AreEqual(cuscuz.Encounters, meal.encountersRemaining, "refresh = back to full");

            meal = CookingRules.Serve(meal, cuscuz, out outcome);
            Assert.AreEqual(cuscuz.Encounters, meal.encountersRemaining, "cooking twice never stacks the duration");

            meal = CookingRules.Serve(meal, cafe, out outcome);
            Assert.AreEqual(ServeOutcome.Replaced, outcome);
            Assert.AreEqual(cafe.RecipeId, meal.recipeId);
        }

        [Test]
        public void CountEncounter_CountsDown_ThenTheMealIsOver()
        {
            var meal = new ActiveMeal(MealCatalog.Cuscuz, 2);
            meal = CookingRules.CountEncounter(meal, out bool expired);
            Assert.IsFalse(expired);
            Assert.AreEqual(1, meal.encountersRemaining);

            meal = CookingRules.CountEncounter(meal, out expired);
            Assert.IsTrue(expired);
            Assert.IsFalse(meal.IsActive);

            meal = CookingRules.CountEncounter(meal, out expired);
            Assert.IsFalse(expired, "no meal, nothing to expire");
        }

        [Test]
        public void Service_EveryResolvedEncounter_CountsOne()
        {
            MealService meals = NewService();
            var bag = Bag((MealCatalog.Wood, 9), (MealCatalog.Herb, 9), (MealCatalog.ProvisionsItemId, 99));
            meals.TryCook(MealCatalog.Cuscuz, Count(bag), Remove(bag), out _);
            int start = meals.Active.encountersRemaining;

            var changes = new List<MealChangedEvent>();
            EventBus.Subscribe<MealChangedEvent>(changes.Add);

            EventBus.Publish(new EncounterEndedEvent(null, EncounterOutcome.Victory));
            Assert.AreEqual(start - 1, meals.Active.encountersRemaining);
            EventBus.Publish(new EncounterEndedEvent(null, EncounterOutcome.Fled));
            EventBus.Publish(new EncounterEndedEvent(null, EncounterOutcome.Defeat));
            Assert.IsFalse(meals.Active.IsActive, "victory, flight and defeat all count");
            Assert.IsTrue(changes.Last().Expired);
        }

        [Test]
        public void Sanitize_DropsUnknownOrEmptyMeals_AndClampsToTheDish()
        {
            Assert.IsFalse(CookingRules.Sanitize(new ActiveMeal("meal_feijoada_removed", 2)).IsActive);
            Assert.IsFalse(CookingRules.Sanitize(new ActiveMeal(MealCatalog.Cuscuz, 0)).IsActive);
            Assert.AreEqual(MealCatalog.Get(MealCatalog.Cuscuz).Encounters, CookingRules.Sanitize(new ActiveMeal(MealCatalog.Cuscuz, 99)).encountersRemaining);
        }

        // ------------------------------------------------------------------
        // Favourites and effects
        // ------------------------------------------------------------------

        [Test]
        public void FavouriteBonus_OnlyForTheCousinWhoLovesIt()
        {
            MealRecipeSpec pamonha = MealCatalog.Get(MealCatalog.Pamonha);
            Assert.IsTrue(CookingRules.BuildEncounterEffects(pamonha, CharacterId.Maria).Any(e => e.affectedStat == StatType.HealthRegen));
            Assert.IsFalse(CookingRules.BuildEncounterEffects(pamonha, CharacterId.Joaquim).Any(e => e.affectedStat == StatType.HealthRegen));
            Assert.IsFalse(CookingRules.BuildEncounterEffects(pamonha, null).Any(e => e.affectedStat == StatType.HealthRegen));

            // Ícaro's bonus rides on Rapadura's burst, not on the whole fight.
            MealRecipeSpec rapadura = MealCatalog.Get(MealCatalog.Rapadura);
            Assert.IsEmpty(CookingRules.BuildEncounterEffects(rapadura, CharacterId.Icaro));
            Assert.IsTrue(CookingRules.BuildOpeningBurst(rapadura, CharacterId.Icaro).Any(e => e.affectedStat == StatType.MoveSpeed));
            Assert.IsFalse(CookingRules.BuildOpeningBurst(rapadura, CharacterId.Lavine).Any(e => e.affectedStat == StatType.MoveSpeed));
            Assert.IsTrue(CookingRules.BuildOpeningBurst(rapadura, CharacterId.Lavine).Any(e => e.affectedStat == StatType.AttackSpeed));
        }

        [Test]
        public void MealEffects_ReachTheBuffController_StackWithOtherSources_AndReplaceOnlyThemselves()
        {
            var go = new GameObject("MealHero");
            _created.Add(go);
            BuffController buffs = go.AddComponent<BuffController>();

            // An attribute-like Defense bonus from another source stays when the meal is (re)applied.
            buffs.ApplyEffect(new StatModifierDefinition { affectedStat = StatType.Defense, application = ModifierApplication.PercentAdditive, value = 0.05f, durationSeconds = 0f }, "attributes");

            List<StatModifierDefinition> baiao = CookingRules.BuildEncounterEffects(MealCatalog.Get(MealCatalog.BaiaoDeDois), CharacterId.Joaquim);
            buffs.ApplyEffects(baiao, CookingRules.EffectSourceKey);
            buffs.ApplyEffects(baiao, CookingRules.EffectSourceKey);
            Assert.AreEqual(0.15f, buffs.GetPercentTotal(StatType.Defense), 0.0001f, "meal +10% stacks with others once, not with itself");
            Assert.AreEqual(1.05f, buffs.GetModifiedValue(StatType.MaxHealth, 1f), 0.0001f, "Joaquim's favourite bonus");
            Assert.IsTrue(buffs.HasEffectsFromSource(CookingRules.EffectSourceKey));

            // What MealEffectApplier watches for: a carried-state restore wiping every effect.
            buffs.ClearAllEffects();
            Assert.IsFalse(buffs.HasEffectsFromSource(CookingRules.EffectSourceKey));
        }

        // ------------------------------------------------------------------
        // Learning
        // ------------------------------------------------------------------

        [Test]
        public void Known_StartersAlways_QuestDishesOnceTheQuestIsDone()
        {
            var completed = new HashSet<string>();
            MealService meals = NewService(completed.Contains);
            Assert.IsTrue(meals.IsKnown(MealCatalog.Cuscuz));
            Assert.IsTrue(meals.IsKnown(MealCatalog.CafeCoado));
            Assert.IsFalse(meals.IsKnown(MealCatalog.Rapadura));

            // A save from before the stove: the quest is already complete, nothing was "learned".
            completed.Add(RegionQuestCatalog.DesertFirst);
            Assert.IsTrue(meals.IsKnown(MealCatalog.Rapadura));
        }

        [Test]
        public void QuestCompleted_TeachesItsDish_Once()
        {
            MealService meals = NewService();
            var learned = new List<MealRecipeLearnedEvent>();
            EventBus.Subscribe<MealRecipeLearnedEvent>(learned.Add);

            EventBus.Publish(new QuestCompletedEvent(RegionQuestCatalog.ForestFirst));
            EventBus.Publish(new QuestCompletedEvent(RegionQuestCatalog.ForestFirst));
            EventBus.Publish(new QuestCompletedEvent(RegionQuestCatalog.ForestSecond));

            Assert.IsTrue(meals.IsKnown(MealCatalog.Pamonha));
            Assert.AreEqual(1, learned.Count);
            Assert.AreEqual(MealCatalog.Pamonha, learned[0].RecipeId);
            Assert.AreEqual(MealLearnSource.Quest, learned[0].Source);
        }

        [Test]
        public void GrandmaLesson_TeachesTheCousinsFavourite_OnceItsRegionIsOpen()
        {
            var unlocked = new HashSet<BiomeType> { BiomeType.Forest };
            MealService meals = NewService(null, unlocked.Contains);

            Assert.IsNull(meals.TryTeachFavourite(CharacterId.Icaro), "rapadura waits for the desert");
            Assert.AreEqual(MealCatalog.Pamonha, meals.TryTeachFavourite(CharacterId.Maria)?.RecipeId);
            Assert.IsNull(meals.TryTeachFavourite(CharacterId.Maria), "already taught");
            Assert.IsNull(meals.TryTeachFavourite(CharacterId.Maithe), "café coado is a starter");
            Assert.IsNull(meals.TryTeachFavourite(null));

            unlocked.Add(BiomeType.Desert);
            Assert.AreEqual(MealCatalog.Rapadura, meals.TryTeachFavourite(CharacterId.Icaro)?.RecipeId);
        }

        // ------------------------------------------------------------------
        // Save
        // ------------------------------------------------------------------

        [Test]
        public void Save_RoundTripsThroughJson_AndANewSessionForgetsEverything()
        {
            MealService meals = NewService();
            meals.Learn(MealCatalog.QueijoCoalho, MealLearnSource.GrandmaLesson);
            var bag = Bag((MealCatalog.Wood, 9), (MealCatalog.IronOre, 9), (MealCatalog.Herb, 9), (MealCatalog.ProvisionsItemId, 99));
            meals.TryCook(MealCatalog.QueijoCoalho, Count(bag), Remove(bag), out _);
            meals.CountEncounter();

            string json = JsonUtility.ToJson(meals.CaptureState());
            MealService loaded = NewService();
            loaded.RestoreState(JsonUtility.FromJson<MealSaveState>(json));

            Assert.IsTrue(loaded.IsKnown(MealCatalog.QueijoCoalho));
            Assert.AreEqual(MealCatalog.QueijoCoalho, loaded.Active.recipeId);
            Assert.AreEqual(MealCatalog.Get(MealCatalog.QueijoCoalho).Encounters - 1, loaded.Active.encountersRemaining);

            loaded.ResetForNewSession();
            Assert.IsFalse(loaded.Active.IsActive);
            Assert.IsFalse(loaded.IsKnown(MealCatalog.QueijoCoalho));
            Assert.IsTrue(loaded.IsKnown(MealCatalog.Cuscuz));
        }

        [Test]
        public void Save_UnknownDishIdsAreDropped()
        {
            MealService meals = NewService();
            meals.RestoreState(new MealSaveState { learnedRecipeIds = new[] { "meal_gone", MealCatalog.Pamonha }, activeRecipeId = "meal_gone", encountersRemaining = 2 });
            CollectionAssert.AreEquivalent(new[] { MealCatalog.Pamonha }, meals.LearnedRecipeIds);
            Assert.IsFalse(meals.Active.IsActive);
        }

        // ------------------------------------------------------------------
        // Texts
        // ------------------------------------------------------------------

        [Test]
        public void Texts_EveryDishCousinAndLessonHasPtBrText()
        {
            foreach (MealRecipeSpec recipe in MealCatalog.Recipes)
            {
                Assert.IsNotNull(KitchenTexts.GetFallback(recipe.NameKey), recipe.NameKey);
                Assert.IsNotNull(KitchenTexts.GetFallback(recipe.EffectKey), recipe.EffectKey);
                if (recipe.FavouriteOf.HasValue)
                {
                    Assert.IsNotNull(KitchenTexts.GetFallback(recipe.FavouriteKey), recipe.FavouriteKey);
                }

                if (recipe.FavouriteOf.HasValue && !recipe.Starter)
                {
                    Assert.IsNotNull(KitchenTexts.GetFallback(KitchenTexts.LessonKey(recipe.FavouriteOf.Value)), $"lesson for {recipe.FavouriteOf}");
                }
            }

            foreach (CharacterId hero in System.Enum.GetValues(typeof(CharacterId)))
            {
                Assert.IsNotNull(KitchenTexts.GetFallback(KitchenTexts.GreetingKey(hero)), $"greeting for {hero}");
            }
        }

        [Test]
        public void Presenter_RowsAndBadge_ReadTheBag()
        {
            MealRecipeSpec cuscuz = MealCatalog.Get(MealCatalog.Cuscuz);
            var bag = Bag((MealCatalog.Wood, 1), (MealCatalog.Herb, 0), (MealCatalog.ProvisionsItemId, 7));

            KitchenRecipeRow row = KitchenPresenter.BuildRow(cuscuz, true, Count(bag), ActiveMeal.None, CharacterId.Lavine, null);
            Assert.AreEqual("Nan do Tandir", row.Title);
            Assert.IsFalse(row.CanCook);
            StringAssert.Contains("1/1", row.Ingredients);
            StringAssert.Contains("0/1", row.Ingredients);
            StringAssert.Contains("(tem 7)", row.Provisions);

            bag[MealCatalog.Herb] = 1;
            row = KitchenPresenter.BuildRow(cuscuz, true, Count(bag), new ActiveMeal(MealCatalog.Cuscuz, 2), CharacterId.Lavine, null);
            Assert.IsTrue(row.CanCook);
            Assert.IsTrue(row.IsActive);
            Assert.AreEqual(KitchenTexts.GetFallback(KitchenTexts.CookAgain), row.ButtonText);

            KitchenRecipeRow locked = KitchenPresenter.BuildRow(MealCatalog.Get(MealCatalog.Pamonha), false, Count(bag), ActiveMeal.None, null, null);
            Assert.AreEqual(KitchenTexts.GetFallback(KitchenTexts.Unknown), locked.Title);
            Assert.IsFalse(locked.CanCook);
            StringAssert.Contains("Maria", locked.HowToLearn);

            Assert.AreEqual("Nan do Tandir · 2", KitchenPresenter.HudBadgeText(new ActiveMeal(MealCatalog.Cuscuz, 2), null));
            Assert.AreEqual("Nan do Tandir · última", KitchenPresenter.HudBadgeText(new ActiveMeal(MealCatalog.Cuscuz, 1), null));
            Assert.IsNull(KitchenPresenter.HudBadgeText(ActiveMeal.None, null));
        }

        // ------------------------------------------------------------------

        private MealService NewService(System.Func<string, bool> isQuestCompleted = null, System.Func<BiomeType, bool> isRegionUnlocked = null)
        {
            var service = new MealService(isQuestCompleted ?? (_ => false), isRegionUnlocked ?? (_ => true));
            service.Initialize();
            _services.Add(service);
            return service;
        }

        private static Dictionary<string, int> Bag(params (string id, int count)[] items)
        {
            var bag = new Dictionary<string, int>();
            foreach ((string id, int count) in items)
            {
                bag[id] = count;
            }

            return bag;
        }

        private static System.Func<string, int> Count(Dictionary<string, int> bag) => id => bag.TryGetValue(id, out int n) ? n : 0;

        private static System.Func<string, int, bool> Remove(Dictionary<string, int> bag) => (id, quantity) =>
        {
            if (!bag.TryGetValue(id, out int n) || n < quantity)
            {
                return false;
            }

            bag[id] = n - quantity;
            return true;
        };
    }
}
