using System;
using Rapadura.Core.Events;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Overworld
{
    /// <summary>
    /// The hero went from one area of a continuous region to another (Docs/Mapas100.md §0.10, "Área atual"): the zone, the
    /// region of the stitched world and the settlement (safe zone of a town or post) before and after. Pure data, raised by
    /// <see cref="OverworldAreaTracker"/> (published on the <see cref="EventBus"/> by <see cref="OverworldView"/>): the HUD
    /// letreiro, the area music and the area weather consume it; the online server can run the same tracker per player.
    /// </summary>
    public readonly struct AreaChangedEvent : IGameEvent
    {
        public readonly string LayoutId;
        public readonly OverworldZone Previous;
        public readonly OverworldZone Current;
        public readonly OverworldPart PreviousPart;
        public readonly OverworldPart CurrentPart;
        public readonly OverworldSettlement PreviousSettlement;
        public readonly OverworldSettlement CurrentSettlement;

        /// <summary>The first reading after an arrival, a barca or a respawn (no hysteresis, nothing walked).</summary>
        public readonly bool Teleported;

        public AreaChangedEvent(string layoutId, OverworldZone previous, OverworldZone current, OverworldPart previousPart, OverworldPart currentPart,
            OverworldSettlement previousSettlement, OverworldSettlement currentSettlement, bool teleported)
        {
            LayoutId = layoutId;
            Previous = previous;
            Current = current;
            PreviousPart = previousPart;
            CurrentPart = currentPart;
            PreviousSettlement = previousSettlement;
            CurrentSettlement = currentSettlement;
            Teleported = teleported;
        }

        public bool ZoneChanged => Previous != Current;
        public bool RegionChanged => PreviousPart != CurrentPart;
        public bool SettlementChanged => PreviousSettlement != CurrentSettlement;

        /// <summary>The hero stepped into a town / post's safe zone.</summary>
        public bool EnteredSettlement => CurrentSettlement != null && CurrentSettlement != PreviousSettlement;

        public override string ToString() => $"{Previous?.Id ?? "-"} -> {Current?.Id ?? "-"}";
    }

    /// <summary>
    /// The area the hero is in, with hysteresis (pure, EditMode-tested, server-runnable): the zone, the region of the stitched
    /// world and the settlement only change once the hero is <see cref="Hysteresis"/> metres past the border of the current
    /// one, so walking along an edge (or the soft barrier's step back) does not flicker the letreiro, the music or the
    /// weather. The soft barrier itself keeps using the exact <see cref="OverworldRules.ZoneAt"/>.
    /// </summary>
    public sealed class OverworldAreaTracker
    {
        /// <summary>Metres past a border before the area switches.</summary>
        public const float DefaultHysteresis = 6f;

        public OverworldAreaTracker(OverworldLayout layout, float hysteresis = DefaultHysteresis)
        {
            Layout = layout;
            Hysteresis = Mathf.Max(0f, hysteresis);
        }

        public OverworldLayout Layout { get; }
        public float Hysteresis { get; }

        public OverworldZone Zone { get; private set; }
        public OverworldPart Part { get; private set; }
        public OverworldSettlement Settlement { get; private set; }

        /// <summary>Raised by <see cref="Update"/> when the area changes.</summary>
        public event Action<AreaChangedEvent> AreaChanged;

        private bool _snap = true;

        /// <summary>The next <see cref="Update"/> reads the position as is (after a teleport): no hysteresis, flagged <see cref="AreaChangedEvent.Teleported"/>.</summary>
        public void Reset() => _snap = true;

        /// <summary>Reads the hero at <paramref name="p"/> (world coordinates); true (and <paramref name="change"/>) when the area changed.</summary>
        public bool Update(Vector2 p, out AreaChangedEvent change)
        {
            change = default;
            if (Layout == null)
            {
                return false;
            }

            float margin = _snap ? 0f : Hysteresis;
            bool teleported = _snap;
            OverworldZone zone = ZoneWithHysteresis(Layout, Zone, p, margin);
            OverworldPart part = PartWithHysteresis(Layout, Part, p, margin);
            OverworldSettlement settlement = SettlementWithHysteresis(Layout, Settlement, p, margin);
            _snap = false;
            if (zone == Zone && part == Part && settlement == Settlement)
            {
                return false;
            }

            change = new AreaChangedEvent(Layout.Id, Zone, zone, Part, part, Settlement, settlement, teleported);
            Zone = zone;
            Part = part;
            Settlement = settlement;
            AreaChanged?.Invoke(change);
            return true;
        }

        /// <summary>
        /// <paramref name="current"/> while <paramref name="p"/> is within <paramref name="margin"/> m of its rectangle, else the zone
        /// under <paramref name="p"/>, else (outside every zone) still <paramref name="current"/>.
        /// </summary>
        public static OverworldZone ZoneWithHysteresis(OverworldLayout layout, OverworldZone current, Vector2 p, float margin)
        {
            if (current != null && Grow(current.Bounds, margin).Contains(p))
            {
                return current;
            }

            return OverworldRules.ZoneAt(layout, p) ?? current;
        }

        /// <summary>The same for the regions of the stitched world (null for a region on its own).</summary>
        public static OverworldPart PartWithHysteresis(OverworldLayout layout, OverworldPart current, Vector2 p, float margin)
        {
            if (current != null && Grow(current.Bounds, margin).Contains(p))
            {
                return current;
            }

            return layout.PartAt(p);
        }

        /// <summary>The same for the safe zones of the settlements (null out of every town).</summary>
        public static OverworldSettlement SettlementWithHysteresis(OverworldLayout layout, OverworldSettlement current, Vector2 p, float margin)
        {
            if (current != null && Grow(current.SafeZone, margin).Contains(p))
            {
                return current;
            }

            return OverworldRules.SettlementAt(layout, p);
        }

        private static Rect Grow(Rect r, float margin) => margin <= 0f ? r : new Rect(r.xMin - margin, r.yMin - margin, r.width + 2f * margin, r.height + 2f * margin);
    }
}
