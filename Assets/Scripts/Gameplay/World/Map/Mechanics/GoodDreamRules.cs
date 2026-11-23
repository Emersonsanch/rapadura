using Rapadura.Gameplay.Enemies;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// The dreams of the Fortim after O Pesadelo da Tábua is beaten (Docs/Mapas100.md 49: "O Pesadelo da Tábua, a sombra de um sonho ruim dos
    /// gigantes que fugiu da tábua; vencido, o sonho vira bom"), pure and server-runnable, on top of <see cref="NartesDreamRules"/>. While the
    /// nightmare walks, Virogdad's dream is blurred: the distance comes rounded to 50 m. Beaten, the dream turns good: Virogdad sees it clear
    /// (rounded to 25 m) and the chest holds more.
    /// <code>
    /// nightmare loose: 50 m rounding · 40 gold nuggets
    /// nightmare beaten: 25 m rounding · 70 gold nuggets · same hiding place of the day for everyone, still once per UTC day
    /// </code>
    /// Decision (2026-11-23): a reward for the map's boss that lasts, but small: the chest is a daily walk, not a farm.
    /// </summary>
    public static class GoodDreamRules
    {
        public const string NightmareEnemyId = RegionBossRules.TabletNightmareId;
        public const int GoodPurse = 70;
        public const int BlurredRounding = 50;
        public const int ClearRounding = 25;

        /// <summary>The save mark of the cousin's line on the first good dream (set once, on that event).</summary>
        public const string FirstGoodDreamKey = "nartes.dream.first_good";

        public static int Purse(bool good) => good ? GoodPurse : NartesDreamRules.Purse;

        /// <summary>The distance Virogdad tells: rounded to 25 m once the dream is good, to 50 m before (never under the rounding).</summary>
        public static int ToldDistance(Vector2 from, Vector2 to, bool good)
        {
            int step = good ? ClearRounding : BlurredRounding;
            return Mathf.Max(step, Mathf.RoundToInt(Vector2.Distance(from, to) / step) * step);
        }
    }
}
