using System;
using System.Collections.Generic;
using Rapadura.Gameplay.Enemies.Visuals;
using UnityEngine;
using UnityEngine.Rendering;

namespace Rapadura.Gameplay.NPC.Visuals
{
    /// <summary>A hand-made / imported mesh that may stand in for a recipe's procedural figure (e.g. Vó Edna's OBJ).</summary>
    public sealed class NpcImportedModel
    {
        public string RecipeId;
        public Mesh Mesh;

        /// <summary>Its own albedo texture (the adapter puts it on a URP Lit material like the palette's).</summary>
        public Texture2D Texture;

        /// <summary>Extra turn so the model faces +z like the figures (degrees).</summary>
        public float Yaw;

        /// <summary>Where it came from (docs, logs).</summary>
        public string Source;
    }

    /// <summary>
    /// Optional swap of a procedural NPC figure for an imported model, behind a flag (<see cref="Enabled"/>, off by
    /// default). The adapter scales the mesh to the recipe's height with the feet on the ground, turns it to face +z and
    /// gives it a URP Lit material built the same way as the palette's (<see cref="EnemyModelPalette.CreateMaterial"/>),
    /// one per texture, cached. The imported mesh hangs under the figure's Body bone, so it still breathes, bobs and
    /// turns to face the player; it has no arms or head bones of its own (no gesture, no head turn). The models are
    /// supplied by <see cref="Provider"/> — in the editor <c>NpcImportedModelsEditor</c> loads them from
    /// <c>Assets/Models/NPCs</c> without touching those files; in a player build nobody registers one and every NPC
    /// stays procedural. Docs/NPCs.md §1.
    /// </summary>
    public static class NpcImportedModels
    {
        /// <summary>Off by default: imported models break the one-material / 600–1500-triangle budget of the figures.</summary>
        public static bool Enabled { get; set; }

        /// <summary>Recipe id → imported model (or null). Registered by an editor hook or a future runtime catalogue.</summary>
        public static Func<string, NpcImportedModel> Provider { get; set; }

        private static readonly Dictionary<Texture2D, Material> Materials = new Dictionary<Texture2D, Material>();

        /// <summary>The imported model for <paramref name="recipeId"/> when the flag is on and one is registered.</summary>
        public static NpcImportedModel Find(string recipeId)
        {
            if (!Enabled || Provider == null || string.IsNullOrEmpty(recipeId))
            {
                return null;
            }

            NpcImportedModel model = Provider(recipeId);
            return model != null && model.Mesh != null ? model : null;
        }

        /// <summary>Uniform scale and lift that stand <paramref name="mesh"/> on y = 0 at <paramref name="height"/> metres.</summary>
        public static void Fit(Bounds meshBounds, float height, out float scale, out Vector3 offset)
        {
            float h = Mathf.Max(1e-4f, meshBounds.size.y);
            scale = height / h;
            offset = new Vector3(-meshBounds.center.x * scale, -meshBounds.min.y * scale, -meshBounds.center.z * scale);
        }

        /// <summary>The adapter material for <paramref name="model"/> (palette material when it has no texture).</summary>
        public static Material MaterialFor(NpcImportedModel model)
        {
            if (model.Texture == null)
            {
                return NpcPalette.SharedMaterial;
            }

            if (Materials.TryGetValue(model.Texture, out Material material) && material != null)
            {
                return material;
            }

            material = EnemyModelPalette.CreateMaterial(model.Texture, null);
            material.name = "NpcImported_" + model.Texture.name;
            material.hideFlags = HideFlags.DontSave;
            Materials[model.Texture] = material;
            return material;
        }

        /// <summary>Destroys the adapter materials (editor cleanup; the textures belong to their assets and stay).</summary>
        public static void DestroyMaterials()
        {
            foreach (Material material in Materials.Values)
            {
                if (material != null)
                {
                    UnityEngine.Object.DestroyImmediate(material);
                }
            }

            Materials.Clear();
        }

        /// <summary>
        /// Puts <paramref name="model"/> under <paramref name="body"/> (the figure's Body bone, whose rest position is
        /// <paramref name="bodyRest"/> above the feet), fitted to <paramref name="height"/>. Returns its renderer.
        /// </summary>
        public static MeshRenderer Attach(Transform body, Vector3 bodyRest, NpcImportedModel model, float height)
        {
            var go = new GameObject(ObjectName);
            go.transform.SetParent(body, false);
            Fit(model.Mesh.bounds, height, out float scale, out Vector3 offset);
            Quaternion yaw = Quaternion.Euler(0f, model.Yaw, 0f);
            go.transform.localPosition = -bodyRest + yaw * offset;
            go.transform.localRotation = yaw;
            go.transform.localScale = Vector3.one * scale;
            go.AddComponent<MeshFilter>().sharedMesh = model.Mesh;
            MeshRenderer renderer = go.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = MaterialFor(model);
            renderer.shadowCastingMode = ShadowCastingMode.On;
            renderer.receiveShadows = true;
            renderer.lightProbeUsage = LightProbeUsage.Off;
            renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
            return renderer;
        }

        /// <summary>Name of the imported model's object under the Body bone.</summary>
        public const string ObjectName = "Npc_Imported";
    }
}
