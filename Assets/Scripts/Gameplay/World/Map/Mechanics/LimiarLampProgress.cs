using System;
using System.Collections.Generic;
using Rapadura.Core.Interfaces;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>The lamp ids the hero has lit (older saves have no entry: nothing lit).</summary>
    [Serializable]
    public class LimiarLampSaveState
    {
        public List<string> lit = new List<string>();
    }

    /// <summary>The lamps the hero has lit, in the save file (<see cref="ISaveable"/>, not PlayerPrefs). The driver registers it while the region exists; the SaveManager carries it meanwhile.</summary>
    public sealed class LimiarLampProgress : ISaveable
    {
        private readonly HashSet<string> _lit = new HashSet<string>();

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
    }
}
