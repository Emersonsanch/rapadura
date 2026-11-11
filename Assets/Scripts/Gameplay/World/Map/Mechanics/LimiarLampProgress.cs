using System;
using System.Collections.Generic;
using Rapadura.Core.DI;
using Rapadura.Core.Interfaces;
using Rapadura.Core.Systems;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>The lamp ids the hero has lit (older saves have no entry: nothing lit).</summary>
    [Serializable]
    public class LimiarLampSaveState
    {
        public List<string> lit = new List<string>();
    }

    /// <summary>
    /// The lamps the hero has lit, in the save file (<see cref="ISaveable"/>, not PlayerPrefs). The driver registers it while the region exists; the SaveManager carries it meanwhile.
    /// Also <see cref="IResettable"/>: it registers itself in the session's <see cref="NewSessionReset"/> (the one the SaveManager runs right before a file is applied),
    /// so a save without the "LimiarLamps" key (loaded from the pause menu while the region is up) no longer keeps the previous slot's lamps lit.
    /// Only the newest instance stays registered (one driver at a time), so a destroyed driver never leaves a trail of stale entries.
    /// </summary>
    public sealed class LimiarLampProgress : ISaveable, IResettable
    {
        private static LimiarLampProgress s_registered;
        private static NewSessionReset s_registeredIn;

        private readonly HashSet<string> _lit = new HashSet<string>();

        /// <summary>Finds the session reset through the SaveManager in the ServiceLocator (none in a standalone scene or a test: nothing registered).</summary>
        public LimiarLampProgress() : this(FindSessionReset())
        {
        }

        /// <summary>Registers in <paramref name="sessionReset"/> (null: not registered), replacing the previous instance.</summary>
        public LimiarLampProgress(NewSessionReset sessionReset)
        {
            if (sessionReset == null)
            {
                return;
            }

            if (s_registered != null && s_registeredIn != null)
            {
                s_registeredIn.Unregister(s_registered);
            }

            sessionReset.Register(this);
            s_registered = this;
            s_registeredIn = sessionReset;
        }

        public string SaveKey => "LimiarLamps";

        public ICollection<string> Lit => _lit;

        public bool Light(string id) => !string.IsNullOrEmpty(id) && _lit.Add(id);

        public object CaptureState() => new LimiarLampSaveState { lit = new List<string>(_lit) };

        public void RestoreState(object state)
        {
            _lit.Clear();
            if (state is LimiarLampSaveState saved && saved.lit != null)
            {
                foreach (string id in saved.lit)
                {
                    Light(id);
                }
            }
        }

        /// <summary>New game / a file about to be applied: no lamp lit.</summary>
        public void ResetForNewSession() => _lit.Clear();

        private static NewSessionReset FindSessionReset()
        {
            return ServiceLocator.TryGet(out Rapadura.Save.SaveManager saves) ? saves.SessionReset : null;
        }
    }
}
