using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.World.Map.Areas;
using Rapadura.Gameplay.World.Map.Overworld;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// The beasts on the roads and trails of the stitched world (OverworldLayouts.AddTrailSpawns): whoever walks the road meets them, never in a city
    /// or a safe place, never on a boss arena, and only the species the clearings of that same area already have.
    /// </summary>
    public class TrailSpawnsTests
    {
        private static OverworldLayout World => OverworldLayouts.StitchedWorld;

        private static List<MapSpawnZone> TrailSpawns => World.World.Zones.Where(s => s.AlongTrail).ToList();

        [Test]
        public void TheTrails_HaveBeasts_SpreadAlongThem()
        {
            List<MapSpawnZone> trail = TrailSpawns;
            Assert.Greater(trail.Count, 300, "groups stand along the roads of the whole world");
            foreach (MapSpawnZone a in trail)
            {
                foreach (MapSpawnZone b in trail)
                {
                    if (a != b)
                    {
                        Assert.GreaterOrEqual(Vector2.Distance(a.Center, b.Center), OverworldLayouts.TrailSpawnSpacing - 0.01f, a.Id + " and " + b.Id);
                    }
                }
            }
        }

        [Test]
        public void EveryGroup_StandsOnATrail_AndIsSmall()
        {
            foreach (MapSpawnZone spawn in TrailSpawns)
            {
                float nearest = World.World.Paths.Min(p => MapLayoutRules.DistanceToSegment(spawn.Center, p.A, p.B) - p.Width * 0.5f);
                Assert.LessOrEqual(nearest, 0.01f, spawn.Id + ": on the trail");
                Assert.LessOrEqual(spawn.MaxAlive, OverworldLayouts.TrailSpawnMaxAlive, spawn.Id);
                Assert.Greater(spawn.Day.Length + (spawn.Night?.Length ?? 0), 0, spawn.Id + ": something to spawn");
            }
        }

        [Test]
        public void NoGroup_InACityOrSafePlace_OnAnArenaOrAtAPass()
        {
            var arenas = RegionBossRules.DirectorSpawns.Select(b => b.LocalArena + b.WorldOffset).ToList();
            foreach (MapSpawnZone spawn in TrailSpawns)
            {
                Assert.GreaterOrEqual(OverworldRules.DistanceToSafety(World, spawn.Center), spawn.Radius + OverworldRules.SpawnSafeMargin, spawn.Id + ": out of the safe places");
                Assert.IsFalse(World.Settlements.Any(s => s.GroundRect.Contains(spawn.Center)), spawn.Id + ": not in a town");
                Assert.IsNull(OverworldRules.SettlementAt(World, spawn.Center), spawn.Id + ": not in a town");
                foreach (Vector2 arena in arenas)
                {
                    Assert.GreaterOrEqual(Vector2.Distance(spawn.Center, arena), RegionBossRules.ArenaClearRadius, spawn.Id + ": off the boss arena");
                }

                foreach (Vector2 pass in World.Passes)
                {
                    Assert.Greater(Vector2.Distance(spawn.Center, pass), 30f, spawn.Id + ": not right at a pass");
                }

                Assert.IsFalse(MapLayoutRules.InRiver(World.World, spawn.Center, 2f), spawn.Id + ": dry ground");
            }
        }

        [Test]
        public void EveryGroup_BringsOnlyTheSpeciesOfItsOwnArea()
        {
            var speciesOf = new Dictionary<OverworldZone, HashSet<string>>();
            foreach (MapSpawnZone clearing in World.World.Zones.Where(s => !s.AlongTrail))
            {
                OverworldZone zone = World.ZoneOfSpawn[clearing.Id];
                if (!speciesOf.TryGetValue(zone, out HashSet<string> set))
                {
                    speciesOf[zone] = set = new HashSet<string>();
                }

                set.UnionWith(clearing.AllEntries().Where(e => e != null).Select(e => e.EnemyId));
            }

            foreach (MapSpawnZone spawn in TrailSpawns)
            {
                OverworldZone zone = World.ZoneOfSpawn[spawn.Id];
                Assert.AreSame(zone, OverworldRules.ZoneAt(World, spawn.Center), spawn.Id);
                Assert.IsTrue(speciesOf.ContainsKey(zone), spawn.Id + ": its area has clearings");
                foreach (string enemy in spawn.AllEntries().Where(e => e != null).Select(e => e.EnemyId))
                {
                    Assert.IsTrue(speciesOf[zone].Contains(enemy), spawn.Id + ": " + enemy + " is not of " + zone.Id);
                }

                Assert.GreaterOrEqual(spawn.MinLevel, zone.MinLevel, spawn.Id);
                Assert.LessOrEqual(spawn.MaxLevel, zone.MaxLevel, spawn.Id);
            }
        }

        [Test]
        public void TheRoadOutOfTheVila_MeetsBeastsBeforeLong()
        {
            // From the edge of the Vila's safe place, the first 600 m of trail around it: a group within 120 m of the road at every step.
            Vector2 vila = World.Vila.Offset;
            List<MapSpawnZone> trail = TrailSpawns;
            int checkedPoints = 0;
            foreach (MapPath path in World.World.Paths)
            {
                for (float t = 0f; t <= 1f; t += 0.25f)
                {
                    Vector2 p = Vector2.Lerp(path.A, path.B, t);
                    float fromVila = Vector2.Distance(p, vila);
                    if (fromVila < 250f || fromVila > 600f || OverworldRules.DistanceToSafety(World, p) < 60f || World.Passes.Any(x => Vector2.Distance(x, p) < 120f))
                    {
                        continue;
                    }

                    checkedPoints++;
                    float nearest = World.World.Zones.Min(s => Vector2.Distance(s.Center, p));
                    Assert.Less(nearest, 120f, "a stretch of road with nothing on it at " + p);
                }
            }

            Assert.Greater(checkedPoints, 10, "the roads round the Vila were walked");
            Assert.Greater(trail.Count(s => Vector2.Distance(s.Center, vila) < 600f), 3, "beasts on the roads round the Vila");
        }

        [Test]
        public void TheRegionsOnTheirOwn_KeepOnlyTheirClearings()
        {
            foreach (OverworldPart part in World.Parts)
            {
                OverworldLayout region = OverworldLayouts.Get(part.Id);
                Assert.IsFalse(region.World.Zones.Any(s => s.AlongTrail), part.Id);
            }
        }
    }
}
