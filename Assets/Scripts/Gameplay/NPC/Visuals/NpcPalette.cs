using Rapadura.Gameplay.Enemies.Visuals;
using UnityEngine;

namespace Rapadura.Gameplay.NPC.Visuals
{
    /// <summary>
    /// Flat colours of the NPC palette texture (one cell each, 8 x 8). Append only (a cell index is baked in the mesh UVs).
    /// Region rows follow Docs/WorldBible_Maniqueismo.md §2 (Zarafshan, Qocho, Litema, Lamparinas).
    /// </summary>
    public enum NpcSwatch
    {
        // Skin (6 tones: the Silk Road and the African highlands) + face
        SkinFair, SkinLight, SkinOlive, SkinTan, SkinBrown, SkinDeep, Blush, Mouth,
        // Hair and eyes
        HairBlack, HairDark, HairBrown, HairAuburn, HairGrey, HairWhite, Eye, EyeShine,
        // Zarafshan (Vila da Vó Edna): adobe apricot, ochre, walnut, poplar green, tile turquoise, pomegranate, indigo, suzani cream
        Apricot, Ochre, Walnut, PoplarGreen, Turquoise, Pomegranate, Indigo, Cream,
        // Qocho: red sandstone, rose rammed earth, golden sand, violet shade, vine green, old gold, saffron, desert blue
        Sandstone, Rose, SandGold, Violet, VineGreen, OldGold, Saffron, DesertBlue,
        // Litema: clay red, lime white, granite, straw, blanket blue, blanket yellow, protea pink, aloe green
        Clay, LimeWhite, Granite, Straw, BlanketBlue, BlanketYellow, Protea, Aloe,
        // Lamparinas: tuff rose, basalt, shadow indigo, lamp robe white, copper, moss, lilac, night violet
        Tuff, Basalt, ShadowIndigo, RobeWhite, Copper, Moss, Lilac, NightViolet,
        // Materials
        Leather, LeatherDark, Wood, WoodDark, Iron, Steel, Brass, Wicker,
        // Glow (emissive) + small accents
        LampGlow, CrystalGlow, Ember, SunGlow, HerbGreen, FlowerPink, FlowerYellow, Feather,
    }

    /// <summary>
    /// The NPCs' palette texture (same trick as <see cref="EnemyModelPalette"/> and the town kit): every NPC mesh samples
    /// one tiny point-filtered texture of flat colour cells through its UVs, so all residents of the island share a single
    /// URP Lit material (one batch-friendly material, no per-NPC textures). The glow cells (lamps, crystals, embers, the
    /// sun pendant) light up through the emission map.
    /// </summary>
    public static class NpcPalette
    {
        public const int Columns = 8;
        public const int Rows = 8;
        private const int CellPixels = 4;

        private static readonly Color[] Colors =
        {
            // Skin + face
            new Color(0.96f, 0.82f, 0.72f), new Color(0.9f, 0.72f, 0.58f), new Color(0.8f, 0.6f, 0.44f), new Color(0.7f, 0.5f, 0.34f),
            new Color(0.52f, 0.34f, 0.22f), new Color(0.36f, 0.23f, 0.15f), new Color(0.93f, 0.52f, 0.5f), new Color(0.42f, 0.16f, 0.16f),
            // Hair + eyes
            new Color(0.09f, 0.08f, 0.09f), new Color(0.2f, 0.13f, 0.09f), new Color(0.38f, 0.24f, 0.13f), new Color(0.52f, 0.24f, 0.12f),
            new Color(0.62f, 0.6f, 0.58f), new Color(0.92f, 0.91f, 0.88f), new Color(0.1f, 0.07f, 0.07f), new Color(1f, 1f, 1f),
            // Zarafshan  #D9A066 #B7793F #6B4226 #7FA650 #3FA7A3
            new Color(0.85f, 0.63f, 0.4f), new Color(0.72f, 0.47f, 0.25f), new Color(0.42f, 0.26f, 0.15f), new Color(0.5f, 0.65f, 0.31f),
            new Color(0.25f, 0.65f, 0.64f), new Color(0.72f, 0.16f, 0.2f), new Color(0.2f, 0.22f, 0.45f), new Color(0.95f, 0.9f, 0.78f),
            // Qocho  #B5482F #D7A28A #E7C27D #6D4C7D #8DAA3A #C9A23F
            new Color(0.71f, 0.28f, 0.18f), new Color(0.84f, 0.64f, 0.54f), new Color(0.91f, 0.76f, 0.49f), new Color(0.43f, 0.3f, 0.49f),
            new Color(0.55f, 0.67f, 0.23f), new Color(0.79f, 0.64f, 0.25f), new Color(0.93f, 0.6f, 0.15f), new Color(0.22f, 0.34f, 0.58f),
            // Litema  #A0522D #EFE6D8 #8A8A84 #C8A85A #2F4F8F #E3B23C
            new Color(0.63f, 0.32f, 0.18f), new Color(0.94f, 0.9f, 0.85f), new Color(0.54f, 0.54f, 0.52f), new Color(0.78f, 0.66f, 0.35f),
            new Color(0.18f, 0.31f, 0.56f), new Color(0.89f, 0.7f, 0.24f), new Color(0.88f, 0.45f, 0.55f), new Color(0.42f, 0.6f, 0.42f),
            // Lamparinas  #C98F86 #3B3B44 #232046 #F2EEE6 #B06B3A
            new Color(0.79f, 0.56f, 0.53f), new Color(0.23f, 0.23f, 0.27f), new Color(0.14f, 0.13f, 0.28f), new Color(0.95f, 0.93f, 0.9f),
            new Color(0.69f, 0.42f, 0.23f), new Color(0.48f, 0.7f, 0.42f), new Color(0.7f, 0.6f, 0.86f), new Color(0.32f, 0.2f, 0.45f),
            // Materials
            new Color(0.5f, 0.32f, 0.19f), new Color(0.3f, 0.19f, 0.12f), new Color(0.58f, 0.41f, 0.24f), new Color(0.33f, 0.22f, 0.13f),
            new Color(0.34f, 0.35f, 0.39f), new Color(0.74f, 0.76f, 0.8f), new Color(0.85f, 0.66f, 0.3f), new Color(0.74f, 0.58f, 0.34f),
            // Glow + accents
            new Color(1f, 0.72f, 0.3f), new Color(0.62f, 0.9f, 1f), new Color(1f, 0.5f, 0.18f), new Color(1f, 0.86f, 0.4f),
            new Color(0.32f, 0.6f, 0.26f), new Color(0.95f, 0.55f, 0.7f), new Color(1f, 0.85f, 0.3f), new Color(0.86f, 0.84f, 0.8f),
        };

        public static int Count => Colors.Length;

        /// <summary>Emission strength per swatch (multiplied onto its colour); 0 = not glowing.</summary>
        public static float GlowOf(NpcSwatch swatch)
        {
            switch (swatch)
            {
                case NpcSwatch.LampGlow: return 1f;
                case NpcSwatch.CrystalGlow: return 0.8f;
                case NpcSwatch.Ember: return 0.9f;
                case NpcSwatch.SunGlow: return 0.8f;
                default: return 0f;
            }
        }

        public static Color ColorOf(NpcSwatch swatch) => Colors[(int)swatch];

        /// <summary>UV at the centre of <paramref name="swatch"/>'s cell.</summary>
        public static Vector2 UvOf(NpcSwatch swatch)
        {
            int index = (int)swatch;
            return new Vector2((index % Columns + 0.5f) / Columns, (index / Columns + 0.5f) / Rows);
        }

        /// <summary>Swatch whose cell contains <paramref name="uv"/> (tests, debugging).</summary>
        public static NpcSwatch SwatchAt(Vector2 uv)
        {
            int column = Mathf.Clamp(Mathf.FloorToInt(uv.x * Columns), 0, Columns - 1);
            int row = Mathf.Clamp(Mathf.FloorToInt(uv.y * Rows), 0, Rows - 1);
            return (NpcSwatch)(row * Columns + column);
        }

        private static Material _material;

        /// <summary>The one material every NPC shares (built once, kept for the session; never saved).</summary>
        public static Material SharedMaterial
        {
            get
            {
                if (_material == null)
                {
                    _material = EnemyModelPalette.CreateMaterial(Bake("NpcPalette", false), Bake("NpcPaletteEmission", true));
                    _material.name = "NpcPalette";
                    _material.hideFlags = HideFlags.DontSave;
                }

                return _material;
            }
        }

        /// <summary>Destroys the shared material and its textures (editor cleanup before Play; rebuilt on next use).</summary>
        public static void DestroyMaterial()
        {
            if (_material == null)
            {
                _material = null;
                return;
            }

            DestroyTexture(_material, "_BaseMap");
            DestroyTexture(_material, "_EmissionMap");
            Object.DestroyImmediate(_material);
            _material = null;
        }

        private static void DestroyTexture(Material material, string property)
        {
            if (material.HasProperty(property))
            {
                Texture texture = material.GetTexture(property);
                if (texture != null)
                {
                    Object.DestroyImmediate(texture);
                }
            }
        }

        private static Texture2D Bake(string textureName, bool emission)
        {
            int width = Columns * CellPixels;
            int height = Rows * CellPixels;
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false)
            {
                name = textureName,
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.DontSave,
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
                        color *= GlowOf((NpcSwatch)index);
                        color.a = 1f;
                    }

                    pixels[y * width + x] = color;
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply(false, false);
            return texture;
        }
    }
}
