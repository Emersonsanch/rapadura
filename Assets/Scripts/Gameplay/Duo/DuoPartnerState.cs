using System;
using System.Collections.Generic;
using Rapadura.Core.DI;
using Rapadura.Core.Events;
using Rapadura.Core.Interfaces;
using Rapadura.Gameplay.Characters;

namespace Rapadura.Gameplay.Duo
{
    /// <summary>One "hero → partner" choice, ids stored by name (like <see cref="CharacterSelectionSaveState"/>).</summary>
    [Serializable]
    public class DuoPartnerSaveEntry
    {
        public string hero;
        public string partner;
    }

    /// <summary>JSON snapshot of <see cref="DuoPartnerState"/> (absent in older saves = every hero uses the default partner).</summary>
    [Serializable]
    public class DuoPartnerSaveState
    {
        public List<DuoPartnerSaveEntry> partners = new List<DuoPartnerSaveEntry>();
    }

    /// <summary>Raised when the player picks another partner (pause menu / village).</summary>
    public readonly struct DuoPartnerChangedEvent : IGameEvent
    {
        public readonly CharacterId Hero;
        public readonly CharacterId Partner;

        public DuoPartnerChangedEvent(CharacterId hero, CharacterId partner)
        {
            Hero = hero;
            Partner = partner;
        }
    }

    /// <summary>
    /// "Golpe em dupla": which cousin each hero calls (one choice per hero, so switching heroes keeps each pick).
    /// A GameManager-built manager registered with <c>SaveManager</c> under <see cref="Key"/> and with
    /// <c>NewSessionReset</c>; "Novo jogo" and saves written before this existed fall back to
    /// <see cref="DuoStrikeRules.DefaultPartnerFor"/> (the sibling; Lavine calls Maria).
    /// <see cref="IsUnlocked"/> is the hook for story-gated cousins — today all five cousins are unlocked.
    /// </summary>
    public class DuoPartnerState : IManager, ISaveable, IResettable
    {
        public const string Key = "DuoPartner";

        private readonly Dictionary<CharacterId, CharacterId> _partners = new Dictionary<CharacterId, CharacterId>();

        public string SaveKey => Key;

        /// <summary>Which cousins can be called (null = all). Story gating can plug in here without touching the rules.</summary>
        public Func<CharacterId, bool> UnlockedFilter { get; set; }

        private static DuoPartnerState _standalone;

        /// <summary>The GameManager's instance, or a process-wide fallback (tests, a scene played without the bootstrap).</summary>
        public static DuoPartnerState ResolveShared()
        {
            if (ServiceLocator.TryGet(out DuoPartnerState state) && state != null)
            {
                return state;
            }

            return _standalone ??= new DuoPartnerState();
        }

        public void Initialize()
        {
        }

        public void Shutdown()
        {
            _partners.Clear();
        }

        public void ResetForNewSession() => _partners.Clear();

        public bool IsUnlocked(CharacterId id) => UnlockedFilter == null || UnlockedFilter(id);

        public List<CharacterId> AvailablePartnersFor(CharacterId hero) => DuoStrikeRules.AvailablePartners(hero, IsUnlocked);

        /// <summary>The cousin <paramref name="hero"/> calls: their saved pick when still valid, else the default, else the first available.</summary>
        public CharacterId? GetPartnerFor(CharacterId hero)
        {
            if (_partners.TryGetValue(hero, out CharacterId chosen) && chosen != hero && IsUnlocked(chosen))
            {
                return chosen;
            }

            CharacterId fallback = DuoStrikeRules.DefaultPartnerFor(hero);
            if (fallback != hero && IsUnlocked(fallback))
            {
                return fallback;
            }

            List<CharacterId> available = AvailablePartnersFor(hero);
            return available.Count > 0 ? available[0] : (CharacterId?)null;
        }

        /// <summary>Records <paramref name="partner"/> for <paramref name="hero"/>; false (nothing changes) for the hero themselves or a locked cousin.</summary>
        public bool SetPartner(CharacterId hero, CharacterId partner)
        {
            if (partner == hero || !IsUnlocked(partner))
            {
                return false;
            }

            _partners[hero] = partner;
            EventBus.Publish(new DuoPartnerChangedEvent(hero, partner));
            return true;
        }

        /// <summary>Moves to the next available cousin (birth order, wrapping) and returns it.</summary>
        public CharacterId? CyclePartner(CharacterId hero)
        {
            CharacterId? next = DuoStrikeRules.NextPartner(hero, GetPartnerFor(hero), IsUnlocked);
            if (next.HasValue)
            {
                SetPartner(hero, next.Value);
            }

            return next;
        }

        public object CaptureState()
        {
            var state = new DuoPartnerSaveState();
            foreach (CharacterId hero in DuoStrikeRules.ByBirthOrder)
            {
                if (_partners.TryGetValue(hero, out CharacterId partner))
                {
                    state.partners.Add(new DuoPartnerSaveEntry { hero = hero.ToString(), partner = partner.ToString() });
                }
            }

            return state;
        }

        public void RestoreState(object state)
        {
            _partners.Clear();
            if (state is not DuoPartnerSaveState saved || saved.partners == null)
            {
                return;
            }

            foreach (DuoPartnerSaveEntry entry in saved.partners)
            {
                if (entry != null
                    && CharacterSelectionState.TryParse(entry.hero, out CharacterId hero)
                    && CharacterSelectionState.TryParse(entry.partner, out CharacterId partner)
                    && hero != partner)
                {
                    _partners[hero] = partner;
                }
            }
        }
    }
}
