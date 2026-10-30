using System;
using System.Collections.Generic;
using System.Linq;
using Rapadura.Gameplay.Characters;

// Unity-free on purpose: the online server keeps the same collection per account and validates the same rules.
namespace Rapadura.Gameplay.Wardrobe
{
    /// <summary>Which of the two colour channels of a piece a dye paints (Housing.md §9.4).</summary>
    public enum DyeChannel
    {
        Primary = 0,
        Detail = 1,
    }

    /// <summary>Why a wardrobe action was refused.</summary>
    public enum WardrobeBlock
    {
        None,
        UnknownEntry,
        NotUnlocked,
        WrongCousin,
        WrongSlot,
        NotADye,
        NoDyeLeft,
        NoLookToDye,
        OutfitSlotLocked,
        EmptyOutfit,
    }

    /// <summary>One appearance layer of one cousin: the look shown, its two dye channels and "hide".</summary>
    [Serializable]
    public class WardrobeSlotChoice
    {
        public AppearanceSlot slot;
        /// <summary>Look id of <c>CatalogoRoupas.md</c>; empty = the model as it is (no override).</summary>
        public string lookId = string.Empty;
        public string dyePrimary = string.Empty;
        public string dyeDetail = string.Empty;
        /// <summary>"Esconder chapéu" (and any other layer): the layer is not drawn at all.</summary>
        public bool hidden;

        public bool IsEmpty => string.IsNullOrEmpty(lookId) && string.IsNullOrEmpty(dyePrimary) && string.IsNullOrEmpty(dyeDetail) && !hidden;

        public WardrobeSlotChoice Clone() => (WardrobeSlotChoice)MemberwiseClone();
    }

    /// <summary>A cousin's whole appearance (also the shape of a saved outfit).</summary>
    [Serializable]
    public class WardrobeAppearance
    {
        public string cousin;
        public WardrobeSlotChoice[] slots = Array.Empty<WardrobeSlotChoice>();

        public WardrobeAppearance Clone()
        {
            return new WardrobeAppearance
            {
                cousin = cousin,
                slots = (slots ?? Array.Empty<WardrobeSlotChoice>()).Where(s => s != null).Select(s => s.Clone()).ToArray(),
            };
        }
    }

    /// <summary>A saved outfit ("traje", §9.3): a name and an appearance.</summary>
    [Serializable]
    public class WardrobeOutfit
    {
        public string cousin;
        public int index;
        public string name = string.Empty;
        public WardrobeAppearance look = new WardrobeAppearance();
    }

    [Serializable]
    public class WardrobeDyeCount
    {
        public string dyeId;
        public int count;
    }

    /// <summary>Save slice <c>"Wardrobe"</c>: the account's Coleção, the dyes in the pot, each cousin's look and outfits.</summary>
    [Serializable]
    public class WardrobeSaveState
    {
        public int version = WardrobeBook.SaveVersion;
        public string[] unlocked = Array.Empty<string>();
        public WardrobeDyeCount[] dyes = Array.Empty<WardrobeDyeCount>();
        public WardrobeAppearance[] appearances = Array.Empty<WardrobeAppearance>();
        public WardrobeOutfit[] outfits = Array.Empty<WardrobeOutfit>();
        /// <summary>One-time sources already paid ("quest:&lt;id&gt;", "ach:&lt;id&gt;", "relic:&lt;id&gt;", "boss:&lt;id&gt;").</summary>
        public string[] claimed = Array.Empty<string>();
    }

    /// <summary>
    /// The wardrobe's rules and live state (H5, Housing.md §9.3–9.5), pure. Appearance never touches stats: nothing here
    /// knows about <c>InventoryManager</c> or the equipped items, the hero keeps the numbers of what is really equipped.
    /// <list type="bullet">
    /// <item><b>Coleção:</b> a look, once unlocked, stays unlocked for the whole account (selling the piece changes nothing).</item>
    /// <item><b>Looks:</b> per cousin and per layer; only unlocked looks of the same layer that fit the cousin (cousin sets
    /// and weapon skins only fit their owner). Switching is free.</item>
    /// <item><b>Dyes:</b> consumables counted in the "pote de tintas"; one dye paints one channel of one chosen look and is
    /// spent. The sabão de coco (<see cref="ClearDye"/>) brings the original colour back for free.</item>
    /// <item><b>Outfits:</b> <see cref="BaseOutfitSlots"/> per cousin, <see cref="RosewoodOutfitSlots"/> with the rosewood
    /// wardrobe placed in the house.</item>
    /// </list>
    /// </summary>
    public sealed class WardrobeBook
    {
        public const int SaveVersion = 1;
        public const int BaseOutfitSlots = 3;
        public const int RosewoodOutfitSlots = 6;
        /// <summary>"Guarda-roupa de jacarandá com espelho": doubles the outfit slots (§9.3).</summary>
        public const string RosewoodWardrobeItemId = "casa_util_007";

        private readonly HashSet<string> _unlocked = new HashSet<string>(StringComparer.Ordinal);
        private readonly Dictionary<string, int> _dyes = new Dictionary<string, int>(StringComparer.Ordinal);
        private readonly Dictionary<CharacterId, Dictionary<AppearanceSlot, WardrobeSlotChoice>> _looks = new Dictionary<CharacterId, Dictionary<AppearanceSlot, WardrobeSlotChoice>>();
        private readonly Dictionary<(CharacterId, int), WardrobeOutfit> _outfits = new Dictionary<(CharacterId, int), WardrobeOutfit>();
        private readonly HashSet<string> _claimed = new HashSet<string>(StringComparer.Ordinal);
        private readonly Func<string, WardrobeEntry> _resolve;

        public WardrobeBook(Func<string, WardrobeEntry> resolve = null)
        {
            _resolve = resolve ?? WardrobeCatalog.Get;
            GrantStarters();
        }

        public IReadOnlyCollection<string> Unlocked => _unlocked;
        public IReadOnlyCollection<string> Claimed => _claimed;

        public bool IsUnlocked(string lookId) => lookId != null && _unlocked.Contains(lookId);

        public int DyeCount(string dyeId) => dyeId != null && _dyes.TryGetValue(dyeId, out int n) ? n : 0;

        public IEnumerable<KeyValuePair<string, int>> Dyes => _dyes.Where(p => p.Value > 0);

        public bool IsClaimed(string key) => key != null && _claimed.Contains(key);

        /// <summary>True the first time <paramref name="key"/> is claimed.</summary>
        public bool Claim(string key) => !string.IsNullOrEmpty(key) && _claimed.Add(key);

        // ------------------------------------------------------------------
        // Gaining
        // ------------------------------------------------------------------

        /// <summary>
        /// Gives a catalog entry: a look goes into the Coleção (true only when it is new); a dye adds
        /// <paramref name="quantity"/> to the pot (always true). False for an unknown id.
        /// </summary>
        public bool Grant(string entryId, int quantity = 1)
        {
            WardrobeEntry entry = _resolve(entryId);
            if (entry == null) return false;
            if (entry.IsDye)
            {
                _dyes[entry.Id] = DyeCount(entry.Id) + Math.Max(1, quantity);
                return true;
            }

            return _unlocked.Add(entry.Id);
        }

        private void GrantStarters()
        {
            foreach (WardrobeEntry entry in WardrobeCatalog.All)
            {
                if (entry.Source == WardrobeSourceType.Starter && entry.IsLook) _unlocked.Add(entry.Id);
            }
        }

        // ------------------------------------------------------------------
        // Looks
        // ------------------------------------------------------------------

        public WardrobeSlotChoice Choice(CharacterId cousin, AppearanceSlot slot)
        {
            return _looks.TryGetValue(cousin, out var slots) && slots.TryGetValue(slot, out WardrobeSlotChoice choice) ? choice : null;
        }

        private WardrobeSlotChoice ChoiceOrNew(CharacterId cousin, AppearanceSlot slot)
        {
            if (!_looks.TryGetValue(cousin, out var slots))
            {
                slots = new Dictionary<AppearanceSlot, WardrobeSlotChoice>();
                _looks.Add(cousin, slots);
            }

            if (!slots.TryGetValue(slot, out WardrobeSlotChoice choice))
            {
                choice = new WardrobeSlotChoice { slot = slot };
                slots.Add(slot, choice);
            }

            return choice;
        }

        /// <summary>The look shown on a layer (null = the model as it is, or hidden).</summary>
        public WardrobeEntry LookOn(CharacterId cousin, AppearanceSlot slot)
        {
            WardrobeSlotChoice choice = Choice(cousin, slot);
            return choice == null || choice.hidden || string.IsNullOrEmpty(choice.lookId) ? null : _resolve(choice.lookId);
        }

        public WardrobeBlock CheckLook(CharacterId cousin, AppearanceSlot slot, string lookId)
        {
            WardrobeEntry entry = _resolve(lookId);
            if (entry == null) return WardrobeBlock.UnknownEntry;
            if (entry.IsDye) return WardrobeBlock.WrongSlot;
            if (entry.Slot != slot) return WardrobeBlock.WrongSlot;
            if (!entry.FitsCousin(cousin)) return WardrobeBlock.WrongCousin;
            if (!IsUnlocked(lookId)) return WardrobeBlock.NotUnlocked;
            return WardrobeBlock.None;
        }

        /// <summary>Shows <paramref name="lookId"/> on the layer (free). A new look keeps no dye of the previous one.</summary>
        public WardrobeBlock SetLook(CharacterId cousin, AppearanceSlot slot, string lookId)
        {
            WardrobeBlock block = CheckLook(cousin, slot, lookId);
            if (block != WardrobeBlock.None) return block;
            WardrobeSlotChoice choice = ChoiceOrNew(cousin, slot);
            if (choice.lookId != lookId)
            {
                choice.dyePrimary = string.Empty;
                choice.dyeDetail = string.Empty;
            }

            choice.lookId = lookId;
            choice.hidden = false;
            return WardrobeBlock.None;
        }

        /// <summary>Back to the model as it is on that layer (dyes dropped, nothing refunded).</summary>
        public void ClearLook(CharacterId cousin, AppearanceSlot slot)
        {
            WardrobeSlotChoice choice = Choice(cousin, slot);
            if (choice == null) return;
            choice.lookId = string.Empty;
            choice.dyePrimary = string.Empty;
            choice.dyeDetail = string.Empty;
        }

        public void SetHidden(CharacterId cousin, AppearanceSlot slot, bool hidden) => ChoiceOrNew(cousin, slot).hidden = hidden;

        // ------------------------------------------------------------------
        // Dyes
        // ------------------------------------------------------------------

        public WardrobeBlock CheckDye(CharacterId cousin, AppearanceSlot slot, string dyeId)
        {
            WardrobeEntry dye = _resolve(dyeId);
            if (dye == null) return WardrobeBlock.UnknownEntry;
            if (!dye.IsDye) return WardrobeBlock.NotADye;
            if (DyeCount(dyeId) <= 0) return WardrobeBlock.NoDyeLeft;
            WardrobeSlotChoice choice = Choice(cousin, slot);
            if (choice == null || string.IsNullOrEmpty(choice.lookId)) return WardrobeBlock.NoLookToDye;
            return WardrobeBlock.None;
        }

        /// <summary>Paints one channel of the layer's look with one dye from the pot (spent).</summary>
        public WardrobeBlock ApplyDye(CharacterId cousin, AppearanceSlot slot, DyeChannel channel, string dyeId)
        {
            WardrobeBlock block = CheckDye(cousin, slot, dyeId);
            if (block != WardrobeBlock.None) return block;
            _dyes[dyeId] = DyeCount(dyeId) - 1;
            WardrobeSlotChoice choice = ChoiceOrNew(cousin, slot);
            if (channel == DyeChannel.Primary) choice.dyePrimary = dyeId;
            else choice.dyeDetail = dyeId;
            return WardrobeBlock.None;
        }

        /// <summary>"Sabão de coco": the channel goes back to the original colour, free (the dye is not refunded).</summary>
        public void ClearDye(CharacterId cousin, AppearanceSlot slot, DyeChannel channel)
        {
            WardrobeSlotChoice choice = Choice(cousin, slot);
            if (choice == null) return;
            if (channel == DyeChannel.Primary) choice.dyePrimary = string.Empty;
            else choice.dyeDetail = string.Empty;
        }

        // ------------------------------------------------------------------
        // Outfits
        // ------------------------------------------------------------------

        public static int OutfitSlots(bool hasRosewoodWardrobe) => hasRosewoodWardrobe ? RosewoodOutfitSlots : BaseOutfitSlots;

        public WardrobeAppearance Capture(CharacterId cousin)
        {
            var look = new WardrobeAppearance { cousin = cousin.ToString() };
            if (_looks.TryGetValue(cousin, out var slots))
            {
                look.slots = slots.Values.Where(s => !s.IsEmpty).OrderBy(s => (int)s.slot).Select(s => s.Clone()).ToArray();
            }

            return look;
        }

        public WardrobeOutfit Outfit(CharacterId cousin, int index) => _outfits.TryGetValue((cousin, index), out WardrobeOutfit o) ? o : null;

        public WardrobeBlock SaveOutfit(CharacterId cousin, int index, string name, int slotsAvailable)
        {
            if (index < 0 || index >= slotsAvailable) return WardrobeBlock.OutfitSlotLocked;
            _outfits[(cousin, index)] = new WardrobeOutfit
            {
                cousin = cousin.ToString(),
                index = index,
                name = name ?? string.Empty,
                look = Capture(cousin),
            };
            return WardrobeBlock.None;
        }

        /// <summary>
        /// Wears a saved outfit (free). Dyes are not spent again: the outfit remembers the colours already paid. A look no
        /// longer valid (unknown id after a catalog change) is skipped.
        /// </summary>
        public WardrobeBlock WearOutfit(CharacterId cousin, int index, int slotsAvailable)
        {
            if (index < 0 || index >= slotsAvailable) return WardrobeBlock.OutfitSlotLocked;
            WardrobeOutfit outfit = Outfit(cousin, index);
            if (outfit == null) return WardrobeBlock.EmptyOutfit;
            _looks.Remove(cousin);
            foreach (WardrobeSlotChoice saved in outfit.look?.slots ?? Array.Empty<WardrobeSlotChoice>())
            {
                if (saved == null) continue;
                if (!string.IsNullOrEmpty(saved.lookId) && CheckLook(cousin, saved.slot, saved.lookId) != WardrobeBlock.None) continue;
                WardrobeSlotChoice choice = ChoiceOrNew(cousin, saved.slot);
                choice.lookId = saved.lookId ?? string.Empty;
                choice.dyePrimary = ValidDye(saved.dyePrimary);
                choice.dyeDetail = ValidDye(saved.dyeDetail);
                choice.hidden = saved.hidden;
            }

            return WardrobeBlock.None;
        }

        private string ValidDye(string dyeId) => !string.IsNullOrEmpty(dyeId) && _resolve(dyeId)?.IsDye == true ? dyeId : string.Empty;

        // ------------------------------------------------------------------
        // Save
        // ------------------------------------------------------------------

        public void Clear()
        {
            _unlocked.Clear();
            _dyes.Clear();
            _looks.Clear();
            _outfits.Clear();
            _claimed.Clear();
            GrantStarters();
        }

        public WardrobeSaveState CaptureState()
        {
            return new WardrobeSaveState
            {
                unlocked = _unlocked.OrderBy(id => id, StringComparer.Ordinal).ToArray(),
                dyes = _dyes.Where(p => p.Value > 0).OrderBy(p => p.Key, StringComparer.Ordinal).Select(p => new WardrobeDyeCount { dyeId = p.Key, count = p.Value }).ToArray(),
                appearances = _looks.Keys.OrderBy(c => (int)c).Select(Capture).Where(a => a.slots.Length > 0).ToArray(),
                outfits = _outfits.OrderBy(p => (int)p.Key.Item1).ThenBy(p => p.Key.Item2).Select(p => p.Value).ToArray(),
                claimed = _claimed.OrderBy(k => k, StringComparer.Ordinal).ToArray(),
            };
        }

        public void RestoreState(WardrobeSaveState state)
        {
            Clear();
            if (state == null) return;
            foreach (string id in state.unlocked ?? Array.Empty<string>())
            {
                WardrobeEntry entry = _resolve(id);
                if (entry != null && entry.IsLook) _unlocked.Add(entry.Id);
            }

            foreach (WardrobeDyeCount dye in state.dyes ?? Array.Empty<WardrobeDyeCount>())
            {
                if (dye != null && dye.count > 0 && _resolve(dye.dyeId)?.IsDye == true) _dyes[dye.dyeId] = dye.count;
            }

            foreach (string key in state.claimed ?? Array.Empty<string>())
            {
                if (!string.IsNullOrEmpty(key)) _claimed.Add(key);
            }

            foreach (WardrobeAppearance look in state.appearances ?? Array.Empty<WardrobeAppearance>())
            {
                if (look == null || !Enum.TryParse(look.cousin, out CharacterId cousin)) continue;
                foreach (WardrobeSlotChoice saved in look.slots ?? Array.Empty<WardrobeSlotChoice>())
                {
                    if (saved == null) continue;
                    if (!string.IsNullOrEmpty(saved.lookId) && CheckLook(cousin, saved.slot, saved.lookId) != WardrobeBlock.None) continue;
                    WardrobeSlotChoice choice = ChoiceOrNew(cousin, saved.slot);
                    choice.lookId = saved.lookId ?? string.Empty;
                    choice.dyePrimary = ValidDye(saved.dyePrimary);
                    choice.dyeDetail = ValidDye(saved.dyeDetail);
                    choice.hidden = saved.hidden;
                }
            }

            foreach (WardrobeOutfit outfit in state.outfits ?? Array.Empty<WardrobeOutfit>())
            {
                if (outfit == null || outfit.index < 0 || outfit.index >= RosewoodOutfitSlots || !Enum.TryParse(outfit.cousin, out CharacterId cousin)) continue;
                _outfits[(cousin, outfit.index)] = new WardrobeOutfit
                {
                    cousin = cousin.ToString(),
                    index = outfit.index,
                    name = outfit.name ?? string.Empty,
                    look = outfit.look?.Clone() ?? new WardrobeAppearance { cousin = cousin.ToString() },
                };
            }
        }
    }
}
