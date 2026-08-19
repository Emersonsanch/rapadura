using System;

namespace Rapadura.Gameplay.Building
{
    /// <summary>Runtime record of one structure that has been placed in the world.</summary>
    [Serializable]
    public class PlacedStructure
    {
        public string instanceId;
        public string structureId;
        public int gridX;
        public int gridY;

        /// <summary>0..3, each step is a 90 degree rotation.</summary>
        public int rotationSteps;
        public int currentLevel = 1;

        public PlacedStructure()
        {
        }

        public PlacedStructure(string instanceId, StructureDefinition definition, int gridX, int gridY, int rotationSteps)
        {
            this.instanceId = instanceId;
            structureId = definition != null ? definition.StructureId : null;
            this.gridX = gridX;
            this.gridY = gridY;
            this.rotationSteps = rotationSteps;
            currentLevel = 1;
        }
    }
}
