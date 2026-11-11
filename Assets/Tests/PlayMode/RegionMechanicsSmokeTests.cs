using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.Player.States;
using Rapadura.Gameplay.World.Cities.Town;
using Rapadura.Gameplay.World.Map;
using Rapadura.Gameplay.World.Map.Areas;
using Rapadura.Gameplay.World.Map.Mechanics;
using Rapadura.Gameplay.World.Map.Overworld;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Rapadura.Tests.PlayMode
{
    /// <summary>
    /// The mechanics of the 16 regions of the stitched world on the real scenes, which the walk tests never reach (they stay in the
    /// first regions): "Mover" to the Pomar loads Main.unity in map mode with the whole world, then the hero is stood (the game's own
    /// teleport: WarmUp + ResetTracking + PlaceAt) at the Cais da Barca of every region's town and the test checks that
    /// <see cref="MechanicsDriverHub"/> runs the drivers near the hero and only those, that the hero stands on the ground with the
    /// CharacterController on, and that nothing logs an error (the test runner fails on any unexpected error or exception; the Debug.Log with the
    /// region name before each one shows where it happened). Then a few metres of click-to-move where the world shoves the
    /// hero (Kaggen's winds, the Delta's wandering islands, Farnkesh's gorge gusts), a world boss woken and put back to sleep by
    /// <see cref="RegionBossDirector"/> (Hedammu), and the touch "Usar" flag (<see cref="InteractPromptRegistry"/>) by a Pomar tree.
    /// The hero is made invulnerable: a level-1 hero stands among level-400 monsters here, and a death would carry them away.
    /// </summary>
    [Category("Smoke")]
    public class RegionMechanicsSmokeTests : WorldMapFlowTestBase
    {
        private const string LogTag = "[RegionMechanicsSmoke]";
        private const float SettleSeconds = 1.5f;
        private const float StableSeconds = 0.5f;
        private const float WalkSeconds = 12f;
        private const float MaxFallSeconds = 1.2f;
        private const float BossWaitSeconds = 6f;

        /// <summary>The 16 regions in catalog order: the region's layout id and the drivers that must run there.</summary>
        private static readonly (int Number, string PartId, Type[] Drivers)[] Regions =
        {
            (1, OverworldLayouts.ZarafshanId, new[] { typeof(ZarafshanMechanicsDriver) }),
            (2, OverworldLayouts.AreiasId, new[] { typeof(AreiasMechanicsDriver) }),
            (3, OverworldLayouts.KaggenId, new[] { typeof(KaggenMechanicsDriver) }),
            (4, OverworldLayouts.LimiarId, new[] { typeof(LimiarMechanicsDriver) }),
            (5, OverworldLayouts.CostaId, new[] { typeof(CostaDeltaTetoMechanicsDriver) }),
            (6, OverworldLayouts.DeltaId, new[] { typeof(CostaDeltaTetoMechanicsDriver) }),
            (7, OverworldLayouts.TetoId, new[] { typeof(CostaDeltaTetoMechanicsDriver) }),
            (8, OverworldLayouts.MingchengId, new[] { typeof(MingchengMechanicsDriver) }),
            (9, OverworldLayouts.NartesId, new[] { typeof(NartesMechanicsDriver) }),
            (10, OverworldLayouts.FarnkeshRegionId, new[] { typeof(FarnkeshMechanicsDriver) }),
            (11, OverworldLayouts.MarGeladoRegionId, new[] { typeof(MarGeladoMechanicsDriver) }),
            (12, OverworldLayouts.HinokuraRegionId, new[] { typeof(HinokuraMechanicsDriver) }),
            (13, OverworldLayouts.RioRegionId, new[] { typeof(RioMechanicsDriver), typeof(RioRaftMechanicsDriver) }),
            (14, OverworldLayouts.MisturaRegionId, new[] { typeof(MisturaMechanicsDriver) }),
            (15, OverworldLayouts.ProfundoRegionId, new[] { typeof(ProfundoMechanicsDriver), typeof(ProfundoGateMechanicsDriver), typeof(ProfundoPathsMechanicsDriver) }),
            (16, OverworldLayouts.CeuRegionId, new[] { typeof(CeuMechanicsDriver) }),
        };

        private bool _bossSpawningBefore;

        // ------------------------------------------------------------------
        // 1. Every region: the hub, the ground, no error
        // ------------------------------------------------------------------

        [UnityTest]
        [Timeout(180000)]
        public IEnumerator EveryRegion_HubRunsOnlyTheNearDrivers_HeroStandsOnTheGround_NoErrors()
        {
            yield return Guarded(EveryRegion());
        }

        private IEnumerator EveryRegion()
        {
            yield return EnterWorld();
            OverworldLayout world = MapModeController.Current.Overworld;
            Assert.AreEqual(Regions.Length, world.Parts.Count, "The stitched world should hold the 16 regions.");
            GameObject player = FindPlayer();
            MechanicsDriverHub hub = MapModeController.Current.OverworldArea.GetComponent<MechanicsDriverHub>();
            Assert.IsNotNull(hub, "The overworld root has no MechanicsDriverHub.");
            Assert.GreaterOrEqual(hub.DriverCount, 15, "The hub should manage every region driver on the overworld root.");

            foreach ((int number, string partId, Type[] drivers) in Regions)
            {
                OverworldPart part = world.Parts.Find(p => p.Id == partId);
                Assert.IsNotNull(part, $"Região {number}: no part '{partId}' in the stitched world.");
                Vector2 spot = SafeSpot(part);
                Assert.AreEqual(partId, world.PartAt(spot)?.Id, $"Região {number}: its town's arrival {spot} lies outside the region.");

                Debug.Log($"{LogTag} Região {number} ({partId}) at {spot}");
                Place(player, spot, part.Vila.HasDock ? part.Vila.DockArrivalFacing : Vector2.down);
                yield return WaitRealtime(SettleSeconds);

                string where = $"Região {number} ({partId}) at {Ground(player)}";
                AssertStanding(player, where);
                AssertHub(player, drivers, where);
                Assert.AreEqual(partId, MapModeController.Current.OverworldArea.CurrentPart?.Id, $"{where}: the overworld did not track the hero into the region.");
            }
        }

        // ------------------------------------------------------------------
        // 2. A few metres where the world shoves the hero
        // ------------------------------------------------------------------

        [UnityTest]
        [Timeout(150000)]
        public IEnumerator PushRegions_ClickWalkAFewMetres_HeroNeverStuckFalling()
        {
            yield return Guarded(PushWalks());
        }

        private IEnumerator PushWalks()
        {
            yield return EnterWorld();
            OverworldLayout world = MapModeController.Current.Overworld;
            GameObject player = FindPlayer();
            PlayerClickToMove click = player.GetComponent<PlayerClickToMove>();
            Assert.IsNotNull(click, "The player has no click-to-move.");

            // Serra de Kaggen: a trail across the Encosta do Vento / Passo do Hahya, away from the rope bridges.
            Vector2 kaggen = AnchorOrigin(world, OverworldLayouts.KaggenAnchorZoneId);
            Rect[] kaggenWind = OverworldLayouts.KaggenWindAreas.Select(r => MechanicsDriverHub.WorldArea(r, kaggen)).ToArray();
            Vector2[] kaggenBridges = OverworldLayouts.KaggenBridges.Select(b => b + kaggen).ToArray();
            yield return WalkTrailOrTown(world, player, click, "Região 3 (Kaggen, winds)", OverworldLayouts.KaggenId, kaggenWind, kaggenBridges, null);

            // Delta de Jhilpur: on one of the wandering islands of the reed bog (they carry the hero, drifting or bobbing).
            Vector2 delta = AnchorOrigin(world, OverworldLayouts.DeltaAnchorZoneId);
            double now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / 1000d;
            Vector2 island = DeltaIslandRules.Position(OverworldLayouts.ReedIslandStations, 0, now) + delta;
            Vector2 inward = (OverworldLayouts.ReedBogCenter + delta - island).normalized;
            Debug.Log($"{LogTag} Região 6 (Delta, island 0) at {island}");
            Place(player, island, inward);
            yield return WaitRealtime(1f);
            yield return WalkAndWatch(player, click, island + inward * 6f, "Região 6 (Delta, wandering island)");

            // Picos de Farnkesh: a trail in the gorge of the Ays, where the gusts blow, away from the rope bridges.
            Vector2 farnkesh = AnchorOrigin(world, OverworldLayouts.FarnkeshAnchorZoneId);
            Rect[] gorge = OverworldLayouts.GustAreas.Select(r => MechanicsDriverHub.WorldArea(r, farnkesh)).ToArray();
            Vector2[][] ropes = OverworldLayouts.WindBridgeLines.Select(line => line.Select(p => p + farnkesh).ToArray()).ToArray();
            yield return WalkTrailOrTown(world, player, click, "Região 10 (Farnkesh, gorge gusts)", OverworldLayouts.FarnkeshRegionId, gorge, null, ropes);
        }

        /// <summary>Walks a trail piece inside <paramref name="areas"/>; when the layout has none there, a few metres out of the region's town instead (logged).</summary>
        private static IEnumerator WalkTrailOrTown(OverworldLayout world, GameObject player, PlayerClickToMove click, string what, string partId,
            Rect[] areas, Vector2[] avoidPoints, Vector2[][] avoidLines)
        {
            Vector2 from;
            Vector2 to;
            if (TryFindTrailPiece(world, areas, avoidPoints, avoidLines, out MapPath piece))
            {
                from = piece.A;
                to = piece.B;
                Debug.Log($"{LogTag} {what}: trail piece {from} -> {to}");
            }
            else
            {
                OverworldPart part = world.Parts.Find(p => p.Id == partId);
                Assert.IsNotNull(part, $"{what}: no part '{partId}'.");
                from = SafeSpot(part);
                Vector2 facing = part.Vila.HasDock ? part.Vila.DockArrivalFacing : Vector2.down;
                to = from + facing.normalized * 6f;
                Debug.Log($"{LogTag} {what}: no trail piece inside the mechanics area, walking out of the town instead {from} -> {to}");
            }

            Place(player, from, to - from);
            yield return WaitRealtime(1f);
            yield return WalkAndWatch(player, click, to, what);
        }

        /// <summary>
        /// Clicks <paramref name="target"/> and walks there in real time (timeScale 1, the pushes run on deltaTime), watching that the hero
        /// never stays in the fall state longer than <see cref="MaxFallSeconds"/>, ends on the ground and moved at all.
        /// </summary>
        private static IEnumerator WalkAndWatch(GameObject player, PlayerClickToMove click, Vector2 target, string what)
        {
            PlayerController controller = player.GetComponent<PlayerController>();
            Assert.IsNotNull(controller, $"{what}: the player has no PlayerController.");
            Vector2 start = Ground(player);
            click.SetDestination(new Vector3(target.x, player.transform.position.y, target.y));
            yield return null;

            float fallSince = -1f;
            float longestFall = 0f;
            float deadline = Time.realtimeSinceStartup + WalkSeconds;
            while (click.HasDestination && Time.realtimeSinceStartup < deadline)
            {
                bool falling = controller.StateMachine != null && controller.StateMachine.IsInState<PlayerFallState>();
                if (falling)
                {
                    if (fallSince < 0f)
                    {
                        fallSince = Time.realtimeSinceStartup;
                    }

                    longestFall = Mathf.Max(longestFall, Time.realtimeSinceStartup - fallSince);
                }
                else
                {
                    fallSince = -1f;
                }

                Assert.Less(longestFall, MaxFallSeconds, $"{what}: the hero has been in the fall state for {longestFall:0.0}s at {Ground(player)} (stuck falling?).");
                yield return null;
            }

            // Blocked (a river, a wall) or arrived: either way the walk is over; what matters is how the hero stands.
            click.ClearDestination();
            yield return WaitRealtime(0.6f);
            Assert.IsFalse(controller.StateMachine.IsInState<PlayerFallState>(), $"{what}: the hero is still in the fall state after the walk, at {Ground(player)}.");
            Assert.Greater(Vector2.Distance(start, Ground(player)), 1.5f, $"{what}: the hero never left {start} (clicked {target}).");
            AssertStanding(player, what);
        }

        // ------------------------------------------------------------------
        // 3. A world boss wakes and sleeps; 4. the touch "Usar" flag by a Pomar tree
        // ------------------------------------------------------------------

        [UnityTest]
        [Timeout(120000)]
        public IEnumerator PomarTree_RaisesTheUseFlag_AndHedammu_WakesNearAndSleepsFar()
        {
            _bossSpawningBefore = RegionBossDirector.SpawningEnabled;
            RegionBossDirector.SpawningEnabled = true;
            try
            {
                yield return Guarded(TreeAndBoss());
            }
            finally
            {
                RegionBossDirector.SpawningEnabled = _bossSpawningBefore;
            }
        }

        private IEnumerator TreeAndBoss()
        {
            yield return EnterWorld();
            OverworldLayout world = MapModeController.Current.Overworld;
            GameObject player = FindPlayer();

            // [E] by a ripe tree of the Pomar (a new game: every tree is ripe on day 1).
            Vector2 zarafshan = AnchorOrigin(world, OverworldLayouts.ZarafshanAnchorZoneId);
            ZarafshanHarvestSpot tree = OverworldLayouts.ZarafshanHarvestSpots.First(s => s.Id == "damasqueiro_leste");
            Vector2 byTree = tree.Position + zarafshan + new Vector2(0f, -2.5f);
            Assert.GreaterOrEqual(ZarafshanHarvestRules.SpotAt(byTree - zarafshan, OverworldLayouts.ZarafshanHarvestSpots), 0, "The test spot must be within the tree's reach.");
            InteractPromptRegistry.Clear();
            Place(player, byTree, Vector2.up);
            yield return WaitUntilOrFail(() => InteractPromptRegistry.IsAvailable(Time.unscaledTime),
                $"InteractPromptRegistry to report the [E] of the tree '{tree.Id}' with the hero at {Ground(player)}", 5f);

            // Hedammu (Região 5): wakes when the hero comes within ActiveRadius of her arena, leaves past SleepRadius.
            RegionBossSpawn spawn = RegionBossRules.SpawnOf(RegionBossRules.HedammuId);
            Assert.IsNotNull(spawn, "Hedammu is not a region boss any more.");
            Vector2? arena = RegionBossRules.ArenaIn(world, spawn);
            Assert.IsTrue(arena.HasValue, $"Hedammu's arena does not land in her area '{spawn.AreaId}' in the stitched world.");
            Assert.IsNull(FindBoss(spawn.EnemyId), "Hedammu stands in the world before the hero went near her.");

            // Inside the arena's cleared circle (no props), well within the wake radius.
            Vector2 near = arena.Value + new Vector2(RegionBossRules.ArenaClearRadius * 0.6f, 0f);
            Debug.Log($"{LogTag} Hedammu arena {arena.Value}, hero at {near}");
            Place(player, near, arena.Value - near);
            yield return WaitUntilOrFail(() => FindBoss(spawn.EnemyId) != null,
                $"RegionBossDirector to wake Hedammu with the hero {Vector2.Distance(near, arena.Value):0} m from her arena", BossWaitSeconds);
            GameObject boss = FindBoss(spawn.EnemyId);
            Assert.Less(Vector2.Distance(new Vector2(boss.transform.position.x, boss.transform.position.z), arena.Value), RegionBossRules.ArenaClearRadius,
                "Hedammu woke away from her arena.");

            // Back to the Costa's town, far past the sleep radius.
            OverworldPart costa = world.Parts.Find(p => p.Id == OverworldLayouts.CostaId);
            Assert.IsNotNull(costa);
            Vector2 far = SafeSpot(costa);
            Assert.Greater(Vector2.Distance(far, arena.Value), RegionBossRules.SleepRadius, "The far spot must be past Hedammu's sleep radius.");
            Place(player, far, Vector2.down);
            yield return WaitUntilOrFail(() => FindBoss(spawn.EnemyId) == null,
                $"RegionBossDirector to put Hedammu back to sleep with the hero {Vector2.Distance(far, arena.Value):0} m away", BossWaitSeconds);
            yield return WaitRealtime(0.5f);
            AssertStanding(player, "Back at the Costa's town after Hedammu");
        }

        /// <summary>The live world boss of <paramref name="enemyId"/> (RegionBossDirector names it "{prefab}_{definition}_world_boss"), or null.</summary>
        private static GameObject FindBoss(string enemyId)
        {
            foreach (MapMonsterInfo info in UnityEngine.Object.FindObjectsByType<MapMonsterInfo>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
            {
                string name = info.gameObject.name;
                if (name.EndsWith("_world_boss", StringComparison.Ordinal) && name.Contains(enemyId))
                {
                    return info.gameObject;
                }
            }

            return null;
        }

        // ------------------------------------------------------------------
        // Session
        // ------------------------------------------------------------------

        /// <summary>No dock, gate or shop keeper takes the hero away while the test stands them next to it.</summary>
        private static IEnumerator Guarded(IEnumerator body)
        {
            MapExitTrigger.Enabled = false;
            CityExitTrigger.Enabled = false;
            CityResidentPost.AutoInteract = false;
            try
            {
                yield return body;
            }
            finally
            {
                MapExitTrigger.Enabled = true;
                CityExitTrigger.Enabled = true;
                CityResidentPost.AutoInteract = true;
                RestoreTimeScale();
            }
        }

        /// <summary>A new game with Lavine, "Mover" to the Pomar: Main.unity in map mode with the stitched world; the hero made invulnerable.</summary>
        private static IEnumerator EnterWorld()
        {
            yield return LoadScene(WorldMapSceneName);
            yield return SelectHero(CharacterId.Lavine, requireCharacterSelect: false);
            WorldMapController map = null;
            yield return WaitForMap(m => map = m);

            SceneTransitionService transition = GetService<SceneTransitionService>();
            Assert.IsTrue(transition.TravelToMap(MapCatalog.PomarId), "TravelToMap refused the Pomar.");
            yield return WaitUntilOrFail(() => SceneManager.GetActiveScene().name == MainSceneName && MapModeController.Current != null && MapModeController.Current.IsReady,
                "Main.unity to load in map mode with the overworld built", SceneLoadTimeoutSeconds);

            MapModeController mode = MapModeController.Current;
            Assert.IsNotNull(mode.Overworld, "The Pomar must load the overworld.");
            Assert.IsTrue(mode.Overworld.IsStitched, "The Pomar must load the stitched world.");
            Assert.IsNotNull(mode.OverworldArea);
            GameObject player = FindPlayer();
            PlayerStats stats = player.GetComponent<PlayerStats>();
            Assert.IsNotNull(stats);
            stats.GrantInvulnerability(3600f);

            // Let the drivers' Start find their regions before the first check.
            yield return WaitFrames(3);
        }

        // ------------------------------------------------------------------
        // Helpers
        // ------------------------------------------------------------------

        /// <summary>Where the game itself stands a hero arriving in the region: its town's Cais da Barca, else the town's respawn.</summary>
        private static Vector2 SafeSpot(OverworldPart part)
        {
            OverworldSettlement vila = part.Vila;
            Assert.IsNotNull(vila, $"The region '{part.Id}' has no town.");
            return vila.HasDock ? vila.DockArrival : vila.Town.Respawn + vila.Offset;
        }

        private static Vector2 AnchorOrigin(OverworldLayout world, string anchorZoneId)
        {
            OverworldZone anchor = world.Zone(anchorZoneId);
            Assert.IsNotNull(anchor, $"The stitched world has no zone '{anchorZoneId}'.");
            return anchor.Origin;
        }

        /// <summary>The game's teleport (arrival, barca, respawn): chunks built round the spot, the barrier's memory reset, the hero stood on the ground.</summary>
        private static void Place(GameObject player, Vector2 at, Vector2 facing)
        {
            OverworldView view = MapModeController.Current.OverworldArea;
            view.WarmUp(at);
            view.ResetTracking(at);
            CityModeController.PlaceAt(player, at, facing);
            if (player.TryGetComponent(out PlayerStats stats))
            {
                stats.GrantInvulnerability(3600f);
            }
        }

        private static Vector2 Ground(GameObject player) => new Vector2(player.transform.position.x, player.transform.position.z);

        private static IEnumerator WaitRealtime(float seconds)
        {
            float end = Time.realtimeSinceStartup + seconds;
            while (Time.realtimeSinceStartup < end)
            {
                yield return null;
            }
        }

        /// <summary>
        /// The hero stands: CharacterController on, a collider (not their own) right under the feet, not sunk under the relief (fallen
        /// through onto the safety slab), not in the fall state, and still over <see cref="StableSeconds"/> (not sliding down through the world).
        /// </summary>
        private static void AssertStanding(GameObject player, string where)
        {
            CharacterController character = player.GetComponent<CharacterController>();
            Assert.IsNotNull(character, $"{where}: the player has no CharacterController.");
            Assert.IsTrue(character.enabled, $"{where}: the CharacterController is off (busy drivers: {BusyDrivers()}).");

            Vector3 p = player.transform.position;
            float surface = MapModeController.Current.OverworldArea.GroundHeight(new Vector2(p.x, p.z));
            Assert.Greater(p.y, surface - 2.5f, $"{where}: the hero is at y {p.y:0.00}, under the ground ({surface:0.00}): fell through the world?");
            Assert.Greater(p.y, -5.5f, $"{where}: the hero is at y {p.y:0.00}, on the safety slab under the world.");
            Assert.IsTrue(GroundUnder(player, out string hit), $"{where}: no collider under the hero's feet at {p} (surface {surface:0.00}).");

            PlayerController controller = player.GetComponent<PlayerController>();
            Assert.IsNotNull(controller);
            Assert.IsFalse(controller.StateMachine.IsInState<PlayerFallState>(), $"{where}: the hero is in the fall state at {p} (ground: {hit}).");
        }

        private static bool GroundUnder(GameObject player, out string hitName)
        {
            Vector3 from = player.transform.position + Vector3.up * 0.6f;
            RaycastHit[] hits = Physics.RaycastAll(from, Vector3.down, 2f, ~0, QueryTriggerInteraction.Ignore);
            foreach (RaycastHit hit in hits.OrderBy(h => h.distance))
            {
                if (!hit.collider.transform.IsChildOf(player.transform))
                {
                    hitName = hit.collider.name;
                    return true;
                }
            }

            hitName = "nothing";
            return false;
        }

        /// <summary>
        /// Checks the hub against the hero's position: each region driver that found its region runs exactly when the hero is within
        /// <see cref="MechanicsDriverHub.Margin"/> of its areas (or it is busy); <paramref name="expected"/> must be among the running ones.
        /// </summary>
        private static void AssertHub(GameObject player, Type[] expected, string where)
        {
            Vector2 hero = Ground(player);
            GameObject root = MapModeController.Current.OverworldArea.gameObject;
            var wrong = new StringBuilder();
            foreach (MonoBehaviour behaviour in root.GetComponents<MonoBehaviour>())
            {
                if (!(behaviour is IRegionMechanics region))
                {
                    continue;
                }

                bool mine = expected.Contains(behaviour.GetType());
                if (region.MechanicsAreas == null)
                {
                    if (mine)
                    {
                        wrong.Append($" {behaviour.GetType().Name} never found its region (switched itself off);");
                    }

                    continue;
                }

                bool shouldRun = MechanicsDriverHub.ShouldRun(region.MechanicsAreas, hero, MechanicsDriverHub.Margin, region.MechanicsBusy);
                if (mine && !shouldRun)
                {
                    wrong.Append($" {behaviour.GetType().Name}'s areas do not cover the hero;");
                }

                if (behaviour.enabled != shouldRun)
                {
                    wrong.Append($" {behaviour.GetType().Name} is {(behaviour.enabled ? "on" : "off")} but should be {(shouldRun ? "on" : "off")};");
                }
            }

            Assert.AreEqual(0, wrong.Length, $"{where}: MechanicsDriverHub:{wrong}");
        }

        private static string BusyDrivers()
        {
            MapModeController mode = MapModeController.Current;
            if (mode == null || mode.OverworldArea == null)
            {
                return "no overworld";
            }

            string busy = string.Join(", ", mode.OverworldArea.GetComponents<MonoBehaviour>()
                .Where(b => b is IRegionMechanics r && r.MechanicsBusy).Select(b => b.GetType().Name));
            return busy.Length > 0 ? busy : "none";
        }

        /// <summary>
        /// A piece of trail (the layout's walkable paths, cut in ≤ 24 m pieces) lying well inside one of <paramref name="areas"/>, in one
        /// zone (no soft barrier on the way), out of any town, clear of rivers and of the given bridge points/lines.
        /// </summary>
        private static bool TryFindTrailPiece(OverworldLayout world, Rect[] areas, Vector2[] avoidPoints, Vector2[][] avoidLines, out MapPath found)
        {
            const float inset = 15f;
            const float minLength = 8f;
            const float bridgeClearance = 30f;
            const float riverClearance = 8f;
            foreach (MapPath piece in world.World.Paths)
            {
                if (Vector2.Distance(piece.A, piece.B) < minLength)
                {
                    continue;
                }

                Vector2 mid = (piece.A + piece.B) * 0.5f;
                if (!areas.Any(a => Inset(a, inset).Contains(piece.A) && Inset(a, inset).Contains(piece.B)))
                {
                    continue;
                }

                OverworldZone zone = OverworldRules.ZoneAt(world, piece.A);
                if (zone == null || zone != OverworldRules.ZoneAt(world, piece.B) || zone != OverworldRules.ZoneAt(world, mid))
                {
                    continue;
                }

                if (OverworldRules.SettlementAt(world, piece.A) != null || OverworldRules.SettlementAt(world, piece.B) != null || OverworldRules.SettlementAt(world, mid) != null)
                {
                    continue;
                }

                if (avoidPoints != null && avoidPoints.Any(b => DistanceToSegment(b, piece.A, piece.B) < bridgeClearance))
                {
                    continue;
                }

                if (avoidLines != null && avoidLines.Any(line => DistanceToPolyline(line, piece.A) < bridgeClearance || DistanceToPolyline(line, piece.B) < bridgeClearance))
                {
                    continue;
                }

                bool wet = false;
                foreach (Vector2 p in new[] { piece.A, mid, piece.B })
                {
                    foreach ((Vector2 center, float length, float _, float width) in world.RiverPiecesIn(OverworldRules.ChunkOf(world, p)))
                    {
                        if (Vector2.Distance(center, p) < length * 0.5f + width * 0.5f + riverClearance)
                        {
                            wet = true;
                            break;
                        }
                    }

                    if (wet)
                    {
                        break;
                    }
                }

                if (wet)
                {
                    continue;
                }

                found = piece;
                return true;
            }

            found = default;
            return false;
        }

        private static Rect Inset(Rect r, float by) => Rect.MinMaxRect(r.xMin + by, r.yMin + by, r.xMax - by, r.yMax - by);

        private static float DistanceToSegment(Vector2 p, Vector2 a, Vector2 b)
        {
            Vector2 ab = b - a;
            float t = ab.sqrMagnitude > 1e-6f ? Mathf.Clamp01(Vector2.Dot(p - a, ab) / ab.sqrMagnitude) : 0f;
            return Vector2.Distance(p, a + ab * t);
        }

        private static float DistanceToPolyline(IReadOnlyList<Vector2> line, Vector2 p)
        {
            float best = float.MaxValue;
            for (int i = 0; i + 1 < line.Count; i++)
            {
                best = Mathf.Min(best, DistanceToSegment(p, line[i], line[i + 1]));
            }

            return best;
        }
    }
}
