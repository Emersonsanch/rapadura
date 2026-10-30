using Rapadura.Gameplay.World.Biomes;
using UnityEngine;
using UnityEngine.Rendering;

namespace Rapadura.Gameplay.NPC.Visuals
{
    /// <summary>
    /// The visible body of an NPC: one <see cref="SkinnedMeshRenderer"/> (the recipe's cached mesh, the shared palette
    /// material) over five bone transforms, plus the cheap procedural idle of <see cref="NpcIdleMath"/> — breathing, the
    /// role gesture, the head turning toward the player and, when the player stands beside them, the whole figure easing
    /// round to face them. Lives on its own child object (<see cref="ObjectName"/>), so the parent keeps its colliders,
    /// triggers and interaction components untouched. Skips the work while off screen.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class NpcFigure : MonoBehaviour
    {
        /// <summary>Name of the figure's object ("Npc_" so the scene solid binder never descends into it).</summary>
        public const string ObjectName = "Npc_Figure";

        private const string PlayerTag = "Player";
        private const float LookDistance = 7f;
        private const float FaceDistance = 3.2f;
        private const float FaceTurnSpeed = 90f;

        [SerializeField] private string _recipeId;

        private NpcRecipe _recipe;
        private NpcModel _model;
        private SkinnedMeshRenderer _renderer;
        private readonly Transform[] _bones = new Transform[NpcBones.Count];
        private Vector3[] _restLocal;
        private float _phase;
        private Vector2 _look;
        private float _homeYaw;
        private float _yawOffset;

        private static Transform _player;
        private static float _nextPlayerSearch;

        public NpcRecipe Recipe => _recipe;
        public NpcModel Model => _model;
        public SkinnedMeshRenderer Renderer => _renderer;
        public string RecipeId => _recipeId;
        public Transform Bone(int index) => _bones[index];

        /// <summary>
        /// Creates the figure under <paramref name="parent"/> with its feet at <paramref name="localPosition"/>, for the NPC
        /// <paramref name="id"/> (a recipe id, an old resident id, or any seed for a generic villager of <paramref name="region"/>).
        /// </summary>
        public static NpcFigure Create(Transform parent, string id, BiomeType region, Vector3 localPosition, float localYaw = 0f)
        {
            return Create(parent, NpcRecipes.Resolve(id, region), localPosition, localYaw);
        }

        public static NpcFigure Create(Transform parent, NpcRecipe recipe, Vector3 localPosition, float localYaw = 0f)
        {
            var go = new GameObject(ObjectName);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;
            go.transform.localRotation = Quaternion.Euler(0f, localYaw, 0f);
            NpcFigure figure = go.AddComponent<NpcFigure>();
            figure.Build(recipe);
            return figure;
        }

        /// <summary>(Re)builds the skeleton and renderer for <paramref name="recipe"/>.</summary>
        public void Build(NpcRecipe recipe)
        {
            _recipe = recipe;
            _recipeId = recipe.Id;
            _model = NpcModels.Get(recipe);

            for (int i = 0; i < _bones.Length; i++)
            {
                if (_bones[i] != null)
                {
                    DestroyImmediateSafe(_bones[i].gameObject);
                }
            }

            Vector3[] pivots = _model.Pivots;
            _restLocal = new Vector3[NpcBones.Count];
            _restLocal[NpcBones.Root] = Vector3.zero;
            _restLocal[NpcBones.Body] = pivots[NpcBones.Body];
            _restLocal[NpcBones.Head] = pivots[NpcBones.Head] - pivots[NpcBones.Body];
            _restLocal[NpcBones.ArmL] = pivots[NpcBones.ArmL] - pivots[NpcBones.Body];
            _restLocal[NpcBones.ArmR] = pivots[NpcBones.ArmR] - pivots[NpcBones.Body];

            for (int i = 0; i < NpcBones.Count; i++)
            {
                var bone = new GameObject(NpcBones.Names[i]).transform;
                Transform parent = i == NpcBones.Root ? transform : i == NpcBones.Body ? _bones[NpcBones.Root] : _bones[NpcBones.Body];
                bone.SetParent(parent, false);
                bone.localPosition = _restLocal[i];
                bone.localRotation = Quaternion.identity;
                _bones[i] = bone;
            }

            _renderer = GetComponent<SkinnedMeshRenderer>();
            if (_renderer == null)
            {
                _renderer = gameObject.AddComponent<SkinnedMeshRenderer>();
            }

            _renderer.sharedMesh = _model.Mesh;
            _renderer.sharedMaterial = NpcPalette.SharedMaterial;
            _renderer.bones = _bones;
            _renderer.rootBone = _bones[NpcBones.Root];
            _renderer.quality = SkinQuality.Bone1;
            _renderer.updateWhenOffscreen = false;
            _renderer.shadowCastingMode = ShadowCastingMode.On;
            _renderer.receiveShadows = true;
            _renderer.lightProbeUsage = LightProbeUsage.Off;
            _renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
            Bounds bounds = _model.Mesh.bounds;
            bounds.Expand(0.6f);
            _renderer.localBounds = bounds;

            _phase = Mathf.Abs(recipe.Id.GetHashCode() % 997) * 0.0173f;
            _homeYaw = transform.localEulerAngles.y;
            _yawOffset = 0f;
            _look = Vector2.zero;
        }

        /// <summary>Takes the current facing as the one to come back to (after the figure was placed).</summary>
        public void ResetHome()
        {
            _yawOffset = 0f;
            _homeYaw = transform.localEulerAngles.y;
        }

        private static void DestroyImmediateSafe(Object target)
        {
            if (Application.isPlaying)
            {
                Destroy(target);
            }
            else
            {
                DestroyImmediate(target);
            }
        }

        private void OnEnable()
        {
            _homeYaw = transform.localEulerAngles.y - _yawOffset;
        }

        private void LateUpdate()
        {
            if (_model == null || _renderer == null || !_renderer.isVisible)
            {
                return;
            }

            Transform player = FindPlayer();
            Animate(Time.time, player != null ? player.position : (Vector3?)null, Time.deltaTime);
        }

        private static Transform FindPlayer()
        {
            if (_player != null)
            {
                return _player;
            }

            if (Time.unscaledTime < _nextPlayerSearch)
            {
                return null;
            }

            _nextPlayerSearch = Time.unscaledTime + 1f;
            GameObject go = GameObject.FindGameObjectWithTag(PlayerTag);
            _player = go != null ? go.transform : null;
            return _player;
        }

        /// <summary>
        /// Poses the bones for <paramref name="time"/>: breathing, gesture and the look toward <paramref name="lookTarget"/>
        /// (world space; null = nobody to look at). Public so the editor preview can animate it outside Play mode.
        /// </summary>
        public void Animate(float time, Vector3? lookTarget, float deltaTime)
        {
            if (_model == null || _bones[NpcBones.Body] == null)
            {
                return;
            }

            float t = time + _phase;
            NpcPoseOffsets o = NpcIdleMath.Gesture(_recipe.Gesture, t);
            float breath = NpcIdleMath.Breath(time, _phase);

            Vector2 lookGoal = Vector2.zero;
            float faceGoal = 0f;
            if (lookTarget.HasValue)
            {
                Vector3 headWorld = _bones[NpcBones.Head].position;
                Vector3 toTarget = lookTarget.Value + Vector3.up * 1.2f - headWorld;
                if (toTarget.sqrMagnitude < LookDistance * LookDistance)
                {
                    Vector3 local = Quaternion.Inverse(transform.rotation) * toTarget;
                    lookGoal = NpcIdleMath.LookAngles(local) * o.LookWeight;
                    float yawToPlayer = Mathf.Atan2(local.x, local.z) * Mathf.Rad2Deg;
                    if (toTarget.sqrMagnitude < FaceDistance * FaceDistance && Mathf.Abs(yawToPlayer) > 40f)
                    {
                        faceGoal = Mathf.Clamp(_yawOffset + yawToPlayer * 0.6f, -150f, 150f);
                    }
                    else
                    {
                        faceGoal = _yawOffset;
                    }
                }
            }

            float k = deltaTime > 0f ? 1f - Mathf.Exp(-deltaTime * 5f) : 1f;
            _look = Vector2.Lerp(_look, lookGoal, k);
            if (deltaTime > 0f && Application.isPlaying)
            {
                _yawOffset = Mathf.MoveTowards(_yawOffset, faceGoal, FaceTurnSpeed * deltaTime);
                transform.localRotation = Quaternion.Euler(0f, _homeYaw + _yawOffset, 0f);
            }

            Transform body = _bones[NpcBones.Body];
            body.localPosition = _restLocal[NpcBones.Body] + Vector3.up * o.BodyBob;
            body.localRotation = Quaternion.Euler(o.BodyPitch, 0f, 0f);
            body.localScale = new Vector3(1f + breath * 0.006f, 1f + breath * 0.012f, 1f + breath * 0.008f);

            _bones[NpcBones.Head].localRotation = Quaternion.Euler(
                Mathf.Clamp(o.HeadPitch + _look.y + breath * 0.8f, -30f, 30f),
                Mathf.Clamp(o.HeadYaw + _look.x, -NpcIdleMath.MaxHeadYaw, NpcIdleMath.MaxHeadYaw),
                o.HeadRoll);
            _bones[NpcBones.ArmL].localRotation = Quaternion.Euler(o.ArmL.x + breath * 1.2f, 0f, -o.ArmL.z - breath * 0.8f);
            _bones[NpcBones.ArmR].localRotation = Quaternion.Euler(o.ArmR.x + breath * 1.2f, 0f, o.ArmR.z + breath * 0.8f);
        }
    }
}
