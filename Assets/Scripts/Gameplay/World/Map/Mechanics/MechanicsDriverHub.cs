using System.Collections.Generic;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// A region mechanics driver the <see cref="MechanicsDriverHub"/> may switch off while the hero is far from its region.
    /// </summary>
    public interface IRegionMechanics
    {
        /// <summary>
        /// The world rectangles the driver works in (its <c>*Bounds</c> moved by the region's origin); null until its Start found
        /// the region, and for ever when the layout does not hold it (the driver switched itself off: the hub leaves it alone).
        /// </summary>
        Rect[] MechanicsAreas { get; }

        /// <summary>Something under way that may cross the border (a ride, a race): the driver stays on wherever the hero is.</summary>
        bool MechanicsBusy { get; }
    }

    /// <summary>
    /// Keeps only the mechanics drivers near the hero running: ten times a second each driver of the overworld root is switched
    /// on when the hero is within <see cref="Margin"/> of one of its <see cref="IRegionMechanics.MechanicsAreas"/> (or it is
    /// <see cref="IRegionMechanics.MechanicsBusy"/>) and off otherwise, so one driver (two at a border) pays an Update instead
    /// of fourteen. The margin is walked well before the 0.1 s tick runs out, so a driver is always on when the hero steps into
    /// its region; each driver's OnDisable puts out its feedback once, as its own "outside the region" branch did every frame.
    /// </summary>
    public sealed class MechanicsDriverHub : MonoBehaviour
    {
        /// <summary>Seconds between checks (the overworld's zone tick).</summary>
        public const float TickSeconds = 0.1f;

        /// <summary>Metres round a driver's areas where it is already on (covers the 0.1 s between checks and bounds that do not match the region exactly).</summary>
        public const float Margin = 32f;

        private const string PlayerTag = "Player";

        private readonly List<Behaviour> _drivers = new List<Behaviour>();
        private readonly List<IRegionMechanics> _regions = new List<IRegionMechanics>();
        private bool _collected;
        private Transform _player;
        private float _nextTick;

        /// <summary>How many drivers are managed (tests).</summary>
        public int DriverCount
        {
            get
            {
                Collect();
                return _drivers.Count;
            }
        }

        /// <summary>The world rectangle of a driver's region-local <paramref name="localBounds"/> for a region at <paramref name="origin"/>.</summary>
        public static Rect WorldArea(Rect localBounds, Vector2 origin) => new Rect(localBounds.position + origin, localBounds.size);

        /// <summary>Whether a driver with these areas runs with the hero at <paramref name="hero"/> (pure).</summary>
        public static bool ShouldRun(Rect[] areas, Vector2 hero, float margin, bool busy)
        {
            if (busy)
            {
                return true;
            }

            if (areas == null)
            {
                return false;
            }

            foreach (Rect area in areas)
            {
                if (hero.x >= area.xMin - margin && hero.x <= area.xMax + margin && hero.y >= area.yMin - margin && hero.y <= area.yMax + margin)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// What <see cref="Refresh"/> does to one driver (pure): null leaves it as it is (not started yet, or the layout has no
        /// such region and the driver turned itself off for good), otherwise whether it runs with the hero at <paramref name="hero"/>.
        /// </summary>
        public static bool? Decide(Rect[] areas, Vector2 hero, bool busy)
        {
            if (areas == null)
            {
                return null;
            }

            return ShouldRun(areas, hero, Margin, busy);
        }

        private void Update()
        {
            if (Time.unscaledTime < _nextTick)
            {
                return;
            }

            _nextTick = Time.unscaledTime + TickSeconds;
            if (_player == null)
            {
                GameObject go = GameObject.FindGameObjectWithTag(PlayerTag);
                if (go == null)
                {
                    return;
                }

                _player = go.transform;
            }

            Vector3 p = _player.position;
            Refresh(new Vector2(p.x, p.z));
        }

        /// <summary>Switches every ready driver on or off for the hero at <paramref name="hero"/> (world x/z).</summary>
        public void Refresh(Vector2 hero)
        {
            Collect();
            for (int i = 0; i < _drivers.Count; i++)
            {
                Behaviour driver = _drivers[i];
                if (driver == null)
                {
                    continue;
                }

                bool? run = Decide(_regions[i].MechanicsAreas, hero, _regions[i].MechanicsBusy);
                if (run.HasValue && driver.enabled != run.Value)
                {
                    driver.enabled = run.Value;
                }
            }
        }

        private void Collect()
        {
            if (_collected)
            {
                return;
            }

            _collected = true;
            foreach (MonoBehaviour behaviour in GetComponents<MonoBehaviour>())
            {
                if (behaviour is IRegionMechanics region && !ReferenceEquals(behaviour, this))
                {
                    _drivers.Add(behaviour);
                    _regions.Add(region);
                }
            }
        }
    }
}
