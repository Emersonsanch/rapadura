using System.Collections.Generic;
using System.Globalization;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Content;
using Rapadura.Gameplay.Enemies;
using Rapadura.UI.WorldMap;
using UnityEngine;

namespace Rapadura.UI.Bestiary
{
    /// <summary>Everything one page of the notebook shows, already localized (built by <see cref="BestiaryPresenter.BuildPage"/>).</summary>
    public sealed class BestiaryPageView
    {
        public bool Locked;
        public bool IsBoss;
        public string Name;
        /// <summary>"Vive em: Bosque do Zarafshan" (+ " · Chefe").</summary>
        public string Meta;
        /// <summary>"Vencidos: 3" (empty when locked).</summary>
        public string Kills;
        /// <summary>"Vida 70 · Golpe 7" (empty when locked).</summary>
        public string Stats;
        /// <summary>Lore paragraph, or the "not faced yet" hint when locked.</summary>
        public string Lore;
        public readonly List<string> Tips = new List<string>();
        public readonly List<string> Drops = new List<string>();
        /// <summary>"Maithe diz" (empty when locked or no hero).</summary>
        public string CommentHeader;
        public string Comment;
        /// <summary>Selected cousin, for the comment's accent colour.</summary>
        public CharacterId? Hero;
    }

    /// <summary>
    /// Pure text building for "Caderno da Vó Edna" (<see cref="BestiaryController"/>): list labels and page
    /// contents from a <see cref="BestiaryEntry"/>, the unlock state and the selected cousin. Locked pages show
    /// "???" and only the region (regions are no secret on the world map); nothing about the enemy itself.
    /// A null <see cref="LocalizationManager"/> means pt-BR fallback texts (EditMode tests).
    /// </summary>
    public static class BestiaryPresenter
    {
        public static string ListLabel(LocalizationManager manager, BestiaryEntry entry, bool unlocked)
        {
            if (entry == null)
            {
                return string.Empty;
            }

            return unlocked ? EnemyName(manager, entry) : BestiaryTexts.Get(manager, BestiaryTexts.LockedName);
        }

        public static string EnemyName(LocalizationManager manager, BestiaryEntry entry) =>
            entry?.Definition != null ? entry.Definition.LocalizedName(manager) : entry?.Id ?? string.Empty;

        public static string RegionName(LocalizationManager manager, BestiaryEntry entry) =>
            entry != null && entry.Region.HasValue
                ? WorldMapHudPresenter.RegionName(manager, entry.Region.Value)
                : BestiaryTexts.Get(manager, BestiaryTexts.UnknownRegion);

        /// <summary>"7/9 páginas escritas", or the completion line once every page is unlocked.</summary>
        public static string ProgressText(LocalizationManager manager, int unlocked, int total)
        {
            return total > 0 && unlocked >= total
                ? BestiaryTexts.Get(manager, BestiaryTexts.Complete)
                : BestiaryTexts.Format(manager, BestiaryTexts.Progress, unlocked, total);
        }

        public static BestiaryPageView BuildPage(LocalizationManager manager, BestiaryEntry entry, bool unlocked, int kills, CharacterId? hero)
        {
            var page = new BestiaryPageView { Locked = !unlocked, Hero = hero };
            if (entry == null)
            {
                return page;
            }

            page.IsBoss = entry.IsBoss;
            string meta = BestiaryTexts.Format(manager, BestiaryTexts.RegionLine, RegionName(manager, entry));
            if (entry.IsBoss)
            {
                // The boss badge is shown on locked pages too: the map already says every region has one.
                meta += " · " + BestiaryTexts.Get(manager, BestiaryTexts.BossBadge);
            }

            page.Meta = meta;

            if (!unlocked)
            {
                page.Name = BestiaryTexts.Get(manager, BestiaryTexts.LockedName);
                page.Lore = BestiaryTexts.Get(manager, entry.IsBoss ? BestiaryTexts.LockedBossHint : BestiaryTexts.LockedHint);
                page.Kills = page.Stats = page.CommentHeader = page.Comment = string.Empty;
                return page;
            }

            page.Name = EnemyName(manager, entry);
            page.Kills = BestiaryTexts.Format(manager, BestiaryTexts.Kills, Mathf.Max(1, kills));
            page.Stats = BestiaryTexts.Format(manager, BestiaryTexts.Stats, Mathf.RoundToInt(entry.MaxHealth), Mathf.RoundToInt(entry.AttackDamage));
            page.Lore = BestiaryTexts.Get(manager, entry.LoreKey);

            foreach (BestiaryTip tip in entry.Tips)
            {
                string text = TipText(manager, tip);
                if (!string.IsNullOrEmpty(text))
                {
                    page.Tips.Add(text);
                }
            }

            foreach (BestiaryDrop drop in entry.Drops)
            {
                string text = DropText(manager, drop);
                if (!string.IsNullOrEmpty(text))
                {
                    page.Drops.Add(text);
                }
            }

            if (page.Drops.Count == 0)
            {
                page.Drops.Add(BestiaryTexts.Get(manager, BestiaryTexts.DropsNone));
            }

            if (hero.HasValue)
            {
                page.CommentHeader = BestiaryTexts.Format(manager, BestiaryTexts.CommentHeader, WorldMapHudPresenter.HeroName(manager, hero));
                page.Comment = BestiaryTexts.Get(manager, entry.CommentKey(hero.Value));
            }
            else
            {
                page.CommentHeader = string.Empty;
                page.Comment = BestiaryTexts.Get(manager, BestiaryTexts.NoHeroComment);
            }

            return page;
        }

        public static string TipText(LocalizationManager manager, BestiaryTip tip)
        {
            string key = BestiaryTexts.TipKey(tip.Kind);
            if (key == null)
            {
                return null;
            }

            switch (tip.Kind)
            {
                case BestiaryTipKind.Telegraph:
                    return BestiaryTexts.Format(manager, key, Decimal(manager, tip.Value));
                case BestiaryTipKind.HeavyHitter:
                    return BestiaryTexts.Format(manager, key, Mathf.RoundToInt(tip.Value));
                case BestiaryTipKind.Flees:
                case BestiaryTipKind.StunResistant:
                    return BestiaryTexts.Format(manager, key, Mathf.RoundToInt(tip.Value * 100f));
                default:
                    return BestiaryTexts.Get(manager, key);
            }
        }

        public static string DropText(LocalizationManager manager, BestiaryDrop drop)
        {
            if (drop?.Item == null)
            {
                return null;
            }

            string name = drop.Item.LocalizedName(manager);
            if (drop.IsBossBonus)
            {
                return BestiaryTexts.Format(manager, BestiaryTexts.DropBossBonusLine, name);
            }

            string frequency = BestiaryTexts.Get(manager, BestiaryTexts.FrequencyKey(drop.Frequency));
            return BestiaryTexts.Format(manager, drop.IsEquipment ? BestiaryTexts.DropEquipmentLine : BestiaryTexts.DropLine, name, frequency);
        }

        /// <summary>One decimal, with the language's separator ("0,4" in pt-BR, "0.4" in en; no manager = pt-BR).</summary>
        public static string Decimal(LocalizationManager manager, float value)
        {
            string text = value.ToString("0.0", CultureInfo.InvariantCulture);
            bool english = manager != null && manager.CurrentLanguage == LanguageCode.en;
            return english ? text : text.Replace('.', ',');
        }
    }
}
