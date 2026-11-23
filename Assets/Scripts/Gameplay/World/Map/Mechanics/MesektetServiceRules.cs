using System;
using Rapadura.Gameplay.Skills;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>The three circles of light Djedi teaches (Cais de Mesektet); appended values only.</summary>
    public enum DjediLesson
    {
        /// <summary>Círculo da Lamparina: mana comes back faster.</summary>
        Lamp = 0,

        /// <summary>Círculo das Horas: skills come back sooner.</summary>
        Hours = 1,

        /// <summary>Círculo do Escriba: skills cost less mana.</summary>
        Scribe = 2,
    }

    /// <summary>What a Mesektet service answers to a choice (<see cref="MesektetServiceRules"/>).</summary>
    public enum MesektetServiceCheck
    {
        Ready,
        InCombat,
        NotEnoughGold,

        /// <summary>The shelf has nothing new (or that shelf is not reached / already received).</summary>
        NothingToClaim,

        /// <summary>A lower shelf must be received first.</summary>
        WaitTurn,
    }

    /// <summary>
    /// The two services of the Cais de Mesektet that only had lines (Docs/Mapas100.md map 71, Docs/Cities.md): Djedi, the trainer of magic,
    /// and Setne, the librarian of the Caderno. Pure and server-runnable (the scene side is <c>CityServicesMechanicsDriver.Mesektet</c>, the
    /// screen <c>CityServicePanel</c>); days are UTC days, the same for everyone.
    /// <code>
    /// Djedi · his circle of light (East street) · out of combat · one circle at a time, 15 min:
    ///   Lamparina +20 % mana regen · Horas +5 % cooldown reduction · Escriba +8 % mana cost reduction
    ///   the first lesson of each UTC day free, then 120 gold each
    /// Setne · his shelf (South street) · every milestone of Caderno pages filled pays once, in order:
    ///   25 · 50 · 75 · 100 · 150 · 200 · 250 · 300 pages → 3 gold a page, Pedras da Bênção, a Pedra da Alma from 100, a Pedra da Vida at 300
    /// </code>
    /// </summary>
    public static class MesektetServiceRules
    {
        // ---- Djedi's circles of light ----

        public const float LessonReach = 2.2f;
        public const float LessonSeconds = 900f;
        public const int LessonGold = 120;

        /// <summary>The daily mark of the free lesson (the UTC day it was last taken).</summary>
        public const string LessonDailyKey = "city.cais_de_mesektet.djedi_lesson";

        public static readonly DjediLesson[] Lessons = { DjediLesson.Lamp, DjediLesson.Hours, DjediLesson.Scribe };

        public static StatType LessonStat(DjediLesson lesson)
        {
            switch (lesson)
            {
                case DjediLesson.Hours: return StatType.CooldownReduction;
                case DjediLesson.Scribe: return StatType.ResourceCostReduction;
                default: return StatType.ManaRegen;
            }
        }

        /// <summary>Mana regen is a percent of the base (like the Samsa or the Tilápia); the two reductions are flat fractions (like the Karkadé).</summary>
        public static ModifierApplication LessonApplication(DjediLesson lesson) =>
            lesson == DjediLesson.Lamp ? ModifierApplication.PercentAdditive : ModifierApplication.Flat;

        public static float LessonValue(DjediLesson lesson)
        {
            switch (lesson)
            {
                case DjediLesson.Hours: return 0.05f;
                case DjediLesson.Scribe: return 0.08f;
                default: return 0.20f;
            }
        }

        /// <summary>Gold of a lesson now: 0 for the first of the UTC day (<paramref name="freeDay"/> is the saved day, 0 when never).</summary>
        public static int LessonCost(long freeDay, long today) => freeDay == today ? LessonGold : 0;

        /// <summary>Whether the lesson's cost is the day's free one.</summary>
        public static bool LessonIsFree(long freeDay, long today) => LessonCost(freeDay, today) == 0;

        /// <summary>A lesson: never in a fight; the gold must be in the bag.</summary>
        public static MesektetServiceCheck CheckLesson(bool inCombat, int cost, int goldOwned)
        {
            if (inCombat)
            {
                return MesektetServiceCheck.InCombat;
            }

            return goldOwned < cost ? MesektetServiceCheck.NotEnoughGold : MesektetServiceCheck.Ready;
        }

        /// <summary>Whole minutes left (rounded up) of a lit circle; 0 when none.</summary>
        public static int MinutesLeft(float secondsLeft) => secondsLeft <= 0f ? 0 : (int)Math.Ceiling(secondsLeft / 60f);

        // ---- Setne's shelf of the Caderno ----

        public const float ShelfReach = 2.2f;

        /// <summary>How many shelves were received (a count, saved as a time mark of <c>WorldMapProgress</c>).</summary>
        public const string ShelfClaimedKey = "city.cais_de_mesektet.setne_shelf";

        /// <summary>Caderno pages filled for each shelf, in order.</summary>
        public static readonly int[] ShelfPages = { 25, 50, 75, 100, 150, 200, 250, 300 };

        public const int GoldPerPage = 3;

        public static int ShelfCount => ShelfPages.Length;

        public static int ShelfGold(int shelf) => ValidShelf(shelf) ? ShelfPages[shelf] * GoldPerPage : 0;

        /// <summary>Pedras da Bênção of a shelf: 1, 1, 2, 2, 3, 3, 4, 4.</summary>
        public static int ShelfBlessingStones(int shelf) => ValidShelf(shelf) ? 1 + shelf / 2 : 0;

        /// <summary>One Pedra da Alma from the 100-page shelf on.</summary>
        public static int ShelfSoulStones(int shelf) => ValidShelf(shelf) && ShelfPages[shelf] >= 100 ? 1 : 0;

        /// <summary>One Pedra da Vida on the last shelf (300 pages).</summary>
        public static int ShelfLifeStones(int shelf) => ValidShelf(shelf) && shelf == ShelfPages.Length - 1 ? 1 : 0;

        /// <summary>How many shelves <paramref name="pages"/> filled pages reach.</summary>
        public static int ShelvesReached(int pages)
        {
            int count = 0;
            while (count < ShelfPages.Length && pages >= ShelfPages[count])
            {
                count++;
            }

            return count;
        }

        /// <summary>Shelves reached and not yet received.</summary>
        public static int Claimable(int pages, int claimed) => Math.Max(0, ShelvesReached(pages) - Math.Max(0, claimed));

        /// <summary>Receiving <paramref name="shelf"/>: only the next one in order, once it is reached.</summary>
        public static MesektetServiceCheck CheckShelf(int shelf, int pages, int claimed)
        {
            claimed = Math.Max(0, claimed);
            if (!ValidShelf(shelf) || shelf < claimed || pages < ShelfPages[shelf])
            {
                return MesektetServiceCheck.NothingToClaim;
            }

            return shelf > claimed ? MesektetServiceCheck.WaitTurn : MesektetServiceCheck.Ready;
        }

        /// <summary>Pages still missing for <paramref name="shelf"/> (0 once reached).</summary>
        public static int PagesToGo(int shelf, int pages) => ValidShelf(shelf) ? Math.Max(0, ShelfPages[shelf] - pages) : 0;

        private static bool ValidShelf(int shelf) => shelf >= 0 && shelf < ShelfPages.Length;
    }
}
