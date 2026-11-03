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
        /// <summary>Costa das Monções (Taa Pwani): leaning coconut palms, baobabs, casuarinas, sea-grape shrubs.</summary>
        Coast = 4,
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
        /// <summary>
        /// Taa Pwani (Swahili coast: Kilwa, Lamu, Gedi): whitewashed coral-stone houses, flat roof terraces with stepped
        /// merlons, carved teak doors studded with brass, a <i>baraza</i> (stone bench) along the front.
        /// </summary>
        CoralStone = 4,
        /// <summary>
        /// Tambor Rubro (the orcs' war camp, Docs/Mapas100.md §3.3): log halls with walls of ochre-dyed hide between corner
        /// posts, very steep hide gable roofs with crossed ridge poles, a palisade of sharpened logs, drum towers, clan tents.
        /// </summary>
        WarCamp = 5,
        /// <summary>
        /// Salões de Bronze de Napirasu (the dwarves' city cut into the mountain, Docs/Mapas100.md §3.3): granite halls whose
        /// fronts are carved into the rock, square bronze-capped pillars, massive bronze doors under a rune lintel that lights
        /// one line at a time, cold lava pits, chain lifts.
        /// </summary>
        RuneHall = 6,
        /// <summary>
        /// Mercado dos Túneis (the kobolds' market, Docs/Mapas100.md §3.3): round basalt tunnels, stalls set into niches under
        /// arches with canvas awnings, mine-cart rails, pulleys, copper air pipes with fans inside, bottle-glass lanterns.
        /// </summary>
        TunnelBazaar = 7,
        /// <summary>
        /// Aratta, the city of the horns (the povo-touro, Docs/Mapas100.md §3.3): great halls of reed bound in arches (the
        /// mudhif of the Mesopotamian marshes) covered in mats, round dry-stone enclosures, horn posts with ribbons as
        /// way-markers, the bull-headed lyre on the plaza.
        /// </summary>
        ReedHorn = 8,
        /// <summary>
        /// Kirta, the reef city (the povo-do-mar, Docs/Mapas100.md §3.3): round coral towers with round windows over a drowned
        /// base, mother-of-pearl domes, shell bridges, kelp gardens and the mother coral, half of it dimmed by the Lodo.
        /// </summary>
        CoralReef = 9,
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
                Ground = TownSwatch.Grass, GroundAlt = TownSwatch.GrassDark,
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

        /// <summary>
        /// Taa Pwani, the Costa da Lâmpada (Docs/Mapas100.md região 5): the first style of a region past the four biomes of the
        /// world map, so a recipe picks it by hand (<see cref="CityTownRecipe.Style"/>). Coral lime #EFE7DA, teak #6B4A2E,
        /// brass #C9A23F, sea #2E9BB0, sail #F2E6C9, laterite #B4583A. <see cref="TownStyle.Region"/> is the world-map biome
        /// the city borrows for its <c>CitySpec</c> (the Forest: Água Viva, and always unlocked).
        /// </summary>
        public static readonly TownStyle Coast = new TownStyle
        {
            Region = BiomeType.Forest,
            Vegetation = TownVegetation.Coast,
            Architecture = TownArchitecture.CoralStone,
            Walls = new[] { TownSwatch.CoralLime, TownSwatch.CoralLime, TownSwatch.LimeWhite, TownSwatch.Sail },
            Trims = new[] { TownSwatch.Laterite, TownSwatch.Sail, TownSwatch.Sea },
            Roofs = new[] { TownSwatch.CoralLime },
            Joinery = new[] { TownSwatch.Teak, TownSwatch.Teak, TownSwatch.WoodDark },
            Accent = TownSwatch.Gold,
            Ground = TownSwatch.Sand, GroundAlt = TownSwatch.SandDark,
            Street = TownSwatch.SandDark, StreetEdge = TownSwatch.CoralLime, Sidewalk = TownSwatch.Paving,
            Plaza = TownSwatch.CoralLime, PlazaAlt = TownSwatch.Laterite,
            Fence = TownSwatch.CoralLime, Plinth = TownSwatch.Laterite,
            HouseRoof = TownRoofKind.Flat,
        };

        /// <summary>
        /// Tambor Rubro, the orcs' war camp (Docs/Mapas100.md §3.3, map 11): picked by hand like <see cref="Coast"/>. The ficha's
        /// palette from the cells that already exist: ochre-red #A8452B (red clay / laterite), sandstone #C9774B (adobe ochre),
        /// leather #7A4A2A (teak), iron #4A4A4F, ember #FF8A3D, cloth band #E8C547 (cloth yellow). The Areias Flamejantes are
        /// a world-map biome, so <see cref="TownStyle.Region"/> is the Desert (date palms and tamarisks in the yards).
        /// </summary>
        public static readonly TownStyle WarCamp = new TownStyle
        {
            Region = BiomeType.Desert,
            Vegetation = TownVegetation.Desert,
            Architecture = TownArchitecture.WarCamp,
            Walls = new[] { TownSwatch.RedClay, TownSwatch.Laterite, TownSwatch.AdobeOchre },
            Trims = new[] { TownSwatch.Teak, TownSwatch.WoodDark },
            Roofs = new[] { TownSwatch.Teak, TownSwatch.WoodDark },
            Joinery = new[] { TownSwatch.WoodDark, TownSwatch.Teak, TownSwatch.Beam },
            Accent = TownSwatch.ClothYellow,
            Ground = TownSwatch.SandDark, GroundAlt = TownSwatch.AdobeOchre,
            Street = TownSwatch.Laterite, StreetEdge = TownSwatch.Sandstone, Sidewalk = TownSwatch.DirtDark,
            Plaza = TownSwatch.Sand, PlazaAlt = TownSwatch.Laterite,
            Fence = TownSwatch.WoodDark, Plinth = TownSwatch.StoneDark,
            HouseRoof = TownRoofKind.Gable,
        };

        /// <summary>
        /// Salões de Bronze de Napirasu, the dwarves' halls inside the Serra de Kaggen (Docs/Mapas100.md §3.3, map 17): picked by
        /// hand like <see cref="WarCamp"/>. The ficha's palette has its own cells (row 9 of <see cref="TownPalette"/>): granite
        /// #6E6A66, bronze #B07A3B, green copper #5E9C8A, cold ember #8C3B2E, rune light #FFE08A. Houses are fronts cut into the
        /// rock (<see cref="TownRoofKind.Rock"/>, <see cref="TownBuildingSpec.RuneHall"/>). Region: the Mountain.
        /// </summary>
        public static readonly TownStyle RuneHall = new TownStyle
        {
            Region = BiomeType.Mountain,
            Vegetation = TownVegetation.Caves,
            Architecture = TownArchitecture.RuneHall,
            Walls = new[] { TownSwatch.Granite, TownSwatch.Granite, TownSwatch.StoneDark },
            Trims = new[] { TownSwatch.Bronze, TownSwatch.Verdigris },
            Roofs = new[] { TownSwatch.Granite },
            Joinery = new[] { TownSwatch.Bronze, TownSwatch.Verdigris },
            Accent = TownSwatch.RuneLight,
            Ground = TownSwatch.StoneDark, GroundAlt = TownSwatch.Granite,
            Street = TownSwatch.Granite, StreetEdge = TownSwatch.Bronze, Sidewalk = TownSwatch.StoneDark,
            Plaza = TownSwatch.Stone, PlazaAlt = TownSwatch.Granite,
            Fence = TownSwatch.Granite, Plinth = TownSwatch.StoneDark,
            HouseRoof = TownRoofKind.Rock,
        };

        /// <summary>
        /// Mercado dos Túneis, the kobolds' market at the crossing of the four galleries of the Grutas do Limiar (Docs/Mapas100.md
        /// §3.3, map 22): picked by hand. Basalt #3B3B44, copper #B06B3A, canvas #C9B48A (sail cloth / thatch), bottle glass green
        /// #7FBF8F and amber #E0A040, Ar Sereno #C9B8F0. Houses are stalls in arched rock niches (<see cref="TownRoofKind.Rock"/>,
        /// <see cref="TownBuildingSpec.TunnelStall"/>). Region: the Caves.
        /// </summary>
        public static readonly TownStyle TunnelBazaar = new TownStyle
        {
            Region = BiomeType.Caves,
            Vegetation = TownVegetation.Caves,
            Architecture = TownArchitecture.TunnelBazaar,
            Walls = new[] { TownSwatch.Basalt, TownSwatch.Basalt, TownSwatch.StoneDark },
            Trims = new[] { TownSwatch.Copper, TownSwatch.Stone },
            Roofs = new[] { TownSwatch.Basalt },
            Joinery = new[] { TownSwatch.WoodDark, TownSwatch.Copper },
            Accent = TownSwatch.GlassAmber,
            Ground = TownSwatch.Basalt, GroundAlt = TownSwatch.StoneDark,
            Street = TownSwatch.DirtDark, StreetEdge = TownSwatch.Copper, Sidewalk = TownSwatch.StoneDark,
            Plaza = TownSwatch.Cobble, PlazaAlt = TownSwatch.Basalt,
            Fence = TownSwatch.Basalt, Plinth = TownSwatch.Basalt,
            HouseRoof = TownRoofKind.Rock,
        };
    
        /// <summary>
        /// Aratta, the city of the horns (Docs/Mapas100.md §3.3, map 26): picked by hand like <see cref="Coast"/>. Bound reed
        /// #C8B27A, reed mat #A88B55, red clay #9C4A2F, golden grass #D9B44A, ribbon blue #3F7CC0 (row 10 of
        /// <see cref="TownPalette"/>). Houses are reed halls (<see cref="TownBuildingSpec.ReedHall"/>). The Costa das Monções has no
        /// world-map biome: <see cref="TownStyle.Region"/> is the borrowed Forest (as Taa Pwani); acacias and baobabs in the yards.
        /// </summary>
        public static readonly TownStyle ReedHorn = new TownStyle
        {
            Region = BiomeType.Forest,
            Vegetation = TownVegetation.Coast,
            Architecture = TownArchitecture.ReedHorn,
            Walls = new[] { TownSwatch.Reed, TownSwatch.Reed, TownSwatch.ReedMat },
            Trims = new[] { TownSwatch.RedClay, TownSwatch.Ribbon },
            Roofs = new[] { TownSwatch.ReedMat, TownSwatch.Reed },
            Joinery = new[] { TownSwatch.Reed, TownSwatch.WoodDark },
            Accent = TownSwatch.Ribbon,
            Ground = TownSwatch.GoldGrass, GroundAlt = TownSwatch.Thatch,
            Street = TownSwatch.RedClay, StreetEdge = TownSwatch.Reed, Sidewalk = TownSwatch.SandDark,
            Plaza = TownSwatch.Sand, PlazaAlt = TownSwatch.RedClay,
            Fence = TownSwatch.Stone, Plinth = TownSwatch.RedClay,
            HouseRoof = TownRoofKind.Gable,
        };

        /// <summary>
        /// Kirta, the reef city on its atoll (Docs/Mapas100.md §3.3, map 28): picked by hand. Live coral #E8836B, mother-of-pearl
        /// (pearl), reef turquoise #2FB3B3, kelp #3E7D57, sand #E9D3A2, dimmed coral #9A9590 (row 10 of <see cref="TownPalette"/>).
        /// Houses are coral towers (<see cref="TownBuildingSpec.ReefTower"/>). Region: the borrowed Forest (as Taa Pwani).
        /// </summary>
        public static readonly TownStyle CoralReef = new TownStyle
        {
            Region = BiomeType.Forest,
            Vegetation = TownVegetation.Coast,
            Architecture = TownArchitecture.CoralReef,
            Walls = new[] { TownSwatch.CoralLive, TownSwatch.CoralLive, TownSwatch.CoralDim },
            Trims = new[] { TownSwatch.ReefTeal, TownSwatch.Pearl },
            Roofs = new[] { TownSwatch.Pearl },
            Joinery = new[] { TownSwatch.Kelp, TownSwatch.Teak },
            Accent = TownSwatch.ReefTeal,
            Ground = TownSwatch.Sand, GroundAlt = TownSwatch.SandDark,
            Street = TownSwatch.Pearl, StreetEdge = TownSwatch.ReefTeal, Sidewalk = TownSwatch.Sand,
            Plaza = TownSwatch.Pearl, PlazaAlt = TownSwatch.ReefTeal,
            Fence = TownSwatch.CoralDim, Plinth = TownSwatch.CoralDim,
            HouseRoof = TownRoofKind.Dome,
        };
    }
}
