using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Refining;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// Apep beaten "cospe o rio e recua até a noite seguinte" (Docs/Mapas100.md 76, ganchos: "loot escama de Apep, Pedra da Mistura"), pure and
    /// server-runnable. The river she drank comes back out of her, and with it the stone of the Mixture she had swallowed with it:
    /// <code>
    /// every victory over Apep: 1 Pedra da Mistura (item_pedra_caos), sure, on top of her own loot (Escama de Apep, the refine-stone roll)
    /// beaten within 1 h of midnight on the game clock (her strongest, ≥ 1.27×): 2
    /// does not fit in the bag: it stays due (one save mark, written only then) and is paid as soon as there is room
    /// </code>
    /// Decision (2026-11-23): she comes back only on a later night, so this is at most one payout a game night; the midnight bonus rewards
    /// fighting her at her peak, the freed barque stays only her preparation (<see cref="ApepBarqueLinkRules"/>, no second reward).
    /// Online the server pays every hero that fought her.
    /// </summary>
    public static class ApepSpitRiverRules
    {
        public const string StoneItemId = RefineItems.ChaosStone;
        public const int Stones = 1;
        public const int MidnightStones = 2;
        public const float MidnightBonusHours = 1f;

        /// <summary>Save key (<see cref="WorldMapProgress"/> time mark): how many stones are still due (0 or missing = none).</summary>
        public const string OwedKey = "rio.apep.spit_owed";

        public static bool IsApep(string enemyId) => enemyId == RegionBossRules.ApepId;

        /// <summary>Hours from midnight of <paramref name="hour"/> (0–24 game clock), wrapping round the day.</summary>
        public static float HoursFromMidnight(float hour) => Mathf.Abs(Mathf.Repeat(hour + 12f, 24f) - 12f);

        /// <summary>The Pedras da Mistura she spits when beaten at <paramref name="hour"/>.</summary>
        public static int StonesFor(float hour) => HoursFromMidnight(hour) <= MidnightBonusHours ? MidnightStones : Stones;

        /// <summary>What is due after a victory at <paramref name="hour"/> with <paramref name="owed"/> still unpaid (never negative).</summary>
        public static int Owe(int owed, float hour) => Mathf.Max(0, owed) + StonesFor(hour);
    }
}
