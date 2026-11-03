using System.Collections;
using NUnit.Framework;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.World.Map;
using Rapadura.Gameplay.World.Map.Areas;
using Rapadura.Gameplay.World.Map.Overworld;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Rapadura.Tests.PlayMode
{
    /// <summary>
    /// The continuous Região 1 on the real scenes (Docs/Mapas100.md §0.10): "Mover" to the Pomar loads Main.unity in map
    /// mode with the whole Zarafshan overworld, the hero at the Vila's dock; then the hero walks with real click-to-move
    /// from the Vila down the south road into the Campos do Aryk (another area) with no scene load at all, the chunks
    /// stream in around them, and a level-1 hero walking east into the Tugai (walk level 2) is pushed back by the soft
    /// barrier instead of being teleported. The four regions are one stitched world (§0.10, "Mundo costurado"): levelled up,
    /// the hero walks on down the south road through the open pass into the Areias Flamejantes, still with no scene load.
    /// </summary>
    [Category("Smoke")]
    public class OverworldWalkSmokeTests : WorldMapFlowTestBase
    {
        private const float WalkTimeoutSeconds = 40f;

        [UnityTest]
        [Timeout(300000)]
        public IEnumerator Pomar_LoadsTheOverworld_WalksFromTheVilaIntoTheFields_WithoutASceneChange()
        {
            MapExitTrigger.Enabled = false;
            try
            {
                yield return Walk();
            }
            finally
            {
                MapExitTrigger.Enabled = true;
                RestoreTimeScale();
            }
        }

        private IEnumerator Walk()
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
            Assert.IsNotNull(mode.Overworld, "The Pomar must load the Zarafshan overworld.");
            Assert.IsNotNull(mode.OverworldArea);
            OverworldLayout world = mode.Overworld;
            GameObject player = FindPlayer();
            Assert.IsNotNull(player);
            Assert.IsTrue(world.Vila.SafeZone.Contains(Ground(player)), $"The hero arrived at {Ground(player)}, outside the Vila.");
            Assert.Greater(mode.OverworldArea.LoadedChunkCount, 4, "The chunks around the Vila were not built.");

            int loads = 0;
            UnityEngine.Events.UnityAction<Scene, LoadSceneMode> counter = (s, m) => loads++;
            SceneManager.sceneLoaded += counter;
            int sceneHandle = SceneManager.GetActiveScene().handle;
            try
            {
                PlayerClickToMove click = player.GetComponent<PlayerClickToMove>();
                Assert.IsNotNull(click, "The player has no click-to-move.");

                // Down the Vila's south road into the Campos do Aryk.
                CityModeControllerPlace(player, new Vector2(0f, -30f), Vector2.down);
                yield return WaitFrames(3);
                yield return WalkTo(click, player, new Vector2(0f, -88f), "the end of the Vila's south road");
                yield return WalkTo(click, player, new Vector2(0f, -150f), "the Campos do Aryk");
                yield return WaitFrames(5);
                Assert.Less(Ground(player).y, -100f, $"The hero stopped at {Ground(player)} before the fields (blocked?).");
                Assert.AreEqual("campos_do_aryk", mode.OverworldArea.CurrentZone?.Id, "The zone is derived from the hero's position.");
                Assert.AreEqual(MapCatalog.PomarId, mode.MapId);
                Assert.IsTrue(mode.OverworldArea.IsChunkLoaded(OverworldRules.ChunkOf(world, Ground(player))), "The chunk under the hero is not loaded.");

                // East along the orchard road towards the Tugai: a level-1 hero is pushed back at the border (walk level 2).
                Assert.AreEqual(1, mode.HeroLevel, "A new hero starts at level 1.");
                CityModeControllerPlace(player, new Vector2(250f, 17f), Vector2.right);
                yield return WaitFrames(3);
                click.SetDestination(new Vector3(340f, player.transform.position.y, 32f));
                yield return null;
                yield return WaitFast(() => !click.HasDestination, "the barrier to stop the hero", WalkTimeoutSeconds, 2f);
                RestoreTimeScale();
                yield return WaitFrames(5);
                Assert.Less(Ground(player).x, 300.5f, $"A level-1 hero got into the Tugai ({Ground(player)}).");
                Assert.Greater(Ground(player).x, 280f, "The barrier teleported the hero away instead of a step back.");
                Assert.AreEqual("pomar", mode.OverworldArea.CurrentZone?.Id);

                // The stitched world: down the Estepe's south road, through the open pass (the old gate to 07) into the Areias.
                Assert.IsTrue(world.IsStitched, "The Pomar loads the stitched world of the four regions.");
                PlayerStats stats = player.GetComponent<PlayerStats>();
                Assert.IsNotNull(stats);
                for (int i = 0; i < 40 && stats.Level < 8; i++)
                {
                    stats.AddExperience(100000);
                }

                Assert.GreaterOrEqual(mode.HeroLevel, 5, "The hero must be able to walk the Estrada das Caravanas (walk level 5).");
                CityModeControllerPlace(player, new Vector2(0f, -930f), Vector2.down);
                yield return WaitFrames(3);
                yield return WalkTo(click, player, new Vector2(0f, -990f), "the pass at the end of the Estepe's south road");
                yield return WalkTo(click, player, new Vector2(-100f, -1010f), "the far side of the pass");
                yield return WalkTo(click, player, new Vector2(-100f, -1070f), "the Estrada das Caravanas");
                yield return WaitFrames(5);
                Assert.Less(Ground(player).y, -1040f, $"The hero stopped at {Ground(player)} before the Areias (blocked at the border?).");
                Assert.AreEqual(OverworldLayouts.AreiasId, mode.OverworldArea.CurrentPart?.Id, "The region follows the hero's position.");
                Assert.AreEqual(MapLayouts.CaravanasId, mode.MapId, "The map is the Estrada das Caravanas (07).");
                Assert.IsTrue(mode.OverworldArea.IsChunkLoaded(OverworldRules.ChunkOf(world, Ground(player))), "The chunk under the hero is not loaded.");

                Assert.AreEqual(0, loads, "No scene may load while walking inside the overworld.");
                int handleNow = SceneManager.GetActiveScene().handle;
                Assert.AreEqual(sceneHandle, handleNow, "The scene changed under the hero.");
            }
            finally
            {
                SceneManager.sceneLoaded -= counter;
            }
        }

        private static Vector2 Ground(GameObject player) => new Vector2(player.transform.position.x, player.transform.position.z);

        private static void CityModeControllerPlace(GameObject player, Vector2 at, Vector2 facing)
        {
            MapModeController.Current.OverworldArea.WarmUp(at);
            MapModeController.Current.OverworldArea.ResetTracking(at);
            Rapadura.Gameplay.World.Cities.Town.CityModeController.PlaceAt(player, at, facing);
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
