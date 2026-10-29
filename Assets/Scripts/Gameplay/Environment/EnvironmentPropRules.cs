using Rapadura.Gameplay.Environment.Wind;
using Rapadura.Gameplay.Environment.Impact;

namespace Rapadura.Gameplay.Environment
{
    /// <summary>
    /// Pure classification of scene props by how they were authored (the scatter tools and
    /// <c>BiomeArenaPropFactory</c> name everything by convention), so
    /// <see cref="EnvironmentPropAttacher"/> can add wind and ground-impact behaviour at load time
    /// without the scenes/prefabs carrying components by hand. EditMode-tested.
    /// </summary>
    public static class EnvironmentPropRules
    {
        public const string GrassTuftMeshName = "GrassTuft";

        /// <summary>Which wind sway (if any) an object gets.</summary>
        public static WindSwayKind ClassifyWind(string objectName, string meshName, bool hasLodGroup)
        {
            if (string.IsNullOrEmpty(objectName))
            {
                return WindSwayKind.None;
            }

            if (hasLodGroup && objectName.StartsWith("Tree_", System.StringComparison.Ordinal))
            {
                return WindSwayKind.Tree;
            }

            if (meshName == GrassTuftMeshName)
            {
                return WindSwayKind.Grass;
            }

            if (objectName.StartsWith("ArenaProp_Cactus", System.StringComparison.Ordinal))
            {
                return WindSwayKind.Shrub;
            }

            return WindSwayKind.None;
        }

        /// <summary>Which ground-impact reaction (if any) an object gets.</summary>
        public static GroundImpactKind ClassifyImpact(string objectName)
        {
            if (string.IsNullOrEmpty(objectName))
            {
                return GroundImpactKind.None;
            }

            if (objectName.StartsWith("Pebble_", System.StringComparison.Ordinal) ||
                objectName.StartsWith("Twig_", System.StringComparison.Ordinal))
            {
                return GroundImpactKind.Pebble;
            }

            if (objectName.StartsWith("ProceduralRock_", System.StringComparison.Ordinal) ||
                objectName.StartsWith("Rock_", System.StringComparison.Ordinal) ||
                objectName == "Rock" ||
                objectName.StartsWith("ArenaProp_SnowyRock", System.StringComparison.Ordinal) ||
                objectName.StartsWith("ArenaProp_Stalagmite", System.StringComparison.Ordinal))
            {
                return GroundImpactKind.Boulder;
            }

            return GroundImpactKind.None;
        }
    }
}
