using Rapadura.Gameplay.Enemies.Visuals;
using UnityEngine;

namespace Rapadura.Gameplay.Pets.Visuals
{
    /// <summary>
    /// Caramelo's code-built model, same pattern as <see cref="ProceduralEnemyModels"/>: a
    /// <see cref="ProceduralEnemyBlueprint"/> filled by <see cref="CarameloRecipe"/>, cached once (meshes
    /// shared by every instance) on the enemies' shared palette material. <see cref="Instantiate"/> prefers
    /// the baked prefab at <c>Resources/Pets/Caramelo</c> (editor menu "Rapadura/Pets/Build Caramelo Model",
    /// which also puts the palette material in builds) and builds the same hierarchy from code otherwise,
    /// so baking is optional. Faces +Z, pivot at the feet, in metres.
    /// </summary>
    public static class CarameloModel
    {
        public const string VisualName = "CarameloVisual";
        public const string PrefabResourcePath = "Pets/Caramelo";
        public const string MeshNamePrefix = "Pet_Caramelo_";
        public const int TriangleBudget = 1500;

        private static ProceduralEnemyBlueprint _cached;

        /// <summary>A freshly built blueprint (not cached: the caller owns its meshes). Deterministic.</summary>
        public static ProceduralEnemyBlueprint CreateBlueprint()
        {
            var blueprint = new ProceduralEnemyBlueprint(ProceduralEnemyKind.None, false, MeshNamePrefix);
            CarameloRecipe.Build(blueprint);
            return blueprint;
        }

        /// <summary>The cached, shared blueprint (rebuilt if its meshes were destroyed).</summary>
        public static ProceduralEnemyBlueprint GetBlueprint()
        {
            if (_cached == null || !IsAlive(_cached))
            {
                _cached = CreateBlueprint();
            }

            return _cached;
        }

        /// <summary>The baked prefab when present, else the code-built model, under <paramref name="parent"/> (named <see cref="VisualName"/>).</summary>
        public static GameObject Instantiate(Transform parent)
        {
            var baked = Resources.Load<GameObject>(PrefabResourcePath);
            GameObject visual = baked != null
                ? Object.Instantiate(baked, parent, false)
                : Instantiate(GetBlueprint(), ProceduralEnemyModels.SharedMaterial, parent);
            visual.name = VisualName;
            return visual;
        }

        /// <summary>Builds the part hierarchy of <paramref name="blueprint"/> (no rig component: the companion animates the parts itself).</summary>
        public static GameObject Instantiate(ProceduralEnemyBlueprint blueprint, Material material, Transform parent)
        {
            var root = new GameObject(VisualName);
            if (parent != null)
            {
                root.transform.SetParent(parent, false);
            }

            var transforms = new Transform[blueprint.Parts.Count];
            for (int i = 0; i < blueprint.Parts.Count; i++)
            {
                ProceduralEnemyBlueprint.Part part = blueprint.Parts[i];
                var node = new GameObject(part.Name);
                Transform t = node.transform;
                t.SetParent(part.Parent >= 0 ? transforms[part.Parent] : root.transform, false);
                t.SetLocalPositionAndRotation(part.Position, part.Rotation);
                t.localScale = part.Scale;
                transforms[i] = t;

                if (part.Mesh != null)
                {
                    node.AddComponent<MeshFilter>().sharedMesh = part.Mesh;
                    node.AddComponent<MeshRenderer>().sharedMaterial = material;
                }
            }

            return root;
        }

        private static bool IsAlive(ProceduralEnemyBlueprint blueprint)
        {
            if (blueprint.Meshes.Count == 0)
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
