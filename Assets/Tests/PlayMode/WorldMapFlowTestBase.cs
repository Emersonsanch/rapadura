using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rapadura.Core.Systems;
using Rapadura.Gameplay.World.Biomes;
using Rapadura.Gameplay.World.Map;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Rapadura.Tests.PlayMode
{
    /// <summary>
    /// World map ⇄ encounter helpers shared by the long PlayMode runs that drive the real game loop
    /// (<see cref="CampaignPlaythroughTests"/>, <see cref="SoakPlaythroughTests"/>): wait for an idle map,
    /// walk the token region by region, restart the app cold, and speed scaled time up while waiting on
    /// arena delays and token travel without ever fighting a pause or a hit-stop.
    /// </summary>
    public abstract class WorldMapFlowTestBase : SmokeTestBase
    {
        /// <summary>Default scaled-time multiplier for <see cref="WaitFast"/>.</summary>
        protected const float DefaultFastTimeScale = 4f;

        protected const float TravelTimeoutSeconds = 45f;

        private static int _restartCounter;

        /// <summary>Waits for the (possibly freshly reloaded) world map to be started, idle and unpaused, and turns off random step rolls.</summary>
        protected static IEnumerator WaitForMap(Action<WorldMapController> found, float timeoutSeconds = SceneLoadTimeoutSeconds)
        {
            WorldMapController map = null;
            yield return WaitUntilOrFail(() =>
            {
                if (SceneManager.GetActiveScene().name != WorldMapSceneName || !SceneManager.GetActiveScene().isLoaded)
                {
                    return false;
                }

                map = UnityEngine.Object.FindFirstObjectByType<WorldMapController>();
                return map != null && map.isActiveAndEnabled && map.Progress != null && map.RegionBosses != null && map.CurrentNode != null
                       && !GameplayPause.IsPaused;
            }, "the world map to be running, idle and unpaused", timeoutSeconds);

            Assert.AreSame(GetService<WorldMapProgress>(), map.Progress, "WorldMapController is not using the GameManager's WorldMapProgress.");

            // Walking between regions must not roll random fights: every encounter in these tests is explicit.
            map.Encounters.Enabled = false;
            found(map);
        }

        /// <summary>Walks the token to <paramref name="destination"/> hop by hop through unlocked regions (real travel, auto-save on arrival).</summary>
        protected static IEnumerator TravelTo(BiomeType destination, float fastTimeScale = DefaultFastTimeScale)
        {
            WorldMapController map = null;
            yield return WaitForMap(m => map = m);

            List<BiomeType> path = FindPath(map.Graph, map.Progress, map.Progress.CurrentRegion, destination);
            Assert.IsNotNull(path, $"No path of unlocked regions from {map.Progress.CurrentRegion} to {destination}.");

            foreach (BiomeType hop in path)
            {
                WorldMapRegionNode node = map.Nodes.FirstOrDefault(n => n != null && n.Region == hop);
                Assert.IsNotNull(node, $"WorldMap.unity has no node for {hop}.");
                Assert.IsTrue(map.TryTravelTo(node), $"The map refused to travel from {map.Progress.CurrentRegion} to {hop}.");

                WorldMapController traveling = map;
                yield return WaitFast(() => !traveling.IsTraveling && traveling.Progress.CurrentRegion == hop,
                    $"the token to arrive in {hop}", TravelTimeoutSeconds, fastTimeScale);
                RestoreTimeScale();
            }

            Assert.AreEqual(destination, map.Progress.CurrentRegion);
            Assert.AreEqual(destination, map.CurrentNode.Region);
        }

        /// <summary>Shortest hop list (excluding <paramref name="from"/>) through unlocked regions, or null when unreachable.</summary>
        protected static List<BiomeType> FindPath(WorldMapGraph graph, WorldMapProgress progress, BiomeType from, BiomeType to)
        {
            var previous = new Dictionary<BiomeType, BiomeType> { [from] = from };
            var queue = new Queue<BiomeType>();
            queue.Enqueue(from);

            while (queue.Count > 0)
            {
                BiomeType at = queue.Dequeue();
                if (at == to)
                {
                    var path = new List<BiomeType>();
                    for (BiomeType step = to; step != from; step = previous[step])
                    {
                        path.Add(step);
                    }

                    path.Reverse();
                    return path;
                }

                foreach (BiomeType neighbour in graph.GetNeighbours(at))
                {
                    if (progress.IsUnlocked(neighbour) && !previous.ContainsKey(neighbour))
                    {
                        previous[neighbour] = at;
                        queue.Enqueue(neighbour);
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// "Closes the app" (every scene and the GameManager destroyed, see <see cref="SmokeTestBase.UnloadSession"/>)
        /// and boots WorldMap.unity again on the same test save folder, so the next boot finds this test's saves.
        /// </summary>
        protected static IEnumerator ColdRestart()
        {
            yield return UnloadSession("FlowRestart_" + (++_restartCounter));
            yield return LoadScene(WorldMapSceneName);
        }

        /// <summary>
        /// <see cref="SmokeTestBase.WaitUntilOrFail"/> with scaled time sped up to <paramref name="fastTimeScale"/>
        /// (arena return delay, token travel). Only raises a running 1x+ clock: never unfreezes a pause and
        /// never fights a hit-stop.
        /// </summary>
        protected static IEnumerator WaitFast(Func<bool> condition, string what, float timeoutSeconds, float fastTimeScale = DefaultFastTimeScale)
        {
            float deadline = Time.realtimeSinceStartup + timeoutSeconds;
            while (!condition() && Time.realtimeSinceStartup < deadline)
            {
                RaiseTimeScale(fastTimeScale);
                yield return null;
            }

            Assert.IsTrue(condition(), $"Timed out after {timeoutSeconds:0.#}s waiting for: {what}");
        }

        /// <summary>Raises a running (1x or faster, unpaused) clock to <paramref name="fastTimeScale"/>; leaves pauses and hit-stops alone.</summary>
        protected static void RaiseTimeScale(float fastTimeScale)
        {
            if (!GameplayPause.IsPaused && Time.timeScale >= 1f && Time.timeScale < fastTimeScale)
            {
                Time.timeScale = fastTimeScale;
            }
        }

        protected static void RestoreTimeScale()
        {
            if (Time.timeScale > 1f)
            {
                Time.timeScale = 1f;
            }
        }
    }
}
