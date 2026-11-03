using System.Collections.Generic;
using NUnit.Framework;
using Rapadura.Core.Events;
using Rapadura.Gameplay.Dungeon;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.Skills;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// The ossuary's props: the cracked bone wall (blunt impacts bring it down at once, plain hits after
    /// <see cref="DungeonRules.WallHitsToBreak"/>), the sarcophagi (70% loot / 30% ambush of two floating skulls, once per
    /// save) and the niche's legendary chest (its fixed Armadura Peitoral de Costelas, at least Epic).
    /// </summary>
    public class DungeonPropsTests
    {
        private readonly List<Object> _created = new List<Object>();

        [SetUp]
        public void SetUp()
        {
            EventBus.Clear();
            ItemDatabase.Invalidate();
        }

        [TearDown]
        public void TearDown()
        {
            foreach (Object obj in _created)
            {
                if (obj != null)
                {
                    Object.DestroyImmediate(obj);
                }
            }

            _created.Clear();
            EventBus.Clear();
        }

        // ------------------------------------------------------------------
        // Bone wall
        // ------------------------------------------------------------------

        [Test]
        public void BluntImpact_IsEarthAreaOrAnExplosive()
        {
            Assert.IsTrue(DungeonRules.IsBluntImpact("skill_joaquim_maremoto", ElementType.Earth, 4f));
            Assert.IsTrue(DungeonRules.IsBluntImpact("skill_icaro_granada_eletrica", ElementType.Lightning, 2f));
            Assert.IsTrue(DungeonRules.IsBluntImpact("skill_explosion", ElementType.Fire, 3f));
            Assert.IsFalse(DungeonRules.IsBluntImpact("skill_earth_single", ElementType.Earth, 0f), "single-target earth is not an impact");
            Assert.IsFalse(DungeonRules.IsBluntImpact("skill_fireball", ElementType.Fire, 3f));
        }

        [Test]
        public void Wall_BreaksAtOnce_UnderABluntImpactInReach()
        {
            var progress = new DungeonProgress();
            BreakableBoneWall wall = NewWall(progress);
            int broken = 0;
            wall.Broken += _ => broken++;

            Assert.IsFalse(wall.TryBluntImpact("skill_fireball", ElementType.Fire, 3f, Vector3.zero), "not blunt");
            Assert.IsFalse(wall.TryBluntImpact("skill_joaquim_maremoto", ElementType.Earth, 3f, new Vector3(10f, 0f, 0f)), "too far");
            Assert.IsFalse(wall.IsBroken);

            Assert.IsTrue(wall.TryBluntImpact("skill_joaquim_maremoto", ElementType.Earth, 3f, new Vector3(4f, 0f, 0f)));
            Assert.IsTrue(wall.IsBroken);
            Assert.IsTrue(progress.IsWallBroken(DungeonRules.BoneWallId));
            Assert.IsFalse(wall.IsAlive);
            Assert.AreEqual(1, broken);

            wall.Break();
            Assert.AreEqual(1, broken, "idempotent");
        }

        [Test]
        public void Wall_CrumblesAfterEnoughPlainHits()
        {
            BreakableBoneWall wall = NewWall(new DungeonProgress());
            for (int i = 0; i < DungeonRules.WallHitsToBreak - 1; i++)
            {
                wall.ApplyDamage(5f, ElementType.Physical, null);
            }

            Assert.IsFalse(wall.IsBroken);
            wall.ApplyDamage(0f, ElementType.Physical, null);
            Assert.IsFalse(wall.IsBroken, "a zero hit does not count");
            wall.ApplyDamage(5f, ElementType.Physical, null);
            Assert.IsTrue(wall.IsBroken);
        }

        [Test]
        public void Wall_LoadsBroken_FromTheSave()
        {
            var progress = new DungeonProgress();
            progress.MarkWallBroken(DungeonRules.BoneWallId);
            BreakableBoneWall wall = NewWall(progress);
            Assert.IsTrue(wall.IsBroken);
            foreach (Collider body in wall.GetComponentsInChildren<Collider>(true))
            {
                Assert.IsFalse(body.enabled, "the passage is free");
            }
        }

        // ------------------------------------------------------------------
        // Sarcophagi and chest
        // ------------------------------------------------------------------

        [Test]
        public void SarcophagusRoll_IsSeventyThirty()
        {
            Assert.AreEqual(SarcophagusOutcome.Loot, DungeonRules.RollSarcophagus(0.0));
            Assert.AreEqual(SarcophagusOutcome.Loot, DungeonRules.RollSarcophagus(0.69));
            Assert.AreEqual(SarcophagusOutcome.Ambush, DungeonRules.RollSarcophagus(0.7));
            Assert.AreEqual(SarcophagusOutcome.Ambush, DungeonRules.RollSarcophagus(0.99));
            Assert.AreEqual(2, DungeonRules.SarcophagusAmbushSkulls);
        }

        [Test]
        public void Sarcophagus_Loot_RollsTheCatacombsTable_OnlyOnce()
        {
            var progress = new DungeonProgress();
            SarcophagusInteractable sarcophagus = NewSarcophagus(progress, DungeonRules.SarcophagusIds[0], false, null);
            sarcophagus.Random = new System.Random(SeedWhereFirstRoll(below: true));
            int ambushes = 0;
            sarcophagus.AmbushRequested = (_, __) => ambushes++;

            SarcophagusOutcome? outcome = sarcophagus.Open(null, out List<LootDrop> drops);

            Assert.AreEqual(SarcophagusOutcome.Loot, outcome);
            Assert.AreEqual(DungeonRules.SarcophagusLootRolls, drops.Count);
            Assert.AreEqual(0, ambushes);
            Assert.IsTrue(progress.IsSarcophagusOpened(DungeonRules.SarcophagusIds[0]));
            Assert.IsNull(sarcophagus.Open(null, out _), "opens once per save");
            Assert.IsFalse(sarcophagus.CanInteract);
        }

        [Test]
        public void Sarcophagus_Ambush_AsksForTwoFloatingSkulls()
        {
            SarcophagusInteractable sarcophagus = NewSarcophagus(new DungeonProgress(), DungeonRules.SarcophagusIds[1], false, null);
            sarcophagus.Random = new System.Random(SeedWhereFirstRoll(below: false));
            int skulls = 0;
            sarcophagus.AmbushRequested = (_, count) => skulls += count;

            SarcophagusOutcome? outcome = sarcophagus.Open(null, out List<LootDrop> drops);

            Assert.AreEqual(SarcophagusOutcome.Ambush, outcome);
            Assert.IsEmpty(drops);
            Assert.AreEqual(DungeonRules.SarcophagusAmbushSkulls, skulls);
        }

        [Test]
        public void LegendaryChest_GivesTheRibBreastplate_AtLeastEpic_PlusTableRolls()
        {
            var progress = new DungeonProgress();
            SarcophagusInteractable chest = NewSarcophagus(progress, DungeonRules.LegendaryChestId, true, DungeonRules.BoneChestItemId);
            chest.Random = new System.Random(7);

            SarcophagusOutcome? outcome = chest.Open(null, out List<LootDrop> drops);

            Assert.AreEqual(SarcophagusOutcome.Loot, outcome, "the chest never ambushes");
            Assert.AreEqual(1 + DungeonRules.LegendaryChestRolls, drops.Count);
            Assert.IsTrue(drops[0].IsGenerated);
            Assert.AreEqual(DungeonRules.BoneChestItemId, drops[0].Generated.BaseItem.ItemId);
            Assert.GreaterOrEqual(drops[0].Generated.Rarity, ItemRarity.Epic);
            Assert.IsTrue(progress.IsChestTaken(DungeonRules.LegendaryChestId));
            Assert.IsNull(chest.Open(null, out _));
        }

        [Test]
        public void ClearingTheOssuary_RefillsSarcophagi_AndMendsTheWall()
        {
            var progress = new DungeonProgress();
            progress.MarkSarcophagusOpened(DungeonRules.SarcophagusIds[0]);
            progress.MarkWallBroken(DungeonRules.BoneWallId);
            progress.MarkChestTaken(DungeonRules.LegendaryChestId);

            progress.RecordClear();

            Assert.IsFalse(progress.IsSarcophagusOpened(DungeonRules.SarcophagusIds[0]));
            Assert.IsFalse(progress.IsWallBroken(DungeonRules.BoneWallId));
            Assert.IsFalse(progress.IsChestTaken(DungeonRules.LegendaryChestId));
        }

        private static int SeedWhereFirstRoll(bool below)
        {
            for (int seed = 0; seed < 1000; seed++)
            {
                bool loot = new System.Random(seed).NextDouble() < DungeonRules.SarcophagusLootChance;
                if (loot == below)
                {
                    return seed;
                }
            }

            Assert.Fail("no seed found");
            return 0;
        }

        private BreakableBoneWall NewWall(DungeonProgress progress)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = "DungeonPropsTests_Wall";
            _created.Add(go);
            BreakableBoneWall wall = go.AddComponent<BreakableBoneWall>();
            wall.Progress = progress;
            wall.Configure(DungeonRules.BoneWallId);
            return wall;
        }

        private SarcophagusInteractable NewSarcophagus(DungeonProgress progress, string id, bool legendary, string guaranteed)
        {
            var go = new GameObject("DungeonPropsTests_Sarcophagus");
            _created.Add(go);
            SarcophagusInteractable sarcophagus = go.AddComponent<SarcophagusInteractable>();
            sarcophagus.Progress = progress;
            sarcophagus.Configure(id, null, null, legendary, guaranteed);
            return sarcophagus;
        }
    }
}
