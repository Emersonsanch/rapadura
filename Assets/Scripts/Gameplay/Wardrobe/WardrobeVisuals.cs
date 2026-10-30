using System;
using System.Collections.Generic;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Items;
using UnityEngine;

namespace Rapadura.Gameplay.Wardrobe
{
    /// <summary>
    /// The v1 art of the wardrobe (Housing.md §9.6), kept honest: the heroes are single Meshy meshes (<c>*_RO</c>), with no
    /// separate head/torso/legs pieces and no modular clothing meshes, so a look cannot swap a garment. What it does:
    /// <list type="bullet">
    /// <item><b>Peito (chest) look → body tint:</b> the hero's renderers are washed with the look's colour (its primary dye,
    /// else a colour per rarity) through a per-renderer <see cref="MaterialPropertyBlock"/>; shared materials untouched.
    /// The graduation tint of <c>ClassEvolutionVisuals</c> is kept as the base when it is there.</item>
    /// <item><b>Cabeça / Rosto-Pescoço / Costas looks → bone attachments:</b> a small low-poly hat, scarf or back bundle
    /// built from primitives, parented to the humanoid head / neck / chest bone (or placed from the model's bounds when the
    /// rig is not humanoid), coloured by the primary dye (detail dye on the trim). "Esconder" removes it.</item>
    /// <item><b>Weapon skin → tint</b> of child renderers whose name looks like a weapon (none on today's heroes).</item>
    /// </list>
    /// Pernas, Pés, Acessório and Caramelo looks are recorded and shown in the wardrobe, with no 3D change yet (v2 = modular meshes).
    /// </summary>
    public static class WardrobeVisuals
    {
        public const string AttachmentPrefix = "WardrobeLook_";
        public const float LookTintStrength = 0.22f;
        public const float DyeTintStrength = 0.4f;
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorId = Shader.PropertyToID("_Color");
        private static readonly string[] WeaponWords = { "weapon", "arma", "bow", "arco", "staff", "cajado", "shield", "escudo", "sword", "espada", "orb" };
        private static MaterialPropertyBlock _block;

        /// <summary>Colour of a rarity when the look has no dye (linen, leaf, sky, lilac, gold).</summary>
        public static Color RarityColor(ItemRarity rarity)
        {
            switch (rarity)
            {
                case ItemRarity.Uncommon: return new Color(0.42f, 0.62f, 0.32f);
                case ItemRarity.Rare: return new Color(0.33f, 0.52f, 0.78f);
                case ItemRarity.Epic: return new Color(0.6f, 0.42f, 0.75f);
                case ItemRarity.Legendary: return new Color(0.9f, 0.72f, 0.28f);
                default: return new Color(0.82f, 0.74f, 0.6f);
            }
        }

        public static bool TryParseHex(string hex, out Color color)
        {
            color = Color.white;
            if (string.IsNullOrEmpty(hex) || hex.Length != 6) return false;
            if (!int.TryParse(hex, System.Globalization.NumberStyles.HexNumber, null, out int rgb)) return false;
            color = new Color(((rgb >> 16) & 0xFF) / 255f, ((rgb >> 8) & 0xFF) / 255f, (rgb & 0xFF) / 255f);
            return true;
        }

        /// <summary>Colour of a channel: its dye when painted, else (primary) the rarity colour or (detail) a darker one.</summary>
        public static Color ChannelColor(WardrobeEntry look, string dyeId, bool detail)
        {
            WardrobeEntry dye = WardrobeCatalog.Get(dyeId);
            if (dye != null && TryParseHex(dye.ColorHex, out Color painted)) return painted;
            Color baseColor = RarityColor(look != null ? look.Rarity : ItemRarity.Common);
            return detail ? baseColor * 0.6f : baseColor;
        }

        /// <summary>Pure: the colour a renderer whose own colour is <paramref name="original"/> shows under the chest look.</summary>
        public static Color TintedColor(Color original, Color tint, bool dyed)
        {
            Color mixed = Color.Lerp(original, tint, dyed ? DyeTintStrength : LookTintStrength);
            mixed.a = original.a;
            return mixed;
        }

        /// <summary>Applies <paramref name="cousin"/>'s wardrobe (from <paramref name="book"/>) on the hero model <paramref name="root"/>.</summary>
        public static void Apply(GameObject root, CharacterId cousin, WardrobeBook book)
        {
            if (root == null || book == null) return;
            RemoveAttachments(root.transform);
            if (!root.TryGetComponent(out WardrobeTintMemory memory)) memory = root.AddComponent<WardrobeTintMemory>();

            WardrobeEntry chest = book.LookOn(cousin, AppearanceSlot.Chest);
            WardrobeSlotChoice chestChoice = book.Choice(cousin, AppearanceSlot.Chest);
            bool chestDyed = chest != null && !string.IsNullOrEmpty(chestChoice?.dyePrimary);
            Color? bodyTint = chest != null ? ChannelColor(chest, chestChoice?.dyePrimary, false) : (Color?)null;

            WardrobeEntry weapon = book.LookOn(cousin, AppearanceSlot.Weapon);
            WardrobeSlotChoice weaponChoice = book.Choice(cousin, AppearanceSlot.Weapon);
            Color? weaponTint = weapon != null ? ChannelColor(weapon, weaponChoice?.dyePrimary, false) : (Color?)null;

            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
            Bounds bounds = default;
            bool hasBounds = false;
            Material source = null;
            _block ??= new MaterialPropertyBlock();
            foreach (Renderer renderer in renderers)
            {
                if (renderer == null || renderer is ParticleSystemRenderer || renderer is TrailRenderer || renderer is LineRenderer
                    || renderer.name.StartsWith(AttachmentPrefix, StringComparison.Ordinal) || renderer.name.StartsWith("ClassAccessory_", StringComparison.Ordinal))
                {
                    continue;
                }

                if (hasBounds) bounds.Encapsulate(renderer.bounds);
                else
                {
                    bounds = renderer.bounds;
                    hasBounds = true;
                }

                Material material = renderer.sharedMaterial;
                if (material == null) continue;
                source ??= material;
                bool isWeapon = LooksLikeWeapon(renderer.transform, root.transform);
                Color? tint = isWeapon ? weaponTint : bodyTint;
                Color baseColor = memory.BaseColor(renderer, material, _block);
                renderer.GetPropertyBlock(_block);
                Color shown = tint.HasValue ? TintedColor(baseColor, tint.Value, isWeapon || chestDyed) : baseColor;
                _block.SetColor(BaseColorId, shown);
                _block.SetColor(ColorId, shown);
                renderer.SetPropertyBlock(_block);
                memory.Remember(renderer, shown, baseColor);
            }

            float height = hasBounds ? Mathf.Max(0.5f, bounds.size.y) : 1.6f;
            float bottom = hasBounds ? bounds.min.y - root.transform.position.y : 0f;
            Animator animator = root.GetComponentInChildren<Animator>();
            Attach(root.transform, animator, HumanBodyBones.Head, AppearanceSlot.Head, cousin, book, height, bottom, source);
            Attach(root.transform, animator, HumanBodyBones.Neck, AppearanceSlot.FaceNeck, cousin, book, height, bottom, source);
            Attach(root.transform, animator, HumanBodyBones.Chest, AppearanceSlot.Back, cousin, book, height, bottom, source);
        }

        public static bool LooksLikeWeapon(Transform t, Transform root)
        {
            for (Transform cursor = t; cursor != null && cursor != root; cursor = cursor.parent)
            {
                string name = cursor.name.ToLowerInvariant();
                foreach (string word in WeaponWords)
                {
                    if (name.Contains(word)) return true;
                }
            }

            return false;
        }

        public static void RemoveAttachments(Transform root)
        {
            if (root == null) return;
            var doomed = new List<GameObject>();
            foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
            {
                if (child != root && child.name.StartsWith(AttachmentPrefix, StringComparison.Ordinal) && (child.parent == null || !child.parent.name.StartsWith(AttachmentPrefix, StringComparison.Ordinal)))
                {
                    doomed.Add(child.gameObject);
                }
            }

            foreach (GameObject go in doomed) DestroySafe(go);
        }

        private static void Attach(Transform root, Animator animator, HumanBodyBones bone, AppearanceSlot slot, CharacterId cousin, WardrobeBook book,
            float h, float bottom, Material source)
        {
            WardrobeEntry look = book.LookOn(cousin, slot);
            if (look == null) return;
            WardrobeSlotChoice choice = book.Choice(cousin, slot);
            Color main = ChannelColor(look, choice?.dyePrimary, false);
            Color trim = ChannelColor(look, choice?.dyeDetail, true);

            Transform boneTransform = animator != null && animator.isHuman ? animator.GetBoneTransform(bone) : null;
            var holder = new GameObject(AttachmentPrefix + slot + "_" + look.Id);
            Vector3 anchor;
            if (boneTransform != null)
            {
                holder.transform.SetParent(boneTransform, false);
                holder.transform.position = boneTransform.position;
                holder.transform.rotation = root.rotation;
                Vector3 lossy = boneTransform.lossyScale;
                float s = Mathf.Approximately(lossy.y, 0f) ? 1f : 1f / Mathf.Abs(lossy.y);
                holder.transform.localScale = Vector3.one * s;
                anchor = Vector3.zero;
            }
            else
            {
                holder.transform.SetParent(root, false);
                float y = slot == AppearanceSlot.Head ? bottom + 0.94f * h : slot == AppearanceSlot.FaceNeck ? bottom + 0.8f * h : bottom + 0.62f * h;
                anchor = new Vector3(0f, y, 0f);
            }

            switch (slot)
            {
                case AppearanceSlot.Head:
                    Part(holder, PrimitiveType.Cylinder, anchor + new Vector3(0f, 0.08f * h, 0f), new Vector3(0.34f * h, 0.01f * h, 0.34f * h), main, source);
                    Part(holder, PrimitiveType.Cylinder, anchor + new Vector3(0f, 0.12f * h, 0f), new Vector3(0.17f * h, 0.045f * h, 0.17f * h), main * 0.9f, source);
                    Part(holder, PrimitiveType.Cylinder, anchor + new Vector3(0f, 0.095f * h, 0f), new Vector3(0.175f * h, 0.012f * h, 0.175f * h), trim, source);
                    break;
                case AppearanceSlot.FaceNeck:
                    Part(holder, PrimitiveType.Cylinder, anchor, new Vector3(0.16f * h, 0.022f * h, 0.14f * h), main, source);
                    Part(holder, PrimitiveType.Cube, anchor + new Vector3(0.02f * h, -0.06f * h, 0.07f * h), new Vector3(0.035f * h, 0.09f * h, 0.01f * h), trim, source);
                    break;
                case AppearanceSlot.Back:
                    Part(holder, PrimitiveType.Cube, anchor + new Vector3(0f, 0f, -0.1f * h), new Vector3(0.2f * h, 0.24f * h, 0.08f * h), main, source);
                    Part(holder, PrimitiveType.Cube, anchor + new Vector3(0f, 0.08f * h, -0.1f * h), new Vector3(0.21f * h, 0.03f * h, 0.085f * h), trim, source);
                    break;
            }
        }

        private static void Part(GameObject holder, PrimitiveType type, Vector3 localPosition, Vector3 scale, Color color, Material source)
        {
            GameObject part = GameObject.CreatePrimitive(type);
            part.name = AttachmentPrefix + "part";
            Collider collider = part.GetComponent<Collider>();
            if (collider != null) DestroySafe(collider);
            part.transform.SetParent(holder.transform, false);
            part.transform.localPosition = localPosition;
            part.transform.localScale = scale;
            Renderer renderer = part.GetComponent<Renderer>();
            if (renderer == null || source == null) return;
            var material = new Material(source) { name = AttachmentPrefix + "mat" };
            if (material.HasProperty(BaseColorId)) material.SetColor(BaseColorId, color);
            if (material.HasProperty(ColorId)) material.SetColor(ColorId, color);
            renderer.sharedMaterial = material;
        }

        private static void DestroySafe(UnityEngine.Object target)
        {
            if (target == null) return;
            if (Application.isPlaying) UnityEngine.Object.Destroy(target);
            else UnityEngine.Object.DestroyImmediate(target);
        }
    }
}
