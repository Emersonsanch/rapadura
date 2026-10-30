using System.Collections;
using System.Collections.Generic;
using Rapadura.Core.DI;
using Rapadura.Core.Events;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Combat;
using Rapadura.Gameplay.Content;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Inventory;
using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.Quests;
using Rapadura.Gameplay.Skills;
using Rapadura.Gameplay.World.Biomes;
using UnityEngine;

namespace Rapadura.Gameplay.Night
{
    /// <summary>
    /// Turns one arena enemy into its night variant or a night species (<see cref="NightLook"/>), the same way the road
    /// event's elite is made (<c>WorldEventArenaHooks.MakeElite</c>): a frame after spawn (EnemyController.Start
    /// re-applies the definition's health) it multiplies the health, adds a PercentAdditive AttackDamage buff, scales
    /// the visual, recolours its materials and adds two small unlit glowing eyes. While alive it engages the hero from
    /// <see cref="NightLook.AggroRadiusMultiplier"/>× the definition's detection radius. On death it pays the bonus XP and
    /// the extra loot rolls (the normal kill XP/loot still come from EnemyRewardGranter/EnemyLootGranter).
    /// Added by <c>EncounterArenaController</c> through <see cref="Attach"/>; never on bosses.
    /// </summary>
    public class NightVariantTag : MonoBehaviour
    {
        public const string BuffSource = "night_variant";
        public const string EyesName = "NightEyes";
        private const string PlayerTag = "Player";

        private static readonly Dictionary<(Material, Color32), Material> TintedMaterials = new Dictionary<(Material, Color32), Material>();
        private static readonly Dictionary<Color32, Material> EyeMaterials = new Dictionary<Color32, Material>();
        private static readonly List<Renderer> RendererBuffer = new List<Renderer>();

        private readonly System.Random _random = new System.Random();
        private EnemyController _controller;
        private GameObject _eyes;
        private bool _applied;
        private bool _rewarded;

        public NightLook Look { get; private set; }
        public bool IsApplied => _applied;
        public GameObject Eyes => _eyes;

        /// <summary>Adds (or re-uses) the tag on <paramref name="enemy"/> with <paramref name="look"/>. Normal looks and bosses are ignored (returns null).</summary>
        public static NightVariantTag Attach(GameObject enemy, NightLook look, bool applyNow = false)
        {
            if (enemy == null || look.IsNormal)
            {
                return null;
            }

            EnemyController controller = enemy.GetComponent<EnemyController>();
            if (controller != null && controller.IsBoss)
            {
                return null;
            }

            NightVariantTag tag = enemy.GetComponent<NightVariantTag>();
            if (tag == null)
            {
                tag = enemy.AddComponent<NightVariantTag>();
            }

            tag.Look = look;
            tag._controller = controller;
            if (applyNow)
            {
                tag.ApplyNow();
            }

            return tag;
        }

        /// <summary>The name for this enemy: "Lobo do Tugai (da noite)" or the species name.</summary>
        public static string DisplayNameFor(EnemyDefinition definition, NightLook look, LocalizationManager localization)
        {
            string baseName = definition != null ? definition.LocalizedName(localization) : string.Empty;
            string speciesName = look.Species != null ? NightTexts.Get(localization, look.Species.NameKey) : null;
            return NightVariantRules.DisplayName(baseName, look, NightTexts.Get(localization, NightTexts.VariantName), speciesName);
        }

        public string DisplayName
        {
            get
            {
                ServiceLocator.TryGet(out LocalizationManager localization);
                return DisplayNameFor(_controller != null ? _controller.Definition : null, Look, localization);
            }
        }

        private void OnEnable()
        {
            EventBus.Subscribe<CombatTargetDiedEvent>(OnCombatTargetDied);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<CombatTargetDiedEvent>(OnCombatTargetDied);
        }

        private IEnumerator Start()
        {
            yield return null;
            ApplyNow();
        }

        /// <summary>Applies stats + look once (idempotent). Public for tests.</summary>
        public void ApplyNow()
        {
            if (_applied || Look.IsNormal)
            {
                return;
            }

            _applied = true;
            if (_controller == null)
            {
                _controller = GetComponent<EnemyController>();
            }

            Health health = _controller != null ? _controller.Health : GetComponent<Health>();
            if (health == null)
            {
                health = GetComponent<Health>();
            }

            if (health != null)
            {
                health.ConfigureMaxHealth(health.MaxHealth * Look.HealthMultiplier);
            }

            BuffController buffs = _controller != null ? _controller.Buffs : GetComponent<BuffController>();
            if (buffs != null && !Mathf.Approximately(Look.DamageBonus, 0f))
            {
                buffs.ApplyEffect(new StatModifierDefinition
                {
                    displayName = BuffSource,
                    affectedStat = StatType.AttackDamage,
                    application = ModifierApplication.PercentAdditive,
                    value = Look.DamageBonus,
                    durationSeconds = 0f,
                }, BuffSource);
            }

            Transform visual = transform.Find(EnemyVisualSwapper.VisualName);
            Transform scaled = visual != null ? visual : transform;
            scaled.localScale *= Look.ScaleMultiplier;

            GameObject tintRoot = visual != null ? visual.gameObject : gameObject;
            TintModel(tintRoot, Look.Tint);
            _eyes = AddEyes(tintRoot.transform, transform, Look.EyeColor);

            if (Application.isPlaying)
            {
                ServiceLocator.TryGet(out LocalizationManager localization);
                CombatFxManager.SpawnLootWarningText(transform.position + Vector3.up * 2.4f, DisplayNameFor(_controller != null ? _controller.Definition : null, Look, localization));
            }
        }

        private void Update()
        {
            if (!_applied || _controller == null || _controller.Definition == null || _controller.IsAggroLocked || _controller.IsHoldingAggro)
            {
                return;
            }

            Health health = _controller.Health;
            if (health != null && health.IsDead)
            {
                return;
            }

            Transform target = _controller.Target;
            if (target == null)
            {
                return;
            }

            float radius = _controller.Definition.DetectionRadius * Look.AggroRadiusMultiplier;
            Vector3 offset = target.position - transform.position;
            offset.y = 0f;
            if (offset.sqrMagnitude <= radius * radius)
            {
                _controller.ForceAggro();
            }
        }

        private void OnCombatTargetDied(CombatTargetDiedEvent evt)
        {
            if (evt.Target != gameObject || _rewarded || !_applied)
            {
                return;
            }

            _rewarded = true;
            if (_eyes != null)
            {
                _eyes.SetActive(false);
            }

            EnemyDefinition definition = _controller != null ? _controller.Definition : null;
            GameObject player = GameObject.FindGameObjectWithTag(PlayerTag);
            if (definition == null || player == null)
            {
                return;
            }

            PlayerStats stats = player.GetComponent<PlayerStats>();
            int bonusXp = NightVariantRules.BonusExperience(definition.ExperienceReward, Look);
            if (stats != null && bonusXp > 0)
            {
                stats.AddExperience(bonusXp);
            }

            InventoryManager inventory = player.GetComponent<InventoryManager>();
            if (inventory == null || Look.ExtraLootRolls <= 0)
            {
                return;
            }

            System.Func<string, int, int> outstanding = ServiceLocator.TryGet(out QuestManager quests) && quests != null
                ? quests.GetOutstandingCollectAmount
                : (System.Func<string, int, int>)null;

            BiomeDefinition biome = BiomeRegistry.FindBiomeOf(definition);
            Vector3 labelPosition = transform.position + Vector3.up * 2.9f;
            int line = 0;
            for (int i = 0; i < Look.ExtraLootRolls; i++)
            {
                foreach (LootDrop drop in EnemyLootRoller.Roll(definition, biome, _random))
                {
                    LootGrantResult result = LootGrantRules.Grant(inventory, drop, outstanding);
                    if (result.Granted > 0)
                    {
                        string name = drop.IsGenerated ? drop.Generated.GeneratedName : drop.Item.LocalizedName();
                        CombatFxManager.SpawnLootText(labelPosition + Vector3.up * (0.35f * line++),
                            result.Granted > 1 ? $"+{result.Granted} {name}" : "+ " + name);
                    }
                }
            }
        }

        /// <summary>
        /// Recolours every renderer under <paramref name="root"/> with shared per-(material, tint) copies (not property
        /// blocks: EnemyDeathVfx fades from sharedMaterial.color, like the boss tint). The eyes are added afterwards so
        /// they keep their own colour.
        /// </summary>
        public static void TintModel(GameObject root, Color tint)
        {
            if (root == null || tint == Color.white)
            {
                return;
            }

            Color32 key = tint;
            root.GetComponentsInChildren(true, RendererBuffer);
            foreach (Renderer renderer in RendererBuffer)
            {
                if (renderer == null || renderer.gameObject.name == EyesName || (renderer.transform.parent != null && renderer.transform.parent.name == EyesName))
                {
                    continue;
                }

                Material[] materials = renderer.sharedMaterials;
                bool changed = false;
                for (int i = 0; i < materials.Length; i++)
                {
                    Material source = materials[i];
                    if (source == null)
                    {
                        continue;
                    }

                    if (!TintedMaterials.TryGetValue((source, key), out Material tinted) || tinted == null)
                    {
                        tinted = new Material(source) { name = source.name + "_Night" };
                        if (tinted.HasProperty("_BaseColor") || tinted.HasProperty("_Color"))
                        {
                            tinted.color = source.color * tint;
                        }

                        TintedMaterials[(source, key)] = tinted;
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

        /// <summary>
        /// Two small unlit spheres at the front-top of the model's bounds, parented to <paramref name="parent"/>. Unlit, so
        /// they stay bright in the dark and in fog: the "eyes in the dark" warning. Returns the eyes root (null if no colour).
        /// </summary>
        public static GameObject AddEyes(Transform parent, Transform enemyRoot, Color color)
        {
            if (parent == null || color.a <= 0f)
            {
                return null;
            }

            Transform existing = parent.Find(EyesName);
            if (existing != null)
            {
                return existing.gameObject;
            }

            Bounds bounds = ComputeBounds(parent, enemyRoot);
            Vector3 forward = enemyRoot != null ? enemyRoot.forward : parent.forward;
            Vector3 right = enemyRoot != null ? enemyRoot.right : parent.right;
            float size = Mathf.Clamp(bounds.size.y * 0.09f, 0.06f, 0.3f);
            float spread = Mathf.Clamp(bounds.extents.x * 0.35f, size, size * 3f);
            float forwardReach = Mathf.Max(Mathf.Abs(Vector3.Dot(bounds.extents, new Vector3(Mathf.Abs(forward.x), Mathf.Abs(forward.y), Mathf.Abs(forward.z)))), 0.1f);
            Vector3 centre = bounds.center + forward * (forwardReach * 0.85f) + Vector3.up * (bounds.extents.y * 0.45f);

            var eyes = new GameObject(EyesName);
            eyes.transform.SetParent(parent, false);
            eyes.transform.position = centre;

            Material material = EyeMaterial(color);
            for (int side = -1; side <= 1; side += 2)
            {
                GameObject eye = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                eye.name = EyesName + (side < 0 ? "_L" : "_R");
                Collider collider = eye.GetComponent<Collider>();
                if (collider != null)
                {
                    if (Application.isPlaying)
                    {
                        Destroy(collider);
                    }
                    else
                    {
                        DestroyImmediate(collider);
                    }
                }

                eye.transform.SetParent(eyes.transform, false);
                eye.transform.position = centre + right * (spread * side);
                Vector3 lossy = eyes.transform.lossyScale;
                eye.transform.localScale = new Vector3(
                    size / Mathf.Max(0.0001f, lossy.x), size / Mathf.Max(0.0001f, lossy.y), size / Mathf.Max(0.0001f, lossy.z));

                Renderer renderer = eye.GetComponent<Renderer>();
                if (renderer != null)
                {
                    if (material != null)
                    {
                        renderer.sharedMaterial = material;
                    }

                    renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                    renderer.receiveShadows = false;
                }
            }

            return eyes;
        }

        private static Bounds ComputeBounds(Transform parent, Transform enemyRoot)
        {
            parent.GetComponentsInChildren(true, RendererBuffer);
            bool any = false;
            Bounds bounds = default;
            foreach (Renderer renderer in RendererBuffer)
            {
                if (renderer == null || renderer is ParticleSystemRenderer)
                {
                    continue;
                }

                if (!any)
                {
                    bounds = renderer.bounds;
                    any = true;
                }
                else
                {
                    bounds.Encapsulate(renderer.bounds);
                }
            }

            RendererBuffer.Clear();
            if (!any)
            {
                Vector3 origin = enemyRoot != null ? enemyRoot.position : parent.position;
                bounds = new Bounds(origin + Vector3.up * 0.8f, new Vector3(0.8f, 1.6f, 0.8f));
            }

            return bounds;
        }

        private static Material EyeMaterial(Color color)
        {
            Color32 key = color;
            if (EyeMaterials.TryGetValue(key, out Material material) && material != null)
            {
                return material;
            }

            Shader shader = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Unlit/Color") ?? Shader.Find("Sprites/Default");
            if (shader == null)
            {
                return null;
            }

            material = new Material(shader) { name = "NightEyes_" + ColorUtility.ToHtmlStringRGB(color) };
            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", color);
            }

            material.color = color;
            EyeMaterials[key] = material;
            return material;
        }
    }
}
