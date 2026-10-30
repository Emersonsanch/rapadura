using System.Collections.Generic;
using Rapadura.Gameplay.World.Cities;
using Rapadura.Gameplay.World.Cities.Town;
using UnityEditor;
using UnityEngine;

namespace Rapadura.Editor
{
    /// <summary>
    /// Editor previews of the walkable towns (Docs/Cities.md §5.6). They build the town into the open scene as a
    /// <c>DontSave</c> object (never written to the scene file, nothing to undo), so the art can be checked and
    /// screenshotted without entering Play Mode. Use an empty scene (File ▸ New Scene) so the arena does not overlap.
    /// </summary>
    public static class CityTownPreviewMenu
    {
        private const string Root = "Rapadura/Cities/";
        private const string PreviewName = "[TownPreview]";

        [MenuItem(Root + "Preview Walkable Town/Vila da Vó Edna (Zarafshan)", priority = 10)]
        private static void PreviewVila() => Preview(CityCatalog.VilaZefa);

        [MenuItem(Root + "Preview Walkable Town/Oásis de Qocho (default plan)", priority = 11)]
        private static void PreviewQocho() => Preview(CityCatalog.AreiaBranca);

        [MenuItem(Root + "Preview Walkable Town/Vila de Litema (default plan)", priority = 12)]
        private static void PreviewLitema() => Preview(CityCatalog.SerraAlta);

        [MenuItem(Root + "Preview Walkable Town/Vila das Lamparinas (default plan)", priority = 13)]
        private static void PreviewLamparinas() => Preview(CityCatalog.Lamparinas);

        [MenuItem(Root + "Preview Walkable Town/Toggle Night", priority = 30)]
        private static void ToggleNight()
        {
            CityNightLights lights = Object.FindAnyObjectByType<CityNightLights>();
            if (lights == null)
            {
                Debug.LogWarning("[TownPreview] Build a preview first.");
                return;
            }

            lights.NightOverride = lights.NightOverride.HasValue && lights.NightOverride.Value > 0.5f ? 0f : 1f;
            lights.Refresh();
            SceneView.RepaintAll();
        }

        [MenuItem(Root + "Preview Walkable Town/Clear Preview", priority = 31)]
        private static void Clear()
        {
            foreach (GameObject go in Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (go != null && go.name == PreviewName)
                {
                    Object.DestroyImmediate(go);
                }
            }

            TownMeshCache.Clear();
            CityTownRecipes.ClearCache();
        }

        [MenuItem(Root + "Validate Town Layouts", priority = 40)]
        private static void Validate()
        {
            int total = 0;
            foreach (CitySpec city in CityCatalog.Cities)
            {
                foreach (bool mascate in new[] { false, true })
                {
                    CityLayout layout = CityLayoutGenerator.Generate(CityTownRecipes.RecipeFor(city.CityId, mascate));
                    List<string> problems = CityLayoutRules.Validate(layout);
                    total += problems.Count;
                    string head = $"[Town] {city.CityId}{(mascate ? " +caravaneer" : string.Empty)}: {layout.Lots.Count} lots, {layout.Props.Count} props, {layout.Solids.Count} colliders";
                    if (problems.Count == 0)
                    {
                        Debug.Log(head + " — OK");
                    }
                    else
                    {
                        Debug.LogError(head + "\n" + string.Join("\n", problems));
                    }
                }
            }

            Debug.Log(total == 0 ? "[Town] Every layout passes." : $"[Town] {total} problem(s).");
        }

        private static void Preview(string cityId)
        {
            Clear();
            CityLayout layout = CityTownRecipes.LayoutFor(cityId);
            var holder = new GameObject(PreviewName) { hideFlags = HideFlags.DontSave };
            CityTownView view = CityTownView.Build(layout, holder.transform, "preview:" + cityId);
            foreach (Transform t in holder.GetComponentsInChildren<Transform>(true))
            {
                t.gameObject.hideFlags = HideFlags.DontSave;
            }

            Selection.activeGameObject = view.gameObject;
            Debug.Log($"[TownPreview] {cityId}: {view.RendererCount} renderers, {view.ColliderCount} colliders, {view.TriangleCount} triangles, {TownMeshCache.Count} cached meshes.");
            if (SceneView.lastActiveSceneView != null)
            {
                SceneView sv = SceneView.lastActiveSceneView;
                sv.orthographic = false;
                sv.LookAt(new Vector3(0f, 0f, 0f), Quaternion.Euler(57.5f, 45f, 0f), 30f);
            }
        }
    }
}
