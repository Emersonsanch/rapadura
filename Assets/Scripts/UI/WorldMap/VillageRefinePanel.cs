using System.Collections.Generic;
using System.Globalization;
using Rapadura.Core.DI;
using Rapadura.Core.Localization;
using Rapadura.Core.Logging;
using Rapadura.Core.Systems;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Content;
using Rapadura.Gameplay.Inventory;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.Items.Durability;
using Rapadura.Gameplay.Items.Options;
using Rapadura.Gameplay.Items.Procedural;
using Rapadura.Gameplay.Refining;
using Rapadura.UI.Common;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rapadura.UI.WorldMap
{
    /// <summary>
    /// "Ferreiro: refinar" of the city hub (<see cref="VillageHubButton"/> → Serviços, every city: the Bancada is there).
    /// Lists every refinable piece (equipped first, then the bag) with its +N, the next step's chance, cost and what a
    /// failure does (<see cref="RefineRules"/>), an Amuleto da Sorte toggle, a Pedra da Vida button and a confirmation
    /// before each roll. Built in code on the map HUD's document like the carro de boi, with
    /// <c>Resources/UI/Refine.uss</c>; works on the hub's live <see cref="CarriedInventorySession"/> bag through
    /// <see cref="RefineService"/>. The result goes to the window's status line, the HUD notice (toast) and the log.
    /// Offline the roll's seed is local; online the server rolls (Fase 12) and this window only shows its answer.
    /// </summary>
    [DisallowMultipleComponent]
    public class VillageRefinePanel : MonoBehaviour
    {
        public const string OverlayName = "wm-refine-overlay";
        public const string StyleResourcePath = "UI/Refine";
        public const string RowPrefix = "wm-refine-row-";

        private UIDocument _hudDocument;
        private VisualElement _overlay;
        private Label _titleLabel;
        private Label _subtitleLabel;
        private Label _smithLabel;
        private Label _ownedLabel;
        private Toggle _charmToggle;
        private Button _repairAllButton;
        private ScrollView _list;
        private Label _statusLabel;
        private VisualElement _confirm;
        private Label _confirmLabel;
        private Button _confirmYes;
        private Button _confirmNo;
        private Button _closeButton;

        private InventoryManager _bag;
        private bool _open;
        private RefineTarget? _pending;
        private readonly System.Random _seeds = new System.Random();

        public bool IsOpen => _open;

        private static LocalizationManager Localization => ServiceLocator.TryGet(out LocalizationManager localization) ? localization : null;

        // ------------------------------------------------------------------
        // Pure (tested)
        // ------------------------------------------------------------------

        /// <summary>"75" for 0.75 (whole percent, invariant).</summary>
        public static string Percent(float chance) => Mathf.RoundToInt(chance * 100f).ToString(CultureInfo.InvariantCulture);

        /// <summary>The "what happens on failure" line of <paramref name="preview"/>.</summary>
        public static string FailureLine(RefinePreview preview, LocalizationManager localization)
        {
            if (preview.Level >= RefineRules.MaxLevel)
            {
                return RefineTexts.Get(localization, RefineTexts.MaxLevel);
            }

            if (!preview.CanFail)
            {
                return RefineTexts.Get(localization, RefineTexts.FailNever);
            }

            return preview.FailureLevel < preview.Level
                ? RefineTexts.Format(localization, RefineTexts.FailDrop, preview.FailureLevel)
                : RefineTexts.Format(localization, RefineTexts.FailKeep, preview.FailureLevel);
        }

        /// <summary>"Custo: 1 Pedra da Alma + 30 de ouro".</summary>
        public static string CostLine(RefinePreview preview, LocalizationManager localization)
        {
            string stone = RefineTexts.StoneName(localization, preview.Stone);
            return preview.GoldCost > 0
                ? RefineTexts.Format(localization, RefineTexts.CostGold, stone, preview.GoldCost)
                : RefineTexts.Format(localization, RefineTexts.Cost, stone);
        }

        /// <summary>Why the refine button is off (empty when it can be pressed).</summary>
        public static string CheckLine(RefineCheck check, RefinePreview preview, LocalizationManager localization)
        {
            switch (check)
            {
                case RefineCheck.MissingStone: return RefineTexts.Format(localization, RefineTexts.MissingStone, RefineTexts.StoneName(localization, preview.Stone));
                case RefineCheck.NotEnoughGold: return RefineTexts.Format(localization, RefineTexts.MissingGold, preview.GoldCost);
                case RefineCheck.MissingCharm: return RefineTexts.Get(localization, RefineTexts.MissingCharm);
                case RefineCheck.MaxLevel: return RefineTexts.Get(localization, RefineTexts.MaxLevel);
                case RefineCheck.MaxOption: return RefineTexts.Get(localization, RefineTexts.MaxOption);
                default: return string.Empty;
            }
        }

        /// <summary>The toast/log line of a finished roll; <paramref name="name"/> already carries the new +N.</summary>
        public static string ResultLine(RefineAttempt attempt, string name, LocalizationManager localization)
        {
            if (attempt.Success)
            {
                return RefineTexts.Format(localization, RefineTexts.ResultSuccess, name);
            }

            return attempt.Dropped
                ? RefineTexts.Format(localization, RefineTexts.ResultDropped, name)
                : RefineTexts.Format(localization, RefineTexts.ResultKept, name);
        }

        /// <summary>USS class of the glow tier (refine-glow--1..5), null below +7.</summary>
        public static string GlowClass(int level)
        {
            int tier = RefineRules.GlowTier(level);
            return tier > 0 ? "refine-glow--" + tier : null;
        }

        // ------------------------------------------------------------------
        // Window
        // ------------------------------------------------------------------

        private void Awake()
        {
            _hudDocument = GetComponent<UIDocument>();
        }

        private void OnDisable()
        {
            Close();
            _overlay?.RemoveFromHierarchy();
            _overlay = null;
        }

        /// <summary>Opens the window on <paramref name="bag"/> (the hub's live carried inventory).</summary>
        public void Open(InventoryManager bag)
        {
            if (_open || bag == null || !EnsureOverlay())
            {
                return;
            }

            _bag = bag;
            _open = true;
            _pending = null;
            _statusLabel.text = string.Empty;
            _confirm.style.display = DisplayStyle.None;
            _overlay.style.display = DisplayStyle.Flex;
            GameplayPause.Acquire(this);
            Rebuild();
        }

        public void Close()
        {
            if (_overlay != null)
            {
                _overlay.style.display = DisplayStyle.None;
            }

            _open = false;
            _bag = null;
            _pending = null;
            GameplayPause.Release(this);
        }

        private bool EnsureOverlay()
        {
            if (_hudDocument == null || _hudDocument.rootVisualElement == null)
            {
                return false;
            }

            VisualElement root = _hudDocument.rootVisualElement.Q<VisualElement>("wm-hud-root") ?? _hudDocument.rootVisualElement;
            if (_overlay != null && _overlay.parent == root)
            {
                return true;
            }

            _overlay?.RemoveFromHierarchy();
            _overlay = new VisualElement { name = OverlayName };
            _overlay.AddToClassList("wm-backpack-overlay");
            _overlay.style.display = DisplayStyle.None;
            var sheet = Resources.Load<StyleSheet>(StyleResourcePath);
            if (sheet != null)
            {
                _overlay.styleSheets.Add(sheet);
            }

            var panel = new VisualElement { name = "wm-refine-panel" };
            panel.AddToClassList("rap-panel");
            panel.AddToClassList("wm-backpack-panel");
            panel.AddToClassList("refine-panel");
            _overlay.Add(panel);

            _titleLabel = AddLabel(panel, "wm-backpack-title");
            _subtitleLabel = AddLabel(panel, "wm-backpack-hint");
            _smithLabel = AddLabel(panel, "refine-smith");
            _ownedLabel = AddLabel(panel, "refine-owned");

            // Repair (FASE 16, RepairService): every worn piece at once, tools included; rows below repair one piece.
            _repairAllButton = new Button(OnRepairAllClicked) { name = "wm-refine-repair-all", focusable = true };
            _repairAllButton.AddToClassList("rap-button");
            _repairAllButton.AddToClassList("refine-repair-all");
            panel.Add(_repairAllButton);

            _charmToggle = new Toggle { name = "wm-refine-charm", focusable = true };
            _charmToggle.AddToClassList("refine-charm");
            _charmToggle.RegisterValueChangedCallback(_ => Rebuild());
            panel.Add(_charmToggle);

            _list = new ScrollView(ScrollViewMode.Vertical) { name = "wm-refine-list" };
            _list.AddToClassList("refine-list");
            panel.Add(_list);
            _statusLabel = AddLabel(panel, "refine-status");

            _confirm = new VisualElement { name = "wm-refine-confirm" };
            _confirm.AddToClassList("refine-confirm");
            _confirm.style.display = DisplayStyle.None;
            _confirmLabel = AddLabel(_confirm, "refine-confirm__text");
            var buttons = new VisualElement();
            buttons.AddToClassList("refine-confirm__buttons");
            _confirm.Add(buttons);
            _confirmYes = new Button(OnConfirmYes) { name = "wm-refine-confirm-yes", focusable = true };
            _confirmYes.AddToClassList("rap-button");
            _confirmYes.AddToClassList("rap-button--primary");
            buttons.Add(_confirmYes);
            _confirmNo = new Button(OnConfirmNo) { name = "wm-refine-confirm-no", focusable = true };
            _confirmNo.AddToClassList("rap-button");
            buttons.Add(_confirmNo);
            panel.Add(_confirm);

            _closeButton = new Button(Close) { name = "wm-refine-close-button", focusable = true };
            _closeButton.AddToClassList("rap-button");
            _closeButton.AddToClassList("rap-button--primary");
            _closeButton.AddToClassList("wm-backpack-close");
            panel.Add(_closeButton);

            root.Add(_overlay);
            return true;
        }

        private static Label AddLabel(VisualElement parent, string className)
        {
            var label = new Label();
            label.AddToClassList(className);
            parent.Add(label);
            return label;
        }

        private static Label NewLabel(string text, string className)
        {
            var label = new Label(text);
            label.AddToClassList(className);
            return label;
        }

        private void Rebuild()
        {
            if (!_open || _bag == null)
            {
                return;
            }

            LocalizationManager localization = Localization;
            RefineTexts.EnsureRegistered(localization);
            CharacterId? hero = ServiceLocator.TryGet(out CharacterSelectionState selection) ? selection.Selected : null;

            _titleLabel.text = RefineTexts.Get(localization, RefineTexts.Title);
            _subtitleLabel.text = RefineTexts.Get(localization, RefineTexts.Subtitle);
            _smithLabel.text = RefineTexts.Get(localization, hero.HasValue ? RefineTexts.GreetingKey(hero.Value) : RefineTexts.SmithAny);
            _ownedLabel.text = RefineTexts.Format(localization, RefineTexts.Owned,
                _bag.GetTotalCount(RefineItems.BlessingStone), _bag.GetTotalCount(RefineItems.SoulStone),
                _bag.GetTotalCount(RefineItems.LifeStone), _bag.GetTotalCount(RefineItems.LuckCharm), _bag.GetTotalCount(RefineItems.Gold));
            _charmToggle.label = RefineTexts.Get(localization, RefineTexts.UseCharm);
            DurabilityTexts.EnsureRegistered(localization);
            ItemOptionTexts.EnsureRegistered(localization);
            int repairCost = RepairService.RepairAllCost(_bag, out int wornPieces);
            _repairAllButton.text = wornPieces > 0
                ? DurabilityTexts.Format(localization, DurabilityTexts.RepairAllButton, wornPieces, repairCost)
                : DurabilityTexts.Get(localization, DurabilityTexts.NothingToRepair);
            _repairAllButton.SetEnabled(wornPieces > 0 && _bag.GetTotalCount(RefineItems.Gold) >= repairCost);
            _closeButton.text = RefineTexts.Get(localization, RefineTexts.Close);
            _confirmYes.text = RefineTexts.Get(localization, RefineTexts.ConfirmYes);
            _confirmNo.text = RefineTexts.Get(localization, RefineTexts.ConfirmNo);

            _list.Clear();
            List<RefineTarget> targets = RefineService.ListTargets(_bag);
            if (targets.Count == 0)
            {
                _list.Add(NewLabel(RefineTexts.Get(localization, RefineTexts.Empty), "refine-empty"));
                _closeButton.Focus();
                return;
            }

            Button first = null;
            for (int i = 0; i < targets.Count; i++)
            {
                RefineTarget target = targets[i];
                RefinePreview preview = RefineService.Preview(_bag, target, _charmToggle.value);
                VisualElement row = BuildRow(target, preview, i, localization, ref first);
                _list.Add(row);
            }

            (first ?? _closeButton).Focus();
        }

        private VisualElement BuildRow(RefineTarget target, RefinePreview preview, int index, LocalizationManager localization, ref Button first)
        {
            var row = new VisualElement { name = RowPrefix + index };
            row.AddToClassList("refine-row");
            string glow = GlowClass(preview.Level);
            if (glow != null)
            {
                row.AddToClassList(glow);
            }

            var body = new VisualElement();
            body.AddToClassList("refine-row__body");
            row.Add(body);

            string name = GeneratedItemName.Compose(preview.Item, preview.Data, localization);
            if (target.IsEquipped)
            {
                name += " " + RefineTexts.Get(localization, RefineTexts.Equipped);
            }

            Label title = NewLabel(name, "refine-row__name");
            TooltipController.ApplyRefineGlow(title, RefineRules.GlowTier(preview.Level));
            body.Add(title);

            if (preview.Level < RefineRules.MaxLevel)
            {
                body.Add(NewLabel(RefineTexts.Format(localization, RefineTexts.Step, preview.Level, preview.Level + 1, Percent(preview.Chance)), "refine-row__step"));
                body.Add(NewLabel(CostLine(preview, localization), "refine-row__cost"));
            }

            body.Add(NewLabel(FailureLine(preview, localization), preview.CanFail ? "refine-row__fail" : "refine-row__safe"));
            string why = CheckLine(preview.Check, preview, localization);
            if (!string.IsNullOrEmpty(why))
            {
                body.Add(NewLabel(why, "refine-row__why"));
            }

            var actions = new VisualElement();
            actions.AddToClassList("refine-row__actions");
            row.Add(actions);

            var refine = new Button(() => AskConfirm(target)) { name = RowPrefix + index + "-refine", focusable = true, text = RefineTexts.Get(localization, RefineTexts.RefineButton) };
            refine.AddToClassList("rap-button");
            refine.AddToClassList("refine-go");
            refine.SetEnabled(preview.CanRefine);
            actions.Add(refine);
            if (first == null && preview.CanRefine)
            {
                first = refine;
            }

            var option = new Button(() => OnOptionClicked(target))
            {
                name = RowPrefix + index + "-option",
                focusable = true,
                text = RefineTexts.Format(localization, RefineTexts.OptionButton, preview.Option, RefineRules.MaxOptionLevel, Percent(RefineRules.LifeStoneChance)),
            };
            option.AddToClassList("rap-button");
            option.AddToClassList("refine-option");
            option.SetEnabled(preview.CanAddOption);
            actions.Add(option);

            AddRepairAndSockets(target, preview, index, localization, body, actions);
            return row;
        }

        /// <summary>The row's durability line + "Consertar", its option/socket lines and one "Engastar" button per engraving.</summary>
        private void AddRepairAndSockets(RefineTarget target, RefinePreview preview, int index, LocalizationManager localization,
            VisualElement body, VisualElement actions)
        {
            RepairQuote quote = RepairService.Quote(_bag, target);
            if (quote.Check != RepairCheck.NotRepairable)
            {
                body.Add(NewLabel(DurabilityTexts.Format(localization, DurabilityTexts.RowLine,
                    Mathf.CeilToInt(quote.Current).ToString(CultureInfo.InvariantCulture),
                    Mathf.RoundToInt(quote.Max).ToString(CultureInfo.InvariantCulture), quote.Cost),
                    quote.IsBroken ? "refine-row__fail" : "refine-row__cost"));
                if (quote.Cost > 0)
                {
                    var repair = new Button(() => OnRepairClicked(target))
                    {
                        name = RowPrefix + index + "-repair",
                        focusable = true,
                        text = DurabilityTexts.Format(localization, DurabilityTexts.RepairButton, quote.Cost),
                    };
                    repair.AddToClassList("rap-button");
                    repair.AddToClassList("refine-repair");
                    repair.SetEnabled(quote.CanRepair);
                    actions.Add(repair);
                }
            }

            foreach (string line in ItemOptionTexts.TooltipLines(localization, preview.Item, preview.Data))
            {
                Label label = NewLabel(line, "refine-row__option");
                label.style.color = new StyleColor(new Color(0.44f, 0.86f, 0.44f));
                body.Add(label);
            }

            if (ItemOptionRules.FirstEmptySocket(preview.Data) < 0)
            {
                return;
            }

            body.Add(NewLabel(ItemOptionTexts.Get(localization, ItemOptionTexts.EngraveTitle), "refine-row__step"));
            for (int e = 0; e < ItemOptionCatalog.Engravings.Length; e++)
            {
                Engraving engraving = ItemOptionCatalog.Engravings[e];
                if (!ItemOptionRules.CanEngrave(preview.Item, engraving))
                {
                    continue;
                }

                var engrave = new Button(() => OnEngraveClicked(target, engraving.Id))
                {
                    name = RowPrefix + index + "-engrave-" + e,
                    focusable = true,
                    text = ItemOptionTexts.Format(localization, ItemOptionTexts.EngraveButton, ItemOptionTexts.Get(localization, engraving.NameKey),
                        engraving.MaterialQuantity, MaterialName(engraving.MaterialId, localization), engraving.Gold),
                };
                engrave.AddToClassList("rap-button");
                engrave.AddToClassList("refine-engrave");
                engrave.SetEnabled(EngraveService.Check(_bag, target, engraving.Id) == EngraveCheck.Ok);
                actions.Add(engrave);
            }
        }

        private static string MaterialName(string itemId, LocalizationManager localization)
        {
            ItemDefinition item = ItemDatabase.GetById(itemId);
            return item != null ? item.LocalizedName(localization) : itemId;
        }

        private void OnRepairClicked(RefineTarget target)
        {
            if (!_open || _bag == null)
            {
                return;
            }

            LocalizationManager localization = Localization;
            InventorySlotData data = RefineService.Resolve(_bag, target);
            string name = data != null ? GeneratedItemName.Compose(ItemDatabase.GetById(data.itemId), data, localization) : string.Empty;
            if (RepairService.TryRepair(_bag, target, out int cost, out RepairCheck check))
            {
                Report(DurabilityTexts.Format(localization, DurabilityTexts.Repaired, name));
                GameLogger.Info("Repair", $"{data?.itemId} repaired for {cost} gold.");
            }
            else
            {
                _statusLabel.text = check == RepairCheck.NotEnoughGold
                    ? DurabilityTexts.Format(localization, DurabilityTexts.MissingGold, cost)
                    : DurabilityTexts.Get(localization, DurabilityTexts.NothingToRepair);
            }

            Rebuild();
        }

        private void OnRepairAllClicked()
        {
            if (!_open || _bag == null)
            {
                return;
            }

            LocalizationManager localization = Localization;
            if (RepairService.TryRepairAll(_bag, out int cost, out int pieces, out RepairCheck check))
            {
                Report(DurabilityTexts.Format(localization, DurabilityTexts.RepairedAll, pieces, cost));
                GameLogger.Info("Repair", $"{pieces} piece(s) repaired for {cost} gold.");
            }
            else
            {
                _statusLabel.text = check == RepairCheck.NotEnoughGold
                    ? DurabilityTexts.Format(localization, DurabilityTexts.MissingGold, cost)
                    : DurabilityTexts.Get(localization, DurabilityTexts.NothingToRepair);
            }

            Rebuild();
        }

        private void OnEngraveClicked(RefineTarget target, string engravingId)
        {
            if (!_open || _bag == null)
            {
                return;
            }

            LocalizationManager localization = Localization;
            if (EngraveService.TryEngrave(_bag, target, engravingId, out EngraveCheck check))
            {
                InventorySlotData data = RefineService.Resolve(_bag, target);
                string name = data != null ? GeneratedItemName.Compose(ItemDatabase.GetById(data.itemId), data, localization) : string.Empty;
                Engraving engraving = ItemOptionCatalog.FindEngraving(engravingId);
                Report(ItemOptionTexts.Format(localization, ItemOptionTexts.Engraved, name, ItemOptionTexts.Get(localization, engraving?.NameKey)));
            }
            else
            {
                _statusLabel.text = ItemOptionTexts.Get(localization,
                    check == EngraveCheck.NoSocket ? ItemOptionTexts.EngraveNoSocket : ItemOptionTexts.EngraveMissing);
            }

            Rebuild();
        }

        private void AskConfirm(RefineTarget target)
        {
            if (!_open || _bag == null)
            {
                return;
            }

            LocalizationManager localization = Localization;
            RefinePreview preview = RefineService.Preview(_bag, target, _charmToggle.value);
            if (!preview.CanRefine)
            {
                _statusLabel.text = CheckLine(preview.Check, preview, localization);
                return;
            }

            _pending = target;
            string name = GeneratedItemName.ComposeUnrefined(preview.Item, preview.Data, localization);
            _confirmLabel.text = RefineTexts.Format(localization, RefineTexts.Confirm, name, preview.Level, preview.Level + 1,
                Percent(preview.Chance), CostLine(preview, localization) + ". " + FailureLine(preview, localization));
            _confirm.style.display = DisplayStyle.Flex;
            _confirmYes.Focus();
        }

        private void OnConfirmNo()
        {
            _pending = null;
            _confirm.style.display = DisplayStyle.None;
            Rebuild();
        }

        private void OnConfirmYes()
        {
            if (!_open || _bag == null || !_pending.HasValue)
            {
                return;
            }

            RefineTarget target = _pending.Value;
            _pending = null;
            _confirm.style.display = DisplayStyle.None;
            LocalizationManager localization = Localization;

            // Offline: a local seed. Online the server picks it and answers with the RefineAttempt.
            if (!RefineService.TryRefine(_bag, target, _charmToggle.value, _seeds.Next(), out RefineAttempt attempt, out RefineCheck check))
            {
                RefinePreview preview = RefineService.Preview(_bag, target, _charmToggle.value);
                _statusLabel.text = CheckLine(check, preview, localization);
                Rebuild();
                return;
            }

            InventorySlotData data = RefineService.Resolve(_bag, target);
            string name = data != null ? GeneratedItemName.Compose(ItemDatabase.GetById(data.itemId), data, localization) : string.Empty;
            Report(ResultLine(attempt, name, localization));
            GameLogger.Info("Refine", $"{data?.itemId} +{attempt.FromLevel} -> +{attempt.ToLevel} (chance {attempt.Chance:0.00}, roll {attempt.Roll:0.000}, charm {attempt.UsedCharm}, gold {attempt.Gold}).");
            Rebuild();
        }

        private void OnOptionClicked(RefineTarget target)
        {
            if (!_open || _bag == null)
            {
                return;
            }

            LocalizationManager localization = Localization;
            if (!RefineService.TryAddOption(_bag, target, _seeds.Next(), out bool success, out RefineCheck check))
            {
                _statusLabel.text = CheckLine(check, RefineService.Preview(_bag, target, false), localization);
                Rebuild();
                return;
            }

            InventorySlotData data = RefineService.Resolve(_bag, target);
            string name = data != null ? GeneratedItemName.Compose(ItemDatabase.GetById(data.itemId), data, localization) : string.Empty;
            Report(success
                ? RefineTexts.Format(localization, RefineTexts.OptionSuccess, name, data != null ? data.refineOptionLevel : 0, RefineRules.MaxOptionLevel)
                : RefineTexts.Format(localization, RefineTexts.OptionFailed, name));
            Rebuild();
        }

        /// <summary>Status line + the map HUD's notice (toast).</summary>
        private void Report(string text)
        {
            _statusLabel.text = text;
            CitySafeZoneHud notice = GetComponent<CitySafeZoneHud>();
            if (notice != null)
            {
                notice.ShowNotice(text);
            }
        }
    }
}
