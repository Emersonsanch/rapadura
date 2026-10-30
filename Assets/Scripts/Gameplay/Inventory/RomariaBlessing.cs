using System;
using System.Collections.Generic;
using System.Linq;
using Rapadura.Core.DI;
using Rapadura.Core.Events;
using Rapadura.Core.Interfaces;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.World.Cities;
using Rapadura.Gameplay.World.Map;
using UnityEngine;

namespace Rapadura.Gameplay.Inventory
{
    /// <summary>Price of the Bênção da Romaria (Docs/Balance/README.md, "Perda de XP, Bênção da Romaria e Patuá").</summary>
    public static class RomariaBlessingRules
    {
        public const string CurrencyItemId = "item_gold";

        /// <summary>Gold at level 0; each level adds <see cref="PricePerLevel"/>.</summary>
        public const int BasePrice = 5;
        public const int PricePerLevel = 1;
        public const int MaxPrice = 60;

        /// <summary>5 + level gold, capped at 60: ~3–8 fights of gold (~1.8 per fight) in the first campaign.</summary>
        public static int Price(int level) => Math.Min(MaxPrice, BasePrice + PricePerLevel * Math.Max(1, level));
    }

    /// <summary>"Patuá da Vó Zefa": the consumable that cancels the whole death penalty (a 1×1 bag item).</summary>
    public static class PatuaRules
    {
        public const string ItemId = "item_patua_vo_zefa";

        /// <summary>Price at the Mascate (rotating, after the Floresta boss): ~67 fights of gold. Item value 100.</summary>
        public const int MascatePrice = 120;
    }

    /// <summary>Outcome of <see cref="RomariaOratory.TryBuy"/>.</summary>
    public enum BlessingPurchase
    {
        Ok,
        AlreadyBlessed,
        NotEnoughGold,
        Invalid,
    }

    /// <summary>
    /// The small oratório of each city where the Bênção da Romaria is asked for, and who keeps it. Additive city data
    /// keyed by <see cref="CityCatalog"/> ids (the keepers are the city's residents, except Dona Fé, the pilgrim who
    /// keeps the Vila's capelinha).
    /// </summary>
    public sealed class OratorySpec
    {
        public readonly string CityId;

        /// <summary>A <see cref="CityCatalog"/> resident id, or null for Dona Fé (<see cref="DeathProtectionTexts.DonaFeName"/>).</summary>
        public readonly string KeeperResidentId;

        public OratorySpec(string cityId, string keeperResidentId)
        {
            CityId = cityId;
            KeeperResidentId = keeperResidentId;
        }

        public string NameKey => "romaria.oratory." + CityId + ".name";
    }

    public static class RomariaOratory
    {
        public static readonly IReadOnlyList<OratorySpec> Oratories = new List<OratorySpec>
        {
            new OratorySpec(CityCatalog.VilaZefa, null),
            new OratorySpec(CityCatalog.AreiaBranca, "socorro"),
            new OratorySpec(CityCatalog.SerraAlta, "graca"),
            new OratorySpec(CityCatalog.Lamparinas, "benedita"),
        };

        public static OratorySpec ForCity(string cityId) => Oratories.FirstOrDefault(o => o.CityId == cityId);

        /// <summary>Pays <see cref="RomariaBlessingRules.Price"/> from <paramref name="bag"/> and blesses. Nothing changes unless it all works.</summary>
        public static BlessingPurchase TryBuy(InventoryManager bag, DeathProtectionState state, int level)
        {
            if (bag == null || state == null)
            {
                return BlessingPurchase.Invalid;
            }

            if (state.IsBlessed)
            {
                return BlessingPurchase.AlreadyBlessed;
            }

            int price = RomariaBlessingRules.Price(level);
            if (bag.GetTotalCount(RomariaBlessingRules.CurrencyItemId) < price || !bag.RemoveById(RomariaBlessingRules.CurrencyItemId, price))
            {
                return BlessingPurchase.NotEnoughGold;
            }

            state.Bless();
            return BlessingPurchase.Ok;
        }
    }

    /// <summary>Raised when the Bênção starts or ends (HUD icon).</summary>
    public readonly struct RomariaBlessingChangedEvent : IGameEvent
    {
        public readonly bool Blessed;

        public RomariaBlessingChangedEvent(bool blessed)
        {
            Blessed = blessed;
        }
    }

    /// <summary>Raised when Vó Zefa leaves a Patuá for the cousins (<see cref="InChest"/>: it went to her Baú, bag full).</summary>
    public readonly struct PatuaGiftedEvent : IGameEvent
    {
        public readonly bool InChest;

        public PatuaGiftedEvent(bool inChest)
        {
            InChest = inChest;
        }
    }

    /// <summary>JSON snapshot of <see cref="DeathProtectionState"/>.</summary>
    [Serializable]
    public class DeathProtectionSaveState
    {
        public bool blessed;
        public int blessingsBought;
        public int patuasGifted;
    }

    /// <summary>
    /// Per-save death-protection state: whether the Bênção da Romaria is on (it lasts until the next death) and a few
    /// counters. Built by GameManager, registered with SaveManager under <see cref="Key"/> and with NewSessionReset (old
    /// saves have no entry and load unblessed). Also hands out Vó Zefa's reward: the first time each region's boss
    /// falls (<see cref="WorldMapRegionClearedEvent"/>, once per campaign cycle) she gives one Patuá — into the hero's
    /// bag when it fits (<see cref="InventoryManager.CanFit"/>, 1×1), else into her Baú, so it is never lost.
    /// </summary>
    public class DeathProtectionState : IManager, ISaveable, IResettable
    {
        public const string Key = "DeathProtection";
        private const string PlayerTag = "Player";

        private static DeathProtectionState _standalone;
        private bool _subscribed;

        public string SaveKey => Key;

        public bool IsBlessed { get; private set; }
        public int BlessingsBought { get; private set; }
        public int PatuasGifted { get; private set; }

        /// <summary>The GameManager's state, or a process-wide unsaved one when a scene is played on its own.</summary>
        public static DeathProtectionState ResolveShared()
        {
            if (ServiceLocator.TryGet(out DeathProtectionState state) && state != null)
            {
                return state;
            }

            return _standalone ??= new DeathProtectionState();
        }

        public void Initialize()
        {
            if (!_subscribed)
            {
                _subscribed = true;
                EventBus.Subscribe<WorldMapRegionClearedEvent>(OnRegionCleared);
            }
        }

        public void Shutdown()
        {
            if (_subscribed)
            {
                _subscribed = false;
                EventBus.Unsubscribe<WorldMapRegionClearedEvent>(OnRegionCleared);
            }
        }

        public void Bless()
        {
            if (IsBlessed)
            {
                return;
            }

            IsBlessed = true;
            BlessingsBought++;
            EventBus.Publish(new RomariaBlessingChangedEvent(true));
        }

        public void ConsumeBlessing()
        {
            if (!IsBlessed)
            {
                return;
            }

            IsBlessed = false;
            EventBus.Publish(new RomariaBlessingChangedEvent(false));
        }

        public void ResetForNewSession()
        {
            bool was = IsBlessed;
            IsBlessed = false;
            BlessingsBought = 0;
            PatuasGifted = 0;
            if (was)
            {
                EventBus.Publish(new RomariaBlessingChangedEvent(false));
            }
        }

        // ---------------------------------------------------------------
        // Vó Zefa's Patuá (region boss reward)
        // ---------------------------------------------------------------

        private void OnRegionCleared(WorldMapRegionClearedEvent evt)
        {
            GameObject player = GameObject.FindGameObjectWithTag(PlayerTag);
            InventoryManager bag = player != null ? player.GetComponent<InventoryManager>() : null;
            GiftPatua(bag, VillageChestStorage.ResolveShared());
        }

        /// <summary>One Patuá into <paramref name="bag"/> when it fits, else into the chest (its inbox when full). False if the item is missing.</summary>
        public bool GiftPatua(InventoryManager bag, VillageChestStorage chest, Func<string, ItemDefinition> resolve = null)
        {
            ItemDefinition patua = (resolve ?? ItemDatabase.GetById)(PatuaRules.ItemId);
            if (patua == null)
            {
                return false;
            }

            bool inChest;
            if (bag != null && bag.CanFit(patua, 1) && bag.AddItem(patua, 1) == 0)
            {
                inChest = false;
            }
            else if (chest != null)
            {
                var slot = new InventorySlotData { itemId = patua.ItemId, quantity = 1 };
                if (chest.Store(slot, 1) < 1)
                {
                    chest.EnqueueOverflow(new InventorySlotData { itemId = patua.ItemId, quantity = 1 });
                }

                inChest = true;
            }
            else
            {
                return false;
            }

            PatuasGifted++;
            EventBus.Publish(new PatuaGiftedEvent(inChest));
            return true;
        }

        // ---------------------------------------------------------------
        // ISaveable
        // ---------------------------------------------------------------

        public object CaptureState()
        {
            return new DeathProtectionSaveState { blessed = IsBlessed, blessingsBought = BlessingsBought, patuasGifted = PatuasGifted };
        }

        public void RestoreState(object state)
        {
            if (state is not DeathProtectionSaveState saved)
            {
                return;
            }

            bool was = IsBlessed;
            IsBlessed = saved.blessed;
            BlessingsBought = Math.Max(0, saved.blessingsBought);
            PatuasGifted = Math.Max(0, saved.patuasGifted);
            if (was != IsBlessed)
            {
                EventBus.Publish(new RomariaBlessingChangedEvent(IsBlessed));
            }
        }
    }
}
