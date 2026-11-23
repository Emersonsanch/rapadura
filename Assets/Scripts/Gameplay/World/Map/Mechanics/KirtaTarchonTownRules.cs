using System;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// The mother coral of Kirta's plaza (28, <c>CityTownRecipes.Kirta(int)</c>, <c>TownKit.CoralMother</c>) follows the five corals of the reef
    /// (<see cref="KirtaCoralRules"/>), pure and server-runnable. It has seven branches; three are relit when the game starts (the east half, the
    /// look the town always had), each coral of the reef lit relights one more, east to west, and with all five the trunk and the rim of the pool
    /// come back too: the mother coral is whole.
    /// <code>
    /// corals lit:  0  1  2  3  4  5
    /// branches:    3  4  5  6  7  7 + trunk and rim (whole)
    /// </code>
    /// Decision (2026-11-23): the fourth coral already lights the last branch so the fifth has its own reward (the whole tree, Kirta's line).
    /// </summary>
    public static class KirtaMotherCoralRules
    {
        public const int Branches = 7;
        public const int BranchesLitAtStart = 3;

        /// <summary>The branches of <c>TownKit.CoralMother</c> (index i points at i × 51.4° + 10°) in the order they relight: east first.</summary>
        public static readonly int[] RelightOrder = { 0, 6, 1, 5, 2, 4, 3 };

        /// <summary>Time mark: the corals counted the last time the hero stood by the mother coral (her line is said once per new coral).</summary>
        public const string SeenKey = "kirta.mother.seen";

        /// <summary>How near the plaza's mother coral the hero hears it (metres).</summary>
        public const float HearRadius = 14f;

        public static int Clamp(int coralsLit) => Mathf.Clamp(coralsLit, 0, KirtaCoralRules.CoralCount);

        public static int LitBranches(int coralsLit) => Mathf.Min(Branches, BranchesLitAtStart + Clamp(coralsLit));

        /// <summary>Whether branch <paramref name="branch"/> (the kit's index) is live coral with <paramref name="coralsLit"/> corals of the reef lit.</summary>
        public static bool BranchLit(int branch, int coralsLit)
        {
            int rank = Array.IndexOf(RelightOrder, branch);
            return rank >= 0 && rank < LitBranches(coralsLit);
        }

        /// <summary>All five lit: the trunk and the rim are live coral too.</summary>
        public static bool Whole(int coralsLit) => Clamp(coralsLit) >= KirtaCoralRules.CoralCount;

        /// <summary>0–1: how much of the mother coral glows (for the live ring round it).</summary>
        public static float Glow(int coralsLit) => Clamp(coralsLit) / (float)KirtaCoralRules.CoralCount;

        /// <summary>Whether the mother coral has news for the hero: more corals lit than she last showed.</summary>
        public static bool HasNews(int coralsLit, long seen) => Clamp(coralsLit) > Math.Max(0L, seen);
    }

    /// <summary>
    /// The Palafitas de Tarchon fill up as the cousins wake the neighbours (33, Docs/Mapas100.md §3.3: "a cidade fica mais cheia e alegre
    /// conforme os primos ajudam"), pure and server-runnable. Every neighbour ever woken counts (<see cref="TarchonLanternRules.TotalKey"/>);
    /// at each threshold one more trol who had wandered off staring at the wisps comes home and stands in the town
    /// (<c>CityTownRecipes.TarchonCrowdSpots</c>), and from the second one on the plaza hangs more lanterns (the recipe's
    /// <c>CityTownRecipes.Tarchon(int)</c>).
    /// <code>
    /// neighbours ever woken: 3  10  20  35  55  80
    /// trols back home:       1   2   3   4   5   6   (Aule, Seianti, Thana, Sethre, Hasti, Tite)
    /// lanterns on the plaza: 2 more from 2 back, 2 more again from 5 back
    /// </code>
    /// Decision (2026-11-23): five wakes a day at most, so the whole town is back in about sixteen days of play; nothing is lost when a day is
    /// skipped. Online each player's own count fills their own view of the town (the server keeps the total per player).
    /// </summary>
    public static class TarchonCrowdRules
    {
        public static readonly int[] Thresholds = { 3, 10, 20, 35, 55, 80 };

        public static int MaxReturned => Thresholds.Length;

        /// <summary>Time mark: how many trols the hero has already seen come home (each one says hello once).</summary>
        public const string SeenKey = "tarchon.crowd.seen";

        /// <summary>How many trols are back home after <paramref name="totalWoken"/> neighbours were ever woken.</summary>
        public static int Returned(long totalWoken)
        {
            int count = 0;
            while (count < Thresholds.Length && totalWoken >= Thresholds[count])
            {
                count++;
            }

            return count;
        }

        /// <summary>Wakes still needed for the next trol to come home (0 when all are back).</summary>
        public static long ToNext(long totalWoken)
        {
            int returned = Returned(totalWoken);
            return returned >= Thresholds.Length ? 0L : Thresholds[returned] - Math.Max(0L, totalWoken);
        }

        /// <summary>Pairs of extra lanterns on the plaza (0–2) for <paramref name="returned"/> trols back.</summary>
        public static int LanternPairs(int returned) => returned >= 5 ? 2 : returned >= 2 ? 1 : 0;

        /// <summary>Whether a trol came home that the hero has not seen yet.</summary>
        public static bool HasNews(int returned, long seen) => returned > Math.Max(0L, seen);
    }

    /// <summary>
    /// Larthia's Cesto de Lanternas buys the freed will-o'-wisps (33, Docs/Mapas100.md §3.3: "compra fogos-fátuos libertados"), pure and
    /// server-runnable. The wisp a dimmed neighbour was staring at goes free when he wakes and follows the hero (a wisp-white stroke at their
    /// side); [E] at Larthia's door sells every wisp held for <see cref="PricePerWisp"/> gold each, and once twenty are sold she weaves a
    /// Lanterna de Fogo-Fátuo for the cousins (<see cref="TarchonLanternRules.WispLanternItem"/>, once per save).
    /// <code>
    /// wake a neighbour: +1 wisp held (up to 10; past that it flies off to the sky)
    /// [E] within 2.2 m of Larthia's door with wisps: 25 gold each, all at once
    /// 20 sold in all: the lantern, once
    /// </code>
    /// Decision (2026-11-23): the wisps are a count in the save, not a bag item (they are light, they weigh nothing and only Larthia wants them).
    /// At five wakes a day it adds up to 125 gold a day on top of Thanchvil's purse; the lantern gift lands on the fourth full day.
    /// </summary>
    public static class LarthiaWispRules
    {
        public const int PricePerWisp = 25;
        public const int MaxHeld = 10;
        public const int LanternGiftAt = 20;
        public const float TradeReach = 2.2f;

        /// <summary>Time mark: wisps the hero holds now.</summary>
        public const string HeldKey = "tarchon.wisps.held";

        /// <summary>Time mark: wisps ever sold to Larthia.</summary>
        public const string SoldKey = "tarchon.wisps.sold";

        /// <summary>Time mark set once Larthia gave her lantern.</summary>
        public const string GiftKey = "tarchon.wisps.lantern_gift";

        /// <summary>The wisps held after one more goes free (never past <see cref="MaxHeld"/>).</summary>
        public static int Catch(long held) => (int)Math.Min(MaxHeld, Math.Max(0L, held) + 1L);

        /// <summary>Whether one more wisp would not fit (it flies off to the sky).</summary>
        public static bool Full(long held) => held >= MaxHeld;

        public static int PriceFor(long held) => (int)Math.Max(0L, Math.Min(MaxHeld, held)) * PricePerWisp;

        public static bool CanSell(long held, float distance, bool alive) => alive && held > 0 && distance <= TradeReach;

        /// <summary>Whether Larthia weaves her lantern now: the sold total reached <see cref="LanternGiftAt"/> and she has not given it yet.</summary>
        public static bool GiftDue(long soldAfter, bool given) => !given && soldAfter >= LanternGiftAt;
    }

    /// <summary>What the Tarchon daily does when the hero comes into town (<see cref="TarchonDailyRules.Decide"/>).</summary>
    public enum TarchonDailyStep
    {
        /// <summary>Today's daily already started (running or done).</summary>
        None = 0,

        /// <summary>A new game day: forget yesterday's run and start today's.</summary>
        Begin = 1,
    }

    /// <summary>
    /// The daily of Despertar o vizinho (33, Docs/Mapas100.md §3.3: "conta para a missão diária"), pure and server-runnable: a real
    /// <c>QuestType.Daily</c> quest (<c>CityDailyQuestCatalog.TarchonWake</c>, in the quest log with the "Diária" badge) that old Tarchon
    /// hands out the first time the cousins walk into the stilts on each game day: wake <see cref="Required"/> neighbours. Each wake counts one
    /// (<c>QuestManager.UpdateObjectiveProgress</c> on <see cref="ObjectiveTarget"/>), the quest pays through the normal quest path.
    /// Decision (2026-11-23): it runs on the game day, like the neighbours' own marks (they dim again every night), not on the calendar day of
    /// <c>DailyQuestService</c>'s random pool: it belongs to the town, so it is always offered there and never shows up far from it.
    /// Three of five so the purse for all five stays the extra.
    /// </summary>
    public static class TarchonDailyRules
    {
        public const string QuestId = "quest_daily_tarchon_wake_neighbours";
        public const string ObjectiveId = "wake_neighbours";
        public const string ObjectiveTarget = "tarchon_neighbour_woken";
        public const int Required = 3;
        public const int RewardGold = 90;
        public const int AuthoredXp = 450;

        /// <summary>Daily mark: the game day Tarchon last handed the daily out.</summary>
        public const string StartedKey = "tarchon.neighbours.daily";

        public static TarchonDailyStep Decide(int startedDay, int today) => startedDay == today && today > 0 ? TarchonDailyStep.None : TarchonDailyStep.Begin;

        /// <summary>Progress to credit when the daily starts late in a day some neighbours were already woken.</summary>
        public static int Credit(int awakeToday) => Mathf.Clamp(awakeToday, 0, Required);
    }
}
