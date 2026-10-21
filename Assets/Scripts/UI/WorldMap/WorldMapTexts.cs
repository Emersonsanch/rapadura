using System.Collections.Generic;
using System.Linq;
using Rapadura.Core.Localization;
using Rapadura.UI.Common;

namespace Rapadura.UI.WorldMap
{
    /// <summary>
    /// Localization keys (en + pt-BR) for the world map HUD (<see cref="WorldMapHudController"/>,
    /// WorldMapHudView.uxml): clock/weather readout, "Vitórias: 2/3 para desbloquear Deserto",
    /// region labels and the read-only Mochila summary. Same self-contained pattern as
    /// <see cref="UiTexts"/>/<c>ReturnToTitleTexts</c>: merged into the live
    /// <see cref="LocalizationManager"/> on first use (keys it already has win), pt-BR fallback when
    /// no manager is registered. Button captions reuse the HUD/pause keys from <see cref="UiTexts"/>
    /// and region names the core <c>biome.&lt;x&gt;.name</c> keys; <see cref="Get"/> resolves those too, so
    /// it can be the <see cref="UiDocumentLocalizer"/> resolver for the whole HUD document.
    /// </summary>
    public static class WorldMapTexts
    {
        // -- Top bar --
        public const string NoHero = "ui.worldmap.no_hero";
        /// <summary>{0} = day, {1} = hour (0-23), {2} = phase name.</summary>
        public const string Clock = "ui.worldmap.clock";
        public const string PhaseDawn = "ui.worldmap.phase.dawn";
        public const string PhaseDay = "ui.worldmap.phase.day";
        public const string PhaseDusk = "ui.worldmap.phase.dusk";
        public const string PhaseNight = "ui.worldmap.phase.night";
        public const string WeatherClear = "ui.worldmap.weather.clear";
        public const string WeatherRain = "ui.worldmap.weather.rain";
        public const string WeatherFog = "ui.worldmap.weather.fog";
        public const string WeatherSandstorm = "ui.worldmap.weather.sandstorm";
        public const string WeatherSnow = "ui.worldmap.weather.snow";
        /// <summary>{0} = victories, {1} = required, {2} = region to unlock (victories counted in the current region).</summary>
        public const string Victories = "ui.worldmap.victories";
        /// <summary>Like <see cref="Victories"/>, {3} = the (other) region the victories must be won in.</summary>
        public const string VictoriesIn = "ui.worldmap.victories_in";
        /// <summary>Victories done but the region is still gated (boss challenge): {0} = region to unlock, {1} = region whose boss must fall.</summary>
        public const string VictoriesBoss = "ui.worldmap.victories_boss";
        public const string AllUnlocked = "ui.worldmap.all_unlocked";
        /// <summary>Boss-gated goal in the current region: {0} = victories, {1} = required (wins only open the boss fight).</summary>
        public const string VictoriesToBoss = "ui.worldmap.victories_to_boss";
        /// <summary>Boss-gated goal in another region: {0} = victories, {1} = required, {2} = region to win them in.</summary>
        public const string VictoriesToBossIn = "ui.worldmap.victories_to_boss_in";
        /// <summary>Victories done in the current region, its boss is next: {0} = region the boss unlocks.</summary>
        public const string VictoriesBossHere = "ui.worldmap.victories_boss_here";
        /// <summary>The current region's boss is beaten: {0} = region, {1} = the next goal's text.</summary>
        public const string RegionDoneNext = "ui.worldmap.region_done_next";
        /// <summary>The current region's boss is beaten and nothing else is left: {0} = region.</summary>
        public const string RegionDoneAll = "ui.worldmap.region_done_all";

        // -- Region labels --
        public const string LabelHere = "ui.worldmap.label.here";
        public const string LabelTravel = "ui.worldmap.label.travel";
        /// <summary>{0} = victories required, {1} = region to win them in, {2} = victories so far.</summary>
        public const string LabelLocked = "ui.worldmap.label.locked";
        /// <summary>Locked label once the victories are done (boss gate): {0} = region whose boss must fall.</summary>
        public const string LabelBoss = "ui.worldmap.label.boss";
        /// <summary>Locked label behind a boss gate: {0} = victories required, {1} = region (battles + its boss), {2} = victories so far.</summary>
        public const string LabelLockedBoss = "ui.worldmap.label.locked_boss";
        /// <summary>Status line of a region whose boss is beaten (shown when no "aqui"/"viajar" hint applies).</summary>
        public const string LabelCleared = "ui.worldmap.label.cleared";

        // -- Region boss challenge --
        /// <summary>"Enfrentar o chefe" button (shown while RegionBossProgress.CanChallengeBoss holds for the current region).</summary>
        public const string BossChallenge = "ui.worldmap.boss_challenge";
        public const string LabelLockedGeneric = "ui.worldmap.label.locked_generic";

        // -- Mochila (read-only summary) --
        public const string BackpackItems = "ui.worldmap.backpack.items";
        public const string BackpackGold = "ui.worldmap.backpack.gold";
        public const string BackpackEquipped = "ui.worldmap.backpack.equipped";
        public const string BackpackItemRow = "ui.worldmap.backpack.item_row";
        public const string BackpackMore = "ui.worldmap.backpack.more";
        public const string BackpackEmpty = "ui.worldmap.backpack.empty";
        public const string BackpackHint = "ui.worldmap.backpack.hint";
        public const string Close = "ui.worldmap.close";

        public static readonly List<LocalizationEntry> Entries = new List<LocalizationEntry>
        {
            new LocalizationEntry(NoHero, "No hero", "Sem herói"),
            new LocalizationEntry(Clock, "Day {0} · {1:00}:00 · {2}", "Dia {0} · {1:00}h · {2}"),
            new LocalizationEntry(PhaseDawn, "Dawn", "Amanhecer"),
            new LocalizationEntry(PhaseDay, "Day", "Dia"),
            new LocalizationEntry(PhaseDusk, "Dusk", "Entardecer"),
            new LocalizationEntry(PhaseNight, "Night", "Noite"),
            new LocalizationEntry(WeatherClear, "Clear", "Céu limpo"),
            new LocalizationEntry(WeatherRain, "Rain", "Chuva"),
            new LocalizationEntry(WeatherFog, "Fog", "Neblina"),
            new LocalizationEntry(WeatherSandstorm, "Sandstorm", "Tempestade de areia"),
            new LocalizationEntry(WeatherSnow, "Snow", "Neve"),
            new LocalizationEntry(Victories, "Victories: {0}/{1} to unlock {2}", "Vitórias: {0}/{1} para desbloquear {2}"),
            new LocalizationEntry(VictoriesIn, "Victories in {3}: {0}/{1} to unlock {2}", "Vitórias em {3}: {0}/{1} para desbloquear {2}"),
            new LocalizationEntry(VictoriesBoss, "Beat the {1} boss to unlock {0}", "Derrote o chefe de {1} para desbloquear {0}"),
            new LocalizationEntry(AllUnlocked, "Every region unlocked", "Todas as regiões desbloqueadas"),
            new LocalizationEntry(VictoriesToBoss, "Victories: {0}/{1} to face the boss", "Vitórias: {0}/{1} para enfrentar o chefe"),
            new LocalizationEntry(VictoriesToBossIn, "Win {1} battles in {2} to face the boss ({0}/{1})", "Vença {1} batalhas em {2} para enfrentar o chefe ({0}/{1})"),
            new LocalizationEntry(VictoriesBossHere, "Beat the boss to unlock {0}", "Derrote o chefe para desbloquear {0}"),
            new LocalizationEntry(RegionDoneNext, "{0} cleared! Next: {1}", "Região {0} concluída! Próximo: {1}"),
            new LocalizationEntry(RegionDoneAll, "{0} cleared! Every region unlocked", "Região {0} concluída! Todas as regiões desbloqueadas"),
            new LocalizationEntry(LabelHere, "You are here", "Você está aqui"),
            new LocalizationEntry(LabelTravel, "Tap to travel", "Toque para viajar"),
            new LocalizationEntry(LabelLocked, "Win {0} battles in {1} ({2}/{0})", "Vença {0} batalhas em {1} ({2}/{0})"),
            new LocalizationEntry(LabelBoss, "Beat the {0} boss", "Derrote o chefe de {0}"),
            new LocalizationEntry(LabelLockedBoss, "Win {0} battles in {1}, then beat its boss ({2}/{0})", "Vença {0} batalhas em {1} e derrote o chefe ({2}/{0})"),
            new LocalizationEntry(LabelCleared, "Cleared", "Concluída"),
            new LocalizationEntry(LabelLockedGeneric, "Locked", "Bloqueado"),
            new LocalizationEntry(BossChallenge, "Face the boss", "Enfrentar o chefe"),
            new LocalizationEntry(BackpackItems, "Items: {0} ({1}/{2} slots)", "Itens: {0} ({1}/{2} espaços)"),
            new LocalizationEntry(BackpackGold, "Gold: {0}", "Ouro: {0}"),
            new LocalizationEntry(BackpackEquipped, "Equipped: {0}", "Equipados: {0}"),
            new LocalizationEntry(BackpackItemRow, "{0} ×{1}", "{0} ×{1}"),
            new LocalizationEntry(BackpackMore, "+{0} more", "+{0} outros"),
            new LocalizationEntry(BackpackEmpty, "The backpack is empty.", "A mochila está vazia."),
            new LocalizationEntry(BackpackHint, "Items are used and organized during adventures.", "Os itens são usados e organizados durante as aventuras."),
            new LocalizationEntry(Close, "Close", "Fechar"),
        };

        private static Dictionary<string, string> _fallbackByKey;
        private static LocalizationManager _mergedInto;

        /// <summary>Merges <see cref="Entries"/> into <paramref name="manager"/>, skipping keys it already has. Safe to call repeatedly.</summary>
        public static void EnsureRegistered(LocalizationManager manager)
        {
            if (manager == null || (ReferenceEquals(manager, _mergedInto) && manager.HasKey(Entries[0].Key)))
            {
                return;
            }

            manager.LoadEntries(Entries.Where(entry => !manager.HasKey(entry.Key)).ToList());
            _mergedInto = manager;
        }

        /// <summary>
        /// Localized text via <paramref name="manager"/> when it knows the key (ours, <see cref="UiTexts"/>'
        /// or the core table's), else the pt-BR fallback, else the key.
        /// </summary>
        public static string Get(LocalizationManager manager, string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return string.Empty;
            }

            if (manager != null)
            {
                EnsureRegistered(manager);
                UiTexts.EnsureRegistered(manager);
                if (manager.HasKey(key))
                {
                    return manager.Get(key);
                }
            }

            return GetFallback(key) ?? key;
        }

        /// <summary><see cref="Get"/> + <see cref="string.Format(string, object[])"/>, tolerant of malformed translations.</summary>
        public static string Format(LocalizationManager manager, string key, params object[] args)
        {
            string pattern = Get(manager, key);
            try
            {
                return string.Format(pattern, args);
            }
            catch (System.FormatException)
            {
                return string.Format(GetFallback(key) ?? key, args);
            }
        }

        /// <summary>pt-BR text from <see cref="Entries"/>, <see cref="UiTexts"/> or the core table (e.g. <c>biome.forest.name</c>), or null.</summary>
        public static string GetFallback(string key)
        {
            if (key == null)
            {
                return null;
            }

            if (_fallbackByKey == null)
            {
                var table = new Dictionary<string, string>();
                foreach (LocalizationEntry entry in Entries)
                {
                    table[entry.Key] = entry.PtBR;
                }

                foreach (LocalizationEntry entry in LocalizationManager.DefaultEntries.All)
                {
                    if (entry != null && !string.IsNullOrEmpty(entry.Key) && !table.ContainsKey(entry.Key))
                    {
                        table[entry.Key] = entry.PtBR;
                    }
                }

                _fallbackByKey = table;
            }

            return _fallbackByKey.TryGetValue(key, out string text) ? text : UiTexts.GetFallback(key);
        }
    }
}
