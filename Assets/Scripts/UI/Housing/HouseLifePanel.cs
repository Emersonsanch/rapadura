using System;
using System.Collections.Generic;
using System.Linq;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Housing;
using UnityEngine.UIElements;

namespace Rapadura.UI.Housing
{
    /// <summary>
    /// The side panel of "Minha casa" for H4 and H6, built in code on the house overlay (<c>Resources/UI/House.uss</c>,
    /// classes <c>house-life*</c>):
    /// <list type="bullet">
    /// <item><b>Melhorar a casa</b>: current → next level, one ✓/✗ line per requirement (hero level, boss, gold,
    /// materials, keepsakes), the provisional-price note, "Melhorar" (confirm card → result card with the family's
    /// moving-day line and what went to the Depósito) and the "Missões da Casa" 2–6 with "Entregar" for deliveries.</item>
    /// <item><b>Vida na casa</b>: comfort and tier, "Descansado" and tonight's charges, the sets started (stage and
    /// title), plants with "Regar as plantas", and the guest book (newest first, cousins signed for "Casa Cheia").</item>
    /// </list>
    /// All rules come from <see cref="HouseService"/>; this class only shows them and forwards the buttons.
    /// </summary>
    public sealed class HouseLifePanel
    {
        public enum Mode
        {
            Hidden,
            Upgrade,
            Life,
        }

        public const string PanelName = "wm-house-life";
        private const int GuestBookShown = 12;

        private readonly VisualElement _root;
        private readonly Label _title;
        private readonly Button _close;
        private readonly ScrollView _scroll;
        private readonly Func<HouseService> _house;
        private readonly Func<IHouseWallet> _wallet;
        private readonly Func<int> _heroLevel;
        private readonly Func<LocalizationManager> _localization;
        private readonly Action<string, string, string, string, string, Action> _card;
        private readonly Action<string, bool> _status;
        private readonly Action _closed;

        public HouseLifePanel(VisualElement parent, Func<HouseService> house, Func<IHouseWallet> wallet, Func<int> heroLevel,
            Func<LocalizationManager> localization, Action<string, string, string, string, string, Action> card, Action<string, bool> status, Action closed)
        {
            _house = house;
            _wallet = wallet;
            _heroLevel = heroLevel;
            _localization = localization;
            _card = card;
            _status = status;
            _closed = closed;

            _root = new VisualElement { name = PanelName };
            _root.AddToClassList("house-life");
            _root.style.display = DisplayStyle.None;
            var header = new VisualElement();
            header.AddToClassList("house-life-header");
            _title = new Label();
            _title.AddToClassList("house-life-title");
            header.Add(_title);
            _close = new Button(Hide) { name = "wm-house-life-close", focusable = true };
            _close.AddToClassList("rap-button");
            _close.AddToClassList("house-life-close");
            header.Add(_close);
            _root.Add(header);
            _scroll = new ScrollView(ScrollViewMode.Vertical) { name = "wm-house-life-scroll" };
            _scroll.AddToClassList("house-life-scroll");
            _root.Add(_scroll);
            parent.Add(_root);
        }

        public Mode Current { get; private set; } = Mode.Hidden;
        public bool IsVisible => Current != Mode.Hidden;
        public VisualElement Root => _root;

        private LocalizationManager L => _localization?.Invoke();

        public void Show(Mode mode)
        {
            Current = mode;
            _root.style.display = mode == Mode.Hidden ? DisplayStyle.None : DisplayStyle.Flex;
            if (mode != Mode.Hidden)
            {
                Refresh();
                _close.Focus();
            }
            else
            {
                _closed?.Invoke();
            }
        }

        public void Toggle(Mode mode) => Show(Current == mode ? Mode.Hidden : mode);

        public void Hide()
        {
            if (Current != Mode.Hidden) Show(Mode.Hidden);
        }

        /// <summary>Rebuilds the visible view from the house's state.</summary>
        public void Refresh()
        {
            HouseService house = _house?.Invoke();
            if (house == null || Current == Mode.Hidden) return;
            LocalizationManager l = L;
            _close.text = HouseLifeTexts.Get(l, HouseLifeTexts.Close);
            _scroll.Clear();
            if (Current == Mode.Upgrade)
            {
                _title.text = HouseLifeTexts.Get(l, HouseLifeTexts.UpgradeTitle);
                BuildUpgrade(house, l);
                BuildQuests(house, l);
            }
            else
            {
                _title.text = HouseLifeTexts.Get(l, HouseLifeTexts.LifeTitle);
                BuildLife(house, l);
            }
        }

        // ------------------------------------------------------------------
        // Upgrade (H4)
        // ------------------------------------------------------------------

        private void BuildUpgrade(HouseService house, LocalizationManager l)
        {
            IHouseWallet wallet = _wallet?.Invoke();
            HouseUpgradeCheck check = house.CheckUpgrade(_heroLevel?.Invoke() ?? 1, wallet);
            if (check.Block == HouseUpgradeBlock.NoHouse)
            {
                Line(HouseLifeTexts.Get(l, HouseLifeTexts.UpgradeNoHouse));
                return;
            }

            if (check.Block == HouseUpgradeBlock.MaxLevel)
            {
                Line(HouseLifeTexts.Get(l, HouseLifeTexts.UpgradeMax));
                return;
            }

            Line(HouseLifeTexts.Format(l, HouseLifeTexts.UpgradeNext, HouseTexts.Get(l, HouseTexts.LevelKey(check.FromLevel)), HouseTexts.Get(l, HouseTexts.LevelKey(check.ToLevel))), "house-section");
            foreach (HouseRequirementRow row in check.Rows)
            {
                Label label = Line(HouseLifeTexts.Get(l, row.Met ? HouseLifeTexts.RowMet : HouseLifeTexts.RowMissing) + " " + RowText(row, l));
                label.AddToClassList(row.Met ? "house-life-row--met" : "house-life-row--missing");
            }

            Line(HouseLifeTexts.Get(l, check.CanUpgrade ? HouseLifeTexts.UpgradeOk : HouseLifeTexts.UpgradeBlocked), check.CanUpgrade ? null : "house-life-line--dim");
            Button upgrade = AddButton("wm-house-upgrade-do", HouseLifeTexts.Get(l, HouseLifeTexts.UpgradeDo), () => AskUpgrade(check));
            upgrade.SetEnabled(check.CanUpgrade);
            if (check.CanUpgrade) upgrade.AddToClassList("rap-button--primary");
            Line(HouseLifeTexts.Get(l, HouseLifeTexts.UpgradeProvisional), "house-life-line--dim");
        }

        public static string RowText(HouseRequirementRow row, LocalizationManager l)
        {
            switch (row.Kind)
            {
                case HouseRequirementKind.CharacterLevel:
                    return HouseLifeTexts.Format(l, HouseLifeTexts.ReqLevel, row.Need, row.Have);
                case HouseRequirementKind.Boss:
                    return HouseLifeTexts.Format(l, HouseLifeTexts.ReqBoss, HouseLifeTexts.Get(l, HouseLifeTexts.BossKey(row.Id)));
                case HouseRequirementKind.Keepsake:
                    return HouseLifeTexts.Format(l, HouseLifeTexts.ReqKeepsake, HouseItemTexts.Name(l, row.Id));
                default:
                    return HouseLifeTexts.Format(l, HouseLifeTexts.ReqItem, HouseLifeTexts.Get(l, HouseLifeTexts.MaterialKey(row.Id)), row.Have, row.Need);
            }
        }

        public static string CostText(IEnumerable<HouseMaterialCost> costs, LocalizationManager l)
        {
            return string.Join(", ", costs.Select(c => c.Quantity + " " + HouseLifeTexts.Get(l, HouseLifeTexts.MaterialKey(c.ItemId)).ToLowerInvariant()));
        }

        private void AskUpgrade(HouseUpgradeCheck check)
        {
            LocalizationManager l = L;
            if (check == null || !check.CanUpgrade)
            {
                _status?.Invoke(HouseLifeTexts.Get(l, HouseLifeTexts.UpgradeBlocked), true);
                return;
            }

            string levelName = HouseTexts.Get(l, HouseTexts.LevelKey(check.ToLevel));
            _card?.Invoke(HouseLifeTexts.Get(l, HouseLifeTexts.UpgradeConfirmTitle),
                HouseLifeTexts.Format(l, HouseLifeTexts.UpgradeConfirmText, levelName, CostText(HouseUpgradeRules.SpentCosts(check.Target), l)),
                HouseLifeTexts.Get(l, HouseLifeTexts.UpgradeProvisional),
                HouseLifeTexts.Get(l, HouseLifeTexts.UpgradeConfirmYes), HouseLifeTexts.Get(l, HouseLifeTexts.UpgradeConfirmNo), DoUpgrade);
        }

        private void DoUpgrade()
        {
            HouseService house = _house?.Invoke();
            LocalizationManager l = L;
            if (house == null) return;
            HouseUpgradeResult result = house.TryUpgrade(_heroLevel?.Invoke() ?? 1, _wallet?.Invoke());
            if (!result.Success)
            {
                bool payment = result.Check != null && result.Check.Block == HouseUpgradeBlock.PaymentFailed;
                _status?.Invoke(HouseLifeTexts.Get(l, payment ? HouseLifeTexts.UpgradePaymentFailed : HouseLifeTexts.UpgradeBlocked), true);
                Refresh();
                return;
            }

            string levelName = HouseTexts.Get(l, HouseTexts.LevelKey(result.ToLevel));
            string text = HouseLifeTexts.Format(l, HouseLifeTexts.UpgradeDoneText, levelName) + "\n" + HouseLifeTexts.Get(l, HouseLifeTexts.SceneKey(result.ToLevel));
            string moved = result.MovedToDepot > 0
                ? HouseLifeTexts.Format(l, HouseLifeTexts.UpgradeMoved, result.MovedToDepot)
                : HouseLifeTexts.Get(l, HouseLifeTexts.UpgradeNothingMoved);
            foreach (string questId in result.CompletedQuests)
            {
                HouseQuestDefinition quest = HouseQuestChain.Get(questId);
                if (quest != null) moved += "\n" + HouseLifeTexts.Format(l, HouseLifeTexts.QuestDoneStatus, HouseLifeTexts.Get(l, quest.NameKey));
            }

            _status?.Invoke(HouseLifeTexts.Format(l, HouseLifeTexts.UpgradeDoneText, levelName), false);
            _card?.Invoke(HouseLifeTexts.Get(l, HouseLifeTexts.UpgradeDoneTitle), text, moved, HouseLifeTexts.Get(l, HouseLifeTexts.Ok), HouseLifeTexts.Get(l, HouseLifeTexts.Close), null);
            Refresh();
        }

        private void BuildQuests(HouseService house, LocalizationManager l)
        {
            Line(HouseLifeTexts.Get(l, HouseLifeTexts.QuestsHeader), "house-section");
            IHouseWallet wallet = _wallet?.Invoke();
            foreach (HouseQuestDefinition quest in HouseQuestChain.All)
            {
                HouseQuestState state = house.QuestState(quest, wallet);
                Label name = Line(HouseLifeTexts.Get(l, quest.NameKey) + " " + HouseLifeTexts.Get(l, HouseLifeTexts.QuestStateKey(state)));
                if (state == HouseQuestState.Completed) name.AddToClassList("house-life-row--met");
                if (state != HouseQuestState.Completed) Line(HouseLifeTexts.Get(l, quest.GoalKey), "house-life-line--dim");
                if (quest.Kind == HouseQuestKind.Deliver && state != HouseQuestState.Completed && state != HouseQuestState.Locked)
                {
                    foreach (HouseMaterialCost cost in quest.Deliver)
                    {
                        int have = wallet != null ? wallet.Count(cost.ItemId) : 0;
                        Label row = Line("  " + HouseLifeTexts.Format(l, HouseLifeTexts.ReqItem, HouseLifeTexts.Get(l, HouseLifeTexts.MaterialKey(cost.ItemId)), have, cost.Quantity));
                        row.AddToClassList(have >= cost.Quantity ? "house-life-row--met" : "house-life-row--missing");
                    }

                    string questId = quest.Id;
                    Button deliver = AddButton("wm-house-quest-" + quest.ShortId, HouseLifeTexts.Get(l, HouseLifeTexts.QuestDeliver), () => Claim(questId));
                    deliver.SetEnabled(state == HouseQuestState.Ready);
                }
            }

            if (house.OwedWardrobe.Count > 0) Line(HouseLifeTexts.Get(l, HouseLifeTexts.WardrobeOwed), "house-life-line--dim");
        }

        private void Claim(string questId)
        {
            HouseService house = _house?.Invoke();
            LocalizationManager l = L;
            HouseQuestDefinition quest = HouseQuestChain.Get(questId);
            if (house == null || quest == null) return;
            if (house.TryClaimQuest(questId, _wallet?.Invoke()))
            {
                _status?.Invoke(HouseLifeTexts.Format(l, HouseLifeTexts.QuestDoneStatus, HouseLifeTexts.Get(l, quest.NameKey)), false);
            }

            Refresh();
        }

        // ------------------------------------------------------------------
        // Life (H6)
        // ------------------------------------------------------------------

        private void BuildLife(HouseService house, LocalizationManager l)
        {
            HouseComfortReport comfort = house.Comfort;
            Line(HouseLifeTexts.Format(l, HouseLifeTexts.ComfortLine, comfort.Total, HouseLifeTexts.Get(l, HouseLifeTexts.TierKey(comfort.Tier))), "house-section");
            Line(HouseLifeTexts.Format(l, HouseLifeTexts.ComfortParts, comfort.ItemComfort, comfort.SetComfort), "house-life-line--dim");
            int missing = HouseRestedRules.ComfortToNextTier(comfort.Total);
            Line(missing > 0
                ? HouseLifeTexts.Format(l, HouseLifeTexts.ComfortNext, missing, HouseLifeTexts.Get(l, HouseLifeTexts.TierKey(comfort.Tier + 1)))
                : HouseLifeTexts.Get(l, HouseLifeTexts.ComfortTop));
            Line(house.IsRested
                ? HouseLifeTexts.Format(l, HouseLifeTexts.RestedOn, house.Rested.percent, house.Rested.fightsLeft)
                : HouseLifeTexts.Get(l, HouseLifeTexts.RestedOff));
            Line(HouseLifeTexts.Format(l, HouseLifeTexts.RestCharges, house.RestChargesLeft, HouseRestedRules.ChargesPerDay), "house-life-line--dim");

            Line(HouseLifeTexts.Get(l, HouseLifeTexts.SetsHeader), "house-section");
            List<HouseSetProgress> started = comfort.Started.OrderByDescending(s => s.Stage).ThenByDescending(s => s.Placed).ToList();
            if (started.Count == 0) Line(HouseLifeTexts.Get(l, HouseLifeTexts.SetsNone), "house-life-line--dim");
            foreach (HouseSetProgress set in started)
            {
                string text = HouseLifeTexts.Format(l, HouseLifeTexts.SetRow, HouseLifeTexts.Get(l, set.Set.NameKey), set.Placed, set.Total, HouseLifeTexts.Get(l, HouseLifeTexts.StageKey(set.Stage)));
                if (set.Complete && set.Set.Perk != HouseSetPerk.None && set.Set.Perk != HouseSetPerk.RestedExtraFights) text += " " + HouseLifeTexts.Get(l, HouseLifeTexts.PerkSoon);
                Label row = Line(text);
                if (set.Complete) row.AddToClassList("house-life-row--met");
            }

            List<string> titles = comfort.Completed.Select(s => HouseLifeTexts.Get(l, s.Set.TitleKey)).ToList();
            if (titles.Count > 0) Line(HouseLifeTexts.Format(l, HouseLifeTexts.HouseTitles, string.Join(", ", titles)));

            Line(HouseLifeTexts.Get(l, HouseLifeTexts.PlantsHeader), "house-section");
            int thirsty = house.PlantsNeedingWater;
            if (house.PlantTimers.Count == 0) Line(HouseLifeTexts.Get(l, HouseLifeTexts.PlantsNone), "house-life-line--dim");
            else Line(thirsty > 0 ? HouseLifeTexts.Format(l, HouseLifeTexts.PlantsThirsty, thirsty) : HouseLifeTexts.Get(l, HouseLifeTexts.PlantsHappy));
            Button water = AddButton("wm-house-water", HouseLifeTexts.Get(l, HouseLifeTexts.WaterButton), Water);
            water.SetEnabled(house.PlantTimers.Count > 0);

            Line(HouseLifeTexts.Get(l, HouseLifeTexts.GuestBookHeader), "house-section");
            Line(HouseLifeTexts.Format(l, HouseLifeTexts.CousinsSigned, Math.Min(HouseVisitRules.CousinsForCasaCheia, HouseVisitRules.SignedCousins(house.GuestBook).Count), HouseVisitRules.CousinsForCasaCheia), "house-life-line--dim");
            if (house.GuestBook.Count == 0) Line(HouseLifeTexts.Get(l, HouseLifeTexts.GuestBookEmpty), "house-life-line--dim");
            foreach (HouseGuestEntry entry in house.GuestBook.Reverse().Take(GuestBookShown))
            {
                Line(GuestLine(entry, l));
            }
        }

        public static string GuestLine(HouseGuestEntry entry, LocalizationManager l)
        {
            if (entry == null) return string.Empty;
            string itemName = string.IsNullOrEmpty(entry.itemId) ? HouseLifeTexts.Get(l, HouseLifeTexts.Ok) : HouseItemTexts.Name(l, entry.itemId);
            string said = HouseLifeTexts.Format(l, HouseLifeTexts.VisitLineKey(entry.visitorId, entry.line), itemName);
            return HouseLifeTexts.Format(l, HouseLifeTexts.GuestEntry, entry.day, HouseLifeTexts.Get(l, HouseLifeTexts.VisitorKey(entry.visitorId)), said);
        }

        private void Water()
        {
            HouseService house = _house?.Invoke();
            if (house == null) return;
            int perked = house.WaterPlants();
            _status?.Invoke(HouseLifeTexts.Get(L, perked > 0 ? HouseLifeTexts.Watered : HouseLifeTexts.WateredHappy), false);
            Refresh();
        }

        // ------------------------------------------------------------------

        private Label Line(string text, string extraClass = null)
        {
            var label = new Label(text ?? string.Empty);
            label.AddToClassList("house-life-line");
            if (!string.IsNullOrEmpty(extraClass)) label.AddToClassList(extraClass);
            _scroll.Add(label);
            return label;
        }

        private Button AddButton(string name, string text, Action onClick)
        {
            var button = new Button(onClick) { name = name, text = text, focusable = true };
            button.AddToClassList("rap-button");
            button.AddToClassList("house-life-button");
            _scroll.Add(button);
            return button;
        }
    }
}
