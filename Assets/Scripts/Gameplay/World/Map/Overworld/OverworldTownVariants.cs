using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Rapadura.Gameplay.World.Cities.Town;
using Rapadura.Gameplay.World.Map.Mechanics;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Overworld
{
    /// <summary>
    /// The progress-driven looks of the stitched world's towns (<see cref="CityTownVariantRules"/>), pure: the generated town of a settlement
    /// in a given variant, fitted to the world like <see cref="OverworldSettlement.Town"/> (same walk bounds, safe zone and ground), the
    /// mesh-cache key of its props, and the checks <see cref="OverworldView"/> runs before swapping a built town to another variant.
    /// <see cref="OverworldSettlement.Town"/> stays variant 0: it is the world's own data (chunk scatter, lots, docks), which a variant never
    /// moves; only the town kit the hero sees and collides with follows the save.
    /// </summary>
    public static class OverworldTownVariants
    {
        private static readonly ConditionalWeakTable<OverworldSettlement, Dictionary<int, CityLayout>> Cache =
            new ConditionalWeakTable<OverworldSettlement, Dictionary<int, CityLayout>>();

        /// <summary>The town of <paramref name="settlement"/> in <paramref name="variant"/> (generated once per settlement and variant).</summary>
        public static CityLayout LayoutFor(OverworldSettlement settlement, int variant)
        {
            if (settlement == null || settlement.Town == null || variant == 0 || !CityTownVariantRules.HasVariants(settlement.Id))
            {
                return settlement?.Town;
            }

            Dictionary<int, CityLayout> byVariant = Cache.GetOrCreateValue(settlement);
            if (byVariant.TryGetValue(variant, out CityLayout cached))
            {
                return cached;
            }

            CityTownRecipe recipe = CityTownVariantRules.RecipeFor(settlement.Id, variant);
            if (recipe == null)
            {
                return settlement.Town;
            }

            CityLayout town = CityLayoutGenerator.Generate(recipe);
            town.WalkBounds = settlement.Town.WalkBounds;
            town.SafeZone = settlement.Town.SafeZone;
            town.GroundHalfExtent = settlement.Town.GroundHalfExtent;
            town.Invalidate();
            byVariant[variant] = town;
            return town;
        }

        /// <summary>The mesh-cache key of the props of a town built under <paramref name="baseKey"/> in <paramref name="variant"/> (variant 0 keeps the plain key).</summary>
        public static string PropsKey(string baseKey, int variant) => variant == 0 ? baseKey : baseKey + "#v" + variant;

        /// <summary>Whether the two towns have the very same solids, in the same places (then the colliders are left untouched).</summary>
        public static bool SameSolids(CityLayout a, CityLayout b)
        {
            IReadOnlyList<CitySolid> sa = a.Solids;
            IReadOnlyList<CitySolid> sb = b.Solids;
            if (sa.Count != sb.Count)
            {
                return false;
            }

            for (int i = 0; i < sa.Count; i++)
            {
                if (!Same(sa[i], sb[i]))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>Whether the two towns light the same plaza lamps (then the night lights are left untouched).</summary>
        public static bool SameLamps(CityLayout a, CityLayout b) => LampCount(a) == LampCount(b);

        /// <summary>
        /// Whether a solid that <paramref name="after"/> has and <paramref name="before"/> has not would stand within <paramref name="radius"/>
        /// of the hero at <paramref name="heroLocal"/> (town coordinates): then the swap waits for them to step aside (never shoves them).
        /// </summary>
        public static bool BlocksHero(CityLayout before, CityLayout after, Vector2 heroLocal, float radius)
        {
            IReadOnlyList<CitySolid> old = before.Solids;
            foreach (CitySolid solid in after.Solids)
            {
                if (solid.DistanceTo(heroLocal) >= radius)
                {
                    continue;
                }

                bool existed = false;
                for (int i = 0; i < old.Count && !existed; i++)
                {
                    existed = Same(old[i], solid);
                }

                if (!existed)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool Same(CitySolid a, CitySolid b) =>
            a.IsCapsule == b.IsCapsule && a.Footprint == b.Footprint && Mathf.Approximately(a.Radius, b.Radius) && Mathf.Approximately(a.Height, b.Height);

        private static int LampCount(CityLayout layout)
        {
            int n = 0;
            foreach (CityProp prop in layout.Props)
            {
                if (prop.Kind == CityPropKind.LampPost)
                {
                    n++;
                }
            }

            return n;
        }
    }
}
