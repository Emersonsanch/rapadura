using System.Collections.Generic;
using Rapadura.Gameplay.Enemies.Visuals;
using Rapadura.Gameplay.Pets.Visuals;
using UnityEngine;

namespace Rapadura.Gameplay.Mounts.Visuals
{
    /// <summary>
    /// The mounts' code-built models, same pattern as <c>CarameloModel</c>: a <see cref="ProceduralEnemyBlueprint"/>
    /// filled by <see cref="MountRecipe"/>, cached per recipe (meshes shared by every instance) on the enemies' shared
    /// palette material (<see cref="ProceduralEnemyModels.SharedMaterial"/>). Faces +Z, pivot on the ground.
    /// </summary>
    public static class MountModels
    {
        public const string VisualName = "MountVisual";
        public const string MeshNamePrefix = "Mount_";
        public const int TriangleBudget = 2500;

        private static readonly Dictionary<string, ProceduralEnemyBlueprint> Cached = new Dictionary<string, ProceduralEnemyBlueprint>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Cached.Clear();

        /// <summary>A freshly built blueprint (not cached: the caller owns its meshes). Deterministic.</summary>
        public static ProceduralEnemyBlueprint CreateBlueprint(MountVisualRecipe recipe, EnemySwatch? saddle = null, EnemySwatch? blanket = null)
        {
            var blueprint = new ProceduralEnemyBlueprint(ProceduralEnemyKind.None, false, MeshNamePrefix + Key(recipe, saddle, blanket) + "_");
            QuadrupedShape shape = MountRecipe.ShapeFor(recipe);
            if (saddle.HasValue)
            {
                shape.Saddle = saddle.Value;
            }

            if (blanket.HasValue)
            {
                shape.Blanket = blanket.Value;
            }

            MountRecipe.Build(blueprint, shape);
            return blueprint;
        }

        /// <summary>The cached model of a recipe with its saddle/blanket colours (one blueprint per combination worn).</summary>
        public static ProceduralEnemyBlueprint GetBlueprint(MountVisualRecipe recipe, EnemySwatch? saddle = null, EnemySwatch? blanket = null)
        {
            string key = Key(recipe, saddle, blanket);
            if (!Cached.TryGetValue(key, out ProceduralEnemyBlueprint blueprint) || !IsAlive(blueprint))
            {
                blueprint = CreateBlueprint(recipe, saddle, blanket);
                Cached[key] = blueprint;
            }

            return blueprint;
        }

        private static string Key(MountVisualRecipe recipe, EnemySwatch? saddle, EnemySwatch? blanket)
        {
            return recipe + (saddle.HasValue ? "_s" + saddle.Value : string.Empty) + (blanket.HasValue ? "_b" + blanket.Value : string.Empty);
        }

        /// <summary>The model of <paramref name="recipe"/> under <paramref name="parent"/> (named <see cref="VisualName"/>), no colliders.</summary>
        public static GameObject Instantiate(MountVisualRecipe recipe, Transform parent, EnemySwatch? saddle = null, EnemySwatch? blanket = null)
        {
            GameObject visual = CarameloModel.Instantiate(GetBlueprint(recipe, saddle, blanket), ProceduralEnemyModels.SharedMaterial, parent);
            visual.name = VisualName;
            return visual;
        }

        private static bool IsAlive(ProceduralEnemyBlueprint blueprint)
        {
            if (blueprint == null || blueprint.Meshes.Count == 0)
            {
                return false;
            }

            foreach (Mesh mesh in blueprint.Meshes)
            {
                if (mesh == null)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
