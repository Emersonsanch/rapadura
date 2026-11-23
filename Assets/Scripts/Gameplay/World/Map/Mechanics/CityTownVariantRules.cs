using System;
using Rapadura.Gameplay.World.Cities.Town;
using Rapadura.Gameplay.World.Map.Overworld;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// Which look of a town the save asks for (its "recipe version"), pure and server-runnable: the variant is world state, read from the
    /// save's time marks only, so the online server and every client agree on it. A town with no progress-driven look is always variant 0.
    /// <code>
    /// Kirta (28)    variant = corals of the reef lit, 0–5        → CityTownRecipes.Kirta(variant)   (the mother coral's branches)
    /// Tarchon (33)  variant = pairs of festive lanterns, 0–2      → CityTownRecipes.Tarchon(trols)    (2 / 5 trols back: 2 / 4 more lamps)
    /// </code>
    /// Decision (2026-11-23): Tarchon's variant is the lantern pairs, not the trols back home, so the town is rebuilt only when its look
    /// changes (twice in the whole game); the trols themselves are figures of the mechanic and come and go on their own.
    /// </summary>
    public static class CityTownVariantRules
    {
        /// <summary>The time-mark keys of the five corals round Kirta, built once (read every check, never concatenated per frame).</summary>
        private static string[] s_coralKeys;

        private static string[] CoralKeys
        {
            get
            {
                if (s_coralKeys == null)
                {
                    KirtaCoral[] corals = OverworldLayouts.KirtaCorals;
                    var keys = new string[corals.Length];
                    for (int i = 0; i < corals.Length; i++)
                    {
                        keys[i] = KirtaCoralRules.MarkKey(corals[i].Id);
                    }

                    s_coralKeys = keys;
                }

                return s_coralKeys;
            }
        }

        /// <summary>Whether the town <paramref name="cityId"/> has looks that follow the save.</summary>
        public static bool HasVariants(string cityId) => cityId == CityTownRecipes.KirtaCityId || cityId == CityTownRecipes.TarchonCityId;

        /// <summary>Kirta's variant: the corals of the reef lit (0–5).</summary>
        public static int KirtaVariant(int coralsLit) => KirtaMotherCoralRules.Clamp(coralsLit);

        /// <summary>Tarchon's variant: the pairs of festive lanterns on the plaza (0–2) after <paramref name="totalWoken"/> neighbours ever woken.</summary>
        public static int TarchonVariant(long totalWoken) => TarchonCrowdRules.LanternPairs(TarchonCrowdRules.Returned(totalWoken));

        /// <summary>The variant of <paramref name="cityId"/> for a save read through <paramref name="timeMark"/> (null = a fresh save).</summary>
        public static int VariantFor(string cityId, Func<string, long> timeMark)
        {
            if (timeMark == null)
            {
                return 0;
            }

            if (cityId == CityTownRecipes.KirtaCityId)
            {
                int lit = 0;
                foreach (string key in CoralKeys)
                {
                    if (KirtaCoralRules.IsLit(timeMark(key)))
                    {
                        lit++;
                    }
                }

                return KirtaVariant(lit);
            }

            if (cityId == CityTownRecipes.TarchonCityId)
            {
                return TarchonVariant(timeMark(TarchonLanternRules.TotalKey));
            }

            return 0;
        }

        /// <summary>The fewest trols back home that hang <paramref name="pairs"/> pairs of festive lanterns (the inverse of <see cref="TarchonCrowdRules.LanternPairs"/>).</summary>
        public static int TarchonReturnedFor(int pairs) => pairs >= 2 ? 5 : pairs == 1 ? 2 : 0;

        /// <summary>
        /// The recipe of <paramref name="cityId"/> in <paramref name="variant"/>, as the stitched world places it (no backdrop), or null for a
        /// town with no variants (it keeps the recipe it was placed with).
        /// </summary>
        public static CityTownRecipe RecipeFor(string cityId, int variant)
        {
            CityTownRecipe recipe;
            if (cityId == CityTownRecipes.KirtaCityId)
            {
                recipe = CityTownRecipes.Kirta(KirtaVariant(variant));
            }
            else if (cityId == CityTownRecipes.TarchonCityId)
            {
                recipe = CityTownRecipes.Tarchon(TarchonReturnedFor(variant));
            }
            else
            {
                return null;
            }

            recipe.Backdrop = false;
            return recipe;
        }
    }
}
