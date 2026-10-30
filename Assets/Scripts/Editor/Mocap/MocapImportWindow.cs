using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Rapadura.Editor.Mocap
{
    /// <summary>
    /// Editor UI for the BVH → Humanoid pipeline (see Docs/Mocap.md):
    ///  - "Rapadura/Animation/Import Mocap BVH…" — one file, all options exposed.
    ///  - "Rapadura/Animation/Import CMU Mocap Set" — every clip listed in tools/mocap/out/manifest.json, baked on
    ///    Ícaro into Assets/Animations/Mocap; clips marked <c>perHero</c> (the locomotion set) are also baked on each
    ///    humanoid hero's own avatar into Assets/Animations/Mocap/&lt;Hero&gt;/ (exact on that hero: no cross-avatar
    ///    muscle retarget between bind poses that differ by 20–50° per limb).
    /// </summary>
    public sealed class MocapImportWindow : EditorWindow
    {
        private const string ManifestRelativePath = "tools/mocap/out/manifest.json";

        [SerializeField] private GameObject _model;
        [SerializeField] private string _bvhPath = "";
        [SerializeField] private MocapImportOptions _options = new MocapImportOptions();

        [MenuItem("Rapadura/Animation/Import Mocap BVH…")]
        private static void Open()
        {
            var w = GetWindow<MocapImportWindow>(true, "Import Mocap BVH");
            if (w._model == null) w._model = AssetDatabase.LoadAssetAtPath<GameObject>(MocapClipBuilder.DefaultModelPath);
            w.minSize = new Vector2(420, 300);
        }

        [MenuItem("Rapadura/Animation/Import CMU Mocap Set")]
        public static void ImportSetMenu() => ImportSet(null);

        /// <summary>Batch entry point (also usable with -executeMethod Rapadura.Editor.Mocap.MocapImportWindow.ImportSetBatch).</summary>
        public static void ImportSetBatch() => ImportSet(null);

        private void OnGUI()
        {
            EditorGUILayout.HelpBox("Converte um BVH (CMU / tools/mocap/out) em AnimationClip Humanoid. " +
                                    "O clipe vale para qualquer heroi Humanoid; o modelo abaixo e so a referencia de retarget.", MessageType.Info);
            _model = (GameObject)EditorGUILayout.ObjectField("Modelo Humanoid", _model, typeof(GameObject), false);
            using (new EditorGUILayout.HorizontalScope())
            {
                _bvhPath = EditorGUILayout.TextField("Arquivo BVH", _bvhPath);
                if (GUILayout.Button("…", GUILayout.Width(28)))
                {
                    string dir = Path.Combine(ProjectRoot(), "tools", "mocap", "out");
                    string p = EditorUtility.OpenFilePanel("BVH", Directory.Exists(dir) ? dir : ProjectRoot(), "bvh");
                    if (!string.IsNullOrEmpty(p))
                    {
                        _bvhPath = p;
                        _options.ClipName = Path.GetFileNameWithoutExtension(p);
                    }
                }
            }
            _options.ClipName = EditorGUILayout.TextField("Nome do clipe", _options.ClipName);
            _options.OutputFolder = EditorGUILayout.TextField("Pasta de saida", _options.OutputFolder);
            using (new EditorGUILayout.HorizontalScope())
            {
                _options.UnitScale = EditorGUILayout.FloatField("Escala (unid.→m)", _options.UnitScale);
                if (GUILayout.Button("CMU bruto", GUILayout.Width(80))) _options.UnitScale = MocapMath.CmuUnitToMeters;
                if (GUILayout.Button("metros", GUILayout.Width(60))) _options.UnitScale = 1f;
            }
            _options.TPoseFrame0 = EditorGUILayout.Toggle("Frame 0 é T-pose", _options.TPoseFrame0);
            _options.Loop = EditorGUILayout.Toggle("Loop", _options.Loop);
            _options.InPlace = EditorGUILayout.Toggle("No lugar (in-place)", _options.InPlace);
            _options.RootMotion = EditorGUILayout.Toggle("Manter root motion", _options.RootMotion);

            GUILayout.Space(8);
            using (new EditorGUI.DisabledScope(_model == null || !File.Exists(_bvhPath)))
            {
                if (GUILayout.Button("Importar", GUILayout.Height(28)))
                {
                    try
                    {
                        var clip = MocapClipBuilder.ImportFile(_bvhPath, _model, _options);
                        EditorGUIUtility.PingObject(clip);
                        Debug.Log($"[Mocap] {_options.ClipName}: {clip.length:0.00}s → {AssetDatabase.GetAssetPath(clip)}");
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                        EditorUtility.DisplayDialog("Mocap", e.Message, "OK");
                    }
                }
            }
            GUILayout.Space(4);
            if (GUILayout.Button("Importar conjunto CMU (manifest.json)")) ImportSet(_model);
        }

        private static void ImportSet(GameObject model)
        {
            model = model != null ? model : AssetDatabase.LoadAssetAtPath<GameObject>(MocapClipBuilder.DefaultModelPath);
            string manifestPath = Path.Combine(ProjectRoot(), ManifestRelativePath);
            if (model == null) { Debug.LogError($"[Mocap] Model not found: {MocapClipBuilder.DefaultModelPath}"); return; }
            if (!File.Exists(manifestPath))
            {
                Debug.LogError($"[Mocap] {manifestPath} not found — run tools/mocap: python bvh_prep.py --set clips.json --out-dir out");
                return;
            }
            var manifest = JsonUtility.FromJson<MocapManifest>(File.ReadAllText(manifestPath));
            string dir = Path.GetDirectoryName(manifestPath);
            var heroes = MocapClipBuilder.HumanoidHeroModels();
            int ok = 0, perHeroOk = 0;
            try
            {
                for (int i = 0; i < manifest.clips.Length; i++)
                {
                    var c = manifest.clips[i];
                    EditorUtility.DisplayProgressBar("Mocap", c.name, (float)i / manifest.clips.Length);
                    var opt = new MocapImportOptions
                    {
                        ClipName = c.name,
                        UnitScale = 1f,
                        TPoseFrame0 = c.tposeFrame0,
                        Loop = c.loop,
                        InPlace = c.inPlace,
                        RootMotion = c.rootMotion,
                    };
                    try
                    {
                        var clip = MocapClipBuilder.ImportFile(Path.Combine(dir, c.file), model, opt);
                        Debug.Log($"[Mocap] {c.name} (CMU {c.cmuTrial}, {c.description}): {clip.length:0.00}s loop={c.loop} → {AssetDatabase.GetAssetPath(clip)}{MocapClipSanity.Describe(clip)}");
                        ok++;
                        if (c.perHero)
                        {
                            foreach (var (hero, heroModel) in heroes)
                            {
                                opt.OutputFolder = MocapClipBuilder.HeroFolder(hero);
                                var heroClip = MocapClipBuilder.ImportFile(Path.Combine(dir, c.file), heroModel, opt);
                                Debug.Log($"[Mocap]   {hero}: {AssetDatabase.GetAssetPath(heroClip)}{MocapClipSanity.Describe(heroClip)}");
                                perHeroOk++;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"[Mocap] {c.name} failed: {e}");
                    }
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
            AssetDatabase.Refresh();
            Debug.Log($"[Mocap] Imported {ok}/{manifest.clips.Length} clips into {MocapClipBuilder.DefaultOutputFolder} using '{model.name}', "
                      + $"plus {perHeroOk} per-hero locomotion clips ({string.Join(", ", heroes.ConvertAll(h => h.hero))}).");
        }

        private static string ProjectRoot() => Directory.GetParent(Application.dataPath).FullName;

#pragma warning disable 0649 // fields are assigned by JsonUtility
        [Serializable]
        private sealed class MocapManifest
        {
            public int fps;
            public ManifestClip[] clips = Array.Empty<ManifestClip>();
        }

        [Serializable]
        private sealed class ManifestClip
        {
            public string name;
            public string file;
            public bool loop;
            public bool inPlace;
            public bool rootMotion;
            public bool tposeFrame0 = true;
            public bool perHero;
            public string cmuTrial;
            public string description;
        }
#pragma warning restore 0649
    }
}
