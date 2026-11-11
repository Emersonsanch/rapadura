using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rapadura.Gameplay.World.Map;
using Rapadura.Gameplay.World.Map.Overworld;

namespace Rapadura.Tests
{
    /// <summary>
    /// The level numbers of <see cref="MapCatalog"/> (band of monsters and walk level of each map, the ficha of
    /// Docs/Mapas100.md §1 and the table of Docs/MetodologiaMapasERacas.md Part II) and the ones of the continuous world areas
    /// that ARE that map (<see cref="OverworldZone.MapId"/> equal to the map's id, <see cref="OverworldLayout.ZonesOfMap"/>)
    /// say the same thing: the Barcas de Luz window, the HUD and the walk barrier must agree.
    /// <para>
    /// Tolerance (decided 2026-11-11): the walk level of the map is exactly the lowest walk level of its areas (the entrance
    /// is a gate, one level off already lets a hero in where the barca says no); the band of the map is the union of its
    /// areas' bands with up to ±2 at each end (an area is a piece of the map and may start a little above the floor or stop a
    /// little below the top, as the hand-made layouts' clearings do); and no area of the map goes more than 2 outside the
    /// map's band nor asks less than the map's walk level.
    /// </para>
    /// <para>
    /// The in-between areas (<see cref="OverworldZone.MapId"/> null, only <see cref="OverworldZone.OwnerMapId"/>) are not
    /// compared: they are the level ramp between two maps (Docs/Mapas100.md §0.10, "nível subindo com a distância"), on
    /// purpose below or above the map they count for — the Eclusa da Lua (92–96, walk 90) at the foot of the Canal das
    /// Lanternas (43, 102–110, walk 98) keeps the east road to the Torres Brancas open, the Mata Morta (74–78, walk 72) is the
    /// ramp from Tarchon to the Coração do Lodo (35, 82–88, walk 78), and the main story steps there (levels 94 and 74) are
    /// built on those numbers.
    /// </para>
    /// </summary>
    public class MapZoneLevelConsistencyTests
    {
        /// <summary>Up to how many levels the band of the areas may differ from the band of the map, at each end.</summary>
        public const int BandTolerance = 2;

        private static IEnumerable<MapDefinition> MapsInTheWorld =>
            MapCatalog.Maps.Where(m => OverworldLayouts.ForMap(m.Id) != null);

        private static List<OverworldZone> AreasOf(MapDefinition map) => OverworldLayouts.ForMap(map.Id).ZonesOfMap(map.Id).ToList();

        private static string Label(MapDefinition map) =>
            $"mapa {map.MapId:00} ({map.Id}, faixa {map.MonsterLevelMin}–{map.MonsterLevelMax}, a pé {map.WalkLevelRequirement}, barca {map.WarpLevelRequirement})";

        private static string Label(OverworldZone zone) => $"área {zone.Id} (faixa {zone.MinLevel}–{zone.MaxLevel}, a pé {zone.WalkLevel})";

        [Test]
        public void EveryMap_IsInTheContinuousWorld_WithAtLeastOneAreaOfItsOwn()
        {
            var missing = new List<string>();
            foreach (MapDefinition map in MapCatalog.Maps)
            {
                OverworldLayout layout = OverworldLayouts.ForMap(map.Id);
                if (layout == null || !layout.ZonesOfMap(map.Id).Any())
                {
                    missing.Add(Label(map));
                }
            }

            Assert.That(missing, Is.Empty, "mapas sem área própria no mundo contínuo: " + string.Join("; ", missing));
        }

        [Test]
        public void TheWalkLevelOfTheMap_IsTheLowestWalkLevelOfItsAreas()
        {
            var wrong = new List<string>();
            foreach (MapDefinition map in MapsInTheWorld)
            {
                List<OverworldZone> areas = AreasOf(map);
                OverworldZone entrance = areas.OrderBy(z => z.WalkLevel).First();
                if (entrance.WalkLevel != map.WalkLevelRequirement)
                {
                    wrong.Add($"{Label(map)}: a entrada a pé mais baixa das áreas é {entrance.WalkLevel} ({entrance.Id}), o catálogo pede {map.WalkLevelRequirement}");
                }
            }

            Assert.That(wrong, Is.Empty, "nível a pé do catálogo diferente do das áreas:\n" + string.Join("\n", wrong));
        }

        [Test]
        public void TheBandOfTheMap_IsTheBandOfItsAreas_WithinTheTolerance()
        {
            var wrong = new List<string>();
            foreach (MapDefinition map in MapsInTheWorld)
            {
                List<OverworldZone> areas = AreasOf(map);
                int min = areas.Min(z => z.MinLevel);
                int max = areas.Max(z => z.MaxLevel);
                if (System.Math.Abs(min - map.MonsterLevelMin) > BandTolerance || System.Math.Abs(max - map.MonsterLevelMax) > BandTolerance)
                {
                    wrong.Add($"{Label(map)}: as áreas ({string.Join(", ", areas.Select(z => z.Id))}) vão de {min} a {max}, " +
                              $"diferença {min - map.MonsterLevelMin:+0;-0;0} no piso e {max - map.MonsterLevelMax:+0;-0;0} no topo (tolerância ±{BandTolerance})");
                }
            }

            Assert.That(wrong, Is.Empty, "faixa de monstros do catálogo diferente da das áreas:\n" + string.Join("\n", wrong));
        }

        [Test]
        public void NoAreaOfTheMap_LeavesItsBand_OrOpensBelowItsWalkLevel()
        {
            var wrong = new List<string>();
            foreach (MapDefinition map in MapsInTheWorld)
            {
                foreach (OverworldZone zone in AreasOf(map))
                {
                    Assert.LessOrEqual(zone.MinLevel, zone.MaxLevel, $"{Label(zone)} do {Label(map)}");
                    if (zone.MinLevel < map.MonsterLevelMin - BandTolerance || zone.MaxLevel > map.MonsterLevelMax + BandTolerance)
                    {
                        wrong.Add($"{Label(map)}: {Label(zone)} sai da faixa do mapa (tolerância ±{BandTolerance})");
                    }

                    if (zone.WalkLevel < map.WalkLevelRequirement)
                    {
                        wrong.Add($"{Label(map)}: {Label(zone)} abre a pé antes do nível {map.WalkLevelRequirement} do mapa");
                    }

                    if (zone.WalkLevel > zone.MinLevel)
                    {
                        wrong.Add($"{Label(map)}: {Label(zone)} pede a pé mais que o bicho mais fraco dela");
                    }
                }
            }

            Assert.That(wrong, Is.Empty, "áreas fora do que o catálogo diz do mapa:\n" + string.Join("\n", wrong));
        }
    }
}
