using System.Collections.Generic;

namespace Rapadura.Gameplay.Tacho
{
    /// <summary>What eating a fruit would do right now (pure answer for the bag and the tests).</summary>
    public enum FruitCheck
    {
        Ok = 0,
        NotAFruit = 1,
        /// <summary>Fruta do Mandacaru: the hero already ate <see cref="FruitRules.MaxGrowthPoints"/>.</summary>
        CapReached = 2,
        /// <summary>Fruta do Umbu: no allocated point to take back.</summary>
        NothingToReset = 3,
        /// <summary>No attribute sheet here (the world map has none: eat it in an encounter).</summary>
        NoAttributes = 4,
    }

    /// <summary>
    /// "Frutas de Luz" (MU Online's fruits, our names; Docs/Balance/README.md "Tacho Encantado e asas"), brewed in the Tacho
    /// Encantado and eaten from the Mochila during an encounter (where the hero's <c>AttributeSet</c> lives):
    /// <list type="bullet">
    /// <item><b>Fruta do Mandacaru</b>: +1 attribute point to spend, at most <see cref="MaxGrowthPoints"/> per save (counted
    /// in the AttributeSet's save; the power budget assumes them all in Poder Arcano, the most offensive spend).</item>
    /// <item><b>Fruta do Umbu</b>: takes 1 point back from the attribute with the most allocated points (ties: sheet order)
    /// so it can be spent again. Never below 0, never touches the hero's innate bonus.</item>
    /// <item>A fruit that cannot do anything (cap reached, nothing to reset) is NOT eaten: the bag keeps it.</item>
    /// </list>
    /// Pure, no UnityEngine. Online the server applies it.
    /// </summary>
    public static class FruitRules
    {
        public const string GrowthFruit = "item_fruta_mandacaru";
        public const string ResetFruit = "item_fruta_umbu";

        /// <summary>Extra attribute points the Fruta do Mandacaru can ever give one save.</summary>
        public const int MaxGrowthPoints = 2;

        public static bool IsFruit(string itemId) => itemId == GrowthFruit || itemId == ResetFruit;

        public static FruitCheck CheckGrowth(int growthPointsEaten) =>
            growthPointsEaten >= MaxGrowthPoints ? FruitCheck.CapReached : FruitCheck.Ok;

        /// <summary>
        /// Index (in <paramref name="allocated"/> order) of the attribute the Umbu takes a point from: the most allocated one,
        /// the first on ties; -1 when nothing is allocated.
        /// </summary>
        public static int ResetTarget(IReadOnlyList<int> allocated)
        {
            int best = -1;
            int bestPoints = 0;
            if (allocated == null)
            {
                return best;
            }

            for (int i = 0; i < allocated.Count; i++)
            {
                if (allocated[i] > bestPoints)
                {
                    best = i;
                    bestPoints = allocated[i];
                }
            }

            return best;
        }

        public static FruitCheck CheckReset(IReadOnlyList<int> allocated) =>
            ResetTarget(allocated) < 0 ? FruitCheck.NothingToReset : FruitCheck.Ok;
    }
}
