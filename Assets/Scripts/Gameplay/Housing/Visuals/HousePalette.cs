using Rapadura.Gameplay.Enemies.Visuals;
using UnityEngine;

namespace Rapadura.Gameplay.Housing.Visuals
{
    /// <summary>Flat colours of the house palette texture (one 4 px cell each, 8 × 6), Housing.md §3.3.</summary>
    public enum HouseSwatch
    {
        WoodRaw, WoodJoint, WoodDark, FloorBoard, Zinc, ZincDark, Straw, Dirt,
        Grass, GrassDark, Leaf, Fruit, Stone, Clay, ClothRed, ClothBlue,
        ClothGreen, ClothYellow, ClothPink, Cream, White, Iron, Gold, LampGlow,
        Flower, Black, GhostOk, GhostBad, Water, Glass, Purple, GridLine,
        // Row 5 (H4 kits N2–N5, §3.3): taipa ocre, reboco, telha escura do casarão, cimento, ladrilho hidráulico,
        // madeira envernizada, cunhal de pedra, tábua verde-água.
        Taipa, Plaster, RoofTileDark, Concrete, Ladrilho, Varnish, Quoin, PaintTeal,
        // Row 6 (Vila da Vo Edna style, world bible 2.1): apricot adobe, ochre, walnut joinery, turquoise tile (detail
        // only), reed mat, beaten copper, grapes, poplar green.
        Adobe, Ochre, Walnut, Turquoise, Reed, Copper, Grape, Poplar,
    }

    /// <summary>
    /// The house's palette (same trick as <see cref="EnemyModelPalette"/>): every hull and furniture mesh samples one
    /// tiny point-filtered texture through its UVs, so the whole house shares one URP Lit material. Built at runtime
    /// (no asset), cached for the session. <see cref="HouseSwatch.LampGlow"/> also lights the emission map.
    /// </summary>
    public static class HousePalette
    {
        public const int Columns = 8;
        public const int Rows = 6;
        private const int CellPixels = 4;

        private static readonly Color32[] Colors =
        {
            Hex(0xB08A5A), Hex(0x6B4E31), Hex(0x5A3E2B), Hex(0xC49A66), Hex(0x9AA3A8), Hex(0x6F787D), Hex(0xC9A55B), Hex(0x8A6A45),
            Hex(0x7DA453), Hex(0x557F38), Hex(0x4E8A3E), Hex(0xE6A23A), Hex(0x9A958C), Hex(0xB5532E), Hex(0xC4473A), Hex(0x2F5D8A),
            Hex(0x3F7D4E), Hex(0xE1B23C), Hex(0xD98C9A), Hex(0xEDE6D6), Hex(0xF4EFE3), Hex(0x4A4F57), Hex(0xE8B84A), Hex(0xFFD58A),
            Hex(0xE07AA0), Hex(0x1E1A17), Hex(0x6FD38A), Hex(0xE05050), Hex(0x5AA0C8), Hex(0xB9D8E0), Hex(0x6B4C8A), Hex(0xF7F1DC),
            Hex(0xC7955A), Hex(0xE2CBA4), Hex(0xA84B2A), Hex(0xBDB6A8), Hex(0x3F7F8F), Hex(0x4A3222), Hex(0x8E8A80), Hex(0x8FC1B5),
            Hex(0xD9A066), Hex(0xB7793F), Hex(0x6B4226), Hex(0x3FA7A3), Hex(0xC8B27A), Hex(0xB06B3A), Hex(0x5B3A6E), Hex(0x7FA650),
        };

        private static Material _material;

        public static Color ColorOf(HouseSwatch swatch) => Colors[(int)swatch];

        public static float GlowOf(HouseSwatch swatch) => swatch == HouseSwatch.LampGlow ? 0.9f : swatch == HouseSwatch.GhostOk || swatch == HouseSwatch.GhostBad ? 0.6f : 0f;

        /// <summary>UV at the centre of <paramref name="swatch"/>'s cell.</summary>
        public static Vector2 UvOf(HouseSwatch swatch)
        {
            int index = (int)swatch;
            return new Vector2((index % Columns + 0.5f) / Columns, (index / Columns + 0.5f) / Rows);
        }

        public static HouseSwatch SwatchAt(Vector2 uv)
        {
            int column = Mathf.Clamp(Mathf.FloorToInt(uv.x * Columns), 0, Columns - 1);
            int row = Mathf.Clamp(Mathf.FloorToInt(uv.y * Rows), 0, Rows - 1);
            return (HouseSwatch)(row * Columns + column);
        }

        /// <summary>The shared palette material (URP Lit through <see cref="EnemyModelPalette.CreateMaterial"/>).</summary>
        public static Material SharedMaterial
        {
            get
            {
                if (_material == null)
                {
                    _material = EnemyModelPalette.CreateMaterial(Bake("HousePalette", false), Bake("HousePaletteEmission", true));
                    _material.name = "HousePalette";
                    _material.enableInstancing = true;
                }

                return _material;
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
                        color *= GlowOf((HouseSwatch)index);
                        color.a = 1f;
                    }

                    pixels[y * width + x] = color;
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply(false, false);
            return texture;
        }

        private static Color32 Hex(int rgb) => new Color32((byte)((rgb >> 16) & 0xFF), (byte)((rgb >> 8) & 0xFF), (byte)(rgb & 0xFF), 255);
    }
}
