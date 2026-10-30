using System.Collections.Generic;
using System.Linq;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Wardrobe;

namespace Rapadura.UI.Housing
{
    /// <summary>
    /// Localization (en + pt-BR) of the wardrobe screen (H5, <see cref="WardrobePanel"/>) and its toast. The entry names
    /// themselves live in <see cref="WardrobeTexts"/>. Same self-contained pattern as <c>HouseSourceTexts</c>.
    /// </summary>
    public static class WardrobeUiTexts
    {
        public const string Title = "ui.wardrobe.title";
        public const string Intro = "ui.wardrobe.intro";
        /// <summary>{0} = cousin name.</summary>
        public const string CousinLine = "ui.wardrobe.cousin_line";
        /// <summary>{0} = unlocked looks, {1} = all looks, {2} = dyes in the pot.</summary>
        public const string CollectionLine = "ui.wardrobe.collection_line";
        /// <summary>{0} = layer, {1} = look.</summary>
        public const string SlotRow = "ui.wardrobe.slot_row";
        public const string LookNone = "ui.wardrobe.look_none";
        public const string LookHidden = "ui.wardrobe.look_hidden";
        /// <summary>{0} = primary dye, {1} = detail dye.</summary>
        public const string DyeRow = "ui.wardrobe.dye_row";
        public const string DyeOriginal = "ui.wardrobe.dye_original";
        public const string Previous = "ui.wardrobe.previous";
        public const string Next = "ui.wardrobe.next";
        public const string Hide = "ui.wardrobe.hide";
        public const string Show = "ui.wardrobe.show";
        public const string PaintPrimary = "ui.wardrobe.paint_primary";
        public const string PaintDetail = "ui.wardrobe.paint_detail";
        public const string Soap = "ui.wardrobe.soap";
        public const string PotSection = "ui.wardrobe.pot_section";
        /// <summary>{0} = dye name, {1} = how many.</summary>
        public const string PotSelected = "ui.wardrobe.pot_selected";
        public const string PotEmpty = "ui.wardrobe.pot_empty";
        public const string OutfitsSection = "ui.wardrobe.outfits_section";
        /// <summary>{0} = number.</summary>
        public const string OutfitName = "ui.wardrobe.outfit_name";
        public const string OutfitEmpty = "ui.wardrobe.outfit_empty";
        public const string OutfitSave = "ui.wardrobe.outfit_save";
        public const string OutfitWear = "ui.wardrobe.outfit_wear";
        public const string OutfitMoreSlots = "ui.wardrobe.outfit_more_slots";
        /// <summary>{0} = outfit name.</summary>
        public const string OutfitSaved = "ui.wardrobe.outfit_saved";
        /// <summary>{0} = outfit name.</summary>
        public const string OutfitWorn = "ui.wardrobe.outfit_worn";
        public const string ArtNote = "ui.wardrobe.art_note";
        public const string Close = "ui.wardrobe.close";
        public const string ToastTitleLook = "ui.wardrobe.toast_title_look";
        public const string ToastTitleDye = "ui.wardrobe.toast_title_dye";
        /// <summary>{0} = entry name.</summary>
        public const string ToastBody = "ui.wardrobe.toast_body";
        public const string BlockNotUnlocked = "ui.wardrobe.block_not_unlocked";
        public const string BlockNoDye = "ui.wardrobe.block_no_dye";
        public const string BlockNoLook = "ui.wardrobe.block_no_look";
        public const string BlockOutfitLocked = "ui.wardrobe.block_outfit_locked";
        public const string BlockFailed = "ui.wardrobe.block_failed";

        private static readonly LocalizationEntry[] EntryArray =
        {
            E(Title, "Wardrobe", "Guarda-roupa"),
            E(Intro, "Looks only change how the cousin looks: the attributes stay those of the gear really equipped. Changing is free.",
                "A aparência só muda o visual: os atributos continuam os da peça equipada de verdade. Trocar é grátis."),
            E(CousinLine, "Dressing: {0}", "Vestindo: {0}"),
            E(CollectionLine, "Collection: {0}/{1} looks · {2} dyes in the pot", "Coleção: {0}/{1} visuais · {2} tintas no pote"),
            E(SlotRow, "{0}: {1}", "{0}: {1}"),
            E(LookNone, "as is", "do jeito que está"),
            E(LookHidden, "hidden", "escondido"),
            E(DyeRow, "  colours: {0} / {1}", "  cores: {0} / {1}"),
            E(DyeOriginal, "original", "original"),
            E(Previous, "◀", "◀"),
            E(Next, "▶", "▶"),
            E(Hide, "Hide", "Esconder"),
            E(Show, "Show", "Mostrar"),
            E(PaintPrimary, "Dye main", "Pintar principal"),
            E(PaintDetail, "Dye detail", "Pintar detalhe"),
            E(Soap, "Coconut soap", "Sabão de coco"),
            E(PotSection, "Pot of dyes (one dye paints one colour and is used up; the coconut soap brings the original back for free)",
                "Pote de tintas (uma tinta pinta uma cor e é gasta; o sabão de coco volta à cor original de graça)"),
            E(PotSelected, "Dye in hand: {0} (x{1})", "Tinta na mão: {0} (x{1})"),
            E(PotEmpty, "No dyes yet: Mariam's thread bazaar, Nana's Bazaar and Zaku's grocery sell them.", "Nenhuma tinta ainda: o bazar de linhas da Mariam, o Bazar da Nana e a mercearia do Zaku vendem."),
            E(OutfitsSection, "Saved outfits", "Trajes salvos"),
            E(OutfitName, "Outfit {0}", "Traje {0}"),
            E(OutfitEmpty, "(empty)", "(vazio)"),
            E(OutfitSave, "Save", "Salvar"),
            E(OutfitWear, "Wear", "Vestir"),
            E(OutfitMoreSlots, "The big wardrobe with a mirror doubles the outfits (6).", "O guarda-roupa grande com espelho dobra os trajes (6)."),
            E(OutfitSaved, "{0} saved.", "{0} salvo."),
            E(OutfitWorn, "Wearing {0}.", "Vestindo o {0}."),
            E(ArtNote, "For now the chest look tints the clothes and caps, scarves and saddlebags are added on top; legs, feet and Ari looks get their models later.",
                "Por enquanto o visual do peito tinge a roupa e gorro, lenço e alforje aparecem por cima; pernas, pés e o Ari ganham modelo depois."),
            E(Close, "Close", "Fechar"),
            E(ToastTitleLook, "New look in the wardrobe!", "Visual novo no guarda-roupa!"),
            E(ToastTitleDye, "A dye for the pot!", "Tinta para o pote!"),
            E(ToastBody, "{0}", "{0}"),
            E(BlockNotUnlocked, "That look is not in the collection yet.", "Esse visual ainda não está na coleção."),
            E(BlockNoDye, "No dye of that colour left.", "Acabou essa tinta."),
            E(BlockNoLook, "Choose a look first, then paint it.", "Escolha um visual antes de pintar."),
            E(BlockOutfitLocked, "That outfit needs the big wardrobe with a mirror.", "Esse traje precisa do guarda-roupa grande com espelho."),
            E(BlockFailed, "That did not work.", "Não deu certo."),
        };

        private static Dictionary<string, string> _fallbackByKey;
        private static LocalizationManager _mergedInto;

        public static IReadOnlyList<LocalizationEntry> Entries => EntryArray;

        private static LocalizationEntry E(string key, string en, string ptBR) => new LocalizationEntry(key, en, ptBR);

        public static string BlockKey(WardrobeBlock block)
        {
            switch (block)
            {
                case WardrobeBlock.None: return null;
                case WardrobeBlock.NotUnlocked: return BlockNotUnlocked;
                case WardrobeBlock.NoDyeLeft: return BlockNoDye;
                case WardrobeBlock.NoLookToDye: return BlockNoLook;
                case WardrobeBlock.OutfitSlotLocked: return BlockOutfitLocked;
                default: return BlockFailed;
            }
        }

        public static void EnsureRegistered(LocalizationManager manager)
        {
            if (manager == null || (ReferenceEquals(manager, _mergedInto) && manager.HasKey(EntryArray[0].Key)))
            {
                return;
            }

            manager.LoadEntries(EntryArray.Where(entry => !manager.HasKey(entry.Key)).ToList());
            _mergedInto = manager;
        }

        public static string Get(LocalizationManager manager, string key)
        {
            if (string.IsNullOrEmpty(key)) return string.Empty;
            if (manager != null)
            {
                EnsureRegistered(manager);
                if (manager.HasKey(key)) return manager.Get(key);
            }

            return GetFallback(key) ?? key;
        }

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

        public static string GetFallback(string key)
        {
            if (key == null) return null;
            if (_fallbackByKey == null)
            {
                var table = new Dictionary<string, string>();
                foreach (LocalizationEntry entry in EntryArray) table[entry.Key] = entry.PtBR;
                _fallbackByKey = table;
            }

            return _fallbackByKey.TryGetValue(key, out string text) ? text : null;
        }
    }
}
