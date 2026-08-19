using System;
using System.Collections.Generic;
using Rapadura.Core.Events;
using Rapadura.Core.Interfaces;
using Rapadura.Gameplay.Crafting;
using Rapadura.Gameplay.Inventory;
using UnityEngine;

namespace Rapadura.Gameplay.Building
{
    /// <summary>
    /// Central runtime authority over placed structures: grid-snapped placement with overlap
    /// validation, resource-cost payment on placement/upgrade, and the structure registry. Like
    /// <see cref="Rapadura.Gameplay.Crafting.CraftingManager"/>, this is a plain <see cref="IManager"/>
    /// (not a MonoBehaviour) registered once in the ServiceLocator by GameManager, and every
    /// operation takes the <see cref="InventoryManager"/> to charge as a parameter rather than
    /// holding a hard reference to one player.
    ///
    /// Placement validation is pure grid math (<see cref="GridPlacementUtility"/>) against the
    /// footprints of already-placed structures — it does not depend on Unity physics/colliders,
    /// so it is fully testable in EditMode with synthetic data, matching the constraint that this
    /// project has never been opened in the Unity Editor (no Scene/Prefab to collide against yet).
    ///
    /// Constructed and registered in <c>GameManager.BuildManagers()</c> alongside the other
    /// top-level managers.
    /// </summary>
    public class BuildingManager : IManager
    {
        [Serializable]
        public class BuildingManagerSaveState
        {
            public PlacedStructure[] structures = Array.Empty<PlacedStructure>();
        }

        private readonly Dictionary<string, StructureDefinition> _structureDatabase = new Dictionary<string, StructureDefinition>();
        private readonly List<PlacedStructure> _placedStructures = new List<PlacedStructure>();

        public float GridCellSize { get; set; } = 1f;

        public IReadOnlyList<PlacedStructure> PlacedStructures => _placedStructures;

        public void Initialize()
        {
        }

        public void Shutdown()
        {
            _placedStructures.Clear();
            _structureDatabase.Clear();
        }

        /// <summary>Registers a structure definition so it can later be resolved by id (e.g. from save data).</summary>
        public void RegisterStructureDefinition(StructureDefinition definition)
        {
            if (definition != null && !string.IsNullOrEmpty(definition.StructureId))
            {
                _structureDatabase[definition.StructureId] = definition;
            }
        }

        public StructureDefinition GetStructureDefinition(string structureId)
        {
            return _structureDatabase.TryGetValue(structureId, out StructureDefinition def) ? def : null;
        }

        // ---------------------------------------------------------------
        // Placement validation
        // ---------------------------------------------------------------

        /// <summary>
        /// True if a structure with <paramref name="footprintSize"/> can occupy the grid footprint
        /// starting at <paramref name="gridOrigin"/> (with rotation) without overlapping any
        /// already-placed structure.
        /// </summary>
        public bool CanPlaceAt(Vector2Int gridOrigin, Vector2Int footprintSize, int rotationSteps)
        {
            RectInt candidate = GridPlacementUtility.GetFootprintBounds(gridOrigin, footprintSize, rotationSteps);

            foreach (PlacedStructure placed in _placedStructures)
            {
                StructureDefinition placedDef = GetStructureDefinition(placed.structureId);
                Vector2Int placedFootprint = placedDef != null ? placedDef.FootprintSize : Vector2Int.one;
                RectInt existing = GridPlacementUtility.GetFootprintBounds(new Vector2Int(placed.gridX, placed.gridY), placedFootprint, placed.rotationSteps);

                if (GridPlacementUtility.Overlaps(candidate, existing))
                {
                    return false;
                }
            }

            return true;
        }

        // ---------------------------------------------------------------
        // Placement
        // ---------------------------------------------------------------

        /// <summary>
        /// Attempts to place a new structure: validates the grid footprint doesn't overlap an
        /// existing one, charges the level-1 cost from <paramref name="inventory"/>, then adds the
        /// structure to the registry. Returns the new <see cref="PlacedStructure"/>, or null if
        /// placement was rejected (overlap or insufficient resources).
        /// </summary>
        public PlacedStructure PlaceStructure(StructureDefinition definition, Vector2Int gridOrigin, int rotationSteps, InventoryManager inventory)
        {
            if (definition == null || inventory == null)
            {
                return null;
            }

            if (!CanPlaceAt(gridOrigin, definition.FootprintSize, rotationSteps))
            {
                return null;
            }

            StructureLevelData levelOneData = definition.GetLevelData(1);

            if (levelOneData == null || !HasResources(levelOneData.cost, inventory))
            {
                return null;
            }

            ChargeCost(levelOneData.cost, inventory);
            RegisterStructureDefinition(definition);

            var structure = new PlacedStructure(Guid.NewGuid().ToString("N"), definition, gridOrigin.x, gridOrigin.y, rotationSteps);
            _placedStructures.Add(structure);

            EventBus.Publish(new StructurePlacedEvent(structure));
            return structure;
        }

        public bool RemoveStructure(PlacedStructure structure)
        {
            if (structure == null || !_placedStructures.Remove(structure))
            {
                return false;
            }

            EventBus.Publish(new StructureRemovedEvent(structure));
            return true;
        }

        // ---------------------------------------------------------------
        // Upgrade
        // ---------------------------------------------------------------

        /// <summary>Attempts to upgrade a placed structure to its next level, charging that level's cost.</summary>
        public bool UpgradeStructure(PlacedStructure structure, InventoryManager inventory)
        {
            if (structure == null || inventory == null)
            {
                return false;
            }

            StructureDefinition definition = GetStructureDefinition(structure.structureId);

            if (definition == null)
            {
                return false;
            }

            int nextLevel = structure.currentLevel + 1;

            if (nextLevel > definition.MaxLevel)
            {
                return false;
            }

            StructureLevelData nextLevelData = definition.GetLevelData(nextLevel);

            if (nextLevelData == null || !HasResources(nextLevelData.cost, inventory))
            {
                return false;
            }

            ChargeCost(nextLevelData.cost, inventory);
            structure.currentLevel = nextLevel;

            EventBus.Publish(new StructureUpgradedEvent(structure, nextLevel));
            return true;
        }

        // ---------------------------------------------------------------
        // Cost helpers
        // ---------------------------------------------------------------

        private static bool HasResources(RecipeIngredient[] cost, InventoryManager inventory)
        {
            if (cost == null)
            {
                return true;
            }

            foreach (RecipeIngredient ingredient in cost)
            {
                if (ingredient.item == null || ingredient.quantity <= 0)
                {
                    continue;
                }

                if (inventory.GetTotalCount(ingredient.item.ItemId) < ingredient.quantity)
                {
                    return false;
                }
            }

            return true;
        }

        private static void ChargeCost(RecipeIngredient[] cost, InventoryManager inventory)
        {
            if (cost == null)
            {
                return;
            }

            foreach (RecipeIngredient ingredient in cost)
            {
                if (ingredient.item == null || ingredient.quantity <= 0)
                {
                    continue;
                }

                inventory.RemoveById(ingredient.item.ItemId, ingredient.quantity);
            }
        }

        // ---------------------------------------------------------------
        // Persistence (plain snapshot helpers — wiring into ISaveable/SaveManager is left to
        // whichever MonoBehaviour host ends up owning world-state save, same as other IManagers
        // that aren't MonoBehaviours don't implement ISaveable themselves).
        // ---------------------------------------------------------------

        public BuildingManagerSaveState CaptureState()
        {
            return new BuildingManagerSaveState { structures = _placedStructures.ToArray() };
        }

        public void RestoreState(BuildingManagerSaveState state)
        {
            _placedStructures.Clear();

            if (state?.structures == null)
            {
                return;
            }

            _placedStructures.AddRange(state.structures);
        }
    }
}
