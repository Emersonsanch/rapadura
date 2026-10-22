using System;
using System.Collections.Generic;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Content;
using Rapadura.Gameplay.Environment;
using Rapadura.Gameplay.Inventory;
using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.World.Biomes;
using Rapadura.Gameplay.World.Map;
using Rapadura.UI.Common;
using UnityEngine;

namespace Rapadura.UI.WorldMap
{
    /// <summary>What a world map region label says about its region (see <see cref="WorldMapHudPresenter.GetLabelStatus"/>).</summary>
    public enum RegionLabelStatus
    {
        /// <summary>Unlocked but not reachable right now (not adjacent, or the token is walking).</summary>
        None = 0,
        /// <summary>The token stands here: "Você está aqui".</summary>
        Current = 1,
        /// <summary>Unlocked and adjacent: "Toque para viajar".</summary>
        Travelable = 2,
        /// <summary>Still locked: lock icon + the unlock requirement.</summary>
        Locked = 3,
    }

    /// <summary>The next region the player is working toward (see <see cref="WorldMapHudPresenter.FindNextUnlock"/>).</summary>
    public readonly struct UnlockGoal
    {
        public readonly bool HasGoal;
        public readonly BiomeType Target;
        public readonly BiomeType RequiredRegion;
        public readonly int Victories;
        public readonly int Required;

        public UnlockGoal(BiomeType target, BiomeType requiredRegion, int victories, int required)
        {
            HasGoal = true;
            Target = target;
            RequiredRegion = requiredRegion;
            Victories = victories;
            Required = required;
        }

        public static UnlockGoal None => default;
    }

    /// <summary>Read-only view of the carried Inventory state (the Player, who owns it, lives in Main.unity).</summary>
    public sealed class BackpackSummary
    {
        public int UsedSlots;
        public int TotalSlots;
        public int TotalItems;
        public int Gold;
        public int Equipped;
        /// <summary>Non-currency stacks merged by item id, biggest first (ties by id).</summary>
        public readonly List<KeyValuePair<string, int>> Items = new List<KeyValuePair<string, int>>();

        public bool IsEmpty => TotalItems == 0 && Gold == 0 && Equipped == 0;
    }

    /// <summary>
    /// Pure formatting/decision logic behind <see cref="WorldMapHudController"/> (no UIDocument, no
    /// scene), so it is EditMode-testable: hero/level, region and clock/weather text, the
    /// "Vitórias: 2/3 para enfrentar o chefe" / "Derrote o chefe para desbloquear Deserto" goal
    /// (<see cref="GoalText"/>), region label status (with a cleared marker) and the Mochila summary
    /// built from SaveManager's carried JSON. Victories only open a region's boss fight; beating the
    /// boss unlocks the next region (<see cref="RegionBossProgress"/>). Every method taking an
    /// optional <see cref="RegionBossProgress"/> treats null as "every region is boss-gated" and
    /// reads cleared regions from <see cref="WorldMapProgress.IsRegionCleared"/>.
    /// </summary>
    public static class WorldMapHudPresenter
    {
        public const string DefaultCurrencyItemId = "item_gold";

        private static readonly BiomeType[] AllRegions = (BiomeType[])Enum.GetValues(typeof(BiomeType));

        private static readonly string[] WeatherIconClasses =
        {
            "wm-weather-icon--clear",
            "wm-weather-icon--rain",
            "wm-weather-icon--fog",
            "wm-weather-icon--sandstorm",
            "wm-weather-icon--snow",
        };

        private static Dictionary<string, string> _contentFallback;

        // ------------------------------------------------------------------
        // Top bar
        // ------------------------------------------------------------------

        public static string HeroName(LocalizationManager manager, CharacterId? hero)
        {
            return hero.HasValue
                ? CharacterRegistry.Get(hero.Value).LocalizedName(manager)
                : WorldMapTexts.Get(manager, WorldMapTexts.NoHero);
        }

        public static string LevelText(LocalizationManager manager, int level)
        {
            return UiTexts.Format(manager, UiTexts.HudLevel, Mathf.Max(1, level));
        }

        public static string RegionNameKey(BiomeType region)
        {
            switch (region)
            {
                case BiomeType.Desert: return "biome.desert.name";
                case BiomeType.Mountain: return "biome.mountain.name";
                case BiomeType.Caves: return "biome.caves.name";
                default: return "biome.forest.name";
            }
        }

        public static string RegionName(LocalizationManager manager, BiomeType region)
        {
            return WorldMapTexts.Get(manager, RegionNameKey(region));
        }

        public static string PhaseKey(TimeOfDayPhase phase)
        {
            switch (phase)
            {
                case TimeOfDayPhase.Dawn: return WorldMapTexts.PhaseDawn;
                case TimeOfDayPhase.Dusk: return WorldMapTexts.PhaseDusk;
                case TimeOfDayPhase.Night: return WorldMapTexts.PhaseNight;
                default: return WorldMapTexts.PhaseDay;
            }
        }

        /// <summary>"Dia 3 · 14h · Dia" (hour wrapped to 0-23, day at least 1).</summary>
        public static string ClockText(LocalizationManager manager, int day, int hour, TimeOfDayPhase phase)
        {
            int wrappedHour = ((hour % 24) + 24) % 24;
            return WorldMapTexts.Format(manager, WorldMapTexts.Clock, Mathf.Max(1, day), wrappedHour, WorldMapTexts.Get(manager, PhaseKey(phase)));
        }

        public static string WeatherKey(WeatherType weather)
        {
            switch (weather)
            {
                case WeatherType.Rain: return WorldMapTexts.WeatherRain;
                case WeatherType.Fog: return WorldMapTexts.WeatherFog;
                case WeatherType.Sandstorm: return WorldMapTexts.WeatherSandstorm;
                case WeatherType.Snow: return WorldMapTexts.WeatherSnow;
                default: return WorldMapTexts.WeatherClear;
            }
        }

        public static string WeatherText(LocalizationManager manager, WeatherType weather) => WorldMapTexts.Get(manager, WeatherKey(weather));

        /// <summary>USS modifier class drawing the weather icon (no allocation: shared strings).</summary>
        public static string WeatherIconClass(WeatherType weather)
        {
            int index = (int)weather;
            return index >= 0 && index < WeatherIconClasses.Length ? WeatherIconClasses[index] : WeatherIconClasses[0];
        }

        /// <summary>Every icon class, so the controller can clear the previous one before adding the new.</summary>
        public static IReadOnlyList<string> AllWeatherIconClasses => WeatherIconClasses;

        // ------------------------------------------------------------------
        // Unlock goal ("Vitórias: 2/3 para desbloquear Deserto")
        // ------------------------------------------------------------------

        /// <summary>
        /// The locked region the player is closest to opening. Only rules whose required region is
        /// already unlocked count (the "frontier"); a rule gated by victories in the current region
        /// wins over any other, then the fewest victories still missing, then enum order.
        /// <see cref="UnlockGoal.None"/> when nothing is left to unlock (or no graph/progress).
        /// </summary>
        public static UnlockGoal FindNextUnlock(WorldMapGraph graph, WorldMapProgress progress)
        {
            if (graph == null || progress == null)
            {
                return UnlockGoal.None;
            }

            UnlockGoal best = UnlockGoal.None;
            bool bestIsCurrent = false;
            int bestMissing = int.MaxValue;

            foreach (BiomeType region in AllRegions)
            {
                if (progress.IsUnlocked(region) || !graph.TryGetUnlockRule(region, out BiomeType required, out int needed))
                {
                    continue;
                }

                if (!progress.IsUnlocked(required))
                {
                    continue;
                }

                int victories = progress.GetVictories(required);
                int missing = Mathf.Max(0, needed - victories);
                bool isCurrent = required == progress.CurrentRegion;

                bool better = !best.HasGoal
                              || (isCurrent && !bestIsCurrent)
                              || (isCurrent == bestIsCurrent && missing < bestMissing);

                if (better)
                {
                    best = new UnlockGoal(region, required, Mathf.Min(victories, needed), needed);
                    bestIsCurrent = isCurrent;
                    bestMissing = missing;
                }
            }

            return best;
        }

        /// <summary>Whether <paramref name="region"/>'s victories only open a boss fight (null bosses = always).</summary>
        public static bool IsBossGated(RegionBossProgress bosses, BiomeType region) => bosses == null || bosses.HasBoss(region);

        /// <summary>The region's boss is beaten (via <paramref name="bosses"/> when given, else the progress flag).</summary>
        public static bool IsCleared(WorldMapProgress progress, RegionBossProgress bosses, BiomeType region)
        {
            if (bosses != null)
            {
                return bosses.IsRegionCleared(region);
            }

            return progress != null && progress.IsRegionCleared(region);
        }

        /// <summary>
        /// Text for one unlock goal. With the boss gate (<paramref name="bossGated"/>, the game's rule)
        /// victories lead to the boss: "Vitórias: 1/3 para enfrentar o chefe", then "Derrote o chefe
        /// para desbloquear Deserto"; goals in another region name it. Without it (a region with no
        /// boss) the victories open the target directly: "Vitórias: 1/3 para desbloquear Deserto".
        /// </summary>
        public static string VictoriesText(LocalizationManager manager, UnlockGoal goal, BiomeType currentRegion, bool bossGated = true)
        {
            if (!goal.HasGoal)
            {
                return WorldMapTexts.Get(manager, WorldMapTexts.AllUnlocked);
            }

            string target = RegionName(manager, goal.Target);
            bool here = goal.RequiredRegion == currentRegion;

            // Victories done but still locked: the region boss gate (RegionBossProgress) is what's left.
            if (goal.Victories >= goal.Required)
            {
                return here
                    ? WorldMapTexts.Format(manager, WorldMapTexts.VictoriesBossHere, target)
                    : WorldMapTexts.Format(manager, WorldMapTexts.VictoriesBoss, target, RegionName(manager, goal.RequiredRegion));
            }

            if (bossGated)
            {
                return here
                    ? WorldMapTexts.Format(manager, WorldMapTexts.VictoriesToBoss, goal.Victories, goal.Required)
                    : WorldMapTexts.Format(manager, WorldMapTexts.VictoriesToBossIn, goal.Victories, goal.Required, RegionName(manager, goal.RequiredRegion));
            }

            return here
                ? WorldMapTexts.Format(manager, WorldMapTexts.Victories, goal.Victories, goal.Required, target)
                : WorldMapTexts.Format(manager, WorldMapTexts.VictoriesIn, goal.Victories, goal.Required, target, RegionName(manager, goal.RequiredRegion));
        }

        /// <summary>
        /// The HUD's goal line for where the player stands. A cleared current region says so first
        /// ("Região Floresta concluída! Próximo: Vença 3 batalhas em Deserto para enfrentar o chefe (0/3)")
        /// instead of a bare goal about another region; an uncleared region whose boss is the only thing
        /// left (the last region, which gates nothing) shows its own boss progress; otherwise
        /// <see cref="VictoriesText"/> for <see cref="FindNextUnlock"/>.
        /// </summary>
        public static string GoalText(LocalizationManager manager, WorldMapGraph graph, WorldMapProgress progress, RegionBossProgress bosses = null)
        {
            if (progress == null)
            {
                return string.Empty;
            }

            BiomeType current = progress.CurrentRegion;
            UnlockGoal goal = FindNextUnlock(graph, progress);

            if (IsCleared(progress, bosses, current))
            {
                return goal.HasGoal
                    ? WorldMapTexts.Format(manager, WorldMapTexts.RegionDoneNext, RegionName(manager, current),
                        VictoriesText(manager, goal, current, IsBossGated(bosses, goal.RequiredRegion)))
                    : WorldMapTexts.Format(manager, WorldMapTexts.RegionDoneAll, RegionName(manager, current));
            }

            if (graph != null && IsBossGated(bosses, current) && (!goal.HasGoal || goal.RequiredRegion != current))
            {
                int needed = graph.GetVictoriesToChallengeBoss(current);
                int victories = Mathf.Min(progress.GetVictories(current), needed);
                return victories >= needed
                    ? WorldMapTexts.Format(manager, WorldMapTexts.LabelBoss, RegionName(manager, current))
                    : WorldMapTexts.Format(manager, WorldMapTexts.VictoriesToBoss, victories, needed);
            }

            return VictoriesText(manager, goal, current, IsBossGated(bosses, goal.RequiredRegion));
        }

        // ------------------------------------------------------------------
        // Region labels
        // ------------------------------------------------------------------

        public static RegionLabelStatus GetLabelStatus(WorldMapGraph graph, WorldMapProgress progress, BiomeType region, bool isTraveling)
        {
            if (progress == null)
            {
                return RegionLabelStatus.None;
            }

            if (!progress.IsUnlocked(region))
            {
                return RegionLabelStatus.Locked;
            }

            if (region == progress.CurrentRegion)
            {
                // While walking away the token is between nodes; keep "Você está aqui" on the origin
                // until WorldMapProgress moves (arrival or an encounter).
                return RegionLabelStatus.Current;
            }

            return !isTraveling && graph != null && graph.CanTravel(progress, region)
                ? RegionLabelStatus.Travelable
                : RegionLabelStatus.None;
        }

        /// <summary>
        /// Whether a region label shows its full card (name + status line) or only the compact name
        /// chip. Full only for the current region, travelable neighbours and a locked region adjacent
        /// to the current one (its unlock requirement is the next goal); far regions stay compact so
        /// cards don't pile up on each other (playtest: Montanha's card covered Deserto/Cavernas).
        /// </summary>
        public static bool ShowFullLabel(WorldMapGraph graph, WorldMapProgress progress, BiomeType region, RegionLabelStatus status)
        {
            switch (status)
            {
                case RegionLabelStatus.Current:
                case RegionLabelStatus.Travelable:
                    return true;
                case RegionLabelStatus.Locked:
                    return graph != null && progress != null && graph.AreConnected(progress.CurrentRegion, region);
                default:
                    return false;
            }
        }

        /// <summary>Placement priority of a label for <see cref="ResolveLabelOverlaps"/> (lower = placed first, never moved).</summary>
        public static int LabelPriority(RegionLabelStatus status, bool fullCard)
        {
            switch (status)
            {
                case RegionLabelStatus.Current:
                    return 0;
                case RegionLabelStatus.Travelable:
                    return 1;
                default:
                    return fullCard ? 2 : 3;
            }
        }

        /// <summary>
        /// Simple screen-space overlap avoidance for region labels. <paramref name="rects"/> are the
        /// desired panel-space rects (y down) of <paramref name="count"/> labels; labels are placed in
        /// <paramref name="priorities"/> order (lower first, ties by index) and each later one is
        /// nudged up or down — whichever is the smaller move — until it clears every placed label by
        /// <paramref name="gap"/>. Writes the vertical shift per label into <paramref name="yOffsets"/>.
        /// Labels with an empty rect (not laid out / off screen) are ignored. No allocation.
        /// </summary>
        public static void ResolveLabelOverlaps(Rect[] rects, int[] priorities, int count, float gap, float[] yOffsets, int[] orderBuffer)
        {
            for (int i = 0; i < count; i++)
            {
                yOffsets[i] = 0f;
                orderBuffer[i] = i;
            }

            // Insertion sort by (priority, index): stable and allocation-free for a handful of labels.
            for (int i = 1; i < count; i++)
            {
                int current = orderBuffer[i];
                int j = i - 1;

                while (j >= 0 && priorities[orderBuffer[j]] > priorities[current])
                {
                    orderBuffer[j + 1] = orderBuffer[j];
                    j--;
                }

                orderBuffer[j + 1] = current;
            }

            for (int placedCount = 0; placedCount < count; placedCount++)
            {
                int index = orderBuffer[placedCount];
                Rect rect = rects[index];

                if (rect.width <= 0f || rect.height <= 0f)
                {
                    continue;
                }

                // Candidate spots: where it wants to be, or just above / just below any placed label.
                // Take the free one closest to the desired spot (the spot below the lowest placed
                // label is always free, so this always terminates with a clear position).
                float bestShift = 0f;
                bool found = IsFree(rect, 0f, rects, yOffsets, orderBuffer, placedCount, gap);

                for (int k = 0; k < placedCount && !(found && bestShift == 0f); k++)
                {
                    int other = orderBuffer[k];
                    Rect placed = rects[other];
                    placed.y += yOffsets[other];

                    if (placed.width <= 0f || placed.height <= 0f)
                    {
                        continue;
                    }

                    // Small epsilon so float rounding can't make an exactly-touching spot count as overlapping.
                    float above = placed.yMin - gap - rect.yMax - 0.01f;
                    float below = placed.yMax + gap - rect.yMin + 0.01f;

                    TryCandidate(above, ref found, ref bestShift, rect, rects, yOffsets, orderBuffer, placedCount, gap);
                    TryCandidate(below, ref found, ref bestShift, rect, rects, yOffsets, orderBuffer, placedCount, gap);
                }

                yOffsets[index] = bestShift;
            }
        }

        private static void TryCandidate(float shift, ref bool found, ref float bestShift, Rect rect, Rect[] rects, float[] yOffsets,
            int[] orderBuffer, int placedCount, float gap)
        {
            if (found && Mathf.Abs(shift) >= Mathf.Abs(bestShift))
            {
                return;
            }

            if (IsFree(rect, shift, rects, yOffsets, orderBuffer, placedCount, gap))
            {
                found = true;
                bestShift = shift;
            }
        }

        private static bool IsFree(Rect rect, float shift, Rect[] rects, float[] yOffsets, int[] orderBuffer, int placedCount, float gap)
        {
            rect.y += shift;

            for (int k = 0; k < placedCount; k++)
            {
                int other = orderBuffer[k];
                Rect placed = rects[other];
                placed.y += yOffsets[other];

                if (placed.width > 0f && placed.height > 0f && OverlapsWithGap(rect, placed, gap))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool OverlapsWithGap(Rect a, Rect b, float gap)
        {
            return a.xMin < b.xMax + gap && a.xMax + gap > b.xMin && a.yMin < b.yMax + gap && a.yMax + gap > b.yMin;
        }

        /// <summary>
        /// Status line under a region label: "Você está aqui", "Toque para viajar", the unlock requirement
        /// of a locked region ("Vença 3 batalhas em Floresta e derrote o chefe (1/3)", then "Derrote o
        /// chefe de Floresta"), or "Concluída" for a cleared region with no other hint.
        /// </summary>
        public static string LabelStatusText(LocalizationManager manager, WorldMapGraph graph, WorldMapProgress progress, BiomeType region, RegionLabelStatus status, RegionBossProgress bosses = null)
        {
            switch (status)
            {
                case RegionLabelStatus.Current:
                    return WorldMapTexts.Get(manager, WorldMapTexts.LabelHere);
                case RegionLabelStatus.Travelable:
                    return WorldMapTexts.Get(manager, WorldMapTexts.LabelTravel);
                case RegionLabelStatus.Locked:
                    if (graph != null && progress != null && graph.TryGetUnlockRule(region, out BiomeType required, out int needed))
                    {
                        int victories = Mathf.Min(progress.GetVictories(required), needed);
                        bool gated = IsBossGated(bosses, required);
                        if (gated && victories >= needed)
                        {
                            return WorldMapTexts.Format(manager, WorldMapTexts.LabelBoss, RegionName(manager, required));
                        }

                        return WorldMapTexts.Format(manager, gated ? WorldMapTexts.LabelLockedBoss : WorldMapTexts.LabelLocked, needed, RegionName(manager, required), victories);
                    }

                    return WorldMapTexts.Get(manager, WorldMapTexts.LabelLockedGeneric);
                default:
                    return IsCleared(progress, bosses, region) ? WorldMapTexts.Get(manager, WorldMapTexts.LabelCleared) : string.Empty;
            }
        }

        // ------------------------------------------------------------------
        // Carried player state (level, Mochila)
        // ------------------------------------------------------------------

        /// <summary>Level from a carried <see cref="PlayerStatsSaveState"/> JSON; 1 when missing/invalid (a new hero).</summary>
        public static int ParseLevel(string playerStatsJson)
        {
            if (string.IsNullOrEmpty(playerStatsJson))
            {
                return 1;
            }

            try
            {
                PlayerStatsSaveState state = JsonUtility.FromJson<PlayerStatsSaveState>(playerStatsJson);
                return state != null ? Mathf.Max(1, state.level) : 1;
            }
            catch (ArgumentException)
            {
                return 1;
            }
        }

        /// <summary>Carried <see cref="InventorySaveState"/> JSON → state, or null when missing/invalid.</summary>
        public static InventorySaveState ParseInventory(string inventoryJson)
        {
            if (string.IsNullOrEmpty(inventoryJson))
            {
                return null;
            }

            try
            {
                return JsonUtility.FromJson<InventorySaveState>(inventoryJson);
            }
            catch (ArgumentException)
            {
                return null;
            }
        }

        public static BackpackSummary Summarize(InventorySaveState state, string currencyItemId = DefaultCurrencyItemId)
        {
            var summary = new BackpackSummary();
            if (state == null)
            {
                return summary;
            }

            var byId = new Dictionary<string, int>();

            if (state.slots != null)
            {
                summary.TotalSlots = state.slots.Length;
                foreach (InventorySlotData slot in state.slots)
                {
                    if (slot == null || string.IsNullOrEmpty(slot.itemId) || slot.quantity <= 0)
                    {
                        continue;
                    }

                    summary.UsedSlots++;

                    if (slot.itemId == currencyItemId)
                    {
                        summary.Gold += slot.quantity;
                        continue;
                    }

                    summary.TotalItems += slot.quantity;
                    byId.TryGetValue(slot.itemId, out int count);
                    byId[slot.itemId] = count + slot.quantity;
                }
            }

            if (state.equipped != null)
            {
                foreach (InventorySaveState.EquippedEntry entry in state.equipped)
                {
                    if (entry != null && !string.IsNullOrEmpty(entry.itemId))
                    {
                        summary.Equipped++;
                    }
                }
            }

            summary.Items.AddRange(byId);
            summary.Items.Sort((a, b) => a.Value != b.Value ? b.Value.CompareTo(a.Value) : string.CompareOrdinal(a.Key, b.Key));
            return summary;
        }

        /// <summary>
        /// Lines of the Mochila panel: items/slots, gold, equipped, then up to <paramref name="maxItems"/>
        /// "Machado ×1" rows and "+N outros". Just the empty message when there is nothing.
        /// <paramref name="itemName"/> defaults to <see cref="ItemDisplayName"/>.
        /// </summary>
        public static List<string> BackpackLines(LocalizationManager manager, BackpackSummary summary, int maxItems = 6, Func<string, string> itemName = null)
        {
            var lines = new List<string>();
            if (summary == null || summary.IsEmpty)
            {
                lines.Add(WorldMapTexts.Get(manager, WorldMapTexts.BackpackEmpty));
                return lines;
            }

            lines.Add(WorldMapTexts.Format(manager, WorldMapTexts.BackpackItems, summary.TotalItems, summary.UsedSlots, summary.TotalSlots));
            lines.Add(WorldMapTexts.Format(manager, WorldMapTexts.BackpackGold, summary.Gold));
            lines.Add(WorldMapTexts.Format(manager, WorldMapTexts.BackpackEquipped, summary.Equipped));

            int shown = Mathf.Min(Mathf.Max(0, maxItems), summary.Items.Count);
            for (int i = 0; i < shown; i++)
            {
                KeyValuePair<string, int> item = summary.Items[i];
                string name = itemName != null ? itemName(item.Key) : ItemDisplayName(manager, item.Key);
                lines.Add(WorldMapTexts.Format(manager, WorldMapTexts.BackpackItemRow, name, item.Value));
            }

            if (summary.Items.Count > shown)
            {
                lines.Add(WorldMapTexts.Format(manager, WorldMapTexts.BackpackMore, summary.Items.Count - shown));
            }

            return lines;
        }

        /// <summary>Localized item name from its id (ContentTexts pt-BR as fallback, then the raw id).</summary>
        public static string ItemDisplayName(LocalizationManager manager, string itemId)
        {
            if (string.IsNullOrEmpty(itemId))
            {
                return string.Empty;
            }

            string key = ContentText.ItemNameKey(itemId);
            return ContentText.Resolve(manager, key, ContentFallback(key) ?? itemId);
        }

        private static string ContentFallback(string key)
        {
            if (_contentFallback == null)
            {
                var table = new Dictionary<string, string>();
                foreach (LocalizationEntry entry in ContentTexts.Entries)
                {
                    if (entry != null && !string.IsNullOrEmpty(entry.Key))
                    {
                        table[entry.Key] = entry.PtBR;
                    }
                }

                _contentFallback = table;
            }

            return _contentFallback.TryGetValue(key, out string text) ? text : null;
        }
    }
}
