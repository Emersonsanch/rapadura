using Rapadura.Gameplay.Characters;
using UnityEngine;

namespace Rapadura.Gameplay.ClassEvolution
{
    /// <summary>
    /// The visual change of a graduation (Docs/ClassEvolution.md §1.3), kept honest: the existing model is not replaced.
    /// Each tier washes the hero's renderers lightly with the tier colour (per-renderer <see cref="MaterialPropertyBlock"/>,
    /// shared materials untouched) and adds one small low-poly accessory built from primitives (hat, scarf, feather,
    /// ribbon, sash, satchel, halo) placed from the model's bounds. Graduations stack: tier 3 wears all three.
    /// </summary>
    public static class ClassEvolutionVisuals
    {
        public const string AccessoryPrefix = "ClassAccessory_";
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorId = Shader.PropertyToID("_Color");
        private static MaterialPropertyBlock _block;

        /// <summary>Pure: the colour a renderer whose own colour is <paramref name="original"/> shows at this tier.</summary>
        public static Color TintedColor(Color original, GraduationSpec spec)
        {
            if (spec == null)
            {
                return original;
            }

            Color tint = new Color(spec.TintR, spec.TintG, spec.TintB, original.a);
            Color mixed = Color.Lerp(original, tint, spec.TintStrength);
            mixed.a = original.a;
            return mixed;
        }

        /// <summary>Applies (or clears, at tier 0) the graduation look of <paramref name="hero"/> on <paramref name="root"/>.</summary>
        public static void Apply(GameObject root, CharacterId hero, int tier)
        {
            if (root == null)
            {
                return;
            }

            RemoveAccessories(root.transform);
            tier = ClassEvolutionRules.ClampTier(tier);
            GraduationSpec top = tier > 0 ? ClassEvolutionCatalog.GetGraduation(hero, tier) : null;

            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
            Bounds bounds = default;
            bool hasBounds = false;
            _block ??= new MaterialPropertyBlock();
            foreach (Renderer renderer in renderers)
            {
                if (renderer == null || renderer is ParticleSystemRenderer || renderer is TrailRenderer || renderer is LineRenderer
                    || renderer.name.StartsWith(AccessoryPrefix))
                {
                    continue;
                }

                if (hasBounds)
                {
                    bounds.Encapsulate(renderer.bounds);
                }
                else
                {
                    bounds = renderer.bounds;
                    hasBounds = true;
                }

                Material material = renderer.sharedMaterial;
                if (top == null || material == null)
                {
                    renderer.SetPropertyBlock(null);
                    continue;
                }

                Color original = material.HasProperty(BaseColorId) ? material.GetColor(BaseColorId)
                    : material.HasProperty(ColorId) ? material.GetColor(ColorId) : Color.white;
                Color tinted = TintedColor(original, top);
                renderer.GetPropertyBlock(_block);
                _block.SetColor(BaseColorId, tinted);
                _block.SetColor(ColorId, tinted);
                renderer.SetPropertyBlock(_block);
            }

            if (top == null)
            {
                return;
            }

            float height = hasBounds ? Mathf.Max(0.5f, bounds.size.y) : 1.6f;
            float bottom = hasBounds ? bounds.min.y - root.transform.position.y : 0f;
            Material source = FirstMaterial(renderers);
            for (int t = 1; t <= tier; t++)
            {
                GraduationSpec spec = ClassEvolutionCatalog.GetGraduation(hero, t);
                if (spec != null)
                {
                    BuildAccessory(root.transform, spec, height, bottom, source);
                }
            }
        }

        public static void RemoveAccessories(Transform root)
        {
            if (root == null)
            {
                return;
            }

            for (int i = root.childCount - 1; i >= 0; i--)
            {
                Transform child = root.GetChild(i);
                if (child.name.StartsWith(AccessoryPrefix))
                {
                    DestroySafe(child.gameObject);
                }
            }
        }

        private static Material FirstMaterial(Renderer[] renderers)
        {
            foreach (Renderer renderer in renderers)
            {
                if (renderer != null && renderer.sharedMaterial != null && !(renderer is ParticleSystemRenderer))
                {
                    return renderer.sharedMaterial;
                }
            }

            return null;
        }

        private static void BuildAccessory(Transform root, GraduationSpec spec, float h, float bottom, Material source)
        {
            var holder = new GameObject(AccessoryPrefix + spec.Tier + "_" + spec.AccessoryId);
            holder.transform.SetParent(root, false);
            Color color = new Color(spec.TintR, spec.TintG, spec.TintB);
            float top = bottom + h;
            switch (spec.Accessory)
            {
                case AccessoryShape.Hat:
                    Part(holder, PrimitiveType.Cylinder, new Vector3(0f, top - 0.02f * h, 0f), new Vector3(0.42f * h, 0.012f * h, 0.42f * h), Vector3.zero, color, source);
                    Part(holder, PrimitiveType.Cylinder, new Vector3(0f, top + 0.03f * h, 0f), new Vector3(0.2f * h, 0.05f * h, 0.2f * h), Vector3.zero, color * 0.85f, source);
                    break;
                case AccessoryShape.Halo:
                    Part(holder, PrimitiveType.Cylinder, new Vector3(0f, top - 0.06f * h, 0f), new Vector3(0.24f * h, 0.008f * h, 0.24f * h), Vector3.zero, color, source);
                    break;
                case AccessoryShape.Feather:
                    Part(holder, PrimitiveType.Cube, new Vector3(0.08f * h, top - 0.02f * h, -0.04f * h), new Vector3(0.015f * h, 0.16f * h, 0.045f * h), new Vector3(0f, 0f, -20f), color, source);
                    break;
                case AccessoryShape.Scarf:
                    Part(holder, PrimitiveType.Cylinder, new Vector3(0f, top - 0.2f * h, 0f), new Vector3(0.2f * h, 0.025f * h, 0.17f * h), Vector3.zero, color, source);
                    Part(holder, PrimitiveType.Cube, new Vector3(0.03f * h, top - 0.27f * h, -0.09f * h), new Vector3(0.04f * h, 0.12f * h, 0.012f * h), new Vector3(0f, 0f, 10f), color, source);
                    break;
                case AccessoryShape.Ribbon:
                    Part(holder, PrimitiveType.Cube, new Vector3(-0.15f * h, bottom + 0.5f * h, 0f), new Vector3(0.05f * h, 0.03f * h, 0.05f * h), Vector3.zero, color, source);
                    Part(holder, PrimitiveType.Cube, new Vector3(-0.15f * h, bottom + 0.46f * h, -0.02f * h), new Vector3(0.015f * h, 0.07f * h, 0.01f * h), new Vector3(0f, 0f, 15f), color, source);
                    break;
                case AccessoryShape.Sash:
                    Part(holder, PrimitiveType.Cube, new Vector3(0f, bottom + 0.62f * h, 0.07f * h), new Vector3(0.05f * h, 0.36f * h, 0.012f * h), new Vector3(0f, 0f, 35f), color, source);
                    break;
                case AccessoryShape.Pouch:
                    Part(holder, PrimitiveType.Cube, new Vector3(0.12f * h, bottom + 0.48f * h, -0.06f * h), new Vector3(0.08f * h, 0.08f * h, 0.04f * h), Vector3.zero, color, source);
                    break;
            }
        }

        private static void Part(GameObject holder, PrimitiveType type, Vector3 localPosition, Vector3 scale, Vector3 euler, Color color, Material source)
        {
            GameObject part = GameObject.CreatePrimitive(type);
            part.name = AccessoryPrefix + "part";
            Collider collider = part.GetComponent<Collider>();
            if (collider != null)
            {
                DestroySafe(collider);
            }

            part.transform.SetParent(holder.transform, false);
            part.transform.localPosition = localPosition;
            part.transform.localRotation = Quaternion.Euler(euler);
            part.transform.localScale = scale;
            Renderer renderer = part.GetComponent<Renderer>();
            if (renderer != null && source != null)
            {
                var material = new Material(source) { name = AccessoryPrefix + "mat" };
                if (material.HasProperty(BaseColorId))
                {
                    material.SetColor(BaseColorId, color);
                }

                if (material.HasProperty(ColorId))
                {
                    material.SetColor(ColorId, color);
                }

                renderer.sharedMaterial = material;
            }
        }

        private static void DestroySafe(Object target)
        {
            if (Application.isPlaying)
            {
                Object.Destroy(target);
            }
            else
            {
                Object.DestroyImmediate(target);
            }
        }
    }
}
