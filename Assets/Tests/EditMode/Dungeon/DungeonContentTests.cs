using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Dungeon;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.Skills;
using Rapadura.Gameplay.World.Biomes;
using Rapadura.Gameplay.World.Map;
using Rapadura.UI.Dungeon;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rapadura.Tests
{
    /// <summary>
    /// The shipped content of the Ossuário dos Artífices (Docs/Dungeons.md): the five enemy assets (Resources/Dungeons, out of
    /// every region's random table), the catacombs' loot table, the Chave de Osso Rúnica and the Armadura de Marfim
    /// (pt-BR text = <see cref="DungeonTexts"/>), the "Toque Espectral" set bonus, the director's pure helpers (wake, goal,
    /// Colosso phases, payload) and the map button / HUD helpers.
    /// </summary>
    public class DungeonContentTests
    {
        [SetUp]
        public void SetUp()
        {
            ItemDatabase.Invalidate();
        }

        [TearDown]
        public void TearDown()
        {
            ItemDatabase.Invalidate();
        }

        // ------------------------------------------------------------------
        // Enemies
        // ------------------------------------------------------------------

        [Test]
        public void EveryEnemy_HasAnAsset_NamedByItsId()
        {
            foreach (string id in DungeonRules.EnemyIds)
            {
                EnemyDefinition enemy = Resources.Load<EnemyDefinition>(DungeonRules.EnemyResourcePath(id));
                Assert.IsNotNull(enemy, $"Resources/{DungeonRules.EnemyResourcePath(id)}");
                Assert.AreEqual(id, DefeatedEnemyRegistry.GetId(enemy), "the id the save and the bestiary use");
                Assert.AreEqual(DungeonTexts.GetFallback(DungeonTexts.EnemyNameKey(id)), enemy.EnemyName, $"{id}: asset name = DungeonTexts pt-BR");
                Assert.AreEqual(id == DungeonRules.BoneColossusId, enemy.IsBoss, $"{id}: only the Colosso is a boss");
                Assert.AreEqual(0f, enemy.FleeHealthThreshold, $"{id}: the guardians never flee");
            }
        }

        [Test]
        public void EnemyNumbers_FollowTheDesign()
        {
            Assert.AreEqual(40f, Enemy(DungeonRules.FloatingSkullId).MaxHealth);
            Assert.AreEqual(7.5f, Enemy(DungeonRules.FloatingSkullId).ChaseSpeed);
            Assert.AreEqual(180f, Enemy(DungeonRules.SkeletonWarriorId).MaxHealth);
            Assert.AreEqual(3.8f, Enemy(DungeonRules.SkeletonWarriorId).ChaseSpeed);
            Assert.AreEqual(95f, Enemy(DungeonRules.SkeletonArcherId).MaxHealth);
            Assert.AreEqual(DungeonRules.ArcherRange, Enemy(DungeonRules.SkeletonArcherId).AttackRange);
            Assert.AreEqual(220f, Enemy(DungeonRules.BoneSorcererId).MaxHealth);
            Assert.AreEqual(1400f, Enemy(DungeonRules.BoneColossusId).MaxHealth);
            Assert.AreEqual(DungeonRules.BoneHelmItemId, Enemy(DungeonRules.BoneColossusId).BossBonusDropItem.ItemId, "the Colosso drops the helm");
        }

        [Test]
        public void DungeonEnemies_AreNotInAnyRegionTable()
        {
            foreach (EnemyDefinition regional in Resources.LoadAll<EnemyDefinition>(EncounterPayload.EnemyResourcesFolder))
            {
                CollectionAssert.DoesNotContain(DungeonRules.EnemyIds, regional.name);
            }

            foreach (string id in DungeonRules.EnemyIds)
            {
                Assert.IsNull(BiomeRegistry.FindBiomeOf(Enemy(id)), $"{id} must not roll as a random encounter");
            }
        }

        [Test]
        public void EveryRoomGuardian_Resolves_AndKeyCarriersExist()
        {
            foreach (IReadOnlyList<string> room in DungeonDirector.RoomGuardians)
            {
                foreach (string id in room)
                {
                    CollectionAssert.Contains(DungeonRules.EnemyIds, id);
                }
            }

            Assert.AreEqual(DungeonRules.SkeletonWarriorId, DungeonDirector.RoomGuardians[0][DungeonDirector.KeyCarrierIndex[0]]);
            Assert.AreEqual(DungeonRules.BoneSorcererId, DungeonDirector.RoomGuardians[1][DungeonDirector.KeyCarrierIndex[1]]);
            Assert.AreEqual(-1, DungeonDirector.KeyCarrierIndex[2]);
        }

        [Test]
        public void Payload_IsADirectedDungeonOfTheRegulars()
        {
            EncounterPayload payload = DungeonDirector.BuildPayload(42);
            Assert.IsNotNull(payload);
            Assert.IsTrue(payload.IsDungeon);
            Assert.IsTrue(payload.IsDirected);
            Assert.AreEqual(DungeonRules.OssuaryId, payload.dungeonId);
            Assert.AreEqual(DungeonRules.Region, payload.biome);
            Assert.AreEqual(4, payload.EnemyCount, "the four regulars");
            CollectionAssert.DoesNotContain(payload.enemyIds, DungeonRules.BoneColossusId);
        }

        // ------------------------------------------------------------------
        // Loot table and items
        // ------------------------------------------------------------------

        [Test]
        public void CatacombsTable_HoldsTheDungeonItems()
        {
            BiomeDefinition table = DungeonLoot.CatacombsTable;
            Assert.IsNotNull(table);
            var ids = table.Loot.Select(entry => entry.Item != null ? entry.Item.ItemId : null).ToList();
            CollectionAssert.AllItemsAreNotNull(ids);
            CollectionAssert.Contains(ids, DungeonRules.BoneFragmentItemId);
            CollectionAssert.Contains(ids, DungeonRules.HoledCoinItemId);
            CollectionAssert.Contains(ids, DungeonRules.RustySwordItemId);
            CollectionAssert.DoesNotContain(ids, DungeonRules.BoneKeyItemId, "keys come from their carriers only");
            Assert.IsFalse(BiomeRegistry.GetAll().Contains(table), "not a region");

            List<LootDrop> drops = DungeonLoot.RollTable(table, 5, false, new System.Random(3));
            Assert.AreEqual(5, drops.Count, "every roll lands");
        }

        [Test]
        public void BoneKey_IsARareFeatherweightKey_WithoutDurability()
        {
            ItemDefinition key = Item(DungeonRules.BoneKeyItemId);
            Assert.AreEqual(DungeonTexts.KeyNamePt, key.DisplayName);
            Assert.AreEqual(DungeonTexts.KeyDescriptionPt, key.Description);
            Assert.AreEqual(ItemRarity.Rare, key.Rarity);
            Assert.AreEqual(0.1f, key.Weight, 1e-4f);
            Assert.IsFalse(key.HasDurability);
            Assert.IsTrue(key.IsStackable);
            Assert.IsFalse(key.IsEquippable);
        }

        [Test]
        public void IvoryArmour_IsAChestAndAHelm_WithTheirStats()
        {
            ItemDefinition chest = Item(DungeonRules.BoneChestItemId);
            ItemDefinition helm = Item(DungeonRules.BoneHelmItemId);
            Assert.AreEqual(EquipmentSlot.Chest, chest.EquipSlot);
            Assert.AreEqual(EquipmentSlot.Head, helm.EquipSlot);
            Assert.Greater(chest.ArmorDefense, 0f);
            Assert.Greater(helm.ArmorDefense, 0f);
            Assert.IsTrue(chest.EquipStatModifiers.Any(m => m.affectedStat == StatType.CriticalChance && m.value > 0f), "+ critical chance");
            Assert.IsTrue(helm.EquipStatModifiers.Any(m => m.affectedStat == StatType.MaxMana && m.value > 0f), "+ max mana");
            Assert.IsTrue(EnemyLootRoller.IsRolledAsEquipment(chest));
            Assert.IsTrue(EnemyLootRoller.IsRolledAsEquipment(helm));
        }

        /// <summary>
        /// The 35–45 band numbers (Docs/Dungeons.md §6, 2026-11-06; the level-12 version had Defense 14/8 and value 160/120):
        /// Defense x the level-45 defensive gear band (~x1.14), value next to the map-18 Roupa Térmica, materials x3 (the boat
        /// fare of the old band's map 19 -> 24b), the Colosso's fragments 8–12; crit, mana and the set bonus kept.
        /// </summary>
        [Test]
        public void IvoryArmourAndSpoils_UseTheThirtyFiveToFortyFiveNumbers()
        {
            ItemDefinition chest = Item(DungeonRules.BoneChestItemId);
            ItemDefinition helm = Item(DungeonRules.BoneHelmItemId);
            Assert.AreEqual(16f, chest.ArmorDefense, 1e-4f);
            Assert.AreEqual(9f, helm.ArmorDefense, 1e-4f);
            Assert.AreEqual(900, chest.Value);
            Assert.AreEqual(650, helm.Value);
            Assert.AreEqual(0.04f, chest.EquipStatModifiers.Single(m => m.affectedStat == StatType.CriticalChance).value, 1e-6f);
            Assert.AreEqual(20f, helm.EquipStatModifiers.Single(m => m.affectedStat == StatType.MaxMana).value, 1e-4f);

            Assert.AreEqual(18, Item(DungeonRules.BoneFragmentItemId).Value);
            Assert.AreEqual(45, Item(DungeonRules.HoledCoinItemId).Value);
            ItemDefinition sword = Item(DungeonRules.RustySwordItemId);
            Assert.AreEqual(90, sword.Value);
            Assert.AreEqual(16f, sword.WeaponDamage, 1e-4f);
            Assert.Less(sword.WeaponDamage, Resources.Load<ItemDefinition>("Items/item_iron_sword").WeaponDamage, "never the best weapon");

            EnemyDefinition colossus = Enemy(DungeonRules.BoneColossusId);
            Assert.AreEqual(8, colossus.LootQuantityMin);
            Assert.AreEqual(12, colossus.LootQuantityMax);
            Assert.AreEqual(DungeonRules.BoneHelmItemId, colossus.BossBonusDropItem.ItemId);
            Assert.AreEqual(ItemRarity.Epic, colossus.BossBonusDropRarity);
        }

        [Test]
        public void DungeonItems_PtBR_MatchesDungeonTexts()
        {
            var expected = new Dictionary<string, (string name, string description)>
            {
                { DungeonRules.BoneKeyItemId, (DungeonTexts.KeyNamePt, DungeonTexts.KeyDescriptionPt) },
                { DungeonRules.BoneChestItemId, (DungeonTexts.ChestNamePt, DungeonTexts.ChestDescriptionPt) },
                { DungeonRules.BoneHelmItemId, (DungeonTexts.HelmNamePt, DungeonTexts.HelmDescriptionPt) },
                { DungeonRules.BoneFragmentItemId, (DungeonTexts.FragmentNamePt, DungeonTexts.FragmentDescriptionPt) },
                { DungeonRules.HoledCoinItemId, (DungeonTexts.CoinNamePt, DungeonTexts.CoinDescriptionPt) },
                { DungeonRules.RustySwordItemId, (DungeonTexts.SwordNamePt, DungeonTexts.SwordDescriptionPt) },
            };

            foreach (KeyValuePair<string, (string name, string description)> pair in expected)
            {
                ItemDefinition item = Item(pair.Key);
                Assert.AreEqual(pair.Value.name, item.DisplayName, pair.Key);
                Assert.AreEqual(pair.Value.description, item.Description, pair.Key);
                Assert.AreEqual(pair.Value.name, DungeonTexts.GetFallback(DungeonTexts.ItemNameKey(pair.Key)));
                Assert.AreEqual(pair.Value.description, DungeonTexts.GetFallback(DungeonTexts.ItemDescriptionKey(pair.Key)));
            }
        }

        // ------------------------------------------------------------------
        // "Toque Espectral"
        // ------------------------------------------------------------------

        [Test]
        public void SetPieces_CountDistinctBonePieces()
        {
            Assert.AreEqual(0, DungeonRules.CountBoneSetPieces(null));
            Assert.AreEqual(0, DungeonRules.CountBoneSetPieces(new[] { "item_leather_chest", null }));
            Assert.AreEqual(1, DungeonRules.CountBoneSetPieces(new[] { DungeonRules.BoneChestItemId, DungeonRules.BoneChestItemId }));
            Assert.AreEqual(2, DungeonRules.CountBoneSetPieces(new[] { DungeonRules.BoneHelmItemId, "item_iron_sword", DungeonRules.BoneChestItemId }));
        }

        [Test]
        public void SpectralTouch_NeedsBothPieces_ACritical_AndTheFifteenPercent()
        {
            Assert.IsTrue(DungeonRules.SpectralTouchProcs(2, true, 0.0));
            Assert.IsTrue(DungeonRules.SpectralTouchProcs(2, true, 0.149));
            Assert.IsFalse(DungeonRules.SpectralTouchProcs(2, true, 0.15));
            Assert.IsFalse(DungeonRules.SpectralTouchProcs(1, true, 0.0), "one piece is not the set");
            Assert.IsFalse(DungeonRules.SpectralTouchProcs(2, false, 0.0), "only criticals");
        }

        [Test]
        public void SpectralShard_HitsForAThirdOfTheCritical()
        {
            Assert.AreEqual(35f, DungeonRules.SpectralShardDamage(100f), 1e-3f);
            Assert.AreEqual(1f, DungeonRules.SpectralShardDamage(1f), 1e-3f, "never below 1");
            Assert.AreEqual(0f, DungeonRules.SpectralShardDamage(0f));
            Assert.AreEqual(0, SpectralTouchSetBonus.PiecesWorn(null));
        }

        // ------------------------------------------------------------------
        // Director helpers and enemy rules
        // ------------------------------------------------------------------

        [Test]
        public void Rooms_WakeOnlyBehindOpenGates()
        {
            var layout = new DungeonLayout(DungeonLayout.DesiredLength);
            Assert.IsTrue(DungeonDirector.ShouldWake(0, 0f, layout, false, false));
            Assert.IsFalse(DungeonDirector.ShouldWake(1, layout.RoomStart(1), layout, false, false), "gallery gate shut");
            Assert.IsTrue(DungeonDirector.ShouldWake(1, layout.RoomStart(1), layout, true, false));
            Assert.IsFalse(DungeonDirector.ShouldWake(1, 0f, layout, true, false), "hero still far");
            Assert.IsFalse(DungeonDirector.ShouldWake(2, layout.RoomStart(2), layout, true, false), "colossus gate shut");
            Assert.IsTrue(DungeonDirector.ShouldWake(2, layout.RoomStart(2), layout, true, true));
        }

        [Test]
        public void Goal_FollowsTheRun()
        {
            Assert.AreEqual(DungeonTexts.GoalVestibule, DungeonDirector.GoalKey(false, false, 0, 3, false, false));
            Assert.AreEqual(DungeonTexts.GoalGate, DungeonDirector.GoalKey(false, false, 0, 0, false, false));
            Assert.AreEqual(DungeonTexts.GoalGallery, DungeonDirector.GoalKey(false, false, 0, 0, true, false));
            Assert.AreEqual(DungeonTexts.GoalGate, DungeonDirector.GoalKey(false, false, 1, 0, true, false));
            Assert.AreEqual(DungeonTexts.GoalBoss, DungeonDirector.GoalKey(false, true, 1, 0, true, true));
            Assert.AreEqual(DungeonTexts.GoalCleared, DungeonDirector.GoalKey(true, true, 2, 0, true, true));
        }

        [Test]
        public void Colossus_Phases_ByHealth()
        {
            Assert.AreEqual(ColossusPhase.Slam, DungeonRules.ColossusPhaseFor(1f));
            Assert.AreEqual(ColossusPhase.Slam, DungeonRules.ColossusPhaseFor(0.71f));
            Assert.AreEqual(ColossusPhase.Skulls, DungeonRules.ColossusPhaseFor(0.7f));
            Assert.AreEqual(ColossusPhase.Skulls, DungeonRules.ColossusPhaseFor(0.3f));
            Assert.AreEqual(ColossusPhase.Cage, DungeonRules.ColossusPhaseFor(0.29f));
            Assert.AreEqual(DungeonRules.CageChargedDamage, DungeonRules.CageBlowDamage(true));
            Assert.AreEqual(0f, DungeonRules.CageBlowDamage(false), "breaking the cage dodges the blow");
        }

        [Test]
        public void BouncingSkull_ReflectsOffTheWalls()
        {
            float lateral = 0.9f;
            float velocity = 2f;
            Assert.IsTrue(DungeonRules.StepBounce(ref lateral, ref velocity, 0.1f, 1f));
            Assert.AreEqual(0.9f, lateral, 1e-4f);
            Assert.Less(velocity, 0f);
            Assert.IsFalse(DungeonRules.StepBounce(ref lateral, ref velocity, 0.1f, 1f));
        }

        [Test]
        public void Warrior_GuardsTheLastSecondsOfEachCycle_AndReflectsArrows()
        {
            Assert.IsFalse(DungeonRules.IsGuarding(0f));
            Assert.IsFalse(DungeonRules.IsGuarding(3.4f));
            Assert.IsTrue(DungeonRules.IsGuarding(3.6f));
            Assert.IsTrue(DungeonRules.IsGuarding(5.9f));
            Assert.IsFalse(DungeonRules.IsGuarding(6.1f));
            Assert.AreEqual(15f, DungeonRules.GuardReflectDamage(100f, true, true), 1e-3f);
            Assert.AreEqual(0f, DungeonRules.GuardReflectDamage(100f, false, true), "melee is not reflected");
            Assert.AreEqual(0f, DungeonRules.GuardReflectDamage(100f, true, false));
            Assert.IsTrue(DungeonRules.ArcherShouldRetreat(3f));
            Assert.IsFalse(DungeonRules.ArcherShouldRetreat(5f));
        }

        // ------------------------------------------------------------------
        // Entry and UI
        // ------------------------------------------------------------------

        [Test]
        public void Entry_And_Guardians_AreInTheCatacombsBand_35To45()
        {
            // Owner's decision (2026-11-06): the ossuary is map 24b of Docs/MetodologiaMapasERacas.md (35–45), no longer 12.
            Assert.AreEqual(35, DungeonRules.MinLevel);
            Assert.AreEqual(35, DungeonRules.MonsterLevelMin);
            Assert.AreEqual(45, DungeonRules.MonsterLevelMax);
            Assert.IsFalse(DungeonRules.CanEnter(12, BiomeType.Caves), "the old level 12 no longer opens it");
            Assert.IsFalse(DungeonRules.CanEnter(34, BiomeType.Caves));
            Assert.IsTrue(DungeonRules.CanEnter(35, BiomeType.Caves));

            foreach (string id in DungeonRules.EnemyIds)
            {
                int level = DungeonRules.EnemyLevel(id);
                Assert.That(level, Is.InRange(DungeonRules.MonsterLevelMin, DungeonRules.MonsterLevelMax), id);
                Assert.AreEqual(level, Enemy(id).Level, $"{id}: the asset's level = DungeonRules.EnemyLevel");
                Assert.AreEqual(level, EnemyLevelScaling.ResolveLevel(Enemy(id).Level, DungeonRules.EnemyLevel(id)), id);
                Assert.Greater(EnemyLevelScaling.HealthMultiplier(level), 1f, $"{id}: scaled above the archetype level 15");
            }

            Assert.AreEqual(DungeonRules.MonsterLevelMax, DungeonRules.EnemyLevel(DungeonRules.BoneColossusId), "the Colosso is the top of the band");
            Assert.AreEqual(DungeonRules.MonsterLevelMin, DungeonRules.EnemyIds.Min(DungeonRules.EnemyLevel));
            Assert.AreEqual(0, DungeonRules.EnemyLevel("enemy_not_in_the_ossuary"));
        }

        [Test]
        public void Guardian_CarriesItsLevel_ForXpAndThePerigosoRule()
        {
            var go = new GameObject("guardian");
            try
            {
                DungeonEnemyTag.Attach(go, DungeonRules.BoneColossusId, 2, null);
                Assert.IsTrue(go.TryGetComponent(out ICreatureLevelSource source));
                Assert.AreEqual(45, source.CreatureLevel);

                DungeonEnemyTag relit = DungeonEnemyTag.Attach(go, DungeonRules.SkeletonWarriorId, 1, null, reanimated: true);
                Assert.AreEqual(38, source.CreatureLevel);
                Assert.AreEqual(0, relit.ScaleKillExperience(EnemyLevelScaling.KillExperience(38)), "a relit guardian still gives 0 XP");

                // A hero at the entry level (35) against the Colosso (45): +25% crit for the boss, the hero's hits still land.
                Assert.AreEqual(0.25f, Rapadura.Gameplay.Combat.LevelDangerRules.CreatureCritChanceBonus(45 - DungeonRules.MinLevel), 1e-6f);
                Assert.AreEqual(1f, Rapadura.Gameplay.Combat.LevelDangerRules.HeroHitRateMultiplier(45 - DungeonRules.MinLevel));
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void Entry_NeedsTheCavesAndTheLevel()
        {
            Assert.IsTrue(DungeonRules.CanEnter(DungeonRules.MinLevel, BiomeType.Caves));
            Assert.IsFalse(DungeonRules.CanEnter(DungeonRules.MinLevel - 1, BiomeType.Caves));
            Assert.IsFalse(DungeonRules.CanEnter(99, BiomeType.Forest));
            Assert.IsFalse(DungeonRules.CanEnter(99, null));

            Assert.IsTrue(DungeonMapButton.ShowsIn(BiomeType.Caves));
            Assert.IsFalse(DungeonMapButton.ShowsIn(BiomeType.Desert));
            Assert.IsFalse(DungeonMapButton.ShowsIn(null));
            Assert.AreEqual("Entrar: Ossuário dos Artífices", DungeonMapButton.ButtonText(null));
            Assert.AreEqual($"Ossuário dos Artífices abre para heróis de nível {DungeonRules.MinLevel}.", DungeonMapButton.BlockMessage(null, 3, BiomeType.Caves));
            Assert.IsEmpty(DungeonMapButton.BlockMessage(null, DungeonRules.MinLevel, BiomeType.Caves));
        }

        [Test]
        public void Texts_ResolveInEnglishToo()
        {
            var manager = new LocalizationManager();
            manager.Initialize();
            manager.SetLanguage(LanguageCode.en);
            Assert.AreEqual("Ossuary of the Artificers", DungeonTexts.Get(manager, DungeonTexts.DungeonName));
            Assert.AreEqual("Bone keys: 2", DungeonHud.KeysLine(manager, 2));
            manager.SetLanguage(LanguageCode.ptBR);
            Assert.AreEqual("Chaves de osso: 0", DungeonHud.KeysLine(manager, -1));
        }

        [Test]
        public void Hud_IsThemed_AndNeverBlocksInput()
        {
            VisualElement view = DungeonHud.CreateView();
            Assert.IsTrue(view.ClassListContains(DungeonStyles.ThemeClass));
            foreach (string name in new[] { "dg-hud-title", "dg-hud-room", "dg-hud-keys", "dg-hud-goal", "dg-boss", "dg-boss-fill", "dg-banner-text" })
            {
                Assert.IsNotNull(view.Q(name), name);
            }

            view.Query<VisualElement>().ForEach(element => Assert.AreEqual(PickingMode.Ignore, element.pickingMode, element.name));
            Assert.AreEqual(0.5f, DungeonHud.BossFraction(700f, 1400f), 1e-4f);
            Assert.AreEqual(0f, DungeonHud.BossFraction(10f, 0f));
            Assert.IsNotNull(DungeonStyles.Sheet, "Resources/UI/Dungeon.uss");
        }

        [Test]
        public void DungeonCues_AreAddedToTheMapOnce()
        {
            var map = ScriptableObject.CreateInstance<Rapadura.Audio.SfxEventMap>();
            try
            {
                Assert.AreEqual(DungeonFx.AllCues.Length, DungeonFx.EnsureCues(map));
                Assert.AreEqual(0, DungeonFx.EnsureCues(map));
                foreach (string cue in DungeonFx.AllCues)
                {
                    Assert.IsTrue(map.TryGetCue(cue, out _), cue);
                }
            }
            finally
            {
                Object.DestroyImmediate(map);
            }
        }

        private static EnemyDefinition Enemy(string id)
        {
            EnemyDefinition enemy = Resources.Load<EnemyDefinition>(DungeonRules.EnemyResourcePath(id));
            Assert.IsNotNull(enemy, id);
            return enemy;
        }

        private static ItemDefinition Item(string id)
        {
            ItemDefinition item = ItemDatabase.GetById(id);
            Assert.IsNotNull(item, $"{id} is not under Resources/Items");
            return item;
        }
    }
}
