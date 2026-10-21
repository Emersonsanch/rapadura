using System.Collections.Generic;

namespace Rapadura.Core.Systems
{
    /// <summary>
    /// Pure bookkeeping behind <see cref="GameplayPause"/>: who is currently asking for gameplay to be
    /// frozen (one entry per open modal screen — character select, dialogue, shop, inventory, death
    /// screen...), plus whether a pause that is <i>not</i> tracked here (PauseMenuController, which
    /// drives Time.timeScale itself) is active. Kept free of Unity/EventBus so the rules are EditMode
    /// testable; <see cref="GameplayPause"/> applies the actual Time.timeScale/event side effects.
    /// </summary>
    public class PauseRequestTracker
    {
        private readonly HashSet<object> _owners = new HashSet<object>();

        /// <summary>A pause owned by someone outside this tracker (see <see cref="GameplayPause"/>) is active.</summary>
        public bool ExternalPaused { get; private set; }

        public int OwnerCount => _owners.Count;

        public bool HasOwners => _owners.Count > 0;

        /// <summary>Gameplay should be frozen: at least one owner, or an external pause.</summary>
        public bool IsPaused => HasOwners || ExternalPaused;

        public bool IsHeldBy(object owner) => owner != null && _owners.Contains(owner);

        /// <summary>
        /// Gameplay is frozen by someone other than <paramref name="owner"/> (another modal screen or
        /// an external pause) — e.g. overlays such as tutorial hints step aside while a modal is up.
        /// </summary>
        public bool IsPausedByOthers(object owner) => ExternalPaused || _owners.Count > (IsHeldBy(owner) ? 1 : 0);

        /// <summary>
        /// Adds <paramref name="owner"/>. True only when this call took gameplay from fully running to
        /// paused — the caller must then freeze time and announce the pause. Acquiring twice with the
        /// same owner is a no-op (each owner counts once).
        /// </summary>
        public bool Acquire(object owner)
        {
            if (owner == null)
            {
                return false;
            }

            bool wasPaused = IsPaused;
            return _owners.Add(owner) && !wasPaused;
        }

        /// <summary>
        /// Removes <paramref name="owner"/>. True only when this call left gameplay fully running
        /// (no owners and no external pause) — the caller must then restore time and announce the
        /// resume. Releasing an owner that never acquired is a no-op.
        /// </summary>
        public bool Release(object owner)
        {
            if (owner == null || !_owners.Remove(owner))
            {
                return false;
            }

            return !IsPaused;
        }

        public void SetExternalPaused(bool paused)
        {
            ExternalPaused = paused;
        }

        public void Clear()
        {
            _owners.Clear();
            ExternalPaused = false;
        }
    }
}
