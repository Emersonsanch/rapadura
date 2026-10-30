using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Rapadura.Core.Localization;

namespace Rapadura.Gameplay.Daily
{
    /// <summary>
    /// Localization keys (en + pt-BR) of the "Fôlego diário" meter/reminders, the "Recompensa diária" popup and the
    /// parental "Lembrete de tempo de jogo" setting. Same self-contained pattern as <c>HapticsTexts</c>: merged into the
    /// live <see cref="LocalizationManager"/> on first use (keys it already has win), pt-BR fallback otherwise.
    /// </summary>
    public static class DailyTexts
    {
        public const string StaminaLabel = "ui.daily.stamina.label";
        public const string TierFull = "ui.daily.stamina.tier_full";
        public const string TierNormal = "ui.daily.stamina.tier_normal";
        public const string TierTired = "ui.daily.stamina.tier_tired";
        public const string TooltipFull = "ui.daily.stamina.tooltip_full";
        public const string TooltipNormal = "ui.daily.stamina.tooltip_normal";
        public const string TooltipTired = "ui.daily.stamina.tooltip_tired";
        public const string TooltipRest = "ui.daily.stamina.tooltip_rest";
        public const string ReminderTired = "ui.daily.stamina.reminder_tired";
        public const string ReminderPlayTime = "ui.daily.stamina.reminder_playtime";
        public const string DurationHours = "ui.daily.duration_hours";
        public const string DurationMinutes = "ui.daily.duration_minutes";

        public const string LoginTitle = "ui.daily.login.title";
        public const string LoginSubtitle = "ui.daily.login.subtitle";
        public const string LoginDay = "ui.daily.login.day";
        public const string LoginToday = "ui.daily.login.today";
        public const string LoginClaimed = "ui.daily.login.claimed";
        public const string LoginClaim = "ui.daily.login.claim";
        public const string LoginClose = "ui.daily.login.close";
        public const string LoginSteppedBack = "ui.daily.login.stepped_back";
        public const string LoginGotIt = "ui.daily.login.got_it";
        public const string LoginToChest = "ui.daily.login.to_chest";
        public const string LoginTitleBanner = "ui.daily.login.title_banner";
        public const string LoginTitleBannerDone = "ui.daily.login.title_banner_done";
        public const string LoginPreviewHint = "ui.daily.login.preview_hint";
        public const string LoginRareGear = "ui.daily.login.rare_gear";

        public const string SettingsPlayTimeReminder = "ui.settings.accessibility.playtime_reminder";
        public const string SettingsPlayTimeReminderOff = "ui.settings.accessibility.playtime_reminder_off";
        public const string SettingsPlayTimeReminderEvery = "ui.settings.accessibility.playtime_reminder_every";

        public static readonly List<LocalizationEntry> Entries = new List<LocalizationEntry>
        {
            new LocalizationEntry(StaminaLabel, "Energy", "Fôlego"),
            new LocalizationEntry(TierFull, "Full energy +30%", "Fôlego cheio +30%"),
            new LocalizationEntry(TierNormal, "Energy", "Fôlego normal"),
            new LocalizationEntry(TierTired, "Tired −50%", "Cansado −50%"),
            new LocalizationEntry(TooltipFull, "Full energy: +30% XP and loot for {0} more.", "Fôlego cheio: +30% de XP e loot por mais {0}."),
            new LocalizationEntry(TooltipNormal, "Normal energy: regular XP and loot for {0} more, then you get tired (−50%).", "Fôlego normal: XP e loot normais por mais {0}; depois vem o cansaço (−50%)."),
            new LocalizationEntry(TooltipTired, "Tired: −50% XP and loot. You can keep playing, but a break brings the energy back.", "Cansado: −50% de XP e loot. Dá para continuar jogando, mas uma pausa traz o fôlego de volta."),
            new LocalizationEntry(TooltipRest, "Resting refills it: 1 hour away gives back 1 hour of full energy.", "Descansar recupera: 1 hora longe do jogo devolve 1 hora de fôlego cheio."),
            new LocalizationEntry(ReminderTired, "Time for a break, Grandma Zefa is calling everyone for coffee!", "Hora de descansar, a Vó Zefa chamou para o café!"),
            new LocalizationEntry(ReminderPlayTime, "You have played for {0} minutes. How about stretching your legs and drinking some water?", "Você já jogou {0} minutos. Que tal esticar as pernas e beber uma água?"),
            new LocalizationEntry(DurationHours, "{0} h {1:00} min", "{0} h {1:00} min"),
            new LocalizationEntry(DurationMinutes, "{0} min", "{0} min"),

            new LocalizationEntry(LoginTitle, "Grandma Zefa's gift of the day", "Presente do dia da Vó Zefa"),
            new LocalizationEntry(LoginSubtitle, "Come back every day for a gift. Missed a day? No problem, the trail only steps back a little.", "Volte todo dia para ganhar um presente. Faltou um dia? Tudo bem, a trilha só volta um passinho."),
            new LocalizationEntry(LoginDay, "Day {0}", "Dia {0}"),
            new LocalizationEntry(LoginToday, "Today", "Hoje"),
            new LocalizationEntry(LoginClaimed, "Received", "Recebido"),
            new LocalizationEntry(LoginClaim, "Receive", "Receber"),
            new LocalizationEntry(LoginClose, "Close", "Fechar"),
            new LocalizationEntry(LoginSteppedBack, "You missed a day, so the trail stepped back one day. Welcome back!", "Você faltou um dia, então a trilha voltou um passinho. Que bom que voltou!"),
            new LocalizationEntry(LoginGotIt, "You received: {0}", "Você recebeu: {0}"),
            new LocalizationEntry(LoginToChest, "{0} went to Grandma Zefa's chest (backpack full).", "{0} foi para o Baú da Vó Zefa (Mochila cheia)."),
            new LocalizationEntry(LoginTitleBanner, "A gift of the day is waiting! Enter the map to receive it.", "Presente do dia esperando! Entre no mapa para receber."),
            new LocalizationEntry(LoginTitleBannerDone, "Today's gift is already yours. Come back tomorrow!", "O presente de hoje já é seu. Volte amanhã!"),
            new LocalizationEntry(LoginPreviewHint, "Load a game and go to the map to receive today's gift.", "Carregue um jogo e vá para o mapa para receber o presente de hoje."),
            new LocalizationEntry(LoginRareGear, "Surprise rare piece", "Peça rara surpresa"),

            new LocalizationEntry(SettingsPlayTimeReminder, "Play time reminder (for parents)", "Lembrete de tempo de jogo (para os pais)"),
            new LocalizationEntry(SettingsPlayTimeReminderOff, "Off", "Desligado"),
            new LocalizationEntry(SettingsPlayTimeReminderEvery, "Every {0} min", "A cada {0} min"),
        };

        private static Dictionary<string, string> _fallbackByKey;
        private static LocalizationManager _mergedInto;

        /// <summary>Merges <see cref="Entries"/> into <paramref name="manager"/>, skipping keys it already has. Safe to call repeatedly.</summary>
        public static void EnsureRegistered(LocalizationManager manager)
        {
            if (manager == null || (ReferenceEquals(manager, _mergedInto) && manager.HasKey(StaminaLabel)))
            {
                return;
            }

            manager.LoadEntries(Entries.Where(entry => !manager.HasKey(entry.Key)).ToList());
            _mergedInto = manager;
        }

        /// <summary>Localized text via <paramref name="manager"/> when available, else the pt-BR fallback, else the key.</summary>
        public static string Get(LocalizationManager manager, string key)
        {
            if (manager != null)
            {
                EnsureRegistered(manager);
                if (manager.HasKey(key))
                {
                    return manager.Get(key);
                }
            }

            return GetFallback(key) ?? key;
        }

        public static string Format(LocalizationManager manager, string key, params object[] args)
        {
            string pattern = Get(manager, key);
            try
            {
                return string.Format(CultureInfo.InvariantCulture, pattern, args);
            }
            catch (FormatException)
            {
                return string.Format(CultureInfo.InvariantCulture, GetFallback(key) ?? key, args);
            }
        }

        public static string GetFallback(string key)
        {
            if (_fallbackByKey == null)
            {
                _fallbackByKey = new Dictionary<string, string>();
                foreach (LocalizationEntry entry in Entries)
                {
                    _fallbackByKey[entry.Key] = entry.PtBR;
                }
            }

            return key != null && _fallbackByKey.TryGetValue(key, out string text) ? text : null;
        }

        /// <summary>"2 h 05 min" / "40 min" (rounded up to the minute, never "0 min" for time still left).</summary>
        public static string Duration(LocalizationManager manager, double seconds)
        {
            int minutes = (int)Math.Ceiling(Math.Max(0, seconds) / 60.0);
            return minutes >= 60
                ? Format(manager, DurationHours, minutes / 60, minutes % 60)
                : Format(manager, DurationMinutes, Math.Max(minutes, seconds > 0 ? 1 : 0));
        }

        public static string TierCaption(LocalizationManager manager, StaminaTier tier)
        {
            switch (tier)
            {
                case StaminaTier.Full:
                    return Get(manager, TierFull);
                case StaminaTier.Tired:
                    return Get(manager, TierTired);
                default:
                    return Get(manager, TierNormal);
            }
        }

        /// <summary>The meter's tooltip: the tier line plus the rest line.</summary>
        public static string Tooltip(LocalizationManager manager, StaminaTier tier, double secondsToNextTier)
        {
            string line;
            switch (tier)
            {
                case StaminaTier.Full:
                    line = Format(manager, TooltipFull, Duration(manager, secondsToNextTier));
                    break;
                case StaminaTier.Normal:
                    line = Format(manager, TooltipNormal, Duration(manager, secondsToNextTier));
                    break;
                default:
                    line = Get(manager, TooltipTired);
                    break;
            }

            return line + "\n" + Get(manager, TooltipRest);
        }

        public static string ReminderText(LocalizationManager manager, StaminaReminderKind kind, int sessionMinutes)
        {
            return kind == StaminaReminderKind.Tired ? Get(manager, ReminderTired) : Format(manager, ReminderPlayTime, sessionMinutes);
        }

        public static string ReminderChoiceLabel(LocalizationManager manager, int minutes)
        {
            return minutes <= 0 ? Get(manager, SettingsPlayTimeReminderOff) : Format(manager, SettingsPlayTimeReminderEvery, minutes);
        }
    }
}
