using System.Collections.Generic;
using Rapadura.Gameplay.World.Biomes;

namespace Rapadura.Gameplay.World.Cities.Town
{
    /// <summary>Which trees and shrubs a town grows (region-specific vegetation, Docs/WorldBible_Maniqueismo.md §2).</summary>
    public enum TownVegetation
    {
        /// <summary>Bosque do Zarafshan: tugai poplars (green and gold), pink tamarisk, apricot, mulberry and pomegranate orchards.</summary>
        Forest = 0,
        /// <summary>Areias Flamejantes: date palms, tamarisk, desert poplar, camel thorn.</summary>
        Desert = 1,
        /// <summary>Serra de Kaggen: twisted mountain cedars, tree aloes in red flower, protea.</summary>
        Mountain = 2,
        /// <summary>Grutas do Limiar: glowing blue mushrooms, fluorescent moss, juniper at the mouth.</summary>
        Caves = 3,
    }

    /// <summary>The "shape signature" of a region's houses (world bible §2.5): what the kit builds for a plain house.</summary>
    public enum TownArchitecture
    {
        /// <summary>Zarafshan: adobe blocks, flat roofs with parapets, aivans on carved columns, pishtaq arches, domes.</summary>
        Sogdian = 0,
        /// <summary>Qocho: rose taipa, latticed grape-drying walls, cistern domes.</summary>
        Oasis = 1,
        /// <summary>Litema: round stone-and-clay rondavels with conical thatch and painted litema bands; dry-stone walls.</summary>
        Rondavel = 2,
        /// <summary>Lamparinas: houses cut into pink tuff, arched doors, round windows, lamp niches.</summary>
        CaveTuff = 3,
    }

    /// <summary>
    /// A region's "material set" for the town kit: which palette cells walls, trims, roofs, doors, columns, ground,
    /// streets and plaza use, the house shape (<see cref="TownArchitecture"/>) and the vegetation. Each building picks
    /// its colours from these lists by a stable hash (<see cref="Pick"/>), so the same town always looks the same and
    /// neighbours rarely repeat. Data only: a new city is a new style plus a layout recipe (Docs/Cities.md §5).
    /// </summary>
    public sealed class TownStyle
    {
        public BiomeType Region;
        public TownVegetation Vegetation;
        public TownArchitecture Architecture;

        public TownSwatch[] Walls;

        /// <summary>Plinth band, arch frames and painted bands.</summary>
        public TownSwatch[] Trims;

        public TownSwatch[] Roofs;

        /// <summary>Doors, shutters and carved columns.</summary>
        public TownSwatch[] Joinery;

        /// <summary>The small precious accent (turquoise tile, old gold, blanket blue, copper).</summary>
        public TownSwatch Accent = TownSwatch.Turquoise;

        public TownSwatch Ground = TownSwatch.Grass;
        public TownSwatch GroundAlt = TownSwatch.GrassDark;
        public TownSwatch Street = TownSwatch.Dirt;
        public TownSwatch StreetEdge = TownSwatch.Cobble;
        public TownSwatch Sidewalk = TownSwatch.Paving;
        public TownSwatch Plaza = TownSwatch.Paving;
        public TownSwatch PlazaAlt = TownSwatch.PavingDark;

        /// <summary>Yard walls (adobe / dry stone).</summary>
        public TownSwatch Fence = TownSwatch.Stone;
        public TownSwatch Plinth = TownSwatch.Stone;

        /// <summary>Roof of a plain house in this region.</summary>
        public TownRoofKind HouseRoof = TownRoofKind.Flat;

        /// <summary>An aryk (narrow water channel) runs along the streets.</summary>
        public bool Aryk;

        /// <summary>Deterministic pick from <paramref name="options"/> (FNV-1a of the key, never a runtime RNG).</summary>
        public static TownSwatch Pick(TownSwatch[] options, string key, int salt = 0)
        {
            if (options == null || options.Length == 0)
            {
                return TownSwatch.Adobe;
            }

            return options[(int)(Hash(key, salt) % (uint)options.Length)];
        }

        public static uint Hash(string key, int salt = 0)
        {
            unchecked
            {
                uint hash = 2166136261u ^ (uint)salt * 2654435761u;
                if (key != null)
                {
                    foreach (char c in key)
                    {
                        hash ^= c;
                        hash *= 16777619u;
                    }
                }

                hash ^= hash >> 13;
                hash *= 0x5bd1e995u;
                hash ^= hash >> 15;
                return hash;
            }
        }

        /// <summary>0..1 from the key (stable).</summary>
        public static float Hash01(string key, int salt = 0) => (Hash(key, salt) & 0xFFFFFF) / (float)0x1000000;
    }

    /// <summary>The four regional material sets (palette hex codes from the world bible §2).</summary>
    public static class TownStyles
    {
        private static readonly Dictionary<BiomeType, TownStyle> Styles = new Dictionary<BiomeType, TownStyle>
        {
            [BiomeType.Forest] = new TownStyle
            {
                Region = BiomeType.Forest,
                Vegetation = TownVegetation.Forest,
                Architecture = TownArchitecture.Sogdian,
                Walls = new[] { TownSwatch.Adobe, TownSwatch.AdobeOchre, TownSwatch.Plaster, TownSwatch.Adobe, TownSwatch.TaipaRose },
                Trims = new[] { TownSwatch.Plaster, TownSwatch.LimeWhite, TownSwatch.AdobeOchre },
                Roofs = new[] { TownSwatch.Mud },
                Joinery = new[] { TownSwatch.Walnut, TownSwatch.WoodDark, TownSwatch.Turquoise, TownSwatch.Walnut },
                Accent = TownSwatch.Turquoise,
                Ground = TownSwatch.Grass, GroundAlt = TownSwatch.GrassDark,
                Street = TownSwatch.Dirt, StreetEdge = TownSwatch.Cobble, Sidewalk = TownSwatch.Paving,
                Plaza = TownSwatch.Paving, PlazaAlt = TownSwatch.Brick,
                Fence = TownSwatch.Adobe, Plinth = TownSwatch.Stone,
                HouseRoof = TownRoofKind.Flat, Aryk = true,
            },
            [BiomeType.Desert] = new TownStyle
            {
                Region = BiomeType.Desert,
                Vegetation = TownVegetation.Desert,
                Architecture = TownArchitecture.Oasis,
                Walls = new[] { TownSwatch.TaipaRose, TownSwatch.Adobe, TownSwatch.Sandstone, TownSwatch.TaipaRose },
                Trims = new[] { TownSwatch.Sandstone, TownSwatch.Plaster },
                Roofs = new[] { TownSwatch.Mud, TownSwatch.TaipaRose },
                Joinery = new[] { TownSwatch.Walnut, TownSwatch.Violet, TownSwatch.WoodDark },
                Accent = TownSwatch.Gold,
                Ground = TownSwatch.Sand, GroundAlt = TownSwatch.SandDark,
                Street = TownSwatch.SandDark, StreetEdge = TownSwatch.Sandstone, Sidewalk = TownSwatch.Plaster,
                Plaza = TownSwatch.Plaster, PlazaAlt = TownSwatch.TaipaRose,
                Fence = TownSwatch.TaipaRose, Plinth = TownSwatch.Sandstone,
                HouseRoof = TownRoofKind.Flat, Aryk = true,
            },
            [BiomeType.Mountain] = new TownStyle
            {
                Region = BiomeType.Mountain,
                Vegetation = TownVegetation.Mountain,
                Architecture = TownArchitecture.Rondavel,
                Walls = new[] { TownSwatch.RedClay, TownSwatch.LimeWhite, TownSwatch.RedClay, TownSwatch.Stone },
                Trims = new[] { TownSwatch.ClothIndigo, TownSwatch.ClothYellow, TownSwatch.LimeWhite },
                Roofs = new[] { TownSwatch.Thatch },
                Joinery = new[] { TownSwatch.WoodDark, TownSwatch.Walnut },
                Accent = TownSwatch.ClothIndigo,
                Ground = TownSwatch.Grass, GroundAlt = TownSwatch.Snow,
                Street = TownSwatch.DirtDark, StreetEdge = TownSwatch.Stone, Sidewalk = TownSwatch.Cobble,
                Plaza = TownSwatch.Cobble, PlazaAlt = TownSwatch.Stone,
                Fence = TownSwatch.Stone, Plinth = TownSwatch.Stone,
                HouseRoof = TownRoofKind.Cone,
            },
            [BiomeType.Caves] = new TownStyle
            {
                Region = BiomeType.Caves,
                Vegetation = TownVegetation.Caves,
                Architecture = TownArchitecture.CaveTuff,
                Walls = new[] { TownSwatch.PinkTuff, TownSwatch.PinkTuff, TownSwatch.TaipaRose },
                Trims = new[] { TownSwatch.Basalt, TownSwatch.Stone },
                Roofs = new[] { TownSwatch.PinkTuff },
                Joinery = new[] { TownSwatch.WoodDark, TownSwatch.Copper },
                Accent = TownSwatch.Copper,
                Ground = TownSwatch.Basalt, GroundAlt = TownSwatch.Indigo,
                Street = TownSwatch.DirtDark, StreetEdge = TownSwatch.Basalt, Sidewalk = TownSwatch.StoneDark,
                Plaza = TownSwatch.Cobble, PlazaAlt = TownSwatch.Basalt,
                Fence = TownSwatch.Basalt, Plinth = TownSwatch.Basalt,
                HouseRoof = TownRoofKind.Rock,
            },
        };

        public static TownStyle For(BiomeType region) => Styles.TryGetValue(region, out TownStyle style) ? style : Styles[BiomeType.Forest];
    }
}
