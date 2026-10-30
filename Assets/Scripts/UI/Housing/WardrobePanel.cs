using System;
using System.Collections.Generic;
using System.Linq;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Wardrobe;
using UnityEngine.UIElements;

namespace Rapadura.UI.Housing
{
    /// <summary>
    /// H5 "Guarda-roupa" (Housing.md §9.3–9.5), opened from a wardrobe placed in the house: the active cousin's layers
    /// (◀ ▶ through the unlocked looks that fit, hide/show, dye main/detail with the dye in hand, coconut soap), the pot of
    /// dyes and the saved outfits (3, or 6 with the rosewood wardrobe). Built in code on the house overlay and styled with
    /// the existing <c>house-life*</c> / <c>house-actions</c> classes of <c>Resources/UI/House.uss</c> (no new sheet). All
    /// rules in <see cref="WardrobeService"/> / <see cref="WardrobeBook"/>.
    /// </summary>
    public sealed class WardrobePanel
    {
        public const string PanelName = "wm-house-wardrobe";

        /// <summary>The layers shown, in order (Caramelo's outfits are listed but only recorded for now).</summary>
        public static readonly AppearanceSlot[] Layers =
        {
            AppearanceSlot.Head, AppearanceSlot.FaceNeck, AppearanceSlot.Chest, AppearanceSlot.Legs, AppearanceSlot.Boots,
            AppearanceSlot.Back, AppearanceSlot.Accessory, AppearanceSlot.Weapon, AppearanceSlot.Caramelo,
        };

        private readonly VisualElement _root;
        private readonly Label _title;
        private readonly Button _close;
        private readonly ScrollView _scroll;
        private readonly Func<WardrobeService> _wardrobe;
        private readonly Func<LocalizationManager> _localization;
        private readonly Action<string, bool> _status;
        private string _dyeInHand;

        public WardrobePanel(VisualElement parent, Func<WardrobeService> wardrobe, Func<LocalizationManager> localization, Action<string, bool> status)
        {
            _wardrobe = wardrobe;
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
            _close = new Button(Hide) { name = "wm-house-wardrobe-close", focusable = true };
            _close.AddToClassList("rap-button");
            _close.AddToClassList("house-life-close");
            header.Add(_close);
            _root.Add(header);
            _scroll = new ScrollView(ScrollViewMode.Vertical) { name = "wm-house-wardrobe-scroll" };
            _scroll.AddToClassList("house-life-scroll");
            _root.Add(_scroll);
            parent.Add(_root);
        }

        public bool IsVisible { get; private set; }
        public VisualElement Root => _root;
        private LocalizationManager L => _localization?.Invoke();

        public void Show()
        {
            IsVisible = true;
            _root.style.display = DisplayStyle.Flex;
            _wardrobe?.Invoke()?.ClaimOwedFromHouse();
            Refresh();
            _close.Focus();
        }

        public void Hide()
        {
            IsVisible = false;
            _root.style.display = DisplayStyle.None;
        }

        /// <summary>
        /// The look after (<paramref name="step"/> = 1) or before (-1) <paramref name="currentId"/> among the unlocked looks
        /// of the layer that fit the cousin; null stands for "as is" and sits at the start of the cycle. Pure.
        /// </summary>
        public static string Cycle(IReadOnlyList<string> options, string currentId, int step)
        {
            var cycle = new List<string> { null };
            cycle.AddRange(options ?? Array.Empty<string>());
            int index = string.IsNullOrEmpty(currentId) ? 0 : Math.Max(0, cycle.IndexOf(currentId));
            int next = ((index + step) % cycle.Count + cycle.Count) % cycle.Count;
            return cycle[next];
        }

        public static List<string> Options(WardrobeBook book, CharacterId cousin, AppearanceSlot slot)
        {
            return WardrobeCatalog.LooksFor(slot, cousin).Where(e => book.IsUnlocked(e.Id)).Select(e => e.Id).ToList();
        }

        public void Refresh()
        {
            WardrobeService wardrobe = _wardrobe?.Invoke();
            if (wardrobe == null || !IsVisible) return;
            LocalizationManager l = L;
            WardrobeBook book = wardrobe.Book;
            CharacterId cousin = wardrobe.ActiveCousin;
            bool pt = l == null || l.CurrentLanguage != LanguageCode.en;

            _title.text = WardrobeUiTexts.Get(l, WardrobeUiTexts.Title);
            _close.text = WardrobeUiTexts.Get(l, WardrobeUiTexts.Close);
            _scroll.Clear();
            Line(WardrobeUiTexts.Get(l, WardrobeUiTexts.Intro), "house-life-line--dim");
            Line(WardrobeUiTexts.Format(l, WardrobeUiTexts.CousinLine, Rapadura.UI.WorldMap.WorldMapHudPresenter.HeroName(l, cousin)), "house-section");
            int looks = WardrobeCatalog.Looks.Count();
            int dyes = book.Dyes.Sum(p => p.Value);
            Line(WardrobeUiTexts.Format(l, WardrobeUiTexts.CollectionLine, book.Unlocked.Count, looks, dyes), "house-life-line--dim");

            // Pot of dyes.
            Line(WardrobeUiTexts.Get(l, WardrobeUiTexts.PotSection), "house-section");
            List<string> pot = book.Dyes.Select(p => p.Key).OrderBy(id => id, StringComparer.Ordinal).ToList();
            if (_dyeInHand == null || book.DyeCount(_dyeInHand) <= 0) _dyeInHand = pot.FirstOrDefault();
            if (_dyeInHand == null)
            {
                Line(WardrobeUiTexts.Get(l, WardrobeUiTexts.PotEmpty), "house-life-line--dim");
            }
            else
            {
                VisualElement row = Row();
                RowLabel(row, WardrobeUiTexts.Format(l, WardrobeUiTexts.PotSelected, WardrobeTexts.Name(l, _dyeInHand), book.DyeCount(_dyeInHand)));
                SmallButton(row, "wm-wardrobe-dye-prev", WardrobeUiTexts.Get(l, WardrobeUiTexts.Previous), () => StepDye(pot, -1));
                SmallButton(row, "wm-wardrobe-dye-next", WardrobeUiTexts.Get(l, WardrobeUiTexts.Next), () => StepDye(pot, 1));
            }

            // Layers.
            foreach (AppearanceSlot slot in Layers)
            {
                AppearanceSlot layer = slot;
                WardrobeSlotChoice choice = book.Choice(cousin, layer);
                string lookName = choice != null && choice.hidden ? WardrobeUiTexts.Get(l, WardrobeUiTexts.LookHidden)
                    : choice != null && !string.IsNullOrEmpty(choice.lookId) ? WardrobeTexts.Name(l, choice.lookId)
                    : WardrobeUiTexts.Get(l, WardrobeUiTexts.LookNone);
                VisualElement row = Row();
                RowLabel(row, WardrobeUiTexts.Format(l, WardrobeUiTexts.SlotRow, WardrobeTexts.SlotName(layer, pt), lookName));
                List<string> options = Options(book, cousin, layer);
                SmallButton(row, "wm-wardrobe-prev-" + layer, WardrobeUiTexts.Get(l, WardrobeUiTexts.Previous), () => StepLook(cousin, layer, -1)).SetEnabled(options.Count > 0);
                SmallButton(row, "wm-wardrobe-next-" + layer, WardrobeUiTexts.Get(l, WardrobeUiTexts.Next), () => StepLook(cousin, layer, 1)).SetEnabled(options.Count > 0);
                bool hidden = choice != null && choice.hidden;
                SmallButton(row, "wm-wardrobe-hide-" + layer, WardrobeUiTexts.Get(l, hidden ? WardrobeUiTexts.Show : WardrobeUiTexts.Hide), () =>
                {
                    wardrobe.SetHidden(cousin, layer, !hidden);
                    Refresh();
                });

                if (choice != null && !string.IsNullOrEmpty(choice.lookId))
                {
                    string original = WardrobeUiTexts.Get(l, WardrobeUiTexts.DyeOriginal);
                    RowLabel(row, WardrobeUiTexts.Format(l, WardrobeUiTexts.DyeRow,
                        string.IsNullOrEmpty(choice.dyePrimary) ? original : WardrobeTexts.Name(l, choice.dyePrimary),
                        string.IsNullOrEmpty(choice.dyeDetail) ? original : WardrobeTexts.Name(l, choice.dyeDetail)));
                    SmallButton(row, "wm-wardrobe-dye-main-" + layer, WardrobeUiTexts.Get(l, WardrobeUiTexts.PaintPrimary), () => Paint(cousin, layer, DyeChannel.Primary)).SetEnabled(_dyeInHand != null);
                    SmallButton(row, "wm-wardrobe-dye-detail-" + layer, WardrobeUiTexts.Get(l, WardrobeUiTexts.PaintDetail), () => Paint(cousin, layer, DyeChannel.Detail)).SetEnabled(_dyeInHand != null);
                    SmallButton(row, "wm-wardrobe-soap-" + layer, WardrobeUiTexts.Get(l, WardrobeUiTexts.Soap), () =>
                    {
                        wardrobe.ClearDye(cousin, layer, DyeChannel.Primary);
                        wardrobe.ClearDye(cousin, layer, DyeChannel.Detail);
                        Refresh();
                    });
                }
            }

            // Outfits.
            Line(WardrobeUiTexts.Get(l, WardrobeUiTexts.OutfitsSection), "house-section");
            int slots = wardrobe.OutfitSlots;
            for (int i = 0; i < slots; i++)
            {
                int index = i;
                WardrobeOutfit outfit = book.Outfit(cousin, index);
                string name = WardrobeUiTexts.Format(l, WardrobeUiTexts.OutfitName, index + 1);
                VisualElement row = Row();
                RowLabel(row, outfit != null ? name : name + " " + WardrobeUiTexts.Get(l, WardrobeUiTexts.OutfitEmpty));
                SmallButton(row, "wm-wardrobe-outfit-save-" + index, WardrobeUiTexts.Get(l, WardrobeUiTexts.OutfitSave), () =>
                {
                    Report(wardrobe.SaveOutfit(cousin, index, name), WardrobeUiTexts.Format(L, WardrobeUiTexts.OutfitSaved, name));
                });
                SmallButton(row, "wm-wardrobe-outfit-wear-" + index, WardrobeUiTexts.Get(l, WardrobeUiTexts.OutfitWear), () =>
                {
                    Report(wardrobe.WearOutfit(cousin, index), WardrobeUiTexts.Format(L, WardrobeUiTexts.OutfitWorn, name));
                }).SetEnabled(outfit != null);
            }

            if (slots < WardrobeBook.RosewoodOutfitSlots) Line(WardrobeUiTexts.Get(l, WardrobeUiTexts.OutfitMoreSlots), "house-life-line--dim");
            Line(WardrobeUiTexts.Get(l, WardrobeUiTexts.ArtNote), "house-life-line--dim");
        }

        private void StepDye(List<string> pot, int step)
        {
            if (pot.Count == 0) return;
            int index = Math.Max(0, pot.IndexOf(_dyeInHand));
            _dyeInHand = pot[((index + step) % pot.Count + pot.Count) % pot.Count];
            Refresh();
        }

        private void StepLook(CharacterId cousin, AppearanceSlot slot, int step)
        {
            WardrobeService wardrobe = _wardrobe?.Invoke();
            if (wardrobe == null) return;
            string current = wardrobe.Book.Choice(cousin, slot)?.lookId;
            string next = Cycle(Options(wardrobe.Book, cousin, slot), current, step);
            if (next == null) wardrobe.ClearLook(cousin, slot);
            else Report(wardrobe.SetLook(cousin, slot, next), null);
            Refresh();
        }

        private void Paint(CharacterId cousin, AppearanceSlot slot, DyeChannel channel)
        {
            WardrobeService wardrobe = _wardrobe?.Invoke();
            if (wardrobe == null || _dyeInHand == null) return;
            Report(wardrobe.ApplyDye(cousin, slot, channel, _dyeInHand), null);
            Refresh();
        }

        private void Report(WardrobeBlock block, string success)
        {
            string key = WardrobeUiTexts.BlockKey(block);
            if (key != null) _status?.Invoke(WardrobeUiTexts.Get(L, key), true);
            else if (success != null) _status?.Invoke(success, false);
            Refresh();
        }

        private VisualElement Row()
        {
            var row = new VisualElement();
            row.AddToClassList("house-actions");
            _scroll.Add(row);
            return row;
        }

        private static void RowLabel(VisualElement row, string text)
        {
            var label = new Label(text ?? string.Empty);
            label.AddToClassList("house-life-line");
            label.style.width = Length.Percent(100);
            row.Add(label);
        }

        private static Button SmallButton(VisualElement row, string name, string text, Action onClick)
        {
            var button = new Button(onClick) { name = name, text = text, focusable = true };
            button.AddToClassList("rap-button");
            button.AddToClassList("house-life-close");
            row.Add(button);
            return button;
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
