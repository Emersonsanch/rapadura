using Rapadura.Core.Events;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Hunting;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rapadura.UI.Hunting
{
    /// <summary>
    /// The "Amuletos" tab of "Caderno da Vó Zefa" (<c>BestiaryController</c>), added in code so the bestiary's UXML and
    /// controller only gain two calls: a "Páginas | Amuletos" tab row under the header, the "Bicho destaque de hoje"
    /// banner, and a charms view that replaces the page list + page while its tab is on. The view shows the "Pontos do
    /// Caderno" available, how many charms are on the cord (<see cref="HuntingCharmCatalog.MaxEquipped"/>) and one row
    /// per charm (name, effect, buy / wear / take off). Rules in <see cref="HuntingProgress"/> / <see cref="HuntingCharmCatalog"/>.
    /// </summary>
    public sealed class BestiaryCharmsTab
    {
        public const string TabsName = "bestiary-hunting-tabs";
        public const string BannerName = "bestiary-hunting-boosted";
        public const string ViewName = "bestiary-charms-view";
        public const string TabSelectedClass = "hunting-tab--selected";

        private readonly VisualElement _root;
        private readonly VisualElement _body;
        private readonly Button _pagesTab;
        private readonly Button _charmsTab;
        private readonly Label _banner;
        private readonly ScrollView _view;
        private LocalizationManager _localization;
        private bool _showCharms;

        public bool ShowingCharms => _showCharms;

        private BestiaryCharmsTab(VisualElement root)
        {
            _root = root;
            _body = root.Q<VisualElement>(className: "bestiary-body");
            VisualElement card = root.Q<VisualElement>("bestiary-card") ?? root;
            StyleSheet sheet = Resources.Load<StyleSheet>(HuntingBoardPanel.StyleResourcePath);
            if (sheet != null)
            {
                card.styleSheets.Add(sheet);
            }

            var tabs = new VisualElement { name = TabsName };
            tabs.AddToClassList("hunting-tabs");
            _pagesTab = new Button(() => Show(false)) { name = "bestiary-tab-pages", focusable = true };
            _charmsTab = new Button(() => Show(true)) { name = "bestiary-tab-charms", focusable = true };
            foreach (Button tab in new[] { _pagesTab, _charmsTab })
            {
                tab.AddToClassList("rap-button");
                tab.AddToClassList("hunting-tab");
                tabs.Add(tab);
            }

            _banner = new Label { name = BannerName, pickingMode = PickingMode.Ignore };
            _banner.AddToClassList("hunting-boosted-banner");

            _view = new ScrollView { name = ViewName };
            _view.AddToClassList("hunting-charms-view");
            _view.style.display = DisplayStyle.None;

            int index = _body != null && _body.parent == card ? card.IndexOf(_body) : card.childCount;
            card.Insert(index, tabs);
            card.Insert(index + 1, _banner);
            card.Insert(index + 3 <= card.childCount ? index + 3 : card.childCount, _view);

            EventBus.Subscribe<HuntingStateChangedEvent>(OnChanged);
        }

        /// <summary>Adds the tab to the bestiary root once (returns the existing one when called again with the same root).</summary>
        public static BestiaryCharmsTab Attach(VisualElement root, BestiaryCharmsTab existing = null)
        {
            if (root == null)
            {
                return existing;
            }

            if (existing != null && existing._root == root && existing._view.parent != null)
            {
                return existing;
            }

            existing?.Detach();
            return new BestiaryCharmsTab(root);
        }

        public void Detach()
        {
            EventBus.Unsubscribe<HuntingStateChangedEvent>(OnChanged);
            _root.Q<VisualElement>(TabsName)?.RemoveFromHierarchy();
            _banner.RemoveFromHierarchy();
            _view.RemoveFromHierarchy();
        }

        private void OnChanged(HuntingStateChangedEvent evt)
        {
            if (_view.parent != null && _root.resolvedStyle.display != DisplayStyle.None)
            {
                Render(_localization);
            }
        }

        public void Show(bool charms)
        {
            _showCharms = charms;
            Render(_localization);
        }

        public void Render(LocalizationManager localization)
        {
            _localization = localization;
            HuntingProgress progress = HuntingProgress.ResolveShared();

            _pagesTab.text = HuntingTexts.Get(localization, HuntingTexts.TabPages);
            _charmsTab.text = HuntingTexts.Get(localization, HuntingTexts.TabCharms);
            _pagesTab.EnableInClassList(TabSelectedClass, !_showCharms);
            _charmsTab.EnableInClassList(TabSelectedClass, _showCharms);
            _charmsTab.style.display = progress != null ? DisplayStyle.Flex : DisplayStyle.None;

            string boosted = progress != null ? HuntingPresenter.BoostedCadernoText(localization, HuntingPresenter.CreatureName(localization, progress.BoostedId)) : null;
            _banner.text = boosted ?? string.Empty;
            _banner.style.display = boosted != null && !string.IsNullOrEmpty(progress.BoostedId) && !_showCharms ? DisplayStyle.Flex : DisplayStyle.None;

            bool charms = _showCharms && progress != null;
            if (_body != null)
            {
                _body.style.display = charms ? DisplayStyle.None : DisplayStyle.Flex;
            }

            _view.style.display = charms ? DisplayStyle.Flex : DisplayStyle.None;
            if (charms)
            {
                RenderCharms(progress, localization);
            }
        }

        private void RenderCharms(HuntingProgress progress, LocalizationManager localization)
        {
            _view.Clear();
            AddLabel("hunting-intro", HuntingTexts.Get(localization, HuntingTexts.CharmIntro));
            AddLabel("hunting-line", HuntingTexts.Format(localization, HuntingTexts.CharmPoints, progress.CadernoPointsAvailable));
            AddLabel("hunting-subtle", HuntingTexts.Format(localization, HuntingTexts.CharmEquipped, progress.EquippedCharms.Count, HuntingCharmCatalog.MaxEquipped));

            int available = progress.CadernoPointsAvailable;
            bool cordFull = progress.EquippedCharms.Count >= HuntingCharmCatalog.MaxEquipped;
            foreach (HuntingCharmSpec charm in HuntingCharmCatalog.All)
            {
                bool unlocked = progress.IsCharmUnlocked(charm.Id);
                bool equipped = progress.IsCharmEquipped(charm.Id);
                bool canBuy = !unlocked && available >= charm.Cost;

                var row = new VisualElement { name = "bestiary-charm-" + charm.Id };
                row.AddToClassList("hunting-row");
                row.AddToClassList("hunting-charm-row");
                row.EnableInClassList("hunting-row--picked", equipped);
                var text = new VisualElement { pickingMode = PickingMode.Ignore };
                text.AddToClassList("hunting-charm-text");
                var name = new Label(HuntingTexts.Get(localization, charm.NameKey)) { pickingMode = PickingMode.Ignore };
                name.AddToClassList("hunting-creature");
                var effect = new Label(HuntingTexts.Get(localization, charm.EffectKey)) { pickingMode = PickingMode.Ignore };
                effect.AddToClassList("hunting-subtle");
                text.Add(name);
                text.Add(effect);
                row.Add(text);

                string id = charm.Id;
                var button = new Button(() =>
                {
                    if (!progress.IsCharmUnlocked(id)) progress.UnlockCharm(id);
                    else if (progress.IsCharmEquipped(id)) progress.UnequipCharm(id);
                    else progress.EquipCharm(id);
                })
                {
                    text = HuntingPresenter.CharmActionText(localization, charm, unlocked, equipped, canBuy, cordFull),
                    focusable = true,
                };
                button.AddToClassList("rap-button");
                button.AddToClassList("hunting-button");
                button.SetEnabled(unlocked ? equipped || !cordFull : canBuy);
                row.Add(button);
                _view.Add(row);
            }
        }

        private void AddLabel(string className, string text)
        {
            var label = new Label(text) { pickingMode = PickingMode.Ignore };
            label.AddToClassList(className);
            _view.Add(label);
        }
    }
}
