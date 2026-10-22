using System;
using System.Collections.Generic;
using UnityEngine;

namespace Rapadura.Gameplay.World.Biomes
{
    /// <summary>What a <see cref="BiomeArenaPropRule"/> does to a matching scene object.</summary>
    public enum BiomeArenaPropAction
    {
        /// <summary>Deactivate the object (and therefore its whole subtree) for the encounter.</summary>
        Hide = 0,

        /// <summary>Recolour every renderer under the object through a MaterialPropertyBlock.</summary>
        Tint = 1,

        /// <summary>Explicitly leave the object as authored (stops later, broader rules from matching it).</summary>
        Keep = 2,
    }

    /// <summary>Cheap primitive-built props <see cref="BiomeArenaDressing"/> can scatter around the arena.</summary>
    public enum BiomeArenaPropKind
    {
        Cactus = 0,
        Stalagmite = 1,
        SnowyRock = 2,
        Boulder = 3,
    }

    /// <summary>
    /// "Objects whose name starts with <see cref="NamePrefix"/> (and, if set, carry <see cref="Tag"/>)
    /// get <see cref="Action"/>". Rules are evaluated in list order and the first match wins (see
    /// <see cref="BiomeArenaRules.ResolveAction"/>); a matched object's children are not visited.
    /// </summary>
    [Serializable]
    public class BiomeArenaPropRule
    {
        [Tooltip("Case-sensitive GameObject name prefix, e.g. \"Tree_\" or \"Rock\". Empty = rule ignored.")]
        [SerializeField] private string _namePrefix = string.Empty;
        [Tooltip("Optional: the object must also carry this tag. Empty = any tag.")]
        [SerializeField] private string _tag = string.Empty;
        [SerializeField] private BiomeArenaPropAction _action = BiomeArenaPropAction.Hide;
        [Tooltip("Tint target colour (Tint only). Alpha is the blend strength: 0 = unchanged, 1 = fully this colour.")]
        [SerializeField] private Color _tint = Color.white;

        public string NamePrefix => _namePrefix;
        public string Tag => _tag;
        public BiomeArenaPropAction Action => _action;
        public Color Tint => _tint;

        public BiomeArenaPropRule()
        {
        }

        /// <summary>Plain-C# construction path for code defaults and EditMode tests.</summary>
        public BiomeArenaPropRule(string namePrefix, BiomeArenaPropAction action, Color tint = default, string tag = null)
        {
            _namePrefix = namePrefix ?? string.Empty;
            _action = action;
            _tint = action == BiomeArenaPropAction.Tint ? tint : Color.white;
            _tag = tag ?? string.Empty;
        }

        public static BiomeArenaPropRule Hide(string namePrefix) => new BiomeArenaPropRule(namePrefix, BiomeArenaPropAction.Hide);

        public static BiomeArenaPropRule Keep(string namePrefix) => new BiomeArenaPropRule(namePrefix, BiomeArenaPropAction.Keep);

        public static BiomeArenaPropRule Recolor(string namePrefix, Color tint) => new BiomeArenaPropRule(namePrefix, BiomeArenaPropAction.Tint, tint);
    }

    /// <summary>"Scatter <see cref="Count"/> props of <see cref="Kind"/> around the arena".</summary>
    [Serializable]
    public class BiomeArenaPropSpawn
    {
        [SerializeField] private BiomeArenaPropKind _kind = BiomeArenaPropKind.Boulder;
        [SerializeField] [Min(0)] private int _count = 4;
        [Tooltip("Main body colour.")]
        [SerializeField] private Color _primaryColor = Color.gray;
        [Tooltip("Accent colour (cactus flowers, snow caps, stalagmite tips).")]
        [SerializeField] private Color _secondaryColor = Color.white;
        [SerializeField] [Min(0.05f)] private float _minScale = 0.8f;
        [SerializeField] [Min(0.05f)] private float _maxScale = 1.4f;

        public BiomeArenaPropKind Kind => _kind;
        public int Count => _count;
        public Color PrimaryColor => _primaryColor;
        public Color SecondaryColor => _secondaryColor;
        public float MinScale => _minScale;
        public float MaxScale => Mathf.Max(_minScale, _maxScale);

        public BiomeArenaPropSpawn()
        {
        }

        /// <summary>Plain-C# construction path for code defaults and EditMode tests.</summary>
        public BiomeArenaPropSpawn(BiomeArenaPropKind kind, int count, Color primary, Color secondary, float minScale, float maxScale)
        {
            _kind = kind;
            _count = Mathf.Max(0, count);
            _primaryColor = primary;
            _secondaryColor = secondary;
            _minScale = Mathf.Max(0.05f, minScale);
            _maxScale = Mathf.Max(_minScale, maxScale);
        }
    }

    /// <summary>
    /// How the shared combat arena (Main.unity, authored as a forest greybox) is re-dressed for an
    /// encounter rolled in <see cref="Biome"/>: ground recolour, which authored props to hide or
    /// recolour (by name prefix/tag), a fog/ambient/sun/sky baseline and a handful of cheap
    /// primitive props. Applied at runtime by <see cref="BiomeArenaDressing"/> only while an
    /// encounter is active. Loaded from Resources/Biomes/Arena/&lt;Biome&gt;.asset by
    /// <see cref="BiomeArenaProfiles"/>; when an asset is missing, <see cref="CreateBuiltInDefault"/>
    /// provides the same values from code (the shipped assets mirror these defaults).
    /// </summary>
    [CreateAssetMenu(menuName = "Rapadura/World/Biome Arena Profile", fileName = "NewBiomeArenaProfile")]
    public class BiomeArenaProfile : ScriptableObject
    {
        [SerializeField] private BiomeType _biome = BiomeType.Forest;

        [Header("Ground")]
        [SerializeField] private bool _tintGround;
        [Tooltip("Ground colour. Alpha is the blend strength over the authored colour.")]
        [SerializeField] private Color _groundTint = Color.white;
        [Tooltip("Drop the authored ground texture (grass) for a flat colour, or use Ground Texture when assigned.")]
        [SerializeField] private bool _replaceGroundTexture;
        [Tooltip("Optional replacement ground texture (only with Replace Ground Texture). Empty = flat colour.")]
        [SerializeField] private Texture2D _groundTexture;

        [Header("Authored props (first matching rule wins)")]
        [SerializeField] private List<BiomeArenaPropRule> _propRules = new List<BiomeArenaPropRule>();

        [Header("Atmosphere")]
        [Tooltip("Replace the scene's clear-weather fog with this baseline (weather fog still blends on top).")]
        [SerializeField] private bool _overrideFog;
        [SerializeField] private Color _fogColor = Color.gray;
        [SerializeField] [Min(0f)] private float _fogDensity = 0.015f;
        [Tooltip("Multiplier on the ambient light (white = unchanged).")]
        [SerializeField] private Color _ambientTint = Color.white;
        [Tooltip("Multiplier on the sun intensity (1 = unchanged; caves are dim).")]
        [SerializeField] [Min(0f)] private float _sunIntensityMultiplier = 1f;

        [Header("Sky (procedural skybox; applied to a runtime copy)")]
        [SerializeField] private bool _tintSkybox;
        [SerializeField] private Color _skyTint = new Color(0.5f, 0.5f, 0.5f, 1f);
        [SerializeField] private Color _skyGroundColor = new Color(0.37f, 0.35f, 0.34f, 1f);
        [SerializeField] [Min(0f)] private float _skyExposure = 1.3f;

        [Header("Procedural props")]
        [SerializeField] private List<BiomeArenaPropSpawn> _props = new List<BiomeArenaPropSpawn>();
        [Tooltip("Props are scattered on a ring around the arena centre, clear of the fight in the middle. Never below BiomeArenaRules.MinPropClearRadius (enemies spawn 6-9 m out; a 9 m inner edge let a rock land on a spawn point).")]
        [SerializeField] [Min(0f)] private float _scatterInnerRadius = BiomeArenaRules.MinPropClearRadius;
        [SerializeField] [Min(0f)] private float _scatterOuterRadius = 26f;

        public BiomeType Biome => _biome;

        public bool TintGround => _tintGround;
        public Color GroundTint => _groundTint;
        public bool ReplaceGroundTexture => _replaceGroundTexture;
        public Texture2D GroundTexture => _groundTexture;

        public IReadOnlyList<BiomeArenaPropRule> PropRules => _propRules;

        public bool OverrideFog => _overrideFog;
        public Color FogColor => _fogColor;
        public float FogDensity => _fogDensity;
        public Color AmbientTint => _ambientTint;
        public float SunIntensityMultiplier => _sunIntensityMultiplier;

        public bool TintSkybox => _tintSkybox;
        public Color SkyTint => _skyTint;
        public Color SkyGroundColor => _skyGroundColor;
        public float SkyExposure => _skyExposure;

        public IReadOnlyList<BiomeArenaPropSpawn> Props => _props;
        public float ScatterInnerRadius => Mathf.Min(_scatterInnerRadius, _scatterOuterRadius);
        public float ScatterOuterRadius => Mathf.Max(_scatterInnerRadius, _scatterOuterRadius);

        /// <summary>True when applying this profile changes nothing visual (e.g. Forest, the arena's authored look).</summary>
        public bool IsNeutral =>
            !_tintGround && !_overrideFog && !_tintSkybox
            && (_propRules == null || _propRules.Count == 0)
            && (_props == null || _props.Count == 0)
            && _ambientTint == Color.white
            && Mathf.Approximately(_sunIntensityMultiplier, 1f);

        /// <summary>Plain-C# setup path for code defaults and EditMode tests (no AssetDatabase).</summary>
        public void SetDataForTests(BiomeType biome, List<BiomeArenaPropRule> propRules, List<BiomeArenaPropSpawn> props = null)
        {
            _biome = biome;
            _propRules = propRules ?? new List<BiomeArenaPropRule>();
            _props = props ?? new List<BiomeArenaPropSpawn>();
        }

        /// <summary>Ground setup used by <see cref="CreateBuiltInDefault"/> (and tests).</summary>
        public void SetGroundForTests(bool tint, Color groundTint, bool replaceTexture)
        {
            _tintGround = tint;
            _groundTint = groundTint;
            _replaceGroundTexture = replaceTexture;
        }

        /// <summary>Atmosphere setup used by <see cref="CreateBuiltInDefault"/> (and tests).</summary>
        public void SetAtmosphereForTests(bool overrideFog, Color fogColor, float fogDensity, Color ambientTint, float sunMultiplier)
        {
            _overrideFog = overrideFog;
            _fogColor = fogColor;
            _fogDensity = Mathf.Max(0f, fogDensity);
            _ambientTint = ambientTint;
            _sunIntensityMultiplier = Mathf.Max(0f, sunMultiplier);
        }

        /// <summary>Sky setup used by <see cref="CreateBuiltInDefault"/> (and tests).</summary>
        public void SetSkyForTests(bool tint, Color skyTint, Color groundColor, float exposure)
        {
            _tintSkybox = tint;
            _skyTint = skyTint;
            _skyGroundColor = groundColor;
            _skyExposure = Mathf.Max(0f, exposure);
        }

        public void SetScatterForTests(float innerRadius, float outerRadius)
        {
            _scatterInnerRadius = Mathf.Max(0f, innerRadius);
            _scatterOuterRadius = Mathf.Max(0f, outerRadius);
        }

        /// <summary>
        /// Code fallback for a biome whose Resources/Biomes/Arena asset is missing. Mirrors the shipped
        /// assets. Name prefixes match Main.unity's greybox: NaturalTrees/Tree_* (trees), GroundDetail's
        /// Tuft/Twig/Pebble, Rocks/Rock_*/ProceduralRock (rocks), Ground (the textured plane).
        /// </summary>
        public static BiomeArenaProfile CreateBuiltInDefault(BiomeType biome)
        {
            var profile = CreateInstance<BiomeArenaProfile>();
            profile.name = $"{biome}_BuiltInArena";
            profile.hideFlags = HideFlags.DontSave;

            switch (biome)
            {
                case BiomeType.Desert:
                {
                    var rock = new Color(0.86f, 0.70f, 0.48f, 0.75f);
                    profile.SetDataForTests(
                        biome,
                        new List<BiomeArenaPropRule>
                        {
                            BiomeArenaPropRule.Hide("NaturalTrees"),
                            BiomeArenaPropRule.Hide("Tree_"),
                            BiomeArenaPropRule.Hide("Tuft"),
                            BiomeArenaPropRule.Hide("GrassTuft"),
                            BiomeArenaPropRule.Hide("Twig"),
                            BiomeArenaPropRule.Recolor("Rock", rock),
                            BiomeArenaPropRule.Recolor("ProceduralRock", rock),
                            BiomeArenaPropRule.Recolor("Pebble", new Color(0.90f, 0.76f, 0.55f, 0.8f)),
                        },
                        new List<BiomeArenaPropSpawn>
                        {
                            new BiomeArenaPropSpawn(BiomeArenaPropKind.Cactus, 7, new Color(0.35f, 0.55f, 0.30f, 1f), new Color(0.95f, 0.75f, 0.40f, 1f), 0.8f, 1.4f),
                            new BiomeArenaPropSpawn(BiomeArenaPropKind.Boulder, 5, new Color(0.80f, 0.62f, 0.42f, 1f), new Color(0.88f, 0.72f, 0.50f, 1f), 0.7f, 1.6f),
                        });
                    profile.SetGroundForTests(true, new Color(0.93f, 0.80f, 0.58f, 1f), true);
                    profile.SetAtmosphereForTests(true, new Color(0.92f, 0.80f, 0.62f, 1f), 0.012f, new Color(1.08f, 1.0f, 0.86f, 1f), 1.1f);
                    profile.SetSkyForTests(true, new Color(0.75f, 0.60f, 0.45f, 1f), new Color(0.80f, 0.65f, 0.45f, 1f), 1.4f);
                    break;
                }

                case BiomeType.Mountain:
                {
                    // Snow-bound: flat blue-white ground (the grass texture read as green), snow-dusted
                    // rocks and pines, no grass tufts, plus snow-capped rocks and snow mounds.
                    var stone = new Color(0.90f, 0.93f, 0.98f, 0.8f);
                    profile.SetDataForTests(
                        biome,
                        new List<BiomeArenaPropRule>
                        {
                            BiomeArenaPropRule.Hide("Tree_Broadleaf"),
                            BiomeArenaPropRule.Hide("Twig"),
                            BiomeArenaPropRule.Recolor("Tree_Pine", new Color(0.80f, 0.88f, 0.92f, 0.5f)),
                            BiomeArenaPropRule.Recolor("Tree_Gnarled", new Color(0.85f, 0.88f, 0.93f, 0.6f)),
                            BiomeArenaPropRule.Hide("Tuft"),
                            BiomeArenaPropRule.Hide("GrassTuft"),
                            BiomeArenaPropRule.Recolor("Rock", stone),
                            BiomeArenaPropRule.Recolor("ProceduralRock", stone),
                            BiomeArenaPropRule.Recolor("Pebble", stone),
                        },
                        new List<BiomeArenaPropSpawn>
                        {
                            new BiomeArenaPropSpawn(BiomeArenaPropKind.SnowyRock, 16, new Color(0.52f, 0.55f, 0.60f, 1f), new Color(0.95f, 0.97f, 1f, 1f), 0.8f, 1.8f),
                            new BiomeArenaPropSpawn(BiomeArenaPropKind.Boulder, 6, new Color(0.92f, 0.95f, 1f, 1f), new Color(0.85f, 0.89f, 0.95f, 1f), 0.6f, 1.4f),
                        });
                    profile.SetGroundForTests(true, new Color(0.74f, 0.78f, 0.85f, 1f), true);
                    profile.SetAtmosphereForTests(true, new Color(0.84f, 0.88f, 0.94f, 1f), 0.02f, new Color(0.92f, 0.97f, 1.08f, 1f), 0.95f);
                    profile.SetSkyForTests(true, new Color(0.60f, 0.70f, 0.85f, 1f), new Color(0.75f, 0.78f, 0.82f, 1f), 1.2f);
                    break;
                }

                case BiomeType.Caves:
                {
                    var rock = new Color(0.32f, 0.30f, 0.34f, 0.8f);
                    profile.SetDataForTests(
                        biome,
                        new List<BiomeArenaPropRule>
                        {
                            BiomeArenaPropRule.Hide("NaturalTrees"),
                            BiomeArenaPropRule.Hide("Tree_"),
                            BiomeArenaPropRule.Hide("Tuft"),
                            BiomeArenaPropRule.Hide("GrassTuft"),
                            BiomeArenaPropRule.Hide("Twig"),
                            BiomeArenaPropRule.Recolor("Rock", rock),
                            BiomeArenaPropRule.Recolor("ProceduralRock", rock),
                            BiomeArenaPropRule.Recolor("Pebble", rock),
                        },
                        new List<BiomeArenaPropSpawn>
                        {
                            new BiomeArenaPropSpawn(BiomeArenaPropKind.Stalagmite, 10, new Color(0.36f, 0.33f, 0.36f, 1f), new Color(0.50f, 0.47f, 0.50f, 1f), 0.8f, 2.0f),
                            new BiomeArenaPropSpawn(BiomeArenaPropKind.Boulder, 4, new Color(0.25f, 0.24f, 0.27f, 1f), new Color(0.30f, 0.29f, 0.32f, 1f), 0.8f, 1.8f),
                        });
                    profile.SetGroundForTests(true, new Color(0.28f, 0.25f, 0.24f, 1f), true);
                    profile.SetAtmosphereForTests(true, new Color(0.035f, 0.035f, 0.05f, 1f), 0.05f, new Color(0.40f, 0.40f, 0.54f, 1f), 0.25f);
                    profile.SetSkyForTests(true, new Color(0.05f, 0.05f, 0.07f, 1f), new Color(0.05f, 0.05f, 0.06f, 1f), 0.25f);
                    break;
                }

                default:
                    // Forest: the arena is authored as a forest, so its profile changes nothing.
                    profile.SetDataForTests(biome, new List<BiomeArenaPropRule>());
                    break;
            }

            return profile;
        }
    }
}
