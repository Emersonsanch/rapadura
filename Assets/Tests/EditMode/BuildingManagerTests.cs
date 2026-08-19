using System.Reflection;
using NUnit.Framework;
using Rapadura.Core.EventBus;
using Rapadura.Gameplay.Building;
using Rapadura.Gameplay.Crafting;
using Rapadura.Gameplay.Inventory;
using Rapadura.Gameplay.Items;
using UnityEditor;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// EditMode tests for the Fase 5 "Construção" backend: grid snapping/overlap math in
    /// <see cref="GridPlacementUtility"/> and the placement/upgrade flow in
    /// <see cref="BuildingManager"/>. Deliberately uses only synthetic data (no Scene/Prefab/Collider)
    /// since this project has never been opened in the Unity Editor.
    /// </summary>
    public class BuildingManagerTests
    {
        private GameObject _owner;
        private InventoryManager _inventory;
        private BuildingManager _building;

        private ItemDefinition _wood;
        private ItemDefinition _stone;

        [SetUp]
        public void SetUp()
        {
            EventBus.Clear();

            _owner = new GameObject("BuildOwner");
            _inventory = _owner.AddComponent<InventoryManager>();
            _building = new BuildingManager();
            _building.Initialize();

            _wood = CreateItem("item_wood", 99);
            _stone = CreateItem("item_stone", 99);
        }

        [TearDown]
        public void TearDown()
        {
            EventBus.Clear();
            _building.Shutdown();
            Object.DestroyImmediate(_owner);
            Object.DestroyImmediate(_wood);
            Object.DestroyImmediate(_stone);
        }

        // ---------------------------------------------------------------
        // Grid math (pure functions)
        // ---------------------------------------------------------------

        [Test]
        public void SnapToGrid_RoundsToNearestCell()
        {
            Vector3 snapped = GridPlacementUtility.SnapToGrid(new Vector3(3.6f, 1.23f, -1.2f), cellSize: 2f);

            Assert.AreEqual(4f, snapped.x, 0.0001f);
            Assert.AreEqual(1.23f, snapped.y, 0.0001f); // Y (height) is untouched by grid snapping.
            Assert.AreEqual(-2f, snapped.z, 0.0001f);
        }

        [Test]
        public void GetFootprintBounds_RotationSwapsWidthAndHeight()
        {
            RectInt unrotated = GridPlacementUtility.GetFootprintBounds(new Vector2Int(0, 0), new Vector2Int(2, 1), rotationSteps: 0);
            RectInt rotated = GridPlacementUtility.GetFootprintBounds(new Vector2Int(0, 0), new Vector2Int(2, 1), rotationSteps: 1);

            Assert.AreEqual(2, unrotated.width);
            Assert.AreEqual(1, unrotated.height);
            Assert.AreEqual(1, rotated.width);
            Assert.AreEqual(2, rotated.height);
        }

        [Test]
        public void Overlaps_DetectsOverlappingFootprints()
        {
            var a = new RectInt(0, 0, 2, 2);
            var b = new RectInt(1, 1, 2, 2);
            var c = new RectInt(2, 0, 2, 2); // touches edge of `a`, should not count as overlap

            Assert.IsTrue(GridPlacementUtility.Overlaps(a, b));
            Assert.IsFalse(GridPlacementUtility.Overlaps(a, c));
        }

        // ---------------------------------------------------------------
        // Placement
        // ---------------------------------------------------------------

        [Test]
        public void PlaceStructure_WithEnoughResources_SucceedsAndChargesCost()
        {
            StructureDefinition wall = CreateStructure("structure_wall", new Vector2Int(1, 1), new[] { (_wood, 10) });
            _inventory.AddItem(_wood, 10);

            PlacedStructure placed = _building.PlaceStructure(wall, new Vector2Int(0, 0), rotationSteps: 0, _inventory);

            Assert.IsNotNull(placed);
            Assert.AreEqual(0, _inventory.GetTotalCount("item_wood"));
            Assert.AreEqual(1, _building.PlacedStructures.Count);
        }

        [Test]
        public void PlaceStructure_MissingResources_Fails()
        {
            StructureDefinition wall = CreateStructure("structure_wall", new Vector2Int(1, 1), new[] { (_wood, 10) });
            _inventory.AddItem(_wood, 3);

            PlacedStructure placed = _building.PlaceStructure(wall, new Vector2Int(0, 0), rotationSteps: 0, _inventory);

            Assert.IsNull(placed);
            Assert.AreEqual(3, _inventory.GetTotalCount("item_wood"));
            Assert.AreEqual(0, _building.PlacedStructures.Count);
        }

        [Test]
        public void PlaceStructure_OverlappingExistingStructure_Fails()
        {
            StructureDefinition wall = CreateStructure("structure_wall", new Vector2Int(2, 2), new[] { (_wood, 5) });
            _inventory.AddItem(_wood, 20);

            PlacedStructure first = _building.PlaceStructure(wall, new Vector2Int(0, 0), rotationSteps: 0, _inventory);
            PlacedStructure second = _building.PlaceStructure(wall, new Vector2Int(1, 1), rotationSteps: 0, _inventory);

            Assert.IsNotNull(first);
            Assert.IsNull(second);
            Assert.AreEqual(1, _building.PlacedStructures.Count);
            // Second attempt's cost must not have been charged.
            Assert.AreEqual(10, _inventory.GetTotalCount("item_wood"));
        }

        [Test]
        public void PlaceStructure_AdjacentNonOverlapping_Succeeds()
        {
            StructureDefinition wall = CreateStructure("structure_wall", new Vector2Int(2, 2), new[] { (_wood, 5) });
            _inventory.AddItem(_wood, 20);

            PlacedStructure first = _building.PlaceStructure(wall, new Vector2Int(0, 0), rotationSteps: 0, _inventory);
            PlacedStructure second = _building.PlaceStructure(wall, new Vector2Int(2, 0), rotationSteps: 0, _inventory);

            Assert.IsNotNull(first);
            Assert.IsNotNull(second);
            Assert.AreEqual(2, _building.PlacedStructures.Count);
        }

        // ---------------------------------------------------------------
        // Upgrade
        // ---------------------------------------------------------------

        [Test]
        public void UpgradeStructure_WithEnoughResources_IncrementsLevel()
        {
            StructureDefinition wall = CreateStructureWithLevels(
                "structure_wall",
                new Vector2Int(1, 1),
                new[]
                {
                    (1, new[] { (_wood, 5) }),
                    (2, new[] { (_stone, 5) })
                });

            _inventory.AddItem(_wood, 5);
            _inventory.AddItem(_stone, 5);

            PlacedStructure placed = _building.PlaceStructure(wall, new Vector2Int(0, 0), rotationSteps: 0, _inventory);
            bool upgraded = _building.UpgradeStructure(placed, _inventory);

            Assert.IsNotNull(placed);
            Assert.IsTrue(upgraded);
            Assert.AreEqual(2, placed.currentLevel);
            Assert.AreEqual(0, _inventory.GetTotalCount("item_stone"));
        }

        [Test]
        public void UpgradeStructure_MissingResources_FailsAndKeepsLevel()
        {
            StructureDefinition wall = CreateStructureWithLevels(
                "structure_wall",
                new Vector2Int(1, 1),
                new[]
                {
                    (1, new[] { (_wood, 5) }),
                    (2, new[] { (_stone, 5) })
                });

            _inventory.AddItem(_wood, 5);
            // No stone for the upgrade.

            PlacedStructure placed = _building.PlaceStructure(wall, new Vector2Int(0, 0), rotationSteps: 0, _inventory);
            bool upgraded = _building.UpgradeStructure(placed, _inventory);

            Assert.IsNotNull(placed);
            Assert.IsFalse(upgraded);
            Assert.AreEqual(1, placed.currentLevel);
        }

        [Test]
        public void UpgradeStructure_BeyondMaxLevel_Fails()
        {
            StructureDefinition wall = CreateStructureWithLevels(
                "structure_wall",
                new Vector2Int(1, 1),
                new[] { (1, new[] { (_wood, 1) }) });

            _inventory.AddItem(_wood, 1);
            PlacedStructure placed = _building.PlaceStructure(wall, new Vector2Int(0, 0), rotationSteps: 0, _inventory);

            bool upgraded = _building.UpgradeStructure(placed, _inventory);

            Assert.IsFalse(upgraded);
            Assert.AreEqual(1, placed.currentLevel);
        }

        // ---------------------------------------------------------------
        // Helpers
        // ---------------------------------------------------------------

        private static ItemDefinition CreateItem(string itemId, int maxStack)
        {
            var item = ScriptableObject.CreateInstance<ItemDefinition>();
            var serialized = new SerializedObject(item);
            serialized.FindProperty("_itemId").stringValue = itemId;
            serialized.FindProperty("_displayName").stringValue = itemId;
            serialized.FindProperty("_type").enumValueIndex = (int)ItemType.Material;
            serialized.FindProperty("_maxStack").intValue = maxStack;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return item;
        }

        private static StructureDefinition CreateStructure(string structureId, Vector2Int footprint, (ItemDefinition item, int quantity)[] cost)
        {
            return CreateStructureWithLevels(structureId, footprint, new[] { (1, cost) });
        }

        private static StructureDefinition CreateStructureWithLevels(string structureId, Vector2Int footprint, (int level, (ItemDefinition item, int quantity)[] cost)[] levels)
        {
            var structure = ScriptableObject.CreateInstance<StructureDefinition>();

            var levelData = new StructureLevelData[levels.Length];
            for (int i = 0; i < levels.Length; i++)
            {
                var costArray = new RecipeIngredient[levels[i].cost.Length];
                for (int j = 0; j < levels[i].cost.Length; j++)
                {
                    costArray[j] = new RecipeIngredient { item = levels[i].cost[j].item, quantity = levels[i].cost[j].quantity };
                }

                levelData[i] = new StructureLevelData { level = levels[i].level, cost = costArray };
            }

            SetPrivateField(structure, "_structureId", structureId);
            SetPrivateField(structure, "_displayName", structureId);
            SetPrivateField(structure, "_footprintSize", footprint);
            SetPrivateField(structure, "_levels", levelData);

            return structure;
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(field, $"expected private field '{fieldName}' on {target.GetType().Name}");
            field.SetValue(target, value);
        }
    }
}
