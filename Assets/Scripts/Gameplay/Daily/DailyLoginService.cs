using System;
using System.Collections.Generic;
using Rapadura.Core.Events;
using Rapadura.Core.Interfaces;
using Rapadura.Core.Logging;
using Rapadura.Gameplay.Inventory;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.Items.Procedural;
using Rapadura.Gameplay.Quests;
using UnityEngine;

namespace Rapadura.Gameplay.Daily
{
    /// <summary>One reward line as it was actually given.</summary>
    public readonly struct DailyRewardGrant
    {
        public readonly ItemDefinition Item;

        /// <summary>The rare piece's rolled name ("Elmo de Couro do Vaqueiro"), else null.</summary>
        public readonly string GeneratedName;

        public readonly int ToBag;
        public readonly int ToChest;
        public readonly int Lost;

        public DailyRewardGrant(ItemDefinition item, string generatedName, int toBag, int toChest, int lost)
        {
            Item = item;
            GeneratedName = generatedName;
            ToBag = toBag;
            ToChest = toChest;
            Lost = lost;
        }
    }

    /// <summary>Raised by <see cref="DailyLoginService.Claim"/> after a day's reward was given.</summary>
    public readonly struct DailyLoginRewardClaimedEvent : IGameEvent
    {
        public readonly int Day;
        public readonly bool SteppedBack;
        public readonly IReadOnlyList<DailyRewardGrant> Grants;

        public DailyLoginRewardClaimedEvent(int day, bool steppedBack, IReadOnlyList<DailyRewardGrant> grants)
        {
            Day = day;
            SteppedBack = steppedBack;
            Grants = grants ?? Array.Empty<DailyRewardGrant>();
        }
    }

    /// <summary>
    /// "Recompensa diária em sequência": plain-C# <see cref="IManager"/> + <see cref="ISaveable"/> ("DailyLogin" slice,
    /// registered in GameManager like every other per-save system) + <see cref="IResettable"/> (new game → day 1).
    /// Rules in <see cref="DailyLoginRules"/>; the world map's popup (<c>DailyLoginPopup</c>) asks <see cref="Offer"/> and
    /// calls <see cref="Claim"/> with the live bag (a <c>CarriedInventorySession</c>) and the Baú da Vó Zefa.
    ///
    /// Items go in through the bag's shared grid fit API (<see cref="InventoryManager.GetFittableAmount"/> /
    /// <see cref="InventoryManager.TryAddCopy"/>); whatever does not fit goes to the village chest, so a full Mochila
    /// never eats a reward. A small mirror in PlayerPrefs (<see cref="PreviewPrefsKey"/>: last claimed day and date,
    /// written on every claim/load) lets the title screen preview the track before any save is loaded.
    /// </summary>
    public class DailyLoginService : IManager, ISaveable, IResettable
    {
        private const string LogCategory = "Daily";
        public const string PreviewPrefsKey = "Daily.Login.Preview";

        private readonly IDailyClock _clock;
        private readonly Action<string> _previewWriter;
        private DailyLoginState _state = new DailyLoginState();

        /// <param name="clock">Null = the device's local clock.</param>
        /// <param name="previewWriter">Null = PlayerPrefs <see cref="PreviewPrefsKey"/> (tests pass a no-op).</param>
        public DailyLoginService(IDailyClock clock = null, Action<string> previewWriter = null)
        {
            _clock = clock ?? SystemDailyClock.Instance;
            _previewWriter = previewWriter ?? (raw => PlayerPrefs.SetString(PreviewPrefsKey, raw));
        }

        public string SaveKey => "DailyLogin";

        public DailyLoginState Snapshot => _state.Clone();

        public string TodayKey => DailyLoginRules.ToDayKey(_clock.Now);

        public DailyLoginOffer Offer => DailyLoginRules.Evaluate(_state, TodayKey);

        public void Initialize()
        {
        }

        public void Shutdown()
        {
        }

        public void ResetForNewSession()
        {
            _state = new DailyLoginState();
        }

        /// <summary>
        /// Gives today's reward (if any) and records the claim. <paramref name="bag"/>/<paramref name="chest"/> may be
        /// null (then that side holds nothing). Returns the grants, or null when there was nothing to claim.
        /// </summary>
        public List<DailyRewardGrant> Claim(InventoryManager bag, VillageChestStorage chest, Func<string, string> localizeName = null)
        {
            DailyLoginOffer offer = Offer;
            if (!offer.CanClaim)
            {
                return null;
            }

            string today = TodayKey;
            var grants = new List<DailyRewardGrant>();
            foreach (DailyRewardEntry entry in DailyLoginRules.RewardsFor(offer.Day))
            {
                DailyRewardGrant? grant = GrantEntry(entry, bag, chest, today, localizeName);
                if (grant.HasValue)
                {
                    grants.Add(grant.Value);
                }
            }

            DailyLoginRules.Claim(_state, offer, today);
            WritePreview();
            GameLogger.Info(LogCategory, $"Daily login reward: day {offer.Day}{(offer.SteppedBack ? " (stepped back)" : string.Empty)}, {grants.Count} line(s).");
            EventBus.Publish(new DailyLoginRewardClaimedEvent(offer.Day, offer.SteppedBack, grants));
            return grants;
        }

        /// <summary>What <paramref name="entry"/> really gives today: the event invite falls back to gold while it does not exist.</summary>
        public static (ItemDefinition item, int quantity) Resolve(DailyRewardEntry entry, int cyclesCompleted)
        {
            switch (entry.Kind)
            {
                case DailyRewardKind.EventInvite:
                {
                    ItemDefinition invite = ItemDatabase.GetById(entry.ItemId);
                    return invite != null ? (invite, entry.Quantity) : (ItemDatabase.GetById(entry.FallbackItemId), entry.FallbackQuantity);
                }

                case DailyRewardKind.RareGear:
                    return (ItemDatabase.GetById(DailyLoginRules.RareGearBaseFor(cyclesCompleted)), 1);

                default:
                    return (ItemDatabase.GetById(entry.ItemId), entry.Quantity);
            }
        }

        private DailyRewardGrant? GrantEntry(DailyRewardEntry entry, InventoryManager bag, VillageChestStorage chest, string today, Func<string, string> localizeName)
        {
            (ItemDefinition item, int quantity) = Resolve(entry, _state.CyclesCompleted);
            if (item == null || quantity <= 0)
            {
                GameLogger.Warning(LogCategory, $"Daily reward '{entry.ItemId}' skipped: item not in the ItemDatabase.");
                return null;
            }

            if (entry.Kind == DailyRewardKind.RareGear)
            {
                GeneratedItemInstance rolled = ProceduralItemGenerator.Generate(item, ItemRarity.Rare, StableSeed(today), localizeName);
                InventorySlotData piece = ToSlot(rolled);
                if (bag != null && bag.TryAddCopy(piece))
                {
                    return new DailyRewardGrant(item, rolled.GeneratedName, 1, 0, 0);
                }

                int stored = chest != null ? chest.Store(piece, 1) : 0;
                return new DailyRewardGrant(item, rolled.GeneratedName, 0, stored, 1 - stored);
            }

            int fit = bag != null ? bag.GetFittableAmount(item, quantity) : 0;
            int toBag = fit > 0 ? fit - bag.AddItem(item, fit) : 0;
            int rest = quantity - toBag;
            int toChest = rest > 0 && chest != null
                ? chest.Store(new InventorySlotData { itemId = item.ItemId, quantity = rest, currentDurability = item.HasDurability ? item.MaxDurability : 0f }, rest)
                : 0;
            return new DailyRewardGrant(item, null, toBag, toChest, rest - toChest);
        }

        private static InventorySlotData ToSlot(GeneratedItemInstance rolled)
        {
            return new InventorySlotData
            {
                itemId = rolled.BaseItem.ItemId,
                quantity = 1,
                currentDurability = rolled.BaseItem.HasDurability ? rolled.BaseItem.MaxDurability : 0f,
                generatedAffixIds = rolled.AffixIds,
                generatedAffixValues = rolled.AffixValues,
                generatedName = rolled.GeneratedName,
                generatedRarity = rolled.Rarity,
            };
        }

        /// <summary>Same date → same rare roll on every device (FNV-1a of the day key).</summary>
        public static int StableSeed(string dayKey)
        {
            unchecked
            {
                uint hash = 2166136261u;
                foreach (char c in dayKey ?? string.Empty)
                {
                    hash = (hash ^ c) * 16777619u;
                }

                return (int)(hash & 0x7fffffff);
            }
        }

        // ------------------------------------------------------------------
        // Title preview mirror
        // ------------------------------------------------------------------

        /// <summary>"4|2026-10-29": the last claimed day and its date, for the title screen (no save loaded there).</summary>
        public static DailyLoginState ReadPreview()
        {
            string raw = PlayerPrefs.GetString(PreviewPrefsKey, string.Empty);
            return ParsePreview(raw);
        }

        public static DailyLoginState ParsePreview(string raw)
        {
            var state = new DailyLoginState();
            if (string.IsNullOrEmpty(raw))
            {
                return state;
            }

            string[] parts = raw.Split('|');
            if (parts.Length >= 2 && int.TryParse(parts[0], out int day) && DailyLoginRules.TryParseDay(parts[1], out _))
            {
                state.LastClaimedDay = DailyLoginRules.ClampDay(day);
                state.LastClaimDayKey = parts[1];
            }

            return state;
        }

        public static string FormatPreview(DailyLoginState state)
        {
            return state == null || state.LastClaimedDay < 1 ? string.Empty : state.LastClaimedDay + "|" + state.LastClaimDayKey;
        }

        private void WritePreview()
        {
            _previewWriter(FormatPreview(_state));
        }

        // ------------------------------------------------------------------
        // Persistence
        // ------------------------------------------------------------------

        public object CaptureState() => _state.Clone();

        public void RestoreState(object state)
        {
            _state = state is DailyLoginState saved ? saved.Clone() : new DailyLoginState();
            if (_state.LastClaimedDay < 0 || _state.LastClaimedDay > DailyLoginRules.TrackLength || !DailyLoginRules.TryParseDay(_state.LastClaimDayKey, out _))
            {
                _state.LastClaimedDay = 0;
                _state.LastClaimDayKey = null;
            }

            WritePreview();
        }
    }
}
