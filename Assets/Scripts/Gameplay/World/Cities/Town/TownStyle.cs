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
        /// <summary>
        /// Jhilpur, the city in the roots (the Delta de Jhilpur, Docs/Mapas100.md §2.1 and map 31): plank houses raised on
        /// root-columns over the murky water, curved "boat-back" thatch roofs (the do-chala of the Bengal delta), brick ghats
        /// stepping down into the water, living-root bridges over the canals, banyans with aerial roots and the Raiz-Mãe.
        /// </summary>
        RootStilt = 10,
        /// <summary>
        /// Palafitas de Tarchon (the trols' city in the open swamp, Docs/Mapas100.md §3.3 and map 33, style <c>StiltVillage</c>):
        /// long houses on tall timber piles over the bog water, their gable end to the street and a steep leaf roof that comes
        /// down to the floor, plank boardwalks, fishing nets drying, giant baskets, floating reed islands and the will-o'-wisps
        /// (sparks caught in the Ar Sereno) in lanterns.
        /// </summary>
        StiltVillage = 11,
        /// <summary>
        /// Sangkent, the stair city (the Teto do Mundo, Docs/Mapas100.md §2.1 and map 36, style <c>StoneTerrace</c>): stone and
        /// loam houses of two storeys with timber lacing and beam ends showing, lattice balconies, flat roofs with apricots
        /// drying; dry-stone terraces stepping up to the fort, polished stone chutes and plank carts between the levels,
        /// irrigation channels and apricot trees in blossom (the Pamir and Hunza villages, the tower houses of Shibam).
        /// </summary>
        StoneTerrace = 12,
        /// <summary>
        /// Mingcheng, the city of nine rings (the Planície de Mingcheng, Docs/Mapas100.md §2.1 and map 41, style <c>RingWall</c>):
        /// halls of grey brick on a stone base with red-lacquered columns and lattice windows, curved grey-tile roofs with
        /// upturned eaves (golden glazed tile only on the inner ring), paper lanterns under the eaves; the rammed-earth ring
        /// walls faced with grey brick and their three-arch gates, the Torre do Tambor e do Sino on the plaza, humpback stone
        /// bridges over the canals, willows and lantern frames (the walled cities of China: Chang'an, Xi'an, the Forbidden City).
        /// </summary>
        RingWall = 13,
        /// <summary>
        /// The Torres Brancas de Amanirenas, the high elves' city (Docs/Mapas100.md §3.3 and map 42, style <c>WhiteSpire</c>):
        /// slender lime-white towers with a glass dome on a low pointed pyramid base (an echo of Meroë), pale-gold bands and
        /// mirrors that pass the sunlight down; garden terraces with the seven spires behind, leaf bridges (curved stone blades),
        /// the Relógio de Espelhos on the plaza, mirror posts and small steep pyramids in the gardens.
        /// </summary>
        WhiteSpire = 14,
        /// <summary>
        /// The Acampamento de Soslan, the centaurs' camp (the Estepe dos Nartes, Docs/Mapas100.md §3.3 and map 47, style
        /// <c>SteppeCamp</c>): big round felt tents on wagons with spoked wheels (the camp that moves across the steppe), dyed
        /// felt bands red and blue, horsehair standards, rope corrals and the collective stone oven (the yurts and kibitkas of
        /// the steppe, the Scythian wagon tents, the Nart sagas of Ossetia).
        /// </summary>
        SteppeCamp = 15,
        /// <summary>
        /// The Fortim dos Gigantes-da-Colina (the Estepe dos Nartes, Docs/Mapas100.md §3.3 and map 49, style <c>HillFort</c>):
        /// round giant houses of dry stone under turf roofs, much bigger than a cousin's house, earth ramparts in rings faced
        /// with stone, stone tables, pots the size of a house over the Living Fire and giant spoons as benches (the hill forts
        /// and brochs of the Iron Age; the penitent giants of the Manichaean Book of Giants).
        /// </summary>
        HillFort = 16,
        /// <summary>
        /// The Arvoredo de Haripushpa, the trunk folk's grove (the Estepe dos Nartes, Docs/Mapas100.md §3.3 and map 50, style
        /// <c>LivingGrove</c>): houses inside huge hollow trunks with root steps and nest windows under a crown of larch, mossy
        /// standing stones in a ring, meltwater streams with log bridges (the taiga at the edge of the steppe; the kings of
        /// Kucha).
        /// </summary>
        LivingGrove = 17,
        /// <summary>
        /// Farnkesh, os Ninhos de Pedra (the Picos de Farnkesh, Docs/Mapas100.md map 54, style <c>PinnacleNest</c>): white houses
        /// with leaning walls on sandstone pinnacles above the clouds, painted red wood windows, plank roofs weighed with stones,
        /// balconies over the void, rope bridges over the cloud gaps, rope basket lifts and the giant wind bell of the plaza
        /// (the dzongs of Bhutan and the rocks of Meteora as form only: houses of people who like wind and silence; no cult).
        /// </summary>
        PinnacleNest = 18,
        /// <summary>
        /// The Vale dos Sátiros (the Picos de Farnkesh, Docs/Mapas100.md §3.3 and map 53, style <c>Glade</c>): clearings in a ring
        /// with gourd-shaped wicker houses, stone stages for the dance, rows of vines, broad oaks and the river of golden
        /// pebbles (the Phrygian myths of Marsyas, Gordias and Midas; the Pactolus).
        /// </summary>
        Glade = 19,
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

        /// <summary>
        /// Jhilpur, the city in the roots (Docs/Mapas100.md §2.1, map 31): picked by hand. Root green #4E6B3A, ghat brick #A4553A,
        /// palm straw #C9A35B, murky water #5F7F6E, the Living Water #4FC3B0, lotus lanterns #FFC46B (row 11 of
        /// <see cref="TownPalette"/>). Houses stand on root-columns (<see cref="TownBuildingSpec.RootStilt"/>); streets are plank
        /// walkways, the plaza a round platform of planks round the Raiz-Mãe. The Delta de Jhilpur has no world-map biome:
        /// <see cref="TownStyle.Region"/> is the borrowed Forest (as Taa Pwani).
        /// </summary>
        public static readonly TownStyle RootStilt = new TownStyle
        {
            Region = BiomeType.Forest,
            Vegetation = TownVegetation.Coast,
            Architecture = TownArchitecture.RootStilt,
            Walls = new[] { TownSwatch.Straw, TownSwatch.WoodWeathered, TownSwatch.Straw },
            Trims = new[] { TownSwatch.GhatBrick, TownSwatch.LivingWater },
            Roofs = new[] { TownSwatch.Thatch, TownSwatch.Straw },
            Joinery = new[] { TownSwatch.RootBark, TownSwatch.WoodDark },
            Accent = TownSwatch.LivingWater,
            Ground = TownSwatch.RootGreen, GroundAlt = TownSwatch.Mud,
            Street = TownSwatch.WoodWeathered, StreetEdge = TownSwatch.RootBark, Sidewalk = TownSwatch.Mud,
            Plaza = TownSwatch.WoodLight, PlazaAlt = TownSwatch.RootBark,
            Fence = TownSwatch.RootBark, Plinth = TownSwatch.GhatBrick,
            HouseRoof = TownRoofKind.Gable,
        };

        /// <summary>
        /// Palafitas de Tarchon, the trols' city (Docs/Mapas100.md §3.3, map 33): picked by hand. Moss blue #4F7A82, wet wood
        /// #5B4636, leaf #6F8F3F, lilac mist #B9A9D6, will-o'-wisp #D6E8FF (glows), bog water #3F5550, rush #B9A55A and net
        /// twine #8E8466 (row 12 of <see cref="TownPalette"/>). Houses are long houses on tall piles
        /// (<see cref="TownBuildingSpec.StiltLonghouse"/>); streets are plank boardwalks, the plaza a round deck of wet planks
        /// round the Mastro dos Fogos-Fátuos. The Delta de Jhilpur has no world-map biome: <see cref="TownStyle.Region"/> is the
        /// borrowed Forest (as Jhilpur).
        /// </summary>
        public static readonly TownStyle StiltVillage = new TownStyle
        {
            Region = BiomeType.Forest,
            Vegetation = TownVegetation.Coast,
            Architecture = TownArchitecture.StiltVillage,
            Walls = new[] { TownSwatch.Rush, TownSwatch.WetWood, TownSwatch.Rush },
            Trims = new[] { TownSwatch.TrollMoss, TownSwatch.LilacMist },
            Roofs = new[] { TownSwatch.SwampLeaf, TownSwatch.Rush },
            Joinery = new[] { TownSwatch.WetWood, TownSwatch.WoodDark },
            Accent = TownSwatch.WillWisp,
            Ground = TownSwatch.SwampLeaf, GroundAlt = TownSwatch.Mud,
            Street = TownSwatch.WoodWeathered, StreetEdge = TownSwatch.WetWood, Sidewalk = TownSwatch.Mud,
            Plaza = TownSwatch.WetWood, PlazaAlt = TownSwatch.WoodWeathered,
            Fence = TownSwatch.WetWood, Plinth = TownSwatch.BogWater,
            HouseRoof = TownRoofKind.Gable,
        };

        /// <summary>
        /// Sangkent, the stair city (Docs/Mapas100.md §2.1, map 36): picked by hand. Terrace stone #8C8173, loam #B98B5E, juniper
        /// #5A3E2B, firn #E6EEF2, apricot blossom #F2B8A0, the caravans' silk red #C0392B and blue #2E86AB, the hearth glow
        /// #FFB866 (row 13 of <see cref="TownPalette"/>). Houses are stone-and-loam houses of two storeys with timber lacing
        /// (<see cref="TownBuildingSpec.StoneTerrace"/>); streets are stone paving, the plaza (the 4th terrace) flagstones
        /// round the Marco das Caravanas. The Teto do Mundo has no world-map biome: <see cref="TownStyle.Region"/> is the
        /// borrowed Forest (as Jhilpur).
        /// </summary>
        public static readonly TownStyle StoneTerrace = new TownStyle
        {
            Region = BiomeType.Forest,
            Vegetation = TownVegetation.Mountain,
            Architecture = TownArchitecture.StoneTerrace,
            Walls = new[] { TownSwatch.TerraceStone, TownSwatch.Loam, TownSwatch.TerraceStone },
            Trims = new[] { TownSwatch.Juniper, TownSwatch.SilkRed },
            Roofs = new[] { TownSwatch.Loam, TownSwatch.Mud },
            Joinery = new[] { TownSwatch.Juniper, TownSwatch.WoodDark },
            Accent = TownSwatch.SilkBlue,
            Ground = TownSwatch.GrassDark, GroundAlt = TownSwatch.Loam,
            Street = TownSwatch.Cobble, StreetEdge = TownSwatch.TerraceStone, Sidewalk = TownSwatch.Loam,
            Plaza = TownSwatch.Paving, PlazaAlt = TownSwatch.TerraceStone,
            Fence = TownSwatch.TerraceStone, Plinth = TownSwatch.StoneDark,
            HouseRoof = TownRoofKind.Flat,
        };

        /// <summary>
        /// Mingcheng, the city of nine rings (Docs/Mapas100.md §2.1, map 41): picked by hand. Grey brick #7D7F86, lacquer red
        /// #B23A2E, golden tile #D9A82B, jade #5E9E7E, lantern paper #F4E3B5 (glows), grey roof tile #4E525A, willow #8DB25A and
        /// canal water #6FA3A8 (row 14 of <see cref="TownPalette"/>). Houses are courtyard halls of grey brick with lacquered
        /// columns and curved tile roofs (<see cref="TownBuildingSpec.RingWallHall"/>); streets are grey paving, the plaza (the
        /// 6th ring) flagstones round the Torre do Tambor e do Sino. The Planície de Mingcheng has no world-map biome:
        /// <see cref="TownStyle.Region"/> is the borrowed Forest (as Jhilpur and Sangkent).
        /// </summary>
        public static readonly TownStyle RingWall = new TownStyle
        {
            Region = BiomeType.Forest,
            Vegetation = TownVegetation.Forest,
            Architecture = TownArchitecture.RingWall,
            Walls = new[] { TownSwatch.GreyBrick, TownSwatch.Plaster, TownSwatch.GreyBrick },
            Trims = new[] { TownSwatch.Lacquer, TownSwatch.Jade },
            Roofs = new[] { TownSwatch.RoofTile, TownSwatch.GoldTile },
            Joinery = new[] { TownSwatch.Lacquer, TownSwatch.WoodDark },
            Accent = TownSwatch.GoldTile,
            Ground = TownSwatch.Grass, GroundAlt = TownSwatch.Willow,
            Street = TownSwatch.Paving, StreetEdge = TownSwatch.GreyBrick, Sidewalk = TownSwatch.PavingDark,
            Plaza = TownSwatch.Paving, PlazaAlt = TownSwatch.GreyBrick,
            Fence = TownSwatch.GreyBrick, Plinth = TownSwatch.StoneDark,
            HouseRoof = TownRoofKind.Gable,
        };

        /// <summary>
        /// The Torres Brancas de Amanirenas (Docs/Mapas100.md §3.3, map 42): picked by hand. Lime white #F3EFE6, pale gold
        /// #E3C46B, sky blue #7FB2E5, silver leaf #A8C5B5, Meroë sandstone #C79E6B, the sunlight #FFF4C2 (glows), mirror bronze
        /// #B58B4C and garden green #5F8F4E (row 15 of <see cref="TownPalette"/>). Houses are white spire houses on a pyramid
        /// base with a glass dome (<see cref="TownBuildingSpec.SpireHouse"/>); streets are sandstone paving, the plaza lime
        /// flagstones round the Relógio de Espelhos. Region: the borrowed Forest (the Planície de Mingcheng has no world-map
        /// biome).
        /// </summary>
        public static readonly TownStyle WhiteSpire = new TownStyle
        {
            Region = BiomeType.Forest,
            Vegetation = TownVegetation.Forest,
            Architecture = TownArchitecture.WhiteSpire,
            Walls = new[] { TownSwatch.SpireLime, TownSwatch.SpireLime, TownSwatch.MeroeSandstone },
            Trims = new[] { TownSwatch.PaleGold, TownSwatch.SkyBlue },
            Roofs = new[] { TownSwatch.Glass, TownSwatch.PaleGold },
            Joinery = new[] { TownSwatch.MirrorBronze, TownSwatch.PaleGold },
            Accent = TownSwatch.SkyBlue,
            Ground = TownSwatch.GardenGreen, GroundAlt = TownSwatch.Grass,
            Street = TownSwatch.MeroeSandstone, StreetEdge = TownSwatch.SpireLime, Sidewalk = TownSwatch.Sandstone,
            Plaza = TownSwatch.SpireLime, PlazaAlt = TownSwatch.MeroeSandstone,
            Fence = TownSwatch.SpireLime, Plinth = TownSwatch.MeroeSandstone,
            HouseRoof = TownRoofKind.Dome,
        };

        /// <summary>
        /// The Acampamento de Soslan (Docs/Mapas100.md §3.3, map 47): picked by hand. Felt #D8C9A8, the dyed felts #8E2F2F and
        /// #2F5E8E, steppe grass #BFAE5A, horsehair #3A2E26, rawhide #6E4A2E, Scythian gold #D4A93A and the camp fire #FF9A4A
        /// (glows; row 16 of <see cref="TownPalette"/>). Houses are felt tents on wagons (<see cref="TownBuildingSpec.FeltYurt"/>);
        /// streets are trodden earth, the plaza packed earth round the stone oven. The Estepe dos Nartes has no world-map biome:
        /// <see cref="TownStyle.Region"/> is the borrowed Forest (as the cities of regions 5–8).
        /// </summary>
        public static readonly TownStyle SteppeCamp = new TownStyle
        {
            Region = BiomeType.Forest,
            Vegetation = TownVegetation.Forest,
            Architecture = TownArchitecture.SteppeCamp,
            Walls = new[] { TownSwatch.Felt, TownSwatch.Felt, TownSwatch.FeltRed },
            Trims = new[] { TownSwatch.FeltRed, TownSwatch.FeltBlue },
            Roofs = new[] { TownSwatch.Felt, TownSwatch.FeltBlue },
            Joinery = new[] { TownSwatch.SteppeLeather, TownSwatch.WoodDark },
            Accent = TownSwatch.ScythianGold,
            Ground = TownSwatch.SteppeGrass, GroundAlt = TownSwatch.GoldGrass,
            Street = TownSwatch.Dirt, StreetEdge = TownSwatch.SteppeGrass, Sidewalk = TownSwatch.DirtDark,
            Plaza = TownSwatch.Dirt, PlazaAlt = TownSwatch.DirtDark,
            Fence = TownSwatch.SteppeLeather, Plinth = TownSwatch.WoodDark,
            HouseRoof = TownRoofKind.Flat,
        };

        /// <summary>
        /// The Fortim dos Gigantes-da-Colina (Docs/Mapas100.md §3.3, map 49): picked by hand. Earth #8A6A44, stone #9A948A, the
        /// good smoke #CFC6B8, the Living Fire's embers #E2763A (glow), hill grass #7F9A4A, giant timber #7A5634, turf #5F7A3A and
        /// pot iron #4A4A4E (row 17 of <see cref="TownPalette"/>). Houses are round giant houses of dry stone under turf
        /// (<see cref="TownBuildingSpec.GiantRoundhouse"/>); streets are flagstones as big as a cart, the plaza packed earth
        /// round the great pot. Region: the borrowed Forest.
        /// </summary>
        public static readonly TownStyle HillFort = new TownStyle
        {
            Region = BiomeType.Forest,
            Vegetation = TownVegetation.Mountain,
            Architecture = TownArchitecture.HillFort,
            Walls = new[] { TownSwatch.FortStone, TownSwatch.FortStone, TownSwatch.FortEarth },
            Trims = new[] { TownSwatch.GiantWood, TownSwatch.FortEarth },
            Roofs = new[] { TownSwatch.TurfRoof, TownSwatch.HillGrass },
            Joinery = new[] { TownSwatch.GiantWood, TownSwatch.WoodDark },
            Accent = TownSwatch.LivingFire,
            Ground = TownSwatch.HillGrass, GroundAlt = TownSwatch.TurfRoof,
            Street = TownSwatch.FortStone, StreetEdge = TownSwatch.FortEarth, Sidewalk = TownSwatch.FortEarth,
            Plaza = TownSwatch.FortEarth, PlazaAlt = TownSwatch.FortStone,
            Fence = TownSwatch.FortStone, Plinth = TownSwatch.FortStone,
            HouseRoof = TownRoofKind.Flat,
        };

        /// <summary>
        /// The Arvoredo de Haripushpa (Docs/Mapas100.md §3.3, map 50): picked by hand. Bark #5E4A3A, moss #6E8F4A, new leaf
        /// #A9C96B, meltwater #86C9D6 (glows a little: the Living Water), flower #F0D27A, larch #4F6B3A, lichen #B7BFA0 and the
        /// dark of a hollow #2E2620 (row 18 of <see cref="TownPalette"/>). Houses are hollow giant trunks
        /// (<see cref="TownBuildingSpec.HollowTrunk"/>); streets are bark-strewn trails, the plaza moss round the circle of
        /// stones. Region: the borrowed Forest.
        /// </summary>
        public static readonly TownStyle LivingGrove = new TownStyle
        {
            Region = BiomeType.Forest,
            Vegetation = TownVegetation.Forest,
            Architecture = TownArchitecture.LivingGrove,
            Walls = new[] { TownSwatch.GroveBark, TownSwatch.GroveBark, TownSwatch.Lichen },
            Trims = new[] { TownSwatch.GroveMoss, TownSwatch.GroveFlower },
            Roofs = new[] { TownSwatch.Larch, TownSwatch.NewLeaf },
            Joinery = new[] { TownSwatch.GroveBark, TownSwatch.WoodDark },
            Accent = TownSwatch.GroveFlower,
            Ground = TownSwatch.GroveMoss, GroundAlt = TownSwatch.Moss,
            Street = TownSwatch.DirtDark, StreetEdge = TownSwatch.GroveMoss, Sidewalk = TownSwatch.Lichen,
            Plaza = TownSwatch.GroveMoss, PlazaAlt = TownSwatch.Lichen,
            Fence = TownSwatch.GroveBark, Plinth = TownSwatch.GroveBark,
            HouseRoof = TownRoofKind.Flat,
        };

        /// <summary>
        /// Farnkesh, os Ninhos de Pedra (Docs/Mapas100.md map 54): picked by hand from the ficha's palette. Lime #F1ECE2, painted
        /// red wood #9A3B2E, gold #D9A82B, pinnacle sandstone #B58A60, cloud #EEF3F7, the high sky #8FB8E0, yak rope #8A7A5E and
        /// bell bronze #A87B3C (row 19 of <see cref="TownPalette"/>). Houses stand on sandstone pinnacles
        /// (<see cref="TownBuildingSpec.PinnacleNest"/>); the ground between them is the sea of clouds, the streets are plank
        /// walks on the rock, the plaza the sandstone top of the largest pinnacle round the wind bell. The Picos de Farnkesh have no
        /// world-map biome: <see cref="TownStyle.Region"/> is the borrowed Forest (as the cities of regions 5–9).
        /// </summary>
        public static readonly TownStyle PinnacleNest = new TownStyle
        {
            Region = BiomeType.Forest,
            Vegetation = TownVegetation.Mountain,
            Architecture = TownArchitecture.PinnacleNest,
            Walls = new[] { TownSwatch.NestLime, TownSwatch.NestLime, TownSwatch.LimeWhite },
            Trims = new[] { TownSwatch.NestRed, TownSwatch.NestRed, TownSwatch.NestGold },
            Roofs = new[] { TownSwatch.WoodWeathered, TownSwatch.YakRope },
            Joinery = new[] { TownSwatch.NestRed, TownSwatch.WoodDark },
            Accent = TownSwatch.NestGold,
            Ground = TownSwatch.CloudWhite, GroundAlt = TownSwatch.HighSky,
            Street = TownSwatch.PinnacleStone, StreetEdge = TownSwatch.WoodWeathered, Sidewalk = TownSwatch.Sandstone,
            Plaza = TownSwatch.PinnacleStone, PlazaAlt = TownSwatch.Sandstone,
            Fence = TownSwatch.YakRope, Plinth = TownSwatch.PinnacleStone,
            HouseRoof = TownRoofKind.Flat,
        };

        /// <summary>
        /// The Vale dos Sátiros (Docs/Mapas100.md §3.3, map 53): picked by hand from the ficha's palette. Glade green #9CCB6A,
        /// wicker #C9A76A, stage stone #B7AFA3, the gold of the river #E3B341 (glows a little), flower #E27A9B, grape #5E3A6E,
        /// vine leaf #5E8F3A and the fireflies #F4F09A (glow; row 20 of <see cref="TownPalette"/>). Houses are gourd-shaped wicker
        /// huts (<see cref="TownBuildingSpec.GourdHut"/>); streets are grass paths between the clearings, the plaza the round
        /// clearing of the dance. Region: the borrowed Forest.
        /// </summary>
        public static readonly TownStyle Glade = new TownStyle
        {
            Region = BiomeType.Forest,
            Vegetation = TownVegetation.Forest,
            Architecture = TownArchitecture.Glade,
            Walls = new[] { TownSwatch.Wicker, TownSwatch.Wicker, TownSwatch.Straw },
            Trims = new[] { TownSwatch.VineFlower, TownSwatch.VineLeaf },
            Roofs = new[] { TownSwatch.VineLeaf, TownSwatch.GladeGreen },
            Joinery = new[] { TownSwatch.WoodWeathered, TownSwatch.Wicker },
            Accent = TownSwatch.PactolusGold,
            Ground = TownSwatch.GladeGreen, GroundAlt = TownSwatch.Grass,
            Street = TownSwatch.Dirt, StreetEdge = TownSwatch.GladeGreen, Sidewalk = TownSwatch.GladeStone,
            Plaza = TownSwatch.GladeGreen, PlazaAlt = TownSwatch.GladeStone,
            Fence = TownSwatch.Wicker, Plinth = TownSwatch.GladeStone,
            HouseRoof = TownRoofKind.Flat,
        };
    }
}
