using UnityEngine;

namespace Rapadura.Gameplay.Enemies.Visuals
{
    /// <summary>Flat colours of the procedural enemy palette texture (one cell each, 8 x 6). Append only.</summary>
    public enum EnemySwatch
    {
        // Cavernas: morcegos
        BatFur, BatFurDark, BatWing, BatEye,
        // Montanha: golem
        GolemStone, GolemStoneDark, GolemSnow, GolemCrack,
        // Montanha: senhor da guerra
        Iron, IronDark, Steel, WarCape, Horn, Leather, Gold, WarEye,
        // Deserto: guardiao da ruina
        Sandstone, SandstoneDark, SandstoneLight, Rune,
        // Cavernas: habitante ancestral
        CaveSkin, CaveSkinDark, CaveBelly, SoftEye, Crystal, StolenLight, CaveMoss,
        // Neutros
        Black, Bone, Shadow, Fang, Spare,
        // Noite: lobisomem, morcego-vampiro, coruja, vaga-lumes, queixadas, mae-da-lua (Docs/DayNight.md)
        WolfFur, WolfFurDark, WolfBelly, Pink, VampFur, VampWing, OwlBuff, OwlFace,
        MoonEye, FireflyGlow, Ember, PeccaryFur, Bark, BarkDark, Lichen, Spare2,
    }

    /// <summary>
    /// The procedural enemies' palette texture (same trick as <c>WorldMapPalette</c>): every enemy
    /// mesh samples one tiny point-filtered texture of flat colour cells through its UVs, so all five
    /// models share a single URP Lit material. A second texture of the same layout is the emission map:
    /// black except the "glow" cells (eyes, cracks, runes, crystals), which light up when the
    /// material's <c>_EMISSION</c> variant is available (the baked material asset keeps it in builds).
    /// Region colours follow the biomes: cave purples/blues, mountain greys + snow, desert sandstone.
    /// </summary>
    public static class EnemyModelPalette
    {
        public const int Columns = 8;
        public const int Rows = 6;
        private const int CellPixels = 4;

        private static readonly Color[] Colors =
        {
            new Color(0.3f, 0.21f, 0.34f),   // BatFur
            new Color(0.17f, 0.12f, 0.21f),  // BatFurDark
            new Color(0.46f, 0.28f, 0.44f),  // BatWing
            new Color(1f, 0.62f, 0.3f),      // BatEye (glow)
            new Color(0.54f, 0.52f, 0.5f),   // GolemStone
            new Color(0.36f, 0.35f, 0.37f),  // GolemStoneDark
            new Color(0.93f, 0.95f, 1f),     // GolemSnow
            new Color(1f, 0.58f, 0.2f),      // GolemCrack (glow)
            new Color(0.36f, 0.38f, 0.44f),  // Iron
            new Color(0.19f, 0.2f, 0.25f),   // IronDark
            new Color(0.78f, 0.8f, 0.84f),   // Steel
            new Color(0.66f, 0.12f, 0.12f),  // WarCape
            new Color(0.9f, 0.84f, 0.7f),    // Horn
            new Color(0.4f, 0.26f, 0.17f),   // Leather
            new Color(0.97f, 0.74f, 0.24f),  // Gold
            new Color(1f, 0.32f, 0.2f),      // WarEye (glow)
            new Color(0.88f, 0.71f, 0.48f),  // Sandstone
            new Color(0.67f, 0.5f, 0.34f),   // SandstoneDark
            new Color(0.97f, 0.88f, 0.68f),  // SandstoneLight
            new Color(0.3f, 0.96f, 0.86f),   // Rune (glow)
            new Color(0.38f, 0.4f, 0.56f),   // CaveSkin
            new Color(0.24f, 0.25f, 0.38f),  // CaveSkinDark
            new Color(0.6f, 0.6f, 0.72f),    // CaveBelly
            new Color(0.78f, 0.96f, 1f),     // SoftEye (glow)
            new Color(0.48f, 0.86f, 1f),     // Crystal (glow)
            new Color(1f, 0.86f, 0.52f),     // StolenLight (glow)
            new Color(0.4f, 0.56f, 0.5f),    // CaveMoss
            new Color(0.05f, 0.04f, 0.06f),  // Black
            new Color(0.92f, 0.9f, 0.84f),   // Bone
            new Color(0.12f, 0.1f, 0.14f),   // Shadow
            new Color(0.98f, 0.97f, 0.92f),  // Fang
            new Color(1f, 0f, 1f),           // Spare
            new Color(0.47f, 0.39f, 0.33f),  // WolfFur (scruffy brown-grey)
            new Color(0.27f, 0.22f, 0.21f),  // WolfFurDark
            new Color(0.76f, 0.66f, 0.52f),  // WolfBelly
            new Color(0.9f, 0.5f, 0.54f),    // Pink (tongue, mouth, snout)
            new Color(0.36f, 0.18f, 0.24f),  // VampFur
            new Color(0.56f, 0.27f, 0.35f),  // VampWing
            new Color(0.84f, 0.66f, 0.4f),   // OwlBuff
            new Color(0.97f, 0.94f, 0.87f),  // OwlFace (pale: reads in the dark)
            new Color(1f, 0.88f, 0.3f),      // MoonEye (glow)
            new Color(0.8f, 1f, 0.32f),      // FireflyGlow (glow)
            new Color(1f, 0.55f, 0.16f),     // Ember (glow)
            new Color(0.31f, 0.28f, 0.28f),  // PeccaryFur
            new Color(0.52f, 0.44f, 0.36f),  // Bark
            new Color(0.31f, 0.26f, 0.21f),  // BarkDark
            new Color(0.64f, 0.68f, 0.56f),  // Lichen
            new Color(1f, 0f, 1f),           // Spare2
        };

        /// <summary>Emission strength per glow swatch (multiplied onto its colour); 0 = not glowing.</summary>
        public static float GlowOf(EnemySwatch swatch)
        {
            switch (swatch)
            {
                case EnemySwatch.BatEye: return 0.9f;
                case EnemySwatch.GolemCrack: return 1f;
                case EnemySwatch.WarEye: return 1f;
                case EnemySwatch.Rune: return 0.9f;
                case EnemySwatch.SoftEye: return 0.8f;
                case EnemySwatch.Crystal: return 0.7f;
                case EnemySwatch.StolenLight: return 1f;
                case EnemySwatch.MoonEye: return 1f;
                case EnemySwatch.FireflyGlow: return 1f;
                case EnemySwatch.Ember: return 0.9f;
                default: return 0f;
            }
        }

        public static bool IsGlow(EnemySwatch swatch) => GlowOf(swatch) > 0f;

        public static Color ColorOf(EnemySwatch swatch) => Colors[(int)swatch];

        /// <summary>UV at the centre of <paramref name="swatch"/>'s cell.</summary>
        public static Vector2 UvOf(EnemySwatch swatch)
        {
            int index = (int)swatch;
            return new Vector2((index % Columns + 0.5f) / Columns, (index / Columns + 0.5f) / Rows);
        }

        /// <summary>Swatch whose cell contains <paramref name="uv"/> (tests, debugging).</summary>
        public static EnemySwatch SwatchAt(Vector2 uv)
        {
            int column = Mathf.Clamp(Mathf.FloorToInt(uv.x * Columns), 0, Columns - 1);
            int row = Mathf.Clamp(Mathf.FloorToInt(uv.y * Rows), 0, Rows - 1);
            return (EnemySwatch)(row * Columns + column);
        }

        /// <summary>The albedo palette (point filtered, clamped, no mips so cells never bleed).</summary>
        public static Texture2D CreateTexture() => Bake("EnemyPalette", false);

        /// <summary>The emission palette: black everywhere except the glow cells.</summary>
        public static Texture2D CreateEmissionTexture() => Bake("EnemyPaletteEmission", true);

        private static Texture2D Bake(string textureName, bool emission)
        {
            int width = Columns * CellPixels;
            int height = Rows * CellPixels;
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false)
            {
                name = textureName,
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
            };

            var pixels = new Color32[width * height];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int index = (y / CellPixels) * Columns + x / CellPixels;
                    Color color = Colors[index];
                    if (emission)
                    {
                        color *= GlowOf((EnemySwatch)index);
                        color.a = 1f;
                    }

                    pixels[y * width + x] = color;
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply(false, false);
            return texture;
        }

        private static readonly int BaseMapId = Shader.PropertyToID("_BaseMap");
        private static readonly int MainTexId = Shader.PropertyToID("_MainTex");
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int EmissionMapId = Shader.PropertyToID("_EmissionMap");
        private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");
        private static readonly int SmoothnessId = Shader.PropertyToID("_Smoothness");
        private static readonly int MetallicId = Shader.PropertyToID("_Metallic");

        /// <summary>
        /// Builds the shared palette material on URP Lit (falls back to the default primitive
        /// material's shader where URP is missing). Flat, matte, emissive glow cells.
        /// </summary>
        public static Material CreateMaterial(Texture2D albedo, Texture2D emission)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            Material material;
            if (shader != null)
            {
                material = new Material(shader);
            }
            else
            {
                GameObject probe = GameObject.CreatePrimitive(PrimitiveType.Cube);
                material = new Material(probe.GetComponent<Renderer>().sharedMaterial);
                Object.DestroyImmediate(probe);
            }

            material.name = "EnemyPalette";
            if (material.HasProperty(BaseMapId)) material.SetTexture(BaseMapId, albedo);
            if (material.HasProperty(MainTexId)) material.SetTexture(MainTexId, albedo);
            if (material.HasProperty(BaseColorId)) material.SetColor(BaseColorId, Color.white);
            if (material.HasProperty(SmoothnessId)) material.SetFloat(SmoothnessId, 0.12f);
            if (material.HasProperty(MetallicId)) material.SetFloat(MetallicId, 0f);
            if (emission != null && material.HasProperty(EmissionMapId))
            {
                material.SetTexture(EmissionMapId, emission);
                material.SetColor(EmissionColorId, Color.white);
                material.EnableKeyword("_EMISSION");
                material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            }

            return material;
        }
    }
}
