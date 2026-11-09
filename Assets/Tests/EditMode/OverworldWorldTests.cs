using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using Rapadura.Gameplay.World.Cities.Town;
using Rapadura.Gameplay.World.Map;
using Rapadura.Gameplay.World.Map.Areas;
using Rapadura.Gameplay.World.Map.Overworld;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Rapadura.Tests
{
    /// <summary>
    /// The stitched world (Docs/Mapas100.md §0.10, "Mundo costurado"): the Zarafshan, the Areias, the Serra de Kaggen, the
    /// Grutas do Limiar, the Costa das Monções, the Delta de Jhilpur, the Teto do Mundo, the Planície de Mingcheng and the Estepe
    /// dos Nartes as ONE map: the four first tile a 4800 × 4800 m square, the Costa sits east of the Zarafshan and the Delta east of
    /// the Areias (south of the Costa), the Teto do Mundo north of the Serra de Kaggen, the Planície de Mingcheng north of the
    /// Zarafshan and the Estepe dos Nartes north of the Costa (a whole 3 × 3 square), and the Picos de Farnkesh north of the steppe,
    /// so the world's bounds grow to 7200 × 9600 m, the Mar Gelado north of Mingcheng, west of the peaks, and the Arquipélago de
    /// Hinokura north of the Teto, west of the frozen sea, so the 3 × 4 rectangle of twelve regions has no hole left; the Rio das Doze
    /// Horas opens a fifth row north of the archipelago (bounds 7200 × 12000 m), the Margens da Mistura fill the cell east of the river
    /// (north of the Mar Gelado) and the Limiar Profundo the row's last cell, north of the Picos de Farnkesh: the world has no hole left.
    /// Every walkable point belongs to one region, the chunk grids line up, the gates between the regions became open passes
    /// (flat, joined by trails), the relief has no hole nor step on the borders, the Zarafshan is the same as on its own away
    /// from them, and the hero can walk on trails from the Vila to Qocho, Litema, the Lamparinas, Taa Pwani, Jhilpur, Sangkent,
    /// Mingcheng, the Acampamento de Soslan, the Vale dos Sátiros, Farnkesh, the Porto da Lua Branca, Hinokura, the Cais de Mesektet, Dur-Jarras and the Fortaleza da Última Lâmpada without a gate;
    /// the island rings, the Ossuário and the docks stay.
    /// </summary>
    public class OverworldWorldTests
    {
        private const int MaxTrianglesPerChunk = 12000;

        private static OverworldLayout World => OverworldLayouts.Get(OverworldLayouts.WorldId);

        private static readonly string[] RegionIds =
        {
            OverworldLayouts.ZarafshanId, OverworldLayouts.AreiasId, OverworldLayouts.KaggenId, OverworldLayouts.LimiarId, OverworldLayouts.CostaId,
            OverworldLayouts.DeltaId, OverworldLayouts.TetoId, OverworldLayouts.MingchengId, OverworldLayouts.NartesId, OverworldLayouts.FarnkeshRegionId,
            OverworldLayouts.MarGeladoRegionId, OverworldLayouts.HinokuraRegionId, OverworldLayouts.RioRegionId, OverworldLayouts.MisturaRegionId, OverworldLayouts.ProfundoRegionId,
        };

        /// <summary>
        /// The cells of the bounds no region covers: the four rows below are whole (3 × 4, no hole). The Rio das Doze Horas (Região 13) opens a
        /// fifth row north of the Arquipélago de Hinokura, the only neighbour its sheet asks for (the ash of the Devouring Fire runs down
        /// to the night river; the region is level 260, after the archipelago's 264–280). The Margens da Mistura (Região 14) fill the cell of
        /// that row east of the river, north of the Mar Gelado (the river ends at level ~300, the Margins start at 300), reached by two passes
        /// on their west edge. The Limiar Profundo (Região 15) fills the last cell, north of the Picos de Farnkesh (the Margins end at level 344–360, the Limiar Profundo
        /// starts at 340, reached by two passes on its west edge): the world is a rectangle of 3 × 5 regions with no hole left.
        /// </summary>
        private static readonly Rect[] Holes = { };

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            MapLayouts.ClearCache();
            OverworldLayouts.ClearCache();
        }

        // ------------------------------------------------------------------
        // The four regions in one world
        // ------------------------------------------------------------------

        [Test]
        public void World_IsValid_AndTilesTheFifteenRegions_EachPointInOneRegion()
        {
            OverworldLayout w = World;
            CollectionAssert.IsEmpty(OverworldRules.Validate(w), string.Join("\n", OverworldRules.Validate(w)));
            Assert.IsTrue(w.IsStitched);
            CollectionAssert.AreEquivalent(RegionIds, w.Parts.Select(p => p.Id));
            Assert.AreEqual(new Rect(-3400f, -3400f, 7200f, 12000f), w.Bounds, "3 × 3 regions of 2400 m, Hinokura, the Mar Gelado and the Picos de Farnkesh on a fourth row, the Rio das Doze Horas, the Margens da Mistura and the Limiar Profundo on a fifth");
            CollectionAssert.AreEquivalent(Holes, w.Holes, "no hole left");
            Assert.AreEqual(OverworldLayouts.CostaWorldOffset, w.Parts.Find(p => p.Id == OverworldLayouts.CostaId).Offset);
            Assert.AreEqual(new Rect(1400f, -1000f, 2400f, 2400f), w.Parts.Find(p => p.Id == OverworldLayouts.CostaId).Bounds, "the Costa east of the Zarafshan");
            Assert.AreEqual(OverworldLayouts.DeltaWorldOffset, w.Parts.Find(p => p.Id == OverworldLayouts.DeltaId).Offset);
            Assert.AreEqual(new Rect(1400f, -3400f, 2400f, 2400f), w.Parts.Find(p => p.Id == OverworldLayouts.DeltaId).Bounds, "the Delta east of the Areias, south of the Costa");
            Assert.AreEqual(OverworldLayouts.TetoWorldOffset, w.Parts.Find(p => p.Id == OverworldLayouts.TetoId).Offset);
            Assert.AreEqual(new Rect(-3400f, 1400f, 2400f, 2400f), w.Parts.Find(p => p.Id == OverworldLayouts.TetoId).Bounds, "the Teto do Mundo north of the Serra de Kaggen");
            Assert.AreEqual(new Vector2(-2200f, 2000f), w.Parts.Find(p => p.Id == OverworldLayouts.TetoId).Vila.Offset, "Sangkent in the world");
            Assert.AreEqual(OverworldLayouts.MingchengWorldOffset, w.Parts.Find(p => p.Id == OverworldLayouts.MingchengId).Offset);
            Assert.AreEqual(new Rect(-1000f, 1400f, 2400f, 2400f), w.Parts.Find(p => p.Id == OverworldLayouts.MingchengId).Bounds,
                "the Planície de Mingcheng north of the Zarafshan, east of the Teto do Mundo");
            Assert.AreEqual(new Vector2(200f, 2000f), w.Parts.Find(p => p.Id == OverworldLayouts.MingchengId).Vila.Offset, "Mingcheng in the world");
            Assert.AreEqual(OverworldLayouts.NartesWorldOffset, w.Parts.Find(p => p.Id == OverworldLayouts.NartesId).Offset);
            Assert.AreEqual(new Rect(1400f, 1400f, 2400f, 2400f), w.Parts.Find(p => p.Id == OverworldLayouts.NartesId).Bounds,
                "the Estepe dos Nartes north of the Costa, east of the Planície de Mingcheng (the hole that was left)");
            Assert.AreEqual(new Vector2(2600f, 2000f), w.Parts.Find(p => p.Id == OverworldLayouts.NartesId).Vila.Offset, "the Acampamento de Soslan in the world");
            Assert.AreEqual(OverworldLayouts.FarnkeshWorldOffset, w.Parts.Find(p => p.Id == OverworldLayouts.FarnkeshRegionId).Offset);
            Assert.AreEqual(new Rect(1400f, 3800f, 2400f, 2400f), w.Parts.Find(p => p.Id == OverworldLayouts.FarnkeshRegionId).Bounds,
                "the Picos de Farnkesh north of the Estepe dos Nartes");
            Assert.AreEqual(new Vector2(2600f, 4400f), w.Parts.Find(p => p.Id == OverworldLayouts.FarnkeshRegionId).Vila.Offset, "the Vale dos Sátiros in the world");
            Assert.AreEqual(OverworldLayouts.MarGeladoWorldOffset, w.Parts.Find(p => p.Id == OverworldLayouts.MarGeladoRegionId).Offset);
            Assert.AreEqual(new Rect(-1000f, 3800f, 2400f, 2400f), w.Parts.Find(p => p.Id == OverworldLayouts.MarGeladoRegionId).Bounds,
                "the Mar Gelado north of Mingcheng, west of the Picos de Farnkesh (the hole next to the peaks)");
            Assert.AreEqual(new Vector2(200f, 4400f), w.Parts.Find(p => p.Id == OverworldLayouts.MarGeladoRegionId).Vila.Offset, "the Porto da Lua Branca in the world");
            Assert.AreEqual(OverworldLayouts.HinokuraWorldOffset, w.Parts.Find(p => p.Id == OverworldLayouts.HinokuraRegionId).Offset);
            Assert.AreEqual(new Rect(-3400f, 3800f, 2400f, 2400f), w.Parts.Find(p => p.Id == OverworldLayouts.HinokuraRegionId).Bounds,
                "the Arquipélago de Hinokura north of the Teto do Mundo, west of the Mar Gelado (the last hole)");
            Assert.AreEqual(new Vector2(-1720f, 4400f), w.Parts.Find(p => p.Id == OverworldLayouts.HinokuraRegionId).Vila.Offset, "Hinokura in the world");
            Assert.AreEqual(OverworldLayouts.RioWorldOffset, w.Parts.Find(p => p.Id == OverworldLayouts.RioRegionId).Offset);
            Assert.AreEqual(new Rect(-3400f, 6200f, 2400f, 2400f), w.Parts.Find(p => p.Id == OverworldLayouts.RioRegionId).Bounds,
                "the Rio das Doze Horas north of the Arquipélago de Hinokura");
            Assert.AreEqual(new Vector2(-2200f, 6800f), w.Parts.Find(p => p.Id == OverworldLayouts.RioRegionId).Vila.Offset, "the Cais de Mesektet in the world");
            Assert.AreEqual(OverworldLayouts.MisturaWorldOffset, w.Parts.Find(p => p.Id == OverworldLayouts.MisturaRegionId).Offset);
            Assert.AreEqual(new Rect(-1000f, 6200f, 2400f, 2400f), w.Parts.Find(p => p.Id == OverworldLayouts.MisturaRegionId).Bounds,
                "the Margens da Mistura east of the Rio das Doze Horas, north of the Mar Gelado");
            Assert.AreEqual(new Vector2(-400f, 7400f), w.Parts.Find(p => p.Id == OverworldLayouts.MisturaRegionId).Vila.Offset, "Dur-Jarras in the world");
            Assert.AreEqual(OverworldLayouts.ProfundoWorldOffset, w.Parts.Find(p => p.Id == OverworldLayouts.ProfundoRegionId).Offset);
            Assert.AreEqual(new Rect(1400f, 6200f, 2400f, 2400f), w.Parts.Find(p => p.Id == OverworldLayouts.ProfundoRegionId).Bounds,
                "the Limiar Profundo east of the Margens da Mistura, north of the Picos de Farnkesh");
            Assert.AreEqual(new Vector2(2000f, 7400f), w.Parts.Find(p => p.Id == OverworldLayouts.ProfundoRegionId).Vila.Offset, "the Fortaleza in the world");
            Assert.AreEqual(new Vector2(2600f, 5480f), w.SettlementOf(OverworldLayouts.FarnkeshMapId).Offset, "Farnkesh in the world");
            Assert.AreEqual(Vector2.zero, w.Parts.Find(p => p.Id == OverworldLayouts.ZarafshanId).Offset, "the Vila stays at (0, 0)");
            Assert.AreEqual(Vector2.zero, w.Vila.Offset);

            float area = 0f;
            foreach (OverworldPart part in w.Parts)
            {
                area += part.Bounds.width * part.Bounds.height;
                OverworldLayout alone = OverworldLayouts.Get(part.Id);
                Assert.AreEqual(alone.Bounds.size, part.Bounds.size, part.Id);
                Assert.AreEqual(alone.Bounds.position + part.Offset, part.Bounds.position, part.Id);
                Assert.AreEqual(0f, Mathf.Repeat(part.Bounds.xMin - w.Bounds.xMin, w.ChunkSize), 1e-3f, $"{part.Id}: on the chunk grid");
                Assert.AreEqual(0f, Mathf.Repeat(part.Bounds.yMin - w.Bounds.yMin, w.ChunkSize), 1e-3f, $"{part.Id}: on the chunk grid");
                Assert.AreEqual(alone.Zones.Count, w.Zones.Count(z => part.Bounds.Contains(z.Bounds.center)), $"{part.Id}: every area came along");
                Assert.AreSame(part.Vila, w.VilaAt(part.Bounds.center));
                Assert.AreEqual(alone.Region, w.RegionAt(part.Bounds.center));
                Assert.AreEqual(alone.World.Biome, part.Biome);
            }

            Assert.AreEqual(w.Bounds.width * w.Bounds.height - Holes.Sum(h => h.width * h.height), area, 1f, "the regions cover the world but its holes");

            // Every point (borders included) belongs to exactly one region and one area of it, but in the holes (beyond the walls).
            for (float x = w.Bounds.xMin; x < w.Bounds.xMax; x += 25f)
            {
                for (float z = w.Bounds.yMin; z < w.Bounds.yMax; z += 25f)
                {
                    var p = new Vector2(x, z);
                    if (Holes.Any(h => h.Contains(p)))
                    {
                        Assert.AreEqual(0, w.Parts.Count(part => part.Bounds.Contains(p)), $"{p}: no region in a hole");
                        Assert.IsNull(OverworldRules.ZoneAt(w, p), $"{p}: no area in a hole");
                        Assert.LessOrEqual(w.WallDistance(p), 0f, $"{p}: a hole is beyond the walls");
                        continue;
                    }

                    Assert.AreEqual(1, w.Parts.Count(part => part.Bounds.Contains(p)), $"{p}: one region");
                    Assert.Greater(w.WallDistance(p), -0.01f, $"{p}: inside the walls");
                    OverworldZone zone = OverworldRules.ZoneAt(w, p);
                    Assert.IsNotNull(zone, $"{p}: one area");
                    Assert.IsTrue(w.PartAt(p).Bounds.Contains(zone.Bounds.center), $"{p}: the area belongs to its region");
                }
            }

            Assert.AreEqual(w.Zones.Count, w.Zones.Select(z => z.Id).Distinct().Count(), "area ids are unique in the world");
            Assert.AreEqual(w.World.Zones.Count, w.World.Zones.Select(z => z.Id).Distinct().Count(), "clearing ids are unique in the world");
            Assert.AreEqual(w.Settlements.Count, w.Settlements.Select(s => s.Id).Distinct().Count(), "town ids are unique in the world");
            Assert.AreEqual(w.World.Exits.Count, w.World.Exits.Select(e => e.Id).Distinct().Count(), "exit ids are unique in the world");
        }

        [Test]
        public void World_IsWhatMapModeLoads_ForEveryMapOfTheFifteenRegions()
        {
            OverworldLayout w = World;
            foreach (string id in RegionIds)
            {
                OverworldLayout alone = OverworldLayouts.Get(id);
                foreach (OverworldZone zone in alone.Zones)
                {
                    string map = zone.MapIdOrOwner;
                    Assert.AreSame(w, OverworldLayouts.WorldOf(map), $"{map}: map mode loads the world");
                    Assert.AreSame(alone, OverworldLayouts.ForMap(map), $"{map}: the region alone is still there (server, catalog)");
                    Assert.AreEqual(id, w.PartOfMap(map)?.Id, map);
                    Assert.IsTrue(w.ContainsMap(zone.MapId) || zone.MapId == null);
                }

                // Each town keeps its dock, the barca carries the hero there inside the same world.
                foreach (OverworldSettlement s in alone.Settlements.Where(s => s.HasDock))
                {
                    Assert.AreEqual(alone.ArrivalFor(s.MapId, null, out _) + w.Parts.Find(p => p.Id == id).Offset, w.ArrivalFor(s.MapId, null, out _), $"{s.Id}: barca stop");
                }
            }

            Assert.AreSame(w, OverworldLayouts.WorldOf(OverworldLayouts.TaaPwaniMapId), "region 5 is part of the world");
            Assert.AreSame(w, OverworldLayouts.WorldOf(OverworldLayouts.JhilpurMapId), "region 6 is part of the world");
            Assert.AreSame(w, OverworldLayouts.WorldOf(OverworldLayouts.SangkentMapId), "region 7 is part of the world");
            Assert.AreSame(w, OverworldLayouts.WorldOf(OverworldLayouts.WakhanId), "region 7 is part of the world");
            Assert.AreSame(w, OverworldLayouts.WorldOf(OverworldLayouts.MingchengMapId), "region 8 is part of the world");
            Assert.AreSame(w, OverworldLayouts.WorldOf(OverworldLayouts.TorresBrancasMapId), "region 8 is part of the world");
            Assert.AreSame(w, OverworldLayouts.WorldOf(OverworldLayouts.SoslanMapId), "region 9 is part of the world");
            Assert.AreSame(w, OverworldLayouts.WorldOf(OverworldLayouts.AmiraniMapId), "region 9 is part of the world");
            Assert.AreSame(w, OverworldLayouts.WorldOf(OverworldLayouts.ValeSatirosMapId), "region 10 is part of the world");
            Assert.AreSame(w, OverworldLayouts.WorldOf(OverworldLayouts.NinhoHumaMapId), "region 10 is part of the world");
            Assert.AreSame(w, OverworldLayouts.WorldOf(OverworldLayouts.PortoLuaBrancaMapId), "region 11 is part of the world");
            Assert.AreSame(w, OverworldLayouts.WorldOf(OverworldLayouts.FundoVelhoKulMapId), "region 11 is part of the world");
            Assert.AreSame(w, OverworldLayouts.WorldOf(OverworldLayouts.HinokuraMapId), "region 12 is part of the world");
            Assert.AreSame(w, OverworldLayouts.WorldOf(OverworldLayouts.CaldeiraMapId), "region 12 is part of the world");
            Assert.AreSame(w, OverworldLayouts.WorldOf(OverworldLayouts.CaisMesektetMapId), "region 13 is part of the world");
            Assert.AreSame(w, OverworldLayouts.WorldOf(OverworldLayouts.BancoApepMapId), "region 13 is part of the world");
            Assert.AreSame(w, OverworldLayouts.WorldOf(OverworldLayouts.DurJarrasMapId), "region 14 is part of the world");
            Assert.AreSame(w, OverworldLayouts.WorldOf(OverworldLayouts.TabuasSonhoMapId), "region 14 is part of the world");
            Assert.AreSame(w, OverworldLayouts.WorldOf(OverworldLayouts.FortalezaUltimaLampadaMapId), "region 15 is part of the world");
            Assert.AreSame(w, OverworldLayouts.WorldOf(OverworldLayouts.PortaoTrevaMapId), "region 15 is part of the world");
            Assert.IsNull(OverworldLayouts.WorldOf("map_043_canal_lanternas"), "not a catalog map");
        }

        [Test]
        public void Gates_BetweenTheRegions_AreOpenPasses_TheRingsTheOssuaryAndTheDocksStay()
        {
            OverworldLayout w = World;
            Assert.IsFalse(w.World.Exits.Exists(e => e.Kind == MapExitKind.Map && w.ContainsMap(e.TargetMapId)), "no gate inside the world");
            Assert.AreEqual(15, w.World.Exits.Count(e => e.Kind == MapExitKind.WorldMap && e.Id.StartsWith("exit_island")), "one stone ring per region");
            Assert.IsNotNull(w.World.Exits.Find(e => e.Id == "exit_island_margens_da_mistura"), "the stone ring by Dur-Jarras");
            Assert.IsNotNull(w.World.Exits.Find(e => e.Id == "exit_island_limiar_profundo"), "the stone ring by the Fortaleza");
            Assert.IsNotNull(w.World.Exits.Find(e => e.Id == "exit_island_rio_doze_horas"), "the stone ring by the Cais de Mesektet");
            Assert.IsNotNull(w.World.Exits.Find(e => e.Id == "exit_island_hinokura"), "the stone ring by Hinokura");
            Assert.IsNotNull(w.World.Exits.Find(e => e.Id == "exit_island_mar_gelado"), "the stone ring by the Porto da Lua Branca");
            Assert.IsNotNull(w.World.Exits.Find(e => e.Id == "exit_island_farnkesh"), "the stone ring by the Vale dos Sátiros");
            Assert.IsNotNull(w.World.Exits.Find(e => e.Id == "exit_island_nartes"), "the stone ring by the Acampamento de Soslan");
            Assert.IsNotNull(w.World.Exits.Find(e => e.Id == "exit_island_teto"), "the stone ring by Sangkent");
            Assert.IsNotNull(w.World.Exits.Find(e => e.Id == "exit_island_mingcheng"), "the stone ring by Mingcheng");
            Assert.IsNotNull(w.World.Exits.Find(e => e.Id == "exit_island_costa"), "the stone ring by Taa Pwani");
            Assert.IsNotNull(w.World.Exits.Find(e => e.Id == "exit_island_delta"), "the stone ring by Jhilpur");
            Assert.IsNotNull(w.World.Exits.Find(e => e.Id == MapModeController.OssuaryExitId), "the Ossuário's portal keeps its id");
            Assert.AreEqual(w.Settlements.Count(s => s.HasDock), w.World.Exits.Count(e => e.Kind == MapExitKind.Dock));

            // The passes: Zarafshan–Areias (south road), Zarafshan–Kaggen (twice), Kaggen–Limiar, Limiar–Areias, Zarafshan–Costa,
            // Areias–Delta (the old gate east of the Areias meets the Delta's west gate), Costa–Delta (twice: the red cliffs' road
            // and Liyongo's mangroves), Kaggen–Teto (twice: the drystone walls' road and the painted shelters' trail, each led from
            // the Teto's gate to the nearest trail of the Serra), Zarafshan–Mingcheng (twice: the spirit way and the loess track, each
            // led to the nearest trail of the Zarafshan's ruins and high wood) and Teto–Mingcheng (twice: by the chute station and
            // below the wind wheels, led to the nearest trail of the Teto), Costa–Nartes (three times: Aratta's savanna, Meroë's
            // court and Husuni's cliffs, each led from the steppe's gate to the nearest trail of the Costa) and Mingcheng–Nartes
            // (three times: the loess track, the Torres Brancas' mirror hills and the nian's peaks, led to Mingcheng's trails), and
            // Nartes–Farnkesh (twice: by the meltwater lake and by Ohya's crest, each led from the peaks' gate to the steppe's nearest trail),
            // Mingcheng–Mar Gelado (once: the pass of the wall, led from the frozen sea's gate to the end of Mingcheng's north road) and
            // Farnkesh–Mar Gelado (twice: the foot woods and the pinnacle wood, led from the frozen sea's gates to the peaks' west trails),
            // Teto–Hinokura (twice: Wakhan's pass and the crests of the ill wind, led from the archipelago's gates to the ends of the Teto's
            // northern trails), Mar Gelado–Hinokura (twice: the tundra and Omöl's lakes, led over the thaw strait to the frozen sea's
            // west trails) and Hinokura–Rio das Doze Horas (twice: the ash fields and the sea of ash, led from the river's gates over the
            // ash to Hinokura's northern road) and Rio das Doze Horas–Margens da Mistura (twice: the two passes on the Margins' west edge, led to the
            // river's east flank, the Beira Fervente) and Margens da Mistura–Limiar Profundo (twice: the two passes on the Limiar Profundo's west edge, led to the Margins' east trails).
            Assert.GreaterOrEqual(w.Passes.Count, 37);
            OverworldRelief relief = OverworldRelief.For(w);
            foreach (Vector2 pass in w.Passes)
            {
                Assert.AreEqual(0f, relief.SurfaceAt(pass), 1e-3f, $"pass {pass}: flat ground");
                Assert.IsTrue(MapLayoutRules.OnPath(w.World, pass, 3f), $"pass {pass}: a trail runs through it");
            }

            foreach ((Vector2 a, Vector2 b) in new[]
            {
                (new Vector2(0f, -900f), new Vector2(-100f, -1100f)), (new Vector2(-900f, -150f), new Vector2(-1100f, -200f)),
                (new Vector2(-1700f, -900f), new Vector2(-1700f, -1100f)), (new Vector2(-1100f, -1110f), new Vector2(-900f, -1110f)),
                (new Vector2(1300f, -230f), new Vector2(1500f, -230f)), (new Vector2(1300f, -1250f), new Vector2(1500f, -1250f)),
                (new Vector2(1940f, -900f), new Vector2(1940f, -1100f)), (new Vector2(3540f, -900f), new Vector2(3540f, -1100f)),
                (new Vector2(-2350f, 1500f), new Vector2(-2350f, 1300f)), (new Vector2(-1080f, 1500f), new Vector2(-1080f, 1300f)),
                (new Vector2(0f, 1500f), new Vector2(0f, 1300f)), (new Vector2(1000f, 1500f), new Vector2(1000f, 1300f)),
                (new Vector2(-900f, 2100f), new Vector2(-1100f, 2100f)), (new Vector2(-900f, 2620f), new Vector2(-1100f, 2620f)),
                (new Vector2(1700f, 1500f), new Vector2(1700f, 1300f)), (new Vector2(2750f, 1500f), new Vector2(2750f, 1300f)),
                (new Vector2(3500f, 1500f), new Vector2(3500f, 1300f)), (new Vector2(1500f, 1850f), new Vector2(1300f, 1850f)),
                (new Vector2(1500f, 2700f), new Vector2(1300f, 2700f)), (new Vector2(1500f, 3420f), new Vector2(1300f, 3420f)),
                (new Vector2(2300f, 3900f), new Vector2(2300f, 3700f)), (new Vector2(3560f, 3900f), new Vector2(3560f, 3700f)),
                (new Vector2(240f, 3900f), new Vector2(240f, 3700f)), (new Vector2(1300f, 3980f), new Vector2(1500f, 3980f)),
                (new Vector2(1300f, 4960f), new Vector2(1500f, 4960f)),
                (new Vector2(-1640f, 3900f), new Vector2(-1640f, 3700f)), (new Vector2(-1200f, 3900f), new Vector2(-1200f, 3700f)),
                (new Vector2(-1100f, 4700f), new Vector2(-900f, 4700f)), (new Vector2(-1100f, 5000f), new Vector2(-900f, 5000f)),
                (new Vector2(-2320f, 6300f), new Vector2(-2320f, 6100f)), (new Vector2(-1720f, 6300f), new Vector2(-1720f, 6100f)),
                (new Vector2(-900f, 7300f), new Vector2(-1100f, 7300f)), (new Vector2(-900f, 7560f), new Vector2(-1100f, 7560f)),
                (new Vector2(1500f, 7300f), new Vector2(1300f, 7300f)), (new Vector2(1500f, 7560f), new Vector2(1300f, 7560f)),
            })
            {
                Assert.AreNotEqual(w.PartAt(a), w.PartAt(b), $"{a} and {b} are in two regions");
                Assert.Less(Vector2.Distance(Nearest(w.Passes, a), a), 220f, $"a pass near {a}");
            }
        }

        // ------------------------------------------------------------------
        // Relief across the borders
        // ------------------------------------------------------------------

        [Test]
        public void Relief_HasNoStepNorHole_OnTheBordersBetweenRegions()
        {
            OverworldLayout w = World;
            OverworldRelief relief = OverworldRelief.For(w);
            const float e = 0.02f;
            float worst = 0f;
            // The inner borders: x = -1000 and z = -1000 (the whole width of the world, Costa | Delta included), x = 1400
            // (Zarafshan | Costa, Areias | Delta, Mingcheng | Nartes), z = 1400 (Kaggen | Teto, Zarafshan | Mingcheng, Costa | Nartes) and,
            // on the fourth row, z = 3800 under the Picos de Farnkesh and the Mar Gelado (Nartes | Farnkesh, Mingcheng | Mar Gelado; west of
            // them, before Região 12, the hole's wall; now the whole width, Teto | Hinokura too), x = 1400 between them (Mar Gelado |
            // Farnkesh), x = -1000 on the fourth and fifth rows (Hinokura | Mar Gelado, Rio | Margens), x = 1400 on the fifth too (Margens | Limiar Profundo) and z = 6200 under the fifth row (Hinokura | Rio,
            // Mar Gelado | Margens, Farnkesh | Limiar Profundo).
            for (float t = w.Bounds.xMin + 0.5f; t < w.Bounds.xMax; t += 1.5f)
            {
                var lines = new List<(Vector2, Vector2)> { (new Vector2(t, -1000f), Vector2.up), (new Vector2(t, 1400f), Vector2.up) };
                lines.Add((new Vector2(t, 3800f), Vector2.up));
                lines.Add((new Vector2(-1000f, t), Vector2.right));
                if (t + 4800f < w.Bounds.yMax)
                {
                    lines.Add((new Vector2(-1000f, t + 4800f), Vector2.right));
                }

                lines.Add((new Vector2(1400f, t), Vector2.right));
                if (t + 4800f < w.Bounds.yMax)
                {
                    lines.Add((new Vector2(1400f, t + 4800f), Vector2.right));
                }

                if (t < w.Bounds.xMax)
                {
                    lines.Add((new Vector2(t, 6200f), Vector2.up));
                }

                foreach ((Vector2 p, Vector2 n) in lines)
                {
                    float a = relief.SurfaceAt(p - n * e);
                    float b = relief.SurfaceAt(p + n * e);
                    worst = Mathf.Max(worst, Mathf.Abs(a - b));
                    Assert.Less(Mathf.Abs(a - b), 0.1f, $"a step on the border at {p}");
                    Assert.IsFalse(float.IsNaN(a) || float.IsNaN(b), $"{p}");
                    Assert.Greater(a, -2.5f, $"a hole on the border at {p}");
                }
            }

            // The chunks either side of a border share their vertices.
            foreach (Vector2 pass in w.Passes)
            {
                Vector2Int c = OverworldRules.ChunkOf(w, pass);
                for (int dx = -2; dx <= 1; dx++)
                {
                    for (int dz = -2; dz <= 1; dz++)
                    {
                        Vector2Int k = c + new Vector2Int(dx, dz);
                        OverworldGroundGrid a = OverworldChunkBuilder.BuildGrid(w, k);
                        OverworldGroundGrid east = OverworldChunkBuilder.BuildGrid(w, k + Vector2Int.right);
                        OverworldGroundGrid north = OverworldChunkBuilder.BuildGrid(w, k + Vector2Int.up);
                        Rect r = OverworldRules.ChunkRect(w, k);
                        for (float s = 0f; s <= w.ChunkSize; s += 0.53f)
                        {
                            var onEast = new Vector2(r.xMax, r.yMin + s);
                            var onNorth = new Vector2(r.xMin + s, r.yMax);
                            Assert.AreEqual(a.Sample(onEast), east.Sample(onEast), 1e-4f, $"seam {k}/east at {onEast}");
                            Assert.AreEqual(a.Sample(onNorth), north.Sample(onNorth), 1e-4f, $"seam {k}/north at {onNorth}");
                        }
                    }
                }
            }

            Debug.Log($"[Overworld World] largest height change across 4 cm of a region border: {worst:0.000} m");
        }

        [Test]
        public void TheZarafshan_IsTheSameAsOnItsOwn_AwayFromTheBorders()
        {
            OverworldLayout w = World;
            OverworldLayout alone = OverworldLayouts.Get(OverworldLayouts.ZarafshanId);
            OverworldRelief stitched = OverworldRelief.For(w);
            OverworldRelief own = OverworldRelief.For(alone);
            var rnd = new System.Random(7);
            int n = 0;
            for (int i = 0; i < 4000 && n < 1500; i++)
            {
                var p = new Vector2(-1000f + (float)rnd.NextDouble() * 2400f, -1000f + (float)rnd.NextDouble() * 2400f);
                if (p.x < -650f || p.y < -650f || p.x > 1050f || p.y > 900f)
                {
                    continue; // the passes' trails and the blend with the neighbours (the Costa east of it, Mingcheng north of it) reach this far
                }

                n++;
                Assert.AreEqual(own.HeightAt(p), stitched.HeightAt(p), 1e-3f, $"{p}");
                Assert.AreEqual(OverworldRules.ZoneAt(alone, p).Id, OverworldRules.ZoneAt(w, p).Id);
            }

            Assert.Greater(n, 1000);
        }

        [Test]
        public void Settlements_AndPasses_StandAtYZero_InTheWorld()
        {
            OverworldLayout w = World;
            foreach (OverworldSettlement settlement in w.Settlements)
            {
                Rect r = settlement.GroundRect;
                for (float x = r.xMin; x <= r.xMax; x += 4f)
                {
                    for (float z = r.yMin; z <= r.yMax; z += 4f)
                    {
                        Assert.AreEqual(0f, OverworldRules.SurfaceAt(w, new Vector2(x, z)), 1e-4f, $"{settlement.Id} ({x}, {z})");
                    }
                }

                // The town walks the whole world: its invisible walls stand on the world's edge, not on its region's.
                Assert.AreEqual(w.Bounds.size, settlement.Town.WalkBounds.size, settlement.Id);
                Assert.AreEqual(w.Bounds.position - settlement.Offset, settlement.Town.WalkBounds.position, settlement.Id);
            }

            foreach (MapExitSpec exit in w.World.Exits)
            {
                Assert.AreEqual(0f, OverworldRules.SurfaceAt(w, exit.Arrival), 1e-4f, $"{exit.Id}: the hero arrives on flat ground");
            }
        }

        // ------------------------------------------------------------------
        // Walking from the Vila to the other regions' towns
        // ------------------------------------------------------------------

        [Test]
        public void Trails_LeadOnFoot_FromTheVila_ToEveryRegionsTown_AndToFarnkesh()
        {
            OverworldLayout w = World;
            OverworldRelief relief = OverworldRelief.For(w);
            List<MapPath> paths = w.World.Paths;
            int count = paths.Count;

            // Trail pieces touch (end to end, or an end on another piece: a fork); a town joins every trail that ends in it.
            var links = new List<int>[count];
            for (int i = 0; i < count; i++)
            {
                links[i] = new List<int>();
            }

            var buckets = new Dictionary<Vector2Int, List<int>>();
            for (int i = 0; i < count; i++)
            {
                foreach (Vector2Int c in Cells(paths[i]))
                {
                    if (!buckets.TryGetValue(c, out List<int> list))
                    {
                        buckets[c] = list = new List<int>();
                    }

                    list.Add(i);
                }
            }

            for (int i = 0; i < count; i++)
            {
                foreach (Vector2Int c in Cells(paths[i]))
                {
                    foreach (int j in buckets[c])
                    {
                        if (j != i && Touch(paths[i], paths[j]))
                        {
                            links[i].Add(j);
                        }
                    }
                }
            }

            var townOf = new Dictionary<int, OverworldSettlement>();
            for (int i = 0; i < count; i++)
            {
                foreach (OverworldSettlement s in w.Settlements)
                {
                    Rect town = CityGeometry.Inflate(s.GroundRect, 10f);
                    if (town.Contains(paths[i].A) || town.Contains(paths[i].B))
                    {
                        townOf[i] = s;
                    }
                }
            }

            foreach (IGrouping<OverworldSettlement, KeyValuePair<int, OverworldSettlement>> group in townOf.GroupBy(p => p.Value))
            {
                List<int> ends = group.Select(p => p.Key).ToList();
                foreach (int a in ends)
                {
                    links[a].AddRange(ends.Where(b => b != a));
                }
            }

            OverworldSettlement vila = w.Vila;
            var report = new List<string>();
            // Qocho, Litema, the Lamparinas, Taa Pwani, Jhilpur, Sangkent, Mingcheng, the Acampamento de Soslan, the Vale dos Sátiros
            // and, up the peaks, Farnkesh (the capital of Região 10, not its region's town), the Porto da Lua Branca, Hinokura, the Cais de Mesektet, Dur-Jarras and the Fortaleza da Última Lâmpada.
            var goals = new List<OverworldSettlement>();
            foreach (string regionId in new[] { OverworldLayouts.AreiasId, OverworldLayouts.KaggenId, OverworldLayouts.LimiarId, OverworldLayouts.CostaId, OverworldLayouts.DeltaId, OverworldLayouts.TetoId, OverworldLayouts.MingchengId,
                OverworldLayouts.NartesId, OverworldLayouts.FarnkeshRegionId, OverworldLayouts.MarGeladoRegionId,
                OverworldLayouts.HinokuraRegionId, OverworldLayouts.RioRegionId, OverworldLayouts.MisturaRegionId, OverworldLayouts.ProfundoRegionId })
            {
                goals.Add(w.Parts.Find(p => p.Id == regionId).Vila);
            }

            goals.Add(w.SettlementOf(OverworldLayouts.FarnkeshMapId));
            foreach (OverworldSettlement goal in goals)
            {
                List<int> route = Route(links, townOf, vila, goal);
                Assert.IsNotNull(route, $"no trail from the Vila to {goal.Id}");
                float length = 0f;
                float steepest = 0f;
                foreach (int i in route)
                {
                    MapPath path = paths[i];
                    float piece = Vector2.Distance(path.A, path.B);
                    length += piece;
                    for (float t = 0f; t < piece; t += 1.5f)
                    {
                        Vector2 a = Vector2.Lerp(path.A, path.B, t / piece);
                        Vector2 b = Vector2.Lerp(path.A, path.B, Mathf.Min(1f, (t + 1.5f) / piece));
                        float run = Vector2.Distance(a, b);
                        if (run < 0.2f)
                        {
                            continue;
                        }

                        float slope = Mathf.Atan(Mathf.Abs(relief.SurfaceAt(b) - relief.SurfaceAt(a)) / run) * Mathf.Rad2Deg;
                        steepest = Mathf.Max(steepest, slope);
                        Assert.Less(slope, 30f, $"to {goal.Id}: the trail at {a} is a wall ({slope:0}°)");
                        Assert.IsFalse(InRiverOffCrossing(w, a), $"to {goal.Id}: the trail wades a river at {a}");
                    }
                }

                report.Add($"Vila → {goal.Id}: {route.Count} trail pieces, {length:0} m, steepest {steepest:0}°");
                Assert.Less(length, 16000f, $"to {goal.Id}: a sensible road (the Fortaleza is the farthest: the road goes through all of Hinokura, the river's east flank and all of the Margins)");
            }

            Debug.Log("[Overworld World] " + string.Join("; ", report));
        }

        // ------------------------------------------------------------------
        // Streaming and chunks
        // ------------------------------------------------------------------

        [Test]
        public void Chunks_AcrossTheBorders_AreWithinTheBudget_AndTheStreamingLoadsOnlyAroundTheHero()
        {
            OverworldLayout w = World;
            Vector2Int count = OverworldRules.ChunkCount(w);
            Assert.AreEqual(150, count.x);
            Assert.AreEqual(250, count.y, "the fifth row of the Rio das Doze Horas");
            var wanted = new List<Vector2Int>();
            foreach (Vector2 pass in w.Passes)
            {
                OverworldRules.ChunksWithin(w, pass, OverworldRules.LoadRadius, wanted);
                Assert.Less(wanted.Count, 40, "only the chunks around the hero, whatever the region");
                Assert.IsTrue(wanted.Exists(c => w.PartAt(OverworldRules.ChunkRect(w, c).center) != w.PartAt(pass)), $"{pass}: the chunks across the border stream in too");
            }

            var builder = new TownMeshBuilder();
            OverworldChunkBuilder.BuildGround(builder, w, OverworldChunkBuilder.Content(w, OverworldRules.ChunkOf(w, Vector2.zero)));
            var times = new List<double>();
            int worst = 0;
            foreach (Vector2 pass in w.Passes)
            {
                Vector2Int c0 = OverworldRules.ChunkOf(w, pass);
                for (int dx = -2; dx <= 2; dx++)
                {
                    for (int dz = -2; dz <= 2; dz++)
                    {
                        var c = c0 + new Vector2Int(dx, dz);
                        var watch = Stopwatch.StartNew();
                        OverworldChunkContent content = OverworldChunkBuilder.Content(w, c);
                        builder.Clear();
                        OverworldChunkBuilder.BuildGround(builder, w, content);
                        OverworldChunkBuilder.BuildProps(builder, content);
                        watch.Stop();
                        times.Add(watch.Elapsed.TotalMilliseconds);
                        worst = Mathf.Max(worst, builder.TriangleCount);
                        Assert.LessOrEqual(builder.TriangleCount, MaxTrianglesPerChunk, $"chunk {c}");
                    }
                }
            }

            Debug.Log($"[Overworld World] {times.Count} chunks at the passes: avg {times.Average():0.00} ms, worst {worst} triangles (Editor, Mono)");
            Assert.Less(times.Average(), 6.0);
        }

        // ------------------------------------------------------------------

        private static Vector2 Nearest(List<Vector2> points, Vector2 p) => points.OrderBy(q => Vector2.Distance(p, q)).First();

        private static IEnumerable<Vector2Int> Cells(MapPath path)
        {
            const float cell = 32f;
            int x0 = Mathf.FloorToInt((Mathf.Min(path.A.x, path.B.x) - 4f) / cell);
            int x1 = Mathf.FloorToInt((Mathf.Max(path.A.x, path.B.x) + 4f) / cell);
            int z0 = Mathf.FloorToInt((Mathf.Min(path.A.y, path.B.y) - 4f) / cell);
            int z1 = Mathf.FloorToInt((Mathf.Max(path.A.y, path.B.y) + 4f) / cell);
            for (int x = x0; x <= x1; x++)
            {
                for (int z = z0; z <= z1; z++)
                {
                    yield return new Vector2Int(x, z);
                }
            }
        }

        private static bool Touch(MapPath a, MapPath b)
        {
            float reach = Mathf.Max(a.Width, b.Width) * 0.5f + 0.75f;
            return MapLayoutRules.DistanceToSegment(a.A, b.A, b.B) <= reach || MapLayoutRules.DistanceToSegment(a.B, b.A, b.B) <= reach
                || MapLayoutRules.DistanceToSegment(b.A, a.A, a.B) <= reach || MapLayoutRules.DistanceToSegment(b.B, a.A, a.B) <= reach;
        }

        private static List<int> Route(List<int>[] links, Dictionary<int, OverworldSettlement> townOf, OverworldSettlement from, OverworldSettlement to)
        {
            var previous = new Dictionary<int, int>();
            var queue = new Queue<int>();
            foreach (KeyValuePair<int, OverworldSettlement> pair in townOf)
            {
                if (pair.Value == from)
                {
                    previous[pair.Key] = -1;
                    queue.Enqueue(pair.Key);
                }
            }

            while (queue.Count > 0)
            {
                int i = queue.Dequeue();
                if (townOf.TryGetValue(i, out OverworldSettlement town) && town == to)
                {
                    var route = new List<int>();
                    for (int k = i; k >= 0; k = previous[k])
                    {
                        route.Add(k);
                    }

                    route.Reverse();
                    return route;
                }

                foreach (int j in links[i])
                {
                    if (!previous.ContainsKey(j))
                    {
                        previous[j] = i;
                        queue.Enqueue(j);
                    }
                }
            }

            return null;
        }

        private static bool InRiverOffCrossing(OverworldLayout w, Vector2 p)
        {
            foreach (MapRiver river in w.World.Rivers)
            {
                if (MapLayoutRules.DistanceToPolyline(p, river.Points) >= river.Width * 0.5f - 0.5f)
                {
                    continue;
                }

                bool crossing = river.Crossings.Exists(c => Vector2.Distance(c.At, p) < Mathf.Max(c.Width, river.Width) * 0.5f + OverworldChunkBuilder.BridgeSpan(river));
                if (!crossing)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
