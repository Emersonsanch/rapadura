using System.Collections.Generic;
using Rapadura.Gameplay.NPC.Visuals;
using Rapadura.Gameplay.World.Biomes;
using UnityEditor;
using UnityEngine;

namespace Rapadura.Editor
{
    /// <summary>
    /// Editor preview of every NPC look (Docs/NPCs.md): the named residents in rows by region, then a row of generic
    /// villagers per region, labelled with their id and story line and playing their idle (head turned to the scene
    /// camera). Built as a <c>DontSave</c> root into the open scene (never written to the scene file); use an empty
    /// scene (File ▸ New Scene) for clean screenshots.
    /// Cleanup is reliable: Clear finds the root with <c>Resources.FindObjectsOfTypeAll</c> (a DontSave root is outside
    /// every scene, FindObjectsByType misses it) and skips assets (<c>EditorUtility.IsPersistent</c>); entering Play mode
    /// or reloading scripts clears the preview first and then frees the cached meshes and the shared material, so a
    /// stale preview can never sit in the game with a destroyed material (the old "magenta" leak).
    /// </summary>
    [InitializeOnLoad]
    public static class NpcPreviewMenu
    {
        private const string Root = "Rapadura/NPCs/";
        private const string PreviewName = "[NpcPreview]";
        private const float Spacing = 1.7f;
        private const float RowSpacing = 2.6f;

        private static readonly List<NpcFigure> Figures = new List<NpcFigure>();
        private static double _lastTime;

        static NpcPreviewMenu()
        {
            EditorApplication.playModeStateChanged -= OnPlayMode;
            EditorApplication.playModeStateChanged += OnPlayMode;
            AssemblyReloadEvents.beforeAssemblyReload -= OnBeforeReload;
            AssemblyReloadEvents.beforeAssemblyReload += OnBeforeReload;
        }

        private static void OnPlayMode(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingEditMode)
            {
                Clear();
                NpcModels.Clear();
                NpcPalette.DestroyMaterial();
            }
        }

        private static void OnBeforeReload() => Clear();

        [MenuItem(Root + "Preview All NPCs", priority = 10)]
        private static void PreviewAll()
        {
            Clear();
            var holder = new GameObject(PreviewName) { hideFlags = HideFlags.DontSave };
            var byRegion = new Dictionary<BiomeType, List<NpcRecipe>>();
            foreach (NpcRecipe recipe in NpcRecipes.Named)
            {
                if (!byRegion.TryGetValue(recipe.Region, out List<NpcRecipe> list))
                {
                    byRegion[recipe.Region] = list = new List<NpcRecipe>();
                }

                list.Add(recipe);
            }

            int row = 0;
            int triangles = 0;
            foreach (BiomeType region in new[] { BiomeType.Forest, BiomeType.Desert, BiomeType.Mountain, BiomeType.Caves })
            {
                if (byRegion.TryGetValue(region, out List<NpcRecipe> list))
                {
                    triangles += PlaceRow(holder.transform, list, row++);
                }
            }

            foreach (BiomeType region in new[] { BiomeType.Forest, BiomeType.Desert, BiomeType.Mountain, BiomeType.Caves })
            {
                var villagers = new List<NpcRecipe>();
                for (int i = 0; i < 6; i++)
                {
                    villagers.Add(NpcRecipes.Villager("preview_" + i, region));
                }

                triangles += PlaceRow(holder.transform, villagers, row++);
            }

            foreach (Transform t in holder.GetComponentsInChildren<Transform>(true))
            {
                t.gameObject.hideFlags = HideFlags.DontSave;
            }

            Selection.activeGameObject = holder;
            Debug.Log($"[NpcPreview] {Figures.Count} figures ({NpcRecipes.Named.Count} named), {triangles} triangles, {NpcModels.Count} cached meshes, 1 material.");

            _lastTime = EditorApplication.timeSinceStartup;
            EditorApplication.update -= Tick;
            EditorApplication.update += Tick;
            SceneView.duringSceneGui -= DrawLabels;
            SceneView.duringSceneGui += DrawLabels;

            if (SceneView.lastActiveSceneView != null)
            {
                SceneView sv = SceneView.lastActiveSceneView;
                sv.orthographic = false;
                sv.LookAt(new Vector3(Spacing * 3f, 1f, RowSpacing * 1.5f), Quaternion.Euler(28f, 15f, 0f), 11f);
            }
        }

        private static int PlaceRow(Transform parent, List<NpcRecipe> recipes, int row)
        {
            int triangles = 0;
            for (int i = 0; i < recipes.Count; i++)
            {
                var position = new Vector3(i * Spacing, 0f, row * RowSpacing);
                NpcFigure figure = NpcFigure.Create(parent, recipes[i], position, 180f);
                figure.gameObject.name = NpcFigure.ObjectName + "_" + recipes[i].Id;
                Figures.Add(figure);
                triangles += figure.Model.Triangles;
            }

            return triangles;
        }

        private static void Tick()
        {
            Figures.RemoveAll(f => f == null);
            if (Figures.Count == 0)
            {
                EditorApplication.update -= Tick;
                SceneView.duringSceneGui -= DrawLabels;
                return;
            }

            double now = EditorApplication.timeSinceStartup;
            float dt = (float)(now - _lastTime);
            if (dt < 1f / 30f)
            {
                return;
            }

            _lastTime = now;
            Vector3? eye = SceneView.lastActiveSceneView != null && SceneView.lastActiveSceneView.camera != null
                ? SceneView.lastActiveSceneView.camera.transform.position - Vector3.up * 1.2f
                : (Vector3?)null;
            foreach (NpcFigure figure in Figures)
            {
                figure.Animate((float)now, eye, dt);
            }

            SceneView.RepaintAll();
        }

        private static void DrawLabels(SceneView view)
        {
            var style = new GUIStyle(EditorStyles.whiteMiniLabel) { alignment = TextAnchor.MiddleCenter, wordWrap = true, fixedWidth = 120f };
            foreach (NpcFigure figure in Figures)
            {
                if (figure == null || figure.Recipe == null)
                {
                    continue;
                }

                Vector3 at = figure.transform.position + Vector3.up * (figure.Model.Height + 0.35f);
                string id = figure.Recipe.Id.StartsWith("villager:") ? "villager" : figure.Recipe.Id;
                Handles.Label(at, $"{id}\n{figure.Model.Triangles} tris\n{figure.Recipe.Story}", style);
            }
        }

        [MenuItem(Root + "Clear NPC Preview", priority = 11)]
        private static void Clear()
        {
            foreach (GameObject go in Resources.FindObjectsOfTypeAll<GameObject>())
            {
                if (go != null && go.transform.parent == null && go.name == PreviewName && !EditorUtility.IsPersistent(go))
                {
                    Object.DestroyImmediate(go);
                }
            }

            Figures.Clear();
            EditorApplication.update -= Tick;
            SceneView.duringSceneGui -= DrawLabels;
        }

        [MenuItem(Root + "Log NPC Budgets", priority = 30)]
        private static void LogBudgets()
        {
            int max = 0, total = 0;
            foreach (NpcRecipe recipe in NpcRecipes.Named)
            {
                int tris = NpcModelBuilder.CountTriangles(recipe);
                max = Mathf.Max(max, tris);
                total += tris;
                Debug.Log($"[Npc] {recipe.Id}: {tris} tris — {recipe.Story}");
            }

            Debug.Log($"[Npc] {NpcRecipes.Named.Count} named NPCs, max {max}, mean {total / Mathf.Max(1, NpcRecipes.Named.Count)} triangles.");
        }
    }
}
