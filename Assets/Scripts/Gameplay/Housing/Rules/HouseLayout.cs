using System;
using System.Collections.Generic;
using System.Linq;

namespace Rapadura.Gameplay.Housing
{
    /// <summary>
    /// The house's furniture and its Depósito (Housing.md §2.4), as pure data plus the operations the player
    /// (offline) or the server (online, H8) applies: place from the depot, move, store back. Every change goes
    /// through <see cref="HousePlacementRules.Validate"/>. Unity-free.
    /// </summary>
    public sealed class HouseLayout
    {
        private readonly Func<string, HouseItemDefinition> _resolve;
        private readonly List<HousePlacedData> _placed = new List<HousePlacedData>();
        private readonly Dictionary<string, int> _depot = new Dictionary<string, int>(StringComparer.Ordinal);
        private readonly List<string> _depotOrder = new List<string>();
        private int _nextInstanceId = 1;

        public HouseLayout(HouseLevelSpec spec, Func<string, HouseItemDefinition> resolve, int depotEntryLimit = HouseLevels.OfflineDepotEntryLimit)
        {
            Spec = spec ?? HouseLevels.Get(1);
            _resolve = resolve ?? (_ => null);
            DepotEntryLimit = Math.Max(1, depotEntryLimit);
        }

        public HouseLevelSpec Spec { get; private set; }
        public int DepotEntryLimit { get; }
        public IReadOnlyList<HousePlacedData> Placed => _placed;
        public int NextInstanceId => _nextInstanceId;

        /// <summary>Raised after any change (place, move, store, depot add, restore).</summary>
        public event Action Changed;

        public HouseItemDefinition Resolve(string itemId) => string.IsNullOrEmpty(itemId) ? null : _resolve(itemId);

        public HousePlacedData Find(int instanceId)
        {
            if (instanceId <= 0) return null;
            foreach (HousePlacedData placed in _placed)
            {
                if (placed.instanceId == instanceId) return placed;
            }

            return null;
        }

        public int InteriorCount => _placed.Count(p => HousePlacementRules.IsInterior(p.layer));
        public int YardCount => _placed.Count(p => p.layer == HouseLayer.Yard);

        public int CountExtraChests() => _placed.Count(p => HousePlacementRules.IsExtraChest(Resolve(p.itemId)));

        /// <summary>Extra chest slots from placed extra chests (Housing.md §5).</summary>
        public int ExtraChestSlots() => _placed.Select(p => Resolve(p.itemId)).Where(HousePlacementRules.IsExtraChest).Sum(i => i.FunctionParam);

        public bool HasPlaced(HouseItemFunction function) => _placed.Any(p => Resolve(p.itemId)?.Function == function);

        public CellRect FootprintOf(HousePlacedData placed)
        {
            return HousePlacementRules.Footprint(Resolve(placed.itemId), placed.layer, placed.x, placed.y, placed.rotation);
        }

        // ------------------------------------------------------------------
        // Depot
        // ------------------------------------------------------------------

        public int DepotCount(string itemId) => itemId != null && _depot.TryGetValue(itemId, out int qty) ? qty : 0;

        public int DepotEntryCount => _depotOrder.Count;

        /// <summary>Depot entries in arrival order (oldest first).</summary>
        public IEnumerable<HouseDepotEntry> Depot => _depotOrder.Select(id => new HouseDepotEntry(id, _depot[id]));

        /// <summary>Adds copies to the depot (a new id needs a free entry; copies of an id already there always stack).</summary>
        public bool AddToDepot(string itemId, int quantity = 1)
        {
            if (string.IsNullOrEmpty(itemId) || quantity <= 0) return false;
            if (!_depot.ContainsKey(itemId))
            {
                if (_depotOrder.Count >= DepotEntryLimit) return false;
                _depot[itemId] = 0;
                _depotOrder.Add(itemId);
            }

            _depot[itemId] += quantity;
            Changed?.Invoke();
            return true;
        }

        /// <summary>
        /// A reward that must never be lost (quest, upgrade): like <see cref="AddToDepot"/> but past the entry limit if it
        /// has to — the same rule <see cref="Restore"/> uses for pieces that no longer fit.
        /// </summary>
        public void GrantToDepot(string itemId, int quantity = 1)
        {
            if (string.IsNullOrEmpty(itemId) || quantity <= 0) return;
            for (int i = 0; i < quantity; i++) ForceToDepot(itemId);
            Changed?.Invoke();
        }

        private bool TakeFromDepot(string itemId)
        {
            if (DepotCount(itemId) <= 0) return false;
            _depot[itemId]--;
            if (_depot[itemId] <= 0)
            {
                _depot.Remove(itemId);
                _depotOrder.Remove(itemId);
            }

            return true;
        }

        private void ForceToDepot(string itemId)
        {
            if (!_depot.ContainsKey(itemId))
            {
                _depot[itemId] = 0;
                _depotOrder.Add(itemId);
            }

            _depot[itemId]++;
        }

        // ------------------------------------------------------------------
        // Place / move / store
        // ------------------------------------------------------------------

        public HousePlacementResult CanPlace(HousePlacementRequest request)
        {
            if (DepotCount(request.ItemId) <= 0) return Resolve(request.ItemId) == null ? HousePlacementResult.UnknownItem : HousePlacementResult.NotInDepot;
            return HousePlacementRules.Validate(this, request);
        }

        /// <summary>Takes one copy out of the depot and places it. <paramref name="instanceId"/> = the new instance.</summary>
        public HousePlacementResult Place(HousePlacementRequest request, out int instanceId)
        {
            instanceId = 0;
            HousePlacementResult result = CanPlace(request);
            if (result != HousePlacementResult.Ok) return result;

            TakeFromDepot(request.ItemId);
            instanceId = _nextInstanceId++;
            _placed.Add(ToData(instanceId, request));
            Changed?.Invoke();
            return HousePlacementResult.Ok;
        }

        public HousePlacementResult CanMove(int instanceId, HousePlacementRequest request)
        {
            HousePlacedData placed = Find(instanceId);
            if (placed == null) return HousePlacementResult.UnknownInstance;
            request.ItemId = placed.itemId;
            return HousePlacementRules.Validate(this, request, instanceId);
        }

        /// <summary>Moves an instance (surface pieces on it move with it).</summary>
        public HousePlacementResult Move(int instanceId, HousePlacementRequest request)
        {
            HousePlacementResult result = CanMove(instanceId, request);
            if (result != HousePlacementResult.Ok) return result;

            HousePlacedData placed = Find(instanceId);
            request.ItemId = placed.itemId;
            int index = _placed.IndexOf(placed);
            _placed[index] = ToData(instanceId, request);
            Changed?.Invoke();
            return HousePlacementResult.Ok;
        }

        /// <summary>Guarda an instance back in the depot, with whatever stands on it. Returns how many items went back.</summary>
        public int StoreInDepot(int instanceId)
        {
            HousePlacedData placed = Find(instanceId);
            if (placed == null) return 0;

            int stored = 0;
            foreach (HousePlacedData child in _placed.Where(p => p.layer == HouseLayer.Surface && p.parentInstanceId == instanceId).ToList())
            {
                _placed.Remove(child);
                ForceToDepot(child.itemId);
                stored++;
            }

            _placed.Remove(placed);
            ForceToDepot(placed.itemId);
            Changed?.Invoke();
            return stored + 1;
        }

        /// <summary>Items standing on <paramref name="parentInstanceId"/>.</summary>
        public IEnumerable<HousePlacedData> ChildrenOf(int parentInstanceId) =>
            _placed.Where(p => p.layer == HouseLayer.Surface && p.parentInstanceId == parentInstanceId);

        /// <summary>First free spot on a surface parent, or -1.</summary>
        public int FreeSurfaceSlot(int parentInstanceId, int ignoreInstanceId = 0)
        {
            HousePlacedData parent = Find(parentInstanceId);
            HouseItemDefinition item = parent != null ? Resolve(parent.itemId) : null;
            if (item == null || item.SurfaceSlots <= 0) return -1;
            for (int slot = 0; slot < item.SurfaceSlots; slot++)
            {
                if (!_placed.Any(p => p.instanceId != ignoreInstanceId && p.layer == HouseLayer.Surface && p.parentInstanceId == parentInstanceId && p.slot == slot))
                {
                    return slot;
                }
            }

            return -1;
        }

        private static HousePlacedData ToData(int instanceId, HousePlacementRequest request)
        {
            return new HousePlacedData
            {
                instanceId = instanceId,
                itemId = request.ItemId,
                layer = request.Layer,
                wall = request.Layer == HouseLayer.Wall ? request.Wall : HouseWallSide.South,
                x = request.Layer == HouseLayer.Surface ? 0 : request.X,
                y = request.Layer == HouseLayer.Surface ? 0 : request.Y,
                rotation = request.Rotation,
                mirrored = request.Layer == HouseLayer.Wall && request.Mirrored,
                parentInstanceId = request.Layer == HouseLayer.Surface ? request.ParentInstanceId : 0,
                slot = request.Layer == HouseLayer.Surface ? request.Slot : 0,
            };
        }

        // ------------------------------------------------------------------
        // Save
        // ------------------------------------------------------------------

        public void Clear()
        {
            _placed.Clear();
            _depot.Clear();
            _depotOrder.Clear();
            _nextInstanceId = 1;
            Changed?.Invoke();
        }

        public HousePlacedData[] CapturePlaced() => _placed.Select(p => p.Clone()).ToArray();

        public HouseDepotEntry[] CaptureDepot() => Depot.ToArray();

        /// <summary>
        /// Loads a saved layout. Nothing is ever lost: a saved piece that no longer fits the rules (a catalog or
        /// rule change, a smaller level) goes to the depot instead; unknown ids are kept in the depot as they are.
        /// Returns how many placed pieces had to go to the depot.
        /// </summary>
        public int Restore(HouseLevelSpec spec, HousePlacedData[] placed, HouseDepotEntry[] depot, int nextInstanceId)
        {
            Spec = spec ?? Spec;
            _placed.Clear();
            _depot.Clear();
            _depotOrder.Clear();

            if (depot != null)
            {
                foreach (HouseDepotEntry entry in depot)
                {
                    if (entry == null || string.IsNullOrEmpty(entry.itemId) || entry.quantity <= 0) continue;
                    for (int i = 0; i < entry.quantity; i++) ForceToDepot(entry.itemId);
                }
            }

            int displaced = 0;
            int maxId = 0;
            if (placed != null)
            {
                // Parents before what stands on them.
                foreach (HousePlacedData saved in placed.Where(p => p != null).OrderBy(p => p.layer == HouseLayer.Surface ? 1 : 0))
                {
                    if (string.IsNullOrEmpty(saved.itemId)) continue;
                    bool duplicateId = saved.instanceId <= 0 || Find(saved.instanceId) != null;
                    if (!duplicateId && HousePlacementRules.Validate(this, HousePlacementRequest.From(saved)) == HousePlacementResult.Ok)
                    {
                        _placed.Add(saved.Clone());
                        maxId = Math.Max(maxId, saved.instanceId);
                    }
                    else
                    {
                        ForceToDepot(saved.itemId);
                        displaced++;
                    }
                }
            }

            _nextInstanceId = Math.Max(Math.Max(1, nextInstanceId), maxId + 1);
            Changed?.Invoke();
            return displaced;
        }
    }
}
