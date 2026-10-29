using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Dressing
{
    /// <summary>Flat colours of the world-map palette texture (one cell each).</summary>
    public enum WorldMapSwatch
    {
        Trunk, Leaf, LeafDark, Grass, Bush, Flower, Cactus, CactusDark,
        Sand, SandRock, Rock, RockDark, Snow, CaveStone, CaveDark, Crystal,
        Roof, Wall, Wood, WoodDark, Ruin, RuinDark, Gold, Banner,
        Cloud, CloudShade, Fog, DeepWater, Black, Lantern, PineLeaf, Pebble,
    }

    /// <summary>
    /// The classic low-poly "palette texture" trick: every procedural world-map mesh samples one
    /// tiny point-filtered texture of flat colour cells through its UVs, so props of every colour
    /// share one material and can be merged into a single mesh (one draw call) on the plain URP Lit
    /// shader (no vertex-colour shader needed, nothing to strip from builds).
    /// </summary>
    public static class WorldMapPalette
    {
        public const int Columns = 8;
        public const int Rows = 4;
        private const int CellPixels = 4;

        private static readonly Color[] Colors =
        {
            new Color(0.45f, 0.31f, 0.2f),   // Trunk
            new Color(0.33f, 0.62f, 0.24f),  // Leaf
            new Color(0.22f, 0.47f, 0.2f),   // LeafDark
            new Color(0.46f, 0.72f, 0.3f),   // Grass
            new Color(0.29f, 0.56f, 0.26f),  // Bush
            new Color(0.98f, 0.82f, 0.32f),  // Flower
            new Color(0.36f, 0.6f, 0.3f),    // Cactus
            new Color(0.26f, 0.47f, 0.24f),  // CactusDark
            new Color(0.94f, 0.82f, 0.55f),  // Sand
            new Color(0.8f, 0.6f, 0.4f),     // SandRock
            new Color(0.56f, 0.55f, 0.54f),  // Rock
            new Color(0.4f, 0.4f, 0.42f),    // RockDark
            new Color(0.95f, 0.97f, 1f),     // Snow
            new Color(0.38f, 0.34f, 0.42f),  // CaveStone
            new Color(0.2f, 0.18f, 0.24f),   // CaveDark
            new Color(0.45f, 0.85f, 0.95f),  // Crystal
            new Color(0.78f, 0.3f, 0.22f),   // Roof
            new Color(0.95f, 0.88f, 0.72f),  // Wall
            new Color(0.62f, 0.44f, 0.27f),  // Wood
            new Color(0.42f, 0.29f, 0.18f),  // WoodDark
            new Color(0.88f, 0.78f, 0.6f),   // Ruin
            new Color(0.72f, 0.6f, 0.44f),   // RuinDark
            new Color(0.98f, 0.78f, 0.22f),  // Gold
            new Color(0.82f, 0.2f, 0.2f),    // Banner
            new Color(1f, 1f, 1f),           // Cloud
            new Color(0.84f, 0.88f, 0.94f),  // CloudShade
            new Color(0.7f, 0.72f, 0.78f),   // Fog
            WorldMapTerrain.DeepWater,       // DeepWater (same as the ground texture's open sea: seamless)
            new Color(0.05f, 0.04f, 0.06f),  // Black
            new Color(1f, 0.72f, 0.3f),      // Lantern
            new Color(0.2f, 0.45f, 0.3f),    // PineLeaf
            new Color(0.68f, 0.66f, 0.62f),  // Pebble
        };

        public static Color ColorOf(WorldMapSwatch swatch) => Colors[(int)swatch];

        /// <summary>UV at the centre of <paramref name="swatch"/>'s cell.</summary>
        public static Vector2 UvOf(WorldMapSwatch swatch)
        {
            int index = (int)swatch;
            int column = index % Columns;
            int row = index / Columns;
            return new Vector2((column + 0.5f) / Columns, (row + 0.5f) / Rows);
        }

        /// <summary>Builds the palette texture (point filtered, clamped, no mips so cells never bleed).</summary>
        public static Texture2D CreateTexture()
        {
            int width = Columns * CellPixels;
            int height = Rows * CellPixels;
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false)
            {
                name = "WorldMapPalette",
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
            };

            var pixels = new Color32[width * height];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int index = (y / CellPixels) * Columns + x / CellPixels;
                    pixels[y * width + x] = index < Colors.Length ? (Color32)Colors[index] : new Color32(255, 0, 255, 255);
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply(false, true);
            return texture;
        }
    }
}
