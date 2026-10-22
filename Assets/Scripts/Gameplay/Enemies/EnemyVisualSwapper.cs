using System.Collections.Generic;
using UnityEngine;

namespace Rapadura.Gameplay.Enemies
{
    /// <summary>Primitive used for an enemy's placeholder body when it has no model (see <see cref="EnemyVisualSwapper"/>).</summary>
    public enum EnemyPlaceholderShape
    {
        Capsule = 0,
        Cube = 1,
        Sphere = 2,
        Cylinder = 3,
    }

    /// <summary>Where the visual of a spawned enemy comes from.</summary>
    public enum EnemyVisualSource
    {
        /// <summary>Keep whatever visual the shared enemy prefab already has (today: the Wolf model inside Enemy_Encounter).</summary>
        SharedPrefab = 0,

        /// <summary>Instantiate <see cref="EnemyDefinition.VisualPrefab"/>.</summary>
        DefinitionPrefab = 1,

        /// <summary>Build a tinted primitive placeholder from the definition's placeholder settings.</summary>
        Placeholder = 2,
    }

    /// <summary>
    /// Data-driven enemy visuals. Every spawner reuses one shared enemy prefab (EnemyController +
    /// Health + capsule collider + EnemyProceduralAnimator) and reconfigures it per
    /// <see cref="EnemyDefinition"/> through <c>EnemyController.ResetForSpawn</c>; until this existed
    /// the prefab's hard-wired Wolf model was kept for everything, so a scorpion looked like a wolf.
    /// <see cref="Apply"/> swaps the enemy's <c>EnemyVisual</c> child to match the definition:
    ///
    ///   1. <see cref="EnemyDefinition.VisualPrefab"/> set → that model, placed with the definition's
    ///      visual position/rotation/scale;
    ///   2. otherwise, if <see cref="EnemyDefinition.UsePlaceholderVisual"/> (default) → a collider-less
    ///      primitive (shape/scale/tint per definition, tint auto-derived from the asset name when not
    ///      authored) with a darker "snout" cube marking the front, base resting on the capsule bottom;
    ///   3. otherwise (or a null definition) → the shared prefab's own visual.
    ///
    /// The prefab's original visual is only deactivated, never destroyed, so a pooled enemy can go
    /// back to it; visuals built here carry an <see cref="EnemyVisualMarker"/> and are rebuilt only
    /// when the definition changes. The root's <see cref="EnemyProceduralAnimator"/> (if any) is
    /// re-targeted to the new visual.
    /// </summary>
    public static class EnemyVisualSwapper
    {
        public const string VisualName = "EnemyVisual";
        public const string SnoutName = "Snout";

        /// <summary>Local y of the enemy's feet when the root has no CapsuleCollider (the shared prefab uses a 2 m capsule centred on the root).</summary>
        public const float DefaultFootHeight = -1f;

        private static readonly Dictionary<Color32, Material> PlaceholderMaterials = new Dictionary<Color32, Material>();
        private static readonly Dictionary<Material, Material> BossModelMaterials = new Dictionary<Material, Material>();
        private static readonly List<Renderer> RendererBuffer = new List<Renderer>();

        /// <summary>
        /// Multiplied onto a boss model's material colour so a boss that reuses a regular enemy's
        /// model (Lobo Alfa = the wolf mesh) reads apart at a glance. Components above 1 on purpose:
        /// the wolf texture is near-black, so only an over-bright multiply can lift it to a
        /// red-brown coat (keeping the fur detail). No emission: no project material uses the
        /// _EMISSION keyword, so that URP Lit variant would be stripped from builds.
        /// </summary>
        public static readonly Color BossModelTint = new Color(3.8f, 1.7f, 1.2f, 1f);

        /// <summary>Decides which visual a definition gets. Pure, so it is unit tested directly.</summary>
        public static EnemyVisualSource ResolveSource(EnemyDefinition definition)
        {
            if (definition == null)
            {
                return EnemyVisualSource.SharedPrefab;
            }

            if (definition.VisualPrefab != null)
            {
                return EnemyVisualSource.DefinitionPrefab;
            }

            return definition.UsePlaceholderVisual ? EnemyVisualSource.Placeholder : EnemyVisualSource.SharedPrefab;
        }

        /// <summary>
        /// Placeholder tint: the authored <see cref="EnemyDefinition.PlaceholderColor"/> (forced
        /// opaque) when its alpha is above 0, otherwise a stable hue hashed from the definition id
        /// so two un-authored enemies still get different colors.
        /// </summary>
        public static Color GetPlaceholderColor(EnemyDefinition definition)
        {
            if (definition == null)
            {
                return Color.gray;
            }

            Color authored = definition.PlaceholderColor;
            if (authored.a > 0f)
            {
                authored.a = 1f;
                return authored;
            }

            string id = DefeatedEnemyRegistry.GetId(definition);
            if (string.IsNullOrEmpty(id))
            {
                id = definition.EnemyName ?? string.Empty;
            }

            float hue = (StableHash(id) % 360u) / 360f;
            return Color.HSVToRGB(hue, 0.65f, 0.85f);
        }

        /// <summary>
        /// Makes <paramref name="enemyRoot"/>'s visual match <paramref name="definition"/> (see class
        /// summary). Returns the now-active visual transform, or null when the shared prefab visual
        /// was requested but the prefab has none.
        /// </summary>
        public static Transform Apply(GameObject enemyRoot, EnemyDefinition definition)
        {
            if (enemyRoot == null)
            {
                return null;
            }

            Transform root = enemyRoot.transform;
            EnemyProceduralAnimator animator = enemyRoot.GetComponent<EnemyProceduralAnimator>();
            FindVisuals(root, animator, out Transform original, out EnemyVisualMarker built);

            EnemyVisualSource source = ResolveSource(definition);

            if (source == EnemyVisualSource.SharedPrefab)
            {
                if (built != null)
                {
                    Discard(built.gameObject);
                }

                if (original != null)
                {
                    original.gameObject.SetActive(true);
                }

                Retarget(animator, original);
                return original;
            }

            if (built != null && built.Source == definition && built.Kind == source)
            {
                built.gameObject.SetActive(true);
                if (original != null)
                {
                    original.gameObject.SetActive(false);
                }

                Retarget(animator, built.transform);
                return built.transform;
            }

            if (built != null)
            {
                Discard(built.gameObject);
            }

            if (original != null)
            {
                original.gameObject.SetActive(false);
            }

            GameObject visual = source == EnemyVisualSource.DefinitionPrefab
                ? BuildModel(root, definition)
                : BuildPlaceholder(root, definition);

            visual.AddComponent<EnemyVisualMarker>().Set(definition, source);
            Retarget(animator, visual.transform);
            return visual.transform;
        }

        private static GameObject BuildModel(Transform root, EnemyDefinition definition)
        {
            GameObject visual = Object.Instantiate(definition.VisualPrefab, root, false);
            visual.name = VisualName;
            visual.transform.localPosition = definition.VisualLocalPosition;
            visual.transform.localEulerAngles = definition.VisualLocalEulerAngles;
            visual.transform.localScale = Vector3.one * Mathf.Max(0.0001f, definition.VisualScale);
            GroundModel(root, visual.transform);
            if (definition.IsBoss)
            {
                TintBossModel(visual);
            }

            return visual;
        }

        /// <summary>
        /// Rests a model's mesh bottom on the enemy's feet (<see cref="GetFootHeight"/>) by adjusting
        /// only its local y. The definitions' hand-tuned <c>VisualLocalPosition.y</c> was authored
        /// against one scale; a different <see cref="EnemyDefinition.VisualScale"/> (alpha wolf 0.019,
        /// scorpion 0.018) or a different mesh (spider) left the model sunk into or floating above the
        /// floor. Uses the meshes' own bounds (not <c>Renderer.bounds</c>, which can be stale on the frame
        /// of instantiation and is padded for skinned meshes), measured in the root's space so the root's
        /// rotation/position don't matter. No mesh → the authored position is kept.
        /// </summary>
        public static void GroundModel(Transform root, Transform visual)
        {
            if (root == null || visual == null || !TryGetMeshBoundsInRootSpace(root, visual, out Bounds bounds))
            {
                return;
            }

            Vector3 local = visual.localPosition;
            local.y += GetFootHeight(root) - bounds.min.y;
            visual.localPosition = local;
        }

        /// <summary>Axis-aligned box (in <paramref name="root"/>'s local space) around every mesh under <paramref name="visual"/>.</summary>
        public static bool TryGetMeshBoundsInRootSpace(Transform root, Transform visual, out Bounds bounds)
        {
            bounds = default;
            bool found = false;
            Matrix4x4 worldToRoot = root.worldToLocalMatrix;

            foreach (MeshFilter filter in visual.GetComponentsInChildren<MeshFilter>(true))
            {
                if (filter.sharedMesh != null)
                {
                    Encapsulate(ref bounds, ref found, worldToRoot * filter.transform.localToWorldMatrix, filter.sharedMesh.bounds);
                }
            }

            foreach (SkinnedMeshRenderer skinned in visual.GetComponentsInChildren<SkinnedMeshRenderer>(true))
            {
                if (skinned.sharedMesh != null)
                {
                    Encapsulate(ref bounds, ref found, worldToRoot * skinned.transform.localToWorldMatrix, skinned.sharedMesh.bounds);
                }
            }

            return found;
        }

        private static void Encapsulate(ref Bounds bounds, ref bool found, Matrix4x4 toRoot, Bounds local)
        {
            Vector3 c = local.center;
            Vector3 e = local.extents;
            for (int i = 0; i < 8; i++)
            {
                Vector3 corner = toRoot.MultiplyPoint3x4(c + new Vector3(
                    (i & 1) == 0 ? -e.x : e.x,
                    (i & 2) == 0 ? -e.y : e.y,
                    (i & 4) == 0 ? -e.z : e.z));
                if (!found)
                {
                    bounds = new Bounds(corner, Vector3.zero);
                    found = true;
                }
                else
                {
                    bounds.Encapsulate(corner);
                }
            }
        }

        private static GameObject BuildPlaceholder(Transform root, EnemyDefinition definition)
        {
            EnemyPlaceholderShape shape = definition.PlaceholderShape;
            Vector3 scale = SanitizeScale(definition.PlaceholderScale);
            Color color = GetPlaceholderColor(definition);

            GameObject body = CreateColliderlessPrimitive(ToPrimitive(shape));
            body.name = VisualName;
            body.transform.SetParent(root, false);
            body.transform.localRotation = Quaternion.identity;
            body.transform.localScale = scale;

            float halfHeight = GetPrimitiveHalfHeight(shape);
            body.transform.localPosition = new Vector3(0f, GetFootHeight(root) + halfHeight * scale.y, 0f);
            Tint(body, color);

            // Small darker cube on the front face (+Z, the direction EnemyController faces) so the
            // heading reads; sized in world units by undoing the body's (possibly non-uniform) scale.
            GameObject snout = CreateColliderlessPrimitive(PrimitiveType.Cube);
            snout.name = SnoutName;
            snout.transform.SetParent(body.transform, false);
            const float snoutSize = 0.25f;
            snout.transform.localScale = new Vector3(snoutSize / scale.x, snoutSize / scale.y, snoutSize / scale.z);
            snout.transform.localPosition = new Vector3(0f, halfHeight * 0.35f, 0.5f);
            Tint(snout, new Color(color.r * 0.4f, color.g * 0.4f, color.b * 0.4f, 1f));

            return body;
        }

        private static void FindVisuals(Transform root, EnemyProceduralAnimator animator, out Transform original, out EnemyVisualMarker built)
        {
            original = null;
            built = null;

            for (int i = 0; i < root.childCount; i++)
            {
                Transform child = root.GetChild(i);
                EnemyVisualMarker marker = child.GetComponent<EnemyVisualMarker>();

                if (marker != null)
                {
                    built ??= marker;
                }
                else if (original == null && child.name == VisualName)
                {
                    original = child;
                }
            }

            // A differently named original visual is still found through the animator wiring.
            if (original == null && animator != null)
            {
                Transform animated = animator.VisualRoot;
                if (animated != null && animated != root && animated.parent == root && animated.GetComponent<EnemyVisualMarker>() == null)
                {
                    original = animated;
                }
            }
        }

        private static void Retarget(EnemyProceduralAnimator animator, Transform visual)
        {
            if (animator != null && visual != null && animator.VisualRoot != visual)
            {
                animator.SetVisualRoot(visual);
            }
        }

        private static void Discard(GameObject visual)
        {
            // Deactivate and unparent first: Destroy() only takes effect at the end of the frame (same
            // reasoning as PlayerVisualSwapper), and the next FindVisuals must not see it.
            visual.SetActive(false);
            visual.transform.SetParent(null);

            if (Application.isPlaying)
            {
                Object.Destroy(visual);
            }
            else
            {
                Object.DestroyImmediate(visual);
            }
        }

        private static GameObject CreateColliderlessPrimitive(PrimitiveType type)
        {
            GameObject primitive = GameObject.CreatePrimitive(type);
            Collider collider = primitive.GetComponent<Collider>();
            if (collider != null)
            {
                // Immediate: a stray collider would block the enemy's own capsule/hurtbox for a frame.
                Object.DestroyImmediate(collider);
            }

            return primitive;
        }

        /// <summary>
        /// Swaps every renderer's materials under <paramref name="visual"/> for cached copies tinted by
        /// <see cref="BossModelTint"/>. Shared material copies (not a MaterialPropertyBlock) for the same
        /// reason as <see cref="Tint"/>: EnemyDeathVfx fades from sharedMaterial.color.
        /// </summary>
        public static void TintBossModel(GameObject visual)
        {
            if (visual == null)
            {
                return;
            }

            visual.GetComponentsInChildren(true, RendererBuffer);
            foreach (Renderer renderer in RendererBuffer)
            {
                Material[] materials = renderer.sharedMaterials;
                bool changed = false;
                for (int i = 0; i < materials.Length; i++)
                {
                    Material source = materials[i];
                    if (source == null)
                    {
                        continue;
                    }

                    if (!BossModelMaterials.TryGetValue(source, out Material tinted) || tinted == null)
                    {
                        tinted = new Material(source) { name = source.name + "_Boss" };
                        if (tinted.HasProperty("_BaseColor") || tinted.HasProperty("_Color"))
                        {
                            tinted.color = source.color * BossModelTint;
                        }

                        BossModelMaterials[source] = tinted;
                    }

                    materials[i] = tinted;
                    changed = true;
                }

                if (changed)
                {
                    renderer.sharedMaterials = materials;
                }
            }

            RendererBuffer.Clear();
        }

        private static void Tint(GameObject target, Color color)
        {
            Renderer renderer = target.GetComponent<Renderer>();
            if (renderer == null)
            {
                return;
            }

            // A shared material per color (not a MaterialPropertyBlock) because EnemyDeathVfx fades
            // from renderer.sharedMaterial.color — a property block tint would flash to the default
            // material color while the enemy sinks.
            Color32 key = color;
            if (!PlaceholderMaterials.TryGetValue(key, out Material material) || material == null)
            {
                Material template = renderer.sharedMaterial;
                if (template == null)
                {
                    Shader shader = Shader.Find("Universal Render Pipeline/Lit");
                    if (shader == null)
                    {
                        return;
                    }

                    template = new Material(shader);
                }

                material = new Material(template) { name = $"EnemyPlaceholder_{key.r:X2}{key.g:X2}{key.b:X2}" };
                material.color = color;
                PlaceholderMaterials[key] = material;
            }

            renderer.sharedMaterial = material;
        }

        /// <summary>
        /// Local y of the enemy's feet: the bottom of the root's vertical CapsuleCollider (the shared
        /// prefab's pivot is the capsule centre, so this is -1 for its 2 m capsule), else
        /// <see cref="DefaultFootHeight"/>. Spawners lift the root by its negation to stand on the ground.
        /// </summary>
        public static float GetFootHeight(Transform root)
        {
            CapsuleCollider capsule = root.GetComponent<CapsuleCollider>();
            if (capsule == null || capsule.direction != 1)
            {
                return DefaultFootHeight;
            }

            float height = Mathf.Max(capsule.height, capsule.radius * 2f);
            return capsule.center.y - height * 0.5f;
        }

        /// <summary>Half the height of Unity's built-in primitive mesh at scale 1.</summary>
        public static float GetPrimitiveHalfHeight(EnemyPlaceholderShape shape)
        {
            return shape == EnemyPlaceholderShape.Capsule || shape == EnemyPlaceholderShape.Cylinder ? 1f : 0.5f;
        }

        public static PrimitiveType ToPrimitive(EnemyPlaceholderShape shape)
        {
            switch (shape)
            {
                case EnemyPlaceholderShape.Cube:
                    return PrimitiveType.Cube;
                case EnemyPlaceholderShape.Sphere:
                    return PrimitiveType.Sphere;
                case EnemyPlaceholderShape.Cylinder:
                    return PrimitiveType.Cylinder;
                default:
                    return PrimitiveType.Capsule;
            }
        }

        private static Vector3 SanitizeScale(Vector3 scale)
        {
            return new Vector3(
                scale.x > 0f ? scale.x : 1f,
                scale.y > 0f ? scale.y : 1f,
                scale.z > 0f ? scale.z : 1f);
        }

        /// <summary>FNV-1a; string.GetHashCode is not guaranteed stable across runtimes/sessions.</summary>
        private static uint StableHash(string value)
        {
            uint hash = 2166136261u;
            foreach (char c in value)
            {
                hash ^= c;
                hash *= 16777619u;
            }

            return hash;
        }
    }
}
