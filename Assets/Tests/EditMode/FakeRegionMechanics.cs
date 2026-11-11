using Rapadura.Gameplay.World.Map.Mechanics;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>A stand-in region driver for the <see cref="MechanicsDriverHub"/> tests (no Update of its own).</summary>
    public sealed class FakeRegionMechanics : MonoBehaviour, IRegionMechanics
    {
        public Rect[] Areas;
        public bool Busy;

        public Rect[] MechanicsAreas => Areas;

        public bool MechanicsBusy => Busy;
    }
}
