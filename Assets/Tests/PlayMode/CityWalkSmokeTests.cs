using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.World.Cities;
using Rapadura.Gameplay.World.Cities.Town;
using Rapadura.Gameplay.World.Map;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Rapadura.Tests.PlayMode
{
    /// <summary>
    /// The walkable Vila on the real scenes (Docs/Cities.md §5): from the world map, "Andar pela cidade" loads Main.unity
    /// in city mode; the town is built, the arena is hidden, the hero is inside the safe zone; then the hero walks with
    /// real click-to-move (CharacterController against the town's colliders) from the plaza to every shop door, ends in
    /// the keeper's interact range each time, and finally leaves by the south gate back to the map.
    /// </summary>
    [Category("Smoke")]
    public class CityWalkSmokeTests : WorldMapFlowTestBase
    {
        private const float WalkTimeoutSeconds = 30f;
        private const float ArrivalTolerance = 0.75f;

        [UnityTest]
        [Timeout(300000)]
        public IEnumerator Vila_CityMode_WalksFromThePlazaToEveryShopDoor_AndLeavesByTheGate()
        {
            CityResidentPost.AutoInteract = false;
            CityExitTrigger.Enabled = false;
            try
            {
                yield return Walk();
            }
            finally
            {
                CityResidentPost.AutoInteract = true;
                CityExitTrigger.Enabled = true;
                RestoreTimeScale();
            }
        }

        private IEnumerator Walk()
        {
            yield return LoadScene(WorldMapSceneName);
            yield return SelectHero(CharacterId.Lavine, requireCharacterSelect: false);
            WorldMapController map = null;
            yield return WaitForMap(m => map = m);
            Assert.AreEqual(Rapadura.Gameplay.World.Biomes.BiomeType.Forest, map.Progress.CurrentRegion, "A new game starts in the Vila's region.");

            SceneTransitionService transition = GetService<SceneTransitionService>();
            Assert.IsTrue(transition.BeginCityVisit(CityCatalog.VilaZefa), "BeginCityVisit refused the Vila.");
            yield return WaitUntilOrFail(() => SceneManager.GetActiveScene().name == MainSceneName && CityModeController.Current != null && CityModeController.Current.IsReady,
                "Main.unity to load in city mode with the Vila built", SceneLoadTimeoutSeconds);

            CityModeController mode = CityModeController.Current;
            CityLayout layout = mode.Layout;
            GameObject player = FindPlayer();
            Assert.IsNotNull(player);
            Assert.AreEqual(ZoneKind.City, CitySafeZones.ZoneAt(player.transform.position), "The hero arrived outside the safe zone.");
            foreach (string root in CityModeController.HiddenSceneRoots)
            {
                GameObject arena = SceneManager.GetActiveScene().GetRootGameObjects().FirstOrDefault(g => g.name == root);
                Assert.IsTrue(arena == null || !arena.activeSelf, $"The arena root '{root}' is still visible in city mode.");
            }

            PlayerClickToMove click = player.GetComponent<PlayerClickToMove>();
            Assert.IsNotNull(click, "The player has no click-to-move.");

            var shopLots = layout.Lots.Where(l => !string.IsNullOrEmpty(l.ShopId)).ToList();
            Assert.GreaterOrEqual(shopLots.Count, 8, "The Vila should have a front for each of its shops.");
            foreach (CityLot lot in shopLots)
            {
                CityModeController.PlaceAt(player, layout.PlazaCenter, Vector2.down);
                yield return WaitFrames(2);
                List<Vector2> route = layout.RouteToDoor(lot);
                for (int i = 1; i < route.Count; i++)
                {
                    yield return WalkTo(click, player, route[i], $"{lot.Id} (waypoint {i})");
                }

                Vector3 at = player.transform.position;
                float miss = Vector2.Distance(new Vector2(at.x, at.z), lot.DoorApproach);
                Assert.Less(miss, ArrivalTolerance, $"The hero stopped {miss:0.00} m short of the door of '{lot.Id}' (blocked by a collider?).");
                yield return WaitFrames(3);
                CityResidentPost post = mode.Town.PostOf(lot.Id);
                Assert.IsNotNull(post, $"No keeper at '{lot.Id}'.");
                Assert.IsTrue(post.PlayerInRange, $"At the door of '{lot.Id}' but out of {post.ResidentId ?? post.ShopId}'s interact range.");
                Assert.AreEqual(ZoneKind.City, CitySafeZones.ZoneAt(player.transform.position));
            }

            // Out by the south gate, back to the map.
            CityExit gate = layout.Exits.First(e => e.Id == "exit_south");
            CityModeController.PlaceAt(player, layout.PlazaCenter, Vector2.down);
            yield return WaitFrames(2);
            CityExitTrigger.Enabled = true;
            click.SetDestination(new Vector3(gate.Target.x, 0f, gate.Target.y));
            yield return WaitFast(() => SceneManager.GetActiveScene().name == WorldMapSceneName && SceneManager.GetActiveScene().isLoaded,
                "the south gate to take the hero back to the world map", SceneLoadTimeoutSeconds, 2f);
            RestoreTimeScale();
            Assert.IsFalse(transition.HasPendingCityVisit, "The city visit was not closed when leaving.");
            yield return WaitForMap(m => map = m);
            Assert.AreEqual(Rapadura.Gameplay.World.Biomes.BiomeType.Forest, map.Progress.CurrentRegion, "Leaving the Vila must put the token back on its node.");
        }

        private static IEnumerator WalkTo(PlayerClickToMove click, GameObject player, Vector2 target, string what)
        {
            click.SetDestination(new Vector3(target.x, player.transform.position.y, target.y));
            yield return null;
            yield return WaitFast(() => !click.HasDestination, $"the hero to reach {what}", WalkTimeoutSeconds, 2f);
            RestoreTimeScale();
        }
    }
}
