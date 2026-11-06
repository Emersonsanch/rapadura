using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Content;
using Rapadura.Gameplay.Inventory;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.Mounts;
using Rapadura.Gameplay.Quests;
using Rapadura.Gameplay.Tacho;
using Rapadura.Gameplay.World.Biomes;
using Rapadura.Gameplay.World.Cities;
using Rapadura.Gameplay.World.Map;
using Rapadura.UI.WorldMap;

namespace Rapadura.Tests
{
    /// <summary>
    /// The special gates of the maps (Docs/MetodologiaMapasERacas.md §2.1, <see cref="MapRequirementRules"/>): every gate
    /// of maps 01–42 has a source the hero can actually get (an item, wings, a mount, a cousin's gift), the items exist with
    /// texts, the ones of regions 1–4 are sold or given in that region's city, and the travel window lets the barca go
    /// once the gear is carried.
    /// </summary>
    public class MapRequirementTests
    {
        private static IEnumerable<string> CatalogTags =>
            MapCatalog.Maps.Where(m => m.HasSpecialRequirement).Select(m => m.SpecialRequirementTag).Distinct();

        /// <summary>A hero with every piece of gear, real wings, a winged and a water mount.</summary>
        private static MapRequirementState Everything()
        {
            var state = new MapRequirementState().Carry(MapRequirementItems.Created).Carry(MapRequirementItems.Pickaxe);
            state.Wear(EquipmentSlot.Boots, MapRequirementItems.RootBoots)
                .Wear(EquipmentSlot.Chest, MapRequirementItems.ThermalClothes)
                .Wear(EquipmentSlot.Wings, WingCatalog.ItemId(WingStyle.Carcara, 1))
                .Own(MountType.Voador)
                .Own(MountType.Aquatico);
            return state;
        }

        /// <summary>The same map, built, so the checks reach the gate.</summary>
        private static MapDefinition Built(int mapId)
        {
            MapDefinition m = MapCatalog.Get(mapId);
            return new MapDefinition(m.MapId, m.Id, m.Region, m.RegionType, m.Size, m.MonsterLevelMin, m.MonsterLevelMax, m.WalkLevelRequirement,
                m.WarpLevelRequirement, m.BaseWarpGoldFee, m.SpecialRequirementTag, m.ReturnCityId, m.Settlement, m.SettlementId, isBuilt: true);
        }

        [Test]
        public void EveryGateOfTheCatalog_HasASource_AndNothingIsMetByAnEmptyHero()
        {
            Assert.IsTrue(MapRequirementRules.IsMet(MapRequirementTags.None, null));
            MapRequirementState everything = Everything();
            var empty = new MapRequirementState();
            foreach (string tag in MapRequirementTags.All)
            {
                Assert.IsTrue(MapRequirementRules.IsMet(tag, everything), $"{tag}: no source can meet it");
                Assert.IsFalse(MapRequirementRules.IsMet(tag, empty), $"{tag}: met by an empty hero");
                Assert.IsFalse(MapRequirementRules.IsMet(tag, null), tag);
            }

            CollectionAssert.IsSubsetOf(CatalogTags.ToList(), MapRequirementTags.All);
            Assert.IsFalse(MapRequirementRules.IsMet("NoSuchGate", everything), "unknown gates stay closed");
        }

        [Test]
        public void Lamp_OpensTheGrottoes_LakeAndTagesHall()
        {
            foreach (int id in new[] { 19, 20 })
            {
                MapDefinition map = Built(id);
                Assert.AreEqual(MapRequirementTags.TorchRequired, map.SpecialRequirementTag);
                var dark = new MapRequirementState(CharacterId.Maria);
                Assert.AreEqual(MapAccessCheck.MissingRequirement,
                    MapAccessRules.CheckWarp(map, 99, CharacterId.Maria, 99999, false, null, false, MapRequirementRules.Lookup(dark)));
                var lit = new MapRequirementState(CharacterId.Maria).Carry(MapRequirementItems.OilLamp);
                Assert.AreEqual(MapAccessCheck.Ok, MapAccessRules.CheckWarp(map, 99, CharacterId.Maria, 99999, false, null, false, MapRequirementRules.Lookup(lit)));
                Assert.AreEqual(MapAccessCheck.Ok, MapAccessRules.CheckWalk(map, 99, CharacterId.Maria, MapRequirementRules.Lookup(lit)));
            }

            Assert.IsTrue(MapRequirementRules.IsMet(MapRequirementTags.TorchRequired, new MapRequirementState().Carry(MapRequirementItems.RunicLamp)), "the runic lamp also lights");
            Assert.IsFalse(MapRequirementRules.IsMet(MapRequirementTags.RunicLamp, new MapRequirementState().Carry(MapRequirementItems.OilLamp)), "but not the other way round");
        }

        [Test]
        public void WoolCloak_OpensTheHahyaPass_AndTheFeltCapeCounts_ButNotWings()
        {
            Assert.AreEqual(MapRequirementTags.WoolCloak, MapCatalog.Get(14).SpecialRequirementTag);
            Assert.IsTrue(MapRequirementRules.IsMet(MapRequirementTags.WoolCloak, new MapRequirementState().Carry(MapRequirementItems.WoolCloak)));
            Assert.IsTrue(MapRequirementRules.IsMet(MapRequirementTags.WoolCloak,
                new MapRequirementState().Wear(EquipmentSlot.Wings, WingCatalog.ItemId(WingStyle.CapaVaqueiro, 2))));
            Assert.IsFalse(MapRequirementRules.IsMet(MapRequirementTags.WoolCloak,
                new MapRequirementState().Wear(EquipmentSlot.Wings, WingCatalog.ItemId(WingStyle.Carcara, 1))));
        }

        [Test]
        public void Wings_AreRealWingsInTheBackSlot_OrAWingedMount()
        {
            foreach (string tag in new[] { MapRequirementTags.WingsOrDoubleJump, MapRequirementTags.WingsRequired })
            {
                Assert.IsTrue(MapRequirementRules.IsMet(tag, new MapRequirementState().Wear(EquipmentSlot.Wings, WingCatalog.ItemId(WingStyle.VagaLume, 3))), tag);
                Assert.IsFalse(MapRequirementRules.IsMet(tag, new MapRequirementState().Wear(EquipmentSlot.Wings, WingCatalog.ItemId(WingStyle.CapaVaqueiro, 3))), "the cape does not fly");
                Assert.IsFalse(MapRequirementRules.IsMet(tag, new MapRequirementState().Carry(WingCatalog.ItemId(WingStyle.Carcara, 1))), "wings in the backpack do not fly");
                Assert.IsTrue(MapRequirementRules.IsMet(tag, new MapRequirementState().Own(MountType.Voador)), tag);
                Assert.IsFalse(MapRequirementRules.IsMet(tag, new MapRequirementState().Own(MountType.Terrestre)), tag);
            }

            // Takhsich's canvas wings give the double jump of the Ninho de Mahaway (24), worn on the back; they glide, they do not fly (39).
            Assert.IsTrue(MapRequirementRules.IsMet(MapRequirementTags.WingsOrDoubleJump, new MapRequirementState().Wear(EquipmentSlot.Wings, MapRequirementItems.CanvasWings)));
            Assert.IsFalse(MapRequirementRules.IsMet(MapRequirementTags.WingsOrDoubleJump, new MapRequirementState().Carry(MapRequirementItems.CanvasWings)), "in the backpack they do not jump");
            Assert.IsFalse(MapRequirementRules.IsMet(MapRequirementTags.WingsRequired, new MapRequirementState().Wear(EquipmentSlot.Wings, MapRequirementItems.CanvasWings)), "canvas does not fly");
            Assert.AreEqual(EquipmentSlot.Wings, ItemDatabase.GetById(MapRequirementItems.CanvasWings).EquipSlot);

            Assert.IsTrue(MapRequirementRules.IsMet(MapRequirementTags.ElevatedPlatforms, new MapRequirementState().Wear(EquipmentSlot.Wings, WingCatalog.ItemId(WingStyle.BeijaFlor, 1))));
            Assert.IsTrue(MapRequirementRules.IsMet(MapRequirementTags.ElevatedPlatforms, new MapRequirementState().Carry(MapRequirementItems.ClimbingRope)));
        }

        [Test]
        public void CousinGifts_WaiveTheirOwnGates_Only()
        {
            Assert.IsTrue(MapRequirementRules.IsMet(MapRequirementTags.LeatherBoots, new MapRequirementState(CharacterId.Joaquim)), "Joaquim does not sink in the mud");
            Assert.IsTrue(MapRequirementRules.IsMet(MapRequirementTags.RootBoots, new MapRequirementState(CharacterId.Joaquim)));
            Assert.IsTrue(MapRequirementRules.IsMet(MapRequirementTags.PoisonProtection, new MapRequirementState(CharacterId.Lavine)), "Lavine breathes the poison mists");
            Assert.IsFalse(MapRequirementRules.IsMet(MapRequirementTags.AntiVenomAmulet, new MapRequirementState(CharacterId.Lavine)), "a bite is not a mist");
            Assert.IsTrue(MapRequirementRules.IsMet(MapRequirementTags.RootBridgeBalance, new MapRequirementState(CharacterId.Maithe)));

            foreach (CharacterId other in new[] { CharacterId.Maria, CharacterId.Icaro })
            {
                Assert.IsFalse(MapRequirementRules.IsMet(MapRequirementTags.LeatherBoots, new MapRequirementState(other)), other.ToString());
                Assert.IsFalse(MapRequirementRules.IsMet(MapRequirementTags.PoisonProtection, new MapRequirementState(other)), other.ToString());
                Assert.IsFalse(MapRequirementRules.IsMet(MapRequirementTags.RootBridgeBalance, new MapRequirementState(other)), other.ToString());
            }
        }

        [Test]
        public void WornGates_NeedTheGearOn_CarriedGatesTheBackpack()
        {
            Assert.IsFalse(MapRequirementRules.IsMet(MapRequirementTags.LeatherBoots, new MapRequirementState().Carry(MapRequirementItems.LeatherBoots)), "boots in the bag do not walk");
            Assert.IsTrue(MapRequirementRules.IsMet(MapRequirementTags.LeatherBoots, new MapRequirementState().Wear(EquipmentSlot.Boots, MapRequirementItems.LeatherBoots)));
            Assert.IsTrue(MapRequirementRules.IsMet(MapRequirementTags.LeatherBoots, new MapRequirementState().Wear(EquipmentSlot.Boots, MapRequirementItems.RootBoots)), "any boots");
            Assert.IsFalse(MapRequirementRules.IsMet(MapRequirementTags.RootBoots, new MapRequirementState().Wear(EquipmentSlot.Boots, MapRequirementItems.LeatherBoots)));
            Assert.IsFalse(MapRequirementRules.IsMet(MapRequirementTags.ThermalClothes, new MapRequirementState().Carry(MapRequirementItems.ThermalClothes)));
            Assert.IsTrue(MapRequirementRules.IsMet(MapRequirementTags.ThermalClothes, new MapRequirementState().Wear(EquipmentSlot.Chest, MapRequirementItems.ThermalClothes)));

            // Pickaxe and torch: both.
            Assert.IsFalse(MapRequirementRules.IsMet(MapRequirementTags.PickaxeTorch, new MapRequirementState().Carry(MapRequirementItems.Pickaxe)));
            Assert.IsFalse(MapRequirementRules.IsMet(MapRequirementTags.PickaxeTorch, new MapRequirementState().Carry(MapRequirementItems.OilLamp)));
            Assert.IsTrue(MapRequirementRules.IsMet(MapRequirementTags.PickaxeTorch,
                new MapRequirementState().Wear(EquipmentSlot.Tool, MapRequirementItems.Pickaxe).Carry(MapRequirementItems.OilLamp)), "a pickaxe in the tool slot counts");
        }

        [Test]
        public void GateItems_ExistInResources_WithTexts()
        {
            var texts = ContentTexts.Entries.ToDictionary(e => e.Key);
            foreach (string id in MapRequirementItems.Created.Concat(new[] { MapRequirementItems.LeatherBoots, MapRequirementItems.Pickaxe }))
            {
                ItemDefinition item = ItemDatabase.GetById(id);
                Assert.IsNotNull(item, $"{id} is not in Resources/Items");
                Assert.IsTrue(texts.TryGetValue(ContentText.ItemNameKey(id), out var name), $"{id}: no name text");
                Assert.AreEqual(item.DisplayName, name.PtBR, id);
                Assert.IsFalse(string.IsNullOrWhiteSpace(name.En), id);
            }

            foreach (string tag in CatalogTags)
            {
                string primary = MapRequirementRules.PrimaryItemOf(tag);
                if (primary != null)
                {
                    Assert.IsNotNull(ItemDatabase.GetById(primary), tag);
                    Assert.IsTrue(MapRequirementRules.IsMet(tag, new MapRequirementState().Carry(primary).Wear(ItemDatabase.GetById(primary).EquipSlot, primary)
                        .Carry(MapRequirementItems.OilLamp)), $"{tag}: its own item does not meet it");
                }
            }

            Assert.AreEqual(EquipmentSlot.Boots, ItemDatabase.GetById(MapRequirementItems.RootBoots).EquipSlot);
            Assert.AreEqual(EquipmentSlot.Chest, ItemDatabase.GetById(MapRequirementItems.ThermalClothes).EquipSlot);
            Assert.IsTrue(ItemDatabase.GetById(MapRequirementItems.GardenKey).IsBound, "a key never falls out on death");
        }

        [Test]
        public void GatesOfRegionsOneToFour_AreSoldOrGivenInTheirRegionsCity()
        {
            foreach (MapDefinition map in MapCatalog.Maps.Where(m => m.HasSpecialRequirement && m.LegacyBiome.HasValue))
            {
                BiomeType biome = map.LegacyBiome.Value;
                var needed = new List<string>();
                string primary = MapRequirementRules.PrimaryItemOf(map.SpecialRequirementTag);
                if (primary != null)
                {
                    needed.Add(primary);
                }

                if (map.SpecialRequirementTag == MapRequirementTags.PickaxeTorch)
                {
                    needed.Add(MapRequirementItems.OilLamp);
                }

                foreach (string itemId in needed)
                {
                    bool sold = CityShopCatalog.Shops.Any(s => s.HomeRegion.HasValue && s.HomeRegion.Value == biome && s.Entries.Any(e => e.IsBagItem && e.ItemId == itemId));
                    bool regionSold = CityShopCatalog.Shops.Any(s => s.HomeRegion.HasValue && s.Entries.Any(e => e.IsBagItem && e.ItemId == itemId));
                    bool given = RegionQuestCatalog.Quests.Any(q => q.Region == biome && q.Rewards.Any(r => r.ItemId == itemId));
                    Assert.IsTrue(sold || given || (map.SpecialRequirementTag == MapRequirementTags.PickaxeTorch && regionSold),
                        $"{map}: {itemId} is neither sold nor given in the {biome} city");
                }
            }

            Assert.IsTrue(RegionQuestCatalog.Quests.Any(q => q.Rewards.Any(r => r.ItemId == MapRequirementItems.OilLamp)), "Parthenos gives the first lamp");
            Assert.IsTrue(RegionQuestCatalog.Quests.Any(q => q.Rewards.Any(r => r.ItemId == MapRequirementItems.WoolCloak)));
            Assert.IsTrue(RegionQuestCatalog.Quests.Any(q => q.Rewards.Any(r => r.ItemId == MapRequirementItems.GardenKey)));
        }

        [Test]
        public void EveryGateOfRegionsOneToNine_HasAnItemKitThatMeetsIt_AndEveryItemIsSoldOrGivenByAQuest()
        {
            // Every region of the catalog (1–8 and the Estepe dos Nartes, região 9, whose Corrente de Amirani 52 asks for crampons).
            var regions = (MapRegion[])System.Enum.GetValues(typeof(MapRegion));

            // The Forte dos Ventos (39) asks for real wings or a flying mount (Tacho Encantado, the Grifo da Coluna): no item kit.
            List<MapDefinition> wingsOnly = MapCatalog.Maps.Where(m => regions.Contains(m.Region) && m.SpecialRequirementTag == MapRequirementTags.WingsRequired).ToList();
            CollectionAssert.AreEqual(new[] { 39 }, wingsOnly.Select(m => m.MapId).ToArray());
            Assert.IsEmpty(MapRequirementRules.KitOf(MapRequirementTags.WingsRequired));

            List<MapDefinition> gated = MapCatalog.Maps.Where(m => m.HasSpecialRequirement && regions.Contains(m.Region) && m.SpecialRequirementTag != MapRequirementTags.WingsRequired).ToList();
            Assert.Greater(gated.Count, 10);
            Assert.IsTrue(gated.Any(m => m.MapId == 36) && gated.Any(m => m.MapId == 38) && gated.Any(m => m.MapId == 40), "the Teto do Mundo's item gates");
            Assert.IsTrue(gated.Any(m => m.MapId == 42 && m.SpecialRequirementTag == MapRequirementTags.SunSeal), "the Planície de Mingcheng's item gate (the Sun Seal)");
            foreach (MapDefinition map in gated.Where(m => m.MapId >= 47 && m.MapId <= 52))
            {
                // Região 9: the gate items of a map are on the counter of its own return city (the Panela Grande for 52).
                CitySpec home = CityCatalog.Get(map.ReturnCityId);
                Assert.IsNotNull(home, $"{map}: its return city {map.ReturnCityId} is a walkable town");
                foreach (string itemId in MapRequirementRules.KitOf(map.SpecialRequirementTag))
                {
                    Assert.IsTrue(home.ShopIds.Any(id => CityShopCatalog.EntryOf(id, itemId) != null), $"{map}: {itemId} is not sold in {home.CityId}");
                }
            }

            foreach (MapDefinition map in gated)
            {
                string[] kit = MapRequirementRules.KitOf(map.SpecialRequirementTag);
                Assert.IsNotEmpty(kit, $"{map}: no item meets {map.SpecialRequirementTag}");
                var state = new MapRequirementState();
                foreach (string itemId in kit)
                {
                    ItemDefinition item = ItemDatabase.GetById(itemId);
                    Assert.IsNotNull(item, $"{map}: {itemId} is not in Resources/Items");
                    state.Carry(itemId).Wear(item.EquipSlot, itemId);

                    bool sold = CityShopCatalog.Shops.Any(s => s.Entries.Any(e => e.IsBagItem && e.ItemId == itemId));
                    bool given = RegionQuestCatalog.Quests.Any(q => q.Rewards.Any(r => r.ItemId == itemId))
                        || CityQuestCatalog.Quests.Any(q => q.Quest.Rewards.Any(r => r.ItemId == itemId));
                    Assert.IsTrue(sold || given, $"{map}: {itemId} is neither sold nor given by a quest");
                }

                Assert.IsTrue(MapRequirementRules.IsMet(map.SpecialRequirementTag, state), $"{map}: its kit does not meet {map.SpecialRequirementTag}");
            }
        }

        [Test]
        public void PureAirBottle_IsTheKobolds_SoldAtMiwnaysFair_AndGivenByTakhsich()
        {
            const string bottle = "item_garrafa_ar_sereno";
            ItemDefinition item = ItemDatabase.GetById(bottle);
            Assert.IsNotNull(item);
            Assert.AreEqual(ItemType.Consumable, item.Type);
            Assert.AreEqual(EquipmentSlot.None, item.EquipSlot);
            Assert.IsTrue(ContentTexts.Entries.Any(e => e.Key == ContentText.ItemNameKey(bottle) && e.PtBR == item.DisplayName));
            Assert.IsNotNull(CityShopCatalog.EntryOf(CityShopCatalog.FeiraMiwnay, bottle));
            Assert.IsTrue(CityQuestCatalog.Quests.Any(q => q.Quest.QuestId == CityQuestCatalog.MercadoWebs && q.Quest.Rewards.Any(r => r.ItemId == bottle)));
        }

        [Test]
        public void HeroState_ReadsTheBackpack_TheWornGear_AndTheMountKinds()
        {
            var slots = new[]
            {
                new InventorySlotData { itemId = MapRequirementItems.OilLamp, quantity = 1 },
                new InventorySlotData { itemId = MapRequirementItems.Waterskin, quantity = 0 },
                null,
                new InventorySlotData(),
            };
            var equipped = new Dictionary<EquipmentSlot, InventorySlotData>
            {
                { EquipmentSlot.Boots, new InventorySlotData { itemId = MapRequirementItems.LeatherBoots, quantity = 1 } },
                { EquipmentSlot.Wings, null },
            };
            var mounts = new[] { new OwnedMount { mountId = MountCatalog.Pony }, new OwnedMount { mountId = "mount_does_not_exist" } };

            MapRequirementState state = MapRequirementHeroState.Capture(slots, equipped, CharacterId.Icaro, mounts);
            Assert.AreEqual(CharacterId.Icaro, state.Hero);
            Assert.IsTrue(state.Has(MapRequirementItems.OilLamp));
            Assert.IsFalse(state.Has(MapRequirementItems.Waterskin), "an empty stack is not carried");
            Assert.AreEqual(MapRequirementItems.LeatherBoots, state.WornIn(EquipmentSlot.Boots));
            Assert.IsNull(state.WornIn(EquipmentSlot.Wings));
            CollectionAssert.AreEquivalent(new[] { MountCatalog.FindSpec(MountCatalog.Pony).Type }, state.OwnedMountTypes);
            Assert.IsTrue(MapRequirementRules.IsMet(MapRequirementTags.TorchRequired, state));
            Assert.IsTrue(MapRequirementRules.IsMet(MapRequirementTags.LeatherBoots, state));
            Assert.IsFalse(MapRequirementRules.IsMet(MapRequirementTags.FreshWater, state));

            MapRequirementState none = MapRequirementHeroState.Capture(null, null, null, null);
            Assert.IsFalse(MapRequirementRules.IsMet(MapRequirementTags.TorchRequired, none));
        }

        [Test]
        public void TravelWindow_SaysWhatIsMissing_AndWhereToGetIt_ThenLetsTheBarcaGo()
        {
            MapDefinition lake = Built(19);
            MapTravelRow dark = FastTravelWindowController.BuildMapRow(lake, 20, CharacterId.Maithe, 9999, false, null, false,
                MapRequirementRules.Lookup(new MapRequirementState(CharacterId.Maithe)), null);
            Assert.AreEqual(MapAccessCheck.MissingRequirement, dark.Check);
            Assert.IsTrue(dark.IsLocked);
            Assert.AreEqual("Exige: Lamparina acesa", dark.Status);
            StringAssert.Contains("Lamparina de Óleo", dark.Hint);
            StringAssert.Contains("Vila das Lamparinas", dark.Hint);

            MapTravelRow lit = FastTravelWindowController.BuildMapRow(lake, 20, CharacterId.Maithe, 9999, false, null, false,
                MapRequirementRules.Lookup(new MapRequirementState(CharacterId.Maithe).Carry(MapRequirementItems.OilLamp)), null);
            Assert.IsTrue(lit.CanTravel);

            foreach (string tag in MapRequirementTags.All)
            {
                Assert.IsNotNull(MapTexts.GetFallback(MapTexts.RequirementHintKey(tag)), $"{tag}: no hint");
            }
        }

        [Test]
        public void Ibex_DoubleJump_DoesNotOpenTheNinhoDeMahaway()
        {
            // Owner's decision (2026-11-06): the Walia Ibex's "Pulo Duplo" (70 gold at the Mountain forge) would bypass the
            // wings gate of map 24 — only real wings, a flying mount or the canvas wings worn on the back open it.
            MapDefinition ninho = MapCatalog.Get(24);
            Assert.AreEqual(MapRequirementTags.WingsOrDoubleJump, ninho.SpecialRequirementTag);

            MountSpec ibex = MountCatalog.FindSpec(MountWaveCatalog.MountainGoat);
            Assert.IsNotNull(ibex);
            Assert.AreEqual(MountAbilityKind.DoubleJump, ibex.AbilityKind, "the ibex really has a double jump");
            Assert.AreNotEqual(MountType.Voador, ibex.Type);

            var slots = new[] { new InventorySlotData { itemId = ibex.WhistleItemId, quantity = 1 } };
            var mounts = new[] { new OwnedMount { mountId = MountWaveCatalog.MountainGoat } };
            MapRequirementState withIbex = MapRequirementHeroState.Capture(slots, null, null, mounts);
            Assert.IsFalse(MapRequirementRules.IsMet(ninho.SpecialRequirementTag, withIbex), "the ibex's double jump is not wings");
            Assert.AreEqual(MapAccessCheck.MissingRequirement,
                MapAccessRules.CheckWalk(ninho, 99, null, MapRequirementRules.Lookup(withIbex)));

            withIbex.Wear(EquipmentSlot.Wings, MapRequirementItems.CanvasWings);
            Assert.IsTrue(MapRequirementRules.IsMet(ninho.SpecialRequirementTag, withIbex), "the wings still open it");
        }
    }
}
