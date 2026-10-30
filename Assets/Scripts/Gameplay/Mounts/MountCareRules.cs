using System.Collections.Generic;
using System.Linq;
using Rapadura.Gameplay.Characters;
using UnityEngine;

namespace Rapadura.Gameplay.Mounts
{
    /// <summary>One food a mount eats (Docs/Mounts.md §4.2): paid with bag items or as one of Vó Edna's dishes.</summary>
    public sealed class MountFoodSpec
    {
        public readonly string FoodId;

        /// <summary>Bag item paid (null for a dish).</summary>
        public readonly string ItemId;
        public readonly int ItemQuantity;

        /// <summary>Cozinha recipe cooked for the mount (its ingredients are paid), or null.</summary>
        public readonly string MealRecipeId;
        public readonly float Satiety;
        public readonly int Xp;
        public readonly int Affection;
        public readonly MountDiet Diets;

        public MountFoodSpec(string foodId, string itemId, int itemQuantity, string mealRecipeId, float satiety, int xp, int affection, MountDiet diets)
        {
            FoodId = foodId;
            ItemId = itemId;
            ItemQuantity = itemQuantity;
            MealRecipeId = mealRecipeId;
            Satiety = satiety;
            Xp = xp;
            Affection = affection;
            Diets = diets;
        }

        public bool IsDish => !string.IsNullOrEmpty(MealRecipeId);
        public string NameKey => "mount.food." + FoodId + ".name";
    }

    /// <summary>
    /// Phase M2 care rules (Docs/Mounts.md §4, §7, §8), plain C# and EditMode-tested like <see cref="MountRules"/>:
    /// <list type="bullet">
    /// <item><b>Hunger</b>: satiety 0..100, −1 per minute mounted (never in the stable); below 30 the mount walks at
    /// ×1.15 and does not gallop (it never stops).</item>
    /// <item><b>Food</b> (§4.2): capim (2 fibras), Ração do Haras, erva fresca, carne assada (carnivores) and three of
    /// Vó Edna's dishes (cuscuz, rapadura — the favourite treat, +5 affection — and baião de dois), with diets.</item>
    /// <item><b>Level 1–20</b>: next = 60 + 20 × level; 1 XP per 20 m ridden, food XP, 2 + 1% of each kill with the hero
    /// (<see cref="MountGrowthRules"/>: capped by the rider's level, the rest stored as Brilho) while the mount is
    /// waiting. Level raises health (+5%/level, <see cref="MountRules.MaxHealthAt"/>), stamina (+1) and shortens the
    /// cooldown (−0.5%/level); never speed nor defense.</item>
    /// <item><b>Affection</b> 0..500, 100 per heart: food, cuddles (30 s apart), +1 per km ridden. Hearts give comfort,
    /// not power: 2 = cooldown −5%, 3 = line on mounting, 4 = 0.75 s whistle, 5 = health +5%.</item>
    /// <item><b>Favourite</b> (§8): the cousin riding gets cooldown −15%, affection ×1.25 and their line; plus each
    /// one's small detail (Lavine on Pé-de-Pano: the Arrancada recharges 3 s sooner).</item>
    /// <item><b>Stable</b> (§7): stalls by house level (2/3/4/6/8, +1 with the Curral de aroeira); the cocho or
    /// manjedoura feed the mounts in their stalls +10 per fight; the bebedouro gives them +1 affection a day.</item>
    /// </list>
    /// </summary>
    public static class MountCareRules
    {
        public const float MaxSatiety = 100f;
        public const float SatietyPerMinuteMounted = 1f;
        public const float HungryThreshold = 30f;
        public const float HungrySpeedMultiplier = 1.15f;

        public const float MetersPerXp = 20f;
        public const float MetersPerAffection = 1000f;
        /// <summary>Legacy +5 per fight won; superseded by per-kill XP (<see cref="MountGrowthRules.CombatXp"/>). Kept for old references.</summary>
        public const int VictoryXp = 5;
        public const int CuddleAffection = 1;
        public const float CuddleCooldownSeconds = 30f;
        public const int AffectionPerHeart = 100;
        public const int MaxAffection = 500;
        public const float FavoriteAffectionMultiplier = 1.25f;
        public const float FavoriteCooldownFactor = 0.85f;
        public const float FastWhistleSeconds = 0.75f;
        public const float FiveHeartHealthBonus = 1.05f;

        public const string StableYardCorral = "casa_curral_010";
        public const string StableTrough = "casa_curral_011";
        public const string StableDrinker = "casa_curral_012";
        public const string StableManger = "casa_curral_026";
        public const float StableFeedPerFight = 10f;

        public const string HayItemId = "item_fiber";
        public const string FeedItemId = "item_mount_feed";

        public static readonly IReadOnlyList<MountFoodSpec> Foods = new List<MountFoodSpec>
        {
            new MountFoodSpec("capim", HayItemId, 2, null, 25f, 10, 0, MountDiet.Herbivora | MountDiet.Encantada),
            new MountFoodSpec("racao", FeedItemId, 1, null, 40f, 15, 0, MountDiet.Herbivora | MountDiet.Carnivora | MountDiet.Encantada),
            new MountFoodSpec("erva", "item_herb", 1, null, 15f, 10, 1, MountDiet.Herbivora | MountDiet.Encantada),
            new MountFoodSpec("carne", "item_cooked_meat", 1, null, 40f, 15, 1, MountDiet.Carnivora),
            new MountFoodSpec("cuscuz", null, 0, "meal_cuscuz", 60f, 40, 2, MountDiet.Herbivora | MountDiet.Carnivora | MountDiet.Encantada),
            new MountFoodSpec("rapadura", null, 0, "meal_rapadura", 20f, 20, 5, MountDiet.Herbivora | MountDiet.Carnivora | MountDiet.Encantada),
            new MountFoodSpec("baiao", null, 0, "meal_baiao_de_dois", 80f, 60, 2, MountDiet.Carnivora | MountDiet.Encantada),
        };

        public static MountFoodSpec FindFood(string foodId) => Foods.FirstOrDefault(f => f.FoodId == foodId);

        // ---------------- Hunger ----------------

        public static bool IsHungry(float satiety) => satiety < HungryThreshold;

        /// <summary>Satiety after <paramref name="seconds"/> mounted (−1 per minute).</summary>
        public static float DecaySatiety(float satiety, float seconds)
        {
            return Mathf.Clamp(satiety - SatietyPerMinuteMounted * Mathf.Max(0f, seconds) / 60f, 0f, MaxSatiety);
        }

        public static bool CanEat(MountDiet diet, MountFoodSpec food) => food != null && (diet & food.Diets) != 0;

        // ---------------- Level ----------------

        public static int XpToNext(int level) => 60 + 20 * Mathf.Clamp(level, 1, MountRules.MaxLevel);

        /// <summary>Adds XP, levelling up (capped at 20, where XP stops). Returns levels gained.</summary>
        public static int AddXp(ref int level, ref int xp, int amount)
        {
            level = Mathf.Clamp(level, 1, MountRules.MaxLevel);
            if (amount <= 0 || level >= MountRules.MaxLevel)
            {
                if (level >= MountRules.MaxLevel)
                {
                    xp = 0;
                }

                return 0;
            }

            int gained = 0;
            xp += amount;
            while (level < MountRules.MaxLevel && xp >= XpToNext(level))
            {
                xp -= XpToNext(level);
                level++;
                gained++;
            }

            if (level >= MountRules.MaxLevel)
            {
                xp = 0;
            }

            return gained;
        }

        /// <summary>Total XP from level 1 to 20 (~4,940, §4.1).</summary>
        public static int TotalXpToMax()
        {
            int total = 0;
            for (int level = 1; level < MountRules.MaxLevel; level++)
            {
                total += XpToNext(level);
            }

            return total;
        }

        // ---------------- Affection ----------------

        public static int Hearts(int affection) => Mathf.Clamp(affection, 0, MaxAffection) / AffectionPerHeart;

        /// <summary>Affection after a gain of <paramref name="points"/> (×1.25 for the rider's favourite; the fraction carries over).</summary>
        public static int AddAffection(int affection, ref float fraction, int points, bool isFavorite)
        {
            if (points <= 0)
            {
                return Mathf.Clamp(affection, 0, MaxAffection);
            }

            float exact = points * (isFavorite ? FavoriteAffectionMultiplier : 1f) + Mathf.Max(0f, fraction);
            int whole = Mathf.FloorToInt(exact + 1e-4f);
            fraction = exact - whole;
            return Mathf.Clamp(affection + whole, 0, MaxAffection);
        }

        public static float WhistleSeconds(int hearts) => hearts >= 4 ? FastWhistleSeconds : MountRules.SummonSeconds;

        public static float HealthMultiplier(int hearts) => hearts >= 5 ? FiveHeartHealthBonus : 1f;

        /// <summary>Line on mounting: always the favourite's, otherwise from 3 hearts on.</summary>
        public static bool SaysLineOnMount(int hearts, bool isFavorite) => isFavorite || hearts >= 3;

        public static bool CanCuddle(float lastCuddleTime, float now) => now - lastCuddleTime >= CuddleCooldownSeconds;

        // ---------------- Favourite ----------------

        public static bool IsFavorite(MountDefinition definition, CharacterId? rider)
        {
            return definition != null && definition.HasFavorite && rider.HasValue && rider.Value == definition.FavoriteCharacter;
        }

        /// <summary>Each favourite's small detail on the ability cooldown (Lavine on the pony: −3 s).</summary>
        public static float FavoriteExtraCooldownCut(string mountId) => mountId == MountCatalog.Pony ? 3f : 0f;

        /// <summary>The full ability cooldown for a mount and its rider.</summary>
        public static float AbilityCooldown(MountDefinition definition, OwnedMount mount, CharacterId? rider)
        {
            if (definition == null || definition.Ability == null)
            {
                return 0f;
            }

            bool favorite = IsFavorite(definition, rider);
            // Effective level under the rider (Docs/Progressao500.md §6.1); Despertar III shortens it by 10% (§6.4).
            float cooldown = MountRules.AbilityCooldownSeconds(definition.Ability.CooldownSeconds, mount != null ? MountCollection.EffectiveLevelOf(mount) : 1,
                mount != null ? Hearts(mount.affection) : 0, favorite) * MountGrowthRules.AbilityCooldownFactor(mount != null ? mount.awakening : 0);
            return Mathf.Max(1f, cooldown - (favorite ? FavoriteExtraCooldownCut(definition.MountId) : 0f));
        }

        // ---------------- Stable ----------------

        /// <summary>Stalls of the stable at a house level (madeira 2, varanda 3, taipa 4, alvenaria 6, casarão 8), +1 with the corral.</summary>
        public static int StallsFor(int houseLevel, bool hasHouse, bool hasCorral)
        {
            if (!hasHouse)
            {
                return 0;
            }

            int[] byLevel = { 2, 3, 4, 6, 8 };
            int stalls = byLevel[Mathf.Clamp(houseLevel, 1, byLevel.Length) - 1];
            return stalls + (hasCorral ? 1 : 0);
        }
    }
}
