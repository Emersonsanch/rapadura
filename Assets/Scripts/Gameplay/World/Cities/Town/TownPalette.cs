using Rapadura.Gameplay.Enemies.Visuals;
using UnityEngine;

namespace Rapadura.Gameplay.World.Cities.Town
{
    /// <summary>Flat colours of the walkable-town palette texture (one 4 px cell each, 8 × 8). Docs/Cities.md §5.</summary>
    public enum TownSwatch
    {
        // Row 0: wood and earth (walnut joinery, carved columns).
        WoodLight, Walnut, WoodDark, WoodWeathered, DoorPlank, Beam, Dirt, DirtDark,
        // Row 1: wall finishes: Sogdian adobe, ochre, plaster, lime; Qocho rose taipa; Litema red clay; Lamparinas pink tuff; red sandstone.
        Adobe, AdobeOchre, Plaster, LimeWhite, TaipaRose, RedClay, PinkTuff, Sandstone,
        // Row 2: roofs and stone (thatch, clay tile, slate, granite, basalt, packed-earth flat roof).
        Thatch, ClayTile, Slate, Stone, StoneDark, Basalt, Cobble, Mud,
        // Row 3: Zarafshan tugai vegetation (poplar, gold poplar, tamarisk, apricot).
        Grass, GrassDark, Poplar, LeafDark, LeafGold, Trunk, Tamarisk, Fruit,
        // Row 4: other regions' ground and plants (sand, vine, snow, pine, aloe, glowing moss and mushrooms).
        Sand, SandDark, Vine, Snow, Pine, Aloe, Moss, MushroomBlue,
        // Row 5: cloth (suzani, Basotho blankets) and metal.
        ClothRed, ClothIndigo, ClothYellow, ClothGreen, ClothWhite, Iron, Gold, Black,
        // Row 6: lights and people (LampGlow, WindowLit, Ember and Water light the emission map at night).
        LampGlow, WindowLit, Glass, Water, Ember, Skin, SkinDark, Hair,
        // Row 7: masonry, paving and accents (tile turquoise, copper, violet shadow, indigo).
        Brick, Turquoise, Paving, PavingDark, Curb, Copper, Violet, Indigo,
    }

    /// <summary>
    /// The walkable towns' palette (the same trick as <c>HousePalette</c>/<c>WorldMapPalette</c>): every kit mesh samples
    /// one tiny point-filtered texture through its UVs, so a whole town (every region) shares ONE URP Lit material and
    /// merges into a handful of meshes. Built at runtime (no asset, nothing added to the build), cached for the session.
    /// Windows, lanterns and embers have an emission cell; <see cref="SetNightGlow"/> fades it in at dusk.
    /// </summary>
    public static class TownPalette
    {
        public const int Columns = 8;
        public const int Rows = 8;
        private const int CellPixels = 4;

        private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

        private static readonly Color32[] Colors =
        {
            Hex(0xB08A5E), Hex(0x6B4226), Hex(0x4A2E1C), Hex(0x8F806E), Hex(0x7A4E2E), Hex(0x4C3424), Hex(0xB08A60), Hex(0x83643F),
            Hex(0xD9A066), Hex(0xB7793F), Hex(0xE6CFA6), Hex(0xEFE6D8), Hex(0xD7A28A), Hex(0xA0522D), Hex(0xC98F86), Hex(0xB5482F),
            Hex(0xC8A85A), Hex(0xB4522F), Hex(0x5C636B), Hex(0x8A8A84), Hex(0x666560), Hex(0x3B3B44), Hex(0x9C8F7C), Hex(0xA27B55),
            Hex(0x86A957), Hex(0x5E8A3E), Hex(0x7FA650), Hex(0x3F6B34), Hex(0xE3B53C), Hex(0x6E5038), Hex(0xD98CA6), Hex(0xE89A3C),
            Hex(0xE7C27D), Hex(0xC9A060), Hex(0x8DAA3A), Hex(0xDDE8F0), Hex(0x2F5C40), Hex(0xC8452E), Hex(0x9BE07A), Hex(0x5FA8E0),
            Hex(0xB8352E), Hex(0x2F4F8F), Hex(0xE3B23C), Hex(0x3F7D4E), Hex(0xF2EEE6), Hex(0x4A4F57), Hex(0xC9A23F), Hex(0x1E1A17),
            Hex(0xFFB347), Hex(0x3B4652), Hex(0xA9CFDB), Hex(0x3F8FA8), Hex(0xFF8A3A), Hex(0xD9A47E), Hex(0x8E5E3E), Hex(0x2E2018),
            Hex(0xA8553A), Hex(0x3FA7A3), Hex(0xD1BE9C), Hex(0xA8957A), Hex(0xC9BBA0), Hex(0xB06B3A), Hex(0x6D4C7D), Hex(0x232046),
        };

        private static Material _material;
        private static float _nightGlow = -1f;

        public static int Count => Colors.Length;

        public static Color ColorOf(TownSwatch swatch) => Colors[(int)swatch];

        /// <summary>Emission strength of <paramref name="swatch"/> at full night (0 = never glows).</summary>
        public static float GlowOf(TownSwatch swatch)
        {
            switch (swatch)
            {
                case TownSwatch.LampGlow: return 1f;
                case TownSwatch.Ember: return 1f;
                case TownSwatch.WindowLit: return 0.85f;
                case TownSwatch.MushroomBlue: return 0.5f;
                case TownSwatch.Moss: return 0.25f;
                default: return 0f;
            }
        }

        /// <summary>Colour the cell emits (the lit window is a warm candle light over a dark daytime pane).</summary>
        private static Color EmissionOf(TownSwatch swatch)
        {
            switch (swatch)
            {
                case TownSwatch.WindowLit: return new Color(1f, 0.74f, 0.38f);
                case TownSwatch.Ember: return new Color(1f, 0.45f, 0.15f);
                default: return ColorOf(swatch);
            }
        }

        /// <summary>UV at the centre of <paramref name="swatch"/>'s cell.</summary>
        public static Vector2 UvOf(TownSwatch swatch)
        {
            int index = (int)swatch;
            return new Vector2((index % Columns + 0.5f) / Columns, (index / Columns + 0.5f) / Rows);
        }

        public static TownSwatch SwatchAt(Vector2 uv)
        {
            int column = Mathf.Clamp(Mathf.FloorToInt(uv.x * Columns), 0, Columns - 1);
            int row = Mathf.Clamp(Mathf.FloorToInt(uv.y * Rows), 0, Rows - 1);
            return (TownSwatch)(row * Columns + column);
        }

        /// <summary>The one material of every town (URP Lit, instancing on, SRP-batcher friendly).</summary>
        public static Material SharedMaterial
        {
            get
            {
                if (_material == null)
                {
                    _material = EnemyModelPalette.CreateMaterial(Bake("TownPalette", false), Bake("TownPaletteEmission", true));
                    _material.name = "TownPalette";
                    _material.enableInstancing = true;
                    _nightGlow = -1f;
                    SetNightGlow(0f);
                }

                return _material;
            }
        }

        /// <summary>Lights windows/lanterns: 0 by day, 1 at night (driven by the town's day/night component).</summary>
        public static void SetNightGlow(float amount)
        {
            amount = Mathf.Clamp01(amount);
            if (_material == null || Mathf.Abs(amount - _nightGlow) < 0.01f)
            {
                return;
            }

            _nightGlow = amount;
            if (_material.HasProperty(EmissionColorId))
            {
                // A faint glow even by day, so the lanterns never read as grey boxes.
                float strength = Mathf.Lerp(0.15f, 1.6f, amount);
                _material.SetColor(EmissionColorId, new Color(strength, strength, strength, 1f));
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
                        color = EmissionOf((TownSwatch)index) * GlowOf((TownSwatch)index);
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
