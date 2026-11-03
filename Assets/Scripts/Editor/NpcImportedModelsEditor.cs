using System.Collections.Generic;
using System.Linq;
using Rapadura.Gameplay.NPC.Visuals;
using UnityEditor;
using UnityEngine;

namespace Rapadura.Editor
{
    /// <summary>
    /// Editor side of <see cref="NpcImportedModels"/>: registers the imported NPC models found in <c>Assets/Models/NPCs</c>
    /// (read only — the asset files are never modified) and the menu toggle <b>Rapadura ▸ NPCs ▸ Use Imported Vó Edna
    /// Model</b> (saved in EditorPrefs, off by default). With it on, Vó Edna's figure (preview, Play mode in the editor)
    /// shows the OBJ from another session scaled to her recipe height, feet on the ground, on a URP Lit adapter material
    /// with its own texture. Player builds never register a provider, so they stay procedural. Docs/NPCs.md §1.
    /// </summary>
    [InitializeOnLoad]
    public static class NpcImportedModelsEditor
    {
        private const string MenuPath = "Rapadura/NPCs/Use Imported Vó Edna Model (experimental)";
        private const string PrefKey = "Rapadura.NpcImportedModels.Enabled";

        /// <summary>Recipe id → (mesh asset, texture asset, yaw). The OBJ faces +z already (Unity mirrors x on import).</summary>
        private static readonly Dictionary<string, (string Mesh, string Texture, float Yaw)> Sources = new Dictionary<string, (string, string, float)>
        {
            { "vo_edna", ("Assets/Models/NPCs/VoEdna/VoEdna.obj", "Assets/Models/NPCs/VoEdna/VoEdna_tex.png", 0f) },
        };

        private static readonly Dictionary<string, NpcImportedModel> Loaded = new Dictionary<string, NpcImportedModel>();

        static NpcImportedModelsEditor()
        {
            NpcImportedModels.Provider = Load;
            NpcImportedModels.Enabled = EditorPrefs.GetBool(PrefKey, false);
            EditorApplication.playModeStateChanged -= OnPlayMode;
            EditorApplication.playModeStateChanged += OnPlayMode;
        }

        private static void OnPlayMode(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingEditMode || state == PlayModeStateChange.ExitingPlayMode)
            {
                NpcImportedModels.DestroyMaterials();
            }
        }

        private static NpcImportedModel Load(string recipeId)
        {
            if (Loaded.TryGetValue(recipeId, out NpcImportedModel cached) && cached.Mesh != null)
            {
                return cached;
            }

            if (!Sources.TryGetValue(recipeId, out var source))
            {
                return null;
            }

            Mesh mesh = AssetDatabase.LoadAllAssetsAtPath(source.Mesh).OfType<Mesh>().FirstOrDefault();
            if (mesh == null)
            {
                Debug.LogWarning($"[NpcImportedModels] No mesh in {source.Mesh}; '{recipeId}' stays procedural.");
                return null;
            }

            var model = new NpcImportedModel
            {
                RecipeId = recipeId,
                Mesh = mesh,
                Texture = AssetDatabase.LoadAssetAtPath<Texture2D>(source.Texture),
                Yaw = source.Yaw,
                Source = source.Mesh,
            };
            Loaded[recipeId] = model;
            return model;
        }

        [MenuItem(MenuPath, priority = 40)]
        private static void Toggle()
        {
            NpcImportedModels.Enabled = !NpcImportedModels.Enabled;
            EditorPrefs.SetBool(PrefKey, NpcImportedModels.Enabled);
            Debug.Log($"[NpcImportedModels] Imported Vó Edna model {(NpcImportedModels.Enabled ? "ON" : "OFF")} — re-run Preview All NPCs / re-enter Play.");
        }

        [MenuItem(MenuPath, true)]
        private static bool ToggleValidate()
        {
            Menu.SetChecked(MenuPath, NpcImportedModels.Enabled);
            return true;
        }
    }
}
