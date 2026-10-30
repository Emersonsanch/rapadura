using System;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Housing;

using UnityEngine.UIElements;

namespace Rapadura.UI.Housing
{
    /// <summary>
    /// H3 "Bancada da casa": the furniture recipes of <see cref="HouseCraftRules"/> for the station that was tapped
    /// (Bancada, Forja or Fogão placed in the house), one block per recipe with its cost lines (✓/✗ against bag + Baú da
    /// casa) and "Fazer". Built in code on the house overlay and styled with the existing <c>house-life*</c> classes of
    /// <c>Resources/UI/House.uss</c> (no new sheet). All rules in <see cref="HouseService.TryCraft"/>.
    /// </summary>
    public sealed class HouseWorkbenchPanel
    {
        public const string PanelName = "wm-house-workbench";

        private readonly VisualElement _root;
        private readonly Label _title;
        private readonly Button _close;
        private readonly ScrollView _scroll;
        private readonly Func<HouseService> _house;
        private readonly Func<IHouseWallet> _wallet;
        private readonly Func<LocalizationManager> _localization;
        private readonly Action<string, bool> _status;
        private HouseItemFunction _station = HouseItemFunction.Bench;

        public HouseWorkbenchPanel(VisualElement parent, Func<HouseService> house, Func<IHouseWallet> wallet, Func<LocalizationManager> localization, Action<string, bool> status)
        {
            _house = house;
            _wallet = wallet;
            _localization = localization;
            _status = status;

            _root = new VisualElement { name = PanelName };
            _root.AddToClassList("house-life");
            _root.style.display = DisplayStyle.None;
            var header = new VisualElement();
            header.AddToClassList("house-life-header");
            _title = new Label();
            _title.AddToClassList("house-life-title");
            header.Add(_title);
            _close = new Button(Hide) { name = "wm-house-workbench-close", focusable = true };
            _close.AddToClassList("rap-button");
            _close.AddToClassList("house-life-close");
            header.Add(_close);
            _root.Add(header);
            _scroll = new ScrollView(ScrollViewMode.Vertical) { name = "wm-house-workbench-scroll" };
            _scroll.AddToClassList("house-life-scroll");
            _root.Add(_scroll);
            parent.Add(_root);
        }

        public bool IsVisible { get; private set; }
        public VisualElement Root => _root;
        private LocalizationManager L => _localization?.Invoke();

        public void Show(HouseItemFunction station)
        {
            _station = station;
            IsVisible = true;
            _root.style.display = DisplayStyle.Flex;
            Refresh();
            _close.Focus();
        }

        public void Hide()
        {
            IsVisible = false;
            _root.style.display = DisplayStyle.None;
        }

        public void Refresh()
        {
            HouseService house = _house?.Invoke();
            if (house == null || !IsVisible) return;
            LocalizationManager l = L;
            _title.text = HouseSourceTexts.Get(l, HouseSourceTexts.CraftTitle);
            _close.text = HouseSourceTexts.Get(l, HouseSourceTexts.Close);
            _scroll.Clear();
            Line(HouseSourceTexts.Get(l, HouseSourceTexts.CraftIntro), "house-life-line--dim");
            IHouseWallet wallet = _wallet?.Invoke();

            foreach (HouseCraftRecipe recipe in HouseCraftRules.All)
            {
                if (recipe.Station != _station) continue;
                HouseCraftBlock block = house.CheckCraft(recipe, wallet);
                string name = ResultName(recipe, l);
                Label title = Line(HouseSourceTexts.Format(l, HouseSourceTexts.CraftRow, name) + " · " + HouseSourceTexts.Get(l, HouseSourceTexts.StationKey(recipe)), "house-section");
                title.AddToClassList(block == HouseCraftBlock.None ? "house-life-row--met" : "house-life-row--missing");
                foreach (HouseMaterialCost cost in recipe.Costs)
                {
                    int have = wallet != null ? wallet.Count(cost.ItemId) : 0;
                    Label row = Line("  " + HouseSourceTexts.Format(l, HouseSourceTexts.CraftCost, HouseSourceTexts.MaterialName(l, cost.ItemId), have, cost.Quantity));
                    row.AddToClassList(have >= cost.Quantity ? "house-life-row--met" : "house-life-row--missing");
                }

                string why = BlockKey(block);
                if (why != null) Line(HouseSourceTexts.Get(l, why), "house-life-line--dim");
                string recipeId = recipe.Id;
                var make = new Button(() => Craft(recipeId)) { name = "wm-house-craft-" + recipe.ResultId, text = HouseSourceTexts.Get(l, HouseSourceTexts.CraftDo), focusable = true };
                make.AddToClassList("rap-button");
                make.AddToClassList("house-life-button");
                make.SetEnabled(block == HouseCraftBlock.None);
                _scroll.Add(make);
            }
        }

        public static string ResultName(HouseCraftRecipe recipe, LocalizationManager l)
        {
            if (recipe == null) return string.Empty;
            if (recipe.Kind == HouseCraftResultKind.HouseItem) return HouseItemTexts.Name(l, recipe.ResultId);
            return recipe.ResultId + " " + HouseSourceTexts.Get(l, HouseSourceTexts.CraftWardrobeNote);
        }

        public static string BlockKey(HouseCraftBlock block)
        {
            switch (block)
            {
                case HouseCraftBlock.NoHouse: return HouseSourceTexts.CraftNoHouse;
                case HouseCraftBlock.NoStation: return HouseSourceTexts.CraftNoStation;
                case HouseCraftBlock.BossNeeded: return HouseSourceTexts.CraftBossNeeded;
                case HouseCraftBlock.MissingMaterials: return HouseSourceTexts.CraftMissing;
                case HouseCraftBlock.None: return null;
                default: return HouseSourceTexts.CraftFailed;
            }
        }

        private void Craft(string recipeId)
        {
            HouseService house = _house?.Invoke();
            if (house == null) return;
            LocalizationManager l = L;
            HouseCraftRecipe recipe = HouseCraftRules.Get(recipeId);
            HouseCraftBlock block = house.TryCraft(recipeId, _wallet?.Invoke());
            _status?.Invoke(block == HouseCraftBlock.None
                ? HouseSourceTexts.Format(l, HouseSourceTexts.CraftDone, ResultName(recipe, l))
                : HouseSourceTexts.Get(l, BlockKey(block) ?? HouseSourceTexts.CraftFailed), block != HouseCraftBlock.None);
            Refresh();
        }

        private Label Line(string text, string extraClass = null)
        {
            var label = new Label(text ?? string.Empty);
            label.AddToClassList("house-life-line");
            if (!string.IsNullOrEmpty(extraClass)) label.AddToClassList(extraClass);
            _scroll.Add(label);
            return label;
        }
    }
}
