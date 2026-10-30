using System;
using System.Collections.Generic;
using System.Linq;
using Rapadura.Core.DI;
using Rapadura.Core.Events;
using Rapadura.Core.Interfaces;
using Rapadura.Gameplay.Shop;
using Rapadura.Gameplay.World.Biomes;
using Rapadura.Gameplay.World.Map;

namespace Rapadura.Gameplay.World.Cities
{
    /// <summary>JSON-serializable snapshot of <see cref="CityProgress"/> (parallel lists: JsonUtility has no dictionaries).</summary>
    [Serializable]
    public class CitySaveState
    {
        public List<int> visitedRegions = new List<int>();

        /// <summary>Last rotation period seen per themed shop (see <see cref="ShopRotation.EffectivePeriod"/>).</summary>
        public List<string> shopIds = new List<string>();
        public List<long> shopPeriods = new List<long>();

        /// <summary>Freguesia points per shop (<see cref="ShopReputation"/>). Old saves: none.</summary>
        public List<string> reputationShopIds = new List<string>();
        public List<int> reputationPoints = new List<int>();

        /// <summary>Bornais de couro bought (the bag expansion is sold once). Old saves: 0.</summary>
        public int bornaisBought;
    }

    /// <summary>
    /// Per-save state of the cities: which ones the cousins already walked to (the carro de boi stops there) and the
    /// last rotation period each themed shop showed. A GameManager-built manager registered with SaveManager under
    /// <see cref="Key"/> and with NewSessionReset; a save written before cities existed has no entry and loads with
    /// only the starting city visited — plus, derived, every region the cousins already won a fight in (they have been
    /// there), so old saves keep their routes.
    /// <para>
    /// It also applies "renasce na cidade da região" (<see cref="SafeZoneRules.RespawnRegion"/>): when an encounter
    /// ends in defeat, before the auto-save and the map load, the current region becomes the city of the region the
    /// defeat happened in, and <see cref="PendingRespawnCity"/> tells the map HUD to say so.
    /// </para>
    /// </summary>
    public class CityProgress : IManager, ISaveable, IResettable
    {
        public const string Key = "Cities";

        private readonly HashSet<BiomeType> _visited = new HashSet<BiomeType>();
        private readonly Dictionary<string, long> _shopPeriods = new Dictionary<string, long>(StringComparer.Ordinal);
        private readonly Dictionary<string, int> _reputation = new Dictionary<string, int>(StringComparer.Ordinal);
        private int _bornaisBought;
        private WorldMapProgress _map;
        private bool _subscribed;

        private static CityProgress _standalone;

        public CityProgress(WorldMapProgress map = null)
        {
            _map = map;
        }

        public string SaveKey => Key;

        /// <summary>City the hero just woke up in after a defeat (null when nothing to tell). Not saved.</summary>
        public CitySpec PendingRespawnCity { get; private set; }

        /// <summary>The GameManager's state, or a process-wide unsaved one when a scene is played on its own.</summary>
        public static CityProgress ResolveShared()
        {
            if (ServiceLocator.TryGet(out CityProgress state))
            {
                return state;
            }

            if (_standalone == null)
            {
                ServiceLocator.TryGet(out WorldMapProgress map);
                _standalone = new CityProgress(map);
            }

            return _standalone;
        }

        public void Initialize()
        {
            if (!_subscribed)
            {
                _subscribed = true;
                EventBus.Subscribe<EncounterEndedEvent>(OnEncounterEnded);
                EventBus.Subscribe<ItemPurchasedEvent>(OnItemPurchased);
            }
        }

        public void Shutdown()
        {
            if (_subscribed)
            {
                _subscribed = false;
                EventBus.Unsubscribe<EncounterEndedEvent>(OnEncounterEnded);
                EventBus.Unsubscribe<ItemPurchasedEvent>(OnItemPurchased);
            }
        }

        public void ResetForNewSession()
        {
            _visited.Clear();
            _shopPeriods.Clear();
            _reputation.Clear();
            _bornaisBought = 0;
            PendingRespawnCity = null;
        }

        /// <summary>Uses <paramref name="map"/> for the derived visits and the respawn (tests, stand-alone scenes).</summary>
        public void BindMap(WorldMapProgress map)
        {
            _map = map;
        }

        // ---------------------------------------------------------------
        // Visits
        // ---------------------------------------------------------------

        public bool IsVisited(BiomeType region)
        {
            if (!CityCatalog.HasCity(region))
            {
                return false;
            }

            if (_visited.Contains(region))
            {
                return true;
            }

            if (_map == null)
            {
                return region == BiomeType.Forest;
            }

            return region == _map.StartingRegion || _map.GetVictories(region) > 0;
        }

        /// <summary>Records a visit; true the first time (a new stop for the carro de boi).</summary>
        public bool MarkVisited(BiomeType region)
        {
            if (!CityCatalog.HasCity(region))
            {
                return false;
            }

            bool wasVisited = IsVisited(region);
            _visited.Add(region);
            return !wasVisited;
        }

        public IEnumerable<BiomeType> VisitedRegions()
        {
            foreach (CitySpec city in CityCatalog.Cities)
            {
                if (IsVisited(city.Region))
                {
                    yield return city.Region;
                }
            }
        }

        // ---------------------------------------------------------------
        // Shop rotation
        // ---------------------------------------------------------------

        public long? LastShopPeriod(string shopId)
        {
            return shopId != null && _shopPeriods.TryGetValue(shopId, out long period) ? period : (long?)null;
        }

        /// <summary>
        /// The period <paramref name="shopId"/> shows now (<see cref="ShopRotation.EffectivePeriod"/>) and records it.
        /// <paramref name="advanced"/> is true when a new period starts for a shop that already had one (the counter
        /// changed: its live stock should be refilled).
        /// </summary>
        public long ResolveShopPeriod(string shopId, long currentPeriod, out bool advanced)
        {
            long? last = LastShopPeriod(shopId);
            long effective = ShopRotation.EffectivePeriod(currentPeriod, last);
            advanced = last.HasValue && effective > last.Value;
            if (shopId != null)
            {
                _shopPeriods[shopId] = effective;
            }

            return effective;
        }

        // ---------------------------------------------------------------
        // Freguesia (shop reputation) and the Bornal de couro
        // ---------------------------------------------------------------

        public int ReputationPoints(string shopId)
        {
            return shopId != null && _reputation.TryGetValue(shopId, out int points) ? points : 0;
        }

        public int ReputationTier(string shopId) => ShopReputation.TierFor(ReputationPoints(shopId));

        /// <summary>Adds freguesia points to <paramref name="shopId"/>; true when that reached a new tier.</summary>
        public bool AddReputation(string shopId, int points)
        {
            if (string.IsNullOrEmpty(shopId) || points <= 0)
            {
                return false;
            }

            int before = ReputationPoints(shopId);
            int after = ShopReputation.Add(before, points);
            _reputation[shopId] = after;
            return ShopReputation.TierFor(after) > ShopReputation.TierFor(before);
        }

        /// <summary>How many Bornais de couro were bought (0 or 1: the bag grows once).</summary>
        public int BornaisBought => _bornaisBought;

        /// <summary>The Bornal de couro is on a counter only until one was bought.</summary>
        public bool CanBuyBornal => _bornaisBought < 1;

        public void RecordBornal() => _bornaisBought++;

        private void OnItemPurchased(ItemPurchasedEvent evt)
        {
            CityShopPurchases.Apply(evt, this);
        }

        // ---------------------------------------------------------------
        // Respawn
        // ---------------------------------------------------------------

        /// <summary>Returns and forgets <see cref="PendingRespawnCity"/>.</summary>
        public CitySpec ConsumeRespawnNotice()
        {
            CitySpec city = PendingRespawnCity;
            PendingRespawnCity = null;
            return city;
        }

        /// <summary>Defeat in <paramref name="defeatRegion"/>: wake up in that region's city (unlocked, so the token can stand there).</summary>
        public BiomeType ApplyDefeat(BiomeType defeatRegion)
        {
            BiomeType fallback = _map != null ? _map.StartingRegion : BiomeType.Forest;
            BiomeType respawn = SafeZoneRules.RespawnRegion(defeatRegion, CityCatalog.HasCity, fallback);
            if (_map != null && !_map.SetCurrentRegion(respawn))
            {
                respawn = _map.CurrentRegion;
            }

            MarkVisited(respawn);
            PendingRespawnCity = CityCatalog.ForRegion(respawn);
            return respawn;
        }

        private void OnEncounterEnded(EncounterEndedEvent evt)
        {
            if (evt.Outcome == EncounterOutcome.Defeat && evt.Payload != null)
            {
                ApplyDefeat(evt.Payload.biome);
            }
        }

        // ---------------------------------------------------------------
        // ISaveable
        // ---------------------------------------------------------------

        public object CaptureState()
        {
            string[] ids = _shopPeriods.Keys.OrderBy(k => k, StringComparer.Ordinal).ToArray();
            string[] repIds = _reputation.Keys.OrderBy(k => k, StringComparer.Ordinal).ToArray();
            return new CitySaveState
            {
                visitedRegions = _visited.Select(r => (int)r).OrderBy(r => r).ToList(),
                shopIds = ids.ToList(),
                shopPeriods = ids.Select(id => _shopPeriods[id]).ToList(),
                reputationShopIds = repIds.ToList(),
                reputationPoints = repIds.Select(id => _reputation[id]).ToList(),
                bornaisBought = _bornaisBought,
            };
        }

        public void RestoreState(object state)
        {
            if (state is not CitySaveState saved)
            {
                return;
            }

            _visited.Clear();
            _shopPeriods.Clear();
            _reputation.Clear();
            _bornaisBought = Math.Max(0, saved.bornaisBought);

            if (saved.reputationShopIds != null && saved.reputationPoints != null)
            {
                for (int i = 0; i < Math.Min(saved.reputationShopIds.Count, saved.reputationPoints.Count); i++)
                {
                    if (!string.IsNullOrEmpty(saved.reputationShopIds[i]) && saved.reputationPoints[i] > 0)
                    {
                        _reputation[saved.reputationShopIds[i]] = saved.reputationPoints[i];
                    }
                }
            }

            if (saved.visitedRegions != null)
            {
                foreach (int region in saved.visitedRegions)
                {
                    if (Enum.IsDefined(typeof(BiomeType), region))
                    {
                        _visited.Add((BiomeType)region);
                    }
                }
            }

            if (saved.shopIds != null && saved.shopPeriods != null)
            {
                for (int i = 0; i < Math.Min(saved.shopIds.Count, saved.shopPeriods.Count); i++)
                {
                    if (!string.IsNullOrEmpty(saved.shopIds[i]))
                    {
                        _shopPeriods[saved.shopIds[i]] = saved.shopPeriods[i];
                    }
                }
            }
        }
    }
}
