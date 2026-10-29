using UnityEngine;

namespace Rapadura.Gameplay.Player
{
    /// <summary>Optional authored override of a hero's <see cref="HeroMotionProfile"/> (assign on the <see cref="PlayerVisualEntry"/>; empty = the code defaults of <see cref="HeroMotionProfile.For"/>).</summary>
    [CreateAssetMenu(menuName = "Rapadura/Animation/Hero Motion Profile", fileName = "HeroMotionProfile")]
    public class HeroMotionProfileAsset : ScriptableObject
    {
        public HeroMotionProfile profile = new HeroMotionProfile();
    }
}
