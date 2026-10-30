using System.Collections.Generic;
using System.Globalization;
using Rapadura.Core.DI;
using Rapadura.Core.Localization;
using Rapadura.Core.Logging;
using Rapadura.Core.Systems;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Inventory;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.Items.Procedural;
using Rapadura.Gameplay.Refining;
using Rapadura.Gameplay.Tacho;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rapadura.UI.WorldMap
{
    /// <summary>
    /// "Tacho Encantado da Vó" of the Vila's hub (<see cref="VillageHubButton"/> → Serviços, only where Vó Zefa lives:
    /// <see cref="Rapadura.Gameplay.World.Cities.CitySpec.HasGrandmaServices"/>). Lists the recipes of <see cref="TachoRules"/>:
    /// wings level 1 per style (on the chosen refined piece), upgrades of the wings the hero has, the Proteção do Caos (on the
    /// chosen piece) and the two Frutas do Sertão, each with its chance, cost and what a failure does (materials spent, main
    /// item kept), and a confirmation before each roll. Built in code on the map HUD's document like the forge window, with
    /// <c>Resources/UI/Tacho.uss</c>; works on the hub's live carried bag through <see cref="TachoService"/>. Offline the
    /// seed is local; online the server rolls (Fase 12) and this window only shows its answer.
    /// </summary>
    [DisallowMultipleComponent]
    public class VillageTachoPanel : MonoBehaviour
    {
        public const string OverlayName = "wm-tacho-overlay";
        public const string StyleResourcePath = "UI/Tacho";
        public const string RowPrefix = "wm-tacho-row-";

        private UIDocument _hudDocument;
        private VisualElement _overlay;
        private Label _titleLabel;
        private Label _subtitleLabel;
        private Label _grandmaLabel;
        private Label _ownedLabel;
        private Label _mainLabel;
        private Button _mainPrevious;
        private Button _mainNext;
        private ScrollView _list;
        private Label _statusLabel;
        private VisualElement _confirm;
        private Label _confirmLabel;
        private Button _confirmYes;
        private Button _confirmNo;
        private Button _closeButton;

        private InventoryManager _bag;
        private bool _open;
        private int _mainIndex;
        private List<RefineTarget> _pieces = new List<RefineTarget>();
        private (TachoRecipe Recipe, RefineTarget? Target)? _pending;
        private readonly System.Random _seeds = new System.Random();

        public bool IsOpen => _open;

        private static LocalizationManager Localization => ServiceLocator.TryGet(out LocalizationManager localization) ? localization : null;

        // ------------------------------------------------------------------
        // Pure (tested)
        // ------------------------------------------------------------------

        /// <summary>"75" for 0.75 (whole percent, invariant).</summary>
        public static string Percent(float chance) => Mathf.RoundToInt(chance * 100f).ToString(CultureInfo.InvariantCulture);

        /// <summary>"1.5" / "3" (percent with at most one decimal, invariant).</summary>
        public static string Number(float percent) => System.Math.Round(percent, 1).ToString("0.#", CultureInfo.InvariantCulture);

        /// <summary>"Custo: 1 Pedra do Caos, 6 Fibra, 3 Pedra + 60 de ouro".</summary>
        public static string CostLine(TachoPreview preview, LocalizationManager localization)
        {
            var parts = new List<string>();
            foreach (TachoIngredient ingredient in preview.Cost)
            {
                parts.Add(TachoTexts.Format(localization, TachoTexts.Ingredient, ingredient.Quantity, TachoTexts.ItemName(localization, ingredient.ItemId)));
            }

            if (preview.Gold > 0)
            {
                parts.Add(TachoTexts.Format(localization, TachoTexts.GoldPart, preview.Gold));
            }

            return TachoTexts.Format(localization, TachoTexts.Cost, string.Join(", ", parts));
        }

        /// <summary>What a failure does: the materials go, the main item (named) stays.</summary>
        public static string FailureLine(TachoPreview preview, string mainName, LocalizationManager localization)
        {
            return preview.Recipe != null && preview.Recipe.NeedsMainItem && !string.IsNullOrEmpty(mainName)
                ? TachoTexts.Format(localization, TachoTexts.FailKeepsMain, mainName)
                : TachoTexts.Get(localization, TachoTexts.FailMaterials);
        }

        /// <summary>"+3% de dano · +1% de defesa · +3% de velocidade · absorve 2% do dano" for a wing item.</summary>
        public static string WingStatsLine(string wingItemId, LocalizationManager localization)
        {
            if (!WingCatalog.TryParse(wingItemId, out WingStyle style, out int level))
            {
                return string.Empty;
            }

            string line = TachoTexts.Format(localization, TachoTexts.WingStats, Number(WingCatalog.DamagePercent(style, level) * 100f),
                Number(WingCatalog.DefensePercent(style, level) * 100f), Number(WingCatalog.MoveSpeedPercent(style, level) * 100f));
            float absorb = WingCatalog.Absorb(level);
            return absorb > 0f ? line + TachoTexts.Format(localization, TachoTexts.WingAbsorb, Number(absorb * 100f)) : line;
        }

        /// <summary>Why the button is off (empty when it can be pressed).</summary>
        public static string CheckLine(TachoPreview preview, LocalizationManager localization, System.Func<string, int> owned)
        {
            switch (preview.Check)
            {
                case TachoCheck.NoMainItem: return TachoTexts.Get(localization, TachoTexts.NeedMain);
                case TachoCheck.MainItemTooLow:
                    return preview.Recipe != null && preview.Recipe.Kind == TachoRecipeKind.CreateWings
                        ? TachoTexts.Format(localization, TachoTexts.NeedRefine, TachoRules.MinRefineForWings)
                        : TachoTexts.Get(localization, TachoTexts.NeedMain);
                case TachoCheck.MissingMaterial:
                    TachoIngredient? missing = TachoRules.FirstMissing(preview.Recipe, preview.MainLevel, owned);
                    string what = missing.HasValue
                        ? TachoTexts.Format(localization, TachoTexts.Ingredient, missing.Value.Quantity, TachoTexts.ItemName(localization, missing.Value.ItemId))
                        : string.Empty;
                    return TachoTexts.Format(localization, TachoTexts.MissingMaterial, what);
                case TachoCheck.NotEnoughGold: return TachoTexts.Format(localization, TachoTexts.MissingGold, preview.Gold);
                case TachoCheck.MaxLevel: return TachoTexts.Get(localization, TachoTexts.MaxLevel);
                case TachoCheck.NotNeeded: return TachoTexts.Get(localization, TachoTexts.NotNeeded);
                default: return string.Empty;
            }
        }

        /// <summary>The toast/log line of a finished roll.</summary>
        public static string ResultLine(TachoOutcome outcome, string mainName, string madeName, bool sentToChest, LocalizationManager localization)
        {
            if (outcome.Success)
            {
                string line = outcome.Kind == TachoRecipeKind.ProtectedRefine
                    ? TachoTexts.Format(localization, TachoTexts.ResultProtected, mainName)
                    : TachoTexts.Format(localization, TachoTexts.ResultMade, madeName);
                return sentToChest ? line + TachoTexts.Get(localization, TachoTexts.ResultToChest) : line;
            }

            return string.IsNullOrEmpty(mainName)
                ? TachoTexts.Get(localization, TachoTexts.ResultFailed)
                : TachoTexts.Format(localization, TachoTexts.ResultFailedKept, mainName);
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
            _mainIndex = 0;
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

            var panel = new VisualElement { name = "wm-tacho-panel" };
            panel.AddToClassList("rap-panel");
            panel.AddToClassList("wm-backpack-panel");
            panel.AddToClassList("tacho-panel");
            _overlay.Add(panel);

            _titleLabel = AddLabel(panel, "wm-backpack-title");
            _subtitleLabel = AddLabel(panel, "wm-backpack-hint");
            _grandmaLabel = AddLabel(panel, "tacho-grandma");
            _ownedLabel = AddLabel(panel, "tacho-owned");

            var picker = new VisualElement { name = "wm-tacho-picker" };
            picker.AddToClassList("tacho-picker");
            _mainPrevious = new Button(() => StepMain(-1)) { name = "wm-tacho-main-previous", focusable = true };
            _mainPrevious.AddToClassList("rap-button");
            _mainPrevious.AddToClassList("tacho-picker__button");
            picker.Add(_mainPrevious);
            _mainLabel = AddLabel(picker, "tacho-picker__label");
            _mainNext = new Button(() => StepMain(1)) { name = "wm-tacho-main-next", focusable = true };
            _mainNext.AddToClassList("rap-button");
            _mainNext.AddToClassList("tacho-picker__button");
            picker.Add(_mainNext);
            panel.Add(picker);

            _list = new ScrollView(ScrollViewMode.Vertical) { name = "wm-tacho-list" };
            _list.AddToClassList("tacho-list");
            panel.Add(_list);
            _statusLabel = AddLabel(panel, "tacho-status");

            _confirm = new VisualElement { name = "wm-tacho-confirm" };
            _confirm.AddToClassList("tacho-confirm");
            _confirm.style.display = DisplayStyle.None;
            _confirmLabel = AddLabel(_confirm, "tacho-confirm__text");
            var buttons = new VisualElement();
            buttons.AddToClassList("tacho-confirm__buttons");
            _confirm.Add(buttons);
            _confirmYes = new Button(OnConfirmYes) { name = "wm-tacho-confirm-yes", focusable = true };
            _confirmYes.AddToClassList("rap-button");
            _confirmYes.AddToClassList("rap-button--primary");
            buttons.Add(_confirmYes);
            _confirmNo = new Button(OnConfirmNo) { name = "wm-tacho-confirm-no", focusable = true };
            _confirmNo.AddToClassList("rap-button");
            buttons.Add(_confirmNo);
            panel.Add(_confirm);

            _closeButton = new Button(Close) { name = "wm-tacho-close-button", focusable = true };
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

        private void StepMain(int step)
        {
            if (_pieces.Count == 0)
            {
                return;
            }

            _mainIndex = (_mainIndex + step + _pieces.Count) % _pieces.Count;
            Rebuild();
        }

        private RefineTarget? ChosenPiece => _pieces.Count > 0 ? _pieces[Mathf.Clamp(_mainIndex, 0, _pieces.Count - 1)] : (RefineTarget?)null;

        private string NameOf(InventorySlotData data, LocalizationManager localization)
        {
            if (data == null)
            {
                return string.Empty;
            }

            ItemDefinition item = ItemDatabase.GetById(data.itemId);
            return WingCatalog.IsWing(data.itemId) ? TachoTexts.ItemName(localization, data.itemId) : GeneratedItemName.Compose(item, data, localization);
        }

        private void Rebuild()
        {
            if (!_open || _bag == null)
            {
                return;
            }

            LocalizationManager localization = Localization;
            TachoTexts.EnsureRegistered(localization);
            RefineTexts.EnsureRegistered(localization);
            CharacterId? hero = ServiceLocator.TryGet(out CharacterSelectionState selection) ? selection.Selected : null;

            _titleLabel.text = TachoTexts.Get(localization, TachoTexts.Title);
            _subtitleLabel.text = TachoTexts.Get(localization, TachoTexts.Subtitle);
            _grandmaLabel.text = TachoTexts.Get(localization, hero.HasValue ? TachoTexts.GreetingKey(hero.Value) : TachoTexts.GrandmaAny);
            _ownedLabel.text = TachoTexts.Format(localization, TachoTexts.Owned,
                _bag.GetTotalCount(RefineItems.ChaosStone), _bag.GetTotalCount(RefineItems.SoulStone), _bag.GetTotalCount(RefineItems.LifeStone),
                _bag.GetTotalCount(RefineItems.BlessingStone), _bag.GetTotalCount(RefineItems.Gold));
            _mainPrevious.text = TachoTexts.Get(localization, TachoTexts.MainPrevious);
            _mainNext.text = TachoTexts.Get(localization, TachoTexts.MainNext);
            _closeButton.text = TachoTexts.Get(localization, TachoTexts.Close);
            _confirmYes.text = TachoTexts.Get(localization, TachoTexts.ConfirmYes);
            _confirmNo.text = TachoTexts.Get(localization, TachoTexts.ConfirmNo);

            _pieces = RefineService.ListTargets(_bag);
            RefineTarget? piece = ChosenPiece;
            InventorySlotData pieceData = piece.HasValue ? RefineService.Resolve(_bag, piece.Value) : null;
            string pieceName = NameOf(pieceData, localization);
            if (piece.HasValue && piece.Value.IsEquipped)
            {
                pieceName += " " + TachoTexts.Get(localization, TachoTexts.Equipped);
            }

            _mainLabel.text = pieceData != null
                ? TachoTexts.Format(localization, TachoTexts.MainPiece, pieceName)
                : TachoTexts.Get(localization, TachoTexts.MainNone);
            _mainPrevious.SetEnabled(_pieces.Count > 1);
            _mainNext.SetEnabled(_pieces.Count > 1);

            _list.Clear();
            Button first = null;
            int index = 0;

            _list.Add(NewLabel(TachoTexts.Get(localization, TachoTexts.WingsHeader), "tacho-section"));
            foreach (TachoRecipe recipe in TachoRules.All)
            {
                if (recipe.Kind != TachoRecipeKind.CreateWings)
                {
                    continue;
                }

                WingStyleSpec spec = WingCatalog.Get(recipe.Style);
                string title = TachoTexts.Format(localization, TachoTexts.WingRecipe, TachoTexts.ItemName(localization, recipe.ResultItemId),
                    TachoTexts.Get(localization, TachoTexts.CousinKey(spec.Cousin)));
                _list.Add(BuildRow(recipe, piece, title, WingStatsLine(recipe.ResultItemId, localization), pieceName, index++, localization, ref first,
                    hero.HasValue && spec.Cousin == hero.Value));
            }

            _list.Add(NewLabel(TachoTexts.Get(localization, TachoTexts.UpgradeHeader), "tacho-section"));
            List<RefineTarget> wings = TachoService.ListUpgradableWings(_bag);
            if (wings.Count == 0)
            {
                _list.Add(NewLabel(TachoTexts.Get(localization, TachoTexts.NoWingsToUpgrade), "tacho-empty"));
            }

            foreach (RefineTarget wing in wings)
            {
                InventorySlotData data = RefineService.Resolve(_bag, wing);
                TachoRecipe recipe = TachoRules.UpgradeFor(data?.itemId);
                if (recipe == null)
                {
                    continue;
                }

                string wingName = NameOf(data, localization) + (wing.IsEquipped ? " " + TachoTexts.Get(localization, TachoTexts.Equipped) : string.Empty);
                string title = TachoTexts.Format(localization, TachoTexts.UpgradeRecipe, NameOf(data, localization), recipe.FromLevel, recipe.FromLevel + 1);
                _list.Add(BuildRow(recipe, wing, title, WingStatsLine(recipe.ResultItemId, localization), wingName, index++, localization, ref first, false));
            }

            _list.Add(NewLabel(TachoTexts.Get(localization, TachoTexts.LegendaryNote), "tacho-note"));

            _list.Add(NewLabel(TachoTexts.Get(localization, TachoTexts.ProtectionHeader), "tacho-section"));
            TachoRecipe protection = TachoRules.Get(TachoRules.ProtectedRefineId);
            int level = pieceData != null ? RefineRules.ClampLevel(pieceData.refineLevel) : 0;
            string step = pieceData != null && level < RefineRules.MaxLevel ? TachoTexts.Format(localization, TachoTexts.ProtectionStep, level, level + 1) : string.Empty;
            _list.Add(BuildRow(protection, piece, TachoTexts.Get(localization, TachoTexts.ProtectionRecipe), step, pieceName, index++, localization, ref first, false));

            _list.Add(NewLabel(TachoTexts.Get(localization, TachoTexts.FruitsHeader), "tacho-section"));
            TachoRecipe growth = TachoRules.Get(TachoRules.GrowthFruitId);
            _list.Add(BuildRow(growth, null, TachoTexts.ItemName(localization, growth.ResultItemId), TachoTexts.Get(localization, TachoTexts.FruitGrowthEffect),
                null, index++, localization, ref first, false));
            TachoRecipe reset = TachoRules.Get(TachoRules.ResetFruitId);
            _list.Add(BuildRow(reset, null, TachoTexts.ItemName(localization, reset.ResultItemId), TachoTexts.Get(localization, TachoTexts.FruitResetEffect),
                null, index, localization, ref first, false));

            (first ?? _closeButton).Focus();
        }

        private VisualElement BuildRow(TachoRecipe recipe, RefineTarget? target, string title, string effect, string mainName, int index,
            LocalizationManager localization, ref Button first, bool suggested)
        {
            TachoPreview preview = TachoService.Preview(_bag, recipe, target);
            var row = new VisualElement { name = RowPrefix + index };
            row.AddToClassList("tacho-row");
            if (suggested)
            {
                row.AddToClassList("tacho-row--suggested");
            }

            var body = new VisualElement();
            body.AddToClassList("tacho-row__body");
            row.Add(body);
            body.Add(NewLabel(title, "tacho-row__name"));
            if (!string.IsNullOrEmpty(effect))
            {
                body.Add(NewLabel(effect, "tacho-row__effect"));
            }

            body.Add(NewLabel(TachoTexts.Format(localization, TachoTexts.Chance, Percent(preview.Chance)), "tacho-row__chance"));
            body.Add(NewLabel(CostLine(preview, localization), "tacho-row__cost"));
            body.Add(NewLabel(FailureLine(preview, mainName, localization), "tacho-row__fail"));
            string why = CheckLine(preview, localization, _bag.GetTotalCount);
            if (!string.IsNullOrEmpty(why))
            {
                body.Add(NewLabel(why, "tacho-row__why"));
            }

            var go = new Button(() => AskConfirm(recipe, target, title, mainName))
            {
                name = RowPrefix + index + "-go",
                focusable = true,
                text = TachoTexts.Get(localization, TachoTexts.CombineButton),
            };
            go.AddToClassList("rap-button");
            go.AddToClassList("tacho-go");
            go.SetEnabled(preview.CanTry);
            row.Add(go);
            if (first == null && preview.CanTry)
            {
                first = go;
            }

            return row;
        }

        private string _pendingTitle;
        private string _pendingMainName;

        private void AskConfirm(TachoRecipe recipe, RefineTarget? target, string title, string mainName)
        {
            if (!_open || _bag == null)
            {
                return;
            }

            LocalizationManager localization = Localization;
            TachoPreview preview = TachoService.Preview(_bag, recipe, target);
            if (!preview.CanTry)
            {
                _statusLabel.text = CheckLine(preview, localization, _bag.GetTotalCount);
                return;
            }

            _pending = (recipe, target);
            _pendingTitle = title;
            _pendingMainName = mainName;
            _confirmLabel.text = TachoTexts.Format(localization, TachoTexts.Confirm, title, Percent(preview.Chance),
                CostLine(preview, localization) + ". " + FailureLine(preview, mainName, localization));
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

            (TachoRecipe recipe, RefineTarget? target) = _pending.Value;
            _pending = null;
            _confirm.style.display = DisplayStyle.None;
            LocalizationManager localization = Localization;

            // Offline: a local seed. Online the server picks it and answers with the TachoOutcome.
            if (!TachoService.TryCombine(_bag, recipe, target, _seeds.Next(), out TachoOutcome outcome, out _, out bool toChest))
            {
                _statusLabel.text = CheckLine(TachoService.Preview(_bag, recipe, target), localization, _bag.GetTotalCount);
                Rebuild();
                return;
            }

            string mainName = _pendingMainName;
            if (outcome.Success && outcome.Kind == TachoRecipeKind.ProtectedRefine && target.HasValue)
            {
                mainName = NameOf(RefineService.Resolve(_bag, target.Value), localization);
            }

            string made = outcome.ResultItemId != null ? TachoTexts.ItemName(localization, outcome.ResultItemId) : _pendingTitle;
            Report(ResultLine(outcome, mainName, made, toChest, localization));
            GameLogger.Info("Tacho", $"{outcome.RecipeId}: {(outcome.Success ? "ok" : "falhou")} (chance {outcome.Chance:0.00}, roll {outcome.Roll:0.000}, " +
                $"{outcome.FromLevel} -> {outcome.ToLevel}, item {outcome.ResultItemId ?? "-"}, baú {toChest}).");
            _mainIndex = 0;
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
