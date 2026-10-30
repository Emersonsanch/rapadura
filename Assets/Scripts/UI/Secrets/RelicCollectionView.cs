using System;
using System.Collections.Generic;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Secrets;
using Rapadura.Gameplay.World.Biomes;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rapadura.UI.Secrets
{
    /// <summary>Look of the code-built secrets UI: <c>Resources/UI/SecretsOverlay.uss</c> (imports RapaduraTheme.uss).</summary>
    public static class SecretsStyles
    {
        public const string ResourcePath = "UI/SecretsOverlay";
        public const string ThemeClass = "rap-theme";

        private static StyleSheet _sheet;

        public static StyleSheet Sheet
        {
            get
            {
                if (_sheet == null)
                {
                    _sheet = Resources.Load<StyleSheet>(ResourcePath);
                }

                return _sheet;
            }
        }

        /// <summary>Attaches the sheet (once) and, when asked, the theme class to <paramref name="root"/>.</summary>
        public static void ApplyTo(VisualElement root, bool addThemeClass)
        {
            if (root == null)
            {
                return;
            }

            StyleSheet sheet = Sheet;
            if (sheet != null && !root.styleSheets.Contains(sheet))
            {
                root.styleSheets.Add(sheet);
            }

            if (addThemeClass)
            {
                root.AddToClassList(ThemeClass);
            }
        }
    }

    /// <summary>One line of the relic panel, already localized.</summary>
    public sealed class RelicRowViewModel
    {
        public string Id;
        public bool Found;
        public BiomeType Region;
        public string Name;
        /// <summary>"Rende o dobro com Maithe" / "Da família toda" (found relics only).</summary>
        public string Tag;
        /// <summary>The passive (found relics only).</summary>
        public string Passive;
        /// <summary>Lore when found, the where-to-look clue when missing.</summary>
        public string Body;
    }

    /// <summary>
    /// Pure presenter of the "Lembranças do Vô Jorge" panel: every relic in catalog order, found ones with name, owner tag,
    /// passive and lore, missing ones as "???" with their clue. <paramref name="localize"/> maps a key to text (tests pass
    /// the key back), <paramref name="heroName"/> a hero to its display name.
    /// </summary>
    public static class RelicCollectionPresenter
    {
        public static List<RelicRowViewModel> Build(SecretsProgress secrets, Func<string, string> localize, Func<CharacterId, string> heroName)
        {
            var rows = new List<RelicRowViewModel>(RelicCatalog.Count);
            foreach (RelicDefinition relic in RelicCatalog.All)
            {
                bool found = secrets != null && secrets.HasRelic(relic.Id);
                var row = new RelicRowViewModel { Id = relic.Id, Found = found, Region = relic.Region };
                if (found)
                {
                    row.Name = localize(relic.NameKey);
                    row.Tag = relic.Hero.HasValue
                        ? SafeFormat(localize(SecretTexts.OwnerBonus), heroName != null ? heroName(relic.Hero.Value) : relic.Hero.Value.ToString())
                        : localize(SecretTexts.FamilyTag);
                    row.Passive = localize(relic.PassiveKey);
                    row.Body = localize(relic.LoreKey);
                }
                else
                {
                    row.Name = localize(SecretTexts.UnknownName);
                    row.Tag = string.Empty;
                    row.Passive = string.Empty;
                    row.Body = localize(relic.ClueKey);
                }

                rows.Add(row);
            }

            return rows;
        }

        /// <summary>"3/12 encontradas".</summary>
        public static string FormatCount(SecretsProgress secrets, Func<string, string> localize)
        {
            return SafeFormat(localize(SecretTexts.PanelCount), secrets != null ? secrets.OwnedRelicCount : 0, RelicCatalog.Count);
        }

        private static string SafeFormat(string pattern, params object[] args)
        {
            try
            {
                return string.Format(pattern, args);
            }
            catch (FormatException)
            {
                return pattern;
            }
        }
    }

    /// <summary>
    /// The panel's visual tree (overlay → panel → title, subtitle, count, note, scroll list, close), built in code with
    /// the classes of SecretsOverlay.uss. No state; <see cref="SecretsMapButtons"/> owns it. EditMode-testable.
    /// </summary>
    public sealed class RelicCollectionView
    {
        public const string OverlayName = "relic-overlay";
        public const string PanelName = "relic-panel";
        public const string ListName = "relic-list";
        public const string CloseName = "relic-close";

        public readonly VisualElement Overlay;
        public readonly Label Title;
        public readonly Label Subtitle;
        public readonly Label Count;
        public readonly Label Note;
        public readonly ScrollView List;
        public readonly Button Close;

        public RelicCollectionView()
        {
            Overlay = new VisualElement { name = OverlayName };
            Overlay.AddToClassList(OverlayName);
            SecretsStyles.ApplyTo(Overlay, addThemeClass: true);
            Overlay.style.display = DisplayStyle.None;

            var panel = new VisualElement { name = PanelName };
            panel.AddToClassList(PanelName);
            panel.AddToClassList("rap-panel");
            Overlay.Add(panel);

            Title = AddLabel(panel, "relic-title");
            Subtitle = AddLabel(panel, "relic-subtitle");
            Count = AddLabel(panel, "relic-count");
            Note = AddLabel(panel, "relic-note");

            List = new ScrollView(ScrollViewMode.Vertical) { name = ListName };
            List.AddToClassList(ListName);
            panel.Add(List);

            Close = new Button { name = CloseName, focusable = true };
            Close.AddToClassList(CloseName);
            Close.AddToClassList("rap-button");
            Close.AddToClassList("rap-button--primary");
            panel.Add(Close);
        }

        public bool IsShown => Overlay.style.display == DisplayStyle.Flex;

        /// <summary>Rebuilds the list from <paramref name="rows"/>.</summary>
        public void Populate(IReadOnlyList<RelicRowViewModel> rows)
        {
            List.Clear();
            foreach (RelicRowViewModel row in rows)
            {
                var line = new VisualElement { name = "relic-row-" + row.Id };
                line.AddToClassList("relic-row");
                if (!row.Found)
                {
                    line.AddToClassList("relic-row--missing");
                }

                var gem = new VisualElement();
                gem.AddToClassList("relic-gem");
                if (row.Found)
                {
                    gem.AddToClassList("relic-gem--" + row.Region.ToString().ToLowerInvariant());
                }

                line.Add(gem);

                var text = new VisualElement();
                text.AddToClassList("relic-text");
                AddLabel(text, "relic-name").text = row.Name;
                if (!string.IsNullOrEmpty(row.Tag))
                {
                    AddLabel(text, "relic-tag").text = row.Tag;
                }

                if (!string.IsNullOrEmpty(row.Passive))
                {
                    AddLabel(text, "relic-passive").text = row.Passive;
                }

                AddLabel(text, "relic-body").text = row.Body;
                line.Add(text);
                List.Add(line);
            }
        }

        private static Label AddLabel(VisualElement parent, string className)
        {
            var label = new Label();
            label.AddToClassList(className);
            parent.Add(label);
            return label;
        }
    }
}
