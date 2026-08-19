using System.IO;
using System.Linq;
using Rapadura.Core.Managers;
using Rapadura.Gameplay.Player;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Rapadura.Editor
{
    /// <summary>
    /// One-click editor tool that builds a minimal playable <c>Assets/Scenes/Main.unity</c> —
    /// GameManager + Player (CharacterController/PlayerInput/PlayerInputHandler/PlayerMotor/
    /// PlayerStats/AttributeSet/PlayerController) + a free-look camera — entirely by code, so
    /// nobody has to hand-assemble GameObjects/components in the Inspector to get the project
    /// into Play Mode for the first time. Safe to re-run: if <c>Assets/Scenes/Main.unity</c>
    /// already exists it is opened and missing pieces are added rather than duplicated.
    ///
    /// Does NOT set up UI Toolkit screens (HUD/Menus/Dialogue/Shop) yet — those need a
    /// PanelSettings asset and per-screen wiring that's a separate follow-up tool. Run the
    /// content seeders (Rapadura &gt; Seed ...) before or after this, order doesn't matter for
    /// the scene itself.
    /// </summary>
    public static class MainSceneBootstrapper
    {
        private const string ScenesFolder = "Assets/Scenes";
        private const string ScenePath = ScenesFolder + "/Main.unity";
        private const string InputActionsPath = "Assets/InputActions/PlayerControls.inputactions";

        [MenuItem("Rapadura/Setup/1) Build Main Scene (Player + Camera + GameManager)")]
        public static void BuildMainScene()
        {
            Scene scene = OpenOrCreateScene();

            GameObject gameManagerGo = FindOrCreate("GameManager");
            if (gameManagerGo.GetComponent<GameManager>() == null)
            {
                gameManagerGo.AddComponent<GameManager>();
            }

            GameObject playerGo = FindOrCreate("Player");
            BuildPlayer(playerGo);

            GameObject cameraGo = BuildCamera(playerGo);

            WirePlayerController(playerGo, cameraGo);

            EditorSceneManager.MarkSceneDirty(scene);
            Directory.CreateDirectory(ScenesFolder);
            EditorSceneManager.SaveScene(scene, ScenePath);

            Selection.activeGameObject = playerGo;
            Debug.Log("[MainSceneBootstrapper] Main scene ready at " + ScenePath +
                       ". Press Play to test movement/camera. UI screens (HUD/Menus) still need " +
                       "a separate PanelSettings-based setup pass.");
        }

        private static Scene OpenOrCreateScene()
        {
            if (File.Exists(ScenePath))
            {
                return EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            }

            Directory.CreateDirectory(ScenesFolder);
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            return scene;
        }

        private static GameObject FindOrCreate(string name)
        {
            GameObject existing = GameObject.Find(name);
            if (existing != null)
            {
                return existing;
            }

            return new GameObject(name);
        }

        private static void BuildPlayer(GameObject playerGo)
        {
            playerGo.tag = "Player";

            var characterController = playerGo.GetComponent<CharacterController>();
            if (characterController == null)
            {
                characterController = playerGo.AddComponent<CharacterController>();
            }
            characterController.center = new Vector3(0f, 1f, 0f);
            characterController.height = 2f;
            characterController.radius = 0.35f;

            var playerInput = playerGo.GetComponent<PlayerInput>();
            if (playerInput == null)
            {
                playerInput = playerGo.AddComponent<PlayerInput>();
            }
            InputActionAsset actions = AssetDatabase.LoadAssetAtPath<InputActionAsset>(InputActionsPath);
            if (actions == null)
            {
                Debug.LogWarning("[MainSceneBootstrapper] Could not find " + InputActionsPath +
                                  " — PlayerInput.actions left unassigned, input will not work until set manually.");
            }
            else
            {
                playerInput.actions = actions;
                string defaultMap = actions.actionMaps.Count > 0 ? actions.actionMaps[0].name : null;
                if (defaultMap != null)
                {
                    playerInput.defaultActionMap = defaultMap;
                }
            }

            AddIfMissing<PlayerInputHandler>(playerGo);
            AddIfMissing<PlayerMotor>(playerGo);
            AddIfMissing<PlayerStats>(playerGo);
            AddIfMissing<AttributeSet>(playerGo);

            if (playerGo.GetComponent<Animator>() == null)
            {
                playerGo.AddComponent<Animator>();
            }

            // PlayerController is added last: its [RequireComponent] attributes need the above
            // components to already exist on the GameObject when Unity validates them.
            AddIfMissing<Rapadura.Gameplay.Player.PlayerController>(playerGo);
        }

        private static GameObject BuildCamera(GameObject playerGo)
        {
            GameObject pivotGo = playerGo.transform.Find("CameraPivot")?.gameObject;
            if (pivotGo == null)
            {
                pivotGo = new GameObject("CameraPivot");
                pivotGo.transform.SetParent(playerGo.transform);
                pivotGo.transform.localPosition = new Vector3(0f, 1.6f, 0f);
            }

            GameObject cameraGo = GameObject.Find("Main Camera");
            if (cameraGo == null)
            {
                cameraGo = new GameObject("Main Camera");
            }
            cameraGo.tag = "MainCamera";

            if (cameraGo.GetComponent<Camera>() == null)
            {
                cameraGo.AddComponent<Camera>();
            }
            if (cameraGo.GetComponent<AudioListener>() == null)
            {
                cameraGo.AddComponent<AudioListener>();
            }

            var playerCamera = cameraGo.GetComponent<PlayerCamera>();
            if (playerCamera == null)
            {
                playerCamera = cameraGo.AddComponent<PlayerCamera>();
            }

            // _pivot is a private [SerializeField] on PlayerCamera — set via SerializedObject
            // rather than reflection so it plays nicely with the Editor's undo/dirty system.
            var so = new SerializedObject(playerCamera);
            SerializedProperty pivotProp = so.FindProperty("_pivot");
            if (pivotProp != null)
            {
                pivotProp.objectReferenceValue = pivotGo.transform;
                so.ApplyModifiedPropertiesWithoutUndo();
            }

            return cameraGo;
        }

        private static void WirePlayerController(GameObject playerGo, GameObject cameraGo)
        {
            var controller = playerGo.GetComponent<Rapadura.Gameplay.Player.PlayerController>();
            var playerCamera = cameraGo.GetComponent<PlayerCamera>();
            Transform pivot = playerGo.transform.Find("CameraPivot");

            var so = new SerializedObject(controller);

            SerializedProperty cameraProp = so.FindProperty("_playerCamera");
            if (cameraProp != null)
            {
                cameraProp.objectReferenceValue = playerCamera;
            }

            SerializedProperty lookProp = so.FindProperty("_cameraLookTransform");
            if (lookProp != null && pivot != null)
            {
                lookProp.objectReferenceValue = pivot;
            }

            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void AddIfMissing<T>(GameObject go) where T : Component
        {
            if (go.GetComponent<T>() == null)
            {
                go.AddComponent<T>();
            }
        }
    }
}
