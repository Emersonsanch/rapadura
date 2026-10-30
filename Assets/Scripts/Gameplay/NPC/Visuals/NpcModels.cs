using System.Collections.Generic;
using UnityEngine;

namespace Rapadura.Gameplay.NPC.Visuals
{
    /// <summary>
    /// Session cache of the built NPC meshes, one per recipe id: every figure of the same person shares the mesh (and all
    /// share <see cref="NpcPalette.SharedMaterial"/>), so re-entering a town or spawning the same keeper builds nothing.
    /// The meshes are <c>DontSave</c>; a destroyed one (editor cleanup) is rebuilt on the next request.
    /// </summary>
    public static class NpcModels
    {
        private static readonly Dictionary<string, NpcModel> Cache = new Dictionary<string, NpcModel>();

        public static int Count => Cache.Count;

        public static NpcModel Get(NpcRecipe recipe)
        {
            if (recipe == null)
            {
                return null;
            }

            if (Cache.TryGetValue(recipe.Id, out NpcModel model) && model.Mesh != null)
            {
                return model;
            }

            model = NpcModelBuilder.Build(recipe);
            Cache[recipe.Id] = model;
            return model;
        }

        /// <summary>Destroys every cached mesh (editor cleanup; figures still using them lose their mesh).</summary>
        public static void Clear()
        {
            foreach (NpcModel model in Cache.Values)
            {
                if (model.Mesh != null)
                {
                    Object.DestroyImmediate(model.Mesh);
                }
            }

            Cache.Clear();
        }
    }
}
